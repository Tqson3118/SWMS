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

    public static async Task EnsureWriteInActivitiesSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
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
    }
}
