namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;

/// <summary>
/// Expands Operating Systems (AcademicCourseId) and English A2-B1 (EnglishCourseId)
/// from 3 Chapters / 6 Lessons to 5 Chapters / 12 Lessons each.
/// Strictly additive and idempotent: NEVER modifies LessonPagingId, Chapters 1..3,
/// existing Lessons 1.1..3.2, or any lesson with CurrentVersion > 1 (e.g. == 2).
/// </summary>
public static class ExpandedSeededCoursesContent
{
    public const string OsMarkerV4 = "<!-- studxy-os-v4 -->";
    public const string OsMarkerV5 = "<!-- studxy-os-v5 -->";
    public const string EnMarkerV2 = "<!-- studxy-en-v2 -->";
    public const string EnMarkerV3 = "<!-- studxy-en-v3 -->";

    // Deterministic IDs for OS Chapters 4..5 & Lessons 4.1..5.3
    public static readonly Guid OsChapter4Id = SeedDataInitializer.CreateDeterministicGuid("swms:os:chapter:4");
    public static readonly Guid OsChapter5Id = SeedDataInitializer.CreateDeterministicGuid("swms:os:chapter:5");
    public static readonly Guid OsLesson41Id = SeedDataInitializer.CreateDeterministicGuid("swms:os:lesson:4.1");
    public static readonly Guid OsLesson42Id = SeedDataInitializer.CreateDeterministicGuid("swms:os:lesson:4.2");
    public static readonly Guid OsLesson43Id = SeedDataInitializer.CreateDeterministicGuid("swms:os:lesson:4.3");
    public static readonly Guid OsLesson51Id = SeedDataInitializer.CreateDeterministicGuid("swms:os:lesson:5.1");
    public static readonly Guid OsLesson52Id = SeedDataInitializer.CreateDeterministicGuid("swms:os:lesson:5.2");
    public static readonly Guid OsLesson53Id = SeedDataInitializer.CreateDeterministicGuid("swms:os:lesson:5.3");

    // Deterministic IDs for English Chapters 4..5 & Lessons 4.1..5.3
    public static readonly Guid EnChapter4Id = SeedDataInitializer.CreateDeterministicGuid("swms:en:chapter:4");
    public static readonly Guid EnChapter5Id = SeedDataInitializer.CreateDeterministicGuid("swms:en:chapter:5");
    public static readonly Guid EnLesson41Id = SeedDataInitializer.CreateDeterministicGuid("swms:en:lesson:4.1");
    public static readonly Guid EnLesson42Id = SeedDataInitializer.CreateDeterministicGuid("swms:en:lesson:4.2");
    public static readonly Guid EnLesson43Id = SeedDataInitializer.CreateDeterministicGuid("swms:en:lesson:4.3");
    public static readonly Guid EnLesson51Id = SeedDataInitializer.CreateDeterministicGuid("swms:en:lesson:5.1");
    public static readonly Guid EnLesson52Id = SeedDataInitializer.CreateDeterministicGuid("swms:en:lesson:5.2");
    public static readonly Guid EnLesson53Id = SeedDataInitializer.CreateDeterministicGuid("swms:en:lesson:5.3");

    private static readonly JsonSerializerOptions SafeJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private sealed record LessonMeta(
        Guid ChapterId,
        Guid LessonId,
        string Title,
        string Slug,
        int SortOrder,
        int EstimatedMinutes,
        string Markdown,
        string Summary1,
        string Summary2
    );

    private sealed record QuizQuestionSpec(
        string ItemType, // multiple_choice | cloze | essay
        string Question,
        string? OptionA,
        string? OptionB,
        string? OptionC,
        string? OptionD,
        string Solution,
        string Explanation,
        string[]? RubricCriteria = null
    );

    // English PracticeActivity ConfigJson DTOs
    private sealed record EnShItem(string Id, string Lang, string Difficulty, string Topic, string Sentence, string Ipa, string MeaningVi, string[] KeyPhrases);
    private sealed record EnDiItem(string Id, string Lang, string Difficulty, string Topic, string Sentence, string Ipa, string MeaningVi, string[] WordChips, string[] Distractors);
    private sealed record EnPrItem(string Id, string Lang, string Difficulty, string Topic, string Sentence, string Ipa, string MeaningVi, string[] TargetWords, string[] KeyPhrases);
    private sealed record EnWmNode(string Text, string Ipa, string MeaningVi, string Example);
    private sealed record EnWmEx(string Sentence, string MeaningVi);
    private sealed record EnWmItem(string Id, string Lang, string Difficulty, string Title, string CentralWord, string Ipa, string MeaningVi, EnWmNode[] Collocations, EnWmNode[] PhrasalVerbs, EnWmNode[] Synonyms, EnWmEx[] ContextExamples);
    private sealed record EnClBlank(string[] AcceptedAnswers, string HintVi, string ExplanationVi);
    private sealed record EnClItem(string Id, string Difficulty, string Title, string PromptVi, string Template, EnClBlank[] Blanks, string FullSentence, string MeaningVi, string ExplanationVi);
    private sealed record EnTrItem(string Id, string Difficulty, string Title, string SourceVi, string[] HintKeywords, string[] AcceptedAnswers, string SampleAnswer, string GrammarNoteVi);
    private sealed record EnGfItem(string Id, string Difficulty, string Title, string IncorrectSentence, string ErrorTypeHintVi, string CorrectedSentence, string[] AcceptedAnswers, string MeaningVi, string ExplanationVi);
    private sealed record EnRpItem(string Id, string Difficulty, string Title, string ScenarioVi, string AiRole, string AiPrompt, string AiPromptVi, string UserGoalVi, string[] ExpectedKeywords, string[] SampleResponses, int MinWords);

    public static async Task EnsureSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var hasOsCourse = await db.Courses.AnyAsync(c => c.Id == SeedDataInitializer.AcademicCourseId);
        var hasEnCourse = await db.Courses.AnyAsync(c => c.Id == SeedDataInitializer.EnglishCourseId);

        if (!hasOsCourse && !hasEnCourse)
        {
            return;
        }

        if (hasOsCourse)
        {
            await EnsureCourseExpansionAsync(
                db,
                courseId: SeedDataInitializer.AcademicCourseId,
                chapters: new[]
                {
                    (OsChapter4Id, "Chương 4: Hệ Thống Tệp Tin, I/O & Tối Ưu Bộ Nhớ Đĩa", 4),
                    (OsChapter5Id, "Chương 5: Đồng Bộ Hóa Nâng Cao, Kernel Linux & Ảo Hóa Container", 5)
                },
                lessons: GetOsExpandedLessons(),
                requiredMarkers: new[] { OsMarkerV4, OsMarkerV5 },
                activities: GetOsExpandedActivities(SeedDataInitializer.AcademicCourseId, now),
                now: now);

            await EnsureQuizzesAsync(db, "os-exp", GetOsQuizSpecs(), now);
            await EnsureOsExpandedFlashcardsAsync(db, now);
        }

        if (hasEnCourse)
        {
            await EnsureCourseExpansionAsync(
                db,
                courseId: SeedDataInitializer.EnglishCourseId,
                chapters: new[]
                {
                    (EnChapter4Id, "Chương 4: Tình Huống Du Lịch, Phỏng Vấn Tech & Cộng Tác Đa Quốc Gia", 4),
                    (EnChapter5Id, "Chương 5: Kể Chuyện Thuyết Phục, Viết Chuẩn B1 & Làm Việc Cùng AI Toàn Cầu", 5)
                },
                lessons: GetEnExpandedLessons(),
                requiredMarkers: new[] { EnMarkerV2, EnMarkerV3 },
                activities: GetEnExpandedActivities(SeedDataInitializer.EnglishCourseId, now),
                now: now);

            await EnsureQuizzesAsync(db, "en-exp", GetEnQuizSpecs(), now);
            await EnsureEnExpandedFlashcardsAsync(db, now);
        }
    }

    private static async Task EnsureCourseExpansionAsync(
        SwmsDbContext db,
        Guid courseId,
        (Guid Id, string Title, int SortOrder)[] chapters,
        List<LessonMeta> lessons,
        string[] requiredMarkers,
        List<PracticeActivity> activities,
        DateTimeOffset now)
    {
        var existingChapterIds = (await db.Chapters
            .Where(c => c.CourseId == courseId)
            .Select(c => c.Id)
            .ToListAsync())
            .ToHashSet();

        var chaptersToAdd = new List<Chapter>();
        foreach (var ch in chapters)
        {
            if (!existingChapterIds.Contains(ch.Id))
            {
                chaptersToAdd.Add(new Chapter
                {
                    Id = ch.Id,
                    CourseId = courseId,
                    Title = ch.Title,
                    SortOrder = ch.SortOrder,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                existingChapterIds.Add(ch.Id);
            }
        }
        if (chaptersToAdd.Count > 0)
        {
            db.Chapters.AddRange(chaptersToAdd);
            await db.SaveChangesAsync();
        }

        var targetLessonIds = lessons.Select(l => l.LessonId).ToList();
        var existingLessons = await db.Lessons
            .Where(l => targetLessonIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id);

        foreach (var meta in lessons)
        {
            // Strict safeguard: never touch LessonPagingId or any existing lesson with CurrentVersion > 1
            if (meta.LessonId == SeedDataInitializer.LessonPagingId) continue;

            if (existingLessons.TryGetValue(meta.LessonId, out var existing))
            {
                var md = existing.ContentMarkdown ?? string.Empty;
                var hasAllMarkers = requiredMarkers.All(m => md.Contains(m, StringComparison.Ordinal));
                if (existing.CurrentVersion == 1 && !hasAllMarkers)
                {
                    existing.ContentMarkdown = meta.Markdown;
                    existing.PlainText = meta.Markdown;
                    existing.EstimatedMinutes = meta.EstimatedMinutes;
                    existing.ContentAstJson = BuildLessonAst(meta.LessonId, existing.Title, meta.Summary1, meta.Summary2);
                    existing.UpdatedAt = now;
                }
            }
            else if (!await db.Lessons.AnyAsync(l => l.Slug == meta.Slug))
            {
                db.Lessons.Add(new Lesson
                {
                    Id = meta.LessonId,
                    ChapterId = meta.ChapterId,
                    Title = meta.Title,
                    Slug = meta.Slug,
                    SortOrder = meta.SortOrder,
                    CurrentVersion = 1,
                    EstimatedMinutes = meta.EstimatedMinutes,
                    ContentMarkdown = meta.Markdown,
                    PlainText = meta.Markdown,
                    ContentAstJson = BuildLessonAst(meta.LessonId, meta.Title, meta.Summary1, meta.Summary2),
                    IsPublished = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }
        await db.SaveChangesAsync();

        var existingActivities = await db.PracticeActivities
            .Where(pa => pa.CourseId == courseId)
            .ToListAsync();
        var existingIds = existingActivities.Select(pa => pa.Id).ToHashSet();
        var existingComposite = existingActivities
            .Select(pa => (pa.LessonId, Type: (pa.Type ?? string.Empty).Trim().ToLowerInvariant(), pa.OrderIndex))
            .ToHashSet();

        var toAdd = new List<PracticeActivity>();
        foreach (var act in activities)
        {
            act.CourseId = courseId;
            var key = (act.LessonId, Type: (act.Type ?? string.Empty).Trim().ToLowerInvariant(), act.OrderIndex);
            if (existingIds.Contains(act.Id) || existingComposite.Contains(key))
            {
                continue;
            }
            existingIds.Add(act.Id);
            existingComposite.Add(key);
            toAdd.Add(act);
        }

        if (toAdd.Count > 0)
        {
            db.PracticeActivities.AddRange(toAdd);
            await db.SaveChangesAsync();
        }
    }

    private static string BuildLessonAst(Guid lessonId, string title, string p1, string p2)
    {
        var ast = new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 1, block_id = SeedDataInitializer.CreateDeterministicGuid($"swms:exp-ast:{lessonId:N}:h1").ToString() },
                    content = new[] { new { type = "text", text = title } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = SeedDataInitializer.CreateDeterministicGuid($"swms:exp-ast:{lessonId:N}:p1").ToString() },
                    content = new[] { new { type = "text", text = p1 } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = SeedDataInitializer.CreateDeterministicGuid($"swms:exp-ast:{lessonId:N}:p2").ToString() },
                    content = new[] { new { type = "text", text = p2 } }
                }
            }
        };
        return JsonSerializer.Serialize(ast, SafeJsonOptions);
    }

    private static async Task EnsureQuizzesAsync(
        SwmsDbContext db,
        string domainPrefix,
        List<(string Key, Guid LessonId, string Title, QuizQuestionSpec[] Questions)> specs,
        DateTimeOffset now)
    {
        var targetLessonIds = specs.Select(s => s.LessonId).Distinct().ToList();
        var existingLessonIds = (await db.Lessons
            .AsNoTracking()
            .Where(l => targetLessonIds.Contains(l.Id))
            .Select(l => l.Id)
            .ToListAsync())
            .ToHashSet();

        foreach (var (key, lessonId, title, questions) in specs)
        {
            if (!existingLessonIds.Contains(lessonId)) continue;

            var quizId = SeedDataInitializer.CreateDeterministicGuid($"swms:{domainPrefix}:quiz:{key}");
            var existing = await db.PracticeQuizzes.FirstOrDefaultAsync(q =>
                q.Id == quizId ||
                (q.UserId == SeedDataInitializer.DefaultUserId && q.LessonId == lessonId && q.Title == title));

            if (existing == null)
            {
                var quiz = new PracticeQuiz
                {
                    Id = quizId,
                    UserId = SeedDataInitializer.DefaultUserId,
                    Title = title,
                    LessonId = lessonId,
                    TotalItems = questions.Length,
                    CorrectItems = 0,
                    Status = "in_progress",
                    StartedAt = now
                };
                db.PracticeQuizzes.Add(quiz);

                for (int i = 0; i < questions.Length; i++)
                {
                    db.PracticeQuizItems.Add(BuildQuizItem(quizId, domainPrefix, key, i, questions[i], now));
                }
            }
            else
            {
                var existingCount = await db.PracticeQuizItems.CountAsync(it => it.QuizId == existing.Id);
                if (existingCount == 0)
                {
                    for (int i = 0; i < questions.Length; i++)
                    {
                        db.PracticeQuizItems.Add(BuildQuizItem(existing.Id, domainPrefix, key, i, questions[i], now));
                    }
                    existing.TotalItems = questions.Length;
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static PracticeQuizItem BuildQuizItem(
        Guid quizId,
        string domainPrefix,
        string key,
        int idx,
        QuizQuestionSpec q,
        DateTimeOffset now)
    {
        string promptJson = q.RubricCriteria is { Length: > 0 }
            ? JsonSerializer.Serialize(new { question = q.Question, rubricCriteria = q.RubricCriteria }, SafeJsonOptions)
            : JsonSerializer.Serialize(new { question = q.Question }, SafeJsonOptions);

        string? choicesJson = null;
        if (q.ItemType == "multiple_choice")
        {
            choicesJson = JsonSerializer.Serialize(new[]
            {
                new { key = "A", text = q.OptionA ?? string.Empty },
                new { key = "B", text = q.OptionB ?? string.Empty },
                new { key = "C", text = q.OptionC ?? string.Empty },
                new { key = "D", text = q.OptionD ?? string.Empty }
            }, SafeJsonOptions);
        }

        return new PracticeQuizItem
        {
            Id = SeedDataInitializer.CreateDeterministicGuid($"swms:{domainPrefix}:quiz-item:{key}:{idx + 1}"),
            QuizId = quizId,
            ItemType = q.ItemType,
            PromptAstJson = promptJson,
            ChoicesJson = choicesJson,
            ClozeSolution = q.Solution,
            AiRubricFeedbackJson = JsonSerializer.Serialize(new { summary = q.Explanation }, SafeJsonOptions),
            CreatedAt = now.AddSeconds(idx)
        };
    }

    private static async Task<Tag> EnsureTagAsync(SwmsDbContext db, string tagName, string colorKey, DateTimeOffset now)
    {
        var tagId = SeedDataInitializer.CreateDeterministicGuid($"swms:tag:{SeedDataInitializer.DefaultUserId:N}:{tagName.Trim().ToLowerInvariant()}");
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Id == tagId || (t.UserId == SeedDataInitializer.DefaultUserId && t.Name == tagName));
        if (tag == null)
        {
            tag = new Tag
            {
                Id = tagId,
                UserId = SeedDataInitializer.DefaultUserId,
                Name = tagName,
                ColorKey = colorKey,
                CreatedAt = now
            };
            db.Tags.Add(tag);
            await db.SaveChangesAsync();
        }
        return tag;
    }

    private static async Task SeedFlashcardsIdempotentlyAsync(
        SwmsDbContext db,
        string domainKey,
        Tag tag,
        string cardType,
        Func<Guid, string> sourceTitleResolver,
        (Guid LessonId, string Front, string Back)[] specs,
        DateTimeOffset now)
    {
        var targetLessonIds = specs.Select(s => s.LessonId).Distinct().ToList();
        var existingLessonIds = (await db.Lessons
            .AsNoTracking()
            .Where(l => targetLessonIds.Contains(l.Id))
            .Select(l => l.Id)
            .ToListAsync())
            .ToHashSet();

        if (existingLessonIds.Count == 0) return;

        var fallbackLessonId = existingLessonIds.First();

        var existingCardIds = (await db.Flashcards
            .AsNoTracking()
            .Where(f => f.UserId == SeedDataInitializer.DefaultUserId)
            .Select(f => f.Id)
            .ToListAsync())
            .ToHashSet();

        var existingFronts = (await db.Flashcards
            .AsNoTracking()
            .Where(f => f.UserId == SeedDataInitializer.DefaultUserId)
            .Select(f => f.FrontAstJson)
            .ToListAsync())
            .ToHashSet();

        var existingTagPairs = (await db.FlashcardTags
            .AsNoTracking()
            .Where(ft => ft.TagId == tag.Id)
            .Select(ft => ft.FlashcardId)
            .ToListAsync())
            .ToHashSet();

        var cardsToAdd = new List<Flashcard>();
        var tagsToAdd = new List<FlashcardTag>();

        for (int i = 0; i < specs.Length; i++)
        {
            var (lessonId, front, back) = specs[i];
            var actualLessonId = existingLessonIds.Contains(lessonId) ? lessonId : fallbackLessonId;
            var cardId = SeedDataInitializer.CreateDeterministicGuid($"swms:{domainKey}-fc:{actualLessonId:N}:{i + 1}");
            var frontJson = JsonSerializer.Serialize(new { text = front }, SafeJsonOptions);
            var backJson = JsonSerializer.Serialize(new { text = back }, SafeJsonOptions);

            if (!existingCardIds.Contains(cardId) && !existingFronts.Contains(frontJson))
            {
                cardsToAdd.Add(new Flashcard
                {
                    Id = cardId,
                    UserId = SeedDataInitializer.DefaultUserId,
                    LessonId = actualLessonId,
                    CardType = cardType,
                    SourceTitleSnapshot = sourceTitleResolver(actualLessonId),
                    FrontAstJson = frontJson,
                    BackAstJson = backJson,
                    Stability = 0.0,
                    Difficulty = 0.0,
                    Reps = 0,
                    Lapses = 0,
                    State = 0,
                    IsArchived = false,
                    DueAt = now.AddMinutes(-i),
                    CreatedAt = now,
                    UpdatedAt = now
                });
                existingCardIds.Add(cardId);
                existingFronts.Add(frontJson);
            }

            if (existingCardIds.Contains(cardId) && !existingTagPairs.Contains(cardId))
            {
                tagsToAdd.Add(new FlashcardTag
                {
                    FlashcardId = cardId,
                    TagId = tag.Id,
                    CreatedAt = now
                });
                existingTagPairs.Add(cardId);
            }
        }

        if (cardsToAdd.Count > 0) db.Flashcards.AddRange(cardsToAdd);
        if (tagsToAdd.Count > 0) db.FlashcardTags.AddRange(tagsToAdd);
        if (cardsToAdd.Count > 0 || tagsToAdd.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }

    private static PracticeActivity MakeOsActivity(
        Guid courseId,
        Guid lessonId,
        int orderIndex,
        string type,
        string title,
        string description,
        object config,
        DateTimeOffset now)
    {
        return new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lessonId, type, orderIndex),
            CourseId = courseId,
            LessonId = lessonId,
            OrderIndex = orderIndex,
            Type = type,
            Title = title,
            Description = description,
            ConfigJson = JsonSerializer.Serialize(config, SafeJsonOptions),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static PracticeActivity MakeEnActivity(
        Guid courseId,
        Guid lessonId,
        int orderIndex,
        string type,
        string title,
        string description,
        object items,
        DateTimeOffset now)
    {
        return new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lessonId, type, orderIndex),
            CourseId = courseId,
            LessonId = lessonId,
            OrderIndex = orderIndex,
            Type = type,
            Title = title,
            Description = description,
            ConfigJson = JsonSerializer.Serialize(new
            {
                lang = "en-US",
                difficulty = "medium",
                items
            }, SafeJsonOptions),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    #region 1. Operating Systems (AcademicCourseId) Lessons 4.1..5.3

    private static List<LessonMeta> GetOsExpandedLessons() => new()
    {
        new LessonMeta(
            OsChapter4Id, OsLesson41Id,
            "Bài 4.1: Kiến trúc Hệ thống Tệp tin: Inode, Extent, B-Tree & Journaling (ext4 / NTFS / APFS)",
            "os-kien-truc-he-thong-tep-tin-inode-extent-journaling",
            1, 45,
            GetOsLesson41Markdown(),
            "Tách biệt siêu dữ liệu (Inode) khỏi tên tệp (Directory Entry) cho phép hỗ trợ Hard Link, phân quyền POSIX và truy xuất khối dữ liệu O(1)/O(log N).",
            "Extent tree giảm phân mảnh cho tệp lớn trên ext4, trong khi Journaling (WAL) bảo vệ tính toàn vẹn metadata khi mất điện đột ngột."
        ),
        new LessonMeta(
            OsChapter4Id, OsLesson42Id,
            "Bài 4.2: Hệ thống I/O, Page Cache, Direct I/O & Thuật toán Lập lịch Đĩa (NVMe / SSD / Queue)",
            "os-he-thong-io-page-cache-direct-io-nvme",
            2, 45,
            GetOsLesson42Markdown(),
            "Linux Page Cache gom nhiều lượt ghi nhỏ thành Dirty Pages và xả bất đồng bộ qua pdflush/writeback, tăng thông lượng gấp nhiều lần nhưng đòi hỏi fsync() khi cần độ bền.",
            "Direct I/O (O_DIRECT) bỏ qua Page Cache cho các DBMS tự quản lý buffer pool, còn NVMe Multi-Queue thay thế bộ lập lịch đơn hàng đợi cũ để khai thác song song phần cứng."
        ),
        new LessonMeta(
            OsChapter4Id, OsLesson43Id,
            "Bài 4.3: RAID, Copy-on-Write (ZFS / Btrfs), Snapshots & Độ Bền Dữ Liệu (Crash Consistency)",
            "os-raid-copy-on-write-zfs-btrfs-crash-consistency",
            3, 45,
            GetOsLesson43Markdown(),
            "RAID phân tách hiệu năng và dự phòng (RAID 0/1/5/6/10) với đánh đổi Write Penalty và lỗ hổng Write Hole khi mất điện giữa chừng.",
            "Copy-on-Write (CoW) trên ZFS/Btrfs không bao giờ ghi đè tại chỗ mà tạo khối mới rồi lật con trỏ gốc nguyên tử, giúp tạo Snapshot tức thời và chống Bit Rot bằng Checksum."
        ),
        new LessonMeta(
            OsChapter5Id, OsLesson51Id,
            "Bài 5.1: Locks, Spinlock vs Mutex, Lock-Free Atomic CAS & Phòng Chống Priority Inversion",
            "os-locks-spinlock-mutex-atomic-cas-priority-inversion",
            1, 45,
            GetOsLesson51Markdown(),
            "Spinlock giữ CPU quay vòng chờ (phù hợp vùng tranh chấp cực ngắn trong Kernel), trong khi Mutex đưa luồng vào trạng thái ngủ (Futex) để nhường CPU.",
            "Compare-And-Swap (CAS) xây dựng cấu trúc dữ liệu Lock-Free nhưng dễ dính lỗi ABA; Priority Inheritance ngăn chặn nghịch lý đảo ưu tiên trên hệ thống thời gian thực."
        ),
        new LessonMeta(
            OsChapter5Id, OsLesson52Id,
            "Bài 5.2: Linux Kernel Namespaces, cgroups v2 & Cơ chế Container Isolation Dưới Góc Nhìn OS",
            "os-linux-namespaces-cgroups-v2-container-isolation",
            2, 45,
            GetOsLesson52Markdown(),
            "Container không phải máy ảo có Kernel riêng mà là các tiến trình Linux thông thường được cô lập tầm nhìn qua Namespaces (PID, NET, MNT, UTS, IPC, USER) và OverlayFS.",
            "cgroups v2 thiết lập phân cấp đơn nhất để giới hạn CPU quota, memory.high/max và IOPS, giúp ngăn chặn hiệu ứng Noisy Neighbor trên hạ tầng đa người thuê."
        ),
        new LessonMeta(
            OsChapter5Id, OsLesson53Id,
            "Bài 5.3: Case Study Hiệu Năng Hệ Thống: eBPF, Page Faults, OOM Killer & Phân Tích Latency Thực Chiến",
            "os-case-study-ebpf-page-faults-oom-killer-latency",
            3, 45,
            GetOsLesson53Markdown(),
            "eBPF cho phép chạy bytecode an toàn ngay trong Kernel Linux qua kprobes/tracepoints với overhead cực thấp để đo độ trễ I/O, syscall và lock contention.",
            "Kết hợp phương pháp USE (Utilization, Saturation, Errors) cùng perf, vmstat, iostat và bpftrace giúp phân tích tận gốc hiện tượng tail-latency p99 và OOM Killer."
        )
    };

    private static string GetOsLesson41Markdown() => """"
# Bài 4.1: Kiến trúc Hệ thống Tệp tin: Inode, Extent, B-Tree & Journaling (ext4 / NTFS / APFS)

> [!NOTE]
> **Khái niệm then chốt:** Ổ đĩa vật lý (SSD/NVMe/HDD) dưới góc nhìn phần cứng chỉ là một mảng khổng lồ các khối logic tuyến tính (LBA — *Logical Block Addressing*, thường mỗi sector 512 bytes hoặc 4 KB). **Hệ thống tệp tin (File System)** là lớp trừu tượng cốt lõi của Hệ điều hành biến mảng byte thô sơ đó thành cây thư mục, tệp tin có tên, quyền truy cập POSIX, dấu thời gian và khả năng chịu lỗi khi mất điện đột ngột.

> [!TIP]
> **Hình ảnh trực quan:** Hãy hình dung một thư viện đại học có hàng triệu cuốn sách:
> - **Data Blocks (Khối dữ liệu):** Các trang giấy chứa nội dung thực tế nằm rải rác trên các giá sách trong kho.
> - **Inode (Index Node):** Tấm thẻ mục lục kỹ thuật của từng cuốn sách — ghi mã số duy nhất (`inode number`), kích thước, tác giả (`uid/gid`), quyền đọc/ghi (`rwxr-xr-x`) và danh sách tọa độ các trang giấy trong kho. **Lưu ý:** Thẻ Inode *không* lưu tên sách!
> - **Directory Entry (Dentry):** Cuốn sổ tra cứu ánh xạ từ *"Tên sách dễ nhớ"* $\to$ *"Mã thẻ Inode"*. Nhờ tách tên khỏi thẻ Inode, một cuốn sách vật lý có thể xuất hiện dưới nhiều tên ở nhiều kệ khác nhau (**Hard Link**)!

---

## 1. Bố cục Vật lý trên Đĩa của ext4 & Cấu trúc Inode

Trên Linux, phân vùng `ext4` chia không gian đĩa thành các **Block Groups** liền kề nhau để tận dụng tính cục bộ không gian (*Spatial Locality*), giúp giảm quãng đường tìm kiếm và tăng tốc đọc tuần tự.

```text
+---------------+---------------------------------------------------------------+
|  Boot Block   | Block Group 0   | Block Group 1   | ... | Block Group N       |
+---------------+---------------------------------------------------------------+
                        |
                        v
+------------+------------+-------------+--------------+------------+-----------+
| Superblock | Group Desc | Block Bitmap| Inode Bitmap | Inode Table| Data Blks |
+------------+------------+-------------+--------------+------------+-----------+
```

### Vai trò từng cấu trúc siêu dữ liệu (Metadata):
1. **Superblock:** Lưu thông tin sinh tử của toàn bộ hệ thống tệp: kích thước block (thường 4096 bytes), tổng số Inode, tổng số Data Blocks, magic number (`0xEF53` với ext4), trạng thái mount sạch hay lỗi (`dirty`). Do quá quan trọng, bản sao dự phòng của Superblock được sao chép sang nhiều Block Group.
2. **Block Bitmap & Inode Bitmap:** Mảng bit nén cực nhẹ (1 bit = 1 block hoặc 1 inode). Để tìm 1 khối trống mới, Kernel chỉ cần quét bitmap bằng lệnh bit CPU (`tzcnt`/`bsf`) với chi phí vài nano-giây.
3. **Inode Table:** Chứa các cấu trúc `struct ext4_inode` kích thước cố định (thường **256 bytes**). Mỗi tệp tin hoặc thư mục chiếm đúng **1 Inode** duy nhất trên phân vùng.

### Cạm bẫy vận hành thực tế: Cạn kiệt Inode khi đĩa vẫn còn trống GB!
Một hệ thống lưu hàng triệu tệp log/cache siêu nhỏ (ví dụ 500 bytes/file) nhưng mỗi tệp vẫn chiếm tối thiểu **1 Inode** và **1 Block 4 KB**. Khi `Inode Bitmap` đầy 100%, hệ điều hành sẽ báo lỗi `No space left on device` (`ENOSPC`) dù dung lượng trống còn tới hàng trăm Gigabyte!

```bash
# Kiểm tra dung lượng khối (Block) vs số lượng Inode thực tế trên Linux
$ df -h /var/lib/studxy
Filesystem      Size  Used Avail Use% Mounted on
/dev/nvme0n1p2  200G   84G  116G  42% /var/lib/studxy

$ df -i /var/lib/studxy
Filesystem       Inodes    IUsed    IFree IUse% Mounted on
/dev/nvme0n1p2 13107200 13107198        2  100% /var/lib/studxy  # HẾT SẠCH INODE!
```

---

## 2. Từ Direct/Indirect Pointers (ext2/3) tới Extent Tree (ext4) & B-Tree (NTFS / APFS)

Làm thế nào một Inode 256 bytes có thể quản lý cả tệp cấu hình 200 bytes lẫn tệp video hoặc database snapshot 4 Terabytes?

### 2.1. Mô hình Con trỏ Đa tầng Cổ điển (ext2 / ext3)
Inode truyền thống có 15 ô trỏ khối (`i_block[0..14]`):
- **12 Direct Pointers (`0..11`):** Trỏ thẳng tới 12 Data Blocks đầu tiên ($12 \times 4\text{ KB} = 48\text{ KB}$). Đọc tệp nhỏ chỉ cần đúng 1 lần đọc Inode.
- **1 Single Indirect (`12`):** Trỏ tới 1 block chứa $4096 / 4 = 1024$ con trỏ (quản lý thêm $4\text{ MB}$).
- **1 Double Indirect (`13`):** Cây 2 tầng quản lý $1024^2 \times 4\text{ KB} = 4\text{ GB}$.
- **1 Triple Indirect (`14`):** Cây 3 tầng quản lý $1024^3 \times 4\text{ KB} = 4\text{ TB}$.
- **Nhược điểm chí mạng:** Với một tệp 100 MB (25,600 blocks), ext3 phải lưu riêng lẻ từng số hiệu block một — cực kỳ tốn metadata và chậm khi xóa tệp (`truncate/unlink`).

### 2.2. Cuộc cách mạng Extent Tree (ext4, XFS, NTFS MFT Runs, APFS)
Thay vì liệt kê `[block 100, 101, 102, ..., 5099]`, **Extent** lưu một bộ ba cô đọng:
$$\text{Extent} = (\text{Logical File Offset},\; \text{Length in Blocks},\; \text{Physical Start Block})$$
Một cấu trúc `ext4_extent` chỉ tốn **12 bytes** nhưng biểu diễn được tới **128 MB** dữ liệu liên tục ($32,768 \times 4\text{ KB}$)! Nếu tệp bị phân mảnh thành nhiều đoạn, các Extent được tổ chức trong một cây **B+Tree nông** (độ sâu tối đa 4-5 tầng) ngay bên trong Inode.

| Hệ thống tệp | Cấu trúc định danh tệp | Cách quản lý khối dữ liệu lớn | Cấu trúc thư mục lớn |
|---|---|---|---|
| **Linux ext4** | Inode (256B) | Extent Tree (B+Tree 4-5 tầng) | HTree (Hashed B-Tree) |
| **Windows NTFS** | MFT Record (1 KB trong `$MFT`) | Data Runs (cặp Offset + Length) | B-Tree trong `$INDEX_ROOT` / `$INDEX_ALLOCATION` |
| **Apple APFS** | Inode Record trong Object Map | B-Tree Extent Records + CoW | B-Tree phân cấp hỗ trợ Clone tức thời |

---

## 3. Hard Link vs Symbolic Link (Soft Link) & Vòng đời Xóa Tệp

Hiểu rõ mối quan hệ giữa `Directory Entry` và `Inode` giải thích chính xác hai loại liên kết trên POSIX:

```text
HARD LINK (ln data.db backup.db)           SYMBOLIC LINK (ln -s data.db shortcut.db)
+---------------+                          +---------------+
| Dentry        |                          | Dentry        |
| "data.db"     |---\                      | "shortcut.db" |---> [Inode #905: type=LNK]
+---------------+    \                     +---------------+     chứa chuỗi "/path/data.db"
                      +--> [Inode #402]                                 |
+---------------+    /     links_count = 2                              v (phân giải theo đường dẫn)
| Dentry        |---/      Data Blocks...  +---------------+     [Inode #402: "data.db"]
| "backup.db"   |                          +---------------+
+---------------+
```

> [!IMPORTANT]
> **Tại sao xóa file log đang mở (`rm app.log`) mà dung lượng đĩa KHÔNG giảm?**
> Khi gọi `unlink("app.log")`, OS chỉ xóa tên tệp khỏi thư mục và giảm `i_links_count` về 0. Tuy nhiên, Inode chỉ thực sự giải phóng Data Blocks khi **CẢ HAI điều kiện đều bằng 0**: `i_links_count == 0` VÀ `open_fd_count == 0` (không còn tiến trình nào giữ File Descriptor trỏ vào tệp đó). Vì vậy, xóa file log đang bị Nginx/Java giữ FD sẽ làm dung lượng đĩa vẫn bị chiếm dụng cho tới khi tiến trình đóng FD hoặc restart!

```bash
# Tìm các tệp đã bị xóa tên (deleted) nhưng vẫn đang bị tiến trình giữ FD chiếm đĩa
$ lsof +L1 | head -n 5
COMMAND   PID USER   FD   TYPE DEVICE   SIZE/OFF NLINK NODE NAME
java     2184 app     4w   REG  259,2 8589934592     0 8194 /var/log/app.log (deleted)
```

---

## 4. VFS (Virtual File System) & Journaling (Crash Consistency)

Nhân Linux không ép mọi ứng dụng phải viết code riêng cho `ext4`, `xfs`, hay `nfs`. Lớp **VFS** định nghĩa 4 đối tượng trừu tượng chung (`super_block`, `inode`, `dentry`, `file`) để mọi lời gọi hệ thống `open()`, `read()`, `write()` hoạt động đồng nhất.

### Vấn đề Crash Consistency (Mất điện giữa chừng khi ghi tệp)
Khi bạn nối thêm 4 KB dữ liệu vào cuối tệp, hệ điều hành phải thực hiện ít nhất **3 thao tác ghi đĩa riêng biệt**:
1. Cập nhật **Block Bitmap** (đánh dấu block #500 đã được dùng).
2. Cập nhật **Inode** (tăng `i_size` thêm 4096 bytes và trỏ tới block #500).
3. Ghi **Data Block #500** chứa nội dung người dùng.

Nếu mất điện sau bước (2) nhưng trước bước (3), Inode sẽ trỏ tới một khối rác cũ (rò rỉ dữ liệu nhạy cảm của người khác hoặc tệp bị hỏng). Nếu mất điện sau bước (1) nhưng trước bước (2), khối #500 bị rò rỉ vĩnh viễn (**Space Leak**).

### Giải pháp Journaling (Write-Ahead Logging — WAL):
Trước khi sửa đổi cấu trúc thật trên đĩa, hệ thống tệp ghi trước nhật ký giao dịch vào một vùng vòng tròn chuyên biệt gọi là **Journal** (`JBD2` trong ext4):

```mermaid
flowchart LR
    A["1. TxB + Metadata/Data"] -->|"Ghi tuan tu vao Journal"| B["2. TxE (Commit Block)"]
    B -->|"fsync / flush thanh cong"| C["3. Checkpoint (Ghi vao vi tri that)"]
    C -->|"Hoan tat"| D["4. Giai phong giao dich trong Journal"]
```

Ba chế độ Journaling của `ext4`:
1. **`journal` (Data + Metadata):** Ghi cả dữ liệu lẫn metadata vào Journal trước $\to$ An toàn tối đa nhưng tốc độ ghi bị giảm một nửa (*Write Amplification = 2x*).
2. **`ordered` (Mặc định của ext4):** Chỉ ghi Metadata vào Journal, nhưng **bắt buộc ghi xong Data Block ra đĩa trước** rồi mới ghi Commit Block (`TxE`) vào Journal. Đảm bảo không bao giờ Inode trỏ tới dữ liệu rác.
3. **`writeback`:** Chỉ ghi Metadata vào Journal, không ép thứ tự ghi Data Block $\to$ Nhanh nhất nhưng có rủi ro dữ liệu rác nếu sập nguồn đúng lúc ghi.

---

## 5. Thực hành Phân tích Inode & Extent trên Linux (`stat`, `debugfs`)

```bash
# Xem thông tin Inode, số khối chiếm dụng (Blocks) và Link count
$ stat /var/lib/studxy/data.db
  File: /var/lib/studxy/data.db
  Size: 104857600       Blocks: 204808     IO Block: 4096   regular file
Device: 259,2   Inode: 1048592     Links: 1
Access: (0640/-rw-r-----)  Uid: ( 1000/  studxy)   Gid: ( 1000/  studxy)

# Kiểm tra phân mảnh Extent của tệp trên ext4
$ filefrag -v /var/lib/studxy/data.db
 ext:     logical_offset:        physical_offset: length:   expected: flags:
   0:        0..   16383:    1245184..   1261567:  16384:             
   1:    16384..   25599:    1290240..   1299455:   9216:    1261568: last,eof
```
- Tệp 100 MB (25,600 blocks) chỉ cần đúng **2 Extents** thay vì 25,600 con trỏ riêng lẻ!

---

## 6. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

1. **Tính dung lượng tối đa của Inode cổ điển:** Nếu kích thước block là 4 KB, địa chỉ block là 4 bytes (32-bit), một Inode có 12 con trỏ trực tiếp, 1 gián tiếp cấp 1, 1 gián tiếp cấp 2. Tổng dung lượng lớn nhất biểu diễn được khi chưa dùng cấp 3 là bao nhiêu?
   > **Đáp án:** $12 \times 4\text{ KB} + 1024 \times 4\text{ KB} + 1024^2 \times 4\text{ KB} = 48\text{ KB} + 4\text{ MB} + 4\text{ GB} \approx 4.004\text{ GB}$.
2. **Tại sao Hard Link không thể tạo xuyên qua hai phân vùng (Filesystem) khác nhau?**
   > **Đáp án:** Vì Hard Link chỉ lưu cặp `(Tên tệp, Số Inode)`. Số hiệu Inode chỉ có tính duy nhất bên trong một phân vùng duy nhất (`Device ID`), không mang thông tin phân vùng đích. Muốn liên kết xuyên phân vùng bắt buộc phải dùng Symbolic Link (lưu đường dẫn văn bản).
3. **Sự khác biệt lớn nhất về tính năng giữa `ordered` và `writeback` trong ext4 là gì?**
   > **Đáp án:** `ordered` ép buộc Data Blocks phải được flush xuống đĩa *trước khi* Metadata Commit được ghi vào Journal, nhờ đó sau crash không bao giờ có tình trạng con trỏ tệp trỏ vào các khối rác chưa kịp ghi.

<!-- studxy-os-v4 -->
<!-- studxy-os-v5 -->
"""";

    private static string GetOsLesson42Markdown() => """"
# Bài 4.2: Hệ thống I/O, Page Cache, Direct I/O & Thuật toán Lập lịch Đĩa (NVMe / SSD / Queue)

> [!NOTE]
> **Khái niệm then chốt:** Tốc độ truy cập RAM chỉ mất vài chục nano-giây ($\sim 80\text{ ns}$), trong khi đọc một trang 4 KB từ ổ SSD NVMe mất khoảng $20 - 80\,\mu\text{s}$ (chậm hơn gần 1,000 lần) và ổ HDD cơ học mất tới $5 - 10\text{ ms}$ (chậm hơn 100,000 lần!). Để lấp đầy hố sâu tốc độ này, Hệ điều hành xây dựng một tầng đệm khổng lồ trong RAM gọi là **Page Cache**, kết hợp với cơ chế **DMA** và kiến trúc hàng đợi đa luồng của **NVMe**.

> [!TIP]
> **Hình ảnh trực quan:** Nếu ổ SSD là nhà kho ở ngoại thành, thì **Page Cache** chính là quầy hàng bày sẵn ngay trong văn phòng của bạn:
> - **Buffered Read:** Khi bạn cần đọc một tài liệu, nhân viên giao hàng (Kernel) kiểm tra trên quầy (Page Cache) trước. Nếu có sẵn (**Cache Hit**), bạn nhận ngay lập tức ở tốc độ RAM mà không cần chạy xe ra kho ngoại thành. Ngoài ra, Kernel còn đoán trước bạn sẽ đọc trang kế tiếp để lấy sẵn (**Read-ahead**).
> - **Buffered Write (Write-back):** Khi bạn ghi chú thay đổi tài liệu, bạn sửa nhanh trên quầy rồi báo *"Đã lưu!"* (`write()` trả về ngay), sau đó xe tải chạy ngầm định kỳ chở các bản sửa (**Dirty Pages**) ra kho ngoại thành sau!

---

## 1. Luồng Thực thi I/O từ User Space xuống Phần cứng

Khi một ứng dụng gọi hàm `read()` hoặc `write()` theo mặc định (Buffered I/O), dữ liệu đi qua 5 lớp phần mềm và phần cứng:

```text
User Space:     [ Application Buffer (malloc/byte[]) ]
                       |   copy_from_user() / copy_to_user()
Kernel Space:   [ VFS & File System (ext4 / xfs) ]
                       |
                [ Linux Page Cache (Radix Tree / XArray 4KB Pages) ]
                       |   (Dirty Page Writeback / Read-ahead)
                [ Block Layer & Multi-Queue (blk-mq: Software/Hardware Queues) ]
                       |   (I/O Scheduler: none / mq-deadline / kyber / bfq)
                [ Device Driver (NVMe / AHCI) + DMA Engine ]
                       |   (PCIe Bus — Direct Memory Access, khong qua CPU)
Hardware:       [ NVMe SSD Controller -> Flash NAND Channels ]
```

### Cơ chế Interrupt vs Polling & DMA:
- **DMA (Direct Memory Access):** Thay vì bắt CPU phải copy từng byte giữa thanh ghi thiết bị và RAM (Programmed I/O), Kernel chỉ lập trình cho **DMA Controller** biết địa chỉ vật lý RAM nguồn/đích và số byte cần chuyển. Trong lúc DMA truyền hàng megabyte dữ liệu từ SSD vào RAM, CPU hoàn toàn rảnh rỗi để chạy tiến trình khác!
- **Interrupt vs Hybrid Polling:** Sau khi xong việc, NVMe phát ngắt **MSI-X Interrupt** đánh thức CPU. Tuy nhiên trên các ổ NVMe siêu nhanh (độ trễ dưới $10\,\mu\text{s}$), chi phí chuyển ngữ cảnh vì ngắt phần cứng lại trở nên đắt đỏ — Linux cho phép bật **I/O Polling** (`io_uring` với `IORING_SETUP_IOPOLL`) để kiểm tra trực tiếp Completion Queue với độ trễ cực tiểu.

---

## 2. Linux Page Cache, Dirty Pages & Cạm bẫy `fsync()`

Toàn bộ RAM nhàn rỗi trên Linux được tận dụng tối đa làm **Page Cache** (hiện ở cột `buff/cache` trong lệnh `free -h`). Linux phân loại các trang trong bộ nhớ thành hai nhóm chính:
1. **Anonymous Pages:** Heap, Stack của tiến trình (không gắn với tệp nào trên đĩa).
2. **Page Cache Pages (File-backed):** Bản sao các khối tệp tin đang đọc/ghi (Clean pages có thể thu hồi tức thì nếu app cần RAM; **Dirty pages** phải được xả xuống đĩa trước).

### Tham số điều khiển Writeback trong `/proc/sys/vm/`:
- `vm.dirty_background_ratio` (mặc định ~10%): Khi lượng Dirty Pages vượt 10% RAM khả dụng, các luồng nền `kworker/flush` âm thầm đẩy dữ liệu xuống đĩa mà không chặn ứng dụng.
- `vm.dirty_ratio` (mặc định ~20%): Nếu ứng dụng ghi quá nhanh khiến Dirty Pages chạm ngưỡng 20%, Kernel **chặn đứng tiến trình đang gọi `write()`** (Sync Throttle) ép nó tự chờ xả xong đĩa mới được chạy tiếp $\to$ nguyên nhân phổ biến gây **Latency Spike (đơ ứng dụng đột ngột)**!

### Buffered I/O vs Direct I/O (`O_DIRECT`) vs Memory-Mapped (`mmap`):

| Chế độ I/O | Đường đi dữ liệu | Ưu điểm | Nhược điểm & Khi nào dùng |
|---|---|---|---|
| **Buffered I/O (Mặc định)** | Disk $\leftrightarrow$ Page Cache $\leftrightarrow$ User Buffer | Tận dụng RAM dư để cache; gom các lượt ghi nhỏ thành batch lớn; Read-ahead tự động | Tốn 1 lần copy CPU giữa Kernel và User; rủi ro **Double Buffering** nếu app tự có cache riêng |
| **Direct I/O (`O_DIRECT`)** | Disk $\leftrightarrow$ User Buffer (qua DMA) | Không tốn RAM Page Cache; tránh Double Buffering; độ trễ dự đoán được | Yêu cầu buffer căn lề (alignment 512B/4KB); phải tự quản lý cache. **Chuẩn của PostgreSQL/MySQL InnoDB/RocksDB** |
| **Memory-Mapped (`mmap`)** | Ánh xạ thẳng Page Cache vào không gian ảo tiến trình | **Zero-copy** (không tốn `copy_to_user`), truy cập tệp như con trỏ mảng RAM | Lỗi ghi/đọc đĩa gây Page Fault bất ngờ (`SIGBUS` nếu file bị truncate); khó kiểm soát thứ tự flush |

> [!WARNING]
> **Bẫy Độ Bền Dữ Liệu (Durability Trap):** Lệnh `write()` thành công *chỉ có nghĩa là dữ liệu đã chép vào RAM Page Cache*. Nếu máy chủ mất điện trước khi tiến trình writeback chạy, dữ liệu sẽ mất sạch! Các cơ sở dữ liệu như SQLite (chế độ WAL) hoặc PostgreSQL bắt buộc phải gọi **`fsync(fd)`** (ép xả cả Data lẫn Metadata Inode xuống bộ nhớ không bay hơi của ổ đĩa) sau mỗi giao dịch Commit.

---

## 3. Từ Thuật toán Lập lịch HDD (SCAN / C-SCAN) đến SSD & NVMe Multi-Queue

### Thời kỳ Ổ cứng Cơ học (HDD):
Trên HDD, chi phí lớn nhất là **Seek Time** ($4 - 9\text{ ms}$) để di chuyển đầu từ cơ học và **Rotational Latency** ($2 - 4\text{ ms}$) chờ đĩa quay đúng sector:
- **SSTF (Shortest Seek Time First):** Luôn chọn yêu cầu gần đầu từ hiện tại nhất $\to$ dễ gây **Starvation** cho các track ở rìa ngoài.
- **SCAN (Elevator Algorithm) & C-SCAN (Circular SCAN):** Đầu từ quét đều từ đầu đến cuối đĩa như thang máy. `C-SCAN` chỉ phục vụ theo một chiều rồi quay nhanh về đầu đĩa, giúp thời gian chờ đồng đều hơn hẳn.

### Thời kỳ NVMe SSD & `blk-mq` (Multi-Queue Block Layer):
Ổ SSD NAND Flash không có đầu từ cơ học; đọc ngẫu nhiên có chi phí gần ngang đọc tuần tự, nhưng SSD cực kỳ nhạy cảm với **Write Amplification** (phải xóa nguyên Erase Block 4-8 MB trước khi ghi lại trang Flash 4-16 KB, được điều phối bởi FTL — *Flash Translation Layer* và lệnh `TRIM` / `fstrim`).

Giao thức **SATA/AHCI** cũ chỉ có **1 hàng đợi duy nhất tối đa 32 lệnh** và dùng khóa spinlock toàn cục gây nghẽn cổ chai trên CPU đa nhân. Giao thức **NVMe (Non-Volatile Memory Express)** chạy thẳng qua bus PCIe hỗ trợ:
- Tối đa **64,000 hàng đợi song song**, mỗi hàng đợi chứa **64,000 lệnh**!
- Mỗi lõi CPU (Core) sở hữu **Submission Queue** và **Completion Queue** riêng biệt trong `blk-mq`, **loại bỏ hoàn toàn tranh chấp khóa (Lockless per-core I/O)**!

```text
CPU Core 0 ---> [ blk-mq SW Queue 0 ] ---> [ NVMe HW Queue 0 ] ---\
CPU Core 1 ---> [ blk-mq SW Queue 1 ] ---> [ NVMe HW Queue 1 ] ----+--> [ NVMe Controller ]
CPU Core N ---> [ blk-mq SW Queue N ] ---> [ NVMe HW Queue N ] ---/     (64K Queues, PCIe)
```

---

## 4. `io_uring` & Công cụ Giám sát I/O Thực Chiến (`iostat`, `iotop`, `strace`)

Trên Linux hiện đại (5.1+), **`io_uring`** giải quyết bài toán chi phí cao của syscall truyền thống bằng hai vòng đệm chia sẻ (**Submission Ring** và **Completion Ring**) giữa User Space và Kernel Space, cho phép gửi hàng ngàn yêu cầu I/O bất đồng bộ với **0 syscall** khi chạy ở chế độ `SQPOLL`.

### Đọc chỉ số nghẽn đĩa qua `iostat -xz 1`:
```bash
$ iostat -xz 1
Device            r/s     w/s     rkB/s     wkB/s   await  aqu-sz  %util
nvme0n1        4200.0  1850.0  67200.0   29600.0    0.45    2.72   38.50
sda              95.0   110.0   3800.0   12400.0   48.20    9.88   99.40
```
- **`await` (ms):** Tổng thời gian trung bình một lệnh I/O phải chờ (cả xếp hàng + phục vụ). NVMe chỉ $0.45\text{ ms}$, còn HDD `sda` đang vọt lên $48.2\text{ ms}$!
- **`aqu-sz` (Average Queue Size):** Số lệnh trung bình đang chờ trong hàng đợi.
- **`%util`:** Phần trăm thời gian thiết bị bận. Với thiết bị đa kênh như NVMe, `%util = 100%` chưa hẳn đã hết công suất vì ổ xử lý song song nhiều chip NAND; chỉ số quyết định độ nghẽn thực sự là **`await`** và **`aqu-sz`** tăng vọt.

---

## 5. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

1. **Tại sao các hệ quản trị CSDL như PostgreSQL hay RocksDB thường ưu tiên `O_DIRECT` hoặc quản lý buffer riêng thay vì tin cậy hoàn toàn vào Linux Page Cache?**
   > **Đáp án:** Vì nếu dùng Buffered I/O thông thường, dữ liệu vừa nằm trong Shared Buffers của Database vừa bị nhân bản thêm một bản trong Linux Page Cache (**Double Buffering**, lãng phí 50% RAM). Ngoài ra, OS dùng chính sách LRU chung không hiểu ngữ nghĩa truy vấn CSDL (ví dụ quét bảng lớn `Sequential Scan` dễ đẩy hết các index nóng khỏi Page Cache), và cơ chế writeback nền có thể gây sụt giảm hiệu năng đột ngột khi chạm `vm.dirty_ratio`.
2. **Nếu gọi `write()` vào tệp mới tạo xong mà quên `fsync()` trước khi mất điện, hai lỗi nào có thể xảy ra?**
   > **Đáp án:** Thứ nhất, dữ liệu mới nằm trong Dirty Page Cache sẽ mất hoàn toàn chưa kịp xuống đĩa. Thứ hai, ngay cả khi dữ liệu đã xuống đĩa, nếu metadata thư mục cha chưa được `fsync()`, tệp mới thậm chí không xuất hiện trong cây thư mục sau khi khởi động lại!
3. **Giải thích cách kiến trúc `blk-mq` kết hợp NVMe khắc phục điểm nghẽn đa nhân của AHCI.**
   > **Đáp án:** `blk-mq` cấp phát Software Queue riêng cho từng lõi CPU (per-core) ánh xạ trực tiếp xuống các Hardware Queue độc lập của bộ điều khiển NVMe (hỗ trợ tối đa 64K queues $\times$ 64K commands), nhờ đó mỗi nhân CPU gửi lệnh I/O mà không phải tranh giành khóa spinlock toàn cục như hàng đợi đơn 32-slot của AHCI.

<!-- studxy-os-v4 -->
<!-- studxy-os-v5 -->
"""";

    private static string GetOsLesson43Markdown() => """"
# Bài 4.3: RAID, Copy-on-Write (ZFS / Btrfs), Snapshots & Độ Bền Dữ Liệu (Crash Consistency)

> [!NOTE]
> **Khái niệm then chốt:** Một ổ cứng đơn lẻ luôn có xác suất hỏng hóc vật lý hoặc lỗi âm thầm từng bit dữ liệu (*Silent Data Corruption / Bit Rot*). Để đạt được cả thông lượng cao lẫn độ tin cậy doanh nghiệp, Hệ điều hành và tầng lưu trữ kết hợp nhiều ổ đĩa thành mảng logic (**RAID**) và áp dụng kiến trúc **Copy-on-Write (CoW)** trên các hệ thống tệp hiện đại như **ZFS** và **Btrfs** nhằm đảm bảo tính nguyên tử tuyệt đối khi ghi dữ liệu và tạo Snapshot gần như tức thì.

> [!TIP]
> **Hình ảnh trực quan:** Sự khác biệt giữa hệ thống tệp truyền thống (*In-place Update*) và **Copy-on-Write (CoW)**:
> - **Ghi đè tại chỗ (`ext4`):** Bạn cầm bút xóa gạch đè trực tiếp lên trang sách đang viết. Nếu bị cúp điện giữa lúc đang tô bút xóa, cả dòng chữ cũ lẫn dòng chữ mới đều bị lem nhem không đọc được!
> - **Copy-on-Write (`ZFS/Btrfs`):** Bạn **tuyệt đối không chạm vào trang cũ**. Bạn lấy một tờ giấy trắng mới hoàn toàn, viết toàn bộ nội dung mới lên đó, kiểm tra kỹ chữ ký (Checksum), rồi chỉ thực hiện **1 động tác duy nhất là đổi số trang mục lục (Atomic Pointer Flip)**! Nhờ trang cũ vẫn còn nguyên vẹn, bạn vừa chống hỏng dữ liệu khi mất điện, vừa giữ được các bản chụp quá khứ (**Snapshot**) mà không tốn thêm dung lượng cho phần dữ liệu không đổi!

---

## 1. Kiến trúc RAID Kinh điển & Đánh đổi Hiệu năng (Write Penalty)

**RAID (Redundant Array of Independent Disks)** ghép $N$ ổ đĩa vật lý thành một thiết bị khối ảo duy nhất thông qua ba kỹ thuật cốt lõi: **Striping** (chia dải tăng tốc độ), **Mirroring** (nhân bản đối xứng) và **Parity** (tính tổng kiểm tra bằng phép toán `XOR` để khôi phục dữ liệu khi hỏng đĩa).

```text
RAID 0 (Striping)       RAID 1 (Mirroring)       RAID 5 (Distributed Parity)
Disk0    Disk1          Disk0    Disk1           Disk0    Disk1    Disk2
[ A1 ]   [ A2 ]         [ A1 ]   [ A1 ]          [ A1 ]   [ A2 ]   [ P_A ]
[ B1 ]   [ B2 ]         [ B2 ]   [ B2 ]          [ B1 ]   [ P_B ]  [ B2  ]
                                                 [ P_C ]  [ C1 ]   [ C2  ]
```

### Bảng So Sánh Các Mức RAID Phổ Biến (với $N$ ổ, dung lượng mỗi ổ $S$):

| Mức RAID | Số đĩa tối thiểu | Dung lượng khả dụng | Khả năng chịu lỗi | Write Penalty (Số I/O ghi cho 1 lần ghi ngẫu nhiên) | Ứng dụng phù hợp |
|---|---|---|---|---|---|
| **RAID 0** | 2 | $N \times S$ | **0 đĩa** (Hỏng 1 mất hết!) | **1x** | Dữ liệu tạm tốc độ cao, render cache |
| **RAID 1** | 2 | $S$ (hoặc $N/2$ cặp) | $N - 1$ đĩa (thường 1 đĩa) | **2x** (ghi cả 2 bản) | Phân vùng Boot OS, DB nhỏ cần độ an toàn cao |
| **RAID 5** | 3 | $(N - 1) \times S$ | **1 đĩa** | **4x** (Đọc Old Data + Đọc Old Parity + Ghi New Data + Ghi New Parity) | Lưu trữ đọc nhiều ghi ít (Media, Archive) |
| **RAID 6** | 4 | $(N - 2) \times S$ | **2 đĩa bất kỳ** | **6x** (Tính 2 khối Parity $P$ và $Q$) | Mảng đĩa dung lượng lớn (> 8 TB/ổ) tránh rủi ro URE khi Rebuild |
| **RAID 10 (1+0)** | 4 | $(N / 2) \times S$ | Tối đa 1 đĩa *trong mỗi cặp Mirror* | **2x** | **Chuẩn vàng cho Database sản xuất (OLTP)** |

### Tại sao RAID 5 ngày càng nguy hiểm trên ổ dung lượng lớn (URE & Write Hole)?
1. **Rủi ro Rebuild Failure (URE — Unrecoverable Read Error):** Khi 1 ổ 16 TB trong RAID 5 chết, quá trình phục hồi (*Rebuild*) phải đọc toàn bộ 100% các sector trên các ổ còn lại. Với tỷ lệ lỗi đọc phần cứng không thể phục hồi $10^{-14}$ bit, xác suất gặp một sector lỗi trong lúc Rebuild hàng chục TB rất cao $\to$ quá trình Rebuild sụp đổ và mất toàn bộ mảng! RAID 6 hoặc RAID 10 giải quyết vấn đề này.
2. **Lỗ hổng RAID Write Hole:** Khi ghi đè dữ liệu vào một stripe RAID 5, nếu mất điện **sau khi đã ghi Data mới nhưng chưa kịp ghi Parity mới**, dữ liệu và Parity bị lệch pha nhau. Lần đọc sau khi đĩa hỏng sẽ khôi phục ra dữ liệu sai lệch hoàn toàn! Giải pháp gồm: bộ điều khiển RAID có pin dự phòng (**BBU / NVRAM**), `mdadm` Write-Intent Bitmap, hoặc chuyển hẳn sang **ZFS RAID-Z** (vì CoW luôn viết toàn bộ stripe mới nên không bao giờ sửa đè stripe cũ!).

---

## 2. Kiến trúc Copy-on-Write (CoW) trên ZFS & Btrfs

Hệ thống tệp **ZFS** và **Btrfs** hợp nhất tầng quản lý Volume (thay thế LVM/RAID truyền thống) và tầng File System vào chung một kiến trúc **Storage Pool** dạng cây Merkle Tree:

```text
                    [ Uberblock / Superblock ]  <-- Chuyen con tro nguyen tu khi Commit!
                               |
                       [ Root Tree Node ]
                      /                  \
          [ Indirect Block ]        [ Indirect Block ]
            (Checksum C1)             (Checksum C2)
             /        \                 /        \
     [ Data A ]   [ Data B ]    [ Data C ]   [ Data D' (MOI!) ]
                                                  ^
                                        (Khoi D cu van duoc giu lai neu co Snapshot)
```

### Quy trình ghi 1 khối dữ liệu `D -> D'` theo CoW:
1. Ghi nội dung mới `D'` vào một khối **trống hoàn toàn** trên đĩa.
2. Tính lại **Checksum** của `D'` và tạo nút cha mới chứa `(Địa chỉ mới, Checksum mới)` cũng ở vùng trống (hiện tượng *Wandering Trees*).
3. Lan truyền lên đến đỉnh cây và ghi bản **Uberblock** mới (sử dụng vòng luân phiên gồm nhiều khe Uberblock kèm số thứ tự `txg` — Transaction Group).
4. **Hiệu ứng kỳ diệu:** Nếu cúp điện ở bất kỳ bước nào trước khi Uberblock mới ghi xong, lần khởi động lại hệ thống chỉ cần trỏ về Uberblock hợp lệ trước đó — **không cần chạy `fsck` hàng giờ đồng hồ** mà hệ thống vẫn nhất quán 100%!

---

## 3. Snapshots, Clones & Tự Chữa Lỗi Bit Rot (Self-Healing Checksums)

### 3.1. Cơ chế Snapshot O(1) Tức Thì
Trong Copy-on-Write, tạo một bản Snapshot toàn bộ hệ thống dữ liệu 10 TB chỉ đơn giản là **đóng băng và giữ lại con trỏ Root Tree hiện tại** (tốn vài KB metadata và hoàn thành trong **< 10 mili-giây**):
- Lúc mới tạo, Snapshot dùng chung 100% các Data Blocks với hệ thống đang chạy $\to$ tốn **0 byte** dung lượng phụ thêm!
- Chỉ khi tệp bị sửa hoặc xóa trên hệ thống chính, CoW ghi dữ liệu mới vào vùng khác, còn khối cũ vẫn được giữ nguyên cho Snapshot tham chiếu.

### 3.2. Chống Silent Data Corruption (Bit Rot) bằng End-to-End Checksum
Ổ cứng giá rẻ hay dây cáp SATA lỗi đôi khi lật nhầm bit dữ liệu mà không báo lỗi I/O cho hệ điều hành (*Bit Rot*). Với các hệ thống tệp cũ hoặc RAID phần cứng thông thường, Checksum chỉ lưu tại từng sector riêng của ổ đĩa nên hệ điều hành tin mù quáng vào dữ liệu sai đó.
Trên **ZFS / Btrfs**:
- **Checksum của khối con được lưu nằm trong khối cha** (tạo thành **Merkle Tree**).
- Khi đọc dữ liệu lên, hệ điều hành đối chiếu Checksum thực tế với Checksum lưu trong khối cha. Nếu sai lệch, ZFS tự động tìm bản sao sạch trên ổ Mirror hoặc tái tạo lại từ Parity trong RAID-Z, trả dữ liệu chuẩn cho ứng dụng và **âm thầm ghi đè sửa lại sector bị hỏng trên đĩa (Self-Healing)**!

---

## 4. Cạm bẫy CoW trên Database & Các Kỹ thuật Giảm Phân Mảnh

Không có kiến trúc nào miễn phí hoàn toàn. Copy-on-Write có hai nhược điểm cần kỹ sư hệ thống xử lý khéo léo:
1. **Phân mảnh nặng khi ghi ngẫu nhiên nhỏ (Random Small Writes):** Các tệp cơ sở dữ liệu (PostgreSQL, MySQL, ảnh đĩa máy ảo QCOW2) thường xuyên cập nhật các trang 4 KB - 16 KB ở giữa tệp. Vì CoW không ghi đè tại chỗ, tệp ban đầu liên tục bị xé lẻ thành hàng trăm nghìn mảnh rời rạc.
   - *Cách xử lý:* Trên Btrfs có thể tắt CoW riêng cho thư mục database bằng `chattr +C /var/lib/postgresql` (`nodatacow`), hoặc trên ZFS tinh chỉnh `recordsize=8k`/`16k` khớp với kích thước trang của Database (`8 KB` cho PostgreSQL, `16 KB` cho InnoDB) để giảm Write Amplification!
2. **Hết dung lượng không xóa được file trên CoW (`ENOSPC`):** Khi đĩa đầy 100%, ngay cả lệnh `rm` hoặc `truncate` cũng cần tạo một transaction metadata mới trước khi giải phóng khối cũ! Do đó luôn phải giữ ít nhất **10% - 15% dung lượng trống** trên các Storage Pool chạy ZFS/Btrfs.

```bash
# Kiểm tra trạng thái Pool, lỗi Checksum và chạy Scrub (quét kiểm tra toàn đĩa) trên ZFS
$ zpool status tank
  pool: tank
 state: ONLINE
  scan: scrub repaired 0B in 00:18:42 with 0 errors on Sun Nov 22 02:18:45 2026
config:
        NAME        STATE     READ WRITE CKSUM
        tank        ONLINE       0     0     0
          raidz1-0  ONLINE       0     0     0
            nvme1n1 ONLINE       0     0     0
            nvme2n1 ONLINE       0     0     0
            nvme3n1 ONLINE       0     0     0

# Tạo Snapshot ZFS tức thì trước khi nâng cấp Schema Database
$ zfs snapshot tank/pgdata@pre-migration-2026
```

---

## 5. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

1. **Giải thích tại sao một lệnh ghi ngẫu nhiên 4 KB trên RAID 5 truyền thống lại tốn tới 4 thao tác I/O vật lý (Write Penalty = 4)?**
   > **Đáp án:** Để cập nhật 1 khối dữ liệu $D_{\text{old}} \to D_{\text{new}}$ mà không đọc toàn bộ stripe, mảng đĩa phải: (1) Đọc $D_{\text{old}}$, (2) Đọc $P_{\text{old}}$, tính lại $P_{\text{new}} = P_{\text{old}} \oplus D_{\text{old}} \oplus D_{\text{new}}$, sau đó (3) Ghi $D_{\text{new}}$ và (4) Ghi $P_{\text{new}}$. Tổng cộng 2 Read + 2 Write = 4 I/O.
2. **Tại sao ZFS/Btrfs lưu Checksum của khối dữ liệu ở trong khối cha (Metadata) thay vì lưu ở ngay đuôi của chính khối dữ liệu đó?**
   > **Đáp án:** Nếu lưu Checksum ngay cạnh dữ liệu trong cùng block, lỗi ghi lạc chỗ (*Misdirected Write* — ổ đĩa ghi nhầm dữ liệu + checksum hợp lệ của block A vào vị trí block B) hoặc lỗi ghi đứt quãng (*Phantom Write*) sẽ vẫn qua mặt được bộ kiểm tra! Khi lưu Checksum ở khối cha, mọi sai lệch địa chỉ hoặc nội dung đều bị phát hiện ngay lập tức.
3. **Tại sao một hệ thống ZFS vừa thực hiện `rm` xóa một file 50 GB nhưng dung lượng trống vẫn không tăng lên?**
   > **Đáp án:** Rất có thể các khối dữ liệu của file 50 GB đó đang được tham chiếu bởi một **Snapshot** trước đó (hoặc một Clone/Process đang mở FD). Vì Snapshot vẫn cần nhìn thấy file đó ở thời điểm chụp, các Data Blocks chưa thể giải phóng cho tới khi Snapshot bị xóa (`zfs destroy`).

<!-- studxy-os-v4 -->
<!-- studxy-os-v5 -->
"""";

    private static string GetOsLesson51Markdown() => """"
# Bài 5.1: Locks, Spinlock vs Mutex, Lock-Free Atomic CAS & Phòng Chống Priority Inversion

> [!NOTE]
> **Khái niệm then chốt:** Ở Chương 2, ta đã biết dùng Mutex và Semaphore để bảo vệ vùng tranh chấp (*Critical Section*). Nhưng ở tầng Kernel và hệ thống hiệu năng cao (High-Frequency Trading, Game Engine, Database Engine), **cách cài đặt khóa** quyết định trực tiếp độ trễ và khả năng mở rộng đa nhân. Khi một luồng không lấy được khóa: nó nên **quay vòng đốt CPU chờ** (*Spinlock*), **nhường CPU đi ngủ** (*Mutex / Futex*), hay **không dùng khóa nào cả** (*Lock-Free Atomic CAS*)?

> [!TIP]
> **Hình ảnh trực quan:** Ba chiến lược chờ đợi trước phòng họp đang có người sử dụng:
> - **Spinlock (Khóa xoay):** Bạn đứng ngay trước cửa, tay đặt sẵn trên tay nắm và gõ cửa liên tục mỗi mili-giây: *"Xong chưa? Xong chưa?"*. Nếu người bên trong chỉ họp đúng **5 giây**, cách này cực nhanh vì bạn bước vào ngay không tốn thời gian đi thang máy về chỗ ngồi. Nhưng nếu họ họp **30 phút**, bạn vừa mệt lả (tốn 100% CPU) vừa chẳng làm được việc gì khác!
> - **Mutex / Futex (Khóa ngủ):** Bạn ghi tên vào danh sách chờ rồi đi về bàn ngủ; khi người kia họp xong sẽ gọi điện đánh thức bạn dậy (Context Switch).
> - **Lock-Free CAS:** Không ai khóa cửa phòng cả — ai vào sửa bảng trắng đều chụp ảnh bảng cũ trước, tính giá trị mới, rồi chỉ ghi lên bảng nếu bức ảnh cũ trên bảng vẫn chưa bị ai sửa đổi!

---

## 1. Spinlock vs Mutex (và Cơ chế Hybrid Futex trên Linux)

Mọi cơ chế khóa ở tầng phần mềm đều phải dựa trên sự hỗ trợ nguyên tử của phần cứng CPU (như tiền tố `LOCK` trên x86-64 hoặc `LDXR/STXR` trên ARM) để đảm bảo thao tác **Read-Modify-Write** không bị hai lõi CPU xen ngang.

### 1.1. Spinlock (Busy-Waiting Lock)
Spinlock dùng vòng lặp kiểm tra liên tục trên biến nguyên tử (`Test-And-Set` hoặc `Ticket Lock` / `MCS Lock` để đảm bảo công bằng FIFO và tránh nghẽn Cache Coherence):
- **Ưu điểm:** Không tốn chi phí System Call hay Context Switch ($\sim 0\text{ ns}$ wakeup overhead nếu chờ ngắn).
- **Cạm bẫy chết người của Spinlock:**
  1. **Trên hệ thống 1 lõi CPU (Single-core):** Tuyệt đối CẤM dùng Spinlock thuần túy không tắt ngắt (*preemption*)! Nếu Thread A giữ Spinlock rồi bị Timer Interrupt cướp CPU nhường cho Thread B có độ ưu tiên cao hơn, Thread B sẽ quay vòng vĩnh viễn chiếm 100% CPU, khiến Thread A không bao giờ được chạy tiếp để nhả khóa (**Deadlock tức thì**)!
  2. **Cache Line Bouncing (MESI Protocol Storm):** Khi 32 lõi CPU cùng quay vòng gọi lệnh ghi `LOCK XCHG` trên cùng một biến khóa, giao thức đồng bộ Cache L1/L2 (MESI) phải liên tục vô hiệu hóa Cache Line giữa các lõi qua bus kết nối, làm sập băng thông bộ nhớ! Kỹ thuật **Test-and-Test-and-Set (TTAS)** đọc bằng lệnh `LOAD` thông thường (trên Cache L1 nội bộ) trước, chỉ gọi lệnh `LOCK` nguyên tử khi thấy khóa vừa chuyển về 0.

### 1.2. Mutex & Futex (Fast Userspace Mutex) trên Linux
Trên Linux, `pthread_mutex` và `std::mutex` không gọi System Call vào Kernel mỗi lần khóa! Chúng dùng cơ chế **Futex (`futex(2)`)** kết hợp thông minh cả hai thế giới:
- **Trường hợp không tranh chấp (Uncontended — chiếm >95% thực tế):** Thực hiện hoàn toàn ở **User Space** bằng 1 lệnh Atomic CAS duy nhất ($\sim 5 - 15\text{ ns}$), **0 syscall**!
- **Trường hợp có tranh chấp (Contended):** Có thể quay vòng ngắn (*Adaptive Spinning* vài chục nano-giây phòng trường hợp chủ khóa sắp xong), sau đó mới gọi syscall `futex(FUTEX_WAIT)` để Kernel đưa luồng vào hàng đợi ngủ và chuyển CPU cho luồng khác.

| Tiêu chí | Spinlock | Mutex / Futex (Sleeping Lock) |
|---|---|---|
| **Hành vi khi chờ khóa** | Quay vòng đốt 100% CPU (Busy-wait) | Gọi Kernel đưa luồng sang trạng thái Blocked/Sleep |
| **Chi phí chuyển ngữ cảnh** | **0** (nếu giữ khóa ngắn) | Cao ($\sim 1 - 5\,\mu\text{s}$ cho 2 lần Context Switch) |
| **Thời gian giữ khóa phù hợp** | Cực ngắn ($< 100\text{ ns}$, vài dòng lệnh không có I/O) | Trung bình đến dài, hoặc bên trong vùng tranh chấp **có I/O / Sleep** |
| **Dùng trong Interrupt Handler?** | **Bắt buộc** (Interrupt Handler không được phép Sleep!) | **Tuyệt đối CẤM** (Sleep trong Interrupt sẽ gây Kernel Panic) |

---

## 2. Lập trình Lock-Free với Atomic CAS & Hiểm họa ABA Problem

### 2.1. Lệnh nguyên tử Compare-And-Swap (`CAS`)
Phần cứng x86-64 cung cấp lệnh **`LOCK CMPXCHG`** (so sánh và tráo đổi nguyên tử):
```text
bool CAS(T* addr, T expected, T new_val):   // Thuc thi NGUYEN TU boi phan cung CPU!
    if (*addr == expected):
        *addr = new_val
        return true
    return false
```

Với `CAS`, ta có thể viết hàm tăng bộ đếm hoặc đẩy phần tử vào Stack/Queue **không cần bất kỳ Mutex nào (Lock-Free)**:
```c
// Lock-Free Atomic Stack Push (C11 / C++20 style)
void push(Stack* s, Node* new_node) {
    Node* old_head = atomic_load(&s->head);
    do {
        new_node->next = old_head;
    } while (!atomic_compare_exchange_weak(&s->head, &old_head, new_node));
}
```
Tính chất **Lock-Free** đảm bảo một nguyên lý mạnh mẽ: Ngay cả khi 1 luồng bị OS tạm dừng (*preempted*) ngay giữa Critical Section, các luồng khác **vẫn luôn tiến triển được** (System-wide progress guaranteed) — không bao giờ xảy ra Deadlock hay Convoy Effect!

### 2.2. Hiểm họa Kinh điển: ABA Problem & Memory Ordering
Xét một Stack đang chứa `Head -> A -> B -> C`:
1. **Thread 1** muốn `pop()`: đọc `old_head = A`, `next = B`, rồi bị OS ngắt ngay trước lệnh `CAS(&head, A, B)`.
2. **Thread 2** chạy chen vào: `pop()` lấy **A** ra, `pop()` lấy **B** ra (giải phóng bộ nhớ B), rồi lại đẩy một nút mới vô tình nằm đúng tại địa chỉ cũ **A** lên đầu Stack (`Head -> A -> C`)!
3. **Thread 1** tỉnh dậy chạy `CAS(&head, A, B)`: Phần cứng thấy `*head == A` vẫn ĐÚNG! Nó liền gán `head = B` — trỏ thẳng vào **vùng nhớ B đã bị giải phóng (Use-After-Free / Hỏng cấu trúc Stack)**!

**Cách tiêu diệt ABA Problem:**
- **Tagged Pointer (Version Counter):** Gói cặp `(Pointer, 16-bit/64-bit Version Counter)` dùng lệnh `CMPXCHG16B` (Double-Width CAS 128-bit). Mỗi lần sửa đổi, `Version` tăng lên 1 nên `(A, v=1) != (A, v=3)`.
- **Hazard Pointers / Epoch-Based Reclamation (EBR):** Trì hoãn việc `free()` nút B cho tới khi chắc chắn không còn luồng nào giữ tham chiếu đọc tới nút B.

---

## 3. Nghịch lý Đảo Ưu tiên (Priority Inversion) & Cách Phòng Chống

Năm 1997, tàu thám hiểm sao Hỏa **Mars Pathfinder** liên tục bị Reset hệ thống giữa nhiệm vụ khoa học. Nguyên nhân gốc rễ chính là lỗi kinh điển của Hệ điều hành: **Priority Inversion**!

```text
Mức ưu tiên: Thread HIGH > Thread MEDIUM > Thread LOW
t=0: [LOW] giữ Mutex_M
t=1: [HIGH] cần Mutex_M -> Bị BLOCK chờ [LOW] nhả Mutex_M!
t=2: [MEDIUM] xuất hiện (không cần Mutex_M) -> CƯỚP CPU của [LOW]!
     [MEDIUM] chạy suốt 10 giây -> [LOW] không được chạy -> Không thể nhả Mutex_M!
     ==> [HIGH] (quan trọng nhất!) bị treo vô thời hạn vì [MEDIUM] (kém quan trọng hơn)!
```

### Hai giải pháp chuẩn công nghiệp trên Kernel & RTOS:
1. **Priority Inheritance Protocol (PIP — Kế thừa độ ưu tiên):** Ngay khi `[HIGH]` bị chặn vì chờ khóa đang thuộc về `[LOW]`, Kernel **tạm thời nâng độ ưu tiên của `[LOW]` lên ngang bằng `[HIGH]`**! Nhờ đó `[MEDIUM]` không thể chen ngang cướp CPU của `[LOW]`. Ngay khi `[LOW]` gọi `Unlock()`, độ ưu tiên của nó tự động trả về mức cũ và `[HIGH]` chạy tiếp ngay! (Hỗ trợ trên Linux qua `PTHREAD_PRIO_INHERIT` với PI-Futex).
2. **Priority Ceiling Protocol (PCP — Trần độ ưu tiên):** Gán cho mỗi Mutex một mức trần bằng độ ưu tiên cao nhất của bất kỳ luồng nào có thể sử dụng nó; luồng đang giữ Mutex lập tức chạy ở mức trần này.

---

## 4. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

1. **Tại sao trong bộ nhớ chia sẻ đa nhân, hai luồng cùng sửa hai biến ĐỘC LẬP `countA` và `countB` nhưng vẫn chậm thê thảm như thể bị khóa?**
   > **Đáp án:** Hiện tượng **False Sharing**! Nếu `countA` và `countB` nằm cạnh nhau trong cùng một **Cache Line (64 bytes)**, mỗi lần Core 0 ghi `countA`, giao thức MESI phải vô hiệu hóa toàn bộ 64-byte Cache Line trên Core 1 khiến Core 1 bị Cache Miss liên tục khi đọc/ghi `countB`. Khắc phục bằng `alignas(64)` để tách hai biến ra hai Cache Line riêng.
2. **Sự khác biệt giữa thuật toán Lock-Free và Wait-Free là gì?**
   > **Đáp án:** **Lock-Free** đảm bảo toàn hệ thống luôn có *ít nhất một luồng* hoàn thành công việc trong số bước hữu hạn (nhưng một luồng đơn lẻ xui xẻo vẫn có thể bị Starvation do retry CAS liên tục). **Wait-Free** mạnh hơn: đảm bảo *mọi luồng* đều hoàn thành thao tác trong số bước hữu hạn bất kể lịch trình lập lịch của OS.
3. **Trong Linux, tại sao lệnh `pthread_mutex_lock()` không gọi Kernel syscall khi không có luồng nào khác đang giữ khóa?**
   > **Đáp án:** Vì `pthread_mutex` được xây dựng trên **Futex**: khi khóa đang rảnh (`val == 0`), một lệnh `CAS(0 -> 1)` thuần túy ở User Space là đủ để chiếm khóa trong vài nano-giây mà không cần chuyển sang Kernel Mode.

<!-- studxy-os-v4 -->
<!-- studxy-os-v5 -->
"""";

    private static string GetOsLesson52Markdown() => """"
# Bài 5.2: Linux Kernel Namespaces, cgroups v2 & Cơ chế Container Isolation Dưới Góc Nhìn OS

> [!NOTE]
> **Khái niệm then chốt:** Nhiều lập trình viên lầm tưởng Docker Container là một "máy ảo siêu nhẹ" chạy một hệ điều hành riêng. Dưới góc nhìn Hệ điều hành Linux, **Container hoàn toàn KHÔNG phải máy ảo (VM)** — nó chỉ là một (hoặc vài) **tiến trình Linux thông thường** chạy trực tiếp trên **cùng một nhân Kernel duy nhất của máy chủ Host**, nhưng bị giới hạn **những gì nó nhìn thấy** (*Namespaces*), **những gì nó được phép tiêu thụ** (*cgroups v2*), **cấu trúc hệ thống tệp phân tầng** (*OverlayFS*), và **những lời gọi hệ thống được phép thực thi** (*Seccomp-BPF*).

> [!TIP]
> **Hình ảnh trực quan:** So sánh Máy ảo (VM) và Container qua hình ảnh tòa nhà chung cư:
> - **Virtual Machine (Hypervisor KVM/VMware):** Mỗi căn nhà tự xây móng, tự kéo hệ thống điện nước và có hẳn quản gia riêng (Guest OS + Guest Kernel). Tốn nhiều diện tích (RAM) và khởi động chậm (hàng chục giây).
> - **Container (Docker / containerd):** Mọi căn hộ dùng chung **1 móng nhà và 1 ban quản lý tòa nhà duy nhất (Host Linux Kernel)**! Nhưng mỗi căn hộ có: tường vách ngăn cách tầm nhìn (**Namespaces** — không thấy hàng xóm đang chạy gì), đồng hồ công-tơ điện nước ngắt tự động khi vượt mức (**cgroups v2** — dùng quá RAM là OOM ngay trong căn hộ đó), và lớp sơn nội thất riêng phủ lên khung tường mẫu (**OverlayFS Copy-on-Write**).

---

## 1. Bảy Linux Kernel Namespaces (Cô lập Tầm nhìn)

Khi khởi tạo Container (qua syscall `clone()` hoặc `unshare()`), Kernel tạo ra các không gian tên tách biệt cho tiến trình con:

| Namespace | Cờ `clone()` | Tài nguyên được cô lập | Hiện tượng thực tế khi vào Container |
|---|---|---|---|
| **PID** | `CLONE_NEWPID` | Bảng định danh tiến trình (`PID`) | Bên trong Container, tiến trình app là **`PID 1`**, chạy `ps aux` chỉ thấy chính mình; nhưng từ máy Host chạy `ps`, nó có thể mang `PID 28419`! |
| **NET** | `CLONE_NEWNET` | Giao diện mạng (`eth0`, `lo`), IP, bảng định tuyến, cổng TCP/UDP, `iptables` | Hai Container cùng lắng nghe cổng `:8080` mà không bị xung đột `EADDRINUSE`, kết nối ra ngoài qua cặp dây ảo **`veth pair`** nối vào Bridge `docker0` |
| **MNT** | `CLONE_NEWNS` | Điểm gắn kết hệ thống tệp (`mount points`) kết hợp `pivot_root` | Container chỉ thấy cây thư mục `/` riêng của Image (Alpine/Ubuntu), không chạm được vào `/etc` hay `/home` của máy Host |
| **UTS** | `CLONE_NEWUTS` | Hostname và NIS domain name | Lệnh `hostname` trả về ID riêng của Container |
| **IPC** | `CLONE_NEWIPC` | POSIX Message Queues, Shared Memory (`shmget`), Semaphores | Ngăn tiến trình Container A đọc trộm vùng nhớ chia sẻ IPC của Container B |
| **USER** | `CLONE_NEWUSER` | Ánh xạ `UID / GID` giữa bên trong và bên ngoài Container (Rootless Container) | Bên trong Container là `root (UID 0)`, nhưng ngoài Host thực chất chỉ là user thường không đặc quyền `UID 100000`! |
| **CGROUP / TIME** | `CLONE_NEWCGROUP` | Ẩn cấu trúc cgroup thật của Host / Cô lập đồng hồ hệ thống `CLOCK_MONOTONIC` | Container không nhìn thấy cấu trúc tài nguyên của các Container hàng xóm |

> [!IMPORTANT]
> **Vì sao `PID 1` trong Container dễ gây tích tụ Zombie Process?**
> Trong Namespace `PID`, tiến trình khởi động đầu tiên của Container (ví dụ `node server.js` hoặc `dotnet Swms.Api.dll`) giữ vai trò `PID 1`. Nếu ứng dụng này sinh tiến trình con (như chạy `ffmpeg` hay `sh`) mà không cài đặt trình thu dọn tín hiệu `SIGCHLD` + `waitpid()`, tiến trình con chết đi sẽ biến thành **Zombie** mãi mãi vì không có `systemd` làm PID 1 dọn giúp! Giải pháp chuẩn: dùng `tini` hoặc cờ `docker run --init` làm `PID 1` siêu nhẹ.

---

## 2. cgroups v2 (Control Groups): Quản trị Tài nguyên & Chống "Noisy Neighbor"

Nếu Namespaces trả lời câu hỏi *"Tiến trình được nhìn thấy gì?"*, thì **cgroups v2** trả lời câu hỏi *"Tiến trình được phép xài tối đa bao nhiêu?"*. Kiến trúc **cgroups v2** (mặc định trên Ubuntu 22.04+, Debian 11+, Kubernetes 1.25+) khắc phục nhược điểm phân mảnh đa cây của cgroups v1 bằng một **hệ thống phân cấp cây hợp nhất (Unified Hierarchy)** tại `/sys/fs/cgroup`:

```text
/sys/fs/cgroup/
 ├── system.slice/
 └── kubepods.slice/
      ├── pod-web-api/
      │    ├── cpu.max          # "50000 100000" -> toi da 0.5 vCPU (50ms moi 100ms)
      │    ├── memory.high      # Ngưỡng mềm: ép thu hồi Page Cache & Throttle nhẹ
      │    ├── memory.max       # Ngưỡng cứng: vượt quá lập tức kích hoạt cgroup OOM Killer!
      │    ├── memory.current   # Lượng RAM hiện tại (Anon + Page Cache + Kernel slab)
      │    └── io.max           # Giới hạn MB/s (rbps/wbps) và IOPS (riops/wiops)
      └── pod-worker-ai/
```

### Cơ chế Điều tiết CPU (`CFS Bandwidth Control` — `cpu.max`):
Giả sử Kubernetes gán limit `cpu: 500m` (0.5 CPU). Docker/containerd ghi vào `cpu.max` giá trị `50000 100000` (50,000 $\mu$s quota trong mỗi chu kỳ 100,000 $\mu$s = 100 ms).
- **Cạm bẫy CPU Throttling với ứng dụng đa luồng:** Nếu một dịch vụ .NET/JVM có 8 luồng xử lý cùng thức dậy và chạy song song trên 8 nhân vật lý trong **6.25 ms**, tổng thời gian CPU tiêu thụ đã chạm trần $8 \times 6.25 = 50\text{ ms}$! Lập tức Kernel **đóng băng toàn bộ Container trong suốt 93.75 ms còn lại** của chu kỳ (`nr_throttled` tăng vọt trong `cpu.stat`) $\to$ gây ra độ trễ p99 tăng vọt dù mức CPU trung bình cả phút chỉ có 15%!

### Hai ngưỡng RAM sống còn trong cgroups v2:
- **`memory.high`:** Khi vượt ngưỡng này, tiến trình không bị giết ngay mà bị Kernel **kìm hãm tốc độ cấp phát** và ép chạy *Direct Reclaim* đẩy bớt Page Cache sạch xuống đĩa.
- **`memory.max`:** Trần tuyệt đối. Khi vượt ngưỡng này và không thu hồi thêm được RAM, **cgroup OOM Killer** chỉ tiêu diệt tiến trình bên trong cgroup đó (bảo vệ an toàn cho toàn bộ máy Host và các Container khác!).

---

## 3. OverlayFS (Union Mount) & Lớp Bảo mật Kernel (Seccomp / Capabilities)

### 3.1. Kiến trúc Hệ thống Tệp Phân tầng OverlayFS
Một Docker Image 200 MB có thể được khởi chạy thành 100 Container mà **không cần nhân bản 200 MB $\times$ 100 = 20 GB** trên đĩa nhờ **OverlayFS**:
- **`lowerdir` (Read-Only):** Các lớp Image nền (`ubuntu:24.04`, `dotnet-runtime`, `app-binaries`) dùng chung hoàn toàn trên Page Cache giữa mọi Container!
- **`upperdir` (Read-Write):** Lớp trống riêng của từng Container. Khi Container tạo file mới, file nằm trong `upperdir`. Khi Container **sửa một file cũ trong `lowerdir`**, OverlayFS thực hiện **Copy-Up**: sao chép file từ `lowerdir` lên `upperdir` rồi mới cho ghi (nên tránh sửa file lớn nằm ở `lowerdir` trong lúc chạy, hãy dùng **Bind Mount / Volume**!).
- **`merged`:** Góc nhìn hợp nhất mà Container nhìn thấy tại `/`.

### 3.2. Thu hẹp Bề mặt Tấn công: Capabilities & Seccomp-BPF
Vì dùng chung nhân Kernel với Host, một lỗ hổng Kernel bên trong Container có thể dẫn tới **Container Escape**:
- **Linux Capabilities:** Chia nhỏ quyền lực tối thượng của `root` thành hơn 40 quyền nhỏ lẻ (như `CAP_NET_BIND_SERVICE` để mở port < 1024, `CAP_SYS_ADMIN` để mount đĩa). Docker mặc định loại bỏ hầu hết các Capability nguy hiểm (như `CAP_SYS_MODULE`, `CAP_SYS_TIME`).
- **Seccomp-BPF (Secure Computing Mode):** Bộ lọc gắn ngay trước cửa ngõ System Call của Kernel. Hồ sơ mặc định của Docker chặn hơn **44 syscall nguy hiểm** (như `kexec_load`, `reboot`, `init_module`, `keyctl`) bằng bộ lọc eBPF cực nhanh.

---

## 4. Kiểm tra Container Isolation trực tiếp trên Linux Shell

```bash
# 1. Xem các Namespace mà một tiến trình Container (ví dụ PID 4512) đang thuộc về
$ sudo ls -l /proc/4512/ns/
lrwxrwxrwx 1 root root 0 Nov 25 10:00 cgroup -> 'cgroup:[4026532510]'
lrwxrwxrwx 1 root root 0 Nov 25 10:00 mnt    -> 'mnt:[4026532502]'
lrwxrwxrwx 1 root root 0 Nov 25 10:00 net    -> 'net:[4026532512]'
lrwxrwxrwx 1 root root 0 Nov 25 10:00 pid    -> 'pid:[4026532506]'

# 2. Xem thống kê throttling và sử dụng bộ nhớ từ cgroups v2
$ cat /sys/fs/cgroup/system.slice/docker-*.scope/cpu.stat
usage_usec 4829104
nr_periods 1200
nr_throttled 42          # Số lần Container bị đóng băng do chạm trần cpu.max!
throttled_usec 1950400

# 3. Bước vào đúng Namespace mạng của Container từ Host để debug (không cần SSH vào Container!)
$ sudo nsenter -t 4512 -n ss -tulpn
```

---

## 5. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

1. **Khi chạy 50 Container cùng dùng chung Base Image `mcr.microsoft.com/dotnet/aspnet:8.0`, các thư viện `.so` chỉ đọc trong Image có bị nhân bản 50 lần trong RAM vật lý không?**
   > **Đáp án:** **Không!** Nhờ `lowerdir` của OverlayFS dùng chung cùng một tập Inode trên Host, nhân Linux chỉ nạp các trang bộ nhớ mã lệnh dùng chung (`libc.so`, `libcoreclr.so`) **đúng 1 lần vào Page Cache** và ánh xạ cùng các Frame vật lý đó (Read-Only) vào bảng trang của cả 50 tiến trình Container!
2. **Tại sao gán CPU Limit quá chặt trên cgroup (`cpu.max`) lại có thể làm chậm API đơn luồng hoặc đa luồng ngay cả khi biểu đồ CPU tổng của máy chủ đang rất thấp?**
   > **Đáp án:** Vì CFS Bandwidth Control tính theo từng cửa sổ thời gian rất nhỏ (mặc định $100\text{ ms}$). Nếu các luồng bùng nổ xử lý trong $20\text{ ms}$ đầu tiên đã xài hết quota $50\text{ ms}$ (khi chạy song song trên nhiều lõi), Kernel sẽ **throttle (treo)** toàn bộ tiến trình suốt $80\text{ ms}$ còn lại của chu kỳ dù các lõi CPU của Host đang hoàn toàn rảnh rỗi!
3. **Nêu hai cơ chế bảo vệ ngăn một Container chạy dưới tài khoản `root` (bên trong Container) chiếm quyền điều khiển toàn bộ máy chủ Host.**
   > **Đáp án:** (1) **User Namespace (`CLONE_NEWUSER`)** ánh xạ `UID 0` bên trong Container thành một `UID` vô quyền (ví dụ `100000`) ngoài Host; (2) **Drop Capabilities + Seccomp-BPF** tước bỏ các quyền đặc biệt (`CAP_SYS_ADMIN`, `CAP_SYS_MODULE`) và chặn các System Call nhạy cảm không cho chạm tới phần cứng hay Kernel Host.

<!-- studxy-os-v4 -->
<!-- studxy-os-v5 -->
"""";

    private static string GetOsLesson53Markdown() => """"
# Bài 5.3: Case Study Hiệu Năng Hệ Thống: eBPF, Page Faults, OOM Killer & Phân Tích Latency Thực Chiến

> [!NOTE]
> **Khái niệm then chốt:** Lý thuyết Hệ điều hành chỉ thực sự tỏa sáng khi máy chủ sản xuất (Production) bỗng dưng có độ trễ đuôi **p99 vọt từ 12 ms lên 4,500 ms** hoặc một Worker biến mất không dấu vết vì **OOM Killer**. Trong bài tổng kết thực chiến này, chúng ta kết hợp **phương pháp luận USE** (*Utilization, Saturation, Errors*), bộ công cụ kinh điển (`vmstat`, `strace`, `perf`), và công nghệ đột phá của nhân Linux hiện đại — **eBPF (Extended Berkeley Packet Filter)** — để truy vết tận gốc rễ nguyên nhân ở tầng Kernel.

> [!TIP]
> **Hình ảnh trực quan:** Làm thế nào để phẫu thuật quan sát tim phổi của nhân Linux đang chạy hàng triệu request/giây mà không làm bệnh nhân ngưng thở?
> - **Các công cụ cũ (`strace`):** Chặn đứng tiến trình tại mọi System Call bằng `ptrace` để chép dữ liệu $\to$ làm chậm tiến trình **10x - 50x** (cực kỳ nguy hiểm trên Production!).
> - **eBPF (`bpftrace` / `bcc`):** Như các cảm biến nano được bộ kiểm duyệt **eBPF Verifier** chứng minh an toàn tuyệt đối (không vòng lặp vô tận, không con trỏ rác) rồi biên dịch JIT thành mã máy chạy thẳng trong Kernel! Chi phí quan sát thường dưới **1% CPU**.

---

## 1. Kiến trúc eBPF: Lập trình An toàn Ngay Trong Nhân Linux

Trước đây, muốn thêm bộ đếm độ trễ ở tầng I/O hoặc Scheduler của nhân Linux, kỹ sư phải sửa mã nguồn C của Kernel, biên dịch lại hoặc viết **Kernel Module (`.ko`)** — chỉ một lỗi con trỏ nhỏ cũng làm **Kernel Panic** sập toàn bộ máy chủ!

**eBPF** thay đổi hoàn toàn cuộc chơi với quy trình 4 bước khép kín:

```text
User Space:   [ Chuong trinh eBPF (C / bpftrace) ]
                             |  syscall bpf(BPF_PROG_LOAD)
Kernel Space:       [ eBPF Verifier ]  <-- Kiem tra tinh dung: khong loop vo han,
                             |             bounds-check bo nho, gioi han do sau stack
                    [ JIT Compiler ]   <-- Bien dich thanh ma may x86-64/ARM64 goc
                             |
              +--------------+--------------+
              v                             v
    [ kprobes / tracepoints ]     [ XDP / Socket / LSM ]
              |                             |
              +-------> [ eBPF Maps ] <-----+  (Hash, RingBuffer, Histogram chia se ve User Space)
```

### Các điểm gắn móc (Hooks) quan trọng nhất của eBPF:
1. **`tracepoints`:** Các điểm đo ổn định được định nghĩa sẵn trong mã nguồn Kernel (như `sched:sched_switch`, `block:block_rq_complete`, `exceptions:page_fault_user`).
2. **`kprobes` / `kretprobes`:** Gắn vào bất kỳ hàm nội bộ nào của Kernel khi vào/ra (ví dụ đo thời gian thực thi của `ext4_sync_file`).
3. **`uprobes`:** Gắn vào các hàm trong User Space (như theo dõi lệnh `malloc`, truy vấn SSL/TLS hoặc độ trễ hàm Go/C#/PostgreSQL mà không cần sửa code!).
4. **`XDP (eXpress Data Path)`:** Chạy ngay trên Driver card mạng *trước cả khi Kernel cấp phát `sk_buff`*, giúp lọc chống DDoS tới **hàng chục triệu gói tin/giây trên một lõi CPU**!

---

## 2. Phương pháp USE & Phân tích Case Study Latency Thực Chiến

Khi hệ thống gặp sự cố chậm bất thường, đừng đoán mò! Hãy áp dụng **USE Method** (Brendan Gregg) kiểm tra lần lượt **CPU $\to$ Memory $\to$ Disk I/O $\to$ Network** theo 3 trục: **Utilization** (% bận), **Saturation** (độ dài hàng đợi chờ), và **Errors** (lỗi).

### Case Study 1: Tại sao API đang chạy nhanh bỗng đơ 2 giây mỗi phút (Tail Latency p99 Spike)?
**Triệu chứng:** API trả về 15 ms ở mức trung vị (p50), nhưng p99 vọt lên 2,100 ms mỗi khi hệ thống ghi log/export file lớn.

**Bước 1 — Soi tổng quan bằng `vmstat 1`:**
```bash
$ vmstat 1
procs -----------memory---------- ---swap-- -----io---- -system-- ------cpu-----
 r  b   swpd   free   buff  cache   si   so    bi    bo   in   cs us sy id wa st
 1  4      0 184200  14200 12450000  0    0    12 98500 4200 6100 12  6 38 44  0
```
- Cột **`b` (Blocked processes waiting for I/O)** tăng lên `4`, cột **`wa` (I/O Wait)** nhảy lên **`44%`**, và cột **`bo` (Blocks out)** xả gần 100 MB/s!
- Nghi vấn ngay **Writeback Saturation** hoặc **Sync `fsync()` trên cùng ổ đĩa** đang cạnh tranh với luồng phục vụ API!

**Bước 2 — Chụp biểu đồ Histogram độ trễ khối I/O bằng `bpftrace`:**
```bash
# Đo phân bố thời gian chờ I/O từ lúc đẩy vào block queue đến lúc phần cứng hoàn tất (microseconds)
$ sudo bpftrace -e '
  tracepoint:block:block_rq_issue { @start[args->dev, args->sector] = nsecs; }
  tracepoint:block:block_rq_complete /@start[args->dev, args->sector]/ {
    @us = hist((nsecs - @start[args->dev, args->sector]) / 1000);
    delete(@start[args->dev, args->sector]);
  }'
```
Kết quả cho thấy đuôi histogram xuất hiện các yêu cầu mất tới `524 ms - 1048 ms`! Kiểm tra thêm lệnh ghi của ứng dụng bằng `bpftrace` trên syscall `fsync` / `write` phát hiện luồng nền ghi file dump chạm trần `vm.dirty_ratio`, khiến Kernel **ép đồng bộ luồng API chính khi API ghi log truy cập**!

**Cách giải quyết triệt để:**
- Hạ `vm.dirty_background_bytes` (ví dụ 64 MB) và `vm.dirty_bytes` (256 MB) để Kernel xả đĩa liên tục thành dòng nhỏ đều đặn thay vì tích tụ thành cơn sóng thần 2 GB mới xả một lần.
- Tách đĩa OS/Log khỏi đĩa Database hoặc chuyển ghi log sang luồng bất đồng bộ hoàn toàn (Non-blocking Ring Buffer).

---

### Case Study 2: Tiến trình Worker biến mất bí ẩn & Giải phẫu OOM Killer

**Triệu chứng:** Dịch vụ xử lý ảnh/AI chạy trong Docker bỗng nhận tín hiệu `SIGKILL (Exit Code 137)` mà không kịp ghi bất kỳ log lỗi nào!

**Bước 1 — Truy vết nhật ký Kernel `dmesg -T`:**
```text
[Tue Nov 25 14:12:08 2026] memory: usage 2097152kB, limit 2097152kB, failcnt 14829
[Tue Nov 25 14:12:08 2026] Memory cgroup out of memory: Killed process 18421 (python3) total-vm:3421000kB, anon-rss:1985400kB, file-rss:102400kB, shmem-rss:0kB, oom_score_adj:0
```
- **Giải mã thông điệp vàng của Kernel:**
  - `Memory cgroup out of memory`: Không phải cả máy chủ hết RAM, mà chính **cgroup Container** này đã chạm trần `limit 2097152kB` (2 GB)!
  - `anon-rss: 1985400kB`: Bộ nhớ ẩn danh (Heap/Arrays của Python) chiếm tới **1.98 GB** nên không thể thu hồi bằng cách bỏ Page Cache (`file-rss` chỉ 100 MB).
  - Kernel chấm điểm `oom_score` (tính theo tỷ lệ RAM tiến trình chiếm trên tổng RAM khả dụng cộng với `oom_score_adj` từ `-1000` đến `+1000`) và tung đòn `SIGKILL` (mã thoát `128 + 9 = 137`).

---

### Case Study 3: Soi nóng CPU & Page Faults bằng `perf` & FlameGraph

Khi CPU `us` (User) hoặc `sy` (System) lên tới 99% mà không biết hàm nào đang đốt CPU:
```bash
# Lấy mẫu Call Stack ở tần số 99 Hz trong 30 giây trên mọi lõi CPU (kèm symbol Kernel & User)
$ sudo perf record -F 99 -a -g -- sleep 30
$ sudo perf report --stdio | head -n 25

# Đếm nhanh số lần Minor/Major Page Faults và Context Switches của một tiến trình
$ perf stat -e page-faults,minor-faults,major-faults,context-switches,cpu-migrations -p 18421 -- sleep 10
```
- Nếu **`sy` (Kernel CPU)** cao bất thường và `perf top` thấy hàm `copy_user_enhanced_fast_string` hoặc `futex_wait` đứng đầu $\to$ ứng dụng đang **tranh chấp khóa (Lock Contention)** quá nặng hoặc gọi quá nhiều syscall nhỏ lẻ!
- Nếu **`minor-faults`** hàng triệu lần/giây $\to$ ứng dụng cấp phát rồi giải phóng vùng nhớ lớn liên tục (`malloc/free` vượt ngưỡng `M_MMAP_THRESHOLD` 128 KB trên `glibc`), ép Kernel liên tục phải xóa sạch trang `0` (*Zero-page demand fault*) và cập nhật bảng trang!

---

## 3. Bảng Tra Cứu Nhanh Công Cụ Chẩn Đoán Linux Production

| Mục tiêu cần soi | Công cụ truyền thống | Công cụ eBPF / Hiện đại | Chỉ số cảnh báo đỏ |
|---|---|---|---|
| **Tổng quan CPU / Swap / I/O** | `vmstat 1`, `mpstat -P ALL 1` | `sysstat` + Prometheus node_exporter | `wa > 20%`, `si/so > 0` (Thrashing), `r > 2 * cores` |
| **Độ trễ khối đĩa & Hàng đợi** | `iostat -xz 1`, `iotop` | `biolatency` / `biosnoop` (bcc/bpftrace) | `await > 10ms` (NVMe), `aqu-sz >> 1` |
| **Truy vết System Call** | `strace -ff -T -p <pid>` *(cẩn thận chậm app!)* | `syscount` / `tracepoint:raw_syscalls` | Hàng trăm nghìn `futex` hoặc `read(1 byte)` lỗi |
| **Hồ sơ đốt CPU & Lock** | `perf record -F 99 -g`, FlameGraphs | `profile`, `offcputime` (đo thời gian ngủ chờ!) | Tháp rộng ở `mutex_lock`, `GC`, hoặc `page_fault` |
| **Mạng & TCP Retransmit** | `ss -ti`, `tcpdump` | `tcplife`, `tcpretrans`, `tcpconnect` | Tỷ lệ `retrans` tăng, `Recv-Q` / `Send-Q` ứ đọng |

---

## 4. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

1. **Tại sao không nên chạy `strace -f -p <pid>` trực tiếp lên một dịch vụ Web đang chịu tải cao trên Production, và eBPF khắc phục điều này ra sao?**
   > **Đáp án:** `strace` dùng `ptrace()` khiến Kernel phải dừng tiến trình mục tiêu và chuyển ngữ cảnh sang tiến trình `strace` **2 lần cho mỗi System Call** (lúc vào và lúc ra), làm chậm dịch vụ gấp hàng chục lần. Ngược lại, **eBPF** chạy trực tiếp trong ngữ cảnh Kernel tại các điểm `tracepoints`, tổng hợp số liệu ngay vào `eBPF Maps` (như histogram) trong RAM Kernel rồi mới đọc tổng hợp về User Space nên overhead cực thấp.
2. **Ý nghĩa của mã thoát `Exit Code 137` khi một Container bị tắt đột ngột là gì? Làm sao phân biệt bị OOM Killer giết với bị ai đó gửi `kill -9`?**
   > **Đáp án:** $137 = 128 + 9$, tức tiến trình bị kết thúc bởi tín hiệu **`SIGKILL (9)`** không thể bắt (*uncatchable*). Để phân biệt do **OOM Killer** hay do quản trị viên chạy `kill -9`, cần kiểm tra `dmesg -T` (tìm dòng `Out of memory: Killed process` / `Memory cgroup out of memory`) hoặc kiểm tra file `memory.events` trong cgroups v2 (bộ đếm `oom_kill > 0`).
3. **Trong `perf stat`, khi nào số lượng `minor-faults` tăng đột biến mà lại không gây ra bất kỳ lượt đọc đĩa nào?**
   > **Đáp án:** Khi tiến trình liên tục cấp phát các mảng bộ nhớ lớn (ví dụ qua `mmap` hoặc `malloc` > 128 KB) rồi chạm tới các trang lần đầu tiên (*Lazy Allocation*). Vì các trang ẩn danh mới chưa từng có trên đĩa, Kernel chỉ cấp phát trang vật lý chứa toàn số 0 ngay trong RAM (*Zero-fill-on-demand*) mà không cần I/O đĩa — đó là **Minor Page Fault**, nhưng nếu xảy ra hàng triệu lần/giây vẫn đốt lượng lớn CPU ở chế độ Kernel (`%sy`) và gây TLB Shootdown khi giải phóng (`munmap`).

<!-- studxy-os-v4 -->
<!-- studxy-os-v5 -->
"""";
    #endregion

    #region 2. Operating Systems (AcademicCourseId) PracticeActivities (24 activities across 4.1..5.3)

    private static List<PracticeActivity> GetOsExpandedActivities(Guid courseId, DateTimeOffset now) => new()
    {
        // Lesson 4.1: Inode, Extent, B-Tree & Journaling
        MakeOsActivity(courseId, OsLesson41Id, 1, "predict_output",
            "Đoán Output JS: Tính dung lượng tối đa qua Direct + Single Indirect Pointers",
            "Tính tổng dung lượng tệp (KB) khi Inode có 12 con trỏ trực tiếp và 1 con trỏ gián tiếp cấp 1.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l41-pred-1",
                        difficulty = "Dễ",
                        title = "Dung lượng tối đa của Inode (12 Direct + 1 Single Indirect)",
                        description = "Với block 4 KB và mỗi địa chỉ block 4 bytes (1024 con trỏ mỗi block), dự đoán tổng dung lượng tính theo KB.",
                        code = "const directCount = 12;\nconst blockSizeKB = 4;\nconst pointersPerBlock = (4 * 1024) / 4;\nconst maxKB = (directCount + pointersPerBlock) * blockSizeKB;\nconsole.log(maxKB);",
                        expectedOutput = "4144",
                        hint = "12 * 4 KB = 48 KB; Single Indirect trỏ tới 1024 * 4 KB = 4096 KB.",
                        explanation = "Tổng cộng 48 + 4096 = 4144 KB (~4.05 MB) trước khi phải dùng tới Double Indirect."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson41Id, 2, "fill_code",
            "Điền Code OS: Điều kiện giải phóng Inode khi xóa tệp POSIX (unlink)",
            "Hoàn chỉnh logic kiểm tra i_links_count và số File Descriptor đang mở trước khi thu hồi Data Blocks.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l41-fill-1",
                        title = "Thu hồi Inode chỉ khi links_count == 0 VÀ open_fds == 0",
                        category = "os",
                        language = "javascript",
                        description = "Điền điều kiện kiểm tra sau lệnh unlink để tránh xóa mất dữ liệu tiến trình đang đọc.",
                        practicalScenario = "Quản trị viên chạy rm app.log nhưng tiến trình Java vẫn giữ FD mở nên dung lượng đĩa chưa giảm.",
                        codeTemplate = new[]
                        {
                            "function onUnlinkFile(inode, openFdCount) {",
                            "  inode.linksCount = Math.max(0, inode.linksCount - 1);",
                            "  if (inode.linksCount {{BLANK_0}} 0 {{BLANK_1}} openFdCount === 0) {",
                            "    return 'FREE_BLOCKS';",
                            "  }",
                            "  return '{{BLANK_2}}';",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "===", acceptableAnswers = new[] { "===", "==" }, hint = "Kiểm tra số Hard Link đã về 0 chưa.", chips = new[] { "===", ">", "!==" }, explanation = "Chỉ khi không còn tên tệp nào (linksCount === 0) mới xét giải phóng." },
                            new { id = "b1", blankIndex = 1, expected = "&&", acceptableAnswers = new[] { "&&" }, hint = "Toán tử logic VÀ: phải thỏa đồng thời cả 2 điều kiện.", chips = new[] { "&&", "||", "^" }, explanation = "Phải đồng thời hết Hard Link VÀ không còn tiến trình nào mở File Descriptor." },
                            new { id = "b2", blankIndex = 2, expected = "KEEP_INODE", acceptableAnswers = new[] { "KEEP_INODE" }, hint = "Vẫn giữ Inode tồn tại cho FD đang mở hoặc Hard Link khác.", chips = new[] { "KEEP_INODE", "FREE_BLOCKS", "PANIC" }, explanation = "Inode tiếp tục sống ẩn danh cho tới khi openFdCount về 0." }
                        },
                        lineExplanations = Array.Empty<object>()
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson41Id, 3, "bug_fix",
            "Sửa Bug OS: Điều kiện cấp phát tệp mới khi cạn kiệt Inode (ENOSPC)",
            "Sửa lỗi kiểm tra thiếu Inode hoặc thiếu Block trống khi tạo tệp mới.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l41-bug-1",
                        title = "Bắt buộc còn cả Free Inode VÀ Free Block khi tạo tệp",
                        language = "javascript",
                        categoryTag = "OS File System",
                        context = "Một phân vùng có thể còn hàng trăm GB (freeBlocks > 0) nhưng hết sạch Inode (freeInodes === 0) vẫn phải từ chối tạo tệp mới!",
                        buggyCode = "function canCreateNonEmptyFile(freeInodes, freeBlocks) {\n  return freeInodes > 0 || freeBlocks > 0;\n}",
                        bugLineNumber = 2,
                        bugBriefDescription = "Dùng || thay vì && khiến tạo tệp vẫn trả true khi hết sạch Inode.",
                        rootCauseAnalysis = "Tạo tệp có dữ liệu bắt buộc cần ít nhất 1 Inode trống VÀ 1 Data Block trống.",
                        cleanFixedCode = "function canCreateNonEmptyFile(freeInodes, freeBlocks) {\n  return freeInodes > 0 && freeBlocks > 0;\n}",
                        requiredTokens = new[] { "&&" },
                        functionName = "canCreateNonEmptyFile",
                        testCases = new object[]
                        {
                            new { name = "freeInodes=0, freeBlocks=5000 -> false", input = new object[] { 0, 5000 }, expected = false },
                            new { name = "freeInodes=10, freeBlocks=100 -> true", input = new object[] { 10, 100 }, expected = true }
                        },
                        explanation = "Đổi || thành && để kiểm tra đồng thời còn Inode lẫn còn Data Block."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson41Id, 4, "js_sandbox",
            "Code Lab OS: Gom nhóm các Physical Blocks thành Extents (ext4)",
            "Viết hàm countExtents(sortedBlocks) đếm số lượng Extent tối thiểu cần để biểu diễn dãy block đã sắp xếp.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l41-js-1",
                        difficulty = "Trung bình",
                        title = "Nén danh sách Block thành Extents liên tục",
                        description = "Cho mảng số hiệu block vật lý đã sắp xếp tăng dần (ví dụ [10, 11, 12, 20, 21]). Các block liên tiếp nhau (+1) gộp thành 1 Extent. Trả về tổng số Extent.",
                        functionName = "countExtents",
                        starterCode = "function countExtents(sortedBlocks) {\n  if (!sortedBlocks || sortedBlocks.length === 0) return 0;\n  let extents = 1;\n  // TODO: nếu sortedBlocks[i] !== sortedBlocks[i - 1] + 1 thì tăng extents\n  return extents;\n}",
                        solutionCode = "function countExtents(sortedBlocks) {\n  if (!sortedBlocks || sortedBlocks.length === 0) return 0;\n  let extents = 1;\n  for (let i = 1; i < sortedBlocks.length; i++) {\n    if (sortedBlocks[i] !== sortedBlocks[i - 1] + 1) extents++;\n  }\n  return extents;\n}",
                        hint = "Duyệt từ i = 1, mỗi lần phát hiện bước nhảy khác +1 là bắt đầu một Extent mới.",
                        testCases = new object[]
                        {
                            new { name = "[10,11,12,20,21] -> 2 extents", input = new object[] { new[] { 10, 11, 12, 20, 21 } }, expected = 2 },
                            new { name = "[5,6,7,8] -> 1 extent", input = new object[] { new[] { 5, 6, 7, 8 } }, expected = 1 },
                            new { name = "[] -> 0 extents", input = new object[] { Array.Empty<int>() }, expected = 0 }
                        }
                    }
                }
            }, now),

        // Lesson 4.2: Page Cache, Direct I/O & Disk Scheduling
        MakeOsActivity(courseId, OsLesson42Id, 1, "predict_output",
            "Đoán Output JS: Tổng quãng đường di chuyển đầu từ FCFS vs Page Cache Dirty Flush",
            "Tính tổng số cylinder đầu từ phải di chuyển khi xử lý hàng đợi đọc đĩa tuần tự.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l42-pred-1",
                        difficulty = "Dễ",
                        title = "Tính tổng quãng đường Seek của hàng đợi I/O",
                        description = "Đầu từ đang ở cylinder 40, lần lượt phục vụ hàng đợi [20, 60, 10]. Dự đoán tổng khoảng cách di chuyển.",
                        code = "const queue = [20, 60, 10];\nlet head = 40, move = 0;\nfor (const q of queue) {\n  move += Math.abs(q - head);\n  head = q;\n}\nconsole.log(move);",
                        expectedOutput = "110",
                        hint = "|20 - 40| = 20; |60 - 20| = 40; |10 - 60| = 50 -> Tổng = 110.",
                        explanation = "Đầu từ di chuyển qua lại 20 + 40 + 50 = 110 cylinders."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson42Id, 2, "fill_code",
            "Điền Code OS: Mô phỏng Page Cache Write-Back & Throttle",
            "Hoàn chỉnh hàm ghi trang vào Page Cache kích hoạt Flush khi đạt ngưỡng dirtyRatio.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l42-fill-1",
                        title = "Đánh dấu Dirty Page và kích hoạt Writeback Flush",
                        category = "os",
                        language = "javascript",
                        description = "Điền thao tác đánh dấu trang bẩn vào tập dirtyPages và xả đĩa khi vượt ngưỡng.",
                        practicalScenario = "Khi dirtyPages đạt ngưỡng vm.dirty_ratio, Kernel chặn tiến trình ghi để xả đĩa.",
                        codeTemplate = new[]
                        {
                            "function writeToPageCache(dirtyPages, pageId, maxDirtyLimit) {",
                            "  dirtyPages.{{BLANK_0}}(pageId);",
                            "  if (dirtyPages.size {{BLANK_1}} maxDirtyLimit) {",
                            "    dirtyPages.{{BLANK_2}}();",
                            "    return 'FLUSHED_TO_DISK';",
                            "  }",
                            "  return 'BUFFERED_IN_RAM';",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "add", acceptableAnswers = new[] { "add" }, hint = "Thêm pageId vào Set các trang bẩn.", chips = new[] { "add", "delete", "has" }, explanation = "Buffered write chỉ đánh dấu trang là Dirty trong RAM Page Cache." },
                            new { id = "b1", blankIndex = 1, expected = ">=", acceptableAnswers = new[] { ">=", ">" }, hint = "Khi số trang bẩn đạt hoặc vượt ngưỡng giới hạn.", chips = new[] { ">=", "<", "===" }, explanation = "Chạm ngưỡng vm.dirty_ratio thì kích hoạt xả đĩa đồng bộ." },
                            new { id = "b2", blankIndex = 2, expected = "clear", acceptableAnswers = new[] { "clear" }, hint = "Xóa danh sách trang bẩn sau khi flush xuống đĩa.", chips = new[] { "clear", "pop", "shift" }, explanation = "Sau khi DMA ghi xong xuống SSD, các trang trở thành Clean Pages." }
                        },
                        lineExplanations = Array.Empty<object>()
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson42Id, 3, "bug_fix",
            "Sửa Bug OS: Tính sai độ trễ trung bình khi Page Cache Miss",
            "Sửa công thức tính thời gian truy cập I/O trung bình khi kết hợp Page Cache Hit và SSD Read.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l42-bug-1",
                        title = "Cộng đủ thời gian kiểm tra Page Cache trong nhánh Miss",
                        language = "javascript",
                        categoryTag = "OS I/O & Page Cache",
                        context = "Khi đọc tệp qua Buffered I/O, Kernel luôn tốn tRamUs để kiểm tra Page Cache trước, rồi nếu Miss mới đọc thêm tNvmeUs từ SSD.",
                        buggyCode = "function calcIoAvgUs(hitRatio, tRamUs, tNvmeUs) {\n  return hitRatio * tRamUs + (1 - hitRatio) * tNvmeUs;\n}",
                        bugLineNumber = 2,
                        bugBriefDescription = "Nhánh Miss (1 - hitRatio) thiếu thời gian tra Page Cache tRamUs.",
                        rootCauseAnalysis = "Khi Cache Miss vẫn tốn tRamUs tra cứu XArray cộng với tNvmeUs đọc ổ đĩa.",
                        cleanFixedCode = "function calcIoAvgUs(hitRatio, tRamUs, tNvmeUs) {\n  return hitRatio * tRamUs + (1 - hitRatio) * (tRamUs + tNvmeUs);\n}",
                        requiredTokens = new[] { "tRamUs + tNvmeUs|tRamUs+tNvmeUs" },
                        functionName = "calcIoAvgUs",
                        testCases = new object[]
                        {
                            new { name = "hit=0.9, ram=1us, nvme=100us -> 11", input = new object[] { 0.9, 1, 100 }, expected = 11 }
                        },
                        explanation = "0.9 * 1 + 0.1 * (1 + 100) = 0.9 + 10.1 = 11 us."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson42Id, 4, "js_sandbox",
            "Code Lab OS: Mô phỏng Thuật toán Lập lịch Đĩa C-SCAN",
            "Viết hàm cscanSeekDistance(head, requests, maxCylinder) tính tổng quãng đường đầu từ di chuyển theo chiều tăng.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l42-js-1",
                        difficulty = "Khó",
                        title = "Thuật toán Circular SCAN (C-SCAN)",
                        description = "Đầu từ di chuyển từ head lên phía maxCylinder để phục vụ các request >= head; nếu còn request < head thì đi tiếp tới maxCylinder, quay về 0 (tính quãng đường maxCylinder), rồi phục vụ tăng dần tới request lớn nhất nhỏ hơn head.",
                        functionName = "cscanSeekDistance",
                        starterCode = "function cscanSeekDistance(head, requests, maxCylinder) {\n  const up = requests.filter(r => r >= head).sort((a, b) => a - b);\n  const down = requests.filter(r => r < head).sort((a, b) => a - b);\n  // TODO: tính tổng quãng đường C-SCAN\n  return 0;\n}",
                        solutionCode = "function cscanSeekDistance(head, requests, maxCylinder) {\n  const up = requests.filter(r => r >= head).sort((a, b) => a - b);\n  const down = requests.filter(r => r < head).sort((a, b) => a - b);\n  if (down.length === 0) {\n    return up.length === 0 ? 0 : up[up.length - 1] - head;\n  }\n  return (maxCylinder - head) + maxCylinder + down[down.length - 1];\n}",
                        hint = "Nếu có request < head: đi từ head -> maxCylinder, nhảy về 0 (+maxCylinder), rồi lên down[down.length - 1].",
                        testCases = new object[]
                        {
                            new { name = "head=50, [95,180,34,119], max=199 -> 382", input = new object[] { 50, new[] { 95, 180, 34, 119 }, 199 }, expected = 382 },
                            new { name = "head=20, [50,90], max=199 -> 70", input = new object[] { 20, new[] { 50, 90 }, 199 }, expected = 70 }
                        }
                    }
                }
            }, now),

        // Lesson 4.3: RAID, Copy-on-Write & Crash Consistency
        MakeOsActivity(courseId, OsLesson43Id, 1, "predict_output",
            "Đoán Output JS: Khôi phục dữ liệu hỏng bằng RAID-5 XOR Parity",
            "Theo dõi cách phép toán XOR tái tạo chính xác dữ liệu của 1 ổ đĩa bị chết.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l43-pred-1",
                        difficulty = "Dễ",
                        title = "Phục hồi khối dữ liệu mất bằng phép toán XOR",
                        description = "3 khối dữ liệu d0=10, d1=12, d2=7 được tính parity XOR. Giả sử đĩa chứa d1 bị hỏng, dự đoán giá trị khôi phục.",
                        code = "const d0 = 10, d1 = 12, d2 = 7;\nconst parity = d0 ^ d1 ^ d2;\nconst recoveredD1 = d0 ^ d2 ^ parity;\nconsole.log(recoveredD1 + ':' + (recoveredD1 === d1));",
                        expectedOutput = "12:true",
                        hint = "Tính chất giao hoán và tự triệt tiêu của XOR: x ^ x = 0.",
                        explanation = "d0 ^ d2 ^ (d0 ^ d1 ^ d2) = d1 = 12, chứng minh RAID-5 khôi phục toàn vẹn khi mất 1 đĩa."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson43Id, 2, "fill_code",
            "Điền Code OS: Cập nhật Copy-on-Write (CoW) không ghi đè khối cũ",
            "Hoàn chỉnh hàm cập nhật nút dữ liệu theo cơ chế Copy-on-Write bảo toàn Snapshot cũ.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l43-fill-1",
                        title = "Tạo bản sao khối mới và lật con trỏ Uberblock",
                        category = "os",
                        language = "javascript",
                        description = "Điền thao tác sao chép mảng gốc sang bản mới trước khi sửa đổi để giữ nguyên Snapshot cũ.",
                        practicalScenario = "ZFS/Btrfs không bao giờ ghi đè tại chỗ; Snapshot giữ tham chiếu tới mảng cũ nguyên vẹn.",
                        codeTemplate = new[]
                        {
                            "function cowWriteBlock(oldRoot, blockIdx, newValue) {",
                            "  const newRoot = [{{BLANK_0}}oldRoot];",
                            "  newRoot[blockIdx] = {{BLANK_1}};",
                            "  return { snapshot: oldRoot, current: {{BLANK_2}} };",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "...", acceptableAnswers = new[] { "..." }, hint = "Spread operator sao chép nông sang vùng nhớ mới.", chips = new[] { "...", "*", "&" }, explanation = "Sao chép tham chiếu các khối không đổi sang cây mới mà không sửa oldRoot." },
                            new { id = "b1", blankIndex = 1, expected = "newValue", acceptableAnswers = new[] { "newValue" }, hint = "Gán giá trị mới vào khối vừa tách bản sao.", chips = new[] { "newValue", "null", "0" }, explanation = "Chỉ khối bị thay đổi mới được cấp phát dữ liệu mới." },
                            new { id = "b2", blankIndex = 2, expected = "newRoot", acceptableAnswers = new[] { "newRoot" }, hint = "Con trỏ Uberblock mới trỏ tới cây vừa cập nhật.", chips = new[] { "newRoot", "oldRoot", "blockIdx" }, explanation = "Snapshot tiếp tục đọc oldRoot an toàn, còn hệ thống hiện tại dùng newRoot." }
                        },
                        lineExplanations = Array.Empty<object>()
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson43Id, 3, "bug_fix",
            "Sửa Bug OS: Sai công thức tính dung lượng khả dụng của RAID 6",
            "Sửa lỗi chỉ trừ 1 ổ parity thay vì 2 ổ parity (P + Q) cho RAID 6.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l43-bug-1",
                        title = "Sửa dung lượng khả dụng RAID 6 (N - 2)",
                        language = "javascript",
                        categoryTag = "OS Storage & RAID",
                        context = "RAID 6 dùng 2 khối Parity độc lập (P và Q) để chịu được hỏng 2 ổ đĩa đồng thời, nên mất 2 ổ cho dự phòng.",
                        buggyCode = "function calcRaid6UsableTB(numDisks, diskSizeTB) {\n  if (numDisks < 4) return 0;\n  return (numDisks - 1) * diskSizeTB;\n}",
                        bugLineNumber = 3,
                        bugBriefDescription = "Trừ 1 (như RAID 5) thay vì trừ 2 ổ Parity cho RAID 6.",
                        rootCauseAnalysis = "RAID 6 dành 2 ổ tương đương cho Parity P và Q nên dung lượng khả dụng là (numDisks - 2) * diskSizeTB.",
                        cleanFixedCode = "function calcRaid6UsableTB(numDisks, diskSizeTB) {\n  if (numDisks < 4) return 0;\n  return (numDisks - 2) * diskSizeTB;\n}",
                        requiredTokens = new[] { "numDisks - 2|numDisks-2" },
                        functionName = "calcRaid6UsableTB",
                        testCases = new object[]
                        {
                            new { name = "6 disks x 4TB -> 16TB", input = new object[] { 6, 4 }, expected = 16 },
                            new { name = "4 disks x 10TB -> 20TB", input = new object[] { 4, 10 }, expected = 20 }
                        },
                        explanation = "Dung lượng khả dụng RAID 6 = (N - 2) × dung lượng 1 đĩa."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson43Id, 4, "js_sandbox",
            "Code Lab OS: Bộ tính Dung lượng & Độ chịu lỗi các mức RAID",
            "Viết hàm calcRaidStats(level, numDisks, diskSizeTB) trả về { usableTB, faultTolerance }.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l43-js-1",
                        difficulty = "Trung bình",
                        title = "Tính Usable Capacity & Fault Tolerance cho RAID 0/1/5/6/10",
                        description = "Hỗ trợ level: 'RAID0', 'RAID1' (mirror tất cả N đĩa -> dung lượng 1 đĩa, chịu N-1), 'RAID5', 'RAID6', 'RAID10' (N/2 cặp mirror -> dung lượng N/2*S, đảm bảo chịu tối thiểu 1 đĩa).",
                        functionName = "calcRaidStats",
                        starterCode = "function calcRaidStats(level, numDisks, diskSizeTB) {\n  // TODO: trả về { usableTB, faultTolerance }\n  return { usableTB: 0, faultTolerance: 0 };\n}",
                        solutionCode = "function calcRaidStats(level, numDisks, diskSizeTB) {\n  if (level === 'RAID0') return { usableTB: numDisks * diskSizeTB, faultTolerance: 0 };\n  if (level === 'RAID1') return { usableTB: diskSizeTB, faultTolerance: numDisks - 1 };\n  if (level === 'RAID5') return { usableTB: (numDisks - 1) * diskSizeTB, faultTolerance: 1 };\n  if (level === 'RAID6') return { usableTB: (numDisks - 2) * diskSizeTB, faultTolerance: 2 };\n  if (level === 'RAID10') return { usableTB: (numDisks / 2) * diskSizeTB, faultTolerance: 1 };\n  return { usableTB: 0, faultTolerance: 0 };\n}",
                        hint = "RAID5: (N-1)*S, chịu 1; RAID6: (N-2)*S, chịu 2; RAID10: (N/2)*S, chịu tối thiểu 1 đĩa chắc chắn.",
                        testCases = new object[]
                        {
                            new { name = "RAID5, 4x4TB -> 12TB, ft=1", input = new object[] { "RAID5", 4, 4 }, expected = new { usableTB = 12, faultTolerance = 1 } },
                            new { name = "RAID10, 4x4TB -> 8TB, ft=1", input = new object[] { "RAID10", 4, 4 }, expected = new { usableTB = 8, faultTolerance = 1 } },
                            new { name = "RAID6, 6x8TB -> 32TB, ft=2", input = new object[] { "RAID6", 6, 8 }, expected = new { usableTB = 32, faultTolerance = 2 } }
                        }
                    }
                }
            }, now),

        // Lesson 5.1: Spinlock vs Mutex, Lock-Free Atomic CAS & Priority Inversion
        MakeOsActivity(courseId, OsLesson51Id, 1, "predict_output",
            "Đoán Output JS: Thực thi Compare-And-Swap (CAS) khi hai luồng đụng độ",
            "Theo dõi kết quả trả về của hai thao tác CAS cạnh tranh cập nhật cùng một giá trị.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l51-pred-1",
                        difficulty = "Dễ",
                        title = "So sánh Atomic CAS giữa 2 luồng",
                        description = "Luồng 1 và Luồng 2 đều đọc shared = 10 rồi gọi cas(10, ...). Dự đoán đầu ra.",
                        code = "let shared = 10;\nfunction cas(expected, next) {\n  if (shared === expected) { shared = next; return true; }\n  return false;\n}\nconst r1 = cas(10, 15);\nconst r2 = cas(10, 20);\nconsole.log(r1 + ':' + r2 + ':' + shared);",
                        expectedOutput = "true:false:15",
                        hint = "r1 đổi shared thành 15, nên r2 kiểm tra shared === 10 thất bại!",
                        explanation = "Lệnh CAS đảm bảo chỉ 1 luồng thắng; luồng thua nhận false và phải đọc lại giá trị mới 15 để thử lại."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson51Id, 2, "fill_code",
            "Điền Code OS: Vòng lặp Atomic CAS cộng dồn không khóa (Lock-Free)",
            "Hoàn chỉnh hàm atomicAdd dùng vòng lặp đọc lại giá trị cũ và gọi CAS cho tới khi thành công.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l51-fill-1",
                        title = "Cập nhật bộ đếm Lock-Free bằng vòng lặp CAS",
                        category = "os",
                        language = "javascript",
                        description = "Điền biến kỳ vọng và điều kiện lặp retry cho CAS loop.",
                        practicalScenario = "Triển khai bộ đếm hiệu năng cao không dùng Mutex (tương tự Interlocked.Add / std::atomic::fetch_add).",
                        codeTemplate = new[]
                        {
                            "function lockFreeAdd(cell, delta, casFn) {",
                            "  let oldVal;",
                            "  do {",
                            "    oldVal = cell.{{BLANK_0}};",
                            "  } while ({{BLANK_1}}casFn(cell, oldVal, oldVal + {{BLANK_2}}));",
                            "  return oldVal;",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "value", acceptableAnswers = new[] { "value" }, hint = "Đọc giá trị hiện tại trong ô nhớ.", chips = new[] { "value", "lock", "next" }, explanation = "Luôn đọc snapshot mới nhất trước mỗi lần thử CAS." },
                            new { id = "b1", blankIndex = 1, expected = "!", acceptableAnswers = new[] { "!" }, hint = "Lặp lại khi casFn trả về false (thất bại do luồng khác chen ngang).", chips = new[] { "!", "", "!!" }, explanation = "Nếu CAS thất bại (!casFn), vòng lặp tự động đọc lại oldVal và thử lại." },
                            new { id = "b2", blankIndex = 2, expected = "delta", acceptableAnswers = new[] { "delta" }, hint = "Giá trị mới = oldVal + delta.", chips = new[] { "delta", "1", "oldVal" }, explanation = "Tính giá trị đích từ bản chụp oldVal." }
                        },
                        lineExplanations = Array.Empty<object>()
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson51Id, 3, "bug_fix",
            "Sửa Bug OS: Sai chiều nâng độ ưu tiên trong Priority Inheritance",
            "Sửa lỗi dùng Math.min thay vì Math.max khi tính độ ưu tiên kế thừa cho luồng đang giữ Mutex.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l51-bug-1",
                        title = "Sửa công thức Priority Inheritance (Math.max)",
                        language = "javascript",
                        categoryTag = "OS Synchronization",
                        context = "Khi nhiều luồng có độ ưu tiên cao hơn đang bị chặn chờ Mutex của holder, holder phải kế thừa độ ưu tiên LỚN NHẤT trong số đó (số lớn = ưu tiên cao).",
                        buggyCode = "function getInheritedPriority(holderBasePrio, waiterPrios) {\n  return Math.min(holderBasePrio, ...waiterPrios);\n}",
                        bugLineNumber = 2,
                        bugBriefDescription = "Dùng Math.min làm holder bị hạ độ ưu tiên xuống thấp hơn nữa!",
                        rootCauseAnalysis = "Priority Inheritance phải nâng độ ưu tiên của holder lên bằng max(holderBasePrio, ...waiterPrios).",
                        cleanFixedCode = "function getInheritedPriority(holderBasePrio, waiterPrios) {\n  return Math.max(holderBasePrio, ...waiterPrios);\n}",
                        requiredTokens = new[] { "Math.max" },
                        functionName = "getInheritedPriority",
                        testCases = new object[]
                        {
                            new { name = "base=10, waiters=[30, 90] -> 90", input = new object[] { 10, new[] { 30, 90 } }, expected = 90 },
                            new { name = "base=50, waiters=[] -> 50", input = new object[] { 50, Array.Empty<int>() }, expected = 50 }
                        },
                        explanation = "Math.max nâng holder lên mức ưu tiên cao nhất của các luồng đang chờ khóa."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson51Id, 4, "js_sandbox",
            "Code Lab OS: Mô phỏng Tagged Pointer chống lỗi ABA",
            "Viết hàm taggedCas(cell, expectedPtr, expectedVer, newPtr) chỉ cập nhật khi cả ptr lẫn version đều khớp, đồng thời tăng version thêm 1.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l51-js-1",
                        difficulty = "Trung bình",
                        title = "Double-Width Tagged CAS chống ABA Problem",
                        description = "cell là object { ptr, version }. Nếu ptr === expectedPtr && version === expectedVer thì cập nhật ptr = newPtr, version += 1 và trả về true; ngược lại trả về false.",
                        functionName = "taggedCas",
                        starterCode = "function taggedCas(cell, expectedPtr, expectedVer, newPtr) {\n  // TODO: kiểm tra đồng thời ptr và version để chống ABA\n  return false;\n}",
                        solutionCode = "function taggedCas(cell, expectedPtr, expectedVer, newPtr) {\n  if (cell.ptr === expectedPtr && cell.version === expectedVer) {\n    cell.ptr = newPtr;\n    cell.version += 1;\n    return true;\n  }\n  return false;\n}",
                        hint = "Kiểm tra cả cell.ptr === expectedPtr && cell.version === expectedVer trước khi tăng cell.version.",
                        testCases = new object[]
                        {
                            new { name = "ABA detected (version lệch) -> false", input = new object[] { new { ptr = "A", version = 3 }, "A", 1, "B" }, expected = false },
                            new { name = "Match ptr & version -> true", input = new object[] { new { ptr = "A", version = 1 }, "A", 1, "B" }, expected = true }
                        }
                    }
                }
            }, now),

        // Lesson 5.2: Linux Kernel Namespaces, cgroups v2 & Container Isolation
        MakeOsActivity(courseId, OsLesson52Id, 1, "predict_output",
            "Đoán Output JS: Hiện tượng CPU Throttling trong cgroups v2 (cpu.max)",
            "Dự đoán xem Container có bị Kernel đóng băng (throttle) trong chu kỳ 100ms khi dùng nhiều luồng song song.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l52-pred-1",
                        difficulty = "Dễ",
                        title = "Phát hiện CPU Throttling với cpu.max = 50000 100000",
                        description = "Container giới hạn 0.5 vCPU (50,000 us mỗi 100,000 us). App dùng 4 luồng chạy đồng thời trong 20 ms đầu chu kỳ.",
                        code = "const quotaUs = 50000, threads = 4, burstMs = 20;\nconst totalUsedUs = threads * burstMs * 1000;\nconst throttled = totalUsedUs > quotaUs;\nconsole.log(throttled + ':' + totalUsedUs);",
                        expectedOutput = "true:80000",
                        hint = "4 luồng * 20 ms = 80,000 us > 50,000 us quota!",
                        explanation = "Dù chỉ mới chạy 20ms đồng hồ thực, tổng CPU time đã vượt 50ms nên Container bị đóng băng 80ms còn lại!"
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson52Id, 2, "fill_code",
            "Điền Code OS: Độ phân giải tệp phân tầng trong OverlayFS",
            "Hoàn chỉnh hàm tra cứu tệp trong OverlayFS: ưu tiên upperdir, kiểm tra tệp xóa trắng (whiteout), rồi mới xuống lowerdir.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l52-fill-1",
                        title = "Đọc tệp qua OverlayFS (upperdir -> whiteout -> lowerdir)",
                        category = "os",
                        language = "javascript",
                        description = "Điền thứ tự kiểm tra lớp trên (upper), tệp đánh dấu đã xóa (whiteouts) và lớp dưới (lower).",
                        practicalScenario = "Container xóa một file thuộc Image gốc (lowerdir) mà không sửa đổi được lớp Read-Only nên tạo whiteout ở upperdir.",
                        codeTemplate = new[]
                        {
                            "function resolveOverlayFile(name, upper, whiteouts, lower) {",
                            "  if (upper[name]) return upper[name];",
                            "  if (whiteouts.{{BLANK_0}}(name)) return {{BLANK_1}};",
                            "  return {{BLANK_2}}[name] ?? null;",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "has", acceptableAnswers = new[] { "has" }, hint = "Kiểm tra Set whiteouts có đánh dấu tệp đã bị xóa trong Container chưa.", chips = new[] { "has", "add", "size" }, explanation = "Nếu có whiteout thì tệp coi như không tồn tại đối với Container." },
                            new { id = "b1", blankIndex = 1, expected = "null", acceptableAnswers = new[] { "null" }, hint = "Trả về null (ENOENT) thay vì đọc xuống lowerdir.", chips = new[] { "null", "true", "upper" }, explanation = "Whiteout che khuất tệp ở lớp lowerdir Read-Only." },
                            new { id = "b2", blankIndex = 2, expected = "lower", acceptableAnswers = new[] { "lower" }, hint = "Nếu upper và whiteout đều không có, đọc từ lowerdir.", chips = new[] { "lower", "upper", "whiteouts" }, explanation = "Tận dụng lớp ảnh Image gốc dùng chung trên Page Cache." }
                        },
                        lineExplanations = Array.Empty<object>()
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson52Id, 3, "bug_fix",
            "Sửa Bug OS: Bỏ quên File Page Cache khi kiểm tra giới hạn cgroup memory.max",
            "Sửa lỗi tính tổng RAM tiêu thụ của cgroup chỉ tính Anonymous Memory mà bỏ qua File Cache.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l52-bug-1",
                        title = "Tính đủ Anon + File Page Cache vào memory.current",
                        language = "javascript",
                        categoryTag = "OS cgroups v2",
                        context = "Trong cgroups v2, memory.current bao gồm cả anonBytes (Heap/Stack) VÀ fileBytes (Page Cache do container đọc/ghi). Nếu bỏ quên fileBytes sẽ tính sai thời điểm chạm memory.max!",
                        buggyCode = "function isCgroupOomExceeded(anonBytes, fileBytes, memMaxBytes) {\n  return anonBytes >= memMaxBytes;\n}",
                        bugLineNumber = 2,
                        bugBriefDescription = "Không cộng fileBytes vào tổng bộ nhớ đang tính cgroup.",
                        rootCauseAnalysis = "memory.current = anonBytes + fileBytes (+ kernel slab). Phải so sánh tổng này với memMaxBytes.",
                        cleanFixedCode = "function isCgroupOomExceeded(anonBytes, fileBytes, memMaxBytes) {\n  return (anonBytes + fileBytes) >= memMaxBytes;\n}",
                        requiredTokens = new[] { "anonBytes + fileBytes|anonBytes+fileBytes" },
                        functionName = "isCgroupOomExceeded",
                        testCases = new object[]
                        {
                            new { name = "anon=1500, file=600, max=2000 -> true", input = new object[] { 1500, 600, 2000 }, expected = true },
                            new { name = "anon=1200, file=500, max=2000 -> false", input = new object[] { 1200, 500, 2000 }, expected = false }
                        },
                        explanation = "Cộng đủ anonBytes + fileBytes giúp phản ánh đúng mức memory.current của cgroup."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson52Id, 4, "js_sandbox",
            "Code Lab OS: Đếm số chu kỳ bị CPU Throttle trong cgroups v2",
            "Viết hàm countThrottledPeriods(burstsUs, quotaUs) đếm số chu kỳ 100ms có mức tiêu thụ CPU > quotaUs.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l52-js-1",
                        difficulty = "Dễ",
                        title = "Thống kê nr_throttled từ chuỗi chu kỳ CPU",
                        description = "Cho mảng burstsUs chứa lượng CPU (microseconds) ứng dụng cố gắng dùng trong mỗi chu kỳ 100ms. Đếm số chu kỳ vượt quá quotaUs.",
                        functionName = "countThrottledPeriods",
                        starterCode = "function countThrottledPeriods(burstsUs, quotaUs) {\n  let throttled = 0;\n  // TODO: đếm các chu kỳ có b > quotaUs\n  return throttled;\n}",
                        solutionCode = "function countThrottledPeriods(burstsUs, quotaUs) {\n  let throttled = 0;\n  for (const b of burstsUs) {\n    if (b > quotaUs) throttled++;\n  }\n  return throttled;\n}",
                        hint = "Duyệt qua mảng burstsUs và tăng biến đếm nếu b > quotaUs.",
                        testCases = new object[]
                        {
                            new { name = "[30000, 80000, 50000, 65000] quota=50000 -> 2", input = new object[] { new[] { 30000, 80000, 50000, 65000 }, 50000 }, expected = 2 },
                            new { name = "[10000, 20000] quota=50000 -> 0", input = new object[] { new[] { 10000, 20000 }, 50000 }, expected = 0 }
                        }
                    }
                }
            }, now),

        // Lesson 5.3: eBPF, Page Faults, OOM Killer & Latency Case Studies
        MakeOsActivity(courseId, OsLesson53Id, 1, "predict_output",
            "Đoán Output JS: Giải mã Exit Code 137 & Tính điểm OOM Score",
            "Tính mã thoát tiến trình khi bị SIGKILL (9) và điểm oom_score sau khi điều chỉnh oom_score_adj.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l53-pred-1",
                        difficulty = "Dễ",
                        title = "Exit Code của SIGKILL và tính điểm OOM Score",
                        description = "Tiến trình chiếm 1500 MB trên tổng 2000 MB RAM và có oom_score_adj = -200. Dự đoán Exit Code và oom_score.",
                        code = "const exitCode = 128 + 9;\nconst baseScore = Math.round((1500 / 2000) * 1000);\nconst finalScore = Math.max(0, Math.min(1000, baseScore - 200));\nconsole.log(exitCode + ':' + finalScore);",
                        expectedOutput = "137:550",
                        hint = "128 + 9 = 137; baseScore = 750; 750 - 200 = 550.",
                        explanation = "Exit code 137 luôn tương ứng SIGKILL (thường do OOM Killer); oom_score nằm trong [0..1000]."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson53Id, 2, "fill_code",
            "Điền Code OS: Bộ lọc eBPF Histogram độ trễ I/O theo từng bậc lũy thừa 2",
            "Hoàn chỉnh hàm phân loại độ trễ request (microseconds) vào các bucket giống lệnh hist() của bpftrace.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l53-fill-1",
                        title = "Tính chỉ số Bucket Log2 cho eBPF Latency Histogram",
                        category = "os",
                        language = "javascript",
                        description = "Điền phép toán log2 và ngưỡng tối thiểu để phân nhóm độ trễ như công cụ biolatency.",
                        practicalScenario = "eBPF map tổng hợp phân bố p50/p99 ngay trong Kernel bằng mảng bucket lũy thừa 2 với O(1) memory.",
                        codeTemplate = new[]
                        {
                            "function getLatencyBucket(latencyUs) {",
                            "  if (latencyUs {{BLANK_0}} 1) return 0;",
                            "  return Math.{{BLANK_1}}(Math.{{BLANK_2}}(latencyUs));",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "<=", acceptableAnswers = new[] { "<=", "<" }, hint = "Các giá trị <= 1 microsecond rơi vào bucket đầu tiên.", chips = new[] { "<=", ">", "===" }, explanation = "Tránh log2(0) trả về -Infinity." },
                            new { id = "b1", blankIndex = 1, expected = "floor", acceptableAnswers = new[] { "floor" }, hint = "Làm tròn xuống số nguyên để lấy chỉ số bucket.", chips = new[] { "floor", "ceil", "round" }, explanation = "Ví dụ [4..7] đều rơi cùng bucket 2." },
                            new { id = "b2", blankIndex = 2, expected = "log2", acceptableAnswers = new[] { "log2" }, hint = "Hàm logarit cơ số 2 trong JavaScript.", chips = new[] { "log2", "log10", "sqrt" }, explanation = "Histogram lũy thừa 2 giúp biểu diễn dải độ trễ từ us tới giây chỉ bằng ~25 buckets." }
                        },
                        lineExplanations = Array.Empty<object>()
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson53Id, 3, "bug_fix",
            "Sửa Bug OS: Kiểm tra điều kiện miễn tử OOM Killer (-1000)",
            "Sửa lỗi kiểm tra sai giá trị oom_score_adj bảo vệ Database khỏi OOM Killer.",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l53-bug-1",
                        title = "Chỉ miễn tử OOM Killer khi oom_score_adj === -1000",
                        language = "javascript",
                        categoryTag = "OS Memory & OOM Killer",
                        context = "Trong nhân Linux, giá trị đặc biệt -1000 trong /proc/[pid]/oom_score_adj (OOM_SCORE_ADJ_MIN) mới bảo vệ tiến trình không bao giờ bị OOM Killer giết, còn 0 là mức mặc định bình thường!",
                        buggyCode = "function isOomProtected(oomScoreAdj) {\n  return oomScoreAdj === 0;\n}",
                        bugLineNumber = 2,
                        bugBriefDescription = "Kiểm tra === 0 (mức mặc định) thay vì <= -1000!",
                        rootCauseAnalysis = "oom_score_adj = 0 là giá trị mặc định của mọi tiến trình thường; chỉ -1000 mới miễn tử.",
                        cleanFixedCode = "function isOomProtected(oomScoreAdj) {\n  return oomScoreAdj <= -1000;\n}",
                        requiredTokens = new[] { "-1000" },
                        functionName = "isOomProtected",
                        testCases = new object[]
                        {
                            new { name = "adj = -1000 -> true", input = new object[] { -1000 }, expected = true },
                            new { name = "adj = 0 -> false", input = new object[] { 0 }, expected = false }
                        },
                        explanation = "Đặt oom_score_adj = -1000 để bảo vệ tiến trình Database/Proxy trọng yếu."
                    }
                }
            }, now),
        MakeOsActivity(courseId, OsLesson53Id, 4, "js_sandbox",
            "Code Lab OS: Mô phỏng OOM Killer chọn Tiến trình Nạn nhân",
            "Viết hàm selectOomVictim(procs, totalRamMb) trả về PID có điểm oom_score cao nhất (bỏ qua tiến trình có adj <= -1000).",
            new
            {
                items = new object[]
                {
                    new
                    {
                        id = "os-l53-js-1",
                        difficulty = "Khó",
                        title = "Thuật toán chọn Victim của Linux OOM Killer",
                        description = "Mỗi phần tử trong procs có { pid, rssMb, adj }. Điểm = clamp(Math.round((rssMb / totalRamMb) * 1000) + adj, 0, 1000). Bỏ qua tiến trình có adj <= -1000. Trả về pid bị giết (hoặc -1 nếu không có).",
                        functionName = "selectOomVictim",
                        starterCode = "function selectOomVictim(procs, totalRamMb) {\n  let victimPid = -1, bestScore = -1;\n  // TODO: tính clamped score cho từng process không có adj <= -1000\n  return victimPid;\n}",
                        solutionCode = "function selectOomVictim(procs, totalRamMb) {\n  let victimPid = -1, bestScore = -1;\n  for (const p of procs) {\n    if (p.adj <= -1000) continue;\n    const raw = Math.round((p.rssMb / totalRamMb) * 1000) + p.adj;\n    const score = Math.max(0, Math.min(1000, raw));\n    if (score > bestScore) { bestScore = score; victimPid = p.pid; }\n  }\n  return victimPid;\n}",
                        hint = "Dùng Math.max(0, Math.min(1000, raw)) và bỏ qua p.adj <= -1000.",
                        testCases = new object[]
                        {
                            new { name = "Postgres (-1000) an toàn, Python (adj=0) bị chọn", input = new object[] { new object[] { new { pid = 101, rssMb = 1800, adj = -1000 }, new { pid = 202, rssMb = 900, adj = 0 } }, 2000 }, expected = 202 },
                            new { name = "Tất cả miễn tử -> -1", input = new object[] { new object[] { new { pid = 101, rssMb = 1800, adj = -1000 } }, 2000 }, expected = -1 }
                        }
                    }
                }
            }, now)
    };

    #endregion

    #region 3. Operating Systems Quizzes (6 Lesson Quizzes + [Đề Chương 4] + [Đề Chương 5])

    private static List<(string Key, Guid LessonId, string Title, QuizQuestionSpec[] Questions)> GetOsQuizSpecs() => new()
    {
        ("os:l41", OsLesson41Id, "Bài 4.1: Kiểm tra Hệ thống Tệp tin, Inode, Extent & Journaling", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Thông tin nào KHÔNG được lưu trữ bên trong cấu trúc Inode của hệ thống tệp POSIX (ext4)?", "Kích thước tệp và dấu thời gian (mtime)", "Tên tệp tin (Filename)", "Quyền truy cập (Permissions) và UID/GID", "Số liên kết cứng (i_links_count) và Extent Tree", "B", "Tên tệp nằm trong Directory Entry (Dentry), còn Inode chỉ lưu siêu dữ liệu và con trỏ khối dữ liệu."),
            new("multiple_choice", "Một máy chủ báo lỗi `No space left on device` dù `df -h` cho thấy còn 80 GB trống. Nguyên nhân phổ biến nhất là gì?", "Cạn kiệt Inode do chứa hàng triệu tệp cực nhỏ", "CPU bị quá tải", "RAM hết Page Cache", "TLB Miss 100%", "A", "Mỗi tệp chiếm 1 Inode; nếu số tệp nhỏ chạm trần Inode Table thì không thể tạo thêm tệp dù còn nhiều GB."),
            new("multiple_choice", "Lợi ích cốt lõi của cấu trúc Extent Tree trong `ext4` so với bảng con trỏ trực tiếp/gián tiếp của `ext3` là gì?", "Cho phép tên tệp dài hơn 1000 ký tự", "Biểu diễn dải khối liên tiếp bằng (Offset, Length, Start) giúp giảm mạnh metadata và phân mảnh", "Loại bỏ hoàn toàn nhu cầu dùng Superblock", "Chạy được trên Windows không cần driver", "B", "Một Extent 12 bytes biểu diễn tới 128 MB liên tiếp, giảm kích thước metadata và tăng tốc đọc/xóa."),
            new("cloze", "Cơ chế ghi nhật ký giao dịch trước khi cập nhật metadata lên vị trí thật để chống hỏng cấu trúc đĩa khi mất điện gọi là {{c1::Journaling}}.", null, null, null, null, "Journaling", "Journaling (như JBD2 trên ext4) áp dụng nguyên lý Write-Ahead Logging."),
            new("essay", "Giải thích tại sao sau khi chạy lệnh `rm app.log` trên Linux, dung lượng trống báo bởi `df -h` vẫn không tăng cho tới khi khởi động lại dịch vụ.", null, null, null, null,
                "Vì lệnh rm chỉ xóa Directory Entry và giảm i_links_count về 0, nhưng tiến trình ứng dụng vẫn đang mở File Descriptor trỏ tới Inode đó nên Kernel chưa thu hồi Data Blocks cho tới khi FD được đóng.",
                "Cần nêu rõ điều kiện giải phóng Inode: links_count == 0 VÀ open_fd_count == 0.",
                new[] { "Phân biệt xóa tên tệp trong Dentry với giải phóng Inode", "Nêu vai trò của File Descriptor đang mở từ tiến trình" })
        }),
        ("os:l42", OsLesson42Id, "Bài 4.2: Kiểm tra I/O, Page Cache, Direct I/O & NVMe Queue", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Trong Linux Page Cache, khi ứng dụng ghi dữ liệu qua `write()`, điều gì xảy ra ngay tại thời điểm hàm trả về thành công?", "Dữ liệu đã ghi xuống chip NAND SSD", "Dữ liệu mới được chép vào Dirty Pages trong RAM Page Cache và sẽ xả xuống đĩa sau", "Ổ đĩa tự chạy fsck", "Dữ liệu bị xóa khỏi bộ nhớ", "B", "Buffered write trả về ngay sau khi chép vào Page Cache; để đảm bảo độ bền vật lý phải gọi fsync()."),
            new("multiple_choice", "Tại sao các hệ quản trị CSDL như PostgreSQL hay InnoDB thường dùng `O_DIRECT`?", "Để bỏ qua Page Cache của OS, tránh nhân đôi dữ liệu (Double Buffering) và kiểm soát bộ nhớ đệm riêng", "Vì `O_DIRECT` không cần dùng CPU", "Để cho phép đọc tệp chưa tồn tại", "Để tăng mức phân mảnh đĩa", "A", "O_DIRECT truyền dữ liệu thẳng giữa SSD và User Buffer qua DMA, tránh lãng phí RAM vì Double Buffering."),
            new("multiple_choice", "Điểm đột phá giúp giao thức NVMe vượt xa SATA/AHCI trên CPU đa nhân là gì?", "Chỉ dùng 1 hàng đợi 32 lệnh toàn cục", "Hỗ trợ tới 64,000 hàng đợi song song (per-core) qua PCIe, loại bỏ khóa tranh chấp toàn cục", "Dùng đầu từ cơ học quay nhanh hơn", "Không cần kiểm tra lỗi CRC", "B", "Kiến trúc blk-mq ánh xạ hàng đợi riêng từng lõi CPU xuống các hàng đợi phần cứng NVMe."),
            new("cloze", "Cơ chế phần cứng cho phép thiết bị I/O truyền thẳng dữ liệu vào RAM mà không bắt CPU copy từng byte là {{c1::DMA}}.", null, null, null, null, "DMA", "Direct Memory Access (DMA) giải phóng CPU trong suốt quá trình truyền khối dữ liệu."),
            new("multiple_choice", "Trong đầu ra `iostat -xz 1`, chỉ số nào phản ánh trực tiếp thời gian trung bình một yêu cầu I/O phải chờ từ lúc xếp hàng tới khi xong?", "rkB/s", "%idle", "await", "avgrq-sz", "C", "await đo tổng thời gian phục vụ + chờ hàng đợi tính bằng mili-giây.")
        }),
        ("os:l43", OsLesson43Id, "Bài 4.3: Kiểm tra RAID, Copy-on-Write (ZFS / Btrfs) & Snapshots", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Với 4 ổ đĩa 8 TB chạy RAID 10, dung lượng khả dụng là bao nhiêu và Write Penalty bằng mấy?", "24 TB, Write Penalty = 4", "16 TB, Write Penalty = 2", "8 TB, Write Penalty = 1", "32 TB, Write Penalty = 6", "B", "RAID 10 ghép 2 cặp Mirror (dung lượng 50% = 16 TB) và chỉ cần ghi 2 bản Mirror nên Write Penalty = 2."),
            new("multiple_choice", "Mỗi thao tác ghi ngẫu nhiên khối nhỏ trên RAID 5 truyền thống tốn bao nhiêu I/O vật lý (Write Penalty)?", "1 I/O", "2 I/O", "4 I/O (2 Read + 2 Write)", "8 I/O", "C", "Phải đọc Old Data + Old Parity, tính XOR mới, rồi ghi New Data + New Parity = 4 I/O."),
            new("multiple_choice", "Lỗ hổng `RAID Write Hole` được giải quyết triệt để trong ZFS RAID-Z nhờ cơ chế nào?", "Không bao giờ sửa đè stripe cũ nhờ Copy-on-Write (ghi toàn bộ stripe mới ở vùng trống)", "Tắt hoàn toàn kiểm tra Parity", "Dùng ổ đĩa tốc độ 5400 RPM", "Chạy `chkdsk` mỗi phút", "A", "Copy-on-Write ghi dữ liệu + parity mới vào stripe trống rồi mới lật con trỏ Uberblock nguyên tử."),
            new("cloze", "Hiện tượng lỗi bit âm thầm trên đĩa cứng không báo lỗi phần cứng được ZFS phát hiện và tự sửa bằng Merkle Tree Checksum gọi là {{c1::Bit Rot}}.", null, null, null, null, "Bit Rot", "Bit Rot (Silent Data Corruption) được phát hiện nhờ lưu Checksum ở khối cha."),
            new("essay", "Giải thích tại sao tạo Snapshot trên hệ thống Copy-on-Write (ZFS/Btrfs) lại hoàn thành gần như tức thì (O(1)) và ban đầu tốn 0 byte dữ liệu phụ thêm.", null, null, null, null,
                "Vì Snapshot chỉ đóng băng và giữ lại con trỏ Root Tree hiện tại; mọi khối dữ liệu không thay đổi đều được dùng chung giữa bản chính và Snapshot, chỉ tốn thêm dung lượng khi có thao tác ghi mới tách nhánh CoW.",
                "Cần nêu giữ con trỏ Root Tree + chia sẻ Data Blocks không đổi.",
                new[] { "Giải thích cơ chế giữ lại Root Pointer / Metadata", "Nêu cách chia sẻ khối dữ liệu và cấp phát mới khi ghi đè" })
        }),
        ("os:l51", OsLesson51Id, "Bài 5.1: Kiểm tra Spinlock, Mutex, Lock-Free CAS & Priority Inversion", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Vì sao TUYỆT ĐỐI CẤM đưa luồng vào trạng thái ngủ (Mutex Sleep) bên trong Interrupt Handler của Kernel?", "Vì Interrupt Handler không thuộc ngữ cảnh một tiến trình có thể lập lịch lại, ngủ sẽ gây treo cứng/Kernel Panic", "Vì Interrupt Handler không có quyền đọc RAM", "Vì Spinlock chậm hơn Mutex 1000 lần", "Vì Mutex chỉ dùng được trong JavaScript", "A", "Interrupt context không có struct task_struct để chuyển trạng thái Blocked; bắt buộc dùng Spinlock tắt ngắt cục bộ."),
            new("multiple_choice", "Trên Linux, cơ chế `Futex` tối ưu hiệu năng cho `pthread_mutex` bằng cách nào?", "Luôn gọi System Call mỗi lần khóa", "Thực hiện trường hợp không tranh chấp bằng 1 lệnh Atomic CAS ở User Space (0 syscall), chỉ vào Kernel ngủ khi có đụng độ", "Dùng busy-wait vĩnh viễn", "Tắt hoàn toàn Cache L1", "B", "Fast Userspace Mutex tránh chuyển ngữ cảnh User->Kernel trong >95% trường hợp không có xung đột."),
            new("multiple_choice", "Lỗi kinh điển nào xảy ra khi con trỏ Lock-Free chuyển từ A -> B -> A khiến lệnh CAS nhận nhầm trạng thái chưa đổi?", "Thrashing", "ABA Problem", "Belady's Anomaly", "Convoy Effect", "B", "ABA Problem khiến CAS tưởng giá trị vẫn là A dù cấu trúc bên dưới đã bị thay đổi và giải phóng."),
            new("cloze", "Giao thức khắc phục nghịch lý Priority Inversion bằng cách tạm nâng độ ưu tiên của luồng giữ khóa lên ngang bằng luồng cao nhất đang chờ gọi là {{c1::Priority Inheritance}}.", null, null, null, null, "Priority Inheritance", "Priority Inheritance Protocol (PIP) ngăn luồng độ ưu tiên trung bình chen ngang cướp CPU."),
            new("multiple_choice", "Hiện tượng `False Sharing` xảy ra khi hai luồng trên hai lõi CPU khác nhau sửa hai biến độc lập nằm cùng ở đâu?", "Cùng một tệp trên ổ cứng", "Cùng một Cache Line 64 bytes trong CPU Cache", "Cùng một bảng trang ảo", "Cùng một cổng mạng TCP", "B", "Giao thức đồng bộ Cache MESI vô hiệu hóa cả Cache Line 64 bytes khi một biến bị ghi, gây Cache Miss liên tục.")
        }),
        ("os:l52", OsLesson52Id, "Bài 5.2: Kiểm tra Linux Namespaces, cgroups v2 & Container Isolation", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Linux Namespace nào giúp hai Container khác nhau cùng mở cổng TCP `:8080` trên một máy chủ Host mà không xung đột?", "PID Namespace", "NET Namespace (`CLONE_NEWNET`)", "UTS Namespace", "MNT Namespace", "B", "Network Namespace cấp cho mỗi Container một ngăn xếp mạng, IP và bảng cổng độc lập."),
            new("multiple_choice", "Trong cgroups v2, điều gì xảy ra khi một Container có giới hạn `cpu.max = 50000 100000` chạy 4 luồng cùng lúc trong 15ms đầu chu kỳ?", "Container được thưởng thêm CPU", "Tổng tiêu thụ 60,000us vượt 50,000us khiến Kernel đóng băng (throttle) Container suốt 85ms còn lại", "Container tự chuyển sang GPU", "Host tắt nguồn", "B", "CFS Bandwidth Control tính tổng thời gian CPU của mọi luồng trong cgroup theo chu kỳ 100ms."),
            new("multiple_choice", "Khi Container cố gắng xóa một tệp có sẵn trong lớp ảnh Read-Only (`lowerdir`) của OverlayFS, hệ thống tệp làm gì?", "Xóa vĩnh viễn tệp trên máy Host và ảnh hưởng mọi Container khác", "Tạo một tệp đánh dấu che khuất (whiteout file) trong `upperdir`", "Báo lỗi không hỗ trợ lệnh rm", "Sao chép toàn bộ hệ điều hành sang phân vùng khác", "B", "Whiteout trong upperdir ẩn tệp ở lowerdir riêng với Container đó mà không chạm tới lớp Read-Only dùng chung."),
            new("cloze", "Trong `cgroups v2`, khi tổng RAM tiêu thụ chạm ngưỡng trần cứng {{c1::memory.max}} mà không thể thu hồi thêm, cgroup OOM Killer sẽ chấm dứt tiến trình bên trong nhóm đó.", null, null, null, null, "memory.max", "memory.high là ngưỡng kìm hãm mềm; memory.max là trần cứng kích hoạt OOM Killer."),
            new("essay", "Tại sao người ta nói 'Container chỉ là tiến trình Linux bình thường được cô lập' chứ không phải Máy ảo (VM)? Hãy đối chiếu dựa trên Kernel, RAM và cơ chế cô lập.", null, null, null, null,
                "Container dùng chung trực tiếp Host Linux Kernel và chia sẻ Page Cache qua OverlayFS, chỉ dùng Namespaces để cô lập tầm nhìn và cgroups v2 để giới hạn tài nguyên; trong khi VM phải ảo hóa phần cứng qua Hypervisor và chạy một Guest Kernel độc lập.",
                "Cần đối chiếu dùng chung Host Kernel vs Guest Kernel riêng, Namespaces/cgroups vs Hypervisor.",
                new[] { "Chỉ ra Container dùng chung Host Kernel", "Nêu vai trò của Namespaces và cgroups v2", "So sánh với kiến trúc Hypervisor/Guest OS của VM" })
        }),
        ("os:l53", OsLesson53Id, "Bài 5.3: Kiểm tra eBPF, Page Faults, OOM Killer & Phân tích Latency", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Thành phần nào trong nhân Linux kiểm duyệt chương trình eBPF trước khi biên dịch JIT để đảm bảo không gây Kernel Panic?", "GCC Linker", "eBPF Verifier", "systemd-journald", "OOM Killer", "B", "eBPF Verifier kiểm tra tĩnh đảm bảo chương trình kết thúc hữu hạn, không truy cập con trỏ ngoài biên."),
            new("multiple_choice", "Khi tiến trình trong Kubernetes bị dừng đột ngột với `Exit Code 137`, tín hiệu nào đã được gửi tới nó?", "SIGTERM (15)", "SIGKILL (9)", "SIGSEGV (11)", "SIGINT (2)", "B", "128 + 9 = 137 tương ứng tín hiệu SIGKILL, thường do OOM Killer phát ra."),
            new("multiple_choice", "Tại sao `eBPF / bpftrace` phù hợp chạy trực tiếp trên Production hơn `strace`?", "Vì strace dùng ptrace dừng tiến trình 2 lần mỗi syscall làm chậm app hàng chục lần, còn eBPF tổng hợp ngay trong Kernel với overhead <1%", "Vì strace không đọc được syscall", "Vì eBPF chỉ chạy ở User Space", "Vì eBPF tự động sửa code ứng dụng", "A", "ptrace context switch overhead rất lớn; eBPF đọc trực tiếp qua tracepoints và eBPF Maps."),
            new("cloze", "Giá trị cấu hình trong `/proc/[pid]/oom_score_adj` giúp bảo vệ hoàn toàn một tiến trình như PostgreSQL khỏi bị OOM Killer bắn hạ là {{c1::-1000}}.", null, null, null, null, "-1000", "OOM_SCORE_ADJ_MIN = -1000 miễn trừ tiến trình khỏi danh sách nạn nhân của OOM Killer."),
            new("multiple_choice", "Trong `perf stat`, khi `minor-faults` tăng hàng triệu lần nhưng `major-faults` bằng 0, nguyên nhân khả dĩ nhất là gì?", "Ổ cứng sắp hỏng", "Ứng dụng cấp phát/chạm các vùng nhớ ẩn danh lớn liên tục (Lazy Allocation) trong RAM mà không cần đọc đĩa", "Mất kết nối mạng", "Lỗi Swap partition đầy", "B", "Minor page fault không cần đọc đĩa I/O mà chỉ xin Kernel gắn trang zero-fill trong RAM vào bảng trang.")
        }),

        // [Đề Chương 4] (10 balanced items, A/B/C/D balanced: 2 A, 2 B, 2 C, 2 D + 1 cloze + 1 essay)
        ("os:ch4", OsLesson43Id, "[Đề Chương 4] Tổng hợp Hệ thống Tệp tin, I/O, Page Cache, RAID & Copy-on-Write", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Cấu trúc nào trên ext4 lưu giữ danh sách địa chỉ vật lý các khối dữ liệu của một tệp tin nhưng KHÔNG lưu tên tệp đó?", "Inode", "Superblock", "Dentry Cache", "MBR Boot Sector", "A", "Inode lưu metadata và cây Extent/con trỏ khối, còn tên tệp nằm trong Directory Entry."),
            new("multiple_choice", "Khi phân vùng còn 40% dung lượng GB trống nhưng không thể tạo thêm tệp nhỏ nào mới (`ENOSPC`), bạn nên kiểm tra lệnh nào đầu tiên?", "df -i (kiểm tra tỷ lệ dùng Inode)", "free -m", "uname -a", "ping localhost", "A", "df -i hiển thị số lượng Inode đã dùng và còn trống trên phân vùng."),
            new("multiple_choice", "Mục đích chính của cơ chế `Ordered Journaling` (mặc định trên ext4) là gì?", "Nén tệp tin nhỏ hơn 50%", "Ghi dữ liệu thật xuống đĩa xong trước khi ghi Metadata Commit vào Journal, tránh lộ dữ liệu rác sau crash", "Tăng tốc CPU clock", "Bỏ qua việc kiểm tra quyền POSIX", "B", "Ordered mode bảo đảm Inode không bao giờ trỏ vào các khối chưa kịp ghi dữ liệu."),
            new("multiple_choice", "Vì sao các cơ sở dữ liệu lớn như PostgreSQL thường tắt hoặc vượt qua Linux Page Cache bằng `O_DIRECT` cho một số tác vụ?", "Để biến đĩa thành RAM", "Để tránh Double Buffering lãng phí RAM và kiểm soát thứ tự flush WAL chính xác", "Vì Linux Page Cache không hỗ trợ tệp lớn hơn 4 MB", "Để vô hiệu hóa DMA", "B", "Database tự quản lý Buffer Pool tối ưu cho B-Tree và cần kiểm soát fsync nghiêm ngặt."),
            new("multiple_choice", "Sự cải tiến quyết định giúp NVMe + `blk-mq` đạt hàng trăm nghìn IOPS vượt xa SATA/AHCI là gì?", "Dùng dây cáp dài hơn", "Tăng kích thước sector lên 64 MB", "Mở rộng từ 1 hàng đợi 32 lệnh có khóa chung thành hàng chục nghìn cặp hàng đợi song song theo từng lõi CPU", "Xóa bỏ hoàn toàn FTL trên SSD", "C", "Per-core queues trong blk-mq loại bỏ lock contention giữa các lõi CPU."),
            new("multiple_choice", "Nhược điểm chính của RAID 5 khi dùng các ổ cứng dung lượng cực lớn (16 TB+) là gì?", "Không đọc tuần tự được", "Không hỗ trợ hệ điều hành Linux", "Write Penalty = 4 và nguy cơ gặp lỗi đọc URE làm sụp đổ quá trình Rebuild kéo dài", "Dung lượng khả dụng bằng 0", "C", "Rebuild một ổ 16 TB mất nhiều ngày và xác suất gặp Unrecoverable Read Error rất cao."),
            new("multiple_choice", "Trong hệ thống tệp ZFS/Btrfs, kỹ thuật nào giúp tự phát hiện và chữa lành lỗi âm thầm Bit Rot trên ổ đĩa?", "Tắt bộ nhớ đệm CPU", "Tăng số lần chạy fsck thủ công", "Chuyển sang FAT32", "Lưu Checksum của khối con trong khối cha theo cây Merkle Tree kết hợp bản sao Mirror/Parity", "D", "End-to-end Merkle Tree Checksum phát hiện sai lệch ngay khi đọc và tự sửa từ bản sao."),
            new("multiple_choice", "Khi chạy Database (nhiều random write 8 KB) trên hệ thống tệp Copy-on-Write như Btrfs, hiện tượng gì dễ xảy ra nếu không tinh chỉnh?", "RAM tự động tăng gấp đôi", "Tệp bị mất tên", "Ổ đĩa tự chuyển thành RAID 0", "Phân mảnh nặng và khuếch đại ghi (Write Amplification) do không ghi đè tại chỗ", "D", "Có thể khắc phục bằng `nodatacow` hoặc khớp `recordsize` với page size của DB."),
            new("cloze", "Khi một tệp đã bị xóa tên bằng `unlink()` nhưng vẫn còn tiến trình giữ File Descriptor mở, dữ liệu chỉ được giải phóng khi số tham chiếu {{c1::open_fd_count}} và `links_count` đều bằng 0.", null, null, null, null, "open_fd_count", "Kernel giữ lại Inode ẩn danh chừng nào vẫn còn tiến trình mở tệp đó."),
            new("essay", "So sánh toàn diện cơ chế đảm bảo an toàn khi mất điện giữa `ext4 Ordered Journaling` và `ZFS Copy-on-Write (CoW)`.", null, null, null, null,
                "ext4 Ordered Journaling ghi Data Block trước rồi ghi Metadata vào Journal vòng trước khi Checkpoint vào vị trí thật (vẫn ghi đè tại chỗ ở bước Checkpoint nên cần replay Journal sau crash). ZFS Copy-on-Write không bao giờ ghi đè tại chỗ mà viết toàn bộ dữ liệu mới + cây metadata mới vào khối trống kèm Checksum, sau cùng lật con trỏ Uberblock nguyên tử, nên không bao giờ bị nửa vời hay Write Hole.",
                "Cần nêu được In-place Update + WAL của ext4 so với Never-overwrite + Atomic Uberblock flip của ZFS CoW.",
                new[] { "Mô tả quy trình Ordered Journaling và Checkpoint của ext4", "Mô tả cơ chế Copy-on-Write + Atomic pointer flip của ZFS", "Chỉ ra ưu thế chống RAID Write Hole và không cần fsck của CoW" })
        }),

        // [Đề Chương 5] (10 balanced items, A/B/C/D balanced: 2 A, 2 B, 2 C, 2 D + 1 cloze + 1 essay)
        ("os:ch5", OsLesson53Id, "[Đề Chương 5] Tổng hợp Đồng bộ hóa Nâng cao, Container & Hiệu năng eBPF", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Trên hệ thống chỉ có 1 lõi CPU (Single-Core) không tắt ngắt, vì sao Spinlock có thể gây treo máy (Deadlock)?", "Luồng B ưu tiên cao hơn quay vòng chiếm 100% CPU chờ khóa khiến luồng A đang giữ khóa không bao giờ được chạy để nhả khóa", "Vì Spinlock tốn quá nhiều RAM Heap", "Vì 1 lõi CPU không có thanh ghi", "Vì Spinlock tự gọi syscall sleep", "A", "Busy-waiting trên 1 lõi ngăn chủ sở hữu khóa tiếp tục thực thi tới lệnh Unlock."),
            new("multiple_choice", "Ưu điểm hiệu năng cốt lõi của cơ chế `Futex` dùng trong `std::mutex` / `pthread_mutex` trên Linux là gì?", "Khi không có tranh chấp, khóa và mở khóa thực hiện thuần túy bằng lệnh Atomic ở User Space mà không cần System Call", "Không bao giờ dùng tới RAM", "Tự động tăng xung nhịp CPU lên 10 GHz", "Biến mọi bài toán thành Single-thread", "A", "Futex chỉ gọi vào Kernel (`FUTEX_WAIT`) khi thực sự có luồng thứ hai tranh chấp khóa."),
            new("multiple_choice", "Để triệt tiêu lỗi `ABA Problem` trong hàng đợi/ngăn xếp Lock-Free dùng lệnh CAS, kỹ thuật phần cứng nào thường được áp dụng?", "Xóa toàn bộ con trỏ", "Gắn thêm bộ đếm phiên bản (Tagged Pointer / Version Counter) với Double-Width CAS", "Chuyển sang dùng đĩa cứng thay RAM", "Tắt bộ nhớ ảo", "B", "Tăng Version Counter sau mỗi lần thay đổi giúp phân biệt `(A, v1)` với `(A, v3)`."),
            new("multiple_choice", "Trong sự cố tàu vũ trụ Mars Pathfinder, giao thức nào đã sửa được lỗi luồng ưu tiên thấp giữ khóa làm treo luồng ưu tiên cao?", "Thuật toán Banker", "Priority Inheritance Protocol (Kế thừa độ ưu tiên)", "FIFO Disk Scheduling", "Belady Optimal", "B", "Nâng tạm độ ưu tiên của luồng giữ khóa lên ngang bằng luồng cao nhất đang chờ khóa."),
            new("multiple_choice", "Cặp thành phần hạt nhân Linux nào tạo nên xương sống cô lập 'tầm nhìn' và 'giới hạn tài nguyên' cho Docker Container?", "BIOS và Bootloader", "Ext4 và Swapfile", "Namespaces (PID, NET, MNT, USER...) và cgroups v2", "Hypervisor KVM và Guest BIOS", "C", "Namespaces cô lập góc nhìn hệ thống; cgroups v2 giới hạn CPU, RAM và I/O."),
            new("multiple_choice", "Tại sao chạy một tiến trình `node` hoặc `dotnet` trực tiếp làm `PID 1` bên trong Container mà không xử lý `SIGCHLD` có thể sinh ra Zombie?", "Vì Docker chặn toàn bộ tín hiệu", "Vì Container không có Namespace PID", "Vì `PID 1` có nghĩa vụ thu dọn các tiến trình con mồ côi qua `wait()`, nhưng app thông thường không làm việc này", "Vì cgroups v2 biến mọi tiến trình thành Zombie", "C", "Dùng `--init` (tini) làm PID 1 để tự động thu dọn tiến trình con kết thúc."),
            new("multiple_choice", "Công cụ nào cho phép chạy mã kiểm tra an toàn ngay trong nhân Linux để đo độ trễ I/O, syscall và hàm user-space với overhead < 1%?", "strace -f", "dd if=/dev/zero", "top -b", "eBPF (bpftrace / bcc)", "D", "eBPF Verifier xác minh an toàn rồi biên dịch JIT chạy thẳng tại tracepoints/kprobes trong Kernel."),
            new("multiple_choice", "Một pod Kubernetes bị tắt đột ngột với mã thoát `137` và `dmesg` báo `Memory cgroup out of memory`. Điều này có nghĩa là gì?", "Cả cụm máy chủ bị cúp điện", "Tiến trình trong pod chia 0 bị lỗi số học", "Lỗi mạng mất kết nối DNS", "Tiến trình chạm ngưỡng `memory.max` của cgroup và bị cgroup OOM Killer bắn hạ bằng `SIGKILL (9)`", "D", "Exit code 137 = 128 + 9 (SIGKILL) từ OOM Killer bên trong giới hạn của cgroup."),
            new("cloze", "Khi hai biến độc lập bị hai lõi CPU sửa đổi liên tục nhưng vô tình nằm chung trong một {{c1::Cache Line}} 64 bytes, hiệu năng bị sụt giảm nặng do hiện tượng False Sharing.", null, null, null, null, "Cache Line", "Giao thức MESI làm vô hiệu hóa cả Cache Line 64 bytes qua lại giữa các lõi."),
            new("essay", "Bạn nhận được cảnh báo một API trong Container có p50 = 10ms nhưng p99 vọt lên 1,800ms. Trình bày quy trình chẩn đoán theo USE Method dùng `cpu.stat`, `vmstat`, `iostat` và `bpftrace` để tìm ra nguyên nhân (CPU throttling vs Writeback I/O vs Lock contention).", null, null, null, null,
                "1) Kiểm tra cgroups v2 cpu.stat xem nr_throttled có tăng mạnh do chạm trần cpu.max không. 2) Chạy vmstat 1 và iostat -xz 1 soi cột wa (I/O Wait), b (blocked), si/so (Thrashing), await (độ trễ đĩa). 3) Dùng perf/bpftrace (offcputime, biolatency, futex tracepoints) xác định luồng đang ngủ chờ khóa Mutex/Futex hay chờ xả Dirty Page Cache.",
                "Cần kết hợp kiểm tra cgroups throttling, I/O/Swap saturation và eBPF/perf profiling.",
                new[] { "Kiểm tra CPU throttling qua cgroups v2 cpu.stat", "Kiểm tra I/O Wait, Swap và độ trễ đĩa qua vmstat/iostat", "Đề xuất dùng bpftrace/perf soi off-CPU time hoặc futex/I/O latency" })
        })
    };

    #endregion

    #region 4. Operating Systems (+75 Deterministic FSRS Flashcards: 15 Ch1..3 + 60 Ch4..5)

    private static async Task EnsureOsExpandedFlashcardsAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var tag = await EnsureTagAsync(db, "Operating-Systems", "blue", now);
        var l11 = AcademicOsPracticeContent.OsLesson11Id;
        var l12 = AcademicOsPracticeContent.OsLesson12Id;
        var l21 = AcademicOsPracticeContent.OsLesson21Id;
        var l22 = AcademicOsPracticeContent.OsLesson22Id;
        var l33 = SeedDataInitializer.LessonPageReplacementId;
        var l41 = OsLesson41Id;
        var l42 = OsLesson42Id;
        var l43 = OsLesson43Id;
        var l51 = OsLesson51Id;
        var l52 = OsLesson52Id;
        var l53 = OsLesson53Id;

        var specs = new (Guid LessonId, string Front, string Back)[]
        {
            // Ch1..Ch3 Core Reinforcement (15 cards)
            (l11, "Trong không gian địa chỉ ảo của Process, vùng BSS lưu trữ loại dữ liệu gì và vì sao tiết kiệm kích thước file nhị phân?", "BSS lưu biến toàn cục và biến static CHƯA khởi tạo giá trị; Kernel chỉ ghi kích thước vùng và tự điền 0 khi nạp vào RAM thay vì lưu hàng loạt byte 0 trong file thực thi."),
            (l11, "Sự khác nhau giữa Tiến trình Zombie (<defunct>) và Tiến trình mồ côi (Orphan Process) trên POSIX là gì?", "Zombie là tiến trình con đã chết nhưng cha chưa gọi wait() để đọc exit code (chiếm giữ PID). Orphan là tiến trình con còn sống khi cha chết trước, được Kernel giao cho PID 1 (init/systemd) nhận nuôi."),
            (l11, "Ưu điểm và nhược điểm của mô hình Threading M:N (như Go Goroutines hoặc Java Virtual Threads) là gì?", "Ghép M luồng người dùng lên N luồng Kernel giúp tạo hàng triệu luồng siêu nhẹ, chuyển đổi ở User Space cực nhanh và vẫn tận dụng đa lõi CPU thông qua Work-Stealing Scheduler."),
            (l12, "Vì sao thuật toán điều phối SJF/SRTF đạt Average Turnaround Time tối ưu nhưng khó dùng thuần túy trong CPU Scheduler thực tế?", "Vì Kernel không thể biết chính xác CPU Burst Time tương lai của tiến trình, và các tiến trình dài dễ bị bỏ đói (Starvation) nếu liên tục có tiến trình ngắn chen vào."),
            (l12, "Trong thuật toán Round Robin, việc chọn Time Quantum q quá lớn sẽ gây ra hậu quả gì?", "Round Robin thoái hóa thành FCFS, làm Response Time của các tác vụ tương tác (UI/API ngắn) tăng vọt vì phải đợi quá lâu tới lượt."),
            (l12, "Quy tắc hạ bậc và nâng bậc của thuật toán điều phối MLFQ (Multi-Level Feedback Queue) hoạt động ra sao?", "Tiến trình mới vào hàng đợi cao nhất; nếu dùng hết quantum bị hạ xuống hàng đợi thấp hơn (CPU-bound), nếu tự nhả CPU chờ I/O sớm được giữ nguyên cấp (I/O-bound); định kỳ Priority Boost đưa tất cả lên đỉnh để chống Starvation."),
            (l21, "Sự khác biệt giữa Race Condition và Data Race trong lập trình đồng thời là gì?", "Data Race xảy ra khi >=2 luồng truy cập cùng vùng nhớ không đồng bộ và có ít nhất 1 lệnh ghi. Race Condition là lỗi logic phụ thuộc thứ tự thời gian thực thi (có thể vẫn xảy ra dù từng thao tác đơn lẻ đã có Mutex!)."),
            (l21, "Cặp Condition Variable (`pthread_cond_wait`) bắt buộc phải đi kèm với yếu tố nào và dùng vòng lặp gì khi kiểm tra điều kiện?", "Bắt buộc đi kèm 1 Mutex và luôn đặt điều kiện trong vòng lặp `while (!condition)` (thay vì `if`) để chống đánh thức giả (Spurious Wakeup) hoặc bị luồng khác cướp điều kiện trước khi tỉnh dậy."),
            (l21, "Reader-Writer Lock (`rwlock`) tối ưu cho kịch bản nào và có nguy cơ gì nếu không cẩn thận?", "Cho phép nhiều Reader đọc song song khi không có Writer; nhưng nếu Reader đến liên tục không ngớt, Writer có thể bị đói tài nguyên (Writer Starvation) trừ khi bật ưu tiên Writer."),
            (l22, "Bốn chiến lược chính để đối phó với Deadlock trong Hệ điều hành là gì?", "1) Ignore (Ostrich Algorithm); 2) Prevention (phá vỡ 1 trong 4 điều kiện Coffman như Lock Ordering); 3) Avoidance (thuật toán Banker giữ Safe State); 4) Detection & Recovery (phát hiện chu trình Wait-For Graph và hủy tiến trình)."),
            (l22, "Khác biệt giữa Deadlock và Livelock trong hệ thống đa luồng là gì?", "Deadlock: các luồng đều bị Blocked đứng yên chờ nhau mãi mãi. Livelock: các luồng vẫn đang chạy (Running), liên tục thay đổi trạng thái hoặc nhường nhau (như 2 người tránh nhau ở hành lang) nhưng không luồng nào tiến triển được."),
            (l22, "Trong thuật toán Banker, công thức tính ma trận Need[i] là gì và khi nào tiến trình i được phép mô phỏng chạy xong?", "Need[i] = Max[i] - Allocation[i]. Tiến trình i được giả lập hoàn tất khi Need[i] <= Work, sau đó giải phóng Work = Work + Allocation[i]."),
            (l33, "Khi trang nạn nhân (Victim Page) bị chọn để đẩy ra đĩa, bit phần cứng nào trong PTE quyết định có phải tốn công ghi ra Swap Space hay không?", "Bit Dirty (Modified bit M). Nếu M=1 trang đã bị sửa nên phải ghi ra đĩa (hoặc Swap); nếu M=0 trang vẫn giống bản trên đĩa nên ghi đè trực tiếp không tốn I/O ghi."),
            (l33, "Thuật toán Enhanced Second-Chance (Clock cải tiến) phân 4 lớp ưu tiên (R, M) theo thứ tự chọn nạn nhân như thế nào?", "Ưu tiên đẩy trước: (0,0) chưa dùng & sạch -> (0,1) chưa dùng & bẩn -> (1,0) mới dùng & sạch -> (1,1) mới dùng & bẩn."),
            (l33, "Nguyên lý cốt lõi của hiện tượng Thrashing và dấu hiệu nhận biết trên `vmstat 1` là gì?", "Tổng Working Set các tiến trình vượt quá RAM vật lý khiến OS hoán đổi trang liên tục. Trên vmstat: cột `si`/`so` (swap in/out) và `wa` (I/O wait) tăng vọt trong khi `%us` tụt thê thảm."),

            // Chapter 4 - Lesson 4.1: Inode, Extent, B-Tree & Journaling (10 cards)
            (l41, "Cấu trúc Inode trên ext4 lưu giữ những thông tin nào và KHÔNG lưu thông tin nào?", "Inode lưu kích thước tệp, UID/GID, quyền rwx, timestamps, i_links_count và cây Extent/con trỏ khối dữ liệu. Inode KHÔNG lưu tên tệp (tên tệp nằm trong Directory Entry)."),
            (l41, "Tại sao phân vùng Linux có thể báo lỗi `ENOSPC` (No space left on device) dù `df -h` vẫn còn hàng chục GB trống?", "Vì phân vùng đã cạn kiệt bảng Inode (`df -i` đạt 100%) do lưu quá nhiều tệp tin cực nhỏ, mỗi tệp chiếm 1 Inode."),
            (l41, "Hard Link (`ln`) và Symbolic Link (`ln -s`) khác nhau ra sao ở tầng Inode?", "Hard Link trỏ thẳng tới cùng một số Inode trong cùng phân vùng (tăng i_links_count); xóa file gốc dữ liệu vẫn còn. Symbolic Link có Inode riêng chứa chuỗi đường dẫn văn bản, có thể trỏ xuyên phân vùng và bị gãy (dangling) nếu xóa file gốc."),
            (l41, "Điều kiện để nhân Linux thực sự thu hồi Data Blocks sau khi gọi lệnh `rm` (unlink) một tệp tin là gì?", "Cả hai bộ đếm phải đồng thời bằng 0: `i_links_count == 0` (không còn Hard Link nào) VÀ `open_fd_count == 0` (không còn tiến trình nào mở File Descriptor trỏ tới tệp đó)."),
            (l41, "Cấu trúc Extent trong ext4 khắc phục nhược điểm gì của bảng con trỏ Direct/Indirect trên ext2/ext3?", "Thay vì lưu từng số hiệu block 4 KB rời rạc, mỗi Extent 12 bytes lưu bộ ba (Logical Offset, Length, Physical Start) biểu diễn tới 128 MB liên tiếp, giảm mạnh kích thước metadata và phân mảnh."),
            (l41, "Superblock trong hệ thống tệp `ext4` chứa thông tin gì và tại sao được sao lưu ở nhiều Block Group?", "Chứa cấu hình cốt lõi toàn phân vùng (block size, tổng số block/inode, magic number 0xEF53, trạng thái clean/dirty). Nếu sector đầu đĩa hỏng, `fsck` khôi phục được từ bản sao ở các Block Group sau."),
            (l41, "Mục đích của lớp VFS (Virtual File System) trong nhân Linux là gì?", "Cung cấp 4 giao diện trừu tượng thống nhất (`super_block`, `inode`, `dentry`, `file`) để syscall của ứng dụng chạy đồng nhất trên ext4, XFS, Btrfs, NFS hay tmpfs."),
            (l41, "Ba chế độ Journaling của `ext4` (`journal`, `ordered`, `writeback`) khác nhau như thế nào?", "`journal`: ghi cả Data+Metadata vào Journal (chậm 2x nhưng an toàn nhất). `ordered` (mặc định): bắt buộc ghi Data xong trước khi Commit Metadata vào Journal. `writeback`: chỉ ghi Metadata vào Journal, không ép thứ tự Data."),
            (l41, "Hệ thống tệp NTFS của Windows và APFS của Apple dùng cấu trúc cốt lõi nào để lập chỉ mục tệp và thư mục?", "NTFS lưu bản ghi tệp trong Master File Table (`$MFT`) dùng Data Runs cho khối dữ liệu và B-Tree cho thư mục lớn. APFS dùng cây B-Tree cho Extent Records kết hợp Copy-on-Write hỗ trợ Clone tức thì."),
            (l41, "Cách phát hiện một tệp đã bị `rm` nhưng vẫn đang bị tiến trình giữ chiếm dung lượng đĩa trên Linux?", "Dùng lệnh `lsof +L1` (liệt kê các tệp mở có số links < 1, hiện chữ `(deleted)`) hoặc tìm symlink trong `/proc/<pid>/fd/`."),

            // Chapter 4 - Lesson 4.2: Page Cache, Direct I/O & Disk Scheduling (10 cards)
            (l42, "Linux Page Cache là gì và tại sao cột `buff/cache` trong lệnh `free -h` thường chiếm phần lớn RAM trống?", "Linux tận dụng RAM nhàn rỗi làm bộ đệm đọc/ghi khối tệp (Clean pages có thể thu hồi tức thì khi app cần RAM). Nhờ vậy truy cập tệp nóng đạt tốc độ RAM thay vì chạm ổ đĩa."),
            (l42, "Sự khác biệt giữa `write()` thông thường (Buffered I/O) và `fsync(fd)` đối với độ bền dữ liệu (Durability) là gì?", "`write()` chỉ chép dữ liệu vào Dirty Pages trong RAM Page Cache rồi trả về ngay. `fsync()` ép Kernel xả toàn bộ Data lẫn Metadata của tệp đó xuống bộ nhớ không bay hơi của ổ đĩa và chờ phần cứng xác nhận."),
            (l42, "Hai tham số `vm.dirty_background_ratio` và `vm.dirty_ratio` trong Linux kiểm soát điều gì?", "Vượt `dirty_background_ratio` (~10%), luồng nền `flush` âm thầm đẩy Dirty Pages xuống đĩa. Chạm trần `dirty_ratio` (~20%), Kernel chặn đứng tiến trình đang gọi `write()` ép xả đồng bộ -> dễ gây latency spike!"),
            (l42, "Tại sao DBMS như PostgreSQL hay MySQL InnoDB dùng cờ `O_DIRECT` (Direct I/O)?", "Bỏ qua Page Cache của OS, dùng DMA truyền thẳng giữa ổ đĩa và Buffer Pool của DBMS để tránh Double Buffering (nhân đôi dữ liệu trong RAM) và tự kiểm soát chính sách thay thế trang."),
            (l42, "Ưu điểm và rủi ro của Memory-Mapped File (`mmap`) so với `read()/write()` là gì?", "Ưu điểm: Zero-copy, không tốn syscall đọc/ghi và chia sẻ bộ nhớ giữa nhiều tiến trình. Rủi ro: lỗi I/O đĩa hoặc file bị cắt ngắn gây tín hiệu `SIGBUS`, và khó kiểm soát thời điểm flush."),
            (l42, "Cơ chế DMA (Direct Memory Access) giúp tối ưu CPU như thế nào trong quá trình đọc/ghi đĩa?", "DMA Controller truyền thẳng hàng MB dữ liệu giữa bộ điều khiển thiết bị (NVMe/NIC) và RAM qua bus PCIe mà không cần CPU thực thi lệnh copy từng byte, chỉ gửi 1 ngắt khi hoàn tất."),
            (l42, "Thuật toán lập lịch đĩa HDD `C-SCAN` (Circular SCAN) khắc phục nhược điểm gì của `SSTF` và `SCAN`?", "SSTF dễ làm đói (Starvation) các track rìa ngoài; SCAN đảo chiều làm các track ở giữa được thăm gấp đôi rìa. C-SCAN chỉ phục vụ theo 1 chiều tăng dần rồi quay về 0 giúp thời gian chờ công bằng đều nhau."),
            (l42, "Tại sao `NVMe` kết hợp tầng `blk-mq` (Multi-Queue) nhanh hơn gấp nhiều lần so với giao thức `SATA/AHCI`?", "AHCI chỉ có 1 hàng đợi tối đa 32 lệnh dùng chung khóa Spinlock toàn cục. NVMe qua PCIe hỗ trợ 64,000 hàng đợi x 64,000 lệnh, cấp hàng đợi riêng cho từng lõi CPU loại bỏ hoàn toàn tranh chấp khóa."),
            (l42, "Hiện tượng `Write Amplification` trên ổ SSD NAND Flash là gì và lệnh `TRIM` (`fstrim`) giúp ích gì?", "SSD không ghi đè từng trang 4 KB mà phải đọc-sửa-xóa cả Erase Block (4-8 MB). `TRIM` báo cho bộ điều khiển FTL biết các khối nào OS đã xóa file để FTL dọn rác (Garbage Collection) trước mà không phải di chuyển dữ liệu rác."),
            (l42, "Ý nghĩa của các cột `await`, `aqu-sz` và `%util` khi chạy `iostat -xz 1` để chẩn đoán nghẽn đĩa là gì?", "`await` (ms) là tổng thời gian chờ + phục vụ 1 I/O; `aqu-sz` là độ dài hàng đợi trung bình; `%util` là % thời gian thiết bị bận. Với NVMe đa kênh, `await` tăng vọt mới là dấu hiệu bão hòa thực sự."),

            // Chapter 4 - Lesson 4.3: RAID, Copy-on-Write & Crash Consistency (10 cards)
            (l43, "So sánh dung lượng khả dụng và Write Penalty của RAID 0, RAID 1, RAID 5, RAID 6 và RAID 10 (với N đĩa, mỗi đĩa S)?", "RAID 0: N*S (WP=1, chịu 0). RAID 1: S (WP=2, chịu N-1). RAID 5: (N-1)*S (WP=4, chịu 1). RAID 6: (N-2)*S (WP=6, chịu 2). RAID 10: (N/2)*S (WP=2, chịu >=1 đĩa khác cặp)."),
            (l43, "Giải thích tại sao Write Penalty của RAID 5 đối với thao tác ghi ngẫu nhiên lại bằng 4?", "Để cập nhật 1 block mà không đọc cả stripe, hệ thống phải: (1) Đọc Data cũ, (2) Đọc Parity cũ, tính Parity mới = P_old ^ D_old ^ D_new, rồi (3) Ghi Data mới và (4) Ghi Parity mới."),
            (l43, "Tại sao RAID 5 không còn được khuyến nghị cho các ổ đĩa HDD/SSD dung lượng lớn (>= 8-16 TB)?", "Vì thời gian Rebuild kéo dài hàng ngày trời và xác suất gặp lỗi đọc không thể phục hồi (URE ~ 10^-14) trên các ổ còn lại rất cao, dễ làm hỏng hoàn toàn quá trình Rebuild."),
            (l43, "Lỗ hổng `RAID Write Hole` là gì?", "Khi mất điện đúng lúc đã ghi xong Data mới vào một đĩa nhưng chưa kịp ghi Parity mới sang đĩa kia, stripe bị lệch pha không khớp Parity; nếu 1 đĩa chết sau đó sẽ khôi phục ra dữ liệu hỏng."),
            (l43, "Nguyên lý hoạt động cốt lõi của kiến trúc `Copy-on-Write (CoW)` trên ZFS và Btrfs là gì?", "Tuyệt đối không ghi đè dữ liệu tại chỗ; luôn ghi dữ liệu và cây metadata mới sang các khối trống kèm Checksum, sau cùng lật con trỏ gốc (Uberblock) bằng một thao tác nguyên tử."),
            (l43, "Tại sao hệ thống tệp CoW (ZFS/Btrfs) không cần chạy `fsck` kéo dài hàng giờ sau khi mất điện?", "Vì cây dữ liệu cũ luôn nguyên vẹn cho tới khi Uberblock mới ghi xong hoàn toàn; khi khởi động lại, OS chỉ việc trỏ về bản Uberblock hợp lệ gần nhất trong vài mili-giây."),
            (l43, "Cơ chế tạo Snapshot trong ZFS/Btrfs hoạt động như thế nào và tiêu tốn bao nhiêu dung lượng khi mới tạo?", "Chỉ đóng băng con trỏ Root Tree hiện tại trong O(1) (<10ms) và chia sẻ 100% Data Blocks với hệ thống sống -> ban đầu tốn gần như 0 byte, chỉ tốn thêm khi có ghi mới tách nhánh."),
            (l43, "Làm thế nào ZFS phát hiện và tự chữa lỗi `Bit Rot` (Silent Data Corruption) mà RAID phần cứng thông thường bỏ sót?", "ZFS lưu Checksum của khối con nằm ở trong khối cha (Merkle Tree). Khi đọc thấy lệch Checksum, ZFS tự lấy bản sạch từ Mirror hoặc tái tạo từ Parity RAID-Z và ghi sửa lại khối hỏng (Self-Healing)."),
            (l43, "Vì sao không nên để các tệp Database ghi ngẫu nhiên liên tục trên CoW mặc định mà không tinh chỉnh, và cách xử lý là gì?", "Ghi ngẫu nhiên 8 KB trên CoW gây phân mảnh nặng và Write Amplification. Có thể tắt CoW riêng cho thư mục DB trên Btrfs (`chattr +C` / `nodatacow`) hoặc chỉnh `recordsize` trên ZFS khớp page size của DB."),
            (l43, "Tại sao luôn phải giữ tối thiểu 10-15% dung lượng trống trên các Storage Pool dùng Copy-on-Write (ZFS/Btrfs)?", "Vì trên CoW, ngay cả lệnh xóa (`rm`) hay cắt tệp cũng cần cấp phát một transaction metadata mới trước khi giải phóng khối cũ; khi đầy 100% có thể bị kẹt không xóa nổi file!"),

            // Chapter 5 - Lesson 5.1: Spinlock vs Mutex, Lock-Free CAS & Priority Inversion (10 cards)
            (l51, "Sự khác biệt cơ bản giữa Spinlock và Mutex khi không giành được khóa là gì?", "Spinlock giữ CPU quay vòng kiểm tra liên tục (busy-wait, tốn 100% CPU nhưng không context switch); Mutex đưa luồng vào hàng đợi ngủ (Blocked/Sleep, nhường CPU nhưng tốn chi phí chuyển ngữ cảnh)."),
            (l51, "Khi nào bắt buộc dùng Spinlock thay vì Mutex, và khi nào tuyệt đối CẤM dùng Spinlock?", "Bắt buộc dùng Spinlock trong Interrupt Handler hoặc vùng tranh chấp cực ngắn (<100ns) trên hệ đa nhân. CẤM dùng Spinlock trên hệ 1 lõi không tắt ngắt hoặc bên trong vùng có thao tác I/O / Sleep."),
            (l51, "Kỹ thuật `Test-and-Test-and-Set (TTAS)` cải tiến `Test-and-Set` thuần túy bằng cách nào?", "Đọc kiểm tra trạng thái khóa bằng lệnh LOAD thông thường trên Cache L1 nội bộ trước; chỉ phát lệnh nguyên tử `LOCK XCHG` đắt đỏ khi thấy khóa đã chuyển về 0, giảm nghẽn Cache Coherence."),
            (l51, "Lệnh `Futex` (Fast Userspace Mutex) trên Linux giúp `pthread_mutex` chạy nhanh như thế nào?", "Trường hợp không tranh chấp (uncontended) được xử lý 100% ở User Space bằng 1 lệnh Atomic CAS trong vài nano-giây (0 syscall); chỉ gọi `futex(FUTEX_WAIT)` vào Kernel ngủ khi có tranh chấp thực sự."),
            (l51, "Nguyên lý của thao tác nguyên tử `Compare-And-Swap (CAS)` là gì?", "So sánh giá trị tại địa chỉ nhớ `*addr` với `expected`; nếu khớp thì ghi `new_val` và trả `true`, ngược lại không ghi gì và trả `false` — toàn bộ diễn ra nguyên tử không thể bị ngắt bởi phần cứng CPU."),
            (l51, "Mô tả `ABA Problem` trong lập trình Lock-Free và hai cách phòng tránh chuẩn công nghiệp.", "Luồng 1 đọc con trỏ A rồi bị ngắt; luồng 2 pop A, giải phóng B, rồi push lại một nút nằm đúng địa chỉ A; luồng 1 tỉnh dậy thấy vẫn là A nên CAS thành công nhầm vào vùng nhớ hỏng. Khắc phục bằng Tagged Pointer (Version Counter) hoặc Hazard Pointers / EBR."),
            (l51, "Khác biệt giữa thuật toán `Lock-Free` và `Wait-Free` là gì?", "Lock-Free đảm bảo toàn hệ thống luôn có ít nhất 1 luồng tiến triển trong hữu hạn bước. Wait-Free mạnh hơn: đảm bảo MỌI luồng đều hoàn thành trong hữu hạn bước bất kể lịch trình lập lịch."),
            (l51, "Hiện tượng `Priority Inversion` là gì (như sự cố Mars Pathfinder)?", "Luồng ưu tiên CAO bị chặn chờ khóa đang do luồng ưu tiên THẤP giữ, trong khi một luồng ưu tiên TRUNG BÌNH cướp mất CPU của luồng thấp khiến luồng thấp không chạy tới chỗ nhả khóa được."),
            (l51, "Hai giao thức phòng chống Priority Inversion phổ biến trên Kernel và RTOS là gì?", "1) Priority Inheritance Protocol (PIP): tạm nâng độ ưu tiên của luồng giữ khóa lên bằng luồng cao nhất đang chờ. 2) Priority Ceiling Protocol (PCP): nâng luôn độ ưu tiên lên mức trần của tài nguyên ngay khi lấy khóa."),
            (l51, "Hiện tượng `False Sharing` là gì và cách khắc phục trong C/C++/Rust?", "Hai luồng trên hai lõi khác nhau ghi vào hai biến độc lập nhưng nằm chung một Cache Line (64 bytes), làm giao thức MESI liên tục vô hiệu hóa Cache của nhau. Khắc phục bằng padding / `alignas(64)` để tách ra hai Cache Line riêng."),

            // Chapter 5 - Lesson 5.2: Linux Namespaces, cgroups v2 & Container Isolation (10 cards)
            (l52, "Bản chất của Linux Container dưới góc nhìn Hệ điều hành là gì (so với Máy ảo VM)?", "Container không phải VM có Guest Kernel riêng; nó là các tiến trình Linux thông thường dùng chung Host Linux Kernel, được cô lập tầm nhìn bằng Namespaces và giới hạn tài nguyên bằng cgroups v2."),
            (l52, "Vai trò của 6 Linux Namespaces chính (`PID`, `NET`, `MNT`, `UTS`, `IPC`, `USER`) là gì?", "PID: đánh số PID riêng (PID 1). NET: giao diện mạng/IP/port riêng. MNT: cây thư mục gốc riêng. UTS: hostname riêng. IPC: cô lập Shared Memory/Semaphores. USER: ánh xạ root UID 0 trong container thành user thường ngoài Host."),
            (l52, "Tại sao ứng dụng chạy làm `PID 1` trong Container dễ gây tích tụ Zombie Process nếu thiếu `--init` (`tini`)?", "Vì theo quy tắc POSIX, PID 1 phải gọi `wait()` thu dọn mọi tiến trình con mồ côi kết thúc; các runtime app thông thường không cài trình bắt `SIGCHLD` cho các tiến trình con phát sinh bất ngờ."),
            (l52, "Sự khác biệt kiến trúc giữa `cgroups v1` và `cgroups v2` là gì?", "cgroups v1 tách rời nhiều cây phân cấp theo từng tài nguyên (dễ xung đột giữa CPU và Memory); cgroups v2 hợp nhất thành một cây phân cấp duy nhất (`/sys/fs/cgroup`) hỗ trợ kế toán đồng bộ Page Cache writeback và PSI."),
            (l52, "Ý nghĩa của `cpu.max = 50000 100000` trong cgroups v2 và bẫy CPU Throttling đối với app đa luồng?", "Cho phép tiêu thụ tối đa 50,000 us CPU time trong mỗi chu kỳ 100,000 us (0.5 vCPU). Nếu 4 luồng chạy song song trong 15 ms (tốn 60,000 us), cả Container bị đóng băng 85 ms còn lại gây tăng vọt p99 latency!"),
            (l52, "Phân biệt hai ngưỡng bộ nhớ `memory.high` và `memory.max` trong cgroups v2?", "`memory.high` là ngưỡng mềm: vượt qua sẽ bị kìm hãm cấp phát và ép Direct Reclaim Page Cache. `memory.max` là trần cứng: vượt quá không thu hồi được sẽ kích hoạt cgroup OOM Killer giết tiến trình."),
            (l52, "Cơ chế hoạt động của `OverlayFS` (`lowerdir`, `upperdir`, `merged`) giúp tiết kiệm đĩa và RAM cho Container ra sao?", "Các Image layers (`lowerdir`) ở chế độ Read-Only được dùng chung trên đĩa và Page Cache giữa hàng trăm Container; thay đổi riêng nằm ở `upperdir` theo cơ chế Copy-Up khi sửa file cũ."),
            (l52, "Khi Container xóa một tệp có sẵn trong lớp Read-Only `lowerdir` của OverlayFS, điều gì thực sự diễn ra?", "OverlayFS không sửa `lowerdir` mà tạo ra một tệp đánh dấu đặc biệt gọi là Whiteout (`.wh.<filename>` hoặc char device 0/0) trong `upperdir` để che khuất tệp đó."),
            (l52, "Hai cơ chế bảo mật hạt nhân nào thu hẹp quyền lực của tiến trình bên trong Container ngay cả khi chạy với tư cách root?", "`Linux Capabilities` (chia nhỏ và tước bỏ quyền như CAP_SYS_ADMIN, CAP_SYS_MODULE) và `Seccomp-BPF` (lọc và chặn hơn 44 System Call nguy hiểm trước khi vào Kernel)."),
            (l52, "Lệnh nào cho phép kỹ sư đứng từ máy Host bước vào đúng Network Namespace của một Container (PID 4512) để debug mạng bằng `ss`/`tcpdump`?", "Dùng lệnh `nsenter -t 4512 -n ss -tulpn` mà không cần cài đặt thêm công cụ debug vào bên trong Image Container."),

            // Chapter 5 - Lesson 5.3: eBPF, Page Faults, OOM Killer & Latency Analysis (10 cards)
            (l53, "Công nghệ `eBPF` trong nhân Linux là gì và tại sao nó an toàn hơn viết Kernel Module truyền thống?", "eBPF cho phép nạp bytecode vào các hook của Kernel (tracepoints/kprobes/XDP) sau khi được `eBPF Verifier` kiểm duyệt tĩnh (không vòng lặp vô tận, không con trỏ ngoài biên) rồi biên dịch JIT thành mã máy gốc."),
            (l53, "Phân biệt 4 loại điểm móc (Hooks) phổ biến của eBPF: `tracepoints`, `kprobes`, `uprobes`, và `XDP`?", "tracepoints: điểm đo ổn định gắn sẵn trong Kernel; kprobes: móc vào mọi hàm nội bộ Kernel; uprobes: móc vào hàm User Space; XDP: xử lý gói tin ngay tại Driver NIC trước khi cấp phát sk_buff."),
            (l53, "Tại sao không nên chạy `strace -f -p <pid>` lên dịch vụ đang tải cao trên Production?", "Vì `strace` dùng `ptrace()` ép Kernel dừng app và chuyển ngữ cảnh 2 lần cho MỖI System Call (entry & exit), có thể làm ứng dụng chậm đi 10x - 50x."),
            (l53, "Ba trục của phương pháp luận phân tích hiệu năng `USE Method` (Brendan Gregg) gồm những gì?", "Kiểm tra mọi tài nguyên (CPU, Memory, Disk I/O, Network) theo 3 chỉ số: Utilization (% bận), Saturation (độ dài hàng đợi/chờ đợi), và Errors (số lượng lỗi)."),
            (l53, "Ý nghĩa của các cột quan trọng `r`, `b`, `si`, `so`, `cs`, `wa` trong lệnh `vmstat 1` là gì?", "`r`: tiến trình Ready chờ CPU; `b`: tiến trình Blocked chờ I/O; `si/so`: lưu lượng Swap-In/Swap-Out (dấu hiệu Thrashing); `cs`: Context Switches/giây; `wa`: % CPU nhàn rỗi chờ I/O đĩa."),
            (l53, "Khi tiến trình nhận tín hiệu `SIGKILL (9)` từ OOM Killer, mã thoát (Exit Code) là bao nhiêu và kiểm chứng ở đâu?", "Exit Code = 128 + 9 = 137. Kiểm chứng bằng `dmesg -T` tìm `Out of memory: Killed process` / `Memory cgroup out of memory`, hoặc xem `memory.events` (`oom_kill`) trong cgroups v2."),
            (l53, "Linux OOM Killer tính điểm `oom_score` như thế nào và làm sao bảo vệ tuyệt đối một tiến trình như PostgreSQL?", "Dựa trên tỷ lệ phần trăm RAM (RSS + Swap) tiến trình chiếm giữ (0..1000) cộng với `oom_score_adj` (-1000..+1000). Ghi `-1000` vào `/proc/<pid>/oom_score_adj` để miễn tử hoàn toàn."),
            (l53, "Trong `perf stat`, khi `minor-faults` rất cao nhưng `major-faults` bằng 0, điều gì đang xảy ra trong bộ nhớ?", "Tiến trình liên tục cấp phát và chạm các trang bộ nhớ mới (Lazy Allocation / Zero-fill-on-demand) ngay trong RAM mà không cần đọc I/O đĩa, nhưng vẫn đốt CPU Kernel (`%sy`) cập nhật bảng trang."),
            (l53, "Sự khác nhau giữa On-CPU profiling (`perf record`) và Off-CPU profiling (`eBPF offcputime`) là gì?", "`perf record` đo các hàm đang chạy đốt CPU (`%us`/`%sy`), vô hình trước thời gian ngủ! `offcputime` đo thời gian luồng bị Blocked ngủ chờ I/O, Lock/Futex hoặc Network."),
            (l53, "Công cụ `bpftrace` tổng hợp dữ liệu (như histogram độ trễ I/O) về User Space bằng cơ chế gì để đạt overhead < 1%?", "Lưu và tính tổng hợp ngay bên trong Kernel bằng cấu trúc `eBPF Maps` (Hash, Per-CPU Array, Hist), chỉ gửi kết quả tóm tắt ra User Space thay vì đẩy từng sự kiện thô.")
        };

        await SeedFlashcardsIdempotentlyAsync(
            db,
            domainKey: "os-exp",
            tag: tag,
            cardType: "standard",
            sourceTitleResolver: id => id switch
            {
                _ when id == l11 => "Bài 1.1: Tiến trình, Luồng & Context Switch",
                _ when id == l12 => "Bài 1.2: Thuật toán Điều phối CPU",
                _ when id == l21 => "Bài 2.1: Đồng bộ hóa, Mutex & Semaphore",
                _ when id == l22 => "Bài 2.2: Deadlock & Thuật toán Banker",
                _ when id == l33 => "Bài 3.3: Thuật toán Thay thế Trang & Working Set",
                _ when id == l41 => "Bài 4.1: Hệ thống Tệp tin, Inode, Extent & Journaling",
                _ when id == l42 => "Bài 4.2: Hệ thống I/O, Page Cache & NVMe Queue",
                _ when id == l43 => "Bài 4.3: RAID, Copy-on-Write & Crash Consistency",
                _ when id == l51 => "Bài 5.1: Spinlock vs Mutex, CAS & Priority Inversion",
                _ when id == l52 => "Bài 5.2: Linux Namespaces, cgroups v2 & Containers",
                _ => "Bài 5.3: Case Study Hiệu năng eBPF, Page Faults & OOM Killer"
            },
            specs: specs,
            now: now);
    }

    #endregion

    #region 5. English A2-B1 (EnglishCourseId) Lessons 4.1..5.3

    private static List<LessonMeta> GetEnExpandedLessons() => new()
    {
        new LessonMeta(
            EnChapter4Id, EnLesson41Id,
            "Bài 4.1: Xuất Nhập Cảnh, Khách Sạn & Xử Lý Sự Cố Chuyến Bay Bằng Tiếng Anh",
            "en-xuat-nhap-canh-khach-san-xu-ly-su-co-chuyen-bay",
            1, 35,
            GetEnLesson41Markdown(),
            "Tự tin trả lời hải quan xuất nhập cảnh, nhận phòng khách sạn và yêu cầu đổi chuyến/bồi thường khi bị trễ hoặc thất lạc hành lý.",
            "Thành thạo câu hỏi lịch sự gián tiếp, mẫu câu đề nghị giải pháp (rebooking, voucher, upgrade) và phát âm chuẩn từ vựng hàng không."
        ),
        new LessonMeta(
            EnChapter4Id, EnLesson42Id,
            "Bài 4.2: Phỏng Vấn Kỹ Thuật & Behavioral Interview (STAR Method Bằng Tiếng Anh)",
            "en-phong-van-ky-thuat-va-behavioral-interview-star",
            2, 35,
            GetEnLesson42Markdown(),
            "Cấu trúc hóa câu trả lời phỏng vấn hành vi theo 4 bước STAR (Situation - Task - Action - Result) bằng động từ hành động mạnh mẽ.",
            "Trình bày tư duy giải quyết sự cố kỹ thuật, phân tích trade-off và đặt câu hỏi ngược thông minh cho nhà tuyển dụng quốc tế."
        ),
        new LessonMeta(
            EnChapter4Id, EnLesson43Id,
            "Bài 4.3: Code Review, Báo Cáo Bug & Thảo Luận Kiến Trúc Với Team Quốc Tế",
            "en-code-review-bao-cao-bug-thao-luan-kien-truc",
            3, 35,
            GetEnLesson43Markdown(),
            "Viết bình luận Code Review mang tính xây dựng, tránh giọng điệu ra lệnh gay gắt bằng cấu trúc gợi ý (Nitpick, Question, Suggestion).",
            "Báo cáo lỗi sản xuất (Severity, Reproduction Steps, Root Cause) và tranh luận lựa chọn kiến trúc hệ thống bằng tiếng Anh chuẩn mực."
        ),
        new LessonMeta(
            EnChapter5Id, EnLesson51Id,
            "Bài 5.1: Storytelling Trong Kinh Doanh, Trình Bày Số Liệu & Thuyết Phục Lãnh Đạo",
            "en-storytelling-trinh-bay-so-lieu-thuyet-phuc-lanh-dao",
            1, 35,
            GetEnLesson51Markdown(),
            "Biến bảng số liệu khô khan thành câu chuyện thuyết phục ban lãnh đạo theo khung Problem - Insight - Impact - Ask.",
            "Sử dụng chính xác động từ mô tả xu hướng tăng/giảm, phần trăm biến động và kỹ thuật neo giá trị ROI khi xin duyệt ngân sách."
        ),
        new LessonMeta(
            EnChapter5Id, EnLesson52Id,
            "Bài 5.2: Phản Biện Đa Chiều, Xử Lý Khủng Hoảng Khách Hàng & Từ Chối Khéo Léo",
            "en-phan-bien-da-chieu-xu-ly-khung-hoang-tu-choi-kheo",
            2, 35,
            GetEnLesson52Markdown(),
            "Hạ nhiệt căng thẳng với khách hàng bằng công thức Acknowledge - Empathize - Resolve mà không tự nhận lỗi pháp lý thiếu kiểm chứng.",
            "Phản biện đa chiều và từ chối yêu cầu ngoài phạm vi bằng kỹ thuật Positive No (khẳng định giá trị trước khi đưa giới hạn và phương án thay thế)."
        ),
        new LessonMeta(
            EnChapter5Id, EnLesson53Id,
            "Bài 5.3: Review & Nâng Cấp Email/Prompt Tiếng Anh Tự Nhiên (Thoát Văn Mẫu Máy Móc)",
            "en-review-nang-cap-email-prompt-tieng-anh-tu-nhien",
            3, 35,
            GetEnLesson53Markdown(),
            "Loại bỏ văn phong dịch từng chữ từ tiếng Việt (word-by-word) và các cụm sáo rỗng máy móc như 'Kindly revert', 'Please be informed'.",
            "Viết Email công việc súc tích, lịch thiệp đúng sắc thái bản xứ và thiết kế Prompt tiếng Anh rõ ràng, chính xác khi cộng tác cùng AI toàn cầu."
        )
    };

    private static string GetEnLesson41Markdown() => """"
# Bài 4.1: Xuất Nhập Cảnh, Khách Sạn & Xử Lý Sự Cố Chuyến Bay Bằng Tiếng Anh

## 1. Mục tiêu bài học (Learning Objectives)
- Trả lời trôi chảy, rõ ràng các câu hỏi kiểm tra tại quầy **Xuất nhập cảnh (Immigration & Customs)** mà không lúng túng.
- Tự tin làm thủ tục **Check-in / Check-out khách sạn**, yêu cầu hỗ trợ phòng ở (đổi phòng yên tĩnh, nhận phòng sớm, gửi hành lý).
- Chủ động xử lý **Sự cố hàng không**: chuyến bay bị hoãn/hủy, lỡ chuyến nối (missed connection), và khai báo thất lạc hành lý (**Lost Luggage / PIR**).
- Phát âm chuẩn các cụm phụ âm và trọng âm từ vựng sân bay hay đọc sai: *itinerary* `/aɪˈtɪnərəri/`, *customs* `/ˈkʌstəmz/`, *receipt* `/rɪˈsiːt/` (âm `p` câm!), *voucher* `/ˈvaʊtʃə/`.

---

## 2. Hội thoại mẫu thực tế (Model Dialogues)

### Tình huống 1: Tại quầy Hải quan Xuất nhập cảnh (Immigration Counter)
- **Officer:** Good afternoon. Passport and boarding pass, please. What is the purpose of your visit?
- **Minh:** Good afternoon. Here you go. I'm visiting Singapore for a five-day tech conference and some sightseeing.
- **Officer:** How long will you be staying, and where are you staying?
- **Minh:** I'll be staying for five nights at the Marina Bay Hotel. Here is my confirmed booking and return ticket.
- **Officer:** Do you have anything to declare at customs?
- **Minh:** Nothing to declare, officer. Just personal belongings and my work laptop.
- **Officer:** Alright, everything looks in order. Enjoy your stay!

### Tình huống 2: Xử lý chuyến bay bị hủy & hành lý thất lạc (Airport Service Desk)
- **Minh:** Excuse me, my connecting flight VN402 was just cancelled due to heavy fog. Could you help rebook me on the next available flight?
- **Agent:** Let me check the system right away. I can put you on the 6:30 PM flight—it's the earliest departure with an open seat.
- **Minh:** That works, thank you. Since that's a six-hour layover, am I eligible for a meal voucher or lounge access?
- **Agent:** Yes, of course. Here is a meal voucher worth twenty dollars. Also, your checked bag will be automatically transferred.
- **Minh:** What if my luggage doesn't arrive at the destination? Where should I file a report?
- **Agent:** Simply go to the Baggage Claim service counter and ask for a Property Irregularity Report—they will deliver it straight to your hotel.

---

## 3. Phân tích Hội thoại & Cụm từ Vàng (Line-by-line Breakdown)

| Ngữ cảnh | Câu tiếng Anh chuẩn | Dịch nghĩa tự nhiên | Bẫy ngữ pháp & Lưu ý phát âm |
|---|---|---|---|
| **Hải quan** | *I'm visiting for a five-day conference.* | Tôi tới dự hội thảo 5 ngày. | ⚠️ Tính từ ghép có gạch nối **KHÔNG thêm `-s`**: nói `a five-day trip` (KHÔNG nói *five-days trip*). |
| **Khai báo** | *Nothing to declare, just personal belongings.* | Không có gì cần khai báo, chỉ có đồ cá nhân. | `belongings` luôn ở dạng số nhiều; `declare` `/dɪˈkleər/` trọng âm âm 2. |
| **Đổi chuyến** | *Could you help rebook me on the next available flight?* | Bạn có thể đặt lại giúp tôi sang chuyến gần nhất còn chỗ không? | Dùng `Could you...` mềm mỏng hơn `Rebook me now!`. |
| **Quyền lợi** | *Am I eligible for a meal voucher?* | Tôi có đủ điều kiện nhận phiếu ăn không? | `eligible for [something]` `/ˈelɪdʒəbl/`: đủ điều kiện hưởng quyền lợi. |
| **Khách sạn** | *I'd like to check in under the name Minh Tran.* | Tôi muốn nhận phòng dưới tên Minh Trần. | `under the name [Name]`: đặt phòng dưới tên ai đó. |
| **Yêu cầu thêm** | *Would it be possible to get a late check-out?* | Liệu tôi có thể trả phòng muộn được không? | `Would it be possible to + V?` là mẫu câu xin phép chuẩn mực nhất. |

---

## 4. Ngữ pháp & Phát âm Trọng tâm

### 4.1. Câu hỏi Gián tiếp và Đề nghị Lịch sự (Polite Requests)
Khi gặp sự cố trễ chuyến hay hỏng phòng khách sạn, thái độ bình tĩnh kết hợp cấu trúc lịch sự giúp nhân viên hỗ trợ bạn nhanh gấp đôi:
- **Hỏi khả năng đáp ứng:** `Would it be possible to + V-inf...?` *(Liệu có thể... không?)*
  - *Ví dụ:* *"Would it be possible to move to a quieter room away from the elevator?"*
- **Hỏi quyền lợi:** `Am I entitled to / eligible for + Noun?` *(Tôi có được hưởng... không?)*
  - *Ví dụ:* *"Am I eligible for complimentary hotel accommodation due to the overnight delay?"*

### 4.2. Phát âm "Âm câm" & Trọng âm hay sai khi đi du lịch
- **`receipt` `/rɪˈsiːt/`** (hóa đơn): Chữ **`p` hoàn toàn CÂM**, tuyệt đối không đọc là *ri-síp*!
- **`itinerary` `/aɪˈtɪnərəri/`** (lịch trình bay): Trọng âm rơi vào âm thứ 2 (`TI`), không đọc *i-ti-ne-ra-ry*.
- **`luggage` / `baggage`**: Cả hai đều là danh từ **không đếm được**! Sai: *"two luggages"* ❌ $\to$ Đúng: *"two pieces of luggage"* hoặc *"two checked bags"* ⭕.

---

## 5. Bảng Từ vựng Thực chiến (Travel & Aviation Vocabulary)

| Từ / Cụm từ | IPA | Nghĩa tiếng Việt | Ví dụ mẫu |
|---|---|---|---|
| **boarding pass** | `/ˈbɔːdɪŋ pɑːs/` | Thẻ lên máy bay | Keep your boarding pass handy at the security checkpoint. |
| **layover / stopover** | `/ˈleɪəʊvə/` | Thời gian quá cảnh giữa 2 chặng | We have a four-hour layover in Tokyo Narita. |
| **checked baggage** | `/tʃekt ˈbæɡɪdʒ/` | Hành lý ký gửi | How many checked bags are included in my ticket? |
| **carry-on bag** | `/ˈkæri ɒn bæɡ/` | Hành lý xách tay lên cabin | Please place your carry-on bag in the overhead bin. |
| **security deposit** | `/sɪˈkjʊərəti dɪˈpɒzɪt/` | Tiền đặt cọc an ninh (khách sạn) | The hotel holds a refundable 100-dollar security deposit on your card. |
| **complimentary** | `/ˌkɒmplɪˈmentri/` | Miễn phí (dịch vụ kèm theo) | Guests receive complimentary Wi-Fi and airport shuttle service. |

---

## 6. Lỗi người Việt hay mắc & Bài tập Tự kiểm tra

1. ❌ **Sai:** *"I stay here five days ago and I have two luggages."*
   ⭕ **Đúng:** *"I am staying here for five days, and I have two pieces of luggage."*
2. ❌ **Sai:** *"My flight was delay because rain."*
   ⭕ **Đúng:** *"My flight was delayed due to heavy rain."* (`be + V3/ed` thể bị động + `due to + Noun`).

### Bài tập nhanh: Chọn đáp án đúng
1. Would it be possible (get / to get / getting) ______ a room on a higher floor?
2. I'm traveling with a (ten-days / ten-day) ______ tourist visa.
3. Excuse me, my suitcase didn't show up at baggage (claim / check-in) ______.

<details>
<summary>👉 Xem đáp án & Giải thích</summary>

1. **to get** — Cấu trúc `Would it be possible to + V-inf?`.
2. **ten-day** — Danh từ ghép làm tính từ đứng trước `visa` không có `s`.
3. **claim** — `baggage claim` là khu vực băng chuyền nhận lại hành lý ký gửi.

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->
"""";

    private static string GetEnLesson42Markdown() => """"
# Bài 4.2: Phỏng Vấn Kỹ Thuật & Behavioral Interview (STAR Method Bằng Tiếng Anh)

## 1. Mục tiêu bài học (Learning Objectives)
- Trình bày mạch lạc kinh nghiệm làm việc và xử lý sự cố theo công thức **STAR** (*Situation — Task — Action — Result*) trong vòng 90–120 giây.
- Nâng cấp từ vựng hành động chuyên nghiệp thay cho các động từ chung chung (*did, made, fixed*) bằng cụm động từ đắt giá (*architected, spearheaded, diagnosed, reduced latency by...*).
- Giải thích tư duy kỹ thuật, đánh đổi thiết kế (*trade-offs*) và tự tin trả lời khi **chưa biết ngay đáp án** mà không im lặng bối rối.
- Đặt câu hỏi ngược thông minh (*Reverse Interview*) tạo ấn tượng chuyên nghiệp với Hiring Manager quốc tế.

---

## 2. Đoạn hội thoại Phỏng vấn Thực tế (Model Interview)

**Bối cảnh:** Phỏng vấn vòng Behavioral & Technical Deep-Dive cho vị trí Backend / Fullstack Engineer tại một công ty công nghệ quốc tế.

- **Interviewer (Sarah):** Tell me about a time you resolved a critical production incident under tight pressure.
- **Minh:** Sure! ***(Situation)*** Last quarter, right before a major product launch, our API checkout service experienced severe timeout spikes affecting 15% of active users. ***(Task)*** As the on-call engineer, my responsibility was to identify the root cause quickly and restore stability without losing payment records.
- **Interviewer:** Interesting. Walk me through how you diagnosed and fixed it.
- **Minh:** ***(Action)*** First, I inspected our APM traces and noticed database connection pool exhaustion caused by an unindexed query during high concurrency. I immediately rolled out a hotfix adding a composite index, introduced Redis caching for read-heavy lookups, and configured graceful circuit breakers. ***(Result)*** As a result, p99 latency dropped from 3.2 seconds down to 45 milliseconds within twenty minutes, with zero failed transactions.
- **Interviewer:** Great impact! Do you have any questions for our engineering team?
- **Minh:** Yes! How does your team balance shipping new features quickly against paying down technical debt?

---

## 3. Cấu trúc STAR & Động từ Hành động Mạnh mẽ (Power Verbs)

| Bước STAR | Tỷ lệ thời gian | Mẫu câu mở đầu tự nhiên | Lưu ý tránh bẫy |
|---|---|---|---|
| **S — Situation** (Bối cảnh) | ~15% | *"In my previous role at...", "A few months ago, our team faced..."* | Ngắn gọn 1-2 câu; tránh kể lan man quá dài về công ty cũ. |
| **T — Task** (Nhiệm vụ) | ~15% | *"I was tasked with...", "My primary objective was to ensure..."* | Nêu rõ vai trò cá nhân của bạn, phân biệt với việc chung của nhóm. |
| **A — Action** (Hành động) | ~50% | *"I diagnosed...", "I refactored...", "To mitigate this, I implemented..."* | Dùng đại từ **`I`** (không lạm dụng *"we"* ở bước này!). |
| **R — Result** (Kết quả đo lường) | ~20% | *"Consequently, this reduced X by 40%.", "As a result, we saved..."* | Luôn gắn **con số cụ thể** (ms, %, giờ công, doanh thu) hoặc bài học rút ra. |

### Nâng cấp Động từ từ A2 lên B1–B2 chuyên nghiệp:
- Thay vì *"I made the query faster"* $\to$ **"I optimized the database query, cutting response time by 60%."**
- Thay vì *"I helped my team"* $\to$ **"I collaborated closely with frontend peers and mentored two junior developers."**
- Thay vì *"I fixed a big problem"* $\to$ **"I diagnosed a race condition and implemented an idempotent retry mechanism."**

---

## 4. Chiến lược Xử lý Câu hỏi Khó (When You Don't Know Everything)

Trong phỏng vấn kỹ thuật tiếng Anh, nhà tuyển dụng đánh giá **cách bạn suy nghĩ thành tiếng (Think Out Loud)** cao hơn việc học thuộc lòng:
1. **Xin thêm 10 giây làm rõ đề bài:**
   - *"That's a great question. Before diving in, could I clarify one constraint regarding expected traffic volume?"*
2. **Nêu Trade-off (Sự đánh đổi kiến trúc):**
   - *"On one hand, caching improves read throughput; on the other hand, it introduces cache invalidation complexity."*
3. **Khi thực sự chưa từng dùng một công nghệ cụ thể:**
   - ❌ Tránh nói cụt lủn: *"I don't know."*
   - ⭕ Nói trung thực + cầu tiến: *"I haven't used Kafka directly in production yet, though I have extensive experience with RabbitMQ and event-driven outbox patterns, so the transition would be straightforward."*

---

## 5. Bảng Từ vựng & IPA Phỏng vấn Kỹ thuật

| Thuật ngữ / Cụm từ | IPA | Nghĩa tiếng Việt | Ví dụ trong câu trả lời |
|---|---|---|---|
| **root cause** | `/ruːt kɔːz/` | Nguyên nhân gốc rễ | Tracing the logs helped us isolate the root cause. |
| **bottleneck** | `/ˈbɒtlnek/` | Điểm nghẽn cổ chai | Synchronous disk writes became our primary bottleneck. |
| **trade-off** | `/ˈtreɪd ɒf/` | Sự đánh đổi hai mặt | Every architectural decision involves a trade-off between consistency and latency. |
| **scalability** | `/ˌskeɪləˈbɪləti/` | Khả năng mở rộng quy mô | We decoupled the worker service to improve horizontal scalability. |
| **spearhead** | `/ˈspɪəhed/` | Dẫn dắt tiên phong | I spearheaded the migration from monolith to modular services. |
| **retrospective** | `/ˌretrəˈspektɪv/` | Buổi họp nhìn lại rút kinh nghiệm | During the post-incident retrospective, we added automated alerts. |

---

## 6. Lỗi người Việt hay mắc & Bài tập Tự luyện

1. **Quên chia thì Quá khứ đơn (Past Simple) khi kể chuyện STAR:**
   - ❌ *"Last year I implement a new cache and reduce error rate."*
   - ⭕ *"Last year, I **implemented** a new cache and **reduced** the error rate."* (Đặc biệt chú ý bật âm cuối `/t/` hoặc `/d/` hoặc `/ɪd/` của đuôi `-ed`!).
2. **Dùng quá nhiều "We" khi được hỏi về đóng góp cá nhân:**
   - Người Việt khiêm tốn hay nói *"We designed, we coded..."* khiến nhà tuyển dụng không biết bạn thực sự làm phần nào. Hãy dùng *"My specific contribution was..."* hoặc *"I personally built..."*.

### Bài tập nhanh: Hoàn chỉnh câu trả lời STAR
1. To (address / addressing) ______ the bottleneck, I introduced connection pooling.
2. Consequently, our deployment time dropped (from / by) ______ 30 minutes to 4 minutes.
3. Let me (walk / walking) ______ you through the trade-offs of this design.

<details>
<summary>👉 Xem đáp án & Giải thích</summary>

1. **address** — `To + V-inf` chỉ mục đích ("Để giải quyết điểm nghẽn").
2. **from** — Cấu trúc `dropped from X to Y` (giảm từ X xuống Y); dùng `by` khi nói mức chênh lệch (*dropped by 26 minutes*).
3. **walk** — `Let me + V-inf`: "Để tôi dẫn dắt bạn qua...".

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->
"""";

    private static string GetEnLesson43Markdown() => """"
# Bài 4.3: Code Review, Báo Cáo Bug & Thảo Luận Kiến Trúc Với Team Quốc Tế

## 1. Mục tiêu bài học (Learning Objectives)
- Viết bình luận **Code Review (PR Comments)** rõ ràng, lịch sự và mang tính xây dựng theo chuẩn *Conventional Comments* (`Nitpick`, `Suggestion`, `Question`, `Blocker`).
- Viết **Bug Report** chuẩn kỹ sư quốc tế gồm 4 phần: *Summary, Steps to Reproduce, Expected vs Actual Behavior, Environment/Logs*.
- Thảo luận **Kiến trúc hệ thống (Architecture Discussion)** và đề xuất phương án thay thế mà không làm đồng nghiệp cảm thấy bị công kích cá nhân.
- Phân biệt sắc thái các trợ động từ khuyên nhủ (`must` vs `should` vs `could we consider`).

---

## 2. Hội thoại Thảo luận Code Review & Kiến trúc (Model Dialogue)

**Bối cảnh:** Cuộc họp sync nhanh trên Slack Huddle / Google Meet giữa Minh (Backend) và Elena (Tech Lead ở Berlin) về một Pull Request xử lý đồng thời.

- **Elena:** Hey Minh, thanks for opening the PR! Overall the logic looks super clean. I left one blocking comment regarding the database transaction.
- **Minh:** Hi Elena! Thanks for reviewing so quickly. Are you referring to the race condition when two requests update the same notebook version?
- **Elena:** Exactly. Right now, the version check runs outside the lock, so concurrent edits could silently overwrite each other.
- **Minh:** Good catch! That makes total sense. What if I wrap the update in an atomic `WHERE version = @baseVersion` check and return HTTP `409 Conflict` on mismatch?
- **Elena:** That's much cleaner and avoids holding a heavy table lock. Also, just a minor nitpick: could we extract that retry helper into a shared utility?
- **Minh:** Definitely! I'll push the fix, add a concurrency integration test, and ping you for a re-review in thirty minutes.

---

## 3. Nghệ thuật Viết Code Review "Đúng Trọng tâm, Không Gay gắt"

Trong môi trường quốc tế, bình luận kiểu *"This code is wrong, fix it"* hay *"Why did you write this?"* bị coi là thiếu chuyên nghiệp (**toxic**). Hãy áp dụng nguyên tắc **"Critique the code, never the person"**:

| Nhãn (Label) | Mục đích | Cách viết THÔ (Tránh) ❌ | Cách viết CHUYÊN NGHIỆP (Nên dùng) ⭕ |
|---|---|---|---|
| **Blocker** (Lỗi nghiêm trọng) | Phải sửa trước khi Merge | *"You forgot error handling here."* | *"If the API times out here, this might throw an unhandled exception. Could we wrap this in a `try/catch` block?"* |
| **Suggestion** (Đề xuất tốt hơn) | Cải thiện hiệu năng/đọc hiểu | *"Change this to a Set."* | *"Suggestion: Using a `Set` instead of an array here would reduce lookup complexity from $O(N)$ to $O(1)$."* |
| **Question** (Hỏi làm rõ) | Chưa hiểu ý đồ tác giả | *"What is this doing?!"* | *"Just out of curiosity, is there a specific reason we're bypassing the cache on this endpoint?"* |
| **Nitpick** (Chi tiết nhỏ) | Naming, formatting (không chặn PR) | *"Rename variable x."* | *"Nit: Would `retryCount` be slightly clearer than `cnt` here? Feel free to ignore if you prefer keeping it."* |

---

## 4. Mẫu Báo Cáo Bug Chuẩn Kỹ Sư (Effective Bug Report)

Một Bug Report tệ chỉ viết: *"Login broken!! Please fix asap."* Một Bug Report chuẩn giúp đồng nghiệp tái hiện lỗi trong 2 phút:

```markdown
### Summary
Submitting an empty password resets the form silently instead of displaying a validation error.

### Steps to Reproduce
1. Navigate to `/auth/login`.
2. Enter a valid email address (`son.tran@studxy.vn`).
3. Leave the password field blank and click **Sign In**.

### Expected vs. Actual Behavior
- **Expected:** Display HTTP 400 validation banner: "Password is required."
- **Actual:** Request returns `200 OK` and clears the email input without error feedback.

### Environment & Evidence
- Browser: Chrome 131 on Windows 11
- Commit: `a4f92c1` | Console shows `TypeError: Cannot read properties of undefined`.
```

---

## 5. Ngữ pháp & Từ vựng Chuyên ngành Phần mềm

### 5.1. Câu điều kiện loại 1 & 2 cho phân tích Kỹ thuật
- **Rủi ro thực tế có thể xảy ra (Loại 1):** *"If we don't paginate this endpoint, the payload **will grow** uncontrollably."*
- **Giả định đề xuất kiến trúc (Loại 2):** *"If we **switched** to an outbox worker, the HTTP request **would return** in under 20 milliseconds."*

### 5.2. Bảng Từ vựng & IPA Code Review / Architecture

| Thuật ngữ | IPA | Nghĩa tiếng Việt | Ví dụ thực tế |
|---|---|---|---|
| **edge case** | `/edʒ keɪs/` | Trường hợp biên hiếm gặp | Let's add a unit test covering the empty-array edge case. |
| **backward-compatible** | `/ˈbækwəd kəmˈpætəbl/` | Tương thích ngược | Ensure the new JSON field remains backward-compatible with v1 clients. |
| **overhead** | `/ˈəʊvəhed/` | Chi phí phụ trội (CPU/RAM/độ trễ) | Adding another network hop introduces unnecessary latency overhead. |
| **single point of failure** | `/ˌsɪŋɡl pɔɪnt əv ˈfeɪljə/` | Điểm lỗi đơn (sập là sập cả hệ thống) | Running a single Redis instance without replicas is a single point of failure. |
| **decouple** | `/diːˈkʌpl/` | Tách rời phụ thuộc | Message queues decouple webhook ingestion from heavy AI processing. |
| **regression** | `/rɪˈɡreʃn/` | Lỗi tái phát (tính năng cũ từng chạy nay hỏng) | Always run the E2E suite to prevent regressions before release. |

---

## 6. Lỗi người Việt hay mắc & Bài tập Tự luyện

1. ❌ **Sai:** *"I have fixed the bug, please help me check."* (Dịch word-by-word *"giúp tôi kiểm tra"*).
   ⭕ **Đúng:** *"I've pushed the fix—could you please take another look when you have a moment?"* (Hoặc *"Ready for re-review!"*).
2. ❌ **Sai:** *"This code is conflict with main branch."* (Dùng `conflict` như tính từ không chia).
   ⭕ **Đúng:** *"This branch **has merge conflicts** with `main`."* hoặc *"These changes **conflict with** the existing middleware."*

### Bài tập nhanh: Chọn cách diễn đạt chuẩn
1. (Nit / Blocker) ______: Could we rename `d` to `durationMs` for readability?
2. Thank you for catching that! That makes total (sense / logic) ______.
3. Wrapping the database update in a transaction prevents partial (writes / writing) ______.

<details>
<summary>👉 Xem đáp án & Giải thích</summary>

1. **Nit** — Đổi tên biến cho dễ đọc là góp ý nhỏ (Nitpick), không chặn merge.
2. **sense** — Cụm cố định: *"That makes total sense"* (Rất hợp lý / Tôi hoàn toàn hiểu ý bạn).
3. **writes** — Danh từ số nhiều `partial writes` (ghi dữ liệu dở dang một nửa).

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->
"""";

    private static string GetEnLesson51Markdown() => """"
# Bài 5.1: Storytelling Trong Kinh Doanh, Trình Bày Số Liệu & Thuyết Phục Lãnh Đạo

## 1. Mục tiêu bài học (Learning Objectives)
- Biến các bảng số liệu khô khan thành **câu chuyện kinh doanh có sức thuyết phục (Data Storytelling)** theo cấu trúc **Hook — Context — Insight — Action (Ask)**.
- Sử dụng chính xác các động từ mô tả xu hướng (*surged, plateaued, plummeted, fluctuated*) cùng giới từ số liệu (**`by`** vs **`to`** vs **`at`**).
- Trình bày bài toán **ROI (Return on Investment)** và đề xuất ngân sách/nhân sự ngắn gọn cho Ban lãnh đạo (Executive Summary — *BLUF: Bottom Line Up Front*).
- Khắc phục lỗi đọc sai các con số lớn (`thousand`, `million`, `billion`) và phần trăm thập phân bằng tiếng Anh.

---

## 2. Hội thoại Báo cáo Lãnh đạo (Executive Pitch Dialogue)

**Bối cảnh:** Minh trình bày đề xuất nâng cấp hạ tầng tìm kiếm và bộ nhớ đệm trước Giám đốc Sản phẩm (VP of Product — David) trong 3 phút đầu cuộc họp.

- **Minh:** Good morning, David. ***(Bottom Line Up Front)*** I'd like to propose investing two sprint weeks into upgrading our search caching layer, which will cut our cloud database bill by 35% and boost checkout conversion.
- **David:** Thirty-five percent is significant. What data led you to that conclusion?
- **Minh:** Let's look at the Q3 metrics. While daily active users surged **by** 40% to 120,000, our search page load time crept up **to** 1.8 seconds. Notice how user drop-off spikes sharply right after the one-second mark.
- **David:** That's a clear correlation. What is the estimated ROI if we approve this?
- **Minh:** Based on our pilot benchmark, response times fell **from** 1.8 seconds **to** 140 milliseconds. In dollar terms, the engineering effort pays for itself within two months through saved database instances alone, not counting higher customer retention.
- **David:** Compelling numbers, Minh. Write up a one-page spec and you have my green light for next sprint.

---

## 3. Khung Kể Chuyện Dữ Liệu 4 Bước (Data Storytelling Framework)

Ban lãnh đạo bận rộn không muốn nghe lịch sử dài dòng từ đầu; họ cần nguyên tắc **BLUF (Bottom Line Up Front — Đưa kết luận lên đầu)**:
1. **The Ask / Bottom Line:** *"I recommend allocating $10K to X in order to achieve Y."*
2. **The Tension (Vấn đề từ số liệu):** Chỉ ra khoảng cách giữa hiện tại và mục tiêu (*"Although signups grew by 25%, activation plateaued at 18%."*).
3. **The Insight (Nguyên nhân gốc từ dữ liệu):** *"Drilling into funnel analytics revealed that 60% of users drop off during onboarding."*
4. **The Impact & Next Step (ROI & Lời kêu gọi):** *"By automating step 2, we project a 15% lift in retention within Q4."*

---

## 4. Ngữ pháp Số liệu Sống còn: Giới từ `by`, `to`, `at`, `of`

Đây là lỗi phổ biến nhất khiến người học tiếng Anh báo cáo sai bản chất con số:

| Giới từ | Ý nghĩa | Ví dụ chuẩn xác | Dịch nghĩa |
|---|---|---|---|
| **`to`** | Chạm tới **mức mới** | *Revenue rose **to** $2.5 million.* | Doanh thu tăng **lên mức** 2,5 triệu đô. |
| **`by`** | Mức **chênh lệch** (tăng/giảm thêm bao nhiêu) | *Costs dropped **by** 35%.* | Chi phí giảm **đi** 35% (so với cũ). |
| **`from ... to`** | Chuyển dịch từ mốc cũ sang mốc mới | *Latency fell **from** 1.8s **to** 0.14s.* | Độ trễ giảm **từ** 1,8 giây **xuống** 0,14 giây. |
| **`at`** | Đứng yên / đạt đỉnh / chạm đáy **tại mốc** | *Churn stabilized **at** 4.2%.* | Tỷ lệ rời bỏ ổn định **ở mức** 4,2%. |
| **`of`** | Dùng sau **Danh từ** chỉ sự thay đổi | *There was **an increase of** 12%.* | Có **một mức tăng** 12%. |

### Cách đọc Số liệu Tự nhiên không vấp:
- `1.8 seconds`: đọc **"one point eight seconds"** (không đọc *one comma eight*).
- `35%`: đọc **"thirty-five percent"** (`percent` KHÔNG BAO GIỜ thêm `s`!).
- `$2.5M`: đọc **"two point five million dollars"**.

---

## 5. Bảng Từ vựng & IPA Kể chuyện Kinh doanh

| Từ / Cụm từ | IPA | Nghĩa tiếng Việt | Ví dụ trong bài thuyết trình |
|---|---|---|---|
| **surge / soar** | `/sɜːdʒ/` / `/sɔːr/` | Tăng vọt mạnh mẽ | Mobile traffic surged immediately after the campaign launch. |
| **plummet / plunge** | `/ˈplʌmɪt/` / `/plʌndʒ/` | Lao dốc, tụt giảm thê thảm | Bounce rate plummeted once we simplified the landing page. |
| **plateau** | `/ˈplætəʊ/` | Đi ngang, chững lại sau khi tăng | User growth plateaued at fifty thousand monthly subscribers. |
| **pay for itself** | `/peɪ fɔːr ɪtˈself/` | Tự hoàn vốn (lợi ích > chi phí) | This automation tool pays for itself within six weeks. |
| **green light** | `/ɡriːn laɪt/` | Bật đèn xanh (phê duyệt triển khai) | Once legal reviews the terms, we'll get the green light. |
| **bottom line** | `/ˈbɒtəm laɪn/` | Kết luận cốt lõi / lợi nhuận ròng | The bottom line is that speed directly drives customer trust. |

---

## 6. Lỗi người Việt hay mắc & Bài tập Tự luyện

1. ❌ **Sai:** *"Our users increased 20 percents last month."*
   ⭕ **Đúng:** *"Our user base grew **by** 20 **percent** last month."* (`percent` không có `-s`, động từ `increase/grow` cần giới từ `by` chỉ biên độ).
2. ❌ **Sai:** *"Please look at the chart, the line goes up very strong."*
   ⭕ **Đúng:** *"As you can see from the chart, the trend shows **a sharp upward trajectory**."*

### Bài tập nhanh: Chọn giới từ hoặc từ đúng
1. Server costs increased (by / to) ______ 15% compared to last quarter, reaching $12,000.
2. There was a significant drop (by / of) ______ 40 milliseconds in average query time.
3. After three months of rapid growth, weekly signups (plummeted / plateaued) ______ at 5,000 users/week.

<details>
<summary>👉 Xem đáp án & Giải thích</summary>

1. **by** — 15% là mức chênh lệch tăng thêm so với quý trước -> dùng `by`.
2. **of** — Sau danh từ `a drop` dùng giới từ `of`.
3. **plateaued** — Đi ngang ổn định ở mức 5,000 -> dùng `plateaued` (`plummeted` là lao dốc).

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->
"""";

    private static string GetEnLesson52Markdown() => """"
# Bài 5.2: Phản Biện Đa Chiều, Xử Lý Khủng Hoảng Khách Hàng & Từ Chối Khéo Léo

## 1. Mục tiêu bài học (Learning Objectives)
- Hạ nhiệt khách hàng đang bức xúc khi xảy ra sự cố nghiêm trọng theo quy trình 4 bước **L.A.E.R.** (*Listen — Acknowledge — Empathize — Resolve*).
- Phản biện đa chiều trong cuộc họp căng thẳng (*Playing Devil's Advocate*) một cách khách quan, dựa trên dữ kiện chứ không đối đầu cá nhân.
- Nói lời **Từ chối Khéo léo (The Positive No)** trước các yêu cầu gấp hoặc ngoài phạm vi (*scope creep*) mà vẫn giữ được thiện cảm và sự tôn trọng của đối tác.
- Làm chủ cấu trúc nhượng bộ (`While I appreciate...`, `Granted that...`, `Even though...`) để nâng cao tính thuyết phục B1–B2.

---

## 2. Hội thoại Xử lý Khủng hoảng Khách hàng (Crisis De-escalation Dialogue)

**Bối cảnh:** Khách hàng doanh nghiệp (Marcus) gọi khẩn cấp vì hệ thống xuất báo cáo bị lỗi đúng ngày chốt sổ cuối tháng. Minh phụ trách tiếp nhận và xoa dịu khủng hoảng.

- **Marcus (Angry Client):** Minh, this is completely unacceptable! Our finance team can't export our monthly reports, and our board meeting starts in two hours!
- **Minh:** ***(Acknowledge & Empathize)*** Marcus, I hear you, and I completely understand how critical this export is right before your board meeting. I sincerely apologize for the disruption this has caused your team.
- **Marcus:** Just tell me when it's going to be fixed! We can't afford to wait until tomorrow.
- **Minh:** ***(Take Ownership & Resolve)*** Our senior engineers are actively investigating the root cause right now as our top priority. While they deploy the permanent patch—which takes about forty minutes—I can generate and securely send your PDF reports manually within fifteen minutes so your meeting isn't delayed.
- **Marcus:** Okay, getting the manual reports in fifteen minutes would save us. Please keep me posted on the permanent fix.
- **Minh:** Absolutely. You'll have the reports in your inbox shortly, and I'll send you a live status update every twenty minutes until service is 100% restored.

---

## 3. Công thức Xoa dịu Khủng hoảng & Từ chối Khéo léo

### 3.1. Công thức "The Positive No" (Từ chối mà vẫn giữ mối quan hệ)
Đừng bao giờ mở đầu câu từ chối bằng *"No, we can't do that"* hoặc *"That's impossible"*. Hãy dùng mô hình **Yes — No — Yes**:
1. **YES (Công nhận giá trị yêu cầu):** *"I completely understand why having that custom dashboard by Friday would help your launch."*
2. **NO (Đặt giới hạn rõ ràng bằng lý do khách quan):** *"However, given our current security audit commitments, we aren't able to deliver the full analytics suite this week without compromising stability."*
3. **YES (Đưa phương án thay thế khả thi):** *"What we can do instead is ship the core CSV export by Friday and roll out the full visual dashboard next Wednesday. Would that work for you?"*

### 3.2. Các cụm Phản biện Đa chiều (Diplomatic Counterarguments)
- **Thừa nhận góc nhìn đối phương trước:** *"That's a valid concern; however, looking at the long-term maintenance cost..."*
- **Đặt giả thuyết phản biện nhẹ nhàng:** *"Just to play devil's advocate, what happens if traffic triples during the holiday sale?"*
- **Tách người khỏi ý tưởng:** *"I love the goal behind this proposal, though I wonder if we might achieve it faster by..."*

---

## 4. Ngữ pháp Trọng tâm: Mệnh đề Nhượng bộ & Giảm nhẹ (Hedging)

Trong giao tiếp cấp độ B1+, người bản xứ dùng **Hedging** (ngôn ngữ làm mềm) để tránh nghe như đang phán xét hay ra lệnh:
- **Thay câu khẳng định gắt:** *"Your plan will fail."* ❌
- **Dùng Hedging:** *"There's a risk that this approach **might** face scalability challenges down the road."* ⭕
- **Cấu trúc Nhượng bộ (Concession):**
  - `While + S + V, S + V` *(Mặc dù..., nhưng...)*: *"While automation speeds up testing, it cannot replace human exploratory review."*
  - `Despite + Noun/V-ing, S + V`: *"Despite the unexpected outage, we met our monthly SLA target."* (⚠️ **CẤM dùng** `Despite of`!).

---

## 5. Bảng Từ vựng & IPA Xử lý Khủng hoảng

| Cụm từ / Từ vựng | IPA | Nghĩa tiếng Việt | Ví dụ thực tế |
|---|---|---|---|
| **disruption** | `/dɪsˈrʌpʃn/` | Sự gián đoạn hoạt động | We apologize for any inconvenience or service disruption. |
| **top priority** | `/tɒp praɪˈɒrəti/` | Ưu tiên số một | Resolving this authentication issue is our top priority. |
| **keep someone posted** | `/kiːp ˈpəʊstɪd/` | Cập nhật tình hình liên tục cho ai | I'll keep you posted as soon as the hotfix goes live. |
| **workaround** | `/ˈwɜːkəraʊnd/` | Giải pháp tạm thời để chạy tiếp | Here is a reliable workaround while we patch the bug. |
| **out of scope** | `/aʊt əv skəʊp/` | Nằm ngoài phạm vi thỏa thuận | Custom SSO integration is currently out of scope for Phase 1. |
| **valid concern** | `/ˈvælɪd kənˈsɜːn/` | Mối lo ngại hoàn toàn chính đáng | Data residency is a valid concern for enterprise clients. |

---

## 6. Lỗi người Việt hay mắc & Bài tập Tự luyện

1. ❌ **Sai:** *"Despite of the heavy traffic, we arrived on time."*
   ⭕ **Đúng:** *"**Despite** the heavy traffic..."* hoặc *"**In spite of** the heavy traffic..."* (`Despite` không bao giờ đi với `of`!).
2. ❌ **Sai:** *"No, we cannot do it because we don't have time."* (Quá phũ phàng với khách hàng).
   ⭕ **Đúng:** *"I understand how urgent this is; however, to ensure quality, the earliest we can deliver is Tuesday. Would a phased handover help?"*

### Bài tập nhanh: Chọn câu đúng ngữ pháp & sắc thái
1. (Despite / Despite of / Although) ______ the tight deadline, the team delivered zero-defect code.
2. We are offering a temporary (workaround / work-out) ______ so your staff can keep exporting invoices today.
3. I understand your urgency; (however / but therefore) ______, we cannot bypass security testing.

<details>
<summary>👉 Xem đáp án & Giải thích</summary>

1. **Despite** — Đi sau là cụm danh từ `the tight deadline` (`Although` phải đi với mệnh đề `S + V`).
2. **workaround** — Danh từ kỹ thuật chỉ giải pháp thay thế tạm thời.
3. **however** — Trạng từ nối đối lập trang trọng giữa hai mệnh đề độc lập.

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->
"""";

    private static string GetEnLesson53Markdown() => """"
# Bài 5.3: Review & Nâng Cấp Email/Prompt Tiếng Anh Tự Nhiên (Thoát Văn Mẫu Máy Móc)

## 1. Mục tiêu bài học (Learning Objectives)
- Nhận diện và loại bỏ các cụm **văn mẫu máy móc, lỗi thời hoặc "Việt-Anh"** (*"Dear Sir/Madam", "Kindly revert", "Please be informed", "Hope this email finds you well" lặp sáo rỗng*).
- Nâng cấp Email công việc lên chuẩn **hiện đại, súc tích, thân thiện nhưng sắc bén** đúng văn phong các công ty công nghệ toàn cầu.
- Làm chủ kỹ năng viết **Prompt tiếng Anh rõ ràng cho AI (Prompt Engineering)** theo khung **Role — Context — Task — Constraints — Format**.
- Tổng ôn các điểm ngữ pháp cốt lõi A2 $\to$ B1 (Mạo từ, Giới từ, Câu điều kiện, Phrasal Verbs, Active Voice).

---

## 2. Cuộc "Lột xác" Email: Từ Văn mẫu Cứng nhắc sang Tự nhiên (Before & After)

### Bản Gốc Cứng nhắc (Robotic / Chinglish-Vinglish) ❌
> *Dear Mr. Alex,*
> *I hope this email finds you well. Please be informed that we already finished the API testing task yesterday. I attach here the test report for your reference. Kindly check and revert to me asap. Thank you very much and best regards.*

**Vì sao bản gốc bị trừ điểm chuyên nghiệp?**
- `"Please be informed that..."`: Nghe như thông báo hành chính cứng nhắc thế kỷ trước.
- `"already finished ... yesterday"`: Thừa từ (`already` thường dùng với Present Perfect, còn có mốc `yesterday` chỉ cần Past Simple!).
- `"revert to me asap"`: Lạm dụng *"revert"* sai nghĩa chuẩn (bản xứ hiểu *revert* là hoàn tác về trạng thái cũ, không dùng thay cho *reply*!), và `"asap"` viết thường nghe hơi hối thúc thô lỗ.

### Bản Nâng cấp Tự nhiên Chuẩn Quốc tế (Natural B1–B2) ⭕
> *Hi Alex,*
> *Quick update: we wrapped up the API integration tests yesterday, and all critical endpoints passed. I've attached the full test summary below for your review.*
> *Could you take a look and share your feedback by Thursday afternoon so we can proceed with Friday's release? Let me know if you have any questions!*
> *Best,*
> *Minh*

---

## 3. Từ điển "Thoát Văn Mẫu" (Robotic vs. Natural Equivalents)

| Cụm sáo rỗng / dịch thô nên BỎ ❌ | Cụm tự nhiên chuẩn bản xứ nên DÙNG ⭕ | Ngữ cảnh sử dụng |
|---|---|---|
| *"Kindly do the needful / Kindly revert."* | *"Could you please review this and let me know your thoughts?"* | Nhờ đồng nghiệp/đối tác phản hồi |
| *"Please be informed that..."* | *"Just a quick heads-up that..."* / *"I'm writing to let you know..."* | Thông báo thay đổi lịch hoặc tiến độ |
| *"Sorry for bothering you."* | *"Thanks for your time!"* / *"When you have a quick moment..."* | Nhờ hỗ trợ (tự tin, không hạ mình quá mức) |
| *"I have a question want to ask you."* | *"I had a quick question regarding..."* | Mở đầu câu hỏi công việc |
| *"We will discuss about this later."* | *"Let's **discuss** this offline / **talk about** this tomorrow."* | ⚠️ `discuss` KHÔNG đi với `about`! |
| *"Thank you for your kind attention."* | *"Thanks in advance!"* / *"Appreciate your help with this."* | Kết thúc email nhờ vả |

---

## 4. Viết Prompt Tiếng Anh Sắc bén Khi Làm Việc Cùng AI Toàn Cầu

Trong kỷ nguyên kỹ sư phần mềm làm việc cùng LLM (ChatGPT, Claude, Gemini), viết Prompt tiếng Anh rõ ràng giúp chất lượng đầu ra tăng gấp 3 lần:

### Công thức Prompt 5 Phần: `R-C-T-C-F`
1. **Role (Vai trò):** *"Act as a senior database reliability engineer."*
2. **Context (Bối cảnh):** *"We run a .NET 8 Web API backed by PostgreSQL 16 experiencing high p99 latency during bulk imports."*
3. **Task (Nhiệm vụ cụ thể dùng động từ mạnh):** *"Analyze the attached SQL query and identify missing indexes or N+1 patterns."*
4. **Constraints (Ràng buộc giới hạn):** *"Keep backward compatibility; avoid adding external Redis dependencies."*
5. **Format (Định dạng đầu ra):** *"Provide a prioritized checklist in a Markdown table with before/after SQL snippets."*

> [!TIP]
> **Mẹo ngữ pháp khi viết Prompt:** Luôn dùng **Câu mệnh lệnh chủ động (Active Imperative Verbs)**: *Summarize, Refactor, Compare, Extract, Critique* thay vì viết dài dòng *"Can you maybe help me try to make this better?"*.

---

## 5. Bảng Từ vựng & IPA Giao tiếp Hiện đại

| Cụm từ hiện đại | IPA | Nghĩa tiếng Việt | Ví dụ mẫu |
|---|---|---|---|
| **heads-up** | `/ˈhedz ʌp/` | Lời báo trước nhanh | Just a quick heads-up: staging will restart in 10 minutes. |
| **wrap up** | `/ræp ʌp/` | Hoàn tất, chốt lại | We wrapped up the security audit ahead of schedule. |
| **follow up** | `/ˈfɒləʊ ʌp/` | Nhắc lại / theo dõi tiếp | I'm following up on my previous email regarding API keys. |
| **concise** | `/kənˈsaɪs/` | Súc tích, ngắn gọn mà đủ ý | Keep your executive summary concise and action-oriented. |
| **actionable** | `/ˈækʃənəbl/` | Có thể hành động ngay, thiết thực | Please give me three actionable steps to fix this bottleneck. |
| **clarify** | `/ˈklærəfaɪ/` | Làm sáng tỏ, nói rõ thêm | Could you clarify what format the client expects? |

---

## 6. Tổng ôn Kiểm tra Chốt khóa A2 → B1

### Bài tập sửa lỗi kinh điển người Việt:
1. ❌ *"Let's discuss about the AI prompt constraints."* $\to$ Sửa lại: **____________________**
2. ❌ *"I am very appreciate your help on this ticket."* $\to$ Sửa lại: **____________________**
3. ❌ *"Look forward to hear from you soon."* $\to$ Sửa lại: **____________________**

<details>
<summary>👉 Xem đáp án & Giải thích</summary>

1. **"Let's discuss the AI prompt constraints."** — Động từ ngoại động từ `discuss` đi thẳng với tân ngữ, KHÔNG dùng giới từ `about`.
2. **"I really appreciate your help with this ticket."** — `appreciate` là động từ thường, KHÔNG ghép với `am`! Dùng trạng từ `really/truly` thay cho `very`.
3. **"I look forward to hearing from you soon."** — Cụm `look forward to` luôn đi với danh động từ `V-ing` (`hearing`).

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->
"""";
    #endregion

    #region 6. English A2-B1 PracticeActivities (48 activities: 8 types x 6 lessons)

    private static List<PracticeActivity> GetEnExpandedActivities(Guid courseId, DateTimeOffset now)
    {
        var list = new List<PracticeActivity>(48);
        list.AddRange(BuildEn41Activities(courseId, now));
        list.AddRange(BuildEn42Activities(courseId, now));
        list.AddRange(BuildEn43Activities(courseId, now));
        list.AddRange(BuildEn51Activities(courseId, now));
        list.AddRange(BuildEn52Activities(courseId, now));
        list.AddRange(BuildEn53Activities(courseId, now));
        return list;
    }

    private static List<PracticeActivity> BuildEn41Activities(Guid c, DateTimeOffset now)
    {
        var l = EnLesson41Id;
        return new()
        {
            MakeEnActivity(c, l, 11, "shadowing",
                "Shadowing (Bài 4.1): Xuất nhập cảnh, Khách sạn & Sự cố Chuyến bay",
                "Nghe và nói đuổi theo ngữ điệu bản xứ qua 6 mẫu câu thực chiến sân bay & khách sạn.",
                new EnShItem[]
                {
                    new("en-l41-shad-e1", "en-US", "easy", "Khai báo hải quan (Dễ)", "I am visiting for a five-day tech conference.", "/aɪ æm ˈvɪzɪtɪŋ fɔːr ə faɪv deɪ tek ˈkɒnfərəns/", "Tôi tới đây tham dự hội thảo công nghệ 5 ngày.", new[] { "visiting for", "five-day conference" }),
                    new("en-l41-shad-e2", "en-US", "easy", "Nhận phòng khách sạn (Dễ)", "I would like to check in under the name Minh Tran.", "/aɪ wʊd laɪk tuː tʃek ɪn ˈʌndə ðə neɪm mɪn træn/", "Tôi muốn làm thủ tục nhận phòng dưới tên Minh Trần.", new[] { "check in", "under the name" }),
                    new("en-l41-shad-m1", "en-US", "medium", "Đổi chuyến bay bị hủy (Vừa)", "Could you help rebook me on the next available flight?", "/kəd juː help ˌriːˈbʊk miː ɒn ðə nekst əˈveɪləbl flaɪt/", "Bạn có thể đặt lại giúp tôi sang chuyến bay gần nhất còn chỗ không?", new[] { "rebook me", "next available flight" }),
                    new("en-l41-shad-m2", "en-US", "medium", "Hỏi quyền lợi phiếu ăn (Vừa)", "Since my layover is six hours, am I eligible for a meal voucher?", "/sɪns maɪ ˈleɪəʊvər ɪz sɪks ˈaʊəz æm aɪ ˈelɪdʒəbl fɔːr ə miːl ˈvaʊtʃə/", "Vì thời gian quá cảnh là 6 tiếng, tôi có đủ điều kiện nhận phiếu ăn không?", new[] { "eligible for", "meal voucher" }),
                    new("en-l41-shad-h1", "en-US", "hard", "Yêu cầu đổi phòng yên tĩnh (Khó)", "Would it be possible to move to a quieter room away from the elevator?", "/wʊd ɪt biː ˈpɒsəbl tuː muːv tuː ə ˈkwaɪətə ruːm əˈweɪ frəm ði ˈelɪveɪtə/", "Liệu tôi có thể chuyển sang một phòng yên tĩnh hơn cách xa thang máy không?", new[] { "would it be possible", "away from the elevator" }),
                    new("en-l41-shad-h2", "en-US", "hard", "Báo cáo thất lạc hành lý (Khó)", "My checked luggage did not arrive at baggage claim, so I need to file a report.", "/maɪ tʃekt ˈlʌɡɪdʒ dɪd nɒt əˈraɪv æt ˈbæɡɪdʒ kleɪm səʊ aɪ niːd tuː faɪl ə rɪˈpɔːt/", "Hành lý ký gửi của tôi không xuất hiện ở băng chuyền nhận đồ, nên tôi cần lập biên bản.", new[] { "checked luggage", "baggage claim", "file a report" })
                }, now),
            MakeEnActivity(c, l, 12, "dictation",
                "Dictation (Bài 4.1): Nghe chép chính tả Du lịch & Sân bay",
                "Nghe và chép lại hoặc ghép word chips cho các tình huống sân bay, khách sạn.",
                new EnDiItem[]
                {
                    new("en-l41-dict-e1", "en-US", "easy", "Hỏi đồ khai báo (Dễ)", "Nothing to declare except my personal laptop.", "/ˈnʌθɪŋ tuː dɪˈkleər ɪkˈsept maɪ ˈpɜːsənl ˈlæptɒp/", "Không có gì phải khai báo ngoại trừ chiếc máy tính xách tay cá nhân.", new[] { "Nothing", "to", "declare", "except", "my", "personal", "laptop" }, new[] { "nothingness", "claim", "private" }),
                    new("en-l41-dict-e2", "en-US", "easy", "Trả phòng muộn (Dễ)", "Can I request a late check-out until two PM?", "/kæn aɪ rɪˈkwest ə leɪt tʃek aʊt ənˈtɪl tuː piː em/", "Tôi có thể yêu cầu trả phòng muộn tới 2 giờ chiều được không?", new[] { "Can", "I", "request", "a", "late", "check-out", "until", "two", "PM" }, new[] { "ask", "early", "check-in" }),
                    new("en-l41-dict-m1", "en-US", "medium", "Xuất trình lịch trình (Vừa)", "Here is my confirmed hotel booking and return itinerary.", "/hɪər ɪz maɪ kənˈfɜːmd həʊˈtel ˈbʊkɪŋ ænd rɪˈtɜːn aɪˈtɪnərəri/", "Đây là xác nhận đặt phòng khách sạn và lịch trình bay khứ hồi của tôi.", new[] { "Here", "is", "my", "confirmed", "hotel", "booking", "and", "return", "itinerary" }, new[] { "ticket", "schedule", "receipt" }),
                    new("en-l41-dict-m2", "en-US", "medium", "Hoãn chuyến do thời tiết (Vừa)", "Flight four zero two was delayed due to heavy fog.", "/flaɪt fɔː ˈzɪərəʊ tuː wɒz dɪˈleɪd djuː tuː ˈhevi fɒɡ/", "Chuyến bay 402 đã bị hoãn do sương mù dày đặc.", new[] { "Flight", "four", "zero", "two", "was", "delayed", "due", "to", "heavy", "fog" }, new[] { "cancelled", "because", "strong" }),
                    new("en-l41-dict-h1", "en-US", "hard", "Giao hành lý tận khách sạn (Khó)", "The airline promised to deliver my misplaced suitcase directly to the hotel.", "/ði ˈeəlaɪn ˈprɒmɪst tuː dɪˈlɪvə maɪ ˌmɪsˈpleɪst ˈsuːtkeɪs dɪˈrektli tuː ðə həʊˈtel/", "Hãng hàng không hứa sẽ giao chiếc vali thất lạc của tôi thẳng tới khách sạn.", new[] { "The", "airline", "promised", "to", "deliver", "my", "misplaced", "suitcase", "directly", "to", "the", "hotel" }, new[] { "airport", "bag", "send" }),
                    new("en-l41-dict-h2", "en-US", "hard", "Hoàn tiền đặt cọc (Khó)", "Your refundable security deposit will be released upon check-out inspection.", "/jɔː rɪˈfʌndəbl sɪˈkjʊərəti dɪˈpɒzɪt wɪl biː rɪˈliːst əˈpɒn tʃek aʊt ɪnˈspekʃn/", "Khoản tiền đặt cọc có thể hoàn lại của bạn sẽ được giải tỏa sau khi kiểm tra trả phòng.", new[] { "Your", "refundable", "security", "deposit", "will", "be", "released", "upon", "check-out", "inspection" }, new[] { "charged", "credit", "arrival" })
                }, now),
            MakeEnActivity(c, l, 13, "pronunciation",
                "Phát âm IPA (Bài 4.1): Luyện âm câm & trọng âm từ vựng sân bay",
                "Đọc to câu mẫu để sửa lỗi phát âm từ receipt, itinerary, customs, luggage.",
                new EnPrItem[]
                {
                    new("en-l41-pron-e1", "en-US", "easy", "Âm /p/ câm trong receipt (Dễ)", "Please keep this official receipt for reimbursement.", "/pliːz kiːp ðɪs əˈfɪʃl rɪˈsiːt fɔː ˌriːɪmˈbɜːsmənt/", "Vui lòng giữ hóa đơn chính thức này để thanh toán công tác phí.", new[] { "receipt", "official", "keep" }, new[] { "official receipt" }),
                    new("en-l41-pron-e2", "en-US", "easy", "Âm cuối /z/ trong customs (Dễ)", "We walked smoothly through airport customs.", "/wiː wɔːkt ˈsmuːðli θruː ˈeəpɔːt ˈkʌstəmz/", "Chúng tôi đã đi qua hải quan sân bay một cách suôn sẻ.", new[] { "customs", "smoothly", "through" }, new[] { "airport customs" }),
                    new("en-l41-pron-m1", "en-US", "medium", "Trọng âm itinerary (Vừa)", "Let me double-check my updated flight itinerary.", "/let miː ˈdʌbl tʃek maɪ ʌpˈdeɪtɪd flaɪt aɪˈtɪnərəri/", "Để tôi kiểm tra lại lịch trình bay đã cập nhật của mình.", new[] { "itinerary", "updated", "double-check" }, new[] { "flight itinerary" }),
                    new("en-l41-pron-m2", "en-US", "medium", "Âm /dʒ/ trong luggage (Vừa)", "Only two pieces of checked luggage are permitted.", "/ˈəʊnli tuː ˈpiːsɪz əv tʃekt ˈlʌɡɪdʒ ɑː pəˈmɪtɪd/", "Chỉ hai kiện hành lý ký gửi được phép mang theo.", new[] { "luggage", "pieces", "permitted" }, new[] { "checked luggage" }),
                    new("en-l41-pron-h1", "en-US", "hard", "Phát âm eligible & voucher (Khó)", "Passengers facing overnight delays are eligible for complimentary hotel vouchers.", "/ˈpæsɪndʒəz ˈfeɪsɪŋ ˌəʊvəˈnaɪt dɪˈleɪz ɑːr ˈelɪdʒəbl fɔː ˌkɒmplɪˈmentri həʊˈtel ˈvaʊtʃəz/", "Hành khách gặp sự cố hoãn qua đêm đủ điều kiện nhận phiếu phòng khách sạn miễn phí.", new[] { "eligible", "complimentary", "vouchers" }, new[] { "eligible for", "complimentary vouchers" }),
                    new("en-l41-pron-h2", "en-US", "hard", "Cụm phụ âm irregularity (Khó)", "Ask the baggage service counter for a property irregularity report.", "/ɑːsk ðə ˈbæɡɪdʒ ˈsɜːvɪs ˈkaʊntə fɔːr ə ˈprɒpəti ɪˌreɡjʊˈlærəti rɪˈpɔːt/", "Hãy hỏi quầy dịch vụ hành lý để lập biên bản thất lạc tài sản.", new[] { "irregularity", "property", "counter" }, new[] { "irregularity report" })
                }, now),
            MakeEnActivity(c, l, 14, "word_map",
                "Word Map (Bài 4.1): Mạng lưới từ vựng Hàng không & Khách sạn",
                "Khám phá collocations, phrasal verbs và từ đồng nghĩa cho các từ khóa du lịch.",
                new EnWmItem[]
                {
                    new("wm-l41-e1", "en-US", "easy", "Sơ đồ từ vựng: flight", "flight", "/flaɪt/", "Chuyến bay",
                        new[] { new EnWmNode("connecting flight", "/kəˈnektɪŋ flaɪt/", "Chuyến bay nối chuyến", "I missed my connecting flight in Bangkok."), new EnWmNode("direct flight", "/daɪˈrekt flaɪt/", "Chuyến bay thẳng", "We booked a direct flight to Singapore.") },
                        new[] { new EnWmNode("take off", "/teɪk ɒf/", "Cất cánh", "The flight took off twenty minutes late."), new EnWmNode("touch down", "/tʌtʃ daʊn/", "Hạ cánh", "Our plane touched down safely at dawn.") },
                        new[] { new EnWmNode("journey", "/ˈdʒɜːni/", "Hành trình", "Have a safe journey back home."), new EnWmNode("departure", "/dɪˈpɑːtʃə/", "Sự khởi hành", "Check the departure board for gate updates.") },
                        new[] { new EnWmEx("Heavy fog delayed our evening flight by two hours.", "Sương mù dày đã làm hoãn chuyến bay tối của chúng tôi 2 tiếng.") }),
                    new("wm-l41-e2", "en-US", "easy", "Sơ đồ từ vựng: booking", "booking", "/ˈbʊkɪŋ/", "Việc đặt chỗ / đặt phòng",
                        new[] { new EnWmNode("confirmed booking", "/kənˈfɜːmd ˈbʊkɪŋ/", "Đặt chỗ đã xác nhận", "Show your confirmed booking at reception."), new EnWmNode("cancel a booking", "/ˈkænsl ə ˈbʊkɪŋ/", "Hủy đặt phòng", "You can cancel a booking free until Tuesday.") },
                        new[] { new EnWmNode("check in", "/tʃek ɪn/", "Nhận phòng / làm thủ tục bay", "We checked in online before arriving."), new EnWmNode("upgrade to", "/ˌʌpˈɡreɪd tuː/", "Nâng hạng lên", "They upgraded our booking to a suite.") },
                        new[] { new EnWmNode("reservation", "/ˌrezəˈveɪʃn/", "Sự đặt trước", "I have a hotel reservation under Tran."), new EnWmNode("itinerary", "/aɪˈtɪnərəri/", "Lịch trình đặt vé", "Print your itinerary for immigration.") },
                        new[] { new EnWmEx("Please present your confirmed booking confirmation at check-in.", "Vui lòng xuất trình xác nhận đặt phòng khi làm thủ tục nhận phòng.") }),
                    new("wm-l41-m1", "en-US", "medium", "Sơ đồ từ vựng: declare", "declare", "/dɪˈkleər/", "Khai báo (hải quan)",
                        new[] { new EnWmNode("nothing to declare", "/ˈnʌθɪŋ tuː dɪˈkleər/", "Không có hàng hóa phải khai báo", "Walk through the green lane if you have nothing to declare."), new EnWmNode("customs declaration", "/ˈkʌstəmz ˌdekləˈreɪʃn/", "Tờ khai hải quan", "Complete the customs declaration form before landing.") },
                        new[] { new EnWmNode("pass through", "/pɑːs θruː/", "Đi qua (cửa kiểm tra)", "Travelers pass through passport control first."), new EnWmNode("fill out", "/fɪl aʊt/", "Điền đầy đủ tờ khai", "Fill out the arrival card accurately.") },
                        new[] { new EnWmNode("disclose", "/dɪsˈkləʊz/", "Tiết lộ khai trình", "Travelers must disclose large sums of cash."), new EnWmNode("report", "/rɪˈpɔːt/", "Báo cáo khai báo", "Report lost goods at the desk.") },
                        new[] { new EnWmEx("All incoming visitors must declare restricted items at customs.", "Mọi du khách nhập cảnh phải khai báo các mặt hàng hạn chế tại hải quan.") }),
                    new("wm-l41-m2", "en-US", "medium", "Sơ đồ từ vựng: rebook", "rebook", "/ˌriːˈbʊk/", "Đặt lại vé/chuyến khác",
                        new[] { new EnWmNode("rebook free of charge", "/ˌriːˈbʊk friː əv tʃɑːdʒ/", "Đổi chuyến miễn phí", "Affected passengers can rebook free of charge."), new EnWmNode("next available flight", "/nekst əˈveɪləbl flaɪt/", "Chuyến sớm nhất còn chỗ", "They rebooked me on the next available flight.") },
                        new[] { new EnWmNode("transfer to", "/trænsˈfɜː tuː/", "Chuyển tiếp sang", "Your luggage will transfer to the new plane."), new EnWmNode("stand by for", "/stænd baɪ fɔː/", "Chờ ghế dự phòng", "We stood by for an earlier departure.") },
                        new[] { new EnWmNode("reschedule", "/ˌriːˈʃedjuːl/", "Sắp xếp lại lịch", "Can I reschedule my return flight?"), new EnWmNode("reroute", "/ˌriːˈruːt/", "Chuyển hướng tuyến bay", "The airline rerouted us through Seoul.") },
                        new[] { new EnWmEx("The service agent kindly rebooked me without charging a penalty fee.", "Nhân viên phục vụ đã vui vẻ đổi chuyến giúp tôi mà không thu phí phạt.") }),
                    new("wm-l41-h1", "en-US", "hard", "Sơ đồ từ vựng: eligible", "eligible", "/ˈelɪdʒəbl/", "Đủ điều kiện hưởng quyền lợi",
                        new[] { new EnWmNode("eligible for compensation", "/ˈelɪdʒəbl fɔː ˌkɒmpenˈseɪʃn/", "Đủ điều kiện được bồi thường", "You are eligible for compensation after a 4-hour delay."), new EnWmNode("eligible for an upgrade", "/ˈelɪdʒəbl fɔːr ən ˈʌpɡreɪd/", "Đủ điều kiện nâng hạng", "Frequent flyers are eligible for a complimentary upgrade.") },
                        new[] { new EnWmNode("qualify for", "/ˈkwɒlɪfaɪ fɔː/", "Đạt chuẩn nhận quyền lợi", "Overnight layovers qualify for hotel stays."), new EnWmNode("claim back", "/kleɪm bæk/", "Đòi bồi hoàn lại chi phí", "Keep receipts to claim back meal expenses.") },
                        new[] { new EnWmNode("entitled", "/ɪnˈtaɪtld/", "Có quyền được hưởng", "Guests are entitled to a full refund."), new EnWmNode("qualified", "/ˈkwɒlɪfaɪd/", "Đủ tiêu chuẩn", "Only qualified tickets get lounge access.") },
                        new[] { new EnWmEx("Passengers delayed overnight are legally eligible for hotel accommodation.", "Hành khách bị hoãn qua đêm đủ điều kiện pháp lý được bố trí khách sạn.") }),
                    new("wm-l41-h2", "en-US", "hard", "Sơ đồ từ vựng: complimentary", "complimentary", "/ˌkɒmplɪˈmentri/", "Miễn phí (tặng kèm dịch vụ)",
                        new[] { new EnWmNode("complimentary shuttle", "/ˌkɒmplɪˈmentri ˈʃʌtl/", "Xe đưa đón miễn phí", "The hotel runs a complimentary airport shuttle."), new EnWmNode("complimentary lounge access", "/ˌkɒmplɪˈmentri laʊndʒ ˈækses/", "Vào phòng chờ miễn phí", "Business tickets include complimentary lounge access.") },
                        new[] { new EnWmNode("hand out", "/hænd aʊt/", "Phát ra cho khách", "Staff handed out complimentary water bottles."), new EnWmNode("waive off", "/weɪv ɒf/", "Miễn trừ phí", "The manager waived the early check-in fee.") },
                        new[] { new EnWmNode("free of charge", "/friː əv tʃɑːdʒ/", "Hoàn toàn miễn phí", "Breakfast is provided free of charge."), new EnWmNode("courtesy", "/ˈkɜːtəsi/", "Ưu đãi tri ân", "A courtesy car picked us up.") },
                        new[] { new EnWmEx("As a goodwill gesture, the hotel offered complimentary breakfast for our entire stay.", "Như một cử chỉ thiện chí, khách sạn đã tặng bữa sáng miễn phí suốt kỳ lưu trú.") })
                }, now),
            MakeEnActivity(c, l, 15, "cloze",
                "Điền từ ngữ cảnh (Bài 4.1): Hải quan, Khách sạn & Sân bay",
                "Hoàn chỉnh 5 câu giao tiếp thực tế với gợi ý và giải thích chi tiết.",
                new EnClItem[]
                {
                    new("en-l41-cloze-1", "easy", "Khai báo mục đích chuyến đi (Dễ)", "Điền giới từ và cụm tính từ ghép:", "I am visiting Singapore {{0}} a {{1}} tech conference.", new[] { new EnClBlank(new[] { "for" }, "chỉ mục đích chuyến đi", "Dùng 'visit [place] for [purpose]'."), new EnClBlank(new[] { "five-day", "5-day" }, "tính từ ghép 5 ngày (không có s)", "Tính từ ghép có gạch nối không thêm -s.") }, "I am visiting Singapore for a five-day tech conference.", "Tôi đến Singapore để tham dự hội thảo công nghệ 5 ngày.", "Nhớ không thêm 's' vào 'day' khi đứng trước danh từ."),
                    new("en-l41-cloze-2", "easy", "Nhận phòng khách sạn (Dễ)", "Điền cụm động từ và giới từ đặt phòng:", "I would like to {{0}} in {{1}} the name Minh Tran.", new[] { new EnClBlank(new[] { "check" }, "làm thủ tục nhận phòng (___ in)", "'check in' = nhận phòng."), new EnClBlank(new[] { "under" }, "dưới tên (___ the name)", "'under the name [Name]' là cụm chuẩn.") }, "I would like to check in under the name Minh Tran.", "Tôi muốn nhận phòng dưới tên Minh Trần.", "Phân biệt check in (nhận phòng) và check out (trả phòng)."),
                    new("en-l41-cloze-3", "medium", "Yêu cầu đổi chuyến bay (Vừa)", "Điền từ khóa đổi vé và chuyến kế tiếp:", "My connecting flight was cancelled; could you {{0}} me on the next {{1}} flight?", new[] { new EnClBlank(new[] { "rebook", "put" }, "đặt lại sang chuyến khác", "'rebook someone on a flight'."), new EnClBlank(new[] { "available" }, "còn chỗ trống (next ___ flight)", "'next available flight' = chuyến sớm nhất còn ghế.") }, "My connecting flight was cancelled; could you rebook me on the next available flight?", "Chuyến nối của tôi bị hủy; bạn đặt lại giúp tôi sang chuyến sớm nhất còn chỗ được không?", "Mẫu câu vàng khi bị hủy chuyến tại sân bay."),
                    new("en-l41-cloze-4", "medium", "Hỏi quyền lợi phiếu ăn (Vừa)", "Điền tính từ điều kiện và phiếu ăn:", "Since the delay exceeds four hours, am I {{0}} for a meal {{1}}?", new[] { new EnClBlank(new[] { "eligible", "qualified" }, "đủ điều kiện (___ for)", "'eligible for' = đủ tiêu chuẩn hưởng."), new EnClBlank(new[] { "voucher", "coupon" }, "phiếu ăn uống", "'meal voucher' = phiếu ăn miễn phí hãng phát.") }, "Since the delay exceeds four hours, am I eligible for a meal voucher?", "Vì trễ quá 4 tiếng, tôi có đủ điều kiện nhận phiếu ăn không?", "Hỏi lịch sự quyền lợi chính đáng của hành khách."),
                    new("en-l41-cloze-5", "hard", "Đặt cọc & Trả phòng (Khó)", "Điền danh từ chuyên ngành khách sạn:", "The hotel places a temporary security {{0}} on your card and releases it upon {{1}}.", new[] { new EnClBlank(new[] { "deposit", "hold" }, "tiền đặt cọc (security ___)", "'security deposit' = tiền cọc bảo đảm."), new EnClBlank(new[] { "check-out", "checkout", "departure" }, "lúc trả phòng", "'upon check-out' = ngay khi trả phòng.") }, "The hotel places a temporary security deposit on your card and releases it upon check-out.", "Khách sạn tạm giữ tiền đặt cọc trên thẻ và giải tỏa khi bạn trả phòng.", "Từ vựng chuẩn xác khi làm thủ tục khách sạn quốc tế.")
                }, now),
            MakeEnActivity(c, l, 16, "translate",
                "Dịch phản xạ Việt → Anh (Bài 4.1): Giao tiếp Sân bay & Khách sạn",
                "Chuyển câu tiếng Việt sang tiếng Anh tự nhiên với nhiều phương án đúng.",
                new EnTrItem[]
                {
                    new("en-l41-trans-1", "easy", "Hải quan: Không có gì khai báo (Dễ)", "Tôi không có gì để khai báo ngoại trừ đồ dùng cá nhân và laptop làm việc.", new[] { "nothing to declare", "personal belongings", "work laptop" }, new[] { "I have nothing to declare except personal belongings and my work laptop.", "Nothing to declare except my personal belongings and work laptop." }, "I have nothing to declare except personal belongings and my work laptop.", "Dùng danh từ số nhiều 'personal belongings'."),
                    new("en-l41-trans-2", "easy", "Khách sạn: Xin trả phòng muộn (Dễ)", "Liệu tôi có thể xin trả phòng muộn đến 2 giờ chiều được không?", new[] { "would it be possible", "late check-out", "until 2 PM" }, new[] { "Would it be possible to get a late check-out until 2 PM?", "Could I possibly get a late check-out until two PM?" }, "Would it be possible to get a late check-out until 2 PM?", "Mẫu 'Would it be possible to + V-inf' rất lịch thiệp."),
                    new("en-l41-trans-3", "medium", "Sân bay: Hỏi quầy đổi chuyến (Vừa)", "Chuyến bay nối của tôi bị hủy do sương mù. Bạn có thể đặt lại cho tôi chuyến sớm nhất còn chỗ không?", new[] { "connecting flight", "due to fog", "rebook me", "next available" }, new[] { "My connecting flight was cancelled due to fog. Could you rebook me on the next available flight?", "My connecting flight got cancelled because of fog. Can you put me on the next available flight?" }, "My connecting flight was cancelled due to fog. Could you rebook me on the next available flight?", "Dùng bị động 'was cancelled due to + Noun'."),
                    new("en-l41-trans-4", "medium", "Khách sạn: Đổi phòng yên tĩnh (Vừa)", "Phòng hiện tại hơi ồn gần thang máy; bạn có thể chuyển tôi sang phòng yên tĩnh hơn được không?", new[] { "near the elevator", "move to a quieter room" }, new[] { "My current room is a bit noisy near the elevator; could you move me to a quieter room?", "The room is quite noisy near the elevator. Would it be possible to move to a quieter room?" }, "My current room is a bit noisy near the elevator; could you move me to a quieter room?", "So sánh hơn tính từ ngắn 2 âm tiết tận cùng -y: quiet -> quieter."),
                    new("en-l41-trans-5", "hard", "Hành lý: Yêu cầu giao tới khách sạn (Khó)", "Nếu hành lý ký gửi của tôi không kịp lên chuyến bay mới, hãng có thể giao thẳng tới khách sạn của tôi không?", new[] { "checked luggage", "make it onto", "deliver it directly" }, new[] { "If my checked luggage doesn't make it onto the new flight, could the airline deliver it directly to my hotel?", "If my checked bag does not arrive on the new flight, can you deliver it straight to my hotel?" }, "If my checked luggage doesn't make it onto the new flight, could the airline deliver it directly to my hotel?", "Câu điều kiện loại 1 dùng Hiện tại đơn trong mệnh đề If.")
                }, now),
            MakeEnActivity(c, l, 17, "grammar_fix",
                "Sửa lỗi ngữ pháp (Bài 4.1): Bẫy danh từ & bị động khi đi du lịch",
                "Phát hiện và sửa 5 lỗi ngữ pháp người Việt thường mắc tại sân bay và khách sạn.",
                new EnGfItem[]
                {
                    new("en-l41-gf-1", "easy", "Danh từ không đếm được luggage (Dễ)", "I have two checked luggages for this flight.", "luggage là danh từ không đếm được, không thêm -s; hãy dùng 'two pieces of luggage' hoặc 'two checked bags'.", "I have two pieces of checked luggage for this flight.", new[] { "I have two pieces of checked luggage for this flight.", "I have two checked bags for this flight." }, "Tôi có hai kiện hành lý ký gửi cho chuyến bay này.", "luggage và baggage luôn ở dạng số ít."),
                    new("en-l41-gf-2", "easy", "Tính từ ghép chỉ thời lượng (Dễ)", "I am attending a five-days conference in Tokyo.", "Tính từ ghép có gạch nối đứng trước danh từ không thêm -s.", "I am attending a five-day conference in Tokyo.", new[] { "I am attending a five-day conference in Tokyo." }, "Tôi đang tham dự một hội thảo 5 ngày ở Tokyo.", "Nói 'a five-day conference', không nói 'five-days'."),
                    new("en-l41-gf-3", "medium", "Thể bị động cho chuyến bay bị hoãn (Vừa)", "My flight was delay because of bad weather.", "Sau 'was' cần phân từ hai (delayed) để tạo thể bị động.", "My flight was delayed because of bad weather.", new[] { "My flight was delayed because of bad weather.", "My flight was delayed due to bad weather." }, "Chuyến bay của tôi bị hoãn do thời tiết xấu.", "Chuyến bay bị tác động -> thể bị động 'was delayed'."),
                    new("en-l41-gf-4", "medium", "Trật tự câu hỏi gián tiếp (Vừa)", "Could you tell me where is the baggage claim counter?", "Câu hỏi gián tiếp không đảo 'is' lên trước chủ ngữ.", "Could you tell me where the baggage claim counter is?", new[] { "Could you tell me where the baggage claim counter is?" }, "Bạn có thể chỉ giúp tôi quầy nhận hành lý thất lạc ở đâu không?", "Đưa động từ 'is' về cuối mệnh đề hỏi gián tiếp."),
                    new("en-l41-gf-5", "hard", "Cấu trúc 'Would it be possible to' (Khó)", "Would it be possible getting a complimentary room upgrade?", "Sau cấu trúc 'Would it be possible' phải dùng 'to + V-infinitive'.", "Would it be possible to get a complimentary room upgrade?", new[] { "Would it be possible to get a complimentary room upgrade?", "Would it be possible to have a complimentary room upgrade?" }, "Liệu tôi có thể được nâng hạng phòng miễn phí không?", "Cấu trúc chuẩn: Would it be possible + to V.")
                }, now),
            MakeEnActivity(c, l, 18, "roleplay",
                "Nhập vai hội thoại (Bài 4.1): Hải quan, Khách sạn & Sân bay",
                "Thực hành phản xạ trực tiếp 4 tình huống xuất nhập cảnh và xử lý sự cố chuyến bay.",
                new EnRpItem[]
                {
                    new("en-l41-rp-1", "easy", "Trả lời Hải quan xuất nhập cảnh (Dễ)", "Bạn đang đứng trước quầy Hải quan tại sân bay Singapore.", "Nhân viên Hải quan", "Good morning. What is the purpose of your visit, and how long will you be staying?", "Chào buổi sáng. Mục đích chuyến thăm của bạn là gì và bạn sẽ lưu trú bao lâu?", "Trả lời rõ đi công tác/hội thảo bao nhiêu ngày và tên khách sạn lưu trú.", new[] { "conference", "days", "staying", "hotel" }, new[] { "Good morning! I am visiting for a five-day tech conference and will be staying at the Marina Bay Hotel." }, 10),
                    new("en-l41-rp-2", "medium", "Xin đổi phòng yên tĩnh tại khách sạn (Vừa)", "Phòng bạn vừa nhận nằm ngay cạnh thang máy rất ồn ào.", "Lễ tân Khách sạn", "Welcome to Grand Hotel! Is everything satisfactory with your room so far?", "Chào mừng tới Grand Hotel! Mọi thứ về phòng của bạn ổn cả chứ?", "Cảm ơn lịch sự và nhờ đổi sang phòng yên tĩnh hơn xa thang máy.", new[] { "noisy", "elevator", "possible", "quieter room" }, new[] { "Thank you for asking! Actually, my room is a bit noisy near the elevator. Would it be possible to move to a quieter room?" }, 12),
                    new("en-l41-rp-3", "medium", "Đổi chuyến bay bị hủy & xin phiếu ăn (Vừa)", "Chuyến nối chuyến của bạn bị hủy vì thời tiết, chuyến mới đợi 6 tiếng.", "Nhân viên Hãng bay", "I'm sorry, flight VN402 has been cancelled due to fog. How can I assist you?", "Rất tiếc chuyến VN402 đã bị hủy do sương mù. Tôi có thể hỗ trợ gì cho bạn?", "Đề nghị đặt lại sang chuyến sớm nhất còn chỗ và hỏi về phiếu ăn (meal voucher).", new[] { "rebook", "next available", "eligible", "voucher" }, new[] { "Could you please rebook me on the next available flight? Also, since it is a long layover, am I eligible for a meal voucher?" }, 14),
                    new("en-l41-rp-4", "hard", "Khai báo thất lạc hành lý tại sân bay (Khó)", "Vali ký gửi của bạn không ra trên băng chuyền ở sân bay đến.", "Nhân viên Quầy Hành lý", "Hello, this is Baggage Services. You look worried—did your suitcase not arrive?", "Xin chào, đây là Quầy Hành lý. Trông bạn có vẻ lo lắng—vali của bạn chưa tới à?", "Cung cấp mã thẻ hành lý, đề nghị lập biên bản PIR và yêu cầu giao thẳng tới khách sạn.", new[] { "checked luggage", "baggage claim", "report", "deliver", "hotel" }, new[] { "Yes, my checked luggage didn't show up at baggage claim. I would like to file an irregularity report and have it delivered directly to my hotel once located." }, 16)
                }, now)
        };
    }

    private static List<PracticeActivity> BuildEn42Activities(Guid c, DateTimeOffset now)
    {
        var l = EnLesson42Id;
        return new()
        {
            MakeEnActivity(c, l, 11, "shadowing",
                "Shadowing (Bài 4.2): Phỏng vấn STAR & Giải thích Kỹ thuật",
                "Luyện nói trôi chảy câu trả lời phỏng vấn STAR và phân tích trade-offs.",
                new EnShItem[]
                {
                    new("en-l42-shad-e1", "en-US", "easy", "Mở đầu tình huống STAR (Dễ)", "Last quarter, our checkout service experienced severe timeout spikes.", "/lɑːst ˈkwɔːtər ˈaʊə ˈtʃekaʊt ˈsɜːvɪs ɪkˈspɪəriənst sɪˈvɪə ˈtaɪmaʊt spaɪks/", "Quý trước, dịch vụ thanh toán của chúng tôi gặp tình trạng quá thời gian nghiêm trọng.", new[] { "experienced", "timeout spikes" }),
                    new("en-l42-shad-e2", "en-US", "easy", "Nêu vai trò cá nhân (Dễ)", "As the on-call engineer, my responsibility was to isolate the root cause.", "/æz ði ɒn kɔːl ˌendʒɪˈnɪə maɪ rɪˌspɒnsəˈbɪləti wɒz tuː ˈaɪsəleɪt ðə ruːt kɔːz/", "Với vai trò kỹ sư trực sự cố, trách nhiệm của tôi là khoanh vùng nguyên nhân gốc rễ.", new[] { "on-call engineer", "root cause" }),
                    new("en-l42-shad-m1", "en-US", "medium", "Mô tả hành động tối ưu (Vừa)", "I diagnosed an unindexed query and rolled out a composite index immediately.", "/aɪ ˈdaɪəɡnəʊzd ən ʌnˈɪndekst ˈkwɪəri ænd rəʊld aʊt ə ˈkɒmpəzɪt ˈɪndeks ɪˈmiːdiətli/", "Tôi phát hiện truy vấn thiếu chỉ mục và lập tức triển khai composite index.", new[] { "diagnosed", "rolled out", "composite index" }),
                    new("en-l42-shad-m2", "en-US", "medium", "Báo cáo kết quả đo lường (Vừa)", "As a result, p99 latency dropped from three seconds to forty-five milliseconds.", "/æz ə rɪˈzʌlt piː ˈnaɪnti naɪn ˈleɪtənsi drɒpt frəm θriː ˈsekəndz tuː ˈfɔːti faɪv ˈmɪlɪsekəndz/", "Kết quả là độ trễ p99 giảm từ 3 giây xuống còn 45 mili-giây.", new[] { "as a result", "latency dropped" }),
                    new("en-l42-shad-h1", "en-US", "hard", "Phân tích Trade-off kiến trúc (Khó)", "Every architectural choice involves a trade-off between consistency and horizontal scalability.", "/ˈevri ˌɑːkɪˈtektʃərəl tʃɔɪs ɪnˈvɒlvz ə ˈtreɪd ɒf bɪˈtwiːn kənˈsɪstənsi ænd ˌhɒrɪˈzɒntl ˌskeɪləˈbɪləti/", "Mọi lựa chọn kiến trúc đều có sự đánh đổi giữa tính nhất quán và khả năng mở rộng ngang.", new[] { "trade-off", "consistency", "scalability" }),
                    new("en-l42-shad-h2", "en-US", "hard", "Đặt câu hỏi ngược nhà tuyển dụng (Khó)", "How does your engineering team balance shipping features quickly against paying down technical debt?", "/haʊ dʌz jɔːr ˌendʒɪˈnɪərɪŋ tiːm ˈbæləns ˈʃɪpɪŋ ˈfiːtʃəz ˈkwɪkli əˈɡenst ˈpeɪɪŋ daʊn ˈteknɪkl det/", "Đội ngũ kỹ thuật của anh/chị cân bằng giữa tốc độ ra tính năng và xử lý nợ kỹ thuật như thế nào?", new[] { "balance shipping", "technical debt" })
                }, now),
            MakeEnActivity(c, l, 12, "dictation",
                "Dictation (Bài 4.2): Nghe chép câu trả lời Phỏng vấn Kỹ thuật",
                "Rèn nghe các cụm động từ và số liệu trong phỏng vấn công nghệ.",
                new EnDiItem[]
                {
                    new("en-l42-dict-e1", "en-US", "easy", "Nêu nhiệm vụ STAR (Dễ)", "I was tasked with reducing API memory usage.", "/aɪ wɒz tɑːskt wɪð rɪˈdjuːsɪŋ eɪ piː aɪ ˈmeməri ˈjuːsɪdʒ/", "Tôi được giao nhiệm vụ giảm mức tiêu thụ RAM của API.", new[] { "I", "was", "tasked", "with", "reducing", "API", "memory", "usage" }, new[] { "asked", "for", "increasing" }),
                    new("en-l42-dict-e2", "en-US", "easy", "Kết quả cụ thể (Dễ)", "Consequently, deployment time fell by sixty percent.", "/ˈkɒnsɪkwəntli dɪˈplɔɪmənt taɪm fel baɪ ˈsɪksti pəˈsent/", "Nhờ vậy, thời gian triển khai đã giảm đi 60%.", new[] { "Consequently", "deployment", "time", "fell", "by", "sixty", "percent" }, new[] { "Therefore", "dropped", "to" }),
                    new("en-l42-dict-m1", "en-US", "medium", "Xin làm rõ đề bài (Vừa)", "Could I clarify one constraint regarding expected traffic volume?", "/kəd aɪ ˈklærəfaɪ wʌn kənˈstreɪnt rɪˈɡɑːdɪŋ ɪkˈspektɪd ˈtræfɪk ˈvɒljuːm/", "Tôi có thể làm rõ một ràng buộc về lưu lượng truy cập dự kiến không?", new[] { "Could", "I", "clarify", "one", "constraint", "regarding", "expected", "traffic", "volume" }, new[] { "explain", "limit", "users" }),
                    new("en-l42-dict-m2", "en-US", "medium", "Giải thích Caching (Vừa)", "Caching improves read throughput but adds invalidation complexity.", "/ˈkæʃɪŋ ɪmˈpruːvz riːd ˈθruːpʊt bʌt ædz ɪnˌvælɪˈdeɪʃn kəmˈpleksəti/", "Bộ nhớ đệm tăng thông lượng đọc nhưng làm tăng độ phức tạp khi xóa cache cũ.", new[] { "Caching", "improves", "read", "throughput", "but", "adds", "invalidation", "complexity" }, new[] { "Storage", "write", "removes" }),
                    new("en-l42-dict-h1", "en-US", "hard", "Xử lý câu hỏi chưa biết (Khó)", "I have not deployed Kafka in production yet, though I understand event-driven patterns.", "/aɪ hæv nɒt dɪˈplɔɪd ˈkæfkə ɪn prəˈdʌkʃn jet ðəʊ aɪ ˌʌndəˈstænd ɪˈvent ˈdrɪvn ˈpætnz/", "Tôi chưa triển khai Kafka trên môi trường sản xuất, dù tôi nắm vững các mẫu thiết kế hướng sự kiện.", new[] { "I", "have", "not", "deployed", "Kafka", "in", "production", "yet", "though", "I", "understand", "event-driven", "patterns" }, new[] { "used", "already", "because" }),
                    new("en-l42-dict-h2", "en-US", "hard", "Bài học sau sự cố (Khó)", "During our post-incident retrospective, we added automated load tests to prevent regressions.", "/ˈdjʊərɪŋ ˈaʊə pəʊst ˈɪnsɪdənt ˌretrəˈspektɪv wiː ˈædɪd ˈɔːtəmeɪtɪd ləʊd tests tuː prɪˈvent rɪˈɡreʃnz/", "Trong buổi họp rút kinh nghiệm sau sự cố, chúng tôi đã bổ sung kiểm thử tải tự động để chống tái diễn.", new[] { "During", "our", "post-incident", "retrospective", "we", "added", "automated", "load", "tests", "to", "prevent", "regressions" }, new[] { "after", "manual", "errors" })
                }, now),
            MakeEnActivity(c, l, 13, "pronunciation",
                "Phát âm IPA (Bài 4.2): Đuôi -ed & Thuật ngữ Phỏng vấn Kỹ thuật",
                "Luyện bật âm cuối -ed (/t/, /d/, /ɪd/) và trọng âm từ vựng phỏng vấn.",
                new EnPrItem[]
                {
                    new("en-l42-pron-e1", "en-US", "easy", "Đuôi -ed /d/ trong resolved (Dễ)", "We quickly resolved the critical database incident.", "/wiː ˈkwɪkli rɪˈzɒlvd ðə ˈkrɪtɪkl ˈdeɪtəbeɪs ˈɪnsɪdənt/", "Chúng tôi đã giải quyết nhanh sự cố cơ sở dữ liệu nghiêm trọng.", new[] { "resolved", "critical", "incident" }, new[] { "database incident" }),
                    new("en-l42-pron-e2", "en-US", "easy", "Đuôi -ed /t/ trong reduced (Dễ)", "Connection pooling reduced query wait times dramatically.", "/kəˈnekʃn ˈpuːlɪŋ rɪˈdjuːst ˈkwɪəri weɪt taɪmz drəˈmætɪkli/", "Connection pooling đã giảm mạnh thời gian chờ truy vấn.", new[] { "reduced", "query", "dramatically" }, new[] { "connection pooling" }),
                    new("en-l42-pron-m1", "en-US", "medium", "Đuôi -ed /ɪd/ trong implemented (Vừa)", "I implemented an idempotent retry worker for failed jobs.", "/aɪ ˈɪmplɪmentɪd ən ˌaɪdɛmˈpəʊtənt ˈriːtraɪ ˈwɜːkə fɔː feɪld dʒɒbz/", "Tôi đã cài đặt một worker thử lại đảm bảo idempotency cho các tác vụ lỗi.", new[] { "implemented", "idempotent", "failed" }, new[] { "retry worker" }),
                    new("en-l42-pron-m2", "en-US", "medium", "Trọng âm scalability (Vừa)", "Decoupling services greatly improved system scalability.", "/diːˈkʌplɪŋ ˈsɜːvɪsɪz ˈɡreɪtli ɪmˈpruːvd ˈsɪstəm ˌskeɪləˈbɪləti/", "Tách rời các dịch vụ đã cải thiện đáng kể khả năng mở rộng hệ thống.", new[] { "scalability", "decoupling", "improved" }, new[] { "system scalability" }),
                    new("en-l42-pron-h1", "en-US", "hard", "Trọng âm bottleneck & exhaustion (Khó)", "Pool exhaustion created a severe throughput bottleneck under peak traffic.", "/puːl ɪɡˈzɔːstʃən kriˈeɪtɪd ə sɪˈvɪə ˈθruːpʊt ˈbɒtlnek ˈʌndə piːk ˈtræfɪk/", "Cạn kiệt pool kết nối đã tạo ra nút thắt cổ chai thông lượng khi tải cao.", new[] { "exhaustion", "bottleneck", "throughput" }, new[] { "throughput bottleneck" }),
                    new("en-l42-pron-h2", "en-US", "hard", "Âm /sp/ trong spearheaded (Khó)", "I spearheaded the migration toward containerized microservices architecture.", "/aɪ ˈspɪəhedɪd ðə maɪˈɡreɪʃn təˈwɔːdz kənˈteɪnəraɪzd ˌmaɪkrəʊˈsɜːvɪsɪz ˈɑːkɪtektʃə/", "Tôi dẫn dắt quá trình chuyển đổi sang kiến trúc microservices đóng gói container.", new[] { "spearheaded", "migration", "architecture" }, new[] { "spearheaded the migration" })
                }, now),
            MakeEnActivity(c, l, 14, "word_map",
                "Word Map (Bài 4.2): Từ vựng Phỏng vấn Behavioral & Engineering",
                "Mở rộng vốn từ vựng STAR để mô tả tác động kỹ thuật ấn tượng.",
                new EnWmItem[]
                {
                    new("wm-l42-e1", "en-US", "easy", "Sơ đồ từ vựng: diagnose", "diagnose", "/ˈdaɪəɡnəʊz/", "Chẩn đoán, tìm nguyên nhân lỗi",
                        new[] { new EnWmNode("diagnose a bottleneck", "/ˈdaɪəɡnəʊz ə ˈbɒtlnek/", "Tìm điểm nghẽn hiệu năng", "APM tools helped us diagnose the bottleneck."), new EnWmNode("diagnose the root cause", "/ˈdaɪəɡnəʊz ðə ruːt kɔːz/", "Truy tìm nguyên nhân gốc", "First, diagnose the root cause before patching.") },
                        new[] { new EnWmNode("track down", "/træk daʊn/", "Lần ra dấu vết", "We tracked down the memory leak."), new EnWmNode("narrow down", "/ˈnærəʊ daʊn/", "Thu hẹp phạm vi nghi vấn", "Logs narrowed down the faulty module.") },
                        new[] { new EnWmNode("pinpoint", "/ˈpɪnpɔɪnt/", "Xác định chính xác điểm lỗi", "Profiling pinpointed the slow loop."), new EnWmNode("isolate", "/ˈaɪsəleɪt/", "Khoanh vùng cô lập", "Isolate the failing component first.") },
                        new[] { new EnWmEx("I diagnosed the connection leak within fifteen minutes using thread dumps.", "Tôi đã chẩn đoán ra lỗi rò rỉ kết nối trong 15 phút nhờ thread dumps.") }),
                    new("wm-l42-e2", "en-US", "easy", "Sơ đồ từ vựng: optimize", "optimize", "/ˈɒptɪmaɪz/", "Tối ưu hóa",
                        new[] { new EnWmNode("optimize query latency", "/ˈɒptɪmaɪz ˈkwɪəri ˈleɪtənsi/", "Tối ưu độ trễ truy vấn", "Composite indexes optimize query latency."), new EnWmNode("optimize resource usage", "/ˈɒptɪmaɪz rɪˈsɔːs ˈjuːsɪdʒ/", "Tối ưu sử dụng tài nguyên", "Streaming optimizes RAM usage.") },
                        new[] { new EnWmNode("speed up", "/spiːd ʌp/", "Tăng tốc độ", "Caching sped up responses tenfold."), new EnWmNode("cut down on", "/kʌt daʊn ɒn/", "Cắt giảm bớt", "Batching cuts down on network round trips.") },
                        new[] { new EnWmNode("streamline", "/ˈstriːmlaɪn/", "Tinh gọn quy trình", "CI scripts streamlined our releases."), new EnWmNode("tune", "/tjuːn/", "Tinh chỉnh tham số", "We tuned JVM garbage collection.") },
                        new[] { new EnWmEx("By optimizing the SQL join, I reduced report generation time by 70 percent.", "Nhờ tối ưu lệnh nối SQL, tôi đã giảm 70% thời gian xuất báo cáo.") }),
                    new("wm-l42-m1", "en-US", "medium", "Sơ đồ từ vựng: bottleneck", "bottleneck", "/ˈbɒtlnek/", "Điểm nghẽn cổ chai",
                        new[] { new EnWmNode("performance bottleneck", "/pəˈfɔːməns ˈbɒtlnek/", "Điểm nghẽn hiệu năng", "Disk I/O became our primary performance bottleneck."), new EnWmNode("eliminate a bottleneck", "/ɪˈlɪmɪneɪt ə ˈbɒtlnek/", "Loại bỏ điểm nghẽn", "Sharding eliminated the write bottleneck.") },
                        new[] { new EnWmNode("slow down", "/sləʊ daʊn/", "Làm chậm lại", "Lock contention slows down throughput."), new EnWmNode("back up", "/bæk ʌp/", "Bị ùn ứ hàng đợi", "Requests backed up behind the slow API.") },
                        new[] { new EnWmNode("chokepoint", "/ˈtʃəʊkpɔɪnt/", "Điểm thắt nghẽn", "The single gateway acted as a chokepoint."), new EnWmNode("constraint", "/kənˈstreɪnt/", "Giới hạn cản trở", "Memory was the key constraint.") },
                        new[] { new EnWmEx("Identifying the database lock bottleneck unlocked triple the throughput.", "Phát hiện ra điểm nghẽn khóa database đã giúp tăng gấp 3 lần thông lượng.") }),
                    new("wm-l42-m2", "en-US", "medium", "Sơ đồ từ vựng: trade-off", "trade-off", "/ˈtreɪd ɒf/", "Sự đánh đổi giữa hai yếu tố",
                        new[] { new EnWmNode("architectural trade-off", "/ˌɑːkɪˈtektʃərəl ˈtreɪd ɒf/", "Đánh đổi kiến trúc", "Discuss architectural trade-offs openly in interviews."), new EnWmNode("weigh trade-offs", "/weɪ ˈtreɪd ɒfs/", "Cân nhắc được và mất", "Engineers constantly weigh cost-versus-speed trade-offs.") },
                        new[] { new EnWmNode("give up", "/ɡɪv ʌp/", "Hy sinh bớt", "You give up strict consistency for availability."), new EnWmNode("opt for", "/ɒpt fɔː/", "Lựa chọn ưu tiên", "We opted for simplicity over premature scaling.") },
                        new[] { new EnWmNode("compromise", "/ˈkɒmprəmaɪz/", "Sự dung hòa", "Eventual consistency was an acceptable compromise."), new EnWmNode("balance", "/ˈbæləns/", "Sự cân bằng", "Strike a balance between speed and safety.") },
                        new[] { new EnWmEx("Walking through the trade-offs shows senior-level engineering judgment.", "Phân tích rõ các đánh đổi thể hiện tư duy kỹ thuật chín chắn.") }),
                    new("wm-l42-h1", "en-US", "hard", "Sơ đồ từ vựng: spearhead", "spearhead", "/ˈspɪəhed/", "Dẫn dắt tiên phong một sáng kiến",
                        new[] { new EnWmNode("spearhead a migration", "/ˈspɪəhed ə maɪˈɡreɪʃn/", "Dẫn dắt dự án chuyển đổi", "She spearheaded the cloud migration."), new EnWmNode("spearhead an initiative", "/ˈspɪəhed ən ɪˈnɪʃətɪv/", "Khởi xướng và dẫn dắt", "He spearheaded our automated testing initiative.") },
                        new[] { new EnWmNode("lead up", "/liːd ʌp/", "Dẫn đường cho", "Strong ownership led up to promotion."), new EnWmNode("roll out", "/rəʊl aʊt/", "Triển khai diện rộng", "We rolled out feature flags gradually.") },
                        new[] { new EnWmNode("pioneer", "/ˌpaɪəˈnɪər/", "Mở đường tiên phong", "The team pioneered zero-downtime deploys."), new EnWmNode("champion", "/ˈtʃæmpiən/", "Đứng ra thúc đẩy bảo vệ", "She championed observability standards.") },
                        new[] { new EnWmEx("I spearheaded the refactoring of our legacy billing module with zero downtime.", "Tôi đã dẫn dắt tái cấu trúc module thanh toán cũ mà không gây gián đoạn dịch vụ.") }),
                    new("wm-l42-h2", "en-US", "hard", "Sơ đồ từ vựng: mitigate", "mitigate", "/ˈmɪtɪɡeɪt/", "Giảm nhẹ tác động rủi ro",
                        new[] { new EnWmNode("mitigate the risk", "/ˈmɪtɪɡeɪt ðə rɪsk/", "Giảm thiểu rủi ro", "Canary releases mitigate deployment risk."), new EnWmNode("mitigate downtime", "/ˈmɪtɪɡeɪt ˈdaʊntaɪm/", "Hạn chế thời gian chết", "Circuit breakers mitigate cascading downtime.") },
                        new[] { new EnWmNode("ward off", "/wɔːd ɒf/", "Ngăn ngừa đẩy lùi", "Rate limiting wards off abuse spikes."), new EnWmNode("cushion against", "/ˈkʊʃn əˈɡenst/", "Làm đệm giảm sốc", "Retries cushion against transient glitches.") },
                        new[] { new EnWmNode("alleviate", "/əˈliːvieɪt/", "Làm dịu bớt", "Read replicas alleviated primary DB load."), new EnWmNode("curb", "/kɜːb/", "Kiềm chế", "Quotas curbed runaway costs.") },
                        new[] { new EnWmEx("To mitigate future incidents, we introduced strict timeouts on all external calls.", "Để giảm nhẹ rủi ro sự cố tương lai, chúng tôi đặt timeout chặt cho mọi cuộc gọi ngoài.") })
                }, now),
            MakeEnActivity(c, l, 15, "cloze",
                "Điền từ ngữ cảnh (Bài 4.2): Phỏng vấn STAR & Kỹ thuật",
                "Hoàn thành các câu trả lời phỏng vấn chuẩn STAR với cấu trúc đo lường kết quả.",
                new EnClItem[]
                {
                    new("en-l42-cloze-1", "easy", "Nêu nhiệm vụ cá nhân (Dễ)", "Điền cụm từ diễn đạt vai trò được giao:", "As the lead backend engineer, I was {{0}} with resolving the memory {{1}}.", new[] { new EnClBlank(new[] { "tasked", "charged" }, "được giao nhiệm vụ (was ___ with)", "'be tasked with + V-ing' = được giao trách nhiệm."), new EnClBlank(new[] { "leak", "issue", "spike" }, "lỗi rò rỉ bộ nhớ", "'memory leak' = rò rỉ bộ nhớ.") }, "As the lead backend engineer, I was tasked with resolving the memory leak.", "Với vai trò kỹ sư backend chính, tôi được giao xử lý lỗi rò rỉ bộ nhớ.", "Dùng 'I was tasked with + V-ing' mở đầu bước Task cực gọn."),
                    new("en-l42-cloze-2", "easy", "Nêu kết quả định lượng (Dễ)", "Điền giới từ chỉ khoảng giảm số liệu:", "As a result, API latency dropped {{0}} 1.5 seconds {{1}} 80 milliseconds.", new[] { new EnClBlank(new[] { "from" }, "mốc ban đầu", "Cấu trúc 'dropped from X to Y'."), new EnClBlank(new[] { "to" }, "mốc sau cải thiện", "Giảm TỪ X XUỐNG Y dùng 'from ... to'.") }, "As a result, API latency dropped from 1.5 seconds to 80 milliseconds.", "Kết quả là độ trễ API giảm từ 1,5 giây xuống 80 mili-giây.", "Luôn dùng số liệu thật ở bước Result trong STAR."),
                    new("en-l42-cloze-3", "medium", "Giải thích hành động xử lý (Vừa)", "Điền động từ hành động STAR:", "First, I {{0}} the slow SQL query and {{1}} a composite index to fix it.", new[] { new EnClBlank(new[] { "diagnosed", "identified", "isolated" }, "chẩn đoán / xác định lỗi", "Chia quá khứ đơn động từ hành động."), new EnClBlank(new[] { "added", "created", "implemented" }, "thêm vào / triển khai", "Động từ quá khứ đơn song song với diagnosed.") }, "First, I diagnosed the slow SQL query and added a composite index to fix it.", "Đầu tiên, tôi chẩn đoán truy vấn SQL chậm và bổ sung composite index để sửa.", "Giữ thống nhất thì Quá khứ đơn khi kể dự án đã qua."),
                    new("en-l42-cloze-4", "medium", "Xin làm rõ trước khi thiết kế (Vừa)", "Điền từ hỏi làm rõ yêu cầu:", "Before diving into the architecture, could I {{0}} one constraint regarding expected {{1}}?", new[] { new EnClBlank(new[] { "clarify", "confirm" }, "làm rõ / xác nhận", "'clarify a constraint' = làm rõ ràng buộc đề bài."), new EnClBlank(new[] { "traffic", "load", "scale" }, "lưu lượng truy cập", "'expected traffic' = lưu lượng dự kiến.") }, "Before diving into the architecture, could I clarify one constraint regarding expected traffic?", "Trước khi đi sâu vào kiến trúc, tôi có thể làm rõ một ràng buộc về lưu lượng dự kiến không?", "Hỏi lại yêu cầu giúp bạn tránh thiết kế sai hướng."),
                    new("en-l42-cloze-5", "hard", "Trình bày Trade-off (Khó)", "Điền cặp cấu trúc so sánh hai mặt:", "On one {{0}}, caching boosts read speed; on the {{1}} hand, it adds invalidation complexity.", new[] { new EnClBlank(new[] { "hand" }, "một mặt (On one ___)", "Thành ngữ 'On one hand ... on the other hand'."), new EnClBlank(new[] { "other" }, "mặt khác (on the ___ hand)", "Đối lập hai mặt lợi/hại trong thiết kế hệ thống.") }, "On one hand, caching boosts read speed; on the other hand, it adds invalidation complexity.", "Một mặt, caching tăng tốc độ đọc; mặt khác, nó làm tăng độ phức tạp khi xóa cache.", "Mẫu câu vàng để trả lời System Design.")
                }, now),
            MakeEnActivity(c, l, 16, "translate",
                "Dịch phản xạ Việt → Anh (Bài 4.2): Trả lời Phỏng vấn STAR",
                "Luyện dịch các câu trả lời phỏng vấn sắc bén sang tiếng Anh chuẩn.",
                new EnTrItem[]
                {
                    new("en-l42-trans-1", "easy", "Nêu trách nhiệm cá nhân (Dễ)", "Với vai trò kỹ sư trực sự cố, tôi chịu trách nhiệm tìm nguyên nhân gốc rễ và khôi phục hệ thống.", new[] { "on-call engineer", "responsible for", "root cause", "restore" }, new[] { "As the on-call engineer, I was responsible for finding the root cause and restoring the system.", "As the on-call engineer, my responsibility was to identify the root cause and restore service." }, "As the on-call engineer, I was responsible for finding the root cause and restoring the system.", "Sau giới từ 'for' dùng danh động từ V-ing (finding, restoring)."),
                    new("en-l42-trans-2", "easy", "Nêu kết quả định lượng (Dễ)", "Nhờ vậy, chi phí máy chủ đã giảm 30% trong quý vừa qua.", new[] { "as a result", "server costs", "dropped by 30 percent" }, new[] { "As a result, server costs dropped by 30 percent last quarter.", "Consequently, our server costs decreased by 30 percent last quarter." }, "As a result, server costs dropped by 30 percent last quarter.", "Dùng 'dropped by 30 percent' (không có s sau percent)."),
                    new("en-l42-trans-3", "medium", "Xử lý khi chưa dùng công nghệ (Vừa)", "Tôi chưa từng triển khai Kafka trên môi trường sản xuất, nhưng tôi có nhiều kinh nghiệm với RabbitMQ.", new[] { "haven't used", "in production yet", "extensive experience" }, new[] { "I haven't deployed Kafka in production yet, though I have extensive experience with RabbitMQ.", "I have not used Kafka in production yet, but I have strong experience with RabbitMQ." }, "I haven't deployed Kafka in production yet, though I have extensive experience with RabbitMQ.", "Dùng Hiện tại hoàn thành 'haven't deployed ... yet' cho kinh nghiệm tới hiện tại."),
                    new("en-l42-trans-4", "medium", "Nêu bài học rút ra (Vừa)", "Sau sự cố đó, tôi đã bổ sung kiểm thử tự động để ngăn lỗi tương tự tái diễn.", new[] { "after that incident", "automated tests", "prevent regressions" }, new[] { "After that incident, I added automated tests to prevent similar regressions.", "Following that incident, I implemented automated tests to prevent similar bugs from recurring." }, "After that incident, I added automated tests to prevent similar regressions.", "Dùng động từ quá khứ đơn 'added' / 'implemented'."),
                    new("en-l42-trans-5", "hard", "Đặt câu hỏi ngược cho Tech Lead (Khó)", "Đội ngũ của anh chị cân bằng như thế nào giữa việc ra mắt tính năng mới nhanh chóng và xử lý nợ kỹ thuật?", new[] { "balance", "shipping new features", "paying down technical debt" }, new[] { "How does your team balance shipping new features quickly against paying down technical debt?", "How do you balance delivering features fast with addressing technical debt?" }, "How does your team balance shipping new features quickly against paying down technical debt?", "Cấu trúc 'balance [V-ing] against/with [V-ing]' rất chuyên nghiệp.")
                }, now),
            MakeEnActivity(c, l, 17, "grammar_fix",
                "Sửa lỗi ngữ pháp (Bài 4.2): Thì quá khứ & Giới từ số liệu phỏng vấn",
                "Sửa các lỗi thì động từ và giới từ làm giảm tính chuyên nghiệp trong phỏng vấn.",
                new EnGfItem[]
                {
                    new("en-l42-gf-1", "easy", "Quên chia quá khứ đơn ở bước Action (Dễ)", "Last year, I optimize our database query and save 20 hours per week.", "Sự việc xảy ra 'Last year' bắt buộc chia thì Quá khứ đơn (optimized, saved).", "Last year, I optimized our database query and saved 20 hours per week.", new[] { "Last year, I optimized our database query and saved 20 hours per week." }, "Năm ngoái, tôi đã tối ưu truy vấn CSDL và tiết kiệm 20 giờ mỗi tuần.", "Cả hai động từ nối bằng 'and' đều phải chia quá khứ đơn."),
                    new("en-l42-gf-2", "easy", "Dùng V-ing sau 'tasked with' (Dễ)", "I was tasked with build a real-time notification service.", "Sau giới từ 'with' phải dùng danh động từ V-ing.", "I was tasked with building a real-time notification service.", new[] { "I was tasked with building a real-time notification service." }, "Tôi được giao nhiệm vụ xây dựng dịch vụ thông báo thời gian thực.", "Cấu trúc chuẩn: 'be tasked with + V-ing'."),
                    new("en-l42-gf-3", "medium", "Nhầm giới từ 'by' và 'to' (Vừa)", "Average response time dropped from 2 seconds by 200 milliseconds.", "Cấu trúc chuyển từ mốc cũ sang mốc mới là 'from X to Y', không dùng 'by'.", "Average response time dropped from 2 seconds to 200 milliseconds.", new[] { "Average response time dropped from 2 seconds to 200 milliseconds." }, "Thời gian phản hồi trung bình giảm từ 2 giây xuống 200 mili-giây.", "Cặp cố định luôn là 'from ... to ...'."),
                    new("en-l42-gf-4", "medium", "Lặp giới từ sai sau 'balance' (Vừa)", "We balance between speed and code quality carefully.", "'balance' là ngoại động từ đi thẳng với tân ngữ 'speed and code quality', không dùng 'balance between'.", "We balance speed and code quality carefully.", new[] { "We balance speed and code quality carefully.", "We strike a balance between speed and code quality carefully." }, "Chúng tôi cân bằng cẩn thận giữa tốc độ và chất lượng code.", "Dùng 'balance A and B' hoặc 'strike a balance between A and B'."),
                    new("en-l42-gf-5", "hard", "Thì Hiện tại hoàn thành với 'yet' (Khó)", "I didn't use Kubernetes in production yet, but I study it daily.", "Có 'yet' ở câu phủ định chỉ trải nghiệm tính tới nay dùng Hiện tại hoàn thành 'haven't used'.", "I haven't used Kubernetes in production yet, though I study it daily.", new[] { "I haven't used Kubernetes in production yet, though I study it daily.", "I haven't used Kubernetes in production yet, but I study it daily." }, "Tôi chưa từng dùng Kubernetes trên sản xuất, nhưng tôi tự học nó hàng ngày.", "'haven't + V3 + yet' diễn tả việc chưa từng làm cho tới hiện tại.")
                }, now),
            MakeEnActivity(c, l, 18, "roleplay",
                "Nhập vai hội thoại (Bài 4.2): Phỏng vấn Kỹ thuật 1-1",
                "Trả lời phỏng vấn theo phương pháp STAR và đặt câu hỏi ngược cho Hiring Manager.",
                new EnRpItem[]
                {
                    new("en-l42-rp-1", "easy", "Giới thiệu điểm mạnh kỹ thuật (Dễ)", "Bạn đang mở đầu buổi phỏng vấn cho vị trí Software Engineer.", "Sarah (Hiring Manager)", "Thanks for joining us today! Could you briefly summarize your engineering background and recent impact?", "Cảm ơn bạn đã tham gia hôm nay! Bạn có thể tóm tắt ngắn gọn kinh nghiệm kỹ thuật và đóng góp gần đây không?", "Giới thiệu vị trí hiện tại, mảng công nghệ chính và 1 thành tựu có con số cụ thể.", new[] { "software engineer", "focused on", "reduced", "percent" }, new[] { "I am a software engineer focused on backend systems. Recently, I optimized our search API and reduced average latency by 45 percent." }, 12),
                    new("en-l42-rp-2", "medium", "Trả lời câu hỏi STAR về sự cố Production (Vừa)", "Nhà tuyển dụng hỏi về một sự cố nghiêm trọng bạn từng xử lý.", "Sarah (Hiring Manager)", "Tell me about a challenging production bug you solved under pressure. What did you personally do?", "Hãy kể về một lỗi nghiêm trọng trên hệ thống thực tế bạn từng giải quyết dưới áp lực. Cá nhân bạn đã làm gì?", "Kể đủ 4 phần Situation - Task - Action (dùng 'I') - Result (có con số ms/%).", new[] { "last quarter", "root cause", "diagnosed", "index", "latency", "dropped" }, new[] { "Last quarter our checkout API had severe timeouts. I was responsible for diagnosing the root cause, so I traced the logs, added a composite database index, and reduced p99 latency from 3 seconds to 50 milliseconds." }, 18),
                    new("en-l42-rp-3", "medium", "Xử lý khi chưa biết một công nghệ (Vừa)", "Nhà tuyển dụng hỏi về công nghệ bạn chưa từng dùng trên Production.", "Sarah (Hiring Manager)", "Our team relies heavily on Apache Kafka. Have you deployed Kafka in production before?", "Đội của chúng tôi dùng rất nhiều Apache Kafka. Bạn đã từng triển khai Kafka trên sản xuất chưa?", "Thành thật chưa dùng Kafka nhưng nêu kinh nghiệm tương đương (RabbitMQ/queues) và khả năng học nhanh.", new[] { "haven't", "production", "experience", "rabbitmq", "patterns" }, new[] { "I haven't deployed Kafka in production yet, though I have extensive experience with RabbitMQ and event-driven patterns, so I can ramp up quickly." }, 14),
                    new("en-l42-rp-4", "hard", "Đặt câu hỏi ngược ở cuối buổi phỏng vấn (Khó)", "Cuối buổi phỏng vấn, nhà tuyển dụng mời bạn đặt câu hỏi.", "Sarah (Hiring Manager)", "We have a few minutes left. What questions do you have for me about the team or role?", "Chúng ta còn vài phút. Bạn có câu hỏi nào dành cho tôi về đội ngũ hoặc vai trò này không?", "Đặt câu hỏi thông minh về cách cân bằng nợ kỹ thuật hoặc thước đo thành công 90 ngày đầu.", new[] { "balance", "technical debt", "success", "first 90 days" }, new[] { "Thank you! How does the team balance shipping new features with paying down technical debt, and what does success look like in the first 90 days?" }, 14)
                }, now)
        };
    }

    private static List<PracticeActivity> BuildEn43Activities(Guid c, DateTimeOffset now)
    {
        var l = EnLesson43Id;
        return new()
        {
            MakeEnActivity(c, l, 11, "shadowing",
                "Shadowing (Bài 4.3): Code Review, Bug Report & Kiến trúc",
                "Luyện nói các mẫu góp ý PR xây dựng và thảo luận kỹ thuật chuyên nghiệp.",
                new EnShItem[]
                {
                    new("en-l43-shad-e1", "en-US", "easy", "Khen ngợi mở đầu PR (Dễ)", "Overall the logic looks super clean, and I left one minor comment.", "/ˌəʊvərˈɔːl ðə ˈlɒdʒɪk lʊks ˈsuːpə kliːn ænd aɪ left wʌn ˈmaɪnə ˈkɒment/", "Nhìn chung logic rất gọn gàng, và tôi có để lại một góp ý nhỏ.", new[] { "looks super clean", "minor comment" }),
                    new("en-l43-shad-e2", "en-US", "easy", "Tiếp nhận góp ý tích cực (Dễ)", "Good catch! That makes total sense, and I will push a fix shortly.", "/ɡʊd kætʃ ðæt meɪks ˈtəʊtl sens ænd aɪ wɪl pʊʃ ə fɪks ˈʃɔːtli/", "Phát hiện rất chuẩn! Điều đó hoàn toàn hợp lý và tôi sẽ đẩy bản sửa ngay.", new[] { "good catch", "makes total sense" }),
                    new("en-l43-shad-m1", "en-US", "medium", "Đề xuất tối ưu cấu trúc dữ liệu (Vừa)", "Suggestion: using a Set here would reduce lookup complexity to constant time.", "/səˈdʒestʃən ˈjuːzɪŋ ə set hɪə wʊd rɪˈdjuːs ˈlʊkʌp kəmˈpleksəti tuː ˈkɒnstənt taɪm/", "Đề xuất: dùng Set ở đây sẽ giảm độ phức tạp tra cứu xuống thời gian hằng số O(1).", new[] { "reduce lookup complexity", "constant time" }),
                    new("en-l43-shad-m2", "en-US", "medium", "Hỏi làm rõ thiết kế lịch sự (Vừa)", "Out of curiosity, is there a reason we are bypassing the cache here?", "/aʊt əv ˌkjʊəriˈɒsəti ɪz ðeər ə ˈriːzn wiː ɑː ˈbaɪpɑːsɪŋ ðə kæʃ hɪə/", "Tò mò chút, có lý do đặc biệt nào chúng ta bỏ qua cache ở đoạn này không?", new[] { "out of curiosity", "bypassing the cache" }),
                    new("en-l43-shad-h1", "en-US", "hard", "Đề xuất Optimistic Locking (Khó)", "What if we wrap this update in an atomic version check to prevent silent overwrites?", "/wɒt ɪf wiː ræp ðɪs ʌpˈdeɪt ɪn ən əˈtɒmɪk ˈvɜːʃn tʃek tuː prɪˈvent ˈsaɪlənt ˌəʊvəˈraɪts/", "Thế còn việc bọc thao tác cập nhật này trong kiểm tra version nguyên tử để tránh ghi đè ngầm thì sao?", new[] { "atomic version check", "silent overwrites" }),
                    new("en-l43-shad-h2", "en-US", "hard", "Đảm bảo tương thích ngược (Khó)", "Let us verify that this schema change remains backward-compatible with older mobile clients.", "/let ʌs ˈverɪfaɪ ðæt ðɪs ˈskiːmə tʃeɪndʒ rɪˈmeɪnz ˈbækwəd kəmˈpætəbl wɪð ˈəʊldə ˈməʊbaɪl ˈklaɪənts/", "Hãy kiểm chứng rằng thay đổi schema này vẫn tương thích ngược với các bản ứng dụng di động cũ.", new[] { "schema change", "backward-compatible" })
                }, now),
            MakeEnActivity(c, l, 12, "dictation",
                "Dictation (Bài 4.3): Nghe chép Code Review & Bug Report",
                "Nghe và chép lại các cụm giao tiếp PR review và báo cáo lỗi.",
                new EnDiItem[]
                {
                    new("en-l43-dict-e1", "en-US", "easy", "Góp ý nhỏ Nitpick (Dễ)", "Nit: could we rename this variable for better readability?", "/nɪt kəd wiː ˌriːˈneɪm ðɪs ˈveəriəbl fɔː ˈbetə ˌriːdəˈbɪləti/", "Chi tiết nhỏ: chúng ta đổi tên biến này cho dễ đọc hơn được không?", new[] { "Nit", "could", "we", "rename", "this", "variable", "for", "better", "readability" }, new[] { "change", "function", "clarity" }),
                    new("en-l43-dict-e2", "en-US", "easy", "Nhờ xem lại PR (Dễ)", "I have pushed the fix; ready for another look.", "/aɪ hæv pʊʃt ðə fɪks ˈredi fɔːr əˈnʌðə lʊk/", "Tôi đã đẩy bản sửa rồi; sẵn sàng để bạn xem lại nhé.", new[] { "I", "have", "pushed", "the", "fix", "ready", "for", "another", "look" }, new[] { "sent", "patch", "check" }),
                    new("en-l43-dict-m1", "en-US", "medium", "Cảnh báo ngoại lệ Timeout (Vừa)", "If the external API times out, this might throw an unhandled exception.", "/ɪf ði ɪkˈstɜːnl eɪ piː aɪ taɪmz aʊt ðɪs maɪt θrəʊ ən ʌnˈhændld ɪkˈsepʃn/", "Nếu API ngoài bị quá hạn, đoạn này có thể ném ra ngoại lệ chưa xử lý.", new[] { "If", "the", "external", "API", "times", "out", "this", "might", "throw", "an", "unhandled", "exception" }, new[] { "internal", "will", "error" }),
                    new("en-l43-dict-m2", "en-US", "medium", "Thêm test trường hợp biên (Vừa)", "Please add a unit test covering the empty-array edge case.", "/pliːz æd ə ˈjuːnɪt test ˈkʌvərɪŋ ði ˈempti əˈreɪ edʒ keɪs/", "Vui lòng bổ sung một unit test bao phủ trường hợp biên mảng rỗng.", new[] { "Please", "add", "a", "unit", "test", "covering", "the", "empty-array", "edge", "case" }, new[] { "write", "integration", "corner" }),
                    new("en-l43-dict-h1", "en-US", "hard", "Tránh điểm lỗi đơn (Khó)", "Running a single Redis instance without replicas creates a single point of failure.", "/ˈrʌnɪŋ ə ˈsɪŋɡl ˈredɪs ˈɪnstəns wɪðˈaʊt ˈreplɪkəz kriˈeɪts ə ˈsɪŋɡl pɔɪnt əv ˈfeɪljə/", "Chạy một thực thể Redis đơn lẻ không có bản sao dự phòng sẽ tạo ra điểm lỗi đơn.", new[] { "Running", "a", "single", "Redis", "instance", "without", "replicas", "creates", "a", "single", "point", "of", "failure" }, new[] { "node", "backups", "causes" }),
                    new("en-l43-dict-h2", "en-US", "hard", "Tách phụ thuộc bằng hàng đợi (Khó)", "Message queues decouple webhook ingestion from heavy background AI processing.", "/ˈmesɪdʒ kjuːz diːˈkʌpl ˈwebhʊk ɪnˈdʒestʃən frəm ˈhevi ˈbækɡraʊnd eɪ aɪ ˈprəʊsesɪŋ/", "Hàng đợi thông điệp tách rời việc tiếp nhận webhook khỏi khâu xử lý AI nặng ở nền.", new[] { "Message", "queues", "decouple", "webhook", "ingestion", "from", "heavy", "background", "AI", "processing" }, new[] { "separate", "receiving", "tasks" })
                }, now),
            MakeEnActivity(c, l, 13, "pronunciation",
                "Phát âm IPA (Bài 4.3): Thuật ngữ Code Review & Kiến trúc",
                "Luyện phát âm chuẩn các từ khó như complexity, concurrency, architecture, regression.",
                new EnPrItem[]
                {
                    new("en-l43-pron-e1", "en-US", "easy", "Âm /k/ và /tʃ/ trong catch (Dễ)", "Good catch, let me extract this helper function.", "/ɡʊd kætʃ let miː ɪkˈstrækt ðɪs ˈhelpə ˈfʌŋkʃn/", "Phát hiện rất hay, để tôi tách hàm trợ giúp này ra.", new[] { "catch", "extract", "helper" }, new[] { "good catch" }),
                    new("en-l43-pron-e2", "en-US", "easy", "Âm cuối /kts/ trong conflicts (Dễ)", "This branch has two merge conflicts with main.", "/ðɪs brɑːntʃ hæz tuː mɜːdʒ ˈkɒnflɪkts wɪð meɪn/", "Nhánh này đang có hai xung đột trộn mã với nhánh main.", new[] { "conflicts", "branch", "merge" }, new[] { "merge conflicts" }),
                    new("en-l43-pron-m1", "en-US", "medium", "Trọng âm concurrency (Vừa)", "We added a concurrency test for simultaneous edits.", "/wiː ˈædɪd ə kənˈkʌrənsi test fɔː ˌsɪmlˈteɪniəs ˈedɪts/", "Chúng tôi đã thêm bài kiểm thử đồng thời cho các thao tác sửa cùng lúc.", new[] { "concurrency", "simultaneous", "edits" }, new[] { "concurrency test" }),
                    new("en-l43-pron-m2", "en-US", "medium", "Âm /ʃ/ trong regression (Vừa)", "Automated suites prevent regressions before every release.", "/ˈɔːtəmeɪtɪd swiːts prɪˈvent rɪˈɡreʃnz bɪˈfɔːr ˈevri rɪˈliːs/", "Các bộ kiểm thử tự động ngăn lỗi tái phát trước mỗi đợt phát hành.", new[] { "regressions", "automated", "prevent" }, new[] { "prevent regressions" }),
                    new("en-l43-pron-h1", "en-US", "hard", "Trọng âm backward-compatible (Khó)", "Keep the REST payload backward-compatible with legacy clients.", "/kiːp ðə rest ˈpeɪləʊd ˈbækwəd kəmˈpætəbl wɪð ˈleɡəsi ˈklaɪənts/", "Giữ cấu trúc dữ liệu REST tương thích ngược với các client cũ.", new[] { "backward-compatible", "payload", "legacy" }, new[] { "backward-compatible" }),
                    new("en-l43-pron-h2", "en-US", "hard", "Âm /əʊ/ trong overhead (Khó)", "Synchronous RPC chains introduce unacceptable latency overhead.", "/ˈsɪŋkrənəs ɑː piː siː tʃeɪnz ˌɪntrəˈdjuːs ˌʌnəkˈseptəbl ˈleɪtənsi ˈəʊvəhed/", "Chuỗi gọi RPC đồng bộ tạo ra chi phí độ trễ phụ trội không thể chấp nhận.", new[] { "overhead", "synchronous", "unacceptable" }, new[] { "latency overhead" })
                }, now),
            MakeEnActivity(c, l, 14, "word_map",
                "Word Map (Bài 4.3): Từ vựng Code Review & System Design",
                "Ghi nhớ hệ thống từ vựng review PR, báo cáo lỗi và kiến trúc hệ thống.",
                new EnWmItem[]
                {
                    new("wm-l43-e1", "en-US", "easy", "Sơ đồ từ vựng: reproduce", "reproduce", "/ˌriːprəˈdjuːs/", "Tái hiện lại lỗi",
                        new[] { new EnWmNode("steps to reproduce", "/steps tuː ˌriːprəˈdjuːs/", "Các bước tái hiện lỗi", "Always include clear steps to reproduce in bug tickets."), new EnWmNode("consistently reproduce", "/kənˈsɪstəntli ˌriːprəˈdjuːs/", "Tái hiện lặp lại ổn định", "QA can consistently reproduce the crash on Safari.") },
                        new[] { new EnWmNode("track down", "/træk daʊn/", "Lần ra nguồn lỗi", "Reproduction helps track down bugs faster."), new EnWmNode("rule out", "/ruːl aʊt/", "Loại trừ nguyên nhân", "Let's rule out caching issues first.") },
                        new[] { new EnWmNode("replicate", "/ˈreplɪkeɪt/", "Tái tạo lại", "Can you replicate this locally?"), new EnWmNode("duplicate", "/ˈdjuːplɪkeɪt/", "Làm lại đúng hiện tượng", "We duplicated the timeout in staging.") },
                        new[] { new EnWmEx("Without clear steps to reproduce, engineers waste hours guessing.", "Nếu thiếu các bước tái hiện rõ ràng, kỹ sư mất hàng giờ đoán lỗi.") }),
                    new("wm-l43-e2", "en-US", "easy", "Sơ đồ từ vựng: extract", "extract", "/ɪkˈstrækt/", "Tách chiết (hàm/module)",
                        new[] { new EnWmNode("extract a helper", "/ɪkˈstrækt ə ˈhelpə/", "Tách thành hàm trợ giúp", "Let's extract a shared helper for validation."), new EnWmNode("extract common logic", "/ɪkˈstrækt ˈkɒmən ˈlɒdʒɪk/", "Tách logic dùng chung", "Extract common logic to avoid duplication.") },
                        new[] { new EnWmNode("break down", "/breɪk daʊn/", "Chia nhỏ ra", "Break down the 200-line handler."), new EnWmNode("clean up", "/kliːn ʌp/", "Dọn dẹp làm gọn", "Extracting helpers cleaned up the controller.") },
                        new[] { new EnWmNode("factor out", "/ˈfæktər aʊt/", "Tách nhân tử chung", "Factor out the retry loop."), new EnWmNode("isolate", "/ˈaɪsəleɪt/", "Cô lập riêng", "Isolate side effects into one service.") },
                        new[] { new EnWmEx("Could we extract this duplicated parsing code into a reusable utility?", "Chúng ta có thể tách đoạn code phân tích trùng lặp này thành hàm dùng chung không?") }),
                    new("wm-l43-m1", "en-US", "medium", "Sơ đồ từ vựng: edge case", "edge case", "/edʒ keɪs/", "Trường hợp biên hiếm gặp",
                        new[] { new EnWmNode("handle an edge case", "/ˈhændl ən edʒ keɪs/", "Xử lý ca biên", "Make sure we handle the null-payload edge case."), new EnWmNode("cover edge cases", "/ˈkʌvər edʒ ˈkeɪsɪz/", "Bao phủ hết ca biên", "Unit tests should cover boundary edge cases.") },
                        new[] { new EnWmNode("fall through", "/fɔːl θruː/", "Bị lọt qua nhánh sai", "Odd inputs fell through to the default branch."), new EnWmNode("guard against", "/ɡɑːd əˈɡenst/", "Rào chắn phòng ngừa", "Early returns guard against invalid states.") },
                        new[] { new EnWmNode("corner case", "/ˈkɔːnə keɪs/", "Trường hợp góc khuất", "Concurrency exposes subtle corner cases."), new EnWmNode("boundary condition", "/ˈbaʊndri kənˈdɪʃn/", "Điều kiện biên", "Check boundary conditions like zero or max int.") },
                        new[] { new EnWmEx("This PR looks solid, except for one subtle edge case when the list is empty.", "PR này rất ổn, ngoại trừ một ca biên nhỏ khi danh sách bị rỗng.") }),
                    new("wm-l43-m2", "en-US", "medium", "Sơ đồ từ vựng: decouple", "decouple", "/diːˈkʌpl/", "Tách rời phụ thuộc giữa các phần",
                        new[] { new EnWmNode("loosely coupled", "/ˈluːsli ˈkʌpld/", "Liên kết lỏng lẻo (dễ bảo trì)", "Event buses keep modules loosely coupled."), new EnWmNode("decouple from", "/diːˈkʌpl frəm/", "Tách rời khỏi", "Decouple billing logic from the auth controller.") },
                        new[] { new EnWmNode("split off", "/splɪt ɒf/", "Tách nhánh riêng", "We split off email sending to a worker."), new EnWmNode("hook into", "/hʊk ˈɪntuː/", "Gắn nối vào qua interface", "Plugins hook into lifecycle events.") },
                        new[] { new EnWmNode("separate", "/ˈsepəreɪt/", "Phân tách riêng", "Separate concerns cleanly."), new EnWmNode("isolate", "/ˈaɪsəleɪt/", "Cô lập độc lập", "Isolate third-party SDK wrappers.") },
                        new[] { new EnWmEx("Decoupling PDF generation from the API request prevented worker timeouts.", "Tách việc tạo PDF khỏi request API đã ngăn tình trạng timeout.") }),
                    new("wm-l43-h1", "en-US", "hard", "Sơ đồ từ vựng: regression", "regression", "/rɪˈɡreʃn/", "Lỗi tái phát trên tính năng cũ",
                        new[] { new EnWmNode("prevent regressions", "/prɪˈvent rɪˈɡreʃnz/", "Ngăn ngừa lỗi tái phát", "Regression tests run on every PR."), new EnWmNode("introduce a regression", "/ˌɪntrəˈdjuːs ə rɪˈɡreʃn/", "Vô tình gây hỏng tính năng cũ", "That hotfix introduced a login regression.") },
                        new[] { new EnWmNode("break backward compatibility", "/breɪk ˈbækwəd kəmˌpætəˈbɪləti/", "Làm gãy tương thích ngược", "Renaming fields broke mobile clients."), new EnWmNode("roll back", "/rəʊl bæk/", "Hoàn tác bản phát hành", "We rolled back after noticing a regression.") },
                        new[] { new EnWmNode("reoccurrence", "/ˌriːəˈkʌrəns/", "Sự tái xuất hiện lỗi", "Alerting caught the bug reoccurrence early."), new EnWmNode("setback", "/ˈsetbæk/", "Bước lùi kỹ thuật", "The outage was a minor setback.") },
                        new[] { new EnWmEx("Adding an integration test ensures this race condition never causes a future regression.", "Thêm bài kiểm thử tích hợp đảm bảo lỗi tranh chấp này không bao giờ tái phát.") }),
                    new("wm-l43-h2", "en-US", "hard", "Sơ đồ từ vựng: overhead", "overhead", "/ˈəʊvəhed/", "Chi phí phụ trội hệ thống",
                        new[] { new EnWmNode("serialization overhead", "/ˌsɪəriəlaɪˈzeɪʃn ˈəʊvəhed/", "Chi phí chuyển đổi JSON/binary", "Protobuf reduces serialization overhead."), new EnWmNode("operational overhead", "/ˌɒpəˈreɪʃənl ˈəʊvəhed/", "Gánh nặng vận hành", "Microservices add operational overhead for small teams.") },
                        new[] { new EnWmNode("eat up", "/iːt ʌp/", "Ngốn cạn tài nguyên", "Excessive logging ate up disk IOPS."), new EnWmNode("cut down", "/kʌt daʊn/", "Cắt giảm bớt", "Connection reuse cuts down TLS handshake overhead.") },
                        new[] { new EnWmNode("penalty", "/ˈpenlti/", "Chi phí hao hụt", "Context switching carries a cache penalty."), new EnWmNode("burden", "/ˈbɜːdn/", "Gánh nặng", "Avoid unnecessary maintenance burden.") },
                        new[] { new EnWmEx("Before splitting into ten services, consider the added operational overhead.", "Trước khi tách thành 10 dịch vụ, hãy cân nhắc gánh nặng vận hành tăng thêm.") })
                }, now),
            MakeEnActivity(c, l, 15, "cloze",
                "Điền từ ngữ cảnh (Bài 4.3): Code Review & Bug Report",
                "Hoàn thiện bình luận PR và báo cáo lỗi chuẩn kỹ sư phần mềm.",
                new EnClItem[]
                {
                    new("en-l43-cloze-1", "easy", "Nhận xét tích cực trong PR (Dễ)", "Điền cụm tiếp nhận góp ý của đồng nghiệp:", "Good {{0}}! That makes total {{1}}—I will fix it now.", new[] { new EnClBlank(new[] { "catch", "point" }, "phát hiện hay (Good ___!)", "'Good catch!' = Cảm ơn bạn đã phát hiện ra lỗi tinh tế này!"), new EnClBlank(new[] { "sense" }, "hoàn toàn hợp lý (makes total ___)", "'make sense' = hợp lý.") }, "Good catch! That makes total sense—I will fix it now.", "Phát hiện rất hay! Hoàn toàn hợp lý—tôi sẽ sửa ngay.", "Cách phản hồi lịch sự và cầu thị nhất trong Code Review."),
                    new("en-l43-cloze-2", "easy", "Báo cáo lỗi: Bước tái hiện (Dễ)", "Điền cụm tiêu đề Bug Report:", "Please list exact {{0}} to reproduce and the {{1}} vs. actual behavior.", new[] { new EnClBlank(new[] { "steps" }, "các bước (___ to reproduce)", "'steps to reproduce' = các bước tái hiện lỗi."), new EnClBlank(new[] { "expected" }, "hành vi kỳ vọng (___ vs. actual)", "'expected vs. actual behavior' = kết quả mong đợi vs thực tế.") }, "Please list exact steps to reproduce and the expected vs. actual behavior.", "Vui lòng liệt kê chính xác các bước tái hiện cùng kết quả mong đợi và thực tế.", "Cấu trúc chuẩn của mọi ticket Jira/GitHub Issue."),
                    new("en-l43-cloze-3", "medium", "Góp ý không gay gắt (Vừa)", "Điền từ đề xuất thay vì ra lệnh:", "Nit: could we {{0}} this retry logic into a shared helper for {{1}}?", new[] { new EnClBlank(new[] { "extract", "move" }, "tách ra thành (___ into)", "'extract X into Y' = tách hàm."), new EnClBlank(new[] { "readability", "clarity", "reuse" }, "dễ đọc / tái sử dụng", "'for readability' = để code dễ đọc hơn.") }, "Nit: could we extract this retry logic into a shared helper for readability?", "Chi tiết nhỏ: chúng ta tách logic thử lại này thành hàm dùng chung cho dễ đọc được không?", "Dùng 'Could we...' biến mệnh lệnh thành lời mời hợp tác."),
                    new("en-l43-cloze-4", "medium", "Cảnh báo lỗi trường hợp biên (Vừa)", "Điền thuật ngữ ca biên và kiểm thử:", "What happens if the array is empty? Let's add a test for this {{0}} {{1}}.", new[] { new EnClBlank(new[] { "edge", "corner" }, "biên (___ case)", "'edge case' = trường hợp biên."), new EnClBlank(new[] { "case", "scenario" }, "trường hợp", "'edge case' là thuật ngữ chuẩn.") }, "What happens if the array is empty? Let's add a test for this edge case.", "Điều gì xảy ra nếu mảng rỗng? Hãy thêm bài test cho ca biên này.", "Câu hỏi gợi mở giúp tác giả tự phát hiện thiếu sót."),
                    new("en-l43-cloze-5", "hard", "Thảo luận kiến trúc (Khó)", "Điền thuật ngữ tách rời và điểm lỗi đơn:", "Adding a message queue helps {{0}} background jobs and removes a single point of {{1}}.", new[] { new EnClBlank(new[] { "decouple", "separate", "isolate" }, "tách rời phụ thuộc", "'decouple A from/and B'."), new EnClBlank(new[] { "failure" }, "điểm lỗi đơn (single point of ___)", "'single point of failure (SPOF)'.") }, "Adding a message queue helps decouple background jobs and removes a single point of failure.", "Thêm hàng đợi thông điệp giúp tách rời tác vụ nền và loại bỏ điểm lỗi đơn.", "Hai thuật ngữ cực kỳ đắt giá trong thảo luận kiến trúc.")
                }, now),
            MakeEnActivity(c, l, 16, "translate",
                "Dịch phản xạ Việt → Anh (Bài 4.3): Code Review & Kiến trúc",
                "Chuyển câu thảo luận kỹ thuật tiếng Việt sang tiếng Anh tự nhiên.",
                new EnTrItem[]
                {
                    new("en-l43-trans-1", "easy", "PR đã sẵn sàng review lại (Dễ)", "Tôi đã đẩy bản sửa lỗi và thêm unit test rồi; bạn xem lại giúp tôi khi rảnh nhé.", new[] { "pushed the fix", "unit test", "take another look" }, new[] { "I have pushed the fix and added a unit test; could you take another look when you have a moment?", "I've pushed the fix and added a unit test—ready for another look whenever you're free." }, "I have pushed the fix and added a unit test; could you take another look when you have a moment?", "Tránh viết 'please help me check', hãy dùng 'take another look'."),
                    new("en-l43-trans-2", "easy", "Báo lỗi xung đột nhánh (Dễ)", "Nhánh này đang có xung đột hợp nhất với nhánh main.", new[] { "merge conflicts", "with main" }, new[] { "This branch has merge conflicts with main.", "There are merge conflicts between this branch and main." }, "This branch has merge conflicts with main.", "Dùng danh từ 'has merge conflicts', không dùng 'is conflict'."),
                    new("en-l43-trans-3", "medium", "Đề xuất tối ưu độ phức tạp (Vừa)", "Đề xuất: Dùng Set ở đây sẽ giúp giảm độ phức tạp tra cứu xuống O(1).", new[] { "suggestion", "using a set", "reduce lookup complexity" }, new[] { "Suggestion: using a Set here would reduce lookup complexity to O(1).", "Suggestion: switching to a Set here would bring lookup complexity down to O(1)." }, "Suggestion: using a Set here would reduce lookup complexity to O(1).", "Dùng 'would + V-inf' cho đề xuất giả định nhẹ nhàng."),
                    new("en-l43-trans-4", "medium", "Hỏi lý do thiết kế (Vừa)", "Tò mò một chút, có lý do gì khiến chúng ta bỏ qua lớp cache ở endpoint này không?", new[] { "out of curiosity", "bypassing", "cache" }, new[] { "Out of curiosity, is there a reason we are bypassing the cache on this endpoint?", "Just out of curiosity, why are we bypassing the cache for this endpoint?" }, "Out of curiosity, is there a reason we are bypassing the cache on this endpoint?", "'Out of curiosity' làm câu hỏi chất vấn trở nên thân thiện."),
                    new("en-l43-trans-5", "hard", "Đảm bảo tương thích ngược (Khó)", "Chúng ta cần đảm bảo thay đổi API này vẫn tương thích ngược với các phiên bản ứng dụng cũ.", new[] { "ensure", "backward-compatible", "older versions" }, new[] { "We need to ensure this API change remains backward-compatible with older app versions.", "We should make sure this API change stays backward-compatible with older clients." }, "We need to ensure this API change remains backward-compatible with older app versions.", "Tính từ ghép 'backward-compatible with...' đi với 'remains' hoặc 'stays'.")
                }, now),
            MakeEnActivity(c, l, 17, "grammar_fix",
                "Sửa lỗi ngữ pháp (Bài 4.3): Tiếng Anh trong PR & Bug Report",
                "Sửa các lỗi sai ngữ pháp và diễn đạt thô trong cộng tác kỹ thuật.",
                new EnGfItem[]
                {
                    new("en-l43-gf-1", "easy", "Lỗi 'conflict' làm tính từ (Dễ)", "My pull request is conflict with the main branch.", "'conflict' không dùng làm tính từ với 'is'; hãy dùng 'has merge conflicts with' hoặc 'conflicts with'.", "My pull request has merge conflicts with the main branch.", new[] { "My pull request has merge conflicts with the main branch.", "My pull request conflicts with the main branch." }, "Pull request của tôi đang bị xung đột với nhánh main.", "Dùng động từ 'has merge conflicts' hoặc 'conflicts with'."),
                    new("en-l43-gf-2", "easy", "Dịch thô 'help me check' (Dễ)", "I fixed the bug, please help me check.", "Tránh 'help me check'; trong PR review hãy nói 'could you please take another look?'.", "I fixed the bug; could you please take another look?", new[] { "I fixed the bug; could you please take another look?", "I've fixed the bug; could you take another look?" }, "Tôi đã sửa lỗi rồi; bạn xem lại giúp tôi nhé?", "'take a look' / 'take another look' tự nhiên hơn nhiều."),
                    new("en-l43-gf-3", "medium", "Thiếu V-ing sau 'prevent ... from' (Vừa)", "Adding a lock prevents two threads overwrite the same row.", "Cấu trúc 'prevent someone/something from + V-ing'.", "Adding a lock prevents two threads from overwriting the same row.", new[] { "Adding a lock prevents two threads from overwriting the same row.", "Adding a lock prevents two threads overwriting the same row." }, "Thêm khóa ngăn hai luồng ghi đè lên cùng một dòng dữ liệu.", "Chuẩn mực là 'prevents X from V-ing'."),
                    new("en-l43-gf-4", "medium", "Giả định câu điều kiện loại 2 (Vừa)", "If we switch to a message queue, the API will responded much faster.", "Sau trợ động từ 'will' (loại 1) hoặc 'would' (loại 2) phải là V nguyên mẫu, không chia -ed.", "If we switched to a message queue, the API would respond much faster.", new[] { "If we switched to a message queue, the API would respond much faster.", "If we switch to a message queue, the API will respond much faster." }, "Nếu chuyển sang hàng đợi thông điệp, API sẽ phản hồi nhanh hơn nhiều.", "Loại 2: If + V2/ed, S + would + V-inf."),
                    new("en-l43-gf-5", "hard", "Thừa giới từ sau 'discuss' (Khó)", "Let's discuss about the database schema trade-offs in our meeting.", "Động từ 'discuss' không bao giờ đi kèm giới từ 'about'.", "Let's discuss the database schema trade-offs in our meeting.", new[] { "Let's discuss the database schema trade-offs in our meeting.", "Let's talk about the database schema trade-offs in our meeting." }, "Hãy thảo luận về các đánh đổi schema CSDL trong cuộc họp của chúng ta.", "'discuss + [something]' hoặc 'talk about + [something]'!")
                }, now),
            MakeEnActivity(c, l, 18, "roleplay",
                "Nhập vai hội thoại (Bài 4.3): Review PR & Thảo luận Kiến trúc",
                "Thực hành phản hồi đồng nghiệp quốc tế trong Code Review và họp kỹ thuật.",
                new EnRpItem[]
                {
                    new("en-l43-rp-1", "easy", "Phản hồi bình luận trên Pull Request (Dễ)", "Tech Lead vừa chỉ ra một lỗi thiếu kiểm tra đầu vào trên PR của bạn.", "Elena (Tech Lead)", "Hey! Overall this PR is super clean, though the endpoint might crash if the list is empty.", "Này! Nhìn chung PR này rất gọn, nhưng endpoint có thể bị sập nếu danh sách truyền vào rỗng đấy.", "Cảm ơn Elena, thừa nhận điểm sót, hứa thêm kiểm tra mảng rỗng và unit test rồi báo review lại.", new[] { "good catch", "empty", "unit test", "re-review" }, new[] { "Good catch, Elena! That makes total sense. I will add an empty-list check plus a unit test and ping you for a re-review." }, 12),
                    new("en-l43-rp-2", "medium", "Viết góp ý Code Review mang tính xây dựng (Vừa)", "Bạn thấy PR của đồng nghiệp đang dùng vòng lặp mảng $O(N)$ trong vòng lặp lớn và tên biến hơi tối nghĩa.", "Alex (Đồng nghiệp)", "Hi! Could you review my PR before lunch? Let me know if anything looks off.", "Chào bạn! Bạn xem giúp mình PR trước giờ ăn trưa nhé? Có gì chưa ổn cứ báo mình.", "Khen phần ổn trước, đưa 1 Suggestion dùng Set để tối ưu và 1 Nit về đặt tên biến lịch sự.", new[] { "clean", "suggestion", "set", "complexity", "nit" }, new[] { "Nice work overall! Suggestion: using a Set instead of an array here would reduce lookup complexity to O(1). Also a minor nit: could we rename 'tmp' for readability?" }, 14),
                    new("en-l43-rp-3", "medium", "Đề xuất phương án xử lý Race Condition (Vừa)", "Cuộc họp đang bàn cách tránh ghi đè dữ liệu khi 2 user cùng lưu 1 ghi chú.", "Elena (Tech Lead)", "If we hold a table-wide lock during saves, throughput will tank. Any alternative ideas?", "Nếu giữ khóa toàn bảng khi lưu, thông lượng sẽ sụt giảm nặng. Có phương án nào khác không?", "Đề xuất dùng Optimistic Concurrency Control (kiểm tra baseVersion nguyên tử và trả 409 Conflict).", new[] { "what if", "atomic", "version", "conflict", "lock" }, new[] { "What if we use an atomic version check instead of a table lock and return 409 Conflict on version mismatch?" }, 14),
                    new("en-l43-rp-4", "hard", "Thuyết phục tách tác vụ nặng khỏi luồng HTTP (Khó)", "Đồng nghiệp muốn gọi trực tiếp API AI mất 15 giây ngay trong HTTP POST request.", "Alex (Đồng nghiệp)", "Why add a background queue? Calling the AI model synchronously inside the POST handler is simpler!", "Sao phải thêm hàng đợi nền? Gọi model AI đồng bộ ngay trong POST handler đơn giản hơn mà!", "Công nhận sự đơn giản nhưng chỉ ra rủi ro HTTP timeout và đề xuất tách bằng Outbox/Queue.", new[] { "see your point", "however", "timeout", "decouple", "queue" }, new[] { "I see your point about simplicity; however, synchronous 15-second AI calls risk HTTP timeouts under load. Decoupling via a background queue keeps API responses fast and resilient." }, 16)
                }, now)
        };
    }

    private static List<PracticeActivity> BuildEn51Activities(Guid c, DateTimeOffset now)
    {
        var l = EnLesson51Id;
        return new()
        {
            MakeEnActivity(c, l, 11, "shadowing",
                "Shadowing (Bài 5.1): Kể chuyện Dữ liệu & Thuyết phục Lãnh đạo",
                "Luyện ngữ điệu thuyết trình Executive Pitch (BLUF) và trình bày con số rõ ràng.",
                new EnShItem[]
                {
                    new("en-l51-shad-e1", "en-US", "easy", "Mở đầu BLUF (Dễ)", "I recommend investing two weeks into upgrading our search cache.", "/aɪ ˌrekəˈmend ɪnˈvestɪŋ tuː wiːks ˈɪntuː ʌpˈɡreɪdɪŋ ˈaʊə sɜːtʃ kæʃ/", "Tôi đề xuất đầu tư hai tuần để nâng cấp lớp bộ nhớ đệm tìm kiếm.", new[] { "recommend investing", "search cache" }),
                    new("en-l51-shad-e2", "en-US", "easy", "Nêu kết quả giảm chi phí (Dễ)", "This improvement will cut our cloud database bill by thirty-five percent.", "/ðɪs ɪmˈpruːvmənt wɪl kʌt ˈaʊə klaʊd ˈdeɪtəbeɪs bɪl baɪ ˈθɜːti faɪv pəˈsent/", "Cải tiến này sẽ cắt giảm 35% hóa đơn cơ sở dữ liệu đám mây của chúng ta.", new[] { "cut our bill", "thirty-five percent" }),
                    new("en-l51-shad-m1", "en-US", "medium", "Mô tả xu hướng tương phản (Vừa)", "While daily active users surged by forty percent, page load times crept up.", "/waɪl ˈdeɪli ˈæktɪv ˈjuːzəz sɜːdʒd baɪ ˈfɔːti pəˈsent peɪdʒ ləʊd taɪmz krept ʌp/", "Trong khi người dùng hoạt động hàng ngày tăng vọt 40%, thời gian tải trang nhích tăng lên.", new[] { "surged by", "crept up" }),
                    new("en-l51-shad-m2", "en-US", "medium", "Trình bày hoàn vốn ROI (Vừa)", "In dollar terms, the engineering effort pays for itself within two months.", "/ɪn ˈdɒlə tɜːmz ði ˌendʒɪˈnɪərɪŋ ˈefət peɪz fɔːr ɪtˈself wɪðˈɪn tuː mʌnθs/", "Tính bằng tiền, nỗ lực kỹ thuật này tự hoàn vốn chỉ trong vòng hai tháng.", new[] { "in dollar terms", "pays for itself" }),
                    new("en-l51-shad-h1", "en-US", "hard", "Chỉ ra Insight từ phễu dữ liệu (Khó)", "Drilling into funnel analytics revealed that sixty percent of users drop off during onboarding.", "/ˈdrɪlɪŋ ˈɪntuː ˈfʌnl ˌænəˈlɪtɪks rɪˈviːld ðæt ˈsɪksti pəˈsent əv ˈjuːzəz drɒp ɒf ˈdjʊərɪŋ ˈɒnbɔːdɪŋ/", "Đi sâu vào phân tích phễu cho thấy 60% người dùng rời bỏ ngay trong bước làm quen.", new[] { "funnel analytics", "drop off" }),
                    new("en-l51-shad-h2", "en-US", "hard", "Kêu gọi phê duyệt ngân sách (Khó)", "By automating validation today, we project a fifteen percent lift in quarterly retention.", "/baɪ ˈɔːtəmeɪtɪŋ ˌvælɪˈdeɪʃn təˈdeɪ wiː prəˈdʒekt ə ˌfɪfˈtiːn pəˈsent lɪft ɪn ˈkwɔːtəli rɪˈtenʃn/", "Bằng cách tự động hóa kiểm duyệt hôm nay, chúng tôi dự kiến mức giữ chân quý tăng 15%.", new[] { "project a lift", "quarterly retention" })
                }, now),
            MakeEnActivity(c, l, 12, "dictation",
                "Dictation (Bài 5.1): Nghe chép Số liệu & Xu hướng Kinh doanh",
                "Nghe chính xác các giới từ số liệu (by/to/at/from) và động từ biến động.",
                new EnDiItem[]
                {
                    new("en-l51-dict-e1", "en-US", "easy", "Doanh thu tăng lên mức (Dễ)", "Quarterly revenue rose to two point five million dollars.", "/ˈkwɔːtəli ˈrevənjuː rəʊz tuː tuː pɔɪnt faɪv ˈmɪljən ˈdɒləz/", "Doanh thu quý đã tăng lên mức 2,5 triệu đô la.", new[] { "Quarterly", "revenue", "rose", "to", "two", "point", "five", "million", "dollars" }, new[] { "by", "comma", "millions" }),
                    new("en-l51-dict-e2", "en-US", "easy", "Tỷ lệ rời bỏ ổn định (Dễ)", "Customer churn stabilized at four point two percent.", "/ˈkʌstəmə tʃɜːn ˈsteɪbəlaɪzd æt fɔː pɔɪnt tuː pəˈsent/", "Tỷ lệ khách hàng rời bỏ đã ổn định ở mức 4,2%.", new[] { "Customer", "churn", "stabilized", "at", "four", "point", "two", "percent" }, new[] { "percents", "to", "by" }),
                    new("en-l51-dict-m1", "en-US", "medium", "Tốc độ trang giảm mạnh (Vừa)", "Response latency plummeted from two seconds to ninety milliseconds.", "/rɪˈspɒns ˈleɪtənsi ˈplʌmɪtɪd frəm tuː ˈsekəndz tuː ˈnaɪnti ˈmɪlɪsekəndz/", "Độ trễ phản hồi đã giảm lao dốc từ 2 giây xuống còn 90 mili-giây.", new[] { "Response", "latency", "plummeted", "from", "two", "seconds", "to", "ninety", "milliseconds" }, new[] { "plateaued", "by", "at" }),
                    new("en-l51-dict-m2", "en-US", "medium", "Xu hướng đi ngang (Vừa)", "After rapid growth, weekly signups plateaued at five thousand users.", "/ˈɑːftə ˈræpɪd ɡrəʊθ ˈwiːkli ˈsaɪnʌps ˈplætəʊd æt faɪv ˈθaʊznd ˈjuːzəz/", "Sau giai đoạn tăng nhanh, lượng đăng ký hàng tuần chững lại ở mức 5.000 người.", new[] { "After", "rapid", "growth", "weekly", "signups", "plateaued", "at", "five", "thousand", "users" }, new[] { "thousands", "dropped", "in" }),
                    new("en-l51-dict-h1", "en-US", "hard", "Xin phê duyệt triển khai (Khó)", "Once leadership reviews the ROI summary, we expect to get the green light.", "/wʌns ˈliːdəʃɪp rɪˈvjuːz ði ɑːr əʊ aɪ ˈsʌməri wiː ɪkˈspekt tuː ɡet ðə ɡriːn laɪt/", "Khi lãnh đạo xem xong bản tóm tắt ROI, chúng tôi kỳ vọng sẽ được bật đèn xanh.", new[] { "Once", "leadership", "reviews", "the", "ROI", "summary", "we", "expect", "to", "get", "the", "green", "light" }, new[] { "approval", "red", "want" }),
                    new("en-l51-dict-h2", "en-US", "hard", "Điểm cốt lõi kinh doanh (Khó)", "The bottom line is that checkout speed directly drives customer trust and revenue.", "/ðə ˈbɒtəm laɪn ɪz ðæt ˈtʃekaʊt spiːd dɪˈrektli draɪvz ˈkʌstəmə trʌst ænd ˈrevənjuː/", "Kết luận cốt lõi là tốc độ thanh toán thúc đẩy trực tiếp niềm tin và doanh thu.", new[] { "The", "bottom", "line", "is", "that", "checkout", "speed", "directly", "drives", "customer", "trust", "and", "revenue" }, new[] { "top", "causes", "profit" })
                }, now),
            MakeEnActivity(c, l, 13, "pronunciation",
                "Phát âm IPA (Bài 5.1): Số liệu & Động từ Xu hướng Kinh doanh",
                "Luyện đọc đúng trọng âm plateau, plummet, trajectory, correlation.",
                new EnPrItem[]
                {
                    new("en-l51-pron-e1", "en-US", "easy", "Âm /s/ và /t/ trong percent (Dễ)", "Cloud expenses fell by twenty-five percent.", "/klaʊd ɪkˈspensɪz fel baɪ ˈtwenti faɪv pəˈsent/", "Chi phí đám mây đã giảm 25%.", new[] { "percent", "expenses", "twenty-five" }, new[] { "twenty-five percent" }),
                    new("en-l51-pron-e2", "en-US", "easy", "Âm /sɜːdʒ/ trong surge (Dễ)", "Mobile traffic surged right after the product launch.", "/ˈməʊbaɪl ˈtræfɪk sɜːdʒd raɪt ˈɑːftə ðə ˈprɒdʌkt lɔːntʃ/", "Lưu lượng di động tăng vọt ngay sau khi ra mắt sản phẩm.", new[] { "surged", "mobile", "launch" }, new[] { "traffic surged" }),
                    new("en-l51-pron-m1", "en-US", "medium", "Phát âm plateau /ˈplætəʊ/ (Vừa)", "User acquisition plateaued at fifty thousand accounts.", "/ˈjuːzər ˌækwɪˈzɪʃn ˈplætəʊd æt ˈfɪfti ˈθaʊznd əˈkaʊnts/", "Lượng người dùng mới đã đi ngang ở mốc 50.000 tài khoản.", new[] { "plateaued", "acquisition", "thousand" }, new[] { "acquisition plateaued" }),
                    new("en-l51-pron-m2", "en-US", "medium", "Âm /tʃ/ và /k/ trong trajectory (Vừa)", "The chart highlights a sharp upward trajectory.", "/ðə tʃɑːt ˈhaɪlaɪts ə ʃɑːp ˈʌpwəd trəˈdʒektəri/", "Biểu đồ làm nổi bật một quỹ đạo đi lên sắc nét.", new[] { "trajectory", "upward", "highlights" }, new[] { "upward trajectory" }),
                    new("en-l51-pron-h1", "en-US", "hard", "Trọng âm correlation & retention (Khó)", "Data shows a strong correlation between page speed and retention.", "/ˈdeɪtə ʃəʊz ə strɒŋ ˌkɒrəˈleɪʃn bɪˈtwiːn peɪdʒ spiːd ænd rɪˈtenʃn/", "Dữ liệu cho thấy mối tương quan mạnh giữa tốc độ trang và khả năng giữ chân.", new[] { "correlation", "retention", "strong" }, new[] { "strong correlation" }),
                    new("en-l51-pron-h2", "en-US", "hard", "Âm cuối /ts/ trong insights & metrics (Khó)", "Actionable insights from Q3 metrics justified the budget increase.", "/ˈækʃənəbl ˈɪnsaɪts frəm kjuː θriː ˈmetrɪks ˈdʒʌstɪfaɪd ðə ˈbʌdʒɪt ˈɪnkriːs/", "Những góc nhìn thiết thực từ số liệu quý 3 đã chứng minh tính hợp lý của việc tăng ngân sách.", new[] { "insights", "metrics", "justified" }, new[] { "actionable insights" })
                }, now),
            MakeEnActivity(c, l, 14, "word_map",
                "Word Map (Bài 5.1): Ngôn ngữ Kể chuyện Số liệu & ROI",
                "Mở rộng vốn từ vựng trình bày biểu đồ, xu hướng và thuyết phục lãnh đạo.",
                new EnWmItem[]
                {
                    new("wm-l51-e1", "en-US", "easy", "Sơ đồ từ vựng: surge", "surge", "/sɜːdʒ/", "Tăng vọt đột biến",
                        new[] { new EnWmNode("surge by 40%", "/sɜːdʒ baɪ ˈfɔːti pəˈsent/", "Tăng vọt thêm 40%", "Orders surged by 40% during the flash sale."), new EnWmNode("sudden surge", "/ˈsʌdn sɜːdʒ/", "Sự tăng vọt bất ngờ", "Prepare auto-scaling for a sudden traffic surge.") },
                        new[] { new EnWmNode("shoot up", "/ʃuːt ʌp/", "Tăng vù vù", "Signups shot up after the keynote."), new EnWmNode("pick up", "/pɪk ʌp/", "Khởi sắc tăng tốc", "Sales picked up in the fourth quarter.") },
                        new[] { new EnWmNode("soar", "/sɔːr/", "Tăng vút lên cao", "Cloud usage soared over the holidays."), new EnWmNode("skyrocket", "/ˈskaɪrɒkɪt/", "Tăng phi mã", "Adoption skyrocketed following the redesign.") },
                        new[] { new EnWmEx("Daily active users surged from 80,000 to 120,000 after launching mobile sync.", "Người dùng hàng ngày tăng vọt từ 80.000 lên 120.000 sau khi ra mắt đồng bộ di động.") }),
                    new("wm-l51-e2", "en-US", "easy", "Sơ đồ từ vựng: plummet", "plummet", "/ˈplʌmɪt/", "Lao dốc, sụt giảm mạnh",
                        new[] { new EnWmNode("plummet sharply", "/ˈplʌmɪt ˈʃɑːpli/", "Giảm sâu đột ngột", "Error rates plummeted sharply after the patch."), new EnWmNode("plummet from X to Y", "/ˈplʌmɪt frəm ... tuː .../", "Tụt từ X xuống Y", "Latency plummeted from 2s to 90ms.") },
                        new[] { new EnWmNode("drop off", "/drɒp ɒf/", "Rơi rụng, sụt giảm", "Traffic dropped off after midnight."), new EnWmNode("fall behind", "/fɔːl bɪˈhaɪnd/", "Tụt lại phía sau", "Without updates, conversion falls behind.") },
                        new[] { new EnWmNode("plunge", "/plʌndʒ/", "Rơi thẳng đứng", "Bounce rates plunged by half."), new EnWmNode("tumble", "/ˈtʌmbl/", "Sụt trượt mạnh", "Support tickets tumbled after FAQ launch.") },
                        new[] { new EnWmEx("Once we added Redis caching, database CPU utilization plummeted by 65 percent.", "Khi thêm cache Redis, mức tải CPU của database đã lao dốc giảm 65%.") }),
                    new("wm-l51-m1", "en-US", "medium", "Sơ đồ từ vựng: plateau", "plateau", "/ˈplætəʊ/", "Đi ngang, chững lại",
                        new[] { new EnWmNode("plateau at", "/ˈplætəʊ æt/", "Chững lại tại mức", "Growth plateaued at ten thousand subscribers."), new EnWmNode("hit a plateau", "/hɪt ə ˈplætəʊ/", "Chạm ngưỡng bão hòa đi ngang", "Organic reach hit a plateau in Q2.") },
                        new[] { new EnWmNode("level off", "/ˈlevl ɒf/", "Bình ổn đi ngang", "Signups leveled off over the summer."), new EnWmNode("flatten out", "/ˈflætn aʊt/", "Phẳng ra không tăng nữa", "The cost curve flattened out.") },
                        new[] { new EnWmNode("stabilize", "/ˈsteɪbəlaɪz/", "Ổn định lại", "Retention stabilized at 82%."), new EnWmNode("stagnate", "/stæɡˈneɪt/", "Trì trệ giậm chân", "Revenue stagnated without new markets.") },
                        new[] { new EnWmEx("User growth plateaued in May, signaling a need for referral incentives.", "Tăng trưởng người dùng chững lại vào tháng 5, báo hiệu cần chính sách giới thiệu bạn bè.") }),
                    new("wm-l51-m2", "en-US", "medium", "Sơ đồ từ vựng: retention", "retention", "/rɪˈtenʃn/", "Tỷ lệ giữ chân người dùng",
                        new[] { new EnWmNode("customer retention", "/ˈkʌstəmə rɪˈtenʃn/", "Giữ chân khách hàng", "Improving onboarding boosts customer retention."), new EnWmNode("retention rate", "/rɪˈtenʃn reɪt/", "Tỷ lệ duy trì quay lại", "Our 90-day retention rate reached 74%.") },
                        new[] { new EnWmNode("stick around", "/stɪk əˈraʊnd/", "Ở lại dùng lâu dài", "Users stick around when the app feels fast."), new EnWmNode("come back", "/kʌm bæk/", "Quay trở lại", "Daily reminders bring learners back.") },
                        new[] { new EnWmNode("loyalty", "/ˈlɔɪəlti/", "Sự trung thành", "Fast support builds brand loyalty."), new EnWmNode("stickiness", "/ˈstɪkinəs/", "Độ gắn kết sản phẩm", "Habit loops increase product stickiness.") },
                        new[] { new EnWmEx("A five-percent lift in retention generates more profit than costly ad campaigns.", "Tăng 5% tỷ lệ giữ chân mang lại lợi nhuận cao hơn các chiến dịch quảng cáo đắt đỏ.") }),
                    new("wm-l51-h1", "en-US", "hard", "Sơ đồ từ vựng: ROI", "ROI", "/ˌɑːr əʊ ˈaɪ/", "Tỷ suất hoàn vốn đầu tư",
                        new[] { new EnWmNode("positive ROI", "/ˈpɒzətɪv ˌɑːr əʊ ˈaɪ/", "Hoàn vốn dương có lãi", "The automation showed positive ROI in two months."), new EnWmNode("quantify ROI", "/ˈkwɒntɪfaɪ ˌɑːr əʊ ˈaɪ/", "Định lượng hiệu quả đầu tư", "Always quantify ROI before pitching executives.") },
                        new[] { new EnWmNode("pay off", "/peɪ ɒf/", "Đem lại trái ngọt", "Investing in tests paid off within weeks."), new EnWmNode("break even", "/breɪk ˈiːvn/", "Hòa vốn", "We broke even on cloud migration in Q4.") },
                        new[] { new EnWmNode("payback", "/ˈpeɪbæk/", "Thời gian thu hồi vốn", "A two-month payback period convinced the CFO."), new EnWmNode("yield", "/jiːld/", "Lợi tức sinh lời", "That sprint delivered the highest yield.") },
                        new[] { new EnWmEx("Framing engineering upgrades around measurable ROI wins rapid executive approval.", "Đóng khung nâng cấp kỹ thuật gắn với ROI đo được giúp thuyết phục lãnh đạo nhanh chóng.") }),
                    new("wm-l51-h2", "en-US", "hard", "Sơ đồ từ vựng: green light", "green light", "/ɡriːn laɪt/", "Sự phê duyệt chính thức",
                        new[] { new EnWmNode("get the green light", "/ɡet ðə ɡriːn laɪt/", "Được bật đèn xanh duyệt làm", "We got the green light to hire two engineers."), new EnWmNode("give the green light", "/ɡɪv ðə ɡriːn laɪt/", "Cấp phép thông qua", "The VP gave the green light for Phase 2.") },
                        new[] { new EnWmNode("sign off on", "/saɪn ɒf ɒn/", "Ký duyệt thông qua", "Finance signed off on the cloud budget."), new EnWmNode("move ahead", "/muːv əˈhed/", "Tiến hành triển khai", "We can move ahead next Monday.") },
                        new[] { new EnWmNode("approval", "/əˈpruːvl/", "Sự chấp thuận", "Budget approval arrived yesterday."), new EnWmNode("endorsement", "/ɪnˈdɔːsmənt/", "Sự tán thành ủng hộ", "Executive endorsement accelerated rollout.") },
                        new[] { new EnWmEx("With compelling benchmark data in hand, we secured the green light in ten minutes.", "Với số liệu benchmark thuyết phục trong tay, chúng tôi được duyệt chỉ sau 10 phút.") })
                }, now),
            MakeEnActivity(c, l, 15, "cloze",
                "Điền từ ngữ cảnh (Bài 5.1): Trình bày Số liệu & Thuyết phục",
                "Chọn đúng giới từ số liệu và từ vựng thuyết phục lãnh đạo.",
                new EnClItem[]
                {
                    new("en-l51-cloze-1", "easy", "Giới từ số liệu tăng thêm vs chạm mốc (Dễ)", "Điền 'by' hoặc 'to' vào chỗ trống:", "Daily active users grew {{0}} 25% last quarter, reaching up {{1}} 90,000.", new[] { new EnClBlank(new[] { "by" }, "mức tăng thêm", "Dùng 'grew by 25%'."), new EnClBlank(new[] { "to" }, "chạm mức mới", "'up to 90,000' = lên tới mốc 90.000.") }, "Daily active users grew by 25% last quarter, reaching up to 90,000.", "Người dùng hoạt động hàng ngày tăng thêm 25% quý trước, đạt tới 90.000.", "Phân biệt chuẩn xác mức chênh lệch (by) và đích đến (to)."),
                    new("en-l51-cloze-2", "easy", "Đứng yên tại một mốc (Dễ)", "Điền động từ đi ngang và giới từ tại mốc:", "After rapid growth, weekly retention {{0}} {{1}} 78%.", new[] { new EnClBlank(new[] { "plateaued", "stabilized", "leveled off" }, "chững lại / ổn định", "Xu hướng đi ngang dùng 'plateaued' hoặc 'stabilized'."), new EnClBlank(new[] { "at" }, "tại một điểm cố định", "Dùng 'at' cho điểm đứng yên.") }, "After rapid growth, weekly retention plateaued at 78%.", "Sau tăng trưởng nhanh, tỷ lệ giữ chân tuần chững lại ở mức 78%.", "Cụm cố định: 'plateau at [number]'."),
                    new("en-l51-cloze-3", "medium", "Tự hoàn vốn nhanh (Vừa)", "Điền thành ngữ hoàn vốn:", "In dollar terms, this caching upgrade {{0}} for {{1}} within two months.", new[] { new EnClBlank(new[] { "pays" }, "chia số ít với upgrade", "'pay for itself' = tự hoàn vốn."), new EnClBlank(new[] { "itself" }, "đại từ phản thân của upgrade", "Thành ngữ cố định: 'pays for itself'.") }, "In dollar terms, this caching upgrade pays for itself within two months.", "Tính bằng tiền, nâng cấp cache này tự hoàn vốn trong vòng 2 tháng.", "Cụm từ cực mạnh khi xin ngân sách với CFO/VP."),
                    new("en-l51-cloze-4", "medium", "Danh từ biến động + of (Vừa)", "Điền giới từ sau danh từ 'a drop':", "There was a sharp {{0}} {{1}} 40% in support tickets after launching self-service.", new[] { new EnClBlank(new[] { "drop", "decrease", "reduction" }, "sự sụt giảm (danh từ)", "Sau 'a sharp' cần danh từ."), new EnClBlank(new[] { "of" }, "giới từ sau danh từ chỉ số liệu", "'a drop of 40%' (không dùng by sau danh từ!).") }, "There was a sharp drop of 40% in support tickets after launching self-service.", "Số lượng phiếu hỗ trợ đã giảm mạnh 40% sau khi ra mắt tự phục vụ.", "Động từ dùng 'by', Danh từ dùng 'of'!"),
                    new("en-l51-cloze-5", "hard", "Chốt kết luận cốt lõi (Khó)", "Điền cụm kết luận và phê duyệt:", "The {{0}} line is that speed drives revenue; may we get the {{1}} light for Sprint 12?", new[] { new EnClBlank(new[] { "bottom" }, "điểm cốt lõi (The ___ line)", "'The bottom line is that...'."), new EnClBlank(new[] { "green" }, "bật đèn xanh phê duyệt", "'get the green light' = được duyệt triển khai.") }, "The bottom line is that speed drives revenue; may we get the green light for Sprint 12?", "Điểm cốt lõi là tốc độ thúc đẩy doanh thu; chúng tôi xin phép được phê duyệt cho Sprint 12 nhé?", "Hai thành ngữ kinh doanh xuất sắc nhất cho phần kết pitch.")
                }, now),
            MakeEnActivity(c, l, 16, "translate",
                "Dịch phản xạ Việt → Anh (Bài 5.1): Thuyết phục bằng Dữ liệu",
                "Chuyển các lập luận dữ liệu kinh doanh sang tiếng Anh tự nhiên.",
                new EnTrItem[]
                {
                    new("en-l51-trans-1", "easy", "Đề xuất đầu tư có số liệu (Dễ)", "Tôi đề xuất đầu tư hai tuần kỹ thuật để cắt giảm 35% chi phí đám mây.", new[] { "recommend investing", "cut cloud costs", "by 35 percent" }, new[] { "I recommend investing two engineering weeks to cut cloud costs by 35 percent.", "I'd recommend investing two engineering weeks to reduce cloud expenses by 35%." }, "I recommend investing two engineering weeks to cut cloud costs by 35 percent.", "Sau 'recommend' dùng trực tiếp V-ing (investing)."),
                    new("en-l51-trans-2", "easy", "Mô tả số liệu giảm mạnh (Dễ)", "Thời gian tải trang đã giảm từ 1,8 giây xuống 0,2 giây.", new[] { "dropped from", "1.8 seconds", "to 0.2 seconds" }, new[] { "Page load time dropped from 1.8 seconds to 0.2 seconds.", "Page load times fell from 1.8 seconds to 0.2 seconds." }, "Page load time dropped from 1.8 seconds to 0.2 seconds.", "Dùng dấu chấm thập phân (one point eight) trong tiếng Anh."),
                    new("en-l51-trans-3", "medium", "Nêu tương quan từ phễu (Vừa)", "Phân tích phễu cho thấy 60% người dùng rời bỏ khi bước đăng ký chậm.", new[] { "funnel analytics", "revealed", "drop off" }, new[] { "Funnel analytics revealed that 60 percent of users drop off when signup is slow.", "Our funnel analysis showed that 60% of users drop off during slow signups." }, "Funnel analytics revealed that 60 percent of users drop off when signup is slow.", "'drop off' = rơi rụng, thoát giữa chừng."),
                    new("en-l51-trans-4", "medium", "Chứng minh khả năng tự hoàn vốn (Vừa)", "Khoản nâng cấp này sẽ tự hoàn vốn trong vòng hai tháng nhờ tiết kiệm tài nguyên máy chủ.", new[] { "upgrade", "pay for itself", "within two months" }, new[] { "This upgrade will pay for itself within two months through server savings.", "This upgrade will pay for itself within two months by saving server resources." }, "This upgrade will pay for itself within two months through server savings.", "'pay for itself' diễn tả khoản đầu tư sinh lời nhanh."),
                    new("en-l51-trans-5", "hard", "Mở đầu BLUF thuyết phục VP (Khó)", "Trong khi lượng truy cập tăng vọt 40%, tỷ lệ giữ chân lại chững ở mức 22% do độ trễ tìm kiếm.", new[] { "while traffic surged", "retention plateaued at" }, new[] { "While traffic surged by 40 percent, retention plateaued at 22 percent due to search latency.", "Although traffic surged 40%, our retention plateaued at 22% because of search latency." }, "While traffic surged by 40 percent, retention plateaued at 22 percent due to search latency.", "Dùng 'While + mệnh đề 1, mệnh đề 2' tạo sự tương phản dữ liệu sắc nét.")
                }, now),
            MakeEnActivity(c, l, 17, "grammar_fix",
                "Sửa lỗi ngữ pháp (Bài 5.1): Lỗi số liệu & phần trăm",
                "Khắc phục các lỗi dùng sai 'percents', 'million', 'increase' và giới từ số liệu.",
                new EnGfItem[]
                {
                    new("en-l51-gf-1", "easy", "Thêm -s vào từ percent (Dễ)", "Our cloud costs dropped by 30 percents this quarter.", "Từ 'percent' KHÔNG bao giờ có dạng số nhiều 'percents'.", "Our cloud costs dropped by 30 percent this quarter.", new[] { "Our cloud costs dropped by 30 percent this quarter." }, "Chi phí đám mây đã giảm 30% trong quý này.", "Luôn viết '30 percent' hoặc '30%'."),
                    new("en-l51-gf-2", "easy", "Thêm -s vào million sau số đếm (Dễ)", "Annual revenue reached two millions dollars.", "Khi có số đếm cụ thể đứng trước (two, five...), hundred/thousand/million KHÔNG thêm -s.", "Annual revenue reached two million dollars.", new[] { "Annual revenue reached two million dollars." }, "Doanh thu hàng năm đã đạt 2 triệu đô la.", "Chỉ thêm -s khi nói ước lượng chung: 'millions of dollars'."),
                    new("en-l51-gf-3", "medium", "Thiếu giới từ 'by' hoặc 'to' (Vừa)", "Daily active users increased 40% yesterday.", "Trong báo cáo trang trọng, sau 'increased' cần 'by 40%' để phân biệt với 'to 40%'.", "Daily active users increased by 40% yesterday.", new[] { "Daily active users increased by 40% yesterday." }, "Số người dùng hoạt động hàng ngày đã tăng thêm 40% hôm qua.", "Thêm 'by' trước mức % chênh lệch."),
                    new("en-l51-gf-4", "medium", "Dùng 'recommend to V' sai cấu trúc (Vừa)", "I recommend to upgrade our database cache before Black Friday.", "Động từ 'recommend' đi trực tiếp với 'V-ing' (hoặc 'that S + V'), KHÔNG dùng 'recommend to V'.", "I recommend upgrading our database cache before Black Friday.", new[] { "I recommend upgrading our database cache before Black Friday.", "I recommend that we upgrade our database cache before Black Friday." }, "Tôi đề xuất nâng cấp bộ nhớ đệm CSDL trước Black Friday.", "Cấu trúc chuẩn: 'recommend + V-ing'."),
                    new("en-l51-gf-5", "hard", "Sai giới từ sau danh từ 'an increase' (Khó)", "There was an increase by 18% in checkout conversions.", "Sau danh từ 'an increase / a drop', dùng giới từ 'of' chứ không dùng 'by'.", "There was an increase of 18% in checkout conversions.", new[] { "There was an increase of 18% in checkout conversions." }, "Tỷ lệ chuyển đổi thanh toán đã có mức tăng 18%.", "Động từ tăng 'by'; Danh từ mức tăng 'of'.")
                }, now),
            MakeEnActivity(c, l, 18, "roleplay",
                "Nhập vai hội thoại (Bài 5.1): Thuyết trình Số liệu trước Ban Giám đốc",
                "Thực hành mở bài BLUF, giải thích biểu đồ và chốt phê duyệt ngân sách.",
                new EnRpItem[]
                {
                    new("en-l51-rp-1", "easy", "Mở đầu báo cáo BLUF trong 2 câu (Dễ)", "Bạn bắt đầu cuộc họp 1-1 với VP of Product để xin 2 tuần nâng cấp tìm kiếm.", "David (VP of Product)", "Hi Minh, I only have ten minutes between meetings. What's your key recommendation today?", "Chào Minh, tôi chỉ có 10 phút giữa các cuộc họp thôi. Đề xuất chính hôm nay của bạn là gì?", "Đưa kết luận lên đầu (BLUF): xin 2 tuần kỹ thuật để giảm 35% chi phí và tăng tốc độ.", new[] { "recommend", "two weeks", "cut", "percent" }, new[] { "I recommend investing two engineering weeks to upgrade our search cache, which will cut cloud database costs by 35 percent." }, 12),
                    new("en-l51-rp-2", "medium", "Giải thích số liệu tương phản trên biểu đồ (Vừa)", "Lãnh đạo hỏi số liệu nào chứng minh sự cần thiết phải nâng cấp.", "David (VP of Product)", "Thirty-five percent sounds promising. What metrics show we need this upgrade right now?", "35% nghe rất hứa hẹn. Số liệu nào cho thấy chúng ta cần nâng cấp này ngay lúc này?", "Nêu rõ lượng user tăng vọt (surged by) trong khi độ trễ nhích tăng khiến user rời bỏ (drop off).", new[] { "surged", "latency", "drop off", "percent" }, new[] { "While daily active users surged by 40 percent, search latency crept up to 1.8 seconds, causing 60 percent of users to drop off." }, 14),
                    new("en-l51-rp-3", "medium", "Chứng minh ROI để xin duyệt ngân sách (Vừa)", "VP băn khoăn về chi phí bỏ ra cho hai tuần kỹ thuật.", "David (VP of Product)", "Two engineering weeks isn't cheap. How soon does this effort pay for itself?", "Hai tuần kỹ thuật không rẻ đâu. Khi nào nỗ lực này mới thu hồi vốn?", "Nêu kết quả pilot benchmark và khẳng định giải pháp tự hoàn vốn (pay for itself) trong 2 tháng.", new[] { "pilot", "dropped", "pay for itself", "months" }, new[] { "Based on our pilot, latency dropped from 1.8 seconds to 140 milliseconds, and the server savings will pay for itself within two months." }, 14),
                    new("en-l51-rp-4", "hard", "Chốt hành động & xin đèn xanh (Khó)", "VP đã bị thuyết phục và hỏi bước tiếp theo.", "David (VP of Product)", "Those numbers make a strong case. What do you need from me to kick this off?", "Các con số đó rất thuyết phục. Bạn cần gì từ tôi để bắt đầu triển khai?", "Xin phê duyệt chính thức (green light) cho Sprint tới và cam kết gửi tài liệu đặc tả 1 trang.", new[] { "green light", "next sprint", "spec", "retention" }, new[] { "If you give us the green light for next sprint, I will send over a one-page spec today and track our quarterly retention lift closely." }, 14)
                }, now)
        };
    }

    private static List<PracticeActivity> BuildEn52Activities(Guid c, DateTimeOffset now)
    {
        var l = EnLesson52Id;
        return new()
        {
            MakeEnActivity(c, l, 11, "shadowing",
                "Shadowing (Bài 5.2): Xử lý Khủng hoảng & Từ chối Khéo léo",
                "Luyện giọng điệu điềm tĩnh, thấu cảm nhưng kiên định trong tình huống căng thẳng.",
                new EnShItem[]
                {
                    new("en-l52-shad-e1", "en-US", "easy", "Công nhận & Xin lỗi lịch sự (Dễ)", "I hear you, and I sincerely apologize for the disruption this caused.", "/aɪ hɪə juː ænd aɪ sɪnˈsɪəli əˈpɒlədʒaɪz fɔː ðə dɪsˈrʌpʃn ðɪs kɔːzd/", "Tôi lắng nghe bạn, và tôi chân thành xin lỗi vì sự gián đoạn này.", new[] { "sincerely apologize", "disruption" }),
                    new("en-l52-shad-e2", "en-US", "easy", "Khẳng định mức ưu tiên (Dễ)", "Resolving this issue is our absolute top priority right now.", "/rɪˈzɒlvɪŋ ðɪs ˈɪʃuː ɪz ˈaʊər ˈæbsəluːt tɒp praɪˈɒrəti raɪt naʊ/", "Giải quyết sự cố này đang là ưu tiên số một của chúng tôi ngay lúc này.", new[] { "top priority", "right now" }),
                    new("en-l52-shad-m1", "en-US", "medium", "Đưa giải pháp tạm thời (Vừa)", "While our engineers deploy the permanent fix, I can provide a reliable workaround.", "/waɪl ˈaʊər ˌendʒɪˈnɪəz dɪˈplɔɪ ðə ˈpɜːmənənt fɪks aɪ kæn prəˈvaɪd ə rɪˈlaɪəbl ˈwɜːkəraʊnd/", "Trong lúc kỹ sư triển khai bản vá vĩnh viễn, tôi có thể cung cấp phương án thay thế tạm thời.", new[] { "permanent fix", "reliable workaround" }),
                    new("en-l52-shad-m2", "en-US", "medium", "Từ chối khéo theo Positive No (Vừa)", "I understand why you want this by Friday; however, we cannot skip security testing.", "/aɪ ˌʌndəˈstænd waɪ juː wɒnt ðɪs baɪ ˈfraɪdeɪ haʊˈevə wiː ˈkænɒt skɪp sɪˈkjʊərəti ˈtestɪŋ/", "Tôi hiểu vì sao bạn cần việc này trước thứ Sáu; tuy nhiên, chúng tôi không thể bỏ qua kiểm thử bảo mật.", new[] { "however", "security testing" }),
                    new("en-l52-shad-h1", "en-US", "hard", "Phản biện đa chiều lịch thiệp (Khó)", "Just to play devil's advocate, what happens if traffic triples during the holiday campaign?", "/dʒʌst tuː pleɪ ˈdevlz ˈædvəkət wɒt ˈhæpənz ɪf ˈtræfɪk ˈtrɪplz ˈdjʊərɪŋ ðə ˈhɒlɪdeɪ kæmˈpeɪn/", "Thử đóng vai phản biện ngược một chút, chuyện gì xảy ra nếu lưu lượng tăng gấp ba dịp lễ?", new[] { "play devil's advocate", "traffic triples" }),
                    new("en-l52-shad-h2", "en-US", "hard", "Đề xuất thay thế giữ quan hệ (Khó)", "What we can do instead is deliver the core export now and ship the full dashboard next Wednesday.", "/wɒt wiː kæn duː ɪnˈsted ɪz dɪˈlɪvə ðə kɔːr ˈekspɔːt naʊ ænd ʃɪp ðə fʊl ˈdæʃbɔːd nekst ˈwenzdeɪ/", "Thay vào đó, điều chúng tôi làm được là giao phần xuất dữ liệu lõi ngay và hoàn thiện dashboard vào thứ Tư tới.", new[] { "what we can do instead", "core export" })
                }, now),
            MakeEnActivity(c, l, 12, "dictation",
                "Dictation (Bài 5.2): Nghe chép Hạ nhiệt Khủng hoảng & Nhượng bộ",
                "Rèn nghe các mẫu câu hạ nhiệt bức xúc và cập nhật tiến độ sự cố.",
                new EnDiItem[]
                {
                    new("en-l52-dict-e1", "en-US", "easy", "Cam kết cập nhật (Dễ)", "I will keep you posted every twenty minutes.", "/aɪ wɪl kiːp juː ˈpəʊstɪd ˈevri ˈtwenti ˈmɪnɪts/", "Tôi sẽ cập nhật tình hình cho bạn mỗi 20 phút.", new[] { "I", "will", "keep", "you", "posted", "every", "twenty", "minutes" }, new[] { "told", "updated", "hours" }),
                    new("en-l52-dict-e2", "en-US", "easy", "Thừa nhận lo ngại (Dễ)", "Data privacy is a completely valid concern.", "/ˈdeɪtə ˈprɪvəsi ɪz ə kəmˈpliːtli ˈvælɪd kənˈsɜːn/", "Quyền riêng tư dữ liệu là một mối lo ngại hoàn toàn chính đáng.", new[] { "Data", "privacy", "is", "a", "completely", "valid", "concern" }, new[] { "true", "worry", "problem" }),
                    new("en-l52-dict-m1", "en-US", "medium", "Cấu trúc Despite chuẩn (Vừa)", "Despite the unexpected outage, we met our monthly SLA target.", "/dɪˈspaɪt ði ˌʌnɪkˈspektɪd ˈaʊtɪdʒ wiː met ˈaʊə ˈmʌnθli es el eɪ ˈtɑːɡɪt/", "Mặc dù gặp sự cố gián đoạn bất ngờ, chúng tôi vẫn đạt mục tiêu SLA tháng.", new[] { "Despite", "the", "unexpected", "outage", "we", "met", "our", "monthly", "SLA", "target" }, new[] { "Although", "of", "missed" }),
                    new("en-l52-dict-m2", "en-US", "medium", "Từ chối ngoài phạm vi (Vừa)", "Custom billing integration is currently out of scope for phase one.", "/ˈkʌstəm ˈbɪlɪŋ ˌɪntɪˈɡreɪʃn ɪz ˈkʌrəntli aʊt əv skəʊp fɔː feɪz wʌn/", "Tích hợp thanh toán tùy chỉnh hiện nằm ngoài phạm vi của giai đoạn một.", new[] { "Custom", "billing", "integration", "is", "currently", "out", "of", "scope", "for", "phase", "one" }, new[] { "inside", "budget", "step" }),
                    new("en-l52-dict-h1", "en-US", "hard", "Thấu cảm với áp lực khách hàng (Khó)", "I completely understand how critical this report is before your board meeting.", "/aɪ kəmˈpliːtli ˌʌndəˈstænd haʊ ˈkrɪtɪkl ðɪs rɪˈpɔːt ɪz bɪˈfɔː jɔː bɔːd ˈmiːtɪŋ/", "Tôi hoàn toàn hiểu báo cáo này quan trọng thế nào trước cuộc họp hội đồng quản trị của bạn.", new[] { "I", "completely", "understand", "how", "critical", "this", "report", "is", "before", "your", "board", "meeting" }, new[] { "important", "after", "call" }),
                    new("en-l52-dict-h2", "en-US", "hard", "Hedging phản biện mềm mỏng (Khó)", "There is a risk that this approach might face scalability issues later.", "/ðeər ɪz ə rɪsk ðæt ðɪs əˈprəʊtʃ maɪt feɪs ˌskeɪləˈbɪləti ˈɪʃuːz ˈleɪtə/", "Có rủi ro là hướng tiếp cận này có thể gặp vấn đề mở rộng quy mô về sau.", new[] { "There", "is", "a", "risk", "that", "this", "approach", "might", "face", "scalability", "issues", "later" }, new[] { "will", "certainly", "problems" })
                }, now),
            MakeEnActivity(c, l, 13, "pronunciation",
                "Phát âm IPA (Bài 5.2): Khủng hoảng & Ngoại giao Phản biện",
                "Luyện phát âm chuẩn disruption, workaround, sincerely, priority, despite.",
                new EnPrItem[]
                {
                    new("en-l52-pron-e1", "en-US", "easy", "Âm /s/ và /ɪə/ trong sincerely (Dễ)", "We sincerely apologize for the sudden delay.", "/wiː sɪnˈsɪəli əˈpɒlədʒaɪz fɔː ðə ˈsʌdn dɪˈleɪ/", "Chúng tôi chân thành xin lỗi vì sự chậm trễ đột ngột này.", new[] { "sincerely", "apologize", "sudden" }, new[] { "sincerely apologize" }),
                    new("en-l52-pron-e2", "en-US", "easy", "Âm /w/ trong workaround (Dễ)", "Here is a temporary workaround for your team.", "/hɪər ɪz ə ˈtemprəri ˈwɜːkəraʊnd fɔː jɔː tiːm/", "Đây là giải pháp tạm thời dành cho đội ngũ của bạn.", new[] { "workaround", "temporary", "team" }, new[] { "temporary workaround" }),
                    new("en-l52-pron-m1", "en-US", "medium", "Âm /ʃ/ trong disruption (Vừa)", "Our engineers are minimizing any service disruption.", "/ˈaʊər ˌendʒɪˈnɪəz ɑː ˈmɪnɪmaɪzɪŋ ˈeni ˈsɜːvɪs dɪsˈrʌpʃn/", "Các kỹ sư của chúng tôi đang giảm thiểu mọi gián đoạn dịch vụ.", new[] { "disruption", "minimizing", "service" }, new[] { "service disruption" }),
                    new("en-l52-pron-m2", "en-US", "medium", "Trọng âm priority (Vừa)", "Restoring full access remains our top priority.", "/rɪˈstɔːrɪŋ fʊl ˈækses rɪˈmeɪnz ˈaʊə tɒp praɪˈɒrəti/", "Khôi phục quyền truy cập đầy đủ vẫn là ưu tiên hàng đầu của chúng tôi.", new[] { "priority", "restoring", "access" }, new[] { "top priority" }),
                    new("en-l52-pron-h1", "en-US", "hard", "Âm /d/ và /v/ trong devil's advocate (Khó)", "Let me play devil's advocate on this security assumption.", "/let miː pleɪ ˈdevlz ˈædvəkət ɒn ðɪs sɪˈkjʊərəti əˈsʌmpʃn/", "Hãy để tôi đóng vai phản biện trái chiều đối với giả định bảo mật này.", new[] { "devil's", "advocate", "assumption" }, new[] { "devil's advocate" }),
                    new("en-l52-pron-h2", "en-US", "hard", "Nhịp điệu nhượng bộ với despite (Khó)", "Despite unprecedented holiday traffic, our infrastructure remained completely resilient.", "/dɪˈspaɪt ʌnˈpresɪdentɪd ˈhɒlɪdeɪ ˈtræfɪk ˈaʊər ˈɪnfrəstrʌktʃə rɪˈmeɪnd kəmˈpliːtli rɪˈzɪliənt/", "Mặc cho lưu lượng dịp lễ cao chưa từng có, hạ tầng của chúng tôi vẫn vững vàng hoàn toàn.", new[] { "unprecedented", "infrastructure", "resilient" }, new[] { "completely resilient" })
                }, now),
            MakeEnActivity(c, l, 14, "word_map",
                "Word Map (Bài 5.2): Từ vựng Xử lý Khủng hoảng & Phản biện",
                "Hệ thống từ vựng giao tiếp khéo léo trong khủng hoảng và đàm phán phạm vi.",
                new EnWmItem[]
                {
                    new("wm-l52-e1", "en-US", "easy", "Sơ đồ từ vựng: apologize", "apologize", "/əˈpɒlədʒaɪz/", "Xin lỗi một cách chuyên nghiệp",
                        new[] { new EnWmNode("sincerely apologize for", "/sɪnˈsɪəli əˈpɒlədʒaɪz fɔː/", "Chân thành xin lỗi về", "We sincerely apologize for the export delay."), new EnWmNode("apologize for the inconvenience", "/əˈpɒlədʒaɪz fɔː ði ˌɪnkənˈviːniəns/", "Xin lỗi vì sự bất tiện", "Please accept our apologies for the inconvenience.") },
                        new[] { new EnWmNode("make up for", "/meɪk ʌp fɔː/", "Bù đắp lại cho", "We offered service credits to make up for the outage."), new EnWmNode("follow up with", "/ˈfɒləʊ ʌp wɪð/", "Theo sát phản hồi tiếp", "I'll follow up with an incident report.") },
                        new[] { new EnWmNode("regret", "/rɪˈɡret/", "Lấy làm tiếc", "We deeply regret this service interruption."), new EnWmNode("acknowledge", "/əkˈnɒlɪdʒ/", "Thừa nhận ghi nhận", "Always acknowledge customer frustration first.") },
                        new[] { new EnWmEx("We sincerely apologize for the disruption and are working around the clock to restore service.", "Chúng tôi chân thành xin lỗi vì sự gián đoạn và đang nỗ lực hết mình khôi phục dịch vụ.") }),
                    new("wm-l52-e2", "en-US", "easy", "Sơ đồ từ vựng: priority", "priority", "/praɪˈɒrəti/", "Mức ưu tiên hàng đầu",
                        new[] { new EnWmNode("top priority", "/tɒp praɪˈɒrəti/", "Ưu tiên số một", "Your account stability is our top priority."), new EnWmNode("give priority to", "/ɡɪv praɪˈɒrəti tuː/", "Dành ưu tiên cho", "Give priority to blocking production bugs.") },
                        new[] { new EnWmNode("put aside", "/pʊt əˈsaɪd/", "Gác lại tạm thời", "Put minor UI tweaks aside during an outage."), new EnWmNode("focus on", "/ˈfəʊkəs ɒn/", "Tập trung toàn lực vào", "Focus on immediate mitigation first.") },
                        new[] { new EnWmNode("urgency", "/ˈɜːdʒənsi/", "Tính cấp bách", "We treat security alerts with utmost urgency."), new EnWmNode("precedence", "/ˈpresɪdəns/", "Quyền ưu tiên trước", "Customer data safety takes precedence.") },
                        new[] { new EnWmEx("Restoring checkout availability has been escalated to our highest engineering priority.", "Khôi phục tính năng thanh toán đã được nâng lên mức ưu tiên kỹ thuật cao nhất.") }),
                    new("wm-l52-m1", "en-US", "medium", "Sơ đồ từ vựng: workaround", "workaround", "/ˈwɜːkəraʊnd/", "Giải pháp khắc phục tạm thời",
                        new[] { new EnWmNode("temporary workaround", "/ˈtemprəri ˈwɜːkəraʊnd/", "Giải pháp tạm thời", "Here is a temporary workaround until tomorrow's release."), new EnWmNode("reliable workaround", "/rɪˈlaɪəbl ˈwɜːkəraʊnd/", "Phương án thay thế tin cậy", "Support shared a reliable workaround via CSV.") },
                        new[] { new EnWmNode("get around", "/ɡet əˈraʊnd/", "Đi vòng vượt qua lỗi", "Batching smaller files gets around the upload limit."), new EnWmNode("roll out", "/rəʊl aʊt/", "Phát hành bản sửa", "We'll roll out the permanent patch tonight.") },
                        new[] { new EnWmNode("stopgap", "/ˈstɒpɡæp/", "Biện pháp chữa cháy tạm", "Manual exports served as a quick stopgap."), new EnWmNode("alternative", "/ɔːlˈtɜːnətɪv/", "Phương án thay thế", "Offer an immediate alternative when saying no.") },
                        new[] { new EnWmEx("Providing a 15-minute manual workaround saved the client's board presentation.", "Cung cấp giải pháp tạm trong 15 phút đã cứu kịp buổi họp hội đồng của khách hàng.") }),
                    new("wm-l52-m2", "en-US", "medium", "Sơ đồ từ vựng: scope", "scope", "/skəʊp/", "Phạm vi dự án / thỏa thuận",
                        new[] { new EnWmNode("out of scope", "/aʊt əv skəʊp/", "Ngoài phạm vi cam kết", "Mobile offline mode is out of scope for v1."), new EnWmNode("scope creep", "/skəʊp kriːp/", "Phình to phạm vi không kiểm soát", "Clear milestones prevent costly scope creep.") },
                        new[] { new EnWmNode("push back on", "/pʊʃ bæk ɒn/", "Từ chối khéo yêu cầu thêm", "Politely push back on unbudgeted requests."), new EnWmNode("defer to", "/dɪˈfɜː tuː/", "Dời sang giai đoạn sau", "Let's defer custom themes to Phase 2.") },
                        new[] { new EnWmNode("boundary", "/ˈbaʊndri/", "Ranh giới giới hạn", "Healthy boundaries protect delivery quality."), new EnWmNode("remit", "/ˈriːmɪt/", "Phạm vi trách nhiệm", "Billing rules fall outside this team's remit.") },
                        new[] { new EnWmEx("When a feature is out of scope, propose moving it into a dedicated Phase 2 sprint.", "Khi một tính năng nằm ngoài phạm vi, hãy đề xuất chuyển nó sang sprint Giai đoạn 2.") }),
                    new("wm-l52-h1", "en-US", "hard", "Sơ đồ từ vựng: disruption", "disruption", "/dɪsˈrʌpʃn/", "Sự gián đoạn dịch vụ",
                        new[] { new EnWmNode("service disruption", "/ˈsɜːvɪs dɪsˈrʌpʃn/", "Gián đoạn dịch vụ", "Zero-downtime deploys avoid service disruption."), new EnWmNode("minimize disruption", "/ˈmɪnɪmaɪz dɪsˈrʌpʃn/", "Giảm thiểu xáo trộn", "Schedule migrations at 2 AM to minimize disruption.") },
                        new[] { new EnWmNode("recover from", "/rɪˈkʌvə frəm/", "Hồi phục sau sự cố", "Redundant nodes recover from outages quickly."), new EnWmNode("guard against", "/ɡɑːd əˈɡenst/", "Phòng chống trước", "Circuit breakers guard against cascading failures.") },
                        new[] { new EnWmNode("interruption", "/ˌɪntəˈrʌpʃn/", "Sự ngắt quãng", "We regret the brief network interruption."), new EnWmNode("outage", "/ˈaʊtɪdʒ/", "Sự cố sập hệ thống", "Post-mortems follow every major outage.") },
                        new[] { new EnWmEx("Transparent communication every twenty minutes calms clients during a service disruption.", "Cập nhật minh bạch mỗi 20 phút giúp trấn an khách hàng khi dịch vụ bị gián đoạn.") }),
                    new("wm-l52-h2", "en-US", "hard", "Sơ đồ từ vựng: concern", "concern", "/kənˈsɜːn/", "Mối quan ngại / lo lắng",
                        new[] { new EnWmNode("valid concern", "/ˈvælɪd kənˈsɜːn/", "Mối lo ngại chính đáng", "Latency during peak hours is a valid concern."), new EnWmNode("address a concern", "/əˈdres ə kənˈsɜːn/", "Giải quyết thỏa đáng nỗi lo", "We addressed security concerns with SOC2 audits.") },
                        new[] { new EnWmNode("ease off", "/iːz ɒf/", "Làm dịu xuống", "Clear SLAs ease client concerns."), new EnWmNode("talk through", "/tɔːk θruː/", "Trao đổi kỹ từng điểm", "Let's talk through your compliance concerns.") },
                        new[] { new EnWmNode("reservation", "/ˌrezəˈveɪʃn/", "Sự e ngại dè dặt", "Legal voiced minor reservations on liability."), new EnWmNode("apprehension", "/ˌæprɪˈhenʃn/", "Sự lo âu", "Honest timelines reduce stakeholder apprehension.") },
                        new[] { new EnWmEx("Validating a client's concern before explaining constraints disarms hostility immediately.", "Công nhận mối lo ngại của khách trước khi giải thích giới hạn sẽ hóa giải căng thẳng ngay.") })
                }, now),
            MakeEnActivity(c, l, 15, "cloze",
                "Điền từ ngữ cảnh (Bài 5.2): Hạ nhiệt Khủng hoảng & Positive No",
                "Hoàn thành các câu ứng xử chuyên nghiệp trong khủng hoảng và từ chối khéo.",
                new EnClItem[]
                {
                    new("en-l52-cloze-1", "easy", "Xin lỗi & Xoa dịu ban đầu (Dễ)", "Điền từ xin lỗi và sự gián đoạn:", "I hear you, and I sincerely {{0}} for the {{1}} this caused your team.", new[] { new EnClBlank(new[] { "apologize" }, "xin lỗi trang trọng", "'sincerely apologize for...'."), new EnClBlank(new[] { "disruption", "inconvenience", "delay" }, "sự gián đoạn / bất tiện", "'disruption' rất phù hợp khi hệ thống lỗi.") }, "I hear you, and I sincerely apologize for the disruption this caused your team.", "Tôi lắng nghe bạn, và tôi chân thành xin lỗi vì sự gián đoạn đã gây ra cho đội của bạn.", "Bước Acknowledge + Empathize giúp hạ nhiệt cảm xúc đối phương."),
                    new("en-l52-cloze-2", "easy", "Giải pháp tạm thời trong lúc chờ vá (Dễ)", "Điền danh từ giải pháp tạm:", "While we deploy the permanent fix, here is a reliable {{0}} so your team can {{1}}.", new[] { new EnClBlank(new[] { "workaround", "alternative" }, "giải pháp thay thế tạm", "'workaround' = giải pháp tạm thời."), new EnClBlank(new[] { "continue", "proceed", "operate" }, "tiếp tục làm việc", "Giúp khách hàng không bị kẹt chờ.") }, "While we deploy the permanent fix, here is a reliable workaround so your team can continue.", "Trong lúc chúng tôi triển khai bản vá chính thức, đây là giải pháp tạm tin cậy để đội bạn làm việc tiếp.", "Luôn trao giải pháp thay thế ngay lập tức."),
                    new("en-l52-cloze-3", "medium", "Cập nhật tiến độ liên tục (Vừa)", "Điền cụm từ 'cập nhật thường xuyên':", "I will {{0}} you {{1}} every twenty minutes until service is fully restored.", new[] { new EnClBlank(new[] { "keep" }, "giữ liên lạc (___ someone posted)", "'keep someone posted'."), new EnClBlank(new[] { "posted", "updated", "informed" }, "cập nhật thông tin", "'keep you posted' là cụm chuẩn bản xứ.") }, "I will keep you posted every twenty minutes until service is fully restored.", "Tôi sẽ cập nhật cho bạn mỗi 20 phút cho tới khi dịch vụ hồi phục hoàn toàn.", "Chủ động hẹn nhịp cập nhật giúp khách hàng yên tâm."),
                    new("en-l52-cloze-4", "medium", "Từ chối ngoài phạm vi (Vừa)", "Điền cụm nằm ngoài phạm vi và đề xuất thay thế:", "That custom feature is out of {{0}} for Friday; what we can do {{1}} is ship it next week.", new[] { new EnClBlank(new[] { "scope" }, "phạm vi (out of ___)", "'out of scope' = ngoài phạm vi."), new EnClBlank(new[] { "instead" }, "thay vào đó (what we can do ___)", "Cấu trúc Positive No: 'What we can do instead is...'") }, "That custom feature is out of scope for Friday; what we can do instead is ship it next week.", "Tính năng tùy chỉnh đó nằm ngoài phạm vi cho thứ Sáu; thay vào đó chúng tôi có thể giao vào tuần tới.", "Kết thúc lời từ chối bằng phương án khả thi."),
                    new("en-l52-cloze-5", "hard", "Mệnh đề nhượng bộ không có 'of' (Khó)", "Điền liên từ nhượng bộ trước cụm danh từ:", "{{0}} the unexpected traffic spike, we {{1}} our monthly SLA target.", new[] { new EnClBlank(new[] { "Despite" }, "mặc dù + cụm danh từ (không có of)", "Dùng 'Despite' (viết hoa đầu câu)."), new EnClBlank(new[] { "met", "achieved", "hit" }, "đạt được mục tiêu (___ target)", "'meet a target' = đạt chỉ tiêu.") }, "Despite the unexpected traffic spike, we met our monthly SLA target.", "Mặc dù lưu lượng tăng đột biến ngoài dự kiến, chúng tôi vẫn đạt mục tiêu SLA tháng.", "Ghi nhớ: KHÔNG BAO GIỜ viết 'Despite of'.")
                }, now),
            MakeEnActivity(c, l, 16, "translate",
                "Dịch phản xạ Việt → Anh (Bài 5.2): Xử lý Khủng hoảng & Từ chối",
                "Chuyển các câu ứng xử tình huống khó sang tiếng Anh chuẩn mực.",
                new EnTrItem[]
                {
                    new("en-l52-trans-1", "easy", "Thừa nhận sự cấp bách (Dễ)", "Tôi hoàn toàn hiểu báo cáo này quan trọng thế nào trước cuộc họp của bạn.", new[] { "completely understand", "how critical", "before your meeting" }, new[] { "I completely understand how critical this report is before your meeting.", "I totally understand how important this report is prior to your meeting." }, "I completely understand how critical this report is before your meeting.", "Không đảo 'is' lên trước 'this report' trong cụm 'how critical this report is'."),
                    new("en-l52-trans-2", "easy", "Khẳng định đang xử lý ưu tiên (Dễ)", "Các kỹ sư cấp cao của chúng tôi đang điều tra nguyên nhân gốc rễ như ưu tiên hàng đầu.", new[] { "senior engineers", "investigating", "root cause", "top priority" }, new[] { "Our senior engineers are investigating the root cause as our top priority.", "Our senior engineers are actively investigating the root cause as a top priority." }, "Our senior engineers are investigating the root cause as our top priority.", "Dùng Hiện tại tiếp diễn 'are investigating' tạo cảm giác hành động tức thì."),
                    new("en-l52-trans-3", "medium", "Đề xuất phương án thay thế (Positive No) (Vừa)", "Chúng tôi không thể bàn giao toàn bộ bảng điều khiển vào thứ Sáu, nhưng thay vào đó chúng tôi có thể gửi file xuất CSV cốt lõi.", new[] { "cannot deliver", "what we can do instead", "core csv export" }, new[] { "We cannot deliver the full dashboard by Friday, though what we can do instead is send the core CSV export.", "While we cannot ship the full dashboard on Friday, we can deliver the core CSV export instead." }, "We cannot deliver the full dashboard by Friday, though what we can do instead is send the core CSV export.", "Cấu trúc 'What we can do instead is + V-inf' cực kỳ khéo léo."),
                    new("en-l52-trans-4", "medium", "Phản biện trái chiều nhẹ nhàng (Vừa)", "Đó là một mối lo ngại chính đáng; tuy nhiên, chúng ta cần cân nhắc chi phí bảo trì dài hạn.", new[] { "valid concern", "however", "long-term maintenance cost" }, new[] { "That is a valid concern; however, we need to consider the long-term maintenance cost.", "That's a valid concern; however, we must take long-term maintenance costs into account." }, "That is a valid concern; however, we need to consider the long-term maintenance cost.", "Thừa nhận ý kiến trước khi đưa ra luận điểm phản biện."),
                    new("en-l52-trans-5", "hard", "Cam kết cập nhật trong khủng hoảng (Khó)", "Mặc dù gặp sự cố gián đoạn bất ngờ, tôi sẽ cập nhật cho bạn mỗi 20 phút cho tới khi hệ thống ổn định hoàn toàn.", new[] { "despite", "unexpected outage", "keep you posted", "fully restored" }, new[] { "Despite the unexpected outage, I will keep you posted every twenty minutes until service is fully restored.", "Despite the unexpected disruption, I'll keep you updated every 20 minutes until the system is completely stable." }, "Despite the unexpected outage, I will keep you posted every twenty minutes until service is fully restored.", "'Despite + Noun phrase' + 'keep you posted'.")
                }, now),
            MakeEnActivity(c, l, 17, "grammar_fix",
                "Sửa lỗi ngữ pháp (Bài 5.2): Nhượng bộ, Giảm nhẹ & Từ vựng Khủng hoảng",
                "Sửa các lỗi sai phổ biến khi dùng Despite, Although, Hedging và phản biện.",
                new EnGfItem[]
                {
                    new("en-l52-gf-1", "easy", "Lỗi kinh điển 'Despite of' (Dễ)", "Despite of the system outage, our support team stayed calm.", "Không bao giờ dùng 'of' sau 'Despite' (chỉ dùng 'Despite' hoặc 'In spite of').", "Despite the system outage, our support team stayed calm.", new[] { "Despite the system outage, our support team stayed calm.", "In spite of the system outage, our support team stayed calm." }, "Mặc dù hệ thống bị gián đoạn, đội hỗ trợ của chúng tôi vẫn giữ bình tĩnh.", "Xóa 'of' ngay sau 'Despite'."),
                    new("en-l52-gf-2", "easy", "Dùng 'Although' trước cụm danh từ (Dễ)", "Although the tight deadline, we refused to skip security tests.", "'Although' phải đi với mệnh đề (S + V); trước cụm danh từ phải dùng 'Despite'.", "Despite the tight deadline, we refused to skip security tests.", new[] { "Despite the tight deadline, we refused to skip security tests.", "Although the deadline was tight, we refused to skip security tests." }, "Mặc dù hạn chót rất gấp, chúng tôi từ chối bỏ qua kiểm thử bảo mật.", "Before Noun phrase -> Despite; Before S + V -> Although."),
                    new("en-l52-gf-3", "medium", "Dùng 'Although ... but' lặp liên từ (Vừa)", "Although I see your point, but this approach adds too much risk.", "Tiếng Anh KHÔNG BAO GIỜ dùng cả 'Although' và 'but' trong cùng một câu!", "Although I see your point, this approach adds too much risk.", new[] { "Although I see your point, this approach adds too much risk.", "I see your point, but this approach adds too much risk." }, "Mặc dù tôi hiểu quan điểm của bạn, hướng tiếp cận này gây ra quá nhiều rủi ro.", "Chọn 1 trong 2: hoặc 'Although A, B' hoặc 'A, but B'."),
                    new("en-l52-gf-4", "medium", "Đảo ngữ trong cụm 'how critical' (Vừa)", "I understand how critical is this export for your meeting.", "Trong mệnh đề danh từ gián tiếp, giữ nguyên trật tự S + V ('this export is'), không đảo 'is'.", "I understand how critical this export is for your meeting.", new[] { "I understand how critical this export is for your meeting." }, "Tôi hiểu việc xuất dữ liệu này quan trọng thế nào cho cuộc họp của bạn.", "Trật tự đúng: 'how + adj + S + V'."),
                    new("en-l52-gf-5", "hard", "Sắc thái Hedging thay cho ra lệnh (Khó)", "Your plan will definitely fail if traffic grows three times.", "Trong phản biện chuyên nghiệp, thay khẳng định gay gắt bằng cấu trúc giảm nhẹ 'might face challenges / could struggle'.", "This plan might face scalability challenges if traffic triples.", new[] { "This plan might face scalability challenges if traffic triples.", "There is a risk that this plan could struggle if traffic triples." }, "Kế hoạch này có thể gặp thách thức về mở rộng nếu lưu lượng tăng gấp ba.", "Dùng 'might/could' và tập trung vào 'the plan' thay vì 'you'.")
                }, now),
            MakeEnActivity(c, l, 18, "roleplay",
                "Nhập vai hội thoại (Bài 5.2): Xoa dịu Khách hàng & Phản biện",
                "Thực hành xử lý khách hàng bức xúc và nói lời từ chối khéo léo.",
                new EnRpItem[]
                {
                    new("en-l52-rp-1", "easy", "Hạ nhiệt khách hàng đang tức giận (Dễ)", "Khách hàng doanh nghiệp gọi báo tính năng xuất báo cáo bị lỗi ngay trước giờ họp.", "Marcus (Khách hàng)", "This is unacceptable! Our finance team cannot export monthly reports, and our board meeting is in two hours!", "Thật không thể chấp nhận được! Đội tài chính của tôi không xuất được báo cáo tháng, mà 2 tiếng nữa họp hội đồng rồi!", "Lắng nghe, công nhận mức độ cấp bách, xin lỗi chân thành vì gián đoạn và báo đang xử lý ưu tiên cao nhất.", new[] { "understand", "apologize", "disruption", "priority" }, new[] { "Marcus, I completely understand how critical this is before your board meeting. I sincerely apologize for the disruption—our senior engineers are treating this as our top priority." }, 14),
                    new("en-l52-rp-2", "medium", "Đưa giải pháp tạm (Workaround) & Cam kết cập nhật (Vừa)", "Khách hàng không thể đợi tới ngày mai để sửa xong bản vá.", "Marcus (Khách hàng)", "We cannot wait until tomorrow for a deployment! What can you give us right now?", "Chúng tôi không thể đợi tới ngày mai để phát hành bản vá! Ngay bây giờ các bạn làm được gì?", "Đề xuất xuất báo cáo PDF thủ công gửi trong 15 phút (workaround) và cam kết cập nhật mỗi 20 phút.", new[] { "workaround", "fifteen minutes", "keep you posted", "permanent" }, new[] { "While our team deploys the permanent fix, I can provide a workaround by generating your PDF reports manually within fifteen minutes and will keep you posted every twenty minutes." }, 16),
                    new("en-l52-rp-3", "medium", "Từ chối yêu cầu thêm ngoài phạm vi (Positive No) (Vừa)", "Khách hàng đòi làm miễn phí thêm trang dashboard tùy chỉnh vào thứ Sáu này.", "Marcus (Khách hàng)", "Can you also build us a custom analytics dashboard by Friday at no extra cost?", "Các bạn làm thêm luôn cho chúng tôi trang dashboard phân tích tùy chỉnh trước thứ Sáu mà không tính phí được không?", "Dùng công thức Positive No: đồng cảm nhu cầu, nêu giới hạn out of scope/quality, đề xuất phương án thay thế khả thi.", new[] { "understand", "however", "out of scope", "what we can do instead" }, new[] { "I understand how useful that dashboard would be; however, custom analytics is currently out of scope for Friday. What we can do instead is deliver the CSV export now and schedule the dashboard for Phase 2." }, 18),
                    new("en-l52-rp-4", "hard", "Phản biện đa chiều trong cuộc họp kỹ thuật (Khó)", "Quản lý đề xuất bỏ qua bước kiểm thử tự động để ra mắt sớm 3 ngày.", "Marcus (Stakeholder)", "Let's skip automated testing this sprint so we can ship three days earlier!", "Hãy bỏ qua kiểm thử tự động ở sprint này để chúng ta ra mắt sớm hơn 3 ngày đi!", "Thừa nhận mong muốn ra mắt sớm, nhưng phản biện nhẹ nhàng về rủi ro lỗi tái phát (regressions) và đề xuất giảm phạm vi thay vì bỏ test.", new[] { "see your point", "however", "regressions", "risk", "instead" }, new[] { "I see your point about hitting the launch date early; however, skipping automated tests introduces a high risk of production regressions. What if we trim one non-critical feature instead so quality stays intact?" }, 16)
                }, now)
        };
    }

    private static List<PracticeActivity> BuildEn53Activities(Guid c, DateTimeOffset now)
    {
        var l = EnLesson53Id;
        return new()
        {
            MakeEnActivity(c, l, 11, "shadowing",
                "Shadowing (Bài 5.3): Email Hiện đại & Prompt Engineering Tự nhiên",
                "Luyện nói tự nhiên thoát khỏi văn mẫu máy móc trong email và giao tiếp hàng ngày.",
                new EnShItem[]
                {
                    new("en-l53-shad-e1", "en-US", "easy", "Mở đầu báo trước thân thiện (Dễ)", "Just a quick heads-up that staging will restart in ten minutes.", "/dʒʌst ə kwɪk ˈhedz ʌp ðæt ˈsteɪdʒɪŋ wɪl ˌriːˈstɑːt ɪn ten ˈmɪnɪts/", "Báo trước nhanh là môi trường staging sẽ khởi động lại sau 10 phút.", new[] { "quick heads-up", "in ten minutes" }),
                    new("en-l53-shad-e2", "en-US", "easy", "Báo hoàn tất công việc tự nhiên (Dễ)", "Quick update: we wrapped up the integration tests yesterday.", "/kwɪk ˈʌpdeɪt wiː ræpt ʌp ði ˌɪntɪˈɡreɪʃn tests ˈjestədeɪ/", "Cập nhật nhanh: hôm qua chúng tôi đã hoàn tất các bài kiểm thử tích hợp.", new[] { "quick update", "wrapped up" }),
                    new("en-l53-shad-m1", "en-US", "medium", "Nhờ phản hồi lịch sự thoát 'kindly revert' (Vừa)", "Could you take a look and share your thoughts by Thursday afternoon?", "/kəd juː teɪk ə lʊk ænd ʃeə jɔː θɔːts baɪ ˈθɜːzdeɪ ˌɑːftəˈnuːn/", "Bạn có thể xem qua và chia sẻ ý kiến trước chiều thứ Năm không?", new[] { "take a look", "share your thoughts" }),
                    new("en-l53-shad-m2", "en-US", "medium", "Cảm ơn tự tin thay 'sorry for bothering' (Vừa)", "I really appreciate your help with clarifying the client requirements.", "/aɪ ˈrɪəli əˈpriːʃieɪt jɔː help wɪð ˈklærəfaɪɪŋ ðə ˈklaɪənt rɪˈkwaɪəmənts/", "Tôi thực sự trân trọng sự giúp đỡ của bạn trong việc làm rõ yêu cầu khách hàng.", new[] { "really appreciate", "clarifying requirements" }),
                    new("en-l53-shad-h1", "en-US", "hard", "Viết Prompt AI theo Role & Task (Khó)", "Act as a senior database engineer and identify missing indexes in the attached SQL query.", "/ækt æz ə ˈsiːniə ˈdeɪtəbeɪs ˌendʒɪˈnɪər ænd aɪˈdentɪfaɪ ˈmɪsɪŋ ˈɪndeksɪz ɪn ði əˈtætʃt es kjuː el ˈkwɪəri/", "Đóng vai kỹ sư CSDL cấp cao và chỉ ra các chỉ mục còn thiếu trong truy vấn SQL đính kèm.", new[] { "act as", "identify missing indexes" }),
                    new("en-l53-shad-h2", "en-US", "hard", "Ràng buộc & Định dạng đầu ra Prompt (Khó)", "Keep backward compatibility and summarize your recommendations in a concise checklist.", "/kiːp ˈbækwəd kəmˌpætəˈbɪləti ænd ˈsʌməraɪz jɔː ˌrekəmenˈdeɪʃnz ɪn ə kənˈsaɪs ˈtʃeklɪst/", "Giữ tương thích ngược và tóm tắt các khuyến nghị thành danh sách kiểm tra súc tích.", new[] { "backward compatibility", "concise checklist" })
                }, now),
            MakeEnActivity(c, l, 12, "dictation",
                "Dictation (Bài 5.3): Nghe chép Email Tự nhiên & Prompt AI",
                "Nghe và chép lại các mẫu câu email súc tích chuẩn công sở quốc tế.",
                new EnDiItem[]
                {
                    new("en-l53-dict-e1", "en-US", "easy", "Hỏi nhanh lịch sự (Dễ)", "When you have a moment, could you review this draft?", "/wen juː hæv ə ˈməʊmənt kəd juː rɪˈvjuː ðɪs drɑːft/", "Khi nào rảnh một chút, bạn xem giúp bản nháp này nhé?", new[] { "When", "you", "have", "a", "moment", "could", "you", "review", "this", "draft" }, new[] { "sorry", "revert", "check" }),
                    new("en-l53-dict-e2", "en-US", "easy", "Nhắc lại email cũ (Dễ)", "I am following up on my email regarding API access.", "/aɪ æm ˈfɒləʊɪŋ ʌp ɒn maɪ ˈiːmeɪl rɪˈɡɑːdɪŋ eɪ piː aɪ ˈækses/", "Tôi nhắn tiếp theo email trước của mình về quyền truy cập API.", new[] { "I", "am", "following", "up", "on", "my", "email", "regarding", "API", "access" }, new[] { "looking", "about", "checking" }),
                    new("en-l53-dict-m1", "en-US", "medium", "Yêu cầu đầu ra thiết thực (Vừa)", "Please give me three actionable steps to reduce memory leaks.", "/pliːz ɡɪv miː θriː ˈækʃənəbl steps tuː rɪˈdjuːs ˈmeməri liːks/", "Hãy đưa cho tôi ba bước hành động thiết thực để giảm rò rỉ bộ nhớ.", new[] { "Please", "give", "me", "three", "actionable", "steps", "to", "reduce", "memory", "leaks" }, new[] { "useful", "ways", "stop" }),
                    new("en-l53-dict-m2", "en-US", "medium", "Thảo luận trực tiếp không dùng about (Vừa)", "Let us discuss these prompt constraints in our afternoon sync.", "/let ʌs dɪˈskʌs ðiːz prɒmpt kənˈstreɪnts ɪn ˈaʊər ˌɑːftəˈnuːn sɪŋk/", "Hãy cùng thảo luận các ràng buộc prompt này trong buổi họp chiều nay.", new[] { "Let", "us", "discuss", "these", "prompt", "constraints", "in", "our", "afternoon", "sync" }, new[] { "about", "talk", "limits" }),
                    new("en-l53-dict-h1", "en-US", "hard", "Đính kèm tóm tắt để chốt lịch phát hành (Khó)", "I have attached the test summary so we can proceed with Friday's release.", "/aɪ hæv əˈtætʃt ðə test ˈsʌməri səʊ wiː kæn prəˈsiːd wɪð ˈfraɪdeɪz rɪˈliːs/", "Tôi đã đính kèm bản tóm tắt kiểm thử để chúng ta tiến hành phát hành vào thứ Sáu.", new[] { "I", "have", "attached", "the", "test", "summary", "so", "we", "can", "proceed", "with", "Friday's", "release" }, new[] { "enclosed", "report", "continue" }),
                    new("en-l53-dict-h2", "en-US", "hard", "Viết Prompt súc tích theo ngữ cảnh (Khó)", "Provide a prioritized Markdown table comparing before and after SQL execution plans.", "/prəˈvaɪd ə praɪˈɒrɪtaɪzd ˈmɑːkdaʊn ˈteɪbl kəmˈpeərɪŋ bɪˈfɔːr ænd ˈɑːftər es kjuː el ˌeksɪˈkjuːʃn plænz/", "Hãy cung cấp bảng Markdown theo thứ tự ưu tiên so sánh kế hoạch thực thi SQL trước và sau.", new[] { "Provide", "a", "prioritized", "Markdown", "table", "comparing", "before", "and", "after", "SQL", "execution", "plans" }, new[] { "Make", "list", "between" })
                }, now),
            MakeEnActivity(c, l, 13, "pronunciation",
                "Phát âm IPA (Bài 5.3): Ngữ điệu Tự nhiên & Thuật ngữ AI/Email",
                "Luyện đọc trôi chảy các từ concise, actionable, clarify, appreciate, wrap up.",
                new EnPrItem[]
                {
                    new("en-l53-pron-e1", "en-US", "easy", "Nối âm trong wrap up (Dễ)", "Let us wrap up the testing report before five.", "/let ʌs ræp ʌp ðə ˈtestɪŋ rɪˈpɔːt bɪˈfɔː faɪv/", "Chúng ta hãy hoàn tất báo cáo kiểm thử trước 5 giờ nhé.", new[] { "wrap", "report", "before" }, new[] { "wrap up" }),
                    new("en-l53-pron-e2", "en-US", "easy", "Âm /kənˈsaɪs/ trong concise (Dễ)", "Keep every status email concise and easy to scan.", "/kiːp ˈevri ˈsteɪtəs ˈiːmeɪl kənˈsaɪs ænd ˈiːzi tuː skæn/", "Hãy giữ mọi email cập nhật súc tích và dễ đọc lướt.", new[] { "concise", "status", "scan" }, new[] { "concise email" }),
                    new("en-l53-pron-m1", "en-US", "medium", "Trọng âm appreciate (Vừa)", "I really appreciate your prompt feedback on this pull request.", "/aɪ ˈrɪəli əˈpriːʃieɪt jɔː prɒmpt ˈfiːdbæk ɒn ðɪs pʊl rɪˈkwest/", "Tôi thực sự trân trọng phản hồi nhanh chóng của bạn cho pull request này.", new[] { "appreciate", "prompt", "feedback" }, new[] { "really appreciate" }),
                    new("en-l53-pron-m2", "en-US", "medium", "Âm /kl/ trong clarify (Vừa)", "Could you clarify the expected JSON output format?", "/kəd juː ˈklærəfaɪ ði ɪkˈspektɪd ˈdʒeɪsɒn ˈaʊtpʊt ˈfɔːmæt/", "Bạn có thể làm rõ định dạng đầu ra JSON mong đợi không?", new[] { "clarify", "expected", "format" }, new[] { "output format" }),
                    new("en-l53-pron-h1", "en-US", "hard", "Trọng âm actionable & prioritized (Khó)", "Generate an actionable checklist prioritized by engineering impact.", "/ˈdʒenəreɪt ən ˈækʃənəbl ˈtʃeklɪst praɪˈɒrɪtaɪzd baɪ ˌendʒɪˈnɪərɪŋ ˈɪmpækt/", "Hãy tạo danh sách kiểm tra thiết thực được xếp ưu tiên theo tác động kỹ thuật.", new[] { "actionable", "prioritized", "impact" }, new[] { "actionable checklist" }),
                    new("en-l53-pron-h2", "en-US", "hard", "Nhịp điệu câu mời hợp tác (Khó)", "Please let me know your thoughts so we can finalize the architecture.", "/pliːz let miː nəʊ jɔː θɔːts səʊ wiː kæn ˈfaɪnəlaɪz ði ˈɑːkɪtektʃə/", "Vui lòng cho tôi biết suy nghĩ của bạn để chúng ta chốt kiến trúc nhé.", new[] { "thoughts", "finalize", "architecture" }, new[] { "finalize the architecture" })
                }, now),
            MakeEnActivity(c, l, 14, "word_map",
                "Word Map (Bài 5.3): Nâng cấp Diễn đạt Email & AI Prompting",
                "Thay thế văn mẫu cũ bằng các cụm động từ và từ vựng hiện đại chuẩn bản xứ.",
                new EnWmItem[]
                {
                    new("wm-l53-e1", "en-US", "easy", "Sơ đồ từ vựng: wrap up", "wrap up", "/ræp ʌp/", "Hoàn tất, chốt lại (công việc/cuộc họp)",
                        new[] { new EnWmNode("wrap up a task", "/ræp ʌp ə tɑːsk/", "Hoàn thành xong đầu việc", "We wrapped up the API migration yesterday."), new EnWmNode("wrap up a meeting", "/ræp ʌp ə ˈmiːtɪŋ/", "Kết thúc buổi họp", "Let's wrap up five minutes early.") },
                        new[] { new EnWmNode("finish up", "/ˈfɪnɪʃ ʌp/", "Làm nốt cho xong", "I'll finish up the docs by noon."), new EnWmNode("sign off", "/saɪn ɒf/", "Chốt duyệt xong", "QA signed off on the build.") },
                        new[] { new EnWmNode("complete", "/kəmˈpliːt/", "Hoàn thành", "We completed testing on schedule."), new EnWmNode("finalize", "/ˈfaɪnəlaɪz/", "Hoàn thiện bước cuối", "Finalize the release notes today.") },
                        new[] { new EnWmEx("Just letting you know we wrapped up QA testing ahead of schedule.", "Báo nhanh bạn biết chúng tôi đã hoàn tất kiểm thử QA sớm hơn dự kiến.") }),
                    new("wm-l53-e2", "en-US", "easy", "Sơ đồ từ vựng: heads-up", "heads-up", "/ˈhedz ʌp/", "Lời báo trước ngắn gọn",
                        new[] { new EnWmNode("a quick heads-up", "/ə kwɪk ˈhedz ʌp/", "Báo trước nhanh", "Just a quick heads-up regarding tomorrow's deploy."), new EnWmNode("give a heads-up", "/ɡɪv ə ˈhedz ʌp/", "Nhắn báo trước cho ai", "Thanks for giving us an early heads-up!") },
                        new[] { new EnWmNode("pass along", "/pɑːs əˈlɒŋ/", "Chuyển tiếp thông tin", "Please pass this heads-up along to QA."), new EnWmNode("flag up", "/flæɡ ʌp/", "Báo cờ cảnh báo sớm", "Flag up blockers early in Slack.") },
                        new[] { new EnWmNode("notice", "/ˈnəʊtɪs/", "Thông báo trước", "Appreciate the advance notice."), new EnWmNode("reminder", "/rɪˈmaɪndə/", "Lời nhắc nhẹ", "Here is a friendly reminder.") },
                        new[] { new EnWmEx("Giving teammates an early heads-up replaces stiff 'Please be informed' emails.", "Nhắn báo trước tự nhiên thay thế hoàn hảo câu mở đầu cứng nhắc 'Please be informed'.") }),
                    new("wm-l53-m1", "en-US", "medium", "Sơ đồ từ vựng: concise", "concise", "/kənˈsaɪs/", "Súc tích, ngắn gọn mà đủ ý",
                        new[] { new EnWmNode("concise summary", "/kənˈsaɪs ˈsʌməri/", "Tóm tắt súc tích", "Write a concise three-bullet summary."), new EnWmNode("clear and concise", "/klɪər ænd kənˈsaɪs/", "Rõ ràng và súc tích", "Good AI prompts are clear and concise.") },
                        new[] { new EnWmNode("trim down", "/trɪm daʊn/", "Cắt gọt bớt cho gọn", "Trim down redundant filler words."), new EnWmNode("boil down to", "/bɔɪl daʊn tuː/", "Cô đọng lại thành", "The prompt boils down to three rules.") },
                        new[] { new EnWmNode("succinct", "/səkˈsɪŋkt/", "Cô đọng", "Keep executive emails succinct."), new EnWmNode("brief", "/briːf/", "Ngắn gọn", "Send a brief Slack update.") },
                        new[] { new EnWmEx("Concise writing respects your reader's time and gets faster replies.", "Cách viết súc tích tôn trọng thời gian người đọc và giúp bạn nhận phản hồi nhanh hơn.") }),
                    new("wm-l53-m2", "en-US", "medium", "Sơ đồ từ vựng: clarify", "clarify", "/ˈklærəfaɪ/", "Làm rõ, giải thích cho sáng tỏ",
                        new[] { new EnWmNode("clarify expectations", "/ˈklærəfaɪ ˌekspekˈteɪʃnz/", "Làm rõ kỳ vọng", "Clarify expectations before writing code."), new EnWmNode("clarify constraints", "/ˈklærəfaɪ kənˈstreɪnts/", "Làm rõ các ràng buộc", "Always clarify prompt constraints explicitly.") },
                        new[] { new EnWmNode("spell out", "/spel aʊt/", "Nêu rõ từng điểm", "Spell out the output format in JSON."), new EnWmNode("clear up", "/klɪər ʌp/", "Gỡ rối hiểu lầm", "A quick call cleared up the confusion.") },
                        new[] { new EnWmNode("specify", "/ˈspesɪfaɪ/", "Chỉ định cụ thể", "Specify edge cases in the prompt."), new EnWmNode("elucidate", "/ɪˈluːsɪdeɪt/", "Làm sáng tỏ", "Examples elucidate ambiguous rules.") },
                        new[] { new EnWmEx("Before prompting an AI assistant, clarify the target role, context, and format.", "Trước khi ra lệnh cho AI, hãy làm rõ vai trò, bối cảnh và định dạng đầu ra.") }),
                    new("wm-l53-h1", "en-US", "hard", "Sơ đồ từ vựng: actionable", "actionable", "/ˈækʃənəbl/", "Thiết thực, có thể hành động ngay",
                        new[] { new EnWmNode("actionable feedback", "/ˈækʃənəbl ˈfiːdbæk/", "Góp ý cụ thể làm được ngay", "Vague praise isn't actionable feedback."), new EnWmNode("actionable checklist", "/ˈækʃənəbl ˈtʃeklɪst/", "Danh sách bước làm cụ thể", "Ask the AI for an actionable remediation checklist.") },
                        new[] { new EnWmNode("act on", "/ækt ɒn/", "Hành động dựa trên", "Teams can act on clear metrics immediately."), new EnWmNode("follow through", "/ˈfɒləʊ θruː/", "Theo làm tới nơi tới chốn", "Follow through on every action item.") },
                        new[] { new EnWmNode("practical", "/ˈpræktɪkl/", "Thực tế áp dụng được", "Focus on practical solutions."), new EnWmNode("concrete", "/ˈkɒŋkriːt/", "Cụ thể rõ ràng", "Give concrete code examples.") },
                        new[] { new EnWmEx("Ending emails with an actionable next step eliminates back-and-forth guessing.", "Kết thúc email bằng một bước hành động cụ thể giúp tránh hỏi đi hỏi lại nhiều lần.") }),
                    new("wm-l53-h2", "en-US", "hard", "Sơ đồ từ vựng: constraint", "constraint", "/kənˈstreɪnt/", "Sự ràng buộc, giới hạn kỹ thuật",
                        new[] { new EnWmNode("prompt constraints", "/prɒmpt kənˈstreɪnts/", "Các ràng buộc trong Prompt", "Define strict prompt constraints to avoid hallucinations."), new EnWmNode("budget constraint", "/ˈbʌdʒɪt kənˈstreɪnt/", "Giới hạn ngân sách", "Operate within current cloud budget constraints.") },
                        new[] { new EnWmNode("work within", "/wɜːk wɪðˈɪn/", "Làm việc trong khuôn khổ", "Work within the 100ms latency budget."), new EnWmNode("factor in", "/ˈfæktər ɪn/", "Tính đến yếu tố", "Factor in backward compatibility constraints.") },
                        new[] { new EnWmNode("limitation", "/ˌlɪmɪˈteɪʃn/", "Sự hạn chế", "Document known API limitations."), new EnWmNode("boundary", "/ˈbaʊndri/", "Ranh giới giới hạn", "Set clear boundaries on output length.") },
                        new[] { new EnWmEx("Explicit constraints turn generic AI responses into production-ready engineering drafts.", "Các ràng buộc rõ ràng biến câu trả lời AI chung chung thành bản nháp kỹ thuật dùng được ngay.") })
                }, now),
            MakeEnActivity(c, l, 15, "cloze",
                "Điền từ ngữ cảnh (Bài 5.3): Thoát Văn mẫu Email & Prompt AI",
                "Thay thế văn mẫu máy móc bằng cụm tiếng Anh tự nhiên và rõ ràng.",
                new EnClItem[]
                {
                    new("en-l53-cloze-1", "easy", "Mở đầu báo trước thân thiện (Dễ)", "Điền cụm từ thay cho 'Please be informed that':", "Just a quick {{0}} that the staging server will {{1}} in ten minutes.", new[] { new EnClBlank(new[] { "heads-up", "note", "update" }, "lời báo trước nhanh", "'Just a quick heads-up' cực kỳ tự nhiên."), new EnClBlank(new[] { "restart", "reboot" }, "khởi động lại", "Động từ nguyên mẫu sau 'will'.") }, "Just a quick heads-up that the staging server will restart in ten minutes.", "Báo trước nhanh là máy chủ staging sẽ khởi động lại sau 10 phút.", "Tạm biệt câu hành chính cứng nhắc 'Please be informed'."),
                    new("en-l53-cloze-2", "easy", "Cảm ơn chuẩn ngữ pháp (Dễ)", "Điền trạng từ và từ cảm ơn đúng (không dùng 'am appreciate'):", "I {{0}} {{1}} your quick feedback on this pull request.", new[] { new EnClBlank(new[] { "really", "truly", "greatly" }, "thực sự (trạng từ bổ nghĩa)", "Không dùng 'very' trước động từ 'appreciate'!"), new EnClBlank(new[] { "appreciate" }, "trân trọng / cảm kích", "'appreciate' là động từ thường, không ghép 'am'.") }, "I really appreciate your quick feedback on this pull request.", "Tôi thực sự trân trọng phản hồi nhanh của bạn cho pull request này.", "Tránh lỗi sai kinh điển 'I am very appreciate'."),
                    new("en-l53-cloze-3", "medium", "Nhờ xem lại thay cho 'kindly revert' (Vừa)", "Điền cụm mời phản hồi tự nhiên:", "Could you {{0}} a look and share your {{1}} by Thursday?", new[] { new EnClBlank(new[] { "take", "have" }, "xem qua (___ a look)", "'take a look' = xem qua."), new EnClBlank(new[] { "thoughts", "feedback", "comments" }, "suy nghĩ / góp ý", "'share your thoughts' tự nhiên hơn 'revert to me'.") }, "Could you take a look and share your thoughts by Thursday?", "Bạn xem qua và chia sẻ ý kiến trước thứ Năm được không?", "Không dùng 'revert' thay cho 'reply' trong tiếng Anh quốc tế."),
                    new("en-l53-cloze-4", "medium", "Động từ 'discuss' không cần 'about' (Vừa)", "Điền động từ thảo luận chuẩn ngữ pháp:", "Let's {{0}} these prompt constraints and keep the summary {{1}}.", new[] { new EnClBlank(new[] { "discuss" }, "thảo luận (KHÔNG dùng about)", "'discuss something' đi thẳng tân ngữ."), new EnClBlank(new[] { "concise", "brief", "short" }, "súc tích ngắn gọn", "'keep X concise' = giữ X súc tích.") }, "Let's discuss these prompt constraints and keep the summary concise.", "Hãy thảo luận các ràng buộc prompt này và giữ bản tóm tắt súc tích.", "Nhớ: 'discuss X' hoặc 'talk about X'."),
                    new("en-l53-cloze-5", "hard", "Thiết kế Prompt AI 5 thành phần (Khó)", "Điền các từ khóa Prompt Engineering:", "{{0}} as a senior SRE and output a prioritized, {{1}} checklist in Markdown.", new[] { new EnClBlank(new[] { "Act" }, "đóng vai (___ as a...)", "Động từ mệnh lệnh mở đầu Role trong Prompt."), new EnClBlank(new[] { "actionable", "concise", "practical" }, "có thể hành động ngay / thiết thực", "'actionable checklist' = danh sách việc làm được ngay.") }, "Act as a senior SRE and output a prioritized, actionable checklist in Markdown.", "Hãy đóng vai một SRE cấp cao và xuất ra danh sách hành động ưu tiên bằng Markdown.", "Prompt rõ ràng dùng động từ mệnh lệnh chủ động.")
                }, now),
            MakeEnActivity(c, l, 16, "translate",
                "Dịch phản xạ Việt → Anh (Bài 5.3): Email Tự nhiên & Prompt AI",
                "Chuyển ý tiếng Việt thành email và prompt tiếng Anh súc tích, không dịch word-by-word.",
                new EnTrItem[]
                {
                    new("en-l53-trans-1", "easy", "Báo cáo hoàn tất công việc tự nhiên (Dễ)", "Cập nhật nhanh: hôm qua chúng tôi đã hoàn tất kiểm thử API và mọi endpoint đều đạt.", new[] { "quick update", "wrapped up", "passed" }, new[] { "Quick update: we wrapped up API testing yesterday, and all endpoints passed.", "Quick update: we finished API testing yesterday and every endpoint passed." }, "Quick update: we wrapped up API testing yesterday, and all endpoints passed.", "Dùng Past Simple vì có mốc 'yesterday' (không thêm 'already')."),
                    new("en-l53-trans-2", "easy", "Nhờ đồng nghiệp góp ý lịch sự (Dễ)", "Khi nào có chút thời gian, bạn xem qua bản nháp này và cho mình xin góp ý nhé.", new[] { "have a moment", "take a look", "feedback" }, new[] { "When you have a moment, could you take a look at this draft and share your feedback?", "Whenever you have a quick moment, could you review this draft and let me know your thoughts?" }, "When you have a moment, could you take a look at this draft and share your feedback?", "Thay thế hoàn hảo cho 'Sorry for bothering you, kindly check'."),
                    new("en-l53-trans-3", "medium", "Theo dõi tiếp email cũ (Vừa)", "Tôi nhắn tiếp để hỏi thăm tiến độ phê duyệt ngân sách đám mây cho quý tới.", new[] { "following up on", "cloud budget approval", "next quarter" }, new[] { "I am following up on the cloud budget approval for next quarter.", "I'm following up to check on the cloud budget approval for next quarter." }, "I am following up on the cloud budget approval for next quarter.", "'follow up on [something]' = nhắc lại / theo dõi tiếp vấn đề cũ."),
                    new("en-l53-trans-4", "medium", "Viết Prompt phân tích lỗi cho AI (Vừa)", "Hãy đóng vai kỹ sư CSDL cấp cao, phân tích câu SQL này và chỉ ra các index bị thiếu.", new[] { "act as", "analyze", "identify missing indexes" }, new[] { "Act as a senior database engineer, analyze this SQL query, and identify missing indexes.", "Act as a senior database engineer to analyze this SQL query and point out missing indexes." }, "Act as a senior database engineer, analyze this SQL query, and identify missing indexes.", "Dùng chuỗi động từ mệnh lệnh chủ động rõ ràng."),
                    new("en-l53-trans-5", "hard", "Đặt ràng buộc đầu ra cho AI (Khó)", "Hãy giữ tương thích ngược, tránh thêm thư viện ngoài và tóm tắt giải pháp trong một bảng súc tích.", new[] { "backward compatibility", "avoid adding", "concise table" }, new[] { "Keep backward compatibility, avoid adding external libraries, and summarize the solution in a concise table.", "Maintain backward compatibility, avoid external dependencies, and summarize your solution in a concise Markdown table." }, "Keep backward compatibility, avoid adding external libraries, and summarize the solution in a concise table.", "Sau 'avoid' luôn dùng danh động từ V-ing ('avoid adding').")
                }, now),
            MakeEnActivity(c, l, 17, "grammar_fix",
                "Sửa lỗi ngữ pháp (Bài 5.3): Thoát lỗi Vinglish & Văn mẫu",
                "Phát hiện và sửa các lỗi người Việt hay mắc nhất trong email và prompt tiếng Anh.",
                new EnGfItem[]
                {
                    new("en-l53-gf-1", "easy", "Lỗi 'I am very appreciate' (Dễ)", "I am very appreciate your help on this bug.", "'appreciate' là động từ thường (không dùng với 'am') và đi với 'really/truly', không dùng 'very'.", "I really appreciate your help with this bug.", new[] { "I really appreciate your help with this bug.", "I really appreciate your help on this bug." }, "Tôi thực sự trân trọng sự giúp đỡ của bạn với lỗi này.", "Sửa thành 'I really appreciate...'."),
                    new("en-l53-gf-2", "easy", "Lỗi 'discuss about' (Dễ)", "We need to discuss about the new prompt template tomorrow.", "'discuss' là ngoại động từ đi thẳng với tân ngữ, không dùng giới từ 'about'.", "We need to discuss the new prompt template tomorrow.", new[] { "We need to discuss the new prompt template tomorrow.", "We need to talk about the new prompt template tomorrow." }, "Chúng ta cần thảo luận về mẫu prompt mới vào ngày mai.", "Bỏ 'about' sau 'discuss'."),
                    new("en-l53-gf-3", "medium", "Dùng 'look forward to + V-inf' sai (Vừa)", "I look forward to hear your feedback by Friday.", "Trong 'look forward to', chữ 'to' là giới từ nên động từ theo sau bắt buộc chia V-ing.", "I look forward to hearing your feedback by Friday.", new[] { "I look forward to hearing your feedback by Friday.", "I look forward to hearing from you by Friday." }, "Tôi rất mong nhận được phản hồi của bạn trước thứ Sáu.", "Nhớ kỹ: 'look forward to + V-ing'."),
                    new("en-l53-gf-4", "medium", "Quá khứ đơn với 'yesterday' vs 'already' (Vừa)", "Please be informed that we already finished the deployment yesterday.", "Có mốc thời gian quá khứ xác định 'yesterday' dùng Past Simple đơn thuần, và thay 'Please be informed' bằng mở đầu tự nhiên.", "Quick update: we finished the deployment yesterday.", new[] { "Quick update: we finished the deployment yesterday.", "Just a quick update: we wrapped up the deployment yesterday." }, "Cập nhật nhanh: hôm qua chúng tôi đã hoàn tất triển khai rồi.", "Văn phong gọn gàng, chuẩn thì quá khứ đơn."),
                    new("en-l53-gf-5", "hard", "Lạm dụng 'revert' và 'asap' thô cứng (Khó)", "Kindly check the attached document and revert to me asap.", "Bản xứ không dùng 'revert' nghĩa là 'trả lời'; hãy dùng câu mời phản hồi tự nhiên kèm thời hạn rõ.", "Could you please review the attached document and let me know your thoughts by tomorrow?", new[] { "Could you please review the attached document and let me know your thoughts by tomorrow?", "Please review the attached document and share your thoughts when you have a moment." }, "Bạn xem giúp tài liệu đính kèm và cho mình biết ý kiến trước ngày mai nhé?", "Thoát văn mẫu cũ kỹ sang giao tiếp chuyên nghiệp hiện đại.")
                }, now),
            MakeEnActivity(c, l, 18, "roleplay",
                "Nhập vai hội thoại (Bài 5.3): Nâng cấp Email & Cộng tác với AI",
                "Thực hành viết lại câu email tự nhiên và đặt Prompt rõ ràng cho trợ lý AI.",
                new EnRpItem[]
                {
                    new("en-l53-rp-1", "easy", "Nâng cấp lời báo trước thay 'Please be informed' (Dễ)", "Bạn cần báo cả team trên Slack rằng server staging sẽ khởi động lại sau 10 phút.", "Alex (Teammate)", "Hey! I'm about to run a 20-minute test on staging—anything I should know before starting?", "Chào bạn! Mình sắp chạy bài test 20 phút trên staging—có gì mình cần biết trước khi bắt đầu không?", "Báo trước nhanh tự nhiên bằng 'Quick heads-up' và thời gian khởi động lại.", new[] { "heads-up", "staging", "restart", "minutes" }, new[] { "Just a quick heads-up: staging is going to restart in ten minutes, so you might want to hold off on that test!" }, 12),
                    new("en-l53-rp-2", "medium", "Hỏi tiến độ tự nhiên thay cho 'Kindly revert' (Vừa)", "Bạn gửi báo cáo kiểm thử từ thứ Ba mà tới thứ Năm đối tác chưa phản hồi.", "Alex (Partner)", "Hi Minh, sorry for the busy week! What did you need from my side for Friday's release?", "Chào Minh, tuần này bận quá! Bạn đang cần gì từ phía mình cho bản phát hành thứ Sáu nhỉ?", "Nhắc lại khéo léo (following up), nhờ xem bản tóm tắt đính kèm và phản hồi trong hôm nay.", new[] { "following up", "attached", "thoughts", "release" }, new[] { "No worries at all! I'm following up on the attached test summary—could you take a look and share your thoughts today so we stay on track for Friday's release?" }, 16),
                    new("en-l53-rp-3", "medium", "Viết Prompt 5 bước cho AI Assistant (Vừa)", "Bạn đang nhờ AI tối ưu một truy vấn PostgreSQL chậm trong hệ thống .NET 8.", "AI Engineering Assistant", "I'm ready to help! What role, context, constraints, and output format would you like me to follow?", "Tôi sẵn sàng hỗ trợ! Bạn muốn tôi tuân theo vai trò, bối cảnh, ràng buộc và định dạng đầu ra nào?", "Áp dụng khung R-C-T-C-F: nêu vai trò (Act as), bối cảnh (.NET/PostgreSQL), nhiệm vụ, ràng buộc và định dạng bảng Markdown.", new[] { "act as", "postgresql", "identify", "avoid", "table" }, new[] { "Act as a senior database engineer. In our .NET 8 and PostgreSQL app, analyze this slow query to identify missing indexes. Avoid adding Redis, and output an actionable checklist in a concise Markdown table." }, 18),
                    new("en-l53-rp-4", "hard", "Sửa & nâng cấp email Vinglish của đồng nghiệp (Khó)", "Một thực tập sinh nhờ bạn sửa email trước khi gửi cho khách hàng Mỹ.", "Junior Dev", "I drafted: 'Dear Sir, I am very appreciate if you kindly revert asap about our API issue.' How can I make it sound natural?", "Em viết nháp: 'Dear Sir, I am very appreciate if you kindly revert asap about our API issue.' Làm sao để nghe tự nhiên hơn ạ?", "Chỉ ra lỗi 'am very appreciate' + 'kindly revert' và viết lại một bản nâng cấp súc tích, thân thiện chuẩn B1+.", new[] { "really appreciate", "take a look", "thoughts", "instead" }, new[] { "Instead of 'am very appreciate' and 'kindly revert', try: 'Hi [Name], I really appreciate your help with our API question. Could you take a look and share your thoughts by tomorrow?'" }, 16)
                }, now)
        };
    }

    #endregion

    #region 7. English A2-B1 Quizzes (6 Lesson Quizzes + [Đề Chương 4] + [Đề Chương 5])

    private static List<(string Key, Guid LessonId, string Title, QuizQuestionSpec[] Questions)> GetEnQuizSpecs() => new()
    {
        ("en:l41", EnLesson41Id, "Bài 4.1: Kiểm tra Xuất nhập cảnh, Khách sạn & Sự cố Chuyến bay", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Câu nào đúng ngữ pháp khi nói về chuyến công tác kéo dài 5 ngày?", "a five-days business trip", "a five-day business trip", "five day business trip", "a five-days business trips", "B", "Tính từ ghép có gạch nối đứng trước danh từ không bao giờ thêm -s."),
            new("multiple_choice", "Danh từ nào sau đây KHÔNG đếm được (tuyệt đối không thêm -s ở dạng số nhiều)?", "suitcases", "boarding passes", "luggage", "vouchers", "C", "luggage và baggage là danh từ không đếm được; dùng 'two pieces of luggage'."),
            new("multiple_choice", "Trong từ 'receipt' (hóa đơn), chữ cái nào là âm câm không phát âm?", "Chữ 'r'", "Chữ 'c'", "Chữ 'p'", "Chữ 't'", "C", "'receipt' phát âm là /rɪˈsiːt/ với âm 'p' hoàn toàn câm."),
            new("cloze", "Mẫu câu lịch sự hỏi quyền lợi phiếu ăn khi chuyến bay bị hoãn lâu: \"Am I {{c1::eligible}} for a meal voucher?\"", null, null, null, null, "eligible", "'eligible for' nghĩa là đủ điều kiện được hưởng quyền lợi."),
            new("multiple_choice", "Cách đặt câu hỏi gián tiếp chuẩn xác khi hỏi đường tới băng chuyền nhận hành lý là gì?", "Could you tell me where is baggage claim?", "Could you tell me where baggage claim is?", "Where baggage claim is?", "Tell me where is baggage claim?", "B", "Trong câu hỏi gián tiếp không đảo động từ 'is' lên trước chủ ngữ.")
        }),
        ("en:l42", EnLesson42Id, "Bài 4.2: Kiểm tra Phỏng vấn Kỹ thuật & STAR Method", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Trong phương pháp STAR, chữ 'A' (Action) nên tập trung diễn đạt bằng đại từ nào?", "'We' để thể hiện tinh thần đồng đội", "'They' để nói về bộ phận khác", "'I' để làm rõ đóng góp kỹ thuật cá nhân của chính bạn", "'It' để giữ văn phong khách quan", "C", "Nhà tuyển dụng muốn biết chính xác cá nhân bạn đã làm gì ở bước Action."),
            new("multiple_choice", "Chọn câu dùng ĐÚNG giới từ khi mô tả kết quả cải thiện độ trễ:", "Latency dropped from 2 seconds by 100ms.", "Latency dropped from 2 seconds to 100ms.", "Latency dropped to 2 seconds from 100ms.", "Latency dropped at 2 seconds to 100ms.", "B", "Chuyển từ mốc cũ sang mốc mới luôn dùng 'from X to Y'."),
            new("multiple_choice", "Cách trả lời chuyên nghiệp nhất khi nhà tuyển dụng hỏi công nghệ bạn chưa từng dùng trên Production là gì?", "I don't know that tool.", "I haven't used it in production yet, though I have strong experience with similar event-driven patterns.", "Skip to the next question please.", "I used it ten years ago and forgot everything.", "B", "Thể hiện sự trung thực kết hợp năng lực chuyển đổi kiến thức tương đương."),
            new("cloze", "Cụm động từ mở đầu bước Task trong STAR: \"As the on-call engineer, I was {{c1::tasked}} with isolating the root cause.\"", null, null, null, null, "tasked", "'be tasked with + V-ing' = được giao nhiệm vụ thực hiện."),
            new("essay", "Viết một đoạn trả lời phỏng vấn ngắn (3-4 câu) theo khung STAR kể về một lần bạn tối ưu tốc độ truy vấn hoặc sửa lỗi hệ thống.", null, null, null, null,
                "Last quarter, our search endpoint experienced high latency spikes during peak hours. As the backend engineer, I was tasked with finding the bottleneck. I diagnosed a missing composite index, added Redis caching, and cut p99 latency from 1.8s to 90ms.",
                "Cần có đủ bối cảnh (Situation), nhiệm vụ (Task), hành động cá nhân dùng 'I' (Action) và kết quả có con số (Result).",
                new[] { "Dùng thì Quá khứ đơn nhất quán", "Nêu rõ hành động cá nhân bằng đại từ 'I'", "Có số liệu đo lường kết quả" })
        }),
        ("en:l43", EnLesson43Id, "Bài 4.3: Kiểm tra Code Review, Bug Report & Kiến trúc", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Nhãn bình luận nào trong Code Review dùng cho góp ý nhỏ về đặt tên biến/format mà không chặn việc merge PR?", "Blocker", "Nitpick (Nit)", "Regression", "Outage", "B", "Nit / Nitpick biểu thị góp ý nhỏ để code đẹp hơn, không bắt buộc chặn PR."),
            new("multiple_choice", "Cách diễn đạt nào tự nhiên nhất khi bạn vừa đẩy bản sửa lỗi và muốn đồng nghiệp xem lại PR?", "Please help me check my PR!", "I've pushed the fix—could you please take another look when you have a moment?", "My PR is conflict with main.", "Check it now!", "B", "'take another look' là cụm tự nhiên chuẩn bản xứ thay cho 'help me check'."),
            new("multiple_choice", "Câu nào đúng ngữ pháp khi báo nhánh code bị xung đột?", "This branch is conflict with main.", "This branch has merge conflicts with main.", "This branch conflicts about main.", "Conflict is on this branch.", "B", "Dùng danh từ 'has merge conflicts with' hoặc động từ 'conflicts with'."),
            new("cloze", "Trong thảo luận kiến trúc, dùng hàng đợi thông điệp giúp {{c1::decouple}} (tách rời phụ thuộc) giữa luồng API và tác vụ nền.", null, null, null, null, "decouple", "'decouple A from B' là thuật ngữ kiến trúc tách rời thành phần."),
            new("multiple_choice", "Câu nào tránh thừa giới từ sai ngữ pháp khi dùng động từ 'discuss'?", "Let's discuss about the schema changes.", "Let's discuss the schema changes.", "Let's discuss on the schema changes.", "Let's discuss for the schema changes.", "B", "Động từ 'discuss' đi thẳng với tân ngữ, tuyệt đối không có 'about'.")
        }),
        ("en:l51", EnLesson51Id, "Bài 5.1: Kiểm tra Storytelling Dữ liệu & Thuyết phục Lãnh đạo", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Nguyên tắc 'BLUF' (Bottom Line Up Front) khi báo cáo lãnh đạo có nghĩa là gì?", "Kể chi tiết kỹ thuật từ năm ngoái trước rồi mới chốt sau", "Đưa kết luận và đề xuất cốt lõi lên ngay câu mở đầu trước khi trình bày số liệu hỗ trợ", "Chỉ gửi bảng Excel không giải thích", "Trình bày mọi lỗi nhỏ trước khi nói mục tiêu", "B", "Lãnh đạo cần biết đề xuất chính và tác động ROI ngay đầu buổi họp."),
            new("multiple_choice", "Chọn câu dùng ĐÚNG ngữ pháp về phần trăm và số liệu:", "Costs dropped by 35 percents.", "Costs dropped by 35 percent.", "There was a drop by 35 percent.", "Revenue reached two millions dollars.", "B", "'percent' không thêm -s; động từ dùng 'by', danh từ 'a drop' dùng 'of'."),
            new("multiple_choice", "Động từ nào diễn tả đường biểu đồ tăng trưởng sau đó đi ngang ổn định ở một mức?", "plummeted", "surged", "plateaued", "tumbled", "C", "'plateau at [level]' nghĩa là chững lại và đi ngang."),
            new("cloze", "Thành ngữ kinh doanh diễn tả một khoản đầu tư nhanh chóng thu hồi chi phí nhờ tiền tiết kiệm được: \"This upgrade {{c1::pays}} for itself within two months.\"", null, null, null, null, "pays", "'pay for itself' = tự hoàn vốn."),
            new("multiple_choice", "Sau danh từ 'an increase', giới từ nào đi kèm trước con số mức tăng?", "by", "of", "at", "for", "B", "Cấu trúc chuẩn: 'an increase of 15%' (khác với động từ 'increased by 15%').")
        }),
        ("en:l52", EnLesson52Id, "Bài 5.2: Kiểm tra Xử lý Khủng hoảng & Từ chối Khéo léo", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Bước đầu tiên cần làm ngay khi khách hàng gọi điện bức xúc vì hệ thống lỗi là gì?", "Tranh cãi chứng minh lỗi do mạng của khách hàng", "Công nhận cảm xúc, thấu cảm mức độ cấp bách và xin lỗi chân thành vì sự gián đoạn (Acknowledge & Empathize)", "Cúp máy chuyển cho bộ phận khác", "Hứa bừa 5 phút nữa xong dù chưa rõ nguyên nhân", "B", "Theo quy trình L.A.E.R., cần hạ nhiệt cảm xúc bằng sự lắng nghe và thấu cảm trước."),
            new("multiple_choice", "Câu nào dùng ĐÚNG cấu trúc nhượng bộ với 'Despite'?", "Despite of the outage, we met our SLA.", "Despite the outage, we met our SLA.", "Although the outage, we met our SLA.", "Despite the outage happened, we met our SLA.", "B", "'Despite' đi thẳng với cụm danh từ và KHÔNG có 'of'."),
            new("multiple_choice", "Cấu trúc 'The Positive No' (Yes - No - Yes) khi từ chối tính năng ngoài phạm vi gồm 3 bước nào?", "Từ chối thẳng -> Phạt tiền -> Kết thúc", "Công nhận giá trị yêu cầu -> Nêu giới hạn khách quan -> Đề xuất phương án thay thế khả thi", "Đồng ý hết -> Làm không kịp -> Xin lỗi sau", "Đổ lỗi cho nhóm khác -> Từ chối -> Im lặng", "B", "Positive No giữ thiện cảm đối tác nhờ công nhận nhu cầu và trao giải pháp thay thế."),
            new("cloze", "Giải pháp khắc phục tạm thời để khách hàng tiếp tục công việc trong khi chờ bản vá chính thức gọi là {{c1::workaround}}.", null, null, null, null, "workaround", "'workaround' là từ vựng cực kỳ phổ biến trong hỗ trợ kỹ thuật doanh nghiệp."),
            new("multiple_choice", "Câu nào áp dụng đúng kỹ thuật 'Hedging' (giảm nhẹ) khi phản biện đồng nghiệp?", "Your architecture is terrible and will crash.", "There's a risk this approach might face scalability bottlenecks under peak load.", "You clearly don't understand databases.", "No, never do that.", "B", "Hedging dùng 'might / there is a risk' và hướng vào giải pháp thay vì công kích cá nhân.")
        }),
        ("en:l53", EnLesson53Id, "Bài 5.3: Kiểm tra Nâng cấp Email & Prompt Engineering Tự nhiên", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Vì sao nên tránh viết 'Kindly revert to me asap' trong email công việc quốc tế?", "Vì từ 'revert' trong tiếng Anh chuẩn nghĩa là 'hoàn tác về trạng thái cũ', không phải 'trả lời', và văn phong này nghe cứng nhắc lỗi thời", "Vì từ 'kindly' sai chính tả", "Vì email không được dùng dấu chấm", "Vì 'me' phải đổi thành 'I'", "A", "Thay bằng 'Could you please review this and share your thoughts by tomorrow?'."),
            new("multiple_choice", "Câu nào sửa đúng lỗi Vinglish 'I am very appreciate your help'?", "I am really appreciate your help.", "I really appreciate your help.", "I very appreciate your help.", "I appreciating your help.", "B", "'appreciate' là động từ thường nên không ghép với 'am' và dùng trạng từ 'really/truly'."),
            new("multiple_choice", "Cụm từ tự nhiên nào thay thế xuất sắc cho câu mở đầu sáo rỗng 'Please be informed that...'?", "Kindly be noticed that...", "Just a quick heads-up that...", "I inform to you that...", "Attention please that...", "B", "'Just a quick heads-up' mang sắc thái thân thiện, hiện đại và đi thẳng vào trọng tâm."),
            new("cloze", "Trong cụm từ kết thúc email 'I look forward to {{c1::hearing}} from you soon', động từ luôn chia ở dạng V-ing.", null, null, null, null, "hearing", "'to' trong 'look forward to' là giới từ nên đi kèm V-ing."),
            new("essay", "Viết lại một câu email tự nhiên hơn thay cho: 'Dear Sir, Please be informed that we already finished the bug fix yesterday. Kindly check and revert asap.'", null, null, null, null,
                "Hi Alex, Quick update: we wrapped up the bug fix yesterday. Could you take a look and share your feedback by tomorrow afternoon? Thanks!",
                "Cần bỏ 'Please be informed', bỏ 'already' khi có 'yesterday', và thay 'kindly revert' bằng lời mời phản hồi tự nhiên.",
                new[] { "Mở đầu thân thiện tự nhiên (Quick update / heads-up)", "Chia đúng thì Past Simple với yesterday", "Thay 'revert' bằng 'take a look / share your feedback'" })
        }),

        // [Đề Chương 4] (10 balanced items: 2 A, 2 B, 2 C, 2 D + 1 cloze + 1 essay)
        ("en:ch4", EnLesson43Id, "[Đề Chương 4] Tiếng Anh Du lịch, Phỏng vấn STAR & Cộng tác Kỹ thuật", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Chọn câu chuẩn xác khi làm thủ tục nhận phòng khách sạn:", "I would like to check in under the name Minh Tran.", "I want check in with name Minh Tran.", "I check-in below name Minh Tran.", "Give me room under Minh Tran.", "A", "'check in under the name [Name]' là mẫu câu lễ tân chuẩn."),
            new("multiple_choice", "Khi hành lý không ra trên băng chuyền sân bay, bạn cần tìm quầy nào và lập biên bản gì?", "Baggage Claim counter — Property Irregularity Report (PIR)", "Duty-Free counter — Boarding Pass", "Currency Exchange — Tax Receipt", "Security Gate — Lounge Voucher", "A", "Quầy dịch vụ hành lý tại Baggage Claim sẽ lập biên bản PIR để giao hành lý về khách sạn."),
            new("multiple_choice", "Câu nào viết đúng ngữ pháp về hành lý và thời gian lưu trú?", "I have two luggages for a five-days trip.", "I have two pieces of luggage for a five-day trip.", "I have two luggage for five-days trip.", "I have a luggage for five day trip.", "B", "'luggage' không đếm được; tính từ ghép 'five-day' không thêm -s."),
            new("multiple_choice", "Trong phỏng vấn hành vi theo mô hình STAR, phần nào nên chiếm khoảng 50% thời lượng trả lời và dùng đại từ 'I'?", "Situation", "Action (Hành động cá nhân bạn thực hiện)", "Introduction", "Greeting", "B", "Nhà tuyển dụng chấm điểm cao nhất ở phần Action cá nhân cụ thể của ứng viên."),
            new("multiple_choice", "Chọn câu diễn đạt kết quả STAR chuẩn xác về mặt giới từ số liệu:", "Latency dropped from 2.4s by 120ms.", "Latency dropped at 2.4s to 120ms.", "Latency dropped from 2.4s to 120ms.", "Latency dropped of 2.4s into 120ms.", "C", "Chuyển từ mốc ban đầu tới mốc mới dùng 'from ... to ...'."),
            new("multiple_choice", "Trong Code Review trên GitHub, câu góp ý nào mang tính xây dựng và lịch sự nhất?", "This loop is stupid, rewrite it!", "Why didn't you use a Set?", "Suggestion: using a Set here would reduce lookup complexity to O(1).", "Fix line 12 immediately.", "C", "Gắn nhãn Suggestion và nêu rõ lợi ích kỹ thuật giúp đồng nghiệp tiếp nhận tích cực."),
            new("multiple_choice", "Cách mở đầu câu hỏi tò mò về lý do thiết kế mà không gây căng thẳng cho tác giả PR là gì?", "Who wrote this mess?", "What is this code doing?!", "Why is this bypassed?!", "Just out of curiosity, is there a specific reason we're bypassing the cache here?", "D", "'Just out of curiosity' làm mềm câu hỏi kỹ thuật."),
            new("multiple_choice", "Một Bug Report chuyên nghiệp tối thiểu phải có 4 mục nào?", "Emoji, chữ in hoa, ảnh nền, lời phàn nàn", "Lời chào dài, tiểu sử tác giả, tên công ty, chữ ký", "Chỉ mỗi câu 'App broken please fix'", "Summary, Steps to Reproduce, Expected vs. Actual Behavior, và Environment/Logs", "D", "Đủ 4 phần này giúp kỹ sư tái hiện và sửa lỗi nhanh chóng."),
            new("cloze", "Khi được hỏi công nghệ chưa từng dùng thực tế, ta nói: \"I haven't deployed Kafka in production {{c1::yet}}, though I understand event-driven patterns.\"", null, null, null, null, "yet", "Dùng Hiện tại hoàn thành + 'yet' diễn đạt trải nghiệm chưa có tính tới hiện tại."),
            new("essay", "Viết 1 đoạn trả lời phỏng vấn STAR ngắn (4-5 câu) về một lần bạn phát hiện lỗi hoặc tối ưu hiệu năng trong dự án, có đầy đủ S-T-A-R và số liệu cụ thể.", null, null, null, null,
                "In my previous project, our report export API timed out during end-of-month traffic spikes. As the backend developer, I was tasked with stabilizing the endpoint. I profiled the SQL queries, added a composite index, and moved PDF generation to an asynchronous worker queue. As a result, p99 response time dropped from 4.5 seconds to 180 milliseconds with zero timeouts.",
                "Cần đủ 4 bước STAR, dùng thì Quá khứ đơn, nêu hành động cá nhân ('I') và kết quả đo lường.",
                new[] { "Có đủ Situation, Task, Action, Result", "Chia thì Quá khứ đơn chính xác", "Có số liệu trước/sau rõ ràng" })
        }),

        // [Đề Chương 5] (10 balanced items: 2 A, 2 B, 2 C, 2 D + 1 cloze + 1 essay)
        ("en:ch5", EnLesson53Id, "[Đề Chương 5] Kể chuyện Dữ liệu, Xử lý Khủng hoảng & Viết Chuẩn B1+", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Theo nguyên tắc BLUF khi thuyết trình trước Ban Giám đốc, câu mở đầu nên trình bày nội dung gì?", "Đề xuất cốt lõi và giá trị kinh doanh (ROI) ngay lập tức", "Lịch sử hình thành dự án từ 3 năm trước", "Toàn bộ mã nguồn chi tiết", "Xin lỗi vì slide chưa đẹp", "A", "Bottom Line Up Front giúp lãnh đạo nắm ngay trọng tâm quyết định."),
            new("multiple_choice", "Câu nào dùng ĐÚNG giới từ mô tả sự thay đổi số liệu sau danh từ và động từ?", "There was a drop of 25%, and costs fell by $5,000.", "There was a drop by 25%, and costs fell of $5,000.", "Costs fell 25 percents to $5,000.", "Revenue rose of two millions dollars.", "A", "Danh từ 'a drop of ...'; Động từ 'fell by ...'; 'percent' không có -s."),
            new("multiple_choice", "Khi lượng người dùng ngừng tăng và giữ nguyên mức ổn định trong nhiều tuần, động từ chuẩn xác nhất là gì?", "plummeted", "plateaued", "surged", "plunged", "B", "'plateau at [level]' = đi ngang, chững lại."),
            new("multiple_choice", "Trong xử lý khủng hoảng khách hàng (L.A.E.R.), sau khi thừa nhận và thấu cảm bức xúc, bước thực tế giúp cứu nguy ngay cho khách hàng là gì?", "Tranh luận ai đúng ai sai", "Cung cấp giải pháp tạm thời (workaround) kèm lịch cập nhật liên tục (keep posted)", "Tắt điện thoại chờ kỹ sư sửa xong", "Yêu cầu khách hàng tự sửa lỗi", "B", "Trao giải pháp tạm thời và cam kết nhịp cập nhật giúp khách hàng kiểm soát tình hình."),
            new("multiple_choice", "Câu nào sửa đúng hoàn toàn lỗi ngữ pháp thường gặp với 'Despite' và 'Although'?", "Despite of the traffic spike, we stayed online.", "Although the traffic spike, we stayed online.", "Despite the unexpected traffic spike, we met our monthly SLA target.", "Although the spike happened, but we met our SLA.", "C", "'Despite + Noun phrase' (không có 'of', không ghép 'Although... but')."),
            new("multiple_choice", "Công thức 'Positive No' (Yes - No - Yes) có tác dụng gì khi đối tác yêu cầu tính năng ngoài phạm vi?", "Làm đối tác tức giận bỏ đi", "Bỏ qua mọi quy trình kiểm thử để chiều khách", "Công nhận nhu cầu -> Giải thích giới hạn khách quan -> Đề xuất phương án thay thế khả thi", "Đồng ý miễn phí mọi yêu cầu", "C", "Giữ vững ranh giới kỹ thuật mà vẫn bảo toàn quan hệ hợp tác."),
            new("multiple_choice", "Cặp sửa lỗi nào loại bỏ ĐÚNG cả hai lỗi Vinglish phổ biến trong email công việc?", "'Please be informed' -> 'Please be known'", "'Kindly revert' -> 'Kindly return'; 'asap' -> 'ASAP!'", "'I look forward to hear' -> 'I look forward hear'", "'I am very appreciate' -> 'I really appreciate'; 'discuss about' -> 'discuss'", "D", "'appreciate' là động từ thường (dùng really); 'discuss' không đi với 'about'."),
            new("multiple_choice", "Cấu trúc 5 phần chuẩn giúp viết Prompt tiếng Anh sắc bén cho AI (R-C-T-C-F) gồm những thành phần nào?", "Greeting - Joke - Question - Emoji - Goodbye", "Only one vague sentence", "Copy-paste raw error without context", "Role - Context - Task - Constraints - Format", "D", "Xác định rõ vai trò, bối cảnh, nhiệm vụ chủ động, ràng buộc và định dạng đầu ra."),
            new("cloze", "Cụm từ tự nhiên thay cho 'Please be informed that...' khi mở đầu thông báo ngắn trên Slack/Email là \"Just a quick {{c1::heads-up}} that...\".", null, null, null, null, "heads-up", "'Just a quick heads-up' là cụm báo trước tự nhiên chuẩn công sở hiện đại."),
            new("essay", "Khách hàng gửi email yêu cầu thêm tính năng báo cáo tùy chỉnh gấp vào thứ Sáu (nằm ngoài phạm vi hợp đồng hiện tại). Hãy viết email ngắn (4-5 câu) áp dụng công thức Positive No để từ chối khéo và đề xuất lộ trình thay thế.", null, null, null, null,
                "Hi Marcus, I completely understand how valuable that custom dashboard would be for your Friday presentation. However, given our ongoing security audit, adding new analytics is out of scope for this sprint without risking stability. What we can do instead is send your core CSV export by Friday and schedule the full dashboard for next Wednesday. Please let me know if that works for you! Best, Minh.",
                "Email cần có thái độ tôn trọng (Acknowledge), từ chối rõ ràng với lý do khách quan (out of scope), đưa phương án thay thế (What we can do instead) và văn phong tự nhiên.",
                new[] { "Áp dụng đúng 3 bước Positive No (Yes - No - Yes)", "Văn phong tự nhiên, không mắc lỗi Vinglish/văn mẫu cứng", "Đưa phương án thay thế và thời hạn rõ ràng" })
        })
    };

    #endregion

    #region 8. English A2-B1 (+60 Deterministic FSRS Flashcards: 10 per new lesson 4.1..5.3)

    private static async Task EnsureEnExpandedFlashcardsAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var tag = await EnsureTagAsync(db, "English-A2-B1", "sky", now);
        var l41 = EnLesson41Id;
        var l42 = EnLesson42Id;
        var l43 = EnLesson43Id;
        var l51 = EnLesson51Id;
        var l52 = EnLesson52Id;
        var l53 = EnLesson53Id;

        var specs = new (Guid LessonId, string Front, string Back)[]
        {
            // Bài 4.1: Xuất nhập cảnh, Khách sạn & Sự cố Chuyến bay (10 thẻ)
            (l41, "Nothing to declare", "/ˈnʌθɪŋ tuː dɪˈkleər/ — Không có hàng hóa gì cần khai báo hải quan.\nVí dụ: I have nothing to declare except personal belongings and my work laptop."),
            (l41, "Connecting flight / Layover", "/kəˈnektɪŋ flaɪt/ | /ˈleɪəʊvə/ — Chuyến bay nối chuyến / Thời gian chờ quá cảnh.\nVí dụ: Due to fog, I missed my connecting flight during a two-hour layover."),
            (l41, "Rebook on the next available flight", "/ˌriːˈbʊk/ — Đặt lại vé sang chuyến sớm nhất còn chỗ.\nVí dụ: Could you please rebook me on the next available flight to Tokyo?"),
            (l41, "Eligible for a meal voucher", "/ˈelɪdʒəbl fɔː/ — Đủ điều kiện nhận phiếu ăn uống miễn phí.\nVí dụ: Since the flight is delayed by five hours, am I eligible for a meal voucher?"),
            (l41, "Baggage claim & Property Irregularity Report", "/ˈbæɡɪdʒ kleɪm/ — Khu vực nhận hành lý & Biên bản thất lạc hành lý (PIR).\nVí dụ: Ask the baggage claim counter for an irregularity report if your suitcase is missing."),
            (l41, "Luggage (uncountable noun)", "/ˈlʌɡɪdʒ/ — Hành lý (danh từ không đếm được, KHÔNG thêm -s).\nVí dụ: I checked in two pieces of luggage (không nói two luggages)."),
            (l41, "Receipt (silent 'p')", "/rɪˈsiːt/ — Biên lai, hóa đơn thanh toán (chữ 'p' hoàn toàn câm!).\nVí dụ: Please keep the official receipt for company reimbursement."),
            (l41, "Check in under the name...", "Làm thủ tục nhận phòng dưới tên của...\nVí dụ: Good afternoon, I would like to check in under the name Minh Tran."),
            (l41, "Would it be possible to...?", "Mẫu câu xin phép / đề nghị lịch thiệp nhất (+ V-inf).\nVí dụ: Would it be possible to move to a quieter room away from the elevator?"),
            (l41, "Refundable security deposit", "/rɪˈfʌndəbl sɪˈkjʊərəti dɪˈpɒzɪt/ — Tiền đặt cọc an ninh sẽ được hoàn lại khi trả phòng.\nVí dụ: The hotel places a temporary security deposit on your card at check-in."),

            // Bài 4.2: Phỏng vấn Kỹ thuật & STAR Method (10 thẻ)
            (l42, "STAR Method (Situation - Task - Action - Result)", "Khung trả lời phỏng vấn hành vi: Bối cảnh -> Nhiệm vụ -> Hành động cá nhân ('I') -> Kết quả có con số.\nVí dụ: Always quantify the Result step with latency, percentage, or cost savings."),
            (l42, "Be tasked with + V-ing", "Được giao trọng trách thực hiện việc gì (bước Task trong STAR).\nVí dụ: As the on-call engineer, I was tasked with isolating the database bottleneck."),
            (l42, "Root cause", "/ruːt kɔːz/ — Nguyên nhân gốc rễ của sự cố.\nVí dụ: Inspecting distributed traces helped me pinpoint the root cause in ten minutes."),
            (l42, "Bottleneck", "/ˈbɒtlnek/ — Điểm nghẽn cổ chai làm chậm toàn hệ thống.\nVí dụ: An unindexed SQL join became our primary throughput bottleneck."),
            (l42, "Architectural trade-off", "/ˈtreɪd ɒf/ — Sự đánh đổi giữa hai mục tiêu kỹ thuật (ví dụ tốc độ vs tính nhất quán).\nVí dụ: Caching improves read latency, at the trade-off of invalidation complexity."),
            (l42, "Spearhead", "/ˈspɪəhed/ — Dẫn dắt tiên phong một dự án hoặc cải tiến lớn.\nVí dụ: I spearheaded the migration from a monolith to containerized microservices."),
            (l42, "Mitigate the risk", "/ˈmɪtɪɡeɪt/ — Giảm nhẹ mức độ rủi ro hoặc tác động sự cố.\nVí dụ: Adding circuit breakers helped mitigate cascading service failures."),
            (l42, "Dropped from X to Y / Dropped by Z%", "Cấu trúc báo cáo số liệu giảm: giảm TỪ X XUỐNG Y (from... to) hoặc giảm ĐI Z% (by...).\nVí dụ: API p99 latency dropped from 3.2 seconds to 45 milliseconds."),
            (l42, "I haven't used X in production yet, though...", "Cách trả lời trung thực khi chưa dùng một công cụ cụ thể.\nVí dụ: I haven't deployed Kafka in production yet, though I have strong experience with RabbitMQ."),
            (l42, "Pay down technical debt", "Trả nợ kỹ thuật (tái cấu trúc mã nguồn cũ để dễ bảo trì).\nVí dụ: How does your team balance shipping features with paying down technical debt?"),

            // Bài 4.3: Code Review, Báo cáo Bug & Kiến trúc (10 thẻ)
            (l43, "Good catch!", "Cụm phản hồi tích cực trong Code Review: 'Phát hiện rất hay / chuẩn đấy!'.\nVí dụ: Good catch! That makes total sense—I'll add a null check right away."),
            (l43, "Nit / Nitpick", "/nɪtpɪk/ — Góp ý chi tiết nhỏ (đặt tên biến, format) không chặn merge PR.\nVí dụ: Nit: could we rename `cnt` to `retryCount` for better clarity?"),
            (l43, "Take another look", "Xem lại / duyệt lại PR (tự nhiên hơn 'help me check').\nVí dụ: I've pushed the unit tests—could you take another look when you have a moment?"),
            (l43, "Has merge conflicts with...", "Có xung đột trộn mã với nhánh khác (KHÔNG dùng 'is conflict').\nVí dụ: This feature branch currently has merge conflicts with main."),
            (l43, "Steps to reproduce", "Các bước tái hiện lỗi trong Bug Report (kèm Expected vs. Actual Behavior).\nVí dụ: Please add clear steps to reproduce so QA can verify the fix."),
            (l43, "Edge case", "/edʒ keɪs/ — Trường hợp biên hiếm gặp (mảng rỗng, giá trị 0, đồng thời).\nVí dụ: Let's add a unit test covering the empty-array edge case."),
            (l43, "Backward-compatible", "/ˈbækwəd kəmˈpætəbl/ — Tương thích ngược với các phiên bản cũ.\nVí dụ: Ensure the updated JSON schema remains backward-compatible with older mobile clients."),
            (l43, "Decouple A from B", "/diːˈkʌpl/ — Tách rời sự phụ thuộc chặt chẽ giữa hai thành phần.\nVí dụ: Message queues decouple HTTP handlers from heavy background AI jobs."),
            (l43, "Single point of failure (SPOF)", "Điểm lỗi đơn (chỉ cần 1 nút này hỏng là cả hệ thống sập).\nVí dụ: Running a single database without a standby replica creates a single point of failure."),
            (l43, "Regression", "/rɪˈɡreʃn/ — Lỗi tái phát làm hỏng một tính năng từng chạy tốt trước đây.\nVí dụ: Automated integration tests prevent regressions before every Friday release."),

            // Bài 5.1: Storytelling Kinh doanh & Trình bày Số liệu (10 thẻ)
            (l51, "BLUF (Bottom Line Up Front)", "Nguyên tắc đưa kết luận và đề xuất cốt lõi lên ngay đầu bài nói khi gặp lãnh đạo.\nVí dụ: Start with 'I recommend investing two weeks to cut cloud costs by 35%'."),
            (l51, "Surge / Soar", "/sɜːdʒ/ | /sɔːr/ — Tăng vọt mạnh mẽ.\nVí dụ: Daily active users surged by 40 percent immediately after the mobile release."),
            (l51, "Plummet / Plunge", "/ˈplʌmɪt/ | /plʌndʒ/ — Lao dốc, tụt giảm rất mạnh.\nVí dụ: Checkout error rates plummeted from 8% down to 0.2%."),
            (l51, "Plateau at...", "/ˈplætəʊ æt/ — Chững lại, đi ngang ổn định tại một mốc.\nVí dụ: After three months of rapid growth, weekly signups plateaued at 5,000."),
            (l51, "Percent (never plural 'percents')", "/pəˈsent/ — Phần trăm (KHÔNG BAO GIỜ thêm 's').\nVí dụ: Retention improved by 15 percent (sai: 15 percents)."),
            (l51, "An increase OF X% vs Increased BY X%", "Sau Danh từ dùng 'of'; sau Động từ dùng 'by'.\nVí dụ: There was an increase of 20% in traffic / Traffic increased by 20%."),
            (l51, "Funnel analytics & Drop off", "Phân tích phễu chuyển đổi & Tỷ lệ người dùng thoát giữa chừng.\nVí dụ: Funnel analytics showed that 60% of visitors drop off on slow search pages."),
            (l51, "Pay for itself", "Tự hoàn vốn (khoản tiết kiệm / lợi nhuận thu về lớn hơn chi phí bỏ ra).\nVí dụ: In dollar terms, this caching layer pays for itself within two months."),
            (l51, "Get / Give the green light", "Nhận được / Cấp sự phê duyệt chính thức để bắt đầu triển khai.\nVí dụ: Once the CFO reviews the ROI spec, we will get the green light for Sprint 12."),
            (l51, "The bottom line is that...", "Kết luận cốt lõi nhất cần nhớ là...\nVí dụ: The bottom line is that faster page speed directly drives higher retention."),

            // Bài 5.2: Phản biện Đa chiều, Khủng hoảng & Từ chối Khéo (10 thẻ)
            (l52, "L.A.E.R. Framework (Crisis De-escalation)", "Quy trình hạ nhiệt khủng hoảng: Listen -> Acknowledge -> Empathize -> Resolve.\nVí dụ: Always acknowledge urgency before jumping into technical explanations."),
            (l52, "Sincerely apologize for the disruption", "/dɪsˈrʌpʃn/ — Chân thành xin lỗi vì sự gián đoạn dịch vụ.\nVí dụ: I sincerely apologize for the disruption this has caused right before your board meeting."),
            (l52, "Top priority", "/tɒp praɪˈɒrəti/ — Ưu tiên hàng đầu không có việc gì xếp trước.\nVí dụ: Restoring your report export is our top priority right now."),
            (l52, "Workaround", "/ˈwɜːkəraʊnd/ — Giải pháp khắc phục tạm thời để khách hàng làm việc tiếp.\nVí dụ: While we deploy the permanent fix, here is a reliable workaround within 15 minutes."),
            (l52, "Keep someone posted", "Cập nhật tiến độ liên tục cho ai đó.\nVí dụ: I will keep you posted every twenty minutes until service is fully restored."),
            (l52, "The Positive No (Yes - No - Yes)", "Từ chối khéo: Công nhận giá trị -> Đặt giới hạn khách quan -> Đưa phương án thay thế.\nVí dụ: What we can do instead is deliver the CSV export Friday and the dashboard next week."),
            (l52, "Out of scope", "/aʊt əv skəʊp/ — Nằm ngoài phạm vi hợp đồng / sprint hiện tại.\nVí dụ: Custom SSO integration is currently out of scope for Phase 1."),
            (l52, "Play devil's advocate", "Đóng vai người phản biện trái chiều để kiểm tra lỗ hổng phương án.\nVí dụ: Just to play devil's advocate, what happens if traffic triples on Black Friday?"),
            (l52, "Despite + Noun/V-ing (NO 'of'!)", "/dɪˈspaɪt/ — Mặc dù (tuyệt đối KHÔNG viết 'Despite of'!).\nVí dụ: Despite the unexpected outage, we met our monthly SLA target."),
            (l52, "Valid concern", "/ˈvælɪd kənˈsɜːn/ — Mối lo ngại hoàn toàn hợp lý, chính đáng.\nVí dụ: Data residency is a valid concern; however, let's review our encryption controls."),

            // Bài 5.3: Review & Nâng cấp Email / Prompt Tự nhiên (10 thẻ)
            (l53, "Just a quick heads-up that...", "/ˈhedz ʌp/ — Mở đầu báo trước tự nhiên (thay cho 'Please be informed that...').\nVí dụ: Just a quick heads-up that the staging server will restart in ten minutes."),
            (l53, "Wrap up", "/ræp ʌp/ — Hoàn tất, chốt lại công việc hoặc cuộc họp.\nVí dụ: Quick update: we wrapped up the integration tests yesterday."),
            (l53, "Follow up on...", "Nhắc lại / theo dõi tiếp một vấn đề đã trao đổi trước đó.\nVí dụ: I am following up on my email regarding production API credentials."),
            (l53, "Share your thoughts (replaces 'Kindly revert')", "Mời phản hồi tự nhiên (KHÔNG dùng 'revert' nghĩa là trả lời!).\nVí dụ: Could you take a look at the draft and share your thoughts by Thursday?"),
            (l53, "I really appreciate... (NOT 'I am very appreciate')", "/əˈpriːʃieɪt/ — Tôi thực sự trân trọng ('appreciate' là động từ thường!).\nVí dụ: I really appreciate your prompt feedback on this pull request."),
            (l53, "Discuss + Object (NO 'about'!)", "/dɪˈskʌs/ — Thảo luận về việc gì (KHÔNG dùng 'discuss about'!).\nVí dụ: Let's discuss the prompt constraints in our afternoon sync."),
            (l53, "Look forward to + V-ing", "Rất mong đợi điều gì ('to' là giới từ nên đi kèm V-ing).\nVí dụ: I look forward to hearing your thoughts before Friday's release."),
            (l53, "R-C-T-C-F Prompt Formula", "Khung viết Prompt AI 5 bước: Role - Context - Task - Constraints - Format.\nVí dụ: Act as a senior SRE (Role), analyze this log (Task), and output a Markdown table (Format)."),
            (l53, "Concise", "/kənˈsaɪs/ — Súc tích, ngắn gọn nhưng đầy đủ ý.\nVí dụ: Keep your executive summary concise and free of robotic filler."),
            (l53, "Actionable", "/ˈækʃənəbl/ — Thiết thực, có thể bắt tay vào thực hiện ngay.\nVí dụ: Ask the AI for three actionable optimization steps instead of vague theory.")
        };

        await SeedFlashcardsIdempotentlyAsync(
            db,
            domainKey: "en-exp",
            tag: tag,
            cardType: "basic",
            sourceTitleResolver: id => id switch
            {
                _ when id == l41 => "Bài 4.1: Xuất nhập cảnh, Khách sạn & Chuyến bay",
                _ when id == l42 => "Bài 4.2: Phỏng vấn Kỹ thuật & STAR Method",
                _ when id == l43 => "Bài 4.3: Code Review, Bug Report & Kiến trúc",
                _ when id == l51 => "Bài 5.1: Storytelling Dữ liệu & Thuyết phục Lãnh đạo",
                _ when id == l52 => "Bài 5.2: Khủng hoảng Khách hàng & Từ chối Khéo",
                _ => "Bài 5.3: Email & Prompt Tiếng Anh Tự nhiên"
            },
            specs: specs,
            now: now);
    }

    #endregion
}
