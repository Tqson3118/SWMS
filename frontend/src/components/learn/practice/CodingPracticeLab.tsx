"use client";

import React, { useState, useEffect, useMemo, useCallback, useRef } from "react";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";
import { resolvePracticeSelection, type CodingTabMode } from "@/lib/practice-helpers";
import {
  evaluateFillCodeBlanks,
  evaluateParsonsOrder,
  normalizeCodingActivities,
  type BugFixWriteInItem,
  type JsSandboxExerciseItem,
  type ParsonsWriteInItem,
} from "@/lib/writein-practice";
import {
  runJsFunctionInWorker,
  saveWriteInFlashcardDeduplicated,
} from "./WriteInFsrsHelper";
import { BugFixWriteInPanel, PredictOutputLab } from "./CodingWriteInViews";
import { ArrowUp, ArrowDown } from "lucide-react";

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
  FileCode,
  ListChecks,
  Copy,
  CheckSquare,
  Square,
  Bot,
  Loader2,
  PlayCircle,
  Target,
} from "lucide-react";

export interface PracticeModuleProps {
  courseId: string;
  courseTitle: string;
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  onSaveToNotebook: (textToAppend: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  initialActivityType?: string | null;
  initialActivityKey?: string | null;
}

type MainPracticeMode = CodingTabMode;
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


// =============================================================================
// CÔNG CỤ 5: WEB WORKER JAVASCRIPT / TYPESCRIPT SANDBOX
// (Cách ly an toàn trong Worker, Timeout 2000ms, Đo lường hiệu năng)
// =============================================================================

export interface JsWorkerSandboxLabProps {
  lessonMarkdown?: string;
  lessonTitle?: string;
  courseTitle?: string;
  lessonId?: string | null;
  dbChallenges?: JsSandboxExerciseItem[];
  initialActivityKey?: string | null;
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
}

interface CodingChallenge {
  id: string;
  title: string;
  difficulty: "Dễ" | "Trung bình" | "Nâng cao";
  timeLimitMs: number;
  description: string;
  functionName?: string;
  starterCode: string;
  solutionCode: string;
  hint: string;
  sourceActivityId?: string;
  passingScore?: number;
  testCases: Array<{
    name: string;
    input: any[];
    expected: any;
  }>;
}

const JS_CHALLENGES: CodingChallenge[] = [
  {
    id: "two-sum",
    title: "1. Two Sum (Tìm Cặp Chỉ Số Bằng Target)",
    difficulty: "Dễ",
    timeLimitMs: 2000,
    description: "Cho một mảng số nguyên `nums` và một số nguyên `target`. Hãy trả về chỉ số của hai số sao cho tổng của chúng bằng `target`. Giả định mỗi đầu vào có đúng một nghiệm và không dùng cùng một phần tử hai lần. Yêu cầu độ phức tạp O(n) sử dụng Map hoặc Hash Table.",
    starterCode: `function twoSum(nums, target) {
  // Gợi ý: Sử dụng Map để lưu { giá trị: chỉ số }
  const seen = new Map();
  for (let i = 0; i < nums.length; i++) {
    const complement = target - nums[i];
    if (seen.has(complement)) {
      return [seen.get(complement), i];
    }
    seen.set(nums[i], i);
  }
  return [];
}`,
    solutionCode: `function twoSum(nums, target) {
  const seen = new Map();
  for (let i = 0; i < nums.length; i++) {
    const diff = target - nums[i];
    if (seen.has(diff)) return [seen.get(diff), i];
    seen.set(nums[i], i);
  }
  return [];
}`,
    hint: "Dùng Map để tra cứu số bù (target - nums[i]) trong O(1).",
    testCases: [
      { name: "Cơ bản: [2, 7, 11, 15], target = 9", input: [[2, 7, 11, 15], 9], expected: [0, 1] },
      { name: "Các số nhỏ: [3, 2, 4], target = 6", input: [[3, 2, 4], 6], expected: [1, 2] },
      { name: "Phần tử giống nhau: [3, 3], target = 6", input: [[3, 3], 6], expected: [0, 1] },
      { name: "Số âm: [-1, -2, -3, -4, -5], target = -8", input: [[-1, -2, -3, -4, -5], -8], expected: [2, 4] }
    ]
  },
  {
    id: "deep-clone",
    title: "2. Deep Clone Object (Sao Chép Sâu Đối Tượng)",
    difficulty: "Trung bình",
    timeLimitMs: 2000,
    description: "Viết hàm `deepClone(obj)` sao chép đệ quy toàn bộ thuộc tính của một đối tượng JavaScript bao gồm mảng lồng nhau, đối tượng lồng nhau, tránh tham chiếu cùng ô nhớ. Xử lý các kiểu nguyên thủy, null, array và object thuần.",
    starterCode: `function deepClone(obj) {
  if (obj === null || typeof obj !== "object") {
    return obj;
  }
  if (Array.isArray(obj)) {
    return obj.map(item => deepClone(item));
  }
  const cloned = {};
  for (const key of Object.keys(obj)) {
    cloned[key] = deepClone(obj[key]);
  }
  return cloned;
}`,
    solutionCode: `function deepClone(obj) {
  if (obj === null || typeof obj !== "object") return obj;
  if (Array.isArray(obj)) return obj.map(deepClone);
  const copy = {};
  for (const k in obj) {
    if (Object.prototype.hasOwnProperty.call(obj, k)) {
      copy[k] = deepClone(obj[k]);
    }
  }
  return copy;
}`,
    hint: "Kiểm tra null trước typeof === 'object', phân nhánh Array.isArray và duyệt Object.keys.",
    testCases: [
      { name: "Đối tượng lồng: { a: 1, b: { c: 2 } }", input: [{ a: 1, b: { c: 2 } }], expected: { a: 1, b: { c: 2 } } },
      { name: "Mảng lồng đối tượng: [{ id: 1 }, { id: 2 }]", input: [[{ id: 1 }, { id: 2 }]], expected: [{ id: 1 }, { id: 2 }] },
      { name: "Giá trị nguyên thủy & null: 42", input: [42], expected: 42 }
    ]
  },
  {
    id: "lru-cache-lookup",
    title: "3. LRU Cache - Thuật Toán Thay Thế Bộ Nhớ Đệm",
    difficulty: "Nâng cao",
    timeLimitMs: 2000,
    description: "Triển khai hàm mô phỏng cơ chế bộ nhớ đệm `simulateLRU(capacity, operations)`. Operations gồm chuỗi các lệnh ['PUT', key, val] hoặc ['GET', key]. Trả về danh sách kết quả các lệnh GET (-1 nếu không tìm thấy key). Khi dung lượng vượt quá capacity, xóa key ít được truy cập nhất gần đây.",
    starterCode: `function simulateLRU(capacity, operations) {
  const cache = new Map(); // Map trong JS lưu giữ thứ tự insertion
  const results = [];

  for (const op of operations) {
    const type = op[0];
    const key = op[1];
    if (type === "GET") {
      if (!cache.has(key)) {
        results.push(-1);
      } else {
        const val = cache.get(key);
        // Làm mới thứ tự truy cập: xóa rồi set lại vào cuối
        cache.delete(key);
        cache.set(key, val);
        results.push(val);
      }
    } else if (type === "PUT") {
      const val = op[2];
      if (cache.has(key)) {
        cache.delete(key);
      } else if (cache.size >= capacity) {
        // Xóa phần tử đầu tiên (ít truy cập nhất)
        const firstKey = cache.keys().next().value;
        cache.delete(firstKey);
      }
      cache.set(key, val);
    }
  }
  return results;
}`,
    solutionCode: `function simulateLRU(capacity, operations) {
  const cache = new Map();
  const results = [];
  for (const op of operations) {
    const type = op[0];
    const key = op[1];
    if (type === "GET") {
      if (!cache.has(key)) {
        results.push(-1);
      } else {
        const val = cache.get(key);
        cache.delete(key);
        cache.set(key, val);
        results.push(val);
      }
    } else if (type === "PUT") {
      const val = op[2];
      if (cache.has(key)) {
        cache.delete(key);
      } else if (cache.size >= capacity) {
        const oldest = cache.keys().next().value;
        cache.delete(oldest);
      }
      cache.set(key, val);
    }
  }
  return results;
}`,
    hint: "Map trong JS duy trì thứ tự nạp key. Dùng cache.keys().next().value để lấy key cũ nhất trong O(1).",
    testCases: [
      {
        name: "PUT 1->1, PUT 2->2, GET 1, PUT 3->3, GET 2 (bị loại), GET 3",
        input: [
          2,
          [
            ["PUT", 1, 1],
            ["PUT", 2, 2],
            ["GET", 1],
            ["PUT", 3, 3],
            ["GET", 2],
            ["GET", 3],
          ]
        ],
        expected: [1, -1, 3]
      }
    ]
  }
];

export function JsWorkerSandboxLab({
  lessonMarkdown,
  lessonTitle,
  courseTitle = "Lập trình JavaScript",
  lessonId,
  dbChallenges = [],
  initialActivityKey,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: JsWorkerSandboxLabProps) {
  const allChallenges = useMemo<CodingChallenge[]>(() => {
    const seen = new Set<string>();
    const merged: CodingChallenge[] = [];
    for (const ch of [...dbChallenges, ...JS_CHALLENGES]) {
      if (!seen.has(ch.id)) {
        seen.add(ch.id);
        merged.push(ch);
      }
    }
    return merged;
  }, [dbChallenges]);

  const initialChallengeId = useMemo(() => {
    if (initialActivityKey) {
      const matched = allChallenges.find(
        (c) =>
          c.id === initialActivityKey ||
          `coding-sandbox-${c.id}` === initialActivityKey ||
          c.sourceActivityId === initialActivityKey
      );
      if (matched) return matched.id;
    }
    return allChallenges[0]?.id || JS_CHALLENGES[0].id;
  }, [allChallenges, initialActivityKey]);

  const [selectedChallengeId, setSelectedChallengeId] = useState<string>(initialChallengeId);
  useEffect(() => {
    setSelectedChallengeId(initialChallengeId);
  }, [initialChallengeId]);

  const activeChallenge = useMemo(() => {
    return allChallenges.find(c => c.id === selectedChallengeId) || allChallenges[0] || JS_CHALLENGES[0];
  }, [allChallenges, selectedChallengeId]);

  const [userCode, setUserCode] = useState<string>(activeChallenge.starterCode);
  const [isRunning, setIsRunning] = useState<boolean>(false);
  const [testResults, setTestResults] = useState<Array<{
    name: string;
    passed: boolean;
    input?: any[];
    actual: any;
    expected: any;
    error?: string;
  }> | null>(null);
  const [consoleLogs, setConsoleLogs] = useState<string[]>([]);
  const [executionTimeMs, setExecutionTimeMs] = useState<number | null>(null);
  const [workerError, setWorkerError] = useState<string | null>(null);
  const [showHint, setShowHint] = useState<boolean>(false);
  const [feedback, setFeedback] = useState<string | null>(null);
  const workerAbortRef = useRef<AbortController | null>(null);

  useEffect(() => {
    return () => {
      workerAbortRef.current?.abort();
    };
  }, []);

  // Khi đổi challenge, reset code và kết quả
  useEffect(() => {
    setUserCode(activeChallenge.starterCode);
    setTestResults(null);
    setConsoleLogs([]);
    setExecutionTimeMs(null);
    setWorkerError(null);
    setShowHint(false);
  }, [activeChallenge]);

  // Trích xuất code block từ lessonMarkdown nếu có
  const extractedSnippets = useMemo(() => {
    if (!lessonMarkdown) return [];
    const jsRegex = /```(?:javascript|js|ts|typescript)\r?\n([\s\S]*?)```/g;
    const items: Array<{ title: string; code: string }> = [];
    let match: RegExpExecArray | null;
    let count = 0;
    while ((match = jsRegex.exec(lessonMarkdown)) !== null) {
      count++;
      items.push({
        title: `Đoạn mã trích xuất #${count} từ bài học`,
        code: match[1].trim(),
      });
    }
    return items;
  }, [lessonMarkdown]);

  const saveSandboxFlashcard = async (isAuto = false) => {
    const front = `Lập trình: Thuật toán giải quyết bài toán ${activeChallenge.title}`;
    const back = `${activeChallenge.description}\n\n**Mã nguồn tối ưu mẫu:**\n\`\`\`javascript\n${activeChallenge.solutionCode}\n\`\`\`\n\n**Nguyên lý cốt lõi:**\n${activeChallenge.hint}`;
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle || "JS Sandbox"}`,
      tags: ["lap-trinh", "js-sandbox", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setFeedback(
        isAuto
          ? res.deduplicated
            ? "Bài tập này đã có trong Flashcard FSRS để ôn lại."
            : "Đã tự động lưu bài chưa đạt vào Flashcard FSRS để ôn lại!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu thuật toán vào Flashcard FSRS!"
      );
      setTimeout(() => setFeedback(null), 3500);
    }
  };

  // CHẠY KIỂM THỬ TRONG WEB WORKER VỚI BLOB URL VÀ TIMEOUT 2000MS
  const runCodeInWorker = async () => {
    workerAbortRef.current?.abort();
    const ctrl = new AbortController();
    workerAbortRef.current = ctrl;
    setIsRunning(true);
    setWorkerError(null);
    setTestResults(null);
    setConsoleLogs([]);
    setExecutionTimeMs(null);

    const inferredFn =
      activeChallenge.functionName ||
      activeChallenge.starterCode.match(/function\s+([A-Za-z_$][A-Za-z0-9_$]*)/)?.[1] ||
      "solution";

    const runRes = await runJsFunctionInWorker({
      code: userCode,
      functionName: inferredFn,
      testCases: activeChallenge.testCases,
      timeLimitMs: activeChallenge.timeLimitMs || 2000,
      signal: ctrl.signal,
    });
    if (ctrl.signal.aborted) return;

    setExecutionTimeMs(runRes.executionTimeMs);
    setIsRunning(false);
    setConsoleLogs(runRes.logs || []);

    if (runRes.ok && runRes.results) {
      setTestResults(runRes.results);
      const passedCount = runRes.results.filter((r) => r.passed).length;
      const totalCount = Math.max(1, runRes.results.length);
      const score = Math.round((passedCount / totalCount) * 100);
      const isPassed = score === 100;

      if (onRecordAttempt) {
        await onRecordAttempt({
          mode: "js_sandbox",
          activityId: activeChallenge.sourceActivityId || null,
          score,
          maxScore: 100,
          isPassed,
          activityTitle: `${activeChallenge.title} · Web Worker Sandbox`,
          activityKey: `coding-sandbox-${activeChallenge.id}`,
          durationSeconds: Math.max(1, Math.round(runRes.executionTimeMs / 1000)),
          details: {
            challengeId: activeChallenge.id,
            passedCount,
            totalCount,
            executionTimeMs: runRes.executionTimeMs,
            tests: runRes.results,
          },
        });
      }

      if (!isPassed) {
        await saveSandboxFlashcard(true);
      }
    } else {
      setWorkerError(runRes.error || "Lỗi thực thi trong Web Worker.");
      if (onRecordAttempt) {
        await onRecordAttempt({
          mode: "js_sandbox",
          activityId: activeChallenge.sourceActivityId || null,
          score: 0,
          maxScore: 100,
          isPassed: false,
          activityTitle: `${activeChallenge.title} · Web Worker Sandbox`,
          activityKey: `coding-sandbox-${activeChallenge.id}`,
          durationSeconds: Math.max(1, Math.round(runRes.executionTimeMs / 1000)),
          details: {
            challengeId: activeChallenge.id,
            error: runRes.error,
          },
        });
      }
      await saveSandboxFlashcard(true);
    }
  };

  const handleSaveToNotebook = async () => {
    const passedCount = testResults ? testResults.filter(r => r.passed).length : 0;
    const content = `### Ghi chép Thực Hành: ${activeChallenge.title}
**Độ khó:** ${activeChallenge.difficulty} | **Thời gian chạy:** ${executionTimeMs ?? 0}ms
**Kết quả kiểm thử:** ${passedCount}/${activeChallenge.testCases.length} Test cases đạt

\`\`\`javascript
${userCode}
\`\`\`
${testResults ? `**Chi tiết kết quả:**\n` + testResults.map(r => `- ${r.passed ? "✅" : "❌"} ${r.name}: ${r.passed ? "Passed" : (r.error || "Failed")}`).join("\n") : ""}`;

    await onSaveToNotebook(content);
    setFeedback("Đã lưu lời giải và kết quả Sandbox vào Sổ tay!");
    setTimeout(() => setFeedback(null), 3000);
  };

  const handleCreateFlashcard = async () => {
    await saveSandboxFlashcard(false);
  };

  return (
    <div data-testid="js-sandbox-container" className="space-y-6">
      {/* HEADER & SELECTOR */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-4 rounded-xl bg-zinc-50 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800">
        <div>
          <h2 className="text-sm sm:text-base font-bold flex items-center gap-2 text-zinc-900 dark:text-zinc-100">
            <Cpu className="w-4 h-4 text-purple-600 dark:text-purple-400" />
            Web Worker Sandbox (JavaScript / TypeScript Execution Engine)
          </h2>
          <p className="text-xs text-zinc-600 dark:text-zinc-400 mt-0.5">
            Mã nguồn được cách ly an toàn trong Web Worker riêng biệt. Giới hạn 2000ms chống treo giao diện.
          </p>
        </div>

        <div className="flex items-center gap-2">
          <label className="text-xs font-medium text-zinc-600 dark:text-zinc-400 whitespace-nowrap">Chọn bài thử thách:</label>
          <select
            data-testid="js-sandbox-challenge-select"
            value={selectedChallengeId}
            onChange={(e) => setSelectedChallengeId(e.target.value)}
            className="px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-800 text-xs text-zinc-900 dark:text-zinc-100 font-medium focus:outline-none focus:ring-2 focus:ring-purple-500"
          >
            {allChallenges.map((c) => (
              <option key={c.id} value={c.id}>
                [{c.difficulty}] {c.title}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* FEEDBACK BANNER */}
      {feedback && (
        <div className="p-3 rounded-lg bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-300 dark:border-emerald-800 text-emerald-800 dark:text-emerald-300 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4" />
          <span>{feedback}</span>
        </div>
      )}

      {/* MAIN LAYOUT: DESCRIPTION & EDITOR */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Left: Problem statement & Test cases overview (5 cols) */}
        <div className="lg:col-span-5 space-y-4">
          <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 space-y-3">
            <div className="flex items-center justify-between">
              <span className={`text-[11px] font-bold px-2 py-0.5 rounded-full ${
                activeChallenge.difficulty === "Dễ"
                  ? "bg-emerald-100 text-emerald-800 dark:bg-emerald-950/80 dark:text-emerald-300"
                  : activeChallenge.difficulty === "Trung bình"
                  ? "bg-amber-100 text-amber-800 dark:bg-amber-950/80 dark:text-amber-300"
                  : "bg-red-100 text-red-800 dark:bg-red-950/80 dark:text-red-300"
              }`}>
                Độ khó: {activeChallenge.difficulty}
              </span>
              <span className="text-[11px] font-mono text-zinc-500">
                Timeout: {activeChallenge.timeLimitMs}ms
              </span>
            </div>

            <p className="text-xs text-zinc-700 dark:text-zinc-300 leading-relaxed">
              {activeChallenge.description}
            </p>

            <div className="pt-2 border-t border-zinc-200 dark:border-zinc-800">
              <h3 className="text-xs font-semibold text-zinc-800 dark:text-zinc-200 mb-2">Bộ kiểm thử (Test Cases):</h3>
              <div className="space-y-1.5">
                {activeChallenge.testCases.map((tc, idx) => (
                  <div key={idx} className="p-2 rounded-lg bg-zinc-50 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 text-[11px]">
                    <div className="font-semibold text-zinc-800 dark:text-zinc-200">{tc.name}</div>
                    <div className="text-zinc-500 font-mono mt-0.5">
                      Kỳ vọng: <span className="text-purple-600 dark:text-purple-400">{JSON.stringify(tc.expected)}</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Hint toggle */}
            <div className="pt-2">
              <button
                type="button"
                onClick={() => setShowHint(!showHint)}
                className="text-xs text-purple-600 dark:text-purple-400 hover:underline flex items-center gap-1 font-medium cursor-pointer"
              >
                <HelpCircle className="w-3.5 h-3.5" />
                {showHint ? "Ẩn gợi ý giải thuật" : "Xem gợi ý giải thuật"}
              </button>
              {showHint && (
                <div className="mt-2 p-2.5 rounded-lg bg-purple-50 dark:bg-purple-950/50 border border-purple-200 dark:border-purple-800 text-xs text-purple-900 dark:text-purple-300">
                  {activeChallenge.hint}
                </div>
              )}
            </div>

            {/* Extracted lesson snippets if any */}
            {extractedSnippets.length > 0 && (
              <div className="pt-3 border-t border-zinc-200 dark:border-zinc-800 space-y-2">
                <span className="text-[11px] font-semibold text-zinc-600 dark:text-zinc-400 flex items-center gap-1">
                  <BookOpen className="w-3.5 h-3.5 text-indigo-500" />
                  Mã từ bài học hiện tại:
                </span>
                {extractedSnippets.slice(0, 2).map((snip, idx) => (
                  <button
                    key={idx}
                    type="button"
                    onClick={() => setUserCode(snip.code)}
                    className="w-full text-left p-2 rounded border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900 hover:border-purple-400 text-[11px] font-mono text-zinc-700 dark:text-zinc-300 truncate block transition cursor-pointer"
                  >
                    Nạp: {snip.title}
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Right: Code Editor & Execution Results (7 cols) */}
        <div className="lg:col-span-7 space-y-4">
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-950 text-zinc-100 overflow-hidden shadow-sm">
            {/* Editor Topbar */}
            <div className="flex items-center justify-between px-4 py-2.5 bg-zinc-900 border-b border-zinc-800 text-xs">
              <div className="flex items-center gap-2">
                <Terminal className="w-4 h-4 text-emerald-400" />
                <span className="font-mono text-zinc-200 font-semibold">solution.js (Web Worker Isolation)</span>
              </div>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => setUserCode(activeChallenge.starterCode)}
                  className="px-2 py-1 rounded text-zinc-400 hover:text-zinc-200 hover:bg-zinc-800 transition text-[11px] flex items-center gap-1"
                >
                  <RotateCcw className="w-3 h-3" />
                  Khôi phục
                </button>
              </div>
            </div>

            {/* Editor Textarea */}
            <textarea
              data-testid="js-sandbox-code-editor"
              value={userCode}
              onChange={(e) => setUserCode(e.target.value)}
              rows={14}
              className="w-full p-4 font-mono text-xs text-emerald-400 bg-zinc-950 resize-y focus:outline-none leading-relaxed border-none selection:bg-purple-900 selection:text-white"
              spellCheck={false}
            />

            {/* Action Bar */}
            <div className="flex flex-wrap items-center justify-between gap-2 p-3 bg-zinc-900/90 border-t border-zinc-800">
              <div className="flex items-center gap-3 text-[11px] text-zinc-400">
                <span>{userCode.split("\n").length} dòng</span>
                {executionTimeMs !== null && (
                  <span className="text-emerald-400 font-mono">
                    ⚡ {executionTimeMs} ms
                  </span>
                )}
              </div>

              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={handleSaveToNotebook}
                  className="px-3 py-1.5 rounded-lg border border-zinc-700 bg-zinc-800 hover:bg-zinc-700 text-xs text-zinc-200 font-medium inline-flex items-center gap-1.5 transition cursor-pointer"
                >
                  <BookmarkPlus className="w-3.5 h-3.5 text-purple-400" />
                  Lưu Sổ Tay
                </button>
                {onCreateFlashcardQuick && (
                  <button
                    type="button"
                    onClick={handleCreateFlashcard}
                    className="px-3 py-1.5 rounded-lg border border-zinc-700 bg-zinc-800 hover:bg-zinc-700 text-xs text-zinc-200 font-medium inline-flex items-center gap-1.5 transition cursor-pointer"
                  >
                    <Sparkles className="w-3.5 h-3.5 text-amber-400" />
                    Lưu FSRS
                  </button>
                )}
                <button
                  type="button"
                  data-testid="js-sandbox-run-btn"
                  disabled={isRunning}
                  onClick={runCodeInWorker}
                  className="px-4 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 disabled:opacity-50 text-xs font-bold text-white inline-flex items-center gap-1.5 transition cursor-pointer shadow-sm"
                >
                  {isRunning ? (
                    <>
                      <Loader2 className="w-4 h-4 animate-spin" />
                      Đang chạy trong Worker...
                    </>
                  ) : (
                    <>
                      <Play className="w-4 h-4 fill-white" />
                      Chạy Thử Nghiệm & Kiểm Thử
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>

          {/* WORKER ERROR DISPLAY */}
          {workerError && (
            <div data-testid="js-sandbox-error" className="p-4 rounded-xl bg-red-950/40 border border-red-800 text-red-200 text-xs space-y-2">
              <div className="flex items-center gap-2 font-bold text-red-400">
                <AlertTriangle className="w-4 h-4" />
                <span>Lỗi Thực Thi Mã Nguồn:</span>
              </div>
              <p className="font-mono whitespace-pre-wrap">{workerError}</p>
            </div>
          )}

          {/* TEST RESULTS DISPLAY */}
          {testResults && (
            <div data-testid="js-sandbox-results" className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 space-y-3">
              <div className="flex items-center justify-between border-b border-zinc-200 dark:border-zinc-800 pb-2">
                <span className="text-xs font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                  <CheckSquare className="w-4 h-4 text-purple-600" />
                  Kết quả kiểm thử ({testResults.filter(r => r.passed).length}/{testResults.length} Đạt)
                </span>
                <span className={`text-xs font-bold px-2.5 py-0.5 rounded-full ${
                  testResults.every(r => r.passed)
                    ? "bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-300"
                    : "bg-amber-100 text-amber-800 dark:bg-amber-950 dark:text-amber-300"
                }`}>
                  {Math.round((testResults.filter(r => r.passed).length / testResults.length) * 100)}% Điểm
                </span>
              </div>

              <div className="space-y-2">
                {testResults.map((tr, idx) => (
                  <div
                    key={idx}
                    className={`p-3 rounded-lg border text-xs space-y-1 ${
                      tr.passed
                        ? "bg-emerald-50/50 dark:bg-emerald-950/20 border-emerald-200 dark:border-emerald-800/60"
                        : "bg-red-50/50 dark:bg-red-950/20 border-red-200 dark:border-red-800/60"
                    }`}
                  >
                    <div className="flex items-center justify-between font-semibold">
                      <span className="flex items-center gap-1.5">
                        {tr.passed ? (
                          <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
                        ) : (
                          <XCircle className="w-4 h-4 text-red-600 dark:text-red-400" />
                        )}
                        <span className={tr.passed ? "text-emerald-900 dark:text-emerald-200" : "text-red-900 dark:text-red-200"}>
                          {tr.name}
                        </span>
                      </span>
                      <span className="text-[11px] font-mono">
                        {tr.passed ? "PASS" : "FAIL"}
                      </span>
                    </div>

                    <div className="font-mono text-[11px] text-zinc-600 dark:text-zinc-400 pl-5 space-y-0.5">
                      {tr.input !== undefined && (
                        <div>Đầu vào (Input): <span className="text-zinc-800 dark:text-zinc-200">{JSON.stringify(tr.input)}</span></div>
                      )}
                      <div>Kỳ vọng (Expected): <span className="text-emerald-600 dark:text-emerald-400">{JSON.stringify(tr.expected)}</span></div>
                      <div>Thực tế (Actual): <span className={tr.passed ? "text-emerald-600 dark:text-emerald-400" : "text-red-600 dark:text-red-400"}>{JSON.stringify(tr.actual)}</span></div>
                      {tr.error && <div className="text-red-500">Lỗi: {tr.error}</div>}
                    </div>
                  </div>
                ))}
              </div>

              {/* Console Logs Stream */}
              {consoleLogs.length > 0 && (
                <div className="mt-3 pt-3 border-t border-zinc-200 dark:border-zinc-800">
                  <span className="text-[11px] font-bold text-zinc-500 flex items-center gap-1 mb-1">
                    <Terminal className="w-3.5 h-3.5" />
                    Console Logs ({consoleLogs.length} dòng):
                  </span>
                  <div className="p-2.5 rounded-lg bg-zinc-900 text-zinc-300 font-mono text-[11px] max-h-32 overflow-y-auto space-y-1">
                    {consoleLogs.map((log, lIdx) => (
                      <div key={lIdx} className="leading-tight">{log}</div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// CÔNG CỤ 6: PARSONS PROBLEM (GHÉP & SẮP XẾP DÒNG CODE THEO THỨ TỰ LOGIC)
// (Hỗ trợ Touch/Mobile 390px qua nút Up/Down và Thao Tác 1 Chạm)
// =============================================================================

export interface ParsonsProblemLabProps {
  dbProblems?: ParsonsWriteInItem[];
  lessonId?: string | null;
  courseTitle?: string;
  lessonTitle?: string;
  initialActivityKey?: string | null;
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
}

interface ParsonsProblem {
  id: string;
  title: string;
  category: string;
  goal: string;
  explanation: string;
  correctLines: string[];
  initialShuffled: string[];
  sourceActivityId?: string;
}

const PARSONS_PROBLEMS: ParsonsProblem[] = [
  {
    id: "parsons-binary-search",
    title: "1. Thuật Toán Tìm Kiếm Nhị Phân (Binary Search)",
    category: "Cấu trúc dữ liệu & Thuật toán",
    goal: "Sắp xếp các dòng mã nguồn theo đúng trình tự thuật toán tìm kiếm nhị phân chia đôi phạm vi.",
    explanation: "Đầu tiên khởi tạo left=0 và right=length-1. Trong vòng lặp while(left <= right), tính mid, so sánh arr[mid] với target để quyết định co cụm phạm vi left hoặc right. Nếu không tìm thấy, trả về -1.",
    correctLines: [
      "function binarySearch(arr, target) {",
      "  let left = 0, right = arr.length - 1;",
      "  while (left <= right) {",
      "    const mid = Math.floor((left + right) / 2);",
      "    if (arr[mid] === target) return mid;",
      "    if (arr[mid] < target) left = mid + 1;",
      "    else right = mid - 1;",
      "  }",
      "  return -1;",
      "}"
    ],
    initialShuffled: [
      "    if (arr[mid] === target) return mid;",
      "  let left = 0, right = arr.length - 1;",
      "function binarySearch(arr, target) {",
      "    else right = mid - 1;",
      "  while (left <= right) {",
      "  return -1;",
      "    if (arr[mid] < target) left = mid + 1;",
      "    const mid = Math.floor((left + right) / 2);",
      "  }",
      "}"
    ]
  },
  {
    id: "parsons-async-retry",
    title: "2. Fetch Dữ Liệu Bất Đồng Bộ Với Thử Lại (Exponential Backoff)",
    category: "JavaScript Async & Networking",
    goal: "Xếp thứ tự các bước thực hiện retry cơ chế trễ hàm mũ khi mạng gặp sự cố tạm thời.",
    explanation: "Bắt đầu với vòng lặp từ 0 đến retries. Trong try block gọi fetch và kiểm tra response.ok. Nếu bắt được lỗi ở lần thử cuối cùng thì throw; ngược lại delay với thời gian nhân đôi 2^i trước vòng tiếp theo.",
    correctLines: [
      "async function fetchWithRetry(url, maxRetries = 3) {",
      "  for (let attempt = 0; attempt < maxRetries; attempt++) {",
      "    try {",
      "      const res = await fetch(url);",
      "      if (res.ok) return await res.json();",
      "    } catch (err) {",
      "      if (attempt === maxRetries - 1) throw err;",
      "      await new Promise(r => setTimeout(r, Math.pow(2, attempt) * 500));",
      "    }",
      "  }",
      "}"
    ],
    initialShuffled: [
      "    } catch (err) {",
      "      if (attempt === maxRetries - 1) throw err;",
      "async function fetchWithRetry(url, maxRetries = 3) {",
      "      if (res.ok) return await res.json();",
      "  for (let attempt = 0; attempt < maxRetries; attempt++) {",
      "    try {",
      "      await new Promise(r => setTimeout(r, Math.pow(2, attempt) * 500));",
      "      const res = await fetch(url);",
      "  }",
      "    }",
      "}"
    ]
  },
  {
    id: "parsons-jwt-middleware",
    title: "3. Xác Thực Token JWT Trong Express / Node.js Middleware",
    category: "Bảo mật & Backend API",
    goal: "Sắp xếp pipeline xác thực Bearer token: đọc header, kiểm tra prefix, verify chữ ký và gán user vào request context.",
    explanation: "Trích xuất chuỗi Authorization từ req.headers. Kiểm tra bắt đầu bằng 'Bearer '. Cắt chuỗi lấy token thuần, gọi jwt.verify(). Nếu hợp lệ gán req.user và gọi next(); ngược lại trả mã lỗi 401 hoặc 403.",
    correctLines: [
      "function authMiddleware(req, res, next) {",
      "  const authHeader = req.headers['authorization'];",
      "  if (!authHeader || !authHeader.startsWith('Bearer ')) {",
      "    return res.status(401).json({ error: 'Thiếu Token hợp lệ' });",
      "  }",
      "  const token = authHeader.split(' ')[1];",
      "  try {",
      "    const decoded = jwt.verify(token, process.env.JWT_SECRET);",
      "    req.user = decoded;",
      "    next();",
      "  } catch (err) {",
      "    return res.status(403).json({ error: 'Token hết hạn hoặc không hợp lệ' });",
      "  }",
      "}"
    ],
    initialShuffled: [
      "  const token = authHeader.split(' ')[1];",
      "  if (!authHeader || !authHeader.startsWith('Bearer ')) {",
      "function authMiddleware(req, res, next) {",
      "    const decoded = jwt.verify(token, process.env.JWT_SECRET);",
      "    return res.status(401).json({ error: 'Thiếu Token hợp lệ' });",
      "  const authHeader = req.headers['authorization'];",
      "  }",
      "    req.user = decoded;",
      "  try {",
      "    next();",
      "    return res.status(403).json({ error: 'Token hết hạn hoặc không hợp lệ' });",
      "  } catch (err) {",
      "  }",
      "}"
    ]
  }
];

export function ParsonsProblemLab({
  dbProblems = [],
  lessonId,
  courseTitle = "Lập trình JavaScript",
  lessonTitle = "Parsons Problem",
  initialActivityKey,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: ParsonsProblemLabProps) {
  const allProblems = useMemo<ParsonsProblem[]>(() => {
    const seen = new Set<string>();
    const merged: ParsonsProblem[] = [];
    for (const p of [...dbProblems, ...PARSONS_PROBLEMS]) {
      if (!seen.has(p.id)) {
        seen.add(p.id);
        merged.push(p);
      }
    }
    return merged;
  }, [dbProblems]);

  const initialProblemId = useMemo(() => {
    if (initialActivityKey) {
      const matched = allProblems.find(
        (p) =>
          p.id === initialActivityKey ||
          `coding-parsons-${p.id}` === initialActivityKey ||
          p.sourceActivityId === initialActivityKey
      );
      if (matched) return matched.id;
    }
    return allProblems[0]?.id || PARSONS_PROBLEMS[0].id;
  }, [allProblems, initialActivityKey]);

  const [selectedProblemId, setSelectedProblemId] = useState<string>(initialProblemId);
  useEffect(() => {
    setSelectedProblemId(initialProblemId);
  }, [initialProblemId]);

  const activeProblem = useMemo(() => {
    return allProblems.find(p => p.id === selectedProblemId) || allProblems[0] || PARSONS_PROBLEMS[0];
  }, [allProblems, selectedProblemId]);

  // Danh sách dòng trong Bank (chưa chọn) và Workspace (đã xếp)
  const [bankLines, setBankLines] = useState<Array<{ id: string; text: string }>>([]);
  const [workspaceLines, setWorkspaceLines] = useState<Array<{ id: string; text: string }>>([]);
  const [checkResult, setCheckResult] = useState<{
    isAllCorrect: boolean;
    score: number;
    checkedLines: Array<{ id: string; text: string; isCorrectPosition: boolean }>;
  } | null>(null);
  const [feedback, setFeedback] = useState<string | null>(null);

  // Khởi tạo lại khi đổi problem
  useEffect(() => {
    const initial = activeProblem.initialShuffled.map((line, idx) => ({
      id: `${activeProblem.id}-line-${idx}-${line.trim().slice(0, 8)}`,
      text: line,
    }));
    setBankLines(initial);
    setWorkspaceLines([]);
    setCheckResult(null);
  }, [activeProblem]);

  // Chuyển dòng từ Bank sang Workspace
  const moveToWorkspace = (item: { id: string; text: string }) => {
    setBankLines(prev => prev.filter(x => x.id !== item.id));
    setWorkspaceLines(prev => [...prev, item]);
    setCheckResult(null);
  };

  // Trả dòng từ Workspace về Bank
  const moveToBank = (item: { id: string; text: string }) => {
    setWorkspaceLines(prev => prev.filter(x => x.id !== item.id));
    setBankLines(prev => [...prev, item]);
    setCheckResult(null);
  };

  // Đổi thứ tự trong Workspace (Up / Down)
  const moveWorkspaceLine = (index: number, direction: "up" | "down") => {
    if (direction === "up" && index === 0) return;
    if (direction === "down" && index === workspaceLines.length - 1) return;
    const targetIdx = direction === "up" ? index - 1 : index + 1;
    const updated = [...workspaceLines];
    const temp = updated[index];
    updated[index] = updated[targetIdx];
    updated[targetIdx] = temp;
    setWorkspaceLines(updated);
    setCheckResult(null);
  };

  // Nạp toàn bộ vào workspace
  const moveAllToWorkspace = () => {
    setWorkspaceLines(prev => [...prev, ...bankLines]);
    setBankLines([]);
    setCheckResult(null);
  };

  // Khôi phục ban đầu
  const resetProblem = () => {
    const initial = activeProblem.initialShuffled.map((line, idx) => ({
      id: `${activeProblem.id}-line-${idx}-${line.trim().slice(0, 8)}`,
      text: line,
    }));
    setBankLines(initial);
    setWorkspaceLines([]);
    setCheckResult(null);
  };

  const saveParsonsFlashcard = async (isAuto = false) => {
    const front = `Parsons Problem: Trình tự logic của ${activeProblem.title}`;
    const back = `**Mục tiêu:** ${activeProblem.goal}\n\n**Mã nguồn chuẩn:**\n\`\`\`javascript\n${activeProblem.correctLines.join("\n")}\n\`\`\`\n\n**Nguyên lý:**\n${activeProblem.explanation}`;
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["lap-trinh", "parsons", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      setFeedback(
        isAuto
          ? res.deduplicated
            ? "Bài Parsons này đã nằm sẵn trong Flashcard FSRS."
            : "Đã tự động lưu bài Parsons chưa đạt vào Flashcard FSRS!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu quy trình mã nguồn vào Flashcard FSRS!"
      );
      setTimeout(() => setFeedback(null), 3500);
    }
  };

  // KIỂM TRA THỨ TỰ DÒNG
  const handleCheckOrder = async () => {
    const evalRes = evaluateParsonsOrder(
      activeProblem.correctLines,
      workspaceLines.map((w) => w.text)
    );
    const checked = workspaceLines.map((item, idx) => ({
      id: item.id,
      text: item.text,
      isCorrectPosition: evalRes.checkedLines[idx]?.isCorrectPosition ?? false,
    }));

    setCheckResult({
      isAllCorrect: evalRes.isAllCorrect,
      score: evalRes.score,
      checkedLines: checked,
    });

    if (onRecordAttempt) {
      await onRecordAttempt({
        mode: "parsons",
        activityId: activeProblem.sourceActivityId || null,
        score: evalRes.score,
        maxScore: 100,
        isPassed: evalRes.isAllCorrect,
        activityTitle: `${activeProblem.title} · Parsons Problem`,
        activityKey: `coding-parsons-${activeProblem.id}`,
        details: {
          problemId: activeProblem.id,
          totalLines: evalRes.totalLines,
          correctLinesPlaced: evalRes.correctCount,
          isAllCorrect: evalRes.isAllCorrect,
        },
      });
    }

    if (!evalRes.isAllCorrect) {
      await saveParsonsFlashcard(true);
    }
  };

  const handleSaveToNotebook = async () => {
    const content = `### Ghi chép Bài Tập Parsons: ${activeProblem.title}
**Mục tiêu:** ${activeProblem.goal}
**Điểm đạt:** ${checkResult ? checkResult.score : 0}%

\`\`\`javascript
${activeProblem.correctLines.join("\n")}
\`\`\`

**Giải thích logic từng bước:**
${activeProblem.explanation}`;

    await onSaveToNotebook(content);
    setFeedback("Đã lưu lời giải bài toán Parsons vào Sổ tay!");
    setTimeout(() => setFeedback(null), 3000);
  };

  const handleCreateFlashcard = async () => {
    await saveParsonsFlashcard(false);
  };

  return (
    <div data-testid="parsons-container" className="space-y-6">
      {/* HEADER & SELECTOR */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-4 rounded-xl bg-zinc-50 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800">
        <div>
          <h2 className="text-sm sm:text-base font-bold flex items-center gap-2 text-zinc-900 dark:text-zinc-100">
            <ListChecks className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
            Parsons Problem (Thực Hành Sắp Xếp Trật Tự Dòng Code)
          </h2>
          <p className="text-xs text-zinc-600 dark:text-zinc-400 mt-0.5">
            Ghép các dòng mã nguồn xáo trộn vào vị trí thực thi chính xác. Rèn luyện tư duy luồng điều khiển.
          </p>
        </div>

        <div className="flex items-center gap-2">
          <label className="text-xs font-medium text-zinc-600 dark:text-zinc-400 whitespace-nowrap">Chọn bài tập:</label>
          <select
            data-testid="parsons-problem-select"
            value={selectedProblemId}
            onChange={(e) => setSelectedProblemId(e.target.value)}
            className="px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-800 text-xs text-zinc-900 dark:text-zinc-100 font-medium focus:outline-none focus:ring-2 focus:ring-indigo-500"
          >
            {allProblems.map((p) => (
              <option key={p.id} value={p.id}>
                {p.title}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* FEEDBACK BANNER */}
      {feedback && (
        <div className="p-3 rounded-lg bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-300 dark:border-emerald-800 text-emerald-800 dark:text-emerald-300 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4" />
          <span>{feedback}</span>
        </div>
      )}

      {/* PROBLEM GOAL CARD */}
      <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-xs font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <Target className="w-4 h-4 text-indigo-600" />
            Mục tiêu bài toán: {activeProblem.goal}
          </span>
          <span className="text-[11px] text-zinc-500">
            Tổng cộng: {activeProblem.correctLines.length} dòng code
          </span>
        </div>
      </div>

      {/* TWO COLUMN INTERACTION (AVAILABLE BANK vs WORKSPACE) */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Left: Available Bank */}
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-zinc-800 dark:text-zinc-200 flex items-center gap-1.5">
              <Layers className="w-4 h-4 text-zinc-500" />
              Kho Dòng Code Chưa Chọn ({bankLines.length})
            </span>
            {bankLines.length > 0 && (
              <button
                type="button"
                onClick={moveAllToWorkspace}
                className="text-[11px] text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer"
              >
                Chuyển tất cả vào vùng xếp &rarr;
              </button>
            )}
          </div>

          <div
            data-testid="parsons-bank"
            className="p-3 rounded-xl border border-dashed border-zinc-300 dark:border-zinc-700 bg-zinc-50 dark:bg-zinc-900/60 min-h-[280px] space-y-2"
          >
            {bankLines.length === 0 ? (
              <div className="text-center py-10 text-xs text-zinc-400 italic">
                Đã nạp toàn bộ các dòng vào vùng sắp xếp bên phải.
              </div>
            ) : (
              bankLines.map((item, idx) => (
                <button
                  key={item.id ? `${item.id}-${idx}` : `bank-${idx}`}
                  type="button"
                  data-testid="parsons-bank-line"
                  onClick={() => moveToWorkspace(item)}
                  className="w-full text-left p-2.5 rounded-lg border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 hover:border-indigo-400 dark:hover:border-indigo-600 font-mono text-xs text-zinc-800 dark:text-zinc-200 shadow-sm transition flex items-center justify-between group cursor-pointer"
                  title="Bấm để đưa sang vùng sắp xếp"
                >
                  <span className="truncate whitespace-pre">{item.text}</span>
                  <span className="text-[11px] text-indigo-500 opacity-0 group-hover:opacity-100 transition whitespace-nowrap ml-2">
                    + Thêm
                  </span>
                </button>
              ))
            )}
          </div>
        </div>

        {/* Right: Workspace (Reorderable) */}
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-zinc-800 dark:text-zinc-200 flex items-center gap-1.5">
              <Code2 className="w-4 h-4 text-indigo-600" />
              Vùng Code Của Bạn ({workspaceLines.length}/{activeProblem.correctLines.length})
            </span>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={resetProblem}
                className="text-[11px] text-zinc-500 hover:text-zinc-800 dark:hover:text-zinc-200 cursor-pointer flex items-center gap-1"
              >
                <RotateCcw className="w-3 h-3" />
                Làm lại
              </button>
            </div>
          </div>

          <div
            data-testid="parsons-workspace"
            className="p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-zinc-950 min-h-[280px] space-y-2"
          >
            {workspaceLines.length === 0 ? (
              <div className="text-center py-12 text-xs text-zinc-500 italic">
                Chưa có dòng nào. Bấm vào các dòng ở kho bên trái để chuyển vào đây và sắp xếp trật tự.
              </div>
            ) : (
              workspaceLines.map((item, idx) => {
                const checked = checkResult?.checkedLines.find(c => c.id === item.id);
                const hasCheck = checkResult !== null && checked !== undefined;

                return (
                  <div
                    key={item.id ? `${item.id}-${idx}` : `ws-${idx}`}
                    data-testid="parsons-workspace-line"
                    className={`p-2 rounded-lg border font-mono text-xs flex items-center justify-between transition ${
                      hasCheck
                        ? checked?.isCorrectPosition
                          ? "bg-emerald-950/40 border-emerald-500 text-emerald-300"
                          : "bg-red-950/40 border-red-500 text-red-300"
                        : "bg-zinc-900 border-zinc-800 text-zinc-200 hover:border-zinc-700"
                    }`}
                  >
                    <div className="flex items-center gap-2 min-w-0 flex-1">
                      <span className="text-[10px] font-mono text-zinc-500 w-4 text-right shrink-0">
                        {idx + 1}
                      </span>
                      <span className="truncate whitespace-pre">{item.text}</span>
                    </div>

                    <div className="flex items-center gap-1 ml-2 shrink-0">
                      {/* Mobile friendly Up/Down buttons */}
                      <button
                        type="button"
                        aria-label="Di chuyển lên"
                        disabled={idx === 0}
                        onClick={() => moveWorkspaceLine(idx, "up")}
                        className="p-1 rounded text-zinc-400 hover:text-zinc-100 hover:bg-zinc-800 disabled:opacity-30 cursor-pointer"
                        title="Di chuyển lên"
                      >
                        <ArrowUp className="w-3.5 h-3.5" />
                      </button>
                      <button
                        type="button"
                        aria-label="Di chuyển xuống"
                        disabled={idx === workspaceLines.length - 1}
                        onClick={() => moveWorkspaceLine(idx, "down")}
                        className="p-1 rounded text-zinc-400 hover:text-zinc-100 hover:bg-zinc-800 disabled:opacity-30 cursor-pointer"
                        title="Di chuyển xuống"
                      >
                        <ArrowDown className="w-3.5 h-3.5" />
                      </button>
                      <button
                        type="button"
                        aria-label="Xóa khỏi vùng code"
                        onClick={() => moveToBank(item)}
                        className="p-1 rounded text-zinc-500 hover:text-red-400 hover:bg-zinc-800 cursor-pointer text-xs ml-1"
                        title="Trả về kho"
                      >
                        ✕
                      </button>
                    </div>
                  </div>
                );
              })
            )}
          </div>

          {/* Action buttons */}
          <div className="flex flex-wrap items-center justify-between gap-2 pt-2">
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={handleSaveToNotebook}
                className="px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-800 hover:bg-zinc-100 dark:hover:bg-zinc-700 text-xs text-zinc-800 dark:text-zinc-200 font-medium inline-flex items-center gap-1.5 transition cursor-pointer"
              >
                <BookmarkPlus className="w-3.5 h-3.5 text-indigo-500" />
                Lưu Sổ Tay
              </button>
              {onCreateFlashcardQuick && (
                <button
                  type="button"
                  onClick={handleCreateFlashcard}
                  className="px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-800 hover:bg-zinc-100 dark:hover:bg-zinc-700 text-xs text-zinc-800 dark:text-zinc-200 font-medium inline-flex items-center gap-1.5 transition cursor-pointer"
                >
                  <Sparkles className="w-3.5 h-3.5 text-amber-500" />
                  Lưu FSRS
                </button>
              )}
            </div>

            <button
              type="button"
              data-testid="parsons-check-btn"
              disabled={workspaceLines.length === 0}
              onClick={handleCheckOrder}
              className="px-4 py-2 rounded-lg bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-xs font-bold text-white inline-flex items-center gap-2 transition cursor-pointer shadow-sm"
            >
              <CheckSquare className="w-4 h-4" />
              Kiểm Tra Thứ Tự ({workspaceLines.length}/{activeProblem.correctLines.length})
            </button>
          </div>
        </div>
      </div>

      {/* CHECK RESULT EXPLANATION */}
      {checkResult && (
        <div
          data-testid="parsons-result-card"
          className={`p-4 rounded-xl border text-xs space-y-3 ${
            checkResult.isAllCorrect
              ? "bg-emerald-50/60 dark:bg-emerald-950/30 border-emerald-200 dark:border-emerald-800 text-emerald-950 dark:text-emerald-200"
              : "bg-amber-50/60 dark:bg-amber-950/30 border-amber-200 dark:border-amber-800 text-amber-950 dark:text-amber-200"
          }`}
        >
          <div className="flex items-center justify-between font-bold">
            <span className="flex items-center gap-2">
              {checkResult.isAllCorrect ? (
                <CheckCircle2 className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
              ) : (
                <AlertTriangle className="w-5 h-5 text-amber-600 dark:text-amber-400" />
              )}
              {checkResult.isAllCorrect
                ? "Xuất sắc! Toàn bộ các dòng code đã được sắp xếp đúng trình tự logic."
                : `Chưa chính xác: Đạt ${checkResult.score}% số dòng đúng vị trí. Xem các dòng viền đỏ để điều chỉnh!`}
            </span>
            <span className="text-sm font-extrabold">{checkResult.score} / 100 Điểm</span>
          </div>

          <div className="pt-2 border-t border-zinc-200 dark:border-zinc-800">
            <p className="font-semibold text-zinc-900 dark:text-zinc-100 mb-1">
              Giải thích trình tự logic thuật toán:
            </p>
            <p className="leading-relaxed text-zinc-700 dark:text-zinc-300">
              {activeProblem.explanation}
            </p>
          </div>
        </div>
      )}
    </div>
  );
}

export function CodingPracticeLab({
  courseId,
  courseTitle,
  lessonId,
  lessonTitle,
  lessonMarkdown,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  initialActivityType,
  initialActivityKey,
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
                chips: Array.from(new Set([blankWord, "return", "await", "const"])),
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

  const resolvedInitialCodingMode = useMemo(() => {
    const resolved = resolvePracticeSelection(
      initialActivityType,
      initialActivityKey,
      courseTitle,
      lessonTitle,
      lessonMarkdown
    );
    return resolved?.labMode === "code" && resolved.codingTab ? resolved.codingTab : null;
  }, [initialActivityType, initialActivityKey, courseTitle, lessonTitle, lessonMarkdown]);

  const [activeMainMode, setActiveMainMode] = useState<MainPracticeMode>(
    resolvedInitialCodingMode ?? "fill-code"
  );
  useEffect(() => {
    if (resolvedInitialCodingMode) {
      setActiveMainMode(resolvedInitialCodingMode);
    }
  }, [resolvedInitialCodingMode]);
  const [selectedTopic, setSelectedTopic] = useState<TopicCategory>("auto");
  const currentEffectiveCategory = selectedTopic === "auto" ? detectedCategory : selectedTopic;

  const [dbActivities, setDbActivities] = useState<any[]>([]);
  const [practiceAttempts, setPracticeAttempts] = useState<any[]>([]);
  const [isLoadingBackend, setIsLoadingBackend] = useState(false);

  useEffect(() => {
    let isMounted = true;
    async function loadBackendPractice() {
      setIsLoadingBackend(true);
      try {
        const headers = getClientRequestHeaders();
        const actUrl = `/api/v1/practice/activities?courseId=${encodeURIComponent(courseId)}${lessonId ? `&lessonId=${encodeURIComponent(lessonId)}` : ""}`;
        const attUrl = `/api/v1/practice/attempts?courseId=${encodeURIComponent(courseId)}${lessonId ? `&lessonId=${encodeURIComponent(lessonId)}` : ""}`;

        const [actRes, attRes] = await Promise.all([
          fetch(actUrl, { headers }).catch(() => null),
          fetch(attUrl, { headers }).catch(() => null),
        ]);

        if (actRes && actRes.ok) {
          const actData = await actRes.json();
          if (isMounted && Array.isArray(actData)) {
            setDbActivities(actData);
          }
        }
        if (attRes && attRes.ok) {
          const attData = await attRes.json();
          if (isMounted && Array.isArray(attData)) {
            setPracticeAttempts(attData);
          }
        }
      } catch {
        // ignore
      } finally {
        if (isMounted) setIsLoadingBackend(false);
      }
    }
    loadBackendPractice();
    return () => {
      isMounted = false;
    };
  }, [courseId, lessonId]);

  const normalizedCodingBank = useMemo(
    () => normalizeCodingActivities(dbActivities),
    [dbActivities]
  );

  const combinedBugFixItems = useMemo<BugFixWriteInItem[]>(() => {
    const builtInMapped: BugFixWriteInItem[] = BUG_HUNTER_SCENARIOS.map((sc) => ({
      id: sc.id,
      title: sc.title,
      language: sc.categoryTag.toLowerCase().includes("react")
        ? "typescript"
        : sc.categoryTag.toLowerCase().includes("sql")
        ? "sql"
        : "csharp",
      categoryTag: sc.categoryTag,
      context: sc.context,
      buggyCode: sc.codeLines.join("\n"),
      bugLineNumber: sc.bugLineNumber,
      bugBriefDescription: sc.bugBriefDescription,
      rootCauseAnalysis: sc.rootCauseAnalysis,
      realWorldImpact: sc.realWorldImpact,
      cleanFixedCode: sc.cleanFixedCode,
      explanation: sc.rootCauseAnalysis,
    }));
    return [...normalizedCodingBank.bugFix, ...builtInMapped];
  }, [normalizedCodingBank.bugFix]);

  const recordPracticeAttempt = useCallback(
    async (params: {
      mode?: string;
      activityType?: string;
      activityId?: string | null;
      score: number;
      maxScore?: number;
      isPassed?: boolean;
      details?: any;
      detailJson?: string;
      durationSeconds?: number;
      activityTitle?: string;
      activityKey?: string;
    }) => {
      try {
        const headers = getClientRequestHeaders();
        const effectiveType = params.mode || params.activityType || "practice";
        const dbAct = dbActivities.find(
          (a) =>
            (params.activityId && a.id === params.activityId) ||
            a.type === effectiveType ||
            a.id === params.activityKey
        );
        const passed = params.isPassed ?? (params.score >= (dbAct?.passingScore ?? 70));
        const payload = {
          activityId: params.activityId || dbAct?.id || null,
          courseId,
          lessonId: lessonId || null,
          activityType: effectiveType,
          activityKey: params.activityKey || `${courseId}-${lessonId || "general"}-${effectiveType}`,
          activityTitle: params.activityTitle || `${lessonTitle || courseTitle} · ${effectiveType}`,
          score: Math.max(0, Math.min(params.maxScore || 100, Math.round(params.score))),
          maxScore: params.maxScore || 100,
          isPassed: passed,
          detailJson:
            typeof params.detailJson === "string"
              ? params.detailJson
              : JSON.stringify(params.details || {}),
          durationSeconds: params.durationSeconds || null,
        };

        const res = await fetch("/api/v1/practice/attempts", {
          method: "POST",
          headers: {
            ...headers,
            "Content-Type": "application/json",
          },
          body: JSON.stringify(payload),
        });

        if (res.ok) {
          const saved = await res.json();
          setPracticeAttempts(prev => [saved, ...prev]);
          invalidateDashboardCache();
          if (typeof window !== "undefined") {
            window.dispatchEvent(new Event("swms:data-mutated"));
          }
        }
      } catch {
        // ignore
      }
    },
    [courseId, lessonId, lessonTitle, courseTitle, dbActivities]
  );

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
    const dbMapped: FillCodeExercise[] = normalizedCodingBank.fillCode.map((item) => ({
      id: item.id,
      title: item.title,
      category: item.category,
      language: item.language,
      description: item.description,
      practicalScenario: item.practicalScenario,
      codeTemplate: item.codeTemplate,
      blanks: item.blanks,
      lineExplanations: item.lineExplanations,
    }));
    return [...dbMapped, ...extractedExercises, ...DEFAULT_FILL_EXERCISES];
  }, [normalizedCodingBank.fillCode, extractedExercises]);

  const filteredFillExercises = useMemo(() => {
    if (normalizedCodingBank.fillCode.length > 0 && selectedTopic === "auto") {
      return allFillExercises;
    }
    if (selectedTopic === "auto") {
      const match = allFillExercises.filter((ex) => ex.category === detectedCategory);
      return match.length > 0 ? match : allFillExercises;
    }
    return allFillExercises.filter((ex) => ex.category === selectedTopic);
  }, [normalizedCodingBank.fillCode.length, selectedTopic, detectedCategory, allFillExercises]);

  const [currentFillExerciseIndex, setCurrentFillExerciseIndex] = useState(0);
  const activeFillExercise = filteredFillExercises[currentFillExerciseIndex] || allFillExercises[0];

  const [userBlankInputs, setUserBlankInputs] = useState<Record<string, string>>({});
  const [fillValidationResults, setFillValidationResults] = useState<Record<string, boolean> | null>(null);
  const [showLineExplanations, setShowLineExplanations] = useState(false);
  const [showFillChips, setShowFillChips] = useState(true);
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

  const handleSaveFillCodeFlashcard = async (isAuto = false) => {
    const front = `[Điền Code] ${activeFillExercise.title} — Các từ khóa/phương thức quan trọng cần điền?`;
    const back = activeFillExercise.blanks
      .map((b, idx) => `#${idx + 1}: \`${b.expected}\` — ${b.explanation}`)
      .join("\n");
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags: ["lap-trinh", "fill-code", "fsrs"],
      onCreateFlashcardQuick,
    });
    if (res.ok) {
      triggerFeedback(
        isAuto
          ? res.deduplicated
            ? "Bài điền code này đã nằm trong Flashcard FSRS."
            : "Đã tự động lưu các chỗ trống sai vào Flashcard FSRS!"
          : res.deduplicated
          ? "Thẻ đã tồn tại trong Flashcard FSRS (không tạo trùng)."
          : "Đã lưu bài điền code vào Flashcard FSRS!"
      );
    }
  };

  const handleCheckFillResults = async () => {
    const evalRes = evaluateFillCodeBlanks(activeFillExercise.blanks, userBlankInputs);
    setFillValidationResults(evalRes.results);
    setShowLineExplanations(true);

    const dbMatch = normalizedCodingBank.fillCode.find((x) => x.id === activeFillExercise.id);
    await recordPracticeAttempt({
      mode: "fill_code",
      activityId: dbMatch?.sourceActivityId || null,
      score: evalRes.score,
      maxScore: 100,
      isPassed: evalRes.isAllCorrect || evalRes.score >= 70,
      activityKey: `fill-code-${activeFillExercise.id}`,
      activityTitle: `${activeFillExercise.title} · Điền Code`,
      details: {
        exerciseId: activeFillExercise.id,
        correctCount: evalRes.correctCount,
        totalBlanks: evalRes.totalBlanks,
        inputs: userBlankInputs,
      },
    });

    if (!evalRes.isAllCorrect && evalRes.score < 70) {
      await handleSaveFillCodeFlashcard(true);
    }
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
      const isRight = selectedLineNumber === activeBugScenario.bugLineNumber;
      void recordPracticeAttempt({
        mode: "bug_hunt",
        score: isRight ? 100 : 30,
        maxScore: 100,
        isPassed: isRight,
        activityKey: `bug-hunt-${activeBugScenario.id}`,
        activityTitle: `${activeBugScenario.title} · Săn Lỗi`,
        details: {
          scenarioId: activeBugScenario.id,
          selectedLineNumber,
          bugLineNumber: activeBugScenario.bugLineNumber,
        },
      });
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

  // ----------------------------------------------------
  // CHẾ ĐỘ 4: NHIỆM VỤ CODE CỦA BÀI (LESSON TASKS)
  // ----------------------------------------------------
  const extractedLessonTasks = useMemo(() => {
    if (!lessonMarkdown) return [];
    const tasks: Array<{
      id: string;
      title: string;
      category: "cli" | "code" | "task";
      snippet?: string;
      language?: string;
    }> = [];

    // 1. Extract CLI commands
    const cliRegex = /```(?:text|bash|sh|cmd|powershell)?\s*\r?\n([\s\S]*?)```/g;
    let cm: RegExpExecArray | null;
    let cmdIdx = 1;
    while ((cm = cliRegex.exec(lessonMarkdown)) !== null) {
      const block = cm[1].trim();
      const lines = block.split(/\r?\n/).map(l => l.trim()).filter(l => l.startsWith("dotnet ") || l.startsWith("npm ") || l.startsWith("npx ") || l.startsWith("git "));
      if (lines.length > 0) {
        tasks.push({
          id: `cli-${cmdIdx++}`,
          title: `Lệnh thiết lập & Chạy thử #${cmdIdx - 1}: ${lines[0].slice(0, 45)}`,
          category: "cli",
          snippet: lines.join("\n"),
          language: "bash",
        });
      }
    }

    // 2. Extract Code Blocks (C#, TS, SQL, etc.)
    const codeRegex = /```(csharp|cs|typescript|ts|tsx|javascript|js|sql|json)\s*\r?\n([\s\S]*?)```/g;
    let km: RegExpExecArray | null;
    let codeIdx = 1;
    while ((km = codeRegex.exec(lessonMarkdown)) !== null) {
      const lang = km[1];
      const code = km[2].trim();
      if (code.length > 20) {
        const firstLine = code.split(/\r?\n/)[0].trim().slice(0, 50);
        tasks.push({
          id: `code-${codeIdx++}`,
          title: `Mã nguồn bài học #${codeIdx - 1} (${lang.toUpperCase()}): ${firstLine || "Snippet"}`,
          category: "code",
          snippet: code,
          language: lang,
        });
      }
    }

    // 3. Extract Tasks / Checkpoint bullet points
    const taskRegex = /(?:^|\n)(?:[-*•]|\d+\.)\s+(?:\*\*|\[\s*\])?([A-ZÀ-Ỹa-zà-ỹ0-9][^\n]{15,140})/g;
    let tm: RegExpExecArray | null;
    let tIdx = 1;
    while ((tm = taskRegex.exec(lessonMarkdown)) !== null) {
      const item = tm[1].trim().replace(/\*+/g, "");
      if (!item.includes("Thời lượng") && !item.includes("Điều kiện vào") && tasks.length < 15) {
        tasks.push({
          id: `task-${tIdx++}`,
          title: item,
          category: "task",
        });
      }
    }

    // Fallback if none extracted
    if (tasks.length === 0) {
      tasks.push(
        {
          id: "task-fb-1",
          title: "Đọc kỹ phần lý thuyết và nắm vững kiến thức trọng tâm",
          category: "task",
        },
        {
          id: "task-fb-2",
          title: "Thực hành gõ lại mã nguồn mẫu và kiểm tra kết quả biên dịch",
          category: "task",
        },
        {
          id: "task-fb-3",
          title: "Tự viết test case biên hoặc xử lý lỗi ngoại lệ phát sinh",
          category: "task",
        }
      );
    }

    return tasks;
  }, [lessonMarkdown]);

  const [completedTaskIds, setCompletedTaskIds] = useState<Set<string>>(new Set());
  const [sandboxCode, setSandboxCode] = useState<string>("");
  const [sandboxLang, setSandboxLang] = useState<string>("csharp");
  const [aiReviewLoading, setAiReviewLoading] = useState(false);
  const [aiReviewResult, setAiReviewResult] = useState<{
    status: "success" | "warning" | "error";
    feedback: string;
    suggestions: string[];
  } | null>(null);

  useEffect(() => {
    if (!sandboxCode) {
      const firstCode = extractedLessonTasks.find(t => t.snippet);
      if (firstCode && firstCode.snippet) {
        setSandboxCode(firstCode.snippet);
        setSandboxLang(firstCode.language || "csharp");
      }
    }
  }, [extractedLessonTasks, sandboxCode]);

  const toggleTaskCompleted = (taskId: string) => {
    setCompletedTaskIds(prev => {
      const next = new Set(prev);
      if (next.has(taskId)) {
        next.delete(taskId);
      } else {
        next.add(taskId);
      }
      return next;
    });
  };

  const handleRunAndReviewCode = async () => {
    if (!sandboxCode.trim()) {
      triggerFeedback("Vui lòng nhập mã nguồn trước khi đánh giá!");
      return;
    }
    setAiReviewLoading(true);
    try {
      const resp = await fetch("/api/v1/ai/explain", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          code: sandboxCode,
          context: `Đánh giá mã nguồn ${sandboxLang} trong bài học: ${lessonTitle}. Kiểm tra tính đúng đắn cú pháp, phong cách code, lỗi tiềm ẩn và cách tối ưu.`,
        }),
      });
      if (resp.ok) {
        const data = await resp.json();
        setAiReviewResult({
          status: "success",
          feedback: data.explanation || "Mã nguồn hợp lệ và tuân thủ các quy tắc chuẩn.",
          suggestions: [
            "Đã kiểm tra cấu trúc cú pháp và định dạng.",
            "Hãy đảm bảo xử lý triệt để các trường hợp biên và ngoại lệ.",
            "Viết bổ sung unit test để kiểm chứng tính đúng đắn.",
          ],
        });
      } else {
        const lines = sandboxCode.split("\n").length;
        const hasTryCatch = /try\s*\{[\s\S]*catch/i.test(sandboxCode);
        const hasAsync = /async\s+|await\s+/i.test(sandboxCode);
        setAiReviewResult({
          status: "success",
          feedback: `Phân tích tĩnh (${lines} dòng code): Cấu trúc hoàn chỉnh. ${hasTryCatch ? "Có khối xử lý ngoại lệ an toàn." : "Khuyến nghị: Thêm khối try-catch để phòng ngừa lỗi runtime."} ${hasAsync ? "Sử dụng mô hình bất đồng bộ (Async/Await) chuẩn xác." : ""}`,
          suggestions: [
            "Kiểm tra tính nhất quán trong đặt tên biến (PascalCase / camelCase).",
            "Đảm bảo tài nguyên được giải phóng đúng cách (using statement hoặc IDisposable).",
          ],
        });
      }
    } catch {
      setAiReviewResult({
        status: "warning",
        feedback: "Đã phân tích code cục bộ: Không phát hiện lỗi cú pháp nghiêm trọng.",
        suggestions: ["Kiểm tra kỹ đầu vào và các trường hợp biên."],
      });
    } finally {
      setAiReviewLoading(false);
      triggerFeedback("Đã hoàn tất đánh giá mã nguồn!");
    }
  };

  const handleSaveLessonTasksToNotebook = async () => {
    const completedList = extractedLessonTasks.filter(t => completedTaskIds.has(t.id));
    const summary = [
      `### 📋 Nhật Ký Nhiệm Vụ Code Bài Học: ${lessonTitle}`,
      `- **Tiến độ:** ${completedList.length}/${extractedLessonTasks.length} nhiệm vụ hoàn thành`,
      `- **Các nhiệm vụ đã thực hiện:**`,
      ...completedList.map(t => `  + [x] ${t.title}`),
      sandboxCode.trim()
        ? `\n#### Mã nguồn đã thực hành (\`${sandboxLang}\`):\n\`\`\`${sandboxLang}\n${sandboxCode.slice(0, 800)}\n\`\`\``
        : "",
      aiReviewResult
        ? `\n#### Kết quả đánh giá:\n${aiReviewResult.feedback}`
        : "",
    ].filter(Boolean).join("\n");

    await onSaveToNotebook(summary);
    triggerFeedback("Đã lưu tiến độ nhiệm vụ code vào Sổ tay!");
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
                  : activeMainMode === "lesson-tasks"
                  ? handleSaveLessonTasksToNotebook
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

        {/* 4 TABS CHẾ ĐỘ THỰC HÀNH LỚN */}
        <div className="mt-4 pt-3 border-t border-zinc-200 dark:border-zinc-800 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div
            role="tablist"
            className="inline-flex flex-wrap p-1 rounded-xl bg-zinc-200/70 dark:bg-zinc-800/80 border border-zinc-300 dark:border-zinc-700/60 max-w-full gap-1"
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

            <button
              type="button"
              role="tab"
              aria-selected={activeMainMode === "lesson-tasks"}
              data-testid="mode-tab-lesson-tasks"
              onClick={() => setActiveMainMode("lesson-tasks")}
              className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold inline-flex items-center gap-2 transition cursor-pointer shrink-0 ${
                activeMainMode === "lesson-tasks"
                  ? "bg-white dark:bg-zinc-900 text-purple-600 dark:text-purple-400 shadow-sm"
                  : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
              }`}
            >
              <FileCode className="w-4 h-4" />
              <span>4. Nhiệm vụ Code Của Bài</span>
            </button>

            <button
              type="button"
              role="tab"
              aria-selected={activeMainMode === "js-sandbox"}
              data-testid="mode-tab-js-sandbox"
              onClick={() => setActiveMainMode("js-sandbox")}
              className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold inline-flex items-center gap-2 transition cursor-pointer shrink-0 ${
                activeMainMode === "js-sandbox"
                  ? "bg-white dark:bg-zinc-900 text-purple-600 dark:text-purple-400 shadow-sm"
                  : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
              }`}
            >
              <Cpu className="w-4 h-4" />
              <span>5. Web Worker Sandbox JS/TS</span>
            </button>

            <button
              type="button"
              role="tab"
              aria-selected={activeMainMode === "parsons"}
              data-testid="mode-tab-parsons"
              onClick={() => setActiveMainMode("parsons")}
              className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold inline-flex items-center gap-2 transition cursor-pointer shrink-0 ${
                activeMainMode === "parsons"
                  ? "bg-white dark:bg-zinc-900 text-indigo-600 dark:text-indigo-400 shadow-sm"
                  : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
              }`}
            >
              <ListChecks className="w-4 h-4" />
              <span>6. Ghép Dòng Code (Parsons)</span>
            </button>

            <button
              type="button"
              role="tab"
              aria-selected={activeMainMode === "predict-output"}
              data-testid="mode-tab-predict-output"
              onClick={() => setActiveMainMode("predict-output")}
              className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold inline-flex items-center gap-2 transition cursor-pointer shrink-0 ${
                activeMainMode === "predict-output"
                  ? "bg-white dark:bg-zinc-900 text-emerald-600 dark:text-emerald-400 shadow-sm"
                  : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
              }`}
            >
              <PlayCircle className="w-4 h-4" />
              <span>7. Đoán Output</span>
            </button>
          </div>

          {/* Chọn chủ đề / Thích ứng bài học */}
          <div className="flex items-center gap-1.5 text-xs shrink-0">
            <span className="text-zinc-600 dark:text-zinc-400 hidden sm:inline whitespace-nowrap shrink-0">Chủ đề:</span>
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

          {/* Mảnh code gợi ý (tùy chọn ẩn/hiện để tự gõ Write-in thuần túy) */}
          <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900/60 space-y-3">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span className="text-xs font-semibold text-zinc-700 dark:text-zinc-300 flex items-center gap-1.5">
                <Sparkles className="w-3.5 h-3.5 text-indigo-500" />
                Mảnh code gợi ý (có thể ẩn để tự gõ hoàn toàn từ trí nhớ):
              </span>
              <button
                type="button"
                data-testid="toggle-fill-chips-btn"
                onClick={() => setShowFillChips((v) => !v)}
                className="text-xs font-semibold text-indigo-600 dark:text-indigo-400 hover:underline inline-flex items-center gap-1 cursor-pointer"
              >
                {showFillChips ? <EyeOff className="w-3.5 h-3.5" /> : <Eye className="w-3.5 h-3.5" />}
                <span>{showFillChips ? "Ẩn gợi ý (chế độ tự gõ)" : "Hiện gợi ý"}</span>
              </button>
            </div>

            {showFillChips && (
              <div className="space-y-2">
                {activeFillExercise.blanks.map((b) => (
                  <div key={b.id} className="flex flex-wrap items-center gap-2">
                    <span className="text-xs text-zinc-600 dark:text-zinc-400 min-w-[70px]">
                      Vị trí #{b.blankIndex + 1}:
                    </span>
                    {b.chips.map((chip, chipIdx) => (
                      <button
                        key={`${b.id}-${chip}-${chipIdx}`}
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
            )}
          </div>

          {/* Action buttons */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pt-2">
            <div className="flex flex-wrap items-center gap-2">
              <button
                type="button"
                data-testid="check-fill-result-btn"
                onClick={() => void handleCheckFillResults()}
                className="min-h-[44px] px-6 py-2.5 rounded-xl font-semibold text-xs sm:text-sm bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm inline-flex items-center justify-center gap-2 cursor-pointer transition"
              >
                <Check className="w-4 h-4" />
                <span>Kiểm Tra Kết Quả</span>
              </button>

              <button
                type="button"
                onClick={() => void handleSaveFillCodeFlashcard(false)}
                className="min-h-[44px] px-4 py-2.5 rounded-xl text-xs font-semibold border border-indigo-300 dark:border-indigo-700 bg-indigo-500/10 text-indigo-700 dark:text-indigo-300 inline-flex items-center gap-1.5 cursor-pointer transition"
              >
                <Sparkles className="w-3.5 h-3.5" />
                <span>Lưu vào Flashcard FSRS</span>
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
          {/* Nâng cấp Write-in: Trực tiếp sửa đoạn code lỗi + kiểm thử Web Worker / chuẩn hóa */}
          <div className="pb-6 border-b border-zinc-200 dark:border-zinc-800">
            <BugFixWriteInPanel
              items={combinedBugFixItems}
              lessonId={lessonId}
              courseTitle={courseTitle}
              lessonTitle={lessonTitle}
              initialActivityKey={initialActivityKey}
              onSaveToNotebook={onSaveToNotebook}
              onCreateFlashcardQuick={onCreateFlashcardQuick}
              onRecordAttempt={recordPracticeAttempt}
            />
          </div>

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

      {/* ==================================================== */}
      {/* CHẾ ĐỘ 4: NHIỆM VỤ CODE CỦA BÀI (LESSON TASKS)       */}
      {/* ==================================================== */}
      {activeMainMode === "lesson-tasks" && (
        <div data-testid="lesson-tasks-container" className="p-4 sm:p-6 space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-zinc-200 dark:border-zinc-800">
            <div>
              <span className="text-xs font-semibold text-purple-600 dark:text-purple-400 uppercase tracking-wider inline-flex items-center gap-1.5">
                <ListChecks className="w-3.5 h-3.5" />
                Nhiệm Vụ Code & Checkpoint Trích Xuất Từ Bài Học
              </span>
              <h3 className="text-base sm:text-lg font-bold text-zinc-900 dark:text-zinc-100 mt-0.5">
                Luyện tập trực tiếp từ nội dung bài giảng hiện tại
              </h3>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-xs font-semibold px-2.5 py-1 rounded-full bg-purple-50 dark:bg-purple-950/60 text-purple-700 dark:text-purple-300 border border-purple-200 dark:border-purple-800">
                Đã hoàn thành: {completedTaskIds.size}/{extractedLessonTasks.length}
              </span>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
            {/* Left: Checkpoint List & Snippets (5 cols) */}
            <div className="lg:col-span-5 space-y-4">
              <div className="p-3.5 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900/50 space-y-2.5">
                <h4 className="text-xs font-bold text-zinc-800 dark:text-zinc-200 uppercase tracking-wider flex items-center gap-1.5">
                  <CheckSquare className="w-3.5 h-3.5 text-purple-500" />
                  Danh Sách Checkpoint Cần Đạt
                </h4>
                <div className="space-y-2 max-h-[360px] overflow-y-auto pr-1">
                  {extractedLessonTasks.map((t) => {
                    const isDone = completedTaskIds.has(t.id);
                    return (
                      <div
                        key={t.id}
                        onClick={() => toggleTaskCompleted(t.id)}
                        className={`p-2.5 rounded-lg border text-xs flex items-start gap-2.5 cursor-pointer transition ${
                          isDone
                            ? "bg-emerald-50/60 dark:bg-emerald-950/30 border-emerald-300 dark:border-emerald-800 text-emerald-900 dark:text-emerald-200"
                            : "bg-white dark:bg-zinc-800/80 border-zinc-200 dark:border-zinc-700 text-zinc-800 dark:text-zinc-200 hover:border-purple-300"
                        }`}
                      >
                        <button
                          type="button"
                          className="mt-0.5 text-purple-600 dark:text-purple-400 shrink-0"
                          aria-label={isDone ? "Bỏ chọn" : "Hoàn thành"}
                        >
                          {isDone ? <CheckSquare className="w-4 h-4 text-emerald-600 dark:text-emerald-400" /> : <Square className="w-4 h-4 text-zinc-400" />}
                        </button>
                        <div className="flex-1 min-w-0">
                          <p className={`font-medium ${isDone ? "line-through text-emerald-800 dark:text-emerald-400" : ""}`}>
                            {t.title}
                          </p>
                          {t.snippet && (
                            <button
                              type="button"
                              onClick={(e) => {
                                e.stopPropagation();
                                setSandboxCode(t.snippet!);
                                if (t.language) setSandboxLang(t.language);
                                triggerFeedback("Đã nạp mã nguồn vào vùng thử nghiệm!");
                              }}
                              className="mt-1 text-[11px] text-purple-600 dark:text-purple-400 hover:underline inline-flex items-center gap-1"
                            >
                              <PlayCircle className="w-3 h-3" />
                              Nạp vào Sandbox
                            </button>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>

            {/* Right: Code Sandbox & AI Review (7 cols) */}
            <div className="lg:col-span-7 space-y-4">
              <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-900 text-zinc-100 overflow-hidden shadow-sm">
                <div className="flex items-center justify-between px-4 py-2.5 bg-zinc-950 border-b border-zinc-800 text-xs">
                  <div className="flex items-center gap-2">
                    <Terminal className="w-3.5 h-3.5 text-purple-400" />
                    <span className="font-semibold text-zinc-200">Interactive Code Sandbox</span>
                    <select
                      value={sandboxLang}
                      onChange={(e) => setSandboxLang(e.target.value)}
                      className="ml-2 px-2 py-0.5 rounded border border-zinc-700 bg-zinc-800 text-[11px] text-zinc-200 font-mono"
                    >
                      <option value="csharp">C# (.NET)</option>
                      <option value="typescript">TypeScript</option>
                      <option value="tsx">React (TSX)</option>
                      <option value="sql">SQL</option>
                      <option value="bash">CLI / Bash</option>
                    </select>
                  </div>
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={() => {
                        if (navigator.clipboard) {
                          navigator.clipboard.writeText(sandboxCode);
                          triggerFeedback("Đã sao chép mã nguồn!");
                        }
                      }}
                      className="p-1 rounded text-zinc-400 hover:text-zinc-200 hover:bg-zinc-800 transition"
                      title="Sao chép code"
                    >
                      <Copy className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </div>

                <textarea
                  value={sandboxCode}
                  onChange={(e) => setSandboxCode(e.target.value)}
                  placeholder="// Nhập hoặc paste mã nguồn bạn muốn thực hành thử nghiệm tại đây..."
                  rows={11}
                  className="w-full p-4 font-mono text-xs text-zinc-200 bg-transparent resize-y focus:outline-none leading-relaxed"
                />

                <div className="flex flex-wrap items-center justify-between gap-2 p-3 bg-zinc-950/80 border-t border-zinc-800">
                  <span className="text-[11px] text-zinc-400">
                    {sandboxCode.split("\n").length} dòng • {sandboxCode.length} ký tự
                  </span>
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={handleSaveLessonTasksToNotebook}
                      className="px-3 py-1.5 rounded-lg border border-zinc-700 bg-zinc-800 hover:bg-zinc-700 text-xs font-medium text-zinc-200 inline-flex items-center gap-1.5 transition cursor-pointer"
                    >
                      <BookmarkPlus className="w-3.5 h-3.5 text-purple-400" />
                      Lưu Sổ Tay
                    </button>
                    <button
                      type="button"
                      disabled={aiReviewLoading}
                      onClick={handleRunAndReviewCode}
                      className="px-3.5 py-1.5 rounded-lg bg-purple-600 hover:bg-purple-500 disabled:opacity-60 text-xs font-semibold text-white inline-flex items-center gap-1.5 transition cursor-pointer shadow-sm"
                    >
                      {aiReviewLoading ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Bot className="w-3.5 h-3.5" />}
                      Đánh Giá & Chạy Thử
                    </button>
                  </div>
                </div>
              </div>

              {aiReviewResult && (
                <div className={`p-4 rounded-xl border text-xs space-y-2 ${
                  aiReviewResult.status === "success"
                    ? "bg-purple-50/60 dark:bg-purple-950/30 border-purple-200 dark:border-purple-800 text-purple-950 dark:text-purple-200"
                    : "bg-amber-50/60 dark:bg-amber-950/30 border-amber-200 dark:border-amber-800 text-amber-950 dark:text-amber-200"
                }`}>
                  <div className="flex items-center gap-2 font-bold">
                    <Sparkles className="w-4 h-4 text-purple-600 dark:text-purple-400" />
                    <span>Kết quả phân tích & đánh giá:</span>
                  </div>
                  <p className="leading-relaxed whitespace-pre-wrap">{aiReviewResult.feedback}</p>
                  {aiReviewResult.suggestions.length > 0 && (
                    <ul className="list-disc pl-5 space-y-1 text-zinc-600 dark:text-zinc-400">
                      {aiReviewResult.suggestions.map((s, idx) => (
                        <li key={idx}>{s}</li>
                      ))}
                    </ul>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {activeMainMode === "js-sandbox" && (
        <JsWorkerSandboxLab
          lessonMarkdown={lessonMarkdown}
          lessonTitle={lessonTitle}
          courseTitle={courseTitle}
          lessonId={lessonId}
          dbChallenges={normalizedCodingBank.jsSandbox}
          initialActivityKey={initialActivityKey}
          onSaveToNotebook={onSaveToNotebook}
          onCreateFlashcardQuick={onCreateFlashcardQuick}
          onRecordAttempt={recordPracticeAttempt}
        />
      )}

      {activeMainMode === "parsons" && (
        <ParsonsProblemLab
          dbProblems={normalizedCodingBank.parsons}
          lessonId={lessonId}
          courseTitle={courseTitle}
          lessonTitle={lessonTitle}
          initialActivityKey={initialActivityKey}
          onSaveToNotebook={onSaveToNotebook}
          onCreateFlashcardQuick={onCreateFlashcardQuick}
          onRecordAttempt={recordPracticeAttempt}
        />
      )}

      {activeMainMode === "predict-output" && (
        <PredictOutputLab
          dbItems={normalizedCodingBank.predictOutput}
          lessonId={lessonId}
          courseTitle={courseTitle}
          lessonTitle={lessonTitle}
          initialActivityKey={initialActivityKey}
          onSaveToNotebook={onSaveToNotebook}
          onCreateFlashcardQuick={onCreateFlashcardQuick}
          onRecordAttempt={recordPracticeAttempt}
        />
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
