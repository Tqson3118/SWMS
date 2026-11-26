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
/// Expands Japanese N5 and Chinese HSK1 courses from 3 Chapters / 6 Lessons to 5 Chapters / 12 Lessons each.
/// Strictly additive and idempotent: never modifies Chapters 1..3, Lessons 1.1..3.2, or CurrentVersion == 2 user edits.
/// </summary>
public static class ExpandedCjkCoursesContent
{
    public const string UpgradeMarkerV1 = "<!-- studxy-cjk-v1 -->";
    public const string UpgradeMarkerV2 = "<!-- studxy-cjk-v2 -->";
    public const string UpgradeMarkerV3 = "<!-- studxy-cjk-v3 -->";

    // Deterministic IDs for Japanese N5 Chapters 4..5 & Lessons 4.1..5.3
    public static readonly Guid JapaneseChapter4Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:ja:chapter:4");
    public static readonly Guid JapaneseChapter5Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:ja:chapter:5");
    public static readonly Guid JapaneseLesson41Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:ja:lesson:4.1");
    public static readonly Guid JapaneseLesson42Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:ja:lesson:4.2");
    public static readonly Guid JapaneseLesson43Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:ja:lesson:4.3");
    public static readonly Guid JapaneseLesson51Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:ja:lesson:5.1");
    public static readonly Guid JapaneseLesson52Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:ja:lesson:5.2");
    public static readonly Guid JapaneseLesson53Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:ja:lesson:5.3");

    // Deterministic IDs for Chinese HSK1 Chapters 4..5 & Lessons 4.1..5.3
    public static readonly Guid ChineseChapter4Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:zh:chapter:4");
    public static readonly Guid ChineseChapter5Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:zh:chapter:5");
    public static readonly Guid ChineseLesson41Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:zh:lesson:4.1");
    public static readonly Guid ChineseLesson42Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:zh:lesson:4.2");
    public static readonly Guid ChineseLesson43Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:zh:lesson:4.3");
    public static readonly Guid ChineseLesson51Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:zh:lesson:5.1");
    public static readonly Guid ChineseLesson52Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:zh:lesson:5.2");
    public static readonly Guid ChineseLesson53Id = SeedDataInitializer.CreateDeterministicGuid("swms:cjk:zh:lesson:5.3");

    private static readonly JsonSerializerOptions CjkJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private sealed record CjkLessonMeta(
        Guid CourseId,
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

    private sealed record ShItem(string Id, string Difficulty, string Topic, string Sentence, string Reading, string MeaningVi, string[] KeyPhrases);
    private sealed record DiItem(string Id, string Difficulty, string Topic, string Sentence, string Reading, string MeaningVi, string[] WordChips, string[] Distractors);
    private sealed record PrItem(string Id, string Difficulty, string Topic, string Sentence, string Reading, string MeaningVi, string[] TargetWords, string SpeakingPrompt, string[] SpeakingOutline, string SampleAnswer, string[] KeyPhrases);
    private sealed record WmNode(string Text, string Reading, string MeaningVi, string Example);
    private sealed record WmEx(string Sentence, string Reading, string MeaningVi);
    private sealed record WmItem(string Id, string Difficulty, string Title, string CentralWord, string Reading, string MeaningVi, WmNode[] Collocations, WmNode[] PhrasalVerbs, WmNode[] Synonyms, WmEx[] ContextExamples);
    private sealed record ClBlank(string[] AcceptedAnswers, string HintVi, string ExplanationVi, bool AllowKanaEquivalence = false);
    private sealed record ClItem(string Id, string Difficulty, string Title, string PromptVi, string Template, ClBlank[] Blanks, string FullSentence, string Reading, string MeaningVi, string ExplanationVi);
    private sealed record TrItem(string Id, string Difficulty, string Title, string SourceVi, string[] HintKeywords, string[] AcceptedAnswers, string SampleAnswer, string Reading, string GrammarNoteVi, bool AllowKanaEquivalence = false);
    private sealed record GfItem(string Id, string Difficulty, string Title, string IncorrectSentence, string ErrorTypeHintVi, string CorrectedSentence, string[] AcceptedAnswers, string Reading, string MeaningVi, string ExplanationVi, bool AllowKanaEquivalence = false);
    private sealed record RpItem(string Id, string Difficulty, string Title, string ScenarioVi, string AiRole, string AiPrompt, string AiPromptReading, string AiPromptVi, string UserGoalVi, string[] ExpectedKeywords, string[] SampleResponses, string SampleReading, int MinWords = 6);

    private sealed record QuizQuestionSpec(
        string ItemType,
        string Question,
        string? OptionA,
        string? OptionB,
        string? OptionC,
        string? OptionD,
        string CorrectSolution,
        string ExplanationVi
    );

    public static async Task EnsureSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var hasJapaneseCourse = await db.Courses.AnyAsync(c => c.Id == SeedDataInitializer.JapaneseCourseId);
        var hasChineseCourse = await db.Courses.AnyAsync(c => c.Id == SeedDataInitializer.ChineseCourseId);

        if (!hasJapaneseCourse && !hasChineseCourse)
        {
            return;
        }

        if (hasJapaneseCourse)
        {
            await EnsureCourseExpansionAsync(
                db,
                courseId: SeedDataInitializer.JapaneseCourseId,
                chapters: new[]
                {
                    (JapaneseChapter4Id, "Chương 4: Tính Từ, Gia Đình, Sở Thích & Diễn Đạt Mức Độ (N5+)", 4),
                    (JapaneseChapter5Id, "Chương 5: Động Từ Thể Te (〜て形), Xin Phép, Năng Lực & Kế Hoạch Tương Lai", 5)
                },
                lessons: GetJapaneseExpandedLessons(),
                activities: GetJapaneseExpandedActivities(SeedDataInitializer.JapaneseCourseId, now),
                now: now);
        }

        if (hasChineseCourse)
        {
            await EnsureCourseExpansionAsync(
                db,
                courseId: SeedDataInitializer.ChineseCourseId,
                chapters: new[]
                {
                    (ChineseChapter4Id, "Chương 4: Thời Tiết, Gia Đình, Công Việc & Miêu Tả Cảm Xúc", 4),
                    (ChineseChapter5Id, "Chương 5: Trợ Động Từ Năng Nguyện, Trải Nghiệm & Giao Tiếp Đời Sống Nâng Cao", 5)
                },
                lessons: GetChineseExpandedLessons(),
                activities: GetChineseExpandedActivities(SeedDataInitializer.ChineseCourseId, now),
                now: now);
        }

        await EnsureExpandedQuizzesAsync(db, hasJapaneseCourse, hasChineseCourse, now);
        await EnsureExpandedFlashcardsAsync(db, hasJapaneseCourse, hasChineseCourse, now);
    }

    private static async Task EnsureCourseExpansionAsync(
        SwmsDbContext db,
        Guid courseId,
        (Guid Id, string Title, int SortOrder)[] chapters,
        List<CjkLessonMeta> lessons,
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
            if (existingLessons.TryGetValue(meta.LessonId, out var existing))
            {
                if (existing.CurrentVersion == 1 && !(existing.ContentMarkdown ?? string.Empty).Contains(UpgradeMarkerV3, StringComparison.Ordinal))
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
                    attrs = new { level = 1, block_id = SeedDataInitializer.CreateDeterministicGuid($"swms:cjk-ast:{lessonId:N}:h1").ToString() },
                    content = new[] { new { type = "text", text = title } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = SeedDataInitializer.CreateDeterministicGuid($"swms:cjk-ast:{lessonId:N}:p1").ToString() },
                    content = new[] { new { type = "text", text = p1 } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = SeedDataInitializer.CreateDeterministicGuid($"swms:cjk-ast:{lessonId:N}:p2").ToString() },
                    content = new[] { new { type = "text", text = p2 } }
                }
            }
        };
        return JsonSerializer.Serialize(ast, CjkJsonOptions);
    }

    private static PracticeActivity MakeActivity(Guid courseId, Guid lessonId, int orderIndex, string type, string title, string description, object config, DateTimeOffset now)
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
            ConfigJson = JsonSerializer.Serialize(config, CjkJsonOptions),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static List<PracticeActivity> BuildLessonPack(
        Guid courseId,
        Guid lessonId,
        string lang,
        string lessonShortName,
        ShItem[] shadowing,
        DiItem[] dictation,
        PrItem[] pronunciation,
        WmItem[] wordMaps,
        ClItem[] cloze,
        TrItem[] translate,
        GfItem[] grammarFix,
        RpItem[] roleplay,
        DateTimeOffset now)
    {
        var isJa = lang == "ja-JP";
        var langName = isJa ? "tiếng Nhật" : "tiếng Trung";
        var translateTitle = isJa ? $"Dịch Việt → Nhật ({lessonShortName})" : $"Dịch Việt → Trung ({lessonShortName})";

        return new List<PracticeActivity>
        {
            MakeActivity(courseId, lessonId, 1, "shadowing",
                $"Shadowing {lessonShortName}: {shadowing.Length} câu đa cấp độ ({lang})",
                $"Nghe giọng mẫu {langName} và đọc đuổi theo đúng nhịp điệu, trường âm/thanh điệu từ Dễ đến Khó.",
                new
                {
                    lang,
                    difficulty = "medium",
                    items = shadowing.Select(s => new
                    {
                        id = s.Id,
                        lang,
                        difficulty = s.Difficulty,
                        topic = s.Topic,
                        sentence = s.Sentence,
                        reading = s.Reading,
                        ipa = s.Reading,
                        meaningVi = s.MeaningVi,
                        keyPhrases = s.KeyPhrases
                    })
                }, now),

            MakeActivity(courseId, lessonId, 2, "dictation",
                $"Dictation {lessonShortName}: Nghe & ghép/chép {dictation.Length} câu {langName}",
                $"Nghe câu mẫu {langName} và ghép thẻ cụm từ hoặc gõ lại chính xác từng ký tự.",
                new
                {
                    lang,
                    difficulty = "medium",
                    items = dictation.Select(d => new
                    {
                        id = d.Id,
                        lang,
                        difficulty = d.Difficulty,
                        topic = d.Topic,
                        sentence = d.Sentence,
                        reading = d.Reading,
                        ipa = d.Reading,
                        meaningVi = d.MeaningVi,
                        wordChips = d.WordChips,
                        distractors = d.Distractors
                    })
                }, now),

            MakeActivity(courseId, lessonId, 3, "pronunciation",
                $"Chấm phát âm {lessonShortName}: {pronunciation.Length} mẫu câu ({lang})",
                $"Đọc to bằng giọng nói ({lang}) để nhận phản hồi chi tiết từng ký tự và gợi ý sửa lỗi phát âm thường gặp.",
                new
                {
                    lang,
                    difficulty = "medium",
                    items = pronunciation.Select(p => new
                    {
                        id = p.Id,
                        lang,
                        difficulty = p.Difficulty,
                        topic = p.Topic,
                        sentence = p.Sentence,
                        reading = p.Reading,
                        ipa = p.Reading,
                        meaningVi = p.MeaningVi,
                        targetWords = p.TargetWords,
                        speakingPrompt = p.SpeakingPrompt,
                        speakingOutline = p.SpeakingOutline,
                        sampleAnswer = p.SampleAnswer,
                        keyPhrases = p.KeyPhrases
                    })
                }, now),

            MakeActivity(courseId, lessonId, 4, "word_map",
                $"Sơ đồ từ vựng {lessonShortName}: {wordMaps.Length} từ khóa trọng tâm",
                $"Khám phá mạng lưới từ vựng {langName}, phiên âm, mẫu câu mở rộng và lưu vào Flashcard FSRS.",
                new
                {
                    lang,
                    difficulty = "medium",
                    items = wordMaps.Select(w => new
                    {
                        id = w.Id,
                        lang,
                        difficulty = w.Difficulty,
                        title = w.Title,
                        centralWord = w.CentralWord,
                        reading = w.Reading,
                        ipa = w.Reading,
                        meaningVi = w.MeaningVi,
                        collocations = w.Collocations.Select(c => new { text = c.Text, reading = c.Reading, ipa = c.Reading, meaningVi = c.MeaningVi, example = c.Example }),
                        phrasalVerbs = w.PhrasalVerbs.Select(c => new { text = c.Text, reading = c.Reading, ipa = c.Reading, meaningVi = c.MeaningVi, example = c.Example }),
                        synonyms = w.Synonyms.Select(c => new { text = c.Text, reading = c.Reading, ipa = c.Reading, meaningVi = c.MeaningVi, example = c.Example }),
                        contextExamples = w.ContextExamples.Select(e => new { sentence = e.Sentence, reading = e.Reading, meaningVi = e.MeaningVi })
                    })
                }, now),

            MakeActivity(courseId, lessonId, 5, "cloze",
                $"Điền chỗ trống (Cloze) {lessonShortName}: {cloze.Length} bài",
                $"Tự nhớ trợ từ, từ vựng và mẫu câu {langName} để điền chính xác vào chỗ trống.",
                new
                {
                    lang,
                    items = cloze.Select(c => new
                    {
                        id = c.Id,
                        lang,
                        difficulty = c.Difficulty,
                        title = c.Title,
                        promptVi = c.PromptVi,
                        template = c.Template,
                        blanks = c.Blanks.Select(b => new
                        {
                            acceptedAnswers = b.AcceptedAnswers,
                            allowKanaEquivalence = b.AllowKanaEquivalence,
                            hintVi = b.HintVi,
                            explanationVi = b.ExplanationVi
                        }),
                        fullSentence = c.FullSentence,
                        reading = c.Reading,
                        meaningVi = c.MeaningVi,
                        explanationVi = c.ExplanationVi
                    })
                }, now),

            MakeActivity(courseId, lessonId, 6, "translate",
                $"{translateTitle}: {translate.Length} câu thực hành",
                $"Luyện phản xạ dịch câu tiếng Việt sang {langName} chuẩn ngữ pháp và tự nhiên.",
                new
                {
                    lang,
                    items = translate.Select(t => new
                    {
                        id = t.Id,
                        lang,
                        difficulty = t.Difficulty,
                        title = t.Title,
                        sourceVi = t.SourceVi,
                        hintKeywords = t.HintKeywords,
                        acceptedAnswers = t.AcceptedAnswers,
                        sampleAnswer = t.SampleAnswer,
                        reading = t.Reading,
                        allowKanaEquivalence = t.AllowKanaEquivalence,
                        grammarNoteVi = t.GrammarNoteVi
                    })
                }, now),

            MakeActivity(courseId, lessonId, 7, "grammar_fix",
                $"Sửa lỗi ngữ pháp {lessonShortName}: {grammarFix.Length} câu hay sai",
                $"Phát hiện và sửa các lỗi trợ từ, chia thể hoặc trật tự từ {langName} thường gặp ở người Việt.",
                new
                {
                    lang,
                    items = grammarFix.Select(g => new
                    {
                        id = g.Id,
                        lang,
                        difficulty = g.Difficulty,
                        title = g.Title,
                        incorrectSentence = g.IncorrectSentence,
                        errorTypeHintVi = g.ErrorTypeHintVi,
                        correctedSentence = g.CorrectedSentence,
                        acceptedAnswers = g.AcceptedAnswers,
                        reading = g.Reading,
                        allowKanaEquivalence = g.AllowKanaEquivalence,
                        meaningVi = g.MeaningVi,
                        explanationVi = g.ExplanationVi
                    })
                }, now),

            MakeActivity(courseId, lessonId, 8, "roleplay",
                $"Nhập vai hội thoại {lessonShortName}: {roleplay.Length} tình huống thực tế",
                $"Phản hồi trực tiếp bằng giọng nói ({lang}) hoặc gõ phím; hỗ trợ đánh giá bằng AI hoặc chấm theo từ khóa CJK offline.",
                new
                {
                    lang,
                    items = roleplay.Select(r => new
                    {
                        id = r.Id,
                        lang,
                        difficulty = r.Difficulty,
                        title = r.Title,
                        scenarioVi = r.ScenarioVi,
                        aiRole = r.AiRole,
                        aiPrompt = r.AiPrompt,
                        aiPromptReading = r.AiPromptReading,
                        aiPromptVi = r.AiPromptVi,
                        userGoalVi = r.UserGoalVi,
                        expectedKeywords = r.ExpectedKeywords,
                        sampleResponses = r.SampleResponses,
                        sampleReading = r.SampleReading,
                        minWords = r.MinWords
                    })
                }, now)
        };
    }

    // =========================================================================
    // KHÓA 1: TIẾNG NHẬT SƠ CẤP (N5+) — 6 BÀI LÝ THUYẾT CHƯƠNG 4 & 5 (>= 5.500 KÝ TỰ/BÀI)
    // =========================================================================

    private static List<CjkLessonMeta> GetJapaneseExpandedLessons() => new()
    {
        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, JapaneseChapter4Id, JapaneseLesson41Id,
            "Bài 4.1: Tính Từ Đuôi -i & -na, Miêu Tả Sự Vật & Thời Tiết",
            "ja-n5-tinh-tu-i-na-mieu-ta-thoi-tiet", 1, 40,
            @"# Bài 4.1: Tính Từ Đuôi -i & -na, Miêu Tả Sự Vật & Thời Tiết

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Bước sang Chương 4, bạn sẽ nâng cấp khả năng diễn đạt từ những câu danh từ đơn giản sang **miêu tả cảm xúc, tính chất đồ vật, con người, món ăn và thời tiết bốn mùa tại Nhật Bản**. Sau bài học này, bạn sẽ:
- Phân biệt rạch ròi hai nhóm tính từ cốt lõi trong tiếng Nhật: **Tính từ đuôi い (いけいようし — I-keiyōshi)** và **Tính từ đuôi な (なけいようし — Na-keiyōshi)**.
- Chia chính xác 4 dạng của cả hai loại tính từ: **Hiện tại khẳng định**, **Hiện tại phủ định**, **Quá khứ khẳng định**, và **Quá khứ phủ định** ở thể lịch sự (`〜です`).
- Nắm vững ngoại lệ kinh điển của tính từ **いい (tốt)** khi biến đổi sang `よくない`, `よかった`, `よくなかった` và các tính từ đuôi `-i` giả dạng (`きれい`, `ゆうめい`, `げんき`, `すき`).
- Sử dụng thành thạo phó từ chỉ mức độ (`とても`, `すごく`, `ちょっと`, `あまり〜ない`, `ぜんぜん〜ない`) để làm câu nói tự nhiên như người bản xứ.
- Vận dụng từ vựng miêu tả **thời tiết, nhiệt độ, bốn mùa** (`あつい`, `さむい`, `すずしい`, `あたたかい`, `はれ`, `あめ`) và cấu trúc nối hai vế đối lập bằng `が` hoặc nối tương đồng bằng `〜くて / 〜で`.
- Tránh triệt để 4 lỗi chia tính từ kinh điển mà người Việt thường mắc phải khi nói và làm bài thi JLPT N5.

---

## 2. 🌱 Bản chất 2 loại Tính từ & Bảng chia 4 thì chuẩn JLPT N5

### 2.1. Vì sao tiếng Nhật chia Tính từ thành 2 nhóm?
Khác với tiếng Việt (tính từ đứng nguyên, chỉ thêm ""không"", ""đã"", ""rất""), tính từ tiếng Nhật **tự biến đổi phần đuôi** để thể hiện thì quá khứ và thể phủ định.

1. **Tính từ đuôi -i (い形容詞)**:
   - Luôn kết thúc bằng chữ Hiragana `い` ở dạng từ điển (khi không có danh từ theo sau). Ví dụ: `おおきい` (to), `ちいさい` (nhỏ), `あたらしい` (mới), `おいしい` (ngon), `さむい` (lạnh).
   - Khi đứng trước danh từ để bổ nghĩa, giữ nguyên `い`: `おおきい いぬ` *(con chó to)*, `おいしい ラーメン` *(mì ramen ngon)*.
   - Bản thân đuôi `い` tự mang nghĩa khẳng định; `です` đặt cuối chỉ để tạo **sắc thái lịch sự**.

2. **Tính từ đuôi -na (な形容詞)**:
   - Ở dạng từ điển KHÔNG có `な` đi kèm (ví dụ: `しずか` yên tĩnh, `にぎやか` náo nhiệt, `べんり` tiện lợi, `ひま` rảnh rỗi).
   - **CẢNH BÁO NGOẠI LỆ VÀNG**: Một số tính từ kết thúc bằng âm `い` nhưng thực chất là **Tính từ đuôi -na** vì `い` nằm trong âm Hán hoặc phần gốc từ:
     - `きれい (綺麗 - đẹp, sạch sẽ)` → **ĐUÔI -NA!** (`きれいな はな`, không nói `きれいい はな`)
     - `ゆうめい (有名 - nổi tiếng)` → **ĐUÔI -NA!** (`ゆうめいな まち`)
     - `きらい (嫌い - ghét)` → **ĐUÔI -NA!** (`きらいな たべもの`)
   - Khi đứng trước danh từ, **BẮT BUỘC phải chèn `な`**: `しずかな まち` *(thành phố yên tĩnh)*, `きれいな うみ` *(biển đẹp)*.

### 2.2. Bảng biến đổi 4 dạng chuẩn (Thể Lịch Sự)

| Dạng câu | Tính từ đuôi -i (ví dụ: たかい - cao/đắt) | Ngoại lệ: いい (tốt) | Tính từ đuôi -na (ví dụ: ひま - rảnh) |
|---|---|---|---|
| **Hiện tại (+) Khẳng định** | `たかい です` | `いい です` | `ひま です` |
| **Hiện tại (-) Phủ định** | Bỏ `い` + `くないです` → `たかくない です` *(hoặc `くありません`)* | `よくない です` | `ひま ではありません` *(hoặc `じゃありません`)* |
| **Quá khứ (+) Khẳng định** | Bỏ `い` + `かったです` → `たかかった です` | `よかった です` | `ひま でした` |
| **Quá khứ (-) Phủ định** | Bỏ `い` + `くなかったです` → `たかくなかった です` | `よくなかった です` | `ひま ではありませんでした` |

> [!IMPORTANT]
> **Quy tắc bất biến cho `いい (tốt)`**: Trước khi chia phủ định hoặc quá khứ, PHẢI chuyển `いい` về gốc cổ **`よい (yoi)`** rồi mới bỏ `い`:
> `いい` → `よい` → `よくないです` (không tốt) / `よかったです` (đã tốt / may quá) / `よくなかったです` (đã không tốt). **TUYỆT ĐỐI KHÔNG BAO GIỜ nói `いくない` hay `いかった`!**

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **おおきい (大きい)** | ookii | Đại | To, lớn | この へやは おおきいです。 *(Căn phòng này rộng lớn.)* |
| **ちいさい (小さい)** | chiisai | Tiểu | Nhỏ, bé | ちいさい ねこが います。 *(Có một chú mèo nhỏ.)* |
| **あたらしい (新しい)** | atarashii | Tân | Mới | あたらしい スマホを かいました。 *(Tôi đã mua điện thoại mới.)* |
| **ふるい (古い)** | furui | Cổ | Cũ (đồ vật, không dùng cho người) | これは ふるい じしょです。 *(Đây là cuốn từ điển cũ.)* |
| **あつい (暑い / 熱い)** | atsui | Thử / Nhiệt | Nóng (thời tiết / vật chạm vào) | きょうは とても あついですね。 *(Hôm nay trời nóng quá nhỉ.)* |
| **さむい (寒い)** | samui | Hàn | Lạnh (thời tiết, không khí) | ふゆの にほんは さむいです。 *(Mùa đông ở Nhật rất lạnh.)* |
| **つめたい (冷たい)** | tsumetai | Lãnh | Lạnh (đồ uống, vật chạm vào) | つめたい おちゃを ください。 *(Cho tôi xin trà đá lạnh.)* |
| **あたたかい (暖かい)** | atatakai | Noãn | Ấm áp | はるは あたたかくて きもちがいいです。 *(Mùa xuân ấm áp dễ chịu.)* |
| **すずしい (涼しい)** | suzushii | Lương | Mát mẻ | あきは すずしいですね。 *(Mùa thu mát mẻ quá nhỉ.)* |
| **むずかしい (難しい)** | muzukashii | Nan | Khó | かんじは すこし むずかしいです。 *(Chữ Hán hơi khó một chút.)* |
| **やさしい (易しい / 優しい)** | yasashii | Dị / Ưu | Dễ (bài tập) / Hiền lành, tốt bụng | せんせいは とても やさしいです。 *(Thầy giáo rất hiền lành.)* |
| **きれい (綺麗)** | kirei (na) | Lệ | Đẹp, sạch sẽ *(đuôi -na)* | さくらは とても きれいですね。 *(Hoa anh đào đẹp thật đấy nhỉ.)* |
| **しずか (静か)** | shizuka (na) | Tĩnh | Yên tĩnh *(đuôi -na)* | この こうえんは しずかです。 *(Công viên này yên tĩnh.)* |
| **にぎやか (賑やか)** | nigiyaka (na) | Chẩn | Náo nhiệt, đông vui *(đuôi -na)* | しんじゅくは にぎやかな まちです。 *(Shinjuku là khu phố náo nhiệt.)* |
| **べんり (便利)** | benri (na) | Tiện Lợi | Tiện lợi *(đuôi -na)* | でんしゃは とても べんりです。 *(Tàu điện rất tiện lợi.)* |
| **てんき (天気)** | tenki | Thiên Khí | Thời tiết | あしたの てんきは はれです。 *(Thời tiết ngày mai trời nắng.)* |
| **さくら (桜)** | sakura | Anh | Hoa anh đào | はるに さくらが さきます。 *(Vào mùa xuân hoa anh đào nở.)* |

---

## 4. Ngữ pháp mở rộng: Phó từ mức độ, Nối tính từ & Câu đối lập

### 4.1. Thang đo Phó từ chỉ mức độ (Rất → Hơi → Không lắm → Hoàn toàn không)
- **とても / すごく** *(Rất / Cực kỳ)* + Khẳng định: `きょうは とても さむいです。` *(Hôm nay trời rất lạnh.)*
- **すこし / ちょっと** *(Một chút / Hơi)* + Khẳng định (thường dùng khi chê nhẹ lịch sự): `この かばんは ちょっと たかいです。` *(Cái túi này hơi đắt một chút.)*
- **あまり + Phủ định** *(Không ... lắm)*: **BẮT BUỘC đi với dạng phủ định!**
  - `テストは あまり むずかしくなかったです。` *(Bài kiểm tra đã không khó lắm.)*
- **ぜんぜん + Phủ định** *(Hoàn toàn không ... chút nào)*: **BẮT BUỘC đi với dạng phủ định!**
  - `この まちは ぜんぜん にぎやかでは ありません。` *(Thị trấn này hoàn toàn không náo nhiệt chút nào.)*

### 4.2. Nối hai tính từ & Hai vế tương phản (`〜が`)
- **Nối 2 tính từ tương đồng (và, lại còn)**:
  - Tính từ đuôi `-i`: Bỏ `い` + `くて` → `おおきくて おいしい` *(to và ngon)*.
  - Tính từ đuôi `-na`: Thêm `で` → `きれいで しずか` *(đẹp và yên tĩnh)*.
  - Ví dụ: `この レストランは やすくて おいしいです。` *(Nhà hàng này vừa rẻ vừa ngon.)*
- **Nối 2 vế trái ngược bằng `が` (nhưng)**:
  - `にほんごは おもしろいですが、かんじが むずかしいです。` *(Tiếng Nhật thú vị nhưng chữ Hán khó.)*

### 4.3. Hỏi cảm nhận & Đặc điểm: `どうですか` và `どんな + N ですか`
- **`[Chủ đề] は どうですか`** *(... thế nào?)* — Hỏi cảm nhận chung:
  - `にほんの せいかつは どうですか。` — `とても たのしいです。` *(Cuộc sống ở Nhật thế nào? — Rất vui.)*
- **`どんな + Danh từ ですか`** *(Là [danh từ] như thế nào?)* — Yêu cầu trả lời kèm tính từ bổ nghĩa:
  - `ハノイは どんな まちですか。` — `にぎやかで きれいな まちです。` *(Hà Nội là thành phố thế nào? — Là thành phố náo nhiệt và xinh đẹp.)*

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: きのうは さむいでした。 (Ghép trực tiếp でした vào sau tính từ đuôi -i)
   ⭕ ĐÚNG: きのうは さむかったです。 (Bỏ い thêm かったです)
   💡 GIẢI THÍCH: Tuyệt đối không dùng でした với tính từ đuôi -i! Chỉ tính từ đuôi -na và danh từ mới dùng でした.

2. ❌ SAI: さくらは きれいい です / きれいくない です (Nhầm きれい là tính từ đuôi -i)
   ⭕ ĐÚNG: さくらは きれいです / きれいでは ありません。 (きれい là tính từ đuôi -na)
   💡 GIẢI THÍCH: Nhóm ""đuôi i nhưng là tính từ -na"" gồm: きれい, ゆうめい, きらい, げんき (không có い).

3. ❌ SAI: この コーヒーは あまり おいしいです。 (Dùng あまり với câu khẳng định)
   ⭕ ĐÚNG: この コーヒーは あまり おいしくないです。 (あまり và ぜんぜん luôn đi kèm Phủ định)
   💡 GIẢI THÍCH: Trong tiếng Việt nói ""rất không ngon"" hoặc ""không ngon lắm"", tiếng Nhật quy định あまり/ぜんぜん bắt buộc phải khóa đuôi phủ định.

4. ❌ SAI: てんきが いいくない です / いいかった です (Chia trực tiếp từ いい)
   ⭕ ĐÚNG: てんきが よくないです / よかったです。
   💡 GIẢI THÍCH: Tính từ いい luôn đổi về gốc よ trước khi thêm くない, かった, くなかった.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Buổi trưa tại căn tin trường đại học ở Tokyo. Chị Sato (người Nhật) hỏi thăm Minh về thời tiết ở Hà Nội và cảm nhận về cuộc sống ở Nhật.

- **Sato**: ミンさん、にほんの せいかつは どうですか。
  - *Romaji*: Min-san, nihon no seikatsu wa dou desu ka?
  - *Phân tích*: `せいかつ` (cuộc sống) + `どうですか` (thế nào?).
  - *Dịch*: Minh ơi, cuộc sống ở Nhật thế nào rồi?

- **Minh**: とても たのしいです。でも、ふゆは ちょっと さむいですね。
  - *Romaji*: Totemo tanoshii desu. Demo, fuyu wa chotto samui desu ne.
  - *Phân tích*: `とても たのしい` (rất vui) + `でも` (nhưng) + `ふゆ` (mùa đông) + `ちょっと さむい` (hơi lạnh).
  - *Dịch*: Rất vui ạ. Nhưng mà mùa đông hơi lạnh chị nhỉ.

- **Sato**: そうですね。ベトナムの ふゆは あまり さむくないですか。
  - *Romaji*: Sou desu ne. Betonamu no fuyu wa amari samukunai desu ka?
  - *Phân tích*: `あまり さむくない` (không lạnh lắm).
  - *Dịch*: Đúng rồi nhỉ. Mùa đông ở Việt Nam không lạnh lắm à em?

- **Minh**: はい、ホーチミンは いちねんじゅう あついですが、ハノイの ふゆは すこし さむいですよ。
  - *Romaji*: Hai, Hoochimin wa ichinenjuu atsui desu ga, Hanoi no fuyu wa sukoshi samui desu yo.
  - *Phân tích*: `いちねんじゅう` (quanh năm) + `あついですが` (nóng nhưng mà) + `すこし さむい` (hơi lạnh).
  - *Dịch*: Vâng, TP.HCM thì nóng quanh năm, nhưng mùa đông ở Hà Nội thì hơi lạnh đấy ạ.

- **Sato**: へえ、そうなんですか。きょうの テストは どうでしたか。
  - *Romaji*: Hee, sou nan desu ka. Kyou no tesuto wa dou deshita ka?
  - *Phân tích*: `どうでしたか` (quá khứ của どうですか: đã thế nào?).
  - *Dịch*: Ồ thế à. Thế bài kiểm tra hôm nay làm thế nào?

- **Minh**: ぜんぜん むずかしくなかったです！とても やすかったですよ。
  - *Romaji*: Zenzen muzukashikunakatta desu! Totemo yasashikatta desu yo.
  - *Phân tích*: `ぜんぜん むずかしくなかった` (hoàn toàn không khó chút nào) + `やすかった` (dễ: やさしかった).
  - *Dịch*: Hoàn toàn không khó chút nào ạ! Đề dễ lắm chị ơi!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện biến đổi tính từ
1. Chia tính từ `あつい` (nóng) sang Quá khứ phủ định lịch sự *(Hôm qua đã không nóng)*.
2. Chia tính từ `しずか` (yên tĩnh) sang Quá khứ khẳng định và Hiện tại phủ định.
3. Nối 2 tính từ vào 1 câu: `この まち` (Thành phố này) + `きれい` (đẹp) + `にぎやか` (náo nhiệt).
4. Sửa lỗi sai trong câu: `きのうの えいがは あまり おもしろかったです。`

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: `きのうは あつくなかったです。` *(Bỏ い thêm くなかったです).*
2. **Câu 2**:
   - Quá khứ khẳng định: `しずかでした。`
   - Hiện tại phủ định: `しずかではありません` *(hoặc `しずかじゃありません`)*.
3. **Câu 3**: `この まちは きれいで、にぎやかです。` *(Vì きれい là tính từ đuôi -na nên nối bằng で).*
4. **Câu 4**: Vì có `あまり`, vế sau bắt buộc phải phủ định: `きのうの えいがは あまり おもしろくなかったです。`
5. **Câu 5 trắc nghiệm**: Cụm nào viết ĐÚNG cho ""Cô giáo tốt bụng""?
   - *Đáp án*: `やさしい せんせい` *(Tính từ đuôi -i đứng trước danh từ giữ nguyên い; tính từ đuôi -na như きれい mới thêm な).*

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Hãy bấm ngay vào **Phòng Thực Hành (Shadowing / Cloze / Grammar Fix)** bên dưới để luyện phản xạ chia quá khứ `〜かった / 〜くなかった` và phân biệt `きれい` với `おおきい` chỉ trong 5 phút!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Làm chủ tính từ đuôi -i và đuôi -na cùng 4 dạng biến đổi hiện tại/quá khứ, khẳng định/phủ định chuẩn N5.",
            "Thành thạo từ vựng miêu tả thời tiết bốn mùa, phó từ mức độ (とても/あまり/ぜんぜん) và cấu trúc nối tính từ."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, JapaneseChapter4Id, JapaneseLesson42Id,
            "Bài 4.2: Gia Đình, Số Đếm Người/Đồ Vật & Biểu Đạt Nguyên Nhân (〜から)",
            "ja-n5-gia-dinh-so-dem-nguyen-nhan-kara", 2, 40,
            @"# Bài 4.2: Gia Đình, Số Đếm Người/Đồ Vật & Biểu Đạt Nguyên Nhân (〜から)

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Trong văn hóa Nhật Bản, cách bạn gọi tên thành viên trong gia đình mình và gia đình người khác hoàn toàn khác nhau — đây là kiến thức nền tảng của kính ngữ sơ cấp và xuất hiện dày đặc trong đề thi JLPT N5. Bài học này giúp bạn:
- Phân biệt chuẩn xác hai hệ thống từ vựng **Gia đình mình (Khiêm nhường / Thân mật)** và **Gia đình người khác (Kính trọng)** (`ちち/おとうさん`, `はは/おかあさん`, `あに/おにいさん`, `いもうと/いもうとさん`).
- Đếm trôi chảy **số lượng người (`〜にん`)** với hai ngoại lệ kinh điển `ひとり` (1 người), `ふたり` (2 người) và `よにん` (4 người).
- Làm chủ các **loại từ đếm thực chiến (`助数詞 — Josūshi`)** phổ biến nhất ở N5: đồ vật chung (`〜つ`), vật mỏng phẳng (`〜まい`), vật dài thon (`〜ほん`), máy móc/xe cộ (`〜だい`), con vật nhỏ (`〜ひき`).
- Nói trôi chảy lý do và nguyên nhân bằng liên từ **`〜から`** *(Vì ... nên ...)* và hỏi lý do với **`どうしてですか`** *(Tại sao vậy?)*.
- Xử lý thuần thục cách hỏi số lượng (`なんにん`, `いくつ`, `なんまい`) và tránh nhầm lẫn trật tự vế nguyên nhân — kết quả giữa tiếng Việt và tiếng Nhật.

---

## 2. 🌱 Nhập môn chậm rãi từng bước: Hệ thống xưng hô Gia đình & Số đếm Nhật Bản

### 2.1. Quy tắc ""Trong nhà — Ngoài ngõ"" (Uchi vs Soto) trong xưng hô gia đình
Người Nhật cực kỳ coi trọng sự khiêm tốn khi nói về gia đình mình với người ngoài, và sự tôn trọng khi nhắc đến gia đình của đối phương:

| Mối quan hệ | Gia đình TÔI (nói với người ngoài) | Gia đình BẠN / Gọi trực tiếp | Hán Việt |
|---|---|---|---|
| Bố | **ちち (父)** | **おとうさん (お父さん)** | Phụ |
| Mẹ | **はは (母)** | **おかあさん (お母さん)** | Mẫu |
| Anh trai | **あに (兄)** | **おにいさん (お兄さん)** | Huynh |
| Chị gái | **あね (姉)** | **おねえさん (お姉さん)** | Tỷ |
| Em trai | **おとうと (弟)** | **おとうとさん (弟さん)** | Đệ |
| Em gái | **いもうと (妹)** | **いもうとさん (妹さん)** | Muội |
| Chồng / Vợ | **しゅじん / つま (または かない)** | **ごしゅじん / おくさん** | Chủ nhân / Thê |
| Con cái | **こども (子供)** | **おこさん (お子さん)** | Tử |

> [!WARNING]
> **Cấm kỵ văn hóa**: Không dùng `ちち` hay `はは` để nói về bố mẹ người đối diện! Ngược lại, khi giới thiệu với sếp hoặc người mới quen: `わたしの ちちは エンジニアです` *(Bố tôi là kỹ sư)*, chứ không nói `わたしの おとうさんは...` (trừ khi nói chuyện thân mật trong nhà).

### 2.2. Đếm người (`〜にん`) và Đếm vật (`〜つ / 〜まい / 〜ほん / 〜だい`)
1. **Đếm người (`〜人 — 〜にん`)**:
   - **1 người**: `ひとり` *(ĐỘC ĐÁO — không đọc いちにん)*
   - **2 người**: `ふたり` *(ĐỘC ĐÁO — không đọc ににん)*
   - **3 người**: `さんにん`
   - **4 người**: `よにん` *(KHÔNG đọc しにん vì し đồng âm với ""tử"" — chết!)*
   - **5 người trở lên**: `ごにん`, `ろくにん`, `しちにん / ななにん`, `はちにん`, `きゅうにん`, `じゅうにん`.
   - **Hỏi bao nhiêu người**: `なんにん (何人)`.

2. **Đếm vật tổng quát thuần Nhật (`〜つ`: 1 đến 10)**:
   - `ひとつ` (1), `ふたつ` (2), `みっつ` (3), `よっつ` (4), `いつつ` (5), `むっつ` (6), `ななつ` (7), `やっつ` (8), `ここのつ` (9), `とお` (10), **Hỏi**: `いくつ` *(Cũng dùng để hỏi tuổi một cách lịch sự nhẹ nhàng)*.

3. **Các lượng từ chuyên biệt hay gặp nhất**:
   - **Vật mỏng phẳng** *(vé tàu, giấy, ảnh, áo sơ mi)* — `〜まい (枚)`: `いちまい`, `にまい`, `さんまい`, `よんまい`... Hỏi: `なんまい`.
   - **Máy móc, xe cộ** *(laptop, điện thoại, ô tô, xe đạp)* — `〜だい (台)`: `いちだい`, `にだい`, `さんだい`... Hỏi: `なんだい`.
   - **Vật thon dài** *(bút, chai nước, ô dù, cây)* — `〜ほん / ぼん / ぽん (本)` *(có biến âm!)*:
     - `1: いっぽん`, `2: にほん`, `3: さんぼん`, `4: よんほん`, `5: ごほん`, `6: ろっぽん`, `7: ななほん`, `8: はっぽん`, `9: きゅうほん`, `10: じゅっぽん`, Hỏi: `なんぼん`.

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **かぞく (家族)** | kazoku | Gia Tộc | Gia đình | ごかぞくは なんにんですか。 *(Gia đình bạn có mấy người?)* |
| **りょうしん (両親)** | ryoushin | Lưỡng Thân | Bố mẹ (cả hai) | りょうしんは ベトナムに すんでいます。 *(Bố mẹ tôi sống ở Việt Nam.)* |
| **きょうだい (兄弟)** | kyoudai | Huynh Đệ | Anh chị em | きょうだいが さんにん います。 *(Tôi có 3 anh chị em.)* |
| **ちち / おとうさん** | chichi / otousan | Phụ | Bố (tôi) / Bố (người khác) | ちちは かいしゃいんです。 *(Bố tôi là nhân viên công ty.)* |
| **はは / おかあさん** | haha / okaasan | Mẫu | Mẹ (tôi) / Mẹ (người khác) | おかあさんは おげんきですか。 *(Mẹ bạn có khỏe không?)* |
| **あに / おにいさん** | ani / oniisan | Huynh | Anh trai (tôi / bạn) | あには とうきょうに います。 *(Anh trai tôi đang ở Tokyo.)* |
| **あね / おねえさん** | ane / oneesan | Tỷ | Chị gái (tôi / bạn) | おねえさんは きれいですね。 *(Chị gái bạn đẹp thật đấy.)* |
| **おとうと / いもうと** | otouto / imouto | Đệ / Muội | Em trai / Em gái | いもうとは こうこうせいです。 *(Em gái tôi là học sinh cấp 3.)* |
| **ひとり / ふたり** | hitori / futari | Nhất Nhân / Nhị Nhân | 1 người / 2 người | ひとりで えいがを みました。 *(Tôi đã xem phim một mình.)* |
| **きっぷ (切符)** | kippu | Thiết Phù | Vé (tàu, xe, phim) | きっぷを にまい かいました。 *(Tôi đã mua 2 tấm vé.)* |
| **しゃしん (写真)** | shashin | Tả Chân | Bức ảnh | しゃしんを ごまい とりました。 *(Tôi đã chụp 5 tấm ảnh.)* |
| **ペン / かさ (傘)** | pen / kasa | — / Tán | Cây bút / Cái ô (dù) | あかい かさを いっぽん かいました。 *(Tôi đã mua 1 chiếc ô đỏ.)* |
| **パソコン / くるま** | pasokon / kuruma | — / Xa | Laptop / Ô tô | うちに くるまが いちだい あります。 *(Nhà tôi có 1 chiếc ô tô.)* |
| **じかん (時間)** | jikan | Thời Gian | Thời gian | きょうは じかんが ありません。 *(Hôm nay tôi không có thời gian.)* |
| **ようじ (用事)** | youji | Dụng Sự | Việc bận, việc riêng | ようじが ありますから、かえります。 *(Vì có việc bận nên tôi xin về trước.)* |
| **どうして** | doushite | — | Tại sao? | どうして きのう やすみましたか。 *(Tại sao hôm qua bạn lại nghỉ vậy?)* |

---

## 4. Ngữ pháp mổ xẻ: Vị trí số lượng từ & Câu nguyên nhân `〜から`

### 4.1. Vị trí đặt Số lượng từ trong câu tiếng Nhật (Cực kỳ quan trọng!)
Trong tiếng Việt, số lượng đứng trước danh từ (*""mua 2 quả táo""*). Trong tiếng Nhật, **số lượng từ thường đứng NGAY TRƯỚC ĐỘNG TỪ (sau trợ từ `を` hoặc `が`)** và KHÔNG cần thêm trợ từ nào sau nó:
- `りんごを ふたつ かいました。` *(Tôi đã mua 2 quả táo.)* — KHÔNG nói `ふたつを かいました`.
- `きょうしつに がくせいが ごにん います。` *(Trong lớp học có 5 học sinh.)*
- `きってを さんまいと はがきを にまい ください。` *(Cho tôi 3 con tem và 2 tấm bưu thiếp.)*

### 4.2. Liên từ chỉ nguyên nhân `〜から` *(Vì ... nên ...)*
- **Công thức đầy đủ**: `[Mệnh đề nguyên nhân (lịch sự hoặc thông thường)] + から、[Mệnh đề kết quả]。`
- **Lưu ý trật tự bất di bất dịch**: Tiếng Nhật luôn đặt **NGUYÊN NHÂN trước, KẾT QUẢ sau**. Khác với tiếng Việt có thể đảo *""Tôi về sớm vì mệt""*, tiếng Nhật BẮT BUỘC đặt `から` gắn ngay sau vế nguyên nhân!
  - `あしたは テストが ありますから、こんばん べんきょうします。` *(Vì ngày mai có bài kiểm tra nên tối nay tôi học bài.)*
  - `さむいですから、まどを しめます。` *(Vì trời lạnh nên tôi đóng cửa sổ.)*
- **Trả lời câu hỏi `どうしてですか` (Tại sao?)**: Đặt `〜から` ở cuối câu trả lời!
  - A: `どうして きのう やすみましたか。` *(Tại sao hôm qua bạn nghỉ?)*
  - B: `あたまが いたかったですから。` *(Vì tôi bị đau đầu ạ.)*

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: わたしの かぞくは しにん です。 (Đọc 4 người là しにん)
   ⭕ ĐÚNG: わたしの かぞくは よにん です。
   💡 GIẢI THÍCH: 1 người = ひとり, 2 người = ふたり, 4 người = よにん (tránh âm し giống chữ Tử).

2. ❌ SAI: りんご ふたつを たべました。 (Đặt trợ từ を sau số lượng từ ふたつ)
   ⭕ ĐÚNG: りんごを ふたつ たべました。 (Trợ từ を đi theo danh từ りんご, số lượng từ đứng trước động từ)
   💡 GIẢI THÍCH: Số lượng từ đóng vai trò như trạng từ bổ nghĩa trực tiếp cho động từ, không gắn thêm を hay が.

3. ❌ SAI: やすみます、かぜを ひきましたから。 (Đặt kết quả lên trước theo thói quen tiếng Việt)
   ⭕ ĐÚNG: かぜを ひきましたから、やすみます。 (Nguyên nhân + から đứng đầu câu)
   💡 GIẢI THÍCH: Trong văn phong chuẩn N5, mệnh đề nguyên nhân kèm から luôn đi trước mệnh đề kết quả.

4. ❌ SAI: あなたの ちちは なんさいですか。 (Dùng ちち để hỏi về bố của bạn mình)
   ⭕ ĐÚNG: おとうさんは おいくつですか。 (Dùng おとうさん khi nói về bố người đối diện)
   💡 GIẢI THÍCH: ちち/はは chỉ dành cho bố/mẹ của bản thân khi kể với người ngoài.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Trong giờ nghỉ giải lao tại trường Nhật ngữ, bạn Kenji xem ảnh gia đình trên điện thoại của Minh và trò chuyện.

- **Kenji**: ミンさん、この しゃしんは ごかぞくですか。
  - *Romaji*: Min-san, kono shashin wa gokazoku desu ka?
  - *Phân tích*: `ごかぞく` (kính ngữ hỏi về gia đình người khác).
  - *Dịch*: Minh ơi, bức ảnh này là gia đình bạn à?

- **Minh**: はい、そうです。わたしの かぞくは よにんです。ちちと ははと いもうとと わたしです。
  - *Romaji*: Hai, sou desu. Watashi no kazoku wa yonin desu. Chichi to haha to imouto to watashi desu.
  - *Phân tích*: `よにん` (4 người) + `ちち/はは/いもうと` (cách gọi khiêm nhường gia đình mình).
  - *Dịch*: Ừ, đúng rồi. Nhà mình có 4 người: bố, mẹ, em gái và mình.

- **Kenji**: いもうとさんは いま なんさいですか。
  - *Romaji*: Imouto-san wa ima nansai desu ka?
  - *Phân tích*: `いもうとさん` (gọi em gái của Minh một cách lịch sự).
  - *Dịch*: Em gái bạn năm nay bao nhiêu tuổi rồi?

- **Minh**: じゅうろくさいです。おんがくが すきですから、まいにち ピアノを れんしゅうしています。
  - *Romaji*: Juurokusai desu. Ongaku ga suki desu kara, mainichi piano o renshuu shite imasu.
  - *Phân tích*: `すきですから` (vì thích nên...) + `まいにち` (mỗi ngày).
  - *Dịch*: 16 tuổi rồi. Vì thích âm nhạc nên ngày nào em ấy cũng tập đàn piano.

- **Kenji**: すごいですね！あした いっしょに えいがを みませんか。きっぷが にまい ありますから。
  - *Romaji*: Sugoi desu ne! Ashita issho ni eiga o mimasen ka. Kippu ga nimai arimasu kara.
  - *Phân tích*: `きっぷが にまい` (2 tấm vé) + `ありますから` (vì có sẵn nên...).
  - *Dịch*: Tuyệt quá nhỉ! Ngày mai đi xem phim cùng mình không? Vì mình đang có sẵn 2 tấm vé này.

- **Minh**: わあ、ありがとうございます！ぜひ いきましょう！
  - *Romaji*: Waa, arigatou gozaimasu! Zehi ikimashou!
  - *Phân tích*: `ぜひ` (nhất định rồi) + `いきましょう` (cùng đi nào!).
  - *Dịch*: Ôi, cảm ơn bạn nhiều! Nhất định cùng đi thôi!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện từ vựng gia đình, lượng từ & `〜から`
1. Đọc đúng bằng Hiragana: `1 người`, `2 người`, `4 người`, `3 cây bút`, `2 chiếc xe ô tô`.
2. Điền đúng từ xưng hô: `わたしの (ちち / おとうさん) は いしゃです。`
3. Nối thành 1 câu có `から`: *(Trời mưa / tôi không đi công viên)* → `あめが ふります` + `こうえんへ いきません`.

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - 1 người: `ひとり`, 2 người: `ふたり`, 4 người: `よにん`
   - 3 cây bút: `ペンが さんぼん` *(biến âm `ぼん`)*
   - 2 chiếc ô tô: `くるまが にだい`
2. **Câu 2**: Chọn **`ちち`** (`わたしの ちちは いしゃです`).
3. **Câu 3**: `あめが ふりますから、こうえんへ いきません。`
4. **Câu 4 trắc nghiệm**: Câu nào đặt đúng vị trí số lượng từ?
   - *Đáp án*: `きってを ごまい かいました。` *(Số lượng từ đứng sau ` danh từ + を` và trước động từ).*
5. **Câu 5 trắc nghiệm**: Khi hỏi ""Nhà bạn có mấy anh chị em?"", dùng từ hỏi nào?
   - *Đáp án*: `なんにん` (`ごきょうだいは なんにんですか`).

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Lượng từ `〜ほん/ぼん/ぽん` và `ひとり/ふたり/よにん` cực kỳ dễ quên nếu chỉ đọc lướt. Hãy mở **Word Map & Dictation** bên dưới để rèn phản xạ âm thanh ngay lập tức!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Phân biệt xưng hô gia đình Uchi/Soto, nắm vững bộ số đếm người/vật N5 và diễn đạt nguyên nhân với 〜から.",
            "Luyện biến âm lượng từ 〜ほん/ぼん/ぽん, ngoại lệ ひとり/ふたり/よにん và trật tự mệnh đề nguyên nhân — kết quả."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, JapaneseChapter4Id, JapaneseLesson43Id,
            "Bài 4.3: Nơi Chốn Công Cộng, Tồn Tại (あります / います) & Chỉ Dẫn Vị Trí Nâng Cao",
            "ja-n5-noi-chon-ton-tai-vi-tri-nang-cao", 3, 40,
            @"# Bài 4.3: Nơi Chốn Công Cộng, Tồn Tại (あります / います) & Chỉ Dẫn Vị Trí Nâng Cao

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Tiếp nối kiến thức di chuyển ở Chương 3, bài học cuối Chương 4 giúp bạn miêu tả **chính xác không gian 3 chiều**, vị trí tương quan giữa các công trình công cộng trong thành phố Nhật Bản, và phân biệt rạch ròi hai mẫu câu tồn tại kinh điển của JLPT N5:
- Sử dụng thành thạo **2 cặp cấu trúc tồn tại** (`[Vật/Người] が [Nơi] に あります/います` và `[Nơi] に [Vật/Người] が あります/います`) và hiểu sắc thái nhấn mạnh của từng mẫu.
- Mở rộng toàn bộ **11 danh từ chỉ vị trí tương đối** (`うえ`, `した`, `まえ`, `うしろ`, `みぎ`, `ひだり`, `なか`, `そと`, `となり`, `ちかく`, `あいだ`) — đặc biệt là cấu trúc kẹp giữa **`A と B の あいだ`** *(giữa A và B)*.
- Phân biệt chuẩn xác khi nào dùng **`や〜など`** *(liệt kê không đầy đủ: ... và ..., vân vân)* thay cho **`と`** *(liệt kê trọn vẹn)*.
- Thuần thục từ vựng **nơi chốn công cộng thực tế** tại Nhật Bản (`こうばん` đồn cảnh sát, `びょういん` bệnh viện, `ほんや` hiệu sách, `デパート` trung tâm thương mại, `こうえん` công viên, `きっさてん` quán cà phê).
- Kết hợp mượt mà câu tồn tại và câu chỉ đường để tự tin hỏi/chỉ dẫn vị trí đồ vật bị thất lạc hoặc tìm đường trong khu phố Nhật.

---

## 2. 🌱 Tư duy không gian & 2 cặp câu Tồn Tại (あります / います)

### 2.1. Chọn `あります` hay `います`?
- **`あります` (thể phủ định: `ありません`)**: Dùng cho **vật vô tri, cây cối, công trình kiến trúc, sự kiện/lịch trình** (`つくえ`, `ぎんこう`, `さくら`, `パーティー`, `じかん`).
- **`います` (thể phủ định: `いません`)**: Dùng cho **người và động vật** có khả năng tự di chuyển (`ともだち`, `せんせい`, `いぬ`, `ねこ`, `とり`).
  - *Mẹo nhớ*: Cây hoa đứng yên một chỗ → `あります`. Con cá bơi trong bể → `います`. Robot tự động? Trong N5 thường xem máy móc đứng yên là `あります` (như máy bán hàng tự động `じどうはんばいき`).

### 2.2. Hai trật tự câu Tồn tại & Khác biệt về Trợ từ (`は` vs `に` vs `が`)
Tùy vào việc bạn muốn **hỏi ""Cái gì ở đâu?""** hay **kể ""Ở chỗ đó có cái gì?""**, người Nhật đảo vị trí của Chủ thể và Địa điểm:

1. **Mẫu 1 — Nói về vị trí của vật/người đã biết** *(A ở chỗ B)*:
   - Công thức: **`[Chủ thể] は [Địa điểm] に あります / います。`**
   - Dùng khi người nghe đã biết đang tìm thứ gì (như tìm nhà vệ sinh, tìm anh Tanaka):
   - `こうばんは えきの まえに あります。` *(Đồn cảnh sát thì ở trước nhà ga.)*
   - `たなかさんは じむしょに います。` *(Anh Tanaka đang ở trong văn phòng.)*

2. **Mẫu 2 — Miêu tả cảnh vật / thông báo sự hiện diện** *(Ở chỗ B có A)*:
   - Công thức: **`[Địa điểm] に [Chủ thể] が あります / います。`**
   - Chú ý: Trước danh từ chưa xác định đi với `が`, KHÔNG dùng `は`!
   - `つくえの うえに しゃしんが あります。` *(Trên bàn có bức ảnh.)*
   - `こうえんの なかに こどもが います。` *(Trong công viên có trẻ em.)*

### 2.3. Liệt kê trọn vẹn `と` vs Liệt kê tiêu biểu `や〜など`
- **`A と B`**: Dùng khi chỉ có đúng A và B *(liệt kê hết)*.
  - `かばんの なかに さいふと かぎが あります。` *(Trong túi có ví và chìa khóa — hết rồi.)*
- **`A や B (など)`**: Dùng khi kể ra 2-3 món tiêu biểu trong nhiều món *(... và ..., vân vân)*.
  - `つくえの うえに ほんや ノートや ペンなどが あります。` *(Trên bàn có sách, vở, bút, v.v...)*

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **うえ (上) / した (下)** | ue / shita | Thượng / Hạ | Trên / Dưới | いすの したに ねこが います。 *(Dưới ghế có một con mèo.)* |
| **まえ (前) / うしろ (後ろ)** | mae / ushiro | Tiền / Hậu | Trước / Sau | えきの まえに バスていが あります。 *(Trước ga có trạm xe buýt.)* |
| **みぎ (右) / ひだり (左)** | migi / hidari | Hữu / Tả | Bên phải / Bên trái | ほんやの みぎに カフェが あります。 *(Bên phải hiệu sách có quán cà phê.)* |
| **なか (中) / そと (外)** | naka / soto | Trung / Ngoại | Bên trong / Bên ngoài | はこの なかに なにも ありません。 *(Trong hộp không có gì cả.)* |
| **となり (隣)** | tonari | Lân | Ngay bên cạnh *(cùng loại)* | ぎんこうの となりに ゆうびんきょくが あります。 *(Cạnh ngân hàng có bưu điện.)* |
| **ちかく (近く)** | chikaku | Cận | Ở gần | だいがくの ちかくに すんでいます。 *(Tôi sống ở gần trường đại học.)* |
| **あいだ (間)** | aida | Gian | Ở giữa (2 vật/2 nơi) | ほんやと はなやの あいだに あります。 *(Nằm giữa hiệu sách và tiệm hoa.)* |
| **こうばん (交番)** | kouban | Giao Phiên | Đồn cảnh sát khu phố | こまったら、こうばんで ききましょう。 *(Nếu gặp khó khăn hãy hỏi ở đồn cảnh sát.)* |
| **びょういん (病院)** | byouin | Bệnh Viện | Bệnh viện | びょういんは どこに ありますか。 *(Bệnh viện ở đâu vậy ạ?)* |
| **ほんや (本屋)** | honya | Bản Ốc | Hiệu sách | ほんやで にほんごの ほんを かいました。 *(Tôi đã mua sách tiếng Nhật ở hiệu sách.)* |
| **デパート** | depaato | — | Trung tâm thương mại | にちようびに デパートへ いきます。 *(Chủ nhật tôi đi trung tâm thương mại.)* |
| **こうえん (公園)** | kouen | Công Viên | Công viên | こうえんで いぬと さんぽします。 *(Tôi đi dạo cùng chó ở công viên.)* |
| **たな (棚) / れいぞうこ** | tana / reizouko | Bằng / Lãnh Tàng Khố | Giá sách / Tủ lạnh | れいぞうこの なかに ぎゅうにゅうが あります。 *(Trong tủ lạnh có sữa tươi.)* |
| **エレベーター / かいだん** | erebeetaa / kaidan | — / Giai Đoạn | Thang máy / Cầu thang bộ | エレベーターは みぎに あります。 *(Thang máy ở bên phải ạ.)* |
| **なにも〜ません / だれも〜ません** | nanimo / daremo | Hà / Thùy | Không cái gì / Không ai cả | へやに だれも いません。 *(Trong phòng không có ai cả.)* |

---

## 4. Ngữ pháp nâng cao Chương 4: Cấu trúc `A と B の あいだ` & Phủ định toàn phần

### 4.1. Định vị kẹp giữa hai mốc: `[Danh từ 1] と [Danh từ 2] の あいだ に`
- Khi một địa điểm hoặc đồ vật nằm kẹp giữa hai đối tượng khác, bắt buộc nối bằng `と` trước rồi mới gắn `の あいだ`:
  - `ゆうびんきょくは ぎんこうと スーパーの あいだに あります。` *(Bưu điện nằm ở giữa ngân hàng và siêu thị.)*
  - `わたしは たなかさんと ミンさんの あいだに すわります。` *(Tôi ngồi giữa anh Tanaka và anh Minh.)*

### 4.2. Phân biệt `となり` (ngay sát cạnh) và `ちかく` (gần quanh đây)
- `となり`: Sát vách, kế bên (thường dùng cho hai đối tượng tương đồng: nhà cạnh nhà, người ngồi ghế cạnh người, hiệu sách cạnh ngân hàng). Không dùng `A の となりに ペンがあります` nếu bàn và bút khác cấp độ quá xa; thay vào đó nói `ちかく` hoặc `よこ` (bên hông).
- `ちかく`: Trong phạm vi bán kính gần quanh đó (chỉ vùng lân cận).

### 4.3. Phủ định hoàn toàn sự tồn tại (`なにも ありません` / `だれも いません`)
- **Với vật (`なに` + `も` + Phủ định)**: `つくえの うえに なにも ありません。` *(Trên bàn chẳng có cái gì cả.)*
- **Với người (`だれ` + `も` + Phủ định)**: `きょうしつに だれも いません。` *(Trong lớp học không có một ai cả.)*
- **Với nơi chốn (`どこ` + `も/へも` + Phủ định)**: `きのう どこへも いきませんでした。` *(Hôm qua tôi không đi đâu cả.)*

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: つくえの うえで ほんが あります。 (Dùng trợ từ で cho động từ あります)
   ⭕ ĐÚNG: つくえの うえに ほんが あります。 (Vị trí tồn tại luôn dùng に)
   💡 GIẢI THÍCH: Trợ từ で dùng cho nơi diễn ra HÀNH ĐỘNG (đọc sách, ăn cơm); trợ từ に dùng cho nơi TỒN TẠI (có, ở).

2. ❌ SAI: いぬと ほんが つくえの したに あります。 (Vừa có người/động vật vừa có vật mà chia あります)
   ⭕ ĐÚNG: Dùng theo chủ thể đứng gần động từ hoặc tách câu: いぬが いすの したに います。ほんが つくえの うえに あります。
   💡 GIẢI THÍCH: Động vật (いぬ, ねこ) luôn chia います; đồ vật (ほん) luôn chia あります.

3. ❌ SAI: ゆうびんきょくは ぎんこうと スーパー あいだに あります。 (Thiếu の trước あいだ)
   ⭕ ĐÚNG: ゆうびんきょくは ぎんこうと スーパーの あいだに あります。
   💡 GIẢI THÍCH: Mọi danh từ chỉ vị trí (うえ, した, なか, あいだ...) đều phải liên kết với danh từ mốc bằng trợ từ の!

4. ❌ SAI: へやに だれが いません。 (Dùng trợ từ が với từ hỏi + phủ định)
   ⭕ ĐÚNG: へやに だれも いません。 (Phải dùng も: だれも / なにも)
   💡 GIẢI THÍCH: Khi diễn đạt ""không ai cả / không gì cả"", tiếng Nhật bắt buộc gắn trợ từ も sau だれ hoặc なに.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Minh bị rơi chìa khóa ký túc xá và đang hỏi bác bảo vệ (kanrinin) cùng bạn cùng phòng để tìm lại.

- **Minh**: すみません、へやの かぎが ありません。どこに ありますか。
  - *Romaji*: Sumimasen, heya no kagi ga arimasen. Doko ni arimasu ka?
  - *Phân tích*: `かぎ` (chìa khóa) + `ありません` (không có/bị mất) + `どこに ありますか` (ở đâu vậy?).
  - *Dịch*: Xin lỗi bác, chìa khóa phòng cháu không thấy đâu cả. Có ở đâu không ạ?

- **Bác bảo vệ**: そうですか。きょうしつの なかや じむしょを みましたか。
  - *Romaji*: Sou desu ka. Kyoushitsu no naka ya jimusho o mimashita ka?
  - *Phân tích*: `なかや じむしょ` (trong lớp học hay văn phòng, v.v...).
  - *Dịch*: Thế à. Cháu đã tìm thử trong lớp học hay văn phòng chưa?

- **Minh**: はい、みましたが、きょうしつには なにも ありませんでした。
  - *Romaji*: Hai, mimashita ga, kyoushitsu ni wa nanimo arimasen deshita.
  - *Phân tích*: `なにも ありませんでした` (quá khứ: đã chẳng có gì cả).
  - *Dịch*: Dạ cháu xem rồi nhưng trong lớp học không có gì hết ạ.

- **Bác bảo vệ**: あ、ちょっと まってください。たなの うえに あかい かばんが ありますね。その となりに かぎが ありますよ！
  - *Romaji*: A, chotto matte kudasai. Tana no ue ni akai kaban ga arimasu ne. Sono tonari ni kagi ga arimasu yo!
  - *Phân tích*: `たなの うえに` (trên kệ) + `その となりに` (bên cạnh cái túi đó).
  - *Dịch*: À, chờ bác một chút. Trên kệ có cái cặp màu đỏ đúng không. Bên cạnh nó có chiếc chìa khóa kia kìa!

- **Minh**: ほんとうだ！ありました！どうも ありがとうございます！
  - *Romaji*: Hontou da! Arimashita! Doumo arigatou gozaimasu!
  - *Phân tích*: `ありました` (tìm thấy rồi / có rồi!).
  - *Dịch*: Đúng thật rồi! Cháu thấy rồi ạ! Cháu cảm ơn bác rất nhiều!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện định vị & trợ từ
1. Điền trợ từ đúng (`に`, `で`, `や`, `の`): `つくえ ___ うえ ___ ノート ___ ペンなど ___ あります。`
2. Dịch sang tiếng Nhật: *""Hiệu sách nằm ở giữa nhà ga và bệnh viện.""*
3. Sửa lỗi câu sau: `こうえんの なかに こどもが たくさん あります。`

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: `つくえ の うえ に ノート や ペンなど が あります。` *(Chú ý `や` liệt kê không hết, `に` cho vị trí tồn tại, `が` cho chủ thể tồn tại).*
2. **Câu 2**: `ほんやは えきと びょういんの あいだに あります。`
3. **Câu 3**: Đổi `あります` → `います` vì `こども` (trẻ em) là người: `こうえんの なかに こどもが たくさん います。`
4. **Câu 4 trắc nghiệm**: Để nói ""Trong tủ lạnh không có gì cả"", câu đúng là:
   - *Đáp án*: `れいぞうこの なかに なにも ありません。`
5. **Câu 5 trắc nghiệm**: Trợ từ nào dùng trong câu: ""Ở gần nhà tôi có một đồn cảnh sát""?
   - *Đáp án*: `うちの ちかく に こうばん が あります。`

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Hãy luyện kỹ cặp trợ từ `に` (tồn tại) và `で` (hành động) trong phần **Cloze & Translate** ngay bên dưới — đây là câu bẫy kinh điển chiếm tới 2–3 điểm trong đề Ngữ pháp JLPT N5!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Mở rộng 11 danh từ chỉ vị trí tương đối, cấu trúc kẹp giữa A と B の あいだ và liệt kê với や〜など.",
            "Phân biệt rạch ròi 2 mẫu câu tồn tại あります/います, trợ từ に vs で và phủ định toàn phần なにも/だれも."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, JapaneseChapter5Id, JapaneseLesson51Id,
            "Bài 5.1: Động Từ Thể Te (〜て形): Yêu Cầu Lịch Sự (〜てください) & Tiếp Diễn (〜ています)",
            "ja-n5-dong-tu-the-te-yeu-cau-tiep-dien", 1, 45,
            @"# Bài 5.1: Động Từ Thể Te (〜て形): Yêu Cầu Lịch Sự (〜てください) & Tiếp Diễn (〜ています)

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Chào mừng bạn bước vào Chương 5 — chương **quan trọng nhất của toàn bộ trình độ tiếng Nhật Sơ cấp N5**! Việc chia thành thạo **Động từ Thể Te (て形 — Te-kei)** là chìa khóa mở ra hơn 60% mẫu ngữ pháp N5 và N4. Sau bài học này, bạn sẽ:
- Phân loại chính xác **3 nhóm động từ tiếng Nhật** (Nhóm I: Ngũ đoạn, Nhóm II: Nhất đoạn, Nhóm III: Bất quy tắc `します / きます`) từ thể lịch sự `〜ます`.
- Thuộc lòng **Quy tắc chia Thể Te** (bao gồm quy tắc biến âm chữ Tẽ: `いて/いで`, `って`, `んで`, `して` và 2 ngoại lệ đặc biệt `いきます → いって`, `きます → きて`).
- Vận dụng ngay 3 mẫu câu ứng dụng thực tế nhất của Thể Te:
  1. **Nhờ vả, yêu cầu lịch sự**: `〜てください` *(Xin hãy làm...)*
  2. **Hành động đang diễn ra ngay lúc nói / Trạng thái nghề nghiệp, cư trú**: `〜ています` *(Đang làm... / Đang sống ở...)*
  3. **Đề nghị giúp đỡ người khác một cách tự nhiên**: `〜ましょうか` *(Để tôi giúp bạn ... nhé?)*
- Nối nhiều hành động theo trình tự thời gian trong cùng một câu (`V1て、V2て、V3ます`).

---

## 2. 🌱 Chia 3 Nhóm Động Từ & Công thức Vàng Thể Te (て形)

### 2.1. Cách nhận biết 3 Nhóm Động từ từ đuôi `〜ます`
Trước khi chia Thể Te, nhìn vào **chữ cái đứng ngay trước `ます`**:

1. **Nhóm III (Bất quy tắc — Chỉ có 2 gốc!)**:
   - `きます (来ます - đến)` → Thể Te: **`きて`**
   - `します (します - làm)` và mọi danh động từ `N + します` (`べんきょうします`, `かいものします`, `コピーします`) → Thể Te: **`して`** (`べんきょうして`).

2. **Nhóm II (Nhất đoạn — Dễ nhất!)**:
   - Đa số động từ có chữ trước `ます` thuộc **cột E** (`たべます`, `ねます`, `あけます`, `しめます`, `おしえます`, `みせます`).
   - Cộng thêm một số động từ **cột I đặc biệt** hay dùng ở N5: `みます (xem)`, `おきます (thức dậy)`, `かります (mượn)`, `います (có/ở)`, `できます (có thể)`, `あびます (tắm)`.
   - **Cách chia Thể Te Nhóm II**: Chỉ cần **Bỏ `ます`, thêm `て`**!
     - `たべます` → **`たべて`** | `みます` → **`みて`** | `おきます` → **`おきて`** | `ねます` → **`ねて`**.

3. **Nhóm I (Ngũ đoạn — Biến âm theo chữ trước `ます`)**:
   - Các động từ còn lại có chữ trước `ます` thuộc **cột I** (`かきます`, `のみます`, `かいます`, `まちます`, `あそびます`, `かえります`, `はなします`).
   - **Bảng quy tắc biến âm Nhóm I (Nhất định phải thuộc như bảng cửu chương!)**:

| Chữ ngay trước `ます` | Quy tắc biến đổi Thể Te | Ví dụ chuyển đổi | Ngoại lệ bắt buộc nhớ |
|---|---|---|---|
| **い, ち, り** | Đổi thành **`って`** | かいます → **かって** / まちます → **まって** / かえります → **かえって** | *(Không có)* |
| **み, び, に** | Đổi thành **`んで`** | のみます → **のんで** / よみます → **よんで** / あそびます → **あそんで** | `しにます` → `しんで` |
| **き** | Đổi thành **`いて`** | かきます → **かいて** / ききます → **きいて** | **`いきます` → `いって`** *(Đặc biệt!)* |
| **ぎ** | Đổi thành **`いで`** | いそぎます → **いそいで** / およぎます → **およいで** | *(Không có)* |
| **し** | Đổi thành **`して`** | はなします → **はなして** / かします → **かして** | *(Không có)* |

> [!TIP]
> **Mẹo hát nhớ Thể Te Nhóm I theo giai điệu**:
> *""I-chi-ri thành **tte** (って); Mi-bi-ni thành **nde** (んで); Ki thành **ite** (いて), Gi thành **ide** (いで); Shi thành **shite** (して); riêng Ikimasu là **itte** (いって)!""*

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Động từ thể ます | Thể Te (て形) | Nhóm | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **あけます (開けます)** | あけて | II | Mở (cửa, hộp) | まどを あけて ください。 *(Xin hãy mở cửa sổ ra.)* |
| **しめます (閉めます)** | しめて | II | Đóng (cửa) | さむいですから、ドアを しめて ください。 *(Vì lạnh nên hãy đóng cửa lại.)* |
| **つけます** | つけて | II | Bật (đèn, điều hòa) | でんきを つけて ください。 *(Làm ơn bật đèn lên giúp tôi.)* |
| **けします (消します)** | けして | I | Tắt (đèn, tivi) | ねるとき、エアコンを けします。 *(Khi đi ngủ tôi tắt điều hòa.)* |
| **まちます (待ちます)** | まって | I | Chờ, đợi | すみません、ちょっと まって ください。 *(Xin lỗi, hãy đợi một chút.)* |
| **てつだいます (手伝います)** | てつだって | I | Giúp đỡ | しごとを てつだって ください。 *(Hãy giúp tôi làm việc này với.)* |
| **もちます (持ちます)** | もって | I | Cầm, mang, xách | その にもつを もちましょうか。 *(Để tôi xách giúp bạn hành lý nhé?)* |
| **いそぎます (急ぎます)** | いそいで | I | Vội vã, khẩn trương | じかんが ありませんから、いそいで ください。 *(Không còn thời gian nữa, nhanh lên nào.)* |
| **よびます (呼びます)** | よんで | I | Gọi (taxi, tên ai) | タクシーを よびましょうか。 *(Tôi gọi taxi giúp bạn nhé?)* |
| **とります (取ります/撮ります)** | とって | I | Lấy / Chụp (ảnh) | ここで しゃしんを とって ください。 *(Hãy chụp ảnh ở đây nhé.)* |
| **すわります (座ります)** | すわって | I | Ngồi xuống | どうぞ、ここに すわって ください。 *(Xin mời ngồi xuống đây ạ.)* |
| **たちます (立ちます)** | たって | I | Đứng lên | みなさんに たって ください。 *(Mọi người hãy đứng dậy ạ.)* |
| **ふります (降ります)** | ふって | I | Rơi (mưa, tuyết) | いま あめが ふっています。 *(Bây giờ trời đang mưa.)* |
| **すみます (住みます)** | すんで | I | Sống, cư trú | わたしは ハノイに すんでいます。 *(Tôi đang sống ở Hà Nội.)* |
| **しります (知ります)** | しって | I | Biết (ai đó/thông tin) | ミンさんを しっていますか。 *(Bạn có biết anh Minh không?)* |
| **けっこんします** | けっこんして | III | Kết hôn | あねは けっこんしています。 *(Chị gái tôi đã kết hôn rồi.)* |

---

## 4. Ngữ pháp ứng dụng Thể Te & 4 Bẫy Sai Kinh Điển

### 4.1. Yêu cầu, nhờ vả lịch sự: `Vて ください` *(Xin hãy ...)*
- Dùng khi nhờ người khác làm gì, hướng dẫn chỉ đường hoặc mời khách làm theo phép lịch sự:
  - `ここに なまえを かいて ください。` *(Xin hãy viết tên vào đây.)*
  - `もう いちど ゆっくり いって ください。` *(Xin hãy nói chậm lại một lần nữa.)*
- *Lưu ý*: Không dùng `Vてください` để ra lệnh cho cấp trên; với sếp nên dùng lời đề nghị mềm hơn ở trình độ cao hơn hoặc thêm `すみませんが...` ở đầu câu.

### 4.2. Hiện tại tiếp diễn & Trạng thái kéo dài: `Vて います`
Mẫu `Vています` có **2 nghĩa cốt lõi** ở N5:
1. **Hành động đang diễn ra ngay lúc nói**:
   - `いま あめが ふっています。` *(Bây giờ trời đang mưa.)*
   - `たなかさんは いま でんわを かけています。` *(Anh Tanaka đang gọi điện thoại.)*
2. **Trạng thái kéo dài / Thói quen / Nghề nghiệp / Cư trú**:
   - `とうきょうに すんでいます。` *(Tôi đang sống ở Tokyo.)*
   - `ソフトの かいしゃで はたらいています。` *(Tôi đang làm việc ở công ty phần mềm.)*
   - **Đặc biệt với `しります` (biết)**: Hỏi ""Bạn có biết không?"" PHẢI dùng **`しっていますか`** (trả lời Không biết: **`しりません`**, KHÔNG nói `しっていません`!).

### 4.3. Nối chuỗi hành động liên tiếp: `V1て、V2て、V3ます`
- Kể các hành động xảy ra theo thứ tự thời gian trong ngày:
  - `まいあさ ろくじに おきて、コーヒーを のんで、かいしゃへ いきます。` *(Mỗi sáng tôi dậy lúc 6 giờ, uống cà phê rồi đi làm.)*
- Lưu ý: Thì của cả câu (hiện tại hay quá khứ) chỉ chia ở động từ CUỐI CÙNG!

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: きょうとへ いきて ください。 (Chia いきます thành いきて theo máy móc)
   ⭕ ĐÚNG: きょうとへ いって ください。 (いきます là ngoại lệ duy nhất của đuôi き → いって)
   💡 GIẢI THÍCH: Trong khi かきます → かいて, ききます → きいて thì いきます bắt buộc là いって!

2. ❌ SAI: いいえ、たなかさんを しっていません。 (Phủ định của しっています)
   ⭕ ĐÚNG: いいえ、しりません。
   💡 GIẢI THÍCH: Phủ định của しっています (biết) trong tiếng Nhật là しりません (không biết), KHÔNG dùng しっていません.

3. ❌ SAI: かいて、いそぎました、かえりました。 (Quên không bỏ て hoặc chia quá khứ ở động từ đầu)
   ⭕ ĐÚNG: いそいで、でんしゃに のって、かえりました。 (Các động từ trước dùng Thể Te, động từ cuối chia quá khứ ました)
   💡 GIẢI THÍCH: Động từ Thể Te không mang thì! Thì quá khứ chỉ đặt ở động từ kết thúc câu.

4. ❌ SAI: でんきを つくって ください。 (Nhầm つきます với つけます / つくります)
   ⭕ ĐÚNG: でんきを つけて ください。 (Bật đèn là つけます nhóm II → つけて)
   💡 GIẢI THÍCH: つくります (làm/chế tạo) → つくって; còn つけます (bật công tắc) → つけて.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Trong văn phòng làm việc ở Tokyo, anh Sato đang hướng dẫn Minh (thực tập sinh mới) làm thủ tục và xử lý công việc buổi sáng.

- **Sato**: ミンさん、いま なにを していますか。
  - *Romaji*: Min-san, ima nani o shite imasu ka?
  - *Phân tích*: `なにを していますか` (đang làm gì thế?).
  - *Dịch*: Minh ơi, bây giờ em đang làm gì thế?

- **Minh**: しりょうを コピーしています。さとうさん、この ペンを かして ください。
  - *Romaji*: Shiryō o kopii shite imasu. Satou-san, kono pen o kashite kudasai.
  - *Phân tích*: `コピーしています` (đang photocopy) + `かして ください` (cho mượn từ かします).
  - *Dịch*: Em đang photocopy tài liệu ạ. Anh Sato ơi, cho em mượn cây bút này một lát nhé.

- **Sato**: はい、どうぞ。あ、ちょっと あついですね。まどを あけて ください。
  - *Romaji*: Hai, douzo. A, chotto atsui desu ne. Mado o akete kudasai.
  - *Phân tích*: `まどを あけて ください` (xin hãy mở cửa sổ).
  - *Dịch*: Đây, mời em. À, hơi nóng nhỉ. Em mở giúp anh cái cửa sổ nhé.

- **Minh**: はい、わかりました。あ、さとうさん、にもつが おおいですね。もちましょうか。
  - *Romaji*: Hai, wakarimashita. A, Satou-san, nimotsu ga ooi desu ne. Mochimashou ka?
  - *Phân tích*: `にもつが おおい` (nhiều hành lý) + `もちましょうか` (để em xách giúp nhé?).
  - *Dịch*: Vâng, em hiểu rồi ạ. À anh Sato ơi, nhiều đồ quá nhỉ. Để em xách phụ anh nhé?

- **Sato**: すみません、おねがいします！その はこを つくえの うえに おいて ください。
  - *Romaji*: Sumimasen, onegaishimasu! Sono hako o tsukue no ue ni oite kudasai.
  - *Phân tích*: `おいて ください` (đặt xuống từ おきます → おいて).
  - *Dịch*: Làm phiền em quá, nhờ em nhé! Em đặt cái hộp đó lên trên bàn giúp anh.

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Chuyển sang Thể Te & Đặt câu
1. Chuyển 6 động từ sau sang Thể Te: `かいます`, `のみます`, `ききます`, `いきます`, `みます`, `します`.
2. Dịch sang tiếng Nhật: *""Bây giờ trời đang mưa nên xin hãy gọi taxi giúp tôi.""*
3. Sửa lỗi sai: `わたしは とうきょうに すみます。` *(Muốn nói đang sinh sống lâu dài tại Tokyo)*.

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: `かって` / `のんで` / `きいて` / **`いって`** / `みて` / `して`.
2. **Câu 2**: `いま あめが ふっていますから、タクシーを よんで ください。`
3. **Câu 3**: Dùng thể tiếp diễn trạng thái: `わたしは とうきょうに すんでいます。`
4. **Câu 4 trắc nghiệm**: Thể Te đúng của động từ `およぎます` (bơi) là gì?
   - *Đáp án*: `およいで` *(Đuôi ぎ → いで có tenten).*
5. **Câu 5 trắc nghiệm**: Trả lời phủ định cho câu hỏi `ミラーさんを しっていますか。` (Bạn có biết anh Miller không?):
   - *Đáp án*: `いいえ、しりません。` *(Không dùng しっていません).*

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Hãy bấm ngay vào **Phòng Thực Hành (Shadowing / Cloze / Grammar Fix)** bên dưới để luyện chia hàng loạt động từ Nhóm I/II/III sang Thể Te tới mức bật ra trong 1 giây!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Làm chủ phân loại 3 nhóm động từ và quy tắc chia Thể Te (〜て形), bao gồm ngoại lệ いって và biến âm んで/いて.",
            "Thành thạo mẫu câu yêu cầu 〜てください, tiếp diễn/trạng thái 〜ています và nối chuỗi hành động."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, JapaneseChapter5Id, JapaneseLesson52Id,
            "Bài 5.2: Xin Phép (〜てもいいですか), Cấm Đoán (〜てはいけません) & Thứ Tự Hành Động",
            "ja-n5-xin-phep-cam-doan-thu-tu-hanh-dong", 2, 40,
            @"# Bài 5.2: Xin Phép (〜てもいいですか), Cấm Đoán (〜てはいけません) & Thứ Tự Hành Động

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Khi đã nắm vững Thể Te ở Bài 5.1, bạn có thể lắp ghép ngay vào 3 cấu trúc quyền lực nhất trong sinh hoạt hằng ngày và nơi công cộng tại Nhật Bản! Bài học này giúp bạn:
- Xin phép một cách lịch thiệp khi chụp ảnh, ngồi ghế, thử đồ hay về sớm bằng **`〜てもいいですか`** *(Tôi làm ... có được không?)*.
- Hiểu và sử dụng đúng mẫu câu cấm đoán, quy định nơi công cộng bằng **`〜てはいけません`** *(Không được phép làm ...)* — đồng thời biết cách **từ chối lời xin phép nhẹ nhàng** không gây mất lòng theo chuẩn văn hóa Nhật.
- Diễn đạt **trình tự hành động liên tiếp có nhấn mạnh ""sau khi làm A xong rồi mới làm B""** bằng mẫu **`V1てから、V2ます`** *(Sau khi ... thì ...)*.
- Phân biệt rõ sự khác biệt giữa chuỗi hành động `V1て、V2` và nhấn mạnh điều kiện tiên quyết `V1てから、V2`.
- Đọc hiểu các biển báo quy định công cộng quen thuộc tại bảo tàng, thư viện, tàu điện và bệnh viện ở Nhật Bản.

---

## 2. 🌱 Mở khóa 3 Cấu trúc vàng từ Thể Te (て形)

### 2.1. Xin phép lịch sự: `Vて も いいですか`
- **Cấu trúc**: `[Động từ Thể Te] + も いいですか` *(Tôi làm ... có được không ạ?)*
- **Ứng dụng**: Dùng khi hỏi ý kiến chủ quán, giáo viên, nhân viên hoặc bạn bè:
  - `ここで しゃしんを とっても いいですか。` *(Tôi chụp ảnh ở đây có được không ạ?)*
  - `この ペンを つかっても いいですか。` *(Tôi dùng cây bút này có được không?)*
  - `あした やすんでも いいですか。` *(Ngày mai tôi xin nghỉ có được không ạ?)*
- **Cách trả lời đồng ý**:
  - `はい、いいですよ。` / `ええ、どうぞ。` *(Vâng, được chứ ạ / Xin mời tự nhiên.)*

### 2.2. Cấm đoán / Quy định: `Vて は いけません`
- **Cấu trúc**: `[Động từ Thể Te] + は いけません` *(Không được phép làm ...)*
- **Ứng dụng**: Ghi trên biển báo quy định, nội quy lớp học, bệnh viện, bảo tàng hoặc khi thầy cô/bác sĩ dặn dò:
  - `びじゅつかんで しゃしんを とっては いけません。` *(Ở bảo tàng mỹ thuật không được phép chụp ảnh.)*
  - `きょうしつで たばこを すっては いけません。` *(Trong lớp học không được hút thuốc lá.)*
  - `きょうは おふろに はいっては いけません。` *(Hôm nay bạn không được tắm bồn nhé — lời bác sĩ dặn.)*
- **Mẹo văn hóa khi từ chối lời xin phép**:
  - Nếu bạn bè hoặc khách hàng hỏi `ここで たばこを すっても いいですか。`, nói thẳng `すっては いけません` với người lớn hay khách hàng sẽ hơi gay gắt!
  - Thay vào đó, người Nhật thường nói mềm đi: **`すみません、ちょっと...`** *(Xin lỗi, chỗ này hơi bất tiện một chút...)* hoặc **`ここは きんえんですから...`** *(Vì ở đây là khu vực cấm hút thuốc nên...)*.

### 2.3. Thứ tự trước sau rõ ràng: `V1て から、V2` *(Sau khi V1 xong thì V2)*
- **Cấu trúc**: `[Động từ 1 Thể Te] + から、[Động từ 2]`
- **Ý nghĩa**: Nhấn mạnh hành động 1 phải **hoàn tất trọn vẹn trước**, rồi mới thực hiện hành động 2:
  - `しゅくだいを してから、ゲームを します。` *(Sau khi làm xong bài tập về nhà, tôi mới chơi game.)*
  - `うちへ かえってから、ごはんを たべます。` *(Sau khi về đến nhà rồi tôi mới ăn cơm.)*
- **Khác biệt với `〜から` (Nguyên nhân)**:
  - `から` đứng sau câu lịch sự (`〜ますから / 〜ですから`) = **Vì ... nên**.
  - `から` đứng ngay sau Thể Te (`Vてから`) = **Sau khi ... thì**.

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **とめます (止めます)** | tomete (II) | Chỉ | Dừng xe, đỗ xe | ここに くるまを とめては いけません。 *(Không được đỗ xe ở đây.)* |
| **すいます (吸います)** | sutte (I) | Hấp | Hút (thuốc lá) | そとで たばこを すっても いいですか。 *(Ra ngoài hút thuốc được không ạ?)* |
| **はいります (入ります)** | haitte (I) | Nhập | Đi vào *(chú ý Nhóm I!)* | へやに はいっても いいですか。 *(Tôi vào phòng được không ạ?)* |
| **でます (出ます)** | dete (II) | Xuất | Đi ra ngoài | きょうしつを でて ください。 *(Hãy đi ra khỏi lớp học.)* |
| **さわります (触ります)** | sawatte (I) | Xúc | Chạm vào, sờ vào | えに さわっては いけません。 *(Không được chạm tay vào bức tranh.)* |
| **おきます (置きます)** | oite (I) | Trí | Đặt, để xuống | ここに かばんを おいても いいですか。 *(Tôi đặt cặp ở đây có được không?)* |
| **だします (出します)** | dashite (I) | Xuất | Nộp (bài), gửi (thư) | レポートを だしてから、かえります。 *(Nộp báo cáo xong tôi mới về.)* |
| **あらいます (洗います)** | aratte (I) | Tẩy | Rửa (tay, mặt, bát) | てを あらってから、たべます。 *(Rửa tay xong rồi mới ăn.)* |
| **あびます (浴びます)** | abite (II) | Dục | Tắm (vòi sen) | まいばん シャワーを あびます。 *(Mỗi tối tôi tắm vòi hoa sen.)* |
| **かえします (返します)** | kaeshite (I) | Phản | Trả lại (đồ mượn) | あした ほんを かえしても いいですか。 *(Ngày mai tôi trả sách có được không?)* |
| **もちかえります** | mochikaette (I) | Trì Quy | Mang về nhà | りょうりを もちかえっても いいですか。 *(Tôi mang đồ ăn về được không?)* |
| **しゅくだい (宿題)** | shukudai | Túc Đề | Bài tập về nhà | しゅくだいを してから、ねます。 *(Làm bài tập xong rồi mới đi ngủ.)* |
| **びじゅつかん (美術館)** | bijutsukan | Mỹ Thuật Quán | Bảo tàng mỹ thuật | びじゅつかんで しゃしんを とってはいけません。 *(Không được chụp ảnh trong bảo tàng.)* |
| **きんえん (禁煙)** | kin'en | Cấm Yên | Cấm hút thuốc | この レストランは きんえんです。 *(Nhà hàng này cấm hút thuốc.)* |
| **おねがいします** | onegaishimasu | Nguyện | Làm ơn, nhờ anh/chị | コピーを おねがいします。 *(Nhờ anh/chị photo giúp.)* |

---

## 4. Ngữ pháp tinh tế: Nhóm I giả Nhóm II & Phân biệt hai loại `から`

### 4.1. Cẩn thận động từ **Nhóm I giả dạng Nhóm II** (`はいります`, `かえります`, `きります`, `しります`)
- Một số động từ có chữ trước `ます` là `り` (thuộc cột I, không phải cột E, nhưng nhìn dạng từ điển `〜る` rất dễ tưởng nhầm Nhóm II!):
  - **`はいります` (vào)** → Nhóm I! Thể Te là **`はいって`** *(KHÔNG phải `はいりて`)*.
  - **`かえります` (về)** → Nhóm I! Thể Te là **`かえって`**.
  - **`きります` (cắt)** → Nhóm I! Thể Te là **`きって`**.
  - **`しります` (biết)** → Nhóm I! Thể Te là **`しって`**.

### 4.2. Giới hạn 1 lần dùng `〜てから` trong một câu
- Trong tiếng Nhật chuẩn, cấu trúc **`〜てから`** chỉ dùng **1 lần duy nhất trong một câu** để nhấn mạnh cột mốc chuyển tiếp quan trọng nhất. Nếu có 3 hành động, các hành động trước nối bằng Thể Te thường:
  - *ĐÚNG*: `うちへ かえって、てを あらってから、ごはんを たべます。` *(Về nhà, rồi sau khi rửa tay xong thì ăn cơm.)*
  - *SAI*: `うちへ かえってから、てを あらってから、たべます。` *(Lặp hai lần てから nghe nặng nề!)*

### 4.3. So sánh `〜てもいいですか` (Xin phép) vs `〜ませんか` (Rủ rê)
- `コーヒーを のんでも いいですか。` = *Tôi uống cà phê (này) có được không?* (Xin phép cho bản thân mình làm).
- `コーヒーを のみませんか。` = *Bạn có muốn uống cà phê không?* (Mời rủ đối phương cùng làm).

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: ここで しゃしんを とって いけません。 (Thiếu trợ từ は trong mẫu cấm đoán)
   ⭕ ĐÚNG: ここで しゃしんを とっては いけません。
   💡 GIẢI THÍCH: Trong cấu trúc cấm đoán, trợ từ は (đọc là wa) là thành phần BẮT BUỘC phải có!

2. ❌ SAI: へやに はいりても いいですか。 (Chia nhầm はいります sang Nhóm II)
   ⭕ ĐÚNG: へやに はいっても いいですか。
   💡 GIẢI THÍCH: はいります (vào) là động từ Nhóm I có đuôi り, nên chia thành はいって.

3. ❌ SAI: しゅくだいを しますから、ねます。 (Dùng ますから khi muốn nói ""Sau khi làm bài xong tôi đi ngủ"")
   ⭕ ĐÚNG: しゅくだいを してから、ねます。
   💡 GIẢI THÍCH: Vます + から nghĩa là ""Vì ... nên...""; muốn nói ""Sau khi làm xong ... thì..."" bắt buộc dùng Thể Te + から!

4. ❌ SAI: あした テストが ありますから、べんきょうして ください。 → Nghĩa đúng nhưng lưu ý:
   ❌ SAI: つかれてから、やすみます。 (Nhầm ""vì mệt nên nghỉ"" thành ""sau khi mệt mới nghỉ"")
   ⭕ ĐÚNG: つかれましたから、やすみます。 (Nguyên nhân: Vì mệt rồi nên nghỉ)
   💡 GIẢI THÍCH: Diễn đạt lý do thì chia thể lịch sự (〜ます/〜ました) + から, không chia Thể Te + から.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Minh đi tham quan một bảo tàng nghệ thuật cổ ở Kyoto và hỏi chuyện nhân viên hướng dẫn.

- **Minh**: すみません、この へやに はいっても いいですか。
  - *Romaji*: Sumimasen, kono heya ni haitte mo ii desu ka?
  - *Phân tích*: `はいっても いいですか` (xin phép đi vào phòng).
  - *Dịch*: Xin lỗi anh, tôi vào căn phòng này có được không ạ?

- **Nhân viên**: はい、どうぞ。でも、この かばんは おおきいですから、ここに おいて ください。
  - *Romaji*: Hai, douzo. Demo, kono kaban wa ookii desu kara, koko ni oite kudasai.
  - *Phân tích*: `おおきいですから` (vì to nên) + `おいて ください` (xin hãy đặt ở đây).
  - *Dịch*: Vâng, xin mời anh. Nhưng chiếc ba lô này hơi to nên anh vui lòng gửi ở đây giúp tôi ạ.

- **Minh**: わかりました。あのう、中で しゃしんを とっても いいですか。
  - *Romaji*: Wakarimashita. Anou, naka de shashin o totte mo ii desu ka?
  - *Phân tích*: `とっても いいですか` (xin phép chụp ảnh).
  - *Dịch*: Vâng, tôi hiểu rồi. À xin hỏi, bên trong tôi chụp ảnh có được không?

- **Nhân viên**: すみません、びじゅつかんですから、しゃしんを とっては いけません。えにも さわらないで ください。
  - *Romaji*: Sumimasen, bijutsukan desu kara, shashin o totte wa ikemasen. E ni mo sawaranaide kudasai.
  - *Phân tích*: `とっては いけません` (không được phép chụp ảnh).
  - *Dịch*: Xin lỗi anh, vì đây là bảo tàng mỹ thuật nên không được phép chụp ảnh ạ. Xin cũng đừng chạm tay vào tranh nhé.

- **Minh**: はい、きをつけます。では、みてから、おみやげやへ いきます。
  - *Romaji*: Hai, ki o tsukemasu. De wa, mite kara, omiyageya e ikimasu.
  - *Phân tích*: `みてから` (sau khi xem xong thì) + `おみやげや` (cửa hàng quà lưu niệm).
  - *Dịch*: Vâng, tôi sẽ chú ý. Vậy sau khi tham quan xong, tôi sẽ sang cửa hàng quà lưu niệm ạ.

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện cấu trúc xin phép, cấm đoán & thứ tự
1. Viết câu xin phép: *""Tôi ngồi ở đây có được không?""*
2. Viết câu quy định: *""Không được ăn uống trong thư viện.""*
3. Nối 2 vế bằng `〜てから`: *(Ăn sáng xong → đi đến trường)* `あさごはんを たべます` + `がっこうへ いきます`.

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: `ここに すわっても いいですか。`
2. **Câu 2**: `としょかんで たべたり のんだりしては いけません。` *(Ở mức N5: `としょかんで たべては いけません。`)*
3. **Câu 3**: `あさごはんを たべてから、がっこうへ いきます。`
4. **Câu 4 trắc nghiệm**: Trong bảo tàng có biển ghi ""撮影禁止"" (Cấm quay phim chụp ảnh). Câu tiếng Nhật tương ứng là gì?
   - *Đáp án*: `しゃしんを とっては いけません。`
5. **Câu 5 trắc nghiệm**: Phân biệt nghĩa của câu `かいしゃを やすんでから、びょういんへ いきました。`
   - *Đáp án*: *Sau khi xin nghỉ làm ở công ty, tôi đã đi đến bệnh viện.* (`Vてから` = sau khi làm V1 thì làm V2).

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Hãy phân biệt thật chắc `Vます + から` *(vì...)* và `Vて + から` *(sau khi...)* bằng cách làm trọn bộ 4 bài **Cloze & Translate** ngay bên dưới!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Sử dụng thành thạo mẫu xin phép 〜てもいいですか, cấm đoán 〜てはいけません và thứ tự hành động 〜てから.",
            "Tránh nhầm lẫn động từ Nhóm I giả Nhóm II (はいります, かえります) và phân biệt hai cấu trúc chứa から."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, JapaneseChapter5Id, JapaneseLesson53Id,
            "Bài 5.3: Ý Muốn (〜たいです), Kế Hoạch Tương Lai & Hội Thoại Du Lịch/Công Việc Nhật Bản",
            "ja-n5-y-muon-ke-hoach-du-lich-cong-viec", 3, 45,
            @"# Bài 5.3: Ý Muốn (〜たいです), Kế Hoạch Tương Lai & Hội Thoại Du Lịch/Công Việc Nhật Bản

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Xin chúc mừng bạn đã đi tới bài học tổng kết cuối cùng của khóa Tiếng Nhật Sơ Cấp (JLPT N5)! Bài học 5.3 sẽ trang bị cho bạn những cấu trúc giao tiếp đắt giá để **bày tỏ ước muốn cá nhân, kế hoạch tương lai và ứng biến tự tin trong môi trường du lịch lẫn công sở Nhật Bản**:
- Diễn đạt **mong muốn làm gì của bản thân** bằng cấu trúc **`Vます (bỏ ます) + たいです`** *(Tôi muốn làm ...)* và **muốn sở hữu đồ vật gì** bằng **`N が ほしいです`** *(Tôi muốn có ...)*.
- Chia mượt mà mẫu `〜たいです` giống như một **Tính từ đuôi -i** (`〜たくないです`, `〜たかったです`, `〜たくなかったです`).
- Sử dụng thành thạo cấu trúc chỉ **mục đích di chuyển**: **`[Địa điểm] へ [Động từ bỏ ます / Danh từ] に いきます / きます / かえります`** *(Đi đâu để làm gì)*.
- Thực hành nói về **kế hoạch, dự định, ước mơ nghề nghiệp** và các cụm giao tiếp lịch sự nơi công sở / du lịch Nhật Bản (`おねがいします`, `ごちゅうもん`, `よやく`, `おさきに しつれいします`).
- Tổng hợp hệ thống ngữ pháp từ Chương 1 đến Chương 5 chuẩn bị vững vàng cho kỳ thi năng lực Nhật ngữ **JLPT N5**.

---

## 2. 🌱 Diễn đạt Ý Muốn (`〜たいです` / `ほしいです`) & Mục Đích (`〜に いきます`)

### 2.1. Phân biệt ""Muốn có vật gì"" (`ほしい`) vs ""Muốn làm gì"" (`〜たい`)
1. **Muốn có một Danh từ (vật / người / thời gian)**:
   - Công thức: **`[Danh từ] が ほしいです`** *(Tôi muốn có ...)*
   - Chú ý: `ほしい` là **Tính từ đuôi -i**! Dùng trợ từ **`が`** (không dùng `を`).
   - Phủ định: `ほしくないです` *(không muốn)*; Quá khứ: `ほしかったです` *(đã từng muốn)*.
   - Ví dụ: `あたらしい パソコンが ほしいです。` *(Tôi muốn có một chiếc laptop mới.)*
   - *Mẹo*: Hỏi bạn bè muốn quà gì: `なにが いちばん ほしいですか。` *(Bạn muốn nhất thứ gì?)*

2. **Muốn thực hiện một Hành động**:
   - Công thức: **`[Động từ bỏ ます] + たいです`** *(Tôi muốn làm ...)*
   - Cách tạo gốc: Bỏ `ます` giữ nguyên phần thân (`たべます → たべたい`, `いきます → いきたい`, `みます → みたい`, `します → したい`).
   - Chia thì y hệt **Tính từ đuôi -i**:
     - Phủ định: `たべたくないです` *(không muốn ăn)*
     - Quá khứ: `いきたかったです` *(đã muốn đi)*
   - Trợ từ tân ngữ: Dùng **`が`** hoặc **`を`** đều đúng (`すしを たべたいです` / `すしが たべたいです`), nhưng với động từ di chuyển như `いきたい` vẫn giữ `へ` (`にほんへ いきたいです`).

> [!WARNING]
> **Quy tắc ngôi thứ nhất**: Trong tiếng Nhật N5, `〜が ほしいです` và `〜たいです` chỉ dùng để nói về **mong muốn của chính BẢN THÂN MÌNH** hoặc **hỏi trực tiếp người đối diện (`〜たいですか`)**. Không dùng thẳng `たなかさんは くるまが ほしいです` để kể về người thứ 3!

### 2.2. Diễn đạt Mục Đích Chuyến Đi: `[Địa điểm] へ [Mục đích] に いきます / きます / かえります`
- **Cấu trúc 1 (với Động từ bỏ ます)**:
  - `デパートへ くつを かいに いきます。` *(Tôi đi trung tâm thương mại để mua giày.)*
  - `こうえんへ さくらを みに いきましょう！` *(Cùng ra công viên ngắm hoa anh đào nào!)*
- **Cấu trúc 2 (với Danh từ hành động — lược bỏ `します`)**:
  - `にほんへ べんきょうに きました。` *(Tôi đã đến Nhật để học tập.)*
  - `しゅうまつに きょうとへ りょこうに いきます。` *(Cuối tuần tôi sẽ đi du lịch đến Kyoto.)*

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **ほしい (欲しい)** | hoshii | Dục | Muốn có (đồ vật) | いま なにが いちばん ほしいですか。 *(Bây giờ bạn muốn thứ gì nhất?)* |
| **りょこう (旅行)** | ryokou | Lữ Hành | Du lịch | にほんへ りょこうに いきたいです。 *(Tôi muốn đi du lịch Nhật Bản.)* |
| **よやく (予約)** | yoyaku | Dự Ước | Đặt trước (vé, phòng, bàn) | ホテルを よやくしたいです。 *(Tôi muốn đặt phòng khách sạn.)* |
| **ホテル / くうこう** | hoteru / kuukou | — / Không Cảng | Khách sạn / Sân bay | くうこうまで バスで いきます。 *(Tôi đi xe buýt ra sân bay.)* |
| **おみやげ (お土産)** | omiyage | Thổ Sản | Quà lưu niệm | かぞくに おみやげを かいたいです。 *(Tôi muốn mua quà lưu niệm cho gia đình.)* |
| **けいけん (経験)** | keiken | Kinh Nghiệm | Kinh nghiệm | いろいろな けいけんを したいです。 *(Tôi muốn có nhiều trải nghiệm đa dạng.)* |
| **しょうらい (将来)** | shourai | Tương Lai | Tương lai | しょうらい にほんで はたらきたいです。 *(Tương lai tôi muốn làm việc ở Nhật.)* |
| **ゆめ (夢)** | yume | Mộng | Giấc mơ, ước mơ | わたしの ゆめは エンジニアに なることです。 *(Ước mơ của tôi là trở thành kỹ sư.)* |
| **けしき (景色)** | keshiki | Cảnh Sắc | Phong cảnh | ふじさんの けしきを みに いきます。 *(Tôi đi ngắm phong cảnh núi Phú Sĩ.)* |
| **おんせん (温泉)** | onsen | Ôn Tuyền | Suối nước nóng | おんせんに はいりたいですね。 *(Tôi muốn được tắm suối nước nóng quá nhỉ.)* |
| **かいぎ (会議)** | kaigi | Hội Nghị | Cuộc họp | ごごから かいぎに でます。 *(Từ chiều tôi tham dự cuộc họp.)* |
| **しゅっちょう (出張)** | shucchou | Xuất Trương | Đi công tác | らいしゅう おおさかへ しゅっちょうします。 *(Tuần sau tôi đi công tác Osaka.)* |
| **つかれました (疲れました)** | tsukaremashita | Bì | Tôi mệt rồi | きょうは とても つかれました。 *(Hôm nay tôi mệt lắm rồi.)* |
| **おなかが すきました** | onaka ga sukimashita | — | Tôi đói bụng rồi | おなかが すきましたから、なにか たべたいです。 *(Đói rồi nên tôi muốn ăn gì đó.)* |
| **のどが かわきました** | nodo ga kawakimashita | — | Tôi khát nước rồi | のどが かわきました。みずを のみたいです。 *(Khát nước quá. Tôi muốn uống nước.)* |

---

## 4. Ngữ pháp tổng hợp Du lịch & Công sở + 4 Bẫy Sai Kinh Điển

### 4.1. Cụm giao tiếp ""sống còn"" khi Du lịch & Làm việc tại Nhật
- **Khi đặt món / đặt phòng lịch sự**: Dùng **`〜を おねがいします`** hoặc **`〜を したいんですが...`** *(Tôi muốn ... ạ, không biết có được không?)*:
  - `チェックインを おねがいします。` *(Làm ơn cho tôi làm thủ tục nhận phòng.)*
  - `あしたの よやくを したいんですが...` *(Tôi muốn đặt lịch cho ngày mai ạ...)*
- **Khi tan làm rời công ty (Văn hóa Nhật bắt buộc!)**:
  - Người về trước nói với đồng nghiệp ở lại: **`おさきに しつれいします。`** *(Tôi xin phép về trước ạ.)*
  - Người ở lại đáp: **`おつかれさまでした。`** *(Bạn hôm nay đã vất vả rồi ạ.)*

### 4.2. Đừng nhầm mục đích `V(bỏ ます) + に いきます` với Thể Te!
- Mục đích đến một nơi: **KHÔNG dùng Thể Te**, phải dùng **gốc `ます`** + **`に`** + `いきます/きます/かえります`!
  - *ĐÚNG*: `ともだちと えいがを みに いきます。` *(Tôi đi xem phim cùng bạn.)*
  - *SAI*: `えいがを みて いきます。` *(Câu này thành ""Xem phim xong rồi mới đi"", sai nghĩa hoàn toàn!)*

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: わたしは あたらしい スマホを ほしいです。 (Dùng trợ từ を với ほしい)
   ⭕ ĐÚNG: わたしは あたらしい スマホが ほしいです。
   💡 GIẢI THÍCH: ほしい là tính từ chỉ mong muốn đối tượng, luôn đi với trợ từ が!

2. ❌ SAI: わたしは にほんへ いきたい ほしいです。 (Ghép cả たい và ほしい vào nhau)
   ⭕ ĐÚNG: わたしは にほんへ いきたいです。
   💡 GIẢI THÍCH: Muốn LÀM hành động dùng Vたいです; muốn CÓ đồ vật dùng Nがほしいです. Không bao giờ ghép chung hai từ này!

3. ❌ SAI: こうえんへ さくらを みるに いきます。 (Giữ nguyên thể từ điển みる trước に)
   ⭕ ĐÚNG: こうえんへ さくらを みに いきます。 (Bỏ ます của みます → み rồi gắn に)
   💡 GIẢI THÍCH: Công thức mục đích bắt buộc bỏ ます, không dùng thể nguyên mẫu る.

4. ❌ SAI: おなかが いたいから、なにも たべたいではありません。 (Phủ định たい sai thành たいではありません)
   ⭕ ĐÚNG: おなかが いたいですから、なにも たべたくないです。 (Chia như tính từ đuôi -i)
   💡 GIẢI THÍCH: Đuôi 〜たい biến đổi hoàn toàn như tính từ đuôi -i: phủ định là 〜たくないです, quá khứ là 〜たかったです.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Buổi chiều thứ Sáu cuối tuần ở văn phòng Tokyo. Anh Suzuki trò chuyện với Minh về kế hoạch kỳ nghỉ cuối tuần và dự định tương lai.

- **Suzuki**: ミンさん、こんしゅうの しゅうまつは なにを したいですか。
  - *Romaji*: Min-san, konshuu no shuumatsu wa nani o shitai desu ka?
  - *Phân tích*: `なにを したいですか` (bạn muốn làm gì?).
  - *Dịch*: Minh ơi, cuối tuần này bạn muốn làm gì thế?

- **Minh**: ともだちと きょうとへ りょこうに いきたいです。おてらを みて、おんせんに はいりたいです。
  - *Romaji*: Tomodachi to Kyouto e ryokou ni ikitai desu. Otera o mite, onsen ni hairitai desu.
  - *Phân tích*: `りょこうに いきたいです` (muốn đi du lịch) + `みて、〜はいりたいです` (xem chùa rồi tắm suối nước nóng).
  - *Dịch*: Em muốn đi du lịch Kyoto cùng bạn ạ. Em muốn đi ngắm chùa rồi ngâm mình ở suối nước nóng.

- **Suzuki**: いいですね！きょうとは まちが きれいですから、たのしいですよ。ホテルは もう よやくしましたか。
  - *Romaji*: Ii desu ne! Kyouto wa machi ga kirei desu kara, tanoshii desu yo. Hoteru wa mou yoyaku shimashita ka?
  - *Phân tích*: `もう よやくしましたか` (đã đặt phòng trước chưa?).
  - *Dịch*: Tuyệt quá nhỉ! Kyoto phố xá đẹp nên đi vui lắm đấy. Bạn đã đặt khách sạn chưa?

- **Minh**: いいえ、これから ネットで よやくします。あのう、さとうさんと すずきさんに おみやげを かいたいです。なにが いいですか。
  - *Romaji*: Iie, korekara netto de yoyaku shimasu. Anou, Satou-san to Suzuki-san ni omiyage o kaitai desu. Nani ga ii desu ka?
  - *Phân tích*: `これから` (từ bây giờ/ngay bây giờ) + `おみやげを かいたい` (muốn mua quà lưu niệm).
  - *Dịch*: Dạ chưa, lát nữa em sẽ đặt qua mạng ạ. À anh ơi, em muốn mua quà cho anh Sato và anh. Mua gì thì hay ạ?

- **Suzuki**: きょうとの まっちゃの おかしが おいしいですよ！では、きょうは これで おわりに しましょう。
  - *Romaji*: Kyouto no maccha no okashi ga oishii desu yo! De wa, kyou wa kore de owari ni shimashou.
  - *Phân tích*: `まっちゃの おかし` (bánh ngọt vị trà xanh).
  - *Dịch*: Bánh trà xanh Kyoto ngon lắm đấy! Thôi hôm nay mình kết thúc công việc ở đây nhé.

- **Minh**: はい、おつかれさまでした！おさきに しつれいします！
  - *Romaji*: Hai, otsukaresama deshita! Osaki ni shitsurei shimasu!
  - *Phân tích*: Cặp câu chào tan làm chuẩn mực công sở Nhật.
  - *Dịch*: Vâng, anh đã vất vả rồi ạ! Em xin phép về trước ạ!

---

## 6. Bài tập tự luyện & Tổng ôn N5 cuối khóa

### Phần A: Luyện cấu trúc ý muốn & mục đích
1. Chuyển sang câu phủ định quá khứ: `わたしは きのう べんきょうしたいです。` *(Hôm qua tôi đã không muốn học).*
2. Điền từ/trợ từ đúng: `デパート ___ ふく ___ かい ___ いきます。`
3. Dịch sang tiếng Nhật: *""Vì tôi đói bụng rồi nên tôi muốn ăn mì Ramen.""*

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: `わたしは きのう べんきょうしたくなかったです。` *(Bỏ い của たい → くなかったです).*
2. **Câu 2**: `デパート へ ふく を かい に いきます。` *(Mục đích hành động: bỏ ます của かいます → かい + に).*
3. **Câu 3**: `おなかが すきましたから、ラーメンを たべたいです。`
4. **Câu 4 trắc nghiệm**: Để nói ""Tôi muốn có một chiếc xe đạp mới"", câu đúng là gì?
   - *Đáp án*: `あたらしい じてんしゃが ほしいです。` *(Dùng trợ từ が với ほしい).*
5. **Câu 5 trắc nghiệm**: Khi bạn về nhà trước đồng nghiệp ở công ty Nhật, câu nói lịch sự chuẩn nhất là gì?
   - *Đáp án*: `おさきに しつれいします。`

</details>

> [!TIP]
> **Chặng cuối N5:** Sau khi hoàn tất 8 hoạt động thực hành ở Bài 5.3, hãy thử sức ngay với **`[Đề Chương 5]`** để kiểm tra toàn bộ kiến thức Thể Te, Xin phép, Cấm đoán và Ý muốn ở trình độ JLPT N5!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Làm chủ diễn đạt ý muốn cá nhân (〜たいです, Nがほしいです) và mục đích di chuyển (〜に いきます/きます).",
            "Ứng dụng giao tiếp du lịch, đặt phòng/vé và phép lịch sự công sở Nhật Bản (おさきに しつれいします).")
    };

    // =========================================================================
    // KHÓA 1: TIẾNG NHẬT N5+ — HOẠT ĐỘNG THỰC HÀNH (8 LOẠI x 6 BÀI = 48 ACTIVITIES)
    // =========================================================================

    private static List<PracticeActivity> GetJapaneseExpandedActivities(Guid cId, DateTimeOffset now)
    {
        var all = new List<PracticeActivity>();

        // ---------------- LESSON 4.1 ----------------
        all.AddRange(BuildLessonPack(cId, JapaneseLesson41Id, "ja-JP", "Nhật N5 · Bài 4.1",
            shadowing: new[]
            {
                new ShItem("ja-l41-sh-1", "easy", "Thời tiết hôm nay (Dễ)", "きょうはとてもあついです。", "きょう は とても あつい です / kyou wa totemo atsui desu", "Hôm nay trời rất nóng.", new[] { "きょう", "とても", "あつい" }),
                new ShItem("ja-l41-sh-2", "easy", "Mùa đông lạnh (Dễ)", "ふゆはさむいですね。", "ふゆ は さむい です ね / fuyu wa samui desu ne", "Mùa đông lạnh quá nhỉ.", new[] { "ふゆ", "さむい" }),
                new ShItem("ja-l41-sh-3", "easy", "Tính từ đuôi -na (Dễ)", "さくらはとてもきれいです。", "さくら は とても きれい です / sakura wa totemo kirei desu", "Hoa anh đào rất đẹp.", new[] { "さくら", "きれい" }),
                new ShItem("ja-l41-sh-4", "medium", "Phủ định tính từ -i (Vừa)", "このテストはむずかしくないです。", "この テスト は むずかしくない です / kono tesuto wa muzukashikunai desu", "Bài kiểm tra này không khó.", new[] { "テスト", "むずかしくない" }),
                new ShItem("ja-l41-sh-5", "medium", "Quá khứ của いい (Vừa)", "きのうのてんきはよかったです。", "きのう の てんき は よかった です / kinou no tenki wa yokatta desu", "Thời tiết hôm qua đã rất đẹp.", new[] { "てんき", "よかった" }),
                new ShItem("ja-l41-sh-6", "medium", "Phó từ あまり + phủ định (Vừa)", "このコーヒーはあまりおいしくないです。", "この コーヒー は あまり おいしくない です / kono koohii wa amari oishikunai desu", "Ly cà phê này không ngon lắm.", new[] { "あまり", "おいしくない" }),
                new ShItem("ja-l41-sh-7", "hard", "Nối 2 tính từ (Khó)", "このレストランはやすくておいしいです。", "この レストラン は やすくて おいしい です / kono resutoran wa yasukute oishii desu", "Nhà hàng này vừa rẻ vừa ngon.", new[] { "やすくて", "おいしい" }),
                new ShItem("ja-l41-sh-8", "hard", "Quá khứ phủ định -i (Khó)", "きのうのえいがはぜんぜんおもしろくなかったです。", "きのう の えいが は ぜんぜん おもしろくなかった です / kinou no eiga wa zenzen omoshirokunakatta desu", "Bộ phim hôm qua hoàn toàn không hay chút nào.", new[] { "ぜんぜん", "おもしろくなかった" }),
                new ShItem("ja-l41-sh-9", "hard", "Hai vế đối lập với が (Khó)", "にほんごはおもしろいですが、かんじがむずかしいです。", "にほんご は おもしろい です が、かんじ が むずかしい です / nihongo wa omoshiroi desu ga, kanji ga muzukashii desu", "Tiếng Nhật thú vị nhưng chữ Hán khó.", new[] { "おもしろいですが", "むずかしい" })
            },
            dictation: new[]
            {
                new DiItem("ja-l41-di-1", "easy", "Trời rất nóng", "きょうはとてもあついです", "kyou wa totemo atsui desu", "Hôm nay rất nóng.", new[] { "きょうは", "とても", "あつい", "です" }, new[] { "さむい", "あまり", "でした" }),
                new DiItem("ja-l41-di-2", "easy", "Công viên yên tĩnh", "このこうえんはしずかです", "kono kouen wa shizuka desu", "Công viên này yên tĩnh.", new[] { "この", "こうえんは", "しずか", "です" }, new[] { "にぎやか", "くない", "でした" }),
                new DiItem("ja-l41-di-3", "medium", "Không đắt lắm", "そのかばんはあまりたかくないです", "sono kaban wa amari takakunai desu", "Chiếc túi đó không đắt lắm.", new[] { "そのかばんは", "あまり", "たかくない", "です" }, new[] { "とても", "たかいです", "やすい" }),
                new DiItem("ja-l41-di-4", "medium", "Hôm qua thật tuyệt", "きのうはとてもよかったです", "kinou wa totemo yokatta desu", "Hôm qua thật tuyệt vời.", new[] { "きのうは", "とても", "よかった", "です" }, new[] { "いかった", "よくない", "あした" }),
                new DiItem("ja-l41-di-5", "hard", "Thành phố đẹp và náo nhiệt", "ハノイはきれいでにぎやかなまちです", "Hanoi wa kirei de nigiyaka na machi desu", "Hà Nội là thành phố đẹp và náo nhiệt.", new[] { "ハノイは", "きれいで", "にぎやかな", "まちです" }, new[] { "きれいくて", "しずか", "でした" }),
                new DiItem("ja-l41-di-6", "hard", "Phủ định quá khứ tính từ -na", "せんしゅうのにちようびはひまではありませんでした", "senshuu no nichiyoubi wa hima dewa arimasen deshita", "Chủ nhật tuần trước tôi đã không rảnh.", new[] { "せんしゅうの", "にちようびは", "ひま", "ではありませんでした" }, new[] { "ひまくなかった", "ひまです", "あした" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l41-pr-1", "easy", "Tính từ あたたかい", "はるはあたたかいです", "haru wa atatakai desu", "Mùa xuân ấm áp.", new[] { "はる", "あたたかい" }, "Đọc rõ từng nhịp a-ta-ta-ka-i, không nuốt âm giữa.", new[] { "Đọc rõ hai âm ta liên tiếp", "Lên giọng nhẹ ở か rồi hạ ở い", "Khép môi nhẹ ở です" }, "はるはあたたかくてきもちがいいです。", new[] { "はる", "あたたかい" }),
                new PrItem("ja-l41-pr-2", "easy", "Ngoại lệ きれい", "さくらはきれいです", "sakura wa kirei desu", "Hoa anh đào rất đẹp.", new[] { "さくら", "きれい" }, "Chạm nhẹ lưỡi âm ら và kéo dài âm け-れい (kirē).", new[] { "さ đọc nhẹ", "れい kéo dài tự nhiên như rē", "Đọc dứt khoát です" }, "にほんのさくらはとてもきれいです。", new[] { "さくら", "きれい" }),
                new PrItem("ja-l41-pr-3", "medium", "Âm ngắt quá khứ よかった", "きのうはよかったです", "kinou wa yokatta desu", "Hôm qua rất tốt.", new[] { "きのう", "よかった" }, "Kéo dài trường âm のう và giữ 1 nhịp ngắt rõ ở っ của よかった.", new[] { "きのう kéo dài 2 phách", "Nén hơi っ giữa か và た", "Giữ tông giọng hài lòng" }, "てんきがよくてよかったです。", new[] { "よくて", "よかった" }),
                new PrItem("ja-l41-pr-4", "medium", "Tính từ むずかしい", "かんじはむずかしくないです", "kanji wa muzukashikunai desu", "Chữ Hán không khó.", new[] { "かんじ", "むずかしくない" }, "Đọc đều nhịp 7 âm tiết mu-zu-ka-shi-ku-na-i.", new[] { "ず rung nhẹ dây thanh", "し đọc êm không chu môi", "Đuôi くない rõ ràng" }, "ひらがなはあまりむずかしくないです。", new[] { "あまり", "むずかしくない" }),
                new PrItem("ja-l41-pr-5", "hard", "Trường âm すずしい", "あきはすずしくてすごしやすいです", "aki wa suzushikute sugoshiyasui desu", "Mùa thu mát mẻ dễ chịu.", new[] { "あき", "すずしくて" }, "Phát âm đúng trường âm し-い (shī) trước khi chuyển sang くて.", new[] { "す đầu lưỡi nhẹ", "ず rung nhẹ", "しい kéo dài rồi ngắt nhẹ ở く" }, "あきはすずしくてとてもいいきせつです。", new[] { "すずしくて", "きせつ" }),
                new PrItem("ja-l41-pr-6", "hard", "Phủ định quá khứ dài", "ぜんぜんおもしろくなかったです", "zenzen omoshirokunakatta desu", "Hoàn toàn không thú vị chút nào.", new[] { "ぜんぜん", "おもしろくなかった" }, "Giữ chắc âm mũi ん trong ぜんぜん và âm ngắt っ trong なかった.", new[] { "ぜんぜん bật rõ z và ん", "Đọc mượt おもしろく", "Nén hơi 1 nhịp っ trước た" }, "きのうのえいがはぜんぜんおもしろくなかったです。", new[] { "ぜんぜん", "おもしろくなかった" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l41-wm-1", "easy", "Sơ đồ từ vựng: てんき (Thời tiết)", "てんき (天気)", "てんき (tenki)", "Thời tiết",
                    new[] { new WmNode("いい てんき", "ii tenki", "Thời tiết đẹp", "きょうは いい てんきですね。"), new WmNode("てんきよほう", "tenkiyohou", "Dự báo thời tiết", "あさの てんきよほうを みます。") },
                    new[] { new WmNode("はれ / あめ / くもり", "hare / ame / kumori", "Nắng / Mưa / Nhiều mây", "あしたは はれです。") },
                    new WmNode[] { new WmNode("きせつ (季節)", "kisetsu", "Mùa (4 mùa)", "はる・なつ・あき・ふゆ") },
                    new[] { new WmEx("きのうは あめでしたが、きょうは いい てんきです。", "kinou wa ame deshita ga, kyou wa ii tenki desu", "Hôm qua trời mưa nhưng hôm nay thời tiết rất đẹp.") }),
                new WmItem("ja-l41-wm-2", "medium", "Sơ đồ từ vựng: きれい (Đẹp / Sạch sẽ)", "きれい (綺麗)", "きれい (kirei)", "Đẹp, sạch sẽ (Tính từ -na)",
                    new[] { new WmNode("きれいな まち", "kirei na machi", "Thành phố xinh đẹp", "きょうとは きれいな まちです。"), new WmNode("へやを きれいにする", "heya o kirei ni suru", "Dọn phòng sạch sẽ", "まいしゅう へやを きれいにします。") },
                    new[] { new WmNode("きれいで しずか", "kirei de shizuka", "Đẹp và yên tĩnh", "ここは きれいで しずかですね。") },
                    new WmNode[] { new WmNode("うつくしい", "utsukushii", "Xinh đẹp (trang trọng)", "うつくしい けしきです。") },
                    new[] { new WmEx("この こうえんの さくらは とても きれいです。", "kono kouen no sakura wa totemo kirei desu", "Hoa anh đào ở công viên này rất đẹp.") }),
                new WmItem("ja-l41-wm-3", "hard", "Sơ đồ từ vựng: いい / よい (Tốt, hay)", "いい (良い)", "いい / よい (ii / yoi)", "Tốt, đẹp, được",
                    new[] { new WmNode("よくないです", "yokunai desu", "Không tốt", "からだに よくないです。"), new WmNode("よかったです", "yokatta desu", "Đã tốt / May quá!", "ごうかくして よかったです。") },
                    new[] { new WmNode("きもちがいい", "kimochi ga ii", "Dễ chịu, sảng khoái", "あさの かぜは きもちがいいです。") },
                    new WmNode[] { new WmNode("すばらしい", "subarashii", "Tuyệt vời", "すばらしい けいけんです。") },
                    new[] { new WmEx("きのうの コンサートは とても よかったです。", "kinou no konsaato wa totemo yokatta desu", "Buổi hòa nhạc hôm qua đã rất tuyệt.") })
            },
            cloze: new[]
            {
                new ClItem("ja-l41-cl-1", "easy", "Chèn な trước danh từ", "Điền từ nối đúng giữa tính từ đuôi -na và danh từ:", "ここは とても にぎやか{{0}} まちです。", new[] { new ClBlank(new[] { "な" }, "trợ từ nối tính từ đuôi -na", "Tính từ đuôi -na đứng trước danh từ bắt buộc thêm な.") }, "ここは とても にぎやかな まちです。", "koko wa totemo nigiyaka na machi desu", "Đây là một khu phố rất náo nhiệt.", "にぎやか là tính từ đuôi -na nên phải có な trước まち."),
                new ClItem("ja-l41-cl-2", "easy", "Phó từ あまり + Phủ định", "Chia tính từ おいしい sao cho khớp với phó từ あまり:", "この ラーメンは あまり おいし{{0}}。", new[] { new ClBlank(new[] { "くないです", "くありません" }, "không ngon lắm", "あまり luôn đi với thể phủ định (くないです).") }, "この ラーメンは あまり おいしくないです。", "kono raamen wa amari oishikunai desu", "Mì ramen này không ngon lắm.", "Bỏ い của おいしい và thêm くないです."),
                new ClItem("ja-l41-cl-3", "medium", "Chia quá khứ ngoại lệ いい", "Chia いい (tốt) sang quá khứ cho 'Hôm qua':", "きのうの てんきは とても {{0}}。", new[] { new ClBlank(new[] { "よかったです", "良かったです" }, "đã tốt (yokatta desu)", "いい chuyển về gốc よ trước khi thêm かったです.") }, "きのうの てんきは とても よかったです。", "kinou no tenki wa totemo yokatta desu", "Thời tiết hôm qua đã rất đẹp.", "Không bao giờ chia là いかったです."),
                new ClItem("ja-l41-cl-4", "hard", "Nối 2 tính từ đuôi -i và -na", "Điền dạng nối của やすい (rẻ) và きれい (đẹp):", "この みせは やす{{0}} おいしいです。へやも きれい{{1}} しずかです。", new[] { new ClBlank(new[] { "くて" }, "nối tính từ -i (yasukute)", "Tính từ đuôi -i bỏ い thêm くて."), new ClBlank(new[] { "で" }, "nối tính từ -na (kirei de)", "Tính từ đuôi -na thêm で.") }, "この みせは やすくて おいしいです。へやも きれいで しずかです。", "kono mise wa yasukute oishii desu. heya mo kirei de shizuka desu", "Quán này vừa rẻ vừa ngon. Phòng cũng đẹp và yên tĩnh.", "Phân biệt chuẩn nối -i (くて) và nối -na (で).")
            },
            translate: new[]
            {
                new TrItem("ja-l41-tr-1", "easy", "Miêu tả thời tiết", "Hôm nay trời rất nóng.", new[] { "きょう", "とても", "あつい" }, new[] { "きょうはとてもあついです。", "今日はとても暑いです。", "きょうは とても あついです。" }, "今日はとても暑いです。", "kyou wa totemo atsui desu", "Tính từ chỉ thời tiết nóng dùng あつい."),
                new TrItem("ja-l41-tr-2", "easy", "Ngoại lệ きれい", "Hoa anh đào rất đẹp.", new[] { "さくら", "きれい" }, new[] { "さくらはとてもきれいです。", "さくらはきれいです。", "桜はとても綺麗です。", "桜は綺麗です。" }, "桜はとても綺麗です。", "sakura wa totemo kirei desu", "きれい là tính từ đuôi -na dù kết thúc bằng chữ い."),
                new TrItem("ja-l41-tr-3", "medium", "Quá khứ phủ định -i", "Bài kiểm tra hôm qua không khó lắm.", new[] { "きのう", "テスト", "あまり", "むずかしくなかった" }, new[] { "きのうのテストはあまりむずかしくなかったです。", "昨日のテストはあまり難しくなかったです。", "きのうの テストは あまり むずかしくなかったです。" }, "昨日のテストはあまり難しくなかったです。", "kinou no tesuto wa amari muzukashikunakatta desu", "Quá khứ phủ định đuôi -i: bỏ い + くなかったです."),
                new TrItem("ja-l41-tr-4", "hard", "Nối 2 tính từ", "Thành phố này vừa yên tĩnh vừa tiện lợi.", new[] { "このまち", "しずかで", "べんり" }, new[] { "このまちはしずかでべんりです。", "この街は静かで便利です。", "この町は静かで便利です。" }, "この街は静かで便利です。", "kono machi wa shizuka de benri desu", "Nối hai tính từ đuôi -na bằng trợ từ で.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l41-gf-1", "easy", "Nhầm でした với tính từ đuôi -i", "きのうはとてもさむいでした。", "Tính từ đuôi -i chia quá khứ bằng cách bỏ い thêm かったです (không dùng でした).", "きのうはとてもさむかったです。", new[] { "きのうはとてもさむかったです。", "昨日はとても寒かったです。" }, "kinou wa totemo samukatta desu", "Hôm qua trời đã rất lạnh.", "Chỉ danh từ và tính từ đuôi -na mới dùng でした."),
                new GfItem("ja-l41-gf-2", "medium", "Chia nhầm いい sang いかった", "きのうのパーティーはとてもいかったです。", "Tính từ いい phải đổi thành よ trước khi chia quá khứ よかった.", "きのうのパーティーはとてもよかったです。", new[] { "きのうのパーティーはとてもよかったです。", "昨日のパーティーはとても良かったです。" }, "kinou no paatii wa totemo yokatta desu", "Bữa tiệc hôm qua rất tuyệt.", "いい → よくない / よかった / よくなかった."),
                new GfItem("ja-l41-gf-3", "hard", "Dùng あまり với câu khẳng định", "このへやはあまりしずかです。", "Phó từ あまり bắt buộc phải đi kèm thể phủ định (ではありません / じゃありません).", "このへやはあまりしずかではありません。", new[] { "このへやはあまりしずかではありません。", "このへやはあまりしずかじゃありません。", "この部屋はあまり静かではありません。" }, "kono heya wa amari shizuka dewa arimasen", "Căn phòng này không yên tĩnh lắm.", "あまり...ない = không ... lắm.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l41-rp-1", "easy", "Hỏi thăm thời tiết hôm nay", "Bạn người Nhật hỏi cảm nhận của bạn về thời tiết hôm nay.", "Bạn Kenji", "きょうのてんきはどうですか。あついですか。", "kyou no tenki wa dou desu ka? atsui desu ka?", "Thời tiết hôm nay thế nào? Có nóng không?", "Trả lời thời tiết hôm nay bằng tính từ (あつい/さむい/すずしい/いい) kèm phó từ とても hoặc あまり〜ない.", new[] { "あつい|さむい|すずしい|あたたかい|いい", "とても|すこし|あまり|ぜんぜん" }, new[] { "きょうはすずしくて、とてもいい天気ですね。" }, "kyou wa suzushikute, totemo ii tenki desu ne.", 6),
                new RpItem("ja-l41-rp-2", "medium", "Nhận xét về thành phố quê hương", "Đồng nghiệp Nhật hỏi Hà Nội/TP.HCM là thành phố như thế nào.", "Anh Sato", "ベトナムのまちはどんなまちですか。", "Betonamu no machi wa donna machi desu ka?", "Thành phố ở Việt Nam là thành phố như thế nào?", "Dùng cấu trúc nối tính từ (〜くて / 〜で) để miêu tả thành phố (にぎやか, きれい, あつい, おいしい).", new[] { "にぎやか|きれい|べんり|あつい", "で|くて", "まち|ところ" }, new[] { "ハノイはきれいで、とてもにぎやかなまちです。" }, "Hanoi wa kirei de, totemo nigiyaka na machi desu.", 8),
                new RpItem("ja-l41-rp-3", "hard", "Review bài kiểm tra hôm qua", "Cô giáo hỏi bài kiểm tra hôm qua của bạn có khó không và kết quả thế nào.", "Cô Tanaka", "きのうのテストはどうでしたか。むずかしかったですか。", "kinou no tesuto wa dou deshita ka? muzukashikatta desu ka?", "Bài kiểm tra hôm qua thế nào? Có khó không?", "Trả lời bằng thì quá khứ tính từ (むずかしかった / むずかしくなかった / よかったです).", new[] { "なかった|かった|よかったです", "むずかしく|やさしく|あまり|ぜんぜん" }, new[] { "あまりむずかしくなかったです。とてもよかったです！" }, "amari muzukashikunakatta desu. totemo yokatta desu!", 8)
            }, now));

        // ---------------- LESSON 4.2 ----------------
        all.AddRange(BuildLessonPack(cId, JapaneseLesson42Id, "ja-JP", "Nhật N5 · Bài 4.2",
            shadowing: new[]
            {
                new ShItem("ja-l42-sh-1", "easy", "Gia đình 4 người (Dễ)", "わたしの かぞくは よにんです。", "わたし の かぞく は よにん です / watashi no kazoku wa yonin desu", "Gia đình tôi có 4 người.", new[] { "かぞく", "よにん" }),
                new ShItem("ja-l42-sh-2", "easy", "Ngoại lệ ひとり・ふたり (Dễ)", "きょうだいは ふたり います。", "きょうだい は ふたり います / kyoudai wa futari imasu", "Tôi có 2 anh chị em.", new[] { "きょうだい", "ふたり" }),
                new ShItem("ja-l42-sh-3", "easy", "Nghề nghiệp bố (Dễ)", "ちちは かいしゃいんです。", "ちち は かいしゃいん です / chichi wa kaishain desu", "Bố tôi là nhân viên công ty.", new[] { "ちち", "かいしゃいん" }),
                new ShItem("ja-l42-sh-4", "medium", "Mua vé xem phim (Vừa)", "えいがの きっぷを にまい かいました。", "えいが の きっぷ を にまい かいました / eiga no kippu o nimai kaimashita", "Tôi đã mua 2 tấm vé xem phim.", new[] { "きっぷ", "にまい" }),
                new ShItem("ja-l42-sh-5", "medium", "Biến âm lượng từ 本 (Vừa)", "あかい ペンを さんぼん ください。", "あかい ペン を さんぼん ください / akai pen o sanbon kudasai", "Cho tôi 3 cây bút màu đỏ.", new[] { "さんぼん", "ください" }),
                new ShItem("ja-l42-sh-6", "medium", "Nêu lý do với から (Vừa)", "じかんが ありませんから、タクシーで いきます。", "じかん が ありません から、タクシー で いきます / jikan ga arimasen kara, takushii de ikimasu", "Vì không có thời gian nên tôi đi taxi.", new[] { "ありませんから", "タクシーで" }),
                new ShItem("ja-l42-sh-7", "hard", "Hỏi gia đình người khác (Khó)", "おとうさんと おかあさんは おげんきですか。", "おとうさん と おかあさん は おげんき です か / otousan to okaasan wa ogenki desu ka", "Bố và mẹ của bạn có khỏe không?", new[] { "おとうさん", "おかあさん", "おげんき" }),
                new ShItem("ja-l42-sh-8", "hard", "Lý do với tính từ + から (Khó)", "きょうは さむいですから、あたたかい おちゃを のみます。", "きょう は さむい です から、あたたかい おちゃ を のみます / kyou wa samui desu kara, atatakai ocha o nomimasu", "Vì hôm nay trời lạnh nên tôi uống trà nóng.", new[] { "さむいですから", "あたたかい" }),
                new ShItem("ja-l42-sh-9", "hard", "Trả lời câu hỏi どうして (Khó)", "あした テストが ありますから、うちで べんきょうします。", "あした テスト が あります から、うち で べんきょうします / ashita tesuto ga arimasu kara, uchi de benkyou shimasu", "Vì ngày mai có bài kiểm tra nên tôi học ở nhà.", new[] { "ありますから", "べんきょうします" })
            },
            dictation: new[]
            {
                new DiItem("ja-l42-di-1", "easy", "Gia đình 4 người", "かぞくはよにんです", "kazoku wa yonin desu", "Gia đình có 4 người.", new[] { "かぞくは", "よにん", "です" }, new[] { "しにん", "ふたり", "います" }),
                new DiItem("ja-l42-di-2", "easy", "Một mình đi Nhật", "ひとりでにほんへいきます", "hitori de Nihon e ikimasu", "Tôi đi Nhật một mình.", new[] { "ひとりで", "にほんへ", "いきます" }, new[] { "いちにん", "ふたり", "きます" }),
                new DiItem("ja-l42-di-3", "medium", "Mua 3 cái ô", "かさをさんぼんかいました", "kasa o sanbon kaimashita", "Tôi đã mua 3 cái ô.", new[] { "かさを", "さんぼん", "かいました" }, new[] { "さんほん", "にまい", "います" }),
                new DiItem("ja-l42-di-4", "medium", "Có 2 chiếc xe", "くるまがにだいあります", "kuruma ga nidai arimasu", "Có 2 chiếc xe ô tô.", new[] { "くるまが", "にだい", "あります" }, new[] { "います", "にまい", "ふたつ" }),
                new DiItem("ja-l42-di-5", "hard", "Vì mệt nên nghỉ ngơi", "つかれましたからやすみます", "tsukaremashita kara yasumimasu", "Vì mệt rồi nên tôi nghỉ ngơi.", new[] { "つかれました", "から", "やすみます" }, new[] { "しかし", "でも", "あした" }),
                new DiItem("ja-l42-di-6", "hard", "Vì bận nên không thể đi", "ようじがありますからいきません", "youji ga arimasu kara ikimasen", "Vì có việc bận nên tôi không đi.", new[] { "ようじが", "あります", "から", "いきません" }, new[] { "います", "けれど", "いきます" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l42-pr-1", "easy", "Ngoại lệ ひとり・ふたり", "ふたりでいきます", "futari de ikimasu", "Hai người cùng đi.", new[] { "ふたり", "いきます" }, "Thổi nhẹ môi ở ふ và chạm nhẹ lưỡi ở り.", new[] { "ふ không đọc thành phu tiếng Việt", "り nhẹ thanh", "Đọc で mượt mà" }, "ともだちとふたりでいきます。", new[] { "ふたり", "いきます" }),
                new PrItem("ja-l42-pr-2", "easy", "Xưng hô おとうさん・おかあさん", "おかあさんはやさしいです", "okaasan wa yasashii desu", "Mẹ rất hiền.", new[] { "おかあさん", "やさしい" }, "Kéo dài trường âm かあ đủ 2 phách để tránh nhầm âm.", new[] { "お nhẹ", "かあ kéo dài 2 nhịp", "し đọc êm" }, "わたしのおかあさんはとてもやさしいです。", new[] { "おかあさん", "やさしい" }),
                new PrItem("ja-l42-pr-3", "medium", "Biến âm いっぽん・さんぼん", "ペンがいっぽんあります", "pen ga ippon arimasu", "Có một cây bút.", new[] { "ペン", "いっぽん" }, "Giữ âm ngắt っ rồi bật hơi môi dứt khoát ở ぽ.", new[] { "Nén hơi っ trước ぽ", "Bật nhẹ âm p của ぽ", "ん mũi ngân nhẹ" }, "つくえのうえにペンがいっぽんあります。", new[] { "いっぽん", "あります" }),
                new PrItem("ja-l42-pr-4", "medium", "Âm ngắt きっぷ", "きっぷをにまいかいました", "kippu o nimai kaimashita", "Tôi đã mua 2 tấm vé.", new[] { "きっぷ", "にまい" }, "Ngắt 1 nhịp ở っ trong きっぷ và đọc rõ にまい.", new[] { "Dừng 1 nhịp ở っ", "Bật nhẹ ぷ", "Đọc đuôi quá khứ ました" }, "えきできっぷをにまいかいました。", new[] { "きっぷ", "にまい" }),
                new PrItem("ja-l42-pr-5", "hard", "Từ vựng りょうしん・きょうだい", "りょうしんはベトナムにいます", "ryoushin wa Betonamu ni imasu", "Bố mẹ tôi đang ở Việt Nam.", new[] { "りょうしん", "ベトナム" }, "Đọc gọn âm ghép りょう kéo dài 2 phách.", new[] { "りょう ghép mượt không tách ri-ou", "しん âm mũi rõ", "ベトナム bật rõ vần" }, "りょうしんはいまベトナムにいます。", new[] { "りょうしん", "います" }),
                new PrItem("ja-l42-pr-6", "hard", "Câu lý do どうして〜から", "きょうはいそがしいですからかえります", "kyou wa isogashii desu kara kaerimasu", "Vì hôm nay bận nên tôi về.", new[] { "いそがしい", "から", "かえります" }, "Ngắt nhẹ sau から trước khi đọc mệnh đề kết quả.", new[] { "きょう trường âm 2 nhịp", "いそがしい kéo dài しい", "Ngắt nhẹ sau から" }, "じかんがありませんから、いそぎます。", new[] { "ありませんから", "いそぎます" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l42-wm-1", "easy", "Sơ đồ từ vựng: かぞく (Gia đình)", "かぞく (家族)", "かぞく (kazoku)", "Gia đình",
                    new[] { new WmNode("ごかぞく", "gokazoku", "Gia đình (của bạn - kính ngữ)", "ごかぞくは なんにんですか。"), new WmNode("かぞくと すんでいます", "kazoku to sunde imasu", "Sống cùng gia đình", "いま かぞくと すんでいます。") },
                    new[] { new WmNode("りょうしん / きょうだい", "ryoushin / kyoudai", "Bố mẹ / Anh chị em", "きょうだいは さんにんいます。") },
                    new WmNode[] { new WmNode("ひとりぐらし", "hitorigurashi", "Sống một mình", "とうきょうで ひとりぐらしをしています。") },
                    new[] { new WmEx("わたしの かぞくは ちちと ははと わたしの さんにんです。", "watashi no kazoku wa chichi to haha to watashi no sannin desu", "Gia đình tôi có 3 người: bố, mẹ và tôi.") }),
                new WmItem("ja-l42-wm-2", "medium", "Sơ đồ từ vựng: なんにん (Bao nhiêu người)", "〜にん (〜人)", "にん / ひとり / ふたり", "Đếm số người",
                    new[] { new WmNode("ひとり / ふたり", "hitori / futari", "1 người / 2 người", "ふたりで ランチを たべます。"), new WmNode("さんにん / よにん", "sannin / yonin", "3 người / 4 người", "よにんの ともだちが きました。") },
                    new[] { new WmNode("ひとりで", "hitori de", "Một mình", "ひとりで りょこうします。") },
                    new WmNode[] { new WmNode("なんにん", "nannin", "Mấy người?", "全部で なんにんですか。") },
                    new[] { new WmEx("この クラスには がくせいが じゅうににん います。", "kono kurasu ni wa gakusei ga juuninin imasu", "Lớp học này có 12 học viên.") }),
                new WmItem("ja-l42-wm-3", "hard", "Sơ đồ từ vựng: 〜から (Vì... nên...)", "〜から", "~kara", "Vì ... nên ... (Nguyên nhân)",
                    new[] { new WmNode("いそがしいですから", "isogashii desu kara", "Vì bận nên...", "いそがしいですから、いきません。"), new WmNode("じかんがありますから", "jikan ga arimasu kara", "Vì có thời gian nên...", "じかんがありますから、あるきましょう。") },
                    new[] { new WmNode("どうしてですか", "doushite desu ka", "Tại sao vậy?", "どうして おくれましたか。") },
                    new WmNode[] { new WmNode("だから", "dakara", "Vì vậy nên (đầu câu)", "あめです。だから、いえに います。") },
                    new[] { new WmEx("きょうは あついですから、つめたい みずを のみましょう。", "kyou wa atsui desu kara, tsumetai mizu o nomimashou", "Vì hôm nay nóng nên chúng ta uống nước lạnh nhé.")  })
            },
            cloze: new[]
            {
                new ClItem("ja-l42-cl-1", "easy", "Đếm 4 người", "Điền từ đếm cho '4 người' (tránh âm し):", "わたしの かぞくは {{0}} です。", new[] { new ClBlank(new[] { "よにん", "四人" }, "4 người (yonin)", "4 người đọc là よにん, không đọc しにん.") }, "わたしの かぞくは よにん です。", "watashi no kazoku wa yonin desu", "Gia đình tôi có 4 người.", "1 người = ひとり, 2 người = ふたり, 4 người = よにん."),
                new ClItem("ja-l42-cl-2", "easy", "Xưng hô gia đình mình", "Điền từ chỉ 'bố tôi' khi nói với người ngoài:", "わたしの {{0}} は エンジニアです。", new[] { new ClBlank(new[] { "ちち", "父" }, "bố tôi (chichi)", "Nói về bố mình với người ngoài dùng ちち.") }, "わたしの ちち は エンジニアです。", "watashi no chichi wa enjinia desu", "Bố tôi là kỹ sư.", "Bố người khác mới gọi là おとうさん."),
                new ClItem("ja-l42-cl-3", "medium", "Lượng từ vé/giấy & bút", "Điền lượng từ đúng cho 2 tấm vé và 3 cây bút:", "きっぷを {{0}} と ペンを {{1}} かいました。", new[] { new ClBlank(new[] { "にまい", "2まい", "二枚" }, "2 tấm (nimai)", "Vật mỏng phẳng (vé, giấy) dùng 〜まい."), new ClBlank(new[] { "さんぼん", "3ぼん", "三本" }, "3 cây (sanbon)", "3 vật thon dài biến âm thành さんぼん.") }, "きっぷを にまいと ペンを さんぼん かいました。", "kippu o nimai to pen o sanbon kaimashita", "Tôi đã mua 2 tấm vé và 3 cây bút.", "Chú ý biến âm của ほん: 1=いっぽん, 3=さんぼん, 6=ろっぽん."),
                new ClItem("ja-l42-cl-4", "hard", "Liên từ nguyên nhân から", "Điền liên từ 'vì... nên' vào câu:", "あした テストが あります{{0}}、きょうは はやく ねます。", new[] { new ClBlank(new[] { "から" }, "vì ... nên (kara)", "Mệnh đề nguyên nhân + から, Mệnh đề kết quả.") }, "あした テストが ありますから、きょうは はやく ねます。", "ashita tesuto ga arimasu kara, kyou wa hayaku nemasu", "Vì ngày mai có bài kiểm tra nên hôm nay tôi đi ngủ sớm.", "Vế nguyên nhân luôn đặt trước から.")
            },
            translate: new[]
            {
                new TrItem("ja-l42-tr-1", "easy", "Gia đình 4 người", "Gia đình tôi có 4 người.", new[] { "かぞく", "よにん" }, new[] { "わたしの かぞくは よにんです。", "わたしのかぞくはよにんです。", "私の家族は四人です。" }, "私の家族は四人です。", "watashi no kazoku wa yonin desu", "4 người đọc là よにん."),
                new TrItem("ja-l42-tr-2", "easy", "Mua 2 tấm ảnh/vé", "Cho tôi 2 tấm vé.", new[] { "きっぷ", "にまい", "ください" }, new[] { "きっぷをにまいください。", "切符を二枚ください。", "きっぷを にまい ください。" }, "切符を二枚ください。", "kippu o nimai kudasai", "Số lượng từ にまい đặt sau を và trước ください."),
                new TrItem("ja-l42-tr-3", "medium", "Vì trời lạnh nên đóng cửa", "Vì trời lạnh nên tôi đóng cửa sổ.", new[] { "さむい", "から", "まど", "しめます" }, new[] { "さむいですから、まどをしめます。", "寒いから、窓を閉めます。", "寒いですから、窓を閉めます。" }, "寒いですから、窓を閉めます。", "samui desu kara, mado o shimemasu", "Đặt nguyên nhân + から lên đầu câu."),
                new TrItem("ja-l42-tr-4", "hard", "Hỏi về gia đình bạn", "Anh trai của bạn bao nhiêu tuổi rồi?", new[] { "おにいさん", "なんさい" }, new[] { "おにいさんはなんさいですか。", "お兄さんは何歳ですか。", "お兄さんはおいくつですか。" }, "お兄さんは何歳ですか。", "oniisan wa nansai desu ka", "Anh trai của người khác phải gọi là おにいさん.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l42-gf-1", "easy", "Sai vị trí trợ từ を với số lượng", "りんごを ふたつを かいました。", "Không thêm trợ từ を sau số lượng từ ふたつ.", "りんごを ふたつ かいました。", new[] { "りんごを ふたつ かいました。", "りんごをふたつ買いました。" }, "ringo o futatsu kaimashita", "Tôi đã mua 2 quả táo.", "Số lượng từ đứng trực tiếp trước động từ, không kèm trợ từ."),
                new GfItem("ja-l42-gf-2", "medium", "Đọc sai 4 người thành しにん", "きょうしつに がくせいが しにん います。", "4 người phải đọc là よにん.", "きょうしつに がくせいが よにん います。", new[] { "きょうしつに がくせいが よにん います。", "教室に学生が四人います。" }, "kyoushitsu ni gakusei ga yonin imasu", "Trong lớp học có 4 học sinh.", "よにん tránh đồng âm với chữ Tử (し)."),
                new GfItem("ja-l42-gf-3", "hard", "Đảo ngược kết quả trước nguyên nhân", "きょうは やすみます、かぜを ひきましたから。", "Trật tự chuẩn tiếng Nhật đặt Nguyên nhân + から ở trước Kết quả.", "かぜを ひきましたから、きょうは やすみます。", new[] { "かぜを ひきましたから、きょうは やすみます。", "風邪を引きましたから、今日は休みます。" }, "kaze o hikimashita kara, kyou wa yasumimasu", "Vì bị cảm nên hôm nay tôi xin nghỉ.", "Nguyên nhân + から → Kết quả.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l42-rp-1", "easy", "Kể về số thành viên gia đình", "Cô giáo hỏi nhà bạn có mấy người và có anh chị em không.", "Cô Tanaka", "ミンさんの ごかぞくは なんにんですか。", "Min-san no gokazoku wa nannin desu ka?", "Gia đình bạn Minh có mấy người thế?", "Trả lời số người (よにん/さんにん/ごにん...) và kể thành viên bằng từ khiêm nhường (ちち/はは/あに/いもうと).", new[] { "にん|ひとり|ふたり", "ちち|はは|あに|あね|おとうと|いもうと" }, new[] { "わたしの かぞくは よにんです。ちちと ははと いもうとと わたしです。" }, "watashi no kazoku wa yonin desu. chichi to haha to imouto to watashi desu.", 8),
                new RpItem("ja-l42-rp-2", "medium", "Mua đồ văn phòng phẩm", "Nhân viên cửa hàng hỏi bạn muốn mua gì và số lượng bao nhiêu.", "Nhân viên cửa hàng", "いらっしゃいませ。なにを おさがしですか。", "irasshaimase. nani o osagashi desu ka?", "Xin chào quý khách. Quý khách muốn mua gì ạ?", "Gọi mua bút/vở/ô kèm lượng từ đúng (いっぽん/にほん/さんぼん/いちまい/にまい) và ください.", new[] { "ペン|ノート|かさ|きっぷ", "ほん|ぼん|ぽん|まい|さつ|つ", "ください" }, new[] { "あかい ペンを さんぼんと ノートを にさつ ください。" }, "akai pen o sanbon to nooto o nisatsu kudasai.", 8),
                new RpItem("ja-l42-rp-3", "hard", "Xin phép về sớm vì có việc bận", "Bạn nói lý do với sếp tại sao hôm nay bạn cần về sớm.", "Anh Sato", "あれ、ミンさん、きょうは もう かえりますか。どうしてですか。", "are, Min-san, kyou wa mou kaerimasu ka? doushite desu ka?", "Ủa Minh, hôm nay em về sớm à? Sao thế?", "Giải thích lý do bằng mẫu câu có 〜から (ví dụ ようじがありますから / びょういんへ いきますから).", new[] { "から", "ようじ|びょういん|あたま|じかん|ともだち", "すみません|おさきに" }, new[] { "すみません、きょうは ようじが ありますから、はやく かえります。" }, "sumimasen, kyou wa youji ga arimasu kara, hayaku kaerimasu.", 8)
            }, now));

        // ---------------- LESSON 4.3 ----------------
        all.AddRange(BuildLessonPack(cId, JapaneseLesson43Id, "ja-JP", "Nhật N5 · Bài 4.3",
            shadowing: new[]
            {
                new ShItem("ja-l43-sh-1", "easy", "Tồn tại đồ vật (Dễ)", "つくえの うえに ほんが あります。", "つくえ の うえ に ほん が あります / tsukue no ue ni hon ga arimasu", "Trên bàn có quyển sách.", new[] { "うえに", "あります" }),
                new ShItem("ja-l43-sh-2", "easy", "Tồn tại động vật (Dễ)", "いすの したに ねこが います。", "いす の した に ねこ が います / isu no shita ni neko ga imasu", "Dưới ghế có con mèo.", new[] { "したに", "います" }),
                new ShItem("ja-l43-sh-3", "easy", "Vị trí bưu điện (Dễ)", "こうばんは えきの まえに あります。", "こうばん は えき の まえ に あります / kouban wa eki no mae ni arimasu", "Đồn cảnh sát ở trước nhà ga.", new[] { "こうばん", "まえに" }),
                new ShItem("ja-l43-sh-4", "medium", "Vị trí kẹp giữa あいだ (Vừa)", "ほんやは ぎんこうと カフェの あいだに あります。", "ほんや は ぎんこう と カフェ の あいだ に あります / honya wa ginkou to kafe no aida ni arimasu", "Hiệu sách nằm ở giữa ngân hàng và quán cà phê.", new[] { "と", "のあいだに" }),
                new ShItem("ja-l43-sh-5", "medium", "Liệt kê với や〜など (Vừa)", "かばんの なかに さいふや かぎなどが あります。", "かばん の なか に さいふ や かぎ など が あります / kaban no naka ni saifu ya kagi nado ga arimasu", "Trong túi có ví, chìa khóa, v.v...", new[] { "や", "など" }),
                new ShItem("ja-l43-sh-6", "medium", "Phủ định hoàn toàn なにも (Vừa)", "はこの なかに なにも ありません。", "はこ の なか に なにも ありません / hako no naka ni nanimo arimasen", "Trong hộp không có gì cả.", new[] { "なにも", "ありません" }),
                new ShItem("ja-l43-sh-7", "hard", "Phủ định người だれも (Khó)", "きょうしつに だれも いませんでした。", "きょうしつ に だれも いません でした / kyoushitsu ni daremo imasen deshita", "Trong lớp học đã không có ai cả.", new[] { "だれも", "いませんでした" }),
                new ShItem("ja-l43-sh-8", "hard", "Miêu tả gần nhà (Khó)", "うちの ちかくに おおきい こうえんが あります。", "うち の ちかく に おおきい こうえん が あります / uchi no chikaku ni ookii kouen ga arimasu", "Ở gần nhà tôi có một công viên lớn.", new[] { "ちかくに", "こうえん" }),
                new ShItem("ja-l43-sh-9", "hard", "Hỏi vị trí lịch sự (Khó)", "すみません、エレベーターは どこに ありますか。", "すみません、エレベーター は どこ に あります か / sumimasen, erebeetaa wa doko ni arimasu ka", "Xin lỗi, thang máy nằm ở đâu vậy ạ?", new[] { "エレベーター", "どこに" })
            },
            dictation: new[]
            {
                new DiItem("ja-l43-di-1", "easy", "Trong tủ lạnh có sữa", "れいぞうこのなかにぎゅうにゅうがあります", "reizouko no naka ni gyuunyuu ga arimasu", "Trong tủ lạnh có sữa tươi.", new[] { "れいぞうこの", "なかに", "ぎゅうにゅうが", "あります" }, new[] { "います", "そと", "で" }),
                new DiItem("ja-l43-di-2", "easy", "Trong phòng có mèo", "へやのなかにねこがいます", "heya no naka ni neko ga imasu", "Trong phòng có con mèo.", new[] { "へやの", "なかに", "ねこが", "います" }, new[] { "あります", "うえ", "で" }),
                new DiItem("ja-l43-di-3", "medium", "Nằm giữa A và B", "ゆうびんきょくはえきとびょういんのあいだにあります", "yuubinkyoku wa eki to byouin no aida ni arimasu", "Bưu điện nằm giữa nhà ga và bệnh viện.", new[] { "ゆうびんきょくは", "えきと", "びょういんの", "あいだにあります" }, new[] { "となりに", "まえに", "います" }),
                new DiItem("ja-l43-di-4", "medium", "Liệt kê không đầy đủ", "つくえのうえにほんやノートなどがあります", "tsukue no ue ni hon ya nooto nado ga arimasu", "Trên bàn có sách, vở, v.v...", new[] { "つくえの", "うえに", "ほんや", "ノートなどがあります" }, new[] { "と", "います", "したに" }),
                new DiItem("ja-l43-di-5", "hard", "Không có ai trong phòng", "じむしょにだれもいません", "jimusho ni daremo imasen", "Trong văn phòng không có ai cả.", new[] { "じむしょに", "だれも", "いません" }, new[] { "なにも", "あります", "だれが" }),
                new DiItem("ja-l43-di-6", "hard", "Bên cạnh hiệu sách", "デパートのとなりにほんやがあります", "depaato no tonari ni honya ga arimasu", "Bên cạnh trung tâm thương mại có hiệu sách.", new[] { "デパートの", "となりに", "ほんやが", "あります" }, new[] { "あいだ", "います", "で" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l43-pr-1", "easy", "Từ vựng こうばん・こうえん", "えきのまえにこうばんがあります", "eki no mae ni kouban ga arimasu", "Trước ga có đồn cảnh sát.", new[] { "えき", "こうばん" }, "Kéo dài trường âm こう thành kō và giữ rõ âm mũi ん.", new[] { "こう kéo dài 2 nhịp", "ばん đọc gọn có ん", "ります chạm lưỡi nhẹ" }, "えきのまえにこうばんがあります。", new[] { "まえ", "こうばん" }),
                new PrItem("ja-l43-pr-2", "easy", "Trường âm びょういん", "びょういんはあそこにあります", "byouin wa asoko ni arimasu", "Bệnh viện ở đằng kia.", new[] { "びょういん", "あそこ" }, "Đọc mượt âm ghép trường âm びょう (byō) và tách nhẹ いん.", new[] { "びょう kéo dài 2 phách", "いん đọc ngắn rõ", "あそこ đều nhịp" }, "びょういんはぎんこうのとなりにあります。", new[] { "びょういん", "となり" }),
                new PrItem("ja-l43-pr-3", "medium", "Tổ hợp れいぞうこ", "れいぞうこのなかにあります", "reizouko no naka ni arimasu", "Ở bên trong tủ lạnh.", new[] { "れいぞうこ", "なか" }, "Kéo dài cả れい (rē) và ぞう (zō) trong cùng một từ.", new[] { "れい kéo dài 2 nhịp", "ぞう kéo dài 2 nhịp", "こ ngắn gọn" }, "れいぞうこのなかにりんごがあります。", new[] { "れいぞうこ", "あります" }),
                new PrItem("ja-l43-pr-4", "medium", "Cấu trúc あいだ", "えきとホテルのあいだです", "eki to hoteru no aida desu", "Ở giữa nhà ga và khách sạn.", new[] { "ホテル", "あいだ" }, "Đọc rõ từng âm tiết a-i-da (không nuốt chữ i).", new[] { "ホ bật hơi nhẹ", "あ-い-だ rõ 3 phách", "です khép nhẹ" }, "ほんやはえきとホテルのあいだにあります。", new[] { "あいだ", "あります" }),
                new PrItem("ja-l43-pr-5", "hard", "Từ vựng エレベーター", "エレベーターはみぎにあります", "erebeetaa wa migi ni arimasu", "Thang máy ở bên phải.", new[] { "エレベーター", "みぎ" }, "Kéo dài cả hai dấu trường âm ベー và ター.", new[] { "エ-レ ngắn", "ベー kéo dài", "ター kéo dài" }, "すみません、エレベーターはどこにありますか。", new[] { "エレベーター", "どこ" }),
                new PrItem("ja-l43-pr-6", "hard", "Phủ định なにも・だれも", "きょうしつにだれもいません", "kyoushitsu ni daremo imasen", "Trong lớp không có ai cả.", new[] { "きょうしつ", "だれも", "いません" }, "Kéo dài きょう và nhấn nhẹ chữ も trong だれも.", new[] { "きょう kéo dài 2 nhịp", "しつ bật nhẹ đầu lưỡi", "だれも xuống giọng ở も" }, "なかにだれもいませんし、なにもありません。", new[] { "だれも", "なにも" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l43-wm-1", "easy", "Sơ đồ từ vựng: うえ / した / なか (Vị trí)", "うえ・した・なか", "ue / shita / naka", "Trên / Dưới / Trong",
                    new[] { new WmNode("つくえの うえ", "tsukue no ue", "Trên bàn", "つくえの うえに パソコンが あります。"), new WmNode("はこの なか", "hako no naka", "Trong hộp", "はこの なかに なにも ありません。") },
                    new[] { new WmNode("そと / まえ / うしろ", "soto / mae / ushiro", "Ngoài / Trước / Sau", "いえの まえに いぬが います。") },
                    new WmNode[] { new WmNode("となり / あいだ", "tonari / aida", "Bên cạnh / Ở giữa", "ぎんこうと ほんやの あいだ") },
                    new[] { new WmEx("いすの したに おちた ペンが あります。", "isu no shita ni ochita pen ga arimasu", "Dưới ghế có cây bút bị rơi.") }),
                new WmItem("ja-l43-wm-2", "medium", "Sơ đồ từ vựng: びょういん / こうばん (Công cộng)", "こうきょうしせつ", "byouin / kouban / honya", "Công trình công cộng",
                    new[] { new WmNode("こうばん", "kouban", "Đồn cảnh sát khu phố", "えきの まえに こうばんが あります。"), new WmNode("びょういん", "byouin", "Bệnh viện", "びょういんへ いきます。") },
                    new[] { new WmNode("デパート / スーパー", "depaato / suupaa", "TTTM / Siêu thị", "デパートの なかに レストランが あります。") },
                    new WmNode[] { new WmNode("ほんや / はなや", "honya / hanaya", "Hiệu sách / Tiệm hoa", "ほんやで じしょを かいました。") },
                    new[] { new WmEx("びょういんは こうえんの ちかくに あります。", "byouin wa kouen no chikaku ni arimasu", "Bệnh viện nằm ở gần công viên.") }),
                new WmItem("ja-l43-wm-3", "hard", "Sơ đồ từ vựng: や〜など (Liệt kê)", "〜や 〜など", "~ya ~nado", "... và ..., vân vân",
                    new[] { new WmNode("ほんや ノートなど", "hon ya nooto nado", "Sách, vở, vân vân", "つくえに ほんや ノートなどが あります。"), new WmNode("パンや たまご", "pan ya tamago", "Bánh mì, trứng, v.v...", "スーパーで パンや たまごを かいました。") },
                    new[] { new WmNode("なにも / だれも", "nanimo / daremo", "Không gì / Không ai", "なにも ありません。") },
                    new WmNode[] { new WmNode("〜と〜", "~to~", "Và (liệt kê hết)", "さいふと かぎが あります。") },
                    new[] { new WmEx("れいぞうこの なかに りんごや ぎゅうにゅうなどが あります。", "reizouko no naka ni ringo ya gyuunyuu nado ga arimasu", "Trong tủ lạnh có táo, sữa tươi, v.v...") })
            },
            cloze: new[]
            {
                new ClItem("ja-l43-cl-1", "easy", "Trợ từ vị trí tồn tại に", "Điền trợ từ kết nối địa điểm và động từ あります:", "つくえの うえ{{0}} かばんが {{1}}。", new[] { new ClBlank(new[] { "に" }, "tại (vị trí tồn tại)", "Nơi tồn tại luôn dùng trợ từ に."), new ClBlank(new[] { "あります" }, "có (vật vô tri)", "Đồ vật dùng あります.") }, "つくえの うえに かばんが あります。", "tsukue no ue ni kaban ga arimasu", "Trên bàn có cái cặp.", "Khác với hành động (で), tồn tại luôn dùng に."),
                new ClItem("ja-l43-cl-2", "easy", "Chọn います cho mèo", "Điền động từ tồn tại cho 'chú mèo':", "ベッドの したに ねこが {{0}}。", new[] { new ClBlank(new[] { "います" }, "có (động vật)", "Người và động vật luôn dùng います.") }, "ベッドの したに ねこが います。", "beddo no shita ni neko ga imasu", "Dưới gầm giường có một chú mèo.", "Vật dùng あります; người/động vật dùng います."),
                new ClItem("ja-l43-cl-3", "medium", "Vị trí kẹp giữa A と B の あいだ", "Hoàn thiện câu 'Ở giữa ngân hàng và bưu điện':", "ほんやは ぎんこう{{0}} ゆうびんきょくの {{1}}に あります。", new[] { new ClBlank(new[] { "と" }, "nối 2 mốc A và B", "A と B の あいだ = giữa A và B."), new ClBlank(new[] { "あいだ", "間" }, "ở giữa (aida)", "あいだ = khoảng giữa.") }, "ほんやは ぎんこうと ゆうびんきょくの あいだに あります。", "honya wa ginkou to yuubinkyoku no aida ni arimasu", "Hiệu sách nằm giữa ngân hàng và bưu điện.", "Nhớ chữ の trước あいだ."),
                new ClItem("ja-l43-cl-4", "hard", "Phủ định toàn phần だれも", "Điền từ thích hợp cho 'Trong phòng không có ai cả':", "へやの なかに {{0}} いません。", new[] { new ClBlank(new[] { "だれも", "誰も" }, "không ai cả (daremo)", "だれ + も + động từ phủ định = không ai cả.") }, "へやの なかに だれも いません。", "heya no naka ni daremo imasen", "Trong phòng không có ai cả.", "Không dùng だれが いません.")
            },
            translate: new[]
            {
                new TrItem("ja-l43-tr-1", "easy", "Trong hộp có chìa khóa", "Trong hộp có chìa khóa.", new[] { "はこ", "なか", "かぎ", "あります" }, new[] { "はこのなかにかぎがあります。", "箱の中に鍵があります。", "はこの なかに かぎが あります。" }, "箱の中に鍵があります。", "hako no naka ni kagi ga arimasu", "Địa điểm + に + Vật + が + あります."),
                new TrItem("ja-l43-tr-2", "easy", "Ngoài sân có con chó", "Ở bên ngoài nhà có một con chó.", new[] { "いえ", "そと", "いぬ", "います" }, new[] { "いえのそとにいぬがいます。", "家の外に犬がいます。", "いえの そとに いぬが います。" }, "家の外に犬がいます。", "ie no soto ni inu ga imasu", "Chó là động vật nên dùng います."),
                new TrItem("ja-l43-tr-3", "medium", "Vị trí kẹp giữa", "Quán cà phê nằm giữa hiệu sách và ngân hàng.", new[] { "カフェ", "ほんや", "ぎんこう", "あいだ" }, new[] { "カフェはほんやとぎんこうのあいだにあります。", "カフェは本屋と銀行の間にあります。" }, "カフェは本屋と銀行の間にあります。", "kafe wa honya to ginkou no aida ni arimasu", "Mẫu: X は A と B の あいだに あります."),
                new TrItem("ja-l43-tr-4", "hard", "Liệt kê với や〜など", "Trong tủ lạnh có táo, sữa, vân vân.", new[] { "れいぞうこ", "りんごや", "ぎゅうにゅうなど" }, new[] { "れいぞうこのなかにりんごやぎゅうにゅうなどがあります。", "冷蔵庫の中にりんごや牛乳などがあります。" }, "冷蔵庫の中にりんごや牛乳などがあります。", "reizouko no naka ni ringo ya gyuunyuu nado ga arimasu", "Dùng や〜など để liệt kê tiêu biểu vài món.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l43-gf-1", "easy", "Sai trợ từ で cho tồn tại", "えきの まえで こうばんが あります。", "Tồn tại phải dùng trợ từ に (không dùng で).", "えきの まえに こうばんが あります。", new[] { "えきの まえに こうばんが あります。", "駅の前に交番があります。" }, "eki no mae ni kouban ga arimasu", "Trước nhà ga có đồn cảnh sát.", "Tồn tại (あります/います) luôn gắn với trợ từ に."),
                new GfItem("ja-l43-gf-2", "medium", "Thiếu の trước あいだ", "びょういんは えきと こうえん あいだに あります。", "Thiếu trợ từ の trước danh từ vị trí あいだ.", "びょういんは えきと こうえんの あいだに あります。", new[] { "びょういんは えきと こうえんの あいだに あります。", "病院は駅と公園の間にあります。" }, "byouin wa eki to kouen no aida ni arimasu", "Bệnh viện nằm giữa ga và công viên.", "Mọi vị trí từ (うえ/なか/あいだ) phải có の phía trước."),
                new GfItem("ja-l43-gf-3", "hard", "Dùng が thay も trong câu phủ định", "きょうしつに だれが いません。", "Câu 'không ai cả' phải dùng だれも (không dùng だれが).", "きょうしつに だれも いません。", new[] { "きょうしつに だれも いません。", "教室に誰もいません。" }, "kyoushitsu ni daremo imasen", "Trong lớp học không có ai cả.", "だれも + phủ định = không ai; なにも + phủ định = không gì.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l43-rp-1", "easy", "Tìm đồ vật thất lạc", "Bạn không thấy chiếc ví của mình đâu và hỏi bạn cùng phòng.", "Bạn cùng phòng", "あれ、さいふを さがしていますか。", "are, saifu o sagashite imasu ka?", "Ủa, bạn đang tìm ví tiền à?", "Hỏi xem chiếc ví ở đâu (どこに ありますか) hoặc có trên bàn (つくえの うえ) không.", new[] { "さいふ|財布|かぎ", "どこ", "あります|うえ|なか" }, new[] { "はい、わたしの さいふが ありません。つくえの うえに ありますか。" }, "hai, watashi no saifu ga arimasen. tsukue no ue ni arimasu ka?", 8),
                new RpItem("ja-l43-rp-2", "medium", "Chỉ vị trí cửa hàng cho khách", "Một du khách hỏi bạn hiệu sách (ほんや) nằm ở đâu quanh đây.", "Du khách Nhật", "すみません、この ちかくに ほんやは ありますか。", "sumimasen, kono chikaku ni honya wa arimasu ka?", "Xin lỗi, ở gần đây có hiệu sách nào không ạ?", "Chỉ vị trí hiệu sách dùng các từ vị trí (となり/まえ/あいだ) và あります.", new[] { "あります", "となり|まえ|あいだ|ちかく|みぎ|ひだり", "えき|ぎんこう|デパート" }, new[] { "はい、えきと ぎんこうの あいだに おおきい ほんやが ありますよ。" }, "hai, eki to ginkou no aida ni ookii honya ga arimasu yo.", 8),
                new RpItem("ja-l43-rp-3", "hard", "Miêu tả bên trong căn phòng", "Cô giáo hỏi trong căn phòng hoặc tủ lạnh của bạn có những gì.", "Cô Suzuki", "ミンさんの へやに なにが ありますか。", "Min-san no heya ni nani ga arimasu ka?", "Trong phòng của Minh có những gì thế?", "Liệt kê 2-3 đồ vật dùng や〜など và kết thúc bằng あります (hoặc なにも ありません).", new[] { "や|と", "など|あります|います", "つくえ|ベッド|パソコン|ほん|ねこ" }, new[] { "へやの なかに つくえや ベッドや パソコンなどが あります。" }, "heya no naka ni tsukue ya beddo ya pasokon nado ga arimasu.", 8)
            }, now));

        // ---------------- LESSON 5.1 ----------------
        all.AddRange(BuildLessonPack(cId, JapaneseLesson51Id, "ja-JP", "Nhật N5 · Bài 5.1",
            shadowing: new[]
            {
                new ShItem("ja-l51-sh-1", "easy", "Nhờ mở cửa sổ (Dễ)", "すみません、まどをあけてください。", "すみません、まど を あけて ください / sumimasen, mado o akete kudasai", "Xin lỗi, hãy mở cửa sổ giúp tôi.", new[] { "あけて", "ください" }),
                new ShItem("ja-l51-sh-2", "easy", "Xin hãy đợi một chút (Dễ)", "ちょっとまってください。", "ちょっと まって ください / chotto matte kudasai", "Xin vui lòng đợi một chút.", new[] { "ちょっと", "まって" }),
                new ShItem("ja-l51-sh-3", "easy", "Đang mưa (Dễ)", "いま、あめがふっています。", "いま、あめ が ふって います / ima, ame ga futte imasu", "Bây giờ trời đang mưa.", new[] { "あめが", "ふっています" }),
                new ShItem("ja-l51-sh-4", "medium", "Đang sống ở Hà Nội (Vừa)", "わたしはハノイにすんでいます。", "わたし は ハノイ に すんで います / watashi wa Hanoi ni sunde imasu", "Tôi đang sống ở Hà Nội.", new[] { "ハノイに", "すんでいます" }),
                new ShItem("ja-l51-sh-5", "medium", "Ngoại lệ いって (Vừa)", "まっすぐいって、みぎにまがってください。", "まっすぐ いって、みぎ に まがって ください / massugu itte, migi ni magatte kudasai", "Hãy đi thẳng rồi rẽ phải.", new[] { "いって", "まがって" }),
                new ShItem("ja-l51-sh-6", "medium", "Nhờ viết tên (Vừa)", "ここになまえをかいてください。", "ここ に なまえ を かいて ください / koko ni namae o kaite kudasai", "Xin hãy viết tên vào đây.", new[] { "なまえを", "かいて" }),
                new ShItem("ja-l51-sh-7", "hard", "Đề nghị giúp đỡ (Khó)", "にもつがおおいですね。もちましょうか。", "にもつ が おおい です ね。もちましょう か / nimotsu ga ooi desu ne. mochimashou ka", "Nhiều hành lý quá nhỉ. Để tôi xách phụ nhé?", new[] { "にもつ", "もちましょうか" }),
                new ShItem("ja-l51-sh-8", "hard", "Biết và Không biết (Khó)", "たなかさんをしっていますか。いいえ、しりません。", "たなかさん を しって います か。いいえ、しりません / Tanaka-san o shitte imasu ka. iie, shirimasen", "Bạn có biết anh Tanaka không? Không, tôi không biết.", new[] { "しっていますか", "しりません" }),
                new ShItem("ja-l51-sh-9", "hard", "Chuỗi 3 hành động Thể Te (Khó)", "あさおきて、コーヒーをのんで、かいしゃへいきます。", "あさ おきて、コーヒー を のんで、かいしゃ へ いきます / asa okite, koohii o nonde, kaisha e ikimasu", "Sáng thức dậy, uống cà phê rồi đi đến công ty.", new[] { "おきて", "のんで", "いきます" })
            },
            dictation: new[]
            {
                new DiItem("ja-l51-di-1", "easy", "Hãy ngồi xuống", "ここにすわってください", "koko ni suwatte kudasai", "Xin mời ngồi ở đây.", new[] { "ここに", "すわって", "ください" }, new[] { "たって", "おきて", "います" }),
                new DiItem("ja-l51-di-2", "easy", "Đang đọc sách", "いまほんをよんでいます", "ima hon o yonde imasu", "Bây giờ tôi đang đọc sách.", new[] { "いま", "ほんを", "よんで", "います" }, new[] { "かいて", "ください", "みました" }),
                new DiItem("ja-l51-di-3", "medium", "Hãy nói chậm lại", "ゆっくりはなしてください", "yukkuri hanashite kudasai", "Xin hãy nói chậm rãi.", new[] { "ゆっくり", "はなして", "ください" }, new[] { "いそいで", "きいて", "います" }),
                new DiItem("ja-l51-di-4", "medium", "Đang làm việc ở công ty", "にほんのかいしゃではたらいています", "Nihon no kaisha de hataraite imasu", "Tôi đang làm việc ở công ty Nhật.", new[] { "にほんの", "かいしゃで", "はたらいて", "います" }, new[] { "すんで", "ください", "に" }),
                new DiItem("ja-l51-di-5", "hard", "Hãy vội lên một chút", "じかんがありませんからいそいでください", "jikan ga arimasen kara isoide kudasai", "Vì không có thời gian nên hãy nhanh lên.", new[] { "じかんが", "ありませんから", "いそいで", "ください" }, new[] { "いそいて", "まって", "います" }),
                new DiItem("ja-l51-di-6", "hard", "Đặt xuống trên bàn", "つくえのうえにおいてください", "tsukue no ue ni oite kudasai", "Xin hãy đặt lên trên bàn.", new[] { "つくえの", "うえに", "おいて", "ください" }, new[] { "おきて", "もって", "で" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l51-pr-1", "easy", "Âm ngắt まって", "ちょっとまってください", "chotto matte kudasai", "Xin đợi một chút.", new[] { "ちょっと", "まって" }, "Ngắt 1 nhịp rõ ở cả hai chữ っ trong ちょっと và まって.", new[] { "Nén hơi っ trước と", "Nén hơi っ trước て", "Đọc ください nhẹ nhàng" }, "すみません、ちょっとまってください。", new[] { "ちょっと", "まって" }),
                new PrItem("ja-l51-pr-2", "easy", "Đuôi んで (よんで)", "このほんをよんでください", "kono hon o yonde kudasai", "Hãy đọc cuốn sách này.", new[] { "ほん", "よんで" }, "Đọc tròn âm mũi ん trước âm đục で trong よんで.", new[] { "よ tròn môi nhẹ", "ん giữ âm mũi", "で rõ âm đục" }, "いっしょにほんをよんでいます。", new[] { "よんで", "います" }),
                new PrItem("ja-l51-pr-3", "medium", "Âm đục いで (いそいで)", "すこしいそいでください", "sukoshi isoide kudasai", "Hãy nhanh lên một chút.", new[] { "すこし", "いそいで" }, "Phân biệt rõ いで (có tenten) với いて.", new[] { "そ rõ ràng", "い nối mượt sang で", "くだ nhấn nhẹ" }, "じかんがないから、いそいでください。", new[] { "いそいで", "ください" }),
                new PrItem("ja-l51-pr-4", "medium", "Trường âm べんきょうして", "まいにちべんきょうしています", "mainichi benkyou shite imasu", "Mỗi ngày tôi đều đang học.", new[] { "べんきょう", "しています" }, "Kéo dài きょう đủ 2 phách rồi chuyển nhanh sang して.", new[] { "べん giữ âm mũi", "きょう kéo dài 2 phách", "して ngắn gọn" }, "いまにほんごをべんきょうしています。", new[] { "べんきょう", "しています" }),
                new PrItem("ja-l51-pr-5", "hard", "Ngoại lệ いって vs おいて", "そこにおいてからいってください", "soko ni oite kara itte kudasai", "Đặt ở đó rồi hãy đi.", new[] { "おいて", "いって" }, "Phân biệt chuẩn おいて (từ おきます) và いって (ngoại lệ いきます).", new[] { "お-い-て đều 3 phách", "いっ-て nén hơi っ", "Giữ ngữ điệu tự nhiên" }, "はこをここにおいてください。", new[] { "おいて", "ください" }),
                new PrItem("ja-l51-pr-6", "hard", "Cặp てつだって", "しごとをてつだってください", "shigoto o tetsudatte kudasai", "Xin hãy giúp tôi công việc này.", new[] { "しごと", "てつだって" }, "Đọc chính xác tổ hợp て-つ-だっ-て.", new[] { "つ bật nhẹ đầu lưỡi", "だ âm đục rõ", "Nén hơi っ trước て" }, "すみませんが、てつだってください。", new[] { "すみませんが", "てつだって" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l51-wm-1", "easy", "Sơ đồ từ vựng: あけます / しめます (Mở / Đóng)", "あけます・しめます", "akemasu / shimemasu", "Mở / Đóng (Nhóm II)",
                    new[] { new WmNode("まどを あけて", "mado o akete", "Mở cửa sổ", "あついから、まどを あけて ください。"), new WmNode("ドアを しめて", "doa o shimete", "Đóng cửa ra vào", "ドアを しめて ください。") },
                    new[] { new WmNode("でんきを つけて / けして", "denki o tsukete / keshite", "Bật đèn / Tắt đèn", "でんきを つけて ください。") },
                    new WmNode[] { new WmNode("エアコン", "eakon", "Điều hòa", "エアコンを けします。") },
                    new[] { new WmEx("へやが くらいですから、でんきを つけて ください。", "heya ga kurai desu kara, denki o tsukete kudasai", "Vì phòng tối nên hãy bật đèn lên giúp tôi.") }),
                new WmItem("ja-l51-wm-2", "medium", "Sơ đồ từ vựng: 〜ています (Tiếp diễn & Trạng thái)", "〜ています", "~te imasu", "Đang ... (hành động / trạng thái)",
                    new[] { new WmNode("すんでいます", "sunde imasu", "Đang sinh sống", "ハノイに すんでいます。"), new WmNode("はたらいています", "hataraite imasu", "Đang làm việc", "ぎんこうで はたらいています。") },
                    new[] { new WmNode("しっています", "shitte imasu", "Biết (phủ định: しりません)", "その うたを しっていますか。") },
                    new WmNode[] { new WmNode("けっこんしています", "kekkon shite imasu", "Đã kết hôn", "あには けっこんしています。") },
                    new[] { new WmEx("いま としょかんで ともだちと べんきょうしています。", "ima toshokan de tomodachi to benkyou shite imasu", "Bây giờ tôi đang học bài cùng bạn ở thư viện.") }),
                new WmItem("ja-l51-wm-3", "hard", "Sơ đồ từ vựng: てつだいます / もちます (Giúp đỡ)", "てつだいます・もちます", "tetsudaimasu / mochimasu", "Giúp đỡ / Cầm mang",
                    new[] { new WmNode("てつだって ください", "tetsudatte kudasai", "Hãy giúp tôi", "ちょっと てつだって ください。"), new WmNode("もちましょうか", "mochimashou ka", "Để tôi xách giúp nhé?", "その にもつを もちましょうか。") },
                    new[] { new WmNode("よびましょうか", "yobimashou ka", "Để tôi gọi giúp nhé?", "タクシーを よびましょうか。") },
                    new WmNode[] { new WmNode("かします / おしえます", "kashimasu / oshiemasu", "Cho mượn / Chỉ dạy", "みちを おしえて ください。") },
                    new[] { new WmEx("すみませんが、もう いちど おしえて ください。", "sumimasen ga, mou ichido oshiete kudasai", "Xin lỗi nhưng làm ơn chỉ lại cho tôi một lần nữa nhé.") })
            },
            cloze: new[]
            {
                new ClItem("ja-l51-cl-1", "easy", "Thể Te Nhóm I: のみます", "Chia động từ のみます sang Thể Te:", "あつい おちゃを {{0}} ください。", new[] { new ClBlank(new[] { "のんで", "飲んで" }, "thể Te của のみます", "Đuôi み đổi thành んで.") }, "あつい おちゃを のんで ください。", "atsui ocha o nonde kudasai", "Xin mời uống trà nóng ạ.", "Nhóm み・び・に → んで."),
                new ClItem("ja-l51-cl-2", "easy", "Ngoại lệ いきます", "Chia いきます sang Thể Te để chỉ đường:", "まっすぐ {{0}}、ひだりに まがって ください。", new[] { new ClBlank(new[] { "いって", "行って" }, "thể Te của いきます", "いきます là ngoại lệ: いって (không phải いいて).") }, "まっすぐ いって、ひだりに まがって ください。", "massugu itte, hidari ni magatte kudasai", "Hãy đi thẳng rồi rẽ trái.", "Chỉ riêng いきます thành いって; かきます vẫn là かいて."),
                new ClItem("ja-l51-cl-3", "medium", "Tiếp diễn trạng thái すみます", "Chia すみます sang dạng đang sinh sống:", "わたしは いま おおさかに {{0}}。", new[] { new ClBlank(new[] { "すんでいます", "住んでいます" }, "đang sống (sunde imasu)", "Đuôi み → んで + います.") }, "わたしは いま おおさかに すんでいます。", "watashi wa ima Oosaka ni sunde imasu", "Hiện tại tôi đang sống ở Osaka.", "Cư trú ở đâu dùng [Nơi] に すんでいます."),
                new ClItem("ja-l51-cl-4", "hard", "Phủ định của しっています", "Trả lời phủ định cho câu hỏi 'Bạn có biết anh Miller không?':", "ミラーさんを しっていますか。— いいえ、{{0}}。", new[] { new ClBlank(new[] { "しりません", "知りません" }, "không biết (shirimasen)", "Phủ định của しっています là しりません (không dùng しっていません).") }, "ミラーさんを しっていますか。— いいえ、しりません。", "Miraa-san o shitte imasu ka? — Iie, shirimasen", "Bạn có biết anh Miller không? — Không, tôi không biết.", "Ngoại lệ đặc biệt phải nhớ trong JLPT N5.")
            },
            translate: new[]
            {
                new TrItem("ja-l51-tr-1", "easy", "Xin hãy viết tên", "Xin hãy viết tên của bạn vào đây.", new[] { "ここ", "なまえ", "かいて", "ください" }, new[] { "ここになまえをかいてください。", "ここに名前を書いてください。", "ここに なまえを かいて ください。" }, "ここに名前を書いてください。", "koko ni namae o kaite kudasai", "かきます (đuôi き) → かいて + ください."),
                new TrItem("ja-l51-tr-2", "easy", "Trời đang mưa", "Bây giờ trời đang mưa.", new[] { "いま", "あめ", "ふっています" }, new[] { "いま、あめがふっています。", "いまあめがふっています。", "今、雨が降っています。" }, "今、雨が降っています。", "ima, ame ga futte imasu", "ふります (đuôi り) → ふって + います."),
                new TrItem("ja-l51-tr-3", "medium", "Đề nghị giúp đỡ", "Để tôi xách hành lý giúp bạn nhé?", new[] { "にもつ", "もちましょうか" }, new[] { "にもつをもちましょうか。", "荷物を持ちましょうか。" }, "荷物を持ちましょうか。", "nimotsu o mochimashou ka", "Dùng Vましょうか để chủ động ngỏ ý giúp đỡ."),
                new TrItem("ja-l51-tr-4", "hard", "Chuỗi 3 hành động", "Mỗi sáng tôi dậy lúc 6 giờ, ăn sáng rồi đi học.", new[] { "まいあさ", "ろくじ", "おきて", "たべて", "いきます" }, new[] { "まいあさろくじにおきて、あさごはんをたべて、がっこうへいきます。", "毎朝6時に起きて、朝ごはんを食べて、学校へ行きます。" }, "毎朝6時に起きて、朝ごはんを食べて、学校へ行きます。", "maiasa rokuji ni okite, asagohan o tabete, gakkou e ikimasu", "Nối các hành động bằng Thể Te, chỉ động từ cuối chia ます.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l51-gf-1", "easy", "Chia sai ngoại lệ いきます", "えきへ いきて ください。", "Thể Te của いきます là いって (ngoại lệ).", "えきへ いって ください。", new[] { "えきへ いって ください。", "駅へ行ってください。" }, "eki e itte kudasai", "Hãy đi ra nhà ga.", "き → いて nhưng いきます → いって."),
                new GfItem("ja-l51-gf-2", "medium", "Phủ định sai của しっています", "いいえ、たなかさんを しっていません。", "Phủ định của しっています là しりません.", "いいえ、たなかさんを しりません。", new[] { "いいえ、たなかさんを しりません。", "いいえ、田中さんを知りません。" }, "iie, Tanaka-san o shirimasen", "Không, tôi không biết anh Tanaka.", "しっています → しりません."),
                new GfItem("ja-l51-gf-3", "hard", "Chia thì quá khứ ở giữa chuỗi Thể Te", "きのう うちへ かえりました、ごはんを たべました。", "Khi nối chuỗi hành động, động từ trước phải chia Thể Te, chỉ động từ cuối chia quá khứ.", "きのう うちへ かえって、ごはんを たべました。", new[] { "きのう うちへ かえって、ごはんを たべました。", "昨日家へ帰って、ご飯を食べました。" }, "kinou uchi e kaette, gohan o tabemashita", "Hôm qua tôi về nhà rồi ăn cơm.", "かえります → かえって.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l51-rp-1", "easy", "Nhờ đồng nghiệp giúp trong văn phòng", "Trời trong phòng hơi nóng và bạn muốn nhờ đồng nghiệp mở cửa sổ hoặc bật điều hòa.", "Anh Sato", "あれ、きょうは ちょっと あついですね。", "are, kyou wa chotto atsui desu ne.", "Ủa, hôm nay hơi nóng nhỉ.", "Dùng Thể Te + ください để nhờ mở cửa sổ (まどを あけて) hoặc bật điều hòa (エアコンを つけて).", new[] { "て|で", "ください", "まど|エアコン|あけて|つけて" }, new[] { "そうですね。すみませんが、まどを あけて ください。" }, "sou desu ne. sumimasen ga, mado o akete kudasai.", 8),
                new RpItem("ja-l51-rp-2", "medium", "Kể về nơi ở và công việc hiện tại", "Người bạn mới quen hỏi bạn hiện đang sống ở đâu và làm gì.", "Bạn Yuki", "ミンさんは いま どこに すんでいますか。おしごとは なにを していますか。", "Min-san wa ima doko ni sunde imasu ka? oshigoto wa nani o shite imasu ka?", "Minh hiện giờ đang sống ở đâu? Công việc đang làm gì thế?", "Trả lời bằng cấu trúc 〜ています (すんでいます / はたらいています / べんきょうしています).", new[] { "ています|ています。", "すんで|はたらいて|べんきょうして", "に|で" }, new[] { "いま ハノイに すんでいます。ITのかいしゃで はたらいています。" }, "ima Hanoi ni sunde imasu. IT no kaisha de hataraite imasu.", 8),
                new RpItem("ja-l51-rp-3", "hard", "Chủ động giúp đỡ người lớn tuổi", "Bạn thấy cô giáo đang xách hai túi tài liệu rất nặng ở hành lang.", "Cô Tanaka", "ふう、きょうは にもつが おもいですね。", "fuu, kyou wa nimotsu ga omoi desu ne.", "Phù, hôm nay hành lý nặng quá đi mất.", "Chủ động ngỏ ý giúp đỡ bằng 〜ましょうか (もちましょうか / てつだいましょうか).", new[] { "ましょうか", "もち|てつだい|にもつ", "だいじょうぶ|すみません" }, new[] { "せんせい、おもそうですね。にもつを もちましょうか。" }, "sensei, omosou desu ne. nimotsu o mochimashou ka?", 8)
            }, now));

        // ---------------- LESSON 5.2 ----------------
        all.AddRange(BuildLessonPack(cId, JapaneseLesson52Id, "ja-JP", "Nhật N5 · Bài 5.2",
            shadowing: new[]
            {
                new ShItem("ja-l52-sh-1", "easy", "Xin phép chụp ảnh (Dễ)", "ここで しゃしんを とっても いいですか。", "ここ で しゃしん を とって も いい です か / koko de shashin o totte mo ii desu ka", "Tôi chụp ảnh ở đây có được không ạ?", new[] { "とっても", "いいですか" }),
                new ShItem("ja-l52-sh-2", "easy", "Xin phép ngồi ghế (Dễ)", "ここに すわっても いいですか。", "ここ に すわって も いい です か / koko ni suwatte mo ii desu ka", "Tôi ngồi ở đây có được không?", new[] { "すわっても", "いいですか" }),
                new ShItem("ja-l52-sh-3", "easy", "Cấm chụp ảnh (Dễ)", "ここで しゃしんを とっては いけません。", "ここ で しゃしん を とって は いけません / koko de shashin o totte wa ikemasen", "Không được phép chụp ảnh ở đây.", new[] { "とっては", "いけません" }),
                new ShItem("ja-l52-sh-4", "medium", "Cấm hút thuốc (Vừa)", "きょうしつで たばこを すっては いけません。", "きょうしつ で たばこ を すって は いけません / kyoushitsu de tabako o sutte wa ikemasen", "Không được hút thuốc lá trong lớp học.", new[] { "すっては", "いけません" }),
                new ShItem("ja-l52-sh-5", "medium", "Ngoại lệ はいります (Vừa)", "へやに はいっても いいですか。ええ、どうぞ。", "へや に はいって も いい です か。ええ、どうぞ / heya ni haitte mo ii desu ka. ee, douzo", "Tôi vào phòng được không? Ừ, mời bạn.", new[] { "はいっても", "どうぞ" }),
                new ShItem("ja-l52-sh-6", "medium", "Thứ tự hành động てから (Vừa)", "てを あらってから、ごはんを たべます。", "て を あらって から、ごはん を たべます / te o aratte kara, gohan o tabemasu", "Sau khi rửa tay xong tôi mới ăn cơm.", new[] { "あらってから", "たべます" }),
                new ShItem("ja-l52-sh-7", "hard", "Từ chối mềm mỏng (Khó)", "すみません、ここは きんえんですから、ちょっと…。", "すみません、ここ は きんえん です から、ちょっと… / sumimasen, koko wa kin'en desu kara, chotto...", "Xin lỗi, chỗ này cấm hút thuốc nên hơi bất tiện một chút...", new[] { "きんえんですから", "ちょっと" }),
                new ShItem("ja-l52-sh-8", "hard", "Cấm chạm tay vào hiện vật (Khó)", "びじゅつかんの えに さわっては いけません。", "びじゅつかん の え に さわって は いけません / bijutsukan no e ni sawatte wa ikemasen", "Không được chạm tay vào tranh trong bảo tàng.", new[] { "さわっては", "いけません" }),
                new ShItem("ja-l52-sh-9", "hard", "Chuỗi kết hợp て và てから (Khó)", "うちへ かえって、シャワーを あびてから、ねます。", "うち へ かえって、シャワー を あびて から、ねます / uchi e kaette, shawaa o abite kara, nemasu", "Về nhà, rồi sau khi tắm vòi sen xong thì tôi đi ngủ.", new[] { "かえって", "あびてから" })
            },
            dictation: new[]
            {
                new DiItem("ja-l52-di-1", "easy", "Xin dùng bút này", "このペンをつかってもいいですか", "kono pen o tsukatte mo ii desu ka", "Tôi dùng cây bút này được không?", new[] { "このペンを", "つかっても", "いいですか" }, new[] { "はいけません", "ください", "つかって" }),
                new DiItem("ja-l52-di-2", "easy", "Không được đỗ xe", "ここにとめてはいけません", "koko ni tomete wa ikemasen", "Không được đỗ xe ở đây.", new[] { "ここに", "とめては", "いけません" }, new[] { "も", "いいですか", "ください" }),
                new DiItem("ja-l52-di-3", "medium", "Sau khi làm bài tập", "しゅくだいをしてからあそびます", "shukudai o shite kara asobimasu", "Làm bài tập xong rồi mới chơi.", new[] { "しゅくだいを", "してから", "あそびます" }, new[] { "しますから", "しています", "まで" }),
                new DiItem("ja-l52-di-4", "medium", "Xin phép mang về", "りょうりをもちかえってもいいですか", "ryouri o mochikaette mo ii desu ka", "Tôi mang đồ ăn về có được không?", new[] { "りょうりを", "もちかえっても", "いいですか" }, new[] { "もちかえりて", "いけません", "ください" }),
                new DiItem("ja-l52-di-5", "hard", "Không được vào phòng này", "このへやにはいってはいけません", "kono heya ni haitte wa ikemasen", "Không được phép vào căn phòng này.", new[] { "このへやに", "はいっては", "いけません" }, new[] { "はいりて", "も", "で" }),
                new DiItem("ja-l52-di-6", "hard", "Nộp báo cáo xong rồi về", "レポートをだしてからかえります", "repooto o dashite kara kaerimasu", "Nộp báo cáo xong rồi tôi về.", new[] { "レポートを", "だしてから", "かえります" }, new[] { "だしますから", "います", "ください" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l52-pr-1", "easy", "Xin phép 〜てもいい", "ここにすわってもいいですか", "koko ni suwatte mo ii desu ka", "Tôi ngồi đây được không?", new[] { "すわって", "いいですか" }, "Giữ âm ngắt っ trong すわって và kéo dài nhẹ いい.", new[] { "っ nén hơi 1 nhịp", "ても nối mượt", "いい kéo dài nhẹ" }, "すみません、ここにすわってもいいですか。", new[] { "すわっても", "いいですか" }),
                new PrItem("ja-l52-pr-2", "easy", "Cấm đoán 〜てはいけません", "しゃしんをとってはいけません", "shashin o totte wa ikemasen", "Không được chụp ảnh.", new[] { "しゃしん", "とっては", "いけません" }, "Đọc trợ từ は thành 'wa' sau とって.", new[] { "とっ nén hơi っ", "は đọc thành wa", "いけません rõ 5 âm" }, "ここでしゃしんをとってはいけません。", new[] { "とっては", "いけません" }),
                new PrItem("ja-l52-pr-3", "medium", "Thứ tự 〜てから", "てをあらってからたべます", "te o aratte kara tabemasu", "Rửa tay xong rồi mới ăn.", new[] { "あらって", "から" }, "Ngắt nhẹ sau から để nhấn mạnh ranh giới hai hành động.", new[] { "ら mềm nhẹ", "っ nén hơi 1 nhịp", "から rõ ràng" }, "うちへかえって、てをあらってからたべます。", new[] { "あらってから", "たべます" }),
                new PrItem("ja-l52-pr-4", "medium", "Động từ すいます → すって", "たばこをすってはいけません", "tabako o sutte wa ikemasen", "Không được hút thuốc.", new[] { "たばこ", "すっては" }, "Đọc tròn miệng す rồi nén hơi っ thật rõ.", new[] { "す tròn nhẹ", "っ giữ 1 phách", "は đọc là wa" }, "びょういんでたばこをすってはいけません。", new[] { "すっては", "いけません" }),
                new PrItem("ja-l52-pr-5", "hard", "Bảo tàng びじゅつかん", "びじゅつかんでとってはいけません", "bijutsukan de totte wa ikemasen", "Không được chụp trong bảo tàng.", new[] { "びじゅつかん", "とっては" }, "Đọc chính xác âm ghép じゅ và âm đầu lưỡi つ trong びじゅつかん.", new[] { "じゅ gọn 1 phách", "つ bật đầu lưỡi nhẹ", "かん âm mũi rõ" }, "びじゅつかんでしゃしんをとってはいけません。", new[] { "びじゅつかん", "とっては" }),
                new PrItem("ja-l52-pr-6", "hard", "Thứ tự シャワーをあびてから", "シャワーをあびてからねます", "shawaa o abite kara nemasu", "Tắm vòi sen xong rồi ngủ.", new[] { "シャワー", "あびてから" }, "Kéo dài trường âm ワー đủ 2 phách và đọc nhẹ あびて.", new[] { "シャ mềm", "ワー kéo dài", "あびて đều nhịp" }, "まいばんシャワーをあびてからねます。", new[] { "シャワー", "あびてから" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l52-wm-1", "easy", "Sơ đồ từ vựng: 〜てもいいですか (Xin phép)", "〜ても いいですか", "~te mo ii desu ka", "Tôi làm ... có được không?",
                    new[] { new WmNode("とっても いいですか", "totte mo ii desu ka", "Chụp ảnh được không?", "ここで しゃしんを とっても いいですか。"), new WmNode("はいっても いいですか", "haitte mo ii desu ka", "Vào được không?", "へやに はいっても いいですか。") },
                    new[] { new WmNode("ええ、どうぞ", "ee, douzo", "Ừ, xin mời", "ええ、いいですよ。どうぞ。") },
                    new WmNode[] { new WmNode("〜ては いけません", "~te wa ikemasen", "Không được làm...", "さわっては いけません。") },
                    new[] { new WmEx("すみません、この いすに すわっても いいですか。", "sumimasen, kono isu ni suwatte mo ii desu ka", "Xin lỗi, tôi ngồi ở chiếc ghế này có được không ạ?") }),
                new WmItem("ja-l52-wm-2", "medium", "Sơ đồ từ vựng: きんえん / びじゅつかん (Quy định)", "きまり・ちゅうい", "kin'en / bijutsukan / chuu'i", "Nội quy & Lưu ý công cộng",
                    new[] { new WmNode("たばこを すっては いけません", "tabako o sutte wa ikemasen", "Cấm hút thuốc", "ここは きんえんです。"), new WmNode("くるまを とめては いけません", "kuruma o tomete wa ikemasen", "Cấm đỗ xe", "みちに とめては いけません。") },
                    new[] { new WmNode("えに さわる", "e ni sawaru", "Chạm vào tranh", "えに さわっては いけません。") },
                    new WmNode[] { new WmNode("すみません、ちょっと", "sumimasen, chotto", "Từ chối mềm mỏng", "すみません、ここは ちょっと…。") },
                    new[] { new WmEx("びじゅつかんの なかでは しずかに して ください。", "bijutsukan no naka de wa shizuka ni shite kudasai", "Trong bảo tàng xin hãy giữ yên lặng.") }),
                new WmItem("ja-l52-wm-3", "hard", "Sơ đồ từ vựng: 〜てから (Sau khi...)", "Vて から、V2", "Vte kara, V2", "Sau khi làm V1 xong thì làm V2",
                    new[] { new WmNode("しゅくだいを してから", "shukudai o shite kara", "Sau khi làm bài tập", "しゅくだいを してから、テレビを みます。"), new WmNode("てを あらってから", "te o aratte kara", "Sau khi rửa tay", "てを あらってから、たべます。") },
                    new[] { new WmNode("レポートを だしてから", "repooto o dashite kara", "Sau khi nộp báo cáo", "レポートを だしてから、かえります。") },
                    new WmNode[] { new WmNode("V1て、V2て、V3", "V1te, V2te, V3", "Liệt kê chuỗi hành động", "おきて、たべて、いきます。") },
                    new[] { new WmEx("にほんごを べんきょうしてから、にほんへ いきたいです。", "nihongo o benkyou shite kara, Nihon e ikitai desu", "Sau khi học tiếng Nhật xong, tôi muốn sang Nhật.") })
            },
            cloze: new[]
            {
                new ClItem("ja-l52-cl-1", "easy", "Cấu trúc xin phép 〜ても いいですか", "Điền đuôi xin phép vào câu hỏi 'Tôi dùng cái này được không?':", "これを つかって{{0}} いいですか。", new[] { new ClBlank(new[] { "も" }, "trợ từ も trong xin phép", "Mẫu xin phép: Thể Te + も + いいですか.") }, "これを つかっても いいですか。", "kore o tsukatte mo ii desu ka", "Tôi dùng cái này có được không ạ?", "Đừng nhầm với は (dùng trong cấm đoán)."),
                new ClItem("ja-l52-cl-2", "easy", "Cấu trúc cấm đoán 〜ては いけません", "Điền trợ từ đúng trong câu cấm đoán:", "ここで くるまを とめて{{0}} いけません。", new[] { new ClBlank(new[] { "は" }, "trợ từ は (đọc là wa)", "Mẫu cấm đoán: Thể Te + は + いけません.") }, "ここで くるまを とめては いけません。", "koko de kuruma o tomete wa ikemasen", "Không được đỗ xe ở đây.", "は là thành phần bắt buộc của mẫu cấm đoán."),
                new ClItem("ja-l52-cl-3", "medium", "Thứ tự hành động 〜てから", "Điền dạng đúng để nói 'Sau khi rửa tay thì ăn cơm':", "てを {{0}}から、ごはんを たべます。", new[] { new ClBlank(new[] { "あらって", "洗って" }, "thể Te của あらいます", "Đuôi い đổi thành って → あらって + から.") }, "てを あらってから、ごはんを たべます。", "te o aratte kara, gohan o tabemasu", "Sau khi rửa tay xong tôi ăn cơm.", "Vてから = sau khi V xong."),
                new ClItem("ja-l52-cl-4", "hard", "Động từ Nhóm I giả Nhóm II: はいります", "Chia động từ はいります (vào) trong câu xin phép:", "へやに {{0}}も いいですか。", new[] { new ClBlank(new[] { "はいって", "入って" }, "thể Te của はいります", "はいります là Nhóm I nên chia là はいって.") }, "へやに はいっても いいですか。", "heya ni haitte mo ii desu ka", "Tôi vào phòng có được không ạ?", "Cẩn thận không chia thành はいりて.")
            },
            translate: new[]
            {
                new TrItem("ja-l52-tr-1", "easy", "Xin phép chụp ảnh", "Tôi chụp ảnh ở đây có được không?", new[] { "ここ", "しゃしん", "とっても", "いいですか" }, new[] { "ここでしゃしんをとってもいいですか。", "ここで写真を撮ってもいいですか。", "ここで しゃしんを とっても いいですか。" }, "ここで写真を撮ってもいいですか。", "koko de shashin o totte mo ii desu ka", "Địa điểm hành động dùng で."),
                new TrItem("ja-l52-tr-2", "easy", "Không được hút thuốc", "Không được hút thuốc lá ở đây.", new[] { "ここ", "たばこ", "すっては", "いけません" }, new[] { "ここでたばこをすってはいけません。", "ここでたばこを吸ってはいけません。" }, "ここでたばこを吸ってはいけません。", "koko de tabako o sutte wa ikemasen", "すいます (đuôi い) → すって."),
                new TrItem("ja-l52-tr-3", "medium", "Sau khi làm bài tập", "Sau khi làm bài tập về nhà xong tôi xem phim.", new[] { "しゅくだい", "してから", "えいが", "みます" }, new[] { "しゅくだいをしてから、えいがをみます。", "宿題をしてから、映画を見ます。" }, "宿題をしてから、映画を見ます。", "shukudai o shite kara, eiga o mimasu", "Dùng してから để nhấn mạnh thứ tự."),
                new TrItem("ja-l52-tr-4", "hard", "Hỏi xin trả sách ngày mai", "Ngày mai tôi trả cuốn sách này có được không?", new[] { "あした", "このほん", "かえしても", "いいですか" }, new[] { "あしたこのほんをかえしてもいいですか。", "明日この本を返してもいいですか。" }, "明日この本を返してもいいですか。", "ashita kono hon o kaeshite mo ii desu ka", "かえします (đuôi し) → かえして.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l52-gf-1", "easy", "Thiếu trợ từ は trong câu cấm đoán", "きょうしつで たべて いけません。", "Mẫu cấm đoán bắt buộc phải có trợ từ は (とってはいけません / たべてはいけません).", "きょうしつで たべては いけません。", new[] { "きょうしつで たべては いけません。", "教室で食べてはいけません。" }, "kyoushitsu de tabete wa ikemasen", "Không được ăn trong lớp học.", "Vて + は + いけません."),
                new GfItem("ja-l52-gf-2", "medium", "Chia nhầm はいります thành はいりて", "ここに はいりても いいですか。", "はいります là động từ Nhóm I nên Thể Te là はいって.", "ここに はいっても いいですか。", new[] { "ここに はいっても いいですか。", "ここに入ってもいいですか。" }, "koko ni haitte mo ii desu ka", "Tôi vào đây có được không?", "Đừng nhầm động từ Nhóm I đuôi り với Nhóm II."),
                new GfItem("ja-l52-gf-3", "hard", "Dùng ますから thay cho てから", "てを あらいますから、ごはんを たべます。", "Muốn nói 'Sau khi rửa tay rồi mới ăn' phải dùng てから (ますから nghĩa là 'vì').", "てを あらってから、ごはんを たべます。", new[] { "てを あらってから、ごはんを たべます。", "手を洗ってから、ご飯を食べます。" }, "te o aratte kara, gohan o tabemasu", "Sau khi rửa tay xong tôi ăn cơm.", "Vて + から = sau khi; Vます + から = vì.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l52-rp-1", "easy", "Xin phép tại thư viện/phòng học", "Bạn vào phòng đọc sách và muốn xin phép thầy giáo ngồi ghế và dùng bút.", "Thầy Tanaka", "はい、ミンさん。どうしましたか。", "hai, Min-san. dou shimashita ka?", "Vâng, có chuyện gì thế em Minh?", "Dùng mẫu xin phép 〜ても いいですか để hỏi ngồi đây (すわっても) hoặc dùng bút/chụp ảnh.", new[] { "ても", "いいですか", "すわって|つかって|とって|はいって" }, new[] { "すみません、ここに すわっても いいですか。" }, "sumimasen, koko ni suwatte mo ii desu ka?", 8),
                new RpItem("ja-l52-rp-2", "medium", "Nhắc nhở quy định bảo tàng", "Bạn là nhân viên bảo tàng thấy khách chuẩn bị giơ máy ảnh lên chụp tranh.", "Khách tham quan", "わあ、きれいな えですね！ここで しゃしんを とっても いいですか。", "waa, kirei na e desu ne! koko de shashin o totte mo ii desu ka?", "Ôi bức tranh đẹp quá! Tôi chụp ảnh ở đây được không?", "Từ chối lịch sự bằng 〜ては いけません hoặc giải thích ngắn và bảo đừng chạm vào tranh.", new[] { "とっては|さわっては|すみません", "いけません|きんし|ちょっと", "びじゅつかん|しゃしん" }, new[] { "すみません、ここでは しゃしんを とっては いけません。" }, "sumimasen, koko de wa shashin o totte wa ikemasen.", 8),
                new RpItem("ja-l52-rp-3", "hard", "Kể kế hoạch tối nay theo thứ tự", "Bạn cùng phòng hỏi tối nay làm bài tập trước hay ăn cơm trước.", "Bạn Kenji", "こんばんは なにを しますか。もう ごはんを たべますか。", "konban wa nani o shimasu ka? mou gohan o tabemasu ka?", "Tối nay bạn định làm gì thế? Ăn cơm luôn chưa?", "Trả lời dùng cấu trúc 〜てから để nêu rõ việc nào làm xong trước rồi mới làm việc sau.", new[] { "てから", "しゅくだい|シャワー|てをあらって|かえって", "ます|たべます|ねます" }, new[] { "いいえ、しゅくだいを してから、ごはんを たべます。" }, "iie, shukudai o shite kara, gohan o tabemasu.", 8)
            }, now));

        // ---------------- LESSON 5.3 ----------------
        all.AddRange(BuildLessonPack(cId, JapaneseLesson53Id, "ja-JP", "Nhật N5 · Bài 5.3",
            shadowing: new[]
            {
                new ShItem("ja-l53-sh-1", "easy", "Muốn đi Nhật (Dễ)", "わたしは にほんへ いきたいです。", "わたし は にほん へ いきたい です / watashi wa Nihon e ikitai desu", "Tôi muốn đi Nhật Bản.", new[] { "にほんへ", "いきたいです" }),
                new ShItem("ja-l53-sh-2", "easy", "Muốn có laptop mới (Dễ)", "あたらしい パソコンが ほしいです。", "あたらしい パソコン が ほしい です / atarashii pasokon ga hoshii desu", "Tôi muốn có một chiếc laptop mới.", new[] { "パソコンが", "ほしいです" }),
                new ShItem("ja-l53-sh-3", "easy", "Muốn ăn sushi (Dễ)", "おいしい すしが たべたいです。", "おいしい すし が たべたい です / oishii sushi ga tabetai desu", "Tôi muốn ăn sushi ngon.", new[] { "すしが", "たべたいです" }),
                new ShItem("ja-l53-sh-4", "medium", "Đi đâu để làm gì (Vừa)", "デパートへ くつを かいに いきます。", "デパート へ くつ を かい に いきます / depaato e kutsu o kai ni ikimasu", "Tôi đi trung tâm thương mại để mua giày.", new[] { "かいに", "いきます" }),
                new ShItem("ja-l53-sh-5", "medium", "Đi ngắm hoa anh đào (Vừa)", "こうえんへ さくらを みに いきましょう。", "こうえん へ さくら を み に いきましょう / kouen e sakura o mi ni ikimashou", "Chúng ta ra công viên ngắm hoa anh đào nhé.", new[] { "みに", "いきましょう" }),
                new ShItem("ja-l53-sh-6", "medium", "Đặt phòng khách sạn (Vừa)", "きょうとの ホテルを よやくしたいです。", "きょうと の ホテル を よやく したい です / Kyouto no hoteru o yoyaku shitai desu", "Tôi muốn đặt phòng khách sạn ở Kyoto.", new[] { "ホテルを", "よやくしたい" }),
                new ShItem("ja-l53-sh-7", "hard", "Đói bụng nên muốn ăn (Khó)", "おなかが すきましたから、ラーメンが たべたいです。", "おなか が すきました から、ラーメン が たべたい です / onaka ga sukimashita kara, raamen ga tabetai desu", "Vì đói bụng rồi nên tôi muốn ăn mì ramen.", new[] { "おなかがすきました", "たべたいです" }),
                new ShItem("ja-l53-sh-8", "hard", "Mục đích du lịch (Khó)", "らいねん ともだちと きょうとへ りょこうに いきたいです。", "らいねん ともだち と きょうと へ りょこう に いきたい です / rainen tomodachi to Kyouto e ryokou ni ikitai desu", "Năm sau tôi muốn đi du lịch Kyoto cùng bạn.", new[] { "りょこうに", "いきたいです" }),
                new ShItem("ja-l53-sh-9", "hard", "Chào tan làm chuẩn công sở (Khó)", "おつかれさまでした。おさきに しつれいします。", "おつかれさま でした。おさき に しつれい します / otsukaresama deshita. osaki ni shitsurei shimasu", "Anh đã vất vả rồi ạ. Tôi xin phép về trước.", new[] { "おつかれさま", "おさきに" })
            },
            dictation: new[]
            {
                new DiItem("ja-l53-di-1", "easy", "Muốn uống trà xanh", "つめたいおちゃがのみたいです", "tsumetai ocha ga nomitai desu", "Tôi muốn uống trà lạnh.", new[] { "つめたい", "おちゃが", "のみたい", "です" }, new[] { "ほしい", "たべたい", "を" }),
                new DiItem("ja-l53-di-2", "easy", "Muốn có thời gian", "いまじかんがほしいです", "ima jikan ga hoshii desu", "Bây giờ tôi muốn có thời gian.", new[] { "いま", "じかんが", "ほしい", "です" }, new[] { "したい", "たべたい", "を" }),
                new DiItem("ja-l53-di-3", "medium", "Đi Nhật để học", "にほんへべんきょうにきました", "Nihon e benkyou ni kimashita", "Tôi đã đến Nhật để học tập.", new[] { "にほんへ", "べんきょうに", "きました" }, new[] { "べんきょうして", "いきます", "で" }),
                new DiItem("ja-l53-di-4", "medium", "Mua quà cho gia đình", "かぞくにおみやげをかいたいです", "kazoku ni omiyage o kaitai desu", "Tôi muốn mua quà lưu niệm cho gia đình.", new[] { "かぞくに", "おみやげを", "かいたい", "です" }, new[] { "ほしい", "かって", "を" }),
                new DiItem("ja-l53-di-5", "hard", "Không muốn đi đâu cả", "つかれましたからどこへもいきたくないです", "tsukaremashita kara doko e mo ikitakunai desu", "Vì mệt rồi nên tôi không muốn đi đâu cả.", new[] { "つかれましたから", "どこへも", "いきたくないです" }, new[] { "いきたいです", "ほしい", "どこに" }),
                new DiItem("ja-l53-di-6", "hard", "Tương lai làm việc ở Nhật", "しょうらいにほんでエンジニアとしてはたらきたいです", "shourai Nihon de enjinia toshite hatarakitai desu", "Tương lai tôi muốn làm việc như kỹ sư ở Nhật.", new[] { "しょうらい", "にほんで", "はたらきたい", "です" }, new[] { "はたらく", "ほしい", "きのう" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l53-pr-1", "easy", "Trường âm りょこう", "にほんへりょこうにいきたいです", "Nihon e ryokou ni ikitai desu", "Tôi muốn đi du lịch Nhật Bản.", new[] { "りょこう", "いきたい" }, "Kéo dài こう đủ 2 phách và đọc nhẹ đuôi たいです.", new[] { "りょ gọn 1 nhịp", "こう kéo dài 2 nhịp", "いきたい đều giọng" }, "なつやすみににほんへりょこうにいきたいです。", new[] { "りょこう", "いきたい" }),
                new PrItem("ja-l53-pr-2", "easy", "Từ ほしい (muốn có)", "カメラがほしいです", "kamera ga hoshii desu", "Tôi muốn có máy ảnh.", new[] { "カメラ", "ほしい" }, "Kéo dài âm しい đủ 2 phách.", new[] { "カ-メ-ラ rõ 3 âm", "ほ nhẹ", "しい kéo dài" }, "あたらしいカメラがほしいです。", new[] { "あたらしい", "ほしい" }),
                new PrItem("ja-l53-pr-3", "medium", "Từ vựng おみやげ", "おみやげをかいにいきます", "omiyage o kai ni ikimasu", "Tôi đi mua quà lưu niệm.", new[] { "おみやげ", "かいに" }, "Đọc rõ âm đục げ (không đọc thành ghê cứng) và nối mượt かいに.", new[] { "お-み-や-げ đều 4 âm", "か-い-に rõ âm i", "いきます nhẹ nhàng" }, "デパートへおみやげをかいにいきます。", new[] { "おみやげ", "かいに" }),
                new PrItem("ja-l53-pr-4", "medium", "Trường âm しょうらい・おんせん", "おんせんにはいりたいです", "onsen ni hairitai desu", "Tôi muốn tắm suối nước nóng.", new[] { "おんせん", "はいりたい" }, "Giữ âm mũi ん ở cả おん và せん.", new[] { "おん ngân mũi nhẹ", "せん âm mũi rõ", "はいりたい nhấn nhẹ い" }, "ふゆにおんせんにはいりたいです。", new[] { "おんせん", "はいりたい" }),
                new PrItem("ja-l53-pr-5", "hard", "Âm ngắt しゅっちょう", "おおさかへしゅっちょうします", "Oosaka e shucchou shimasu", "Tôi đi công tác Osaka.", new[] { "おおさか", "しゅっちょう" }, "Kéo dài おお và giữ âm ngắt っ trước trường âm ちょう.", new[] { "おお kéo dài 2 phách", "しゅっ nén hơi 1 nhịp", "ちょう kéo dài 2 phách" }, "らいしゅうおおさかへしゅっちょうします。", new[] { "おおさか", "しゅっちょう" }),
                new PrItem("ja-l53-pr-6", "hard", "Chào công sở おさきに", "おさきにしつれいします", "osaki ni shitsurei shimasu", "Tôi xin phép về trước ạ.", new[] { "おさきに", "しつれい" }, "Đọc trôi chảy しつれい (tạo âm れい dài tự nhiên).", new[] { "お-さ-き-に đều nhịp", "しつ nhẹ gọn", "れい kéo dài tự nhiên" }, "おつかれさまでした。おさきにしつれいします。", new[] { "おつかれさま", "しつれいします" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l53-wm-1", "easy", "Sơ đồ từ vựng: 〜たいです (Ý muốn hành động)", "〜たいです", "~tai desu", "Muốn làm gì đó",
                    new[] { new WmNode("いきたいです", "ikitai desu", "Muốn đi", "きょうとへ いきたいです。"), new WmNode("たべたいです", "tabetai desu", "Muốn ăn", "おいしい すしが たべたいです。") },
                    new[] { new WmNode("〜たくないです", "~takunai desu", "Không muốn làm...", "なにも したくないです。") },
                    new WmNode[] { new WmNode("〜が ほしいです", "~ga hoshii desu", "Muốn có (vật)", "くるまが ほしいです。") },
                    new[] { new WmEx("おなかが すきましたから、なにか たべたいです。", "onaka ga sukimashita kara, nanika tabetai desu", "Vì đói bụng rồi nên tôi muốn ăn cái gì đó.") }),
                new WmItem("ja-l53-wm-2", "medium", "Sơ đồ từ vựng: 〜に いきます (Mục đích)", "〜に いきます", "~ni ikimasu", "Đi đâu để làm gì",
                    new[] { new WmNode("かいに いきます", "kai ni ikimasu", "Đi mua", "スーパーへ ぎゅうにゅうを かいに いきます。"), new WmNode("みに いきます", "mi ni ikimasu", "Đi xem/ngắm", "さくらを みに いきます。") },
                    new[] { new WmNode("りょこうに いきます", "ryokou ni ikimasu", "Đi du lịch", "ふじさんへ りょこうに いきます。") },
                    new WmNode[] { new WmNode("べんきょうに きます", "benkyou ni kimasu", "Đến học", "にほんへ べんきょうに きました。") },
                    new[] { new WmEx("しゅうまつに ともだちと えいがを みに いきます。", "shuumatsu ni tomodachi to eiga o mi ni ikimasu", "Cuối tuần tôi đi xem phim cùng bạn.") }),
                new WmItem("ja-l53-wm-3", "hard", "Sơ đồ từ vựng: よやく / しゅっちょう (Du lịch & Công sở)", "よやく・しゅっちょう", "yoyaku / shucchou", "Đặt trước / Công tác",
                    new[] { new WmNode("ホテルを よやくします", "hoteru o yoyaku shimasu", "Đặt phòng khách sạn", "ネットで ホテルを よやくしたいです。"), new WmNode("しゅっちょうします", "shucchou shimasu", "Đi công tác", "とうきょうへ しゅっちょうします。") },
                    new[] { new WmNode("おさきに しつれいします", "osaki ni shitsurei shimasu", "Xin phép về trước", "おつかれさま。おさきに しつれいします。") },
                    new WmNode[] { new WmNode("おみやげ", "omiyage", "Quà lưu niệm", "おみやげを かいます。") },
                    new[] { new WmEx("らいげつ とうきょうへ しゅっちょうしますから、ひこうきを よやくします。", "raigetsu Toukyou e shucchou shimasu kara, hikouki o yoyaku shimasu", "Vì tháng sau tôi đi công tác Tokyo nên tôi đặt vé máy bay.") })
            },
            cloze: new[]
            {
                new ClItem("ja-l53-cl-1", "easy", "Trợ từ đi với ほしい", "Điền trợ từ đúng trước ほしいです:", "あたらしい くるま{{0}} ほしいです。", new[] { new ClBlank(new[] { "が" }, "trợ từ đi với ほしい", "ほしい luôn dùng trợ từ が (không dùng を).") }, "あたらしい くるまが ほしいです。", "atarashii kuruma ga hoshii desu", "Tôi muốn có một chiếc ô tô mới.", "N + が + ほしいです."),
                new ClItem("ja-l53-cl-2", "easy", "Mẫu ý muốn 〜たいです", "Chia động từ のみます (uống) sang dạng 'muốn uống':", "のどが かわきましたから、みずを {{0}}。", new[] { new ClBlank(new[] { "のみたいです", "飲みたいです" }, "muốn uống (nomitai desu)", "Bỏ ます thêm たいです.") }, "のどが かわきましたから、みずを のみたいです。", "nodo ga kawakimashita kara, mizu o nomitai desu", "Vì khát nước nên tôi muốn uống nước.", "のみます → のみたい."),
                new ClItem("ja-l53-cl-3", "medium", "Mẫu mục đích đi làm gì", "Điền dạng động từ trước に いきます để nói 'Đi mua giày':", "デパートへ くつを {{0}}に いきます。", new[] { new ClBlank(new[] { "かい", "買い" }, "gốc của かいます", "Mục đích hành động dùng Động từ bỏ ます + に + いきます.") }, "デパートへ くつを かいに いきます。", "depaato e kutsu o kai ni ikimasu", "Tôi đi trung tâm thương mại để mua giày.", "Không dùng thể từ điển かう hay Thể Te かって."),
                new ClItem("ja-l53-cl-4", "hard", "Phủ định của 〜たいです", "Chia phủ định cho 'Không muốn làm gì cả':", "つかれましたから、なにも {{0}}。", new[] { new ClBlank(new[] { "したくないです" }, "không muốn làm (shitakunai desu)", "します → したい → phủ định たくない: したくないです.") }, "つかれましたから、なにも したくないです。", "tsukaremashita kara, nanimo shitakunai desu", "Vì mệt rồi nên tôi không muốn làm gì cả.", "〜たい chia như tính từ đuôi -i.")
            },
            translate: new[]
            {
                new TrItem("ja-l53-tr-1", "easy", "Muốn đi du lịch", "Tôi muốn đi du lịch Nhật Bản.", new[] { "にほん", "りょこう", "いきたい" }, new[] { "にほんへりょこうにいきたいです。", "日本へ旅行に行きたいです。", "わたしはにほんへりょこうにいきたいです。" }, "日本へ旅行に行きたいです。", "Nihon e ryokou ni ikitai desu", "Kết hợp [Nơi] へ + [Mục đích] に + いきたいです."),
                new TrItem("ja-l53-tr-2", "easy", "Muốn có máy ảnh mới", "Tôi muốn có một chiếc máy ảnh mới.", new[] { "あたらしい", "カメラ", "ほしい" }, new[] { "あたらしいカメラがほしいです。", "新しいカメラが欲しいです。" }, "新しいカメラが欲しいです。", "atarashii kamera ga hoshii desu", "N が ほしいです = Muốn có N."),
                new TrItem("ja-l53-tr-3", "medium", "Đi ngắm hoa anh đào", "Tôi ra công viên để ngắm hoa anh đào.", new[] { "こうえん", "さくら", "みに", "いきます" }, new[] { "こうえんへさくらをみにいきます。", "公園へ桜を見に行きます。" }, "公園へ桜を見に行きます。", "kouen e sakura o mi ni ikimasu", "みます bỏ ます → み + に + いきます."),
                new TrItem("ja-l53-tr-4", "hard", "Đặt phòng khách sạn", "Tôi muốn đặt một phòng khách sạn ở Kyoto.", new[] { "きょうと", "ホテル", "よやくしたい" }, new[] { "きょうとのホテルをよやくしたいです。", "京都のホテルを予約したいです。" }, "京都のホテルを予約したいです。", "Kyouto no hoteru o yoyaku shitai desu", "よやくします → よやくしたいです.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l53-gf-1", "easy", "Dùng を thay が trước ほしい", "わたしは いぬを ほしいです。", "ほしい luôn đi với trợ từ が.", "わたしは いぬが ほしいです。", new[] { "わたしは いぬが ほしいです。", "私は犬が欲しいです。" }, "watashi wa inu ga hoshii desu", "Tôi muốn có một chú chó.", "N が ほしいです."),
                new GfItem("ja-l53-gf-2", "medium", "Dùng thể từ điển みる trước に いきます", "えいがを みるに いきます。", "Trước に いきます phải bỏ ます (không dùng thể từ điển みる).", "えいがを みに いきます。", new[] { "えいがを みに いきます。", "映画を見に行きます。" }, "eiga o mi ni ikimasu", "Tôi đi xem phim.", "みます → み + に + いきます."),
                new GfItem("ja-l53-gf-3", "hard", "Phủ định sai たいではありません", "きょうは なにも たべたいではありません。", "〜たい chia như tính từ đuôi -i, nên phủ định là 〜たくないです.", "きょうは なにも たべたくないです。", new[] { "きょうは なにも たべたくないです。", "今日は何も食べたくないです。" }, "kyou wa nanimo tabetakunai desu", "Hôm nay tôi không muốn ăn gì cả.", "〜たい → 〜たくないです.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l53-rp-1", "easy", "Lên kế hoạch cuối tuần", "Bạn Nhật hỏi cuối tuần này bạn muốn đi đâu và làm gì.", "Bạn Yuki", "こんしゅうの しゅうまつは どこへ いきたいですか。", "konshuu no shuumatsu wa doko e ikitai desu ka?", "Cuối tuần này bạn muốn đi đâu thế?", "Trả lời nơi muốn đi và việc muốn làm dùng mẫu 〜たいです hoặc 〜に いきたいです.", new[] { "たいです", "へ|に", "いき|み|たべ|かい|りょこう" }, new[] { "ともだちと きょうとへ りょこうに いきたいです。おんせんに はいりたいです。" }, "tomodachi to Kyouto e ryokou ni ikitai desu. onsen ni hairitai desu.", 8),
                new RpItem("ja-l53-rp-2", "medium", "Đặt phòng khách sạn du lịch", "Bạn gọi điện đến quầy lễ tân khách sạn ở Nhật để đặt phòng.", "Nhân viên lễ tân", "お電話ありがとうございます。さくらホテルでございます。", "odenwa arigatou gozaimasu. Sakura Hoteru de gozaimasu.", "Cảm ơn quý khách đã gọi. Đây là khách sạn Sakura ạ.", "Nói bạn muốn đặt phòng (よやくしたい) hoặc nhờ giúp đỡ lịch sự (おねがいします).", new[] { "よやく|予約", "したい|おねがいします", "ホテル|へや|部屋|あした" }, new[] { "すみません、あしたの へやを よやくしたいんですが、おねがいします。" }, "sumimasen, ashita no heya o yoyaku shitai n desu ga, onegaishimasu.", 8),
                new RpItem("ja-l53-rp-3", "hard", "Chào tạm biệt cuối ngày tại công ty", "Hết giờ làm việc, sếp nói 'Otsukaresama deshita' và hỏi bạn về ước mơ công việc sắp tới.", "Anh Suzuki", "ミンさん、きょうも おつかれさまでした！しょうらい にほんで なにを したいですか。", "Min-san, kyou mo otsukaresama deshita! shourai Nihon de nani o shitai desu ka?", "Minh hôm nay cũng vất vả rồi! Tương lai em muốn làm gì ở Nhật?", "Chia sẻ dự định bằng 〜たいです và nói câu chào tan làm chuẩn công sở (おさきに しつれいします).", new[] { "たいです", "はたらき|エンジニア|しごと|べんきょう", "おさきに|しつれいします|おつかれさま" }, new[] { "しょうらい にほんで エンジニアとして はたらきたいです。おつかれさまでした、おさきに しつれいします！" }, "shourai Nihon de enjinia toshite hatarakitai desu. otsukaresama deshita, osaki ni shitsurei shimasu!", 10)
            }, now));

        return all;
    }

    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG SƠ CẤP (HSK1+) — 6 BÀI LÝ THUYẾT CHƯƠNG 4 & 5 (>= 5.500 KÝ TỰ/BÀI)
    // =========================================================================

    private static List<CjkLessonMeta> GetChineseExpandedLessons() => new()
    {
        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, ChineseChapter4Id, ChineseLesson41Id,
            "Bài 4.1: Thời Tiết, Bốn Mùa & Cấu Trúc Nhấn Mạnh Thái/Rất (太...了 / 很 / 非常)",
            "zh-hsk1-thoi-tiet-bon-mua-tai-le-feichang", 1, 40,
            @"# Bài 4.1: Thời Tiết, Bốn Mùa & Cấu Trúc Nhấn Mạnh Thái/Rất (太...了 / 很 / 非常)

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Bước sang Chương 4 của lộ trình HSK1 nâng cao, bạn sẽ làm chủ chủ đề giao tiếp mở đầu kinh điển nhất của người Trung Quốc — **hỏi thăm thời tiết, bốn mùa và bày tỏ cảm xúc trước sự vật, hiện tượng**. Sau bài học này, bạn sẽ:
- Nắm trọn bộ từ vựng **thời tiết và bốn mùa**: `天气` (thời tiết), `冷` (lạnh), `热` (nóng), `下雨` (mưa), `下雪` (tuyết rơi), `春天 / 夏天 / 秋天 / 冬天` (Xuân / Hạ / Thu / Đông).
- Sử dụng chuẩn xác **Đại từ nghi vấn `怎么样 (zěnmeyàng)`** *(như thế nào?)* để hỏi thăm thời tiết, sức khỏe, món ăn hay một bộ phim.
- Làm chủ **Thang đo Phó từ Mức độ** từ nhẹ đến cực đại: `有点儿` *(hơi)* → `很` *(rất/bình thường)* → `非常 / 真` *(vô cùng/thật sự)* → `太...了` *(quá... rồi!)*.
- Hiểu rõ sự khác biệt sinh tử giữa **`有点儿 + Tính từ`** *(hơi... — mang ý chê nhẹ)* và **`一点儿 + Danh từ`** *(một chút gì đó)*.
- Phân biệt rạch ròi cách nói hiện tượng thời tiết **đang xảy ra** (`今天下雨了` / `在下雨`) và **dự đoán tương lai** (`明天可能会下雨`).

---

## 2. 🌱 Nhập môn chậm rãi từng bước: Hỏi thời tiết & Thang đo mức độ cảm xúc

### 2.1. Đại từ nghi vấn `怎么样 (zěnmeyàng)` — ""Như thế nào?""
Trong tiếng Trung, muốn hỏi nhận xét hoặc tình hình của một sự vật, thời tiết hay công việc, chỉ cần đặt `怎么样` vào **cuối câu**:
- **Công thức**: `[Chủ đề] + 怎么样？`
- **Ví dụ**:
  - `今天天气怎么样？` *(Thời tiết hôm nay thế nào?)*
  - `你最近身体怎么样？` *(Dạo này sức khỏe bạn thế nào?)*
  - `那个电影怎么样？` *(Bộ phim đó thế nào, hay không?)*
- **Cẩn thận bẫy sai**: Không ghép `怎么样` với tính từ (tức là KHÔNG nói `今天怎么样好?`); `怎么样` đứng một mình thay cho phần vị ngữ!

### 2.2. Thang đo Phó từ Mức độ & Cảm thán chuẩn HSK1
Người Việt học tiếng Trung rất hay lạm dụng mỗi chữ `很` cho mọi tình huống. Hãy nâng cấp khả năng biểu đạt cảm xúc bằng 5 cấp độ tự nhiên sau:

1. **`太 + [Tính từ] + 了！` (Quá ... rồi!)**: Biểu thị mức độ cao vượt kỳ vọng hoặc cảm xúc mạnh (khen hoặc than vãn).
   - Khen ngợi: `今天天气太好了！` *(Thời tiết hôm nay đẹp quá đi mất!)*
   - Than vãn: `今天太热了！` *(Hôm nay nóng quá rồi!)*
   - **Phủ định của `太...了`**: Dùng **`不太 + Tính từ`** *(Không ... lắm)* — **LƯU Ý**: Khi phủ định KHÔNG thêm `了` ở cuối! (`今天不太冷。` *Hôm nay không lạnh lắm*).
2. **`非常 (fēicháng)` (Vô cùng / Rất)**: Mức độ mạnh hơn `很`, trang trọng và nhấn mạnh hơn:
   - `这里的冬天非常冷。` *(Mùa đông ở đây vô cùng lạnh.)*
3. **`真 (zhēn)` (Thật là)**: Dùng trong câu cảm thán trực tiếp từ trái tim:
   - `春天真漂亮！` *(Mùa xuân thật là đẹp!)*
4. **`很 (hěn)` (Rất / Cân bằng âm tiết)**: Dùng trong câu trần thuật bình thường (`我很忙`, `今天很热`).
5. **`有点儿 (yǒudiǎnr)` (Hơi ... một chút)**: Đặt **TRƯỚC Tính từ**, thường biểu thị điều gì đó **chưa được như ý muốn**:
   - `今天有点儿冷。` *(Hôm nay trời hơi lạnh.)*
   - `这件衣服有点儿贵。` *(Chiếc áo này hơi đắt một chút.)*

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbiǎo)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **天气** | tiānqì | Thiên Khí | Thời tiết | 北京的天气怎么样？ *(Thời tiết Bắc Kinh thế nào?)* |
| **怎么样** | zěnmeyàng | Chẩm Ma Dạng | Như thế nào, ra sao | 你觉得这个菜怎么样？ *(Bạn thấy món này thế nào?)* |
| **热 / 冷** | rè / lěng | Nhiệt / Lãnh | Nóng / Lạnh | 夏天很热，冬天很冷。 *(Mùa hè rất nóng, mùa đông rất lạnh.)* |
| **暖和 / 凉快** | nuǎnhuo / liángkuai | Noãn Hòa / Lương Khoái | Ấm áp / Mát mẻ | 秋天不冷也不热，很凉快。 *(Mùa thu không lạnh cũng không nóng, rất mát.)* |
| **下雨** | xià yǔ | Hạ Vũ | Trời mưa | 今天下午下雨了。 *(Chiều nay trời mưa rồi.)* |
| **下雪** | xià xuě | Hạ Tuyết | Trời có tuyết rơi | 北京的冬天经常下雪。 *(Mùa đông Bắc Kinh hay có tuyết rơi.)* |
| **晴天 / 阴天** | qíngtiān / yīntiān | Tình Thiên / Âm Thiên | Trời nắng / Trời râm | 明天是晴天，我们去公园吧！ *(Mai trời nắng, tụi mình đi công viên nhé!)* |
| **春天 / 夏天** | chūntiān / xiàtiān | Xuân Thiên / Hạ Thiên | Mùa xuân / Mùa hè | 我最喜欢春天，花很多。 *(Tôi thích mùa xuân nhất, có nhiều hoa.)* |
| **秋天 / 冬天** | qiūtiān / dōngtiān | Thu Thiên / Đông Thiên | Mùa thu / Mùa đông | 河内的秋天非常舒服。 *(Mùa thu Hà Nội vô cùng dễ chịu.)* |
| **太...了** | tài... le | Thái ... Liễu | Quá ... rồi! | 太好了！我们明天一起去。 *(Tuyệt quá! Mai tụi mình cùng đi.)* |
| **非常** | fēicháng | Phi Thường | Vô cùng, rất | 我非常喜欢学汉语。 *(Tôi vô cùng thích học tiếng Hán.)* |
| **有点儿** | yǒudiǎnr | Hữu Điểm Nhi | Hơi, có chút (+ tính từ) | 今天风有点儿大。 *(Hôm nay gió hơi to một chút.)* |
| **舒服** | shūfu | Thư Phục | Thoải mái, dễ chịu | 今天天气真舒服！ *(Thời tiết hôm nay thật dễ chịu!)* |
| **刮风** | guā fēng | Quát Phong | Thổi gió, có gió | 外面刮风了，多穿点儿衣服。 *(Ngoài trời nổi gió rồi, mặc thêm áo đi.)* |
| **雨伞** | yǔsǎn | Vũ Tán | Cái ô (che mưa) | 下雨了，别忘了带雨伞。 *(Mưa rồi, đừng quên mang ô nhé.)* |

---

## 4. Ngữ pháp mổ xẻ: `有点儿` vs `一点儿` & Động từ ly hợp `下雨`

### 4.1. Phân biệt cặp song sinh hay nhầm nhất: `有点儿` vs `一点儿`
| Tiêu chí | `有点儿 (yǒudiǎnr)` | `一点儿 (yìdiǎnr)` |
|---|---|---|
| **Vị trí đứng** | Đứng **TRƯỚC Tính từ / Động từ tâm lý** | Đứng **SAU Động từ** hoặc **TRƯỚC Danh từ** |
| **Sắc thái ý nghĩa** | ""Hơi..."" (thường mang ý chê, than phiền nhẹ) | ""Một chút..."" (trung tính, dùng khi gọi đồ, so sánh) |
| **Ví dụ chuẩn** | `今天有点儿冷。` *(Hôm nay hơi lạnh.)* | `请给我一点儿水。` *(Cho tôi một chút nước.)* |
| **Ví dụ hay sai** | ❌ `我有一点儿冷` (thường dùng `有点儿`) | ❌ `有点儿水` (không đứng trước danh từ chỉ lượng) |

### 4.2. Bản chất Động từ Ly Hợp `下雨` (mưa) và `下雪` (tuyết rơi)
- Trong tiếng Việt ta nói *""Trời đang mưa""* (như một tính từ/động từ đơn), nhưng trong tiếng Trung **`下雨`** gồm động từ **`下`** (rơi xuống) + tân ngữ **`雨`** (mưa)!
- Vì vậy:
  - Khi muốn nói ""Mưa to"": Chèn tính từ `大` vào giữa: **`下大雨`** *(mưa to)*, **`下小雨`** *(mưa nhỏ)* — **KHÔNG nói `下雨很大`**!
  - Khi nói ""Đang mưa"": `在下雨呢` hoặc `正在下雨`.
  - Khi nói ""Mưa rồi"": `下雨了`.

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 今天不太热了。 (Giữ chữ 了 khi phủ định mẫu 太...了)
   ⭕ ĐÚNG: 今天不太热。 (Phủ định 不太 + Tính từ KHÔNG có 了 ở cuối)
   💡 GIẢI THÍCH: Chữ 了 trong 太...了 chỉ đi với câu khẳng định cảm thán; khi đổi sang 不太 thì bỏ 了!

2. ❌ SAI: 今天一点儿冷 / 我想喝有点儿水。 (Đổi nhầm vị trí 有点儿 và 一点儿)
   ⭕ ĐÚNG: 今天有点儿冷。 / 我想喝一点儿水。
   💡 GIẢI THÍCH: 有点儿 + Tính từ; Động từ + 一点儿 + Danh từ.

3. ❌ SAI: 今天下雨很大。 (Đặt 很大 sau tân ngữ 雨)
   ⭕ ĐÚNG: 今天下大雨了。 (Chèn 大 vào giữa 下 và 雨)
   💡 GIẢI THÍCH: 下雨 là động từ ly hợp (Động từ 下 + Tân ngữ 雨), nên tính từ bổ nghĩa cho mưa phải chèn vào giữa!

4. ❌ SAI: 我是很冷 / 今天是太热了。 (Chèn động từ 是 trước tính từ)
   ⭕ ĐÚNG: 我很冷。 / 今天太热了。
   💡 GIẢI THÍCH: Câu vị ngữ tính từ tiếng Trung TUYỆT ĐỐI không dùng động từ 是.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Buổi sáng cuối tuần tại ký túc xá Bắc Kinh, Minh gọi điện hỏi bạn Trương Vĩ về thời tiết ngoài trời trước khi ra phố đi chơi.

- **Minh**: 张伟，今天北京的天气怎么样？还冷吗？
  - *Pinyin*: Zhāng Wěi, jīntiān Běijīng de tiānqì zěnmeyàng? Hái lěng ma?
  - *Phân tích*: `天气怎么样` (thời tiết thế nào?) + `还冷吗` (vẫn lạnh chứ?).
  - *Dịch*: Trương Vĩ ơi, hôm nay thời tiết Bắc Kinh thế nào? Vẫn lạnh à?

- **Zhang Wei**: 不太冷了，今天是晴天，非常舒服！
  - *Pinyin*: Bú tài lěng le, jīntiān shì qíngtiān, fēicháng shūfu!
  - *Phân tích*: `不太冷` (không lạnh lắm — thêm `了` ở đây biểu thị sự thay đổi trạng thái từ lạnh sang hết lạnh) + `非常舒服` (vô cùng dễ chịu).
  - *Dịch*: Hết lạnh lắm rồi, hôm nay trời nắng đẹp, vô cùng dễ chịu luôn!

- **Minh**: 太好了！河内今天有点儿热，下午可能会下大雨。
  - *Pinyin*: Tài hǎo le! Hénèi jīntiān yǒudiǎnr rè, xiàwǔ kěnéng huì xià dàyǔ.
  - *Phân tích*: `太好了` (tuyệt quá!) + `有点儿热` (hơi nóng) + `下大雨` (mưa to).
  - *Dịch*: Tuyệt quá! Hà Nội hôm nay hơi nóng, chiều nay có khi sẽ mưa to đấy.

- **Zhang Wei**: 那你出门的时候别忘了带雨伞啊！你最喜欢哪个季节？
  - *Pinyin*: Nà nǐ chūmén de shíhou bié wàng le dài yǔsǎn a! Nǐ zuì xǐhuan nǎge jìjié?
  - *Phân tích*: `带雨伞` (mang ô) + `哪个季节` (mùa nào?).
  - *Dịch*: Vậy lúc ra ngoài cậu đừng quên mang theo ô nhé! Cậu thích mùa nào nhất?

- **Minh**: 我最喜欢秋天，因为秋天不冷也不热，非常凉快。
  - *Pinyin*: Wǒ zuì xǐhuan qiūtiān, yīnwèi qiūtiān bù lěng yě bú rè, fēicháng liángkuai.
  - *Phân tích*: `不冷也不热` (không lạnh cũng không nóng) + `凉快` (mát mẻ).
  - *Dịch*: Mình thích mùa thu nhất, vì mùa thu không lạnh cũng chẳng nóng, rất mát mẻ.

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện từ vựng thời tiết & cấu trúc mức độ
1. Điền `有点儿` hoặc `一点儿`:
   - `今天风 ___ 大，我 ___ 累了。`
   - `请给我 ___ 热茶吧。`
2. Chuyển câu sau sang dạng phủ định hợp lý: `今天太冷了。`
3. Sửa lỗi sai trong câu: `昨天北京下雪很大。`

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - Câu đầu điền **`有点儿`** ở cả hai chỗ (`有点儿大`, `有点儿累` — đứng trước tính từ).
   - Câu sau điền **`一点儿`** (`一点儿热茶` — đứng trước danh từ).
2. **Câu 2**: `今天不太冷。` *(Bỏ `了` khi phủ định trạng thái cực đoan `太...了`).*
3. **Câu 3**: `昨天北京下大雪了。` *(Tính từ `大` chèn vào giữa `下` và `雪`).*
4. **Câu 4 trắc nghiệm**: Để hỏi ""Thời tiết ngày mai thế nào?"", câu đúng là:
   - *Đáp án*: `明天天气怎么样？`
5. **Câu 5 trắc nghiệm**: Trong các từ sau, từ nào diễn đạt mùa thu mát mẻ?
   - *Đáp án*: `秋天很凉快` (qiūtiān hěn liángkuai).

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Nhấn ngay vào **Phòng Thực Hành (Shadowing / Cloze / Grammar Fix)** bên dưới để luyện phân biệt `有点儿` vs `一点儿` và cảm thán `太...了` chuẩn giọng Bắc Kinh (zh-CN)!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Làm chủ từ vựng thời tiết 4 mùa, mẫu hỏi 怎么样 và thang đo mức độ cảm xúc (有点儿/很/非常/太...了).",
            "Phân biệt chuẩn xác 有点儿 vs 一点儿 và cấu trúc động từ ly hợp 下雨 / 下大雨."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, ChineseChapter4Id, ChineseLesson42Id,
            "Bài 4.2: Gia Đình, Lượng Từ Thực Chiến (口, 个, 本, 张) & Sở Hữu Với 的",
            "zh-hsk1-gia-dinh-luong-tu-so-huu-de", 2, 40,
            @"# Bài 4.2: Gia Đình, Lượng Từ Thực Chiến (口, 个, 本, 张) & Sở Hữu Với 的

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Khác với tiếng Việt có thể nói *""hai sách""* hoặc *""bố tôi""* không cần từ nối, tiếng Trung bắt buộc phải có **Lượng từ (量词 — Liàngcí)** đứng giữa số đếm và danh từ, đồng thời có quy tắc tinh tế khi dùng hoặc lược bỏ **trợ từ kết cấu `的 (de)`** trong quan hệ gia đình và sở hữu. Sau bài học này, bạn sẽ:
- Mở rộng đầy đủ từ vựng **gia đình đa thế hệ** (`爷爷`, `奶奶`, `爸爸`, `妈妈`, `哥哥`, `姐姐`, `弟弟`, `妹妹`, `儿子`, `女儿`).
- Nắm chắc **6 lượng từ thực chiến nhất cấp độ HSK1–HSK2**: `口` (nhân khẩu), `个` (vạn năng/người/hoa quả), `本` (sách/vở), `张` (bàn/giấy/ảnh/vé/giường), `件` (quần áo), `只` (mèo/chó nhỏ).
- Phân biệt rõ khi nào dùng **`二 (èr)`** vs **`两 (liǎng)`** khi ghép cùng lượng từ và trong số nhiều chữ số.
- Hiểu đúng quy luật dùng và lược bỏ **`的 (de)`**: Khi nào nói `我爸爸` (thân thiết) và khi nào bắt buộc dùng `我的书` (sở hữu đồ vật).
- Sử dụng thành thạo mẫu câu hỏi số lượng trên dưới 10 bằng **`几 (jǐ)`** vs trên 10 bằng **`多少 (duōshao)`**.

---

## 2. 🌱 Nhập môn chậm rãi từng bước: Hệ thống Lượng từ & Quy tắc `二 / 两`

### 2.1. Quy tắc vàng ghép Số từ + Lượng từ + Danh từ
Công thức bất di bất dịch trong tiếng Trung:
$$\text{Số từ / Đại từ chỉ thị (这, 那, 几)} + \mathbf{\text{Lượng từ}} + \text{Danh từ}$$
- ĐÚNG: `三本书` *(3 quyển sách)*, `这张桌子` *(cái bàn này)*, `几口人？` *(mấy người?)*.
- SAI: ❌ `三书` *(thiếu lượng từ)*, ❌ `这桌子` *(thiếu lượng từ giữa 这 và 桌子)*.

### 2.2. Bảng 6 Lượng từ ""sống còn"" HSK1 thực chiến
| Lượng từ | Pinyin | Dùng cho nhóm danh từ nào? | Ví dụ chuẩn xác |
|---|---|---|---|
| **口** | kǒu | **Số thành viên trong gia đình** (nhân khẩu) | `我家有五口人。` *(Nhà tôi có 5 người.)* |
| **个** | gè | Người nói chung, hoa quả tròn, tháng, đồ vật chung *(lượng từ vạn năng)* | `三个学生` *(3 học sinh)* / `两个苹果` *(2 quả táo)* |
| **本** | běn | Vật đóng gáy thành quyển *(sách, vở, từ điển, tạp chí)* | `两本汉语书` *(2 quyển sách tiếng Hán)* |
| **张** | zhāng | Vật có mặt phẳng rộng/mỏng *(bàn, giường, giấy, ảnh, vé, thẻ)* | `一张照片` *(1 bức ảnh)* / `两张桌子` *(2 cái bàn)* |
| **件** | jiàn | Quần áo (phần thân trên), sự việc, hành lý | `一件衣服` *(1 chiếc áo)* |
| **只** | zhī | Động vật nhỏ *(mèo, chó, chim)* hoặc 1 chiếc trong đôi *(giày, tất)* | `两只小猫` *(2 chú mèo nhỏ)* |

### 2.3. Khi nào dùng `两 (liǎng)` và khi nào dùng `二 (èr)`?
Đây là lỗi kinh điển của 90% người mới học tiếng Trung:
1. **Khi đi TRƯỚC lượng từ** (đếm số lượng = 2): **BẮT BUỘC dùng `两`**, tuyệt đối không dùng `二`!
   - `两个人` *(2 người)*, `两本书` *(2 quyển sách)*, `两点` *(2 giờ)*, `两张票` *(2 tấm vé)*.
2. **Khi đếm số thứ tự, số phòng, số điện thoại, số thập phân, hàng đơn vị/chục**: Dùng **`二`**:
   - `一、二、三` *(1, 2, 3)*, `星期二` *(Thứ Ba)*, `二月` *(Tháng 2)*, `十二个人` *(12 người — hàng đơn vị vẫn là 二!)*.

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbiǎo)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **爷爷 / 奶奶** | yéye / nǎinai | Gia Gia / Nãi Nãi | Ông nội / Bà nội | 爷爷奶奶都七十多岁了。 *(Ông bà nội đều hơn 70 tuổi rồi.)* |
| **爸爸 / 妈妈** | bàba / māma | Ba Ba / Ma Ma | Bố / Mẹ | 我爸爸是医生，妈妈是老师。 *(Bố tôi là bác sĩ, mẹ là giáo viên.)* |
| **哥哥 / 姐姐** | gēge / jiějie | Ca Ca / Tỷ Tỷ | Anh trai / Chị gái | 我有一个姐姐，没有哥哥。 *(Tôi có 1 chị gái, không có anh trai.)* |
| **弟弟 / 妹妹** | dìdi / mèimei | Đệ Đệ / Muội Muội | Em trai / Em gái | 妹妹今年十岁，上小学。 *(Em gái năm nay 10 tuổi, học tiểu học.)* |
| **儿子 / 女儿** | érzi / nǚ'ér | Nhi Tử / Nữ Nhi | Con trai / Con gái | 李老师有一个女儿。 *(Cô Lý có một cô con gái.)* |
| **张** | zhāng | Trương | Tờ, tấm, bức, chiếc (lượng từ) | 桌子上有三张照片。 *(Trên bàn có 3 bức ảnh.)* |
| **件** | jiàn | Kiện | Chiếc (áo), việc (lượng từ) | 这件衣服很好看。 *(Chiếc áo này rất đẹp.)* |
| **只** | zhī | Chích | Con (động vật nhỏ - lượng từ) | 我家里有两只小狗。 *(Nhà tôi có 2 chú cún con.)* |
| **照片** | zhàopiàn | Chiếu Phiến | Bức ảnh, tấm hình | 这是我们家的全家福照片。 *(Đây là ảnh đại gia đình nhà tôi.)* |
| **票** | piào | Phiếu | Vé (xe, tàu, máy bay, phim) | 我买了两张电影票。 *(Tôi đã mua 2 vé xem phim.)* |
| **衣服** | yīfu | Y Phục | Quần áo | 妈妈给我买了一件新衣服。 *(Mẹ mua cho tôi một chiếc áo mới.)* |
| **词典** | cídiǎn | Từ Điển | Cuốn từ điển | 我有一本汉越词典。 *(Tôi có một cuốn từ điển Hán-Việt.)* |
| **电脑 / 手机** | diànnǎo / shǒujī | Điện Não / Thủ Cơ | Máy tính / Điện thoại | 这是我哥哥的电脑。 *(Đây là máy tính của anh trai tôi.)* |
| **几 / 多少** | jǐ / duōshao | Kỷ / Đa Thiểu | Mấy (<=10) / Bao nhiêu | 你们班有多少个学生？ *(Lớp các bạn có bao nhiêu học sinh?)* |
| **和** | hé | Hòa | Và (chỉ nối Danh từ!) | 爸爸和妈妈都在家。 *(Bố và mẹ đều đang ở nhà.)* |

---

## 4. Ngữ pháp mổ xẻ: Quy tắc ẩn/hiện chữ `的` & Liên từ `和`

### 4.1. Khi nào giữ `的 (de)` và khi nào lược bỏ `的`?
- **Công thức sở hữu chuẩn**: **`[Chủ sở hữu] + 的 + [Danh từ]`**
  - `我的书` *(Sách của tôi)*, `老师的电脑` *(Máy tính của thầy giáo)*.
- **Khi nói về người thân ruột thịt hoặc tổ chức gắn bó thân thiết** (`我家`, `我爸爸`, `我们学校`): Người Trung Quốc **thường lược bỏ `的`** để nghe gần gũi tự nhiên:
  - `我妈妈非常喜欢喝茶。` *(Mẹ tôi rất thích uống trà.)* — tự nhiên hơn `我的妈妈...`.
  - `这是我哥哥的手机。` *(Đây là điện thoại của anh trai tôi.)* — chữ `的` đầu lược bỏ giữa `我` và `哥哥`, chữ `的` thứ hai GIỮ LẠI giữa `哥哥` và `手机`!
- **BẮT BUỘC giữ `的` khi danh từ bị sở hữu là đồ vật/tính chất** (`我的钱包`, `红的苹果`).

### 4.2. Cấm kỵ với liên từ `和 (hé - và)`
- **`和` chỉ dùng để nối hai DANH TỪ hoặc ĐẠI TỪ** (`爸爸和妈妈`, `我和你`, `苹果和香蕉`).
- **TUYỆT ĐỐI KHÔNG dùng `和` để nối hai mệnh đề/hai động từ** như tiếng Việt hay tiếng Anh (*""Tôi ăn cơm và xem tivi""*):
  - ❌ SAI: `我吃饭和看电视。`
  - ⭕ ĐÚNG: `我吃饭，然后看电视。` *(Hoặc dùng `一边...一边...` nếu làm cùng lúc, hoặc chỉ cần dấu phẩy).*

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 我家有二口人 / 我买了二本书。 (Dùng 二 trực tiếp trước lượng từ)
   ⭕ ĐÚNG: 我家有两口人。 / 我买了两本书。
   💡 GIẢI THÍCH: Trước mọi lượng từ (口, 个, 本, 张, 点...), số 2 BẮT BUỘC phải viết/đọc là 两 (liǎng)!

2. ❌ SAI: 桌子上有三张书和两本照片。 (Dùng nhầm lượng từ 张 cho sách và 本 cho ảnh)
   ⭕ ĐÚNG: 桌子上有三本书和两张照片。 (Sách dùng 本, ảnh phẳng dùng 张)
   💡 GIẢI THÍCH: 本 dùng cho vật đóng tập dày; 张 dùng cho vật phẳng mỏng như tờ giấy, bức ảnh, cái bàn, vé.

3. ❌ SAI: 他是我的朋友和我很喜欢他。 (Dùng 和 để nối hai câu hoàn chỉnh)
   ⭕ ĐÚNG: 他是我的好朋友，我也很喜欢他。 (Dùng dấu phẩy và 也/然后)
   💡 GIẢI THÍCH: 和 trong tiếng Trung chỉ nối danh từ/cụm danh từ, KHÔNG nối hai mệnh đề!

4. ❌ SAI: 我家没有几个猫。 (Dùng 几 trong câu phủ định có 没)
   ⭕ ĐÚNG: 我家没有猫。 / 我只有一只小猫。
   💡 GIẢI THÍCH: 几 dùng để hỏi số lượng ước chừng nhỏ hơn 10 (hoặc nghĩa ""vài"" trong câu khẳng định: 几本书).
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Lan đến nhà Minh chơi lần đầu, xem album ảnh gia đình đặt trên bàn phòng khách và hỏi thăm các thành viên.

- **Lan**: 明，这是你的全家福照片吗？真好看！你家有几口人？
  - *Pinyin*: Míng, zhè shì nǐ de quánjiāfú zhàopiàn ma? Zhēn hǎokàn! Nǐ jiā yǒu jǐ kǒu rén?
  - *Phân tích*: `这张照片` / `的` (sở hữu bức ảnh) + `几口人` (mấy nhân khẩu?).
  - *Dịch*: Minh ơi, đây là ảnh đại gia đình nhà cậu à? Đẹp thật đấy! Nhà cậu có mấy người?

- **Minh**: 我家有五口人：爷爷、奶奶、爸爸、妈妈和我。
  - *Pinyin*: Wǒ jiā yǒu wǔ kǒu rén: yéye, nǎinai, bàba, māma hé wǒ.
  - *Phân tích*: `五口人` (5 người) + `和` (nối danh từ cuối cùng).
  - *Dịch*: Nhà mình có 5 người: ông nội, bà nội, bố, mẹ và mình.

- **Lan**: 你没有哥哥或者姐姐吗？桌子上这两张票是谁的？
  - *Pinyin*: Nǐ méiyǒu gēge huòzhě jiějie ma? Zhuōzi shàng zhè liǎng zhāng piào shì shéi de?
  - *Phân tích*: `没有` (không có — phủ định của `有`) + `两张票` (2 tấm vé) + `谁的` (của ai?).
  - *Dịch*: Cậu không có anh trai hay chị gái à? Hai tấm vé trên bàn này là của ai thế?

- **Minh**: 这是我爸爸和妈妈的电影票。哦，你看，我们家还有两只小猫呢！
  - *Pinyin*: Zhè shì wǒ bàba hé māma de diànyǐng piào. Ò, nǐ kàn, wǒmen jiā hái yǒu liǎng zhī xiǎomāo ne!
  - *Phân tích*: `我爸爸和妈妈的` (lược `的` sau `我`, giữ `的` trước `电影票`) + `两只小猫` (2 chú mèo nhỏ).
  - *Dịch*: Đây là vé xem phim của bố mẹ mình đấy. À cậu nhìn xem, nhà mình còn có hai chú mèo con nữa cơ!

- **Lan**: 哇，两只小猫太可爱了！那本厚厚的书也是你的吗？
  - *Pinyin*: Wa, liǎng zhī xiǎomāo tài kě'ài le! Nà běn hòuhòu de shū yě shì nǐ de ma?
  - *Phân tích*: `太可爱了` (đáng yêu quá!) + `那本...书` (cuốn sách kia).
  - *Dịch*: Ôi, hai chú mèo đáng yêu quá đi! Cuốn sách dày cộp kia cũng là của cậu à?

- **Minh**: 不是，那是我爷爷的词典。我有一本汉越词典在书包里。
  - *Pinyin*: Bú shì, nà shì wǒ yéye de cídiǎn. Wǒ yǒu yì běn Hàn-Yuè cídiǎn zài shūbāo lǐ.
  - *Phân tích*: `一本...词典` (1 cuốn từ điển).
  - *Dịch*: Không phải, đó là từ điển của ông nội mình. Mình có một cuốn từ điển Hán-Việt ở trong ba lô cơ.

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện lượng từ, `两/二` & `的`
1. Điền lượng từ đúng (`口`, `本`, `张`, `件`, `只`):
   - `一 ___ 照片`, `两 ___ 衣服`, `三 ___ 汉语书`, `四 ___ 人`, `两 ___ 小猫`.
2. Chọn `二` hoặc `两`: `我买了 ___ 张火车票，现在是十 ___ 点。`
3. Sửa lỗi câu sau: `我喝咖啡和看书。`

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: `一张照片`, `两件衣服`, `三本汉语书`, `四口人`, `两只小猫`.
2. **Câu 2**: `我买了 两 张火车票，现在是十 二 点。` *(Trước lượng từ dùng `两`; số 12 giữ nguyên `十二`).*
3. **Câu 3**: Bỏ `和` vì `和` không nối hai động từ/mệnh đề: `我一边喝咖啡，一边看书。` hoặc `我喝咖啡，然后看书。`
4. **Câu 4 trắc nghiệm**: Lượng từ đúng cho ""một cái bàn"" (`桌子`) là gì?
   - *Đáp án*: `一张桌子` *(vì mặt bàn phẳng rộng dùng `张`).*
5. **Câu 5 trắc nghiệm**: Câu nào lược bỏ `的` tự nhiên và chuẩn ngữ pháp nhất cho ""Mẹ của bạn tôi""?
   - *Đáp án*: `我朋友的妈妈` *(Lược `的` đầu giữa `我` và `朋友`, giữ `的` sau trước `妈妈`).*

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Hãy làm ngay bộ **Cloze & Word Map** bên dưới để ghi nhớ vĩnh viễn cặp lượng từ `本 / 张 / 件 / 口 / 只` và quy tắc `两` vs `二`!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Nắm vững từ vựng gia đình đa thế hệ, 6 lượng từ thực chiến HSK1 (口, 个, 本, 张, 件, 只) và quy tắc ẩn/hiện 的.",
            "Phân biệt chuẩn xác 两 vs 二 trước lượng từ và tránh lỗi dùng 和 nối hai động từ."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, ChineseChapter4Id, ChineseLesson43Id,
            "Bài 4.3: Nghề Nghiệp, Nơi Làm Việc & Mẫu Câu Đang Làm Gì (在 + Động Từ)",
            "zh-hsk1-nghe-nghiep-noi-lam-viec-zai", 3, 40,
            @"# Bài 4.3: Nghề Nghiệp, Nơi Làm Việc & Mẫu Câu Đang Làm Gì (在 + Động Từ)

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Khép lại Chương 4, bài học 4.3 kết nối chủ đề **nghề nghiệp, môi trường làm việc** với một trong những điểm ngữ pháp xuất hiện nhiều nhất trong giao tiếp thực tế và đề thi HSK1: **chức năng kép của chữ `在 (zài)`** *(vừa chỉ nơi chốn thực hiện hành động, vừa diễn đạt thì hiện tại tiếp diễn ""đang làm gì"")*. Sau bài học này, bạn sẽ:
- Nắm chắc bộ từ vựng **nghề nghiệp & nơi làm việc thông dụng**: `医生` (bác sĩ), `护士` (y tá), `工程师` (kỹ sư), `公司职员` (nhân viên công ty), `经理` (giám đốc/quản lý), `服务员` (phục vụ), `医院`, `公司`, `饭店`.
- Phân biệt rõ **3 vai trò của `在 (zài)`**:
  1. Động từ chỉ vị trí: `A 在 B` *(A ở tại B)*.
  2. Giới từ chỉ nơi diễn ra hành động: `在 + Địa điểm + Động từ` *(làm gì ở đâu)*.
  3. Phó từ thì tiếp diễn: `在 / 正在 + Động từ (+ 呢)` *(đang làm gì)*.
- Kết hợp thành thạo cả Địa điểm và Tiếp diễn trong cùng một câu chuẩn trật tự tiếng Trung: **`Chủ ngữ + 正在 + Địa điểm + Động từ`** hay **`在 Địa điểm + Động từ`**?
- Thực hành hỏi thăm công việc lịch sự (`你在哪儿工作？`, `你是做什么工作的？`) và trả lời tự nhiên như người bản xứ.

---

## 2. 🌱 Nhập môn chậm rãi từng bước: 3 Chức năng vàng của chữ `在 (zài)`

### 2.1. Phân biệt 3 vai trò của `在` trong một bảng duy nhất
Rất nhiều người học bị rối khi thấy chữ `在` xuất hiện ở nhiều vị trí khác nhau trong câu. Hãy nhìn cấu trúc đứng ngay sau `在`:

| Vai trò | Theo sau `在` là gì? | Công thức chuẩn | Ví dụ & Nghĩa tiếng Việt |
|---|---|---|---|
| **1. Ở tại (Động từ)** | Danh từ chỉ nơi chốn (cuối câu) | `Chủ ngữ + 在 + Nơi chốn` | `王老师在办公室。` *(Thầy Vương ở trong văn phòng.)* |
| **2. Tại nơi nào làm gì (Giới từ)** | Nơi chốn + **Động từ hành động** | `Chủ ngữ + 在 + Nơi chốn + Động từ` | `我在医院工作。` *(Tôi làm việc tại bệnh viện.)* |
| **3. Đang làm gì (Phó từ tiếp diễn)** | **Động từ** (có thể thêm `正/正在` và `呢` cuối câu) | `Chủ ngữ + (正)在 + Động từ (+ 呢)` | `他正在打电话呢。` *(Anh ấy đang gọi điện thoại đấy.)* |

### 2.2. Khi câu có CẢ ""Đang"" VÀ ""Ở đâu"" thì ghép thế nào?
- **TUYỆT ĐỐI KHÔNG lặp hai chữ `在` cạnh nhau** như `我在在学校学习` ❌!
- Khi muốn nói *""Tôi đang học bài ở thư viện""*, dùng **`正在`** đứng trước **`在 + Địa điểm`** hoặc lược bỏ `在` tiếp diễn chỉ giữ `在 Địa điểm + Động từ + 呢`:
  - `我在图书馆看书呢。` *(Tôi đang đọc sách ở thư viện đây.)*
  - `他正在公司开会呢。` *(Anh ấy đang họp ở công ty đấy.)*
- **Phủ định của hành động đang diễn ra**: Dùng **`没(有) + 在 + Động từ`** (KHÔNG dùng `不在`!):
  - `我没在看电视，我在学习呢。` *(Tôi không có đang xem tivi, tôi đang học bài mà.)*

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbiǎo)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **工作** | gōngzuò | Công Tác | Làm việc / Công việc | 你在哪家公司工作？ *(Bạn làm việc ở công ty nào?)* |
| **公司** | gōngsī | Công Ty | Công ty | 我们公司有很多年轻工程师。 *(Công ty chúng tôi có nhiều kỹ sư trẻ.)* |
| **工程师** | gōngchéngshī | Công Trình Sư | Kỹ sư | 我哥哥是一位软件工程师。 *(Anh trai tôi là một kỹ sư phần mềm.)* |
| **医生 / 护士** | yīshēng / hùshi | Y Sinh / Hộ Sĩ | Bác sĩ / Y tá | 她在人民医院当医生。 *(Cô ấy làm bác sĩ ở Bệnh viện Nhân dân.)* |
| **经理 / 职员** | jīnglǐ / zhíyuán | Kinh Lý / Chức Viên | Quản lý, giám đốc / Nhân viên | 李经理正在办公室看文件。 *(Giám đốc Lý đang xem tài liệu ở văn phòng.)* |
| **服务员** | fúwùyuán | Phục Vụ Viên | Nhân viên phục vụ | 服务员，请给我一杯冰水！ *(Phục vụ ơi, cho tôi một ly nước đá!)* |
| **银行** | yínháng | Ngân Hàng | Ngân hàng | 银行在学校对面。 *(Ngân hàng ở đối diện trường học.)* |
| **办公室** | bàngōngshì | Biện Công Thất | Văn phòng làm việc | 经理现在不在办公室。 *(Hiện tại giám đốc không ở văn phòng.)* |
| **正在...呢** | zhèngzài... ne | Chính Tại ... Ni | Đang ... (nhấn mạnh tiếp diễn) | 妈妈正在厨房做饭呢。 *(Mẹ đang nấu cơm trong bếp đấy.)* |
| **打电话** | dǎ diànhuà | Đả Điện Thoại | Gọi điện thoại | 爸爸正在给爷爷打电话。 *(Bố đang gọi điện thoại cho ông nội.)* |
| **写作业** | xiě zuòyè | Tả Tác Nghiệp | Làm bài tập về nhà | 弟弟在房间里写作业呢。 *(Em trai đang làm bài tập trong phòng.)* |
| **睡觉** | shuìjiào | Thụy Giác | Đi ngủ | 这么晚了，他还在睡觉吗？ *(Muộn thế này rồi mà anh ấy vẫn đang ngủ à?)* |
| **开会** | kāihuì | Khai Hội | Họp hành | 今天上午九点我们要开会。 *(9 giờ sáng nay chúng tôi phải họp.)* |
| **忙 / 累** | máng / lèi | Mang / Lụy | Bận rộn / Mệt mỏi | 这几天工作非常忙，也很累。 *(Mấy ngày nay công việc cực kỳ bận, cũng rất mệt.)* |
| **找** | zhǎo | Trảo | Tìm kiếm | 请问，您找谁？ *(Xin hỏi, anh/chị tìm ai ạ?)* |

---

## 4. Ngữ pháp mổ xẻ: Hỏi nghề nghiệp, Động từ ly hợp `睡觉 / 工作` & Trật tự câu

### 4.1. Hai cách hỏi nghề nghiệp tự nhiên như người bản xứ
1. **Cách hỏi lịch sự thông dụng**: `你是做什么工作的？` *(Bạn làm công việc gì thế?)*
   - Trả lời: `我是软件工程师。` *(Tôi là kỹ sư phần mềm.)* hoặc `我在一家IT公司工作。` *(Tôi làm việc ở một công ty IT.)*
2. **Cách hỏi trực tiếp**: `你做什么工作？` hoặc `你在哪儿工作？` *(Bạn làm việc ở đâu?)*

### 4.2. Cẩn thận với Động từ Ly Hợp `睡觉` (ngủ) và `开会` (họp)
- Không được thêm tân ngữ trực tiếp sau `睡觉` hay `开会`!
  - Muốn nói *""Ngủ 8 tiếng""*: Chèn `八个小时` vào giữa `睡` và `觉`: **`睡八个小时觉`** *(hoặc đơn giản: `睡了八个小时`)*.
  - Muốn nói *""Họp xong rồi""*: `开完会了`.

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 我工作在一家电脑公司。 (Đặt 在 + Địa điểm sau động từ 工作 theo ngữ pháp tiếng Việt)
   ⭕ ĐÚNG: 我在一家电脑公司工作。 (Trạng ngữ địa điểm 在... luôn đứng TRƯỚC động từ)
   💡 GIẢI THÍCH: Trong tiếng Trung, bạn ở đâu thì mới làm việc ở đó — nên ""在 + Nơi chốn"" luôn đi trước động từ chính!

2. ❌ SAI: 他在在办公室打电脑。 (Lặp hai chữ 在 liên tiếp)
   ⭕ ĐÚNG: 他正在办公室工作呢。 / 他在办公室工作呢。
   💡 GIẢI THÍCH: Không bao giờ đặt hai chữ 在 liền kề; dùng 正在 ở đầu để biểu thị ""đang"", và 在 để dẫn nơi chốn.

3. ❌ SAI: 我不在看电视，我在学习。 (Dùng 不 trong phủ định hành động đang diễn ra)
   ⭕ ĐÚNG: 我没在看电视，我在学习呢。 (Phủ định tiếp diễn dùng 没(有)在 + Động từ)
   💡 GIẢI THÍCH: Phủ định của 在/正在 làm gì bắt buộc dùng 没 (méi), không dùng 不 (bù) vì 不在 nghĩa là ""không có mặt ở địa điểm"".

4. ❌ SAI: 你是工程师什么？ (Đặt từ hỏi 什么 ở cuối câu hỏi nghề nghiệp)
   ⭕ ĐÚNG: 你是做什么工作的？ / 你做什么工作？
   💡 GIẢI THÍCH: Từ hỏi 什么 luôn đứng ở vị trí của danh từ mà nó hỏi (做 [什么] 工作).
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Minh gọi điện đến công ty của anh Lý (bạn cũ đại học) để hỏi thăm công việc mới và rủ đi ăn trưa.

- **Lễ tân**: 喂，您好！这里是华星科技公司。请问您找哪位？
  - *Pinyin*: Wéi, nínhǎo! Zhèlǐ shì Huáxīng kējì gōngsī. Qǐngwèn nín zhǎo nǎ wèi?
  - *Phân tích*: `科技公司` (công ty công nghệ) + `找哪位` (tìm vị nào — lịch sự hơn `找谁`).
  - *Dịch*: A lô, xin chào! Đây là công ty công nghệ Hoa Tinh. Xin hỏi anh tìm ai ạ?

- **Minh**: 您好！请问李明经理在吗？我是他的大学同学。
  - *Pinyin*: Nínhǎo! Qǐngwèn Lǐ Míng jīnglǐ zài ma? Wǒ shì tā de dàxué tóngxué.
  - *Phân tích*: `在吗` (có ở đó không?) + `大学同学` (bạn học đại học).
  - *Dịch*: Xin chào! Cho hỏi quản lý Lý Minh có ở đó không ạ? Tôi là bạn học đại học của anh ấy.

- **Lễ tân**: 抱歉，李经理现在正在会议室开会呢，请您等一下儿。
  - *Pinyin*: Bàoqiàn, Lǐ jīnglǐ xiànzài zhèngzài huìyìshì kāihuì ne, qǐng nín děng yíxiàr.
  - *Phân tích*: `正在会议室开会呢` (đang họp trong phòng họp).
  - *Dịch*: Xin lỗi anh, hiện tại quản lý Lý đang họp trong phòng họp ạ, anh vui lòng đợi một lát nhé.

*(Một lúc sau, Lý Minh gọi lại cho Minh)*

- **Lý Minh**: 喂，明！不好意思，刚才我在开会，没在看手机。你最近在哪儿工作？
  - *Pinyin*: Wéi, Míng! Bù hǎoyìsi, gāngcái wǒ zài kāihuì, méi zài kàn shǒujī. Nǐ zuìjìn zài nǎr gōngzuò?
  - *Phân tích*: `没在看手机` (không có đang xem điện thoại) + `在哪儿工作` (làm việc ở đâu?).
  - *Dịch*: A lô Minh à! Ngại quá, vừa nãy mình đang họp nên không xem điện thoại. Dạo này cậu làm việc ở đâu rồi?

- **Minh**: 我现在在一家软件公司当工程师，工作虽然有点儿忙，但是很有意思。你中午有时间吗？
  - *Pinyin*: Wǒ xiànzài zài yì jiā ruǎnjiàn gōngsī dāng gōngchéngshī, gōngzuò suīrán yǒudiǎnr máng, dànshì hěn yǒuyìsi. Nǐ zhōngwǔ yǒu shíjiān ma?
  - *Phân tích*: `当工程师` (làm kỹ sư) + `有点儿忙` (hơi bận).
  - *Dịch*: Bây giờ mình đang làm kỹ sư ở một công ty phần mềm, công việc tuy hơi bận nhưng rất thú vị. Trưa nay cậu có rảnh không?

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện cấu trúc `在` & Trật tự câu
1. Sắp xếp các từ sau thành câu đúng: `正在` / `妹妹` / `写作业` / `房间里` / `呢`.
2. Điền từ phủ định đúng: `他 ___ 在睡觉，他在看书呢。` (`不` hay `没`?)
3. Dịch sang tiếng Trung: *""Mẹ tôi làm bác sĩ ở bệnh viện.""*

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: `妹妹正在房间里写作业呢。` *(Chủ ngữ + 正在 + Địa điểm + Động từ + 呢).*
2. **Câu 2**: Chọn **`没`** (`他没在睡觉，他在看书呢。`).
3. **Câu 3**: `我妈妈在医院工作，她是医生。` hoặc `我妈妈在医院当医生。`
4. **Câu 4 trắc nghiệm**: Khi gọi điện đến công ty muốn hỏi quản lý Vương có ở đó không, ta nói:
   - *Đáp án*: `请问，王经理在吗？`
5. **Câu 5 trắc nghiệm**: Câu nào viết ĐÚNG ngữ pháp về vị trí nơi làm việc?
   - *Đáp án*: `我在银行工作。` *(Trạng ngữ địa điểm 在银行 đặt trước động từ 工作).*

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi hoàn thành 8 hoạt động thực hành ở Bài 4.3, hãy bấm vào **`[Đề Chương 4]`** để tổng kiểm tra toàn bộ chủ đề Thời tiết, Gia đình, Lượng từ và Công việc!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Làm chủ 3 chức năng của chữ 在 (vị trí, nơi hành động, thì tiếp diễn 正在...呢) và bộ từ vựng nghề nghiệp.",
            "Sắp xếp đúng trật tự Chủ ngữ + 正在 + Địa điểm + Động từ và phủ định tiếp diễn với 没在."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, ChineseChapter5Id, ChineseLesson51Id,
            "Bài 5.1: Khả Năng & Ý Muốn (会, 能, 可以, 想) Trong Giao Tiếp Hằng Ngày",
            "zh-hsk1-kha-nang-y-muon-hui-neng-keyi-xiang", 1, 45,
            @"# Bài 5.1: Khả Năng & Ý Muốn (会, 能, 可以, 想) Trong Giao Tiếp Hằng Ngày

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Bước vào Chương 5 — chương tổng kết và nâng cao năng lực giao tiếp thực chiến HSK1+, bạn sẽ giải quyết triệt để một trong những ""nỗi ám ảnh"" lớn nhất của người học tiếng Trung: **Phân biệt 4 trợ động từ năng nguyện (能愿动词) hay nhầm lẫn nhất: `会 (huì)`, `能 (néng)`, `可以 (kěyǐ)`, và `想 (xiǎng)`**. Sau bài học này, bạn sẽ:
- Hiểu rõ bản chất từng trợ động từ:
  - **`会 (huì)`**: Biết làm (kỹ năng phải học mới có) & Sẽ (dự đoán tương lai).
  - **`能 (néng)`**: Có thể (đủ năng lực thể chất hoặc điều kiện khách quan cho phép).
  - **`可以 (kěyǐ)`**: Có thể, được phép (xin phép, cho phép hoặc đề xuất giải pháp).
  - **`想 (xiǎng)`** vs **`要 (yào)`**: Muốn nhẹ nhàng/dự định vs Muốn dứt khoát/cần làm.
- Phân biệt chuẩn xác cách phủ định của từng từ (`不会`, `不能`, `不可以` vs `没`, `不想` vs `不要 / 不想`).
- Đặt câu hỏi xin phép lịch sự (`我可以坐这儿吗？`) và từ chối khéo léo theo văn hóa giao tiếp Trung Hoa (`不好意思，这儿有人了`).
- Kết hợp nhuần nhuyễn trợ động từ năng nguyện với trạng ngữ thời gian và địa điểm.

---

## 2. 🌱 Mổ xẻ bản chất 4 Trợ động từ Năng nguyện (会 / 能 / 可以 / 想)

### 2.1. Bảng so sánh 4 Trợ động từ ""có thể / biết / muốn""
| Trợ động từ | Ý nghĩa cốt lõi | Ngữ cảnh sử dụng chuẩn | Dạng phủ định | Ví dụ minh họa |
|---|---|---|---|---|
| **会 (huì)** | **Biết** *(kỹ năng học tập/rèn luyện)* / **Sẽ** *(tương lai)* | Nói tiếng, bơi, lái xe, nấu ăn, dự báo thời tiết | **`不会`** *(Không biết / Sẽ không)* | `我会说汉语。` *(Tôi biết nói tiếng Hán.)*<br>`明天会下雨吗？` *(Mai trời sẽ mưa à?)* |
| **能 (néng)** | **Có thể** *(năng lực bản thân hoặc điều kiện khách quan)* | Bị đau chân không đi được, bận không đến được, uống được mấy cốc | **`不能`** *(Không thể / Cấm trong biển báo)* | `今天我太忙了，不能去。` *(Nay bận quá tôi không đi được.)*<br>`你能喝两杯咖啡吗？` *(Bạn uống nổi 2 ly cà phê không?)* |
| **可以 (kěyǐ)** | **Có thể / Được phép** *(xin phép, cho phép)* | Xin ngồi, xin chụp ảnh, thanh toán bằng thẻ | **`不可以`** *(Không được phép — hơi cứng)* / dùng **`不能`** cho nhẹ hơn | `我可以坐在这儿吗？` *(Tôi ngồi đây có được không?)*<br>`可以用微信支付吗？` *(Có thể trả bằng WeChat Pay không?)* |
| **想 (xiǎng)** | **Muốn / Nhớ / Nghĩ** | Mong muốn nhẹ nhàng, dự định | **`不想`** *(Không muốn)* | `我想去北京旅游。` *(Tôi muốn đi du lịch Bắc Kinh.)* |

### 2.2. Mẹo phân biệt nhanh `会` vs `能` khi cùng dịch là ""biết/có thể""
- Hãy tự hỏi: **Việc đó có phải học mới làm được không?**
  - **CÓ** (nói tiếng Trung, viết chữ Hán, lái xe, bơi lội, nấu cơm) → Dùng **`会`**: `你会开车吗？` *(Bạn biết lái xe không?)*
  - **KHÔNG** (uống 10 cốc nước, chạy 5km, mai đến dự tiệc được không) hoặc **nói về hoàn cảnh cho phép** → Dùng **`能`**: `你明天能来参加我的生日吗？` *(Ngày mai bạn có đến dự sinh nhật tôi được không?)* — Không hỏi `你会来吗` trừ khi mang nghĩa *""Mai bạn có sẽ đến không?""*.
- **Cảnh báo lịch sự khi từ chối lời xin phép `可以...吗？`**:
  - Nếu khách hỏi: `这里可以吸烟吗？` *(Ở đây hút thuốc được không?)*
  - Trả lời tự nhiên là **`对不起，这里不能吸烟。`** hoặc **`不可以`** (nếu muốn nhấn mạnh cấm đoán). Không dùng `不会` cho xin phép!

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbiǎo)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **会** | huì | Hội | Biết (kỹ năng) / Sẽ | 我会写两百个汉字。 *(Tôi biết viết 200 chữ Hán.)* |
| **能** | néng | Năng | Có thể (năng lực, điều kiện) | 你今天晚上能回来吗？ *(Tối nay bạn có về được không?)* |
| **可以** | kěyǐ | Khả Dĩ | Có thể, được phép | 这里可以拍照吗？ *(Ở đây có được chụp ảnh không?)* |
| **想** | xiǎng | Tưởng | Muốn / Nhớ / Nghĩ | 我很想我的爸爸妈妈。 *(Tôi rất nhớ bố mẹ của tôi.)* |
| **要** | yào | Yếu | Muốn (dứt khoát) / Cần / Phải | 我要买一张去上海的票。 *(Tôi muốn mua một vé đi Thượng Hải.)* |
| **游泳** | yóuyǒng | Du Vịnh | Bơi lội | 夏天我喜欢去游泳。 *(Mùa hè tôi thích đi bơi.)* |
| **开车** | kāichē | Khai Xa | Lái xe (ô tô) | 我爸爸会开车，我不会。 *(Bố tôi biết lái xe, tôi thì không.)* |
| **做饭** | zuòfàn | Tác Phạn | Nấu cơm, nấu ăn | 你会做中国菜吗？ *(Bạn biết nấu món Trung Quốc không?)* |
| **帮忙** | bāngmáng | Bang Mang | Giúp đỡ | 请问，你能帮我一个忙吗？ *(Xin hỏi, bạn giúp tôi một việc được không?)* |
| **手机支付** | shǒujī zhīfù | Thủ Cơ Chi Phó | Thanh toán qua điện thoại | 在中国可以用手机支付。 *(Ở Trung Quốc có thể thanh toán qua điện thoại.)* |
| **拍照** | pāizhào | Phách Chiếu | Chụp ảnh | 博物馆里面不能拍照。 *(Bên trong bảo tàng không được chụp ảnh.)* |
| **吸烟 / 抽烟** | xīyān / chōuyān | Hấp Yên / Trừu Yên | Hút thuốc lá | 公共场所不可以吸烟。 *(Nơi công cộng không được phép hút thuốc.)* |
| **参加** | cānjiā | Tham Gia | Tham gia | 周末你能参加我们的聚会吗？ *(Cuối tuần bạn tham gia buổi tụ họp với tụi mình được không?)* |
| **因为...所以...** | yīnwèi... suǒyǐ... | Nhân Vị... Sở Dĩ... | Bởi vì ... cho nên ... | 因为生病了，所以我不能去上班。 *(Vì bị ốm rồi nên tôi không thể đi làm.)* |
| **问题** | wèntí | Vấn Đề | Câu hỏi, vấn đề | 没问题，我可以帮你！ *(Không vấn đề gì, tôi có thể giúp bạn!)* |

---

## 4. Ngữ pháp nâng cao: Câu phức `因为...所以...` & Bẫy phủ định `不想` vs `不要`

### 4.1. Cặp liên từ nguyên nhân — kết quả: `因为...，所以...`
- Khác với tiếng Anh (cấm dùng *Because... so...* cùng lúc), tiếng Trung **thường xuyên dùng trọn cặp `因为 (vì) ..., 所以 (cho nên) ...`** hoặc dùng một trong hai vế!
  - `因为我不会开车，所以我坐地铁去。` *(Vì tôi không biết lái xe nên tôi đi tàu điện ngầm.)*
  - `明天要考试，所以我今天晚上不能去看电影。` *(Mai phải thi rồi, cho nên tối nay tôi không đi xem phim được.)*

### 4.2. Tinh tế giữa `不想` (không muốn) vs `不要` (đừng / không lấy)
- **`不想 + Động từ`**: Diễn đạt ý nghĩ trong lòng *""Không muốn làm...""* (nhẹ nhàng, lịch sự):
  - `我今天有点儿累，不想出去吃饭。` *(Hôm nay tôi hơi mệt, không muốn ra ngoài ăn.)*
- **`不要 + Động từ / Danh từ`**: Mang sắc thái **ngăn cản/khuyên nhủ (""Đừng..."")** hoặc **từ chối thẳng đồ vật (""Không lấy..."")**:
  - `太晚了，不要喝咖啡了！` *(Muộn quá rồi, đừng uống cà phê nữa!)*
  - `谢谢，我不要袋子。` *(Cảm ơn, tôi không cần túi đâu.)*

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 我能说一点儿汉语，但是我不能写汉字。 (Dùng 能 cho kỹ năng học được)
   ⭕ ĐÚNG: 我会说一点儿汉语，但是不会写汉字。 (Kỹ năng học mới biết dùng 会)
   💡 GIẢI THÍCH: Nói ngoại ngữ, viết chữ Hán, bơi, lái xe là kỹ năng tập luyện, nên dùng 会!

2. ❌ SAI: 我很想会游泳。 (Ghép từ mức độ 很 trực tiếp trước 会/能 không hợp lý hoặc nhầm 想 với 会)
   ⭕ ĐÚNG: 我很想学游泳。 / 我非常会游泳。 (Muốn học thì dùng 想学; biết giỏi thì dùng 很会)
   💡 GIẢI THÍCH: 很 chỉ đứng trước các trợ động từ chỉ tâm lý như 想, 喜欢, 会 (khi chỉ trình độ thành thạo).

3. ❌ SAI: 明天我会能去你家。 (Xếp hai trợ động từ 会 và 能 liền nhau sai nghĩa)
   ⭕ ĐÚNG: 明天我能去你家。 (Có thể đến: 能) hoặc 明天我会去你家。 (Sẽ đến: 会)
   💡 GIẢI THÍCH: Chọn đúng một trợ động từ phù hợp nhất: dự định chắc chắn sẽ làm dùng 会, đủ điều kiện thực hiện dùng 能.

4. ❌ SAI: 我可以去洗手间嗎？— 是，你可以。 (Trả lời bằng 是 như tiếng Anh ""Yes, you can"")
   ⭕ ĐÚNG: 可以，请！ / 当然可以！ (Lặp lại chính trợ động từ 可以)
   💡 GIẢI THÍCH: Trong tiếng Trung, trả lời câu hỏi trợ động từ phải lặp lại chính trợ động từ đó (可以 / 会 / 能), KHÔNG trả lời bằng 是!
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Cuối tuần, Trương Vĩ rủ Minh đi chơi công viên và ăn lẩu, nhưng hai người cần trao đổi xem có thể đi bằng xe hay tàu điện và thanh toán thế nào.

- **Zhang Wei**: 明，明天是星期六，我们一起去爬山，怎么样？
  - *Pinyin*: Míng, míngtiān shì Xīngqīliù, wǒmen yìqǐ qù páshān, zěnmeyàng?
  - *Phân tích*: `爬山` (leo núi) + `怎么样` (thế nào?).
  - *Dịch*: Minh ơi, mai là thứ Bảy rồi, tụi mình cùng đi leo núi nhé, thấy sao?

- **Minh**: 好啊！但是明天会下雨吗？如果下雨，我们就不能去了。
  - *Pinyin*: Hǎo a! Dànshì míngtiān huì xiàyǔ ma? Rúguǒ xiàyǔ, wǒmen jiù bù néng qù le.
  - *Phân tích*: `会下雨吗` (sẽ mưa không? — dự đoán) + `不能去了` (không thể đi được — điều kiện khách quan).
  - *Dịch*: Hay quá! Nhưng mà mai trời có mưa không? Nếu mưa thì mình không đi được đâu.

- **Zhang Wei**: 我看天气预报了，明天不会下雨，是晴天！你会开车吗？
  - *Pinyin*: Wǒ kàn tiānqì yùbào le, míngtiān bú huì xiàyǔ, shì qíngtiān! Nǐ huì kāichē ma?
  - *Phân tích*: `不会下雨` (sẽ không mưa) + `会开车吗` (biết lái xe không? — kỹ năng).
  - *Dịch*: Mình xem dự báo thời tiết rồi, mai trời không mưa đâu, nắng đẹp! Cậu có biết lái xe ô tô không?

- **Minh**: 不好意思，我不会开车。我们可以坐地铁去吗？
  - *Pinyin*: Bù hǎoyìsi, wǒ bú huì kāichē. Wǒmen kěyǐ zuò dìtiě qù ma?
  - *Phân tích*: `不会` (không biết kỹ năng) + `可以...吗` (đề xuất phương án có được không?).
  - *Dịch*: Ngại quá, mình không biết lái ô tô. Tụi mình đi tàu điện ngầm được không?

- **Zhang Wei**: 当然可以！对了，那家火锅店不能用现金，只能用手机支付，你可以吗？
  - *Pinyin*: Dāngrán kěyǐ! Duìle, nà jiā huǒguōdiàn bù néng yòng xiànjīn, zhǐ néng yòng shǒujī zhīfù, nǐ kěyǐ ma?
  - *Phân tích*: `不能用现金` (không thể dùng tiền mặt) + `手机支付` (thanh toán điện thoại).
  - *Dịch*: Đương nhiên là được chứ! À đúng rồi, quán lẩu đó không dùng tiền mặt được, chỉ thanh toán qua điện thoại thôi, cậu dùng được chứ?

- **Minh**: 没问题！我早就学会手机支付了。明天见！
  - *Pinyin*: Méi wèntí! Wǒ zǎo jiù xuéhuì shǒujī zhīfù le. Míngtiān jiàn!
  - *Phân tích*: `没问题` (không vấn đề) + `学会` (học thành thạo).
  - *Dịch*: Không vấn đề gì! Mình học cách thanh toán qua điện thoại từ lâu rồi. Hẹn mai gặp nhé!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Điền trợ động từ phù hợp (`会`, `能`, `可以`, `想`)
1. `我 ___ 说一点儿汉语，但是写汉字写得不太好。` *(kỹ năng)*
2. `对不起，今天我脚疼，不 ___ 去跑步了。` *(điều kiện thể chất)*
3. `请问，我 ___ 在这里拍照吗？` *(xin phép)*
4. `周末我 ___ 去图书馆看书。` *(dự định mong muốn)*

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: `会` (nói ngoại ngữ là kỹ năng học được).
2. **Câu 2**: `能` (điều kiện thể chất đau chân không cho phép đi chạy bộ).
3. **Câu 3**: `可以` (xin phép làm gì ở nơi nào đó).
4. **Câu 4**: `想` (mong muốn, dự định nhẹ nhàng).
5. **Câu 5 trắc nghiệm**: Để trả lời đồng ý cho câu hỏi ""请问，我可以坐这儿吗？"", cách đáp tự nhiên chuẩn nhất là:
   - *Đáp án*: `可以，请坐！` *(Không dùng `是，你可以`).*

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Hãy làm ngay 4 bài **Cloze & Grammar Fix** bên dưới để không bao giờ nhầm giữa `会` (kỹ năng/sẽ), `能` (khả năng/điều kiện) và `可以` (xin phép)!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Phân biệt rạch ròi 4 trợ động từ năng nguyện 会 (kỹ năng/sẽ), 能 (khả năng), 可以 (xin phép) và 想/要 (ý muốn).",
            "Thành thạo câu phức nguyên nhân 因为...所以... và cách trả lời, từ chối lời xin phép lịch sự."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, ChineseChapter5Id, ChineseLesson52Id,
            "Bài 5.2: Câu Chữ 是...的 (Nhấn Mạnh Thời Gian/Địa Điểm/Phương Thức Quá Khứ) & Hoàn Thành 了",
            "zh-hsk1-cau-shi-de-hoan-thanh-le", 2, 45,
            @"# Bài 5.2: Câu Chữ 是...的 (Nhấn Mạnh Thời Gian/Địa Điểm/Phương Thức Quá Khứ) & Hoàn Thành 了

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Bài học 5.2 là **đỉnh cao ngữ pháp của trình độ HSK1 và bước đệm sang HSK2**! Người Việt thường nói *""Tôi đến Bắc Kinh bằng máy bay hôm qua""* bằng cách thêm `了` bừa bãi, trong khi người bản xứ dùng một cấu trúc cực kỳ thanh thoát: **Cấu trúc nhấn mạnh quá khứ `是...的 (shì... de)`** và **Trợ từ hoàn thành `了 (le)`**. Sau bài học này, bạn sẽ:
- Phân biệt rõ hai vai trò phổ biến nhất của chữ **`了 (le)`**:
  1. **`了` đứng sau động từ** (Động thái `了1`): Hành động đã hoàn thành (`我买了两本书`).
  2. **`了` đứng cuối câu** (Ngữ khí `了2`): Biểu thị sự thay đổi trạng thái mới (`下雨了` trời mưa rồi, `他二十岁了` nó 20 tuổi rồi).
- Chia chuẩn xác **Phủ định của hành động đã qua**: Dùng **`没有 / 没 + Động từ`** và **TUYỆT ĐỐI BỎ `了`**!
- Làm chủ **Cấu trúc `是...的` (shì... de)** để hỏi và nhấn mạnh **Khi nào? Ở đâu? Bằng cách nào? Cùng với ai?** cho một sự việc **ĐÃ XẢY RA**.
- Nắm chắc vị trí đặt chữ `的` trong câu `是...的` khi động từ có mang tân ngữ (`我是坐飞机来北京的` vs `我是坐飞机来的北京`).

---

## 2. 🌱 Mổ xẻ bản chất Trợ từ `了` & Cấu trúc nhấn mạnh `是...的`

### 2.1. Hai vị trí của `了 (le)` và Quy tắc phủ định `没(有)`
1. **`了` chỉ hành động hoàn thành (sau Động từ)**:
   - `昨天我买了一件新衣服。` *(Hôm qua tôi đã mua một chiếc áo mới.)*
   - Khi có số lượng cụ thể đi sau, `了` thường đứng ngay sau động từ: `喝了一杯茶`, `看了一个电影`.
2. **`了` chỉ sự thay đổi trạng thái (cuối câu)**:
   - `天气冷了。` *(Trời trở lạnh rồi — trước đó không lạnh.)*
   - `我不想去了。` *(Tôi không muốn đi nữa rồi — thay đổi ý định.)*
3. **QUY TẮC BẤT DI BẤT DỊCH KHI PHỦ ĐỊNH QUÁ KHỨ**:
   - Phủ định hành động đã hoàn thành: Dùng **`没 / 没有 + Động từ`** và **BẮT BUỘC XÓA `了`**!
   - *ĐÚNG*: `昨天我没买衣服。` *(Hôm qua tôi không mua áo.)*
   - *SAI*: ❌ `昨天我没买了衣服` | ❌ `昨天我不买衣服`.

### 2.2. Cấu trúc nhấn mạnh quá khứ `是...的 (shì... de)`
Khi cả hai người **đều đã biết sự việc đó xảy ra rồi** (ví dụ: bạn đang đứng trước mặt tôi ở Trung Quốc), chúng ta không cần hỏi *""Bạn có đến không?""*, mà muốn **nhấn mạnh chi tiết xoay quanh sự việc đã qua**:
- **Công thức**: **`Chủ ngữ + (是) + [Thời gian / Địa điểm / Phương thức / Với ai] + Động từ + 的`**
  - **Nhấn mạnh THỜI GIAN**: `你是什么时候来中国的？` — `我是去年九月来的。` *(Bạn đến TQ khi nào? — Tôi đến vào tháng 9 năm ngoái.)*
  - **Nhấn mạnh PHƯƠNG THỨC**: `你是怎么来的？` — `我是坐飞机来的。` *(Bạn đến bằng cách nào? — Tôi đi máy bay đến.)*
  - **Nhấn mạnh ĐỊA ĐIỂM**: `你是在哪儿买的这本书？` — `我是在北京买的。` *(Bạn mua sách này ở đâu? — Tôi mua ở Bắc Kinh.)*
  - **Nhấn mạnh NGƯỜI CÙNG ĐI**: `我是和朋友一起来的。` *(Tôi đi cùng với bạn.)*
- **Phủ định của `是...的`**: Chỉ cần thêm **`不`** trước **`是`** → **`不是...的`**!
  - `我不是坐火车来的，我是坐飞机来的。` *(Không phải tôi đi tàu hỏa đến đâu, tôi đi máy bay đến đấy.)*

> [!IMPORTANT]
> **Mẹo đặt `的` khi có Tân ngữ là Danh từ riêng/Địa danh**:
> - Cách an toàn nhất (luôn đúng 100% ở HSK1–HSK2): Đặt `的` ở **cuối cùng** (`我是去年来中国的`).
> - Nếu tân ngữ là vật cụ thể không phải địa danh/người, có thể đặt `的` trước tân ngữ (`我是在商店买的苹果`) hoặc ở cuối (`我是在商店买苹果的`). Ở giai đoạn đầu, cứ đặt `的` ở cuối câu với đại từ/địa danh là chuẩn xác nhất!

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbiǎo)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **去年 / 明年** | qùnián / míngnián | Khứ Niên / Minh Niên | Năm ngoái / Năm sau | 我是去年九月来北京的。 *(Tôi đến Bắc Kinh vào tháng 9 năm ngoái.)* |
| **上个月 / 下个月** | shàng ge yuè / xià ge yuè | Thượng / Hạ Cá Nguyệt | Tháng trước / Tháng sau | 他上个月去上海出差了。 *(Tháng trước anh ấy đã đi công tác Thượng Hải.)* |
| **已经** | yǐjīng | Dĩ Kinh | Đã ... rồi (nhấn mạnh) | 我已经吃过早饭了。 *(Tôi đã ăn sáng rồi.)* |
| **刚才** | gāngcái | Cương Tài | Vừa nãy, ban nãy | 刚才谁给你打电话了？ *(Vừa nãy ai gọi điện cho bạn thế?)* |
| **飞机** | fēijī | Phi Cơ | Máy bay | 我们是坐飞机去旅游的。 *(Chúng tôi đi du lịch bằng máy bay.)* |
| **火车** | huǒchē | Hỏa Xa | Tàu hỏa | 坐火车去大概需要三个小时。 *(Đi tàu hỏa mất khoảng 3 tiếng.)* |
| **认识** | rènshi | Nhận Thức | Quen biết (ai đó) | 你们是怎么认识的？ *(Hai bạn quen nhau như thế nào thế?)* |
| **学习** | xuéxí | Học Tập | Học tập | 我是在大学学习汉语的。 *(Tôi đã học tiếng Trung ở trường đại học.)* |
| **出生** | chūshēng | Xuất Sinh | Sinh ra, chào đời | 我是在河内出生的。 *(Tôi sinh ra ở Hà Nội.)* |
| **毕业** | bìyè | Tốt Nghiệp | Tốt nghiệp | 姐姐是前年大学毕业的。 *(Chị gái tốt nghiệp đại học năm kia.)* |
| **买** | mǎi | Mãi | Mua | 这双鞋是在网上买的。 *(Đôi giày này là mua trên mạng đấy.)* |
| **和...一起** | hé... yìqǐ | Hòa... Nhất Khởi | Cùng với ai đó | 我是和妹妹一起来看电影的。 *(Tôi đi xem phim cùng với em gái.)* |
| **怎么** | zěnme | Chẩm Ma | Thế nào / Bằng cách nào | 你是怎么学汉语的？ *(Bạn đã học tiếng Trung bằng cách nào thế?)* |
| **网上** | wǎngshàng | Võng Thượng | Trên mạng Internet | 我经常在网上买书。 *(Tôi thường xuyên mua sách trên mạng.)* |
| **送** | sòng | Tống | Tặng, biếu / Tiễn | 这本书是老师送给我的。 *(Cuốn sách này là thầy giáo tặng cho tôi.)* |

---

## 4. Ngữ pháp tinh tế: Phân biệt `了` vs `是...的` & 4 Bẫy Sai Kinh Điển

### 4.1. Khi nào dùng `了` và khi nào dùng `是...的`?
- Dùng **`了`** khi muốn thông báo **SỰ VIỆC ĐÃ XẢY RA CHƯA** (người nghe chưa biết):
  - `你买手机了吗？` — `买了，昨天买了一个新手机。` *(Cậu mua điện thoại chưa? — Mua rồi, hôm qua mua 1 chiếc mới.)*
- Dùng **`是...的`** khi cả hai **ĐÃ BIẾT việc đó xảy ra rồi**, chỉ muốn hỏi/nhấn mạnh **chi tiết (Khi nào / Ở đâu / Bằng gì / Ai)**:
  - `哇，你的新手机真漂亮！是在哪儿买的？` *(Ôi điện thoại mới đẹp quá! Mua ở đâu thế?)*
  - `是在网上买的。` *(Mua trên mạng đấy.)*
- **Cảnh báo**: KHÔNG BAO GIỜ đặt cả `是...的` và `了` vào cùng một động từ (tức là ❌ `我是昨天买手机了的`).

### 4.2. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 我昨天没去学校了。 (Giữ lại chữ 了 trong câu phủ định 没)
   ⭕ ĐÚNG: 我昨天没去学校。 (Có 没 thì PHẢI bỏ 了)
   💡 GIẢI THÍCH: 没 phủ định rằng hành động chưa từng xảy ra, còn 了 báo hành động đã hoàn tất — hai từ đối nghịch không đứng cùng nhau!

2. ❌ SAI: 我坐飞机来北京昨天。 (Đặt thời gian 昨天 ở cuối câu theo thói quen tiếng Anh/Việt)
   ⭕ ĐÚNG: 我是昨天坐飞机来北京的。 (Thời gian + Phương thức đứng TRƯỚC động từ trong khung 是...的)
   💡 GIẢI THÍCH: Thời gian trong tiếng Trung luôn đứng trước động từ; muốn nhấn mạnh bối cảnh quá khứ hãy bọc bằng 是...的.

3. ❌ SAI: 这本书不我买的。 (Đặt 不 trực tiếp trước đại từ 我 để phủ định 是...的)
   ⭕ ĐÚNG: 这本书不是我买的，是朋友送的。 (Phủ định 是...的 bắt buộc dùng 不是)
   💡 GIẢI THÍCH: Trong câu nhấn mạnh 是...的, chữ 是 có thể ẩn trong câu khẳng định nhưng BẮT BUỘC phải hiện ra khi phủ định (不是...的)!

4. ❌ SAI: 我不吃早饭了 (khi ý muốn nói ""Sáng nay tôi chưa ăn sáng"")
   ⭕ ĐÚNG: 我还没吃早饭呢。 (Chưa làm việc gì dùng 还没...呢)
   💡 GIẢI THÍCH: 我不吃了 nghĩa là ""Tôi không ăn nữa đâu"" (từ chối/thay đổi ý định); còn ""chưa ăn"" phải dùng 还没吃!
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Trong buổi gặp mặt du học sinh mới tại Thượng Hải, bạn Trần Lệ trò chuyện làm quen với Minh và hỏi về chuyến đi của Minh.

- **Chen Li**: 明，欢迎你来上海！你是昨天到的吗？
  - *Pinyin*: Míng, huānyíng nǐ lái Shànghǎi! Nǐ shì zuótiān dào de ma?
  - *Phân tích*: `是昨天到的吗` (có phải đến vào hôm qua không?).
  - *Dịch*: Minh ơi, chào mừng cậu đến Thượng Hải! Cậu đến vào hôm qua phải không?

- **Minh**: 不是，我是前天晚上到的。
  - *Pinyin*: Bú shì, wǒ shì qiántiān wǎnshang dào de.
  - *Phân tích*: `不是` (phủ định điểm nhấn) + `前天晚上` (tối hôm kia).
  - *Dịch*: Không phải đâu, mình đến từ tối hôm kia rồi.

- **Chen Li**: 哦！那你是坐飞机来的，还是坐火车来的？
  - *Pinyin*: Ò! Nà nǐ shì zuò fēijī lái de, háishi zuò huǒchē lái de?
  - *Phân tích*: Kết hợp `是...的` với câu hỏi lựa chọn `还是`.
  - *Dịch*: Ồ! Thế cậu đi máy bay đến hay đi tàu hỏa đến?

- **Minh**: 我是从河内坐飞机来的，非常快！对了，你这件好看的大衣是在哪儿买的？
  - *Pinyin*: Wǒ shì cóng Hénèi zuò fēijī lái de, fēicháng kuài! Duìle, nǐ zhè jiàn hǎokàn de dàyī shì zài nǎr mǎi de?
  - *Phân tích*: `在哪儿买的` (mua ở đâu thế?).
  - *Dịch*: Mình bay thẳng từ Hà Nội sang, nhanh lắm! À đúng rồi, chiếc áo khoác đẹp này cậu mua ở đâu thế?

- **Chen Li**: 这不是我买的，是我姐姐上个月在网上买，然后送给我的！
  - *Pinyin*: Zhè bú shì wǒ mǎi de, shì wǒ jiějie shàng ge yuè zài wǎngshàng mǎi, ránhòu sòng gěi wǒ de!
  - *Phân tích*: `不是我买的` + `姐姐...送给我的` (nhấn mạnh chủ thể tặng).
  - *Dịch*: Cái này không phải mình mua đâu, là chị gái mình tháng trước mua trên mạng rồi tặng cho mình đấy!

- **Minh**: 真好！你吃饭了吗？我还没吃午饭呢，我们一起去吧！
  - *Pinyin*: Zhēn hǎo! Nǐ chīfàn le ma? Wǒ hái méi chī wǔfàn ne, wǒmen yìqǐ qù ba!
  - *Phân tích*: `吃饭了吗` (ăn cơm chưa? — `了`) vs `还没吃...呢` (chưa ăn).
  - *Dịch*: Thích thật! Cậu ăn cơm chưa? Mình vẫn chưa ăn trưa đâu, tụi mình cùng đi ăn nhé!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện `了` và `是...的`
1. Chuyển sang câu phủ định quá khứ: `昨天我买了一本书。`
2. Đặt câu hỏi nhấn mạnh bằng `是...的` cho từ gạch chân: `我是 **坐火车** 来北京的。`
3. Sửa lỗi sai: `你们是在哪儿认识了的？`

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: `昨天我没买书。` *(Phủ định quá khứ dùng `没` và bỏ `了`).*
2. **Câu 2**: `你是怎么来北京的？` *(Hỏi phương thức di chuyển bằng `怎么` trong khung `是...的`).*
3. **Câu 3**: Bỏ `了`: `你们是在哪儿认识的？` *(Không dùng chung `了` và `是...的`).*
4. **Câu 4 trắc nghiệm**: Để nói ""Tôi chưa uống cà phê đâu"", câu đúng là:
   - *Đáp án*: `我还没喝咖啡呢。`
5. **Câu 5 trắc nghiệm**: Để phủ định ""Không phải tôi đi taxi đến"", câu đúng là:
   - *Đáp án*: `我不是坐出租车来的。`

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Cặp câu hỏi `吃饭了吗？` (đã ăn chưa — dùng `了`) vs `在哪儿吃的？` (ăn ở đâu — dùng `是...的`) là điểm ngữ pháp phân loại thí sinh giỏi ở cuối HSK1. Luyện ngay trong **Cloze & Translate** bên dưới nhé!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Phân biệt hai chức năng của trợ từ 了 (hoàn thành vs đổi trạng thái) và quy tắc bỏ 了 khi phủ định với 没.",
            "Làm chủ cấu trúc nhấn mạnh quá khứ 是...的 (thời gian, địa điểm, phương thức, người đi cùng)."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, ChineseChapter5Id, ChineseLesson53Id,
            "Bài 5.3: Đi Bệnh Viện, Đặt Vé, Phương Tiện Giao Thông & Đời Sống Tại Trung Quốc",
            "zh-hsk1-benh-vien-dat-ve-giao-thong", 3, 45,
            @"# Bài 5.3: Đi Bệnh Viện, Đặt Vé, Phương Tiện Giao Thông & Đời Sống Tại Trung Quốc

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Xin chúc mừng bạn đã đến với bài học tổng kết cuối cùng của khóa Tiếng Trung Giao Tiếp Sơ Cấp (HSK1+)! Bài học 5.3 trang bị cho bạn hành trang ngôn ngữ thực chiến để **tự tin xử lý 3 tình huống sống còn khi du học, công tác hoặc du lịch tại Trung Quốc**: đi khám bệnh khi trái gió trở trời, đặt vé tàu/máy bay/khách sạn, và sử dụng các phương tiện giao thông cùng cách nói thời gian di chuyển (`坐...要多长时间`). Sau bài học này, bạn sẽ:
- Làm chủ bộ từ vựng **Sức khỏe & Khám bệnh**: `生病` (bị ốm), `发烧` (sốt), `头疼` (đau đầu), `感冒` (cảm cúm), `吃药` (uống thuốc), `休息` (nghỉ ngơi), `挂号` (lấy số khám).
- Nắm chắc mẫu câu hỏi **Khoảng cách & Thời gian di chuyển**:
  - **`从 A 到 B (坐...) 要多长时间？`** *(Từ A đến B đi bằng... mất bao lâu?)*
  - **`离 A 远不远？`** *(Cách A có xa không?)*
- Sử dụng thành thạo **Số thứ tự và Thói quen đời sống số tại Trung Quốc**: `第一` (thứ nhất), đặt vé (`订机票 / 订酒店`), quét mã (`扫码`), tàu cao tốc (`高铁`), gọi xe (`打车`).
- Vận dụng linh hoạt **Câu cầu khiến lịch sự và lời khuyên**: `请 + V`, `多喝水` *(uống nhiều nước)*, `好好休息` *(nghỉ ngơi cho khỏe)*.

---

## 2. 🌱 Thực chiến 3 Tình huống Sống còn tại Trung Quốc

### 2.1. Hỏi đường dài, Khoảng cách (`离`) & Thời gian mất bao lâu (`要多长时间`)
1. **Khoảng cách từ A tới B có xa không?**
   - Công thức: **`A 离 B 远吗？`** *(hoặc **`A 离 B 远不远？`**)*
   - Chú ý chữ **`离 (lí)`** = cách (khoảng cách không gian). **KHÔNG dùng `从` cho khoảng cách tĩnh!**
   - Ví dụ: `学校离地铁站很近，走路五分钟就到了。` *(Trường cách trạm tàu điện ngầm rất gần, đi bộ 5 phút là tới rồi.)*
2. **Đi từ A đến B mất bao lâu?**
   - Công thức: **`从 A 到 B (+ Phương tiện) + 要多长时间？`**
   - Chữ **`从 (cóng)`** = từ điểm xuất phát, **`到 (dào)`** = đến điểm đích, **`要 (yào)`** = cần/mất.
   - Ví dụ: `从北京坐高铁到上海要五个小时。` *(Từ Bắc Kinh đi tàu cao tốc đến Thượng Hải mất 5 tiếng đồng hồ.)*
   - *Lưu ý kinh điển*: **1 tiếng** phải nói là **`一个小时`** (hoặc **`一小时`**), KHÔNG nói `一点钟` (đó là 1 giờ đồng hồ)!

### 2.2. Cách hỏi & nói Số thứ tự với chữ `第 (dì)`
- Muốn biến số đếm thành số thứ tự (thứ nhất, lần đầu, ga số 2): Chỉ cần đặt **`第`** trước số từ!
  - `第一次` *(lần đầu tiên)*, `第一天` *(ngày đầu tiên)*, `第二个路口` *(ngã tư thứ hai)*.
  - `这是我第一次来中国。` *(Đây là lần đầu tiên tôi đến Trung Quốc.)*

### 2.3. Giao tiếp trong Bệnh viện & Nhà thuốc (看病 — Kànbìng)
- Người Trung Quốc không nói *""uống thuốc""* là `喝药` hay `吃药`? Trong tiếng Trung phổ thông chuẩn dùng **`吃药 (chīyào)`** — chữ `吃` (ăn) đi với thuốc viên!
- Các mẫu câu bác sĩ & bệnh nhân hay dùng nhất:
  - `您哪儿不舒服？` *(Bác thấy khó chịu ở đâu?)*
  - `我有点儿发烧，头也很疼。` *(Tôi hơi sốt, đầu cũng rất đau.)*
  - `多喝热水，按时吃药，好好休息几天就好了。` *(Uống nhiều nước ấm, uống thuốc đúng giờ, nghỉ ngơi vài ngày là khỏi thôi.)*

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbiǎo)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **生病 / 感冒** | shēngbìng / gǎnmào | Sinh Bệnh / Cảm Mạo | Bị ốm / Bị cảm | 天气冷了，别感冒了！ *(Trời lạnh rồi, đừng để bị cảm nhé!)* |
| **发烧 / 头疼** | fāshāo / tóuténg | Phát Thiêu / Đầu Đông | Bị sốt / Đau đầu | 我昨天发烧了，今天头还很疼。 *(Hôm qua tôi sốt, nay đầu vẫn đau.)* |
| **吃药** | chīyào | Ngật Dược | Uống thuốc *(dùng 吃!)* | 医生说每天要吃三次药。 *(Bác sĩ bảo mỗi ngày uống thuốc 3 lần.)* |
| **休息** | xiūxi | Hưu Tức | Nghỉ ngơi | 你太累了，回家好好休息吧。 *(Bạn mệt quá rồi, về nhà nghỉ ngơi đi.)* |
| **身体** | shēntǐ | Thân Thể | Cơ thể, sức khỏe | 祝您身体健康！ *(Chúc ngài sức khỏe dồi dào!)* |
| **从...到...** | cóng... dào... | Tòng ... Đáo ... | Từ ... đến ... | 从这儿到机场要一个小时。 *(Từ đây ra sân bay mất 1 tiếng.)* |
| **离** | lí | Ly | Cách (khoảng cách) | 我家离公司不太远。 *(Nhà tôi cách công ty không xa lắm.)* |
| **近 / 远** | jìn / yuǎn | Cận / Viễn | Gần / Xa | 火车站离酒店很近。 *(Ga tàu cách khách sạn rất gần.)* |
| **小时** | xiǎoshí | Tiểu Thời | Tiếng đồng hồ (khoảng thời gian) | 我每天学习两个小时汉语。 *(Mỗi ngày tôi học tiếng Trung 2 tiếng.)* |
| **高铁** | gāotiě | Cao Thiết | Tàu cao tốc | 中国的高铁又快又舒服！ *(Tàu cao tốc Trung Quốc vừa nhanh vừa êm!)* |
| **打车** | dǎchē | Đả Xa | Bắt taxi / Đặt xe công nghệ | 下雨了，我们打车去吧。 *(Mưa rồi, tụi mình gọi taxi đi nhé.)* |
| **订 (机票/酒店)** | dìng | Đính | Đặt trước (vé máy bay / khách sạn) | 我已经在网上订好酒店了。 *(Tôi đã đặt xong khách sạn trên mạng rồi.)* |
| **第一** | dì-yī | Đệ Nhất | Thứ nhất, đầu tiên | 这是我第一次坐高铁。 *(Đây là lần đầu tôi đi tàu cao tốc.)* |
| **就** | jiù | Tựu | Thì, liền, ngay (nhấn mạnh nhanh) | 坐地铁二十分钟就到了。 *(Đi tàu điện ngầm 20 phút là tới ngay.)* |
| **多** | duō | Đa | Nhiều (đặt trước động từ khuyên nhủ) | 生病的时候要多喝水。 *(Lúc ốm phải uống nhiều nước vào.)* |

---

## 4. Ngữ pháp tổng kết: Phân biệt `时间点` vs `时间段` & Lời khuyên `多 + V`

### 4.1. Đừng nhầm lẫn chết người giữa **Thời điểm (`点`)** và **Khoảng thời gian (`个小时`)**
| Loại | Nghĩa | Dùng chữ gì? | Ví dụ chuẩn |
|---|---|---|---|
| **Thời điểm (Clock time)** | Mấy giờ trên đồng hồ? | **`点 (diǎn)`** | `现在两点。` *(Bây giờ là 2 giờ chiều.)* |
| **Thời lượng (Duration)** | Bao nhiêu tiếng đồng hồ? | **`个小时 (ge xiǎoshí)`** | `我坐了两个小时火车。` *(Tôi đã đi tàu 2 tiếng đồng hồ.)* |

### 4.2. Cấu trúc khuyên nhủ lịch sự: `多 + Động từ (+ Tân ngữ)`
- Khác với tiếng Việt đặt *""nhiều""* ở cuối (*""Ăn nhiều cơm vào, uống nhiều nước""*), tiếng Trung đặt **`多`** (hoặc `少` - ít) **TRƯỚC Động từ**:
  - `多喝水！` *(Uống nhiều nước vào!)* — KHÔNG nói `喝水多` ❌
  - `多吃水果！` *(Ăn nhiều hoa quả vào!)*
  - `少看手机，早点儿睡觉。` *(Ít xem điện thoại thôi, đi ngủ sớm chút đi.)*

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 我昨天发烧了，今天要喝药。 (Dùng 喝 cho thuốc viên theo tiếng Việt ""uống thuốc"")
   ⭕ ĐÚNG: 我昨天发烧了，今天要吃药。 (Dùng động từ 吃 với 药)
   💡 GIẢI THÍCH: Trong tiếng Trung phổ thông, uống thuốc dùng 吃药 (chīyào), không dùng 喝药.

2. ❌ SAI: 我学习汉语两点了。 (Dùng 点 để đếm khoảng thời gian 2 tiếng)
   ⭕ ĐÚNG: 我学习了两个小时汉语。 (Khoảng thời gian dùng 个小时)
   💡 GIẢI THÍCH: 两点 là 2 giờ đúng trên đồng hồ; 两个小时 là khoảng thời lượng 2 tiếng!

3. ❌ SAI: 我家从学校很近。 (Dùng 从 cho khoảng cách tĩnh)
   ⭕ ĐÚNG: 我家离学校很近。 (Khoảng cách A cách B dùng 离)
   💡 GIẢI THÍCH: 从 dùng với 到 cho hành trình di chuyển (从A到B); 离 dùng với 近/远 cho khoảng cách tĩnh (A离B很近).

4. ❌ SAI: 你生病了，喝水多吧！ (Đặt 多 sau danh từ 水)
   ⭕ ĐÚNG: 你生病了，多喝水吧！ (Phó từ 多/少 luôn đặt TRƯỚC động từ)
   💡 GIẢI THÍCH: ""Làm nhiều việc gì"" tiếng Trung nói là 多 + Động từ + Tân ngữ (多喝水, 多休息).
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Minh bị cảm sốt nhẹ khi đi công tác ở Thượng Hải nên gọi hỏi đồng nghiệp Trương Vĩ đường đến bệnh viện gần nhất và kế hoạch đặt vé tàu về Bắc Kinh.

- **Minh**: 张伟，我今天有点儿发烧，头也有点儿疼，想去医院看看。
  - *Pinyin*: Zhāng Wěi, wǒ jīntiān yǒudiǎnr fāshāo, tóu yě yǒudiǎnr téng, xiǎng qù yīyuàn kànkan.
  - *Phân tích*: `去医院看看` (đi bệnh viện khám thử).
  - *Dịch*: Trương Vĩ ơi, hôm nay mình hơi sốt, đầu cũng hơi đau, muốn đi bệnh viện khám xem sao.

- **Zhang Wei**: 啊？你感冒了吗？医院离你的酒店不远，打车十分钟就到了！
  - *Pinyin*: A? Nǐ gǎnmào le ma? Yīyuàn lí nǐ de jiǔdiàn bù yuǎn, dǎchē shí fēnzhōng jiù dào le!
  - *Phân tích*: `离...不远` (cách ... không xa) + `打车` (bắt taxi) + `就到了` (là tới ngay).
  - *Dịch*: Hả? Cậu bị cảm à? Bệnh viện cách khách sạn của cậu không xa đâu, gọi xe 10 phút là tới ngay!

- **Minh**: 好，那我现在就打车去。对了，明天从上海坐高铁回北京要多长时间？
  - *Pinyin*: Hǎo, nà wǒ xiànzài jiù dǎchē qù. Duìle, míngtiān cóng Shànghǎi zuò gāotiě huí Běijīng yào duō cháng shíjiān?
  - *Phân tích*: `从上海...回北京` + `要多长时间` (mất bao lâu?).
  - *Dịch*: Ừ, vậy giờ mình bắt xe đi luôn đây. À đúng rồi, mai từ Thượng Hải đi tàu cao tốc về Bắc Kinh mất bao lâu thế?

- **Zhang Wei**: 大概要四个半小时。你订好高铁票了吗？
  - *Pinyin*: Dàgài yào sì ge bàn xiǎoshí. Nǐ dìng hǎo gāotiěpiào le ma?
  - *Phân tích*: `四个半小时` (4 tiếng rưỡi) + `订好` (đặt xong).
  - *Dịch*: Mất khoảng 4 tiếng rưỡi đấy. Cậu đã đặt xong vé tàu cao tốc chưa?

- **Minh**: 订好了！这是我第一次坐中国的高铁，很期待。
  - *Pinyin*: Dìng hǎo le! Zhè shì wǒ dì-yī cì zuò Zhōngguó de gāotiě, hěn qīdài.
  - *Phân tích*: `第一次` (lần đầu tiên) + `期待` (mong chờ).
  - *Dịch*: Đặt xong rồi! Đây là lần đầu tiên mình đi tàu cao tốc của Trung Quốc, háo hức lắm.

- **Zhang Wei**: 今天看完医生记得按时吃药，多喝热水，好好休息啊！
  - *Pinyin*: Jīntiān kàn wán yīshēng jìde ànshí chīyào, duō hē rèshuǐ, hǎohao xiūxi a!
  - *Phân tích*: `按时吃药` (uống thuốc đúng giờ) + `多喝热水` (uống nhiều nước ấm).
  - *Dịch*: Nay khám bác sĩ xong nhớ uống thuốc đúng giờ, uống nhiều nước ấm và nghỉ ngơi cho khỏe nhé!

---

## 6. Bài tập tự luyện & Tổng ôn HSK1 cuối khóa

### Phần A: Luyện thực chiến bệnh viện & giao thông
1. Điền `离` hoặc `从`: `___ 酒店到机场开车要四十分钟。` / `我家 ___ 医院很近。`
2. Sửa lỗi sai: `医生让我吃水果多，和喝药按时。`
3. Dịch sang tiếng Trung: *""Đây là lần đầu tiên tôi đi tàu cao tốc.""*

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: Câu đầu điền **`从`** (`从...到...`); câu sau điền **`离`** (`A 离 B 很近`).
2. **Câu 2**: `医生让我多吃水果，按时吃药。` *(Đặt `多` trước `吃水果`, bỏ `和`, và dùng `吃药` thay cho `喝药`).*
3. **Câu 3**: `这是我第一次坐高铁。`
4. **Câu 4 trắc nghiệm**: Câu nào diễn đạt ĐÚNG ""Tôi học tiếng Trung 2 tiếng đồng hồ mỗi ngày""?
   - *Đáp án*: `我每天学习两个小时汉语。` *(Dùng `两个小时`, không dùng `两点`).*
5. **Câu 5 trắc nghiệm**: Để hỏi ""Từ đây đến ga tàu đi bộ mất bao lâu?"", câu chuẩn nhất là:
   - *Đáp án*: `从这儿走路到火车站要多长时间？`

</details>

> [!TIP]
> **Chặng cuối HSK1:** Sau khi hoàn tất 8 hoạt động thực hành ở Bài 5.3, hãy mở ngay **`[Đề Chương 5]`** để kiểm tra toàn bộ ngữ pháp Trợ động từ năng nguyện, Câu chữ `是...的`, Trợ từ `了` và Giao tiếp đời sống tại Trung Quốc!

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Ứng dụng thực chiến 3 tình huống sống còn: khám bệnh (发烧, 吃药), đặt vé (订票) và di chuyển (离, 从...到...).",
            "Phân biệt thời điểm (点) vs thời lượng (个小时), số thứ tự 第 và cấu trúc khuyên nhủ 多 + Động từ.")
    };

    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG HSK1+ — HOẠT ĐỘNG THỰC HÀNH (8 LOẠI x 6 BÀI = 48 ACTIVITIES)
    // =========================================================================

    private static List<PracticeActivity> GetChineseExpandedActivities(Guid cId, DateTimeOffset now)
    {
        var all = new List<PracticeActivity>();

        // ---------------- LESSON 4.1 ----------------
        all.AddRange(BuildLessonPack(cId, ChineseLesson41Id, "zh-CN", "Trung HSK1 · Bài 4.1",
            shadowing: new[]
            {
                new ShItem("zh-l41-sh-1", "easy", "Hỏi thời tiết (Dễ)", "今天天气怎么样？", "jīntiān tiānqì zěnmeyàng?", "Thời tiết hôm nay thế nào?", new[] { "天气", "怎么样" }),
                new ShItem("zh-l41-sh-2", "easy", "Cảm thán quá nóng (Dễ)", "今天太热了！", "jīntiān tài rè le!", "Hôm nay nóng quá rồi!", new[] { "太热了" }),
                new ShItem("zh-l41-sh-3", "easy", "Hơi lạnh (Dễ)", "今天有点儿冷。", "jīntiān yǒudiǎnr lěng", "Hôm nay trời hơi lạnh.", new[] { "有点儿", "冷" }),
                new ShItem("zh-l41-sh-4", "medium", "Phủ định 不太 + Tính từ (Vừa)", "今天不太冷，很舒服。", "jīntiān bú tài lěng, hěn shūfu", "Hôm nay không lạnh lắm, rất dễ chịu.", new[] { "不太冷", "舒服" }),
                new ShItem("zh-l41-sh-5", "medium", "Mùa thu mát mẻ (Vừa)", "我最喜欢秋天，很凉快。", "wǒ zuì xǐhuan qiūtiān, hěn liángkuai", "Tôi thích mùa thu nhất, rất mát mẻ.", new[] { "秋天", "凉快" }),
                new ShItem("zh-l41-sh-6", "medium", "Động từ ly hợp 下大雨 (Vừa)", "今天下午下大雨了。", "jīntiān xiàwǔ xià dàyǔ le", "Chiều nay trời đổ mưa to rồi.", new[] { "下大雨" }),
                new ShItem("zh-l41-sh-7", "hard", "Phân biệt 一点儿 (Khó)", "外面很热，请给我一点儿冰水。", "wàimiàn hěn rè, qǐng gěi wǒ yìdiǎnr bīngshuǐ", "Ngoài trời nóng lắm, cho tôi chút nước đá nhé.", new[] { "一点儿", "冰水" }),
                new ShItem("zh-l41-sh-8", "hard", "Mùa đông Bắc Kinh (Khó)", "北京的冬天非常冷，经常下雪。", "Běijīng de dōngtiān fēicháng lěng, jīngcháng xiàxuě", "Mùa đông Bắc Kinh vô cùng lạnh, hay có tuyết rơi.", new[] { "非常冷", "下雪" }),
                new ShItem("zh-l41-sh-9", "hard", "Nhắc mang ô khi trời mưa (Khó)", "外面刮风了，别忘了带雨伞。", "wàimiàn guāfēng le, bié wàng le dài yǔsǎn", "Ngoài trời nổi gió rồi, đừng quên mang ô nhé.", new[] { "刮风", "雨伞" })
            },
            dictation: new[]
            {
                new DiItem("zh-l41-di-1", "easy", "Thời tiết hôm nay", "今天天气很好", "jīntiān tiānqì hěn hǎo", "Hôm nay thời tiết rất đẹp.", new[] { "今天", "天气", "很", "好" }, new[] { "是", "太", "吗" }),
                new DiItem("zh-l41-di-2", "easy", "Trời hơi lạnh", "今天有点儿冷", "jīntiān yǒudiǎnr lěng", "Hôm nay hơi lạnh.", new[] { "今天", "有点儿", "冷" }, new[] { "一点儿", "热", "是" }),
                new DiItem("zh-l41-di-3", "medium", "Không nóng lắm", "秋天不太热", "qiūtiān bú tài rè", "Mùa thu không nóng lắm.", new[] { "秋天", "不太", "热" }, new[] { "了", "夏天", "冷" }),
                new DiItem("zh-l41-di-4", "medium", "Thời tiết ngày mai", "明天天气怎么样", "míngtiān tiānqì zěnmeyàng", "Thời tiết ngày mai thế nào?", new[] { "明天", "天气", "怎么样" }, new[] { "什么", "好", "吗" }),
                new DiItem("zh-l41-di-5", "hard", "Mưa to rồi", "下午下大雨了", "xiàwǔ xià dàyǔ le", "Buổi chiều trời mưa to rồi.", new[] { "下午", "下", "大雨", "了" }, new[] { "很大", "下雪", "不" }),
                new DiItem("zh-l41-di-6", "hard", "Cho tôi một chút nước", "请给我一点儿水", "qǐng gěi wǒ yìdiǎnr shuǐ", "Làm ơn cho tôi một chút nước.", new[] { "请", "给我", "一点儿", "水" }, new[] { "有点儿", "茶", "喝" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l41-pr-1", "easy", "Thanh điệu 天气 (tiānqì)", "今天天气太好了", "jīntiān tiānqì tài hǎo le", "Hôm nay thời tiết tuyệt quá!", new[] { "天气", "太好" }, "Giữ thanh 1 cao phẳng cho tiān và bật hơi rơi mạnh ở qì.", new[] { "tiān cao phẳng 55", "qì bật hơi q thanh 4", "tài bật hơi t thanh 4" }, "今天天气太好了！", new[] { "天气", "太好了" }),
                new PrItem("zh-l41-pr-2", "easy", "Âm uốn lưỡi 热 (rè)", "夏天非常热", "xiàtiān fēicháng rè", "Mùa hè vô cùng nóng.", new[] { "夏", "非常", "热" }, "Cong đầu lưỡi nhẹ và rung thanh quản cho rè (thanh 4 rơi gọn).", new[] { "fēi thanh 1 cao", "cháng uốn lưỡi bật hơi thanh 2", "rè uốn lưỡi thanh 4" }, "河内的夏天非常热。", new[] { "夏天", "非常热" }),
                new PrItem("zh-l41-pr-3", "medium", "Âm cong lưỡi 有点儿", "今天有点儿冷", "jīntiān yǒudiǎnr lěng", "Hôm nay hơi lạnh.", new[] { "有点儿", "冷" }, "Áp dụng biến điệu thanh 3 giữa yǒu và diǎnr rồi cuộn nhẹ âm r cuối.", new[] { "yǒu đọc thành thanh 2 (yóu)", "diǎnr hạ trầm rồi cuộn lưỡi r", "lěng hạ sâu thanh 3" }, "今天风有点儿大，有点儿冷。", new[] { "有点儿", "冷" }),
                new PrItem("zh-l41-pr-4", "medium", "Từ vựng 舒服 (shūfu)", "秋天的天气很舒服", "qiūtiān de tiānqì hěn shūfu", "Thời tiết mùa thu rất dễ chịu.", new[] { "秋", "舒服" }, "Bật hơi q (qiū), uốn lưỡi cao phẳng shū, chữ fu đọc thanh nhẹ.", new[] { "qiū bật hơi q thanh 1", "shū uốn lưỡi thanh 1", "fu thả lỏng thanh nhẹ" }, "秋天的天气真舒服！", new[] { "秋天", "舒服" }),
                new PrItem("zh-l41-pr-5", "hard", "Biến điệu 不太 (bú tài)", "这里不太冷", "zhèlǐ bú tài lěng", "Ở đây không lạnh lắm.", new[] { "这", "不太", "冷" }, "Chữ 不 đứng trước tài (thanh 4) biến điệu thành bú (thanh 2).", new[] { "zhè uốn lưỡi thanh 4", "bú lên giọng thanh 2", "tài bật hơi rơi thanh 4" }, "河内的冬天不太冷。", new[] { "不太", "冷" }),
                new PrItem("zh-l41-pr-6", "hard", "Phân biệt 下雨 vs 下雪", "昨天刮风又下雪", "zuótiān guāfēng yòu xiàxuě", "Hôm qua vừa gió vừa có tuyết.", new[] { "刮风", "下雪" }, "Tròn môi ở âm xuě (chữ ü ẩn dấu chấm sau x).", new[] { "guā thanh 1 cao phẳng", "xià thanh 4 dứt khoát", "xuě tròn môi thanh 3" }, "北京的冬天经常刮风下雪。", new[] { "刮风", "下雪" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l41-wm-1", "easy", "Sơ đồ từ vựng: 天气 (Thời tiết)", "天气", "tiānqì", "Thời tiết",
                    new[] { new WmNode("天气怎么样", "tiānqì zěnmeyàng", "Thời tiết thế nào?", "明天天气怎么样？"), new WmNode("天气预报", "tiānqì yùbào", "Dự báo thời tiết", "我看天气预报了。") },
                    new[] { new WmNode("晴天 / 阴天", "qíngtiān / yīntiān", "Trời nắng / Trời râm", "今天是晴天。") },
                    new WmNode[] { new WmNode("下雨 / 下雪", "xiàyǔ / xiàxuě", "Mưa / Tuyết rơi", "今天下大雨了。") },
                    new[] { new WmEx("今天天气真舒服，我们去公园散步吧！", "jīntiān tiānqì zhēn shūfu, wǒmen qù gōngyuán sànbù ba", "Thời tiết hôm nay dễ chịu quá, mình ra công viên đi dạo nhé!") }),
                new WmItem("zh-l41-wm-2", "medium", "Sơ đồ từ vựng: 太...了 (Quá... rồi)", "太...了", "tài... le", "Quá ... rồi! (Cảm thán)",
                    new[] { new WmNode("太好了", "tài hǎo le", "Tuyệt quá!", "太好了，明天是晴天！"), new WmNode("太热了", "tài rè le", "Nóng quá rồi!", "今天太热了！") },
                    new[] { new WmNode("不太 + Tính từ", "bú tài...", "Không ... lắm (bỏ 了)", "今天不太冷。") },
                    new WmNode[] { new WmNode("非常 / 真", "fēicháng / zhēn", "Vô cùng / Thật là", "春天真漂亮！") },
                    new[] { new WmEx("这件衣服太贵了，有没有便宜一点儿的？", "zhè jiàn yīfu tài guì le, yǒu méiyǒu piányi yìdiǎnr de", "Chiếc áo này đắt quá, có cái nào rẻ hơn chút không?") }),
                new WmItem("zh-l41-wm-3", "hard", "Sơ đồ từ vựng: 有点儿 (Hơi...)", "有点儿", "yǒudiǎnr", "Hơi ... (đứng trước Tính từ)",
                    new[] { new WmNode("有点儿冷", "yǒudiǎnr lěng", "Hơi lạnh", "外面风大，有点儿冷。"), new WmNode("有点儿累", "yǒudiǎnr lèi", "Hơi mệt", "我今天工作有点儿累。") },
                    new WmNode[] { new WmNode("一点儿 + Danh từ", "yìdiǎnr...", "Một chút (đồ vật)", "请给我一点儿热茶。") },
                    new WmNode[] { new WmNode("非常 / 很", "fēicháng / hěn", "Rất / Vô cùng", "今天非常凉快。") },
                    new[] { new WmEx("这个菜有点儿辣，但我很喜欢吃。", "zhège cài yǒudiǎnr là, dàn wǒ hěn xǐhuan chī", "Món này hơi cay một chút nhưng tôi rất thích ăn.") })
            },
            cloze: new[]
            {
                new ClItem("zh-l41-cl-1", "easy", "Hỏi nhận xét 怎么样", "Điền từ hỏi 'như thế nào' vào câu hỏi thời tiết:", "北京今天的天气{{0}}？", new[] { new ClBlank(new[] { "怎么样", "zěnmeyàng", "zen3 me yang4" }, "như thế nào (zěnmeyàng)", "Chủ đề + 怎么样？ dùng để hỏi tình trạng/nhận xét.") }, "北京今天的天气怎么样？", "Běijīng jīntiān de tiānqì zěnmeyàng?", "Thời tiết Bắc Kinh hôm nay thế nào?", "Không dùng 什么 cho câu hỏi tính chất."),
                new ClItem("zh-l41-cl-2", "easy", "Phủ định của 太...了", "Hoàn thiện câu 'Hôm nay không nóng lắm':", "今天{{0}}热。", new[] { new ClBlank(new[] { "不太", "bú tài", "bu2 tai4" }, "không ... lắm (bú tài)", "Phủ định 太...了 dùng 不太 và bỏ 了.") }, "今天不太热。", "jīntiān bú tài rè", "Hôm nay không nóng lắm.", "Không thêm 了 cuối câu 不太 + Tính từ."),
                new ClItem("zh-l41-cl-3", "medium", "Phân biệt 有点儿 và 一点儿", "Điền đúng 有点儿 (trước tính từ) và 一点儿 (trước danh từ):", "我{{0}}渴，想喝{{1}}水。", new[] { new ClBlank(new[] { "有点儿", "yǒudiǎnr" }, "hơi (trước tính từ 渴)", "有点儿 đứng trước tính từ."), new ClBlank(new[] { "一点儿", "yìdiǎnr" }, "một chút (trước danh từ 水)", "一点儿 đứng sau động từ / trước danh từ.") }, "我有点儿渴，想喝一点儿水。", "wǒ yǒudiǎnr kě, xiǎng hē yìdiǎnr shuǐ", "Tôi hơi khát, muốn uống một chút nước.", "Quy tắc vàng: 有点儿 + Adj | V + 一点儿 + N."),
                new ClItem("zh-l41-cl-4", "hard", "Động từ ly hợp 下大雨", "Điền cụm từ 'mưa to' đúng trật tự động từ ly hợp:", "今天下午{{0}}了。", new[] { new ClBlank(new[] { "下大雨", "xià dàyǔ" }, "mưa to (xià dàyǔ)", "Tính từ 大 chèn vào giữa 下 và 雨.") }, "今天下午下大雨了。", "jīntiān xiàwǔ xià dàyǔ le", "Chiều nay trời mưa to rồi.", "Không nói 下雨很大.")
            },
            translate: new[]
            {
                new TrItem("zh-l41-tr-1", "easy", "Hỏi thời tiết", "Thời tiết hôm nay thế nào?", new[] { "今天", "天气", "怎么样" }, new[] { "今天天气怎么样？", "今天的天气怎么样？", "jīntiān tiānqì zěnmeyàng" }, "今天天气怎么样？", "jīntiān tiānqì zěnmeyàng?", "Đặt 怎么样 ở cuối câu hỏi."),
                new TrItem("zh-l41-tr-2", "easy", "Cảm thán nóng quá", "Hôm nay nóng quá rồi!", new[] { "今天", "太热了" }, new[] { "今天太热了！", "今天太热了", "jīntiān tài rè le" }, "今天太热了！", "jīntiān tài rè le!", "Cấu trúc cảm thán: 太 + Tính từ + 了."),
                new TrItem("zh-l41-tr-3", "medium", "Hơi lạnh nhưng không mưa", "Hôm nay hơi lạnh nhưng không mưa.", new[] { "有点儿冷", "没下雨" }, new[] { "今天有点儿冷，但是没下雨。", "今天有点儿冷，没下雨。", "jīntiān yǒudiǎnr lěng, dànshì méi xiàyǔ" }, "今天有点儿冷，但是没下雨。", "jīntiān yǒudiǎnr lěng, dànshì méi xiàyǔ.", "Có nhưng dùng 但是, hơi lạnh dùng 有点儿冷."),
                new TrItem("zh-l41-tr-4", "hard", "Thích mùa thu mát mẻ", "Tôi thích mùa thu nhất, rất mát mẻ và dễ chịu.", new[] { "最喜欢", "秋天", "凉快", "舒服" }, new[] { "我最喜欢秋天，很凉快也很舒服。", "我最喜欢秋天，非常凉快舒服。" }, "我最喜欢秋天，很凉快也很舒服。", "wǒ zuì xǐhuan qiūtiān, hěn liángkuai yě hěn shūfu.", "最喜欢 = thích nhất.")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l41-gf-1", "easy", "Dùng thừa 了 trong câu phủ định 不太", "今天不太冷了。", "Câu phủ định bình thường của 太...了 là 不太 + Tính từ (không có 了).", "今天不太冷。", new[] { "今天不太冷。", "今天不太冷" }, "jīntiān bú tài lěng", "Hôm nay không lạnh lắm.", "Bỏ 了 cuối câu khi dùng 不太."),
                new GfItem("zh-l41-gf-2", "medium", "Nhầm 一点儿 trước tính từ", "昨天北京一点儿冷。", "Đứng trước tính từ 冷 phải dùng 有点儿 (không dùng 一点儿).", "昨天北京有点儿冷。", new[] { "昨天北京有点儿冷。", "昨天北京有点儿冷" }, "zuótiān Běijīng yǒudiǎnr lěng", "Hôm qua Bắc Kinh hơi lạnh.", "有点儿 + Tính từ; Động từ + 一点儿 + Danh từ."),
                new GfItem("zh-l41-gf-3", "hard", "Sai trật tự động từ ly hợp 下雨", "今天下雨很大。", "Tính từ 大 phải chèn vào giữa động từ ly hợp 下 và 雨.", "今天下大雨了。", new[] { "今天下大雨了。", "今天下大雨。" }, "jīntiān xià dàyǔ le", "Hôm nay trời mưa to rồi.", "下大雨 = mưa to; 下小雨 = mưa nhỏ.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l41-rp-1", "easy", "Hỏi thăm thời tiết Bắc Kinh", "Bạn gọi điện hỏi bạn Trung Quốc xem thời tiết hôm nay thế nào.", "Bạn Trương Vĩ", "喂，明！你那儿今天天气怎么样？", "wéi, Míng! nǐ nàr jīntiān tiānqì zěnmeyàng?", "A lô Minh! Chỗ cậu hôm nay thời tiết thế nào?", "Trả lời thời tiết dùng 很/太...了/有点儿/不太 cùng tính từ (热, 冷, 凉快, 舒服, 下雨).", new[] { "热|冷|凉快|舒服|下雨|晴天|re|leng", "很|太|有点儿|非常|不太" }, new[] { "今天天气非常舒服，不太热也不太冷。" }, "jīntiān tiānqì fēicháng shūfu, bú tài rè yě bú tài lěng.", 6),
                new RpItem("zh-l41-rp-2", "medium", "Chọn mùa yêu thích", "Cô giáo hỏi bạn thích mùa nào nhất trong năm và vì sao.", "Cô Lý", "一年有四个季节，你最喜欢哪个季节？为什么？", "yì nián yǒu sì ge jìjié, nǐ zuì xǐhuan nǎge jìjié? wèishénme?", "Một năm có 4 mùa, em thích mùa nào nhất? Tại sao?", "Nêu mùa bạn thích (春天/夏天/秋天/冬天) và mô tả thời tiết mùa đó.", new[] { "喜欢|xi huan", "春天|夏天|秋天|冬天", "凉快|暖和|舒服|好看|热|冷" }, new[] { "我最喜欢秋天，因为秋天不太热，非常凉快。" }, "wǒ zuì xǐhuan qiūtiān, yīnwèi qiūtiān bú tài rè, fēicháng liángkuai.", 8),
                new RpItem("zh-l41-rp-3", "hard", "Gọi đồ khi trời nóng", "Ngoài trời rất nóng, bạn vào quán uống nước giải khát.", "Nhân viên quán", "您好，外面太热了！您想喝点儿什么？", "nínhǎo, wàimiàn tài rè le! nín xiǎng hē diǎnr shénme?", "Chào anh, ngoài trời nóng quá! Anh muốn uống chút gì ạ?", "Nói trời hơi nóng/quá nóng (有点儿热/太热了) và xin chút nước lạnh/trà (一点儿冰水/一杯茶).", new[] { "有点儿|太|很", "一点儿|一杯", "水|茶|咖啡|冰" }, new[] { "外面太热了！请给我一杯冰水，谢谢！" }, "wàimiàn tài rè le! qǐng gěi wǒ yì bēi bīngshuǐ, xièxie!", 8)
            }, now));

        // ---------------- LESSON 4.2 ----------------
        all.AddRange(BuildLessonPack(cId, ChineseLesson42Id, "zh-CN", "Trung HSK1 · Bài 4.2",
            shadowing: new[]
            {
                new ShItem("zh-l42-sh-1", "easy", "Nhân khẩu gia đình (Dễ)", "我家有五口人。", "wǒ jiā yǒu wǔ kǒu rén", "Nhà tôi có 5 người.", new[] { "五口人" }),
                new ShItem("zh-l42-sh-2", "easy", "Lượng từ tấm ảnh 张 (Dễ)", "这是一张全家福照片。", "zhè shì yì zhāng quánjiāfú zhàopiàn", "Đây là một bức ảnh đại gia đình.", new[] { "一张", "照片" }),
                new ShItem("zh-l42-sh-3", "easy", "Hai con mèo nhỏ (Dễ)", "我家有两只小猫。", "wǒ jiā yǒu liǎng zhī xiǎomāo", "Nhà tôi có hai chú mèo con.", new[] { "两只", "小猫" }),
                new ShItem("zh-l42-sh-4", "medium", "Quyển từ điển Hán-Việt (Vừa)", "我买了两本汉越词典。", "wǒ mǎi le liǎng běn Hàn-Yuè cídiǎn", "Tôi đã mua 2 cuốn từ điển Hán-Việt.", new[] { "两本", "词典" }),
                new ShItem("zh-l42-sh-5", "medium", "Chiếc áo mới (Vừa)", "妈妈给我买了一件新衣服。", "māma gěi wǒ mǎi le yí jiàn xīn yīfu", "Mẹ đã mua cho tôi một chiếc áo mới.", new[] { "一件", "衣服" }),
                new ShItem("zh-l42-sh-6", "medium", "Lược 的 trong gia đình (Vừa)", "这是我哥哥的电脑。", "zhè shì wǒ gēge de diànnǎo", "Đây là máy tính của anh trai tôi.", new[] { "我哥哥", "的电脑" }),
                new ShItem("zh-l42-sh-7", "hard", "Hỏi số lượng lớp học (Khó)", "你们班有多少个学生？", "nǐmen bān yǒu duōshao ge xuésheng?", "Lớp của các bạn có bao nhiêu học sinh?", new[] { "多少个", "学生" }),
                new ShItem("zh-l42-sh-8", "hard", "Ông bà nội (Khó)", "爷爷和奶奶身体都很好。", "yéye hé nǎinai shēntǐ dōu hěn hǎo", "Ông nội và bà nội sức khỏe đều rất tốt.", new[] { "爷爷", "奶奶", "都" }),
                new ShItem("zh-l42-sh-9", "hard", "Phân biệt 两 và 二 (Khó)", "桌子上放着两张火车票和十二块钱。", "zhuōzi shàng fàngzhe liǎng zhāng huǒchēpiào hé shí'èr kuài qián", "Trên bàn đặt 2 tấm vé tàu hỏa và 12 đồng tệ.", new[] { "两张", "十二" })
            },
            dictation: new[]
            {
                new DiItem("zh-l42-di-1", "easy", "Nhà tôi có 4 người", "我家有四口人", "wǒ jiā yǒu sì kǒu rén", "Nhà tôi có 4 người.", new[] { "我家", "有", "四口人" }, new[] { "个", "二", "的" }),
                new DiItem("zh-l42-di-2", "easy", "Hai quyển sách", "我买了两本书", "wǒ mǎi le liǎng běn shū", "Tôi đã mua 2 quyển sách.", new[] { "我", "买了", "两本", "书" }, new[] { "二", "张", "口" }),
                new DiItem("zh-l42-di-3", "medium", "Hai tấm vé xem phim", "这是两张电影票", "zhè shì liǎng zhāng diànyǐng piào", "Đây là 2 tấm vé xem phim.", new[] { "这是", "两张", "电影票" }, new[] { "二张", "本", "件" }),
                new DiItem("zh-l42-di-4", "medium", "Chiếc áo này đẹp", "这件衣服很好看", "zhè jiàn yīfu hěn hǎokàn", "Chiếc áo này rất đẹp.", new[] { "这件", "衣服", "很", "好看" }, new[] { "张", "本", "是" }),
                new DiItem("zh-l42-di-5", "hard", "Mèo và chó", "家里有一只猫和一只狗", "jiālǐ yǒu yì zhī māo hé yì zhī gǒu", "Trong nhà có một con mèo và một con chó.", new[] { "家里", "有", "一只猫", "和一只狗" }, new[] { "个", "张", "本" }),
                new DiItem("zh-l42-di-6", "hard", "Sách của chị gái", "那是我姐姐的汉语词典", "nà shì wǒ jiějie de Hànyǔ cídiǎn", "Kia là cuốn từ điển tiếng Hán của chị gái tôi.", new[] { "那是", "我姐姐的", "汉语词典" }, new[] { "我的姐姐", "张", "本" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l42-pr-1", "easy", "Âm 两 (liǎng) & 口 (kǒu)", "我家有两口人", "wǒ jiā yǒu liǎng kǒu rén", "Nhà tôi có hai người.", new[] { "两", "口", "人" }, "Biến điệu thanh 3 nối tiếp giữa yǒu và liǎng rồi bật hơi nhẹ kǒu.", new[] { "yǒu → thanh 2", "liǎng → thanh 2", "kǒu bật hơi k thanh 3" }, "我们家只有两口人。", new[] { "两口人" }),
                new PrItem("zh-l42-pr-2", "easy", "Uốn lưỡi 张 (zhāng)", "桌子上有三张纸", "zhuōzi shàng yǒu sān zhāng zhǐ", "Trên bàn có 3 tờ giấy.", new[] { "桌", "张", "纸" }, "Uốn lưỡi rõ cả zhuō, zhāng và zhǐ (không bật hơi).", new[] { "zhuō uốn lưỡi thanh 1", "zhāng uốn lưỡi thanh 1", "zhǐ uốn lưỡi thanh 3" }, "桌子上有两张照片。", new[] { "两张", "照片" }),
                new PrItem("zh-l42-pr-3", "medium", "Biến điệu 一 (yí) trước thanh 4", "这是一件新衣服", "zhè shì yí jiàn xīn yīfu", "Đây là một chiếc áo mới.", new[] { "这", "一件", "衣服" }, "Chữ 一 đứng trước jiàn (thanh 4) đổi thành thanh 2: yí jiàn.", new[] { "yí lên giọng thanh 2", "jiàn rơi mạnh thanh 4", "yīfu thanh 1 + nhẹ" }, "我想买一件新衣服。", new[] { "一件", "衣服" }),
                new PrItem("zh-l42-pr-4", "medium", "Uốn lưỡi 只 (zhī)", "我有一只小狗", "wǒ yǒu yì zhī xiǎogǒu", "Tôi có một chú cún nhỏ.", new[] { "一", "只", "小狗" }, "Lượng từ 只 đọc thanh 1 cao phẳng uốn lưỡi zhī, chữ 一 phía trước đổi thành yì (thanh 4).", new[] { "yì thanh 4", "zhī uốn lưỡi thanh 1 cao phẳng", "xiǎogǒu biến điệu 2 + 3" }, "我家有两只小猫和一只小狗。", new[] { "两只", "一只" }),
                new PrItem("zh-l42-pr-5", "hard", "Từ vựng 词典 (cídiǎn)", "两本汉越词典", "liǎng běn Hàn-Yuè cídiǎn", "Hai cuốn từ điển Hán-Việt.", new[] { "两", "本", "词典" }, "Bật hơi đầu lưỡi thẳng ở cí (thanh 2) và đọc rõ diǎn.", new[] { "liǎng → thanh 2", "běn thanh 3 trầm", "cí bật hơi đầu lưỡi thẳng" }, "我买了两本汉越词典。", new[] { "两本", "词典" }),
                new PrItem("zh-l42-pr-6", "hard", "Cặp 女儿 (nǚ'ér)", "她有两个女儿", "tā yǒu liǎng ge nǚ'ér", "Cô ấy có hai cô con gái.", new[] { "女儿", "两" }, "Tròn môi ở âm ü của nǚ và cuộn nhẹ âm ér thanh 2.", new[] { "nǚ tròn môi thanh 3", "ér uốn lưỡi đi lên thanh 2", "Tách nhẹ nǚ và ér" }, "王老师有一个儿子和一个女儿。", new[] { "儿子", "女儿" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l42-wm-1", "easy", "Sơ đồ từ vựng: 张 / 本 / 件 (Lượng từ)", "量词", "zhāng / běn / jiàn", "Lượng từ thực chiến HSK1",
                    new[] { new WmNode("一张照片 / 票", "yì zhāng zhàopiàn / piào", "1 bức ảnh / tấm vé", "桌子上有两张票。"), new WmNode("两本书 / 词典", "liǎng běn shū / cídiǎn", "2 quyển sách / từ điển", "我买了两本词典。") },
                    new[] { new WmNode("一件衣服", "yí jiàn yīfu", "1 chiếc áo", "这件衣服多少钱？") },
                    new WmNode[] { new WmNode("两只猫 / 五口人", "liǎng zhī māo / wǔ kǒu rén", "2 con mèo / 5 người nhà", "我家有五口人。") },
                    new[] { new WmEx("我书包里有两本书和一张照片。", "wǒ shūbāo lǐ yǒu liǎng běn shū hé yì zhāng zhàopiàn", "Trong cặp sách tôi có 2 quyển sách và 1 tấm ảnh.") }),
                new WmItem("zh-l42-wm-2", "medium", "Sơ đồ từ vựng: 两 vs 二 (Số 2)", "两 / 二", "liǎng / èr", "Số hai (trước lượng từ vs số đếm)",
                    new[] { new WmNode("两个人 / 两本书", "liǎng ge rén / liǎng běn shū", "2 người / 2 quyển sách", "我有两本汉语书。"), new WmNode("两点半", "liǎng diǎn bàn", "2 giờ rưỡi", "下午两点半见。") },
                    new WmNode[] { new WmNode("十二 / 二十", "shí'èr / èrshí", "12 / 20", "我有十二块钱。") },
                    new WmNode[] { new WmNode("星期二 / 二月", "Xīngqī'èr / Èryuè", "Thứ Ba / Tháng 2", "今天是二月二号。") },
                    new[] { new WmEx("我有十二个学生，其中两个是越南人。", "wǒ yǒu shí'èr ge xuésheng, qízhōng liǎng ge shì Yuènán rén", "Tôi có 12 học sinh, trong đó có 2 người là người Việt.") }),
                new WmItem("zh-l42-wm-3", "hard", "Sơ đồ từ vựng: 的 (Sở hữu & Lược bỏ)", "的", "de", "Của (trợ từ sở hữu)",
                    new[] { new WmNode("我的手机", "wǒ de shǒujī", "Điện thoại của tôi", "这是我的手机。"), new WmNode("我爸爸妈妈", "wǒ bàba māma", "Bố mẹ tôi (lược 的)", "我爸爸妈妈都很健康。") },
                    new WmNode[] { new WmNode("我哥哥的朋友", "wǒ gēge de péngyou", "Bạn của anh trai tôi", "他是我哥哥的好朋友。") },
                    new WmNode[] { new WmNode("谁的？", "shéi de?", "Của ai?", "这本书是谁的？") },
                    new[] { new WmEx("我妹妹的衣服非常漂亮。", "wǒ mèimei de yīfu fēicháng piàoliang", "Quần áo của em gái tôi vô cùng đẹp.") })
            },
            cloze: new[]
            {
                new ClItem("zh-l42-cl-1", "easy", "Dùng 两 trước lượng từ", "Điền chữ số 2 đúng trước lượng từ 本:", "我买了{{0}}本汉语词典。", new[] { new ClBlank(new[] { "两", "liǎng", "liang3" }, "hai (liǎng)", "Trước lượng từ luôn dùng 两, không dùng 二.") }, "我买了两本汉语词典。", "wǒ mǎi le liǎng běn Hànyǔ cídiǎn", "Tôi đã mua 2 cuốn từ điển tiếng Hán.", "Chỉ trong 12, 20... hoặc thứ tự mới dùng 二."),
                new ClItem("zh-l42-cl-2", "easy", "Lượng từ cho ảnh/vé/bàn 张", "Điền lượng từ đúng cho 'bức ảnh' và 'bộ quần áo':", "这是一{{0}}照片，那是一{{1}}新衣服。", new[] { new ClBlank(new[] { "张", "zhāng", "zhang1" }, "tấm, bức (zhāng)", "Ảnh, vé, giấy, bàn dùng 张."), new ClBlank(new[] { "件", "jiàn", "jian4" }, "chiếc (jiàn)", "Quần áo dùng 件.") }, "这是一张照片，那是一件新衣服。", "zhè shì yì zhāng zhàopiàn, nà shì yí jiàn xīn yīfu", "Đây là một bức ảnh, kia là một chiếc áo mới.", "Nhớ phân biệt 张 (mặt phẳng) và 件 (quần áo)."),
                new ClItem("zh-l42-cl-3", "medium", "Lượng từ động vật nhỏ 只", "Điền lượng từ cho mèo con và nhân khẩu gia đình:", "我家有四{{0}}人，还有两{{1}}小猫。", new[] { new ClBlank(new[] { "口", "kǒu", "kou3" }, "khẩu (đếm người nhà)", "Đếm thành viên gia đình dùng 口."), new ClBlank(new[] { "只", "zhī", "zhi1" }, "con (động vật nhỏ)", "Động vật nhỏ như mèo, chó dùng 只.") }, "我家有四口人，还有两只小猫。", "wǒ jiā yǒu sì kǒu rén, hái yǒu liǎng zhī xiǎomāo", "Nhà tôi có 4 người, còn có 2 chú mèo con.", "Lượng từ làm câu tiếng Trung chính xác tự nhiên."),
                new ClItem("zh-l42-cl-4", "hard", "Hỏi số lượng 几 vs 多少", "Điền từ hỏi 'bao nhiêu' cho lớp học đông học sinh:", "你们学校有{{0}}个老师？", new[] { new ClBlank(new[] { "多少", "duōshao" }, "bao nhiêu (duōshao)", "Ước lượng trên 10 người dùng 多少.") }, "你们学校有多少个老师？", "nǐmen xuéxiào yǒu duōshao ge lǎoshī?", "Trường các bạn có bao nhiêu giáo viên?", "Dưới 10 dùng 几; trên 10 dùng 多少.")
            },
            translate: new[]
            {
                new TrItem("zh-l42-tr-1", "easy", "Gia đình 5 người", "Nhà tôi có 5 người: ông bà, bố mẹ và tôi.", new[] { "我家", "五口人", "爷爷", "奶奶" }, new[] { "我家有五口人：爷爷、奶奶、爸爸、妈妈和我。", "我家有五口人，爷爷、奶奶、爸爸、妈妈和我。" }, "我家有五口人：爷爷、奶奶、爸爸、妈妈和我。", "wǒ jiā yǒu wǔ kǒu rén: yéye, nǎinai, bàba, māma hé wǒ.", "Dùng 口 cho nhân khẩu và 和 nối danh từ cuối."),
                new TrItem("zh-l42-tr-2", "easy", "Hai tấm vé xem phim", "Tôi đã mua 2 tấm vé xem phim.", new[] { "买了", "两张", "电影票" }, new[] { "我买了两张电影票。", "我买了两张电影票" }, "我买了两张电影票。", "wǒ mǎi le liǎng zhāng diànyǐng piào.", "Dùng 两张 cho 2 tấm vé."),
                new TrItem("zh-l42-tr-3", "medium", "Chiếc máy tính của anh trai", "Đây là máy tính của anh trai tôi.", new[] { "这是", "我哥哥的", "电脑" }, new[] { "这是我哥哥的电脑。", "这是我哥哥的电脑" }, "这是我哥哥的电脑。", "zhè shì wǒ gēge de diànnǎo.", "Lược bỏ 的 sau 我, giữ 的 sau 哥哥."),
                new TrItem("zh-l42-tr-4", "hard", "Hai chú chó nhỏ", "Trên ghế có hai chú chó nhỏ rất đáng yêu.", new[] { "椅子上", "两只", "小狗" }, new[] { "椅子上有两只很可爱的小狗。", "椅子上有两只小狗，很可爱。" }, "椅子上有两只很可爱的小狗。", "yǐzi shàng yǒu liǎng zhī hěn kě'ài de xiǎogǒu.", "Lượng từ cho chó con là 只.")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l42-gf-1", "easy", "Dùng 二 trước lượng từ 本", "我买了二本汉语书。", "Trước lượng từ 本 bắt buộc dùng 两 (không dùng 二).", "我买了两本汉语书。", new[] { "我买了两本汉语书。", "我买了两本书。" }, "wǒ mǎi le liǎng běn Hànyǔ shū", "Tôi đã mua 2 quyển sách tiếng Hán.", "两 + lượng từ; 二 dùng trong đếm thứ tự/con số."),
                new GfItem("zh-l42-gf-2", "medium", "Thiếu lượng từ giữa 这 và 衣服", "这衣服很漂亮。", "Giữa đại từ chỉ thị (这/那) và danh từ bắt buộc phải có lượng từ (件).", "这件衣服很漂亮。", new[] { "这件衣服很漂亮。", "这件衣服很漂亮" }, "zhè jiàn yīfu hěn piàoliang", "Chiếc áo này rất đẹp.", "这/那 + Lượng từ + Danh từ."),
                new GfItem("zh-l42-gf-3", "hard", "Dùng 和 để nối hai động từ/mệnh đề", "我回家和做作业。", "Chữ 和 chỉ nối danh từ/đại từ, không nối hai hành động/mệnh đề.", "我回家，然后做作业。", new[] { "我回家，然后做作业。", "我回家做作业。" }, "wǒ huí jiā, ránhòu zuò zuòyè", "Tôi về nhà rồi làm bài tập.", "Bỏ 和, dùng 然后 hoặc viết câu liên động trực tiếp.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l42-rp-1", "easy", "Giới thiệu ảnh gia đình", "Bạn Trung Quốc chỉ vào ảnh gia đình bạn và hỏi nhà bạn có mấy người.", "Bạn Trần Lệ", "明，这张照片真好看！你家有几口人？", "Míng, zhè zhāng zhàopiàn zhēn hǎokàn! nǐ jiā yǒu jǐ kǒu rén?", "Minh ơi, tấm ảnh này đẹp quá! Nhà cậu có mấy người thế?", "Trả lời số thành viên bằng 口人 và kể tên các thành viên (爸爸, 妈妈, 哥哥, 姐姐, 弟弟, 妹妹).", new[] { "口人|kou ren", "爸爸|妈妈|哥哥|姐姐|弟弟|妹妹|爷爷|奶奶", "有|you" }, new[] { "我家有四口人：爸爸、妈妈、妹妹和我。" }, "wǒ jiā yǒu sì kǒu rén: bàba, māma, mèimei hé wǒ.", 8),
                new RpItem("zh-l42-rp-2", "medium", "Mua đồ dùng và quần áo", "Nhân viên cửa hàng hỏi bạn muốn mua sách, từ điển hay quần áo.", "Nhân viên cửa hàng", "欢迎光临！请问您想买什么？", "huānyíng guānglín! qǐngwèn nín xiǎng mǎi shénme?", "Chào mừng quý khách! Xin hỏi quý khách muốn mua gì ạ?", "Nói bạn muốn mua 1-2 món kèm lượng từ chuẩn (本/张/件) và số 两 nếu là 2.", new[] { "本|张|件|个", "两|一|三", "书|词典|衣服|票|照片" }, new[] { "我想买两本汉越词典和一件新衣服。" }, "wǒ xiǎng mǎi liǎng běn Hàn-Yuè cídiǎn hé yí jiàn xīn yīfu.", 8),
                new RpItem("zh-l42-rp-3", "hard", "Hỏi đồ vật của ai trên bàn", "Bạn thấy một chiếc điện thoại và quyển sách trên bàn lớp học, hỏi bạn cùng lớp xem của ai.", "Bạn cùng lớp", "桌子上有一台电脑和两本书，那是你的吗？", "zhuōzi shàng yǒu yì tái diànnǎo hé liǎng běn shū, nà shì nǐ de ma?", "Trên bàn có một máy tính và 2 quyển sách, cái đó của cậu à?", "Trả lời cái nào là của bạn hoặc của người thân/bạn bạn dùng 的 (và lược 的 đúng chỗ).", new[] { "的", "是|不是", "电脑|书|我|朋友|哥哥|妹妹" }, new[] { "那两本书是我的，但是电脑是我哥哥的。" }, "nà liǎng běn shū shì wǒ de, dànshì diànnǎo shì wǒ gēge de.", 8)
            }, now));

        // ---------------- LESSON 4.3 ----------------
        all.AddRange(BuildLessonPack(cId, ChineseLesson43Id, "zh-CN", "Trung HSK1 · Bài 4.3",
            shadowing: new[]
            {
                new ShItem("zh-l43-sh-1", "easy", "Hỏi nghề nghiệp (Dễ)", "你是做什么工作的？", "nǐ shì zuò shénme gōngzuò de?", "Bạn làm công việc gì thế?", new[] { "做什么工作" }),
                new ShItem("zh-l43-sh-2", "easy", "Nơi làm việc (Dễ)", "我在一家医院工作。", "wǒ zài yì jiā yīyuàn gōngzuò", "Tôi làm việc ở một bệnh viện.", new[] { "在...医院", "工作" }),
                new ShItem("zh-l43-sh-3", "easy", "Đang gọi điện thoại (Dễ)", "他正在打电话呢。", "tā zhèngzài dǎ diànhuà ne", "Anh ấy đang gọi điện thoại đấy.", new[] { "正在", "打电话" }),
                new ShItem("zh-l43-sh-4", "medium", "Đang họp ở công ty (Vừa)", "李经理正在办公室开会呢。", "Lǐ jīnglǐ zhèngzài bàngōngshì kāihuì ne", "Giám đốc Lý đang họp trong văn phòng đấy.", new[] { "办公室", "开会" }),
                new ShItem("zh-l43-sh-5", "medium", "Phủ định tiếp diễn 没在 (Vừa)", "我没在看电视，我在写作业呢。", "wǒ méi zài kàn diànshì, wǒ zài xiě zuòyè ne", "Tôi không có đang xem tivi, tôi đang làm bài tập mà.", new[] { "没在", "写作业" }),
                new ShItem("zh-l43-sh-6", "medium", "Nghề kỹ sư phần mềm (Vừa)", "我哥哥是一位软件工程师。", "wǒ gēge shì yí wèi ruǎnjiàn gōngchéngshī", "Anh trai tôi là một kỹ sư phần mềm.", new[] { "工程师" }),
                new ShItem("zh-l43-sh-7", "hard", "Hỏi tìm người qua điện thoại (Khó)", "请问，王经理现在在办公室吗？", "qǐngwèn, Wáng jīnglǐ xiànzài zài bàngōngshì ma?", "Xin hỏi hiện tại giám đốc Vương có ở văn phòng không?", new[] { "在办公室吗" }),
                new ShItem("zh-l43-sh-8", "hard", "Công việc bận nhưng thú vị (Khó)", "工作虽然有点儿忙，但是很有意思。", "gōngzuò suīrán yǒudiǎnr máng, dànshì hěn yǒuyìsi", "Công việc tuy hơi bận nhưng rất thú vị.", new[] { "虽然", "但是", "有意思" }),
                new ShItem("zh-l43-sh-9", "hard", "Đang ngủ hay đang nấu cơm (Khó)", "妈妈正在厨房做饭，弟弟还在睡觉呢。", "māma zhèngzài chúfáng zuòfàn, dìdi hái zài shuìjiào ne", "Mẹ đang nấu cơm trong bếp, em trai vẫn đang ngủ đấy.", new[] { "厨房", "睡觉" })
            },
            dictation: new[]
            {
                new DiItem("zh-l43-di-1", "easy", "Làm việc ở công ty", "我在公司工作", "wǒ zài gōngsī gōngzuò", "Tôi làm việc ở công ty.", new[] { "我", "在", "公司", "工作" }, new[] { "医院", "是", "吗" }),
                new DiItem("zh-l43-di-2", "easy", "Mẹ là bác sĩ", "我妈妈是医生", "wǒ māma shì yīshēng", "Mẹ tôi là bác sĩ.", new[] { "我妈妈", "是", "医生" }, new[] { "护士", "在", "工作" }),
                new DiItem("zh-l43-di-3", "medium", "Đang gọi điện thoại", "爸爸正在打电话呢", "bàba zhèngzài dǎ diànhuà ne", "Bố đang gọi điện thoại đấy.", new[] { "爸爸", "正在", "打电话", "呢" }, new[] { "了", "吗", "不" }),
                new DiItem("zh-l43-di-4", "medium", "Không có đang ngủ", "他没在睡觉", "tā méi zài shuìjiào", "Anh ấy không có đang ngủ.", new[] { "他", "没在", "睡觉" }, new[] { "不在", "了", "开会" }),
                new DiItem("zh-l43-di-5", "hard", "Đang họp trong phòng họp", "经理正在会议室开会呢", "jīnglǐ zhèngzài huìyìshì kāihuì ne", "Giám đốc đang họp trong phòng họp đấy.", new[] { "经理", "正在", "会议室", "开会呢" }, new[] { "在在", "银行", "了" }),
                new DiItem("zh-l43-di-6", "hard", "Bạn làm nghề gì?", "请问你在哪儿工作", "qǐngwèn nǐ zài nǎr gōngzuò", "Xin hỏi bạn làm việc ở đâu?", new[] { "请问", "你", "在哪儿", "工作" }, new[] { "谁", "什么", "是" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l43-pr-1", "easy", "Từ vựng 工作 (gōngzuò)", "我在银行工作", "wǒ zài yínháng gōngzuò", "Tôi làm việc ở ngân hàng.", new[] { "银行", "工作" }, "Giữ thanh 1 cao phẳng cho gōng và đọc gọn đầu lưỡi thẳng zuò (thanh 4).", new[] { "yínháng đọc rõ háng thanh 2", "gōng thanh 1 cao phẳng", "zuò đầu lưỡi thẳng thanh 4" }, "我姐姐在银行工作。", new[] { "银行", "工作" }),
                new PrItem("zh-l43-pr-2", "easy", "Uốn lưỡi 工程师", "我是软件工程师", "wǒ shì ruǎnjiàn gōngchéngshī", "Tôi là kỹ sư phần mềm.", new[] { "软件", "工程师" }, "Uốn lưỡi cả ở ruǎn, chéng (bật hơi) và shī (thanh 1 cao).", new[] { "ruǎn uốn lưỡi thanh 3", "chéng uốn lưỡi bật hơi thanh 2", "shī uốn lưỡi thanh 1" }, "我哥哥是一位软件工程师。", new[] { "软件", "工程师" }),
                new PrItem("zh-l43-pr-3", "medium", "Tiếp diễn 正在...呢", "他正在开会呢", "tā zhèngzài kāihuì ne", "Anh ấy đang họp đấy.", new[] { "正在", "开会" }, "Uốn lưỡi zhèng (thanh 4) rồi chuyển sang zài đầu lưỡi thẳng.", new[] { "zhèng uốn lưỡi thanh 4", "zài đầu lưỡi thẳng thanh 4", "kāi bật hơi k thanh 1" }, "王经理正在开会呢。", new[] { "正在", "开会呢" }),
                new PrItem("zh-l43-pr-4", "medium", "Cặp 医生 & 护士", "医生和护士很忙", "yīshēng hé hùshi hěn máng", "Bác sĩ và y tá rất bận.", new[] { "医生", "护士", "忙" }, "Giữ thanh 1 cao đều cho cả hai âm tiết yī-shēng (uốn lưỡi shēng).", new[] { "yīshēng cả 2 chữ thanh 1", "hùshi chữ shi nhẹ", "máng thanh 2 đi lên" }, "医院里的医生和护士都很忙。", new[] { "医生", "护士" }),
                new PrItem("zh-l43-pr-5", "hard", "Động từ ly hợp 睡觉", "弟弟正在睡觉呢", "dìdi zhèngzài shuìjiào ne", "Em trai đang ngủ đấy.", new[] { "正在", "睡觉" }, "Uốn lưỡi shuì (thanh 4) và giữ rõ vần ia-o trong jiào (thanh 4).", new[] { "dìdi thanh 4 + nhẹ", "shuì uốn lưỡi thanh 4", "jiào không bật hơi thanh 4" }, "现在太晚了，妹妹正在睡觉呢。", new[] { "正在", "睡觉呢" }),
                new PrItem("zh-l43-pr-6", "hard", "Từ vựng 服务员", "服务员非常热情", "fúwùyuán fēicháng rèqíng", "Nhân viên phục vụ rất nhiệt tình.", new[] { "服务员", "非常" }, "Đọc tròn môi yuán (thanh 2) và phân biệt rõ fú (f) với wù (w).", new[] { "fú răng chạm môi", "wù tròn môi thanh 4", "yuán âm ü tròn môi thanh 2" }, "那家饭店的服务员很热情。", new[] { "服务员" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l43-wm-1", "easy", "Sơ đồ từ vựng: 工作 (Công việc)", "工作", "gōngzuò", "Làm việc / Công việc",
                    new[] { new WmNode("在医院工作", "zài yīyuàn gōngzuò", "Làm việc ở bệnh viện", "他在人民医院工作。"), new WmNode("找工作", "zhǎo gōngzuò", "Tìm việc làm", "毕业后我想找一份好工作。") },
                    new[] { new WmNode("做什么工作", "zuò shénme gōngzuò", "Làm nghề gì?", "请问您是做什么工作的？") },
                    new WmNode[] { new WmNode("忙 / 累", "máng / lèi", "Bận / Mệt", "最近工作很忙。") },
                    new[] { new WmEx("我在一家科技公司工作，非常喜欢我的工作。", "wǒ zài yì jiā kējì gōngsī gōngzuò, fēicháng xǐhuan wǒ de gōngzuò", "Tôi làm việc ở một công ty công nghệ, rất thích công việc của mình.") }),
                new WmItem("zh-l43-wm-2", "medium", "Sơ đồ từ vựng: 正在...呢 (Đang làm gì)", "正在...呢", "zhèngzài... ne", "Đang ... (Thì tiếp diễn)",
                    new[] { new WmNode("正在开会呢", "zhèngzài kāihuì ne", "Đang họp", "经理正在开会呢。"), new WmNode("正在打电话呢", "zhèngzài dǎ diànhuà ne", "Đang gọi điện", "爸爸正在打电话呢。") },
                    new WmNode[] { new WmNode("没在 + Động từ", "méi zài...", "Không có đang...", "我没在玩手机。") },
                    new WmNode[] { new WmNode("在 + Địa điểm + Động từ", "zài + place + V", "Làm gì ở đâu", "我在图书馆学习。") },
                    new[] { new WmEx("不好意思，他正在办公室写报告呢。", "bù hǎoyìsi, tā zhèngzài bàngōngshì xiě bàogào ne", "Ngại quá, anh ấy đang viết báo cáo trong văn phòng đấy.") }),
                new WmItem("zh-l43-wm-3", "hard", "Sơ đồ từ vựng: 医生 / 工程师 (Nghề nghiệp)", "职业", "zhíyè", "Nghề nghiệp & Nơi làm việc",
                    new[] { new WmNode("医生 / 护士", "yīshēng / hùshi", "Bác sĩ / Y tá (医院)", "医院有很多医生。"), new WmNode("工程师 / 经理", "gōngchéngshī / jīnglǐ", "Kỹ sư / Quản lý (公司)", "他是一位软件工程师。") },
                    new WmNode[] { new WmNode("服务员", "fúwùyuán", "Nhân viên phục vụ (饭店)", "服务员，买单！") },
                    new WmNode[] { new WmNode("老师 / 学生", "lǎoshī / xuésheng", "Giáo viên / Học sinh (学校)", "王老师在学校教书。") },
                    new[] { new WmEx("我爸爸是医生，我哥哥是工程师，我是大学生。", "wǒ bàba shì yīshēng, wǒ gēge shì gōngchéngshī, wǒ shì dàxuéshēng", "Bố tôi là bác sĩ, anh trai tôi là kỹ sư, còn tôi là sinh viên.") })
            },
            cloze: new[]
            {
                new ClItem("zh-l43-cl-1", "easy", "Trật tự 在 + Địa điểm + Động từ", "Điền từ nối đúng để nói 'làm việc ở ngân hàng':", "我哥哥{{0}}银行{{1}}。", new[] { new ClBlank(new[] { "在", "zài", "zai4" }, "ở tại (zài)", "Trạng ngữ địa điểm 在 + Nơi chốn đặt trước động từ."), new ClBlank(new[] { "工作", "gōngzuò", "gong1 zuo4" }, "làm việc (gōngzuò)", "Động từ đặt cuối.") }, "我哥哥在银行工作。", "wǒ gēge zài yínháng gōngzuò", "Anh trai tôi làm việc ở ngân hàng.", "Không đặt 在银行 ra sau 工作."),
                new ClItem("zh-l43-cl-2", "easy", "Nhấn mạnh thì tiếp diễn", "Hoàn thiện câu 'Anh ấy đang họp đấy':", "王经理{{0}}开会{{1}}。", new[] { new ClBlank(new[] { "正在", "在", "zhèngzài" }, "đang (zhèngzài)", "正在 + Động từ + 呢."), new ClBlank(new[] { "呢", "ne" }, "trợ từ ngữ khí tiếp diễn", "呢 đặt cuối câu nhấn mạnh hành động đang diễn ra.") }, "王经理正在开会呢。", "Wáng jīnglǐ zhèngzài kāihuì ne", "Quản lý Vương đang họp đấy.", "Cấu trúc chuẩn: 正在 + V + 呢."),
                new ClItem("zh-l43-cl-3", "medium", "Phủ định tiếp diễn 没在", "Điền từ phủ định đúng cho hành động đang diễn ra:", "他{{0}}看电视，他在写作业呢。", new[] { new ClBlank(new[] { "没在", "没有在", "méi zài" }, "không có đang (méi zài)", "Phủ định của 在/正在 + V là 没(有)在 + V.") }, "他没在看电视，他在写作业呢。", "tā méi zài kàn diànshì, tā zài xiě zuòyè ne", "Nó không có đang xem tivi đâu, nó đang làm bài tập đấy.", "Không dùng 不在 trước động từ tiếp diễn."),
                new ClItem("zh-l43-cl-4", "hard", "Ghép Tiếp diễn + Địa điểm không lặp 在", "Điền từ chỉ 'đang' để tránh lặp hai chữ 在:", "妈妈{{0}}厨房做饭呢。", new[] { new ClBlank(new[] { "正在在", "正在" }, "đang ở tại (zhèngzài)", "Khi có địa điểm đứng trước động từ, dùng 正在 (hoặc 在) là đủ, không viết 在在.") }, "妈妈正在厨房做饭呢。", "māma zhèngzài chúfáng zuòfàn ne", "Mẹ đang nấu cơm trong bếp đấy.", "正在 + Địa điểm + Động từ + 呢.")
            },
            translate: new[]
            {
                new TrItem("zh-l43-tr-1", "easy", "Hỏi nghề nghiệp", "Bạn làm công việc gì thế?", new[] { "你", "做", "什么工作" }, new[] { "你做什么工作？", "你是做什么工作的？", "nǐ zuò shénme gōngzuò" }, "你是做什么工作的？", "nǐ shì zuò shénme gōngzuò de?", "Cả hai mẫu 你做什么工作？ và 你是做什么工作的？ đều chuẩn."),
                new TrItem("zh-l43-tr-2", "easy", "Bác sĩ ở bệnh viện", "Chị gái tôi làm bác sĩ ở bệnh viện.", new[] { "姐姐", "在医院", "医生" }, new[] { "我姐姐在医院工作，她是医生。", "我姐姐在医院当医生。", "姐姐在医院做医生。" }, "我姐姐在医院当医生。", "wǒ jiějie zài yīyuàn dāng yīshēng.", "Đặt 在医院 trước động từ."),
                new TrItem("zh-l43-tr-3", "medium", "Đang gọi điện thoại", "Bố đang gọi điện thoại cho ông nội đấy.", new[] { "正在", "给爷爷", "打电话" }, new[] { "爸爸正在给爷爷打电话呢。", "爸爸正在给爷爷打电话。" }, "爸爸正在给爷爷打电话呢。", "bàba zhèngzài gěi yéye dǎ diànhuà ne.", "给 [ai] 打电话 = gọi điện cho ai."),
                new TrItem("zh-l43-tr-4", "hard", "Đang họp trong văn phòng", "Giám đốc Lý đang họp ở văn phòng, bạn đợi một lát nhé.", new[] { "正在办公室", "开会", "等一下" }, new[] { "李经理正在办公室开会呢，请等一下。", "李经理正在办公室开会呢，请您等一下儿。" }, "李经理正在办公室开会呢，请您等一下儿。", "Lǐ jīnglǐ zhèngzài bàngōngshì kāihuì ne, qǐng nín děng yíxiàr.", "Tránh lặp hai chữ 在 bằng cách dùng 正在.")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l43-gf-1", "easy", "Đặt 在 + Địa điểm sau động từ 工作", "我工作在一家软件公司。", "Trạng ngữ địa điểm 在 + Nơi chốn bắt buộc đứng TRƯỚC động từ 工作.", "我在一家软件公司工作。", new[] { "我在一家软件公司工作。", "我在软件公司工作。" }, "wǒ zài yì jiā ruǎnjiàn gōngsī gōngzuò", "Tôi làm việc ở một công ty phần mềm.", "Trật tự chuẩn: Chủ ngữ + 在 Nơi chốn + Động từ."),
                new GfItem("zh-l43-gf-2", "medium", "Lặp hai chữ 在 liên tiếp", "他在在图书馆看书呢。", "Không bao giờ viết 在在 cạnh nhau; dùng 正在 ở đầu hoặc chỉ giữ một chữ 在.", "他正在图书馆看书呢。", new[] { "他正在图书馆看书呢。", "他在图书馆看书呢。" }, "tā zhèngzài túshūguǎn kànshū ne", "Anh ấy đang đọc sách ở thư viện đấy.", "正在 + Địa điểm + Động từ + 呢."),
                new GfItem("zh-l43-gf-3", "hard", "Dùng 不在 thay cho 没在", "我不在睡觉，我在学习呢。", "Phủ định hành động đang diễn ra phải dùng 没在 (không dùng 不在).", "我没在睡觉，我在学习呢。", new[] { "我没在睡觉，我在学习呢。", "我没有在睡觉，我在学习呢。" }, "wǒ méi zài shuìjiào, wǒ zài xuéxí ne", "Tôi không có đang ngủ, tôi đang học bài mà.", "没(有)在 + V = không có đang làm gì.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l43-rp-1", "easy", "Hỏi thăm nghề nghiệp & nơi làm việc", "Một người bạn mới quen hỏi bạn làm nghề gì và làm ở đâu.", "Bạn Trương Vĩ", "请问你是做什么工作的？在哪儿工作？", "qǐngwèn nǐ shì zuò shénme gōngzuò de? zài nǎr gōngzuò?", "Xin hỏi bạn làm công việc gì thế? Làm việc ở đâu?", "Trả lời nghề nghiệp (工程师/老师/医生/职员/学生) và nơi làm việc đặt 在 Nơi chốn trước động từ.", new[] { "在|zai", "工作|当|是|学习", "公司|医院|学校|银行" }, new[] { "我是软件工程师，我在一家科技公司工作。" }, "wǒ shì ruǎnjiàn gōngchéngshī, wǒ zài yì jiā kējì gōngsī gōngzuò.", 8),
                new RpItem("zh-l43-rp-2", "medium", "Gọi điện tìm người ở văn phòng", "Bạn là lễ tân công ty, khách gọi điện hỏi thăm sếp Lý đang làm gì có rảnh không.", "Khách hàng", "喂，你好！请问李经理现在在办公室吗？", "wéi, nǐ hǎo! qǐngwèn Lǐ jīnglǐ xiànzài zài bàngōngshì ma?", "A lô xin chào! Cho hỏi quản lý Lý hiện có ở văn phòng không?", "Trả lời lịch sự anh ấy đang họp/gọi điện ở phòng họp bằng 正在...呢 và mời khách đợi.", new[] { "正在|在", "开会|打电话|工作|看文件", "呢|等一下|抱歉|不好意思" }, new[] { "不好意思，李经理现在正在会议室开会呢，请您等一下儿。" }, "bù hǎoyìsi, Lǐ jīnglǐ xiànzài zhèngzài huìyìshì kāihuì ne, qǐng nín děng yíxiàr.", 8),
                new RpItem("zh-l43-rp-3", "hard", "Giải thích lúc nãy đang làm gì", "Bạn của bạn nhắn tin trách sao vừa nãy gọi không nghe máy.", "Bạn Trần Lệ", "明，刚才我给你打电话，你在睡觉吗？", "Míng, gāngcái wǒ gěi nǐ dǎ diànhuà, nǐ zài shuìjiào ma?", "Minh ơi, nãy mình gọi cho cậu, cậu đang ngủ à?", "Phủ định bằng 没在 và nói rõ lúc nãy bạn đang làm gì (在/正在 + V + 呢).", new[] { "没在|没有在", "我在|正在", "开会|学习|工作|做饭|写作业" }, new[] { "不好意思，我没在睡觉，刚才我正在公司开会呢！" }, "bù hǎoyìsi, wǒ méi zài shuìjiào, gāngcái wǒ zhèngzài gōngsī kāihuì ne!", 8)
            }, now));

        // ---------------- LESSON 5.1 ----------------
        all.AddRange(BuildLessonPack(cId, ChineseLesson51Id, "zh-CN", "Trung HSK1 · Bài 5.1",
            shadowing: new[]
            {
                new ShItem("zh-l51-sh-1", "easy", "Biết nói tiếng Hán (Dễ)", "我会说汉语，也会写汉字。", "wǒ huì shuō Hànyǔ, yě huì xiě Hànzì", "Tôi biết nói tiếng Hán và cũng biết viết chữ Hán.", new[] { "会说", "会写" }),
                new ShItem("zh-l51-sh-2", "easy", "Xin phép ngồi ghế (Dễ)", "请问，我可以坐这儿吗？", "qǐngwèn, wǒ kěyǐ zuò zhèr ma?", "Xin hỏi tôi ngồi đây được không?", new[] { "可以", "坐这儿" }),
                new ShItem("zh-l51-sh-3", "easy", "Dự định cuối tuần (Dễ)", "周末我想去游泳。", "zhōumò wǒ xiǎng qù yóuyǒng", "Cuối tuần tôi muốn đi bơi.", new[] { "想", "游泳" }),
                new ShItem("zh-l51-sh-4", "medium", "Điều kiện khách quan 不能 (Vừa)", "对不起，今天我太忙了，不能去。", "duìbuqǐ, jīntiān wǒ tài máng le, bù néng qù", "Xin lỗi, hôm nay tôi bận quá không đi được.", new[] { "太忙了", "不能去" }),
                new ShItem("zh-l51-sh-5", "medium", "Không biết lái xe (Vừa)", "我不会开车，我们坐地铁去吧。", "wǒ bú huì kāichē, wǒmen zuò dìtiě qù ba", "Tôi không biết lái xe, tụi mình đi tàu điện ngầm nhé.", new[] { "不会开车" }),
                new ShItem("zh-l51-sh-6", "medium", "Thanh toán qua điện thoại (Vừa)", "这儿可以用手机支付吗？", "zhèr kěyǐ yòng shǒujī zhīfù ma?", "Ở đây có thể thanh toán bằng điện thoại không?", new[] { "可以", "手机支付" }),
                new ShItem("zh-l51-sh-7", "hard", "Cấm chụp ảnh bảo tàng (Khó)", "对不起，博物馆里面不可以拍照。", "duìbuqǐ, bówùguǎn lǐmiàn bù kěyǐ pāizhào", "Xin lỗi, bên trong bảo tàng không được phép chụp ảnh.", new[] { "不可以", "拍照" }),
                new ShItem("zh-l51-sh-8", "hard", "Câu phức 因为...所以... (Khó)", "因为生病了，所以我不能参加聚会。", "yīnwèi shēngbìng le, suǒyǐ wǒ bù néng cānjiā jùhuì", "Vì bị ốm rồi nên tôi không thể tham gia buổi tụ họp.", new[] { "因为", "所以", "不能" }),
                new ShItem("zh-l51-sh-9", "hard", "Nhờ giúp đỡ lịch sự (Khó)", "请问，你能帮我一个忙吗？— 没问题！", "qǐngwèn, nǐ néng bāng wǒ yí ge máng ma? — méi wèntí!", "Xin hỏi bạn giúp tôi một việc được không? — Không vấn đề!", new[] { "帮忙", "没问题" })
            },
            dictation: new[]
            {
                new DiItem("zh-l51-di-1", "easy", "Biết nấu món Trung", "我会做中国菜", "wǒ huì zuò Zhōngguó cài", "Tôi biết nấu món Trung Quốc.", new[] { "我", "会", "做", "中国菜" }, new[] { "能", "想", "不" }),
                new DiItem("zh-l51-di-2", "easy", "Xin phép chụp ảnh", "这里可以拍照吗", "zhèlǐ kěyǐ pāizhào ma", "Ở đây có được chụp ảnh không?", new[] { "这里", "可以", "拍照", "吗" }, new[] { "会", "想", "呢" }),
                new DiItem("zh-l51-di-3", "medium", "Không thể đi được", "今天我不能去", "jīntiān wǒ bù néng qù", "Hôm nay tôi không thể đi được.", new[] { "今天", "我", "不能", "去" }, new[] { "不会", "不想", "没" }),
                new DiItem("zh-l51-di-4", "medium", "Không muốn ra ngoài", "我有点儿累不想出去", "wǒ yǒudiǎnr lèi bù xiǎng chūqù", "Tôi hơi mệt, không muốn ra ngoài.", new[] { "我", "有点儿累", "不想", "出去" }, new[] { "不要", "不会", "不能" }),
                new DiItem("zh-l51-di-5", "hard", "Ngày mai sẽ mưa không?", "你觉得明天会下雨吗", "nǐ juéde míngtiān huì xiàyǔ ma", "Bạn thấy ngày mai trời có mưa không?", new[] { "你觉得", "明天", "会", "下雨吗" }, new[] { "能", "可以", "想" }),
                new DiItem("zh-l51-di-6", "hard", "Vì bận nên không đến", "因为很忙所以我不能来", "yīnwèi hěn máng suǒyǐ wǒ bù néng lái", "Vì rất bận nên tôi không thể đến.", new[] { "因为", "很忙", "所以", "我不能来" }, new[] { "但是", "不会", "没" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l51-pr-1", "easy", "Biến điệu 不 trước 会 (bú huì)", "我不会开车", "wǒ bú huì kāichē", "Tôi không biết lái xe.", new[] { "不会", "开车" }, "Chữ 不 đứng trước huì (thanh 4) biến điệu thành thanh 2: bú huì.", new[] { "bú lên giọng thanh 2", "huì rơi nhanh thanh 4", "kāichē giữ cao ngang thanh 1" }, "不好意思，我不会开车。", new[] { "不会", "开车" }),
                new PrItem("zh-l51-pr-2", "easy", "Cặp 可以 (kěyǐ)", "我可以进来吗", "wǒ kěyǐ jìnlái ma", "Tôi vào trong được không?", new[] { "可以", "进来" }, "Biến điệu thanh 3 giữa kě và yǐ: đọc thành ké yǐ.", new[] { "kě bật hơi k lên thanh 2", "yǐ hạ sâu thanh 3", "jìn thanh 4 rõ ràng" }, "老师，我可以进来吗？", new[] { "可以", "进来" }),
                new PrItem("zh-l51-pr-3", "medium", "Từ vựng 游泳 (yóuyǒng)", "他很会游泳", "tā hěn huì yóuyǒng", "Anh ấy bơi rất giỏi.", new[] { "会", "游泳" }, "Đọc rõ sự khác biệt thanh 2 (yóu) và thanh 3 (yǒng).", new[] { "hěn hạ thanh 3", "yóu lên giọng thanh 2", "yǒng hạ sâu thanh 3" }, "夏天我最喜欢去游泳。", new[] { "喜欢", "游泳" }),
                new PrItem("zh-l51-pr-4", "medium", "Cặp 拍照 (pāizhào)", "这里不能拍照", "zhèlǐ bù néng pāizhào", "Ở đây không được chụp ảnh.", new[] { "不能", "拍照" }, "Bật hơi mạnh ở pāi (thanh 1) và uốn lưỡi zhào (thanh 4).", new[] { "bù giữ nguyên thanh 4 trước néng", "pāi bật hơi p thanh 1", "zhào uốn lưỡi thanh 4" }, "对不起，里面不能拍照。", new[] { "不能", "拍照" }),
                new PrItem("zh-l51-pr-5", "hard", "Từ 手机支付 (shǒujī zhīfù)", "可以用手机支付", "kěyǐ yòng shǒujī zhīfù", "Có thể dùng thanh toán điện thoại.", new[] { "可以", "手机", "支付" }, "Uốn lưỡi shǒu và zhī rõ ràng, tách biệt với jī (mặt lưỡi).", new[] { "shǒu uốn lưỡi thanh 3", "jī mặt lưỡi thanh 1", "zhī uốn lưỡi thanh 1" }, "在这里可以用手机支付吗？", new[] { "手机支付" }),
                new PrItem("zh-l51-pr-6", "hard", "Cặp 因为...所以...", "因为下雨所以不能去", "yīnwèi xiàyǔ suǒyǐ bù néng qù", "Vì trời mưa nên không đi được.", new[] { "因为", "所以", "不能" }, "Đọc đúng thanh 1 cho yīn, thanh 4 cho wèi và biến điệu thanh 3 suǒyǐ.", new[] { "yīnwèi thanh 1 + thanh 4", "suǒ thanh 2 (biến điệu)", "qù bật hơi q tròn môi" }, "因为明天有考试，所以我不能去。", new[] { "因为", "所以" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l51-wm-1", "easy", "Sơ đồ từ vựng: 会 (Biết kỹ năng / Sẽ)", "会", "huì", "Biết (học mới có) / Sẽ (tương lai)",
                    new[] { new WmNode("会说汉语", "huì shuō Hànyǔ", "Biết nói tiếng Hán", "我会说一点儿汉语。"), new WmNode("会开车 / 游泳", "huì kāichē / yóuyǒng", "Biết lái xe / bơi", "我不会开车。") },
                    new[] { new WmNode("明天会下雨吗", "míngtiān huì xiàyǔ ma", "Mai sẽ mưa không?", "明天不会下雨。") },
                    new WmNode[] { new WmNode("能 / 可以", "néng / kěyǐ", "Có thể (điều kiện / xin phép)", "我可以坐这儿吗？") },
                    new[] { new WmEx("我姐姐非常会做中国菜。", "wǒ jiějie fēicháng huì zuò Zhōngguó cài", "Chị gái tôi nấu món Trung Quốc rất giỏi.") }),
                new WmItem("zh-l51-wm-2", "medium", "Sơ đồ từ vựng: 能 / 可以 (Khả năng & Xin phép)", "能・可以", "néng / kěyǐ", "Có thể / Được phép",
                    new[] { new WmNode("可以坐这儿吗", "kěyǐ zuò zhèr ma", "Ngồi đây được không?", "可以，请坐！"), new WmNode("你能来吗", "nǐ néng lái ma", "Bạn có đến được không?", "明天我太忙，不能来。") },
                    new WmNode[] { new WmNode("不能吸烟", "bù néng xīyān", "Không được hút thuốc", "公共场所不能吸烟。") },
                    new WmNode[] { new WmNode("没问题", "méi wèntí", "Không vấn đề gì", "当然可以，没问题！") },
                    new[] { new WmEx("请问，你能帮我看一下儿行李吗？", "qǐngwèn, nǐ néng bāng wǒ kàn yíxiàr xíngli ma", "Xin hỏi bạn có thể trông giúp tôi hành lý một lát không?") }),
                new WmItem("zh-l51-wm-3", "hard", "Sơ đồ từ vựng: 想 vs 要 (Ý muốn)", "想・要", "xiǎng / yào", "Muốn (nhẹ nhàng) vs Cần/Muốn (dứt khoát)",
                    new[] { new WmNode("想去旅游", "xiǎng qù lǚyóu", "Muốn đi du lịch", "今年我想去上海旅游。"), new WmNode("不想出去", "bù xiǎng chūqù", "Không muốn ra ngoài", "我很累，不想出去。") },
                    new WmNode[] { new WmNode("不要 + Động từ", "bú yào...", "Đừng... (khuyên ngăn)", "太晚了，不要喝咖啡了！") },
                    new WmNode[] { new WmNode("因为...所以...", "yīnwèi... suǒyǐ...", "Bởi vì... cho nên...", "因为累了，所以想休息。") },
                    new[] { new WmEx("我想喝一杯热茶，不要太甜。", "wǒ xiǎng hē yì bēi rè chá, bú yào tài tián", "Tôi muốn uống một tách trà nóng, đừng ngọt quá.") })
            },
            cloze: new[]
            {
                new ClItem("zh-l51-cl-1", "easy", "Kỹ năng học được 会", "Điền trợ động từ phù hợp cho kỹ năng bơi lội:", "我{{0}}游泳，但是游得不太快。", new[] { new ClBlank(new[] { "会", "huì", "hui4" }, "biết (kỹ năng học được)", "Bơi lội là kỹ năng phải học nên dùng 会.") }, "我游泳，我會游泳...", "wǒ huì yóuyǒng, dànshì yóu de bú tài kuài", "Tôi biết bơi nhưng bơi không nhanh lắm.", "Kỹ năng dùng 会."),
                new ClItem("zh-l51-cl-2", "easy", "Xin phép lịch sự 可以", "Điền trợ động từ xin phép thanh toán qua điện thoại:", "请问，这儿{{0}}用微信支付吗？", new[] { new ClBlank(new[] { "可以", "能", "kěyǐ", "néng" }, "có thể (kěyǐ)", "Hỏi xem nơi này có cho phép trả bằng WeChat không dùng 可以.") }, "请问，这儿可以用微信支付吗？", "qǐngwèn, zhèr kěyǐ yòng Wēixìn zhīfù ma?", "Xin hỏi ở đây có thể thanh toán bằng WeChat không?", "Xin phép hoặc hỏi quy định dùng 可以."),
                new ClItem("zh-l51-cl-3", "medium", "Điều kiện khách quan 不能", "Điền từ chỉ 'không thể' do điều kiện sức khỏe:", "因为我脚疼，所以今天{{0}}去打篮球了。", new[] { new ClBlank(new[] { "不能", "bù néng", "bu4 neng2" }, "không thể (bù néng)", "Đau chân là điều kiện khách quan nên dùng 不能.") }, "因为我脚疼，所以今天不能去打篮球了。", "yīnwèi wǒ jiǎo téng, suǒyǐ jīntiān bù néng qù dǎ lánqiú le", "Vì chân bị đau nên hôm nay tôi không thể đi chơi bóng rổ được.", "Khả năng/hoàn cảnh dùng 能."),
                new ClItem("zh-l51-cl-4", "hard", "Phủ định khuyên ngăn 不要", "Điền từ mang nghĩa 'Đừng' vào lời khuyên:", "已经晚上十一点了，{{0}}再玩手机了，快睡觉吧！", new[] { new ClBlank(new[] { "不要", "别", "bú yào" }, "đừng (bú yào / bié)", "Khuyên ngăn 'đừng làm gì nữa' dùng 不要...了.") }, "已经晚上十一点了，不要再玩手机了，快睡觉吧！", "yǐjīng wǎnshang shíyī diǎn le, bú yào zài wán shǒujī le, kuài shuìjiào ba", "Đã 11 giờ đêm rồi, đừng chơi điện thoại nữa, mau đi ngủ đi!", "Đừng nhầm với 不想 (không muốn).")
            },
            translate: new[]
            {
                new TrItem("zh-l51-tr-1", "easy", "Biết nói nhưng không biết viết", "Tôi biết nói tiếng Trung nhưng không biết viết chữ Hán.", new[] { "会说", "但是", "不会写" }, new[] { "我会说汉语，但是不会写汉字。", "我会说中文，但是不会写汉字。" }, "我会说汉语，但是不会写汉字。", "wǒ huì shuō Hànyǔ, dànshì bú huì xiě Hànzì.", "Kỹ năng học tập dùng 会 / 不会."),
                new TrItem("zh-l51-tr-2", "easy", "Xin phép ngồi đây", "Xin hỏi tôi ngồi ở đây có được không?", new[] { "请问", "可以", "坐这儿吗" }, new[] { "请问，我可以坐这儿吗？", "请问，我可以坐在这儿吗？", "请问，我能坐这儿吗？" }, "请问，我可以坐这儿吗？", "qǐngwèn, wǒ kěyǐ zuò zhèr ma?", "Mẫu xin phép kinh điển: 我可以...吗？"),
                new TrItem("zh-l51-tr-3", "medium", "Vì bận nên không đi được", "Vì hôm nay rất bận nên tôi không thể đi xem phim.", new[] { "因为", "很忙", "所以", "不能" }, new[] { "因为今天很忙，所以我不能去看电影。", "因为我今天太忙了，所以不能去看电影。" }, "因为今天很忙，所以我不能去看电影。", "yīnwèi jīntiān hěn máng, suǒyǐ wǒ bù néng qù kàn diànyǐng.", "Dùng cặp 因为...所以... và 不能."),
                new TrItem("zh-l51-tr-4", "hard", "Nhờ giúp đỡ một việc", "Xin hỏi, bạn có thể giúp tôi một việc được không?", new[] { "请问", "能", "帮我一个忙吗" }, new[] { "请问，你能帮我一个忙吗？", "请问，你可以帮我一个忙吗？" }, "请问，你能帮我一个忙吗？", "qǐngwèn, nǐ néng bāng wǒ yí ge máng ma?", "帮我一个忙 = giúp tôi một việc (từ ly hợp 帮忙).")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l51-gf-1", "easy", "Trả lời câu hỏi 可以 bằng 是", "请问我可以坐这儿吗？— 是，你可以。", "Trả lời câu hỏi trợ động từ phải lặp lại chính trợ động từ đó (可以), không dùng 是.", "请问我可以坐这儿吗？— 可以，请坐！", new[] { "请问我可以坐这儿吗？— 可以，请坐！", "可以，请坐！" }, "qǐngwèn wǒ kěyǐ zuò zhèr ma? — kěyǐ, qǐng zuò!", "Xin hỏi tôi ngồi đây được không? — Được, mời ngồi!", "Trả lời 会/能/可以 chỉ cần lặp lại 会/能/可以."),
                new GfItem("zh-l51-gf-2", "medium", "Dùng 能 thay 会 cho kỹ năng lái xe", "我不能开车，我没学过。", "Biết hay không biết một kỹ năng phải học (lái xe) dùng 会 / 不会.", "我不会开车，我没学过。", new[] { "我不会开车，我没学过。", "我不会开车。" }, "wǒ bú huì kāichē, wǒ méi xué guo", "Tôi không biết lái xe, tôi chưa từng học.", "Học mới biết làm → 不会."),
                new GfItem("zh-l51-gf-3", "hard", "Lạm dụng 不要 khi nói ý định cá nhân", "今天下雨，我不要去公园。", "Diễn đạt bản thân không muốn làm gì thì dùng 不想 (不要 mang nghĩa cấm/đừng/không lấy).", "今天下雨，我不想去公园。", new[] { "今天下雨，我不想去公园。", "今天下雨，我不能去公园。" }, "jīntiān xiàyǔ, wǒ bù xiǎng qù gōngyuán", "Hôm nay trời mưa, tôi không muốn đi công viên.", "Không muốn làm gì = 不想 + V.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l51-rp-1", "easy", "Xin phép trong quán cà phê/thư viện", "Bạn thấy một chiếc ghế trống cạnh một người khách và muốn xin phép ngồi.", "Khách trong quán", "您好，请问有什么事吗？", "nínhǎo, qǐngwèn yǒu shénme shì ma?", "Xin chào, xin hỏi có việc gì không ạ?", "Dùng mẫu xin phép 可以...吗 để hỏi ngồi ở đây (坐这儿) hoặc chụp ảnh/dùng ổ điện.", new[] { "可以|ke yi", "坐|zuo|拍照|用", "谢谢|请问" }, new[] { "请问，这儿没有人，我可以坐这儿吗？谢谢！" }, "qǐngwèn, zhèr méiyǒu rén, wǒ kěyǐ zuò zhèr ma? xièxie!", 6),
                new RpItem("zh-l51-rp-2", "medium", "Rủ bạn cuối tuần & hỏi khả năng", "Bạn Trung Quốc rủ bạn đi leo núi bằng ô tô vào thứ Bảy này.", "Bạn Trương Vĩ", "明，星期六我们开车去爬山吧！你会开车吗？", "Míng, Xīngqīliù wǒmen kāichē qù páshān ba! nǐ huì kāichē ma?", "Minh ơi, thứ Bảy tụi mình lái xe đi leo núi nhé! Cậu biết lái xe không?", "Trả lời bạn biết (会) hay không biết (不会) lái xe, và đề xuất có thể (可以) đi tàu điện/xe buýt.", new[] { "会|不会|hui", "可以|能|ke yi|neng", "地铁|公交|出租车|开车" }, new[] { "不好意思，我不会开车。我们可以坐地铁或者打车去吗？" }, "bù hǎoyìsi, wǒ bú huì kāichē. wǒmen kěyǐ zuò dìtiě huòzhě dǎchē qù ma?", 8),
                new RpItem("zh-l51-rp-3", "hard", "Từ chối khéo lời mời đi tiệc", "Đồng nghiệp mời bạn tối nay đi ăn nhưng bạn đang bị ốm/bận nên không đi được.", "Đồng nghiệp", "晚上大家一起去聚餐，你能来参加吗？", "wǎnshang dàjiā yìqǐ qù jùcān, nǐ néng lái cānjiā ma?", "Tối nay mọi người cùng đi liên hoan, cậu đến tham gia được không?", "Từ chối lịch sự bằng 因为...所以... và 不能 (hoặc 不想) kèm lời xin lỗi.", new[] { "不好意思|对不起", "因为|所以|太忙|生病|不舒服", "不能|不想" }, new[] { "不好意思，因为今天有点儿生病，所以我不能去参加了。" }, "bù hǎoyìsi, yīnwèi jīntiān yǒudiǎnr shēngbìng, suǒyǐ wǒ bù néng qù cānjiā le.", 8)
            }, now));

        // ---------------- LESSON 5.2 ----------------
        all.AddRange(BuildLessonPack(cId, ChineseLesson52Id, "zh-CN", "Trung HSK1 · Bài 5.2",
            shadowing: new[]
            {
                new ShItem("zh-l52-sh-1", "easy", "Nhấn mạnh thời gian quá khứ (Dễ)", "我是去年九月来北京的。", "wǒ shì qùnián jiǔ yuè lái Běijīng de", "Tôi đến Bắc Kinh vào tháng 9 năm ngoái.", new[] { "是...的", "去年" }),
                new ShItem("zh-l52-sh-2", "easy", "Nhấn mạnh phương thức (Dễ)", "我是坐飞机来的。", "wǒ shì zuò fēijī lái de", "Tôi đi máy bay đến đấy.", new[] { "坐飞机", "来的" }),
                new ShItem("zh-l52-sh-3", "easy", "Hành động hoàn thành 了 (Dễ)", "昨天我买了一件新衣服。", "zuótiān wǒ mǎi le yí jiàn xīn yīfu", "Hôm qua tôi đã mua một chiếc áo mới.", new[] { "买了" }),
                new ShItem("zh-l52-sh-4", "medium", "Phủ định quá khứ 没 (Vừa)", "昨天我没去商店，我在家休息。", "zuótiān wǒ méi qù shāngdiàn, wǒ zài jiā xiūxi", "Hôm qua tôi không đi cửa hàng, tôi nghỉ ở nhà.", new[] { "没去" }),
                new ShItem("zh-l52-sh-5", "medium", "Nhấn mạnh địa điểm (Vừa)", "这本书是在网上买的。", "zhè běn shū shì zài wǎngshàng mǎi de", "Cuốn sách này là mua trên mạng đấy.", new[] { "在网上", "买的" }),
                new ShItem("zh-l52-sh-6", "medium", "Phủ định 是...的 (Vừa)", "我不是坐火车来的，我是坐飞机来的。", "wǒ bú shì zuò huǒchē lái de, wǒ shì zuò fēijī lái de", "Không phải tôi đi tàu hỏa đến đâu, tôi đi máy bay đến đấy.", new[] { "不是...的" }),
                new ShItem("zh-l52-sh-7", "hard", "Hỏi cách quen nhau (Khó)", "你们是怎么认识的？— 我们是在大学认识的。", "nǐmen shì zěnme rènshi de? — wǒmen shì zài dàxué rènshi de", "Hai bạn quen nhau thế nào? — Tụi mình quen ở đại học.", new[] { "怎么认识的", "在大学" }),
                new ShItem("zh-l52-sh-8", "hard", "Chưa làm 还没...呢 (Khó)", "我还没吃早饭呢，现在去吃吧。", "wǒ hái méi chī zǎofàn ne, xiànzài qù chī ba", "Tôi vẫn chưa ăn sáng đâu, giờ đi ăn nhé.", new[] { "还没", "呢" }),
                new ShItem("zh-l52-sh-9", "hard", "Người tặng quà (Khó)", "这块手表是爸爸去年送给我的。", "zhè kuài shǒubiǎo shì bàba qùnián sòng gěi wǒ de", "Chiếc đồng hồ này là bố tặng cho tôi năm ngoái.", new[] { "送给我的" })
            },
            dictation: new[]
            {
                new DiItem("zh-l52-di-1", "easy", "Đi máy bay đến", "我是坐飞机来的", "wǒ shì zuò fēijī lái de", "Tôi đi máy bay đến.", new[] { "我是", "坐飞机", "来的" }, new[] { "了", "不", "去" }),
                new DiItem("zh-l52-di-2", "easy", "Hôm qua đã mua sách", "我昨天买了两本书", "wǒ zuótiān mǎi le liǎng běn shū", "Hôm qua tôi đã mua 2 quyển sách.", new[] { "我昨天", "买了", "两本书" }, new[] { "的", "没", "二" }),
                new DiItem("zh-l52-di-3", "medium", "Hôm qua không đi học", "昨天他没去学校", "zuótiān tā méi qù xuéxiào", "Hôm qua anh ấy không đi học.", new[] { "昨天", "他", "没去", "学校" }, new[] { "了", "不去", "的" }),
                new DiItem("zh-l52-di-4", "medium", "Sinh ra ở Hà Nội", "我是在河内出生的", "wǒ shì zài Hénèi chūshēng de", "Tôi sinh ra ở Hà Nội.", new[] { "我是", "在河内", "出生的" }, new[] { "了", "没", "去" }),
                new DiItem("zh-l52-di-5", "hard", "Vẫn chưa ăn cơm", "我还没吃午饭呢", "wǒ hái méi chī wǔfàn ne", "Tôi vẫn chưa ăn trưa đâu.", new[] { "我", "还没", "吃午饭", "呢" }, new[] { "了", "不吃", "的" }),
                new DiItem("zh-l52-di-6", "hard", "Bạn đến khi nào?", "你是什么时候来中国的", "nǐ shì shénme shíhou lái Zhōngguó de", "Bạn đến Trung Quốc khi nào thế?", new[] { "你是", "什么时候", "来中国的" }, new[] { "了", "怎么", "哪儿" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l52-pr-1", "easy", "Khung 是...的", "我是昨天来的", "wǒ shì zuótiān lái de", "Tôi đến vào hôm qua.", new[] { "是", "昨天", "来的" }, "Uốn lưỡi shì (thanh 4), đọc rõ zuó đầu lưỡi thẳng thanh 2, de thanh nhẹ.", new[] { "shì uốn lưỡi thanh 4", "zuótiān thanh 2 + 1", "de thả lỏng thanh nhẹ" }, "我是昨天晚上来的。", new[] { "昨天", "来的" }),
                new PrItem("zh-l52-pr-2", "easy", "Chữ 去年 (qùnián)", "我是去年毕业的", "wǒ shì qùnián bìyè de", "Tôi tốt nghiệp năm ngoái.", new[] { "去年", "毕业" }, "Bật hơi tròn môi ở qù (thanh 4) và kéo nhẹ nián (thanh 2).", new[] { "qù bật hơi q tròn môi", "nián thanh 2 đi lên", "bìyè cả hai thanh 4 gọn" }, "我姐姐是去年大学毕业的。", new[] { "去年", "毕业" }),
                new PrItem("zh-l52-pr-3", "medium", "Cặp 出生 (chūshēng)", "我在河内出生的", "wǒ zài Hénèi chūshēng de", "Tôi sinh ra ở Hà Nội.", new[] { "出生" }, "Cả chū và shēng đều uốn lưỡi thanh 1 cao phẳng; bật hơi ở chū.", new[] { "chū bật hơi uốn lưỡi cao", "shēng uốn lưỡi thanh 1 cao", "de thanh nhẹ" }, "我是在河内出生的。", new[] { "在河内", "出生" }),
                new PrItem("zh-l52-pr-4", "medium", "Từ vựng 认识 (rènshi)", "你们是怎么认识的", "nǐmen shì zěnme rènshi de", "Hai bạn quen nhau thế nào?", new[] { "怎么", "认识" }, "Uốn lưỡi rèn (thanh 4), chữ shi đọc thanh nhẹ.", new[] { "zěnme thanh 3 + nhẹ", "rèn uốn lưỡi thanh 4", "shi nhẹ không kéo dài" }, "我们是在大学认识的。", new[] { "认识" }),
                new PrItem("zh-l52-pr-5", "hard", "Từ vựng 飞机 & 火车", "坐飞机还是坐火车", "zuò fēijī háishi zuò huǒchē", "Đi máy bay hay đi tàu hỏa?", new[] { "飞机", "火车" }, "Giữ thanh 1 cao ở fēijī và bật hơi uốn lưỡi chē (huǒchē).", new[] { "fēijī cả hai thanh 1 cao", "huǒ thanh 3 trầm", "chē bật hơi uốn lưỡi thanh 1" }, "我是从河内坐飞机来的。", new[] { "坐飞机", "来的" }),
                new PrItem("zh-l52-pr-6", "hard", "Cụm 已经...了", "我已经吃过早饭了", "wǒ yǐjīng chī guo zǎofàn le", "Tôi đã ăn sáng rồi.", new[] { "已经", "早饭" }, "Đọc rõ jīng thanh 1 cao và chī uốn lưỡi bật hơi thanh 1.", new[] { "yǐ thanh 3", "jīng mặt lưỡi thanh 1 cao", "chī bật hơi uốn lưỡi" }, "我已经买好火车票了。", new[] { "已经", "了" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l52-wm-1", "easy", "Sơ đồ từ vựng: 是...的 (Nhấn mạnh quá khứ)", "是...的", "shì... de", "Nhấn mạnh Thời gian / Nơi / Cách thức đã qua",
                    new[] { new WmNode("是什么时候来的", "shì shénme shíhou lái de", "Đến khi nào thế?", "我是去年九月来的。"), new WmNode("是怎么来的", "shì zěnme lái de", "Đến bằng gì thế?", "我是坐飞机来的。") },
                    new[] { new WmNode("是在哪儿买的", "shì zài nǎr mǎi de", "Mua ở đâu thế?", "是在网上买的。") },
                    new WmNode[] { new WmNode("不是...的", "bú shì... de", "Không phải ... đâu", "我不是一个人来的。") },
                    new[] { new WmEx("我是在河内出生的，去年才来北京工作。", "wǒ shì zài Hénèi chūshēng de, qùnián cái lái Běijīng gōngzuò", "Tôi sinh ra ở Hà Nội, năm ngoái mới sang Bắc Kinh làm việc.") }),
                new WmItem("zh-l52-wm-2", "medium", "Sơ đồ từ vựng: 了 (Hoàn thành & Thay đổi)", "了", "le", "Đã xong / Thay đổi trạng thái",
                    new[] { new WmNode("买了一本书", "mǎi le yì běn shū", "Đã mua 1 quyển sách", "昨天我买了一本书。"), new WmNode("下雨了 / 天冷了", "xiàyǔ le / tiān lěng le", "Mưa rồi / Trời lạnh rồi", "天气冷了，多穿衣服。") },
                    new WmNode[] { new WmNode("没 + Động từ (bỏ 了)", "méi + V", "Đã không làm...", "昨天我没去。") },
                    new WmNode[] { new WmNode("还没...呢", "hái méi... ne", "Vẫn chưa...", "我还没吃饭呢。") },
                    new[] { new WmEx("我已经下班了，正在回家的路上。", "wǒ yǐjīng xiàbān le, zhèngzài huíjiā de lùshang", "Tôi tan làm rồi, đang trên đường về nhà.") }),
                new WmItem("zh-l52-wm-3", "hard", "Sơ đồ từ vựng: 去年 / 毕业 / 认识", "经历", "qùnián / bìyè / rènshi", "Cột mốc & Trải nghiệm quá khứ",
                    new[] { new WmNode("大学毕业", "dàxué bìyè", "Tốt nghiệp đại học", "他是去年大学毕业的。"), new WmNode("怎么认识的", "zěnme rènshi de", "Quen nhau thế nào", "我们是在公司认识的。") },
                    new WmNode[] { new WmNode("和朋友一起", "hé péngyou yìqǐ", "Cùng với bạn", "我是和朋友一起来的。") },
                    new WmNode[] { new WmNode("送给我", "sòng gěi wǒ", "Tặng cho tôi", "这是他送给我的。") },
                    new[] { new WmEx("我是在大学一年级认识她的，我们现在还是好朋友。", "wǒ shì zài dàxué yì niánjí rènshi tā de, wǒmen xiànzài háishi hǎo péngyou", "Tôi quen cô ấy hồi năm nhất đại học, giờ chúng tôi vẫn là bạn thân.") })
            },
            cloze: new[]
            {
                new ClItem("zh-l52-cl-1", "easy", "Nhấn mạnh quá khứ 是...的", "Hoàn thiện câu 'Tôi đi máy bay đến Bắc Kinh':", "我{{0}}坐飞机来北京{{1}}。", new[] { new ClBlank(new[] { "是", "shì" }, "là (shì)", "Khung nhấn mạnh quá khứ 是...的."), new ClBlank(new[] { "的", "de" }, "đứng cuối câu (de)", "Đặt 的 sau động từ/địa danh cuối câu.") }, "我是坐飞机来北京的。", "wǒ shì zuò fēijī lái Běijīng de", "Tôi đi máy bay đến Bắc Kinh đấy.", "Nhấn mạnh phương thức di chuyển đã xảy ra."),
                new ClItem("zh-l52-cl-2", "easy", "Phủ định hành động quá khứ 没", "Điền từ phủ định đúng cho hành động hôm qua (chú ý không có 了):", "昨天我{{0}}去图书馆。", new[] { new ClBlank(new[] { "没", "没有", "méi", "méiyǒu" }, "đã không (méi / méiyǒu)", "Phủ định quá khứ dùng 没(有) và bỏ 了.") }, "昨天我没去图书馆。", "zuótiān wǒ méi qù túshūguǎn", "Hôm qua tôi không đi thư viện.", "Không dùng 不 cho hành động cụ thể đã qua."),
                new ClItem("zh-l52-cl-3", "medium", "Chưa làm việc gì 还没...呢", "Hoàn thiện câu 'Tôi vẫn chưa ăn tối đâu':", "我{{0}}吃晚饭{{1}}。", new[] { new ClBlank(new[] { "还没", "还没有", "hái méi" }, "vẫn chưa (hái méi)", "Chưa hoàn thành hành động dùng 还没."), new ClBlank(new[] { "呢", "ne" }, "trợ từ cuối câu (ne)", "Cuối câu 还没... thường kết hợp 呢.") }, "我还没吃晚饭呢。", "wǒ hái méi chī wǎnfàn ne", "Tôi vẫn chưa ăn tối đâu.", "Phân biệt với 我不吃晚饭了 (tôi không ăn nữa)."),
                new ClItem("zh-l52-cl-4", "hard", "Phủ định của 是...的", "Phủ định phần phương thức 'Không phải đi tàu hỏa đến':", "我{{0}}坐火车来的，我是坐飞机来的。", new[] { new ClBlank(new[] { "不是", "bú shì" }, "không phải (bú shì)", "Phủ định câu nhấn mạnh bắt buộc dùng 不是...的.") }, "我不是坐火车来的，我是坐飞机来的。", "wǒ bú shì zuò huǒchē lái de, wǒ shì zuò fēijī lái de", "Không phải tôi đi tàu hỏa đến, tôi đi máy bay đến.", "Chữ 是 không được lược khi ở thể phủ định.")
            },
            translate: new[]
            {
                new TrItem("zh-l52-tr-1", "easy", "Đến bằng máy bay", "Tôi đi máy bay đến Trung Quốc.", new[] { "是", "坐飞机", "来的" }, new[] { "我是坐飞机来中国的。", "我是坐飞机来的。" }, "我是坐飞机来中国的。", "wǒ shì zuò fēijī lái Zhōngguó de.", "Khung nhấn mạnh phương thức 是...的."),
                new TrItem("zh-l52-tr-2", "easy", "Hôm qua không mua", "Hôm qua tôi không mua quần áo.", new[] { "昨天", "没买", "衣服" }, new[] { "昨天我没买衣服。", "我昨天没有买衣服。" }, "昨天我没买衣服。", "zuótiān wǒ méi mǎi yīfu.", "Dùng 没买 và tuyệt đối không thêm 了."),
                new TrItem("zh-l52-tr-3", "medium", "Hỏi nơi mua đồ", "Chiếc điện thoại này bạn mua ở đâu thế?", new[] { "这个手机", "在哪儿", "买的" }, new[] { "这个手机你是哪儿买的？", "这个手机你是在哪儿买的？", "你是在哪儿买这个手机的？" }, "这个手机你是在哪儿买的？", "zhège shǒujī nǐ shì zài nǎr mǎi de?", "Hỏi chi tiết địa điểm của việc đã mua xong dùng 是...的."),
                new TrItem("zh-l52-tr-4", "hard", "Quen nhau ở đại học", "Tôi và anh ấy quen nhau vào năm ngoái ở trường đại học.", new[] { "去年", "在大学", "认识的" }, new[] { "我和他是去年在大学认识的。", "我们是去年在大学认识的。" }, "我和他是去年在大学认识的。", "wǒ hé tā shì qùnián zài dàxué rènshi de.", "Thời gian trước, Địa điểm sau, cùng bọc trong 是...的.")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l52-gf-1", "easy", "Giữ 了 trong câu phủ định 没", "昨天他没去了学校。", "Đã dùng 没 phủ định quá khứ thì BẮT BUỘC phải bỏ 了.", "昨天他没去学校。", new[] { "昨天他没去学校。", "他昨天没有去学校。" }, "zuótiān tā méi qù xuéxiào", "Hôm qua anh ấy không đến trường.", "没 + Động từ (cấm thêm 了)."),
                new GfItem("zh-l52-gf-2", "medium", "Dùng chung 了 và 是...的", "我是昨天坐飞机来了的。", "Không dùng 了 bên trong khung nhấn mạnh 是...的.", "我是昨天坐飞机来的。", new[] { "我是昨天坐飞机来的。", "我是昨天坐飞机来的" }, "wǒ shì zuótiān zuò fēijī lái de", "Tôi đi máy bay đến vào hôm qua.", "Khung chuẩn: 是 + [Bối cảnh] + Động từ + 的."),
                new GfItem("zh-l52-gf-3", "hard", "Thiếu 是 khi phủ định câu 是...的", "我坐火车不来的，我坐飞机来的。", "Phủ định của 是...的 phải đặt 不 trước 是 (不是...的).", "我不是坐火车来的，我是坐飞机来的。", new[] { "我不是坐火车来的，我是坐飞机来的。", "我不是坐火车来的。" }, "wǒ bú shì zuò huǒchē lái de, wǒ shì zuò fēijī lái de", "Không phải tôi đi tàu hỏa đến, tôi đi máy bay đến.", "不是 + ... + 的.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l52-rp-1", "easy", "Kể thời gian & cách đến Trung Quốc", "Một người bạn mới gặp hỏi bạn sang Trung Quốc khi nào và đi bằng phương tiện gì.", "Bạn Trần Lệ", "明，你是什么时候来中国的？坐飞机来的吗？", "Míng, nǐ shì shénme shíhou lái Zhōngguó de? zuò fēijī lái de ma?", "Minh ơi, cậu sang Trung Quốc khi nào thế? Đi máy bay sang à?", "Trả lời bằng cấu trúc 是...的 nêu thời gian (去年/上个月/昨天) và phương tiện (坐飞机/坐火车).", new[] { "是|shi", "的|de", "去年|上个月|昨天|飞机|火车" }, new[] { "我是去年九月从河内坐飞机来的。" }, "wǒ shì qùnián jiǔ yuè cóng Hénèi zuò fēijī lái de.", 8),
                new RpItem("zh-l52-rp-2", "medium", "Hỏi thăm mua đồ mới", "Bạn khen chiếc áo/điện thoại mới của bạn mình và hỏi mua ở đâu, bao nhiêu tiền.", "Bạn Trương Vĩ", "你看，这是我昨天刚买的新电脑！", "nǐ kàn, zhè shì wǒ zuótiān gāng mǎi de xīn diànnǎo!", "Cậu nhìn xem, đây là máy tính mới mình vừa mua hôm qua đấy!", "Khen đẹp (太好看了/真漂亮) và dùng 是...的 hỏi mua ở đâu (在哪儿买的) hoặc ai tặng.", new[] { "在哪儿|网上|商店", "买的|de", "好看|漂亮|多少钱" }, new[] { "真漂亮！请问你是在网上买的，还是在商店买的？" }, "zhēn piàoliang! qǐngwèn nǐ shì zài wǎngshàng mǎi de, háishi zài shāngdiàn mǎi de?", 8),
                new RpItem("zh-l52-rp-3", "hard", "Hỏi đã ăn chưa & chưa làm", "Trưa nay đồng nghiệp rủ đi ăn và hỏi bạn ăn chưa, hôm qua bạn có đi học không.", "Đồng nghiệp", "已经十二点了，你吃午饭了吗？", "yǐjīng shí'èr diǎn le, nǐ chī wǔfàn le ma?", "Đã 12 giờ rồi, cậu ăn trưa chưa?", "Trả lời chưa ăn bằng 还没...呢 (hoặc đã ăn rồi bằng 已经...了) và rủ đi ăn cùng.", new[] { "还没|已经|没有", "呢|了", "吃|一起|去" }, new[] { "我还没吃午饭呢，我们现在一起去吃饭吧！" }, "wǒ hái méi chī wǔfàn ne, wǒmen xiànzài yìqǐ qù chīfàn ba!", 8)
            }, now));

        // ---------------- LESSON 5.3 ----------------
        all.AddRange(BuildLessonPack(cId, ChineseLesson53Id, "zh-CN", "Trung HSK1 · Bài 5.3",
            shadowing: new[]
            {
                new ShItem("zh-l53-sh-1", "easy", "Hỏi triệu chứng bệnh (Dễ)", "我有点儿发烧，头也很疼。", "wǒ yǒudiǎnr fāshāo, tóu yě hěn téng", "Tôi hơi bị sốt, đầu cũng rất đau.", new[] { "发烧", "头疼" }),
                new ShItem("zh-l53-sh-2", "easy", "Uống thuốc nghỉ ngơi (Dễ)", "医生让我按时吃药，好好休息。", "yīshēng ràng wǒ ànshí chīyào, hǎohao xiūxi", "Bác sĩ bảo tôi uống thuốc đúng giờ, nghỉ ngơi cho khỏe.", new[] { "吃药", "休息" }),
                new ShItem("zh-l53-sh-3", "easy", "Khoảng cách gần 离...很近 (Dễ)", "学校离地铁站很近。", "xuéxiào lí dìtiězhàn hěn jìn", "Trường học cách trạm tàu điện ngầm rất gần.", new[] { "离", "很近" }),
                new ShItem("zh-l53-sh-4", "medium", "Thời gian di chuyển (Vừa)", "从这儿坐出租车到机场要四十分钟。", "cóng zhèr zuò chūzūchē dào jīchǎng yào sìshí fēnzhōng", "Từ đây đi taxi ra sân bay mất 40 phút.", new[] { "从...到...", "要" }),
                new ShItem("zh-l53-sh-5", "medium", "Tàu cao tốc Trung Quốc (Vừa)", "这是我第一次坐中国的高铁。", "zhè shì wǒ dì-yī cì zuò Zhōngguó de gāotiě", "Đây là lần đầu tiên tôi đi tàu cao tốc Trung Quốc.", new[] { "第一次", "高铁" }),
                new ShItem("zh-l53-sh-6", "medium", "Đặt vé & khách sạn (Vừa)", "我已经在网上订好酒店了。", "wǒ yǐjīng zài wǎngshàng dìng hǎo jiǔdiàn le", "Tôi đã đặt xong khách sạn trên mạng rồi.", new[] { "订好", "酒店" }),
                new ShItem("zh-l53-sh-7", "hard", "Khoảng thời gian 两个小时 (Khó)", "我每天晚上学习两个小时汉语。", "wǒ měitiān wǎnshang xuéxí liǎng ge xiǎoshí Hànyǔ", "Mỗi tối tôi học tiếng Trung 2 tiếng đồng hồ.", new[] { "两个小时" }),
                new ShItem("zh-l53-sh-8", "hard", "Lời khuyên 多喝水 (Khó)", "感冒的时候要多喝热水，少看手机。", "gǎnmào de shíhou yào duō hē rèshuǐ, shǎo kàn shǒujī", "Lúc bị cảm phải uống nhiều nước ấm, ít xem điện thoại.", new[] { "多喝热水", "少看" }),
                new ShItem("zh-l53-sh-9", "hard", "Hỏi đường đi mất bao lâu (Khó)", "请问，从北京坐高铁到上海要多长时间？", "qǐngwèn, cóng Běijīng zuò gāotiě dào Shànghǎi yào duō cháng shíjiān?", "Xin hỏi từ Bắc Kinh đi tàu cao tốc đến Thượng Hải mất bao lâu?", new[] { "从...到...", "多长时间" })
            },
            dictation: new[]
            {
                new DiItem("zh-l53-di-1", "easy", "Tôi bị cảm rồi", "我感冒了有点儿发烧", "wǒ gǎnmào le yǒudiǎnr fāshāo", "Tôi bị cảm rồi, hơi sốt một chút.", new[] { "我", "感冒了", "有点儿", "发烧" }, new[] { "喝药", "很近", "吗" }),
                new DiItem("zh-l53-di-2", "easy", "Uống nhiều nước ấm", "请多喝热水好好休息", "qǐng duō hē rèshuǐ hǎohao xiūxi", "Hãy uống nhiều nước ấm và nghỉ ngơi cho khỏe.", new[] { "请", "多喝热水", "好好休息" }, new[] { "喝水多", "吃药", "了" }),
                new DiItem("zh-l53-di-3", "medium", "Nhà cách công ty rất gần", "我家离公司很近", "wǒ jiā lí gōngsī hěn jìn", "Nhà tôi cách công ty rất gần.", new[] { "我家", "离", "公司", "很近" }, new[] { "从", "到", "远" }),
                new DiItem("zh-l53-di-4", "medium", "Mất 1 tiếng đồng hồ", "到机场要一个小时", "dào jīchǎng yào yí ge xiǎoshí", "Đi ra sân bay mất 1 tiếng đồng hồ.", new[] { "到机场", "要", "一个", "小时" }, new[] { "一点", "离", "的" }),
                new DiItem("zh-l53-di-5", "hard", "Lần đầu tiên đi tàu cao tốc", "这是我第一次坐高铁", "zhè shì wǒ dì-yī cì zuò gāotiě", "Đây là lần đầu tiên tôi đi tàu cao tốc.", new[] { "这是", "我", "第一次", "坐高铁" }, new[] { "一个", "坐飞机", "了" }),
                new DiItem("zh-l53-di-6", "hard", "Từ Bắc Kinh đến Thượng Hải", "从北京到上海要五个小时", "cóng Běijīng dào Shànghǎi yào wǔ ge xiǎoshí", "Từ Bắc Kinh đến Thượng Hải mất 5 tiếng.", new[] { "从北京", "到上海", "要", "五个小时" }, new[] { "离", "五点", "吃药" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l53-pr-1", "easy", "Cụm 吃药 & 休息", "按时吃药好好休息", "ànshí chīyào hǎohao xiūxi", "Uống thuốc đúng giờ, nghỉ ngơi cho tốt.", new[] { "吃药", "休息" }, "Bật hơi uốn lưỡi chī (thanh 1) và giữ thanh 1 cao phẳng xiū.", new[] { "chī bật hơi uốn lưỡi thanh 1", "yào thanh 4 dứt khoát", "xiūxi thanh 1 + nhẹ" }, "回家按时吃药，好好休息吧。", new[] { "吃药", "休息" }),
                new PrItem("zh-l53-pr-2", "easy", "Triệu chứng 发烧 (fāshāo)", "我今天有点儿发烧", "wǒ jīntiān yǒudiǎnr fāshāo", "Hôm nay tôi hơi sốt.", new[] { "发烧" }, "Giữ cao độ 55 cho cả hai âm tiết fā và uốn lưỡi shāo.", new[] { "fā thanh 1 cao", "shāo uốn lưỡi thanh 1 cao", "yǒudiǎnr biến điệu 2 + 3" }, "我感冒了，有点儿发烧。", new[] { "感冒", "发烧" }),
                new PrItem("zh-l53-pr-3", "medium", "Cụm 多长时间", "坐高铁要多长时间", "zuò gāotiě yào duō cháng shíjiān", "Đi tàu cao tốc mất bao lâu?", new[] { "高铁", "多长", "时间" }, "Bật hơi uốn lưỡi cháng (thanh 2) và uốn lưỡi shí (thanh 2).", new[] { "gāotiě thanh 1 + 3", "cháng uốn lưỡi bật hơi", "shíjiān thanh 2 + 1" }, "从这儿到机场要多长时间？", new[] { "多长时间" }),
                new PrItem("zh-l53-pr-4", "medium", "Khoảng thời lượng 小时", "我坐了两个小时", "wǒ zuò le liǎng ge xiǎoshí", "Tôi đã ngồi xe 2 tiếng đồng hồ.", new[] { "两个", "小时" }, "Áp dụng biến điệu thanh 3 giữa liǎng và xiǎo (liáng ge xiǎoshí).", new[] { "zuò đầu lưỡi thẳng thanh 4", "xiǎo hạ sâu thanh 3", "shí uốn lưỡi thanh 2" }, "从河内坐飞机到北京要四个小时。", new[] { "四个小时" }),
                new PrItem("zh-l53-pr-5", "hard", "Từ vựng 第一次 (dì-yī cì)", "这是我第一次坐高铁", "zhè shì wǒ dì-yī cì zuò gāotiě", "Đây là lần đầu tiên tôi đi tàu cao tốc.", new[] { "第一次", "高铁" }, "Bật hơi đầu lưỡi thẳng ở cì (thanh 4) và giữ yī thanh 1 sau 第.", new[] { "dì thanh 4", "yī giữ nguyên thanh 1", "cì bật hơi đầu lưỡi thẳng" }, "这是我第一次来中国旅游。", new[] { "第一次" }),
                new PrItem("zh-l53-pr-6", "hard", "Từ vựng 订酒店 (dìng jiǔdiàn)", "我已经订好酒店了", "wǒ yǐjīng dìng hǎo jiǔdiàn le", "Tôi đã đặt xong khách sạn rồi.", new[] { "已经", "订好", "酒店" }, "Đọc chắc thanh 4 ở dìng và biến điệu thanh 3 rõ ở jiǔ.", new[] { "dìng thanh 4 gọn", "hǎo hạ trầm thanh 3", "jiǔdiàn thanh 3 + 4" }, "明天去上海，我已经订好机票和酒店了。", new[] { "订好", "酒店" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l53-wm-1", "easy", "Sơ đồ từ vựng: 生病 / 吃药 (Bệnh viện)", "生病・看病", "shēngbìng / kànbìng", "Bị ốm / Đi khám bệnh",
                    new[] { new WmNode("发烧 / 头疼 / 感冒", "fāshāo / tóuténg / gǎnmào", "Sốt / Đau đầu / Cảm", "我发烧了，头很疼。"), new WmNode("吃药", "chīyào", "Uống thuốc (dùng 吃)", "按时吃药。") },
                    new[] { new WmNode("多喝水 / 好好休息", "duō hē shuǐ / hǎohao xiūxi", "Uống nhiều nước / Nghỉ ngơi", "多喝热水，好好休息。") },
                    new WmNode[] { new WmNode("身体健康", "shēntǐ jiànkāng", "Sức khỏe dồi dào", "祝您身体健康！") },
                    new[] { new WmEx("我今天生病了，想去医院看病，不能去上班。", "wǒ jīntiān shēngbìng le, xiǎng qù yīyuàn kànbìng, bù néng qù shàngbān", "Hôm nay tôi bị ốm rồi, muốn đi bệnh viện khám, không đi làm được.") }),
                new WmItem("zh-l53-wm-2", "medium", "Sơ đồ từ vựng: 离 & 从...到... (Khoảng cách)", "离・从...到...", "lí / cóng... dào...", "Cách (khoảng cách) & Từ... đến...",
                    new[] { new WmNode("A 离 B 很近 / 远", "A lí B hěn jìn / yuǎn", "A cách B rất gần / xa", "学校离医院很近。"), new WmNode("从 A 到 B 要...", "cóng A dào B yào...", "Từ A đến B mất...", "从北京到上海要五小时。") },
                    new WmNode[] { new WmNode("坐高铁 / 打车", "zuò gāotiě / dǎchē", "Đi tàu cao tốc / Gọi xe", "打车十分钟就到了。") },
                    new WmNode[] { new WmNode("多长时间", "duō cháng shíjiān", "Bao lâu?", "坐飞机要多长时间？") },
                    new[] { new WmEx("我家离地铁站很近，从我家到公司只要二十分钟。", "wǒ jiā lí dìtiězhàn hěn jìn, cóng wǒ jiā dào gōngsī zhǐ yào èrshí fēnzhōng", "Nhà tôi cách trạm tàu điện ngầm rất gần, từ nhà đến công ty chỉ mất 20 phút.") }),
                new WmItem("zh-l53-wm-3", "hard", "Sơ đồ từ vựng: 订 / 小时 / 第 (Đặt vé & Thời lượng)", "订・小时・第", "dìng / xiǎoshí / dì", "Đặt trước / Tiếng đồng hồ / Số thứ tự",
                    new[] { new WmNode("订机票 / 订酒店", "dìng jīpiào / jiǔdiàn", "Đặt vé máy bay / khách sạn", "我已经订好酒店了。"), new WmNode("两个小时", "liǎng ge xiǎoshí", "2 tiếng đồng hồ", "学习两个小时。") },
                    new WmNode[] { new WmNode("第一次", "dì-yī cì", "Lần đầu tiên", "这是我第一次来北京。") },
                    new WmNode[] { new WmNode("一点 vs 一个小时", "yì diǎn vs yí ge xiǎoshí", "1 giờ vs 1 tiếng", "一点钟 / 一个小时") },
                    new[] { new WmEx("这是我第一次坐高铁，两个小时就到了，太快了！", "zhè shì wǒ dì-yī cì zuò gāotiě, liǎng ge xiǎoshí jiù dào le, tài kuài le", "Đây là lần đầu tôi đi tàu cao tốc, 2 tiếng đã tới rồi, nhanh quá!") })
            },
            cloze: new[]
            {
                new ClItem("zh-l53-cl-1", "easy", "Động từ đi với thuốc 吃药", "Điền động từ đúng cho 'uống thuốc' trong tiếng Trung:", "你感冒了，记得按时{{0}}药。", new[] { new ClBlank(new[] { "吃", "chī", "chi1" }, "uống/ăn thuốc (chī)", "Tiếng Trung nói 吃药, không nói 喝药.") }, "你感冒了，记得按时吃药。", "nǐ gǎnmào le, jìde ànshí chīyào", "Bạn bị cảm rồi, nhớ uống thuốc đúng giờ.", "Mẹo nhớ: Thuốc viên dùng 吃药."),
                new ClItem("zh-l53-cl-2", "easy", "Khoảng cách dùng 离", "Điền giới từ chỉ khoảng cách giữa hai địa điểm:", "医院{{0}}我家很近，走路五分钟就到了。", new[] { new ClBlank(new[] { "离", "lí", "li2" }, "cách (lí)", "A 离 B + 很近 / 很远.") }, "医院离我家很近，走路五分钟就到了。", "yīyuàn lí wǒ jiā hěn jìn, zǒulù wǔ fēnzhōng jiù dào le", "Bệnh viện cách nhà tôi rất gần, đi bộ 5 phút là tới.", "Không dùng 从 cho khoảng cách tĩnh."),
                new ClItem("zh-l53-cl-3", "medium", "Thời lượng 个小时", "Điền đơn vị đo khoảng thời gian 'tiếng đồng hồ':", "从北京坐高铁到上海要五个{{0}}。", new[] { new ClBlank(new[] { "小时", "xiǎoshí" }, "tiếng đồng hồ (xiǎoshí)", "Khoảng thời gian dùng 个小时, không dùng 点.") }, "从北京坐高铁到上海要五个小时。", "cóng Běijīng zuò gāotiě dào Shànghǎi yào wǔ ge xiǎoshí", "Từ Bắc Kinh đi tàu cao tốc đến Thượng Hải mất 5 tiếng.", "Phân biệt 点 (giờ trên đồng hồ) và 小时 (tiếng đồng hồ)."),
                new ClItem("zh-l53-cl-4", "hard", "Khuyên nhủ 多 + Động từ", "Sắp xếp 'uống nhiều nước' đúng trật tự tiếng Trung:", "你发烧了，要{{0}}水，好好休息。", new[] { new ClBlank(new[] { "多喝", "duō hē" }, "uống nhiều (duō hē)", "Phó từ 多 đặt TRƯỚC động từ 喝.") }, "你发烧了，要多喝水，好好休息。", "nǐ fāshāo le, yào duō hē shuǐ, hǎohao xiūxi", "Bạn bị sốt rồi, phải uống nhiều nước, nghỉ ngơi cho khỏe.", "多 + Động từ + Tân ngữ.")
            },
            translate: new[]
            {
                new TrItem("zh-l53-tr-1", "easy", "Hơi sốt và đau đầu", "Hôm nay tôi hơi sốt, đầu cũng rất đau.", new[] { "有点儿发烧", "头", "疼" }, new[] { "今天我有点儿发烧，头也很疼。", "我今天有点儿发烧，头很疼。" }, "今天我有点儿发烧，头也很疼。", "jīntiān wǒ yǒudiǎnr fāshāo, tóu yě hěn téng.", "Dùng 有点儿 trước 发烧."),
                new TrItem("zh-l53-tr-2", "easy", "Cách trường học rất gần", "Nhà tôi cách trường học rất gần.", new[] { "我家", "离", "学校", "很近" }, new[] { "我家离学校很近。", "我家离学校很近" }, "我家离学校很近。", "wǒ jiā lí xuéxiào hěn jìn.", "Mẫu: A 离 B 很近."),
                new TrItem("zh-l53-tr-3", "medium", "Lần đầu tiên đi tàu cao tốc", "Đây là lần đầu tiên tôi đi tàu cao tốc.", new[] { "这是", "第一次", "坐高铁" }, new[] { "这是我第一次坐高铁。", "这是我第一次坐高铁" }, "这是我第一次坐高铁。", "zhè shì wǒ dì-yī cì zuò gāotiě.", "Số thứ tự: 第 + 一 + 次."),
                new TrItem("zh-l53-tr-4", "hard", "Hỏi thời gian di chuyển", "Từ đây đi taxi đến sân bay mất bao lâu?", new[] { "从这儿", "打车到机场", "要多长时间" }, new[] { "从这儿打车到机场要多长时间？", "从这儿坐出租车到机场要多长时间？" }, "从这儿打车到机场要多长时间？", "cóng zhèr dǎchē dào jīchǎng yào duō cháng shíjiān?", "Từ... đến... đi phương tiện... mất bao lâu.")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l53-gf-1", "easy", "Dùng 喝药 thay cho 吃药", "生病了要按时喝药。", "Uống thuốc trong tiếng Trung dùng 吃药 (không dùng 喝药).", "生病了要按时吃药。", new[] { "生病了要按时吃药。", "生病了要吃药。" }, "shēngbìng le yào ànshí chīyào", "Bị ốm rồi phải uống thuốc đúng giờ.", "吃药 = uống thuốc."),
                new GfItem("zh-l53-gf-2", "medium", "Dùng 点 cho khoảng thời gian", "我每天工作八点。", "8 tiếng đồng hồ phải dùng 八个小时 (八点 là 8 giờ sáng).", "我每天工作八个小时。", new[] { "我每天工作八个小时。", "我每天工作八小时。" }, "wǒ měitiān gōngzuò bā ge xiǎoshí", "Mỗi ngày tôi làm việc 8 tiếng đồng hồ.", "Giờ đồng hồ = 点; Thời lượng = 个小时."),
                new GfItem("zh-l53-gf-3", "hard", "Đặt 多 sau danh từ", "医生说要喝热水多。", "Phó từ 多 phải đặt TRƯỚC động từ 喝 (多喝热水).", "医生说要多喝热水。", new[] { "医生说要多喝热水。", "医生说要多喝水。" }, "yīshēng shuō yào duō hē rèshuǐ", "Bác sĩ bảo phải uống nhiều nước ấm.", "多/少 + Động từ.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l53-rp-1", "easy", "Đi khám bệnh tại phòng khám", "Bác sĩ hỏi bạn cảm thấy khó chịu ở đâu và bị từ khi nào.", "Bác sĩ", "您好！请问您哪儿不舒服？", "nínhǎo! qǐngwèn nín nǎr bù shūfu?", "Xin chào! Xin hỏi anh/chị thấy khó chịu ở đâu ạ?", "Kể triệu chứng bằng 发烧/头疼/感冒 và nói từ hôm qua.", new[] { "发烧|头疼|感冒|不舒服", "有点儿|很", "昨天|今天" }, new[] { "医生您好，我从昨天开始感冒了，有点儿发烧，头也很疼。" }, "yīshēng nínhǎo, wǒ cóng zuótiān kāishǐ gǎnmào le, yǒudiǎnr fāshāo, tóu yě hěn téng.", 8),
                new RpItem("zh-l53-rp-2", "medium", "Hỏi đường & thời gian ra sân bay", "Bạn ở quầy lễ tân khách sạn hỏi đường ra sân bay và mất bao lâu.", "Lễ tân khách sạn", "早上好！请问有什么可以帮您的？", "zǎoshang hǎo! qǐngwèn yǒu shénme kěyǐ bāng nín de?", "Chào buổi sáng! Xin hỏi tôi có thể giúp gì cho quý khách?", "Hỏi sân bay cách đây có xa không (离这儿远吗) hoặc đi taxi/tàu điện mất bao lâu (要多长时间).", new[] { "机场|ji chang", "离|从|到", "多长时间|远|近|小时|分钟" }, new[] { "请问，机场离这儿远吗？打车到机场要多长时间？" }, "qǐngwèn, jīchǎng lí zhèr yuǎn ma? dǎchē dào jīchǎng yào duō cháng shíjiān?", 8),
                new RpItem("zh-l53-rp-3", "hard", "Lên kế hoạch đi tàu cao tốc sang Thượng Hải", "Bạn bè hỏi bạn đã đặt vé tàu/khách sạn chưa và đi tàu cao tốc mất mấy tiếng.", "Bạn Trương Vĩ", "明，你订好去上海的高铁票和酒店了吗？要多长时间？", "Míng, nǐ dìng hǎo qù Shànghǎi de gāotiěpiào hé jiǔdiàn le ma? yào duō cháng shíjiān?", "Minh ơi, cậu đặt vé tàu cao tốc và khách sạn Thượng Hải chưa? Đi mất bao lâu?", "Trả lời đã đặt xong (订好了), nêu số tiếng bằng 个小时 và chia sẻ cảm xúc lần đầu (第一次).", new[] { "订好|已经", "个小时|小时", "第一次|高铁|快|舒服" }, new[] { "我已经订好了！大概四个半小时就到，这是我第一次坐高铁。" }, "wǒ yǐjīng dìng hǎo le! dàgài sì ge bàn xiǎoshí jiù dào, zhè shì wǒ dì-yī cì zuò gāotiě.", 10)
            }, now));

        return all;
    }

    // =========================================================================
    // QUIZZES (12 BÀI QUIZ + 4 ĐỀ CHƯƠNG [Đề Chương 4] & [Đề Chương 5])
    // =========================================================================

    private static async Task EnsureExpandedQuizzesAsync(
        SwmsDbContext db,
        bool hasJapaneseCourse,
        bool hasChineseCourse,
        DateTimeOffset now)
    {
        var specs = new List<(string Key, Guid LessonId, string Title, QuizQuestionSpec[] Questions)>();

        if (hasJapaneseCourse)
        {
            specs.AddRange(GetJapaneseQuizSpecs());
        }
        if (hasChineseCourse)
        {
            specs.AddRange(GetChineseQuizSpecs());
        }

        if (specs.Count == 0)
        {
            return;
        }

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

            var quizId = SeedDataInitializer.CreateDeterministicGuid($"swms:cjk-exp-quiz:{key}");
            var existing = await db.PracticeQuizzes.FirstOrDefaultAsync(q =>
                q.Id == quizId ||
                (q.UserId == SeedDataInitializer.DefaultUserId && q.LessonId == lessonId && q.Title == title));

            if (existing == null)
            {
                db.PracticeQuizzes.Add(new PracticeQuiz
                {
                    Id = quizId,
                    UserId = SeedDataInitializer.DefaultUserId,
                    Title = title,
                    LessonId = lessonId,
                    TotalItems = questions.Length,
                    CorrectItems = 0,
                    Status = "in_progress",
                    StartedAt = now
                });

                for (int i = 0; i < questions.Length; i++)
                {
                    db.PracticeQuizItems.Add(BuildQuizItem(quizId, key, i, questions[i], now));
                }
            }
            else
            {
                var existingItemIds = (await db.PracticeQuizItems
                    .Where(it => it.QuizId == existing.Id)
                    .Select(it => it.Id)
                    .ToListAsync())
                    .ToHashSet();

                if (existingItemIds.Count == 0)
                {
                    for (int i = 0; i < questions.Length; i++)
                    {
                        var item = BuildQuizItem(existing.Id, key, i, questions[i], now);
                        if (!existingItemIds.Contains(item.Id))
                        {
                            db.PracticeQuizItems.Add(item);
                        }
                    }
                    existing.TotalItems = questions.Length;
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static PracticeQuizItem BuildQuizItem(Guid quizId, string key, int idx, QuizQuestionSpec q, DateTimeOffset now)
    {
        var promptJson = JsonSerializer.Serialize(new
        {
            question = q.Question,
            explanationVi = q.ExplanationVi
        }, CjkJsonOptions);

        string? choicesJson = null;
        if (q.ItemType == "multiple_choice")
        {
            choicesJson = JsonSerializer.Serialize(new[]
            {
                new { key = "A", text = q.OptionA ?? string.Empty },
                new { key = "B", text = q.OptionB ?? string.Empty },
                new { key = "C", text = q.OptionC ?? string.Empty },
                new { key = "D", text = q.OptionD ?? string.Empty }
            }, CjkJsonOptions);
        }

        return new PracticeQuizItem
        {
            Id = SeedDataInitializer.CreateDeterministicGuid($"swms:cjk-exp-quiz-item:{key}:{idx + 1}"),
            QuizId = quizId,
            ItemType = q.ItemType,
            PromptAstJson = promptJson,
            ChoicesJson = choicesJson,
            ClozeSolution = q.CorrectSolution,
            AiRubricFeedbackJson = JsonSerializer.Serialize(new { summary = q.ExplanationVi }, CjkJsonOptions),
            CreatedAt = now.AddSeconds(idx)
        };
    }

    private static List<(string Key, Guid LessonId, string Title, QuizQuestionSpec[] Questions)> GetJapaneseQuizSpecs() => new()
    {
        ("ja:l41", JapaneseLesson41Id, "Quiz · Bài 4.1: Tính Từ Đuôi -i & -na, Miêu Tả Sự Vật & Thời Tiết", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Tính từ nào sau đây tuy kết thúc bằng âm い nhưng là TÍNH TỪ ĐUÔI -NA?", "おおきい", "きれい", "あたらしい", "さむい", "B", "きれい (đẹp/sạch) và ゆうめい (nổi tiếng) là tính từ đuôi -na ngoại lệ."),
            new("multiple_choice", "Dạng quá khứ khẳng định đúng của tính từ 'いい (tốt)' là gì?", "いいでした", "いかったです", "よかったです", "よくないです", "C", "Tính từ いい phải đổi thành gốc よ trước khi thêm かったです → よかったです."),
            new("multiple_choice", "Khi dùng phó từ 'あまり (không ... lắm)', động từ/tính từ phía sau bắt buộc chia ở dạng nào?", "Thể khẳng định hiện tại", "Thể phủ định", "Thể mệnh lệnh", "Thể từ điển", "B", "あまり và ぜんぜん luôn đi với thể phủ định."),
            new("cloze", "Hoàn thiện câu nối hai tính từ đuôi -i: 'Nhà hàng này vừa rẻ vừa ngon' → この レストランは やす{{c1::くて}} おいしいです。", null, null, null, null, "くて", "Tính từ đuôi -i bỏ い thêm くて để nối sang tính từ tiếp theo."),
            new("multiple_choice", "Để hỏi 'Hà Nội là thành phố như thế nào?', từ hỏi đúng điền vào chỗ trống 'ハノイは ___ まちですか' là gì?", "どう", "どんな", "どこ", "だれ", "B", "Trước danh từ (まち) phải dùng どんな; đứng một mình mới dùng どうですか.")
        }),

        ("ja:l42", JapaneseLesson42Id, "Quiz · Bài 4.2: Gia Đình, Số Đếm Người/Đồ Vật & Biểu Đạt Nguyên Nhân (〜から)", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Khi nói về bố của MÌNH với người ngoài, dùng từ nào sau đây?", "おとうさん", "ちち", "おにいさん", "ごしゅじん", "B", "ちち/はは dùng cho gia đình mình theo nguyên tắc khiêm nhường Uchi-Soto."),
            new("multiple_choice", "Cách đọc chuẩn của '4 người' trong tiếng Nhật là gì?", "しにん", "よにん", "よんにん", "ふたり", "B", "4 người đọc là よにん để tránh chữ し đồng âm với Tử."),
            new("multiple_choice", "Số lượng '3 cây bút' (lượng từ 本 - ほん) đọc biến âm như thế nào?", "さんほん", "さんぼん", "さんぽん", "みほん", "B", "Số 3 với ほん biến âm thành さんぼん (bật thành b)."),
            new("cloze", "Điền liên từ 'vì... nên...' vào chỗ trống: 'あした テストが あります{{c1::から}}、きょうは べんきょうします。'", null, null, null, null, "から", "Nguyên nhân + から + Kết quả."),
            new("multiple_choice", "Câu nào đặt vị trí số lượng từ ĐÚNG chuẩn ngữ pháp tiếng Nhật?", "りんご ふたつを かいました。", "りんごを ふたつ かいました。", "ふたつ りんごを かいました。", "りんごが ふたつを あります。", "B", "Số lượng từ đứng sau [Danh từ + を/が] và trước động từ, không kèm trợ từ.")
        }),

        ("ja:l43", JapaneseLesson43Id, "Quiz · Bài 4.3: Nơi Chốn Công Cộng, Tồn Tại (あります / います) & Chỉ Dẫn Vị Trí Nâng Cao", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Điền trợ từ đúng cho câu tồn tại: 'つくえの うえ___ ほんが あります。'", "で", "に", "を", "へ", "B", "Địa điểm tồn tại đi với あります/います luôn dùng trợ từ に."),
            new("multiple_choice", "Chủ thể nào sau đây BẮT BUỘC dùng động từ 'います' thay vì 'あります'?", "さくら (hoa anh đào)", "こうばん (đồn cảnh sát)", "ねこ (con mèo)", "かぎ (chìa khóa)", "C", "Người và động vật dùng います; vật vô tri và cây cối dùng あります."),
            new("cloze", "Điền từ chỉ vị trí 'ở giữa': 'ほんやは ぎんこうと えきの {{c1::あいだ}}に あります。'", null, null, null, null, "あいだ", "A と B の あいだ = ở giữa A và B."),
            new("multiple_choice", "Để nói 'Trong lớp học không có ai cả', câu chuẩn là gì?", "きょうしつに だれが いません。", "きょうしつに だれも いません。", "きょうしつに なにも いません。", "きょうしつに だれも ありません。", "B", "Người dùng だれ + も + いません = không ai cả."),
            new("multiple_choice", "Khác biệt giữa 'A と B' và 'A や B など' là gì?", "と liệt kê trọn vẹn; や〜など liệt kê tiêu biểu không đầy đủ", "Cả hai giống hệt nhau", "や chỉ dùng cho người", "と dùng cho câu phủ định", "A", "と liệt kê tất cả các món; や〜など kể đại diện vài món.")
        }),

        ("ja:l51", JapaneseLesson51Id, "Quiz · Bài 5.1: Động Từ Thể Te (〜て形): Yêu Cầu Lịch Sự (〜てください) & Tiếp Diễn (〜ています)", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Thể Te của động từ ngoại lệ 'いきます (đi)' là gì?", "いきて", "いって", "いで", "いんで", "B", "Đuôi き thường thành いて, riêng いきます là ngoại lệ → いって."),
            new("multiple_choice", "Thể Te của động từ Nhóm I 'のみます (uống)' là gì?", "のみて", "のって", "のんで", "のいて", "C", "Động từ Nhóm I kết thúc bằng み・び・に trước ます biến thành んで."),
            new("cloze", "Trả lời phủ định chuẩn cho câu hỏi 'ミラーさんを しっていますか。': 'いいえ、{{c1::しりません}}。'", null, null, null, null, "しりません", "Phủ định của しっています là しりません (không dùng しっていません)."),
            new("multiple_choice", "Câu nào diễn đạt đúng 'Tôi đang sống ở Hà Nội'?", "わたしは ハノイに すみます。", "わたしは ハノイに すんでいます。", "わたしは ハノイで すんで ください。", "わたしは ハノイを すんでいます。", "B", "Trạng thái cư trú lâu dài dùng [Nơi] に すんでいます."),
            new("multiple_choice", "Để ngỏ ý giúp đỡ 'Để tôi xách hành lý giúp bạn nhé?', dùng mẫu câu nào?", "にもつを もって ください。", "にもつを もちましょうか。", "にもつを もっています。", "にもつを もちませんか。", "B", "Vましょうか dùng để đề nghị làm giúp ai đó việc gì.")
        }),

        ("ja:l52", JapaneseLesson52Id, "Quiz · Bài 5.2: Xin Phép (〜てもいいですか), Cấm Đoán (〜てはいけません) & Thứ Tự Hành Động", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Mẫu câu xin phép lịch sự 'Tôi chụp ảnh ở đây có được không?' là gì?", "ここで しゃしんを とって ください。", "ここで しゃしんを とっても いいですか。", "ここで しゃしんを とっては いけません。", "ここで しゃしんを とりました。", "B", "Vても いいですか = Xin phép làm gì có được không."),
            new("multiple_choice", "Động từ 'はいります (đi vào)' thuộc nhóm mấy và có Thể Te là gì?", "Nhóm II → はいりて", "Nhóm I → はいって", "Nhóm III → はいして", "Nhóm I → はいいて", "B", "はいります là động từ Nhóm I giả Nhóm II (đuôi り) → はいって."),
            new("cloze", "Hoàn chỉnh câu cấm đoán: 'きょうしつで たばこを すって{{c1::は}} いけません。'", null, null, null, null, "は", "Mẫu cấm đoán bắt buộc có trợ từ は (đọc là wa): Vては いけません."),
            new("multiple_choice", "Câu 'てを あらってから、ごはんを たべます' có nghĩa là gì?", "Vì rửa tay nên ăn cơm", "Sau khi rửa tay xong rồi mới ăn cơm", "Vừa rửa tay vừa ăn cơm", "Không được rửa tay trước khi ăn", "B", "V1てから、V2 = Sau khi làm V1 xong thì làm V2."),
            new("multiple_choice", "Trong bảo tàng khi từ chối khéo lời xin phép chụp ảnh của khách, người Nhật thường nói gì?", "すみません、ちょっと… (hoặc とっては いけません)", "はい、どうぞ", "とても おいしいです", "よろしく おねがいします", "A", "Dùng すみません、ちょっと… để từ chối mềm mỏng hoặc とっては いけません để nêu quy định.")
        }),

        ("ja:l53", JapaneseLesson53Id, "Quiz · Bài 5.3: Ý Muốn (〜たいです), Kế Hoạch Tương Lai & Hội Thoại Du Lịch/Công Việc Nhật Bản", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Điền trợ từ đúng cho câu muốn sở hữu đồ vật: 'あたらしい パソコン___ ほしいです。'", "を", "が", "に", "で", "B", "Tính từ ほしい (muốn có) luôn đi với trợ từ が."),
            new("multiple_choice", "Dạng phủ định đúng của 'たべたいです (muốn ăn)' là gì?", "たべたいではありません", "たべたくないです", "たべないたいです", "たべませんたいです", "B", "Đuôi 〜たい biến đổi giống tính từ đuôi -i → 〜たくないです."),
            new("cloze", "Điền gốc động từ mục đích vào chỗ trống: 'デパートへ くつを {{c1::かい}}に いきます。'", null, null, null, null, "かい", "Mục đích di chuyển: Động từ bỏ ます (かいます → かい) + に + いきます."),
            new("multiple_choice", "Khi về trước đồng nghiệp lúc tan làm ở công ty Nhật, bạn nói câu nào?", "おさきに しつれいします。", "はじめまして。", "いただきます。", "いらっしゃいませ。", "A", "おさきに しつれいします = Tôi xin phép về trước ạ."),
            new("multiple_choice", "Câu nào dùng để đặt phòng khách sạn?", "ホテルを よやくしたいです。", "ホテルを とっては いけません。", "ホテルが すんでいます。", "ホテルは おいしかったです。", "A", "よやくします (đặt trước) → よやくしたいです (muốn đặt phòng).")
        }),

        // Chapter 4 Exam (Japanese) - 10 questions attached to 4.3
        ("ja:ch4", JapaneseLesson43Id, "[Đề Chương 4] Tiếng Nhật N5+: Tính Từ, Gia Đình, Số Đếm & Tồn Tại", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Dạng quá khứ phủ định của tính từ 'あつい (nóng)' là gì?", "あつくなかったです", "あついではありませんでした", "あつかったです", "あつくないでした", "A", "Tính từ đuôi -i: bỏ い + くなかったです."),
            new("multiple_choice", "Tính từ nào sau đây CẦN thêm 'な' khi đứng trước danh từ 'まち (thành phố)'?", "おおきい", "ふるい", "しずか", "ちいさい", "C", "しずか (yên tĩnh) là tính từ đuôi -na → しずかな まち."),
            new("multiple_choice", "Câu nào chia đúng quá khứ của 'いい (tốt)'?", "きのうの てんきは いかったです。", "きのうの てんきは よかったです。", "きのうの てんきは いいでした。", "きのうの てんきは よくでした。", "B", "いい chuyển gốc thành よ trước khi chia → よかったです."),
            new("cloze", "Hoàn thiện câu nối tính từ đuôi -na: 'この こうえんは きれい{{c1::で}} しずかです。'", null, null, null, null, "で", "Tính từ đuôi -na nối với tính từ khác bằng で."),
            new("multiple_choice", "Gọi 'mẹ của bạn' một cách lịch sự trong tiếng Nhật là gì?", "はは", "おかあさん", "おとうさん", "おばあさん", "B", "Mẹ của người khác gọi là おかあさん; mẹ mình khi nói với người ngoài là はは."),
            new("multiple_choice", "Đếm '1 người' và '2 người' trong tiếng Nhật đọc là gì?", "いちにん・ににん", "ひとり・ふたり", "ひとつ・ふたつ", "いっぽん・にほん", "B", "1 người = ひとり, 2 người = ふたり."),
            new("multiple_choice", "Chọn lượng từ đúng cho '2 tấm vé' (きっぷ):", "にさつ", "にまい", "にほん", "にだい", "B", "Vật mỏng phẳng như vé, giấy, ảnh dùng 〜まい."),
            new("cloze", "Điền liên từ 'vì... nên': 'じかんが ありません{{c1::から}}、タクシーで いきます。'", null, null, null, null, "から", "Nguyên nhân + から, Kết quả."),
            new("multiple_choice", "Chọn câu đúng cho 'Trên bàn có sách, vở, v.v...':", "つくえの うえに ほんや ノートなどが あります。", "つくえの うえで ほんや ノートなどが あります。", "つくえの うえに ほんや ノートなどが います。", "つくえの うえを ほんと ノートが あります。", "A", "Tồn tại dùng に và あります (sách vở là đồ vật); liệt kê không hết dùng や〜など."),
            new("multiple_choice", "Dịch 'Bưu điện nằm ở giữa ngân hàng và nhà ga':", "ゆうびんきょくは ぎんこうと えきの あいだに あります。", "ゆうびんきょくは ぎんこうと えき あいだに あります。", "ゆうびんきょくは ぎんこうの となりに あります。", "ゆうびんきょくは ぎんこうと えきの うえに あります。", "A", "A と B の あいだに あります.")
        }),

        // Chapter 5 Exam (Japanese) - 12 questions attached to 5.3
        ("ja:ch5", JapaneseLesson53Id, "[Đề Chương 5] Tiếng Nhật N5+: Động Từ Thể Te, Xin Phép, Cấm Đoán & Ý Muốn", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Thể Te của 3 động từ 'かいます・のみます・かきます' lần lượt là gì?", "かって・のんで・かいて", "かいて・のんで・かって", "かって・のいて・かいで", "かいって・のみて・かきって", "A", "い→って, み→んで, き→いて."),
            new("multiple_choice", "Ngoại lệ Thể Te của 'いきます (đi)' là gì?", "いきて", "いって", "いいて", "いで", "B", "いきます → いって."),
            new("multiple_choice", "Câu 'いま あめが ふっています' diễn đạt điều gì?", "Ngày mai trời sẽ mưa", "Bây giờ trời đang mưa", "Hôm qua trời đã mưa", "Trời không mưa nữa", "B", "Vています = hành động đang diễn ra."),
            new("cloze", "Chia động từ 'しります' ở dạng phủ định cho 'Tôi không biết': 'いいえ、{{c1::しりません}}。'", null, null, null, null, "しりません", "Phủ định của しっています là しりません."),
            new("multiple_choice", "Mẫu câu 'ここで しゃしんを とっても いいですか' dùng để làm gì?", "Ra lệnh chụp ảnh", "Xin phép chụp ảnh ở đây", "Cấm chụp ảnh ở đây", "Rủ rê cùng chụp ảnh", "B", "Vても いいですか = xin phép làm gì có được không."),
            new("multiple_choice", "Biển báo bảo tàng ghi 'Không được chạm tay vào tranh'. Câu đúng là gì?", "えに さわっても いいです。", "えに さわっては いけません。", "えに さわって ください。", "えに さわりました。", "B", "Vては いけません = không được phép làm gì."),
            new("multiple_choice", "Thể Te đúng của động từ Nhóm I 'はいります (vào)' là gì?", "はいりて", "はいって", "はいして", "はいで", "B", "はいります thuộc Nhóm I (đuôi り) → はいって."),
            new("cloze", "Hoàn thiện câu 'Sau khi rửa tay xong tôi ăn cơm': 'てを {{c1::あらってから}}、ごはんを たべます。'", null, null, null, null, "あらってから", "V1てから、V2 = Sau khi làm V1 thì làm V2."),
            new("multiple_choice", "Khác biệt giữa 'A が ほしいです' và 'Vたいです' là gì?", "ほしい dùng cho danh từ (muốn có vật); 〜たい dùng cho động từ (muốn làm)", "Cả hai giống hệt nhau", "ほしい dùng cho quá khứ", "〜たい đi với trợ từ で", "A", "Nがほしいです vs V(bỏ ます)+たいです."),
            new("multiple_choice", "Dạng quá khứ của 'いきたいです (muốn đi)' là gì?", "いきたかったです", "いきたいでした", "いきたくないです", "いって ほしかったです", "A", "Đuôi 〜たい chia quá khứ như tính từ đuôi -i → 〜たかったです."),
            new("multiple_choice", "Câu 'こうえんへ さくらを みに いきます' có nghĩa là gì?", "Đi ngắm hoa anh đào ở công viên", "Đã ngắm hoa anh đào rồi mới ra công viên", "Không muốn ra công viên", "Đang trồng hoa anh đào", "A", "[Địa điểm] へ [V bỏ ます] に いきます = đi đâu để làm gì."),
            new("multiple_choice", "Khi đồng nghiệp ở lại làm việc còn bạn ra về trước, bạn nói câu nào?", "おさきに しつれいします。", "ただいま。", "おかえりなさい。", "ごちそうさまでした。", "A", "Câu chào tan làm lịch sự chuẩn công sở Nhật: おさきに しつれいします.")
        })
    };

    private static List<(string Key, Guid LessonId, string Title, QuizQuestionSpec[] Questions)> GetChineseQuizSpecs() => new()
    {
        ("zh:l41", ChineseLesson41Id, "Quiz · Bài 4.1: Thời Tiết, Bốn Mùa & Cấu Trúc Nhấn Mạnh Thái/Rất (太...了 / 很 / 非常)", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Để hỏi 'Thời tiết hôm nay thế nào?', từ nghi vấn đúng đặt cuối câu là gì?", "什么", "怎么样", "哪儿", "多少", "B", "Chủ đề + 怎么样？ dùng để hỏi tình trạng/nhận xét."),
            new("multiple_choice", "Phủ định hợp lý của câu cảm thán '今天太热了！' (Hôm nay nóng quá!) là gì?", "今天不太热了。", "今天不太热。", "今天没有太热了。", "今天不热太。", "B", "Phủ định của 太...了 là 不太 + Tính từ (bỏ 了 ở cuối)."),
            new("cloze", "Điền từ 'hơi' đứng trước tính từ: '今天风很大，外面{{c1::有点儿}}冷。'", null, null, null, null, "有点儿", "有点儿 đứng trước Tính từ (hơi...); 一点儿 đứng trước Danh từ."),
            new("multiple_choice", "Câu nào diễn đạt ĐÚNG 'Hôm nay trời đổ mưa to rồi'?", "今天下雨很大。", "今天下大雨了。", "今天大雨下了。", "今天是下雨大。", "B", "下雨 là động từ ly hợp, tính từ 大 phải chèn vào giữa 下 và 雨."),
            new("multiple_choice", "Từ nào miêu tả đặc trưng thời tiết 'mát mẻ' của mùa thu?", "热", "冷", "凉快", "下雪", "C", "凉快 (liángkuai) = mát mẻ.")
        }),

        ("zh:l42", ChineseLesson42Id, "Quiz · Bài 4.2: Gia Đình, Lượng Từ Thực Chiến (口, 个, 本, 张) & Sở Hữu Với 的", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Trước các lượng từ như 本, 张, 口, 个, số '2' bắt buộc dùng chữ nào?", "二 (èr)", "两 (liǎng)", "双 (shuāng)", "第 (dì)", "B", "Trước mọi lượng từ bắt buộc dùng 两 (两个人, 两本书, 两张票)."),
            new("multiple_choice", "Lượng từ nào dùng chuẩn cho 'ảnh (照片)', 'vé (票)' và 'bàn (桌子)'?", "本", "口", "张", "只", "C", "张 (zhāng) dùng cho vật có mặt phẳng rộng hoặc mỏng."),
            new("cloze", "Điền lượng từ cho động vật nhỏ (mèo, chó): '我家有两{{c1::只}}小猫。'", null, null, null, null, "只", "Động vật nhỏ như mèo, chó dùng lượng từ 只 (zhī)."),
            new("multiple_choice", "Câu nào sử dụng liên từ '和' ĐÚNG chuẩn ngữ pháp tiếng Trung?", "我吃饭和看电视。", "爸爸和妈妈都在家。", "他很忙和很累。", "我去北京和上海了玩。", "B", "和 chỉ nối danh từ/đại từ, tuyệt đối không nối hai động từ hay mệnh đề."),
            new("multiple_choice", "Trong cụm 'Đây là máy tính của anh trai tôi', câu nào tự nhiên và đúng ngữ pháp nhất?", "这是我哥哥的电脑。", "这是的哥哥电脑。", "这是我哥哥电脑。", "这电脑是我哥哥。", "A", "Có thể lược 的 giữa 我 và 哥哥, nhưng giữ 的 trước danh từ đồ vật 电脑.")
        }),

        ("zh:l43", ChineseLesson43Id, "Quiz · Bài 4.3: Nghề Nghiệp, Nơi Làm Việc & Mẫu Câu Đang Làm Gì (在 + Động Từ)", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Câu nào đúng trật tự ngữ pháp tiếng Trung cho 'Tôi làm việc tại ngân hàng'?", "我工作在银行。", "我在银行工作。", "我在工作银行。", "银行我在工作。", "B", "Trạng ngữ địa điểm 在 + Nơi chốn luôn đứng TRƯỚC động từ."),
            new("multiple_choice", "Khi muốn nói 'Anh ấy đang họp trong văn phòng', cách viết nào ĐÚNG (không lặp chữ 在)?", "他在在办公室开会呢。", "他正在办公室开会呢。", "他在开会办公室呢。", "他办公室在开会。", "B", "Dùng 正在 ở đầu để biểu thị tiếp diễn, và 在 cho nơi chốn."),
            new("cloze", "Điền từ phủ định hành động đang diễn ra: '他{{c1::没在}}睡觉，他在写作业呢。'", null, null, null, null, "没在", "Phủ định của 正在/在 + V dùng 没(有)在 + V, không dùng 不在."),
            new("multiple_choice", "Cách hỏi nghề nghiệp lịch sự 'Bạn làm công việc gì thế?' là gì?", "你是做什么工作的？", "你工作是什么？", "你是工作什么？", "你在做谁工作？", "A", "你是做什么工作的？ / 你做什么工作？"),
            new("multiple_choice", "Từ nào chỉ nghề 'Kỹ sư' trong tiếng Trung?", "医生", "工程师", "服务员", "护士", "B", "工程师 (gōngchéngshī) = kỹ sư.")
        }),

        ("zh:l51", ChineseLesson51Id, "Quiz · Bài 5.1: Khả Năng & Ý Muốn (会, 能, 可以, 想) Trong Giao Tiếp Hằng Ngày", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Để diễn đạt kỹ năng học mới biết làm như 'nói tiếng Trung, lái xe, bơi', dùng trợ động từ nào?", "会 (huì)", "能 (néng)", "可以 (kěyǐ)", "想 (xiǎng)", "A", "Kỹ năng phải qua học tập rèn luyện mới biết dùng 会."),
            new("multiple_choice", "Câu nào dùng để xin phép lịch sự 'Tôi ngồi ở đây có được không?'", "我会坐这儿吗？", "我可以坐这儿吗？", "我想坐这儿吗？", "我要坐这儿吗？", "B", "Xin phép dùng 可以...吗？ (hoặc 能...吗？)."),
            new("cloze", "Điền từ chỉ 'điều kiện khách quan không cho phép': '今天我脚疼，{{c1::不能}}去踢足球了。'", null, null, null, null, "不能", "Điều kiện thể chất/khách quan không làm được dùng 不能."),
            new("multiple_choice", "Trả lời ĐỒNG Ý tự nhiên nhất cho câu hỏi '请问，我可以在这里拍照吗？' là gì?", "是，你可以。", "可以，请拍吧！", "会，你可以。", "不客气。", "B", "Lặp lại trợ động từ 可以, không trả lời bằng 是."),
            new("multiple_choice", "Khác biệt giữa '我不想去' và '不要喝太多咖啡' là gì?", "不想 = bản thân không muốn; 不要 = đừng / khuyên ngăn / không lấy", "Cả hai đều sai", "不要 dùng cho quá khứ", "不想 dùng để ra lệnh", "A", "不想 diễn đạt ý định cá nhân; 不要 mang nghĩa khuyên đừng làm hoặc từ chối đồ vật.")
        }),

        ("zh:l52", ChineseLesson52Id, "Quiz · Bài 5.2: Câu Chữ 是...的 (Nhấn Mạnh Thời Gian/Địa Điểm/Phương Thức Quá Khứ) & Hoàn Thành 了", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Câu nào phủ định ĐÚNG hành động quá khứ 'Hôm qua tôi không mua sách'?", "昨天我没买了书。", "昨天我没买书。", "昨天我不买书了。", "昨天我没有买了书。", "B", "Phủ định quá khứ dùng 没(有) + Động từ và BẮT BUỘC bỏ 了."),
            new("multiple_choice", "Để hỏi nhấn mạnh phương thức đã đến ('Bạn đi bằng gì đến đây thế?'), dùng cấu trúc nào?", "你怎么来了的？", "你是怎么来的？", "你来是怎么的？", "你来得怎么？", "B", "Khung nhấn mạnh quá khứ: Chủ ngữ + 是 + [Từ hỏi] + Động từ + 的."),
            new("cloze", "Điền từ đúng để phủ định khung 是...的: '我{{c1::不是}}坐火车来的，我是坐飞机来的。'", null, null, null, null, "不是", "Khi phủ định khung 是...的, chữ 是 bắt buộc hiện diện thành 不是...的."),
            new("multiple_choice", "Để nói 'Tôi vẫn chưa ăn sáng đâu', câu chuẩn là gì?", "我不吃早饭了。", "我还没吃早饭呢。", "我没吃了早饭。", "我不吃早饭呢。", "B", "Chưa làm việc gì dùng 还没 + V + 呢."),
            new("multiple_choice", "Lỗi sai trong câu '我是去年来北京了的' là gì?", "Thiếu địa điểm", "Dùng chung cả 了 và khung 是...的", "Sai vị trí 去年", "Thiếu lượng từ", "B", "Không đặt 了 bên trong khung nhấn mạnh 是...的.")
        }),

        ("zh:l53", ChineseLesson53Id, "Quiz · Bài 5.3: Đi Bệnh Viện, Đặt Vé, Phương Tiện Giao Thông & Đời Sống Tại Trung Quốc", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Cụm từ chuẩn tiếng Trung cho 'uống thuốc' là gì?", "喝药", "吃药", "买药", "看药", "B", "Thuốc dùng động từ 吃 → 吃药 (chīyào)."),
            new("multiple_choice", "Điền giới từ đúng cho khoảng cách tĩnh: '学校___ 地铁站很近。'", "从", "离", "到", "在", "B", "A 离 B + 很近 / 很远."),
            new("cloze", "Điền đơn vị đo khoảng thời lượng đúng: '从北京坐高铁到上海要五个{{c1::小时}}。'", null, null, null, null, "小时", "Khoảng thời gian (mất mấy tiếng) dùng 个小时, không dùng 点."),
            new("multiple_choice", "Câu khuyên nhủ nào đúng trật tự tiếng Trung?", "医生说要喝水多。", "医生说要多喝水。", "医生说喝多水要。", "医生说多水喝。", "B", "Phó từ 多/少 đặt TRƯỚC động từ: 多喝水, 多休息."),
            new("multiple_choice", "Số thứ tự 'Lần đầu tiên' viết là gì?", "一次", "第一", "第一次", "一个次", "C", "第 + Số từ + Lượng từ → 第一次 (lần đầu tiên).")
        }),

        // Chapter 4 Exam (Chinese) - 10 questions attached to 4.3
        ("zh:ch4", ChineseLesson43Id, "[Đề Chương 4] Tiếng Trung HSK1+: Thời Tiết, Gia Đình, Lượng Từ & Công Việc", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Để hỏi 'Thời tiết ngày mai thế nào?', câu chuẩn là gì?", "明天天气什么？", "明天天气怎么样？", "明天天气怎么？", "明天天气好吗？", "B", "Hỏi tình trạng/nhận xét dùng 怎么样."),
            new("multiple_choice", "Phủ định câu '今天太冷了' (Hôm nay lạnh quá!) là gì?", "今天不太冷。", "今天不太冷了。", "今天没太冷。", "今天很冷不。", "A", "不太 + Tính từ (bỏ 了)."),
            new("multiple_choice", "Chọn câu dùng đúng 有点儿 và 一点儿:", "我一点儿累，想喝有点儿茶。", "我有点儿累，想喝一点儿茶。", "我有点儿累，想喝有点儿茶。", "我一点儿累，想喝一点儿茶。", "B", "有点儿 + Tính từ (累); Động từ + 一点儿 + Danh từ (茶)."),
            new("cloze", "Hoàn chỉnh cụm động từ ly hợp 'mưa to rồi': '今天下午下{{c1::大雨}}了。'", null, null, null, null, "大雨", "Động từ ly hợp 下雨 chèn 大 vào giữa thành 下大雨."),
            new("multiple_choice", "Lượng từ nào đi đúng với '两___ 衣服' và '两___ 照片'?", "件 và 张", "张 và 本", "本 và 只", "口 và 件", "A", "Quần áo dùng 件; ảnh/vé/bàn dùng 张."),
            new("multiple_choice", "Số đếm '2' nào đi đúng trước lượng từ '本' (quyển sách)?", "二本", "两本", "双本", "二个本", "B", "Trước lượng từ luôn dùng 两: 两本书."),
            new("multiple_choice", "Câu nào dùng '和' SAI ngữ pháp?", "爸爸和妈妈", "我和你", "我回家和吃饭。", "书和词典", "C", "和 không nối hai động từ/mệnh đề."),
            new("cloze", "Sắp xếp trật tự nơi làm việc: 'Tôi làm việc ở ngân hàng' → 我{{c1::在银行}}工作。", null, null, null, null, "在银行", "Trạng ngữ địa điểm 在 + Nơi chốn đứng TRƯỚC động từ."),
            new("multiple_choice", "Câu nào diễn đạt đúng 'Giám đốc Lý đang họp trong phòng họp đấy'?", "李经理在在会议室开会。", "李经理正在会议室开会呢。", "李经理开会正在会议室。", "李经理没在开会呢。", "B", "正在 + Địa điểm + Động từ + 呢."),
            new("multiple_choice", "Phủ định của '我在看电视' (Tôi đang xem tivi) là gì?", "我不在看电视。", "我没在看电视。", "我不看电视呢。", "我看没电视。", "B", "Phủ định tiếp diễn dùng 没(有)在 + Động từ.")
        }),

        // Chapter 5 Exam (Chinese) - 12 questions attached to 5.3
        ("zh:ch5", ChineseLesson53Id, "[Đề Chương 5] Tiếng Trung HSK1+: Năng Nguyện, Câu 是...的 & Đời Sống Thực Chiến", new QuizQuestionSpec[]
        {
            new("multiple_choice", "Điền trợ động từ phù hợp cho kỹ năng đã học: '我___ 说汉语，但是不会写汉字。'", "会", "能", "可以", "想", "A", "Kỹ năng học tập dùng 会."),
            new("multiple_choice", "Để hỏi xin phép 'Ở đây có thể thanh toán bằng điện thoại không?', dùng từ nào?", "会", "可以", "想", "要", "B", "Xin phép / hỏi quy định cho phép dùng 可以."),
            new("multiple_choice", "Vì bị đau chân nên hôm nay không thể đi chạy bộ, dùng từ phủ định nào?", "不会", "不能", "不想", "不要", "B", "Điều kiện thể chất/khách quan không cho phép dùng 不能."),
            new("cloze", "Hoàn thiện câu phức: '{{c1::因为}}今天下雨，所以我不能去公园。'", null, null, null, null, "因为", "Cặp liên từ nguyên nhân — kết quả: 因为... 所以...."),
            new("multiple_choice", "Trả lời đồng ý chuẩn mực cho '我可以坐这儿吗？' là gì?", "是，你可以。", "可以，请坐！", "会，请坐。", "不客气。", "B", "Lặp lại trợ động từ 可以."),
            new("multiple_choice", "Phủ định đúng cho '昨天我买了一件衣服' là gì?", "昨天我没买了衣服。", "昨天我没买衣服。", "昨天我不买衣服。", "昨天我没有买了衣服。", "B", "Có 没 phủ định quá khứ thì bỏ 了."),
            new("multiple_choice", "Câu nào dùng đúng cấu trúc nhấn mạnh quá khứ '是...的'?", "我是昨天坐飞机来了的。", "我是昨天坐飞机来的。", "我昨天坐飞机了的。", "我坐飞机不来的。", "B", "Khung 是...的 không đi kèm 了."),
            new("cloze", "Phủ định khung 是...的: '我{{c1::不是}}一个人来的，我是和朋友一起来的。'", null, null, null, null, "不是", "Phủ định điểm nhấn 是...的 bắt buộc dùng 不是...的."),
            new("multiple_choice", "Để nói 'Tôi vẫn chưa ăn tối đâu', chọn câu đúng:", "我不吃晚饭了。", "我还没吃晚饭呢。", "我没吃了晚饭。", "我吃没晚饭。", "B", "还没 + Động từ + 呢 = vẫn chưa..."),
            new("multiple_choice", "Trong bệnh viện, bác sĩ dặn 'uống thuốc đúng giờ' dùng cụm từ nào?", "按时喝药", "按时吃药", "多喝药", "吃药多", "B", "Uống thuốc = 吃药."),
            new("multiple_choice", "Chọn câu đúng phân biệt khoảng cách (离) và thời lượng (个小时):", "我家从公司很近，坐车两点。", "我家离公司很近，坐车要两个小时。", "我家离公司很近，坐车要两点。", "我家到公司很近，要二小时。", "B", "Khoảng cách tĩnh dùng 离; khoảng thời gian 2 tiếng dùng 两个小时."),
            new("multiple_choice", "Lời khuyên 'Phải uống nhiều nước ấm' viết chuẩn là gì?", "要喝热水多。", "要多喝热水。", "要喝多热水。", "多要热水喝。", "B", "Phó từ 多 đặt trước động từ → 多喝热水.")
        })
    };

    // =========================================================================
    // FLASHCARDS FSRS v5 (+60 JA + 60 ZH = 120 THẺ MỚI CHO CHƯƠNG 4 & 5)
    // =========================================================================

    private static async Task EnsureExpandedFlashcardsAsync(
        SwmsDbContext db,
        bool hasJapaneseCourse,
        bool hasChineseCourse,
        DateTimeOffset now)
    {
        if (hasJapaneseCourse)
        {
            var jaTag = await EnsureTagAsync(db, "N5-Japanese", "rose", now);
            await SeedCardsForLessonsAsync(db, "jp-n5-exp", jaTag, GetJapaneseExpandedFlashcards(), now);
        }

        if (hasChineseCourse)
        {
            var zhTag = await EnsureTagAsync(db, "HSK1-Chinese", "amber", now);
            await SeedCardsForLessonsAsync(db, "zh-hsk1-exp", zhTag, GetChineseExpandedFlashcards(), now);
        }
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

    private static async Task SeedCardsForLessonsAsync(
        SwmsDbContext db,
        string domainKey,
        Tag tag,
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

        if (existingLessonIds.Count == 0)
        {
            return;
        }

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
            if (!existingLessonIds.Contains(lessonId)) continue;

            var cardId = SeedDataInitializer.CreateDeterministicGuid($"swms:lang-fc:{domainKey}:{lessonId:N}:{i + 1}");
            var frontJson = JsonSerializer.Serialize(new { text = front }, CjkJsonOptions);
            var backJson = JsonSerializer.Serialize(new { text = back }, CjkJsonOptions);

            if (!existingCardIds.Contains(cardId) && !existingFronts.Contains(frontJson))
            {
                cardsToAdd.Add(new Flashcard
                {
                    Id = cardId,
                    UserId = SeedDataInitializer.DefaultUserId,
                    LessonId = lessonId,
                    CardType = "basic",
                    SourceTitleSnapshot = $"Thẻ từ vựng {tag.Name}",
                    FrontAstJson = frontJson,
                    BackAstJson = backJson,
                    Stability = 0,
                    Difficulty = 0,
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

            if (!existingTagPairs.Contains(cardId) && (existingCardIds.Contains(cardId)))
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

        if (cardsToAdd.Count > 0)
        {
            db.Flashcards.AddRange(cardsToAdd);
        }
        if (tagsToAdd.Count > 0)
        {
            db.FlashcardTags.AddRange(tagsToAdd);
        }
        if (cardsToAdd.Count > 0 || tagsToAdd.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }

    private static (Guid LessonId, string Front, string Back)[] GetJapaneseExpandedFlashcards()
    {
        var l41 = JapaneseLesson41Id;
        var l42 = JapaneseLesson42Id;
        var l43 = JapaneseLesson43Id;
        var l51 = JapaneseLesson51Id;
        var l52 = JapaneseLesson52Id;
        var l53 = JapaneseLesson53Id;

        return new (Guid, string, string)[]
        {
            // Bài 4.1 (10 thẻ)
            (l41, "大きい / 小さい (おおきい / ちいさい - Ookii / Chiisai)", "To lớn / Nhỏ bé (Tính từ đuôi -i).\nVí dụ: この部屋は大きいです。(Căn phòng này rộng lớn.)"),
            (l41, "新しい / 古い (あたらしい / ふるい - Atarashii / Furui)", "Mới / Cũ (chỉ dùng cho đồ vật).\nVí dụ: 新しいスマホを買いました。(Tôi đã mua điện thoại mới.)"),
            (l41, "暑い / 寒い (あつい / さむい - Atsui / Samui)", "Nóng / Lạnh (chỉ thời tiết, không khí).\nVí dụ: 冬の日本はとても寒いです。(Mùa đông ở Nhật rất lạnh.)"),
            (l41, "暖かい / 涼しい (あたたかい / すずしい - Atatakai / Suzushii)", "Ấm áp / Mát mẻ.\nVí dụ: 春は暖かくて、秋は涼しいです。(Mùa xuân ấm áp, mùa thu mát mẻ.)"),
            (l41, "綺麗 (きれい - Kirei [na])", "Đẹp, sạch sẽ (NGOẠI LỆ: Tính từ đuôi -na dù kết thúc bằng い!).\nVí dụ: 桜はとても綺麗ですね。(Hoa anh đào đẹp quá nhỉ.)"),
            (l41, "静か / 賑やか (しずか / にぎやか - Shizuka / Nigiyaka [na])", "Yên tĩnh / Náo nhiệt, đông vui (Tính từ đuôi -na).\nVí dụ: 新宿は賑やかな街です。(Shinjuku là khu phố náo nhiệt.)"),
            (l41, "便利 (べんり - Benri [na])", "Tiện lợi, thuận tiện (Tính từ đuôi -na).\nVí dụ: 電車はとても便利です。(Tàu điện rất tiện lợi.)"),
            (l41, "いい → よくない / よかった (Ii → Yokunai / Yokatta)", "Tốt → Không tốt / Đã tốt (ngoại lệ biến về gốc よ).\nVí dụ: 昨日の天気は良かったです。(Thời tiết hôm qua đã rất đẹp.)"),
            (l41, "あまり〜ない / ぜんぜん〜ない (Amari / Zenzen ~nai)", "Không ... lắm / Hoàn toàn không (LUÔN đi với thể phủ định).\nVí dụ: テストはあまり難しくなかったです。(Bài thi không khó lắm.)"),
            (l41, "難しい / 易しい (むずかしい / やさしい - Muzukashii / Yasashii)", "Khó / Dễ (hoặc Hiền lành).\nVí dụ: 漢字は少し難しいですが、先生は優しいです。(Chữ Hán hơi khó nhưng thầy rất hiền.)"),

            // Bài 4.2 (10 thẻ)
            (l42, "家族 (かぞく - Kazoku) & ご家族", "Gia đình (của tôi) / Gia đình (của bạn - kính ngữ).\nVí dụ: ご家族は何人ですか。(Gia đình bạn có mấy người vậy?)"),
            (l42, "父 / お父さん (ちち / おとうさん - Chichi / Otousan)", "Bố tôi (khiêm nhường) / Bố bạn (kính trọng).\nVí dụ: 私の父は会社員です。(Bố tôi là nhân viên công ty.)"),
            (l42, "母 / お母さん (はは / おかあさん - Haha / Okaasan)", "Mẹ tôi (khiêm nhường) / Mẹ bạn (kính trọng).\nVí dụ: お母さんはお元気ですか。(Mẹ bạn có khỏe không ạ?)"),
            (l42, "兄 / 姉 / 弟 / 妹 (Ani / Ane / Otouto / Imouto)", "Anh trai / Chị gái / Em trai / Em gái của tôi.\nVí dụ: 妹は高校生です。(Em gái tôi là học sinh cấp ba.)"),
            (l42, "両親 / 兄弟 (りょうしん / きょうだい - Ryoushin / Kyoudai)", "Bố mẹ (cả hai) / Anh chị em.\nVí dụ: 兄弟は三人います。(Tôi có ba anh chị em.)"),
            (l42, "一人 / 二人 / 四人 (ひとり / ふたり / よにん)", "1 người / 2 người / 4 người (các số đếm người đặc biệt).\nVí dụ: 私の家族は四人です。(Gia đình tôi có 4 người.)"),
            (l42, "〜枚 (まい - Mai)", "Lượng từ đếm vật mỏng phẳng (vé, giấy, ảnh, áo).\nVí dụ: 切符を二枚買いました。(Tôi đã mua 2 tấm vé.)"),
            (l42, "〜本 (いっぽん / にほん / さんぼん)", "Lượng từ đếm vật thon dài (bút, ô dù, chai nước) — chú ý biến âm!\nVí dụ: 赤いペンを三本ください。(Cho tôi 3 cây bút đỏ.)"),
            (l42, "〜台 (だい - Dai)", "Lượng từ đếm máy móc, xe cộ (PC, ô tô, xe đạp).\nVí dụ: 家に車が二台あります。(Nhà tôi có 2 chiếc ô tô.)"),
            (l42, "〜から (~kara - Nguyên nhân)", "Vì ... nên ... (đặt sau mệnh đề nguyên nhân).\nVí dụ: 時間がありませんから、急ぎます。(Vì không có thời gian nên tôi vội đi.)"),

            // Bài 4.3 (10 thẻ)
            (l43, "上・下・前・後ろ (うえ・した・まえ・うしろ)", "Trên / Dưới / Trước / Sau.\nVí dụ: 机の上に本があります。(Trên bàn có quyển sách.)"),
            (l43, "右・左・中・外 (みぎ・ひだり・なか・そと)", "Bên phải / Bên trái / Bên trong / Bên ngoài.\nVí dụ: 箱の中に何もありません。(Trong hộp không có gì cả.)"),
            (l43, "隣・近く (となり・ちかく - Tonari / Chikaku)", "Ngay bên cạnh / Ở gần khu vực.\nVí dụ: 銀行の隣に郵便局があります。(Bên cạnh ngân hàng có bưu điện.)"),
            (l43, "〜と〜の間 (あいだ - ~to ~no aida)", "Ở giữa A và B.\nVí dụ: 本屋は駅と銀行の間にあります。(Hiệu sách nằm giữa ga và ngân hàng.)"),
            (l43, "〜や〜など (~ya ~nado)", "... và ..., vân vân (liệt kê tiêu biểu không đầy đủ).\nVí dụ: 鞄の中に財布や鍵などがあります。(Trong cặp có ví, chìa khóa, v.v...)"),
            (l43, "交番 (こうばん - Kouban)", "Đồn cảnh sát khu phố (nơi hỏi đường/báo mất đồ ở Nhật).\nVí dụ: 駅の前に交番があります。(Trước nhà ga có đồn cảnh sát.)"),
            (l43, "病院 / 本屋 (びょういん / ほんや - Byouin / Honya)", "Bệnh viện / Hiệu sách.\nVí dụ: 病院は公園の近くにあります。(Bệnh viện ở gần công viên.)"),
            (l43, "デパート / 公園 (Depaato / こうえん - Kouen)", "Trung tâm thương mại / Công viên.\nVí dụ: 公園に子どもがいます。(Trong công viên có trẻ em.)"),
            (l43, "何も〜ません / 誰も〜ません (Nanimo / Daremo ~masen)", "Không có cái gì cả / Không có ai cả.\nVí dụ: 教室に誰もいません。(Trong lớp học không có ai cả.)"),
            (l43, "エレベーター / 階段 (Erebeetaa / かいだん - Kaidan)", "Thang máy / Cầu thang bộ.\nVí dụ: エレベーターは右にあります。(Thang máy nằm ở bên phải.)"),

            // Bài 5.1 (10 thẻ)
            (l51, "〜てください (~te kudasai)", "Xin hãy làm ... (nhờ vả, yêu cầu lịch sự).\nVí dụ: ここに名前を書いてください。(Xin hãy viết tên vào đây.)"),
            (l51, "〜ています (~te imasu)", "Đang làm ... / Trạng thái kéo dài (sống ở, làm việc ở).\nVí dụ: 今雨が降っています。(Bây giờ trời đang mưa.)"),
            (l51, "行って (いって - Itte)", "Thể Te NGOẠI LỆ của 行きます (đi).\nVí dụ: まっすぐ行って、左に曲がってください。(Đi thẳng rồi rẽ trái nhé.)"),
            (l51, "開けます / 閉めます (あけます / しめます - Akete / Shimete)", "Mở / Đóng (cửa sổ, cửa ra vào — Nhóm II).\nVí dụ: 暑いから、窓を開けてください。(Nóng quá, hãy mở cửa sổ ra nhé.)"),
            (l51, "つけます / 消します (Tsukete / けして - Keshite)", "Bật / Tắt (đèn, điều hòa).\nVí dụ: 部屋の電気をつけてください。(Làm ơn bật đèn phòng lên.)"),
            (l51, "待ちます / 急ぎます (まって / いそいで - Matte / Isoide)", "Chờ đợi / Vội vã.\nVí dụ: すみません、ちょっと待ってください。(Xin lỗi, vui lòng đợi một chút.)"),
            (l51, "手伝います / 持ちます (てつだって / もって)", "Giúp đỡ / Cầm mang xách.\nVí dụ: 重そうですね。荷物を持ちましょうか。(Trông nặng quá. Để tôi xách phụ nhé?)"),
            (l51, "住みます (すんでいます - Sunde imasu)", "Sinh sống, cư trú (luôn dùng Thể 〜ています).\nVí dụ: 私はハノイに住んでいます。(Tôi đang sống ở Hà Nội.)"),
            (l51, "知っています / 知りません (しっています / しりません)", "Biết / Không biết (LƯU Ý: phủ định là しりません!).\nVí dụ: 田中さんを知っていますか。— いいえ、知りません。"),
            (l51, "降ります / 呼びます (ふって / よんで - Futte / Yonde)", "Rơi (mưa, tuyết) / Gọi (taxi, tên).\nVí dụ: タクシーを呼びましょうか。(Tôi gọi taxi giúp bạn nhé?)"),

            // Bài 5.2 (10 thẻ)
            (l52, "〜てもいいですか (~te mo ii desu ka)", "Tôi làm ... có được không ạ? (xin phép lịch sự).\nVí dụ: ここで写真を撮ってもいいですか。(Tôi chụp ảnh ở đây được không?)"),
            (l52, "〜てはいけません (~te wa ikemasen)", "Không được phép làm ... (cấm đoán, nội quy).\nVí dụ: 美術館で写真を撮ってはいけません。(Trong bảo tàng không được chụp ảnh.)"),
            (l52, "V1てから、V2 (~te kara)", "Sau khi làm V1 xong thì làm V2 (nhấn mạnh thứ tự).\nVí dụ: 手を洗ってから、ごはんを食べます。(Rửa tay xong rồi mới ăn cơm.)"),
            (l52, "入ります → 入って (はいって - Haitte)", "Đi vào (Nhóm I giả Nhóm II — Thể Te là はいって!).\nVí dụ: 部屋に入ってもいいですか。(Tôi vào phòng được không?)"),
            (l52, "吸います → 吸って (すって - Sutte)", "Hút (thuốc lá).\nVí dụ: ここでたばこを吸ってはいけません。(Không được hút thuốc ở đây.)"),
            (l52, "止めます / 置きます (とめて / おいて - Tomete / Oite)", "Dừng đỗ xe / Đặt xuống.\nVí dụ: ここに車を止めてはいけません。(Không được đỗ xe ở đây.)"),
            (l52, "触ります (さわって - Sawatte)", "Chạm vào, sờ vào (đi với trợ từ に).\nVí dụ: 絵に触ってはいけません。(Không được chạm tay vào tranh.)"),
            (l52, "洗います / 浴びます (あらって / あびて - Aratte / Abite)", "Rửa (tay, mặt) / Tắm (vòi sen).\nVí dụ: シャワーを浴びてから、寝ます。(Tắm vòi sen xong rồi ngủ.)"),
            (l52, "出します / 返します (だして / かえして - Dashite / Kaeshite)", "Nộp (báo cáo) / Trả lại (sách mượn).\nVí dụ: レポートを出してから、帰ります。(Nộp báo cáo xong tôi mới về.)"),
            (l52, "禁煙 (きんえん - Kin'en) & すみません、ちょっと", "Cấm hút thuốc & Cụm từ chối mềm mỏng lịch sự.\nVí dụ: すみません、ここは禁煙ですから、ちょっと…。"),

            // Bài 5.3 (10 thẻ)
            (l53, "〜が欲しいです (ほしいです - ~ga hoshii desu)", "Muốn có (danh từ/đồ vật — dùng trợ từ が!).\nVí dụ: 新しいパソコンが欲しいです。(Tôi muốn có một chiếc laptop mới.)"),
            (l53, "Vます(bỏ ます) + たいです (~tai desu)", "Muốn làm (hành động — chia phủ định là 〜たくないです).\nVí dụ: 日本へ旅行に行きたいです。(Tôi muốn đi du lịch Nhật Bản.)"),
            (l53, "[Địa điểm] へ [V bỏ ます] に 行きます", "Đi đến đâu để làm gì (mục đích chuyến đi).\nVí dụ: デパートへ靴を買いに行きます。(Đi trung tâm thương mại mua giày.)"),
            (l53, "予約します (よやくします - Yoyaku shimasu)", "Đặt trước (vé máy bay, phòng khách sạn, bàn ăn).\nVí dụ: 京都のホテルを予約したいです。(Tôi muốn đặt phòng khách sạn ở Kyoto.)"),
            (l53, "お土産 (おみやげ - Omiyage)", "Quà lưu niệm (văn hóa mua quà khi đi du lịch về).\nVí dụ: 家族にお土産を買いたいです。(Tôi muốn mua quà cho gia đình.)"),
            (l53, "温泉 / 景色 (おんせん / けしき - Onsen / Keshiki)", "Suối nước nóng / Phong cảnh.\nVí dụ: 温泉に入りたいですね。(Tôi muốn tắm suối nước nóng quá nhỉ.)"),
            (l53, "将来 / 夢 (しょうらい / ゆめ - Shourai / Yume)", "Tương lai / Ước mơ.\nVí dụ: 将来日本で働きたいです。(Tương lai tôi muốn làm việc ở Nhật.)"),
            (l53, "出張 / 会議 (しゅっちょう / かいぎ - Shucchou / Kaigi)", "Đi công tác / Cuộc họp.\nVí dụ: 来週大阪へ出張します。(Tuần sau tôi đi công tác Osaka.)"),
            (l53, "おなかがすきました / のどがかわきました", "Tôi đói bụng rồi / Tôi khát nước rồi.\nVí dụ: おなかがすきましたから、ラーメンが食べたいです。"),
            (l53, "お先に失礼します (おさきに しつれいします)", "Tôi xin phép về trước ạ (câu chào tan làm bắt buộc nơi công sở).\nVí dụ: お疲れ様でした！お先に失礼します！")
        };
    }

    private static (Guid LessonId, string Front, string Back)[] GetChineseExpandedFlashcards()
    {
        var l41 = ChineseLesson41Id;
        var l42 = ChineseLesson42Id;
        var l43 = ChineseLesson43Id;
        var l51 = ChineseLesson51Id;
        var l52 = ChineseLesson52Id;
        var l53 = ChineseLesson53Id;

        return new (Guid, string, string)[]
        {
            // Bài 4.1 (10 thẻ)
            (l41, "天气 (tiānqì) & 怎么样 (zěnmeyàng)", "Thời tiết & Như thế nào?\nVí dụ: 今天北京的天气怎么样？(Thời tiết Bắc Kinh hôm nay thế nào?)"),
            (l41, "热 / 冷 (rè / lěng)", "Nóng / Lạnh.\nVí dụ: 夏天非常热，冬天非常冷。(Mùa hè rất nóng, mùa đông rất lạnh.)"),
            (l41, "暖和 / 凉快 (nuǎnhuo / liángkuai)", "Ấm áp / Mát mẻ.\nVí dụ: 河内的秋天非常凉快。(Mùa thu Hà Nội rất mát mẻ.)"),
            (l41, "春天・夏天・秋天・冬天", "Mùa xuân / Mùa hè / Mùa thu / Mùa đông.\nVí dụ: 一年有四个季节，我最喜欢秋天。(Một năm có 4 mùa, tôi thích mùa thu nhất.)"),
            (l41, "下雨 / 下大雨 (xiàyǔ / xià dàyǔ)", "Trời mưa / Mưa to (động từ ly hợp — chèn 大 vào giữa!).\nVí dụ: 今天下午下大雨了。(Chiều nay trời mưa to rồi.)"),
            (l41, "下雪 / 刮风 (xiàxuě / guāfēng)", "Tuyết rơi / Có gió thổi.\nVí dụ: 外面刮风了，多穿点儿衣服。(Ngoài trời nổi gió rồi, mặc thêm áo đi.)"),
            (l41, "晴天 / 阴天 (qíngtiān / yīntiān)", "Trời nắng đẹp / Trời âm u nhiều mây.\nVí dụ: 明天是晴天，我们去公园吧！(Mai trời nắng, đi công viên nhé!)"),
            (l41, "太...了 / 不太... (tài... le / bú tài...)", "Quá ... rồi! / Không ... lắm (khi phủ định BỎ 了!).\nVí dụ: 今天太好了！/ 今天不太冷。(Hôm nay tuyệt quá! / Không lạnh lắm.)"),
            (l41, "非常 / 真 (fēicháng / zhēn)", "Vô cùng, rất / Thật là.\nVí dụ: 今天天气真舒服！(Thời tiết hôm nay thật dễ chịu!)"),
            (l41, "有点儿 (yǒudiǎnr) vs 一点儿 (yìdiǎnr)", "Hơi (+ Tính từ) vs Một chút (+ Danh từ).\nVí dụ: 我有点儿渴，想喝一点儿水。(Tôi hơi khát, muốn uống chút nước.)"),

            // Bài 4.2 (10 thẻ)
            (l42, "爷爷 / 奶奶 (yéye / nǎinai)", "Ông nội / Bà nội.\nVí dụ: 爷爷奶奶身体都很好。(Ông bà nội sức khỏe đều rất tốt.)"),
            (l42, "儿子 / 女儿 (érzi / nǚ'ér)", "Con trai / Con gái.\nVí dụ: 王老师有一个儿子和一个女儿。(Cô Vương có 1 con trai và 1 con gái.)"),
            (l42, "张 (zhāng - Lượng từ)", "Tờ, tấm, bức, chiếc (ảnh, vé, giấy, bàn, giường).\nVí dụ: 桌子上有两张电影票。(Trên bàn có 2 tấm vé xem phim.)"),
            (l42, "件 (jiàn - Lượng từ)", "Chiếc, bộ (quần áo), sự việc.\nVí dụ: 这件新衣服非常好看。(Chiếc áo mới này rất đẹp.)"),
            (l42, "只 (zhī - Lượng từ)", "Con (động vật nhỏ như mèo, chó).\nVí dụ: 我家里有两只小猫。(Nhà tôi có 2 chú mèo con.)"),
            (l42, "两 (liǎng) vs 二 (èr)", "Hai (dùng trước lượng từ) vs Số hai (số đếm, thứ tự, 12, 20).\nVí dụ: 我买了两本书，现在是十二点。(Tôi mua 2 quyển sách, giờ là 12 giờ.)"),
            (l42, "照片 / 票 (zhàopiàn / piào)", "Bức ảnh / Tấm vé.\nVí dụ: 这张全家福照片真好看！(Bức ảnh đại gia đình này đẹp thật!)"),
            (l42, "衣服 / 词典 (yīfu / cídiǎn)", "Quần áo / Cuốn từ điển.\nVí dụ: 我有一本汉越词典。(Tôi có một cuốn từ điển Hán-Việt.)"),
            (l42, "几 (jǐ) vs 多少 (duōshao)", "Mấy (ước chừng <=10) vs Bao nhiêu (>10).\nVí dụ: 你们班有多少个学生？(Lớp các bạn có bao nhiêu học sinh?)"),
            (l42, "和 (hé - Và)", "Và (CHỈ nối Danh từ/Đại từ, KHÔNG nối 2 động từ!).\nVí dụ: 爸爸和妈妈都在家。(Bố và mẹ đều ở nhà.)"),

            // Bài 4.3 (10 thẻ)
            (l43, "工作 / 公司 (gōngzuò / gōngsī)", "Làm việc / Công ty (luôn đặt 在 Nơi chốn TRƯỚC 工作).\nVí dụ: 我在一家科技公司工作。(Tôi làm ở một công ty công nghệ.)"),
            (l43, "工程师 (gōngchéngshī)", "Kỹ sư.\nVí dụ: 我哥哥是一位软件工程师。(Anh trai tôi là một kỹ sư phần mềm.)"),
            (l43, "医生 / 护士 (yīshēng / hùshi)", "Bác sĩ / Y tá.\nVí dụ: 我妈妈在医院当医生。(Mẹ tôi làm bác sĩ ở bệnh viện.)"),
            (l43, "经理 / 职员 (jīnglǐ / zhíyuán)", "Giám đốc, quản lý / Nhân viên văn phòng.\nVí dụ: 李经理现在不在办公室。(Giám đốc Lý hiện không ở văn phòng.)"),
            (l43, "服务员 (fúwùyuán)", "Nhân viên phục vụ.\nVí dụ: 服务员，请给我一杯水！(Phục vụ ơi, cho tôi một ly nước!)"),
            (l43, "银行 / 办公室 (yínháng / bàngōngshì)", "Ngân hàng / Văn phòng làm việc.\nVí dụ: 银行在学校对面。(Ngân hàng ở đối diện trường học.)"),
            (l43, "正在...呢 (zhèngzài... ne)", "Đang ... (nhấn mạnh hành động đang diễn ra).\nVí dụ: 经理正在会议室开会呢。(Giám đốc đang họp trong phòng họp đấy.)"),
            (l43, "没在 + Động từ (méi zài...)", "Không có đang ... (phủ định tiếp diễn — KHÔNG dùng 不在!).\nVí dụ: 我没在看电视，我在学习呢。(Tôi không có đang xem tivi, tôi đang học.)"),
            (l43, "打电话 / 开会 (dǎ diànhuà / kāihuì)", "Gọi điện thoại / Họp hành.\nVí dụ: 爸爸正在给爷爷打电话。(Bố đang gọi điện cho ông nội.)"),
            (l43, "写作业 / 睡觉 (xiě zuòyè / shuìjiào)", "Làm bài tập về nhà / Đi ngủ.\nVí dụ: 弟弟还在房间里睡觉呢。(Em trai vẫn đang ngủ trong phòng đấy.)"),

            // Bài 5.1 (10 thẻ)
            (l51, "会 (huì)", "Biết (kỹ năng phải học mới có) / Sẽ (tương lai).\nVí dụ: 我会说汉语，不会开车。(Tôi biết nói tiếng Hán, không biết lái xe.)"),
            (l51, "能 (néng)", "Có thể (năng lực thể chất hoặc điều kiện khách quan).\nVí dụ: 今天太忙了，我不能去。(Nay bận quá, tôi không thể đi được.)"),
            (l51, "可以 (kěyǐ)", "Có thể, được phép (dùng để xin phép / cho phép).\nVí dụ: 请问，我可以坐这儿吗？(Xin hỏi tôi ngồi đây được không?)"),
            (l51, "想 (xiǎng) vs 要 (yào)", "Muốn nhẹ nhàng, nhớ / Muốn dứt khoát, cần.\nVí dụ: 我想去上海旅游。(Tôi muốn đi du lịch Thượng Hải.)"),
            (l51, "不想 (bù xiǎng) vs 不要 (bú yào)", "Không muốn làm / Đừng (khuyên ngăn) hoặc Không lấy.\nVí dụ: 我累了，不想出去。/ 太晚了，不要喝咖啡了！"),
            (l51, "游泳 / 开车 (yóuyǒng / kāichē)", "Bơi lội / Lái xe ô tô.\nVí dụ: 夏天我喜欢去游泳。(Mùa hè tôi thích đi bơi.)"),
            (l51, "做饭 / 帮忙 (zuòfàn / bāngmáng)", "Nấu cơm / Giúp đỡ (帮我一个忙).\nVí dụ: 你能帮我一个忙吗？(Bạn giúp tôi một việc được không?)"),
            (l51, "手机支付 (shǒujī zhīfù)", "Thanh toán qua điện thoại (WeChat Pay / Alipay).\nVí dụ: 这儿可以用手机支付吗？(Ở đây trả qua điện thoại được không?)"),
            (l51, "拍照 / 吸烟 (pāizhào / xīyān)", "Chụp ảnh / Hút thuốc lá.\nVí dụ: 博物馆里面不能拍照。(Bên trong bảo tàng không được chụp ảnh.)"),
            (l51, "因为...所以... (yīnwèi... suǒyǐ...)", "Bởi vì ... cho nên ... (có thể dùng cả cặp câu!).\nVí dụ: 因为下雨，所以我不能去。(Vì trời mưa nên tôi không đi được.)"),

            // Bài 5.2 (10 thẻ)
            (l52, "是...的 (shì... de)", "Nhấn mạnh Thời gian / Địa điểm / Phương thức của việc ĐÃ xảy ra.\nVí dụ: 我是坐飞机来北京的。(Tôi đi máy bay đến Bắc Kinh.)"),
            (l52, "不是...的 (bú shì... de)", "Phủ định cấu trúc nhấn mạnh quá khứ (bắt buộc hiện chữ 是).\nVí dụ: 我不是坐火车来的。(Không phải tôi đi tàu hỏa đến.)"),
            (l52, "了 (le - Hoàn thành & Đổi trạng thái)", "Đã xong (sau động từ) / Đổi trạng thái (cuối câu).\nVí dụ: 我买了两本书。/ 天气冷了。(Tôi đã mua 2 sách. / Trời lạnh rồi.)"),
            (l52, "没 + Động từ (bỏ 了!)", "Phủ định hành động đã qua (TUYỆT ĐỐI không dùng 了).\nVí dụ: 昨天我没去商店。(Hôm qua tôi không đi cửa hàng.)"),
            (l52, "还没...呢 (hái méi... ne)", "Vẫn chưa làm việc gì.\nVí dụ: 我还没吃早饭呢。(Tôi vẫn chưa ăn sáng đâu.)"),
            (l52, "去年 / 上个月 (qùnián / shàng ge yuè)", "Năm ngoái / Tháng trước.\nVí dụ: 我是去年九月来中国的。(Tôi sang Trung Quốc tháng 9 năm ngoái.)"),
            (l52, "飞机 / 火车 (fēijī / huǒchē)", "Máy bay / Tàu hỏa.\nVí dụ: 我是从河内坐飞机来的。(Tôi bay từ Hà Nội sang.)"),
            (l52, "出生 / 毕业 (chūshēng / bìyè)", "Sinh ra / Tốt nghiệp.\nVí dụ: 我是在河内出生的。(Tôi sinh ra ở Hà Nội.)"),
            (l52, "已经 / 刚才 (yǐjīng / gāngcái)", "Đã ... rồi / Vừa nãy.\nVí dụ: 刚才谁给你打电话了？(Vừa nãy ai gọi cho bạn thế?)"),
            (l52, "送 / 网上 (sòng / wǎngshàng)", "Tặng, biếu / Trên mạng.\nVí dụ: 这件衣服是在网上买的。(Áo này mua trên mạng đấy.)"),

            // Bài 5.3 (10 thẻ)
            (l53, "生病 / 感冒 (shēngbìng / gǎnmào)", "Bị ốm / Bị cảm cúm.\nVí dụ: 天气冷了，别感冒了！(Trời lạnh rồi, đừng để bị cảm nhé!)"),
            (l53, "发烧 / 头疼 (fāshāo / tóuténg)", "Bị sốt / Đau đầu.\nVí dụ: 我有点儿发烧，头也很疼。(Tôi hơi sốt, đầu cũng rất đau.)"),
            (l53, "吃药 / 休息 (chīyào / xiūxi)", "Uống thuốc (dùng 吃!) / Nghỉ ngơi.\nVí dụ: 按时吃药，好好休息！(Uống thuốc đúng giờ, nghỉ ngơi cho khỏe!)"),
            (l53, "多喝水 (duō hē shuǐ)", "Uống nhiều nước (đặt 多 TRƯỚC động từ!).\nVí dụ: 感冒的时候要多喝热水。(Lúc cảm phải uống nhiều nước ấm.)"),
            (l53, "离...近 / 远 (lí... jìn / yuǎn)", "Cách ... gần / xa (khoảng cách tĩnh).\nVí dụ: 我家离学校很近。(Nhà tôi cách trường rất gần.)"),
            (l53, "从 A 到 B 要多长时间？", "Từ A đến B mất bao lâu?\nVí dụ: 从北京到上海要五个小时。(Từ Bắc Kinh đến Thượng Hải mất 5 tiếng.)"),
            (l53, "个小时 (ge xiǎoshí) vs 点 (diǎn)", "Tiếng đồng hồ (thời lượng) vs Giờ (thời điểm trên đồng hồ).\nVí dụ: 我每天学习两个小时。(Mỗi ngày tôi học 2 tiếng đồng hồ.)"),
            (l53, "高铁 / 打车 (gāotiě / dǎchē)", "Tàu cao tốc / Gọi taxi (xe công nghệ).\nVí dụ: 中国的高铁又快又舒服！(Tàu cao tốc TQ vừa nhanh vừa êm!)"),
            (l53, "订机票 / 订酒店 (dìng jīpiào / jiǔdiàn)", "Đặt vé máy bay / Đặt phòng khách sạn.\nVí dụ: 我已经在网上订好酒店了。(Tôi đã đặt xong khách sạn trên mạng rồi.)"),
            (l53, "第一次 (dì-yī cì)", "Lần đầu tiên (số thứ tự 第 + số đếm).\nVí dụ: 这是我第一次坐高铁。(Đây là lần đầu tiên tôi đi tàu cao tốc.)")
        };
    }
}
