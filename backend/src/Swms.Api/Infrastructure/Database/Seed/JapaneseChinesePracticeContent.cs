namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;

public static partial class JapaneseChinesePracticeContent
{
    public const string JapaneseCourseDescription = "Khóa học tiếng Nhật nền tảng JLPT N5: làm chủ bảng chữ cái Hiragana và Katakana, rèn trường âm và âm ngắt chuẩn giọng Nhật (ja-JP), cùng các tình huống giao tiếp đời sống qua Shadowing, Dictation, Sơ đồ từ vựng và Nhập vai.";
    public const string ChineseCourseDescription = "Khóa học tiếng Trung giao tiếp chuẩn HSK1: làm chủ hệ thống Pinyin, 4 thanh điệu và quy tắc biến điệu, ghi nhớ chữ Hán nền tảng và thực hành hội thoại đời sống chuẩn giọng phổ thông (zh-CN).";

    public const string UpgradeMarker = "<!-- studxy-cjk-v2 -->";
    public const string UpgradeMarkerV3 = "<!-- studxy-cjk-v3 -->";

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

    public static async Task EnsureSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
        await EnsureCourseShellAsync(
            db,
            courseId: SeedDataInitializer.JapaneseCourseId,
            title: "Tiếng Nhật Sơ Cấp (N5)",
            slug: "tieng-nhat-so-cap-n5",
            description: JapaneseCourseDescription,
            chapters: new[]
            {
                (SeedDataInitializer.JapaneseChapter1Id, "Chương 1: Bảng chữ cái Hiragana/Katakana & Chào hỏi N5", 1),
                (SeedDataInitializer.JapaneseChapter2Id, "Chương 2: Số đếm, Thời gian & Mua sắm", 2),
                (SeedDataInitializer.JapaneseChapter3Id, "Chương 3: Di chuyển & Giao tiếp Đời sống N5", 3)
            },
            lessons: GetJapaneseLessons(),
            activities: GetJapaneseActivities(SeedDataInitializer.JapaneseCourseId, now),
            now: now);

        await EnsureCourseShellAsync(
            db,
            courseId: SeedDataInitializer.ChineseCourseId,
            title: "Tiếng Trung Sơ Cấp (HSK1)",
            slug: "tieng-trung-so-cap-hsk1",
            description: ChineseCourseDescription,
            chapters: new[]
            {
                (SeedDataInitializer.ChineseChapter1Id, "Chương 1: Pinyin, Thanh điệu & Chữ Hán Cơ bản", 1),
                (SeedDataInitializer.ChineseChapter2Id, "Chương 2: Số đếm, Gia đình & Mua sắm HSK1", 2),
                (SeedDataInitializer.ChineseChapter3Id, "Chương 3: Sinh hoạt Hằng ngày & Hỏi đường HSK1", 3)
            },
            lessons: GetChineseLessons(),
            activities: GetChineseActivities(SeedDataInitializer.ChineseCourseId, now),
            now: now);

