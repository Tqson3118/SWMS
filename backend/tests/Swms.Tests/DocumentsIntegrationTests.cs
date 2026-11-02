using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Swms.Api.Features;
using Xunit;

namespace Swms.Tests;

public class DocumentsIntegrationTests
{
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

    [Fact]
    public async Task Upload_Multipart_PhapLuatDaiCuong_Succeeds_Extracts298Pages_StreamsRange()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var filePath = Path.Combine(GetRepoRoot(), "test", "Giáo trình Pháp luật đại cương_ebook.pdf");
        Assert.True(File.Exists(filePath), $"File {filePath} must exist.");

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "Giáo trình Pháp luật đại cương_ebook.pdf");
        form.Add(new StringContent("Giáo trình Pháp luật đại cương"), "title");

        // 1. Upload multi-page PDF
        var uploadResp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);

        var docJson = await uploadResp.Content.ReadFromJsonAsync<JsonElement>();
        var docId = docJson.GetProperty("id").GetGuid();
        var totalPages = docJson.GetProperty("totalPages").GetInt32();
        var fileUrl = docJson.GetProperty("fileUrl").GetString();

        Assert.Equal(298, totalPages);
        Assert.Equal($"/api/v1/documents/{docId}/file", fileUrl);

        // 2. Query Page 150 detail and compare with Oracle
        var page150Resp = await client.GetAsync($"/api/v1/documents/{docId}/pages/150");
        Assert.Equal(HttpStatusCode.OK, page150Resp.StatusCode);

        var page150Json = await page150Resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(150, page150Json.GetProperty("pageNumber").GetInt32());
        Assert.True(page150Json.GetProperty("hasTextLayer").GetBoolean());
        Assert.True(page150Json.GetProperty("charCount").GetInt32() > 1000);
        var page150Text = page150Json.GetProperty("plainText").GetString();
        Assert.NotNull(page150Text);
        // Assert text matches oracle ground truth on page 150
        Assert.Contains("Được thành lập một cách hợp pháp", page150Text);

        // 3. Query pages range
        var pagesRangeResp = await client.GetAsync($"/api/v1/documents/{docId}/pages?from=1&to=5");
        Assert.Equal(HttpStatusCode.OK, pagesRangeResp.StatusCode);
        var pagesRange = await pagesRangeResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(5, pagesRange.GetArrayLength());

        // 4. Test binary file serving: full download (200 OK with Accept-Ranges)
        var fileGetResp = await client.GetAsync($"/api/v1/documents/{docId}/file");
        Assert.Equal(HttpStatusCode.OK, fileGetResp.StatusCode);
        Assert.Equal("application/pdf", fileGetResp.Content.Headers.ContentType?.MediaType);
        Assert.Contains("bytes", fileGetResp.Headers.AcceptRanges);

        // 5. Test range streaming: partial content (HTTP 206)
        using var rangeReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/{docId}/file");
        rangeReq.Headers.Range = new RangeHeaderValue(0, 1024);
        var rangeResp = await client.SendAsync(rangeReq);

        Assert.Equal(HttpStatusCode.PartialContent, rangeResp.StatusCode);
        var partialBytes = await rangeResp.Content.ReadAsByteArrayAsync();
        Assert.Equal(1025, partialBytes.Length);
        // Validate %PDF header in range response
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(partialBytes.AsSpan(0, 5)));
    }

    [Fact]
    public async Task Upload_Multipart_NgonNguLapTrinhC_Succeeds_Extracts211Pages()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var filePath = Path.Combine(GetRepoRoot(), "test", "Ngôn ngữ lập trình C_ebook.pdf");
        Assert.True(File.Exists(filePath));

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "Ngôn ngữ lập trình C_ebook.pdf");

        var uploadResp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);

        var docJson = await uploadResp.Content.ReadFromJsonAsync<JsonElement>();
        var totalPages = docJson.GetProperty("totalPages").GetInt32();
        var docId = docJson.GetProperty("id").GetGuid();

        Assert.Equal(211, totalPages);

        var page211Resp = await client.GetAsync($"/api/v1/documents/{docId}/pages/211");
        Assert.Equal(HttpStatusCode.OK, page211Resp.StatusCode);
    }

    [Fact]
    public async Task Upload_CorruptPdf_Returns422_AndDoesNotDeductQuota()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        // 1. Check initial storage quota
        var quotaBeforeResp = await client.GetFromJsonAsync<JsonElement>("/api/v1/workspace/quotas");
        var storageBefore = quotaBeforeResp.GetProperty("storageBytesUsed").GetInt64();

        // 2. Upload corrupt PDF
        var corruptPath = Path.Combine(GetRepoRoot(), "test-fixtures", "corrupt-broken.pdf");
        Assert.True(File.Exists(corruptPath));
        var fileBytes = await File.ReadAllBytesAsync(corruptPath);

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "corrupt-broken.pdf");

        var resp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);

        // 3. Verify storage quota is intact
        var quotaAfterResp = await client.GetFromJsonAsync<JsonElement>("/api/v1/workspace/quotas");
        var storageAfter = quotaAfterResp.GetProperty("storageBytesUsed").GetInt64();
        Assert.Equal(storageBefore, storageAfter);
    }

    [Fact]
    public async Task Upload_EncryptedPdf_Returns422_AndDoesNotDeductQuota()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var quotaBeforeResp = await client.GetFromJsonAsync<JsonElement>("/api/v1/workspace/quotas");
        var storageBefore = quotaBeforeResp.GetProperty("storageBytesUsed").GetInt64();

        var encryptedPath = Path.Combine(GetRepoRoot(), "test-fixtures", "encrypted-password.pdf");
        Assert.True(File.Exists(encryptedPath));
        var fileBytes = await File.ReadAllBytesAsync(encryptedPath);

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "encrypted-password.pdf");

        var resp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);

        var quotaAfterResp = await client.GetFromJsonAsync<JsonElement>("/api/v1/workspace/quotas");
        var storageAfter = quotaAfterResp.GetProperty("storageBytesUsed").GetInt64();
        Assert.Equal(storageBefore, storageAfter);
    }

    [Fact]
    public async Task CreateAnnotation_OnDocumentPage_SucceedsWithExclusiveTarget_AndEnforcesValidation()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        // 1. Upload small 3-page PDF
        var fixturePath = Path.Combine(GetRepoRoot(), "test-fixtures", "small-3p.pdf");
        var fileBytes = await File.ReadAllBytesAsync(fixturePath);

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "small-3p.pdf");

        var uploadResp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);
        var docJson = await uploadResp.Content.ReadFromJsonAsync<JsonElement>();
        var docId = docJson.GetProperty("id").GetGuid();

        // 2. Create valid Document Page annotation on page 2
        var validAnnReq = new CreateAnnotationRequest(
            LessonId: null,
            BlockId: null,
            DocumentId: docId,
            PageNumber: 2,
            BoundingBoxesJson: "[{\"x\":0.1,\"y\":0.2,\"width\":0.3,\"height\":0.05}]",
            ExactQuote: "Text on page 2",
            PrefixQuote: "Before ",
            SuffixQuote: " After",
            StartOffset: 0,
            EndOffset: 15,
            ColorKey: "yellow",
            SemanticLabel: "Khái niệm cốt lõi",
            NoteComment: "Ghi chú trang 2"
        );

        var annResp = await client.PostAsJsonAsync("/api/v1/annotations", validAnnReq);
        Assert.Equal(HttpStatusCode.OK, annResp.StatusCode);

        var annJson = await annResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(docId, annJson.GetProperty("documentId").GetGuid());
        Assert.Equal(2, annJson.GetProperty("pageNumber").GetInt32());
        Assert.Equal(JsonValueKind.Null, annJson.GetProperty("lessonId").ValueKind);
        Assert.Equal(JsonValueKind.Null, annJson.GetProperty("blockId").ValueKind);

        // 3. Query annotations by document and page number
        var getAnnResp = await client.GetAsync($"/api/v1/annotations?documentId={docId}&pageNumber=2");
        Assert.Equal(HttpStatusCode.OK, getAnnResp.StatusCode);
        var annList = await getAnnResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(annList.GetArrayLength() >= 1);

        // 4. Violation 1: Both LessonId AND DocumentId supplied -> Returns 400 Bad Request
        var dualTargetReq = new CreateAnnotationRequest(
            LessonId: Guid.NewGuid(),
            BlockId: Guid.NewGuid(),
            DocumentId: docId,
            PageNumber: 2,
            BoundingBoxesJson: null,
            ExactQuote: "Dual target test",
            PrefixQuote: null,
            SuffixQuote: null,
            StartOffset: 0,
            EndOffset: 10,
            ColorKey: "yellow",
            SemanticLabel: null,
            NoteComment: null
        );
        var dualResp = await client.PostAsJsonAsync("/api/v1/annotations", dualTargetReq);
        Assert.Equal(HttpStatusCode.BadRequest, dualResp.StatusCode);

        // 5. Violation 2: Neither LessonId nor DocumentId supplied -> Returns 400 Bad Request
        var noTargetReq = new CreateAnnotationRequest(
            LessonId: null,
            BlockId: null,
            DocumentId: null,
            PageNumber: null,
            BoundingBoxesJson: null,
            ExactQuote: "No target test",
            PrefixQuote: null,
            SuffixQuote: null,
            StartOffset: 0,
            EndOffset: 10,
            ColorKey: "yellow",
            SemanticLabel: null,
            NoteComment: null
        );
        var noTargetResp = await client.PostAsJsonAsync("/api/v1/annotations", noTargetReq);
        Assert.Equal(HttpStatusCode.BadRequest, noTargetResp.StatusCode);

        // 6. Violation 3: PageNumber exceeds document total pages (999 > 3) -> Returns 400 Bad Request
        var pageOutOfRangeReq = new CreateAnnotationRequest(
            LessonId: null,
            BlockId: null,
            DocumentId: docId,
            PageNumber: 999,
            BoundingBoxesJson: null,
            ExactQuote: "Page out of range",
            PrefixQuote: null,
            SuffixQuote: null,
            StartOffset: 0,
            EndOffset: 10,
            ColorKey: "yellow",
            SemanticLabel: null,
            NoteComment: null
        );
        var outOfRangeResp = await client.PostAsJsonAsync("/api/v1/annotations", pageOutOfRangeReq);
        Assert.Equal(HttpStatusCode.BadRequest, outOfRangeResp.StatusCode);
    }
}
