using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Security;
using Swms.Api.Infrastructure.Database.Seed;

namespace Swms.Api.Infrastructure.Database;

public static class SeedDataInitializer
{
    public static readonly Guid DefaultUserId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    public static readonly Guid PersonalCourseId = Guid.Parse("22222222-2222-4222-8222-222222222221");
    public static readonly Guid AcademicCourseId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    public static readonly Guid ChapterMemoryId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    public static readonly Guid LessonPagingId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    public static readonly Guid LessonPageReplacementId = Guid.Parse("44444444-4444-4444-8444-444444444445");
    public static readonly Guid SampleDocumentId = Guid.Parse("55555555-5555-4555-8555-555555555555");
    public static readonly Guid DefaultNotebookId = Guid.Parse("66666666-6666-4666-8666-666666666666");
    public static readonly Guid DefaultQuizId = Guid.Parse("77777777-7777-4777-8777-777777777777");

    // English Communication Course & Lesson UUIDs (Phase B Seed)
    public static readonly Guid EnglishCourseId = Guid.Parse("22222222-2222-4222-8222-222222222223");
    public static readonly Guid EnglishChapter1Id = Guid.Parse("33333333-3333-4333-8333-333333333334");
    public static readonly Guid EnglishChapter2Id = Guid.Parse("33333333-3333-4333-8333-333333333335");
    public static readonly Guid EnglishChapter3Id = Guid.Parse("33333333-3333-4333-8333-333333333336");

    public static readonly Guid EnglishLesson11Id = Guid.Parse("44444444-4444-4444-8444-444444444446");
    public static readonly Guid EnglishLesson12Id = Guid.Parse("44444444-4444-4444-8444-444444444447");
    public static readonly Guid EnglishLesson21Id = Guid.Parse("44444444-4444-4444-8444-444444444448");
    public static readonly Guid EnglishLesson22Id = Guid.Parse("44444444-4444-4444-8444-444444444449");
    public static readonly Guid EnglishLesson31Id = Guid.Parse("44444444-4444-4444-8444-444444444450");
    public static readonly Guid EnglishLesson32Id = Guid.Parse("44444444-4444-4444-8444-444444444451");

    // Investing Practice Course & Lessons (Phase C Seed)
    public static readonly Guid InvestingCourseId = Guid.Parse("22222222-2222-4222-8222-222222222224");
    public static readonly Guid InvestingChapter1Id = Guid.Parse("33333333-3333-4333-8333-333333333337");
    public static readonly Guid InvestingChapter2Id = Guid.Parse("33333333-3333-4333-8333-333333333338");
    public static readonly Guid InvestingChapter3Id = Guid.Parse("33333333-3333-4333-8333-333333333339");
    public static readonly Guid InvestingLesson11Id = Guid.Parse("44444444-4444-4444-8444-444444444452");
    public static readonly Guid InvestingLesson12Id = Guid.Parse("44444444-4444-4444-8444-444444444453");
    public static readonly Guid InvestingLesson21Id = Guid.Parse("44444444-4444-4444-8444-444444444454");
    public static readonly Guid InvestingLesson22Id = Guid.Parse("44444444-4444-4444-8444-444444444455");
    public static readonly Guid InvestingLesson31Id = Guid.Parse("44444444-4444-4444-8444-444444444456");
    public static readonly Guid InvestingLesson32Id = Guid.Parse("44444444-4444-4444-8444-444444444457");

    // Coding Practice Course & Lessons (Phase C Seed)
    public static readonly Guid CodingCourseId = Guid.Parse("22222222-2222-4222-8222-222222222225");
    public static readonly Guid CodingChapter1Id = Guid.Parse("33333333-3333-4333-8333-333333333340");
    public static readonly Guid CodingChapter2Id = Guid.Parse("33333333-3333-4333-8333-333333333341");
    public static readonly Guid CodingChapter3Id = Guid.Parse("33333333-3333-4333-8333-333333333342");
    public static readonly Guid CodingLesson11Id = Guid.Parse("44444444-4444-4444-8444-444444444458");
    public static readonly Guid CodingLesson12Id = Guid.Parse("44444444-4444-4444-8444-444444444459");
    public static readonly Guid CodingLesson21Id = Guid.Parse("44444444-4444-4444-8444-444444444460");
    public static readonly Guid CodingLesson22Id = Guid.Parse("44444444-4444-4444-8444-444444444461");
    public static readonly Guid CodingLesson31Id = Guid.Parse("44444444-4444-4444-8444-444444444462");
    public static readonly Guid CodingLesson32Id = Guid.Parse("44444444-4444-4444-8444-444444444463");

    // Psychology Practice Course & Lessons (Phase C Seed)
    public static readonly Guid PsychologyCourseId = Guid.Parse("22222222-2222-4222-8222-222222222226");
    public static readonly Guid PsychologyChapter1Id = Guid.Parse("33333333-3333-4333-8333-333333333343");
    public static readonly Guid PsychologyChapter2Id = Guid.Parse("33333333-3333-4333-8333-333333333344");
    public static readonly Guid PsychologyChapter3Id = Guid.Parse("33333333-3333-4333-8333-333333333345");
    public static readonly Guid PsychologyLesson11Id = Guid.Parse("44444444-4444-4444-8444-444444444464");
    public static readonly Guid PsychologyLesson12Id = Guid.Parse("44444444-4444-4444-8444-444444444465");
    public static readonly Guid PsychologyLesson21Id = Guid.Parse("44444444-4444-4444-8444-444444444466");
    public static readonly Guid PsychologyLesson22Id = Guid.Parse("44444444-4444-4444-8444-444444444467");
    public static readonly Guid PsychologyLesson31Id = Guid.Parse("44444444-4444-4444-8444-444444444468");
    public static readonly Guid PsychologyLesson32Id = Guid.Parse("44444444-4444-4444-8444-444444444469");

    // Stable Block UUIDs inside Lesson 3.2 AST
    public static readonly Guid BlockHeading1Id = Guid.Parse("aaaaaaa1-1111-4111-8111-111111111111");
    public static readonly Guid BlockPara1Id = Guid.Parse("aaaaaaa2-2222-4222-8222-222222222222");
    public static readonly Guid BlockHeading2Id = Guid.Parse("aaaaaaa3-3333-4333-8333-333333333333");
    public static readonly Guid BlockPara2Id = Guid.Parse("aaaaaaa4-4444-4444-8444-444444444444");
    public static readonly Guid BlockPara3Id = Guid.Parse("aaaaaaa5-5555-4555-8555-555555555555");
    public static readonly Guid BlockHeading3Id = Guid.Parse("aaaaaaa6-6666-4666-8666-666666666666");
    public static readonly Guid BlockPara4Id = Guid.Parse("aaaaaaa7-7777-4777-8777-777777777777");

    // Sample Annotation UUIDs
    public static readonly Guid AnnCoreTlbId = Guid.Parse("bbbbbbb1-1111-4111-8111-111111111111");
    public static readonly Guid AnnExampleFormulaId = Guid.Parse("bbbbbbb2-2222-4222-8222-222222222222");
    public static readonly Guid AnnQuestionMultiLevelId = Guid.Parse("bbbbbbb3-3333-4333-8333-333333333333");
    public static readonly Guid AnnOrphaned1Id = Guid.Parse("bbbbbbb4-4444-4444-8444-444444444444");
    public static readonly Guid AnnOrphaned2Id = Guid.Parse("bbbbbbb5-5555-4555-8555-555555555555");

    private static readonly SemaphoreSlim SeedLock = new(1, 1);

    public static Task SeedAsync(SwmsDbContext db) => SeedAsync(db, null);

