namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Swms.Api.Domain.Entities;

/// <summary>
/// Expanded Write-in Practice Content for all 6 lessons of Psychology &amp; Cognitive Science Course.
/// Ensures EVERY lesson has &gt;= 8 write-in activities (OrderIndex 3..10):
/// - bias_spotter (&gt;= 2 activities/prompts): real scenarios requiring typing bias name and reason
/// - feynman (&gt;= 2 activities/prompts): core concepts for student to self-define with everyday examples
/// - cbt_exercise (&gt;= 2 activities/prompts): CBT scenarios to identify cognitive distortions and reframe
/// - apply_journal (&gt;= 2 activities/prompts): real-world application &amp; NVC 4-step communication.
/// </summary>
public static class PsychologyWriteInContent
{
    public static List<PracticeActivity> GetActivities(DateTimeOffset now)
    {
        var list = new List<PracticeActivity>();
        var courseId = SeedDataInitializer.PsychologyCourseId;

        // =========================================================================
        // LESSON 1.1: Hệ Thống 1 & 2, Hiệu Ứng Stroop, Giao Tiếp NVC
        // (Legacy activities: OrderIndex 1, 2; Starter Write-in: OrderIndex 3..6)
        // We supplement with OrderIndex 7..10 so Lesson 1.1 has 8 write-in activities.
        // =========================================================================
        var l11 = SeedDataInitializer.PsychologyLesson11Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "bias_spotter", 7),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 7,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Phản Ứng Cảm Xúc Vội Vàng Của Hệ Thống 1",
            Description = "Đọc tình huống đời thực, tự gõ tên thiên kiến/cơ chế tâm lý và viết lý do ngắn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l11-bias-3",
                        title = "Quyết định mua sắm vội vàng do giảm giá ảo",
                        contextTag = "Hệ thống 1 & Thiên kiến Mỏ neo",
                        scenario = "Một khóa học kỹ năng đề giá gốc 10.000.000đ gạch ngang, chỉ còn 1.990.000đ trong 2 giờ tới. Dù chưa đọc kỹ đề cương, Hùng vẫn quẹt thẻ mua ngay vì Hệ thống 1 phản ứng với con số 10 triệu làm mỏ neo giảm giá.",
                        canonicalNameVi = "Hiệu ứng mỏ neo / Phản ứng Hệ thống 1 (Anchoring Bias / System 1)",
                        acceptedNames = new[] { "Hiệu ứng mỏ neo", "Thiên kiến mỏ neo", "Mỏ neo", "Hệ thống 1", "Anchoring Bias", "Anchoring", "System 1" },
                        reasonKeywords = new[] { "mỏ neo", "giá gốc", "hệ thống 1", "vội vàng", "giảm giá" },
                        minReasonWords = 8,
                        sampleExplanation = "Hệ thống 1 bị thu hút bởi con số 10 triệu đầu tiên làm mỏ neo tham chiếu, tạo cảm giác 'hời ảo' mà không kích hoạt Hệ thống 2 để kiểm chứng giá trị thực tế của khóa học.",
                        counterMeasureVi = "Dừng lại 24 giờ trước khi mua sắm giảm giá, tra cứu đánh giá độc lập của học viên cũ."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "feynman", 8),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 8,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: Hiệu Ứng Xung Đột Stroop & Ức Chế Nhận Thức",
            Description = "Tự diễn giải cơ chế Hiệu ứng Stroop bằng ngôn ngữ đời thường kèm ví dụ minh họa.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l11-feyn-2",
                        conceptTitle = "Hiệu ứng Stroop (Stroop Effect & Cognitive Interference)",
                        promptVi = "Hãy tự định nghĩa cơ chế Hiệu ứng Stroop và giải thích vì sao não bộ bị chậm lại khi màu mực xung đột với nghĩa của chữ.",
                        requiredKeywords = new[] { "stroop", "xung đột|can thiệp", "tự động|hệ thống 1", "chậm lại|độ trễ", "màu mực" },
                        minWords = 16,
                        sampleAnswer = "Hiệu ứng Stroop xảy ra khi quá trình đọc chữ tự động của Hệ thống 1 xung đột với nhiệm vụ gọi tên màu mực của Hệ thống 2. Sự can thiệp nhận thức này làm tăng thời gian phản xạ và dễ gây nói nhầm.",
                        everydayExampleHintVi = "Ví dụ chữ 'ĐỎ' được in bằng mực xanh lá cây khiến bạn mất thêm nửa giây để gọi đúng tên 'xanh lá'."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "cbt_exercise", 9),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 9,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: Phán Xét Nhân Cách Thay Vì Quan Sát Hành Vi",
            Description = "Phân biệt giữa dán nhãn nhân cách và quan sát khách quan, tái cấu trúc suy nghĩ.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l11-cbt-2",
                        title = "Bạn bè quên chúc mừng sinh nhật",
                        scenario = "Hôm nay là sinh nhật của bạn, người bạn thân nhất không nhắn tin chúc mừng. Bạn cảm thấy bị phản bội và nghĩ bạn mình là kẻ vô ơn, ích kỷ.",
                        sampleAutomaticThought = "Nó không coi mình ra gì, đúng là một kẻ ích kỷ và vô tâm.",
                        canonicalDistortionVi = "Dán nhãn / Khái quát hóa quá mức (Labeling / Overgeneralization)",
                        acceptedDistortions = new[] { "Dán nhãn", "dan nhan", "Labeling", "Khái quát hóa quá mức", "Overgeneralization", "Đọc suy nghĩ", "Mind Reading" },
                        distortionChoicesHint = new[] { "Dán nhãn (Labeling)", "Đọc suy nghĩ (Mind Reading)", "Trầm trọng hóa (Catastrophizing)" },
                        balancedKeywords = new[] { "bận", "quên", "không có nghĩa", "hỏi thăm", "tình bạn" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Việc bạn thân chưa nhắn tin có thể do họ đang bận việc đột xuất hoặc quên ngày; điều đó không đồng nghĩa họ là người vô ơn hay phủ nhận tình bạn nhiều năm qua. Mình có thể chủ động nhắn tin rủ bạn đi uống cà phê.",
                        explanationVi = "Quy chụp toàn bộ nhân cách một người dựa trên một hành vi đơn lẻ là lỗi Dán nhãn (Labeling)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "apply_journal", 10),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 10,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Thiết Kế Khoảng Lặng 5 Giây Trước Khi Phản Hồi Xung Đột",
            Description = "Viết kế hoạch kiểm soát phản ứng bộc phát khi đối mặt với lời chỉ trích nơi làm việc.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l11-apply-2",
                        title = "Quy trình dừng 5 giây và chuyển giao quyền điều khiển cho Hệ thống 2",
                        promptVi = "Khi bị người khác chỉ trích gay gắt, hãy mô tả cách bạn hít thở sâu 5 giây để hạ nhiệt phản xạ Hệ thống 1 và soạn câu trả lời khách quan.",
                        expectedKeywords = new[] { "hít thở", "5 giây", "hệ thống 2", "bình tĩnh", "lắng nghe" },
                        samplePlanVi = "Khi nhận phản hồi tiêu cực, mình sẽ hít sâu 3 nhịp trong 5 giây, ghi nhận cảm giác tức giận nhưng không nói ngay. Sau đó mình sẽ hỏi lại để làm rõ sự kiện cụ thể trước khi đưa ra phương án giải quyết."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "apply_journal", 11),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 11,
            Type = "apply_journal",
            Title = "Tình Huống Khởi Động: Nhận Diện Hệ Thống 1 Khi Mua Hàng Flash Sale & Quy Tắc 24 Giờ",
            Description = "Nhận diện phản ứng bốc đồng của Hệ thống 1 trước chiêu trò giảm giá và thiết lập bộ đệm trì hoãn 24 giờ cho Hệ thống 2.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l11-warmup",
                        title = "Phản xạ săn sale chớp nhoáng trên sàn thương mại điện tử",
                        promptVi = "Khi thấy thông báo 'Flash Sale giảm 60% chỉ còn 8 phút', Hệ thống 1 của bạn phản ứng thế nào và bạn sẽ áp dụng 'Quy tắc 24 giờ' ra sao để trao quyền quyết định cho Hệ thống 2?",
                        expectedKeywords = new[] { "hệ thống 1", "hệ thống 2", "24 giờ", "flash sale", "cảm xúc", "cần thiết" },
                        samplePlanVi = "Hệ thống 1 bị kích hoạt bởi nỗi sợ bỏ lỡ (FOMO) và con số giảm giá ảo, thôi thúc nhấn nút mua ngay lập tức. Để kiểm soát, mình áp dụng quy tắc 24 giờ: thêm món hàng vào giỏ nhưng không thanh toán, chờ sau 24 giờ khi cảm xúc hưng phấn hạ nhiệt để Hệ thống 2 đánh giá xem món đồ có thực sự cần thiết với ngân sách hay không."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // =========================================================================
        // LESSON 1.2: Thiên Kiến Xác Nhận, Mỏ Neo, Hiệu Ứng Khung (Framing)
        // (Legacy activities: OrderIndex 1, 2)
        // Write-in activities: OrderIndex 3..10 (8 activities)
        // =========================================================================
        var l12 = SeedDataInitializer.PsychologyLesson12Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "bias_spotter", 3),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 3,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Chỉ Tìm Dẫn Chứng Ủng Hộ Niềm Tin Có Sẵn",
            Description = "Tự gõ tên thiên kiến và giải thích biểu hiện trong tình huống nghiên cứu.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l12-bias-1",
                        title = "Tìm kiếm thông tin một chiều về phương pháp ăn kiêng",
                        contextTag = "Tư duy phản biện & Sức khỏe",
                        scenario = "Phương tin rằng chế độ ăn KETO chữa bách bệnh. Khi đọc tài liệu, Phương chỉ bấm vào các bài viết ca ngợi KETO và lập tức lướt qua các cảnh báo của bác sĩ về nguy cơ tăng mỡ máu.",
                        canonicalNameVi = "Thiên kiến xác nhận (Confirmation Bias)",
                        acceptedNames = new[] { "Thiên kiến xác nhận", "thien kien xac nhan", "Confirmation Bias", "Confirmation" },
                        reasonKeywords = new[] { "chọn lọc", "bỏ qua", "ủng hộ", "niềm tin", "bằng chứng" },
                        minReasonWords = 8,
                        sampleExplanation = "Phương chỉ chọn lọc thông tin ủng hộ niềm tin có sẵn và gạt bỏ bằng chứng phản biện — biểu hiện kinh điển của Thiên kiến xác nhận.",
                        counterMeasureVi = "Chủ động tìm kiếm 2 bài báo khoa học uy tín chỉ ra rủi ro của phương pháp trước khi áp dụng."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "bias_spotter", 4),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 4,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Hiệu Ứng Khung Trong Thông Báo Tỷ Lệ Phẫu Thuật",
            Description = "Phân tích cách đóng khung ngôn từ ảnh hưởng đến quyết định của người nghe.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l12-bias-2",
                        title = "90% cơ hội sống sót vs 10% nguy cơ tử vong",
                        contextTag = "Hiệu ứng Khung (Framing Effect)",
                        scenario = "Bác sĩ A nói với bệnh nhân 'Ca phẫu thuật có 90% tỷ lệ thành công', bệnh nhân vui vẻ đồng ý. Bác sĩ B nói với bệnh nhân khác 'Ca mổ có 10% nguy cơ tử vong', bệnh nhân hoảng sợ từ chối dù cùng một xác suất toán học.",
                        canonicalNameVi = "Hiệu ứng khung (Framing Effect)",
                        acceptedNames = new[] { "Hiệu ứng khung", "hieu ung khung", "Framing Effect", "Framing", "Dong khung" },
                        reasonKeywords = new[] { "đóng khung", "xác suất", "tỷ lệ", "tâm lý", "ngôn từ" },
                        minReasonWords = 8,
                        sampleExplanation = "Cách diễn đạt tích cực (90% sống) kích hoạt tâm lý tìm kiếm an toàn, trong khi diễn đạt tiêu cực (10% chết) kích hoạt nỗi sợ mất mát dù bản chất dữ liệu y khoa như nhau.",
                        counterMeasureVi = "Luôn quy đổi thông tin sang cả hai mặt được/mất trước khi ra quyết định hệ trọng."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "feynman", 5),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 5,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: Thiên Kiến Xác Nhận (Confirmation Bias)",
            Description = "Tự diễn giải Thiên kiến xác nhận cho một học sinh cấp 2 hiểu được kèm ví dụ sinh động.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l12-feyn-1",
                        conceptTitle = "Thiên kiến xác nhận (Confirmation Bias)",
                        promptVi = "Hãy tự giải thích Thiên kiến xác nhận bằng lời của bạn và nêu 1 ví dụ cụ thể trong việc sử dụng mạng xã hội.",
                        requiredKeywords = new[] { "thiên kiến xác nhận", "tìm kiếm|thu nhận", "ủng hộ|đồng tình", "bỏ qua|phớt lờ", "thuật toán|mạng xã hội" },
                        minWords = 16,
                        sampleAnswer = "Thiên kiến xác nhận là xu hướng não bộ chỉ ghi nhận thông tin đồng tình với quan điểm sẵn có và tự động bỏ qua bằng chứng trái chiều. Trên mạng xã hội, thuật toán gợi ý các video hợp gu khiến chúng ta tưởng cả thế giới đều nghĩ giống mình.",
                        everydayExampleHintVi = "Ví dụ tin rằng cung hoàng đạo của mình bướng bỉnh thì chỉ nhớ những lúc mình bướng bỉnh."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "feynman", 6),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 6,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: Hiệu Ứng Mỏ Neo (Anchoring Effect)",
            Description = "Tự giải thích cơ chế con số đầu tiên chi phối toàn bộ thang đánh giá tiếp theo.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l12-feyn-2",
                        conceptTitle = "Hiệu ứng mỏ neo (Anchoring Effect)",
                        promptVi = "Hãy tự giải thích cơ chế hoạt động của Hiệu ứng mỏ neo trong đàm phán giá cả và mua bán hàng hóa.",
                        requiredKeywords = new[] { "mỏ neo", "con số đầu tiên|giá ban đầu", "tham chiếu", "điều chỉnh", "ảnh hưởng" },
                        minWords = 16,
                        sampleAnswer = "Hiệu ứng mỏ neo là hiện tượng con số đầu tiên chúng ta tiếp nhận trở thành cột mốc tham chiếu cho mọi phán đoán tiếp theo. Trong đàm phán, người ra giá mỏ neo đầu tiên thường kéo toàn bộ khoảng dao động về phía có lợi cho mình.",
                        everydayExampleHintVi = "Ví dụ người bán báo giá 500k, bạn mặc cả xuống 350k và thấy hời, dù giá trị thật chỉ 200k."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "cbt_exercise", 7),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 7,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: Lọc Tiêu Cực (Mental Filter) Khi Nhận Đánh Giá",
            Description = "Nhận diện lỗi chỉ tập trung vào 1 lời chê duy nhất giữa 10 lời khen ngợi.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l12-cbt-1",
                        title = "Bài thuyết trình nhận 9 lời khen và 1 góp ý chỉnh sửa font",
                        scenario = "Sau buổi báo cáo dự án, 9 đồng nghiệp khen ngợi phần phân tích xuất sắc. Một đồng nghiệp góp ý font chữ slide hơi nhỏ. Bạn về nhà mất ngủ và nghĩ cả buổi báo cáo là thảm họa.",
                        sampleAutomaticThought = "Mình làm hỏng hết cả buổi báo cáo rồi, mọi người chỉ để ý đến lỗi font chữ.",
                        canonicalDistortionVi = "Lọc tiêu cực (Mental Filter)",
                        acceptedDistortions = new[] { "Lọc tiêu cực", "loc tieu cuc", "Mental Filter", "Trầm trọng hóa", "Catastrophizing", "Tư duy trắng đen", "All-or-Nothing" },
                        distortionChoicesHint = new[] { "Lọc tiêu cực (Mental Filter)", "Trầm trọng hóa (Catastrophizing)", "Tư duy trắng - đen (All-or-Nothing)" },
                        balancedKeywords = new[] { "9 lời khen", "góp ý nhỏ", "chỉnh sửa", "tổng thể", "thành công" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Thực tế là 9/10 người đánh giá rất cao nội dung chuyên môn của mình; góp ý về font chữ chỉ là chi tiết kỹ thuật nhỏ dễ sửa trong 2 phút, không thể biến toàn bộ buổi báo cáo thành thất bại.",
                        explanationVi = "Tập trung tuyệt đối vào 1 chi tiết tiêu cực duy nhất và phớt lờ toàn bộ thành tựu tích cực là lỗi 'Lọc tiêu cực' (Mental Filter)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "cbt_exercise", 8),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 8,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: Tư Duy Trắng - Đen Trong Đặt Mục Tiêu Cá Nhân",
            Description = "Hóa giải lỗi nhận thức 'hoàn hảo hoặc không có gì' khi lỡ một thói quen.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l12-cbt-2",
                        title = "Lỡ ăn 1 miếng bánh ngọt khi đang ăn kiêng giảm cân",
                        scenario = "Bạn quyết tâm ăn lành mạnh trong 30 ngày. Đến ngày thứ 10, trong tiệc sinh nhật bạn ăn 1 miếng bánh ngọt nhỏ. Bạn nghĩ: 'Kế hoạch hỏng bét rồi, mình ăn xả láng luôn cho xong'.",
                        sampleAutomaticThought = "Đã phá vỡ kỷ luật rồi thì coi như công sức 10 ngày đổ sông đổ biển, ăn tiếp cũng chẳng sao.",
                        canonicalDistortionVi = "Tư duy trắng - đen (All-or-Nothing Thinking)",
                        acceptedDistortions = new[] { "Tư duy trắng - đen", "Tư duy trắng đen", "tu duy trang den", "All-or-Nothing", "Tất cả hoặc không có gì" },
                        distortionChoicesHint = new[] { "Tư duy trắng - đen (All-or-Nothing)", "Trầm trọng hóa (Catastrophizing)", "Biện minh cảm tính (Emotional Reasoning)" },
                        balancedKeywords = new[] { "1 miếng bánh", "10 ngày", "tiếp tục", "không phải thất bại", "dài hạn" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Một miếng bánh ngọt chỉ chiếm 200 calo trong hàng chục ngàn calo đã kiểm soát tốt 10 ngày qua. Mình vẫn đang đi đúng hướng 95% thời gian, bữa tiếp theo sẽ quay lại thực đơn chuẩn.",
                        explanationVi = "Xem một sai lệch nhỏ như sự thất bại 100% là biểu hiện điển hình của Tư duy trắng - đen (All-or-Nothing Thinking)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "apply_journal", 9),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 9,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Kỹ Thuật Steelman Phá Vỡ Thiên Kiến Xác Nhận",
            Description = "Tự viết lập luận mạnh nhất của phe đối lập cho một quan điểm gây tranh cãi.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l12-apply-1",
                        title = "Xây dựng lập luận mạnh nhất cho góc nhìn trái ngược (Steelman)",
                        promptVi = "Chọn một chủ đề bạn đang có quan điểm rất kiên định (ví dụ: làm việc từ xa vs lên văn phòng). Hãy viết 2 luận điểm mạnh nhất ủng hộ phương án bạn vốn không thích.",
                        expectedKeywords = new[] { "steelman", "góc nhìn đối lập", "luận điểm", "thấu cảm", "khách quan" },
                        samplePlanVi = "Dù thích làm từ xa, mình công nhận lên văn phòng giúp xây dựng niềm tin đồng đội nhanh hơn gấp nhiều lần và giảm thiểu rủi ro hiểu nhầm qua tin nhắn văn bản."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "apply_journal", 10),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 10,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Tái Đóng Khung Thông Điệp Để Đạt Đồng Thuận",
            Description = "Soạn thảo lại một đề xuất công việc từ góc độ giải quyết nỗi đau của cấp trên.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l12-apply-2",
                        title = "Chuyển từ khung chi phí sang khung đầu tư phòng ngừa rủi ro",
                        promptVi = "Hãy viết lại đề xuất mua phần mềm tự động hóa kiểm thử từ cách diễn đạt 'tốn thêm 20 triệu' sang đóng khung lợi ích dài hạn.",
                        expectedKeywords = new[] { "đóng khung", "lợi ích", "chi phí", "rủi ro", "đầu tư" },
                        samplePlanVi = "Thay vì nói 'Xin cấp 20 triệu mua tool test', mình sẽ đóng khung: 'Đầu tư 20 triệu cho tool kiểm thử giúp ngăn chặn nguy cơ crash hệ thống giờ cao điểm và tiết kiệm 80 giờ làm thêm của đội ngũ kỹ thuật.'"
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "bias_spotter", 11),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 11,
            Type = "bias_spotter",
            Title = "Tình Huống Khởi Động: Tách 'Sự Thật Quan Sát' Khỏi 'Suy Diễn Cảm Xúc' Trong Tin Nhắn",
            Description = "Phân biệt dữ kiện khách quan với suy diễn chủ quan để phá vỡ bẫy Thiên kiến xác nhận trong giao tiếp thường nhật.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l12-warmup",
                        title = "Tin nhắn trả lời cụt lủn và cái bẫy suy diễn ác ý",
                        contextTag = "Thiên kiến xác nhận & Suy diễn cảm xúc",
                        scenario = "Bạn nhắn một đoạn tin dài chia sẻ về ngày làm việc mệt mỏi cho bạn thân nhưng người đó chỉ trả lời vỏn vẹn chữ 'Ừ'. Bạn lập tức nghĩ rằng họ chán ghét mình và bắt đầu lục tìm lại các cử chỉ lạnh nhạt trước đây để củng cố cho kết luận đó.",
                        canonicalNameVi = "Thiên kiến xác nhận / Suy diễn cảm xúc (Confirmation Bias / Emotional Reasoning)",
                        acceptedNames = new[] { "Thiên kiến xác nhận", "Suy diễn cảm xúc", "Đọc suy nghĩ", "Confirmation Bias", "Emotional Reasoning", "Mind Reading" },
                        reasonKeywords = new[] { "sự thật", "suy diễn", "tin nhắn", "xác nhận", "bằng chứng", "khách quan" },
                        minReasonWords = 8,
                        sampleExplanation = "Sự thật quan sát khách quan duy nhất là tin nhắn chữ 'Ừ'. Việc kết luận bạn thân chán ghét mình là suy diễn chủ quan; việc lục tìm lại kỷ niệm tiêu cực để chứng minh là biểu hiện điển hình của Thiên kiến xác nhận.",
                        counterMeasureVi = "Tách riêng dữ kiện (bạn gửi tin ngắn) và suy diễn (bạn ghét mình). Chủ động hỏi thăm: 'Hôm nay cậu bận hoặc mệt à?' thay vì tự dán nhãn tiêu cực."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // =========================================================================
        // LESSON 2.1: Trí Nhớ Làm Việc, Spaced Repetition & Chunking
        // (Legacy activities: OrderIndex 1, 2)
        // Write-in activities: OrderIndex 3..10 (8 activities)
        // =========================================================================
        var l21 = SeedDataInitializer.PsychologyLesson21Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "bias_spotter", 3),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 3,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Ảo Tưởng Năng Lực Khi Đọc Thụ Động (Illusion of Competence)",
            Description = "Nhận diện sự nhầm lẫn giữa việc 'nhận ra' và 'nhớ lại được'.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l21-bias-1",
                        title = "Đọc highlight nhiều lần tưởng mình đã thuộc bài",
                        contextTag = "Khoa học Thần kinh & Học tập",
                        scenario = "Bảo đọc lại giáo trình 4 lần và dùng bút dạ quang tô kín các trang sách. Thấy nội dung rất quen thuộc, Bảo tự tin rằng mình đã nắm chắc 100%. Đến khi gấp sách lại làm đề thi thử, Bảo không viết nổi 2 dòng.",
                        canonicalNameVi = "Ảo tưởng năng lực (Illusion of Competence)",
                        acceptedNames = new[] { "Ảo tưởng năng lực", "ao tuong nang luc", "Illusion of Competence", "Ảo tưởng ghi nhớ", "Đọc thụ động" },
                        reasonKeywords = new[] { "quen thuộc", "nhận ra", "chủ động", "ảo tưởng", "gấp sách" },
                        minReasonWords = 8,
                        sampleExplanation = "Bảo nhầm lẫn cảm giác quen mắt khi đọc thụ động với năng lực truy xuất thông tin chủ động (Active Recall). Đây là Ảo tưởng năng lực.",
                        counterMeasureVi = "Gấp sách lại và tự viết ra giấy nháp các ý chính trước khi tự đánh giá bản thân đã hiểu bài."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "bias_spotter", 4),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 4,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Quá Tải Nhận Thức Khi Đa Nhiệm (Cognitive Overload)",
            Description = "Phân tích tác động phân tán của việc vừa học vừa nhắn tin đến Trí nhớ làm việc.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l21-bias-2",
                        title = "Vừa giải bài tập khó vừa lướt mạng xã hội",
                        contextTag = "Dung lượng Trí nhớ Làm việc (Working Memory)",
                        scenario = "Trang vừa viết code vừa mở 3 tab chat và nghe nhạc có lời. Sau 2 giờ, Trang cảm thấy kiệt sức, nhức đầu nhưng chỉ viết được 10 dòng code đầy lỗi cơ bản.",
                        canonicalNameVi = "Quá tải nhận thức / Phân mảnh chú ý (Cognitive Overload / Attention Residue)",
                        acceptedNames = new[] { "Quá tải nhận thức", "qua tai nhan thuc", "Cognitive Overload", "Phân mảnh chú ý", "Attention Residue", "Đa nhiệm", "Multitasking" },
                        reasonKeywords = new[] { "trí nhớ làm việc", "quá tải", "phân mảnh", "chú ý", "chuyển đổi" },
                        minReasonWords = 8,
                        sampleExplanation = "Trí nhớ làm việc chỉ chứa được 4±1 đơn vị thông tin; việc chuyển đổi liên tục giữa code và tin nhắn làm tiêu hao tài nguyên nhận thức và để lại cặn chú ý (Attention Residue).",
                        counterMeasureVi = "Áp dụng kỹ thuật Pomodoro 25 phút toàn tâm toàn ý, tắt toàn bộ thông báo và mạng xã hội."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "feynman", 5),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 5,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: Kỹ Thuật Gom Cụm Chunking Trong Trí Nhớ Làm Việc",
            Description = "Tự giải thích cách não bộ nhóm thông tin để vượt qua giới hạn dung lượng 4±1.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l21-feyn-1",
                        conceptTitle = "Gom cụm thông tin (Chunking Technique)",
                        promptVi = "Hãy tự giải thích cơ chế của Kỹ thuật Chunking và cho ví dụ cách nhớ 1 dãy số điện thoại hoặc mã thẻ ngân hàng.",
                        requiredKeywords = new[] { "chunking", "gom cụm|nhóm lại", "trí nhớ làm việc", "dung lượng", "ý nghĩa|liên kết" },
                        minWords = 16,
                        sampleAnswer = "Chunking là kỹ thuật nhóm nhiều mẩu thông tin đơn lẻ thành một khối có ý nghĩa lớn hơn. Vì trí nhớ ngắn hạn chỉ chứa được khoảng 4 cụm, việc chia số điện thoại 10 số thành 3 cụm (0912 - 345 - 678) giúp não bộ xử lý dễ dàng gấp nhiều lần.",
                        everydayExampleHintVi = "Ví dụ chia số 19451975 thành hai mốc năm lịch sử quen thuộc 1945 và 1975."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "feynman", 6),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 6,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: Ôn Tập Ngắt Quãng (Spaced Repetition & FSRS)",
            Description = "Tự giải thích đường cong quên lãng Ebbinghaus và nguyên lý thuật toán FSRS.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l21-feyn-2",
                        conceptTitle = "Ôn tập ngắt quãng (Spaced Repetition & FSRS)",
                        promptVi = "Hãy tự giải thích tại sao học 20 phút mỗi ngày trong 7 ngày lại nhớ lâu hơn học dồn 3 tiếng trong 1 đêm.",
                        requiredKeywords = new[] { "ngắt quãng|spaced repetition", "đường cong quên", "củng cố|synapse", "ngủ|phục hồi", "trí nhớ dài hạn" },
                        minWords = 16,
                        sampleAnswer = "Ôn tập ngắt quãng giãn cách thời gian giữa các lần gợi nhớ đúng vào lúc ký ức sắp phai mờ. Mỗi lần nỗ lực truy xuất lại thông tin sẽ củng cố liên kết synapse thần kinh mạnh hơn, giúp chuyển kiến thức vào trí nhớ dài hạn bền vững thay vì trôi tuột như học dồn thi xong quên sạch.",
                        everydayExampleHintVi = "Ví dụ ôn lại flashcard sau 1 ngày, 3 ngày, 7 ngày, 21 ngày."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "cbt_exercise", 7),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 7,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: Tự Chỉ Trích Khi Quên Kiến Thức Cũ",
            Description = "Tái cấu trúc niềm tin sai lầm rằng quên lãng là dấu hiệu của sự kém cỏi.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l21-cbt-1",
                        title = "Quên định nghĩa môn học vừa thi tuần trước",
                        scenario = "Sau kỳ thi 2 tuần, bạn được người khác hỏi về một thuật ngữ cốt lõi và bạn không nhớ chi tiết. Bạn cảm thấy xấu hổ và nghĩ đầu óc mình có vấn đề thoái hóa.",
                        sampleAutomaticThought = "Não mình như bã đậu, học đâu quên đó, học tiếp cũng vô ích.",
                        canonicalDistortionVi = "Dán nhãn / Trầm trọng hóa (Labeling / Catastrophizing)",
                        acceptedDistortions = new[] { "Dán nhãn", "Trầm trọng hóa", "Labeling", "Catastrophizing", "Khái quát hóa quá mức", "Overgeneralization" },
                        distortionChoicesHint = new[] { "Dán nhãn (Labeling)", "Trầm trọng hóa (Catastrophizing)", "Khái quát hóa quá mức (Overgeneralization)" },
                        balancedKeywords = new[] { "đường cong quên", "tự nhiên", "ôn tập", "não bộ", "không ôn thì quên" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Quên lãng là cơ chế sinh học hoàn toàn tự nhiên của não bộ để loại bỏ thông tin chưa được kích hoạt lặp lại. Điều này không có nghĩa là mình kém cỏi; chỉ cần ôn ngắt quãng vài lần bằng Flashcard FSRS là kiến thức sẽ được củng cố vững chắc.",
                        explanationVi = "Tự dán nhãn 'não như bã đậu' và phóng đại sự quên lãng tự nhiên thành bất tài là lỗi Dán nhãn (Labeling)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "cbt_exercise", 8),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 8,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: Lo Sợ Bị Tụt Hậu Khi Giảm Tải Công Việc",
            Description = "Hóa giải hội chứng FOMO nhận thức và áp lực phải học mọi thứ cùng lúc.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l21-cbt-2",
                        title = "Nhìn thấy người khác học 3 chứng chỉ cùng lúc trên mạng xã hội",
                        scenario = "Thấy bạn bè đăng chứng chỉ AWS, IELTS và CFA liên tục, bạn cảm thấy bồn chồn và tự ép mình đăng ký học cả 3 khóa cùng lúc dù đang làm toàn thời gian 45 tiếng/tuần.",
                        sampleAutomaticThought = "Mình đang bị tụt lại phía sau quá xa, nếu không học ngày đêm mình sẽ thất bại hoàn toàn trong sự nghiệp.",
                        canonicalDistortionVi = "Trầm trọng hóa / Phán đoán cảm tính (Catastrophizing / Emotional Reasoning)",
                        acceptedDistortions = new[] { "Trầm trọng hóa", "Phán đoán cảm tính", "Catastrophizing", "Emotional Reasoning", "Tư duy trắng đen", "All-or-Nothing" },
                        distortionChoicesHint = new[] { "Trầm trọng hóa (Catastrophizing)", "Phán đoán cảm tính (Emotional Reasoning)", "Lọc tiêu cực (Mental Filter)" },
                        balancedKeywords = new[] { "lộ trình riêng", "quá tải", "chất lượng", "từng bước", "bền vững" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Mỗi người có vạch xuất phát và lộ trình riêng; cố học dồn cả 3 chứng chỉ trong trạng thái quá tải chỉ dẫn đến kiệt sức và học hời hợt. Mình tập trung tinh gọn vào 1 kỹ năng cốt lõi nhất trong 3 tháng tới sẽ hiệu quả và bền vững hơn nhiều.",
                        explanationVi = "Cảm thấy lo sợ rồi suy ra mình 'chắc chắn sẽ thất bại' là biểu hiện của Phán đoán cảm tính (Emotional Reasoning)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "apply_journal", 9),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 9,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Lập Kế Hoạch Chunking Cho 1 Kiến Thức Phức Tạp",
            Description = "Chia nhỏ một module kỹ thuật hoặc tài chính thành 3–4 cụm độc lập để học.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l21-apply-1",
                        title = "Phân rã chủ đề khó thành các Chunk nhỏ dễ tiêu hóa",
                        promptVi = "Chọn 1 chủ đề bạn thấy khó nhất hiện tại. Viết kế hoạch chia nhỏ nó thành 3 khối Chunking cụ thể kèm từ khóa neo giữ.",
                        expectedKeywords = new[] { "chunking", "chia nhỏ", "từ khóa", "khái niệm", "thực hành" },
                        samplePlanVi = "Chủ đề: Lập trình bất đồng bộ C#. Chunk 1: Luồng xử lý đơn luồng vs đa luồng. Chunk 2: Cú pháp async/await và trạng thái Task. Chunk 3: Xử lý ngoại lệ Task.WhenAll. Mỗi chunk luyện 30 phút."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "apply_journal", 10),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 10,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Kỷ Luật Ôn Tập Flashcard FSRS Mỗi Ngày",
            Description = "Thiết lập mỏ neo thói quen để duy trì 5 phút ôn tập ngắt quãng đều đặn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l21-apply-2",
                        title = "Thiết kế thói quen gắn Flashcard FSRS vào hoạt động thường nhật",
                        promptVi = "Viết cam kết ôn tập FSRS theo công thức Thói quen nguyên tử (Sau khi [Hành động có sẵn], tôi sẽ mở Studxy ôn 10 thẻ Flashcard).",
                        expectedKeywords = new[] { "thói quen", "flashcard", "fsrs", "mỗi ngày", "cam kết" },
                        samplePlanVi = "Sau khi pha xong ly cà phê sáng lúc 8h, mình sẽ mở Studxy trên điện thoại và ôn toàn bộ 15 thẻ Flashcard FSRS đến hạn trước khi đọc tin nhắn công việc."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "apply_journal", 11),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 11,
            Type = "apply_journal",
            Title = "Tình Huống Khởi Động: Ứng Dụng Kỹ Thuật Chunking Ghi Nhớ Danh Sách Đi Siêu Thị",
            Description = "Gom nhóm thông tin rời rạc thành các cụm ý nghĩa để vượt qua giới hạn 4–7 đơn vị của trí nhớ ngắn hạn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l21-warmup",
                        title = "Gom nhóm danh sách mua sắm 9 món theo danh mục chức năng",
                        promptVi = "Giả sử bạn cần mua 9 món đồ: Táo, Kem đánh răng, Sữa chua, Xà phòng tắm, Thịt bò, Nước rửa chén, Chuối, Cá hồi, Bột giặt. Hãy áp dụng kỹ thuật Chunking để phân thành 3 cụm logic giúp bạn ghi nhớ hoàn hảo mà không cần nhìn sổ.",
                        expectedKeywords = new[] { "chunking", "gom nhóm", "thực phẩm", "tẩy rửa", "rau củ", "trí nhớ ngắn hạn" },
                        samplePlanVi = "Mình áp dụng kỹ thuật Chunking chia 9 món thành 3 cụm (mỗi cụm 3 món): Cụm 1 - Đồ tươi sống/Protein: Thịt bò, Cá hồi, Sữa chua. Cụm 2 - Trái cây tươi: Táo, Chuối. Cụm 3 - Hóa phẩm vệ sinh cá nhân & gia đình: Kem đánh răng, Xà phòng tắm, Nước rửa chén, Bột giặt. Bằng cách giảm từ 9 đơn vị rời rạc xuống còn 3 cụm danh mục, trí nhớ làm việc (Working Memory) dễ dàng lưu giữ và tái hiện đầy đủ."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // =========================================================================
        // LESSON 2.2: Phục Hồi Ký Ức, Quản Lý Năng Lượng & Phục Hồi Burnout
        // (Legacy activities: OrderIndex 1, 2)
        // Write-in activities: OrderIndex 3..10 (8 activities)
        // =========================================================================
        var l22 = SeedDataInitializer.PsychologyLesson22Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "bias_spotter", 3),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 3,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Chi Phí Chìm Trong Dự Án Gây Kiệt Sức",
            Description = "Nhận biết việc cố đấm ăn xôi vì tiếc công sức quá khứ làm cạn kiệt năng lượng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l22-bias-1",
                        title = "Cố ôm đồm dự án thất bại vì đã tốn 6 tháng công sức",
                        contextTag = "Ra quyết định & Phòng ngừa Burnout",
                        scenario = "Khánh đã bỏ ra 6 tháng và 80 triệu làm một ứng dụng nhưng dữ liệu thực tế cho thấy không ai dùng. Khánh kiệt quệ tinh thần nhưng vẫn vay thêm tiền để tiếp tục vì 'đã lỡ đâm lao thì phải theo lao'.",
                        canonicalNameVi = "Ảo tưởng chi phí chìm (Sunk Cost Fallacy)",
                        acceptedNames = new[] { "Ảo tưởng chi phí chìm", "chi phi chim", "Sunk Cost Fallacy", "Sunk Cost", "Thiên kiến chi phí chìm" },
                        reasonKeywords = new[] { "chi phí chìm", "đã mất", "tiếc nuối", "quá khứ", "tương lai" },
                        minReasonWords = 8,
                        sampleExplanation = "Thời gian 6 tháng và 80 triệu là chi phí chìm không thể lấy lại; tiếp tục đổ thêm sức khỏe và tiền bạc vào dự án không khả thi chỉ làm gia tăng tổn thất và dẫn đến Burnout.",
                        counterMeasureVi = "Tự hỏi: 'Nếu hôm nay mình chưa từng bỏ ra đồng nào, mình có đầu tư vào dự án này không?' Nếu không, hãy dũng cảm dừng lại."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "bias_spotter", 4),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 4,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Ảo Tưởng Lập Kế Hoạch Lạc Quan Quá Mức (Planning Fallacy)",
            Description = "Nhận diện xu hướng đánh giá thấp thời gian hoàn thành dẫn đến thức trắng đêm.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l22-bias-2",
                        title = "Nghĩ rằng bài tiểu luận 15 trang chỉ cần viết trong 1 buổi tối",
                        contextTag = "Tâm lý học Thời gian & Burnout",
                        scenario = "Hải luôn tin rằng mình chỉ cần 4 tiếng là viết xong báo cáo nghiên cứu. Đến hạn chót, Hải phải thức trắng đêm đến 5h sáng, nộp bài vội vàng và bị suy nhược cơ thể 2 ngày sau đó.",
                        canonicalNameVi = "Ảo tưởng lập kế hoạch (Planning Fallacy)",
                        acceptedNames = new[] { "Ảo tưởng lập kế hoạch", "ao tuong lap ke hoach", "Planning Fallacy", "Thiên kiến lập kế hoạch", "Lạc quan tếu" },
                        reasonKeywords = new[] { "lập kế hoạch", "đánh giá thấp", "thời gian", "chậm trễ", "thức đêm" },
                        minReasonWords = 8,
                        sampleExplanation = "Hải đánh giá thấp thời gian thực tế và bỏ qua các rủi ro phát sinh trong quá trình viết — biểu hiện của Ảo tưởng lập kế hoạch (Planning Fallacy).",
                        counterMeasureVi = "Ước lượng thời gian theo thực tế quá khứ và luôn nhân hệ số dự phòng 1.5x trước hạn chót."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "feynman", 5),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 5,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: Mạng Lưới Trạng Thái Mặc Định Của Não (Default Mode Network)",
            Description = "Tự giải thích vai trò của khoảng nghỉ không màn hình trong việc kết nối ý tưởng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l22-feyn-1",
                        conceptTitle = "Mạng lưới mặc định (Default Mode Network - DMN)",
                        promptVi = "Hãy tự giải thích vì sao những ý tưởng sáng tạo đột phá thường xuất hiện khi đang đi dạo hoặc tắm thay vì lúc ngồi gõ bàn phím.",
                        requiredKeywords = new[] { "default mode network|dmn", "trạng thái mặc định", "nghỉ ngơi|đi dạo", "liên kết|sáng tạo", "vô thức" },
                        minWords = 16,
                        sampleAnswer = "Khi não thôi tập trung cao độ vào một nhiệm vụ cụ thể, Mạng lưới mặc định (DMN) được kích hoạt. Ở chế độ này, não bộ âm thầm tổng hợp, kết nối các mẩu thông tin rời rạc trong vô thức, giúp nảy sinh những ý tưởng giải quyết bế tắc một cách tự nhiên.",
                        everydayExampleHintVi = "Ví dụ Archimedes phát hiện ra định luật lực đẩy khi thả lỏng ngâm mình trong bồn tắm."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "feynman", 6),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 6,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: Cạn Kiệt Năng Lượng Ý Chí (Ego Depletion & Quản Lý Nhịp)",
            Description = "Tự giải thích lý do tại sao càng về tối chúng ta càng dễ lướt mạng xã hội vô bổ.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l22-feyn-2",
                        conceptTitle = "Sự cạn kiệt ý chí (Ego Depletion & Energy Management)",
                        promptVi = "Hãy tự giải thích cơ chế suy giảm năng lượng tự kiểm soát trong ngày và cách khắc phục bằng việc ưu tiên công việc.",
                        requiredKeywords = new[] { "ý chí", "hạn chế|cạn kiệt", "buổi sáng", "năng lượng", "thói quen" },
                        minWords = 16,
                        sampleAnswer = "Năng lượng ý chí và khả năng tự chủ của con người có hạn như một thanh pin. Sau một ngày dài liên tục đưa ra quyết định, thanh pin cạn dần khiến ta khó cưỡng lại cám dỗ vào ban đêm. Vì vậy cần giải quyết việc khó nhất vào đầu ngày khi pin còn đầy.",
                        everydayExampleHintVi = "Ví dụ buổi sáng tập gym rất dễ nhưng tối về chỉ muốn nằm bấm điện thoại."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "cbt_exercise", 7),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 7,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: Cảm Giác Tội Lỗi Khi Đi Ngủ Sớm Hoặc Nghỉ Ngơi",
            Description = "Hóa giải niềm tin độc hại 'nghỉ ngơi là lười biếng' trong văn hóa hối hả (Hustle Culture).",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l22-cbt-1",
                        title = "Cảm thấy cắn rứt lương tâm khi dành tối Chủ Nhật để xem phim thư giãn",
                        scenario = "Sau một tuần làm việc 50 tiếng, bạn quyết định dành trọn buổi tối để nghỉ ngơi cùng gia đình. Một suy nghĩ lóe lên: 'Trong lúc mình ngồi đây xem phim thì người khác đang cày cuốc thăng tiến'.",
                        sampleAutomaticThought = "Mình đang lãng phí thời gian, mình lười biếng và sẽ bị người khác vượt mặt.",
                        canonicalDistortionVi = "Tuyên bố nên - phải / So sánh độc hại (Should Statements / Unfair Comparison)",
                        acceptedDistortions = new[] { "Tuyên bố nên - phải", "Nên - phải", "Should Statements", "So sánh bất công", "Trầm trọng hóa", "Catastrophizing" },
                        distortionChoicesHint = new[] { "Tuyên bố nên - phải (Should Statements)", "Trầm trọng hóa (Catastrophizing)", "Tư duy trắng - đen (All-or-Nothing)" },
                        balancedKeywords = new[] { "nghỉ ngơi", "tái tạo", "bền vững", "không phải lười", "nạp năng lượng" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Nghỉ ngơi có chủ đích không phải lười biếng mà là bước bắt buộc để nạp lại pin sinh học và phòng tránh kiệt sức. Không ai có thể chạy nước rút trên chặng đường marathon 30 năm sự nghiệp.",
                        explanationVi = "Áp đặt những mệnh lệnh cứng nhắc 'lúc nào mình cũng phải cày cuốc' là lỗi 'Tuyên bố nên - phải' (Should Statements)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "cbt_exercise", 8),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 8,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: Phóng Đại Hậu Quả Của Việc Trễ Deadline Nhẹ",
            Description = "Hạ nhiệt nỗi sợ hãi thảm họa hóa khi cần xin gia hạn tiến độ công việc.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l22-cbt-2",
                        title = "Phải xin dời lịch nộp báo cáo thêm 1 ngày vì bị ốm",
                        scenario = "Bạn bị sốt xuất huyết và không thể hoàn thành slide đúng hạn thứ Năm. Bạn sợ hãi không dám nhắn tin cho sếp vì nghĩ sếp sẽ lập tức sa thải hoặc gạch tên bạn khỏi dự án.",
                        sampleAutomaticThought = "Nếu mình xin hoãn, sếp sẽ nghĩ mình bất tài và đuổi việc mình ngay lập tức.",
                        canonicalDistortionVi = "Trầm trọng hóa / Đọc suy nghĩ (Catastrophizing / Mind Reading)",
                        acceptedDistortions = new[] { "Trầm trọng hóa", "Catastrophizing", "Đọc suy nghĩ", "Mind Reading", "Thảm họa hóa" },
                        distortionChoicesHint = new[] { "Trầm trọng hóa (Catastrophizing)", "Đọc suy nghĩ (Mind Reading)", "Tư duy trắng - đen (All-or-Nothing)" },
                        balancedKeywords = new[] { "sức khỏe", "thông báo sớm", "lịch sự", "không bị đuổi", "hợp lý" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Bị ốm là sự cố bất khả kháng; nếu mình chủ động thông báo sớm và kèm theo bản nháp tiến độ đã làm, cấp trên sẽ thông cảm và sắp xếp hỗ trợ. Không người quản lý chuyên nghiệp nào sa thải nhân sự vì bị ốm xin dời 1 ngày.",
                        explanationVi = "Tưởng tượng kịch bản tồi tệ nhất bị đuổi việc khi chỉ xin dời lịch vì bệnh là lỗi Trầm trọng hóa (Catastrophizing)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "apply_journal", 9),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 9,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Thiết Kế Khoảng Nghỉ Không Kỹ Thuật Số (Digital Sunset)",
            Description = "Lên lịch trình tắt màn hình 45 phút trước khi đi ngủ để bảo vệ giấc ngủ sâu.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l22-apply-1",
                        title = "Kế hoạch tắt điện thoại lúc 22h30 và thay thế bằng đọc sách giấy",
                        promptVi = "Mô tả cụ thể quy trình ngắt kết nối thiết bị điện tử buổi tối của bạn để phục hồi chất lượng giấc ngủ REM.",
                        expectedKeywords = new[] { "ngắt kết nối", "điện thoại", "giấc ngủ", "đọc sách", "phục hồi" },
                        samplePlanVi = "Đúng 22h30, mình để điện thoại ở chế độ máy bay và cất ngoài phòng khách. Mình thay bằng 20 phút đọc sách giấy và làm bài giãn cơ nhẹ trước khi ngủ lúc 23h."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "apply_journal", 10),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 10,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Đàm Phán Giảm Tải Công Việc Khi Có Dấu Hiệu Tiền Burnout",
            Description = "Soạn thảo lời đề xuất theo 4 bước NVC gửi cấp trên để tái phân bổ khối lượng công việc.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l22-apply-2",
                        title = "Soạn thảo thông điệp NVC trao đổi khối lượng công việc với quản lý",
                        promptVi = "Hãy viết thông điệp giao tiếp phi bạo lực (Quan sát, Cảm xúc, Nhu cầu, Đề nghị) để xin tái phân bổ 1 đầu việc phụ.",
                        expectedKeywords = new[] { "quan sát", "cảm xúc", "nhu cầu", "đề nghị", "nvc" },
                        samplePlanVi = "Anh/chị ơi, tuần này em đang phụ trách 3 dự án đồng thời với 12 đầu việc (quan sát), em cảm thấy quá tải và lo lắng chất lượng sản phẩm cuối cùng bị ảnh hưởng (cảm xúc), em muốn tập trung hoàn thiện dự án A tốt nhất (nhu cầu). Anh/chị có thể bàn giao phần báo cáo B cho bạn Hùng hỗ trợ tuần này được không (đề nghị)?"
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "apply_journal", 11),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 11,
            Type = "apply_journal",
            Title = "Tình Huống Khởi Động: Lập Lịch Ôn Tập Giãn Cách (FSRS) Cho Một Môn Học Cụ Thể",
            Description = "Lên kế hoạch ôn tập ngắt quãng khoa học để đánh bại đường cong quên lãng Ebbinghaus mà không bị quá tải.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l22-warmup",
                        title = "Kế hoạch Spaced Repetition kết hợp Active Recall trong 30 ngày",
                        promptVi = "Chọn 1 chủ đề hoặc môn học bạn đang theo dõi. Hãy thiết kế lịch ôn tập giãn cách theo các mốc thời gian (1 ngày, 3 ngày, 7 ngày, 21 ngày) và phương pháp tự kiểm tra chủ động thay vì đọc lại thụ động.",
                        expectedKeywords = new[] { "spaced repetition", "active recall", "ebbinghaus", "ôn tập", "flashcard", "đường cong quên" },
                        samplePlanVi = "Chủ đề: 30 thuật ngữ Tâm lý học nhận thức. Lịch giãn cách: Mốc 1 (Sau 24 giờ): Tự kiểm tra bằng flashcard giấy; Mốc 2 (Sau 3 ngày): Tự giải thích lại các khái niệm theo phương pháp Feynman; Mốc 3 (Sau 7 ngày): Làm bài quiz trắc nghiệm tổng hợp; Mốc 4 (Sau 21 ngày): Viết một case study ứng dụng thực tế. Không đọc lại tài liệu thụ động mà luôn tự truy hồi (Active Retrieval) trước khi lật đáp án."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // =========================================================================
        // LESSON 3.1: Tái Cấu Trúc Nhận Thức CBT & Thiết Lập Ranh Giới Cá Nhân
        // (Legacy activities: OrderIndex 1, 2)
        // Write-in activities: OrderIndex 3..10 (8 activities)
        // =========================================================================
        var l31 = SeedDataInitializer.PsychologyLesson31Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "bias_spotter", 3),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 3,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Cá Nhân Hóa Lỗi Lầm Của Người Khác (Personalization)",
            Description = "Nhận diện việc nhận vơ trách nhiệm cho những biến cố nằm ngoài tầm kiểm soát.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l31-bias-1",
                        title = "Sếp bước vào văn phòng với khuôn mặt cau có",
                        contextTag = "Liệu pháp Nhận thức Hành vi (CBT)",
                        scenario = "Sếp bước vào phòng họp với vẻ mặt bực bội và nói trống không. Linh lập tức toát mồ hôi, tim đập thình thịch và đinh ninh rằng sếp đang giận mình vì email báo cáo chiều hôm qua.",
                        canonicalNameVi = "Cá nhân hóa (Personalization)",
                        acceptedNames = new[] { "Cá nhân hóa", "ca nhan hoa", "Personalization", "Nhận vơ", "Đọc suy nghĩ", "Mind Reading" },
                        reasonKeywords = new[] { "cá nhân hóa", "nhận về mình", "bằng chứng", "tâm trạng", "nguyên nhân khác" },
                        minReasonWords = 8,
                        sampleExplanation = "Linh tự động quy chụp tâm trạng tiêu cực của người khác là do lỗi của bản thân mà không có bất kỳ bằng chứng xác thực nào — đây là lỗi Cá nhân hóa (Personalization).",
                        counterMeasureVi = "Liệt kê ít nhất 3 nguyên nhân bên ngoài có thể khiến sếp bực bội (kẹt xe, cãi nhau ở nhà, áp lực chỉ tiêu tài chính)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "bias_spotter", 4),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 4,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Ảo Tưởng Về Sự Minh Bạch (Illusion of Transparency)",
            Description = "Phân tích xu hướng nghĩ rằng người khác đọc thấu nỗi sợ thầm kín của mình.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l31-bias-2",
                        title = "Tưởng cả hội trường đều nhìn thấy tim mình đang đập mạnh",
                        contextTag = "Tâm lý học Xã hội & Thuyết trình",
                        scenario = "Khi bước lên bục thuyết trình, Tuấn cảm thấy tay hơi run và tim đập nhanh. Tuấn nghĩ rằng 200 khán giả bên dưới đều đang nhìn thấy rõ sự sợ hãi và chê cười mình yếu bóng vía.",
                        canonicalNameVi = "Ảo tưởng về sự minh bạch (Illusion of Transparency)",
                        acceptedNames = new[] { "Ảo tưởng về sự minh bạch", "ao tuong minh bach", "Illusion of Transparency", "Hiệu ứng tâm điểm", "Spotlight Effect" },
                        reasonKeywords = new[] { "minh bạch", "tâm điểm", "khán giả", "phóng đại", "nhận thức bên trong" },
                        minReasonWords = 8,
                        sampleExplanation = "Tuấn phóng đại khả năng người khác nhận ra trạng thái cảm xúc bên trong của mình. Trên thực tế, khán giả chỉ nghe nội dung và không hề chú ý đến nhịp tim hay sự hồi hộp bên trong.",
                        counterMeasureVi = "Nhắc nhở bản thân rằng cảm xúc nội tâm là vô hình đối với người ngoài; tập trung 100% vào giá trị trao đi cho khán giả."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "feynman", 5),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 5,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: Mô Hình Nhận Thức ABC Của Albert Ellis",
            Description = "Tự giải thích cơ chế: không phải sự kiện (A) mà chính niềm tin (B) tạo nên cảm xúc (C).",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l31-feyn-1",
                        conceptTitle = "Mô hình ABC trong CBT (Activating Event - Belief - Consequence)",
                        promptVi = "Hãy tự giải thích tại sao cùng 1 sự kiện trời mưa (A) lại khiến một người thấy buồn bực nhưng người khác lại thấy lãng mạn, thư thái.",
                        requiredKeywords = new[] { "abc", "sự kiện|kích hoạt", "niềm tin|suy nghĩ", "hệ quả|cảm xúc", "góc nhìn" },
                        minWords = 16,
                        sampleAnswer = "Mô hình ABC chỉ ra rằng sự kiện khách quan A không trực tiếp gây ra cảm xúc C; chính hệ thống niềm tin và cách ta diễn giải B mới là nguyên nhân. Cùng trời mưa, người nghĩ 'bẩn xe, trễ giờ' sẽ tức giận; người nghĩ 'mát mẻ, dễ ngủ' sẽ thư thái. Thay đổi niềm tin B sẽ thay đổi toàn bộ cảm xúc C.",
                        everydayExampleHintVi = "Ví dụ bị kẹt xe: nếu nghĩ 'lãng phí đời' thì phát cáu, nếu nghĩ 'dịp nghe podcast' thì vui vẻ."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "feynman", 6),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 6,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: Ranh Giới Cá Nhân Lành Mạnh (Boundaries)",
            Description = "Tự định nghĩa ranh giới tâm lý không phải sự ích kỷ mà là sự bảo vệ năng lượng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l31-feyn-2",
                        conceptTitle = "Ranh giới cá nhân (Healthy Boundaries)",
                        promptVi = "Hãy tự giải thích vì sao nói 'Không' với điều vượt quá năng lượng chính là nói 'Có' với bản thân.",
                        requiredKeywords = new[] { "ranh giới", "từ chối|nói không", "năng lượng", "tôn trọng", "bản thân" },
                        minWords = 16,
                        sampleAnswer = "Ranh giới cá nhân là tấm màng bảo vệ sức khỏe tinh thần và thời gian của bạn. Thiết lập ranh giới rõ ràng giúp người khác biết điểm dừng và biết cách tôn trọng bạn; từ chối một lời nhờ vả vượt quá sức là cách duy nhất để bảo toàn năng lượng cho những ưu tiên thực sự có giá trị.",
                        everydayExampleHintVi = "Ví dụ từ chối đi nhậu khi cần ôn thi hoặc từ chối trả lời tin nhắn công việc lúc 23h đêm."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "cbt_exercise", 7),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 7,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: Nỗi Sợ Bị Người Khác Đánh Giá Khi Nói Lời Từ Chối",
            Description = "Tái cấu trúc suy nghĩ tự động của người có tính cả nể (People Pleaser).",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l31-cbt-1",
                        title = "Bạn bè nhờ vay tiền lần thứ ba khi khoản cũ chưa trả",
                        scenario = "Một người quen nhắn tin hỏi mượn bạn 10 triệu đồng. Bạn đang cần số tiền đó để đóng tiền nhà. Bạn rất muốn từ chối nhưng sợ họ ghét và nói xấu mình với cả nhóm bạn chung.",
                        sampleAutomaticThought = "Nếu mình từ chối, họ sẽ coi mình là kẻ keo kiệt và tình bạn này sẽ chấm dứt vĩnh viễn.",
                        canonicalDistortionVi = "Đọc suy nghĩ / Trầm trọng hóa (Mind Reading / Catastrophizing)",
                        acceptedDistortions = new[] { "Đọc suy nghĩ", "Trầm trọng hóa", "Mind Reading", "Catastrophizing", "Cả nể" },
                        distortionChoicesHint = new[] { "Đọc suy nghĩ (Mind Reading)", "Trầm trọng hóa (Catastrophizing)", "Tư duy trắng - đen (All-or-Nothing)" },
                        balancedKeywords = new[] { "tiền nhà", "ranh giới", "tình bạn chân chính", "từ chối lịch sự", "tôn trọng" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Mình có trách nhiệm tài chính ưu tiên tiền nhà của bản thân; từ chối một cách lịch sự, rõ ràng là quyền chính đáng. Một tình bạn chân chính sẽ tôn trọng hoàn cảnh của nhau chứ không phụ thuộc vào việc có cho vay tiền hay không.",
                        explanationVi = "Tự suy diễn rằng người khác chắc chắn sẽ thù ghét mình là lỗi Đọc suy nghĩ (Mind Reading)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "cbt_exercise", 8),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 8,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: Hội Chứng Kẻ Giả Mạo Nơi Công Sở (Imposter Syndrome)",
            Description = "Vượt qua cảm giác bản thân là kẻ lừa dối dù đạt được thành tích thật.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l31-cbt-2",
                        title = "Vừa được thăng chức lên vị trí nhóm trưởng (Team Lead)",
                        scenario = "Sau 2 năm hoàn thành xuất sắc mọi dự án, bạn được bổ nhiệm làm Trưởng nhóm. Thay vì vui mừng, bạn lại run sợ nghĩ rằng mình chỉ gặp may và sớm muộn mọi người sẽ phát hiện ra bạn chẳng biết gì.",
                        sampleAutomaticThought = "Mình chỉ ăn may thôi, mình không đủ trình độ làm lead và sẽ làm hỏng mọi việc.",
                        canonicalDistortionVi = "Hạ thấp thành tích / Phán đoán cảm tính (Discounting the Positive / Emotional Reasoning)",
                        acceptedDistortions = new[] { "Hạ thấp thành tích", "Gạt bỏ tích cực", "Discounting the Positive", "Phán đoán cảm tính", "Emotional Reasoning", "Hội chứng kẻ giả mạo", "Imposter Syndrome" },
                        distortionChoicesHint = new[] { "Hạ thấp thành tích (Discounting the Positive)", "Phán đoán cảm tính (Emotional Reasoning)", "Đọc suy nghĩ (Mind Reading)" },
                        balancedKeywords = new[] { "nỗ lực", "bằng chứng", "2 năm", "được tin tưởng", "học hỏi" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Việc được thăng chức dựa trên kết quả cụ thể trong 2 năm làm việc chăm chỉ của mình, không thể quy chụp là ăn may. Chưa biết hết mọi thứ là điều bình thường của người mới nhận chức; mình hoàn toàn có năng lực học hỏi để hoàn thành tốt vai trò.",
                        explanationVi = "Quy chụp mọi thành tích thực tế thành 'do may mắn' là lỗi Hạ thấp thành tích (Discounting the Positive)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "apply_journal", 9),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 9,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Luyện Câu Từ Chối Lịch Thiệp Mà Dứt Khoát",
            Description = "Viết kịch bản từ chối một lời mời họp ngoài giờ không liên quan đến chuyên môn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l31-apply-1",
                        title = "Soạn thảo kịch bản từ chối lời mời họp ngoài giờ",
                        promptVi = "Hãy viết 1 câu từ chối lịch thiệp nhưng dứt khoát không để ngỏ hiểu nhầm khi được rủ vào một dự án phụ.",
                        expectedKeywords = new[] { "cảm ơn", "từ chối", "tập trung", "ưu tiên", "chúc may mắn" },
                        samplePlanVi = "Cảm ơn bạn đã tin tưởng và nghĩ đến mình. Hiện tại mình đang dành toàn bộ thời gian tập trung cho dự án cốt lõi nên không thể nhận lời tham gia dự án này được. Chúc dự án của bạn gặt hái nhiều thành công!"
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "apply_journal", 10),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 10,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Nhật Ký Tái Cấu Trúc Nhận Thức 4 Cột Hàng Ngày",
            Description = "Thực hành ghi nhận 1 sự kiện khó chịu trong ngày và viết lại góc nhìn thực tế.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l31-apply-2",
                        title = "Nhật ký CBT chuyển hóa cơn giận thành sự thấu cảm",
                        promptVi = "Ghi lại 1 tình huống khiến bạn khó chịu hôm nay theo 4 bước: Sự kiện thật - Suy nghĩ tự động - Lỗi nhận thức - Suy nghĩ cân bằng.",
                        expectedKeywords = new[] { "sự kiện", "suy nghĩ tự động", "lỗi nhận thức", "suy nghĩ cân bằng", "cảm xúc" },
                        samplePlanVi = "Sự kiện: Bạn cùng phòng quên rửa bát. Suy nghĩ tự động: 'Nó coi mình là người ở'. Lỗi nhận thức: Đọc suy nghĩ & Trầm trọng hóa. Suy nghĩ cân bằng: 'Bạn ấy vừa ôn thi sấp mặt cả đêm; mình sẽ nhắc bạn một cách vui vẻ trước giờ cơm tối.'"
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "cbt_exercise", 11),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 11,
            Type = "cbt_exercise",
            Title = "Tình Huống Khởi Động: Vượt Qua Trì Hoãn Bằng 'Quy Tắc 5 Phút' & Tái Cấu Trúc Nhận Thức",
            Description = "Phá vỡ suy nghĩ phóng đại khó khăn và kích hoạt hành động nhỏ đầu tiên theo nguyên lý CBT.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l31-warmup",
                        title = "Chiến thắng cảm giác ngại ngùng khi bắt đầu viết báo cáo lớn",
                        scenario = "Trước nhiệm vụ viết báo cáo học tập 10 trang, bạn cảm thấy ngột ngạt, đầu óc nghĩ: 'Nhiệm vụ này quá khổng lồ, mình không thể làm nổi một cách hoàn hảo hôm nay, thôi lướt mạng một lát rồi tối tính'.",
                        sampleAutomaticThought = "Báo cáo này quá khó và dài, mình không có đủ khả năng hoàn thành nó hoàn hảo lúc này nên để mai làm.",
                        canonicalDistortionVi = "Trầm trọng hóa / Tư duy tất cả hoặc không (Catastrophizing / All-or-Nothing)",
                        acceptedDistortions = new[] { "Trầm trọng hóa", "Tư duy tất cả hoặc không", "Né tránh", "Catastrophizing", "All-or-Nothing", "Tư duy trắng đen" },
                        distortionChoicesHint = new[] { "Trầm trọng hóa (Catastrophizing)", "Tư duy tất cả hoặc không (All-or-Nothing)", "Dán nhãn (Labeling)" },
                        balancedKeywords = new[] { "quy tắc 5 phút", "bước nhỏ", "từng phần", "bắt đầu", "hoàn thành hơn hoàn hảo" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Không ai bắt mình phải hoàn thành cả 10 trang ngay lúc này. Mình áp dụng quy tắc 5 phút: chỉ cần mở file, gõ tiêu đề và 3 gạch đầu dòng dàn ý trong đúng 5 phút. Hoàn thành từng bước nhỏ tốt hơn là chờ đợi sự hoàn hảo.",
                        explanationVi = "Bộ não trì hoãn do sợ thất bại và phóng đại khối lượng công việc (Trầm trọng hóa). Hạ thấp rào cản hành động bằng 'Quy tắc 5 phút' giúp kích hoạt dopamine và xóa tan sức ỳ tâm lý."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // =========================================================================
        // LESSON 3.2: Tự Trắc Ẩn, Chánh Niệm & Đối Thoại Nội Tâm (Inner Critic)
        // (Legacy activities: OrderIndex 1, 2)
        // Write-in activities: OrderIndex 3..10 (8 activities)
        // =========================================================================
        var l32 = SeedDataInitializer.PsychologyLesson32Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "bias_spotter", 3),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 3,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Tiếng Nói Tự Phê Phán Độc Hại (Inner Critic Harshness)",
            Description = "Phân biệt giữa sự cầu tiến lành mạnh và sự sỉ nhục bản thân.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l32-bias-1",
                        title = "Nói với bản thân những lời mà bạn không bao giờ dám nói với bạn thân",
                        contextTag = "Tâm lý học Tự trắc ẩn (Self-Compassion)",
                        scenario = "Sau khi làm rơi vỡ một chiếc cốc thủy tinh, Huy lầm bầm: 'Mày đúng là đồ vô dụng hậu đậu, làm cái gì cũng hỏng bét, chết đi cho rồi'.",
                        canonicalNameVi = "Tiếng nói tự chỉ trích độc hại (Harsh Inner Critic / Self-Criticism)",
                        acceptedNames = new[] { "Tiếng nói tự chỉ trích", "Tự chỉ trích", "Inner Critic", "Self-Criticism", "Dán nhãn", "Labeling" },
                        reasonKeywords = new[] { "tự chỉ trích", "sỉ nhục", "lỗi nhỏ", "độc hại", "trầm trọng hóa" },
                        minReasonWords = 8,
                        sampleExplanation = "Huy trút sự giận dữ tàn nhẫn lên chính mình chỉ vì một tai nạn vô ý nhỏ — biểu hiện của Tiếng nói tự chỉ trích độc hại (Inner Critic).",
                        counterMeasureVi = "Tự hỏi: 'Nếu bạn thân làm vỡ cốc, mình sẽ nói gì với họ?' Hãy dùng chính sự tử tế đó để nói với bản thân."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "bias_spotter", 4),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 4,
            Type = "bias_spotter",
            Title = "Nhận Diện Thiên Kiến: Cô Lập Hóa Nỗi Đau (Isolation Fallacy)",
            Description = "Nhận diện ý nghĩ rằng 'chỉ có duy nhất một mình mình đau khổ và thất bại'.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l32-bias-2",
                        title = "Tin rằng cả thế giới đều thành công rực rỡ trừ mình",
                        contextTag = "Nhân tính chung (Common Humanity)",
                        scenario = "Khi thi trượt chứng chỉ chuyên môn, Nga cảm thấy cô đơn tột cùng và nghĩ: 'Tất cả mọi người đều giỏi giang, chỉ có duy nhất mình là đứa thất bại đáng thương hại'.",
                        canonicalNameVi = "Ảo tưởng cô lập hóa (Isolation Fallacy / Feeling of Isolation)",
                        acceptedNames = new[] { "Ảo tưởng cô lập", "Isolation Fallacy", "Cô lập hóa", "Thiếu nhân tính chung", "Isolation" },
                        reasonKeywords = new[] { "cô lập", "duy nhất", "thất bại", "nhân tính chung", "ai cũng có lúc" },
                        minReasonWords = 8,
                        sampleExplanation = "Nga rơi vào cái bẫy cô lập nỗi đau, quên rằng vấp ngã và thất bại là trải nghiệm phổ quát không thể tránh khỏi của toàn thể nhân loại.",
                        counterMeasureVi = "Nhắc nhở bản thân về nguyên lý Nhân tính chung: thất bại là một phần tất yếu của quá trình học hỏi của con người."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "feynman", 5),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 5,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: 3 Trụ Cột Tự Trắc Ẩn (Kristin Neff)",
            Description = "Tự giải thích Chánh niệm, Nhân tính chung và Tử tế với bản thân.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l32-feyn-1",
                        conceptTitle = "3 Trụ cột Tự trắc ẩn (Mindfulness - Common Humanity - Self-Kindness)",
                        promptVi = "Hãy tự giải thích 3 thành tố của lòng trắc ẩn với chính mình theo nghiên cứu của Tiến sĩ Kristin Neff.",
                        requiredKeywords = new[] { "tự trắc ẩn", "chánh niệm|mindfulness", "nhân tính chung|common humanity", "tử tế|self-kindness", "nỗi đau|thất bại" },
                        minWords = 18,
                        sampleAnswer = "Tự trắc ẩn gồm 3 trụ cột: 1. Chánh niệm (nhìn nhận nỗi đau đúng như nó vốn có, không kìm nén cũng không phóng đại); 2. Nhân tính chung (nhận ra khó khăn là trải nghiệm chia sẻ của con người, không cảm thấy cô độc); 3. Sự tử tế (đối xử dịu dàng, bao dung và nâng đỡ chính mình thay vì tự sỉ nhục).",
                        everydayExampleHintVi = "Ví dụ khi thi trượt: thừa nhận mình buồn, biết rằng ai cũng từng trượt, và tự nấu một bữa ăn ngon để động viên."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "feynman", 6),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 6,
            Type = "feynman",
            Title = "Feynman Tự Định Nghĩa: Chánh Niệm Trong Cuộc Sống Đời Thường (Mindfulness)",
            Description = "Tự giải thích Chánh niệm không phải điều thần bí mà là sự có mặt trọn vẹn trong hiện tại.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l32-feyn-2",
                        conceptTitle = "Chánh niệm (Mindfulness & Non-judgmental Awareness)",
                        promptVi = "Hãy tự giải thích Chánh niệm là gì cho một người chưa từng thực hành thiền hiểu được.",
                        requiredKeywords = new[] { "chánh niệm|mindfulness", "hiện tại|khoảnh khắc này", "quan sát", "không phán xét", "tập trung" },
                        minWords = 16,
                        sampleAnswer = "Chánh niệm là khả năng chú ý có chủ đích vào khoảnh khắc hiện tại với thái độ tò mò, cởi mở và không phán xét. Khi rửa bát chỉ biết đang rửa bát, cảm nhận làn nước mát thay vì vừa rửa vừa bực bội chuyện hôm qua hay lo lắng chuyện ngày mai.",
                        everydayExampleHintVi = "Ví dụ uống một ngụm trà và cảm nhận trọn vẹn hương vị ấm áp nơi đầu lưỡi."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "cbt_exercise", 7),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 7,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: Chuyển Hóa Tiếng Nói Tự Phán Xét Sau Lỗi Lầm",
            Description = "Thay thế những câu sỉ nhục bản thân bằng lời thoại của một người bạn thông thái.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l32-cbt-1",
                        title = "Gửi nhầm email chưa đính kèm file cho đối tác quan trọng",
                        scenario = "Vừa bấm nút Gửi email cho khách hàng VIP, bạn nhận ra mình quên đính kèm tệp báo giá. Bạn lập tức thấy hoảng loạn và nghĩ mình là kẻ đại ngốc không xứng đáng đi làm.",
                        sampleAutomaticThought = "Mình là đứa bất cẩn, ngớ ngẩn nhất quả đất, đối tác chắc chắn sẽ hủy hợp đồng.",
                        canonicalDistortionVi = "Dán nhãn / Trầm trọng hóa (Labeling / Catastrophizing)",
                        acceptedDistortions = new[] { "Dán nhãn", "Trầm trọng hóa", "Labeling", "Catastrophizing", "Tự trắc ẩn" },
                        distortionChoicesHint = new[] { "Dán nhãn (Labeling)", "Trầm trọng hóa (Catastrophizing)", "Tư duy trắng - đen (All-or-Nothing)" },
                        balancedKeywords = new[] { "sai sót", "bình tĩnh", "gửi lại", "chuyên nghiệp", "con người" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Quên đính kèm file là sơ suất phổ biến mà bất kỳ ai làm văn phòng cũng từng gặp ít nhất một lần. Mình chỉ cần bình tĩnh gửi lại email kèm file và lời xin lỗi ngắn gọn, lịch thiệp là xong.",
                        explanationVi = "Tự gọi mình là 'kẻ đại ngốc' và lo sợ bị hủy hợp đồng vì 1 file quên đính kèm là lỗi Dán nhãn kết hợp Trầm trọng hóa."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "cbt_exercise", 8),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 8,
            Type = "cbt_exercise",
            Title = "Bài Tập CBT Tình Huống: So Sánh Cơ Thể Và Ngoại Hình Với Ảnh Chỉnh Sửa",
            Description = "Chấm dứt việc tự trừng phạt bản thân trước những tiêu chuẩn phi thực tế.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l32-cbt-2",
                        title = "Lướt mạng xã hội thấy ai cũng eo thon, da đẹp không tì vết",
                        scenario = "Nhìn thấy các bức ảnh lung linh của người nổi tiếng trên Instagram, bạn nhìn vào gương và thấy ghét bỏ cơ thể mình, nghĩ rằng mình xấu xí và không đáng được yêu thương.",
                        sampleAutomaticThought = "Mình thật thảm hại và xấu xí, người như mình sẽ chẳng bao giờ tìm được hạnh phúc.",
                        canonicalDistortionVi = "So sánh bất công / Cá nhân hóa (Unfair Comparison / Personalization)",
                        acceptedDistortions = new[] { "So sánh bất công", "Unfair Comparison", "Cá nhân hóa", "Dán nhãn", "Labeling", "Phán đoán cảm tính" },
                        distortionChoicesHint = new[] { "So sánh bất công (Unfair Comparison)", "Dán nhãn (Labeling)", "Phán đoán cảm tính (Emotional Reasoning)" },
                        balancedKeywords = new[] { "góc chụp", "chỉnh sửa", "khỏe mạnh", "biết ơn cơ thể", "thực tế" },
                        minBalancedWords = 12,
                        sampleBalancedThought = "Hình ảnh trên mạng xã hội đã qua chỉnh sửa bộ lọc, ánh sáng và góc chụp chuyên nghiệp. Cơ thể mình đang phục vụ tốt cho sức khỏe và cuộc sống mỗi ngày; giá trị của mình nằm ở nhân cách và trí tuệ chứ không phải số đo trên ảnh ảo.",
                        explanationVi = "So sánh thực tế hậu trường của mình với những khoảnh khắc đã chọn lọc chỉnh sửa của người khác là lỗi So sánh bất công (Unfair Comparison)."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "apply_journal", 9),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 9,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Thực Hành Nghỉ Ngơi Tự Trắc Ẩn (Self-Compassion Break)",
            Description = "Viết bức thư ngắn gửi cho chính mình bằng giọng điệu của một người bạn thấu cảm.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l32-apply-1",
                        title = "Bức thư tự trắc ẩn 3 bước khi gặp một chuyện buồn trong tuần",
                        promptVi = "Hãy viết 3 câu tự nhủ với bản thân theo 3 bước: 1. Thừa nhận khoảnh khắc đau khổ; 2. Nhớ rằng đau khổ là một phần của kiếp người; 3. Cầu chúc cho bản thân sự bình an và bao dung.",
                        expectedKeywords = new[] { "thừa nhận", "đau khổ", "con người", "bình an", "bao dung" },
                        samplePlanVi = "1. Mình biết lúc này lòng mình đang rất đau và hụt hẫng. 2. Khó khăn này là điều mà bất kỳ ai trên đường trưởng thành cũng đều phải trải qua, mình không đơn độc. 3. Cầu chúc cho mình được bình an, mạnh mẽ và đối xử thật dịu dàng với chính mình lúc này."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "apply_journal", 10),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 10,
            Type = "apply_journal",
            Title = "Nhật Ký Áp Dụng: Bài Tập Chánh Niệm 3 Phút (3-Minute Breathing Space)",
            Description = "Thiết kế khoảng dừng chánh niệm giữa các giờ làm việc căng thẳng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l32-apply-2",
                        title = "Quy trình dừng lại và hít thở có ý thức 3 phút giữa giờ làm việc",
                        promptVi = "Mô tả cụ thể 3 phút dừng chân của bạn: Phút 1 (Nhận biết cơ thể), Phút 2 (Tập trung hơi thở), Phút 3 (Mở rộng trường chú ý).",
                        expectedKeywords = new[] { "3 phút", "hơi thở", "nhận biết", "thả lỏng", "chánh niệm" },
                        samplePlanVi = "Mỗi khi đổi ca làm việc lúc 15h, mình sẽ dừng gõ phím 3 phút: Phút 1 nhận biết các vùng căng cứng ở vai cổ; Phút 2 tập trung theo dõi luồng khí đi vào và đi ra nơi cánh mũi; Phút 3 mở rộng nhận thức toàn thân và mỉm cười nhẹ nhàng trước khi tiếp tục công việc."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "apply_journal", 11),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 11,
            Type = "apply_journal",
            Title = "Tình Huống Khởi Động: Luyện Nói Lời Từ Chối Lịch Thiệp Mà Dứt Khoát & Tự Trắc Ẩn",
            Description = "Thực hành thiết lập ranh giới cá nhân khi đang quá tải và bao dung với chính mình mà không áy náy.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "psy-l32-warmup",
                        title = "Từ chối lời nhờ vả làm việc thêm giờ khi đã kiệt sức",
                        promptVi = "Một đồng nghiệp nhờ bạn nhận làm thêm một bản khảo sát gấp vào tối nay trong khi bạn đang bị đau đầu và kiệt sức sau tuần làm việc căng thẳng. Hãy viết câu từ chối theo nguyên tắc ranh giới lành mạnh và lời tự nhủ trắc ẩn với bản thân.",
                        expectedKeywords = new[] { "từ chối", "ranh giới", "sức khỏe", "tự trắc ẩn", "lịch thiệp", "dứt khoát" },
                        samplePlanVi = "Lời từ chối gửi đồng nghiệp: 'Cảm ơn bạn đã tin tưởng, nhưng tối nay mình đang bị quá tải sức khỏe và có lịch nghỉ ngơi cố định nên không thể hỗ trợ bản khảo sát này được. Chúc bạn sớm hoàn thành nhé!' Lời tự trắc ẩn với chính mình: 'Việc từ chối không có nghĩa là mình ích kỷ hay kém cỏi. Lắng nghe nhu cầu cơ thể và đặt ranh giới lành mạnh là điều cần thiết để mình phục hồi năng lượng và làm việc bền vững.'"
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

    private sealed record QuizQuestionSpec(
        string Question,
        string OptionA,
        string OptionB,
        string OptionC,
        string OptionD,
        string CorrectKey,
        string Explanation
    );

    private static Guid CreateDeterministicQuizGuid(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"swms:psychology-quiz:{key}"));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }

    private static (PracticeQuiz Quiz, List<PracticeQuizItem> Items) BuildQuiz(
        string quizKey,
        Guid lessonId,
        string title,
        DateTimeOffset startedAt,
        DateTimeOffset itemBaseCreatedAt,
        QuizQuestionSpec[] questions)
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
            StartedAt = startedAt
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
                PromptAstJson = JsonSerializer.Serialize(new { question = q.Question }),
                ChoicesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "A", text = q.OptionA },
                    new { key = "B", text = q.OptionB },
                    new { key = "C", text = q.OptionC },
                    new { key = "D", text = q.OptionD }
                }),
                ClozeSolution = q.CorrectKey,
                AiRubricFeedbackJson = JsonSerializer.Serialize(new { summary = q.Explanation }),
                CreatedAt = itemBaseCreatedAt.AddSeconds(i)
            });
        }

        return (quiz, items);
    }

    /// <summary>
    /// Returns 6 lesson quizzes (PsychologyLesson11Id .. PsychologyLesson32Id) plus 1 comprehensive final course quiz
    /// attached to PsychologyLesson32Id. Each quiz contains 10 deterministic multiple_choice items
    /// with comprehensive cognitive and psychological feedback in AiRubricFeedbackJson.
    /// </summary>
    public static List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)> GetCourseQuizzes(DateTimeOffset now)
    {
        return new List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)>
        {
            // =========================================================================
            // 1. Quiz L11: Kiểm Tra Đánh Giá Năng Lực Bài 1.1: Xử Lý Thông Tin Tự Động vs Có Chủ Đích (Hệ Thống 1 & 2)
            // =========================================================================
            BuildQuiz(
                "l11",
                SeedDataInitializer.PsychologyLesson11Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 1.1: Xử Lý Thông Tin Tự Động vs Có Chủ Đích (Hệ Thống 1 & 2)",
                now.AddSeconds(1),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Điểm khác biệt căn bản nhất giữa Hệ thống 1 (Fast Thinking) và Hệ thống 2 (Slow Thinking) theo Daniel Kahneman là gì?",
                        "Hệ thống 1 hoạt động tự động, nhanh chóng, tiêu tốn ít năng lượng và mang tính cảm xúc/trực giác; Hệ thống 2 đòi hỏi sự tập trung có chủ đích, nỗ lực nhận thức cao và tuân theo quy luật logic",
                        "Hệ thống 1 luôn luôn đưa ra quyết định sai lầm, trong khi Hệ thống 2 không bao giờ mắc lỗi",
                        "Hệ thống 1 chỉ xuất hiện ở động vật, còn con người chỉ sử dụng duy nhất Hệ thống 2",
                        "Hệ thống 1 tiêu thụ nhiều glucose hơn Hệ thống 2 khi xử lý cùng một vấn đề phức tạp",
                        "A",
                        "Daniel Kahneman (Thinking, Fast and Slow) định nghĩa: Hệ thống 1 là xử lý ngầm định, tự động, nhanh và tiết kiệm năng lượng nhận thức. Hệ thống 2 là xử lý tường minh, chậm, có kiểm soát và tiêu tốn tài nguyên chú ý. B sai vì Hệ thống 1 rất hiệu quả trong sinh tồn hàng ngày; C sai về mặt tiến hóa sinh học; D sai vì Hệ thống 2 tiêu hao tài nguyên nhận thức và năng lượng não bộ lớn hơn nhiều."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Hiệu ứng Stroop (Stroop Effect) phản ánh cơ chế xung đột nhận thức nào trong não bộ?",
                        "Sự suy giảm trí nhớ dài hạn do thiếu hụt giấc ngủ REM",
                        "Sự xung đột giữa quá trình đọc chữ tự động của Hệ thống 1 và nhiệm vụ nhận diện màu mực có chủ đích của Hệ thống 2",
                        "Khả năng ghi nhớ hình ảnh tốt hơn ghi nhớ văn bản viết",
                        "Hiện tượng ảo giác thị giác do khúc xạ ánh sáng trên võng mạc",
                        "B",
                        "Trong thí nghiệm Stroop, việc đọc hiểu từ ngữ là một quá trình tự động hóa cao (Hệ thống 1). Khi màu mực (ví dụ: mực xanh lá) không khớp với từ ngữ (chữ 'ĐỎ'), Hệ thống 2 phải nỗ lực ức chế phản xạ đọc chữ để gọi tên màu mực, dẫn đến độ trễ phản xạ và dễ mắc lỗi."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Hiện tượng 'Cạn kiệt cái tôi' (Ego Depletion) hoặc mỏi mệt nhận thức (Cognitive Fatigue) liên quan thế nào đến Hệ thống 2?",
                        "Hệ thống 2 có dung lượng năng lượng nhận thức hữu hạn; khi phải liên tục tự kiểm soát hoặc tính toán phức tạp, khả năng tự chủ của Hệ thống 2 suy giảm khiến ta dễ đầu hàng trước cám dỗ của Hệ thống 1",
                        "Hệ thống 2 càng hoạt động nhiều thì càng tràn đầy năng lượng vô hạn mà không cần nghỉ ngơi",
                        "Khi mệt mỏi, con người hoàn toàn không thể sử dụng trực giác hay cảm xúc",
                        "Ego Depletion chỉ xảy ra ở những người có chỉ số IQ thấp",
                        "A",
                        "Nghiên cứu của Baumeister và Kahneman chỉ ra rằng sự tự chủ và nỗ lực tính toán của Hệ thống 2 dựa trên nguồn lực chú ý hữu hạn. Khi nguồn lực này bị vắt kiệt qua các quyết định căng thẳng, não bộ có xu hướng chuyển quyền điều khiển cho Hệ thống 1 để tiết kiệm năng lượng, dẫn đến quyết định bốc đồng hoặc bỏ cuộc sớm."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Trong mô hình Giao tiếp phi bạo lực (Nonviolent Communication - NVC) của Marshall Rosenberg, 4 bước biểu đạt sự thật lành mạnh theo đúng thứ tự là:",
                        "Phán xét → Trách móc → Yêu cầu trừng phạt → Tuyên bố cắt đứt",
                        "Quan sát không phán xét → Nhận diện Cảm xúc → Làm rõ Nhu cầu cốt lõi → Đưa ra Đề nghị cụ thể, khả thi",
                        "Đè nén cảm xúc → Nhượng bộ hoàn toàn → Bằng lòng giả tạo → Âm thầm trả đũa",
                        "Đọc suy nghĩ → Kết luận ý đồ xấu → Công kích cá nhân → Đòi bồi thường danh dự",
                        "B",
                        "NVC gồm 4 bước chuẩn mực: (1) Observation (Quan sát dữ kiện khách quan không kèm suy diễn), (2) Feeling (Cảm xúc xuất phát từ nội tâm), (3) Need (Nhu cầu, giá trị chưa được đáp ứng), (4) Request (Đề nghị cụ thể, tích cực, tôn trọng quyền từ chối của đối phương)."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tình huống): Bạn đang đi bộ trên vỉa hè thì bất ngờ một tiếng còi xe tải inh ỏi vang lên sát bên tai, bạn giật thót mình và nhảy lùi lại trong tích tắc trước khi kịp nhận ra chuyện gì xảy ra. Cơ chế tâm lý nào đã chi phối phản xạ này?",
                        "Hệ thống 2 đã thực hiện phân tích vận tốc và khối lượng xe tải rồi gửi lệnh đến cơ chân",
                        "Hệ thống 1 thông qua hạch hạnh nhân (Amygdala) đã kích hoạt phản xạ sinh tồn tự động nhanh chóng để bảo vệ cơ thể trước khi vỏ não trước trán kịp phân tích chi tiết",
                        "Ảo tưởng thành thạo (Illusion of Competence)",
                        "Thiên kiến xác nhận (Confirmation Bias)",
                        "B",
                        "Tiếng động lớn bất ngờ kích hoạt đường truyền thần kinh ngắn (low road) từ đồi thị thẳng tới hạch hạnh nhân (Amygdala) - bản chất là phản ứng sinh tồn tức thì của Hệ thống 1, diễn ra trong mili giây trước khi tín hiệu đến vỏ não trước trán (Hệ thống 2) để phân tích ngữ cảnh."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tình huống): Trong một cuộc tranh luận dự án, đồng nghiệp nói: 'Cách triển khai này của bạn sẽ làm trễ tiến độ'. Bạn cảm thấy tim đập nhanh và thôi thúc muốn hét lên: 'Anh thì biết cái gì mà nói!'. Để kích hoạt Hệ thống 2 và áp dụng NVC, hành động đầu tiên bạn nên làm là gì?",
                        "Bộc phát cơn giận ngay lập tức để chứng minh mình không yếu thế",
                        "Dừng lại 5 giây, hít sâu thở chậm để hạ nhiệt phản xạ chiến-hay-biến của Hệ thống 1, phân tách lời nói của đồng nghiệp (góp ý về phương án) khỏi lòng tự ái cá nhân",
                        "Lập tức bỏ về và viết đơn tố cáo đồng nghiệp lên ban giám đốc",
                        "Đồng ý mù quáng và hủy bỏ toàn bộ phương án dù biết cách làm của mình có cơ sở",
                        "B",
                        "Khoảng dừng 5 giây (The 5-second pause) kết hợp hít thở sâu kích hoạt hệ thần kinh phó giao cảm, giúp vỏ não trước trán (Hệ thống 2) giành lại quyền kiểm soát từ phản xạ bản năng phòng vệ của hạch hạnh nhân, từ đó chuyển từ phản ứng bốc đồng sang phản hồi có chủ đích."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tình huống): Đâu là câu nói tuân thủ chuẩn mực bước 1 của NVC (Quan sát thuần túy, tách biệt khỏi phán xét / suy diễn)?",
                        "'Anh lúc nào cũng vô trách nhiệm và không coi ai ra gì.'",
                        "'Trong 3 buổi họp tuần này, em thấy anh vào phòng họp trễ 10 phút so với giờ bắt đầu thông báo trên lịch.'",
                        "'Chắc chắn là cậu ghét nhóm này nên mới cố tình không nộp báo cáo đúng hạn.'",
                        "'Mày làm ăn như thế này thì sớm muộn công ty cũng phá sản.'",
                        "B",
                        "B là dữ kiện quan sát cụ thể (vào trễ 10 phút trong 3 buổi họp), có thể đo lường và kiểm chứng bởi camera hoặc biên bản. Các câu A, C, D đều chứa nhãn dán quy chụp ('vô trách nhiệm', 'cố tình ghét', 'làm ăn phá sản'), biến sự việc thành cuộc công kích cá nhân."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tình huống): Khi một nhân viên cảm thấy kiệt sức vì làm việc liên tục 12 tiếng không nghỉ, hiện tượng nào sau đây có khả năng xảy ra cao nhất ở quyết định cuối ngày của họ?",
                        "Họ sẽ viết code hoặc giải toán với độ chính xác và sáng tạo cao hơn buổi sáng",
                        "Do cạn kiệt tài nguyên nhận thức của Hệ thống 2, họ dễ mắc các lỗi logic ngớ ngẩn, đưa ra phán đoán bốc đồng hoặc dễ bị cuốn vào các bẫy thiên kiến cảm xúc",
                        "Khả năng tập trung chú ý có chủ đích đạt trạng thái cực đại (Superfocus)",
                        "Hệ thống 1 sẽ tự động tắt hoàn toàn để nhường chỗ cho tư duy chiều sâu",
                        "B",
                        "Cạn kiệt năng lượng nhận thức (Cognitive Depletion) làm suy yếu khả năng giám sát và ức chế của Hệ thống 2. Lúc này não bộ rơi vào chế độ tiết kiệm năng lượng, dễ chấp nhận các phán đoán vội vã, thiếu kiểm chứng của Hệ thống 1."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Ứng dụng): Một website bán vé máy bay hiển thị thông báo nhấp nháy đỏ: 'Chỉ còn 1 vé duy nhất với giá này! Có 14 người khác đang xem!'. Mục đích tâm lý của thiết kế này là gì?",
                        "Kích hoạt Hệ thống 2 của khách hàng để họ bình tĩnh đối chiếu giá vé giữa các hãng bay",
                        "Kích hoạt Hệ thống 1 thông qua hiệu ứng khan hiếm nhân tạo và nỗi sợ bỏ lỡ (FOMO), ép khách hàng quyết định mua vội mà không kịp cân nhắc lý trí",
                        "Cung cấp số liệu thống kê học thuật khách quan phi lợi nhuận",
                        "Giúp khách hàng rèn luyện năng lực kiềm chế cảm xúc",
                        "B",
                        "Đây là kỹ thuật 'Dark Pattern' đánh vào phản xạ sợ mất mát (Loss Aversion) và tính khan hiếm (Scarcity Heuristic) của Hệ thống 1, tạo áp lực thời gian để vô hiệu hóa sự can thiệp phân tích kỹ lưỡng của Hệ thống 2."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Ứng dụng): Để thiết kế môi trường làm việc giúp giảm tải nhận thức (Cognitive Offloading) và bảo toàn năng lượng cho Hệ thống 2, biện pháp nào sau đây là khoa học nhất?",
                        "Cố gắng nhớ toàn bộ lịch hẹn và deadline trong đầu mà không dùng sổ tay hay phần mềm",
                        "Mở cùng lúc 30 tab trình duyệt và bật toàn bộ thông báo tin nhắn để không bỏ lỡ thông tin",
                        "Xây dựng quy trình chuẩn (Checklist), sắp xếp không gian gọn gàng và sử dụng công cụ ghi chú số để giải phóng dung lượng bộ nhớ làm việc cho các tác vụ tư duy phức tạp",
                        "Làm việc liên tục 8 tiếng không giải lao để duy trì đà hưng phấn",
                        "C",
                        "Cognitive Offloading là kỹ thuật chuyển giao thông tin từ bộ nhớ sinh học sang công cụ vật lý/số học bên ngoài (checklist, calendar, kanban). Việc này giúp giảm tải bộ nhớ làm việc (Working Memory), bảo tồn năng lượng nhận thức của Hệ thống 2 cho các bài toán sáng tạo và phân tích chiến lược."
                    ),
                }
            ),
            // =========================================================================
            // 2. Quiz L12: Kiểm Tra Đánh Giá Năng Lực Bài 1.2: Ảo Tưởng Trí Nhớ & Thiên Kiến Xác Nhận
            // =========================================================================
            BuildQuiz(
                "l12",
                SeedDataInitializer.PsychologyLesson12Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 1.2: Ảo Tưởng Trí Nhớ & Thiên Kiến Xác Nhận",
                now.AddSeconds(1),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Thiên kiến xác nhận (Confirmation Bias) biểu hiện qua cơ chế tâm lý nào?",
                        "Xu hướng chỉ chủ động tìm kiếm, ghi nhớ và ưu tiên các thông tin củng cố niềm tin có sẵn của bản thân, đồng thời phớt lờ hoặc đánh giá thấp các bằng chứng phản biện ngược chiều",
                        "Khả năng nhớ chính xác 100% các sự kiện xảy ra từ thuở thơ ấu",
                        "Xu hướng luôn tin tưởng tuyệt đối vào mọi thông tin do người lạ cung cấp",
                        "Sự nghi ngờ mù quáng tất cả các định lý khoa học đã được kiểm chứng",
                        "A",
                        "Confirmation Bias là xu hướng nhận thức phổ biến nhất của con người: khi đã hình thành một giả thuyết hay định kiến, não bộ hoạt động như một 'luật sư biện hộ' chỉ đi gom chứng cứ bảo vệ thân chủ, thay vì hoạt động như một 'nhà khoa học khách quan' tìm kiếm sự thật."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Nghiên cứu kinh điển của Elizabeth Loftus về trí nhớ tái cấu trúc (Reconstructive Memory) đã chứng minh điều gì?",
                        "Trí nhớ con người hoạt động như một chiếc máy ghi hình video hoàn hảo, lưu trữ nguyên vẹn dữ liệu gốc và không bao giờ bị biến dạng",
                        "Ký ức không phải là bản sao cố định mà là một tiến trình tái tạo liên tục; việc cài cắm từ ngữ mang tính định hướng hoặc thông tin sai lệch sau sự kiện (Misinformation Effect) có thể thay đổi hoặc thậm chí tạo ra ký ức giả",
                        "Con người chỉ có thể nhớ những điều xảy ra trong vòng 24 giờ gần nhất",
                        "Trí nhớ thị giác không bao giờ bị đánh lừa bởi bất kỳ ảo giác nào",
                        "B",
                        "Elizabeth Loftus chứng minh qua hàng loạt thí nghiệm (như thay đổi từ ngữ mô tả tai nạn xe hơi từ 'va chạm nhẹ - hit' sang 'đâm nát - smash') rằng ký ức con người rất dễ bị bóp méo, bổ sung chi tiết giả mạo theo định hướng câu hỏi của người khác mà bản thân người nhớ vẫn tin đó là thật 100%."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Hiệu ứng mỏ neo (Anchoring Effect) là gì?",
                        "Hiện tượng thông tin đầu tiên nhận được (dù hoàn toàn ngẫu nhiên hoặc không liên quan) trở thành điểm tham chiếu chi phối toàn bộ các ước lượng và phán đoán tiếp theo",
                        "Kỹ thuật ghi nhớ từ vựng bằng cách liên tưởng đến mỏ neo tàu thủy",
                        "Khả năng duy trì sự kiên định trước áp lực dư luận xã hội",
                        "Xu hướng mua sắm tài sản cố định thay vì đầu tư tài chính",
                        "A",
                        "Anchoring Bias xảy ra khi một con số hoặc ấn tượng ban đầu được não bộ neo lại trong tâm trí. Mọi điều chỉnh nhận thức sau đó thường xoay quanh mỏ neo này và hiếm khi thoát ra được khỏi vùng ảnh hưởng của nó."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Hiệu ứng khung (Framing Effect) tác động thế nào đến hành vi con người?",
                        "Cách thức đóng khung thông điệp (nhấn mạnh vào lợi ích đạt được hay rủi ro mất mát) có thể làm thay đổi hoàn toàn quyết định của người nghe, dù bản chất toán học của dữ liệu là tương đương nhau",
                        "Người nghe chỉ quan tâm đến sự thật tuyệt đối và hoàn toàn miễn nhiễm với cách trình bày câu chữ",
                        "Đóng khung chỉ có tác dụng trong việc treo ảnh nghệ thuật trong phòng khách",
                        "Mọi người luôn luôn chọn phương án có nhiều từ ngữ hoa mỹ nhất",
                        "A",
                        "Daniel Kahneman và Amos Tversky chỉ ra qua bài toán 'Dịch bệnh châu Á' rằng: khi cùng một tỷ lệ xác suất được đóng khung theo hướng 'cứu sống' (gain frame), con người né tránh rủi ro; khi đóng khung theo hướng 'tử vong' (loss frame), con người lại sẵn sàng chấp nhận mạo hiểm."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tình huống): Một bác sĩ thông báo phương pháp phẫu thuật mới có 'tỷ lệ sống sót sau 1 tháng là 90%'. Một bác sĩ khác thông báo phương pháp đó có 'tỷ lệ tử vong trong 1 tháng là 10%'. Nghiên cứu tâm lý học cho thấy phản ứng của bệnh nhân sẽ như thế nào?",
                        "Bệnh nhân tiếp nhận cả hai thông điệp như nhau vì 90% sống sót tương đương 10% tử vong",
                        "Tỷ lệ bệnh nhân đồng ý phẫu thuật khi nghe '90% sống sót' cao hơn đáng kể so với khi nghe '10% tử vong' do Hiệu ứng khung chi phối cảm xúc",
                        "Không có bệnh nhân nào đồng ý phẫu thuật trong cả hai trường hợp",
                        "Bệnh nhân sẽ ngay lập tức khiếu nại bác sĩ vì vi phạm đạo đức y tế",
                        "B",
                        "Dù về mặt toán học hai phát biểu hoàn toàn giống nhau, từ ngữ 'sống sót' mang lại cảm giác an tâm tích cực, trong khi từ 'tử vong' kích hoạt nỗi sợ hãi mất mát sâu sắc của Hệ thống 1, khiến tỷ lệ chấp thuận điều trị chênh lệch rất lớn."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tình huống): Nam tin chắc rằng loại thực phẩm chức năng X giúp tăng cường trí nhớ vượt bậc. Khi lên mạng tra cứu, Nam gõ từ khóa: 'bằng chứng thực phẩm chức năng X giúp thông minh'. Nam lướt qua 10 bài viết khen ngợi trên trang bán hàng và lập tức bỏ qua 3 bài báo khoa học phản biện cảnh báo tác dụng phụ. Nam đang mắc bẫy gì?",
                        "Thiên kiến xác nhận (Confirmation Bias) trong việc tìm kiếm và chọn lọc thông tin",
                        "Kỹ thuật tư duy phản biện đa chiều xuất sắc",
                        "Hiệu ứng Stroop nhận thức",
                        "Trí nhớ làm việc bị quá tải do số liệu",
                        "A",
                        "Nam tự đặt câu hỏi tìm kiếm mang tính định hướng một chiều nhằm củng cố niềm tin có sẵn, đồng thời chủ động gạt bỏ dữ liệu phản biện khách quan - đây là hành vi kinh điển của Confirmation Bias."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tình huống): Một cửa hàng thời trang niêm yết chiếc áo khoác với giá gốc gạch ngang: '2.500.000đ', bên cạnh là giá khuyến mãi: '890.000đ'. Khách hàng cảm thấy món đồ quá hời và mua ngay dù chưa rõ chất lượng vải. Yếu tố tâm lý mấu chốt ở đây là gì?",
                        "Mức giá 2.500.000đ đóng vai trò là một mỏ neo (Anchor) tạo cảm giác tiết kiệm được 1.610.000đ, dù giá trị thực tế của chiếc áo chỉ đáng 500.000đ",
                        "Khách hàng đã kích hoạt Hệ thống 2 để đo đạc độ bền của sợi cotton",
                        "Hiệu ứng Dunning-Kruger về kỹ năng may mặc",
                        "Sự thấu cảm sâu sắc giữa người mua và người bán",
                        "A",
                        "Con số 2,5 triệu đồng thiết lập một mỏ neo tham chiếu giá trị cao. Khi nhìn vào mức giá 890 ngàn, người mua so sánh với mỏ neo 2,5 triệu thay vì so sánh với giá trị sử dụng thực tế của chiếc áo, tạo ra ảo tưởng về một món hời lớn."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Kỹ thuật): Kỹ thuật 'Steelman' (Người thép) - đối lập với 'Strawman' (Người rơm) - giúp phá vỡ Thiên kiến xác nhận bằng cách nào?",
                        "Cố tình bóp méo và chế giễu quan điểm của đối phương để dễ dàng đập tan lập luận của họ",
                        "Nỗ lực xây dựng lại luận điểm của đối thủ ở phiên bản mạnh mẽ, logic và thuyết phục nhất có thể trước khi đưa ra phản biện của chính mình",
                        "Mời các võ sĩ thép đến tham gia tranh luận để áp đảo tinh thần đối phương",
                        "Từ chối lắng nghe bất kỳ ý kiến nào trái ngược với lập trường ban đầu của bản thân",
                        "B",
                        "Kỹ thuật Steelman buộc não bộ phải chủ động tìm kiếm các bằng chứng mạnh mẽ nhất ủng hộ góc nhìn đối lập, vô hiệu hóa xu hướng tự biện hộ của Confirmation Bias và mở rộng tầm nhìn nhận thức toàn diện."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tình huống): Sau một buổi họp lớp, Mai và Lan cùng nhớ lại một kỷ niệm cách đây 5 năm. Mai kể rất chi tiết rằng lúc đó trời mưa to và cả nhóm trú dưới gốc cây bàng. Lan khẳng định hôm đó trời nắng ráo và nhóm ngồi trong quán cà phê. Cả hai đều quả quyết 100% ký ức của mình là đúng sự thật. Lời giải thích khoa học nhận thức phù hợp nhất là:",
                        "Chắc chắn có một người đang cố tình nói dối để trục lợi cá nhân",
                        "Ký ức của con người mang tính tái cấu trúc và dễ bị chắp vá, biến dạng theo thời gian bởi những thông tin và cảm xúc sau đó; việc tin chắc chắn 100% không bảo đảm tính chính xác khách quan của ký ức",
                        "Mai bị mất trí nhớ tạm thời do chấn thương não bộ",
                        "Bộ não của Lan có khả năng ghi nhớ quang học bất biến",
                        "B",
                        "Độ tin cậy chủ quan (Confidence) của một người về ký ức không tương quan tỷ lệ thuận với tính chính xác khách quan (Accuracy) của nó. Cả hai người đều có thể đã vô tình ghép các mảnh ký ức từ các dịp khác nhau vào buổi gặp mặt 5 năm trước mà không hề hay biết."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Ứng dụng): Để giảm thiểu tác động tiêu cực của Thiên kiến xác nhận khi ra quyết định kinh doanh hoặc nghiên cứu, nhóm làm việc nên áp dụng cơ chế nào?",
                        "Chỉ tuyển dụng những người có cùng quan điểm và sở thích với người lãnh đạo",
                        "Phân công chính thức một cá nhân hoặc nhóm đóng vai 'Người biện hộ của quỷ' (Devil's Advocate) có nhiệm vụ tìm kiếm lỗ hổng và bằng chứng phản biện lại kế hoạch",
                        "Trừng phạt bất kỳ thành viên nào dám đưa ra ý kiến trái chiều trong cuộc họp",
                        "Bỏ phiếu kín xem ai là người nói nhiều nhất trong tuần",
                        "B",
                        "Thiết lập vai trò 'Devil\\'s Advocate' (Người biện hộ của quỷ) hợp pháp hóa việc phản biện trong tổ chức, giúp bảo vệ người phản biện khỏi áp lực hòa nhập đám đông (Groupthink) và buộc toàn đội phải kiểm tra lại các giả định ngầm của dự án."
                    ),
                }
            ),
            // =========================================================================
            // 3. Quiz L21: Kiểm Tra Đánh Giá Năng Lực Bài 2.1: Dung Lượng Trí Nhớ & Kỹ Thuật Chunking
            // =========================================================================
            BuildQuiz(
                "l21",
                SeedDataInitializer.PsychologyLesson21Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 2.1: Dung Lượng Trí Nhớ & Kỹ Thuật Chunking",
                now.AddSeconds(1),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Trí nhớ làm việc (Working Memory) khác với Trí nhớ ngắn hạn truyền thống (Short-term Memory) ở điểm cốt lõi nào?",
                        "Working Memory không chỉ tạm thời lưu trữ thông tin mà còn trực tiếp thao tác, biến đổi và xử lý các mẩu dữ liệu đó để phục vụ suy luận và giải quyết vấn đề",
                        "Working Memory có dung lượng vô hạn và lưu giữ thông tin suốt đời",
                        "Working Memory chỉ hoạt động khi con người đang ngủ sâu",
                        "Working Memory không tiêu tốn bất kỳ năng lượng glucose nào của não bộ",
                        "A",
                        "Baddeley & Hitch định nghĩa Working Memory là hệ thống năng động bao gồm bộ nhớ đệm lưu trữ tạm thời kết hợp với bộ điều hành trung tâm (Central Executive) thao tác, tính toán và xử lý dữ liệu phục vụ các tác vụ nhận thức cấp cao."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Quy tắc kinh điển của George Miller (1956) và các hiệu chỉnh hiện đại của Nelson Cowan (2001) về dung lượng của bộ nhớ làm việc cho biết giới hạn lưu trữ là bao nhiêu đơn vị (chunks)?",
                        "Miller đề xuất 7 ± 2 đơn vị; Cowan và các nghiên cứu thần kinh hiện đại chỉ ra rằng khi không được nhẩm lại, dung lượng thực tế chỉ khoảng 4 ± 1 đơn vị",
                        "Dung lượng của bộ nhớ làm việc là 100 đơn vị độc lập cùng lúc",
                        "Con người chỉ có thể nhớ duy nhất 1 từ tại một thời điểm",
                        "Dung lượng bộ nhớ làm việc tăng gấp đôi sau mỗi năm tuổi thọ",
                        "A",
                        "George Miller đưa ra 'con số kỳ diệu 7 ± 2', nhưng các thực nghiệm hiện đại của Nelson Cowan chứng minh rằng nếu ngăn chặn việc nhẩm thầm (rehearsal), dung lượng thuần túy của Working Memory chỉ đạt khoảng 3–5 chunks (trung bình 4 đơn vị)."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Kỹ thuật 'Chunking' (Phân cụm thông tin) giúp tối ưu hóa trí nhớ làm việc bằng cách nào?",
                        "Xóa bỏ hoàn toàn ký ức cũ để nhường chỗ cho dữ liệu mới",
                        "Tổ chức và gom các mẩu thông tin đơn lẻ, rời rạc thành các cụm có ý nghĩa hoặc liên kết logic với kiến thức trong trí nhớ dài hạn, giúp não bộ xử lý nhiều thông tin hơn trong cùng một giới hạn dung lượng",
                        "Uống nhiều cà phê đậm đặc để ép tế bào thần kinh hoạt động gấp ba lần",
                        "Đọc lướt thật nhanh mà không cần hiểu ý nghĩa của các từ ngữ",
                        "B",
                        "Chunking khai thác cấu trúc kiến thức sẵn có trong Long-term Memory để nén nhiều dữ liệu rời rạc thành một khối đơn vị (chunk). Nhờ đó, 10–12 ký tự rời rạc có thể được nén thành 2–3 chunks dễ dàng chứa trong Working Memory."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Theo Thuyết tải nhận thức (Cognitive Load Theory) của John Sweller, 3 loại tải nhận thức bao gồm:",
                        "Tải vật lý, tải hóa học và tải sinh học",
                        "Tải nội tại (Intrinsic Load), Tải ngoại lai (Extraneous Load) và Tải hữu ích (Germane Load)",
                        "Tải thị giác, tải thính giác và tải xúc giác",
                        "Tải tốc độ cao, tải trung bình và tải không trọng lực",
                        "B",
                        "John Sweller phân loại: Intrinsic Load (độ khó tự nhiên vốn có của tài liệu), Extraneous Load (tải rác do thiết kế bài giảng rối rắm, font chữ lộn xộn, tiếng ồn gây phân tâm), và Germane Load (tải nhận thức hữu ích dành cho việc kết nối kiến thức và xây dựng lược đồ tư duy - schema)."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tình huống): Hãy nhìn dãy số ngẫu nhiên gồm 10 chữ số: '0 9 1 2 3 4 5 6 7 8'. Nếu một người cố nhớ 10 con số rời rạc, họ sẽ rất dễ quên. Nhưng nếu họ nhận ra đây là số điện thoại gồm mã mạng và dãy tiến '0912 - 345 - 678', họ nhớ ngay lập tức. Đây là minh họa chính xác của:",
                        "Kỹ thuật Chunking (Phân cụm dựa trên quy luật quen thuộc)",
                        "Hiệu ứng Stroop",
                        "Thiên kiến mỏ neo",
                        "Tái cấu trúc nhận thức CBT",
                        "A",
                        "Thay vì phải gánh 10 chunks số học vượt quá giới hạn của Working Memory (vốn chỉ chứa được 4–7 chunks), người học nén lại thành 3 cụm (mã mạng 0912, nhóm 345, nhóm 678), giảm tải nhận thức xuống mức tối ưu."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tình huống): Một giảng viên trình bày slide có toàn chữ li ti đặc kín màn hình, vừa bật nhạc nền sôi động, vừa nói một nội dung hoàn toàn khác với chữ trên slide. Sinh viên cảm thấy đau đầu và không tiếp thu được gì. Giảng viên này đã vi phạm nguyên tắc gì của Thuyết tải nhận thức?",
                        "Tạo ra quá nhiều Tải ngoại lai (Extraneous Load) và gây nghẽn hiệu ứng kênh đôi (Split-attention effect) giữa kênh nghe và kênh nhìn",
                        "Tăng cường quá mức Tải hữu ích (Germane Load) giúp sinh viên khai mở trí tuệ",
                        "Làm giảm Tải nội tại (Intrinsic Load) xuống mức 0",
                        "Giúp sinh viên rèn luyện khả năng đa nhiệm (Multitasking) hoàn hảo",
                        "A",
                        "Việc đưa quá nhiều yếu tố gây nhiễu (nhạc nền, chữ đặc kín mâu thuẫn lời nói) tạo ra Extraneous Load khổng lồ, làm tê liệt bộ nhớ làm việc của người học trước khi họ kịp xử lý nội dung cốt lõi của bài giảng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tình huống): Một kiện tướng cờ vua có thể nhớ chính xác vị trí của 25 quân cờ trên bàn cờ chỉ sau 5 giây quan sát một thế cờ thực tế, nhưng nếu các quân cờ đó được đặt ngẫu nhiên phi lý (không theo quy luật thi đấu), khả năng ghi nhớ của kiện tướng giảm xuống ngang bằng một người mới chơi. Thí nghiệm kinh điển này của De Groot và Chase & Simon chứng minh điều gì?",
                        "Kiện tướng cờ vua sinh ra đã có bộ não chụp ảnh (Photographic memory) bẩm sinh",
                        "Khả năng nhớ siêu phàm của chuyên gia phụ thuộc vào các cụm tri thức (chunks / schemas) sâu sắc đã tích lũy trong Trí nhớ dài hạn; khi bối cảnh không có cấu trúc ý nghĩa, ưu thế chunking biến mất",
                        "Cờ vua là môn thể thao hoàn toàn dựa trên may rủi",
                        "Trí nhớ làm việc của chuyên gia có sức chứa 50 chunks độc lập",
                        "B",
                        "Thí nghiệm của Chase & Simon (1973) khẳng định: chuyên gia không có dung lượng Working Memory sinh học lớn hơn người thường, mà họ có kho tàng các khối mẫu thế cờ (chunks) khổng lồ trong Long-term Memory. Khi nhìn thế cờ chuẩn, họ chỉ nhớ 3–4 cụm chiến thuật thay vì nhớ 25 quân cờ rời rạc."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Kỹ thuật): Phương pháp 'Dual Coding' (Mã hóa kép) của Allan Paivio hỗ trợ bộ nhớ làm việc như thế nào?",
                        "Buộc người học phải học cùng lúc hai ngôn ngữ khác nhau để kích thích bán cầu não",
                        "Kết hợp đồng thời kênh biểu đạt ngôn từ (Verbal code) và kênh hình ảnh thị giác (Visual code) giúp thông tin được xử lý qua hai kênh độc lập của Working Memory, tăng cường khả năng ghi nhớ sâu sắc",
                        "Viết một từ hai lần bằng cả tay trái và tay phải",
                        "Đọc to một văn bản hai lần liên tiếp không ngừng nghỉ",
                        "B",
                        "Paivio chứng minh não bộ có hai phân hệ xử lý thông tin song song: âm thanh/ngôn từ (Phonological Loop) và hình ảnh không gian (Visuospatial Sketchpad). Kết hợp hình ảnh minh họa cô đọng cùng giải thích bằng lời giúp nhân đôi cơ hội ghi nhớ mà không làm quá tải một kênh duy nhất."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tình huống): Một lập trình viên đang cố gắng gỡ lỗi một hàm phức tạp có 8 biến số thay đổi liên tục. Người đó liên tục bị quên giá trị của các biến trước đó. Giải pháp kỹ thuật nào sau đây áp dụng đúng nguyên lý giải phóng bộ nhớ làm việc?",
                        "Cố gắng tập trung nhắm mắt lại để nhớ cả 8 biến trong đầu",
                        "Viết test case tự động hoặc in giá trị các biến ra log màn hình (Cognitive Offloading) để chuyển việc lưu trữ sang môi trường bên ngoài, dành trọn vẹn Working Memory cho việc phân tích logic thuật toán",
                        "Bật thêm nhạc rap âm lượng lớn để kích thích thần kinh",
                        "Viết thêm 10 biến số mới vào hàm để thử vận may",
                        "B",
                        "8 biến số biến thiên vượt quá dung lượng 4 chunks của Working Memory. Việc in log hoặc ghi ra nháp là hình thức giải phóng tải nhận thức (Cognitive Offloading), giúp bộ não không phải gồng mình ghi nhớ dữ liệu thô mà tập trung vào suy luận tìm nguyên nhân gốc rễ."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Ứng dụng): Để tối ưu hóa việc tự học một chủ đề chuyên môn khó (như tài chính hoặc kiến trúc phần mềm), người học nên cấu trúc lộ trình như thế nào theo nguyên lý Chunking?",
                        "Học dàn trải cùng lúc 20 khái niệm nâng cao trong một buổi tối duy nhất",
                        "Đi từ các khái niệm nền tảng cốt lõi, luyện tập đến mức thuần thục để chúng tạo thành các khối tri thức tự động (Automated chunks/schemas), sau đó mới ghép nối các khối đó vào các mô hình phức tạp hơn",
                        "Chỉ học các ví dụ ngoài lề mà bỏ qua lý thuyết cơ bản",
                        "Tuyệt đối không bao giờ làm bài tập thực hành vì sẽ làm mỏi trí nhớ làm việc",
                        "B",
                        "Tự động hóa tri thức nền tảng (Schema Automation) biến các quy trình phức tạp thành một chunk duy nhất gần như không tốn tải nhận thức. Nhờ đó, người học có thể tiếp nhận và giải quyết các bài toán cấp cao hơn mà không bị kiệt quệ Working Memory."
                    ),
                }
            ),
            // =========================================================================
            // 4. Quiz L22: Kiểm Tra Đánh Giá Năng Lực Bài 2.2: Đường Cong Quên Ebbinghaus & Ôn Tập FSRS
            // =========================================================================
            BuildQuiz(
                "l22",
                SeedDataInitializer.PsychologyLesson22Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 2.2: Đường Cong Quên Ebbinghaus & Ôn Tập FSRS",
                now.AddSeconds(1),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Đường cong quên lãng của Hermann Ebbinghaus (Forgetting Curve) mô tả quy luật sinh học nào của trí nhớ?",
                        "Lượng thông tin ghi nhớ suy giảm nhanh chóng theo hàm số mũ ngay trong những giờ và ngày đầu tiên sau khi học; nếu không có sự gợi nhắc, ta có thể quên tới 50–70% nội dung chỉ sau 24 đến 48 giờ",
                        "Não bộ con người giữ nguyên 100% kiến thức trong 30 ngày đầu rồi mới bắt đầu quên",
                        "Trẻ em không bao giờ bị quên kiến thức một khi đã đọc qua sách giáo khoa",
                        "Đường cong quên là một đường thẳng đi ngang không bao giờ suy giảm",
                        "A",
                        "Năm 1885, Hermann Ebbinghaus phát hiện ra rằng tốc độ quên diễn ra nhanh nhất ngay sau khi học (mất hơn 50% trong ngày đầu tiên), sau đó tốc độ quên dần phẳng lại theo thời gian. Muốn duy trì ký ức, cần phải có các lần can thiệp ôn tập để bẻ cong đường dốc này."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): 'Hiệu ứng kiểm tra' (Testing Effect) hay Tái hiện chủ động (Active Recall / Retrieval Practice) khẳng định điều gì?",
                        "Việc làm bài kiểm tra chỉ gây thêm căng thẳng và không có giá trị học tập",
                        "Việc chủ động nỗ lực truy hồi thông tin từ trong não bộ (không nhìn tài liệu) tạo ra dấu ấn thần kinh mạnh mẽ và bền vững hơn rất nhiều so với việc đọc lại thụ động (Passive Rereading)",
                        "Đọc đi đọc lại một trang sách 10 lần có hiệu quả ghi nhớ cao gấp đôi việc tự làm quiz thử sức",
                        "Chỉ nên kiểm tra kiến thức khi đã chắc chắn nhớ đúng 100% tài liệu",
                        "B",
                        "Nghiên cứu của Roediger & Karpicke (2006) chứng minh: tiến trình nỗ lực truy hồi ký ức (retrieval) kích hoạt các liên kết synapse thần kinh mới và củng cố dấu vết ký ức (memory trace), mang lại hiệu quả lưu giữ dài hạn vượt trội hơn hẳn so với việc đọc lại nhiều lần."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Hiện tượng 'Ảo tưởng thành thạo' (Illusion of Competence) khi ôn thi thường xuất phát từ hành vi học tập sai lầm nào?",
                        "Tự làm đề thi thử trong điều kiện bấm giờ nghiêm ngặt",
                        "Đọc lướt qua sách hoặc xem lời giải có sẵn, thấy chữ quen mắt thì nhầm tưởng rằng mình đã thực sự hiểu và có thể tự làm lại được trong phòng thi",
                        "Dạy lại kiến thức cho một người bạn chưa hiểu bài theo phương pháp Feynman",
                        "Tự vẽ sơ đồ tư duy từ trí nhớ mà không nhìn tài liệu gốc",
                        "B",
                        "Khi đọc lại tài liệu có sẵn, não bộ nhận diện sự quen thuộc bề mặt (Fluency Heuristic) và đánh lừa ta rằng ta đã làm chủ kiến thức. Chỉ khi đóng sách lại và tự viết ra (Active Recall), lỗ hổng kiến thức thực sự mới lộ diện."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Thuật toán ôn tập hiện đại FSRS v5 (Free Spaced Repetition Scheduler) dựa trên 3 biến số cốt lõi nào của mô hình trí nhớ DSR?",
                        "Độ dài trang sách, số lượng hình ảnh, và thời gian đọc bài",
                        "Độ khó của tài liệu (Difficulty - D), Độ bền vững của ký ức (Stability - S), và Khả năng truy hồi thành công tại thời điểm hiện tại (Retrievability - R)",
                        "Cân nặng học viên, nhiệt độ phòng học, và tốc độ gõ bàn phím",
                        "Số lượng bài tập làm sai, số lần bật khóc, và điểm số bài thi giữa kỳ",
                        "B",
                        "Mô hình DSR của FSRS định lượng hóa trí nhớ: D (Difficulty - độ khó tương đối của thẻ), S (Stability - thời gian tính bằng ngày để xác suất nhớ giảm từ 100% xuống 90%), R (Retrievability - xác suất truy hồi đúng thẻ tại thời điểm t). FSRS tính toán khoảng cách ôn tập tiếp theo dựa trên giá trị R mục tiêu (thường là 90%)."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Khái niệm): Nguyên lý 'Khó khăn có ích' (Desirable Difficulties) của Robert Bjork giải thích vì sao việc học không nên quá dễ dàng:",
                        "Càng học trong điều kiện khổ sở, đói khát thì trí thông minh càng phát triển",
                        "Những nỗ lực nhận thức vừa sức đòi hỏi não bộ phải vất vả truy hồi và xử lý sâu sắc sẽ tạo ra sự biến đổi thần kinh bền vững; trải nghiệm học tập quá trơn tru, dễ dãi thường chỉ tạo ra trí nhớ ngắn hạn bề mặt",
                        "Mọi tài liệu học tập đều phải được mã hóa bằng tiếng Latin cổ để gây khó khăn cho học viên",
                        "Không bao giờ được phép cho học sinh xem đáp án đúng kể cả sau khi đã nộp bài",
                        "B",
                        "Robert Bjork chứng minh rằng các điều kiện học tập tưởng như làm chậm tiến độ trước mắt (như ôn tập ngắt quãng Spaced Repetition, xáo trộn chủ đề Interleaving, tự kiểm tra Active Recall) chính là những 'khó khăn có ích' giúp củng cố ký ức dài hạn sâu sắc nhất."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tình huống): Bạn Nam có một bài kiểm tra lịch sử sau 4 tuần nữa. Nam có 12 giờ để ôn tập. Chiến lược phân bổ thời gian nào dưới đây đem lại kết quả ghi nhớ dài hạn tốt nhất theo khoa học trí nhớ?",
                        "Học dồn 12 tiếng liên tục thâu đêm ngay trước ngày thi (Cramming / Massed Practice)",
                        "Chia 12 giờ thành 6 buổi học (mỗi buổi 2 giờ), rải đều trong suốt 4 tuần với khoảng cách giãn dần kết hợp tự làm flashcard kiểm tra (Spaced Practice)",
                        "Dành 11 tiếng đọc sách trong tuần đầu tiên và 1 tiếng còn lại ngồi thiền ở tuần thứ tư",
                        "Không học gì cả và tin tưởng hoàn toàn vào trực giác may mắn của bản thân",
                        "B",
                        "Học ngắt quãng (Distributed / Spaced Practice) vượt trội hơn hẳn học dồn (Massed Practice). Mỗi lần quay lại ôn tập sau một khoảng nghỉ, ký ức hơi phai mờ buộc não bộ phải nỗ lực truy hồi lại, làm tăng vọt độ bền vững (Stability) của thông tin."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tình huống): Khi ôn tập từ vựng bằng flashcard số (như Anki hoặc Studxy), một thẻ từ mới hiện ra. Bạn nên làm hành động nào trước tiên để tối đa hóa hiệu quả học tập?",
                        "Bấm nút xem đáp án ngay lập tức trong 0,5 giây để tiết kiệm thời gian đọc tiếp từ khác",
                        "Dừng lại 5–10 giây, chủ động lục tìm trong tâm trí nghĩa của từ, phát âm và đặt câu thử trong đầu trước khi lật mặt sau đối chiếu",
                        "Đọc nhẩm 100 lần mặt trước mà không cần quan tâm mặt sau viết gì",
                        "Chụp ảnh màn hình lưu vào thư viện ảnh điện thoại rồi bỏ qua",
                        "B",
                        "Hành động dừng lại tự truy hồi (Retrieval Attempt) chính là lúc não bộ kích hoạt liên kết synapse. Xem đáp án ngay lập tức biến buổi học thành đọc thụ động và kích hoạt ảo tưởng thành thạo."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tình huống): Một học viên phàn nàn: 'Mỗi lần tôi ôn tập ngắt quãng sau 3 ngày, tôi cảm thấy rất vất vả để nhớ lại từ vựng, không như khi vừa học xong thì đọc trơn tru. Có phải phương pháp này không hiệu quả?'. Bạn sẽ giải thích thế nào dựa trên khoa học thần kinh?",
                        "Bạn học viên nói đúng, nên quay lại phương pháp học thuộc lòng ngay lập tức khi vừa đọc xong",
                        "Cảm giác vất vả đó chính là 'khó khăn có ích'; chính nỗ lực vượt qua sự phai mờ của ký ức để kéo thông tin trở lại vùng nhận thức mới là điều kích hoạt sự tái củng cố ký ức (Memory Reconsolidation) và giúp nhớ lâu hơn",
                        "Học viên này đã bị suy giảm trí nhớ vĩnh viễn và không nên học ngoại ngữ nữa",
                        "Nên tăng số lần học dồn lên 10 tiếng mỗi ngày để không còn cảm giác khó khăn",
                        "B",
                        "Trí nhớ ngắn hạn trơn tru đánh lừa cảm giác của người học. Theo Bjork, 'Storage Strength' (sức bền lưu trữ) chỉ tăng trưởng mạnh mẽ khi 'Retrieval Strength' (sức mạnh truy hồi hiện thời) đang ở mức thấp vừa phải - tức là lúc ta thấy hơi khó nhớ lại."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Kỹ thuật): Phương pháp 'Học xen kẽ' (Interleaving) khác với 'Học theo khối' (Blocked Practice) ở điểm nào và mang lại lợi ích gì?",
                        "Học theo khối là làm liên tục một dạng bài A cho đến khi chán; Học xen kẽ là trộn lẫn các dạng bài A, B, C trong cùng buổi học, giúp não rèn luyện kỹ năng phân biệt bản chất vấn đề và chọn công cụ phù hợp",
                        "Học xen kẽ là vừa xem phim vừa làm bài tập toán",
                        "Học theo khối giúp trở thành thiên tài chỉ sau 2 ngày ôn tập",
                        "Học xen kẽ làm giảm khả năng tư duy độc lập của học sinh",
                        "A",
                        "Interleaving buộc não bộ không được áp dụng máy móc một công thức cho mọi câu hỏi kế tiếp, mà phải liên tục so sánh, phân tích dấu hiệu đặc trưng để nhận diện dạng bài toán trước khi giải, giúp nâng cao khả năng áp dụng thực tế và làm bài thi."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Ứng dụng): Trong thuật toán FSRS v5, nếu bạn đánh giá một thẻ là 'Again' (Quên hoàn toàn), hệ thống sẽ phản ứng như thế nào để phục hồi trí nhớ cho bạn?",
                        "Xóa vĩnh viễn thẻ đó khỏi tài khoản người dùng để tránh làm phiền",
                        "Đặt lại chu kỳ ôn tập về bước ngắn nhất (short interval), giảm chỉ số Stability để bạn ôn lại ngay trong phiên học và tăng tần suất xuất hiện cho đến khi ký ức được củng cố lại vững chắc",
                        "Đánh dấu bạn đã hoàn thành xuất sắc thẻ đó và không bao giờ hỏi lại nữa",
                        "Tự động trừ điểm kinh nghiệm và khóa tài khoản học tập trong 24 giờ",
                        "B",
                        "Khi bấm 'Again', thuật toán xác định Retrievability đã rơi xuống dưới ngưỡng an toàn. Thẻ sẽ được đưa vào chuỗi phục hồi (Relearning phase) với khoảng thời gian ngắn để xây dựng lại dấu vết ký ức từ đầu."
                    ),
                }
            ),
            // =========================================================================
            // 5. Quiz L31: Kiểm Tra Đánh Giá Năng Lực Bài 3.1: Mô Hình ABC & Tái Cấu Trúc Nhận Thức CBT
            // =========================================================================
            BuildQuiz(
                "l31",
                SeedDataInitializer.PsychologyLesson31Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 3.1: Mô Hình ABC & Tái Cấu Trúc Nhận Thức CBT",
                now.AddSeconds(1),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Trong mô hình ABC của liệu pháp Hành vi Nhận thức (CBT do Albert Ellis sáng lập), các chữ cái A, B, C đại diện cho những thành tố nào?",
                        "A (Action - Hành động), B (Brain - Não bộ), C (Character - Tính cách)",
                        "A (Activating Event - Sự kiện kích hoạt), B (Beliefs - Hệ thống niềm tin / suy nghĩ diễn giải), C (Consequences - Hệ quả cảm xúc và hành vi)",
                        "A (Anger - Cơn giận), B (Blame - Đổ lỗi), C (Crisis - Khủng hoảng)",
                        "A (Advice - Lời khuyên), B (Benefit - Lợi ích), C (Cost - Chi phí)",
                        "B",
                        "Cốt lõi của CBT khẳng định: Không phải sự kiện A trực tiếp gây ra cảm xúc C, mà chính là lăng kính diễn giải và niềm tin B của cá nhân về sự kiện A mới là nguyên nhân trực tiếp dẫn đến cảm xúc và hành vi C."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Triết gia Khắc kỷ Epictetus từng nói một câu nổi tiếng làm nền tảng cho CBT hiện đại:",
                        "'Tiền bạc và danh vọng là thước đo duy nhất của hạnh phúc con người.'",
                        "'Con người không bị xáo trộn bởi sự vật, mà bởi những định kiến và quan điểm mà họ có về sự vật.'",
                        "'Số phận đã an bài tất cả, mọi nỗ lực thay đổi suy nghĩ đều là vô ích.'",
                        "'Cảm xúc tiêu cực cần phải được giải tỏa bằng cách đập phá đồ đạc ngay lập tức.'",
                        "B",
                        "Câu nói của Epictetus ('Men are disturbed not by things, but by the view which they take of them') chính là nguồn cảm hứng trực tiếp để Aaron Beck và Albert Ellis xây dựng nên lý thuyết tái cấu trúc nhận thức CBT."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Lỗi méo mó nhận thức 'Tư duy trắng - đen' hay 'Tất cả hoặc không' (All-or-Nothing / Dichotomous Thinking) có biểu hiện đặc trưng là gì?",
                        "Nhìn nhận sự việc chỉ ở hai thái cực đối lập hoàn toàn (hoàn hảo tuyệt đối hoặc thất bại thảm hại), không chấp nhận bất kỳ vùng xám trung gian hay sự tiến bộ từng phần nào",
                        "Khả năng phối màu trang phục đen trắng rất thanh lịch nơi công sở",
                        "Khả năng đọc suy nghĩ của người khác qua cử chỉ ánh mắt",
                        "Thói quen ngủ đủ 8 tiếng mỗi ngày không gián đoạn",
                        "A",
                        "Người mắc bẫy All-or-Nothing tin rằng: nếu không đạt điểm 10 tuyệt đối thì coi như điểm 0; nếu một buổi tập gym bị gián đoạn 5 phút thì coi như cả buổi tập vô nghĩa. Lỗi này tạo ra áp lực hoàn hảo độc hại và dẫn tới bỏ cuộc sớm."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Lỗi nhận thức 'Trầm trọng hóa' (Catastrophizing) thúc đẩy tâm lý con người phản ứng như thế nào?",
                        "Luôn bình thản mỉm cười trước mọi hiểm nguy thực sự",
                        "Tự động phóng đại sự việc tiêu cực nhỏ thành thảm họa hủy diệt tồi tệ nhất có thể tưởng tượng được, và tin chắc rằng mình hoàn toàn bất lực không thể chịu đựng nổi",
                        "Đánh giá quá cao khả năng tài chính của bản thân",
                        "Lạc quan tếu và không mua bảo hiểm y tế",
                        "B",
                        "Catastrophizing biến một sai sót nhỏ (ví dụ lỡ lời trong cuộc họp) thành kịch bản tận thế: 'Mình sẽ bị sa thải, mất hết danh dự, trở thành kẻ vô gia cư và chết đói ngoài đường'."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Khái niệm): Phương pháp 'Hỏi kiểu Socrates' (Socratic Questioning) trong CBT được sử dụng nhằm mục đích gì?",
                        "Ép buộc thân chủ phải thừa nhận mình ngu ngốc trước mặt chuyên gia",
                        "Sử dụng các câu hỏi gợi mở, logic và dựa trên bằng chứng để giúp người học tự khám phá, kiểm chứng và thách thức tính xác thực của những suy nghĩ tự động tiêu cực của chính mình",
                        "Thuyết trình độc thoại liên tục 3 tiếng về triết học Hy Lạp cổ đại",
                        "Khuyên nhủ mọi người hãy suy nghĩ tích cực một cách mù quáng",
                        "B",
                        "Socratic Questioning không áp đặt quan điểm, mà hướng dẫn người học tự đóng vai nhà điều tra: 'Có bằng chứng khách quan nào ủng hộ suy nghĩ này không?', 'Điều tồi tệ nhất có thể xảy ra là gì và mình có thể ứng phó ra sao?', 'Nếu một người bạn thân gặp hoàn cảnh này, mình sẽ nói gì với họ?'."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tình huống): Sau khi nộp bài luận, Hương nhận được lời khen ngợi từ 4 giám khảo về tính sáng tạo, nhưng giám khảo thứ 5 có nhận xét: 'Phần tài liệu tham khảo cần bổ sung thêm 2 nguồn trích dẫn'. Hương rơi vào u uất cả tuần, tin rằng bài luận của mình là một mớ rác rưởi. Hương đang mắc lỗi nhận thức nào?",
                        "Bộ lọc tiêu cực (Mental Filter) - chỉ phóng đại chi tiết góp ý nhỏ và phớt lờ toàn bộ bức tranh tích cực áp đảo xung quanh",
                        "Tự trắc ẩn lành mạnh (Self-Compassion)",
                        "Khả năng đánh giá khách quan đa chiều",
                        "Hiệu ứng Dunning-Kruger",
                        "A",
                        "Mental Filter hoạt động như một giọt mực rơi vào cốc nước trong: người học chỉ tập trung vào một hạt sạn duy nhất (lời góp ý trích dẫn) và để nó nhuộm đen toàn bộ 4 lời khen ngợi xuất sắc của các giám khảo khác."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tình huống): Minh gửi email đề xuất ý tưởng cho sếp. Sau 4 giờ chưa thấy sếp hồi âm, Minh bắt đầu bồn chồn lo lắng và nghĩ: 'Chắc chắn sếp thấy mình kém cỏi và chuẩn bị đuổi việc mình'. Lỗi nhận thức nào đang vận hành ở đây?",
                        "Đọc suy nghĩ (Mind Reading) kết hợp Kết luận vội vàng (Jumping to Conclusions)",
                        "Tư duy phản biện khoa học dựa trên dữ liệu định lượng",
                        "Giao tiếp phi bạo lực NVC",
                        "Ứng dụng xuất sắc mô hình Baddeley",
                        "A",
                        "Minh tự gán cho sếp những suy nghĩ tiêu cực về bản thân ('sếp thấy mình kém cỏi') mà không hề có bất kỳ bằng chứng cụ thể nào. Thực tế có thể sếp đang bận họp hội đồng quản trị hoặc chưa kịp mở hộp thư."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Kỹ thuật): Cấu trúc của 'Nhật ký suy nghĩ 4 cột' (Thought Record) trong thực hành CBT gồm những cột nào?",
                        "1. Tên kẻ thù; 2. Tội trạng; 3. Hình phạt đề xuất; 4. Ngày thi hành",
                        "1. Tình huống kích hoạt (A); 2. Suy nghĩ tự động tiêu cực (B); 3. Cảm xúc & Hành vi tương ứng (C); 4. Suy nghĩ cân bằng / Thách thức nhận thức mới dựa trên dữ kiện",
                        "1. Số tiền chi tiêu; 2. Số tiền thu vào; 3. Tỷ lệ lạm phát; 4. Lãi suất ngân hàng",
                        "1. Mục tiêu năm; 2. Danh sách việc cần làm; 3. Người theo dõi; 4. Kết quả đánh giá KPI",
                        "B",
                        "Thought Record là công cụ cốt lõi của CBT: giúp tách rời dòng suy nghĩ tự động ra trang giấy, nhận diện lỗi méo mó và tái cấu trúc thành một suy nghĩ thực tế, cân bằng và có tính xây dựng hơn."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tình huống): Bạn chuẩn bị bước lên bục thuyết trình trước 100 người và tim đập thình thịch. Suy nghĩ tự động xuất hiện: 'Tim mình đập nhanh quá, mình sắp ngất xỉu rồi, mình sẽ làm trò cười cho cả hội trường'. Cách tái cấu trúc nhận thức nào sau đây là khoa học và hữu ích nhất?",
                        "'Tim đập nhanh là dấu hiệu cơ thể đang tiết adrenaline để bơm oxy lên não, chuẩn bị năng lượng tốt nhất cho bài nói; đây là phản ứng hưng phấn hữu ích chứ không phải điềm báo ngất xỉu.'",
                        "Tự nhủ rằng mình hoàn toàn không hồi hộp chút nào và cố nín thở để tim đập chậm lại",
                        "Lập tức chạy trốn khỏi hội trường và thề không bao giờ nói trước đám đông nữa",
                        "Tự tát vào mặt mình nhiều cái để xua đuổi cảm xúc sợ hãi",
                        "A",
                        "Đây là kỹ thuật tái thẩm định kích thích sinh lý (Arousal Reappraisal của Kelly McGonigal): thay vì diễn giải tim đập nhanh là dấu hiệu của 'thảm họa', ta diễn giải nó là sự chuẩn bị sinh học của cơ thể để đối mặt với thử thách, giúp biến lo âu thành sự tập trung sắc bén."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Ứng dụng): CBT có đồng nghĩa với chủ nghĩa 'Tích cực độc hại' (Toxic Positivity - ép buộc phải luôn vui vẻ và phớt lờ nỗi đau thực tế) không?",
                        "Có, CBT dạy con người phải luôn cười nói vui vẻ và coi mọi đau khổ trên đời chỉ là ảo giác không có thật",
                        "Hoàn toàn không; CBT không dạy suy nghĩ tích cực mù quáng, mà hướng tới suy nghĩ thực tế, chính xác và cân bằng dựa trên sự thật khách quan, đồng thời tôn trọng và chấp nhận các cảm xúc tiêu cực chính đáng",
                        "CBT là phương pháp thôi miên ép não bộ quên đi mọi ký ức buồn",
                        "CBT khuyến khích việc đổ lỗi cho nạn nhân khi họ gặp biến cố cuộc đời",
                        "B",
                        "CBT hướng đến tính linh hoạt nhận thức (Cognitive Flexibility) và sự chính xác khách quan. Nếu một người mất đi người thân, cảm xúc đau buồn là hoàn toàn bình thường và cần được trân trọng. CBT chỉ can thiệp vào những suy nghĩ méo mó tự hủy hoại (như tự trách vô cớ hoặc tuyệt vọng hóa tương lai)."
                    ),
                }
            ),
            // =========================================================================
            // 6. Quiz L32: Kiểm Tra Đánh Giá Năng Lực Bài 3.2: Ranh Giới Cá Nhân & Tự Trắc Ẩn (Self-Compassion)
            // =========================================================================
            BuildQuiz(
                "l32",
                SeedDataInitializer.PsychologyLesson32Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 3.2: Ranh Giới Cá Nhân & Tự Trắc Ẩn (Self-Compassion)",
                now.AddSeconds(1),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Tiến sĩ Kristin Neff định nghĩa 3 thành phần cốt lõi của Lòng tự trắc ẩn (Self-Compassion) bao gồm những yếu tố nào?",
                        "1. Tự hào về thành tích; 2. So sánh mình vượt trội hơn người khác; 3. Tự thưởng xa xỉ",
                        "1. Lòng tốt với bản thân (Self-Kindness); 2. Tính nhân loại chung (Common Humanity); 3. Chánh niệm nhận biết (Mindfulness)",
                        "1. Tự thương hại bản thân; 2. Cô lập nỗi đau; 3. Phóng đại cảm xúc bất hạnh",
                        "1. Buông thả kỷ luật; 2. Bỏ qua trách nhiệm công việc; 3. Đổ lỗi cho xã hội",
                        "B",
                        "Kristin Neff xác lập 3 trụ cột: Self-Kindness (đối xử dịu dàng, nâng đỡ bản thân khi thất bại thay vì phán xét cay nghiệt), Common Humanity (nhận thức rằng vấp ngã và khổ đau là trải nghiệm chung của loài người chứ không phải ta bị cô lập), và Mindfulness (quan sát cảm xúc với góc nhìn cân bằng, không né tránh cũng không đồng nhất hóa quá mức)."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Điểm khác biệt quan trọng giữa Lòng tự trắc ẩn (Self-Compassion) và Lòng tự trọng (Self-Esteem) là gì?",
                        "Self-Esteem ổn định và bền vững hơn Self-Compassion trong mọi nghịch cảnh",
                        "Self-Esteem thường phụ thuộc vào thành công, ngoại hình hoặc sự so sánh hơn thua với người khác; trong khi Self-Compassion luôn hiện diện vô điều kiện ngay cả khi ta thất bại, yếu thế hoặc phạm lỗi lầm",
                        "Self-Compassion chỉ làm con người trở nên yếu đuối và lười biếng",
                        "Không có sự khác biệt nào giữa hai khái niệm này",
                        "B",
                        "Nghiên cứu của Neff & Vonk (2009) chỉ ra: Self-Esteem biến động theo thành tích và dễ dẫn đến tự cao tự đại hoặc suy sụp khi bị chê bai; Self-Compassion cung cấp một vùng đệm tâm lý an toàn vững chắc vì nó không đòi hỏi ta phải giỏi hơn người khác mới xứng đáng được yêu thương."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Ba trạng thái của đường biên ranh giới cá nhân (Personal Boundaries) bao gồm:",
                        "Ranh giới xốp/lỏng lẻo (Porous), Ranh giới cứng nhắc/bất khả xâm phạm (Rigid), và Ranh giới lành mạnh/linh hoạt (Healthy)",
                        "Ranh giới sắt, ranh giới đồng và ranh giới nhôm",
                        "Ranh giới vĩnh viễn, ranh giới tạm thời và ranh giới ảo",
                        "Ranh giới giàu, ranh giới nghèo và ranh giới trung lưu",
                        "A",
                        "Ranh giới xốp khiến ta không thể nói 'Không', dễ bị người khác lợi dụng và gánh vác cảm xúc hộ người khác; Ranh giới cứng nhắc tạo ra bức tường cô lập, từ chối mọi sự gần gũi; Ranh giới lành mạnh biết rõ giá trị của mình, cởi mở kết nối nhưng kiên quyết bảo vệ không gian và quyền tự chủ cá nhân."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Hội chứng 'Làm hài lòng người khác' (People-Pleasing) xuất phát từ động cơ tâm lý sâu xa nào?",
                        "Lòng vị tha thuần khiết của bậc thánh nhân không màng bản thân",
                        "Nỗi sợ hãi sâu sắc bị từ chối, bị bỏ rơi hoặc sợ đối mặt với xung đột, khiến cá nhân sẵn sàng hy sinh nhu cầu, sức khỏe và giá trị cốt lõi của mình để đổi lấy sự chấp thuận tạm thời của người khác",
                        "Mong muốn thống trị và kiểm soát cuộc sống của mọi người xung quanh",
                        "Trí nhớ làm việc bị suy giảm do thiếu ngủ",
                        "B",
                        "People-Pleasing thực chất là một cơ chế phòng vệ (Fawn response) học được từ quá khứ: người bệnh tin rằng mình chỉ có giá trị khi làm vừa lòng người khác, dẫn đến việc tích tụ cảm xúc ấm ức, kiệt sức và đánh mất bản sắc cá nhân."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tình huống): Sau một buổi phỏng vấn xin việc không thành công, tiếng nói nội tâm chỉ trích cay nghiệt (Inner Critic) vang lên: 'Mày là kẻ thất bại bất tài, cả đời này mày sẽ không bao giờ làm nên trò trống gì!'. Theo nguyên lý Tự trắc ẩn của Kristin Neff, phản hồi nội tâm nào sau đây là chuẩn mực nhất?",
                        "'Mày nói đúng rồi, mình đúng là đồ rác rưởi, thôi nằm một chỗ khỏi cố gắng nữa.'",
                        "'Đây là một khoảnh khắc rất đau lòng và thất vọng (Mindfulness). Việc trượt phỏng vấn là điều mà hầu như bất kỳ ai trên con đường lập nghiệp cũng từng trải qua nhiều lần (Common Humanity). Mình yêu quý và trân trọng sự dũng cảm dám thử sức của bản thân; mình sẽ nghỉ ngơi hôm nay rồi rà soát lại bài học để tiến bộ hơn (Self-Kindness).'",
                        "Tự tát vào mặt mình và thề sẽ nhịn ăn tối để trừng phạt bản thân",
                        "Đổ lỗi rằng công ty đó là lũ lừa đảo và tự cho rằng mình hoàn hảo không có tì vết nào",
                        "B",
                        "Câu B thể hiện trọn vẹn cả 3 trụ cột của Neff: thừa nhận nỗi đau mà không phóng đại (Mindfulness), đặt bản thân vào trải nghiệm chung của loài người (Common Humanity), và dành sự vỗ về ấm áp cùng kế hoạch hành động xây dựng (Self-Kindness)."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tình huống): Một đồng nghiệp thường xuyên nhờ bạn làm hộ việc vào lúc 17h30 chiều dù không phải trách nhiệm của bạn, khiến bạn liên tục phải về trễ và lỡ giờ cơm với gia đình. Đâu là câu từ chối thiết lập ranh giới quyết đoán (Assertive) và chuyên nghiệp nhất?",
                        "'Anh có bị điên không mà việc của anh lại đùn đẩy sang cho tôi làm thế?'",
                        "'Em xin lỗi anh nhiều lắm, em tệ quá, em bất tài quá nên em không làm giúp anh được, anh đừng giận em nhé!'",
                        "'Rất tiếc là sau 17h30 em có kế hoạch gia đình cố định nên không thể hỗ trợ anh làm thêm việc này được. Chúc anh sớm hoàn thành công việc nhé!'",
                        "Nhận lời làm hộ trong sự tức tối và lên mạng xã hội viết status bóng gió chửi đồng nghiệp",
                        "C",
                        "Câu C tuân thủ nguyên tắc thiết lập ranh giới lành mạnh: ngắn gọn, lịch thiệp, nêu rõ giới hạn thời gian mà không cần viện lý do vòng vo hay xin lỗi thừa thãi, thể hiện sự tự trọng và tôn trọng đối phương."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Kỹ thuật): Kỹ thuật 'Đĩa hát xước' (Broken Record Technique) trong duy trì ranh giới cá nhân được thực hiện như thế nào khi gặp đối tượng ngoan cố ép buộc?",
                        "Hét to vào tai đối phương cho đến khi họ chịu bỏ đi",
                        "Điềm tĩnh lặp lại thông điệp từ chối cốt lõi bằng cùng một tông giọng trung tính, không bị cuốn vào việc giải thích dài dòng hay tranh cãi với những lời thao túng tâm lý của đối phương",
                        "Mua một chiếc đĩa than bị xước tặng cho người đó",
                        "Đổi sang một chủ đề hoàn toàn không liên quan như thời tiết ở Bắc Cực",
                        "B",
                        "Kỹ thuật 'Broken Record' vô hiệu hóa các chiêu trò thao túng cảm giác tội lỗi (Guilt-tripping). Bằng cách lặp lại bình thản cùng một câu từ chối ('Tôi hiểu bạn đang vội, nhưng tôi không thể nhận việc này'), ta không cung cấp thêm nguyên liệu cho đối phương bẻ cong lập luận."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tình huống): Nghiên cứu của Breines & Chen (2012) tại Đại học California, Berkeley về tác động của Lòng tự trắc ẩn đối với động lực sửa sai đã đem lại phát hiện khoa học nào?",
                        "Tự trắc ẩn làm sinh viên lười biếng hơn và điểm số tụt dốc thảm hại",
                        "Những người thực hành tự trắc ẩn sau thất bại có động lực học tập cao hơn, dành nhiều thời gian ôn luyện hơn và có niềm tin phát triển mạnh mẽ hơn so với nhóm tự chỉ trích bản thân gay gắt",
                        "Tự chỉ trích tàn nhẫn là cách duy nhất để con người đạt được thành công bền vững",
                        "Lòng tự trắc ẩn chỉ có tác dụng đối với động vật linh trưởng",
                        "B",
                        "Trái với định kiến cho rằng 'không tự chửi bới mình thì sẽ lười biếng', thực nghiệm của Breines & Chen chứng minh: lòng trắc ẩn tạo ra cảm giác an toàn tâm lý, giải phóng não bộ khỏi nỗi sợ hãi thất bại bị trừng phạt, từ đó thôi thúc con người nỗ lực học hỏi và kiên trì sửa chữa lỗi lầm hơn hẳn nhóm tự sỉ nhục."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tình huống): Lan luôn cảm thấy vô cùng tội lỗi mỗi khi từ chối lời mời đi chơi của bạn bè, dù cơ thể Lan đang sốt nhẹ và kiệt sức. Niềm tin sai lệch (Core Belief) nào đang chi phối phản ứng này của Lan?",
                        "'Nhu cầu của người khác quan trọng hơn sức khỏe của mình; nếu mình từ chối, mình là một người ích kỷ và xấu xa.'",
                        "'Sức khỏe thể chất là nền tảng quan trọng nhất để duy trì các mối quan hệ bền vững.'",
                        "'Từ chối là một kỹ năng giao tiếp cơ bản của người trưởng thành.'",
                        "'Bạn bè sẽ luôn thông cảm khi mình mệt mỏi.'",
                        "A",
                        "Đây là niềm tin cốt lõi độc hại của người có ranh giới xốp (Porous Boundaries). Họ đồng nhất việc thiết lập ranh giới tự chăm sóc bản thân với 'sự ích kỷ', dẫn đến cảm giác tội lỗi giả tạo (False Guilt) mỗi khi phải bảo vệ sức khỏe của chính mình."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Ứng dụng): Bài tập 'Self-Compassion Break' (Khoảng dừng tự trắc ẩn) gồm 3 câu tự nhủ ngắn gọn có thể thực hành trong 1 phút giữa cuộc sống bận rộn tương ứng với 3 trụ cột của Neff là:",
                        "1. 'Mày phải mạnh mẽ lên!'; 2. 'Không ai thương mày bằng mày đâu!'; 3. 'Phải làm việc gấp đôi!'",
                        "1. 'Đây là một khoảnh khắc khó khăn (Chánh niệm)'; 2. 'Khó khăn là một phần không thể tránh khỏi của cuộc sống con người (Nhân loại chung)'; 3. 'Mong mình hãy dịu dàng và đối xử tốt với chính mình lúc này (Lòng tốt với bản thân)'",
                        "1. 'Kệ mọi sự đời!'; 2. 'Ai sai mặc ai!'; 3. 'Đi ngủ thôi!'",
                        "1. 'Tiền là tất cả'; 2. 'Thành công xóa nhòa thất bại'; 3. 'Không được phép khóc'",
                        "B",
                        "Công thức 3 câu của Kristin Neff giúp kích hoạt phản xạ tự xoa dịu nhanh chóng của hệ thần kinh, chuyển đổi trạng thái sinh học từ căng thẳng (hạch hạnh nhân) sang an dịu (kích hoạt oxytocin và hệ thần kinh phó giao cảm) ngay giữa ngày làm việc áp lực."
                    ),
                }
            ),
            // =========================================================================
            // 7. Quiz FINAL: Bài Thi Tổng Kết Khóa Học: Tâm Lý Học Ứng Dụng & Khoa Học Trí Nhớ
            // =========================================================================
            BuildQuiz(
                "final",
                SeedDataInitializer.PsychologyLesson32Id,
                "Bài Thi Tổng Kết Khóa Học: Tâm Lý Học Ứng Dụng & Khoa Học Trí Nhớ",
                now.AddSeconds(1),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Tổng hợp - Hệ thống 1 & 2): Bản chất của việc làm chủ nhận thức và cảm xúc không phải là tiêu diệt Hệ thống 1, mà là:",
                        "Rèn luyện khả năng nhận biết thời điểm Hệ thống 1 phát tín hiệu bốc đồng để kích hoạt khoảng dừng chánh niệm (Mindful Pause), tạo điều kiện cho Hệ thống 2 can thiệp thẩm định logic và đưa ra phản hồi phù hợp",
                        "Luôn luôn ép buộc bản thân phải tính toán toán học trong mọi tình huống mua sắm hay chào hỏi hàng ngày",
                        "Phẫu thuật cắt bỏ hạch hạnh nhân để không bao giờ cảm thấy sợ hãi hay giận dữ",
                        "Tin tưởng mù quáng vào giác quan thứ sáu trong các quyết định tài chính và sự nghiệp lớn",
                        "A",
                        "Hệ thống 1 là kỳ quan tiến hóa giúp con người sinh tồn và phản xạ nhanh. Làm chủ nhận thức là khả năng tự quan sát (Metacognition) để phát hiện khi nào trực giác đang bị dẫn dụ bởi các bẫy thiên kiến, từ đó chủ động trao quyền điều khiển cho Hệ thống 2."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Tổng hợp - Thiên kiến nhận thức): Khi một người vừa tin vào thuyết âm mưu, vừa gạt bỏ mọi số liệu khoa học chính thống, vừa neo vào tin đồn đầu tiên nghe được trên mạng xã hội, người đó đang chịu sự chi phối tổng hợp của:",
                        "Thiên kiến xác nhận (Confirmation Bias) kết hợp Hiệu ứng mỏ neo (Anchoring Effect) và Tư duy lọc tiêu cực (Mental Filter)",
                        "Năng lực tư duy phản biện độc lập bậc cao",
                        "Ứng dụng chuẩn mực của thuật toán FSRS v5",
                        "Ranh giới cá nhân linh hoạt và lành mạnh",
                        "A",
                        "Tin đồn đầu tiên đóng vai trò là mỏ neo (Anchoring); sau đó người này chỉ đi tìm các bài viết ủng hộ tin đồn đó và phớt lờ số liệu khoa học (Confirmation Bias), đồng thời lọc bỏ các dữ kiện thực tế xung quanh (Mental Filter)."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Tổng hợp - Trí nhớ & Tải nhận thức): Để thiết kế một bài thuyết trình chuyên môn giúp khán giả tiếp thu sâu và không bị quá tải bộ nhớ làm việc (Working Memory), diễn giả nên áp dụng nguyên lý nào?",
                        "Chiếu slide đặc kín chữ và đọc lại nguyên văn từng chữ trên màn hình",
                        "Triệt tiêu Tải ngoại lai (Extraneous Load) bằng slide trực quan cô đọng (Mã hóa kép Dual Coding), phân cụm kiến thức thành 3–4 khối chính (Chunking), và tạo cơ hội cho khán giả tự tương tác trả lời câu hỏi (Active Recall)",
                        "Nói liên tục không ngừng nghỉ trong 3 tiếng với tốc độ 300 từ/phút",
                        "Sử dụng thật nhiều thuật ngữ tiếng nước ngoài không giải thích để thể hiện uyên bác",
                        "B",
                        "Sự kết hợp giữa Dual Coding (hình ảnh + lời giải thích), Chunking (3–4 đơn vị vừa vặn dung lượng Cowan), và Active Recall (tương tác truy hồi) tối ưu hóa toàn diện Thuyết tải nhận thức của John Sweller."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Tổng hợp - Khoa học ghi nhớ FSRS): Sự khác biệt căn bản giữa một người học 'chăm chỉ nhưng sai phương pháp' và một người học 'thông minh theo khoa học nhận thức' khi chuẩn bị cho kỳ thi lớn là gì?",
                        "Người học sai phương pháp dành toàn bộ thời gian đọc đi đọc lại sách thụ động và học dồn sát ngày thi; người học thông minh chia nhỏ thời gian ôn tập giãn cách (Spaced Repetition) và liên tục tự kiểm tra bằng flashcard truy hồi chủ động (Active Recall)",
                        "Người học thông minh chỉ cần ngủ qua đêm với cuốn sách đặt dưới gối là nhớ được toàn bộ kiến thức",
                        "Không có sự khác biệt nào vì trí nhớ hoàn toàn là do gen di truyền quyết định",
                        "Người học chăm chỉ luôn đạt điểm cao hơn người áp dụng kỹ thuật ngắt quãng",
                        "A",
                        "Đường cong Ebbinghaus chứng minh việc đọc lại thụ động tạo ra ảo tưởng thành thạo (Illusion of Competence) nhưng quên sạch sau bài thi. Ôn tập ngắt quãng với Active Recall là phương pháp duy nhất được khoa học thực nghiệm chứng minh bảo tồn ký ức vĩnh cửu vào Long-term Memory."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tổng hợp - CBT & Tái cấu trúc nhận thức): Khi bạn gặp một sự cố công việc và suy nghĩ tự động xuất hiện: 'Mình làm hỏng tất cả rồi, mình là kẻ thất bại', quy trình 3 bước xử lý theo CBT chuẩn mực là:",
                        "1. Tin ngay vào suy nghĩ đó; 2. Nằm khóc cả ngày; 3. Nộp đơn xin nghỉ việc",
                        "1. Nhận diện suy nghĩ tự động và gọi tên lỗi nhận thức (Trắng - đen & Dán nhãn); 2. Thu thập bằng chứng khách quan ủng hộ và phản bác suy nghĩ đó; 3. Xây dựng suy nghĩ thay thế cân bằng và hành động từng bước nhỏ giải quyết vấn đề",
                        "1. Đổ hết lỗi cho cấp dưới; 2. Xóa sạch dữ liệu dự án; 3. Giả vờ như không có chuyện gì xảy ra",
                        "1. Tự phạt bản thân nhịn ăn; 2. Làm việc xuyên đêm; 3. Tránh mặt mọi người",
                        "B",
                        "Quy trình CBT kinh điển: Bắt suy nghĩ (Catch it) -> Kiểm tra chứng cứ (Check it) -> Thay đổi nhận thức cân bằng (Change it)."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tổng hợp - NVC & Giao tiếp trắc ẩn): Khi cần góp ý cho một người bạn thường xuyên hủy hẹn vào phút chót, thông điệp nào dưới đây thể hiện sự kết hợp hoàn hảo giữa NVC và Ranh giới cá nhân lành mạnh?",
                        "'Cậu là đồ bạn tồi, lúc nào cũng vô tâm và ích kỷ!'",
                        "'Trong tháng này có 3 lần cậu báo hủy hẹn trước giờ gặp dưới 30 phút (Quan sát). Mình cảm thấy hụt hẫng và thời gian của mình không được trân trọng (Cảm xúc & Nhu cầu). Nếu lần tới có việc bận, cậu có thể báo trước cho mình ít nhất 3 tiếng được không; nếu tiếp tục hủy sát giờ mình sẽ không thể lên lịch hẹn riêng với cậu nữa (Đề nghị & Ranh giới rõ ràng).'",
                        "Im lặng không nói gì nhưng trong lòng ấm ức và âm thầm hủy kết bạn trên mạng xã hội",
                        "Cười trừ bảo không sao đâu nhưng lần sau rủ lại từ chối với lý do bịa đặt",
                        "B",
                        "Câu B tuân thủ hoàn hảo 4 bước NVC (Quan sát dữ kiện, Nêu cảm xúc, Bày tỏ nhu cầu trân trọng thời gian, Đưa ra đề nghị cụ thể) đồng thời tuyên bố rõ ranh giới bảo vệ bản thân nếu hành vi tái diễn mà không hề xúc phạm đối phương."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tổng hợp - Tự trắc ẩn & Thất bại): Một người vừa khởi nghiệp thất bại và mất sạch số vốn tiết kiệm. Thay vì chìm vào rượu chè hoặc tự sỉ nhục bản thân, người đó dành cho mình một khoảng lặng tự trắc ẩn và bắt đầu phân tích bài học kinh nghiệm. Yếu tố tâm lý mấu chốt giúp người này không bị sụp đổ là:",
                        "Khả năng nhận thức Tính nhân loại chung (Common Humanity) - hiểu rằng rủi ro và vấp ngã là quy luật tự nhiên của thương trường mà bao doanh nhân kiệt xuất đều trải qua - kết hợp với Lòng tốt với bản thân (Self-Kindness)",
                        "Sự vô cảm tuyệt đối và không coi tiền bạc ra gì",
                        "Ảo tưởng thành thạo cho rằng mình không hề có lỗi sai nào",
                        "Ranh giới xốp khiến người đó không bận tâm đến hậu quả tài chính",
                        "A",
                        "Common Humanity cứu vãn cá nhân khỏi cái bẫy cô lập nỗi đau ('chỉ có mình bất tài mới thất bại nhục nhã thế này'). Khi nhận diện vấp ngã là một phần tất yếu của quá trình học hỏi của con người, cá nhân phục hồi tinh thần và dũng cảm tái thiết lập mục tiêu."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tổng hợp - Ranh giới & Công sở): Khi sếp liên tục nhắn tin giao việc lúc 23h đêm và yêu cầu trả lời ngay, việc một nhân viên dám lịch thiệp trao đổi lại vào sáng hôm sau về khung giờ làm việc và cam kết chất lượng thể hiện:",
                        "Sự ngạo mạn, vô kỷ luật và thiếu tôn trọng cấp trên",
                        "Năng lực thiết lập Ranh giới thích ứng (Adaptive Boundary) lành mạnh, bảo vệ sức khỏe tâm thần và giấc ngủ để duy trì hiệu suất làm việc chuyên nghiệp, bền vững lâu dài",
                        "Biểu hiện của hội chứng People-Pleasing ở mức độ nặng",
                        "Sự cạn kiệt tài nguyên của Hệ thống 1",
                        "B",
                        "Ranh giới thích ứng nơi công sở không phải là sự chống đối, mà là việc định hình kỳ vọng chuyên nghiệp. Làm việc 24/7 chỉ dẫn đến kiệt quệ (Burnout) và sai sót nghiêm trọng. Thiết lập ranh giới nghỉ ngơi giúp duy trì năng lượng cho Hệ thống 2 hoạt động sắc bén trong giờ làm việc."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tổng hợp - Siêu nhận thức Metacognition): Năng lực cốt lõi bao trùm toàn bộ khóa học 'Tâm lý học Ứng dụng & Khoa học Trí nhớ' là Siêu nhận thức (Metacognition - Tư duy về tư duy), được hiểu là:",
                        "Khả năng tự quan sát, theo dõi, đánh giá và chủ động điều chỉnh các tiến trình nhận thức, cảm xúc, mô thức ghi nhớ và hành vi của chính mình để đạt hiệu quả sống và làm việc tối ưu",
                        "Khả năng đọc được suy nghĩ của người khác từ khoảng cách 100 mét",
                        "Việc học thuộc lòng toàn bộ từ điển bách khoa toàn thư mà không cần hiểu nghĩa",
                        "Thói quen suy nghĩ luẩn quẩn và tự dằn vặt về quá khứ mỗi đêm",
                        "A",
                        "Metacognition (Flavell, 1979) là tầng nhận thức cấp cao: đứng lùi lại một bước để quan sát chính bộ não mình đang vận hành. Biết khi nào mình đang thiên kiến, biết khi nào trí nhớ đang phai mờ, biết khi nào tiếng nói chỉ trích đang phóng đại - đó chính là đỉnh cao của tự do tâm trí."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tổng hợp - Tuyên ngôn ứng dụng): Thông điệp hành động xuyên suốt của khóa học để chuyển hóa tri thức tâm lý học thành sự trưởng thành nội tâm thực sự là:",
                        "'Chỉ cần đọc sách tâm lý học thật nhiều là cuộc đời sẽ tự động trở nên hoàn hảo không cần hành động.'",
                        "'Dừng lại để nhận diện (Chánh niệm) → Thách thức sự méo mó bằng dữ kiện (CBT/Khoa học nhận thức) → Ôn tập ngắt quãng để kiến thức ăn sâu vào tiềm thức (FSRS) → Giao tiếp chân thành và bảo vệ bản thân bằng ranh giới trắc ẩn (NVC & Self-Compassion).'",
                        "'Luôn nghi ngờ tất cả mọi người và sống tách biệt hoàn toàn khỏi xã hội để an toàn tuyệt đối.'",
                        "'Cố gắng làm hài lòng tất cả mọi người xung quanh để không bao giờ bị ai chê trách.'",
                        "B",
                        "Đây là chuỗi liên hoàn tinh hoa tích hợp toàn bộ các phương pháp của khóa học: từ nhận thức (Mindfulness/Hệ thống 2), thẩm định lý trí (CBT/Khoa học), củng cố trí nhớ lâu dài (FSRS/Spaced Repetition), cho tới hành vi đối thoại xã hội (NVC) và tình yêu thương chính mình (Self-Compassion)."
                    ),
                }
            ),
        };
    }
}
