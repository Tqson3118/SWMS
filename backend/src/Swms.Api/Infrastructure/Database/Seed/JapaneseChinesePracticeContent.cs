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
    public const string UpgradeMarker = "<!-- studxy-cjk-v1 -->";

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
            description: "Khóa học tiếng Nhật nền tảng JLPT N5: làm chủ Hiragana/Katakana, phát âm trường âm & âm ngắt chuẩn giọng Nhật (ja-JP), giao tiếp đời sống, mua sắm, hỏi đường với đầy đủ bài tập Shadowing, Dictation, Sơ đồ từ vựng và Nhập vai AI.",
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
            description: "Khóa học tiếng Trung giao tiếp chuẩn HSK1: hệ thống Pinyin & 4 thanh điệu, quy tắc biến điệu, chữ Hán nền tảng, giao tiếp gia đình, mua sắm, ăn uống và hỏi đường chuẩn giọng phổ thông (zh-CN).",
            chapters: new[]
            {
                (SeedDataInitializer.ChineseChapter1Id, "Chương 1: Pinyin, Thanh điệu & Chữ Hán Cơ bản", 1),
                (SeedDataInitializer.ChineseChapter2Id, "Chương 2: Số đếm, Gia đình & Mua sắm HSK1", 2),
                (SeedDataInitializer.ChineseChapter3Id, "Chương 3: Sinh hoạt Hằng ngày & Hỏi đường HSK1", 3)
            },
            lessons: GetChineseLessons(),
            activities: GetChineseActivities(SeedDataInitializer.ChineseCourseId, now),
            now: now);
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
                if (existing.CurrentVersion == 1 && !(existing.ContentMarkdown ?? string.Empty).Contains(UpgradeMarker))
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
                $"Đọc to bằng Web Speech API ({lang}) để nhận phản hồi từng ký tự và gợi ý lỗi phát âm hay gặp của người Việt.",
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
                $"Phản hồi trực tiếp bằng giọng nói ({lang}) hoặc gõ phím; chấm bằng AI trực tiếp hoặc chấm từ khóa CJK offline.",
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
    // KHÓA 1: TIẾNG NHẬT SƠ CẤP (N5) — 6 BÀI LÝ THUYẾT CHI TIẾT
    // =========================================================================

    private static List<CjkLessonMeta> GetJapaneseLessons() => new()
    {
        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter1Id, SeedDataInitializer.JapaneseLesson11Id,
            "Bài 1.1: Hiragana nền tảng & Câu chào hỏi hằng ngày",
            "ja-n5-hiragana-chao-hoi-hang-ngay", 1, 25,
            @"# Bài 1.1: Hiragana nền tảng & Câu chào hỏi hằng ngày

## 1. Mục tiêu bài học
- Đọc và nhận diện trọn vẹn 46 chữ cái **Hiragana** cơ bản cùng âm đục (だくおん), âm ngắt **っ** và trường âm.
- Sử dụng đúng các câu chào hỏi tiếng Nhật theo thời điểm trong ngày và sắc thái lịch sự.
- Nhận diện và khắc phục sớm lỗi phát âm hay gặp của người Việt: trường âm `おう/えい`, âm ngắt `っ`, cặp `つ/す` và `し/ち`.

## 2. Bảng chữ cái Hiragana cốt lõi (Gojūon — 五十音)

| Hàng | a | i | u | e | o |
|---|---|---|---|---|---|
| **Nguyên âm** | あ (a) | い (i) | う (u) | え (e) | お (o) |
| **K** | か (ka) | き (ki) | く (ku) | け (ke) | こ (ko) |
| **S** | さ (sa) | し (shi) | す (su) | せ (se) | そ (so) |
| **T** | た (ta) | ち (chi) | つ (tsu) | て (te) | と (to) |
| **N** | な (na) | に (ni) | ぬ (nu) | ね (ne) | の (no) |
| **H** | は (ha) | ひ (hi) | ふ (fu) | へ (he) | ほ (ho) |
| **M / Y** | ま (ma) / や (ya) | み (mi) | む (mu) / ゆ (yu) | め (me) | も (mo) / よ (yo) |
| **R / W / N** | ら (ra) / わ (wa) | り (ri) | る (ru) | れ (re) | ろ (ro) / を (wo) / ん (n) |

## 3. Hội thoại mẫu chào hỏi buổi sáng
- **Tanaka:** おはようございます！ *(Ohayou gozaimasu!)* — Chào buổi sáng ạ!
- **Minh:** おはようございます、たなかさん。おげんきですか。 *(Ohayou gozaimasu, Tanaka-san. Ogenki desu ka?)* — Chào buổi sáng anh Tanaka. Anh có khỏe không ạ?
- **Tanaka:** はい、げんきです。ありがとうございます。 *(Hai, genki desu. Arigatou gozaimasu.)* — Vâng, tôi khỏe. Cảm ơn bạn.
- **Minh:** すみません、ちょっとまってください。 *(Sumimasen, chotto matte kudasai.)* — Xin lỗi, vui lòng đợi một chút ạ.
- **Tanaka:** いいえ、だいじょうぶですよ。またあした！ *(Iie, daijoubu desu yo. Mata ashita!)* — Không sao đâu. Hẹn gặp lại ngày mai!

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Hiragana (ひらがな)**: Bảng chữ mềm cơ bản của tiếng Nhật dùng để viết từ thuần Nhật, trợ từ và đuôi chia động từ.
- **Trợ từ は (đọc là wa)**: Đánh dấu chủ đề của câu, dù viết bằng chữ `は` nhưng luôn đọc là `wa` (ví dụ: `こんにちは`, `わたしは`).
- **Đuôi lịch sự です (desu)**: Đặt cuối câu danh từ/tính từ để tạo sắc thái lịch sự chuẩn N5, âm `u` cuối thường đọc rất nhẹ gần như câm (`des`).
- **Âm ngắt っ (Sokuon)**: Chữ `つ` viết nhỏ biểu thị nhịp dừng nén hơi đúng 1 phách trước phụ âm kế tiếp (ví dụ: `ちょっと` chotto, `まって` matte).

## 5. Lỗi phát âm người Việt hay mắc
- **Nuốt trường âm `おう` / `えい`**: Trong `おはよう` (ohayou) và `ありがとう` (arigatou), đuôi `おう` đọc kéo dài thành `ō` (2 nhịp), không đọc ngắt thành `o` ngắn.
- **Nhầm `つ (tsu)` với `す (su)` và `し (shi)` với `ち (chi)`**: `つ` phải bật hơi đầu lưỡi `ts`; `し` đẩy hơi mềm không bật như `ち`.
- **Đọc cứng chữ `ら/り/る/れ/ろ` thành `d` hoặc rung mạnh `r`**: Chạm nhẹ đầu lưỡi lên lợi trên một lần dứt khoát (giữa `l` và `r`).

<!-- studxy-cjk-v1 -->",
            "Làm chủ 46 chữ cái Hiragana nền tảng và các mẫu câu chào hỏi lịch sự hằng ngày.",
            "Luyện trường âm, âm ngắt っ và phân biệt cặp âm つ/す, し/ち chuẩn giọng Nhật (ja-JP)."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter1Id, SeedDataInitializer.JapaneseLesson12Id,
            "Bài 1.2: Katakana, Từ mượn & Tự giới thiệu bản thân",
            "ja-n5-katakana-tu-muon-gioi-thieu", 2, 25,
            @"# Bài 1.2: Katakana, Từ mượn & Tự giới thiệu bản thân

## 1. Mục tiêu bài học
- Thành thạo bảng chữ cứng **Katakana** dùng cho từ mượn ngoại lai, tên quốc gia và tên người nước ngoài.
- Tự giới thiệu bản thân đầy đủ theo chuẩn N5: `はじめまして` — tên — quốc tịch — nghề nghiệp — `どうぞよろしくおねがいします`.
- Vận dụng cấu trúc khẳng định/phủ định/nghi vấn với **A は B です / ではありません / ですか**.

## 2. Bảng chữ cái Katakana trọng tâm & Từ mượn thông dụng

| Hàng | a | i | u | e | o | Từ mượn hay gặp |
|---|---|---|---|---|---|---|
| **Nguyên âm** | ア (a) | イ (i) | ウ (u) | エ (e) | オ (o) | エアコン (máy lạnh) |
| **K / S** | カ / サ | キ / シ | ク / ス | ケ / セ | コ / ソ | コーヒー (cà phê), クラス (lớp học) |
| **T / N** | タ / ナ | チ / ニ | ツ / ヌ | テ / ネ | ト / ノ | ノート (vở), テスト (bài kiểm tra) |
| **H / M** | ハ / マ | ヒ / ミ | フ / ム | ヘ / メ | ホ / モ | ホテル (khách sạn), メール (email) |
| **R / W / ー** | ラ / ワ | リ | ル | レ | ロ / ン | ベトナム (Việt Nam), レストラン (nhà hàng) |

## 3. Hội thoại tự giới thiệu bản thân (じこしょうかい)
- **Minh:** はじめまして。わたしはミンです。ベトナムからきました。 *(Hajimemashite. Watashi wa Min desu. Betonamu kara kimashita.)* — Xin chào lần đầu gặp mặt. Tôi là Minh. Tôi đến từ Việt Nam.
- **Sato:** はじめまして、さとうです。ミンさんはがくせいですか。 *(Hajimemashite, Satou desu. Min-san wa gakusei desu ka?)* — Rất hân hạnh, tôi là Sato. Bạn Minh là sinh viên phải không?
- **Minh:** はい、だいがくのがくせいです。エンジニアをめざしています。 *(Hai, daigaku no gakusei desu. Enjinia o mezashite imasu.)* — Vâng, tôi là sinh viên đại học. Tôi đang hướng tới trở thành kỹ sư.
- **Sato:** そうですか！どうぞよろしくおねがいします。 *(Sou desu ka! Douzo yoroshiku onegaishimasu.)* — Vậy à! Rất mong được giúp đỡ ạ.

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Katakana (カタカナ)**: Bảng chữ nét thẳng dùng ghi từ vay mượn nước ngoài (`コーヒー`, `パソコン`), tên nước (`ベトナム`) và tên người nước ngoài.
- **Cấu trúc A は B です**: Câu danh từ cơ bản nghĩa là ""A là B""; phủ định lịch sự là `A は B ではありません` (hoặc `じゃありません`).
- **Trợ từ の (no)**: Nối hai danh từ biểu thị sở hữu hoặc thuộc về (ví dụ: `だいがく の がくせい` = sinh viên của trường đại học).
- **Trợ từ も (mo)**: Thay thế cho `は` mang nghĩa ""cũng"" (ví dụ: `わたしもベトナムじんです` = Tôi cũng là người Việt Nam).

## 5. Lỗi phát âm & chữ viết người Việt hay mắc
- **Nhầm 4 chữ Katakana dễ lẫn**: `シ (shi)` viết nét từ trên xuống hướng ngang khác `ツ (tsu)` viết từ trái sang hướng dọc; tương tự `ン (n)` khác `ソ (so)`.
- **Bỏ quên dấu trường âm `ー`**: Trong Katakana, gạch ngang `ー` kéo dài nguyên âm trước đó đúng 1 nhịp (`コーヒー` koohii, `カード` kaado).

<!-- studxy-cjk-v1 -->",
            "Nhận diện bảng Katakana, phân biệt シ/ツ và ソ/ン, thực hành mẫu câu giới thiệu bản thân N5.",
            "Vận dụng chuẩn trợ từ は, の, も và hậu tố chỉ quốc tịch 〜じん (〜人)."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter2Id, SeedDataInitializer.JapaneseLesson21Id,
            "Bài 2.1: Số đếm, Giờ giấc & Lịch hẹn cơ bản",
            "ja-n5-so-dem-gio-giac-lich-hen", 1, 25,
            @"# Bài 2.1: Số đếm, Giờ giấc & Lịch hẹn cơ bản

## 1. Mục tiêu bài học
- Đếm số từ 1 đến 10.000 (`いち` → `いちまん`), nắm các biến âm đặc biệt khi ghép với **〜じ (giờ)** và **〜ふん/ぷん (phút)**.
- Hỏi và trả lời thời gian hiện tại, ngày trong tuần và đặt lịch hẹn với **〜から〜まで** (từ... đến...).
- Phân biệt rõ trợ từ **に** đứng sau mốc thời gian cụ thể.

## 2. Bảng số đếm & Giờ giấc trọng tâm

| Số | Đọc cơ bản | Giờ (〜じ) | Phút (〜ふん / ぷん) | Lưu ý biến âm |
|---|---|---|---|---|
| 1 | いち (ichi) | いちじ (1 giờ) | いっぷん (1 phút) | Biến âm ngắt っ + ぷ |
| 3 | さん (san) | さんじ (3 giờ) | さんぷん (3 phút) | Đọc `ぷん` |
| 4 | よん / し | **よじ** (4 giờ) | よんぷん (4 phút) | Không đọc しじ! |
| 6 | ろく (roku) | ろくじ (6 giờ) | **ろっぷん** (6 phút) | Biến âm っ + ぷ |
| 7 | なな / しち | **しちじ** (7 giờ) | ななふん (7 phút) | Ưu tiên しちじ |
| 9 | きゅう / く | **くじ** (9 giờ) | きゅうふん (9 phút) | Không đọc きゅうじ! |
| 10 | じゅう (juu) | じゅうじ (10 giờ) | **じゅっぷん** (10 phút) | Biến âm ngắt っ |

## 3. Hội thoại hỏi giờ & hẹn lịch
- **Lan:** すみません、いまなんじですか。 *(Sumimasen, ima nanji desu ka?)* — Xin lỗi, bây giờ là mấy giờ rồi ạ?
- **Ken:** ごぜんくじはん（9:30）です。 *(Gozen kuji han desu.)* — Là 9 giờ rưỡi sáng ạ.
- **Lan:** かいぎはなんじからなんじまでですか。 *(Kaigi wa nanji kara nanji made desu ka?)* — Cuộc họp diễn ra từ mấy giờ đến mấy giờ vậy?
- **Ken:** じゅうじからじゅういちじさんじゅっぷんまでです。 *(Juuji kara juuichiji sanjuppun made desu.)* — Từ 10 giờ đến 11 giờ 30 phút.
- **Lan:** わかりました。あしたのしちじにおきます。 *(Wakarimashita. Ashita no shichiji ni okimasu.)* — Tôi hiểu rồi. Ngày mai tôi sẽ thức dậy lúc 7 giờ.

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Trợ từ に (ni) chỉ thời điểm**: Gắn ngay sau mốc thời gian có con số cụ thể như giờ, ngày, thứ (`しちじにおきます` = dậy lúc 7 giờ), KHÔNG dùng sau `きょう/あした/まいにち`.
- **Cấu trúc 〜から〜まで**: Diễn đạt khoảng thời gian hoặc không gian ""từ A đến B"" (`くじからごじまで` = từ 9 giờ đến 5 giờ).
- **Biến âm giờ đặc biệt**: Ba mốc giờ luôn phải thuộc lòng là **4 giờ (よじ)**, **7 giờ (しちじ)** và **9 giờ (くじ)**.
- **Từ chỉ buổi sáng/chiều**: `ごぜん (午前)` = sáng (AM), `ごご (午後)` = chiều/tối (PM), đặt TRƯỚC số giờ.

## 5. Lỗi phát âm người Việt hay mắc
- **Quên âm ngắt `っ` trong phút**: `いっぷん` (1 phút), `ろっぷん` (6 phút), `はっぷん` (8 phút), `じゅっぷん` (10 phút) đều bắt buộc dừng nhẹ 1 phách rồi bật hơi âm `p`.
- **Đọc sai trường âm `じゅう (10)` thành `じゅ`**: `じゅうじ` (10 giờ) kéo dài âm `uu`.

<!-- studxy-cjk-v1 -->",
            "Thuộc số đếm 1–10.000, các biến âm giờ phút đặc biệt (よじ, しちじ, くじ, ろっぷん) và cấu trúc 〜から〜まで.",
            "Đặt lịch hẹn cơ bản và sử dụng chính xác trợ từ に sau mốc thời gian."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter2Id, SeedDataInitializer.JapaneseLesson22Id,
            "Bài 2.2: Hỏi giá, Mua sắm & Đặt món ăn",
            "ja-n5-hoi-gia-mua-sam-dat-mon", 2, 25,
            @"# Bài 2.2: Hỏi giá, Mua sắm & Đặt món ăn

## 1. Mục tiêu bài học
- Phân biệt chuẩn hệ thống đại từ chỉ định **これ / それ / あれ** và **この / その / あの + Danh từ**.
- Tự tin hỏi giá tiền (`いくらですか`), đọc số tiền Yên (`ひゃく / せん / まん`) và gọi món bằng **〜をください**.
- Luyện các cụm lịch sự quen thuộc trong cửa hàng và quán ăn tại Nhật Bản.

## 2. Bảng chỉ định từ Ko-So-A-Do & Số tiền Yên

| Khoảng cách | Đại từ đứng độc lập | Đi trước Danh từ (+ N) | Nghĩa tiếng Việt |
|---|---|---|---|
| Gần người nói | **これ** (kore) | **この** + N (kono) | Cái này / N này |
| Gần người nghe | **それ** (sore) | **その** + N (sono) | Cái đó / N đó |
| Xa cả hai | **あれ** (are) | **あの** + N (ano) | Cái kia / N kia |
| Nghi vấn | **どれ** (dore) | **どの** + N (dono) | Cái nào / N nào |

> **Biến âm trăm & nghìn cần nhớ:** `300 = さんびゃく`, `600 = ろっぴゃく`, `800 = はっぴゃく`, `3000 = さんぜん`, `8000 = はっせん`.

## 3. Hội thoại tại quán cà phê & cửa hàng
- **Nhân viên:** いらっしゃいませ！ごちゅうもんはおきまりですか。 *(Irasshaimase! Gochuumon wa okimari desu ka?)* — Xin kính chào quý khách! Quý khách đã chọn được món chưa ạ?
- **Minh:** すみません、このコーヒーはいくらですか。 *(Sumimasen, kono koohii wa ikura desu ka?)* — Xin lỗi, ly cà phê này bao nhiêu tiền ạ?
- **Nhân viên:** さんびゃくごじゅうえんです。 *(Sanbyaku gojuu en desu.)* — 350 yên ạ.
- **Minh:** じゃ、ホットコーヒーをひとつと、サンドイッチをください。 *(Ja, hotto koohii o hitotsu to, sandoicchi o kudasai.)* — Vậy cho tôi 1 cà phê nóng và bánh sandwich ạ.
- **Nhân viên:** かしこまりました。ぜんぶでななひゃくえんです。 *(Kashikomarimashita. Zenbu de nanahyaku en desu.)* — Vâng tôi đã rõ. Tổng cộng là 700 yên ạ.

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Mẫu câu gọi món 〜をください**: Nghĩa là ""Làm ơn cho tôi [danh từ]"", dùng trợ từ **を (đọc là o)** ngay trước `ください`.
- **Phân biệt これ và この**: `これ` đứng một mình làm chủ ngữ (`これはいくらですか`), còn `この` bắt buộc có danh từ theo sau (`このかばんはいくらですか`).
- **Số đếm đồ vật thuần Nhật (ひとつ〜とお)**: `ひとつ (1)`, `ふたつ (2)`, `みっつ (3)` cực kỳ hữu dụng khi gọi món ăn hoặc mua hàng ở Nhật.
- **Trợ từ と (to) liệt kê**: Nối hai danh từ đầy đủ nghĩa là ""A và B"" (`コーヒーとケーキ` = cà phê và bánh ngọt).

## 5. Lỗi phát âm người Việt hay mắc
- **Quên biến âm ngắt trong giá tiền**: `ろっぴゃくえん` (600 yên), `はっぴゃくえん` (800 yên), `いらっしゃいませ` đều có âm ngắt `っ`.
- **Đọc `を` thành `wo` nặng**: Trong giao tiếp hiện đại, trợ từ `を` đọc nhẹ là `o`.

<!-- studxy-cjk-v1 -->",
            "Sử dụng linh hoạt これ/それ/あれ và この/その/あの để hỏi giá, mua sắm và gọi món ăn.",
            "Thuộc biến âm giá tiền Yên (さんびゃく, ろっぴゃく, さんぜん) và mẫu câu lịch sự 〜をください."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter3Id, SeedDataInitializer.JapaneseLesson31Id,
            "Bài 3.1: Hỏi đường, Tàu điện & Phương hướng",
            "ja-n5-hoi-duong-tau-dien-phuong-huong", 1, 25,
            @"# Bài 3.1: Hỏi đường, Tàu điện & Phương hướng

## 1. Mục tiêu bài học
- Hỏi vị trí địa điểm bằng **〜はどこですか** và phân biệt hai động từ tồn tại **あります** (đồ vật/địa điểm) và **います** (người/động vật).
- Sử dụng thành thạo từ chỉ phương hướng (`みぎ`, `ひだり`, `まっすぐ`, `まえ`, `うしろ`, `となり`) và phương tiện giao thông với trợ từ **で**.
- Giao tiếp cơ bản khi mua vé tàu điện và hỏi hướng đi tại nhà ga Nhật Bản.

## 2. Từ vựng Vị trí, Phương hướng & Phương tiện

| Tiếng Nhật | Đọc (Hiragana / Romaji) | Nghĩa tiếng Việt | Ví dụ ngắn |
|---|---|---|---|
| **駅 / えき** | えき (eki) | Nhà ga tàu điện | えきはどこですか。 |
| **右 / 左** | みぎ (migi) / ひだり (hidari) | Bên phải / Bên trái | みぎにまがってください。 |
| **まっすぐ** | まっすぐ (massugu) | Đi thẳng | まっすぐいってください。 |
| **隣 / 前** | となり (tonari) / まえ (mae) | Bên cạnh / Phía trước | ぎんこうのとなりにあります。 |
| **電車 / バス** | でんしゃ (densha) / バス (basu) | Tàu điện / Xe buýt | でんしゃでいきます。 |
| **切符** | きっぷ (kippu) | Vé tàu / vé xe | きっぷをかいます。 |

## 3. Hội thoại hỏi đường ra nhà ga
- **Du khách:** すみません、とうきょうえきはどこですか。 *(Sumimasen, Toukyou-eki wa doko desu ka?)* — Xin lỗi, ga Tokyo ở đâu vậy ạ?
- **Người dân:** このみちをまっすぐいって、つぎのかどをひだりにまがってください。 *(Kono michi o massugu itte, tsugi no kado o hidari ni magatte kudasai.)* — Bạn đi thẳng con đường này, rồi rẽ trái ở góc đường tiếp theo nhé.
- **Du khách:** あるいてなんぷんぐらいかかりますか。 *(Aruite nanpun gurai kakarimasu ka?)* — Đi bộ mất khoảng mấy phút ạ?
- **Người dân:** ごふんぐらいです。コンビニのとなりにありますよ。 *(Gofun gurai desu. Konbini no tonari ni arimasu yo.)* — Khoảng 5 phút thôi. Nó nằm ngay cạnh cửa hàng tiện lợi ấy.
- **Du khách:** どうもありがとうございます！ *(Doumo arigatou gozaimasu!)* — Cảm ơn anh rất nhiều ạ!

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Phân biệt あります và います**: Dùng `あります` cho vật vô tri/địa điểm (`えきがあります`) và `います` cho người/động vật (`ともだちがいます`).
- **Trợ từ で (de) chỉ phương tiện**: Gắn sau phương tiện di chuyển (`でんしゃでいきます` = đi bằng tàu điện; riêng đi bộ là `あるいていきます`, không dùng `で`).
- **Trợ từ へ (đọc là e) / に chỉ hướng đến**: Dùng trước động từ di chuyển `いきます / きます / かえります` (`えきへいきます`).
- **Mẫu câu chỉ đường 〜てください**: Nhờ vả hoặc chỉ dẫn lịch sự (`まっすぐいってください` = hãy đi thẳng, `みぎにまがってください` = hãy rẽ phải).

## 5. Lỗi phát âm người Việt hay mắc
- **Âm ngắt trong chỉ đường**: `まっすぐ` (đi thẳng), `きっぷ` (vé tàu), `いって` (đi) đều có âm ngắt `っ` rất rõ.
- **Nhầm trợ từ `へ` đọc là `he`**: Khi là trợ từ chỉ phương hướng (`えきへいきます`), `へ` luôn đọc là `e`.

<!-- studxy-cjk-v1 -->",
            "Hỏi vị trí địa điểm, phân biệt あります/います và nghe hiểu hướng dẫn đi thẳng, rẽ trái/phải.",
            "Vận dụng trợ từ で (phương tiện), へ/に (hướng đến) và cấu trúc chỉ dẫn 〜てください."),

        new CjkLessonMeta(
            SeedDataInitializer.JapaneseCourseId, SeedDataInitializer.JapaneseChapter3Id, SeedDataInitializer.JapaneseLesson32Id,
            "Bài 3.2: Thói quen hằng ngày, Sở thích & Đề nghị lịch sự",
            "ja-n5-thoi-quen-so-thich-de-nghi", 2, 25,
            @"# Bài 3.2: Thói quen hằng ngày, Sở thích & Đề nghị lịch sự

## 1. Mục tiêu bài học
- Diễn đạt lịch trình sinh hoạt hằng ngày bằng động từ thể **〜ます / 〜ません / 〜ました** và trợ từ tân ngữ **を**, địa điểm hành động **で**.
- Nói về sở thích cá nhân với cấu trúc **〜がすきです** và **しゅみは〜ことです**.
- Mời rủ và đưa ra đề nghị lịch sự bằng **〜ませんか** (Bạn có muốn cùng... không?) và **〜ましょう** (Chúng ta cùng... nhé!).

## 2. Động từ sinh hoạt hằng ngày & Chia thể Lịch sự (〜ます)

| Nguyên mẫu / Nghĩa | Hiện tại khẳng định | Hiện tại phủ định | Quá khứ khẳng định | Ví dụ thực tế |
|---|---|---|---|---|
| Ăn | たべます (tabemasu) | たべません | たべました | あさごはんをたべます |
| Uống | のみます (nomimasu) | のみません | のみました | おちゃをのみます |
| Học | べんきょうします | べんきょうしません | べんきょうしました | にほんごをべんきょうします |
| Xem / Nhìn | みます (mimasu) | みません | みました | えいがをみます |
| Nghe | ききます (kikimasu) | ききません | ききました | おんがくをききます |

## 3. Hội thoại về sở thích & rủ bạn cuối tuần
- **Yuki:** ミンさんのしゅみはなんですか。 *(Min-san no shumi wa nan desu ka?)* — Sở thích của Minh là gì thế?
- **Minh:** わたしのしゅみはえいがをみることです。にほんのおんがくもすきです。 *(Watashi no shumi wa eiga o miru koto desu. Nihon no ongaku mo suki desu.)* — Sở thích của tôi là xem phim. Tôi cũng thích nhạc Nhật nữa.
- **Yuki:** いいですね！まいにちにほんごをべんきょうしますか。 *(Ii desu ne! Mainichi nihongo o benkyou shimasu ka?)* — Hay quá nhỉ! Ngày nào bạn cũng học tiếng Nhật à?
- **Minh:** はい、まいばんとしょかんでいちじかんべんきょうします。 *(Hai, maiban toshokan de ichijikan benkyou shimasu.)* — Vâng, mỗi tối tôi học một tiếng ở thư viện.
- **Yuki:** しゅうまつにいっしょにラーメンをたべませんか。 *(Shuumatsu ni issho ni raamen o tabemasen ka?)* — Cuối tuần chúng ta cùng đi ăn ramen nhé?
- **Minh:** ええ、いいですね！いきましょう！ *(Ee, ii desu ne! Ikimashou!)* — Ừ, hay quá! Đi thôi nào!

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Trợ từ で (de) chỉ nơi diễn ra hành động**: Khác với `に` (chỉ sự tồn tại), `で` dùng khi có hành động xảy ra tại địa điểm đó (`としょかんでべんきょうします` = học bài ở thư viện).
- **Cấu trúc sở thích 〜がすきです**: Đối tượng yêu thích đi với trợ từ **が** (không dùng `を`), ví dụ `にほんごがすきです` (Tôi thích tiếng Nhật).
- **Lời mời lịch sự 〜ませんか**: Dùng thể phủ định + `か` để mời người khác một cách tinh tế, tôn trọng ý kiến đối phương (`いっしょにいきませんか`).
- **Lời rủ đồng thuận 〜ましょう**: Dùng để hưởng ứng lời mời hoặc rủ người thân quen (`たべましょう` = Cùng ăn nào!).

## 5. Lỗi phát âm người Việt hay mắc
- **Nuốt trường âm trong `べんきょう` và `えいが`**: `べんきょう` (benkyou) kéo dài âm `ō`; `えいが` (eiga) kéo dài âm `ē`.
- **Nhầm `しゅうまつ (cuối tuần)` với `しゅまつ`**: Nhớ kéo dài `しゅう` và phát âm nhẹ nhàng `し` không bật mạnh thành `ち`.

<!-- studxy-cjk-v1 -->",
            "Chia động từ thể ます/ません/ました, dùng đúng trợ từ で cho địa điểm hành động và が cho sở thích.",
            "Tự tin trò chuyện về thói quen hằng ngày và mời rủ lịch sự bằng 〜ませんか / 〜ましょう.")
    };

    // =========================================================================
    // KHÓA 1: TIẾNG NHẬT SƠ CẤP (N5) — KHO ĐỀ LUYỆN TẬP 6 BÀI (48 PACK / 228 CÂU)
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
