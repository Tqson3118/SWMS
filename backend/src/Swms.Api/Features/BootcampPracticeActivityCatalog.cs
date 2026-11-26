using System.Text.Json;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Database;

namespace Swms.Api.Features;

/// <summary>
/// Builds deterministic, phase- and language-specific interactive coding activities
/// (fill_code, predict_output, bug_fix) for every lesson of the Studxy Full-Stack Developer Bootcamp.
/// Schema matches CodingPracticeLab.tsx and writein-practice.ts 100%.
/// </summary>
public static class BootcampPracticeActivityCatalog
{
    private sealed record BlankSpec(
        string Id,
        int BlankIndex,
        string Expected,
        string[] AcceptableAnswers,
        string Hint,
        string[] Chips,
        string Explanation);

    private sealed record LineExpSpec(
        int LineNumber,
        string CodeSnippet,
        string Purpose,
        string RiskIfWrong);

    private sealed record ExerciseTripleSpec(
        string Language,
        string FillCategory,
        string FillTitle,
        string FillDescription,
        string FillScenario,
        string[] FillTemplate,
        BlankSpec[] FillBlanks,
        LineExpSpec[] FillLineExplanations,
        string PredictDifficulty,
        string PredictTitle,
        string PredictDescription,
        string PredictCode,
        string PredictExpectedOutput,
        string[] PredictAcceptedOutputs,
        string PredictHint,
        string PredictExplanation,
        string BugTitle,
        string BugCategoryTag,
        string BugContext,
        string BuggyCode,
        int BugLineNumber,
        string CleanFixedCode,
        string[] RequiredTokens,
        string? FunctionName,
        string BugExplanation);

    public static IReadOnlyList<PracticeActivity> BuildForLesson(
        string relativePath,
        Guid courseId,
        Lesson lesson,
        string markdown,
        DateTimeOffset now)
    {
        var normPath = (relativePath ?? string.Empty).Replace('\\', '/').ToLowerInvariant();
        var title = string.IsNullOrWhiteSpace(lesson.Title) ? "Bài học Bootcamp" : lesson.Title.Trim();
        var spec = ResolveExerciseSpec(normPath, title.ToLowerInvariant(), title);
        return MaterializeActivities(spec, courseId, lesson, title, now);
    }

    public static bool IsAsciiOrDiagramBlock(string? fenceLang, string? code)
    {
        var lang = (fenceLang ?? string.Empty).Trim().ToLowerInvariant();
        if (lang is "text" or "txt" or "mermaid" or "ascii" or "diagram")
        {
            return true;
        }
        if (string.IsNullOrWhiteSpace(code))
        {
            return true;
        }
        if (code.Contains("+---+", StringComparison.Ordinal) ||
            code.Contains("|---|", StringComparison.Ordinal) ||
            code.IndexOfAny(['┌', '─', '┐', '│', '└', '┘', '├', '┤', '┬', '┴', '┼']) >= 0)
        {
            return true;
        }
        return false;
    }

