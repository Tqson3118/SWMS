namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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

        // =========================================================================
        // BEGINNER WARM-UP ACTIVITIES (OrderIndex 10+ / 13+ for every lesson 1.1..3.2)
        // Helps complete beginners write their first lines of JS with step-by-step hints.
        // =========================================================================

        // 1.1 Beginner Warm-up: js_sandbox (OrderIndex 13) & fill_code (OrderIndex 14)
        list.Add(CreateBeginnerJsSandbox(
            courseId, l11, 13,
            "Khởi Động JS: Nhân Đôi Các Số Trong Mảng Với Map",
            "Làm quen hàm mũi tên và phương thức .map() bằng bài toán nhân đôi mọi phần tử trong mảng số.",
            "code-l11-warmup-js-double",
            "Khởi động: Nhân đôi từng số trong mảng",
            "Viết hàm doubleNumbers(nums) nhận vào mảng số nguyên và trả về mảng mới trong đó mỗi số được nhân với 2 (không làm thay đổi mảng gốc).",
            "doubleNumbers",
            "function doubleNumbers(nums) {\n  // TODO Bước 1: Gọi nums.map(...)\n  // TODO Bước 2: Với mỗi số n, trả về n * 2\n  \n}",
            "function doubleNumbers(nums) {\n  return nums.map(n => n * 2);\n}",
            "Bước 1: Dùng `return nums.map((n) => ...);`. Bước 2: Bên trong hàm mũi tên trả về `n * 2`. Phương thức `map` tự xử lý cả mảng rỗng `[]`.",
            new object[]
            {
                new { name = "Mảng dương cơ bản [1, 2, 3]", input = new object[] { new[] { 1, 2, 3 } }, expected = new[] { 2, 4, 6 } },
                new { name = "Có số 0 và số âm [0, -5, 10]", input = new object[] { new[] { 0, -5, 10 } }, expected = new[] { 0, -10, 20 } },
                new { name = "Mảng rỗng []", input = new object[] { Array.Empty<int>() }, expected = Array.Empty<int>() }
            },
            now));

        list.Add(CreateBeginnerFillCode(
            courseId, l11, 14,
            "Khởi Động Điền Code: Lọc Các Số Lớn Hơn 10 Bằng Filter",
            "Điền phương thức .filter() và điều kiện so sánh để giữ lại các số lớn hơn 10.",
            "code-l11-warmup-fill-filter10",
            "Khởi động: Lọc số lớn hơn 10",
            "Hoàn thiện hàm filterGreaterThanTen(nums) trả về mảng các số > 10.",
            "Lọc danh sách điểm thưởng hoặc nhiệt độ vượt ngưỡng an toàn > 10.",
            new[]
            {
                "function filterGreaterThanTen(nums) {",
                "  return nums.{{BLANK_0}}(n => {{BLANK_1}});",
                "}"
            },
            new object[]
            {
                new { id = "b0", blankIndex = 0, expected = "filter", acceptableAnswers = new[] { "filter" }, hint = "Phương thức giữ lại các phần tử thỏa điều kiện đúng/sai", chips = new[] { "filter", "map", "reduce", "forEach" }, explanation = "filter() tạo mảng mới chỉ gồm các phần tử làm callback trả về true." },
                new { id = "b1", blankIndex = 1, expected = "n > 10", acceptableAnswers = new[] { "n > 10", "n>10" }, hint = "Biểu thức kiểm tra n lớn hơn 10", chips = new[] { "n > 10", "n >= 10", "n < 10", "n * 2" }, explanation = "Chỉ giữ những số lớn hơn hẳn 10 (không lấy bằng 10)." }
            },
            new object[]
            {
                new { lineNumber = 2, codeSnippet = "return nums.filter(n => n > 10);", purpose = "Lọc và trả về mảng mới các số > 10.", riskIfWrong = "Dùng map() sẽ trả về mảng toàn true/false thay vì giữ lại giá trị số." }
            },
            now));

        // 1.2 Beginner Warm-up: js_sandbox (OrderIndex 10) & fill_code (OrderIndex 11)
        list.Add(CreateBeginnerJsSandbox(
            courseId, l12, 10,
            "Khởi Động JS: Tìm Vị Trí Phần Tử Đầu Tiên Trong Mảng",
            "Làm quen chỉ số mảng 0, 1, 2 và vòng lặp for để tìm vị trí phần tử trước khi sang Binary Search.",
            "code-l12-warmup-js-linear",
            "Khởi động: Tìm kiếm tuyến tính theo chỉ số",
            "Viết hàm linearFindIndex(arr, target) duyệt từ đầu mảng và trả về chỉ số (index) đầu tiên mà arr[i] === target. Nếu không tìm thấy, trả về -1.",
            "linearFindIndex",
            "function linearFindIndex(arr, target) {\n  // TODO Bước 1: Chạy vòng lặp for (let i = 0; i < arr.length; i++)\n  // TODO Bước 2: Nếu arr[i] === target thì return i\n  // TODO Bước 3: Hết vòng lặp không thấy thì return -1\n  \n}",
            "function linearFindIndex(arr, target) {\n  for (let i = 0; i < arr.length; i++) {\n    if (arr[i] === target) return i;\n  }\n  return -1;\n}",
            "Bước 1: Dùng vòng lặp `for (let i = 0; i < arr.length; i++)`. Bước 2: Kiểm tra `if (arr[i] === target) return i;`. Bước 3: Sau vòng lặp ghi `return -1;`.",
            new object[]
            {
                new { name = "Tìm thấy ở giữa [10, 20, 30], target = 20", input = new object[] { new[] { 10, 20, 30 }, 20 }, expected = 1 },
                new { name = "Không tồn tại [5, 6], target = 9", input = new object[] { new[] { 5, 6 }, 9 }, expected = -1 },
                new { name = "Mảng rỗng [], target = 1", input = new object[] { Array.Empty<int>(), 1 }, expected = -1 }
            },
            now));

        list.Add(CreateBeginnerFillCode(
            courseId, l12, 11,
            "Khởi Động Điền Code: Khởi Tạo Hai Con Trỏ & Tính Chỉ Số Giữa",
            "Điền giá trị ban đầu cho hai con trỏ left, right và công thức làm tròn xuống để tính vị trí giữa mid.",
            "code-l12-warmup-fill-pointers",
            "Khởi động: Đặt hai con trỏ đầu - cuối và tính mid",
            "Hoàn thành hàm getMiddleValue(arr) đặt 2 đầu mút mảng và trả về phần tử ở giữa.",
            "Làm quen khái niệm left = 0, right = arr.length - 1 và Math.floor trước khi viết Binary Search.",
            new[]
            {
                "function getMiddleValue(arr) {",
                "  if (arr.length === 0) return null;",
                "  const left = {{BLANK_0}};",
                "  const right = arr.length - {{BLANK_1}};",
                "  const mid = Math.{{BLANK_2}}((left + right) / 2);",
                "  return arr[mid];",
                "}"
            },
            new object[]
            {
                new { id = "b0", blankIndex = 0, expected = "0", acceptableAnswers = new[] { "0" }, hint = "Chỉ số đầu tiên của mọi mảng trong JS", chips = new[] { "0", "1", "-1" }, explanation = "Mảng trong JS luôn đánh số từ 0." },
                new { id = "b1", blankIndex = 1, expected = "1", acceptableAnswers = new[] { "1" }, hint = "Chỉ số cuối bằng độ dài mảng trừ mấy?", chips = new[] { "1", "0", "2" }, explanation = "Vì bắt đầu từ 0 nên phần tử cuối nằm ở vị trí arr.length - 1." },
                new { id = "b2", blankIndex = 2, expected = "floor", acceptableAnswers = new[] { "floor" }, hint = "Hàm làm tròn xuống số nguyên trong đối tượng Math", chips = new[] { "floor", "round", "ceil" }, explanation = "Math.floor() làm tròn xuống số nguyên để làm chỉ số mảng hợp lệ." }
            },
            new object[]
            {
                new { lineNumber = 4, codeSnippet = "const right = arr.length - 1;", purpose = "Tránh lỗi truy cập vượt biên (undefined).", riskIfWrong = "Nếu lấy arr.length sẽ trỏ ra ngoài mảng." }
            },
            now));

        // 2.1 Beginner Warm-up: js_sandbox (OrderIndex 10) & fill_code (OrderIndex 11)
        list.Add(CreateBeginnerJsSandbox(
            courseId, l21, 10,
            "Khởi Động JS: Tạo Bộ Đếm Bước Bằng Closure",
            "Làm quen cách hàm con bên trong ghi nhớ biến của hàm ngoài (Closure) trước khi xây dựng Debounce/Throttle.",
            "code-l21-warmup-js-closure",
            "Khởi động: Đếm số lần gọi bằng Closure",
            "Viết hàm simulateClosureCounter(steps) tạo biến let count = 0 và một hàm con next() mỗi lần gọi tăng count lên 1 rồi trả về count; gọi next() đúng steps lần và trả về mảng kết quả.",
            "simulateClosureCounter",
            "function simulateClosureCounter(steps) {\n  let count = 0;\n  function next() {\n    // TODO Bước 1: Tăng count lên 1 và return count\n  }\n  const log = [];\n  // TODO Bước 2: Dùng vòng lặp for (let i = 0; i < steps; i++) đẩy next() vào log\n  return log;\n}",
            "function simulateClosureCounter(steps) {\n  let count = 0;\n  function next() {\n    count += 1;\n    return count;\n  }\n  const log = [];\n  for (let i = 0; i < steps; i++) {\n    log.push(next());\n  }\n  return log;\n}",
            "Bước 1: Trong `next()`, viết `count += 1; return count;`. Bước 2: Trong vòng `for (let i = 0; i < steps; i++)`, gọi `log.push(next());`.",
            new object[]
            {
                new { name = "Gọi 3 lần -> [1, 2, 3]", input = new object[] { 3 }, expected = new[] { 1, 2, 3 } },
                new { name = "Gọi 1 lần -> [1]", input = new object[] { 1 }, expected = new[] { 1 } },
                new { name = "0 lần -> []", input = new object[] { 0 }, expected = Array.Empty<int>() }
            },
            now));

        list.Add(CreateBeginnerFillCode(
            courseId, l21, 11,
            "Khởi Động Điền Code: Đặt & Hủy Đồng Hồ Hẹn Giờ setTimeout",
            "Làm quen hai hàm setTimeout và clearTimeout quản lý hẹn giờ trong JavaScript.",
            "code-l21-warmup-fill-timer",
            "Khởi động: Hủy hẹn giờ cũ trước khi đặt hẹn giờ mới",
            "Hoàn thiện hàm scheduleSave(oldTimerId, callback) để hủy timer cũ nếu có rồi hẹn giờ chạy sau 300ms.",
            "Tránh gửi nhiều lệnh lưu nháp chồng chéo khi người dùng gõ liên tiếp.",
            new[]
            {
                "function scheduleSave(oldTimerId, callback) {",
                "  if (oldTimerId !== null) {",
                "    {{BLANK_0}}(oldTimerId);",
                "  }",
                "  return {{BLANK_1}}(callback, 300);",
                "}"
            },
            new object[]
            {
                new { id = "b0", blankIndex = 0, expected = "clearTimeout", acceptableAnswers = new[] { "clearTimeout" }, hint = "Lệnh tắt đồng hồ hẹn giờ cũ", chips = new[] { "clearTimeout", "setTimeout", "stopTimer" }, explanation = "clearTimeout(id) hủy hẹn giờ chưa kịp chạy." },
                new { id = "b1", blankIndex = 1, expected = "setTimeout", acceptableAnswers = new[] { "setTimeout" }, hint = "Lệnh tạo đồng hồ hẹn giờ mới", chips = new[] { "setTimeout", "clearTimeout", "setInterval" }, explanation = "setTimeout(callback, 300) lên lịch chạy callback 1 lần sau 300ms." }
            },
            new object[]
            {
                new { lineNumber = 3, codeSnippet = "clearTimeout(oldTimerId);", purpose = "Hủy bỏ lần hẹn giờ trước đó.", riskIfWrong = "Không hủy thì hàm callback cũ vẫn chạy gây dư thừa request." }
            },
            now));

        // 2.2 Beginner Warm-up: js_sandbox (OrderIndex 10) & fill_code (OrderIndex 11)
        list.Add(CreateBeginnerJsSandbox(
            courseId, l22, 10,
            "Khởi Động JS: Tra Cứu & Lưu Kết Quả Bình Phương Trong Cache",
            "Thực hành cơ chế Cache Hit / Cache Miss đơn giản trên object trước khi viết hàm memoize tổng quát.",
            "code-l22-warmup-js-cache",
            "Khởi động: Kiểm tra Cache Hit hay Cache Miss",
            "Viết hàm getOrComputeSquare(cacheObj, n): nếu khóa String(n) đã tồn tại trong cacheObj (dùng Object.prototype.hasOwnProperty.call), trả về { hit: true, value: cacheObj[String(n)] }; nếu chưa có, tính n * n, lưu vào cacheObj[String(n)] và trả về { hit: false, value: n * n }.",
            "getOrComputeSquare",
            "function getOrComputeSquare(cacheObj, n) {\n  const key = String(n);\n  // TODO Bước 1: Kiểm tra Object.prototype.hasOwnProperty.call(cacheObj, key)\n  // TODO Bước 2: Nếu có -> return { hit: true, value: cacheObj[key] }\n  // TODO Bước 3: Nếu chưa -> tính val = n * n, gán cacheObj[key] = val, return { hit: false, value: val }\n  \n}",
            "function getOrComputeSquare(cacheObj, n) {\n  const key = String(n);\n  if (Object.prototype.hasOwnProperty.call(cacheObj, key)) {\n    return { hit: true, value: cacheObj[key] };\n  }\n  const val = n * n;\n  cacheObj[key] = val;\n  return { hit: false, value: val };\n}",
            "Bước 1: Dùng `Object.prototype.hasOwnProperty.call(cacheObj, key)` (an toàn kể cả khi value là `0`). Bước 2: Trả `{ hit: true, value: cacheObj[key] }`. Bước 3: Tính `n * n`, lưu vào `cacheObj[key]` và trả `{ hit: false, value: val }`.",
            new object[]
            {
                new { name = "Cache Hit với n = 4", input = new object[] { new Dictionary<string, int> { { "4", 16 } }, 4 }, expected = new { hit = true, value = 16 } },
                new { name = "Cache Miss với n = 5", input = new object[] { new Dictionary<string, int>(), 5 }, expected = new { hit = false, value = 25 } },
                new { name = "Cache Hit với giá trị falsy 0", input = new object[] { new Dictionary<string, int> { { "0", 0 } }, 0 }, expected = new { hit = true, value = 0 } }
            },
            now));

        list.Add(CreateBeginnerFillCode(
            courseId, l22, 11,
            "Khởi Động Điền Code: Ba Thao Tác Cơ Bản Với Map (has, get, set)",
            "Điền 3 phương thức cơ bản nhất của đối tượng Map để tra cứu và lưu điểm học viên.",
            "code-l22-warmup-fill-map",
            "Khởi động: Thao tác has / get / set trên Map",
            "Hoàn thiện hàm getOrInsertDouble(cache, n) dùng Map.",
            "Làm quen cú pháp Map trước khi viết hàm memoize đa tham số.",
            new[]
            {
                "function getOrInsertDouble(cache, n) {",
                "  if (cache.{{BLANK_0}}(n)) {",
                "    return cache.{{BLANK_1}}(n);",
                "  }",
                "  const val = n * 2;",
                "  cache.{{BLANK_2}}(n, val);",
                "  return val;",
                "}"
            },
            new object[]
            {
                new { id = "b0", blankIndex = 0, expected = "has", acceptableAnswers = new[] { "has" }, hint = "Kiểm tra khóa đã có trong Map chưa", chips = new[] { "has", "includes", "exists" }, explanation = "Map.prototype.has(key) trả về boolean ngay cả khi value là 0." },
                new { id = "b1", blankIndex = 1, expected = "get", acceptableAnswers = new[] { "get" }, hint = "Lấy giá trị ứng với khóa n trong Map", chips = new[] { "get", "read", "find" }, explanation = "Map.prototype.get(key) đọc giá trị đã lưu." },
                new { id = "b2", blankIndex = 2, expected = "set", acceptableAnswers = new[] { "set" }, hint = "Ghi cặp khóa-giá trị mới vào Map", chips = new[] { "set", "put", "add" }, explanation = "Map.prototype.set(key, val) lưu cặp khóa-giá trị vào Map." }
            },
            new object[]
            {
                new { lineNumber = 2, codeSnippet = "if (cache.has(n))", purpose = "Phát hiện Cache Hit chính xác kể cả giá trị 0.", riskIfWrong = "Dùng includes sẽ lỗi vì Map không có hàm includes." }
            },
            now));

        // 3.1 Beginner Warm-up: js_sandbox (OrderIndex 10) & fill_code (OrderIndex 11)
        list.Add(CreateBeginnerJsSandbox(
            courseId, l31, 10,
            "Khởi Động JS: Tổng Hợp Trạng Thái Promise.allSettled",
            "Làm quen cấu trúc kết quả của Promise.allSettled bằng cách đếm số tác vụ thành công (fulfilled) và thất bại (rejected).",
            "code-l31-warmup-js-settled",
            "Khởi động: Đếm kết quả fulfilled và rejected",
            "Viết hàm summarizeSettledResults(results) nhận mảng đối tượng [{ status: 'fulfilled' | 'rejected' }] và trả về object { fulfilled: number, rejected: number }.",
            "summarizeSettledResults",
            "function summarizeSettledResults(results) {\n  let fulfilled = 0;\n  let rejected = 0;\n  // TODO: Duyệt từng item trong results, kiểm tra item.status === 'fulfilled' hoặc 'rejected'\n  return { fulfilled, rejected };\n}",
            "function summarizeSettledResults(results) {\n  let fulfilled = 0;\n  let rejected = 0;\n  for (const item of results) {\n    if (item.status === 'fulfilled') fulfilled++;\n    else if (item.status === 'rejected') rejected++;\n  }\n  return { fulfilled, rejected };\n}",
            "Dùng vòng lặp `for (const item of results)`: nếu `item.status === 'fulfilled'` tăng `fulfilled++`; nếu `'rejected'` tăng `rejected++`.",
            new object[]
            {
                new { name = "Có 2 thành công và 1 thất bại", input = new object[] { new object[] { new { status = "fulfilled" }, new { status = "rejected" }, new { status = "fulfilled" } } }, expected = new { fulfilled = 2, rejected = 1 } },
                new { name = "Tất cả thất bại", input = new object[] { new object[] { new { status = "rejected" } } }, expected = new { fulfilled = 0, rejected = 1 } },
                new { name = "Mảng rỗng []", input = new object[] { Array.Empty<object>() }, expected = new { fulfilled = 0, rejected = 0 } }
            },
            now));

        list.Add(CreateBeginnerFillCode(
            courseId, l31, 11,
            "Khởi Động Điền Code: Bọc Lệnh Await An Toàn Với Try / Catch",
            "Điền từ khóa async, await và catch để gọi tác vụ bất đồng bộ mà không làm crash ứng dụng khi có lỗi.",
            "code-l31-warmup-fill-trycatch",
            "Khởi động: Dùng async/await kèm try...catch",
            "Hoàn thiện hàm loadProfileSafe(fetchFn) trả về null nếu lỗi.",
            "Ngăn ngừa lỗi Unhandled Promise Rejection khi mạng bị ngắt đột ngột.",
            new[]
            {
                "{{BLANK_0}} function loadProfileSafe(fetchFn) {",
                "  try {",
                "    const data = {{BLANK_1}} fetchFn();",
                "    return data;",
                "  } {{BLANK_2}} (err) {",
                "    return null;",
                "  }",
                "}"
            },
            new object[]
            {
                new { id = "b0", blankIndex = 0, expected = "async", acceptableAnswers = new[] { "async" }, hint = "Từ khóa đặt trước function để cho phép dùng await bên trong", chips = new[] { "async", "await", "promise" }, explanation = "Chỉ bên trong hàm khai báo `async` mới sử dụng được từ khóa `await`." },
                new { id = "b1", blankIndex = 1, expected = "await", acceptableAnswers = new[] { "await" }, hint = "Từ khóa tạm dừng chờ Promise hoàn tất", chips = new[] { "await", "async", "yield" }, explanation = "`await` chờ Promise giải quyết rồi trả về giá trị thực tế." },
                new { id = "b2", blankIndex = 2, expected = "catch", acceptableAnswers = new[] { "catch" }, hint = "Khối hứng lỗi khi Promise bị rejected", chips = new[] { "catch", "finally", "throw" }, explanation = "`catch (err)` bắt lỗi ném ra từ lệnh `await` bên trong `try`." }
            },
            new object[]
            {
                new { lineNumber = 3, codeSnippet = "const data = await fetchFn();", purpose = "Chờ kết quả bất đồng bộ trước khi return.", riskIfWrong = "Thiếu await thì data sẽ là đối tượng Promise thay vì dữ liệu thật." }
            },
            now));

        // 3.2 Beginner Warm-up: js_sandbox (OrderIndex 10) & fill_code (OrderIndex 11)
        list.Add(CreateBeginnerJsSandbox(
            courseId, l32, 10,
            "Khởi Động JS: Cập Nhật Object Bất Biến Bằng Spread Operator",
            "Làm quen sao chép bất biến 1 tầng bằng toán tử spread { ...obj } để không sửa nhầm dữ liệu gốc.",
            "code-l32-warmup-js-spread",
            "Khởi động: Tạo bản sao object với tuổi mới",
            "Viết hàm updateUserAge(user, newAge) trả về một object MỚI có toàn bộ thuộc tính của user nhưng cập nhật age thành newAge, giữ nguyên hoàn toàn object user gốc.",
            "updateUserAge",
            "function updateUserAge(user, newAge) {\n  // TODO: Trả về object mới dùng toán tử spread { ...user, age: newAge }\n  \n}",
            "function updateUserAge(user, newAge) {\n  return { ...user, age: newAge };\n}",
            "Không viết `user.age = newAge`! Hãy viết `return { ...user, age: newAge };` để tạo bản sao mới trong bộ nhớ.",
            new object[]
            {
                new { name = "Cập nhật tuổi của Minh từ 20 lên 21", input = new object[] { new { name = "Minh", age = 20 }, 21 }, expected = new { name = "Minh", age = 21 } },
                new { name = "Giữ nguyên thuộc tính khác như role", input = new object[] { new { name = "Lan", age = 18, role = "student" }, 19 }, expected = new { name = "Lan", age = 19, role = "student" } }
            },
            now));

        list.Add(CreateBeginnerFillCode(
            courseId, l32, 11,
            "Khởi Động Điền Code: Điều Kiện Dừng Đệ Quy An Toàn Với Null",
            "Điền kiểm tra null và typeof trong hàm clone đệ quy để tránh lỗi TypeError: Object.keys(null).",
            "code-l32-warmup-fill-nullguard",
            "Khởi động: Chặn null trước khi kiểm tra typeof === 'object'",
            "Hoàn thiện điều kiện dừng đệ quy của hàm simpleClone(val).",
            "Trong JavaScript, `typeof null` trả về `'object'`, nên bắt buộc phải kiểm tra `val === null` trước!",
            new[]
            {
                "function simpleClone(val) {",
                "  if (val === {{BLANK_0}} || {{BLANK_1}} val !== 'object') {",
                "    return val;",
                "  }",
                "  if (Array.isArray(val)) return val.map(simpleClone);",
                "  const copy = {};",
                "  for (const k of Object.keys(val)) copy[k] = simpleClone(val[k]);",
                "  return copy;",
                "}"
            },
            new object[]
            {
                new { id = "b0", blankIndex = 0, expected = "null", acceptableAnswers = new[] { "null" }, hint = "Giá trị đặc biệt có typeof bằng 'object' trong JS", chips = new[] { "null", "undefined", "false" }, explanation = "Phải chặn val === null ngay từ đầu vì typeof null === 'object'." },
                new { id = "b1", blankIndex = 1, expected = "typeof", acceptableAnswers = new[] { "typeof" }, hint = "Toán tử kiểm tra kiểu dữ liệu nguyên thủy", chips = new[] { "typeof", "instanceof", "type" }, explanation = "typeof val !== 'object' nhận diện số, chuỗi, boolean, undefined." }
            },
            new object[]
            {
                new { lineNumber = 2, codeSnippet = "if (val === null || typeof val !== 'object')", purpose = "Điều kiện dừng đệ quy an toàn cho mọi kiểu nguyên thủy và null.", riskIfWrong = "Nếu thiếu val === null, Object.keys(null) sẽ ném TypeError làm sập hàm." }
            },
            now));

        return list;
    }

    private static PracticeActivity CreateBeginnerJsSandbox(
        Guid courseId,
        Guid lessonId,
        int orderIndex,
        string title,
        string description,
        string itemId,
        string itemTitle,
        string itemDescription,
        string functionName,
        string starterCode,
        string solutionCode,
        string hint,
        object[] testCases,
        DateTimeOffset now)
    {
        return new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lessonId, "js_sandbox", orderIndex),
            CourseId = courseId,
            LessonId = lessonId,
            OrderIndex = orderIndex,
            Type = "js_sandbox",
            Title = title,
            Description = description,
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = itemId,
                        title = itemTitle,
                        difficulty = "Dễ",
                        description = itemDescription,
                        functionName,
                        starterCode,
                        solutionCode,
                        hint,
                        testCases
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static PracticeActivity CreateBeginnerFillCode(
        Guid courseId,
        Guid lessonId,
        int orderIndex,
        string title,
        string description,
        string itemId,
        string itemTitle,
        string itemDescription,
        string practicalScenario,
        string[] codeTemplate,
        object[] blanks,
        object[] lineExplanations,
        DateTimeOffset now)
    {
        return new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lessonId, "fill_code", orderIndex),
            CourseId = courseId,
            LessonId = lessonId,
            OrderIndex = orderIndex,
            Type = "fill_code",
            Title = title,
            Description = description,
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = itemId,
                        title = itemTitle,
                        category = "react",
                        language = "javascript",
                        description = itemDescription,
                        practicalScenario,
                        codeTemplate,
                        blanks,
                        lineExplanations
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        };
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
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"swms:coding-quiz:{key}"));
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
    /// Returns 6 lesson quizzes (CodingLesson11Id .. CodingLesson32Id) plus 1 comprehensive final course quiz
    /// attached to CodingLesson32Id. Each quiz contains 10 deterministic multiple_choice items
    /// (4 Basic + 4 Code Output/Bug + 2 Edge-Case/Complexity) with detailed AiRubricFeedbackJson explanations.
    /// </summary>
    public static List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)> GetCourseQuizzes(DateTimeOffset now)
    {
        return new List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)>
        {
            // 1. Quiz Bài 1.1: Map, Filter, Reduce & Tính Bất Biến
            BuildQuiz(
                "l11",
                SeedDataInitializer.CodingLesson11Id,
                "Kiểm tra Bài 1.1: Biến Đổi & Lọc Dữ Liệu (Map, Filter, Reduce)",
                now.AddSeconds(1),
                now,
                new[]
                {
                    // 4 Cơ bản
                    new QuizQuestionSpec(
                        "Câu 1 (Cơ bản): Chỉ số (index) của phần tử đầu tiên và phần tử cuối cùng trong mảng `const arr = [10, 20, 30, 40]` lần lượt là bao nhiêu?",
                        "1 và 4",
                        "0 và 3",
                        "0 và 4",
                        "1 và 3",
                        "B",
                        "Đáp án B đúng vì mảng JavaScript đánh chỉ số từ 0 đến arr.length - 1 (4 - 1 = 3). Đáp án A và D sai vì đếm từ 1; đáp án C sai vì arr[4] vượt biên và trả về undefined."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Cơ bản): Giá trị trả về của phương thức `Array.prototype.forEach()` là gì?",
                        "Một mảng mới có cùng độ dài với mảng gốc",
                        "Phần tử cuối cùng được duyệt qua",
                        "undefined",
                        "Số lượng phần tử đã duyệt",
                        "C",
                        "Đáp án C đúng vì forEach() chỉ dùng để thực thi hành động lặp và luôn trả về undefined. Đáp án A mô tả map(), còn B và D không đúng đặc tả ECMAScript."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Cơ bản): Khi biến đổi một mảng 5 phần tử bằng `.map()`, độ dài của mảng mới trả về sẽ là bao nhiêu?",
                        "Luôn bằng đúng 5 phần tử",
                        "Có thể nhỏ hơn 5 nếu gặp giá trị null",
                        "Bằng 1 giá trị tổng duy nhất",
                        "Thay đổi ngẫu nhiên tùy callback",
                        "A",
                        "Đáp án A đúng vì map() thực hiện ánh xạ 1-1 nên mảng kết quả luôn có độ dài bằng đúng mảng gốc. Đáp án B nhầm với filter(), đáp án C nhầm với reduce(), đáp án D là sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Cơ bản): Trong hai phương thức `.filter()` và `.find()`, phát biểu nào sau đây chính xác?",
                        "filter() trả về phần tử đầu tiên, find() trả về mảng",
                        "Cả hai đều sửa trực tiếp mảng gốc",
                        "filter() trả về boolean, find() trả về chỉ số index",
                        "filter() trả về mảng các phần tử khớp, find() chỉ trả về 1 phần tử đầu tiên khớp (hoặc undefined)",
                        "D",
                        "Đáp án D đúng: filter() luôn trả về mảng (kể cả rỗng []), còn find() trả về phần tử đầu tiên thỏa điều kiện hoặc undefined nếu không thấy. Đáp án A đảo ngược hai hàm; B sai vì cả hai đều bất biến; C nhầm với some() và findIndex()."
                    ),
                    // 4 Đọc code đoán output / Tìm lỗi sai
                    new QuizQuestionSpec(
                        "Câu 5 (Đọc code): Đoạn code JS sau in ra gì? `const a = [1, 2, 3, 4]; const b = a.filter(x => x % 2 === 0).map(x => x * 10); console.log(JSON.stringify(b), a.length);`",
                        "[20,40] 4",
                        "[10,20,30,40] 4",
                        "[20,40] 2",
                        "[2,4] 2",
                        "A",
                        "Đáp án A đúng: filter giữ lại số chẵn [2, 4], map nhân 10 thành [20, 40], và mảng gốc a vẫn giữ nguyên 4 phần tử. Đáp án B bỏ sót bước filter; C và D sai vì cho rằng a bị rút ngắn xuống còn 2 phần tử."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tìm lỗi sai): Đoạn code tính tổng điểm `const total = [{ s: 10 }, { s: 20 }].reduce((acc, item) => acc + item.s);` cho kết quả sai là gì?",
                        "30",
                        "\"[object Object]20\"",
                        "NaN",
                        "0",
                        "B",
                        "Đáp án B đúng: do thiếu initialValue = 0 ở tham số thứ 2, reduce lấy phần tử đầu tiên là object `{ s: 10 }` làm `acc`, rồi cộng với `20` nên JS ép object thành chuỗi `\"[object Object]20\"`. A chỉ đúng nếu truyền `, 0`; C và D không khớp ép kiểu cộng chuỗi với object."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Đọc code): Kết quả in ra của đoạn mã sau là gì? `const nums = [10, 2, 30]; const sorted = nums.sort(); console.log(sorted[0], nums[0]);`",
                        "2 10",
                        "2 2",
                        "10 10",
                        "10 2",
                        "C",
                        "Đáp án C đúng vì 2 lý do: (1) `.sort()` không có hàm so sánh sẽ sắp xếp theo thứ tự từ điển chuỗi nên `'10'` đứng trước `'2'`; (2) `.sort()` đột biến trực tiếp mảng `nums` nên cả `sorted[0]` và `nums[0]` đều là 10. A, B, D sai do nhầm về sắp xếp chuỗi hoặc tính bất biến."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tìm lỗi sai): Hàm `const res = arr.map(u => { u.active = true; return u; });` vi phạm nguyên tắc nào?",
                        "Lỗi cú pháp arrow function không có ngoặc nhọn",
                        "Gây vòng lặp vô hạn khi arr có hơn 1 phần tử",
                        "Làm hàm map trả về undefined thay vì mảng",
                        "Đột biến trực tiếp (mutate) các object nằm trong mảng arr gốc",
                        "D",
                        "Đáp án D đúng: tuy map() tạo mảng mới nhưng gán `u.active = true` sửa thẳng vào object gốc trong Heap. Cần viết `u => ({ ...u, active: true })`. Đáp án A, B, C đều sai vì cú pháp hoàn toàn hợp lệ và không lặp vô tận."
                    ),
                    // 2 Tình huống & Tối ưu thuật toán
                    new QuizQuestionSpec(
                        "Câu 9 (Edge-case): Khi chạy `[].reduce((sum, x) => sum + x)` mà KHÔNG truyền giá trị khởi tạo, chuyện gì xảy ra?",
                        "Trả về 0",
                        "Trả về undefined",
                        "Ném lỗi TypeError: Reduce of empty array with no initial value",
                        "Trả về mảng rỗng []",
                        "C",
                        "Đáp án C đúng: theo chuẩn JS, gọi reduce trên mảng rỗng mà thiếu initialValue sẽ ném TypeError ngay lúc chạy. A chỉ đúng khi truyền `, 0`; B và D không đúng hành vi reduce."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tối ưu): Trong React, bạn cần lọc danh sách 1.000 đơn hàng và tính tổng tiền bằng 1 lần duyệt duy nhất thay vì gọi `.filter().map().reduce()` tạo 2 mảng trung gian. Cách nào tối ưu nhất?",
                        "Dùng một lệnh .reduce((sum, o) => o.paid ? sum + o.amount : sum, 0)",
                        "Dùng .forEach() kết hợp .sort() trước rồi cộng",
                        "Chuyển mảng sang chuỗi JSON rồi tách ra cộng",
                        "Dùng .find() nhiều lần trong vòng lặp while",
                        "A",
                        "Đáp án A đúng: gom điều kiện lọc và phép cộng vào một lần `.reduce(..., 0)` đạt độ phức tạp O(n) thời gian và O(1) bộ nhớ phụ. Đáp án B làm tốn O(n log n) để sort và mutate mảng; C và D chậm và sai mục đích."
                    )
                }
            ),

            // 2. Quiz Bài 1.2: Tìm kiếm nhị phân & Two Pointers
            BuildQuiz(
                "l12",
                SeedDataInitializer.CodingLesson12Id,
                "Kiểm tra Bài 1.2: Tìm Kiếm Nhị Phân & Kỹ Thuật Hai Con Trỏ",
                now.AddSeconds(2),
                now,
                new[]
                {
                    // 4 Cơ bản
                    new QuizQuestionSpec(
                        "Câu 1 (Cơ bản): Điều kiện tiên quyết bắt buộc để thuật toán Tìm kiếm Nhị phân (Binary Search) hoạt động chính xác là gì?",
                        "Mảng phải có số lượng phần tử chẵn",
                        "Mảng phải chứa toàn số nguyên dương",
                        "Mảng phải không có số trùng lặp",
                        "Mảng phải được sắp xếp theo thứ tự (tăng dần hoặc giảm dần)",
                        "D",
                        "Đáp án D đúng vì Binary Search dựa vào tính đơn điệu để loại bỏ một nửa mảng ở mỗi bước. Đáp án A, B, C đều không phải điều kiện bắt buộc của Binary Search."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Cơ bản): Độ phức tạp thời gian trong trường hợp xấu nhất của Tìm kiếm Tuyến tính và Tìm kiếm Nhị phân lần lượt là gì?",
                        "O(n) và O(log n)",
                        "O(1) và O(n)",
                        "O(n²) và O(n)",
                        "O(log n) và O(n)",
                        "A",
                        "Đáp án A đúng: tìm kiếm tuyến tính duyệt tuần tự tối đa n phần tử O(n), còn tìm kiếm nhị phân chia đôi không gian sau mỗi bước nên chỉ mất O(log n). Đáp án B, C, D sai về độ phức tạp."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Cơ bản): Trong JavaScript, khi tính chỉ số giữa `mid` từ `left` và `right`, tại sao phải dùng `Math.floor((left + right) / 2)`?",
                        "Để ép kết quả thành số thập phân chính xác",
                        "Để làm tròn xuống thành số nguyên hợp lệ làm chỉ số mảng",
                        "Để đảo dấu số âm thành số dương",
                        "Để sắp xếp lại mảng tự động",
                        "B",
                        "Đáp án B đúng: phép chia `(left + right) / 2` có thể ra số lẻ `.5`, và `arr[1.5]` sẽ trả về `undefined`, nên cần `Math.floor` làm tròn xuống. Đáp án A, C, D sai tác dụng của Math.floor."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Cơ bản): Khi sử dụng kỹ thuật Hai con trỏ đối xứng hai đầu (`left = 0`, `right = arr.length - 1`) để kiểm tra chuỗi đối xứng, độ phức tạp bộ nhớ phụ là bao nhiêu?",
                        "O(n)",
                        "O(n log n)",
                        "O(n²)",
                        "O(1)",
                        "D",
                        "Đáp án D đúng: ta chỉ tạo 2 biến số nguyên `left` và `right` mà không cấp phát mảng phụ mới nên bộ nhớ phụ là hằng số O(1). Đáp án A, B, C đánh giá sai bộ nhớ sử dụng."
                    ),
                    // 4 Đọc code đoán output / Tìm lỗi sai
                    new QuizQuestionSpec(
                        "Câu 5 (Đọc code): Đoạn code hai con trỏ sau in ra gì? `const a = [1, 2, 3, 4, 5]; let l = 0, r = a.length - 1; while (l < r) { [a[l], a[r]] = [a[r], a[l]]; l++; r--; } console.log(JSON.stringify(a));`",
                        "[5,4,3,2,1]",
                        "[1,2,3,4,5]",
                        "[5,2,3,4,1]",
                        "[4,5,3,1,2]",
                        "A",
                        "Đáp án A đúng: lượt 1 đổi 1 và 5 -> [5,2,3,4,1]; lượt 2 đổi 2 và 4 -> [5,4,3,2,1]; sau đó l = 2, r = 2 nên dừng và trả về [5,4,3,2,1]. B, C, D chỉ là trạng thái trung gian hoặc sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Tìm lỗi sai): Trong hàm Binary Search `while (left <= right) { const mid = Math.floor((left + right) / 2); if (arr[mid] === target) return mid; if (arr[mid] < target) left = mid; else right = mid - 1; }`, lỗi nghiêm trọng là gì?",
                        "Thiếu dấu ngoặc ở Math.floor",
                        "Điều kiện `left <= right` gây lỗi truy cập ngoài mảng",
                        "Gán `left = mid` gây vòng lặp vô hạn khi left và right kề nhau",
                        "Phải gán `right = mid + 1` thay vì `mid - 1`",
                        "C",
                        "Đáp án C đúng: khi left = 0, right = 1 và target > arr[0], mid = 0 và gán lại `left = 0` khiến vòng lặp quay vô tận! Phải sửa thành `left = mid + 1`. A, B, D đều sai nhận định."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Đọc code): Cho `twoSumSorted([2, 5, 8, 11, 15], 13)` dùng 2 con trỏ 2 đầu (trả về `[left, right]`). Lần lặp đầu tiên tổng là 2 + 15 = 17 > 13 nên giảm `right`. Hàm trả về cặp chỉ số nào?",
                        "[0, 4]",
                        "[1, 2]",
                        "[0, 3]",
                        "[2, 3]",
                        "C",
                        "Đáp án C đúng: Lượt 1 có arr[0] + arr[4] = 2 + 15 = 17 > 13 nên giảm right xuống 3. Lượt 2 có arr[0] + arr[3] = 2 + 11 = 13 khớp target, trả về ngay [0, 3]. Đáp án B ([1, 2] là 5 + 8 = 13) không được chọn trước vì thuật toán dừng ngay khi tìm thấy [0, 3]; A và D có tổng khác 13."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tìm lỗi sai): Hàm Binary Search dùng `while (left < right)` (thiếu dấu `=`) sẽ gặp lỗi ở trường hợp nào sau đây?",
                        "Khi tìm số ở chính giữa mảng có 5 phần tử",
                        "Khi mảng có 1 phần tử `[7]` và `target = 7`",
                        "Khi mảng đã sắp xếp giảm dần",
                        "Khi mảng chứa toàn số âm",
                        "B",
                        "Đáp án B đúng: với `[7]`, `left = 0` và `right = 0`. Điều kiện `0 < 0` là false nên vòng lặp không chạy lần nào và trả về `-1` dù số `7` có trong mảng! Dùng `while (left <= right)` khắc phục triệt để lỗi off-by-one này."
                    ),
                    // 2 Tình huống & Tối ưu thuật toán
                    new QuizQuestionSpec(
                        "Câu 9 (Edge-case): Khi mảng đầu vào là mảng rỗng `[]` và bạn khởi tạo `let left = 0, right = arr.length - 1;`, giá trị của `right` là bao nhiêu và `while (left <= right)` chạy mấy lần?",
                        "right = -1, vòng lặp chạy 0 lần và an toàn",
                        "right = 0, vòng lặp chạy 1 lần rồi crash",
                        "right = undefined, gây lỗi NaN",
                        "right = 1, trả về chỉ số 0",
                        "A",
                        "Đáp án A đúng: vì `[].length === 0` nên `right = -1`. Điều kiện `0 <= -1` là sai ngay lập tức nên vòng lặp chạy 0 lần và thoát an toàn trả về `-1`. B, C, D không đúng thực tế thực thi."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tối ưu): Hệ thống có 100.000 mã đơn hàng đã sắp xếp tăng dần. Cần xóa bỏ mã trùng lặp tại chỗ mà KHÔNG tạo thêm mảng phụ hay Set. Nên dùng mẫu nào?",
                        "Dùng 2 vòng lặp for + arr.splice() trong mỗi lần trùng (O(n²))",
                        "Dùng Binary Search tìm từng phần tử",
                        "Dùng Two Pointers cùng chiều `read` và `write` ghi đè trong O(n) và O(1) bộ nhớ",
                        "Dùng .filter() kết hợp .indexOf() (O(n²))",
                        "C",
                        "Đáp án C đúng: mẫu Fast & Slow (read/write) duyệt qua mảng đúng 1 lần trong O(n) và không cấp phát bộ nhớ phụ O(1). Đáp án A và D tốn O(n²); đáp án B không dùng để lọc trùng in-place."
                    )
                }
            ),

            // 3. Quiz Bài 2.1: Debounce & Throttle
            BuildQuiz(
                "l21",
                SeedDataInitializer.CodingLesson21Id,
                "Kiểm tra Bài 2.1: Closure, Debounce & Throttle",
                now.AddSeconds(3),
                now,
                new[]
                {
                    // 4 Cơ bản
                    new QuizQuestionSpec(
                        "Câu 1 (Cơ bản): Closure (bao đóng) trong JavaScript là gì?",
                        "Khả năng hàm con ghi nhớ và truy cập biến thuộc phạm vi hàm cha ngay cả khi hàm cha đã chạy xong",
                        "Cú pháp đóng toàn bộ tab trình duyệt bằng window.close()",
                        "Một lỗi tràn bộ nhớ khiến hàm không thể return",
                        "Cách viết rút gọn của câu lệnh if...else",
                        "A",
                        "Đáp án A đúng: Closure cho phép hàm con giữ tham chiếu tới biến trong lexical scope của hàm cha (như `timerId` hay `inThrottle`). B, C, D là khái niệm sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Cơ bản): Sự khác biệt cốt lõi giữa `Debounce` và `Throttle` là gì?",
                        "Debounce chỉ chạy trên trình duyệt Chrome, Throttle chỉ chạy trên Node.js",
                        "Debounce đợi người dùng ngừng phát sự kiện đủ `delay` ms mới chạy 1 lần; Throttle cho chạy đều đặn tối đa 1 lần mỗi chu kỳ `limit` ms",
                        "Debounce dùng cho sự kiện scroll, Throttle dùng cho ô tìm kiếm gõ phím",
                        "Hai kỹ thuật hoàn toàn giống nhau, chỉ khác tên gọi",
                        "B",
                        "Đáp án B đúng: Debounce gom sự kiện liên tiếp đến khi ngừng hẳn, Throttle tiết chế tần suất chạy theo nhịp. C đảo ngược ca sử dụng chuẩn; A và D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Cơ bản): Hàm `clearTimeout(timerId)` dùng để làm gì?",
                        "Xóa toàn bộ biến trong bộ nhớ RAM",
                        "Tăng tốc độ đếm của setTimeout lên gấp đôi",
                        "Hủy bỏ một bộ hẹn giờ setTimeout đang chờ trước khi hàm callback của nó kịp thực thi",
                        "Khởi động lại vòng lặp for từ đầu",
                        "C",
                        "Đáp án C đúng: `clearTimeout(timerId)` hủy timer đã đăng ký qua `setTimeout` trước khi hết hạn. A, B, D đều không phải chức năng của `clearTimeout`."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Cơ bản): Kịch bản nào sau đây phù hợp nhất với **Debounce** thay vì Throttle?",
                        "Cập nhật thanh tiến trình khi người dùng cuộn trang dài",
                        "Gửi API gợi ý từ khóa khi người dùng đang gõ vào ô tìm kiếm (Search Autocomplete)",
                        "Theo dõi tọa độ chuột khi kéo thả thanh trượt (slider)",
                        "Giới hạn tốc độ bắn đạn trong game HTML5",
                        "B",
                        "Đáp án B đúng: với ô tìm kiếm, ta chỉ muốn gọi API sau khi người dùng dừng gõ 300ms để đỡ tốn request. A, C, D đều cần phản hồi đều đặn trong lúc đang thao tác nên hợp với Throttle."
                    ),
                    // 4 Đọc code đoán output / Tìm lỗi sai
                    new QuizQuestionSpec(
                        "Câu 5 (Đọc code): Đoạn code JS sau in ra gì? `function makeCounter() { let n = 0; return () => ++n; } const c1 = makeCounter(); const c2 = makeCounter(); console.log(c1(), c1(), c2());`",
                        "1 2 3",
                        "1 1 1",
                        "1 2 1",
                        "0 1 0",
                        "C",
                        "Đáp án C đúng: mỗi lần gọi `makeCounter()` tạo ra một closure độc lập! `c1` có biến `n` riêng (1 rồi 2), còn `c2` có biến `n` riêng mới khởi tạo từ 0 nên `c2()` trả về `1`. A sai vì tưởng chung biến; B và D sai phép tiền tăng `++n`."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Đọc code): Đoạn code sau in ra giá trị nào? `let x = 5; const id = setTimeout(() => { x = 20; }, 100); clearTimeout(id); console.log(x);`",
                        "20",
                        "undefined",
                        "100",
                        "5",
                        "D",
                        "Đáp án D đúng: `clearTimeout(id)` đã hủy callback `x = 20` ngay lập tức trước khi nó có cơ hội chạy, nên `console.log(x)` in ra `5`. A, B, C đều sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tìm lỗi sai): Trong hàm `function badDebounce(fn, delay) { let timer; return function(...args) { timer = setTimeout(() => fn(...args), delay); }; }`, tại sao gõ 5 ký tự vẫn gọi `fn` đủ 5 lần?",
                        "Vì thiếu `clearTimeout(timer)` trước khi đặt `setTimeout` mới",
                        "Vì thiếu từ khóa `async` trước `function`",
                        "Vì `...args` không hoạt động trong `setTimeout`",
                        "Vì biến `timer` phải khai báo là `const`",
                        "A",
                        "Đáp án A đúng: do không gọi `clearTimeout(timer)` để hủy hẹn giờ cũ, cả 5 lần hẹn giờ vẫn giữ nguyên và cùng nổ sau `delay` ms. B, C sai; D càng sai vì `timer` cần gán lại nên phải dùng `let`."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tìm lỗi sai): Trong hàm `function debounce(fn, delay) { let t; return function(...args) { clearTimeout(t); t = setTimeout(() => fn(), delay); }; }`, lỗi nào xảy ra khi gọi `debouncedFn('react')`?",
                        "Hàm `fn` bị gọi lặp vô tận",
                        "Hàm `fn` không nhận được tham số `'react'` (nhận `undefined`) và mất ngữ cảnh `this`",
                        "Ném lỗi SyntaxError khi biên dịch",
                        "Biến `t` bị reset về 0 sau 1 lần gọi",
                        "B",
                        "Đáp án B đúng: dòng `fn()` bỏ quên `...args` và `this`. Phải viết `fn.apply(this, args)` để truyền đúng ngữ cảnh và tham số. A, C, D không đúng với đoạn mã."
                    ),
                    // 2 Tình huống & Tối ưu thuật toán
                    new QuizQuestionSpec(
                        "Câu 9 (Edge-case): Trong React, một developer viết `const onSearch = debounce(callApi, 300);` trực tiếp ngay bên trong thân component mà không dùng `useMemo`/`useRef`. Hậu quả là gì?",
                        "Ứng dụng không biên dịch được",
                        "Mỗi lần component re-render lại tạo ra một instance `debounce` và `timerId` mới tinh, làm chức năng debounce mất tác dụng",
                        "React tự động biến nó thành Throttle",
                        "Tất cả API đều trả về mã lỗi 404",
                        "B",
                        "Đáp án B đúng: mỗi lần state đổi và component render lại, `debounce(...)` bị gọi lại, tạo closure mới với `timerId` mới nên không hủy được timer của lần render trước. A, C, D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tối ưu): Tại sao hàm `debounce` chuẩn production luôn gắn thêm phương thức `.cancel()` để gọi khi người dùng rời màn hình (unmount)?",
                        "Để tăng tốc độ CPU khi cuộn trang",
                        "Để dọn dẹp timer đang treo trong Event Loop, tránh callback chạy trên component đã unmount gây lỗi và rò rỉ bộ nhớ (Memory Leak)",
                        "Để ép trình duyệt xóa LocalStorage tự động",
                        "Để chuyển Debounce thành Promise",
                        "B",
                        "Đáp án B đúng: nếu user gõ xong và chuyển trang ngay trong lúc timer 300ms chưa nổ, việc không gọi `.cancel()` khiến callback vẫn kích hoạt ngầm trên dữ liệu cũ, gây Memory Leak. A, C, D sai mục đích của `.cancel()`."
                    )
                }
            ),

            // 4. Quiz Bài 2.2: Memoization & Cache
            BuildQuiz(
                "l22",
                SeedDataInitializer.CodingLesson22Id,
                "Kiểm tra Bài 2.2: Hàm Thuần Khiết, Map & Memoization",
                now.AddSeconds(4),
                now,
                new[]
                {
                    // 4 Cơ bản
                    new QuizQuestionSpec(
                        "Câu 1 (Cơ bản): Điều kiện tiên quyết để một hàm có thể áp dụng kỹ thuật Memoization an toàn là gì?",
                        "Hàm phải gọi ít nhất 1 API mạng",
                        "Hàm phải có ít nhất 3 tham số trở lên",
                        "Hàm phải là Hàm thuần khiết (Pure Function): cùng input luôn trả về cùng output và không có side-effect",
                        "Hàm phải luôn trả về kiểu số nguyên",
                        "C",
                        "Đáp án C đúng: nếu hàm phụ thuộc biến ngoài hay thời gian/ngẫu nhiên (bất thuần khiết), việc trả về kết quả cũ từ cache sẽ làm dữ liệu bị sai lệch. A, B, D đều sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Cơ bản): Trong đối tượng `Map` của JavaScript, 3 phương thức tương ứng với (1) kiểm tra khóa tồn tại, (2) đọc giá trị, (3) lưu khóa-giá trị là gì?",
                        "contains, read, put",
                        "has, get, set",
                        "includes, find, add",
                        "exists, fetch, push",
                        "B",
                        "Đáp án B đúng: `Map` dùng `cache.has(key)`, `cache.get(key)`, và `cache.set(key, val)`. Các tên hàm ở A, C, D không tồn tại trên `Map.prototype`."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Cơ bản): Khi gọi hàm đã được memoize với bộ tham số đã từng tính toán trước đó và kết quả có sẵn trong cache, trạng thái đó gọi là gì?",
                        "Cache Hit",
                        "Cache Miss",
                        "Cache Overflow",
                        "Key Collision",
                        "A",
                        "Đáp án A đúng: Cache Hit nghĩa là tìm thấy ngay dữ liệu trong bộ nhớ đệm với O(1). Cache Miss (B) là chưa có nên phải tính lại; C là tràn cache; D là đụng độ khóa."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Cơ bản): Hàm nào sau đây KHÔNG phải là hàm thuần khiết và KHÔNG nên memoize vĩnh viễn?",
                        "function sum(a, b) { return a + b; }",
                        "function formatVnd(amount) { return amount + ' đ'; }",
                        "function isEven(n) { return n % 2 === 0; }",
                        "function getGreetingWithTime(name) { return name + ' lúc ' + Date.now(); }",
                        "D",
                        "Đáp án D đúng: `getGreetingWithTime` phụ thuộc vào `Date.now()` (thời gian thay đổi liên tục), không cho cùng kết quả với cùng đầu vào `name`. A, B, C đều thuần khiết 100%."
                    ),
                    // 4 Đọc code đoán output / Tìm lỗi sai
                    new QuizQuestionSpec(
                        "Câu 5 (Đọc code): Đoạn code sau in ra `calls` bằng bao nhiêu? `let calls = 0; const m = new Map(); function calc(n) { if (m.has(n)) return m.get(n); calls++; const r = n * 2; m.set(n, r); return r; } calc(3); calc(3); calc(5); calc(3); console.log(calls);`",
                        "4",
                        "3",
                        "2",
                        "1",
                        "C",
                        "Đáp án C đúng: Lần 1 `calc(3)` miss (`calls = 1`), lưu 3=>6. Lần 2 `calc(3)` hit. Lần 3 `calc(5)` miss (`calls = 2`), lưu 5=>10. Lần 4 `calc(3)` hit. Tổng `calls` chỉ bằng 2! A, B, D đếm sai Cache Hit."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Đọc code): Kết quả của `const cache = {}; function keyOf(o) { return o.toString(); } console.log(keyOf({ x: 1 }) === keyOf({ y: 99 }));` là gì?",
                        "true",
                        "false",
                        "\"[object Object]\"",
                        "Ném lỗi TypeError",
                        "A",
                        "Đáp án A đúng: mọi plain object trong JS khi gọi `.toString()` đều mặc định biến thành `\"[object Object]\"`, nên cả hai ra chuỗi giống hệt nhau dẫn đến `true` (bẫy Key Collision). B, C, D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tìm lỗi sai): Trong `function memo(fn) { const c = new Map(); return (x) => { if (c.has(x)) return c.get(x); const r = fn(x); return r; }; }`, hàm bị lỗi gì?",
                        "Thiếu lệnh `c.set(x, r)` nên lần nào gọi cũng bị Cache Miss",
                        "Dùng `c.has(x)` không kiểm tra được số âm",
                        "Map không lưu được giá trị trả về của hàm `fn`",
                        "Không thể dùng arrow function bên trong `memo`",
                        "A",
                        "Đáp án A đúng: hàm tính ra `r = fn(x)` nhưng quên gọi `c.set(x, r)` để lưu vào Map! Vì thế `c.has(x)` luôn trả về `false`. B, C, D đều sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tìm lỗi sai): Hàm `function getCached(key) { if (cache.has(key)) return cache.get(key); const arr = [1, 2]; cache.set(key, arr); return arr; }` gặp nguy cơ nào khi bên ngoài làm `const list = getCached('k'); list.push(3);`?",
                        "Map tự xóa key `'k'` ngay khi gọi `.push()`",
                        "Trả về tham chiếu trực tiếp của mảng trong cache khiến `list.push(3)` làm đột biến dữ liệu đang lưu trong cache cho mọi lần gọi sau",
                        "Lỗi SyntaxError vì không thể push vào mảng lấy từ Map",
                        "Lệnh `list.push(3)` chỉ thay đổi biến `list` chứ không ảnh hưởng cache",
                        "B",
                        "Đáp án B đúng: `arr` là kiểu tham chiếu; trả về trực tiếp khiến caller có thể sửa hỏng mảng trong cache. Cách sửa chuẩn là trả bản sao `[...arr]`. A, C, D sai."
                    ),
                    // 2 Tình huống & Tối ưu thuật toán
                    new QuizQuestionSpec(
                        "Câu 9 (Edge-case): Nếu dùng object thường `{}` làm cache và viết `if (cache[key]) return cache[key];`, trường hợp nào gây lỗi ngầm nghiêm trọng?",
                        "Khi `key` là số nguyên dương lớn hơn 100",
                        "Khi kết quả hợp lệ trả về là `0`, `false`, hoặc `\"\"` (falsy) khiến cache luôn tưởng là chưa có dữ liệu",
                        "Khi `cache` có hơn 5 phần tử",
                        "Khi `fn` có 1 tham số",
                        "B",
                        "Đáp án B đúng: các giá trị falsy (`0`, `false`, `\"\"`, `null`) làm `if (cache[key])` ra `false`, dẫn tới tính lại mỗi lần gọi. Luôn dùng `Map` và `cache.has(key)` để kiểm tra tồn tại khóa."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tối ưu): Áp dụng Memoization cho hàm tính số Fibonacci đệ quy giúp cải thiện độ phức tạp thời gian từ mức nào xuống mức nào, và cần lưu ý gì để tránh tràn RAM khi cache chạy lâu?",
                        "Từ O(n) xuống O(1); không cần giới hạn RAM",
                        "Từ O(n²) xuống O(n log n); dùng thêm setTimeout",
                        "Từ O(2ⁿ) hàm mũ xuống O(n) tuyến tính; cần giới hạn kích thước cache (FIFO/LRU eviction) để tránh OutOfMemory",
                        "Từ O(log n) xuống O(1); phải chuyển Map sang Array",
                        "C",
                        "Đáp án C đúng: Fibonacci đệ quy thuần mất O(2ⁿ) do tính lặp lại nhánh con; Memoization giảm xuống O(n). Tuy nhiên cache vô hạn sẽ phình RAM mãi nên cần LRU/FIFO eviction. A, B, D sai độ phức tạp hoặc giải pháp."
                    )
                }
            ),

            // 5. Quiz Bài 3.1: Async/Await, Promise Pipeline & Retry
            BuildQuiz(
                "l31",
                SeedDataInitializer.CodingLesson31Id,
                "Kiểm tra Bài 3.1: Event Loop, Async/Await & Promise Pipeline",
                now.AddSeconds(5),
                now,
                new[]
                {
                    // 4 Cơ bản
                    new QuizQuestionSpec(
                        "Câu 1 (Cơ bản): Một đối tượng `Promise` trong JavaScript có đúng 3 trạng thái nào?",
                        "Start, Running, Stop",
                        "Pending (Đang chờ), Fulfilled (Thành công), Rejected (Thất bại)",
                        "Open, Closed, Error",
                        "Sync, Async, Timeout",
                        "B",
                        "Đáp án B đúng: mọi Promise khởi đầu ở `Pending` và chuyển sang duy nhất một trạng thái kết thúc là `Fulfilled` hoặc `Rejected`. A, C, D không phải trạng thái chuẩn của Promise."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Cơ bản): Thứ tự ưu tiên xả hàng đợi của Event Loop khi Call Stack vừa trống là gì?",
                        "Macrotask Queue (setTimeout) chạy trước Microtask Queue (Promise.then)",
                        "Chạy ngẫu nhiên tùy trình duyệt",
                        "Microtask Queue (Promise.then / phần sau await) được xả sạch trước rồi mới đến Macrotask Queue (setTimeout)",
                        "Hai hàng đợi chạy song song trên 2 luồng CPU khác nhau",
                        "C",
                        "Đáp án C đúng: sau khi Call Stack chạy hết code đồng bộ, Event Loop ưu tiên xả toàn bộ Microtask Queue trước khi lấy 1 tác vụ từ Macrotask Queue. A, B, D sai cơ chế Event Loop."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Cơ bản): Nếu bạn gọi một hàm `async function getUser() { return 'An'; }` mà KHÔNG dùng `await` phía trước, giá trị nhận được là gì?",
                        "Chuỗi 'An'",
                        "undefined",
                        "Một đối tượng Promise đang chứa giá trị 'An'",
                        "Ném lỗi SyntaxError",
                        "C",
                        "Đáp án C đúng: mọi hàm khai báo với `async` luôn luôn bọc giá trị trả về vào trong một `Promise`. Muốn lấy chuỗi `'An'` trực tiếp phải dùng `await getUser()` hoặc `.then()`. A, B, D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Cơ bản): Sự khác biệt quan trọng giữa `Promise.all()` và `Promise.allSettled()` là gì?",
                        "Promise.all() chạy tuần tự, Promise.allSettled() chạy song song",
                        "Promise.all() dừng và reject ngay nếu 1 task lỗi (Fail-Fast); Promise.allSettled() chờ tất cả hoàn tất và trả về trạng thái từng task",
                        "Promise.allSettled() tự động retry 3 lần khi có task lỗi",
                        "Promise.all() chỉ nhận tối đa 2 Promise",
                        "B",
                        "Đáp án B đúng: `Promise.all` fail-fast khi có 1 promise bị rejected, còn `Promise.allSettled` luôn chờ đủ tất cả kết quả (`fulfilled` hoặc `rejected`). A, C, D sai đặc tả."
                    ),
                    // 4 Đọc code đoán output / Tìm lỗi sai
                    new QuizQuestionSpec(
                        "Câu 5 (Đọc code): Đoạn code JS sau in ra theo thứ tự nào? `console.log('A'); setTimeout(() => console.log('B'), 0); Promise.resolve().then(() => console.log('C')); console.log('D');`",
                        "A -> B -> C -> D",
                        "A -> D -> B -> C",
                        "C -> A -> D -> B",
                        "A -> D -> C -> B",
                        "D",
                        "Đáp án D đúng: đồng bộ `'A'` và `'D'` chạy trong Call Stack trước -> Microtask `'C'` chạy ngay sau đó -> Macrotask `setTimeout` `'B'` chạy sau cùng. A, B, C sai thứ tự ưu tiên Event Loop."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Đọc code): Đoạn code sau in ra theo thứ tự nào? `async function run() { console.log('1'); await Promise.resolve(); console.log('2'); } run(); console.log('3');`",
                        "1 -> 3 -> 2",
                        "1 -> 2 -> 3",
                        "3 -> 1 -> 2",
                        "2 -> 1 -> 3",
                        "A",
                        "Đáp án A đúng: lệnh bên trong `run()` trước `await` chạy đồng bộ nên in `'1'`; gặp `await`, phần sau đó (`console.log('2')`) đưa vào Microtask Queue nên hàm trả quyền lại cho luồng chính in `'3'`, sau đó mới in `'2'`. B, C, D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tìm lỗi sai): Trong `async function loadAll(urls) { const res = []; urls.forEach(async (u) => { res.push(await fetch(u)); }); return res; }`, kết quả trả về là gì?",
                        "Mảng chứa đầy đủ dữ liệu đã tải theo đúng thứ tự",
                        "Mảng rỗng `[]` vì `forEach` không chờ `await` bên trong callback hoàn tất",
                        "Ném lỗi SyntaxError vì không thể dùng async trong forEach",
                        "Hàm bị treo vô tận ở dòng đầu tiên",
                        "B",
                        "Đáp án B đúng: `Array.prototype.forEach` gọi các callback async rồi kết thúc ngay lập tức mà không chờ Promise! Để chạy tuần tự phải dùng `for (const u of urls)`, hoặc song song dùng `Promise.all(urls.map(...))`."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tìm lỗi sai): Trong `function delayFail(ms) { return new Promise((resolve, reject) => { setTimeout(() => { /* Quên gọi reject */ }, ms); }); }`, hậu quả khi gọi `await delayFail(100)` là gì?",
                        "Trả về undefined sau 100ms",
                        "Tự động nhảy vào khối catch sau 100ms",
                        "Promise bị kẹt mãi ở trạng thái Pending nên lệnh `await` treo vĩnh viễn không bao giờ chạy tiếp",
                        "Trình duyệt tự động biến Promise thành Fulfilled",
                        "C",
                        "Đáp án C đúng: một Promise chỉ kết thúc khi gọi `resolve(...)` hoặc `reject(...)`. Nếu không gọi cả hai, nó mãi mãi ở `Pending` làm lệnh `await` đứng chờ vô hạn. A, B, D sai."
                    ),
                    // 2 Tình huống & Tối ưu thuật toán
                    new QuizQuestionSpec(
                        "Câu 9 (Edge-case): Điều gì xảy ra nếu trong một hàm `async` bạn gọi `await fetchApi()` mà KHÔNG bọc `try/catch` khi `fetchApi()` bị lỗi mạng?",
                        "Hàm tự động thử lại thêm 1 lần nữa",
                        "Lệnh `await` tự trả về `null` an toàn",
                        "Lỗi văng ra làm Promise của hàm bị Rejected (Unhandled Promise Rejection) nếu bên ngoài cũng không bắt lỗi, có thể gây crash hoặc treo UI",
                        "JavaScript tự bỏ qua dòng lỗi và chạy dòng tiếp theo",
                        "C",
                        "Đáp án C đúng: `await` trên một Promise bị reject sẽ ném lỗi như một exception đồng bộ; thiếu `try/catch` sẽ khiến hàm thoát đột ngột và để lại Unhandled Promise Rejection. A, B, D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tối ưu): Khi thiết kế cơ chế tự động gọi lại API bị lỗi (Retry), vì sao phải dùng **Exponential Backoff kết hợp Jitter** thay vì retry ngay lập tức trong vòng lặp `while`?",
                        "Để nén kích thước gói tin JSON nhỏ hơn",
                        "Để giãn thời gian chờ theo cấp số nhân và phân tán thời điểm gọi ngẫu nhiên, tránh hàng nghìn client cùng dội lại đánh sập server (Thundering Herd)",
                        "Để vượt qua lỗi cú pháp JavaScript",
                        "Để bắt buộc các request phải chạy đồng bộ",
                        "B",
                        "Đáp án B đúng: tăng thời gian chờ cấp số nhân (Exponential Backoff) giúp server có thời gian hồi phục, và Jitter ngẫu nhiên ngăn bão request đồng loạt. A, C, D sai bản chất kỹ thuật."
                    )
                }
            ),

            // 6. Quiz Bài 3.2: Deep Clone, Immutability & Error Handling
            BuildQuiz(
                "l32",
                SeedDataInitializer.CodingLesson32Id,
                "Kiểm tra Bài 3.2: Deep Clone, Tính Bất Biến & Xử Lý Lỗi",
                now.AddSeconds(6),
                now,
                new[]
                {
                    // 4 Cơ bản
                    new QuizQuestionSpec(
                        "Câu 1 (Cơ bản): Trong JavaScript, kiểu dữ liệu nào truyền/gán theo **tham chiếu ô nhớ** khiến việc sửa biến mới có thể làm biến gốc đổi theo?",
                        "Number và String",
                        "Boolean và Undefined",
                        "Object và Array",
                        "Symbol và BigInt",
                        "C",
                        "Đáp án C đúng: Object và Array là kiểu tham chiếu (Reference Type), biến chỉ giữ địa chỉ trỏ tới vùng nhớ Heap. A, B, D là kiểu nguyên thủy (Primitive Type) sao chép giá trị độc lập."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Cơ bản): Toán tử spread `{ ...obj }` hoặc `[...arr]` thực hiện loại sao chép nào?",
                        "Sao chép sâu (Deep Clone) mọi tầng vô hạn",
                        "Sao chép nông (Shallow Copy) ở tầng đầu tiên; các object/array lồng bên trong vẫn chung tham chiếu",
                        "Không tạo object/array mới mà chỉ tạo bí danh",
                        "Chuyển object thành chuỗi JSON",
                        "B",
                        "Đáp án B đúng: Spread chỉ tạo bản sao độc lập cho tầng thứ nhất; mọi thuộc tính là object/array lồng sâu hơn vẫn trỏ chung vùng nhớ. A, C, D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Cơ bản): Hàm dựng sẵn hiện đại `structuredClone(obj)` KHÔNG hỗ trợ sao chép loại dữ liệu nào sau đây?",
                        "Object lồng nhau nhiều tầng",
                        "Array và đối tượng Date",
                        "Map, Set và RegExp",
                        "Hàm (Function) và phần tử DOM HTML",
                        "D",
                        "Đáp án D đúng: `structuredClone()` clone tốt Object, Array, Date, RegExp, Map, Set và cả Circular Reference, nhưng sẽ ném `DataCloneError` nếu gặp Function hoặc DOM node. A, B, C đều được hỗ trợ."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Cơ bản): Khi viết một Custom Error Class `class ValidationError extends Error`, dòng nào BẮT BUỘC phải gọi trong `constructor` trước khi dùng `this`?",
                        "super(message);",
                        "return this;",
                        "Error.init(this);",
                        "new Error();",
                        "A",
                        "Đáp án A đúng: trong lớp con kế thừa JS (`extends Error`), phải gọi `super(message)` trước khi truy cập `this`, nếu không sẽ văng `ReferenceError`. B, C, D sai cú pháp/ngữ nghĩa."
                    ),
                    // 4 Đọc code đoán output / Tìm lỗi sai
                    new QuizQuestionSpec(
                        "Câu 5 (Đọc code): Đoạn code JS sau in ra gì? `const m = [[1, 2], [3, 4]]; const copy = [...m]; copy[0].push(99); console.log(m[0].length, copy[0].length);`",
                        "2 3",
                        "3 3",
                        "2 2",
                        "3 2",
                        "B",
                        "Đáp án B đúng: `[...m]` chỉ sao chép nông mảng ngoài; `copy[0]` và `m[0]` trỏ chung cùng một mảng con `[1, 2]` trong Heap, nên `copy[0].push(99)` làm cả hai đều có độ dài 3! A, C, D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Đọc code): Kết quả in ra của đoạn code sau là gì? `const orig = { meta: { tag: 'v1' } }; const cloned = structuredClone(orig); cloned.meta.tag = 'v2'; console.log(orig.meta.tag);`",
                        "'v2'",
                        "undefined",
                        "'v1'",
                        "Ném lỗi DataCloneError",
                        "C",
                        "Đáp án C đúng: `structuredClone` tạo bản sao sâu hoàn toàn độc lập ở mọi tầng, nên sửa `cloned.meta.tag = 'v2'` không ảnh hưởng gì tới `orig.meta.tag` (vẫn là `'v1'`). A, B, D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Tìm lỗi sai): Trong hàm `function clone(v) { if (typeof v !== 'object') return v; const res = {}; for (const k of Object.keys(v)) res[k] = clone(v[k]); return res; }`, gọi `clone({ user: null })` sẽ bị lỗi gì?",
                        "Trả về `{ user: {} }` hoặc ném TypeError vì `typeof null === 'object'` khiến `Object.keys(null)` bị crash",
                        "Chạy bình thường và trả về `{ user: null }`",
                        "Gây vòng lặp vô hạn",
                        "Biến `null` thành chuỗi `'null'`",
                        "A",
                        "Đáp án A đúng: do lỗi lịch sử của JS, `typeof null` bằng `'object'`, nên `null` lọt qua điều kiện dừng và gây crash ngay tại `Object.keys(null)`. Cần thêm `if (v === null || typeof v !== 'object') return v;`. B, C, D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tìm lỗi sai): Đoạn code `try { validate(data); } catch (err) { if (typeof err === 'ValidationError') handle400(); }` có lỗi logic gì?",
                        "Từ khóa `catch` không nhận biến `err`",
                        "`typeof err` luôn trả về `'object'` chứ không bao giờ trả về `'ValidationError'`; phải dùng `err instanceof ValidationError`",
                        "Không thể viết `if` bên trong khối `catch`",
                        "`validate` bắt buộc phải là hàm `async`",
                        "B",
                        "Đáp án B đúng: `typeof` chỉ phân biệt kiểu nguyên thủy và `'object'`/`'function'`; để kiểm tra thực thể thuộc class nào phải dùng `err instanceof ValidationError`. A, C, D sai."
                    ),
                    // 2 Tình huống & Tối ưu thuật toán
                    new QuizQuestionSpec(
                        "Câu 9 (Edge-case): Dùng `JSON.parse(JSON.stringify(obj))` để deep clone sẽ mất dữ liệu hoặc gây lỗi trong trường hợp nào sau đây?",
                        "Object chỉ chứa các số nguyên và chuỗi thuần",
                        "Object chứa đối tượng `Date` (bị biến thành string), giá trị `undefined` (bị xóa mất), `NaN` (biến thành null), hoặc có tham chiếu vòng (ném TypeError)",
                        "Object là mảng 1 chiều các chuỗi",
                        "Object có 2 thuộc tính boolean",
                        "B",
                        "Đáp án B đúng: kỹ thuật JSON serialize đánh mất kiểu `Date`, `RegExp`, `Map`, `Set`, thuộc tính `undefined`, `NaN`, và crash ngay khi gặp circular reference. A, C, D đều clone được qua JSON."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tối ưu): Khi tự viết hàm `safeDeepClone(value, seen = new WeakMap())` hỗ trợ tham chiếu vòng (`a.self = a`), tại sao phải dùng `WeakMap` và phải gọi `seen.set(value, clone)` TRƯỚC khi đệ quy thuộc tính con?",
                        "Để vòng lặp tự động sắp xếp các key theo bảng chữ cái",
                        "WeakMap cho phép bộ dọn rác (GC) tự thu hồi bộ nhớ khi không dùng nữa, và việc lưu `seen.set` trước khi đệ quy giúp nhánh con gặp lại `value` trả về ngay bản sao thay vì đệ quy vô tận gây tràn Stack",
                        "Vì WeakMap tự động chuyển hàm thành chuỗi",
                        "Để giảm thời gian chạy xuống O(1) bất kể kích thước object",
                        "B",
                        "Đáp án B đúng: đăng ký `seen.set(value, clone)` trước khi đệ quy sâu giúp ngắt ngay chu trình lặp vòng khi gặp lại cùng tham chiếu; `WeakMap` còn tránh rò rỉ bộ nhớ cho object. A, C, D sai."
                    )
                }
            ),

            // 7. Đề Tổng Kết Khóa Học (gắn CodingLesson32Id)
            BuildQuiz(
                "final",
                SeedDataInitializer.CodingLesson32Id,
                "Đề Tổng Kết Khóa Học: Lập trình JavaScript & Giải quyết Vấn đề",
                now,
                now,
                new[]
                {
                    // 4 Cơ bản
                    new QuizQuestionSpec(
                        "Câu 1 (Cơ bản): Phương thức mảng nào trả về ĐÚNG 1 giá trị tổng hợp (số, object hoặc mảng) bằng cách tích lũy qua từng phần tử từ một giá trị khởi tạo?",
                        "map()",
                        "filter()",
                        "reduce()",
                        "forEach()",
                        "C",
                        "Đáp án C đúng: `reduce((acc, item) => ..., initialValue)` nén mảng thành bất kỳ kiểu dữ liệu nào. A biến đổi 1-1; B lọc phần tử; D trả về undefined."
                    ),
                    new QuizQuestionSpec(
                        "Câu 2 (Cơ bản): Độ phức tạp thời gian của thuật toán Tìm kiếm Nhị phân (Binary Search) trên mảng đã sắp xếp gồm N phần tử là bao nhiêu?",
                        "O(1)",
                        "O(log N)",
                        "O(N)",
                        "O(N²)",
                        "B",
                        "Đáp án B đúng: mỗi bước Binary Search loại bỏ một nửa không gian tìm kiếm nên mất O(log N). A là tra cứu Map; C là duyệt tuyến tính; D là hai vòng lặp lồng nhau."
                    ),
                    new QuizQuestionSpec(
                        "Câu 3 (Cơ bản): Khi muốn đảm bảo hàm chỉ chạy 1 lần cuối cùng sau khi người dùng NGỪNG gõ phím được 400ms, ta dùng kỹ thuật nào?",
                        "Debounce",
                        "Throttle",
                        "Memoization",
                        "Deep Clone",
                        "A",
                        "Đáp án A đúng: Debounce hủy hẹn giờ cũ mỗi khi có sự kiện mới và chỉ chạy sau khi ngừng thao tác đủ thời gian delay. B chạy theo nhịp; C lưu cache; D sao chép dữ liệu."
                    ),
                    new QuizQuestionSpec(
                        "Câu 4 (Cơ bản): Trong JavaScript, giá trị của biểu thức `typeof null` là gì?",
                        "'null'",
                        "'undefined'",
                        "'boolean'",
                        "'object'",
                        "D",
                        "Đáp án D đúng: đây là một đặc điểm lịch sử nổi tiếng của JavaScript (`typeof null === 'object'`), vì vậy luôn phải kiểm tra `val === null` trước khi xử lý object. A, B, C đều sai kết quả thực tế."
                    ),
                    // 4 Đọc code đoán output / Tìm lỗi sai
                    new QuizQuestionSpec(
                        "Câu 5 (Đọc code): Kết quả in ra của đoạn mã sau là gì? `const prices = [100, 200, 300]; const res = prices.filter(p => p >= 200).reduce((s, p) => s + p, 0); console.log(res, prices.length);`",
                        "500 3",
                        "500 2",
                        "600 3",
                        "300 2",
                        "A",
                        "Đáp án A đúng: filter giữ [200, 300], reduce cộng từ 0 ra 500; vì filter và reduce đều bất biến nên `prices.length` vẫn bằng 3. B sai độ dài mảng gốc; C không lọc; D tính sai tổng."
                    ),
                    new QuizQuestionSpec(
                        "Câu 6 (Đọc code): Đoạn mã sau in ra gì? `console.log('S'); setTimeout(() => console.log('T'), 0); Promise.resolve().then(() => console.log('P')); console.log('E');`",
                        "S -> T -> P -> E",
                        "S -> E -> P -> T",
                        "S -> E -> T -> P",
                        "P -> S -> E -> T",
                        "B",
                        "Đáp án B đúng: đồng bộ `'S'` và `'E'` chạy trong Call Stack -> Microtask `'P'` chạy trước -> Macrotask `'T'` chạy sau cùng. A, C, D sai thứ tự Event Loop."
                    ),
                    new QuizQuestionSpec(
                        "Câu 7 (Đọc code): Đoạn code sau in ra gì? `const state = { user: { name: 'An' } }; const next = { ...state }; next.user.name = 'Bình'; console.log(state.user.name);`",
                        "'An'",
                        "undefined",
                        "'Bình'",
                        "Ném TypeError",
                        "C",
                        "Đáp án C đúng: `{ ...state }` chỉ sao chép nông tầng 1, nên `next.user` và `state.user` dùng chung cùng 1 object trong Heap. Sửa `next.user.name` làm `state.user.name` cũng đổi thành `'Bình'`. A, B, D sai."
                    ),
                    new QuizQuestionSpec(
                        "Câu 8 (Tìm lỗi sai): Trong hàm `function bs(a, t) { let l = 0, r = a.length - 1; while (l <= r) { const m = Math.floor((l + r) / 2); if (a[m] === t) return m; if (a[m] < t) l = m; else r = m - 1; } return -1; }`, lỗi nằm ở đâu?",
                        "Cần đổi `l = m` thành `l = m + 1` để tránh vòng lặp vô hạn",
                        "Cần đổi `r = m - 1` thành `r = m + 1`",
                        "Cần đổi `Math.floor` thành `Math.ceil`",
                        "Không thể dùng `return -1` ở cuối hàm",
                        "A",
                        "Đáp án A đúng: giữ nguyên `l = m` làm `m` kẹt mãi ở `l` khi `r === l + 1` và `a[m] < t`. Phải tiến lên `l = m + 1` vì phần tử `m` đã kiểm tra rồi. B, C, D đều sai."
                    ),
                    // 2 Tình huống & Tối ưu thuật toán
                    new QuizQuestionSpec(
                        "Câu 9 (Edge-case): Một hàm `memoize(fn)` dùng `JSON.stringify(args)` làm khóa trong `Map` để cache kết quả, nhưng gặp hàm `getItems(cat)` trả về một mảng mới. Cần làm gì để vừa an toàn mảng rỗng vừa không bị người gọi làm hỏng cache?",
                        "Dùng `arg.toString()` thay cho `JSON.stringify`",
                        "Trả về bản sao của mảng (hoặc `structuredClone`) khi trả kết quả từ cache và giới hạn kích thước tối đa của `Map`",
                        "Đặt `initialValue` bằng `null` và không bao giờ xóa cache",
                        "Thay `Map` bằng `forEach`",
                        "B",
                        "Đáp án B đúng: trả về bản sao ngăn caller mutate mảng đang nằm trong cache, và giới hạn `maxSize` (LRU/FIFO) giúp RAM không phình vô hạn. A gây Key Collision; C gây tràn RAM; D vô nghĩa."
                    ),
                    new QuizQuestionSpec(
                        "Câu 10 (Tối ưu): Bạn cần tải song song dữ liệu từ 5 API độc lập (mỗi API có thể thất bại riêng lẻ), đồng thời tự thử lại tối đa 3 lần với độ trễ giãn dần khi bị lỗi mạng tạm thời, nhưng vẫn hiển thị được các API thành công. Tổ hợp kỹ thuật nào đúng chuẩn?",
                        "Dùng `forEach(async ...)` + `Promise.all`",
                        "Bọc mỗi lời gọi API trong hàm `retryWithBackoff` (Exponential Backoff + Jitter + try/catch), rồi tổng hợp bằng `Promise.allSettled()`",
                        "Dùng `setTimeout` lồng nhau 5 cấp không có `clearTimeout`",
                        "Dùng `JSON.parse(JSON.stringify(promises))`",
                        "B",
                        "Đáp án B đúng: `retryWithBackoff` tự phục hồi lỗi tạm thời mà không đánh sập server (nhờ Backoff + Jitter), còn `Promise.allSettled()` thu thập tất cả kết quả `fulfilled`/`rejected` mà không bị fail-fast mất trắng dữ liệu như `Promise.all`. A, C, D đều sai nghiêm trọng."
                    )
                }
            )
        };
    }
}
