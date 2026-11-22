namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
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

        return list;
    }
}
