using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ExpenseImportPersistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using Xunit;

namespace Stage8IntegrationApiTests;

[Collection("API")]
public sealed class BoundaryTests
{
    private const string Csv = "categoryId,amount,spentOn,note\n1,4.00,2026-09-22,cancel\n";

    [Fact]
    public async Task Before_upload_cancellation_returns_cancelled_dto_and_leaves_zero_rows()
    {
        using var cancellation = new CancellationTokenSource();
        await using var fixture = await CancellationFixture.CreateAsync(cancellation, _ => new ImportHooks(), _ => new ImportRequestHooks
        { BeforeUpload = _ => { cancellation.Cancel(); return ValueTask.CompletedTask; } });
        var response = await fixture.PostAsync(Csv);
        await fixture.AssertCancelledZeroRowsAsync(response);
    }

    [Fact]
    public async Task Mid_upload_cancellation_returns_cancelled_dto_and_leaves_zero_rows()
    {
        using var cancellation = new CancellationTokenSource();
        await using var fixture = await CancellationFixture.CreateAsync(cancellation, _ => new ImportHooks(), _ => new ImportRequestHooks
        { AfterUploadBytes = (_, _) => { cancellation.Cancel(); return ValueTask.CompletedTask; } });
        var response = await fixture.PostAsync("categoryId,amount,spentOn,note\n1,4.00,2026-09-22," + new string('x', 100_000) + "\n");
        await fixture.AssertCancelledZeroRowsAsync(response);
    }

    [Fact]
    public async Task Mid_parser_cancellation_returns_cancelled_dto_and_leaves_zero_rows()
    {
        using var cancellation = new CancellationTokenSource();
        await using var fixture = await CancellationFixture.CreateAsync(cancellation, _ => new ImportHooks
        { AfterSourceRow = _ => { cancellation.Cancel(); return ValueTask.CompletedTask; } });
        var response = await fixture.PostAsync(Csv);
        await fixture.AssertCancelledZeroRowsAsync(response);
    }
    [Fact]
    public async Task Precommit_cancellation_returns_cancelled_dto_and_leaves_zero_rows()
    {
        using var cancellation = new CancellationTokenSource();
        await using var fixture = await CancellationFixture.CreateAsync(cancellation, hooks => new ImportHooks
        {
            BeforeCommit = _ =>
            {
                cancellation.Cancel();
                return ValueTask.CompletedTask;
            }
        });

        await fixture.AssertCancelledZeroRowsAsync(await fixture.PostAsync(Csv));
    }

    [Fact]
    public async Task After_confirmed_commit_cancellation_remains_committed()
    {
        using var cancellation = new CancellationTokenSource();
        await using var fixture = await CancellationFixture.CreateAsync(cancellation, _ => new ImportHooks
        { AfterCommit = () => { cancellation.Cancel(); return ValueTask.CompletedTask; } });
        var response = await fixture.PostAsync(Csv);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("committed", body.GetProperty("status").GetString());
        Assert.Equal(1, await fixture.CountAsync());
    }

    [Fact]
    public async Task Unknown_after_commit_simulation_returns_safe_unknown_and_observer_sees_row()
    {
        await using var fixture = await UnknownCommitFixture.CreateAsync();
        var response = await fixture.PostAsync(Csv);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("unknown", body.GetProperty("status").GetString());
        Assert.True(body.GetProperty("traceId").GetString()!.Length > 0);
        Assert.Equal(1, await fixture.CountAsync());
    }
}

public sealed class CancellationFixture : IAsyncDisposable
{
    public required HttpClient Client { get; init; }
    public required WebApplicationFactory<Program> Factory { get; init; }
    private string Root { get; init; } = null!;

