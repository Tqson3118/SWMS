namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Swms.Api.Domain.Entities;

/// <summary>
/// Expanded Write-in Practice Content for all 6 lessons of Investing &amp; Portfolio Management Course.
/// Ensures EVERY lesson has &gt;= 8 write-in activities (OrderIndex 3..10):
/// - hand_calc (&gt;= 2 activities): covering compound, dca, pe, margin_of_safety, drawdown
/// - investment_case (&gt;= 3 activities/scenarios): options, correctIndex, reasonPromptVi, expectedKeywords, sampleExplanation
/// - decision_journal (&gt;= 3 activities/prompts): 4-pillar structured journal prompts.
/// </summary>
public static class InvestingWriteInContent
{
    public static List<PracticeActivity> GetActivities(DateTimeOffset now)
    {
        var list = new List<PracticeActivity>();
        var courseId = SeedDataInitializer.InvestingCourseId;

        // =========================================================================
        // LESSON 1.1: Sức Mạnh Lãi Kép & Chiến Lược DCA Định Kỳ
        // (Legacy activities: OrderIndex 1, 2; Starter Write-in: OrderIndex 3, 4, 5)
        // OrderIndex 3: hand_calc (compound, dca, pe, margin_of_safety, drawdown)
        // OrderIndex 4: investment_case (case 1)
        // OrderIndex 5: decision_journal (journal 1)
        // We supplement with OrderIndex 6..10 so Lesson 1.1 has 8 write-in activities.
        // =========================================================================
        var l11 = SeedDataInitializer.InvestingLesson11Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "investment_case", 6),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 6,
            Type = "investment_case",
            Title = "Tình Huống Đầu Tư: Lạm Phát Bào Mòn Tiền Tiết Kiệm & Thời Điểm DCA",
            Description = "Phân tích tình huống thực tế và tự viết giải thích bằng lời theo nguyên lý tài chính.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l11-case-2",
                        title = "Giữ 100% tiền mặt trong 10 năm lạm phát 4.5%/năm",
                        scenario = "Một người có 500 triệu đồng tiền nhàn rỗi dự định gửi tiết kiệm kỳ hạn ngắn liên tục trong 10 năm với lãi suất 5.5%/năm, trong khi lạm phát thực tế trung bình là 4.5%/năm. Đây có phải chiến lược bảo toàn tài sản tối ưu không?",
                        options = new[]
                        {
                            "Rất tối ưu vì tiền gửi ngân hàng được nhà nước bảo hiểm và không bao giờ mất giá danh nghĩa",
                            "Không tối ưu vì lợi suất thực tế chỉ đạt ~1%/năm, sức mua gia tăng không đáng kể; nên phân bổ một phần sang tài sản sinh lời vượt lạm phát",
                            "Nên rút toàn bộ mua vé số hoặc tiền điện tử lướt sóng",
                            "Nên cất tiền mặt vào két sắt để tránh thuế"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích vì sao giữ tiền mặt thuần túy trong dài hạn lại gặp rủi ro sức mua trước lạm phát:",
                        expectedKeywords = new[] { "lạm phát", "sức mua", "lợi suất thực tế", "bào mòn", "tích lũy" },
                        sampleExplanation = "Lợi suất thực tế chỉ bằng lãi suất danh nghĩa trừ đi lạm phát (~1%/năm). Sau 10 năm, sức mua của 500 triệu hầu như không tăng trưởng, do đó cần tích lũy DCA vào các tài sản tăng trưởng để bảo vệ sức mua dài hạn.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "investment_case", 7),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 7,
            Type = "investment_case",
            Title = "Tình Huống Đầu Tư: Bắt Đầu Sớm Số Tiền Nhỏ vs Chờ Có Vốn Lớn",
            Description = "Lựa chọn phương án và giải thích sức mạnh thời gian trong công thức lãi kép.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l11-case-3",
                        title = "Bắt đầu DCA sớm với 1.5 triệu/tháng vs chờ 30 tuổi có 100 triệu",
                        scenario = "Bạn 22 tuổi vừa ra trường, mỗi tháng chỉ dư được 1.5 triệu đồng. Một người bạn khuyên nên đợi đến năm 30 tuổi khi lương cao có 100 triệu mới đầu tư một thể. Lựa chọn nào có lợi hơn cho lãi kép?",
                        options = new[]
                        {
                            "Đợi 30 tuổi có cục tiền to đầu tư một lần sẽ kiếm được nhiều tiền hơn",
                            "Bắt đầu DCA ngay 1.5 triệu/tháng từ năm 22 tuổi để tận dụng tối đa 8 năm tích lũy thời gian của lãi kép",
                            "Dùng 1.5 triệu mua hàng hiệu để nâng cao hình ảnh cá nhân",
                            "Vay nóng để có 100 triệu đầu tư ngay hôm nay"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích tại sao yếu tố thời gian lại quan trọng hơn quy mô vốn ban đầu trong công thức lãi kép:",
                        expectedKeywords = new[] { "thời gian", "lãi kép", "hàm mũ", "bắt đầu sớm", "dca" },
                        sampleExplanation = "Trong công thức lãi kép FV = PV * (1 + r)^n, biến số số chu kỳ n nằm ở số mũ. Bắt đầu sớm 8 năm giúp lãi kép có đủ thời gian để kích hoạt giai đoạn tăng trưởng bùng nổ, vượt trội hơn việc vào vốn muộn.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "decision_journal", 8),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 8,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Đối Phó Khi Thị Trường Tăng Nóng & Muốn FOMO",
            Description = "Viết 4 trụ cột kỷ luật để ngăn chặn tâm lý muốn rút quỹ khẩn cấp ra tất tay theo đà tăng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l11-dj-2",
                        title = "Nhật ký: Đối phó khi thị trường tăng nóng và muốn FOMO tất tay",
                        scenario = "Thị trường vừa tăng liên tiếp 3 tháng, bạn đang thực hiện DCA 4 triệu/tháng nhưng cảm thấy quá chậm và muốn rút quỹ khẩn cấp ra tất tay một lần.",
                        expectedKeywords = new[] { "fomo", "kỷ luật", "dca", "quỹ khẩn cấp", "rủi ro", "dài hạn" },
                        sampleAnswer = "Luận điểm: DCA là chiến lược tích lũy dài hạn, không phải lướt sóng bắt đỉnh. Định giá: Thị trường đã tăng nóng 3 tháng, định giá không còn rẻ. Kịch bản sai: Nếu rút quỹ khẩn cấp mà thị trường điều chỉnh 20%, bản thân sẽ vừa mất thanh khoản vừa gánh lỗ. Kỷ luật: Giữ nguyên mức DCA 4 triệu/tháng, bảo toàn tuyệt đối quỹ dự phòng."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "decision_journal", 9),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 9,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Lộ Trình Tăng Tỷ Lệ Tiết Kiệm Ròng Từ 15% Lên 30%",
            Description = "Viết 4 trụ cột thẩm định kế hoạch tối ưu hóa chi phí để gia tăng nguồn vốn DCA hàng tháng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l11-dj-3",
                        title = "Nhật ký: Lộ trình tăng tỷ lệ tiết kiệm ròng từ 15% lên 30%",
                        scenario = "Xây dựng kế hoạch tối ưu chi phí sinh hoạt và chuyển đổi dòng tiền thặng dư hàng tháng vào danh mục DCA trong 12 tháng tới.",
                        expectedKeywords = new[] { "dòng tiền", "tiết kiệm ròng", "dca", "chi phí thiết yếu", "lãi kép" },
                        sampleAnswer = "Luận điểm: Gia tăng tỷ lệ tiết kiệm ròng giúp rút ngắn thời gian tự do tài chính theo cấp số nhân. Định giá: Rà soát cắt giảm chi tiêu không thiết yếu để tăng thêm 3 triệu/tháng cho DCA. Kịch bản sai: Nếu thu nhập sụt giảm, duy trì tỷ lệ tối thiểu 15%. Kỷ luật: Chuyển tiền tự động vào ngày mùng 5 hàng tháng."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "hand_calc", 10),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 10,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Lãi Kép & Tích Lũy DCA Nâng Cao",
            Description = "Luyện tính nhẩm và bấm máy tính công thức tích lũy tài sản đều hàng tháng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "compound", "dca" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "investment_case", 11),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 11,
            Type = "investment_case",
            Title = "Tình Huống Khởi Động: Gửi Tiết Kiệm 100 Triệu Đồng vs Lạm Phát Sau 3 Năm",
            Description = "Phân tích tình huống thực tế về sức mua của tiền gửi tiết kiệm và ý nghĩa của việc đầu tư gia tăng tài sản.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l11-case-warmup",
                        title = "Tính toán sức mua 100 triệu sau 3 năm gửi tiết kiệm 5%/năm khi lạm phát 4%/năm",
                        scenario = "Một người có 100 triệu đồng gửi tiết kiệm liên tục trong 3 năm với lãi suất 5.0%/năm. Cùng giai đoạn đó, lạm phát trung bình là 4.0%/năm. Số tiền danh nghĩa sau 3 năm là bao nhiêu và sức mua thực tế gia tăng thế nào?",
                        options = new[]
                        {
                            "Số tiền danh nghĩa sau 3 năm đạt 115.76 triệu đồng; tuy nhiên do lạm phát 4%/năm, sức mua thực tế chỉ tương đương khoảng 102.9 triệu đồng (lợi suất thực ~1%/năm), chứng minh giữ tiền thuần túy không đủ làm giàu nhanh",
                            "Số tiền danh nghĩa bị giảm xuống dưới 100 triệu do lạm phát ăn mòn trực tiếp số dư tài khoản ngân hàng",
                            "Sức mua thực tế tăng gấp đôi sau 3 năm nhờ hiệu ứng ghép lãi của ngân hàng",
                            "Ngân hàng sẽ tự động điều chỉnh trả lãi 9%/năm để bù đắp hoàn toàn lạm phát"
                        },
                        correctIndex = 0,
                        reasonPromptVi = "Giải thích sự khác biệt giữa tăng trưởng danh nghĩa và tăng trưởng sức mua thực tế:",
                        expectedKeywords = new[] { "danh nghĩa", "sức mua", "lạm phát", "lợi suất thực", "bảo toàn" },
                        sampleExplanation = "Sau 3 năm với lãi suất 5%, số tiền danh nghĩa đạt 100 * (1.05)^3 = 115.76 triệu đồng. Nhưng sức mua thực tế sau khi điều chỉnh lạm phát 4% là 115.76 / (1.04)^3 ≈ 102.91 triệu đồng. Mức tăng trưởng thực chỉ khoảng ~1%/năm, do đó cần đầu tư DCA vào tài sản sinh lời cao hơn để tăng trưởng của cải thực sự.",
                        minWords = 15
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });


        // =========================================================================
        // LESSON 1.2: Kế Hoạch Tự Do Tài Chính (FIRE) & Quản Lý Dòng Tiền
        // (Legacy activities: OrderIndex 1, 2)
        // Write-in activities: OrderIndex 3..10 (8 activities)
        // =========================================================================
        var l12 = SeedDataInitializer.InvestingLesson12Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "hand_calc", 3),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 3,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Con Số FIRE Mục Tiêu & Tích Lũy DCA",
            Description = "Bấm máy tính tính toán số tiền tích lũy cần thiết theo quy tắc 4% và dòng tiền hàng tháng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "compound", "dca" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "investment_case", 4),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 4,
            Type = "investment_case",
            Title = "Tình Huống: Quản Lý Nợ Lãi Suất Cao vs Độc Lập Tài Chính FIRE",
            Description = "Lựa chọn phương án tối ưu dòng tiền và viết lời giải thích chi tiết.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l12-case-1",
                        title = "Khoản nợ tín dụng 40 triệu lãi suất 28%/năm",
                        scenario = "Bạn vừa nhận được khoản thưởng cuối năm 50 triệu đồng. Bạn đang có khoản nợ thẻ tín dụng 40 triệu đồng lãi suất 28%/năm. Một người bạn khuyên nên giữ nợ trả dần tối thiểu và đem 50 triệu đi mua cổ phiếu lướt sóng.",
                        options = new[]
                        {
                            "Đem toàn bộ 50 triệu đi lướt sóng để hy vọng nhân đôi trả nợ sau",
                            "Tất toán dứt điểm toàn bộ 40 triệu nợ thẻ tín dụng ngay lập tức, 10 triệu còn lại chuyển vào quỹ khẩn cấp",
                            "Chia nhỏ mỗi tháng trả một ít để duy trì điểm tín dụng",
                            "Đi du lịch nghỉ dưỡng giải tỏa căng thẳng"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích lý do tại sao trả sạch nợ thẻ tín dụng lại là quyết định đầu tư có lợi suất phi rủi ro cao nhất:",
                        expectedKeywords = new[] { "lãi suất cao", "phi rủi ro", "nợ thẻ", "dòng tiền", "bào mòn" },
                        sampleExplanation = "Lãi suất 28%/năm là khoản chi phí tài chính chắc chắn phải trả. Trả dứt điểm nợ thẻ tương đương với việc đạt tỷ suất sinh lời phi rủi ro 28% mỗi năm, vượt trội hơn hầu hết mọi kênh đầu tư hợp pháp.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "investment_case", 5),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 5,
            Type = "investment_case",
            Title = "Tình Huống: Quy Tắc 4% Khi Thị Trường Bước Vào Suy Thoái Kéo Dài",
            Description = "Phân tích rủi ro chuỗi lợi nhuận (Sequence of Returns Risk) và đưa ra quyết định.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l12-case-2",
                        title = "Quy tắc 4% khi thị trường bước vào suy thoái kéo dài",
                        scenario = "Anh Nam vừa đạt tự do tài chính với danh mục 6 tỷ đồng (chi tiêu 240 triệu/năm, rút 4%). Năm đầu tiên nghỉ hưu, thị trường giảm 20% khiến danh mục còn 4.8 tỷ. Anh Nam nên điều chỉnh ra sao?",
                        options = new[]
                        {
                            "Vẫn rút đủ 240 triệu cộng thêm lạm phát như bình thường",
                            "Chủ động thắt lưng buộc bụng, cắt giảm chi tiêu cá nhân xuống 180 triệu và tìm thêm việc freelance nhẹ để không phải bán tháo chứng chỉ quỹ ở vùng đáy",
                            "Bán sạch toàn bộ danh mục 4.8 tỷ chuyển sang tiền mặt",
                            "Vay nợ ngân hàng để tiêu xài"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích rủi ro chuỗi lợi nhuận (Sequence of Returns Risk) và cách xử lý:",
                        expectedKeywords = new[] { "chuỗi lợi nhuận", "bán tháo", "đáy", "cắt giảm chi tiêu", "bảo toàn vốn" },
                        sampleExplanation = "Rút tiền lớn khi danh mục đang sụt giảm ở vùng đáy (Sequence of Returns Risk) sẽ làm hao mòn vốn gốc vĩnh viễn. Việc linh hoạt cắt giảm chi tiêu giúp danh mục có thời gian phục hồi khi thị trường tăng trở lại.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "investment_case", 6),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 6,
            Type = "investment_case",
            Title = "Tình Huống: Cạm Bẫy Lạm Phát Lối Sống Khi Thu Nhập Tăng Gấp Đôi",
            Description = "Phân tích tác hại của Lifestyle Inflation đối với cột mốc FIRE.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l12-case-3",
                        title = "Lạm phát lối sống khi thu nhập tăng gấp đôi",
                        scenario = "Sau 3 năm làm việc chăm chỉ, lương của Lan tăng từ 15 triệu lên 30 triệu/tháng. Lan dự định đổi xe tay ga xịn, chuyển sang căn hộ cao cấp và ăn nhà hàng thường xuyên khiến chi tiêu tăng từ 10 triệu lên 25 triệu/tháng.",
                        options = new[]
                        {
                            "Rất bình thường vì kiếm được tiền thì phải nâng tầm phong cách sống",
                            "Đây là cái bẫy 'Lạm phát lối sống' (Lifestyle Inflation), khiến Con số FIRE mục tiêu bị đẩy ra xa gấp đôi; nên cố định chi tiêu cơ bản và dành ít nhất 60% phần tăng thêm để đầu tư",
                            "Nên tiêu hết 30 triệu mỗi tháng để kích cầu kinh tế",
                            "Nên vay mua thêm xe hơi trả góp"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích tác hại của Lạm phát lối sống đối với lộ trình độc lập tài chính:",
                        expectedKeywords = new[] { "lạm phát lối sống", "chi tiêu", "fire", "tỷ lệ tiết kiệm", "tích lũy" },
                        sampleExplanation = "Khi chi tiêu tăng từ 120 triệu lên 300 triệu/năm, số tiền FIRE cần tích lũy tăng từ 3 tỷ lên 7.5 tỷ đồng. Lạm phát lối sống làm triệt tiêu hoàn toàn lợi thế của việc tăng lương.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "decision_journal", 7),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 7,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Xác Lập Con Số FIRE & Kế Hoạch Dòng Tiền 10 Năm",
            Description = "Viết 4 trụ cột thẩm định lộ trình FIRE cá nhân dựa trên chi tiêu thực tế và tỷ lệ tiết kiệm ròng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l12-dj-1",
                        title = "Nhật ký: Xác lập con số FIRE và kế hoạch dòng tiền 10 năm",
                        scenario = "Hãy thẩm định lộ trình FIRE cá nhân dựa trên chi tiêu thực tế hàng năm và tỷ lệ tiết kiệm ròng.",
                        expectedKeywords = new[] { "fire", "quy tắc 4%", "chi tiêu", "tỷ lệ tiết kiệm", "dòng tiền", "kỷ luật" },
                        sampleAnswer = "Luận điểm: Tích lũy đủ 25 lần chi tiêu năm để đạt tự do tài chính. Định giá: Chi tiêu năm 180 triệu -> Con số FIRE mục tiêu là 4.5 tỷ đồng. Kịch bản sai: Nếu thị trường suy thoái khi vừa nghỉ hưu, quỹ dự phòng 2 năm chi tiêu giúp tránh bán tháo tài sản. Kỷ luật: Duy trì tỷ lệ tiết kiệm ròng >= 40% thu nhập hàng tháng."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "decision_journal", 8),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 8,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Chiến Lược Xóa Nợ Tiêu Dùng & Thiết Lập Quỹ Khẩn Cấp",
            Description = "Viết kế hoạch hành động dứt điểm nợ và khóa van rò rỉ tài chính vi mô.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l12-dj-2",
                        title = "Nhật ký: Chiến lược xóa nợ tiêu dùng và thiết lập quỹ khẩn cấp",
                        scenario = "Kế hoạch hành động dứt điểm nợ và khóa van rò rỉ tài chính vi mô.",
                        expectedKeywords = new[] { "trả nợ", "quỹ khẩn cấp", "lãi suất", "cắt giảm", "kỷ luật" },
                        sampleAnswer = "Luận điểm: Xóa nợ lãi cao là bước đệm bắt buộc trước khi đầu tư. Định giá: Trả nợ tương đương đạt lợi suất phi rủi ro 25%/năm. Kịch bản sai: Nếu xảy ra biến cố mất việc trong khi đang trả nợ, quỹ khẩn cấp 3 tháng giúp duy trì sinh hoạt. Kỷ luật: Không mở thêm thẻ tín dụng mới, ưu tiên phương pháp Tuyết lở dứt điểm khoản lãi cao nhất."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "decision_journal", 9),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 9,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Kế Hoạch Tạo Nguồn Thu Nhập Thụ Động Bổ Sung",
            Description = "Thẩm định tính khả thi của việc xây dựng dòng tiền cổ tức và sản phẩm số.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l12-dj-3",
                        title = "Nhật ký: Đa dạng hóa dòng tiền thụ động hướng tới Barbell FIRE",
                        scenario = "Thiết lập kế hoạch kết hợp giữa công việc tự do linh hoạt (Coast FIRE) và danh mục cổ tức tăng trưởng.",
                        expectedKeywords = new[] { "thu nhập thụ động", "cổ tức", "coast fire", "đa dạng hóa", "kỷ luật" },
                        sampleAnswer = "Luận điểm: Kết hợp danh mục cổ tức với công việc chuyên môn yêu thích giúp giảm áp lực tích lũy vốn lớn. Định giá: Mục tiêu cổ tức đạt 5 triệu/tháng sau 5 năm. Kịch bản sai: Nếu doanh nghiệp cắt giảm cổ tức, danh mục tăng trưởng ETF sẽ bù đắp. Kỷ luật: Tái đầu tư 100% cổ tức nhận được trong 10 năm đầu."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "hand_calc", 10),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 10,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Tỷ Lệ Chi Tiêu & Số Năm Cần Tích Lũy Để FIRE",
            Description = "Tính toán số năm cần thiết để tự do tài chính theo tỷ lệ tiết kiệm ròng 20% đến 60%.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "compound", "drawdown" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "investment_case", 11),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 11,
            Type = "investment_case",
            Title = "Tình Huống Khởi Động: Bắt Đầu Lộ Trình FIRE Với Thu Nhập 15 Triệu/Tháng",
            Description = "Phân tích cấu trúc phân bổ dòng tiền và tỷ lệ tiết kiệm ròng cho người mới đi làm hướng tới độc lập tài chính.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l12-case-warmup",
                        title = "Thu nhập 15 triệu/tháng: Bắt đầu tích lũy 2.5 triệu ngay hay chờ lương cao?",
                        scenario = "Một bạn trẻ 23 tuổi mới ra trường có thu nhập 15 triệu đồng/tháng. Sau khi chi trả tiền thuê nhà và sinh hoạt thiết yếu 9 triệu (60%), bạn còn dư 6 triệu. Bạn phân vân nên bắt đầu trích cố định 2.5 triệu đồng (tỷ lệ tiết kiệm ~16.7%) để tích lũy đầu tư dài hạn ngay hay nên tiêu hết và đợi khi lương 30 triệu mới bắt đầu?",
                        options = new[]
                        {
                            "Bắt đầu ngay với 2.5 triệu/tháng để hình thành thói quen kỷ luật và tận dụng tối đa thời gian tích lũy của lãi kép",
                            "Nên tiêu hết 6 triệu vào mua sắm giải trí vì 2.5 triệu quá nhỏ không đáng để đầu tư",
                            "Vay thêm nợ tín dụng để chi tiêu cho bằng bạn bè",
                            "Đợi đến 35 tuổi khi có lương 40 triệu mới bắt đầu tiết kiệm một thể"
                        },
                        correctIndex = 0,
                        reasonPromptVi = "Giải thích tại sao tỷ lệ tiết kiệm ròng và thói quen tích lũy sớm lại quan trọng hơn mức lương ban đầu:",
                        expectedKeywords = new[] { "kỷ luật", "tiết kiệm", "bắt đầu sớm", "thói quen", "lãi kép" },
                        sampleExplanation = "Bắt đầu sớm với tỷ lệ tiết kiệm 16.7% giúp xây dựng thói quen tài chính lành mạnh và biến thời gian thành đòn bẩy lãi kép mạnh mẽ nhất. Chờ đợi lương cao thường đi kèm bẫy lạm phát lối sống (lifestyle creep), khiến tỷ lệ tiết kiệm không tăng dù thu nhập tăng.",
                        minWords = 15
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });


        // =========================================================================
        // LESSON 2.1: Phân Bổ Tài Sản & Tương Quan Giữa Các Lớp Tài Sản
        // (Legacy activities: OrderIndex 1, 2)
        // Write-in activities: OrderIndex 3..10 (8 activities)
        // =========================================================================
        var l21 = SeedDataInitializer.InvestingLesson21Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "hand_calc", 3),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 3,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Tỷ Trọng Danh Mục & Sụt Giảm Drawdown",
            Description = "Bấm máy tính tính toán mức độ sụt giảm và phục hồi tài sản khi phân bổ danh mục.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "compound", "drawdown" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "investment_case", 4),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 4,
            Type = "investment_case",
            Title = "Tình Huống: Tương Quan Giữa Các Lớp Tài Sản Trong Khủng Hoảng",
            Description = "Lựa chọn chiến lược phân bổ và giải thích cơ chế tương quan âm/thấp giữa cổ phiếu và vàng/trái phiếu.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l21-case-1",
                        title = "Phân bổ 100% vào cổ phiếu công nghệ vs danh mục Barbell đa lớp",
                        scenario = "Một nhà đầu tư trẻ muốn đạt lợi nhuận tối đa nên dồn 100% danh mục vào cổ phiếu công nghệ tăng trưởng nóng. Khi thị trường điều chỉnh mạnh 35%, danh mục bốc hơi hơn 40%.",
                        options = new[]
                        {
                            "Tiếp tục vay margin để mua thêm gấp đôi gỡ lại vốn",
                            "Tái cơ cấu danh mục sang mô hình Barbell: giữ 70% tài sản tăng trưởng và 30% tài sản phòng thủ (trái phiếu, vàng, tiền gửi) để giảm Drawdown",
                            "Bán sạch toàn bộ danh mục ở đáy và thề không bao giờ đầu tư nữa",
                            "Chờ đợi và không làm gì cả dù bị áp lực tâm lý nặng nề"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích vì sao việc đa dạng hóa sang các lớp tài sản có tương quan thấp lại giúp giảm độ biến động mà không triệt tiêu lợi nhuận dài hạn:",
                        expectedKeywords = new[] { "tương quan", "đa dạng hóa", "drawdown", "phòng thủ", "biến động" },
                        sampleExplanation = "Các lớp tài sản như cổ phiếu, trái phiếu và vàng có hệ số tương quan thấp hoặc âm trong các chu kỳ kinh tế khác nhau. Khi cổ phiếu giảm sâu, trái phiếu hoặc vàng thường giữ giá hoặc tăng, giúp giảm tối đa mức sụt giảm Max Drawdown và giữ vững tâm lý cho nhà đầu tư.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "investment_case", 5),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 5,
            Type = "investment_case",
            Title = "Tình Huống: Tái Cân Bằng Danh Mục Định Kỳ (Rebalancing)",
            Description = "Giải thích kỷ luật bán bớt tài sản tăng nóng để mua vào tài sản đang bị định giá thấp.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l21-case-2",
                        title = "Cổ phiếu tăng phi mã làm lệch tỷ trọng mục tiêu 60/40",
                        scenario = "Bạn đặt mục tiêu danh mục 60% Cổ phiếu / 40% Trái phiếu. Sau 1 năm tăng mạnh, tỷ trọng cổ phiếu vọt lên 80%, còn trái phiếu chỉ còn 20%. Bạn nên làm gì vào ngày tái cân bằng?",
                        options = new[]
                        {
                            "Giữ nguyên 80% cổ phiếu hoặc mua thêm vì cổ phiếu đang tăng mạnh",
                            "Chủ động chốt lời một phần cổ phiếu để đưa về 60% và mua thêm trái phiếu lên 40% (hoặc dùng dòng tiền DCA mới rót toàn bộ vào trái phiếu)",
                            "Bán hết sạch cả hai chuyển sang tiền mặt",
                            "Vay thêm tiền mua thêm cổ phiếu đầu cơ"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích cơ chế mua thấp bán cao tự động của nghiệp vụ Tái cân bằng (Rebalancing):",
                        expectedKeywords = new[] { "tái cân bằng", "chốt lời", "mua thấp bán cao", "kỷ luật", "tỷ trọng" },
                        sampleExplanation = "Tái cân bằng buộc nhà đầu tư phải chốt lời tài sản đã tăng giá quá cao và mua thêm tài sản đang có giá hấp dẫn mà không bị cảm xúc chi phối, tự động hiện thực hóa nguyên lý mua thấp bán cao một cách có kỷ luật.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "investment_case", 6),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 6,
            Type = "investment_case",
            Title = "Tình Huống: Vàng Trong Danh Mục Phòng Thủ Chống Lạm Phát Phi Mã",
            Description = "Đánh giá vai trò của kim loại quý và tài sản cứng khi tiền tệ mất giá.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l21-case-3",
                        title = "Phân bổ 5-10% Vàng vật chất trong giai đoạn bất ổn địa chính trị",
                        scenario = "Trong bối cảnh căng thẳng địa chính trị leo thang và lạm phát toàn cầu vượt 8%, một nhà đầu tư băn khoăn có nên phân bổ 10% danh mục vào vàng hay gửi tiết kiệm 100%.",
                        options = new[]
                        {
                            "Gửi tiết kiệm 100% vì được hưởng lãi suất danh nghĩa cố định",
                            "Duy trì tỷ trọng 5-10% vàng như một khoản bảo hiểm tài sản cứng chống lại rủi ro mất giá tiền tệ và bất ổn hệ thống",
                            "Dùng 100% tiền mua vàng lướt sóng đòn bẩy",
                            "Mua ngoại tệ chợ đen không rõ nguồn gốc"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích vai trò của vàng như một công cụ bảo hiểm đuôi (Tail-risk hedge) trong danh mục:",
                        expectedKeywords = new[] { "vàng", "bảo hiểm", "lạm phát", "tài sản cứng", "rủi ro đuôi" },
                        sampleExplanation = "Vàng không tạo ra dòng tiền nhưng có lịch sử hàng ngàn năm bảo toàn sức mua trước lạm phát và suy thoái hệ thống. Phân bổ 5-10% đóng vai trò như hợp đồng bảo hiểm rủi ro đuôi (Tail-risk) cho toàn bộ danh mục tài sản.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "decision_journal", 7),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 7,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Thiết Kế Ma Trận Phân Bổ Danh Mục Cá Nhân",
            Description = "Viết 4 trụ cột thẩm định tỷ trọng cổ phiếu, trái phiếu và tiền mặt phù hợp với khẩu vị rủi ro.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l21-dj-1",
                        title = "Nhật ký: Xác lập tỷ trọng mục tiêu theo mô hình Core & Explore",
                        scenario = "Thiết lập danh mục gồm 80% tài sản lõi (Core: ETF chỉ số đa dạng hóa) và 20% tài sản tìm kiếm cơ hội (Explore: cổ phiếu riêng lẻ hoặc vàng).",
                        expectedKeywords = new[] { "core", "explore", "phân bổ", "tương quan", "rủi ro", "kỷ luật" },
                        sampleAnswer = "Luận điểm: Mô hình Core & Explore bảo vệ danh mục gốc bằng 80% quỹ chỉ số trong khi vẫn giữ 20% linh hoạt tìm kiếm alpha. Định giá: Chỉ phân bổ vốn cho Explore khi P/E thị trường dưới trung bình 5 năm. Kịch bản sai: Nếu phần Explore lỗ 25%, cắt lỗ dứt điểm và đưa vốn về Core. Kỷ luật: Tái cân bằng danh mục bán niên một lần vào tháng 6 và tháng 12."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "decision_journal", 8),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 8,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Kế Hoạch Tái Cân Bằng (Rebalance Policy) Chống Cảm Xúc",
            Description = "Viết quy tắc ngưỡng lệch tỷ trọng (Band Rebalancing) để không cần đoán đỉnh đoán đáy.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l21-dj-2",
                        title = "Nhật ký: Quy tắc tái cân bằng theo dải biên độ 5%",
                        scenario = "Thiết lập kỷ luật tự động kích hoạt tái cân bằng khi bất kỳ lớp tài sản nào lệch quá 5% so với tỷ trọng mục tiêu.",
                        expectedKeywords = new[] { "tái cân bằng", "dải biên độ", "kỷ luật", "chốt lời", "hạ rủi ro" },
                        sampleAnswer = "Luận điểm: Tái cân bằng theo ngưỡng lệch 5% giúp kiểm soát rủi ro biến động mà không tốn phí giao dịch thường xuyên. Định giá: Rà soát danh mục ngày cuối mỗi quý. Kịch bản sai: Nếu thị trường tăng dốc liên tục, dải biên độ giúp chốt lời từng phần an toàn. Kỷ luật: Tuyệt đối tuân thủ lệnh bán phần vượt ngưỡng và mua phần thiếu hụt."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "decision_journal", 9),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 9,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Phân Bổ Quỹ Dự Phòng Khẩn Cấp Bằng Tiền Gửi Linh Hoạt",
            Description = "Thẩm định quy mô quỹ khẩn cấp 6 tháng chi phí sinh hoạt trước khi nâng tỷ trọng cổ phiếu.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l21-dj-3",
                        title = "Nhật ký: Khóa van an toàn thanh khoản bằng 6 tháng chi phí sinh hoạt",
                        scenario = "Xác định quy mô quỹ dự phòng và phương thức lưu trữ đảm bảo an toàn tuyệt đối.",
                        expectedKeywords = new[] { "quỹ khẩn cấp", "thanh khoản", "tiền gửi", "an toàn", "chi phí" },
                        sampleAnswer = "Luận điểm: Quỹ khẩn cấp 6 tháng bảo vệ nhà đầu tư không bao giờ phải bán tháo cổ phiếu ở đáy khi gặp sự cố y tế hoặc mất việc. Định giá: Chi phí 15 triệu/tháng -> Quỹ khẩn cấp tối thiểu 90 triệu đồng. Kịch bản sai: Không bao giờ mang quỹ khẩn cấp đi đầu tư sinh lời cao. Kỷ luật: Để tại tài khoản tiết kiệm kỳ hạn 1 tháng xoay vòng liên tục."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "hand_calc", 10),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 10,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Tỷ Suất Sinh Lời Kỳ Vọng Của Danh Mục Đa Lớp",
            Description = "Bấm máy tính tính trung bình gia quyền lợi nhuận và độ biến động danh mục.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "compound", "dca" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "investment_case", 11),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 11,
            Type = "investment_case",
            Title = "Tình Huống Khởi Động: Phân Bổ 3 Lớp Tài Sản Cốt Lõi Cho Người Mới",
            Description = "Lựa chọn cấu trúc danh mục cân bằng giữa an toàn, thanh khoản và tăng trưởng vốn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l21-case-warmup",
                        title = "Phân bổ 50 triệu đồng vốn ban đầu vào 3 lớp tài sản",
                        scenario = "Bạn vừa tích lũy được 50 triệu đồng đầu tiên và muốn tham gia thị trường tài chính. Phương án phân bổ nào sau đây phù hợp nhất cho người mới bắt đầu để vừa học hỏi vừa kiểm soát rủi ro?",
                        options = new[]
                        {
                            "Chia thành 3 lớp: 20 triệu gửi tiết kiệm dự phòng thanh khoản cao (40%), 25 triệu tích lũy ETF chỉ số thị trường (50%), 5 triệu trải nghiệm cổ phiếu doanh nghiệp đầu ngành (10%)",
                            "Dồn toàn bộ 50 triệu mua một mã cổ phiếu đầu cơ penny có tin đồn sắp tăng trần 5 phiên",
                            "Mua toàn bộ vé số hoặc đặt cược quyền chọn nhị phân trên mạng",
                            "Cất 50 triệu tiền mặt trong tủ khóa kín không gửi ngân hàng"
                        },
                        correctIndex = 0,
                        reasonPromptVi = "Giải thích nguyên tắc phân bổ đa lớp tài sản giúp giảm rủi ro cháy tài khoản cho người mới:",
                        expectedKeywords = new[] { "phân bổ", "đa dạng hóa", "dự phòng", "thanh khoản", "kiểm soát rủi ro" },
                        sampleExplanation = "Mô hình 3 lớp tài sản giúp nhà đầu tư luôn có đệm an toàn thanh khoản (quỹ dự phòng), đồng thời hưởng trọn tăng trưởng của nền kinh tế qua ETF chỉ số, và chỉ dùng một phần vốn rất nhỏ để học hỏi thực chiến mà không lo nguy cơ phá sản tài chính cá nhân.",
                        minWords = 15
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });


        // =========================================================================
        // LESSON 2.2: Đọc Báo Cáo Tài Chính & Chỉ Số Định Giá (P/E, P/B, ROE)
        // (Legacy activities: OrderIndex 1, 2)
        // Write-in activities: OrderIndex 3..10 (8 activities)
        // =========================================================================
        var l22 = SeedDataInitializer.InvestingLesson22Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "hand_calc", 3),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 3,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Định Giá P/E & Biên An Toàn (Margin of Safety)",
            Description = "Bấm máy tính tính toán P/E danh mục, giá trị nội tại ước tính và mức chiết khấu an toàn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "pe", "margin_of_safety" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "investment_case", 4),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 4,
            Type = "investment_case",
            Title = "Tình Huống: Bẫy Giá Rẻ (Value Trap) Khi Cổ Phiếu Có P/E Thấp Ảo",
            Description = "Phân tích doanh nghiệp có P/E = 4 do lợi nhuận đột biến một lần từ bán đất dự án.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l22-case-1",
                        title = "Cổ phiếu thép/bất động sản P/E = 4 do bán tài sản một lần",
                        scenario = "Một nhà đầu tư thấy cổ phiếu XYZ có P/E chỉ 4.5 trong khi trung bình ngành là 12. Sau khi đọc kỹ thuyết minh báo cáo tài chính, 70% lợi nhuận năm qua đến từ thanh lý nhà xưởng, còn mảng kinh doanh cốt lõi bị lỗ.",
                        options = new[]
                        {
                            "Tất tay mua ngay vì cổ phiếu quá rẻ so với trung bình ngành",
                            "Cảnh giác đây là 'Bẫy giá trị' (Value Trap); lợi nhuận không bền vững, EPS chu kỳ tới sẽ sụt giảm mạnh khiến P/E thực tế tăng vọt",
                            "Vay nợ ngân hàng mua gom thâu tóm công ty",
                            "Không cần đọc báo cáo tài chính, chỉ nhìn biểu đồ nến"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích tại sao cần loại trừ lợi nhuận bất thường một lần khi tính toán EPS và định giá P/E chuẩn xác:",
                        expectedKeywords = new[] { "bẫy giá trị", "value trap", "lợi nhuận bất thường", "kinh doanh cốt lõi", "bền vững" },
                        sampleExplanation = "P/E thấp ảo do lợi nhuận đột biến một lần từ bán tài sản không phản ánh năng lực tạo tiền của hoạt động kinh doanh cốt lõi. Khi không còn tài sản để bán ở năm tiếp theo, lợi nhuận sẽ sụt giảm mạnh, đẩy P/E thực tế lên cao và khiến nhà đầu tư thua lỗ.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "investment_case", 5),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 5,
            Type = "investment_case",
            Title = "Tình Huống: Doanh Nghiệp Có ROE Cao Nhờ Sử Dụng Đòn Bẩy Nợ Vay Quá Mức",
            Description = "Phân tích mô hình DuPont để bóc tách chất lượng lợi nhuận trên vốn chủ sở hữu ROE.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l22-case-2",
                        title = "ROE 35% nhưng tỷ lệ nợ vay trên vốn chủ D/E vọt lên 4.0 lần",
                        scenario = "Công ty ABC báo cáo ROE ấn tượng 35%. Tuy nhiên, hệ số Nợ/Vốn chủ sở hữu (D/E) lên tới 4.0, phần lớn tài sản hình thành từ nợ vay ngắn hạn ngân hàng khi lãi suất đang có xu hướng tăng.",
                        options = new[]
                        {
                            "Rất xuất sắc vì ROE 35% là thuộc top đầu thị trường",
                            "Rủi ro đòn bẩy tài chính cực lớn; ROE cao chủ yếu do thổi phồng hệ số nhân vốn chủ (Equity Multiplier) chứ không phải do biên lợi nhuận ròng hay hiệu quả sử dụng tài sản",
                            "Mua ngay cổ phiếu bất chấp rủi ro nợ",
                            "Báo cáo tài chính không quan trọng bằng tin đồn đội lái"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Sử dụng phân tích DuPont để giải thích vì sao ROE cao do nợ vay tiềm ẩn rủi ro phá sản:",
                        expectedKeywords = new[] { "dupont", "đòn bẩy", "nợ vay", "hệ số nhân vốn chủ", "chi phí lãi vay" },
                        sampleExplanation = "Theo phân tích DuPont, ROE = Biên lợi nhuận x Vòng quay tài sản x Đòn bẩy tài chính. ROE cao thuần túy do đòn bẩy nợ vay quá mức sẽ trở thành con dao hai lưỡi khi kinh tế suy thoái hoặc lãi suất tăng, dễ dẫn đến mất khả năng thanh toán.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "investment_case", 6),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 6,
            Type = "investment_case",
            Title = "Tình Huống: Dòng Tiền Thuần Từ Hoạt Động Kinh Doanh (OCF) Bị Âm Kéo Dài",
            Description = "Phân biệt giữa lợi nhuận kế toán trên giấy và dòng tiền thực thu về két.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l22-case-3",
                        title = "Lợi nhuận sau thuế 500 tỷ nhưng dòng tiền kinh doanh OCF âm 400 tỷ liên tục 2 năm",
                        scenario = "Doanh nghiệp niêm yết báo lãi sau thuế tăng trưởng 20% mỗi năm, nhưng Báo cáo lưu chuyển tiền tệ cho thấy Dòng tiền thuần từ HĐKD (OCF) âm nặng do các khoản phải thu khách hàng phình to bất thường.",
                        options = new[]
                        {
                            "Doanh nghiệp rất tốt vì lợi nhuận sau thuế trên Báo cáo kết quả kinh doanh vẫn dương lớn",
                            "Chất lượng lợi nhuận rất kém; lợi nhuận chỉ nằm trên giấy do bán chịu chưa thu được tiền thực tế, đối mặt nguy cơ trích lập dự phòng nợ xấu và cạn kiệt thanh khoản",
                            "Mua thêm vì khoản phải thu là tài sản của công ty",
                            "Không cần quan tâm báo cáo lưu chuyển tiền tệ"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích câu nói 'Lợi nhuận là quan điểm kế toán, dòng tiền mới là sự thật' (Cash is King):",
                        expectedKeywords = new[] { "dòng tiền", "ocf", "khoản phải thu", "lợi nhuận trên giấy", "thanh khoản" },
                        sampleExplanation = "Lợi nhuận kế toán có thể được ghi nhận trước theo nguyên tắc dồn tích khi xuất hóa đơn bán chịu. Nếu OCF âm kéo dài, doanh nghiệp không có tiền mặt thực tế để trả lương, trả nợ và tái đầu tư, rất dễ rơi vào cảnh mất thanh khoản kỹ thuật.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "decision_journal", 7),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 7,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Thẩm Định Cổ Phiếu Cơ Bản 4 Trụ Cột",
            Description = "Viết nhật ký định giá một mã cổ phiếu cụ thể với biên an toàn tối thiểu 25%.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l22-dj-1",
                        title = "Nhật ký: Thẩm định cổ phiếu đầu ngành có con hào kinh tế Moat",
                        scenario = "Phân tích một doanh nghiệp đầu ngành sản xuất tiêu dùng có dòng tiền OCF đều đặn và P/E chiết khấu sâu.",
                        expectedKeywords = new[] { "con hào kinh tế", "moat", "p/e", "biên an toàn", "ocf", "dài hạn" },
                        sampleAnswer = "Luận điểm: Doanh nghiệp sở hữu con hào kinh tế thương hiệu mạnh, ROE duy trì > 22% suốt 5 năm qua. Định giá: P/E hiện tại là 11.5, chiết khấu 30% so với mức định giá hợp lý P/E 16. Kịch bản sai: Nếu giá nguyên liệu đầu vào tăng sốc bào mòn biên lợi nhuận quá 5%, xem xét giảm tỷ trọng. Kỷ luật: Chỉ mua gom trong vùng giá dưới biên an toàn, không mua đuổi khi cổ phiếu vượt giá trị nội tại."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "decision_journal", 8),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 8,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Checklist Sàng Lọc Báo Cáo Tài Chính Trước Khi Mua",
            Description = "Viết 4 bài kiểm tra sức khỏe tài chính bắt buộc (Nợ vay, Phải thu, OCF, Biên lợi nhuận).",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l22-dj-2",
                        title = "Nhật ký: Bộ tiêu chí 'Loại trừ rủi ro gian lận kế toán'",
                        scenario = "Thiết lập quy tắc từ chối mua các doanh nghiệp có dấu hiệu xào nấu báo cáo tài chính.",
                        expectedKeywords = new[] { "sàng lọc", "gian lận", "báo cáo tài chính", "nợ vay", "dòng tiền", "kỷ luật" },
                        sampleAnswer = "Luận điểm: Bảo vệ vốn quan trọng hơn tìm kiếm siêu cổ phiếu; loại bỏ các công ty có vấn đề quản trị là cách giữ tiền tốt nhất. Định giá: Loại bỏ các doanh nghiệp có nợ vay ròng > vốn chủ và OCF âm 2 năm liên tiếp. Kịch bản sai: Nếu kiểm toán có ý kiến ngoại trừ, lập tức bán toàn bộ. Kỷ luật: Tuyệt đối không nghe phím hàng trên hội nhóm nếu doanh nghiệp không vượt qua checklist."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "decision_journal", 9),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 9,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Chiến Lược Giải Ngân Từng Phần Khi Giá Giảm Vào Vùng Rẻ",
            Description = "Lên kế hoạch chia nhỏ 3 lần giải ngân khi cổ phiếu chiết khấu sâu qua các mốc biên an toàn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l22-dj-3",
                        title = "Nhật ký: Mua gom theo kim tự tháp khi thị trường định giá rẻ",
                        scenario = "Kế hoạch giải ngân tỷ lệ 30% - 30% - 40% tương ứng mức chiết khấu 20%, 30% và 40% so với giá trị nội tại.",
                        expectedKeywords = new[] { "giải ngân từng phần", "biên an toàn", "chiết khấu", "kỷ luật", "giá trị nội tại" },
                        sampleAnswer = "Luận điểm: Không ai có thể bắt chính xác đáy thị trường; giải ngân từng phần giúp bình quân giá vốn thuận lợi trong vùng an toàn. Định giá: Giá trị nội tại 50.000đ -> Giải ngân lần 1 tại 40.000đ, lần 2 tại 35.000đ, lần 3 tại 30.000đ. Kịch bản sai: Dừng giải ngân nếu doanh nghiệp bị suy thoái nội tại cơ bản. Kỷ luật: Chỉ dùng tiền nhàn rỗi, không dùng đòn bẩy."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "hand_calc", 10),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 10,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Biên An Toàn & Chiết Khấu Định Giá Cổ Phiếu",
            Description = "Bấm máy tính tính tỷ lệ phần trăm biên an toàn từ thị giá và giá trị nội tại.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "pe", "margin_of_safety" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "investment_case", 11),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 11,
            Type = "investment_case",
            Title = "Tình Huống Khởi Động: Doanh Thu, Chi Phí Quán Cà Phê Nhỏ Trước Khi Đọc BCTC",
            Description = "Thấu hiểu cấu trúc Báo cáo Kết quả Kinh doanh (P&L) qua mô hình kinh doanh quán cà phê gần gũi.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l22-case-warmup",
                        title = "Phân tích P&L quán cà phê: Doanh thu 120M, Giá vốn 40M, Chi phí vận hành 50M, Thuế 6M",
                        scenario = "Một quán cà phê có doanh thu tháng là 120 triệu đồng. Chi phí hạt cà phê và sữa (COGS) là 40 triệu đồng. Chi phí thuê mặt bằng và lương nhân viên (OPEX) là 50 triệu đồng. Thuế và khấu hao máy pha là 6 triệu đồng. Lợi nhuận ròng (Net Profit) và Biên lợi nhuận ròng (Net Profit Margin) của quán là bao nhiêu?",
                        options = new[]
                        {
                            "Lợi nhuận ròng là 24 triệu đồng; Biên lợi nhuận ròng đạt 20.0%",
                            "Lợi nhuận ròng là 80 triệu đồng; Biên lợi nhuận ròng đạt 66.7%",
                            "Lợi nhuận ròng là 30 triệu đồng; Biên lợi nhuận ròng đạt 25.0%",
                            "Quán bị lỗ 10 triệu đồng do chi phí quá cao"
                        },
                        correctIndex = 0,
                        reasonPromptVi = "Trình bày các bước tính từ Doanh thu thuần đến Lợi nhuận gộp và Lợi nhuận ròng:",
                        expectedKeywords = new[] { "doanh thu", "giá vốn", "lợi nhuận gộp", "chi phí vận hành", "lợi nhuận ròng", "biên lợi nhuận" },
                        sampleExplanation = "Doanh thu = 120M. Lợi nhuận gộp = 120 - 40 = 80M. Lợi nhuận từ hoạt động = 80 - 50 = 30M. Lợi nhuận ròng sau thuế và khấu hao = 30 - 6 = 24 triệu đồng. Biên lợi nhuận ròng = 24 / 120 = 20.0%. Đây chính là trình tự đọc Báo cáo Kết quả Hoạt động Kinh doanh của mọi doanh nghiệp niêm yết.",
                        minWords = 15
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });


        // =========================================================================
        // LESSON 3.1: Quỹ Chỉ Số ETF, Phí Ẩn & Kỷ Luật Tâm Lý Đầu Tư
        // (Legacy activities: OrderIndex 1, 2)
        // Write-in activities: OrderIndex 3..10 (8 activities)
        // =========================================================================
        var l31 = SeedDataInitializer.InvestingLesson31Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "hand_calc", 3),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 3,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Tác Động Của Phí Quản Lý Quỹ (Expense Ratio) Sau 20 Năm",
            Description = "Bấm máy tính tính toán số tiền chênh lệch khổng lồ giữa mức phí 0.5% vs 2.0%/năm qua lãi kép.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "compound", "dca" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "investment_case", 4),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 4,
            Type = "investment_case",
            Title = "Tình Huống: Quỹ Chủ Động Thu Phí 2.5%/Năm Thua Lỗ Quỹ Chỉ Số ETF",
            Description = "Phân tích dữ liệu thực nghiệm SPIVA: hơn 85% quỹ chủ động không thắng được chỉ số thị trường sau 15 năm.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l31-case-1",
                        title = "Chọn quỹ chủ động phí 2.5%/năm hay ETF chỉ số phí 0.4%/năm",
                        scenario = "Một nhân viên tư vấn mời bạn mua chứng chỉ quỹ mở chủ động với lời hứa hẹn 'đội ngũ chuyên gia sẽ đánh bại thị trường', thu phí quản lý 2.5%/năm và phí bán 1.5%. Bạn đối chiếu với quỹ ETF chỉ số chỉ thu 0.4%/năm.",
                        options = new[]
                        {
                            "Gửi tiền cho quỹ chủ động vì chuyên gia chắc chắn giỏi hơn thị trường chung",
                            "Chọn quỹ ETF chỉ số chi phí thấp 0.4%/năm; theo thống kê SPIVA dài hạn, chi phí quản lý cao 2.5% sẽ bào mòn hơn 40% lợi nhuận lãi kép trong 20 năm",
                            "Không đầu tư gì cả mà giữ tiền mặt",
                            "Mua cổ phiếu Penny theo tin đồn"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích vì sao chi phí quản lý quỹ (Expense Ratio) lại là yếu tố dự báo lợi nhuận ròng dài hạn tin cậy nhất:",
                        expectedKeywords = new[] { "phí quản lý", "etf", "spiva", "bào mòn", "lãi kép dài hạn" },
                        sampleExplanation = "Phí quản lý được trừ thẳng vào giá trị tài sản ròng mỗi ngày bất kể quỹ lãi hay lỗ. Chênh lệch phí 2%/năm qua 20-30 năm sẽ làm thất thoát hàng trăm triệu đến hàng tỷ đồng tiền lãi kép của nhà đầu tư.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "investment_case", 5),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 5,
            Type = "investment_case",
            Title = "Tình Huống: Tâm Lý Bán Đáy Khi Thị Trường Khủng Hoảng Bão Tin Tức Tiêu Cực",
            Description = "Phân tích thiên kiến sợ mất mát (Loss Aversion) và quyết định giữ vững danh mục ETF.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l31-case-2",
                        title = "Tin tức chiến tranh và dịch bệnh tràn ngập mặt báo khiến VN-Index giảm 30%",
                        scenario = "Truyền thông liên tục đưa tin về khủng hoảng kinh tế, các diễn đàn khuyên bán sạch tháo chạy. Bạn đang nắm giữ danh mục ETF chỉ số và thấy tài khoản tạm âm 22%. Bạn nên hành động thế nào?",
                        options = new[]
                        {
                            "Bán hoảng loạn toàn bộ danh mục ngay phiên sàn kế tiếp để 'bảo toàn vốn'",
                            "Duy trì kỷ luật DCA, tiếp tục giải ngân đều đặn theo kế hoạch; lịch sử chứng minh mọi cuộc khủng hoảng đều trôi qua và kinh tế sẽ hồi phục lập đỉnh mới",
                            "Vay nặng lãi để bắt đáy một lần duy nhất",
                            "Xóa app đầu tư và từ bỏ hoàn toàn"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích thiên kiến Sợ mất mát (Loss Aversion) khiến nhà đầu tư thường bán tháo ở đáy như thế nào:",
                        expectedKeywords = new[] { "sợ mất mát", "loss aversion", "bán đáy", "kỷ luật dca", "hồi phục" },
                        sampleExplanation = "Nỗi đau của việc mất 1 đồng lớn gấp 2-2.5 lần niềm vui khi kiếm được 1 đồng (Loss Aversion). Nỗi sợ này thúc đẩy hành vi bán tháo ở đáy để chấm dứt cơn đau tâm lý, vô tình biến khoản lỗ trên giấy thành khoản lỗ vĩnh viễn và bỏ lỡ nhịp hồi phục mạnh mẽ nhất sau đó.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "investment_case", 6),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 6,
            Type = "investment_case",
            Title = "Tình Huống: Sai Lầm Cố Gắng Bắt Đỉnh Đoán Đáy Thị Trường (Market Timing)",
            Description = "Phân tích tác hại của việc bỏ lỡ 10 ngày tăng trưởng mạnh nhất của thị trường.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l31-case-3",
                        title = "Bỏ lỡ 10 phiên tăng mạnh nhất khiến hiệu suất giảm một nửa",
                        scenario = "Một nhà đầu tư liên tục nhảy ra nhảy vào thị trường vì tin rằng mình dự đoán được khi nào thị trường tạo đáy. Kết quả thống kê cho thấy chỉ cần lỡ 10 ngày tăng mạnh nhất trong 20 năm, lợi nhuận tổng thể giảm hơn 50%.",
                        options = new[]
                        {
                            "Tiếp tục lướt sóng T+ vì chỉ có lướt sóng mới giàu nhanh",
                            "Áp dụng nguyên lý 'Thời gian trong thị trường quan trọng hơn thời điểm vào thị trường' (Time in the market beats timing the market); duy trì vị thế đầu tư dài hạn qua ETF",
                            "Bỏ thị trường chứng khoán sang chơi cờ bạc",
                            "Gửi tiền cho các hội nhóm VIP thu phí"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích vì sao việc cố gắng đoán đỉnh đáy (Market Timing) hầu như luôn dẫn đến hiệu suất kém hơn mua và nắm giữ (Buy & Hold):",
                        expectedKeywords = new[] { "market timing", "thời gian trong thị trường", "bỏ lỡ", "buy and hold", "kỷ luật" },
                        sampleExplanation = "Những ngày tăng trưởng bùng nổ nhất thường diễn ra ngay giữa giai đoạn thị trường hoảng loạn và ảm đạm nhất. Việc cố đoán đỉnh đáy thường khiến nhà đầu tư bán ở đáy và chỉ mua lại khi giá đã lập đỉnh mới.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "decision_journal", 7),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 7,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Kế Hoạch Tích Lũy Quỹ ETF Chỉ Số Toàn Diện",
            Description = "Viết 4 trụ cột thẩm định cho lộ trình DCA quỹ ETF chỉ số VN30 / Diamond trong 15 năm.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l31-dj-1",
                        title = "Nhật ký: Lộ trình DCA quỹ ETF chỉ số đa dạng hóa 15 năm",
                        scenario = "Xây dựng kế hoạch giải ngân cố định 6 triệu/tháng vào quỹ ETF chỉ số bất chấp tin tức thị trường ngắn hạn.",
                        expectedKeywords = new[] { "etf", "dca", "chỉ số", "chi phí thấp", "dài hạn", "kỷ luật" },
                        sampleAnswer = "Luận điểm: Quỹ ETF chỉ số đại diện cho sự tăng trưởng của các doanh nghiệp hàng đầu nền kinh tế, loại bỏ rủi ro chọn sai cổ phiếu đơn lẻ. Định giá: Đầu tư định kỳ không cần canh điểm số VN-Index. Kịch bản sai: Nếu thị trường giảm 30%, tiếp tục giải ngân để tích lũy thêm chứng chỉ quỹ giá rẻ. Kỷ luật: Thiết lập trích nợ tự động ngày nhận lương, tuyệt đối không hủy lệnh."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "decision_journal", 8),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 8,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Kế Hoạch Phòng Vệ Cảm Xúc Trước Bão Tin Tức Tiêu Cực",
            Description = "Thiết lập quy tắc cách ly tin tức rác và hội nhóm 'lùa gà' trên mạng xã hội.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l31-dj-2",
                        title = "Nhật ký: Thiết lập bộ lọc thông tin 'Chế độ ăn tinh thần lành mạnh'",
                        scenario = "Quy tắc hủy theo dõi các hội nhóm hô hào, không đọc bảng điện liên tục trong giờ làm việc.",
                        expectedKeywords = new[] { "bộ lọc", "tin tức", "bảng điện", "kỷ luật cảm xúc", "tâm lý" },
                        sampleAnswer = "Luận điểm: Xem bảng điện liên tục kích hoạt phản xạ Hệ thống 1 và lòng tham/nỗi sợ ngắn hạn. Định giá: Tập trung vào giá trị dài hạn thay vì dao động xanh đỏ từng phút. Kịch bản sai: Nếu thấy tâm lý bất an, xóa app theo dõi giá trong 1 tuần. Kỷ luật: Chỉ mở tài khoản kiểm tra giao dịch 1 lần mỗi tháng vào ngày DCA."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "decision_journal", 9),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 9,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Kế Hoạch Rút Vốn An Toàn Khi Bước Vào Giai Đoạn Nghỉ Hưu",
            Description = "Thẩm định cơ chế chuyển dịch từ danh mục tăng trưởng sang danh mục tạo dòng tiền.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l31-dj-3",
                        title = "Nhật ký: Cơ chế trích rút danh mục 4% an toàn trong 30 năm",
                        scenario = "Lên kế hoạch chuyển dần một phần ETF cổ phiếu sang trái phiếu và tiền gửi ngắn hạn 3 năm trước khi nghỉ hưu.",
                        expectedKeywords = new[] { "quy tắc 4%", "trích rút", "nghỉ hưu", "an toàn", "trái phiếu" },
                        sampleAnswer = "Luận điểm: Giai đoạn nghỉ hưu ưu tiên bảo toàn vốn và tạo dòng tiền chi tiêu hơn là tăng trưởng tối đa. Định giá: Duy trì quỹ tiền mặt dự phòng đủ chi tiêu trong 3 năm để không phải bán ETF khi thị trường giảm. Kịch bản sai: Nếu thị trường suy thoái, chỉ rút 3% thay vì 4%. Kỷ luật: Rút tiền định kỳ theo quý, không rút dồn cục."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "hand_calc", 10),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 10,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Mức Sụt Giảm Drawdown & Tỷ Lệ Tăng Để Hòa Vốn",
            Description = "Bấm máy tính tính toán mức phần trăm hồi phục cần thiết khi danh mục giảm 20% đến 50%.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "drawdown", "margin_of_safety" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "investment_case", 11),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 11,
            Type = "investment_case",
            Title = "Tình Huống Khởi Động: Chọn Quỹ Chỉ Số ETF Phí Thấp vs Quỹ Chủ Động Phí Cao",
            Description = "Phân tích tác động lâu dài của chi phí quản lý quỹ (TER) đối với tài sản tích lũy lãi kép.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l31-case-warmup",
                        title = "So sánh phí 0.6%/năm của ETF và phí 2.0%/năm của quỹ mở chủ động sau 20 năm",
                        scenario = "Một nhà đầu tư tích lũy định kỳ trong 20 năm. Lựa chọn A là Quỹ ETF chỉ số với tổng phí quản lý 0.6%/năm. Lựa chọn B là Quỹ mở chủ động với phí quản lý 2.0%/năm cộng phí mua bán 1.0%. Nếu cả hai quỹ cùng đạt lợi suất thô 10%/năm trước phí, kết quả dài hạn sẽ ra sao?",
                        options = new[]
                        {
                            "Khoản chênh lệch phí ~1.5%/năm qua 20 năm tích lũy lãi kép sẽ làm sụt giảm 25%–30% tổng tài sản cuối kỳ của Quỹ B so với Quỹ A, chứng minh chi phí thấp là lợi thế bền vững nhất",
                            "Quỹ chủ động phí cao luôn đảm bảo sinh lời gấp đôi quỹ ETF trong mọi năm",
                            "Chi phí quản lý quỹ không có bất kỳ ảnh hưởng nào đến giá trị tài sản nhận về",
                            "Quỹ ETF sẽ bị công ty quản lý thu hồi toàn bộ vốn sau 5 năm"
                        },
                        correctIndex = 0,
                        reasonPromptVi = "Giải thích tại sao phí quản lý quỹ được xem là kẻ bào mòn thầm lặng lớn nhất của lãi kép:",
                        expectedKeywords = new[] { "chi phí", "phí quản lý", "bào mòn", "lãi kép", "etf", "dài hạn" },
                        sampleExplanation = "Phí quản lý quỹ bị trừ trực tiếp vào giá trị tài sản ròng (NAV) mỗi ngày, bất kể thị trường tăng hay giảm. Mức chênh lệch 1.4%/năm tưởng như nhỏ nhưng khi ghép lãi kép qua 20 năm sẽ lấy đi gần 1/3 tổng tài sản tích lũy, do đó chọn sản phẩm chỉ số phí thấp là chiến lược ưu việt cho đa số nhà đầu tư.",
                        minWords = 15
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });


        // =========================================================================
        // LESSON 3.2: Quản Trị Rủi Ro Danh Mục, Đi Vốn & Cắt Lỗ Kỷ Luật
        // (Legacy activities: OrderIndex 1, 2)
        // Write-in activities: OrderIndex 3..10 (8 activities)
        // =========================================================================
        var l32 = SeedDataInitializer.InvestingLesson32Id;

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "hand_calc", 3),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 3,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Quản Trị Rủi Ro Max Drawdown & Đi Vốn An Toàn",
            Description = "Bấm máy tính tính toán mức rủi ro tối đa 1-2% trên tổng danh mục cho mỗi vị thế đầu tư.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "drawdown", "margin_of_safety" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "investment_case", 4),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 4,
            Type = "investment_case",
            Title = "Tình Huống: Gồng Lỗ Cổ Phiếu Đầu Cơ vs Kỷ Luật Cắt Lỗ Tại Mốc 7-8%",
            Description = "Phân tích tâm lý từ chối thừa nhận sai lầm và quyết định cắt lỗ cứu lấy vốn gốc.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l32-case-1",
                        title = "Cổ phiếu đầu cơ giảm vi phạm mốc cắt lỗ 8% nhưng nhà đầu tư gồng lỗ chờ hòa vốn",
                        scenario = "Bạn mua một cổ phiếu tăng trưởng nóng theo đà (momentum) với nguyên tắc cắt lỗ nếu giá vi phạm -8%. Giá giảm chạm ngưỡng -8%, bạn tiếc tiền và quyết định gồng lỗ, tự an ủi 'không bán là chưa lỗ'. Cổ phiếu sau đó giảm tiếp -45%.",
                        options = new[]
                        {
                            "Tiếp tục gồng lỗ và hy vọng phép màu xảy ra trong tương lai",
                            "Thực hiện cắt lỗ dứt khoát tại mốc -8% đúng theo kế hoạch ban đầu để bảo vệ 92% vốn gốc; chấp nhận sai lầm là một phần tất yếu của quản trị rủi ro",
                            "Vay tiền đóng thêm margin gỡ gạc",
                            "Lên mạng xã hội chửi bới ban lãnh đạo công ty"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích toán học về sự bất đối xứng giữa mức lỗ và mức lãi cần thiết để hòa vốn (Lỗ 50% cần lãi 100% để hòa vốn):",
                        expectedKeywords = new[] { "cắt lỗ", "bất đối xứng", "hòa vốn", "bảo vệ vốn", "kỷ luật" },
                        sampleExplanation = "Khi để khoản lỗ phình to từ 8% lên 50%, nhà đầu tư cần đạt tỷ suất sinh lời 100% cho số vốn còn lại chỉ để về điểm hòa vốn. Cắt lỗ nhỏ và sớm là chìa khóa duy nhất để bảo toàn vốn gốc cho các cơ hội vượt trội tiếp theo.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "investment_case", 5),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 5,
            Type = "investment_case",
            Title = "Tình Huống: Sử Dụng Đòn Bẩy Tài Chính Margin Trong Thị Trường Biến Động",
            Description = "Phân tích rủi ro cháy tài khoản do hiện tượng Call Margin chéo.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l32-case-2",
                        title = "Vay margin 1:1 trong giai đoạn thị trường phân phối đỉnh",
                        scenario = "Một nhà đầu tư có 500 triệu vốn gốc, vay thêm 500 triệu margin (tỷ lệ 1:1) để mua cổ phiếu. Khi thị trường điều chỉnh 25%, tổng tài sản giảm 250 triệu khiến vốn gốc bị mất 50% và bị công ty chứng khoán Call Margin ép bán.",
                        options = new[]
                        {
                            "Vay nóng bên ngoài nộp thêm vào để gồng tiếp",
                            "Hạ ngay đòn bẩy margin về 0; nhà đầu tư cá nhân dài hạn không nên dùng margin vì đòn bẩy tước đi yếu tố thời gian và buộc bạn phải bán tháo đúng đáy khi bị giải chấp",
                            "Khiếu nại công ty chứng khoán vì đã bán tháo cổ phiếu của mình",
                            "Chuyển toàn bộ danh mục sang tiền điện tử phái sinh"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích tại sao đòn bẩy Margin là 'kẻ thù số 1' của chiến lược đầu tư dài hạn:",
                        expectedKeywords = new[] { "margin", "đòn bẩy", "giải chấp", "cháy tài khoản", "thời gian" },
                        sampleExplanation = "Lợi thế lớn nhất của nhà đầu tư dài hạn là thời gian để chờ doanh nghiệp tạo giá trị và vượt qua suy thoái. Đòn bẩy margin tước đi lợi thế đó bằng áp lực lãi vay và rủi ro bị giải chấp bắt buộc đúng vùng đáy hoảng loạn.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "investment_case", 6),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 6,
            Type = "investment_case",
            Title = "Tình Huống: Giới Hạn Tỷ Trọng Tối Đa Cho Một Cổ Phiếu Riêng Lẻ",
            Description = "Phân tích quy tắc không để 1 cổ phiếu vượt quá 15–20% tổng danh mục.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l32-case-3",
                        title = "Tất tay 80% tài sản vào một cổ phiếu 'chắc thắng 100%'",
                        scenario = "Nghe một nguồn tin nội bộ đáng tin cậy về dự án mới, nhà đầu tư định dồn 80% tổng tài sản vào một mã cổ phiếu duy nhất. Dự án sau đó gặp trục trặc pháp lý và bị đình chỉ vô thời hạn.",
                        options = new[]
                        {
                            "Rất đáng thử vì muốn giàu nhanh thì phải liều lĩnh tập trung",
                            "Vi phạm nghiêm trọng nguyên tắc quản trị rủi ro; không có khoản đầu tư nào là chắc thắng 100%, một cổ phiếu riêng lẻ không bao giờ nên vượt quá 15-20% tổng danh mục để tránh rủi ro phá sản cá nhân",
                            "Bán nhà để mua gom thêm chứng minh niềm tin",
                            "Đổ lỗi cho người chia sẻ thông tin"
                        },
                        correctIndex = 1,
                        reasonPromptVi = "Giải thích nguyên tắc giới hạn tỷ trọng vị thế tối đa (Position Sizing) để bảo toàn sự sống còn:",
                        expectedKeywords = new[] { "tỷ trọng vị thế", "position sizing", "rủi ro phi hệ thống", "đa dạng hóa", "sinh tồn" },
                        sampleExplanation = "Rủi ro phi hệ thống (gian lận, tai nạn pháp lý, thay đổi công nghệ) có thể đánh sập bất kỳ doanh nghiệp riêng lẻ nào. Giới hạn tỷ trọng vị thế tối đa 15-20% đảm bảo rằng dù tình huống tồi tệ nhất xảy ra, danh mục vẫn an toàn và sống sót tiếp tục đầu tư.",
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "decision_journal", 7),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 7,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Kế Hoạch Quản Trị Rủi Ro Vị Thế 2% (2% Risk Rule)",
            Description = "Viết 4 trụ cột xác định điểm dừng lỗ và khối lượng cổ phiếu được phép giải ngân.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l32-dj-1",
                        title = "Nhật ký: Quy tắc rủi ro tối đa 2% tổng tài sản cho mỗi thương vụ",
                        scenario = "Tính toán số tiền lỗ tối đa chấp nhận được và điểm dừng lỗ kỹ thuật trước khi đặt lệnh.",
                        expectedKeywords = new[] { "quy tắc 2%", "cắt lỗ", "khối lượng", "quản trị rủi ro", "kỷ luật" },
                        sampleAnswer = "Luận điểm: Luôn xác định rủi ro mất tiền trước khi mơ đến lợi nhuận. Định giá: Danh mục 200 triệu -> Mức lỗ tối đa cho 1 vị thế là 4 triệu (2%). Điểm cắt lỗ -8% -> Giá trị giải ngân tối đa là 50 triệu. Kịch bản sai: Giá chạm ngưỡng dừng lỗ lập tức bấm bán, không do dự. Kỷ luật: Tuyệt đối không hủy lệnh dừng lỗ hoặc dời điểm cắt lỗ sâu hơn."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "decision_journal", 8),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 8,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Kế Hoạch Hóa Giải Tâm Lý 'Cay Cú Gỡ Lạc' (Revenge Trading)",
            Description = "Thiết lập khoảng dừng bắt buộc 48 giờ sau khi vừa cắt lỗ một thương vụ lớn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l32-dj-2",
                        title = "Nhật ký: Khóa tài khoản 48 giờ sau khi cắt lỗ để hạ nhiệt cảm xúc",
                        scenario = "Ngăn chặn hành vi vào lệnh bừa bãi để trả thù thị trường khi vừa gánh một khoản lỗ đau đớn.",
                        expectedKeywords = new[] { "trả thù thị trường", "revenge trading", "khoảng dừng", "kỷ luật cảm xúc", "hệ thống 2" },
                        sampleAnswer = "Luận điểm: Tâm lý cay cú sau khi thua lỗ kích hoạt Hệ thống 1 và lòng hiếu thắng nguy hiểm. Định giá: Thị trường luôn ở đó, không bao giờ cần phải gỡ gạc ngay trong ngày. Kịch bản sai: Nếu thấy bực bội và muốn vào lệnh mới, đóng máy tính đi dạo. Kỷ luật: Tự động dừng giao dịch tối thiểu 48 giờ để lấy lại sự khách quan và đánh giá lại bài học."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "decision_journal", 9),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 9,
            Type = "decision_journal",
            Title = "Nhật Ký Quyết Định: Bản Tuyên Ngôn Kỷ Luật Đầu Tư Suốt Đời",
            Description = "Viết 10 điều răn kỷ luật tài chính cá nhân làm kim chỉ nam xuyên suốt sự nghiệp đầu tư.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l32-dj-3",
                        title = "Nhật ký: Bản tuyên ngôn 5 không trong quản trị tài sản cá nhân",
                        scenario = "Xây dựng các giới hạn đỏ không bao giờ được phép vượt qua trong bất kỳ hoàn cảnh nào.",
                        expectedKeywords = new[] { "tuyên ngôn", "kỷ luật", "không margin", "dài hạn", "biên an toàn" },
                        sampleAnswer = "Luận điểm: Sự trường tồn và sống sót trên thị trường là điều kiện tiên quyết để hưởng trọn quả ngọt lãi kép. Định giá: 5 Không: Không vay margin, Không tất tay 1 mã, Không mua tài sản không hiểu, Không gồng lỗ đầu cơ, Không FOMO tin đồn. Kịch bản sai: Đọc lại bản tuyên ngôn mỗi đầu quý. Kỷ luật: In ra giấy và dán trước bàn làm việc."
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
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "hand_calc", 10),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 10,
            Type = "hand_calc",
            Title = "Tự Tính Tay: Lãi Kép & Biên An Toàn Tích Lũy Dài Hạn",
            Description = "Bấm máy tính tính toán mức tăng trưởng danh mục khi kiểm soát tốt Drawdown dưới 15%.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                problemTypes = new[] { "compound", "dca", "margin_of_safety" },
                tolerancePercent = 1.5
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "investment_case", 11),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 11,
            Type = "investment_case",
            Title = "Tình Huống Khởi Động: Tại Sao Người Mới Tuyệt Đối Không Dùng Đòn Bẩy (Margin)",
            Description = "Thấu hiểu cơ chế Call Margin và rủi ro cháy sạch tài khoản khi dùng tiền vay mượn để đầu cơ.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "inv-l32-case-warmup",
                        title = "Vốn 100 triệu, vay margin 1:1 thành 200 triệu, cổ phiếu giảm 25%: Chuyện gì xảy ra?",
                        scenario = "Một nhà đầu tư F0 có 100 triệu vốn tự có, vay margin tỷ lệ 1:1 mua 200 triệu đồng cổ phiếu. Nếu thị trường giảm mạnh và giá cổ phiếu giảm 25%, tổng tài sản giảm xuống còn 150 triệu. Khi đó vốn tự có của nhà đầu tư còn bao nhiêu và tỷ lệ lỗ trên vốn gốc là bao nhiêu?",
                        options = new[]
                        {
                            "Vốn tự có giảm từ 100 triệu xuống còn 50 triệu (bị mất 50 triệu), tức là lỗ 50% vốn tự có dù cổ phiếu chỉ giảm 25%; đối mặt nguy cơ bị công ty chứng khoán giải chấp (Call Margin)",
                            "Nhà đầu tư chỉ bị lỗ 25 triệu đồng và công ty chứng khoán sẽ chịu phần lỗ còn lại",
                            "Công ty chứng khoán sẽ tự động bù tiền cho nhà đầu tư để đưa tài khoản về hòa vốn",
                            "Khoản vay margin không cần hoàn trả nếu cổ phiếu bị giảm giá"
                        },
                        correctIndex = 0,
                        reasonPromptVi = "Giải thích cơ chế đòn bẩy hai lưỡi và tại sao người mới không bao giờ nên dùng margin:",
                        expectedKeywords = new[] { "đòn bẩy", "margin", "call margin", "giải chấp", "cháy tài khoản", "kỷ luật" },
                        sampleExplanation = "Khoản nợ vay 100 triệu là cố định và phải trả kèm lãi vay. Khi tài sản từ 200M giảm về 150M, toàn bộ khoản lỗ 50M trừ thẳng vào 100M vốn tự có, khiến nhà đầu tư mất đứt 50% vốn. Đòn bẩy khuếch đại mức lỗ và tước đoạt quyền chờ đợi thị trường hồi phục do áp lực Call Margin, vì vậy người mới tuyệt đối không được dùng đòn bẩy.",
                        minWords = 15
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
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"swms:investing-quiz:{key}"));
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
    /// Returns 6 lesson quizzes (InvestingLesson11Id .. InvestingLesson32Id) plus 1 comprehensive final course quiz
    /// attached to InvestingLesson32Id. Each quiz contains 10 deterministic multiple_choice items
    /// (4 Concepts + 4 Calculations with specific numbers + 2 Psychology/Risk Management scenarios)
    /// with detailed step-by-step mathematical solutions in AiRubricFeedbackJson.
    /// </summary>
    public static List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)> GetCourseQuizzes(DateTimeOffset now)
    {
        return new List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)>
        {
            // =========================================================================
            // 1. Quiz Bài 1.1: Sức Mạnh Lãi Kép & Chiến Lược DCA Định Kỳ
            // =========================================================================
            BuildQuiz(
                "l11",
                SeedDataInitializer.InvestingLesson11Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 1.1: Sức Mạnh Lãi Kép & Chiến Lược DCA",
                now.AddSeconds(1),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Bản chất toán học của lãi kép (Compound Interest) khác biệt căn bản với lãi đơn (Simple Interest) ở điểm nào?",
                        "Lãi kép tính lãi trên cả vốn gốc lẫn phần tiền lãi đã tích lũy từ các kỳ trước, tạo ra tốc độ tăng trưởng theo hàm số mũ theo thời gian",
                        "Lãi kép chỉ tính lãi cố định trên số vốn gốc ban đầu và không thay đổi số tiền lãi mỗi kỳ",
                        "Lãi kép chỉ được áp dụng cho các giao dịch vay nợ tín dụng tiêu dùng ngân hàng",
                        "Lãi kép bắt buộc nhà đầu tư phải nộp thêm tiền vốn gấp đôi sau mỗi tháng",
                        "A",
                        "Bước 1: Nhớ lại định nghĩa lãi kép FV = PV * (1 + r)^n. Bước 2: So sánh với lãi đơn FV = PV * (1 + r*n). Phân tích phương án nhiễu: Phương án A đúng vì lãi sinh ra được nhập gốc để tiếp tục sinh lãi ở kỳ tiếp theo. B sai vì đó là định nghĩa của lãi đơn. C và D là các ngụy biện phi tài chính. Do đó A là đáp án chính xác."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Mục tiêu cốt lõi của chiến lược Dollar-Cost Averaging (DCA - Đầu tư định kỳ) là gì?",
                        "Giúp nhà đầu tư luôn luôn mua được cổ phiếu tại đúng vùng đáy tuyệt đối của thị trường",
                        "Triệt tiêu áp lực tâm lý phán đoán đỉnh - đáy ngắn hạn, tự động mua nhiều cổ phiếu hơn khi giá giảm và mua ít hơn khi giá tăng để đạt giá vốn bình quân tối ưu",
                        "Đảm bảo chắc chắn danh mục sẽ tăng trưởng tối thiểu 50%/năm trong mọi bối cảnh kinh tế",
                        "Thay thế hoàn toàn việc đọc hiểu báo cáo tài chính và phân tích doanh nghiệp",
                        "B",
                        "Bước 1: Xác định cơ chế của DCA: phân bổ một khoản tiền cố định theo chu kỳ thời gian cố định. Bước 2: Khi giá giảm, cùng số tiền đó sẽ mua được nhiều đơn vị tài sản hơn; khi giá tăng mua được ít hơn. Phân tích nhiễu: A sai vì không ai có thể dự đoán chính xác đáy tuyệt đối liên tục. C và D là những kỳ vọng sai lệch. Do đó B là đáp án chính xác."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Tác động của lạm phát (Inflation) đối với sức mua thực tế của tài sản tích lũy dài hạn được diễn giải như thế nào?",
                        "Lạm phát làm tăng sức mua thực tế của tiền mặt dự trữ trong két sắt gia đình",
                        "Lợi suất thực tế bằng lãi suất danh nghĩa trừ đi tỷ lệ lạm phát; giữ tiền mặt thuần túy trong dài hạn đồng nghĩa với việc chấp nhận tài sản bị bào mòn sức mua mỗi năm",
                        "Lạm phát hoàn toàn không ảnh hưởng đến người gửi tiết kiệm tại các ngân hàng thương mại nhà nước",
                        "Lạm phát luôn luôn kéo giảm thị giá của tất cả các cổ phiếu niêm yết về mức 0",
                        "B",
                        "Bước 1: Áp dụng phương trình Fisher: Lãi suất thực ≈ Lãi suất danh nghĩa - Lạm phát. Bước 2: Nếu giữ tiền mặt không sinh lãi (lãi danh nghĩa = 0), lạm phát 4%/năm sẽ làm mất 4% sức mua mỗi năm. Phân tích nhiễu: A đảo ngược bản chất; C sai vì tiền gửi vẫn chịu lạm phát bào mòn; D sai vì cổ phiếu đại diện cho tài sản thực có khả năng chuyển lạm phát vào giá bán hàng. Do đó B là đáp án chính xác."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Với cùng một mức lãi suất danh nghĩa hàng năm, việc tăng tần suất ghép lãi (từ hàng năm sang hàng quý hoặc hàng tháng) mang lại kết quả nào?",
                        "Làm lãi suất hiệu dụng hàng năm (EAR) cao hơn và giá trị tương lai (FV) lớn hơn do lãi phát sinh sớm được nhập gốc sinh lãi liên tục",
                        "Làm giảm giá trị tương lai vì tiền lãi bị chia nhỏ ra nhiều kỳ thanh toán",
                        "Không làm thay đổi giá trị tương lai vì tổng lãi suất danh nghĩa của cả năm là cố định",
                        "Làm phát sinh phí phạt quản trị tài khoản tiền gửi của ngân hàng",
                        "A",
                        "Bước 1: Công thức lãi suất hiệu dụng: EAR = (1 + r/m)^m - 1, trong đó m là số kỳ ghép lãi trong năm. Khi m tăng (ghép quý m=4, ghép tháng m=12), EAR luôn lớn hơn lãi suất danh nghĩa r. Phân tích nhiễu: B và C sai vì bỏ qua hiệu ứng lãi kép nội kỳ (compounding effect). D sai vì không có ngân hàng nào phạt ghép lãi. Do đó A là đáp án chính xác."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tính toán): Bạn gửi tiết kiệm 50.000.000 đồng với lãi suất kép 7,0%/năm trong thời hạn 5 năm, ghép lãi hàng năm. Tổng số tiền cả gốc và lãi nhận được sau 5 năm xấp xỉ là bao nhiêu?",
                        "70.127.587 đồng",
                        "67.500.000 đồng",
                        "53.500.000 đồng",
                        "85.000.000 đồng",
                        "A",
                        "Bước 1: Công thức tính giá trị tương lai lãi kép: FV = PV * (1 + r)^n. Bước 2: Thay số: PV = 50.000.000; r = 0,07; n = 5. FV = 50.000.000 * (1 + 0,07)^5 = 50.000.000 * 1,4025517 ≈ 70.127.587 đồng. Phân tích phương án nhiễu: B là kết quả tính theo lãi đơn: 50M * (1 + 0,07*5) = 67.500.000 đồng (thiếu mất 2.627.587 đồng tiền lãi kép). C chỉ là kết quả sau 1 năm. D là con số ước lượng không căn cứ. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tính toán): Một người thực hiện chiến lược DCA mua chứng chỉ quỹ định kỳ trong 3 tháng với số tiền cố định 6.000.000 đồng/tháng: Tháng 1 mua giá 20.000 đ/ccq (được 300 ccq); Tháng 2 thị trường giảm, mua giá 15.000 đ/ccq (được 400 ccq); Tháng 3 thị trường hồi phục, mua giá 24.000 đ/ccq (được 250 ccq). Tổng tiền bỏ ra là 18.000.000 đồng và sở hữu 950 ccq. Giá vốn bình quân mỗi chứng chỉ quỹ là bao nhiêu?",
                        "18.947 đồng/ccq",
                        "19.667 đồng/ccq",
                        "20.000 đồng/ccq",
                        "15.000 đồng/ccq",
                        "A",
                        "Bước 1: Tính tổng số tiền đầu tư = 6M * 3 = 18.000.000 đồng. Bước 2: Tính tổng số lượng CCQ mua được: Tháng 1 = 6.000.000 / 20.000 = 300 ccq; Tháng 2 = 6.000.000 / 15.000 = 400 ccq; Tháng 3 = 6.000.000 / 24.000 = 250 ccq. Tổng số CCQ = 300 + 400 + 250 = 950 ccq. Bước 3: Giá vốn bình quân = 18.000.000 / 950 ≈ 18.947,37 đồng/ccq. Phân tích phương án nhiễu: B sai vì tính trung bình cộng giản đơn các mức giá: (20k + 15k + 24k) / 3 = 19.667 đồng. Thực tế DCA cho giá vốn thấp hơn trung bình số học nhờ mua được nhiều hơn ở vùng giá rẻ (400 ccq ở giá 15k). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tính toán): Một ngân hàng thương mại chào lãi suất tiền gửi danh nghĩa 6,5%/năm. Cùng giai đoạn đó, tỷ lệ lạm phát thực tế là 3,5%/năm. Theo phương trình Fisher xấp xỉ và phương trình Fisher chính xác, lãi suất thực tế nhận được lần lượt là:",
                        "3,00% (xấp xỉ) và 2,90% (chính xác)",
                        "10,00% (xấp xỉ) và 9,80% (chính xác)",
                        "3,50% (xấp xỉ) và 3,20% (chính xác)",
                        "2,00% (xấp xỉ) và 1,85% (chính xác)",
                        "A",
                        "Bước 1: Phương trình Fisher xấp xỉ: r_real ≈ r_nominal - i = 6,5% - 3,5% = 3,00%. Bước 2: Phương trình Fisher chính xác: (1 + r_real) = (1 + r_nominal) / (1 + i) => r_real = (1 + 0,065) / (1 + 0,035) - 1 = 1,065 / 1,035 - 1 = 0,028985 ≈ 2,90%. Phân tích phương án nhiễu: B sai do lấy lãi danh nghĩa cộng lạm phát. C và D sai lệch số học. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tính toán): Bạn đặt mục tiêu cần có 121.000.000 đồng sau đúng 2 năm nữa để đóng học phí cao học. Với mức lợi suất kỳ vọng 10,0%/năm ghép lãi hàng năm, số tiền vốn hiện tại (PV) cần chuẩn bị đầu tư ngay hôm nay là bao nhiêu?",
                        "100.000.000 đồng",
                        "110.000.000 đồng",
                        "96.800.000 đồng",
                        "146.410.000 đồng",
                        "A",
                        "Bước 1: Công thức chiết khấu giá trị hiện tại: PV = FV / (1 + r)^n. Bước 2: Thay số liệu đề bài: FV = 121.000.000 đồng; r = 0,10; n = 2. PV = 121.000.000 / (1 + 0,10)^2 = 121.000.000 / 1,21 = 100.000.000 đồng. Phân tích phương án nhiễu: B sai vì chỉ chiết khấu cho 1 năm: 121M / 1,1 = 110M. C sai do lấy 121M trừ đi 20%. D sai do tính FV thay vì PV: 121M * 1,21. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tâm lý/Rủi ro): Khi thị trường chứng khoán giảm sâu 25% trong một đợt suy thoái ngắn hạn, phản ứng nào sau đây thể hiện kỷ luật đầu tư DCA đúng đắn và kiểm soát cảm xúc tốt nhất?",
                        "Hoảng loạn đặt lệnh bán tháo toàn bộ chứng chỉ quỹ ngay tại đáy để tránh mất thêm tiền",
                        "Dừng vĩnh viễn việc nộp tiền định kỳ vì mất niềm tin hoàn toàn vào thị trường tài chính",
                        "Kiên định duy trì lịch nộp tiền mua định kỳ theo đúng kế hoạch vì giá giảm chính là cơ hội mua tích lũy được nhiều chứng chỉ quỹ với giá vốn rẻ hơn",
                        "Vay nóng tín dụng đen bên ngoài để gấp thếp (martingale) bắt đáy toàn bộ tài sản",
                        "C",
                        "Bước 1: Nhận diện bản chất DCA là chiến lược dài hạn tận dụng biến động ngắn hạn. Bước 2: Khi giá giảm, người đầu tư DCA mua được lượng tài sản lớn nhất. Bán tháo ở đáy biến khoản lỗ trên giấy thành lỗ vĩnh viễn. Phân tích nhiễu: A và B là phản ứng tâm lý hoảng loạn bầy đàn (Panic Selling). D là hành vi đánh bạc cờ bạc làm tăng nguy cơ phá sản. Do đó C là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tâm lý/Rủi ro): Sai lầm tâm lý 'Hưng phấn đỉnh điểm' (Euphoria FOMO) thường dẫn đến cạm bẫy giải ngân nào sau đây đối với người mới tham gia đầu tư?",
                        "Dồn toàn bộ tiền tiết kiệm tích lũy nhiều năm vào mua một lần (Lump-sum) tại đúng vùng đỉnh hưng phấn khi thấy bạn bè xung quanh liên tục khoe lãi",
                        "Chia nhỏ số vốn để giải ngân đều đặn theo từng tháng bất chấp tin đồn thị trường",
                        "Trích lập đầy đủ quỹ dự phòng khẩn cấp trước khi bắt đầu giải ngân vào cổ phiếu",
                        "Kiên nhẫn ghi chép nhật ký đầu tư và rà soát các tiêu chí rủi ro trước khi ra quyết định",
                        "A",
                        "Bước 1: Phân tích cơ chế của FOMO (Fear of Missing Out): Khi thị trường lập đỉnh, tin tức truyền thông và mạng xã hội tràn ngập câu chuyện làm giàu nhanh, kích thích lòng tham. Bước 2: Người mới thường không kiềm chế được cảm xúc, phá vỡ kế hoạch DCA và dồn hết vốn một lần tại đỉnh. Phân tích nhiễu: B, C, D là các biểu hiện của nhà đầu tư có kỷ luật và phương pháp luận vững vàng. Do đó A là đáp án đúng."
                    )
                }
            ),

            // =========================================================================
            // 2. Quiz Bài 1.2: Kế Hoạch Tự Do Tài Chính (FIRE) & Quản Lý Dòng Tiền
            // =========================================================================
            BuildQuiz(
                "l12",
                SeedDataInitializer.InvestingLesson12Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 1.2: Kế Hoạch Tự Do Tài Chính (FIRE) & Quản Lý Dòng Tiền",
                now.AddSeconds(2),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Mô hình FIRE (Financial Independence, Retire Early) và Nghiên cứu Trinity định nghĩa trạng thái độc lập tài chính khi nào?",
                        "Khi giá trị khối tài sản đầu tư tích lũy đủ lớn để tỷ lệ rút vốn an toàn trang trải toàn bộ chi phí sinh hoạt hàng năm mà không còn phụ thuộc vào tiền lương chủ động",
                        "Khi cá nhân trúng giải độc đắc xổ số và tiêu xài thoải mái không cần ghi chép thu chi",
                        "Khi đạt đủ độ tuổi nghỉ hưu theo quy định của pháp luật lao động và nhận lương hưu nhà nước",
                        "Khi vay mượn tối đa tiền ngân hàng để gom mua thật nhiều bất động sản đầu cơ",
                        "A",
                        "Bước 1: Hiểu định nghĩa FIRE: Độc lập tài chính là khi dòng tiền sinh lời từ tài sản đầu tư tự trang trải đủ cuộc sống. Phân tích nhiễu: B là may rủi; C là nghỉ hưu truyền thống; D là đòn bẩy rủi ro cao. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Quy tắc 4% (Safe Withdrawal Rate - SWR) dựa trên kết quả của Trinity Study chỉ ra điều gì?",
                        "Nếu rút 4% giá trị danh mục trong năm đầu tiên và điều chỉnh theo lạm phát ở các năm sau, xác suất danh mục không cạn vốn trong 30 năm là trên 95% (với danh mục cổ phiếu/trái phiếu cân bằng)",
                        "Danh mục đầu tư của bạn sẽ được bảo hiểm luôn luôn tăng trưởng đúng 4% mỗi năm không bao giờ biến động",
                        "Nhà đầu tư chỉ được phép chi tiêu tối đa 4 triệu đồng mỗi tháng bất kể quy mô tài sản",
                        "Toàn bộ tài sản đầu tư bắt buộc phải gửi vào tài khoản tiết kiệm ngân hàng kỳ hạn 1 tháng",
                        "A",
                        "Bước 1: Trinity Study (1998) kiểm định dữ liệu thị trường Mỹ giai đoạn 1926-1995. Với danh mục 50% cổ phiếu - 50% trái phiếu, tỷ lệ rút 4% năm đầu và trượt giá theo lạm phát có xác suất thành công >95% trong 30 năm. Phân tích nhiễu: B, C, D là các hiểu lầm thô thiển về quy tắc 4%. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Quy mô tài sản cần tích lũy để đạt mốc FIRE theo quy tắc 4% bằng bao nhiêu lần chi phí sinh hoạt hàng năm?",
                        "25 lần chi phí sinh hoạt năm (Số tiền FIRE = Chi phí năm / 0,04 = Chi phí năm * 25)",
                        "10 lần chi phí sinh hoạt năm",
                        "50 lần chi phí sinh hoạt năm",
                        "5 lần chi phí sinh hoạt năm",
                        "A",
                        "Bước 1: Công thức: Nếu Chi phí năm = 4% * Tài sản FIRE, thì Tài sản FIRE = Chi phí năm / 0,04 = Chi phí năm * 25. Ví dụ chi tiêu 200M/năm thì cần 200M * 25 = 5 tỷ đồng. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Tỷ lệ tiết kiệm ròng (Savings Rate) có ý nghĩa quyết định như thế nào đối với thời gian đạt được FIRE?",
                        "Tỷ lệ tiết kiệm ròng càng cao thì thời gian cần lao động tích lũy để đạt FIRE càng rút ngắn mạnh mẽ vì vừa tăng tiền tích lũy vừa rèn luyện lối sống chi tiêu hợp lý",
                        "Tỷ lệ tiết kiệm ròng chỉ phản ánh mức độ ki bo của một người chứ không liên quan đến thời gian nghỉ hưu",
                        "Tỷ lệ tiết kiệm ròng trên 50% sẽ làm giảm chỉ số tín dụng cá nhân tại ngân hàng",
                        "Người có thu nhập cao thì không cần quan tâm đến tỷ lệ tiết kiệm ròng",
                        "A",
                        "Bước 1: Theo biểu đồ Shockingly Simple Math Behind Early Retirement của Mr. Money Mustache: Với lợi suất thực 5%, nếu tiết kiệm 10% thu nhập bạn cần 51 năm làm việc; nhưng nếu tăng lên 50% bạn chỉ cần 17 năm; nếu đạt 65% bạn chỉ cần 10,5 năm. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tính toán): Gia đình bạn có chi phí sinh hoạt thiết yếu trung bình là 20.000.000 đồng/tháng (tương đương 240.000.000 đồng/năm). Theo quy tắc 4%, quy mô khối tài sản đầu tư cần tích lũy để đạt mốc FIRE độc lập tài chính là bao nhiêu?",
                        "6,0 tỷ đồng",
                        "4,8 tỷ đồng",
                        "2,4 tỷ đồng",
                        "12,0 tỷ đồng",
                        "A",
                        "Bước 1: Tính chi phí năm = 20.000.000 * 12 = 240.000.000 đồng. Bước 2: Tài sản mục tiêu FIRE = Chi phí năm * 25 = 240.000.000 * 25 = 6.000.000.000 đồng (6,0 tỷ đồng). Phân tích phương án nhiễu: B là 240M * 20 = 4,8 tỷ (tương ứng quy tắc rút 5%). C là 240M * 10 (chỉ đủ sống 10 năm). D là 240M * 50 (quy tắc rút 2%). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tính toán): Anh Tuấn có thu nhập thực nhận sau thuế là 30.000.000 đồng/tháng. Tổng chi tiêu sinh hoạt là 18.000.000 đồng/tháng. Toàn bộ khoản thặng dư 12.000.000 đồng được đưa vào quỹ đầu tư sinh lời. Tỷ lệ tiết kiệm ròng (Savings Rate) của anh Tuấn là:",
                        "40,0%",
                        "60,0%",
                        "66,7%",
                        "25,0%",
                        "A",
                        "Bước 1: Công thức tỷ lệ tiết kiệm ròng = Thặng dư tích lũy / Tổng thu nhập = (Thu nhập - Chi tiêu) / Thu nhập. Bước 2: Thay số: (30M - 18M) / 30M = 12M / 30M = 0,40 = 40,0%. Phân tích phương án nhiễu: B sai do nhầm sang tỷ lệ chi tiêu (18M/30M = 60%). C sai do lấy 12M chia cho chi tiêu 18M. D sai lệch số học. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tính toán): Một người đạt mốc FIRE với khối danh mục tài sản 5,0 tỷ đồng. Trong năm đầu tiên nghỉ hưu, số tiền người này có thể rút ra chi tiêu an toàn theo quy tắc 4% là bao nhiêu (tính theo năm và trung bình mỗi tháng)?",
                        "200.000.000 đồng/năm (khoảng 16.666.667 đồng/tháng)",
                        "250.000.000 đồng/năm (khoảng 20.833.333 đồng/tháng)",
                        "100.000.000 đồng/năm (khoảng 8.333.333 đồng/tháng)",
                        "400.000.000 đồng/năm (khoảng 33.333.333 đồng/tháng)",
                        "A",
                        "Bước 1: Số tiền rút năm đầu = 5.000.000.000 * 4% = 200.000.000 đồng/năm. Bước 2: Chia cho 12 tháng: 200.000.000 / 12 = 16.666.667 đồng/tháng. Phân tích phương án nhiễu: B tương ứng tỷ lệ rút 5% (250M). C tương ứng tỷ lệ rút 2% (100M). D tương ứng tỷ lệ rút 8% (400M, có rủi ro cạn vốn rất cao). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tính toán): Bạn loại bỏ các khoản rò rỉ vi mô (Micro-leaks) như hủy đăng ký ứng dụng không dùng và tự nấu ăn, tiết kiệm được 1.500.000 đồng/tháng. Đem số tiền này đầu tư định kỳ hàng tháng với tỷ suất sinh lời 10%/năm trong 10 năm (120 tháng). Giá trị tương lai (FV) ước tính nhận được sau 10 năm xấp xỉ là bao nhiêu?",
                        "Khoảng 307 triệu đồng (Tổng vốn gốc 180 triệu, tiền lãi sinh ra khoảng 127 triệu đồng)",
                        "Đúng 180 triệu đồng (Chỉ bằng tổng số tiền gốc không có lãi)",
                        "Khoảng 210 triệu đồng",
                        "Khoảng 650 triệu đồng",
                        "A",
                        "Bước 1: Đây là bài toán tính giá trị tương lai của dòng tiền đều (Annuity FV): FV = PMT * [((1 + r_m)^n - 1) / r_m]. Với PMT = 1.500.000 đ; lãi tháng r_m = 10% / 12 ≈ 0,8333%/tháng; n = 120 tháng. Bước 2: FV ≈ 1.500.000 * [((1 + 0,008333)^120 - 1) / 0,008333] ≈ 307.300.000 đồng. Tổng gốc = 1,5M * 120 = 180 triệu đồng. Lãi kép tạo ra = 307M - 180M = 127 triệu đồng. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tâm lý/Rủi ro): Hiện tượng 'Lạm phát lối sống' (Lifestyle Creep) là kẻ thù nguy hiểm nhất cản trở lộ trình FIRE vì:",
                        "Khi thu nhập tăng lên, người ta có xu hướng tự động nâng cấp mức chi tiêu tương ứng (mua xe đắt hơn, ở nhà sang hơn), khiến tỷ lệ tiết kiệm ròng không hề tăng dù kiếm được nhiều tiền hơn",
                        "Tỷ lệ lạm phát của nền kinh tế tự động tăng vọt mỗi khi một cá nhân được tăng lương",
                        "Các công ty quản lý quỹ từ chối nhận tiền đầu tư của người có lối sống tiết kiệm",
                        "Ngân hàng trung ương bắt buộc người dân phải chi tiêu hết tiền lương trong tháng",
                        "A",
                        "Bước 1: Phân tích tâm lý học: Lạm phát lối sống khiến chi phí sinh hoạt (Needs & Wants) phình to theo đà tăng thu nhập. Bước 2: Khi chi phí năm tăng, con số mục tiêu FIRE (25 lần chi phí năm) bị đẩy ra xa hơn liên tục. Cách khắc phục: Mỗi khi được tăng lương, trích ngay 50% khoản tăng thêm vào quỹ đầu tư tự động. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tâm lý/Rủi ro): 'Rủi ro chuỗi lợi nhuận' (Sequence of Returns Risk) đối với người mới bước vào giai đoạn nghỉ hưu FIRE là gì và biện pháp phòng vệ hữu hiệu nhất là gì?",
                        "Thị trường sụt giảm mạnh ngay trong 2–3 năm đầu tiên sau khi nghỉ hưu khiến việc rút vốn ăn mòn nghiêm trọng vào gốc tài sản; biện pháp phòng vệ là thiết lập 'bộ đệm tiền mặt/trái phiếu ngắn hạn' tương đương 2–3 năm chi tiêu",
                        "Danh mục tăng trưởng quá mạnh làm người nghỉ hưu phải nộp nhiều thuế thu nhập cá nhân hơn",
                        "Nguy cơ ngân hàng tự ý khóa tài khoản vì không có dòng tiền lương hàng tháng đổ về",
                        "Sự thay đổi chính sách tiền tệ khiến toàn bộ cổ phiếu trên sàn bị hủy niêm yết",
                        "A",
                        "Bước 1: Hiểu bản chất: Rút tiền khi thị trường đang rơi đáy buộc nhà đầu tư phải bán tài sản với giá rất rẻ, khiến danh mục không thể hồi phục khi thị trường quay đầu tăng. Bước 2: Biện pháp phòng vệ: Tạo Cash Buffer 2-3 năm chi tiêu. Khi thị trường giảm, chi tiêu từ tiền mặt dự phòng và không đụng vào cổ phiếu. Do đó A là đáp án đúng."
                    )
                }
            ),

            // =========================================================================
            // 3. Quiz Bài 2.1: Phân Bổ Tài Sản & Tương Quan Giữa Các Lớp Tài Sản
            // =========================================================================
            BuildQuiz(
                "l21",
                SeedDataInitializer.InvestingLesson21Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 2.1: Phân Bổ Tài Sản & Tương Quan Đa Tài Sản",
                now.AddSeconds(3),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Luận điểm cốt lõi của Lý thuyết Danh mục Hiện đại (Modern Portfolio Theory - MPT) do Harry Markowitz khởi xướng là:",
                        "Phân bổ tài sản chiến lược giữa các nhóm tài sản có tương quan thấp giúp giảm đáng kể rủi ro biến động mà không phải đánh đổi tỷ suất sinh lời kỳ vọng",
                        "Chọn được đúng 1 mã cổ phiếu tăng trưởng tốt nhất quan trọng hơn toàn bộ việc phân bổ danh mục",
                        "Luôn luôn dồn 100% tài sản vào một kênh đầu tư duy nhất để tối đa hóa lợi nhuận",
                        "Thị trường tài chính hoàn toàn không có bất kỳ rủi ro nào nếu nắm giữ trên 1 tháng",
                        "A",
                        "Bước 1: Markowitz chỉ ra rằng rủi ro của danh mục không chỉ là trung bình rủi ro của từng tài sản riêng lẻ, mà phụ thuộc vào hệ số hiệp phương sai/tương quan giữa chúng. Kết hợp các tài sản không cùng pha giúp triệt tiêu rủi ro phi hệ thống. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Hệ số tương quan (Correlation Coefficient - r) giữa hai lớp tài sản nhận giá trị trong khoảng nào và giá trị nào mang lại hiệu quả đa dạng hóa cao nhất?",
                        "Từ -1,0 đến +1,0; trong đó r = -1,0 là tương quan nghịch hoàn hảo, đem lại hiệu quả triệt tiêu biến động rủi ro triệt để nhất",
                        "Từ 0 đến 100%; trong đó 100% là đa dạng hóa tốt nhất",
                        "Từ 1,0 đến vô cùng; r càng lớn đa dạng hóa càng mạnh",
                        "Luôn luôn bằng 0 trong mọi hoàn cảnh thị trường",
                        "A",
                        "Bước 1: Hệ số tương quan r nằm trong đoạn [-1, +1]. r = +1: cùng chiều hoàn hảo (không giảm rủi ro). r = 0: độc lập. r = -1: ngược chiều hoàn hảo (khi một tài sản giảm thì tài sản kia tăng bù đắp, triệt tiêu biến động). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Vai trò và đặc tính của 3 lớp tài sản cơ bản: Cổ phiếu, Trái phiếu chính phủ, và Tiền mặt/Tiết kiệm trong danh mục là:",
                        "Cổ phiếu là động lực tăng trưởng vốn dài hạn nhưng biến động lớn; Trái phiếu cung cấp dòng tiền cố định và giảm sốc; Tiền mặt mang lại thanh khoản tức thời và bảo toàn vốn danh nghĩa",
                        "Cả ba lớp tài sản đều có mức độ rủi ro và khả năng sinh lời hoàn toàn giống hệt nhau",
                        "Trái phiếu chính phủ luôn có độ rủi ro phá sản cao hơn cổ phiếu doanh nghiệp",
                        "Tiền mặt là tài sản sinh lời vượt trội nhất giúp đánh bại lạm phát dài hạn",
                        "A",
                        "Bước 1: Hiểu bản chất từng lớp tài sản: Cổ phiếu (Equity) - tăng trưởng; Trái phiếu (Fixed Income) - thu nhập cố định và phòng thủ; Tiền mặt (Cash) - thanh khoản. Kết hợp 3 lớp giúp danh mục chịu đựng được các chu kỳ kinh tế khác nhau. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Hoạt động Tái cân bằng danh mục (Portfolio Rebalancing) được định nghĩa chính xác là:",
                        "Việc đưa tỷ trọng của các lớp tài sản trong danh mục quay trở lại mức mục tiêu ban đầu sau khi biến động thị trường làm lệch tỷ trọng phân bổ",
                        "Bán toàn bộ danh mục cũ để mua toàn bộ danh mục mới theo lời khuyên của người quen",
                        "Vay thêm tiền ngân hàng để mua đuổi những tài sản vừa tăng giá mạnh",
                        "Đổi toàn bộ cổ phiếu sang vàng mỗi khi thị trường chứng khoán có phiên điều chỉnh đỏ",
                        "A",
                        "Bước 1: Tái cân bằng danh mục (Rebalancing) là quy trình kỷ luật định kỳ (theo thời gian 6 tháng/1 năm hoặc theo ngưỡng lệch ±5%). Nó tự động buộc nhà đầu tư chốt lời tài sản đã tăng nóng và mua thêm tài sản đang định giá rẻ. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tính toán): Một danh mục đầu tư được phân bổ 60% vào Cổ phiếu (tỷ suất sinh lời kỳ vọng 12,0%/năm) và 40% vào Trái phiếu (tỷ suất sinh lời kỳ vọng 6,0%/năm). Tỷ suất sinh lời kỳ vọng hàng năm của toàn bộ danh mục là bao nhiêu?",
                        "9,60%/năm",
                        "9,00%/năm",
                        "8,40%/năm",
                        "10,20%/năm",
                        "A",
                        "Bước 1: Công thức tỷ suất sinh lời kỳ vọng của danh mục: E(R_p) = w_1 * R_1 + w_2 * R_2. Bước 2: Thay số: w_1 = 0,60; R_1 = 12,0%; w_2 = 0,40; R_2 = 6,0%. E(R_p) = (0,60 * 12,0%) + (0,40 * 6,0%) = 7,2% + 2,4% = 9,60%/năm. Phân tích phương án nhiễu: B sai do tính trung bình cộng giản đơn: (12% + 6%)/2 = 9,0% (bỏ qua tỷ trọng 60/40). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tính toán): Danh mục ban đầu trị giá 100 triệu đồng có tỷ trọng mục tiêu 70% Cổ phiếu (70 triệu) và 30% Trái phiếu (30 triệu). Sau 1 năm, Cổ phiếu tăng 30% lên 91 triệu đồng, Trái phiếu tăng 5% lên 31,5 triệu đồng (Tổng danh mục = 122,5 triệu đồng). Tỷ trọng mới của Cổ phiếu và hành động tái cân bằng là gì?",
                        "Tỷ trọng Cổ phiếu tăng lên 74,29%; cần bán bớt 5,25 triệu đồng cổ phiếu để mua thêm trái phiếu nhằm đưa tỷ trọng về đúng 70/30",
                        "Tỷ trọng Cổ phiếu giảm xuống 65%; cần mua thêm cổ phiếu",
                        "Giữ nguyên không làm gì vì cả hai tài sản đều đang có lãi",
                        "Bán toàn bộ 91 triệu đồng cổ phiếu chuyển sang gửi tiết kiệm ngân hàng",
                        "A",
                        "Bước 1: Tổng danh mục sau 1 năm = 91 + 31,5 = 122,5 triệu đồng. Tỷ trọng cổ phiếu mới = 91 / 122,5 ≈ 74,29% (vượt quá mục tiêu 70%). Bước 2: Giá trị cổ phiếu mục tiêu = 122,5 * 70% = 85,75 triệu đồng. Bước 3: Số tiền cổ phiếu cần bán = 91 - 85,75 = 5,25 triệu đồng. Đem 5,25 triệu này mua thêm trái phiếu (Trái phiếu mới = 31,5 + 5,25 = 36,75 triệu = 30% * 122,5M). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tính toán): Nhà đầu tư 30 tuổi áp dụng quy tắc kinh nghiệm 'Tỷ lệ cổ phiếu = 110 - Tuổi' để phân bổ số vốn 200.000.000 đồng. Số tiền phân bổ vào Cổ phiếu và Trái phiếu lần lượt là bao nhiêu?",
                        "Cổ phiếu: 160.000.000 đồng (80%); Trái phiếu: 40.000.000 đồng (20%)",
                        "Cổ phiếu: 140.000.000 đồng (70%); Trái phiếu: 60.000.000 đồng (30%)",
                        "Cổ phiếu: 100.000.000 đồng (50%); Trái phiếu: 100.000.000 đồng (50%)",
                        "Cổ phiếu: 180.000.000 đồng (90%); Trái phiếu: 20.000.000 đồng (10%)",
                        "A",
                        "Bước 1: Tính tỷ lệ cổ phiếu = 110 - 30 = 80%. Tỷ lệ trái phiếu = 100% - 80% = 20%. Bước 2: Số tiền vào Cổ phiếu = 200.000.000 * 80% = 160.000.000 đồng. Số tiền vào Trái phiếu = 200.000.000 * 20% = 40.000.000 đồng. Phân tích phương án nhiễu: B là theo quy tắc cũ 100 - Tuổi (70/30). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tính toán): Hai tài sản A và B có độ lệch chuẩn lần lượt là 20% và 10%, hệ số tương quan r = 0. Nếu phân bổ tỷ trọng 50/50, phương sai danh mục là 0,0125. Độ lệch chuẩn của danh mục (căn bậc hai của phương sai) xấp xỉ là bao nhiêu?",
                        "11,18%",
                        "15,00%",
                        "20,00%",
                        "10,00%",
                        "A",
                        "Bước 1: Tính căn bậc hai của phương sai: σ_P = √(0,0125) ≈ 0,1118 = 11,18%. Phân tích phương án nhiễu: B sai do tính trung bình cộng giản đơn của hai độ lệch chuẩn: (20% + 10%) / 2 = 15,0%. Việc kết hợp hai tài sản độc lập giúp độ lệch chuẩn rủi ro giảm từ 15% xuống chỉ còn 11,18% (giảm gần 25% rủi ro biến động). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tâm lý/Rủi ro): Rào cản tâm lý lớn nhất khiến đa số nhà đầu tư thất bại trong việc thực hiện kỷ luật Tái cân bằng danh mục là gì?",
                        "Phải chiến thắng cảm giác tham lam và tiếc nuối khi buộc phải bán bớt tài sản đang tăng trưởng nóng để mua thêm tài sản đang ảm đạm hoặc sụt giảm",
                        "Sở giao dịch chứng khoán thu thuế phạt rất nặng đối với hành vi tái cân bằng",
                        "Không thể dùng máy tính để tính toán được tỷ trọng phần trăm tài sản",
                        "Công ty chứng khoán cấm khách hàng nắm giữ đồng thời cả cổ phiếu và trái phiếu",
                        "A",
                        "Bước 1: Về mặt tâm lý học hành vi: Con người có xu hướng bị cuốn theo hiệu ứng bầy đàn (Herding) và thiên kiến ngoại suy (Extrapolation Bias - tin rằng tài sản đang tăng sẽ tăng mãi). Tái cân bằng đòi hỏi hành động ngược dòng (Contrarian): bán tài sản tăng để mua tài sản giảm. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tâm lý/Rủi ro): Thiên kiến 'Ưa thích tài sản quen thuộc' (Home Country / Familiarity Bias) dẫn đến sai lầm cấu trúc danh mục nào sau đây?",
                        "Dồn hầu hết tài sản vào một công ty nơi mình đang công tác hoặc chỉ mua bất động sản tại đúng khu vực mình sinh sống, bỏ qua hoàn toàn các cơ hội đa dạng hóa rộng lớn trên toàn thị trường",
                        "Phân bổ quá nhiều tiền vào các quỹ chỉ số toàn cầu ETF chi phí thấp",
                        "Nắm giữ quá nhiều trái phiếu chính phủ an toàn trong danh mục hưu trí",
                        "Thường xuyên rà soát lại mức độ tương quan giữa các lớp tài sản",
                        "A",
                        "Bước 1: Familiarity Bias khiến nhà đầu tư nhầm lẫn giữa sự quen thuộc và sự an toàn. Ví dụ: mua cổ phiếu của chính công ty mình làm việc khiến cả thu nhập lương lẫn tài sản đầu tư cùng phụ thuộc vào một rủi ro duy nhất. Do đó A là đáp án đúng."
                    )
                }
            ),

            // =========================================================================
            // 4. Quiz Bài 2.2: Đọc Báo Cáo Tài Chính & Chỉ Số Định Giá (P/E, P/B, ROE)
            // =========================================================================
            BuildQuiz(
                "l22",
                SeedDataInitializer.InvestingLesson22Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 2.2: Đọc BCTC & Chỉ Số Định Giá (P/E, P/B, ROE)",
                now.AddSeconds(4),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Phương trình kế toán cốt lõi làm nền tảng cho Bảng Cân đối Kế toán (Balance Sheet) là:",
                        "Tổng Tài sản = Tổng Nợ phải trả + Vốn chủ sở hữu",
                        "Tổng Tài sản = Doanh thu thuần - Giá vốn hàng bán",
                        "Tổng Tài sản = Dòng tiền kinh doanh + Dòng tiền tài chính",
                        "Tổng Tài sản = Lợi nhuận sau thuế * Chỉ số P/E",
                        "A",
                        "Bước 1: Nguyên tắc bất biến của kế toán tài chính: Mọi tài sản của doanh nghiệp (Asset) đều được tài trợ từ hai nguồn: Nợ vay (Liabilities) hoặc Vốn góp của cổ đông (Equity). Do đó Tổng Tài sản = Tổng Nợ phải trả + Vốn chủ sở hữu. Đáp án A chính xác."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Chỉ số P/E (Price-to-Earnings) phản ánh điều gì và khi nào P/E rất thấp có thể là 'Bẫy giá trị' (Value Trap)?",
                        "P/E phản ánh mức giá nhà đầu tư sẵn sàng trả cho 1 đồng lợi nhuận sau thuế của doanh nghiệp; P/E rất thấp có thể là bẫy nếu lợi nhuận đột biến đến từ bán tài sản một lần hoặc doanh nghiệp đang suy thoái mất thị phần vĩnh viễn",
                        "P/E thấp luôn chứng minh cổ phiếu chắc chắn sẽ tăng giá gấp 3 lần trong tháng tiếp theo",
                        "P/E là tỷ lệ nợ vay ngân hàng chia cho vốn điều lệ của công ty cổ phần",
                        "P/E chỉ tính được cho những doanh nghiệp kinh doanh thua lỗ liên tiếp nhiều năm",
                        "A",
                        "Bước 1: P/E = Thị giá / EPS. P/E cho biết cần bao nhiêu năm để hòa vốn nếu lợi nhuận giữ nguyên. Bước 2: Cảnh giác Value Trap: Doanh nghiệp có P/E = 4 do thanh lý nhà xưởng (lợi nhuận ảo 1 lần) hoặc ngành than đá/thép ở đỉnh chu kỳ lợi nhuận. Khi chu kỳ đảo chiều, E sụt giảm mạnh và P/E thực tế tăng vọt. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Chỉ số ROE (Return on Equity) đo lường điều gì và tiêu chuẩn doanh nghiệp kinh doanh hiệu quả của Warren Buffett là gì?",
                        "ROE đo lường hiệu quả sinh lời trên vốn của cổ đông (Tạo ra bao nhiêu đồng lợi nhuận từ mỗi 100 đồng vốn chủ sở hữu); Warren Buffett ưa thích doanh nghiệp duy trì ROE ổn định trên 15%–20%",
                        "ROE đo lường tổng doanh thu bằng tiền mặt thu về trong cả năm",
                        "ROE là tỷ lệ phần trăm cổ tức tiền mặt chia cho ban điều hành doanh nghiệp",
                        "ROE đo lường mức độ biến động tăng giảm giá cổ phiếu trên bảng điện tử",
                        "A",
                        "Bước 1: ROE = Lợi nhuận sau thuế / Vốn chủ sở hữu bình quân. ROE thể hiện năng lực sinh lời của ban lãnh đạo trên đồng vốn của cổ đông. Warren Buffett xem ROE > 15% liên tục trong 5-10 năm là dấu hiệu của doanh nghiệp có con hào kinh tế bền vững. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Sự khác biệt căn bản giữa Doanh thu / Lợi nhuận trên Báo cáo Kết quả Kinh doanh và Dòng tiền thuần từ kinh doanh (CFO) trên Báo cáo Lưu chuyển Tiền tệ là:",
                        "Báo cáo KQKD ghi nhận theo nguyên tắc dồn tích (Accrual - ghi nhận doanh thu khi giao hàng dù chưa thu được tiền), trong khi CFO ghi nhận tiền thực tế ra - vào tài khoản doanh nghiệp",
                        "Hai báo cáo này hoàn toàn đồng nhất về số liệu và cách thức hạch toán tiền mặt",
                        "CFO chỉ dùng để nộp thuế thu nhập, còn nhà đầu tư chỉ được xem Báo cáo KQKD",
                        "Dòng tiền CFO luôn luôn phải bằng đúng 100% lợi nhuận sau thuế của công ty",
                        "A",
                        "Bước 1: Kế toán dồn tích cho phép ghi nhận doanh thu khi xuất hóa đơn (dù khách hàng còn nợ tiền). Nếu doanh nghiệp bán chịu quá nhiều, doanh thu và lãi trên sổ sách rất cao nhưng thực tế không có tiền mặt về két (CFO âm). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tính toán): Công ty Cổ phần Thủy sản An Phát có thị giá cổ phiếu là 42.000 đồng/cổ phiếu. Lợi nhuận sau thuế trên mỗi cổ phần (EPS) 4 quý gần nhất là 3.500 đồng. Chỉ số P/E hiện tại của cổ phiếu là bao nhiêu?",
                        "12,0 lần",
                        "8,3 lần",
                        "15,0 lần",
                        "10,5 lần",
                        "A",
                        "Bước 1: Công thức tính chỉ số P/E = Thị giá (P) / Thu nhập trên mỗi cổ phần (EPS). Bước 2: Thay số: P = 42.000 đồng; EPS = 3.500 đồng. P/E = 42.000 / 3.500 = 12,0 lần. Phân tích phương án nhiễu: B sai do lấy EPS chia cho P (3.500 / 42.000 ≈ 8,3% là tỷ suất thu nhập E/P). C và D sai lệch số học. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tính toán): Doanh nghiệp X có Vốn chủ sở hữu bình quân trong năm là 400 tỷ đồng. Lợi nhuận sau thuế năm 2025 đạt 88 tỷ đồng. Chỉ số ROE của doanh nghiệp X là bao nhiêu và có đạt tiêu chuẩn doanh nghiệp tốt của Warren Buffett (ROE > 15%) không?",
                        "ROE = 22,0%; đạt tiêu chuẩn vượt trội của Warren Buffett (88 / 400 = 22,0% > 15%)",
                        "ROE = 12,5%; không đạt tiêu chuẩn",
                        "ROE = 4,5%; rất yếu kém",
                        "ROE = 30,0%",
                        "A",
                        "Bước 1: Công thức tính ROE = Lợi nhuận sau thuế / Vốn chủ sở hữu = 88 / 400 = 0,22 = 22,0%. Bước 2: So sánh tiêu chuẩn Warren Buffett: 22,0% > 15,0%, chứng tỏ doanh nghiệp có hiệu quả sử dụng vốn cổ đông rất xuất sắc. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tính toán): Cổ phiếu Ngân hàng Á Châu có Giá trị sổ sách mỗi cổ phần (BVPS) là 20.000 đồng. Thị giá đang giao dịch khớp lệnh trên sàn chứng khoán là 26.000 đồng. Chỉ số P/B (Price-to-Book Ratio) của cổ phiếu là:",
                        "1,30 lần",
                        "0,77 lần",
                        "1,60 lần",
                        "2,00 lần",
                        "A",
                        "Bước 1: Công thức tính chỉ số P/B = Thị giá (P) / Giá trị sổ sách mỗi cổ phần (BVPS). Bước 2: Thay số: P = 26.000 đồng; BVPS = 20.000 đồng. P/B = 26.000 / 20.000 = 1,30 lần. Phân tích phương án nhiễu: B sai do lấy nghịch đảo BVPS / P = 20.000 / 26.000 ≈ 0,77. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tính toán): Doanh nghiệp Bán lẻ Thế Giới Số có Doanh thu thuần đạt 1.200 tỷ đồng. Giá vốn hàng bán (COGS) là 900 tỷ đồng. Chi phí bán hàng và quản lý (OPEX) là 180 tỷ đồng. Thuế TNDN là 24 tỷ đồng. Lợi nhuận gộp và Biên lợi nhuận ròng (Net Profit Margin) của công ty lần lượt là:",
                        "Lợi nhuận gộp = 300 tỷ đồng; Biên lợi nhuận ròng = 8,0%",
                        "Lợi nhuận gộp = 400 tỷ đồng; Biên lợi nhuận ròng = 12,0%",
                        "Lợi nhuận gộp = 300 tỷ đồng; Biên lợi nhuận ròng = 25,0%",
                        "Lợi nhuận gộp = 200 tỷ đồng; Biên lợi nhuận ròng = 5,0%",
                        "A",
                        "Bước 1: Lợi nhuận gộp = Doanh thu thuần - Giá vốn hàng bán = 1.200 - 900 = 300 tỷ đồng. Bước 2: Lợi nhuận ròng sau thuế = Lợi nhuận gộp - Chi phí bán hàng & QLDN - Thuế TNDN = 300 - 180 - 24 = 96 tỷ đồng. Bước 3: Biên lợi nhuận ròng = 96 / 1.200 = 0,08 = 8,0%. Phân tích phương án nhiễu: C sai do nhầm sang Biên lợi nhuận gộp (300 / 1.200 = 25,0%). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tâm lý/Rủi ro): Dấu hiệu cảnh báo nguy cơ thao túng hoặc thổi phồng số liệu lợi nhuận (Earnings Quality Warning) khi đọc báo cáo tài chính là:",
                        "Doanh thu và lợi nhuận trên Báo cáo KQKD tăng trưởng liên tục nhưng Dòng tiền thuần từ hoạt động kinh doanh (CFO) âm nặng triền miên nhiều năm, đi kèm Khoản phải thu khách hàng phình to bất thường",
                        "Dòng tiền CFO dương lớn hơn cả lợi nhuận sau thuế và doanh nghiệp chi trả cổ tức tiền mặt đều đặn",
                        "Doanh nghiệp chủ động trả hết các khoản nợ vay ngân hàng ngắn hạn",
                        "Tỷ lệ trích lập dự phòng nợ khó đòi tăng lên theo đúng quy định kế toán",
                        "A",
                        "Bước 1: Kỹ thuật kiểm tra chất lượng lợi nhuận: So sánh Lợi nhuận sau thuế với Dòng tiền CFO. Nếu CFO âm liên tục trong khi LNST dương lớn, doanh nghiệp đang bán chịu không thu được tiền (nguy cơ tạo doanh thu ảo qua các bên liên quan) hoặc hàng tồn kho ứ đọng. Đây là dấu hiệu rủi ro cao của bẫy kế toán. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tâm lý/Rủi ro): Sai lầm phương pháp luận phổ biến nhất khi người mới sử dụng chỉ số P/E để chọn cổ phiếu là gì?",
                        "So sánh chéo chỉ số P/E trực tiếp giữa hai doanh nghiệp thuộc hai ngành có đặc thù hoàn toàn khác biệt (ví dụ P/E ngành Bất động sản vs P/E ngành Công nghệ phần mềm) mà không xét đến chu kỳ và tỷ suất ROE",
                        "So sánh P/E của một doanh nghiệp với P/E trung bình lịch sử 5 năm của chính nó",
                        "So sánh P/E của doanh nghiệp với các đối thủ cạnh tranh trực tiếp cùng ngành",
                        "Kết hợp xem xét P/E cùng với tốc độ tăng trưởng EPS (chỉ số PEG)",
                        "A",
                        "Bước 1: Mỗi ngành nghề có cấu trúc vốn, mức độ thâm dụng tài sản, rào cản gia nhập và tốc độ tăng trưởng kỳ vọng khác nhau. Ngành công nghệ có ROE cao và tăng trưởng 30%/năm xứng đáng có P/E 25x, trong khi ngành thép có tính chu kỳ cao thường chỉ giao dịch ở P/E 6x-8x. So sánh máy móc bỏ qua bối cảnh ngành là sai lầm kinh điển. Do đó A là đáp án đúng."
                    )
                }
            ),

            // =========================================================================
            // 5. Quiz Bài 3.1: Biên An Toàn & Quỹ Chỉ Số ETF
            // =========================================================================
            BuildQuiz(
                "l31",
                SeedDataInitializer.InvestingLesson31Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 3.1: Biên An Toàn & Quỹ Chỉ Số ETF",
                now.AddSeconds(5),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Khái niệm 'Biên an toàn' (Margin of Safety) theo Benjamin Graham và Warren Buffett được định nghĩa chuẩn xác là:",
                        "Khoảng chênh lệch chiết khấu khi mua một tài sản ở mức thị giá thấp hơn đáng kể so với Giá trị nội tại ước tính, nhằm bảo vệ nhà đầu tư trước các sai số dự báo và biến cố bất ngờ",
                        "Mức lãi suất tối thiểu mà ngân hàng nhà nước cam kết chi trả cho người gửi tiền",
                        "Hạn mức thấu chi vay nợ mà công ty chứng khoán cấp sẵn trong tài khoản giao dịch",
                        "Quy định pháp luật cấm doanh nghiệp niêm yết không được để giá cổ phiếu giảm",
                        "A",
                        "Bước 1: Benjamin Graham trong tác phẩm The Intelligent Investor xem Biên an toàn là hòn đá tảng của triết lý đầu tư giá trị. Biên an toàn hấp thụ các cú sốc tương lai và những sai sót không thể tránh khỏi trong các giả định tài chính. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Quỹ hoán đổi danh mục (ETF - Exchange Traded Fund) khác biệt căn bản với Quỹ mở truyền thống (Mutual Fund) ở điểm nào?",
                        "Chứng chỉ quỹ ETF được niêm yết và giao dịch trực tiếp trên sàn chứng khoán theo thời gian thực như một cổ phiếu thông thường, với chi phí quản lý thấp và danh mục minh bạch hàng ngày",
                        "Quỹ ETF chỉ cho phép nhà đầu tư mua bán trực tiếp với công ty quản lý quỹ vào cuối ngày theo giá NAV",
                        "Quỹ ETF cam kết luôn đem lại tỷ suất sinh lời tối thiểu 20%/năm bằng văn bản",
                        "Quỹ ETF không được phép nắm giữ cổ phiếu mà chỉ được nắm giữ tiền mặt",
                        "A",
                        "Bước 1: ETF kết hợp tính đa dạng hóa của quỹ mở với tính thanh khoản linh hoạt của cổ phiếu niêm yết. Nhà đầu tư có thể mua bán chứng chỉ quỹ ETF trên bảng điện tử trong suốt phiên giao dịch với chi phí quản trị (TER) thường thấp hơn rất nhiều so với quỹ mở chủ động. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Tác động lâu dài của Chi phí quản lý quỹ (Total Expense Ratio - TER) đối với hiệu quả tích lũy lãi kép trong 20–30 năm:",
                        "Phí quản lý bị khấu trừ trực tiếp vào giá trị tài sản ròng hàng ngày; chênh lệch phí chỉ 1%–1,5%/năm sau 20–30 năm có thể làm sụt giảm tới 25%–35% tổng tài sản tích lũy của nhà đầu tư",
                        "Phí quản lý quỹ hoàn toàn không ảnh hưởng gì đến số tiền nhận về khi tất toán danh mục",
                        "Quỹ có phí quản lý càng đắt đỏ thì chắc chắn hiệu quả sinh lời thực tế trong dài hạn càng cao",
                        "Toàn bộ phí quản lý quỹ sẽ được hoàn trả 100% khi nhà đầu tư đạt tuổi nghỉ hưu",
                        "A",
                        "Bước 1: John Bogle (nhà sáng lập Vanguard) đã chỉ ra rằng trong đầu tư tài chính, bạn nhận được những gì bạn KHÔNG phải trả. Chi phí quản lý tích lũy theo thời gian lấy đi một phần rất lớn của thành quả lãi kép. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Sai lệch bám sát (Tracking Error) của quỹ chỉ số ETF đo lường điều gì?",
                        "Mức độ chênh lệch giữa tỷ suất sinh lời của quỹ ETF so với tỷ suất sinh lời của chỉ số tham chiếu mà quỹ mô phỏng; tracking error càng nhỏ chứng tỏ quỹ mô phỏng càng chuẩn xác",
                        "Tỷ lệ lệnh giao dịch bị lỗi kỹ thuật của hệ thống giao dịch công ty chứng khoán",
                        "Khoản phí hoa hồng trả cho nhân viên môi giới chứng khoán",
                        "Số lượng mã cổ phiếu bị hủy niêm yết khỏi thị trường chứng khoán trong năm",
                        "A",
                        "Bước 1: Mục tiêu của quỹ ETF thụ động là bám sát chỉ số tham chiếu (như VN30, VN100, Diamond). Tracking Error phản ánh sai lệch do chi phí quản lý, phí giao dịch tái cơ cấu danh mục và tiền mặt nhàn rỗi. Tracking error càng thấp thì quỹ vận hành càng hiệu quả. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tính toán): Một nhà đầu tư thận trọng định giá Giá trị nội tại của cổ phiếu Cơ điện Lạnh là 60.000 đồng/cổ phiếu. Nhà đầu tư đặt ra nguyên tắc kỷ luật yêu cầu Biên an toàn tối thiểu là 25%. Mức thị giá mua tối đa có thể chấp nhận giải ngân là bao nhiêu?",
                        "45.000 đồng/cổ phiếu",
                        "50.000 đồng/cổ phiếu",
                        "48.000 đồng/cổ phiếu",
                        "55.000 đồng/cổ phiếu",
                        "A",
                        "Bước 1: Công thức Giá mua mục tiêu tối đa = Giá trị nội tại * (1 - Tỷ lệ Biên an toàn). Bước 2: Thay số: Giá mua tối đa = 60.000 * (1 - 0,25) = 60.000 * 0,75 = 45.000 đồng/cổ phiếu. Phân tích phương án nhiễu: B là mua ở giá 50.000đ (Biên an toàn chỉ đạt 16,7%). C là mua ở giá 48.000đ (Biên an toàn 20%). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tính toán): Cổ phiếu Vinamilk đang giao dịch trên sàn chứng khoán ở mức giá 68.000 đồng/cổ phiếu. Sau khi phân tích dòng tiền chiết khấu bảo thủ, bạn định giá Giá trị nội tại dài hạn của công ty là 85.000 đồng. Tỷ lệ Biên an toàn hiện tại của thương vụ đầu tư này là bao nhiêu %?",
                        "20,0%",
                        "25,0%",
                        "15,0%",
                        "12,5%",
                        "A",
                        "Bước 1: Công thức tính Biên an toàn = (Giá trị nội tại - Thị giá) / Giá trị nội tại. Bước 2: Thay số: (85.000 - 68.000) / 85.000 = 17.000 / 85.000 = 0,20 = 20,0%. Phân tích phương án nhiễu: B sai do chia nhầm cho thị giá: 17.000 / 68.000 = 25,0% (đây là tỷ lệ tăng giá kỳ vọng, không phải biên an toàn). C và D sai lệch số học. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tính toán): Giả sử Quỹ ETF chỉ số A (phí 0,6%/năm, sinh lời ròng 10,4%/năm) và Quỹ mở chủ động B (phí 2,2%/năm, sinh lời ròng 8,8%/năm) cùng được đầu tư 100 triệu đồng ban đầu trong 20 năm. Giá trị tài sản sau 20 năm của hai quỹ và số tiền chênh lệch thất thoát do phí quản lý xấp xỉ là bao nhiêu?",
                        "Quỹ A đạt ~720 triệu đồng; Quỹ B đạt ~535 triệu đồng; chênh lệch thất thoát do phí lên tới ~185 triệu đồng (gần gấp 2 lần vốn gốc ban đầu)",
                        "Quỹ A đạt 500 triệu đồng; Quỹ B đạt 490 triệu đồng; chênh lệch 10 triệu đồng",
                        "Hai quỹ nhận về số tiền hoàn toàn bằng nhau",
                        "Quỹ B nhận về số tiền lớn hơn Quỹ A nhờ có chuyên gia chọn cổ phiếu",
                        "A",
                        "Bước 1: Tính FV Quỹ A = 100M * (1 + 0,104)^20 ≈ 100M * 7,204 ≈ 720,4 triệu đồng. Bước 2: Tính FV Quỹ B = 100M * (1 + 0,088)^20 ≈ 100M * 5,354 ≈ 535,4 triệu đồng. Bước 3: Chênh lệch tài sản = 720,4 - 535,4 = 185 triệu đồng. Mức chênh lệch phí chỉ 1,6%/năm đã lấy đi 185 triệu đồng của nhà đầu tư. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tính toán): Chứng chỉ quỹ ETF VN Diamond có Giá trị tài sản ròng trên một đơn vị (NAV/ccq) tại ngày định giá là 30.000 đồng. Trên sàn HOSE, giá thị trường khớp lệnh của chứng chỉ quỹ này là 30.450 đồng. Trạng thái giao dịch của chứng chỉ quỹ và tỷ lệ chênh lệch giá là:",
                        "Giao dịch thặng dư (Premium) với mức thặng dư +1,50% so với NAV",
                        "Giao dịch chiết khấu (Discount) với mức chiết khấu -1,50% so với NAV",
                        "Giao dịch ngang giá (At Par)",
                        "Giao dịch thặng dư +4,50%",
                        "A",
                        "Bước 1: So sánh Thị giá (P) và NAV: P = 30.450 đ > NAV = 30.000 đ => Chứng chỉ quỹ đang giao dịch thặng dư (Premium). Bước 2: Tỷ lệ Premium = (Thị giá - NAV) / NAV = (30.450 - 30.000) / 30.000 = 450 / 30.000 = +0,015 = +1,50%. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tâm lý/Rủi ro): Tâm lý 'Nôn nóng làm giàu nhanh' (Get-Rich-Quick Syndrome) khiến người mới bước chân vào đầu tư thường mắc sai lầm nào đối với Quỹ chỉ số ETF?",
                        "Chê bai quỹ ETF tăng trưởng quá chậm và nhàm chán (chỉ 10%–15%/năm), bỏ rơi kỷ luật tích lũy để lao vào lướt sóng cổ phiếu đầu cơ penny hoặc coin rác, dẫn đến thua lỗ nặng",
                        "Nghĩ rằng Quỹ ETF chỉ dành riêng cho các ngân hàng trung ương",
                        "Lo sợ quỹ ETF không có thanh khoản khi cần rút tiền",
                        "Sợ hãi vì danh mục của quỹ ETF nắm giữ quá nhiều cổ phiếu lớn minh bạch",
                        "A",
                        "Bước 1: Charlie Munger từng nói: 'Đầu tư đúng đắn giống như xem sơn khô hoặc cỏ mọc'. Tâm lý nôn nóng muốn nhân tài khoản bằng lần chỉ trong vài tháng thúc đẩy hành vi cờ bạc, phá vỡ nguyên lý lãi kép tích lũy chậm mà chắc của quỹ chỉ số. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tâm lý/Rủi ro): Thiên kiến 'Ảo tưởng chính xác' (Illusion of Precision) trong định giá doanh nghiệp cảnh báo nhà đầu tư điều gì?",
                        "Việc xây dựng mô hình Excel phức tạp với hàng trăm biến số lẻ không đảm bảo định giá đúng 100%; tương lai luôn bất định nên nhà đầu tư bắt buộc phải có Biên an toàn dày để phòng vệ rủi ro",
                        "Tất cả các mô hình định giá chiết khấu dòng tiền đều bảo đảm tính toán chính xác tuyệt đối giá cổ phiếu trong tương lai",
                        "Nhà đầu tư không bao giờ cần đọc báo cáo tài chính khi mua cổ phiếu",
                        "Giá trị nội tại của một doanh nghiệp là con số cố định vĩnh viễn không bao giờ thay đổi",
                        "A",
                        "Bước 1: Bất kỳ mô hình định giá nào (DCF, DDM, P/E) đều dựa trên các giả định về tương lai (tốc độ tăng trưởng, tỷ suất chiết khấu). Một thay đổi nhỏ 1% trong giả định có thể làm thay đổi 30% kết quả định giá. Vì vậy, Biên an toàn chính là công cụ bù đắp cho sự không hoàn hảo của việc định giá. Do đó A là đáp án đúng."
                    )
                }
            ),

            // =========================================================================
            // 6. Quiz Bài 3.2: Quản Trị Rủi Ro Danh Mục & Tâm Lý Đầu Tư
            // =========================================================================
            BuildQuiz(
                "l32",
                SeedDataInitializer.InvestingLesson32Id,
                "Kiểm Tra Đánh Giá Năng Lực Bài 3.2: Quản Trị Rủi Ro Danh Mục & Tâm Lý Đầu Tư",
                now.AddSeconds(6),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Mức sụt giảm tối đa (Maximum Drawdown - MDD) đo lường điều gì trong quản trị rủi ro danh mục?",
                        "Tỷ lệ phần trăm sụt giảm lớn nhất của giá trị tài sản danh mục tính từ đỉnh cao nhất xuống đáy thấp nhất trong một chu kỳ đầu tư",
                        "Mức thuế thu nhập cá nhân tối đa mà nhà đầu tư phải nộp cho cơ quan thuế",
                        "Tỷ lệ tiền mặt tối đa mà nhà đầu tư được phép giữ trong tài khoản",
                        "Số lượng mã cổ phiếu tối đa được phép bán trong một phiên giao dịch",
                        "A",
                        "Bước 1: Max Drawdown (MDD) = (Giá trị đỉnh - Giá trị đáy) / Giá trị đỉnh. MDD là thước đo rủi ro thực tế sâu sắc nhất vì nó phản ánh nỗi đau tâm lý lớn nhất mà nhà đầu tư phải trải qua trong các đợt khủng hoảng thị trường. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Nguyên tắc Quản lý quy mô vị thế (Position Sizing) và 'Quy tắc rủi ro 1%–2%' quy định điều gì?",
                        "Trong bất kỳ một thương vụ giao dịch nào, số tiền thua lỗ tối đa cho phép (nếu chạm điểm cắt lỗ) không bao giờ được vượt quá 1%–2% tổng giá trị tài sản ròng của danh mục",
                        "Chỉ được phép phân bổ tối đa 1%–2% vốn để mua cổ phiếu, còn lại 98% phải gửi tiết kiệm",
                        "Chỉ được đầu tư vào những cổ phiếu có tỷ lệ cổ tức tiền mặt 1%–2%/năm",
                        "Mỗi năm chỉ được phép đặt lệnh giao dịch từ 1 đến 2 lần",
                        "A",
                        "Bước 1: Position Sizing bảo vệ nhà đầu tư khỏi nguy cơ cháy tài khoản. Nếu giới hạn rủi ro 1% danh mục cho mỗi lệnh, nhà đầu tư phải thua lỗ liên tiếp 100 lần mới mất hết vốn (xác suất gần như bằng 0). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Theo Lý thuyết Triển vọng (Prospect Theory), hiện tượng 'Ác cảm thua lỗ' (Loss Aversion) dẫn đến sai lầm kinh điển nào của nhà đầu tư?",
                        "Chốt lời quá vội vàng khi vừa có một khoản lãi nhỏ (vì sợ mất lãi), nhưng lại gồng lỗ buông xuôi và không chịu cắt lỗ khi vị thế bị âm nặng (vì không muốn chấp nhận nỗi đau mất tiền thực tế)",
                        "Cắt lỗ rất nhanh và kiên nhẫn gồng lãi cổ phiếu suốt 10 năm",
                        "Hoàn toàn không quan tâm đến biến động giá của thị trường chứng khoán",
                        "Luôn luôn mua cổ phiếu khi giá đang giảm sàn liên tục",
                        "A",
                        "Bước 1: Nghiên cứu của Daniel Kahneman và Amos Tversky chỉ ra rằng nỗi đau tâm lý khi mất 1 đồng tiền lớn gấp khoảng 2 đến 2,5 lần niềm vui khi kiếm được 1 đồng tiền. Do đó, người ta sẵn sàng đánh bạc với rủi ro bằng cách gồng lỗ với hy vọng mong manh hòa vốn. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Vai trò của Lệnh cắt lỗ kỷ luật (Stop-loss) và Nhật ký Quyết định (Decision Journal) trong quản trị danh mục là:",
                        "Stop-loss bảo vệ nguồn vốn sống còn trước khi tổn thất vượt ngoài tầm kiểm soát; Decision Journal ghi chép lại luận điểm ban đầu để rà soát khách quan, loại bỏ cảm xúc và nâng cao năng lực ra quyết định",
                        "Stop-loss là công cụ bắt buộc để Sở giao dịch phạt tiền tài khoản nhà đầu tư",
                        "Decision Journal chỉ dùng để khoe lãi trên các diễn đàn mạng xã hội",
                        "Cắt lỗ chứng tỏ nhà đầu tư thất bại và nên rời bỏ thị trường vĩnh viễn",
                        "A",
                        "Bước 1: Quản trị rủi ro là yếu tố số 1 giúp nhà đầu tư sống sót trên thị trường. Stop-loss là dây an toàn; Decision Journal là hộp đen máy bay giúp học hỏi từ sai lầm và duy trì tính khách quan. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tính toán): Danh mục đầu tư của một cá nhân giảm từ mức đỉnh 500.000.000 đồng xuống mức đáy 325.000.000 đồng trong đợt điều chỉnh của thị trường. Mức sụt giảm tối đa (Max Drawdown) của danh mục là bao nhiêu?",
                        "35,0%",
                        "53,8%",
                        "30,0%",
                        "25,0%",
                        "A",
                        "Bước 1: Công thức Max Drawdown = (Đỉnh - Đáy) / Đỉnh. Bước 2: Thay số: MDD = (500M - 325M) / 500M = 175M / 500M = 0,35 = 35,0%. Phân tích phương án nhiễu: B sai do lấy khoản giảm chia cho đáy (175M / 325M ≈ 53,8%). C và D sai lệch số học. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tính toán): Tính bất đối xứng của tỷ lệ hòa vốn sau thua lỗ: Nếu danh mục của bạn bị sụt giảm 50,0% giá trị (từ 100 triệu đồng xuống còn 50 triệu đồng), mức tăng trưởng phần trăm cần thiết từ đáy để danh mục quay về mức hòa vốn 100 triệu đồng ban đầu là bao nhiêu?",
                        "Cần tăng trưởng 100,0% từ đáy",
                        "Chỉ cần tăng trưởng 50,0% từ đáy",
                        "Cần tăng trưởng 75,0% từ đáy",
                        "Cần tăng trưởng 200,0% từ đáy",
                        "A",
                        "Bước 1: Công thức tỷ lệ tăng cần thiết để hòa vốn = L / (1 - L), trong đó L là tỷ lệ lỗ. Bước 2: Với mức lỗ L = 50% = 0,5: Tỷ lệ tăng hòa vốn = 0,5 / (1 - 0,5) = 0,5 / 0,5 = 1,0 = 100,0%. Từ 50 triệu muốn về 100 triệu bạn phải kiếm được thêm 50 triệu, tức tài sản phải tăng gấp đôi (+100%). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tính toán): Tương tự, nếu danh mục của bạn bị sụt giảm 20,0% (từ 100M còn 80M), mức tăng trưởng cần thiết để hòa vốn là bao nhiêu? Và nếu sụt giảm 33,33% (từ 100M còn 66,67M), mức tăng cần thiết là bao nhiêu?",
                        "Sụt giảm 20% cần tăng 25,0% để hòa vốn; sụt giảm 33,33% cần tăng 50,0% để hòa vốn",
                        "Sụt giảm 20% chỉ cần tăng 20% để hòa vốn; sụt giảm 33,33% chỉ cần tăng 33,33% để hòa vốn",
                        "Sụt giảm 20% cần tăng 40% để hòa vốn; sụt giảm 33,33% cần tăng 100% để hòa vốn",
                        "Sụt giảm 20% cần tăng 15% để hòa vốn; sụt giảm 33,33% cần tăng 25% để hòa vốn",
                        "A",
                        "Bước 1: Với mức lỗ 20%: Cần tăng = 0,20 / (1 - 0,20) = 0,20 / 0,80 = 25,0% (80M * 1,25 = 100M). Bước 2: Với mức lỗ 33,33%: Cần tăng = (1/3) / (2/3) = 1/2 = 50,0% (66,67M * 1,5 = 100M). Mức lỗ càng sâu thì mức tăng cần thiết để hòa vốn càng tăng theo hàm phi tuyến tính. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tính toán): Tổng quy mô danh mục của bạn là 400.000.000 đồng. Bạn tuân thủ kỷ luật rủi ro tối đa 1,5% tổng danh mục cho một thương vụ (khoản lỗ tối đa cho phép là 6.000.000 đồng). Bạn dự định mua cổ phiếu Hòa Phát tại giá 30.000 đồng/cp, điểm cắt lỗ xác định tại 27.000 đồng/cp (mức lỗ 3.000 đ/cp = 10%). Số lượng cổ phiếu tối đa được phép mua và giá trị giải ngân là:",
                        "Mua tối đa 2.000 cổ phiếu; Giá trị giải ngân = 60.000.000 đồng (chiếm 15% tổng danh mục)",
                        "Mua tối đa 4.000 cổ phiếu; Giá trị giải ngân = 120.000.000 đồng",
                        "Mua tối đa 1.000 cổ phiếu; Giá trị giải ngân = 30.000.000 đồng",
                        "Giải ngân toàn bộ 400.000.000 đồng để mua cổ phiếu Hòa Phát",
                        "A",
                        "Bước 1: Khoản lỗ tối đa cho phép = 400.000.000 * 1,5% = 6.000.000 đồng. Bước 2: Mức lỗ trên mỗi cổ phiếu nếu chạm cắt lỗ = 30.000 - 27.000 = 3.000 đồng/cp. Bước 3: Số lượng cổ phiếu tối đa = 6.000.000 / 3.000 = 2.000 cổ phiếu. Bước 4: Giá trị giải ngân = 2.000 * 30.000 = 60.000.000 đồng (tương đương 15% danh mục). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tâm lý/Rủi ro): Cạm bẫy tâm lý 'Bình quân giá xuống mù quáng' (Averaging Down Trap) khác với chiến lược đầu tư DCA có kế hoạch ở điểm mấu chốt nào?",
                        "DCA là giải ngân định kỳ theo kế hoạch vào chỉ số toàn thị trường hoặc doanh nghiệp có nền tảng vững chắc; còn bình quân giá xuống mù quáng là tiếp tục bơm thêm tiền gỡ gạc vào một cổ phiếu đầu cơ đang trên đà phá sản/suy thoái do mắc bẫy chi phí chìm",
                        "Hai hành vi này hoàn toàn giống hệt nhau về mọi mặt",
                        "DCA là chỉ được mua khi cổ phiếu liên tục lập đỉnh mới",
                        "Bình quân giá xuống luôn luôn đảm bảo 100% tài khoản sẽ về bờ trong vòng 1 tuần",
                        "A",
                        "Bước 1: Phân biệt rõ: Mua DCA vào ETF chỉ số (như VN30, S&P 500) dựa trên giả định nền kinh tế sẽ luôn tăng trưởng trong dài hạn. Còn mua trung bình giá xuống một cổ phiếu riêng lẻ đang sụp đổ về nội tại (bị điều tra gian lận, mất thị trường) sẽ đẩy nhà đầu tư vào nguy cơ mất trắng toàn bộ vốn tích lũy. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tâm lý/Rủi ro): Cơ chế 'Call Margin' (Yêu cầu bổ sung ký quỹ) và 'Force Sell' (Giải chấp bắt buộc) diễn ra như thế nào khi nhà đầu tư lạm dụng đòn bẩy vay nợ quá mức?",
                        "Khi giá chứng khoán giảm làm tỷ lệ tài sản thực tế trên nợ vay rớt xuống dưới ngưỡng an toàn, công ty chứng khoán yêu cầu nộp thêm tiền ký quỹ; nếu không nộp đúng hạn, CTCK sẽ tự động bán cưỡng bức cổ phiếu trong tài khoản để thu hồi nợ vay",
                        "Công ty chứng khoán sẽ tự động chuyển khoản tiền lãi cho nhà đầu tư để hỗ trợ vượt qua giai đoạn khó khăn",
                        "Khoản nợ margin sẽ tự động được xóa bỏ nếu thị trường chung giảm quá 30%",
                        "Call Margin chỉ xảy ra khi các mã cổ phiếu trong tài khoản đồng loạt tăng trần",
                        "A",
                        "Bước 1: Cơ chế đòn bẩy margin: CTCK cho vay tiền với tài sản bảo đảm chính là số cổ phiếu trong tài khoản. Khi giá cổ phiếu giảm, giá trị tài sản bảo đảm giảm, tỷ lệ ký quỹ rơi xuống dưới ngưỡng duy trì (thường là 30%-35%). CTCK phát lệnh Call Margin; nếu tài khoản không được bổ sung tiền, lệnh Force Sell sẽ được kích hoạt bán giải chấp bất chấp mức giá ở đáy thị trường. Do đó A là đáp án đúng."
                    )
                }
            ),

            // =========================================================================
            // 7. Đề Kiểm Tra Tổng Kết: Khóa Học Đầu Tư & Quản Trị Danh Mục Thực Chiến
            // =========================================================================
            BuildQuiz(
                "final",
                SeedDataInitializer.InvestingLesson32Id,
                "Đề Kiểm Tra Tổng Kết: Khóa Học Đầu Tư & Quản Trị Danh Mục Thực Chiến",
                now.AddSeconds(7),
                now,
                new[]
                {
                    new QuizQuestionSpec(
                        "Câu 1 (Khái niệm): Xuyên suốt toàn bộ chương trình đào tạo, nền tảng số 1 quyết định sự thịnh vượng tài chính bền vững của nhà đầu tư cá nhân là:",
                        "Kỷ luật tích lũy định kỳ (DCA), tận dụng tối đa sức mạnh thời gian của lãi kép, đa dạng hóa phân bổ tài sản, thiết lập biên an toàn dày và làm chủ cảm xúc thiên kiến hành vi",
                        "Tìm kiếm các hội nhóm kín (room phím hàng) để lướt sóng bắt đáy cổ phiếu penny theo tin đồn",
                        "Vay nợ đòn bẩy margin tối đa mỗi khi thấy thị trường xuất hiện nhịp tăng điểm ngắn hạn",
                        "Tất tay toàn bộ tài sản vào một mã cổ phiếu duy nhất để nhân 5 lần tài khoản nhanh chóng",
                        "A",
                        "Bước 1: Tổng kết các trụ cột khóa học: Thành công trong đầu tư không đến từ sự phỏng đoán may rủi hay lướt sóng ngắn hạn, mà đến từ hệ thống nguyên tắc toán học vững chắc: Lãi kép (Bài 1.1), Kế hoạch FIRE & Dòng tiền (Bài 1.2), Phân bổ đa tài sản (Bài 2.1), Định giá & Đọc BCTC (Bài 2.2), Quỹ ETF & Biên an toàn (Bài 3.1) và Quản trị rủi ro & Cảm xúc (Bài 3.2). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Khái niệm): Mô hình FIRE và Quy tắc 4% hướng tới mục tiêu tài chính nào cho người học?",
                        "Tích lũy khối tài sản đầu tư bằng 25 lần chi phí năm, tạo ra dòng tiền rút an toàn bảo toàn vốn gốc dài hạn để giải phóng bản thân khỏi áp lực sinh kế bắt buộc",
                        "Chi tiêu toàn bộ thu nhập kiếm được mỗi tháng để tối đa hóa hưởng thụ ngắn hạn",
                        "Phụ thuộc hoàn toàn vào nguồn tiền trợ cấp từ gia đình và người thân",
                        "Đầu tư mạo hiểm vào các loại tiền ảo không có giá trị nội tại để mong đổi đời sau 1 đêm",
                        "A",
                        "Bước 1: FIRE không phải là việc nằm lười không làm việc, mà là tự do lựa chọn công việc mình yêu thích mà không bị ràng buộc bởi áp lực cơm áo gạo tiền nhờ dòng tiền rút an toàn từ tài sản tích lũy. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Khái niệm): Đa dạng hóa danh mục theo Harry Markowitz phát huy hiệu quả bảo vệ tài sản tối ưu nhất khi:",
                        "Kết hợp các lớp tài sản có hệ số tương quan thấp hoặc tương quan âm (Cổ phiếu, Trái phiếu, Tiền mặt dự phòng, Vàng) để triệt tiêu rủi ro phi hệ thống mà không làm suy giảm tỷ suất sinh lời kỳ vọng",
                        "Mua cùng lúc 10 mã cổ phiếu đều thuộc một nhóm ngành bất động sản đầu cơ",
                        "Chia nhỏ tiền mua 5 loại xổ số khác nhau trong cùng một ngày",
                        "Đầu tư 100% tiền vào các khoản cho vay ngang hàng P2P không tài sản bảo đảm",
                        "A",
                        "Bước 1: Markowitz đã chứng minh rằng nếu đa dạng hóa đúng cách (chọn các tài sản có tương quan nghịch hoặc thấp), độ biến động rủi ro của toàn bộ danh mục sẽ thấp hơn độ biến động của từng tài sản thành phần. Mua nhiều mã cùng ngành (B) không phải là đa dạng hóa. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Khái niệm): Định nghĩa chuẩn mực của Warren Buffett và Benjamin Graham về 'Biên an toàn' (Margin of Safety):",
                        "Luôn kiên nhẫn mua tài sản ở mức thị giá chiết khấu sâu so với giá trị nội tại ước tính để tạo vùng đệm bảo vệ vốn trước những sai số dự báo và rủi ro kinh tế bất khả kháng",
                        "Chỉ mua những cổ phiếu có thị giá rẻ nhất trên bảng điện tử (dưới 5.000 đồng/cp)",
                        "Cam kết của công ty chứng khoán về việc không để giá cổ phiếu giảm",
                        "Để 100% tài sản ở tài khoản thanh toán không kỳ hạn để an toàn tuyệt đối",
                        "A",
                        "Bước 1: Warren Buffett từng tóm tắt bí quyết đầu tư thành hai quy tắc: Quy tắc số 1: Không bao giờ để mất tiền; Quy tắc số 2: Không bao giờ quên quy tắc số 1. Biên an toàn chính là công cụ thực thi hai quy tắc này. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 5 (Tính toán): Anh Hùng 25 tuổi, thực hiện kỷ luật DCA 4.000.000 đồng/tháng liên tục trong 20 năm (240 tháng) vào rổ ETF chỉ số với mức sinh lời kỳ vọng 11,0%/năm. Tổng số vốn gốc anh Hùng đã nộp là bao nhiêu và giá trị tương lai (FV) ước tính nhận được sau 20 năm?",
                        "Vốn gốc = 960.000.000 đồng; Giá trị tương lai (FV) ước tính đạt ~3,23 tỷ đồng (Tiền lãi kép sinh ra hơn 2,27 tỷ đồng, gấp hơn 2,3 lần tổng vốn gốc)",
                        "Vốn gốc = 960.000.000 đồng; Giá trị tương lai chỉ đạt đúng 1,1 tỷ đồng",
                        "Vốn gốc = 480.000.000 đồng; Giá trị tương lai đạt 5,0 tỷ đồng",
                        "Vốn gốc = 1,2 tỷ đồng; Giá trị tương lai đạt 2,0 tỷ đồng",
                        "A",
                        "Bước 1: Tổng vốn gốc đã nộp = 4.000.000 * 240 tháng = 960.000.000 đồng. Bước 2: Lãi suất tháng r_m = 11% / 12 ≈ 0,9167%/tháng. Bước 3: Tính giá trị tương lai tích lũy đều: FV = 4.000.000 * [((1 + 0,009167)^240 - 1) / 0,009167] ≈ 3.232.000.000 đồng (khoảng 3,23 tỷ đồng). Số tiền lãi kép sinh ra = 3,23 tỷ - 960 triệu ≈ 2,27 tỷ đồng. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tính toán): Một doanh nghiệp sản xuất đầu ngành có EPS hiện tại là 5.000 đồng/cổ phiếu và duy trì tốc độ tăng trưởng bền vững 10%/năm. P/E trung bình hợp lý của ngành là 14,0 lần. Định giá giá trị nội tại hợp lý của cổ phiếu là bao nhiêu và với Biên an toàn yêu cầu 30%, mức giá mua mục tiêu tối đa là bao nhiêu?",
                        "Giá trị nội tại hợp lý = 70.000 đồng/cp; Mức giá mua mục tiêu tối đa = 49.000 đồng/cp",
                        "Giá trị nội tại hợp lý = 50.000 đồng/cp; Mức giá mua mục tiêu tối đa = 35.000 đồng/cp",
                        "Giá trị nội tại hợp lý = 80.000 đồng/cp; Mức giá mua mục tiêu tối đa = 60.000 đồng/cp",
                        "Giá trị nội tại hợp lý = 70.000 đồng/cp; Mức giá mua mục tiêu tối đa = 65.000 đồng/cp",
                        "A",
                        "Bước 1: Tính Giá trị nội tại hợp lý = EPS * P/E hợp lý = 5.000 * 14 = 70.000 đồng/cổ phiếu. Bước 2: Với Biên an toàn yêu cầu 30%, mức giá mua tối đa = 70.000 * (1 - 0,30) = 70.000 * 0,70 = 49.000 đồng/cổ phiếu. Phân tích phương án nhiễu: D mua ở 65.000đ (biên an toàn chỉ có 7,1%, quá mỏng). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tính toán): Khủng hoảng tài chính khiến danh mục 1,0 tỷ đồng của nhà đầu tư bị sụt giảm 40,0% xuống còn 600.000.000 đồng. Để giá trị danh mục phục hồi trở lại mốc 1,0 tỷ đồng ban đầu, tỷ lệ sinh lời cần thiết tính từ đáy 600 triệu là bao nhiêu %?",
                        "Cần tăng trưởng +66,67% từ đáy (Khoản cần tăng = 400 triệu; Tỷ lệ tăng = 400 / 600 = 66,67%)",
                        "Chỉ cần tăng đúng +40,0% từ đáy là hòa vốn",
                        "Cần tăng trưởng +100,0% từ đáy",
                        "Cần tăng trưởng +50,0% từ đáy",
                        "A",
                        "Bước 1: Khoản tiền cần tăng để hòa vốn = 1.000.000.000 - 600.000.000 = 400.000.000 đồng. Bước 2: Tỷ lệ phần trăm tăng trưởng cần thiết = Khoản cần tăng / Giá trị đáy = 400M / 600M = 2/3 ≈ +66,67%. Phân tích phương án nhiễu: B là ngụy biện đối xứng số học (nhầm tưởng giảm 40% thì tăng 40% là hòa vốn; thực tế 600M * 1,4 = 840M, vẫn lỗ 160M). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tính toán): Nhà đầu tư có danh mục 500 triệu đồng với tỷ trọng mục tiêu 60% Cổ phiếu (300 triệu) và 40% Trái phiếu/Tiền gửi (200 triệu). Sau một đợt tăng mạnh, phần Cổ phiếu tăng lên 420 triệu, phần Trái phiếu tăng lên 210 triệu (Tổng danh mục = 630 triệu đồng). Để tái cân bằng danh mục về đúng tỷ trọng 60/40, nhà đầu tư cần thực hiện hành động nào?",
                        "Bán bớt 42.000.000 đồng Cổ phiếu chuyển sang mua Trái phiếu (Cổ phiếu mục tiêu = 630M * 60% = 378 triệu; Cổ phiếu hiện có 420M -> Bán 42M; Trái phiếu tăng từ 210M lên 252M = 40%)",
                        "Mua thêm 50.000.000 đồng Cổ phiếu vì cổ phiếu đang tăng mạnh",
                        "Bán toàn bộ 210 triệu Trái phiếu để dồn hết vào Cổ phiếu",
                        "Giữ nguyên không tái cân bằng",
                        "A",
                        "Bước 1: Tính tổng danh mục mới = 420 + 210 = 630 triệu đồng. Bước 2: Giá trị Cổ phiếu mục tiêu = 630 * 60% = 378 triệu đồng. Bước 3: Số tiền Cổ phiếu cần bán = 420 - 378 = 42 triệu đồng. Bước 4: Chuyển 42 triệu sang mua Trái phiếu => Trái phiếu mới = 210 + 42 = 252 triệu đồng (đúng bằng 630 * 40%). Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 9 (Tâm lý/Rủi ro): Biện pháp quản trị tâm lý hiệu quả nhất giúp nhà đầu tư vượt qua hai thái cực 'Hưng phấn tột độ (Greed/FOMO)' ở vùng đỉnh và 'Hoảng loạn tột độ (Fear/Panic Selling)' ở vùng đáy là gì?",
                        "Xây dựng Bản Tuyên ngôn Chính sách Đầu tư (IPS) bằng văn bản với các tiêu chí định lượng rõ ràng, tuân thủ kỷ luật phân bổ tài sản định kỳ và hạn chế dán mắt vào bảng điện tử biến động từng phút",
                        "Tham gia thật nhiều diễn đàn và nhóm chat chứng khoán để hành động theo số đông",
                        "Bán tháo toàn bộ cổ phiếu ngay khi thấy thị trường rung lắc giảm 1%–2%",
                        "Vay thêm nợ tín dụng để gỡ gạc mỗi khi tài khoản bị lỗ",
                        "A",
                        "Bước 1: Tâm lý học đầu tư: Bảng điện tử nhảy múa kích hoạt hạch hạnh nhân (Amygdala) thúc đẩy các quyết định bản năng sinh tồn sai lầm. Bản IPS đóng vai trò là chiếc mỏ neo lý trí, biến mọi hành vi mua bán thành quy trình có kế hoạch từ trước. Do đó A là đáp án đúng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tâm lý/Rủi ro): Bản tuyên ngôn '5 KHÔNG' sống còn trong quản trị tài sản cá nhân thực chiến gồm những nguyên tắc nào?",
                        "Không vay nợ margin quá mức; Không tất tay (all-in) một mã; Không đầu tư vào thứ mình không hiểu; Không gồng lỗ đầu cơ mù quáng; Không chạy theo tâm lý đám đông FOMO",
                        "Không gửi tiết kiệm; Không mua bảo hiểm; Không đầu tư cổ phiếu; Không đọc sách tài chính; Không lập ngân sách",
                        "Không chốt lời; Không cắt lỗ; Không tái cân bằng; Không giữ tiền mặt; Không nghe chuyên gia",
                        "Không giao dịch trực tuyến; Không nộp thuế; Không dùng tài khoản ngân hàng; Không lập di chúc; Không đa dạng hóa",
                        "A",
                        "Bước 1: 5 Không là kim chỉ nam bảo vệ nguồn vốn xuyên suốt sự nghiệp đầu tư. Giữ vững kỷ luật 5 Không đảm bảo nhà đầu tư không bao giờ rơi vào tình cảnh phá sản tài chính vĩnh viễn, từ đó hưởng trọn vẹn thành quả của lãi kép dài hạn. Do đó A là đáp án đúng."
                    )
                }
            )
        };
    }

}
