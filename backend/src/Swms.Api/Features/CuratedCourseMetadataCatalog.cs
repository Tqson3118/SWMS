using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Database;

namespace Swms.Api.Features;

public sealed record CourseCuratedMetadata(
    string Category,
    string Level,
    string TargetAudience,
    string LabLabel,
    bool HasAudio,
    string[] LearningOutcomes,
    string[] Prerequisites);

public static class CuratedCourseMetadataCatalog
{
    private static readonly Dictionary<string, CourseCuratedMetadata> MetadataBySlug = new(StringComparer.OrdinalIgnoreCase)
    {
        ["doc-course-studxy-fullstack-bootcamp"] = new(
            Category: "Lập trình Full-Stack",
            Level: "intermediate",
            TargetAudience: "Lập trình viên muốn làm chủ stack .NET 10 Web API & React 19 TypeScript hiện đại, kiến trúc Clean/Vertical Slice và CI/CD.",
            LabLabel: "Code Lab & Debug",
            HasAudio: false,
            LearningOutcomes:
            [
                "Thiết kế và xây dựng RESTful Web API chuẩn enterprise với ASP.NET Core và C# 13.",
                "Xây dựng giao diện Single Page Application mượt mà, responsive với React 19, TypeScript và Tailwind CSS.",
                "Mô hình hóa dữ liệu và tối ưu truy vấn hiệu năng cao với Entity Framework Core và PostgreSQL/SQLite.",
                "Áp dụng nguyên lý Clean Architecture, CQRS, xác thực JWT & Google OAuth2 và bảo mật đa lớp.",
                "Tự động hóa kiểm thử đơn vị, kiểm thử tích hợp xUnit và đóng gói container Docker sẵn sàng deploy."
            ],
            Prerequisites:
            [
                "Nền tảng lập trình hướng đối tượng (OOP) cơ bản và tư duy thuật toán.",
                "Hiểu biết cơ bản về HTML, CSS và cú pháp JavaScript ES6.",
                "Đã cài đặt .NET SDK, Node.js và một trình soạn thảo mã nguồn (VS Code / Visual Studio)."
            ]
        ),

        ["document-course-ielts"] = new(
            Category: "Ngoại ngữ IELTS",
            Level: "intermediate",
            TargetAudience: "Học viên chuẩn bị thi IELTS Academic đặt mục tiêu band 6.5–7.5+, cần củng cố phản xạ nghe – nói tự nhiên và tư duy viết học thuật.",
            LabLabel: "Phòng Nghe – Nói IPA",
            HasAudio: true,
            LearningOutcomes:
            [
                "Làm chủ 4 kỹ năng Listening, Reading, Writing và Speaking theo format bài thi chuẩn Cambridge IELTS.",
                "Rèn luyện kỹ thuật Skimming, Scanning và nhận diện Paraphrasing để xử lý bài đọc học thuật phức tạp.",
                "Phát triển phản xạ phát âm chuẩn IPA, ngữ điệu tự nhiên và cấu trúc bài nói mạch lạc cho IELTS Speaking Part 1-3.",
                "Xây dựng luận điểm sắc bén, liên kết ý logic và đa dạng hóa cấu trúc ngữ pháp trong IELTS Writing Task 1 & 2.",
                "Ghi nhớ và ứng dụng hơn 1.200 từ vựng học thuật chuyên sâu theo chủ đề qua hệ thống lặp lại ngắt quãng FSRS."
            ],
            Prerequisites:
            [
                "Trình độ tiếng Anh tương đương CEFR B1 hoặc IELTS 4.5–5.0.",
                "Nắm vững ngữ pháp cơ bản (các thì, câu điều kiện, mệnh đề quan hệ).",
                "Có tai nghe hoặc thiết bị âm thanh để luyện tập các bài nghe tương tác."
            ]
        ),

        ["document-course-investing"] = new(
            Category: "Tài chính & Đầu tư",
            Level: "intermediate",
            TargetAudience: "Nhà đầu tư cá nhân muốn xây dựng hệ thống phân tích doanh nghiệp, định giá cổ phiếu và quản trị danh mục khoa học, tránh bẫy tâm lý thị trường.",
            LabLabel: "Lab Đầu tư & Định giá",
            HasAudio: false,
            LearningOutcomes:
            [
                "Đọc hiểu và bóc tách chuyên sâu Báo cáo tài chính (Bảng CĐKT, Báo cáo KQKD, Báo cáo LCTT) của doanh nghiệp niêm yết.",
                "Áp dụng các phương pháp định giá phổ biến (P/E, P/B, DCF chiết khấu dòng tiền) để xác định biên an toàn đầu tư.",
                "Thiết kế danh mục đầu tư đa tài sản tối ưu rủi ro và lợi nhuận phù hợp với từng chu kỳ kinh tế vĩ mô.",
                "Thiết lập kỷ luật cắt lỗ, chốt lời và chiến lược bình quân giá DCA kiểm soát tối đa mức sụt giảm (Max Drawdown).",
                "Nhận diện các thiên kiến hành vi kinh điển (FOMO, mỏ neo, tâm lý bầy đàn) để giữ vững quyết định lý tính."
            ],
            Prerequisites:
            [
                "Hiểu biết các khái niệm toán tài chính căn bản (lãi suất, tỷ suất sinh lời, phần trăm).",
                "Có mong muốn nghiêm túc học đầu tư dài hạn dựa trên giá trị cốt lõi của doanh nghiệp."
            ]
        ),

        ["document-course-personalfinance"] = new(
            Category: "Tài chính & Đầu tư",
            Level: "foundation",
            TargetAudience: "Sinh viên, người mới đi làm và gia đình trẻ muốn làm chủ dòng tiền, xóa nợ xấu, xây dựng quỹ dự phòng và lộ trình tự do tài chính (FIRE).",
            LabLabel: "Lab Tài chính Cá nhân",
            HasAudio: false,
            LearningOutcomes:
            [
                "Kiểm soát toàn diện dòng tiền thu chi cá nhân với mô hình 6 chiếc lọ và nguyên tắc ngân sách 50/30/20 linh hoạt.",
                "Xây dựng quỹ khẩn cấp 3–6 tháng chi phí sinh hoạt và chiến lược dọn dẹp nợ lãi cao hiệu quả (Snowball/Avalanche).",
                "Hiểu rõ bản chất các loại bảo hiểm nhân thọ, phi nhân thọ để bảo vệ tài chính gia đình trước rủi ro bất ngờ.",
                "Tận dụng sức mạnh lãi kép để bắt đầu tích lũy đầu tư quỹ chỉ số, vàng và tiền gửi tiết kiệm thông minh từ số vốn nhỏ.",
                "Xác định con số mục tiêu độc lập tài chính (FIRE) và lập bản đồ kế hoạch tài chính cá nhân 5–10 năm."
            ],
            Prerequisites:
            [
                "Không yêu cầu kiến thức tài chính chuyên sâu trước đó.",
                "Cam kết theo dõi và ghi chép chi tiêu thực tế trong quá trình học tập."
            ]
        ),

        ["document-course-lifeskills"] = new(
            Category: "Tâm lý & Kỹ năng sống",
            Level: "foundation",
            TargetAudience: "Tất cả người học muốn nâng cao trí tuệ cảm xúc (EQ), giao tiếp trắc ẩn, giải quyết xung đột, tư duy phản biện và quản lý năng lượng cá nhân.",
            LabLabel: "Phòng Tình huống NVC & CBT",
            HasAudio: false,
            LearningOutcomes:
            [
                "Thực hành giao tiếp phi bạo lực (NVC - Nonviolent Communication) để biểu đạt nhu cầu và lắng nghe trắc ẩn mà không phán xét.",
                "Nhận diện và điều hòa cảm xúc cá nhân bằng các kỹ thuật tái cấu trúc nhận thức và thở chánh niệm.",
                "Phát triển tư duy phản biện (Critical Thinking) để phân biệt dữ kiện thật, ý kiến chủ quan và nhận diện ngụy biện logic.",
                "Thiết lập ranh giới lành mạnh trong công việc và các mối quan hệ xã hội mà không cảm thấy tội lỗi hay đối đầu.",
                "Quản lý năng lượng sinh học và thời gian theo ma trận Eisenhower để làm việc hiệu quả và hạn chế kiệt sức (burnout)."
            ],
            Prerequisites:
            [
                "Tâm thế cởi mở, sẵn sàng tự soi chiếu hành vi và thói quen giao tiếp của bản thân.",
                "Mong muốn cải thiện chất lượng mối quan hệ và sự an yên trong nội tâm."
            ]
        ),

        ["document-course-psychology"] = new(
            Category: "Tâm lý & Kỹ năng sống",
            Level: "intermediate",
            TargetAudience: "Người đam mê khám phá cơ chế não bộ, khoa học hành vi con người, tâm lý học nhận thức, các kiểu gắn bó tình cảm và liệu pháp tâm lý.",
            LabLabel: "Phòng Tình huống NVC & CBT",
            HasAudio: false,
            LearningOutcomes:
            [
                "Hiểu rõ cơ chế hoạt động của não bộ, hệ thần kinh và sự điều tiết của các chất dẫn truyền thần kinh (Dopamine, Serotonin, Cortisol).",
                "Phân tích cơ chế tư duy của Hệ thống 1 (trực giác nhanh) và Hệ thống 2 (suy luận chậm) theo lý thuyết Kahneman.",
                "Khám phá 4 phong cách gắn bó trong tình yêu (an toàn, lo âu, né tránh, hỗn loạn) và phương pháp chữa lành vết thương tâm lý.",
                "Ứng dụng mô hình ABC trong Liệu pháp Nhận thức Hành vi (CBT) để nhận diện và thay đổi các suy nghĩ tự động tiêu cực.",
                "Làm chủ các quy luật ghi nhớ não bộ (Đường cong quên Ebbinghaus, Chunking, Retrieval Practice) để học tập siêu tốc."
            ],
            Prerequisites:
            [
                "Yêu thích tìm hiểu về con người và khoa học tâm lý học đại cương.",
                "Sẵn sàng đọc các nghiên cứu thực nghiệm và tình huống lâm sàng thực tế."
            ]
        ),

        ["he-dieu-hanh-bo-nho-ao"] = new(
            Category: "Khoa học máy tính",
            Level: "advanced",
            TargetAudience: "Sinh viên Khoa học Máy tính, kỹ sư phần mềm muốn hiểu sâu tầng hệ thống: cơ chế bộ nhớ ảo, paging phần cứng, MMU, TLB và tối ưu hiệu năng OS.",
            LabLabel: "OS Memory & Scheduling Lab",
            HasAudio: false,
            LearningOutcomes:
            [
                "Làm chủ nguyên lý quản lý bộ nhớ ảo (Virtual Memory), phân đoạn (Segmentation) và phân trang (Paging) trong hệ điều hành hiện đại.",
                "Phân tích cơ chế hoạt động của Memory Management Unit (MMU) và Translation Lookaside Buffer (TLB) trong ánh xạ địa chỉ ảo-vật lý.",
                "Tính toán và tối ưu thời gian truy cập bộ nhớ hiệu dụng (EAT) dựa trên tỷ lệ hit/miss TLB.",
                "Hiểu rõ cấu trúc bảng trang đa cấp 4 tầng (PGD, PUD, PMD, PTE) và bảng trang nghịch đảo (Inverted Page Table).",
                "So sánh, mô phỏng và cài đặt các thuật toán thay thế trang kinh điển: FIFO, LRU, Clock (Second Chance) và Optimal."
            ],
            Prerequisites:
            [
                "Nắm vững ngôn ngữ C/C++ cơ bản và kiến trúc máy tính (thanh ghi, CPU, RAM).",
                "Hiểu biết về hệ nhị phân, thập lục phân và các phép toán thao tác bit (bitmask, bitwise)."
            ]
        ),

        ["tieng-anh-giao-tiep-thuc-chien-a2-b1"] = new(
            Category: "Ngoại ngữ",
            Level: "foundation",
            TargetAudience: "Người đi làm hoặc sinh viên mất gốc tiếng Anh cần xây dựng phản xạ giao tiếp tự tin trong đời sống hàng ngày và môi trường công sở quốc tế.",
            LabLabel: "Phòng Nghe – Nói IPA",
            HasAudio: true,
            LearningOutcomes:
            [
                "Tự tin giới thiệu bản thân, bắt chuyện Small Talk và duy trì cuộc đối thoại tự nhiên với đồng nghiệp và người bản xứ.",
                "Xử lý linh hoạt các tình huống hỏi đường, đặt phòng khách sạn, gọi món nhà hàng và mua sắm khi đi công tác nước ngoài.",
                "Viết email công việc chuẩn mực: đặt lịch hẹn, báo cáo tiến độ dự án, xin nghỉ phép và theo dõi phản hồi chuyên nghiệp.",
                "Nắm vững mẫu câu tham gia cuộc họp trực tuyến, bày tỏ quan điểm đồng tình hoặc từ chối lịch sự và tóm tắt biên bản.",
                "Trình bày thuyết trình ngắn (5–10 phút) với cấu trúc mở đầu, dẫn chứng và kết luận rõ ràng, mạch lạc."
            ],
            Prerequisites:
            [
                "Biết phát âm bảng chữ cái tiếng Anh và các từ vựng chào hỏi cơ bản.",
                "Sẵn sàng luyện nói to và thu âm kiểm tra phát âm hàng ngày."
            ]
        ),

        ["tieng-nhat-so-cap-n5"] = new(
            Category: "Ngoại ngữ",
            Level: "foundation",
            TargetAudience: "Người mới bắt đầu học tiếng Nhật từ con số 0, chuẩn bị du học, làm việc tại doanh nghiệp Nhật Bản hoặc thi đỗ chứng chỉ JLPT N5.",
            LabLabel: "Lab Luyện Nhật N5",
            HasAudio: true,
            LearningOutcomes:
            [
                "Làm chủ thành thạo 2 bảng chữ cái Hiragana và Katakana cùng các biến âm (Dakuon, Handakuon, Yoon, Sokuon).",
                "Nhận diện và viết đúng 100 chữ Hán (Kanji) căn bản cấp độ N5 kèm âm On và âm Kun thông dụng.",
                "Nắm vững hơn 600 từ vựng cốt lõi và các trợ từ cơ bản (は, が, を, に, で, へ, と, も, から, まで) trong tiếng Nhật.",
                "Chia thành thạo các thể động từ cơ bản (thể Masu, thể Te, thể Nai, thể Từ điển) và các mẫu câu xin phép, chỉ thị, cấm đoán.",
                "Tự tin nghe hiểu các đoạn hội thoại sinh hoạt thường ngày và đọc hiểu mẩu tin ngắn chuẩn đề thi JLPT N5."
            ],
            Prerequisites:
            [
                "Không yêu cầu kiến thức tiếng Nhật trước đó.",
                "Chuẩn bị tập viết ô ly để rèn luyện nét chữ Hiragana, Katakana và Kanji."
            ]
        ),

        ["tieng-trung-so-cap-hsk1"] = new(
            Category: "Ngoại ngữ",
            Level: "foundation",
            TargetAudience: "Người bắt đầu tiếp cận tiếng Trung Quốc (tiếng Quan Thoại), mong muốn phát âm chuẩn Bắc Kinh và đạt chứng chỉ HSK 1 với điểm số cao.",
            LabLabel: "Lab Luyện Trung HSK1",
            HasAudio: true,
            LearningOutcomes:
            [
                "Làm chủ hệ thống ngữ âm Pinyin (Phiên âm): 21 thanh mẫu, 36 vận mẫu và 4 thanh điệu chuẩn xác cùng quy tắc biến điệu.",
                "Nhận diện và ghi nhớ cách viết 150 chữ Hán (Hán tự) thông dụng nhất cấp độ HSK 1 theo đúng quy tắc thuận bút.",
                "Sử dụng thành thạo hơn 150 từ vựng và các cấu trúc ngữ pháp then chốt để giao tiếp trong các chủ đề căn bản.",
                "Thực hành hội thoại chào hỏi, hỏi tuổi, hỏi ngày giờ, đếm số, giới thiệu thành viên gia đình và mua sắm đồ dùng hàng ngày.",
                "Rèn luyện kỹ năng nghe hiểu tốc độ chậm và đọc lướt câu ngắn, tự tin vượt qua bài thi HSK 1 với số điểm 180+/200."
            ],
            Prerequisites:
            [
                "Không yêu cầu biết tiếng Trung trước đó.",
                "Có micro hoặc thiết bị ghi âm để luyện tập đối chiếu thanh điệu Pinyin."
            ]
        )
    };