    private static string Truncate(string value, int maxLen)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLen) return value ?? string.Empty;
        return value[..maxLen];
    }

    private static List<PracticeActivity> MaterializeActivities(
        ExerciseTripleSpec s,
        Guid courseId,
        Lesson lesson,
        string lessonTitle,
        DateTimeOffset now)
    {
        var shortTag = $"bc-{Math.Abs(lesson.Id.GetHashCode()):x}";

        var fillAct = new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "fill_code", 101),
            CourseId = courseId,
            LessonId = lesson.Id,
            OrderIndex = 101,
            Type = "fill_code",
            Title = Truncate($"Điền Code ({s.Language}): {s.FillTitle}", 250),
            Description = $"{s.FillDescription} — Bài: {lessonTitle}",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = $"{shortTag}-fill-101",
                        title = s.FillTitle,
                        category = s.FillCategory,
                        language = s.Language,
                        description = s.FillDescription,
                        practicalScenario = s.FillScenario,
                        codeTemplate = s.FillTemplate,
                        blanks = s.FillBlanks.Select(b => new
                        {
                            id = b.Id,
                            blankIndex = b.BlankIndex,
                            expected = b.Expected,
                            acceptableAnswers = b.AcceptableAnswers,
                            hint = b.Hint,
                            chips = b.Chips,
                            explanation = b.Explanation
                        }).ToArray(),
                        lineExplanations = s.FillLineExplanations.Select(le => new
                        {
                            lineNumber = le.LineNumber,
                            codeSnippet = le.CodeSnippet,
                            purpose = le.Purpose,
                            riskIfWrong = le.RiskIfWrong
                        }).ToArray()
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var predictAct = new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "predict_output", 102),
            CourseId = courseId,
            LessonId = lesson.Id,
            OrderIndex = 102,
            Type = "predict_output",
            Title = Truncate($"Đoán Output ({s.Language}): {s.PredictTitle}", 250),
            Description = $"{s.PredictDescription} — Bài: {lessonTitle}",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = $"{shortTag}-pred-102",
                        difficulty = s.PredictDifficulty,
                        language = s.Language,
                        title = s.PredictTitle,
                        description = s.PredictDescription,
                        code = s.PredictCode,
                        expectedOutput = s.PredictExpectedOutput,
                        acceptedOutputs = s.PredictAcceptedOutputs,
                        hint = s.PredictHint,
                        explanation = s.PredictExplanation
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var bugAct = new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lesson.Id, "bug_fix", 103),
            CourseId = courseId,
            LessonId = lesson.Id,
            OrderIndex = 103,
            Type = "bug_fix",
            Title = Truncate($"Sửa Bug ({s.Language}): {s.BugTitle}", 250),
            Description = $"{s.BugContext} — Bài: {lessonTitle}",
            ConfigJson = JsonSerializer.Serialize(new
            {
                items = new object[]
                {
                    new
                    {
                        id = $"{shortTag}-bug-103",
                        title = s.BugTitle,
                        language = s.Language,
                        categoryTag = s.BugCategoryTag,
                        context = s.BugContext,
                        buggyCode = s.BuggyCode,
                        bugLineNumber = s.BugLineNumber,
                        cleanFixedCode = s.CleanFixedCode,
                        requiredTokens = s.RequiredTokens,
                        functionName = s.FunctionName,
                        explanation = s.BugExplanation
                    }
                }
            }),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        return [fillAct, predictAct, bugAct];
    }

    private static ExerciseTripleSpec ResolveExerciseSpec(string path, string titleLower, string lessonTitle)
    {
        if (path.StartsWith("phase-0-orientation", StringComparison.Ordinal))
            return BuildPhase0Spec(path, titleLower, lessonTitle);
        if (path.StartsWith("phase-1-cs-foundations", StringComparison.Ordinal))
            return BuildPhase1Spec(path, titleLower, lessonTitle);
        if (path.StartsWith("phase-2-web-foundations", StringComparison.Ordinal))
            return BuildPhase2Spec(path, titleLower, lessonTitle);
        if (path.StartsWith("phase-3-javascript-mastery", StringComparison.Ordinal))
            return BuildPhase3Spec(path, titleLower, lessonTitle);
        if (path.StartsWith("phase-4-frontend-react", StringComparison.Ordinal))
            return BuildPhase4Spec(path, titleLower, lessonTitle);
        if (path.StartsWith("phase-5-backend-dotnet", StringComparison.Ordinal))
            return BuildPhase5Spec(path, titleLower, lessonTitle);
        if (path.StartsWith("phase-6-database-sql", StringComparison.Ordinal))
            return BuildPhase6Spec(path, titleLower, lessonTitle);
        if (path.StartsWith("phase-7-fullstack-integration", StringComparison.Ordinal))
            return BuildPhase7Spec(path, titleLower, lessonTitle);
        if (path.StartsWith("phase-8-testing-quality", StringComparison.Ordinal))
            return BuildPhase8Spec(path, titleLower, lessonTitle);
        if (path.StartsWith("phase-9-deployment-production", StringComparison.Ordinal) ||
            path.StartsWith("capstone-project", StringComparison.Ordinal) ||
            path.StartsWith("resources", StringComparison.Ordinal))
            return BuildPhase9AndCapstoneSpec(path, titleLower, lessonTitle);

        return BuildOrientationRootSpec(path, lessonTitle);
    }

    private static ExerciseTripleSpec BuildOrientationRootSpec(string path, string lessonTitle)
    {
        if (path.Contains("syllabus", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "bash", "os",
                "Khởi tạo cấu trúc Workspace Full-Stack chuẩn",
                "Hoàn thiện chuỗi lệnh CLI tạo thư mục dự án và kiểm tra phiên bản công cụ.",
                "Bạn mới vào Bootcamp và cần dựng nhanh khung thư mục backend/frontend đồng bộ với Syllabus.",
                [
                    "{{BLANK_0}} -p studxy-app/frontend studxy-app/backend",
                    "cd studxy-app && {{BLANK_1}} init",
                    "dotnet --version && node {{BLANK_2}}"
                ],
                [
                    new("b0", 0, "mkdir", ["mkdir"], "Lệnh tạo thư mục lồng nhau với cờ -p", ["mkdir", "touch", "cd", "ls"], "mkdir -p tự động tạo toàn bộ thư mục cha nếu chưa tồn tại."),
                    new("b1", 1, "git", ["git"], "Công cụ quản lý phiên bản cục bộ", ["git", "npm", "dotnet", "docker"], "git init khởi tạo kho lưu trữ Git cục bộ (.git)."),
                    new("b2", 2, "--version", ["--version", "-v"], "Cờ kiểm tra phiên bản Node.js", ["--version", "--help", "--info", "--list"], "node --version (hoặc -v) in ra phiên bản runtime đang cài đặt.")
                ],
                [
                    new(1, "mkdir -p studxy-app/frontend studxy-app/backend", "Tạo đồng thời hai thư mục con theo cấu trúc Monorepo.", "Nếu thiếu -p, lệnh sẽ báo lỗi khi thư mục cha studxy-app chưa tồn tại."),
                    new(2, "cd studxy-app && git init", "Chỉ khởi tạo Git sau khi lệnh cd thành công.", "Chạy git init ở thư mục gốc sai vị trí sẽ theo dõi nhầm toàn bộ ổ đĩa.")
                ],
                "Dễ",
                "Thứ tự thực thi toán tử && trên Shell",
                "Dự đoán output khi kết hợp kiểm tra biến môi trường và toán tử &&.",
                "echo \"Phase 0\"\ntrue && echo \"Node OK\"\nfalse && echo \"Skipped\"\necho \"Done\"",
                "Phase 0\nNode OK\nDone",
                ["Phase 0\nNode OK\nDone"],
                "Toán tử && chỉ chạy lệnh bên phải nếu lệnh bên trái trả về exit code 0 (true).",
                "Lệnh true thành công nên in 'Node OK'; lệnh false thất bại nên 'Skipped' bị bỏ qua.",
                "Sửa lệnh gitignore tránh commit thư mục phụ thuộc nặng",
                "CLI & Git Setup",
                "File .gitignore đang dùng sai cú pháp nên node_modules vẫn bị track vào Git.",
                "git add .\ngit commit -m \"init\"\n# .gitignore:\nnode_module",
                4,
                "git add .\ngit commit -m \"init\"\n# .gitignore:\nnode_modules/\nbin/\nobj/",
                ["node_modules", "bin/", "obj/"],
                null,
                "Tên đúng của thư mục npm là node_modules/ (có chữ s) và cần loại trừ thêm bin/, obj/ của .NET."
            );
        }

        return new ExerciseTripleSpec(
            "bash", "os",
            $"Thiết lập Git & Môi trường thực hành ({lessonTitle})",
            "Điền các lệnh CLI kiểm tra công cụ và nhánh làm việc trước khi code.",
            "Thiết lập môi trường chuẩn xác ngay từ ngày đầu giúp tránh lỗi lệch phiên bản khi làm nhóm.",
            [
                "git {{BLANK_0}} -b feat/bootcamp-start",
                "dotnet {{BLANK_1}} backend/Swms.Api.sln",
                "npm {{BLANK_2}} --prefix frontend"
            ],
            [
                new("b0", 0, "checkout", ["checkout", "switch"], "Lệnh tạo và chuyển sang nhánh mới", ["checkout", "branch", "merge", "clone"], "git checkout -b (hoặc git switch -c) vừa tạo vừa chuyển sang nhánh mới."),
                new("b1", 1, "build", ["build", "restore"], "Lệnh biên dịch solution .NET", ["build", "run", "test", "clean"], "dotnet build kiểm tra biên dịch toàn bộ project trong solution."),
                new("b2", 2, "ci", ["ci", "install"], "Cài đặt sạch package theo lockfile", ["ci", "init", "audit", "run"], "npm ci cài đúng phiên bản trong package-lock.json cho môi trường ổn định.")
            ],
            [
                new(1, "git checkout -b feat/bootcamp-start", "Tách nhánh tính năng riêng thay vì sửa thẳng trên main.", "Code trực tiếp trên main dễ gây xung đột và mất lịch sử sạch."),
                new(2, "dotnet build backend/Swms.Api.sln", "Xác minh backend biên dịch sạch trước khi chạy server.", "Bỏ qua bước build khiến lỗi cú pháp chỉ lộ khi khởi chạy.")
            ],
            "Dễ",
            "Kiểm tra Exit Code và đường dẫn tương đối",
            "Dự đoán đầu ra của đoạn script kiểm tra cấu trúc khóa học.",
            "const phases = ['phase-0', 'phase-1', 'phase-9'];\nconsole.log(phases.length);\nconsole.log(phases[phases.length - 1]);",
            "3\nphase-9",
            ["3\nphase-9"],
            "Chỉ số mảng bắt đầu từ 0 nên phần tử cuối là length - 1.",
            "Mảng có 3 phần tử nên phases.length = 3 và phases[2] = 'phase-9'.",
            "Sửa lỗi cấu hình biến môi trường bị commit nhầm",
            "Environment Hygiene",
            "Lập trình viên vô tình đưa chuỗi kết nối chứa mật khẩu vào appsettings.json thay vì dùng .env.",
            "const conn = \"Host=prod;Password=123456\";\nconsole.log(conn);",
            1,
            "const conn = process.env.DATABASE_URL ?? \"\";\nconsole.log(Boolean(conn));",
            ["process.env.DATABASE_URL"],
            null,
            "Thông tin nhạy cảm phải đọc từ biến môi trường (Environment Variables) thay vì hardcode trong mã nguồn."
        );
    }

    private static ExerciseTripleSpec BuildPhase0Spec(string path, string titleLower, string lessonTitle)
    {
        if (path.Contains("git", StringComparison.Ordinal) || titleLower.Contains("git", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "bash", "os",
                "Git Workflow: Stage, Commit & Tạo nhánh tính năng",
                "Hoàn thiện quy trình đưa file vào Staging Area, commit và tạo nhánh mới.",
                "Bạn vừa hoàn thành bài tập đầu tiên và cần lưu mốc lịch sử rõ ràng trước khi mở Pull Request.",
                [
                    "git {{BLANK_0}} -b feat/lesson-04",
                    "git {{BLANK_1}} README.md",
                    "git {{BLANK_2}} -m \"feat: complete phase 0 exercise\""
                ],
                [
                    new("b0", 0, "checkout", ["checkout", "switch"], "Tạo và chuyển sang nhánh mới", ["checkout", "merge", "pull", "status"], "git checkout -b <branch> tạo nhánh mới và chuyển HEAD sang nhánh đó."),
                    new("b1", 1, "add", ["add"], "Đưa tệp vào Staging Area", ["add", "stage", "push", "diff"], "git add đưa thay đổi vào vùng đệm chuẩn bị cho lần commit kế tiếp."),
                    new("b2", 2, "commit", ["commit"], "Ghi nhận snapshot vào lịch sử Git", ["commit", "save", "log", "tag"], "git commit -m lưu snapshot kèm thông điệp mô tả thay đổi.")
                ],
                [
                    new(1, "git checkout -b feat/lesson-04", "Tách nhánh riêng cho từng tính năng hoặc bài tập.", "Làm việc trực tiếp trên main dễ ghi đè code của đồng đội."),
                    new(2, "git add README.md", "Chỉ chọn lọc file cần commit vào Staging Area.", "Dùng git add . bừa bãi dễ đưa nhầm file log hoặc file bí mật .env.")
                ],
                "Dễ",
                "Trạng thái file trong Git Working Tree & Staging",
                "Dự đoán output khi kiểm tra trạng thái sau lệnh git add và git branch.",
                "$ git init\n$ git checkout -b feat/auth\n$ git branch --show-current",
                "feat/auth",
                ["feat/auth"],
                "git branch --show-current in ra tên nhánh hiện tại đang trỏ bởi HEAD.",
                "Sau khi chạy git checkout -b feat/auth, HEAD trỏ tới nhánh feat/auth.",
                "Sửa lỗi Push bị từ chối do chưa kéo thay đổi từ Remote",
                "Git Collaboration",
                "Đồng đội đã push commit mới lên nhánh main, lệnh git push của bạn bị reject non-fast-forward.",
                "git add .\ngit commit -m \"update\"\ngit push --force origin main",
                3,
                "git add .\ngit commit -m \"update\"\ngit pull --rebase origin main\ngit push origin main",
                ["pull", "--rebase"],
                null,
                "Không bao giờ dùng --force lên nhánh chung main; hãy dùng git pull --rebase (hoặc git pull) để đồng bộ lịch sử trước khi push."
            );
        }

        if (path.Contains("command-line", StringComparison.Ordinal) || titleLower.Contains("command", StringComparison.Ordinal) || titleLower.Contains("terminal", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "bash", "os",
                "Terminal CLI: Điều hướng thư mục & Thao tác tệp an toàn",
                "Điền các lệnh shell chuẩn để tạo thư mục, chuyển hướng và kiểm tra đường dẫn hiện tại.",
                "Kỹ sư backend thao tác phần lớn trên terminal Linux/PowerShell khi deploy server không có giao diện đồ họa.",
                [
                    "{{BLANK_0}} -p src/services",
                    "{{BLANK_1}} src/services",
                    "{{BLANK_2}}"
                ],
                [
                    new("b0", 0, "mkdir", ["mkdir"], "Lệnh tạo thư mục mới", ["mkdir", "rmdir", "touch", "cat"], "mkdir -p tạo cả cây thư mục nếu thư mục cha chưa có."),
                    new("b1", 1, "cd", ["cd"], "Lệnh thay đổi thư mục làm việc hiện tại", ["cd", "ls", "pwd", "mv"], "cd (change directory) di chuyển shell vào thư mục đích."),
                    new("b2", 2, "pwd", ["pwd"], "In ra đường dẫn tuyệt đối hiện tại", ["pwd", "ls", "whoami", "echo"], "pwd (print working directory) xác nhận vị trí hiện tại trước khi chạy lệnh nhạy cảm.")
                ],
                [
                    new(1, "mkdir -p src/services", "Tạo thư mục lồng nhau an toàn không báo lỗi nếu tồn tại.", "Thiếu -p sẽ lỗi nếu thư mục src chưa được tạo."),
                    new(3, "pwd", "Xác nhận lại đường dẫn hiện tại.", "Xóa hoặc ghi file nhầm thư mục rất nguy hiểm trên server thật.")
                ],
                "Dễ",
                "Phân biệt toán tử > (ghi đè) và >> (nối thêm)",
                "Dự đoán nội dung file log.txt sau 3 lệnh redirect.",
                "echo \"Start\" > log.txt\necho \"Step 1\" >> log.txt\necho \"Reset\" > log.txt\ncat log.txt",
                "Reset",
                ["Reset"],
                "Toán tử > đơn sẽ xóa sạch nội dung cũ của tệp trước khi ghi dòng mới.",
                "Dòng đầu ghi 'Start', dòng 2 nối 'Step 1', nhưng dòng 3 dùng > (overwrite) nên ghi đè toàn bộ file thành 'Reset'.",
                "Sửa lệnh xóa tệp tạm tránh rủi ro mất thư mục gốc",
                "Shell Safety",
                "Script dọn dẹp build đang dùng đường dẫn có khoảng trắng không bọc dấu ngoặc kép.",
                "DIR_PATH=\"./build output\"\nrm -rf $DIR_PATH/*",
                2,
                "DIR_PATH=\"./build output\"\nrm -rf \"$DIR_PATH\"/*",
                ["\"$DIR_PATH\""],
                null,
                "Luôn bọc biến đường dẫn trong dấu ngoặc kép \"$DIR_PATH\" để tránh word-splitting xóa nhầm file khi tên thư mục có khoảng trắng."
            );
        }

        return new ExerciseTripleSpec(
            "bash", "os",
            $"Môi trường Dev (.NET & Node.js): {lessonTitle}",
            "Hoàn thiện các lệnh kiểm tra runtime và chạy ứng dụng cục bộ.",
            "Trước khi bắt đầu Phase 1, cần kiểm chứng Git, Node.js và .NET SDK hoạt động trơn tru trên máy.",
            [
                "node {{BLANK_0}}",
                "dotnet {{BLANK_1}}",
                "npm run {{BLANK_2}}"
            ],
            [
                new("b0", 0, "--version", ["--version", "-v"], "Cờ in phiên bản Node.js", ["--version", "--check", "--env", "--path"], "node --version xác nhận Node.js đã nằm trong biến môi trường PATH."),
                new("b1", 1, "--info", ["--info", "--version"], "Kiểm tra chi tiết .NET SDK & Runtime", ["--info", "--help", "--build", "--new"], "dotnet --info liệt kê đầy đủ SDK, Runtime và OS environment."),
                new("b2", 2, "dev", ["dev", "build", "test"], "Chạy script phát triển frontend", ["dev", "install", "init", "upgrade"], "npm run dev khởi chạy máy chủ phát triển cục bộ.")
            ],
            [
                new(1, "node --version", "Kiểm tra nhanh phiên bản Node LTS.", "Phiên bản quá cũ có thể thiếu cú pháp ES2022+ hoặc fetch tích hợp."),
                new(2, "dotnet --info", "Xác minh .NET 10 SDK đã sẵn sàng.", "Thiếu SDK đúng phiên bản sẽ khiến dotnet build thất bại.")
            ],
            "Dễ",
            "Kiểm tra hệ nhị phân & biến PATH",
            "Dự đoán kết quả chuyển đổi nhị phân 8-bit sang hệ thập phân.",
            "const binary = \"00001101\";\nconsole.log(parseInt(binary, 2));",
            "13",
            ["13"],
            "Các bit bật là 2^3 (8) + 2^2 (4) + 2^0 (1).",
            "00001101 ở cơ số 2 bằng 8 + 4 + 1 = 13 trong hệ thập phân.",
            "Sửa lỗi không nhận lệnh dotnet/node do sai tên biến môi trường",
            "Dev Environment Setup",
            "Script kiểm tra công cụ đang gọi sai tên cờ CLI.",
            "node -version\ndotnet -version",
            1,
            "node --version\ndotnet --version",
            ["--version"],
            null,
            "Cả Node.js và .NET CLI đều dùng hai dấu gạch ngang --version (hoặc -v cho Node), dùng 1 gạch -version sẽ báo lỗi."
        );
    }

    private static ExerciseTripleSpec BuildPhase1Spec(string path, string titleLower, string lessonTitle)
    {
        if (path.Contains("variables-types", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "JavaScript: Khai báo const/let, typeof & So sánh nghiêm ngặt ===",
                "Hoàn thiện hàm chuẩn hóa giá tiền và kiểm tra kiểu dữ liệu an toàn.",
                "Trong tính toán hóa đơn, nhầm lẫn giữa chuỗi '100' và số 100 khi dùng == hoặc + sẽ gây sai lệch tổng tiền.",
                [
                    "function parseSafePrice(raw) {",
                    "  {{BLANK_0}} price = Number(raw);",
                    "  if ({{BLANK_1}} price !== 'number' || Number.isNaN(price)) return 0;",
                    "  return price {{BLANK_2}} 0 ? price : 0;",
                    "}"
                ],
                [
                    new("b0", 0, "const", ["const"], "Từ khóa khai báo hằng số block-scope không gán lại", ["const", "var", "let", "static"], "Ưu tiên const khi biến không cần tái gán giá trị."),
                    new("b1", 1, "typeof", ["typeof"], "Toán tử kiểm tra kiểu dữ liệu nguyên thủy", ["typeof", "instanceof", "type", "is"], "typeof trả về chuỗi tên kiểu như 'number', 'string', 'boolean'."),
                    new("b2", 2, ">", [">", ">="], "So sánh giá dương hợp lệ", [">", "===", "<", "!=="], "Giá trị hợp lệ phải lớn hơn 0 trước khi cộng vào hóa đơn.")
                ],
                [
                    new(2, "const price = Number(raw);", "Ép kiểu tường minh sang số trước khi tính toán.", "Cộng chuỗi '100' + 50 trong JS sẽ ra '10050' thay vì 150."),
                    new(3, "if (typeof price !== 'number' || Number.isNaN(price))", "Chặn NaN vì typeof NaN vẫn là 'number'.", "Nếu chỉ kiểm tra typeof mà quên Number.isNaN, chuỗi lỗi sẽ lọt qua.")
                ],
                "Dễ",
                "Ép kiểu ngầm định vs === và typeof null",
                "Dự đoán output của phép so sánh lỏng lẻo vs nghiêm ngặt trong JS.",
                "console.log(0 == '0', 0 === '0');\nconsole.log(typeof null);",
                "true false\nobject",
                ["true false\nobject"],
                "== tự ép kiểu còn === kiểm tra cả giá trị lẫn kiểu; typeof null là một quirk lịch sử của JS.",
                "0 == '0' là true vì '0' bị ép thành 0; 0 === '0' là false; typeof null trả về 'object'.",
                "Sửa lỗi phép cộng chuỗi thay vì cộng số từ input form",
                "JS Types & Coercion",
                "Input từ người dùng luôn là string, hàm tính tổng đang ghép chuỗi '20' + '30' thành '2030'.",
                "function sumInputs(a, b) {\n  return a + b;\n}",
                2,
                "function sumInputs(a, b) {\n  return Number(a) + Number(b);\n}",
                ["Number("],
                "sumInputs",
                "Phải ép kiểu sang Number(a) + Number(b) để thực hiện phép cộng số học thay vì nối chuỗi."
            );
        }

        if (path.Contains("control-flow", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "Control Flow: Guard Clause (Early Return) & Switch",
                "Hoàn thiện hàm phân loại hạng thành viên dùng Early Return sạch sẽ.",
                "Tránh lồng nhiều tầng if-else (Pyramid of Doom) bằng kỹ thuật Early Return.",
                [
                    "function getTierDiscount(points) {",
                    "  if (points {{BLANK_0}} 0) return 0;",
                    "  if (points >= 1000) {{BLANK_1}} 0.2;",
                    "  if (points >= 500) return 0.1;",
                    "  {{BLANK_2}} 0.05;",
                    "}"
                ],
                [
                    new("b0", 0, "<", ["<", "<="], "Kiểm tra điều kiện biên điểm âm", ["<", ">", "===", "!="], "Chặn ngay điểm âm hoặc 0 ở đầu hàm (Guard Clause)."),
                    new("b1", 1, "return", ["return"], "Trả về kết quả sớm ngay khi thỏa điều kiện", ["return", "break", "continue", "yield"], "return kết thúc hàm ngay lập tức, không cần nhánh else."),
                    new("b2", 2, "return", ["return"], "Giá trị mặc định cuối hàm", ["return", "throw", "break", "default"], "Trả về mức mặc định 5% cho các trường hợp còn lại.")
                ],
                [
                    new(2, "if (points < 0) return 0;", "Xử lý edge case ngay đầu hàm.", "Giúp phần thân chính không bị thụt lề sâu."),
                    new(3, "if (points >= 1000) return 0.2;", "Kiểm tra ngưỡng cao nhất trước ngưỡng thấp hơn.", "Nếu kiểm tra >= 500 trước, người có 1200 điểm sẽ bị nhận nhầm 10%.")
                ],
                "Dễ",
                "Lỗi Fall-through khi thiếu break trong switch",
                "Dự đoán output khi case không có lệnh break.",
                "const role = 'editor';\nlet perms = '';\nswitch (role) {\n  case 'editor': perms += 'W';\n  case 'viewer': perms += 'R'; break;\n  default: perms += 'N';\n}\nconsole.log(perms);",
                "WR",
                ["WR"],
                "Thiếu break ở case 'editor' sẽ khiến luồng chạy xuyên xuống case 'viewer'.",
                "Do case 'editor' không có break, perms nhận 'W' rồi rơi tiếp xuống case 'viewer' cộng thêm 'R' -> 'WR'.",
                "Sửa lỗi thứ tự điều kiện FizzBuzz bị nuốt nhánh",
                "Control Flow Order",
                "Điều kiện chia hết cho 3 được đặt trước chia hết cho 15 nên số 30 chỉ trả về 'Fizz'.",
                "function fizzBuzz(n) {\n  if (n % 3 === 0) return 'Fizz';\n  if (n % 15 === 0) return 'FizzBuzz';\n  return String(n);\n}",
                2,
                "function fizzBuzz(n) {\n  if (n % 15 === 0) return 'FizzBuzz';\n  if (n % 3 === 0) return 'Fizz';\n  if (n % 5 === 0) return 'Buzz';\n  return String(n);\n}",
                ["n % 15 === 0", "FizzBuzz"],
                "fizzBuzz",
                "Điều kiện ngặt nhất (n % 15 === 0) phải đặt lên đầu trước khi kiểm tra chia hết cho 3 hoặc 5."
            );
        }

        if (path.Contains("loops", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "Vòng lặp (Loops): Duyệt mảng & Tính tổng có điều kiện",
                "Hoàn thiện vòng lặp tính tổng các số chẵn dương trong mảng.",
                "Xử lý dữ liệu cảm biến: bỏ qua giá trị âm lỗi bằng continue và cộng dồn số hợp lệ.",
                [
                    "function sumEvenPositive(nums) {",
                    "  let total = 0;",
                    "  {{BLANK_0}} (const n of nums) {",
                    "    if (n <= 0 || n % 2 !== 0) {{BLANK_1}};",
                    "    total {{BLANK_2}} n;",
                    "  }",
                    "  return total;",
                    "}"
                ],
                [
                    new("b0", 0, "for", ["for"], "Từ khóa vòng lặp for...of", ["for", "while", "if", "each"], "for (const n of nums) duyệt qua từng giá trị phần tử."),
                    new("b1", 1, "continue", ["continue"], "Bỏ qua lượt lặp hiện tại chuyển sang phần tử kế", ["continue", "break", "return", "skip"], "continue chỉ bỏ qua phần tử hiện tại mà không thoát cả vòng lặp."),
                    new("b2", 2, "+=", ["+="], "Toán tử cộng dồn vào biến tích lũy", ["+=", "=", "-=", "++"], "total += n cộng thêm n vào total.")
                ],
                [
                    new(4, "if (n <= 0 || n % 2 !== 0) continue;", "Lọc bỏ số âm và số lẻ.", "Dùng break ở đây sẽ dừng hẳn vòng lặp ngay khi gặp phần tử không hợp lệ đầu tiên.")
                ],
                "Dễ",
                "break vs continue trong vòng lặp lồng",
                "Dự đoán output khi dùng continue với số chia hết cho 2 và break tại 5.",
                "const out = [];\nfor (let i = 1; i <= 6; i++) {\n  if (i % 2 === 0) continue;\n  if (i === 5) break;\n  out.push(i);\n}\nconsole.log(out.join(','));",
                "1,3",
                ["1,3"],
                "i=2,4 bị continue bỏ qua; i=5 kích hoạt break dừng vòng lặp.",
                "i=1 push 1; i=2 continue; i=3 push 3; i=4 continue; i=5 break nên kết quả là '1,3'.",
                "Sửa lỗi Off-by-One lặp quá độ dài mảng",
                "Loops & Bounds",
                "Vòng lặp for dùng <= nums.length khiến phần tử cuối bị cộng undefined thành NaN.",
                "function sumArray(nums) {\n  let sum = 0;\n  for (let i = 0; i <= nums.length; i++) {\n    sum += nums[i];\n  }\n  return sum;\n}",
                3,
                "function sumArray(nums) {\n  let sum = 0;\n  for (let i = 0; i < nums.length; i++) {\n    sum += nums[i];\n  }\n  return sum;\n}",
                ["i < nums.length"],
                "sumArray",
                "Chỉ số hợp lệ của mảng chạy từ 0 đến nums.length - 1, nên điều kiện phải là i < nums.length."
            );
        }

        if (path.Contains("functions", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "Functions: Pure Function, Default Parameter & Return",
                "Viết hàm thuần khiết tính giá sau giảm giá có tham số mặc định.",
                "Pure function giúp kết quả luôn dự đoán được và dễ viết Unit Test.",
                [
                    "function applyDiscount(price, rate {{BLANK_0}} 0.1) {",
                    "  if (price < 0) {{BLANK_1}} 0;",
                    "  return Math.round(price * (1 {{BLANK_2}} rate));",
                    "}"
                ],
                [
                    new("b0", 0, "=", ["="], "Cú pháp giá trị mặc định cho tham số", ["=", ":", "=>", "??"], "rate = 0.1 gán giá trị mặc định khi đối số là undefined."),
                    new("b1", 1, "return", ["return"], "Trả kết quả sớm khi đầu vào không hợp lệ", ["return", "break", "yield", "let"], "Trả về 0 thay vì số tiền âm."),
                    new("b2", 2, "-", ["-"], "Phép trừ tỷ lệ giảm giá", ["-", "+", "*", "/"], "Nhân với (1 - rate) để tính phần giá còn lại.")
                ],
                [
                    new(1, "function applyDiscount(price, rate = 0.1)", "Tham số mặc định ES6 gọn gàng.", "Nếu không có default, price * (1 - undefined) sẽ trả về NaN.")
                ],
                "Trung bình",
                "Hàm thuần khiết vs Tác dụng phụ (Side Effect)",
                "Dự đoán giá trị in ra của biến toàn cục và kết quả hàm.",
                "function doubleIt(x) { return x * 2; }\nlet val = 5;\nconst res = doubleIt(val);\nconsole.log(val, res);",
                "5 10",
                ["5 10"],
                "Kiểu nguyên thủy truyền theo giá trị (pass by value), biến val bên ngoài không đổi.",
                "doubleIt là pure function trả về 10, biến val vẫn giữ nguyên là 5.",
                "Sửa lỗi hàm quên từ khóa return nên trả về undefined",
                "Functions & Return",
                "Hàm tính chu vi hình chữ nhật chỉ console.log mà quên return giá trị.",
                "function calcPerimeter(w, h) {\n  const p = 2 * (w + h);\n}",
                2,
                "function calcPerimeter(w, h) {\n  return 2 * (w + h);\n}",
                ["return"],
                "calcPerimeter",
                "Hàm không có lệnh return tường minh sẽ luôn trả về undefined."
            );
        }

        if (path.Contains("arrays-and-strings", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "Arrays & Strings: Chuẩn hóa Slug URL không đột biến mảng",
                "Hoàn thiện hàm tạo slug từ tiêu đề bài học bằng chuỗi phương thức String/Array.",
                "Tạo đường dẫn thân thiện SEO (ví dụ 'Hello World' -> 'hello-world') trong CMS.",
                [
                    "function toSlug(title) {",
                    "  return title",
                    "    .{{BLANK_0}}()",
                    "    .toLowerCase()",
                    "    .{{BLANK_1}}(/\\s+/g)",
                    "    .{{BLANK_2}}('-');",
                    "}"
                ],
                [
                    new("b0", 0, "trim", ["trim"], "Loại bỏ khoảng trắng hai đầu chuỗi", ["trim", "strip", "clean", "slice"], "trim() xóa khoảng trắng thừa đầu/cuối chuỗi."),
                    new("b1", 1, "split", ["split"], "Tách chuỗi thành mảng từ theo khoảng trắng", ["split", "slice", "match", "replace"], "split(/\\s+/g) tách chuỗi thành mảng các từ."),
                    new("b2", 2, "join", ["join"], "Nối mảng các từ thành chuỗi ngăn bởi dấu gạch ngang", ["join", "concat", "merge", "toString"], "join('-') ghép các phần tử mảng bằng '-'.")
                ],
                [
                    new(3, ".trim()", "Xóa khoảng trắng đầu/cuối trước khi tách từ.", "Không trim() sẽ tạo dấu '-' thừa ở đầu hoặc cuối slug.")
                ],
                "Dễ",
                "slice (bất biến) vs splice (đột biến mảng)",
                "Dự đoán độ dài và phần tử mảng sau khi gọi slice.",
                "const arr = ['a', 'b', 'c', 'd'];\nconst part = arr.slice(1, 3);\nconsole.log(part.join('-'));\nconsole.log(arr.length);",
                "b-c\n4",
                ["b-c\n4"],
                "slice(1, 3) lấy từ index 1 đến trước index 3 và không làm thay đổi mảng arr gốc.",
                "part là ['b', 'c'] -> 'b-c', còn arr vẫn giữ đủ 4 phần tử.",
                "Sửa lỗi .reverse() làm đảo ngược nhầm mảng gốc",
                "Array Immutability",
                "Hàm getReversedCopy làm biến dạng luôn mảng gốc vì .reverse() đột biến tại chỗ.",
                "function getReversedCopy(items) {\n  return items.reverse();\n}",
                2,
                "function getReversedCopy(items) {\n  return [...items].reverse();\n}",
                ["[...items]", "reverse"],
                "getReversedCopy",
                "Dùng spread [...items] (hoặc toReversed()) để tạo bản sao trước khi đảo ngược."
            );
        }

        if (path.Contains("objects-and-classes", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "OOP JavaScript: Class, Constructor & Method",
                "Hoàn thiện lớp BankAccount đóng gói số dư và phương thức nạp tiền.",
                "Mô hình hóa thực thể nghiệp vụ với trạng thái và hành vi gắn liền trong class.",
                [
                    "class BankAccount {",
                    "  {{BLANK_0}}(owner, balance = 0) {",
                    "    {{BLANK_1}}.owner = owner;",
                    "    this.balance = balance;",
                    "  }",
                    "  deposit(amount) {",
                    "    if (amount > 0) this.balance {{BLANK_2}} amount;",
                    "  }",
                    "}"
                ],
                [
                    new("b0", 0, "constructor", ["constructor"], "Phương thức khởi tạo của class", ["constructor", "init", "new", "create"], "constructor tự động chạy khi gọi new BankAccount(...)."),
                    new("b1", 1, "this", ["this"], "Tham chiếu tới instance hiện tại", ["this", "self", "super", "that"], "this trỏ tới đối tượng vừa được khởi tạo."),
                    new("b2", 2, "+=", ["+="], "Cộng thêm số tiền nạp vào số dư", ["+=", "=", "-=", "*="], "Cập nhật số dư tích lũy an toàn.")
                ],
                [
                    new(2, "constructor(owner, balance = 0)", "Khởi tạo thuộc tính ban đầu cho object.", "Thiếu constructor khiến owner và balance không được gán khi new.")
                ],
                "Trung bình",
                "Ngữ cảnh this trong method của Class",
                "Dự đoán output khi tạo 2 instance độc lập từ class Counter.",
                "class Counter {\n  constructor() { this.n = 0; }\n  inc() { this.n++; return this.n; }\n}\nconst a = new Counter();\nconst b = new Counter();\nconsole.log(a.inc(), a.inc(), b.inc());",
                "1 2 1",
                ["1 2 1"],
                "Mỗi instance a và b có thuộc tính this.n riêng biệt trên Heap.",
                "a gọi inc() 2 lần ra 1, 2; b có n riêng nên gọi inc() ra 1 -> '1 2 1'.",
                "Sửa lỗi thiếu từ khóa this khi truy cập thuộc tính trong class",
                "OOP Class Scope",
                "Method getInfo() ném ReferenceError vì gọi trực tiếp name thay vì this.name.",
                "class Student {\n  constructor(name) { this.name = name; }\n  getInfo() { return `HV: ${name}`; }\n}",
                3,
                "class Student {\n  constructor(name) { this.name = name; }\n  getInfo() { return `HV: ${this.name}`; }\n}",
                ["this.name"],
                "Student",
                "Trong class JavaScript, phải dùng this.name để truy cập thuộc tính của instance."
            );
        }

        if (path.Contains("stack-and-queue", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "Stack (LIFO) & Queue (FIFO): Kiểm tra ngoặc hợp lệ",
                "Hoàn thiện hàm kiểm tra dấu ngoặc đóng mở cân bằng bằng cấu trúc Stack.",
                "Trình biên dịch dùng Stack để phát hiện thiếu dấu ngoặc ) ] } trong code.",
                [
                    "function isBalanced(str) {",
                    "  const stack = [];",
                    "  for (const ch of str) {",
                    "    if (ch === '(') stack.{{BLANK_0}}(ch);",
                    "    else if (ch === ')') {",
                    "      if (stack.length === 0) return {{BLANK_1}};",
                    "      stack.{{BLANK_2}}();",
                    "    }",
                    "  }",
                    "  return stack.length === 0;",
                    "}"
                ],
                [
                    new("b0", 0, "push", ["push"], "Đẩy phần tử lên đỉnh Stack", ["push", "pop", "shift", "unshift"], "push() thêm phần tử vào đỉnh Stack trong O(1)."),
                    new("b1", 1, "false", ["false"], "Trả về sai ngay nếu gặp ) khi Stack rỗng", ["false", "true", "null", "0"], "Gặp ngoặc đóng dư thừa chứng tỏ chuỗi không cân bằng."),
                    new("b2", 2, "pop", ["pop"], "Lấy phần tử ở đỉnh Stack ra theo nguyên tắc LIFO", ["pop", "shift", "push", "peek"], "pop() gỡ ngoặc mở tương ứng gần nhất trên đỉnh Stack.")
                ],
                [
                    new(4, "stack.push(ch);", "Lưu ngoặc mở để đối chiếu theo LIFO.", "Dùng shift() thay pop() sẽ biến thành Queue (FIFO), sai thứ tự lồng nhau.")
                ],
                "Trung bình",
                "LIFO (Stack) vs FIFO (Queue)",
                "Dự đoán output khi đẩy phần tử và lấy ra bằng pop() vs shift().",
                "const s = [];\ns.push(1); s.push(2); s.push(3);\nconsole.log(s.pop(), s.shift());",
                "3 1",
                ["3 1"],
                "pop() lấy phần tử cuối (LIFO = 3), shift() lấy phần tử đầu (FIFO = 1).",
                "Sau 3 lần push có [1,2,3]; pop() trả về 3, shift() trả về 1.",
                "Sửa lỗi kiểm tra thiếu stack.length === 0 cuối hàm isBalanced",
                "Stack Logic",
                "Chuỗi '((' vẫn trả về true nếu cuối hàm trả về true cứng thay vì kiểm tra stack rỗng.",
                "function hasBalancedParens(s) {\n  const st = [];\n  for (const c of s) {\n    if (c === '(') st.push(c);\n    else if (c === ')') { if (!st.pop()) return false; }\n  }\n  return true;\n}",
                7,
                "function hasBalancedParens(s) {\n  const st = [];\n  for (const c of s) {\n    if (c === '(') st.push(c);\n    else if (c === ')') { if (!st.pop()) return false; }\n  }\n  return st.length === 0;\n}",
                ["st.length === 0"],
                "hasBalancedParens",
                "Chỉ hợp lệ khi duyệt hết chuỗi và Stack không còn ngoặc mở dư (st.length === 0)."
            );
        }

        if (path.Contains("linked-list", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "Linked List: Duyệt danh sách liên kết & Đếm Node",
                "Hoàn thiện hàm duyệt qua các node trong Singly Linked List.",
                "Hiểu con trỏ next giúp nắm chắc cách cấp phát động và cấu trúc dữ liệu không liền kề.",
                [
                    "function countNodes(head) {",
                    "  let count = 0;",
                    "  let curr = {{BLANK_0}};",
                    "  while (curr !== {{BLANK_1}}) {",
                    "    count++;",
                    "    curr = curr.{{BLANK_2}};",
                    "  }",
                    "  return count;",
                    "}"
                ],
                [
                    new("b0", 0, "head", ["head"], "Bắt đầu duyệt từ node đầu tiên", ["head", "tail", "null", "0"], "Con trỏ tạm curr khởi đầu tại head để không làm mất tham chiếu gốc head."),
                    new("b1", 1, "null", ["null"], "Điều kiện dừng khi hết danh sách", ["null", "0", "false", "head"], "Node cuối cùng có next === null."),
                    new("b2", 2, "next", ["next"], "Di chuyển con trỏ sang node kế tiếp", ["next", "prev", "head", "value"], "curr = curr.next dịch chuyển con trỏ qua từng node.")
                ],
                [
                    new(6, "curr = curr.next;", "Tiến tới node kế tiếp trong mỗi bước lặp.", "Quên dòng này sẽ gây vòng lặp vô hạn (Infinite Loop).")
                ],
                "Trung bình",
                "Truy xuất con trỏ next trên Singly Linked List",
                "Dự đoán chuỗi giá trị in ra khi duyệt danh sách liên kết 3 node.",
                "const n3 = { val: 30, next: null };\nconst n2 = { val: 20, next: n3 };\nconst n1 = { val: 10, next: n2 };\nlet p = n1, res = [];\nwhile (p) { res.push(p.val); p = p.next; }\nconsole.log(res.join('->'));",
                "10->20->30",
                ["10->20->30"],
                "Duyệt từ n1 -> n2 -> n3 cho đến khi p === null.",
                "Mỗi bước đẩy p.val vào mảng và nhảy p = p.next -> '10->20->30'.",
                "Sửa lỗi mất liên kết khi chèn Node vào đầu Linked List",
                "Linked List Pointer",
                "Gán head = newNode trước khi nối newNode.next = head làm mất toàn bộ danh sách cũ.",
                "function prependNode(head, val) {\n  const newNode = { val, next: null };\n  head = newNode;\n  newNode.next = head;\n  return head;\n}",
                3,
                "function prependNode(head, val) {\n  const newNode = { val, next: head };\n  return newNode;\n}",
                ["next: head"],
                "prependNode",
                "Phải trỏ newNode.next tới head cũ trước rồi mới cập nhật head = newNode."
            );
        }

        if (path.Contains("hashmap", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "HashMap & Set: Bài toán Two Sum O(n) với Map",
                "Hoàn thiện thuật toán tìm cặp chỉ số có tổng bằng target trong O(n).",
                "Thay thế hai vòng lặp lồng nhau O(n²) bằng tra cứu bảng băm O(1) mỗi bước.",
                [
                    "function twoSum(nums, target) {",
                    "  const seen = new {{BLANK_0}}();",
                    "  for (let i = 0; i < nums.length; i++) {",
                    "    const need = target - nums[i];",
                    "    if (seen.{{BLANK_1}}(need)) return [seen.get(need), i];",
                    "    seen.{{BLANK_2}}(nums[i], i);",
                    "  }",
                    "  return [];",
                    "}"
                ],
                [
                    new("b0", 0, "Map", ["Map"], "Cấu trúc bảng băm chuẩn ES6", ["Map", "Set", "Array", "WeakSet"], "Map lưu cặp key-value với tốc độ tra cứu trung bình O(1)."),
                    new("b1", 1, "has", ["has"], "Kiểm tra số bù đã xuất hiện chưa", ["has", "get", "includes", "find"], "seen.has(need) trả về true/false kể cả khi index là 0."),
                    new("b2", 2, "set", ["set"], "Lưu giá trị và chỉ số vào Map", ["set", "add", "push", "put"], "seen.set(key, value) thêm/cập nhật khóa vào Map.")
                ],
                [
                    new(5, "if (seen.has(need))", "Dùng .has() tránh lỗi falsy khi index bằng 0.", "Nếu viết if (seen.get(need)) thì index = 0 sẽ bị coi là false!")
                ],
                "Trung bình",
                "Khử trùng phần tử với Set trong JavaScript",
                "Dự đoán output khi dùng Set để đếm số phần tử phân biệt.",
                "const ids = [101, 102, 101, 103, 102];\nconst unique = new Set(ids);\nconsole.log(unique.size, unique.has(103));",
                "3 true",
                ["3 true"],
                "Set tự động loại bỏ các giá trị trùng lặp và dùng .size (không phải .length).",
                "Các giá trị duy nhất là {101, 102, 103} nên size = 3 và has(103) = true.",
                "Sửa lỗi kiểm tra falsy với index 0 trong Two Sum",
                "HashMap Edge Case",
                "Khi phần tử đầu tiên ở index 0 là đáp án, if (map[need]) bị hiểu nhầm là false.",
                "function twoSumObj(nums, target) {\n  const map = {};\n  for (let i = 0; i < nums.length; i++) {\n    const need = target - nums[i];\n    if (map[need]) return [map[need], i];\n    map[nums[i]] = i;\n  }\n  return [];\n}",
                5,
                "function twoSumObj(nums, target) {\n  const map = {};\n  for (let i = 0; i < nums.length; i++) {\n    const need = target - nums[i];\n    if (map[need] !== undefined) return [map[need], i];\n    map[nums[i]] = i;\n  }\n  return [];\n}",
                ["!== undefined"],
                "twoSumObj",
                "Index 0 là falsy trong JS nên phải kiểm tra map[need] !== undefined (hoặc dùng Map.has())."
            );
        }

        if (path.Contains("sorting-and-searching", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "Binary Search: Tìm kiếm nhị phân O(log n)",
                "Hoàn thiện thuật toán tìm kiếm nhị phân trên mảng tăng dần.",
                "Tìm kiếm 1 bản ghi trong 1 triệu dòng chỉ cần tối đa ~20 bước so sánh thay vì 1 triệu bước.",
                [
                    "function binarySearch(arr, target) {",
                    "  let left = 0, right = arr.length - 1;",
                    "  while (left <= right) {",
                    "    const mid = Math.{{BLANK_0}}((left + right) / 2);",
                    "    if (arr[mid] === target) return mid;",
                    "    if (arr[mid] < target) left = mid {{BLANK_1}} 1;",
                    "    else right = mid {{BLANK_2}} 1;",
                    "  }",
                    "  return -1;",
                    "}"
                ],
                [
                    new("b0", 0, "floor", ["floor"], "Làm tròn xuống thành chỉ số nguyên", ["floor", "round", "ceil", "abs"], "Math.floor đảm bảo mid luôn là số nguyên hợp lệ."),
                    new("b1", 1, "+", ["+"], "Thu hẹp phạm vi sang nửa bên phải", ["+", "-", "*", "/"], "Do arr[mid] < target nên loại bỏ nửa trái, đặt left = mid + 1."),
                    new("b2", 2, "-", ["-"], "Thu hẹp phạm vi sang nửa bên trái", ["-", "+", "*", "/"], "Do arr[mid] > target nên loại bỏ nửa phải, đặt right = mid - 1.")
                ],
                [
                    new(6, "left = mid + 1;", "Bỏ qua mid đã kiểm tra để đảm bảo vòng lặp hội tụ.", "Đặt left = mid dễ gây vòng lặp vô hạn khi còn 2 phần tử.")
                ],
                "Trung bình",
                "Cẩn trọng mặc định Array.prototype.sort() trong JS",
                "Dự đoán output khi gọi .sort() không truyền hàm so sánh trên mảng số.",
                "const nums = [10, 2, 30, 5];\nnums.sort();\nconsole.log(nums.join(','));",
                "10,2,30,5",
                ["10,2,30,5"],
                "Mặc định .sort() chuyển phần tử thành chuỗi UTF-16 để so sánh ('10' < '2' < '30').",
                "Không có comparator (a, b) => a - b, JS sắp xếp theo thứ tự từ điển chuỗi: '10', '2', '30', '5'.",
                "Sửa lỗi sắp xếp số tăng dần sai thứ tự trong JS",
                "Sorting Comparator",
                "Hàm sortNumbers gọi .sort() mặc định khiến [10, 2, 5] sắp sai thành [10, 2, 5].",
                "function sortNumbers(nums) {\n  return [...nums].sort();\n}",
                2,
                "function sortNumbers(nums) {\n  return [...nums].sort((a, b) => a - b);\n}",
                ["a - b"],
                "sortNumbers",
                "Phải truyền comparator (a, b) => a - b để sắp xếp theo giá trị số học."
            );
        }

        // Big-O & Capstone-01 & fallback Phase 1
        return new ExerciseTripleSpec(
            "javascript", "react",
            $"Tối ưu Big-O & Thuật toán: {lessonTitle}",
            "Chuyển giải pháp kiểm tra trùng lặp từ O(n²) sang O(n) bằng Set.",
            "Loại bỏ vòng lặp lồng nhau để hệ thống chịu tải hàng chục nghìn phần tử mượt mà.",
            [
                "function hasDuplicate(items) {",
                "  const seen = new {{BLANK_0}}();",
                "  for (const item of items) {",
                "    if (seen.{{BLANK_1}}(item)) return true;",
                "    seen.{{BLANK_2}}(item);",
                "  }",
                "  return false;",
                "}"
            ],
            [
                new("b0", 0, "Set", ["Set"], "Tập hợp tra cứu O(1)", ["Set", "Array", "String", "Stack"], "Set lưu giá trị duy nhất với thao tác has/add O(1)."),
                new("b1", 1, "has", ["has"], "Kiểm tra phần tử đã tồn tại", ["has", "includes", "find", "get"], "seen.has(item) tránh phải quét lại từ đầu mảng."),
                new("b2", 2, "add", ["add"], "Thêm phần tử vào Set", ["add", "push", "set", "insert"], "seen.add(item) ghi nhận phần tử vừa gặp.")
            ],
            [
                new(2, "const seen = new Set();", "Đánh đổi O(n) bộ nhớ để hạ độ phức tạp thời gian từ O(n²) xuống O(n).", "Dùng Array.includes() bên trong vòng lặp vẫn là O(n²).")
            ],
            "Trung bình",
            "Đếm số bước thực thi O(n) vs O(n²)",
            "Dự đoán số bước của vòng lặp lồng nhau phụ thuộc n.",
            "let ops = 0;\nconst n = 4;\nfor (let i = 0; i < n; i++) {\n  for (let j = i + 1; j < n; j++) ops++;\n}\nconsole.log(ops);",
            "6",
            ["6"],
            "Tổng số cặp (i, j) với i < j là n*(n-1)/2 = 4*3/2.",
            "Khi n = 4, có 6 cặp (0,1),(0,2),(0,3),(1,2),(1,3),(2,3) -> O(n²).",
            "Sửa lỗi thuật toán đếm ký tự dùng vòng lặp lồng O(n²)",
            "Big-O Optimization",
            "Đếm số lần xuất hiện dùng 2 vòng for lồng nhau đang chậm khi chuỗi dài.",
            "function countUnique(arr) {\n  let c = 0;\n  for (let i = 0; i < arr.length; i++) {\n    if (arr.indexOf(arr[i]) === i) c++;\n  }\n  return c;\n}",
            4,
            "function countUnique(arr) {\n  return new Set(arr).size;\n}",
            ["Set", "size"],
            "countUnique",
            "indexOf bên trong vòng for gây độ phức tạp O(n²); dùng new Set(arr).size đạt O(n)."
        );
    }

    private static ExerciseTripleSpec BuildPhase2Spec(string path, string titleLower, string lessonTitle)
    {
        if (path.Contains("html", StringComparison.Ordinal) || path.Contains("accessibility", StringComparison.Ordinal) || path.Contains("how-web-works", StringComparison.Ordinal))
        {
            if (path.Contains("forms", StringComparison.Ordinal))
            {
                return new ExerciseTripleSpec(
                    "html", "react",
                    "HTML5 Forms: Label gắn kết & Validation gốc",
                    "Hoàn thiện biểu mẫu đăng ký có label truy cập được và ràng buộc bắt buộc.",
                    "Form chuẩn HTML5 giúp trình đọc màn hình đọc đúng nhãn và trình duyệt chặn gửi thiếu dữ liệu.",
                    [
                        "<form action=\"/api/register\" method=\"post\">",
                        "  <{{BLANK_0}} for=\"email\">Email</label>",
                        "  <input id=\"email\" type=\"{{BLANK_1}}\" name=\"email\" {{BLANK_2}} />",
                        "  <button type=\"submit\">Gửi</button>",
                        "</form>"
                    ],
                    [
                        new("b0", 0, "label", ["label"], "Thẻ nhãn gắn với input qua thuộc tính for", ["label", "span", "p", "title"], "Thuộc tính for trùng id giúp bấm vào nhãn cũng focus được ô nhập."),
                        new("b1", 1, "email", ["email"], "Kiểu nhập liệu email chuẩn HTML5", ["email", "text", "mail", "string"], "type=\"email\" tự động kiểm tra định dạng email và hiện bàn phím phù hợp trên mobile."),
                        new("b2", 2, "required", ["required"], "Thuộc tính bắt buộc nhập trước khi submit", ["required", "mandatory", "validate", "notnull"], "required ngăn submit nếu trường còn trống.")
                    ],
                    [
                        new(2, "<label for=\"email\">", "Gắn ngữ nghĩa cho ô nhập liệu.", "Chỉ dùng placeholder thay label khiến người dùng quên nội dung ô khi đang gõ.")
                    ],
                    "Dễ",
                    "Kiểm tra cấu trúc DOM và thuộc tính Form",
                    "Dự đoán chuỗi hiển thị khi phần tử có thuộc tính hidden hoặc disabled.",
                    "<button type=\"submit\" disabled>Lưu</button>",
                    "disabled=true, type=submit",
                    ["disabled=true, type=submit", "Nút bị vô hiệu hóa không thể submit"],
                    "Nút có thuộc tính boolean disabled sẽ không kích hoạt sự kiện submit.",
                    "Thuộc tính disabled vô hiệu hóa tương tác chuột và bàn phím của nút bấm.",
                    "Sửa lỗi Input thiếu liên kết label for và thiếu name khi submit",
                    "HTML5 Forms & a11y",
                    "Form gửi lên server bị thiếu trường username và không focus được khi click nhãn.",
                    "<form>\n  <span>Tên đăng nhập</span>\n  <input id=\"user\" type=\"text\" />\n</form>",
                    2,
                    "<form>\n  <label for=\"user\">Tên đăng nhập</label>\n  <input id=\"user\" name=\"username\" type=\"text\" required />\n</form>",
                    ["<label", "for=\"user\"", "name="],
                    null,
                    "Cần dùng <label for=\"user\"> thay cho <span> và bổ sung thuộc tính name + required cho <input>."
                );
            }

            if (path.Contains("accessibility", StringComparison.Ordinal))
            {
                return new ExerciseTripleSpec(
                    "html", "react",
                    "Accessibility (a11y): Semantic Button, Alt & ARIA",
                    "Hoàn thiện thẻ ảnh có mô tả alt và nút đóng modal hỗ trợ Screen Reader.",
                    "Đảm bảo người dùng bàn phím và trình đọc màn hình thao tác được 100% giao diện.",
                    [
                        "<img src=\"/hero.webp\" {{BLANK_0}}=\"Kiến trúc hệ thống Studxy\" />",
                        "<{{BLANK_1}} type=\"button\" {{BLANK_2}}=\"Đóng hộp thoại\">×</button>"
                    ],
                    [
                        new("b0", 0, "alt", ["alt"], "Văn bản thay thế cho ảnh", ["alt", "title", "desc", "aria"], "alt giúp Screen Reader đọc nội dung ảnh và hiển thị khi ảnh lỗi."),
                        new("b1", 1, "button", ["button"], "Thẻ nút ngữ nghĩa hỗ trợ Enter/Space mặc định", ["button", "div", "span", "a"], "Luôn dùng <button> cho hành động tại chỗ thay vì <div onClick>."),
                        new("b2", 2, "aria-label", ["aria-label"], "Nhãn mô tả cho nút chỉ có biểu tượng", ["aria-label", "alt", "placeholder", "role"], "Nút chỉ có dấu '×' cần aria-label để trình đọc màn hình hiểu ý nghĩa.")
                    ],
                    [
                        new(2, "<button type=\"button\" aria-label=\"Đóng hộp thoại\">", "Hỗ trợ điều hướng bàn phím (Tab/Enter/Space) và Screen Reader.", "Dùng <div> không thể Tab tới và không kích hoạt được bằng Enter.")
                    ],
                    "Dễ",
                    "Tương tác bàn phím trên phần tử Semantic",
                    "Dự đoán hành vi focus của <button> so với <div>.",
                    "<button type=\"button\">Mở menu</button>\n<!-- Tab order: 0 (focusable) -->",
                    "Focusable via Tab & Enter",
                    ["Focusable via Tab & Enter", "0"],
                    "Thẻ <button> mặc định nằm trong luồng Tab tự nhiên của trình duyệt.",
                    "<button> tự động có tabindex=0 và lắng nghe phím Enter/Space mà không cần JS bổ sung.",
                    "Sửa lỗi dùng <div> làm nút bấm gây mất truy cập bàn phím",
                    "Web Accessibility (a11y)",
                    "Nút thanh toán được viết bằng <div> nên người dùng chỉ dùng bàn phím không thể bấm được.",
                    "<div class=\"btn-pay\" onclick=\"submitOrder()\">Thanh toán</div>",
                    1,
                    "<button type=\"button\" class=\"btn-pay\" onclick=\"submitOrder()\">Thanh toán</button>",
                    ["<button", "type=\"button\""],
                    null,
                    "Thay <div> bằng thẻ ngữ nghĩa <button type=\"button\"> để đảm bảo khả năng truy cập bàn phím và Screen Reader."
                );
            }

            return new ExerciseTripleSpec(
                "html", "react",
                $"HTML5 Semantic Structure: {lessonTitle}",
                "Hoàn thiện khung trang web ngữ nghĩa chuẩn HTML5 cho SEO và Screen Reader.",
                "Thay thế 'div soup' bằng các landmark tags chuẩn như header, main, article.",
                [
                    "<{{BLANK_0}}><h1>Studxy Bootcamp</h1></header>",
                    "<{{BLANK_1}}>",
                    "  <{{BLANK_2}}><h2>Bài 01</h2><p>Nội dung...</p></article>",
                    "</main>"
                ],
                [
                    new("b0", 0, "header", ["header"], "Vùng đầu trang hoặc đầu bài viết", ["header", "head", "top", "div"], "<header> biểu thị phần mở đầu nội dung (khác với <head> chứa metadata)."),
                    new("b1", 1, "main", ["main"], "Vùng chứa nội dung chính duy nhất của trang", ["main", "body", "section", "div"], "Mỗi trang chỉ nên có 1 thẻ <main> để Screen Reader nhảy thẳng tới nội dung chính."),
                    new("b2", 2, "article", ["article"], "Khối nội dung độc lập có thể phân phối riêng", ["article", "span", "aside", "nav"], "<article> đại diện cho bài viết hoặc thẻ nội dung hoàn chỉnh độc lập.")
                ],
                [
                    new(2, "<main>", "Định vị phần nội dung trung tâm cho trình duyệt và công cụ tìm kiếm.", "Bỏ quên <main> làm giảm điểm Accessibility và SEO.")
                ],
                "Dễ",
                "Phân biệt <head> và <header> trong HTML5",
                "Dự đoán phần tử hiển thị trên màn hình người dùng.",
                "<head><title>Tab Title</title></head>\n<body><header><h1>Page Heading</h1></header></body>",
                "Page Heading",
                ["Page Heading"],
                "<head> chứa metadata không vẽ lên viewport, còn <header> nằm trong <body>.",
                "Chỉ nội dung trong <body> (bao gồm <h1>Page Heading</h1>) được hiển thị trên trang.",
                "Sửa lỗi lồng nhiều thẻ <h1> và dùng <div> cho thanh điều hướng",
                "Semantic HTML5",
                "Trang web đặt sai thẻ điều hướng bằng <div class=\"nav\"> và dùng thẻ <b> thay tiêu đề.",
                "<div class=\"nav\">\n  <a href=\"/\">Trang chủ</a>\n</div>",
                1,
                "<nav aria-label=\"Điều hướng chính\">\n  <a href=\"/\">Trang chủ</a>\n</nav>",
                ["<nav", "</nav>"],
                null,
                "Dùng thẻ <nav> để trình duyệt và công cụ hỗ trợ nhận diện đây là khu vực liên kết điều hướng chính."
            );
        }

        if (path.Contains("flexbox", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "css", "react",
                "CSS Flexbox: Căn giữa và phân bổ khoảng cách Navbar",
                "Hoàn thiện luật CSS đưa logo sang trái, menu sang phải và căn giữa theo trục dọc.",
                "Navbar ứng dụng web luôn cần Flexbox để các phần tử thẳng hàng gọn gàng.",
                [
                    ".navbar {",
                    "  display: {{BLANK_0}};",
                    "  justify-content: {{BLANK_1}};",
                    "  align-items: {{BLANK_2}};",
                    "}"
                ],
                [
                    new("b0", 0, "flex", ["flex"], "Kích hoạt mô hình bố cục Flexbox", ["flex", "grid", "block", "inline"], "display: flex biến phần tử thành Flex Container."),
                    new("b1", 1, "space-between", ["space-between"], "Đẩy các phần tử ra hai đầu trục chính", ["space-between", "center", "flex-start", "stretch"], "space-between đặt phần tử đầu sát mép trái, phần tử cuối sát mép phải."),
                    new("b2", 2, "center", ["center"], "Căn giữa theo trục phụ (cross axis)", ["center", "middle", "baseline", "auto"], "align-items: center căn thẳng hàng theo chiều dọc khi flex-direction là row.")
                ],
                [
                    new(3, "justify-content: space-between;", "Phân bố khoảng trống đều giữa các item.", "Dùng center sẽ làm logo và menu dính sát vào giữa.")
                ],
                "Dễ",
                "Thay đổi trục chính khi dùng flex-direction: column",
                "Dự đoán trục căn của justify-content và align-items khi xoay dọc.",
                ".box {\n  display: flex;\n  flex-direction: column;\n  justify-content: center; /* căn theo chiều nào? */\n}",
                "Chiều dọc (vertical)",
                ["Chiều dọc (vertical)", "vertical", "dọc"],
                "Khi flex-direction là column, Main Axis xoay thành chiều dọc.",
                "justify-content luôn bám theo Main Axis; vì vậy khi column nó căn theo chiều dọc.",
                "Sửa lỗi các thẻ card bị tràn ngang khi màn hình hẹp",
                "CSS Flexbox",
                "Hàng thẻ sản phẩm bị tràn viewport vì thiếu flex-wrap.",
                ".card-row {\n  display: flex;\n  gap: 16px;\n}",
                2,
                ".card-row {\n  display: flex;\n  flex-wrap: wrap;\n  gap: 16px;\n}",
                ["flex-wrap: wrap"],
                null,
                "Thêm flex-wrap: wrap cho phép các flex item tự xuống hàng khi không đủ chiều rộng."
            );
        }

        if (path.Contains("grid", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "css", "react",
                "CSS Grid: Lưới 3 cột đều nhau & Responsive Auto-Fit",
                "Hoàn thiện bộ lưới CSS Grid chia đều 3 cột linh hoạt bằng đơn vị fr.",
                "Dàn lưới danh sách khóa học trên Course Hub mà không cần tính phần trăm thủ công.",
                [
                    ".course-grid {",
                    "  display: {{BLANK_0}};",
                    "  grid-template-columns: {{BLANK_1}}(3, 1fr);",
                    "  {{BLANK_2}}: 24px;",
                    "}"
                ],
                [
                    new("b0", 0, "grid", ["grid"], "Kích hoạt bố cục lưới 2 chiều CSS Grid", ["grid", "flex", "table", "block"], "display: grid kích hoạt hệ thống hàng và cột 2 chiều."),
                    new("b1", 1, "repeat", ["repeat"], "Hàm lặp lại định nghĩa cột", ["repeat", "minmax", "calc", "span"], "repeat(3, 1fr) tạo 3 cột có tỷ lệ phần còn lại bằng nhau."),
                    new("b2", 2, "gap", ["gap", "grid-gap"], "Khoảng cách giữa các ô lưới", ["gap", "margin", "padding", "spacing"], "gap: 24px tạo khoảng thở giữa hàng và cột mà không bị dư mép ngoài.")
                ],
                [
                    new(3, "grid-template-columns: repeat(3, 1fr);", "Chia đều không gian khả dụng thành 3 cột.", "Nếu dùng 33.33% kèm gap 24px, tổng chiều rộng sẽ vượt 100% gây tràn ngang.")
                ],
                "Dễ",
                "Tính toán chiều rộng cột với đơn vị fr",
                "Dự đoán chiều rộng từng cột của lưới 600px với 1fr 2fr.",
                ".grid { width: 600px; display: grid; grid-template-columns: 1fr 2fr; gap: 0; }",
                "200px 400px",
                ["200px 400px"],
                "Tổng số phần là 1 + 2 = 3 phần từ 600px.",
                "1fr = 600 / 3 = 200px; 2fr = 400px.",
                "Sửa lỗi Grid bị vỡ bố cục trên màn hình nhỏ vì số cột cứng",
                "CSS Grid Responsive",
                "Lưới repeat(4, 1fr) làm các card bị bóp méo trên điện thoại.",
                ".cards {\n  display: grid;\n  grid-template-columns: repeat(4, 1fr);\n}",
                3,
                ".cards {\n  display: grid;\n  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));\n  gap: 16px;\n}",
                ["auto-fit", "minmax("],
                null,
                "Kết hợp repeat(auto-fit, minmax(260px, 1fr)) giúp lưới tự động co giãn số cột theo độ rộng khung nhìn."
            );
        }

        // Box Model / Responsive / Animation / BEM / Modern CSS
        return new ExerciseTripleSpec(
            "css", "react",
            $"CSS Layout & Responsive: {lessonTitle}",
            "Hoàn thiện chuẩn Box Model và Media Query Mobile-First.",
            "Tránh việc padding và border làm phình kích thước thực tế của khối giao diện.",
            [
                "* { box-sizing: {{BLANK_0}}; }",
                ".container { max-width: 1200px; margin: 0 {{BLANK_1}}; }",
                "@{{BLANK_2}} (min-width: 768px) { .container { padding: 24px; } }"
            ],
            [
                new("b0", 0, "border-box", ["border-box"], "Tính cả padding và border vào trong width/height", ["border-box", "content-box", "padding-box", "inherit"], "box-sizing: border-box giúp width: 100% không bị tràn khi thêm padding."),
                new("b1", 1, "auto", ["auto"], "Tự động cân bằng lề trái phải để căn giữa khối", ["auto", "0", "center", "100%"], "margin: 0 auto căn giữa khối block có chiều rộng xác định."),
                new("b2", 2, "media", ["media"], "Quy tắc responsive theo kích thước màn hình", ["media", "keyframes", "import", "supports"], "@media (min-width: 768px) áp dụng kiểu dáng từ tablet trở lên (Mobile-first).")
            ],
            [
                new(1, "* { box-sizing: border-box; }", "Thiết lập reset chuẩn cho mọi dự án CSS hiện đại.", "Mặc định content-box cộng dồn padding + border ra ngoài width gây vỡ layout.")
            ],
            "Dễ",
            "Tính tổng chiều rộng Box Model: content-box vs border-box",
            "Dự đoán chiều rộng hiển thị thực tế của phần tử .card.",
            ".card {\n  box-sizing: border-box;\n  width: 300px;\n  padding: 20px;\n  border: 2px solid #000;\n}",
            "300px",
            ["300px"],
            "Với border-box, padding và border được tính gộp vào bên trong 300px.",
            "Nhờ box-sizing: border-box, tổng chiều rộng vẫn đúng bằng 300px.",
            "Sửa lỗi Box bị tràn 100% chiều rộng do dùng content-box mặc định",
            "CSS Box Model",
            "Khối .panel đặt width: 100% kèm padding: 24px bị xuất hiện thanh cuộn ngang.",
            ".panel {\n  width: 100%;\n  padding: 24px;\n  box-sizing: content-box;\n}",
            4,
            ".panel {\n  width: 100%;\n  padding: 24px;\n  box-sizing: border-box;\n}",
            ["border-box"],
            null,
            "Chuyển sang box-sizing: border-box để phần padding nằm gọn bên trong 100% chiều rộng."
        );
    }

    private static ExerciseTripleSpec BuildPhase3Spec(string path, string titleLower, string lessonTitle)
    {
        if (path.Contains("typescript", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "typescript", "react",
                "TypeScript: Interface, Union Type & Generic ApiResponse",
                "Hoàn thiện interface mô tả dữ liệu trả về từ API với Generic <T>.",
                "Ngăn lỗi undefined/sai tên trường ngay lúc biên dịch trước khi chạy trình duyệt.",
                [
                    "interface ApiResponse<{{BLANK_0}}> {",
                    "  status: 'ok' | 'error';",
                    "  data: {{BLANK_1}};",
                    "}",
                    "function unwrap<T>(res: ApiResponse<T>): T { {{BLANK_2}} res.data; }"
                ],
                [
                    new("b0", 0, "T", ["T"], "Tham số kiểu Generic đại diện cho kiểu dữ liệu động", ["T", "any", "void", "object"], "Dùng T thay cho any để bảo toàn type-safety khi gọi hàm."),
                    new("b1", 1, "T", ["T"], "Gán kiểu T cho trường data", ["T", "unknown", "any", "string"], "Trường data mang đúng kiểu được truyền vào ApiResponse<User>."),
                    new("b2", 2, "return", ["return"], "Trả về dữ liệu đã định kiểu", ["return", "yield", "throw", "export"], "Trả về res.data có kiểu chính xác là T.")
                ],
                [
                    new(1, "interface ApiResponse<T>", "Khuôn mẫu tái sử dụng cho mọi endpoint API.", "Dùng any làm mất toàn bộ kiểm tra tĩnh của TypeScript.")
                ],
                "Trung bình",
                "Type Narrowing với Union Type trong TypeScript",
                "Dự đoán output khi thu hẹp kiểu union string | number.",
                "function formatId(id) {\n  return typeof id === 'number' ? `#${id}` : id.toUpperCase();\n}\nconsole.log(formatId(7), formatId('ab'));",
                "#7 AB",
                ["#7 AB"],
                "typeof phân nhánh rành mạch giữa number và string.",
                "formatId(7) -> '#7'; formatId('ab') -> 'AB'.",
                "Sửa lỗi lạm dụng kiểu any làm bỏ qua kiểm tra thuộc tính",
                "TypeScript Type Safety",
                "Hàm nhận user: any khiến gõ nhầm user.emial không bị báo lỗi lúc build.",
                "function getEmail(user: any): string {\n  return user.emial;\n}",
                1,
                "interface User { email: string; }\nfunction getEmail(user: User): string {\n  return user.email;\n}",
                ["interface", "email: string"],
                "getEmail",
                "Định nghĩa interface User { email: string } giúp TypeScript phát hiện lỗi gõ sai tên thuộc tính ngay lập tức."
            );
        }

        if (path.Contains("promises", StringComparison.Ordinal) || path.Contains("async-await", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "Async/Await & Promise.all: Gọi API song song an toàn",
                "Hoàn thiện hàm tải đồng thời thông tin khóa học và tiến độ học viên.",
                "Chạy tuần tự 2 lệnh await mất gấp đôi thời gian mạng so với Promise.all.",
                [
                    "async function loadCourseBundle(courseId) {",
                    "  {{BLANK_0}} {",
                    "    const [course, progress] = await Promise.{{BLANK_1}}([",
                    "      fetchCourse(courseId),",
                    "      fetchProgress(courseId)",
                    "    ]);",
                    "    return { course, progress };",
                    "  } catch (err) {",
                    "    console.error(err);",
                    "    {{BLANK_2}} null;",
                    "  }",
                    "}"
                ],
                [
                    new("b0", 0, "try", ["try"], "Bắt ngoại lệ cho mã async/await", ["try", "catch", "if", "finally"], "try/catch bắt lỗi promise bị reject khi dùng await."),
                    new("b1", 1, "all", ["all"], "Chạy song song các promise độc lập", ["all", "race", "any", "resolve"], "Promise.all thực thi đồng thời tất cả tác vụ bất đồng bộ."),
                    new("b2", 2, "return", ["return"], "Trả về giá trị an toàn khi lỗi", ["return", "throw", "break", "await"], "Xử lý fallback rõ ràng trong khối catch.")
                ],
                [
                    new(3, "await Promise.all([...])", "Giảm tổng độ trễ từ T1 + T2 xuống max(T1, T2).", "Await nối tiếp hai tác vụ không phụ thuộc nhau gây chậm UI.")
                ],
                "Nâng cao",
                "Event Loop: Call Stack vs Microtask Queue vs Macrotask Queue",
                "Dự đoán chính xác thứ tự 4 dòng in ra Console.",
                "console.log('1');\nsetTimeout(() => console.log('2'), 0);\nPromise.resolve().then(() => console.log('3'));\nconsole.log('4');",
                "1\n4\n3\n2",
                ["1\n4\n3\n2"],
                "Đồng bộ chạy trước -> Microtask (Promise) -> Macrotask (setTimeout).",
                "1 và 4 chạy đồng bộ trước; Microtask Queue xả '3'; cuối cùng Macrotask Queue chạy '2'.",
                "Sửa lỗi quên từ khóa await khiến hàm trả về [object Promise]",
                "Async/Await",
                "Biến data nhận giá trị Promise pending thay vì JSON thực tế.",
                "async function getProfile() {\n  const res = fetch('/api/me');\n  return res.json();\n}",
                2,
                "async function getProfile() {\n  const res = await fetch('/api/me');\n  if (!res.ok) throw new Error('HTTP ' + res.status);\n  return await res.json();\n}",
                ["await fetch", "res.ok"],
                "getProfile",
                "fetch() và res.json() đều trả về Promise nên bắt buộc phải dùng await và kiểm tra res.ok."
            );
        }

        if (path.Contains("dom", StringComparison.Ordinal) || path.Contains("events", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "javascript", "react",
                "DOM & Event Delegation: Xử lý danh sách hiệu năng cao",
                "Hoàn thiện Event Delegation gắn 1 listener duy nhất trên phần tử cha.",
                "Khi danh sách có 500 dòng, gắn 500 addEventListener tốn RAM; dùng bubbling trên <ul> tối ưu hơn.",
                [
                    "const list = document.{{BLANK_0}}('#todo-list');",
                    "list.{{BLANK_1}}('click', (e) => {",
                    "  const btn = e.target.{{BLANK_2}}('.delete-btn');",
                    "  if (btn) btn.parentElement.remove();",
                    "});"
                ],
                [
                    new("b0", 0, "querySelector", ["querySelector"], "Truy vấn phần tử DOM theo CSS selector", ["querySelector", "getElement", "select", "find"], "querySelector trả về phần tử đầu tiên khớp bộ chọn."),
                    new("b1", 1, "addEventListener", ["addEventListener"], "Đăng ký sự kiện click trên phần tử cha", ["addEventListener", "onClick", "listen", "bind"], "addEventListener cho phép gắn nhiều handler và hỗ trợ Event Bubbling."),
                    new("b2", 2, "closest", ["closest"], "Tìm phần tử tổ tiên khớp selector kể cả khi click vào icon con", ["closest", "matches", "parent", "find"], "e.target.closest('.delete-btn') xử lý chuẩn khi trong nút có thẻ SVG/span.")
                ],
                [
                    new(3, "e.target.closest('.delete-btn')", "Bắt đúng nút ngay cả khi người dùng bấm vào icon bên trong nút.", "Chỉ kiểm tra e.target.classList sẽ trượt nếu click vào icon con.")
                ],
                "Trung bình",
                "Closure & Phạm vi biến let trong vòng lặp",
                "Dự đoán output khi tạo 3 callback với let vs var.",
                "const fns = [];\nfor (let i = 0; i < 3; i++) {\n  fns.push(() => i);\n}\nconsole.log(fns.map(fn => fn()).join('-'));",
                "0-1-2",
                ["0-1-2"],
                "Biến let tạo một Lexical Binding mới cho mỗi vòng lặp.",
                "Mỗi closure nhớ giá trị i riêng của vòng lặp đó nên kết quả là '0-1-2' (nếu dùng var sẽ ra '3-3-3').",
                "Sửa lỗi XSS do dùng innerHTML với dữ liệu người dùng",
                "DOM Security (XSS)",
                "Hiển thị tên bình luận bằng innerHTML cho phép kẻ tấn công chèn thẻ <img onerror=...>.",
                "function renderComment(el, text) {\n  el.innerHTML = text;\n}",
                2,
                "function renderComment(el, text) {\n  el.textContent = text;\n}",
                ["textContent"],
                "renderComment",
                "Luôn dùng textContent (hoặc innerText) khi chèn chuỗi văn bản thuần từ người dùng để vô hiệu hóa kịch bản XSS."
            );
        }

        // Closures / Array Methods / ES6+ / OOP / Functional / Regex / Web APIs
        return new ExerciseTripleSpec(
            "javascript", "react",
            $"JavaScript Mastery: {lessonTitle}",
            "Hoàn thiện pipeline biến đổi dữ liệu bất biến với filter, map và reduce.",
            "Tính tổng doanh thu các đơn đã thanh toán mà không thay đổi mảng đơn hàng gốc.",
            [
                "function calcPaidTotal(orders) {",
                "  return orders",
                "    .{{BLANK_0}}(o => o.paid)",
                "    .{{BLANK_1}}(o => o.amount)",
                "    .{{BLANK_2}}((sum, x) => sum + x, 0);",
                "}"
            ],
            [
                new("b0", 0, "filter", ["filter"], "Lọc các đơn hàng có paid === true", ["filter", "find", "some", "every"], "filter() giữ lại mọi phần tử thỏa điều kiện."),
                new("b1", 1, "map", ["map"], "Trích xuất giá trị số tiền từng đơn", ["map", "forEach", "flat", "slice"], "map() biến mỗi object đơn hàng thành con số amount."),
                new("b2", 2, "reduce", ["reduce"], "Cộng gộp mảng số thành 1 tổng duy nhất", ["reduce", "sum", "join", "fold"], "reduce((acc, cur) => acc + cur, 0) có giá trị khởi tạo 0 an toàn cho mảng rỗng.")
            ],
            [
                new(5, ".reduce((sum, x) => sum + x, 0)", "Truyền giá trị khởi tạo 0.", "Thiếu 0 khiến mảng rỗng ném TypeError: Reduce of empty array with no initial value.")
            ],
            "Trung bình",
            "Closure bảo toàn trạng thái riêng tư",
            "Dự đoán output khi gọi closure bộ đếm tạo từ createCounter.",
            "function createCounter(start = 10) {\n  let n = start;\n  return () => ++n;\n}\nconst next = createCounter(5);\nconsole.log(next(), next());",
            "6 7",
            ["6 7"],
            "Hàm con giữ tham chiếu tới biến n trong Lexical Scope của createCounter.",
            "++n tăng trước rồi trả về: lần 1 ra 6, lần 2 ra 7.",
            "Sửa lỗi thiếu initialValue trong reduce khi mảng rỗng",
            "ES6 Array Methods",
            "Hàm sumPrices bị crash TypeError khi giỏ hàng chưa có sản phẩm nào.",
            "function sumPrices(items) {\n  return items.reduce((acc, item) => acc + item.price);\n}",
            2,
            "function sumPrices(items) {\n  return items.reduce((acc, item) => acc + item.price, 0);\n}",
            [", 0"],
            "sumPrices",
            "Truyền 0 làm tham số thứ 2 (initialValue) của reduce giúp hàm trả về 0 an toàn khi items rỗng."
        );
    }

    private static ExerciseTripleSpec BuildPhase4Spec(string path, string titleLower, string lessonTitle)
    {
        if (path.Contains("useeffect", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "typescript", "react",
                "React useEffect: Cleanup Timer & Dependency Array",
                "Hoàn thiện hook debounce từ khóa tìm kiếm có dọn dẹp timer khi unmount.",
                "Ngăn Memory Leak và race condition khi người dùng gõ nhanh hoặc rời trang.",
                [
                    "useEffect(() => {",
                    "  const timer = setTimeout(() => onSearch(query), 300);",
                    "  {{BLANK_0}} () => {{BLANK_1}}(timer);",
                    "}, [{{BLANK_2}}, onSearch]);"
                ],
                [
                    new("b0", 0, "return", ["return"], "Trả về hàm cleanup từ useEffect", ["return", "finally", "await", "export"], "React gọi hàm trả về của useEffect trước lần chạy kế tiếp và khi unmount."),
                    new("b1", 1, "clearTimeout", ["clearTimeout"], "Hủy timer cũ chưa kịp chạy", ["clearTimeout", "clearInterval", "abort", "cancel"], "clearTimeout hủy yêu cầu tìm kiếm cũ nếu người dùng tiếp tục gõ."),
                    new("b2", 2, "query", ["query"], "Biến phụ thuộc kích hoạt effect khi đổi", ["query", "timer", "setTimeout", "null"], "Mỗi khi query thay đổi, effect chạy lại để đặt timer mới.")
                ],
                [
                    new(3, "return () => clearTimeout(timer);", "Hủy timer tồn đọng tránh gọi setState trên component đã unmount.", "Không cleanup sẽ gây gửi hàng loạt API thừa cho từng phím gõ.")
                ],
                "Trung bình",
                "Vòng đời chạy lại của useEffect và Cleanup",
                "Dự đoán thứ tự log khi component mount với state count.",
                "console.log('1. Render');\n// React commit DOM xong mới chạy effect\nPromise.resolve().then(() => console.log('2. Effect'));",
                "1. Render\n2. Effect",
                ["1. Render\n2. Effect"],
                "useEffect luôn chạy bất đồng bộ sau khi trình duyệt đã vẽ xong DOM.",
                "Render chạy trước để tạo JSX, sau đó React mới kích hoạt callback của useEffect.",
                "Sửa lỗi vòng lặp vô hạn trong useEffect do thiếu Dependency Array",
                "React Hooks",
                "Gọi setUsers bên trong useEffect không có dependency array khiến component re-render vô tận.",
                "useEffect(() => {\n  fetch('/api/users').then(r => r.json()).then(setUsers);\n});",
                3,
                "useEffect(() => {\n  fetch('/api/users').then(r => r.json()).then(setUsers);\n}, []);",
                ["}, []);"],
                null,
                "Thêm mảng phụ thuộc rỗng [] để effect chỉ fetch dữ liệu một lần duy nhất sau khi mount."
            );
        }

        if (path.Contains("state-usestate", StringComparison.Ordinal) || path.Contains("lists-conditional", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "typescript", "react",
                "React State Immutability & Danh sách Key ổn định",
                "Cập nhật trạng thái đếm bằng functional updater và render danh sách với key chuẩn.",
                "Trong React, đột biến trực tiếp mảng state hoặc dùng index làm key khi xóa sẽ gây lỗi UI.",
                [
                    "const [count, setCount] = {{BLANK_0}}<number>(0);",
                    "const incrementTwice = () => {",
                    "  setCount({{BLANK_1}} => prev + 1);",
                    "  setCount(prev => prev + 1);",
                    "};",
                    "// Render: items.map(item => <li {{BLANK_2}}={item.id}>{item.name}</li>)"
                ],
                [
                    new("b0", 0, "useState", ["useState"], "Hook khai báo state cục bộ", ["useState", "useRef", "useMemo", "useEffect"], "useState<number>(0) khởi tạo state có kiểu number."),
                    new("b1", 1, "prev", ["prev"], "Tham số nhận giá trị state mới nhất trong hàng đợi", ["prev", "count", "state", "next"], "Functional updater prev => prev + 1 tránh stale closure khi gọi nhiều lần liên tiếp."),
                    new("b2", 2, "key", ["key"], "Thuộc tính giúp React nhận diện từng phần tử danh sách", ["key", "id", "index", "ref"], "key={item.id} ổn định giúp Virtual DOM diff chính xác khi thêm/xóa.")
                ],
                [
                    new(3, "setCount(prev => prev + 1);", "Đọc giá trị mới nhất thay vì giá trị chụp tại lần render hiện tại.", "Gọi setCount(count + 1) 2 lần chỉ tăng tổng cộng 1 đơn vị.")
                ],
                "Trung bình",
                "State Batching trong React 18/19",
                "Dự đoán giá trị count mới sau 2 lần gọi setCount(count + 1).",
                "let count = 0;\nconst queued = [count + 1, count + 1]; // cả 2 đều đọc count = 0\nconsole.log(queued[queued.length - 1]);",
                "1",
                ["1"],
                "Trong cùng một lần render, biến count không đổi ngay lập tức sau lệnh setCount.",
                "Cả hai lệnh đều tính 0 + 1 = 1; muốn tăng 2 phải dùng prev => prev + 1.",
                "Sửa lỗi đột biến mảng state bằng .push() khiến React không re-render",
                "React State Immutability",
                "Hàm addTask dùng tasks.push(newTask) giữ nguyên tham chiếu mảng nên UI không cập nhật.",
                "function handleAdd(tasks: string[], setTasks: (v: string[]) => void, next: string) {\n  tasks.push(next);\n  setTasks(tasks);\n}",
                2,
                "function handleAdd(tasks: string[], setTasks: (v: string[]) => void, next: string) {\n  setTasks([...tasks, next]);\n}",
                ["[...tasks, next]"],
                null,
                "Phải tạo tham chiếu mảng mới bằng spread [...tasks, next] để React nhận biết state đã thay đổi."
            );
        }

        // General React + TS (JSX, Props, Forms, Router, Custom Hooks, Context, Performance, Tailwind)
        return new ExerciseTripleSpec(
            "typescript", "react",
            $"React & TypeScript: {lessonTitle}",
            "Hoàn thiện Controlled Form Component có xử lý submit và ngăn tải lại trang.",
            "Quản lý input qua React State và TypeScript event type chuẩn xác.",
            [
                "function LoginForm({ onSubmit }: { onSubmit: (email: string) => void }) {",
                "  const [email, setEmail] = useState('');",
                "  const handleSubmit = (e: React.{{BLANK_0}}) => {",
                "    e.{{BLANK_1}}();",
                "    onSubmit(email);",
                "  };",
                "  return <form onSubmit={handleSubmit}><input {{BLANK_2}}={email} onChange={e => setEmail(e.target.value)} /></form>;",
                "}"
            ],
            [
                new("b0", 0, "FormEvent", ["FormEvent", "FormEvent<HTMLFormElement>"], "Kiểu sự kiện submit form trong React TypeScript", ["FormEvent", "ChangeEvent", "MouseEvent", "Event"], "React.FormEvent cung cấp kiểu cho sự kiện gửi biểu mẫu."),
                new("b1", 1, "preventDefault", ["preventDefault"], "Ngăn trình duyệt reload toàn bộ trang khi submit", ["preventDefault", "stopPropagation", "cancel", "reset"], "Mặc định form HTML tải lại trang và mất toàn bộ state SPA."),
                new("b2", 2, "value", ["value"], "Ràng buộc hai chiều cho Controlled Component", ["value", "defaultValue", "text", "model"], "Truyền value={email} giúp React làm Single Source of Truth cho ô nhập.")
            ],
            [
                new(4, "e.preventDefault();", "Giữ nguyên trạng thái ứng dụng SPA khi submit form.", "Quên gọi preventDefault khiến trang reload mất dữ liệu.")
            ],
            "Trung bình",
            "Tối ưu Re-render với React.memo / useMemo",
            "Dự đoán hành vi tính toán lại của useMemo khi dependency không đổi.",
            "const depsA = 'react';\nconst depsB = 'react';\nconsole.log(Object.is(depsA, depsB));",
            "true",
            ["true"],
            "React so sánh các dependency bằng Object.is().",
            "Chuỗi 'react' bằng chính nó qua Object.is nên useMemo trả về giá trị cache.",
            "Sửa lỗi dùng index làm key trong danh sách có thể xóa phần tử",
            "React Rendering & Keys",
            "Dùng idx làm key khiến state bên trong <Row /> bị dính nhầm sang phần tử khác khi xóa đầu mảng.",
            "{items.map((item, idx) => (\n  <CourseCard key={idx} title={item.title} />\n))}",
            2,
            "{items.map((item) => (\n  <CourseCard key={item.id} title={item.title} />\n))}",
            ["key={item.id}"],
            null,
            "Dùng mã định danh duy nhất ổn định item.id làm key thay cho chỉ số mảng idx."
        );
    }

    private static ExerciseTripleSpec BuildPhase5Spec(string path, string titleLower, string lessonTitle)
    {
        if (path.Contains("dependency-injection", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "csharp", "dotnet",
                "ASP.NET Core DI: Vòng đời Scoped, Transient & Singleton",
                "Đăng ký đúng vòng đời cho DbContext, Service nghiệp vụ và Cache dùng chung.",
                "Đăng ký nhầm DbContext thành Singleton hoặc tiêm Scoped vào Singleton (Captive Dependency) sẽ gây crash dưới tải đa luồng.",
                [
                    "builder.Services.Add{{BLANK_0}}<SwmsDbContext>(opt => opt.UseNpgsql(conn));",
                    "builder.Services.Add{{BLANK_1}}<ICourseService, CourseService>();",
                    "builder.Services.Add{{BLANK_2}}<ITokenCache, MemoryTokenCache>();"
                ],
                [
                    new("b0", 0, "DbContext", ["DbContext", "Scoped"], "Đăng ký DbContext mặc định theo vòng đời Scoped", ["DbContext", "Singleton", "Transient", "HostedService"], "AddDbContext đăng ký DbContext với vòng đời Scoped (mỗi HTTP request 1 instance)."),
                    new("b1", 1, "Scoped", ["Scoped"], "Service dùng chung DbContext trong 1 HTTP Request", ["Scoped", "Singleton", "Transient", "Factory"], "Service phụ thuộc vào DbContext bắt buộc phải là Scoped (hoặc Transient), tuyệt đối không được là Singleton."),
                    new("b2", 2, "Singleton", ["Singleton"], "Dịch vụ bộ nhớ đệm tồn tại xuyên suốt vòng đời app", ["Singleton", "Scoped", "Transient", "Local"], "Cache dùng chung giữa mọi request cần AddSingleton và phải thread-safe.")
                ],
                [
                    new(2, "builder.Services.AddScoped<ICourseService, CourseService>();", "Đồng bộ vòng đời với DbContext trong cùng HTTP Request.", "Đăng ký Singleton mà tiêm DbContext vào sẽ gây lỗi Captive Dependency.")
                ],
                "Trung bình",
                "Số lần khởi tạo Instance theo DI Lifetime",
                "Dự đoán số instance tạo ra qua 2 HTTP Request (mỗi request resolve service 2 lần).",
                "// Singleton: 1 instance toàn app\n// Scoped: 1 instance / request\n// Transient: mỗi lần inject tạo mới\nConsole.WriteLine(\"Singleton=1, Scoped=2, Transient=4\");",
                "Singleton=1, Scoped=2, Transient=4",
                ["Singleton=1, Scoped=2, Transient=4"],
                "2 requests x 2 lần resolve mỗi request = 4 lần resolve tổng cộng.",
                "Singleton tạo 1 lần; Scoped tạo 1 lần mỗi request (2); Transient tạo mới mỗi lần resolve (4).",
                "Sửa lỗi Captive Dependency: tiêm Scoped DbContext vào Singleton Service",
                "ASP.NET Core DI",
                "Background/Singleton service đang giữ trực tiếp SwmsDbContext trong constructor gây lỗi concurrency và stale data.",
                "public class StatsCacheService(SwmsDbContext db)\n{\n    public Task<int> CountAsync() => db.Courses.CountAsync();\n}",
                1,
                "public class StatsCacheService(IServiceScopeFactory scopeFactory)\n{\n    public async Task<int> CountAsync() {\n        using var scope = scopeFactory.CreateScope();\n        var db = scope.ServiceProvider.GetRequiredService<SwmsDbContext>();\n        return await db.Courses.CountAsync();\n    }\n}",
                ["IServiceScopeFactory", "CreateScope"],
                "StatsCacheService",
                "Singleton không được giữ trực tiếp Scoped DbContext; phải inject IServiceScopeFactory và tạo scope ngắn hạn khi truy vấn."
            );
        }

        if (path.Contains("middleware", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "csharp", "dotnet",
                "ASP.NET Core Middleware Pipeline & Short-Circuit",
                "Hoàn thiện Middleware đo thời gian xử lý và trả 401 khi thiếu Header xác thực.",
                "Mọi request HTTP đều đi qua chuỗi Middleware theo thứ tự đăng ký trong Program.cs.",
                [
                    "app.{{BLANK_0}}(async (context, next) =>",
                    "{",
                    "    if (!context.Request.Headers.ContainsKey(\"X-Api-Key\")) {",
                    "        context.Response.StatusCode = {{BLANK_1}};",
                    "        return;",
                    "    }",
                    "    await {{BLANK_2}}();",
                    "});"
                ],
                [
                    new("b0", 0, "Use", ["Use"], "Đăng ký middleware có gọi bước kế tiếp", ["Use", "Run", "Map", "Get"], "app.Use cho phép chạy logic trước/sau next(); app.Run sẽ ngắt luôn pipeline."),
                    new("b1", 1, "401", ["401", "StatusCodes.Status401Unauthorized"], "Mã HTTP Unauthorized khi chưa xác thực", ["401", "200", "404", "500"], "Ngắt luồng (short-circuit) bằng cách đặt 401 và return không gọi next()."),
                    new("b2", 2, "next", ["next"], "Chuyển tiếp request sang middleware tiếp theo", ["next", "continue", "forward", "invoke"], "await next() bàn giao quyền xử lý xuống phần còn lại của pipeline.")
                ],
                [
                    new(5, "return;", "Short-circuit ngay sau khi đặt StatusCode 401.", "Nếu vẫn gọi await next() sau khi ghi response sẽ ném InvalidOperationException.")
                ],
                "Trung bình",
                "Thứ tự thực thi In-Out của Middleware Pipeline",
                "Dự đoán chuỗi log in ra khi M1 gọi next() sang M2.",
                "// M1: log 'A', await next(), log 'D'\n// M2: log 'B', await next(), log 'C'\nConsole.WriteLine(\"A -> B -> C -> D\");",
                "A -> B -> C -> D",
                ["A -> B -> C -> D"],
                "Pipeline đi xuôi lúc vào (M1 -> M2) và đi ngược lúc ra (M2 -> M1).",
                "M1 vào ('A') -> M2 vào ('B') -> M2 ra ('C') -> M1 ra ('D').",
                "Sửa lỗi thứ tự UseAuthorization đặt trước UseAuthentication",
                "ASP.NET Core Pipeline",
                "Endpoint [Authorize] luôn trả về 401 dù client gửi JWT hợp lệ vì middleware sai thứ tự.",
                "app.UseAuthorization();\napp.UseAuthentication();\napp.MapControllers();",
                1,
                "app.UseAuthentication();\napp.UseAuthorization();\napp.MapControllers();",
                ["UseAuthentication", "UseAuthorization"],
                null,
                "Phải gọi app.UseAuthentication() để giải mã danh tính User trước khi app.UseAuthorization() kiểm tra quyền."
            );
        }

        if (path.Contains("csharp-linq", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "csharp", "dotnet",
                "C# LINQ: Lọc, Chiếu DTO & Thực thi trì hoãn",
                "Hoàn thiện chuỗi truy vấn LINQ lọc bài học đã xuất bản và chuyển sang DTO.",
                "Dùng Select để chỉ kéo đúng các cột cần thiết thay vì toàn bộ cột nặng của bảng.",
                [
                    "var publishedDtos = lessons",
                    "    .{{BLANK_0}}(l => l.IsPublished)",
                    "    .{{BLANK_1}}(l => l.SortOrder)",
                    "    .{{BLANK_2}}(l => new LessonDto(l.Id, l.Title));"
                ],
                [
                    new("b0", 0, "Where", ["Where"], "Lọc các phần tử thỏa điều kiện", ["Where", "Filter", "Find", "Any"], "Where() tạo biểu thức lọc điều kiện."),
                    new("b1", 1, "OrderBy", ["OrderBy"], "Sắp xếp tăng dần theo SortOrder", ["OrderBy", "Sort", "OrderByDescending", "GroupBy"], "OrderBy() sắp xếp tăng dần theo khóa chỉ định."),
                    new("b2", 2, "Select", ["Select"], "Ánh xạ sang kiểu DTO gọn nhẹ", ["Select", "Map", "Project", "Convert"], "Select() chuyển đổi thực thể thành DTO.")
                ],
                [
                    new(4, ".Select(l => new LessonDto(l.Id, l.Title))", "Chỉ lấy 2 cột cần thiết giảm băng thông DB.", "Kéo cả cột Markdown dài hàng chục KB khi chỉ cần danh sách tiêu đề gây chậm API.")
                ],
                "Trung bình",
                "Deferred Execution (Thực thi trì hoãn) trong LINQ",
                "Dự đoán output khi thêm phần tử vào list trước khi gọi ToList().",
                "var nums = new List<int> { 1, 2 };\nvar query = nums.Where(x => x > 1);\nnums.Add(3);\nConsole.WriteLine(string.Join(\",\", query.ToList()));",
                "2,3",
                ["2,3"],
                "LINQ Where chưa chạy cho tới khi gọi ToList() hoặc foreach.",
                "Vì query thực thi lúc gọi ToList() sau khi Add(3), kết quả gồm cả 2 và 3.",
                "Sửa lỗi dùng .First() gây crash InvalidOperationException khi rỗng",
                "C# LINQ Safety",
                "Truy vấn tìm user theo email đang dùng .First() làm sập API 500 khi email không tồn tại.",
                "var user = await db.Users.FirstAsync(u => u.Email == email);",
                1,
                "var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);\nif (user is null) return Results.NotFound();",
                ["FirstOrDefaultAsync", "null"],
                null,
                "Dùng FirstOrDefaultAsync và kiểm tra null để trả 404 NotFound thay vì ném ngoại lệ 500."
            );
        }

        // General C# & ASP.NET Core (Basics, OOP, Async, REST API, Validation, File Upload, Config, Logging, Caching, Swagger)
        return new ExerciseTripleSpec(
            "csharp", "dotnet",
            $"C# & ASP.NET Core: {lessonTitle}",
            "Hoàn thiện Minimal API endpoint bất đồng bộ chuẩn REST với kiểm tra hợp lệ.",
            "Xây dựng endpoint tạo bài học mới trả về 201 Created và hỗ trợ CancellationToken.",
            [
                "app.MapPost(\"/api/v1/lessons\", async (CreateLessonDto dto, SwmsDbContext db, {{BLANK_0}} ct) =>",
                "{",
                "    if (string.IsNullOrWhiteSpace(dto.Title)) return Results.{{BLANK_1}}(new { error = \"Thiếu tiêu đề\" });",
                "    var lesson = new Lesson { Title = dto.Title.Trim() };",
                "    db.Lessons.Add(lesson);",
                "    await db.SaveChangesAsync(ct);",
                "    return Results.{{BLANK_2}}($\"/api/v1/lessons/{lesson.Id}\", lesson);",
                "});"
            ],
            [
                new("b0", 0, "CancellationToken", ["CancellationToken"], "Token hủy tác vụ DB khi client ngắt kết nối", ["CancellationToken", "Task", "HttpContext", "IResult"], "Truyền CancellationToken xuống EF Core giúp giải phóng tài nguyên DB ngay khi client đóng tab."),
                new("b1", 1, "BadRequest", ["BadRequest"], "Mã HTTP 400 khi dữ liệu đầu vào không hợp lệ", ["BadRequest", "NotFound", "Ok", "Forbid"], "Results.BadRequest trả về HTTP 400 chuẩn REST."),
                new("b2", 2, "Created", ["Created"], "Mã HTTP 201 kèm URI tài nguyên mới tạo", ["Created", "Ok", "Accepted", "NoContent"], "Results.Created trả về HTTP 201 kèm header Location.")
            ],
            [
                new(6, "await db.SaveChangesAsync(ct);", "Lưu thay đổi bất đồng bộ kèm token hủy.", "Gọi SaveChanges() đồng bộ gây nghẽn ThreadPool thread.")
            ],
            "Trung bình",
            "Độ chính xác kiểu decimal vs double cho Tiền tệ trong C#",
            "Dự đoán sự khác biệt khi cộng 0.1 + 0.2 bằng double vs decimal.",
            "double d = 0.1 + 0.2;\ndecimal m = 0.1m + 0.2m;\nConsole.WriteLine($\"{d == 0.3} {m == 0.3m}\");",
            "False True",
            ["False True"],
            "double dùng hệ nhị phân IEEE 754 bị sai số làm tròn, decimal dùng hệ cơ số 10.",
            "0.1 + 0.2 ở kiểu double cho 0.30000000000000004 nên d == 0.3 là False; còn decimal chính xác là True.",
            "Sửa lỗi dùng double cho giá tiền & chặn async void nguy hiểm",
            "C# Best Practices",
            "Phương thức tính học phí dùng float/double và khai báo async void làm ngoại lệ không bắt được.",
            "public async void ChargeFee(double price, double tax)\n{\n    await Task.Delay(10);\n    var total = price + tax;\n}",
            1,
            "public async Task<decimal> ChargeFeeAsync(decimal price, decimal tax)\n{\n    await Task.Delay(10);\n    return price + tax;\n}",
            ["async Task", "decimal"],
            "ChargeFeeAsync",
            "Tiền tệ bắt buộc dùng kiểu decimal và phương thức bất đồng bộ phải trả về Task/Task<T> thay vì async void."
        );
    }

    private static ExerciseTripleSpec BuildPhase6Spec(string path, string titleLower, string lessonTitle)
    {
        if (path.Contains("ef-core", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "csharp", "sql",
                "EF Core: AsNoTracking & Eager Loading chống N+1 Query",
                "Tối ưu truy vấn danh mục khóa học và bài học trong 1 câu SQL duy nhất.",
                "Mỗi request tra cứu chỉ đọc không cần ChangeTracker; dùng Include để gom bảng con.",
                [
                    "var courses = await db.Courses",
                    "    .{{BLANK_0}}()",
                    "    .{{BLANK_1}}(c => c.Chapters)",
                    "    .Where(c => c.IsPublished)",
                    "    .{{BLANK_2}}(ct);"
                ],
                [
                    new("b0", 0, "AsNoTracking", ["AsNoTracking"], "Tắt ChangeTracker cho truy vấn chỉ đọc", ["AsNoTracking", "AsTracking", "Distinct", "IgnoreQueryFilters"], "AsNoTracking() giảm 50-70% RAM snapshot khi đọc danh sách lớn."),
                    new("b1", 1, "Include", ["Include"], "Nạp kèm bảng quan hệ trong 1 lần JOIN", ["Include", "Join", "SelectMany", "Load"], "Include() ngăn lỗi N+1 Query khi duyệt c.Chapters."),
                    new("b2", 2, "ToListAsync", ["ToListAsync"], "Thực thi truy vấn bất đồng bộ xuống SQL", ["ToListAsync", "ToList", "First", "AsEnumerable"], "ToListAsync(ct) giải phóng thread trong lúc chờ database phản hồi.")
                ],
                [
                    new(2, ".AsNoTracking()", "Loại bỏ chi phí theo dõi thực thể cho endpoint GET.", "Để Tracking mặc định trên danh sách hàng nghìn dòng gây áp lực lớn cho GC.")
                ],
                "Trung bình",
                "IQueryable vs IEnumerable trong EF Core",
                "Dự đoán nơi thực thi điều kiện Where khi ép kiểu AsEnumerable() quá sớm.",
                "// db.Lessons.AsEnumerable().Where(l => l.IsPublished)\n// Điều kiện Where lọc ở SQL Server hay RAM ứng dụng?\nConsole.WriteLine(\"RAM (Client-side)\");",
                "RAM (Client-side)",
                ["RAM (Client-side)", "RAM"],
                "Sau AsEnumerable(), đối tượng chuyển thành IEnumerable nên EF Core kéo toàn bộ bảng về RAM trước.",
                "Gọi AsEnumerable() sớm làm mất Expression Tree IQueryable, khiến SQL tải toàn bộ bảng về C# rồi mới lọc.",
                "Sửa lỗi N+1 Query khi đếm bài học trong vòng lặp foreach",
                "EF Core Performance",
                "Vòng lặp gọi db.Lessons.CountAsync() cho từng chapter sinh ra 1 + N câu SQL riêng lẻ.",
                "foreach (var ch in chapters) {\n  var count = await db.Lessons.CountAsync(l => l.ChapterId == ch.Id);\n}",
                2,
                "var counts = await db.Lessons.AsNoTracking()\n  .GroupBy(l => l.ChapterId)\n  .Select(g => new { ChapterId = g.Key, Count = g.Count() })\n  .ToDictionaryAsync(x => x.ChapterId, x => x.Count);",
                ["GroupBy", "AsNoTracking"],
                null,
                "Gom nhóm bằng GroupBy trên DB hoặc dùng Projection Select trong 1 truy vấn duy nhất để triệt tiêu N+1 Query."
            );
        }

        if (path.Contains("sql-joins", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "sql", "sql",
                "SQL JOINs: LEFT JOIN lấy đầy đủ bản ghi kể cả chưa có dữ liệu con",
                "Viết truy vấn liệt kê tất cả học viên và số bài đã hoàn thành (kể cả 0 bài).",
                "Nếu dùng INNER JOIN, học viên mới chưa hoàn thành bài nào sẽ biến mất khỏi báo cáo.",
                [
                    "SELECT u.id, u.full_name, COUNT(p.lesson_id) AS completed_count",
                    "FROM users u",
                    "{{BLANK_0}} JOIN user_lesson_progresses p",
                    "  {{BLANK_1}} p.user_id = u.id AND p.is_completed = TRUE",
                    "GROUP {{BLANK_2}} u.id, u.full_name;"
                ],
                [
                    new("b0", 0, "LEFT", ["LEFT", "LEFT OUTER"], "Giữ toàn bộ bản ghi bên trái (users)", ["LEFT", "INNER", "RIGHT", "CROSS"], "LEFT JOIN giữ lại user dù chưa có bản ghi nào khớp ở bảng progress."),
                    new("b1", 1, "ON", ["ON"], "Điều kiện nối bảng đặt tại ON để không biến LEFT thành INNER", ["ON", "WHERE", "HAVING", "USING"], "Đặt p.is_completed = TRUE trong ON giúp giữ nguyên user có 0 bài hoàn thành."),
                    new("b2", 2, "BY", ["BY"], "Gom nhóm theo từng học viên", ["BY", "INTO", "AS", "WITH"], "GROUP BY u.id, u.full_name tổng hợp COUNT cho mỗi học viên.")
                ],
                [
                    new(4, "ON p.user_id = u.id AND p.is_completed = TRUE", "Lọc bảng phải ngay trong điều kiện ON.", "Đặt p.is_completed = TRUE ở WHERE sẽ loại bỏ các dòng NULL từ LEFT JOIN.")
                ],
                "Trung bình",
                "Sự khác biệt giữa đặt điều kiện ở ON vs WHERE trong LEFT JOIN",
                "Dự đoán số dòng trả về khi user chưa có đơn hàng và lọc WHERE o.status = 'paid'.",
                "-- users: (1, 'An'), (2, 'Bình')\n-- orders: (10, user_id=1, status='paid')\n-- SELECT u.name FROM users u LEFT JOIN orders o ON o.user_id = u.id WHERE o.status = 'paid';",
                "1 dòng: An",
                ["1 dòng: An", "An"],
                "WHERE chạy sau khi JOIN, nên dòng của Bình có o.status = NULL bị loại bỏ.",
                "Điều kiện WHERE o.status = 'paid' loại mất NULL, biến LEFT JOIN thành INNER JOIN chỉ còn 'An'.",
                "Sửa lỗi WHERE làm mất bản ghi NULL của LEFT JOIN",
                "SQL JOIN Logic",
                "Truy vấn cần lấy mọi khóa học và số học viên, nhưng WHERE e.role = 'student' đã làm mất khóa chưa có học viên.",
                "SELECT c.title, COUNT(e.id)\nFROM courses c\nLEFT JOIN course_enrollments e ON e.course_id = c.id\nWHERE e.role = 'student'\nGROUP BY c.id, c.title;",
                4,
                "SELECT c.title, COUNT(e.id)\nFROM courses c\nLEFT JOIN course_enrollments e ON e.course_id = c.id AND e.role = 'student'\nGROUP BY c.id, c.title;",
                ["AND e.role = 'student'"],
                null,
                "Chuyển điều kiện lọc của bảng bên phải (e.role = 'student') từ WHERE lên mệnh đề ON để giữ lại các khóa có 0 học viên."
            );
        }

        if (path.Contains("sql-aggregation", StringComparison.Ordinal) || path.Contains("advanced-sql", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "sql", "sql",
                "SQL Chuyên sâu: GROUP BY, HAVING & Window Function",
                "Hoàn thiện truy vấn xếp hạng điểm học viên trong từng khóa bằng CTE và Window Function.",
                "Phân tích bảng xếp hạng mà không làm mất dòng chi tiết như GROUP BY.",
                [
                    "{{BLANK_0}} ranked AS (",
                    "  SELECT course_id, user_id, score,",
                    "    ROW_NUMBER() {{BLANK_1}} (PARTITION BY course_id ORDER BY score DESC) AS rn",
                    "  FROM practice_attempts",
                    ")",
                    "SELECT * FROM ranked {{BLANK_2}} rn <= 3;"
                ],
                [
                    new("b0", 0, "WITH", ["WITH"], "Khai báo Common Table Expression (CTE)", ["WITH", "CREATE", "DECLARE", "SELECT"], "WITH định nghĩa bảng tạm CTE giúp câu lệnh dễ đọc và dễ bảo trì."),
                    new("b1", 1, "OVER", ["OVER"], "Chỉ định cửa sổ phân vùng cho Window Function", ["OVER", "BY", "IN", "WITHIN"], "OVER (PARTITION BY ... ORDER BY ...) tính xếp hạng riêng cho từng course_id."),
                    new("b2", 2, "WHERE", ["WHERE"], "Lọc Top 3 từ CTE đã tính sẵn rn", ["WHERE", "HAVING", "FILTER", "LIMIT"], "Window Function không dùng trực tiếp trong WHERE nên phải bọc qua CTE rồi lọc.")
                ],
                [
                    new(3, "ROW_NUMBER() OVER (PARTITION BY course_id ORDER BY score DESC)", "Đánh số thứ tự 1, 2, 3 riêng trong mỗi khóa học.", "Thiếu PARTITION BY sẽ xếp hạng chung toàn bộ bảng thay vì từng khóa.")
                ],
                "Trung bình",
                "Thứ tự thực thi logic của câu lệnh SQL (FROM -> WHERE -> GROUP BY -> HAVING -> SELECT)",
                "Dự đoán vì sao alias đặt ở SELECT không dùng được trong WHERE.",
                "-- SELECT score * 10 AS pts FROM attempts WHERE pts > 500;\n-- Kết quả thực thi trên PostgreSQL/SQL Server:",
                "Lỗi: column \"pts\" does not exist",
                ["Lỗi: column \"pts\" does not exist", "Error"],
                "Mệnh đề WHERE chạy trước SELECT nên chưa nhìn thấy bí danh (alias) pts.",
                "Thứ tự thực thi SQL: FROM -> WHERE -> GROUP BY -> HAVING -> SELECT -> ORDER BY.",
                "Sửa lỗi dùng hàm tổng hợp COUNT() trong WHERE thay vì HAVING",
                "SQL Aggregation",
                "Câu truy vấn lọc khóa học có hơn 10 bài học dùng nhầm WHERE COUNT(*) > 10.",
                "SELECT course_id, COUNT(*) AS total\nFROM lessons\nWHERE COUNT(*) > 10\nGROUP BY course_id;",
                3,
                "SELECT course_id, COUNT(*) AS total\nFROM lessons\nGROUP BY course_id\nHAVING COUNT(*) > 10;",
                ["HAVING COUNT(*) > 10"],
                null,
                "Hàm tổng hợp (COUNT, SUM, AVG) phải đặt trong HAVING sau GROUP BY chứ không được dùng trong WHERE."
            );
        }

        if (path.Contains("db-security", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "sql", "sql",
                "Bảo mật CSDL: Parameterized Query chống SQL Injection",
                "Hoàn thiện câu lệnh SQL tham số hóa an toàn tuyệt đối trước input độc hại.",
                "Nối chuỗi trực tiếp email từ người dùng cho phép kẻ tấn công chèn ' OR '1'='1 để vượt qua đăng nhập.",
                [
                    "SELECT id, email, role",
                    "FROM users",
                    "WHERE email = {{BLANK_0}}",
                    "  AND is_active = {{BLANK_1}};"
                ],
                [
                    new("b0", 0, "@email", ["@email", "$1", ":email"], "Placeholder tham số hóa thay vì nối chuỗi", ["@email", "'email'", "${email}", "+ email"], "Tham số @email được DB driver truyền riêng biệt như dữ liệu thuần, không bao giờ bị hiểu nhầm thành lệnh SQL."),
                    new("b1", 1, "TRUE", ["TRUE", "1"], "Hằng boolean lọc tài khoản đang hoạt động", ["TRUE", "'TRUE'", "ACTIVE", "ON"], "Lọc điều kiện kích hoạt chuẩn xác.")
                ],
                [
                    new(3, "WHERE email = @email", "Tách biệt hoàn toàn mã lệnh SQL và dữ liệu người dùng.", "Cộng chuỗi '\" + email + \"' mở toang cửa cho SQL Injection.")
                ],
                "Trung bình",
                "Cơ chế phòng vệ của Parameterized Query",
                "Dự đoán kết quả khi kẻ tấn công nhập \"' OR 1=1 --\" vào tham số @email.",
                "-- WHERE email = @email với @email = \"' OR 1=1 --\"\n-- Database tìm user có cột email bằng đúng chuỗi ký tự nào?",
                "Tìm chính xác chuỗi \"' OR 1=1 --\" (0 bản ghi)",
                ["Tìm chính xác chuỗi \"' OR 1=1 --\" (0 bản ghi)", "0 bản ghi"],
                "Driver đóng gói giá trị tham số như một literal string thuần túy.",
                "Database không biên dịch nội dung @email thành toán tử OR mà chỉ so sánh chuỗi theo nghĩa đen.",
                "Sửa lỗ hổng SQL Injection trong EF Core FromSqlRaw",
                "SQL Injection Prevention",
                "Truy vấn nội suy chuỗi $\"...\" trong FromSqlRaw đang cho phép tiêm mã SQL.",
                "var rows = db.Users.FromSqlRaw($\"SELECT * FROM users WHERE email = '{email}'\").ToList();",
                1,
                "var rows = db.Users.FromSqlInterpolated($\"SELECT * FROM users WHERE email = {email}\").ToList();",
                ["FromSqlInterpolated"],
                null,
                "Dùng FromSqlInterpolated (hoặc truyền DbParameter vào FromSqlRaw) để EF Core tự động tham số hóa {email} thành @p0."
            );
        }

        // DB Basics, Design, ER, Indexes, Transactions
        return new ExerciseTripleSpec(
            "sql", "sql",
            $"Database & SQL: {lessonTitle}",
            "Hoàn thiện giao dịch ACID và tạo chỉ mục B-Tree tối ưu truy vấn.",
            "Chuyển tiền hoặc ghi nhận thanh toán bắt buộc phải toàn vẹn (All-or-Nothing) và có Index trên cột hay tra cứu.",
            [
                "{{BLANK_0}} TRANSACTION;",
                "UPDATE wallets SET balance = balance - 100 WHERE user_id = @from_id;",
                "UPDATE wallets SET balance = balance + 100 WHERE user_id = @to_id;",
                "{{BLANK_1}};",
                "CREATE {{BLANK_2}} idx_wallets_user_id ON wallets (user_id);"
            ],
            [
                new("b0", 0, "BEGIN", ["BEGIN", "START"], "Mở đầu khối giao dịch ACID", ["BEGIN", "OPEN", "INIT", "TRY"], "BEGIN TRANSACTION gom các lệnh thành 1 đơn vị nguyên tử (Atomicity)."),
                new("b1", 1, "COMMIT", ["COMMIT"], "Xác nhận lưu vĩnh viễn toàn bộ thay đổi", ["COMMIT", "SAVE", "END", "FLUSH"], "COMMIT đảm bảo cả hai lệnh UPDATE đều thành công mới lưu bền vững."),
                new("b2", 2, "INDEX", ["INDEX"], "Tạo cấu trúc B-Tree tăng tốc tìm kiếm từ O(N) xuống O(log N)", ["INDEX", "KEY", "VIEW", "TABLE"], "Index trên user_id tránh quét toàn bảng (Full Table Scan).")
            ],
            [
                new(1, "BEGIN TRANSACTION;", "Đảm bảo nguyên tử: trừ tiền và cộng tiền cùng thành công hoặc cùng ROLLBACK.", "Nếu máy chủ sập giữa 2 lệnh UPDATE không có transaction, tiền sẽ bị mất oan.")
            ],
            "Trung bình",
            "Tính chất ACID: Atomicity khi xảy ra lỗi giữa chừng",
            "Dự đoán số dư nếu lệnh thứ 2 thất bại và hệ thống gọi ROLLBACK.",
            "-- Ban đầu: A = 500, B = 200\n-- BEGIN; UPDATE A = 400; (lỗi tại UPDATE B) -> ROLLBACK;",
            "A = 500, B = 200",
            ["A = 500, B = 200"],
            "ROLLBACK hoàn nguyên toàn bộ trạng thái về trước thời điểm BEGIN.",
            "Nhờ Atomicity của Transaction, A được khôi phục nguyên vẹn về 500 và B vẫn là 200.",
            "Sửa lỗi thiếu Index và lọc cột hàm làm mất tác dụng B-Tree Index",
            "SQL Index Performance",
            "Câu truy vấn dùng YEAR(created_at) = 2026 khiến DB không thể dùng Index trên cột created_at.",
            "SELECT * FROM orders WHERE YEAR(created_at) = 2026;",
            1,
            "SELECT id, user_id, created_at FROM orders WHERE created_at >= '2026-01-01' AND created_at < '2027-01-01';",
            ["created_at >=", "created_at <"],
            null,
            "Tránh bọc cột được đánh Index trong hàm (non-SARGable); hãy viết thành khoảng so sánh trực tiếp để DB tận dụng B-Tree Index."
        );
    }

    private static ExerciseTripleSpec BuildPhase7Spec(string path, string titleLower, string lessonTitle)
    {
        if (path.Contains("cors", StringComparison.Ordinal) || path.Contains("jwt", StringComparison.Ordinal) || path.Contains("authorization", StringComparison.Ordinal) || path.Contains("login", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "csharp", "dotnet",
                "Full-Stack Auth & CORS: Chính sách Origin & Phân quyền RBAC",
                "Cấu hình CORS an toàn cho cookie xác thực và gắn quyền Admin lên endpoint.",
                "Khi trình duyệt gửi HttpOnly Refresh Cookie từ localhost:3000 sang API, AllowAnyOrigin() sẽ bị trình duyệt chặn đứng.",
                [
                    "builder.Services.AddCors(opt => opt.AddPolicy(\"Frontend\", p =>",
                    "    p.{{BLANK_0}}(\"http://localhost:3000\")",
                    "     .AllowAnyHeader().AllowAnyMethod().{{BLANK_1}}()));",
                    "app.MapDelete(\"/api/v1/admin/users/{id}\", [{{BLANK_2}}(Roles = \"Admin\")] (Guid id) => Results.Ok());"
                ],
                [
                    new("b0", 0, "WithOrigins", ["WithOrigins"], "Chỉ định rõ danh sách Origin hợp lệ", ["WithOrigins", "AllowAnyOrigin", "SetHost", "AddDomain"], "Bắt buộc dùng WithOrigins tường minh khi bật AllowCredentials()."),
                    new("b1", 1, "AllowCredentials", ["AllowCredentials"], "Cho phép gửi kèm Cookie/Authorization header qua CORS", ["AllowCredentials", "AllowCookies", "WithAuth", "EnableSession"], "Thiếu AllowCredentials(), cookie HttpOnly sẽ không được đính kèm."),
                    new("b2", 2, "Authorize", ["Authorize"], "Attribute kiểm tra JWT và Role", ["Authorize", "AllowAnonymous", "Auth", "Role"], "[Authorize(Roles = \"Admin\")] chặn mọi user không mang role Admin với 403 Forbidden.")
                ],
                [
                    new(2, ".WithOrigins(\"http://localhost:3000\")", "Giới hạn nguồn gốc được phép gọi API.", "Kết hợp AllowAnyOrigin() + AllowCredentials() sẽ ném lỗi ngay khi khởi động ASP.NET Core.")
                ],
                "Trung bình",
                "HTTP 401 Unauthorized vs 403 Forbidden",
                "Dự đoán mã trạng thái trả về cho user đã đăng nhập (role='student') gọi endpoint [Authorize(Roles='Admin')].",
                "// Token hợp lệ, User.Identity.IsAuthenticated = true, Role = 'student'\n// Endpoint yêu cầu Roles = 'Admin'\nConsole.WriteLine(\"403 Forbidden\");",
                "403 Forbidden",
                ["403 Forbidden", "403"],
                "401 là chưa xác thực (Authentication), 403 là đã xác thực nhưng thiếu quyền (Authorization).",
                "User đã có JWT hợp lệ nhưng không có role Admin -> ASP.NET Core trả 403 Forbidden.",
                "Sửa lỗi cấu hình CORS AllowAnyOrigin đi cùng AllowCredentials",
                "CORS & Security",
                "ASP.NET Core ném ngoại lệ khi cấu hình AllowAnyOrigin() kết hợp AllowCredentials().",
                "policy.AllowAnyOrigin().AllowAnyHeader().AllowCredentials();",
                1,
                "policy.WithOrigins(\"http://localhost:3000\").AllowAnyHeader().AllowCredentials();",
                ["WithOrigins", "AllowCredentials"],
                null,
                "Theo chuẩn bảo mật CORS, khi bật AllowCredentials() bắt buộc phải liệt kê cụ thể nguồn tin cậy bằng WithOrigins(...)."
            );
        }

        if (path.Contains("signalr", StringComparison.Ordinal) || path.Contains("react-query", StringComparison.Ordinal) || path.Contains("form-handling", StringComparison.Ordinal) || path.Contains("error-handling", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "typescript", "react",
                $"Full-Stack Client Integration: {lessonTitle}",
                "Hoàn thiện tích hợp TanStack Query / SignalR / Zod an toàn ở Frontend.",
                "Tự động kết nối lại Realtime hoặc làm mới cache ngay sau khi mutation thành công.",
                [
                    "const queryClient = useQueryClient();",
                    "const mutation = {{BLANK_0}}({",
                    "  mutationFn: (payload: CreateNoteDto) => api.post('/notes', payload),",
                    "  onSuccess: () => queryClient.{{BLANK_1}}({ {{BLANK_2}}: ['notes'] }),",
                    "});"
                ],
                [
                    new("b0", 0, "useMutation", ["useMutation"], "Hook thực hiện thao tác ghi POST/PUT/DELETE", ["useMutation", "useQuery", "useEffect", "useAction"], "useMutation quản lý trạng thái pending, error và callback onSuccess cho thao tác thay đổi dữ liệu."),
                    new("b1", 1, "invalidateQueries", ["invalidateQueries"], "Đánh dấu dữ liệu cache đã cũ để tự fetch lại", ["invalidateQueries", "clearCache", "resetQueries", "refetchAll"], "invalidateQueries kích hoạt đồng bộ lại danh sách mới nhất từ Backend."),
                    new("b2", 2, "queryKey", ["queryKey"], "Khóa định danh nhóm truy vấn cần làm mới", ["queryKey", "cacheKey", "id", "tag"], "queryKey: ['notes'] làm mới toàn bộ cache liên quan đến notes.")
                ],
                [
                    new(4, "queryClient.invalidateQueries({ queryKey: ['notes'] })", "Đảm bảo UI luôn khớp trạng thái thật trên server.", "Không invalidate khiến người dùng tưởng tạo ghi chú thất bại vì danh sách không hiện item mới.")
                ],
                "Trung bình",
                "Xử lý lỗi HTTP khi dùng fetch() chuẩn Web API",
                "Dự đoán liệu fetch('/api/missing') trả HTTP 404 có nhảy vào khối catch() không.",
                "fetch('/api/not-found')\n  .then(res => console.log('then:', res.ok, res.status))\n  .catch(() => console.log('catch'));",
                "then: false 404",
                ["then: false 404"],
                "fetch() chỉ reject vào catch() khi mất mạng hoặc lỗi CORS, còn HTTP 404/500 vẫn vào .then() với res.ok = false.",
                "Phải kiểm tra if (!res.ok) throw new Error(...) chứ không được tin rằng catch() sẽ bắt được lỗi 404/500.",
                "Sửa lỗi fetch() nuốt lỗi HTTP 400/500 mà không ném ngoại lệ",
                "Fetch Error Handling",
                "Hàm loadCourse trả về dữ liệu lỗi JSON như thành công vì quên kiểm tra !res.ok.",
                "async function loadCourse(id: string) {\n  const res = await fetch(`/api/v1/courses/${id}`);\n  return await res.json();\n}",
                2,
                "async function loadCourse(id: string) {\n  const res = await fetch(`/api/v1/courses/${id}`);\n  if (!res.ok) throw new Error(`HTTP ${res.status}`);\n  return await res.json();\n}",
                ["if (!res.ok)", "throw"],
                "loadCourse",
                "Luôn kiểm tra if (!res.ok) để ném lỗi tường minh khi server trả mã 4xx/5xx."
            );
        }

        // Pagination, File Upload E2E, Connect React + .NET, Caching
        return new ExerciseTripleSpec(
            "csharp", "dotnet",
            $"Full-Stack Integration: {lessonTitle}",
            "Hoàn thiện API phân trang chuẩn server-side bằng Skip và Take.",
            "Không bao giờ trả toàn bộ bảng hàng triệu dòng về React; phải phân trang tại DB.",
            [
                "var safePage = Math.{{BLANK_0}}(1, page);",
                "var safeSize = Math.Clamp(pageSize, 1, 100);",
                "var items = await db.Lessons.AsNoTracking()",
                "    .OrderBy(l => l.SortOrder)",
                "    .{{BLANK_1}}((safePage - 1) * safeSize)",
                "    .{{BLANK_2}}(safeSize)",
                "    .ToListAsync();"
            ],
            [
                new("b0", 0, "Max", ["Max"], "Đảm bảo số trang tối thiểu là 1", ["Max", "Min", "Abs", "Clamp"], "Ngăn page = 0 hoặc âm gây offset âm."),
                new("b1", 1, "Skip", ["Skip"], "Bỏ qua số bản ghi của các trang trước", ["Skip", "Offset", "Drop", "Jump"], "Skip((page - 1) * pageSize) nhảy tới đúng vị trí trang hiện tại."),
                new("b2", 2, "Take", ["Take"], "Giới hạn số bản ghi trả về mỗi trang", ["Take", "Limit", "Top", "Count"], "Take(safeSize) tránh DoS RAM khi client truyền pageSize = 999999.")
            ],
            [
                new(2, "Math.Clamp(pageSize, 1, 100)", "Giới hạn kích thước trang tối đa 100 bản ghi.", "Cho phép pageSize không giới hạn dễ làm sập server khi bị tấn công.")
            ],
            "Trung bình",
            "Tính tổng số trang (TotalPages) khi phân trang",
            "Dự đoán số trang khi có 25 bản ghi và pageSize = 10.",
            "int total = 25, pageSize = 10;\nint totalPages = (int)Math.Ceiling(total / (double)pageSize);\nConsole.WriteLine(totalPages);",
            "3",
            ["3"],
            "25 / 10.0 = 2.5, làm tròn lên bằng Math.Ceiling cho 3 trang.",
            "Trang 1 (10), Trang 2 (10), Trang 3 (5) -> tổng 3 trang.",
            "Sửa lỗi chia nguyên mất phần dư khi tính totalPages trong C#",
            "Pagination Math",
            "25 / 10 trong C# cho kết quả 2 (int) làm mất 5 bản ghi cuối ở trang 3.",
            "int totalPages = totalItems / pageSize;",
            1,
            "int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);",
            ["Ceiling", "(double)"],
            null,
            "Ép kiểu sang double trước khi chia rồi dùng Math.Ceiling để không mất trang lẻ cuối cùng."
        );
    }

    private static ExerciseTripleSpec BuildPhase8Spec(string path, string titleLower, string lessonTitle)
    {
        if (path.Contains("vitest", StringComparison.Ordinal) || path.Contains("playwright", StringComparison.Ordinal) || path.Contains("code-quality", StringComparison.Ordinal) || path.Contains("code-review", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "typescript", "react",
                $"Frontend Testing & Quality: {lessonTitle}",
                "Hoàn thiện bài kiểm thử Vitest / Playwright theo hành vi người dùng.",
                "Kiểm thử theo vai trò ARIA (getByRole) thay vì class CSS giúp test bền vững khi đổi giao diện.",
                [
                    "import { describe, it, expect } from '{{BLANK_0}}';",
                    "describe('Discount calculator', () => {",
                    "  it('applies 20% off', () => {",
                    "    const price = applyDiscount(100, 0.2);",
                    "    {{BLANK_1}}(price).{{BLANK_2}}(80);",
                    "  });",
                    "});"
                ],
                [
                    new("b0", 0, "vitest", ["vitest"], "Framework unit/component test hiện đại cho Vite/React", ["vitest", "xunit", "mocha", "playwright"], "Vitest chạy siêu nhanh và tương thích ESM/TypeScript gốc."),
                    new("b1", 1, "expect", ["expect"], "Hàm tạo khẳng định (assertion)", ["expect", "assert", "verify", "check"], "expect(actual) mở đầu chuỗi kiểm tra kết quả."),
                    new("b2", 2, "toBe", ["toBe", "toEqual"], "So sánh bằng nghiêm ngặt cho giá trị nguyên thủy", ["toBe", "toEqual", "toContain", "toMatch"], "toBe() kiểm tra giá trị nguyên thủy bằng Object.is.")
                ],
                [
                    new(5, "expect(price).toBe(80);", "Khẳng định giá trị đầu ra thực tế khớp kỳ vọng.", "Test không có assertion luôn pass giả tạo dù hàm trả sai.")
                ],
                "Trung bình",
                "Playwright Auto-waiting vs waitForTimeout cứng",
                "Dự đoán cách viết E2E Playwright chuẩn mực ổn định trong CI.",
                "// Tránh: await page.waitForTimeout(5000);\n// Chuẩn: await expect(page.getByRole('button', { name: 'Nộp bài' })).toBeEnabled();\nConsole.log('Auto-waiting assertion');",
                "Auto-waiting assertion",
                ["Auto-waiting assertion"],
                "Playwright tự động chờ thông minh tới khi phần tử sẵn sàng thay vì ngủ cứng.",
                "Dùng expect(locator).toBeVisible()/toBeEnabled() vừa nhanh hơn vừa tránh flaky test.",
                "Sửa lỗi Flaky Test dùng sleep cứng trong Playwright E2E",
                "Playwright Best Practices",
                "Dùng waitForTimeout(500) bị trượt ngẫu nhiên khi CI chậm.",
                "await page.click('#submit-btn');\nawait page.waitForTimeout(500);\nconst text = await page.textContent('.toast');",
                2,
                "await page.getByRole('button', { name: 'Gửi' }).click();\nawait expect(page.getByText('Thành công')).toBeVisible();",
                ["expect(", "toBeVisible()"],
                null,
                "Thay waitForTimeout cứng bằng web-first assertion expect(locator).toBeVisible() có cơ chế auto-waiting."
            );
        }

        if (path.Contains("cicd", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "bash", "os",
                "CI/CD Pipeline: Chuỗi lệnh kiểm chứng tự động trước khi Merge",
                "Hoàn thiện các bước Build & Test trong GitHub Actions CI.",
                "Tự động chặn Pull Request nếu backend build lỗi hoặc bất kỳ unit/integration test nào thất bại.",
                [
                    "dotnet {{BLANK_0}} backend/Swms.Api.sln -c Release",
                    "dotnet {{BLANK_1}} backend/Swms.Api.sln -c Release --no-build",
                    "npm {{BLANK_2}} --prefix frontend && npm run build --prefix frontend"
                ],
                [
                    new("b0", 0, "build", ["build"], "Biên dịch giải pháp backend ở cấu hình Release", ["build", "clean", "publish", "restore"], "dotnet build -c Release xác minh mã C# biên dịch sạch."),
                    new("b1", 1, "test", ["test"], "Chạy toàn bộ bộ kiểm thử xUnit", ["test", "run", "watch", "pack"], "dotnet test --no-build tái sử dụng bản build vừa tạo để tiết kiệm thời gian CI."),
                    new("b2", 2, "ci", ["ci"], "Cài đặt phụ thuộc khớp 100% package-lock.json", ["ci", "install", "update", "audit"], "npm ci nhanh và đảm bảo tái lập môi trường chính xác trong CI.")
                ],
                [
                    new(2, "dotnet test backend/Swms.Api.sln -c Release --no-build", "Chạy toàn bộ test mà không tốn thời gian biên dịch lại.", "Bỏ qua test trong CI khiến bug lọt thẳng lên Production.")
                ],
                "Dễ",
                "Ý nghĩa Exit Code trong CI/CD Pipeline",
                "Dự đoán trạng thái job CI khi lệnh test trả về exit code 1.",
                "dotnet build && dotnet test\n# Nếu dotnet test trả về exit code 1:",
                "Pipeline dừng và báo FAIL (đỏ)",
                ["Pipeline dừng và báo FAIL (đỏ)", "FAIL"],
                "Bất kỳ exit code khác 0 nào trong CI đều đánh dấu step thất bại.",
                "CI/CD chỉ cho phép merge khi toàn bộ step trả về exit code 0.",
                "Sửa lỗi pipeline CI dùng `|| true` nuốt lỗi test thất bại",
                "CI/CD Quality Gate",
                "Dòng lệnh gắn `|| true` khiến GitHub Actions luôn xanh dù có test bị fail.",
                "dotnet test backend/Swms.Api.sln || true",
                1,
                "dotnet test backend/Swms.Api.sln -c Release --no-build",
                ["dotnet test", "--no-build"],
                null,
                "Tuyệt đối không dùng `|| true` sau lệnh test; phải để exit code thất bại chặn PR lỗi."
            );
        }

        // xUnit, Mocking, Integration Testing WebApplicationFactory, TDD
        return new ExerciseTripleSpec(
            "csharp", "dotnet",
            $"xUnit & Integration Testing: {lessonTitle}",
            "Hoàn thiện bài kiểm thử xUnit với [Theory], [InlineData] và Arrange-Act-Assert.",
            "Kiểm thử tham số hóa cho phép test nhiều trường hợp biên trong 1 phương thức gọn gàng.",
            [
                "[{{BLANK_0}}]",
                "[{{BLANK_1}}(100, 0.2, 80)]",
                "[InlineData(50, 0, 50)]",
                "public void ApplyDiscount_ReturnsExpected(decimal price, decimal rate, decimal expected)",
                "{",
                "    var actual = Pricing.ApplyDiscount(price, rate);",
                "    {{BLANK_2}}.Equal(expected, actual);",
                "}"
            ],
            [
                new("b0", 0, "Theory", ["Theory"], "Attribute xUnit cho test có tham số đầu vào", ["Theory", "Fact", "Test", "TestCase"], "[Theory] chạy hàm test nhiều lần tương ứng với mỗi [InlineData]."),
                new("b1", 1, "InlineData", ["InlineData"], "Cung cấp bộ dữ liệu kiểm thử cho [Theory]", ["InlineData", "TestCase", "DataRow", "Values"], "[InlineData(...)] truyền các giá trị biên trực tiếp vào tham số hàm test."),
                new("b2", 2, "Assert", ["Assert"], "Lớp khẳng định kết quả của xUnit", ["Assert", "Expect", "Should", "Verify"], "Assert.Equal(expected, actual) kiểm tra khớp giá trị chính xác.")
            ],
            [
                new(1, "[Theory]", "Tránh lặp lại code test cho cùng một quy tắc nghiệp vụ.", "Dùng [Fact] cho hàm có tham số sẽ bị lỗi biên dịch xUnit.")
            ],
            "Trung bình",
            "Chu kỳ TDD: Red -> Green -> Refactor",
            "Dự đoán thứ tự 3 bước chuẩn của quy trình Test-Driven Development.",
            "Console.WriteLine(\"1. Red (Test fails) -> 2. Green (Minimal pass) -> 3. Refactor\");",
            "1. Red (Test fails) -> 2. Green (Minimal pass) -> 3. Refactor",
            ["1. Red (Test fails) -> 2. Green (Minimal pass) -> 3. Refactor", "Red -> Green -> Refactor"],
            "Viết test trượt trước khi viết code thực thi, sau đó mới tối ưu.",
            "Chu trình Red -> Green -> Refactor đảm bảo mọi dòng code sinh ra đều có test bảo vệ.",
            "Sửa lỗi test tích hợp dùng chung 1 DB InMemory gây nhiễu giữa các test",
            "Integration Test Isolation",
            "Hai test chạy song song dùng chung tên DB cố định 'SharedTestDb' làm sai lệch kết quả đếm.",
            "builder.UseSetting(\"Database:InMemoryName\", \"SharedTestDb\");",
            1,
            "builder.UseSetting(\"Database:InMemoryName\", $\"SwmsTest_{Guid.NewGuid():N}\");",
            ["Guid.NewGuid()"],
            null,
            "Mỗi test dùng WebApplicationFactory phải cấp tên database cô lập bằng Guid.NewGuid() để không xung đột khi chạy song song."
        );
    }

    private static ExerciseTripleSpec BuildPhase9AndCapstoneSpec(string path, string titleLower, string lessonTitle)
    {
        if (path.Contains("docker-fundamentals", StringComparison.Ordinal) || titleLower.Contains("docker", StringComparison.Ordinal) && !path.Contains("compose", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "dockerfile", "os",
                "Dockerfile Multi-Stage Build cho .NET 10 API",
                "Hoàn thiện Dockerfile tách biệt bước build SDK nặng và runtime Alpine/AspNet tối giản.",
                "Giảm kích thước Docker Image từ ~850MB xuống ~110MB và loại bỏ SDK thừa khỏi môi trường Production.",
                [
                    "FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build",
                    "WORKDIR /src",
                    "COPY . .",
                    "RUN dotnet {{BLANK_0}} -c Release -o /app/publish",
                    "FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime",
                    "{{BLANK_1}} /app",
                    "{{BLANK_2}} --from=build /app/publish .",
                    "ENTRYPOINT [\"dotnet\", \"Swms.Api.dll\"]"
                ],
                [
                    new("b0", 0, "publish", ["publish"], "Đóng gói ứng dụng .NET kèm DLL sẵn sàng chạy", ["publish", "build", "run", "test"], "dotnet publish -c Release xuất tệp nhị phân gọn nhẹ vào /app/publish."),
                    new("b1", 1, "WORKDIR", ["WORKDIR"], "Thiết lập thư mục làm việc trong container runtime", ["WORKDIR", "CD", "RUN", "ENV"], "WORKDIR /app tạo thư mục và chuyển đường dẫn làm việc sang /app."),
                    new("b2", 2, "COPY", ["COPY"], "Sao chép artifact đã build từ stage trước", ["COPY", "ADD", "MOVE", "CLONE"], "COPY --from=build chỉ lấy kết quả biên dịch, bỏ lại toàn bộ mã nguồn và SDK.")
                ],
                [
                    new(5, "FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime", "Dùng runtime image nhẹ cho stage cuối.", "Giữ nguyên sdk image trên production làm tăng bề mặt tấn công và tốn dung lượng.")
                ],
                "Trung bình",
                "Tối ưu Layer Caching trong Dockerfile",
                "Dự đoán lý do COPY *.csproj trước COPY . . giúp build nhanh hơn.",
                "# Layer 1: COPY *.csproj .\n# Layer 2: RUN dotnet restore\n# Layer 3: COPY . .\nConsole.WriteLine(\"Cache dotnet restore khi chi sua code .cs\");",
                "Cache dotnet restore khi chi sua code .cs",
                ["Cache dotnet restore khi chi sua code .cs"],
                "Docker chỉ rebuild lại từ layer bị thay đổi trở xuống.",
                "Đặt COPY *.csproj + dotnet restore trước COPY . . giúp không phải tải lại NuGet packages mỗi lần sửa code C#.",
                "Sửa lỗi Dockerfile chạy bằng quyền root và chứa file bin/obj cục bộ",
                "Container Security",
                "Image không có .dockerignore và chạy container dưới quyền root nguy hiểm.",
                "FROM mcr.microsoft.com/dotnet/sdk:10.0\nWORKDIR /app\nCOPY . .\nENTRYPOINT [\"dotnet\", \"run\"]",
                1,
                "FROM mcr.microsoft.com/dotnet/aspnet:10.0\nWORKDIR /app\nUSER app\nCOPY --from=build /app/publish .\nENTRYPOINT [\"dotnet\", \"Swms.Api.dll\"]",
                ["aspnet:10.0", "USER app"],
                null,
                "Dùng aspnet runtime image (không chạy dotnet run trên SDK), chỉ copy output publish và chuyển sang USER không đặc quyền."
            );
        }

        if (path.Contains("linux", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "bash", "os",
                "Linux Administration: Phân quyền chmod, Socket ss & Service systemd",
                "Hoàn thiện các lệnh vận hành máy chủ Linux cho dịch vụ backend.",
                "Triển khai trực tiếp trên Ubuntu Server cần nắm quyền thực thi script và kiểm tra port lắng nghe.",
                [
                    "{{BLANK_0}} 750 deploy.sh",
                    "{{BLANK_1}} -tulpn | grep 5000",
                    "sudo {{BLANK_2}} restart studxy-api.service"
                ],
                [
                    new("b0", 0, "chmod", ["chmod"], "Thay đổi quyền đọc/ghi/thực thi tệp", ["chmod", "chown", "ls", "mv"], "750 = User rwx (7), Group rx (5), Others --- (0)."),
                    new("b1", 1, "ss", ["ss"], "Công cụ kiểm tra socket TCP/UDP đang mở", ["ss", "ping", "df", "top"], "ss -tulpn liệt kê các cổng lắng nghe kèm tên tiến trình."),
                    new("b2", 2, "systemctl", ["systemctl"], "Trình quản lý service systemd trên Linux", ["systemctl", "service", "docker", "apt"], "systemctl restart khởi động lại dịch vụ nền an toàn.")
                ],
                [
                    new(1, "chmod 750 deploy.sh", "Cấp quyền thực thi cho chủ sở hữu, chặn người lạ đọc/sửa script.", "Dùng chmod 777 cho phép bất kỳ user nào trên máy sửa đổi script deploy.")
                ],
                "Dễ",
                "Giải mã quyền số Octal Linux: 754",
                "Dự đoán quyền của User, Group, Others với chmod 754.",
                "# r=4, w=2, x=1\n# 7 = 4+2+1, 5 = 4+0+1, 4 = 4+0+0\necho \"User:rwx Group:r-x Others:r--\"",
                "User:rwx Group:r-x Others:r--",
                ["User:rwx Group:r-x Others:r--"],
                "Cộng các bit 4 (read), 2 (write), 1 (execute) cho từng nhóm.",
                "7=rwx, 5=r-x, 4=r--.",
                "Sửa lỗi cấp quyền quá rộng chmod 777 cho file cấu hình .env",
                "Linux Security",
                "File chứa bí mật .env đang mở quyền đọc/ghi cho toàn hệ thống (777).",
                "chmod 777 /etc/studxy/.env",
                1,
                "chmod 600 /etc/studxy/.env",
                ["chmod 600"],
                null,
                "File chứa secret như .env hoặc SSH private key chỉ nên cấp quyền 600 (chỉ chủ sở hữu được đọc và ghi)."
            );
        }

        if (titleLower.Contains("ai", StringComparison.Ordinal) || titleLower.Contains("llm", StringComparison.Ordinal) || titleLower.Contains("rag", StringComparison.Ordinal) || titleLower.Contains("python", StringComparison.Ordinal) || path.Contains("lesson-11", StringComparison.Ordinal) || path.Contains("lesson-12", StringComparison.Ordinal))
        {
            return new ExerciseTripleSpec(
                "python", "os",
                $"Python FastAPI & AI Engineering: {lessonTitle}",
                "Hoàn thiện endpoint FastAPI gọi dịch vụ AI bất đồng bộ với Pydantic schema.",
                "Kết nối mô hình ngôn ngữ lớn (LLM) và Vector Search RAG từ microservice Python.",
                [
                    "from fastapi import FastAPI, HTTPException",
                    "from pydantic import BaseModel",
                    "",
                    "class AskRequest({{BLANK_0}}):",
                    "    question: str",
                    "",
                    "@app.post(\"/api/ai/ask\")",
                    "async def ask(req: AskRequest):",
                    "    if not req.question.{{BLANK_1}}():",
                    "        raise {{BLANK_2}}(status_code=400, detail=\"Question required\")",
                    "    return {\"answer\": await run_rag(req.question)}"
                ],
                [
                    new("b0", 0, "BaseModel", ["BaseModel"], "Lớp cơ sở của Pydantic để validate payload JSON", ["BaseModel", "dict", "object", "Schema"], "BaseModel tự động parse JSON và trả 422 nếu sai kiểu dữ liệu."),
                    new("b1", 1, "strip", ["strip"], "Loại bỏ khoảng trắng thừa kiểm tra chuỗi rỗng", ["strip", "trim", "clean", "split"], "Trong Python dùng .strip() (khác với .trim() trong JS/C#)."),
                    new("b2", 2, "HTTPException", ["HTTPException"], "Ném lỗi HTTP chuẩn trong FastAPI", ["HTTPException", "ValueError", "Error", "Exception"], "raise HTTPException(400) trả response JSON lỗi chuẩn REST.")
                ],
                [
                    new(8, "async def ask(req: AskRequest):", "Dùng coroutine bất đồng bộ khi chờ I/O mạng từ LLM API.", "Chặn event loop bằng requests.post đồng bộ làm nghẽn toàn bộ worker FastAPI.")
                ],
                "Trung bình",
                "Vector Cosine Similarity & RAG Context Window",
                "Dự đoán thứ tự ưu tiên chunk trong RAG khi so Cosine Similarity.",
                "scores = [('chunk_A', 0.42), ('chunk_B', 0.89), ('chunk_C', 0.71)]\ntop = sorted(scores, key=lambda x: x[1], reverse=True)[0]\nprint(top[0])",
                "chunk_B",
                ["chunk_B"],
                "Cosine Similarity càng gần 1.0 thì độ tương đồng ngữ nghĩa càng cao.",
                "chunk_B có điểm 0.89 cao nhất nên được chọn làm ngữ cảnh hàng đầu đưa vào prompt LLM.",
                "Sửa lỗi gọi thư viện requests đồng bộ bên trong async def FastAPI",
                "Python Async & AI",
                "Endpoint async def dùng requests.post() đồng bộ làm treo toàn bộ event loop khi LLM phản hồi chậm.",
                "import requests\n@app.post('/ask')\nasync def ask(q: str):\n    r = requests.post('https://api.llm/v1', json={'q': q}, timeout=30)\n    return r.json()",
                4,
                "import httpx\n@app.post('/ask')\nasync def ask(q: str):\n    async with httpx.AsyncClient(timeout=30) as client:\n        r = await client.post('https://api.llm/v1', json={'q': q})\n        r.raise_for_status()\n        return r.json()",
                ["httpx.AsyncClient", "await client.post"],
                "ask",
                "Trong async def Python phải dùng HTTP client bất đồng bộ như httpx.AsyncClient kèm await và raise_for_status()."
            );
        }

        // Compose / Networking / Cloud / Monitoring / Capstone / Resources
        return new ExerciseTripleSpec(
            "bash", "os",
            $"Production DevOps & Capstone: {lessonTitle}",
            "Hoàn thiện quy trình kiểm tra sức khỏe (Healthcheck), log container và triển khai an toàn.",
            "Đảm bảo hệ thống đạt Zero-Downtime, cô lập mạng DB nội bộ và xoay vòng log chống đầy đĩa.",
            [
                "docker compose {{BLANK_0}} -d --build",
                "docker compose {{BLANK_1}} --tail=100 backend",
                "curl -fsS http://localhost:5000/{{BLANK_2}}"
            ],
            [
                new("b0", 0, "up", ["up"], "Khởi chạy toàn bộ stack container", ["up", "start", "run", "build"], "docker compose up -d --build tạo mạng, volume và chạy nền."),
                new("b1", 1, "logs", ["logs"], "Xem 100 dòng nhật ký mới nhất của service backend", ["logs", "ps", "top", "exec"], "Giúp kiểm tra lỗi khởi động hoặc kết nối DB nhanh chóng."),
                new("b2", 2, "health", ["health", "healthz"], "Endpoint kiểm tra readiness/liveness của API", ["health", "status", "ping", "metrics"], "Trả 200 OK khi API sẵn sàng nhận traffic.")
            ],
            [
                new(3, "curl -fsS http://localhost:5000/health", "Kiểm chứng service sống thực sự trước khi chuyển traffic vào.", "Cờ -f giúp curl trả exit code lỗi nếu API trả 500.")
            ],
            "Dễ",
            "Giới hạn dung lượng Log Docker json-file",
            "Dự đoán dung lượng log tối đa khi đặt max-size: 10m và max-file: 3.",
            "# logging: driver: json-file, max-size: 10m, max-file: 3\necho \"30MB\"",
            "30MB",
            ["30MB", "30 MB"],
            "Tổng dung lượng tối đa = max-size × max-file.",
            "10MB × 3 tệp xoay vòng = tối đa 30MB mỗi container, tránh tràn ổ cứng server.",
            "Sửa lỗi phơi bày cổng Database 5432 ra Public Internet trong Docker Compose",
            "Docker Network Security",
            "Service postgres đang mở ports 0.0.0.0:5432:5432 trực tiếp ra ngoài internet.",
            "services:\n  db:\n    image: postgres:16\n    ports:\n      - \"5432:5432\"",
            4,
            "services:\n  db:\n    image: postgres:16\n    networks:\n      - internal_net\nnetworks:\n  internal_net:\n    internal: true",
            ["internal: true", "internal_net"],
            null,
            "Xóa ports public khỏi container DB và đặt vào mạng internal: true để chỉ backend cùng mạng Docker mới kết nối được."
        );
    }
}
