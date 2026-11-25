namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;

/// <summary>
/// Idempotent Write-in (Active Recall) practice content seeder for
/// Coding (JS), Investing &amp; Finance, and Applied Psychology courses.
/// Uses deterministic GUIDs (CreateDeterministicActivityId) + composite key
/// (LessonId, Type, OrderIndex) so re-running never duplicates or overwrites legacy rows.
/// </summary>
public static class WriteInPracticeContent
{
    public static List<PracticeActivity> GetStarterWriteInActivities(DateTimeOffset now)
    {
        var list = new List<PracticeActivity>
        {
            // =========================================================================
            // 1. CODING — Lesson 1.1 (Map, Filter, Reduce)
            // Legacy activities occupy OrderIndex 1 (js_sandbox) & 2 (parsons).
            // =========================================================================
            new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(
                    SeedDataInitializer.CodingCourseId,
                    SeedDataInitializer.CodingLesson11Id,
                    "predict_output",
                    3),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson11Id,
                OrderIndex = 3,
                Type = "predict_output",
                Title = "Đoán Output JS: Pipeline Map / Filter / Reduce & Tính Bất Biến",
                Description = "Tự chạy nhẩm trong đầu trước rồi kiểm chứng trực tiếp bằng Web Worker sandbox.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "code-l11-pred-1",
                            difficulty = "Dễ",
                            title = "Lọc giao dịch thu nhập & cộng tổng bằng reduce",
                            description = "Dự đoán chính xác 2 dòng console.log sẽ in ra.",
                            code = "const txs = [\n  { type: 'in', amount: 120 },\n  { type: 'out', amount: 50 },\n  { type: 'in', amount: 80 }\n];\nconst totalIn = txs.filter(t => t.type === 'in').reduce((s, t) => s + t.amount, 0);\nconsole.log(totalIn);\nconsole.log(txs.length);",
                            expectedOutput = "200\n3",
                            hint = "filter giữ 2 giao dịch 'in' (120 + 80 = 200) và không làm thay đổi mảng txs ban đầu.",
                            explanation = "filter() và reduce() đều tạo kết quả mới mà không đột biến mảng gốc txs, nên tổng là 200 và độ dài mảng vẫn là 3."
                        },
                        new
                        {
                            id = "code-l11-pred-2",
                            difficulty = "Trung bình",
                            title = "Thứ tự biến đổi map → filter trên mảng số",
                            description = "Dự đoán output của mảng kết quả sau chuỗi map và filter.",
                            code = "const arr = [1, 2, 3, 4];\nconst res = arr.map(x => x * 2).filter(x => x > 4);\nconsole.log(JSON.stringify(res));",
                            expectedOutput = "[6,8]",
                            acceptedOutputs = new[] { "[6, 8]", "[6,8]" },
                            hint = "map nhân đôi thành [2, 4, 6, 8], sau đó filter giữ phần tử > 4.",
                            explanation = "Bước map tạo [2, 4, 6, 8]. Điều kiện x > 4 loại 2 và 4, chỉ còn [6,8]."
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
                Id = SeedDataInitializer.CreateDeterministicActivityId(
                    SeedDataInitializer.CodingCourseId,
                    SeedDataInitializer.CodingLesson11Id,
                    "bug_fix",
                    4),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson11Id,
                OrderIndex = 4,
                Type = "bug_fix",
                Title = "Sửa Bug Write-in: Lỗi reduce thiếu giá trị khởi tạo & đột biến mảng",
                Description = "Trực tiếp sửa đoạn mã lỗi JS và kiểm chứng bằng test case thật trên Web Worker.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "code-l11-bugfix-1",
                            title = "Sửa lỗi reduce cộng doanh thu đơn hàng hoàn tất",
                            language = "javascript",
                            categoryTag = "JS Array Reduce",
                            context = "Hàm sumPaidOrders(orders) trả về sai kết quả hoặc văng lỗi khi mảng rỗng vì thiếu giá trị khởi tạo trong reduce.",
                            buggyCode = "function sumPaidOrders(orders) {\n  return orders\n    .filter(o => o.status === 'paid')\n    .reduce((acc, o) => acc + o.amount);\n}",
                            bugLineNumber = 4,
                            bugBriefDescription = "Thiếu giá trị khởi tạo 0 ở tham số thứ 2 của reduce.",
                            cleanFixedCode = "function sumPaidOrders(orders) {\n  return orders\n    .filter(o => o.status === 'paid')\n    .reduce((acc, o) => acc + o.amount, 0);\n}",
                            requiredTokens = new[] { "reduce", ", 0" },
                            functionName = "sumPaidOrders",
                            testCases = new object[]
                            {
                                new { name = "Có đơn paid & pending", input = new object[] { new object[] { new { amount = 150, status = "paid" }, new { amount = 90, status = "pending" }, new { amount = 250, status = "paid" } } }, expected = 400 },
                                new { name = "Không có đơn nào paid", input = new object[] { new object[] { new { amount = 90, status = "pending" } } }, expected = 0 },
                                new { name = "Mảng rỗng", input = new object[] { Array.Empty<object>() }, expected = 0 }
                            },
                            explanation = "Truyền giá trị khởi tạo 0 vào reduce((acc, o) => acc + o.amount, 0) giúp acc luôn là kiểu số và không văng TypeError khi mảng rỗng."
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
                Id = SeedDataInitializer.CreateDeterministicActivityId(
                    SeedDataInitializer.CodingCourseId,
                    SeedDataInitializer.CodingLesson11Id,
                    "fill_code",
                    5),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson11Id,
                OrderIndex = 5,
                Type = "fill_code",
                Title = "Điền Code JS: Chuỗi Filter + Reduce Tính Tổng Thu Nhập Ròng",
                Description = "Tự gõ phương thức mảng phù hợp để hoàn thành pipeline xử lý giao dịch bất biến.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "code-l11-fill-1",
                            title = "JS Pipeline: Lọc giao dịch hợp lệ & tính tổng ròng",
                            category = "react",
                            language = "javascript",
                            description = "Hoàn thiện chuỗi filter và reduce trên mảng transactions.",
                            practicalScenario = "Màn hình thống kê ví cần lọc các giao dịch 'income' và cộng dồn amount mà không thay đổi mảng gốc.",
                            codeTemplate = new[]
                            {
                                "function totalIncome(transactions) {",
                                "  return transactions",
                                "    .{{BLANK_0}}(t => t.type === 'income')",
                                "    .{{BLANK_1}}((sum, t) => sum + t.amount, {{BLANK_2}});",
                                "}"
                            },
                            blanks = new object[]
                            {
                                new { id = "b0", blankIndex = 0, expected = "filter", acceptableAnswers = new[] { "filter" }, hint = "Phương thức lọc phần tử thỏa mãn điều kiện", chips = new[] { "filter", "map", "find", "some" }, explanation = "filter() trả về mảng mới chỉ gồm các phần tử khớp điều kiện." },
                                new { id = "b1", blankIndex = 1, expected = "reduce", acceptableAnswers = new[] { "reduce" }, hint = "Phương thức gộp mảng thành 1 giá trị tổng", chips = new[] { "reduce", "forEach", "slice", "concat" }, explanation = "reduce() tích lũy từng phần tử thành tổng duy nhất." },
                                new { id = "b2", blankIndex = 2, expected = "0", acceptableAnswers = new[] { "0" }, hint = "Giá trị khởi tạo an toàn cho tổng số học", chips = new[] { "0", "1", "null", "[]" }, explanation = "Khởi tạo bằng 0 đảm bảo hàm trả về 0 ngay cả khi mảng rỗng." }
                            },
                            lineExplanations = new object[]
                            {
                                new { lineNumber = 3, codeSnippet = ".filter(t => t.type === 'income')", purpose = "Lọc riêng các giao dịch thu nhập.", riskIfWrong = "Nếu dùng find() chỉ lấy 1 phần tử đầu tiên thay vì toàn bộ." },
                                new { lineNumber = 4, codeSnippet = ".reduce((sum, t) => sum + t.amount, 0)", purpose = "Cộng dồn số tiền bắt đầu từ 0.", riskIfWrong = "Nếu thiếu 0, mảng rỗng sẽ gây TypeError runtime." }
                            }
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // =========================================================================
            // 2. INVESTING — Lesson 1.1 (Lãi Kép & Chiến Lược DCA)
            // Legacy activities occupy OrderIndex 1 (compound_calculator) & 2 (behavioral_quiz).
            // =========================================================================
            new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(
                    SeedDataInitializer.InvestingCourseId,
                    SeedDataInitializer.InvestingLesson11Id,
                    "hand_calc",
                    3),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson11Id,
                OrderIndex = 3,
                Type = "hand_calc",
                Title = "Tự Tính Tay: Lãi Kép, Tích Lũy DCA, P/E, Biên An Toàn & Drawdown",
                Description = "Luyện bấm máy tính tay với số liệu ngẫu nhiên mới mỗi lần thử và đối chiếu từng bước công thức.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    problemTypes = new[] { "compound", "dca", "pe", "margin_of_safety", "drawdown" },
                    tolerancePercent = 1.5
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(
                    SeedDataInitializer.InvestingCourseId,
                    SeedDataInitializer.InvestingLesson11Id,
                    "investment_case",
                    4),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson11Id,
                OrderIndex = 4,
                Type = "investment_case",
                Title = "Tình Huống & Giải Thích: Kỷ Luật DCA Khi Thị Trường Giảm 25%",
                Description = "Chọn hướng xử lý và tự viết lập luận giải thích bằng lời để được chấm điểm chi tiết.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "inv-l11-case-1",
                            title = "Thị trường giảm sâu 25% giữa lộ trình DCA 15 năm",
                            scenario = "Bạn đang thực hiện DCA 5 triệu đ/tháng vào quỹ ETF chỉ số trong 15 năm. Sau năm đầu tiên, thị trường giảm 25% khiến danh mục tạm lỗ 15%. Bạn nên làm gì?",
                            options = new[]
                            {
                                "Bán toàn bộ cắt lỗ và rời khỏi thị trường",
                                "Duy trì kỷ luật DCA đều đặn mỗi tháng để mua thêm nhiều chứng chỉ quỹ ở vùng giá rẻ",
                                "Vay nợ thẻ tín dụng lãi cao để tất tay bắt đáy",
                                "Ngừng góp tiền chờ thị trường lập đỉnh mới rồi mua lại"
                            },
                            correctIndex = 1,
                            reasonPromptVi = "Giải thích bằng lời vì sao lựa chọn của bạn tối ưu về giá vốn trung bình, lãi kép và kỷ luật cảm xúc:",
                            expectedKeywords = new[] { "dca", "giá vốn", "kỷ luật", "dài hạn", "lãi kép" },
                            sampleExplanation = "Với mục tiêu dài hạn 15 năm, duy trì kỷ luật DCA khi thị trường giảm 25% giúp mua được nhiều chứng chỉ quỹ hơn với cùng số tiền, hạ giá vốn trung bình và khuếch đại sức mạnh lãi kép khi thị trường hồi phục.",
                            minWords = 12
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
                Id = SeedDataInitializer.CreateDeterministicActivityId(
                    SeedDataInitializer.InvestingCourseId,
                    SeedDataInitializer.InvestingLesson11Id,
                    "decision_journal",
                    5),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson11Id,
                OrderIndex = 5,
                Type = "decision_journal",
                Title = "Nhật Ký Quyết Định Đầu Tư: Kế Hoạch Tích Lũy Lãi Kép & DCA",
                Description = "Viết 4 trụ cột quyết định đầu tư (Luận điểm, Định giá & Biên an toàn, Kịch bản sai & Rủi ro, Kỷ luật cảm xúc).",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "inv-l11-dj-1",
                            title = "Nhật Ký Quyết Định: Lộ Trình DCA Quỹ Chỉ Số Dài Hạn",
                            scenario = "Hãy viết đầy đủ 4 trụ cột thẩm định cho kế hoạch tích lũy đều hàng tháng (DCA) của riêng bạn trong 5–15 năm tới.",
                            expectedKeywords = new[] { "dca", "lãi kép", "biên an toàn", "rủi ro", "kỷ luật", "lạm phát" },
                            sampleAnswer = "Luận điểm: Tích lũy quỹ chỉ số đa dạng hóa để vượt lạm phát và tận dụng lãi kép dài hạn. Định giá: Chỉ giải ngân bằng tiền nhàn rỗi, duy trì biên an toàn qua phân bổ tài sản. Kịch bản sai: Nếu thu nhập giảm hoặc thị trường sụt 35%, quỹ dự phòng 6 tháng giúp không phải bán tháo. Kỷ luật: Thiết lập chuyển khoản DCA tự động đầu tháng, không FOMO theo tin đồn."
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // =========================================================================
            // 3. PSYCHOLOGY — Lesson 1.1 (Hệ Thống 1 & 2, Hiệu Ứng Stroop, NVC)
            // Legacy activities occupy OrderIndex 1 (stroop_test) & 2 (psych_scenario).
            // =========================================================================
            new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(
                    SeedDataInitializer.PsychologyCourseId,
                    SeedDataInitializer.PsychologyLesson11Id,
                    "bias_spotter",
                    3),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson11Id,
                OrderIndex = 3,
                Type = "bias_spotter",
                Title = "Nhận diện Thiên kiến (Tự gõ): Hệ Thống 1, Mỏ Neo & Xác Nhận",
                Description = "Đọc tình huống đời thực, tự gõ tên thiên kiến/khái niệm tâm lý và viết lý do ngắn.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "psy-l11-bias-1",
                            title = "Phản ứng vội vàng theo ấn tượng đầu tiên",
                            contextTag = "Hệ thống 1 & Hiệu ứng Mỏ neo",
                            scenario = "Trong buổi phỏng vấn, ứng viên A đi trễ 2 phút do tắc đường nên người phỏng vấn lập tức kết luận ứng viên thiếu kỷ luật và đánh giá thấp toàn bộ câu trả lời chuyên môn xuất sắc sau đó.",
                            canonicalNameVi = "Hiệu ứng hào quang / Mỏ neo ấn tượng đầu (Halo / Anchoring Effect)",
                            acceptedNames = new[] { "Hiệu ứng hào quang", "Hiệu ứng sừng", "Hiệu ứng mỏ neo", "Thiên kiến mỏ neo", "Hệ thống 1", "Halo Effect", "Horn Effect", "Anchoring Bias", "System 1" },
                            reasonKeywords = new[] { "ấn tượng đầu", "hệ thống 1", "vội vàng", "đánh giá", "bằng chứng" },
                            minReasonWords = 8,
                            sampleExplanation = "Người phỏng vấn để Hệ thống 1 phản ứng tự động từ ấn tượng đầu tiên (đi trễ 2 phút) làm mỏ neo chi phối toàn bộ đánh giá năng lực chuyên môn phía sau.",
                            counterMeasureVi = "Chủ động kích hoạt Hệ thống 2 bằng bảng tiêu chí chấm điểm độc lập cho từng phần năng lực."
                        },
                        new
                        {
                            id = "psy-l11-bias-2",
                            title = "Chỉ tìm dẫn chứng ủng hộ giả định ban đầu",
                            contextTag = "Thiên kiến xác nhận",
                            scenario = "Sau khi tin rằng đồng nghiệp không thích mình, An chỉ chú ý những lần người đó im lặng trong cuộc họp và bỏ qua những lần họ hỗ trợ sửa lỗi kỹ thuật rất nhiệt tình.",
                            canonicalNameVi = "Thiên kiến xác nhận (Confirmation Bias)",
                            acceptedNames = new[] { "Thiên kiến xác nhận", "thien kien xac nhan", "Confirmation Bias", "Confirmation" },
                            reasonKeywords = new[] { "chú ý", "bỏ qua", "niềm tin", "củng cố", "bằng chứng" },
                            minReasonWords = 8,
                            sampleExplanation = "An chỉ chọn lọc thông tin củng cố niềm tin có sẵn và bỏ qua bằng chứng ngược lại — đây là Thiên kiến xác nhận.",
                            counterMeasureVi = "Viết ra ít nhất 2 sự kiện khách quan trái ngược với giả định ban đầu trước khi kết luận."
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
                Id = SeedDataInitializer.CreateDeterministicActivityId(
                    SeedDataInitializer.PsychologyCourseId,
                    SeedDataInitializer.PsychologyLesson11Id,
                    "feynman",
                    4),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson11Id,
                OrderIndex = 4,
                Type = "feynman",
                Title = "Feynman Tự Định Nghĩa: Hệ Thống 1 & 2 và Hiệu Ứng Stroop",
                Description = "Tự diễn giải khái niệm tâm lý học bằng ngôn từ đời thường kèm ví dụ thực tế của bạn.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "psy-l11-feyn-1",
                            conceptTitle = "Hệ thống 1 vs Hệ thống 2 (Kahneman)",
                            promptVi = "Hãy tự định nghĩa bằng lời của bạn cơ chế vận hành của Hệ thống 1 và Hệ thống 2, kèm 1 ví dụ đời thường cụ thể.",
                            requiredKeywords = new[] { "hệ thống 1|system 1", "hệ thống 2|system 2", "tự động|nhanh", "chậm|nỗ lực|logic", "ví dụ" },
                            minWords = 18,
                            sampleAnswer = "Hệ thống 1 là chế độ suy nghĩ nhanh, tự động và cảm tính (ví dụ đọc chữ quen thuộc hay giật mình khi nghe tiếng còi). Hệ thống 2 chậm hơn, cần nỗ lực tập trung và logic (ví dụ tính nhẩm phép nhân hai chữ số hoặc kiểm tra lại quyết định quan trọng).",
                            everydayExampleHintVi = "Ví dụ: phản xạ đọc chữ tự động vs cố gắng gọi tên màu mực trong bài test Stroop."
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
                Id = SeedDataInitializer.CreateDeterministicActivityId(
                    SeedDataInitializer.PsychologyCourseId,
                    SeedDataInitializer.PsychologyLesson11Id,
                    "cbt_exercise",
                    5),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson11Id,
                OrderIndex = 5,
                Type = "cbt_exercise",
                Title = "Bài Tập CBT Tình Huống: Nhận Diện Lỗi Nhận Thức & Tái Cấu Trúc",
                Description = "Từ tình huống mẫu, tự ghi suy nghĩ tự động, gọi tên lỗi nhận thức và viết lại suy nghĩ cân bằng.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "psy-l11-cbt-1",
                            title = "Đồng nghiệp chưa trả lời tin nhắn công việc sau 2 giờ",
                            scenario = "Bạn gửi đề xuất cải tiến lên nhóm chat lúc 10h sáng. Đến 12h trưa thấy 4 người đã xem nhưng chưa ai phản hồi. Bạn bắt đầu thấy căng thẳng và nghĩ đề xuất của mình thật ngớ ngẩn.",
                            sampleAutomaticThought = "Chắc chắn mọi người thấy ý tưởng của mình ngớ ngẩn và đang coi thường mình.",
                            canonicalDistortionVi = "Đọc suy nghĩ (Mind Reading)",
                            acceptedDistortions = new[] { "Đọc suy nghĩ", "doc suy nghi", "Mind Reading", "Kết luận vội vàng", "Jumping to Conclusions", "Trầm trọng hóa", "Catastrophizing" },
                            distortionChoicesHint = new[] { "Đọc suy nghĩ (Mind Reading)", "Trầm trọng hóa (Catastrophizing)", "Tư duy trắng - đen (All-or-Nothing)" },
                            balancedKeywords = new[] { "bằng chứng", "bận", "giờ nghỉ|trưa", "hỏi lại|làm rõ", "không có nghĩa" },
                            minBalancedWords = 12,
                            sampleBalancedThought = "Thực tế là chưa có bằng chứng nào cho thấy đồng nghiệp chê đề xuất của mình; có thể họ đang bận họp hoặc nghỉ trưa. Mình sẽ chủ động hỏi lại lịch góp ý vào 14h chiều.",
                            explanationVi = "Tự suy diễn điều người khác đang nghĩ mà không có dữ kiện xác thực là lỗi 'Đọc suy nghĩ' (Mind Reading)."
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
                Id = SeedDataInitializer.CreateDeterministicActivityId(
                    SeedDataInitializer.PsychologyCourseId,
                    SeedDataInitializer.PsychologyLesson11Id,
                    "apply_journal",
                    6),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson11Id,
                OrderIndex = 6,
                Type = "apply_journal",
                Title = "Nhật Ký Áp Dụng: Kích Hoạt Hệ Thống 2 & Giao Tiếp NVC 4 Bước",
                Description = "Viết kế hoạch áp dụng thực tế hoặc lời thoại NVC (Quan sát - Cảm xúc - Nhu cầu - Đề nghị) cho tình huống của chính bạn.",
                ConfigJson = JsonSerializer.Serialize(new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "psy-l11-apply-1",
                            title = "Kế Hoạch Áp Dụng NVC & Dừng 5 Giây Kích Hoạt Hệ Thống 2",
                            promptVi = "Chọn 1 cuộc trao đổi dễ căng thẳng trong tuần này của bạn, viết câu giao tiếp theo 4 bước NVC và kế hoạch thực hành trong 48 giờ tới.",
                            expectedKeywords = new[] { "quan sát", "cảm xúc", "nhu cầu", "đề nghị", "hệ thống 2" },
                            samplePlanVi = "Tình huống: Đồng đội nộp phần việc sát giờ. Mình dừng 5 giây kích hoạt Hệ thống 2, sau đó nói theo NVC: 'Mình thấy phần slide được gửi lúc 22h (quan sát), mình cảm thấy lo lắng (cảm xúc) vì cần thời gian duyệt chung để bài trình bày mượt mà (nhu cầu). Lần tới bạn có thể gửi bản nháp trước 18h được không (đề nghị)?'"
                        }
                    }
                }),
                PassingScore = 70,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        // Append expanded write-in activities across all 18 lessons for Coding, Investing, and Psychology
        list.AddRange(CodingWriteInContent.GetActivities(now));
        list.AddRange(InvestingWriteInContent.GetActivities(now));
        list.AddRange(PsychologyWriteInContent.GetActivities(now));

        return list;
    }

    /// <summary>
    /// Legacy OrderIndex 1..2 starter practice activities (12 Investing + 12 Coding + 12 Psychology = 36 total).
    /// Formerly seeded directly into retired courses; now migrated to the 3 standard curated courses.
    /// </summary>
    public static List<PracticeActivity> GetLegacyExpansionStarterActivities(DateTimeOffset now)
    {
        var list = new List<PracticeActivity>();

        // 1. Investing Legacy 1..2 (12 activities)
        {
            // 1.1
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson11Id, "compound_calculator", 1),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson11Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson11Id, "behavioral_quiz", 2),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson11Id,
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

            // 1.2
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson12Id, "compound_calculator", 1),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson12Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson12Id, "investment_case", 2),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson12Id,
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

            // 2.1
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson21Id, "portfolio_sim", 1),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson21Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson21Id, "investment_case", 2),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson21Id,
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

            // 2.2
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson22Id, "portfolio_sim", 1),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson22Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson22Id, "behavioral_quiz", 2),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson22Id,
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

            // 3.1
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson31Id, "investment_case", 1),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson31Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson31Id, "behavioral_quiz", 2),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson31Id,
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

            // 3.2
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson32Id, "investment_case", 1),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson32Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.InvestingCourseId, SeedDataInitializer.InvestingLesson32Id, "behavioral_quiz", 2),
                CourseId = SeedDataInitializer.InvestingCourseId,
                LessonId = SeedDataInitializer.InvestingLesson32Id,
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

        // 2. Coding Legacy 1..2 (12 activities)
        {
            // 1.1
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson11Id, "js_sandbox", 1),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson11Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson11Id, "parsons", 2),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson11Id,
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

            // 1.2
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson12Id, "js_sandbox", 1),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson12Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson12Id, "parsons", 2),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson12Id,
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

            // 2.1
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson21Id, "js_sandbox", 1),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson21Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson21Id, "bug_hunt", 2),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson21Id,
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

            // 2.2
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson22Id, "js_sandbox", 1),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson22Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson22Id, "bug_hunt", 2),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson22Id,
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

            // 3.1
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson31Id, "js_sandbox", 1),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson31Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson31Id, "parsons", 2),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson31Id,
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

            // 3.2
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson32Id, "js_sandbox", 1),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson32Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.CodingCourseId, SeedDataInitializer.CodingLesson32Id, "bug_hunt", 2),
                CourseId = SeedDataInitializer.CodingCourseId,
                LessonId = SeedDataInitializer.CodingLesson32Id,
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

        // 3. Psychology Legacy 1..2 (12 activities)
        {
            // 1.1
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson11Id, "stroop_test", 1),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson11Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson11Id, "psych_scenario", 2),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson11Id,
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

            // 1.2
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson12Id, "psych_scenario", 1),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson12Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson12Id, "reflection_journal", 2),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson12Id,
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

            // 2.1
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson21Id, "memory_span", 1),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson21Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson21Id, "reflection_journal", 2),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson21Id,
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

            // 2.2
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson22Id, "memory_span", 1),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson22Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson22Id, "psych_scenario", 2),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson22Id,
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

            // 3.1
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson31Id, "reflection_journal", 1),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson31Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson31Id, "psych_scenario", 2),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson31Id,
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

            // 3.2
            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson32Id, "psych_scenario", 1),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson32Id,
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

            list.Add(new PracticeActivity
            {
                Id = SeedDataInitializer.CreateDeterministicActivityId(SeedDataInitializer.PsychologyCourseId, SeedDataInitializer.PsychologyLesson32Id, "reflection_journal", 2),
                CourseId = SeedDataInitializer.PsychologyCourseId,
                LessonId = SeedDataInitializer.PsychologyLesson32Id,
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

        return list;
    }

    /// <summary>
    /// Migrates all 191 write-in labs (59 JS + 66 Finance + 66 Psychology) into the 3 standardized curated courses:
    /// 1. doc-course-studxy-fullstack-bootcamp (59 JS Write-In Labs)
    /// 2. document-course-investing (66 Finance Write-In Labs)
    /// 3. document-course-psychology (66 Psychology Write-In Labs)
    /// Strictly respects OrderIndex anti-collision (OrderIndex = 200 + originalOrderIndex, unique (LessonId, Type, OrderIndex))
    /// and uses deterministic GUIDs for 100% idempotent execution.
    /// Also migrates lesson quizzes to target lessons avoiding SQLite FK errors.
    /// </summary>
    public static async Task EnsureMigratedWriteInLabsSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var legacyStarters = GetLegacyExpansionStarterActivities(now);
        var midStarters = GetStarterWriteInActivities(now);

        // Group raw activities by domain
        var jsActivities = new List<PracticeActivity>();
        jsActivities.AddRange(legacyStarters.Where(a => a.CourseId == SeedDataInitializer.CodingCourseId));
        jsActivities.AddRange(midStarters.Where(a => a.CourseId == SeedDataInitializer.CodingCourseId));

        var finActivities = new List<PracticeActivity>();
        finActivities.AddRange(legacyStarters.Where(a => a.CourseId == SeedDataInitializer.InvestingCourseId));
        finActivities.AddRange(midStarters.Where(a => a.CourseId == SeedDataInitializer.InvestingCourseId));

        var psyActivities = new List<PracticeActivity>();
        psyActivities.AddRange(legacyStarters.Where(a => a.CourseId == SeedDataInitializer.PsychologyCourseId));
        psyActivities.AddRange(midStarters.Where(a => a.CourseId == SeedDataInitializer.PsychologyCourseId));

        var jsLessonSlugMap = new Dictionary<Guid, string>
        {
            [SeedDataInitializer.CodingLesson11Id] = "phase-3-javascript-mastery-lesson-05-array-methods-readme",
            [SeedDataInitializer.CodingLesson12Id] = "phase-3-javascript-mastery-lesson-04-es6-essentials-readme",
            [SeedDataInitializer.CodingLesson21Id] = "phase-3-javascript-mastery-lesson-07-closures-scope-readme",
            [SeedDataInitializer.CodingLesson22Id] = "phase-3-javascript-mastery-lesson-13-functional-programming-readme",
            [SeedDataInitializer.CodingLesson31Id] = "phase-3-javascript-mastery-lesson-09-async-await-readme",
            [SeedDataInitializer.CodingLesson32Id] = "phase-3-javascript-mastery-lesson-14-debugging-readme"
        };

        var finLessonSlugMap = new Dictionary<Guid, string>
        {
            [SeedDataInitializer.InvestingLesson11Id] = "notes-module-2-week-10-growth-index-etf-dca",
            [SeedDataInitializer.InvestingLesson12Id] = "notes-module-1-week-01-tien-te-lam-phat-lai-suat-tvm",
            [SeedDataInitializer.InvestingLesson21Id] = "notes-module-2-week-08-danh-muc-phan-bo-tai-san-mpt",
            [SeedDataInitializer.InvestingLesson22Id] = "notes-module-2-week-07-quan-tri-rui-ro-position-sizing-stop-loss",
            [SeedDataInitializer.InvestingLesson31Id] = "notes-module-2-week-09-dau-tu-gia-tri",
            [SeedDataInitializer.InvestingLesson32Id] = "notes-module-4-week-15-behavioral-finance-herd-fomo-loss-aversion-anchoring"
        };

        var psyLessonSlugMap = new Dictionary<Guid, string>
        {
            [SeedDataInitializer.PsychologyLesson11Id] = "notes-02-understanding-self-week-15-bias-blind-spots",
            [SeedDataInitializer.PsychologyLesson12Id] = "notes-01-scientific-foundations-week-05-sensation-perception",
            [SeedDataInitializer.PsychologyLesson21Id] = "notes-01-scientific-foundations-week-08-memory-stage-review",
            [SeedDataInitializer.PsychologyLesson22Id] = "notes-06-integration-week-44-psychology-work-learning",
            [SeedDataInitializer.PsychologyLesson31Id] = "notes-05-development-mental-health-week-42-evidence-based-therapies",
            [SeedDataInitializer.PsychologyLesson32Id] = "notes-04-love-relationships-week-30-boundaries-consent"
        };

        await MigrateDomainActivitiesAsync(db, "doc-course-studxy-fullstack-bootcamp", jsActivities, jsLessonSlugMap, now);
        await MigrateDomainActivitiesAsync(db, "document-course-investing", finActivities, finLessonSlugMap, now);
        await MigrateDomainActivitiesAsync(db, "document-course-psychology", psyActivities, psyLessonSlugMap, now);

        await EnsureMigratedCourseQuizzesSeededAsync(db, jsLessonSlugMap, finLessonSlugMap, psyLessonSlugMap, now);
    }

    private static async Task MigrateDomainActivitiesAsync(
        SwmsDbContext db,
        string targetCourseSlug,
        List<PracticeActivity> sourceActivities,
        Dictionary<Guid, string> lessonSlugMap,
        DateTimeOffset now)
    {
        var targetCourse = await db.Courses.FirstOrDefaultAsync(c => c.Slug == targetCourseSlug);
        if (targetCourse == null) return;

        var targetLessons = await db.Lessons
            .Where(l => db.Chapters.Any(ch => ch.Id == l.ChapterId && ch.CourseId == targetCourse.Id))
            .ToListAsync();

        if (targetLessons.Count == 0) return;

        var defaultFallbackLesson = targetLessons.OrderBy(l => l.SortOrder).First();
        var lessonBySlug = targetLessons.ToDictionary(l => l.Slug, StringComparer.OrdinalIgnoreCase);

        var existingInTarget = await db.PracticeActivities
            .Where(pa => pa.CourseId == targetCourse.Id)
            .ToListAsync();

        var existingIds = existingInTarget.Select(a => a.Id).ToHashSet();
        var existingCompositeKeys = existingInTarget
            .Where(a => a.LessonId.HasValue)
            .Select(a => (a.LessonId!.Value, Type: (a.Type ?? string.Empty).Trim().ToLowerInvariant(), a.OrderIndex))
            .ToHashSet();

        var usedOrderIndices = existingInTarget
            .Where(a => a.LessonId.HasValue)
            .GroupBy(a => a.LessonId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.OrderIndex).ToHashSet());

        var toAdd = new List<PracticeActivity>();

        foreach (var src in sourceActivities.OrderBy(a => a.LessonId).ThenBy(a => a.OrderIndex))
        {
            Lesson targetLesson = defaultFallbackLesson;
            if (src.LessonId.HasValue && lessonSlugMap.TryGetValue(src.LessonId.Value, out var slug))
            {
                if (lessonBySlug.TryGetValue(slug, out var matchedLesson))
                {
                    targetLesson = matchedLesson;
                }
            }

            var defaultMigratedId = SeedDataInitializer.CreateDeterministicGuid($"migrated-writein:{targetCourse.Id:N}:{src.Id:N}");
            var legacyFormulaId = SeedDataInitializer.CreateDeterministicGuid(
                $"migrated-writein:{targetCourse.Id:N}:{targetLesson.Id:N}:{(src.Type ?? "practice").Trim().ToLowerInvariant()}:{200 + src.OrderIndex}");

            if (existingIds.Contains(defaultMigratedId) || existingIds.Contains(legacyFormulaId))
            {
                continue;
            }

            if (!usedOrderIndices.TryGetValue(targetLesson.Id, out var usedOrders))
            {
                usedOrders = new HashSet<int>();
                usedOrderIndices[targetLesson.Id] = usedOrders;
            }

            var candidateOrder = 200 + src.OrderIndex;
            var actType = (src.Type ?? "practice").Trim().ToLowerInvariant();

            while (usedOrders.Contains(candidateOrder) ||
                   existingCompositeKeys.Contains((targetLesson.Id, actType, candidateOrder)))
            {
                candidateOrder++;
            }

            var newOrderIndex = candidateOrder;
            var newId = defaultMigratedId;

            usedOrders.Add(newOrderIndex);
            existingIds.Add(newId);
            existingCompositeKeys.Add((targetLesson.Id, actType, newOrderIndex));

            toAdd.Add(new PracticeActivity
            {
                Id = newId,
                CourseId = targetCourse.Id,
                LessonId = targetLesson.Id,
                OrderIndex = newOrderIndex,
                Type = src.Type,
                Title = src.Title,
                Description = src.Description,
                ConfigJson = src.ConfigJson,
                PassingScore = src.PassingScore,
                IsSeeded = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (toAdd.Count > 0)
        {
            db.PracticeActivities.AddRange(toAdd);
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureMigratedCourseQuizzesSeededAsync(
        SwmsDbContext db,
        Dictionary<Guid, string> jsLessonSlugMap,
        Dictionary<Guid, string> finLessonSlugMap,
        Dictionary<Guid, string> psyLessonSlugMap,
        DateTimeOffset now)
    {
        var rawQuizPacks = new List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)>();
        rawQuizPacks.AddRange(CodingWriteInContent.GetCourseQuizzes(now));
        rawQuizPacks.AddRange(InvestingWriteInContent.GetCourseQuizzes(now));
        rawQuizPacks.AddRange(PsychologyWriteInContent.GetCourseQuizzes(now));

        var combinedMap = new Dictionary<Guid, string>(jsLessonSlugMap);
        foreach (var (k, v) in finLessonSlugMap) combinedMap[k] = v;
        foreach (var (k, v) in psyLessonSlugMap) combinedMap[k] = v;

        var targetSlugs = combinedMap.Values.Distinct().ToList();
        var targetLessons = await db.Lessons.Where(l => targetSlugs.Contains(l.Slug)).ToListAsync();
        var lessonBySlug = targetLessons.ToDictionary(l => l.Slug, StringComparer.OrdinalIgnoreCase);

        var existingQuizzes = await db.PracticeQuizzes
            .Where(q => q.UserId == SeedDataInitializer.DefaultUserId)
            .ToListAsync();
        var existingQuizIds = existingQuizzes.Select(q => q.Id).ToHashSet();
        var existingQuizKeys = existingQuizzes
            .Where(q => q.LessonId.HasValue)
            .Select(q => (q.LessonId!.Value, Title: q.Title.Trim()))
            .ToHashSet();

        var existingItemIds = (await db.PracticeQuizItems.Select(qi => qi.Id).ToListAsync()).ToHashSet();

        var quizzesToAdd = new List<PracticeQuiz>();
        var itemsToAdd = new List<PracticeQuizItem>();

        foreach (var (rawQuiz, rawItems) in rawQuizPacks)
        {
            if (!rawQuiz.LessonId.HasValue) continue;
            if (!combinedMap.TryGetValue(rawQuiz.LessonId.Value, out var targetSlug)) continue;
            if (!lessonBySlug.TryGetValue(targetSlug, out var targetLesson)) continue;

            var targetLessonId = targetLesson.Id;
            var migratedQuizId = SeedDataInitializer.CreateDeterministicGuid($"migrated-quiz:{targetLessonId:N}:{rawQuiz.Title.Trim()}");

            var key = (targetLessonId, Title: rawQuiz.Title.Trim());
            bool exists = existingQuizIds.Contains(migratedQuizId) || existingQuizKeys.Contains(key);

            Guid actualQuizId = migratedQuizId;
            if (!exists)
            {
                var newQuiz = new PracticeQuiz
                {
                    Id = migratedQuizId,
                    UserId = SeedDataInitializer.DefaultUserId,
                    Title = rawQuiz.Title,
                    LessonId = targetLessonId,
                    TotalItems = rawQuiz.TotalItems,
                    CorrectItems = 0,
                    Status = "in_progress",
                    StartedAt = rawQuiz.StartedAt
                };
                quizzesToAdd.Add(newQuiz);
                existingQuizIds.Add(migratedQuizId);
                existingQuizKeys.Add(key);
            }
            else
            {
                var matched = existingQuizzes.FirstOrDefault(q => q.Id == migratedQuizId)
                    ?? existingQuizzes.FirstOrDefault(q => q.LessonId == targetLessonId && q.Title.Trim() == rawQuiz.Title.Trim());
                actualQuizId = matched?.Id ?? migratedQuizId;
            }

            for (int i = 0; i < rawItems.Count; i++)
            {
                var rawItem = rawItems[i];
                var itemId = SeedDataInitializer.CreateDeterministicGuid($"migrated-quiz-item:{actualQuizId:N}:{i + 1}");
                if (!existingItemIds.Contains(itemId))
                {
                    itemsToAdd.Add(new PracticeQuizItem
                    {
                        Id = itemId,
                        QuizId = actualQuizId,
                        ItemType = rawItem.ItemType,
                        PromptAstJson = rawItem.PromptAstJson,
                        ChoicesJson = rawItem.ChoicesJson,
                        ClozeSolution = rawItem.ClozeSolution,
                        AiRubricFeedbackJson = rawItem.AiRubricFeedbackJson,
                        CreatedAt = now.AddSeconds(i)
                    });
                    existingItemIds.Add(itemId);
                }
            }
        }

        if (quizzesToAdd.Count > 0) db.PracticeQuizzes.AddRange(quizzesToAdd);
        if (itemsToAdd.Count > 0) db.PracticeQuizItems.AddRange(itemsToAdd);
        if (quizzesToAdd.Count > 0 || itemsToAdd.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }

    public static async Task EnsureWriteInActivitiesSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
        await EnsureMigratedWriteInLabsSeededAsync(db, now);

        // First upgrade lesson theory content safely (idempotent, only if CurrentVersion == 1 and no v2 marker)
        await WriteInTheoryExpansion.EnsureLessonsUpgradedAsync(db, now);

        var candidates = GetStarterWriteInActivities(now);
        var targetCourseIds = candidates.Select(c => c.CourseId).Distinct().ToList();

        var existing = await db.PracticeActivities
            .Where(pa => targetCourseIds.Contains(pa.CourseId))
            .Select(pa => new { pa.Id, pa.CourseId, pa.LessonId, pa.Type, pa.OrderIndex })
            .ToListAsync();

        var existingIds = existing.Select(x => x.Id).ToHashSet();
        var existingCompositeKeys = existing
            .Select(x => (
                x.CourseId,
                x.LessonId,
                Type: (x.Type ?? string.Empty).Trim().ToLowerInvariant(),
                x.OrderIndex))
            .ToHashSet();

        var toAdd = new List<PracticeActivity>();
        foreach (var act in candidates)
        {
            var key = (
                act.CourseId,
                act.LessonId,
                Type: (act.Type ?? string.Empty).Trim().ToLowerInvariant(),
                act.OrderIndex);

            if (existingIds.Contains(act.Id) || existingCompositeKeys.Contains(key))
            {
                continue;
            }

            existingIds.Add(act.Id);
            existingCompositeKeys.Add(key);
            toAdd.Add(act);
        }

        if (toAdd.Count > 0)
        {
            db.PracticeActivities.AddRange(toAdd);
            await db.SaveChangesAsync();
        }

        // Seed deterministic course quizzes for Coding, Investing, and Psychology
        await EnsureWriteInCourseQuizzesSeededAsync(db, now);
    }

    /// <summary>
    /// Seeds deterministic multiple-choice quizzes across all 3 write-in courses (Coding, Investing, Psychology).
    /// Fully idempotent: checks by UserId, LessonId and Title (as well as Id) so repeated runs produce 0 duplicate records.
    /// </summary>
    public static async Task EnsureWriteInCourseQuizzesSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var courseQuizPacks = new List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)>();
        courseQuizPacks.AddRange(CodingWriteInContent.GetCourseQuizzes(now));
        courseQuizPacks.AddRange(InvestingWriteInContent.GetCourseQuizzes(now));
        courseQuizPacks.AddRange(PsychologyWriteInContent.GetCourseQuizzes(now));

        var quizCandidateIds = courseQuizPacks.Select(p => p.Quiz.Id).ToList();
        var targetLessonIds = courseQuizPacks.Where(p => p.Quiz.LessonId.HasValue).Select(p => p.Quiz.LessonId!.Value).Distinct().ToList();

        var existingQuizzes = await db.PracticeQuizzes
            .Where(q => q.UserId == SeedDataInitializer.DefaultUserId &&
                        (quizCandidateIds.Contains(q.Id) || (q.LessonId.HasValue && targetLessonIds.Contains(q.LessonId.Value))))
            .ToListAsync();

        var existingQuizIds = existingQuizzes.Select(q => q.Id).ToHashSet();
        var existingQuizKeys = existingQuizzes
            .Select(q => (q.UserId, q.LessonId, Title: q.Title.Trim()))
            .ToHashSet();

        var candidateQuizIds = courseQuizPacks.Select(p => p.Quiz.Id).ToList();
        var existingItemIds = (await db.PracticeQuizItems
            .Where(qi => candidateQuizIds.Contains(qi.QuizId))
            .Select(qi => qi.Id)
            .ToListAsync())
            .ToHashSet();

        var quizzesToAdd = new List<PracticeQuiz>();
        var itemsToAdd = new List<PracticeQuizItem>();

        foreach (var (quiz, items) in courseQuizPacks)
        {
            var key = (quiz.UserId, quiz.LessonId, Title: quiz.Title.Trim());
            bool quizExists = existingQuizIds.Contains(quiz.Id) || existingQuizKeys.Contains(key);

            Guid targetQuizId;
            if (!quizExists)
            {
                quizzesToAdd.Add(quiz);
                existingQuizIds.Add(quiz.Id);
                existingQuizKeys.Add(key);
                targetQuizId = quiz.Id;
            }
            else
            {
                var matched = existingQuizzes.FirstOrDefault(q => q.Id == quiz.Id)
                    ?? existingQuizzes.FirstOrDefault(q => q.UserId == quiz.UserId && q.LessonId == quiz.LessonId && q.Title.Trim() == quiz.Title.Trim());
                targetQuizId = matched?.Id ?? quiz.Id;
            }

            foreach (var item in items)
            {
                if (!existingItemIds.Contains(item.Id))
                {
                    item.QuizId = targetQuizId;
                    itemsToAdd.Add(item);
                    existingItemIds.Add(item.Id);
                }
            }
        }

        if (quizzesToAdd.Count > 0)
        {
            db.PracticeQuizzes.AddRange(quizzesToAdd);
        }
        if (itemsToAdd.Count > 0)
        {
            db.PracticeQuizItems.AddRange(itemsToAdd);
        }
        if (quizzesToAdd.Count > 0 || itemsToAdd.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }
}
