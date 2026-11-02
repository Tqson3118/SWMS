using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;
using Swms.Api.Features;
using Swms.Api.Features.Pdf;
using Swms.Api.Infrastructure.Database;
using Xunit;

namespace Swms.Tests;

public class Milestone1EmpiricalChallengeTests
{
    private readonly PdfExtractionService _pdfService = new();

    private static string GetRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (Directory.Exists(Path.Combine(dir, "test-fixtures")) && Directory.Exists(Path.Combine(dir, "test")))
            {
                return dir;
            }
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        throw new DirectoryNotFoundException("Could not find repository root containing 'test-fixtures' and 'test'.");
    }

    // =========================================================================
    // 1. ADVERSARIAL HTTP 206 RANGE STREAMING & CONCURRENCY
    // =========================================================================

    [Fact]
    public async Task RangeStreaming_SingleByte_MiddleSlice_Suffix_AndInvalidRange_BehavePerHttpSpec()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        // Upload small 3-page PDF to obtain a valid document
        var fixturePath = Path.Combine(GetRepoRoot(), "test-fixtures", "small-3p.pdf");
        var fileBytes = await File.ReadAllBytesAsync(fixturePath);
        var fileSize = fileBytes.Length; // 1972 bytes

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "small-3p.pdf");

        var uploadResp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);
        var docJson = await uploadResp.Content.ReadFromJsonAsync<JsonElement>();
        var docId = docJson.GetProperty("id").GetGuid();

        // Case 1A: Single-byte range (bytes=0-0)
        using (var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{docId}/file"))
        {
            req.Headers.Range = new RangeHeaderValue(0, 0);
            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            var content = await resp.Content.ReadAsByteArrayAsync();
            Assert.Single(content);
            Assert.Equal(fileBytes[0], content[0]);
            Assert.Equal("bytes", resp.Headers.AcceptRanges.ToString());
            Assert.NotNull(resp.Content.Headers.ContentRange);
            Assert.Equal(0, resp.Content.Headers.ContentRange.From);
            Assert.Equal(0, resp.Content.Headers.ContentRange.To);
            Assert.Equal(fileSize, resp.Content.Headers.ContentRange.Length);
        }

        // Case 1B: Intermediate slice (bytes=100-200, 101 bytes)
        using (var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{docId}/file"))
        {
            req.Headers.Range = new RangeHeaderValue(100, 200);
            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            var content = await resp.Content.ReadAsByteArrayAsync();
            Assert.Equal(101, content.Length);
            Assert.Equal(fileBytes.AsSpan(100, 101).ToArray(), content);
        }

        // Case 1C: Suffix range (last 128 bytes)
        using (var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{docId}/file"))
        {
            req.Headers.Range = new RangeHeaderValue(fileSize - 128, fileSize - 1);
            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            var content = await resp.Content.ReadAsByteArrayAsync();
            Assert.Equal(128, content.Length);
            Assert.Equal(fileBytes.AsSpan(fileSize - 128, 128).ToArray(), content);
        }

        // Case 1D: Out of bounds range (bytes=9999999-9999999) -> 416 Range Not Satisfiable
        using (var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{docId}/file"))
        {
            req.Headers.Range = new RangeHeaderValue(fileSize + 1000, fileSize + 2000);
            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, resp.StatusCode);
        }

        // Case 1E: Non-existent document -> 404 NotFound
        var missingDocResp = await client.GetAsync($"/api/v1/documents/{Guid.NewGuid()}/file");
        Assert.Equal(HttpStatusCode.NotFound, missingDocResp.StatusCode);
    }

    [Fact]
    public async Task RangeStreaming_ConcurrentStress_20ParallelSlices_SucceedsWithoutFileLockException()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var fixturePath = Path.Combine(GetRepoRoot(), "test-fixtures", "marker-12p.pdf");
        var fileBytes = await File.ReadAllBytesAsync(fixturePath);

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "marker-12p.pdf");

        var uploadResp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);
        var docJson = await uploadResp.Content.ReadFromJsonAsync<JsonElement>();
        var docId = docJson.GetProperty("id").GetGuid();

        // Fire 20 parallel requests with different byte ranges
        var tasks = Enumerable.Range(0, 20).Select(async i =>
        {
            long start = (i * 100) % (fileBytes.Length - 100);
            long end = start + 50;

            using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{docId}/file");
            req.Headers.Range = new RangeHeaderValue(start, end);
            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            var bytes = await resp.Content.ReadAsByteArrayAsync();
            Assert.Equal(51, bytes.Length);
            Assert.Equal(fileBytes.AsSpan((int)start, 51).ToArray(), bytes);
        });

        await Task.WhenAll(tasks);
    }

    // =========================================================================
    // 2. EDGE CASES: LARGE-305P, SCANNED/IMAGE-ONLY, SPECIAL MARKERS
    // =========================================================================

    [Fact]
    public void Large305PagePdf_DirectExtraction_SucceedsAndMatchesMarkers()
    {
        var path = Path.Combine(GetRepoRoot(), "test-fixtures", "large-305p.pdf");
        Assert.True(File.Exists(path));

        var bytes = File.ReadAllBytes(path);
        var result = _pdfService.Extract(bytes);

        Assert.True(result.Success);
        Assert.Equal(305, result.TotalPages);
        Assert.Equal(305, result.Pages.Count);

        // Verify special markers defined in manifest.json
        var p150 = result.Pages[149];
        Assert.Equal(150, p150.PageNumber);
        Assert.Contains("LARGE-PAGE-150-MARKER", p150.Text);

        var p200 = result.Pages[199];
        Assert.Equal(200, p200.PageNumber);
        Assert.Contains("LARGE-PAGE-200-MARKER", p200.Text);

        var p305 = result.Pages[304];
        Assert.Equal(305, p305.PageNumber);
        Assert.Contains("LARGE-PAGE-305-MARKER", p305.Text);
    }

    [Fact]
    public async Task Large305PagePdf_UploadAndPageQuery_SucceedsAllPages()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var path = Path.Combine(GetRepoRoot(), "test-fixtures", "large-305p.pdf");
        var fileBytes = await File.ReadAllBytesAsync(path);

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "large-305p.pdf");

        var uploadResp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);
        var docJson = await uploadResp.Content.ReadFromJsonAsync<JsonElement>();
        var docId = docJson.GetProperty("id").GetGuid();
        Assert.Equal(305, docJson.GetProperty("totalPages").GetInt32());

        // Verify querying page 305 specifically
        var p305Resp = await client.GetAsync($"/api/v1/documents/{docId}/pages/305");
        Assert.Equal(HttpStatusCode.OK, p305Resp.StatusCode);
        var p305Json = await p305Resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(305, p305Json.GetProperty("pageNumber").GetInt32());
        Assert.Contains("LARGE-PAGE-305-MARKER", p305Json.GetProperty("plainText").GetString());
    }

    [Fact]
    public void MixedScannedTextPdf_DistinguishesTextVsImageOnlyPages()
    {
        var path = Path.Combine(GetRepoRoot(), "test-fixtures", "mixed-scanned-text.pdf");
        Assert.True(File.Exists(path));

        var bytes = File.ReadAllBytes(path);
        var result = _pdfService.Extract(bytes);

        Assert.True(result.Success);
        Assert.Equal(4, result.TotalPages);
        Assert.Equal(4, result.Pages.Count);

        // Pages 1 and 3 have text
        Assert.True(result.Pages[0].HasTextLayer, "Page 1 should have text layer");
        Assert.True(result.Pages[0].CharCount > 0);

        Assert.True(result.Pages[2].HasTextLayer, "Page 3 should have text layer");
        Assert.True(result.Pages[2].CharCount > 0);

        // Pages 2 and 4 are image-only scans
        Assert.False(result.Pages[1].HasTextLayer, "Page 2 is image-only scan");
        Assert.Equal(0, result.Pages[1].CharCount);
        Assert.Equal(0, result.Pages[1].WordCount);
        Assert.Empty(result.Pages[1].Text);

        Assert.False(result.Pages[3].HasTextLayer, "Page 4 is image-only scan");
        Assert.Equal(0, result.Pages[3].CharCount);
        Assert.Equal(0, result.Pages[3].WordCount);
        Assert.Empty(result.Pages[3].Text);
    }

    [Fact]
    public void ComplexRealWorldPdfs_IetfRfc_And_MozillaTracemonkey_ExtractSuccessfully()
    {
        // 1. IETF RFC 7995 (26 pages)
        var rfcPath = Path.Combine(GetRepoRoot(), "test-fixtures", "ietf-rfc7995-26p.pdf");
        if (File.Exists(rfcPath))
        {
            var rfcResult = _pdfService.Extract(File.ReadAllBytes(rfcPath));
            Assert.True(rfcResult.Success);
            Assert.Equal(26, rfcResult.TotalPages);
            Assert.All(rfcResult.Pages, p => Assert.True(p.HasTextLayer));
        }

        // 2. Mozilla TraceMonkey (14 pages)
        var tracePath = Path.Combine(GetRepoRoot(), "test-fixtures", "mozilla-tracemonkey-14p.pdf");
        if (File.Exists(tracePath))
        {
            var traceResult = _pdfService.Extract(File.ReadAllBytes(tracePath));
            Assert.True(traceResult.Success);
            Assert.Equal(14, traceResult.TotalPages);
            Assert.All(traceResult.Pages, p => Assert.True(p.HasTextLayer));
        }
    }

    // =========================================================================
    // 3. MALFORMED, NON-PDF, AND TRUNCATED INPUTS
    // =========================================================================

    [Theory]
    [InlineData("corrupt-broken.pdf")]
    [InlineData("corrupt.pdf")]
    [InlineData("fake-pdf-text.pdf")]
    [InlineData("non-pdf.pdf")]
    public void MalformedAndFakePdfs_FailGracefullyWithCorruptFlag(string fixtureName)
    {
        var path = Path.Combine(GetRepoRoot(), "test-fixtures", fixtureName);
        if (!File.Exists(path)) return;

        var bytes = File.ReadAllBytes(path);
        var result = _pdfService.Extract(bytes);

        Assert.False(result.Success);
        Assert.True(result.IsCorrupt);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void EmptyAndTruncatedBytes_ReturnCorruptFailure()
    {
        var emptyResult = _pdfService.Extract(Array.Empty<byte>());
        Assert.False(emptyResult.Success);
        Assert.True(emptyResult.IsCorrupt);

        var tinyResult = _pdfService.Extract(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // %PDF without -
        Assert.False(tinyResult.Success);
        Assert.True(tinyResult.IsCorrupt);

        var nullStreamResult = _pdfService.Extract((Stream)null!);
        Assert.False(nullStreamResult.Success);
        Assert.True(nullStreamResult.IsCorrupt);
    }

    [Fact]
    public async Task Upload_NonPdfExtension_Returns400BadRequest()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("alert('evil')"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/javascript");
        form.Add(fileContent, "file", "malicious_script.js");

        var resp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    // =========================================================================
    // 4. PER-PAGE BOUNDARY & QUERY DEFENSIVE CHECKS
    // =========================================================================

    [Fact]
    public async Task PagesEndpoint_BoundaryPageNumbers_AndRangeFilters_BehavePredictably()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var path = Path.Combine(GetRepoRoot(), "test-fixtures", "marker-12p.pdf");
        var fileBytes = await File.ReadAllBytesAsync(path);

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "marker-12p.pdf");

        var uploadResp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);
        var docJson = await uploadResp.Content.ReadFromJsonAsync<JsonElement>();
        var docId = docJson.GetProperty("id").GetGuid();

        // 1. Boundary: Page 0 -> 400 Bad Request
        var page0Resp = await client.GetAsync($"/api/v1/documents/{docId}/pages/0");
        Assert.Equal(HttpStatusCode.BadRequest, page0Resp.StatusCode);

        // 2. Boundary: Negative page -> 400 Bad Request or route reject
        var negPageResp = await client.GetAsync($"/api/v1/documents/{docId}/pages/-5");
        Assert.True(negPageResp.StatusCode == HttpStatusCode.BadRequest || negPageResp.StatusCode == HttpStatusCode.NotFound);

        // 3. Boundary: Page beyond TotalPages (13 > 12) -> 400 Bad Request
        var page13Resp = await client.GetAsync($"/api/v1/documents/{docId}/pages/13");
        Assert.Equal(HttpStatusCode.BadRequest, page13Resp.StatusCode);

        // 4. Inverted range: from=10&to=5 -> returns empty array (0 items)
        var invertedResp = await client.GetAsync($"/api/v1/documents/{docId}/pages?from=10&to=5");
        Assert.Equal(HttpStatusCode.OK, invertedResp.StatusCode);
        var invertedJson = await invertedResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, invertedJson.GetArrayLength());

        // 5. Pagination: page=2, pageSize=5 -> returns pages 6 to 10
        var pagedResp = await client.GetAsync($"/api/v1/documents/{docId}/pages?page=2&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, pagedResp.StatusCode);
        var pagedJson = await pagedResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(5, pagedJson.GetArrayLength());
        Assert.Equal(6, pagedJson[0].GetProperty("pageNumber").GetInt32());
        Assert.Equal(10, pagedJson[4].GetProperty("pageNumber").GetInt32());
    }

    // =========================================================================
    // 5. CASCADE DELETION RELATIONAL INTEGRITY
    // =========================================================================

    [Fact]
    public async Task CascadeDelete_DeletingDocument_RemovesAllLinkedDocumentPages()
    {
        var options = new DbContextOptionsBuilder<SwmsDbContext>()
            .UseInMemoryDatabase($"SwmsCascadeTest_{Guid.NewGuid()}")
            .Options;

        await using var db = new SwmsDbContext(options);

        var docId = Guid.NewGuid();
        var doc = new Document
        {
            Id = docId,
            CourseId = Guid.NewGuid(),
            UploaderId = Guid.NewGuid(),
            Title = "Cascade Test Doc",
            FileUrl = $"/api/v1/documents/{docId}/file",
            FileSizeBytes = 1024,
            TotalPages = 3,
            MimeType = "application/pdf",
            OcrStatus = "completed"
        };
        db.Documents.Add(doc);

        for (int i = 1; i <= 3; i++)
        {
            db.DocumentPages.Add(new DocumentPage
            {
                Id = Guid.NewGuid(),
                DocumentId = docId,
                PageNumber = i,
                PlainText = $"Page {i} content",
                HasTextLayer = true,
                CharCount = 14,
                WordCount = 3
            });
        }
        await db.SaveChangesAsync();

        Assert.Equal(3, await db.DocumentPages.CountAsync(p => p.DocumentId == docId));

        // Delete document
        db.Documents.Remove(doc);
        await db.SaveChangesAsync();

        // DocumentPages should be cascade-deleted!
        Assert.Equal(0, await db.DocumentPages.CountAsync(p => p.DocumentId == docId));
    }
}
