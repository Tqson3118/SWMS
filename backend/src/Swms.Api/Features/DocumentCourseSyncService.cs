using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Database;
using Swms.Api.Infrastructure.Security;

namespace Swms.Api.Features;

public sealed record DocumentCourseSyncReport(
    int SyncedCourses,
    int CreatedCourses,
    int UpdatedCourses,
    int CreatedChapters,
    int CreatedLessons,
    int UpdatedLessons,
    int CreatedQuizzes,
    int UpdatedQuizzes,
    int CreatedFlashcards,
    string RootPath,
    double ElapsedMs);

/// <summary>
/// Deterministically and idempotently synchronizes the 7 curated curriculum folders under
/// DocumentLibrary:RootPath into Studxy's Course/Chapter/Lesson/PracticeQuiz/Flashcard tables,
/// preserving all user learning history (UserLessonProgress, FlashcardReviews, Annotations, Notebooks).
/// </summary>
public static class DocumentCourseSyncService
{
    public const string DefaultDocumentRootPath = @"D:\03_Study\Studxy\document";
    private const int DefaultChunkLimit = 300;

    private static readonly SemaphoreSlim SyncLock = new(1, 1);

    public sealed record CourseDefinition(
        string SourceFolder,
        string Slug,
        string Title,
        string Description,
        string Category,
        int SortPriority);

    public static readonly IReadOnlyList<CourseDefinition> CatalogDefinitions =
    [
        new(
            "DotNet-React-Roadmap",
            "document-course-dotnet-react-roadmap",
            ".NET & React Full-Stack Roadmap",
            "Lộ trình 14 chặng từ C# nền tảng, SQL Server, ASP.NET Core Web API, React TypeScript đến kiến trúc, bảo mật, kiểm thử và triển khai.",
            "Lập trình Full-Stack",
            1),
        new(
            "FullStack-CSharp-React-Refresh",
            "document-course-fullstack-csharp-react-refresh",
            "FullStack C# & React Refresh (28 Tuần)",
            "Hệ thống 28 tuần ôn tập chuyên sâu HTML/CSS/JS, C#, EF Core, ASP.NET Core và React kèm quiz kiểm tra và flashcard ghi nhớ.",
            "Lập trình Full-Stack",
            2),
        new(
            "IELTS",
            "document-course-ielts",
            "IELTS Mastery Roadmap (Phase 0 – Phase 3)",
            "Lộ trình 4 giai đoạn Listening, Reading, Speaking, Writing, Grammar & Vocabulary tích hợp hơn 60 bài nghe WAV bản ngữ.",
            "Ngoại ngữ IELTS",
            3),
        new(
            "Investing",
            "document-course-investing",
            "Đầu Tư Tài Chính Nền Tảng (18 Tuần)",
            "18 tuần xây dựng tư duy đầu tư từ giá trị thời gian của tiền, đọc báo cáo tài chính, định giá đến quản trị danh mục và hành vi.",
            "Tài chính & Đầu tư",
            4),
        new(
            "PersonalFinance",
            "document-course-personalfinance",
            "Tài Chính Cá Nhân Thực Hành (12 Tuần)",
            "12 tuần làm chủ tâm lý tiền bạc, dòng tiền, ngân sách, quỹ dự phòng, bảo hiểm, quản lý nợ, lãi kép và kế hoạch tài chính trọn đời.",
            "Tài chính & Đầu tư",
            5),
        new(
            "LifeSkills",
            "document-course-lifeskills",
            "Kỹ Năng Sống & Giao Tiếp Hiệu Quả (16 Tuần)",
            "16 tuần thực hành tự nhận thức, trí tuệ cảm xúc, lắng nghe chủ động, giao tiếp phi bạo lực, tư duy phản biện và quản lý năng lượng.",
            "Tâm lý & Kỹ năng sống",
            6),
        new(
            "Psychology",
            "document-course-psychology",
            "Tự Học Tâm Lý Học 48 Tuần",
            "Chương trình 48 tuần từ nền tảng khoa học thần kinh, nhận thức, cảm xúc, tâm lý xã hội, tình yêu đến sức khỏe tinh thần.",
            "Tâm lý & Kỹ năng sống",
            7)
    ];

    private static readonly Dictionary<string, CourseDefinition> DefinitionBySlug =
        CatalogDefinitions.ToDictionary(d => d.Slug, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, CourseDefinition> DefinitionByFolder =
        CatalogDefinitions.ToDictionary(d => d.SourceFolder, StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> IgnoredDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "node_modules", "bin", "obj", ".git", ".next", "packages", "__pycache__",
        "_originals_2026-09-22", "quality", "validation", "handoff", "pm-prompt-std",
        "scripts", "templates", "tools", "reference", "tasks", "docs", "data", "activities"
    };

    private static readonly HashSet<string> IgnoredExactFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "REVIEW_REPORT.md", "REVIEW_RULES.md", "AGENTS.md", "source-policy.md",
        "importer-allowlist.json", ".gitkeep", "README-export.md"
    };

