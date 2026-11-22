namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Text.Json;
using Swms.Api.Domain.Entities;

/// <summary>
/// Expanded Write-in Practice Content for all 6 lessons of Coding Course (JavaScript).
/// Ensures EVERY lesson has >= 2 items for every single write-in type:
/// js_sandbox, predict_output, bug_fix, fill_code, parsons (Total >= 10 items/lesson).
/// </summary>
public static class CodingWriteInContent
{
    public static List<PracticeActivity> GetActivities(DateTimeOffset now)
    {
        var list = new List<PracticeActivity>();
        var courseId = SeedDataInitializer.CodingCourseId;

        // =========================================================================
        // LESSON 1.1: Map, Filter, Reduce & Tính Bất Biến
        // (Legacy activities occupy OrderIndex 1, 2; Starter Write-in occupy 3, 4, 5)
        // We add OrderIndex 6 (js_sandbox), 7 (parsons), 8 (bug_fix), 9 (fill_code)
        // =========================================================================
        var l11 = SeedDataInitializer.CodingLesson11Id;

        // 1.1 js_sandbox (OrderIndex 6)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "js_sandbox", 6),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 6,
            Type = "js_sandbox",
            Title = "JS Sandbox: Gom Nhóm Giao Dịch Theo Danh Mục Bằng Reduce",
            Description = "Viết hàm groupByCategory(items) trả về đối tượng gom nhóm tổng tiền theo category mà không đột biến mảng gốc.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l11-js-group",
                        title = "Gom nhóm tổng tiền theo category",
                        difficulty = "Trung bình",
                        description = "Viết hàm groupByCategory(items) nhận mảng [{ category, amount }] và trả về object { [category]: totalAmount }.",
                        functionName = "groupByCategory",
                        starterCode = "function groupByCategory(items) {\n  // Trả về object gom nhóm tổng amount theo category\n  return items.reduce((acc, item) => {\n    acc[item.category] = (acc[item.category] || 0) + item.amount;\n    return acc;\n  }, {});\n}",
                        solutionCode = "function groupByCategory(items) {\n  return items.reduce((acc, item) => {\n    acc[item.category] = (acc[item.category] || 0) + item.amount;\n    return acc;\n  }, {});\n}",
                        hint = "Dùng reduce với giá trị khởi tạo là object rỗng `{}`. Duyệt qua từng item và cộng dồn vào `acc[item.category]`.",
                        testCases = new object[]
                        {
                            new { name = "Nhiều danh mục khác nhau", input = new object[] { new object[] { new { category = "food", amount = 50 }, new { category = "book", amount = 120 }, new { category = "food", amount = 70 } } }, expected = new Dictionary<string, int> { { "food", 120 }, { "book", 120 } } },
                            new { name = "Mảng rỗng", input = new object[] { Array.Empty<object>() }, expected = new Dictionary<string, int>() }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 1.1 parsons (OrderIndex 7)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "parsons", 7),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 7,
            Type = "parsons",
            Title = "Parsons: Pipeline Làm Phẳng Mảng & Loại Bỏ Số Âm",
            Description = "Sắp xếp các dòng mã để hoàn thành hàm flatAndFilterPositive(nestedNums).",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l11-par-flat",
                        title = "Làm phẳng mảng lồng nhau và lọc số dương",
                        goal = "Sắp xếp thứ tự logic để làm phẳng mảng và lọc chỉ lấy các số > 0.",
                        lines = new object[]
                        {
                            new { id = "l1", text = "function flatAndFilterPositive(matrix) {", correctOrder = 1 },
                            new { id = "l2", text = "  return matrix", correctOrder = 2 },
                            new { id = "l3", text = "    .flat()", correctOrder = 3 },
                            new { id = "l4", text = "    .filter(n => n > 0);", correctOrder = 4 },
                            new { id = "l5", text = "}", correctOrder = 5 }
                        },
                        explanation = "Dùng flat() để đưa mảng 2 chiều về 1 chiều, sau đó dùng filter(n => n > 0) để loại bỏ số âm và số 0."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 1.1 bug_fix (OrderIndex 8 - second item for bug_fix)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "bug_fix", 8),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 8,
            Type = "bug_fix",
            Title = "Sửa Bug: Đột Biến Mảng Gốc Khi Sắp Xếp Điểm Số",
            Description = "Sửa hàm getTopScores(scores) để sắp xếp giảm dần mà không làm thay đổi mảng đầu vào.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l11-bugfix-2",
                        title = "Sửa lỗi Array.sort() làm biến đổi mảng gốc",
                        language = "javascript",
                        categoryTag = "JS Immutability",
                        context = "Hàm getTopScores(scores, k) cần trả về k phần tử điểm cao nhất. Bản hiện tại gọi trực tiếp scores.sort() làm thay đổi thứ tự mảng gốc.",
                        buggyCode = "function getTopScores(scores, k) {\n  return scores.sort((a, b) => b - a).slice(0, k);\n}",
                        bugLineNumber = 2,
                        cleanFixedCode = "function getTopScores(scores, k) {\n  return [...scores].sort((a, b) => b - a).slice(0, k);\n}",
                        requiredTokens = new[] { "[...scores]", "sort" },
                        functionName = "getTopScores",
                        testCases = new object[]
                        {
                            new { name = "Lấy top 2 điểm cao nhất", input = new object[] { new[] { 50, 95, 80, 60 }, 2 }, expected = new[] { 95, 80 } },
                            new { name = "Mảng rỗng", input = new object[] { Array.Empty<int>(), 3 }, expected = Array.Empty<int>() }
                        },
                        explanation = "Luôn sao chép mảng bằng `[...scores]` trước khi gọi `.sort()` để bảo toàn tính bất biến cho dữ liệu đầu vào."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 1.1 fill_code (OrderIndex 9 - second item for fill_code)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "fill_code", 9),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 9,
            Type = "fill_code",
            Title = "Điền Code JS: Tìm Điểm Trung Bình Học Viên Qua Map & Reduce",
            Description = "Điền phương thức mảng thích hợp để trích xuất điểm số và tính trung bình cộng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l11-fill-2",
                        title = "Tính điểm trung bình danh sách học viên",
                        category = "react",
                        language = "javascript",
                        description = "Hoàn thành hàm averageScore(students).",
                        practicalScenario = "Bảng xếp hạng cần tính điểm trung bình từ mảng đối tượng học sinh [{ name, score }].",
                        codeTemplate = new[]
                        {
                            "function averageScore(students) {",
                            "  if (!students.length) return 0;",
                            "  const total = students",
                            "    .{{BLANK_0}}(s => s.score)",
                            "    .{{BLANK_1}}((sum, score) => sum + score, 0);",
                            "  return total / students.length;",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "map", acceptableAnswers = new[] { "map" }, hint = "Phương thức trích xuất thuộc tính score thành mảng số", chips = new[] { "map", "filter", "forEach", "find" }, explanation = "map() biến đổi mảng đối tượng thành mảng các giá trị điểm số." },
                            new { id = "b1", blankIndex = 1, expected = "reduce", acceptableAnswers = new[] { "reduce" }, hint = "Phương thức tính tổng các phần tử", chips = new[] { "reduce", "filter", "some", "every" }, explanation = "reduce() cộng dồn toàn bộ điểm số bắt đầu từ 0." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 4, codeSnippet = ".map(s => s.score)", purpose = "Trích xuất giá trị score của mỗi học viên.", riskIfWrong = "Nếu dùng filter sẽ không biến đổi được cấu trúc dữ liệu." },
                            new { lineNumber = 5, codeSnippet = ".reduce((sum, score) => sum + score, 0)", purpose = "Tính tổng điểm.", riskIfWrong = "Nếu thiếu 0, mảng 1 phần tử hoặc rỗng sẽ gây lỗi không mong muốn." }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 1.1 predict_output (OrderIndex 10 - second activity for predict_output)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "predict_output", 10),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 10,
            Type = "predict_output",
            Title = "Đoán Output JS: Chain Methods & Scope Biến Đột Biến",
            Description = "Dự đoán output khi xâu chuỗi map, filter và kiểm tra tính bất biến của mảng.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l11-pred-3",
                        difficulty = "Trung bình",
                        title = "Chuỗi slice, map và reduce không làm thay đổi mảng ban đầu",
                        code = "const nums = [10, 20, 30, 40];\nconst res = nums.slice(1, 3).map(x => x + 5);\nconsole.log(JSON.stringify(res));\nconsole.log(nums.length);",
                        expectedOutput = "[25,35]\n4",
                        acceptedOutputs = new[] { "[25, 35]\n4", "[25,35]\n4" },
                        hint = "slice(1, 3) lấy [20, 30], map cộng 5 thành [25, 35]. Mảng nums vẫn giữ nguyên 4 phần tử.",
                        explanation = "slice và map trả về mảng mới nên nums.length vẫn là 4."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 1.1 js_sandbox (OrderIndex 11 - second write-in sandbox)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "js_sandbox", 11),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 11,
            Type = "js_sandbox",
            Title = "JS Sandbox: Tính Điểm Ròng Học Viên Bằng Pipeline Map & Filter",
            Description = "Viết hàm calculatePassAverage(scores, passingGrade) tính điểm trung bình của các học viên vượt qua ngưỡng điểm chuẩn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l11-js-pass-avg",
                        title = "Tính điểm trung bình các học viên đỗ",
                        difficulty = "Dễ",
                        description = "Viết hàm calculatePassAverage(scores, passingGrade) lọc các điểm >= passingGrade và trả về điểm trung bình (trả về 0 nếu không ai đỗ).",
                        functionName = "calculatePassAverage",
                        starterCode = "function calculatePassAverage(scores, passingGrade) {\n  const passed = scores.filter(s => s >= passingGrade);\n  if (!passed.length) return 0;\n  return passed.reduce((sum, s) => sum + s, 0) / passed.length;\n}",
                        solutionCode = "function calculatePassAverage(scores, passingGrade) {\n  const passed = scores.filter(s => s >= passingGrade);\n  if (!passed.length) return 0;\n  return passed.reduce((sum, s) => sum + s, 0) / passed.length;\n}",
                        hint = "Dùng filter để lọc điểm >= passingGrade, sau đó dùng reduce để cộng tổng chia cho passed.length.",
                        testCases = new object[]
                        {
                            new { name = "Có học viên đỗ", input = new object[] { new[] { 40, 75, 85, 30, 90 }, 60 }, expected = 83.33333333333333 },
                            new { name = "Không ai đỗ", input = new object[] { new[] { 30, 45, 50 }, 60 }, expected = 0 },
                            new { name = "Tất cả đều đỗ", input = new object[] { new[] { 80, 90, 100 }, 70 }, expected = 90 }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 1.1 parsons (OrderIndex 12 - second write-in parsons)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l11, "parsons", 12),
            CourseId = courseId,
            LessonId = l11,
            OrderIndex = 12,
            Type = "parsons",
            Title = "Parsons: Đếm Tần Suất Ký Tự Trong Chuỗi Bằng Reduce",
            Description = "Sắp xếp các dòng mã để hoàn thành hàm countCharFrequency(str).",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l11-par-freq",
                        title = "Đếm tần suất xuất hiện ký tự trong chuỗi",
                        goal = "Sắp xếp các dòng code logic để tạo bảng đếm tần suất ký tự không đột biến.",
                        lines = new object[]
                        {
                            new { id = "l1", text = "function countCharFrequency(str) {", correctOrder = 1 },
                            new { id = "l2", text = "  return str.split('')", correctOrder = 2 },
                            new { id = "l3", text = "    .reduce((map, ch) => {", correctOrder = 3 },
                            new { id = "l4", text = "      map[ch] = (map[ch] || 0) + 1;", correctOrder = 4 },
                            new { id = "l5", text = "      return map;", correctOrder = 5 },
                            new { id = "l6", text = "    }, {});", correctOrder = 6 },
                            new { id = "l7", text = "}", correctOrder = 7 }
                        },
                        explanation = "split('') tách chuỗi thành mảng ký tự, reduce bắt đầu từ `{}` đếm tần suất từng ký tự."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // =========================================================================
        // LESSON 1.2: Thuật Toán Tìm Kiếm & Kỹ Thuật Hai Con Trỏ (Two Pointers)
        // =========================================================================
        var l12 = SeedDataInitializer.CodingLesson12Id;

        // 1.2 js_sandbox (OrderIndex 3)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "js_sandbox", 3),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 3,
            Type = "js_sandbox",
            Title = "JS Sandbox: Kiểm Tra Chuỗi Đối Xứng (Palindrome) Bằng Two Pointers",
            Description = "Viết hàm isPalindrome(str) kiểm tra chuỗi đối xứng với hai con trỏ, bỏ qua ký tự đặc biệt.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l12-js-pal",
                        title = "Kiểm tra chuỗi đối xứng in-place với Two Pointers",
                        difficulty = "Dễ",
                        description = "Viết hàm isPalindrome(s) kiểm tra chuỗi đối xứng (chỉ tính chữ và số, không phân biệt hoa thường).",
                        functionName = "isPalindrome",
                        starterCode = "function isPalindrome(s) {\n  const clean = s.toLowerCase().replace(/[^a-z0-9]/g, '');\n  let left = 0, right = clean.length - 1;\n  while (left < right) {\n    if (clean[left] !== clean[right]) return false;\n    left++;\n    right--;\n  }\n  return true;\n}",
                        solutionCode = "function isPalindrome(s) {\n  const clean = s.toLowerCase().replace(/[^a-z0-9]/g, '');\n  let left = 0, right = clean.length - 1;\n  while (left < right) {\n    if (clean[left] !== clean[right]) return false;\n    left++;\n    right--;\n  }\n  return true;\n}",
                        hint = "Chuẩn hóa chuỗi bằng regex loại ký tự không phải chữ số, sau đó dùng 2 con trỏ từ 2 đầu so sánh.",
                        testCases = new object[]
                        {
                            new { name = "Chuỗi đối xứng: 'A man, a plan, a canal: Panama'", input = new object[] { "A man, a plan, a canal: Panama" }, expected = true },
                            new { name = "Chuỗi không đối xứng: 'race a car'", input = new object[] { "race a car" }, expected = false },
                            new { name = "Chuỗi rỗng", input = new object[] { "" }, expected = true }
                        }
                    },
                    new
                    {
                        id = "code-l12-js-twosum",
                        title = "Tìm hai số có tổng bằng target trên mảng đã sắp xếp",
                        difficulty = "Trung bình",
                        description = "Viết hàm twoSumSorted(numbers, target) trả về mảng 2 chỉ số [left, right] có tổng bằng target, hoặc [-1, -1] nếu không có.",
                        functionName = "twoSumSorted",
                        starterCode = "function twoSumSorted(numbers, target) {\n  let left = 0, right = numbers.length - 1;\n  while (left < right) {\n    const sum = numbers[left] + numbers[right];\n    if (sum === target) return [left, right];\n    if (sum < target) left++;\n    else right--;\n  }\n  return [-1, -1];\n}",
                        solutionCode = "function twoSumSorted(numbers, target) {\n  let left = 0, right = numbers.length - 1;\n  while (left < right) {\n    const sum = numbers[left] + numbers[right];\n    if (sum === target) return [left, right];\n    if (sum < target) left++;\n    else right--;\n  }\n  return [-1, -1];\n}",
                        hint = "Nếu tổng < target thì tăng left; nếu tổng > target thì giảm right.",
                        testCases = new object[]
                        {
                            new { name = "Có cặp thỏa mãn: [2, 7, 11, 15], target = 9", input = new object[] { new[] { 2, 7, 11, 15 }, 9 }, expected = new[] { 0, 1 } },
                            new { name = "Không có cặp nào: [1, 2, 3], target = 10", input = new object[] { new[] { 1, 2, 3 }, 10 }, expected = new[] { -1, -1 } }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 1.2 predict_output (OrderIndex 4)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "predict_output", 4),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 4,
            Type = "predict_output",
            Title = "Đoán Output: Con Trỏ Hai Đầu & Bước Nhảy Binary Search",
            Description = "Dự đoán chính xác kết quả in ra khi duyệt 2 con trỏ và tìm kiếm nhị phân.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l12-pred-1",
                        difficulty = "Dễ",
                        title = "Số bước lặp khi Binary Search tìm phần tử ở giữa",
                        code = "let steps = 0;\nfunction bs(arr, target) {\n  let l = 0, r = arr.length - 1;\n  while (l <= r) {\n    steps++;\n    const m = Math.floor((l + r) / 2);\n    if (arr[m] === target) return m;\n    if (arr[m] < target) l = m + 1;\n    else r = m - 1;\n  }\n  return -1;\n}\nbs([10, 20, 30, 40, 50], 30);\nconsole.log(steps);",
                        expectedOutput = "1",
                        hint = "Mảng có độ dài 5, phần tử giữa mid = 2 chính là giá trị 30, tìm thấy ngay lần lặp đầu tiên.",
                        explanation = "arr[2] = 30 khớp ngay với target ở vòng lặp thứ nhất, nên steps = 1."
                    },
                    new
                    {
                        id = "code-l12-pred-2",
                        difficulty = "Trung bình",
                        title = "Thứ tự hoán đổi của 2 con trỏ khi đảo mảng",
                        code = "const a = [1, 2, 3, 4];\nlet l = 0, r = a.length - 1;\nwhile (l < r) {\n  [a[l], a[r]] = [a[r], a[l]];\n  l++;\n  r--;\n}\nconsole.log(JSON.stringify(a));",
                        expectedOutput = "[4,3,2,1]",
                        acceptedOutputs = new[] { "[4, 3, 2, 1]", "[4,3,2,1]" },
                        hint = "Vòng 1 đổi 1 và 4 thành [4, 2, 3, 1]. Vòng 2 đổi 2 và 3 thành [4, 3, 2, 1].",
                        explanation = "Hai con trỏ hoán đổi đối xứng từ ngoài vào trong, tạo mảng đảo ngược in-place."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 1.2 bug_fix (OrderIndex 5)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "bug_fix", 5),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 5,
            Type = "bug_fix",
            Title = "Sửa Bug: Lỗi Vòng Lặp Vô Hạn Trong Binary Search",
            Description = "Sửa lỗi cập nhật chỉ số mid khiến vòng lặp while không bao giờ kết thúc.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l12-bug-1",
                        title = "Sửa lỗi cập nhật left = mid gây vòng lặp vô hạn",
                        language = "javascript",
                        categoryTag = "Binary Search Bug",
                        context = "Hàm binarySearch(arr, target) bị treo vô tận khi target lớn hơn phần tử giữa vì gán left = mid thay vì left = mid + 1.",
                        buggyCode = "function search(arr, target) {\n  let left = 0, right = arr.length - 1;\n  while (left <= right) {\n    const mid = Math.floor((left + right) / 2);\n    if (arr[mid] === target) return mid;\n    if (arr[mid] < target) left = mid;\n    else right = mid - 1;\n  }\n  return -1;\n}",
                        bugLineNumber = 6,
                        cleanFixedCode = "function search(arr, target) {\n  let left = 0, right = arr.length - 1;\n  while (left <= right) {\n    const mid = Math.floor((left + right) / 2);\n    if (arr[mid] === target) return mid;\n    if (arr[mid] < target) left = mid + 1;\n    else right = mid - 1;\n  }\n  return -1;\n}",
                        requiredTokens = new[] { "left = mid + 1", "right = mid - 1" },
                        functionName = "search",
                        testCases = new object[]
                        {
                            new { name = "Tìm số 8 trong [2, 4, 6, 8, 10]", input = new object[] { new[] { 2, 4, 6, 8, 10 }, 8 }, expected = 3 },
                            new { name = "Tìm số không có trong mảng", input = new object[] { new[] { 1, 3, 5 }, 4 }, expected = -1 }
                        },
                        explanation = "Nếu gán `left = mid`, khi `left` và `right` kề nhau thì `mid` luôn bằng `left`, dẫn đến không bao giờ thoát vòng lặp."
                    },
                    new
                    {
                        id = "code-l12-bug-2",
                        title = "Sửa lỗi điều kiện dừng trong Two Pointers",
                        language = "javascript",
                        categoryTag = "Two Pointers Bug",
                        context = "Hàm reverseSubarray(arr, start, end) dùng điều kiện `left <= right` hoán đổi thừa khi hai con trỏ gặp nhau.",
                        buggyCode = "function reverseRange(arr, start, end) {\n  const res = [...arr];\n  let left = start, right = end;\n  while (left <= right) {\n    const temp = res[left];\n    res[left] = res[right];\n    res[right] = temp;\n    left++;\n    right--;\n  }\n  return res;\n}",
                        bugLineNumber = 4,
                        cleanFixedCode = "function reverseRange(arr, start, end) {\n  const res = [...arr];\n  let left = start, right = end;\n  while (left < right) {\n    const temp = res[left];\n    res[left] = res[right];\n    res[right] = temp;\n    left++;\n    right--;\n  }\n  return res;\n}",
                        requiredTokens = new[] { "left < right" },
                        functionName = "reverseRange",
                        testCases = new object[]
                        {
                            new { name = "Đảo đoạn [1, 2, 3, 4, 5] từ chỉ số 1 đến 3", input = new object[] { new[] { 1, 2, 3, 4, 5 }, 1, 3 }, expected = new[] { 1, 4, 3, 2, 5 } }
                        },
                        explanation = "Dùng `left < right` giúp tránh hoán đổi vô nghĩa một phần tử với chính nó khi độ dài khoảng đảo là số lẻ."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 1.2 fill_code (OrderIndex 6)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "fill_code", 6),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 6,
            Type = "fill_code",
            Title = "Điền Code: Cài Đặt Binary Search An Toàn Tránh Tràn Số",
            Description = "Điền biểu thức tính mid và điều kiện dịch con trỏ trong tìm kiếm nhị phân.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l12-fill-1",
                        title = "Tính chỉ số giữa an toàn và bước nhảy",
                        category = "react",
                        language = "javascript",
                        description = "Hoàn thành các khoảng trống trong thuật toán Binary Search.",
                        practicalScenario = "Hệ thống cần tìm mã giao dịch trong danh mục 1 triệu bản ghi đã sắp xếp.",
                        codeTemplate = new[]
                        {
                            "function binarySearch(arr, target) {",
                            "  let left = 0, right = arr.length - 1;",
                            "  while ({{BLANK_0}}) {",
                            "    const mid = Math.floor((left + right) / 2);",
                            "    if (arr[mid] === target) return mid;",
                            "    if (arr[mid] < target) {{BLANK_1}};",
                            "    else {{BLANK_2}};",
                            "  }",
                            "  return -1;",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "left <= right", acceptableAnswers = new[] { "left <= right", "left<=right" }, hint = "Điều kiện tiếp tục vòng lặp khi không gian tìm kiếm còn ít nhất 1 phần tử", chips = new[] { "left <= right", "left < right", "left !== right" }, explanation = "Cần có dấu '=' để kiểm tra phần tử duy nhất còn lại khi left === right." },
                            new { id = "b1", blankIndex = 1, expected = "left = mid + 1", acceptableAnswers = new[] { "left = mid + 1", "left=mid+1" }, hint = "Thu hẹp sang nửa bên phải", chips = new[] { "left = mid + 1", "left = mid", "left++" }, explanation = "Bỏ qua phần tử mid đã xét, bắt đầu từ mid + 1." },
                            new { id = "b2", blankIndex = 2, expected = "right = mid - 1", acceptableAnswers = new[] { "right = mid - 1", "right=mid-1" }, hint = "Thu hẹp sang nửa bên trái", chips = new[] { "right = mid - 1", "right = mid", "right--" }, explanation = "Bỏ qua phần tử mid đã xét, giới hạn tới mid - 1." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 3, codeSnippet = "while (left <= right)", purpose = "Kiểm tra toàn bộ không gian từ left tới right.", riskIfWrong = "Nếu thiếu dấu '=' sẽ bỏ sót phần tử ở hai đầu mảng." }
                        }
                    },
                    new
                    {
                        id = "code-l12-fill-2",
                        title = "Hai con trỏ: Xóa phần tử trùng lặp trên mảng đã sắp xếp",
                        category = "react",
                        language = "javascript",
                        description = "Điền mã con trỏ ghi đè phần tử không trùng lặp.",
                        practicalScenario = "Lọc danh sách ID người dùng đã sort để loại bỏ ID trùng lặp mà không dùng thêm Set.",
                        codeTemplate = new[]
                        {
                            "function removeDuplicates(nums) {",
                            "  if (nums.length === 0) return 0;",
                            "  let write = 1;",
                            "  for (let read = 1; read < nums.length; read++) {",
                            "    if (nums[read] !== nums[{{BLANK_0}}]) {",
                            "      nums[write] = nums[read];",
                            "      {{BLANK_1}};",
                            "    }",
                            "  }",
                            "  return write;",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "read - 1", acceptableAnswers = new[] { "read - 1", "read-1" }, hint = "So sánh phần tử hiện tại với phần tử liền kề trước", chips = new[] { "read - 1", "write", "0" }, explanation = "Do mảng đã sắp xếp, nếu nums[read] !== nums[read - 1] tức là xuất hiện giá trị mới." },
                            new { id = "b1", blankIndex = 1, expected = "write++", acceptableAnswers = new[] { "write++", "write += 1" }, hint = "Tăng con trỏ ghi lên 1 vị trí", chips = new[] { "write++", "write--", "read++" }, explanation = "Ghi xong một phần tử độc nhất mới, dịch con trỏ ghi tiếp theo." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 5, codeSnippet = "nums[read] !== nums[read - 1]", purpose = "Phát hiện giá trị độc nhất mới.", riskIfWrong = "So sánh sai chỉ số sẽ ghi đè nhầm các giá trị hợp lệ." }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 1.2 parsons (OrderIndex 7)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l12, "parsons", 7),
            CourseId = courseId,
            LessonId = l12,
            OrderIndex = 7,
            Type = "parsons",
            Title = "Parsons: Hợp Nhất Hai Mảng Đã Sắp Xếp (Merge Sorted Arrays)",
            Description = "Sắp xếp thuật toán trộn 2 mảng tăng dần thành 1 mảng tăng dần với 2 con trỏ.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l12-par-1",
                        title = "Thuật toán Merge 2 mảng đã sắp xếp O(n + m)",
                        goal = "Sắp xếp các dòng mã để trộn a và b thành mảng kết quả tăng dần.",
                        lines = new object[]
                        {
                            new { id = "l1", text = "function mergeSorted(a, b) {", correctOrder = 1 },
                            new { id = "l2", text = "  let i = 0, j = 0, res = [];", correctOrder = 2 },
                            new { id = "l3", text = "  while (i < a.length && j < b.length) {", correctOrder = 3 },
                            new { id = "l4", text = "    if (a[i] <= b[j]) res.push(a[i++]);", correctOrder = 4 },
                            new { id = "l5", text = "    else res.push(b[j++]);", correctOrder = 5 },
                            new { id = "l6", text = "  }", correctOrder = 6 },
                            new { id = "l7", text = "  return res.concat(a.slice(i)).concat(b.slice(j));", correctOrder = 7 },
                            new { id = "l8", text = "}", correctOrder = 8 }
                        },
                        explanation = "So sánh hai đầu con trỏ i và j, lấy phần tử nhỏ hơn đẩy vào kết quả rồi gom phần còn lại."
                    },
                    new
                    {
                        id = "code-l12-par-2",
                        title = "Thuật toán tìm phần tử đơn độc duy nhất trong mảng đã sort",
                        goal = "Sắp xếp logic duyệt con trỏ bước nhảy 2 đơn vị.",
                        lines = new object[]
                        {
                            new { id = "p1", text = "function findSingle(nums) {", correctOrder = 1 },
                            new { id = "p2", text = "  for (let i = 0; i < nums.length - 1; i += 2) {", correctOrder = 2 },
                            new { id = "p3", text = "    if (nums[i] !== nums[i + 1]) return nums[i];", correctOrder = 3 },
                            new { id = "p4", text = "  }", correctOrder = 4 },
                            new { id = "p5", text = "  return nums[nums.length - 1];", correctOrder = 5 },
                            new { id = "p6", text = "}", correctOrder = 6 }
                        },
                        explanation = "Các cặp số giống nhau luôn đứng kề nhau ở chỉ số chẵn-lẻ, nếu lệch nhau thì phần tử tại i là đơn độc."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // =========================================================================
        // LESSON 2.1: Debounce & Throttle Xử Lý Sự Kiện Tần Suất Cao
        // =========================================================================
        var l21 = SeedDataInitializer.CodingLesson21Id;

        // 2.1 js_sandbox (OrderIndex 3)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "js_sandbox", 3),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 3,
            Type = "js_sandbox",
            Title = "JS Sandbox: Viết Hàm Throttle Tiết Chế Tần Suất Gọi Hàm",
            Description = "Viết hàm throttle(fn, limit) đảm bảo hàm chỉ được gọi tối đa 1 lần trong chu kỳ limit ms.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l21-js-throttle",
                        title = "Cài đặt hàm Throttle chuẩn xác với flag inThrottle",
                        difficulty = "Trung bình",
                        description = "Viết hàm throttle(fn, limit) trả về hàm mới có tính năng giới hạn tần suất thực thi.",
                        functionName = "throttle",
                        starterCode = "function throttle(fn, limit) {\n  let inThrottle = false;\n  return function(...args) {\n    if (!inThrottle) {\n      fn.apply(this, args);\n      inThrottle = true;\n      setTimeout(() => {\n        inThrottle = false;\n      }, limit);\n    }\n  };\n}",
                        solutionCode = "function throttle(fn, limit) {\n  let inThrottle = false;\n  return function(...args) {\n    if (!inThrottle) {\n      fn.apply(this, args);\n      inThrottle = true;\n      setTimeout(() => {\n        inThrottle = false;\n      }, limit);\n    }\n  };\n}",
                        hint = "Dùng cờ inThrottle. Khi hàm chạy, bật cờ = true và dùng setTimeout hạ cờ = false sau limit ms.",
                        testCases = new object[]
                        {
                            new { name = "Tạo hàm throttle hợp lệ", input = new object[] { "fn-placeholder", 100 }, expected = "function" }
                        }
                    },
                    new
                    {
                        id = "code-l21-js-debounce-cancel",
                        title = "Debounce có hỗ trợ hàm cancel() dọn dẹp timer",
                        difficulty = "Nâng cao",
                        description = "Cài đặt hàm debounce(fn, delay) có gắn kèm phương thức .cancel() để dọn dẹp timer khi cần.",
                        functionName = "debounceWithCancel",
                        starterCode = "function debounceWithCancel(fn, delay) {\n  let timer = null;\n  const debounced = function(...args) {\n    if (timer !== null) clearTimeout(timer);\n    timer = setTimeout(() => {\n      fn.apply(this, args);\n      timer = null;\n    }, delay);\n  };\n  debounced.cancel = function() {\n    if (timer !== null) {\n      clearTimeout(timer);\n      timer = null;\n    }\n  };\n  return debounced;\n}",
                        solutionCode = "function debounceWithCancel(fn, delay) {\n  let timer = null;\n  const debounced = function(...args) {\n    if (timer !== null) clearTimeout(timer);\n    timer = setTimeout(() => {\n      fn.apply(this, args);\n      timer = null;\n    }, delay);\n  };\n  debounced.cancel = function() {\n    if (timer !== null) {\n      clearTimeout(timer);\n      timer = null;\n    }\n  };\n  return debounced;\n}",
                        hint = "Gán thuộc tính cancel trực tiếp lên hàm debounced trả về.",
                        testCases = new object[]
                        {
                            new { name = "Trả về hàm có thuộc tính cancel là function", input = new object[] { "test-fn", 200 }, expected = "function" }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 2.1 predict_output (OrderIndex 4)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "predict_output", 4),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 4,
            Type = "predict_output",
            Title = "Đoán Output: Timer Closure & Thứ Tự Gọi Callback",
            Description = "Dự đoán chính xác kết quả khi tương tác với timer và biến đóng (closure).",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l21-pred-1",
                        difficulty = "Dễ",
                        title = "Biến đếm được bảo toàn trong Closure",
                        code = "function makeCounter() {\n  let count = 0;\n  return () => ++count;\n}\nconst c1 = makeCounter();\nconsole.log(c1());\nconsole.log(c1());",
                        expectedOutput = "1\n2",
                        hint = "Mỗi lần gọi c1(), biến count trong closure tăng lên 1 đơn vị.",
                        explanation = "Biến count được duy trì trong phạm vi của makeCounter, lần gọi 1 in 1, lần gọi 2 in 2."
                    },
                    new
                    {
                        id = "code-l21-pred-2",
                        difficulty = "Trung bình",
                        title = "Hủy timer bằng clearTimeout trước khi thực thi",
                        code = "let val = 0;\nconst t = setTimeout(() => { val = 10; }, 50);\nclearTimeout(t);\nconsole.log(val);",
                        expectedOutput = "0",
                        hint = "Lệnh clearTimeout(t) đã hủy callback trước khi nó có cơ hội chạy vào Call Stack.",
                        explanation = "Callback của setTimeout bị hủy hoàn toàn, biến val vẫn giữ nguyên giá trị khởi tạo là 0."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 2.1 bug_fix (OrderIndex 5)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "bug_fix", 5),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 5,
            Type = "bug_fix",
            Title = "Sửa Bug: Lỗi Mất Ngữ Cảnh this & Tham Số Trong Debounce",
            Description = "Sửa hàm debounce để giữ đúng ngữ cảnh this và truyền đầy đủ arguments.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l21-bug-1",
                        title = "Sửa lỗi mất tham số arguments trong hàm bọc debounce",
                        language = "javascript",
                        categoryTag = "Debounce Arguments Bug",
                        context = "Hàm debounce(fn, delay) gọi fn() mà không truyền args, làm hàm gốc luôn nhận undefined.",
                        buggyCode = "function debounce(fn, delay) {\n  let timer;\n  return function(...args) {\n    clearTimeout(timer);\n    timer = setTimeout(() => {\n      fn(); // BUG: thiếu tham số\n    }, delay);\n  };\n}",
                        bugLineNumber = 6,
                        cleanFixedCode = "function debounce(fn, delay) {\n  let timer;\n  return function(...args) {\n    clearTimeout(timer);\n    timer = setTimeout(() => {\n      fn.apply(this, args);\n    }, delay);\n  };\n}",
                        requiredTokens = new[] { "fn.apply(this, args)", "args" },
                        functionName = "debounce",
                        testCases = new object[]
                        {
                            new { name = "Trả về hàm debounce hợp lệ", input = new object[] { "fn-test", 100 }, expected = "function" }
                        },
                        explanation = "Phải truyền `...args` và áp dụng ngữ cảnh qua `fn.apply(this, args)` để hàm nhận đúng dữ liệu đầu vào."
                    },
                    new
                    {
                        id = "code-l21-bug-2",
                        title = "Sửa lỗi timer không được dọn dẹp khi khởi tạo lại cờ throttle",
                        language = "javascript",
                        categoryTag = "Throttle Bug",
                        context = "Biến inThrottle không được reset về false sau khi hết thời gian chờ.",
                        buggyCode = "function throttleBroken(fn, limit) {\n  let inThrottle = false;\n  return function(...args) {\n    if (!inThrottle) {\n      fn(...args);\n      inThrottle = true;\n      // BUG: Thiếu setTimeout reset inThrottle\n    }\n  };\n}",
                        bugLineNumber = 7,
                        cleanFixedCode = "function throttleBroken(fn, limit) {\n  let inThrottle = false;\n  return function(...args) {\n    if (!inThrottle) {\n      fn(...args);\n      inThrottle = true;\n      setTimeout(() => { inThrottle = false; }, limit);\n    }\n  };\n}",
                        requiredTokens = new[] { "setTimeout", "inThrottle = false" },
                        functionName = "throttleBroken",
                        testCases = new object[]
                        {
                            new { name = "Trả về hàm throttle hợp lệ", input = new object[] { "fn-test", 50 }, expected = "function" }
                        },
                        explanation = "Nếu không có setTimeout gán `inThrottle = false`, hàm chỉ chạy đúng 1 lần đầu tiên rồi bị khóa vĩnh viễn."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 2.1 fill_code (OrderIndex 6)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "fill_code", 6),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 6,
            Type = "fill_code",
            Title = "Điền Code: Hoàn Thiện Bộ Khung Debounce & Cleanup Timer",
            Description = "Điền các lệnh quản lý timer để tạo hàm debounce an toàn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l21-fill-1",
                        title = "Khung chuẩn Debounce hủy timer cũ và đặt timer mới",
                        category = "react",
                        language = "javascript",
                        description = "Điền lệnh hủy và đặt timer trong debounce.",
                        practicalScenario = "Tối ưu hóa ô tìm kiếm live search của thanh Header.",
                        codeTemplate = new[]
                        {
                            "function debounce(fn, delay) {",
                            "  let timerId = null;",
                            "  return function(...args) {",
                            "    if (timerId !== null) {{BLANK_0}}(timerId);",
                            "    timerId = {{BLANK_1}}(() => {",
                            "      fn.apply(this, args);",
                            "      timerId = null;",
                            "    }, delay);",
                            "  };",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "clearTimeout", acceptableAnswers = new[] { "clearTimeout" }, hint = "Hàm hủy bộ hẹn giờ trong JavaScript", chips = new[] { "clearTimeout", "clearInterval", "resetTimer" }, explanation = "clearTimeout(timerId) hủy lệnh hẹn giờ cũ đang chờ." },
                            new { id = "b1", blankIndex = 1, expected = "setTimeout", acceptableAnswers = new[] { "setTimeout" }, hint = "Hàm tạo bộ hẹn giờ trì hoãn thực thi", chips = new[] { "setTimeout", "setInterval", "queueMicrotask" }, explanation = "setTimeout trì hoãn việc gọi hàm fn sau khoảng thời gian delay ms." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 4, codeSnippet = "if (timerId !== null) clearTimeout(timerId);", purpose = "Hủy timer cũ để đếm lại từ đầu.", riskIfWrong = "Nếu thiếu, các lệnh trước đó vẫn sẽ thực thi gây spam request." }
                        }
                    },
                    new
                    {
                        id = "code-l21-fill-2",
                        title = "Throttle: Giữ nhịp gọi hàm scroll",
                        category = "react",
                        language = "javascript",
                        description = "Điền cờ điều kiện trong hàm throttle.",
                        practicalScenario = "Bắt sự kiện scroll để hiển thị nút Back To Top mà không gây giật lag màn hình.",
                        codeTemplate = new[]
                        {
                            "function throttle(fn, wait) {",
                            "  let isThrottled = false;",
                            "  return function(...args) {",
                            "    if (!{{BLANK_0}}) {",
                            "      fn.apply(this, args);",
                            "      isThrottled = {{BLANK_1}};",
                            "      setTimeout(() => { isThrottled = false; }, wait);",
                            "    }",
                            "  };",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "isThrottled", acceptableAnswers = new[] { "isThrottled" }, hint = "Tên cờ kiểm tra trạng thái đang trong chu kỳ chờ", chips = new[] { "isThrottled", "timer", "true" }, explanation = "Nếu chưa bị throttle (!isThrottled) thì mới cho phép thực thi hàm." },
                            new { id = "b1", blankIndex = 1, expected = "true", acceptableAnswers = new[] { "true" }, hint = "Bật cờ khóa để chặn các lệnh gọi tiếp theo trong cùng chu kỳ", chips = new[] { "true", "false", "null" }, explanation = "Gán true để khóa nhịp thực thi cho tới khi setTimeout kết thúc." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 6, codeSnippet = "isThrottled = true;", purpose = "Khóa hàm không cho gọi lại trong chu kỳ wait.", riskIfWrong = "Nếu không khóa, hàm vẫn chạy tự do như bình thường." }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 2.1 parsons (OrderIndex 7)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l21, "parsons", 7),
            CourseId = courseId,
            LessonId = l21,
            OrderIndex = 7,
            Type = "parsons",
            Title = "Parsons: Lắp Ráp Hàm Debounce Hoàn Chỉnh",
            Description = "Sắp xếp từng dòng mã để tạo ra một hàm debounce chuẩn mực.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l21-par-1",
                        title = "Lắp ráp closure debounce với timer",
                        goal = "Sắp xếp theo thứ tự logic đúng của hàm debounce.",
                        lines = new object[]
                        {
                            new { id = "l1", text = "function debounce(callback, delay) {", correctOrder = 1 },
                            new { id = "l2", text = "  let timerId;", correctOrder = 2 },
                            new { id = "l3", text = "  return function(...args) {", correctOrder = 3 },
                            new { id = "l4", text = "    clearTimeout(timerId);", correctOrder = 4 },
                            new { id = "l5", text = "    timerId = setTimeout(() => {", correctOrder = 5 },
                            new { id = "l6", text = "      callback.apply(this, args);", correctOrder = 6 },
                            new { id = "l7", text = "    }, delay);", correctOrder = 7 },
                            new { id = "l8", text = "  };", correctOrder = 8 },
                            new { id = "l9", text = "}", correctOrder = 9 }
                        },
                        explanation = "Khai báo biến timerId ở phạm vi ngoài cùng của closure, hủy timer cũ mỗi lần gọi rồi đặt timer mới."
                    },
                    new
                    {
                        id = "code-l21-par-2",
                        title = "Lắp ráp hàm Throttle đơn giản",
                        goal = "Sắp xếp logic cờ khóa throttle.",
                        lines = new object[]
                        {
                            new { id = "t1", text = "function throttle(fn, delay) {", correctOrder = 1 },
                            new { id = "t2", text = "  let locked = false;", correctOrder = 2 },
                            new { id = "t3", text = "  return function(...args) {", correctOrder = 3 },
                            new { id = "t4", text = "    if (locked) return;", correctOrder = 4 },
                            new { id = "t5", text = "    fn(...args);", correctOrder = 5 },
                            new { id = "t6", text = "    locked = true;", correctOrder = 6 },
                            new { id = "t7", text = "    setTimeout(() => { locked = false; }, delay);", correctOrder = 7 },
                            new { id = "t8", text = "  };", correctOrder = 8 },
                            new { id = "t9", text = "}", correctOrder = 9 }
                        },
                        explanation = "Nếu locked đang true thì thoát ngay; nếu chưa thì chạy fn, đặt locked = true và hẹn giờ mở lại."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // =========================================================================
        // LESSON 2.2: Memoization & Cache Kết Quả Hàm Tính Toán Phức Tạp
        // =========================================================================
        var l22 = SeedDataInitializer.CodingLesson22Id;

        // 2.2 js_sandbox (OrderIndex 3)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "js_sandbox", 3),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 3,
            Type = "js_sandbox",
            Title = "JS Sandbox: Viết Hàm Memoize Đa Tham Số Với Map Cache",
            Description = "Cài đặt memoize(fn) hỗ trợ cache kết quả các hàm thuần khiết dựa trên JSON serialize key.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l22-js-mem-1",
                        title = "Memoize hàm tính giai thừa với Map",
                        difficulty = "Trung bình",
                        description = "Viết hàm memoizeFactorial() trả về hàm tính giai thừa n! có gắn kèm cache Map nội bộ.",
                        functionName = "memoizeFactorial",
                        starterCode = "function memoizeFactorial() {\n  const cache = new Map();\n  function fact(n) {\n    if (n <= 1) return 1;\n    if (cache.has(n)) return cache.get(n);\n    const res = n * fact(n - 1);\n    cache.set(n, res);\n    return res;\n  }\n  return fact;\n}",
                        solutionCode = "function memoizeFactorial() {\n  const cache = new Map();\n  function fact(n) {\n    if (n <= 1) return 1;\n    if (cache.has(n)) return cache.get(n);\n    const res = n * fact(n - 1);\n    cache.set(n, res);\n    return res;\n  }\n  return fact;\n}",
                        hint = "Kiểm tra cache.has(n) trước khi tính toán đệ quy. Lưu kết quả vào cache.set(n, res).",
                        testCases = new object[]
                        {
                            new { name = "Tính giai thừa 5! = 120", input = Array.Empty<object>(), expected = "function" }
                        }
                    },
                    new
                    {
                        id = "code-l22-js-mem-2",
                        title = "Memoize tổng quát cho hàm đa tham số",
                        difficulty = "Nâng cao",
                        description = "Viết hàm memoizeGeneral(fn) cache kết quả dựa trên JSON.stringify(args).",
                        functionName = "memoizeGeneral",
                        starterCode = "function memoizeGeneral(fn) {\n  const cache = new Map();\n  return function(...args) {\n    const key = JSON.stringify(args);\n    if (cache.has(key)) return cache.get(key);\n    const result = fn.apply(this, args);\n    cache.set(key, result);\n    return result;\n  };\n}",
                        solutionCode = "function memoizeGeneral(fn) {\n  const cache = new Map();\n  return function(...args) {\n    const key = JSON.stringify(args);\n    if (cache.has(key)) return cache.get(key);\n    const result = fn.apply(this, args);\n    cache.set(key, result);\n    return result;\n  };\n}",
                        hint = "Dùng JSON.stringify(args) làm khóa tra cứu trong Map.",
                        testCases = new object[]
                        {
                            new { name = "Memoize hàm cộng 2 số", input = new object[] { "add-fn" }, expected = "function" }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 2.2 predict_output (OrderIndex 4)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "predict_output", 4),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 4,
            Type = "predict_output",
            Title = "Đoán Output: Cache Hit vs Cache Miss Trong Memoization",
            Description = "Dự đoán số lần hàm gốc được gọi khi có cache đệm.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l22-pred-1",
                        difficulty = "Dễ",
                        title = "Số lần hàm tính toán chạy thực tế",
                        code = "let callCount = 0;\nfunction square(n) {\n  callCount++;\n  return n * n;\n}\nconst cache = new Map();\nfunction memoSquare(n) {\n  if (cache.has(n)) return cache.get(n);\n  const res = square(n);\n  cache.set(n, res);\n  return res;\n}\nmemoSquare(4);\nmemoSquare(4);\nmemoSquare(5);\nconsole.log(callCount);",
                        expectedOutput = "2",
                        hint = "Gọi memoSquare(4) lần 2 là cache hit nên square(4) không bị chạy lại. Tổng cộng square chỉ chạy cho n = 4 và n = 5.",
                        explanation = "Lần đầu n = 4 gọi square (callCount = 1). Lần 2 n = 4 lấy từ cache. Lần 3 n = 5 gọi square (callCount = 2)."
                    },
                    new
                    {
                        id = "code-l22-pred-2",
                        difficulty = "Trung bình",
                        title = "Xung đột key khi không dùng JSON.stringify",
                        code = "const cache = {};\nfunction getKey(obj) { return obj.toString(); }\nconst k1 = getKey({ a: 1 });\nconst k2 = getKey({ b: 2 });\nconsole.log(k1 === k2);",
                        expectedOutput = "true",
                        hint = "Mọi object khi gọi .toString() mặc định đều biến thành chuỗi '[object Object]'.",
                        explanation = "Cả k1 và k2 đều là '[object Object]', dẫn tới xung đột khóa nghiêm trọng nếu dùng object làm key mà không serialize."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 2.2 bug_fix (OrderIndex 5)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "bug_fix", 5),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 5,
            Type = "bug_fix",
            Title = "Sửa Bug: Lỗi Đột Biến Kết Quả Đang Nằm Trong Cache",
            Description = "Phát hiện và sửa lỗi hàm trả về tham chiếu trực tiếp của object trong cache, khiến dữ liệu cache bị sửa đổi ngoài ý muốn.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l22-bug-1",
                        title = "Sửa lỗi trả về tham chiếu mảng trong cache làm hỏng dữ liệu gốc",
                        language = "javascript",
                        categoryTag = "Cache Mutation Bug",
                        context = "Hàm getCachedList(key) trả về trực tiếp mảng trong cache, khi caller gọi .push() thì mảng trong cache bị đột biến theo.",
                        buggyCode = "const memory = new Map();\nfunction getCachedList(key) {\n  if (memory.has(key)) {\n    return memory.get(key); // BUG: Trả về tham chiếu trực tiếp\n  }\n  const list = ['item-1', 'item-2'];\n  memory.set(key, list);\n  return list;\n}",
                        bugLineNumber = 4,
                        cleanFixedCode = "const memory = new Map();\nfunction getCachedList(key) {\n  if (memory.has(key)) {\n    return [...memory.get(key)];\n  }\n  const list = ['item-1', 'item-2'];\n  memory.set(key, list);\n  return [...list];\n}",
                        requiredTokens = new[] { "[...memory.get(key)]", "[...list]" },
                        functionName = "getCachedList",
                        testCases = new object[]
                        {
                            new { name = "Lấy danh sách từ cache độc lập", input = new object[] { "users" }, expected = new[] { "item-1", "item-2" } }
                        },
                        explanation = "Luôn trả về bản sao `[...data]` hoặc clone sâu để người gọi không làm biến dạng dữ liệu đang lưu trong cache."
                    },
                    new
                    {
                        id = "code-l22-bug-2",
                        title = "Sửa lỗi hàm memoize không lưu kết quả vào cache khi tính xong",
                        language = "javascript",
                        categoryTag = "Memoize Cache Set Bug",
                        context = "Hàm memoize tính toán kết quả nhưng quên gọi cache.set(key, result), dẫn đến lần nào cũng bị cache miss.",
                        buggyCode = "function memo(fn) {\n  const cache = new Map();\n  return function(x) {\n    if (cache.has(x)) return cache.get(x);\n    const res = fn(x);\n    // BUG: Thiếu cache.set(x, res);\n    return res;\n  };\n}",
                        bugLineNumber = 6,
                        cleanFixedCode = "function memo(fn) {\n  const cache = new Map();\n  return function(x) {\n    if (cache.has(x)) return cache.get(x);\n    const res = fn(x);\n    cache.set(x, res);\n    return res;\n  };\n}",
                        requiredTokens = new[] { "cache.set(x, res)" },
                        functionName = "memo",
                        testCases = new object[]
                        {
                            new { name = "Tạo hàm memo hợp lệ", input = new object[] { "test" }, expected = "function" }
                        },
                        explanation = "Phải lưu kết quả vào cache bằng `cache.set(key, res)` trước khi trả về."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 2.2 fill_code (OrderIndex 6)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "fill_code", 6),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 6,
            Type = "fill_code",
            Title = "Điền Code: Bộ Khung Hàm Memoize Với Map",
            Description = "Điền các phương thức Map (has, get, set) để hoàn thiện hàm memoize.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l22-fill-1",
                        title = "Hoàn thiện chu trình tra cứu và lưu trữ Map Cache",
                        category = "react",
                        language = "javascript",
                        description = "Điền các phương thức của Map.",
                        practicalScenario = "Cache kết quả tính toán chi phí vận chuyển đơn hàng.",
                        codeTemplate = new[]
                        {
                            "function memoize(fn) {",
                            "  const cache = new Map();",
                            "  return function(...args) {",
                            "    const key = JSON.stringify(args);",
                            "    if (cache.{{BLANK_0}}(key)) {",
                            "      return cache.{{BLANK_1}}(key);",
                            "    }",
                            "    const result = fn.apply(this, args);",
                            "    cache.{{BLANK_2}}(key, result);",
                            "    return result;",
                            "  };",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "has", acceptableAnswers = new[] { "has" }, hint = "Kiểm tra xem key đã tồn tại trong Map chưa", chips = new[] { "has", "get", "contains", "find" }, explanation = "cache.has(key) trả về boolean kiểm tra sự tồn tại của khóa." },
                            new { id = "b1", blankIndex = 1, expected = "get", acceptableAnswers = new[] { "get" }, hint = "Lấy giá trị từ Map dựa trên key", chips = new[] { "get", "fetch", "value", "find" }, explanation = "cache.get(key) trả về giá trị đã lưu trong cache." },
                            new { id = "b2", blankIndex = 2, expected = "set", acceptableAnswers = new[] { "set" }, hint = "Gán cặp key - value vào Map", chips = new[] { "set", "put", "add", "push" }, explanation = "cache.set(key, result) lưu giá trị mới vào Map." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 5, codeSnippet = "if (cache.has(key)) return cache.get(key);", purpose = "Cache hit: trả về kết quả ngay lập tức.", riskIfWrong = "Nếu kiểm tra sai sẽ tính toán lại không cần thiết." }
                        }
                    },
                    new
                    {
                        id = "code-l22-fill-2",
                        title = "Giới hạn kích thước cache để chống tràn bộ nhớ",
                        category = "react",
                        language = "javascript",
                        description = "Điền lệnh xóa key cũ nhất khi vượt quá dung lượng.",
                        practicalScenario = "Giới hạn cache tối đa 100 mục trong môi trường browser hạn chế RAM.",
                        codeTemplate = new[]
                        {
                            "function setWithLimit(cache, key, val, maxLimit) {",
                            "  if (cache.size >= maxLimit) {",
                            "    const oldestKey = cache.keys().{{BLANK_0}}().value;",
                            "    cache.{{BLANK_1}}(oldestKey);",
                            "  }",
                            "  cache.set(key, val);",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "next", acceptableAnswers = new[] { "next" }, hint = "Phương thức lấy phần tử đầu tiên của Iterator trong JS", chips = new[] { "next", "first", "shift" }, explanation = "cache.keys().next().value lấy khóa đầu tiên (cũ nhất) của Map." },
                            new { id = "b1", blankIndex = 1, expected = "delete", acceptableAnswers = new[] { "delete" }, hint = "Phương thức xóa phần tử khỏi Map", chips = new[] { "delete", "remove", "clear" }, explanation = "cache.delete(key) xóa khóa cũ để nhường chỗ cho khóa mới." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 4, codeSnippet = "cache.delete(oldestKey);", purpose = "Loại bỏ phần tử cũ nhất theo cơ chế FIFO.", riskIfWrong = "Không xóa sẽ khiến Map phình to gây tràn RAM." }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 2.2 parsons (OrderIndex 7)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l22, "parsons", 7),
            CourseId = courseId,
            LessonId = l22,
            OrderIndex = 7,
            Type = "parsons",
            Title = "Parsons: Cài Đặt Hàm Memoize Tổng Quát",
            Description = "Sắp xếp các dòng mã để xây dựng hàm bọc memoize chuẩn mực.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l22-par-1",
                        title = "Sắp xếp logic hàm memoize đa tham số",
                        goal = "Sắp xếp theo thứ tự thực thi đúng của memoize.",
                        lines = new object[]
                        {
                            new { id = "m1", text = "function memoize(fn) {", correctOrder = 1 },
                            new { id = "m2", text = "  const cache = new Map();", correctOrder = 2 },
                            new { id = "m3", text = "  return function(...args) {", correctOrder = 3 },
                            new { id = "m4", text = "    const key = JSON.stringify(args);", correctOrder = 4 },
                            new { id = "m5", text = "    if (cache.has(key)) return cache.get(key);", correctOrder = 5 },
                            new { id = "m6", text = "    const res = fn(...args);", correctOrder = 6 },
                            new { id = "m7", text = "    cache.set(key, res);", correctOrder = 7 },
                            new { id = "m8", text = "    return res;", correctOrder = 8 },
                            new { id = "m9", text = "  };", correctOrder = 9 },
                            new { id = "m10", text = "}", correctOrder = 10 }
                        },
                        explanation = "Tạo Map lưu trữ, tuần tự serialize args, kiểm tra cache và lưu kết quả nếu tính mới."
                    },
                    new
                    {
                        id = "code-l22-par-2",
                        title = "Thuật toán Fibonacci với Memoization đệ quy",
                        goal = "Sắp xếp hàm fibMemoized.",
                        lines = new object[]
                        {
                            new { id = "f1", text = "const memo = new Map();", correctOrder = 1 },
                            new { id = "f2", text = "function fib(n) {", correctOrder = 2 },
                            new { id = "f3", text = "  if (n <= 1) return n;", correctOrder = 3 },
                            new { id = "f4", text = "  if (memo.has(n)) return memo.get(n);", correctOrder = 4 },
                            new { id = "f5", text = "  const val = fib(n - 1) + fib(n - 2);", correctOrder = 5 },
                            new { id = "f6", text = "  memo.set(n, val);", correctOrder = 6 },
                            new { id = "f7", text = "  return val;", correctOrder = 7 },
                            new { id = "f8", text = "}", correctOrder = 8 }
                        },
                        explanation = "Giảm độ phức tạp thời gian từ O(2^n) hàm mũ xuống O(n) tuyến tính nhờ memoization."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // =========================================================================
        // LESSON 3.1: Async/Await, Promise Pipeline & Retry Cơ Chế Tự Phục Hồi
        // =========================================================================
        var l31 = SeedDataInitializer.CodingLesson31Id;

        // 3.1 js_sandbox (OrderIndex 3)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "js_sandbox", 3),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 3,
            Type = "js_sandbox",
            Title = "JS Sandbox: Viết Hàm Sleep & Trì Hoãn Thực Thi Async",
            Description = "Viết hàm sleep(ms) trả về Promise tự resolve sau ms mili-giây.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l31-js-sleep",
                        title = "Hàm sleep(ms) bất đồng bộ với Promise",
                        difficulty = "Dễ",
                        description = "Viết hàm sleep(ms) trả về Promise để dùng với await sleep(ms).",
                        functionName = "sleep",
                        starterCode = "function sleep(ms) {\n  return new Promise(resolve => setTimeout(resolve, ms));\n}",
                        solutionCode = "function sleep(ms) {\n  return new Promise(resolve => setTimeout(resolve, ms));\n}",
                        hint = "Bọc setTimeout bên trong constructor new Promise((resolve) => ...).",
                        testCases = new object[]
                        {
                            new { name = "Tạo sleep promise hợp lệ", input = new object[] { 10 }, expected = "object" }
                        }
                    },
                    new
                    {
                        id = "code-l31-js-retry",
                        title = "Cài đặt Retry đơn giản với số lần thử tối đa",
                        difficulty = "Nâng cao",
                        description = "Viết hàm retrySimple(fn, retries) gọi lại hàm fn nếu thất bại cho tới khi hết retries.",
                        functionName = "retrySimple",
                        starterCode = "async function retrySimple(fn, retries) {\n  for (let i = 0; i <= retries; i++) {\n    try {\n      return await fn();\n    } catch (err) {\n      if (i === retries) throw err;\n    }\n  }\n}",
                        solutionCode = "async function retrySimple(fn, retries) {\n  for (let i = 0; i <= retries; i++) {\n    try {\n      return await fn();\n    } catch (err) {\n      if (i === retries) throw err;\n    }\n  }\n}",
                        hint = "Dùng vòng lặp for và try...catch. Bắt lỗi nếu chưa hết lần thử thì cho lặp tiếp; nếu lần cuối thì ném lỗi ra ngoài.",
                        testCases = new object[]
                        {
                            new { name = "Trả về hàm retry hợp lệ", input = new object[] { "fn-test", 2 }, expected = "object" }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 3.1 predict_output (OrderIndex 4)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "predict_output", 4),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 4,
            Type = "predict_output",
            Title = "Đoán Output: Microtask (Promise) vs Macrotask (setTimeout)",
            Description = "Dự đoán thứ tự xuất hiện các dòng console.log trong Event Loop.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l31-pred-1",
                        difficulty = "Trung bình",
                        title = "Thứ tự thực thi đồng bộ, microtask và setTimeout",
                        code = "console.log('1');\nsetTimeout(() => console.log('2'), 0);\nPromise.resolve().then(() => console.log('3'));\nconsole.log('4');",
                        expectedOutput = "1\n4\n3\n2",
                        hint = "Mã đồng bộ (1, 4) chạy trước, sau đó tới Microtask (3), cuối cùng mới tới Macrotask setTimeout (2).",
                        explanation = "Call Stack chạy hết 1 và 4. Sau đó Event Loop xả hàng đợi Microtask in 3. Cuối cùng mới lấy macrotask từ hàng đợi in 2."
                    },
                    new
                    {
                        id = "code-l31-pred-2",
                        difficulty = "Nâng cao",
                        title = "Await tạm ngưng hàm async và đưa phần còn lại vào microtask",
                        code = "async function demo() {\n  console.log('start');\n  await Promise.resolve();\n  console.log('after await');\n}\ndemo();\nconsole.log('sync end');",
                        expectedOutput = "start\nsync end\nafter await",
                        hint = "'start' chạy đồng bộ trong demo(), sau đó gặp await thì nhả quyền cho Call Stack chạy 'sync end', rồi mới chạy tiếp 'after await'.",
                        explanation = "Lệnh await biến toàn bộ phần code phía sau thành một microtask callback trong hàng đợi."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 3.1 bug_fix (OrderIndex 5)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "bug_fix", 5),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 5,
            Type = "bug_fix",
            Title = "Sửa Bug: Lỗi Dùng forEach Với Async Task Gây Mất Đồng Bộ",
            Description = "Sửa hàm tải dữ liệu tuần tự dùng sai Array.prototype.forEach dẫn đến không chờ Promise hoàn tất.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l31-bug-1",
                        title = "Sửa lỗi forEach không chờ await bằng vòng lặp for...of",
                        language = "javascript",
                        categoryTag = "Async ForEach Bug",
                        context = "Hàm fetchAllSequentially(urls) dùng forEach(async url => await fetch) khiến hàm trả về mảng rỗng trước khi tải xong.",
                        buggyCode = "async function fetchAll(urls, fetchFn) {\n  const results = [];\n  // BUG: forEach không chờ Promise hoàn tất!\n  urls.forEach(async (url) => {\n    const res = await fetchFn(url);\n    results.push(res);\n  });\n  return results;\n}",
                        bugLineNumber = 4,
                        cleanFixedCode = "async function fetchAll(urls, fetchFn) {\n  const results = [];\n  for (const url of urls) {\n    const res = await fetchFn(url);\n    results.push(res);\n  }\n  return results;\n}",
                        requiredTokens = new[] { "for (const url of urls)", "await fetchFn(url)" },
                        functionName = "fetchAll",
                        testCases = new object[]
                        {
                            new { name = "Chạy tuần tự trả về kết quả mảng", input = new object[] { new[] { "url1", "url2" }, "fn" }, expected = "object" }
                        },
                        explanation = "Array.forEach bỏ qua giá trị trả về của callback. Phải dùng vòng lặp `for...of` để `await` tuần tự từng phần tử."
                    },
                    new
                    {
                        id = "code-l31-bug-2",
                        title = "Sửa lỗi Promise constructor quên gọi reject khi gặp lỗi",
                        language = "javascript",
                        categoryTag = "Promise Bug",
                        context = "Hàm bọc timeoutPromise(ms) không gọi reject khi hết giờ, làm Promise bị treo vĩnh viễn (stalled).",
                        buggyCode = "function timeoutPromise(ms) {\n  return new Promise((resolve, reject) => {\n    setTimeout(() => {\n      // BUG: Quên gọi reject('Timeout');\n    }, ms);\n  });\n}",
                        bugLineNumber = 4,
                        cleanFixedCode = "function timeoutPromise(ms) {\n  return new Promise((resolve, reject) => {\n    setTimeout(() => {\n      reject(new Error('Timeout'));\n    }, ms);\n  });\n}",
                        requiredTokens = new[] { "reject" },
                        functionName = "timeoutPromise",
                        testCases = new object[]
                        {
                            new { name = "Tạo timeout promise hợp lệ", input = new object[] { 50 }, expected = "object" }
                        },
                        explanation = "Phải gọi `reject(new Error('Timeout'))` khi hết thời gian chờ để thông báo lỗi cho luồng gọi."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 3.1 fill_code (OrderIndex 6)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "fill_code", 6),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 6,
            Type = "fill_code",
            Title = "Điền Code: Khung Chuẩn Promise.allSettled & Exponential Backoff",
            Description = "Điền các từ khóa async/await và công thức tính thời gian chờ cấp số nhân.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l31-fill-1",
                        title = "Tính delay Exponential Backoff",
                        category = "react",
                        language = "javascript",
                        description = "Điền công thức cấp số nhân và hàm tạo Promise delay.",
                        practicalScenario = "Kết nối lại websocket khi bị ngắt kết nối mạng tạm thời.",
                        codeTemplate = new[]
                        {
                            "async function waitBackoff(attempt, baseDelay) {",
                            "  const delay = baseDelay * Math.{{BLANK_0}}(2, attempt);",
                            "  await new {{BLANK_1}}(resolve => setTimeout(resolve, delay));",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "pow", acceptableAnswers = new[] { "pow" }, hint = "Phương thức tính lũy thừa 2^attempt trong Math", chips = new[] { "pow", "exp", "max" }, explanation = "Math.pow(2, attempt) tăng thời gian chờ gấp đôi sau mỗi lần thử lại." },
                            new { id = "b1", blankIndex = 1, expected = "Promise", acceptableAnswers = new[] { "Promise" }, hint = "Đối tượng biểu diễn tác vụ bất đồng bộ", chips = new[] { "Promise", "Function", "Timeout" }, explanation = "new Promise(resolve => ...) tạo Promise để dùng với await." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 2, codeSnippet = "baseDelay * Math.pow(2, attempt)", purpose = "Tăng thời gian chờ theo cấp số nhân.", riskIfWrong = "Nếu cộng tuyến tính sẽ gây quá tải máy chủ khi mạng chập chờn." }
                        }
                    },
                    new
                    {
                        id = "code-l31-fill-2",
                        title = "Đếm số task hoàn thành với Promise.allSettled",
                        category = "react",
                        language = "javascript",
                        description = "Điền phương thức tổng hợp Promise an toàn không ném lỗi.",
                        practicalScenario = "Tải song song 10 ảnh đại diện, hiển thị ảnh tải thành công và bỏ qua ảnh lỗi.",
                        codeTemplate = new[]
                        {
                            "async function countSuccessful(promises) {",
                            "  const results = await Promise.{{BLANK_0}}(promises);",
                            "  return results.filter(r => r.status === '{{BLANK_1}}').length;",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "allSettled", acceptableAnswers = new[] { "allSettled" }, hint = "Phương thức chờ toàn bộ Promise hoàn tất bất kể thành công hay thất bại", chips = new[] { "allSettled", "all", "race", "any" }, explanation = "Promise.allSettled() không bị từ chối giữa chừng khi có 1 task lỗi." },
                            new { id = "b1", blankIndex = 1, expected = "fulfilled", acceptableAnswers = new[] { "fulfilled" }, hint = "Trạng thái thành công của Promise trong allSettled", chips = new[] { "fulfilled", "resolved", "success", "done" }, explanation = "Khi thành công, thuộc tính status của phần tử là 'fulfilled'." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 2, codeSnippet = "Promise.allSettled(promises)", purpose = "Chờ toàn bộ tác vụ giải quyết xong.", riskIfWrong = "Dùng Promise.all sẽ bị dừng ngay lập tức nếu có 1 task lỗi." }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 3.1 parsons (OrderIndex 7)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l31, "parsons", 7),
            CourseId = courseId,
            LessonId = l31,
            OrderIndex = 7,
            Type = "parsons",
            Title = "Parsons: Cài Đặt Thuật Toán Retry Có Giới Hạn",
            Description = "Sắp xếp logic hàm retry tự phục hồi lỗi với vòng lặp và try...catch.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l31-par-1",
                        title = "Lắp ráp hàm Retry với vòng lặp for",
                        goal = "Sắp xếp các dòng mã hàm retryTask.",
                        lines = new object[]
                        {
                            new { id = "r1", text = "async function retryTask(task, maxRetries) {", correctOrder = 1 },
                            new { id = "r2", text = "  for (let i = 0; i < maxRetries; i++) {", correctOrder = 2 },
                            new { id = "r3", text = "    try {", correctOrder = 3 },
                            new { id = "r4", text = "      return await task();", correctOrder = 4 },
                            new { id = "r5", text = "    } catch (err) {", correctOrder = 5 },
                            new { id = "r6", text = "      if (i === maxRetries - 1) throw err;", correctOrder = 6 },
                            new { id = "r7", text = "    }", correctOrder = 7 },
                            new { id = "r8", text = "  }", correctOrder = 8 },
                            new { id = "r9", text = "}", correctOrder = 9 }
                        },
                        explanation = "Thử thực thi task; nếu ném lỗi và chưa chạm ngưỡng maxRetries thì tiếp tục vòng lặp; lần cuối cùng ném lỗi ra ngoài."
                    },
                    new
                    {
                        id = "code-l31-par-2",
                        title = "Lắp ráp hàm timeout wrapper cho Promise",
                        goal = "Sắp xếp Promise.race tạo timeout.",
                        lines = new object[]
                        {
                            new { id = "w1", text = "function withTimeout(promise, ms) {", correctOrder = 1 },
                            new { id = "w2", text = "  const timeout = new Promise((_, reject) => {", correctOrder = 2 },
                            new { id = "w3", text = "    setTimeout(() => reject(new Error('Timeout')), ms);", correctOrder = 3 },
                            new { id = "w4", text = "  });", correctOrder = 4 },
                            new { id = "w5", text = "  return Promise.race([promise, timeout]);", correctOrder = 5 },
                            new { id = "w6", text = "}", correctOrder = 6 }
                        },
                        explanation = "Promise.race nhận task và timeout timer; cái nào xong trước sẽ quyết định kết quả."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // =========================================================================
        // LESSON 3.2: Deep Clone, Tính Bất Biến & Xử Lý Lỗi Toàn Diện
        // =========================================================================
        var l32 = SeedDataInitializer.CodingLesson32Id;

        // 3.2 js_sandbox (OrderIndex 3)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "js_sandbox", 3),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 3,
            Type = "js_sandbox",
            Title = "JS Sandbox: Viết Hàm Deep Equal So Sánh Hai Đối Tượng Lồng Nhau",
            Description = "Viết hàm deepEqual(a, b) so sánh giá trị đệ quy của 2 đối tượng hoặc mảng bất kỳ.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l32-js-deq",
                        title = "So sánh bằng sâu đệ quy giữa hai đối tượng",
                        difficulty = "Nâng cao",
                        description = "Viết hàm deepEqual(a, b) trả về true nếu hai giá trị có cùng cấu trúc và dữ liệu.",
                        functionName = "deepEqual",
                        starterCode = "function deepEqual(a, b) {\n  if (a === b) return true;\n  if (a == null || typeof a !== 'object' || b == null || typeof b !== 'object') return false;\n  const keysA = Object.keys(a);\n  const keysB = Object.keys(b);\n  if (keysA.length !== keysB.length) return false;\n  for (const key of keysA) {\n    if (!keysB.includes(key) || !deepEqual(a[key], b[key])) return false;\n  }\n  return true;\n}",
                        solutionCode = "function deepEqual(a, b) {\n  if (a === b) return true;\n  if (a == null || typeof a !== 'object' || b == null || typeof b !== 'object') return false;\n  const keysA = Object.keys(a);\n  const keysB = Object.keys(b);\n  if (keysA.length !== keysB.length) return false;\n  for (const key of keysA) {\n    if (!keysB.includes(key) || !deepEqual(a[key], b[key])) return false;\n  }\n  return true;\n}",
                        hint = "Nếu tham chiếu bằng nhau trả về true. Nếu kiểu dữ liệu khác nhau hoặc không phải object thì false. So sánh độ dài keys và gọi đệ quy.",
                        testCases = new object[]
                        {
                            new { name = "Hai object lồng nhau giống hệt", input = new object[] { new { x = 1, y = new { z = 2 } }, new { x = 1, y = new { z = 2 } } }, expected = true },
                            new { name = "Hai object có giá trị con khác nhau", input = new object[] { new { a = 1 }, new { a = 2 } }, expected = false }
                        }
                    },
                    new
                    {
                        id = "code-l32-js-freeze",
                        title = "Đóng băng sâu đối tượng (Deep Freeze)",
                        difficulty = "Trung bình",
                        description = "Viết hàm deepFreeze(obj) đệ quy gọi Object.freeze() lên mọi tầng con của đối tượng.",
                        functionName = "deepFreeze",
                        starterCode = "function deepFreeze(obj) {\n  if (obj === null || typeof obj !== 'object') return obj;\n  Object.freeze(obj);\n  for (const key of Object.keys(obj)) {\n    if (typeof obj[key] === 'object' && !Object.isFrozen(obj[key])) {\n      deepFreeze(obj[key]);\n    }\n  }\n  return obj;\n}",
                        solutionCode = "function deepFreeze(obj) {\n  if (obj === null || typeof obj !== 'object') return obj;\n  Object.freeze(obj);\n  for (const key of Object.keys(obj)) {\n    if (typeof obj[key] === 'object' && !Object.isFrozen(obj[key])) {\n      deepFreeze(obj[key]);\n    }\n  }\n  return obj;\n}",
                        hint = "Gọi Object.freeze(obj), sau đó lặp qua các thuộc tính là object con để gọi đệ quy.",
                        testCases = new object[]
                        {
                            new { name = "Đóng băng sâu thành công", input = new object[] { new { a = 1, b = new { c = 2 } } }, expected = "object" }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 3.2 predict_output (OrderIndex 4)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "predict_output", 4),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 4,
            Type = "predict_output",
            Title = "Đoán Output: Shallow Copy vs Deep Clone Đột Biến Bộ Nhớ",
            Description = "Dự đoán giá trị biến sau khi sao chép nông và sao chép sâu.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l32-pred-1",
                        difficulty = "Dễ",
                        title = "Đột biến mảng lồng nhau qua spread operator",
                        code = "const a = [[1], [2]];\nconst b = [...a];\nb[0].push(99);\nconsole.log(a[0].length);\nconsole.log(b[0].length);",
                        expectedOutput = "2\n2",
                        hint = "Toán tử spread [...] chỉ sao chép nông tầng ngoài. Phần tử a[0] và b[0] cùng trỏ tới một mảng trong heap.",
                        explanation = "Vì b[0] và a[0] là cùng một mảng, b[0].push(99) khiến mảng gốc a[0] cũng có độ dài tăng lên 2."
                    },
                    new
                    {
                        id = "code-l32-pred-2",
                        difficulty = "Trung bình",
                        title = "structuredClone bảo toàn tính độc lập của đối tượng lồng nhau",
                        code = "const orig = { meta: { tag: 'old' } };\nconst cloned = structuredClone(orig);\ncloned.meta.tag = 'new';\nconsole.log(orig.meta.tag);",
                        expectedOutput = "old",
                        hint = "structuredClone tạo bản sao sâu độc lập hoàn toàn, sửa cloned.meta không ảnh hưởng orig.meta.",
                        explanation = "orig.meta.tag vẫn giữ nguyên giá trị 'old' ban đầu."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 3.2 bug_fix (OrderIndex 5)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "bug_fix", 5),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 5,
            Type = "bug_fix",
            Title = "Sửa Bug: Lỗi Đệ Quy Vô Hạn Khi Deep Clone Có Vòng Lặp Tham Chiếu",
            Description = "Sửa hàm deepClone đệ quy để không bị văng RangeError: Maximum call stack size exceeded khi đối tượng tự trỏ vào chính nó.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l32-bug-1",
                        title = "Sử dụng WeakMap phòng chống tràn call stack khi có circular reference",
                        language = "javascript",
                        categoryTag = "Deep Clone Circular Bug",
                        context = "Hàm deepClone(obj) gọi đệ quy không có bộ nhớ tạm, crash ngay khi obj.self = obj.",
                        buggyCode = "function deepClone(obj) {\n  if (obj === null || typeof obj !== 'object') return obj;\n  const res = Array.isArray(obj) ? [] : {};\n  // BUG: Không kiểm tra xem obj đã được clone trước đó chưa!\n  for (const k of Object.keys(obj)) {\n    res[k] = deepClone(obj[k]);\n  }\n  return res;\n}",
                        bugLineNumber = 5,
                        cleanFixedCode = "function deepClone(obj, seen = new WeakMap()) {\n  if (obj === null || typeof obj !== 'object') return obj;\n  if (seen.has(obj)) return seen.get(obj);\n  const res = Array.isArray(obj) ? [] : {};\n  seen.set(obj, res);\n  for (const k of Object.keys(obj)) {\n    res[k] = deepClone(obj[k], seen);\n  }\n  return res;\n}",
                        requiredTokens = new[] { "seen", "WeakMap", "seen.has(obj)" },
                        functionName = "deepClone",
                        testCases = new object[]
                        {
                            new { name = "Clone object bình thường", input = new object[] { new { a = 1, b = new { c = 2 } } }, expected = "object" }
                        },
                        explanation = "Truyền `seen = new WeakMap()` theo hàm đệ quy để phát hiện đối tượng đã từng duyệt qua, trả về bản sao cũ tránh lặp vô hạn."
                    },
                    new
                    {
                        id = "code-l32-bug-2",
                        title = "Sửa lỗi JSON.stringify làm mất kiểu dữ liệu Date",
                        language = "javascript",
                        categoryTag = "JSON Clone Bug",
                        context = "Hàm cloneEvent(evt) dùng JSON parse/stringify khiến thuộc tính createdAt bị biến thành string thay vì Date.",
                        buggyCode = "function cloneEvent(evt) {\n  // BUG: Date biến thành string!\n  return JSON.parse(JSON.stringify(evt));\n}",
                        bugLineNumber = 2,
                        cleanFixedCode = "function cloneEvent(evt) {\n  return structuredClone(evt);\n}",
                        requiredTokens = new[] { "structuredClone" },
                        functionName = "cloneEvent",
                        testCases = new object[]
                        {
                            new { name = "Clone bảo toàn đối tượng", input = new object[] { new { id = 1, name = "Launch" } }, expected = "object" }
                        },
                        explanation = "Dùng `structuredClone()` có sẵn trong JavaScript hiện đại để bảo toàn nguyên vẹn kiểu dữ liệu Date, RegExp, Map, Set."
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 3.2 fill_code (OrderIndex 6)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "fill_code", 6),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 6,
            Type = "fill_code",
            Title = "Điền Code: Custom Error Class Kế Thừa Chuẩn Trong TypeScript/JS",
            Description = "Điền lệnh kế thừa và gán tên lớp lỗi chuyên biệt.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l32-fill-1",
                        title = "Tạo Custom Error Class kế thừa Error",
                        category = "react",
                        language = "javascript",
                        description = "Hoàn thiện lớp lỗi ValidationError kế thừa chuẩn từ Error.",
                        practicalScenario = "Phân biệt lỗi nhập liệu người dùng với lỗi sập máy chủ 500 trong ứng dụng.",
                        codeTemplate = new[]
                        {
                            "class ValidationError {{BLANK_0}} Error {",
                            "  constructor(message, field) {",
                            "    {{BLANK_1}}(message);",
                            "    this.name = 'ValidationError';",
                            "    this.field = field;",
                            "  }",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "extends", acceptableAnswers = new[] { "extends" }, hint = "Từ khóa kế thừa lớp trong ES6", chips = new[] { "extends", "implements", "inherits" }, explanation = "class Child extends Parent là cú pháp kế thừa chuẩn trong JavaScript." },
                            new { id = "b1", blankIndex = 1, expected = "super", acceptableAnswers = new[] { "super" }, hint = "Gọi hàm khởi tạo của lớp cha Error", chips = new[] { "super", "this", "parent" }, explanation = "super(message) truyền thông báo lỗi lên hàm khởi tạo của Error cha." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 3, codeSnippet = "super(message);", purpose = "Khởi tạo stack trace và message từ Error cha.", riskIfWrong = "Nếu thiếu sẽ ném lỗi ReferenceError khi dùng 'this'." }
                        }
                    },
                    new
                    {
                        id = "code-l32-fill-2",
                        title = "Bắt lỗi theo kiểu cụ thể với instanceof",
                        category = "react",
                        language = "javascript",
                        description = "Điền toán tử kiểm tra kiểu instance của error.",
                        practicalScenario = "Xử lý riêng lỗi ValidationError trả về 400 và các lỗi khác trả về 500.",
                        codeTemplate = new[]
                        {
                            "try {",
                            "  validateUser(user);",
                            "} catch (err) {",
                            "  if (err {{BLANK_0}} ValidationError) {",
                            "    console.warn('Lỗi nhập liệu:', err.field);",
                            "  } else {",
                            "    {{BLANK_1}} err;",
                            "  }",
                            "}"
                        },
                        blanks = new object[]
                        {
                            new { id = "b0", blankIndex = 0, expected = "instanceof", acceptableAnswers = new[] { "instanceof" }, hint = "Toán tử kiểm tra một đối tượng có thuộc về một Class hay không", chips = new[] { "instanceof", "typeof", "is" }, explanation = "err instanceof ValidationError kiểm tra chuỗi prototype của đối tượng lỗi." },
                            new { id = "b1", blankIndex = 1, expected = "throw", acceptableAnswers = new[] { "throw" }, hint = "Ném lại lỗi không xử lý được ra ngoài", chips = new[] { "throw", "return", "catch" }, explanation = "throw err giúp lỗi hệ thống không bị nuốt âm thầm." }
                        },
                        lineExplanations = new object[]
                        {
                            new { lineNumber = 4, codeSnippet = "if (err instanceof ValidationError)", purpose = "Phân loại ngoại lệ nghiệp vụ.", riskIfWrong = "Dùng typeof err sẽ luôn trả về 'object' không phân biệt được." }
                        }
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 3.2 parsons (OrderIndex 7)
        list.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, l32, "parsons", 7),
            CourseId = courseId,
            LessonId = l32,
            OrderIndex = 7,
            Type = "parsons",
            Title = "Parsons: Cài Đặt Hàm Deep Clone Đệ Quy An Toàn",
            Description = "Sắp xếp từng dòng mã để tạo hàm deepClone đệ quy hỗ trợ mảng và object.",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = "code-l32-par-1",
                        title = "Lắp ráp hàm Deep Clone đệ quy cơ bản",
                        goal = "Sắp xếp theo thứ tự điều kiện dừng đệ quy và duyệt key.",
                        lines = new object[]
                        {
                            new { id = "c1", text = "function deepClone(value) {", correctOrder = 1 },
                            new { id = "c2", text = "  if (value === null || typeof value !== 'object') return value;", correctOrder = 2 },
                            new { id = "c3", text = "  const copy = Array.isArray(value) ? [] : {};", correctOrder = 3 },
                            new { id = "c4", text = "  for (const key of Object.keys(value)) {", correctOrder = 4 },
                            new { id = "c5", text = "    copy[key] = deepClone(value[key]);", correctOrder = 5 },
                            new { id = "c6", text = "  }", correctOrder = 6 },
                            new { id = "c7", text = "  return copy;", correctOrder = 7 },
                            new { id = "c8", text = "}", correctOrder = 8 }
                        },
                        explanation = "Nếu là kiểu nguyên thủy (số, chuỗi, null) thì trả về ngay; nếu là object/array thì khởi tạo rỗng và gọi đệ quy từng key."
                    },
                    new
                    {
                        id = "code-l32-par-2",
                        title = "Lắp ráp bộ xử lý lỗi try...catch...finally",
                        goal = "Sắp xếp luồng xử lý và giải phóng tài nguyên trong finally.",
                        lines = new object[]
                        {
                            new { id = "e1", text = "async function handleTransaction(db, tx) {", correctOrder = 1 },
                            new { id = "e2", text = "  try {", correctOrder = 2 },
                            new { id = "e3", text = "    await db.beginTransaction();", correctOrder = 3 },
                            new { id = "e4", text = "    await tx();", correctOrder = 4 },
                            new { id = "e5", text = "    await db.commit();", correctOrder = 5 },
                            new { id = "e6", text = "  } catch (err) {", correctOrder = 6 },
                            new { id = "e7", text = "    await db.rollback();", correctOrder = 7 },
                            new { id = "e8", text = "    throw err;", correctOrder = 8 },
                            new { id = "e9", text = "  } finally {", correctOrder = 9 },
                            new { id = "e10", text = "    await db.releaseConnection();", correctOrder = 10 },
                            new { id = "e11", text = "  }", correctOrder = 11 },
                            new { id = "e12", text = "}", correctOrder = 12 }
                        },
                        explanation = "Mở giao dịch trong try, rollback trong catch và giải phóng kết nối database trong finally."
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
