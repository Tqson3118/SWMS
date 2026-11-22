namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
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

        return list;
    }
}