    private static readonly HashSet<string> IgnoredExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bak", ".py", ".ps1", ".trx", ".log", ".html", ".htm", ".json", ".yaml", ".yml",
        ".csv", ".txt", ".wav", ".mp3", ".ogg", ".png", ".jpg", ".jpeg", ".svg", ".webp"
    };

    private static readonly HashSet<string> AllowedAssetExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".wav", ".mp3", ".ogg", ".png", ".jpg", ".jpeg", ".svg", ".webp", ".html", ".md", ".txt"
    };

    private static readonly Dictionary<string, string> ModuleTitleOverrides = new(StringComparer.OrdinalIgnoreCase)
    {
        ["module-1"] = "Module 1: Nền tảng cốt lõi",
        ["module-2"] = "Module 2: Hệ thống & Vận hành",
        ["module-3"] = "Module 3: Chuyên sâu & Thực hành",
        ["module-4"] = "Module 4: Ứng dụng & Tổng hợp",
        ["module-5"] = "Module 5: Kiến trúc & Tích hợp",
        ["module-6"] = "Module 6: Bảo mật, Kiểm thử & Tối ưu",
        ["module-7"] = "Module 7: Dự án tổng hợp & Triển khai",
        ["01-scientific-foundations"] = "Chặng 1: Nền tảng khoa học Tâm lý",
        ["02-understanding-self"] = "Chặng 2: Thấu hiểu bản thân (Cảm xúc & Nhận thức)",
        ["03-understanding-others"] = "Chặng 3: Tâm lý học Xã hội & Tương tác",
        ["04-love-relationships"] = "Chặng 4: Gắn bó, Tình yêu & Mối quan hệ",
        ["05-development-mental-health"] = "Chặng 5: Phát triển & Sức khỏe tinh thần",
        ["06-integration"] = "Chặng 6: Tích hợp & Kế hoạch phát triển"
    };

    private sealed record LessonBlueprint(
        string RelativePath,
        string Slug,
        string Title,
        int SortOrder,
        string Markdown,
        string PlainText,
        int EstimatedMinutes,
        int? WeekOrStageNumber);

    private sealed record ChapterBlueprint(
        string Title,
        int SortOrder,
        List<LessonBlueprint> Lessons);

    private sealed record QuizBlueprint(
        string Title,
        string QuizRelativePath,
        string QuizMarkdown,
        string? AnswerKeyMarkdown,
        string TargetLessonRelativePath,
        int? TargetWeekNumber);

    private sealed record FlashcardSpec(
        string Concept,
        string Front,
        string Back,
        string TargetLessonRelativePath,
        int? TargetWeekNumber,
        string DeckKey);

    private sealed record CourseBlueprint(
        CourseDefinition Definition,
        List<ChapterBlueprint> Chapters,
        List<QuizBlueprint> Quizzes,
        List<FlashcardSpec> Flashcards);

    private sealed record RootSnapshot(
        string NormalizedRootPath,
        List<CourseBlueprint> Courses);

    private static readonly ConcurrentDictionary<string, RootSnapshot> SnapshotCache = new(StringComparer.OrdinalIgnoreCase);

    public static string ResolveDocumentRootPath(IConfiguration? config = null)
    {
        var configured = config?["DocumentLibrary:RootPath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.Trim();
        }
        return DefaultDocumentRootPath;
    }

    public static int GetSortPriority(string? courseSlug)
    {
        if (!string.IsNullOrWhiteSpace(courseSlug) && DefinitionBySlug.TryGetValue(courseSlug, out var def))
        {
            return def.SortPriority;
        }
        return 999;
    }

    public static (string? SourceFolder, string Category) ResolveCourseMetadata(string? courseSlug, string? title = null, string? astSourceFolder = null)
    {
        if (!string.IsNullOrWhiteSpace(courseSlug) && DefinitionBySlug.TryGetValue(courseSlug, out var def))
        {
            return (def.SourceFolder, def.Category);
        }

        if (!string.IsNullOrWhiteSpace(astSourceFolder) && DefinitionByFolder.TryGetValue(astSourceFolder, out var byFolder))
        {
            return (byFolder.SourceFolder, byFolder.Category);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            if (title.Contains("Hệ điều hành", StringComparison.OrdinalIgnoreCase) ||
                title.Contains("Paging", StringComparison.OrdinalIgnoreCase))
            {
                return (null, "Khoa học máy tính");
            }
            if (title.Contains("Tài Chính Cá Nhân", StringComparison.OrdinalIgnoreCase))
            {
                return ("PersonalFinance", "Tài chính & Đầu tư");
            }
        }

        return (astSourceFolder, "Khóa học của tôi");
    }

    public static string? GetAssetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".wav" => "audio/wav",
            ".mp3" => "audio/mpeg",
            ".ogg" => "audio/ogg",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".html" => "text/html; charset=utf-8",
            ".md" => "text/markdown; charset=utf-8",
            ".txt" => "text/plain; charset=utf-8",
            _ => null
        };
    }

    public static bool TryResolveSafeAssetFile(
        string rootPath,
        string courseFolder,
        string relativeAssetPath,
        out string fullPath,
        out string contentType,
        out string normalizedRelativePath,
        out int failureStatusCode,
        out string? failureMessage)
    {
        fullPath = string.Empty;
        contentType = "application/octet-stream";
        normalizedRelativePath = string.Empty;
        failureStatusCode = StatusCodes.Status400BadRequest;
        failureMessage = null;

        if (string.IsNullOrWhiteSpace(courseFolder) ||
            courseFolder.Contains("..", StringComparison.Ordinal) ||
            courseFolder.Contains('/') ||
            courseFolder.Contains('\\') ||
            !DefinitionByFolder.ContainsKey(courseFolder))
        {
            failureStatusCode = StatusCodes.Status403Forbidden;
            failureMessage = "Thư mục khóa học không hợp lệ hoặc không được phép truy cập.";
            return false;
        }

        if (!TryNormalizeRelativePath(null, relativeAssetPath, out normalizedRelativePath, out _))
        {
            failureStatusCode = StatusCodes.Status403Forbidden;
            failureMessage = "Đường dẫn tài nguyên không hợp lệ (nghi ngờ path traversal).";
            return false;
        }

        var ext = Path.GetExtension(normalizedRelativePath);
        var mime = GetAssetContentType(ext);
        if (mime == null || !AllowedAssetExtensions.Contains(ext))
        {
            failureStatusCode = StatusCodes.Status403Forbidden;
            failureMessage = "Định dạng tài nguyên không nằm trong danh sách cho phép.";
            return false;
        }

        var fullRoot = Path.GetFullPath(rootPath);
        var courseRoot = Path.GetFullPath(Path.Combine(fullRoot, courseFolder));
        var candidate = Path.GetFullPath(Path.Combine(courseRoot, normalizedRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var courseRootPrefix = courseRoot.EndsWith(Path.DirectorySeparatorChar)
            ? courseRoot
            : courseRoot + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(courseRootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            failureStatusCode = StatusCodes.Status403Forbidden;
            failureMessage = "Truy cập ngoài phạm vi thư mục khóa học bị từ chối.";
            return false;
        }

        if (!File.Exists(candidate))
        {
            failureStatusCode = StatusCodes.Status404NotFound;
            failureMessage = "Không tìm thấy tệp tài nguyên.";
            return false;
        }

        fullPath = candidate;
        contentType = mime;
        failureStatusCode = StatusCodes.Status200OK;
        return true;
    }

    public static bool TryNormalizeRelativePath(
        string? currentLessonRelativePath,
        string requestedPath,
        out string normalizedRelativePath,
        out string? fragmentAnchor)
    {
        normalizedRelativePath = string.Empty;
        fragmentAnchor = null;

        if (string.IsNullOrWhiteSpace(requestedPath))
        {
            return false;
        }

        var raw = requestedPath.Trim();
        var hashIdx = raw.IndexOf('#');
        if (hashIdx >= 0)
        {
            fragmentAnchor = raw[(hashIdx + 1)..].Trim();
            raw = raw[..hashIdx];
        }

        var queryIdx = raw.IndexOf('?');
        if (queryIdx >= 0)
        {
            raw = raw[..queryIdx];
        }

        raw = Uri.UnescapeDataString(raw).Trim().Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(raw) || raw.Contains('\0') || raw.Contains(':'))
        {
            return false;
        }

        var segments = new List<string>();
        bool isRelativeToCurrent = raw.StartsWith("./", StringComparison.Ordinal) || raw.StartsWith("../", StringComparison.Ordinal);
        if (isRelativeToCurrent && !string.IsNullOrWhiteSpace(currentLessonRelativePath))
        {
            var baseDir = currentLessonRelativePath.Replace('\\', '/');
            var lastSlash = baseDir.LastIndexOf('/');
            if (lastSlash > 0)
            {
                foreach (var baseSeg in baseDir[..lastSlash].Split('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    segments.Add(baseSeg);
                }
            }
        }

        foreach (var part in raw.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part == ".") continue;
            if (part == "..")
            {
                if (segments.Count == 0)
                {
                    return false;
                }
                segments.RemoveAt(segments.Count - 1);
                continue;
            }
            segments.Add(part);
        }

        if (segments.Count == 0)
        {
            return false;
        }

        normalizedRelativePath = string.Join('/', segments);
        return true;
    }

    public static async Task<DocumentCourseSyncReport> SyncAllFromDocumentsAsync(
        SwmsDbContext db,
        string? rootPath = null,
        bool useSnapshotCache = true,
        CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var effectiveRoot = string.IsNullOrWhiteSpace(rootPath) ? DefaultDocumentRootPath : rootPath.Trim();

        if (!Directory.Exists(effectiveRoot))
        {
            return new DocumentCourseSyncReport(0, 0, 0, 0, 0, 0, 0, 0, 0, effectiveRoot, sw.Elapsed.TotalMilliseconds);
        }

        await SyncLock.WaitAsync(ct);
        try
        {
            var snapshot = LoadSnapshot(effectiveRoot, useSnapshotCache);
            var instructorId = await EnsureDefaultInstructorExistsAsync(db, ct);

            int createdCourses = 0;
            int updatedCourses = 0;
            int createdChapters = 0;
            int createdLessons = 0;
            int updatedLessons = 0;
            int createdQuizzes = 0;
            int updatedQuizzes = 0;
            int createdFlashcards = 0;

            var now = DateTimeOffset.UtcNow;

            foreach (var bp in snapshot.Courses)
            {
                ct.ThrowIfCancellationRequested();

                var course = await FindOrUpgradeCourseAsync(db, bp.Definition, instructorId, now, ct,
                    isNew =>
                    {
                        if (isNew) createdCourses++;
                        else updatedCourses++;
                    });

                var existingChapters = await db.Chapters
                    .Where(ch => ch.CourseId == course.Id)
                    .ToListAsync(ct);
                var existingChapterIds = existingChapters.Select(ch => ch.Id).ToList();

                var existingLessons = existingChapterIds.Count > 0
                    ? await db.Lessons.Where(l => existingChapterIds.Contains(l.ChapterId)).ToListAsync(ct)
                    : new List<Lesson>();

                var lessonBySlug = new Dictionary<string, Lesson>(StringComparer.OrdinalIgnoreCase);
                var lessonByRelPath = new Dictionary<string, Lesson>(StringComparer.OrdinalIgnoreCase);
                foreach (var exLesson in existingLessons)
                {
                    lessonBySlug.TryAdd(exLesson.Slug, exLesson);
                    var (_, relPath) = CourseGenerationService.ExtractLessonSourceMetadata(exLesson.ContentAstJson);
                    if (!string.IsNullOrWhiteSpace(relPath))
                    {
                        lessonByRelPath.TryAdd(relPath, exLesson);
                    }
                }

                // Legacy migration helper for unmigrated PersonalFinance lessons: match by week number if needed
                var unmatchedLegacyLessons = existingLessons
                    .Where(l => string.IsNullOrWhiteSpace(CourseGenerationService.ExtractLessonSourceMetadata(l.ContentAstJson).SourceRelativePath))
                    .ToList();

                var chapterMapByTitle = existingChapters.ToDictionary(c => c.Title, StringComparer.OrdinalIgnoreCase);
                var chapterMapByOrder = existingChapters.ToDictionary(c => c.SortOrder);
                var syncedLessonMap = new Dictionary<string, Lesson>(StringComparer.OrdinalIgnoreCase);
                var syncedByWeekMap = new Dictionary<int, Lesson>();
                Lesson? firstLesson = null;

                foreach (var chBp in bp.Chapters)
                {
                    if (!chapterMapByTitle.TryGetValue(chBp.Title, out var chapter))
                    {
                        if (chapterMapByOrder.TryGetValue(chBp.SortOrder, out var byOrder))
                        {
                            chapter = byOrder;
                            chapter.Title = chBp.Title;
                            chapter.UpdatedAt = now;
                        }
                        else
                        {
                            chapter = new Chapter
                            {
                                Id = Guid.NewGuid(),
                                CourseId = course.Id,
                                Title = chBp.Title,
                                SortOrder = chBp.SortOrder,
                                CreatedAt = now,
                                UpdatedAt = now
                            };
                            db.Chapters.Add(chapter);
                            createdChapters++;
                        }
                        chapterMapByTitle[chapter.Title] = chapter;
                        chapterMapByOrder[chapter.SortOrder] = chapter;
                    }

                    foreach (var lBp in chBp.Lessons)
                    {
                        Lesson? target = null;
                        if (lessonByRelPath.TryGetValue(lBp.RelativePath, out var byPath))
                        {
                            target = byPath;
                        }
                        else if (lessonBySlug.TryGetValue(lBp.Slug, out var bySlug))
                        {
                            target = bySlug;
                        }
                        else if (lBp.WeekOrStageNumber.HasValue)
                        {
                            target = TryConsumeLegacyLessonByWeek(unmatchedLegacyLessons, lBp.WeekOrStageNumber.Value);
                        }

                        var (astJson, plainText) = CourseWizardAndImportService.BuildMarkdownAstAndPlainText(
                            course.Id,
                            lBp.Title,
                            lBp.Markdown,
                            bp.Definition.SourceFolder,
                            lBp.RelativePath);

                        if (target == null)
                        {
                            target = new Lesson
                            {
                                Id = Guid.NewGuid(),
                                ChapterId = chapter.Id,
                                Title = Truncate(lBp.Title, 250),
                                Slug = Truncate(lBp.Slug, 250),
                                SortOrder = lBp.SortOrder,
                                CurrentVersion = 1,
                                ContentAstJson = astJson,
                                PlainText = plainText,
                                ContentMarkdown = lBp.Markdown,
                                EstimatedMinutes = lBp.EstimatedMinutes,
                                IsPublished = true,
                                CreatedAt = now,
                                UpdatedAt = now
                            };
                            db.Lessons.Add(target);
                            createdLessons++;
                        }
                        else
                        {
                            bool changed =
                                target.ChapterId != chapter.Id ||
                                !string.Equals(target.Title, Truncate(lBp.Title, 250), StringComparison.Ordinal) ||
                                !string.Equals(target.Slug, Truncate(lBp.Slug, 250), StringComparison.Ordinal) ||
                                target.SortOrder != lBp.SortOrder ||
                                !string.Equals(target.ContentMarkdown, lBp.Markdown, StringComparison.Ordinal) ||
                                string.IsNullOrWhiteSpace(CourseGenerationService.ExtractLessonSourceMetadata(target.ContentAstJson).SourceFolder);

                            if (changed)
                            {
                                target.ChapterId = chapter.Id;
                                target.Title = Truncate(lBp.Title, 250);
                                target.Slug = Truncate(lBp.Slug, 250);
                                target.SortOrder = lBp.SortOrder;
                                target.ContentMarkdown = lBp.Markdown;
                                target.PlainText = plainText;
                                target.ContentAstJson = astJson;
                                target.EstimatedMinutes = lBp.EstimatedMinutes;
                                target.IsPublished = true;
                                target.UpdatedAt = now;
                                updatedLessons++;
                            }
                        }

                        firstLesson ??= target;
                        syncedLessonMap[lBp.RelativePath] = target;
                        if (lBp.WeekOrStageNumber.HasValue)
                        {
                            syncedByWeekMap.TryAdd(lBp.WeekOrStageNumber.Value, target);
                        }
                    }
                }

                await ReconcileDuplicateLegacyLessonsAsync(
                    db,
                    course.Id,
                    existingChapters,
                    existingLessons,
                    syncedLessonMap.Values.ToList(),
                    syncedByWeekMap,
                    now,
                    ct);

                var courseTag = await EnsureCourseTagAsync(db, instructorId, bp.Definition.Title, now, ct);

                // Sync Quizzes matched to their canonical lessons
                var courseLessonIds = syncedLessonMap.Values.Select(l => l.Id).ToHashSet();
                var existingQuizzes = courseLessonIds.Count > 0
                    ? await db.PracticeQuizzes
                        .Where(q => q.UserId == instructorId && q.LessonId.HasValue && courseLessonIds.Contains(q.LessonId.Value))
                        .ToListAsync(ct)
                    : new List<PracticeQuiz>();

                var existingQuizIds = existingQuizzes.Select(q => q.Id).ToList();
                var existingItemCounts = existingQuizIds.Count > 0
                    ? await db.PracticeQuizItems
                        .Where(i => existingQuizIds.Contains(i.QuizId))
                        .GroupBy(i => i.QuizId)
                        .Select(g => new { QuizId = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.QuizId, x => x.Count, ct)
                    : new Dictionary<Guid, int>();

                var quizByKey = new Dictionary<string, PracticeQuiz>(StringComparer.OrdinalIgnoreCase);
                foreach (var q in existingQuizzes)
                {
                    if (q.LessonId.HasValue)
                    {
                        quizByKey.TryAdd($"{q.LessonId.Value:N}|{q.Title}", q);
                        quizByKey.TryAdd($"{q.LessonId.Value:N}", q);
                    }
                }

                foreach (var qBp in bp.Quizzes)
                {
                    var matchedLesson = ResolveTargetLesson(
                        qBp.TargetLessonRelativePath,
                        qBp.TargetWeekNumber,
                        syncedLessonMap,
                        syncedByWeekMap,
                        firstLesson);

                    if (matchedLesson == null) continue;

                    var qTitle = Truncate(qBp.Title, 250);
                    quizByKey.TryGetValue($"{matchedLesson.Id:N}|{qTitle}", out var existingQuiz);
                    existingQuiz ??= quizByKey.GetValueOrDefault($"{matchedLesson.Id:N}");

                    if (existingQuiz == null)
                    {
                        var quizId = Guid.NewGuid();
                        var parsedItems = CourseWizardAndImportService.ParseQuizMarkdown(
                            quizId,
                            qBp.QuizMarkdown,
                            qBp.AnswerKeyMarkdown,
                            now);

                        if (parsedItems.Count == 0) continue;

                        var newQuiz = new PracticeQuiz
                        {
                            Id = quizId,
                            UserId = instructorId,
                            LessonId = matchedLesson.Id,
                            Title = qTitle,
                            TotalItems = parsedItems.Count,
                            CorrectItems = 0,
                            Status = "not_started",
                            StartedAt = now
                        };
                        db.PracticeQuizzes.Add(newQuiz);
                        db.PracticeQuizItems.AddRange(parsedItems);
                        quizByKey[$"{matchedLesson.Id:N}|{qTitle}"] = newQuiz;
                        quizByKey.TryAdd($"{matchedLesson.Id:N}", newQuiz);
                        createdQuizzes++;
                    }
                    else if (existingItemCounts.GetValueOrDefault(existingQuiz.Id, 0) == 0)
                    {
                        var parsedItems = CourseWizardAndImportService.ParseQuizMarkdown(
                            existingQuiz.Id,
                            qBp.QuizMarkdown,
                            qBp.AnswerKeyMarkdown,
                            now);

                        if (parsedItems.Count > 0)
                        {
                            db.PracticeQuizItems.AddRange(parsedItems);
                            existingQuiz.TotalItems = parsedItems.Count;
                            existingQuiz.Title = qTitle;
                            updatedQuizzes++;
                        }
                    }
                }

                // Sync Flashcards matched to their canonical lessons without duplicating existing cards or losing FSRS reviews
                if (bp.Flashcards.Count > 0 && courseLessonIds.Count > 0)
                {
                    var existingCards = await db.Flashcards
                        .Where(f => f.UserId == instructorId && !f.IsArchived && f.LessonId.HasValue && courseLessonIds.Contains(f.LessonId.Value))
                        .ToListAsync(ct);

                    var existingCardIds = existingCards.Select(c => c.Id).ToHashSet();
                    var existingTagLinks = existingCardIds.Count > 0
                        ? await db.FlashcardTags
                            .Where(ft => ft.TagId == courseTag.Id && existingCardIds.Contains(ft.FlashcardId))
                            .Select(ft => ft.FlashcardId)
                            .ToHashSetAsync(ct)
                        : new HashSet<Guid>();

                    var cardsByFront = new Dictionary<string, Flashcard>(StringComparer.Ordinal);
                    foreach (var exCard in existingCards)
                    {
                        var fText = ExtractTextFromFrontAst(exCard.FrontAstJson);
                        if (!string.IsNullOrEmpty(fText))
                        {
                            cardsByFront.TryAdd(fText, exCard);
                        }
                        if (!existingTagLinks.Contains(exCard.Id))
                        {
                            db.FlashcardTags.Add(new FlashcardTag { FlashcardId = exCard.Id, TagId = courseTag.Id });
                            existingTagLinks.Add(exCard.Id);
                        }
                    }

                    foreach (var fc in bp.Flashcards)
                    {
                        var matchedLesson = ResolveTargetLesson(
                            fc.TargetLessonRelativePath,
                            fc.TargetWeekNumber,
                            syncedLessonMap,
                            syncedByWeekMap,
                            firstLesson);

                        if (matchedLesson == null) continue;

                        if (cardsByFront.TryGetValue(fc.Front, out var existing))
                        {
                            // Upgrade fallback lesson link to exact matched lesson without touching FSRS review state
                            if (existing.LessonId != matchedLesson.Id && firstLesson != null && existing.LessonId == firstLesson.Id)
                            {
                                existing.LessonId = matchedLesson.Id;
                                existing.UpdatedAt = now;
                            }
                            continue;
                        }

                        var cardId = Guid.NewGuid();
                        var newCard = new Flashcard
                        {
                            Id = cardId,
                            UserId = instructorId,
                            LessonId = matchedLesson.Id,
                            CardType = "standard",
                            SourceTitleSnapshot = Truncate(matchedLesson.Title, 250),
                            SourceQuoteSnapshot = string.IsNullOrWhiteSpace(fc.Concept) ? null : Truncate(fc.Concept, 500),
                            FrontAstJson = JsonSerializer.Serialize(new { text = fc.Front }),
                            BackAstJson = JsonSerializer.Serialize(new { text = fc.Back }),
                            Stability = 0,
                            Difficulty = 0,
                            Reps = 0,
                            Lapses = 0,
                            State = 0,
                            DueAt = now,
                            CreatedAt = now,
                            UpdatedAt = now
                        };

                        db.Flashcards.Add(newCard);
                        db.FlashcardTags.Add(new FlashcardTag { FlashcardId = cardId, TagId = courseTag.Id });
                        cardsByFront[fc.Front] = newCard;
                        createdFlashcards++;
                    }
                }
            }

            await db.SaveChangesAsync(ct);

            return new DocumentCourseSyncReport(
                snapshot.Courses.Count,
                createdCourses,
                updatedCourses,
                createdChapters,
                createdLessons,
                updatedLessons,
                createdQuizzes,
                updatedQuizzes,
                createdFlashcards,
                effectiveRoot,
                Math.Round(sw.Elapsed.TotalMilliseconds, 1));
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private static async Task ReconcileDuplicateLegacyLessonsAsync(
        SwmsDbContext db,
        Guid courseId,
        List<Chapter> existingChapters,
        List<Lesson> existingLessons,
        List<Lesson> syncedLessons,
        Dictionary<int, Lesson> syncedByWeekMap,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var syncedIds = syncedLessons.Select(l => l.Id).ToHashSet();
        var orphanLessons = existingLessons
            .Where(l => !syncedIds.Contains(l.Id) &&
                        string.IsNullOrWhiteSpace(CourseGenerationService.ExtractLessonSourceMetadata(l.ContentAstJson).SourceRelativePath))
            .ToList();

        if (orphanLessons.Count == 0 || syncedLessons.Count == 0)
        {
            return;
        }

        // Persist any new canonical lessons/chapters before re-pointing FKs from duplicate legacy lessons
        await db.SaveChangesAsync(ct);

        var removedLessonIds = new HashSet<Guid>();
        foreach (var orphan in orphanLessons)
        {
            Lesson? canonical = null;
            var weekNum = ExtractWeekOrStageNumber(orphan.Slug) ?? ExtractWeekOrStageNumber(orphan.Title);
            if (weekNum.HasValue)
            {
                syncedByWeekMap.TryGetValue(weekNum.Value, out canonical);
            }

            if (canonical == null)
            {
                var normOrphanTitle = RemoveDiacritics(orphan.Title).ToLowerInvariant();
                canonical = syncedLessons.FirstOrDefault(sl =>
                    RemoveDiacritics(sl.Title).Equals(normOrphanTitle, StringComparison.OrdinalIgnoreCase));
            }

            if (canonical == null || canonical.Id == orphan.Id) continue;

            // 1. UserLessonProgresses
            var orphanProgress = await db.UserLessonProgresses.Where(p => p.LessonId == orphan.Id).ToListAsync(ct);
            foreach (var p in orphanProgress)
            {
                var alreadyCompleted = await db.UserLessonProgresses
                    .AnyAsync(x => x.UserId == p.UserId && x.LessonId == canonical.Id, ct);
                if (alreadyCompleted)
                {
                    db.UserLessonProgresses.Remove(p);
                }
                else
                {
                    p.LessonId = canonical.Id;
                }
            }

            // 2. Annotations
            var orphanAnns = await db.Annotations.Where(a => a.LessonId == orphan.Id).ToListAsync(ct);
            foreach (var a in orphanAnns)
            {
                a.LessonId = canonical.Id;
                a.UpdatedAt = now;
            }

            // 3. Notebooks & NotebookLinks
            var orphanNotebooks = await db.Notebooks.Where(n => n.PrimarySourceLessonId == orphan.Id).ToListAsync(ct);
            foreach (var n in orphanNotebooks)
            {
                n.PrimarySourceLessonId = canonical.Id;
                n.UpdatedAt = now;
            }
            var orphanLinks = await db.NotebookLinks.Where(nl => nl.RefLessonId == orphan.Id).ToListAsync(ct);
            foreach (var nl in orphanLinks)
            {
                nl.RefLessonId = canonical.Id;
            }

            // 4. Flashcards (preserve all reviewed cards; deduplicate only unreviewed exact duplicates)
            var orphanCards = await db.Flashcards.Where(f => f.LessonId == orphan.Id).ToListAsync(ct);
            var canonicalCardFronts = await db.Flashcards
                .Where(f => f.LessonId == canonical.Id && !f.IsArchived)
                .Select(f => new { f.UserId, f.FrontAstJson })
                .ToListAsync(ct);

            foreach (var card in orphanCards)
            {
                bool hasDuplicateOnCanonical = canonicalCardFronts.Any(c =>
                    c.UserId == card.UserId && string.Equals(c.FrontAstJson, card.FrontAstJson, StringComparison.Ordinal));
                bool hasReviews = await db.FlashcardReviews.AnyAsync(r => r.FlashcardId == card.Id, ct);

                if (hasDuplicateOnCanonical && !hasReviews)
                {
                    var cardTags = await db.FlashcardTags.Where(ft => ft.FlashcardId == card.Id).ToListAsync(ct);
                    db.FlashcardTags.RemoveRange(cardTags);
                    db.Flashcards.Remove(card);
                }
                else
                {
                    card.LessonId = canonical.Id;
                    card.UpdatedAt = now;
                }
            }

            // 5. PracticeQuizzes
            var orphanQuizzes = await db.PracticeQuizzes.Where(q => q.LessonId == orphan.Id).ToListAsync(ct);
            foreach (var q in orphanQuizzes)
            {
                bool canonicalHasQuiz = await db.PracticeQuizzes.AnyAsync(x => x.LessonId == canonical.Id && x.UserId == q.UserId, ct);
                bool hasUserAttempt = await db.PracticeQuizItems.AnyAsync(i => i.QuizId == q.Id && i.EvaluatedAt != null, ct);

                if (canonicalHasQuiz && !hasUserAttempt)
                {
                    var qItems = await db.PracticeQuizItems.Where(i => i.QuizId == q.Id).ToListAsync(ct);
                    db.PracticeQuizItems.RemoveRange(qItems);
                    db.PracticeQuizzes.Remove(q);
                }
                else
                {
                    q.LessonId = canonical.Id;
                }
            }

            // 6. LessonVersions on orphan shell
            var orphanVersions = await db.LessonVersions.Where(v => v.LessonId == orphan.Id).ToListAsync(ct);
            db.LessonVersions.RemoveRange(orphanVersions);

            await db.SaveChangesAsync(ct);
            db.Lessons.Remove(orphan);
            removedLessonIds.Add(orphan.Id);
        }

        if (removedLessonIds.Count > 0)
        {
            await db.SaveChangesAsync(ct);

            // Clean up any empty chapters left behind after merging legacy duplicate lessons
            var courseChapters = await db.Chapters.Where(ch => ch.CourseId == courseId).ToListAsync(ct);
            foreach (var ch in courseChapters)
            {
                bool hasAnyLesson = await db.Lessons.AnyAsync(l => l.ChapterId == ch.Id, ct);
                if (!hasAnyLesson)
                {
                    db.Chapters.Remove(ch);
                }
            }
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task<Guid> EnsureDefaultInstructorExistsAsync(SwmsDbContext db, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == SeedDataInitializer.DefaultUserId, ct);
        if (user != null) return user.Id;

        var users = await db.Users.ToListAsync(ct);
        var fallback = users.OrderBy(u => u.CreatedAt).FirstOrDefault();
        if (fallback != null) return fallback.Id;

        var now = DateTimeOffset.UtcNow;
        var created = new User
        {
            Id = SeedDataInitializer.DefaultUserId,
            Email = "son.tran@studxy.vn",
            PasswordHash = PasswordHashUtility.HashPassword("Studxy@2026"),
            FullName = "Sơn Trần",
            Role = "student",
            TokenVersion = 1,
            StreakDays = 7,
            OnboardingStep = 2,
            IsActivated = false,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Users.Add(created);
        await db.SaveChangesAsync(ct);
        return created.Id;
    }

    private static async Task<Course> FindOrUpgradeCourseAsync(
        SwmsDbContext db,
        CourseDefinition def,
        Guid instructorId,
        DateTimeOffset now,
        CancellationToken ct,
        Action<bool> onTrack)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Slug == def.Slug, ct);
        if (course == null && def.SourceFolder.Equals("PersonalFinance", StringComparison.OrdinalIgnoreCase))
        {
            // Upgrade legacy seeded PersonalFinance course (from personal-finance-course.zip) cleanly in place
            course = await db.Courses.FirstOrDefaultAsync(
                c => c.InstructorId == instructorId &&
                     c.Title.Contains("Tài Chính Cá Nhân") &&
                     !c.Slug.StartsWith("document-course-"),
                ct);
            if (course != null)
            {
                course.Slug = def.Slug;
            }
        }

        if (course == null)
        {
            course = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = instructorId,
                Title = def.Title,
                Slug = def.Slug,
                Description = def.Description,
                Visibility = "public",
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Courses.Add(course);
            db.CourseEnrollments.Add(new CourseEnrollment
            {
                Id = Guid.NewGuid(),
                UserId = instructorId,
                CourseId = course.Id,
                Role = "instructor",
                EnrolledAt = now
            });
            await db.SaveChangesAsync(ct);
            onTrack(true);
            return course;
        }

        bool metadataChanged =
            !string.Equals(course.Title, def.Title, StringComparison.Ordinal) ||
            !string.Equals(course.Description, def.Description, StringComparison.Ordinal) ||
            course.Visibility != "public" ||
            !course.IsPublished;

        if (metadataChanged)
        {
            course.Title = def.Title;
            course.Description = def.Description;
            course.Visibility = "public";
            course.IsPublished = true;
            course.UpdatedAt = now;
        }

        var enrolled = await db.CourseEnrollments.AnyAsync(e => e.UserId == instructorId && e.CourseId == course.Id, ct);
        if (!enrolled)
        {
            db.CourseEnrollments.Add(new CourseEnrollment
            {
                Id = Guid.NewGuid(),
                UserId = instructorId,
                CourseId = course.Id,
                Role = "instructor",
                EnrolledAt = now
            });
        }

        onTrack(false);
        return course;
    }

    private static async Task<Tag> EnsureCourseTagAsync(
        SwmsDbContext db,
        Guid userId,
        string courseTitle,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var tagName = Truncate(courseTitle, 100);
        var existing = await db.Tags.FirstOrDefaultAsync(t => t.UserId == userId && t.Name == tagName, ct);
        if (existing != null) return existing;

        var created = new Tag
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = tagName,
            ColorKey = "#4F46E5",
            CreatedAt = now
        };
        db.Tags.Add(created);
        return created;
    }

    private static Lesson? TryConsumeLegacyLessonByWeek(List<Lesson> unmatchedLegacyLessons, int weekNumber)
    {
        var idx = unmatchedLegacyLessons.FindIndex(l =>
        {
            var extracted = ExtractWeekOrStageNumber(l.Slug) ?? ExtractWeekOrStageNumber(l.Title);
            return extracted == weekNumber;
        });
        if (idx < 0) return null;
        var matched = unmatchedLegacyLessons[idx];
        unmatchedLegacyLessons.RemoveAt(idx);
        return matched;
    }

    private static Lesson? ResolveTargetLesson(
        string? targetRelPath,
        int? weekNum,
        Dictionary<string, Lesson> byRelPath,
        Dictionary<int, Lesson> byWeek,
        Lesson? fallback)
    {
        if (!string.IsNullOrWhiteSpace(targetRelPath))
        {
            var normalized = targetRelPath.Replace('\\', '/');
            if (byRelPath.TryGetValue(normalized, out var exact))
            {
                return exact;
            }
            var fileName = Path.GetFileName(normalized);
            var suffixMatch = byRelPath.FirstOrDefault(kv =>
                Path.GetFileName(kv.Key).Equals(fileName, StringComparison.OrdinalIgnoreCase)).Value;
            if (suffixMatch != null) return suffixMatch;
        }

        if (weekNum.HasValue && byWeek.TryGetValue(weekNum.Value, out var byWk))
        {
            return byWk;
        }

        return fallback;
    }

    private static string ExtractTextFromFrontAst(string frontAstJson)
    {
        if (string.IsNullOrWhiteSpace(frontAstJson)) return string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(frontAstJson);
            if (doc.RootElement.TryGetProperty("text", out var t))
            {
                return t.GetString() ?? string.Empty;
            }
        }
        catch { }
        return frontAstJson;
    }

    private static RootSnapshot LoadSnapshot(string rootPath, bool useCache)
    {
        var normalizedRoot = Path.GetFullPath(rootPath);
        if (useCache && SnapshotCache.TryGetValue(normalizedRoot, out var cached))
        {
            return cached;
        }

        var courses = new List<CourseBlueprint>();
        foreach (var def in CatalogDefinitions)
        {
            var folderPath = Path.Combine(normalizedRoot, def.SourceFolder);
            if (!Directory.Exists(folderPath)) continue;

            CourseBlueprint bp = def.SourceFolder switch
            {
                "DotNet-React-Roadmap" => BuildDotNetRoadmapBlueprint(def, folderPath),
                "IELTS" => BuildIeltsBlueprint(def, folderPath),
                _ => BuildManifestDrivenBlueprint(def, folderPath)
            };

            if (bp.Chapters.Count > 0)
            {
                courses.Add(bp);
            }
        }

        var snapshot = new RootSnapshot(normalizedRoot, courses);
        SnapshotCache[normalizedRoot] = snapshot;
        return snapshot;
    }

    private static CourseBlueprint BuildDotNetRoadmapBlueprint(CourseDefinition def, string courseRoot)
    {
        var introLessons = new List<LessonBlueprint>();
        var stage1To4 = new List<LessonBlueprint>();
        var stage5To8 = new List<LessonBlueprint>();
        var stage9To14 = new List<LessonBlueprint>();

        var startDocx = Path.Combine(courseRoot, "00_Bat_Dau.docx");
        if (File.Exists(startDocx))
        {
            var bytes = File.ReadAllBytes(startDocx);
            var extracted = CourseWizardAndImportService.ExtractTextFromBytes(bytes, ".docx");
            if (!string.IsNullOrWhiteSpace(extracted))
            {
                var md = $"# Hướng dẫn bắt đầu (.NET & React Full-Stack)\n\n{extracted}";
                introLessons.Add(CreateLessonBlueprint("00_Bat_Dau.docx", "00_Bat_Dau.docx", "Hướng dẫn bắt đầu lộ trình (.NET & React)", 1, md, 0));
            }
        }

        var readmePath = Path.Combine(courseRoot, "README.md");
        if (File.Exists(readmePath))
        {
            var md = File.ReadAllText(readmePath, Encoding.UTF8);
            var title = ExtractMarkdownTitle(md, "Tổng quan lộ trình 14 chặng");
            introLessons.Add(CreateLessonBlueprint("README.md", "README.md", title, 2, md, null));
        }

        var manifestTitles = new Dictionary<int, string>();
        foreach (var manifestFile in Directory.GetFiles(courseRoot, "manifest-stage-*.yaml"))
        {
            var stageNum = ExtractWeekOrStageNumber(Path.GetFileName(manifestFile));
            if (!stageNum.HasValue) continue;
            var yamlText = File.ReadAllText(manifestFile, Encoding.UTF8);
            var unitTitle = ExtractYamlScalarValue(yamlText, "display_title");
            if (!string.IsNullOrWhiteSpace(unitTitle))
            {
                manifestTitles[stageNum.Value] = unitTitle;
            }
        }

        for (int stage = 1; stage <= 14; stage++)
        {
            var mdFile = Path.Combine(courseRoot, $"{stage}.md");
            if (!File.Exists(mdFile)) continue;

            var md = File.ReadAllText(mdFile, Encoding.UTF8);
            var title = ExtractMarkdownTitle(md, string.Empty);
            if (string.IsNullOrWhiteSpace(title))
            {
                title = manifestTitles.TryGetValue(stage, out var mt) ? mt : $"Chặng {stage}";
            }
            var lesson = CreateLessonBlueprint($"{stage}.md", $"{stage}.md", title, stage, md, stage);

            if (stage <= 4) stage1To4.Add(lesson);
            else if (stage <= 8) stage5To8.Add(lesson);
            else stage9To14.Add(lesson);
        }

        var chapters = new List<ChapterBlueprint>();
        if (introLessons.Count > 0) chapters.Add(new("Chặng 0: Bắt đầu & Tổng quan lộ trình", 1, introLessons));
        if (stage1To4.Count > 0) chapters.Add(new("Chặng 1–4: Nền tảng C#, Debug & SQL", 2, stage1To4));
        if (stage5To8.Count > 0) chapters.Add(new("Chặng 5–8: ASP.NET Core Web API & Frontend React", 3, stage5To8));
        if (stage9To14.Count > 0) chapters.Add(new("Chặng 9–14: Kiến trúc, Bảo mật, Kiểm thử & Triển khai", 4, stage9To14));

        return new CourseBlueprint(def, chapters, [], []);
    }

    private static CourseBlueprint BuildIeltsBlueprint(CourseDefinition def, string courseRoot)
    {
        var overviewLessons = new List<LessonBlueprint>();
        var phase0Lessons = new List<LessonBlueprint>();
        var phase1Lessons = new List<LessonBlueprint>();
        var phase2Lessons = new List<LessonBlueprint>();
        var phase3Lessons = new List<LessonBlueprint>();

        foreach (var file in Directory.EnumerateFiles(courseRoot, "*.md", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(courseRoot, file).Replace('\\', '/');
            if (IsIgnoredRelativePath(rel, allowStartDocx: false)) continue;

            var md = File.ReadAllText(file, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(md)) continue;

            var fallbackTitle = Path.GetFileNameWithoutExtension(rel);
            var title = ExtractMarkdownTitle(md, fallbackTitle);
            var bp = CreateLessonBlueprint(rel, rel, title, 0, md, null);

            if (rel.StartsWith("Phase-0/", StringComparison.OrdinalIgnoreCase)) phase0Lessons.Add(bp);
            else if (rel.StartsWith("Phase-1/", StringComparison.OrdinalIgnoreCase)) phase1Lessons.Add(bp);
            else if (rel.StartsWith("Phase-2/", StringComparison.OrdinalIgnoreCase)) phase2Lessons.Add(bp);
            else if (rel.StartsWith("Phase-3/", StringComparison.OrdinalIgnoreCase)) phase3Lessons.Add(bp);
            else overviewLessons.Add(bp);
        }

        static List<LessonBlueprint> SortIeltsLessons(List<LessonBlueprint> list)
        {
            return list
                .OrderBy(l => GetIeltsLessonPriority(l.RelativePath))
                .ThenBy(l => NaturalSortKey(l.RelativePath), StringComparer.OrdinalIgnoreCase)
                .Select((l, idx) => l with { SortOrder = idx + 1 })
                .ToList();
        }

        var chapters = new List<ChapterBlueprint>();
        var sortedOverview = SortIeltsLessons(overviewLessons);
        var sortedP0 = SortIeltsLessons(phase0Lessons);
        var sortedP1 = SortIeltsLessons(phase1Lessons);
        var sortedP2 = SortIeltsLessons(phase2Lessons);
        var sortedP3 = SortIeltsLessons(phase3Lessons);

        if (sortedOverview.Count > 0) chapters.Add(new("Tổng quan, Lộ trình & Tài nguyên IELTS", 1, sortedOverview));
        if (sortedP0.Count > 0) chapters.Add(new("Phase 0: Warm-up & Khởi động phát âm", 2, sortedP0));
        if (sortedP1.Count > 0) chapters.Add(new("Phase 1: Foundation (Nền tảng 4 kỹ năng)", 3, sortedP1));
        if (sortedP2.Count > 0) chapters.Add(new("Phase 2: Acceleration (Tăng tốc & Luyện đề)", 4, sortedP2));
        if (sortedP3.Count > 0) chapters.Add(new("Phase 3: Mastery & Đánh giá chuyên sâu", 5, sortedP3));

        // Parse IELTS Phase-3 CSV vocabulary flashcards if present
        var flashcards = new List<FlashcardSpec>();
        var fcDir = Path.Combine(courseRoot, "Phase-3", "flashcards");
        if (Directory.Exists(fcDir))
        {
            var fallbackP3Rel = sortedP3.FirstOrDefault(l => l.RelativePath.Contains("vocabulary", StringComparison.OrdinalIgnoreCase))?.RelativePath
                ?? sortedP3.FirstOrDefault()?.RelativePath
                ?? sortedP1.FirstOrDefault()?.RelativePath
                ?? sortedOverview.FirstOrDefault()?.RelativePath
                ?? string.Empty;

            foreach (var csvFile in Directory.GetFiles(fcDir, "*.csv").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var relDeck = Path.GetRelativePath(courseRoot, csvFile).Replace('\\', '/');
                flashcards.AddRange(ParseCsvFlashcards(File.ReadAllText(csvFile, Encoding.UTF8), fallbackP3Rel, relDeck));
            }
        }

        return new CourseBlueprint(def, chapters, [], flashcards);
    }

    private static CourseBlueprint BuildManifestDrivenBlueprint(CourseDefinition def, string courseRoot)
    {
        var overviewLessons = new List<LessonBlueprint>();
        var canonicalOverviewCandidates = new[]
        {
            "README.md", "learning-roadmap.md", "setup.md", "study-method.md",
            "english-practice.md", "project-spec.md", "glossary.md", "resources.md",
            "progress.md", "labs/README.md", "appendices/refresh-only.md"
        };

        int overviewIdx = 1;
        foreach (var candRel in canonicalOverviewCandidates)
        {
            var abs = Path.Combine(courseRoot, candRel.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(abs)) continue;
            var md = File.ReadAllText(abs, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(md)) continue;
            var title = ExtractMarkdownTitle(md, Path.GetFileNameWithoutExtension(candRel));
            overviewLessons.Add(CreateLessonBlueprint(candRel, candRel, title, overviewIdx++, md, null));
        }

        var manifestFiles = Directory.GetFiles(courseRoot, "manifest*.yaml", SearchOption.TopDirectoryOnly)
            .OrderBy(f => ExtractWeekOrStageNumber(Path.GetFileName(f)) ?? 1)
            .ThenBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var moduleBuckets = new Dictionary<string, List<LessonBlueprint>>(StringComparer.OrdinalIgnoreCase);
        var moduleOrder = new List<string>();
        var quizzes = new List<QuizBlueprint>();
        var flashcards = new List<FlashcardSpec>();
        var trackedQuizRelPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var trackedFlashcardRelPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (manifestFiles.Count > 0)
        {
            foreach (var mf in manifestFiles)
            {
                var yaml = File.ReadAllText(mf, Encoding.UTF8);
                var moduleKey = ExtractYamlScalarValue(yaml, "module") ?? "module-1";
                var unitTitle = ExtractUnitDisplayTitle(yaml);
                var unitOrder = ExtractUnitSortOrder(yaml) ?? ExtractWeekOrStageNumber(Path.GetFileName(mf)) ?? 1;
                var lessonPath = ExtractArtifactPathByRole(yaml, "lesson_theory");
                var quizPath = ExtractArtifactPathByRole(yaml, "quiz_questions");
                var ansPath = ExtractArtifactPathByRole(yaml, "quiz_answers");
                var deckPath = ExtractArtifactPathByRole(yaml, "flashcard_deck");

                if (string.IsNullOrWhiteSpace(lessonPath)) continue;
                var resolvedLessonRel = ResolveMarkdownLessonPath(courseRoot, lessonPath);
                if (resolvedLessonRel == null || IsIgnoredRelativePath(resolvedLessonRel, allowStartDocx: false))
                {
                    continue;
                }

                var absLesson = Path.Combine(courseRoot, resolvedLessonRel.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(absLesson)) continue;

                var lessonMd = File.ReadAllText(absLesson, Encoding.UTF8);
                var weekNum = ExtractWeekOrStageNumber(resolvedLessonRel) ?? unitOrder;
                var title = ExtractMarkdownTitle(lessonMd, string.Empty);
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = !string.IsNullOrWhiteSpace(unitTitle)
                        ? unitTitle
                        : Path.GetFileNameWithoutExtension(resolvedLessonRel);
                }

                if (!moduleBuckets.TryGetValue(moduleKey, out var bucket))
                {
                    bucket = [];
                    moduleBuckets[moduleKey] = bucket;
                    moduleOrder.Add(moduleKey);
                }

                bucket.Add(CreateLessonBlueprint(resolvedLessonRel, resolvedLessonRel, title, unitOrder, lessonMd, weekNum));

                if (!string.IsNullOrWhiteSpace(quizPath))
                {
                    var normQuizRel = quizPath.Replace('\\', '/');
                    var absQuiz = Path.Combine(courseRoot, normQuizRel.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(absQuiz) && trackedQuizRelPaths.Add(normQuizRel))
                    {
                        var quizMd = File.ReadAllText(absQuiz, Encoding.UTF8);
                        string? ansMd = null;
                        if (!string.IsNullOrWhiteSpace(ansPath))
                        {
                            var absAns = Path.Combine(courseRoot, ansPath.Replace('/', Path.DirectorySeparatorChar));
                            if (File.Exists(absAns)) ansMd = File.ReadAllText(absAns, Encoding.UTF8);
                        }
                        var quizTitle = ExtractMarkdownTitle(quizMd, $"Bài kiểm tra: {title}");
                        quizzes.Add(new QuizBlueprint(quizTitle, normQuizRel, quizMd, ansMd, resolvedLessonRel, weekNum));
                    }
                }

                if (!string.IsNullOrWhiteSpace(deckPath))
                {
                    var normDeckRel = deckPath.Replace('\\', '/');
                    var absDeck = Path.Combine(courseRoot, normDeckRel.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(absDeck) && trackedFlashcardRelPaths.Add(normDeckRel))
                    {
                        var (_, parsedCards) = CourseWizardAndImportService.ParseFlashcardsYamlDeck(File.ReadAllText(absDeck, Encoding.UTF8));
                        foreach (var c in parsedCards)
                        {
                            var cardTargetRel = NormalizeSourceLessonPath(c.SourceLesson, def.SourceFolder) ?? resolvedLessonRel;
                            var cardWeek = ExtractWeekOrStageNumber(c.LessonId) ?? weekNum;
                            flashcards.Add(new FlashcardSpec(c.Concept, c.Front, c.Back, cardTargetRel, cardWeek, normDeckRel));
                        }
                    }
                }
            }
        }
        else
        {
            // Fallback discovery for synthetic course directories without manifest*.yaml
            DiscoverWithoutManifests(courseRoot, def.SourceFolder, moduleBuckets, moduleOrder, quizzes, flashcards);
        }

        var chapters = new List<ChapterBlueprint>();
        int chOrder = 1;
        if (overviewLessons.Count > 0)
        {
            chapters.Add(new ChapterBlueprint("Tổng quan & Lộ trình khóa học", chOrder++, overviewLessons));
        }

        var sortedModules = moduleOrder
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => ExtractLeadingNumber(m) ?? 999)
            .ThenBy(m => m, StringComparer.OrdinalIgnoreCase);

        foreach (var modKey in sortedModules)
        {
            var sortedLessons = moduleBuckets[modKey]
                .GroupBy(l => l.RelativePath, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(l => l.WeekOrStageNumber ?? l.SortOrder)
                .ThenBy(l => NaturalSortKey(l.RelativePath), StringComparer.OrdinalIgnoreCase)
                .Select((l, idx) => l with { SortOrder = idx + 1 })
                .ToList();

            if (sortedLessons.Count == 0) continue;

            var chTitle = FormatModuleTitle(modKey);
            chapters.Add(new ChapterBlueprint(chTitle, chOrder++, sortedLessons));
        }

        return new CourseBlueprint(def, chapters, quizzes, flashcards);
    }

    private static void DiscoverWithoutManifests(
        string courseRoot,
        string sourceFolder,
        Dictionary<string, List<LessonBlueprint>> moduleBuckets,
        List<string> moduleOrder,
        List<QuizBlueprint> quizzes,
        List<FlashcardSpec> flashcards)
    {
        var notesRoot = Path.Combine(courseRoot, "notes");
        if (Directory.Exists(notesRoot))
        {
            foreach (var file in Directory.EnumerateFiles(notesRoot, "*.md", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(courseRoot, file).Replace('\\', '/');
                if (IsIgnoredRelativePath(rel, allowStartDocx: false)) continue;

                var parts = rel.Split('/');
                var modKey = parts.Length >= 3 ? parts[1] : "module-1";
                var md = File.ReadAllText(file, Encoding.UTF8);
                var weekNum = ExtractWeekOrStageNumber(rel);
                var title = ExtractMarkdownTitle(md, Path.GetFileNameWithoutExtension(rel));

                if (!moduleBuckets.TryGetValue(modKey, out var bucket))
                {
                    bucket = [];
                    moduleBuckets[modKey] = bucket;
                    moduleOrder.Add(modKey);
                }
                bucket.Add(CreateLessonBlueprint(rel, rel, title, weekNum ?? bucket.Count + 1, md, weekNum));
            }
        }

        var quizDir = Path.Combine(courseRoot, "assessments", "quizzes");
        var ansDir = Path.Combine(courseRoot, "assessments", "answer-keys");
        if (Directory.Exists(quizDir))
        {
            var answerFiles = Directory.Exists(ansDir)
                ? Directory.GetFiles(ansDir, "*.md").ToList()
                : [];

            foreach (var qf in Directory.GetFiles(quizDir, "*.md").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var rel = Path.GetRelativePath(courseRoot, qf).Replace('\\', '/');
                var weekNum = ExtractWeekOrStageNumber(Path.GetFileName(qf));
                string? matchingAnsFile = weekNum.HasValue
                    ? answerFiles.FirstOrDefault(af => ExtractWeekOrStageNumber(Path.GetFileName(af)) == weekNum.Value)
                    : null;

                var quizMd = File.ReadAllText(qf, Encoding.UTF8);
                var ansMd = matchingAnsFile != null ? File.ReadAllText(matchingAnsFile, Encoding.UTF8) : null;
                var qTitle = ExtractMarkdownTitle(quizMd, Path.GetFileNameWithoutExtension(qf));
                quizzes.Add(new QuizBlueprint(qTitle, rel, quizMd, ansMd, string.Empty, weekNum));
            }
        }

        var fcDir = Path.Combine(courseRoot, "flashcards");
        if (Directory.Exists(fcDir))
        {
            foreach (var yf in Directory.GetFiles(fcDir, "*.yaml").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var relDeck = Path.GetRelativePath(courseRoot, yf).Replace('\\', '/');
                var deckWeek = ExtractWeekOrStageNumber(Path.GetFileName(yf));
                var (_, cards) = CourseWizardAndImportService.ParseFlashcardsYamlDeck(File.ReadAllText(yf, Encoding.UTF8));
                foreach (var c in cards)
                {
                    var cardTargetRel = NormalizeSourceLessonPath(c.SourceLesson, sourceFolder) ?? string.Empty;
                    var cardWeek = ExtractWeekOrStageNumber(c.LessonId) ?? deckWeek;
                    flashcards.Add(new FlashcardSpec(c.Concept, c.Front, c.Back, cardTargetRel, cardWeek, relDeck));
                }
            }
        }
    }

    private static IEnumerable<FlashcardSpec> ParseCsvFlashcards(string csvContent, string targetLessonRelPath, string deckKey)
    {
        var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var raw in lines.Skip(1))
        {
            var fields = ParseCsvRow(raw);
            if (fields.Count < 2) continue;
            var front = fields[0].Trim();
            var back = fields[1].Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase).Trim();
            if (string.IsNullOrWhiteSpace(front) || string.IsNullOrWhiteSpace(back)) continue;

            yield return new FlashcardSpec(front, front, back, targetLessonRelPath, null, deckKey);
        }
    }

    private static List<string> ParseCsvRow(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result;
    }

    private static LessonBlueprint CreateLessonBlueprint(
        string relativePath,
        string slugSeed,
        string title,
        int sortOrder,
        string markdown,
        int? weekOrStage)
    {
        var normalizedRel = relativePath.Replace('\\', '/');
        var cleanMarkdown = Regex.Replace(markdown.Trim(), @"^---\r?\n[\s\S]*?\r?\n---\r?\n", string.Empty).Trim();
        var wordCount = cleanMarkdown.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var estMinutes = Math.Clamp((int)Math.Ceiling(wordCount / 180.0), 5, 90);
        var slug = BuildDeterministicSlug(slugSeed);

        return new LessonBlueprint(normalizedRel, slug, Truncate(title, 240), sortOrder, markdown, cleanMarkdown, estMinutes, weekOrStage);
    }

    private static string BuildDeterministicSlug(string relativePath)
    {
        var withoutExt = relativePath.Replace('\\', '/');
        var ext = Path.GetExtension(withoutExt);
        if (!string.IsNullOrEmpty(ext))
        {
            withoutExt = withoutExt[..^ext.Length];
        }
        var slug = Regex.Replace(withoutExt.ToLowerInvariant(), @"[^a-z0-9\-]+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "lesson" : Truncate(slug, 240);
    }

    private static string? ResolveMarkdownLessonPath(string courseRoot, string rawManifestPath)
    {
        var norm = rawManifestPath.Trim().Trim('"', '\'').Replace('\\', '/');
        if (norm.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || norm.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
        {
            var mdCandidate = Path.ChangeExtension(norm, ".md");
            var absMd = Path.Combine(courseRoot, mdCandidate.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(absMd))
            {
                return mdCandidate;
            }
        }
        return norm;
    }

    private static string? NormalizeSourceLessonPath(string? rawSourceLesson, string sourceFolder)
    {
        if (string.IsNullOrWhiteSpace(rawSourceLesson)) return null;
        var s = rawSourceLesson.Trim().Replace('\\', '/');
        var hashIdx = s.IndexOf('#');
        if (hashIdx >= 0) s = s[..hashIdx];
        if (s.StartsWith(sourceFolder + "/", StringComparison.OrdinalIgnoreCase))
        {
            s = s[(sourceFolder.Length + 1)..];
        }
        return s;
    }

    private static string? ExtractYamlScalarValue(string yaml, string key)
    {
        var m = Regex.Match(yaml, $@"^{Regex.Escape(key)}:\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        if (!m.Success) return null;
        return m.Groups[1].Value.Trim().Trim('"', '\'');
    }

    private static string? ExtractUnitDisplayTitle(string yaml)
    {
        var unitsIdx = yaml.IndexOf("\nunits:", StringComparison.OrdinalIgnoreCase);
        if (unitsIdx < 0) return ExtractYamlScalarValue(yaml, "display_title");
        var unitsSection = yaml[unitsIdx..];
        var m = Regex.Match(unitsSection, @"^\s+display_title:\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value.Trim().Trim('"', '\'') : ExtractYamlScalarValue(yaml, "display_title");
    }

    private static int? ExtractUnitSortOrder(string yaml)
    {
        var m = Regex.Match(yaml, @"^\s+sort_order:\s*(\d+)\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        return m.Success && int.TryParse(m.Groups[1].Value, out int parsed) ? parsed : null;
    }

    private static string? ExtractArtifactPathByRole(string yaml, string role)
    {
        var m = Regex.Match(
            yaml,
            $@"role:\s*{Regex.Escape(role)}\b[\s\S]{{0,300}}?path:\s*([^\r\n]+)",
            RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value.Trim().Trim('"', '\'') : null;
    }

    private static bool IsIgnoredRelativePath(string relativePath, bool allowStartDocx)
    {
        var norm = relativePath.Replace('\\', '/');
        var segments = norm.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < segments.Length - 1; i++)
        {
            if (IgnoredDirectoryNames.Contains(segments[i]))
            {
                return true;
            }
        }

        var fileName = segments.Length > 0 ? segments[^1] : norm;
        if (IgnoredExactFileNames.Contains(fileName)) return true;
        if (fileName.StartsWith("HANDOFF", StringComparison.OrdinalIgnoreCase)) return true;
        if (fileName.StartsWith("implementation_plan", StringComparison.OrdinalIgnoreCase)) return true;
        if (fileName.StartsWith("test-preview", StringComparison.OrdinalIgnoreCase)) return true;
        if (fileName.StartsWith("preview-", StringComparison.OrdinalIgnoreCase)) return true;

        var ext = Path.GetExtension(fileName);
        if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
        {
            return !(allowStartDocx && fileName.Equals("00_Bat_Dau.docx", StringComparison.OrdinalIgnoreCase));
        }
        return IgnoredExtensions.Contains(ext);
    }

    private static int GetIeltsLessonPriority(string relPath)
    {
        var norm = relPath.Replace('\\', '/').ToLowerInvariant();
        if (norm.Contains("daily-guide")) return 0;
        if (norm.Contains("roadmap") || norm.EndsWith("readme.md") || norm.Contains("/alphabet/")) return 1;
        if (norm.Contains("/grammar/")) return 2;
        if (norm.Contains("/vocabulary/")) return 3;
        if (norm.Contains("/listening/")) return 4;
        if (norm.Contains("/reading/")) return 5;
        if (norm.Contains("/speaking/")) return 6;
        if (norm.Contains("/writing/")) return 7;
        if (norm.Contains("/transcripts/")) return 8;
        if (norm.Contains("/tests/") || norm.Contains("/assessments/") || norm.Contains("error-log")) return 9;
        return 10;
    }

    private static string NaturalSortKey(string input)
    {
        return Regex.Replace(input, @"\d+", m => m.Value.PadLeft(6, '0'));
    }

    private static int? ExtractWeekOrStageNumber(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var clean = RemoveDiacritics(text);
        var m = Regex.Match(clean, @"(?:week|stage|lesson|quiz|tuan|bai|chang|w)[-_ ]*0*(\d+)", RegexOptions.IgnoreCase);
        if (m.Success && int.TryParse(m.Groups[1].Value, out int n)) return n;
        var simpleNum = Regex.Match(Path.GetFileNameWithoutExtension(clean), @"^0*(\d+)$");
        return simpleNum.Success && int.TryParse(simpleNum.Groups[1].Value, out int sn) ? sn : null;
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch == 'đ' ? 'd' : (ch == 'Đ' ? 'D' : ch));
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static int? ExtractLeadingNumber(string text)
    {
        var m = Regex.Match(text, @"(\d+)");
        return m.Success && int.TryParse(m.Groups[1].Value, out int n) ? n : null;
    }

    private static string FormatModuleTitle(string moduleKey)
    {
        if (ModuleTitleOverrides.TryGetValue(moduleKey, out var mapped))
        {
            return mapped;
        }
        var cleaned = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(moduleKey.Replace('-', ' ').Replace('_', ' '));
        return cleaned;
    }

    private static string ExtractMarkdownTitle(string markdown, string fallback)
    {
        var cleaned = Regex.Replace(markdown.TrimStart(), @"^---\r?\n[\s\S]*?\r?\n---\r?\n", string.Empty);
        var m = Regex.Match(cleaned, @"^#{1,2}\s+(.+)$", RegexOptions.Multiline);
        if (!m.Success) return fallback;
        var raw = m.Groups[1].Value.Replace("**", string.Empty).Replace("`", string.Empty).Trim();
        return string.IsNullOrWhiteSpace(raw) ? fallback : raw;
    }

    private static string Truncate(string value, int maxLen)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLen) return value;
        return value[..(maxLen - 1)].TrimEnd() + "…";
    }
}
