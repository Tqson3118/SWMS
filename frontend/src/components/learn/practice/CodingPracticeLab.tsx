"use client";

import React, { useState, useEffect, useMemo, useCallback } from "react";
import {
  Code2,
  Bug,
  Network,
  Cpu,
  CheckCircle2,
  XCircle,
  AlertTriangle,
  ArrowRight,
  Play,
  Pause,
  RotateCcw,
  Sparkles,
  BookOpen,
  BookmarkPlus,
  HelpCircle,
  Layers,
  ShieldCheck,
  Zap,
  Server,
  Database,
  Globe,
  ChevronRight,
  Check,
  RefreshCw,
  Eye,
  EyeOff,
  Terminal,
  Sliders,
  Info,
  SlidersHorizontal,
} from "lucide-react";

export interface PracticeModuleProps {
  courseId: string;
  courseTitle: string;
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  onSaveToNotebook: (textToAppend: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
}

type MainPracticeMode = "fill-code" | "bug-hunter" | "system-visualizer";
type TopicCategory = "auto" | "dotnet" | "react" | "sql" | "os";

// ==========================================
// DỮ LIỆU CHẾ ĐỘ 1: ĐIỀN CODE CÒN THIẾU
// ==========================================
interface CodeBlank {
  id: string;
  blankIndex: number;
  expected: string;
  acceptableAnswers: string[];
  hint: string;
  chips: string[];
  explanation: string;
}

interface CodeLineExplanation {
  lineNumber: number;
  codeSnippet: string;
  purpose: string;
  riskIfWrong: string;
}

interface FillCodeExercise {
  id: string;
  title: string;
  category: "dotnet" | "react" | "sql" | "os";
  language: string;
  description: string;
  practicalScenario: string;
  codeTemplate: string[];
  blanks: CodeBlank[];
  lineExplanations: CodeLineExplanation[];
  isExtractedFromMarkdown?: boolean;
}

const DEFAULT_FILL_EXERCISES: FillCodeExercise[] = [
  {
    id: "dotnet-ef-optimization",
    title: "EF Core: Truy Vấn Read-Only & Eager Loading Tránh N+1 Query",
    category: "dotnet",
    language: "csharp",
    description: "Tối ưu hóa endpoint tra cứu bài học: loại bỏ ChangeTracker và nạp quan hệ trong 1 lệnh duy nhất.",
    practicalScenario: "Hệ thống có 5,000 học viên đồng thời xem danh mục bài học. Nếu để mặc định, DbContext tạo snapshot trong RAM gây lỗi OutOfMemory và chạy hàng nghìn câu SELECT thừa thãi.",
    codeTemplate: [
      "public async Task<List<CourseDto>> GetCourseCatalogAsync(ApplicationDbContext db)",
      "{",
      "    return await db.Courses",
      "        .{{BLANK_0}}() // 1. Bỏ theo dõi thay đổi để tối ưu bộ nhớ RAM",
      "        .{{BLANK_1}}(c => c.Lessons) // 2. Eager loading danh sách bài học tránh N+1",
      "        .Where(c => c.IsPublished)",
      "        .Select(c => new CourseDto(c.Id, c.Title, c.Lessons.Count))",
      "        .{{BLANK_2}}(); // 3. Thực thi truy vấn bất đồng bộ",
      "}",
    ],
    blanks: [
      {
        id: "b0",
        blankIndex: 0,
        expected: "AsNoTracking",
        acceptableAnswers: ["AsNoTracking", "AsNoTracking()"],
        hint: "Phương thức EF Core bỏ thực thể khỏi ChangeTracker.",
        chips: ["AsNoTracking", "AsTracking", "Include", "Distinct"],
        explanation: "AsNoTracking() vô hiệu hóa bộ theo dõi thay đổi, tiết kiệm 50%-70% RAM và tăng tốc độ xử lý JSON đáng kể.",
      },
      {
        id: "b1",
        blankIndex: 1,
        expected: "Include",
        acceptableAnswers: ["Include", "Include()"],
        hint: "Phương thức Eager Loading nạp bảng liên quan trong 1 câu lệnh JOIN.",
        chips: ["Include", "ThenInclude", "Join", "LoadRelated"],
        explanation: "Include(c => c.Lessons) thực hiện LEFT JOIN nạp bài học trong 1 lần gọi SQL duy nhất thay vì N câu lệnh con.",
      },
      {
        id: "b2",
        blankIndex: 2,
        expected: "ToListAsync",
        acceptableAnswers: ["ToListAsync", "ToListAsync()"],
        hint: "Phương thức bất đồng bộ giải phóng luồng trong khi chờ kết quả từ SQL.",
        chips: ["ToListAsync", "ToList", "ToArray", "ExecuteReaderAsync"],
        explanation: "ToListAsync() gửi câu lệnh sang Database và nhả ThreadPool thread hiện tại, tăng khả năng chịu tải hàng nghìn concurrent requests.",
      },
    ],
    lineExplanations: [
      {
        lineNumber: 4,
        codeSnippet: ".AsNoTracking()",
        purpose: "Vô hiệu hóa ChangeTracker của EF Core cho các truy vấn chỉ đọc (read-only).",
        riskIfWrong: "Nếu để mặc định (Tracking), EF Core lưu bản sao mọi đối tượng tải về vào RAM, làm tăng chi phí Garbage Collection và dễ tràn bộ nhớ.",
      },
      {
        lineNumber: 5,
        codeSnippet: ".Include(c => c.Lessons)",
        purpose: "Eager loading bảng Lessons trong cùng lệnh truy vấn SQL.",
        riskIfWrong: "Nếu duyệt foreach gọi lazy load, hệ thống sẽ thực thi 1 + N câu lệnh SQL (N+1 Query) gây nghẽn kết nối Database.",
      },
      {
        lineNumber: 8,
        codeSnippet: ".ToListAsync()",
        purpose: "Thực thi truy vấn I/O bất đồng bộ không phong tỏa CPU worker thread.",
        riskIfWrong: "Nếu dùng .ToList() đồng bộ, thread sẽ bị phong tỏa chờ Database phản hồi, dễ dẫn tới ThreadPool Starvation dưới tải lớn.",
      },
    ],
  },
  {
    id: "dotnet-webapi-controller",
    title: "ASP.NET Core Web API: Controller Action Chuẩn REST & DTO Validation",
    category: "dotnet",
    language: "csharp",
    description: "Xây dựng endpoint tạo mới bài tập với kiểm tra ModelState, mã trạng thái HTTP 201 CreatedAtAction.",
    practicalScenario: "Khi học viên gửi form tạo bài tập mới, API cần xác minh dữ liệu hợp lệ và trả về Header Location trỏ đến resource vừa tạo chuẩn RESTful.",
    codeTemplate: [
      "[HttpPost]",
      "public async Task<{{BLANK_0}}> CreateLessonAsync([FromBody] CreateLessonDto dto)",
      "{",
      "    if (!{{BLANK_1}}.IsValid)",
      "        return BadRequest(ModelState);",
      "    ",
      "    var lesson = await _lessonService.CreateLessonAsync(dto);",
      "    return {{BLANK_2}}(nameof(GetById), new { id = lesson.Id }, lesson);",
      "}",
    ],
    blanks: [
      {
        id: "b0",
        blankIndex: 0,
        expected: "IActionResult",
        acceptableAnswers: ["IActionResult", "ActionResult<LessonDto>", "ActionResult"],
        hint: "Interface chuẩn trả về kết quả HTTP trong ASP.NET Core MVC/Web API.",
        chips: ["IActionResult", "ActionResult", "Task", "HttpResponseMessage"],
        explanation: "IActionResult cho phép linh hoạt trả về các mã HTTP status khác nhau như Ok, BadRequest, CreatedAtAction.",
      },
      {
        id: "b1",
        blankIndex: 1,
        expected: "ModelState",
        acceptableAnswers: ["ModelState"],
        hint: "Đối tượng lưu trạng thái validation từ DataAnnotations hoặc FluentValidation.",
        chips: ["ModelState", "ModelBinding", "ValidationContext", "Request"],
        explanation: "ModelState.IsValid kiểm tra xem các quy tắc như [Required], [MaxLength] có bị vi phạm hay không.",
      },
      {
        id: "b2",
        blankIndex: 2,
        expected: "CreatedAtAction",
        acceptableAnswers: ["CreatedAtAction"],
        hint: "Phương thức trả về HTTP 201 Created kèm Header Location trỏ tới resource.",
        chips: ["CreatedAtAction", "Ok", "Created", "Accepted"],
        explanation: "CreatedAtAction trả về HTTP 201 kèm URL dẫn tới tài nguyên vừa được tạo thành công.",
      },
    ],
    lineExplanations: [
      {
        lineNumber: 1,
        codeSnippet: "[HttpPost]",
        purpose: "Chỉ định hành động này lắng nghe phương thức HTTP POST tạo mới tài nguyên.",
        riskIfWrong: "Nếu không gán thuộc tính hoặc nhầm thành [HttpGet], request gửi payload body sẽ bị từ chối hoặc lỗi định tuyến.",
      },
      {
        lineNumber: 4,
        codeSnippet: "if (!ModelState.IsValid) return BadRequest(ModelState);",
        purpose: "Chặn đứng các payload sai định dạng trước khi đi sâu vào tầng nghiệp vụ (Domain Layer).",
        riskIfWrong: "Nếu bỏ qua kiểm tra, dữ liệu rác hoặc giá trị null sẽ đi vào database gây lỗi NullReferenceException hoặc sai lệch dữ liệu.",
      },
      {
        lineNumber: 8,
        codeSnippet: "return CreatedAtAction(nameof(GetById), ...);",
        purpose: "Trả về mã HTTP 201 chuẩn REST kèm header Location.",
        riskIfWrong: "Nếu chỉ trả 200 OK đơn thuần, các client chuẩn RESTful sẽ không tự động nhận biết URI của đối tượng mới tạo.",
      },
    ],
  },
  {
    id: "react-effect-cleanup",
    title: "React Hooks: Đồng Bộ Cửa Sổ & Dọn Dẹp Tránh Memory Leak",
    category: "react",
    language: "typescript",
    description: "Lắng nghe sự kiện đổi kích thước màn hình và hủy listener khi component unmount.",
    practicalScenario: "Người học chuyển qua lại giữa các bài giảng nhiều lần. Nếu không dọn dẹp listener, các hàm cũ vẫn bị giữ lại trong bộ nhớ gây rò rỉ RAM và tính sai layout.",
    codeTemplate: [
      "useEffect(() => {",
      "  const handleResize = () => setWindowWidth(window.innerWidth);",
      "  window.{{BLANK_0}}('resize', handleResize);",
      "  ",
      "  // Hàm dọn dẹp (cleanup function) trước khi unmount",
      "  return () => {",
      "    window.{{BLANK_1}}('resize', handleResize);",
      "  };",
      "}, [{{BLANK_2}}]); // Dependency array phù hợp",
    ],
    blanks: [
      {
        id: "b0",
        blankIndex: 0,
        expected: "addEventListener",
        acceptableAnswers: ["addEventListener"],
        hint: "Phương thức DOM đăng ký lắng nghe sự kiện từ trình duyệt.",
        chips: ["addEventListener", "attachEvent", "on", "subscribe"],
        explanation: "window.addEventListener('resize', ...) bắt đầu lắng nghe các thay đổi kích thước từ trình duyệt.",
      },
      {
        id: "b1",
        blankIndex: 1,
        expected: "removeEventListener",
        acceptableAnswers: ["removeEventListener"],
        hint: "Phương thức DOM hủy đăng ký lắng nghe sự kiện để giải phóng bộ nhớ.",
        chips: ["removeEventListener", "detachEvent", "off", "unsubscribe"],
        explanation: "window.removeEventListener('resize', ...) đảm bảo xóa hàm callback khỏi DOM, ngăn chặn Memory Leak khi người học rời màn hình.",
      },
      {
        id: "b2",
        blankIndex: 2,
        expected: "",
        acceptableAnswers: ["", " "],
        hint: "Mảng rỗng [] nghĩa là chỉ chạy 1 lần duy nhất lúc mount.",
        chips: ["[]", "[windowWidth]", "[setWindowWidth]"],
        explanation: "Mảng phụ thuộc rỗng [] đảm bảo listener chỉ được gắn 1 lần khi component khởi tạo và dọn dẹp khi unmount.",
      },
    ],
    lineExplanations: [
      {
        lineNumber: 3,
        codeSnippet: "window.addEventListener('resize', handleResize);",
        purpose: "Gắn hàm lắng nghe sự kiện thay đổi khung nhìn trình duyệt.",
        riskIfWrong: "Nếu gắn nhầm hoặc gọi nhiều lần trong render, hàm sẽ bị nhân bản liên tục.",
      },
      {
        lineNumber: 7,
        codeSnippet: "return () => window.removeEventListener('resize', handleResize);",
        purpose: "Cleanup function trả về từ useEffect, chạy khi unmount hoặc trước lần re-run tiếp theo.",
        riskIfWrong: "Nếu quên dọn dẹp, closure cũ giữ tham chiếu tới component đã hủy, sinh ra cảnh báo rò rỉ bộ nhớ.",
      },
      {
        lineNumber: 9,
        codeSnippet: "}, []);",
        purpose: "Mảng rỗng để useEffect chỉ kích hoạt một lần duy nhất khi render lần đầu.",
        riskIfWrong: "Nếu quên mảng phụ thuộc (bỏ trống hoàn toàn), useEffect sẽ chạy lại ở MỖI lần component render, gắn hàng trăm event listener.",
      },
    ],
  },
  {
    id: "sql-parameterized-security",
    title: "SQL & Security: Truy Vấn Tham Số Chống Tấn Công SQL Injection",
    category: "sql",
    language: "sql",
    description: "Viết truy vấn lọc bài tập của học viên an toàn tuyệt đối với Parameterized Query thay vì nối chuỗi.",
    practicalScenario: "Hacker nhập chuỗi ' OR '1'='1 vào ô tìm kiếm để xem trộm bài tập và điểm của toàn bộ học viên khác trong hệ thống.",
    codeTemplate: [
      "-- Truy vấn kết hợp bảng có chỉ mục và tham số an toàn",
      "SELECT s.Id, s.FullName, COUNT(p.Id) AS TotalSubmissions",
      "FROM Students s",
      "{{BLANK_0}} Submissions p ON s.Id = p.StudentId",
      "WHERE s.CourseId = {{BLANK_1}}",
      "GROUP BY s.Id, s.FullName",
      "HAVING COUNT(p.Id) >= {{BLANK_2}};",
    ],
    blanks: [
      {
        id: "b0",
        blankIndex: 0,
        expected: "LEFT JOIN",
        acceptableAnswers: ["LEFT JOIN", "LEFT OUTER JOIN", "INNER JOIN"],
        hint: "Dùng JOIN để giữ lại cả những học viên chưa nộp bài nào.",
        chips: ["LEFT JOIN", "INNER JOIN", "CROSS JOIN", "RIGHT JOIN"],
        explanation: "LEFT JOIN giữ lại đầy đủ học viên kể cả khi họ chưa nộp bài (COUNT sẽ ra 0), tránh làm mất dữ liệu học viên mới.",
      },
      {
        id: "b1",
        blankIndex: 1,
        expected: "@CourseId",
        acceptableAnswers: ["@CourseId", ":CourseId", "$1"],
        hint: "Tên tham số chuẩn trong SQL Server / SQLite (bắt đầu bằng @).",
        chips: ["@CourseId", "' + courseId + '", "$CourseId", "courseId"],
        explanation: "@CourseId là tham số được cơ chế Database biên dịch riêng biệt với câu lệnh, loại trừ hoàn toàn nguy cơ SQL Injection.",
      },
      {
        id: "b2",
        blankIndex: 2,
        expected: "@MinSubmissions",
        acceptableAnswers: ["@MinSubmissions", ":MinSubmissions", "1", "0"],
        hint: "Tham số lọc số bài nộp tối thiểu trong mệnh đề HAVING.",
        chips: ["@MinSubmissions", "MinSubmissions", "'1'", "COUNT(*)"],
        explanation: "HAVING lọc trên kết quả gom nhóm (aggregate function), cũng phải dùng tham số hóa để bảo vệ an toàn.",
      },
    ],
    lineExplanations: [
      {
        lineNumber: 4,
        codeSnippet: "LEFT JOIN Submissions p ON s.Id = p.StudentId",
        purpose: "Kết nối danh sách bài nộp với học viên mà không loại bỏ học viên chưa có bài.",
        riskIfWrong: "Nếu dùng INNER JOIN, những học viên chưa kịp nộp bài sẽ biến mất hoàn toàn khỏi danh sách thống kê.",
      },
      {
        lineNumber: 5,
        codeSnippet: "WHERE s.CourseId = @CourseId",
        purpose: "Lọc theo khóa học sử dụng tham số hóa do Database Driver xử lý.",
        riskIfWrong: "Nếu dùng phép cộng chuỗi (' + input + '), kẻ tấn công có thể chèn mã độc chiếm quyền kiểm soát cơ sở dữ liệu.",
      },
      {
        lineNumber: 7,
        codeSnippet: "HAVING COUNT(p.Id) >= @MinSubmissions;",
        purpose: "Lọc sau phép gom nhóm GROUP BY theo điều kiện tổng số lượng.",
        riskIfWrong: "Nếu đặt điều kiện này vào mệnh đề WHERE, câu truy vấn sẽ báo lỗi cú pháp vì aggregate function không chạy trong WHERE.",
      },
    ],
  },
  {
    id: "dotnet-parallel-async",
    title: "C# Async/Await: Chạy Tác Vụ Song Song Bằng Task.WhenAll",
    category: "dotnet",
    language: "csharp",
    description: "Đồng thời lấy hồ sơ người dùng, thông báo và lịch học để giảm thời gian phản hồi từ 600ms xuống 200ms.",
    practicalScenario: "Màn hình dashboard gọi 3 API độc lập nhau. Nếu dùng tuần tự 3 lệnh await, người dùng phải chờ tổng thời gian của cả 3 truy vấn cộng lại.",
    codeTemplate: [
      "public async Task<DashboardData> LoadDashboardAsync(Guid userId)",
      "{",
      "    var profileTask = _userService.GetProfileAsync(userId);",
      "    var coursesTask = _courseService.GetEnrolledCoursesAsync(userId);",
      "    var statsTask = _analyticsService.GetStudyStreakAsync(userId);",
      "    ",
      "    // Chạy song song cả 3 tác vụ I/O cùng lúc",
      "    await {{BLANK_0}}.{{BLANK_1}}(profileTask, coursesTask, statsTask);",
      "    ",
      "    return new DashboardData(await {{BLANK_2}}, await coursesTask, await statsTask);",
      "}",
    ],
    blanks: [
      {
        id: "b0",
        blankIndex: 0,
        expected: "Task",
        acceptableAnswers: ["Task"],
        hint: "Lớp cốt lõi trong System.Threading.Tasks đại diện cho tác vụ bất đồng bộ.",
        chips: ["Task", "Thread", "Parallel", "AsyncContext"],
        explanation: "Task là lớp tĩnh cung cấp các tiện ích điều phối nhiều tác vụ bất đồng bộ trong C#.",
      },
      {
        id: "b1",
        blankIndex: 1,
        expected: "WhenAll",
        acceptableAnswers: ["WhenAll", "WhenAll()"],
        hint: "Phương thức chờ đợi tất cả các Task được truyền vào hoàn thành.",
        chips: ["WhenAll", "WaitAll", "WhenAny", "RunSynchronously"],
        explanation: "Task.WhenAll trả về một Task hoàn tất khi toàn bộ các tác vụ con kết thúc mà không phong tỏa thread nào.",
      },
      {
        id: "b2",
        blankIndex: 2,
        expected: "profileTask",
        acceptableAnswers: ["profileTask"],
        hint: "Biến tác vụ đã khởi chạy lấy thông tin hồ sơ ở dòng 3.",
        chips: ["profileTask", "coursesTask", "statsTask", "userId"],
        explanation: "Vì profileTask đã hoàn thành sau Task.WhenAll, việc await nó sẽ trích xuất kết quả tức thời mà không tốn thêm thời gian I/O.",
      },
    ],
    lineExplanations: [
      {
        lineNumber: 3,
        codeSnippet: "var profileTask = _userService.GetProfileAsync(userId);",
        purpose: "Kích hoạt tác vụ bất đồng bộ mà KHÔNG dùng từ khóa await ngay lập tức.",
        riskIfWrong: "Nếu đặt await ngay ở dòng này, code sẽ bị dừng lại chờ xong mới chạy dòng tiếp, làm mất tính song song.",
      },
      {
        lineNumber: 8,
        codeSnippet: "await Task.WhenAll(profileTask, coursesTask, statsTask);",
        purpose: "Chờ cả 3 tác vụ I/O hoàn tất đồng thời.",
        riskIfWrong: "Nếu dùng Task.WaitAll(...) đồng bộ, bạn sẽ khóa cứng ThreadPool thread, gây lãng phí tài nguyên CPU nghiêm trọng.",
      },
    ],
  },
];

// ==========================================
// DỮ LIỆU CHẾ ĐỘ 2: SĂN LỖI THỰC CHIẾN (BUG HUNTER)
// 6+ TÌNH HUỐNG LỖI KINH ĐIỂN
// ==========================================
interface BugHunterScenario {
  id: string;
  title: string;
  categoryTag: string;
  context: string;
  codeLines: string[];
  bugLineNumber: number; // 1-based index
  bugBriefDescription: string;
  rootCauseAnalysis: string;
  realWorldImpact: string;
  fixOptions: {
    id: string;
    label: string;
    isCorrect: boolean;
    explanation: string;
  }[];
  cleanFixedCode: string;
}

const BUG_HUNTER_SCENARIOS: BugHunterScenario[] = [
  {
    id: "bug-ef-nplusone",
    title: "1. C# / EF Core: Lỗi N+1 Query & Bỏ Quên AsNoTracking",
    categoryTag: "C# / EF Core & Database",
    context: "Hàm lấy danh mục khóa học kèm số lượng bài học hiển thị cho trang chủ. Khi hệ thống có 200 khóa học, trang web mất tới 15 giây để nạp và làm quá tải kết nối SQL.",
    codeLines: [
      "public async Task<List<CourseSummaryDto>> GetCatalog(AppDbContext db)",
      "{",
      "    var courses = await db.Courses.ToListAsync();",
      "    var result = new List<CourseSummaryDto>();",
      "    foreach (var c in courses)",
      "    {",
      "        // Lấy số lượng bài học cho từng khóa học",
      "        var count = db.Lessons.Count(l => l.CourseId == c.Id);",
      "        result.Add(new CourseSummaryDto(c.Title, count));",
      "    }",
      "    return result;",
      "}",
    ],
    bugLineNumber: 8,
    bugBriefDescription: "Dòng 8: Gọi truy vấn db.Lessons.Count(...) bên trong vòng lặp foreach (Lỗi N+1 Query)",
    rootCauseAnalysis: "Truy vấn dòng 3 lấy N bản ghi Courses. Vòng lặp foreach thực thi thêm N câu lệnh SELECT COUNT(*) riêng biệt xuống cơ sở dữ liệu. Tổng cộng thực thi 1 + N truy vấn, gây tắc nghẽn Pool kết nối và tăng độ trễ mạng theo cấp số nhân.",
    realWorldImpact: "Trên môi trường production với 500 khóa học, mỗi lượt xem trang chủ kích hoạt 501 câu lệnh SQL. Database CPU vọt lên 100%, làm sập kết nối của các dịch vụ khác.",
    fixOptions: [
      {
        id: "f1",
        label: "Dùng GroupBy hoặc phép chiếu LINQ Select() trực tiếp trong 1 truy vấn duy nhất kèm AsNoTracking()",
        isCorrect: true,
        explanation: "EF Core sẽ dịch toàn bộ sang 1 câu lệnh SQL duy nhất có LEFT JOIN và GROUP BY, thực hiện tính toán trên server DB và trả về kết quả ngay lập tức.",
      },
      {
        id: "f2",
        label: "Thêm từ khóa await trước db.Lessons.CountAsync(...) bên trong vòng lặp",
        isCorrect: false,
        explanation: "Chuyển sang CountAsync chỉ giúp không block luồng CPU nhưng vẫn thực hiện N lần round-trip qua mạng xuống DB, không giải quyết được căn nguyên N+1 Query.",
      },
      {
        id: "f3",
        label: "Tăng kích thước Connection Pool trong chuỗi kết nối appsettings.json",
        isCorrect: false,
        explanation: "Đây là cách chữa cháy tạm thời và chỉ làm Database quá tải thêm chứ không sửa được thuật toán truy vấn sai.",
      },
    ],
    cleanFixedCode: `public async Task<List<CourseSummaryDto>> GetCatalog(AppDbContext db)
{
    return await db.Courses
        .AsNoTracking()
        .Select(c => new CourseSummaryDto(
            c.Title,
            c.Lessons.Count() // EF Core tự động gom nhóm thành 1 câu SQL duy nhất
        ))
        .ToListAsync();
}`,
  },
  {
    id: "bug-race-condition",
    title: "2. ASP.NET Core: Race Condition Trừ Số Dư Không Có Lock",
    categoryTag: "ASP.NET Core & Concurrency",
    context: "Endpoint thanh toán hoặc trừ điểm học tập. Khi người dùng bấm nút liên tục hai lần trên hai thiết bị trong cùng 1 tích tắc, tài khoản bị trừ 1 lần nhưng hệ thống ghi nhận thành công cả 2 giao dịch.",
    codeLines: [
      "public async Task<bool> DeductBalance(Guid userId, decimal amount)",
      "{",
      "    var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);",
      "    if (wallet == null || wallet.Balance < amount) return false;",
      "    ",
      "    // Giả lập độ trễ kiểm tra hoặc tính toán chiết khấu",
      "    await Task.Delay(50);",
      "    ",
      "    wallet.Balance -= amount;",
      "    await _db.SaveChangesAsync();",
      "    return true;",
      "}",
    ],
    bugLineNumber: 9,
    bugBriefDescription: "Dòng 9: Trừ số dư dựa trên giá trị đã đọc trước đó mà không có Lock, Transaction hoặc Optimistic Concurrency",
    rootCauseAnalysis: "Hai request đồng thời (Thread A và B) cùng đọc dòng 3 khi số dư là 100,000đ. Cả 2 đều pass kiểm tra số dư ở dòng 4. Sau đó cả 2 cùng trừ và ghi đè giá trị còn lại, dẫn đến thất thoát tiền hoặc điểm học tập của nền tảng.",
    realWorldImpact: "Lỗ hổng trục lợi ví tiền ảo / voucher / đăng ký khóa học quá giới hạn suất học (Overselling). Kẻ xấu có thể rút tiền gấp nhiều lần số dư thực tế.",
    fixOptions: [
      {
        id: "f1",
        label: "Dùng câu lệnh SQL nguyên tử (Atomic Update) hoặc Optimistic Concurrency Token ([Timestamp])",
        isCorrect: true,
        explanation: "Thực thi 'UPDATE Wallets SET Balance = Balance - @amount WHERE UserId = @id AND Balance >= @amount' đảm bảo tầng Database khóa dòng trong mili-giây cập nhật.",
      },
      {
        id: "f2",
        label: "Dùng từ khóa lock (object) trong C# bao bọc toàn bộ hàm",
        isCorrect: false,
        explanation: "Không thể dùng lệnh 'lock' chứa 'await' trong C# (trình biên dịch sẽ báo lỗi CS1996), đồng thời lock trong RAM không hoạt động khi chạy nhiều container / server web.",
      },
      {
        id: "f3",
        label: "Chỉ cần bỏ dòng Task.Delay(50) là hết lỗi",
        isCorrect: false,
        explanation: "Bỏ Task.Delay chỉ làm khe hở thời gian nhỏ lại nhưng dưới tải cao, race condition vẫn xảy ra đều đặn giữa các luồng CPU.",
      },
    ],
    cleanFixedCode: `public async Task<bool> DeductBalance(Guid userId, decimal amount)
{
    // Thực thi cập nhật nguyên tử ở tầng Database:
    var rowsAffected = await _db.Database.ExecuteSqlInterpolatedAsync(
        $"UPDATE Wallets SET Balance = Balance - {amount} WHERE UserId = {userId} AND Balance >= {amount}"
    );
    return rowsAffected > 0;
}`,
  },
  {
    id: "bug-react-stale-closure",
    title: "3. React Hooks: Stale Closure & Quên Dọn Dẹp Trong useEffect",
    categoryTag: "React & Hooks Lifecycle",
    context: "Thành phần đếm ngược thời gian làm bài thi (ExamTimer). Đồng hồ dừng lại sau giây đầu tiên hoặc chạy loạn xạ khi người học chuyển câu hỏi.",
    codeLines: [
      "export function ExamTimer({ onTimeout }: { onTimeout: () => void }) {",
      "  const [secondsLeft, setSecondsLeft] = useState(60);",
      "  ",
      "  useEffect(() => {",
      "    const timer = setInterval(() => {",
      "      // Đếm ngược mỗi giây",
      "      setSecondsLeft(secondsLeft - 1);",
      "    }, 1000);",
      "  }, []); // Thiếu hàm cleanup và bị stale closure",
      "  return <div className=\"font-mono\">{secondsLeft}s</div>;",
      "}",
    ],
    bugLineNumber: 7,
    bugBriefDescription: "Dòng 7: Tham chiếu trực tiếp biến state 'secondsLeft' trong setInterval đóng băng và thiếu cleanup clearInterval",
    rootCauseAnalysis: "1) Stale Closure: Callback trong setInterval lưu trữ giá trị 'secondsLeft = 60' tại thời điểm mount. Mỗi giây hàm chạy, nó luôn tính 60 - 1 = 59, khiến đồng hồ bị kẹt ở 59s. 2) Memory Leak: Thiếu 'return () => clearInterval(timer)' khiến timer tiếp tục chạy ngầm khi component unmount.",
    realWorldImpact: "Đồng hồ hiển thị sai thời gian thi cử, người học bị khóa bài sớm hoặc tiếp tục chạy ngầm tiêu hao pin thiết bị di động.",
    fixOptions: [
      {
        id: "f1",
        label: "Dùng hàm cập nhật functional update 'setSecondsLeft(prev => prev - 1)' và return () => clearInterval(timer)",
        isCorrect: true,
        explanation: "Functional update luôn nhận giá trị state mới nhất mà không phụ thuộc vào closure của effect, đồng thời hàm cleanup giải phóng timer khi unmount.",
      },
      {
        id: "f2",
        label: "Thêm secondsLeft vào mảng dependency [secondsLeft]",
        isCorrect: false,
        explanation: "Cách này sẽ tạo mới và hủy interval liên tục mỗi giây một lần, gây giật lag và không phải là best practice cho interval đếm giờ.",
      },
      {
        id: "f3",
        label: "Đổi sang dùng biến toàn cục let secondsLeft = 60",
        isCorrect: false,
        explanation: "Biến toàn cục sẽ bị chia sẻ giữa tất cả các thí sinh và không kích hoạt re-render UI khi thay đổi.",
      },
    ],
    cleanFixedCode: `export function ExamTimer({ onTimeout }: { onTimeout: () => void }) {
  const [secondsLeft, setSecondsLeft] = useState(60);

  useEffect(() => {
    const timer = setInterval(() => {
      setSecondsLeft((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
          onTimeout();
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    // Dọn dẹp sạch timer khi unmount
    return () => clearInterval(timer);
  }, [onTimeout]);

  return <div className="font-mono text-lg font-bold">{secondsLeft}s</div>;
}`,
  },
  {
    id: "bug-sql-injection",
    title: "4. SQL Security: Lỗ Hổng Nối Chuỗi Trực Tiếp (SQL Injection)",
    categoryTag: "Security & Database",
    context: "Tính năng tìm kiếm học viên theo email hoặc tên trong trang quản trị viện. Mã nguồn sử dụng nội suy chuỗi $ trực tiếp vào câu lệnh SQL thô.",
    codeLines: [
      "public async Task<User?> SearchUserByEmail(string inputEmail)",
      "{",
      "    // Nhận trực tiếp chuỗi từ request của người dùng",
      "    string query = $\"SELECT * FROM Users WHERE Email = '{inputEmail}'\";",
      "    ",
      "    var user = await _context.Users",
      "        .FromSqlRaw(query)",
      "        .FirstOrDefaultAsync();",
      "    return user;",
      "}",
    ],
    bugLineNumber: 4,
    bugBriefDescription: "Dòng 4: Nối chuỗi nội suy $\"SELECT ... '{inputEmail}'\" đưa trực tiếp input vào parser SQL",
    rootCauseAnalysis: "Kẻ tấn công có thể truyền chuỗi độc hại: \"admin@swms.vn' OR '1'='1\" hoặc \"'; DROP TABLE Logs; --\". Trình biên dịch SQL hiểu chuỗi này như mã lệnh thực thi thay vì dữ liệu, phá vỡ logic kiểm tra quyền.",
    realWorldImpact: "Rò rỉ toàn bộ cơ sở dữ liệu học viên, lộ mật khẩu mã hóa (hash) và có thể dẫn tới chiếm quyền máy chủ nếu Database chạy quyền cao.",
    fixOptions: [
      {
        id: "f1",
        label: "Dùng FromSqlInterpolated hoặc truyền SqlParameter để cơ sở dữ liệu xử lý an toàn",
        isCorrect: true,
        explanation: "FromSqlInterpolated trong EF Core tự động bọc chuỗi nội suy thành các DbParameter riêng biệt, vô hiệu hóa hoàn toàn ký tự thoát.",
      },
      {
        id: "f2",
        label: "Dùng hàm inputEmail.Replace(\"'\", \"\") để xóa dấu nháy đơn",
        isCorrect: false,
        explanation: "Sanitization bằng regex thủ công rất dễ bị bypass bằng các kỹ thuật encode như hex, comment SQL hoặc ký tự unicode đặc biệt.",
      },
      {
        id: "f3",
        label: "Đổi cổng kết nối SQL Server sang cổng khác 1433",
        isCorrect: false,
        explanation: "Đổi port mạng không có tác dụng bảo vệ khi lỗ hổng nằm ở tầng logic ứng dụng web.",
      },
    ],
    cleanFixedCode: `public async Task<User?> SearchUserByEmail(string inputEmail)
{
    // Dùng FromSqlInterpolated để EF Core tự động parameterize an toàn tuyệt đối:
    return await _context.Users
        .FromSqlInterpolated($"SELECT * FROM Users WHERE Email = {inputEmail}")
        .AsNoTracking()
        .FirstOrDefaultAsync();
}`,
  },
  {
    id: "bug-react-unhandled-fetch",
    title: "5. React: Fetch Không Hủy AbortController Gây Race Condition & Leak",
    categoryTag: "React & Network State",
    context: "Người học bấm chuyển nhanh giữa các bài học số 1, số 2 và số 3. Mạng bài 1 chập chờn phản hồi chậm hơn bài 3, khiến nội dung bài 1 đè lên hiển thị của bài 3.",
    codeLines: [
      "useEffect(() => {",
      "  setIsLoading(true);",
      "  // Gửi request tải nội dung bài học",
      "  fetch(`/api/lessons/${lessonId}`)",
      "    .then(res => res.json())",
      "    .then(data => {",
      "      setLessonData(data);",
      "      setIsLoading(false);",
      "    });",
      "}, [lessonId]); // Không có cơ chế hủy request đang bay",
    ],
    bugLineNumber: 4,
    bugBriefDescription: "Dòng 4: Gọi fetch thuần mà không cung cấp AbortSignal và không có cờ hủy bỏ",
    rootCauseAnalysis: "Khi lessonId thay đổi từ A sang B, request A vẫn đang xử lý trên mạng. Nếu mạng chậm, request A hoàn thành sau request B, hàm setLessonData(data) của A sẽ ghi đè dữ liệu của B (Network Race Condition) và gây rò rỉ nếu component unmount.",
    realWorldImpact: "Người học nhìn thấy tiêu đề bài 3 nhưng nội dung bài tập lại hiển thị của bài 1, nộp bài bị chấm sai hoàn toàn.",
    fixOptions: [
      {
        id: "f1",
        label: "Khởi tạo AbortController, truyền signal vào fetch và gọi controller.abort() trong return cleanup",
        isCorrect: true,
        explanation: "Khi người học đổi bài hoặc unmount, trình duyệt sẽ ngắt kết nối HTTP ngay lập tức, bỏ qua callback setLessonData cũ.",
      },
      {
        id: "f2",
        label: "Bọc hàm fetch trong try / catch là giải quyết xong",
        isCorrect: false,
        explanation: "Try/catch chỉ bắt lỗi runtime, không thể ngăn chặn callback `.then()` chạy khi một request cũ phản hồi muộn.",
      },
      {
        id: "f3",
        label: "Giảm thời gian setTimeout của request trên server backend",
        isCorrect: false,
        explanation: "Độ trễ mạng của người dùng di động (4G/Wifi chập chờn) là yếu tố không thể kiểm soát từ server.",
      },
    ],
    cleanFixedCode: `useEffect(() => {
  const controller = new AbortController();
  setIsLoading(true);

  fetch(\`/api/lessons/\${lessonId}\`, { signal: controller.signal })
    .then((res) => res.json())
    .then((data) => {
      setLessonData(data);
      setIsLoading(false);
    })
    .catch((err) => {
      if (err.name !== "AbortError") {
        console.error("Lỗi tải bài:", err);
        setIsLoading(false);
      }
    });

  // Hủy request ngay khi đổi lessonId hoặc unmount
  return () => controller.abort();
}, [lessonId]);`,
  },
  {
    id: "bug-dotnet-async-deadlock",
    title: "6. C# Async: Sync-over-Async (.Result / .Wait()) Gây Deadlock",
    categoryTag: "C# Async & ThreadPool",
    context: "Trong API Controller ASP.NET Core, lập trình viên gọi hàm bất đồng bộ nhưng dùng đuôi .Result hoặc .Wait() để lấy dữ liệu đồng bộ.",
    codeLines: [
      "[HttpGet(\"profile\")]",
      "public IActionResult GetUserProfile()",
      "{",
      "    var userId = User.GetUserId();",
      "    // Chờ kết quả bất đồng bộ bằng thuộc tính đồng bộ .Result",
      "    var profile = _userService.GetProfileAsync(userId).Result;",
      "    return Ok(profile);",
      "}",
    ],
    bugLineNumber: 6,
    bugBriefDescription: "Dòng 6: Sử dụng .Result trên một Task bất đồng bộ (Sync-over-Async Pattern)",
    rootCauseAnalysis: "Thuộc tính .Result phong tỏa (block) thread hiện tại của ThreadPool cho đến khi Task hoàn thành. Nếu GetProfileAsync bên trong cần thread để tiếp tục (hoặc trong môi trường có SynchronizationContext), thread bị block không thể nhả ra, tạo nên tình trạng Deadlock hoặc ThreadPool Starvation dưới tải cao.",
    realWorldImpact: "Server bị treo toàn bộ API dù CPU sử dụng chưa tới 5%. Tất cả request mới đều rơi vào hàng đợi và trả về lỗi 504 Gateway Timeout.",
    fixOptions: [
      {
        id: "f1",
        label: "Chuyển hàm sang async Task<IActionResult> và dùng 'await _userService.GetProfileAsync(userId)'",
        isCorrect: true,
        explanation: "Từ khóa await giải phóng thread hiện tại quay về ThreadPool phục vụ request khác trong khi chờ I/O mạng / DB hoàn tất.",
      },
      {
        id: "f2",
        label: "Dùng Task.Run(() => _userService.GetProfileAsync(userId)).Result",
        isCorrect: false,
        explanation: "Cách này lại tiêu tốn thêm một thread nữa của ThreadPool và vẫn giữ nguyên nguy cơ nghẽn ThreadPool Starvation.",
      },
      {
        id: "f3",
        label: "Bỏ thuộc tính .Result và trả về chính đối tượng Task trong Ok()",
        isCorrect: false,
        explanation: "Next.js / Client sẽ nhận về một đối tượng JSON đại diện cho metadata của Task C# thay vì dữ liệu hồ sơ người dùng thực tế.",
      },
    ],
    cleanFixedCode: `[HttpGet("profile")]
public async Task<IActionResult> GetUserProfile()
{
    var userId = User.GetUserId();
    // Luôn await từ đầu đến cuối (Async all the way):
    var profile = await _userService.GetProfileAsync(userId);
    return Ok(profile);
}`,
  },
  {
    id: "bug-os-deadlock-lock-inversion",
    title: "7. Hệ Điều Hành: Nghịch Đảo Thứ Tự Khóa (Lock Ordering Inversion)",
    categoryTag: "Hệ Điều Hành & Concurrency",
    context: "Chuyển tiền giữa 2 tài khoản qua dịch vụ ngân hàng. Hai luồng thực hiện chuyển tiền qua lại đồng thời: Luồng 1 chuyển A sang B, Luồng 2 chuyển B sang A.",
    codeLines: [
      "public void TransferFunds(Account source, Account target, decimal amount)",
      "{",
      "    // Khóa tài khoản nguồn trước, tài khoản đích sau",
      "    lock (source.SyncRoot)",
      "    {",
      "        lock (target.SyncRoot)",
      "        {",
      "            source.Balance -= amount;",
      "            target.Balance += amount;",
      "        }",
      "    }",
      "}",
    ],
    bugLineNumber: 6,
    bugBriefDescription: "Dòng 6: Khóa tài khoản target mà không sắp xếp thứ tự ưu tiên lock (Lock Inversion)",
    rootCauseAnalysis: "Nếu Luồng 1 gọi Transfer(A, B) nắm khóa A chờ khóa B; cùng lúc Luồng 2 gọi Transfer(B, A) nắm khóa B chờ khóa A. Cả 2 luồng rơi vào điều kiện Chờ Vòng Tròn (Circular Wait) của Deadlock Coffman.",
    realWorldImpact: "Toàn bộ giao dịch ngân hàng của 2 tài khoản bị đóng băng vĩnh viễn, kéo sập chuỗi xử lý background batching của toàn hệ thống.",
    fixOptions: [
      {
        id: "f1",
        label: "Luôn khóa theo thứ tự ID tăng dần (Global Lock Ordering) hoặc dùng Monitor.TryEnter với Timeout",
        isCorrect: true,
        explanation: "So sánh source.Id và target.Id để luôn khóa tài khoản có Id nhỏ hơn trước, triệt tiêu hoàn toàn điều kiện Circular Wait.",
      },
      {
        id: "f2",
        label: "Đảo lại: Khóa target trước rồi mới khóa source",
        isCorrect: false,
        explanation: "Đổi thứ tự chỉ chuyển vị trí lỗi sang phía ngược lại, bản chất Deadlock khi gọi đồng thời vẫn giữ nguyên.",
      },
      {
        id: "f3",
        label: "Tăng dung lượng RAM máy chủ để chứa được nhiều luồng hơn",
        isCorrect: false,
        explanation: "Deadlock là lỗi logic đồng bộ hóa (synchronization), không liên quan tới dung lượng phần cứng.",
      },
    ],
    cleanFixedCode: `public void TransferFunds(Account source, Account target, decimal amount)
{
    // Xác định thứ tự khóa toàn cục cố định theo ID:
    var firstLock = source.Id < target.Id ? source.SyncRoot : target.SyncRoot;
    var secondLock = source.Id < target.Id ? target.SyncRoot : source.SyncRoot;

    lock (firstLock)
    {
        lock (secondLock)
        {
            source.Balance -= amount;
            target.Balance += amount;
        }
    }
}`,
  },
];

// ==========================================
// DỮ LIỆU CHẾ ĐỘ 3: MÔ PHỎNG LUỒNG HỆ THỐNG
// ==========================================
interface RequestStage {
  id: string;
  name: string;
  tech: string;
  badgeColor: string;
  inputDescription: string;
  outputDescription: string;
  codeSnippet: string;
  failureScenario: string;
}

const REQUEST_STAGES: RequestStage[] = [
  {
    id: "client",
    name: "1. Trình Duyệt / React Client",
    tech: "Next.js 15, React 19, Fetch API",
    badgeColor: "bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/30",
    inputDescription: "Người học bấm nút 'Nộp bài tập'. Client đóng gói JSON payload { lessonId, code } và gắn kèm Authorization: Bearer <JWT>.",
    outputDescription: "HTTP POST Request dispatch qua mạng Internet đến Domain Gateway.",
    codeSnippet: `const response = await fetch('/api/submissions', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': \`Bearer \${token}\`
  },
  body: JSON.stringify({ lessonId, code })
});`,
    failureScenario: "Mất kết nối mạng hoặc Client không có token hợp lệ → Báo lỗi Offline / Chuyển hướng về trang đăng nhập.",
  },
  {
    id: "gateway",
    name: "2. Gateway & Rate Limiter / Auth Guard",
    tech: "ASP.NET Core RateLimiter & JWT Bearer",
    badgeColor: "bg-purple-500/10 text-purple-600 dark:text-purple-400 border-purple-500/30",
    inputDescription: "Kiểm tra IP trong Token Bucket (giới hạn 30 req/phút). Giải mã chữ ký HMAC-SHA256 của JWT để xác minh danh tính người học.",
    outputDescription: "ClaimsPrincipal hợp lệ với UserId và Role được chuyển tiếp vào Pipeline.",
    codeSnippet: `app.UseRateLimiter(); // Chặn DDoS / Brute-force
app.UseAuthentication(); // Giải mã JWT
app.UseAuthorization(); // Kiểm tra quyền truy cập`,
    failureScenario: "Nếu gửi > 30 request/phút → Trả ngay mã HTTP 429 Too Many Requests; Nếu token hết hạn → Trả mã 401 Unauthorized mà không tốn tài nguyên Database.",
  },
  {
    id: "validation",
    name: "3. Tầng Kiểm Tra Dữ Liệu (Validation)",
    tech: "FluentValidation & Model Binding",
    badgeColor: "bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/30",
    inputDescription: "Xác thực payload: LessonId phải tồn tại, đoạn mã nộp không vượt quá 50,000 ký tự và không chứa script độc hại.",
    outputDescription: "DTO sạch được chuyển tiếp vào Application Service.",
    codeSnippet: `RuleFor(x => x.LessonId).NotEmpty();
RuleFor(x => x.Code)
  .NotEmpty()
  .MaximumLength(50000)
  .WithMessage("Đoạn mã không được vượt quá 50,000 ký tự.");`,
    failureScenario: "Nếu vi phạm quy tắc → Dừng xử lý ngay lập tức và trả về mã HTTP 400 Bad Request kèm chi tiết lỗi từng trường.",
  },
  {
    id: "cache-service",
    name: "4. Cache Phân Tán & Dịch Vụ Nghiệp Vụ",
    tech: "Redis Distributed Cache & C# Domain Service",
    badgeColor: "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/30",
    inputDescription: "Tra cứu Cache xem bài học này có quy tắc chấm đặc biệt hay không. Kiểm tra trạng thái nộp bài trước đó trong Redis.",
    outputDescription: "Chuẩn bị giao dịch lưu trữ kết quả và tính toán điểm Streak học tập.",
    codeSnippet: `var cachedRule = await _cache.GetStringAsync($"lesson:{id}:rules");
if (cachedRule == null) {
    // Cache Miss -> Đọc DB và nạp vào Redis trong 1 giờ
}`,
    failureScenario: "Nếu Redis gặp sự cố, hệ thống tự động Fallback đọc trực tiếp từ Database mà không làm gián đoạn người học.",
  },
  {
    id: "database",
    name: "5. Cơ Sở Dữ Liệu Quan Hệ (Database)",
    tech: "PostgreSQL / SQLite via EF Core",
    badgeColor: "bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-500/30",
    inputDescription: "Bắt đầu Database Transaction: Ghi bản ghi nộp bài vào bảng Submissions, cập nhật tiến độ học tập và ghi điểm tích lũy.",
    outputDescription: "Transaction Commit thành công, trả về Id bài nộp và số điểm thưởng.",
    codeSnippet: `await using var tx = await _db.Database.BeginTransactionAsync();
_db.Submissions.Add(submission);
await _db.SaveChangesAsync();
await tx.CommitAsync();`,
    failureScenario: "Nếu vi phạm khóa ngoại hoặc deadlock → Tự động Rollback hoàn toàn và trả về lỗi 500 có kiểm soát.",
  },
];

export function CodingPracticeLab({
  courseId,
  courseTitle,
  lessonId,
  lessonTitle,
  lessonMarkdown,
  onSaveToNotebook,
  onCreateFlashcardQuick,
}: PracticeModuleProps) {
  // Tự động nhận diện chủ đề học tập
  const detectedCategory = useMemo<TopicCategory>(() => {
    const text = `${courseTitle} ${lessonTitle} ${lessonMarkdown}`.toLowerCase();
    if (text.includes("hệ điều hành") || text.includes("tlb") || text.includes("bộ nhớ ảo") || text.includes("phân trang") || text.includes("deadlock")) {
      return "os";
    }
    if (text.includes("sql") || text.includes("database") || text.includes("join") || text.includes("truy vấn") || text.includes("injection")) {
      return "sql";
    }
    if (text.includes("react") || text.includes("useeffect") || text.includes("usestate") || text.includes("hook") || text.includes("closure")) {
      return "react";
    }
    if (text.includes(".net") || text.includes("c#") || text.includes("csharp") || text.includes("ef core") || text.includes("async")) {
      return "dotnet";
    }
    return "dotnet";
  }, [courseTitle, lessonTitle, lessonMarkdown]);

  // Trích xuất code block từ lessonMarkdown nếu có
  const extractedExercises = useMemo<FillCodeExercise[]>(() => {
    if (!lessonMarkdown) return [];
    const codeBlockRegex = /```([a-zA-Z0-9_-]*)\r?\n([\s\S]*?)```/g;
    const list: FillCodeExercise[] = [];
    let match: RegExpExecArray | null;
    let idx = 0;

    while ((match = codeBlockRegex.exec(lessonMarkdown)) !== null) {
      const rawLang = (match[1] || "csharp").toLowerCase();
      const code = match[2].trim();
      const lines = code.split("\n");
      if (lines.length >= 4 && lines.length <= 25) {
        idx++;
        // Tìm dòng có từ khóa thích hợp để tạo chỗ trống
        const candidateLineIndex = lines.findIndex((l) =>
          /\b(await|return|async|const|function|public|SELECT|WHERE|JOIN)\b/.test(l)
        );

        if (candidateLineIndex !== -1) {
          const targetLine = lines[candidateLineIndex];
          const words = targetLine.trim().split(/\s+/);
          const blankWord = words[0] || "return";
          const replacedTemplate = [...lines];
          replacedTemplate[candidateLineIndex] = targetLine.replace(blankWord, "{{BLANK_0}}");

          list.push({
            id: `extracted-${idx}`,
            title: `Trích xuất từ bài học: Đoạn mã #${idx} (${rawLang.toUpperCase()})`,
            category: rawLang.includes("sql") ? "sql" : rawLang.includes("ts") || rawLang.includes("js") ? "react" : "dotnet",
            language: rawLang || "code",
            description: `Đoạn code thực tế được trích xuất trực tiếp từ bài học '${lessonTitle}'.`,
            practicalScenario: "Thực hành điền từ khóa điều hướng cốt lõi trong đoạn code trích xuất từ bài giảng.",
            codeTemplate: replacedTemplate,
            blanks: [
              {
                id: `eb0`,
                blankIndex: 0,
                expected: blankWord,
                acceptableAnswers: [blankWord],
                hint: `Từ khóa dòng ${candidateLineIndex + 1}`,
                chips: [blankWord, "return", "await", "const"],
                explanation: `Từ khóa '${blankWord}' giữ vai trò thực thi cốt lõi tại dòng này.`,
              },
            ],
            lineExplanations: [
              {
                lineNumber: candidateLineIndex + 1,
                codeSnippet: targetLine.trim(),
                purpose: "Dòng xử lý mấu chốt được trích xuất từ bài giảng.",
                riskIfWrong: "Nếu thiếu từ khóa này, chương trình sẽ báo lỗi cú pháp hoặc sai luồng dữ liệu.",
              },
            ],
            isExtractedFromMarkdown: true,
          });
        }
      }
    }
    return list;
  }, [lessonMarkdown, lessonTitle]);

  const [activeMainMode, setActiveMainMode] = useState<MainPracticeMode>("fill-code");
  const [selectedTopic, setSelectedTopic] = useState<TopicCategory>("auto");
  const currentEffectiveCategory = selectedTopic === "auto" ? detectedCategory : selectedTopic;

  // Banner thông báo
  const [feedbackMsg, setFeedbackMsg] = useState<{ text: string; type: "success" | "info" } | null>(null);
  const triggerFeedback = useCallback((text: string, type: "success" | "info" = "success") => {
    setFeedbackMsg({ text, type });
    setTimeout(() => setFeedbackMsg(null), 3500);
  }, []);

  // ----------------------------------------------------
  // CHẾ ĐỘ 1: ĐIỀN CODE CÒN THIẾU
  // ----------------------------------------------------
  const allFillExercises = useMemo(() => {
    return [...extractedExercises, ...DEFAULT_FILL_EXERCISES];
  }, [extractedExercises]);

  const filteredFillExercises = useMemo(() => {
    if (selectedTopic === "auto") {
      const match = allFillExercises.filter((ex) => ex.category === detectedCategory);
      return match.length > 0 ? match : allFillExercises;
    }
    return allFillExercises.filter((ex) => ex.category === selectedTopic);
  }, [selectedTopic, detectedCategory, allFillExercises]);

  const [currentFillExerciseIndex, setCurrentFillExerciseIndex] = useState(0);
  const activeFillExercise = filteredFillExercises[currentFillExerciseIndex] || allFillExercises[0];

  const [userBlankInputs, setUserBlankInputs] = useState<Record<string, string>>({});
  const [fillValidationResults, setFillValidationResults] = useState<Record<string, boolean> | null>(null);
  const [showLineExplanations, setShowLineExplanations] = useState(false);
  const [activeHintBlankId, setActiveHintBlankId] = useState<string | null>(null);

  useEffect(() => {
    setUserBlankInputs({});
    setFillValidationResults(null);
    setShowLineExplanations(false);
    setActiveHintBlankId(null);
  }, [activeFillExercise.id]);

  const handleFillInput = (blankId: string, value: string) => {
    setUserBlankInputs((prev) => ({ ...prev, [blankId]: value }));
  };

  const handleCheckFillResults = () => {
    const results: Record<string, boolean> = {};
    activeFillExercise.blanks.forEach((b) => {
      const val = (userBlankInputs[b.id] || "").trim();
      const isOk =
        val.toLowerCase() === b.expected.toLowerCase() ||
        b.acceptableAnswers.some((ans) => ans.toLowerCase() === val.toLowerCase());
      results[b.id] = isOk;
    });
    setFillValidationResults(results);
    setShowLineExplanations(true);
  };

  const isAllBlanksCorrect = useMemo(() => {
    if (!fillValidationResults) return false;
    return activeFillExercise.blanks.every((b) => fillValidationResults[b.id] === true);
  }, [fillValidationResults, activeFillExercise.blanks]);

  // ----------------------------------------------------
  // CHẾ ĐỘ 2: SĂN LỖI THỰC CHIẾN (BUG HUNTER)
  // ----------------------------------------------------
  const [currentBugScenarioIndex, setCurrentBugScenarioIndex] = useState(0);
  const activeBugScenario = BUG_HUNTER_SCENARIOS[currentBugScenarioIndex] || BUG_HUNTER_SCENARIOS[0];

  const [selectedLineNumber, setSelectedLineNumber] = useState<number | null>(null);
  const [isLineConfirmed, setIsLineConfirmed] = useState(false);
  const [selectedFixOptionId, setSelectedFixOptionId] = useState<string | null>(null);
  const [showCleanCode, setShowCleanCode] = useState(false);

  useEffect(() => {
    setSelectedLineNumber(null);
    setIsLineConfirmed(false);
    setSelectedFixOptionId(null);
    setShowCleanCode(false);
  }, [activeBugScenario.id]);

  const handleConfirmBugLine = () => {
    if (selectedLineNumber !== null) {
      setIsLineConfirmed(true);
    }
  };

  // ----------------------------------------------------
  // CHẾ ĐỘ 3: MÔ PHỎNG LUỒNG HỆ THỐNG
  // ----------------------------------------------------
  const [visualizerSubMode, setVisualizerSubMode] = useState<"webrequest" | "tlb">(() => {
    return detectedCategory === "os" ? "tlb" : "webrequest";
  });

  // 3A: Request Lifecycle
  const [currentStageIndex, setCurrentStageIndex] = useState(0);
  const [isSimulatingAuto, setIsSimulatingAuto] = useState(false);
  const [simulationScenarioType, setSimulationScenarioType] = useState<"success" | "auth-fail" | "validation-fail">("success");

  useEffect(() => {
    let timer: NodeJS.Timeout | null = null;
    if (isSimulatingAuto) {
      timer = setInterval(() => {
        setCurrentStageIndex((prev) => {
          if (prev >= REQUEST_STAGES.length - 1) {
            setIsSimulatingAuto(false);
            return prev;
          }
          return prev + 1;
        });
      }, 1600);
    }
    return () => {
      if (timer) clearInterval(timer);
    };
  }, [isSimulatingAuto]);

  // 3B: Virtual Memory & TLB
  const [tlbHitRate, setTlbHitRate] = useState<number>(90);
  const [tlbAccessTimeNs, setTlbAccessTimeNs] = useState<number>(2);
  const [ramAccessTimeNs, setRamAccessTimeNs] = useState<number>(100);

  const eatCalculation = useMemo(() => {
    const alpha = tlbHitRate / 100;
    const hitTime = tlbAccessTimeNs + ramAccessTimeNs;
    const missTime = tlbAccessTimeNs + 2 * ramAccessTimeNs;
    const eat = alpha * hitTime + (1 - alpha) * missTime;
    const slowdownPercentage = ((eat - hitTime) / hitTime) * 100;
    return {
      alpha,
      hitTime,
      missTime,
      eat: parseFloat(eat.toFixed(2)),
      slowdownPercentage: parseFloat(slowdownPercentage.toFixed(1)),
    };
  }, [tlbHitRate, tlbAccessTimeNs, ramAccessTimeNs]);

  interface MemoryAccessEvent {
    id: string;
    virtualAddress: string;
    pageNumber: number;
    offset: string;
    isHit: boolean;
    durationNs: number;
    timeString: string;
  }
  const [accessHistory, setAccessHistory] = useState<MemoryAccessEvent[]>([]);

  const handleSimulateRandomAccess = () => {
    const randomPage = Math.floor(Math.random() * 64);
    const randomOffset = Math.floor(Math.random() * 4096);
    const isHit = Math.random() * 100 < tlbHitRate;
    const duration = isHit ? eatCalculation.hitTime : eatCalculation.missTime;
    const hexAddr = `0x${((randomPage << 12) | randomOffset).toString(16).toUpperCase().padStart(4, "0")}`;

    const newEvent: MemoryAccessEvent = {
      id: Math.random().toString(),
      virtualAddress: hexAddr,
      pageNumber: randomPage,
      offset: `0x${randomOffset.toString(16).toUpperCase().padStart(3, "0")}`,
      isHit,
      durationNs: duration,
      timeString: new Date().toLocaleTimeString("vi-VN"),
    };
    setAccessHistory((prev) => [newEvent, ...prev.slice(0, 4)]);
  };

  // ----------------------------------------------------
  // NOTEBOOK & FLASHCARD
  // ----------------------------------------------------
  const handleSaveFillCodeToNotebook = async () => {
    const summary = [
      `### 💻 Bài Thực Hành Lập Trình: ${activeFillExercise.title}`,
      `- Chủ đề: ${activeFillExercise.category.toUpperCase()} | Ngôn ngữ: ${activeFillExercise.language}`,
      `- Tình huống thực tế: ${activeFillExercise.practicalScenario}`,
      "```" + activeFillExercise.language,
      activeFillExercise.codeTemplate
        .map((line) => {
          let replaced = line;
          activeFillExercise.blanks.forEach((b) => {
            replaced = replaced.replace(`{{BLANK_${b.blankIndex}}}`, userBlankInputs[b.id] || `[${b.expected}]`);
          });
          return replaced;
        })
        .join("\n"),
      "```",
      "- Giải thích mấu chốt:",
      ...activeFillExercise.blanks.map((b) => `  * ${b.expected}: ${b.explanation}`),
    ].join("\n");

    await onSaveToNotebook(summary);
    triggerFeedback("Đã lưu bài thực hành code vào Sổ tay học tập!");
  };

  const handleCreateBugFlashcard = async () => {
    if (!onCreateFlashcardQuick) return;
    const front = `[Lỗi Thường Gặp] ${activeBugScenario.title}\nNhận diện nguyên nhân & cách xử lý triệt để?`;
    const back = `**Nguyên nhân:** ${activeBugScenario.rootCauseAnalysis}\n\n**Hậu quả:** ${activeBugScenario.realWorldImpact}\n\n**Giải pháp chuẩn:**\n${activeBugScenario.cleanFixedCode}`;
    await onCreateFlashcardQuick(front, back);
    triggerFeedback("Đã tạo thẻ ghi nhớ Flashcard cho lỗi này!");
  };

  const handleSaveVisualizerToNotebook = async () => {
    if (visualizerSubMode === "webrequest") {
      const summary = [
        `### 🌐 Sơ Đồ Kiến Trúc: Chu Trình Request Web Full-Stack`,
        `1. Client (Next.js / React 19) -> Gửi JSON Payload + JWT Authorization Header`,
        `2. Gateway & Rate Limiter -> Chặn DDoS, xác minh HMAC-SHA256 Token`,
        `3. Validation Layer -> FluentValidation kiểm tra cấu trúc dữ liệu`,
        `4. Cache & Service -> Tra cứu Redis Cache, giảm tải cho Database`,
        `5. Database -> Thực thi Transaction ACID, cập nhật dữ liệu tin cậy`,
      ].join("\n");
      await onSaveToNotebook(summary);
      triggerFeedback("Đã lưu kiến trúc Request Lifecycle vào Sổ tay!");
    } else {
      const summary = [
        `### 🧠 Hệ Điều Hành: Bộ Nhớ Ảo & Thời Gian Truy Cập Trung Bình (EAT)`,
        `- Tỷ lệ TLB Hit: ${tlbHitRate}% (Thời gian đọc TLB: ${tlbAccessTimeNs}ns, RAM: ${ramAccessTimeNs}ns)`,
        `- Công thức: EAT = α × (t_TLB + t_RAM) + (1 - α) × (t_TLB + 2 × t_RAM)`,
        `- Kết quả EAT thực tế: ${eatCalculation.eat} ns`,
        `- Độ trễ suy giảm do TLB Miss: +${eatCalculation.slowdownPercentage}% so với trường hợp tối ưu`,
      ].join("\n");
      await onSaveToNotebook(summary);
      triggerFeedback("Đã lưu công thức & số liệu EAT vào Sổ tay!");
    }
  };

  return (
    <div
      data-testid="coding-practice-lab"
      className="w-full max-w-6xl mx-auto rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 shadow-sm overflow-hidden text-zinc-900 dark:text-zinc-100 transition-colors"
    >
      {/* ==================================================== */}
      {/* HEADER                                               */}
      {/* ==================================================== */}
      <div className="border-b border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900/60 p-4 sm:p-5">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-indigo-50 dark:bg-indigo-950/60 text-indigo-600 dark:text-indigo-400 border border-indigo-200 dark:border-indigo-800/60">
                <Code2 className="w-3.5 h-3.5" />
                Lab Thực Hành Lập Trình & Hệ Thống
              </span>
              <span className="text-xs text-zinc-600 dark:text-zinc-400">
                Mục tiêu: {courseTitle || "Chuyên ngành Công nghệ thông tin"}
              </span>
            </div>
            <h2 className="text-lg sm:text-xl font-bold tracking-tight mt-1 text-zinc-900 dark:text-zinc-100">
              {lessonTitle || "Thực hành Kỹ thuật Lập trình Chuyên sâu"}
            </h2>
          </div>

          {/* Quick Actions (Sổ tay & Flashcard) */}
          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              data-testid="lab-save-notebook-btn"
              onClick={
                activeMainMode === "fill-code"
                  ? handleSaveFillCodeToNotebook
                  : activeMainMode === "system-visualizer"
                  ? handleSaveVisualizerToNotebook
                  : handleSaveFillCodeToNotebook
              }
              className="min-h-[44px] px-3.5 py-2 rounded-xl text-xs font-medium border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 hover:bg-zinc-100 dark:hover:bg-zinc-800 text-zinc-700 dark:text-zinc-300 inline-flex items-center gap-2 transition cursor-pointer shadow-xs"
            >
              <BookmarkPlus className="w-4 h-4 text-indigo-500" />
              <span>Lưu Sổ Tay</span>
            </button>
            {onCreateFlashcardQuick && (
              <button
                type="button"
                data-testid="lab-create-flashcard-btn"
                onClick={handleCreateBugFlashcard}
                className="min-h-[44px] px-3.5 py-2 rounded-xl text-xs font-medium border border-indigo-300 dark:border-indigo-800 bg-indigo-50/50 dark:bg-indigo-950/40 hover:bg-indigo-100 dark:hover:bg-indigo-900/50 text-indigo-700 dark:text-indigo-300 inline-flex items-center gap-2 transition cursor-pointer shadow-xs"
              >
                <Sparkles className="w-4 h-4 text-indigo-500" />
                <span>Tạo Flashcard Lỗi</span>
              </button>
            )}
          </div>
        </div>

        {/* Feedback message banner */}
        {feedbackMsg && (
          <div
            data-testid="lab-feedback-banner"
            className="mt-3 px-3.5 py-2 rounded-xl text-xs font-medium bg-emerald-50 dark:bg-emerald-950/60 text-emerald-800 dark:text-emerald-200 border border-emerald-300 dark:border-emerald-800 flex items-center gap-2 transition"
          >
            <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
            <span>{feedbackMsg.text}</span>
          </div>
        )}

        {/* 3 TABS CHẾ ĐỘ THỰC HÀNH LỚN */}
        <div className="mt-4 pt-3 border-t border-zinc-200 dark:border-zinc-800 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div
            role="tablist"
            className="inline-flex p-1 rounded-xl bg-zinc-200/70 dark:bg-zinc-800/80 border border-zinc-300 dark:border-zinc-700/60 overflow-x-auto max-w-full"
          >
            <button
              type="button"
              role="tab"
              aria-selected={activeMainMode === "fill-code"}
              data-testid="mode-tab-fill-code"
              onClick={() => setActiveMainMode("fill-code")}
              className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold inline-flex items-center gap-2 transition cursor-pointer shrink-0 ${
                activeMainMode === "fill-code"
                  ? "bg-white dark:bg-zinc-900 text-indigo-600 dark:text-indigo-400 shadow-sm"
                  : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
              }`}
            >
              <Terminal className="w-4 h-4" />
              <span>1. Điền Code & Giải Thích</span>
            </button>

            <button
              type="button"
              role="tab"
              aria-selected={activeMainMode === "bug-hunter"}
              data-testid="mode-tab-bug-hunter"
              onClick={() => setActiveMainMode("bug-hunter")}
              className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold inline-flex items-center gap-2 transition cursor-pointer shrink-0 ${
                activeMainMode === "bug-hunter"
                  ? "bg-white dark:bg-zinc-900 text-amber-600 dark:text-amber-400 shadow-sm"
                  : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
              }`}
            >
              <Bug className="w-4 h-4" />
              <span>2. Săn Lỗi Thực Chiến (6+ Bug)</span>
            </button>

            <button
              type="button"
              role="tab"
              aria-selected={activeMainMode === "system-visualizer"}
              data-testid="mode-tab-system-visualizer"
              onClick={() => setActiveMainMode("system-visualizer")}
              className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold inline-flex items-center gap-2 transition cursor-pointer shrink-0 ${
                activeMainMode === "system-visualizer"
                  ? "bg-white dark:bg-zinc-900 text-teal-600 dark:text-teal-400 shadow-sm"
                  : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
              }`}
            >
              <Network className="w-4 h-4" />
              <span>3. Mô Phỏng Hệ Thống</span>
            </button>
          </div>

          {/* Chọn chủ đề / Thích ứng bài học */}
          <div className="flex items-center gap-1.5 text-xs">
            <span className="text-zinc-600 dark:text-zinc-400 hidden sm:inline">Chủ đề:</span>
            <select
              data-testid="topic-category-select"
              value={selectedTopic}
              onChange={(e) => setSelectedTopic(e.target.value as TopicCategory)}
              className="min-h-[44px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 text-xs font-medium text-zinc-800 dark:text-zinc-200 focus:outline-none focus:ring-2 focus:ring-indigo-500 cursor-pointer"
            >
              <option value="auto">Tự động nhận diện ({detectedCategory.toUpperCase()})</option>
              <option value="dotnet">.NET 10 / C# & EF Core</option>
              <option value="react">React 19 / TypeScript</option>
              <option value="sql">SQL & Security Injection</option>
              <option value="os">Hệ Điều Hành & Bộ Nhớ</option>
            </select>
          </div>
        </div>
      </div>

      {/* ==================================================== */}
      {/* CHẾ ĐỘ 1: ĐIỀN CODE CÒN THIẾU                        */}
      {/* ==================================================== */}
      {activeMainMode === "fill-code" && (
        <div data-testid="fill-code-container" className="p-4 sm:p-6 space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-zinc-200 dark:border-zinc-800">
            <div>
              <div className="flex items-center gap-2">
                <span className="px-2 py-0.5 rounded text-[11px] font-semibold uppercase tracking-wider bg-zinc-200 dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 font-mono">
                  {activeFillExercise.language}
                </span>
                {activeFillExercise.isExtractedFromMarkdown && (
                  <span className="px-2 py-0.5 rounded text-[11px] font-semibold bg-indigo-100 dark:bg-indigo-950 text-indigo-700 dark:text-indigo-300 border border-indigo-300 dark:border-indigo-800">
                    Trích xuất từ bài giảng
                  </span>
                )}
                <span className="text-xs font-medium text-indigo-600 dark:text-indigo-400">
                  {activeFillExercise.category === "dotnet"
                    ? "ASP.NET Core / EF Core"
                    : activeFillExercise.category === "react"
                    ? "React Lifecycle"
                    : activeFillExercise.category === "sql"
                    ? "Database Security"
                    : "Operating Systems"}
                </span>
              </div>
              <h3 className="text-base sm:text-lg font-bold text-zinc-900 dark:text-zinc-100 mt-1">
                {activeFillExercise.title}
              </h3>
            </div>

            <div className="flex items-center gap-2">
              <span className="text-xs text-zinc-600 dark:text-zinc-400">
                Bài {currentFillExerciseIndex + 1}/{filteredFillExercises.length}
              </span>
              <div className="inline-flex rounded-lg border border-zinc-300 dark:border-zinc-700 overflow-hidden">
                <button
                  type="button"
                  data-testid="prev-fill-exercise-btn"
                  disabled={currentFillExerciseIndex === 0}
                  onClick={() => setCurrentFillExerciseIndex((prev) => prev - 1)}
                  className="min-h-[44px] px-3 bg-white dark:bg-zinc-900 hover:bg-zinc-100 dark:hover:bg-zinc-800 disabled:opacity-40 text-xs font-medium cursor-pointer"
                >
                  Trước
                </button>
                <button
                  type="button"
                  data-testid="next-fill-exercise-btn"
                  disabled={currentFillExerciseIndex >= filteredFillExercises.length - 1}
                  onClick={() => setCurrentFillExerciseIndex((prev) => prev + 1)}
                  className="min-h-[44px] px-3 bg-white dark:bg-zinc-900 hover:bg-zinc-100 dark:hover:bg-zinc-800 disabled:opacity-40 text-xs font-medium border-l border-zinc-300 dark:border-zinc-700 cursor-pointer"
                >
                  Sau
                </button>
              </div>
            </div>
          </div>

          <div className="p-3.5 rounded-xl bg-amber-50/70 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-900/50 text-xs text-amber-900 dark:text-amber-200 flex items-start gap-2.5">
            <AlertTriangle className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0 mt-0.5" />
            <div>
              <span className="font-semibold">Tình huống thực tế: </span>
              {activeFillExercise.practicalScenario}
            </div>
          </div>

          {/* Editor Container */}
          <div className="rounded-xl border border-zinc-800 bg-zinc-950 text-zinc-100 font-mono text-xs sm:text-sm shadow-md overflow-hidden">
            <div className="px-4 py-2 border-b border-zinc-800 bg-zinc-900/90 flex items-center justify-between text-xs text-zinc-400">
              <div className="flex items-center gap-2">
                <div className="flex gap-1.5">
                  <div className="w-2.5 h-2.5 rounded-full bg-rose-500/80" />
                  <div className="w-2.5 h-2.5 rounded-full bg-amber-500/80" />
                  <div className="w-2.5 h-2.5 rounded-full bg-emerald-500/80" />
                </div>
                <span className="font-sans text-[11px] text-zinc-400 ml-1">Editor: {activeFillExercise.title}</span>
              </div>
              <span className="text-[11px] text-zinc-400">Tự gõ hoặc chọn mảnh code bên dưới</span>
            </div>

            <div className="p-4 overflow-x-auto">
              <div className="space-y-1.5 min-w-[540px]">
                {activeFillExercise.codeTemplate.map((line, lineIdx) => {
                  const lineNumber = lineIdx + 1;
                  const hasBlank = line.includes("{{BLANK_");

                  if (!hasBlank) {
                    return (
                      <div key={lineIdx} className="flex items-baseline gap-4 leading-relaxed">
                        <span className="w-6 text-right text-zinc-600 select-none text-xs">{lineNumber}</span>
                        <span className="text-zinc-300">{line}</span>
                      </div>
                    );
                  }

                  return (
                    <div key={lineIdx} className="flex items-baseline gap-4 leading-relaxed py-1 bg-zinc-900/40 rounded px-1">
                      <span className="w-6 text-right text-zinc-500 font-semibold select-none text-xs">{lineNumber}</span>
                      <div className="flex flex-wrap items-center gap-1.5 text-zinc-300">
                        {activeFillExercise.blanks.map((blank) => {
                          const placeholder = `{{BLANK_${blank.blankIndex}}}`;
                          if (!line.includes(placeholder)) return null;

                          const parts = line.split(placeholder);
                          const userVal = userBlankInputs[blank.id] || "";
                          const isEvaluated = fillValidationResults !== null;
                          const isCorrect = fillValidationResults?.[blank.id];

                          return (
                            <React.Fragment key={blank.id}>
                              <span>{parts[0]}</span>
                              <div className="inline-flex flex-col gap-1">
                                <div className="inline-flex items-center gap-1">
                                  <input
                                    type="text"
                                    data-testid={`fill-blank-input-${blank.id}`}
                                    value={userVal}
                                    placeholder={blank.hint}
                                    onChange={(e) => handleFillInput(blank.id, e.target.value)}
                                    className={`min-h-[38px] px-2.5 py-1 rounded border text-xs sm:text-sm font-mono font-semibold focus:outline-none transition ${
                                      isEvaluated
                                        ? isCorrect
                                          ? "border-emerald-500 bg-emerald-950/80 text-emerald-200"
                                          : "border-rose-500 bg-rose-950/80 text-rose-200"
                                        : "border-amber-500/70 bg-zinc-900 text-amber-300 focus:border-amber-400"
                                    }`}
                                  />
                                  {isEvaluated && (
                                    <span>
                                      {isCorrect ? (
                                        <CheckCircle2 className="w-4 h-4 text-emerald-400 inline" />
                                      ) : (
                                        <XCircle className="w-4 h-4 text-rose-400 inline" />
                                      )}
                                    </span>
                                  )}
                                </div>
                              </div>
                              <span>{parts[1]}</span>
                            </React.Fragment>
                          );
                        })}
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>

          {/* Mảnh code gợi ý */}
          <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900/60 space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold text-zinc-700 dark:text-zinc-300 flex items-center gap-1.5">
                <Sparkles className="w-3.5 h-3.5 text-indigo-500" />
                Mảnh code gợi ý (Bấm chọn nhanh cho điện thoại):
              </span>
              <span className="text-[11px] text-zinc-600 dark:text-zinc-400">Chạm để điền vào chỗ trống</span>
            </div>

            <div className="space-y-2">
              {activeFillExercise.blanks.map((b) => (
                <div key={b.id} className="flex flex-wrap items-center gap-2">
                  <span className="text-xs text-zinc-600 dark:text-zinc-400 min-w-[70px]">
                    Vị trí #{b.blankIndex + 1}:
                  </span>
                  {b.chips.map((chip) => (
                    <button
                      key={chip}
                      type="button"
                      data-testid={`code-chip-${b.id}-${chip}`}
                      onClick={() => handleFillInput(b.id, chip)}
                      className={`min-h-[44px] px-3 py-1.5 rounded-lg text-xs font-mono font-semibold border transition cursor-pointer ${
                        userBlankInputs[b.id] === chip
                          ? "border-indigo-500 bg-indigo-50 dark:bg-indigo-950 text-indigo-600 dark:text-indigo-300"
                          : "border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-800 text-zinc-700 dark:text-zinc-200 hover:border-zinc-400"
                      }`}
                    >
                      {chip || '"" (rỗng)'}
                    </button>
                  ))}
                  <button
                    type="button"
                    onClick={() => setActiveHintBlankId(activeHintBlankId === b.id ? null : b.id)}
                    className="text-[11px] text-indigo-600 dark:text-indigo-400 underline ml-1 cursor-pointer"
                  >
                    {activeHintBlankId === b.id ? "Đóng gợi ý" : "Xem gợi ý"}
                  </button>
                  {activeHintBlankId === b.id && (
                    <span className="text-xs text-amber-700 dark:text-amber-300 italic bg-amber-100/50 dark:bg-amber-950/40 px-2 py-1 rounded">
                      💡 {b.hint}
                    </span>
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* Action buttons */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pt-2">
            <div className="flex items-center gap-2">
              <button
                type="button"
                data-testid="check-fill-result-btn"
                onClick={handleCheckFillResults}
                className="min-h-[44px] px-6 py-2.5 rounded-xl font-semibold text-xs sm:text-sm bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm inline-flex items-center justify-center gap-2 cursor-pointer transition"
              >
                <Check className="w-4 h-4" />
                <span>Kiểm Tra Kết Quả</span>
              </button>

              <button
                type="button"
                data-testid="toggle-explanations-btn"
                onClick={() => setShowLineExplanations((prev) => !prev)}
                className="min-h-[44px] px-4 py-2.5 rounded-xl text-xs font-medium border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 hover:bg-zinc-100 dark:hover:bg-zinc-800 text-zinc-700 dark:text-zinc-300 inline-flex items-center gap-1.5 cursor-pointer transition"
              >
                {showLineExplanations ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                <span>{showLineExplanations ? "Ẩn Giải Thích" : "Hiện Giải Thích Từng Dòng"}</span>
              </button>
            </div>

            {fillValidationResults && (
              <div
                data-testid="fill-score-badge"
                className={`px-3.5 py-2 rounded-xl text-xs font-semibold flex items-center gap-2 ${
                  isAllBlanksCorrect
                    ? "bg-emerald-100 dark:bg-emerald-950/60 text-emerald-800 dark:text-emerald-300 border border-emerald-300 dark:border-emerald-800"
                    : "bg-amber-100 dark:bg-amber-950/60 text-amber-800 dark:text-amber-300 border border-amber-300 dark:border-amber-800"
                }`}
              >
                {isAllBlanksCorrect ? (
                  <>
                    <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
                    <span>Chính xác hoàn toàn! Đoạn code đã sẵn sàng chạy production.</span>
                  </>
                ) : (
                  <>
                    <AlertTriangle className="w-4 h-4 text-amber-600 dark:text-amber-400" />
                    <span>Có chỗ trống chưa chính xác. Hãy xem giải thích bên dưới để sửa lại.</span>
                  </>
                )}
              </div>
            )}
          </div>

          {/* Line by line explanations */}
          {showLineExplanations && (
            <div
              data-testid="line-explanations-section"
              className="mt-6 p-4 sm:p-5 rounded-2xl border border-indigo-200 dark:border-indigo-900/50 bg-indigo-50/30 dark:bg-indigo-950/20 space-y-4"
            >
              <div className="flex items-center gap-2">
                <BookOpen className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
                <h4 className="text-sm font-bold text-zinc-900 dark:text-zinc-100">
                  Phân Tích Chuyên Sâu Từng Dòng Code Trọng Yếu:
                </h4>
              </div>

              <div className="grid grid-cols-1 gap-3">
                {activeFillExercise.lineExplanations.map((lineExp) => (
                  <div
                    key={lineExp.lineNumber}
                    className="p-3.5 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shadow-xs space-y-2"
                  >
                    <div className="flex items-center justify-between">
                      <span className="text-xs font-mono font-bold text-indigo-600 dark:text-indigo-400">
                        Dòng {lineExp.lineNumber}: <code className="bg-zinc-100 dark:bg-zinc-800 px-1.5 py-0.5 rounded">{lineExp.codeSnippet}</code>
                      </span>
                      <span className="text-[11px] px-2 py-0.5 rounded-full bg-zinc-100 dark:bg-zinc-800 text-zinc-500">
                        Giải nghĩa
                      </span>
                    </div>

                    <p className="text-xs text-zinc-700 dark:text-zinc-300">
                      <span className="font-semibold text-zinc-900 dark:text-zinc-100">Mục đích kỹ thuật: </span>
                      {lineExp.purpose}
                    </p>

                    <div className="text-xs text-rose-700 dark:text-rose-300 bg-rose-50 dark:bg-rose-950/40 p-2.5 rounded-lg border border-rose-200 dark:border-rose-900/40">
                      <span className="font-semibold">Hậu quả nếu viết sai/thiếu: </span>
                      {lineExp.riskIfWrong}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* ==================================================== */}
      {/* CHẾ ĐỘ 2: SĂN LỖI THỰC CHIẾN (BUG HUNTER)           */}
      {/* ==================================================== */}
      {activeMainMode === "bug-hunter" && (
        <div data-testid="bug-hunter-container" className="p-4 sm:p-6 space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-zinc-200 dark:border-zinc-800">
            <div>
              <span className="text-xs font-semibold px-2 py-0.5 rounded bg-amber-100 dark:bg-amber-950 text-amber-700 dark:text-amber-300 font-mono">
                {activeBugScenario.categoryTag}
              </span>
              <h3 className="text-base sm:text-lg font-bold text-zinc-900 dark:text-zinc-100 mt-1">
                {activeBugScenario.title}
              </h3>
            </div>

            <div className="flex items-center gap-2">
              <span className="text-xs text-zinc-600 dark:text-zinc-400">
                Lỗi {currentBugScenarioIndex + 1}/{BUG_HUNTER_SCENARIOS.length}
              </span>
              <select
                data-testid="bug-scenario-select"
                value={currentBugScenarioIndex}
                onChange={(e) => setCurrentBugScenarioIndex(Number(e.target.value))}
                className="min-h-[44px] px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 text-xs font-medium text-zinc-800 dark:text-zinc-200 focus:outline-none focus:ring-2 focus:ring-amber-500 cursor-pointer"
              >
                {BUG_HUNTER_SCENARIOS.map((sc, idx) => (
                  <option key={sc.id} value={idx}>
                    #{idx + 1}: {sc.title.split(":")[0]}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="p-4 rounded-xl bg-zinc-100 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 text-xs sm:text-sm text-zinc-800 dark:text-zinc-200 space-y-1">
            <span className="font-bold text-amber-600 dark:text-amber-400 flex items-center gap-1.5">
              <Bug className="w-4 h-4" /> Bối cảnh thực tế:
            </span>
            <p className="text-zinc-600 dark:text-zinc-400">{activeBugScenario.context}</p>
          </div>

          <div className="flex items-center gap-2 text-xs text-zinc-600 dark:text-zinc-400">
            <HelpCircle className="w-3.5 h-3.5 text-amber-500" />
            <span>
              <strong>Nhiệm vụ:</strong> Đọc đoạn code dưới đây và <strong>chạm/bấm trực tiếp vào dòng</strong> bạn nghi ngờ gây lỗi.
            </span>
          </div>

          {/* Interactive Code Lines */}
          <div className="rounded-xl border border-zinc-800 bg-zinc-950 text-zinc-100 font-mono text-xs sm:text-sm shadow-md overflow-hidden">
            <div className="px-4 py-2 border-b border-zinc-800 bg-zinc-900/90 flex items-center justify-between text-xs text-zinc-400">
              <span className="font-sans text-[11px]">Trình săn lỗi thực chiến — Chạm vào dòng nghi vấn</span>
              <span className="text-[11px] text-amber-400 font-medium">
                {selectedLineNumber !== null ? `Đã chọn: Dòng ${selectedLineNumber}` : "Chưa chọn dòng nào"}
              </span>
            </div>

            <div className="p-3 sm:p-4 overflow-x-auto">
              <div className="space-y-1 min-w-[500px]">
                {activeBugScenario.codeLines.map((line, idx) => {
                  const lineNum = idx + 1;
                  const isSelected = selectedLineNumber === lineNum;
                  const isBugLine = activeBugScenario.bugLineNumber === lineNum;

                  return (
                    <button
                      key={lineNum}
                      type="button"
                      data-testid={`bug-line-${lineNum}`}
                      onClick={() => {
                        setSelectedLineNumber(lineNum);
                        setIsLineConfirmed(false);
                      }}
                      className={`w-full text-left flex items-baseline gap-3 py-1 px-2 rounded transition cursor-pointer ${
                        isSelected
                          ? isLineConfirmed
                            ? isBugLine
                              ? "bg-emerald-950/80 border border-emerald-500 text-emerald-200"
                              : "bg-rose-950/80 border border-rose-500 text-rose-200"
                            : "bg-amber-950/60 border border-amber-500/80 text-amber-200"
                          : "hover:bg-zinc-900 text-zinc-300"
                      }`}
                    >
                      <span className="w-6 text-right text-zinc-600 select-none text-xs">{lineNum}</span>
                      <span className="grow">{line}</span>
                      {isSelected && (
                        <span className="text-xs font-sans font-semibold shrink-0">
                          {isLineConfirmed ? (isBugLine ? "✅ ĐÚNG DÒNG NÀY" : "❌ KHÔNG PHẢI") : "👉 Nghi vấn"}
                        </span>
                      )}
                    </button>
                  );
                })}
              </div>
            </div>
          </div>

          {selectedLineNumber !== null && !isLineConfirmed && (
            <div className="flex items-center gap-3">
              <button
                type="button"
                data-testid="confirm-bug-line-btn"
                onClick={handleConfirmBugLine}
                className="min-h-[44px] px-5 py-2 rounded-xl text-xs sm:text-sm font-semibold bg-amber-600 hover:bg-amber-700 text-white shadow-sm inline-flex items-center gap-2 cursor-pointer transition"
              >
                <Check className="w-4 h-4" />
                <span>Xác Nhận Dòng {selectedLineNumber} Là Nơi Gây Lỗi</span>
              </button>
            </div>
          )}

          {isLineConfirmed && (
            <div className="space-y-5 animate-in fade-in duration-300">
              <div
                data-testid="bug-line-feedback"
                className={`p-4 rounded-xl border text-xs sm:text-sm flex items-start gap-3 ${
                  selectedLineNumber === activeBugScenario.bugLineNumber
                    ? "bg-emerald-50 dark:bg-emerald-950/60 border-emerald-300 dark:border-emerald-800 text-emerald-900 dark:text-emerald-200"
                    : "bg-rose-50 dark:bg-rose-950/60 border-rose-300 dark:border-rose-800 text-rose-900 dark:text-rose-200"
                }`}
              >
                {selectedLineNumber === activeBugScenario.bugLineNumber ? (
                  <CheckCircle2 className="w-5 h-5 text-emerald-600 dark:text-emerald-400 shrink-0 mt-0.5" />
                ) : (
                  <XCircle className="w-5 h-5 text-rose-600 dark:text-rose-400 shrink-0 mt-0.5" />
                )}
                <div>
                  <h4 className="font-bold">
                    {selectedLineNumber === activeBugScenario.bugLineNumber
                      ? `Chính xác! Dòng ${activeBugScenario.bugLineNumber} là nơi phát sinh sự cố.`
                      : `Chưa chính xác! Dòng bạn chọn (${selectedLineNumber}) không phải nguyên nhân gốc rễ.`}
                  </h4>
                  <p className="mt-1 text-xs">
                    {selectedLineNumber === activeBugScenario.bugLineNumber
                      ? activeBugScenario.bugBriefDescription
                      : `Dòng gây lỗi thực sự là Dòng ${activeBugScenario.bugLineNumber}: ${activeBugScenario.bugBriefDescription}.`}
                  </p>
                </div>
              </div>

              {/* Lựa chọn phương án sửa */}
              <div className="p-4 sm:p-5 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900/60 space-y-3">
                <h4 className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                  <ShieldCheck className="w-4 h-4 text-indigo-500" />
                  Bạn sẽ sửa lỗi này như thế nào để đảm bảo chuẩn Production?
                </h4>

                <div className="space-y-2.5">
                  {activeBugScenario.fixOptions.map((opt) => {
                    const isSelected = selectedFixOptionId === opt.id;
                    return (
                      <button
                        key={opt.id}
                        type="button"
                        data-testid={`fix-option-${opt.id}`}
                        onClick={() => setSelectedFixOptionId(opt.id)}
                        className={`w-full text-left p-3.5 rounded-xl text-xs sm:text-sm font-medium border transition cursor-pointer ${
                          isSelected
                            ? opt.isCorrect
                              ? "bg-emerald-50 dark:bg-emerald-950/80 border-emerald-500 text-emerald-900 dark:text-emerald-200"
                              : "bg-rose-50 dark:bg-rose-950/80 border-rose-500 text-rose-900 dark:text-rose-200"
                            : "bg-white dark:bg-zinc-800 border-zinc-200 dark:border-zinc-700 text-zinc-700 dark:text-zinc-300 hover:border-zinc-400"
                        }`}
                      >
                        <div className="flex items-start gap-2.5">
                          <span
                            className={`w-5 h-5 rounded-full flex items-center justify-center text-xs shrink-0 mt-0.5 ${
                              isSelected
                                ? opt.isCorrect
                                  ? "bg-emerald-500 text-white"
                                  : "bg-rose-500 text-white"
                                : "bg-zinc-200 dark:bg-zinc-700 text-zinc-600 dark:text-zinc-300"
                            }`}
                          >
                            {opt.id.toUpperCase()}
                          </span>
                          <div className="grow">
                            <p>{opt.label}</p>
                            {isSelected && (
                              <p
                                className={`text-xs mt-2 pt-2 border-t ${
                                  opt.isCorrect
                                    ? "border-emerald-200 dark:border-emerald-800 text-emerald-800 dark:text-emerald-300"
                                    : "border-rose-200 dark:border-rose-800 text-rose-800 dark:text-rose-300"
                                }`}
                              >
                                {opt.explanation}
                              </p>
                            )}
                          </div>
                        </div>
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* Phân tích nguyên nhân & Hậu quả */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 space-y-1.5">
                  <span className="text-xs font-bold text-indigo-600 dark:text-indigo-400 flex items-center gap-1.5">
                    <Info className="w-3.5 h-3.5" /> Nguyên nhân gốc rễ (Root Cause):
                  </span>
                  <p className="text-xs text-zinc-700 dark:text-zinc-300 leading-relaxed">
                    {activeBugScenario.rootCauseAnalysis}
                  </p>
                </div>

                <div className="p-4 rounded-xl border border-rose-200 dark:border-rose-900/50 bg-rose-50/40 dark:bg-rose-950/20 space-y-1.5">
                  <span className="text-xs font-bold text-rose-600 dark:text-rose-400 flex items-center gap-1.5">
                    <AlertTriangle className="w-3.5 h-3.5" /> Hậu quả thực tế trên Production:
                  </span>
                  <p className="text-xs text-rose-800 dark:text-rose-300 leading-relaxed">
                    {activeBugScenario.realWorldImpact}
                  </p>
                </div>
              </div>

              {/* Xem code sạch */}
              <div className="space-y-3">
                <button
                  type="button"
                  data-testid="toggle-clean-code-btn"
                  onClick={() => setShowCleanCode((prev) => !prev)}
                  className="min-h-[44px] px-4 py-2 rounded-xl text-xs font-semibold bg-emerald-600 hover:bg-emerald-700 text-white shadow-sm inline-flex items-center gap-2 cursor-pointer transition"
                >
                  <Code2 className="w-4 h-4" />
                  <span>{showCleanCode ? "Ẩn Đoạn Code Sau Khi Sửa" : "Xem Đoạn Code Sạch Sau Khi Sửa Chuẩn"}</span>
                </button>

                {showCleanCode && (
                  <div
                    data-testid="clean-code-block"
                    className="rounded-xl border border-emerald-800 bg-zinc-950 text-emerald-300 font-mono text-xs sm:text-sm p-4 overflow-x-auto shadow-inner"
                  >
                    <div className="text-[11px] text-zinc-500 mb-2 font-sans flex items-center justify-between border-b border-zinc-800 pb-1">
                      <span>Phiên bản đã xử lý triệt để bug:</span>
                      <span className="text-emerald-400">Production Ready</span>
                    </div>
                    <pre className="whitespace-pre">{activeBugScenario.cleanFixedCode}</pre>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      )}

      {/* ==================================================== */}
      {/* CHẾ ĐỘ 3: MÔ PHỎNG LUỒNG HỆ THỐNG                    */}
      {/* ==================================================== */}
      {activeMainMode === "system-visualizer" && (
        <div data-testid="system-visualizer-container" className="p-4 sm:p-6 space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-zinc-200 dark:border-zinc-800">
            <div>
              <span className="text-xs font-semibold text-teal-600 dark:text-teal-400">
                Interactive Architecture Visualizer
              </span>
              <h3 className="text-base sm:text-lg font-bold text-zinc-900 dark:text-zinc-100">
                Mô Phỏng Trực Quan Cơ Chế Vận Hành Hệ Thống
              </h3>
            </div>

            <div className="inline-flex p-1 rounded-xl bg-zinc-100 dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700">
              <button
                type="button"
                data-testid="submode-webrequest-btn"
                onClick={() => setVisualizerSubMode("webrequest")}
                className={`min-h-[44px] px-3.5 py-1.5 rounded-lg text-xs font-semibold inline-flex items-center gap-1.5 transition cursor-pointer ${
                  visualizerSubMode === "webrequest"
                    ? "bg-white dark:bg-zinc-900 text-teal-600 dark:text-teal-400 shadow-sm"
                    : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
                }`}
              >
                <Globe className="w-3.5 h-3.5" />
                <span>Đường Đi Request Web</span>
              </button>

              <button
                type="button"
                data-testid="submode-tlb-btn"
                onClick={() => setVisualizerSubMode("tlb")}
                className={`min-h-[44px] px-3.5 py-1.5 rounded-lg text-xs font-semibold inline-flex items-center gap-1.5 transition cursor-pointer ${
                  visualizerSubMode === "tlb"
                    ? "bg-white dark:bg-zinc-900 text-teal-600 dark:text-teal-400 shadow-sm"
                    : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
                }`}
              >
                <Cpu className="w-3.5 h-3.5" />
                <span>Bộ Nhớ Ảo & TLB</span>
              </button>
            </div>
          </div>

          {/* VISUALIZER 3A: ĐƯỜNG ĐI REQUEST WEB FULL-STACK */}
          {visualizerSubMode === "webrequest" && (
            <div data-testid="visualizer-webrequest-box" className="space-y-6">
              <div className="overflow-x-auto pb-2">
                <div className="flex items-center justify-between min-w-[640px] relative">
                  <div className="absolute top-1/2 left-0 right-0 h-1 bg-zinc-200 dark:bg-zinc-800 -translate-y-1/2 z-0" />

                  {REQUEST_STAGES.map((stg, idx) => {
                    const isActive = currentStageIndex === idx;
                    const isPassed = currentStageIndex > idx;

                    return (
                      <button
                        key={stg.id}
                        type="button"
                        data-testid={`stage-node-${stg.id}`}
                        onClick={() => setCurrentStageIndex(idx)}
                        className="relative z-10 flex flex-col items-center gap-1.5 group cursor-pointer focus:outline-none"
                      >
                        <div
                          className={`w-9 h-9 rounded-full flex items-center justify-center font-bold text-xs transition ${
                            isActive
                              ? "bg-teal-600 text-white ring-4 ring-teal-500/20 shadow-md scale-110"
                              : isPassed
                              ? "bg-emerald-500 text-white"
                              : "bg-zinc-200 dark:bg-zinc-800 text-zinc-600 dark:text-zinc-400 group-hover:bg-zinc-300"
                          }`}
                        >
                          {isPassed ? <Check className="w-4 h-4" /> : idx + 1}
                        </div>
                        <span
                          className={`text-[11px] font-medium text-center max-w-[100px] leading-tight ${
                            isActive
                              ? "text-teal-600 dark:text-teal-400 font-bold"
                              : "text-zinc-600 dark:text-zinc-400"
                          }`}
                        >
                          {stg.name.split(". ")[1] || stg.name}
                        </span>
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* Controls */}
              <div className="flex flex-wrap items-center justify-between gap-3 p-3.5 rounded-xl bg-zinc-50 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800">
                <div className="flex items-center gap-2">
                  <button
                    type="button"
                    data-testid="request-sim-prev-btn"
                    disabled={currentStageIndex === 0}
                    onClick={() => setCurrentStageIndex((prev) => Math.max(0, prev - 1))}
                    className="min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-800 disabled:opacity-40 cursor-pointer"
                  >
                    Bước trước
                  </button>

                  <button
                    type="button"
                    data-testid="request-sim-next-btn"
                    disabled={currentStageIndex === REQUEST_STAGES.length - 1}
                    onClick={() => setCurrentStageIndex((prev) => Math.min(REQUEST_STAGES.length - 1, prev + 1))}
                    className="min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold bg-teal-600 hover:bg-teal-700 text-white disabled:opacity-40 cursor-pointer shadow-xs"
                  >
                    Bước kế tiếp
                  </button>

                  <button
                    type="button"
                    data-testid="request-sim-autoplay-btn"
                    onClick={() => setIsSimulatingAuto((prev) => !prev)}
                    className="min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold border border-teal-300 dark:border-teal-700 text-teal-700 dark:text-teal-300 hover:bg-teal-50 dark:hover:bg-teal-950/40 inline-flex items-center gap-1.5 cursor-pointer"
                  >
                    {isSimulatingAuto ? <Pause className="w-3.5 h-3.5" /> : <Play className="w-3.5 h-3.5" />}
                    <span>{isSimulatingAuto ? "Tạm dừng" : "Chạy tự động"}</span>
                  </button>

                  <button
                    type="button"
                    data-testid="request-sim-reset-btn"
                    onClick={() => {
                      setCurrentStageIndex(0);
                      setIsSimulatingAuto(false);
                    }}
                    className="min-h-[44px] px-3 py-2 rounded-lg text-xs font-medium text-zinc-500 hover:text-zinc-800 dark:hover:text-zinc-200 inline-flex items-center gap-1 cursor-pointer"
                  >
                    <RotateCcw className="w-3.5 h-3.5" />
                    <span>Đặt lại</span>
                  </button>
                </div>

                <div className="flex items-center gap-1.5 text-xs">
                  <span className="text-zinc-600 dark:text-zinc-400">Thử kịch bản:</span>
                  <select
                    value={simulationScenarioType}
                    onChange={(e) => setSimulationScenarioType(e.target.value as any)}
                    className="min-h-[44px] px-2.5 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-800 text-xs font-medium cursor-pointer"
                  >
                    <option value="success">Thành công trọn vẹn (200 OK)</option>
                    <option value="auth-fail">Lỗi tại Tầng 2: Hết hạn Token (401)</option>
                    <option value="validation-fail">Lỗi tại Tầng 3: Payload không hợp lệ (400)</option>
                  </select>
                </div>
              </div>

              {/* Stage Detail Card */}
              {(() => {
                const currentStage = REQUEST_STAGES[currentStageIndex];
                return (
                  <div
                    data-testid="current-stage-detail"
                    className="p-5 rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shadow-sm space-y-4"
                  >
                    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 pb-2 border-b border-zinc-100 dark:border-zinc-800">
                      <div>
                        <span className={`px-2.5 py-0.5 rounded-full text-xs font-semibold border ${currentStage.badgeColor}`}>
                          {currentStage.tech}
                        </span>
                        <h4 className="text-base font-bold text-zinc-900 dark:text-zinc-100 mt-1">
                          {currentStage.name}
                        </h4>
                      </div>
                      <span className="text-xs text-zinc-600 dark:text-zinc-400 font-mono">
                        Stage {currentStageIndex + 1} / {REQUEST_STAGES.length}
                      </span>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-xs">
                      <div className="p-3.5 rounded-xl bg-zinc-50 dark:bg-zinc-800/60 border border-zinc-200 dark:border-zinc-700/60 space-y-1">
                        <span className="font-bold text-indigo-600 dark:text-indigo-400">
                          📥 Dữ liệu vào (Input Request):
                        </span>
                        <p className="text-zinc-700 dark:text-zinc-300">{currentStage.inputDescription}</p>
                      </div>

                      <div className="p-3.5 rounded-xl bg-zinc-50 dark:bg-zinc-800/60 border border-zinc-200 dark:border-zinc-700/60 space-y-1">
                        <span className="font-bold text-teal-600 dark:text-teal-400">
                          📤 Dữ liệu xử lý xong (Output):
                        </span>
                        <p className="text-zinc-700 dark:text-zinc-300">{currentStage.outputDescription}</p>
                      </div>
                    </div>

                    <div className="space-y-1.5">
                      <span className="text-xs font-semibold text-zinc-600 dark:text-zinc-400">
                        Mã nguồn thực thi tại tầng này:
                      </span>
                      <pre className="p-3.5 rounded-xl bg-zinc-950 text-zinc-200 font-mono text-xs overflow-x-auto border border-zinc-800">
                        {currentStage.codeSnippet}
                      </pre>
                    </div>

                    <div className="p-3 rounded-xl bg-rose-50 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-900/50 text-xs text-rose-800 dark:text-rose-300 flex items-start gap-2">
                      <AlertTriangle className="w-4 h-4 text-rose-600 dark:text-rose-400 shrink-0 mt-0.5" />
                      <div>
                        <span className="font-semibold">Xử lý khi xảy ra lỗi ở tầng này: </span>
                        {currentStage.failureScenario}
                      </div>
                    </div>
                  </div>
                );
              })()}
            </div>
          )}

          {/* VISUALIZER 3B: BỘ NHỚ ẢO & TLB PAGING (HỆ ĐIỀU HÀNH) */}
          {visualizerSubMode === "tlb" && (
            <div data-testid="visualizer-tlb-box" className="space-y-6">
              <div className="p-5 rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900/60 space-y-5">
                <div className="flex items-center justify-between">
                  <h4 className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                    <Sliders className="w-4 h-4 text-teal-600 dark:text-teal-400" />
                    Bảng Điều Khiển Tham Số Phần Cứng & TLB:
                  </h4>
                  <span className="text-xs text-zinc-600 dark:text-zinc-400 font-mono">
                    Hệ điều hành: Paging & Virtual Memory
                  </span>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-3 gap-5">
                  <div className="space-y-2">
                    <div className="flex justify-between text-xs">
                      <span className="font-semibold text-zinc-700 dark:text-zinc-300">
                        Tỷ lệ TLB Hit (α):
                      </span>
                      <span className="font-mono font-bold text-teal-600 dark:text-teal-400">{tlbHitRate}%</span>
                    </div>
                    <input
                      type="range"
                      data-testid="tlb-hit-rate-slider"
                      min={50}
                      max={99}
                      value={tlbHitRate}
                      onChange={(e) => setTlbHitRate(Number(e.target.value))}
                      className="w-full accent-teal-600 cursor-pointer min-h-[30px]"
                    />
                    <div className="flex justify-between text-[10px] text-zinc-600 dark:text-zinc-400 font-mono">
                      <span>50%</span>
                      <span>Mặc định 90%</span>
                      <span>99%</span>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between text-xs">
                      <span className="font-semibold text-zinc-700 dark:text-zinc-300">
                        Độ trễ TLB (t_tlb):
                      </span>
                      <span className="font-mono font-bold text-teal-600 dark:text-teal-400">{tlbAccessTimeNs} ns</span>
                    </div>
                    <input
                      type="range"
                      data-testid="tlb-access-time-slider"
                      min={1}
                      max={10}
                      value={tlbAccessTimeNs}
                      onChange={(e) => setTlbAccessTimeNs(Number(e.target.value))}
                      className="w-full accent-teal-600 cursor-pointer min-h-[30px]"
                    />
                    <div className="flex justify-between text-[10px] text-zinc-600 dark:text-zinc-400 font-mono">
                      <span>1 ns</span>
                      <span>5 ns</span>
                      <span>10 ns</span>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between text-xs">
                      <span className="font-semibold text-zinc-700 dark:text-zinc-300">
                        Độ trễ RAM (t_ram):
                      </span>
                      <span className="font-mono font-bold text-teal-600 dark:text-teal-400">{ramAccessTimeNs} ns</span>
                    </div>
                    <input
                      type="range"
                      data-testid="ram-access-time-slider"
                      min={20}
                      max={200}
                      step={5}
                      value={ramAccessTimeNs}
                      onChange={(e) => setRamAccessTimeNs(Number(e.target.value))}
                      className="w-full accent-teal-600 cursor-pointer min-h-[30px]"
                    />
                    <div className="flex justify-between text-[10px] text-zinc-600 dark:text-zinc-400 font-mono">
                      <span>20 ns</span>
                      <span>100 ns</span>
                      <span>200 ns</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Card EAT */}
              <div
                data-testid="eat-calculation-card"
                className="p-5 rounded-2xl border border-teal-200 dark:border-teal-900/60 bg-teal-50/40 dark:bg-teal-950/20 space-y-4"
              >
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
                  <h4 className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                    <Zap className="w-4 h-4 text-teal-600 dark:text-teal-400" />
                    Thời Gian Truy Cập Hiệu Dụng Thực Tế (EAT):
                  </h4>
                  <div className="text-xl sm:text-2xl font-mono font-bold text-teal-600 dark:text-teal-400">
                    EAT = {eatCalculation.eat} ns
                  </div>
                </div>

                <div className="p-3.5 rounded-xl bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 font-mono text-xs space-y-2 text-zinc-800 dark:text-zinc-200 overflow-x-auto">
                  <div className="text-zinc-500 font-sans text-[11px]">Công thức lý thuyết chuẩn Hệ điều hành:</div>
                  <div className="font-bold text-indigo-600 dark:text-indigo-400">
                    EAT = α × (t_TLB + t_RAM) + (1 - α) × (t_TLB + 2 × t_RAM)
                  </div>
                  <div className="text-zinc-600 dark:text-zinc-400">
                    = {eatCalculation.alpha} × ({tlbAccessTimeNs} + {ramAccessTimeNs}) + {(1 - eatCalculation.alpha).toFixed(2)} × ({tlbAccessTimeNs} + 2 × {ramAccessTimeNs})
                  </div>
                  <div className="text-zinc-600 dark:text-zinc-400">
                    = {eatCalculation.alpha} × {eatCalculation.hitTime} ns (TLB Hit) + {(1 - eatCalculation.alpha).toFixed(2)} × {eatCalculation.missTime} ns (TLB Miss)
                  </div>
                  <div className="font-bold text-emerald-600 dark:text-emerald-400">
                    = {(eatCalculation.alpha * eatCalculation.hitTime).toFixed(2)} ns + {((1 - eatCalculation.alpha) * eatCalculation.missTime).toFixed(2)} ns = {eatCalculation.eat} ns
                  </div>
                </div>

                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 text-xs text-zinc-600 dark:text-zinc-400">
                  <span>
                    ⚡ Khi TLB Hit: chỉ mất <strong>{eatCalculation.hitTime} ns</strong>. Khi Miss: mất tới <strong>{eatCalculation.missTime} ns</strong>.
                  </span>
                  <span className="font-semibold text-amber-600 dark:text-amber-400">
                    Tổn hao hiệu năng do Miss: +{eatCalculation.slowdownPercentage}%
                  </span>
                </div>
              </div>

              {/* Sơ đồ 4 khối & Mô phỏng ngẫu nhiên */}
              <div className="p-5 rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 space-y-4">
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                  <h4 className="text-sm font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                    <Layers className="w-4 h-4 text-indigo-500" />
                    Sơ Đồ Phân Giải Địa Chỉ Ảo (Address Translation Pipeline):
                  </h4>
                  <button
                    type="button"
                    data-testid="simulate-memory-access-btn"
                    onClick={handleSimulateRandomAccess}
                    className="min-h-[44px] px-4 py-2 rounded-xl text-xs font-semibold bg-teal-600 hover:bg-teal-700 text-white shadow-xs inline-flex items-center gap-1.5 cursor-pointer transition"
                  >
                    <Play className="w-3.5 h-3.5" />
                    <span>Tạo Ngẫu Nhiên Truy Cập Bộ Nhớ</span>
                  </button>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-4 gap-3 text-center">
                  <div className="p-3.5 rounded-xl border border-blue-300 dark:border-blue-900 bg-blue-50/60 dark:bg-blue-950/40">
                    <span className="text-[11px] font-bold text-blue-700 dark:text-blue-300">1. CPU</span>
                    <p className="text-xs text-zinc-700 dark:text-zinc-300 mt-1 font-mono">Địa chỉ ảo</p>
                    <span className="text-[10px] text-zinc-500">[Page # | Offset]</span>
                  </div>

                  <div className="p-3.5 rounded-xl border border-teal-300 dark:border-teal-900 bg-teal-50/60 dark:bg-teal-950/40">
                    <span className="text-[11px] font-bold text-teal-700 dark:text-teal-300">2. TLB Cache ({tlbHitRate}%)</span>
                    <p className="text-xs text-zinc-700 dark:text-zinc-300 mt-1 font-mono">{tlbAccessTimeNs} ns</p>
                    <span className="text-[10px] text-teal-600 dark:text-teal-400">Tra cứu tức thì</span>
                  </div>

                  <div className="p-3.5 rounded-xl border border-amber-300 dark:border-amber-900 bg-amber-50/60 dark:bg-amber-950/40">
                    <span className="text-[11px] font-bold text-amber-700 dark:text-amber-300">3. Bảng Trang (Khi Miss)</span>
                    <p className="text-xs text-zinc-700 dark:text-zinc-300 mt-1 font-mono">+{ramAccessTimeNs} ns</p>
                    <span className="text-[10px] text-amber-600 dark:text-amber-400">Đọc RAM lần 1</span>
                  </div>

                  <div className="p-3.5 rounded-xl border border-emerald-300 dark:border-emerald-900 bg-emerald-50/60 dark:bg-emerald-950/40">
                    <span className="text-[11px] font-bold text-emerald-700 dark:text-emerald-300">4. Ô Nhớ Vật Lý (RAM)</span>
                    <p className="text-xs text-zinc-700 dark:text-zinc-300 mt-1 font-mono">+{ramAccessTimeNs} ns</p>
                    <span className="text-[10px] text-emerald-600 dark:text-emerald-400">Nạp dữ liệu / lệnh</span>
                  </div>
                </div>

                {accessHistory.length > 0 && (
                  <div className="space-y-2 pt-2 border-t border-zinc-100 dark:border-zinc-800">
                    <span className="text-xs font-semibold text-zinc-700 dark:text-zinc-300">
                      Lịch sử 5 lần truy cập bộ nhớ gần nhất:
                    </span>
                    <div className="space-y-1.5">
                      {accessHistory.map((ev) => (
                        <div
                          key={ev.id}
                          className="p-2.5 rounded-lg border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-800/60 flex items-center justify-between text-xs font-mono"
                        >
                          <div className="flex items-center gap-2">
                            <span
                              className={`px-2 py-0.5 rounded text-[10px] font-bold ${
                                ev.isHit
                                  ? "bg-emerald-100 dark:bg-emerald-950 text-emerald-700 dark:text-emerald-300"
                                  : "bg-amber-100 dark:bg-amber-950 text-amber-700 dark:text-amber-300"
                              }`}
                            >
                              {ev.isHit ? "TLB HIT" : "TLB MISS"}
                            </span>
                            <span className="text-zinc-700 dark:text-zinc-300">
                              Đ/c: {ev.virtualAddress} (Page {ev.pageNumber}, Offset {ev.offset})
                            </span>
                          </div>
                          <span className="text-zinc-500">{ev.durationNs} ns</span>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      )}

      {/* FOOTER */}
      <div className="border-t border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900/60 px-4 py-3 text-xs text-zinc-600 dark:text-zinc-400 flex flex-col sm:flex-row items-center justify-between gap-2">
        <span className="flex items-center gap-1.5">
          <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" />
          Module Coding Practice Lab: Tương thích WCAG 2.1 AA, Responsive 390×844 & Desktop
        </span>
        <span className="font-mono text-[11px] text-zinc-600 dark:text-zinc-400">
          Course: {courseId || "CS-CORE"} | Mode: {activeMainMode}
        </span>
      </div>
    </div>
  );
}

export default CodingPracticeLab;
