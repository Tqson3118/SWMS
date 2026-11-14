using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Swms.Api.Infrastructure.Database;
using Swms.Api.Features;
using Xunit;

namespace Swms.Tests;

public class CourseCreatorStudioTests
{
    private static WebApplicationFactory<Program> CreateIsolatedFactory()
    {
        var dbName = $"SwmsCourseStudioTestDb_{Guid.NewGuid():N}";
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Database:InMemoryName", dbName);
            });
    }

    [Fact]
    public async Task Test1_DownloadTemplateZip_ContainsValidSwmFiles()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var res = await client.GetAsync("/api/v1/courses/template-zip");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/zip", res.Content.Headers.ContentType?.MediaType);

        var zipBytes = await res.Content.ReadAsByteArrayAsync();
        Assert.True(zipBytes.Length > 0);

        using var ms = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);

        var entryNames = archive.Entries.Select(e => e.FullName.Replace('\\', '/')).ToList();
        Assert.Contains("manifest.yaml", entryNames);
        Assert.Contains("lessons/01_bai_hoc_mau.md", entryNames);
        Assert.Contains("flashcards/deck-sample.yaml", entryNames);
        Assert.Contains("quizzes/quiz-sample.md", entryNames);
    }

    [Fact]
    public async Task Test2_ImportSwmZip_CreatesCourseWithLessonsFlashcardsQuizAndNotebook()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // 1. Download template zip
        var templateRes = await client.GetAsync("/api/v1/courses/template-zip");
        Assert.Equal(HttpStatusCode.OK, templateRes.StatusCode);
        var zipBytes = await templateRes.Content.ReadAsByteArrayAsync();

        // 2. Upload zip to import-zip endpoint
        using var form = new MultipartFormDataContent();
        var zipContent = new ByteArrayContent(zipBytes);
        zipContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(zipContent, "file", "swm-template.zip");

        var importRes = await client.PostAsync("/api/v1/courses/import-zip", form);
        var resBody = await importRes.Content.ReadAsStringAsync();
        Assert.True(importRes.IsSuccessStatusCode, $"Import failed with status {importRes.StatusCode}: {resBody}");
        Assert.Equal(HttpStatusCode.OK, importRes.StatusCode);

        var json = await importRes.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("courseId", out var cidProp) || root.TryGetProperty("id", out cidProp));
        var courseId = cidProp.GetString();
        Assert.False(string.IsNullOrWhiteSpace(courseId));

        Assert.True(root.TryGetProperty("title", out var titleProp));
        Assert.Contains("Khóa học Mẫu", titleProp.GetString());

        Assert.True(root.TryGetProperty("chapters", out var chArray));
        Assert.True(chArray.GetArrayLength() >= 2);

        // 3. Verify course is retrievable via GET /courses/{id}
        var getRes = await client.GetAsync($"/api/v1/courses/{courseId}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
    }

    [Fact]
    public async Task Test3_MultiFormatDocumentUpload_MarkdownAndTxt_CreatesCourse()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var mdContent = """
        # Khóa học: Nhập môn Kiến trúc Máy tính

        ## Chương 1: Bộ nhớ và CPU
        Bộ nhớ trong máy tính được tổ chức theo cấp bậc từ thanh ghi, cache L1/L2/L3, đến RAM và đĩa cứng.

        ## Chương 2: Nguyên lý Ảo hóa
        Hệ điều hành sử dụng Bảng phân trang (Page Table) và TLB (Translation Lookaside Buffer) để quản lý không gian địa chỉ ảo.
        """;

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(mdContent));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/markdown");
        form.Add(fileContent, "file", "computer-architecture.md");
        form.Add(new StringContent("Kiến trúc Máy tính"), "title");

        var uploadRes = await client.PostAsync("/api/v1/documents/upload", form);
        Assert.Equal(HttpStatusCode.OK, uploadRes.StatusCode);

        var json = await uploadRes.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("mimeType", out var mimeProp));
        Assert.Equal("text/markdown", mimeProp.GetString());

        Assert.True(root.TryGetProperty("generatedCourse", out var genCourseProp));
        Assert.True(genCourseProp.TryGetProperty("courseId", out var cidProp) || genCourseProp.TryGetProperty("id", out cidProp));
        Assert.False(string.IsNullOrWhiteSpace(cidProp.GetString()));
    }

    [Fact]
    public async Task Test4_AiWizardFlow_Clarify_Preview_Commit()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // 1. Clarify
        var clarifyRes = await client.PostAsJsonAsync("/api/v1/courses/generate-wizard/clarify", new WizardClarifyRequest("Học máy cơ bản"));
        Assert.Equal(HttpStatusCode.OK, clarifyRes.StatusCode);
        var clarifyJson = await clarifyRes.Content.ReadFromJsonAsync<WizardClarifyResponse>();
        Assert.NotNull(clarifyJson);
        Assert.Equal(3, clarifyJson.Questions.Count);

        // 2. Preview
        var answers = new Dictionary<string, string>
        {
            ["level"] = "Mới bắt đầu",
            ["duration"] = "Tiêu chuẩn toàn diện",
            ["focus"] = "Cân bằng lý thuyết và thực hành"
        };
        var previewRes = await client.PostAsJsonAsync("/api/v1/courses/generate-wizard/preview", new WizardPreviewRequest("Học máy cơ bản", answers));
        Assert.Equal(HttpStatusCode.OK, previewRes.StatusCode);
        var previewJson = await previewRes.Content.ReadFromJsonAsync<WizardPreviewResponse>();
        Assert.NotNull(previewJson);
        Assert.NotEmpty(previewJson.Chapters);
        Assert.NotEmpty(previewJson.Chapters[0].Lessons);

        // 3. Commit
        var commitReq = new WizardCommitRequest(
            previewJson.Topic,
            previewJson.CourseTitle,
            previewJson.Description,
            previewJson.Chapters
        );
        var commitRes = await client.PostAsJsonAsync("/api/v1/courses/generate-wizard/commit", commitReq);
        Assert.Equal(HttpStatusCode.OK, commitRes.StatusCode);

        var commitDoc = JsonDocument.Parse(await commitRes.Content.ReadAsStringAsync());
        Assert.True(commitDoc.RootElement.TryGetProperty("courseId", out var cidProp) || commitDoc.RootElement.TryGetProperty("id", out cidProp));
        var courseId = cidProp.GetString();
        Assert.False(string.IsNullOrWhiteSpace(courseId));
    }

    [Fact]
    public async Task Test5_InPlaceEditing_UpdateCourseChapterLessonAndDeletion()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // 1. Create a course via template import
        var templateRes = await client.GetAsync("/api/v1/courses/template-zip");
        var zipBytes = await templateRes.Content.ReadAsByteArrayAsync();

        using var form = new MultipartFormDataContent();
        var zipContent = new ByteArrayContent(zipBytes);
        zipContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(zipContent, "file", "swm-template.zip");

        var importRes = await client.PostAsync("/api/v1/courses/import-zip", form);
        var resBody = await importRes.Content.ReadAsStringAsync();
        Assert.True(importRes.IsSuccessStatusCode, $"Import failed with status {importRes.StatusCode}: {resBody}");
        Assert.Equal(HttpStatusCode.OK, importRes.StatusCode);

        var courseJson = JsonDocument.Parse(await importRes.Content.ReadAsStringAsync());
        var courseId = courseJson.RootElement.GetProperty("courseId").GetString();
        var chapter0 = courseJson.RootElement.GetProperty("chapters")[0];
        var chapterId = chapter0.GetProperty("id").GetString();
        var lesson0 = chapter0.GetProperty("lessons")[0];
        var lessonId = lesson0.GetProperty("id").GetString();

        // 2. Update Course
        var updateCourseRes = await client.PutAsJsonAsync($"/api/v1/courses/{courseId}", new UpdateCourseRequest(
            "Khóa học Đã Đổi Tên",
            "Mô tả cập nhật mới",
            "personal",
            true
        ));
        Assert.Equal(HttpStatusCode.OK, updateCourseRes.StatusCode);

        // 3. Update Chapter
        var updateChapterRes = await client.PutAsJsonAsync($"/api/v1/chapters/{chapterId}", new UpdateChapterRequest(
            "Chương 1 Đổi Tên",
            2
        ));
        Assert.Equal(HttpStatusCode.OK, updateChapterRes.StatusCode);

        // 4. Update Lesson
        var updateLessonRes = await client.PutAsJsonAsync($"/api/v1/lessons/{lessonId}", new UpdateLessonRequest(
            "Bài 1 Đổi Tên",
            5,
            JsonSerializer.Serialize(new { type = "doc", content = new[] { new { type = "paragraph", content = new[] { new { type = "text", text = "Nội dung mới" } } } } }),
            "Nội dung mới",
            "Chỉnh sửa thử nghiệm"
        ));
        Assert.Equal(HttpStatusCode.OK, updateLessonRes.StatusCode);

        // 5. Delete Lesson
        var delLessonRes = await client.DeleteAsync($"/api/v1/lessons/{lessonId}");
        Assert.Equal(HttpStatusCode.OK, delLessonRes.StatusCode);

        // 6. Delete Chapter
        var delChapterRes = await client.DeleteAsync($"/api/v1/chapters/{chapterId}");
        Assert.Equal(HttpStatusCode.OK, delChapterRes.StatusCode);

        // 7. Delete Course
        var delCourseRes = await client.DeleteAsync($"/api/v1/courses/{courseId}");
        Assert.Equal(HttpStatusCode.OK, delCourseRes.StatusCode);
    }

    [Fact]
    public async Task Test6_ImportPersonalFinanceCourse_LoadsRealMarkdownAndQuizzes()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var pfPath = @"D:\03_Study\Studxy\document\PersonalFinance";
        if (!Directory.Exists(pfPath))
        {
            return; // Skip if document folder not mounted
        }

        // Create a zip with week 1 documents
        using var ms = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
        {
            var manifestFile = Path.Combine(pfPath, "manifest.yaml");
            if (File.Exists(manifestFile))
            {
                archive.CreateEntryFromFile(manifestFile, "manifest.yaml");
            }

            var noteFile = Path.Combine(pfPath, "notes", "module-1", "week-01-money-psychology.md");
            if (File.Exists(noteFile))
            {
                archive.CreateEntryFromFile(noteFile, "notes/module-1/week-01-money-psychology.md");
            }

            var fcFile = Path.Combine(pfPath, "flashcards", "week-01.yaml");
            if (File.Exists(fcFile))
            {
                archive.CreateEntryFromFile(fcFile, "flashcards/week-01.yaml");
            }

            var qFile = Path.Combine(pfPath, "assessments", "quizzes", "week-01-quiz.md");
            if (File.Exists(qFile))
            {
                archive.CreateEntryFromFile(qFile, "assessments/quizzes/week-01-quiz.md");
            }

            var akFile = Path.Combine(pfPath, "assessments", "answer-keys", "week-01-answers.md");
            if (File.Exists(akFile))
            {
                archive.CreateEntryFromFile(akFile, "assessments/answer-keys/week-01-answers.md");
            }
        }

        ms.Position = 0;
        using var form = new MultipartFormDataContent();
        var zipContent = new ByteArrayContent(ms.ToArray());
        zipContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
        form.Add(zipContent, "file", "personal-finance-w01.zip");

        var response = await client.PostAsync("/api/v1/courses/import-zip", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("courseId", out var cidProp) || root.TryGetProperty("id", out cidProp));
        var courseId = Guid.Parse(cidProp.GetString()!);

        // Verify in DB
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();

        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        Assert.NotNull(course);
        Assert.Contains("Tài Chính Cá Nhân", course.Title, StringComparison.OrdinalIgnoreCase);

        var chapters = await db.Chapters.Where(ch => ch.CourseId == courseId).ToListAsync();
        Assert.NotEmpty(chapters);

        var lessons = await db.Lessons.Where(l => chapters.Select(c => c.Id).Contains(l.ChapterId)).ToListAsync();
        Assert.NotEmpty(lessons);

        var lesson = lessons.FirstOrDefault();
        Assert.NotNull(lesson);
        Assert.Contains("Tâm Lý Tài Chính", lesson.Title);
        // Ensure real markdown was imported, not placeholder
        Assert.Contains("Homo Economicus", lesson.PlainText);
        Assert.DoesNotContain("Nội dung bài giảng đang được hoàn thiện", lesson.PlainText);

        // Verify flashcards
        var cards = await db.Flashcards.Where(f => f.SourceTitleSnapshot == course.Title).ToListAsync();
        Assert.NotEmpty(cards);
        Assert.Contains(cards, c => c.SourceQuoteSnapshot != null && c.SourceQuoteSnapshot.Contains("Tài chính hành vi"));

        // Verify quiz
        var quiz = await db.PracticeQuizzes.FirstOrDefaultAsync(q => q.LessonId == lesson.Id);
        Assert.NotNull(quiz);
        var quizItems = await db.PracticeQuizItems.Where(qi => qi.QuizId == quiz.Id).OrderBy(qi => qi.CreatedAt).ToListAsync();
        Assert.Equal(10, quizItems.Count);
        // Question 1 solution should be "C" according to answer key
        Assert.Equal("C", quizItems[0].ClozeSolution);
    }

    [Fact]
    public async Task Test7_CourseStudioAuthoring_CreateChapterAndLesson_UpdateWithMarkdownAndVideo_PublishToggle()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // 1. Create course via template import
        var templateRes = await client.GetAsync("/api/v1/courses/template-zip");
        var zipBytes = await templateRes.Content.ReadAsByteArrayAsync();

        using var form = new MultipartFormDataContent();
        var zipContent = new ByteArrayContent(zipBytes);
        zipContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(zipContent, "file", "swm-template.zip");

        var importRes = await client.PostAsync("/api/v1/courses/import-zip", form);
        Assert.Equal(HttpStatusCode.OK, importRes.StatusCode);
        var courseDoc = JsonDocument.Parse(await importRes.Content.ReadAsStringAsync());
        var courseId = courseDoc.RootElement.GetProperty("courseId").GetString()!;

        // 2. Add new chapter via POST /api/v1/courses/{id}/chapters
        var addChRes = await client.PostAsJsonAsync($"/api/v1/courses/{courseId}/chapters", new CreateChapterRequest(
            "Chương 3: Kiến trúc Microservices & Phân tán",
            3
        ));
        Assert.Equal(HttpStatusCode.OK, addChRes.StatusCode);
        var chDoc = JsonDocument.Parse(await addChRes.Content.ReadAsStringAsync());
        var chapterId = chDoc.RootElement.GetProperty("id").GetString()!;
        Assert.Equal("Chương 3: Kiến trúc Microservices & Phân tán", chDoc.RootElement.GetProperty("title").GetString());
        Assert.Equal(3, chDoc.RootElement.GetProperty("sortOrder").GetInt32());

        // 3. Add new lesson via POST /api/v1/chapters/{id}/lessons
        var mdContent = """
        # Bài học: Microservices và Giao tiếp Bất đồng bộ

        ## 1. Giới thiệu
        Kiến trúc hướng dịch vụ microservices chia nhỏ hệ thống thành các module độc lập.

        ## 2. Giao tiếp Event-Driven
        Sử dụng Message Broker như RabbitMQ hoặc Kafka.
        """;

        var addLesRes = await client.PostAsJsonAsync($"/api/v1/chapters/{chapterId}/lessons", new CreateLessonRequest(
            "Bài 4: Giới thiệu Microservices",
            mdContent,
            "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            15,
            1
        ));
        Assert.Equal(HttpStatusCode.OK, addLesRes.StatusCode);
        var lesDoc = JsonDocument.Parse(await addLesRes.Content.ReadAsStringAsync());
        var lessonId = lesDoc.RootElement.GetProperty("id").GetString()!;
        Assert.Equal("Bài 4: Giới thiệu Microservices", lesDoc.RootElement.GetProperty("title").GetString());
        Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ", lesDoc.RootElement.GetProperty("videoUrl").GetString());
        Assert.Equal(15, lesDoc.RootElement.GetProperty("estimatedMinutes").GetInt32());

        // 4. Update lesson via PUT /api/v1/lessons/{id} with markdown and videoUrl
        var updatedMd = """
        # Bài học: Microservices Nâng cao

        Cập nhật bổ sung nội dung thực hành Docker & Kubernetes.
        """;

        var updateLesRes = await client.PutAsJsonAsync($"/api/v1/lessons/{lessonId}", new UpdateLessonRequest(
            Title: "Bài 4: Microservices Nâng cao",
            SortOrder: 2,
            ContentMarkdown: updatedMd,
            VideoUrl: "https://vimeo.com/987654321",
            EstimatedMinutes: 25
        ));
        Assert.Equal(HttpStatusCode.OK, updateLesRes.StatusCode);

        // Verify via GET /api/v1/lessons/{id}
        var getLesRes = await client.GetAsync($"/api/v1/lessons/{lessonId}");
        Assert.Equal(HttpStatusCode.OK, getLesRes.StatusCode);
        var getLesDoc = JsonDocument.Parse(await getLesRes.Content.ReadAsStringAsync());
        var rootLes = getLesDoc.RootElement;
        Assert.Equal("Bài 4: Microservices Nâng cao", rootLes.GetProperty("title").GetString());
        Assert.Equal("https://vimeo.com/987654321", rootLes.GetProperty("videoUrl").GetString());
        Assert.Equal(25, rootLes.GetProperty("estimatedMinutes").GetInt32());
        Assert.Contains("Docker & Kubernetes", rootLes.GetProperty("plainText").GetString());

        // 5. Toggle publish via POST /api/v1/courses/{id}/publish
        // Default template course is public (isPublished = true) -> toggle should make it private
        var pubToggleRes1 = await client.PostAsync($"/api/v1/courses/{courseId}/publish", null);
        Assert.Equal(HttpStatusCode.OK, pubToggleRes1.StatusCode);
        var pubDoc1 = JsonDocument.Parse(await pubToggleRes1.Content.ReadAsStringAsync());
        Assert.Equal("private", pubDoc1.RootElement.GetProperty("visibility").GetString());
        Assert.False(pubDoc1.RootElement.GetProperty("isPublished").GetBoolean());

        // Toggle again -> should make it public
        var pubToggleRes2 = await client.PostAsync($"/api/v1/courses/{courseId}/publish", null);
        Assert.Equal(HttpStatusCode.OK, pubToggleRes2.StatusCode);
        var pubDoc2 = JsonDocument.Parse(await pubToggleRes2.Content.ReadAsStringAsync());
        Assert.Equal("public", pubDoc2.RootElement.GetProperty("visibility").GetString());
        Assert.True(pubDoc2.RootElement.GetProperty("isPublished").GetBoolean());

        // Set explicitly via body
        var pubExplicitRes = await client.PostAsJsonAsync($"/api/v1/courses/{courseId}/publish", new PublishCourseRequest("private", false));
        Assert.Equal(HttpStatusCode.OK, pubExplicitRes.StatusCode);
        var pubDoc3 = JsonDocument.Parse(await pubExplicitRes.Content.ReadAsStringAsync());
        Assert.Equal("private", pubDoc3.RootElement.GetProperty("visibility").GetString());
    }

    [Fact]
    public async Task Test8_LessonProgressTracking_Complete_Unmark_CourseProgressCalculation()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // 1. Create course via template import
        var templateRes = await client.GetAsync("/api/v1/courses/template-zip");
        var zipBytes = await templateRes.Content.ReadAsByteArrayAsync();

        using var form = new MultipartFormDataContent();
        var zipContent = new ByteArrayContent(zipBytes);
        zipContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(zipContent, "file", "swm-template.zip");

        var importRes = await client.PostAsync("/api/v1/courses/import-zip", form);
        var courseDoc = JsonDocument.Parse(await importRes.Content.ReadAsStringAsync());
        var courseId = courseDoc.RootElement.GetProperty("courseId").GetString()!;

        // Get lessons from course summary
        var ch0 = courseDoc.RootElement.GetProperty("chapters")[0];
        var lesson0 = ch0.GetProperty("lessons")[0];
        var lesson0Id = lesson0.GetProperty("id").GetString()!;
        var lesson1 = ch0.GetProperty("lessons")[1];
        var lesson1Id = lesson1.GetProperty("id").GetString()!;

        // 2. Initial progress should be 0
        var progRes0 = await client.GetAsync($"/api/v1/courses/{courseId}/progress");
        Assert.Equal(HttpStatusCode.OK, progRes0.StatusCode);
        var progDoc0 = JsonDocument.Parse(await progRes0.Content.ReadAsStringAsync());
        var totalLessons = progDoc0.RootElement.GetProperty("totalLessons").GetInt32();
        Assert.True(totalLessons >= 3);
        Assert.Equal(0, progDoc0.RootElement.GetProperty("completedLessons").GetInt32());
        Assert.Equal(0, progDoc0.RootElement.GetProperty("percentCompleted").GetInt32());
        Assert.Empty(progDoc0.RootElement.GetProperty("completedLessonIds").EnumerateArray());

        // 3. Mark lesson 0 complete via POST /api/v1/lessons/{id}/complete
        var completeRes0 = await client.PostAsync($"/api/v1/lessons/{lesson0Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completeRes0.StatusCode);
        var compDoc0 = JsonDocument.Parse(await completeRes0.Content.ReadAsStringAsync());
        Assert.True(compDoc0.RootElement.GetProperty("isCompleted").GetBoolean());

        // Verify progress updated
        var progRes1 = await client.GetAsync($"/api/v1/courses/{courseId}/progress");
        var progDoc1 = JsonDocument.Parse(await progRes1.Content.ReadAsStringAsync());
        Assert.Equal(1, progDoc1.RootElement.GetProperty("completedLessons").GetInt32());
        var compIds1 = progDoc1.RootElement.GetProperty("completedLessonIds").EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.Contains(lesson0Id, compIds1);
        Assert.True(progDoc1.RootElement.GetProperty("percentCompleted").GetInt32() > 0);

        // 4. Mark lesson 1 complete
        var completeRes1 = await client.PostAsync($"/api/v1/lessons/{lesson1Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completeRes1.StatusCode);

        var progRes2 = await client.GetAsync($"/api/v1/courses/{courseId}/progress");
        var progDoc2 = JsonDocument.Parse(await progRes2.Content.ReadAsStringAsync());
        Assert.Equal(2, progDoc2.RootElement.GetProperty("completedLessons").GetInt32());
        var compIds2 = progDoc2.RootElement.GetProperty("completedLessonIds").EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.Contains(lesson0Id, compIds2);
        Assert.Contains(lesson1Id, compIds2);

        // 5. Unmark lesson 0 via DELETE /api/v1/lessons/{id}/complete
        var unmarkRes0 = await client.DeleteAsync($"/api/v1/lessons/{lesson0Id}/complete");
        Assert.Equal(HttpStatusCode.OK, unmarkRes0.StatusCode);
        var unmarkDoc0 = JsonDocument.Parse(await unmarkRes0.Content.ReadAsStringAsync());
        Assert.False(unmarkDoc0.RootElement.GetProperty("isCompleted").GetBoolean());

        // Verify progress decreased
        var progRes3 = await client.GetAsync($"/api/v1/courses/{courseId}/progress");
        var progDoc3 = JsonDocument.Parse(await progRes3.Content.ReadAsStringAsync());
        Assert.Equal(1, progDoc3.RootElement.GetProperty("completedLessons").GetInt32());
        var compIds3 = progDoc3.RootElement.GetProperty("completedLessonIds").EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.DoesNotContain(lesson0Id, compIds3);
        Assert.Contains(lesson1Id, compIds3);
    }

    [Fact]
    public async Task Test9_CourseExportZip_PackagesValidSwmZip_RoundtripImportable()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        // 1. Create a course via template import
        var templateRes = await client.GetAsync("/api/v1/courses/template-zip");
        var origZipBytes = await templateRes.Content.ReadAsByteArrayAsync();

        using var form = new MultipartFormDataContent();
        var zipContent = new ByteArrayContent(origZipBytes);
        zipContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(zipContent, "file", "swm-template.zip");

        var importRes = await client.PostAsync("/api/v1/courses/import-zip", form);
        Assert.Equal(HttpStatusCode.OK, importRes.StatusCode);
        var courseDoc = JsonDocument.Parse(await importRes.Content.ReadAsStringAsync());
        var courseId = courseDoc.RootElement.GetProperty("courseId").GetString()!;

        // 2. Call GET /api/v1/courses/{id}/export-zip
        var exportRes = await client.GetAsync($"/api/v1/courses/{courseId}/export-zip");
        Assert.Equal(HttpStatusCode.OK, exportRes.StatusCode);
        Assert.Equal("application/zip", exportRes.Content.Headers.ContentType?.MediaType);

        var exportedZipBytes = await exportRes.Content.ReadAsByteArrayAsync();
        Assert.True(exportedZipBytes.Length > 0);

        // 3. Inspect zip contents
        using var ms = new MemoryStream(exportedZipBytes);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);

        var entryNames = archive.Entries.Select(e => e.FullName.Replace('\\', '/')).ToList();
        Assert.Contains("manifest.yaml", entryNames);
        Assert.Contains(entryNames, e => e.StartsWith("lessons/") && e.EndsWith(".md"));
        Assert.Contains(entryNames, e => e.StartsWith("flashcards/") && e.EndsWith(".yaml"));
        Assert.Contains(entryNames, e => e.StartsWith("quizzes/") && e.EndsWith(".md"));

        // Verify manifest content
        var manifestEntry = archive.GetEntry("manifest.yaml")!;
        using (var reader = new StreamReader(manifestEntry.Open(), Encoding.UTF8))
        {
            var yaml = await reader.ReadToEndAsync();
            Assert.Contains("schema_version: 2.0.0", yaml);
            Assert.Contains("chapters:", yaml);
            Assert.Contains("lessons:", yaml);
        }

        // 4. Verify round-trip importability: upload exported zip to /api/v1/courses/import-zip
        using var roundtripForm = new MultipartFormDataContent();
        var reimportContent = new ByteArrayContent(exportedZipBytes);
        reimportContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        roundtripForm.Add(reimportContent, "file", "exported-course.zip");

        var reimportRes = await client.PostAsync("/api/v1/courses/import-zip", roundtripForm);
        Assert.True(reimportRes.IsSuccessStatusCode, $"Re-import failed with {reimportRes.StatusCode}: {await reimportRes.Content.ReadAsStringAsync()}");

        var reimportDoc = JsonDocument.Parse(await reimportRes.Content.ReadAsStringAsync());
        Assert.True(reimportDoc.RootElement.TryGetProperty("courseId", out var newCidProp) || reimportDoc.RootElement.TryGetProperty("id", out newCidProp));
        Assert.False(string.IsNullOrWhiteSpace(newCidProp.GetString()));
    }

    [Fact]
    public async Task Test10_ExportZip_EnforcesIdorProtection_ForPrivateCourses()
    {
        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        // 1. Create a private course owned by User A
        using var form = new MultipartFormDataContent();
        var templateRes = await client.GetAsync("/api/v1/courses/template-zip");
        var origZipBytes = await templateRes.Content.ReadAsByteArrayAsync();
        var zipContent = new ByteArrayContent(origZipBytes);
        zipContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(zipContent, "file", "swm-template.zip");

        client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        client.DefaultRequestHeaders.Add("X-Test-User-Id", userA.ToString());

        var importRes = await client.PostAsync("/api/v1/courses/import-zip", form);
        Assert.Equal(HttpStatusCode.OK, importRes.StatusCode);
        var courseDoc = JsonDocument.Parse(await importRes.Content.ReadAsStringAsync());
        var courseId = courseDoc.RootElement.GetProperty("courseId").GetString()!;

        // Ensure course is private
        var pubRes = await client.PostAsJsonAsync($"/api/v1/courses/{courseId}/publish", new { visibility = "private", isPublished = false });
        Assert.Equal(HttpStatusCode.OK, pubRes.StatusCode);

        // 2. User A (owner) can export zip
        var ownerExportRes = await client.GetAsync($"/api/v1/courses/{courseId}/export-zip");
        Assert.Equal(HttpStatusCode.OK, ownerExportRes.StatusCode);

        // 3. User B (unauthorized) gets 403 Forbidden
        client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        client.DefaultRequestHeaders.Add("X-Test-User-Id", userB.ToString());

        var attackerExportRes = await client.GetAsync($"/api/v1/courses/{courseId}/export-zip");
        Assert.Equal(HttpStatusCode.Forbidden, attackerExportRes.StatusCode);
    }

    [Fact]
    public async Task Test11_SampleSwmRoadmapZip_ImportsCleanlyWithChaptersLessonsFlashcardsAndQuiz()
    {
        var zipPath = Path.GetFullPath(Path.Combine(
            DocumentCourseSyncService.DefaultDocumentRootPath,
            "..",
            "examples",
            "sample-swm-roadmap.zip"));
        Assert.True(File.Exists(zipPath), $"Missing sample ZIP at {zipPath}");

        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();
        var zipBytes = await File.ReadAllBytesAsync(zipPath);
        var zipContent = new ByteArrayContent(zipBytes);
        zipContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(zipContent, "file", "sample-swm-roadmap.zip");

        var importRes = await client.PostAsync("/api/v1/courses/import-zip", form);
        var body = await importRes.Content.ReadAsStringAsync();
        Assert.True(importRes.IsSuccessStatusCode, $"Sample roadmap import failed: {importRes.StatusCode} - {body}");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var courseId = root.GetProperty("courseId").GetString()!;
        Assert.Contains("Lộ trình Mẫu Chuẩn SWM", root.GetProperty("title").GetString());
        Assert.Equal(2, root.GetProperty("chapterCount").GetInt32());
        Assert.Equal(4, root.GetProperty("lessonCount").GetInt32());
        Assert.Equal(6, root.GetProperty("flashcardCount").GetInt32());

        var courseDetail = await client.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}");
        var firstLessonId = courseDetail.GetProperty("chapters")[0].GetProperty("lessons")[0].GetProperty("id").GetGuid();
        var lessonDetail = await client.GetFromJsonAsync<JsonElement>($"/api/v1/lessons/{firstLessonId}");
        var md = lessonDetail.GetProperty("contentMarkdown").GetString() ?? "";
        Assert.Contains("| Chặng | Chủ đề trọng tâm |", md);
        Assert.Contains("<details>", md);
    }

    [Fact]
    public async Task Test12_CourseProgressAndWorkspaceSearch_ReturnAccurateProgressAndCourseId()
    {
        var zipPath = Path.GetFullPath(Path.Combine(
            DocumentCourseSyncService.DefaultDocumentRootPath,
            "..",
            "examples",
            "sample-swm-roadmap.zip"));
        Assert.True(File.Exists(zipPath), $"Missing sample ZIP at {zipPath}");

        await using var factory = CreateIsolatedFactory();
        using var client = factory.CreateClient();

        using var form = new MultipartFormDataContent();
        var zipContent = new ByteArrayContent(await File.ReadAllBytesAsync(zipPath));
        zipContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(zipContent, "file", "sample-swm-roadmap.zip");

        var importRes = await client.PostAsync("/api/v1/courses/import-zip", form);
        Assert.Equal(HttpStatusCode.OK, importRes.StatusCode);
        var importJson = await importRes.Content.ReadFromJsonAsync<JsonElement>();
        var courseId = importJson.GetProperty("courseId").GetString()!;

        var detailBefore = await client.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}");
        var firstLessonId = detailBefore.GetProperty("chapters")[0].GetProperty("lessons")[0].GetProperty("id").GetString()!;
        Assert.Equal(4, detailBefore.GetProperty("totalLessons").GetInt32());
        Assert.Equal(0, detailBefore.GetProperty("completedLessons").GetInt32());
        Assert.Equal(0, detailBefore.GetProperty("progressPercent").GetInt32());

        var completeRes = await client.PostAsync($"/api/v1/lessons/{firstLessonId}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completeRes.StatusCode);

        var detailAfter = await client.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}");
        Assert.Equal(4, detailAfter.GetProperty("totalLessons").GetInt32());
        Assert.Equal(1, detailAfter.GetProperty("completedLessons").GetInt32());
        Assert.Equal(25, detailAfter.GetProperty("progressPercent").GetInt32());

        var dashJson = await client.GetFromJsonAsync<JsonElement>("/api/v1/workspace/dashboard");
        var matchedCourse = dashJson.GetProperty("courses").EnumerateArray()
            .First(c => c.GetProperty("id").GetString() == courseId);
        Assert.Equal(25, matchedCourse.GetProperty("progressPercent").GetInt32());

        var searchJson = await client.GetFromJsonAsync<JsonElement>("/api/v1/workspace/search?q=Chặng");
        var hits = searchJson.GetProperty("results").EnumerateArray().ToList();
        var lessonHit = hits.FirstOrDefault(h =>
            h.GetProperty("type").GetString() == "lesson" &&
            h.GetProperty("courseId").GetString() == courseId);
        Assert.NotEqual(JsonValueKind.Undefined, lessonHit.ValueKind);
        Assert.Equal(courseId, lessonHit.GetProperty("courseId").GetString());
        Assert.Contains("Lộ trình Mẫu Chuẩn SWM", lessonHit.GetProperty("subtitle").GetString());
    }
}
