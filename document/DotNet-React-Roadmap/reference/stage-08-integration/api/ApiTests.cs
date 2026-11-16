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

[CollectionDefinition("API", DisableParallelization = true)] public sealed class ApiCollection : ICollectionFixture<ApiFixture> { }
[Collection("API")]
public sealed class ApiTests(ApiFixture fixture)
{
    [Fact] public void Listen_url_must_be_loopback_with_ephemeral_port() { foreach (var invalid in new[] { "http://0.0.0.0:5000", "http://127.0.0.1:5000", "https://127.0.0.1:0", "http://[::1]:0", "http://127.0.0.2:0" }) Assert.Throws<InvalidOperationException>(() => ApiContract.ValidateListenUrl(invalid)); Assert.Equal("http://127.0.0.1:0", ApiContract.ValidateListenUrl("http://127.0.0.1:0")); Assert.Equal("http://127.0.0.1:0", ApiContract.ValidateListenUrl("http://127.0.0.1:0/")); }
    [Fact] public async Task Valid_max_money_and_get_preserve_strings() { using var form = fixture.Form("categoryId,amount,spentOn,note\n1,9999999999999999.99,2026-09-22,max\n"); var post = await fixture.Client.PostAsync("/api/v1/expenses/import", form); Assert.Equal(HttpStatusCode.OK, post.StatusCode); var body = await post.Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal("committed", body.GetProperty("status").GetString()); var get = await fixture.Client.GetFromJsonAsync<JsonElement[]>("/api/v1/expenses"); Assert.Contains(get!, x => x.GetProperty("amount").GetString() == "9999999999999999.99" && x.GetProperty("spentOn").GetString() == "2026-09-22"); }
    [Fact] public async Task Malformed_mixed_batch_has_safe_line_error_and_zero_new_effects() { var before = await fixture.CountAsync(); using var form = fixture.Form("categoryId,amount,spentOn,note\n1,1.00,2026-09-22,ok\n9,2.00,2026-09-23,bad\n"); var response = await fixture.Client.PostAsync("/api/v1/expenses/import", form); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); var body = await response.Content.ReadFromJsonAsync<JsonElement>(); Assert.True(body.GetProperty("traceId").GetString()!.Length > 0); var error = Assert.Single(body.GetProperty("errors").EnumerateArray()); Assert.Equal(3, error.GetProperty("lineNumber").GetInt32()); Assert.Equal("category", error.GetProperty("code").GetString()); Assert.DoesNotContain("D:\\", error.GetProperty("message").GetString()); Assert.Equal(before, await fixture.CountAsync()); }
    [Fact] public async Task Duplicate_only_commits_unique_rows() { using var form = fixture.Form("categoryId,amount,spentOn,note\n2,2.00,2026-09-23,d\n2,2.00,2026-09-23,d\n"); var response = await fixture.Client.PostAsync("/api/v1/expenses/import", form); Assert.Equal(HttpStatusCode.OK, response.StatusCode); var body = await response.Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal(1, body.GetProperty("skippedDuplicates").GetInt32()); Assert.Equal(1, body.GetProperty("acceptedRows").GetInt32()); }
    [Fact] public async Task Missing_empty_and_header_only_are_explicit_safe_rejections() { using var missing = new MultipartFormDataContent(); var missingResponse = await fixture.Client.PostAsync("/api/v1/expenses/import", missing); Assert.Equal(HttpStatusCode.BadRequest, missingResponse.StatusCode); var missingBody = await missingResponse.Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal("invalid_multipart", Assert.Single(missingBody.GetProperty("errors").EnumerateArray()).GetProperty("code").GetString()); using var wrongContent = new StringContent("{}", Encoding.UTF8, "application/json"); var wrongResponse = await fixture.Client.PostAsync("/api/v1/expenses/import", wrongContent); Assert.Equal(HttpStatusCode.BadRequest, wrongResponse.StatusCode); using var empty = fixture.Form(""); var emptyBody = await (await fixture.Client.PostAsync("/api/v1/expenses/import", empty)).Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal("committed", emptyBody.GetProperty("status").GetString()); Assert.Equal(0, emptyBody.GetProperty("readRows").GetInt32()); using var header = fixture.Form("categoryId,amount,spentOn,note\n"); var headerBody = await (await fixture.Client.PostAsync("/api/v1/expenses/import", header)).Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal("committed", headerBody.GetProperty("status").GetString()); Assert.Equal(0, headerBody.GetProperty("acceptedRows").GetInt32()); }
    [Fact] public async Task File_limit_is_actual_bytes() { using var form = fixture.Form(new string('x', 1024 * 1024 + 1)); var response = await fixture.Client.PostAsync("/api/v1/expenses/import", form); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); }
    [Fact] public async Task Row_limit_is_rejected_without_effect() { var csv = new StringBuilder("categoryId,amount,spentOn,note\n"); for (var i = 0; i < 1001; i++) csv.Append("1,1.00,2026-09-22,row\n"); var before = await fixture.CountAsync(); using var form = fixture.Form(csv.ToString()); var response = await fixture.Client.PostAsync("/api/v1/expenses/import", form); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); Assert.Equal(before, await fixture.CountAsync()); }

    [Fact] public async Task Instructor_precommit_cancellation_is_safe_and_leaves_zero_rows()
    {
        await using var injected = await InjectedApi.CreateAsync(() => new ImportHooks { BeforeCommit = _ => throw new OperationCanceledException() });
        using var form = injected.Form("categoryId,amount,spentOn,note\n1,4.00,2026-09-22,cancel\n");
        var response = await injected.Client.PostAsync("/api/v1/expenses/import", form);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Empty(await injected.GetRowsAsync());
    }

    [Fact] public async Task Instructor_postcommit_and_diagnostic_failures_preserve_committed_status()
    {
        await using var injected = await InjectedApi.CreateAsync(() => new ImportHooks { AfterCommit = () => throw new InvalidOperationException("private hook"), DiagnosticSink = _ => throw new InvalidOperationException("private sink") });
        using var form = injected.Form("categoryId,amount,spentOn,note\n1,5.00,2026-09-22,committed\n");
        var body = await (await injected.Client.PostAsync("/api/v1/expenses/import", form)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("committed-with-postcommit-error", body.GetProperty("status").GetString());
        Assert.Single(await injected.GetRowsAsync());
    }
}