    private static readonly Dictionary<Guid, CourseCuratedMetadata> MetadataByCourseId = new()
    {
        [SeedDataInitializer.AcademicCourseId] = MetadataBySlug["he-dieu-hanh-bo-nho-ao"],
        [SeedDataInitializer.EnglishCourseId] = MetadataBySlug["tieng-anh-giao-tiep-thuc-chien-a2-b1"],
        [SeedDataInitializer.JapaneseCourseId] = MetadataBySlug["tieng-nhat-so-cap-n5"],
        [SeedDataInitializer.ChineseCourseId] = MetadataBySlug["tieng-trung-so-cap-hsk1"]
    };

    public static CourseCuratedMetadata ResolveMetadata(Course course)
    {
        CourseCuratedMetadata? defaultMeta = null;
        if (!string.IsNullOrWhiteSpace(course.Slug) && MetadataBySlug.TryGetValue(course.Slug, out var bySlug))
        {
            defaultMeta = bySlug;
        }
        else if (MetadataByCourseId.TryGetValue(course.Id, out var byId))
        {
            defaultMeta = byId;
        }

        defaultMeta ??= ResolveFallbackMetadata(course);

        var effectiveCategory = !string.IsNullOrWhiteSpace(course.Category)
            ? course.Category.Trim()
            : defaultMeta.Category;

        var effectiveLevel = !string.IsNullOrWhiteSpace(course.Level)
            ? course.Level.Trim().ToLowerInvariant()
            : defaultMeta.Level;

        var effectiveOutcomes = TryDeserializeStringList(course.LearningOutcomesJson);
        if (effectiveOutcomes == null || effectiveOutcomes.Length == 0)
        {
            effectiveOutcomes = defaultMeta.LearningOutcomes;
        }

        var effectivePrerequisites = TryDeserializeStringList(course.PrerequisitesJson);
        if (effectivePrerequisites == null || effectivePrerequisites.Length == 0)
        {
            effectivePrerequisites = defaultMeta.Prerequisites;
        }

        return new CourseCuratedMetadata(
            Category: effectiveCategory,
            Level: effectiveLevel,
            TargetAudience: defaultMeta.TargetAudience,
            LabLabel: defaultMeta.LabLabel,
            HasAudio: defaultMeta.HasAudio,
            LearningOutcomes: effectiveOutcomes,
            Prerequisites: effectivePrerequisites);
    }

