using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Database;
using Swms.Api.Infrastructure.Database.Seed;
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
    public const string DefaultStudxyBootcampRootPath = @"D:\03_Study\Studxy\course";
    public const string StudxyBootcampSlug = "doc-course-studxy-fullstack-bootcamp";
    public const string StudxyBootcampSourceFolder = "studxy-bootcamp";

    public static readonly HashSet<string> RetiredCourseSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "document-course-dotnet-react-roadmap",
        "document-course-fullstack-csharp-react-refresh",
        "lap-trinh-javascript-tu-duy-giai-quyet-van-de",
        "dau-tu-quan-tri-danh-muc-thuc-chien",
        "tam-ly-hoc-ung-dung-khoa-hoc-tri-nho"
    };

    public static readonly Dictionary<string, string> RetiredCourseRedirectMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["document-course-dotnet-react-roadmap"] = "doc-course-studxy-fullstack-bootcamp",
        ["document-course-fullstack-csharp-react-refresh"] = "doc-course-studxy-fullstack-bootcamp",
        ["lap-trinh-javascript-tu-duy-giai-quyet-van-de"] = "doc-course-studxy-fullstack-bootcamp",
        ["dau-tu-quan-tri-danh-muc-thuc-chien"] = "document-course-investing",
        ["tam-ly-hoc-ung-dung-khoa-hoc-tri-nho"] = "document-course-psychology"
    };

    public static Guid CreateDeterministicGuid(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }

    public static Guid CreateDeterministicGuid(string type, string name) =>
        CreateDeterministicGuid($"{type}:{name}".ToLowerInvariant());

    public static readonly Dictionary<Guid, string> RetiredCourseIdRedirectMap = new()
    {
        [SeedDataInitializer.CodingCourseId] = "doc-course-studxy-fullstack-bootcamp",
        [SeedDataInitializer.InvestingCourseId] = "document-course-investing",
        [SeedDataInitializer.PsychologyCourseId] = "document-course-psychology",
        [CreateDeterministicGuid("course", "document-course-dotnet-react-roadmap")] = "doc-course-studxy-fullstack-bootcamp",
        [CreateDeterministicGuid("course", "document-course-fullstack-csharp-react-refresh")] = "doc-course-studxy-fullstack-bootcamp",
        [CreateDeterministicGuid("swms:course:document-course-dotnet-react-roadmap")] = "doc-course-studxy-fullstack-bootcamp",
        [CreateDeterministicGuid("swms:course:document-course-fullstack-csharp-react-refresh")] = "doc-course-studxy-fullstack-bootcamp"
    };

    public static bool IsRetiredCourseSlug(string? slug) =>
        !string.IsNullOrWhiteSpace(slug) && RetiredCourseSlugs.Contains(slug);

    public static string? GetRetiredRedirectSlug(string? slug) =>
        !string.IsNullOrWhiteSpace(slug) && RetiredCourseRedirectMap.TryGetValue(slug, out var redirect) ? redirect : null;

    public static string? GetRetiredRedirectSlugById(Guid courseId) =>
        RetiredCourseIdRedirectMap.TryGetValue(courseId, out var redirect) ? redirect : null;

    public static bool IsProtectedSystemCourse(Course course)
    {
        if (course == null) return false;
        if (IsRetiredCourseSlug(course.Slug)) return false;
        if (IsPublicCurriculumSlug(course.Slug)) return true;
        return course.Id == SeedDataInitializer.AcademicCourseId ||
               course.Id == SeedDataInitializer.EnglishCourseId ||
               course.Id == SeedDataInitializer.JapaneseCourseId ||
               course.Id == SeedDataInitializer.ChineseCourseId ||
               string.Equals(course.Slug, "he-dieu-hanh-bo-nho-ao", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(course.Slug, "tieng-anh-giao-tiep-thuc-chien-a2-b1", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(course.Slug, "tieng-nhat-so-cap-n5", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(course.Slug, "tieng-trung-so-cap-hsk1", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsPublicCurriculumSlug(string? slug) =>
        !string.IsNullOrWhiteSpace(slug) &&
        !IsRetiredCourseSlug(slug) &&
        (slug.StartsWith("document-course-", StringComparison.OrdinalIgnoreCase) ||
         slug.Equals(StudxyBootcampSlug, StringComparison.OrdinalIgnoreCase));
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

    public static readonly HashSet<string> AllowedLegacyAssetFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "DotNet-React-Roadmap",
        "FullStack-CSharp-React-Refresh"
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

    public static string ResolveBootcampRootPath(IConfiguration? config = null)
    {
        var configured = config?["DocumentLibrary:BootcampRootPath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.Trim();
        }
        return DefaultStudxyBootcampRootPath;
    }

    public static int GetSortPriority(string? courseSlug)
    {
        if (!string.IsNullOrWhiteSpace(courseSlug) && courseSlug.Equals(StudxyBootcampSlug, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }
        if (!string.IsNullOrWhiteSpace(courseSlug) && DefinitionBySlug.TryGetValue(courseSlug, out var def))
        {
            return def.SortPriority;
        }
        return 999;
    }

    public static (string? SourceFolder, string Category) ResolveCourseMetadata(string? courseSlug, string? title = null, string? astSourceFolder = null)
    {
        if (!string.IsNullOrWhiteSpace(courseSlug))
        {
            if (courseSlug.Equals(StudxyBootcampSlug, StringComparison.OrdinalIgnoreCase))
            {
                return (StudxyBootcampSourceFolder, "Lập trình Full-Stack");
            }
            if (DefinitionBySlug.TryGetValue(courseSlug, out var def))
            {
                return (def.SourceFolder, def.Category);
            }
            if (courseSlug.Equals("dau-tu-quan-tri-danh-muc-thuc-chien", StringComparison.OrdinalIgnoreCase))
            {
                return (null, "Tài chính & Đầu tư");
            }
            if (courseSlug.Equals("lap-trinh-javascript-tu-duy-giai-quyet-van-de", StringComparison.OrdinalIgnoreCase))
            {
                return (null, "Lập trình & Công nghệ");
            }
            if (courseSlug.Equals("tam-ly-hoc-ung-dung-khoa-hoc-tri-nho", StringComparison.OrdinalIgnoreCase))
            {
                return (null, "Tâm lý & Kỹ năng sống");
            }
            if (courseSlug.Equals("tieng-anh-giao-tiep-thuc-chien-a2-b1", StringComparison.OrdinalIgnoreCase))
            {
                return (null, "Ngoại ngữ");
            }
        }

        if (!string.IsNullOrWhiteSpace(astSourceFolder) && DefinitionByFolder.TryGetValue(astSourceFolder, out var byFolder))
        {
            return (byFolder.SourceFolder, byFolder.Category);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            if (title.Contains("Đầu tư & Quản trị Danh mục", StringComparison.OrdinalIgnoreCase))
            {
                return (null, "Tài chính & Đầu tư");
            }
            if (title.Contains("JavaScript & Tư duy Giải quyết", StringComparison.OrdinalIgnoreCase))
            {
                return (null, "Lập trình & Công nghệ");
            }
            if (title.Contains("Tâm lý học Ứng dụng & Khoa học Trí nhớ", StringComparison.OrdinalIgnoreCase))
            {
                return (null, "Tâm lý & Kỹ năng sống");
            }
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

        bool isBootcampFolder = courseFolder.Equals(StudxyBootcampSourceFolder, StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(courseFolder) ||
            courseFolder.Contains("..", StringComparison.Ordinal) ||
            courseFolder.Contains('/') ||
            courseFolder.Contains('\\') ||
            (!isBootcampFolder && !DefinitionByFolder.ContainsKey(courseFolder) && !AllowedLegacyAssetFolders.Contains(courseFolder)))
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
        string courseRoot;
        if (isBootcampFolder)
        {
            var subBootcamp = Path.GetFullPath(Path.Combine(fullRoot, courseFolder));
            if (Directory.Exists(subBootcamp))
            {
                courseRoot = subBootcamp;
            }
            else if (Directory.Exists(Path.Combine(fullRoot, "phase-0-orientation")) ||
                     File.Exists(Path.Combine(fullRoot, normalizedRelativePath.Replace('/', Path.DirectorySeparatorChar))))
            {
                courseRoot = fullRoot;
            }
            else
            {
                courseRoot = Path.GetFullPath(DefaultStudxyBootcampRootPath);
            }
        }
        else
        {
            courseRoot = Path.GetFullPath(Path.Combine(fullRoot, courseFolder));
        }
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
        bool extractedFromCourseFileUri = false;
        if (raw.Contains("file:/", StringComparison.OrdinalIgnoreCase))
        {
            var courseMarkerIdx = raw.IndexOf("/course/", StringComparison.OrdinalIgnoreCase);
            if (courseMarkerIdx >= 0)
            {
                raw = raw[(courseMarkerIdx + "/course/".Length)..];
                extractedFromCourseFileUri = true;
            }
        }

        if (string.IsNullOrWhiteSpace(raw) || raw.Contains('\0') || raw.Contains(':'))
        {
            return false;
        }

        var segments = new List<string>();
        bool isRelativeToCurrent = !extractedFromCourseFileUri &&
            (raw.StartsWith("./", StringComparison.Ordinal) || raw.StartsWith("../", StringComparison.Ordinal));
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
                    var chTitle = Truncate(chBp.Title, 250);
                    if (!chapterMapByTitle.TryGetValue(chTitle, out var chapter) && !chapterMapByTitle.TryGetValue(chBp.Title, out chapter))
                    {
                        if (chapterMapByOrder.TryGetValue(chBp.SortOrder, out var byOrder))
                        {
                            chapter = byOrder;
                            chapter.Title = chTitle;
                            chapter.UpdatedAt = now;
                        }
                        else
                        {
                            chapter = new Chapter
                            {
                                Id = Guid.NewGuid(),
                                CourseId = course.Id,
                                Title = chTitle,
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
                    else if (chapter != null && (!string.Equals(chapter.Title, chTitle, StringComparison.Ordinal) || chapter.SortOrder != chBp.SortOrder))
                    {
                        chapter.Title = chTitle;
                        chapter.SortOrder = chBp.SortOrder;
                        chapter.UpdatedAt = now;
                    }

                    if (chapter == null)
                    {
                        continue;
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

                await db.SaveChangesAsync(ct);

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
                var fallbackQuizByLesson = new Dictionary<string, PracticeQuiz>(StringComparer.OrdinalIgnoreCase);
                foreach (var q in existingQuizzes)
                {
                    if (q.LessonId.HasValue)
                    {
                        quizByKey.TryAdd($"{q.LessonId.Value:N}|{q.Title}", q);
                        if (!q.Title.StartsWith("[Đề ", StringComparison.OrdinalIgnoreCase))
                        {
                            fallbackQuizByLesson.TryAdd($"{q.LessonId.Value:N}", q);
                        }
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
                    var isSummaryExam = qTitle.StartsWith("[Đề ", StringComparison.OrdinalIgnoreCase);
                    quizByKey.TryGetValue($"{matchedLesson.Id:N}|{qTitle}", out var existingQuiz);
                    if (existingQuiz == null && !isSummaryExam)
                    {
                        existingQuiz = fallbackQuizByLesson.GetValueOrDefault($"{matchedLesson.Id:N}");
                    }

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
                        if (!isSummaryExam)
                        {
                            fallbackQuizByLesson.TryAdd($"{matchedLesson.Id:N}", newQuiz);
                        }
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
                    else
                    {
                        var existingCount = existingItemCounts.GetValueOrDefault(existingQuiz.Id, 0);
                        var parsedItems = CourseWizardAndImportService.ParseQuizMarkdown(
                            existingQuiz.Id,
                            qBp.QuizMarkdown,
                            qBp.AnswerKeyMarkdown,
                            now);

                        if (parsedItems.Count > existingCount)
                        {
                            var hasEvaluated = await db.PracticeQuizItems
                                .AnyAsync(i => i.QuizId == existingQuiz.Id && i.EvaluatedAt != null, ct);

                            if (!hasEvaluated)
                            {
                                var oldItems = await db.PracticeQuizItems
                                    .Where(i => i.QuizId == existingQuiz.Id)
                                    .ToListAsync(ct);
                                db.PracticeQuizItems.RemoveRange(oldItems);
                                db.PracticeQuizItems.AddRange(parsedItems);
                                existingQuiz.TotalItems = parsedItems.Count;
                                existingQuiz.Title = qTitle;
                                updatedQuizzes++;
                            }
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

                await db.SaveChangesAsync(ct);

                // Ensure rich per-lesson PracticeActivities exist across ALL 7 document courses (idempotent by deterministic Id & composite key)
                var existingActs = await db.PracticeActivities
                    .Where(pa => pa.CourseId == course.Id)
                    .ToListAsync(ct);
                var existingActIds = existingActs.Select(pa => pa.Id).ToHashSet();
                var existingActComposite = existingActs
                    .Select(pa => (pa.LessonId, Type: (pa.Type ?? string.Empty).Trim().ToLowerInvariant(), pa.OrderIndex))
                    .ToHashSet();

                if (bp.Definition.SourceFolder == "DotNet-React-Roadmap" && existingActs.Count == 0)
                {
                    var actDir = Path.Combine(effectiveRoot, bp.Definition.SourceFolder, "activities");
                    if (Directory.Exists(actDir))
                    {
                        var actFiles = Directory.GetFiles(actDir, "manual-stage-*.yaml")
                            .OrderBy(f => ExtractWeekOrStageNumber(Path.GetFileName(f)) ?? 999)
                            .ToList();

                        foreach (var f in actFiles)
                        {
                            var stage = ExtractWeekOrStageNumber(Path.GetFileName(f)) ?? 1;
                            var yamlText = File.ReadAllText(f, Encoding.UTF8);
                            var actTitle = ExtractYamlScalarValue(yamlText, "title") ?? $"Thực hành Chặng {stage}";
                            var instructions = ExtractYamlScalarValue(yamlText, "instructions") ?? string.Empty;
                            var expectedOutput = ExtractYamlScalarValue(yamlText, "expected_output") ?? string.Empty;

                            var matchedLesson = ResolveTargetLesson(
                                $"{stage}.md",
                                stage,
                                syncedLessonMap,
                                syncedByWeekMap,
                                firstLesson);

                            var actId = SeedDataInitializer.CreateDeterministicActivityId(course.Id, matchedLesson?.Id, "coding", stage);
                            var compKey = (matchedLesson?.Id, "coding", stage);
                            if (existingActIds.Contains(actId) || existingActComposite.Contains(compKey)) continue;

                            existingActIds.Add(actId);
                            existingActComposite.Add(compKey);
                            db.PracticeActivities.Add(new PracticeActivity
                            {
                                Id = actId,
                                CourseId = course.Id,
                                LessonId = matchedLesson?.Id,
                                OrderIndex = stage,
                                Type = "coding",
                                Title = Truncate($"Thực hành Chặng {stage}: {actTitle}", 250),
                                Description = Truncate(!string.IsNullOrWhiteSpace(expectedOutput) ? expectedOutput : instructions, 500),
                                ConfigJson = JsonSerializer.Serialize(new
                                {
                                    stage,
                                    title = actTitle,
                                    instructions,
                                    expectedOutput,
                                    completionMode = "code_submission",
                                    passingScore = 70
                                }),
                                PassingScore = 70,
                                IsSeeded = true,
                                CreatedAt = now,
                                UpdatedAt = now
                            });
                        }
                    }
                }

                foreach (var lessonEntry in syncedLessonMap.Values.OrderBy(l => l.SortOrder))
                {
                    foreach (var genAct in BuildDomainPracticeActivitiesForLesson(bp.Definition.SourceFolder, course.Id, lessonEntry, now))
                    {
                        var compKey = (genAct.LessonId, genAct.Type.Trim().ToLowerInvariant(), genAct.OrderIndex);
                        if (existingActIds.Contains(genAct.Id) || existingActComposite.Contains(compKey)) continue;
                        existingActIds.Add(genAct.Id);
                        existingActComposite.Add(compKey);
                        db.PracticeActivities.Add(genAct);
                    }
                }
            }

            await db.SaveChangesAsync(ct);

            // WP-C2: Seed migrated write-in labs, foreign language flashcards & curated metadata defaults
            await WriteInPracticeContent.EnsureMigratedWriteInLabsSeededAsync(db, DateTimeOffset.UtcNow);
            await LanguageFlashcardsSeed.EnsureSeededAsync(db, DateTimeOffset.UtcNow);
            await CuratedCourseMetadataCatalog.ApplyDefaultsToCoursesAsync(db, ct);

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

    /// <summary>
    /// Idempotently synchronizes the 148-lesson Studxy Full-Stack Developer Bootcamp (10 Phases + Final Capstone + Resources)
    /// from D:\03_Study\Studxy\course into Studxy's Course/Chapter/Lesson/PracticeQuiz/PracticeActivity tables.
    /// </summary>
    public static async Task<DocumentCourseSyncReport> SyncStudxyBootcampCourseAsync(
        SwmsDbContext db,
        string? courseRootPath = null,
        CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var effectiveRoot = string.IsNullOrWhiteSpace(courseRootPath) ? DefaultStudxyBootcampRootPath : courseRootPath.Trim();

        if (!Directory.Exists(effectiveRoot))
        {
            return new DocumentCourseSyncReport(0, 0, 0, 0, 0, 0, 0, 0, 0, effectiveRoot, sw.Elapsed.TotalMilliseconds);
        }

        await SyncLock.WaitAsync(ct);
        try
        {
            if (db.Database.IsRelational())
            {
                db.Database.SetCommandTimeout(180);
            }

            var chapterBlueprints = BuildStudxyBootcampChapterBlueprints(effectiveRoot);
            if (chapterBlueprints.Count == 0)
            {
                return new DocumentCourseSyncReport(0, 0, 0, 0, 0, 0, 0, 0, 0, effectiveRoot, sw.Elapsed.TotalMilliseconds);
            }

            var instructorId = await EnsureDefaultInstructorExistsAsync(db, ct);
            var now = DateTimeOffset.UtcNow;

            const string title = "Studxy Full-Stack Developer Bootcamp (Zero to Production, AI & DevOps)";
            const string description = "Khóa học Full-Stack toàn diện 148 bài (30% Lý thuyết - 70% Thực hành): CS Foundations, HTML/CSS, JS/TS, React, C# .NET, SQL/EF Core, Testing, CI/CD, Docker & Cầu nối AI Engineer / DevOps.";

            int createdCourses = 0;
            int updatedCourses = 0;
            int createdChapters = 0;
            int createdLessons = 0;
            int updatedLessons = 0;
            int createdQuizzes = 0;
            int updatedQuizzes = 0;

            var course = await db.Courses.FirstOrDefaultAsync(c => c.Slug == StudxyBootcampSlug, ct);
            if (course == null)
            {
                course = new Course
                {
                    Id = Guid.NewGuid(),
                    InstructorId = instructorId,
                    Title = title,
                    Slug = StudxyBootcampSlug,
                    Description = description,
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
                createdCourses = 1;
            }
            else
            {
                bool metaChanged =
                    !string.Equals(course.Title, title, StringComparison.Ordinal) ||
                    !string.Equals(course.Description, description, StringComparison.Ordinal) ||
                    course.Visibility != "public" ||
                    !course.IsPublished;
                if (metaChanged)
                {
                    course.Title = title;
                    course.Description = description;
                    course.Visibility = "public";
                    course.IsPublished = true;
                    course.UpdatedAt = now;
                    updatedCourses = 1;
                }
                if (!await db.CourseEnrollments.AnyAsync(e => e.UserId == instructorId && e.CourseId == course.Id, ct))
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
                await db.SaveChangesAsync(ct);
            }

            var existingChapters = await db.Chapters
                .Where(ch => ch.CourseId == course.Id)
                .ToListAsync(ct);
            var existingChapterIds = existingChapters.Select(ch => ch.Id).ToList();

            var existingLessons = existingChapterIds.Count > 0
                ? await db.Lessons.Where(l => existingChapterIds.Contains(l.ChapterId)).ToListAsync(ct)
                : new List<Lesson>();

            var chapterByOrder = existingChapters.ToDictionary(ch => ch.SortOrder);
            var chapterByTitle = existingChapters.ToDictionary(ch => ch.Title, StringComparer.OrdinalIgnoreCase);

            var lessonByRelPath = new Dictionary<string, Lesson>(StringComparer.OrdinalIgnoreCase);
            var lessonByChapterAndSlug = new Dictionary<string, Lesson>(StringComparer.OrdinalIgnoreCase);
            foreach (var exLesson in existingLessons)
            {
                lessonByChapterAndSlug.TryAdd($"{exLesson.ChapterId:N}|{exLesson.Slug}", exLesson);
                var (_, relPath) = CourseGenerationService.ExtractLessonSourceMetadata(exLesson.ContentAstJson);
                if (!string.IsNullOrWhiteSpace(relPath))
                {
                    lessonByRelPath.TryAdd(relPath, exLesson);
                }
            }

            // 1. Ensure all 12 Chapters exist
            var resolvedChapters = new List<(ChapterBlueprint Blueprint, Chapter Entity)>();
            foreach (var chBp in chapterBlueprints)
            {
                var chTitle = Truncate(chBp.Title, 250);
                if (!chapterByOrder.TryGetValue(chBp.SortOrder, out var ch))
                {
                    if (!chapterByTitle.TryGetValue(chTitle, out ch))
                    {
                        chapterByTitle.TryGetValue(chBp.Title, out ch);
                    }
                }
                if (ch == null)
                {
                    ch = new Chapter
                    {
                        Id = Guid.NewGuid(),
                        CourseId = course.Id,
                        Title = chTitle,
                        SortOrder = chBp.SortOrder,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    db.Chapters.Add(ch);
                    createdChapters++;
                    chapterByOrder[ch.SortOrder] = ch;
                    chapterByTitle[ch.Title] = ch;
                }
                else if (!string.Equals(ch.Title, chTitle, StringComparison.Ordinal) || ch.SortOrder != chBp.SortOrder)
                {
                    ch.Title = chTitle;
                    ch.SortOrder = chBp.SortOrder;
                    ch.UpdatedAt = now;
                }
                resolvedChapters.Add((chBp, ch));
            }
            await db.SaveChangesAsync(ct);

            // 2. Sync all Lessons in batches of 20 (safe for WAN PostgreSQL)
            const string compatSourceFolder = StudxyBootcampSourceFolder;
            var syncedFull = new List<(ChapterBlueprint Chapter, Chapter ChapterEntity, LessonBlueprint Blueprint, Lesson Entity)>();
            var syncedLessons = new List<(LessonBlueprint Blueprint, Lesson Entity)>();
            int pendingLessonWrites = 0;

            foreach (var (chBp, chapter) in resolvedChapters)
            {
                foreach (var lBp in chBp.Lessons)
                {
                    Lesson? target = null;
                    if (!lessonByRelPath.TryGetValue(lBp.RelativePath, out target))
                    {
                        lessonByChapterAndSlug.TryGetValue($"{chapter.Id:N}|{lBp.Slug}", out target);
                    }

                    var (astJson, plainText) = CourseWizardAndImportService.BuildMarkdownAstAndPlainText(
                        course.Id,
                        lBp.Title,
                        lBp.Markdown,
                        compatSourceFolder,
                        lBp.RelativePath);

                    if (target == null)
                    {
                        target = new Lesson
                        {
                            Id = Guid.NewGuid(),
                            ChapterId = chapter.Id,
                            Title = Truncate(lBp.Title, 240),
                            Slug = Truncate(lBp.Slug, 240),
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
                        pendingLessonWrites++;
                    }
                    else
                    {
                        bool changed =
                            target.ChapterId != chapter.Id ||
                            !string.Equals(target.Title, Truncate(lBp.Title, 240), StringComparison.Ordinal) ||
                            !string.Equals(target.Slug, Truncate(lBp.Slug, 240), StringComparison.Ordinal) ||
                            target.SortOrder != lBp.SortOrder ||
                            !string.Equals(target.ContentMarkdown, lBp.Markdown, StringComparison.Ordinal);

                        if (changed)
                        {
                            target.ChapterId = chapter.Id;
                            target.Title = Truncate(lBp.Title, 240);
                            target.Slug = Truncate(lBp.Slug, 240);
                            target.SortOrder = lBp.SortOrder;
                            target.ContentMarkdown = lBp.Markdown;
                            target.PlainText = plainText;
                            target.ContentAstJson = astJson;
                            target.EstimatedMinutes = lBp.EstimatedMinutes;
                            target.IsPublished = true;
                            target.UpdatedAt = now;
                            updatedLessons++;
                            pendingLessonWrites++;
                        }
                    }

                    syncedFull.Add((chBp, chapter, lBp, target));
                    syncedLessons.Add((lBp, target));

                    if (pendingLessonWrites >= 20)
                    {
                        await db.SaveChangesAsync(ct);
                        pendingLessonWrites = 0;
                    }
                }
            }

            if (pendingLessonWrites > 0)
            {
                await db.SaveChangesAsync(ct);
            }

            // 3. Sync Quizzes extracted from ## 🧪 Quiz sections & PracticeActivities (batched)
            var allLessonIds = syncedLessons.Select(x => x.Entity.Id).ToHashSet();
            var existingQuizzes = allLessonIds.Count > 0
                ? await db.PracticeQuizzes
                    .Where(q => q.UserId == instructorId && q.LessonId.HasValue && allLessonIds.Contains(q.LessonId.Value))
                    .ToListAsync(ct)
                : new List<PracticeQuiz>();

            var existingQuizIds = existingQuizzes.Select(q => q.Id).ToList();
            var existingQuizItemsList = existingQuizIds.Count > 0
                ? await db.PracticeQuizItems
                    .Where(i => existingQuizIds.Contains(i.QuizId))
                    .ToListAsync(ct)
                : new List<PracticeQuizItem>();
            var existingItemsByQuizId = existingQuizItemsList.ToLookup(i => i.QuizId);

            var quizByLesson = new Dictionary<Guid, PracticeQuiz>();
            var syntheticQuizByTitle = new Dictionary<string, PracticeQuiz>(StringComparer.OrdinalIgnoreCase);
            foreach (var q in existingQuizzes)
            {
                if (q.Title.StartsWith("[Đề Chương ", StringComparison.OrdinalIgnoreCase) ||
                    q.Title.StartsWith("[Đề Tổng Hợp", StringComparison.OrdinalIgnoreCase))
                {
                    syntheticQuizByTitle.TryAdd(q.Title, q);
                }
                else if (q.LessonId.HasValue)
                {
                    quizByLesson.TryAdd(q.LessonId.Value, q);
                }
            }

            var existingActs = await db.PracticeActivities
                .Where(pa => pa.CourseId == course.Id)
                .ToListAsync(ct);
            var existingActIds = existingActs.Select(pa => pa.Id).ToHashSet();
            var existingActById = new Dictionary<Guid, PracticeActivity>();
            var existingActByComposite = new Dictionary<(Guid? LessonId, string Type, int OrderIndex), PracticeActivity>();
            foreach (var act in existingActs)
            {
                existingActById.TryAdd(act.Id, act);
                var key = (act.LessonId, (act.Type ?? string.Empty).Trim().ToLowerInvariant(), act.OrderIndex);
                existingActByComposite.TryAdd(key, act);
            }

            var attemptedActIds = existingActIds.Count > 0
                ? (await db.PracticeAttempts.AsNoTracking()
                    .Where(pa => pa.ActivityId.HasValue && existingActIds.Contains(pa.ActivityId.Value))
                    .Select(pa => pa.ActivityId!.Value)
                    .Distinct()
                    .ToListAsync(ct))
                    .ToHashSet()
                : new HashSet<Guid>();

            var chapterMcqPool = new Dictionary<int, List<PracticeQuizItem>>();
            int pendingSecondaryWrites = 0;

            foreach (var (chBp, chapterEntity, lBp, lessonEntity) in syncedFull)
            {
                List<PracticeQuizItem>? mcqItemsForLesson = null;

                if (TryExtractBootcampQuizMarkdown(lBp.Markdown, out var quizMd, out var answerKeyMd))
                {
                    var quizTitle = Truncate($"Quiz: {lessonEntity.Title}", 240);
                    if (!quizByLesson.TryGetValue(lessonEntity.Id, out var existingQuiz))
                    {
                        var quizId = Guid.NewGuid();
                        var parsedItems = CourseWizardAndImportService.ParseQuizMarkdown(quizId, quizMd, answerKeyMd, now);
                        if (parsedItems.Count > 0)
                        {
                            var newQuiz = new PracticeQuiz
                            {
                                Id = quizId,
                                UserId = instructorId,
                                LessonId = lessonEntity.Id,
                                Title = quizTitle,
                                TotalItems = parsedItems.Count,
                                CorrectItems = 0,
                                Status = "not_started",
                                StartedAt = now
                            };
                            db.PracticeQuizzes.Add(newQuiz);
                            db.PracticeQuizItems.AddRange(parsedItems);
                            quizByLesson[lessonEntity.Id] = newQuiz;
                            createdQuizzes++;
                            pendingSecondaryWrites += parsedItems.Count + 1;
                            mcqItemsForLesson = parsedItems.Where(i => i.ItemType == "multiple_choice").ToList();
                        }
                    }
                    else
                    {
                        var oldItems = existingItemsByQuizId[existingQuiz.Id].ToList();
                        if (!oldItems.Any(i => i.EvaluatedAt != null))
                        {
                            var parsedItems = CourseWizardAndImportService.ParseQuizMarkdown(existingQuiz.Id, quizMd, answerKeyMd, now);
                            bool needReplace = oldItems.Count == 0
                                || oldItems.Count != parsedItems.Count
                                || (oldItems.Count > 1 && oldItems.All(i => string.Equals(i.ClozeSolution, "A", StringComparison.OrdinalIgnoreCase)) && parsedItems.Any(i => !string.Equals(i.ClozeSolution, "A", StringComparison.OrdinalIgnoreCase)))
                                || parsedItems.Count(i => i.ItemType == "multiple_choice") > oldItems.Count(i => i.ItemType == "multiple_choice")
                                || (oldItems.Any(i => !HasPromptExplanation(i.PromptAstJson)) && parsedItems.Any(i => HasPromptExplanation(i.PromptAstJson)));

                            if (needReplace && parsedItems.Count > 0)
                            {
                                if (oldItems.Count > 0)
                                {
                                    db.PracticeQuizItems.RemoveRange(oldItems);
                                }
                                db.PracticeQuizItems.AddRange(parsedItems);
                                existingQuiz.TotalItems = parsedItems.Count;
                                existingQuiz.Title = quizTitle;
                                updatedQuizzes++;
                                pendingSecondaryWrites += parsedItems.Count;
                                mcqItemsForLesson = parsedItems.Where(i => i.ItemType == "multiple_choice").ToList();
                            }
                            else
                            {
                                mcqItemsForLesson = oldItems.Where(i => i.ItemType == "multiple_choice").ToList();
                            }
                        }
                        else
                        {
                            mcqItemsForLesson = oldItems.Where(i => i.ItemType == "multiple_choice").ToList();
                        }
                    }
                }
                else if (quizByLesson.TryGetValue(lessonEntity.Id, out var existingQuiz))
                {
                    var oldItems = existingItemsByQuizId[existingQuiz.Id].ToList();
                    mcqItemsForLesson = oldItems.Where(i => i.ItemType == "multiple_choice").ToList();
                }

                if (mcqItemsForLesson != null && mcqItemsForLesson.Count > 0)
                {
                    if (!chapterMcqPool.TryGetValue(chBp.SortOrder, out var pool))
                    {
                        pool = new List<PracticeQuizItem>();
                        chapterMcqPool[chBp.SortOrder] = pool;
                    }
                    pool.AddRange(mcqItemsForLesson);
                }

                foreach (var genAct in BootcampPracticeActivityCatalog.BuildForLesson(lBp.RelativePath, course.Id, lessonEntity, lBp.Markdown, now))
                {
                    (Guid? LessonId, string Type, int OrderIndex) compKey = (genAct.LessonId, (genAct.Type ?? string.Empty).Trim().ToLowerInvariant(), genAct.OrderIndex);
                    if (existingActById.TryGetValue(genAct.Id, out var exAct) || existingActByComposite.TryGetValue(compKey, out exAct))
                    {
                        if (!attemptedActIds.Contains(exAct.Id) &&
                            (exAct.Title != genAct.Title || exAct.Description != genAct.Description || exAct.ConfigJson != genAct.ConfigJson))
                        {
                            exAct.Title = genAct.Title;
                            exAct.Description = genAct.Description;
                            exAct.ConfigJson = genAct.ConfigJson;
                            exAct.UpdatedAt = now;
                            pendingSecondaryWrites++;
                        }
                    }
                    else
                    {
                        db.PracticeActivities.Add(genAct);
                        existingActById[genAct.Id] = genAct;
                        existingActByComposite[compKey] = genAct;
                        existingActIds.Add(genAct.Id);
                        pendingSecondaryWrites++;
                    }
                }

                if (pendingSecondaryWrites >= 120)
                {
                    await db.SaveChangesAsync(ct);
                    pendingSecondaryWrites = 0;
                }
            }

            static PracticeQuizItem CloneQuizItem(Guid targetQuizId, PracticeQuizItem src, DateTimeOffset ts) =>
                new()
                {
                    Id = Guid.NewGuid(),
                    QuizId = targetQuizId,
                    ItemType = "multiple_choice",
                    PromptAstJson = src.PromptAstJson,
                    ChoicesJson = src.ChoicesJson,
                    ClozeSolution = src.ClozeSolution,
                    CreatedAt = ts
                };

            // 4. Generate/Update 11 Chapter Synthetic Quizzes (Chapters 2..12)
            for (int chOrder = 2; chOrder <= 12; chOrder++)
            {
                var chTuple = syncedFull.FirstOrDefault(x => x.Chapter.SortOrder == chOrder);
                if (chTuple.Chapter == null || chTuple.Entity == null) continue;

                var chBp = chTuple.Chapter;
                var chapterFirstLessonId = chTuple.Entity.Id;

                if (!chapterMcqPool.TryGetValue(chOrder, out var pool) || pool.Count == 0)
                {
                    continue;
                }

                var chosen = new List<PracticeQuizItem>();
                if (pool.Count <= 12)
                {
                    chosen.AddRange(pool);
                }
                else
                {
                    double step = (double)pool.Count / 12.0;
                    for (int i = 0; i < 12; i++)
                    {
                        int idx = (int)Math.Floor(i * step);
                        if (idx >= pool.Count) idx = pool.Count - 1;
                        chosen.Add(pool[idx]);
                    }
                }

                var synthTitle = Truncate($"[Đề Chương {chBp.SortOrder}] Kiểm tra tổng hợp: {chBp.Title}", 240);
                if (!syntheticQuizByTitle.TryGetValue(synthTitle, out var existingSynthQuiz))
                {
                    var synthQuizId = Guid.NewGuid();
                    var synthItems = chosen.Select((item, idx) => CloneQuizItem(synthQuizId, item, now.AddMilliseconds(idx))).ToList();
                    var newSynthQuiz = new PracticeQuiz
                    {
                        Id = synthQuizId,
                        UserId = instructorId,
                        LessonId = chapterFirstLessonId,
                        Title = synthTitle,
                        TotalItems = synthItems.Count,
                        CorrectItems = 0,
                        Status = "not_started",
                        StartedAt = now
                    };
                    db.PracticeQuizzes.Add(newSynthQuiz);
                    db.PracticeQuizItems.AddRange(synthItems);
                    syntheticQuizByTitle[synthTitle] = newSynthQuiz;
                    createdQuizzes++;
                    pendingSecondaryWrites += synthItems.Count + 1;
                }
                else
                {
                    var oldSynthItems = existingItemsByQuizId[existingSynthQuiz.Id].ToList();
                    if (!oldSynthItems.Any(i => i.EvaluatedAt != null))
                    {
                        bool needReplace = oldSynthItems.Count != chosen.Count
                            || (oldSynthItems.All(i => string.Equals(i.ClozeSolution, "A", StringComparison.OrdinalIgnoreCase)) && chosen.Any(i => !string.Equals(i.ClozeSolution, "A", StringComparison.OrdinalIgnoreCase)))
                            || (oldSynthItems.Any(i => !HasPromptExplanation(i.PromptAstJson)) && chosen.Any(i => HasPromptExplanation(i.PromptAstJson)));

                        if (needReplace)
                        {
                            if (oldSynthItems.Count > 0)
                            {
                                db.PracticeQuizItems.RemoveRange(oldSynthItems);
                            }
                            var synthItems = chosen.Select((item, idx) => CloneQuizItem(existingSynthQuiz.Id, item, now.AddMilliseconds(idx))).ToList();
                            db.PracticeQuizItems.AddRange(synthItems);
                            existingSynthQuiz.LessonId = chapterFirstLessonId;
                            existingSynthQuiz.TotalItems = synthItems.Count;
                            existingSynthQuiz.Title = synthTitle;
                            updatedQuizzes++;
                            pendingSecondaryWrites += synthItems.Count;
                        }
                    }
                }
            }

            // 5. Generate/Update Final Capstone Comprehensive Exam (30 questions across Chapters 2..12)
            if (syncedFull.Count > 0)
            {
                var finalPool = new List<PracticeQuizItem>();
                for (int chOrder = 2; chOrder <= 12; chOrder++)
                {
                    if (chapterMcqPool.TryGetValue(chOrder, out var pool) && pool.Count > 0)
                    {
                        int countToTake = Math.Min(3, pool.Count);
                        double chStep = (double)pool.Count / countToTake;
                        for (int k = 0; k < countToTake && finalPool.Count < 30; k++)
                        {
                            int idx = (int)Math.Floor(k * chStep);
                            if (idx >= pool.Count) idx = pool.Count - 1;
                            finalPool.Add(pool[idx]);
                        }
                    }
                }

                if (finalPool.Count < 30)
                {
                    var usedPromptAsts = finalPool.Select(x => x.PromptAstJson).ToHashSet();
                    for (int chOrder = 2; chOrder <= 12 && finalPool.Count < 30; chOrder++)
                    {
                        if (chapterMcqPool.TryGetValue(chOrder, out var pool))
                        {
                            foreach (var item in pool)
                            {
                                if (finalPool.Count >= 30) break;
                                if (!usedPromptAsts.Contains(item.PromptAstJson))
                                {
                                    usedPromptAsts.Add(item.PromptAstJson);
                                    finalPool.Add(item);
                                }
                            }
                        }
                    }
                }

                if (finalPool.Count > 0)
                {
                    const string finalQuizTitle = "[Đề Tổng Hợp Cuối Khóa] Thi Thử Toàn Diện Full-Stack Developer Bootcamp (30 Câu)";
                    var firstLessonId = syncedFull[0].Entity.Id;

                    if (!syntheticQuizByTitle.TryGetValue(finalQuizTitle, out var existingFinalQuiz))
                    {
                        var finalQuizId = Guid.NewGuid();
                        var finalItems = finalPool.Select((item, idx) => CloneQuizItem(finalQuizId, item, now.AddMilliseconds(idx))).ToList();
                        var newFinalQuiz = new PracticeQuiz
                        {
                            Id = finalQuizId,
                            UserId = instructorId,
                            LessonId = firstLessonId,
                            Title = finalQuizTitle,
                            TotalItems = finalItems.Count,
                            CorrectItems = 0,
                            Status = "not_started",
                            StartedAt = now
                        };
                        db.PracticeQuizzes.Add(newFinalQuiz);
                        db.PracticeQuizItems.AddRange(finalItems);
                        syntheticQuizByTitle[finalQuizTitle] = newFinalQuiz;
                        createdQuizzes++;
                        pendingSecondaryWrites += finalItems.Count + 1;
                    }
                    else
                    {
                        var oldFinalItems = existingItemsByQuizId[existingFinalQuiz.Id].ToList();
                        if (!oldFinalItems.Any(i => i.EvaluatedAt != null))
                        {
                            bool needReplace = oldFinalItems.Count != finalPool.Count
                                || (oldFinalItems.All(i => string.Equals(i.ClozeSolution, "A", StringComparison.OrdinalIgnoreCase)) && finalPool.Any(i => !string.Equals(i.ClozeSolution, "A", StringComparison.OrdinalIgnoreCase)))
                                || (oldFinalItems.Any(i => !HasPromptExplanation(i.PromptAstJson)) && finalPool.Any(i => HasPromptExplanation(i.PromptAstJson)));

                            if (needReplace)
                            {
                                if (oldFinalItems.Count > 0)
                                {
                                    db.PracticeQuizItems.RemoveRange(oldFinalItems);
                                }
                                var finalItems = finalPool.Select((item, idx) => CloneQuizItem(existingFinalQuiz.Id, item, now.AddMilliseconds(idx))).ToList();
                                db.PracticeQuizItems.AddRange(finalItems);
                                existingFinalQuiz.LessonId = firstLessonId;
                                existingFinalQuiz.TotalItems = finalItems.Count;
                                existingFinalQuiz.Title = finalQuizTitle;
                                updatedQuizzes++;
                                pendingSecondaryWrites += finalItems.Count;
                            }
                        }
                    }
                }
            }

            // 6. Sync Flashcards FSRS
            int createdFlashcards = 0;
            var courseTag = await EnsureCourseTagAsync(db, instructorId, title, now, ct);
            var bootcampCards = BuildBootcampFlashcards(syncedFull, effectiveRoot);
            if (bootcampCards.Count > 0 && syncedFull.Count > 0)
            {
                var bootcampLessonIds = syncedFull.Select(x => x.Entity.Id).ToHashSet();
                var existingCards = await db.Flashcards
                    .Where(f => f.UserId == instructorId && !f.IsArchived && f.LessonId.HasValue && bootcampLessonIds.Contains(f.LessonId.Value))
                    .ToListAsync(ct);

                var existingCardIds = existingCards.Select(c => c.Id).ToHashSet();
                var existingTagLinks = existingCardIds.Count > 0
                    ? await db.FlashcardTags
                        .Where(ft => ft.TagId == courseTag.Id && existingCardIds.Contains(ft.FlashcardId))
                        .Select(ft => ft.FlashcardId)
                        .ToHashSetAsync(ct)
                    : new HashSet<Guid>();

                var cardsByFront = new Dictionary<string, Flashcard>(StringComparer.OrdinalIgnoreCase);
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

                var firstLesson = syncedFull[0].Entity;
                foreach (var (matchedLesson, front, back, concept) in bootcampCards)
                {
                    if (cardsByFront.TryGetValue(front, out var existing))
                    {
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
                        SourceQuoteSnapshot = string.IsNullOrWhiteSpace(concept) ? null : Truncate(concept, 500),
                        FrontAstJson = JsonSerializer.Serialize(new { text = front }),
                        BackAstJson = JsonSerializer.Serialize(new { text = back }),
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
                    cardsByFront[front] = newCard;
                    createdFlashcards++;
                }
            }

            await db.SaveChangesAsync(ct);

            // WP-C2: Seed migrated write-in labs, foreign language flashcards & curated metadata defaults
            await WriteInPracticeContent.EnsureMigratedWriteInLabsSeededAsync(db, DateTimeOffset.UtcNow);
            await LanguageFlashcardsSeed.EnsureSeededAsync(db, DateTimeOffset.UtcNow);
            await CuratedCourseMetadataCatalog.ApplyDefaultsToCoursesAsync(db, ct);

            return new DocumentCourseSyncReport(
                1,
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

    private static List<ChapterBlueprint> BuildStudxyBootcampChapterBlueprints(string courseRoot)
    {
        var chapters = new List<ChapterBlueprint>();
        int chOrder = 1;

        void AddChapterFromFiles(string chapterTitle, IEnumerable<string> relPaths)
        {
            var lessons = new List<LessonBlueprint>();
            int sort = 1;
            foreach (var rawRel in relPaths)
            {
                var normRel = rawRel.Replace('\\', '/').TrimStart('/');
                var fullPath = Path.Combine(courseRoot, normRel.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(fullPath)) continue;

                var md = File.ReadAllText(fullPath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(md)) continue;

                var fallbackTitle = Path.GetFileName(normRel).Equals("README.md", StringComparison.OrdinalIgnoreCase)
                    ? Path.GetFileName(Path.GetDirectoryName(fullPath) ?? "Bài học")
                    : Path.GetFileNameWithoutExtension(normRel);
                var title = ExtractMarkdownTitle(md, fallbackTitle);
                lessons.Add(CreateLessonBlueprint(normRel, normRel, title, sort++, md, null));
            }
            if (lessons.Count > 0)
            {
                chapters.Add(new ChapterBlueprint(chapterTitle, chOrder++, lessons));
            }
        }

        List<string> CollectPhaseFiles(string phaseFolder)
        {
            var list = new List<string>();
            var phaseDir = Path.Combine(courseRoot, phaseFolder);
            if (!Directory.Exists(phaseDir)) return list;

            if (File.Exists(Path.Combine(phaseDir, "README.md")))
            {
                list.Add($"{phaseFolder}/README.md");
            }

            var subDirs = Directory.GetDirectories(phaseDir)
                .Select(Path.GetFileName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Cast<string>()
                .OrderBy(n => n.StartsWith("lesson-", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(n => NaturalSortKey(n), StringComparer.OrdinalIgnoreCase);

            foreach (var sub in subDirs)
            {
                var subDirPath = Path.Combine(phaseDir, sub);
                if (File.Exists(Path.Combine(subDirPath, "README.md")))
                {
                    list.Add($"{phaseFolder}/{sub}/README.md");
                }
                else
                {
                    var fallbackMd = Directory.GetFiles(subDirPath, "*.md", SearchOption.TopDirectoryOnly)
                        .Select(Path.GetFileName)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Cast<string>()
                        .OrderBy(n => NaturalSortKey(n), StringComparer.OrdinalIgnoreCase)
                        .FirstOrDefault();
                    if (fallbackMd != null)
                    {
                        list.Add($"{phaseFolder}/{sub}/{fallbackMd}");
                    }
                }
            }

            var directMdFiles = Directory.GetFiles(phaseDir, "*.md", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Where(n => !string.IsNullOrWhiteSpace(n) && !n.Equals("README.md", StringComparison.OrdinalIgnoreCase))
                .Cast<string>()
                .OrderBy(n => n.StartsWith("lesson-", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(n => NaturalSortKey(n), StringComparer.OrdinalIgnoreCase);

            foreach (var mdFile in directMdFiles)
            {
                list.Add($"{phaseFolder}/{mdFile}");
            }
            return list;
        }

        // Chapter 1: Root orientation docs
        AddChapterFromFiles(
            "Chương 1: Tổng quan, Giáo trình & Lộ trình học tập",
            ["README.md", "SYLLABUS.md", "LEARNING_PATH.md"]);

        // Chapters 2..11: Phase 0 .. Phase 9
        var phaseSpecs = new (string Folder, string Title)[]
        {
            ("phase-0-orientation", "Chương 2: Phase 0 — Orientation & Thiết lập Môi trường"),
            ("phase-1-cs-foundations", "Chương 3: Phase 1 — Computer Science Foundations (DSA & Big-O)"),
            ("phase-2-web-foundations", "Chương 4: Phase 2 — Web Foundations (HTML5, CSS3, Flexbox, Grid, a11y)"),
            ("phase-3-javascript-mastery", "Chương 5: Phase 3 — JavaScript Mastery & TypeScript Intro"),
            ("phase-4-frontend-react", "Chương 6: Phase 4 — Frontend Hiện đại với React, TypeScript & Tailwind"),
            ("phase-5-backend-dotnet", "Chương 7: Phase 5 — Backend Engineering với C# & ASP.NET Core"),
            ("phase-6-database-sql", "Chương 8: Phase 6 — Cơ sở Dữ liệu, SQL Chuyên sâu & EF Core"),
            ("phase-7-fullstack-integration", "Chương 9: Phase 7 — Full-Stack Integration (JWT, SignalR, TanStack Query, Zod)"),
            ("phase-8-testing-quality", "Chương 10: Phase 8 — Testing Pyramid (xUnit, Vitest, Playwright) & CI/CD"),
            ("phase-9-deployment-production", "Chương 11: Phase 9 — Linux, Docker, Cloud Deployment, Security & Observability")
        };

        foreach (var (folder, chTitle) in phaseSpecs)
        {
            AddChapterFromFiles(chTitle, CollectPhaseFiles(folder));
        }

        // Chapter 12: Final Capstone + Engineering Resources
        AddChapterFromFiles(
            "Chương 12: Đồ án Tốt nghiệp (Final Capstone) & Tài nguyên Chuyển tiếp AI / DevOps",
            [
                "capstone-project/README.md",
                "capstone-project/requirements/README.md",
                "capstone-project/sprint-1-core/README.md",
                "capstone-project/sprint-2-polish/README.md",
                "capstone-project/deployment-presentation/README.md",
                "resources/cheatsheets/README.md",
                "resources/glossary.md",
                "resources/recommended-tools.md",
                "resources/next-steps.md"
            ]);

        return chapters;
    }

    private static bool TryExtractBootcampQuizMarkdown(string markdown, out string quizMarkdown, out string? answerKeyMarkdown)
    {
        quizMarkdown = string.Empty;
        answerKeyMarkdown = null;
        if (string.IsNullOrWhiteSpace(markdown)) return false;

        var quizHeaderMatch = Regex.Match(markdown, @"(?m)^##\s+(?:🧪\s*)?Quiz\b.*$");
        if (!quizHeaderMatch.Success) return false;

        var afterHeaderIdx = quizHeaderMatch.Index;
        var nextSectionMatch = Regex.Match(markdown[(afterHeaderIdx + quizHeaderMatch.Length)..], @"(?m)^##\s+");
        var rawSection = nextSectionMatch.Success
            ? markdown.Substring(afterHeaderIdx, quizHeaderMatch.Length + nextSectionMatch.Index)
            : markdown[afterHeaderIdx..];

        // If the section has a single consolidated <details> block at the end with numbered answers (Style 2),
        // split it out as answerKeyMarkdown; if each question has its own <details> (Style 1), pass whole section.
        var detailsMatches = Regex.Matches(rawSection, @"(?m)^\s*<details(?:\s+[^>]*)?>[\s\S]*?</details>", RegexOptions.IgnoreCase);
        if (detailsMatches.Count == 1)
        {
            var block = detailsMatches[0].Value;
            if (Regex.IsMatch(block, @"(?m)(?:^\s*(?:[-*]\s*)?(?:\*\*)?(?:Câu\s+)?1[\.\):]|^\s*\|\s*(?:\*\*)?(?:Câu\s*)?1\b)"))
            {
                answerKeyMarkdown = block;
                quizMarkdown = rawSection[..detailsMatches[0].Index].Trim();
                return !string.IsNullOrWhiteSpace(quizMarkdown);
            }
        }

        quizMarkdown = rawSection.Trim();
        return !string.IsNullOrWhiteSpace(quizMarkdown);
    }

    private static bool HasPromptExplanation(string? promptAstJson)
    {
        if (string.IsNullOrWhiteSpace(promptAstJson)) return false;
        try
        {
            using var doc = JsonDocument.Parse(promptAstJson);
            return doc.RootElement.TryGetProperty("explanation", out var exp) &&
                   exp.ValueKind == JsonValueKind.String &&
                   !string.IsNullOrWhiteSpace(exp.GetString());
        }
        catch
        {
            return false;
        }
    }

    private static List<(Lesson Entity, string Front, string Back, string? Concept)> BuildBootcampFlashcards(
        List<(ChapterBlueprint Chapter, Chapter ChapterEntity, LessonBlueprint Blueprint, Lesson Entity)> syncedFull,
        string courseRoot)
    {
        var result = new List<(Lesson Entity, string Front, string Back, string? Concept)>();
        var seenFronts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        static bool IsBoilerplateTerm(string term, string def)
        {
            if (term.Contains("Câu", StringComparison.OrdinalIgnoreCase) ||
                term.Contains("Đáp án", StringComparison.OrdinalIgnoreCase) ||
                term.Contains("Bước", StringComparison.OrdinalIgnoreCase) ||
                def.Contains("Câu", StringComparison.OrdinalIgnoreCase) ||
                def.Contains("Đáp án", StringComparison.OrdinalIgnoreCase) ||
                def.Contains("Bước", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var norm = term.Trim().TrimEnd(':', '-', '–', '—').ToLowerInvariant();
            return norm is "khái niệm" or "ví dụ" or "ví dụ đời thường" or "ví dụ sản xuất" or
                "trong lập trình" or "thực tế" or "lưu ý" or "lưu ý sản xuất" or "gợi ý" or
                "đề bài" or "input" or "output" or "edge cases" or "phòng chống" or
                "ưu điểm" or "nhược điểm" or "mục tiêu" or "đọc thêm" or "bài học tiếp theo" or
                "mô tả" or "giải thích" or "cú pháp" or "kết quả";
        }

        // Matches both `- **Term**: Def` and `- **Term:** Def`
        var bulletPattern = new Regex(@"(?m)^\s*[-*]\s+\*\*(?:(?<termIn>[^*\r\n:]+?)\s*[:\-–—]\*\*|(?<termOut>[^*\r\n]+?)\*\*\s*[:\-–—])\s*(?<def>[^\r\n]+)");

        // 1. Extract up to 3 flashcards per lesson from bullets
        foreach (var (chBp, chapterEntity, lBp, lessonEntity) in syncedFull)
        {
            if (result.Count >= 380) break;
            if (string.IsNullOrWhiteSpace(lBp.Markdown)) continue;

            int cardsForLesson = 0;
            var matches = bulletPattern.Matches(lBp.Markdown);
            foreach (Match m in matches)
            {
                if (cardsForLesson >= 3 || result.Count >= 380) break;

                var term = (m.Groups["termIn"].Success ? m.Groups["termIn"].Value : m.Groups["termOut"].Value).Trim().TrimEnd(':', '-', '–', '—').Trim();
                var def = m.Groups["def"].Value.Trim();

                if (IsBoilerplateTerm(term, def)) continue;
                if (term.Length < 3 || term.Length > 120 || def.Length < 12) continue;

                if (seenFronts.Add(term))
                {
                    result.Add((lessonEntity, term, def, Truncate($"{chBp.Title} › {lessonEntity.Title}", 500)));
                    cardsForLesson++;
                }
            }
        }

        // 2. Read resources/glossary.md if present (headings ### X.Y. **Term** + paragraph, table rows, and bullets)
        var glossaryPath = Path.Combine(courseRoot, "resources", "glossary.md");
        if (File.Exists(glossaryPath) && result.Count < 380)
        {
            var glossaryLesson = syncedFull.FirstOrDefault(x =>
                x.Blueprint.RelativePath.Replace('\\', '/').Equals("resources/glossary.md", StringComparison.OrdinalIgnoreCase)).Entity
                ?? (syncedFull.Count > 0 ? syncedFull[0].Entity : null);

            if (glossaryLesson != null)
            {
                var glossaryText = File.ReadAllText(glossaryPath, Encoding.UTF8);

                // 2a. Heading pattern: ### 1.1. **Term** followed by definition line(s)
                var headingDefPattern = new Regex(@"(?m)^###\s+\d+\.\d+\.\s+\*\*(?<term>[^*\r\n]+)\*\*\s*\r?\n(?<def>[^\r\n#*|`>][^\r\n]+)");
                foreach (Match m in headingDefPattern.Matches(glossaryText))
                {
                    if (result.Count >= 380) break;
                    var term = m.Groups["term"].Value.Trim().TrimEnd(':', '-', '–', '—').Trim();
                    var def = m.Groups["def"].Value.Trim();
                    if (IsBoilerplateTerm(term, def)) continue;
                    if (term.Length < 3 || term.Length > 120 || def.Length < 12) continue;
                    if (seenFronts.Add(term))
                    {
                        result.Add((glossaryLesson, term, def, "Bootcamp Engineering Glossary"));
                    }
                }

                // 2b. Table rows: | **Term** | Meaning |
                var tableRowPattern = new Regex(@"(?m)^\s*\|\s*\*\*(?<term>[^*\r\n]+)\*\*\s*\|\s*(?<meaning>[^|\r\n]+)");
                foreach (Match m in tableRowPattern.Matches(glossaryText))
                {
                    if (result.Count >= 380) break;
                    var term = m.Groups["term"].Value.Trim().TrimEnd(':', '-', '–', '—').Trim();
                    var meaning = m.Groups["meaning"].Value.Trim();
                    if (IsBoilerplateTerm(term, meaning)) continue;
                    if (term.Length < 3 || term.Length > 120 || meaning.Length < 12) continue;
                    if (seenFronts.Add(term))
                    {
                        result.Add((glossaryLesson, term, meaning, "Bootcamp Engineering Glossary"));
                    }
                }

                // 2c. Bullet format: * **Term:** Meaning or - **Term**: Meaning
                if (result.Count < 380)
                {
                    foreach (Match m in bulletPattern.Matches(glossaryText))
                    {
                        if (result.Count >= 380) break;
                        var term = (m.Groups["termIn"].Success ? m.Groups["termIn"].Value : m.Groups["termOut"].Value).Trim().TrimEnd(':', '-', '–', '—').Trim();
                        var def = m.Groups["def"].Value.Trim();
                        if (IsBoilerplateTerm(term, def)) continue;
                        if (term.Length < 3 || term.Length > 120 || def.Length < 12) continue;
                        if (seenFronts.Add(term))
                        {
                            result.Add((glossaryLesson, term, def, "Bootcamp Engineering Glossary"));
                        }
                    }
                }
            }
        }

        return result;
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

        var courseTitle = Truncate(def.Title, 250);
        var courseSlug = Truncate(def.Slug, 250);

        if (course == null)
        {
            course = new Course
            {
                Id = Guid.NewGuid(),
                InstructorId = instructorId,
                Title = courseTitle,
                Slug = courseSlug,
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
            !string.Equals(course.Title, courseTitle, StringComparison.Ordinal) ||
            !string.Equals(course.Slug, courseSlug, StringComparison.Ordinal) ||
            !string.Equals(course.Description, def.Description, StringComparison.Ordinal) ||
            course.Visibility != "public" ||
            !course.IsPublished;

        if (metadataChanged)
        {
            course.Title = courseTitle;
            course.Slug = courseSlug;
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
        var tagName = Truncate(courseTitle, 50);
        var existing = await db.Tags.FirstOrDefaultAsync(t => t.UserId == userId && t.Name == tagName, ct);
        if (existing != null) return existing;

        var created = new Tag
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = tagName,
            ColorKey = "blue",
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

        var quizzes = new List<QuizBlueprint>();
        for (int stage = 1; stage <= 14; stage++)
        {
            var stageTitle = manifestTitles.TryGetValue(stage, out var mt) ? mt : $"Chặng {stage}";
            var mdFile = Path.Combine(courseRoot, $"{stage}.md");
            string? extractedQuizMd = null;
            if (File.Exists(mdFile))
            {
                var md = File.ReadAllText(mdFile, Encoding.UTF8);
                if ((md.Contains("### Câu") || md.Contains("## 📋 Đề Kiểm Tra") || md.Contains("Đề kiểm tra")) &&
                    Regex.IsMatch(md, @"[-*]?\s*[A-D]\.\s+"))
                {
                    extractedQuizMd = md;
                }
            }

            extractedQuizMd ??= GenerateDotNetStageQuizMarkdown(stage, stageTitle);
            quizzes.Add(new QuizBlueprint(
                $"Đề kiểm tra đánh giá Chặng {stage}: {stageTitle}",
                $"{stage}.md",
                extractedQuizMd,
                null,
                $"{stage}.md",
                stage));
        }

        var chapters = new List<ChapterBlueprint>();
        if (introLessons.Count > 0) chapters.Add(new("Chặng 0: Bắt đầu & Tổng quan lộ trình", 1, introLessons));
        if (stage1To4.Count > 0) chapters.Add(new("Chặng 1–4: Nền tảng C#, Debug & SQL", 2, stage1To4));
        if (stage5To8.Count > 0) chapters.Add(new("Chặng 5–8: ASP.NET Core Web API & Frontend React", 3, stage5To8));
        if (stage9To14.Count > 0) chapters.Add(new("Chặng 9–14: Kiến trúc, Bảo mật, Kiểm thử & Triển khai", 4, stage9To14));

        // Đề tổng hợp theo Chương & Cuối khóa cho lộ trình .NET & React
        if (stage1To4.Count > 0)
        {
            quizzes.Add(new QuizBlueprint(
                "[Đề Chương 1] Nền tảng C#, Debug & SQL (Chặng 1–4)",
                "__exam/dotnet-ch1.md",
                BuildDotNetChapter1ExamMarkdown(),
                null,
                stage1To4[^1].RelativePath,
                4));
        }
        if (stage5To8.Count > 0)
        {
            quizzes.Add(new QuizBlueprint(
                "[Đề Chương 2] ASP.NET Core Web API & Frontend React (Chặng 5–8)",
                "__exam/dotnet-ch2.md",
                BuildDotNetChapter2ExamMarkdown(),
                null,
                stage5To8[^1].RelativePath,
                8));
        }
        if (stage9To14.Count > 0)
        {
            quizzes.Add(new QuizBlueprint(
                "[Đề Chương 3] Kiến trúc, Bảo mật, Kiểm thử & Triển khai (Chặng 9–14)",
                "__exam/dotnet-ch3.md",
                BuildDotNetChapter3ExamMarkdown(),
                null,
                stage9To14[^1].RelativePath,
                14));
        }
        var totalStagesPresent = stage1To4.Count + stage5To8.Count + stage9To14.Count;
        if (totalStagesPresent > 0)
        {
            var finalTargetRel = stage9To14.LastOrDefault()?.RelativePath
                ?? stage5To8.LastOrDefault()?.RelativePath
                ?? stage1To4.LastOrDefault()?.RelativePath
                ?? "14.md";
            quizzes.Add(new QuizBlueprint(
                "[Đề Tổng Hợp Cuối Khóa] Lộ trình Full-Stack .NET & React (Chặng 1–14)",
                "__exam/dotnet-final.md",
                BuildDotNetFinalExamMarkdown(),
                null,
                finalTargetRel,
                14));
        }

        var availableStagePaths = stage1To4.Concat(stage5To8).Concat(stage9To14)
            .Select(l => l.RelativePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var flashcards = BuildDotNetRoadmapFlashcards(availableStagePaths);

        return new CourseBlueprint(def, chapters, quizzes, flashcards);
    }

    private static CourseBlueprint BuildIeltsBlueprint(CourseDefinition def, string courseRoot)
    {
        var ch0Lessons = new List<LessonBlueprint>();
        var p0Lessons = new List<LessonBlueprint>();
        var p1ALessons = new List<LessonBlueprint>();
        var p1BLessons = new List<LessonBlueprint>();
        var p1CLessons = new List<LessonBlueprint>();
        var p2ALessons = new List<LessonBlueprint>();
        var p2BLessons = new List<LessonBlueprint>();
        var p2CLessons = new List<LessonBlueprint>();
        var p3Lessons = new List<LessonBlueprint>();

        var testFiles = new List<(string Rel, string Title, int? Number, string Content)>();
        var answerKeyFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var readmeFile = Path.Combine(courseRoot, "README.md");
        if (!File.Exists(readmeFile)) readmeFile = Path.Combine(courseRoot, "roadmap.md");
        if (File.Exists(readmeFile))
        {
            var readmeMd = File.ReadAllText(readmeFile, Encoding.UTF8);
            if (!string.IsNullOrWhiteSpace(readmeMd))
            {
                var relReadme = Path.GetRelativePath(courseRoot, readmeFile).Replace('\\', '/');
                ch0Lessons.Add(CreateLessonBlueprint(relReadme, relReadme, "Bài 0: Lộ trình & Hướng dẫn tự học IELTS cho người mới", 1, readmeMd, null));
            }
        }

        foreach (var file in Directory.EnumerateFiles(courseRoot, "*.md", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(courseRoot, file).Replace('\\', '/');
            if (IsIgnoredRelativePath(rel, allowStartDocx: false)) continue;

            if (!rel.Contains('/')) continue;

            if (rel.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase) ||
                rel.EndsWith("error-log.md", StringComparison.OrdinalIgnoreCase) ||
                rel.Contains("psychology-concepts-index", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var md = File.ReadAllText(file, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(md)) continue;

            var fileName = Path.GetFileName(rel);
            bool isTestOrAssessment = rel.Contains("/tests/", StringComparison.OrdinalIgnoreCase) ||
                                      rel.Contains("/assessments/", StringComparison.OrdinalIgnoreCase) ||
                                      fileName.Contains("quiz", StringComparison.OrdinalIgnoreCase) ||
                                      fileName.StartsWith("test-", StringComparison.OrdinalIgnoreCase) ||
                                      fileName.Contains("mini-test", StringComparison.OrdinalIgnoreCase) ||
                                      fileName.Contains("checkpoint", StringComparison.OrdinalIgnoreCase);

            if (isTestOrAssessment)
            {
                if (fileName.EndsWith("-answers.md", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith("quiz-key.md", StringComparison.OrdinalIgnoreCase) ||
                    fileName.Contains("answers", StringComparison.OrdinalIgnoreCase))
                {
                    var baseKey = fileName.Replace("-answers.md", ".md", StringComparison.OrdinalIgnoreCase)
                                          .Replace("-retry-answers.md", "-retry.md", StringComparison.OrdinalIgnoreCase)
                                          .Replace("quiz-key.md", "quiz.md", StringComparison.OrdinalIgnoreCase);
                    answerKeyFiles[baseKey] = md;
                }
                else
                {
                    var title = ExtractMarkdownTitle(md, Path.GetFileNameWithoutExtension(rel));
                    var num = ExtractWeekOrStageNumber(fileName);
                    testFiles.Add((rel, title, num, md));
                }
                continue;
            }

            var fallbackTitle = Path.GetFileNameWithoutExtension(rel);
            var lessonTitle = ExtractMarkdownTitle(md, fallbackTitle);
            var weekNum = ExtractWeekOrStageNumber(rel);
            var bp = CreateLessonBlueprint(rel, rel, lessonTitle, 0, md, weekNum);

            if (rel.StartsWith("Phase-0/", StringComparison.OrdinalIgnoreCase))
            {
                if (rel.Contains("/alphabet/", StringComparison.OrdinalIgnoreCase) ||
                    fileName.Equals("daily-guide.md", StringComparison.OrdinalIgnoreCase))
                {
                    ch0Lessons.Add(bp);
                }
                else
                {
                    p0Lessons.Add(bp);
                }
            }
            else if (rel.StartsWith("Phase-1/", StringComparison.OrdinalIgnoreCase))
            {
                if (rel.Contains("/listening/", StringComparison.OrdinalIgnoreCase) ||
                    rel.Contains("/transcripts/", StringComparison.OrdinalIgnoreCase) ||
                    rel.Contains("/audio/", StringComparison.OrdinalIgnoreCase) ||
                    rel.Contains("/pronunciation/", StringComparison.OrdinalIgnoreCase))
                {
                    p1ALessons.Add(bp);
                }
                else if (rel.Contains("/vocabulary/", StringComparison.OrdinalIgnoreCase) ||
                         rel.Contains("/grammar/", StringComparison.OrdinalIgnoreCase) ||
                         fileName.StartsWith("daily-guide", StringComparison.OrdinalIgnoreCase))
                {
                    p1BLessons.Add(bp);
                }
                else
                {
                    p1CLessons.Add(bp);
                }
            }
            else if (rel.StartsWith("Phase-2/", StringComparison.OrdinalIgnoreCase))
            {
                if (rel.Contains("/listening/", StringComparison.OrdinalIgnoreCase) ||
                    rel.Contains("/transcripts/", StringComparison.OrdinalIgnoreCase) ||
                    rel.Contains("/audio/", StringComparison.OrdinalIgnoreCase) ||
                    rel.Contains("/reading/", StringComparison.OrdinalIgnoreCase))
                {
                    p2ALessons.Add(bp);
                }
                else if (rel.Contains("/speaking/", StringComparison.OrdinalIgnoreCase) ||
                         rel.Contains("/writing/", StringComparison.OrdinalIgnoreCase))
                {
                    p2BLessons.Add(bp);
                }
                else
                {
                    p2CLessons.Add(bp);
                }
            }
            else if (rel.StartsWith("Phase-3/", StringComparison.OrdinalIgnoreCase) ||
                     rel.StartsWith("packages/", StringComparison.OrdinalIgnoreCase))
            {
                p3Lessons.Add(bp);
            }
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
        int chOrder = 1;
        var sortedCh0 = SortIeltsLessons(ch0Lessons);
        var sortedP0 = SortIeltsLessons(p0Lessons);
        var sortedP1A = SortIeltsLessons(p1ALessons);
        var sortedP1B = SortIeltsLessons(p1BLessons);
        var sortedP1C = SortIeltsLessons(p1CLessons);
        var sortedP2A = SortIeltsLessons(p2ALessons);
        var sortedP2B = SortIeltsLessons(p2BLessons);
        var sortedP2C = SortIeltsLessons(p2CLessons);
        var sortedP3 = SortIeltsLessons(p3Lessons);

        if (sortedCh0.Count > 0) chapters.Add(new("Chặng 0: Làm quen & Lộ trình cho người mới", chOrder++, sortedCh0));
        if (sortedP0.Count > 0) chapters.Add(new("Phase 0: Khởi động Phát âm, Từ vựng & Đọc cơ bản", chOrder++, sortedP0));
        if (sortedP1A.Count > 0) chapters.Add(new("Phase 1A: Nền tảng Nghe & Phát âm (Listening & Pronunciation)", chOrder++, sortedP1A));
        if (sortedP1B.Count > 0) chapters.Add(new("Phase 1B: Nền tảng Từ vựng & Ngữ pháp (Vocab & Grammar)", chOrder++, sortedP1B));
        if (sortedP1C.Count > 0) chapters.Add(new("Phase 1C: Nền tảng Đọc, Nói & Viết (Reading, Speaking & Writing)", chOrder++, sortedP1C));
        if (sortedP2A.Count > 0) chapters.Add(new("Phase 2A: Tăng tốc Nghe & Đọc hiểu (Listening & Reading)", chOrder++, sortedP2A));
        if (sortedP2B.Count > 0) chapters.Add(new("Phase 2B: Tăng tốc Nói & Viết (Speaking & Writing)", chOrder++, sortedP2B));
        if (sortedP2C.Count > 0) chapters.Add(new("Phase 2C: Từ vựng, Ngữ pháp & Luyện đề tăng tốc", chOrder++, sortedP2C));
        if (sortedP3.Count > 0) chapters.Add(new("Phase 3: Mastery & Tổng ôn chuyên sâu", chOrder++, sortedP3));

        var ieltsQuizzes = new List<QuizBlueprint>();
        foreach (var tf in testFiles)
        {
            var fileName = Path.GetFileName(tf.Rel);
            answerKeyFiles.TryGetValue(fileName, out var ansMd);

            string targetLessonRel;
            if (tf.Rel.StartsWith("Phase-0/", StringComparison.OrdinalIgnoreCase))
            {
                targetLessonRel = sortedP0.FirstOrDefault(l => l.WeekOrStageNumber == tf.Number)?.RelativePath
                    ?? sortedP0.FirstOrDefault()?.RelativePath
                    ?? sortedCh0.FirstOrDefault()?.RelativePath
                    ?? string.Empty;
            }
            else if (tf.Rel.StartsWith("Phase-1/", StringComparison.OrdinalIgnoreCase))
            {
                targetLessonRel = sortedP1B.FirstOrDefault(l => l.WeekOrStageNumber == tf.Number)?.RelativePath
                    ?? sortedP1A.FirstOrDefault(l => l.RelativePath.Contains($"cycle{tf.Number}"))?.RelativePath
                    ?? sortedP1B.FirstOrDefault()?.RelativePath
                    ?? string.Empty;
            }
            else if (tf.Rel.StartsWith("Phase-2/", StringComparison.OrdinalIgnoreCase))
            {
                targetLessonRel = sortedP2A.FirstOrDefault(l => l.RelativePath.Contains($"cycle{tf.Number}"))?.RelativePath
                    ?? sortedP2C.FirstOrDefault(l => l.WeekOrStageNumber == tf.Number)?.RelativePath
                    ?? sortedP2A.FirstOrDefault()?.RelativePath
                    ?? string.Empty;
            }
            else
            {
                targetLessonRel = sortedP3.FirstOrDefault()?.RelativePath ?? string.Empty;
            }

            var (quizMd, answerKeyMd) = FormatIeltsTestToQuizMarkdown(tf.Content, ansMd, tf.Title);
            ieltsQuizzes.Add(new QuizBlueprint(
                tf.Title,
                tf.Rel,
                quizMd,
                answerKeyMd,
                targetLessonRel,
                tf.Number));
        }

        // Parse IELTS Phase-3 CSV vocabulary flashcards if present
        var flashcards = new List<FlashcardSpec>();
        var fcDir = Path.Combine(courseRoot, "Phase-3", "flashcards");
        if (Directory.Exists(fcDir))
        {
            var fallbackP3Rel = sortedP3.FirstOrDefault(l => l.RelativePath.Contains("vocabulary", StringComparison.OrdinalIgnoreCase))?.RelativePath
                ?? sortedP3.FirstOrDefault()?.RelativePath
                ?? sortedP1B.FirstOrDefault()?.RelativePath
                ?? sortedCh0.FirstOrDefault()?.RelativePath
                ?? string.Empty;

            foreach (var csvFile in Directory.GetFiles(fcDir, "*.csv").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var relDeck = Path.GetRelativePath(courseRoot, csvFile).Replace('\\', '/');
                flashcards.AddRange(ParseCsvFlashcards(File.ReadAllText(csvFile, Encoding.UTF8), fallbackP3Rel, relDeck));
            }
        }

        var ieltsCh1Target = sortedP1C.FirstOrDefault()?.RelativePath
            ?? sortedP1B.FirstOrDefault()?.RelativePath
            ?? sortedP1A.FirstOrDefault()?.RelativePath
            ?? sortedP0.FirstOrDefault()?.RelativePath
            ?? sortedCh0.FirstOrDefault()?.RelativePath
            ?? string.Empty;
        if (!string.IsNullOrEmpty(ieltsCh1Target))
        {
            ieltsQuizzes.Add(new QuizBlueprint(
                "[Đề Chương 1] IELTS Phase 0–1: Nền tảng Phát âm, Từ vựng, Ngữ pháp & Nghe Đọc",
                "__exam/ielts-ch1.md",
                BuildIeltsChapter1ExamMarkdown(),
                null,
                ieltsCh1Target,
                1));
        }

        var ieltsCh2Target = sortedP2C.FirstOrDefault()?.RelativePath
            ?? sortedP2B.FirstOrDefault()?.RelativePath
            ?? sortedP2A.FirstOrDefault()?.RelativePath
            ?? ieltsCh1Target;
        if (!string.IsNullOrEmpty(ieltsCh2Target))
        {
            ieltsQuizzes.Add(new QuizBlueprint(
                "[Đề Chương 2] IELTS Phase 2: Tăng tốc 4 Kỹ năng Listening, Reading, Speaking & Writing",
                "__exam/ielts-ch2.md",
                BuildIeltsChapter2ExamMarkdown(),
                null,
                ieltsCh2Target,
                2));
        }

        var ieltsCh3Target = sortedP3.FirstOrDefault()?.RelativePath ?? ieltsCh2Target;
        if (!string.IsNullOrEmpty(ieltsCh3Target))
        {
            ieltsQuizzes.Add(new QuizBlueprint(
                "[Đề Chương 3] IELTS Phase 3: Mastery & Chiến thuật Band 6.5+",
                "__exam/ielts-ch3.md",
                BuildIeltsChapter3ExamMarkdown(),
                null,
                ieltsCh3Target,
                3));
            ieltsQuizzes.Add(new QuizBlueprint(
                "[Đề Tổng Hợp Cuối Khóa] Thi Thử Tổng Hợp IELTS 4 Kỹ Năng",
                "__exam/ielts-final.md",
                BuildIeltsFinalExamMarkdown(),
                null,
                ieltsCh3Target,
                4));
        }

        return new CourseBlueprint(def, chapters, ieltsQuizzes, flashcards);
    }

    private static CourseBlueprint BuildManifestDrivenBlueprint(CourseDefinition def, string courseRoot)
    {
        var overviewLessons = new List<LessonBlueprint>();

        // For PersonalFinance, preserve the canonical 14-lesson structure (README + learning-roadmap + 12 weeks)
        // verified by regression test RealSqlite_ReconcileDuplicateLegacyPersonalFinanceLessons_MergesOrphansAndPreservesProgress.
        // For other courses, keep at most 1 orientation lesson and filter out meta files (progress, glossary, resources, etc.).
        if (def.SourceFolder.Equals("PersonalFinance", StringComparison.OrdinalIgnoreCase))
        {
            int overviewIdx = 1;
            foreach (var candRel in new[] { "README.md", "learning-roadmap.md" })
            {
                var abs = Path.Combine(courseRoot, candRel.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(abs)) continue;
                var md = File.ReadAllText(abs, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(md)) continue;
                var title = candRel == "README.md"
                    ? "Bài 0: Tổng quan lộ trình & Cách học cho người mới"
                    : ExtractMarkdownTitle(md, Path.GetFileNameWithoutExtension(candRel));
                overviewLessons.Add(CreateLessonBlueprint(candRel, candRel, title, overviewIdx++, md, null));
            }
        }
        else
        {
            var orientationCandidates = new[] { "README.md", "learning-roadmap.md", "setup.md", "study-method.md" };
            var chosenOrientation = orientationCandidates.FirstOrDefault(c =>
                File.Exists(Path.Combine(courseRoot, c.Replace('/', Path.DirectorySeparatorChar))));

            if (chosenOrientation != null)
            {
                var abs = Path.Combine(courseRoot, chosenOrientation.Replace('/', Path.DirectorySeparatorChar));
                var md = File.ReadAllText(abs, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(md))
                {
                    var title = "Bài 0: Tổng quan lộ trình & Cách học cho người mới";
                    overviewLessons.Add(CreateLessonBlueprint(chosenOrientation, chosenOrientation, title, 1, md, null));
                }
            }
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

    private static (string QuizMd, string? AnsMd) FormatIeltsTestToQuizMarkdown(string rawTestMd, string? rawAnsMd, string quizTitle)
    {
        if (Regex.IsMatch(rawTestMd, @"^\s*[-*]?\s*[A-D]\.\s+", RegexOptions.Multiline))
        {
            return (rawTestMd, rawAnsMd);
        }

        var lines = rawTestMd.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var questions = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;
            var m = Regex.Match(trimmed, @"^(?:(?:\*\*)?(?:Q\d+|\d+)[\.\:]\s*(?:\*\*)?|(?:\*\*)?Question\s+\d+[\.\:]\s*(?:\*\*)?)(.+)$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                var qText = m.Groups[1].Value.Trim().Trim('*', '_', '`');
                if (qText.Length >= 3 && !qText.StartsWith("Điểm", StringComparison.OrdinalIgnoreCase) && !qText.StartsWith("Phần", StringComparison.OrdinalIgnoreCase))
                {
                    questions.Add(qText);
                }
            }
        }

        if (questions.Count == 0)
        {
            questions.Add($"Kiểm tra đánh giá kiến thức: {quizTitle}");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"# 📝 {quizTitle}\n");

        for (int i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            sb.AppendLine($"### Câu {i + 1}: [Q{i + 1:D2}] {q}");
            sb.AppendLine("- A. Phương án đúng / Chính xác theo ngữ cảnh bài học");
            sb.AppendLine("- B. Phương án sai / Không phù hợp ngữ pháp");
            sb.AppendLine("- C. Cần bổ sung thêm thông tin chi tiết");
            sb.AppendLine("- D. Không thể xác định từ dữ liệu bài học");
            sb.AppendLine();
            sb.AppendLine("> [!TIP]");
            sb.AppendLine("> **Đáp án:** A");
            sb.AppendLine($"> **Giải thích:** Dựa trên nội dung chuẩn của bài kiểm tra '{quizTitle}'.\n");
        }

        return (sb.ToString(), null);
    }

    private static string GenerateDotNetStageQuizMarkdown(int stage, string stageTitle)
    {
        return stage switch
        {
            1 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 1: C# Cơ Bản & Ứng Dụng Console

            ### Câu 1: [Q01-01] Kiểu dữ liệu nào trong C# phù hợp nhất để lưu trữ và tính toán tiền tệ chính xác?
            - A. float
            - B. double
            - C. decimal
            - D. long

            > [!TIP]
            > **Đáp án:** C
            > **Giải thích:** Kiểu decimal có độ chính xác 128-bit và giảm thiểu sai số biểu diễn số thực dấu phẩy động, chuẩn cho tài chính.

            ### Câu 2: [Q01-02] Phương thức nào được dùng để chuyển đổi chuỗi sang số nguyên an toàn không gây ném ngoại lệ?
            - A. int.Parse(str)
            - B. Convert.ToInt32(str)
            - C. (int)str
            - D. int.TryParse(str, out var result)

            > [!TIP]
            > **Đáp án:** D
            > **Giải thích:** int.TryParse trả về bool cho biết chuyển đổi thành công hay không và không ném ngoại lệ khi input sai định dạng.

            ### Câu 3: [Q01-03] Trong C# Collection List<T>, thuộc tính nào trả về số lượng phần tử hiện có?
            - A. Length
            - B. Count
            - C. Size
            - D. Capacity

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Mảng Array dùng .Length, còn List<T> dùng .Count để lấy số lượng phần tử hiện có.

            ### Câu 4: [Q01-04] Lệnh nào dùng để in văn bản ra màn hình Console và tự động xuống dòng?
            - A. Console.Write()
            - B. Console.Print()
            - C. Console.WriteLine()
            - D. Console.Out()

            > [!TIP]
            > **Đáp án:** C
            > **Giải thích:** Console.WriteLine in chuỗi rồi xuống dòng, còn Console.Write giữ con trỏ tại dòng hiện tại.

            ### Câu 5: [Q01-05] Khái niệm Top-level statements trong C# hiện đại cho phép điều gì?
            - A. Khai báo biến không cần kiểu dữ liệu
            - B. Viết các câu lệnh thực thi trực tiếp trong Program.cs mà không cần bọc class Program và static void Main
            - C. Bỏ qua hoàn toàn việc biên dịch mã nguồn
            - D. Chạy C# trực tiếp trên trình duyệt mà không cần .NET SDK

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Top-level statements giúp đơn giản hóa mã khởi đầu của ứng dụng bằng cách bỏ qua boilerplate class/Main.
            """,

            2 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 2: Giải Quyết Bài Toán & Debug

            ### Câu 1: [Q02-01] Khi debug trong VS Code hoặc Visual Studio, phím tắt nào thường dùng cho "Step Over" (chạy qua dòng hiện tại)?
            - A. F5
            - B. F10
            - C. F11
            - D. Shift+F5

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** F10 thực hiện Step Over dòng hiện tại, còn F11 thực hiện Step Into để đi sâu vào bên trong hàm.

            ### Câu 2: [Q02-02] Điểm dừng (Breakpoint) có tác dụng gì trong quá trình chạy ứng dụng khi gỡ lỗi?
            - A. Tự động sửa lỗi cú pháp trong mã nguồn
            - B. Tạm dừng thực thi chương trình tại dòng chỉ định để quan sát biến và Call Stack
            - C. Đóng ứng dụng ngay lập tức
            - D. Ghi log dữ liệu ra file văn bản

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Breakpoint giúp lập trình viên đóng băng ứng dụng tại vị trí nghi ngờ lỗi để kiểm tra trạng thái bộ nhớ.

            ### Câu 3: [Q02-03] Lỗi Off-by-one thường xảy ra trong trường hợp nào dưới đây?
            - A. Phép toán chia một số cho 0
            - B. Vòng lặp duyệt mảng lặp thừa hoặc thiếu 1 phần tử (ví dụ <= count thay vì < count)
            - C. Ép kiểu dữ liệu thất bại
            - D. Tràn ngăn xếp StackOverflow

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Off-by-one là lỗi logic kinh điển khi biên giới điều kiện vòng lặp duyệt chệch 1 chỉ số so với giới hạn mảng.

            ### Câu 4: [Q02-04] Độ phức tạp thời gian của thuật toán tìm kiếm nhị phân (Binary Search) trên mảng đã sắp xếp là gì?
            - A. O(1)
            - B. O(n)
            - C. O(log n)
            - D. O(n^2)

            > [!TIP]
            > **Đáp án:** C
            > **Giải thích:** Tìm kiếm nhị phân chia đôi không gian tìm kiếm sau mỗi bước nên có độ phức tạp O(log n).

            ### Câu 5: [Q02-05] Cửa sổ "Call Stack" trong debugger hiển thị thông tin gì?
            - A. Danh sách các biến toàn cục trong ứng dụng
            - B. Chuỗi các hàm đang được gọi lồng nhau dẫn đến vị trí thực thi hiện tại
            - C. Dung lượng bộ nhớ RAM mà ứng dụng đang chiếm dụng
            - D. Tất cả các file mã nguồn có trong solution

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Call Stack hiển thị dấu vết ngăn xếp lời gọi từ hàm xuất phát tới hàm đang thực thi.
            """,

            3 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 3: C# Cho Ứng Dụng Nhỏ & Quản Lý Mã Nguồn

            ### Câu 1: [Q03-01] Khối lệnh finally trong cấu trúc try-catch-finally có đặc điểm gì?
            - A. Chỉ chạy khi có ngoại lệ xảy ra trong khối try
            - B. Luôn luôn chạy dù có ngoại lệ hay không, thường dùng để giải phóng tài nguyên
            - C. Chỉ chạy khi không có ngoại lệ
            - D. Dùng để ném lại ngoại lệ ra ngoài

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Khối finally luôn được đảm bảo thực thi kể cả khi có return hay exception.

            ### Câu 2: [Q03-02] Câu lệnh using trong C# áp dụng cho các đối tượng triển khai interface nào?
            - A. IEnumerable
            - B. IComparable
            - C. IDisposable
            - D. ICloneable

            > [!TIP]
            > **Đáp án:** C
            > **Giải thích:** using statement tự động gọi Dispose() trên các đối tượng triển khai IDisposable khi ra khỏi scope.

            ### Câu 3: [Q03-03] Trong Git, lệnh nào dùng để kiểm tra trạng thái các file đã thay đổi trong working directory?
            - A. git log
            - B. git status
            - C. git diff
            - D. git branch

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** git status liệt kê các file untracked, modified và staged trong working tree.

            ### Câu 4: [Q03-04] Từ khóa truy cập internal trong C# có ý nghĩa gì?
            - A. Chỉ truy cập được trong cùng class
            - B. Truy cập được từ bất kỳ assembly nào
            - C. Chỉ truy cập được trong cùng một assembly (project)
            - D. Chỉ truy cập được từ class kế thừa

            > [!TIP]
            > **Đáp án:** C
            > **Giải thích:** internal giới hạn phạm vi truy cập bên trong cùng một compiled assembly.

            ### Câu 5: [Q03-05] Lớp nào trong System.IO cung cấp các phương thức tĩnh tiện lợi để đọc ghi file văn bản?
            - A. FileStream
            - B. Directory
            - C. Path
            - D. File

            > [!TIP]
            > **Đáp án:** D
            > **Giải thích:** System.IO.File cung cấp File.ReadAllText, File.WriteAllText, File.Exists rất tiện dụng.
            """,

            4 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 4: Cơ Sở Dữ Liệu Quan Hệ & SQL

            ### Câu 1: [Q04-01] Khóa chính (Primary Key) trong cơ sở dữ liệu quan hệ có đặc tính nào?
            - A. Cho phép chứa giá trị NULL
            - B. Giá trị duy nhất trên mỗi hàng và không được phép chứa NULL
            - C. Tự động tăng và có thể trùng lặp
            - D. Chỉ có thể áp dụng cho kiểu số nguyên

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Primary Key định danh duy nhất từng bản ghi trong bảng và bắt buộc NOT NULL.

            ### Câu 2: [Q04-02] Phép LEFT JOIN giữa bảng A và bảng B trả về kết quả như thế nào?
            - A. Chỉ các hàng có khóa khớp ở cả hai bảng
            - B. Toàn bộ các hàng từ bảng A kèm các hàng khớp từ bảng B (nếu không khớp điền NULL)
            - C. Toàn bộ các hàng từ bảng B
            - D. Tích đề-các của hai bảng

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** LEFT JOIN giữ lại tất cả bản ghi từ bảng bên trái và ghép cột NULL nếu bảng phải không có hàng khớp.

            ### Câu 3: [Q04-03] Mệnh đề nào trong SQL dùng để lọc dữ liệu dựa trên kết quả của hàm tổng hợp (Aggregate Function)?
            - A. WHERE
            - B. ORDER BY
            - C. HAVING
            - D. GROUP BY

            > [!TIP]
            > **Đáp án:** C
            > **Giải thích:** WHERE lọc trên từng hàng trước khi gom nhóm, còn HAVING lọc sau khi đã GROUP BY và tổng hợp.

            ### Câu 4: [Q04-04] Tính chất ACID trong giao dịch cơ sở dữ liệu gồm những yếu tố nào?
            - A. Atomicity, Consistency, Isolation, Durability
            - B. Availability, Consistency, Integrity, Durability
            - C. Accuracy, Concurrency, Isolation, Dependability
            - D. Authentication, Confidentiality, Integrity, Non-repudiation

            > [!TIP]
            > **Đáp án:** A
            > **Giải thích:** ACID là 4 thuộc tính đảm bảo giao dịch CSDL an toàn và tin cậy.

            ### Câu 5: [Q04-05] Index (Chỉ mục) trong cơ sở dữ liệu có ưu và nhược điểm chính là gì?
            - A. Tăng tốc độ ghi nhưng làm chậm đọc
            - B. Tăng tốc độ truy vấn đọc nhưng tốn thêm dung lượng đĩa và làm chậm thao tác INSERT/UPDATE
            - C. Đảm bảo dữ liệu không bị mất khi mất điện
            - D. Tự động nén kích thước bảng dữ liệu

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Index tạo cấu trúc B-Tree giúp tìm kiếm O(log N) thay vì Table Scan, nhưng phải cập nhật cây khi ghi dữ liệu.
            """,

            5 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 5: HTTP & ASP.NET Core Minimal APIs

            ### Câu 1: [Q05-01] Phương thức HTTP nào mang tính chất Idempotent và Safe, dùng để truy vấn dữ liệu mà không thay đổi trạng thái server?
            - A. POST
            - B. GET
            - C. PATCH
            - D. CONNECT

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** GET an toàn và lũy đẳng, cho phép client gọi nhiều lần mà không tạo side-effect thay đổi trạng thái tài nguyên.

            ### Câu 2: [Q05-02] Mã trạng thái HTTP 201 Created chuẩn RESTful biểu thị điều gì?
            - A. Yêu cầu thành công và không có nội dung trả về
            - B. Tài nguyên mới đã được tạo lập thành công trên máy chủ
            - C. Yêu cầu không hợp lệ từ client
            - D. Tài nguyên đã tồn tại từ trước

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** HTTP 201 Created là phản hồi chuẩn khi tạo mới tài nguyên thành công qua POST.

            ### Câu 3: [Q05-03] Vòng đời dịch vụ nào trong Dependency Injection của ASP.NET Core tạo một instance mới cho mỗi HTTP Request?
            - A. Transient
            - B. Scoped
            - C. Singleton
            - D. PerThread

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Scoped tạo một thể hiện duy nhất trong suốt vòng đời của một HTTP Request và chia sẻ giữa các service cùng scope.

            ### Câu 4: [Q05-04] Middleware trong pipeline xử lý của ASP.NET Core hoạt động theo nguyên lý nào?
            - A. Chạy song song không đồng bộ
            - B. Chuỗi trách nhiệm (Chain of Responsibility) thực thi theo thứ tự đăng ký cả chiều request và response
            - C. Chỉ thực thi khi có lỗi xảy ra
            - D. Chạy độc lập ngoài tiến trình ứng dụng

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Middleware nối tiếp nhau theo thứ tự; mỗi middleware có thể can thiệp trước và sau khi gọi `next()`.

            ### Câu 5: [Q05-05] Trong Minimal API của .NET, phương thức nào dùng để đăng ký một GET endpoint?
            - A. app.UseGet()
            - B. app.MapGet()
            - C. app.RegisterRoute()
            - D. app.CreateGet()

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** EndpointRouteBuilderExtensions.MapGet() được dùng để ánh xạ URI tới handler trong Minimal API.
            """,

            6 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 6: Nền Tảng Frontend (HTML, CSS & JavaScript)

            ### Câu 1: [Q06-01] Trong CSS Box Model, thứ tự từ trong ra ngoài của một phần tử là gì?
            - A. Content -> Border -> Padding -> Margin
            - B. Content -> Padding -> Border -> Margin
            - C. Content -> Margin -> Padding -> Border
            - D. Padding -> Content -> Border -> Margin

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** CSS Box Model gồm lõi Content, tiếp đến khoảng đệm Padding, đường viền Border và lề ngoài Margin.

            ### Câu 2: [Q06-02] Thuộc tính CSS box-sizing: border-box có tác dụng gì?
            - A. Thêm đường viền nét đôi cho phần tử
            - B. Tính cả padding và border vào tổng chiều rộng và chiều cao quy định của phần tử
            - C. Xóa bỏ margin bên ngoài phần tử
            - D. Tự động căn giữa phần tử theo chiều ngang

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** border-box giúp việc tính toán kích thước layout ổn định, width/height bao gồm cả content, padding và border.

            ### Câu 3: [Q06-03] Từ khóa const trong JavaScript ES6 khai báo biến có đặc tính gì?
            - A. Biến có phạm vi function và có thể gán lại giá trị bất kỳ lúc nào
            - B. Biến có phạm vi block (khối mã) và không thể gán lại (reassign) giá trị mới
            - C. Biến toàn cục bất biến không thể thay đổi thuộc tính bên trong object
            - D. Tự động đồng bộ biến với server

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** const tạo định danh bất biến về mặt phép gán (assignment) trong phạm vi block scope.

            ### Câu 4: [Q06-04] Phương thức fetch() trong JavaScript trả về kiểu dữ liệu gì?
            - A. Chuỗi JSON
            - B. Đối tượng XMLHttpRequest
            - C. Đối tượng Promise phân giải thành Response
            - D. Callback function

            > [!TIP]
            > **Đáp án:** C
            > **Giải thích:** fetch() API hiện đại trả về một Promise bất đồng bộ giải quyết thành Response stream.

            ### Câu 5: [Q06-05] Trong CSS Flexbox, thuộc tính nào trên container dùng để căn chỉnh các phần tử con theo trục chính (Main Axis)?
            - A. align-items
            - B. justify-content
            - C. flex-direction
            - D. align-content

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** justify-content căn chỉnh item theo Main Axis, còn align-items căn chỉnh theo Cross Axis.
            """,

            7 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 7: React & Hoàn Thiện Ứng Dụng

            ### Câu 1: [Q07-01] Hook useState trong React dùng để làm gì?
            - A. Gọi API bất đồng bộ khi component mount
            - B. Khai báo và quản lý biến trạng thái (state) nội bộ của component
            - C. Thao tác trực tiếp với DOM thật
            - D. Tối ưu hóa bộ nhớ đệm cache

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** useState trả về một cặp gồm giá trị state hiện tại và hàm cập nhật state kích hoạt re-render.

            ### Câu 2: [Q07-02] Trong React, luồng dữ liệu (Data Flow) truyền giữa các component tuân theo quy tắc nào?
            - A. Hai chiều (Two-way data binding) mặc định
            - B. Một chiều từ cha xuống con thông qua props (Unidirectional Data Flow)
            - C. Tự do giữa các component ngang hàng
            - D. Chỉ truyền qua context

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** React tuân thủ luồng dữ liệu một chiều nghiêm ngặt giúp trạng thái ứng dụng dễ dự đoán và debug.

            ### Câu 3: [Q07-03] Mảng phụ thuộc rỗng [] trong hook useEffect có ý nghĩa gì?
            - A. Effect chạy sau mỗi lần component re-render
            - B. Effect chỉ chạy một lần duy nhất sau khi component được mount lần đầu
            - C. Effect không bao giờ chạy
            - D. Effect chạy lại khi bất kỳ state nào thay đổi

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Dependencies array rỗng tương đương với componentDidMount, chỉ kích hoạt một lần duy nhất.

            ### Câu 4: [Q07-04] Tại sao cần cung cấp thuộc tính key duy nhất khi render một danh sách phần tử trong React?
            - A. Để áp dụng CSS animation
            - B. Giúp thuật toán Reconciliation của React nhận diện phần tử nào đã thay đổi, thêm hay xóa để cập nhật DOM hiệu quả
            - C. Bắt buộc để gắn event handler onClick
            - D. Đảm bảo dữ liệu được mã hóa an toàn

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** key giúp React Virtual DOM phân biệt các item trong danh sách và tái sử dụng node DOM tối ưu.

            ### Câu 5: [Q07-05] Cú pháp JSX trong React là gì?
            - A. Một ngôn ngữ lập trình độc lập thay thế hoàn toàn JS
            - B. Cú pháp mở rộng của JavaScript cho phép viết cấu trúc giao diện tương tự HTML trong code JS
            - C. Một template engine chạy trên máy chủ
            - D. Thư viện UI của Google

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** JSX là cú pháp mở rộng được Babel/SWC chuyển đổi thành các lời gọi React.createElement().
            """,

            8 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 8: C# Chuyên Sâu (LINQ, Async/Await & Generics)

            ### Câu 1: [Q08-01] Phương thức LINQ nào được dùng để chiếu (transform) từng phần tử của danh sách sang dạng dữ liệu mới?
            - A. Where
            - B. Select
            - C. GroupBy
            - D. Aggregate

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Select ánh xạ mỗi phần tử nguồn sang một phần tử đích tương ứng (phép chiếu Projection).

            ### Câu 2: [Q08-02] Từ khóa await trong C# có tác dụng gì khi gọi một phương thức bất đồng bộ trả về Task?
            - A. Chặn đứng luồng (blocking thread) cho đến khi tác vụ hoàn thành
            - B. Tạm dừng thực thi phương thức hiện tại và nhả luồng cho tác vụ khác cho đến khi Task hoàn tất mà không chặn luồng
            - C. Chạy tác vụ trên tiến trình riêng của hệ điều hành
            - D. Hủy bỏ tác vụ nếu thời gian chờ quá 5 giây

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** await giải phóng thread hiện tại quay lại ThreadPool phục vụ request khác trong lúc đợi I/O.

            ### Câu 3: [Q08-03] Phương thức LINQ First() khác với FirstOrDefault() như thế nào khi tập hợp rỗng?
            - A. First() trả về null còn FirstOrDefault() ném ngoại lệ
            - B. First() ném ngoại lệ InvalidOperationException còn FirstOrDefault() trả về giá trị mặc định của kiểu (null hoặc 0)
            - C. Cả hai phương thức đều ném ngoại lệ
            - D. Cả hai phương thức đều trả về null

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** First() yêu cầu tối thiểu 1 phần tử thỏa mãn; nếu không có sẽ ném ngoại lệ, còn FirstOrDefault() trả default(T).

            ### Câu 4: [Q08-04] Khái niệm "Deferred Execution" (Thực thi trì hoãn) trong LINQ có nghĩa là gì?
            - A. Truy vấn được lưu vĩnh viễn trong bộ nhớ đệm
            - B. Truy vấn chỉ thực sự được thực thi khi kết quả được duyệt qua (qua foreach, ToList, Count)
            - C. Truy vấn luôn chạy chậm hơn vòng lặp thông thường
            - D. Truy vấn chỉ chạy sau khi ứng dụng tắt

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** LINQ to Objects và LINQ to Entities sử dụng Deferred Execution, chỉ lặp qua nguồn khi có lệnh tiêu thụ dữ liệu.

            ### Câu 5: [Q08-05] Generic trong C# (ví dụ class Repository<T>) đem lại lợi ích cốt lõi nào?
            - A. Tăng dung lượng bộ nhớ stack
            - B. Tái sử dụng mã nguồn, kiểm tra an toàn kiểu tại thời điểm biên dịch và tránh boxing/unboxing
            - C. Ép buộc mọi đối tượng phải kế thừa từ class object
            - D. Tự động sinh cơ sở dữ liệu

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Generics cho phép tạo thuật toán và cấu trúc dữ liệu không phụ thuộc kiểu cụ thể nhưng vẫn Type-safe tuyệt đối.
            """,

            9 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 9: Thiết Kế Code & Architecture Patterns

            ### Câu 1: [Q09-01] Nguyên lý Đơn trách nhiệm (Single Responsibility Principle - chữ S trong SOLID) phát biểu điều gì?
            - A. Mỗi class chỉ nên có duy nhất một hàm public
            - B. Một class chỉ nên có một lý do duy nhất để thay đổi (chịu trách nhiệm về một chức năng duy nhất)
            - C. Mỗi project chỉ có một file cấu hình
            - D. Mỗi bảng chỉ có một cột khóa chính

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** SRP quy định mỗi module/class chỉ phục vụ một nhóm actor và có một lý do duy nhất để bị chỉnh sửa.

            ### Câu 2: [Q09-02] Mẫu thiết kế Repository Pattern đóng vai trò gì trong kiến trúc ứng dụng?
            - A. Đóng gói logic truy cập dữ liệu và trừu tượng hóa tầng lưu trữ khỏi tầng nghiệp vụ (Business Logic)
            - B. Quản lý phiên đăng nhập của người dùng
            - C. Thay thế hoàn toàn Entity Framework Core
            - D. Định tuyến các HTTP Request trong API

            > [!TIP]
            > **Đáp án:** A
            > **Giải thích:** Repository Pattern hoạt động như một tập hợp đối tượng trong bộ nhớ, che giấu chi tiết SQL/ORM phía sau interface.

            ### Câu 3: [Q09-03] Trong Clean Architecture, quy tắc phụ thuộc (Dependency Rule) quy định luồng phụ thuộc như thế nào?
            - A. Tầng Domain phụ thuộc vào tầng CSDL và Web UI
            - B. Mọi phụ thuộc mã nguồn chỉ được hướng vào bên trong (hướng về tầng Domain/Entities cốt lõi)
            - C. Các tầng phụ thuộc vòng tròn lẫn nhau
            - D. Tầng Infrastructure nằm ở vị trí trung tâm

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Quy tắc cốt lõi của Clean Architecture: mã nguồn ở vòng ngoài phụ thuộc vào vòng trong; vòng trong không biết gì về vòng ngoài.

            ### Câu 4: [Q09-04] Mẫu thiết kế Unit of Work giải quyết bài toán gì khi kết hợp với Repository Pattern?
            - A. Tạo kết nối CSDL mới cho mỗi bảng
            - B. Duy trì danh sách các đối tượng bị tác động và phối hợp ghi nhận tất cả thay đổi trong một transaction duy nhất
            - C. Tự động viết unit test cho các class
            - D. Chia ứng dụng thành các microservice

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Unit of Work đảm bảo tính nguyên tử: nhiều thao tác repository được commit cùng nhau trong một giao dịch.

            ### Câu 5: [Q09-05] Nguyên lý Đảo ngược phụ thuộc (Dependency Inversion Principle - chữ D trong SOLID) khuyến nghị điều gì?
            - A. Module cấp cao nên phụ thuộc trực tiếp vào module cấp thấp
            - B. Module cấp cao không nên phụ thuộc vào module cấp thấp; cả hai nên phụ thuộc vào abstractions (interfaces)
            - C. Luôn sử dụng biến toàn cục để tránh truyền tham số
            - D. Không dùng interface trong mã nguồn

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** DIP tách rời sự phụ thuộc trực tiếp giữa các module bằng cách đặt interface ở giữa làm ranh giới ngữ nghĩa.
            """,

            10 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 10: SQL & Entity Framework Core Chuyên Sâu

            ### Câu 1: [Q10-01] Phương thức AsNoTracking() trong EF Core có tác dụng gì đối với các câu lệnh truy vấn đọc?
            - A. Khóa bảng dữ liệu ngăn không cho cập nhật
            - B. Bỏ qua việc theo dõi thay đổi (Change Tracker) giúp giảm tiêu thụ bộ nhớ và tăng tốc độ truy vấn đáng kể
            - C. Tự động xóa dữ liệu sau khi đọc xong
            - D. Bỏ qua kiểm tra ràng buộc khóa ngoại

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Khi chỉ đọc dữ liệu để hiển thị, AsNoTracking() tắt Change Tracker giúp EF Core xử lý nhanh hơn nhiều.

            ### Câu 2: [Q10-02] Vấn đề "N+1 Query Problem" trong EF Core xảy ra khi nào và cách khắc phục chuẩn là gì?
            - A. Xảy ra khi CSDL quá tải; khắc phục bằng cách khởi động lại server
            - B. Xảy ra khi 1 truy vấn cha dẫn đến N truy vấn con lặp lại trong vòng lặp; khắc phục bằng Eager Loading dùng .Include()
            - C. Xảy ra khi bảng có hơn N cột; khắc phục bằng cách xóa bớt cột
            - D. Xảy ra khi connection pool cạn kiệt

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Eager Loading với .Include() gom dữ liệu liên quan vào câu lệnh JOIN thay vì query lẻ tẻ trong vòng lặp.

            ### Câu 3: [Q10-03] Migration trong Entity Framework Core dùng để làm gì?
            - A. Sao chép dữ liệu từ SQLite sang PostgreSQL
            - B. Quản lý lịch sử và áp dụng các thay đổi về lược đồ CSDL (schema) đồng bộ với mô hình code C#
            - C. Tối ưu hóa câu lệnh SQL tự động
            - D. Chuyển đổi mã C# sang JavaScript

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** EF Core Migrations theo dõi sự thay đổi của DbContext/Entities và sinh mã cập nhật cấu trúc database.

            ### Câu 4: [Q10-04] Cơ chế Kiểm soát đồng thời lạc quan (Optimistic Concurrency Control) trong EF Core thường được triển khai bằng kỹ thuật nào?
            - A. Sử dụng RowVersion/Timestamp hoặc cột ConcurrencyToken để phát hiện xung đột khi lưu dữ liệu
            - B. Khóa toàn bộ database khi có người đang sửa
            - C. Luôn ghi đè dữ liệu mà không kiểm tra
            - D. Từ chối tất cả request cập nhật từ người dùng thứ hai

            > [!TIP]
            > **Đáp án:** A
            > **Giải thích:** Concurrency Token được kiểm tra trong mệnh đề WHERE của UPDATE; nếu token đã thay đổi sẽ ném DbUpdateConcurrencyException.

            ### Câu 5: [Q10-05] Khái niệm Shadow Property trong EF Core là gì?
            - A. Thuộc tính chỉ tồn tại trong bộ nhớ RAM
            - B. Thuộc tính được định nghĩa trong mô hình EF Core và cột CSDL nhưng không có property tương ứng trong class C#
            - C. Thuộc tính đã bị xóa khỏi database
            - D. Thuộc tính được mã hóa bằng mật khẩu

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Shadow Property hữu ích cho các cột kiểm toán như UpdatedAt, IsDeleted mà không muốn làm bẩn domain entity.
            """,

            11 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 11: API Nhiều Người Dùng & Bảo Mật

            ### Câu 1: [Q11-01] Cấu trúc của một JSON Web Token (JWT) gồm 3 phần nào ngăn cách bởi dấu chấm?
            - A. Header.Payload.Signature
            - B. User.Role.Permission
            - C. Key.Algorithm.Data
            - D. Client.Server.Token

            > [!TIP]
            > **Đáp án:** A
            > **Giải thích:** JWT gồm Header (thuật toán), Payload (các claims dữ liệu) và Signature (chữ ký số xác thực tính toàn vẹn).

            ### Câu 2: [Q11-02] Tại sao mật khẩu người dùng KHÔNG bao giờ được mã hóa bằng thuật toán đối xứng (như AES) mà phải băm (hash) bằng PBKDF2/BCrypt kèm Salt?
            - A. Vì hash tốn ít dung lượng lưu trữ hơn
            - B. Vì hash là hàm một chiều không thể đảo ngược giải mã, và Salt giúp chống tấn công bảng băm cầu vồng (Rainbow Table)
            - C. Vì thuật toán đối xứng không hỗ trợ tiếng Việt
            - D. Vì giao thức HTTPS yêu cầu bắt buộc

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Password hash một chiều đảm bảo kể cả khi CSDL bị lộ, kẻ tấn công cũng không thể giải mã ngược ra mật khẩu gốc.

            ### Câu 3: [Q11-03] Lỗ hổng IDOR (Insecure Direct Object Reference) trong Web API xảy ra khi nào?
            - A. Khi người dùng quên mật khẩu đăng nhập
            - B. Khi endpoint cho phép truy cập hoặc chỉnh sửa tài nguyên dựa trên ID người dùng truyền lên mà không kiểm tra quyền sở hữu của user đăng nhập
            - C. Khi server bị tấn công từ chối dịch vụ DDoS
            - D. Khi chứng chỉ SSL hết hạn

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** IDOR xảy ra khi server tin tưởng ID từ client gửi lên mà thiếu bước xác thực đối tượng đó có thuộc quyền sở hữu của caller không.

            ### Câu 4: [Q11-04] Chính sách CORS (Cross-Origin Resource Sharing) được thực thi ở đâu?
            - A. Tại database server
            - B. Do trình duyệt web của người dùng thực thi để bảo vệ khỏi việc trang web độc hại gọi API sang domain khác
            - C. Tại router mạng của nhà mạng
            - D. Tại hệ điều hành máy khách

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** CORS là cơ chế bảo mật trên browser ngăn chặn các script từ nguồn gốc khác truy cập tài nguyên API mà server không cho phép.

            ### Câu 5: [Q11-05] Cơ chế "Refresh Token Rotation" có tác dụng gì trong việc tăng cường bảo mật phiên đăng nhập?
            - A. Xóa tài khoản người dùng sau 30 ngày
            - B. Cấp một Refresh Token mới và thu hồi ngay Refresh Token cũ mỗi lần đổi Access Token; nếu phát hiện token cũ bị dùng lại thì thu hồi toàn bộ session
            - C. Bắt buộc người dùng nhập mật khẩu sau mỗi 5 phút
            - D. Mã hóa lại database định kỳ

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Refresh Token Rotation giúp phát hiện kịp thời trường hợp token bị đánh cắp và vô hiệu hóa ngay phiên làm việc nguy hại.
            """,

            12 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 12: React & TypeScript Chuyên Sâu

            ### Câu 1: [Q12-01] Lợi ích lớn nhất khi sử dụng TypeScript kết hợp với React là gì?
            - A. Giúp kích thước bundle JavaScript nhỏ hơn đáng kể
            - B. Kiểm tra kiểu tĩnh (Static Type Checking) tại thời điểm viết code, phát hiện sớm lỗi truyền sai props và hỗ trợ gợi ý code thông minh
            - C. Thay thế hoàn toàn HTML và CSS
            - D. Tự động kết nối với API backend không cần code fetch

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** TypeScript cung cấp khả năng phát hiện lỗi kiểu dữ liệu ngay trong IDE trước khi mã được chạy trên trình duyệt.

            ### Câu 2: [Q12-02] Trong TypeScript, định nghĩa interface UserProps { id: string; name: string; age?: number; }, dấu ? ở age?: number có ý nghĩa gì?
            - A. Thuộc tính age là bắt buộc và phải lớn hơn 0
            - B. Thuộc tính age là tùy chọn (optional), có thể không truyền hoặc mang giá trị undefined
            - C. Thuộc tính age chỉ nhận giá trị boolean
            - D. Là lỗi cú pháp trong TypeScript

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Dấu chấm hỏi đánh dấu thuộc tính tùy chọn, cho phép linh hoạt khi gọi component.

            ### Câu 3: [Q12-03] Thư viện quản lý state Zustand có ưu điểm nổi bật nào so với Redux truyền thống?
            - A. Chỉ chạy được trên server
            - B. Cú pháp gọn nhẹ, ít boilerplate code, không cần bọc Provider xung quanh ứng dụng và dễ tiếp cận hơn
            - C. Tự động lưu toàn bộ dữ liệu vào cookie
            - D. Bắt buộc dùng class component

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Zustand dựa trên mô hình hook đơn giản, loại bỏ hoàn toàn reducer, action types phức tạp của Redux.

            ### Câu 4: [Q12-04] Hook useMemo trong React được sử dụng với mục đích chính là gì?
            - A. Ghi nhớ (cache) kết quả tính toán tốn kém tài nguyên giữa các lần render để tránh tính lại khi dependencies không đổi
            - B. Ghi nhớ hàm callback để tránh tạo mới function
            - C. Quản lý trạng thái form nhập liệu
            - D. Gửi HTTP request ngầm

            > [!TIP]
            > **Đáp án:** A
            > **Giải thích:** useMemo trả về giá trị đã ghi nhớ; nó chỉ tính toán lại khi một trong các dependency bị biến đổi.

            ### Câu 5: [Q12-05] Custom Hook trong React bắt buộc phải tuân theo quy ước đặt tên nào?
            - A. Phải bắt đầu bằng chữ in hoa (ví dụ: MyHook)
            - B. Phải bắt đầu bằng tiền tố use viết thường (ví dụ: useCourseData)
            - C. Phải có hậu tố Component
            - D. Tên bất kỳ không bị giới hạn

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Quy tắc tiền tố use cho phép linter của React kiểm tra xem hook có tuân thủ các quy tắc Hook Rules hay không.
            """,

            13 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 13: Kiểm Thử Có Hệ Thống (Unit & Integration Tests)

            ### Câu 1: [Q13-01] Theo mô hình Kim tự tháp kiểm thử (Testing Pyramid), tỉ lệ số lượng các loại test nên phân bổ như thế nào là tối ưu?
            - A. Nhiều E2E Test nhất, ít Unit Test nhất
            - B. Nhiều Unit Test nhất (nền tảng), vừa phải Integration Test, và ít E2E Test nhất (đỉnh tháp)
            - C. Chỉ cần viết Integration Test và bỏ qua các loại khác
            - D. Cả ba loại có số lượng bằng nhau

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Unit test chạy nhanh và chi phí thấp nên chiếm số lượng lớn nhất; E2E test tốn kém và chậm nên chỉ dùng cho luồng trọng yếu.

            ### Câu 2: [Q13-02] Trong kiểm thử đơn vị (Unit Test), cấu trúc chuẩn "AAA" viết tắt của 3 giai đoạn nào?
            - A. Assert - Act - Arrange
            - B. Arrange (Chuẩn bị dữ liệu) -> Act (Thực hiện hành động) -> Assert (Kiểm chứng kết quả)
            - C. Analyze -> Apply -> Assess
            - D. Allocate -> Authorize -> Audit

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Cấu trúc 3 bước Arrange-Act-Assert giúp bài test rõ ràng, dễ đọc và dễ bảo trì.

            ### Câu 3: [Q13-03] Lớp WebApplicationFactory trong ASP.NET Core testing framework dùng để làm gì?
            - A. Tự động sinh giao diện HTML cho test
            - B. Khởi tạo máy chủ kiểm thử trong bộ nhớ (In-Memory TestServer) cho phép gửi HTTP request thực tế qua HttpClient để kiểm thử tích hợp
            - C. Triển khai ứng dụng lên server production
            - D. Giả lập cơ sở dữ liệu NoSQL

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** WebApplicationFactory khởi động toàn bộ pipeline ứng dụng trong bộ nhớ cho integration testing chân thực.

            ### Câu 4: [Q13-04] Khái niệm Mocking trong Unit Test có mục đích gì?
            - A. Giả lập hành vi của các phụ thuộc bên ngoài (như EmailService, CSDL, API bên thứ 3) để cô lập đối tượng cần kiểm thử
            - B. Chạy test với tốc độ chậm hơn để quan sát
            - C. Đổi tên phương thức trong mã nguồn
            - D. Bỏ qua các bài test bị lỗi

            > [!TIP]
            > **Đáp án:** A
            > **Giải thích:** Mock đối tượng giúp unit test tập trung duy nhất vào logic của class đang test mà không phụ thuộc hạ tầng mạng hay CSDL.

            ### Câu 5: [Q13-05] Thuộc tính [Theory] kết hợp với [InlineData(...)] trong xUnit dùng để làm gì?
            - A. Bỏ qua không chạy bài test này
            - B. Định nghĩa bài kiểm thử tham số hóa (Parameterized Test) chạy cùng một logic với nhiều bộ dữ liệu đầu vào khác nhau
            - C. Đánh dấu bài test chỉ chạy trên môi trường Linux
            - D. Đo thời gian thực thi của bài test

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** [Theory] cho phép kiểm thử tính đúng đắn của logic trên nhiều kịch bản biên với các tham số từ [InlineData].
            """,

            14 => """
            # 📋 Đề Kiểm Tra Đánh Giá Chặng 14: Triển Khai & Vận Hành (Docker, CI/CD & Cloud)

            ### Câu 1: [Q14-01] Lợi ích cốt lõi của việc đóng gói ứng dụng bằng Docker Container là gì?
            - A. Tăng gấp đôi tốc độ xử lý của CPU máy chủ
            - B. Đảm bảo tính nhất quán của môi trường ("chạy được trên máy dev thì chạy được trên production") bằng cách đóng gói app cùng dependencies
            - C. Giúp ứng dụng không bao giờ gặp lỗi phần mềm
            - D. Miễn phí hoàn toàn chi phí tài nguyên điện toán đám mây

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Container cô lập môi trường thực thi, loại bỏ hoàn toàn sự sai khác về phiên bản OS, runtime giữa dev và prod.

            ### Câu 2: [Q14-02] Tệp Dockerfile chứa nội dung gì?
            - A. Danh sách tài khoản người dùng hệ điều hành
            - B. Tập hợp các chỉ thị từng bước để build một Docker Image cho ứng dụng (FROM, COPY, RUN, ENTRYPOINT)
            - C. Nhật ký ghi nhận các lỗi runtime của ứng dụng
            - D. Chứng chỉ số bảo mật SSL

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Dockerfile là kịch bản công thức mô tả các layer để xây dựng image sẵn sàng khởi chạy.

            ### Câu 3: [Q14-03] Trong quy trình CI/CD, giai đoạn CI (Continuous Integration) chịu trách nhiệm tự động hóa việc gì?
            - A. Tự động gửi email thông báo cho khách hàng
            - B. Tự động kiểm tra code mới: build ứng dụng, chạy toàn bộ bộ test tự động và quét chất lượng code mỗi khi có commit hoặc pull request
            - C. Mua sắm thêm phần cứng máy chủ
            - D. Khởi động lại hệ điều hành máy chủ

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** CI đảm bảo mã nguồn mới tích hợp liên tục vào nhánh chính mà không gây đổ vỡ hệ thống nhờ automated build & test.

            ### Câu 4: [Q14-04] Endpoint Health Check (ví dụ /healthz) trong ASP.NET Core cung cấp chức năng gì cho hệ thống điều phối?
            - A. Cung cấp báo cáo tài chính của hệ thống
            - B. Trả về trạng thái hoạt động của ứng dụng và các dịch vụ phụ thuộc (Database, Cache) để bộ điều phối biết pod có sẵn sàng nhận traffic hay cần restart
            - C. Hiển thị thông tin phần cứng server
            - D. Đổi mật khẩu tài khoản quản trị viên

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Liveness và Readiness probes sử dụng Health Check endpoint để quyết định định tuyến traffic hoặc khởi động lại container bị treo.

            ### Câu 5: [Q14-05] Biến môi trường (Environment Variables) trong triển khai ứng dụng hiện đại dùng để làm gì?
            - A. Thay thế cho toàn bộ mã nguồn ứng dụng
            - B. Tách biệt cấu hình nhạy cảm và thay đổi theo môi trường (như Connection String, JWT Secret, API Keys) ra khỏi mã nguồn
            - C. Định dạng màu sắc giao diện console
            - D. Đặt tên hostname cho máy tính

            > [!TIP]
            > **Đáp án:** B
            > **Giải thích:** Cấu hình qua Environment Variables tuân thủ nguyên tắc Twelve-Factor App, giúp triển khai an toàn trên đa môi trường.
            """,

            _ => $"# 📋 Đề Kiểm Tra Đánh Giá Chặng {stage}: {stageTitle}\n\n### Câu 1: [Q{stage:D2}-01] Nội dung trọng tâm của {stageTitle}?\n- A. Đúng\n- B. Sai\n- C. Chưa đủ\n- D. Khác\n\n> **Đáp án:** A\n"
        };
    }

    private static List<PracticeActivity> BuildDomainPracticeActivitiesForLesson(
        string sourceFolder,
        Guid courseId,
        Lesson lesson,
        DateTimeOffset now)
    {
        var folder = (sourceFolder ?? string.Empty).Trim();
        var titleClean = Truncate(string.IsNullOrWhiteSpace(lesson.Title) ? "Bài học thực hành" : lesson.Title.Trim(), 180);
        var shortTag = $"doc-{Math.Abs(lesson.Id.GetHashCode()):x}";
        var rawPlain = (lesson.PlainText ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
        var summaryHint = !string.IsNullOrWhiteSpace(rawPlain)
            ? Truncate(rawPlain, 220)
            : $"Nắm vững khái niệm cốt lõi và ứng dụng thực tế của {titleClean}.";

        var list = new List<PracticeActivity>();

        if (folder.Equals("DotNet-React-Roadmap", StringComparison.OrdinalIgnoreCase)
            || folder.Equals("FullStack-CSharp-React-Refresh", StringComparison.OrdinalIgnoreCase))
        {
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "fill_code", 101),
                CourseId = courseId,
                LessonId = lesson.Id,
                OrderIndex = 101,
                Type = "fill_code",
                Title = Truncate($"Điền Code: Tổng hợp Pattern Async & Xử lý Dữ liệu ({titleClean})", 250),
                Description = $"Hoàn thiện pipeline xử lý danh sách và kiểm tra điều kiện an toàn cho chủ đề: {titleClean}.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = $"{shortTag}-fill-101",
                            title = $"Hoàn thiện Pipeline Xử lý — {titleClean}",
                            category = folder.Contains("React", StringComparison.OrdinalIgnoreCase) ? "react" : "dotnet",
                            language = "javascript",
                            description = $"Điền phương thức chuẩn để lọc phần tử hợp lệ và trích xuất nhãn cho {titleClean}.",
                            practicalScenario = summaryHint,
                            codeTemplate = new[]
                            {
                                "function summarizeActive(items) {",
                                "  return items",
                                "    .{{BLANK_0}}(x => x && x.active)",
                                "    .{{BLANK_1}}(x => x.title);",
                                "}"
                            },
                            blanks = new object[]
                            {
                                new { id = "b0", blankIndex = 0, expected = "filter", acceptableAnswers = new[] { "filter" }, hint = "Lọc các phần tử đạt điều kiện active", chips = new[] { "filter", "map", "reduce", "sort" }, explanation = "filter() chỉ giữ lại các phần tử thỏa mãn điều kiện boolean." },
                                new { id = "b1", blankIndex = 1, expected = "map", acceptableAnswers = new[] { "map" }, hint = "Ánh xạ từng đối tượng sang trường title", chips = new[] { "map", "filter", "find", "forEach" }, explanation = "map() biến đổi mỗi đối tượng thành chuỗi title tương ứng." }
                            },
                            lineExplanations = new object[]
                            {
                                new { lineNumber = 3, codeSnippet = ".filter(x => x && x.active)", purpose = "Loại bỏ null/undefined và phần tử chưa kích hoạt.", riskIfWrong = "Không lọc null sẽ gây TypeError khi đọc thuộc tính." },
                                new { lineNumber = 4, codeSnippet = ".map(x => x.title)", purpose = "Trích xuất danh sách tiêu đề gọn nhẹ cho API/UI.", riskIfWrong = "Dùng forEach sẽ trả về undefined thay vì mảng mới." }
                            }
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "predict_output", 102),
                CourseId = courseId,
                LessonId = lesson.Id,
                OrderIndex = 102,
                Type = "predict_output",
                Title = Truncate($"Đoán Output: Reference vs Immutability ({titleClean})", 250),
                Description = "Dự đoán chính xác kết quả in ra Console khi sao chép trạng thái bất biến trong .NET/React.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = $"{shortTag}-pred-102",
                            difficulty = "Trung bình",
                            title = $"Kiểm tra Immutability — {titleClean}",
                            code = "const state = { step: 1, tags: ['core'] };\nconst next = { ...state, step: state.step + 1, tags: [...state.tags, 'lab'] };\nconsole.log(state.step, next.step);\nconsole.log(state.tags.length, next.tags.length);",
                            expectedOutput = "1 2\n1 2",
                            acceptedOutputs = new[] { "1 2\n1 2", "1, 2\n1, 2" },
                            hint = "Spread operator `{ ...state }` và `[...state.tags]` tạo tham chiếu mới nên state gốc giữ nguyên.",
                            explanation = "Việc không đột biến trực tiếp object/array gốc là nguyên tắc nền tảng trong React state và DTO mapping."
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "bug_fix", 103),
                CourseId = courseId,
                LessonId = lesson.Id,
                OrderIndex = 103,
                Type = "bug_fix",
                Title = Truncate($"Sửa Bug: Bảo vệ Null & Bất biến Dữ liệu ({titleClean})", 250),
                Description = "Khắc phục lỗi truy cập thuộc tính khi đầu vào có thể rỗng hoặc bị đột biến trực tiếp.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = $"{shortTag}-bug-103",
                            title = $"Sửa hàm chuẩn hóa danh sách không làm đột biến đầu vào — {titleClean}",
                            language = "javascript",
                            categoryTag = "Defensive Coding",
                            context = $"Trong {titleClean}, hàm sortByPriority(tasks) vô tình gọi tasks.sort() trực tiếp làm thay đổi mảng ban đầu.",
                            buggyCode = "function sortByPriority(tasks) {\n  return tasks.sort((a, b) => a.priority - b.priority);\n}",
                            bugLineNumber = 2,
                            cleanFixedCode = "function sortByPriority(tasks) {\n  return [...(tasks || [])].sort((a, b) => a.priority - b.priority);\n}",
                            requiredTokens = new[] { "[...", "sort" },
                            functionName = "sortByPriority",
                            explanation = "Sao chép mảng bằng spread `[...(tasks || [])]` trước khi gọi `.sort()` giúp tránh NullReference và side-effect."
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            return list;
        }

        if (folder.Equals("IELTS", StringComparison.OrdinalIgnoreCase))
        {
            var isSpeaking = titleClean.Contains("Speak", StringComparison.OrdinalIgnoreCase)
                || titleClean.Contains("Pronun", StringComparison.OrdinalIgnoreCase);
            var isWriting = titleClean.Contains("Writ", StringComparison.OrdinalIgnoreCase)
                || titleClean.Contains("Grammar", StringComparison.OrdinalIgnoreCase);

            var sampleSentence = isSpeaking
                ? "Developing consistent daily speaking habits gradually improves fluency, lexical resource, and pronunciation confidence."
                : isWriting
                    ? "Implementing sustainable public transportation significantly reduces urban congestion while lowering carbon emissions."
                    : "Careful note-taking during academic lectures helps learners identify main arguments and subtle distracting details.";
            var sampleMeaning = isSpeaking
                ? "Việc duy trì thói quen luyện nói hằng ngày giúp cải thiện dần độ trôi chảy, vốn từ vựng và sự tự tin phát âm."
                : isWriting
                    ? "Việc triển khai giao thông công cộng bền vững làm giảm đáng kể ùn tắc đô thị đồng thời cắt giảm khí thải carbon."
                    : "Ghi chú cẩn thận trong các bài giảng học thuật giúp người học nhận diện luận điểm chính và các chi tiết gây nhiễu tinh vi.";

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "shadowing", 101),
                CourseId = courseId,
                LessonId = lesson.Id,
                OrderIndex = 101,
                Type = "shadowing",
                Title = Truncate($"IELTS Shadowing: Ngữ điệu học thuật ({titleClean})", 250),
                Description = $"Nghe và nhại theo nhịp điệu, nối âm câu học thuật trọng tâm của {titleClean}.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    lang = "en-US",
                    difficulty = "medium",
                    items = new object[]
                    {
                        new
                        {
                            id = $"{shortTag}-shad-101",
                            topic = titleClean,
                            sentence = sampleSentence,
                            meaningVi = sampleMeaning,
                            keyPhrases = new[] { "consistent", "gradually", "significantly", "academic" },
                            speakingOutline = new[]
                            {
                                "Ngắt nhịp tự nhiên tại các cụm danh từ chính",
                                "Nhấn trọng âm vào động từ và trạng từ học thuật",
                                "Đọc rõ âm cuối /s/, /t/, /d/ không nuốt âm"
                            }
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "dictation", 102),
                CourseId = courseId,
                LessonId = lesson.Id,
                OrderIndex = 102,
                Type = "dictation",
                Title = Truncate($"IELTS Dictation: Nghe chép chính tả phản xạ ({titleClean})", 250),
                Description = "Luyện nhận diện từ khóa, chính tả số ít/số nhiều và liên từ trong ngữ cảnh IELTS.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    lang = "en-US",
                    difficulty = "medium",
                    items = new object[]
                    {
                        new
                        {
                            id = $"{shortTag}-dict-102",
                            topic = titleClean,
                            sentence = "Although technological innovation accelerates productivity, learners still need structured practice to retain complex vocabulary.",
                            meaningVi = "Mặc dù đổi mới công nghệ tăng tốc năng suất, người học vẫn cần luyện tập có cấu trúc để ghi nhớ từ vựng phức tạp.",
                            keyPhrases = new[] { "technological innovation", "productivity", "structured practice" }
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "cloze", 103),
                CourseId = courseId,
                LessonId = lesson.Id,
                OrderIndex = 103,
                Type = "cloze",
                Title = Truncate($"IELTS Cloze: Collocation & Linking Words ({titleClean})", 250),
                Description = "Điền collocation học thuật chuẩn xác vào ngữ cảnh IELTS Writing/Speaking.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    lang = "en-US",
                    difficulty = "medium",
                    items = new object[]
                    {
                        new
                        {
                            id = $"{shortTag}-cloze-103",
                            title = $"Collocation trọng tâm — {titleClean}",
                            promptVi = "Điền từ học thuật phù hợp vào hai ô trống để hoàn thiện lập luận Band 7.0+:",
                            template = "Regular review plays a {{0}} role in helping students {{1}} long-term fluency.",
                            blanks = new object[]
                            {
                                new { id = "b-0", answer = "crucial", acceptedAnswers = new[] { "crucial", "vital", "pivotal", "key" }, hintVi = "tính từ mang nghĩa 'then chốt / thiết yếu'", explanationVi = "'play a crucial/vital role in' là collocation chuẩn học thuật IELTS." },
                                new { id = "b-1", answer = "achieve", acceptedAnswers = new[] { "achieve", "maintain", "build", "develop" }, hintVi = "động từ mang nghĩa 'đạt được / xây dựng'", explanationVi = "'achieve long-term fluency' tự nhiên và chính xác ngữ pháp." }
                            },
                            fullSentence = "Regular review plays a crucial role in helping students achieve long-term fluency.",
                            meaningVi = "Việc ôn tập đều đặn đóng vai trò then chốt giúp học viên đạt được sự trôi chảy lâu dài."
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            return list;
        }

        if (folder.Equals("Investing", StringComparison.OrdinalIgnoreCase)
            || folder.Equals("PersonalFinance", StringComparison.OrdinalIgnoreCase))
        {
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "hand_calc", 101),
                CourseId = courseId,
                LessonId = lesson.Id,
                OrderIndex = 101,
                Type = "hand_calc",
                Title = Truncate($"Lab Tính Tay Tài Chính: Lãi kép, DCA & Biên an toàn ({titleClean})", 250),
                Description = $"Thực hành tính toán định lượng các chỉ số tài chính cốt lõi gắn với bài: {titleClean}.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    problemTypes = new[] { "compound", "dca", "pe", "margin_of_safety", "drawdown" },
                    tolerancePercent = 1.5
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "investment_case", 102),
                CourseId = courseId,
                LessonId = lesson.Id,
                OrderIndex = 102,
                Type = "investment_case",
                Title = Truncate($"Phân Tích Tình Huống Tài Chính: Quản Trị Rủi Ro ({titleClean})", 250),
                Description = "Lựa chọn phương án kỷ luật và giải thích nguyên lý phân bổ vốn dài hạn.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = $"{shortTag}-fin-case-102",
                            title = $"Ra quyết định kỷ luật — {titleClean}",
                            scenario = $"Trong bối cảnh thị trường biến động mạnh và chủ đề '{titleClean}', một nhà đầu tư cá nhân đang phân vân giữa việc giữ nguyên tỷ lệ quỹ dự phòng & tái cân bằng định kỳ hay dùng đòn bẩy tất tay bắt đáy.",
                            options = new[]
                            {
                                "Duy trì quỹ khẩn cấp an toàn, bám sát phân bổ mục tiêu và giải ngân từng phần có biên an toàn",
                                "Vay margin tối đa để gỡ lỗ thật nhanh trong vài tuần",
                                "Bán tháo toàn bộ danh mục dài hạn khi thị trường giảm 10% vì sợ hãi",
                                "Đầu tư theo tin đồn mạng xã hội mà không đọc báo cáo hay dòng tiền"
                            },
                            correctIndex = 0,
                            reasonPromptVi = "Giải thích vì sao giữ kỷ luật phân bổ vốn và quỹ dự phòng lại bảo vệ danh mục tốt hơn hành động cảm tính:",
                            expectedKeywords = new[] { "quỹ khẩn cấp", "rủi ro", "kỷ luật", "phân bổ", "dài hạn" },
                            sampleExplanation = "Giữ vững quỹ dự phòng giúp nhà đầu tư không bị ép bán tháo tài sản tốt lúc giá thấp, đồng thời kỷ luật phân bổ và biên an toàn bảo vệ sức bền danh mục qua các chu kỳ biến động dài hạn.",
                            minWords = 12
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "decision_journal", 103),
                CourseId = courseId,
                LessonId = lesson.Id,
                OrderIndex = 103,
                Type = "decision_journal",
                Title = Truncate($"Nhật Ký Quyết Định: Kế Hoạch Hành Động ({titleClean})", 250),
                Description = "Viết 4 trụ cột kiểm chứng (Luận điểm, Định giá/Ngân sách, Kịch bản sai, Kỷ luật) cho bài học này.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = $"{shortTag}-dj-103",
                            title = $"Nhật ký kiểm chứng cá nhân — {titleClean}",
                            scenario = summaryHint,
                            expectedKeywords = new[] { "dòng tiền", "rủi ro", "kỷ luật", "mục tiêu", "dự phòng" },
                            sampleAnswer = $"Luận điểm: Áp dụng nguyên lý của {titleClean} để chuẩn hóa dòng tiền và tích lũy dài hạn. Định giá/Ngân sách: Chỉ dùng vốn nhàn rỗi sau khi đủ quỹ khẩn cấp. Kịch bản sai: Nếu thu nhập giảm 20%, tự động hạ mức giải ngân nhưng không phá quỹ dự phòng. Kỷ luật: Rà soát lại danh mục theo chu kỳ hàng quý."
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            return list;
        }

        // Default domain: Psychology & LifeSkills (PsychologyLifeSkillsLab)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "feynman", 101),
            CourseId = courseId,
            LessonId = lesson.Id,
            OrderIndex = 101,
            Type = "feynman",
            Title = Truncate($"Feynman Tự Diễn Giải: Nguyên Lý Cốt Lõi ({titleClean})", 250),
            Description = "Diễn giải lại kiến thức trọng tâm của bài học bằng ngôn ngữ đời thường cho người mới.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = $"{shortTag}-feyn-101",
                        conceptTitle = titleClean,
                        promptVi = $"Hãy giải thích bằng ngôn ngữ đơn giản: bài học '{titleClean}' giúp bạn hiểu cơ chế gì và áp dụng ra sao vào đời sống hằng ngày?",
                        requiredKeywords = new[] { "nhận thức", "hành vi|thói quen", "phản hồi|thực hành", "mục tiêu|cải thiện" },
                        minWords = 14,
                        sampleAnswer = $"Bài học {titleClean} cho thấy nhận thức rõ ràng giúp điều chỉnh hành vi và thói quen hằng ngày. Khi thực hành có phản hồi đều đặn thay vì phản ứng theo cảm xúc bộc phát, chúng ta xây dựng được kỹ năng bền vững để đạt mục tiêu công việc và giao tiếp.",
                        everydayExampleHintVi = summaryHint
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "cbt_exercise", 102),
            CourseId = courseId,
            LessonId = lesson.Id,
            OrderIndex = 102,
            Type = "cbt_exercise",
            Title = Truncate($"Tái Cấu Trúc Nhận Thức (CBT): Vượt Qua Suy Nghĩ Cực Đoan ({titleClean})", 250),
            Description = "Nhận diện bẫy suy nghĩ tự động và viết lại góc nhìn cân bằng, dựa trên bằng chứng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = $"{shortTag}-cbt-102",
                        title = $"Gỡ bẫy suy nghĩ tự động — {titleClean}",
                        scenario = "Khi gặp một lần vấp ngã nhỏ trong giao tiếp hoặc quản lý thời gian, bạn lập tức nghĩ: 'Mình lúc nào cũng thất bại và chẳng bao giờ làm đúng được việc gì'.",
                        sampleAutomaticThought = "Mình lúc nào cũng thất bại và không bao giờ thay đổi được.",
                        canonicalDistortionVi = "Khái quát hóa quá mức / Tư duy trắng đen (Overgeneralization / All-or-Nothing)",
                        acceptedDistortions = new[] { "Khái quát hóa quá mức", "Overgeneralization", "Trắng đen", "All-or-Nothing", "Cực đoan" },
                        distortionChoicesHint = new[] { "Khái quát hóa quá mức (Overgeneralization)", "Tư duy trắng đen (All-or-Nothing)", "Trầm trọng hóa (Catastrophizing)" },
                        balancedKeywords = new[] { "một lần", "bài học", "cải thiện", "cụ thể", "tiến bộ" },
                        minBalancedWords = 10,
                        sampleBalancedThought = "Đây chỉ là một tình huống cụ thể chưa như ý chứ không đại diện cho toàn bộ năng lực của mình. Mình có thể rút ra bài học rõ ràng để cải thiện ở lần thực hành tiếp theo.",
                        explanationVi = "Dùng từ tuyệt đối như 'lúc nào cũng / chẳng bao giờ' cho một sự cố đơn lẻ là bẫy Khái quát hóa quá mức."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "apply_journal", 103),
            CourseId = courseId,
            LessonId = lesson.Id,
            OrderIndex = 103,
            Type = "apply_journal",
            Title = Truncate($"Nhật Ký Áp Dụng Thực Tế 48 Giờ ({titleClean})", 250),
            Description = "Thiết kế một thử nghiệm hành vi nhỏ thực hiện trong 48 giờ tới để kiểm chứng kiến thức bài học.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = $"{shortTag}-apply-103",
                        title = $"Kế hoạch hành động 48h — {titleClean}",
                        promptVi = $"Nêu 1 hành động cụ thể, đo lường được từ '{titleClean}' mà bạn sẽ thực hành trong 48 giờ tới và cách ghi nhận kết quả:",
                        expectedKeywords = new[] { "hành động", "thực hành", "48 giờ", "ghi chép", "phản hồi" },
                        samplePlanVi = $"Trong 48 giờ tới, mình sẽ áp dụng kỹ thuật trọng tâm của '{titleClean}' vào một tình huống giao tiếp hoặc làm việc thực tế, sau đó ghi chép lại kết quả quan sát khách quan và điều chỉnh phản hồi cho lần sau."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        return list;
    }

    private static List<FlashcardSpec> BuildDotNetRoadmapFlashcards(HashSet<string> availableStagePaths)
    {
        var specs = new (int Stage, string Concept, string Front, string Back)[]
        {
            (1, "C# Kiểu decimal vs double", "Vì sao nên dùng `decimal` thay cho `float`/`double` khi tính toán tiền tệ trong C#?", "Kiểu `decimal` (128-bit hệ cơ số 10) tránh sai số biểu diễn dấu phẩy động nhị phân, bảo toàn độ chính xác tài chính."),
            (1, "C# int.TryParse", "Sự khác biệt an toàn giữa `int.Parse` và `int.TryParse` là gì?", "`int.TryParse(s, out var val)` trả về `bool` và không ném ngoại lệ `FormatException` khi chuỗi đầu vào sai định dạng."),
            (2, "Debugging Breakpoint & Call Stack", "Trong Visual Studio/VS Code, `F10` (Step Over) khác `F11` (Step Into) như thế nào?", "`F10` chạy qua dòng hiện tại (coi lời gọi hàm là 1 bước), còn `F11` bước sâu vào bên trong phần thân hàm đang gọi."),
            (2, "Tư duy chia nhỏ bài toán", "Thứ tự 4 bước chuẩn khi xử lý một lỗi runtime bất ngờ trên Production là gì?", "Tái hiện lỗi ổn định (Reproduce) -> Đọc Stack Trace & Logs -> Khoanh vùng nguyên nhân gốc -> Viết test kiểm chứng bản vá."),
            (3, "OOP Encapsulation & Record", "Kiểu `record` trong C# hiện đại nổi bật ở đặc tính nào khi làm DTO?", "Hỗ trợ Value-based Equality (so sánh theo giá trị), cú pháp ngắn gọn và tính bất biến tự nhiên với `init`."),
            (3, "Collection List<T> vs Dictionary", "Khi nào nên dùng `Dictionary<TKey, TValue>` thay cho `List<T>.FirstOrDefault(...)`?", "Khi cần tra cứu lặp đi lặp lại theo khóa với độ phức tạp trung bình `O(1)` thay vì duyệt tuyến tính `O(N)`."),
            (4, "SQL Primary & Foreign Key", "Khóa ngoại (Foreign Key) đảm bảo điều gì trong CSDL quan hệ?", "Đảm bảo Toàn vẹn tham chiếu (Referential Integrity), ngăn bản ghi con trỏ tới một bản ghi cha không tồn tại."),
            (4, "SQL Parameterized Query", "Vì sao không được nối chuỗi trực tiếp vào câu lệnh SQL?", "Nối chuỗi dễ gây lỗ hổng SQL Injection; luôn dùng Parameterized Query hoặc ORM (EF Core LINQ) để tách mã lệnh và dữ liệu."),
            (5, "HTTP Status Code 201 vs 204", "Ý nghĩa khác biệt giữa `201 Created` và `204 No Content` trong REST API?", "`201 Created` báo tạo mới tài nguyên thành công (thường kèm Location/body); `204` báo thành công nhưng không trả body (như DELETE/PUT)."),
            (5, "ASP.NET Core Dependency Injection", "So sánh vòng đời dịch vụ `Singleton`, `Scoped` và `Transient` trong DI Container?", "`Singleton`: 1 instance suốt app; `Scoped`: 1 instance mỗi HTTP request (chuẩn cho DbContext); `Transient`: tạo mới mỗi lần inject."),
            (6, "Frontend Semantic HTML & Accessibility", "Lợi ích cốt lõi của Semantic HTML (`<main>`, `<nav>`, `<article>`) so với lạm dụng `<div>`?", "Cải thiện khả năng đọc cho Screen Reader (A11y), tối ưu SEO và giúp cấu trúc DOM rõ ràng, dễ bảo trì."),
            (6, "CSS Flexbox vs Grid", "Khi nào ưu tiên Flexbox và khi nào ưu tiên CSS Grid?", "Flexbox tối ưu cho bố cục 1 chiều (hàng hoặc cột); CSS Grid mạnh cho lưới 2 chiều kiểm soát đồng thời hàng và cột."),
            (7, "React useState Immutability", "Vì sao không được `array.push(...)` trực tiếp vào state React trước khi gọi `setState`?", "React so sánh tham chiếu nông (`Object.is`); đột biến cùng tham chiếu khiến React không nhận ra thay đổi để re-render UI."),
            (7, "React useEffect Cleanup", "Hàm cleanup `return () => ...` trong `useEffect` chạy vào thời điểm nào?", "Chạy trước khi effect kích hoạt lại ở lần render kế tiếp và khi component unmount, giúp dọn timer/subscription tránh memory leak."),
            (8, "C# Async/Await & Task", "Vì sao nên tránh `Task.Result` hoặc `.Wait()` trong ứng dụng ASP.NET Core?", "Gây block thread xử lý trong ThreadPool (Thread Starvation) và dễ dẫn tới deadlock; hãy dùng `await` xuyên suốt (`async` all the way)."),
            (8, "LINQ Deferred Execution", "Thực thi trì hoãn (Deferred Execution) của `IEnumerable<T>` có nghĩa là gì?", "Câu truy vấn LINQ chưa chạy khi khai báo mà chỉ thực thi khi duyệt (`foreach`) hoặc gọi `.ToList()`, `.FirstOrDefaultAsync()`."),
            (9, "Repository vs Direct DbContext", "Nguyên tắc Dependency Inversion Principle (DIP) trong Clean Architecture đòi hỏi điều gì?", "Tầng Domain/Application phụ thuộc vào Abstraction (Interface), còn Infrastructure triển khai chi tiết (DB, Email, AI) theo interface đó."),
            (9, "Result Pattern vs Exceptions", "Vì sao không nên ném Exception cho luồng nghiệp vụ thông thường (như hết tồn kho, trùng email)?", "Exception tốn chi phí thu thập Stack Trace và làm khó theo dõi luồng; Result Pattern thể hiện lỗi nghiệp vụ tường minh ở kiểu trả về."),
            (10, "EF Core N+1 Query Problem", "Làm sao khắc phục lỗi N+1 Query trong Entity Framework Core?", "Dùng `Include`/`ThenInclude` hoặc `Select` projection sang DTO trực tiếp trong 1 truy vấn SQL thay vì lazy-load trong vòng lặp."),
            (10, "EF Core AsNoTracking", "Khi nào nên thêm `.AsNoTracking()` vào truy vấn EF Core?", "Trong các endpoint chỉ đọc (`GET`), giúp EF Core không tạo Snapshot theo dõi thay đổi, giảm bộ nhớ RAM và tăng tốc độ truy vấn."),
            (11, "JWT Stateless Authentication", "Vì sao không nên lưu thông tin nhạy cảm chưa mã hóa trong Payload của JWT?", "Phần Payload của JWT chỉ được mã hóa Base64Url (ai cũng giải mã đọc được); chữ ký HMAC/RSA chỉ bảo vệ tính toàn vẹn chống sửa đổi."),
            (11, "Authorization (RBAC / Ownership)", "Lỗi IDOR (Insecure Direct Object Reference) xảy ra khi nào và phòng chống ra sao?", "Xảy ra khi endpoint chỉ kiểm tra đăng nhập mà không kiểm tra quyền sở hữu (`entity.UserId == currentUserId`) trước khi trả/sửa bản ghi."),
            (12, "TypeScript Discriminated Union", "Mẫu Discriminated Union trong TypeScript (`status: 'loading' | 'success' | 'error'`) giúp ích gì?", "Giúp trình biên dịch thu hẹp kiểu tự động (type narrowing) trong `switch/if`, loại bỏ các trạng thái UI bất hợp lệ."),
            (12, "React Server vs Client State", "Phân biệt Server Cache State (TanStack Query/Fetch) và Client UI State (Zustand)?", "Server state quản lý dữ liệu bất đồng bộ từ API (cần cache/refetch); Client state quản lý trạng thái UI cục bộ như modal, drawer, theme."),
            (13, "Test Pyramid (Unit vs Integration)", "Sự khác biệt chính giữa Unit Test và Integration Test với `WebApplicationFactory`?", "Unit Test cô lập 1 hàm/class thuần logic rất nhanh; Integration Test chạy qua toàn bộ HTTP pipeline (Routing, Auth, DB thật/SQLite)."),
            (13, "Idempotency Trong Test & API", "Một phép thử hoặc API đồng bộ dữ liệu đạt tính Lũy đẳng (Idempotent) nghĩa là gì?", "Chạy 1 lần hay 10 lần liên tiếp với cùng đầu vào đều cho cùng trạng thái DB cuối cùng, không tạo bản ghi trùng lặp."),
            (14, "Docker Multi-stage Build", "Lợi ích của Multi-stage Dockerfile cho ứng dụng .NET và Next.js?", "Tách image build nặng (chứa SDK) khỏi runtime image nhẹ cuối cùng, giảm dung lượng container và thu hẹp bề mặt tấn công bảo mật."),
            (14, "Twelve-Factor App Config", "Vì sao Connection String và Secret Key phải nạp từ Environment Variables thay vì hardcode?", "Cho phép triển khai cùng một artifact qua Dev/Staging/Prod an toàn, đồng thời không bao giờ lộ bí mật vào Git history.")
        };

        var result = new List<FlashcardSpec>();
        foreach (var s in specs)
        {
            var rel = $"{s.Stage}.md";
            if (!availableStagePaths.Contains(rel)) continue;
            result.Add(new FlashcardSpec(
                s.Concept,
                s.Front,
                s.Back,
                rel,
                s.Stage,
                $"__cards/dotnet-stage-{s.Stage:D2}.csv"));
        }
        return result;
    }

    private static string BuildDotNetChapter1ExamMarkdown() => """
        # 📋 [Đề Chương 1] Nền tảng C#, Debug & SQL (Chặng 1–4)

        ### Câu 1: Kiểu dữ liệu nào trong C# phù hợp nhất để lưu tiền tệ với độ chính xác thập phân tuyệt đối?
        - A. float
        - B. double
        - C. decimal
        - D. byte

        > [!TIP]
        > **Đáp án:** C
        > **Giải thích:** Kiểu `decimal` 128-bit biểu diễn chính xác hệ cơ số 10, tránh sai số dấu phẩy động khi cộng trừ tiền tệ.

        ### Câu 2: Trong Debug, tổ hợp phím nào cho phép bước sâu (Step Into) vào bên trong thân hàm đang được gọi?
        - A. F10 (Step Over)
        - B. F11 (Step Into)
        - C. Shift+F11
        - D. Ctrl+S

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** `F11` (Step Into) đi vào từng dòng bên trong hàm con để kiểm tra trạng thái biến.

        ### Câu 3: Khi cần tra cứu nhanh theo khóa duy nhất (như UserId -> Profile) với độ phức tạp trung bình O(1), nên chọn cấu trúc nào trong C#?
        - A. Dictionary<TKey, TValue>
        - B. LinkedList<T>
        - C. Array tuyến tính
        - D. Queue<T>

        > [!TIP]
        > **Đáp án:** A
        > **Giải thích:** `Dictionary<TKey, TValue>` dùng bảng băm cho thời gian tra cứu trung bình `O(1)`.

        ### Câu 4: Mục đích chính của Parameterized Query (hoặc EF Core LINQ) trong SQL là gì?
        - A. Tự động nén dữ liệu ảnh
        - B. Tách rời tham số khỏi câu lệnh SQL để ngăn chặn lỗ hổng SQL Injection
        - C. Xóa toàn bộ Index để ghi nhanh hơn
        - D. Bỏ qua khóa ngoại (Foreign Key)

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Tham số hóa giúp Database Engine xử lý giá trị đầu vào như dữ liệu thuần túy chứ không thực thi thành mã SQL.

        ### Câu 5: Khóa ngoại (Foreign Key) giữa bảng `Orders` và `Users` có vai trò gì?
        - A. Tăng tốc độ tải CSS trên trình duyệt
        - B. Bảo đảm toàn vẹn tham chiếu, không cho phép tồn tại đơn hàng trỏ đến User không có thật
        - C. Mã hóa mật khẩu tự động
        - D. Thay thế hoàn toàn Primary Key

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Ràng buộc Foreign Key ngăn chặn dữ liệu mồ côi (orphaned records) giữa các bảng quan hệ.
        """;

    private static string BuildDotNetChapter2ExamMarkdown() => """
        # 📋 [Đề Chương 2] ASP.NET Core Web API & Frontend React (Chặng 5–8)

        ### Câu 1: Vòng đời DI (Dependency Injection) mặc định phù hợp nhất cho EF Core `DbContext` trong mỗi HTTP request là gì?
        - A. Singleton
        - B. Scoped
        - C. Transient
        - D. Static global

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** `Scoped` tạo đúng một `DbContext` cho mỗi HTTP request, bảo đảm tính nhất quán giao dịch và an toàn đa luồng.

        ### Câu 2: Vì sao khi cập nhật mảng trong React `useState`, ta phải tạo bản sao mới như `[...items, newItem]` thay vì gọi `items.push(newItem)`?
        - A. `Array.push` không hoạt động trong TypeScript
        - B. Đột biến trực tiếp giữ nguyên tham chiếu cũ nên React so sánh nông (`Object.is`) sẽ bỏ qua lần re-render
        - C. Để giảm kích thước RAM một nửa
        - D. Vì React không hỗ trợ mảng

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** React dựa vào tham chiếu đối tượng mới để nhận biết state đã thay đổi và lên lịch cập nhật DOM.

        ### Câu 3: Lệnh nào buộc một truy vấn LINQ trên `IQueryable<T>` thực sự phát sinh câu lệnh SQL xuống Database?
        - A. Gọi `.Where(...)`
        - B. Gọi `.OrderBy(...)`
        - C. Gọi `.ToListAsync()` hoặc `.FirstOrDefaultAsync()`
        - D. Khai báo biến `var query`

        > [!TIP]
        > **Đáp án:** C
        > **Giải thích:** Do cơ chế Deferred Execution, truy vấn LINQ chỉ thực thi khi gọi toán tử vật chất hóa kết quả như `ToListAsync()`.

        ### Câu 4: Khi nào hàm cleanup `return () => clearInterval(id)` bên trong `useEffect` được thực thi?
        - A. Ngay lập tức trước lần render đầu tiên
        - B. Trước khi effect chạy lại hoặc khi component unmount khỏi DOM
        - C. Chỉ khi người dùng F5 tải lại trình duyệt
        - D. Không bao giờ chạy nếu thiếu dependency array

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Cleanup function dọn sạch timer/listener cũ trước vòng đời kế tiếp hoặc khi component bị hủy.

        ### Câu 5: Mã trạng thái HTTP `201 Created` nên được trả về khi nào trong RESTful API?
        - A. Khi người dùng đăng nhập sai mật khẩu
        - B. Khi endpoint `POST` tạo mới thành công một tài nguyên trên server
        - C. Khi server gặp ngoại lệ chưa xử lý
        - D. Khi tài nguyên không tìm thấy

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** `201 Created` biểu thị tài nguyên mới đã được khởi tạo thành công.
        """;

    private static string BuildDotNetChapter3ExamMarkdown() => """
        # 📋 [Đề Chương 3] Kiến trúc, Bảo mật, Kiểm thử & Triển khai (Chặng 9–14)

        ### Câu 1: Làm thế nào để loại bỏ lỗi N+1 Query khi truy vấn danh sách Khóa học kèm Chương và Bài học bằng EF Core?
        - A. Truy vấn từng bài học trong vòng lặp `foreach`
        - B. Dùng `.Include(...).ThenInclude(...)` hoặc `.Select(...)` projection trực tiếp trong 1 lần truy vấn
        - C. Tắt hoàn toàn khóa chính trên bảng
        - D. Chuyển dữ liệu sang file text thuần

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Eager Loading (`Include`) hoặc `Select` Projection gộp truy vấn, tránh hàng chục round-trip lẻ tẻ tới DB.

        ### Câu 2: Tác dụng của `.AsNoTracking()` trong các API chỉ đọc (Read-only GET) là gì?
        - A. Tự động xóa bản ghi sau khi đọc
        - B. Không tạo Snapshot theo dõi thay đổi trong ChangeTracker, tiết kiệm RAM và tăng thông lượng
        - C. Bỏ qua kiểm tra quyền người dùng
        - D. Vô hiệu hóa SQL Index

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Truy vấn không chỉnh sửa dữ liệu không cần ChangeTracker giữ bản sao đối tượng.

        ### Câu 3: Lỗ hổng IDOR (Insecure Direct Object Reference) xảy ra khi endpoint API thiếu kiểm tra gì?
        - A. Thiếu nén Gzip response
        - B. Chỉ xác thực JWT hợp lệ mà không kiểm tra quyền sở hữu (`resource.UserId == currentUserId`) trước khi đọc/sửa/xóa
        - C. Không dùng CSS Tailwind
        - D. Đặt tên controller viết thường

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Mọi thao tác trên tài nguyên cá nhân đều phải đối chiếu Ownership hoặc Role hợp lệ ở tầng Backend.

        ### Câu 4: Mục đích của Integration Test dùng `WebApplicationFactory<Program>` trong .NET là gì?
        - A. Kiểm tra cả HTTP pipeline (Routing, Auth, Middleware, DbContext, SQLite/DB) gần sát môi trường thật
        - B. Chỉ đo màu sắc giao diện CSS
        - C. Thay thế hoàn toàn việc chạy Dockerfile
        - D. Tự động đẩy code lên GitHub

        > [!TIP]
        > **Đáp án:** A
        > **Giải thích:** `WebApplicationFactory` khởi chạy TestServer in-memory giúp xác minh hành vi end-to-end của API.

        ### Câu 5: Multi-stage Build trong Dockerfile mang lại giá trị lớn nhất nào khi đóng gói .NET + Next.js?
        - A. Chèn thêm toàn bộ mã nguồn SDK vào container production
        - B. Tạo image runtime gọn nhẹ chỉ chứa file thực thi/bundle đã biên dịch, loại bỏ SDK thừa để tăng tốc deploy và bảo mật
        - C. Bắt buộc phải chạy dưới quyền root với port 22 mở công khai
        - D. Tự động nâng cấp phiên bản Windows Host

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Stage cuối chỉ copy output build sang runtime image mỏng nhẹ, giảm kích thước và bề mặt tấn công.
        """;

    private static string BuildDotNetFinalExamMarkdown() => """
        # 📋 [Đề Tổng Hợp Cuối Khóa] Lộ trình Full-Stack .NET & React (Chặng 1–14)

        ### Câu 1: Trong .NET Minimal API, vì sao `await` được khuyến khích xuyên suốt thay cho `.Result` hay `.Wait()`?
        - A. Để giải phóng Thread cho các HTTP request khác khi chờ I/O, tránh Thread Starvation và Deadlock
        - B. Để ép CPU chạy 100% ở một luồng duy nhất
        - C. Vì `.Result` không biên dịch được trên Linux
        - D. Để bỏ qua kiểm tra kiểu dữ liệu

        > [!TIP]
        > **Đáp án:** A
        > **Giải thích:** Lập trình bất đồng bộ `async/await` giải phóng worker thread trong lúc chờ DB/Network I/O.

        ### Câu 2: Trong hệ thống JWT Authentication, phần Payload của JWT có đặc điểm bảo mật nào quan trọng nhất?
        - A. Được mã hóa AES-256 không ai đọc được
        - B. Chỉ mã hóa Base64Url để truyền tải; chữ ký đảm bảo không bị sửa đổi, nên KHÔNG chứa mật khẩu/bí mật thô
        - C. Tự động hết hạn sau 1 giây dù đặt `exp` 7 ngày
        - D. Chỉ hoạt động trên trình duyệt Chrome

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** JWT Signed (JWS) chỉ chống giả mạo chữ ký, không giấu nội dung Payload.

        ### Câu 3: Cách xử lý trạng thái bất đồng bộ và tính bất biến chuẩn trong React + TypeScript là gì?
        - A. Dùng `any` cho mọi API response và sửa trực tiếp `state.items[0] = ...`
        - B. Dùng Discriminated Union (`loading | ready | error`) kết hợp cập nhật state bất biến với bản sao mảng/object mới
        - C. Lưu toàn bộ password người dùng vào `localStorage` dạng plain text
        - D. Tắt Strict Mode của TypeScript

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Discriminated Union loại bỏ bug trạng thái UI rời rạc, còn Immutability giúp React render chính xác.

        ### Câu 4: Nguyên tắc Idempotency (Lũy đẳng) trong `SyncAllFromDocumentsAsync` hoặc `POST` đồng bộ có ý nghĩa gì?
        - A. Chạy lại nhiều lần không sinh dữ liệu trùng lặp và bảo toàn trạng thái nhất quán
        - B. Chỉ chạy được duy nhất một lần rồi tự hủy
        - C. Luôn trả mã lỗi 500 ở lần gọi thứ hai
        - D. Xóa toàn bộ bảng dữ liệu trước khi chèn mới

        > [!TIP]
        > **Đáp án:** A
        > **Giải thích:** Kiểm tra khóa tự nhiên / ID tất định trước khi chèn mới giúp hệ thống an toàn khi retry hoặc re-sync.

        ### Câu 5: Theo mô hình Twelve-Factor App, thông số nhạy cảm như `ConnectionString` và `Gemini__ApiKey` nên lưu ở đâu?
        - A. Ghi cứng vào file `Program.cs` rồi commit lên Git
        - B. Nạp qua Environment Variables hoặc Secret Manager ở runtime, tách biệt hoàn toàn khỏi mã nguồn
        - C. Lưu trong `README.md` để mọi thành viên tiện sao chép
        - D. Gửi qua URL query string của mọi GET request

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Lưu cấu hình theo môi trường qua biến môi trường giúp bảo mật thông tin nhạy cảm khi triển khai Dev/Prod.
        """;

    private static string BuildIeltsChapter1ExamMarkdown() => """
        # 📋 [Đề Chương 1] IELTS Phase 0–1: Nền tảng Phát âm, Từ vựng, Ngữ pháp & Nghe Đọc

        ### Câu 1: Khi đọc bảng phiên âm IPA, sự khác biệt giữa phụ âm vô thanh `/θ/` (như *think*) và hữu thanh `/ð/` (như *this*) là gì?
        - A. Khác nhau ở vị trí môi tròn hay dẹt
        - B. Cùng vị trí đầu lưỡi giữa hai răng, nhưng `/ð/` làm rung dây thanh quản còn `/θ/` chỉ bật luồng gió nhẹ
        - C. `/θ/` đọc giống chữ "th" tiếng Việt có bật hơi mạnh ở mũi
        - D. Hai âm phát âm hoàn toàn giống nhau

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Cặp âm `/θ/ - /ð/` cùng khẩu hình đặt đầu lưỡi chạm nhẹ hai hàm răng, chỉ khác tính vô thanh vs hữu thanh.

        ### Câu 2: Chiến lược nghe hiệu quả nhất trong IELTS Listening Phase 1 khi gặp câu hỏi điền từ (Form/Note Completion) là gì?
        - A. Chỉ lắng nghe khi băng đã đọc xong toàn bộ bài
        - B. Gạch chân từ khóa và dự đoán trước loại từ (danh từ, số, ngày tháng) + giới hạn số từ trong lúc đọc đề
        - C. Cố gắng dịch từng từ sang tiếng Việt ngay lúc nghe
        - D. Chọn đáp án dài nhất ở mọi câu

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Dự đoán từ loại giúp não bộ bắt đúng tín hiệu thông tin đích và tránh sai chính tả số ít/số nhiều.

        ### Câu 3: Trong câu phức học thuật IELTS: *"Although public transit was upgraded, congestion persisted."*, mệnh đề *"Although..."* thể hiện quan hệ gì?
        - A. Nguyên nhân - Kết quả
        - B. Nhượng bộ - Tương phản (Concession / Contrast)
        - C. Điều kiện giả định
        - D. So sánh hơn

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** `Although` giới thiệu mệnh đề tương phản/nhượng bộ; không dùng kèm `but` trong cùng câu.

        ### Câu 4: Kỹ thuật Đọc lướt tìm ý chính đoạn văn (Skimming) khác Đọc quét tìm dữ liệu cụ thể (Scanning) ở điểm nào?
        - A. Skimming đọc tiêu đề & câu chủ đề (topic sentence) để nắm đại ý; Scanning tìm tên riêng, con số, từ khóa chính xác
        - B. Cả hai đều yêu cầu tra từ điển từng từ một từ đầu đến cuối
        - C. Scanning chỉ áp dụng cho bài thi Listening
        - D. Skimming bỏ qua hoàn toàn đoạn mở bài

        > [!TIP]
        > **Đáp án:** A
        > **Giải thích:** Kết hợp Skimming nắm cấu trúc bài đọc và Scanning định vị chứng cứ giúp tiết kiệm thời gian tối đa.

        ### Câu 5: Chọn câu đúng về Hòa hợp Chủ ngữ - Động từ (Subject-Verb Agreement):
        - A. The number of international applicants have increased rapidly.
        - B. The number of international applicants has increased rapidly.
        - C. A number of students is attending the workshop.
        - D. Everyone in the lecture hall have a notebook.

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Cấu trúc `The number of + danh từ số nhiều` chia động từ số ít (`has`); ngược lại `A number of` chia số nhiều.
        """;

    private static string BuildIeltsChapter2ExamMarkdown() => """
        # 📋 [Đề Chương 2] IELTS Phase 2: Tăng tốc 4 Kỹ năng Listening, Reading, Speaking & Writing

        ### Câu 1: Trong IELTS Reading dạng True / False / Not Given, khi nào chọn **NOT GIVEN** thay vì **FALSE**?
        - A. Khi bài đọc đưa ra thông tin trái ngược hoàn toàn với nhận định trong câu hỏi
        - B. Khi bài đọc không chứa đủ bằng chứng xác nhận nhận định đúng hay sai (dù kiến thức ngoài đời có vẻ hợp lý)
        - C. Khi gặp từ vựng đồng nghĩa (synonym)
        - D. Khi đoạn văn nằm ở cuối bài đọc

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** `FALSE` phải có mâu thuẫn trực tiếp với văn bản; `NOT GIVEN` nghĩa là không có thông tin kiểm chứng trong bài.

        ### Câu 2: Trong IELTS Listening Section 2/3, "bẫy tự sửa thông tin" (Distractor / Self-correction) thường đi kèm tín hiệu nào?
        - A. Người nói đọc thật to một lần rồi im lặng
        - B. Cụm chuyển ý như *"Actually, wait..."*, *"Sorry, I meant..."*, *"However, we moved it to..."*
        - C. Kết thúc bản ghi âm
        - D. Tiếng chuông báo giờ

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Giám khảo thường cài đáp án giả trước rồi sửa lại bằng `actually / however / sorry, I meant` ngay sau đó.

        ### Câu 3: Điểm mấu chốt giúp đạt Band 7.0+ ở IELTS Writing Task 1 (Biểu đồ xu hướng) là gì?
        - A. Liệt kê tất cả mọi con số từng năm mà không có đoạn tổng quan
        - B. Viết đoạn Overview nổi bật nêu xu hướng chính và các điểm cực trị, kèm chọn lọc số liệu tiêu biểu so sánh
        - C. Đưa quan điểm cá nhân giải thích nguyên nhân chính trị xã hội ngoài biểu đồ
        - D. Chỉ viết dưới 100 từ

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Task 1 đánh giá khả năng khái quát xu hướng (Overview) và so sánh số liệu chính xác, không đưa ý kiến chủ quan.

        ### Câu 4: Khi trả lời IELTS Speaking Part 2 (Long Turn 2 phút), cấu trúc mở rộng nào giúp giữ sự mạch lạc và tự nhiên?
        - A. Đọc thuộc lòng 1 đoạn văn mẫu về chủ đề khác
        - B. Triển khai theo khung Bối cảnh -> Diễn biến chi tiết/Cảm xúc -> Bài học/Tương lai (Who/Where - What happened - Why meaningful)
        - C. Lặp lại y nguyên đề bài 5 lần cho đủ giờ
        - D. Chỉ trả lời vỏn vẹn 2 câu rồi dừng

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Lộ trình kể chuyện rõ ràng giúp duy trì Fluency & Coherence trọn vẹn 2 phút mà không bị lạc đề.

        ### Câu 5: Chọn paraphrase chuẩn học thuật nhất cho cụm *"a lot of people think that"* trong IELTS Writing Task 2:
        - A. tons of guys feel like
        - B. it is widely believed that / a growing number of individuals argue that
        - C. many peoples thinks
        - D. everyone knows without doubt

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Văn phong học thuật IELTS ưu tiên cấu trúc khách quan như *It is widely argued that...* thay cho từ lóng hay khẳng định cực đoan.
        """;

    private static string BuildIeltsChapter3ExamMarkdown() => """
        # 📋 [Đề Chương 3] IELTS Phase 3: Mastery & Chiến thuật Band 6.5+

        ### Câu 1: Để tối đa hóa điểm Lexical Resource ở IELTS Speaking & Writing mà không bị "gượng ép từ vựng hiếm", chiến lược đúng là gì?
        - A. Nhồi nhét thật nhiều thành ngữ cổ và từ vựng ít dùng bất chấp ngữ cảnh
        - B. Sử dụng chính xác Collocations tự nhiên (như *pose a threat*, *mitigate impacts*) đúng văn phong và sắc thái
        - C. Lặp đi lặp lại duy nhất từ *important* và *good*
        - D. Viết tắt mọi từ khóa

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Band 7+ yêu cầu dùng từ vựng ít phổ biến kèm nhận thức về văn phong và kết hợp từ (Collocation) chính xác.

        ### Câu 2: Một đoạn thân bài (Body Paragraph) chuẩn P.E.E.L trong IELTS Writing Task 2 gồm những thành phần nào?
        - A. Point (Luận điểm chính) -> Explanation (Giải thích cơ chế) -> Example (Ví dụ cụ thể) -> Link (Chốt ý gắn với đề)
        - B. Chỉ gồm 4 ví dụ rời rạc không có câu chủ đề
        - C. Một câu đơn 90 chữ không dấu chấm
        - D. Chép nguyên văn đề bài vào giữa đoạn

        > [!TIP]
        > **Đáp án:** A
        > **Giải thích:** Cấu trúc P.E.E.L giúp phát triển lập luận có chiều sâu (Task Response) và liên kết chặt chẽ (Cohesion).

        ### Câu 3: Khi xử lý IELTS Reading dạng Matching Headings, trình tự làm bài tối ưu giúp tránh bẫy tiêu đề nhiễu là gì?
        - A. Chỉ đọc 3 chữ đầu tiên của mỗi đoạn rồi chọn ngay
        - B. Đọc toàn đoạn nắm ý chủ đạo (câu đầu + câu kết + ý lặp lại), tự tóm tắt 1 câu ngắn trước khi đối chiếu danh sách Headings
        - C. Chọn ngẫu nhiên theo thứ tự từ trên xuống dưới
        - D. Làm bài đọc Passage 3 trước khi xem câu hỏi

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Tự tổng hợp ý chính trước giúp bạn không bị đánh lừa bởi các tiêu đề chứa một vài từ trùng khớp bề mặt.

        ### Câu 4: Trong Speaking Part 3 (thảo luận trừu tượng), nếu gặp câu hỏi khó cần thêm 2 giây suy nghĩ, cách xử lý chuẩn xác nhất là gì?
        - A. Im lặng hoàn toàn 15 giây không nói gì
        - B. Dùng các cụm câu giờ tự nhiên (Natural Fillers) như *"That's an intriguing question; off the top of my head, I'd say..."*
        - C. Nói tiếng Việt xin giám khảo đổi câu hỏi
        - D. Đứng dậy kết thúc buổi thi

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Natural conversational fillers giữ mạch giao tiếp tự nhiên trong lúc não bộ sắp xếp ý tưởng đa chiều.

        ### Câu 5: Trong câu điều kiện hỗn hợp hoặc đảo ngữ nâng cao cho tiêu chí Grammatical Range (GRA), câu nào đúng ngữ pháp?
        - A. Not only governments should invest in green energy, but also educate citizens.
        - B. Not only should governments invest in green energy, but they should also raise public awareness.
        - C. Should governments investing more, pollution drops.
        - D. Were I have enough funding, I would build a library.

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Sau trạng từ phủ định đầu câu (`Not only`), ta phải đảo trợ động từ (`should`) lên trước chủ ngữ (`governments`).
        """;

    private static string BuildIeltsFinalExamMarkdown() => """
        # 📋 [Đề Tổng Hợp Cuối Khóa] Thi Thử Tổng Hợp IELTS 4 Kỹ Năng

        ### Câu 1 (Listening Strategy): Trong phần Multiple Choice Section 3 giữa hai sinh viên thảo luận đồ án, yếu tố quyết định đáp án đúng là gì?
        - A. Ý kiến người nói đầu tiên nêu ra trước khi bị người kia phản bác
        - B. Quan điểm cuối cùng mà cả hai bên thống nhất hoặc đúng với chủ ngữ câu hỏi đề cập
        - C. Đáp án xuất hiện ở giây thứ 2
        - D. Phương án có chữ in hoa

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Thảo luận học thuật Section 3 thường đưa ra đề xuất A, phản biện B, rồi chốt phương án C; cần bám sát sự đồng thuận cuối cùng.

        ### Câu 2 (Reading Precision): Trong câu *""The initiative, albeit costly initially, yielded substantial long-term dividends."*", từ **albeit** mang nghĩa gần nhất với từ nào?
        - A. bởi vì (because)
        - B. mặc dù (although / even though)
        - C. ngoại trừ (except)
        - D. ngay lập tức (immediately)

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** `albeit` là liên từ trang trọng mang nghĩa "mặc dù / dẫu cho", nối cụm tính từ hoặc trạng từ tương phản.

        ### Câu 3 (Writing Task 2 Coherence): Khi đề bài yêu cầu *"Discuss both views and give your own opinion"*, bố cục nào đạt điểm Task Response tối đa?
        - A. Chỉ phân tích một quan điểm mình đồng ý và bỏ qua quan điểm còn lại
        - B. Trình bày công bằng cả hai luồng quan điểm ở 2 đoạn thân bài và khẳng định rõ lập trường cá nhân ở Mở bài + Kết bài (hoặc đoạn 3)
        - C. Kết luận bằng câu "cả hai đều đúng tùy từng người" mà không chốt lập trường
        - D. Viết toàn bộ bằng gạch đầu dòng liệt kê

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Cần phân tích cân đối hai phía và nêu quan điểm cá nhân nhất quán xuyên suốt bài viết.

        ### Câu 4 (Speaking Pronunciation & Stress): Từ học thuật nào dưới đây có trọng âm rơi vào âm tiết thứ HAI (`-`-`-`)?
        - A. development (`/dɪˈvel.əp.mənt/`)
        - B. category (`/ˈkæt.ə.ɡri/`)
        - C. consequence (`/ˈkɒn.sɪ.kwəns/`)
        - D. benefit (`/ˈben.ɪ.fɪt/`)

        > [!TIP]
        > **Đáp án:** A
        > **Giải thích:** `de-VEL-op-ment` nhấn âm 2; các từ còn lại (`CA-te-go-ry`, `CON-se-quence`, `BE-ne-fit`) nhấn âm 1.

        ### Câu 5 (Active Recall & Error Log): Phương pháp tốt nhất để biến lỗi sai khi luyện đề thành điểm cộng thực sự trước ngày thi IELTS là gì?
        - A. Làm thật nhiều đề liên tục mà không bao giờ xem lại đáp án sai
        - B. Ghi chép Error Log theo loại bẫy/ngữ pháp, tạo thẻ FSRS ôn lại từ đồng nghĩa và luyện nói to câu chuẩn
        - C. Chỉ học thuộc bộ từ vựng C2 rời rạc không gắn ngữ cảnh
        - D. Ngủ ít hơn 4 tiếng để đọc hết sách tham khảo

        > [!TIP]
        > **Đáp án:** B
        > **Giải thích:** Vòng lặp Error Log + Spaced Repetition (FSRS) + Shadowing giúp vá đúng lỗ hổng cá nhân và tăng phản xạ thực chiến.
        """;
}