        await EnsureCjkQuizzesSeededAsync(db, now);
    }

    private static async Task EnsureCourseShellAsync(
        SwmsDbContext db,
        Guid courseId,
        string title,
        string slug,
        string description,
        (Guid Id, string Title, int SortOrder)[] chapters,
        List<CjkLessonMeta> lessons,
        List<PracticeActivity> activities,
        DateTimeOffset now)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId || c.Slug == slug);
        if (course == null)
        {
            course = new Course
            {
                Id = courseId,
                InstructorId = SeedDataInitializer.DefaultUserId,
                Title = title,
                Slug = slug,
                Description = description,
                Visibility = "public",
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Courses.Add(course);
            await db.SaveChangesAsync();
        }

        var targetCourseId = course.Id;

        if (!await db.CourseEnrollments.AnyAsync(e => e.UserId == SeedDataInitializer.DefaultUserId && e.CourseId == targetCourseId))
        {
            db.CourseEnrollments.Add(new CourseEnrollment
            {
                Id = Guid.NewGuid(),
                UserId = SeedDataInitializer.DefaultUserId,
                CourseId = targetCourseId,
                Role = "student",
                EnrolledAt = now
            });
            await db.SaveChangesAsync();
        }

        var chaptersToAdd = new List<Chapter>();
        foreach (var ch in chapters)
        {
            if (!await db.Chapters.AnyAsync(c => c.Id == ch.Id))
            {
                chaptersToAdd.Add(new Chapter
                {
                    Id = ch.Id,
                    CourseId = targetCourseId,
                    Title = ch.Title,
                    SortOrder = ch.SortOrder,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }
        if (chaptersToAdd.Count > 0)
        {
            db.Chapters.AddRange(chaptersToAdd);
            await db.SaveChangesAsync();
        }

        foreach (var meta in lessons)
        {
            var existing = await db.Lessons.FirstOrDefaultAsync(l => l.Id == meta.LessonId);
            if (existing != null)
            {
                if (existing.CurrentVersion == 1 && !(existing.ContentMarkdown ?? string.Empty).Contains(UpgradeMarkerV3, StringComparison.Ordinal))
                {
                    existing.ContentMarkdown = meta.Markdown;
                    existing.PlainText = meta.Markdown;
                    existing.EstimatedMinutes = meta.EstimatedMinutes;
                    existing.ContentAstJson = BuildLessonAst(existing.Title, meta.Summary1, meta.Summary2);
                    existing.UpdatedAt = now;
                }
            }
            else
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
                    ContentAstJson = BuildLessonAst(meta.Title, meta.Summary1, meta.Summary2),
                    IsPublished = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }
        await db.SaveChangesAsync();

        var existingActivities = await db.PracticeActivities
            .Where(pa => pa.CourseId == targetCourseId)
            .ToListAsync();
        var existingIds = existingActivities.Select(pa => pa.Id).ToHashSet();
        var existingComposite = existingActivities
            .Select(pa => (pa.LessonId, Type: (pa.Type ?? string.Empty).Trim().ToLowerInvariant(), pa.OrderIndex))
            .ToHashSet();

        var toAdd = new List<PracticeActivity>();
        foreach (var act in activities)
        {
            act.CourseId = targetCourseId;
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

    private static string BuildLessonAst(string title, string p1, string p2)
    {
        var ast = new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 1, block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = title } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = p1 } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
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
    // KHÓA 1: TIẾNG NHẬT SƠ CẤP (N5) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================
    // KHÓA 1: TIẾNG NHẬT SƠ CẤP (N5) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================
    // KHÓA 1: TIẾNG NHẬT SƠ CẤP (N5) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================
    // KHÓA 1: TIẾNG NHẬT SƠ CẤP (N5) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================
    // KHÓA 1: TIẾNG NHẬT SƠ CẤP (N5) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================
    // KHÓA 1: TIẾNG NHẬT SƠ CẤP (N5) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================

    private static List<CjkLessonMeta> GetJapaneseLessons() => new()
    {
        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter1Id, SeedDataInitializer.JapaneseLesson11Id,
            "Bài 1.1: Hiragana nền tảng & Câu chào hỏi hằng ngày",
            "ja-n5-hiragana-chao-hoi-hang-ngay", 1, 35,
            @"# Bài 1.1: Hiragana nền tảng & Câu chào hỏi hằng ngày

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Chào mừng bạn bước chân vào hành trình chinh phục tiếng Nhật! Bài học đầu tiên này được thiết kế chậm rãi, tỉ mỉ nhằm giúp bạn:
- Nắm vững triết lý và lý do Nhật Bản kết hợp 3 bảng chữ cái (**Hiragana**, **Katakana**, **Kanji**) thay vì chỉ dùng một bảng chữ duy nhất.
- Đọc, viết và ghi nhớ sâu sắc toàn bộ 46 chữ cái **Hiragana** cơ bản (Gojūon — 五十音) thông qua phương pháp liên tưởng hình ảnh trực quan (Mnemonics).
- Làm chủ hệ thống âm biến đổi: **Âm đục (Dakuon)**, **Âm bán đục (Handakuon)**, **Âm ghép (Yōon)**, **Âm ngắt (Sokuon - っ)** và **Trường âm (Chōon)**.
- Hiểu rõ bản chất trật tự câu tiếng Nhật (SOV: Chủ ngữ - Tân ngữ - Động từ) và sự khác biệt căn bản so với tiếng Việt (SVO).
- Sử dụng thành thạo các câu chào hỏi thường nhật theo đúng văn hóa cúi chào (Ojigi) của người bản xứ.
- Nhận diện và sửa triệt để các lỗi phát âm kinh điển mà người Việt Nam mới học rất hay mắc phải.

---

## 2. 🌱 Nhập môn chậm rãi từng bước: Vì sao tiếng Nhật dùng đến 3 bộ chữ?

### 2.1. Bản chất của 3 bộ chữ trong tiếng Nhật
Khi mới bắt đầu học, người mới thường băn khoăn: *""Tại sao người Nhật không dùng mẫu tự Latinh hay chỉ dùng một bảng chữ cái?""*
Câu trả lời nằm ở cấu trúc ngữ âm và từ vựng đặc thù của tiếng Nhật:
1. **Hiragana (ひらがな - Chữ mềm)**: Phát triển từ lối viết thảo các chữ Hán cổ, nét tròn, uốn lượn mềm mại. Dùng để viết từ thuần Nhật (Yamato kotoba), các trợ từ ngữ pháp (`は, が, を, に, で...`) và phần đuôi biến đổi của động từ, tính từ (`たべます`, `おおきい`).
2. **Katakana (カタカナ - Chữ cứng)**: Trích từ một phần nét của chữ Hán, nét thẳng, sắc sảo. Dùng để phiên âm từ mượn tiếng nước ngoài (`コーヒー` - cà phê, `ベトナム` - Việt Nam), tên người/địa danh ngoại quốc, hoặc nhấn mạnh từ ngữ.
3. **Kanji (漢字 - Chữ Hán)**: Hệ thống chữ biểu ý mượn từ chữ Hán cổ. Mỗi chữ Kanji chứa đựng một ý niệm trọn vẹn.
4. **Vì sao phải phối hợp cả 3 bộ chữ?**
   - Trong tiếng Nhật, người ta **hoàn toàn không dùng dấu cách** (khoảng trắng giữa các từ).
   - Số lượng âm tiết tiếng Nhật rất ít (chỉ khoảng 100 âm tiết), dẫn đến hiện tượng **đồng âm khác nghĩa cực kỳ phổ biến**. Ví dụ: `はな` (hana) vừa là bông hoa (花) vừa là cái mũi (鼻); `かみ` (kami) có thể là giấy (紙), tóc (髪) hay thần linh (神).
   - Nếu viết toàn bộ bằng Hiragana: `わたしははなをかいました` sẽ khiến mắt người đọc khó phân tách từ. Khi kết hợp Kanji: `私は花を買いました` (Tôi đã mua hoa), mắt nhận diện ngay danh từ và động từ cốt lõi, còn Hiragana đóng vai trò liên kết ngữ pháp.

### 2.2. Mẹo liên tưởng nhớ bảng chữ cái Hiragana (Hàng Nguyên âm & Phụ âm cốt lõi)
Hệ thống 46 chữ cái cơ bản sắp xếp theo 5 nguyên âm: `a - i - u - e - o`:
- **Hàng Nguyên âm (あ - い - う - え - お)**:
  - `あ (a)`: Hình quả táo (apple) có cuống uốn cong. Đọc mở khẩu hình tự nhiên ""a"".
  - `い (i)`: Hai con lươn bơi song song, nét trái dài hơn. Đọc là ""i"".
  - `う (u)`: Người cúi khom lưng vác nặng. Môi khép hờ, nằm giữa ""u"" và ""ư"".
  - `え (e)`: Chú chim bồ câu vươn cổ. Đọc tự nhiên giữa ""e"" và ""ê"".
  - `お (o)`: Cú đánh bóng golf có chấm tròn quả bóng. Đọc dứt khoát ""ô/o"".
- **Hàng K (か - き - く - け - こ)**:
  - `か (ka)`: Giống chữ Lực (力) có giọt mồ hôi.
  - `き (ki)`: Giống chiếc chìa khóa (key).
  - `く (ku)`: Mỏ chim đang kêu cúc cu.
  - `け (ke)`: Cây cung và thân kiếm.
  - `こ (ko)`: Hai khúc gỗ song song.
- **Hàng S (さ - し - す - せ - そ)**: `さ` giống chìa khóa một nét ngang; `し` như chiếc lưỡi câu; `す` như xích đu có vòng xoắn; `せ` như người ngồi ngắm cảnh; `そ` zíc-zắc như mũi kim khâu.
- **Hàng T (た - ち - つ - て - と)**: `た` chữ Ta ghép; `ち` số 5 lộn ngược; `つ` con sóng đại dương (nén khí bật đầu lưỡi `tsu`); `て` cánh tay vươn dài; `と` ngón chân cái bị gai đâm.
- **Hàng N, H, M, Y, R, W, N**: `ふ (fu)` khép hờ hai môi thổi nhẹ; `ん (n)` âm mũi đứng một mình, không đứng đầu từ.

### 2.3. Quy tắc các biến âm bắt buộc phải thuộc lòng
1. **Âm đục (Dakuon)**: Thêm dấu phẩy kép `゛` (Tenten): K → G (`が, ぎ, ぐ, げ, ご`), S → Z (`ざ, じ, ず, ぜ, ぞ`), T → D (`だ, ぢ, づ, で, ど`), H → B (`ば, び, ぶ, べ, ぼ`).
2. **Âm bán đục (Handakuon)**: Thêm dấu tròn nhỏ `゜` (Maru) vào hàng H → P: `ぱ, ぴ, ぷ, ぺ, ぽ`.
3. **Âm ngắt (Sokuon - っ)**: Chữ `つ` viết nhỏ biểu thị nhịp ngưng hơi đúng **1 phách** trước các phụ âm k, s, t, p (ví dụ: `ちょっと` chotto, `まって` matte).
4. **Trường âm (Chōon)**: Kéo dài nguyên âm 2 phách: hàng A + あ, hàng I + い, hàng U + う, hàng E + え/い (`せんせい`), hàng O + お/う (`おはよう`, `ありがとう`).

### 2.4. Trật tự câu tiếng Nhật: Tư duy SOV
- Tiếng Việt (SVO): Chủ ngữ + Động từ + Tân ngữ (`Tôi uống nước`).
- Tiếng Nhật (SOV): Chủ ngữ + `は` + Tân ngữ + `を` + Động từ (`わたしは みずを のみます`).
- Động từ luôn nằm ở cuối câu để quyết định thái độ: khẳng định (`〜ます`), phủ định (`〜ません`), quá khứ (`〜ました`) hay lịch sự mời mọc (`〜ませんか`).

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **わたし (私)** | watashi | Tư | Tôi, bản thân tôi | わたしは ベトナムじんです。 *(Tôi là người VN.)* |
| **おはようございます** | ohayou gozaimasu | Tảo | Chào buổi sáng (lịch sự) | せんせい、おはようございます！ *(Chào buổi sáng thầy ạ!)* |
| **こんにちは** | konnichiwa | Kim Nhật | Xin chào (buổi trưa / ban ngày) | みなさん、こんにちは。 *(Xin chào mọi người.)* |
| **こんばんは** | konbanwa | Kim Vãn | Chào buổi tối | たなかさん、こんばんは。 *(Chào buổi tối anh Tanaka.)* |
| **おやすみなさい** | oyasuminasai | Hưu | Chúc ngủ ngon | おかあさん、おやすみなさい。 *(Chúc mẹ ngủ ngon ạ.)* |
| **ありがとう ございます** | arigatou gozaimasu | Hữu Nan | Xin cảm ơn rất nhiều | どうも ありがとうございます。 *(Cảm ơn bạn rất nhiều.)* |
| **すみません** | sumimasen | Tạ | Xin lỗi / Xin phép làm phiền | すみません、ちょっと まってください。 *(Xin lỗi hãy đợi một chút.)* |
| **ごめんなさい** | gomennasai | Miễn | Xin lỗi (thân mật) | おくれて、ごめんなさい。 *(Tớ đến muộn, xin lỗi nhé.)* |
| **はじめまして** | hajimemashite | Sơ | Rất hân hạnh lần đầu gặp | はじめまして、ミンです。 *(Hân hạnh lần đầu gặp, tôi là Minh.)* |
| **おげんきですか** | ogenki desu ka | Nguyên Khí | Bạn có khỏe không? | たなかさん、おげんきですか。 *(Anh Tanaka có khỏe không?)* |
| **はい、げんきです** | hai, genki desu | Nguyên Khí | Vâng, tôi khỏe | はい、げんきです。ありがとう。 *(Vâng tôi khỏe, cảm ơn anh.)* |
| **さようなら** | sayounara | Tác Dương | Tạm biệt (thời gian dài) | せんせい、さようなら。 *(Em chào tạm biệt thầy ạ.)* |
| **また あした** | mata ashita | Minh Nhật | Hẹn gặp lại ngày mai | じゃ、また あした！ *(Thế nhé, hẹn mai gặp lại!)* |
| **どうぞ** | douzo | — | Xin mời | おちゃを どうぞ。 *(Xin mời bạn dùng trà.)* |
| **だいじょうぶです** | daijoubu desu | Đại Trượng Phu | Không sao đâu / Tôi ổn | しんぱいしないで、だいじょうぶです。 *(Đừng lo, tôi ổn mà.)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Mẫu câu khẳng định: `N1 は N2 です`
- **Ý nghĩa**: ""N1 là N2"".
- **Trợ từ `は` (đọc là wa)**: Viết là `は`, nhưng khi làm trợ từ đánh dấu chủ đề thì đọc là **wa**.
- **Đuôi `です` (desu)**: Đặt cuối câu để tạo sắc thái lịch sự, âm `u` phát âm rất nhẹ khép môi.
- Ví dụ: `わたしは がくせいです。` *(Tôi là sinh viên.)*

### 4.2. Mẫu câu phủ định: `N1 は N2 ではありません`
- Thân mật hơn có thể dùng `じゃありません`.
- Ví dụ: `わたしは せんせいでは ありません。` *(Tôi không phải là giáo viên.)*

### 4.3. Mẫu câu nghi vấn Có/Không: `N1 は N2 ですか`
- Thêm `か` vào cuối câu khẳng định. Trả lời: `はい、そうです` hoặc `いいえ、ちがいます`.
- Ví dụ: `ミンさんは がくせいですか。` — `はい、がくせいです。`

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: わたしは はな です (Đọc trợ từ は giống như ha)
   ⭕ ĐÚNG: わたしは (watashi wa) はな (hana) です。
   💡 GIẢI THÍCH: Trợ từ は đánh dấu chủ đề luôn đọc là ""wa"", còn chữ は trong từ vựng đọc là ""ha"".

2. ❌ SAI: わたし です がくせい (Dịch word-by-word theo trật tự tiếng Việt SVO)
   ⭕ ĐÚNG: わたしは がくせいです。 (Trật tự SOV chuẩn Nhật)
   💡 GIẢI THÍCH: Đuôi câu です luôn luôn nằm ở vị trí tận cùng của câu!

3. ❌ SAI: おはよう！ (Chào cụt lủn với giáo viên hoặc sếp)
   ⭕ ĐÚNG: おはようございます！ (Thêm gozaimasu trang trọng)
   💡 GIẢI THÍCH: おはよう chỉ dùng cho bạn bè ngang hàng hoặc người nhỏ tuổi hơn.

4. ❌ SAI: Nuốt trường âm đọc thành ""ô-ha-dô"" hoặc ""a-ri-ga-tô""
   ⭕ ĐÚNG: おはよう (o-ha-yō), ありがとう (a-ri-ga-tō) — ngân vang đủ 2 phách âm.
   💡 GIẢI THÍCH: Đuôi おう là trường âm hàng O, nếu ngắt cụt sẽ biến âm hoặc cộc lốc.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Buổi sáng ngày đầu tiên tại hành lang trường học. Anh Tanaka (giáo viên) gặp bạn Minh (du học sinh mới sang).

- **Tanaka**: おはようございます！
  - *Romaji*: Ohayou gozaimasu!
  - *Phân tích*: `おはよう` (chào buổi sáng) + `ございます` (hậu tố kính ngữ trang trọng).
  - *Dịch*: Chào buổi sáng em!

- **Minh**: おはようございます、たなかせんせい。おげんきですか。
  - *Romaji*: Ohayou gozaimasu, Tanaka-sensei. Ogenki desu ka?
  - *Phân tích*: `たなか` (họ) + `せんせい` (thầy giáo) + `おげんき` (khỏe khoắn) + `ですか` (hỏi lịch sự).
  - *Dịch*: Em chào buổi sáng thầy Tanaka ạ. Thầy có khỏe không ạ?

- **Tanaka**: はい、げんきです。ありがとうございます。ミンさんは？
  - *Romaji*: Hai, genki desu. Arigatou gozaimasu. Min-san wa?
  - *Phân tích*: `はい` (vâng) + `げんきです` (tôi khỏe) + `ありがとうございます` (cảm ơn) + `ミンさんは` (còn Minh thì sao?).
  - *Dịch*: Thầy khỏe, cảm ơn em. Còn Minh thì sao nào?

- **Minh**: はい、わたしも げんきです。きょうも よろしくおねがいします！
  - *Romaji*: Hai, watashi mo genki desu. Kyou mo yoroshiku onegaishimasu!
  - *Phân tích*: `わたしも` (em cũng) + `げんきです` (khỏe) + `きょうも` (hôm nay cũng) + `よろしくおねがいします` (nhờ thầy giúp đỡ).
  - *Dịch*: Vâng, em cũng khỏe ạ. Hôm nay em cũng nhờ thầy chỉ bảo thêm ạ!

- **Tanaka**: ええ、がんばりましょう！
  - *Romaji*: Ee, ganbarimashou!
  - *Phân tích*: `ええ` (ừ) + `がんばりましょう` (cùng nhau cố gắng nhé).
  - *Dịch*: Ừ, thầy trò mình cùng cố gắng nhé!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập phản xạ điền từ & trợ từ
1. Hãy điền trợ từ thích hợp vào chỗ trống: `わたし___ ベトナムじんです。`
2. Hãy viết lại câu sau theo trật tự tiếng Nhật đúng: *(Tôi là sinh viên đại học)*
3. Chữ Hiragana sau đây đọc là gì: `ち`, `つ`, `し`, `さ`?
4. Khi nhận được sự giúp đỡ của ai đó, câu nói cảm ơn lịch sự là gì?

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**: Trợ từ cần điền là **は** (phát âm là `wa`).
   - *Giải thích*: Trợ từ `は` dùng để gắn sau chủ đề của câu `わたし`.
2. **Câu 2**: `わたしは だいがくせいです。`
   - *Giải thích*: Trật tự câu tiếng Nhật là `Chủ ngữ + は + Vị ngữ danh từ + です`.
3. **Câu 3**:
   - `ち`: chi; `つ`: tsu (âm bật hơi đầu lưỡi nén khí); `し`: shi (âm xì hơi êm); `さ`: sa.
4. **Câu 4**: `ありがとうございます。` (Arigatou gozaimasu).
   - *Giải thích*: Dùng thể đầy đủ có `gozaimasu` để đảm bảo lễ nghi trang trọng.
5. **Câu 5 trắc nghiệm**: Khi bạn bè nói *""おやすみなさい""*, ngữ cảnh là lúc nào?
   - *Đáp án*: Vào buổi tối trước khi đi ngủ (Chúc ngủ ngon).

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phản xạ nghe-nói ja-JP và lưu từ mới vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Làm chủ 46 chữ cái Hiragana nền tảng, biến âm Dakuon, Sokuon và câu chào hỏi N5.",
            "Luyện trường âm, âm ngắt っ và phân biệt cặp âm つ/す, し/ち chuẩn giọng Nhật (ja-JP)."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter1Id, SeedDataInitializer.JapaneseLesson12Id,
            "Bài 1.2: Katakana, Từ mượn & Tự giới thiệu bản thân",
            "ja-n5-katakana-tu-muon-gioi-thieu", 2, 35,
            @"# Bài 1.2: Katakana, Từ mượn & Tự giới thiệu bản thân

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Bài học này sẽ dẫn dắt bạn làm chủ bảng chữ cái thứ hai cùng kỹ năng giới thiệu bản thân chuẩn phong thái N5:
- Nhận diện, đọc và viết chuẩn xác bảng chữ cứng **Katakana (カタカナ)**, hiểu rõ chức năng và phạm vi sử dụng.
- Nắm chắc mẹo phân biệt các cặp chữ Katakana ""song sinh"" dễ gây nhầm lẫn nhất: `シ` vs `ツ`, `ン` vs `ソ`.
- Làm chủ quy tắc sử dụng **Dấu trường âm Katakana (ー)** và cách phiên âm các từ mượn ngoại lai (Gairaigo).
- Thực hành mẫu tự giới thiệu bản thân (Jikoshoukai — 自己紹介) 4 bước hoàn chỉnh theo đúng tác phong giao tiếp chuẩn Nhật.
- Vận dụng linh hoạt các trợ từ sở hữu `の (no)`, trợ từ tương đồng `も (mo)`, hậu tố xưng hô lịch sự `〜さん` và hậu tố chỉ quốc tịch `〜じん`.
- Khắc phục các lỗi sai ngớ ngẩn thường gặp khi người Việt tự giới thiệu tên mình bằng tiếng Nhật.

---

## 2. 🌱 Nhập môn chậm rãi từng bước: Bảng chữ cái Katakana & Văn hóa tự giới thiệu

### 2.1. Nguồn gốc & Chức năng đặc thù của Katakana
Nếu như Hiragana mang vẻ đẹp mềm mại của nét bút lông thư pháp thì Katakana lại sở hữu những nét thẳng, sắc sảo, dứt khoát.
1. **Nguồn gốc**: Katakana ra đời vào thời Heian (thế kỷ thứ 9), do các tăng ni Phật giáo tạo ra bằng cách trích lấy một phần nét từ các chữ Hán để ghi chú nhanh cách đọc bên cạnh kinh văn chữ Hán.
2. **Khi nào bắt buộc dùng Katakana?**
   - **Từ mượn ngoại lai (Gairaigo)**: Các từ có gốc từ tiếng Anh, tiếng Pháp, tiếng Đức du nhập vào Nhật Bản hiện đại (`コーヒー` - coffee, `パン` - pan/bánh mì, `パソコン` - máy tính cá nhân, `タクシー` - taxi).
   - **Tên người và tên quốc gia nước ngoài**: Ví dụ `ベトナム` (Việt Nam), `アメリカ` (Nước Mỹ), tên người Việt như `ミン` (Minh), `リン` (Linh).
   - **Từ tượng thanh, tượng hình**: Biểu thị âm thanh tiếng động (`ドキドキ` - tim đập thình thịch, `ワンワン` - tiếng chó sủa).
   - **Tên khoa học, động thực vật**: Trong sách giáo khoa hoặc tài liệu nghiên cứu.
   - **Nhấn mạnh từ ngữ**: Tương tự như việc in hoa hoặc in nghiêng trong văn bản tiếng Anh để thu hút sự chú ý.

### 2.2. Mẹo phân biệt 2 cặp chữ Katakana ""dễ gây lú lẫn nhất""
Rất nhiều người học tiếng Nhật lâu năm vẫn viết sai 4 chữ cái này. Hãy ghi nhớ mẹo sau:
- **Cặp 1: `シ (shi)` vs `ツ (tsu)`**:
  - `シ (shi)`: Nhìn giống mặt cười đang liếc ngang. Hai nét phẩy ngắn nằm gần như theo hàng dọc bên trái, nét vuốt dài bắt đầu từ **dưới vuốt chếch lên trên sang phải**. (Liên tưởng: cô gái mắt liếc ngang duyên dáng Shhh!).
  - `ツ (tsu)`: Nhìn giống hai giọt nước từ trên cao rơi thẳng xuống. Hai nét phẩy ngắn nằm ngang trên đỉnh đầu, nét vuốt dài bắt đầu từ **trên vuốt chém xuống dưới sang trái**. (Liên tưởng: sóng thần Tsu-nami ập từ trên cao xuống).
- **Cặp 2: `ン (n)` vs `ソ (so)`**:
  - `ン (n)`: Chỉ có 1 nét phẩy ngắn nằm ngang bên trái, nét dài vuốt từ **dưới lên trên**.
  - `ソ (so)`: Nét phẩy ngắn dựng đứng bên trái, nét dài chém từ **trên xuống dưới**.

### 2.3. Dấu trường âm trong Katakana: Dấu gạch ngang `ー`
- Trong Hiragana, trường âm được biểu thị bằng cách viết thêm nguyên âm tương ứng (`おおきい`, `せんせい`).
- Nhưng trong Katakana, **toàn bộ trường âm của mọi nguyên âm đều được biểu thị duy nhất bằng một dấu gạch ngang: `ー`** (Chōonpu).
- Quy tắc: Kéo dài nguyên âm liền trước đúng 1 phách âm.
  - Ví dụ: `コーヒー` (koo-hii: cà phê, kéo dài cả 2 âm); `カード` (kaa-do: thẻ card); `ノート` (noo-to: cuốn vở ghi chép); `スーパー` (suu-paa: siêu thị).
  - Tuyệt đối không được bỏ quên dấu gạch ngang này, vì bỏ đi sẽ làm biến đổi hoàn toàn nghĩa của từ!

### 2.4. Văn hóa Tự giới thiệu bản thân (Jikoshoukai - 自己紹介)
Người Nhật cực kỳ coi trọng ấn tượng đầu tiên (Dai-ichi inshou). Một bài tự giới thiệu bản thân chuẩn mực gồm 4 bước:
1. **Lời mở đầu**: `はじめまして` (Hajimemashite — Rất hân hạnh lần đầu tiên được diện kiến). Cúi đầu chào góc 30 độ.
2. **Tên & Quốc tịch**: `わたしは [Tên] です。ベトナムから きました。` (Tôi là... Tôi đến từ Việt Nam).
3. **Nghề nghiệp / Trực thuộc**: `[Tên trường/Công ty] の [Nghề nghiệp] です。`
4. **Lời kết thúc kinh điển**: `どうぞ よろしく おねがいします。` (Douzo yoroshiku onegaishimasu — Rất mong nhận được sự giúp đỡ và chỉ bảo của bạn). Cúi đầu chào sâu hơn.

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **ベトナム** | Betonamu | — | Việt Nam | わたしは ベトナムから きました。 *(Tôi đến từ VN.)* |
| **にほん (日本)** | Nihon | Nhật Bản | Nhật Bản | にほんは きれいな くにです。 *(Nhật Bản là đất nước đẹp.)* |
| **アメリカ** | Amerika | — | Nước Mỹ / Hoa Kỳ | スミスさんは アメリカじんです。 *(Anh Smith là người Mỹ.)* |
| **がくせい (学生)** | gakusei | Học Sinh | Học sinh, sinh viên | わたしは だいがくの がくせいです。 *(Tôi là sinh viên ĐH.)* |
| **せんせい (先生)** | sensei | Tiên Sinh | Thầy/Cô giáo | やまだせんせいは にほんごの せんせいです。 *(Thầy Yamada dạy tiếng Nhật.)* |
| **かいしゃいん (会社員)** | kaishain | Hội Xã Viên | Nhân viên công ty | たなかさんは かいしゃいんです。 *(Anh Tanaka là nhân viên công ty.)* |
| **エンジニア** | enjinia | — | Kỹ sư (Engineer) | ミンさんは ITの エンジニアです。 *(Minh là kỹ sư IT.)* |
| **だいがく (大学)** | daigaku | Đại Học | Trường đại học | とうきょうだいがくの がくせいです。 *(Sinh viên ĐH Tokyo.)* |
| **コーヒー** | koohii | — | Cà phê (Coffee) | まいあさ コーヒーを のみます。 *(Mỗi sáng tôi uống cà phê.)* |
| **パソコン** | pasokon | — | Máy tính cá nhân (PC) | これは あたらしい パソコンです。 *(Đây là chiếc máy tính mới.)* |
| **ノート** | nooto | — | Vở ghi chép (Notebook) | ノートに ひらがなを かきます。 *(Viết Hiragana vào vở.)* |
| **ホテル** | hoteru | — | Khách sạn (Hotel) | えきの まえに ホテルが あります。 *(Trước ga có khách sạn.)* |
| **レストラン** | resutoran | — | Nhà hàng (Restaurant) | レストランで ごはんを たべます。 *(Ăn cơm ở nhà hàng.)* |
| **どうぞ よろしく** | douzo yoroshiku | — | Rất mong được giúp đỡ | はじめまして、どうぞ よろしく。 *(Hân hạnh gặp bạn, nhờ bạn giúp đỡ.)* |
| **〜さん** | ~san | — | Anh/Chị/Ông/Bà (kính cẩn) | さとうさんは やさしい ひとです。 *(Chị Sato là người tốt bụng.)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Trợ từ sở hữu & quan hệ `の (no)`
- **Công thức**: `[Danh từ 1] の [Danh từ 2]`
- **Ý nghĩa**: N2 của N1 (thuộc quyền sở hữu, hoặc N1 bổ nghĩa làm rõ phạm trù cho N2).
- Khác biệt so với tiếng Việt: Tiếng Việt nói ""Sinh viên của trường đại học"" (Cái chính nói trước, cái thuộc về nói sau). Tiếng Nhật nói ngược lại hoàn toàn: `だいがく (N1) の がくせい (N2)`.
- Ví dụ:
  - `わたしの ほん` *(Sách của tôi)*.
  - `FPTの エンジニア` *(Kỹ sư của công ty FPT)*.
  - `にほんごの せんせい` *(Giáo viên dạy tiếng Nhật)*.

### 4.2. Trợ từ tương đồng `も (mo)`
- **Ý nghĩa**: ""Cũng..."".
- **Cách dùng**: Khi chủ ngữ hoặc chủ đề câu thứ hai có đặc điểm giống hệt câu thứ nhất, ta **thay thế trực tiếp trợ từ `は` bằng trợ từ `も`**.
- Ví dụ:
  - `ミンさんは がくせいです。` *(Minh là sinh viên.)*
  - `アンさんも がくせいです。` *(An cũng là sinh viên — Thay は bằng も).*

### 4.3. Hậu tố xưng hô `〜さん` và hậu tố chỉ quốc tịch `〜じん`
- **Hậu tố `〜さん` (san)**: Gắn sau tên người khác để biểu lộ sự kính trọng lịch sự (tương đương anh/chị/ông/bà trong tiếng Việt).
  - **Quy tắc vàng bất di bất dịch**: **TUYỆT ĐỐI KHÔNG BAO GIỜ GẮN `〜さん` SAU TÊN CỦA CHÍNH BẢN THÂN MÌNH!** Tự xưng mình là `Minh-san` là hành vi khiếm nhã, tự kiêu trong văn hóa Nhật.
- **Hậu tố `〜じん` (jin — 人)**: Gắn sau tên quốc gia để tạo thành từ chỉ người mang quốc tịch nước đó.
  - `ベトナム` (Việt Nam) → `ベトナムじん` (Người Việt Nam).
  - `にほん` (Nhật Bản) → `にほんじん` (Người Nhật Bản).

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: わたしは ミンさん です。 (Tự xưng tên mình có thêm hậu tố さん)
   ⭕ ĐÚNG: わたしは ミンです。 (Hoặc trang trọng hơn: ミンと もうします)
   💡 GIẢI THÍCH: Hậu tố さん dùng để tôn kính người đối diện. Tự xưng mình là さん là lỗi văn hóa rất nặng!

2. ❌ SAI: わたしは ベトナム人 の がくせい です。 (Lạm dụng trợ từ の sai vị trí)
   ⭕ ĐÚNG: わたしは ベトナムのがくせいです。 hoặc わたしは ベトナムじんです。
   💡 GIẢI THÍCH: Không ghép quốc tịch và nghề nghiệp lủng củng bằng trợ từ の thừa thãi.

3. ❌ SAI: Quên dấu trường âm Katakana: コヒ, パソコ
   ⭕ ĐÚNG: コーヒー (koohii), パソコン (pasokon)
   💡 GIẢI THÍCH: Trong Katakana, thiếu dấu trường âm ー sẽ làm sai lệch hẳn từ vựng chuẩn quốc tế.

4. ❌ SAI: Nhầm nét chữ シ (shi) thành ツ (tsu) trong タクシー (taxi)
   ⭕ ĐÚNG: タクシー (Viết nét vuốt từ dưới lên cho chữ シ)
   💡 GIẢI THÍCH: Nếu vuốt từ trên xuống sẽ thành タクツー, người Nhật nhìn vào sẽ thấy kỳ lạ.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Buổi lễ chào đón sinh viên quốc tế tại Trường Đại học Công nghệ Tokyo. Bạn Minh gặp chị Sato (cố vấn học tập khoa CNTT).

- **Minh**: はじめまして。わたしは ミンです。ベトナムから きました。
  - *Romaji*: Hajimemashite. Watashi wa Min desu. Betonamu kara kimashita.
  - *Phân tích*: `はじめまして` (lần đầu gặp gỡ) + `わたしは` (tôi là) + `ミンです` (Minh) + `ベトナムから` (từ Việt Nam) + `きました` (đã đến).
  - *Dịch*: Xin chào lần đầu gặp mặt. Tôi tên là Minh. Tôi đến từ Việt Nam.

- **Sato**: はじめまして、さとうです。ミンさんは がくせいですか。
  - *Romaji*: Hajimemashite, Satou desu. Min-san wa gakusei desu ka?
  - *Phân tích*: `さとうです` (tôi là Sato) + `ミンさんは` (bạn Minh) + `がくせいですか` (là sinh viên phải không?).
  - *Dịch*: Rất vui được gặp bạn, tôi là Sato. Bạn Minh là sinh viên đúng không?

- **Minh**: はい、ITの がくせいです。エンジニアを めざしています。
  - *Romaji*: Hai, IT no gakusei desu. Enjinia o mezashite imasu.
  - *Phân tích*: `はい` (vâng) + `ITの がくせい` (sinh viên ngành CNTT) + `エンジニアを` (kỹ sư) + `めざしています` (đang hướng tới mục tiêu trở thành).
  - *Dịch*: Vâng, tôi là sinh viên ngành IT. Tôi đang phấn đấu trở thành kỹ sư phần mềm.

- **Sato**: そうですか！すばらしいですね。どうぞ よろしくおねがいします。
  - *Romaji*: Sou desu ka! Subarashii desu ne. Douzo yoroshiku onegaishimasu.
  - *Phân tích*: `そうですか` (vậy sao!) + `すばらしいですね` (tuyệt vời quá nhỉ) + `どうぞよろしくおねがいします` (rất vui được hợp tác và giúp đỡ em).
  - *Dịch*: Vậy à! Tuyệt vời quá. Rất mong được giúp đỡ bạn nhé!

- **Minh**: こちらこそ、よろしく おねがいします！
  - *Romaji*: Kochira koso, yoroshiku onegaishimasu!
  - *Phân tích*: `こちらこそ` (chính tôi mới là người cần nói câu đó / về phía tôi cũng vậy).
  - *Dịch*: Chính tôi mới là người cần sự giúp đỡ của chị ạ!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập viết chữ & ngữ pháp
1. Hãy chuyển các từ mượn sau đây sang chữ Katakana: *Coffee*, *Notebook*, *Hotel*.
2. Điền trợ từ `の` hoặc `も` vào chỗ trống:
   - `たなかさんは せんせいです。すずきさん___ せんせいです。`
   - `これは わたし___ パソコンです。`
3. Hãy viết một đoạn giới thiệu bản thân 4 câu bằng tiếng Nhật dựa trên thông tin của chính bạn.

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - Coffee → `コーヒー` (có 2 dấu gạch ngang trường âm)
   - Notebook → `ノート`
   - Hotel → `ホテル`
2. **Câu 2**:
   - Câu đầu điền **も** (`すずきさんも せんせいです` — Suzuki cũng là giáo viên).
   - Câu sau điền **の** (`わたしの パソコン` — Máy tính của tôi).
3. **Câu 3 mẫu**:
   - `はじめまして。わたしは アンです。ベトナムじんです。だいがくせいです。どうぞよろしくおねがいします。`
4. **Câu 4**: Phân biệt chữ `シ` và `ツ` trong từ Katakana `タクシー`:
   - Chữ thứ 3 là `シ (shi)` với nét vuốt từ dưới lên trên.
5. **Câu 5 trắc nghiệm**: Tại sao không được nói `わたしは ミンさん です`?
   - *Đáp án*: Vì hậu tố `さん` thể hiện sự tôn kính đối phương, không được dùng để xưng hô bản thân mình.

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phản xạ nghe-nói ja-JP và lưu từ mới vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Nhận diện bảng Katakana, phân biệt シ/ツ và ソ/ン, thực hành tự giới thiệu bản thân 4 bước N5.",
            "Vận dụng chuẩn trợ từ は, の, も và hậu tố chỉ quốc tịch 〜じん (〜人)."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter2Id, SeedDataInitializer.JapaneseLesson21Id,
            "Bài 2.1: Số đếm, Giờ giấc & Lịch hẹn cơ bản",
            "ja-n5-so-dem-gio-giac-lich-hen", 1, 35,
            @"# Bài 2.1: Số đếm, Giờ giấc & Lịch hẹn cơ bản

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Làm chủ hệ thống con số và thời gian là bước ngoặt quan trọng giúp bạn bắt đầu sinh hoạt thực tế tại Nhật Bản:
- Đếm thành thạo các con số từ 0 đến 10.000 trong tiếng Nhật, nắm bắt cơ chế tư duy theo vạn (`万 - まん`) thay vì theo nghìn của người Á Đông.
- Thuộc làu các trường hợp biến âm bất quy tắc kinh điển khi ghép số với đơn vị đo giờ (**〜じ**) và phút (**〜ふん / 〜ぷん**).
- Làm chủ cách gọi 7 ngày trong tuần dựa trên thuyết ngũ hành và thiên thể (Nguyệt, Hỏa, Thủy, Mộc, Kim, Thổ, Nhật).
- Sử dụng chuẩn xác trợ từ thời điểm **に (ni)**, phân biệt rõ các trường hợp bắt buộc dùng `に` và trường hợp cấm kỵ dùng `に`.
- Thiết lập lịch hẹn, hỏi giờ hiện tại và diễn đạt khoảng thời gian làm việc bằng cấu trúc **〜から 〜まで** (Từ... đến...).
- Vượt qua các lỗi nhầm lẫn tai hại của người Việt về mốc 4 giờ, 7 giờ, 9 giờ và các âm ngắt phút.

---

## 2. 🌱 Nhập môn chậm rãi từng bước: Hệ thống số đếm & Thời gian

### 2.1. Bản chất hệ đếm từ 1 đến 10.000 trong tiếng Nhật
Hệ số đếm cơ bản từ 1 đến 10:
- `1: いち (ichi)` | `2: に (ni)` | `3: さん (san)` | `4: よん / し (yon / shi)` | `5: ご (go)`
- `6: ろく (roku)` | `7: なな / しち (nana / shichi)` | `8: はち (hachi)` | `9: きゅう / く (kyuu / ku)` | `10: じゅう (juu)`

**Điểm đặc biệt cần lưu ý về hàng số lớn**:
- `100: ひゃく (hyaku)` | `1.000: せん (sen)` | `10.000: いちまん (ichiman)`
- Trong tiếng Việt và phương Tây, các con số được ngắt nhóm 3 chữ số (hàng nghìn - thousand): `10.000` đọc là ""mười nghìn"" (ten thousand).
- Nhưng trong tiếng Nhật và văn hóa Á Đông, các con số được ngắt nhóm **4 chữ số** (hàng vạn - 万): `10.000` là một đơn vị độc lập gọi là **いちまん (1 vạn)**. Do đó `50.000` sẽ là `ごまん (5 vạn)`, không có từ ghép ""năm mươi nghìn"".

### 2.2. Biến âm giờ (〜じ - ji): 3 mốc thời gian ""bẫy"" học viên
Khi đếm giờ, ta gắn hậu tố `〜じ (時)` sau số đếm. Có 3 mốc giờ đặc biệt bắt buộc bạn phải nhớ như in:
1. **4 giờ: よじ (yoji)** — **Tuyệt đối KHÔNG đọc là しじ hay よんじ!** (Vì chữ `し` đồng âm với chữ ""Tử - chết"" nên người Nhật kiêng kỵ khi nói giờ).
2. **7 giờ: しちじ (shichiji)** — Thường ưu tiên đọc là `しちじ` thay vì `ななじ`.
3. **9 giờ: くじ (kuji)** — **Tuyệt đối KHÔNG đọc là きゅうじ!** (Chữ `く` ở đây là biến âm ngắn gọn của số 9).

### 2.3. Bảng biến âm phút (〜ふん / 〜ぷん): Quy tắc âm ngắt
Đếm phút là thử thách lớn nhất cho người mới bắt đầu vì hiện tượng biến âm giữa `ふん (fun)` và `ぷん (pun - có âm ngắt)`:
- **Các số đi với `ぷん (pun)` và có âm ngắt `っ`**:
  - `1 phút`: **いっぷん** (ippun) — biến từ `いち + ふん`.
  - `3 phút`: **さんぷん** (sanpun) — đọc nhẹ âm p.
  - `4 phút`: **よんぷん** (yonpun).
  - `6 phút`: **ろっぷん** (roppun) — biến từ `ろく + ふん`.
  - `8 phút`: **はっぷん** (happun) — biến từ `はち + ふん`.
  - `10 phút`: **じゅっぷん** (juppun / hoặc jippun).
- **Các số đi với `ふん (fun)` bình thường**:
  - `2 phút: にふん` | `5 phút: ごふん` | `7 phút: ななふん` | `9 phút: きゅうふん`.
- **Nửa giờ (30 phút)**: dùng từ **はん (半 - han)**. Ví dụ: `くじはん` = 9 giờ rưỡi.

### 2.4. Thứ trong tuần theo quy luật Thiên thể & Ngũ hành
Người Nhật đặt tên 7 ngày trong tuần theo mặt trời, mặt trăng và 5 hành tố vũ trụ:
- **Thứ Hai**: `げつようび (月曜日 - Nguyệt)` — Ngày của mặt trăng.
- **Thứ Ba**: `かようび (火曜日 - Hỏa)` — Ngày của lửa.
- **Thứ Tư**: `すいようび (水曜日 - Thủy)` — Ngày của nước.
- **Thứ Năm**: `もくようび (木曜日 - Mộc)` — Ngày của cây cối rừng xanh.
- **Thứ Sáu**: `きんようび (金曜日 - Kim)` — Ngày của vàng bạc kim loại.
- **Thứ Bảy**: `どようび (土曜日 - Thổ)` — Ngày của đất mẹ.
- **Chủ Nhật**: `にちようび (日曜日 - Nhật)` — Ngày của vầng thái dương.

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **いま (今)** | ima | Kim | Bây giờ | いま なんじですか。 *(Bây giờ là mấy giờ?)* |
| **〜じ (〜時)** | ~ji | Thời | ... giờ | いまは くじです。 *(Bây giờ là 9 giờ.)* |
| **〜ふん / 〜ぷん (〜分)** | ~fun / ~pun | Phân | ... phút | じゅうごふん かかります。 *(Mất khoảng 15 phút.)* |
| **はん (半)** | han | Bán | Rưỡi, một nửa (30 phút) | しちじはんに おきます。 *(Thức dậy lúc 7 giờ rưỡi.)* |
| **なんじ (何時)** | nanji | Hà Thời | Mấy giờ? | かいぎは なんじからですか。 *(Cuộc họp từ mấy giờ?)* |
| **なんぷん (何分)** | nanpun | Hà Phân | Mấy phút? | なんぷん まちましたか。 *(Bạn đã đợi mấy phút rồi?)* |
| **ごぜん (午前)** | gozen | Ngọ Tiền | Buổi sáng (AM) | ごぜん はちじに いきます。 *(Đi lúc 8 giờ sáng.)* |
| **ごご (午後)** | gogo | Ngọ Hậu | Buổi chiều / tối (PM) | ごご にじに あいましょう。 *(Gặp nhau lúc 2 giờ chiều nhé.)* |
| **きょう (今日)** | kyou | Kim Nhật | Hôm nay | きょうは すいようびです。 *(Hôm nay là thứ Tư.)* |
| **あした (明日)** | ashita | Minh Nhật | Ngày mai | あした テストが あります。 *(Ngày mai có bài kiểm tra.)* |
| **きのう (昨日)** | kinou | Tạc Nhật | Hôm qua | きのう たくさん べんきょうしました。 *(Hôm qua học nhiều.)* |
| **まいにち (毎日)** | mainichi | Mỗi Nhật | Mỗi ngày, hằng ngày | まいにち にほんごを はなします。 *(Hằng ngày tôi nói tiếng Nhật.)* |
| **まいあさ (毎朝)** | maiasa | Mỗi Triêu | Mỗi buổi sáng | まいあさ ろくじに おきます。 *(Mỗi sáng tôi dậy lúc 6 giờ.)* |
| **まいばん (毎晩)** | maiban | Mỗi Vãn | Mỗi buổi tối | まいばん ほんを よみます。 *(Mỗi tối tôi đọc sách.)* |
| **おきます (起きます)** | okimasu | Khởi | Thức dậy | あした しちじに おきます。 *(Ngày mai tôi sẽ dậy lúc 7 giờ.)* |
| **ねます (寝ます)** | nemasu | Tẩm | Đi ngủ | まいばん じゅういちじに ねます。 *(Tôi ngủ lúc 11 giờ đêm.)* |
| **はたらきます (働きます)** | hatarakimasu | Động | Làm việc | ぎんこうで はたらきます。 *(Làm việc ở ngân hàng.)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Trợ từ thời điểm `に (ni)`: Khi nào DÙNG và khi nào CẤM DÙNG?
- **Quy tắc bắt buộc DÙNG `に`**:
  - Gắn sau các mốc thời gian **có con số cụ thể** (giờ, phút, ngày, tháng, năm, thứ trong tuần).
  - Ví dụ: `しちじに おきます` *(Thức dậy lúc 7 giờ)*; `にちようびに あいます` *(Gặp nhau vào Chủ nhật)*.
- **Quy tắc CẤM DÙNG `に`**:
  - Tuyệt đối **KHÔNG dùng `に`** sau các từ chỉ thời gian mang tính ước lệ, tương đối so với thời điểm nói như: `きょう (hôm nay)`, `あした (ngày mai)`, `きのう (hôm qua)`, `いま (bây giờ)`, `まいにち (mỗi ngày)`, `こんしゅう (tuần này)`.
  - Nói `あしたに いきます` là hoàn toàn sai ngữ pháp! Phải nói: `あした いきます`.

### 4.2. Cấu trúc khoảng thời gian: `〜から 〜まで` (Từ... đến...)
- **Công thức**: `[Thời điểm 1] から [Thời điểm 2] まで [Động từ]`
- `から (kara)`: Từ (mốc bắt đầu).
- `まで (made)`: Đến (mốc kết thúc).
- Có thể dùng kết hợp cả hai hoặc dùng độc lập từng từ.
- Ví dụ:
  - `くじから ごじまで はたらきます。` *(Tôi làm việc từ 9 giờ đến 5 giờ).*
  - `かいぎは じゅうじからです。` *(Cuộc họp bắt đầu từ 10 giờ).*

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: きょうに とうきょうへ いきます (Thêm trợ từ に sau từ thời gian tương đối)
   ⭕ ĐÚNG: きょう とうきょうへ いきます。 (Bỏ hoàn toàn trợ từ に)
   💡 GIẢI THÍCH: きょう, あした, まいにち không có con số cụ thể nên không bao giờ đi với に.

2. ❌ SAI: 4 giờ nói là しじ hay よんじ
   ⭕ ĐÚNG: よじ (yoji) — phát âm ngắn gọn 2 âm tiết.
   💡 GIẢI THÍCH: 4 giờ có quy tắc biến âm bất di bất dịch trong tiếng Nhật là よじ.

3. ❌ SAI: 9 giờ nói là きゅうじ
   ⭕ ĐÚNG: くじ (kuji)
   💡 GIẢI THÍCH: Tương tự như 4 giờ, 9 giờ chỉ được phát âm là くじ khi đi với hậu tố 時.

4. ❌ SAI: 10.000 yên đọc là じゅうせんえん (Mười nghìn theo tiếng Việt)
   ⭕ ĐÚNG: いちまんえん (1 vạn yên)
   💡 GIẢI THÍCH: Tiếng Nhật gom nhóm 4 số 0 thành đơn vị まん (万), 10.000 bắt buộc đọc là いちまん.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Tại văn phòng làm việc ở Tokyo. Chị Lan hỏi anh Ken về lịch trình cuộc họp dự án sắp tới.

- **Lan**: すみません、いま なんじですか。
  - *Romaji*: Sumimasen, ima nanji desu ka?
  - *Phân tích*: `すみません` (xin lỗi xin phép) + `いま` (bây giờ) + `なんじですか` (mấy giờ rồi?).
  - *Dịch*: Xin lỗi anh, bây giờ là mấy giờ rồi ạ?

- **Ken**: いま ごぜん くじはんです。
  - *Romaji*: Ima gozen kuji han desu.
  - *Phân tích*: `いま` (hiện tại) + `ごぜん` (buổi sáng AM) + `くじはん` (9 giờ 30 phút / 9 rưỡi) + `です` (lịch sự).
  - *Dịch*: Bây giờ là 9 giờ rưỡi sáng.

- **Lan**: かいぎは なんじから なんじまでですか。
  - *Romaji*: Kaigi wa nanji kara nanji made desu ka?
  - *Phân tích*: `かいぎは` (cuộc họp) + `なんじから` (từ mấy giờ) + `なんじまで` (đến mấy giờ) + `ですか` (hỏi).
  - *Dịch*: Cuộc họp diễn ra từ mấy giờ đến mấy giờ vậy anh?

- **Ken**: じゅうじから じゅういちじさんじゅっぷんまでです。
  - *Romaji*: Juuji kara juuichiji sanjuppun made desu.
  - *Phân tích*: `じゅうじから` (từ 10:00) + `じゅういちじさんじゅっぷんまで` (đến 11:30) + `です`.
  - *Dịch*: Từ 10 giờ đến 11 giờ 30 phút em nhé.

- **Lan**: わかりました。あしたは なんじに おきますか。
  - *Romaji*: Wakarimashita. Ashita wa nanji ni okimasu ka?
  - *Phân tích*: `わかりました` (tôi đã hiểu rồi) + `あしたは` (ngày mai) + `なんじに` (lúc mấy giờ) + `おきますか` (anh thức dậy?).
  - *Dịch*: Em hiểu rồi ạ. Sáng mai anh thức dậy lúc mấy giờ thế?

- **Ken**: ろくじはんに おきます。じゃ、じゅんびを しましょう！
  - *Romaji*: Rokuji han ni okimasu. Ja, junbi o shimashou!
  - *Phân tích*: `ろくじはんに` (lúc 6:30) + `おきます` (dậy) + `じゃ` (vậy thì) + `じゅんびをしましょう` (chúng ta cùng chuẩn bị nào).
  - *Dịch*: Anh dậy lúc 6 rưỡi. Nào, chúng mình cùng chuẩn bị tài liệu thôi!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập phản xạ đọc số & thời gian
1. Đọc các mốc giờ sau bằng Hiragana: `4:00`, `7:00`, `9:00`, `11:30`.
2. Đọc các số phút sau: `1 phút`, `3 phút`, `6 phút`, `10 phút`.
3. Điền trợ từ `に`, `から`, `まで` thích hợp vào câu:
   - `わたしは まいばん じゅうにじ___ ねます。`
   - `ぎんこうは くじ___ さんじ___ です。`

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - 4:00: `よじ` (yoji)
   - 7:00: `しちじ` (shichiji)
   - 9:00: `くじ` (kuji)
   - 11:30: `じゅういちじはん` (juuichiji han)
2. **Câu 2**:
   - 1 phút: `いっぷん` (ippun)
   - 3 phút: `さんぷん` (sanpun)
   - 6 phút: `ろっぷん` (roppun)
   - 10 phút: `じゅっぷん` (juppun)
3. **Câu 3**:
   - Câu đầu điền **に** (`じゅうにじに ねます` — Đi ngủ lúc 12 giờ đêm).
   - Câu sau điền **から / まで** (`くじから さんじまで` — Từ 9 giờ đến 3 giờ).
4. **Câu 4 trắc nghiệm**: Tại sao câu `あしたに がっこうへ いきます` lại sai?
   - *Đáp án*: Vì `あした` là từ chỉ thời gian tương đối, không được đi với trợ từ `に`.
5. **Câu 5 trắc nghiệm**: Thứ Tư trong tiếng Nhật đọc là gì?
   - *Đáp án*: `すいようび (水曜日)` — Ngày Thủy.

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phản xạ nghe-nói ja-JP và lưu từ mới vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Thuộc số đếm 1–10.000, các biến âm giờ phút đặc biệt (よじ, しちじ, くじ, ろっぷん) và cấu trúc 〜から〜まで.",
            "Đặt lịch hẹn cơ bản và sử dụng chính xác trợ từ に sau mốc thời gian cụ thể."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter2Id, SeedDataInitializer.JapaneseLesson22Id,
            "Bài 2.2: Hỏi giá, Mua sắm & Đặt món ăn",
            "ja-n5-hoi-gia-mua-sam-dat-mon", 2, 35,
            @"# Bài 2.2: Hỏi giá, Mua sắm & Đặt món ăn

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Đi mua sắm tại cửa hàng tiện lợi (Konbini) và tự tin gọi món ăn tại tiệm mì Nhật Bản là trải nghiệm tuyệt vời:
- Phân biệt rạch ròi hệ thống đại từ chỉ định **これ / それ / あれ / どれ** (đứng độc lập) và bộ từ hạn định **この / その / あの / どの + Danh từ**.
- Nắm vững bộ số đếm thuần Nhật từ 1 đến 10 (**ひとつ〜とお**) chuyên dùng đếm đồ vật khi gọi món ăn và mua sắm.
- Đọc chính xác mệnh giá tiền Yên Nhật (円 - えん), thuộc lòng các biến âm hàng trăm (**さんびゃく, ろっぴゃく, はっぴゃく**) và hàng nghìn (**さんぜん, はっせん**).
- Vận dụng nhuần nhuyễn mẫu câu hỏi giá tiền **〜は いくらですか** và mẫu câu đề nghị lịch sự **〜を ください**.
- Sử dụng đúng trợ từ liên kết **と (to)** khi gọi nhiều món ăn cùng lúc.
- Luyện phong thái giao tiếp tự nhiên với nhân viên phục vụ qua các cụm từ cửa sổ: `いらっしゃいませ`, `かしこまりました`.

---

## 2. 🌱 Nhập môn chậm rãi từng bước: Hệ từ chỉ định & Bộ số đếm thuần Nhật

### 2.1. Hệ thống chỉ định từ Ko - So - A - Do (こ・そ・あ・ど)
Trong tiếng Nhật, vị trí của đồ vật được xác định dựa trên mối tương quan khoảng cách giữa Người nói và Người nghe:
- **Nhóm こ (Ko - Gần người nói)**:
  - `これ (kore)`: Cái này (vật ở sát ngay cạnh người nói).
  - `この + N (kono)`: ... này (bắt buộc đi kèm danh từ ngay phía sau).
  - `ここ (koko)`: Chỗ này, ở đây.
- **Nhóm そ (So - Gần người nghe)**:
  - `それ (sore)`: Cái đó (vật ở gần người nghe, xa người nói).
  - `その + N (sono)`: ... đó.
  - `そこ (soko)`: Chỗ đó, ở đó.
- **Nhóm あ (A - Xa cả hai người)**:
  - `あれ (are)`: Cái kia (vật ở xa cả người nói lẫn người nghe).
  - `あの + N (ano)`: ... kia.
  - `あそこ (asoko)`: Chỗ kia.
- **Nhóm ど (Do - Nghi vấn từ)**:
  - `どれ (dore)`: Cái nào?
  - `どの + N (dono)`: ... nào?
  - `どこ (doko)`: Ở đâu?

**SỰ KHÁC BIỆT SỐNG CÒN GIỮA `これ` VÀ `この`**:
- `これ` là **Đại từ độc lập**, có thể đứng một mình làm chủ ngữ trước trợ từ `は`:
  - `これは いくらですか。` *(Cái này bao nhiêu tiền? — ĐÚNG).*
- `この` là **Từ hạn định (Liên thể từ)**, **BẮT BUỘC phải có Danh từ đi kèm ngay sau nó**, không bao giờ đứng đơn độc một mình:
  - `このほんは いくらですか。` *(Cuốn sách này bao nhiêu tiền? — ĐÚNG).*
  - `このは いくらですか。` *(SAI HOÀN TOÀN!).*

### 2.2. Bộ số đếm thuần Nhật cho đồ vật (ひとつ 〜 とお)
Khi gọi món ăn (cà phê, bánh mì, bát mì ramen, quả táo), người Nhật không đếm bằng `いち, に, さん` mà dùng bộ số đếm thuần Nhật cổ:
- `1 cái`: **ひとつ (hitotsu)**
- `2 cái`: **ふたつ (futatsu)**
- `3 cái`: **みっつ (mittsu)** — có âm ngắt.
- `4 cái`: **よっつ (yottsu)** — có âm ngắt.
- `5 cái`: **いつつ (itsutsu)**
- `6 cái`: **むっつ (muttsu)** — có âm ngắt.
- `7 cái`: **ななつ (nanatsu)**
- `8 cái`: **やっつ (yattsu)** — có âm ngắt.
- `9 cái`: **ここのつ (kokonotsu)**
- `10 cái`: **とお (too)** — kéo dài trường âm, không có đuôi `つ`.
- Từ 11 trở lên: Quay trở lại dùng số đếm thường ghép với danh từ.

### 2.3. Tiền Yên Nhật & Biến âm hàng Trăm, Nghìn
Đơn vị tiền tệ của Nhật là Yên (円), đọc là **えん (en)**.
- **Biến âm hàng trăm (〜ひゃく - hyaku)**:
  - 100: `ひゃく`
  - 300: **さんびゃく** (sanbyaku) — biến âm đục `びゃ`.
  - 600: **ろっぴゃく** (roppyaku) — biến âm ngắt + bán đục `っぴゃ`.
  - 800: **はっぴゃく** (happyaku) — biến âm ngắt + bán đục `っぴゃ`.
- **Biến âm hàng nghìn (〜せん - sen)**:
  - 1.000: `せん`
  - 3.000: **さんぜん** (sanzen) — biến âm đục `ぜ`.
  - 8.000: **はっせん** (hassen) — biến âm ngắt `っせ`.

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **これ / それ / あれ** | kore / sore / are | — | Cái này / Cái đó / Cái kia | これは いくらですか。 *(Cái này bao nhiêu tiền?)* |
| **この / その / あの** | kono / sono / ano | — | ... này / ... đó / ... kia | このシャツは たかいです。 *(Chiếc áo sơ mi này đắt.)* |
| **いくら** | ikura | — | Bao nhiêu tiền? | このラーメンは いくらですか。 *(Mì ramen này bao nhiêu tiền?)* |
| **〜えん (〜円)** | ~en | Yên | Đồng Yên Nhật | ぜんぶで はっぴゃくえんです。 *(Tổng cộng là 800 yên.)* |
| **〜を ください** | ~o kudasai | — | Làm ơn cho tôi... | これを ひとつ ください。 *(Cho tôi lấy 1 cái này.)* |
| **メニュー** | menyuu | — | Thực đơn (Menu) | メニューを みせてください。 *(Làm ơn cho tôi xem thực đơn.)* |
| **ラーメン** | raamen | — | Mì Ramen Nhật | しょうゆラーメンを ひとつ ください。 *(Cho tôi 1 mì ramen shoyu.)* |
| **サンドイッチ** | sandoicchi | — | Bánh sandwich | サンドイッチを ふたつ かいました。 *(Tôi đã mua 2 cái sandwich.)* |
| **みず (水) / おみず** | mizu / omizu | Thủy | Nước lọc | おみずを おねがいします。 *(Làm ơn cho tôi xin nước lọc.)* |
| **おちゃ (お茶)** | ocha | Trà | Trà xanh Nhật Bản | あたたかい おちゃを どうぞ。 *(Xin mời bạn dùng trà nóng.)* |
| **りんご** | ringo | — | Quả táo | あかい りんごを みっつ ください。 *(Cho tôi 3 quả táo đỏ.)* |
| **レシート** | reshiito | — | Hóa đơn, biên lai | レシートは けっこうです。 *(Tôi không lấy hóa đơn đâu ạ.)* |
| **おつり** | otsuri | — | Tiền thừa, tiền thối lại | にひゃくえんの おつりです。 *(Tiền thừa thối lại 200 yên.)* |
| **いらっしゃいませ** | irasshaimase | — | Kính chào quý khách! | いらっしゃいませ、なんめいさまですか。 *(Chào quý khách, đi mấy người ạ?)* |
| **かしこまりました** | kashikomarimashita | — | Tôi đã hiểu rõ (kính ngữ phục vụ) | はい、かしこまりました。 *(Vâng, tôi đã tiếp nhận yêu cầu ạ.)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Mẫu câu hỏi giá: `[Danh từ] は いくらですか`
- **Ý nghĩa**: ""[Danh từ] có giá bao nhiêu tiền?"".
- Trợ từ `は` đứng sau danh từ cần hỏi giá.
- Ví dụ:
  - `これは いくらですか。` *(Cái này giá bao nhiêu?)*
  - `このホットコーヒーは いくらですか。` *(Cốc cà phê nóng này giá bao nhiêu?)*

### 4.2. Mẫu câu gọi món & đề nghị: `[Danh từ] を [Số lượng] ください`
- **Ý nghĩa**: ""Làm ơn cho tôi [số lượng] [danh từ]"".
- **Trợ từ tân ngữ `を` (đọc là o)**: Đứng sau món đồ muốn lấy.
- Vị trí của số đếm: Số lượng đồ vật thuần Nhật (`ひとつ, ふたつ...`) thường đặt ngay trước `ください`.
- Ví dụ:
  - `コーヒーを ひとつ ください。` *(Cho tôi 1 cốc cà phê).*
  - `サンドイッチを ふたつと おちゃを ひとつ ください。` *(Cho tôi 2 bánh sandwich và 1 trà).*

### 4.3. Trợ từ liên kết liệt kê `と (to)`
- **Công thức**: `N1 と N2`
- **Ý nghĩa**: ""N1 và N2"".
- Dùng để liệt kê toàn bộ các danh từ ngang hàng với nhau.
- Ví dụ: `パンと ミルクを かいます。` *(Tôi mua bánh mì và sữa).*

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: このは いくらですか (Dùng この đứng một mình trước は)
   ⭕ ĐÚNG: これは いくらですか。 hoặc このかばんは いくらですか。
   💡 GIẢI THÍCH: この bắt buộc phải có danh từ đi liền phía sau, không bao giờ đứng độc lập.

2. ❌ SAI: コーヒーを いち ください (Dùng số đếm thường khi gọi món)
   ⭕ ĐÚNG: コーヒーを ひとつ ください。
   💡 GIẢI THÍCH: Khi gọi món đồ vật, người Nhật luôn dùng bộ số đếm thuần Nhật ひとつ, ふたつ...

3. ❌ SAI: 300 yên đọc là さんひゃくえん
   ⭕ ĐÚNG: さんびゃくえん (sanbyaku en) — biến âm đục びゃ.
   💡 GIẢI THÍCH: 300, 600, 800 có biến âm bắt buộc, đọc sai sẽ khiến người bán khó hiểu.

4. ❌ SAI: Đặt số đếm trước trợ từ を: ひとつを コーヒー ください
   ⭕ ĐÚNG: コーヒーを ひとつ ください。
   💡 GIẢI THÍCH: Trật tự tự nhiên nhất là [Danh từ] + を + [Số lượng] + ください.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Buổi trưa tại một quán cà phê bánh ngọt ấm cúng ở khu phố Shibuya, Tokyo. Bạn Minh vào quầy gọi đồ ăn trưa.

- **Nhân viên**: いらっしゃいませ！ごちゅうもんは おきまりですか。
  - *Romaji*: Irasshaimase! Gochuumon wa okimari desu ka?
  - *Phân tích*: `いらっしゃいませ` (kính chào quý khách) + `ごちゅうもん` (món gọi của quý khách - kính ngữ ご) + `おきまりですか` (đã quyết định xong chưa ạ?).
  - *Dịch*: Xin kính chào quý khách! Quý khách đã chọn được món chưa ạ?

- **Minh**: すみません、この サンドイッチは いくらですか。
  - *Romaji*: Sumimasen, kono sandoicchi wa ikura desu ka?
  - *Phân tích*: `すみません` (xin lỗi) + `この サンドイッチは` (chiếc bánh sandwich này) + `いくらですか` (bao nhiêu tiền?).
  - *Dịch*: Xin lỗi, chiếc bánh sandwich này bao nhiêu tiền vậy ạ?

- **Nhân viên**: さんびゃくごじゅうえんです。
  - *Romaji*: Sanbyaku gojuu en desu.
  - *Phân tích*: `さんびゃく` (300) + `ごじゅう` (50) + `えんです` (yên ạ).
  - *Dịch*: Dạ 350 yên ạ.

- **Minh**: じゃ、サンドイッチを ひとつと、ホットコーヒーを ひとつ ください。
  - *Romaji*: Ja, sandoicchi o hitotsu to, hotto koohii o hitotsu kudasai.
  - *Phân tích*: `じゃ` (vậy thì) + `サンドイッチを ひとつ` (1 sandwich) + `と` (và) + `ホットコーヒーを ひとつ` (1 cà phê nóng) + `ください` (cho tôi xin).
  - *Dịch*: Vậy làm ơn cho tôi 1 bánh sandwich và 1 cà phê nóng nhé.

- **Nhân viên**: かしこまりました。ぜんぶで ななひゃくえんです。
  - *Romaji*: Kashikomarimashita. Zenbu de nanahyaku en desu.
  - *Phân tích*: `かしこまりました` (tôi đã hiểu rõ yêu cầu) + `ぜんぶで` (tổng cộng tất cả) + `ななひゃくえんです` (là 700 yên ạ).
  - *Dịch*: Vâng tôi xin tiếp nhận đơn. Tổng cộng hết 700 yên ạ.

- **Minh**: せんえん おねがいします。
  - *Romaji*: Sen'en onegaishimasu.
  - *Phân tích*: `せんえん` (1.000 yên) + `おねがいします` (xin gửi bạn).
  - *Dịch*: Tôi gửi 1.000 yên nhé.

- **Nhân viên**: はい、せんえん おあずかりします。さんびゃくえんの おつりです。ありがとうございます！
  - *Romaji*: Hai, sen'en oazukari shimasu. Sanbyaku en no otsuri desu. Arigatou gozaimasu!
  - *Phân tích*: `おあずかりします` (tôi xin nhận tạm) + `さんびゃくえんの おつり` (tiền thối lại 300 yên) + `ありがとうございます` (cảm ơn quý khách).
  - *Dịch*: Vâng, tôi nhận 1.000 yên. Tiền thừa của quý khách là 300 yên. Cảm ơn quý khách rất nhiều!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập chỉ định từ & số tiền
1. Điền `これ` hoặc `この` vào chỗ trống:
   - `___ は わたしのかばんです。`
   - `___ シャツは いくらですか。`
2. Viết cách đọc Hiragana cho các mức giá sau: `300 yên`, `600 yên`, `800 yên`, `3.000 yên`.
3. Dịch câu sau sang tiếng Nhật: *""Cho tôi 2 bát mì ramen và 1 ly nước lọc.""*

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - Câu đầu điền **これ** (`これは わたしのかばんです` — Cái này là cặp của tôi).
   - Câu sau điền **この** (`このシャツ` — Áo sơ mi này).
2. **Câu 2**:
   - 300 yên: `さんびゃくえん`
   - 600 yên: `ろっぴゃくえん`
   - 800 yên: `はっぴゃくえん`
   - 3.000 yên: `さんぜんえん`
3. **Câu 3**: `ラーメンを ふたつと おみずを ひとつ ください。`
4. **Câu 4 trắc nghiệm**: Tại sao số 10 trong bộ đếm đồ vật thuần Nhật lại là `とお`?
   - *Đáp án*: Vì đây là chữ số tròn chục duy nhất không có đuôi `つ`, đọc kéo dài trường âm `too`.
5. **Câu 5 trắc nghiệm**: Cụm từ `かしこまりました` thường do ai nói?
   - *Đáp án*: Do nhân viên bán hàng hoặc phục vụ nói với khách hàng để biểu thị sự tiếp nhận yêu cầu một cách cung kính.

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phản xạ nghe-nói ja-JP và lưu từ mới vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Sử dụng linh hoạt これ/それ/あれ và この/その/あの để hỏi giá, mua sắm và gọi món ăn.",
            "Thuộc biến âm giá tiền Yên (さんびゃく, ろっぴゃく, さんぜん) và mẫu câu lịch sự 〜をください."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter3Id, SeedDataInitializer.JapaneseLesson31Id,
            "Bài 3.1: Hỏi đường, Tàu điện & Phương hướng",
            "ja-n5-hoi-duong-tau-dien-phuong-huong", 1, 35,
            @"# Bài 3.1: Hỏi đường, Tàu điện & Phương hướng

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Đi tàu điện và hỏi đường xá là kỹ năng sinh tồn thiết yếu bậc nhất khi đặt chân đến các đô thị sầm uất của Nhật Bản:
- Phân biệt bản chất tuyệt đối giữa hai động từ tồn tại cốt lõi: **あります** (vật vô tri, địa điểm) và **います** (sinh mệnh có ý thức: con người, động vật).
- Làm chủ toàn bộ từ vựng chỉ phương hướng và vị trí không gian: `みぎ (phải)`, `ひだり (trái)`, `まえ (trước)`, `うしろ (sau)`, `となり (bên cạnh)`, `ちかく (gần)`.
- Sử dụng thành thạo trợ từ phương tiện **で (de)** khi di chuyển bằng tàu điện, xe buýt, máy bay, và ghi nhớ ngoại lệ đi bộ (`あるいて`).
- Vận dụng chuẩn xác trợ từ phương hướng **へ (đọc là e)** và **に (ni)** trước các động từ chuyển động: `いきます`, `きます`, `かえります`.
- Tự tin hỏi đường người bản xứ bằng cấu trúc **〜は どこですか** và lắng nghe hướng dẫn chỉ đường bằng thể **〜てください**.
- Hiểu văn hóa ga tàu điện Nhật Bản: mua vé tàu (`きっぷ`), qua cửa soát vé (`かいさつぐち`) và sử dụng thẻ IC thông minh.

---

## 2. 🌱 Nhập môn chậm rãi từng bước: Động từ tồn tại & Phương hướng

### 2.1. Phân biệt sống còn giữa `あります` và `います`
Trong tiếng Việt, ta chỉ dùng một từ ""có"" cho mọi thứ (""Có quyển sách"", ""Có con mèo"", ""Có thầy giáo""). Nhưng trong tiếng Nhật, việc chọn động từ phụ thuộc vào việc chủ thể **có khả năng tự di chuyển bằng ý chí sinh học hay không**:
1. **あります (arimasu - Có / Tồn tại)**:
   - Dùng cho **vật vô tri vô giác**: bàn ghế, sách vở, tiền bạc, điện thoại (`ほんが あります`).
   - Dùng cho **thực vật, cây cỏ, hoa lá**: tuy là sinh vật sống nhưng cây cối không có ý thức tự chuyển động (`はなが あります`).
   - Dùng cho **các địa điểm tĩnh, tòa nhà**: nhà ga, ngân hàng, trường học, bệnh viện (`えきが あります`).
2. **います (imasu - Có / Ở)**:
   - Dùng cho **con người**: bạn bè, thầy cô, gia đình, nhân viên (`ともだちが います`).
   - Dùng cho **động vật, thú cưng, côn trùng**: chó, mèo, chim, cá (`ねこが います`).

### 2.2. Vị trí & Phương hướng trong không gian (Vị trí từ)
Để mô tả đường đi và địa điểm, bạn cần thuộc lòng các cặp từ đối lập sau:
- `みぎ (migi - Bên phải)` ↔ `ひだり (hidari - Bên trái)`
- `まえ (mae - Phía trước)` ↔ `うしろ (ushiro - Phía sau)`
- `うえ (ue - Phía trên)` ↔ `した (shita - Phía dưới)`
- `なか (naka - Bên trong)` ↔ `そと (soto - Bên ngoài)`
- `となり (tonari - Bên cạnh sát vách, cùng chủng loại)`: Ví dụ ngân hàng nằm ngay cạnh hiệu sách (`ほんやの となり`).
- `ちかく (chikaku - Ở gần khu vực)`: Không nhất thiết phải sát vách, chỉ cần ở phạm vi lân cận.
- `まっすぐ (massugu - Đi thẳng)`: Có âm ngắt `っ`, đọc dứt khoát.

### 2.3. Trợ từ phương tiện `で` & Ngoại lệ đi bộ
Khi muốn nói đi bằng phương tiện gì, ta đặt trợ từ **で (de)** ngay sau tên phương tiện:
- `でんしゃで いきます` *(Đi bằng tàu điện)*.
- `バスで いきます` *(Đi bằng xe buýt)*.
- `タクシーで いきます` *(Đi bằng taxi)*.
- **NGOẠI LỆ TỐI QUAN TRỌNG**:
  - Khi **đi bộ**, ta dùng thể Te của động từ đi bộ là **あるいて (aruite)** và **TUYỆT ĐỐI KHÔNG DÙNG TRỢ TỪ で**!
  - Nói `あるきで いきます` là SAI! Bắt buộc phải nói: `あるいて いきます` *(Tôi đi bộ đi)*.

### 2.4. Trợ từ phương hướng `へ` (đọc là e)
- Chữ viết là `へ` (hàng he), nhưng khi đóng vai trò là **trợ từ chỉ hướng đích đến**, bắt buộc phải phát âm là **e**.
- Trợ từ `へ` hướng sự chú ý về phía đích đến của hành trình:
  - `とうきょうへ いきます。` *(Đi về hướng Tokyo).*
  - `いえへ かえります。` *(Về nhà).*

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **えき (駅)** | eki | Dịch | Nhà ga tàu điện | えきは どこですか。 *(Nhà ga ở đâu vậy?)* |
| **でんしゃ (電車)** | densha | Điện Xa | Tàu điện | でんしゃで がっこうへ いきます。 *(Đi học bằng tàu điện.)* |
| **ちかてつ (地下鉄)** | chikatetsu | Địa Hạ Thiết | Tàu điện ngầm | ちかてつに のります。 *(Lên tàu điện ngầm.)* |
| **バス** | basu | — | Xe buýt (Bus) | バスていは どこですか。 *(Trạm xe buýt ở đâu?)* |
| **きっぷ (切符)** | kippu | Thiết Phù | Vé tàu, vé xe | きっぷうりばで きっぷを かいます。 *(Mua vé ở quầy bán vé.)* |
| **ぎんこう (銀行)** | ginkou | Ngân Hàng | Ngân hàng | ぎんこうは えきの まえです。 *(Ngân hàng ở trước ga.)* |
| **びょういん (病院)** | byouin | Bệnh Viện | Bệnh viện | おおきい びょういんへ いきます。 *(Đi đến bệnh viện lớn.)* |
| **コンビニ** | konbini | — | Cửa hàng tiện lợi | コンビニの となりに あります。 *(Nằm cạnh cửa hàng tiện lợi.)* |
| **みぎ (右) / ひだり (左)** | migi / hidari | Hữu / Tả | Bên phải / Bên trái | つぎのかどを みぎへ まがります。 *(Rẽ phải ở góc phố tiếp theo.)* |
| **まっすぐ** | massugu | — | Đi thẳng | このみちを まっすぐ いってください。 *(Hãy đi thẳng con đường này.)* |
| **かど (角)** | kado | Giác | Góc đường, ngã rẽ | かどを ひだりへ まがってください。 *(Hãy rẽ trái ở góc đường.)* |
| **あるいて (歩いて)** | aruite | Bộ | Đi bộ | えきから あるいて ごふんです。 *(Từ ga đi bộ mất 5 phút.)* |
| **どこ** | doko | — | Ở đâu? | トイレは どこですか。 *(Nhà vệ sinh ở đâu ạ?)* |
| **あります** | arimasu | Tại | Có (đồ vật, tòa nhà) | あそこに ゆうびんきょくが あります。 *(Đằng kia có bưu điện.)* |
| **います** | imasu | Cư | Có, ở (người, động vật) | あそこに けいさつかんが います。 *(Đằng kia có chú cảnh sát.)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Mẫu câu hỏi vị trí: `[Địa điểm] は どこですか`
- **Ý nghĩa**: ""[Địa điểm] nằm ở đâu?"".
- Để lịch sự hơn trong giao tiếp với người lạ, người Nhật dùng từ `どちら (dochira)` thay cho `どこ`.
- Ví dụ:
  - `すみません、とうきょうえきは どこですか。` *(Xin lỗi, ga Tokyo ở đâu vậy ạ?)*
  - `おてあらいは どちらですか。` *(Nhà vệ sinh ở phía nào ạ?)*

### 4.2. Mẫu câu chỉ vị trí tồn tại: `N1 は N2 の [Vị trí từ] に あります / います`
- **Ý nghĩa**: ""N1 nằm ở [vị trí] của N2"".
- **Trợ từ `に` (ni)**: Đánh dấu nơi chốn của sự tồn tại tĩnh tại.
- Ví dụ:
  - `ぎんこうは えきの まえに あります。` *(Ngân hàng ở phía trước nhà ga).*
  - `いぬは つくえの したに います。` *(Con chó ở dưới gầm bàn).*

### 4.3. Mẫu câu chỉ đường cầu khiến lịch sự: `〜てください`
- **Công thức**: `[Phương hướng] へ まがってください` *(Hãy rẽ về hướng...)* hoặc `まっすぐ いってください` *(Hãy đi thẳng)*.
- Động từ thể Te + `ください` tạo thành câu chỉ dẫn ân cần, giúp người nghe dễ dàng lần theo lộ trình.

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: あそこに ねこが あります (Dùng あります cho động vật sống)
   ⭕ ĐÚNG: あそこに ねこが います。
   💡 GIẢI THÍCH: Mèo là sinh vật có ý thức di chuyển, bắt buộc phải dùng động từ います.

2. ❌ SAI: わたしは あるきで がっこうへ いきます (Dùng で sau あるき)
   ⭕ ĐÚNG: わたしは あるいて がっこうへ いきます。
   💡 GIẢI THÍCH: Đi bộ dùng thể động từ あるいて, tuyệt đối không dùng trợ từ で.

3. ❌ SAI: Phát âm trợ từ へ là ""he"" trong えきへ いきます
   ⭕ ĐÚNG: えきへ (eki e) いきます。
   💡 GIẢI THÍCH: Khi đóng vai trò trợ từ chỉ phương hướng chuyển động, へ luôn đọc là ""e"".

4. ❌ SAI: Nhầm lẫn となり (sát vách cùng hàng) với ちかく (ở gần khu vực)
   ⭕ ĐÚNG: ぎんこうの となり (Ngay sát cạnh ngân hàng); えきの ちかく (Ở khu vực gần ga).
   💡 GIẢI THÍCH: となり yêu cầu hai vật thể tiếp giáp cạnh nhau và thường cùng tính chất.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Bạn Minh vừa bước ra khỏi một con phố nhỏ ở Shinjuku và bị lạc đường. Minh tiến lại gần một bác người dân địa phương tốt bụng để hỏi đường ra nhà ga.

- **Minh**: すみません、しんじゅくえきは どこですか。
  - *Romaji*: Sumimasen, Shinjuku-eki wa doko desu ka?
  - *Phân tích*: `すみません` (lời mở đầu lịch sự) + `しんじゅくえきは` (ga Shinjuku) + `どこですか` (ở đâu vậy ạ?).
  - *Dịch*: Xin lỗi bác, ga Shinjuku nằm ở đâu vậy ạ?

- **Người dân**: ああ、しんじゅくえきですね。このみちを まっすぐ いってください。
  - *Romaji*: Aa, Shinjuku-eki desu ne. Kono michi o massugu itte kudasai.
  - *Phân tích*: `ああ` (À) + `しんじゅくえきですね` (ga Shinjuku nhỉ) + `このみちを` (con đường này) + `まっすぐ` (thẳng) + `いってください` (hãy đi).
  - *Dịch*: À, ga Shinjuku hả cháu. Cháu cứ đi thẳng con đường này nhé.

- **Minh**: はい、まっすぐですね。
  - *Romaji*: Hai, massugu desu ne.
  - *Phân tích*: `はい` (vâng) + `まっすぐですね` (đi thẳng đúng không ạ).
  - *Dịch*: Dạ vâng, đi thẳng ạ.

- **Người dân**: ええ。そして、つぎの しんごうを ひだりへ まがってください。コンビニの となりに ありますよ。
  - *Romaji*: Ee. Soshite, tsugi no shingou o hidari e magatte kudasai. Konbini no tonari ni arimasu yo.
  - *Phân tích*: `そして` (sau đó) + `つぎの しんごうを` (cột đèn giao thông tiếp theo) + `ひだりへ` (về bên trái) + `まがってください` (hãy rẽ) + `コンビニの となりに` (ngay cạnh cửa hàng tiện lợi) + `ありますよ` (có đấy nhé).
  - *Dịch*: Ừ. Sau đó, ở cột đèn giao thông tiếp theo cháu rẽ trái nhé. Nó nằm ngay cạnh cửa hàng tiện lợi Konbini đấy.

- **Minh**: あるいて なんぷんぐらい かかりますか。
  - *Romaji*: Aruite nanpun gurai kakarimasu ka?
  - *Phân tích*: `あるいて` (đi bộ) + `なんぷんぐらい` (khoảng mấy phút) + `かかりますか` (mất tốn thời gian?).
  - *Dịch*: Đi bộ mất khoảng bao nhiêu phút ạ?

- **Người dân**: ごふんぐらいですよ。ちかいですよ。
  - *Romaji*: Gofun gurai desu yo. Chikai desu yo.
  - *Phân tích*: `ごふんぐらい` (khoảng 5 phút) + `ちかいですよ` (gần lắm đấy).
  - *Dịch*: Khoảng 5 phút thôi cháu, gần lắm!

- **Minh**: どうも ありがとうございました！
  - *Romaji*: Doumo arigatou gozaimashita!
  - *Phân tích*: Dùng quá khứ `gozaimashita` để cảm ơn sự việc đã được giúp đỡ xong xuôi.
  - *Dịch*: Cháu cảm ơn bác rất nhiều ạ!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập phân biệt trợ từ & động từ
1. Chọn `あります` hoặc `います` điền vào chỗ trống:
   - `こうえんに こどもが ___。`
   - `へやに テレビが ___。`
2. Điền trợ từ `で`, `へ`, `に` thích hợp:
   - `わたしは でんしゃ___ とうきょう___ いきます。`
   - `ぎんこうは えきの まえ___ あります。`
3. Hãy viết câu chỉ dẫn: *""Hãy rẽ phải ở ngã rẽ tiếp theo.""*

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - Trẻ em (こども) là con người → điền **います** (`こどもが います`).
   - Tivi (テレビ) là đồ vật vô tri → điền **あります** (`テレビが あります`).
2. **Câu 2**:
   - `でんしゃで` (bằng tàu điện) + `とうきょうへ` (đến Tokyo).
   - `えきの まえに` (ở phía trước nhà ga — tồn tại dùng `に`).
3. **Câu 3**: `つぎのかどを みぎへ まがってください。`
4. **Câu 4 trắc nghiệm**: Khi đi bộ đến trường, cách nói nào đúng?
   - *Đáp án*: `あるいて がっこうへ いきます` (không dùng trợ từ `で` sau `あるいて`).
5. **Câu 5 trắc nghiệm**: Trợ từ `へ` trong câu chỉ phương hướng đọc là gì?
   - *Đáp án*: Đọc là `e` (giống nguyên âm `え`).

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phản xạ nghe-nói ja-JP và lưu từ mới vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Hỏi vị trí địa điểm, phân biệt あります/います và nghe hiểu hướng dẫn đi thẳng, rẽ trái/phải.",
            "Vận dụng trợ từ で (phương tiện), へ/に (hướng đến) và cấu trúc chỉ dẫn 〜てください."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter3Id, SeedDataInitializer.JapaneseLesson32Id,
            "Bài 3.2: Thói quen hằng ngày, Sở thích & Đề nghị lịch sự",
            "ja-n5-thoi-quen-so-thich-de-nghi", 2, 35,
            @"# Bài 3.2: Thói quen hằng ngày, Sở thích & Đề nghị lịch sự

## 1. Mục tiêu bài học (がくしゅうもくひょう)
Đây là bài học tổng kết quan trọng của cấp độ N5, mở ra cánh cửa giao tiếp sinh hoạt và kết nối tình bạn tự nhiên:
- Nắm vững hệ thống chia động từ thể lịch sự **〜ます (Masu)** theo 4 thì: Hiện tại khẳng định (`〜ます`), Hiện tại phủ định (`〜ません`), Quá khứ khẳng định (`〜ました`) và Quá khứ phủ định (`〜ませんでした`).
- Sử dụng chuẩn xác trợ từ tân ngữ trực tiếp **を (đọc là o)** và phân biệt rạch ròi trợ từ địa điểm hành động **で (de)** với trợ từ vị trí tồn tại **に (ni)**.
- Bày tỏ sở thích cá nhân thành thạo qua 2 mẫu câu kinh điển: **[Danh từ] が すきです** và **しゅみは [Động từ thể từ điển] ことです**.
- Làm chủ nghệ thuật mời rủ và giao tiếp tinh tế của người Nhật bằng cấu trúc **〜ませんか** (Bạn có muốn cùng... không?) và hưởng ứng bằng **〜ましょう** (Chúng ta cùng... nhé!).
- Nắm vững các phó từ chỉ tần suất thường nhật: `まいにち`, `いつも`, `よく`, `ときどき`, `あまり + Phủ định`.
- Chỉnh sửa dứt điểm các lỗi dùng nhầm trợ từ `を` trước tính từ `すき` và lỗi quên chia thì quá khứ.

---

## 2. 🌱 Nhập môn chậm rãi từng bước: Hệ thống thì Động từ & Trợ từ cốt lõi

### 2.1. 4 thì cơ bản của Động từ thể lịch sự (〜ます)
Trong tiếng Nhật, động từ ở thể lịch sự (Masu-kei) biến đổi linh hoạt ở đuôi từ để biểu thị thời gian và sắc thái khẳng định hay phủ định:

| Thời / Sắc thái | Đuôi chia | Động từ Ăn (たべます) | Động từ Uống (のみます) | Động từ Học (べんきょうします) |
|---|---|---|---|---|
| **Hiện tại / Tương lai Khẳng định** | **〜ます** | たべます *(ăn / sẽ ăn)* | のみます *(uống / sẽ uống)* | べんきょうします *(học / sẽ học)* |
| **Hiện tại / Tương lai Phủ định** | **〜ません** | たべません *(không ăn)* | のみません *(không uống)* | べんきょうしません *(không học)* |
| **Quá khứ Khẳng định** | **〜ました** | たべました *(đã ăn)* | のみました *(đã uống)* | べんきょうしました *(đã học)* |
| **Quá khứ Phủ định** | **〜ませんでした** | たべませんでした *(đã không ăn)* | のみませんでした *(đã không uống)* | べんきょうしませんでした *(đã không học)* |

**Đặc điểm thời gian**: Tiếng Nhật không có thì tương lai riêng biệt. Thể `〜ます` đảm nhận cả hai chức năng: diễn tả thói quen lặp đi lặp lại ở hiện tại và diễn tả hành động sẽ diễn ra trong tương lai.

### 2.2. Phân biệt sống còn giữa trợ từ `で` và `に`
Hai trợ từ này đều đi sau từ chỉ nơi chốn, nhưng bản chất khác nhau 180 độ:
- **Trợ từ `で` (Địa điểm diễn ra HÀNH ĐỘNG)**: Dùng khi tại địa điểm đó có một **hành động cụ thể, tích cực** đang diễn ra (học bài, ăn cơm, xem phim, đọc sách, làm việc).
  - `としょかんで べんきょうします。` *(Học bài ở thư viện).*
  - `レストランで たべます。` *(Ăn ở nhà hàng).*
- **Trợ từ `に` (Địa điểm của sự TỒN TẠI hoặc ĐÍCH ĐẾN)**: Chỉ sự tồn tại tĩnh tại (`あります/います`) hoặc điểm đến của sự di chuyển (`いきます/きます`).
  - `いえに います。` *(Ở nhà — tĩnh tại).*
  - `がっこうに いきます。` *(Đến trường — đích đến).*

### 2.3. Bày tỏ sở thích: Tại sao dùng `が` mà không dùng `を`?
Trong tiếng Việt ta nói: ""Tôi thích tiếng Nhật"" (tiếng Nhật đóng vai trò như tân ngữ).
Nhưng trong tiếng Nhật, từ `すき (suki - thích)` về bản chất ngữ pháp là một **Tính từ đuôi Na**. Vì nó là tính từ, nên đối tượng gây ra cảm xúc yêu thích đó **BẮT BUỘC phải đi với trợ từ `が`**, tuyệt đối KHÔNG đi với trợ từ `を`!
- `わたしは にほんごが すきです。` *(Tôi thích tiếng Nhật — ĐÚNG).*
- `わたしは にほんごを すきです。` *(SAI NGHIÊM TRỌNG).*

Khi muốn nói sở thích là một hành động cụ thể, ta đổi động từ sang thể từ điển rồi thêm `こと (việc/cái)` để danh từ hóa nó:
- `わたしの しゅみは えいがを みることです。` *(Sở thích của tôi là việc xem phim).*
- `わたしの しゅみは おんがくを きくことです。` *(Sở thích của tôi là nghe nhạc).*

### 2.4. Nghệ thuật mời rủ tinh tế: `〜ませんか` vs `〜ましょう`
Văn hóa Nhật Bản rất sợ việc áp đặt ý kiến lên người khác. Vì vậy khi đưa ra một lời mời, họ sử dụng thể phủ định nghi vấn:
1. **Lời mời lịch sự `〜ませんか` (Masen ka)**:
   - ""Bạn có muốn cùng... không?"".
   - Hỏi bằng thể phủ định để người nghe nếu bận rộn có thể từ chối một cách dễ dàng, tự nhiên mà không cảm thấy áy náy hay thô lỗ.
   - Ví dụ: `いっしょに コーヒーを のみませんか。` *(Bạn có muốn cùng đi uống cà phê không?)*.
2. **Lời rủ đồng thuận hoặc hưởng ứng `〜ましょう` (Mashou)**:
   - ""Chúng ta cùng... nhé!"" / ""Cùng làm thôi nào!"".
   - Dùng khi hai người đã thân thiết, hoặc dùng để đáp lại một cách hào hứng lời mời của đối phương.
   - Ví dụ: `ええ、いいですね！いきましょう！` *(Ừ, hay quá! Đi thôi nào!)*.

---

## 3. Bảng Từ vựng đầy đủ (単語リスト — Tango Risuto)

| Chữ Kana / Kanji | Romaji | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **たべます (食べます)** | tabemasu | Thực | Ăn | あさごはんを たべます。 *(Tôi ăn bữa sáng.)* |
| **のみます (飲みます)** | nomimasu | Ẩm | Uống | おちゃを のみます。 *(Tôi uống trà.)* |
| **みます (見ます)** | mimasu | Kiến | Xem, nhìn | まいばん えいがを みます。 *(Mỗi tối tôi xem phim.)* |
| **ききます (聞きます)** | kikimasu | Văn | Nghe | にほんの おんがくを ききます。 *(Tôi nghe nhạc Nhật.)* |
| **よみます (読みます)** | yomimasu | Độc | Đọc | としょかんで ほんを よみます。 *(Đọc sách ở thư viện.)* |
| **かきます (書きます)** | kakimasu | Thư | Viết | てがみを かきます。 *(Tôi viết thư.)* |
| **べんきょうします** | benkyou shimasu | Miễn Cưỡng | Học tập | まいにち にほんごを べんきょうします。 *(Học tiếng Nhật mỗi ngày.)* |
| **しゅみ (趣味)** | shumi | Thú Vị | Sở thích | あなたの しゅみは なんですか。 *(Sở thích của bạn là gì?)* |
| **スポーツ** | supootsu | — | Thể thao (Sports) | スポーツが だいすきです。 *(Tôi rất thích thể thao.)* |
| **おんがく (音楽)** | ongaku | Âm Nhạc | Âm nhạc | おんがくを きくのが すきです。 *(Tôi thích việc nghe nhạc.)* |
| **えいが (映画)** | eiga | Ánh Họa | Phim ảnh | しゅうまつに えいがを みます。 *(Cuối tuần tôi xem phim.)* |
| **としょかん (図書館)** | toshokan | Đồ Thư Quán | Thư viện | としょかんで べんきょうします。 *(Học ở thư viện.)* |
| **いっしょに** | issho ni | Nhất Tự | Cùng nhau | いっしょに ひるごはんを たべませんか。 *(Cùng ăn trưa nhé?)* |
| **しゅうまつ (週末)** | shuumatsu | Chu Mạt | Cuối tuần | しゅうまつは ひまですか。 *(Cuối tuần bạn có rảnh không?)* |
| **いつも** | itsumo | — | Luôn luôn | いつも ろくじに おきます。 *(Tôi luôn dậy lúc 6 giờ.)* |
| **ときどき** | tokidoki | Thì | Thỉnh thoảng | ときどき カフェへ いきます。 *(Thỉnh thoảng tôi ra quán cà phê.)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Mẫu câu hành động tân ngữ: `N を V-ます`
- **Trợ từ `を` (đọc là o)**: Đánh dấu đối tượng trực tiếp chịu tác động của hành động.
- Trật tự chuẩn: `[Tân ngữ] を [Động từ]`.
- Ví dụ:
  - `みずを のみます。` *(Uống nước).*
  - `にほんごを べんきょうします。` *(Học tiếng Nhật).*

### 4.2. Mẫu câu địa điểm hành động: `[Địa điểm] で N を V-ます`
- **Trợ từ `で` (de)**: Đánh dấu nơi diễn ra hành động.
- Ví dụ:
  - `いえで ばんごはんを たべます。` *(Ăn tối ở nhà).*
  - `としょかんで ほんを よみます。` *(Đọc sách ở thư viện).*

### 4.3. Cấu trúc sở thích: `N が すきです`
- Đối tượng được yêu thích đi kèm trợ từ **が**.
- Mức độ cao hơn dùng `だいすきです` (Rất thích). Phủ định dùng `すきじゃありません` (Không thích).
- Ví dụ:
  - `わたしは にほんりょうりが すきです。` *(Tôi thích ẩm thực Nhật).*
  - `スポーツが だいすきです。` *(Tôi cực kỳ thích thể thao).*

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: わたしは にほんごを すきです (Dùng を trước すき)
   ⭕ ĐÚNG: わたしは にほんごが すきです。
   💡 GIẢI THÍCH: すき là tính từ chỉ cảm xúc, đối tượng đi kèm bắt buộc phải dùng trợ từ が.

2. ❌ SAI: カフェに コーヒーを のみます (Dùng に cho nơi diễn ra hành động uống)
   ⭕ ĐÚNG: カフェで コーヒーを のみます。
   💡 GIẢI THÍCH: Nơi diễn ra hành động tích cực bắt buộc dùng trợ từ で, に chỉ dùng cho sự tồn tại tĩnh.

3. ❌ SAI: きのう えいがを みます (Không chia quá khứ khi có từ chỉ quá khứ きのう)
   ⭕ ĐÚNG: きのう えいがを みました。
   💡 GIẢI THÍCH: Đã xảy ra trong quá khứ thì đuôi động từ phải chia sang 〜ました.

4. ❌ SAI: いっしょに たべますか (Dùng thể khẳng định để mời mọc nghe cộc lốc)
   ⭕ ĐÚNG: いっしょに たべませんか。 (Dùng thể phủ định lịch sự)
   💡 GIẢI THÍCH: Trong văn hóa Nhật, mời bằng 〜ませんか thể hiện sự tôn trọng tối đa sự lựa chọn của đối phương.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (かいわ — Kaiwa)

### Bối cảnh:
Giờ giải lao chiều thứ Sáu tại thư viện trường. Bạn Yuki (bạn học người Nhật) trò chuyện cùng bạn Minh về sở thích và kế hoạch cuối tuần.

- **Yuki**: ミンさんの しゅみは なんですか。
  - *Romaji*: Min-san no shumi wa nan desu ka?
  - *Phân tích*: `ミンさんの` (của Minh) + `しゅみは` (sở thích) + `なんですか` (là cái gì?).
  - *Dịch*: Sở thích của bạn Minh là gì thế?

- **Minh**: わたしの しゅみは えいがを みることです。にほんの おんがくも すきです。
  - *Romaji*: Watashi no shumi wa eiga o miru koto desu. Nihon no ongaku mo suki desu.
  - *Phân tích*: `えいがを みること` (việc xem phim) + `です` + `にほんの おんがくも` (nhạc Nhật cũng) + `すきです` (thích).
  - *Dịch*: Sở thích của mình là xem phim. Mình cũng thích cả âm nhạc Nhật Bản nữa.

- **Yuki**: そうですか！いいですね。まいにち にほんごを べんきょうしますか。
  - *Romaji*: Sou desu ka! Ii desu ne. Mainichi nihongo o benkyou shimasu ka?
  - *Phân tích*: `まいにち` (mỗi ngày) + `にほんごを` (tiếng Nhật) + `べんきょうしますか` (bạn có học không?).
  - *Dịch*: Thế à! Hay quá nhỉ. Hằng ngày bạn đều học tiếng Nhật à?

- **Minh**: はい、まいばん としょかんで にじかん べんきょうします。
  - *Romaji*: Hai, maiban toshokan de nijikan benkyou shimasu.
  - *Phân tích*: `まいばん` (mỗi tối) + `としょかんで` (ở thư viện) + `にじかん` (2 tiếng đồng hồ) + `べんきょうします` (học).
  - *Dịch*: Vâng, mỗi tối mình đều học 2 tiếng ở thư viện.

- **Yuki**: すばらしいですね！しゅうまつに いっしょに ラーメンを たべませんか。
  - *Romaji*: Subarashii desu ne! Shuumatsu ni issho ni raamen o tabemasen ka?
  - *Phân tích*: `しゅうまつに` (vào cuối tuần) + `いっしょに` (cùng nhau) + `ラーメンを` (mì ramen) + `たべませんか` (cùng đi ăn không?).
  - *Dịch*: Tuyệt vời thật đấy! Cuối tuần này bạn có muốn cùng mình đi ăn mì ramen không?

- **Minh**: ええ、いいですね！ぜひ いきましょう！
  - *Romaji*: Ee, ii desu ne! Zehi ikimashou!
  - *Phân tích*: `ええ` (Ừ) + `いいですね` (hay quá) + `ぜひ` (nhất định) + `いきましょう` (chúng ta cùng đi nào!).
  - *Dịch*: Ôi hay quá! Nhất định chúng mình cùng đi nhé!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập chia động từ & trợ từ
1. Chia động từ trong ngoặc sang dạng đúng:
   - `きのうの ばん、わたしは テレビを (みます → ___ )。`
   - `あした ともだちと えいがを (みます → ___ )。`
2. Điền trợ từ `を`, `で`, `が` vào chỗ trống:
   - `カフェ___ コーヒー___ のみます。`
   - `わたしは アニメ___ すきです。`
3. Hãy đặt một câu mời bạn đi uống trà vào Chủ nhật bằng cấu trúc `〜ませんか`.

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng N5)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - `きのう` (hôm qua) → chia quá khứ: **みました** (`テレビを みました`).
   - `あした` (ngày mai) → chia tương lai khẳng định: **みます** (`えいがを みます`).
2. **Câu 2**:
   - `カフェで` (địa điểm hành động dùng `で`) + `コーヒーを` (tân ngữ dùng `を`).
   - `アニメが` (sở thích đi với tính từ `すき` dùng `が`).
3. **Câu 3**: `にちようびに いっしょに おちゃを のみませんか。`
4. **Câu 4 trắc nghiệm**: Khác biệt giữa `〜ませんか` và `〜ましょう` là gì?
   - *Đáp án*: `〜ませんか` là lời mời lịch sự tạo cơ hội cho đối phương dễ lựa chọn, còn `〜ましょう` là lời rủ hào hứng hoặc hưởng ứng lời mời.
5. **Câu 5 trắc nghiệm**: Tại sao nói `カフェに コーヒーを のみます` là sai?
   - *Đáp án*: Vì uống cà phê là một hành động cụ thể, nơi diễn ra hành động bắt buộc phải dùng trợ từ `で`.

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phản xạ nghe-nói ja-JP và lưu từ mới vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Chia động từ thể ます/ません/ました, dùng đúng trợ từ で cho địa điểm hành động và が cho sở thích.",
            "Tự tin trò chuyện về thói quen hằng ngày và mời rủ lịch sự bằng 〜ませんか / 〜ましょう.")
    };


    private sealed record CjkQuizQuestion(
        string Question,
        string OptionA,
        string OptionB,
        string OptionC,
        string OptionD,
        string CorrectKey,
        string Explanation
    );

    private static (PracticeQuiz Quiz, List<PracticeQuizItem> Items) BuildCjkQuiz(
        string quizKey,
        Guid lessonId,
        string title,
        DateTimeOffset now,
        CjkQuizQuestion[] questions)
    {
        var quizId = CreateDeterministicQuizGuid(quizKey);
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

        var items = new List<PracticeQuizItem>(questions.Length);
        for (int i = 0; i < questions.Length; i++)
        {
            var q = questions[i];
            items.Add(new PracticeQuizItem
            {
                Id = CreateDeterministicQuizGuid($"{quizKey}:item:{i + 1}"),
                QuizId = quizId,
                ItemType = "multiple_choice",
                PromptAstJson = JsonSerializer.Serialize(new { question = q.Question }, CjkJsonOptions),
                ChoicesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "A", text = q.OptionA },
                    new { key = "B", text = q.OptionB },
                    new { key = "C", text = q.OptionC },
                    new { key = "D", text = q.OptionD }
                }, CjkJsonOptions),
                ClozeSolution = q.CorrectKey,
                AiRubricFeedbackJson = JsonSerializer.Serialize(new { summary = q.Explanation }, CjkJsonOptions),
                CreatedAt = now.AddSeconds(i)
            });
        }

        return (quiz, items);
    }

    private static Guid CreateDeterministicQuizGuid(string seedKey)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seedKey));
        var bytes = new byte[16];
        Array.Copy(hash, bytes, 16);
        return new Guid(bytes);
    }

    private static async Task EnsureCjkQuizzesSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var quizzes = new List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)>();
        quizzes.AddRange(GetJapaneseQuizzes(now));
        quizzes.AddRange(GetChineseQuizzes(now));

        foreach (var (quiz, items) in quizzes)
        {
            var existing = await db.PracticeQuizzes.FirstOrDefaultAsync(q =>
                q.Id == quiz.Id ||
                (q.UserId == SeedDataInitializer.DefaultUserId &&
                 q.LessonId == quiz.LessonId &&
                 q.Title == quiz.Title));

            if (existing == null)
            {
                db.PracticeQuizzes.Add(quiz);
                var existingItemIds = await db.PracticeQuizItems
                    .Where(i => items.Select(x => x.Id).Contains(i.Id))
                    .Select(i => i.Id)
                    .ToListAsync();
                var itemsToAdd = items.Where(it => !existingItemIds.Contains(it.Id)).ToList();
                if (itemsToAdd.Count > 0)
                {
                    db.PracticeQuizItems.AddRange(itemsToAdd);
                }
            }
            else
            {
                existing.LessonId = quiz.LessonId;
                existing.Title = quiz.Title;
                existing.TotalItems = quiz.TotalItems;

                var existingItemIds = await db.PracticeQuizItems
                    .Where(i => i.QuizId == existing.Id)
                    .Select(i => i.Id)
                    .ToListAsync();

                var missingItems = items.Where(it => !existingItemIds.Contains(it.Id)).ToList();
                if (missingItems.Count > 0)
                {
                    foreach (var it in missingItems)
                    {
                        it.QuizId = existing.Id;
                    }
                    db.PracticeQuizItems.AddRange(missingItems);
                }
            }
        }
        await db.SaveChangesAsync();
    }

    // =========================================================================
    // KHÓA 1: TIẾNG NHẬT SƠ CẤP (N5) — 7 ĐỀ THI THỬ (70 CÂU HỎI MULTIPLE CHOICE)
    // =========================================================================

    private static List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)> GetJapaneseQuizzes(DateTimeOffset now)
    {
        return new List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)>
        {
            BuildCjkQuiz("cjk-quiz:ja:l11", SeedDataInitializer.JapaneseLesson11Id,
                "Bài 1.1: Trắc nghiệm Hiragana & Chào hỏi N5", now,
                new CjkQuizQuestion[]
                {
                    new("Chữ cái Hiragana \"あ\" được phát âm chuẩn là gì?", "a", "o", "e", "u", "A", "あ là chữ cái đầu tiên trong hàng nguyên âm Gojūon của tiếng Nhật, phát âm là 'a'."),
                    new("Chữ cái Hiragana \"し\" trong hàng Sa được phát âm theo chuẩn Hepburn là gì?", "si", "shi", "chi", "su", "B", "し đọc là 'shi' (phát âm êm, đẩy hơi nhẹ qua kẽ răng, không chu môi)."),
                    new("Chữ Hán \"私\" trong câu \"私はベトナム人です\" có cách đọc Hiragana là gì?", "わたし", "あなた", "かれ", "だれ", "A", "私 (âm Hán Việt: Tư) có cách đọc thuần Nhật là わたし (watashi - tôi)."),
                    new("Lời chào buổi sáng lịch sự, trang trọng dùng cho thầy cô và cấp trên là gì?", "おはようございます", "こんにちは", "こんばんは", "おやすみなさい", "A", "おはようございます là lời chào buổi sáng trang trọng đầy đủ kính ngữ."),
                    new("Khi muốn nói \"Cảm ơn bạn rất nhiều\" một cách chân thành và lịch thiệp, người Nhật nói gì?", "どういたしまして", "ありがとうございます", "すみません", "ごめんなさい", "B", "ありがとうございます (hoặc どうもありがとうございます) là mẫu câu cảm ơn chuẩn mực."),
                    new("Từ nào sau đây có chứa trường âm (kéo dài 2 phách) hàng O?", "さくら", "おはよう", "あさ", "はな", "B", "Trong おはよう, đuôi おう là trường âm của hàng O, phát âm kéo dài 2 phách là ohayō."),
                    new("Điền trợ từ đánh dấu chủ đề (đọc là wa) vào chỗ trống: \"わたし___ ミンです。\"", "は", "が", "を", "に", "A", "Trợ từ đánh dấu chủ đề viết bằng chữ Hiragana は nhưng luôn phát âm là 'wa'."),
                    new("Đuôi câu lịch sự khẳng định chuẩn N5 đứng ở vị trí cuối câu danh từ là gì?", "だ", "です", "ます", "でした", "B", "です (desu) là đuôi câu lịch sự khẳng định đi sau danh từ và tính từ đuôi Na."),
                    new("Tanaka hỏi: \"おげんきですか。\" Minh nên đáp lại lịch sự như thế nào?", "はい、げんきです。", "いいえ、がくせいです。", "こんにちは。", "またあした。", "A", "Khi được hỏi thăm sức khỏe (Ogenki desu ka?), câu đáp lại lịch sự là はい、げんきです (Vâng, tôi khỏe)."),
                    new("Trước khi ra về vào cuối ngày làm việc hoặc chia tay đồng nghiệp, câu nói chuẩn nhất là gì?", "おつかれさまでした。", "おはようございます。", "いただきます。", "ごちそうさまでした。", "A", "おつかれさまでした (Bạn đã vất vả rồi) là câu chào kinh điển khi kết thúc ngày làm việc tại Nhật.")
                }),

            BuildCjkQuiz("cjk-quiz:ja:l12", SeedDataInitializer.JapaneseLesson12Id,
                "Bài 1.2: Trắc nghiệm Katakana, Từ mượn & Giới thiệu bản thân", now,
                new CjkQuizQuestion[]
                {
                    new("Chữ Katakana nào sau đây đọc là \"コ\" (ko)?", "コ", "ロ", "ユ", "ヨ", "A", "コ là chữ ko trong hàng Ka Katakana. ロ là ro, ユ là yu, ヨ là yo."),
                    new("Phân biệt cặp chữ dễ nhầm: Chữ nào sau đây là \"シ\" (shi) với các nét vuốt từ dưới lên?", "シ", "ツ", "ソ", "ン", "A", "シ (shi) có nét vuốt từ dưới chếch lên; còn ツ (tsu) có nét chém từ trên xuống."),
                    new("Dấu gạch ngang \"ー\" trong từ Katakana \"コーヒー\" (cà phê) có vai trò gì?", "Biểu thị trường âm kéo dài 1 phách", "Biểu thị âm ngắt dừng hơi", "Biểu thị biến âm đục", "Biểu thị trọng âm cao", "A", "Dấu gạch ngang ー (Chōonpu) trong Katakana dùng để kéo dài nguyên âm đứng trước đúng 1 phách."),
                    new("Tên quốc gia \"Việt Nam\" được viết bằng Katakana chuẩn là gì?", "ベトナム", "バトナム", "ベトナン", "ベトマム", "A", "Việt Nam phiên âm sang Katakana chuẩn là ベトナム (Betonamu)."),
                    new("Từ Katakana nào có nghĩa là \"kỹ sư\" (Engineer)?", "がくせい", "かいしゃいん", "エンジニア", "せんせい", "C", "エンジニア (Enjinia) là từ mượn tiếng Anh có nghĩa là kỹ sư."),
                    new("Cụm từ mở đầu một bài tự giới thiệu bản thân (Lần đầu gặp mặt) là gì?", "はじめまして", "よろしく", "どうぞ", "こんばんは", "A", "はじめまして (Hajimemashite - Sơ) là lời chào trang trọng khi lần đầu tiên gặp mặt ai đó."),
                    new("Điền trợ từ nối hai danh từ biểu thị quan hệ sở hữu/thuộc về: \"わたし___ パソコンです。\"", "は", "の", "も", "と", "B", "Trợ từ の nối hai danh từ (A の B = B của A; わたしの パソコン = Máy tính của tôi)."),
                    new("Để diễn đạt \"Tôi cũng là sinh viên\", ta thay thế trợ từ は bằng trợ từ nào?", "の", "も", "で", "に", "B", "Trợ từ も mang nghĩa 'cũng', thay thế trực tiếp cho trợ từ は khi thông tin câu sau tương đồng câu trước."),
                    new("Sato: \"ミンさんは がくせいですか。\" Minh là sinh viên, Minh trả lời thế nào?", "はい、がくせいです。", "いいえ、がくせいではありません。", "はい、せんせいです。", "いいえ、そうです。", "A", "Khẳng định đúng là sinh viên thì trả lời: はい、がくせいです (Vâng, tôi là sinh viên)."),
                    new("Minh: \"どうぞ よろしく おねがいします。\" Sato đáp lại thế nào để thể hiện sự tương hỗ lịch sự?", "こちらこそ、よろしくおねがいします。", "いいえ、ちがいます。", "さようなら。", "どういたしまして。", "A", "こちらこそ、よろしくおねがいします (Chính tôi mới là người cần sự giúp đỡ của bạn) là câu đáp lễ lịch sự tuyệt đối.")
                }),

            BuildCjkQuiz("cjk-quiz:ja:l21", SeedDataInitializer.JapaneseLesson21Id,
                "Bài 2.1: Trắc nghiệm Số đếm, Giờ giấc & Lịch hẹn N5", now,
                new CjkQuizQuestion[]
                {
                    new("Số 4 khi đi với đơn vị giờ (〜じ) có biến âm bất quy tắc đọc là gì?", "しじ", "よじ", "よんじ", "ななじ", "B", "4 giờ bắt buộc đọc là よじ (yoji), tuyệt đối không đọc là しじ hay よんじ."),
                    new("Số 9 khi đi với đơn vị giờ (〜じ) có biến âm bất quy tắc đọc là gì?", "きゅうじ", "くじ", "ここのじ", "はちじ", "B", "9 giờ bắt buộc đọc là くじ (kuji), không đọc là きゅうじ."),
                    new("\"1 phút\" trong tiếng Nhật có biến âm ngắt đọc là gì?", "いちふん", "いっぷん", "いちぷん", "いつふん", "B", "1 phút có biến âm ngắt っ + ぷ: いっぷん (ippun)."),
                    new("\"6 giờ rưỡi sáng\" trong tiếng Nhật được diễn đạt là gì?", "ごご ろくじはん", "ごぜん ろくじはん", "ごぜん しちじはん", "ごご ろくじじゅっぷん", "B", "ごぜん (buổi sáng AM) + ろくじはん (6 giờ rưỡi) = ごぜん ろくじはん."),
                    new("Từ vựng nào sau đây chỉ \"Hôm nay\"?", "きのう", "きょう", "あした", "あさって", "B", "きょう (今日) là hôm nay. きのう là hôm qua, あした là ngày mai."),
                    new("Thứ Tư trong tuần gắn với hành Thủy, Kanji là \"水曜日\", đọc là gì?", "げつようび", "かようび", "すいようび", "もくようび", "C", "水曜日 đọc là すいようび (suiyoubi - thứ Tư)."),
                    new("Điền trợ từ đi sau mốc thời gian cụ thể: \"まいあさ しちじ___ おきます。\"", "に", "で", "を", "から", "A", "Trợ từ に gắn ngay sau mốc thời gian có con số cụ thể (しちじに = lúc 7 giờ)."),
                    new("Cấu trúc nào diễn đạt khoảng thời gian \"từ 9 giờ đến 5 giờ\"?", "くじから ごじまで", "くじまで ごじから", "くじに ごじで", "くじへ ごじを", "A", "Cấu trúc 〜から (từ) 〜まで (đến) = くじから ごじまで."),
                    new("A: \"いま なんじですか。\" B: \"ごご にじじゅっぷんです。\" Bây giờ là mấy giờ?", "2 giờ 10 phút chiều", "2 giờ 10 phút sáng", "12 giờ 20 phút trưa", "2 giờ 50 phút chiều", "A", "ごご (chiều PM) + にじ (2 giờ) + じゅっぷん (10 phút) = 2 giờ 10 phút chiều."),
                    new("Thông báo: \"かいぎは じゅうじから じゅういちじはんまでです。\" Cuộc họp diễn ra trong bao lâu?", "1 tiếng", "1 tiếng 30 phút", "2 tiếng", "45 phút", "B", "Từ 10:00 (じゅうじ) đến 11:30 (じゅういちじはん) kéo dài đúng 1 tiếng 30 phút.")
                }),

            BuildCjkQuiz("cjk-quiz:ja:l22", SeedDataInitializer.JapaneseLesson22Id,
                "Bài 2.2: Trắc nghiệm Mua sắm, Hỏi giá & Gọi món N5", now,
                new CjkQuizQuestion[]
                {
                    new("Đại từ chỉ định độc lập chỉ vật ở gần người nghe nhưng xa người nói là gì?", "これ", "それ", "あれ", "どれ", "B", "それ (sore) chỉ vật ở gần người nghe. これ gần người nói, あれ xa cả hai người."),
                    new("Từ hạn định nào BẮT BUỘC phải đi kèm danh từ ngay phía sau (không đứng một mình)?", "これ", "この", "ここ", "どれ", "B", "この (kono) là liên thể từ, bắt buộc đi kèm danh từ (ví dụ: このほん, このシャツ)."),
                    new("Khi gọi \"3 cái bánh sandwich\", ta dùng từ đếm đồ vật thuần Nhật nào?", "さん", "みっつ", "ひとつ", "ふたつ", "B", "みっつ (mittsu) là số đếm đồ vật thuần Nhật chỉ số lượng 3 cái."),
                    new("Số tiền \"300 yên\" có biến âm đục, đọc chuẩn là gì?", "さんひゃくえん", "さんびゃくえん", "さんぴゃくえん", "さんぜんえん", "B", "300 yên có biến âm đục: さんびゃくえん (sanbyaku en)."),
                    new("Câu nói chào đón khách quen thuộc nhất khi bước chân vào cửa hàng Nhật là gì?", "いらっしゃいませ", "かしこまりました", "ごちそうさまでした", "おねがいします", "A", "いらっしゃいませ (Irasshaimase) là câu chào kính cẩn đón khách của mọi cửa hàng Nhật."),
                    new("Mẫu câu chuẩn để hỏi giá \"Cái này bao nhiêu tiền?\" là gì?", "これは なんじですか", "これは いくらですか", "これは だれですか", "これは どこですか", "B", "いくら (ikura) là từ để hỏi giá tiền: これは いくらですか."),
                    new("Điền trợ từ tân ngữ vào câu gọi món: \"ホットコーヒー___ ひとつ ください。\"", "は", "を", "に", "で", "B", "Trợ từ を đánh dấu món đồ cần lấy trước động từ ください (ホットコーヒーを ひとつ ください)."),
                    new("Trợ từ \"と\" trong câu \"パンと ミルクを ください\" có ý nghĩa gì?", "Liệt kê toàn bộ: và, cùng với", "Chỉ phương tiện", "Chỉ nơi chốn", "Chỉ thời gian", "A", "Trợ từ と dùng để nối hai danh từ ngang hàng biểu thị sự liệt kê (A và B)."),
                    new("Khách: \"このリンゴは いくらですか。\" Người bán: \"ひとつ ひゃくごじゅうえんです。\" Mua 2 quả táo hết bao nhiêu tiền?", "150 yên", "200 yên", "300 yên", "450 yên", "C", "1 quả giá 150 yên (ひゃくごじゅうえん), mua 2 quả (ふたつ) = 150 x 2 = 300 yên (さんびゃくえん)."),
                    new("Khách: \"コーヒーを ふたつと ケーキを ひとつ ください。\" Khách đã gọi những gì?", "2 cà phê và 1 bánh ngọt", "1 cà phê và 2 bánh ngọt", "2 trà và 1 bánh sandwich", "3 cà phê", "A", "コーヒーを ふたつ (2 cà phê) + ケーキを ひとつ (1 bánh ngọt).")
                }),

            BuildCjkQuiz("cjk-quiz:ja:l31", SeedDataInitializer.JapaneseLesson31Id,
                "Bài 3.1: Trắc nghiệm Hỏi đường, Phương hướng & Di chuyển N5", now,
                new CjkQuizQuestion[]
                {
                    new("Động từ \"あります\" (có / tồn tại) được dùng cho đối tượng nào sau đây?", "Con người", "Động vật sống (chó, mèo)", "Đồ vật vô tri và địa điểm tĩnh (nhà ga, ngân hàng)", "Cả người và động vật", "C", "あります dùng cho đồ vật vô tri, thực vật và công trình địa điểm tĩnh. Người và động vật dùng います."),
                    new("Chữ Kanji \"右\" (Hữu) và \"左\" (Tả) có cách đọc tương ứng là gì?", "みぎ (phải) và ひだり (trái)", "ひだり (phải) và みぎ (trái)", "まえ (trước) và うしろ (sau)", "うえ (trên) và した (dưới)", "A", "右 đọc là みぎ (migi - phải), 左 đọc là ひだり (hidari - trái)."),
                    new("Từ vựng nào sau đây có nghĩa là \"đi thẳng\"?", "まっすぐ", "かど", "となり", "ちかく", "A", "まっすぐ (massugu) nghĩa là đi thẳng. かど là góc phố, となり là bên cạnh."),
                    new("Từ nào có nghĩa là \"nhà ga tàu điện\"?", "びょういん", "ぎんこう", "えき", "ゆうびんきょく", "C", "えき (駅) là nhà ga tàu điện. びょういん là bệnh viện, ぎんこう là ngân hàng."),
                    new("Phương tiện \"tàu điện\" trong tiếng Nhật đọc là gì?", "でんしゃ", "バス", "ひこうき", "じてんしゃ", "A", "でんしゃ (電車) là tàu điện."),
                    new("Khi muốn nói \"đi bộ đến trường\", cách diễn đạt chuẩn xác là gì?", "あるきで いきます", "あるいて いきます", "あるくに いきます", "あるくを いきます", "B", "Đi bộ dùng thể Te của động từ là あるいて いきます, tuyệt đối không dùng trợ từ で!"),
                    new("Điền trợ từ chỉ phương tiện di chuyển: \"わたしは でんしゃ___ かいしゃへ いきます。\"", "で", "に", "を", "は", "A", "Trợ từ で đứng sau phương tiện giao thông (でんしゃで = đi bằng tàu điện)."),
                    new("Điền trợ từ chỉ hướng đích đến (phát âm là e): \"あした とうきょう___ いきます。\"", "へ", "で", "を", "から", "A", "Trợ từ へ (đọc là e) chỉ hướng di chuyển đến đích (とうきょうへ いきます)."),
                    new("Người hỏi: \"ぎんこうは どこですか。\" Người chỉ đường: \"えきの まえに あります。\" Ngân hàng nằm ở đâu?", "Phía sau nhà ga", "Phía trước nhà ga", "Bên cạnh nhà ga", "Trong nhà ga", "B", "えきの まえ (trước nhà ga) + に あります = Nằm ở phía trước nhà ga."),
                    new("Chỉ dẫn: \"つぎのかどを みぎへ まがってください。\" Người nghe cần thao tác gì ở góc rẽ tiếp theo?", "Rẽ trái", "Rẽ phải", "Đi thẳng", "Dừng lại", "B", "みぎへ まがってください có nghĩa là hãy rẽ về bên phải.")
                }),

            BuildCjkQuiz("cjk-quiz:ja:l32", SeedDataInitializer.JapaneseLesson32Id,
                "Bài 3.2: Trắc nghiệm Thói quen, Sở thích & Đề nghị N5", now,
                new CjkQuizQuestion[]
                {
                    new("Động từ \"たべます\" (ăn) khi chuyển sang thì quá khứ khẳng định sẽ biến đổi thành gì?", "たべません", "たべました", "たべませんでした", "たべる", "B", "Quá khứ khẳng định của thể ます là 〜ました (たべました = đã ăn)."),
                    new("Động từ \"のみます\" (uống) khi chuyển sang thì hiện tại phủ định là gì?", "のみません", "のみました", "のみませんでした", "のむ", "A", "Hiện tại phủ định của thể ます là 〜ません (のみません = không uống)."),
                    new("Trong câu \"わたしは おんがく___ すきです\", trợ từ nào đi cùng tính từ すき?", "を", "が", "で", "に", "B", "すき là tính từ biểu thị cảm xúc, đối tượng được yêu thích bắt buộc phải đi với trợ từ が."),
                    new("Từ nào sau đây có nghĩa là \"sở thích\" trong tiếng Nhật?", "しゅみ", "しごと", "べんきょう", "りょこう", "A", "しゅみ (趣味) có nghĩa là sở thích."),
                    new("Từ chỉ tần suất \"Mỗi tối\" trong tiếng Nhật đọc là gì?", "まいあさ", "まいにち", "まいばん", "こんばん", "C", "まいばん (毎晩) nghĩa là mỗi tối. まいあさ là mỗi sáng, まいにち là mỗi ngày."),
                    new("Mẫu câu nào dùng để đưa ra lời mời rủ lịch sự: \"Bạn có muốn cùng đi ăn ramen không?\"", "ラーメンを たべましょう", "ラーメンを たべませんか", "ラーメンを たべますか", "ラーメンを たべてください", "B", "〜ませんか là mẫu câu mời mọc lịch sự, tôn trọng sự lựa chọn của đối phương."),
                    new("Điền trợ từ đánh dấu nơi diễn ra hành động: \"としょかん___ ほんを よみます。\"", "で", "に", "へ", "を", "A", "Trợ từ で đánh dấu nơi diễn ra hành động tích cực (としょかんで = tại thư viện)."),
                    new("Điền trợ từ tân ngữ chịu tác động trực tiếp của hành động: \"あさごはん___ たべます。\"", "を", "は", "が", "で", "A", "Trợ từ を (đọc là o) đánh dấu tân ngữ trực tiếp trước động từ ngoại động từ."),
                    new("Yuki: \"しゅうまつに いっしょに えいがを みませんか。\" Minh: \"ええ、いいですね！いきましょう！\" Minh có thái độ thế nào?", "Đồng ý hào hứng tham gia", "Từ chối khéo léo vì bận", "Hẹn sang tuần sau", "Minh muốn ở nhà ngủ", "A", "ええ、いいですね！いきましょう！ thể hiện sự đồng thuận hào hứng hưởng ứng lời mời."),
                    new("Câu: \"きのうの ばん、カフェで ともだちと にほんごを べんきょうしました。\" Diễn ra khi nào và ở đâu?", "Sáng nay ở thư viện", "Tối qua ở quán cà phê", "Trưa nay ở nhà", "Tối mai ở trường học", "B", "きのうの ばん (tối hôm qua) + カフェで (ở quán cà phê).")
                }),

            BuildCjkQuiz("cjk-quiz:ja:final-n5", SeedDataInitializer.JapaneseLesson32Id,
                "Đề thi thử Tổng kết Năng lực Tiếng Nhật JLPT N5", now,
                new CjkQuizQuestion[]
                {
                    new("[JLPT N5 Kanji] Chữ Hán \"日本\" trong câu \"にほんへ いきます\" có cách đọc là gì?", "にほん", "にっぽん", "にほんご", "にちほん", "A", "日本 có cách đọc thông dụng là にほん (Nihon - Nhật Bản)."),
                    new("[JLPT N5 Kanji] Cách đọc Hiragana chuẩn của chữ Hán \"先生\" là gì?", "がくせい", "せんせい", "いしゃ", "かいしゃ", "B", "先生 (Tiên sinh) đọc là せんせい (sensei - thầy/cô giáo)."),
                    new("[JLPT N5 Katakana] Từ mượn \"パソコン\" là viết tắt của cụm từ tiếng Anh nào?", "Personal Computer (Máy tính cá nhân)", "Passport (Hộ chiếu)", "Party (Bữa tiệc)", "Package (Gói hàng)", "A", "パソコン (Pasokon) là cách viết tắt độc đáo của người Nhật từ cụm Personal Computer."),
                    new("[JLPT N5 Lượng từ] Khi đếm 2 chiếc vé tàu (vật mỏng dẹt), ta dùng đơn vị đếm nào?", "にまい", "ふたつ", "にほん", "にさつ", "A", "Đồ vật mỏng, phẳng (vé tàu, giấy, áo sơ mi) đếm bằng lượng từ 〜まい (枚): にまい (2 chiếc)."),
                    new("[JLPT N5 Biến âm] Mức tiền \"8.000 yên\" có biến âm ngắt đọc là gì?", "はちせんえん", "はっせんえん", "はっぴゃくえん", "はちひゃくえん", "B", "8.000 yên có biến âm ngắt: はっせんえん (hassen en)."),
                    new("[JLPT N5 Từ vựng] Từ chỉ thời gian mang nghĩa \"Hôm qua\" là gì?", "きょう", "きのう", "あした", "おととい", "B", "きのう (昨日) nghĩa là hôm qua. おととい là hôm kia."),
                    new("[JLPT N5 Ngữ pháp] Chọn cặp trợ từ đúng: \"わたしは まいにち しちじ___ おきて、がっこう___ いきます。\"", "に / へ", "で / を", "を / に", "は / で", "A", "Thời gian có số cụ thể dùng に (しちじに); hướng đích đến dùng へ (がっこうへ)."),
                    new("[JLPT N5 Ngữ pháp] Chia động từ đúng thì: \"きのう カフェで えいがを ____。\"", "みます", "みました", "みません", "みること", "B", "Có từ chỉ quá khứ きのう (hôm qua) nên động từ phải chia quá khứ khẳng định: みました."),
                    new("[JLPT N5 Đọc hiểu] \"わたしは ミンです。まいあさ ろくじはんにおきます。あさごはんを たべてから、しちじに バスで がっこうへ いきます。\" Minh đi đến trường bằng phương tiện gì và lúc mấy giờ?", "Đi tàu điện lúc 6:30", "Đi xe buýt lúc 7:00", "Đi bộ lúc 8:00", "Đi xe đạp lúc 6:30", "B", "Trong bài viết: しちじに バスで がっこうへ いきます (Lúc 7:00 đi xe buýt đến trường)."),
                    new("[JLPT N5 Đọc hiểu] Khách mua sách: \"すみません、このほんを ください。\" Nhân viên: \"はい、せんごひゃくえんです。\" Khách đưa 2.000 yên, nhân viên thối lại bao nhiêu tiền?", "500 yên", "1.500 yên", "200 yên", "1.000 yên", "A", "Sách giá 1.500 yên (せんごひゃくえん), khách đưa 2.000 yên (にせんえん) thì tiền thối (おつり) là 500 yên (ごひゃくえん).")
                })

        };
    }

    // =========================================================================

    private static List<PracticeActivity> GetJapaneseActivities(Guid cId, DateTimeOffset now)
    {
        var all = new List<PracticeActivity>();

        // ---------------- LESSON 1.1 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.JapaneseLesson11Id, "ja-JP", "Nhật N5 · Bài 1.1",
            shadowing: new[]
            {
                new ShItem("ja-l11-sh-1", "easy", "Chào buổi sáng (Dễ)", "おはようございます。", "おはよう ございます / ohayou gozaimasu", "Chào buổi sáng ạ (lịch sự).", new[] { "おはよう", "ございます" }),
                new ShItem("ja-l11-sh-2", "easy", "Chào buổi trưa (Dễ)", "こんにちは、おげんきですか。", "こんにちは、おげんき ですか / konnichiwa, ogenki desu ka", "Xin chào, bạn có khỏe không?", new[] { "こんにちは", "げんき" }),
                new ShItem("ja-l11-sh-3", "easy", "Cảm ơn (Dễ)", "ありがとうございます。", "ありがとう ございます / arigatou gozaimasu", "Xin cảm ơn rất nhiều ạ.", new[] { "ありがとう", "ございます" }),
                new ShItem("ja-l11-sh-4", "medium", "Chào buổi tối (Vừa)", "こんばんは、きょうもがんばりましょう。", "こんばんは、きょう も がんばりましょう / konbanwa, kyou mo ganbarimashou", "Chào buổi tối, hôm nay cũng cùng cố gắng nhé.", new[] { "こんばんは", "きょう" }),
                new ShItem("ja-l11-sh-5", "medium", "Xin lỗi đợi chút (Vừa)", "すみません、ちょっとまってください。", "すみません、ちょっと まって ください / sumimasen, chotto matte kudasai", "Xin lỗi, vui lòng đợi một chút ạ.", new[] { "すみません", "ちょっと", "まって" }),
                new ShItem("ja-l11-sh-6", "medium", "Không có chi (Vừa)", "いいえ、どういたしまして。", "いいえ、どういたしまして / iie, dou itashimashite", "Không có gì đâu ạ.", new[] { "いいえ", "どういたしまして" }),
                new ShItem("ja-l11-sh-7", "hard", "Hỏi thăm sức khỏe (Khó)", "おかげさまで、とてもげんきです。", "おかげさまで、とても げんき です / okagesama de, totemo genki desu", "Nhờ ơn bạn, tôi rất khỏe ạ.", new[] { "おかげさまで", "げんき" }),
                new ShItem("ja-l11-sh-8", "hard", "Tạm biệt lịch sự (Khó)", "きょうはありがとうございました。またあした。", "きょう は ありがとう ございました。また あした / kyou wa arigatou gozaimashita. mata ashita", "Cảm ơn về hôm nay. Hẹn gặp lại ngày mai.", new[] { "ありがとう", "またあした" }),
                new ShItem("ja-l11-sh-9", "hard", "Luyện trường âm & âm ngắt (Khó)", "せんせい、ゆっくりはなしてください。", "せんせい、ゆっくり はなして ください / sensei, yukkuri hanashite kudasai", "Thưa thầy/cô, xin hãy nói chậm lại ạ.", new[] { "せんせい", "ゆっくり", "はなして" })
            },
            dictation: new[]
            {
                new DiItem("ja-l11-di-1", "easy", "Chào buổi sáng", "おはようございます", "ohayou gozaimasu", "Chào buổi sáng ạ.", new[] { "おはよう", "ございます" }, new[] { "こんばんは", "さようなら", "ありがとう" }),
                new DiItem("ja-l11-di-2", "easy", "Hỏi sức khỏe", "おげんきですか", "ogenki desu ka", "Bạn có khỏe không?", new[] { "おげんき", "です", "か" }, new[] { "ます", "はい", "いいえ" }),
                new DiItem("ja-l11-di-3", "medium", "Xin lỗi & chờ đợi", "すみませんちょっとまってください", "sumimasen chotto matte kudasai", "Xin lỗi, vui lòng đợi một chút.", new[] { "すみません", "ちょっと", "まって", "ください" }, new[] { "ありがとう", "おはよう", "です" }),
                new DiItem("ja-l11-di-4", "medium", "Cảm ơn & đáp lễ", "はいげんきですありがとうございます", "hai genki desu arigatou gozaimasu", "Vâng, tôi khỏe. Cảm ơn bạn.", new[] { "はい", "げんきです", "ありがとう", "ございます" }, new[] { "いいえ", "すみません", "また" }),
                new DiItem("ja-l11-di-5", "hard", "Hẹn gặp ngày mai", "またあしたあいましょう", "mata ashita aimashou", "Hẹn gặp lại vào ngày mai nhé.", new[] { "また", "あした", "あいましょう" }, new[] { "きょう", "きのう", "ください" }),
                new DiItem("ja-l11-di-6", "hard", "Nhờ nói chậm", "もういちどゆっくりおねがいします", "mou ichido yukkuri onegaishimasu", "Xin vui lòng nói chậm lại một lần nữa.", new[] { "もういちど", "ゆっくり", "おねがいします" }, new[] { "ちょっと", "げんき", "おはよう" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l11-pr-1", "easy", "Trường âm おう", "おはようございます", "ohayou gozaimasu", "Chào buổi sáng ạ.", new[] { "おはよう", "ございます" }, "Đọc rõ trường âm おう trong lời chào buổi sáng.", new[] { "Kéo dài âm おう đủ 2 nhịp", "Không đọc ngắt thành 'ohayo'", "Hạ giọng nhẹ cuối câu ございます" }, "おはようございます。", new[] { "おはよう", "ございます" }),
                new PrItem("ja-l11-pr-2", "easy", "Trợ từ は đọc wa", "こんにちは", "konnichiwa", "Xin chào (ban ngày).", new[] { "こん", "にち", "は" }, "Phát âm chữ は cuối thành 'wa' và giữ nhịp ん rõ.", new[] { "Nối nhịp ん (n) tự nhiên", "Đọc ち (chi) rõ không lẫn し", "Đọc は cuối thành 'wa'" }, "こんにちは。", new[] { "こんにちは" }),
                new PrItem("ja-l11-pr-3", "medium", "Âm ngắt っ (Sokuon)", "ちょっとまってください", "chotto matte kudasai", "Vui lòng đợi một chút.", new[] { "ちょっと", "まって", "ください" }, "Giữ nhịp dừng 1 phách ở cả hai chữ っ nhỏ.", new[] { "Khựng nhẹ trước と trong ちょっと", "Khựng nhẹ trước て trong まって", "Đọc mượt ください" }, "すみません、ちょっとまってください。", new[] { "ちょっと", "まって" }),
                new PrItem("ja-l11-pr-4", "medium", "Phân biệt つ và す", "つくえのうえにあります", "tsukue no ue ni arimasu", "Ở trên cái bàn.", new[] { "つくえ", "うえ", "あります" }, "Phân biệt âm bật đầu lưỡi つ (tsu) với す (su).", new[] { "Đặt đầu lưỡi sát răng trên bật つ", "Chạm nhẹ lưỡi cho âm り", "Giữ す cuối nhẹ hơi" }, "つくえのうえにあります。", new[] { "つくえ", "あります" }),
                new PrItem("ja-l11-pr-5", "hard", "Trường âm えい & âm ngắt", "せんせいゆっくりはなしてください", "sensei yukkuri hanashite kudasai", "Thưa thầy/cô, xin nói chậm lại ạ.", new[] { "せんせい", "ゆっくり", "はなして" }, "Kết hợp kéo dài trường âm せい và ngừng っ trong ゆっくり.", new[] { "Kéo dài せい thành sē", "Dừng 1 nhịp ở っ của ゆっくり", "Đọc し mềm mại" }, "せんせい、ゆっくりはなしてください。", new[] { "せんせい", "ゆっくり" }),
                new PrItem("ja-l11-pr-6", "hard", "Lời chào tạm biệt", "しつれいしますおさきにしつれいします", "shitsurei shimasu, osaki ni shitsurei shimasu", "Xin phép, tôi xin phép về trước ạ.", new[] { "しつれい", "おさきに" }, "Luyện cặp し/つ và trường âm れい trong câu chào công sở.", new[] { "Phân biệt し và つ liên tiếp", "Kéo dài trường âm れい", "Giữ tốc độ đều và lịch sự" }, "おさきにしつれいします。", new[] { "しつれい", "おさきに" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l11-wm-1", "easy", "Sơ đồ từ vựng: ありがとう (Cảm ơn)", "ありがとう", "ありがとう (arigatou)", "Cảm ơn",
                    new[] { new WmNode("ありがとうございます", "arigatou gozaimasu", "Cảm ơn ạ (lịch sự)", "いつもありがとうございます。"), new WmNode("どうもありがとう", "doumo arigatou", "Cảm ơn nhiều nhé", "てつだってくれてどうもありがとう。") },
                    new[] { new WmNode("ありがとうございました", "arigatou gozaimashita", "Cảm ơn về việc đã qua", "きょうはありがとうございました。"), new WmNode("どういたしまして", "dou itashimashite", "Không có gì đâu (đáp lễ)", "いいえ、どういたしまして。") },
                    new[] { new WmNode("どうも", "doumo", "Cảm ơn (ngắn gọn)", "どうも、すみません。"), new WmNode("たすかりました", "tasukarimashita", "May quá, cảm ơn đã giúp", "ほんとうにたすかりました。") },
                    new[] { new WmEx("おしえてくれてありがとうございます。", "oshiete kurete arigatou gozaimasu", "Cảm ơn bạn đã chỉ giúp tôi ạ.") }),
                new WmItem("ja-l11-wm-2", "medium", "Sơ đồ từ vựng: すみません (Xin lỗi / Làm phiền)", "すみません", "すみません (sumimasen)", "Xin lỗi / Cho tôi hỏi nhờ",
                    new[] { new WmNode("すみません、ちょっと…", "sumimasen, chotto...", "Xin lỗi, cho hỏi chút...", "すみません、ちょっときいてもいいですか。"), new WmNode("おそくなってすみません", "osoku natte sumimasen", "Xin lỗi vì đến muộn", "おそくなってすみません！") },
                    new[] { new WmNode("ごめんなさい", "gomennasai", "Xin lỗi (thân mật hơn)", "わすれてごめんなさい。"), new WmNode("しつれいします", "shitsurei shimasu", "Xin thất lễ / Xin phép vào phòng", "しつれいします、はいってもいいですか。") },
                    new[] { new WmNode("もうしわけありません", "moushiwake arimasen", "Thành thật xin lỗi (rất trang trọng)", "ごめいわくをかけてもうしわけありません。") },
                    new[] { new WmEx("すみません、もういちどおねがいします。", "sumimasen, mou ichido onegaishimasu", "Xin lỗi, vui lòng nhắc lại một lần nữa ạ.") }),
                new WmItem("ja-l11-wm-3", "hard", "Sơ đồ từ vựng: げんき (Khỏe mạnh, tràn đầy năng lượng)", "げんき (元気)", "げんき (genki)", "Khỏe mạnh, vui vẻ, sung sức",
                    new[] { new WmNode("おげんきですか", "ogenki desu ka", "Bạn có khỏe không?", "たなかさん、おげんきですか。"), new WmNode("とてもげんきです", "totemo genki desu", "Rất khỏe mạnh", "おかげさまでとてもげんきです。") },
                    new[] { new WmNode("げんきをだして", "genki o dashite", "Hãy vui lên / Cố lên nhé", "しんぱいしないで、げんきをだして。") },
                    new[] { new WmNode("だいじょうぶ", "daijoubu", "Ổn, không sao đâu", "はい、だいじょうぶです。") },
                    new[] { new WmEx("ごかぞくのみなさんもおげんきですか。", "gokazoku no minasan mo ogenki desu ka", "Mọi người trong gia đình bạn cũng khỏe chứ?") })
            },
            cloze: new[]
            {
                new ClItem("ja-l11-cl-1", "easy", "Chào buổi sáng lịch sự", "Điền phần đuôi lịch sự cho lời chào buổi sáng:", "おはよう {{0}}。", new[] { new ClBlank(new[] { "ございます" }, "đuôi kính ngữ sau おはよう (5 chữ Hiragana)", "おはようございます là cách chào buổi sáng lịch sự chuẩn.") }, "おはようございます。", "ohayou gozaimasu", "Chào buổi sáng ạ.", "Buổi sáng dùng おはよう (thân mật) hoặc おはようございます (lịch sự)."),
                new ClItem("ja-l11-cl-2", "easy", "Hỏi thăm sức khỏe", "Điền từ chỉ sức khỏe và trợ từ nghi vấn:", "お{{0}}です{{1}}。", new[] { new ClBlank(new[] { "げんき", "元気" }, "khỏe mạnh (3 chữ Hiragana)", "おげんき = sức khỏe."), new ClBlank(new[] { "か" }, "trợ từ đặt cuối câu hỏi", "Thêm か cuối câu để tạo câu hỏi trong tiếng Nhật.") }, "おげんきですか。", "ogenki desu ka", "Bạn có khỏe không?", "Cấu trúc câu hỏi lịch sự cơ bản: 〜ですか。"),
                new ClItem("ja-l11-cl-3", "medium", "Xin chờ một chút", "Điền từ 'một chút' có âm ngắt っ:", "すみません、{{0}}まってください。", new[] { new ClBlank(new[] { "ちょっと" }, "một chút (chotto)", "ちょっと viết bằng chữ っ nhỏ ở giữa.") }, "すみません、ちょっとまってください。", "sumimasen, chotto matte kudasai", "Xin lỗi, vui lòng đợi một chút ạ.", "Âm ngắt っ thay đổi hoàn toàn ý nghĩa từ."),
                new ClItem("ja-l11-cl-4", "hard", "Đáp lời cảm ơn", "Điền câu đáp lễ lịch sự 'Không có chi':", "いいえ、{{0}}。", new[] { new ClBlank(new[] { "どういたしまして" }, "không có gì (dou itashimashite)", "どういたしまして là câu đáp lễ chuẩn mực sau lời cảm ơn.") }, "いいえ、どういたしまして。", "iie, dou itashimashite", "Không có gì đâu ạ.", "Đáp lại ありがとうございます một cách tự nhiên.")
            },
            translate: new[]
            {
                new TrItem("ja-l11-tr-1", "easy", "Chào buổi sáng lịch sự", "Chào buổi sáng ạ!", new[] { "おはよう", "ございます" }, new[] { "おはようございます。", "おはようございます" }, "おはようございます。", "ohayou gozaimasu", "Lời chào buổi sáng lịch sự dùng với thầy cô, đồng nghiệp, người lớn tuổi."),
                new TrItem("ja-l11-tr-2", "easy", "Cảm ơn lịch sự", "Xin cảm ơn rất nhiều ạ.", new[] { "ありがとう", "ございます" }, new[] { "ありがとうございます。", "どうもありがとうございます。", "ありがとうございます" }, "ありがとうございます。", "arigatou gozaimasu", "Chú ý trường âm ありがとう có chữ う ở cuối."),
                new TrItem("ja-l11-tr-3", "medium", "Hỏi thăm sức khỏe", "Bạn có khỏe không?", new[] { "おげんき", "ですか" }, new[] { "おげんきですか。", "お元気ですか。", "げんきですか。" }, "おげんきですか。", "ogenki desu ka", "Thêm tiền tố lịch sự お trước げんき khi hỏi thăm người khác."),
                new TrItem("ja-l11-tr-4", "hard", "Xin lỗi, đợi một chút", "Xin lỗi, vui lòng đợi một chút ạ.", new[] { "すみません", "ちょっと", "まってください" }, new[] { "すみません、ちょっとまってください。", "すみません、ちょっと待ってください。" }, "すみません、ちょっとまってください。", "sumimasen, chotto matte kudasai", "Mẫu câu 〜てください dùng để nhờ vả lịch sự.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l11-gf-1", "easy", "Trợ từ は trong こんにちは", "こんにちわ", "Chào buổi trưa viết bằng chữ は (không viết わ).", "こんにちは", new[] { "こんにちは", "こんにちは。" }, "konnichiwa", "Xin chào (buổi trưa).", "Dù đọc là 'wa', từ こんにちは bắt nguồn từ 今日(こんにち)は nên phải viết là は."),
                new GfItem("ja-l11-gf-2", "medium", "Thiếu chữ っ nhỏ trong ちょっと", "すみません、ちょとまってください。", "Từ 'một chút' cần âm ngắt っ (ちょっと).", "すみません、ちょっとまってください。", new[] { "すみません、ちょっとまってください。", "すみません、ちょっと待ってください。" }, "sumimasen, chotto matte kudasai", "Xin lỗi, vui lòng đợi một chút.", "ちょっと luôn có chữ っ nhỏ tạo âm ngắt."),
                new GfItem("ja-l11-gf-3", "hard", "Thiếu trợ từ hỏi か", "おげんきです？", "Câu hỏi lịch sự chuẩn văn viết cần trợ từ か cuối câu.", "おげんきですか。", new[] { "おげんきですか。", "お元気ですか。" }, "ogenki desu ka", "Bạn có khỏe không?", "Trong văn phong lịch sự N5, luôn đặt か sau です để tạo câu hỏi.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l11-rp-1", "easy", "Chào buổi sáng tại lớp học", "Bạn gặp thầy Tanaka vào buổi sáng tại hành lang trường tiếng Nhật.", "Thầy Tanaka", "おはようございます！おげんきですか。", "ohayou gozaimasu! ogenki desu ka?", "Chào buổi sáng! Em có khỏe không?", "Chào buổi sáng thầy, trả lời rằng bạn khỏe và cảm ơn thầy.", new[] { "おはよう|ohayou", "げんき|元気|genki", "ありがとう|arigatou" }, new[] { "おはようございます！はい、げんきです。ありがとうございます。" }, "ohayou gozaimasu! hai, genki desu. arigatou gozaimasu.", 8),
                new RpItem("ja-l11-rp-2", "medium", "Nhờ nói chậm lại trong lớp", "Cô giáo nói tiếng Nhật hơi nhanh và bạn muốn xin phép nghe lại chậm hơn.", "Cô Suzuki", "では、つぎのページをよみましょう。わかりましたか。", "dewa, tsugi no peeji o yomimashou. wakarimashita ka?", "Nào, chúng ta cùng đọc trang tiếp theo. Em hiểu chưa?", "Xin lỗi cô lịch sự và nhờ cô nói chậm lại một lần nữa.", new[] { "すみません|sumimasen", "ゆっくり|yukkuri", "ください|おねがいします|kudasai|onegaishimasu" }, new[] { "すみません、もういちどゆっくりはなしてください。" }, "sumimasen, mou ichido yukkuri hanashite kudasai.", 8),
                new RpItem("ja-l11-rp-3", "hard", "Chào tạm biệt sau giờ học", "Buổi học đầu tiên kết thúc, thầy Tanaka chào tạm biệt bạn.", "Thầy Tanaka", "きょうはおつかれさまでした。またあしたあいましょう！", "kyou wa otsukaresama deshita. mata ashita aimashou!", "Hôm nay em đã vất vả rồi. Hẹn gặp lại ngày mai nhé!", "Cảm ơn thầy về buổi học hôm nay và hẹn gặp lại vào ngày mai.", new[] { "ありがとう|arigatou", "また|あした|mata|ashita" }, new[] { "せんせい、きょうはありがとうございました。またあした！" }, "sensei, kyou wa arigatou gozaimashita. mata ashita!", 8)
            }, now));

        // ---------------- LESSON 1.2 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.JapaneseLesson12Id, "ja-JP", "Nhật N5 · Bài 1.2",
            shadowing: new[]
            {
                new ShItem("ja-l12-sh-1", "easy", "Chào lần đầu (Dễ)", "はじめまして、わたしはミンです。", "はじめまして、わたし は ミン です / hajimemashite, watashi wa Min desu", "Xin chào lần đầu gặp mặt, tôi là Minh.", new[] { "はじめまして", "わたし" }),
                new ShItem("ja-l12-sh-2", "easy", "Quốc tịch (Dễ)", "ベトナムからきました。", "ベトナム から きました / Betonamu kara kimashita", "Tôi đến từ Việt Nam.", new[] { "ベトナム", "きました" }),
                new ShItem("ja-l12-sh-3", "easy", "Kết thúc giới thiệu (Dễ)", "どうぞよろしくおねがいします。", "どうぞ よろしく おねがいします / douzo yoroshiku onegaishimasu", "Rất mong được giúp đỡ ạ.", new[] { "よろしく", "おねがいします" }),
                new ShItem("ja-l12-sh-4", "medium", "Nghề nghiệp (Vừa)", "わたしはだいがくのがくせいです。", "わたし は だいがく の がくせい です / watashi wa daigaku no gakusei desu", "Tôi là sinh viên đại học.", new[] { "だいがく", "がくせい" }),
                new ShItem("ja-l12-sh-5", "medium", "Từ mượn Katakana (Vừa)", "これはわたしのパソコンとノートです。", "これ は わたし の パソコン と ノート です / kore wa watashi no pasokon to nooto desu", "Đây là máy tính cá nhân và quyển vở của tôi.", new[] { "パソコン", "ノート" }),
                new ShItem("ja-l12-sh-6", "medium", "Phủ định lịch sự (Vừa)", "わたしはかいしゃいんではありません。", "わたし は かいしゃいん では ありません / watashi wa kaishain dewa arimasen", "Tôi không phải là nhân viên công ty.", new[] { "かいしゃいん", "ではありません" }),
                new ShItem("ja-l12-sh-7", "hard", "Trợ từ も (Khó)", "ランさんもベトナムじんですか。", "ランさん も ベトナムじん です か / Ran-san mo Betonamu-jin desu ka", "Bạn Lan cũng là người Việt Nam phải không?", new[] { "ベトナムじん", "も" }),
                new ShItem("ja-l12-sh-8", "hard", "Giới thiệu chuyên ngành (Khó)", "わたしのせんもんはコンピュータかがくです。", "わたし の せんもん は コンピュータ かがく です / watashi no senmon wa konpyuuta kagaku desu", "Chuyên ngành của tôi là khoa học máy tính.", new[] { "せんもん", "コンピュータ" }),
                new ShItem("ja-l12-sh-9", "hard", "Hỏi công việc (Khó)", "おしごとはなんですか。エンジニアです。", "おしごと は なん です か。エンジニア です / oshigoto wa nan desu ka. enjinia desu", "Công việc của bạn là gì? Tôi là kỹ sư.", new[] { "おしごと", "エンジニア" })
            },
            dictation: new[]
            {
                new DiItem("ja-l12-di-1", "easy", "Giới thiệu tên", "はじめましてわたしはミンです", "hajimemashite watashi wa Min desu", "Xin chào lần đầu, tôi là Minh.", new[] { "はじめまして", "わたしは", "ミン", "です" }, new[] { "かれは", "さん", "ます" }),
                new DiItem("ja-l12-di-2", "easy", "Từ mượn Katakana", "コーヒーとノート", "koohii to nooto", "Cà phê và quyển vở.", new[] { "コーヒー", "と", "ノート" }, new[] { "ホテル", "バス", "パン" }),
                new DiItem("ja-l12-di-3", "medium", "Quốc tịch", "わたしはベトナムじんです", "watashi wa Betonamu-jin desu", "Tôi là người Việt Nam.", new[] { "わたし", "は", "ベトナムじん", "です" }, new[] { "にほんじん", "も", "か" }),
                new DiItem("ja-l12-di-4", "medium", "Sinh viên đại học", "だいがくのがくせいです", "daigaku no gakusei desu", "Tôi là sinh viên đại học.", new[] { "だいがく", "の", "がくせい", "です" }, new[] { "せんせい", "は", "から" }),
                new DiItem("ja-l12-di-5", "hard", "Phủ định", "わたしはせんせいではありません", "watashi wa sensei dewa arimasen", "Tôi không phải là giáo viên.", new[] { "わたしは", "せんせい", "ではありません" }, new[] { "がくせい", "です", "ます" }),
                new DiItem("ja-l12-di-6", "hard", "Trợ từ も", "わたしもエンジニアですよろしくおねがいします", "watashi mo enjinia desu yoroshiku onegaishimasu", "Tôi cũng là kỹ sư, rất mong được giúp đỡ.", new[] { "わたしも", "エンジニアです", "よろしく", "おねがいします" }, new[] { "は", "ありがとう", "さようなら" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l12-pr-1", "easy", "Katakana trường âm", "コーヒーをのみます", "koohii o nomimasu", "Tôi uống cà phê.", new[] { "コーヒー", "のみます" }, "Kéo dài đúng hai dấu trường âm ー trong コーヒー.", new[] { "Kéo dài コ thành kō", "Kéo dài ヒ thành hī", "Không rút gọn thành 'cohi'" }, "コーヒーをのみます。", new[] { "コーヒー" }),
                new PrItem("ja-l12-pr-2", "easy", "Chào gặp mặt lần đầu", "はじめましてどうぞよろしくおねがいします", "hajimemashite douzo yoroshiku onegaishimasu", "Rất hân hạnh gặp mặt, mong được giúp đỡ.", new[] { "はじめまして", "よろしく", "おねがいします" }, "Đọc trôi chảy câu mở đầu và kết thúc khi làm quen.", new[] { "Giữ rõ âm じ và し", "Kéo dài どう trong どうぞ", "Chạm lưỡi nhẹ ở ろ trong よろしく" }, "はじめまして、どうぞよろしくおねがいします。", new[] { "はじめまして", "よろしく" }),
                new PrItem("ja-l12-pr-3", "medium", "Tên nước Katakana", "ベトナムからきました", "Betonamu kara kimashita", "Tôi đến từ Việt Nam.", new[] { "ベトナム", "から", "きました" }, "Đọc chuẩn 4 phách đều nhau ベ-ト-ナ-ム.", new[] { "Không đọc như tiếng Việt 'Việt Nam'", "Đọc rõ ら nhẹ trong から", "Giữ し nhẹ trong きました" }, "わたしはベトナムからきました。", new[] { "ベトナム", "きました" }),
                new PrItem("ja-l12-pr-4", "medium", "Katakana công nghệ", "パソコンとスマートフォンをつかいます", "pasokon to sumaatofon o tsukaimasu", "Tôi dùng máy tính và điện thoại thông minh.", new[] { "パソコン", "スマートフォン", "つかいます" }, "Giữ trường âm ー và âm bật つ rõ ràng.", new[] { "Kéo dài マ thành mā", "Kéo dài フォ thành fō", "Bật đầu lưỡi ở つ của つかいます" }, "パソコンとスマートフォンをつかいます。", new[] { "パソコン", "つかいます" }),
                new PrItem("ja-l12-pr-5", "hard", "Phủ định lịch sự", "わたしはかいしゃいんではありません", "watashi wa kaishain dewa arimasen", "Tôi không phải nhân viên công ty.", new[] { "かいしゃいん", "ではありません" }, "Đọc liền mạch cụm phủ định では ありません.", new[] { "Đọc しゃ mềm (sha)", "Đọc は trong では thành 'wa'", "Chạm lưỡi nhẹ cho り" }, "わたしはかいしゃいんではありません。", new[] { "かいしゃいん", "ではありません" }),
                new PrItem("ja-l12-pr-6", "hard", "Tự giới thiệu đầy đủ", "はじめましてハノイからきたエンジニアです", "hajimemashite Hanoi kara kita enjinia desu", "Xin chào, tôi là kỹ sư đến từ Hà Nội.", new[] { "ハノイ", "エンジニア" }, "Kết hợp Katakana và câu giới thiệu tự tin.", new[] { "Phát âm rõ ハ-ノ-イ", "Đọc じ trong エンジニア rõ", "Ngắt nghỉ tự nhiên sau はじめまして" }, "はじめまして、ハノイからきたエンジニアです。", new[] { "ハノイ", "エンジニア" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l12-wm-1", "easy", "Sơ đồ từ vựng: がくせい (Sinh viên)", "がくせい (学生)", "がくせい (gakusei)", "Học sinh, sinh viên",
                    new[] { new WmNode("だいがくのがくせい", "daigaku no gakusei", "Sinh viên đại học", "わたしはだいがくのがくせいです。"), new WmNode("りゅうがくせい", "ryuugakusei", "Du học sinh", "かれはにほんのりゅうがくせいです。") },
                    new[] { new WmNode("がくせいしょう", "gakuseishou", "Thẻ sinh viên", "がくせいしょうをみせてください。") },
                    new[] { new WmNode("せいと", "seito", "Học sinh phổ thông", "こうこうのせいとです。") },
                    new[] { new WmEx("わたしはにほんごのがくせいです。", "watashi wa nihongo no gakusei desu", "Tôi là học viên tiếng Nhật.") }),
                new WmItem("ja-l12-wm-2", "medium", "Sơ đồ từ vựng: かいしゃ (Công ty)", "かいしゃ (会社)", "かいしゃ (kaisha)", "Công ty, doanh nghiệp",
                    new[] { new WmNode("かいしゃいん", "kaishain", "Nhân viên công ty", "あにはかいしゃいんです。"), new WmNode("ＩＴかいしゃ", "ai-tii kaisha", "Công ty công nghệ IT", "ITかいしゃではたらいています。") },
                    new[] { new WmNode("かいしゃへいく", "kaisha e iku", "Đi đến công ty", "まいあさかいしゃへいきます。") },
                    new[] { new WmNode("しごと", "shigoto", "Công việc", "おしごとはなんですか。") },
                    new[] { new WmEx("ちちはにほんのかいしゃではたらいています。", "chichi wa nihon no kaisha de hataraite imasu", "Bố tôi làm việc tại một công ty Nhật Bản.") }),
                new WmItem("ja-l12-wm-3", "hard", "Sơ đồ từ vựng: よろしく (Rất mong được giúp đỡ)", "よろしく", "よろしく (yoroshiku)", "Xin nhờ cậy / Rất hân hạnh",
                    new[] { new WmNode("どうぞよろしくおねがいします", "douzo yoroshiku onegaishimasu", "Rất mong được giúp đỡ (trang trọng)", "はじめまして、どうぞよろしくおねがいします。"), new WmNode("これからもよろしく", "kore kara mo yoroshiku", "Từ nay về sau cũng mong được giúp đỡ", "これからもよろしくおねがいします。") },
                    new[] { new WmNode("〜によろしくつたえてください", "~ ni yoroshiku tsutaete kudasai", "Cho tôi gửi lời hỏi thăm tới...", "ごかぞくによろしくおつたえください。") },
                    new[] { new WmNode("おねがいします", "onegaishimasu", "Xin vui lòng / Nhờ anh chị", "チェックをおねがいします。") },
                    new[] { new WmEx("きょうからいっしょにはたらきます。よろしくおねがいします。", "kyou kara issho ni hatarakimasu. yoroshiku onegaishimasu", "Từ hôm nay tôi sẽ làm việc cùng mọi người. Rất mong được giúp đỡ.") })
            },
            cloze: new[]
            {
                new ClItem("ja-l12-cl-1", "easy", "Trợ từ chủ đề は", "Điền trợ từ chủ đề đúng:", "わたし{{0}}ベトナムじんです。", new[] { new ClBlank(new[] { "は" }, "trợ từ chủ đề (đọc là wa)", "Dùng は để đánh dấu chủ đề 'Tôi'.") }, "わたしはベトナムじんです。", "watashi wa Betonamu-jin desu", "Tôi là người Việt Nam.", "Mẫu câu danh từ cơ bản: A は B です。"),
                new ClItem("ja-l12-cl-2", "easy", "Từ mượn Katakana: Cà phê", "Viết từ 'cà phê' bằng Katakana chuẩn (có dấu trường âm ー):", "これは{{0}}です。", new[] { new ClBlank(new[] { "コーヒー" }, "Katakana 4 ký tự: コ-ヒ-", "Từ mượn nước ngoài bắt buộc viết bằng Katakana: コーヒー.", false) }, "これはコーヒーです。", "kore wa koohii desu", "Đây là cà phê.", "Kiểm tra phân biệt Katakana — không chấp nhận Hiragana."),
                new ClItem("ja-l12-cl-3", "medium", "Trợ từ sở hữu の", "Điền trợ từ nối hai danh từ:", "わたしはハノイだいがく{{0}}がくせいです。", new[] { new ClBlank(new[] { "の" }, "của / thuộc về (no)", "N1 の N2 = N2 thuộc về N1.") }, "わたしはハノイだいがくのがくせいです。", "watashi wa Hanoi daigaku no gakusei desu", "Tôi là sinh viên của Đại học Hà Nội.", "Trợ từ の đứng giữa 2 danh từ."),
                new ClItem("ja-l12-cl-4", "hard", "Trợ từ も (cũng)", "Điền trợ từ mang nghĩa 'cũng':", "サトウさんはエンジニアです。わたし{{0}}エンジニアです。", new[] { new ClBlank(new[] { "も" }, "cũng (mo)", "Thay は bằng も khi chủ ngữ thứ hai có cùng tính chất.") }, "サトウさんはエンジニアです。わたしもエンジニアです。", "Satou-san wa enjinia desu. watashi mo enjinia desu", "Anh Sato là kỹ sư. Tôi cũng là kỹ sư.", "Trợ từ も thay thế hoàn toàn trợ từ は.")
            },
            translate: new[]
            {
                new TrItem("ja-l12-tr-1", "easy", "Từ mượn Katakana (bắt buộc Katakana)", "Đây là cà phê.", new[] { "これ", "コーヒー", "です" }, new[] { "これはコーヒーです。", "これはコーヒーです" }, "これはコーヒーです。", "kore wa koohii desu", "Từ mượn 'cà phê' phải viết bằng Katakana là コーヒー (không viết Hiragana).", false),
                new TrItem("ja-l12-tr-2", "easy", "Giới thiệu quốc tịch", "Tôi là người Việt Nam.", new[] { "わたし", "ベトナムじん", "です" }, new[] { "わたしはベトナムじんです。", "私はベトナム人です。", "わたしはベトナム人です。" }, "私はベトナム人です。", "watashi wa Betonamu-jin desu", "Tên nước (Katakana) + じん (人) tạo danh từ chỉ quốc tịch."),
                new TrItem("ja-l12-tr-3", "medium", "Đến từ đâu", "Tôi đến từ Việt Nam.", new[] { "ベトナム", "から", "きました" }, new[] { "ベトナムからきました。", "わたしはベトナムからきました。", "私はベトナムから来ました。", "ベトナムから来ました。" }, "ベトナムから来ました。", "Betonamu kara kimashita", "Mẫu [Địa danh] + から + きました = Đến từ..."),
                new TrItem("ja-l12-tr-4", "hard", "Phủ định nghề nghiệp", "Tôi không phải là nhân viên công ty.", new[] { "かいしゃいん", "ではありません" }, new[] { "わたしはかいしゃいんではありません。", "私は会社員ではありません。", "わたしはかいしゃいんじゃありません。" }, "私は会社員ではありません。", "watashi wa kaishain dewa arimasen", "Dạng phủ định trang trọng của です là ではありません.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l12-gf-1", "easy", "Sai chữ Katakana 'Việt Nam'", "わたしはべとなむじんです。", "Tên nước ngoài viết bằng Katakana (ベトナム).", "わたしはベトナムじんです。", new[] { "わたしはベトナムじんです。", "私はベトナム人です。" }, "watashi wa Betonamu-jin desu", "Tôi là người Việt Nam.", "Tên quốc gia nước ngoài như ベトナム luôn dùng Katakana."),
                new GfItem("ja-l12-gf-2", "medium", "Dùng thừa は trước も", "わたしもはがくせいです。", "も thay thế は, không đứng cùng は.", "わたしもがくせいです。", new[] { "わたしもがくせいです。", "私も学生です。" }, "watashi mo gakusei desu", "Tôi cũng là sinh viên.", "Khi dùng も ('cũng'), phải bỏ は."),
                new GfItem("ja-l12-gf-3", "hard", "Ngược thứ tự N1 の N2", "わたしはがくせいのだいがくです。", "Danh từ chính đứng SAU の (đại học -> sinh viên).", "わたしはだいがくのがくせいです。", new[] { "わたしはだいがくのがくせいです。", "私は大学の学生です。" }, "watashi wa daigaku no gakusei desu", "Tôi là sinh viên đại học.", "Trong tiếng Nhật, bổ ngữ đứng trước の và danh từ chính đứng sau の.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l12-rp-1", "easy", "Làm quen bạn học mới", "Ngày đầu nhập học tại Tokyo, bạn cùng bàn chủ động chào làm quen.", "Bạn Ken", "はじめまして！ケンです。おなまえはなんですか。", "hajimemashite! Ken desu. onamae wa nan desu ka?", "Xin chào lần đầu! Mình là Ken. Tên bạn là gì?", "Chào lại bằng はじめまして, giới thiệu tên của bạn và nói よろしくおねがいします.", new[] { "はじめまして|hajimemashite", "です|desu", "よろしく|yoroshiku" }, new[] { "はじめまして！わたしはミンです。どうぞよろしくおねがいします。" }, "hajimemashite! watashi wa Min desu. douzo yoroshiku onegaishimasu.", 10),
                new RpItem("ja-l12-rp-2", "medium", "Hỏi quốc tịch và nghề nghiệp", "Đồng nghiệp mới hỏi bạn đến từ đâu và làm nghề gì.", "Chị Tanaka", "どちらからきましたか。かいしゃいんですか。", "dochira kara kimashita ka? kaishain desu ka?", "Bạn đến từ đâu thế? Bạn là nhân viên công ty à?", "Trả lời đến từ Việt Nam (ベトナムから) và giới thiệu bạn là sinh viên hoặc kỹ sư.", new[] { "ベトナム|Betonamu", "から|きました|kara|kimashita", "がくせい|学生|エンジニア|かいしゃいん|会社員" }, new[] { "ベトナムからきました。わたしはエンジニアです。" }, "Betonamu kara kimashita. watashi wa enjinia desu.", 10),
                new RpItem("ja-l12-rp-3", "hard", "Tự giới thiệu 3 câu hoàn chỉnh", "Quản lý mời bạn tự giới thiệu ngắn trước nhóm 5 người.", "Quản lý Sato", "では、かんたんにじこしょうかいをおねがいします。", "dewa, kantan ni jikoshoukai o onegaishimasu.", "Vậy mời bạn tự giới thiệu ngắn gọn về bản thân nhé.", "Giới thiệu đủ 3 ý: はじめまして + tên + đến từ Việt Nam + nghề nghiệp + よろしくおねがいします.", new[] { "はじめまして", "ベトナム", "です", "よろしく" }, new[] { "はじめまして。ミンです。ベトナムからきました。だいがくのがくせいです。どうぞよろしくおねがいします。" }, "hajimemashite. Min desu. Betonamu kara kimashita. daigaku no gakusei desu. douzo yoroshiku onegaishimasu.", 15)
            }, now));

        // ---------------- LESSON 2.1 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.JapaneseLesson21Id, "ja-JP", "Nhật N5 · Bài 2.1",
            shadowing: new[]
            {
                new ShItem("ja-l21-sh-1", "easy", "Hỏi giờ (Dễ)", "いまなんじですか。", "いま なんじ です か / ima nanji desu ka", "Bây giờ là mấy giờ rồi?", new[] { "いま", "なんじ" }),
                new ShItem("ja-l21-sh-2", "easy", "Biến âm 4 giờ (Dễ)", "いまごごよじです。", "いま ごご よじ です / ima gogo yoji desu", "Bây giờ là 4 giờ chiều.", new[] { "ごご", "よじ" }),
                new ShItem("ja-l21-sh-3", "easy", "Biến âm 9 giờ rưỡi (Dễ)", "ごぜんくじはんです。", "ごぜん くじ はん です / gozen kuji han desu", "Là 9 giờ rưỡi sáng.", new[] { "ごぜん", "くじはん" }),
                new ShItem("ja-l21-sh-4", "medium", "Giờ thức dậy (Vừa)", "まいあさろくじはんにおきます。", "まいあさ ろくじはん に おきます / maiasa rokuji han ni okimasu", "Mỗi sáng tôi dậy lúc 6 giờ rưỡi.", new[] { "まいあさ", "ろくじはん", "おきます" }),
                new ShItem("ja-l21-sh-5", "medium", "Khoảng thời gian (Vừa)", "ぎんこうはくじからさんじまでです。", "ぎんこう は くじ から さんじ まで です / ginkou wa kuji kara sanji made desu", "Ngân hàng mở cửa từ 9 giờ đến 3 giờ.", new[] { "から", "まで", "ぎんこう" }),
                new ShItem("ja-l21-sh-6", "medium", "Ngày trong tuần (Vừa)", "きょうはげつようびです。", "きょう は げつようび です / kyou wa getsuyoubi desu", "Hôm nay là thứ Hai.", new[] { "きょう", "げつようび" }),
                new ShItem("ja-l21-sh-7", "hard", "Lịch học chi tiết (Khó)", "にほんごのクラスはじゅうじじゅっぷんからです。", "にほんご の クラス は じゅうじ じゅっぷん から です / nihongo no kurasu wa juuji juppun kara desu", "Lớp tiếng Nhật bắt đầu từ 10 giờ 10 phút.", new[] { "クラス", "じゅうじ", "じゅっぷん" }),
                new ShItem("ja-l21-sh-8", "hard", "Hẹn gặp cuối tuần (Khó)", "どようびのごごにじにあいましょう。", "どようび の ごご にじ に あいましょう / doyoubi no gogo niji ni aimashou", "Chúng ta hãy gặp nhau lúc 2 giờ chiều thứ Bảy nhé.", new[] { "どようび", "ごごにじ", "あいましょう" }),
                new ShItem("ja-l21-sh-9", "hard", "Hỏi ngày nghỉ (Khó)", "おやすみはなんようびですか。にちようびです。", "おやすみ は なんようび です か。にちようび です / oyasumi wa nanyoubi desu ka. nichiyoubi desu", "Ngày nghỉ là thứ mấy? Là Chủ nhật.", new[] { "おやすみ", "なんようび", "にちようび" })
            },
            dictation: new[]
            {
                new DiItem("ja-l21-di-1", "easy", "Hỏi giờ", "いまなんじですか", "ima nanji desu ka", "Bây giờ là mấy giờ?", new[] { "いま", "なんじ", "です", "か" }, new[] { "あした", "よじ", "ます" }),
                new DiItem("ja-l21-di-2", "easy", "9 giờ rưỡi sáng", "ごぜんくじはんです", "gozen kuji han desu", "9 giờ rưỡi sáng.", new[] { "ごぜん", "くじ", "はん", "です" }, new[] { "ごご", "よじ", "か" }),
                new DiItem("ja-l21-di-3", "medium", "Từ... đến...", "くじからごじまではたらきます", "kuji kara goji made hatarakimasu", "Tôi làm việc từ 9 giờ đến 5 giờ.", new[] { "くじ", "から", "ごじ", "まで", "はたらきます" }, new[] { "に", "を", "おきます" }),
                new DiItem("ja-l21-di-4", "medium", "Dậy lúc 7 giờ", "しちじにおきます", "shichiji ni okimasu", "Tôi dậy lúc 7 giờ.", new[] { "しちじ", "に", "おきます" }, new[] { "ねます", "で", "は" }),
                new DiItem("ja-l21-di-5", "hard", "10 giờ 10 phút", "かいぎはじゅうじじゅっぷんからです", "kaigi wa juuji juppun kara desu", "Cuộc họp bắt đầu từ 10 giờ 10 phút.", new[] { "かいぎは", "じゅうじ", "じゅっぷん", "からです" }, new[] { "まで", "ろっぷん", "です" }),
                new DiItem("ja-l21-di-6", "hard", "Hẹn thứ Sáu", "きんようびのごごさんじにあいましょう", "kinyoubi no gogo sanji ni aimashou", "Hẹn gặp lúc 3 giờ chiều thứ Sáu nhé.", new[] { "きんようびの", "ごご", "さんじに", "あいましょう" }, new[] { "げつようび", "から", "ます" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l21-pr-1", "easy", "Giờ biến âm 4h & 9h", "よじからくじまでです", "yoji kara kuji made desu", "Từ 4 giờ đến 9 giờ.", new[] { "よじ", "くじ" }, "Đọc chuẩn よじ (không đọc しじ) và くじ (không đọc きゅうじ).", new[] { "Rõ ràng よじ", "Rõ ràng くじ", "Ngắt nhẹ sau から" }, "よじからくじまでです。", new[] { "よじ", "くじ" }),
                new PrItem("ja-l21-pr-2", "easy", "Biến âm phút っ", "いっぷんとろっぷんです", "ippun to roppun desu", "1 phút và 6 phút.", new[] { "いっぷん", "ろっぷん" }, "Luyện âm ngắt っ + ぷ bật hơi gọn.", new[] { "Dừng 1 nhịp trước ぷ", "Bật nhẹ âm p trong ぷん", "Đọc ろ nhẹ không cứng" }, "いっぷんとろっぷんです。", new[] { "いっぷん", "ろっぷん" }),
                new PrItem("ja-l21-pr-3", "medium", "Thứ trong tuần", "げつようびからきんようびまで", "getsuyoubi kara kinyoubi made", "Từ thứ Hai đến thứ Sáu.", new[] { "げつようび", "きんようび" }, "Chú ý trường âm よう và âm つ trong げつようび.", new[] { "Bật nhẹ つ trong げつ", "Kéo dài よう thành yō", "Giữ rõ ん trong きん" }, "げつようびからきんようびまではたらきます。", new[] { "げつようび", "きんようび" }),
                new PrItem("ja-l21-pr-4", "medium", "7 giờ sáng", "ごぜんしちじにおきます", "gozen shichiji ni okimasu", "Thức dậy lúc 7 giờ sáng.", new[] { "ごぜん", "しちじ", "おきます" }, "Phân biệt cặp âm し và ち liền nhau trong しちじ.", new[] { "し mềm rồi sang ち bật nhẹ", "Đọc じ rõ rung nhẹ", "Nhấn nhẹ おきます" }, "まいあさごぜんしちじにおきます。", new[] { "しちじ", "おきます" }),
                new PrItem("ja-l21-pr-5", "hard", "10 giờ 30 phút", "じゅうじさんじゅっぷんにはじまります", "juuji sanjuppun ni hajimarimasu", "Bắt đầu lúc 10 giờ 30 phút.", new[] { "じゅうじ", "さんじゅっぷん", "はじまります" }, "Kéo dài じゅう và ngắt rõ っ trong じゅっぷん.", new[] { "じゅう kéo dài 2 nhịp", "じゅっぷん ngắn + âm ngắt っ", "Đọc mượt はじまります" }, "クラスはじゅうじさんじゅっぷんにはじまります。", new[] { "じゅうじ", "じゅっぷん" }),
                new PrItem("ja-l21-pr-6", "hard", "Hẹn lịch cuối tuần", "どようびのごごにじにえきであいましょう", "doyoubi no gogo niji ni eki de aimashou", "Gặp nhau ở nhà ga lúc 2 giờ chiều thứ Bảy nhé.", new[] { "どようび", "にじ", "あいましょう" }, "Giữ trường âm よう và しょう ổn định.", new[] { "Kéo dài どようび", "Nói rõ trợ từ に và で", "Kéo dài ましょう cuối câu" }, "どようびのごごにじにえきであいましょう。", new[] { "どようび", "あいましょう" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l21-wm-1", "easy", "Sơ đồ từ vựng: じかん (Thời gian)", "じかん (時間)", "じかん (jikan)", "Thời gian, tiếng đồng hồ",
                    new[] { new WmNode("いちじかん", "ichijikan", "Một tiếng đồng hồ", "いちじかんべんきょうします。"), new WmNode("じかんがあります", "jikan ga arimasu", "Có thời gian", "あしたはじかんがありますか。") },
                    new[] { new WmNode("じかんがかかる", "jikan ga kakaru", "Tốn thời gian", "ここまでさんじゅっぷんかかります。") },
                    new[] { new WmNode("とき", "toki", "Lúc, khi", "ひまなときにほんをよみます。") },
                    new[] { new WmEx("にほんごのじゅぎょうはにじかんです。", "nihongo no jugyou wa nijikan desu", "Tiết học tiếng Nhật kéo dài 2 tiếng.") }),
                new WmItem("ja-l21-wm-2", "medium", "Sơ đồ từ vựng: あした (Ngày mai)", "あした (明日)", "あした (ashita)", "Ngày mai",
                    new[] { new WmNode("あしたのあさ", "ashita no asa", "Sáng mai", "あしたのあさはやくおきます。"), new WmNode("またあした", "mata ashita", "Hẹn gặp lại ngày mai", "じゃ、またあした！") },
                    new[] { new WmNode("あさって", "asatte", "Ngày kia", "あさってはやすみです。") },
                    new[] { new WmNode("きょう / きのう", "kyou / kinou", "Hôm nay / Hôm qua", "きょうはかようびです。") },
                    new[] { new WmEx("あしたのくじにテストがあります。", "ashita no kuji ni tesuto ga arimasu", "9 giờ ngày mai có bài kiểm tra.") }),
                new WmItem("ja-l21-wm-3", "hard", "Sơ đồ từ vựng: やすみ (Ngày nghỉ)", "やすみ (休み)", "やすみ (yasumi)", "Nghỉ ngơi, ngày nghỉ",
                    new[] { new WmNode("ひるやすみ", "hiruyasumi", "Giờ nghỉ trưa", "ひるやすみはじゅうにじからです。"), new WmNode("なつやすみ", "natsuyasumi", "Kỳ nghỉ hè", "なつやすみににほんへいきます。") },
                    new[] { new WmNode("かいしゃをやすむ", "kaisha o yasumu", "Nghỉ làm", "かぜでかいしゃをやすみました。") },
                    new[] { new WmNode("きゅうじつ", "kyuujitsu", "Ngày nghỉ lễ", "きゅうじつはゆっくりします。") },
                    new[] { new WmEx("としょかんのやすみはげつようびです。", "toshokan no yasumi wa getsuyoubi desu", "Ngày nghỉ của thư viện là thứ Hai.") })
            },
            cloze: new[]
            {
                new ClItem("ja-l21-cl-1", "easy", "Hỏi giờ hiện tại", "Điền từ 'mấy giờ':", "いま、{{0}}ですか。", new[] { new ClBlank(new[] { "なんじ", "何時" }, "mấy giờ (nanji)", "なんじ = mấy giờ.") }, "いま、なんじですか。", "ima, nanji desu ka", "Bây giờ là mấy giờ?", "Dùng いまなんじですか khi hỏi giờ."),
                new ClItem("ja-l21-cl-2", "easy", "Trợ từ thời điểm に", "Điền trợ từ đứng sau mốc giờ cụ thể:", "まいあさろくじ{{0}}おきます。", new[] { new ClBlank(new[] { "に" }, "trợ từ chỉ thời điểm (ni)", "Sau mốc giờ/ngày có con số luôn dùng に.") }, "まいあさろくじにおきます。", "maiasa rokuji ni okimasu", "Mỗi sáng tôi dậy lúc 6 giờ.", "Không dùng に sau まいあさ."),
                new ClItem("ja-l21-cl-3", "medium", "Cấu trúc Từ... Đến...", "Điền 2 trợ từ diễn tả 'từ 9 giờ đến 5 giờ':", "しごとはくじ{{0}}ごじ{{1}}です。", new[] { new ClBlank(new[] { "から" }, "từ (kara)", "から = bắt đầu từ."), new ClBlank(new[] { "まで" }, "đến (made)", "まで = kết thúc lúc.") }, "しごとはくじからごじまでです。", "shigoto wa kuji kara goji made desu", "Công việc kéo dài từ 9 giờ đến 5 giờ.", "Mẫu 〜から〜まで dùng được cho cả thời gian và địa điểm."),
                new ClItem("ja-l21-cl-4", "hard", "Biến âm 4 giờ & 10 phút", "Điền cách đọc Hiragana chuẩn cho '4 giờ 10 phút':", "いま{{0}}{{1}}です。", new[] { new ClBlank(new[] { "よじ" }, "4 giờ (yoji)", "4 giờ đọc là よじ, không đọc しじ."), new ClBlank(new[] { "じゅっぷん", "じっぷん" }, "10 phút (juppun)", "10 phút có biến âm ngắt じゅっぷん.") }, "いまよじじゅっぷんです。", "ima yoji juppun desu", "Bây giờ là 4 giờ 10 phút.", "Thuộc lòng biến âm giờ phút để tránh nhầm trong kỳ thi JLPT N5.")
            },
            translate: new[]
            {
                new TrItem("ja-l21-tr-1", "easy", "Hỏi giờ", "Bây giờ là mấy giờ?", new[] { "いま", "なんじ", "ですか" }, new[] { "いまなんじですか。", "今何時ですか。", "いま、なんじですか。" }, "今何時ですか。", "ima nanji desu ka", "Câu hỏi giờ thông dụng nhất trong tiếng Nhật."),
                new TrItem("ja-l21-tr-2", "easy", "4 giờ chiều", "Bây giờ là 4 giờ chiều.", new[] { "ごご", "よじ" }, new[] { "いまごごよじです。", "今午後四時です。", "ごごよじです。", "今、午後4時です。" }, "今、午後四時です。", "ima, gogo yoji desu", "ごご (chiều) đặt trước số giờ; 4 giờ đọc là よじ."),
                new TrItem("ja-l21-tr-3", "medium", "Thức dậy lúc 7 giờ", "Tôi thức dậy lúc 7 giờ sáng.", new[] { "しちじ", "に", "おきます" }, new[] { "わたしはごぜんしちじにおきます。", "ごぜんしちじにおきます。", "しちじにおきます。", "午前七時に起きます。" }, "午前七時に起きます。", "gozen shichiji ni okimasu", "Đặt trợ từ に ngay sau しちじ."),
                new TrItem("ja-l21-tr-4", "hard", "Giờ làm việc", "Ngân hàng làm việc từ 9 giờ đến 3 giờ.", new[] { "ぎんこう", "くじから", "さんじまで" }, new[] { "ぎんこうはくじからさんじまでです。", "銀行は九時から三時までです。", "ぎんこうは9じから3じまでです。" }, "銀行は九時から三時までです。", "ginkou wa kuji kara sanji made desu", "Mẫu [Địa điểm] は [Giờ] から [Giờ] までです.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l21-gf-1", "easy", "Sai cách đọc 4 giờ", "いま、しじです。", "4 giờ trong tiếng Nhật đọc là よじ (không đọc しじ).", "いま、よじです。", new[] { "いま、よじです。", "いまよじです。", "今、四時です。" }, "ima, yoji desu", "Bây giờ là 4 giờ.", "Ba mốc giờ đặc biệt: 4h よじ, 7h しちじ, 9h くじ."),
                new GfItem("ja-l21-gf-2", "medium", "Dùng nhầm に sau あした", "あしたにろくじにおきます。", "Không gắn に sau あした (ngày mai).", "あしたろくじにおきます。", new[] { "あしたろくじにおきます。", "明日六時に起きます。" }, "ashita rokuji ni okimasu", "Ngày mai tôi dậy lúc 6 giờ.", "Chỉ dùng に sau mốc thời gian có số đếm cụ thể."),
                new GfItem("ja-l21-gf-3", "hard", "Đặt nhầm vị trí ごご", "いまさんじごごです。", "ごぜん/ごご luôn đứng TRƯỚC số giờ.", "いまごごさんじです。", new[] { "いまごごさんじです。", "今午後三時です。" }, "ima gogo sanji desu", "Bây giờ là 3 giờ chiều.", "Trật tự chuẩn tiếng Nhật: Từ lớn (buổi) → nhỏ (giờ → phút).")
            },
            roleplay: new[]
            {
                new RpItem("ja-l21-rp-1", "easy", "Hỏi giờ trên phố", "Một người qua đường hỏi bạn bây giờ là mấy giờ.", "Người đi đường", "すみません、いまなんじですか。", "sumimasen, ima nanji desu ka?", "Xin lỗi, bây giờ là mấy giờ ạ?", "Trả lời bây giờ là 9 giờ 30 phút sáng (ごぜんくじはん).", new[] { "ごぜん|午前|gozen", "くじ|九時|9時|kuji", "はん|さんじゅっぷん|半" }, new[] { "いまごぜんくじはんです。" }, "ima gozen kuji han desu.", 6),
                new RpItem("ja-l21-rp-2", "medium", "Xác nhận giờ họp", "Đồng nghiệp hỏi lịch họp hôm nay kéo dài từ mấy giờ đến mấy giờ.", "Anh Yamada", "きょうのかいぎはなんじからなんじまでですか。", "kyou no kaigi wa nanji kara nanji made desu ka?", "Cuộc họp hôm nay từ mấy giờ đến mấy giờ thế?", "Trả lời từ 10 giờ đến 11 giờ 30 phút bằng 〜から〜まで.", new[] { "じゅうじ|十時|10時|から", "まで|made" }, new[] { "じゅうじからじゅういちじはんまでです。" }, "juuji kara juuichiji han made desu.", 8),
                new RpItem("ja-l21-rp-3", "hard", "Hẹn gặp cuối tuần", "Bạn rủ đồng nghiệp gặp nhau vào thứ Bảy tuần này.", "Chị Mori", "どようびはじかんがありますよ。なんじにあいましょうか。", "doyoubi wa jikan ga arimasu yo. nanji ni aimashou ka?", "Thứ Bảy mình có thời gian đấy. Chúng ta gặp lúc mấy giờ nhé?", "Đề xuất gặp vào 2 giờ chiều (ごごにじに) bằng 〜ましょう.", new[] { "ごご|午後", "にじ|二時|2時", "に", "ましょう" }, new[] { "どようびのごごにじにあいましょう！" }, "doyoubi no gogo niji ni aimashou!", 8)
            }, now));

        // ---------------- LESSON 2.2 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.JapaneseLesson22Id, "ja-JP", "Nhật N5 · Bài 2.2",
            shadowing: new[]
            {
                new ShItem("ja-l22-sh-1", "easy", "Hỏi giá cơ bản (Dễ)", "これはいくらですか。", "これ は いくら です か / kore wa ikura desu ka", "Cái này bao nhiêu tiền ạ?", new[] { "これ", "いくら" }),
                new ShItem("ja-l22-sh-2", "easy", "Gọi món đơn giản (Dễ)", "おみずをください。", "おみず を ください / omizu o kudasai", "Làm ơn cho tôi nước lọc ạ.", new[] { "おみず", "ください" }),
                new ShItem("ja-l22-sh-3", "easy", "Cái này (この+N) (Dễ)", "このりんごはひゃくえんです。", "この りんご は ひゃくえん です / kono ringo wa hyakuen desu", "Quả táo này giá 100 yên.", new[] { "このりんご", "ひゃくえん" }),
                new ShItem("ja-l22-sh-4", "medium", "Biến âm 300 yên (Vừa)", "このコーヒーはさんびゃくえんです。", "この コーヒー は さんびゃくえん です / kono koohii wa sanbyakuen desu", "Ly cà phê này giá 300 yên.", new[] { "コーヒー", "さんびゃくえん" }),
                new ShItem("ja-l22-sh-5", "medium", "Đặt 2 món (Vừa)", "ラーメンをふたつください。", "ラーメン を ふたつ ください / raamen o futatsu kudasai", "Cho tôi 2 bát ramen ạ.", new[] { "ラーメン", "ふたつ", "ください" }),
                new ShItem("ja-l22-sh-6", "medium", "Xem đồ gần người bán (Vừa)", "すみません、そのかばんをみせてください。", "すみません、その かばん を みせて ください / sumimasen, sono kaban o misete kudasai", "Xin lỗi, cho tôi xem chiếc túi đó được không?", new[] { "そのかばん", "みせてください" }),
                new ShItem("ja-l22-sh-7", "hard", "Gọi kết hợp với と (Khó)", "ホットコーヒーをひとつとケーキをください。", "ホットコーヒー を ひとつ と ケーキ を ください / hotto koohii o hitotsu to keeki o kudasai", "Cho tôi 1 cà phê nóng và bánh ngọt ạ.", new[] { "ひとつ", "ケーキ", "ください" }),
                new ShItem("ja-l22-sh-8", "hard", "Hỏi thanh toán thẻ (Khó)", "クレジットカードでいいですか。", "クレジットカード で いい です か / kurejitto kaado de ii desu ka", "Thanh toán bằng thẻ tín dụng có được không ạ?", new[] { "クレジットカード", "いいですか" }),
                new ShItem("ja-l22-sh-9", "hard", "Biến âm 600 & 3000 yên (Khó)", "ぜんぶでさんぜんろっぴゃくえんです。", "ぜんぶ で さんぜん ろっぴゃくえん です / zenbu de sanzen roppyakuen desu", "Tổng cộng là 3.600 yên ạ.", new[] { "ぜんぶで", "さんぜん", "ろっぴゃくえん" })
            },
            dictation: new[]
            {
                new DiItem("ja-l22-di-1", "easy", "Hỏi giá", "これはいくらですか", "kore wa ikura desu ka", "Cái này bao nhiêu tiền?", new[] { "これ", "は", "いくら", "ですか" }, new[] { "それ", "なんじ", "どこ" }),
                new DiItem("ja-l22-di-2", "easy", "Gọi trà", "おちゃをください", "ocha o kudasai", "Cho tôi trà nhé.", new[] { "おちゃ", "を", "ください" }, new[] { "は", "が", "です" }),
                new DiItem("ja-l22-di-3", "medium", "Giá 300 yên", "このパンはさんびゃくえんです", "kono pan wa sanbyakuen desu", "Bánh mì này 300 yên.", new[] { "このパン", "は", "さんびゃくえん", "です" }, new[] { "その", "ひゃくえん", "か" }),
                new DiItem("ja-l22-di-4", "medium", "Gọi 2 phần", "コーヒーをふたつください", "koohii o futatsu kudasai", "Cho tôi 2 ly cà phê.", new[] { "コーヒー", "を", "ふたつ", "ください" }, new[] { "ひとつ", "みっつ", "は" }),
                new DiItem("ja-l22-di-5", "hard", "Xem món đồ kia", "あのかばんをみせてください", "ano kaban o misete kudasai", "Xin cho tôi xem chiếc túi kia.", new[] { "あの", "かばんを", "みせて", "ください" }, new[] { "あれ", "たべて", "のんで" }),
                new DiItem("ja-l22-di-6", "hard", "Tính tiền", "すみませんおかいけいをおねがいします", "sumimasen okaikei o onegaishimasu", "Xin lỗi, vui lòng tính tiền giúp tôi ạ.", new[] { "すみません", "おかいけいを", "おねがいします" }, new[] { "ありがとう", "ください", "です" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l22-pr-1", "easy", "Biến âm 300 yên", "さんびゃくえんです", "sanbyakuen desu", "Là 300 yên.", new[] { "さんびゃく", "えん" }, "Đọc âm đục びゃく thay vì ひゃく cho số 300.", new[] { "Đọc rõ âm đục びゃ", "Không ngắt trước えん", "Hạ nhẹ です" }, "さんびゃくえんです。", new[] { "さんびゃく" }),
                new PrItem("ja-l22-pr-2", "easy", "Trợ từ を đọc o", "みずをください", "mizu o kudasai", "Cho tôi nước.", new[] { "みず", "を", "ください" }, "Phát âm ず rõ ràng và đọc を là 'o'.", new[] { "Âm ず có rung thanh quản", "Đọc を nhẹ là o", "Đọc ください rõ ràng" }, "おみずをください。", new[] { "みず", "ください" }),
                new PrItem("ja-l22-pr-3", "medium", "Âm ngắt 600 & 800", "ろっぴゃくえんとはっぴゃくえん", "roppyakuen to happyakuen", "600 yên và 800 yên.", new[] { "ろっぴゃく", "はっぴゃく" }, "Nén hơi 1 phách ở っ rồi bật ぴゃ.", new[] { "Ngừng nhẹ sau ろ rồi bật ぴゃ", "Ngừng nhẹ sau は rồi bật ぴゃ", "Giữ nhịp đều cả câu" }, "ろっぴゃくえんです。", new[] { "ろっぴゃく" }),
                new PrItem("ja-l22-pr-4", "medium", "Gọi số lượng ひとつ/ふたつ", "りんごをみっつください", "ringo o mittsu kudasai", "Cho tôi 3 quả táo.", new[] { "りんご", "みっつ", "ください" }, "Kết hợp hàng r (り), âm ngắt っ và âm つ.", new[] { "Chạm nhẹ lưỡi cho り", "Dừng 1 nhịp ở っ trong みっつ", "Bật đầu lưỡi âm つ" }, "りんごをみっつください。", new[] { "りんご", "みっつ" }),
                new PrItem("ja-l22-pr-5", "hard", "Hỏi dùng thẻ", "カードではらえますか", "kaado de haraemasu ka", "Tôi có thể trả bằng thẻ không?", new[] { "カード", "はらえます" }, "Kéo dài trường âm カ thành kā và đọc ら nhẹ.", new[] { "Giữ trường âm ー của カード", "Đọc ら nhẹ giữa l và r", "Lên giọng nhẹ cuối か" }, "クレジットカードではらえますか。", new[] { "カード", "はらえます" }),
                new PrItem("ja-l22-pr-6", "hard", "Cảm ơn & chào khách", "いらっしゃいませぜんぶでせんごひゃくえんです", "irasshaimase zenbu de sen gohyakuen desu", "Kính chào quý khách, tổng cộng 1.500 yên ạ.", new[] { "いらっしゃいませ", "ぜんぶで", "ごひゃくえん" }, "Đọc trôi chảy âm ngắt っ và âm しゃ trong いらっしゃいませ.", new[] { "Dừng nhẹ っ trước しゃ", "Giữ rõ ぜ trong ぜんぶ", "Đọc せんごひゃく liền mạch" }, "ぜんぶでせんごひゃくえんです。", new[] { "ぜんぶで", "ごひゃくえん" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l22-wm-1", "easy", "Sơ đồ từ vựng: いくら (Bao nhiêu tiền)", "いくら", "いくら (ikura)", "Bao nhiêu tiền",
                    new[] { new WmNode("これはいくらですか", "kore wa ikura desu ka", "Cái này bao nhiêu tiền?", "すみません、これはいくらですか。"), new WmNode("ぜんぶでいくらですか", "zenbu de ikura desu ka", "Tổng cộng bao nhiêu tiền?", "ぜんぶでいくらですか。") },
                    new[] { new WmNode("たかい / やすい", "takai / yasui", "Đắt / Rẻ", "このみせはとてもやすいです。") },
                    new[] { new WmNode("おねだん", "onedan", "Giá cả (lịch sự)", "おねだんをおしえてください。") },
                    new[] { new WmEx("あのくつはいくらですか。", "ano kutsu wa ikura desu ka", "Đôi giày kia giá bao nhiêu ạ?") }),
                new WmItem("ja-l22-wm-2", "medium", "Sơ đồ từ vựng: ください (Làm ơn cho tôi)", "ください", "ください (kudasai)", "Xin hãy cho tôi / Vui lòng...",
                    new[] { new WmNode("これをください", "kore o kudasai", "Cho tôi lấy cái này", "じゃ、これをください。"), new WmNode("メニューをください", "menyuu o kudasai", "Cho tôi xin thực đơn", "すみません、メニューをください。") },
                    new[] { new WmNode("みせてください", "misete kudasai", "Cho tôi xem với", "そのシャツをみせてください。") },
                    new[] { new WmNode("おねがいします", "onegaishimasu", "Xin nhờ / Cho tôi (lịch sự hơn)", "コーヒーをひとつおねがいします。") },
                    new[] { new WmEx("すみません、おみずをもういっぱいください。", "sumimasen, omizu o mou ippai kudasai", "Xin lỗi, cho tôi thêm 1 ly nước nữa ạ.") }),
                new WmItem("ja-l22-wm-3", "hard", "Sơ đồ từ vựng: かいもの (Mua sắm)", "かいもの (買い物)", "かいもの (kaimono)", "Việc mua sắm",
                    new[] { new WmNode("かいものをする", "kaimono o suru", "Đi mua sắm", "スーパーでかいものをします。"), new WmNode("かいものにいく", "kaimono ni iku", "Đi để mua sắm", "ともだちとかいものにいきます。") },
                    new WmNode[] { new WmNode("レシートをもらう", "reshiito o morau", "Nhận hóa đơn", "レシートをください。") },
                    new[] { new WmNode("ショッピング", "shoppingu", "Shopping (từ mượn)", "しゅうまつにショッピングします。") },
                    new[] { new WmEx("デパートでかぞくのおみやげをかいました。", "depaato de kazoku no omiyage o kaimashita", "Tôi đã mua quà lưu niệm cho gia đình ở trung tâm thương mại.") })
            },
            cloze: new[]
            {
                new ClItem("ja-l22-cl-1", "easy", "Hỏi giá tiền", "Điền từ nghi vấn hỏi giá tiền:", "このコーヒーは{{0}}ですか。", new[] { new ClBlank(new[] { "いくら" }, "bao nhiêu tiền (ikura)", "いくら dùng riêng để hỏi giá tiền.") }, "このコーヒーはいくらですか。", "kono koohii wa ikura desu ka", "Ly cà phê này bao nhiêu tiền?", "Không dùng なん (cái gì) để hỏi giá."),
                new ClItem("ja-l22-cl-2", "easy", "Trợ từ tân ngữ を", "Điền trợ từ đứng trước ください:", "ラーメンをふたつと、おちゃ{{0}}ください。", new[] { new ClBlank(new[] { "を" }, "trợ từ tân ngữ (o)", "Mẫu gọi món: [Danh từ] を ください.") }, "ラーメンをふたつと、おちゃをください。", "raamen o futatsu to, ocha o kudasai", "Cho tôi 2 ramen và trà ạ.", "Nhớ dùng を trước ください."),
                new ClItem("ja-l22-cl-3", "medium", "Phân biệt これ và この", "Điền chỉ định từ đúng đứng trước danh từ かばん:", "{{0}}かばんはさんぜんえんです。", new[] { new ClBlank(new[] { "この", "その", "あの" }, "đứng trước danh từ: この/その/あの", "Trước danh từ phải dùng この/その/あの, không dùng これ.") }, "このかばんはさんぜんえんです。", "kono kaban wa sanzen en desu", "Chiếc túi này giá 3.000 yên.", "これ đứng độc lập, この luôn đi kèm danh từ."),
                new ClItem("ja-l22-cl-4", "hard", "Biến âm 600 yên", "Điền cách đọc Hiragana của '600 yên':", "ぜんぶで{{0}}です。", new[] { new ClBlank(new[] { "ろっぴゃくえん" }, "600 yên (roppyakuen)", "600 biến âm thành ろっぴゃく.") }, "ぜんぶでろっぴゃくえんです。", "zenbu de roppyakuen desu", "Tổng cộng là 600 yên.", "300 さんびゃく, 600 ろっぴゃく, 800 はっぴゃく.")
            },
            translate: new[]
            {
                new TrItem("ja-l22-tr-1", "easy", "Hỏi giá cái này", "Cái này bao nhiêu tiền?", new[] { "これ", "いくら" }, new[] { "これはいくらですか。", "これはいくらですか" }, "これはいくらですか。", "kore wa ikura desu ka", "これ dùng khi đồ vật ở gần người nói."),
                new TrItem("ja-l22-tr-2", "easy", "Gọi nước lọc", "Làm ơn cho tôi nước lọc.", new[] { "おみず", "を", "ください" }, new[] { "おみずをください。", "みずをください。", "お水をください。", "水をください。" }, "お水をください。", "omizu o kudasai", "Thêm お trước みず để lịch sự hơn khi gọi tại quán."),
                new TrItem("ja-l22-tr-3", "medium", "Cho xem chiếc túi đó", "Xin lỗi, cho tôi xem chiếc túi đó.", new[] { "すみません", "そのかばん", "みせてください" }, new[] { "すみません、そのかばんをみせてください。", "すみません、そのかばんを見せてください。", "そのかばんをみせてください。" }, "すみません、そのかばんを見せてください。", "sumimasen, sono kaban o misete kudasai", "Đồ vật ở gần người bán nên dùng その (đó)."),
                new TrItem("ja-l22-tr-4", "hard", "Đặt 2 món", "Cho tôi 1 ly cà phê và 2 quả táo.", new[] { "コーヒー", "ひとつ", "りんご", "ふたつ", "ください" }, new[] { "コーヒーをひとつとりんごをふたつください。", "コーヒーをひとつと、りんごをふたつください。", "コーヒーひとつとりんごふたつください。" }, "コーヒーをひとつと、りんごをふたつください。", "koohii o hitotsu to, ringo o futatsu kudasai", "Dùng と để nối hai danh từ cần mua.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l22-gf-1", "easy", "Nhầm これ với この trước danh từ", "これかばんはいくらですか。", "Trước danh từ かばん phải dùng この (không dùng これ).", "このかばんはいくらですか。", new[] { "このかばんはいくらですか。", "この鞄はいくらですか。" }, "kono kaban wa ikura desu ka", "Chiếc túi này bao nhiêu tiền?", "これ đứng độc lập; この/その/あの bổ nghĩa cho danh từ phía sau."),
                new GfItem("ja-l22-gf-2", "medium", "Thiếu trợ từ を trước ください", "コーヒーはください。", "Mẫu gọi món dùng trợ từ を (không dùng は).", "コーヒーをください。", new[] { "コーヒーをください。" }, "koohii o kudasai", "Cho tôi ly cà phê ạ.", "Mẫu chuẩn N5: [Danh từ] を ください."),
                new GfItem("ja-l22-gf-3", "hard", "Sai biến âm 300 yên", "これはさんひゃくえんです。", "300 biến âm thành さんびゃく (không phải さんひゃく).", "これはさんびゃくえんです。", new[] { "これはさんびゃくえんです。", "これは三百円です。" }, "kore wa sanbyakuen desu", "Cái này giá 300 yên.", "300 = さんびゃく, 600 = ろっぴゃく, 800 = はっぴゃく.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l22-rp-1", "easy", "Hỏi giá ở cửa hàng tiện lợi", "Bạn cầm một hộp cơm Bento và muốn hỏi nhân viên giá bao nhiêu.", "Nhân viên Konbini", "いらっしゃいませ！なにかおさがしですか。", "irasshaimase! nanika osagashi desu ka?", "Xin chào quý khách! Quý khách đang tìm gì ạ?", "Xin lỗi và hỏi cái này bao nhiêu tiền (これはいくらですか).", new[] { "すみません|sumimasen", "これ|この|kore|kono", "いくら|ikura" }, new[] { "すみません、これはいくらですか。" }, "sumimasen, kore wa ikura desu ka?", 6),
                new RpItem("ja-l22-rp-2", "medium", "Gọi đồ uống tại quán cà phê", "Nhân viên quán cà phê mời bạn chọn đồ uống.", "Nhân viên quán", "ごちゅうもんはおきまりですか。", "gochuumon wa okimari desu ka?", "Quý khách đã chọn được món chưa ạ?", "Gọi 1 cà phê (コーヒー) hoặc trà (おちゃ) với số lượng ひとつ/ふたつ và ください.", new[] { "コーヒー|おちゃ|ラーメン", "ひとつ|ふたつ", "ください|おねがいします" }, new[] { "ホットコーヒーをひとつと、サンドイッチをください。" }, "hotto koohii o hitotsu to, sandoicchi o kudasai.", 8),
                new RpItem("ja-l22-rp-3", "hard", "Mua đồ & thanh toán bằng thẻ", "Nhân viên báo giá chiếc túi là 3.500 yên, bạn đồng ý mua và hỏi trả bằng thẻ.", "Nhân viên bán hàng", "このかばんはさんぜんごひゃくえんです。", "kono kaban wa sanzen gohyakuen desu.", "Chiếc túi này giá 3.500 yên ạ.", "Nói bạn sẽ lấy cái này (これをください) và hỏi trả bằng thẻ (カード) có được không.", new[] { "これ|ください", "カード|クレジットカード", "いいですか|はらえますか" }, new[] { "じゃ、これをください。クレジットカードでいいですか。" }, "ja, kore o kudasai. kurejitto kaado de ii desu ka?", 10)
            }, now));

        // ---------------- LESSON 3.1 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.JapaneseLesson31Id, "ja-JP", "Nhật N5 · Bài 3.1",
            shadowing: new[]
            {
                new ShItem("ja-l31-sh-1", "easy", "Hỏi vị trí nhà ga (Dễ)", "すみません、えきはどこですか。", "すみません、えき は どこ です か / sumimasen, eki wa doko desu ka", "Xin lỗi, nhà ga ở đâu vậy ạ?", new[] { "えき", "どこ" }),
                new ShItem("ja-l31-sh-2", "easy", "Đi bằng tàu điện (Dễ)", "でんしゃでえきへいきます。", "でんしゃ で えき へ いきます / densha de eki e ikimasu", "Tôi đi đến nhà ga bằng tàu điện.", new[] { "でんしゃで", "いきます" }),
                new ShItem("ja-l31-sh-3", "easy", "Chỉ vị trí (Dễ)", "トイレはあそこにあります。", "トイレ は あそこ に あります / toire wa asoko ni arimasu", "Nhà vệ sinh ở đằng kia ạ.", new[] { "トイレ", "あそこ", "あります" }),
                new ShItem("ja-l31-sh-4", "medium", "Đi thẳng & rẽ trái (Vừa)", "まっすぐいって、ひだりにまがってください。", "まっすぐ いって、ひだり に まがって ください / massugu itte, hidari ni magatte kudasai", "Hãy đi thẳng rồi rẽ trái nhé.", new[] { "まっすぐ", "ひだり", "まがって" }),
                new ShItem("ja-l31-sh-5", "medium", "Đồ vật vs Người (Vừa)", "えきのまえにともだちがいます。", "えき の まえ に ともだち が います / eki no mae ni tomodachi ga imasu", "Có bạn tôi đang ở trước nhà ga.", new[] { "えきのまえ", "ともだち", "います" }),
                new ShItem("ja-l31-sh-6", "medium", "Hỏi thời gian đi bộ (Vừa)", "ここからあるいてなんぷんかかりますか。", "ここ から あるいて なんぷん かかります か / koko kara aruite nanpun kakarimasu ka", "Từ đây đi bộ mất mấy phút ạ?", new[] { "あるいて", "なんぷん", "かかります" }),
                new ShItem("ja-l31-sh-7", "hard", "Mô tả cạnh cửa hàng (Khó)", "ゆうびんきょくはぎんこうのとなりにあります。", "ゆうびんきょく は ぎんこう の となり に あります / yuubinkyoku wa ginkou no tonari ni arimasu", "Bưu điện nằm ở bên cạnh ngân hàng.", new[] { "ゆうびんきょく", "となり", "あります" }),
                new ShItem("ja-l31-sh-8", "hard", "Mua vé tàu (Khó)", "しんじゅくまでのきっぷをいちまいください。", "しんじゅく まで の きっぷ を いちまい ください / Shinjuku made no kippu o ichimai kudasai", "Cho tôi 1 vé đi đến Shinjuku ạ.", new[] { "まで", "きっぷ", "ください" }),
                new ShItem("ja-l31-sh-9", "hard", "Chỉ đường đầy đủ (Khó)", "つぎのしんごうをみぎにまがると、えきがみえます。", "つぎ の しんごう を みぎ に まがる と、えき が みえます / tsugi no shingou o migi ni magaru to, eki ga miemasu", "Rẽ phải ở đèn giao thông tiếp theo sẽ thấy nhà ga.", new[] { "しんごう", "みぎ", "みえます" })
            },
            dictation: new[]
            {
                new DiItem("ja-l31-di-1", "easy", "Hỏi nhà ga ở đâu", "えきはどこですか", "eki wa doko desu ka", "Nhà ga ở đâu ạ?", new[] { "えき", "は", "どこ", "ですか" }, new[] { "だれ", "いつ", "いくら" }),
                new DiItem("ja-l31-di-2", "easy", "Đi bằng xe buýt", "バスでいきます", "basu de ikimasu", "Tôi đi bằng xe buýt.", new[] { "バス", "で", "いきます" }, new[] { "を", "が", "きます" }),
                new DiItem("ja-l31-di-3", "medium", "Đi thẳng", "このみちをまっすぐいってください", "kono michi o massugu itte kudasai", "Hãy đi thẳng con đường này.", new[] { "このみちを", "まっすぐ", "いって", "ください" }, new[] { "みぎ", "ひだり", "みます" }),
                new DiItem("ja-l31-di-4", "medium", "Bên cạnh ngân hàng", "ぎんこうのとなりにあります", "ginkou no tonari ni arimasu", "Ở bên cạnh ngân hàng.", new[] { "ぎんこうの", "となりに", "あります" }, new[] { "います", "まえ", "で" }),
                new DiItem("ja-l31-di-5", "hard", "Rẽ phải ở góc đường", "つぎのかどをみぎにまがってください", "tsugi no kado o migi ni magatte kudasai", "Hãy rẽ phải ở góc đường tiếp theo.", new[] { "つぎの", "かどを", "みぎに", "まがってください" }, new[] { "ひだり", "まっすぐ", "あります" }),
                new DiItem("ja-l31-di-6", "hard", "Đi bộ khoảng 5 phút", "あるいてごふんぐらいかかります", "aruite gofun gurai kakarimasu", "Đi bộ mất khoảng 5 phút.", new[] { "あるいて", "ごふん", "ぐらい", "かかります" }, new[] { "でんしゃ", "から", "います" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l31-pr-1", "easy", "Âm ngắt まっすぐ", "まっすぐいってください", "massugu itte kudasai", "Xin hãy đi thẳng.", new[] { "まっすぐ", "いって" }, "Giữ âm ngắt っ rõ trong cả まっすぐ và いって.", new[] { "Dừng nhẹ giữa ま và す", "Dừng nhẹ giữa い và て", "Giữ giọng tự nhiên cuối câu" }, "まっすぐいってください。", new[] { "まっすぐ", "いって" }),
                new PrItem("ja-l31-pr-2", "easy", "Phương hướng trái/phải", "ひだりにまがってください", "hidari ni magatte kudasai", "Hãy rẽ trái.", new[] { "ひだり", "まがって" }, "Chạm nhẹ lưỡi âm り và ngắt rõ っ trong まがって.", new[] { "Đọc り nhẹ không thành 'di'", "Dừng 1 nhịp ở っ của まがって", "Đọc ください rõ ràng" }, "ひだりにまがってください。", new[] { "ひだり", "まがって" }),
                new PrItem("ja-l31-pr-3", "medium", "Âm ngắt きっぷ", "きっぷをかいます", "kippu o kaimasu", "Tôi mua vé tàu.", new[] { "きっぷ", "かいます" }, "Bật hơi âm ぷ sau âm ngắt っ thật gọn.", new[] { "Nén hơi 1 nhịp ở っ", "Bật nhẹ ぷ", "Đọc を thành 'o'" }, "えきできっぷをかいます。", new[] { "きっぷ", "かいます" }),
                new PrItem("ja-l31-pr-4", "medium", "Trợ từ へ đọc e", "でんしゃでとうきょうへいきます", "densha de Toukyou e ikimasu", "Tôi đi đến Tokyo bằng tàu điện.", new[] { "でんしゃ", "とうきょう", "へ" }, "Kéo dài trường âm とうきょう và đọc trợ từ へ thành 'e'.", new[] { "Đọc しゃ mềm", "Kéo dài cả とう và きょう", "Đọc へ thành e (không đọc he)" }, "でんしゃでとうきょうへいきます。", new[] { "でんしゃ", "とうきょう" }),
                new PrItem("ja-l31-pr-5", "hard", "Trường âm ゆうびんきょく", "ゆうびんきょくはえきのまえにあります", "yuubinkyoku wa eki no mae ni arimasu", "Bưu điện nằm trước nhà ga.", new[] { "ゆうびんきょく", "えき", "あります" }, "Kéo dài ゆう và đọc rõ âm ghép きょく.", new[] { "Kéo dài ゆう đủ 2 phách", "Đọc gọn きょく", "Chạm nhẹ lưỡi cho り của あります" }, "ゆうびんきょくはえきのまえにあります。", new[] { "ゆうびんきょく", "あります" }),
                new PrItem("ja-l31-pr-6", "hard", "Hỏi đường đầy đủ", "つぎのしんごうをひだりにまがってください", "tsugi no shingou o hidari ni magatte kudasai", "Hãy rẽ trái ở đèn giao thông tiếp theo.", new[] { "つぎ", "しんごう", "ひだり" }, "Luyện tổ hợp つ, し, trường âm ごう và âm ngắt っ.", new[] { "Bật đầu lưỡi つ của つぎ", "Kéo dài ごう thành gō", "Đọc rõ り và っ" }, "つぎのしんごうをひだりにまがってください。", new[] { "つぎ", "しんごう", "ひだり" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l31-wm-1", "easy", "Sơ đồ từ vựng: えき (Nhà ga)", "えき (駅)", "えき (eki)", "Nhà ga tàu điện",
                    new[] { new WmNode("えきのまえ", "eki no mae", "Phía trước nhà ga", "えきのまえでまちます。"), new WmNode("えきまであるく", "eki made aruku", "Đi bộ ra ga", "いえからえきまであるきます。") },
                    new[] { new WmNode("えきいん", "ekiin", "Nhân viên nhà ga", "えきいんにききます。") },
                    new WmNode[] { new WmNode("バスてい", "basutei", "Trạm xe buýt", "バスていはあそこです。") },
                    new[] { new WmEx("すみません、いちばんちかいえきはどこですか。", "sumimasen, ichiban chikai eki wa doko desu ka", "Xin lỗi, nhà ga gần nhất ở đâu ạ?") }),
                new WmItem("ja-l31-wm-2", "medium", "Sơ đồ từ vựng: いきます (Đi)", "いきます (行きます)", "いきます (ikimasu)", "Đi (tới địa điểm nào đó)",
                    new[] { new WmNode("でんしゃでいきます", "densha de ikimasu", "Đi bằng tàu điện", "まいにちでんしゃでいきます。"), new WmNode("あるいていきます", "aruite ikimasu", "Đi bộ tới", "ちかいのであるいていきます。") },
                    new[] { new WmNode("まっすぐいってください", "massugu itte kudasai", "Xin hãy đi thẳng", "このみちをまっすぐいってください。") },
                    new[] { new WmNode("きます / かえります", "kimasu / kaerimasu", "Đến / Về nhà", "ろくじにいえへかえります。") },
                    new[] { new WmEx("あしたともだちときょうとへいきます。", "ashita tomodachi to Kyouto e ikimasu", "Ngày mai tôi đi Kyoto cùng bạn.") }),
                new WmItem("ja-l31-wm-3", "hard", "Sơ đồ từ vựng: あります / います (Có, tồn tại)", "あります / います", "あります / います (arimasu / imasu)", "Có / Ở (đồ vật vs người)",
                    new[] { new WmNode("となりにあります", "tonari ni arimasu", "Nằm ở bên cạnh (vật/nơi)", "ぎんこうのとなりにあります。"), new WmNode("へやにいます", "heya ni imasu", "Đang ở trong phòng (người)", "たなかさんはへやにいます。") },
                    new[] { new WmNode("ちかくにあります", "chikaku ni arimasu", "Nằm ở gần đây", "えきのちかくにホテルがあります。") },
                    new[] { new WmNode("みえます", "miemasu", "Nhìn thấy được", "みぎにコンビニがみえます。") },
                    new[] { new WmEx("こうえんにいぬとこどもがいます。", "kouen ni inu to kodomo ga imasu", "Trong công viên có chó và trẻ em.") })
            },
            cloze: new[]
            {
                new ClItem("ja-l31-cl-1", "easy", "Trợ từ phương tiện で", "Điền trợ từ chỉ phương tiện giao thông:", "わたしはでんしゃ{{0}}がっこうへいきます。", new[] { new ClBlank(new[] { "で" }, "bằng (phương tiện)", "Dùng で sau danh từ chỉ phương tiện giao thông.") }, "わたしはでんしゃでがっこうへいきます。", "watashi wa densha de gakkou e ikimasu", "Tôi đi học bằng tàu điện.", "Riêng đi bộ là あるいていきます (không dùng で)."),
                new ClItem("ja-l31-cl-2", "easy", "Hỏi địa điểm ở đâu", "Điền từ hỏi 'ở đâu':", "すみません、トイレは{{0}}ですか。", new[] { new ClBlank(new[] { "どこ", "どちら" }, "ở đâu (doko)", "どこ = ở đâu (lịch sự hơn là どちら).") }, "すみません、トイレはどこですか。", "sumimasen, toire wa doko desu ka", "Xin lỗi, nhà vệ sinh ở đâu ạ?", "Mẫu hỏi vị trí: [Địa điểm] はどこですか。"),
                new ClItem("ja-l31-cl-3", "medium", "Phân biệt あります và います", "Chọn động từ tồn tại đúng cho 'cửa hàng tiện lợi' (địa điểm):", "えきのまえにコンビニが{{0}}。", new[] { new ClBlank(new[] { "あります" }, "tồn tại cho vật/địa điểm", "Địa điểm/đồ vật luôn dùng あります (người/động vật mới dùng います).") }, "えきのまえにコンビニがあります。", "eki no mae ni konbini ga arimasu", "Trước nhà ga có cửa hàng tiện lợi.", "Phân biệt chuẩn あります (vật) vs います (người/động vật)."),
                new ClItem("ja-l31-cl-4", "hard", "Chỉ đường đi thẳng & rẽ phải", "Điền từ 'đi thẳng' và 'bên phải':", "{{0}}いって、つぎのかどを{{1}}にまがってください。", new[] { new ClBlank(new[] { "まっすぐ" }, "đi thẳng (massugu)", "まっすぐ = đi thẳng."), new ClBlank(new[] { "みぎ", "右" }, "bên phải (migi)", "みぎ = bên phải, ひだり = bên trái.") }, "まっすぐいって、つぎのかどをみぎにまがってください。", "massugu itte, tsugi no kado o migi ni magatte kudasai", "Đi thẳng rồi rẽ phải ở góc đường tiếp theo nhé.", "Hai cụm chỉ đường quan trọng nhất trong giao tiếp du lịch Nhật Bản.")
            },
            translate: new[]
            {
                new TrItem("ja-l31-tr-1", "easy", "Hỏi vị trí nhà ga", "Xin lỗi, nhà ga ở đâu ạ?", new[] { "すみません", "えき", "どこ" }, new[] { "すみません、えきはどこですか。", "すみません、駅はどこですか。" }, "すみません、駅はどこですか。", "sumimasen, eki wa doko desu ka", "Mở lời bằng すみません khi hỏi đường người lạ."),
                new TrItem("ja-l31-tr-2", "easy", "Đi bằng tàu điện", "Tôi đi đến trường bằng tàu điện.", new[] { "でんしゃで", "がっこう", "いきます" }, new[] { "わたしはでんしゃでがっこうへいきます。", "でんしゃでがっこうへいきます。", "電車で学校へ行きます。", "電車で学校に行きます。" }, "電車で学校へ行きます。", "densha de gakkou e ikimasu", "Phương tiện + で, địa điểm đến + へ/に."),
                new TrItem("ja-l31-tr-3", "medium", "Ở bên cạnh ngân hàng", "Bưu điện ở bên cạnh ngân hàng.", new[] { "ゆうびんきょく", "ぎんこう", "となり", "あります" }, new[] { "ゆうびんきょくはぎんこうのとなりにあります。", "郵便局は銀行の隣にあります。" }, "郵便局は銀行の隣にあります。", "yuubinkyoku wa ginkou no tonari ni arimasu", "Cấu trúc: A は B の [vị trí] にあります."),
                new TrItem("ja-l31-tr-4", "hard", "Đi thẳng rồi rẽ trái", "Hãy đi thẳng rồi rẽ trái.", new[] { "まっすぐ", "いって", "ひだり", "まがってください" }, new[] { "まっすぐいって、ひだりにまがってください。", "まっすぐ行って、左に曲がってください。" }, "まっすぐ行って、左に曲がってください。", "massugu itte, hidari ni magatte kudasai", "Dùng thể て (いって) để nối hai hành động liên tiếp.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l31-gf-1", "easy", "Nhầm います với あります cho địa điểm", "えきのまえにコンビニがいます。", "Cửa hàng tiện lợi là địa điểm/vật nên dùng あります.", "えきのまえにコンビニがあります。", new[] { "えきのまえにコンビニがあります。", "駅の前にコンビニがあります。" }, "eki no mae ni konbini ga arimasu", "Trước nhà ga có cửa hàng tiện lợi.", "います chỉ dùng cho người và động vật."),
                new GfItem("ja-l31-gf-2", "medium", "Dùng thừa で với đi bộ", "あるいてでえきへいきます。", "あるいて (đi bộ) đã là động từ thể て, không dùng trợ từ で.", "あるいてえきへいきます。", new[] { "あるいてえきへいきます。", "歩いて駅へ行きます。", "歩いて駅に行きます。" }, "aruite eki e ikimasu", "Tôi đi bộ ra nhà ga.", "Chỉ dùng で với danh từ phương tiện như バスで, でんしゃで."),
                new GfItem("ja-l31-gf-3", "hard", "Sai trợ từ phương tiện に", "わたしはバスにいきます。", "Đi bằng phương tiện gì phải dùng trợ từ で.", "わたしはバスでいきます。", new[] { "わたしはバスでいきます。", "私はバスで行きます。" }, "watashi wa basu de ikimasu", "Tôi đi bằng xe buýt.", "Trợ từ で chỉ công cụ / phương tiện di chuyển.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l31-rp-1", "easy", "Hỏi đường tới ga Tokyo", "Bạn đang đứng trên phố và muốn hỏi đường đến ga gần nhất.", "Người dân địa phương", "はい、どうかしましたか。", "hai, douka shimashita ka?", "Vâng, có chuyện gì thế bạn?", "Xin lỗi lịch sự và hỏi nhà ga (えき) ở đâu (どこですか).", new[] { "すみません", "えき|駅", "どこ" }, new[] { "すみません、えきはどこですか。" }, "sumimasen, eki wa doko desu ka?", 6),
                new RpItem("ja-l31-rp-2", "medium", "Hỏi thời gian đi bộ", "Người dân vừa chỉ đường rẽ trái ở ngã tư tiếp theo.", "Người dân địa phương", "つぎのかどをひだりにまがってください。すぐですよ。", "tsugi no kado o hidari ni magatte kudasai. sugu desu yo.", "Hãy rẽ trái ở góc đường tiếp theo. Gần lắm.", "Hỏi xem đi bộ (あるいて) mất mấy phút (なんぷん) và cảm ơn.", new[] { "あるいて|歩いて", "なんぷん|何分|かかります", "ありがとう" }, new[] { "あるいてなんぷんかかりますか。ありがとうございます！" }, "aruite nanpun kakarimasu ka? arigatou gozaimasu!", 8),
                new RpItem("ja-l31-rp-3", "hard", "Mua vé tàu tại quầy", "Nhân viên nhà ga hỏi bạn muốn mua vé đi đâu.", "Nhân viên ga", "どちらまでいらっしゃいますか。", "dochira made irasshaimasu ka?", "Quý khách muốn đi tới ga nào ạ?", "Nói bạn muốn mua 1 vé (きっぷ) đến Shinjuku hoặc Tokyo bằng 〜をください.", new[] { "まで", "きっぷ|切符", "ください|おねがいします" }, new[] { "とうきょうまでのきっぷをいちまいください。" }, "Toukyou made no kippu o ichimai kudasai.", 8)
            }, now));

        // ---------------- LESSON 3.2 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.JapaneseLesson32Id, "ja-JP", "Nhật N5 · Bài 3.2",
            shadowing: new[]
            {
                new ShItem("ja-l32-sh-1", "easy", "Thói quen buổi sáng (Dễ)", "まいあさあさごはんをたべます。", "まいあさ あさごはん を たべます / maiasa asagohan o tabemasu", "Mỗi sáng tôi ăn bữa sáng.", new[] { "まいあさ", "あさごはん", "たべます" }),
                new ShItem("ja-l32-sh-2", "easy", "Nói sở thích (Dễ)", "わたしはおんがくがすきです。", "わたし は おんがく が すき です / watashi wa ongaku ga suki desu", "Tôi thích âm nhạc.", new[] { "おんがく", "すき" }),
                new ShItem("ja-l32-sh-3", "easy", "Học mỗi ngày (Dễ)", "まいにちにほんごをべんきょうします。", "まいにち にほんご を べんきょう します / mainichi nihongo o benkyou shimasu", "Mỗi ngày tôi đều học tiếng Nhật.", new[] { "まいにち", "にほんご", "べんきょうします" }),
                new ShItem("ja-l32-sh-4", "medium", "Địa điểm hành động で (Vừa)", "としょかんでほんをよみます。", "としょかん で ほん を よみます / toshokan de hon o yomimasu", "Tôi đọc sách ở thư viện.", new[] { "としょかんで", "ほん", "よみます" }),
                new ShItem("ja-l32-sh-5", "medium", "Lời mời 〜ませんか (Vừa)", "いっしょにひるごはんをたべませんか。", "いっしょ に ひるごはん を たべません か / issho ni hirugohan o tabemasen ka", "Bạn có muốn cùng ăn trưa không?", new[] { "いっしょに", "たべませんか" }),
                new ShItem("ja-l32-sh-6", "medium", "Hưởng ứng 〜ましょう (Vừa)", "ええ、いいですね。いきましょう！", "ええ、いい です ね。いきましょう / ee, ii desu ne. ikimashou", "Ừ, hay quá. Chúng ta cùng đi nhé!", new[] { "いいですね", "いきましょう" }),
                new ShItem("ja-l32-sh-7", "hard", "Sở thích là làm gì (Khó)", "わたしのしゅみはえいがをみることです。", "わたし の しゅみ は えいが を みる こと です / watashi no shumi wa eiga o miru koto desu", "Sở thích của tôi là xem phim.", new[] { "しゅみ", "えいが", "みること" }),
                new ShItem("ja-l32-sh-8", "hard", "Quá khứ 〜ました (Khó)", "きのうともだちとカフェでコーヒーをのみました。", "きのう ともだち と カフェ で コーヒー を のみました / kinou tomodachi to kafe de koohii o nomimashita", "Hôm qua tôi đã uống cà phê ở quán với bạn.", new[] { "きのう", "カフェで", "のみました" }),
                new ShItem("ja-l32-sh-9", "hard", "Đề nghị giúp đỡ (Khó)", "にもつをもちましょうか。ありがとうございます。", "にもつ を もちましょう か。ありがとう ございます / nimotsu o mochimashou ka. arigatou gozaimasu", "Để tôi xách hành lý giúp bạn nhé? Cảm ơn bạn.", new[] { "にもつ", "もちましょうか" })
            },
            dictation: new[]
            {
                new DiItem("ja-l32-di-1", "easy", "Thích tiếng Nhật", "にほんごがすきです", "nihongo ga suki desu", "Tôi thích tiếng Nhật.", new[] { "にほんご", "が", "すき", "です" }, new[] { "を", "で", "ます" }),
                new DiItem("ja-l32-di-2", "easy", "Học mỗi ngày", "まいにちべんきょうします", "mainichi benkyou shimasu", "Mỗi ngày tôi học bài.", new[] { "まいにち", "べんきょう", "します" }, new[] { "たべます", "みます", "きのう" }),
                new DiItem("ja-l32-di-3", "medium", "Đọc sách ở thư viện", "としょかんでほんをよみます", "toshokan de hon o yomimasu", "Tôi đọc sách ở thư viện.", new[] { "としょかん", "で", "ほんを", "よみます" }, new[] { "に", "みます", "ききます" }),
                new DiItem("ja-l32-di-4", "medium", "Mời cùng ăn cơm", "いっしょにたべませんか", "issho ni tabemasen ka", "Cùng ăn nhé?", new[] { "いっしょに", "たべません", "か" }, new[] { "たべます", "です", "で" }),
                new DiItem("ja-l32-di-5", "hard", "Sở thích xem phim", "しゅみはえいがをみることです", "shumi wa eiga o miru koto desu", "Sở thích là xem phim.", new[] { "しゅみは", "えいがを", "みること", "です" }, new[] { "おんがく", "きく", "が" }),
                new DiItem("ja-l32-di-6", "hard", "Cuối tuần đi chơi", "しゅうまつにいっしょにえいがをみませんか", "shuumatsu ni issho ni eiga o mimasen ka", "Cuối tuần bạn có muốn cùng đi xem phim không?", new[] { "しゅうまつに", "いっしょに", "えいがを", "みませんか" }, new[] { "きのう", "みました", "ます" })
            },
            pronunciation: new[]
            {
                new PrItem("ja-l32-pr-1", "easy", "Trường âm べんきょう", "にほんごをべんきょうします", "nihongo o benkyou shimasu", "Tôi học tiếng Nhật.", new[] { "にほんご", "べんきょう" }, "Kéo dài đúng trường âm きょう (kyō) và đọc し mềm.", new[] { "Giữ rõ âm mũi ん", "Kéo dài きょう đủ 2 phách", "Đọc します nhẹ gọn" }, "まいにちにほんごをべんきょうします。", new[] { "にほんご", "べんきょう" }),
                new PrItem("ja-l32-pr-2", "easy", "Âm ngắt いっしょに", "いっしょにいきましょう", "issho ni ikimashou", "Chúng ta cùng đi nhé!", new[] { "いっしょに", "いきましょう" }, "Dừng 1 nhịp ở っ trong いっしょ và kéo dài しょう.", new[] { "Nén hơi っ trước しょ", "Kéo dài しょう thành shō", "Giọng vui vẻ tự nhiên" }, "いっしょにいきましょう！", new[] { "いっしょに", "いきましょう" }),
                new PrItem("ja-l32-pr-3", "medium", "Trường âm えいが", "えいがをみるのがすきです", "eiga o miru no ga suki desu", "Tôi thích xem phim.", new[] { "えいが", "みる", "すき" }, "Kéo dài えい thành ē và đọc る nhẹ.", new[] { "Kéo dài えい không tách rời e-i", "Chạm lưỡi nhẹ ở る", "Đọc す nhẹ không bật thành つ" }, "えいがをみるのがすきです。", new[] { "えいが", "すき" }),
                new PrItem("ja-l32-pr-4", "medium", "Quá khứ きのう", "きのうカフェでべんきょうしました", "kinou kafe de benkyou shimashita", "Hôm qua tôi đã học bài ở quán cà phê.", new[] { "きのう", "べんきょう", "しました" }, "Kéo dài cả のう và きょう trong cùng một câu.", new[] { "Kéo dài きのう", "Đọc カフェ gọn 2 phách", "Nhấn nhẹ đuôi quá khứ しました" }, "きのうカフェでべんきょうしました。", new[] { "きのう", "べんきょう" }),
                new PrItem("ja-l32-pr-5", "hard", "Trường âm しゅうまつ", "しゅうまつにおんがくをききます", "shuumatsu ni ongaku o kikimasu", "Cuối tuần tôi nghe nhạc.", new[] { "しゅうまつ", "おんがく", "ききます" }, "Kéo dài しゅう và bật rõ つ trong しゅうまつ.", new[] { "しゅう kéo dài 2 nhịp", "Bật đầu lưỡi つ cuối ま-つ", "Đọc mượt おんがく" }, "しゅうまつにおんがくをききます。", new[] { "しゅうまつ", "おんがく" }),
                new PrItem("ja-l32-pr-6", "hard", "Đề nghị giúp đỡ", "にもつをもちましょうか", "nimotsu o mochimashou ka", "Để tôi xách hành lý giúp bạn nhé?", new[] { "にもつ", "もちましょう" }, "Phân biệt つ (にもつ) và ち (もち) kế nhau.", new[] { "Đọc つ bật đầu lưỡi", "Đọc ち khác し", "Kéo dài ましょう rồi lên giọng nhẹ ở か" }, "にもつをもちましょうか。", new[] { "にもつ", "もちましょう" })
            },
            wordMaps: new[]
            {
                new WmItem("ja-l32-wm-1", "easy", "Sơ đồ từ vựng: すき (Thích)", "すき (好き)", "すき (suki)", "Thích, yêu thích",
                    new[] { new WmNode("にほんごがすきです", "nihongo ga suki desu", "Thích tiếng Nhật", "わたしはにほんごがすきです。"), new WmNode("だいすきです", "daisuki desu", "Rất thích, mê tít", "ラーメンがだいすきです。") },
                    new[] { new WmNode("すきな〜", "suki na ~", "... mà tôi thích", "すきなたべものはすしです。") },
                    new[] { new WmNode("しゅみ", "shumi", "Sở thích", "しゅみはどくしょです。") },
                    new[] { new WmEx("どんなおんがくがすきですか。", "donna ongaku ga suki desu ka", "Bạn thích thể loại âm nhạc nào?") }),
                new WmItem("ja-l32-wm-2", "medium", "Sơ đồ từ vựng: べんきょう (Học tập)", "べんきょう (勉強)", "べんきょう (benkyou)", "Việc học tập",
                    new[] { new WmNode("べんきょうします", "benkyou shimasu", "Học bài", "まいばんいちじかんべんきょうします。"), new WmNode("べんきょうちゅう", "benkyouchuu", "Đang trong lúc học", "いまべんきょうちゅうです。") },
                    new[] { new WmNode("いっしょにべんきょうする", "issho ni benkyou suru", "Cùng nhau học tập", "としょかんでいっしょにべんきょうしませんか。") },
                    new[] { new WmNode("ならいます", "naraimasu", "Học (từ ai đó)", "たなかせんせいににほんごをならいます。") },
                    new[] { new WmEx("まいにちすこしずつにほんごをべんきょうしています。", "mainichi sukoshi zutsu nihongo o benkyou shite imasu", "Mỗi ngày tôi đều học tiếng Nhật từng chút một.") }),
                new WmItem("ja-l32-wm-3", "hard", "Sơ đồ từ vựng: いっしょに (Cùng nhau)", "いっしょに (一緒に)", "いっしょに (issho ni)", "Cùng nhau",
                    new[] { new WmNode("いっしょにいきましょう", "issho ni ikimashou", "Cùng đi nào!", "あしたいっしょにいきましょう。"), new WmNode("いっしょにたべませんか", "issho ni tabemasen ka", "Bạn có muốn cùng ăn không?", "ひるごはんをいっしょにたべませんか。") },
                    new[] { new WmNode("〜といっしょに", "~ to issho ni", "Cùng với ai đó", "かぞくといっしょにりょこうします。") },
                    new[] { new WmNode("みんなで", "minna de", "Tất cả mọi người cùng", "みんなでしゃしんをとりましょう。") },
                    new[] { new WmEx("しゅうまつにともだちといっしょにえいがをみました。", "shuumatsu ni tomodachi to issho ni eiga o mimashita", "Cuối tuần tôi đã đi xem phim cùng bạn.") })
            },
            cloze: new[]
            {
                new ClItem("ja-l32-cl-1", "easy", "Trợ từ が với すき", "Điền trợ từ đứng trước すきです:", "わたしはにほんりょうり{{0}}すきです。", new[] { new ClBlank(new[] { "が" }, "trợ từ đi với すき (ga)", "Đối tượng thích/ghét đi với trợ từ が.") }, "わたしはにほんりょうりがすきです。", "watashi wa nihon ryouri ga suki desu", "Tôi thích ẩm thực Nhật Bản.", "Không dùng を trước すきです."),
                new ClItem("ja-l32-cl-2", "easy", "Trợ từ địa điểm hành động で", "Điền trợ từ chỉ nơi diễn ra hành động 'đọc sách':", "としょかん{{0}}ほんをよみます。", new[] { new ClBlank(new[] { "で" }, "tại (nơi hành động)", "Động từ hành động đi với trợ từ で.") }, "としょかんでほんをよみます。", "toshokan de hon o yomimasu", "Tôi đọc sách ở thư viện.", "Phân biệt で (nơi hành động) và に (nơi tồn tại)."),
                new ClItem("ja-l32-cl-3", "medium", "Lời mời lịch sự 〜ませんか", "Điền đuôi lời mời lịch sự 'Bạn có muốn cùng uống cà phê không?':", "いっしょにコーヒーを{{0}}か。", new[] { new ClBlank(new[] { "のみません", "飲みません" }, "phủ định của のみます", "〜ませんか dùng để mời rủ lịch sự.") }, "いっしょにコーヒーをのみませんか。", "issho ni koohii o nomimasen ka", "Bạn có muốn cùng đi uống cà phê không?", "Đáp lại lời mời dùng ええ、いいですね / いきましょう."),
                new ClItem("ja-l32-cl-4", "hard", "Thể quá khứ 〜ました", "Chia động từ たべます sang quá khứ cho 'Hôm qua':", "きのう、すしを{{0}}。", new[] { new ClBlank(new[] { "たべました", "食べました" }, "đã ăn (tabemashita)", "Quá khứ lịch sự đổi 〜ます → 〜ました.") }, "きのう、すしをたべました。", "kinou, sushi o tabemashita", "Hôm qua tôi đã ăn sushi.", "きのう (hôm qua) luôn đi với động từ quá khứ 〜ました.")
            },
            translate: new[]
            {
                new TrItem("ja-l32-tr-1", "easy", "Thích âm nhạc", "Tôi thích âm nhạc.", new[] { "わたし", "おんがく", "が", "すき" }, new[] { "わたしはおんがくがすきです。", "私は音楽が好きです。", "おんがくがすきです。" }, "私は音楽が好きです。", "watashi wa ongaku ga suki desu", "Luôn dùng trợ từ が trước すきです."),
                new TrItem("ja-l32-tr-2", "easy", "Học ở thư viện", "Tôi học tiếng Nhật ở thư viện.", new[] { "としょかんで", "にほんごを", "べんきょうします" }, new[] { "としょかんでにほんごをべんきょうします。", "わたしはとしょかんでにほんごをべんきょうします。", "図書館で日本語を勉強します。", "私は図書館で日本語を勉強します。" }, "図書館で日本語を勉強します。", "toshokan de nihongo o benkyou shimasu", "Địa điểm hành động dùng で, tân ngữ dùng を."),
                new TrItem("ja-l32-tr-3", "medium", "Mời cùng ăn trưa", "Bạn có muốn cùng ăn trưa không?", new[] { "いっしょに", "ひるごはん", "たべませんか" }, new[] { "いっしょにひるごはんをたべませんか。", "一緒に昼ごはんを食べませんか。", "一緒に昼ご飯を食べませんか。" }, "一緒に昼ごはんを食べませんか。", "issho ni hirugohan o tabemasen ka", "Dùng 〜ませんか để mời lịch sự."),
                new TrItem("ja-l32-tr-4", "hard", "Sở thích xem phim", "Sở thích của tôi là xem phim.", new[] { "しゅみ", "えいがをみること" }, new[] { "わたしのしゅみはえいがをみることです。", "私の趣味は映画を見ることです。" }, "私の趣味は映画を見ることです。", "watashi no shumi wa eiga o miru koto desu", "Động từ nguyên mẫu + こと biến hành động thành danh từ.")
            },
            grammarFix: new[]
            {
                new GfItem("ja-l32-gf-1", "easy", "Dùng nhầm を trước すきです", "わたしはにほんごをすきです。", "すきです đi với trợ từ が (không đi với を).", "わたしはにほんごがすきです。", new[] { "わたしはにほんごがすきです。", "私は日本語が好きです。" }, "watashi wa nihongo ga suki desu", "Tôi thích tiếng Nhật.", "Tính từ chỉ sở thích/năng lực (すき, じょうず) luôn dùng trợ từ が."),
                new GfItem("ja-l32-gf-2", "medium", "Dùng nhầm に cho địa điểm hành động", "カフェにコーヒーをのみます。", "Hành động 'uống cà phê' tại quán phải dùng で.", "カフェでコーヒーをのみます。", new[] { "カフェでコーヒーをのみます。", "カフェでコーヒーを飲みます。" }, "kafe de koohii o nomimasu", "Tôi uống cà phê ở quán cà phê.", "Địa điểm xảy ra hành động luôn đi với trợ từ で."),
                new GfItem("ja-l32-gf-3", "hard", "Không chia quá khứ với きのう", "きのうえいがをみます。", "Có きのう (hôm qua) nên động từ phải chia quá khứ みました.", "きのうえいがをみました。", new[] { "きのうえいがをみました。", "昨日映画を見ました。" }, "kinou eiga o mimashita", "Hôm qua tôi đã xem phim.", "Thì quá khứ khẳng định: 〜ます → 〜ました.")
            },
            roleplay: new[]
            {
                new RpItem("ja-l32-rp-1", "easy", "Chia sẻ sở thích", "Bạn Nhật hỏi về sở thích của bạn trong giờ giải lao.", "Bạn Yuki", "しゅみはなんですか。なにがすきですか。", "shumi wa nan desu ka? nani ga suki desu ka?", "Sở thích của bạn là gì? Bạn thích gì?", "Trả lời sở thích của bạn dùng 〜がすきです (âm nhạc おんがく / phim えいが / tiếng Nhật にほんご).", new[] { "すき|好き", "おんがく|えいが|にほんご|スポーツ|音楽|映画|日本語" }, new[] { "わたしのしゅみはえいがをみることです。おんがくもすきです。" }, "watashi no shumi wa eiga o miru koto desu. ongaku mo suki desu.", 8),
                new RpItem("ja-l32-rp-2", "medium", "Kể thói quen học mỗi ngày", "Cô giáo hỏi hằng ngày bạn học tiếng Nhật ở đâu và vào lúc nào.", "Cô Suzuki", "まいにちどこでにほんごをべんきょうしますか。", "mainichi doko de nihongo o benkyou shimasu ka?", "Mỗi ngày em học tiếng Nhật ở đâu thế?", "Trả lời địa điểm + で (ví dụ としょかんで / いえで) và động từ べんきょうします.", new[] { "で", "べんきょう|勉強", "としょかん|いえ|へや|カフェ|図書館|家" }, new[] { "まいばんとしょかんでいちじかんべんきょうします。" }, "maiban toshokan de ichijikan benkyou shimasu.", 8),
                new RpItem("ja-l32-rp-3", "hard", "Mời bạn đi ăn cuối tuần", "Cuối tuần này bạn rảnh và muốn rủ Yuki cùng đi ăn ramen hoặc uống cà phê.", "Bạn Yuki", "こんしゅうのどようびはひまですよ。", "konshuu no doyoubi wa hima desu yo.", "Thứ Bảy tuần này mình rảnh đấy!", "Mời Yuki cùng đi ăn/uống bằng いっしょに và 〜ませんか hoặc 〜ましょう.", new[] { "いっしょに|一緒に", "ませんか|ましょう", "ラーメン|コーヒー|えいが|ひるごはん" }, new[] { "そうですか！いっしょにラーメンをたべませんか。" }, "sou desu ka! issho ni raamen o tabemasen ka?", 8)
            }, now));

        return all;
    }
}