    public static async Task SeedAsync(SwmsDbContext db, string? documentRootPath)
    {
        await SeedLock.WaitAsync();
        try
        {
            if (await db.Users.AnyAsync())
            {
                await Features.DocumentCourseSyncService.SyncAllFromDocumentsAsync(db, documentRootPath);
                await EnsurePersonalFinanceSeededAsync(db);
                await EnsureEnglishCommunicationCourseSeededAsync(db);
                await EnsurePracticeExpansionCoursesSeededAsync(db);
                return;
            }

            var now = DateTimeOffset.UtcNow;

        // 1. Default Learner User: Sơn Trần
        var user = new User
        {
            Id = DefaultUserId,
            Email = "son.tran@studxy.vn",
            PasswordHash = PasswordHashUtility.HashPassword("Studxy@2026"),
            FullName = "Sơn Trần",
            Role = "student",
            TokenVersion = 1,
            StreakDays = 7,
            OnboardingStep = 2,
            IsActivated = false,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now
        };
        db.Users.Add(user);

        // 1.1 Learner FSRS v5 Parameters (19 canonical weights)
        db.UserFsrsParameters.Add(new UserFsrsParameters
        {
            UserId = DefaultUserId,
            DesiredRetention = 0.90,
            MaximumIntervalDays = 36500
        });

        // 1.2 User Quotas & Initial AI Usage Ledger
        db.UserQuotas.Add(new UserQuota
        {
            UserId = DefaultUserId,
            Tier = "free",
            DailyAiTokenLimit = 50000,
            DailyAiCreditLimit = 50,
            StorageLimitBytes = 52428800, // 50 MB
            CurrentStorageBytes = 19293798 // 18.4 MB
        });

        db.AiUsageLedgers.AddRange(
            new AiUsageLedger
            {
                UserId = DefaultUserId,
                Feature = "qa_generation",
                CreditsCharged = 8,
                Model = "gemini-3.8-flash",
                PromptTokens = 2100,
                CompletionTokens = 940,
                TotalTokens = 3040,
                CostUsd = 0.001520m,
                CreatedAt = now.AddHours(-3)
            },
            new AiUsageLedger
            {
                UserId = DefaultUserId,
                Feature = "explain_concept",
                CreditsCharged = 4,
                Model = "gemini-3.8-flash",
                PromptTokens = 1200,
                CompletionTokens = 680,
                TotalTokens = 1880,
                CostUsd = 0.000940m,
                CreatedAt = now.AddHours(-2)
            },
            new AiUsageLedger
            {
                UserId = DefaultUserId,
                Feature = "essay_feedback",
                CreditsCharged = 2,
                Model = "gemini-3.8-flash",
                PromptTokens = 850,
                CompletionTokens = 520,
                TotalTokens = 1370,
                CostUsd = 0.000685m,
                CreatedAt = now.AddHours(-1)
            }
        );

        // 2. Courses: Personal Workspace + Academic Course
        var personalCourse = new Course
        {
            Id = PersonalCourseId,
            InstructorId = DefaultUserId,
            Title = "Không gian cá nhân (Personal Workspace)",
            Slug = "personal-workspace-son-tran",
            Description = "Không gian tự học chủ quyền mặc định cho tài liệu PDF, ghi chú tổng hợp và thẻ FSRS.",
            Visibility = "personal",
            IsPublished = true
        };

        var academicCourse = new Course
        {
            Id = AcademicCourseId,
            InstructorId = DefaultUserId,
            Title = "Hệ điều hành: Kiến trúc Bộ nhớ ảo & Phân trang",
            Slug = "he-dieu-hanh-bo-nho-ao",
            Description = "Khóa học cốt lõi về Quản lý bộ nhớ, Phần cứng phân trang (MMU/TLB), Bảng trang đa cấp và Thuật toán thay thế trang.",
            Visibility = "public",
            IsPublished = true
        };
        db.Courses.AddRange(personalCourse, academicCourse);

        db.CourseEnrollments.Add(new CourseEnrollment
        {
            UserId = DefaultUserId,
            CourseId = AcademicCourseId,
            Role = "student"
        });

        var chapter3 = new Chapter
        {
            Id = ChapterMemoryId,
            CourseId = AcademicCourseId,
            Title = "Chương 3: Quản lý Bộ nhớ & Phân trang (Memory Management)",
            SortOrder = 1
        };
        db.Chapters.Add(chapter3);

        // 3. Lesson 3.2 AST & Plain Text
        var p1Text = "Phân trang (Paging) là cơ chế quản lý bộ nhớ cho phép không gian địa chỉ vật lý của một tiến trình không cần phải liên tục. Không gian địa chỉ vật lý được chia thành các khối kích thước cố định gọi là khung trang (frames), còn không gian địa chỉ logic được chia thành các trang (pages) cùng kích thước.";
        var p2Text = "Phần cứng phân trang dựa trên bộ đệm Translation Lookaside Buffer (TLB), một bộ nhớ cache liên kết tốc độ cao nằm trong Memory Management Unit (MMU) cho phép ánh xạ địa chỉ ảo sang địa chỉ vật lý trong O(1) chu kỳ xung nhịp.";
        var p3Text = "Thời gian truy cập bộ nhớ hiệu dụng (Effective Access Time - EAT) được tính theo công thức: EAT = α × (t_TLB + t_MEM) + (1 - α) × (t_TLB + 2 × t_MEM), trong đó α là tỷ lệ TLB Hit Ratio. Khi α = 98%, chi phí tra cứu gần như tương đương truy cập RAM trực tiếp.";
        var p4Text = "Đối với kiến trúc 64-bit, một bảng trang phẳng (flat page table) sẽ tiêu tốn hàng Terabyte bộ nhớ chỉ để lưu chỉ mục. Do đó, hệ điều hành hiện đại (Linux x86-64) sử dụng bảng trang đa cấp 4 tầng (PGD -> PUD -> PMD -> PTE) kết hợp với bảng trang nghịch đảo (Inverted Page Table) để chỉ cấp phát các nút bảng trang thực sự được tiến trình sử dụng.";

        var lessonAst = new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 2, block_id = BlockHeading1Id.ToString(), section = "1.1 Cơ sở lý thuyết Phân trang" },
                    content = new[] { new { type = "text", text = "1.1 Cơ sở lý thuyết Phân trang & Không gian Địa chỉ" } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = BlockPara1Id.ToString() },
                    content = new[] { new { type = "text", text = p1Text } }
                },
                new
                {
                    type = "heading",
                    attrs = new { level = 2, block_id = BlockHeading2Id.ToString(), section = "1.2 Phần cứng Phân trang & TLB" },
                    content = new[] { new { type = "text", text = "1.2 Phần cứng Phân trang & Tăng tốc phần cứng với TLB" } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = BlockPara2Id.ToString() },
                    content = new[] { new { type = "text", text = p2Text } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = BlockPara3Id.ToString() },
                    content = new[] { new { type = "text", text = p3Text } }
                },
                new
                {
                    type = "heading",
                    attrs = new { level = 2, block_id = BlockHeading3Id.ToString(), section = "1.3 Bảng trang đa cấp (Multi-level Page Tables)" },
                    content = new[] { new { type = "text", text = "1.3 Bảng trang đa cấp & Bảng trang nghịch đảo" } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = BlockPara4Id.ToString() },
                    content = new[] { new { type = "text", text = p4Text } }
                }
            }
        };

        var fullPlainText = string.Join("\n\n", new[]
        {
            "1.1 Cơ sở lý thuyết Phân trang & Không gian Địa chỉ",
            p1Text,
            "1.2 Phần cứng Phân trang & Tăng tốc phần cứng với TLB",
            p2Text,
            p3Text,
            "1.3 Bảng trang đa cấp & Bảng trang nghịch đảo",
            p4Text
        });

        var lesson32 = new Lesson
        {
            Id = LessonPagingId,
            ChapterId = ChapterMemoryId,
            Title = "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp",
            Slug = "bo-nho-ao-tlb-va-bang-trang-da-cap",
            SortOrder = 1,
            CurrentVersion = 2,
            ContentAstJson = JsonSerializer.Serialize(lessonAst),
            PlainText = fullPlainText,
            IsPublished = true
        };

        var lesson33Ast = new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 2, block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = "2.1 Thuật toán Thay thế Trang (FIFO, Optimal, LRU & Clock)" } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = "Khi xảy ra Page Fault và không còn khung trang trống trong RAM, hệ điều hành phải chọn một trang nạn nhân (victim frame) để hoán đổi ra đĩa. Thuật toán LRU (Least Recently Used) và xấp xỉ Clock (Second-Chance) sử dụng bit tham chiếu (Reference Bit) để tối ưu tỷ lệ lỗi trang mà không gặp nghịch lý Belady." } }
                }
            }
        };

        var lesson33 = new Lesson
        {
            Id = LessonPageReplacementId,
            ChapterId = ChapterMemoryId,
            Title = "Bài 3.3: Thuật toán Thay thế Trang (LRU, Clock & Working Set)",
            Slug = "thuat-toan-thay-the-trang-lru-clock",
            SortOrder = 2,
            CurrentVersion = 1,
            ContentAstJson = JsonSerializer.Serialize(lesson33Ast),
            PlainText = "2.1 Thuật toán Thay thế Trang (FIFO, Optimal, LRU & Clock)\n\nKhi xảy ra Page Fault và không còn khung trang trống trong RAM, hệ điều hành phải chọn một trang nạn nhân (victim frame) để hoán đổi ra đĩa.",
            IsPublished = true
        };

        db.Lessons.AddRange(lesson32, lesson33);

        db.LessonVersions.Add(new LessonVersion
        {
            LessonId = LessonPagingId,
            VersionNumber = 2,
            ContentAstJson = lesson32.ContentAstJson,
            PlainText = lesson32.PlainText,
            PublishedBy = DefaultUserId,
            ChangeSummary = "Cập nhật công thức EAT và cấu trúc bảng trang 4 tầng Linux x86-64"
        });

        // 4. Sample PDF Document in Personal Workspace
        var pdfDocAst = new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 2, block_id = "pdf-page-1" },
                    content = new[] { new { type = "text", text = "Operating Systems: Three Easy Pieces — Chapter 19: Translation Lookaside Buffers" } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = "pdf-page-1-p1" },
                    content = new[] { new { type = "text", text = "Using paging as the core mechanism to support virtual memory can lead to high performance overheads. By chopping the address space into small, fixed-sized units (pages), a hardware TLB cache is mandatory to avoid extra memory references per instruction fetch." } }
                }
            }
        };

        db.Documents.Add(new Document
        {
            Id = SampleDocumentId,
            CourseId = PersonalCourseId,
            UploaderId = DefaultUserId,
            Title = "Operating Systems: Three Easy Pieces (OSTEP - Ch.18-20 Paging & TLB).pdf",
            FileUrl = "/samples/ostep-paging-tlb.pdf",
            FileSizeBytes = 19293798,
            TotalPages = 48,
            MimeType = "application/pdf",
            OcrStatus = "completed",
            ContentAstJson = JsonSerializer.Serialize(pdfDocAst),
            PlainText = "Operating Systems: Three Easy Pieces — Chapter 19: Translation Lookaside Buffers. Using paging as the core mechanism to support virtual memory can lead to high performance overheads."
        });

        db.DocumentPages.Add(new DocumentPage
        {
            Id = Guid.NewGuid(),
            DocumentId = SampleDocumentId,
            PageNumber = 1,
            PlainText = "Operating Systems: Three Easy Pieces — Chapter 19: Translation Lookaside Buffers. Using paging as the core mechanism to support virtual memory can lead to high performance overheads.",
            HasTextLayer = true,
            CharCount = 186,
            WordCount = 27
        });

        // 5. Annotations (3 anchored + 2 orphaned for reconciliation banner)
        var quote1 = "Translation Lookaside Buffer (TLB), một bộ nhớ cache liên kết tốc độ cao nằm trong Memory Management Unit (MMU)";
        var idx1 = p2Text.IndexOf(quote1, StringComparison.Ordinal);

        var quote2 = "EAT = α × (t_TLB + t_MEM) + (1 - α) × (t_TLB + 2 × t_MEM)";
        var idx2 = p3Text.IndexOf(quote2, StringComparison.Ordinal);

        var quote3 = "bảng trang đa cấp 4 tầng (PGD -> PUD -> PMD -> PTE)";
        var idx3 = p4Text.IndexOf(quote3, StringComparison.Ordinal);

        db.Annotations.AddRange(
            new Annotation
            {
                Id = AnnCoreTlbId,
                UserId = DefaultUserId,
                LessonId = LessonPagingId,
                BlockId = BlockPara2Id,
                ExactQuote = quote1,
                OriginalExactQuote = quote1,
                PrefixQuote = p2Text.Substring(0, idx1),
                SuffixQuote = p2Text.Substring(idx1 + quote1.Length, Math.Min(32, p2Text.Length - (idx1 + quote1.Length))),
                StartOffset = idx1,
                EndOffset = idx1 + quote1.Length,
                ColorKey = "yellow",
                SemanticLabel = "Khái niệm cốt lõi",
                NoteComment = "TLB quyết định tốc độ dịch địa chỉ O(1) trong phần cứng MMU.",
                Status = "anchored",
                CreatedAt = now.AddDays(-2)
            },
            new Annotation
            {
                Id = AnnExampleFormulaId,
                UserId = DefaultUserId,
                LessonId = LessonPagingId,
                BlockId = BlockPara3Id,
                ExactQuote = quote2,
                OriginalExactQuote = quote2,
                PrefixQuote = p3Text.Substring(Math.Max(0, idx2 - 32), Math.Min(32, idx2)),
                SuffixQuote = p3Text.Substring(idx2 + quote2.Length, Math.Min(32, p3Text.Length - (idx2 + quote2.Length))),
                StartOffset = idx2,
                EndOffset = idx2 + quote2.Length,
                ColorKey = "green",
                SemanticLabel = "Ví dụ & Dẫn chứng",
                NoteComment = "Công thức chuẩn tính thời gian truy cập bộ nhớ hiệu dụng khi có TLB.",
                Status = "anchored",
                CreatedAt = now.AddDays(-1)
            },
            new Annotation
            {
                Id = AnnQuestionMultiLevelId,
                UserId = DefaultUserId,
                LessonId = LessonPagingId,
                BlockId = BlockPara4Id,
                ExactQuote = quote3,
                OriginalExactQuote = quote3,
                PrefixQuote = p4Text.Substring(Math.Max(0, idx3 - 32), Math.Min(32, idx3)),
                SuffixQuote = p4Text.Substring(idx3 + quote3.Length, Math.Min(32, p4Text.Length - (idx3 + quote3.Length))),
                StartOffset = idx3,
                EndOffset = idx3 + quote3.Length,
                ColorKey = "purple",
                SemanticLabel = "Ý tưởng tổng hợp",
                NoteComment = "So sánh chi phí duyệt 4 tầng cây trang với bảng trang nghịch đảo băm.",
                Status = "anchored",
                CreatedAt = now.AddHours(-12)
            },
            new Annotation
            {
                Id = AnnOrphaned1Id,
                UserId = DefaultUserId,
                LessonId = LessonPagingId,
                BlockId = BlockPara1Id,
                ExactQuote = "Cơ chế phân trang hai cấp giải quyết triệt để vấn đề phân mảnh ngoài trong bộ nhớ vật lý cổ điển.",
                OriginalExactQuote = "Cơ chế phân trang hai cấp giải quyết triệt để vấn đề phân mảnh ngoài trong bộ nhớ vật lý cổ điển.",
                PrefixQuote = "Trong phiên bản 1.0: ",
                SuffixQuote = " Tuy nhiên bảng trang lớn.",
                StartOffset = 0,
                EndOffset = 95,
                ColorKey = "yellow",
                SemanticLabel = "Khái niệm cốt lõi",
                NoteComment = "Trích dẫn từ bản nháp v1.0 trước khi cập nhật lên bảng trang 4 tầng.",
                Status = "orphaned",
                CreatedAt = now.AddDays(-5)
            },
            new Annotation
            {
                Id = AnnOrphaned2Id,
                UserId = DefaultUserId,
                LessonId = LessonPagingId,
                BlockId = BlockPara2Id,
                ExactQuote = "TLB Miss buộc phần cứng phải thực hiện 4 lần truy cập bộ nhớ liên tiếp nếu không có Page Walk Cache.",
                OriginalExactQuote = "TLB Miss buộc phần cứng phải thực hiện 4 lần truy cập bộ nhớ liên tiếp nếu không có Page Walk Cache.",
                PrefixQuote = "Lưu ý hiệu năng: ",
                SuffixQuote = " Cần chú ý trong bài thi.",
                StartOffset = 10,
                EndOffset = 110,
                ColorKey = "rose",
                SemanticLabel = "Lưu ý quan trọng",
                NoteComment = "Điểm bẫy thường gặp khi tính toán chu kỳ trễ của TLB Miss trên kiến trúc 64-bit.",
                Status = "orphaned",
                CreatedAt = now.AddDays(-4)
            }
        );

        // 6. Default Freeform Notebook linked to Lesson 3.2
        var nbBlock1 = Guid.NewGuid();
        var nbBlock2 = Guid.NewGuid();
        var nbBlock3 = Guid.NewGuid();

        var notebookAst = new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 1, block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = "Ghi chú Tổng hợp: Bộ nhớ ảo, TLB & Bảng trang đa cấp" } }
                },
                new
                {
                    type = "annotationRef",
                    attrs = new
                    {
                        block_id = nbBlock1.ToString(),
                        ref_annotation_id = AnnCoreTlbId.ToString(),
                        ref_lesson_id = LessonPagingId.ToString(),
                        ref_block_id = BlockPara2Id.ToString(),
                        color_key = "yellow",
                        source_title = "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp",
                        source_quote = quote1
                    }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = nbBlock2.ToString() },
                    content = new[]
                    {
                        new { type = "text", text = "💡 Nhận xét cốt lõi: Nếu không có TLB, mỗi lệnh truy cập bộ nhớ trên hệ thống phân trang 4 cấp sẽ cần tới 5 lần đọc RAM liên tiếp. Nhờ nguyên lý cục bộ không gian và thời gian (Spatial & Temporal Locality), TLB đạt tỷ lệ trúng (Hit Ratio) trên 98%. Xem thêm liên kết hai chiều: [[Bài 3.3: Thuật toán Thay thế Trang (LRU, Clock & Working Set)]]." }
                    }
                },
                new
                {
                    type = "annotationRef",
                    attrs = new
                    {
                        block_id = nbBlock3.ToString(),
                        ref_annotation_id = AnnExampleFormulaId.ToString(),
                        ref_lesson_id = LessonPagingId.ToString(),
                        ref_block_id = BlockPara3Id.ToString(),
                        color_key = "green",
                        source_title = "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp",
                        source_quote = quote2
                    }
                }
            }
        };

        db.Notebooks.Add(new Notebook
        {
            Id = DefaultNotebookId,
            UserId = DefaultUserId,
            Title = "Ghi chú: Bài 3.2 Bộ nhớ ảo, TLB và Bảng trang đa cấp",
            PrimarySourceLessonId = LessonPagingId,
            ContentAstJson = JsonSerializer.Serialize(notebookAst),
            Version = 1,
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now.AddHours(-1)
        });

        db.NotebookLinks.AddRange(
            new NotebookLink
            {
                NotebookId = DefaultNotebookId,
                SourceBlockId = nbBlock1,
                TargetType = "annotation",
                RefAnnotationId = AnnCoreTlbId,
                RefLessonId = LessonPagingId
            },
            new NotebookLink
            {
                NotebookId = DefaultNotebookId,
                SourceBlockId = nbBlock3,
                TargetType = "annotation",
                RefAnnotationId = AnnExampleFormulaId,
                RefLessonId = LessonPagingId
            }
        );

        // 7. Orthogonal Tags & Unified Review Queue Flashcards (No Decks per ADR-020)
        var tagOs = new Tag { Id = Guid.Parse("88888881-1111-4111-8111-111111111111"), UserId = DefaultUserId, Name = "OS-Paging", ColorKey = "blue" };
        var tagMemory = new Tag { Id = Guid.Parse("88888882-2222-4222-8222-222222222222"), UserId = DefaultUserId, Name = "Memory", ColorKey = "yellow" };
        var tagTlb = new Tag { Id = Guid.Parse("88888883-3333-4333-8333-333333333333"), UserId = DefaultUserId, Name = "TLB", ColorKey = "green" };
        var tagAlgorithms = new Tag { Id = Guid.Parse("88888884-4444-4444-8444-444444444444"), UserId = DefaultUserId, Name = "Algorithms", ColorKey = "purple" };
        db.Tags.AddRange(tagOs, tagMemory, tagTlb, tagAlgorithms);

        var card1Id = Guid.Parse("99999991-1111-4111-8111-111111111111");
        var card2Id = Guid.Parse("99999992-2222-4222-8222-222222222222");
        var card3Id = Guid.Parse("99999993-3333-4333-8333-333333333333");
        var card4Id = Guid.Parse("99999994-4444-4444-8444-444444444444");

        db.Flashcards.AddRange(
            new Flashcard
            {
                Id = card1Id,
                UserId = DefaultUserId,
                LessonId = LessonPagingId,
                AnnotationId = AnnCoreTlbId,
                CardType = "cloze",
                SourceTitleSnapshot = "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp",
                SourceQuoteSnapshot = quote1,
                FrontAstJson = JsonSerializer.Serialize(new
                {
                    text = "Phần cứng phân trang dựa trên bộ đệm {{c1::Translation Lookaside Buffer (TLB)}}, một bộ nhớ cache liên kết tốc độ cao nằm trong MMU cho phép dịch địa chỉ trong O(1) chu kỳ."
                }),
                BackAstJson = JsonSerializer.Serialize(new
                {
                    text = "Translation Lookaside Buffer (TLB) — Bộ nhớ đệm ánh xạ nhanh giữa Số hiệu trang ảo (VPN) và Số hiệu khung trang vật lý (PFN), giảm thiểu chi phí duyệt bảng trang trong RAM."
                }),
                Stability = 3.1262,
                Difficulty = 5.21,
                Reps = 1,
                Lapses = 0,
                State = 2, // Review
                IsArchived = false,
                LastReviewAt = now.AddDays(-3),
                DueAt = now.AddHours(-2) // Due today!
            },
            new Flashcard
            {
                Id = card2Id,
                UserId = DefaultUserId,
                LessonId = LessonPagingId,
                AnnotationId = AnnExampleFormulaId,
                CardType = "cloze",
                SourceTitleSnapshot = "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp",
                SourceQuoteSnapshot = quote2,
                FrontAstJson = JsonSerializer.Serialize(new
                {
                    text = "Công thức tính Thời gian truy cập bộ nhớ hiệu dụng (EAT) khi có TLB với tỷ lệ trúng α là: {{c1::EAT = α × (t_TLB + t_MEM) + (1 - α) × (t_TLB + 2 × t_MEM)}}."
                }),
                BackAstJson = JsonSerializer.Serialize(new
                {
                    text = "Khi TLB Hit (xác suất α), tốn 1 lần tra TLB + 1 lần đọc RAM. Khi TLB Miss (xác suất 1 - α), tốn 1 lần tra TLB + 1 lần đọc bảng trang trong RAM + 1 lần đọc dữ liệu thực trong RAM."
                }),
                Stability = 0.0,
                Difficulty = 0.0,
                Reps = 0,
                Lapses = 0,
                State = 0, // New
                IsArchived = false,
                DueAt = now.AddMinutes(-30) // Due today!
            },
            new Flashcard
            {
                Id = card3Id,
                UserId = DefaultUserId,
                LessonId = LessonPagingId,
                AnnotationId = AnnQuestionMultiLevelId,
                CardType = "ai_generated",
                SourceTitleSnapshot = "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp",
                SourceQuoteSnapshot = quote3,
                FrontAstJson = JsonSerializer.Serialize(new
                {
                    text = "Tại sao kiến trúc 64-bit bắt buộc phải dùng Bảng trang đa cấp (Multi-level Page Table) hoặc Bảng trang nghịch đảo thay vì Bảng trang phẳng (Flat Page Table)?"
                }),
                BackAstJson = JsonSerializer.Serialize(new
                {
                    text = "Vì không gian địa chỉ 64-bit có tới 2^52 trang (với trang 4KB); một bảng trang phẳng liên tục sẽ chiếm hàng Terabyte RAM cho mỗi tiến trình. Bảng trang đa cấp 4 tầng (PGD -> PUD -> PMD -> PTE) chỉ cấp phát các nhánh bảng trang thực sự có dữ liệu."
                }),
                Stability = 1.1829,
                Difficulty = 6.45,
                Reps = 1,
                Lapses = 0,
                State = 1, // Learning
                IsArchived = false,
                LastReviewAt = now.AddDays(-1),
                DueAt = now.AddMinutes(-10) // Due today!
            },
            new Flashcard
            {
                Id = card4Id,
                UserId = DefaultUserId,
                DocumentId = SampleDocumentId,
                CardType = "standard",
                SourceTitleSnapshot = "Operating Systems: Three Easy Pieces (OSTEP).pdf — Trang 14",
                SourceQuoteSnapshot = "Spatial locality and temporal locality ensure high TLB hit rates.",
                FrontAstJson = JsonSerializer.Serialize(new
                {
                    text = "Sự khác biệt giữa Tính cục bộ thời gian (Temporal Locality) và Tính cục bộ không gian (Spatial Locality) trong hiệu năng TLB là gì?"
                }),
                BackAstJson = JsonSerializer.Serialize(new
                {
                    text = "• Temporal Locality: Một trang vừa được truy cập có xác suất cao sẽ được truy cập lại trong tương lai gần (ví dụ vòng lặp).\n• Spatial Locality: Các phần tử dữ liệu nằm liền kề nhau trong cùng một trang 4KB thường được truy cập nối tiếp nhau (ví dụ duyệt mảng)."
                }),
                Stability = 0.0,
                Difficulty = 0.0,
                Reps = 0,
                Lapses = 0,
                State = 0, // New
                IsArchived = false,
                DueAt = now.AddMinutes(-5)
            }
        );

        db.FlashcardTags.AddRange(
            new FlashcardTag { FlashcardId = card1Id, TagId = tagOs.Id },
            new FlashcardTag { FlashcardId = card1Id, TagId = tagTlb.Id },
            new FlashcardTag { FlashcardId = card2Id, TagId = tagOs.Id },
            new FlashcardTag { FlashcardId = card2Id, TagId = tagMemory.Id },
            new FlashcardTag { FlashcardId = card3Id, TagId = tagMemory.Id },
            new FlashcardTag { FlashcardId = card4Id, TagId = tagTlb.Id },
            new FlashcardTag { FlashcardId = card4Id, TagId = tagAlgorithms.Id }
        );

        // Initial review record on Card 1 so ON DELETE RESTRICT can be tested
        db.FlashcardReviews.Add(new FlashcardReview
        {
            ReviewEventId = Guid.Parse("cccccccc-1111-4111-8111-111111111111"),
            FlashcardId = card1Id,
            UserId = DefaultUserId,
            Rating = 3,
            ReviewDurationMs = 3400,
            ScheduledDays = 3.0,
            ElapsedDays = 0.0,
            LastStability = 0.0,
            NewStability = 3.1262,
            LastDifficulty = 0.0,
            NewDifficulty = 5.21,
            ReviewedAt = now.AddDays(-3)
        });

        // 8. Formative Practice Quiz (ADR-021)
        var quiz = new PracticeQuiz
        {
            Id = DefaultQuizId,
            UserId = DefaultUserId,
            Title = "Bài luyện tập thực hành: Cơ chế Phân trang, TLB & Bảng trang đa cấp",
            LessonId = LessonPagingId,
            TotalItems = 3,
            CorrectItems = 0,
            Status = "in_progress",
            StartedAt = now
        };
        db.PracticeQuizzes.Add(quiz);

        db.PracticeQuizItems.AddRange(
            new PracticeQuizItem
            {
                Id = Guid.Parse("ddddddd1-1111-4111-8111-111111111111"),
                QuizId = DefaultQuizId,
                ItemType = "multiple_choice",
                PromptAstJson = JsonSerializer.Serialize(new
                {
                    question = "Giả sử thời gian tra cứu TLB là 1ns, thời gian truy cập RAM là 100ns, và bảng trang đơn cấp nằm trong RAM. Nếu tỷ lệ TLB Hit (α) là 90%, thời gian truy cập bộ nhớ hiệu dụng (EAT) là bao nhiêu?"
                }),
                ChoicesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "A", text = "101 ns" },
                    new { key = "B", text = "111 ns" },
                    new { key = "C", text = "120 ns" },
                    new { key = "D", text = "201 ns" }
                }),
                ClozeSolution = "B"
            },
            new PracticeQuizItem
            {
                Id = Guid.Parse("ddddddd2-2222-4222-8222-222222222222"),
                QuizId = DefaultQuizId,
                ItemType = "cloze",
                PromptAstJson = JsonSerializer.Serialize(new
                {
                    question = "Điền thuật ngữ chính xác: Trong bộ quản lý bộ nhớ MMU, bộ đệm phần cứng giúp ánh xạ nhanh từ địa chỉ trang ảo sang khung trang vật lý trong O(1) được viết tắt là {{c1::TLB}}."
                }),
                ClozeSolution = "TLB"
            },
            new PracticeQuizItem
            {
                Id = Guid.Parse("ddddddd3-3333-4333-8333-333333333333"),
                QuizId = DefaultQuizId,
                ItemType = "essay",
                PromptAstJson = JsonSerializer.Serialize(new
                {
                    question = "Phân tích sự đánh đổi (trade-off) giữa Bảng trang đa cấp (Multi-level Page Table) và Bảng trang phẳng khi xảy ra sự kiện TLB Miss. Làm thế nào phần cứng hiện đại giảm thiểu chi phí duyệt nhiều tầng bảng trang?",
                    rubricCriteria = new[]
                    {
                        "Giải thích rõ lợi ích tiết kiệm bộ nhớ RAM của bảng trang đa cấp thông qua cấp phát thưa (sparse allocation).",
                        "Phân tích nhược điểm khi xảy ra TLB Miss: phải thực hiện nhiều lần truy cập bộ nhớ tuần tự (4-5 memory walks trên x86-64).",
                        "Đề cập giải pháp giảm thiểu: tăng tỷ lệ TLB Hit (Huge Pages 2MB/1GB) hoặc Page Walk Cache (PWC) trong MMU."
                    }
                }),
                ClozeSolution = "Bảng trang đa cấp tiết kiệm RAM nhờ không cấp phát vùng trống nhưng làm tăng độ trễ khi TLB Miss (4 lần đọc RAM). Giải pháp gồm Huge Pages và Page Walk Cache."
            }
        );

        await db.SaveChangesAsync();

        // 6. Sync all curated document-library courses cleanly, falling back to PF zip only if document library is absent
        await Features.DocumentCourseSyncService.SyncAllFromDocumentsAsync(db, documentRootPath);
        await EnsurePersonalFinanceSeededAsync(db);
        await EnsureEnglishCommunicationCourseSeededAsync(db);
        await EnsurePracticeExpansionCoursesSeededAsync(db);
        }
        finally
        {
            SeedLock.Release();
        }
    }

    private static async Task EnsurePersonalFinanceSeededAsync(SwmsDbContext db)
    {
        if (await db.Courses.AnyAsync(c => c.Slug == "document-course-personalfinance" || c.Title.Contains("Tài Chính Cá Nhân")))
        {
            return;
        }

        var pfZipFixture = Path.Combine(AppContext.BaseDirectory, "test-fixtures", "personal-finance-course.zip");
        if (!File.Exists(pfZipFixture))
        {
            var fallbackFixture = @"D:\03_Study\Studxy\frontend\e2e\fixtures\personal-finance-course.zip";
            if (File.Exists(fallbackFixture)) pfZipFixture = fallbackFixture;
        }

        if (File.Exists(pfZipFixture))
        {
            try
            {
                using var fs = File.OpenRead(pfZipFixture);
                await Features.CourseWizardAndImportService.ImportCourseFromZipAsync(db, fs, DefaultUserId);
            }
            catch
            {
                // Non-critical fallback
            }
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
        return JsonSerializer.Serialize(ast);
    }

    public static Guid CreateDeterministicActivityId(Guid courseId, Guid? lessonId, string type, int orderIndex)
    {
        var key = $"swms:practice-activity:{courseId:N}:{(lessonId.HasValue ? lessonId.Value.ToString("N") : "course")}:{type.Trim().ToLowerInvariant()}:{orderIndex}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }

    public static async Task EnsureEnglishCommunicationCourseSeededAsync(SwmsDbContext db)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == EnglishCourseId || c.Slug == "tieng-anh-giao-tiep-thuc-chien-a2-b1");
        var now = DateTimeOffset.UtcNow;

        if (course == null)
        {
            course = new Course
            {
                Id = EnglishCourseId,
                InstructorId = DefaultUserId,
                Title = "Tiếng Anh Giao Tiếp Thực Chiến (A2 → B1)",
                Slug = "tieng-anh-giao-tiep-thuc-chien-a2-b1",
                Description = "Khóa học giao tiếp phản xạ thực chiến A2 đến B1 dành cho sinh viên và người đi làm, trang bị phương pháp Shadowing, Nghe - Chép chính tả (Dictation), Luyện phát âm chuẩn IPA và Sơ đồ từ vựng Mind Map.",
                Visibility = "public",
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Courses.Add(course);
            await db.SaveChangesAsync();
        }

        var targetCourseId = course.Id;
        var enrollmentExists = await db.CourseEnrollments.AnyAsync(e => e.UserId == DefaultUserId && e.CourseId == targetCourseId);
        if (!enrollmentExists)
        {
            db.CourseEnrollments.Add(new CourseEnrollment
            {
                Id = Guid.NewGuid(),
                UserId = DefaultUserId,
                CourseId = targetCourseId,
                Role = "student",
                EnrolledAt = now
            });
            await db.SaveChangesAsync();
        }

        // Chapters
        var chapters = new List<Chapter>();
        if (!await db.Chapters.AnyAsync(ch => ch.Id == EnglishChapter1Id))
        {
            chapters.Add(new Chapter
            {
                Id = EnglishChapter1Id,
                CourseId = EnglishCourseId,
                Title = "Chương 1: Phản Xạ Giao Tiếp Đời Thường (Social & Daily Life)",
                SortOrder = 1,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        if (!await db.Chapters.AnyAsync(ch => ch.Id == EnglishChapter2Id))
        {
            chapters.Add(new Chapter
            {
                Id = EnglishChapter2Id,
                CourseId = EnglishCourseId,
                Title = "Chương 2: Tiếng Anh Công Sở & Giao Tiếp Đội Nhóm (Workplace Communication)",
                SortOrder = 2,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        if (!await db.Chapters.AnyAsync(ch => ch.Id == EnglishChapter3Id))
        {
            chapters.Add(new Chapter
            {
                Id = EnglishChapter3Id,
                CourseId = EnglishCourseId,
                Title = "Chương 3: Thuyết Trình, Đàm Phán & Phản Biện (Presentation & Negotiation)",
                SortOrder = 3,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        if (chapters.Count > 0)
        {
            db.Chapters.AddRange(chapters);
            await db.SaveChangesAsync();
        }

        // Lessons
        var lessonsToAdd = new List<Lesson>();

        // 1.1
        if (!await db.Lessons.AnyAsync(l => l.Id == EnglishLesson11Id))
        {
            var title = "Bài 1.1: Small Talk, Giới Thiệu Bản Thân & Phản Xạ Tự Nhiên";
            var md = @"# Bài 1.1: Small Talk, Giới Thiệu Bản Thân & Phản Xạ Tự Nhiên

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững cấu trúc giới thiệu bản thân 3 bước: Hook - Background - Passion.
- Mở đầu cuộc trò chuyện tự nhiên trong các sự kiện giao lưu văn hóa và quốc tế.
- Làm chủ kỹ năng Shadowing ngữ điệu bản xứ và Dictation nghe chép chính tả câu hội thoại đời sống.

## 2. Đoạn hội thoại mẫu (Model Dialogue)
- **Alex:** Hi there! Is this seat taken?
- **Mai:** No, please go ahead. Beautiful morning for a conference, isn't it?
- **Alex:** Absolutely! I'm Alex from Melbourne. Are you attending the tech workshop today?
- **Mai:** Yes, exactly! I'm Mai, a software engineer based in Hanoi. It is a pleasure to meet you.

## 3. Cụm từ then chốt (Key Phrases & IPA)
- **break the ice** `/breɪk ðiː aɪs/`: Phá vỡ sự ngại ngùng ban đầu.
- **pleasure to meet you** `/ˈpleʒə tuː miːt juː/`: Rất hân hạnh được gặp bạn.
- **fruitful collaboration** `/ˈfruːtfl kəˌlæbəˈreɪʃn/`: Sự hợp tác hiệu quả và đem lại thành quả tốt đẹp.";
            lessonsToAdd.Add(new Lesson
            {
                Id = EnglishLesson11Id,
                ChapterId = EnglishChapter1Id,
                Title = title,
                Slug = "small-talk-gioi-thieu-ban-than-phan-xa",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 15,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Nắm vững kỹ năng mở đầu cuộc trò chuyện tự nhiên.", "Thực hành Shadowing và Dictation phản xạ."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        // 1.2
        if (!await db.Lessons.AnyAsync(l => l.Id == EnglishLesson12Id))
        {
            var title = "Bài 1.2: Hỏi Đường, Mua Sắm & Xử Lý Tình Huống Thực Tế";
            var md = @"# Bài 1.2: Hỏi Đường, Mua Sắm & Xử Lý Tình Huống Thực Tế

## 1. Mục tiêu bài học (Learning Objectives)
- Sử dụng các mẫu câu chỉ đường (giving directions), hỏi giá và thanh toán không tiền mặt.
- Xử lý các tình huống bất ngờ khi du lịch hoặc công tác nước ngoài.

## 2. Đoạn hội thoại mẫu (Model Dialogue)
- **Tourist:** Excuse me, could you tell me how to get to the central station?
- **Local:** Sure! Walk straight for two blocks, then turn left at the traffic light. You can't miss it.
- **Tourist:** Thank you so much! Is it within walking distance?
- **Local:** Yes, it takes about ten minutes on foot.";
            lessonsToAdd.Add(new Lesson
            {
                Id = EnglishLesson12Id,
                ChapterId = EnglishChapter1Id,
                Title = title,
                Slug = "hoi-duong-mua-sam-xu-ly-tinh-huong",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 15,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Hỏi đường và xử lý các tình huống đời sống.", "Mẫu câu thực tế và phản xạ nhanh."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        // 2.1
        if (!await db.Lessons.AnyAsync(l => l.Id == EnglishLesson21Id))
        {
            var title = "Bài 2.1: Viết Email Công Việc, Đặt Lịch Hẹn & Báo Cáo Tiến Độ";
            var md = @"# Bài 2.1: Viết Email Công Việc, Đặt Lịch Hẹn & Báo Cáo Tiến Độ

## 1. Mục tiêu bài học (Learning Objectives)
- Viết email công việc chuẩn mực với phong thái trang trọng (professional tone).
- Đặt lịch hẹn, hoãn lịch khéo léo và báo cáo tiến độ tuần theo phương pháp SMART.

## 2. Mẫu Email công việc (Sample Email)
Dear Mr. Johnson,
I am writing to provide a brief status update regarding our quarterly deliverables. All key milestones have been completed on schedule. Would you be available for a brief sync tomorrow at 3 PM?
Best regards,
Mai Tran";
            lessonsToAdd.Add(new Lesson
            {
                Id = EnglishLesson21Id,
                ChapterId = EnglishChapter2Id,
                Title = title,
                Slug = "viet-email-cong-viec-dat-lich-hen-bao-cao",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 20,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Viết email chuyên nghiệp và cập nhật tiến độ công việc.", "Cấu trúc văn phong văn phòng chuẩn quốc tế."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        // 2.2
        if (!await db.Lessons.AnyAsync(l => l.Id == EnglishLesson22Id))
        {
            var title = "Bài 2.2: Hội Họp Trực Tuyến, Trình Bày Ý Kiến & Thống Nhất Phương Án";
            var md = @"# Bài 2.2: Hội Họp Trực Tuyến, Trình Bày Ý Kiến & Thống Nhất Phương Án

## 1. Mục tiêu bài học (Learning Objectives)
- Làm chủ các cụm từ điều phối cuộc họp online (virtual meetings etiquette).
- Bày tỏ sự đồng thuận, phản biện nhẹ nhàng (diplomatic disagreement) và tổng kết biên bản hành động.

## 2. Đoạn hội thoại cuộc họp (Meeting Dialogue)
- **Chair:** Let's get down to business. Mai, could you walk us through the technical architecture?
- **Mai:** Certainly. As you can see on slide four, we have decoupled the frontend from our API services to maximize scalability.
- **David:** I see your point, but have we taken network latency into account?";
            lessonsToAdd.Add(new Lesson
            {
                Id = EnglishLesson22Id,
                ChapterId = EnglishChapter2Id,
                Title = title,
                Slug = "hoi-hop-truc-tuyen-trinh-bay-y-kien",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 20,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Hội họp trực tuyến và diễn đạt ý kiến chuyên môn.", "Kỹ năng phản biện ngoại giao và thuyết phục."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        // 3.1
        if (!await db.Lessons.AnyAsync(l => l.Id == EnglishLesson31Id))
        {
            var title = "Bài 3.1: Cấu Trúc Bài Thuyết Trình Tinh Gọn (Opening - Body - Conclusion)";
            var md = @"# Bài 3.1: Cấu Trúc Bài Thuyết Trình Tinh Gọn (Opening - Body - Conclusion)

## 1. Mục tiêu bài học (Learning Objectives)
- Xây dựng bài thuyết trình tiếng Anh 5 phút theo cấu trúc Opening - Body - Conclusion.
- Sử dụng các từ nối chuyển ý (signposting language) để giữ sự chú ý của khán giả.

## 2. Cấu trúc thuyết trình mẫu
- **Opening:** Good morning everyone, today I'm excited to present our innovative product strategy.
- **Signposting:** Moving on to our next key metric, let's take a look at customer retention.
- **Conclusion:** To wrap up, investing in continuous learning is the single highest-yield decision.";
            lessonsToAdd.Add(new Lesson
            {
                Id = EnglishLesson31Id,
                ChapterId = EnglishChapter3Id,
                Title = title,
                Slug = "cau-truc-bai-thuyet-trinh-tinh-gon",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Cấu trúc bài thuyết trình tiếng Anh tinh gọn.", "Kỹ thuật dẫn dắt và giữ chân người nghe."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        // 3.2
        if (!await db.Lessons.AnyAsync(l => l.Id == EnglishLesson32Id))
        {
            var title = "Bài 3.2: Đàm Phán Lợi Ích, Phản Biện Khéo Léo & Thuyết Phục Đối Tác";
            var md = @"# Bài 3.2: Đàm Phán Lợi Ích, Phản Biện Khéo Léo & Thuyết Phục Đối Tác

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững kỹ thuật đàm phán nguyên tắc (principled negotiation: win-win solution).
- Xử lý phản hồi từ chối bằng các câu hỏi làm rõ (clarifying questions) thay vì tranh cãi.

## 2. Đoạn hội thoại đàm phán mẫu
- **Client:** The timeline seems a bit tight for our internal review process.
- **Consultant:** We completely understand your concern. If we deliver the core module by Friday, would that give your team sufficient time to review?";
            lessonsToAdd.Add(new Lesson
            {
                Id = EnglishLesson32Id,
                ChapterId = EnglishChapter3Id,
                Title = title,
                Slug = "dam-phan-loi-ich-phan-bien-kheo-leo",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Đàm phán nguyên tắc và tìm giải pháp win-win.", "Thuyết phục đối tác chuyên nghiệp."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (lessonsToAdd.Count > 0)
        {
            db.Lessons.AddRange(lessonsToAdd);
            await db.SaveChangesAsync();
        }

        await EnglishPracticeContent.EnsureLessonsUpgradedOrSeededAsync(db, targetCourseId, now);

        // Practice Activities for the lessons
        var activities = new List<PracticeActivity>
        {
            // Lesson 1.1 Activities
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson11Id,
                OrderIndex = 1,
                Type = "shadowing",
                Title = "Shadowing: Giới thiệu bản thân và mở đầu cuộc trò chuyện",
                Description = "Nghe và nói đuổi theo câu nói với ngữ điệu tự nhiên, chọn tốc độ 0.75x - 1.25x và so sánh giọng của bạn.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "It is a pleasure to meet you, and I look forward to our fruitful collaboration.",
                    ipa = "/ɪt ɪz ə ˈpleʒə tuː miːt juː, ænd aɪ lʊk ˈfɔːwəd tuː ˈaʊə ˈfruːtfl kəˌlæbəˈreɪʃn/",
                    meaningVi = "Rất hân hạnh được gặp bạn, và tôi rất mong đợi vào sự hợp tác hiệu quả của chúng ta.",
                    keyPhrases = new[] { "pleasure to meet you", "look forward to", "fruitful collaboration" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson11Id,
                OrderIndex = 2,
                Type = "dictation",
                Title = "Dictation: Nghe và chép chính tả câu Small Talk",
                Description = "Nghe câu hội thoại và gõ lại chính xác từng từ để rèn luyện thính giác tiếng Anh giao tiếp.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "Making a positive first impression helps build long-term trust in professional relationships.",
                    ipa = "/ˈmeɪkɪŋ ə ˈpɒzətɪv fɜːst ɪmˈpreʃn helps bɪld ˈlɒŋ tɜːm trʌst ɪn prəˈfeʃənl rɪˈleɪʃnʃɪps/",
                    meaningVi = "Tạo ấn tượng ban đầu tích cực giúp xây dựng sự tin cậy lâu dài trong các mối quan hệ chuyên nghiệp.",
                    wordChips = new[] { "Making", "a", "positive", "first", "impression", "helps", "build", "long-term", "trust", "in", "professional", "relationships" },
                    distractors = new[] { "however", "therefore", "crucial", "often" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson11Id,
                OrderIndex = 3,
                Type = "pronunciation",
                Title = "Chấm phát âm: Ngữ điệu câu chào hỏi chuyên nghiệp",
                Description = "Đọc to câu mục tiêu và nhận dạng giọng nói tự động để phát hiện các từ phát âm chưa chuẩn.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "I look forward to learning more about your background and recent projects.",
                    ipa = "/aɪ lʊk ˈfɔːwəd tuː ˈlɜːnɪŋ mɔːr əˈbaʊt jɔː ˈbækɡraʊnd ænd ˈriːsnt ˈprɒdʒekts/",
                    meaningVi = "Tôi rất mong được tìm hiểu thêm về nền tảng học vấn và các dự án gần đây của bạn.",
                    targetWords = new[] { "look", "forward", "learning", "background", "recent", "projects" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson11Id,
                OrderIndex = 4,
                Type = "word_map",
                Title = "Sơ đồ từ vựng: Introduce & Relationship Building",
                Description = "Khám phá sơ đồ tư duy (Word Map) quanh từ khóa introduce: collocations, phrasal verbs, từ đồng nghĩa và lưu vào Flashcard FSRS.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    centralWord = "introduce",
                    ipa = "/ˌɪntrəˈdjuːs/",
                    meaningVi = "Giới thiệu, làm quen, đưa vào áp dụng",
                    collocations = new object[]
                    {
                        new { text = "introduce oneself", ipa = "/ˌɪntrəˈdjuːs wʌnˈself/", meaningVi = "Tự giới thiệu bản thân", example = "Allow me to introduce myself to the whole engineering team." },
                        new { text = "introduce new ideas", ipa = "/ˌɪntrəˈdjuːs njuː aɪˈdɪəz/", meaningVi = "Đưa ra các ý tưởng mới", example = "She introduced innovative concepts during the kick-off meeting." },
                        new { text = "introduce changes", ipa = "/ˌɪntrəˈdjuːs ˈtʃeɪndʒɪz/", meaningVi = "Áp dụng những cải tiến mới", example = "Management plans to introduce flexible working hours." }
                    },
                    phrasalVerbs = new object[]
                    {
                        new { text = "reach out to", ipa = "/riːtʃ aʊt tuː/", meaningVi = "Chủ động liên hệ, làm quen", example = "Feel free to reach out to me anytime on LinkedIn." },
                        new { text = "break the ice", ipa = "/breɪk ðiː aɪs/", meaningVi = "Phá băng sự e ngại ban đầu", example = "Telling a light joke helped break the ice at the table." }
                    },
                    synonyms = new object[]
                    {
                        new { text = "present", ipa = "/prɪˈzent/", meaningVi = "Trình bày, giới thiệu", example = "Let me present our newest team member." },
                        new { text = "acquaint", ipa = "/əˈkweɪnt/", meaningVi = "Làm quen với", example = "You should acquaint yourself with company guidelines." }
                    },
                    contextExamples = new object[]
                    {
                        new { sentence = "She introduced herself with remarkable confidence and warmth.", meaningVi = "Cô ấy tự giới thiệu bản thân với sự tự tin và ấm áp đáng nể." }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Lesson 1.2 Activities
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson12Id,
                OrderIndex = 1,
                Type = "shadowing",
                Title = "Shadowing: Hỏi đường và chỉ phương hướng",
                Description = "Luyện phản xạ nghe và lặp lại câu hỏi đường thông dụng.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "Could you tell me the quickest way to get to the international terminal?",
                    ipa = "/kəd juː tel miː ðə ˈkwɪkɪst weɪ tuː ɡet tuː ði ˌɪntəˈnæʃnəl ˈtɜːmɪnl/",
                    meaningVi = "Bạn có thể chỉ giúp tôi đường nhanh nhất để đến nhà ga quốc tế được không?",
                    keyPhrases = new[] { "quickest way", "get to", "international terminal" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson12Id,
                OrderIndex = 2,
                Type = "dictation",
                Title = "Dictation: Tình huống mua sắm & thanh toán",
                Description = "Nghe câu đề nghị thanh toán và kiểm tra độ chính xác.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "Excuse me, do you accept contactless payments or credit cards here?",
                    ipa = "/ɪkˈskjuːs miː, duː juː əkˈsept ˈkɒntæktləs ˈpeɪmənts ɔː ˈkredɪt kɑːdz hɪə/",
                    meaningVi = "Xin lỗi, ở đây có chấp nhận thanh toán chạm hoặc thẻ tín dụng không?",
                    wordChips = new[] { "Excuse", "me", "do", "you", "accept", "contactless", "payments", "or", "credit", "cards", "here" },
                    distractors = new[] { "cash", "exchange", "refund" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson12Id,
                OrderIndex = 3,
                Type = "pronunciation",
                Title = "Chấm phát âm: Câu chỉ đường chuẩn xác",
                Description = "Luyện phát âm từ vựng chỉ phương hướng và khoảng cách.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "Go straight ahead for two blocks, then turn left at the intersection.",
                    ipa = "/ɡəʊ streɪt əˈhed fɔː tuː blɒks, ðen tɜːn left æt ði ˈɪntəsekʃn/",
                    meaningVi = "Đi thẳng hai dãy nhà nữa, sau đó rẽ trái ở ngã tư.",
                    targetWords = new[] { "straight", "ahead", "blocks", "turn", "intersection" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson12Id,
                OrderIndex = 4,
                Type = "word_map",
                Title = "Sơ đồ từ vựng: Direction & Navigation",
                Description = "Khám phá sơ đồ tư duy về từ khóa navigate và các cụm từ chỉ hướng.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    centralWord = "navigate",
                    ipa = "/ˈnævɪɡeɪt/",
                    meaningVi = "Định vị, dò đường, vượt qua tình huống phức tạp",
                    collocations = new object[]
                    {
                        new { text = "navigate the city", ipa = "/ˈnævɪɡeɪt ðə ˈsɪti/", meaningVi = "Dò đường trong thành phố", example = "Google Maps makes it easy to navigate unfamiliar cities." },
                        new { text = "navigate challenges", ipa = "/ˈnævɪɡeɪt ˈtʃælɪndʒɪz/", meaningVi = "Vượt qua thử thách", example = "We successfully navigated several logistical hurdles." }
                    },
                    phrasalVerbs = new object[]
                    {
                        new { text = "head towards", ipa = "/hed təˈwɔːdz/", meaningVi = "Đi về hướng", example = "Head towards the landmark and you will spot the station." },
                        new { text = "turn out", ipa = "/tɜːn aʊt/", meaningVi = "Hóa ra là", example = "The shortcut turned out to be much faster." }
                    },
                    synonyms = new object[]
                    {
                        new { text = "guide", ipa = "/ɡaɪd/", meaningVi = "Dẫn đường", example = "She kindly guided us through the market." },
                        new { text = "maneuver", ipa = "/məˈnuːvə/", meaningVi = "Điều động, xoay xở", example = "He maneuvered his luggage through the crowded lobby." }
                    },
                    contextExamples = new object[]
                    {
                        new { sentence = "Navigating public transit in a foreign country builds great confidence.", meaningVi = "Tự đi lại bằng phương tiện công cộng ở nước ngoài rèn luyện sự tự tin rất lớn." }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Lesson 2.1 Activities
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson21Id,
                OrderIndex = 1,
                Type = "shadowing",
                Title = "Shadowing: Viết email công việc và cập nhật tiến độ",
                Description = "Nghe và phát âm câu email công sở chuẩn xác.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "I am writing to provide a concise update regarding our project milestones and next deliverables.",
                    ipa = "/aɪ æm ˈraɪtɪŋ tuː prəˈvaɪd ə kənˈsaɪs ˌʌpˈdeɪt rɪˈɡɑːdɪŋ ˈaʊə ˈprɒdʒekt ˈmaɪlstəʊnz ænd nekst dɪˈlɪvərəblz/",
                    meaningVi = "Tôi viết thư này nhằm cung cấp cập nhật cô đọng về các mốc dự án và sản phẩm bàn giao tiếp theo.",
                    keyPhrases = new[] { "concise update", "project milestones", "next deliverables" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson21Id,
                OrderIndex = 2,
                Type = "dictation",
                Title = "Dictation: Lời đề nghị đặt lịch hẹn",
                Description = "Nghe câu hẹn lịch gặp và gõ lại đúng chính tả.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "Would you be available for a brief sync tomorrow afternoon to review the proposed budget?",
                    ipa = "/wʊd juː biː əˈveɪləbl fɔːr ə briːf sɪŋk təˈmɒrəʊ ˌɑːftəˈnuːn tuː rɪˈvjuː ðə prəˈpəʊzd ˈbʌdʒɪt/",
                    meaningVi = "Anh/chị có tiện dành chút thời gian họp nhanh vào chiều mai để xem qua ngân sách đề xuất không?",
                    wordChips = new[] { "Would", "you", "be", "available", "for", "a", "brief", "sync", "tomorrow", "afternoon", "to", "review", "the", "proposed", "budget" },
                    distractors = new[] { "schedule", "reschedule", "postpone" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson21Id,
                OrderIndex = 3,
                Type = "pronunciation",
                Title = "Chấm phát âm: Đề xuất giải pháp bằng văn phong trang trọng",
                Description = "Đọc câu đề xuất giải pháp với ngữ điệu tự tin, lịch thiệp.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "We recommend prioritizing automated testing to streamline future release cycles.",
                    ipa = "/wiː ˌrekəˈmend praɪˈɒrətaɪzɪŋ ˈɔːtəmeɪtɪd ˈtestɪŋ tuː ˈstriːmlaɪn ˈfjuːtʃə rɪˈliːs ˈsaɪklz/",
                    meaningVi = "Chúng tôi khuyến nghị ưu tiên kiểm thử tự động để tối ưu hóa chu kỳ phát hành trong tương lai.",
                    targetWords = new[] { "recommend", "prioritizing", "automated", "streamline", "release", "cycles" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson21Id,
                OrderIndex = 4,
                Type = "word_map",
                Title = "Sơ đồ từ vựng: Prioritize & Workflow Optimization",
                Description = "Khám phá sơ đồ tư duy quanh từ khóa prioritize và lưu vào Flashcard FSRS.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    centralWord = "prioritize",
                    ipa = "/praɪˈɒrətaɪz/",
                    meaningVi = "Ưu tiên, đặt lên hàng đầu",
                    collocations = new object[]
                    {
                        new { text = "prioritize tasks", ipa = "/praɪˈɒrətaɪz tɑːsks/", meaningVi = "Sắp xếp thứ tự ưu tiên công việc", example = "Product managers must prioritize tasks based on user impact." },
                        new { text = "top priority", ipa = "/tɒp praɪˈɒrəti/", meaningVi = "Ưu tiên hàng đầu", example = "System stability is our team's top priority this quarter." }
                    },
                    phrasalVerbs = new object[]
                    {
                        new { text = "focus on", ipa = "/ˈfəʊkəs ɒn/", meaningVi = "Tập trung vào", example = "Let's focus on resolving the blocking defects first." },
                        new { text = "set aside", ipa = "/set əˈsaɪd/", meaningVi = "Tạm gác lại", example = "We set aside minor feature requests for the next sprint." }
                    },
                    synonyms = new object[]
                    {
                        new { text = "emphasize", ipa = "/ˈemfəsaɪz/", meaningVi = "Nhấn mạnh, chú trọng", example = "The team emphasizes clean code architecture." },
                        new { text = "highlight", ipa = "/ˈhaɪlaɪt/", meaningVi = "Nêu bật", example = "The report highlights major risk factors." }
                    },
                    contextExamples = new object[]
                    {
                        new { sentence = "Prioritizing high-leverage activities maximizes overall team productivity.", meaningVi = "Ưu tiên các hoạt động có đòn bẩy cao giúp tối đa hóa năng suất toàn đội ngũ." }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Lesson 2.2 Activities
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson22Id,
                OrderIndex = 1,
                Type = "shadowing",
                Title = "Shadowing: Trình bày quan điểm trong cuộc họp",
                Description = "Nghe và nói câu phát biểu đóng góp ý kiến trong buổi họp.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "From my perspective, allocating more resources to documentation will reduce onboarding time.",
                    ipa = "/frɒm maɪ pəˈspektɪv, ˈæləkeɪtɪŋ mɔː rɪˈsɔːsɪz tuː ˌdɒkjumenˈteɪʃn wɪl rɪˈdjuːs ˈɒnbɔːdɪŋ taɪm/",
                    meaningVi = "Theo góc nhìn của tôi, việc phân bổ thêm nguồn lực cho tài liệu hướng dẫn sẽ giúp giảm thời gian tiếp nhận nhân sự mới.",
                    keyPhrases = new[] { "from my perspective", "allocating resources", "onboarding time" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson22Id,
                OrderIndex = 2,
                Type = "dictation",
                Title = "Dictation: Phản biện mang tính xây dựng",
                Description = "Nghe câu phản biện lịch sự và chép lại.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "I see your point, but we should also evaluate the long-term maintenance costs.",
                    ipa = "/aɪ siː jɔː pɔɪnt, bʌt wiː ʃʊd ˈɔːlsəʊ ɪˈvæljueɪt ðə ˈlɒŋ tɜːm ˈmeɪntənəns kɒsts/",
                    meaningVi = "Tôi hiểu ý của bạn, nhưng chúng ta cũng nên đánh giá chi phí bảo trì lâu dài.",
                    wordChips = new[] { "I", "see", "your", "point", "but", "we", "should", "also", "evaluate", "the", "long-term", "maintenance", "costs" },
                    distractors = new[] { "disagree", "reject", "however" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson22Id,
                OrderIndex = 3,
                Type = "pronunciation",
                Title = "Chấm phát âm: Thống nhất phương án hành động",
                Description = "Luyện phát âm câu chốt hành động sau cuộc họp.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "Let us summarize the action items and assign clear ownership for each deliverable.",
                    ipa = "/let ʌs ˈsʌməraɪz ði ˈækʃn ˈaɪtəmz ænd əˈsaɪn klɪər ˈəʊnəʃɪp fɔːr iːtʃ dɪˈlɪvərəbl/",
                    meaningVi = "Chúng ta hãy tóm tắt các đầu việc cần làm và phân công người chịu trách nhiệm rõ ràng cho từng sản phẩm bàn giao.",
                    targetWords = new[] { "summarize", "action", "items", "assign", "ownership", "deliverable" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson22Id,
                OrderIndex = 4,
                Type = "word_map",
                Title = "Sơ đồ từ vựng: Collaborate & Cross-Functional Teams",
                Description = "Khám phá sơ đồ tư duy từ khóa collaborate và tạo thẻ Flashcard FSRS.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    centralWord = "collaborate",
                    ipa = "/kəˈlæbəreɪt/",
                    meaningVi = "Hợp tác, cộng tác làm việc cùng nhau",
                    collocations = new object[]
                    {
                        new { text = "collaborate closely", ipa = "/kəˈlæbəreɪt ˈkləʊsli/", meaningVi = "Hợp tác chặt chẽ", example = "Engineering and product teams collaborate closely on roadmap planning." },
                        new { text = "cross-disciplinary collaboration", ipa = "/krɒs ˈdɪsəplɪnəri kəˌlæbəˈreɪʃn/", meaningVi = "Hợp tác liên ngành", example = "Cross-disciplinary collaboration drives remarkable breakthroughs." }
                    },
                    phrasalVerbs = new object[]
                    {
                        new { text = "team up with", ipa = "/tiːm ʌp wɪð/", meaningVi = "Hợp tác cùng", example = "We teamed up with our design agency to launch the redesign." },
                        new { text = "bounce ideas off", ipa = "/baʊns aɪˈdɪəz ɒf/", meaningVi = "Trao đổi ý kiến thử", example = "I'd love to bounce a few ideas off you before the presentation." }
                    },
                    synonyms = new object[]
                    {
                        new { text = "cooperate", ipa = "/kəʊˈɒpəreɪt/", meaningVi = "Hợp tác", example = "Both teams cooperated smoothly to meet the launch deadline." },
                        new { text = "coordinate", ipa = "/kəʊˈɔːdɪneɪt/", meaningVi = "Phối hợp điều phối", example = "We need to coordinate the rollout schedule." }
                    },
                    contextExamples = new object[]
                    {
                        new { sentence = "Effective collaboration requires transparent communication and shared accountability.", meaningVi = "Sự hợp tác hiệu quả đòi hỏi sự giao tiếp minh bạch và trách nhiệm chung." }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Lesson 3.1 Activities
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson31Id,
                OrderIndex = 1,
                Type = "shadowing",
                Title = "Shadowing: Mở đầu bài thuyết trình hấp dẫn",
                Description = "Nghe và phát âm câu mở đầu lôi cuốn sự chú ý của khán giả.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "Today I would like to demonstrate how continuous learning accelerates professional development.",
                    ipa = "/təˈdeɪ aɪ wʊd laɪk tuː ˈdemənstreɪt haʊ kənˈtɪnjuəs ˈlɜːnɪŋ əkˈseləreɪts prəˈfeʃənl dɪˈveləpmənt/",
                    meaningVi = "Hôm nay tôi muốn chứng minh việc học tập liên tục tăng tốc sự phát triển nghề nghiệp như thế nào.",
                    keyPhrases = new[] { "demonstrate", "continuous learning", "accelerates development" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson31Id,
                OrderIndex = 2,
                Type = "dictation",
                Title = "Dictation: Dẫn dắt chuyển ý (Signposting)",
                Description = "Nghe câu chuyển ý trong bài nói và chép lại.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "Now that we have covered the theoretical foundation, let us examine the practical implications.",
                    ipa = "/naʊ ðæt wiː hæv ˈkʌvəd ðə ˌθɪəˈretɪkl faʊnˈdeɪʃn, let ʌs ɪɡˈzæmɪn ðə ˈpræktɪkl ˌɪmplɪˈkeɪʃnz/",
                    meaningVi = "Sau khi đã đi qua phần cơ sở lý thuyết, bây giờ chúng ta hãy cùng xem xét các ứng dụng thực tế.",
                    wordChips = new[] { "Now", "that", "we", "have", "covered", "the", "theoretical", "foundation", "let", "us", "examine", "the", "practical", "implications" },
                    distractors = new[] { "furthermore", "subsequently", "contrary" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson31Id,
                OrderIndex = 3,
                Type = "pronunciation",
                Title = "Chấm phát âm: Đúc kết thông điệp thuyết trình",
                Description = "Luyện phát âm câu kết luận bài nói với sức nặng và sự thuyết phục.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "In conclusion, disciplined daily practice yields compound interest in communication mastery.",
                    ipa = "/ɪn kənˈkluːʒn, ˈdɪsəplɪnd ˈdeɪli ˈpræktɪs jiːldz ˈkɒmpaʊnd ˈɪntrəst ɪn kəˌmjuːnɪˈkeɪʃn ˈmɑːstəri/",
                    meaningVi = "Tóm lại, sự luyện tập có kỷ luật mỗi ngày mang lại hiệu ứng lãi kép cho việc làm chủ kỹ năng giao tiếp.",
                    targetWords = new[] { "conclusion", "disciplined", "practice", "compound", "communication", "mastery" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson31Id,
                OrderIndex = 4,
                Type = "word_map",
                Title = "Sơ đồ từ vựng: Demonstrate & Visual Communication",
                Description = "Khám phá sơ đồ tư duy từ khóa demonstrate và tạo thẻ Flashcard FSRS.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    centralWord = "demonstrate",
                    ipa = "/ˈdemənstreɪt/",
                    meaningVi = "Chứng minh, thị phạm, làm rõ",
                    collocations = new object[]
                    {
                        new { text = "demonstrate capability", ipa = "/ˈdemənstreɪt ˌkeɪpəˈbɪləti/", meaningVi = "Chứng minh năng lực", example = "The prototype clearly demonstrates our technical capability." },
                        new { text = "demonstrate commitment", ipa = "/ˈdemənstreɪt kəˈmɪtmənt/", meaningVi = "Thể hiện sự cam kết", example = "The founder demonstrated deep commitment to customer satisfaction." }
                    },
                    phrasalVerbs = new object[]
                    {
                        new { text = "point out", ipa = "/pɔɪnt aʊt/", meaningVi = "Chỉ ra", example = "The speaker pointed out several hidden assumptions." },
                        new { text = "spell out", ipa = "/spel aʊt/", meaningVi = "Giải thích cặn kẽ", example = "Let me spell out the primary benefits for you." }
                    },
                    synonyms = new object[]
                    {
                        new { text = "illustrate", ipa = "/ˈɪləstreɪt/", meaningVi = "Minh họa", example = "This diagram illustrates the message processing pipeline." },
                        new { text = "exemplify", ipa = "/ɪɡˈzemplɪfaɪ/", meaningVi = "Là ví dụ điển hình", example = "His presentation exemplified concise storytelling." }
                    },
                    contextExamples = new object[]
                    {
                        new { sentence = "A well-structured presentation demonstrates analytical rigor and audience empathy.", meaningVi = "Một bài thuyết trình có cấu trúc tốt thể hiện sự chặt chẽ về phân tích và sự thấu cảm với khán giả." }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Lesson 3.2 Activities
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson32Id,
                OrderIndex = 1,
                Type = "shadowing",
                Title = "Shadowing: Đàm phán và tìm giải pháp đôi bên cùng có lợi",
                Description = "Nghe và nói câu đàm phán nguyên tắc win-win.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "Our objective is to reach a mutually beneficial agreement that safeguards both parties' long-term interests.",
                    ipa = "/ˈaʊər əbˈdʒektɪv ɪz tuː riːtʃ ə ˈmjuːtʃuəli ˌbenɪˈfɪʃl əˈɡriːmənt ðæt ˈseɪfɡɑːdz bəʊθ ˈpɑːtiz ˈlɒŋ tɜːm ˈɪntrəsts/",
                    meaningVi = "Mục tiêu của chúng tôi là đạt được thỏa thuận đôi bên cùng có lợi nhằm bảo vệ lợi ích dài hạn của cả hai phía.",
                    keyPhrases = new[] { "mutually beneficial", "safeguards interests", "long-term agreement" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson32Id,
                OrderIndex = 2,
                Type = "dictation",
                Title = "Dictation: Nhượng bộ chiến lược có điều kiện",
                Description = "Nghe câu đàm phán có điều kiện và chép lại.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "We could adjust the delivery schedule on condition that the scope remains clearly defined.",
                    ipa = "/wiː kʊd əˈdʒʌst ðə dɪˈlɪvəri ˈʃedjuːl ɒn kənˈdɪʃn ðæt ðə skəʊp rɪˈmeɪnz ˈklɪəli dɪˈfaɪnd/",
                    meaningVi = "Chúng tôi có thể điều chỉnh tiến độ giao hàng với điều kiện là phạm vi công việc vẫn được xác định rõ ràng.",
                    wordChips = new[] { "We", "could", "adjust", "the", "delivery", "schedule", "on", "condition", "that", "the", "scope", "remains", "clearly", "defined" },
                    distractors = new[] { "however", "provided", "otherwise" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson32Id,
                OrderIndex = 3,
                Type = "pronunciation",
                Title = "Chấm phát âm: Đặt câu hỏi làm rõ sự từ chối",
                Description = "Luyện phát âm câu hỏi làm rõ để mở rộng không gian đàm phán.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    sentence = "Could you elaborate on the main constraints preventing your team from adopting this timeline?",
                    ipa = "/kəd juː ɪˈlæbəreɪt ɒn ðə meɪn kənˈstreɪnts prɪˈventɪŋ jɔː tiːm frəm əˈdɒptɪŋ ðɪs ˈtaɪmlaɪn/",
                    meaningVi = "Bạn có thể chia sẻ cụ thể hơn về những rào cản chính khiến đội ngũ của bạn chưa thể áp dụng mốc thời gian này được không?",
                    targetWords = new[] { "elaborate", "constraints", "preventing", "adopting", "timeline" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = EnglishCourseId,
                LessonId = EnglishLesson32Id,
                OrderIndex = 4,
                Type = "word_map",
                Title = "Sơ đồ từ vựng: Negotiate & Principled Compromise",
                Description = "Khám phá sơ đồ tư duy từ khóa negotiate và lưu vào Flashcard FSRS.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    centralWord = "negotiate",
                    ipa = "/nɪˈɡəʊʃieɪt/",
                    meaningVi = "Đàm phán, thương lượng, dàn xếp",
                    collocations = new object[]
                    {
                        new { text = "negotiate terms", ipa = "/nɪˈɡəʊʃieɪt tɜːmz/", meaningVi = "Thương lượng điều khoản", example = "We spent weeks negotiating favorable contractual terms." },
                        new { text = "successful negotiation", ipa = "/səkˈsesfl ˌneɡəʊʃiˈeɪʃn/", meaningVi = "Cuộc đàm phán thành công", example = "The successful negotiation laid the groundwork for future expansion." }
                    },
                    phrasalVerbs = new object[]
                    {
                        new { text = "settle on", ipa = "/ˈsetl ɒn/", meaningVi = "Đồng ý chốt lại ở", example = "After long debate, they settled on a compromise figure." },
                        new { text = "iron out", ipa = "/ˈaɪən aʊt/", meaningVi = "Giải quyết các khúc mắc", example = "We still need to iron out a few technical details." }
                    },
                    synonyms = new object[]
                    {
                        new { text = "bargain", ipa = "/ˈbɑːɡən/", meaningVi = "Mặc cả, thương lượng", example = "Both partners bargained hard for equity shares." },
                        new { text = "arbitrate", ipa = "/ˈɑːbɪtreɪt/", meaningVi = "Đứng ra hòa giải", example = "A neutral advisor helped arbitrate the dispute." }
                    },
                    contextExamples = new object[]
                    {
                        new { sentence = "Mastering principled negotiation enables you to resolve conflicts with grace and confidence.", meaningVi = "Làm chủ nghệ thuật đàm phán nguyên tắc cho phép bạn giải quyết các xung đột một cách khéo léo và tự tin." }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // =========================================================================
            // Lesson 1.1 Extended Schema Activities (Idempotent OrderIndex 5..10)
            // =========================================================================
            new PracticeActivity
            {
                Id = CreateDeterministicActivityId(targetCourseId, EnglishLesson11Id, "shadowing", 5),
                CourseId = targetCourseId,
                LessonId = EnglishLesson11Id,
                OrderIndex = 5,
                Type = "shadowing",
                Title = "Shadowing mở rộng: Phân tầng Dễ → Vừa → Khó (âm th & âm cuối)",
                Description = "Bộ câu luyện phát âm đa cấp độ tập trung vào âm /θ/-/ð/, âm cuối và đuôi -s/-ed hay gặp của người Việt.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    lang = "en-US",
                    difficulty = "medium",
                    items = new object[]
                    {
                        new
                        {
                            id = "en-l11-shad-easy",
                            lang = "en-US",
                            difficulty = "easy",
                            topic = "Mở đầu hội thoại (Dễ)",
                            sentence = "Thank you for joining this workshop with us today.",
                            ipa = "/θæŋk juː fɔː ˈdʒɔɪnɪŋ ðɪs ˈwɜːkʃɒp wɪð ʌs təˈdeɪ/",
                            meaningVi = "Cảm ơn bạn đã tham gia buổi workshop cùng chúng tôi hôm nay.",
                            keyPhrases = new[] { "thank you", "this workshop", "with us" }
                        },
                        new
                        {
                            id = "en-l11-shad-medium",
                            lang = "en-US",
                            difficulty = "medium",
                            topic = "Giới thiệu công việc (Vừa)",
                            sentence = "I work on software projects that help teams build strong trust.",
                            ipa = "/aɪ wɜːk ɒn ˈsɒftweə ˈprɒdʒekts ðæt helps tiːmz bɪld strɒŋ trʌst/",
                            meaningVi = "Tôi làm việc trong các dự án phần mềm giúp đội nhóm xây dựng niềm tin vững chắc.",
                            keyPhrases = new[] { "software projects", "build strong trust", "work on" }
                        },
                        new
                        {
                            id = "en-l11-shad-hard",
                            lang = "en-US",
                            difficulty = "hard",
                            topic = "Kết nối chuyên nghiệp (Khó)",
                            sentence = "Although we walked into the conference as strangers, we established meaningful relationships.",
                            ipa = "/ɔːlˈðəʊ wiː wɔːkt ˈɪntuː ðə ˈkɒnfrəns æz ˈstreɪndʒəz, wiː ɪˈstæblɪʃt ˈmiːnɪŋfl rɪˈleɪʃnʃɪps/",
                            meaningVi = "Dù bước vào hội nghị như những người xa lạ, chúng tôi đã xây dựng được những mối quan hệ đầy ý nghĩa.",
                            keyPhrases = new[] { "walked into", "established relationships", "meaningful relationships" }
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = CreateDeterministicActivityId(targetCourseId, EnglishLesson11Id, "word_map", 6),
                CourseId = targetCourseId,
                LessonId = EnglishLesson11Id,
                OrderIndex = 6,
                Type = "word_map",
                Title = "Sơ đồ từ vựng 2: Impression & Networking Confidence",
                Description = "Mở rộng vốn cụm từ quanh từ khóa impression để tự tin tạo thiện cảm khi làm quen.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    lang = "en-US",
                    difficulty = "medium",
                    centralWord = "impression",
                    ipa = "/ɪmˈpreʃn/",
                    meaningVi = "Ấn tượng, cảm nhận ban đầu",
                    collocations = new object[]
                    {
                        new { text = "make a good impression", ipa = "/meɪk ə ɡʊd ɪmˈpreʃn/", meaningVi = "Tạo ấn tượng tốt", example = "Listening attentively helps you make a good impression." },
                        new { text = "first impression", ipa = "/fɜːst ɪmˈpreʃn/", meaningVi = "Ấn tượng đầu tiên", example = "A warm smile creates a lasting first impression." },
                        new { text = "lasting impression", ipa = "/ˈlɑːstɪŋ ɪmˈpreʃn/", meaningVi = "Ấn tượng sâu đậm lâu dài", example = "Her concise introduction left a lasting impression." }
                    },
                    phrasalVerbs = new object[]
                    {
                        new { text = "come across as", ipa = "/kʌm əˈkrɒs æz/", meaningVi = "Tạo cảm giác là người như thế nào", example = "He came across as approachable and genuinely curious." },
                        new { text = "follow up with", ipa = "/ˈfɒləʊ ʌp wɪð/", meaningVi = "Liên hệ nối tiếp sau khi gặp", example = "Remember to follow up with new contacts after the event." }
                    },
                    synonyms = new object[]
                    {
                        new { text = "perception", ipa = "/pəˈsepʃn/", meaningVi = "Sự nhìn nhận, cảm nhận", example = "Clear body language shapes positive perception." },
                        new { text = "impact", ipa = "/ˈɪmpækt/", meaningVi = "Tác động, dấu ấn", example = "Genuine curiosity leaves a strong impact." }
                    },
                    contextExamples = new object[]
                    {
                        new { sentence = "Making a genuine first impression opens doors to fruitful collaboration.", meaningVi = "Tạo ấn tượng ban đầu chân thành mở ra cánh cửa cho sự hợp tác hiệu quả." }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = CreateDeterministicActivityId(targetCourseId, EnglishLesson11Id, "cloze", 7),
                CourseId = targetCourseId,
                LessonId = EnglishLesson11Id,
                OrderIndex = 7,
                Type = "cloze",
                Title = "Điền chỗ trống (Cloze): Small Talk & Tự giới thiệu",
                Description = "Tự gõ từ còn thiếu từ trí nhớ cho các mẫu câu mở đầu hội thoại theo 3 mức độ Dễ / Vừa / Khó.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    lang = "en-US",
                    items = new object[]
                    {
                        new
                        {
                            id = "en-l11-cloze-1",
                            difficulty = "easy",
                            title = "Chào hỏi lần đầu (Dễ)",
                            promptVi = "Điền 2 từ còn thiếu để hoàn thành câu làm quen tự nhiên:",
                            template = "Hi there! Is this {{0}} taken? It is a {{1}} to meet you.",
                            blanks = new object[]
                            {
                                new { acceptedAnswers = new[] { "seat", "chair" }, hintVi = "chỗ ngồi (bắt đầu bằng s)", explanationVi = "\"Is this seat taken?\" = Ghế này có ai ngồi chưa?" },
                                new { acceptedAnswers = new[] { "pleasure" }, hintVi = "niềm hân hạnh (bắt đầu bằng p)", explanationVi = "\"It is a pleasure to meet you.\" = Rất hân hạnh được gặp bạn." }
                            },
                            fullSentence = "Hi there! Is this seat taken? It is a pleasure to meet you.",
                            meaningVi = "Chào bạn! Ghế này có ai ngồi chưa? Rất hân hạnh được gặp bạn.",
                            explanationVi = "Hai mẫu câu mở lời cực kỳ tự nhiên tại hội thảo hoặc quán cà phê."
                        },
                        new
                        {
                            id = "en-l11-cloze-2",
                            difficulty = "medium",
                            title = "Phá băng & giới thiệu công việc (Vừa)",
                            promptVi = "Điền cụm từ mở đầu và nơi làm việc:",
                            template = "Telling a light joke helped {{0}} the ice. I am a software engineer {{1}} in Hanoi.",
                            blanks = new object[]
                            {
                                new { acceptedAnswers = new[] { "break", "to break" }, hintVi = "phá vỡ (___ the ice)", explanationVi = "\"break the ice\" = phá vỡ sự ngại ngùng ban đầu." },
                                new { acceptedAnswers = new[] { "based", "working", "living" }, hintVi = "đặt trụ sở / làm việc tại (bắt đầu bằng b)", explanationVi = "\"based in Hanoi\" là cách diễn đạt rất tự nhiên khi giới thiệu nơi làm việc." }
                            },
                            fullSentence = "Telling a light joke helped break the ice. I am a software engineer based in Hanoi.",
                            meaningVi = "Kể một câu chuyện vui nhẹ giúp phá vỡ sự ngại ngùng. Tôi là kỹ sư phần mềm làm việc tại Hà Nội.",
                            explanationVi = "Dùng \"based in + thành phố\" để giới thiệu địa điểm công tác chuyên nghiệp."
                        },
                        new
                        {
                            id = "en-l11-cloze-3",
                            difficulty = "hard",
                            title = "Mong đợi hợp tác lâu dài (Khó)",
                            promptVi = "Điền 2 từ học thuật/công sở để hoàn chỉnh câu kết nối:",
                            template = "I look {{0}} to our fruitful {{1}} on upcoming international projects.",
                            blanks = new object[]
                            {
                                new { acceptedAnswers = new[] { "forward" }, hintVi = "mong đợi (look ___ to)", explanationVi = "\"look forward to + danh từ/V-ing\" = mong đợi điều gì." },
                                new { acceptedAnswers = new[] { "collaboration", "cooperation", "partnership" }, hintVi = "sự hợp tác (bắt đầu bằng c)", explanationVi = "\"fruitful collaboration\" = sự hợp tác hiệu quả, sinh trái ngọt." }
                            },
                            fullSentence = "I look forward to our fruitful collaboration on upcoming international projects.",
                            meaningVi = "Tôi rất mong đợi sự hợp tác hiệu quả của chúng ta trong các dự án quốc tế sắp tới.",
                            explanationVi = "Cụm \"fruitful collaboration\" nâng tầm văn phong giao tiếp lên trình độ B1–B2."
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = CreateDeterministicActivityId(targetCourseId, EnglishLesson11Id, "translate", 8),
                CourseId = targetCourseId,
                LessonId = EnglishLesson11Id,
                OrderIndex = 8,
                Type = "translate",
                Title = "Dịch Việt → Anh: Phản xạ câu Small Talk",
                Description = "Chuyển câu tiếng Việt sang tiếng Anh tự nhiên với nhiều đáp án được chấp nhận.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    lang = "en-US",
                    items = new object[]
                    {
                        new
                        {
                            id = "en-l11-trans-1",
                            difficulty = "easy",
                            title = "Giới thiệu bản thân (Dễ)",
                            sourceVi = "Xin chào, tôi là Mai và tôi là kỹ sư phần mềm tại Hà Nội.",
                            hintKeywords = new[] { "software engineer", "based in Hanoi", "I am" },
                            acceptedAnswers = new[]
                            {
                                "Hello, I am Mai and I am a software engineer in Hanoi.",
                                "Hi, I'm Mai and I'm a software engineer based in Hanoi.",
                                "Hello, I'm Mai and I'm a software engineer based in Hanoi.",
                                "Hi, I am Mai, a software engineer in Hanoi."
                            },
                            sampleAnswer = "Hi, I'm Mai and I'm a software engineer based in Hanoi.",
                            grammarNoteVi = "Nhớ dùng mạo từ \"a\" trước nghề nghiệp số ít: \"a software engineer\"."
                        },
                        new
                        {
                            id = "en-l11-trans-2",
                            difficulty = "medium",
                            title = "Mở lời tại hội thảo (Vừa)",
                            sourceVi = "Hôm nay bạn có tham dự buổi hội thảo công nghệ này không?",
                            hintKeywords = new[] { "attending", "tech workshop", "today" },
                            acceptedAnswers = new[]
                            {
                                "Are you attending this tech workshop today?",
                                "Are you attending the tech workshop today?",
                                "Are you joining this tech workshop today?",
                                "Are you attending this technology workshop today?"
                            },
                            sampleAnswer = "Are you attending this tech workshop today?",
                            grammarNoteVi = "Dùng thì hiện tại tiếp diễn \"Are you attending...\" cho sự kiện đang/sắp diễn ra trong ngày."
                        },
                        new
                        {
                            id = "en-l11-trans-3",
                            difficulty = "hard",
                            title = "Xây dựng niềm tin lâu dài (Khó)",
                            sourceVi = "Tạo ấn tượng đầu tiên tích cực giúp xây dựng niềm tin lâu dài trong các mối quan hệ công việc.",
                            hintKeywords = new[] { "positive first impression", "build long-term trust", "professional relationships" },
                            acceptedAnswers = new[]
                            {
                                "Making a positive first impression helps build long-term trust in professional relationships.",
                                "Making a positive first impression helps to build long-term trust in professional relationships.",
                                "Creating a positive first impression helps build long-term trust in work relationships."
                            },
                            sampleAnswer = "Making a positive first impression helps build long-term trust in professional relationships.",
                            grammarNoteVi = "Cụm V-ing \"Making a positive first impression\" đóng vai trò chủ ngữ số ít nên động từ chia \"helps\"."
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = CreateDeterministicActivityId(targetCourseId, EnglishLesson11Id, "grammar_fix", 9),
                CourseId = targetCourseId,
                LessonId = EnglishLesson11Id,
                OrderIndex = 9,
                Type = "grammar_fix",
                Title = "Sửa lỗi ngữ pháp: Các lỗi giao tiếp hay gặp",
                Description = "Phát hiện và viết lại câu tiếng Anh chuẩn ngữ pháp cho các tình huống làm quen.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    lang = "en-US",
                    items = new object[]
                    {
                        new
                        {
                            id = "en-l11-gf-1",
                            difficulty = "easy",
                            title = "Thiếu mạo từ nghề nghiệp (Dễ)",
                            incorrectSentence = "I am software engineer and I live in Hanoi.",
                            errorTypeHintVi = "Thiếu mạo từ bất định (a/an) trước danh từ nghề nghiệp đếm được số ít.",
                            correctedSentence = "I am a software engineer and I live in Hanoi.",
                            acceptedAnswers = new[]
                            {
                                "I am a software engineer and I live in Hanoi.",
                                "I'm a software engineer and I live in Hanoi."
                            },
                            meaningVi = "Tôi là một kỹ sư phần mềm và tôi sống ở Hà Nội.",
                            explanationVi = "Trong tiếng Anh luôn phải có mạo từ \"a/an\" trước nghề nghiệp số ít (a teacher, an engineer, a software engineer)."
                        },
                        new
                        {
                            id = "en-l11-gf-2",
                            difficulty = "medium",
                            title = "Cấu trúc look forward to + V-ing (Vừa)",
                            incorrectSentence = "I look forward to learn more about your recent projects.",
                            errorTypeHintVi = "Động từ đứng sau cụm \"look forward to\" phải chia dạng V-ing.",
                            correctedSentence = "I look forward to learning more about your recent projects.",
                            acceptedAnswers = new[]
                            {
                                "I look forward to learning more about your recent projects.",
                                "I am looking forward to learning more about your recent projects."
                            },
                            meaningVi = "Tôi rất mong được tìm hiểu thêm về các dự án gần đây của bạn.",
                            explanationVi = "\"to\" trong \"look forward to\" là giới từ nên động từ theo sau bắt buộc thêm -ing (learning)."
                        },
                        new
                        {
                            id = "en-l11-gf-3",
                            difficulty = "hard",
                            title = "Hòa hợp chủ ngữ V-ing & danh từ số nhiều (Khó)",
                            incorrectSentence = "Making a positive first impression help build long-term trust in professional relationship.",
                            errorTypeHintVi = "Kiểm tra chia động từ theo chủ ngữ số ít (Making...) và số nhiều của danh từ đếm được (relationship).",
                            correctedSentence = "Making a positive first impression helps build long-term trust in professional relationships.",
                            acceptedAnswers = new[]
                            {
                                "Making a positive first impression helps build long-term trust in professional relationships.",
                                "Making a positive first impression helps to build long-term trust in professional relationships."
                            },
                            meaningVi = "Tạo ấn tượng ban đầu tích cực giúp xây dựng niềm tin lâu dài trong các mối quan hệ chuyên nghiệp.",
                            explanationVi = "Chủ ngữ là danh động từ (Making...) luôn chia động từ số ít (helps), và \"relationships\" cần ở số nhiều khi nói chung không có mạo từ."
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = CreateDeterministicActivityId(targetCourseId, EnglishLesson11Id, "roleplay", 10),
                CourseId = targetCourseId,
                LessonId = EnglishLesson11Id,
                OrderIndex = 10,
                Type = "roleplay",
                Title = "Nhập vai hội thoại: Small Talk tại Hội thảo Quốc tế",
                Description = "Trò chuyện phản xạ bằng giọng nói hoặc gõ phím với đồng nghiệp quốc tế (AI chấm trực tiếp hoặc chấm từ khóa offline).",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    lang = "en-US",
                    items = new object[]
                    {
                        new
                        {
                            id = "en-l11-rp-1",
                            difficulty = "easy",
                            title = "Bắt chuyện giờ giải lao (Dễ)",
                            scenarioVi = "Bạn đang ngồi chờ khai mạc hội thảo công nghệ thì một đồng nghiệp người Úc bước tới hỏi mượn ghế bên cạnh.",
                            aiRole = "Alex (Kỹ sư từ Melbourne)",
                            aiPrompt = "Hi there! Is this seat taken? Beautiful morning for a conference, isn't it?",
                            aiPromptVi = "Chào bạn! Ghế này có ai ngồi chưa? Buổi sáng nay đẹp trời cho một buổi hội thảo nhỉ?",
                            userGoalVi = "Mời Alex ngồi tự nhiên, đồng tình về thời tiết/buổi sáng và giới thiệu tên của bạn.",
                            expectedKeywords = new[] { "go ahead", "morning", "name", "meet" },
                            sampleResponses = new[]
                            {
                                "No, please go ahead! Yes, it really is a lovely morning. My name is Mai, nice to meet you!"
                            },
                            minWords = 7
                        },
                        new
                        {
                            id = "en-l11-rp-2",
                            difficulty = "medium",
                            title = "Giới thiệu công việc 3 bước Hook-Background-Passion (Vừa)",
                            scenarioVi = "Alex vừa giới thiệu anh ấy đến từ Melbourne và hỏi về công việc hiện tại của bạn.",
                            aiRole = "Alex (Kỹ sư từ Melbourne)",
                            aiPrompt = "I'm Alex from Melbourne. Are you attending the tech workshop today? What kind of work do you do?",
                            aiPromptVi = "Tôi là Alex từ Melbourne. Hôm nay bạn cũng dự buổi workshop công nghệ à? Bạn đang làm công việc gì thế?",
                            userGoalVi = "Xác nhận tham dự workshop, giới thiệu vai trò công việc + thành phố của bạn và hỏi ngược lại Alex.",
                            expectedKeywords = new[] { "workshop", "based", "work", "pleasure", "about you" },
                            sampleResponses = new[]
                            {
                                "Yes, I am attending the workshop today! I work as a software engineer based in Hanoi. It is a pleasure to meet you—what about you?"
                            },
                            minWords = 10
                        },
                        new
                        {
                            id = "en-l11-rp-3",
                            difficulty = "hard",
                            title = "Kết thúc cuộc trò chuyện & hẹn giữ liên lạc (Khó)",
                            scenarioVi = "Giờ giải lao sắp kết thúc, bạn muốn khép lại cuộc trò chuyện khéo léo và trao đổi LinkedIn để hợp tác sau này.",
                            aiRole = "Alex (Kỹ sư từ Melbourne)",
                            aiPrompt = "It looks like the keynote session is about to start. It was great chatting with you!",
                            aiPromptVi = "Có vẻ phiên thuyết trình chính sắp bắt đầu rồi. Nói chuyện với bạn thật thú vị!",
                            userGoalVi = "Bày tỏ niềm vui khi trò chuyện, đề nghị kết nối (LinkedIn/email) và mong đợi hợp tác hiệu quả trong tương lai.",
                            expectedKeywords = new[] { "pleasure", "connect", "look forward", "collaboration" },
                            sampleResponses = new[]
                            {
                                "It was truly a pleasure talking with you too, Alex! Let's connect on LinkedIn so we can stay in touch. I look forward to our fruitful collaboration."
                            },
                            minWords = 12
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        activities.AddRange(EnglishPracticeContent.GetExtendedPracticeActivities(targetCourseId, now));

        var existingActivities = await db.PracticeActivities
            .Where(pa => pa.CourseId == targetCourseId)
            .ToListAsync();

        var existingIds = existingActivities.Select(pa => pa.Id).ToHashSet();
        var existingCompositeKeys = existingActivities
            .Select(pa => (pa.LessonId, Type: (pa.Type ?? string.Empty).Trim().ToLowerInvariant(), pa.OrderIndex))
            .ToHashSet();

        var activitiesToAdd = new List<PracticeActivity>();
        foreach (var act in activities)
        {
            act.CourseId = targetCourseId;
            var compositeKey = (act.LessonId, Type: (act.Type ?? string.Empty).Trim().ToLowerInvariant(), act.OrderIndex);
            if (existingIds.Contains(act.Id) || existingCompositeKeys.Contains(compositeKey))
            {
                continue;
            }
            existingIds.Add(act.Id);
            existingCompositeKeys.Add(compositeKey);
            activitiesToAdd.Add(act);
        }

        if (activitiesToAdd.Count > 0)
        {
            db.PracticeActivities.AddRange(activitiesToAdd);
            await db.SaveChangesAsync();
        }
    }

    public static async Task EnsurePracticeExpansionCoursesSeededAsync(SwmsDbContext db)
    {
        var now = DateTimeOffset.UtcNow;

        // =========================================================================
        // 1. KHÓA HỌC 1: ĐẦU TƯ & QUẢN TRỊ DANH MỤC THỰC CHIẾN
        // =========================================================================
        var investCourse = await db.Courses.FirstOrDefaultAsync(c => c.Id == InvestingCourseId || c.Slug == "dau-tu-quan-tri-danh-muc-thuc-chien");
        if (investCourse == null)
        {
            investCourse = new Course
            {
                Id = InvestingCourseId,
                InstructorId = DefaultUserId,
                Title = "Đầu tư & Quản trị Danh mục Thực chiến",
                Slug = "dau-tu-quan-tri-danh-muc-thuc-chien",
                Description = "Lộ trình thực chiến từ lãi kép, tích lũy DCA, mô phỏng phân bổ danh mục đa tài sản đến thấu hiểu tâm lý và thiên kiến hành vi của nhà đầu tư.",
                Visibility = "public",
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Courses.Add(investCourse);

            if (!await db.CourseEnrollments.AnyAsync(e => e.UserId == DefaultUserId && e.CourseId == InvestingCourseId))
            {
                db.CourseEnrollments.Add(new CourseEnrollment
                {
                    Id = Guid.NewGuid(),
                    UserId = DefaultUserId,
                    CourseId = InvestingCourseId,
                    Role = "student",
                    EnrolledAt = now
                });
            }
        }

        // Chapters Investing
        var investChapters = new List<Chapter>();
        if (!await db.Chapters.AnyAsync(ch => ch.Id == InvestingChapter1Id))
        {
            investChapters.Add(new Chapter { Id = InvestingChapter1Id, CourseId = InvestingCourseId, Title = "Chương 1: Lãi Kép, Dòng Tiền & Tích Lũy DCA Thực Chiến", SortOrder = 1, CreatedAt = now, UpdatedAt = now });
        }
        if (!await db.Chapters.AnyAsync(ch => ch.Id == InvestingChapter2Id))
        {
            investChapters.Add(new Chapter { Id = InvestingChapter2Id, CourseId = InvestingCourseId, Title = "Chương 2: Xây Dựng & Tối Ưu Hóa Danh Mục Đa Tài Sản", SortOrder = 2, CreatedAt = now, UpdatedAt = now });
        }
        if (!await db.Chapters.AnyAsync(ch => ch.Id == InvestingChapter3Id))
        {
            investChapters.Add(new Chapter { Id = InvestingChapter3Id, CourseId = InvestingCourseId, Title = "Chương 3: Phân Tích Cơ Bản, Định Giá & Tâm Lý Hành Vi", SortOrder = 3, CreatedAt = now, UpdatedAt = now });
        }
        if (investChapters.Count > 0)
        {
            db.Chapters.AddRange(investChapters);
            await db.SaveChangesAsync();
        }

        // Lessons Investing
        var investLessonsToAdd = new List<Lesson>();
        if (!await db.Lessons.AnyAsync(l => l.Id == InvestingLesson11Id))
        {
            var title = "Bài 1.1: Sức Mạnh Lãi Kép & Chiến Lược DCA Định Kỳ";
            var md = @"# Bài 1.1: Sức Mạnh Lãi Kép & Chiến Lược DCA Định Kỳ

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững công thức và bản chất toán học của lãi kép trong đầu tư dài hạn.
- Hiểu và áp dụng chiến lược Dollar-Cost Averaging (DCA) để triệt tiêu áp lực chọn thời điểm thị trường.
- Phân biệt tác động của tỷ lệ lạm phát đối với sức mua thực tế của tài sản tích lũy.

> [!NOTE]
> Albert Einstein từng gọi lãi kép là **kỳ quan thứ tám của thế giới**: *""Ai hiểu được nó sẽ kiếm được tiền từ nó; ai không hiểu sẽ phải trả giá cho nó.""*

## 2. Bản chất của Dollar-Cost Averaging (DCA)
Chiến lược DCA giải phóng nhà đầu tư cá nhân khỏi áp lực phán đoán đỉnh - đáy. Bằng cách phân bổ đều đặn một khoản tiền cố định vào các mốc thời gian cố định:
- Khi thị trường giảm, bạn mua được **nhiều cổ phiếu/chứng chỉ quỹ hơn**.
- Khi thị trường tăng, bạn mua ít hơn nhưng giá trị tài sản đã nắm giữ tăng lên.
- Giá vốn trung bình theo thời gian sẽ thấp hơn đáng kể so với mức giá bình quân giản đơn.

> [!TIP]
> Kỷ luật thực hiện DCA hàng tháng quan trọng hơn nhiều so với việc cố gắng dự đoán xu hướng thị trường ngắn hạn.

## 3. Lạm phát và Sức mua Thực tế
Lợi suất danh nghĩa trừ đi tỷ lệ lạm phát mới là **lợi suất thực tế** gia tăng tài sản của bạn. Do đó, việc nắm giữ tiền mặt thuần túy trong dài hạn đồng nghĩa với việc chấp nhận tài sản bị bào mòn mỗi năm.";
            investLessonsToAdd.Add(new Lesson
            {
                Id = InvestingLesson11Id,
                ChapterId = InvestingChapter1Id,
                Title = title,
                Slug = "suc-manh-lai-kep-va-chien-luoc-dca",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 20,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Sức mạnh lãi kép và kỷ luật DCA định kỳ.", "Triệt tiêu rủi ro tâm lý và bảo vệ sức mua."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == InvestingLesson12Id))
        {
            var title = "Bài 1.2: Kế Hoạch Tự Do Tài Chính (FIRE) & Quản Lý Dòng Tiền";
            var md = @"# Bài 1.2: Kế Hoạch Tự Do Tài Chính (FIRE) & Quản Lý Dòng Tiền

## 1. Mục tiêu bài học (Learning Objectives)
- Hiểu mô hình FIRE (Financial Independence, Retire Early) và quy tắc 4% (Trinity Study).
- Xây dựng bảng dòng tiền cá nhân và xác định tỷ lệ tiết kiệm ròng (Savings Rate).
- Tối ưu hóa thặng dư hàng tháng để đưa vào quỹ tích lũy sinh lời.

> [!IMPORTANT]
> Quy tắc 4% chỉ ra rằng nếu bạn tích lũy được khối tài sản bằng **25 lần chi tiêu năm**, bạn có thể rút 4% mỗi năm mà xác suất cạn vốn trong 30 năm là dưới 5%.

## 2. Ba Trụ Cột Tối Ưu Hóa Dòng Tiền
1. **Kiểm soát chi phí cố định (Needs):** Nhà ở, đi lại, thực phẩm dinh dưỡng.
2. **Loại bỏ rò rỉ vi mô (Micro-leaks):** Các đăng ký dịch vụ không sử dụng, chi tiêu bốc đồng.
3. **Mở rộng nguồn thu nhập:** Gia tăng kỹ năng nghề nghiệp và phát triển dòng thu nhập thứ hai.";
            investLessonsToAdd.Add(new Lesson
            {
                Id = InvestingLesson12Id,
                ChapterId = InvestingChapter1Id,
                Title = title,
                Slug = "ke-hoach-tu-do-tai-chinh-fire-va-dong-tien",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 20,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Kế hoạch tự do tài chính FIRE và quy tắc 4%.", "Tối ưu hóa dòng tiền và quản trị chi tiêu."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == InvestingLesson21Id))
        {
            var title = "Bài 2.1: Phân Bổ Tài Sản & Tương Quan Giữa Các Lớp Tài Sản";
            var md = @"# Bài 2.1: Phân Bổ Tài Sản & Tương Quan Giữa Các Lớp Tài Sản

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững lý thuyết danh mục hiện đại (Modern Portfolio Theory) của Harry Markowitz.
- Hiểu hệ số tương quan (Correlation) giữa Cổ phiếu, Trái phiếu, Vàng và Tiền mặt.
- Thiết lập tỷ trọng danh mục tối ưu phù hợp với khẩu vị rủi ro cá nhân.

> [!NOTE]
> Phân bổ tài sản quyết định tới **hơn 90% độ biến động và kết quả lợi nhuận** của danh mục trong dài hạn, vượt trội hoàn toàn so với việc lựa chọn từng mã cổ phiếu riêng lẻ.

## 2. Năm Lớp Tài Sản Cốt Lõi
- **Cổ phiếu tăng trưởng:** Động lực tạo lợi nhuận cao nhưng biến động mạnh.
- **Cổ phiếu cổ tức:** Mang lại dòng tiền phòng thủ ổn định.
- **Trái phiếu chính phủ:** Giảm biến động danh mục khi thị trường cổ phiếu điều chỉnh.
- **Vàng:** Phòng vệ lạm phát phi mã và rủi ro địa chính trị.
- **Tiền mặt / Tiền gửi:** Thanh khoản tức thì cho cơ hội bắt đáy hoặc chi tiêu khẩn cấp.";
            investLessonsToAdd.Add(new Lesson
            {
                Id = InvestingLesson21Id,
                ChapterId = InvestingChapter2Id,
                Title = title,
                Slug = "phan-bo-tai-san-va-tuong-quan",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Phân bổ tài sản và lý thuyết danh mục hiện đại.", "Tương quan đa tài sản và kiểm soát rủi ro."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == InvestingLesson22Id))
        {
            var title = "Bài 2.2: Quản Trị Rủi Ro & Mức Sụt Giảm Tối Đa (Max Drawdown)";
            var md = @"# Bài 2.2: Quản Trị Rủi Ro & Mức Sụt Giảm Tối Đa (Max Drawdown)

## 1. Mục tiêu bài học (Learning Objectives)
- Định lượng rủi ro danh mục qua Mức sụt giảm tối đa (Max Drawdown) và Độ lệch chuẩn (Volatility).
- Xây dựng kịch bản kiểm thử áp lực (Stress Testing) cho các giai đoạn khủng hoảng lịch sử.
- Thiết lập kỷ luật tái cân bằng danh mục định kỳ (Portfolio Rebalancing).

> [!CAUTION]
> Khi danh mục giảm **50%**, bạn cần mức tăng **100%** chỉ để hòa vốn. Quản trị mức sụt giảm tối đa là chìa khóa sống còn của mọi nhà đầu tư chuyên nghiệp.";
            investLessonsToAdd.Add(new Lesson
            {
                Id = InvestingLesson22Id,
                ChapterId = InvestingChapter2Id,
                Title = title,
                Slug = "quan-tri-rui-ro-va-max-drawdown",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Định lượng rủi ro và quản trị sụt giảm danh mục.", "Stress testing và kỷ luật tái cân bằng."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == InvestingLesson31Id))
        {
            var title = "Bài 3.1: Biên An Toàn & Định Giá Doanh Nghiệp Cơ Bản";
            var md = @"# Bài 3.1: Biên An Toàn & Định Giá Doanh Nghiệp Cơ Bản

## 1. Mục tiêu bài học (Learning Objectives)
- Khái niệm Biên an toàn (Margin of Safety) theo Benjamin Graham và Warren Buffett.
- Đọc hiểu nhanh các chỉ số P/E, P/B, ROE và Dòng tiền tự do (Free Cash Flow).
- Phân biệt giữa biến động giá ngắn hạn và giá trị nội tại dài hạn của doanh nghiệp.

> [!TIP]
> Biên an toàn cho phép bạn chấp nhận những sai số không thể tránh khỏi trong các giả định tương lai mà không phải trả giá bằng việc mất vốn vĩnh viễn.";
            investLessonsToAdd.Add(new Lesson
            {
                Id = InvestingLesson31Id,
                ChapterId = InvestingChapter3Id,
                Title = title,
                Slug = "bien-an-toan-va-dinh-gia-doanh-nghiep",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 30,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Biên an toàn và định giá doanh nghiệp cơ bản.", "Phân tích P/E, ROE và dòng tiền tự do."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == InvestingLesson32Id))
        {
            var title = "Bài 3.2: Tâm Lý Học Hành Vi & Quản Trị Cảm Xúc Trong Đầu Tư";
            var md = @"# Bài 3.2: Tâm Lý Học Hành Vi & Quản Trị Cảm Xúc Trong Đầu Tư

## 1. Mục tiêu bài học (Learning Objectives)
- Nhận diện 5 thiên kiến hành vi kinh điển: FOMO, Loss Aversion, Anchoring, Sunk Cost, Herd Mentality.
- Rèn luyện tư duy ngược dòng (Contrarian Thinking) dựa trên dữ liệu thay vì tâm lý bầy đàn.
- Xây dựng bộ quy tắc checklist trước khi bấm lệnh giao dịch.

> [!IMPORTANT]
> Kẻ thù lớn nhất của nhà đầu tư thường không phải là thị trường, mà chính là **cảm xúc và thiên kiến tâm lý của chính họ**.";
            investLessonsToAdd.Add(new Lesson
            {
                Id = InvestingLesson32Id,
                ChapterId = InvestingChapter3Id,
                Title = title,
                Slug = "tam-ly-hoc-hanh-vi-va-quan-tri-cam-xuc",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Tâm lý học hành vi và khắc phục thiên kiến.", "Tư duy ngược dòng và checklist giao dịch."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (investLessonsToAdd.Count > 0)
        {
            db.Lessons.AddRange(investLessonsToAdd);
            await db.SaveChangesAsync();
        }

        // =========================================================================
        // 2. KHÓA HỌC 2: LẬP TRÌNH JAVASCRIPT & TƯ DUY GIẢI QUYẾT VẤN ĐỀ
        // =========================================================================
        var codingCourse = await db.Courses.FirstOrDefaultAsync(c => c.Id == CodingCourseId || c.Slug == "lap-trinh-javascript-tu-duy-giai-quyet-van-de");
        if (codingCourse == null)
        {
            codingCourse = new Course
            {
                Id = CodingCourseId,
                InstructorId = DefaultUserId,
                Title = "Lập trình JavaScript & Tư duy Giải quyết Vấn đề",
                Slug = "lap-trinh-javascript-tu-duy-giai-quyet-van-de",
                Description = "Rèn luyện tư duy thuật toán, cấu trúc dữ liệu, xử lý bất đồng bộ và viết code sạch qua Web Worker sandbox thực chiến.",
                Visibility = "public",
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Courses.Add(codingCourse);

            if (!await db.CourseEnrollments.AnyAsync(e => e.UserId == DefaultUserId && e.CourseId == CodingCourseId))
            {
                db.CourseEnrollments.Add(new CourseEnrollment
                {
                    Id = Guid.NewGuid(),
                    UserId = DefaultUserId,
                    CourseId = CodingCourseId,
                    Role = "student",
                    EnrolledAt = now
                });
            }
        }

        var codingChapters = new List<Chapter>();
        if (!await db.Chapters.AnyAsync(ch => ch.Id == CodingChapter1Id))
        {
            codingChapters.Add(new Chapter { Id = CodingChapter1Id, CourseId = CodingCourseId, Title = "Chương 1: Xử Lý Dữ Liệu Mảng & Tư Duy Thuật Toán", SortOrder = 1, CreatedAt = now, UpdatedAt = now });
        }
        if (!await db.Chapters.AnyAsync(ch => ch.Id == CodingChapter2Id))
        {
            codingChapters.Add(new Chapter { Id = CodingChapter2Id, CourseId = CodingCourseId, Title = "Chương 2: Lập Trình Hàm, Closure & Tối Ưu Hiệu Năng", SortOrder = 2, CreatedAt = now, UpdatedAt = now });
        }
        if (!await db.Chapters.AnyAsync(ch => ch.Id == CodingChapter3Id))
        {
            codingChapters.Add(new Chapter { Id = CodingChapter3Id, CourseId = CodingCourseId, Title = "Chương 3: Bất Đồng Bộ, Pipeline & Xử Lý Lỗi Toàn Diện", SortOrder = 3, CreatedAt = now, UpdatedAt = now });
        }
        if (codingChapters.Count > 0)
        {
            db.Chapters.AddRange(codingChapters);
            await db.SaveChangesAsync();
        }

        var codingLessonsToAdd = new List<Lesson>();
        if (!await db.Lessons.AnyAsync(l => l.Id == CodingLesson11Id))
        {
            var title = "Bài 1.1: Biến Đổi & Lọc Dữ Liệu Nâng Cao (Map, Filter, Reduce)";
            var md = @"# Bài 1.1: Biến Đổi & Lọc Dữ Liệu Nâng Cao (Map, Filter, Reduce)

## 1. Mục tiêu bài học (Learning Objectives)
- Sử dụng thành thạo `filter`, `map`, `reduce` để xử lý mảng dữ liệu mà không làm biến đổi mảng gốc (immutability).
- Thực thi code trực tiếp trong Web Worker Sandbox biệt lập, bảo vệ UI thread khỏi đóng băng.
- Lắp ráp logic thuật toán qua các câu đố Parsons Problem.

> [!NOTE]
> Phương thức `reduce` là công cụ mạnh mẽ nhất trong mảng JavaScript, cho phép bạn biến đổi một mảng thành bất kỳ kiểu dữ liệu nào: số, chuỗi, đối tượng gom nhóm hoặc mảng phẳng.

## 2. Ví dụ thực tế: Gom nhóm và tính tổng
```javascript
const transactions = [
  { id: 1, type: 'income', amount: 1200000 },
  { id: 2, type: 'expense', amount: 350000 },
  { id: 3, type: 'income', amount: 800000 },
];

const totalIncome = transactions
  .filter(t => t.type === 'income')
  .reduce((sum, t) => sum + t.amount, 0);
// totalIncome = 2000000
```";
            codingLessonsToAdd.Add(new Lesson
            {
                Id = CodingLesson11Id,
                ChapterId = CodingChapter1Id,
                Title = title,
                Slug = "bien-doi-va-loc-du-lieu-mang",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 20,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Biến đổi và lọc mảng với map, filter, reduce.", "Lập trình hàm và tính bất biến dữ liệu."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == CodingLesson12Id))
        {
            var title = "Bài 1.2: Thuật Toán Tìm Kiếm & Kỹ Thuật Hai Con Trỏ (Two Pointers)";
            var md = @"# Bài 1.2: Thuật Toán Tìm Kiếm & Kỹ Thuật Hai Con Trỏ (Two Pointers)

## 1. Mục tiêu bài học (Learning Objectives)
- Hiểu và cài đặt thuật toán Tìm kiếm nhị phân (Binary Search) với độ phức tạp thời gian O(log n).
- Nắm vững kỹ thuật hai con trỏ (Two Pointers) trên mảng đã sắp xếp.
- Giải quyết bài toán Two-Sum và đảo ngược mảng in-place.

> [!TIP]
> Binary Search chỉ hoạt động trên mảng **đã được sắp xếp**. Hãy cẩn thận với phép tính chỉ số giữa `Math.floor((left + right) / 2)` để tránh tràn số nguyên.";
            codingLessonsToAdd.Add(new Lesson
            {
                Id = CodingLesson12Id,
                ChapterId = CodingChapter1Id,
                Title = title,
                Slug = "thuat-toan-tim-kiem-va-two-pointers",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Thuật toán tìm kiếm nhị phân và kỹ thuật two pointers.", "Độ phức tạp O(log n) và tối ưu bộ nhớ."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == CodingLesson21Id))
        {
            var title = "Bài 2.1: Kỹ Thuật Debounce & Throttle Xử Lý Sự Kiện Tần Suất Cao";
            var md = @"# Bài 2.1: Kỹ Thuật Debounce & Throttle Xử Lý Sự Kiện Tần Suất Cao

## 1. Mục tiêu bài học (Learning Objectives)
- Phân biệt cơ chế hoạt động của Debounce vs Throttle trong ứng dụng web.
- Tự viết hàm debounce với closure và bảo vệ `clearTimeout`.
- Tìm và khắc phục các bug rò rỉ timer phổ biến trong môi trường production.

> [!IMPORTANT]
> **Debounce:** Chờ người dùng ngừng phát sự kiện trong một khoảng thời gian delay mới thực thi (ví dụ: ô tìm kiếm autocomplete).
> **Throttle:** Đảm bảo hàm chỉ được gọi tối đa 1 lần trong mỗi chu kỳ thời gian (ví dụ: sự kiện cuộn trang `scroll`, resize cửa sổ).";
            codingLessonsToAdd.Add(new Lesson
            {
                Id = CodingLesson21Id,
                ChapterId = CodingChapter2Id,
                Title = title,
                Slug = "debounce-va-throttle-su-kien",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Kỹ thuật debounce và throttle trong JavaScript.", "Closure, timer và tối ưu hiệu năng UI."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == CodingLesson22Id))
        {
            var title = "Bài 2.2: Memoization & Cache Kết Quả Hàm Tính Toán Phức Tạp";
            var md = @"# Bài 2.2: Memoization & Cache Kết Quả Hàm Tính Toán Phức Tạp

## 1. Mục tiêu bài học (Learning Objectives)
- Hiểu nguyên lý Memoization: lưu trữ kết quả tính toán đắt đỏ của các hàm thuần khiết (pure functions).
- Viết hàm `memoize` hỗ trợ đa tham số và serialization key an toàn.
- Phát hiện các bug liên quan đến việc tham chiếu object trong cache map.";
            codingLessonsToAdd.Add(new Lesson
            {
                Id = CodingLesson22Id,
                ChapterId = CodingChapter2Id,
                Title = title,
                Slug = "memoization-va-cache-ket-qua-ham",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Kỹ thuật memoization và cache kết quả tính toán.", "Hàm thuần khiết và quản lý bộ nhớ đệm."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == CodingLesson31Id))
        {
            var title = "Bài 3.1: Async/Await, Promise Pipeline & Retry Cơ Chế Tự Phục Hồi";
            var md = @"# Bài 3.1: Async/Await, Promise Pipeline & Retry Cơ Chế Tự Phục Hồi

## 1. Mục tiêu bài học (Learning Objectives)
- Làm chủ vòng lặp sự kiện (Event Loop), microtasks và macrotasks.
- Viết hàm Retry với Exponential Backoff tự động gọi lại API khi lỗi mạng.
- Xây dựng pipeline xử lý tuần tự một danh sách Promise.";
            codingLessonsToAdd.Add(new Lesson
            {
                Id = CodingLesson31Id,
                ChapterId = CodingChapter3Id,
                Title = title,
                Slug = "async-await-promise-pipeline-retry",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 30,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Xử lý bất đồng bộ, event loop và promise pipeline.", "Retry tự phục hồi với exponential backoff."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == CodingLesson32Id))
        {
            var title = "Bài 3.2: Deep Clone, Tính Bất Biến & Xử Lý Lỗi Toàn Diện";
            var md = @"# Bài 3.2: Deep Clone, Tính Bất Biến & Xử Lý Lỗi Toàn Diện

## 1. Mục tiêu bài học (Learning Objectives)
- Phân biệt Shallow Copy vs Deep Clone và nguy cơ đột biến dữ liệu lồng nhau.
- Tự viết hàm Deep Clone hỗ trợ Date, Array, Object và vòng lặp tham chiếu.
- Sử dụng `structuredClone` chuẩn hiện đại và xử lý các ngoại lệ biên.";
            codingLessonsToAdd.Add(new Lesson
            {
                Id = CodingLesson32Id,
                ChapterId = CodingChapter3Id,
                Title = title,
                Slug = "deep-clone-immutability-va-xu-ly-loi",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Deep clone, tính bất biến và xử lý lỗi sâu.", "Tránh đột biến trạng thái ngoài ý muốn."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (codingLessonsToAdd.Count > 0)
        {
            db.Lessons.AddRange(codingLessonsToAdd);
            await db.SaveChangesAsync();
        }

        // =========================================================================
        // 3. KHÓA HỌC 3: TÂM LÝ HỌC ỨNG DỤNG & KHOA HỌC TRÍ NHỚ
        // =========================================================================
        var psychCourse = await db.Courses.FirstOrDefaultAsync(c => c.Id == PsychologyCourseId || c.Slug == "tam-ly-hoc-ung-dung-khoa-hoc-tri-nho");
        if (psychCourse == null)
        {
            psychCourse = new Course
            {
                Id = PsychologyCourseId,
                InstructorId = DefaultUserId,
                Title = "Tâm lý học Ứng dụng & Khoa học Trí nhớ",
                Slug = "tam-ly-hoc-ung-dung-khoa-hoc-tri-nho",
                Description = "Khám phá cơ chế nhận thức, khoa học trí nhớ FSRS, kiểm soát cảm xúc qua mô hình CBT và giao tiếp phi bạo lực NVC.",
                Visibility = "public",
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Courses.Add(psychCourse);

            if (!await db.CourseEnrollments.AnyAsync(e => e.UserId == DefaultUserId && e.CourseId == PsychologyCourseId))
            {
                db.CourseEnrollments.Add(new CourseEnrollment
                {
                    Id = Guid.NewGuid(),
                    UserId = DefaultUserId,
                    CourseId = PsychologyCourseId,
                    Role = "student",
                    EnrolledAt = now
                });
            }
        }

        var psychChapters = new List<Chapter>();
        if (!await db.Chapters.AnyAsync(ch => ch.Id == PsychologyChapter1Id))
        {
            psychChapters.Add(new Chapter { Id = PsychologyChapter1Id, CourseId = PsychologyCourseId, Title = "Chương 1: Nhận Thức, Chú Ý & Can Thiệp Não Bộ", SortOrder = 1, CreatedAt = now, UpdatedAt = now });
        }
        if (!await db.Chapters.AnyAsync(ch => ch.Id == PsychologyChapter2Id))
        {
            psychChapters.Add(new Chapter { Id = PsychologyChapter2Id, CourseId = PsychologyCourseId, Title = "Chương 2: Khoa Học Trí Nhớ, Chunking & Lặp Lại Ngắt Quãng", SortOrder = 2, CreatedAt = now, UpdatedAt = now });
        }
        if (!await db.Chapters.AnyAsync(ch => ch.Id == PsychologyChapter3Id))
        {
            psychChapters.Add(new Chapter { Id = PsychologyChapter3Id, CourseId = PsychologyCourseId, Title = "Chương 3: Liệu Pháp CBT & Trí Tuệ Cảm Xúc Ứng Dụng", SortOrder = 3, CreatedAt = now, UpdatedAt = now });
        }
        if (psychChapters.Count > 0)
        {
            db.Chapters.AddRange(psychChapters);
            await db.SaveChangesAsync();
        }

        var psychLessonsToAdd = new List<Lesson>();
        if (!await db.Lessons.AnyAsync(l => l.Id == PsychologyLesson11Id))
        {
            var title = "Bài 1.1: Xử Lý Thông Tin Tự Động vs Có Chủ Đích (Hệ Thống 1 & 2)";
            var md = @"# Bài 1.1: Xử Lý Thông Tin Tự Động vs Có Chủ Đích (Hệ Thống 1 & 2)

## 1. Mục tiêu bài học (Learning Objectives)
- Hiểu mô hình hai hệ thống tư duy của Daniel Kahneman (Hệ thống 1: Nhanh, tự động, cảm xúc; Hệ thống 2: Chậm, nỗ lực, logic).
- Trải nghiệm trực tiếp hiệu ứng Stroop thông qua mini lab đo lường thời gian phản xạ thật.
- Áp dụng nguyên lý Giao tiếp phi bạo lực (NVC) để tách bạch sự kiện khách quan khỏi phán xét chủ quan.

> [!NOTE]
> Hiệu ứng Stroop chứng minh rằng **Hệ thống 1 tự động đọc chữ** nhanh hơn việc nhận diện màu mực, gây ra độ trễ thời gian phản xạ (Interference Effect) khi chữ và màu xung đột.";
            psychLessonsToAdd.Add(new Lesson
            {
                Id = PsychologyLesson11Id,
                ChapterId = PsychologyChapter1Id,
                Title = title,
                Slug = "he-thong-1-va-2-nhan-thuc",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 20,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Hệ thống 1 và 2 trong nhận thức não bộ.", "Hiệu ứng Stroop và giao tiếp phi bạo lực NVC."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == PsychologyLesson12Id))
        {
            var title = "Bài 1.2: Ảo Tưởng Trí Nhớ & Thiên Kiến Xác Nhận (Confirmation Bias)";
            var md = @"# Bài 1.2: Ảo Tưởng Trí Nhớ & Thiên Kiến Xác Nhận (Confirmation Bias)

## 1. Mục tiêu bài học (Learning Objectives)
- Cơ chế não bộ tự lấp đầy lỗ hổng ký ức bằng các giả định và định kiến có sẵn.
- Nhận diện Thiên kiến xác nhận (Confirmation Bias) trong giao tiếp, học tập và tranh biện.
- Thực hành ghi nhật ký phản tư nhận diện các phán xét tự động trong ngày.";
            psychLessonsToAdd.Add(new Lesson
            {
                Id = PsychologyLesson12Id,
                ChapterId = PsychologyChapter1Id,
                Title = title,
                Slug = "ao-tuong-tri-nho-va-thien-kien-xac-nhan",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 20,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Ảo tưởng trí nhớ và thiên kiến xác nhận.", "Nhật ký phản tư và tư duy phản biện."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == PsychologyLesson21Id))
        {
            var title = "Bài 2.1: Dung Lượng Trí Nhớ Ngắn Hạn & Sức Mạnh Kỹ Thuật Chunking";
            var md = @"# Bài 2.1: Dung Lượng Trí Nhớ Ngắn Hạn & Sức Mạnh Kỹ Thuật Chunking

## 1. Mục tiêu bài học (Learning Objectives)
- Quy luật kỳ diệu của George Miller: Dung lượng trí nhớ ngắn hạn giới hạn trong 7 ± 2 đơn vị (chunks).
- Thực nghiệm đo Memory Span cá nhân và kiểm chứng sự mở rộng khi áp dụng Chunking.
- Tích hợp nguyên lý Chunking vào Flashcard FSRS v5 để tối ưu hóa việc học sâu.";
            psychLessonsToAdd.Add(new Lesson
            {
                Id = PsychologyLesson21Id,
                ChapterId = PsychologyChapter2Id,
                Title = title,
                Slug = "dung-luong-tri-nho-va-ky-thuat-chunking",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Dung lượng trí nhớ ngắn hạn và kỹ thuật chunking.", "Quy luật 7 ± 2 và tối ưu hóa Flashcard FSRS."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == PsychologyLesson22Id))
        {
            var title = "Bài 2.2: Đường Cong Quên Lãng Ebbinghaus & Tái Hiện Chủ Động";
            var md = @"# Bài 2.2: Đường Cong Quên Lãng Ebbinghaus & Tái Hiện Chủ Động

## 1. Mục tiêu bài học (Learning Objectives)
- Hiểu đường cong quên lãng Ebbinghaus: hơn 50% kiến thức bị mất trong 24 giờ nếu không ôn tập.
- Spaced Repetition (Lặp lại ngắt quãng) vs Cramming (Học dồn): Tại sao lặp lại ngắt quãng tạo ra trí nhớ vĩnh viễn.
- Sức mạnh của Retrieval Practice: Tự kiểm tra hiệu quả gấp 3 lần việc đọc lại thụ động.";
            psychLessonsToAdd.Add(new Lesson
            {
                Id = PsychologyLesson22Id,
                ChapterId = PsychologyChapter2Id,
                Title = title,
                Slug = "duong-cong-ebbinghaus-va-tai-hien-chu-dong",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Đường cong quên lãng Ebbinghaus và retrieval practice.", "Lặp lại ngắt quãng và củng cố synap thần kinh."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == PsychologyLesson31Id))
        {
            var title = "Bài 3.1: Mô Hình ABC Nhận Thức & Thách Thức Suy Nghĩ Tự Động";
            var md = @"# Bài 3.1: Mô Hình ABC Nhận Thức & Thách Thức Suy Nghĩ Tự Động

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững mô hình ABC của Albert Ellis / Aaron Beck trong Liệu pháp Nhận thức Hành vi (CBT):
  - **A (Activating Event):** Sự kiện khách quan.
  - **B (Belief):** Niềm tin và cách diễn giải sự kiện.
  - **C (Consequence):** Cảm xúc và hành vi hệ quả.
- Viết nhật ký CBT để tái cấu trúc nhận thức và chuyển đổi suy nghĩ tiêu cực.";
            psychLessonsToAdd.Add(new Lesson
            {
                Id = PsychologyLesson31Id,
                ChapterId = PsychologyChapter3Id,
                Title = title,
                Slug = "mo-hinh-cbt-abc-va-tai-cau-truc-nhan-thuc",
                SortOrder = 1,
                CurrentVersion = 1,
                EstimatedMinutes = 30,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Mô hình CBT ABC và tái cấu trúc nhận thức.", "Thách thức suy nghĩ tiêu cực tự động."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!await db.Lessons.AnyAsync(l => l.Id == PsychologyLesson32Id))
        {
            var title = "Bài 3.2: Thiết Lập Ranh Giới Cá Nhân & Tự Trắc Ẩn (Self-Compassion)";
            var md = @"# Bài 3.2: Thiết Lập Ranh Giới Cá Nhân & Tự Trắc Ẩn (Self-Compassion)

## 1. Mục tiêu bài học (Learning Objectives)
- Xây dựng ranh giới lành mạnh (Healthy Boundaries) trong công việc và cuộc sống.
- Kỹ năng nói lời từ chối khéo léo, tự tin mà không cảm thấy tội lỗi.
- Thực hành tự trắc ẩn (Self-Compassion) qua 3 yếu tố của Kristin Neff: Chánh niệm, Nhân tính chung, Sự tử tế với bản thân.";
            psychLessonsToAdd.Add(new Lesson
            {
                Id = PsychologyLesson32Id,
                ChapterId = PsychologyChapter3Id,
                Title = title,
                Slug = "thiet-lap-ranh-gioi-va-tu-trac-an",
                SortOrder = 2,
                CurrentVersion = 1,
                EstimatedMinutes = 25,
                ContentMarkdown = md,
                PlainText = md,
                ContentAstJson = BuildLessonAst(title, "Thiết lập ranh giới lành mạnh và tự trắc ẩn.", "Giao tiếp quyết đoán và xoa dịu cảm xúc."),
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (psychLessonsToAdd.Count > 0)
        {
            db.Lessons.AddRange(psychLessonsToAdd);
            await db.SaveChangesAsync();
        }

        // =========================================================================
        // 4. SEED PRACTICE ACTIVITIES FOR ALL 3 EXPANSION COURSES
        // =========================================================================
        var activitiesToAdd = new List<PracticeActivity>();

        // Check if activities already exist for Investing
        if (!await db.PracticeActivities.AnyAsync(pa => pa.CourseId == InvestingCourseId))
        {
            // Investing 1.1
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson11Id,
                OrderIndex = 1,
                Type = "compound_calculator",
                Title = "Máy tính Lãi kép & DCA tương tác: Tự do tài chính",
                Description = "Nhập vốn ban đầu, đóng góp hàng tháng (DCA), lãi suất kỳ vọng và lạm phát để kiểm tra sức mạnh lãi kép qua thời gian.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    initialPrincipal = 50000000,
                    monthlyDca = 5000000,
                    expectedReturn = 12.0,
                    inflationRate = 4.0,
                    targetYears = 15,
                    freedomTarget = 2000000000L
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson11Id,
                OrderIndex = 2,
                Type = "behavioral_quiz",
                Title = "Thiên kiến Hành vi: FOMO & Tâm lý Đám đông",
                Description = "Phân tích và khắc phục tâm lý sợ bỏ lỡ cơ hội khi thị trường tăng nóng.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    quizKey = "fomo-herd",
                    biasName = "FOMO & Tâm lý bầy đàn",
                    question = "Khi thấy một mã cổ phiếu tăng trần 5 phiên liên tiếp và các diễn đàn đều hưng phấn, phản ứng kỷ luật nhất là gì?",
                    options = new[]
                    {
                        "Mua đuổi ngay lập tức với đòn bẩy tối đa",
                        "Bỏ qua định giá vì sóng thị trường đang quá mạnh",
                        "Kiểm tra biên an toàn và kế hoạch phân bổ vốn trước khi ra quyết định",
                        "Rút hết tiền tiết kiệm mua gom một lần"
                    },
                    correctIndex = 2,
                    explanation = "FOMO khiến nhà đầu tư mua ở đỉnh vì cảm xúc hưng phấn thay vì định giá khách quan. Kỷ luật phân bổ vốn là chiếc khiên bảo vệ tài sản."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Investing 1.2
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson12Id,
                OrderIndex = 1,
                Type = "compound_calculator",
                Title = "Mục tiêu Độc lập Tài chính FIRE & Quy tắc 4%",
                Description = "Tính toán số vốn cần thiết để sống dựa trên 4% rút ra hàng năm mà không suy giảm vốn gốc.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    annualExpense = 180000000L,
                    safeWithdrawalRate = 4.0,
                    targetFund = 4500000000L,
                    monthlyDca = 10000000L,
                    expectedReturn = 10.0,
                    inflationRate = 3.5,
                    targetYears = 15
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson12Id,
                OrderIndex = 2,
                Type = "investment_case",
                Title = "Tình huống: Tối ưu hóa dòng tiền thặng dư hàng tháng",
                Description = "Lựa chọn phương án tối ưu giữa trả bớt nợ lãi cao hay dồn tiền vào cổ phiếu biến động.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    scenario = "Bạn có khoản nợ thẻ tín dụng lãi suất 28%/năm và 20 triệu tiền nhàn rỗi. Một người bạn khuyên nên mua cổ phiếu lướt sóng.",
                    options = new[]
                    {
                        "Dồn hết 20 triệu mua cổ phiếu với hy vọng lãi 50% trả nợ sau",
                        "Ưu tiên thanh toán dứt điểm nợ thẻ tín dụng lãi suất cao trước",
                        "Chia đôi: 10 triệu mua cổ phiếu, 10 triệu để đi du lịch xả stress",
                        "Vay thêm từ thẻ khác để đầu tư kép"
                    },
                    correctIndex = 1,
                    explanation = "Lãi suất 28%/năm của nợ thẻ tín dụng là khoản 'lỗ chắc chắn'. Trả nợ tương đương với việc đạt tỷ suất sinh lời phi rủi ro 28%."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Investing 2.1
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson21Id,
                OrderIndex = 1,
                Type = "portfolio_sim",
                Title = "Mô phỏng Phân bổ Danh mục Đa tài sản (Dữ liệu MẪU)",
                Description = "Mô phỏng tỷ trọng 5 lớp tài sản (Cổ phiếu tăng trưởng, Cổ tức, Trái phiếu, Vàng, Tiền mặt) và kiểm tra hiệu suất qua các chu kỳ.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    isSampleData = true,
                    sampleDataNotice = "Dữ liệu mẫu lịch sử mô phỏng phục vụ thực hành — không phải khuyến nghị đầu tư",
                    defaultAllocations = new
                    {
                        growthStocks = 40,
                        dividendStocks = 25,
                        bonds = 20,
                        gold = 10,
                        cash = 5
                    },
                    scenarios = new[] { "bull", "inflation", "recession" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson21Id,
                OrderIndex = 2,
                Type = "investment_case",
                Title = "Tình huống: Tái cân bằng danh mục khi cổ phiếu tăng vượt tỷ trọng",
                Description = "Khi nhóm cổ phiếu tăng vọt chiếm 75% danh mục (mục tiêu ban đầu 50%), quyết định đúng đắn là gì?",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    scenario = "Cổ phiếu tăng mạnh khiến tỷ trọng tăng từ 50% lên 75% danh mục. Thị trường đang hưng phấn tột độ.",
                    options = new[]
                    {
                        "Giữ nguyên và mua thêm để tối đa hóa lợi nhuận",
                        "Chốt lời một phần cổ phiếu, chuyển sang trái phiếu/tiền mặt để đưa về tỷ trọng mục tiêu 50%",
                        "Bán sạch toàn bộ danh mục chuyển sang gửi tiết kiệm",
                        "Vay margin để gồng lãi tiếp"
                    },
                    correctIndex = 1,
                    explanation = "Tái cân bằng giúp hiện thực hóa lợi nhuận khi tài sản tăng nóng và duy trì hồ sơ rủi ro ban đầu của bạn."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Investing 2.2
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson22Id,
                OrderIndex = 1,
                Type = "portfolio_sim",
                Title = "Stress Testing: Kiểm thử Max Drawdown trong Khủng hoảng (Dữ liệu MẪU)",
                Description = "Kiểm tra danh mục chịu đựng ra sao khi thị trường sụt giảm 35% trong kịch bản suy thoái.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    isSampleData = true,
                    sampleDataNotice = "Dữ liệu mẫu lịch sử mô phỏng phục vụ thực hành — không phải khuyến nghị đầu tư",
                    stressScenario = "recession",
                    marketDrop = -35.0,
                    targetMaxDrawdown = -20.0
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson22Id,
                OrderIndex = 2,
                Type = "behavioral_quiz",
                Title = "Thiên kiến Sợ thua lỗ (Loss Aversion) & Kỷ luật Cắt lỗ",
                Description = "Tâm lý sợ thua lỗ khiến nhà đầu tư chối bỏ thực tế và gồng lỗ phi lý trí.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    quizKey = "loss-aversion",
                    biasName = "Loss Aversion (Sợ thua lỗ)",
                    question = "Một khoản đầu tư đã chạm ngưỡng cắt lỗ 8% nhưng bạn không bán vì 'chưa bán là chưa lỗ'. Đây là biểu hiện của:",
                    options = new[]
                    {
                        "Chiến lược đầu tư giá trị kiên định",
                        "Thiên kiến sợ thua lỗ (Loss Aversion) và gồng lỗ phi lý trí",
                        "Tư duy độc lập tài chính",
                        "Tối ưu hóa thuế thu nhập"
                    },
                    correctIndex = 1,
                    explanation = "Nỗi đau mất tiền lớn gấp 2-2.5 lần niềm vui kiếm được cùng số tiền, khiến nhà đầu tư ôm giữ tài sản xấu thay vì cắt lỗ bảo toàn vốn."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Investing 3.1
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson31Id,
                OrderIndex = 1,
                Type = "investment_case",
                Title = "Tình huống: Đánh giá Biên an toàn khi thị trường điều chỉnh 25%",
                Description = "Định giá một công ty đầu ngành có P/E giảm về mức hấp dẫn nhưng tin tức vĩ mô ảm đạm.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    scenario = "Một doanh nghiệp bán lẻ đầu ngành có tăng trưởng lợi nhuận 15%/năm, không nợ vay, nhưng giá cổ phiếu giảm 25% theo thị trường chung khiến P/E giảm từ 18x về 10x.",
                    options = new[]
                    {
                        "Bán tháo theo thị trường vì sợ giá còn giảm nữa",
                        "Phân tích lại dòng tiền tự do, xác nhận biên an toàn và bắt đầu giải ngân từng phần (DCA)",
                        "Vay ký quỹ tối đa all-in một lệnh duy nhất",
                        "Bỏ qua không theo dõi nữa"
                    },
                    correctIndex = 1,
                    explanation = "Biên an toàn được tạo ra khi giá thị trường chiết khấu sâu so với giá trị nội tại của doanh nghiệp chất lượng cao."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson31Id,
                OrderIndex = 2,
                Type = "behavioral_quiz",
                Title = "Thiên kiến Mỏ neo (Anchoring) & Chi phí Chìm (Sunk Cost)",
                Description = "Tránh đưa ra quyết định dựa trên mức giá mua quá khứ.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    quizKey = "anchoring-sunk-cost",
                    biasName = "Anchoring & Sunk Cost Fallacy",
                    question = "Bạn mua cổ phiếu giá 100.000đ, hiện giảm về 60.000đ do doanh nghiệp mất khách hàng lớn. Bạn quyết tâm đợi hòa vốn 100.000đ mới bán. Đây là sai lầm gì?",
                    options = new[]
                    {
                        "Kỷ luật đầu tư dài hạn mẫu mực",
                        "Bị neo vào giá mua cũ (Anchoring) và tiếc chi phí chìm (Sunk Cost)",
                        "Quản trị danh mục theo trường phái hiện đại",
                        "Phân bổ tài sản chiến lược"
                    },
                    correctIndex = 1,
                    explanation = "Thị trường không hề biết bạn mua giá nào. Quyết định bán hay giữ phải phụ thuộc vào triển vọng tương lai chứ không phải giá vốn quá khứ."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Investing 3.2
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson32Id,
                OrderIndex = 1,
                Type = "investment_case",
                Title = "Tình huống: Ra quyết định khi thị trường hoảng loạn tột độ",
                Description = "Chỉ số nỗi sợ chạm đỉnh, truyền thông dự báo suy thoái kéo dài. Bạn xử lý ra sao?",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    scenario = "Thị trường giảm liên tiếp 4 tuần, nhà đầu tư bán tháo ồ ạt. Bạn còn 35% tiền mặt và danh mục là các cổ phiếu phòng thủ trả cổ tức đều.",
                    options = new[]
                    {
                        "Bán tháo theo đám đông để bớt căng thẳng",
                        "Giữ vững kế hoạch, nhận cổ tức tái đầu tư và tiếp tục giải ngân DCA theo các mốc định trước",
                        "Xóa app đầu tư và không bao giờ quay lại",
                        "Tham gia hội nhóm đánh bạc phái sinh để gỡ gạc"
                    },
                    correctIndex = 1,
                    explanation = "Lợi nhuận vượt trội trong đầu tư dài hạn được sinh ra từ việc giữ vững kỷ luật khi người khác hoảng loạn."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = InvestingCourseId,
                LessonId = InvestingLesson32Id,
                OrderIndex = 2,
                Type = "behavioral_quiz",
                Title = "Tư duy Ngược dòng (Contrarian) & Tự chủ Cảm xúc",
                Description = "Luyện tập tư duy độc lập khi đám đông tham lam hoặc sợ hãi.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    quizKey = "contrarian-mindset",
                    biasName = "Tư duy Ngược dòng & Tự chủ Cảm xúc",
                    question = "Yếu tố quyết định giúp nhà đầu tư ngược dòng thành công trong khủng hoảng là gì?",
                    options = new[]
                    {
                        "Khả năng đoán chính xác đáy thị trường",
                        "Nguồn vốn dài hạn, hiểu sâu giá trị tài sản và kiểm soát tâm lý cá nhân",
                        "Tin đồn nội bộ từ các diễn đàn kín",
                        "Lướt sóng mua bán hàng ngày"
                    },
                    correctIndex = 1,
                    explanation = "Vốn dài hạn nhàn rỗi cùng sự hiểu biết giá trị nội tại giúp nhà đầu tư bình thản vượt qua các đợt sụt giảm tạm thời."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        // Check if activities already exist for Coding
        if (!await db.PracticeActivities.AnyAsync(pa => pa.CourseId == CodingCourseId))
        {
            // Coding 1.1
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson11Id,
                OrderIndex = 1,
                Type = "js_sandbox",
                Title = "Thực hành JS Sandbox: Lọc & Tính Tổng Giao Dịch Hợp Lệ",
                Description = "Viết hàm `calculateNetIncome(transactions)` lọc các giao dịch có status 'completed' và tính tổng doanh thu ròng.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    functionName = "calculateNetIncome",
                    starterCode = @"function calculateNetIncome(transactions) {
  // transactions: [{ id, type: 'income' | 'expense', amount, status: 'completed' | 'pending' }]
  // Trả về tổng tiền ròng (income - expense) của các giao dịch 'completed'
  return transactions
    .filter(t => t.status === 'completed')
    .reduce((sum, t) => t.type === 'income' ? sum + t.amount : sum - t.amount, 0);
}",
                    testCases = new object[]
                    {
                        new { input = new object[] { new object[] { new { id = 1, type = "income", amount = 1000, status = "completed" }, new { id = 2, type = "expense", amount = 300, status = "completed" }, new { id = 3, type = "income", amount = 500, status = "pending" } } }, expected = 700, description = "Tính đúng tổng ròng bỏ qua giao dịch pending" },
                        new { input = new object[] { new object[] { new { id = 1, type = "expense", amount = 500, status = "completed" } } }, expected = -500, description = "Tính đúng khi chỉ có khoản chi" },
                        new { input = new object[] { Array.Empty<object>() }, expected = 0, description = "Trả về 0 khi mảng rỗng" }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson11Id,
                OrderIndex = 2,
                Type = "parsons",
                Title = "Parsons Problem: Lắp ráp thuật toán Two-Sum",
                Description = "Sắp xếp các dòng code giải thuật tìm cặp chỉ số có tổng bằng target.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    title = "Thuật toán Two-Sum O(n) với Map",
                    lines = new[]
                    {
                        new { id = "l1", text = "function twoSum(nums, target) {", correctOrder = 1, indent = 0 },
                        new { id = "l2", text = "  const map = new Map();", correctOrder = 2, indent = 1 },
                        new { id = "l3", text = "  for (let i = 0; i < nums.length; i++) {", correctOrder = 3, indent = 1 },
                        new { id = "l4", text = "    const complement = target - nums[i];", correctOrder = 4, indent = 2 },
                        new { id = "l5", text = "    if (map.has(complement)) {", correctOrder = 5, indent = 2 },
                        new { id = "l6", text = "      return [map.get(complement), i];", correctOrder = 6, indent = 3 },
                        new { id = "l7", text = "    }", correctOrder = 7, indent = 2 },
                        new { id = "l8", text = "    map.set(nums[i], i);", correctOrder = 8, indent = 2 },
                        new { id = "l9", text = "  }", correctOrder = 9, indent = 1 },
                        new { id = "l10", text = "  return [];", correctOrder = 10, indent = 1 },
                        new { id = "l11", text = "}", correctOrder = 11, indent = 0 }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Coding 1.2
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson12Id,
                OrderIndex = 1,
                Type = "js_sandbox",
                Title = "Thực hành JS Sandbox: Cài đặt Binary Search O(log n)",
                Description = "Viết hàm `binarySearch(arr, target)` tìm vị trí target trong mảng đã sắp xếp, trả về -1 nếu không tìm thấy.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    functionName = "binarySearch",
                    starterCode = @"function binarySearch(arr, target) {
  let left = 0;
  let right = arr.length - 1;
  while (left <= right) {
    const mid = Math.floor((left + right) / 2);
    if (arr[mid] === target) return mid;
    if (arr[mid] < target) left = mid + 1;
    else right = mid - 1;
  }
  return -1;
}",
                    testCases = new object[]
                    {
                        new { input = new object[] { new[] { 1, 3, 5, 7, 9, 11 }, 7 }, expected = 3, description = "Tìm thấy phần tử ở giữa mảng" },
                        new { input = new object[] { new[] { 2, 4, 6, 8, 10 }, 2 }, expected = 0, description = "Tìm thấy phần tử đầu mảng" },
                        new { input = new object[] { new[] { 10, 20, 30 }, 25 }, expected = -1, description = "Trả về -1 khi phần tử không có trong mảng" }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson12Id,
                OrderIndex = 2,
                Type = "parsons",
                Title = "Parsons Problem: Đảo ngược mảng bằng Two Pointers",
                Description = "Sắp xếp thuật toán hoán đổi hai đầu mảng không dùng hàm có sẵn.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    title = "Đảo ngược mảng in-place với Two Pointers",
                    lines = new[]
                    {
                        new { id = "p1", text = "function reverseArray(arr) {", correctOrder = 1, indent = 0 },
                        new { id = "p2", text = "  let left = 0, right = arr.length - 1;", correctOrder = 2, indent = 1 },
                        new { id = "p3", text = "  while (left < right) {", correctOrder = 3, indent = 1 },
                        new { id = "p4", text = "    const temp = arr[left];", correctOrder = 4, indent = 2 },
                        new { id = "p5", text = "    arr[left] = arr[right];", correctOrder = 5, indent = 2 },
                        new { id = "p6", text = "    arr[right] = temp;", correctOrder = 6, indent = 2 },
                        new { id = "p7", text = "    left++; right--;", correctOrder = 7, indent = 2 },
                        new { id = "p8", text = "  }", correctOrder = 8, indent = 1 },
                        new { id = "p9", text = "  return arr;", correctOrder = 9, indent = 1 },
                        new { id = "p10", text = "}", correctOrder = 10, indent = 0 }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Coding 2.1
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson21Id,
                OrderIndex = 1,
                Type = "js_sandbox",
                Title = "Thực hành JS Sandbox: Viết hàm Debounce với Timeout Bảo Vệ",
                Description = "Viết hàm `debounce(fn, delay)` chuẩn xác giúp trì hoãn thực thi.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    functionName = "debounce",
                    starterCode = @"function debounce(fn, delay) {
  let timerId = null;
  return function(...args) {
    if (timerId !== null) clearTimeout(timerId);
    timerId = setTimeout(() => {
      fn.apply(this, args);
    }, delay);
  };
}",
                    testCases = new object[]
                    {
                        new { input = new object[] { "test-function", 50 }, expected = "function", description = "Trả về hàm bọc debounce hợp lệ" }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson21Id,
                OrderIndex = 2,
                Type = "bug_hunt",
                Title = "Săn Bug: Sửa lỗi rò rỉ Timer trong Debounce",
                Description = "Tìm và chỉ ra dòng code quên gọi clearTimeout khiến sự kiện bị kích hoạt nhiều lần.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    bugKey = "debounce-missing-clear",
                    codeSnippet = @"function badDebounce(fn, delay) {
  let timer;
  return function(...args) {
    // BUG: Thiếu clearTimeout(timer);
    timer = setTimeout(() => fn(...args), delay);
  };
}",
                    bugLine = 5,
                    explanation = "Nếu không gọi clearTimeout(timer) trước khi đặt timer mới, mỗi lần gõ phím sẽ kích hoạt một bộ hẹn giờ riêng, phá hỏng mục đích gom nhóm của debounce."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Coding 2.2
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson22Id,
                OrderIndex = 1,
                Type = "js_sandbox",
                Title = "Thực hành JS Sandbox: Viết hàm Memoize Cho Phép Đa Tham Số",
                Description = "Viết hàm `memoize(fn)` lưu trữ kết quả dựa trên serialized JSON key.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    functionName = "memoize",
                    starterCode = @"function memoize(fn) {
  const cache = new Map();
  return function(...args) {
    const key = JSON.stringify(args);
    if (cache.has(key)) return cache.get(key);
    const result = fn.apply(this, args);
    cache.set(key, result);
    return result;
  };
}",
                    testCases = new object[]
                    {
                        new { input = new object[] { "memoize-test" }, expected = "function", description = "Tạo hàm memoized thành công" }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson22Id,
                OrderIndex = 2,
                Type = "bug_hunt",
                Title = "Săn Bug: Lỗi biến đổi key đối tượng trong Cache Map",
                Description = "Phát hiện lỗi ép kiểu `String(obj)` thành `[object Object]` làm trùng lặp key cache.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    bugKey = "memoize-object-key-collision",
                    codeSnippet = @"function brokenMemoize(fn) {
  const cache = {};
  return function(arg) {
    // BUG: arg là object sẽ biến thành '[object Object]' cho mọi object khác nhau
    const key = arg.toString();
    if (cache[key]) return cache[key];
    return cache[key] = fn(arg);
  };
}",
                    bugLine = 5,
                    explanation = "Dùng arg.toString() khiến mọi đối tượng khác nhau đều có key là '[object Object]', dẫn đến trả về kết quả sai của đối tượng trước đó."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Coding 3.1
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson31Id,
                OrderIndex = 1,
                Type = "js_sandbox",
                Title = "Thực hành JS Sandbox: Viết hàm Clamp & Giới Hạn Giá Trị",
                Description = "Viết hàm `clamp(val, min, max)` giữ giá trị trong khoảng hợp lệ.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    functionName = "clamp",
                    starterCode = @"function clamp(val, min, max) {
  if (val < min) return min;
  if (val > max) return max;
  return val;
}",
                    testCases = new object[]
                    {
                        new { input = new object[] { 15, 0, 10 }, expected = 10, description = "Giá trị vượt max được gán về max" },
                        new { input = new object[] { -5, 0, 100 }, expected = 0, description = "Giá trị dưới min được gán về min" },
                        new { input = new object[] { 42, 0, 100 }, expected = 42, description = "Giá trị nằm trong khoảng được giữ nguyên" }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson31Id,
                OrderIndex = 2,
                Type = "parsons",
                Title = "Parsons Problem: Pipeline thực thi Promise tuần tự",
                Description = "Sắp xếp logic thực thi danh sách async task theo chuỗi.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    title = "Thực thi tuần tự Promise Pipeline",
                    lines = new[]
                    {
                        new { id = "pl1", text = "async function runSequential(tasks) {", correctOrder = 1, indent = 0 },
                        new { id = "pl2", text = "  const results = [];", correctOrder = 2, indent = 1 },
                        new { id = "pl3", text = "  for (const task of tasks) {", correctOrder = 3, indent = 1 },
                        new { id = "pl4", text = "    const res = await task();", correctOrder = 4, indent = 2 },
                        new { id = "pl5", text = "    results.push(res);", correctOrder = 5, indent = 2 },
                        new { id = "pl6", text = "  }", correctOrder = 6, indent = 1 },
                        new { id = "pl7", text = "  return results;", correctOrder = 7, indent = 1 },
                        new { id = "pl8", text = "}", correctOrder = 8, indent = 0 }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Coding 3.2
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson32Id,
                OrderIndex = 1,
                Type = "js_sandbox",
                Title = "Thực hành JS Sandbox: Deep Clone An Toàn",
                Description = "Viết hàm `safeDeepClone(obj)` sao chép sâu cấu trúc mảng và object lồng nhau.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    functionName = "safeDeepClone",
                    starterCode = @"function safeDeepClone(obj) {
  if (obj === null || typeof obj !== 'object') return obj;
  if (Array.isArray(obj)) return obj.map(safeDeepClone);
  const copy = {};
  for (const key of Object.keys(obj)) {
    copy[key] = safeDeepClone(obj[key]);
  }
  return copy;
}",
                    testCases = new object[]
                    {
                        new { input = new object[] { new { a = 1, b = new { c = 2 } } }, expected = new { a = 1, b = new { c = 2 } }, description = "Sao chép thành công object lồng nhau" },
                        new { input = new object[] { new object[] { 1, new object[] { 2, 3 } } }, expected = new object[] { 1, new object[] { 2, 3 } }, description = "Sao chép thành công mảng lồng nhau" }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = CodingCourseId,
                LessonId = CodingLesson32Id,
                OrderIndex = 2,
                Type = "bug_hunt",
                Title = "Săn Bug: Shallow Copy gây đột biến mảng lồng nhau",
                Description = "Phát hiện lỗi dùng cú pháp spread `[...arr]` làm đột biến các phần tử con.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    bugKey = "shallow-copy-nested-mutation",
                    codeSnippet = @"function duplicateMatrix(matrix) {
  // BUG: Chỉ sao chép mảng ngoài, các mảng con vẫn giữ nguyên tham chiếu!
  const copy = [...matrix];
  copy[0][0] = 999; // Gây đột biến matrix gốc!
  return copy;
}",
                    bugLine = 3,
                    explanation = "Toán tử spread chỉ tạo nông (shallow copy). Các mảng con bên trong vẫn tham chiếu tới bộ nhớ gốc, dẫn đến lỗi side-effect khó truy vết."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        // Check if activities already exist for Psychology
        if (!await db.PracticeActivities.AnyAsync(pa => pa.CourseId == PsychologyCourseId))
        {
            // Psychology 1.1
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson11Id,
                OrderIndex = 1,
                Type = "stroop_test",
                Title = "Mini Lab Nhận thức: Thí nghiệm Hiệu ứng Stroop",
                Description = "Đo thời gian phản xạ (ms) khi xử lý từ ngữ trùng khớp (Congruent) vs xung đột (Incongruent) giữa chữ và màu mực.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    trialsCount = 10,
                    words = new[] { "ĐỎ", "XANH DƯƠNG", "XANH LÁ", "VÀNG" },
                    colors = new[] { "#ef4444", "#3b82f6", "#10b981", "#eab308" }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson11Id,
                OrderIndex = 2,
                Type = "psych_scenario",
                Title = "Tình huống NVC: Tách Quan Sát Khỏi Phán Xét Khi Đồng Đội Trễ Hẹn",
                Description = "Lựa chọn phản hồi theo 4 bước Giao tiếp Phi bạo lực (Observation - Feeling - Need - Request).",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    scenario = "Đồng nghiệp gửi trễ slide thuyết trình lần thứ hai trong tuần khiến bạn phải đợi đến nửa đêm.",
                    options = new[]
                    {
                        "Công kích: 'Sao lúc nào bạn cũng vô trách nhiệm và làm chậm tiến độ cả nhóm thế?'",
                        "NVC: 'Tôi thấy slide được gửi lúc 23h30 thay vì 17h như đã hẹn. Tôi thấy lo lắng vì 8h sáng mai chúng ta phải trình bày, và tôi cần sự phối hợp đúng hạn để duyệt lại bài. Lần tới bạn có thể gửi trước 17h được không?'",
                        "Thụ động né tránh: Không nói gì nhưng trong lòng ấm ức và nói xấu sau lưng",
                        "Phán xét: 'Người như bạn không bao giờ làm việc nhóm chuyên nghiệp được'"
                    },
                    correctIndex = 1,
                    explanation = "NVC bắt đầu bằng việc nêu sự kiện khách quan có thể kiểm chứng (gửi lúc 23h30), nói cảm xúc của bản thân (lo lắng), nhu cầu (đúng hạn) và đưa ra đề nghị cụ thể."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Psychology 1.2
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson12Id,
                OrderIndex = 1,
                Type = "psych_scenario",
                Title = "Tình huống: Vượt qua Thiên kiến Xác nhận trong tranh luận",
                Description = "Nhận biết xu hướng chỉ tìm kiếm thông tin củng cố quan điểm cá nhân.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    scenario = "Trong một cuộc họp thảo luận về phương án kiến trúc mới, bạn rất thích giải pháp A và có xu hướng bỏ qua mọi rủi ro mà đồng đội cảnh báo.",
                    options = new[]
                    {
                        "Tìm mọi lý lẽ để phản bác đồng đội và bảo vệ giải pháp A bằng mọi giá",
                        "Chủ động hỏi: 'Những bằng chứng hoặc kịch bản nào có thể chứng minh giải pháp A sẽ thất bại?'",
                        "Ủy thác cho sếp quyết định để tránh chịu trách nhiệm",
                        "Bỏ họp giữa chừng vì bất đồng quan điểm"
                    },
                    correctIndex = 1,
                    explanation = "Kỹ thuật phản biện tích cực (Falsification) giúp phá vỡ Thiên kiến xác nhận bằng cách chủ động tìm kiếm các bằng chứng bác bỏ giả định ban đầu."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson12Id,
                OrderIndex = 2,
                Type = "reflection_journal",
                Title = "Nhật ký Phản tư: Nhận diện 3 Phán xét Tự động trong ngày",
                Description = "Ghi lại 3 khoảnh khắc Hệ thống 1 phản ứng tự động và chuyển hóa sang góc nhìn thấu cảm.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    prompts = new[]
                    {
                        "Khoảnh khắc bạn vừa phán xét ai đó hoặc chính mình trong ngày hôm nay là gì?",
                        "Sự kiện khách quan thực tế xảy ra là gì (không chứa từ ngữ định kiến)?",
                        "Góc nhìn đa chiều hoặc giải thích thay thế nào có thể hợp lý hơn?"
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Psychology 2.1
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson21Id,
                OrderIndex = 1,
                Type = "memory_span",
                Title = "Mini Lab Nhận thức: Thí nghiệm Đo Memory Span & Chunking",
                Description = "Đo lường dung lượng trí nhớ ngắn hạn cá nhân (3 đến 8 đơn vị) và kiểm chứng hiệu quả khi nhóm cụm (Chunking).",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    minSpan = 3,
                    maxSpan = 8,
                    displayDurationMs = 2500
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson21Id,
                OrderIndex = 2,
                Type = "reflection_journal",
                Title = "Nhật ký: Lên Kế Hoạch Ứng Dụng FSRS v5 & Spaced Repetition",
                Description = "Thiết kế kế hoạch ôn tập ngắt quãng cho môn học khó nhất trong học kỳ.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    prompts = new[]
                    {
                        "Môn học hoặc chủ đề nào bạn thường xuyên quên nhanh nhất sau khi học?",
                        "Làm thế nào bạn có thể chia nhỏ kiến thức đó thành các đơn vị Chunking rõ ràng?",
                        "Mục tiêu duy trì ôn tập FSRS ít nhất 5 phút mỗi ngày tại Studxy của bạn là gì?"
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Psychology 2.2
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson22Id,
                OrderIndex = 1,
                Type = "memory_span",
                Title = "Thực nghiệm: Kiểm tra Khả năng Phục hồi Ký ức (Retrieval)",
                Description = "Thử thách nhớ lại chuỗi thông tin sau một khoảng thời gian gián đoạn ngắn.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    minSpan = 4,
                    maxSpan = 7,
                    distractionIntervalSeconds = 5
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson22Id,
                OrderIndex = 2,
                Type = "psych_scenario",
                Title = "Tình huống: Vượt qua Burnout khi học tập và làm việc dồn dập",
                Description = "Lựa chọn phương pháp quản lý năng lượng và khoảng nghỉ Pomodoro khoa học.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    scenario = "Bạn đã ngồi liên tục 5 tiếng trước màn hình, mắt mỏi nhừ và đọc một câu văn 3 lần vẫn không hiểu nghĩa.",
                    options = new[]
                    {
                        "Uống thêm lon nước tăng lực và ép bản thân ngồi tiếp 3 tiếng nữa",
                        "Đứng dậy rời màn hình, đi dạo 15 phút, uống nước và thực hiện một chu kỳ Pomodoro ngắn sau đó",
                        "Bỏ cuộc hoàn toàn và tự trách bản thân yếu kém",
                        "Lướt mạng xã hội trên điện thoại ngay tại bàn làm việc"
                    },
                    correctIndex = 1,
                    explanation = "Khoảng nghỉ tách biệt hoàn toàn khỏi kích thích kỹ thuật số giúp não bộ chuyển sang chế độ Default Mode Network để củng cố ký ức và phục hồi dopamine."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Psychology 3.1
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson31Id,
                OrderIndex = 1,
                Type = "reflection_journal",
                Title = "Nhật ký CBT: Tái Cấu Trúc Nhận Thức theo Mô Hình ABC",
                Description = "Thực hành ghi lại Sự kiện kích hoạt (A), Niềm tin tự động (B) và chuyển hóa Hệ quả (C).",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    model = "CBT_ABC",
                    prompts = new[]
                    {
                        "A - Activating Event (Sự kiện gì vừa xảy ra? Chỉ ghi sự thật khách quan):",
                        "B - Belief (Suy nghĩ tự động hiện lên trong đầu bạn là gì?):",
                        "C - Consequence (Cảm xúc và phản ứng cơ thể của bạn lúc đó ra sao?):",
                        "D - Dispute & Reframe (Bằng chứng nào phản bác suy nghĩ B? Góc nhìn cân bằng hơn là gì?):"
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson31Id,
                OrderIndex = 2,
                Type = "psych_scenario",
                Title = "Tình huống: Thiết lập Ranh Giới Cá Nhân và Nói Lời Từ Chối",
                Description = "Từ chối lời nhờ vả vượt quá khả năng một cách lịch thiệp, tôn trọng và dứt khoát.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    scenario = "Một người quen nhờ bạn làm miễn phí một website phức tạp vào cuối tuần khi bạn đang cần hoàn thành đồ án tốt nghiệp.",
                    options = new[]
                    {
                        "Miễn cưỡng đồng ý vì sợ họ giận, rồi thức trắng đêm làm trong bức bối",
                        "Từ chối dứt khoát và lịch thiệp: 'Cảm ơn bạn đã tin tưởng. Hiện tại mình đang tập trung toàn bộ thời gian cho đồ án tốt nghiệp nên không thể nhận hỗ trợ được. Chúc dự án của bạn thuận lợi!'",
                        "Im lặng không trả lời tin nhắn để họ tự hiểu",
                        "Chửi mắng người nhờ vì đã làm phiền"
                    },
                    correctIndex = 1,
                    explanation = "Nói 'Không' với điều vượt quá năng lượng của bạn chính là nói 'Có' với ưu tiên và sức khỏe tinh thần của chính bạn. Một lời từ chối rõ ràng luôn tốt hơn một sự đồng ý miễn cưỡng."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Psychology 3.2
            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson32Id,
                OrderIndex = 1,
                Type = "psych_scenario",
                Title = "Tình huống: Đối thoại Nội tâm Tự Trắc Ẩn khi Thất Bại",
                Description = "Thay thế tiếng nói tự chỉ trích (Inner Critic) bằng sự tử tế và thấu cảm với bản thân.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    scenario = "Bạn vừa nhận kết quả một bài kiểm tra quan trọng với điểm số không như ý.",
                    options = new[]
                    {
                        "Tự sỉ nhục: 'Mình thật ngu ngốc và vô dụng, sẽ chẳng bao giờ làm được trò trống gì'",
                        "Tự trắc ẩn: 'Điểm số này thật sự đáng thất vọng và mình cảm thấy buồn. Nhưng thất bại là một phần tất yếu của quá trình học hỏi của con người. Mình sẽ xem lại bài để biết phần nào cần cải thiện.'",
                        "Đổ lỗi hoàn toàn cho đề thi bất công và thầy cô chấm khó",
                        "Giả vờ như không quan tâm và không nhìn lại bài thi"
                    },
                    correctIndex = 1,
                    explanation = "Tự trắc ẩn kết hợp Chánh niệm (thừa nhận nỗi buồn không phóng đại), Nhân tính chung (ai cũng có lúc thất bại) và Sự tử tế (đối xử với bản thân như một người bạn thân thiết)."
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            activitiesToAdd.Add(new PracticeActivity
            {
                Id = Guid.NewGuid(),
                CourseId = PsychologyCourseId,
                LessonId = PsychologyLesson32Id,
                OrderIndex = 2,
                Type = "reflection_journal",
                Title = "Nhật ký Tự Trắc Ẩn: 3 Bước Xoa Dịu Căng Thẳng & Lo Âu",
                Description = "Ghi lại bài thực hành tự trắc ẩn của Kristin Neff để vượt qua thời điểm căng thẳng.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    model = "Self_Compassion_Break",
                    prompts = new[]
                    {
                        "1. Chánh niệm: Đây là một khoảnh khắc đau buồn / căng thẳng (thừa nhận cảm giác hiện tại).",
                        "2. Nhân tính chung: Nỗi đau và thử thách là một phần của trải nghiệm làm người (mình không đơn độc).",
                        "3. Tử tế với bản thân: Cầu chúc cho bản thân được bình an, bao dung và có đủ sức mạnh để vượt qua."
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (activitiesToAdd.Count > 0)
        {
            db.PracticeActivities.AddRange(activitiesToAdd);
            await db.SaveChangesAsync();
        }
    }
}
