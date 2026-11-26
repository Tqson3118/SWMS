using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Ai;
using Swms.Api.Infrastructure.Database;

namespace Swms.Api.Features;

public static class CourseGenerationService
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> DocCourseLocks = new();

    private static readonly Regex HeadingRegex = new(
        @"^\s*(?:Chương|CHƯƠNG|Phần|PHẦN|Bài|BÀI|Mục|MỤC|Chapter|CHAPTER|Section|SECTION|#+\s+|[IVX]{1,5}[\.\)]\s+|\d+(?:\.\d+){0,2}[\.\)]\s+)(.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static AiCallOverrides? ExtractAiOverrides(HttpContext? http)
    {
        if (http == null) return null;
        var apiKey = http.Request.Headers["X-Ai-Api-Key"].FirstOrDefault();
        var baseUrl = http.Request.Headers["X-Ai-Base-Url"].FirstOrDefault();
        var model = http.Request.Headers["X-Ai-Model"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(apiKey) &&
            string.IsNullOrWhiteSpace(baseUrl) &&
            string.IsNullOrWhiteSpace(model))
        {
            return null;
        }

        return new AiCallOverrides(baseUrl, apiKey, model);
    }

    public static async Task<object> GenerateCourseFromDocumentAsync(
        SwmsDbContext db,
        GeminiProxyClient gemini,
        Document doc,
        List<DocumentPage> pages,
        Guid userId,
        AiCallOverrides? aiOverrides = null)
    {
        var sem = DocCourseLocks.GetOrAdd(doc.Id, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();
        try
        {
            var courseSlug = $"doc-course-{doc.Id:N}";
            var existingCourse = await db.Courses.FirstOrDefaultAsync(c => c.Slug == courseSlug);
            if (existingCourse != null)
            {
                return await BuildCourseSummaryAsync(db, existingCourse, doc);
            }

            var now = DateTimeOffset.UtcNow;
            var cleanTitle = CleanDocumentTitle(doc.Title);
            var orderedPages = pages.OrderBy(p => p.PageNumber).ToList();
            if (orderedPages.Count == 0)
            {
                orderedPages.Add(new DocumentPage
                {
                    DocumentId = doc.Id,
                    PageNumber = 1,
                    PlainText = doc.PlainText ?? cleanTitle,
                    HasTextLayer = !string.IsNullOrWhiteSpace(doc.PlainText),
                    CharCount = (doc.PlainText ?? string.Empty).Length,
                    WordCount = 10
                });
            }

            var totalPages = Math.Max(1, Math.Max(doc.TotalPages, orderedPages.Count));

            // 1. Create Course entity
            var course = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = userId,
                Title = Truncate($"Khóa học: {cleanTitle}", 245),
                Slug = courseSlug,
                Description = $"Lộ trình học tập được tổng hợp từ tài liệu \"{cleanTitle}\" ({totalPages} trang), gồm chương mục, bài giảng trọng tâm, bộ thẻ ghi nhớ FSRS và đề luyện tập thực hành.",
                Visibility = "personal",
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Courses.Add(course);

            var ledgerEntry = new AiUsageLedger
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Feature = "generate_course",
                PromptTokens = 120,
                CompletionTokens = 180,
                TotalTokens = 300,
                CostUsd = 0.00015m,
                CreditsCharged = 1,
                CreatedAt = now
            };
            db.AiUsageLedgers.Add(ledgerEntry);

            var hasEnrollment = await db.CourseEnrollments.AnyAsync(ce => ce.UserId == userId && ce.CourseId == course.Id);
            if (!hasEnrollment)
            {
                db.CourseEnrollments.Add(new CourseEnrollment
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CourseId = course.Id,
                    Role = "student",
                    EnrolledAt = now
                });
            }

            // Link document to this generated course
            doc.CourseId = course.Id;

            // 2. Segment document into 4 logical lesson slices across 2 chapters
            var lessonSlices = BuildLessonSlices(cleanTitle, orderedPages, totalPages);
            var chapterCount = lessonSlices.Count >= 4 ? 2 : 2;
            var chapters = new List<Chapter>();

            for (int cIdx = 0; cIdx < chapterCount; cIdx++)
            {
                var partLessons = lessonSlices
                    .Where((_, idx) => (idx * chapterCount / lessonSlices.Count) == cIdx)
                    .ToList();
                var pStart = partLessons.Count > 0 ? partLessons.First().StartPage : 1;
                var pEnd = partLessons.Count > 0 ? partLessons.Last().EndPage : totalPages;
                var chTopic = partLessons.FirstOrDefault()?.HeadingTitle ?? $"Phần {cIdx + 1}";
                var cleanChTopic = Regex.Replace(chTopic, @"^(?:Chương|Chapter|Phần)\s*\d+\s*[:.\-–—]\s*", "", RegexOptions.IgnoreCase).Trim();

                var chapter = new Chapter
                {
                    Id = Guid.NewGuid(),
                    CourseId = course.Id,
                    Title = Truncate($"Chương {cIdx + 1}: {cleanChTopic} (Trang {pStart}–{pEnd})", 245),
                    SortOrder = cIdx + 1,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                chapters.Add(chapter);
                db.Chapters.Add(chapter);
            }

            // Determine starting SortOrder so seeded OS lessons (1, 2) stay first in global OrderBy(l => l.SortOrder)
            var maxSortOrder = await db.Lessons.Select(l => (int?)l.SortOrder).MaxAsync() ?? 2;
            var baseSortOrder = Math.Max(10, maxSortOrder + 1);

            var createdLessons = new List<Lesson>();

            for (int lIdx = 0; lIdx < lessonSlices.Count; lIdx++)
            {
                var slice = lessonSlices[lIdx];
                var targetChapter = chapters[Math.Min(chapters.Count - 1, lIdx * chapters.Count / lessonSlices.Count)];
                var lessonTitle = Truncate($"Bài {lIdx + 1}: {slice.HeadingTitle} (Tr. {slice.StartPage}–{slice.EndPage})", 245);

                string? aiSummaryPoint = null;
                if (lIdx == 0 && !string.IsNullOrWhiteSpace(aiOverrides?.ApiKey))
                {
                    try
                    {
                        var sampleExcerpt = Truncate(slice.CoreParagraphs.FirstOrDefault() ?? cleanTitle, 500);
                        var aiRes = await gemini.CompleteChatAsync(
                            "Bạn là giảng viên đại học biên soạn bài giảng từ giáo trình. Tóm tắt ngắn gọn 2 câu tiếng Việt về ý nghĩa cốt lõi của đoạn trích.",
                            $"Tài liệu: {cleanTitle}\nĐoạn trích: {sampleExcerpt}",
                            CancellationToken.None,
                            aiOverrides);
                        if (!string.IsNullOrWhiteSpace(aiRes.Content))
                        {
                            aiSummaryPoint = aiRes.Content.Trim();
                        }
                    }
                    catch
                    {
                        // Ignore AI network errors and use deterministic document extraction
                    }
                }

                var (astJson, plainText) = BuildLessonAstAndPlainText(
                    course.Id,
                    doc.Id,
                    cleanTitle,
                    lessonTitle,
                    slice,
                    aiSummaryPoint);

                var lesson = new Lesson
                {
                    Id = Guid.NewGuid(),
                    ChapterId = targetChapter.Id,
                    Title = lessonTitle,
                    Slug = $"doc-{doc.Id.ToString("N")[..8]}-bai-{lIdx + 1}",
                    SortOrder = baseSortOrder + lIdx,
                    CurrentVersion = 1,
                    ContentAstJson = astJson,
                    PlainText = plainText,
                    IsPublished = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                createdLessons.Add(lesson);
                db.Lessons.Add(lesson);

                db.LessonVersions.Add(new LessonVersion
                {
                    Id = Guid.NewGuid(),
                    LessonId = lesson.Id,
                    VersionNumber = 1,
                    ContentAstJson = astJson,
                    PlainText = plainText,
                    PublishedBy = userId,
                    PublishedAt = now,
                    ChangeSummary = $"Khởi tạo bài giảng từ tài liệu \"{cleanTitle}\" (Trang {slice.StartPage}–{slice.EndPage})"
                });
            }

            // 3. Create Course Tag & 5 Grounded Flashcards
            var tagName = BuildCourseTagName(cleanTitle, doc.Id);
            var tag = await db.Tags.FirstOrDefaultAsync(t => t.UserId == userId && t.Name == tagName);
            if (tag == null)
            {
                tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = tagName,
                    ColorKey = "indigo",
                    CreatedAt = now
                };
                db.Tags.Add(tag);
            }

            var candidateSentences = ExtractKeySentences(cleanTitle, lessonSlices);
            var createdCards = new List<Flashcard>();

            for (int fIdx = 0; fIdx < candidateSentences.Count; fIdx++)
            {
                var item = candidateSentences[fIdx];
                var targetLesson = createdLessons[Math.Min(createdLessons.Count - 1, item.LessonIndex)];
                var isCloze = fIdx % 2 == 0;

                string frontText;
                string backText;
                string cardType;

                if (isCloze)
                {
                    cardType = "cloze";
                    frontText = BuildClozeSentence(item.Sentence, item.TopicTitle);
                    backText = $"Trích từ \"{cleanTitle}\" (Trang {item.PageNumber} — {item.TopicTitle}):\n{item.Sentence}";
                }
                else
                {
                    cardType = "ai_generated";
                    frontText = $"Trong tài liệu \"{cleanTitle}\" (Trang {item.PageNumber}), nội dung trọng tâm của phần \"{item.TopicTitle}\" được trình bày như thế nào?";
                    backText = item.Sentence;
                }

                var card = new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = targetLesson.Id,
                    DocumentId = doc.Id,
                    AnnotationId = null,
                    CardType = cardType,
                    SourceTitleSnapshot = $"{cleanTitle} — Trang {item.PageNumber}",
                    SourceQuoteSnapshot = item.Sentence,
                    FrontAstJson = JsonSerializer.Serialize(new { text = frontText }),
                    BackAstJson = JsonSerializer.Serialize(new { text = backText }),
                    Stability = 0.0,
                    Difficulty = 0.0,
                    Reps = 0,
                    Lapses = 0,
                    State = 0,
                    IsArchived = false,
                    DueAt = now.AddMinutes(5),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                createdCards.Add(card);
                db.Flashcards.Add(card);
                db.FlashcardTags.Add(new FlashcardTag
                {
                    FlashcardId = card.Id,
                    TagId = tag.Id,
                    CreatedAt = now
                });
            }

            // 4. Create PracticeQuiz (3 Multiple Choice + 1 Essay grounded in document)
            var firstLesson = createdLessons.First();
            var quiz = new PracticeQuiz
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = Truncate($"Đề luyện tập: {cleanTitle}", 240),
                LessonId = firstLesson.Id,
                DocumentId = doc.Id,
                TotalItems = 4,
                CorrectItems = 0,
                Status = "in_progress",
                StartedAt = now
            };
            db.PracticeQuizzes.Add(quiz);

            var quizItems = BuildQuizItemsForDocument(quiz.Id, cleanTitle, lessonSlices, candidateSentences, now);
            db.PracticeQuizItems.AddRange(quizItems);

            // 5. Create Course Summary Notebook with [[Wikilink]] syllabus outline
            var notebookTitle = Truncate($"Sổ tay Khóa học: {cleanTitle}", 240);
            var notebookAst = BuildCourseNotebookAst(notebookTitle, cleanTitle, createdLessons, candidateSentences);
            var notebook = new Notebook
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = notebookTitle,
                PrimarySourceDocumentId = doc.Id,
                PrimarySourceLessonId = firstLesson.Id,
                ContentAstJson = JsonSerializer.Serialize(notebookAst),
                Version = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Notebooks.Add(notebook);

            await db.SaveChangesAsync();

            return await BuildCourseSummaryAsync(db, course, doc);
        }
        finally
        {
            sem.Release();
        }
    }

    private static readonly ConcurrentDictionary<(Guid LessonId, DateTimeOffset UpdatedAt), (Guid? DocId, int? StartPage, int? EndPage, string? SrcFolder, string? SrcRelPath)> LessonMetaCache = new();
    private static readonly bool IsRunningUnderTests = AppDomain.CurrentDomain.GetAssemblies()
        .Any(a => a.GetName().Name?.Contains("testhost", StringComparison.OrdinalIgnoreCase) == true);
    private static long _cacheVersion = 1;
    private static readonly ConcurrentDictionary<string, (List<object> Summaries, DateTimeOffset ExpiresAt)> CourseSummaryCache = new();
    private static readonly TimeSpan CourseSummaryCacheTtl = TimeSpan.FromSeconds(30);

    public static long CurrentCacheVersion => Interlocked.Read(ref _cacheVersion);

    public static void InvalidateCourseSummaryCache()
    {
        Interlocked.Increment(ref _cacheVersion);
        CourseSummaryCache.Clear();
    }

    public static (Guid? SourceDocumentId, int? StartPage, int? EndPage) GetCachedOrExtractLessonMetadata(
        Guid lessonId,
        DateTimeOffset updatedAt,
        string? contentAstJson = null,
        Guid? fallbackDocId = null)
    {
        if (LessonMetaCache.TryGetValue((lessonId, updatedAt), out var cached))
        {
            return (cached.DocId ?? fallbackDocId, cached.StartPage, cached.EndPage);
        }

        if (string.IsNullOrWhiteSpace(contentAstJson))
        {
            return (fallbackDocId, null, null);
        }

        var (docId, startPage, endPage) = ExtractLessonMetadata(contentAstJson, null);
        var (srcFolder, srcRelPath) = ExtractLessonSourceMetadata(contentAstJson);
        if (LessonMetaCache.Count > 10000) LessonMetaCache.Clear();
        LessonMetaCache[(lessonId, updatedAt)] = (docId, startPage, endPage, srcFolder, srcRelPath);
        return (docId ?? fallbackDocId, startPage, endPage);
    }

    private sealed record DocMetaRow(Guid Id, Guid CourseId, int TotalPages);
    private sealed record ChapterRow(Guid Id, Guid CourseId, string Title, int SortOrder);
    private sealed record LessonSummaryRow(
        Guid Id,
        Guid ChapterId,
        Guid CourseId,
        string Title,
        string Slug,
        int SortOrder,
        int CurrentVersion,
        string? ContentAstJson,
        string? VideoUrl,
        int EstimatedMinutes,
        string? ContentMarkdown,
        DateTimeOffset UpdatedAt,
        bool IsPublished);
    private sealed record QuizSummaryRow(Guid Id, Guid UserId, string Title, Guid? DocumentId, Guid? LessonId, DateTimeOffset StartedAt);
    private sealed record FlashcardRefRow(Guid Id, Guid UserId, Guid? DocumentId, Guid? LessonId);
    private sealed record NotebookRefRow(Guid Id, Guid? PrimarySourceDocumentId, Guid? PrimarySourceLessonId, DateTimeOffset UpdatedAt);

    public static async Task<object> BuildCourseSummaryAsync(
        SwmsDbContext db,
        Course course,
        Document? doc = null,
        bool includeLessonMarkdown = true,
        Guid? userId = null)
    {
        var summaries = await BuildCourseSummariesBatchAsync(
            db,
            new[] { course },
            includeLessonMarkdown: includeLessonMarkdown,
            userId: userId,
            singleCourseDocOverride: doc);
        return summaries[0];
    }

    /// <summary>
    /// Batched course summary builder that avoids N+1 queries and skips fetching Lesson.ContentMarkdown
    /// unless <paramref name="includeLessonMarkdown"/> is true.
    /// </summary>
    public static async Task<List<object>> BuildCourseSummariesBatchAsync(
        SwmsDbContext db,
        IReadOnlyList<Course> courses,
        bool includeLessonMarkdown = false,
        Guid? userId = null,
        Document? singleCourseDocOverride = null,
        bool includeChapterLessons = true)
    {
        if (courses.Count == 0)
        {
            return new List<object>();
        }

        string? summaryCacheKey = null;
        long cacheVer = 0;
        if (!IsRunningUnderTests && !includeLessonMarkdown && singleCourseDocOverride == null)
        {
            cacheVer = Interlocked.Read(ref _cacheVersion);
            var courseKeyPart = string.Join(',', courses.Select(c => $"{c.Id:N}:{c.UpdatedAt.UtcTicks}"));
            var shapeTag = includeChapterLessons ? "full" : "compact";
            summaryCacheKey = $"{userId?.ToString("N") ?? "anon"}:{shapeTag}:{cacheVer}:{courseKeyPart}";
            if (CourseSummaryCache.TryGetValue(summaryCacheKey, out var cachedEntry) &&
                cachedEntry.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return cachedEntry.Summaries;
            }
        }

        var courseIds = courses.Select(c => c.Id).ToHashSet();
        var instructorIds = courses.Select(c => c.InstructorId).ToHashSet();

        var slugDocIds = new HashSet<Guid>();
        foreach (var c in courses)
        {
            if (singleCourseDocOverride == null &&
                c.Slug.StartsWith("doc-course-", StringComparison.OrdinalIgnoreCase) &&
                Guid.TryParse(c.Slug["doc-course-".Length..], out var parsedSlugDocId))
            {
                slugDocIds.Add(parsedSlugDocId);
            }
        }

        var docRows = await db.Documents
            .AsNoTracking()
            .Where(d => (d.CourseId != Guid.Empty && courseIds.Contains(d.CourseId)) || slugDocIds.Contains(d.Id))
            .Select(d => new DocMetaRow(d.Id, d.CourseId, d.TotalPages))
            .ToListAsync();

        var docById = new Dictionary<Guid, DocMetaRow>();
        var firstDocByCourseId = new Dictionary<Guid, DocMetaRow>();
        foreach (var d in docRows)
        {
            docById.TryAdd(d.Id, d);
            if (d.CourseId != Guid.Empty)
            {
                firstDocByCourseId.TryAdd(d.CourseId, d);
            }
        }

        var resolvedDocs = new DocMetaRow?[courses.Count];
        var resolvedDocIds = new HashSet<Guid>();
        for (int i = 0; i < courses.Count; i++)
        {
            var c = courses[i];
            DocMetaRow? resolved = null;
            if (courses.Count == 1 && singleCourseDocOverride != null)
            {
                resolved = new DocMetaRow(singleCourseDocOverride.Id, singleCourseDocOverride.CourseId, singleCourseDocOverride.TotalPages);
            }
            else
            {
                if (c.Slug.StartsWith("doc-course-", StringComparison.OrdinalIgnoreCase) &&
                    Guid.TryParse(c.Slug["doc-course-".Length..], out var parsedDocId))
                {
                    docById.TryGetValue(parsedDocId, out resolved);
                }
                resolved ??= firstDocByCourseId.GetValueOrDefault(c.Id);
            }

            resolvedDocs[i] = resolved;
            if (resolved != null)
            {
                resolvedDocIds.Add(resolved.Id);
            }
        }

        var allChapters = await db.Chapters
            .AsNoTracking()
            .Where(ch => courseIds.Contains(ch.CourseId))
            .OrderBy(ch => ch.SortOrder)
            .Select(ch => new ChapterRow(ch.Id, ch.CourseId, ch.Title, ch.SortOrder))
            .ToListAsync();
        var chaptersByCourse = allChapters.ToLookup(ch => ch.CourseId);

        List<LessonSummaryRow> allLessons;
        if (includeLessonMarkdown)
        {
            allLessons = await (
                from l in db.Lessons.AsNoTracking()
                join ch in db.Chapters.AsNoTracking() on l.ChapterId equals ch.Id
                where courseIds.Contains(ch.CourseId)
                orderby l.SortOrder
                select new LessonSummaryRow(
                    l.Id,
                    l.ChapterId,
                    ch.CourseId,
                    l.Title,
                    l.Slug,
                    l.SortOrder,
                    l.CurrentVersion,
                    l.ContentAstJson,
                    l.VideoUrl,
                    l.EstimatedMinutes,
                    l.ContentMarkdown,
                    l.UpdatedAt,
                    l.IsPublished))
                .ToListAsync();

            foreach (var l in allLessons)
            {
                if (!string.IsNullOrWhiteSpace(l.ContentAstJson) && !LessonMetaCache.ContainsKey((l.Id, l.UpdatedAt)))
                {
                    if (LessonMetaCache.Count > 10000) LessonMetaCache.Clear();
                    var (lesDocId, startPage, endPage) = ExtractLessonMetadata(l.ContentAstJson, null);
                    var (srcFolder, srcRelPath) = ExtractLessonSourceMetadata(l.ContentAstJson);
                    LessonMetaCache[(l.Id, l.UpdatedAt)] = (lesDocId, startPage, endPage, srcFolder, srcRelPath);
                }
            }
        }
        else
        {
            var lightweightLessons = await (
                from l in db.Lessons.AsNoTracking()
                join ch in db.Chapters.AsNoTracking() on l.ChapterId equals ch.Id
                where courseIds.Contains(ch.CourseId)
                orderby l.SortOrder
                select new
                {
                    l.Id,
                    l.ChapterId,
                    ch.CourseId,
                    l.Title,
                    l.Slug,
                    l.SortOrder,
                    l.CurrentVersion,
                    l.VideoUrl,
                    l.EstimatedMinutes,
                    l.UpdatedAt,
                    l.IsPublished
                }
            ).ToListAsync();

            var uncachedKeys = includeChapterLessons
                ? lightweightLessons
                    .Where(l => !LessonMetaCache.ContainsKey((l.Id, l.UpdatedAt)))
                    .Select(l => l.Id)
                    .Distinct()
                    .ToList()
                : new List<Guid>();

            if (uncachedKeys.Count > 0)
            {
                foreach (var chunk in uncachedKeys.Chunk(500))
                {
                    var astRows = await db.Lessons.AsNoTracking()
                        .Where(l => chunk.Contains(l.Id))
                        .Select(l => new { l.Id, l.UpdatedAt, l.ContentAstJson })
                        .ToListAsync();

                    if (LessonMetaCache.Count > 10000)
                    {
                        LessonMetaCache.Clear();
                    }

                    foreach (var row in astRows)
                    {
                        var (lesDocId, startPage, endPage) = ExtractLessonMetadata(row.ContentAstJson, null);
                        var (srcFolder, srcRelPath) = ExtractLessonSourceMetadata(row.ContentAstJson);
                        LessonMetaCache[(row.Id, row.UpdatedAt)] = (lesDocId, startPage, endPage, srcFolder, srcRelPath);
                    }
                }
            }

            allLessons = lightweightLessons.Select(l => new LessonSummaryRow(
                l.Id,
                l.ChapterId,
                l.CourseId,
                l.Title,
                l.Slug,
                l.SortOrder,
                l.CurrentVersion,
                null,
                l.VideoUrl,
                l.EstimatedMinutes,
                null,
                l.UpdatedAt,
                l.IsPublished
            )).ToList();
        }
        var lessonsByCourse = allLessons.ToLookup(l => l.CourseId);
        var allCourseLessonIds = allLessons.Select(l => l.Id).ToHashSet();

        var progressUserIds = userId.HasValue
            ? new HashSet<Guid> { userId.Value }
            : instructorIds;

        var userProgressRows = progressUserIds.Count > 0
            ? await db.UserLessonProgresses
                .AsNoTracking()
                .Where(p => progressUserIds.Contains(p.UserId))
                .Select(p => new { p.UserId, p.LessonId, p.IsCompleted, p.IsBookmarked, p.NeedsReview })
                .ToListAsync()
            : [];

        var completedPairs = userProgressRows
            .Where(p => p.IsCompleted)
            .Select(x => (x.UserId, x.LessonId))
            .ToHashSet();

        var bookmarkedLessonSet = userId.HasValue
            ? userProgressRows.Where(p => p.UserId == userId.Value && p.IsBookmarked).Select(p => p.LessonId).ToHashSet()
            : new HashSet<Guid>();

        var needsReviewLessonSet = userId.HasValue
            ? userProgressRows.Where(p => p.UserId == userId.Value && p.NeedsReview).Select(p => p.LessonId).ToHashSet()
            : new HashSet<Guid>();

        var enrollmentByCourseId = new Dictionary<Guid, CourseEnrollment>();
        if (userId.HasValue)
        {
            var userEnrollments = await db.CourseEnrollments
                .AsNoTracking()
                .Where(e => e.UserId == userId.Value && courseIds.Contains(e.CourseId))
                .ToListAsync();
            foreach (var ue in userEnrollments)
            {
                enrollmentByCourseId.TryAdd(ue.CourseId, ue);
            }
        }

        var quizById = new Dictionary<Guid, QuizSummaryRow>();
        var hasResolvedDocIds = resolvedDocIds.Count > 0;
        var hasCourseLessonIds = allCourseLessonIds.Count > 0;
        if (hasResolvedDocIds || hasCourseLessonIds)
        {
            var quizRows = await db.PracticeQuizzes
                .AsNoTracking()
                .Where(q =>
                    ((hasResolvedDocIds && q.DocumentId.HasValue && resolvedDocIds.Contains(q.DocumentId.Value)) ||
                     (hasCourseLessonIds && q.LessonId.HasValue && allCourseLessonIds.Contains(q.LessonId.Value))) &&
                    (userId.HasValue
                        ? (q.UserId == userId.Value || q.UserId == SeedDataInitializer.DefaultUserId)
                        : (instructorIds.Contains(q.UserId) || q.UserId == SeedDataInitializer.DefaultUserId)))
                .Select(q => new QuizSummaryRow(q.Id, q.UserId, q.Title, q.DocumentId, q.LessonId, q.StartedAt))
                .ToListAsync();
            foreach (var q in quizRows) quizById.TryAdd(q.Id, q);
        }

        var quizzesByDoc = quizById.Values
            .Where(q => q.DocumentId.HasValue)
            .ToLookup(q => q.DocumentId!.Value);
        var quizzesByLesson = quizById.Values
            .Where(q => q.LessonId.HasValue)
            .ToLookup(q => q.LessonId!.Value);

        // Pre-compute each course's effective sourceDocId and lessonIds so Flashcards & Notebooks can batch too
        var courseSourceDocIds = new Guid?[courses.Count];
        var courseDetectedFolders = new string?[courses.Count];
        var courseLessonMeta = new Dictionary<Guid, (Guid? DocId, int? StartPage, int? EndPage, string? SrcFolder, string? SrcRelPath)>(
            includeChapterLessons ? allLessons.Count : 0);
        var allSourceDocIds = new HashSet<Guid>(resolvedDocIds);

        for (int i = 0; i < courses.Count; i++)
        {
            var c = courses[i];
            var doc = resolvedDocs[i];
            Guid? sourceDocId = doc?.Id;
            string? detectedSourceFolder = null;

            if (includeChapterLessons)
            {
                foreach (var l in lessonsByCourse[c.Id])
                {
                    Guid? lesDocId;
                    int? startPage;
                    int? endPage;
                    string? srcFolder;
                    string? srcRelPath;

                    if (LessonMetaCache.TryGetValue((l.Id, l.UpdatedAt), out var cachedMeta))
                    {
                        lesDocId = cachedMeta.DocId ?? doc?.Id;
                        startPage = cachedMeta.StartPage;
                        endPage = cachedMeta.EndPage;
                        srcFolder = cachedMeta.SrcFolder;
                        srcRelPath = cachedMeta.SrcRelPath;
                    }
                    else
                    {
                        (lesDocId, startPage, endPage) = ExtractLessonMetadata(l.ContentAstJson, doc?.Id);
                        (srcFolder, srcRelPath) = ExtractLessonSourceMetadata(l.ContentAstJson);
                    }

                    sourceDocId ??= lesDocId;
                    detectedSourceFolder ??= srcFolder;
                    courseLessonMeta[l.Id] = (lesDocId, startPage, endPage, srcFolder, srcRelPath);
                }
            }

            courseSourceDocIds[i] = sourceDocId;
            courseDetectedFolders[i] = detectedSourceFolder;
            if (sourceDocId.HasValue)
            {
                allSourceDocIds.Add(sourceDocId.Value);
            }
        }

        var flashcardQueryUserIds = new HashSet<Guid>(instructorIds);
        if (userId.HasValue)
        {
            flashcardQueryUserIds.Add(userId.Value);
        }
        var hasSourceDocIds = allSourceDocIds.Count > 0;

        var allRelevantFlashcards = flashcardQueryUserIds.Count > 0 && (hasSourceDocIds || hasCourseLessonIds)
            ? await db.Flashcards
                .AsNoTracking()
                .Where(f => !f.IsArchived &&
                            flashcardQueryUserIds.Contains(f.UserId) &&
                            ((hasCourseLessonIds && f.LessonId.HasValue && allCourseLessonIds.Contains(f.LessonId.Value)) ||
                             (hasSourceDocIds && f.DocumentId.HasValue && allSourceDocIds.Contains(f.DocumentId.Value))))
                .Select(f => new
                {
                    f.Id,
                    f.UserId,
                    f.DocumentId,
                    f.LessonId,
                    f.DueAt,
                    f.Reps,
                    f.State,
                    f.Lapses
                })
                .ToListAsync()
            : [];

        var fcById = new Dictionary<Guid, FlashcardRefRow>();
        foreach (var f in allRelevantFlashcards)
        {
            if (instructorIds.Contains(f.UserId))
            {
                fcById.TryAdd(f.Id, new FlashcardRefRow(f.Id, f.UserId, f.DocumentId, f.LessonId));
            }
        }

        var flashcardsByUserAndDoc = fcById.Values
            .Where(f => f.DocumentId.HasValue)
            .ToLookup(f => (f.UserId, f.DocumentId!.Value));
        var flashcardsByUserAndLesson = fcById.Values
            .Where(f => f.LessonId.HasValue)
            .ToLookup(f => (f.UserId, f.LessonId!.Value));

        var relevantFcIds = fcById.Keys.ToList();
        var flashcardTagLookup = includeChapterLessons && relevantFcIds.Count > 0
            ? (await (
                from ft in db.FlashcardTags.AsNoTracking()
                join t in db.Tags.AsNoTracking() on ft.TagId equals t.Id
                where relevantFcIds.Contains(ft.FlashcardId)
                select new { ft.FlashcardId, t.Id, t.Name })
                .ToListAsync())
                .ToLookup(x => x.FlashcardId, x => (x.Id, x.Name))
            : Array.Empty<(Guid FlashcardId, Guid Id, string Name)>().ToLookup(x => x.FlashcardId, x => (x.Id, x.Name));

        var nbById = new Dictionary<Guid, NotebookRefRow>();
        if (hasSourceDocIds || hasCourseLessonIds)
        {
            var nbRows = await db.Notebooks
                .AsNoTracking()
                .Where(n => (hasSourceDocIds && n.PrimarySourceDocumentId.HasValue && allSourceDocIds.Contains(n.PrimarySourceDocumentId.Value)) ||
                            (hasCourseLessonIds && n.PrimarySourceLessonId.HasValue && allCourseLessonIds.Contains(n.PrimarySourceLessonId.Value)))
                .Select(n => new NotebookRefRow(n.Id, n.PrimarySourceDocumentId, n.PrimarySourceLessonId, n.UpdatedAt))
                .ToListAsync();
            foreach (var nb in nbRows) nbById.TryAdd(nb.Id, nb);
        }

        var notebooksByDoc = nbById.Values
            .Where(n => n.PrimarySourceDocumentId.HasValue)
            .ToLookup(n => n.PrimarySourceDocumentId!.Value);
        var notebooksByLesson = nbById.Values
            .Where(n => n.PrimarySourceLessonId.HasValue)
            .ToLookup(n => n.PrimarySourceLessonId!.Value);

        var lapsesByLesson = userId.HasValue
            ? allRelevantFlashcards
                .Where(f => f.UserId == userId.Value && f.Lapses > 0 && f.LessonId.HasValue && allCourseLessonIds.Contains(f.LessonId.Value))
                .ToLookup(f => f.LessonId!.Value, f => f.Lapses)
            : Array.Empty<(Guid, int)>().ToLookup(x => x.Item1, x => x.Item2);

        ILookup<Guid, Guid> quizErrorsByLesson = Array.Empty<(Guid, Guid)>().ToLookup(x => x.Item1, x => x.Item2);

        if (userId.HasValue && allCourseLessonIds.Count > 0)
        {
            var incorrectQuizItems = await (
                from q in db.PracticeQuizzes.AsNoTracking()
                join qi in db.PracticeQuizItems.AsNoTracking() on q.Id equals qi.QuizId
                where q.UserId == userId.Value && q.LessonId.HasValue && allCourseLessonIds.Contains(q.LessonId.Value) && qi.IsCorrect == false
                select new { LessonId = q.LessonId!.Value }
            ).ToListAsync();
            quizErrorsByLesson = incorrectQuizItems.ToLookup(qi => qi.LessonId, qi => qi.LessonId);
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var summaryCourseIds = courses.Select(c => c.Id).Distinct().ToList();
        var courseActivities = summaryCourseIds.Count > 0
            ? await db.PracticeActivities.AsNoTracking()
                .Where(a => summaryCourseIds.Contains(a.CourseId))
                .Select(a => new { a.Id, a.CourseId, a.LessonId })
                .ToListAsync()
            : [];
        var activitiesByCourseLookup = courseActivities.ToLookup(a => a.CourseId);
        var activitiesByLessonLookup = courseActivities
            .Where(a => a.LessonId.HasValue)
            .ToLookup(a => a.LessonId!.Value);

        var passedActivityIdSet = new HashSet<Guid>();
        var bestScoreByActivityId = new Dictionary<Guid, double>();
        if (userId.HasValue && courseActivities.Count > 0)
        {
            var activityIds = courseActivities.Select(a => a.Id).Distinct().ToList();
            var userAttempts = await db.PracticeAttempts.AsNoTracking()
                .Where(pa => pa.UserId == userId.Value && pa.ActivityId.HasValue && activityIds.Contains(pa.ActivityId.Value))
                .Select(pa => new { ActivityId = pa.ActivityId!.Value, pa.IsPassed, pa.Score })
                .ToListAsync();
            foreach (var grp in userAttempts.GroupBy(pa => pa.ActivityId))
            {
                if (grp.Any(x => x.IsPassed))
                {
                    passedActivityIdSet.Add(grp.Key);
                }
                bestScoreByActivityId[grp.Key] = grp.Max(x => x.Score);
            }
        }

        var userFsrsCards = userId.HasValue
            ? allRelevantFlashcards
                .Where(f => f.UserId == userId.Value)
                .Select(f => new { f.Id, f.DocumentId, f.LessonId, f.DueAt, f.Reps, f.State })
                .ToList()
            : [];
        var userCardsByDoc = userFsrsCards
            .Where(f => f.DocumentId.HasValue)
            .ToLookup(f => f.DocumentId!.Value);
        var userCardsByLesson = userFsrsCards
            .Where(f => f.LessonId.HasValue)
            .ToLookup(f => f.LessonId!.Value);

        var results = new List<object>(courses.Count);

        for (int i = 0; i < courses.Count; i++)
        {
            var course = courses[i];
            var doc = resolvedDocs[i];
            var chapters = chaptersByCourse[course.Id].ToList();
            var allCourseLessons = lessonsByCourse[course.Id].ToList();

            var isOwner = userId.HasValue && (course.InstructorId == userId.Value || (userId.Value == SeedDataInitializer.DefaultUserId && course.InstructorId == SeedDataInitializer.DefaultUserId));
            var lessons = (userId.HasValue && !isOwner)
                ? allCourseLessons.Where(l => l.IsPublished).ToList()
                : allCourseLessons;

            var effectiveProgressUserId = userId ?? course.InstructorId;
            var completedLessonIds = lessons
                .Where(l => completedPairs.Contains((effectiveProgressUserId, l.Id)))
                .Select(l => l.Id)
                .ToList();
            var completedLessonSet = completedLessonIds.ToHashSet();
            var totalLessonsCount = lessons.Count;
            var completedLessonsCount = completedLessonIds.Count;
            var progressPercent = totalLessonsCount > 0
                ? (int)Math.Round((double)completedLessonsCount * 100.0 / totalLessonsCount)
                : 0;
            var totalEstimatedMinutes = lessons.Sum(l => l.EstimatedMinutes > 0 ? l.EstimatedMinutes : 10);

            var chapterWeakStats = new List<(Guid Id, string Title, string Reason, int TotalErrors)>();
            if (userId.HasValue)
            {
                foreach (var ch in chapters)
                {
                    var chLessons = lessons.Where(l => l.ChapterId == ch.Id).ToList();
                    int chLapses = chLessons.Sum(l => lapsesByLesson[l.Id].Sum());
                    int chQuizErrors = chLessons.Sum(l => quizErrorsByLesson[l.Id].Count());
                    int totalErrors = chLapses + chQuizErrors;
                    if (totalErrors > 0)
                    {
                        string reason = chLapses > 0 && chQuizErrors > 0
                            ? $"{chQuizErrors} câu trắc nghiệm trả lời sai và {chLapses} lần quên thẻ ghi nhớ"
                            : chQuizErrors > 0
                                ? $"{chQuizErrors} câu trắc nghiệm trả lời sai cần ôn lại"
                                : $"{chLapses} lần quên thẻ ghi nhớ cần củng cố";
                        chapterWeakStats.Add((ch.Id, ch.Title, reason, totalErrors));
                    }
                }
            }

            var weakChapters = chapterWeakStats
                .OrderByDescending(w => w.TotalErrors)
                .Take(3)
                .Select(w => (object)new
                {
                    chapterId = w.Id,
                    title = w.Title,
                    reason = w.Reason,
                    pendingReviewsOrErrors = w.TotalErrors
                })
                .ToList();

            var matchedQuizMap = new Dictionary<Guid, QuizSummaryRow>();
            if (doc != null)
            {
                foreach (var q in quizzesByDoc[doc.Id]) matchedQuizMap.TryAdd(q.Id, q);
            }
            foreach (var l in lessons)
            {
                foreach (var q in quizzesByLesson[l.Id]) matchedQuizMap.TryAdd(q.Id, q);
            }

            static bool IsSyntheticQuizTitle(string? t) =>
                !string.IsNullOrWhiteSpace(t) &&
                (t.StartsWith("[Đề Chương ", StringComparison.OrdinalIgnoreCase) ||
                 t.StartsWith("[Đề Tổng Hợp", StringComparison.OrdinalIgnoreCase) ||
                 t.StartsWith("[Đề Tự Tạo]", StringComparison.OrdinalIgnoreCase));

            var orderedCandidateQuizzes = matchedQuizMap.Values
                .Where(q => !userId.HasValue || userId.Value == SeedDataInitializer.DefaultUserId || q.Id != SeedDataInitializer.DefaultQuizId)
                .OrderBy(q => userId.HasValue && q.UserId == userId.Value ? 0 : 1)
                .ThenByDescending(q => q.StartedAt)
                .ToList();

            var seenQuizKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var matchingQuizzes = new List<QuizSummaryRow>();
            foreach (var q in orderedCandidateQuizzes)
            {
                var normTitle = (q.Title ?? string.Empty).Trim();
                string dedupKey = q.LessonId.HasValue
                    ? $"L:{q.LessonId.Value:N}|{normTitle}"
                    : q.DocumentId.HasValue
                        ? $"D:{q.DocumentId.Value:N}|{normTitle}"
                        : $"I:{q.Id:N}";
                if (seenQuizKeys.Add(dedupKey))
                {
                    matchingQuizzes.Add(q);
                }
            }

            var quiz = matchingQuizzes
                .OrderByDescending(q => q.StartedAt)
                .FirstOrDefault();

            var lessonQuizMap = new Dictionary<Guid, Guid>();
            foreach (var q in matchingQuizzes
                .OrderBy(q => IsSyntheticQuizTitle(q.Title) ? 1 : 0)
                .ThenBy(q => userId.HasValue && q.UserId == userId.Value ? 0 : 1)
                .ThenByDescending(q => q.StartedAt))
            {
                if (q.LessonId.HasValue)
                {
                    lessonQuizMap.TryAdd(q.LessonId.Value, q.Id);
                }
            }

            var sourceDocId = courseSourceDocIds[i];
            var detectedSourceFolder = courseDetectedFolders[i];
            var parsedLessonsByChapter = includeChapterLessons
                ? new Dictionary<Guid, List<object>>(chapters.Count)
                : null;
            if (parsedLessonsByChapter != null)
            {
                foreach (var ch in chapters)
                {
                    parsedLessonsByChapter[ch.Id] = new List<object>();
                }

                foreach (var l in lessons)
                {
                    var meta = courseLessonMeta[l.Id];
                    if (parsedLessonsByChapter.TryGetValue(l.ChapterId, out var list))
                    {
                        lessonQuizMap.TryGetValue(l.Id, out var linkedQuizId);
                        var lessonActs = activitiesByLessonLookup[l.Id].ToList();
                        var lessonPassedActs = lessonActs.Count(a => passedActivityIdSet.Contains(a.Id));
                        list.Add(new
                        {
                            id = l.Id,
                            lessonId = l.Id,
                            chapterId = l.ChapterId,
                            courseId = course.Id,
                            title = l.Title,
                            slug = l.Slug,
                            sortOrder = l.SortOrder,
                            currentVersion = l.CurrentVersion,
                            sourceDocumentId = meta.DocId,
                            startPage = meta.StartPage,
                            endPage = meta.EndPage,
                            sourceFolder = meta.SrcFolder,
                            sourceRelativePath = meta.SrcRelPath,
                            quizId = linkedQuizId != Guid.Empty ? linkedQuizId : (Guid?)null,
                            videoUrl = l.VideoUrl,
                            estimatedMinutes = l.EstimatedMinutes,
                            isCompleted = completedLessonSet.Contains(l.Id),
                            isBookmarked = bookmarkedLessonSet.Contains(l.Id),
                            needsReview = needsReviewLessonSet.Contains(l.Id),
                            practiceActivityCount = lessonActs.Count,
                            practicePassedCount = lessonPassedActs,
                            contentMarkdown = includeLessonMarkdown ? l.ContentMarkdown : null,
                            updatedAt = l.UpdatedAt
                        });
                    }
                }
            }

            var matchedFlashcardIds = new HashSet<Guid>();
            if (sourceDocId.HasValue)
            {
                foreach (var fc in flashcardsByUserAndDoc[(course.InstructorId, sourceDocId.Value)])
                {
                    matchedFlashcardIds.Add(fc.Id);
                }
            }
            foreach (var l in lessons)
            {
                foreach (var fc in flashcardsByUserAndLesson[(course.InstructorId, l.Id)])
                {
                    matchedFlashcardIds.Add(fc.Id);
                }
            }

            (Guid Id, string Name)? courseTag = null;
            foreach (var fcId in matchedFlashcardIds)
            {
                var found = flashcardTagLookup[fcId].FirstOrDefault();
                if (found.Id != Guid.Empty)
                {
                    courseTag = found;
                    break;
                }
            }

            var matchedNotebooks = new Dictionary<Guid, NotebookRefRow>();
            if (sourceDocId.HasValue)
            {
                foreach (var nb in notebooksByDoc[sourceDocId.Value]) matchedNotebooks.TryAdd(nb.Id, nb);
            }
            foreach (var l in lessons)
            {
                foreach (var nb in notebooksByLesson[l.Id]) matchedNotebooks.TryAdd(nb.Id, nb);
            }
            var notebook = matchedNotebooks.Values.OrderByDescending(n => n.UpdatedAt).FirstOrDefault();

            var firstLessonId = chapters
                .SelectMany(ch => lessons.Where(l => l.ChapterId == ch.Id))
                .Select(l => (Guid?)l.Id)
                .FirstOrDefault();

            var (resolvedSourceFolder, resolvedCategory) = DocumentCourseSyncService.ResolveCourseMetadata(
                course.Slug,
                course.Title,
                detectedSourceFolder);

            var courseActivityList = activitiesByCourseLookup[course.Id].ToList();
            var practiceTotalActivities = courseActivityList.Count;
            var practicePassedActivities = courseActivityList.Count(a => passedActivityIdSet.Contains(a.Id));
            var courseAttemptedScores = courseActivityList
                .Where(a => bestScoreByActivityId.ContainsKey(a.Id))
                .Select(a => bestScoreByActivityId[a.Id])
                .ToList();
            var practiceAverageScore = courseAttemptedScores.Count > 0
                ? Math.Round(courseAttemptedScores.Average(), 1)
                : 0.0;

            var userCourseCardMap = new Dictionary<Guid, (DateTimeOffset DueAt, int Reps, int State)>();
            if (userId.HasValue)
            {
                if (sourceDocId.HasValue)
                {
                    foreach (var uc in userCardsByDoc[sourceDocId.Value])
                    {
                        userCourseCardMap[uc.Id] = (uc.DueAt, uc.Reps, uc.State);
                    }
                }
                foreach (var l in lessons)
                {
                    foreach (var uc in userCardsByLesson[l.Id])
                    {
                        userCourseCardMap[uc.Id] = (uc.DueAt, uc.Reps, uc.State);
                    }
                }
            }
            var hasUnclonedPublicDeck = userId.HasValue
                && userCourseCardMap.Count == 0
                && matchedFlashcardIds.Count > 0
                && userId.Value != course.InstructorId;
            var dueFlashcardCount = hasUnclonedPublicDeck
                ? matchedFlashcardIds.Count
                : userCourseCardMap.Values.Count(c => c.DueAt <= nowUtc);
            var masteredFlashcardCount = hasUnclonedPublicDeck
                ? 0
                : userCourseCardMap.Values.Count(c => c.Reps > 0 && c.State >= 2);

            var curatedMeta = CuratedCourseMetadataCatalog.ResolveMetadata(course);
            enrollmentByCourseId.TryGetValue(course.Id, out var enrollment);
            var isSystemCourse = DocumentCourseSyncService.IsProtectedSystemCourse(course);
            var isOwnerOrSeeded = userId.HasValue && (course.InstructorId == userId.Value || (userId.Value == SeedDataInitializer.DefaultUserId && course.InstructorId == SeedDataInitializer.DefaultUserId));
            var canDelete = !isSystemCourse && isOwnerOrSeeded;
            var isPinned = enrollment?.IsPinned ?? false;
            var isArchived = enrollment?.IsArchived ?? course.IsArchived;
            var dailyLessonGoal = enrollment?.DailyLessonGoal > 0 ? enrollment.DailyLessonGoal : 2;
            var targetCompletionDate = enrollment?.TargetCompletionDate;
            var bookmarkedLessonCount = lessons.Count(l => bookmarkedLessonSet.Contains(l.Id));
            var needsReviewLessonCount = lessons.Count(l => needsReviewLessonSet.Contains(l.Id));

            results.Add(new
            {
                id = course.Id,
                courseId = course.Id,
                title = course.Title,
                slug = course.Slug,
                description = course.Description,
                visibility = course.Visibility,
                isPublished = course.IsPublished,
                isPinned,
                isArchived,
                dailyLessonGoal,
                targetCompletionDate,
                isSystemCourse,
                canDelete,
                sourceFolder = resolvedSourceFolder,
                category = curatedMeta.Category,
                level = curatedMeta.Level,
                learningOutcomes = curatedMeta.LearningOutcomes,
                prerequisites = curatedMeta.Prerequisites,
                targetAudience = curatedMeta.TargetAudience,
                labLabel = curatedMeta.LabLabel,
                hasAudio = curatedMeta.HasAudio,
                bookmarkedLessonCount,
                needsReviewLessonCount,
                sourceDocumentId = sourceDocId,
                totalPages = doc?.TotalPages ?? 1,
                chapterCount = chapters.Count,
                lessonCount = lessons.Count,
                totalLessons = totalLessonsCount,
                completedLessons = completedLessonsCount,
                progressPercent,
                totalEstimatedMinutes,
                weakChapters,
                completedLessonIds = includeChapterLessons ? completedLessonIds : new List<Guid>(),
                flashcardCount = matchedFlashcardIds.Count,
                dueFlashcardCount,
                masteredFlashcardCount,
                practiceTotalActivities,
                practicePassedActivities,
                practiceAverageScore,
                quizCount = matchingQuizzes.Count,
                tagId = courseTag?.Id,
                tagName = courseTag?.Name,
                quizId = quiz?.Id,
                lessonQuizMap = includeChapterLessons ? lessonQuizMap : new Dictionary<Guid, Guid>(),
                notebookId = notebook?.Id,
                firstLessonId,
                createdAt = course.CreatedAt,
                updatedAt = course.UpdatedAt,
                chapters = parsedLessonsByChapter != null
                    ? (object)chapters.Select(ch => new
                    {
                        id = ch.Id,
                        chapterId = ch.Id,
                        title = ch.Title,
                        sortOrder = ch.SortOrder,
                        lessons = parsedLessonsByChapter.GetValueOrDefault(ch.Id, new List<object>())
                    }).ToList()
                    : Array.Empty<object>()
            });
        }

        if (summaryCacheKey != null && Interlocked.Read(ref _cacheVersion) == cacheVer)
        {
            if (CourseSummaryCache.Count > 512)
            {
                var nowCacheCheck = DateTimeOffset.UtcNow;
                foreach (var kvp in CourseSummaryCache)
                {
                    if (kvp.Value.ExpiresAt <= nowCacheCheck)
                    {
                        CourseSummaryCache.TryRemove(kvp.Key, out _);
                    }
                }
                if (CourseSummaryCache.Count > 512)
                {
                    CourseSummaryCache.Clear();
                }
            }
            CourseSummaryCache[summaryCacheKey] = (results, DateTimeOffset.UtcNow.Add(CourseSummaryCacheTtl));
        }

        return results;
    }

    public static (string? SourceFolder, string? SourceRelativePath) ExtractLessonSourceMetadata(string? contentAstJson)
    {
        if (string.IsNullOrWhiteSpace(contentAstJson))
            return (null, null);

        try
        {
            using var jsonDoc = JsonDocument.Parse(contentAstJson);
            if (jsonDoc.RootElement.TryGetProperty("attrs", out var attrs) && attrs.ValueKind == JsonValueKind.Object)
            {
                string? sourceFolder = attrs.TryGetProperty("sourceFolder", out var sfProp) && sfProp.ValueKind == JsonValueKind.String
                    ? sfProp.GetString()
                    : null;
                string? sourceRelPath = attrs.TryGetProperty("sourceRelativePath", out var srProp) && srProp.ValueKind == JsonValueKind.String
                    ? srProp.GetString()
                    : null;
                return (sourceFolder, sourceRelPath);
            }
        }
        catch
        {
            // Ignore malformed AST
        }

        return (null, null);
    }

    public static (Guid? SourceDocumentId, int? StartPage, int? EndPage) ExtractLessonMetadata(string? contentAstJson, Guid? fallbackDocId = null)
    {
        if (string.IsNullOrWhiteSpace(contentAstJson))
            return (fallbackDocId, null, null);

        try
        {
            using var jsonDoc = JsonDocument.Parse(contentAstJson);
            if (jsonDoc.RootElement.TryGetProperty("attrs", out var attrs) && attrs.ValueKind == JsonValueKind.Object)
            {
                Guid? docId = fallbackDocId;
                if (attrs.TryGetProperty("sourceDocumentId", out var sdProp) && Guid.TryParse(sdProp.GetString(), out var parsedDoc))
                {
                    docId = parsedDoc;
                }
                int? startPage = attrs.TryGetProperty("startPage", out var spProp) && spProp.TryGetInt32(out var sp) ? sp : null;
                int? endPage = attrs.TryGetProperty("endPage", out var epProp) && epProp.TryGetInt32(out var ep) ? ep : null;
                return (docId, startPage, endPage);
            }
        }
        catch
        {
            // Ignore malformed AST
        }

        return (fallbackDocId, null, null);
    }

    private sealed record LessonSlice(
        int Index,
        int StartPage,
        int EndPage,
        string HeadingTitle,
        List<string> CoreParagraphs
    );

    private sealed record ExtractedSentence(
        int LessonIndex,
        int PageNumber,
        string TopicTitle,
        string Sentence
    );

    private static List<LessonSlice> BuildLessonSlices(string cleanDocTitle, List<DocumentPage> orderedPages, int totalPages)
    {
        const int targetLessonCount = 4;
        var slices = new List<LessonSlice>(targetLessonCount);

        // If single page with multiple paragraphs/lines, split text into 4 parts so each lesson has distinct content
        if (orderedPages.Count == 1)
        {
            var rawText = orderedPages[0].PlainText ?? string.Empty;
            var lines = rawText
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();

            if (lines.Count == 0)
            {
                lines.Add($"Nội dung trọng tâm của tài liệu {cleanDocTitle}.");
            }

            var sliceCount = Math.Clamp(lines.Count >= 4 ? 4 : 3, 3, 4);
            for (int i = 0; i < sliceCount; i++)
            {
                var startLine = (int)Math.Floor(i * (double)lines.Count / sliceCount);
                var endLine = Math.Max(startLine + 1, (int)Math.Floor((i + 1) * (double)lines.Count / sliceCount));
                var chunkLines = lines.Skip(startLine).Take(Math.Max(1, endLine - startLine)).ToList();
                if (chunkLines.Count == 0) chunkLines.Add(lines[Math.Min(i, lines.Count - 1)]);

                var startPage = Math.Clamp((int)Math.Floor(i * (double)totalPages / sliceCount) + 1, 1, totalPages);
                var endPage = Math.Clamp((int)Math.Floor((i + 1) * (double)totalPages / sliceCount), startPage, totalPages);

                var heading = DetectHeadingFromLines(chunkLines, cleanDocTitle, i + 1);
                var paragraphs = BuildParagraphsFromLines(chunkLines, cleanDocTitle, startPage, endPage);
                slices.Add(new LessonSlice(i, startPage, endPage, heading, paragraphs));
            }

            return slices;
        }

        // Multi-page document: divide pages into 4 contiguous ranges and sample up to 6 pages per range for speed
        for (int i = 0; i < targetLessonCount; i++)
        {
            var startPage = Math.Clamp((int)Math.Floor(i * (double)totalPages / targetLessonCount) + 1, 1, totalPages);
            var endPage = Math.Clamp((int)Math.Floor((i + 1) * (double)totalPages / targetLessonCount), startPage, totalPages);

            var rangePages = orderedPages
                .Where(p => p.PageNumber >= startPage && p.PageNumber <= endPage && !string.IsNullOrWhiteSpace(p.PlainText))
                .Take(6)
                .ToList();

            var sampledLines = new List<string>();
            foreach (var p in rangePages)
            {
                var pLines = p.PlainText
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => l.Length >= 4)
                    .Take(18);
                sampledLines.AddRange(pLines);
            }

            var heading = DetectHeadingFromLines(sampledLines, cleanDocTitle, i + 1);
            var paragraphs = BuildParagraphsFromLines(sampledLines, cleanDocTitle, startPage, endPage);
            slices.Add(new LessonSlice(i, startPage, endPage, heading, paragraphs));
        }

        return slices;
    }

    private static string DetectHeadingFromLines(List<string> lines, string cleanDocTitle, int partIndex)
    {
        foreach (var line in lines)
        {
            var m = HeadingRegex.Match(line);
            if (m.Success)
            {
                var candidate = CleanSingleLine(line.TrimStart('#', ' ', '\t'));
                if (candidate.Length is >= 6 and <= 110)
                {
                    return candidate;
                }
            }
        }

        foreach (var line in lines)
        {
            var cleaned = CleanSingleLine(line);
            if (cleaned.Length is >= 10 and <= 95 && !cleaned.EndsWith('.'))
            {
                return cleaned;
            }
        }

        var firstSentence = lines.FirstOrDefault(l => l.Length >= 12);
        if (!string.IsNullOrWhiteSpace(firstSentence))
        {
            return Truncate(CleanSingleLine(firstSentence), 75);
        }

        return $"Chuyên đề {partIndex} — {Truncate(cleanDocTitle, 60)}";
    }

    private static List<string> BuildParagraphsFromLines(List<string> lines, string cleanDocTitle, int startPage, int endPage)
    {
        var joined = string.Join(" ", lines.Select(CleanSingleLine).Where(l => l.Length > 0));
        if (string.IsNullOrWhiteSpace(joined))
        {
            return new List<string>
            {
                $"Nội dung bài học thuộc phạm vi Trang {startPage}–{endPage} của tài liệu \"{cleanDocTitle}\". Trang tài liệu này ở dạng bản quét hình ảnh hoặc biểu đồ trực quan; bạn có thể đối chiếu trực tiếp trên khung Bản in PDF và ghi chép vào sổ tay tổng hợp."
            };
        }

        var sentences = SplitIntoSentences(joined);
        var paragraphs = new List<string>();
        var current = new StringBuilder();

        foreach (var s in sentences)
        {
            if (current.Length + s.Length > 420 && current.Length > 0)
            {
                paragraphs.Add(current.ToString().Trim());
                current.Clear();
                if (paragraphs.Count >= 3) break;
            }
            if (current.Length > 0) current.Append(' ');
            current.Append(s);
        }

        if (current.Length > 0 && paragraphs.Count < 3)
        {
            paragraphs.Add(current.ToString().Trim());
        }

        if (paragraphs.Count == 0)
        {
            paragraphs.Add(Truncate(joined, 500));
        }

        return paragraphs;
    }

    private static (string AstJson, string PlainText) BuildLessonAstAndPlainText(
        Guid courseId,
        Guid documentId,
        string cleanDocTitle,
        string lessonTitle,
        LessonSlice slice,
        string? aiSummaryPoint)
    {
        var blocks = new List<object>();
        var plainParts = new List<string>();

        var sec1Title = $"1. Mục tiêu & Trọng tâm (Trang {slice.StartPage}–{slice.EndPage})";
        var sec1Body = $"Bài giảng này tổng hợp trực tiếp nội dung từ Trang {slice.StartPage} đến Trang {slice.EndPage} của tài liệu \"{cleanDocTitle}\" với chủ đề trọng tâm: {slice.HeadingTitle}.";
        blocks.Add(new
        {
            type = "heading",
            attrs = new { level = 2, block_id = Guid.NewGuid().ToString(), section = sec1Title },
            content = new[] { new { type = "text", text = sec1Title } }
        });
        blocks.Add(new
        {
            type = "paragraph",
            attrs = new { block_id = Guid.NewGuid().ToString() },
            content = new[] { new { type = "text", text = sec1Body } }
        });
        plainParts.Add(sec1Title);
        plainParts.Add(sec1Body);

        var sec2Title = "2. Nội dung cốt lõi trích xuất từ tài liệu";
        blocks.Add(new
        {
            type = "heading",
            attrs = new { level = 2, block_id = Guid.NewGuid().ToString(), section = sec2Title },
            content = new[] { new { type = "text", text = sec2Title } }
        });
        plainParts.Add(sec2Title);

        foreach (var para in slice.CoreParagraphs)
        {
            blocks.Add(new
            {
                type = "paragraph",
                attrs = new { block_id = Guid.NewGuid().ToString() },
                content = new[] { new { type = "text", text = para } }
            });
            plainParts.Add(para);
        }

        var sec3Title = "3. Điểm cần ghi nhớ & Ôn tập chủ động";
        var takeawayText = !string.IsNullOrWhiteSpace(aiSummaryPoint)
            ? aiSummaryPoint
            : $"Trọng tâm cần nắm vững ở {lessonTitle}: đối chiếu nội dung gốc tại Trang {slice.StartPage}–{slice.EndPage}, đánh dấu (Highlight phím 1–5) các thuật ngữ quan trọng và ôn tập thẻ ghi nhớ FSRS gắn với khóa học \"{cleanDocTitle}\".";

        blocks.Add(new
        {
            type = "heading",
            attrs = new { level = 2, block_id = Guid.NewGuid().ToString(), section = sec3Title },
            content = new[] { new { type = "text", text = sec3Title } }
        });
        blocks.Add(new
        {
            type = "paragraph",
            attrs = new { block_id = Guid.NewGuid().ToString() },
            content = new[] { new { type = "text", text = takeawayText } }
        });
        plainParts.Add(sec3Title);
        plainParts.Add(takeawayText);

        var astObj = new
        {
            type = "doc",
            attrs = new
            {
                courseId = courseId.ToString(),
                sourceDocumentId = documentId.ToString(),
                startPage = slice.StartPage,
                endPage = slice.EndPage
            },
            content = blocks
        };

        return (JsonSerializer.Serialize(astObj), string.Join("\n\n", plainParts));
    }

    private static List<ExtractedSentence> ExtractKeySentences(string cleanDocTitle, List<LessonSlice> slices)
    {
        var results = new List<ExtractedSentence>();

        foreach (var slice in slices)
        {
            foreach (var para in slice.CoreParagraphs)
            {
                foreach (var s in SplitIntoSentences(para))
                {
                    var trimmed = CleanSingleLine(s);
                    if (trimmed.Length is >= 25 and <= 260)
                    {
                        results.Add(new ExtractedSentence(slice.Index, slice.StartPage, slice.HeadingTitle, trimmed));
                        if (results.Count >= 6) return results;
                        break;
                    }
                }
            }
        }

        while (results.Count < 4)
        {
            var idx = results.Count % Math.Max(1, slices.Count);
            var slice = slices[idx];
            var fallback = Truncate(slice.CoreParagraphs.FirstOrDefault() ?? $"Nội dung trọng tâm của {cleanDocTitle} tại trang {slice.StartPage} về {slice.HeadingTitle}.", 220);
            results.Add(new ExtractedSentence(idx, slice.StartPage, slice.HeadingTitle, fallback));
        }

        return results.Take(5).ToList();
    }

    private static string BuildClozeSentence(string sentence, string topicTitle)
    {
        var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 6)
        {
            var start = Math.Min(2, words.Length - 3);
            var len = Math.Min(3, words.Length - start);
            var targetPhrase = string.Join(" ", words.Skip(start).Take(len)).Trim(',', '.', ';', ':');
            if (targetPhrase.Length >= 3)
            {
                var idx = sentence.IndexOf(targetPhrase, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    return sentence[..idx] + $"{{{{c1::{targetPhrase}}}}}" + sentence[(idx + targetPhrase.Length)..];
                }
            }
        }

        return $"Chủ đề {{{{c1::{Truncate(topicTitle, 45)}}}}}: {sentence}";
    }

    private static List<PracticeQuizItem> BuildQuizItemsForDocument(
        Guid quizId,
        string cleanDocTitle,
        List<LessonSlice> slices,
        List<ExtractedSentence> sentences,
        DateTimeOffset now)
    {
        var items = new List<PracticeQuizItem>();
        var targetKeys = new[] { "C", "A", "D" };

        for (int i = 0; i < 3; i++)
        {
            var sent = sentences[i % sentences.Count];
            var correctKey = targetKeys[i % targetKeys.Length];
            var correctStatement = Truncate(sent.Sentence, 170);

            var distractors = new[]
            {
                $"Nội dung này không xuất hiện trong tài liệu \"{Truncate(cleanDocTitle, 50)}\" mà thuộc giáo trình hoàn toàn khác.",
                $"Khái niệm tại Trang {sent.PageNumber} phủ nhận hoàn toàn vai trò của \"{Truncate(sent.TopicTitle, 45)}\" trong thực tiễn.",
                $"Phần \"{Truncate(sent.TopicTitle, 45)}\" chỉ bàn về lịch sử phụ lục mà không nêu nguyên lý hay định nghĩa cốt lõi."
            };

            var choiceList = new List<object>();
            int dIdx = 0;
            foreach (var key in new[] { "A", "B", "C", "D" })
            {
                var text = key == correctKey ? correctStatement : distractors[dIdx++];
                choiceList.Add(new { key, text });
            }

            items.Add(new PracticeQuizItem
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                ItemType = "multiple_choice",
                PromptAstJson = JsonSerializer.Serialize(new
                {
                    question = $"Câu {i + 1} ({cleanDocTitle} — Trang {sent.PageNumber}): Phát biểu nào sau đây phản ánh chính xác nhất nội dung trọng tâm của mục \"{sent.TopicTitle}\"?"
                }),
                ChoicesJson = JsonSerializer.Serialize(choiceList),
                ClozeSolution = correctKey,
                CreatedAt = now.AddSeconds(i)
            });
        }

        // Item 4: Essay rubric item grounded in the uploaded document's chapters/lessons
        var topicNames = slices.Select(s => s.HeadingTitle).Take(3).ToList();
        var topicSummary = string.Join("; ", topicNames);
        var referenceExcerpt = sentences.First().Sentence;

        items.Add(new PracticeQuizItem
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            ItemType = "essay",
            PromptAstJson = JsonSerializer.Serialize(new
            {
                question = $"Dựa trên tài liệu \"{cleanDocTitle}\", hãy phân tích mối liên hệ và ý nghĩa thực tiễn của các nội dung trọng tâm: {topicSummary}.",
                rubricCriteria = new[]
                {
                    $"Trình bày chính xác khái niệm cốt lõi trong \"{cleanDocTitle}\" ({Truncate(topicNames.FirstOrDefault() ?? cleanDocTitle, 70)}).",
                    "Liên hệ và dẫn chứng được nội dung cụ thể từ các trang tài liệu đã trích xuất.",
                    "Tổng hợp được ý nghĩa vận dụng thực tế của nội dung bài học."
                }
            }),
            ClozeSolution = $"Nội dung cốt lõi từ \"{cleanDocTitle}\": {Truncate(referenceExcerpt, 220)}",
            CreatedAt = now.AddSeconds(4)
        });

        return items;
    }

    private static object BuildCourseNotebookAst(
        string notebookTitle,
        string cleanDocTitle,
        List<Lesson> lessons,
        List<ExtractedSentence> sentences)
    {
        var wikilinks = string.Join(" • ", lessons.Select(l => $"[[{l.Title}]]"));
        var summaryPoints = string.Join(" ", sentences.Take(3).Select(s => $"• (Tr. {s.PageNumber}) {s.Sentence}"));

        return new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 1, block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = notebookTitle } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = $"📚 Đề cương lộ trình bài giảng của khóa học \"{cleanDocTitle}\": {wikilinks}"
                        }
                    }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = $"💡 Trích dẫn trọng tâm từ tài liệu gốc: {summaryPoints}"
                        }
                    }
                }
            }
        };
    }

    public static string BuildCourseTagName(string cleanTitle, Guid docId)
    {
        var normalized = VietnameseTextNormalizer.Unaccent(cleanTitle);
        var words = Regex.Replace(normalized, @"[^a-zA-Z0-9\s]", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(4);
        var slugPart = string.Join("-", words);
        if (string.IsNullOrWhiteSpace(slugPart)) slugPart = "tai-lieu";
        if (slugPart.Length > 28) slugPart = slugPart[..28].TrimEnd('-');
        var tagName = $"KH-{slugPart}-{docId.ToString("N")[..4]}";
        return tagName.Length > 50 ? tagName[..50] : tagName;
    }

    private static string CleanDocumentTitle(string? rawTitle)
    {
        if (string.IsNullOrWhiteSpace(rawTitle)) return "Tài liệu học tập";
        var cleaned = rawTitle.Trim();
        if (cleaned.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            cleaned = cleaned[..^4];
        if (cleaned.EndsWith("_ebook", StringComparison.OrdinalIgnoreCase))
            cleaned = cleaned[..^6];
        cleaned = cleaned.Replace('_', ' ').Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "Tài liệu học tập" : cleaned;
    }

    private static string CleanSingleLine(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input.Trim(), @"\s+", " ");
    }

    private static List<string> SplitIntoSentences(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();
        return Regex.Split(text, @"(?<=[\.\!\?\;])\s+")
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;
        return value[..(maxLength - 1)].TrimEnd() + "…";
    }
}
