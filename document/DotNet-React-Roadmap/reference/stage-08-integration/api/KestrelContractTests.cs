using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Concurrent;
using ExpenseImportPersistence;
using Xunit;

namespace Stage8IntegrationApiTests;

[Collection("API")]
public sealed class KestrelContractTests
{
    [Fact]
    public async Task Real_kestrel_binds_loopback_and_rejects_oversized_whole_multipart_safely()
    {
        var evidence = Environment.GetEnvironmentVariable("DN54_EVIDENCE_ROOT") ?? Path.Combine(Path.GetTempPath(), "dn54-kestrel-978-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(evidence);
        var fixtureRoot = Path.Combine(evidence, "kestrel-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fixtureRoot);
        var database = ImportDatabase.NewName();
        var outputPath = Path.Combine(evidence, "kestrel.stdout.log");
        var errorPath = Path.Combine(evidence, "kestrel.stderr.log");
        var project = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../Stage8IntegrationApi.csproj"));
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --no-build --project \"{project}\"",
                WorkingDirectory = Path.GetDirectoryName(project)!,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        process.StartInfo.Environment["DN54_DB_NAME"] = database;
        process.StartInfo.Environment["DN54_FIXTURE_ROOT"] = fixtureRoot;
        process.StartInfo.Environment["DN54_LISTEN_URL"] = ApiContract.DefaultListenUrl;
        using var output = new StreamWriter(new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite));
        using var error = new StreamWriter(new FileStream(errorPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite));
        var lines = new ConcurrentQueue<string>();
        process.OutputDataReceived += (_, args) => { if (args.Data is not null) { lines.Enqueue(args.Data); output.WriteLine(args.Data); output.Flush(); } };
        process.ErrorDataReceived += (_, args) => { if (args.Data is not null) { error.WriteLine(args.Data); error.Flush(); } };
        Assert.True(process.Start());
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        try
        {
            var port = await WaitForPortAsync(lines, process, TimeSpan.FromSeconds(30));
            using var client = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") };
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(new string('x', 1_200_000)), "metadata");
            form.Add(new StringContent("categoryId,amount,spentOn,note\n"), "file", "empty.csv");
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/expenses/import") { Content = form };
            request.Headers.ExpectContinue = true;
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("rejected", body.GetProperty("status").GetString());
            Assert.Equal("request_too_large", Assert.Single(body.GetProperty("errors").EnumerateArray()).GetProperty("code").GetString());
            Assert.Equal(0, body.GetProperty("readRows").GetInt32());
            Assert.Equal(0, body.GetProperty("acceptedRows").GetInt32());
            Assert.Equal(0, body.GetProperty("skippedDuplicates").GetInt32());
            Assert.Empty(await client.GetFromJsonAsync<JsonElement[]>("/api/v1/expenses") ?? []);
        }
        finally
        {
            if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
        }
    }

    private static async Task<int> WaitForPortAsync(ConcurrentQueue<string> lines, Process process, TimeSpan timeout)
    {
        var until = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < until && !process.HasExited)
        {
            if (lines.LastOrDefault(x => x.StartsWith("DN54_LISTENING=http://127.0.0.1:", StringComparison.Ordinal)) is { } line)
            {
                if (int.TryParse(line[(line.LastIndexOf(':') + 1)..].TrimEnd('/'), out var port)) return port;
            }
            await Task.Delay(100);
        }
        throw new TimeoutException($"Kestrel did not report loopback readiness. Exit={process.ExitCode}");
    }
}
