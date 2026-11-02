using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Security;

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

    public static async Task SeedAsync(SwmsDbContext db)
    {
        await SeedLock.WaitAsync();
        try
        {
            if (await db.Users.AnyAsync())
                return;

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
        }
        finally
        {
            SeedLock.Release();
        }
    }
}