    public static Task<CancellationFixture> CreateAsync(CancellationTokenSource cancellation, Func<CancellationTokenSource, ImportHooks> createHooks, Func<CancellationTokenSource, ImportRequestHooks>? createRequestHooks = null)
    {
        var evidenceRoot = Environment.GetEnvironmentVariable("DN54_EVIDENCE_ROOT") ?? Path.Combine(Path.GetTempPath(), "dn54-api-boundary-973-" + Guid.NewGuid().ToString("N"));
        var root = Path.Combine(evidenceRoot, "cancel-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Environment.SetEnvironmentVariable("DN54_DB_NAME", "RoadmapImport915_" + Guid.NewGuid().ToString("N"));
        Environment.SetEnvironmentVariable("DN54_FIXTURE_ROOT", root);
        Environment.SetEnvironmentVariable("DN54_LISTEN_URL", "http://127.0.0.1:0");
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<ImportHooks>();
            services.AddSingleton(createHooks(cancellation));
            services.RemoveAll<ImportRequestHooks>();
            services.AddSingleton(createRequestHooks?.Invoke(cancellation) ?? new ImportRequestHooks());
            services.AddSingleton<IStartupFilter>(new RequestAbortedFilter(cancellation.Token));
        }));
        return Task.FromResult(new CancellationFixture { Client = factory.CreateClient(), Factory = factory, Root = root });
    }

    public async Task<HttpResponseMessage> PostAsync(string csv)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(csv, Encoding.UTF8, "text/csv"), "file", "cancel.csv");
        return await Client.PostAsync("/api/v1/expenses/import", form);
    }

    public async Task AssertCancelledZeroRowsAsync(HttpResponseMessage response)
    {
        Assert.Equal((HttpStatusCode)499, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("cancelled", body.GetProperty("status").GetString());
        Assert.True(body.GetProperty("traceId").GetString()!.Length > 0);
        Assert.Equal(0, await CountAsync());
    }

    public async Task<int> CountAsync()
    {
        var rows = await Client.GetFromJsonAsync<JsonElement[]>("/api/v1/expenses");
        return rows?.Length ?? 0;
    }

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return ValueTask.CompletedTask;
    }
}

public sealed class RequestAbortedFilter(CancellationToken requestToken) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use(async (context, continuation) =>
        {
            if (context.Request.Method == HttpMethods.Post && context.Request.Path.Equals("/api/v1/expenses/import", StringComparison.OrdinalIgnoreCase))
                context.RequestAborted = requestToken;
            await continuation();
        });
        next(app);
    };
}

public sealed class UnknownCommitFixture : IAsyncDisposable
{
    public required HttpClient Client { get; init; }
    public required WebApplicationFactory<Program> Factory { get; init; }
    public required string DatabaseName { get; init; }

    public static Task<UnknownCommitFixture> CreateAsync()
    {
        var evidenceRoot = Environment.GetEnvironmentVariable("DN54_EVIDENCE_ROOT") ?? Path.Combine(Path.GetTempPath(), "dn54-api-boundary-973-" + Guid.NewGuid().ToString("N"));
        var root = Path.Combine(evidenceRoot, "unknown-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var databaseName = "RoadmapImport915_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable("DN54_DB_NAME", databaseName);
        Environment.SetEnvironmentVariable("DN54_FIXTURE_ROOT", root);
        Environment.SetEnvironmentVariable("DN54_LISTEN_URL", "http://127.0.0.1:0");
        var interceptor = new ThrowAfterCommitInterceptor();
        var connectionString = ImportDatabase.ConnectionString(databaseName);
        var options = new DbContextOptionsBuilder<ImportDbContext>().UseSqlServer(connectionString).AddInterceptors(interceptor).Options;
        var contextFactory = new PooledDbContextFactory<ImportDbContext>(options);
        var service = new ExpenseImportService(contextFactory);
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextFactory<ImportDbContext>>();
            services.RemoveAll<ExpenseImportService>();
            services.AddSingleton<IDbContextFactory<ImportDbContext>>(contextFactory);
            services.AddSingleton(service);
        }));
        return Task.FromResult(new UnknownCommitFixture { Client = factory.CreateClient(), Factory = factory, DatabaseName = databaseName });
    }

    public MultipartFormDataContent Form(string csv)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(csv, Encoding.UTF8, "text/csv"), "file", "unknown.csv");
        return form;
    }

    public async Task<HttpResponseMessage> PostAsync(string csv) => await Client.PostAsync("/api/v1/expenses/import", Form(csv));
    public async Task<int> CountAsync() => (await Client.GetFromJsonAsync<JsonElement[]>("/api/v1/expenses"))?.Length ?? 0;
    public ValueTask DisposeAsync() { Client.Dispose(); Factory.Dispose(); return ValueTask.CompletedTask; }
}

public sealed class ThrowAfterCommitInterceptor : DbTransactionInterceptor
{
    private int thrown;
    public override Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref thrown, 1) == 0) throw new InvalidOperationException("simulated post-commit transport uncertainty");
        return Task.CompletedTask;
    }
}