    private static string[]? TryDeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(json);
            return list != null && list.Count > 0 ? list.ToArray() : null;
        }
        catch
        {
            return null;
        }
    }

    private static CourseCuratedMetadata ResolveFallbackMetadata(Course course)
    {
        var title = course.Title ?? string.Empty;
        var desc = course.Description ?? string.Empty;

        string category = "Chung";
        string level = "foundation";
        string targetAudience = "Người học tự định hướng trên nền tảng Studxy.";
        string labLabel = "Thực hành & Ôn tập";
        bool hasAudio = false;

        if (title.Contains("Lập trình", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("Code", StringComparison.OrdinalIgnoreCase) ||
            title.Contains(".NET", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("React", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("Web", StringComparison.OrdinalIgnoreCase))
        {
            category = "Lập trình Full-Stack";
            level = "intermediate";
            labLabel = "Code Lab & Debug";
        }
        else if (title.Contains("Tài chính", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Đầu tư", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Tiền tệ", StringComparison.OrdinalIgnoreCase))
        {
            category = "Tài chính & Đầu tư";
            level = "intermediate";
            labLabel = "Lab Tài chính & Đầu tư";
        }
        else if (title.Contains("Tâm lý", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Kỹ năng", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Giao tiếp", StringComparison.OrdinalIgnoreCase))
        {
            category = "Tâm lý & Kỹ năng sống";
            level = "foundation";
            labLabel = "Phòng Tình huống & EQ";
        }
        else if (title.Contains("Tiếng", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("IELTS", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Ngoại ngữ", StringComparison.OrdinalIgnoreCase))
        {
            category = "Ngoại ngữ";
            level = "foundation";
            labLabel = "Phòng Nghe – Nói";
            hasAudio = true;
        }

        return new CourseCuratedMetadata(
            Category: category,
            Level: level,
            TargetAudience: targetAudience,
            LabLabel: labLabel,
            HasAudio: hasAudio,
            LearningOutcomes: new[] { "Nắm vững kiến thức nền tảng và phương pháp tư duy", "Áp dụng vào thực hành và giải quyết vấn đề thực tế" },
            Prerequisites: new[] { "Sẵn sàng học tập và kiên trì rèn luyện mỗi ngày" });
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task ApplyDefaultsToCoursesAsync(SwmsDbContext db, CancellationToken ct = default)
    {
        var courses = await db.Courses.ToListAsync(ct);
        bool modified = false;
        var now = DateTimeOffset.UtcNow;

        foreach (var course in courses)
        {
            if (DocumentCourseSyncService.IsRetiredCourseSlug(course.Slug))
            {
                continue;
            }

            bool courseChanged = false;
            var meta = ResolveMetadata(course);

            if (string.IsNullOrWhiteSpace(course.Category) && !string.IsNullOrWhiteSpace(meta.Category))
            {
                course.Category = meta.Category;
                courseChanged = true;
            }

            if (string.IsNullOrWhiteSpace(course.Level) && !string.IsNullOrWhiteSpace(meta.Level))
            {
                course.Level = meta.Level;
                courseChanged = true;
            }

            if (string.IsNullOrWhiteSpace(course.LearningOutcomesJson) && meta.LearningOutcomes is { Length: > 0 })
            {
                course.LearningOutcomesJson = JsonSerializer.Serialize(meta.LearningOutcomes, JsonOptions);
                courseChanged = true;
            }

            if (string.IsNullOrWhiteSpace(course.PrerequisitesJson) && meta.Prerequisites is { Length: > 0 })
            {
                course.PrerequisitesJson = JsonSerializer.Serialize(meta.Prerequisites, JsonOptions);
                courseChanged = true;
            }

            if (courseChanged)
            {
                course.UpdatedAt = now;
                modified = true;
            }
        }

        if (modified)
        {
            await db.SaveChangesAsync(ct);
        }
    }
}
