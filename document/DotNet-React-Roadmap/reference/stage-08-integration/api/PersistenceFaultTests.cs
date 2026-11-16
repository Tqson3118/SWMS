using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ExpenseImportPersistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Stage8IntegrationApiTests;

[Collection("API")]
public sealed class PersistenceFaultTests
{
    [Fact]
    public async Task Saving_changes_dbupdate_fault_returns_safe_unknown_and_zero_observed_rows()
    {
        await using var fixture = await FaultFixture.CreateAsync(new ThrowOnSavingChangesInterceptor());
        var response = await fixture.PostAsync("categoryId,amount,spentOn,note\n1,8.00,2026-09-22,save-fault\n");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("unknown", body.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("readRows").ValueKind);
        Assert.Equal(JsonValueKind.Null, body.GetProperty("acceptedRows").ValueKind);
        Assert.Equal(JsonValueKind.Null, body.GetProperty("skippedDuplicates").ValueKind);
        Assert.Equal("outcome_unknown", Assert.Single(body.GetProperty("errors").EnumerateArray()).GetProperty("code").GetString());
        Assert.True(body.GetProperty("traceId").GetString()!.Length > 0);
        Assert.Empty(await fixture.GetRowsAsync());
    }

    [Fact]
    public async Task Exclusive_retained_file_lease_returns_safe_invalid_import_and_zero_rows()
    {
        FileStream? lease = null;
        try
        {
            var hooks = new ImportRequestHooks
            {
                AfterRetain = (path, _) =>
                {
                    lease = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                    return ValueTask.CompletedTask;
                }
            };
            await using var fixture = await FaultFixture.CreateAsync(requestHooks: hooks);
            var response = await fixture.PostAsync("categoryId,amount,spentOn,note\n1,9.00,2026-09-22,lease\n");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("rejected", body.GetProperty("status").GetString());
            Assert.Equal("read-fault", Assert.Single(body.GetProperty("errors").EnumerateArray()).GetProperty("code").GetString());
            Assert.DoesNotContain("D:\\", body.GetProperty("errors")[0].GetProperty("message").GetString());
            Assert.Empty(await fixture.GetRowsAsync());
        }
        finally { lease?.Dispose(); }
    }
}

public sealed class ThrowOnSavingChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        => ValueTask.FromException<InterceptionResult<int>>(new DbUpdateException("private database detail", new InvalidOperationException("private inner detail")));
}

public sealed class FaultFixture : IAsyncDisposable
{
    public required HttpClient Client { get; init; }
    public required WebApplicationFactory<Program> Factory { get; init; }

    public static Task<FaultFixture> CreateAsync(SaveChangesInterceptor? interceptor = null, ImportRequestHooks? requestHooks = null)
    {
        var evidence = Environment.GetEnvironmentVariable("DN54_EVIDENCE_ROOT") ?? Path.Combine(Path.GetTempPath(), "dn54-api-final-gaps-986-" + Guid.NewGuid().ToString("N"));
        var root = Path.Combine(evidence, "fault-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var name = ImportDatabase.NewName();
        Environment.SetEnvironmentVariable("DN54_DB_NAME", name);
        Environment.SetEnvironmentVariable("DN54_FIXTURE_ROOT", root);
        Environment.SetEnvironmentVariable("DN54_LISTEN_URL", ApiContract.DefaultListenUrl);
        var optionsBuilder = new DbContextOptionsBuilder<ImportDbContext>().UseSqlServer(ImportDatabase.ConnectionString(name));
        if (interceptor is not null) optionsBuilder.AddInterceptors(interceptor);
        var contextFactory = new PooledDbContextFactory<ImportDbContext>(optionsBuilder.Options);
        var service = new ExpenseImportService(contextFactory);
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextFactory<ImportDbContext>>();
            services.RemoveAll<ExpenseImportService>();
            services.AddSingleton<IDbContextFactory<ImportDbContext>>(contextFactory);
            services.AddSingleton(service);
            if (requestHooks is not null) { services.RemoveAll<ImportRequestHooks>(); services.AddSingleton(requestHooks); }
        }));
        return Task.FromResult(new FaultFixture { Client = factory.CreateClient(), Factory = factory });
    }

    public async Task<HttpResponseMessage> PostAsync(string csv)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(csv, Encoding.UTF8, "text/csv"), "file", "fault.csv");
        return await Client.PostAsync("/api/v1/expenses/import", form);
    }

    public async Task<JsonElement[]> GetRowsAsync() => await Client.GetFromJsonAsync<JsonElement[]>("/api/v1/expenses") ?? [];
    public ValueTask DisposeAsync() { Client.Dispose(); Factory.Dispose(); return ValueTask.CompletedTask; }
}
