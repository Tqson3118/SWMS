using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ExpenseImportPersistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Stage8IntegrationApiTests;

[Collection("API")]
public sealed class ContractTests
{
    [Fact]
    public async Task Exactly_1000_unique_rows_commit_with_numeric_counts()
    {
        await using var fixture = await ContractFixture.CreateAsync();
        var csv = new StringBuilder("categoryId,amount,spentOn,note\n");
        for (var i = 0; i < 1000; i++) csv.Append($"1,{i + 1}.00,2026-09-22,unique-{i}\n");
        var body = await fixture.PostJsonAsync(csv.ToString());
        Assert.Equal("committed", body.GetProperty("status").GetString());
        Assert.Equal(1000, body.GetProperty("readRows").GetInt32());
        Assert.Equal(1000, body.GetProperty("acceptedRows").GetInt32());
        Assert.Equal(0, body.GetProperty("skippedDuplicates").GetInt32());
        Assert.Equal(JsonValueKind.Array, body.GetProperty("errors").ValueKind);
        Assert.Equal(1000, await fixture.CountAsync());
    }

    [Fact]
    public async Task Missing_file_and_malformed_multipart_use_complete_safe_error_cards()
    {
        await using var fixture = await ContractFixture.CreateAsync();
        using var missing = new MultipartFormDataContent();
        var missingBody = await fixture.PostAsync(missing);
        Assert.Equal(HttpStatusCode.BadRequest, missingBody.Response.StatusCode);
        AssertSafeRejected(missingBody, "invalid_multipart");

        using var malformed = new StringContent("not multipart", Encoding.UTF8, "multipart/form-data");
        var malformedBody = await fixture.PostAsync(malformed);
        Assert.Equal(HttpStatusCode.BadRequest, malformedBody.Response.StatusCode);
        AssertSafeRejected(malformedBody, "invalid_multipart");
    }

    [Fact]
    public async Task Header_only_and_empty_files_are_explicit_noop_commits()
    {
        await using var fixture = await ContractFixture.CreateAsync();
        foreach (var csv in new[] { "", "categoryId,amount,spentOn,note\n" })
        {
            var body = await fixture.PostJsonAsync(csv);
            Assert.Equal("committed", body.GetProperty("status").GetString());
            Assert.Equal(0, body.GetProperty("readRows").GetInt32());
            Assert.Equal(0, body.GetProperty("acceptedRows").GetInt32());
            Assert.Equal(0, body.GetProperty("skippedDuplicates").GetInt32());
            Assert.Empty(body.GetProperty("errors").EnumerateArray());
        }
    }

    [Fact]
    public async Task Retained_file_growth_is_rechecked_and_rejected_before_parse()
    {
        await using var fixture = await ContractFixture.CreateAsync(requestHooks: new ImportRequestHooks
        {
            AfterRetain = async (path, _) => await File.AppendAllTextAsync(path, new string('x', 1024 * 1024))
        });
        var result = await fixture.PostResultAsync("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,growth\n");
        AssertSafeRejected(result, "file_too_large");
        Assert.Equal(0, await fixture.CountAsync());
    }

    [Fact]
    public async Task Unknown_response_has_null_counts_and_one_safe_outcome_error()
    {
        await using var fixture = await ContractFixture.CreateAsync(new ImportHooks { BeforeCommit = _ => throw new InvalidOperationException("test unknown") });
        var body = await fixture.PostJsonAsync("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,unknown\n");
        Assert.Equal("unknown", body.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("readRows").ValueKind);
        Assert.Equal(JsonValueKind.Null, body.GetProperty("acceptedRows").ValueKind);
        Assert.Equal(JsonValueKind.Null, body.GetProperty("skippedDuplicates").ValueKind);
        var error = Assert.Single(body.GetProperty("errors").EnumerateArray());
        Assert.Equal("outcome_unknown", error.GetProperty("code").GetString());
        Assert.True(body.GetProperty("traceId").GetString()!.Length > 0);
    }

    [Fact]
    public async Task Service_source_fault_is_safe_invalid_import_with_zero_rows()
    {
        await using var fixture = await ContractFixture.CreateAsync(new ImportHooks { FaultAfterSourceRow = 1 });
        var result = await fixture.PostResultAsync("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,fault\n");
        AssertSafeRejected(result, "invalid_import");
        Assert.Equal(0, await fixture.CountAsync());
    }

    private static void AssertSafeRejected((HttpResponseMessage Response, JsonElement Json) result, string code)
    {
        Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
        Assert.Equal("rejected", result.Json.GetProperty("status").GetString());
        Assert.Equal(0, result.Json.GetProperty("readRows").GetInt32());
        Assert.Equal(0, result.Json.GetProperty("acceptedRows").GetInt32());
        Assert.Equal(0, result.Json.GetProperty("skippedDuplicates").GetInt32());
        var error = Assert.Single(result.Json.GetProperty("errors").EnumerateArray());
        Assert.Equal(code, error.GetProperty("code").GetString());
        Assert.DoesNotContain("D:\\", error.GetProperty("message").GetString());
        Assert.True(result.Json.GetProperty("traceId").GetString()!.Length > 0);
    }
}

public sealed class ContractFixture : IAsyncDisposable
{
    private string Root { get; init; } = null!;
    public required HttpClient Client { get; init; }
    public required WebApplicationFactory<Program> Factory { get; init; }

    public static Task<ContractFixture> CreateAsync(ImportHooks? hooks = null, ImportRequestHooks? requestHooks = null)
    {
        var evidence = Environment.GetEnvironmentVariable("DN54_EVIDENCE_ROOT") ?? Path.Combine(Path.GetTempPath(), "dn54-contract-978-" + Guid.NewGuid().ToString("N"));
        var root = Path.Combine(evidence, "contract-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Environment.SetEnvironmentVariable("DN54_DB_NAME", ImportDatabase.NewName());
        Environment.SetEnvironmentVariable("DN54_FIXTURE_ROOT", root);
        Environment.SetEnvironmentVariable("DN54_LISTEN_URL", ApiContract.DefaultListenUrl);
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            if (hooks is not null) { services.RemoveAll<ImportHooks>(); services.AddSingleton(hooks); }
            if (requestHooks is not null) { services.RemoveAll<ImportRequestHooks>(); services.AddSingleton(requestHooks); }
        }));
        return Task.FromResult(new ContractFixture { Client = factory.CreateClient(), Factory = factory, Root = root });
    }

    public async Task<(HttpResponseMessage Response, JsonElement Json)> PostAsync(HttpContent content)
    {
        var response = await Client.PostAsync("/api/v1/expenses/import", content);
        return (response, await response.Content.ReadFromJsonAsync<JsonElement>());
    }

    public async Task<JsonElement> PostJsonAsync(string csv)
    {
        using var form = Form(csv);
        return (await PostAsync(form)).Json;
    }

    public async Task<(HttpResponseMessage Response, JsonElement Json)> PostResultAsync(string csv)
    {
        using var form = Form(csv);
        return await PostAsync(form);
    }

    private MultipartFormDataContent Form(string csv) { var form = new MultipartFormDataContent(); form.Add(new StringContent(csv, Encoding.UTF8, "text/csv"), "file", "contract.csv"); return form; }
    public async Task<int> CountAsync() => (await Client.GetFromJsonAsync<JsonElement[]>("/api/v1/expenses"))?.Length ?? 0;
    public ValueTask DisposeAsync() { Client.Dispose(); Factory.Dispose(); return ValueTask.CompletedTask; }
}