public sealed class ApiFixture : IAsyncLifetime
{
    public HttpClient Client { get; private set; } = null!;
    private WebApplicationFactory<Program> Factory { get; set; } = null!;
    private string FixtureRoot { get; } = Environment.GetEnvironmentVariable("DN54_EVIDENCE_ROOT") ?? Path.Combine(Path.GetTempPath(), "dn54-api-" + Guid.NewGuid().ToString("N"));
    public Task InitializeAsync() { Directory.CreateDirectory(FixtureRoot); Environment.SetEnvironmentVariable("DN54_DB_NAME", "RoadmapImport915_" + Guid.NewGuid().ToString("N")); Environment.SetEnvironmentVariable("DN54_FIXTURE_ROOT", FixtureRoot); Factory = new WebApplicationFactory<Program>(); Client = Factory.CreateClient(); return Task.CompletedTask; }
    public Task DisposeAsync() { Client.Dispose(); Factory.Dispose(); return Task.CompletedTask; }
    public MultipartFormDataContent Form(string csv) { var form = new MultipartFormDataContent(); form.Add(new StringContent(csv, Encoding.UTF8, "text/csv"), "file", "upload.csv"); return form; }
    public async Task<int> CountAsync() { var rows = await Client.GetFromJsonAsync<JsonElement[]>("/api/v1/expenses"); return rows?.Length ?? 0; }
}

public sealed class InjectedApi : IAsyncDisposable
{
    public required HttpClient Client { get; init; }
    public required WebApplicationFactory<Program> Factory { get; init; }
    private string Root { get; init; } = null!;
    public MultipartFormDataContent Form(string csv) { var form = new MultipartFormDataContent(); form.Add(new StringContent(csv, Encoding.UTF8, "text/csv"), "file", "upload.csv"); return form; }
    public async Task<JsonElement[]> GetRowsAsync() => await Client.GetFromJsonAsync<JsonElement[]>("/api/v1/expenses") ?? [];
    public static Task<InjectedApi> CreateAsync(Func<ImportHooks> createHooks)
    {
        var evidenceRoot = Environment.GetEnvironmentVariable("DN54_EVIDENCE_ROOT") ?? Path.Combine(Path.GetTempPath(), "dn54-api-961-" + Guid.NewGuid().ToString("N")); var root = Path.Combine(evidenceRoot, "injected-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root); Environment.SetEnvironmentVariable("DN54_DB_NAME", "RoadmapImport915_" + Guid.NewGuid().ToString("N")); Environment.SetEnvironmentVariable("DN54_FIXTURE_ROOT", root);
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureServices(services => { services.RemoveAll<ImportHooks>(); services.AddSingleton(createHooks()); }));
        return Task.FromResult(new InjectedApi { Client = factory.CreateClient(), Factory = factory, Root = root });
    }
    public ValueTask DisposeAsync() { Client.Dispose(); Factory.Dispose(); return ValueTask.CompletedTask; }
}
