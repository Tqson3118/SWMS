using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Swms.Api.Features;
using Xunit;

namespace Swms.Tests;

public class CourseGenerationTests
{
    private static WebApplicationFactory<Program> CreateIsolatedFactory()
    {
        var dbName = $"SwmsCourseGenTestDb_{Guid.NewGuid():N}";
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Database:InMemoryName", dbName);
            });
    }

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
    public async Task Test1_UploadMultipartPdf_AutoGeneratesFullCourseSyllabusLessonsFlashcardsQuizAndNotebook_GroundedInPdfContent()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var filePath = Path.Combine(GetRepoRoot(), "test", "Giáo trình Pháp luật đại cương_ebook.pdf");
        Assert.True(File.Exists(filePath));

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "Giáo trình Pháp luật đại cương_ebook.pdf");
        form.Add(new StringContent("Giáo trình Pháp luật đại cương"), "title");

        var uploadResp = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);

        var uploadJson = await uploadResp.Content.ReadFromJsonAsync<JsonElement>();
        var docId = uploadJson.GetProperty("id").GetGuid();
        Assert.Equal(298, uploadJson.GetProperty("totalPages").GetInt32());

        // Verify generatedCourse payload on upload response
        Assert.True(uploadJson.TryGetProperty("generatedCourse", out var genCourse), "Expected generatedCourse property on upload response.");
        var courseId = genCourse.GetProperty("courseId").GetGuid();
        var firstLessonId = genCourse.GetProperty("firstLessonId").GetGuid();
        var tagId = genCourse.GetProperty("tagId").GetGuid();
        var quizId = genCourse.GetProperty("quizId").GetGuid();
        var notebookId = genCourse.GetProperty("notebookId").GetGuid();

        Assert.Equal(docId, genCourse.GetProperty("sourceDocumentId").GetGuid());
        Assert.True(genCourse.GetProperty("chapterCount").GetInt32() >= 2);
        Assert.True(genCourse.GetProperty("lessonCount").GetInt32() >= 3);
        Assert.True(genCourse.GetProperty("flashcardCount").GetInt32() >= 4);

        // Verify generated lesson contains real text from the uploaded PDF (not hardcoded OS TLB text)
        var lessonResp = await client.GetAsync($"/api/v1/lessons/{firstLessonId}");
        Assert.Equal(HttpStatusCode.OK, lessonResp.StatusCode);
        var lessonJson = await lessonResp.Content.ReadFromJsonAsync<JsonElement>();
        var lessonText = lessonJson.GetProperty("plainText").GetString() ?? "";
        Assert.NotEmpty(lessonText);
        Assert.DoesNotContain("Translation Lookaside Buffer", lessonText);

        // Verify generated flashcards linked to course tag
        var fcResp = await client.GetAsync($"/api/v1/flashcards/due?tagId={tagId}&includeAll=true");
        Assert.Equal(HttpStatusCode.OK, fcResp.StatusCode);
        var fcArray = await fcResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(fcArray.GetArrayLength() >= 4);
        Assert.All(fcArray.EnumerateArray(), card =>
        {
            Assert.Equal(docId, card.GetProperty("documentId").GetGuid());
        });

        // Verify generated practice quiz (3 MCQ + 1 Essay)
        var quizResp = await client.GetAsync($"/api/v1/practice-quizzes/{quizId}");
        Assert.Equal(HttpStatusCode.OK, quizResp.StatusCode);
        var quizJson = await quizResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(4, quizJson.GetProperty("totalItems").GetInt32());
        var items = quizJson.GetProperty("items");
        Assert.Equal(4, items.GetArrayLength());
        Assert.Equal(3, items.EnumerateArray().Count(i => i.GetProperty("itemType").GetString() == "multiple_choice"));
        Assert.Equal(1, items.EnumerateArray().Count(i => i.GetProperty("itemType").GetString() == "essay"));

        // Verify generated course summary notebook
        var nbResp = await client.GetAsync($"/api/v1/notebooks/{notebookId}");
        Assert.Equal(HttpStatusCode.OK, nbResp.StatusCode);
        var nbJson = await nbResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(docId, nbJson.GetProperty("primarySourceDocumentId").GetGuid());
    }

    [Fact]
    public async Task Test2_GenerateCourseEndpoint_CoursesApi_DashboardIntegration_And_MultiTenantIsolation()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // Register User A and User B
        var regAResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"course.a.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "Course User A"));
        var regAJson = await regAResp.Content.ReadFromJsonAsync<JsonElement>();
        var userAId = regAJson.GetProperty("user").GetProperty("id").GetGuid();

        var regBResp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest($"course.b.{Guid.NewGuid():N}@studxy.vn", "Studxy@2026", "Course User B"));
        var regBJson = await regBResp.Content.ReadFromJsonAsync<JsonElement>();
        var userBId = regBJson.GetProperty("user").GetProperty("id").GetGuid();

        var customText = """
            Chương 1: Kiến trúc Vi dịch vụ (Microservices) và Phân rã Miền (DDD)
            Bài 1.1: Nguyên lý Bounded Context trong Domain-Driven Design
            Bounded Context giúp định hình ranh giới ngữ nghĩa rõ ràng giữa các dịch vụ độc lập như Đặt hàng, Thanh toán và Kho vận. Mỗi dịch vụ sở hữu cơ sở dữ liệu riêng để tránh phụ thuộc chặt ở tầng lưu trữ.

            Chương 2: Giao tiếp Bất đồng bộ, Event Sourcing và Mẫu Saga
            Bài 2.1: Event Sourcing và CQRS trong hệ thống phân tán
            Event Sourcing lưu trữ mọi thay đổi trạng thái của hệ thống dưới dạng chuỗi sự kiện bất biến (Immutable Event Log). Kết hợp với CQRS giúp tách biệt mô hình đọc (Read Model) và mô hình ghi (Write Model) nhằm tối ưu hiệu năng truy vấn.
            """;

        using var uploadReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/documents/upload")
        {
            Content = JsonContent.Create(new UploadDocumentRequest(
                Title: "Kiến trúc Microservices, DDD & Event Sourcing",
                FileSizeBytes: 8192,
                TotalPages: 4,
                MimeType: "application/pdf",
                ExtractedText: customText))
        };
        uploadReq.Headers.Add("X-Test-User-Id", userAId.ToString());
        var uploadResp = await client.SendAsync(uploadReq);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);
        var uploadJson = await uploadResp.Content.ReadFromJsonAsync<JsonElement>();
        var docAId = uploadJson.GetProperty("id").GetGuid();
        var courseAId = uploadJson.GetProperty("generatedCourse").GetProperty("courseId").GetGuid();

        // Idempotent call to POST /api/v1/documents/{id}/generate-course returns the same courseId
        using var regenReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/documents/{docAId}/generate-course");
        regenReq.Headers.Add("X-Test-User-Id", userAId.ToString());
        var regenResp = await client.SendAsync(regenReq);
        Assert.Equal(HttpStatusCode.OK, regenResp.StatusCode);
        var regenJson = await regenResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(courseAId, regenJson.GetProperty("courseId").GetGuid());

        // GET /api/v1/courses and GET /api/v1/courses/{id} for User A
        using var listCoursesReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/courses");
        listCoursesReq.Headers.Add("X-Test-User-Id", userAId.ToString());
        var listCoursesResp = await client.SendAsync(listCoursesReq);
        Assert.Equal(HttpStatusCode.OK, listCoursesResp.StatusCode);
        var coursesArr = await listCoursesResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(coursesArr.EnumerateArray(), c => c.GetProperty("courseId").GetGuid() == courseAId);

        using var getCourseReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/courses/{courseAId}");
        getCourseReq.Headers.Add("X-Test-User-Id", userAId.ToString());
        var getCourseResp = await client.SendAsync(getCourseReq);
        Assert.Equal(HttpStatusCode.OK, getCourseResp.StatusCode);

        // GET /api/v1/workspace/dashboard for User A includes courses, quizzes, and enriched lessons
        using var dashAReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/workspace/dashboard");
        dashAReq.Headers.Add("X-Test-User-Id", userAId.ToString());
        var dashAResp = await client.SendAsync(dashAReq);
        Assert.Equal(HttpStatusCode.OK, dashAResp.StatusCode);
        var dashAJson = await dashAResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(dashAJson.TryGetProperty("courses", out var dashCourses) && dashCourses.GetArrayLength() >= 1);
        Assert.True(dashAJson.TryGetProperty("quizzes", out var dashQuizzes) && dashQuizzes.GetArrayLength() >= 1);

        // Multi-tenant isolation: User B cannot generate or fetch User A's course
        using var bRegenReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/documents/{docAId}/generate-course");
        bRegenReq.Headers.Add("X-Test-User-Id", userBId.ToString());
        var bRegenResp = await client.SendAsync(bRegenReq);
        Assert.True(bRegenResp.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound);

        using var bGetCourseReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/courses/{courseAId}");
        bGetCourseReq.Headers.Add("X-Test-User-Id", userBId.ToString());
        var bGetCourseResp = await client.SendAsync(bGetCourseReq);
        Assert.True(bGetCourseResp.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound);
    }
}
