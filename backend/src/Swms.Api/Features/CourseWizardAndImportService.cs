using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Ai;
using Swms.Api.Infrastructure.Database;

namespace Swms.Api.Features;

public record WizardClarifyQuestion(string Id, string Question, List<string> Options);
public record WizardClarifyResponse(string Topic, List<WizardClarifyQuestion> Questions);

public record WizardLessonPreviewDto(string Title, string Summary, int EstimatedMinutes, string? FilePath = null, string? VideoUrl = null);
public record WizardChapterPreviewDto(string Title, int SortOrder, List<WizardLessonPreviewDto> Lessons);
public record WizardPreviewResponse(string Topic, string CourseTitle, string Description, List<WizardChapterPreviewDto> Chapters);

public record WizardCommitRequest(
    string Topic,
    string? Title,
    string? Description,
    List<WizardChapterPreviewDto> Chapters,
    Guid? IdempotencyKey = null
);

public static class CourseWizardAndImportService
{
    private static readonly Regex HeadingRegex = new(
        @"^\s*(?:Chương|CHƯƠNG|Phần|PHẦN|Bài|BÀI|Mục|MỤC|Chapter|CHAPTER|Section|SECTION|#+\s+|[IVX]{1,5}[\.\)]\s+|\d+(?:\.\d+){0,2}[\.\)]\s+)(.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // ========================================================================
    // 1. MULTI-FORMAT TEXT EXTRACTION (.docx, .md, .txt)
    // ========================================================================
    public static string ExtractTextFromBytes(byte[] bytes, string extension)
    {
        var ext = (extension ?? string.Empty).ToLowerInvariant().Trim();
        if (ext == ".docx")
        {
            return ExtractDocxText(bytes);
        }

        // Default UTF-8 text for .md, .markdown, .txt, etc.
        try
        {
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return Encoding.Latin1.GetString(bytes);
        }
    }

    private static string ExtractDocxText(byte[] bytes)
    {
        try
        {
            using var ms = new MemoryStream(bytes);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
            var entry = zip.GetEntry("word/document.xml");
            if (entry == null) return string.Empty;

            var rels = LoadDocxExternalHyperlinks(zip);

            using var stream = entry.Open();
            var xdoc = XDocument.Load(stream);
            XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            XNamespace rNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

            var body = xdoc.Root?.Element(w + "body");
            if (body == null)
            {
                var fallbackSb = new StringBuilder();
                foreach (var para in xdoc.Descendants(w + "p"))
                {
                    var line = ExtractDocxParagraphText(para, w, rNs, rels);
                    if (!string.IsNullOrEmpty(line))
                    {
                        fallbackSb.AppendLine(line);
                        fallbackSb.AppendLine();
                    }
                }
                return fallbackSb.ToString().Trim();
            }

            var sb = new StringBuilder();
            bool previousWasListItem = false;

            foreach (var element in body.Elements())
            {
                if (element.Name == w + "p")
                {
                    var line = ExtractDocxParagraphText(element, w, rNs, rels);
                    if (string.IsNullOrEmpty(line)) continue;

                    var pPr = element.Element(w + "pPr");
                    var pStyle = pPr?.Element(w + "pStyle")?.Attribute(w + "val")?.Value?.Trim() ?? string.Empty;
                    var hasNumPr = pPr?.Element(w + "numPr") != null;

                    string formattedLine;
                    bool isListItem = false;

                    if (string.Equals(pStyle, "Title", StringComparison.OrdinalIgnoreCase))
                    {
                        formattedLine = $"# {line}";
                    }
                    else if (string.Equals(pStyle, "Heading1", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(pStyle, "Heading 1", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(pStyle, "1", StringComparison.OrdinalIgnoreCase))
                    {
                        formattedLine = $"## {line}";
                    }
                    else if (string.Equals(pStyle, "Heading2", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(pStyle, "Heading 2", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(pStyle, "2", StringComparison.OrdinalIgnoreCase))
                    {
                        formattedLine = $"### {line}";
                    }
                    else if (string.Equals(pStyle, "Heading3", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(pStyle, "Heading 3", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(pStyle, "Heading4", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(pStyle, "Heading 4", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(pStyle, "3", StringComparison.OrdinalIgnoreCase))
                    {
                        formattedLine = $"#### {line}";
                    }
                    else if (pStyle.Contains("ListBullet", StringComparison.OrdinalIgnoreCase) ||
                             (hasNumPr && !StartsWithOrderedPrefix(line)))
                    {
                        isListItem = true;
                        formattedLine = line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal)
                            ? line
                            : $"- {line}";
                    }
                    else
                    {
                        isListItem = StartsWithOrderedPrefix(line);
                        formattedLine = line;
                    }

                    if (previousWasListItem && !isListItem && sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine(formattedLine);
                    if (!isListItem)
                    {
                        sb.AppendLine();
                    }

                    previousWasListItem = isListItem;
                }
                else if (element.Name == w + "tbl")
                {
                    if (previousWasListItem && sb.Length > 0)
                    {
                        sb.AppendLine();
                        previousWasListItem = false;
                    }

                    var tableMd = ExtractDocxTableMarkdown(element, w, rNs, rels);
                    if (!string.IsNullOrEmpty(tableMd))
                    {
                        sb.AppendLine(tableMd);
                        sb.AppendLine();
                    }
                }
            }

            return sb.ToString().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static Dictionary<string, string> LoadDocxExternalHyperlinks(ZipArchive zip)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var relsEntry = zip.GetEntry("word/_rels/document.xml.rels");
        if (relsEntry == null) return map;

        try
        {
            using var stream = relsEntry.Open();
            var relDoc = XDocument.Load(stream);
            foreach (var rel in relDoc.Descendants().Where(e => e.Name.LocalName == "Relationship"))
            {
                var id = rel.Attribute("Id")?.Value;
                var target = rel.Attribute("Target")?.Value;
                var targetMode = rel.Attribute("TargetMode")?.Value;
                if (!string.IsNullOrWhiteSpace(id) &&
                    !string.IsNullOrWhiteSpace(target) &&
                    (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                     target.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                {
                    map[id] = target.Trim();
                }
            }
        }
        catch
        {
            // Ignore malformed optional rels
        }

        return map;
    }

    private static string ExtractDocxParagraphText(
        XElement para,
        XNamespace w,
        XNamespace rNs,
        IReadOnlyDictionary<string, string> rels)
    {
        var sb = new StringBuilder();
        foreach (var child in para.Elements())
        {
            if (child.Name == w + "r")
            {
                AppendDocxRunText(child, w, sb);
            }
            else if (child.Name == w + "hyperlink")
            {
                var innerSb = new StringBuilder();
                foreach (var r in child.Elements(w + "r"))
                {
                    AppendDocxRunText(r, w, innerSb);
                }
                var linkText = innerSb.ToString().Trim();
                var rId = child.Attribute(rNs + "id")?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(linkText) && rels.TryGetValue(rId, out var url))
                {
                    sb.Append('[').Append(linkText).Append("](").Append(url).Append(')');
                }
                else
                {
                    sb.Append(innerSb);
                }
            }
            else if (child.Name != w + "pPr" && child.Name != w + "bookmarkStart" && child.Name != w + "bookmarkEnd")
            {
                foreach (var t in child.Descendants(w + "t"))
                {
                    sb.Append(t.Value);
                }
            }
        }
        return sb.ToString().Trim();
    }

    private static void AppendDocxRunText(XElement run, XNamespace w, StringBuilder sb)
    {
        foreach (var node in run.Elements())
        {
            if (node.Name == w + "t" || node.Name == w + "delText")
            {
                sb.Append(node.Value);
            }
            else if (node.Name == w + "tab")
            {
                sb.Append(' ');
            }
            else if (node.Name == w + "br" || node.Name == w + "cr")
            {
                sb.Append(' ');
            }
        }
    }

    private static string ExtractDocxTableMarkdown(
        XElement table,
        XNamespace w,
        XNamespace rNs,
        IReadOnlyDictionary<string, string> rels)
    {
        var parsedRows = new List<List<string>>();
        foreach (var tr in table.Elements(w + "tr"))
        {
            var rowCells = new List<string>();
            foreach (var tc in tr.Elements(w + "tc"))
            {
                var cellParts = tc.Elements(w + "p")
                    .Select(p => ExtractDocxParagraphText(p, w, rNs, rels))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Replace("\r", " ").Replace("\n", " ").Trim())
                    .ToList();

                var cellText = string.Join("; ", cellParts)
                    .Replace("|", "\\|");
                rowCells.Add(cellText);
            }

            if (rowCells.Any(c => !string.IsNullOrWhiteSpace(c)))
            {
                parsedRows.Add(rowCells);
            }
        }

        if (parsedRows.Count == 0) return string.Empty;

        int colCount = parsedRows.Max(r => r.Count);
        if (colCount == 0) return string.Empty;

        static List<string> PadRow(List<string> row, int cols)
        {
            if (row.Count == cols) return row;
            var padded = new List<string>(cols);
            padded.AddRange(row);
            while (padded.Count < cols) padded.Add(string.Empty);
            return padded;
        }

        var sb = new StringBuilder();
        var header = PadRow(parsedRows[0], colCount);
        sb.Append("| ").Append(string.Join(" | ", header)).AppendLine(" |");
        sb.Append("| ").Append(string.Join(" | ", Enumerable.Repeat("---", colCount))).AppendLine(" |");

        for (int i = 1; i < parsedRows.Count; i++)
        {
            var row = PadRow(parsedRows[i], colCount);
            sb.Append("| ").Append(string.Join(" | ", row)).AppendLine(" |");
        }

        return sb.ToString().TrimEnd();
    }

    private static bool StartsWithOrderedPrefix(string line)
    {
        int i = 0;
        while (i < line.Length && char.IsDigit(line[i])) i++;
        if (i == 0 || i >= line.Length) return false;
        return (line[i] == '.' || line[i] == ')') && i + 1 < line.Length && line[i + 1] == ' ';
    }

    // ========================================================================
    // 2. TEMPLATE ZIP GENERATION (SWM Standard Package)
    // ========================================================================
    public static byte[] GenerateTemplateZip()
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            // 1. manifest.yaml
            var manifestEntry = archive.CreateEntry("manifest.yaml", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(manifestEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("""
                schema_version: 2.0.0
                package_id: package:swm:template-sample
                title: "Khóa học Mẫu: Lập trình Web Full-Stack Hiện đại"
                slug: "swm-fullstack-template"
                description: "Khóa học mẫu cấu trúc chuẩn SWM với bài học chi tiết, bộ thẻ ghi nhớ FSRS và đề kiểm tra định hình."
                subject: "CongNgheThongTin"
                version: 1.0.0
                chapters:
                  - title: "Chương 1: Khởi động & Nền tảng Kiến trúc"
                    sort_order: 1
                    lessons:
                      - title: "Bài 1: Tổng quan Hệ thống & Thiết lập Môi trường"
                        slug: "bai-1-tong-quan"
                        sort_order: 1
                        file: "lessons/01_bai_hoc_mau.md"
                      - title: "Bài 2: Thiết kế RESTful API & Cơ sở Dữ liệu"
                        slug: "bai-2-api-database"
                        sort_order: 2
                        file: "lessons/02_bai_hoc_api.md"
                  - title: "Chương 2: Thực chiến Giao diện & Tối ưu Trí nhớ"
                    sort_order: 2
                    lessons:
                      - title: "Bài 3: Xây dựng Giao diện React & Quản trị Trạng thái"
                        slug: "bai-3-react-state"
                        sort_order: 3
                        file: "lessons/03_bai_hoc_react.md"
                """);
            }

            // 2. lessons/01_bai_hoc_mau.md
            var l1Entry = archive.CreateEntry("lessons/01_bai_hoc_mau.md", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(l1Entry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("""
                # 📚 Bài học: Tổng quan Hệ thống & Thiết lập Môi trường

                ## 🎯 Mục tiêu
                - Nắm vững kiến trúc Client-Server và luồng dữ liệu 3 lớp.
                - Cài đặt môi trường .NET 10 SDK và Node.js LTS thành công.
                - Khởi chạy ứng dụng Hello World đầu tiên và kiểm tra kết nối API.

                ## 📖 Khái niệm cốt lõi
                ### 1. Kiến trúc Hợp nhất (Unified Architecture)
                Hệ thống gồm tầng Frontend hiện đại (Next.js 15, React 19) giao tiếp với Backend ASP.NET Core Minimal API thông qua chuẩn RESTful HTTP. Dữ liệu được quản trị an toàn bằng Entity Framework Core kết hợp thuật toán lặp lại ngắt quãng FSRS v5.

                ### 2. Chuẩn trao đổi dữ liệu RESTful
                Giao thức HTTP định nghĩa các động từ chuẩn (GET, POST, PUT, DELETE) cùng các mã trạng thái rõ ràng (200 OK, 201 Created, 400 Bad Request, 404 Not Found, 409 Conflict).

                ## 💡 Ví dụ thực tế
                > **Tình huống:** Người dùng gửi yêu cầu lấy danh sách khóa học.  
                > **Thực thi:** Frontend gọi `GET /api/v1/courses`, Backend xác thực JWT token, truy vấn CSDL qua EF Core và trả về JSON chuẩn trong dưới 50ms.

                ## ⚠️ Cảnh báo & Lưu ý
                - Tuyệt đối không hardcode bí mật (API Key, JWT Secret) trong mã nguồn frontend.
                - Luôn kiểm tra tính hợp lệ của dữ liệu đầu vào (Input Validation) trước khi lưu vào CSDL.

                ## 🧠 Bài tập tự luyện
                1. Hãy giải thích sự khác biệt giữa HTTP GET và HTTP POST.  
                   *Gợi ý: GET dùng để đọc dữ liệu không tạo hiệu ứng phụ, POST dùng để tạo mới tài nguyên.*
                2. Hãy mô tả vòng đời của một HTTP Request từ trình duyệt đến CSDL.

                ## 📖 Nguồn tham khảo
                - [SRC-01] Tài liệu chính thức Microsoft .NET 10 Documentation
                - [SRC-02] React 19 Architecture Guide
                """);
            }

            // 3. lessons/02_bai_hoc_api.md
            var l2Entry = archive.CreateEntry("lessons/02_bai_hoc_api.md", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(l2Entry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("""
                # 📚 Bài học: Thiết kế RESTful API & Cơ sở Dữ liệu

                ## 🎯 Mục tiêu
                - Thiết kế schema CSDL chuẩn hóa (Normalized Database Schema).
                - Xây dựng các Endpoint CRUD an toàn với Optimistic Concurrency Control (OCC).

                ## 📖 Khái niệm cốt lõi
                ### 1. Quản lý Đồng thời Lạc quan (OCC)
                OCC sử dụng trường `Version` để bảo vệ dữ liệu chống mất mát khi nhiều người dùng cùng ghi đồng thời, trả về HTTP 409 Conflict để tách nhánh dữ liệu an toàn.

                ## 🧠 Bài tập tự luyện
                1. Viết một lệnh migration trong EF Core và giải thích ý nghĩa.
                """);
            }

            // 4. lessons/03_bai_hoc_react.md
            var l3Entry = archive.CreateEntry("lessons/03_bai_hoc_react.md", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(l3Entry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("""
                # 📚 Bài học: Xây dựng Giao diện React & Quản trị Trạng thái

                ## 🎯 Mục tiêu
                - Tối ưu hóa render bằng React 19 Server Components và Zustand Store.
                - Thiết kế giao diện Bento Grid đáp ứng chuẩn WCAG AA.
                """);
            }

            // 5. flashcards/deck-sample.yaml
            var fcEntry = archive.CreateEntry("flashcards/deck-sample.yaml", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(fcEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("""
                deck_id: "deck-swm-sample-01"
                subject: "CongNgheThongTin"
                description: "Bộ thẻ Flashcard chuẩn hóa ôn tập FSRS v5"
                cards:
                  - id: "fc-swm-001"
                    concept: "Kiến trúc RESTful API"
                    front: "Mã trạng thái HTTP nào được trả về khi có xung đột dữ liệu đồng thời (Optimistic Concurrency Control)?"
                    back: "HTTP 409 Conflict - Báo hiệu tài nguyên đã bị luồng khác sửa đổi kể từ khi phiên bản hiện tại được tải."
                    tags: ["api", "rest", "concurrency"]
                  - id: "fc-swm-002"
                    concept: "Thuật toán FSRS v5"
                    front: "Thuật toán FSRS v5 tính toán thời điểm ôn tập tiếp theo dựa trên những yếu tố cốt lõi nào?"
                    back: "Độ ổn định của trí nhớ (Stability - S) và Độ khó của khái niệm (Difficulty - D) qua 4 mức đánh giá (Again, Hard, Good, Easy)."
                    tags: ["fsrs", "memory", "spaced-repetition"]
                  - id: "fc-swm-003"
                    concept: "React 19 Server Components"
                    front: "Lợi ích nổi bật nhất của Server Components trong React 19 là gì?"
                    back: "Giảm kích thước bundle tải về máy khách, cho phép truy cập trực tiếp tài nguyên backend mà không cần qua tầng API trung gian."
                    tags: ["react", "frontend"]
                """);
            }

            // 6. quizzes/quiz-sample.md
            var qEntry = archive.CreateEntry("quizzes/quiz-sample.md", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(qEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("""
                # 📝 Bài trắc nghiệm: Kiểm tra Đánh giá Kiến thức Nền tảng

                ## Câu hỏi trắc nghiệm
                *Chọn đáp án đúng nhất.*

                **Câu 1:** Đâu là mã trạng thái chuẩn khi một tài nguyên được tạo mới thành công qua phương thức HTTP POST?
                - A. 200 OK
                - B. 201 Created
                - C. 204 No Content
                - D. 302 Found

                > [!TIP]
                > **Đáp án:** B  
                > **Giải thích:** Mã 201 Created là chuẩn RESTful biểu thị tài nguyên mới đã được tạo lập thành công trên máy chủ.

                **Câu 2:** Thuật toán FSRS viết tắt của cụm từ nào sau đây?
                - A. Fast System Recovery Scheduler
                - B. Free Spaced Repetition Scheduler
                - C. Flexible State Retention Standard
                - D. Formative Study Review Sequence

                > [!TIP]
                > **Đáp án:** B  
                > **Giải thích:** FSRS là Free Spaced Repetition Scheduler, thuật toán lặp lại ngắt quãng hiện đại tối ưu hóa theo khoa học trí nhớ.

                **Câu 3:** Khung nhìn chia đôi (Split Workspace) trong Studxy áp dụng tỉ lệ chuẩn nào giữa Sơ đồ, Trình đọc và Sổ tay?
                - A. 33 / 33 / 33
                - B. 15 / 45 / 40
                - C. 20 / 50 / 30
                - D. 10 / 60 / 30

                > [!TIP]
                > **Đáp án:** B  
                > **Giải thích:** Tỉ lệ công thái học 15/45/40 phân bổ tối ưu giữa Mục lục (15%), Khung đọc tài liệu (45%) và Sổ tay ghi chép (40%).
                """);
            }
        }

        return ms.ToArray();
    }

    // ========================================================================
    // 2b. FULL COURSE SWM ZIP EXPORT
    // ========================================================================
    public static async Task<byte[]> ExportCourseToZipAsync(SwmsDbContext db, Guid courseId)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null)
            throw new KeyNotFoundException($"Course {courseId} not found");

        var chapters = await db.Chapters
            .Where(ch => ch.CourseId == courseId)
            .OrderBy(ch => ch.SortOrder)
            .ToListAsync();

        var chapterIds = chapters.Select(ch => ch.Id).ToHashSet();
        var lessons = await db.Lessons
            .Where(l => chapterIds.Contains(l.ChapterId))
            .OrderBy(l => l.SortOrder)
            .ToListAsync();
        var lessonIds = lessons.Select(l => l.Id).ToHashSet();

        // Find flashcards linked to this course's lessons or title
        var flashcards = await db.Flashcards
            .Where(f => !f.IsArchived && (
                (f.LessonId.HasValue && lessonIds.Contains(f.LessonId.Value)) ||
                f.SourceTitleSnapshot == course.Title))
            .ToListAsync();

        // Find quizzes linked to this course's lessons
        var quizzes = await db.PracticeQuizzes
            .Where(q => q.LessonId.HasValue && lessonIds.Contains(q.LessonId.Value))
            .ToListAsync();
        var quizIds = quizzes.Select(q => q.Id).ToHashSet();
        var rawQuizItems = await db.PracticeQuizItems
            .Where(qi => quizIds.Contains(qi.QuizId))
            .ToListAsync();
        var quizItems = rawQuizItems
            .OrderBy(qi => qi.CreatedAt)
            .ToList();

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            // 1. manifest.yaml
            var manifestEntry = archive.CreateEntry("manifest.yaml", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(manifestEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("schema_version: 2.0.0");
                writer.WriteLine($"package_id: \"package:swm:{course.Slug}\"");
                writer.WriteLine($"title: \"{course.Title.Replace("\"", "\\\"")}\"");
                writer.WriteLine($"slug: \"{course.Slug}\"");
                if (!string.IsNullOrWhiteSpace(course.Description))
                {
                    writer.WriteLine($"description: \"{course.Description.Replace("\"", "\\\"")}\"");
                }
                writer.WriteLine("subject: \"General\"");
                writer.WriteLine("version: 1.0.0");
                writer.WriteLine("chapters:");

                int globalLessonIdx = 1;
                foreach (var ch in chapters)
                {
                    writer.WriteLine($"  - title: \"{ch.Title.Replace("\"", "\\\"")}\"");
                    writer.WriteLine($"    sort_order: {ch.SortOrder}");
                    writer.WriteLine("    lessons:");

                    var chLessons = lessons.Where(l => l.ChapterId == ch.Id).OrderBy(l => l.SortOrder).ToList();
                    foreach (var les in chLessons)
                    {
                        var safeSlug = Regex.Replace(les.Slug, @"[^a-zA-Z0-9_-]", "_");
                        if (string.IsNullOrWhiteSpace(safeSlug)) safeSlug = $"les_{globalLessonIdx}";
                        var relPath = $"lessons/{globalLessonIdx:D2}_{safeSlug}.md";

                        writer.WriteLine($"      - title: \"{les.Title.Replace("\"", "\\\"")}\"");
                        writer.WriteLine($"        slug: \"{les.Slug}\"");
                        writer.WriteLine($"        sort_order: {les.SortOrder}");
                        writer.WriteLine($"        file: \"{relPath}\"");
                        if (!string.IsNullOrWhiteSpace(les.VideoUrl))
                        {
                            writer.WriteLine($"        video_url: \"{les.VideoUrl.Replace("\"", "\\\"")}\"");
                        }
                        writer.WriteLine($"        estimated_minutes: {les.EstimatedMinutes}");

                        globalLessonIdx++;
                    }
                }
            }

            // 2. Lesson markdown files
            int fileLessonIdx = 1;
            foreach (var ch in chapters)
            {
                var chLessons = lessons.Where(l => l.ChapterId == ch.Id).OrderBy(l => l.SortOrder).ToList();
                foreach (var les in chLessons)
                {
                    var safeSlug = Regex.Replace(les.Slug, @"[^a-zA-Z0-9_-]", "_");
                    if (string.IsNullOrWhiteSpace(safeSlug)) safeSlug = $"les_{fileLessonIdx}";
                    var relPath = $"lessons/{fileLessonIdx:D2}_{safeSlug}.md";

                    var lesEntry = archive.CreateEntry(relPath, CompressionLevel.Optimal);
                    using (var writer = new StreamWriter(lesEntry.Open(), Encoding.UTF8))
                    {
                        var md = !string.IsNullOrWhiteSpace(les.ContentMarkdown)
                            ? les.ContentMarkdown
                            : (!string.IsNullOrWhiteSpace(les.PlainText)
                                ? (les.PlainText.StartsWith("#") ? les.PlainText : $"# {les.Title}\n\n{les.PlainText}")
                                : $"# {les.Title}\n\nNội dung bài học.");
                        writer.WriteLine(md);
                    }

                    fileLessonIdx++;
                }
            }

            // 3. flashcards/deck.yaml
            if (flashcards.Count > 0)
            {
                var fcEntry = archive.CreateEntry("flashcards/deck.yaml", CompressionLevel.Optimal);
                using (var writer = new StreamWriter(fcEntry.Open(), Encoding.UTF8))
                {
                    writer.WriteLine($"deck_id: \"deck-{course.Slug}\"");
                    writer.WriteLine("subject: \"General\"");
                    writer.WriteLine($"description: \"Bộ thẻ ghi nhớ FSRS v5 cho khóa học {course.Title.Replace("\"", "\\\"")}\"");
                    writer.WriteLine("cards:");

                    foreach (var fc in flashcards)
                    {
                        var front = ExtractTextFromAstOrJsonString(fc.FrontAstJson);
                        var back = ExtractTextFromAstOrJsonString(fc.BackAstJson);
                        var concept = string.IsNullOrWhiteSpace(fc.SourceQuoteSnapshot) ? "Khái niệm trọng tâm" : fc.SourceQuoteSnapshot;

                        writer.WriteLine($"  - id: \"fc-{fc.Id:N}\"");
                        writer.WriteLine($"    concept: \"{concept.Replace("\"", "\\\"")}\"");
                        writer.WriteLine($"    front: \"{front.Replace("\"", "\\\"").Replace("\r\n", " ").Replace("\n", " ")}\"");
                        writer.WriteLine($"    back: \"{back.Replace("\"", "\\\"").Replace("\r\n", " ").Replace("\n", " ")}\"");
                        writer.WriteLine("    tags: [\"study\", \"swm\"]");
                    }
                }
            }

            // 4. quizzes/quiz.md
            if (quizItems.Count > 0)
            {
                var qEntry = archive.CreateEntry("quizzes/quiz.md", CompressionLevel.Optimal);
                using (var writer = new StreamWriter(qEntry.Open(), Encoding.UTF8))
                {
                    writer.WriteLine($"# 📝 Bài trắc nghiệm: {course.Title}");
                    writer.WriteLine();
                    writer.WriteLine("## Câu hỏi trắc nghiệm");
                    writer.WriteLine("*Chọn đáp án đúng nhất.*");
                    writer.WriteLine();

                    int qNum = 1;
                    foreach (var item in quizItems)
                    {
                        var questionText = ExtractQuestionFromPrompt(item.PromptAstJson);
                        writer.WriteLine($"**Câu {qNum}:** {questionText}");
                        if (!string.IsNullOrWhiteSpace(item.ChoicesJson))
                        {
                            try
                            {
                                var choices = JsonSerializer.Deserialize<List<string>>(item.ChoicesJson);
                                if (choices != null)
                                {
                                    char opt = 'A';
                                    foreach (var c in choices)
                                    {
                                        writer.WriteLine($"- {opt}. {c}");
                                        opt++;
                                    }
                                }
                            }
                            catch { }
                        }

                        if (!string.IsNullOrWhiteSpace(item.ClozeSolution))
                        {
                            writer.WriteLine();
                            writer.WriteLine("> [!TIP]");
                            writer.WriteLine($"> **Đáp án:** {item.ClozeSolution}");
                        }
                        writer.WriteLine();
                        qNum++;
                    }
                }
            }
        }

        return ms.ToArray();
    }

    private static string ExtractTextFromAstOrJsonString(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("text", out var t))
                return t.GetString() ?? json;
        }
        catch { }
        return json;
    }

    private static string ExtractQuestionFromPrompt(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("question", out var q))
                return q.GetString() ?? json;
        }
        catch { }
        return json;
    }

    // ========================================================================
    // 3. SWM ZIP PACKAGE IMPORT (.zip -> Course, Chapters, Lessons, Cards, Quiz)
    // ========================================================================
    public static async Task<object> ImportCourseFromZipAsync(
        SwmsDbContext db,
        Stream zipStream,
        Guid userId)
    {
        using var ms = new MemoryStream();
        await zipStream.CopyToAsync(ms);
        ms.Position = 0;

        using var archive = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: false);
        var entries = archive.Entries
            .Where(e => !string.IsNullOrEmpty(e.Name))
            .GroupBy(e => e.FullName.Replace('\\', '/'), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Find manifest entry
        ZipArchiveEntry? manifestEntry = entries.Values.FirstOrDefault(e =>
            string.Equals(e.Name, "manifest.yaml", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.Name, "manifest.yml", StringComparison.OrdinalIgnoreCase) ||
            e.Name.StartsWith("manifest", StringComparison.OrdinalIgnoreCase) && (e.Name.EndsWith(".yaml") || e.Name.EndsWith(".yml")));

        string courseTitle = "Khóa học SWM Đã Nhập";
        string courseDescription = "Khóa học được nhập tự động từ gói lưu trữ chuẩn SWM.";
        string courseSlug = $"swm-{Guid.NewGuid():N}";

        var chapterPlans = new List<WizardChapterPreviewDto>();

        if (manifestEntry != null)
        {
            using var reader = new StreamReader(manifestEntry.Open(), Encoding.UTF8);
            var manifestText = await reader.ReadToEndAsync();
            ParseManifestYaml(manifestText, out courseTitle, out courseDescription, out courseSlug, chapterPlans);
        }

        // If no chapters from manifest, scan lesson files
        var lessonEntries = entries.Values
            .Where(e =>
            {
                var norm = e.FullName.Replace('\\', '/');
                return (norm.StartsWith("lessons/", StringComparison.OrdinalIgnoreCase) ||
                        norm.StartsWith("notes/", StringComparison.OrdinalIgnoreCase) ||
                        Regex.IsMatch(e.Name, @"^\d+.*\.md$")) &&
                       e.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase) &&
                       !norm.Contains("quality/", StringComparison.OrdinalIgnoreCase) &&
                       !norm.Contains("templates/", StringComparison.OrdinalIgnoreCase) &&
                       !norm.Contains("answer-keys/", StringComparison.OrdinalIgnoreCase) &&
                       !norm.Contains("quizzes/", StringComparison.OrdinalIgnoreCase) &&
                       !norm.Contains("assessments/", StringComparison.OrdinalIgnoreCase);
            })
            .OrderBy(e => e.FullName.Replace('\\', '/'))
            .ToList();

        if (chapterPlans.Count == 0 && lessonEntries.Count > 0)
        {
            var groupedByFolder = lessonEntries.GroupBy(le =>
            {
                var norm = le.FullName.Replace('\\', '/').Trim('/');
                var parts = norm.Split('/');
                if (parts.Length > 2 && parts[0].Equals("notes", StringComparison.OrdinalIgnoreCase))
                {
                    return parts[1]; // e.g. "module-1"
                }
                if (parts.Length > 1)
                {
                    return parts[^2];
                }
                return "root";
            }).ToList();

            int chNum = 1;
            foreach (var group in groupedByFolder)
            {
                string chTitle;
                if (group.Key.StartsWith("module-", StringComparison.OrdinalIgnoreCase))
                {
                    chTitle = $"Chương {chNum}: Module {group.Key[7..].ToUpperInvariant()}";
                }
                else if (group.Key != "root" && group.Key != "lessons" && group.Key != "notes")
                {
                    var cleanKey = Regex.Replace(group.Key, @"^(?:Chương|Chapter|Phần)\s*\d+\s*[:.\-–—]\s*", "", RegexOptions.IgnoreCase).Trim();
                    chTitle = $"Chương {chNum}: {cleanKey}";
                }
                else
                {
                    chTitle = $"Chương {chNum}: Nội dung Khóa học";
                }

                var chDto = new WizardChapterPreviewDto(chTitle, chNum++, new List<WizardLessonPreviewDto>());
                foreach (var le in group)
                {
                    using var r = new StreamReader(le.Open(), Encoding.UTF8);
                    var md = await r.ReadToEndAsync();
                    var title = ExtractFirstHeading(md) ?? Path.GetFileNameWithoutExtension(le.Name);
                    chDto.Lessons.Add(new WizardLessonPreviewDto(title, "Bài học từ gói SWM", 25, le.FullName.Replace('\\', '/')));
                }
                chapterPlans.Add(chDto);
            }
        }
        else if (chapterPlans.Count == 0)
        {
            chapterPlans.Add(new WizardChapterPreviewDto(
                "Chương 1: Tổng quan Khóa học",
                1,
                new List<WizardLessonPreviewDto>
                {
                    new("Bài 1: Giới thiệu & Khởi động", "Bài học khởi động của khóa học", 15)
                }
            ));
        }

        var now = DateTimeOffset.UtcNow;

        // Ensure unique course slug even if the same manifest package is imported multiple times
        var normalizedBaseSlug = string.IsNullOrWhiteSpace(courseSlug)
            ? $"swm-{Guid.NewGuid():N}"
            : Truncate(courseSlug.Trim(), 220);
        if (await db.Courses.AnyAsync(c => c.Slug == normalizedBaseSlug))
        {
            courseSlug = Truncate($"{normalizedBaseSlug}-{Guid.NewGuid():N}"[..Math.Min(245, normalizedBaseSlug.Length + 9)], 245);
            while (await db.Courses.AnyAsync(c => c.Slug == courseSlug))
            {
                courseSlug = $"swm-{Guid.NewGuid():N}";
            }
        }
        else
        {
            courseSlug = normalizedBaseSlug;
        }

        // 1. Create Course Entity
        var course = new Course
        {
            Id = Guid.NewGuid(),
            InstructorId = userId,
            Title = Truncate(courseTitle, 245),
            Slug = courseSlug,
            Description = Truncate(courseDescription, 500),
            Visibility = "personal",
            IsPublished = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Courses.Add(course);

        // Enrollment
        db.CourseEnrollments.Add(new CourseEnrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = course.Id,
            Role = "student",
            EnrolledAt = now
        });

        // 2. Create Chapters & Lessons
        var maxSortOrder = await db.Lessons.Select(l => (int?)l.SortOrder).MaxAsync() ?? 2;
        var baseLessonSort = Math.Max(10, maxSortOrder + 1);

        var createdLessons = new List<Lesson>();
        int globalLessonIdx = 0;

        for (int cIdx = 0; cIdx < chapterPlans.Count; cIdx++)
        {
            var chPlan = chapterPlans[cIdx];
            var cleanTitle = Regex.Replace(chPlan.Title, @"^(?:Chương|Chapter|Phần)\s*\d+\s*[:.\-–—]\s*", "", RegexOptions.IgnoreCase).Trim();
            var chapter = new Chapter
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                Title = Truncate($"Chương {cIdx + 1}: {cleanTitle}", 245),
                SortOrder = cIdx + 1,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Chapters.Add(chapter);

            foreach (var lPlan in chPlan.Lessons)
            {
                // Find matching file in zip if any
                string lessonMd = string.Empty;
                ZipArchiveEntry? matchedEntry = null;

                if (!string.IsNullOrWhiteSpace(lPlan.FilePath))
                {
                    var cleanPath = lPlan.FilePath.Replace('\\', '/');
                    entries.TryGetValue(cleanPath, out matchedEntry);
                }

                if (matchedEntry == null)
                {
                    matchedEntry = entries.Values.FirstOrDefault(e =>
                        e.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase) &&
                        (e.FullName.Equals(lPlan.FilePath, StringComparison.OrdinalIgnoreCase) ||
                         e.Name.Contains(lPlan.Title, StringComparison.OrdinalIgnoreCase) ||
                         lPlan.Title.Contains(Path.GetFileNameWithoutExtension(e.Name), StringComparison.OrdinalIgnoreCase)));
                }

                if (matchedEntry != null)
                {
                    using var r = new StreamReader(matchedEntry.Open(), Encoding.UTF8);
                    lessonMd = await r.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(lessonMd))
                {
                    lessonMd = $"""
                    # 📚 Bài học: {lPlan.Title}

                    ## 🎯 Mục tiêu
                    - {lPlan.Summary}

                    ## 📖 Khái niệm cốt lõi
                    Nội dung bài giảng đang được hoàn thiện. Bạn có thể sử dụng Sổ tay Split Workspace để ghi chép và bổ sung thêm tài liệu tham khảo.

                    ## 💡 Ví dụ & Thực hành
                    > Thực hành các ví dụ và khái niệm trọng tâm đã nêu.
                    """;
                }

                var (astJson, plainText) = BuildMarkdownAstAndPlainText(course.Id, lPlan.Title, lessonMd);

                var lesson = new Lesson
                {
                    Id = Guid.NewGuid(),
                    ChapterId = chapter.Id,
                    Title = Truncate(lPlan.Title, 245),
                    Slug = $"swm-{course.Id.ToString("N")[..8]}-les-{globalLessonIdx + 1}",
                    SortOrder = baseLessonSort + globalLessonIdx,
                    CurrentVersion = 1,
                    ContentMarkdown = lessonMd,
                    VideoUrl = lPlan.VideoUrl,
                    EstimatedMinutes = lPlan.EstimatedMinutes > 0 ? lPlan.EstimatedMinutes : 5,
                    ContentAstJson = astJson,
                    PlainText = plainText,
                    IsPublished = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.Lessons.Add(lesson);
                createdLessons.Add(lesson);

                db.LessonVersions.Add(new LessonVersion
                {
                    Id = Guid.NewGuid(),
                    LessonId = lesson.Id,
                    VersionNumber = 1,
                    ContentAstJson = astJson,
                    PlainText = plainText,
                    PublishedBy = userId,
                    PublishedAt = now,
                    ChangeSummary = $"Nhập bài học từ gói chuẩn SWM \"{lPlan.Title}\""
                });

                globalLessonIdx++;
            }
        }

        if (createdLessons.Count == 0)
        {
            var fallbackChapter = new Chapter
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                Title = "Chương 1: Tổng quan Khóa học",
                SortOrder = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Chapters.Add(fallbackChapter);

            var fallbackLesson = new Lesson
            {
                Id = Guid.NewGuid(),
                ChapterId = fallbackChapter.Id,
                Title = "Bài 1: Giới thiệu Khóa học",
                Slug = $"swm-{course.Id.ToString("N")[..8]}-les-1",
                SortOrder = baseLessonSort,
                CurrentVersion = 1,
                ContentAstJson = JsonSerializer.Serialize(new { type = "doc", content = new[] { new { type = "paragraph", content = new[] { new { type = "text", text = "Chào mừng bạn đến với khóa học." } } } } }),
                PlainText = "Chào mừng bạn đến với khóa học.",
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Lessons.Add(fallbackLesson);
            createdLessons.Add(fallbackLesson);
        }

        // 3. Import Flashcards
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = Truncate($"KH-SWM-{course.Id.ToString("N")[..6]}", 45),
            ColorKey = "emerald",
            CreatedAt = now
        };
        db.Tags.Add(tag);

        var flashcardEntries = entries.Values.Where(e =>
        {
            var norm = e.FullName.Replace('\\', '/');
            return (norm.StartsWith("flashcards/", StringComparison.OrdinalIgnoreCase) ||
                    e.Name.Contains("flashcard", StringComparison.OrdinalIgnoreCase) ||
                    e.Name.Contains("deck", StringComparison.OrdinalIgnoreCase)) &&
                   (e.Name.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || e.Name.EndsWith(".yml", StringComparison.OrdinalIgnoreCase));
        }).ToList();

        var importedCards = new List<Flashcard>();
        foreach (var fe in flashcardEntries)
        {
            using var r = new StreamReader(fe.Open(), Encoding.UTF8);
            var yaml = await r.ReadToEndAsync();
            var parsedCards = ParseFlashcardsYaml(yaml);

            foreach (var pc in parsedCards)
            {
                var targetLesson = createdLessons.FirstOrDefault();
                var card = new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = targetLesson?.Id,
                    DocumentId = null,
                    AnnotationId = null,
                    CardType = "ai_generated",
                    SourceTitleSnapshot = course.Title,
                    SourceQuoteSnapshot = pc.concept,
                    FrontAstJson = JsonSerializer.Serialize(new { text = pc.front }),
                    BackAstJson = JsonSerializer.Serialize(new { text = pc.back }),
                    Stability = 0.0,
                    Difficulty = 0.0,
                    Reps = 0,
                    Lapses = 0,
                    State = 0,
                    IsArchived = false,
                    DueAt = now.AddMinutes(5),
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.Flashcards.Add(card);
                importedCards.Add(card);

                db.FlashcardTags.Add(new FlashcardTag
                {
                    FlashcardId = card.Id,
                    TagId = tag.Id,
                    CreatedAt = now
                });
            }
        }

        // If no flashcards in zip, generate default 3 cards
        if (importedCards.Count == 0 && createdLessons.Count > 0)
        {
            for (int i = 0; i < Math.Min(3, createdLessons.Count); i++)
            {
                var l = createdLessons[i];
                var card = new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = l.Id,
                    CardType = "ai_generated",
                    SourceTitleSnapshot = course.Title,
                    SourceQuoteSnapshot = l.Title,
                    FrontAstJson = JsonSerializer.Serialize(new { text = $"Mục tiêu trọng tâm của bài \"{l.Title}\" là gì?" }),
                    BackAstJson = JsonSerializer.Serialize(new { text = $"Nắm vững các khái niệm và kỹ năng cốt lõi được trình bày trong bài \"{l.Title}\"." }),
                    Stability = 0.0,
                    Difficulty = 0.0,
                    Reps = 0,
                    Lapses = 0,
                    State = 0,
                    DueAt = now.AddMinutes(5),
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.Flashcards.Add(card);
                db.FlashcardTags.Add(new FlashcardTag { FlashcardId = card.Id, TagId = tag.Id, CreatedAt = now });
            }
        }

        // 4. Import Practice Quizzes
        var quizEntries = entries.Values.Where(e =>
        {
            var norm = e.FullName.Replace('\\', '/');
            return (norm.StartsWith("quizzes/", StringComparison.OrdinalIgnoreCase) ||
                    norm.StartsWith("assessments/quizzes/", StringComparison.OrdinalIgnoreCase) ||
                    norm.Contains("quiz", StringComparison.OrdinalIgnoreCase)) &&
                   e.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
        }).OrderBy(e => e.FullName.Replace('\\', '/')).ToList();

        var answerKeyEntries = entries.Values.Where(e =>
        {
            var norm = e.FullName.Replace('\\', '/');
            return (norm.Contains("answer-key", StringComparison.OrdinalIgnoreCase) ||
                    norm.Contains("answer", StringComparison.OrdinalIgnoreCase)) &&
                   e.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
        }).ToDictionary(e => Path.GetFileNameWithoutExtension(e.Name), e => e, StringComparer.OrdinalIgnoreCase);

        var firstLesson = createdLessons.FirstOrDefault();
        if (firstLesson != null)
        {
            var quiz = new PracticeQuiz
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = Truncate($"Đề trắc nghiệm: {course.Title}", 240),
                LessonId = firstLesson.Id,
                TotalItems = 3,
                CorrectItems = 0,
                Status = "in_progress",
                StartedAt = now
            };
            db.PracticeQuizzes.Add(quiz);

            var quizItems = new List<PracticeQuizItem>();
            if (quizEntries.Count > 0)
            {
                var qEntry = quizEntries.First();
                using var r = new StreamReader(qEntry.Open(), Encoding.UTF8);
                var qMd = await r.ReadToEndAsync();

                string? akMd = null;
                var baseName = Path.GetFileNameWithoutExtension(qEntry.Name).Replace("-quiz", string.Empty);
                var matchingAk = answerKeyEntries.Values.FirstOrDefault(ak => ak.Name.Contains(baseName, StringComparison.OrdinalIgnoreCase));
                if (matchingAk != null)
                {
                    using var akReader = new StreamReader(matchingAk.Open(), Encoding.UTF8);
                    akMd = await akReader.ReadToEndAsync();
                }

                quizItems = ParseQuizMarkdown(quiz.Id, qMd, akMd, now);
            }

            if (quizItems.Count == 0)
            {
                quizItems.Add(new PracticeQuizItem
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    ItemType = "multiple_choice",
                    PromptAstJson = JsonSerializer.Serialize(new { question = $"Khóa học \"{course.Title}\" được cấu trúc theo chuẩn nào?" }),
                    ChoicesJson = JsonSerializer.Serialize(new[]
                    {
                        new { key = "A", text = "Chuẩn Kiến trúc SWM với FSRS v5 & Split PKM" },
                        new { key = "B", text = "Tài liệu tĩnh không có bài tập" },
                        new { key = "C", text = "Chỉ bao gồm video" },
                        new { key = "D", text = "Chỉ gồm đề thi" }
                    }),
                    ClozeSolution = "A",
                    CreatedAt = now
                });
            }

            db.PracticeQuizItems.AddRange(quizItems);
            quiz.TotalItems = quizItems.Count;
        }

        // 5. Create Course Summary Notebook
        var nbAst = new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 1, block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = $"Sổ tay Khóa học: {course.Title}" } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = $"Lộ trình bài giảng: {string.Join(" • ", createdLessons.Select(l => $"[[{l.Title}]]"))}"
                        }
                    }
                }
            }
        };

        var nb = new Notebook
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = Truncate($"Sổ tay: {course.Title}", 240),
            PrimarySourceLessonId = firstLesson?.Id,
            ContentAstJson = JsonSerializer.Serialize(nbAst),
            Version = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Notebooks.Add(nb);

        await db.SaveChangesAsync();

        return await CourseGenerationService.BuildCourseSummaryAsync(db, course);
    }

    // ========================================================================
    // 4. AI WIZARD GENERATION (CLARIFY, PREVIEW, COMMIT)
    // ========================================================================
    public static async Task<WizardClarifyResponse> GenerateClarifyQuestionsAsync(
        GeminiProxyClient gemini,
        string topic,
        AiCallOverrides? aiOverrides = null)
    {
        var cleanTopic = (topic ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(cleanTopic)) cleanTopic = "Kiến thức Tổng quan";

        // Structured deterministic questions ensuring reliable high-yield UX
        var questions = new List<WizardClarifyQuestion>
        {
            new(
                "level",
                $"Trình độ hiện tại của bạn trong chủ đề \"{cleanTopic}\" là gì?",
                new List<string>
                {
                    "Mới bắt đầu (Chưa có nền tảng, cần tiếp cận từ số 0)",
                    "Đã có kiến thức cơ bản (Muốn hệ thống hóa kiến thức vững chắc)",
                    "Trung cấp / Đã có kinh nghiệm (Muốn nâng cao thực chiến và chuyên sâu)",
                    "Nâng cao / Chuẩn bị phỏng vấn hoặc chứng chỉ chuyên môn"
                }
            ),
            new(
                "duration",
                "Thời lượng và quy mô khóa học mong muốn?",
                new List<string>
                {
                    "Tinh gọn cấp tốc (3 - 4 bài trọng tâm, hoàn thành trong 1 tuần)",
                    "Tiêu chuẩn toàn diện (6 - 8 bài học kèm thực hành, 2 - 3 tuần)",
                    "Chuyên sâu hoàn chỉnh (10 - 14 bài với đề thi và dự án, 1 - 2 tháng)"
                }
            ),
            new(
                "focus",
                "Phong cách và phương pháp học bạn muốn ưu tiên?",
                new List<string>
                {
                    "Thực hành thực chiến & Bài tập tình huống (Hands-on)",
                    "Bản chất lý thuyết & Tư duy nền tảng sâu (First Principles)",
                    "Cân bằng 50% Lý thuyết + 50% Bài tập & Thẻ nhớ FSRS"
                }
            )
        };

        if (!string.IsNullOrWhiteSpace(aiOverrides?.ApiKey))
        {
            try
            {
                var prompt = $$"""
                Chủ đề khóa học: "{{cleanTopic}}".
                Hãy tạo 3 câu hỏi trắc nghiệm tiếng Việt (mỗi câu 3-4 lựa chọn) để làm rõ nhu cầu học tập của người học về trình độ, thời lượng và định hướng học.
                Trả về JSON định dạng:
                [
                  { "id": "level", "question": "...", "options": ["...", "..."] },
                  { "id": "duration", "question": "...", "options": ["...", "..."] },
                  { "id": "focus", "question": "...", "options": ["...", "..."] }
                ]
                """;
                var aiRes = await gemini.CompleteChatAsync(
                    "Bạn là chuyên gia thiết kế lộ trình học tập đại học. Trả về đúng JSON yêu cầu, không kèm giải thích thừa.",
                    prompt,
                    CancellationToken.None,
                    aiOverrides);

                var match = Regex.Match(aiRes.Content ?? string.Empty, @"\[\s*\{.*\}\s*\]", RegexOptions.Singleline);
                if (match.Success)
                {
                    var parsed = JsonSerializer.Deserialize<List<WizardClarifyQuestion>>(match.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (parsed != null && parsed.Count >= 3)
                    {
                        questions = parsed.Take(3).ToList();
                    }
                }
            }
            catch
            {
                // Fallback gracefully to high quality deterministic questions
            }
        }

        return new WizardClarifyResponse(cleanTopic, questions);
    }

    public static async Task<WizardPreviewResponse> GenerateSyllabusPreviewAsync(
        GeminiProxyClient gemini,
        string topic,
        Dictionary<string, string> answers,
        AiCallOverrides? aiOverrides = null)
    {
        var cleanTopic = (topic ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(cleanTopic)) cleanTopic = "Khóa học Chuyên đề";

        var courseTitle = $"Khóa học: {cleanTopic}";
        var courseDescription = $"Lộ trình học tập cá nhân hóa về {cleanTopic} được thiết kế tối ưu với phương pháp đọc sâu và thuật toán FSRS v5.";

        var chapters = new List<WizardChapterPreviewDto>();

        // Default 2 chapters with 4 lessons
        chapters.Add(new WizardChapterPreviewDto(
            $"Nền tảng & Khái niệm Cốt lõi của {cleanTopic}",
            1,
            new List<WizardLessonPreviewDto>
            {
                new($"Bài 1: Giới thiệu & Tổng quan về {cleanTopic}", "Tìm hiểu bối cảnh, vai trò và mục tiêu cốt lõi của chủ đề.", 20),
                new($"Bài 2: Các nguyên lý và thuật ngữ trọng tâm", "Nắm vững các thuật ngữ nền tảng và cách áp dụng chuẩn xác.", 25)
            }
        ));

        chapters.Add(new WizardChapterPreviewDto(
            $"Thực hành & Nâng cao Kỹ năng {cleanTopic}",
            2,
            new List<WizardLessonPreviewDto>
            {
                new($"Bài 3: Kỹ thuật phân tích & Quy trình thực thi", "Xây dựng các bước giải quyết vấn đề và quy trình làm việc chuẩn.", 30),
                new($"Bài 4: Tổng hợp kiến thức & Đánh giá thực chiến", "Luyện tập tình huống, câu hỏi phản xạ FSRS và bài kiểm tra định hình.", 35)
            }
        ));

        if (!string.IsNullOrWhiteSpace(aiOverrides?.ApiKey))
        {
            try
            {
                var answersSummary = string.Join(", ", answers.Select(kv => $"{kv.Key}: {kv.Value}"));
                var prompt = $$"""
                Chủ đề: "{{cleanTopic}}".
                Yêu cầu người học: {{answersSummary}}.
                Hãy sinh lộ trình gồm 2-3 chương, mỗi chương 2-3 bài học.
                Trả về JSON định dạng:
                {
                  "courseTitle": "...",
                  "description": "...",
                  "chapters": [
                    {
                      "title": "Chương 1: ...",
                      "sortOrder": 1,
                      "lessons": [
                        { "title": "Bài 1: ...", "summary": "...", "estimatedMinutes": 20 },
                        { "title": "Bài 2: ...", "summary": "...", "estimatedMinutes": 25 }
                      ]
                    }
                  ]
                }
                """;

                var aiRes = await gemini.CompleteChatAsync(
                    "Bạn là chuyên gia thiết kế chương trình đào tạo. Trả về đúng định dạng JSON.",
                    prompt,
                    CancellationToken.None,
                    aiOverrides);

                var match = Regex.Match(aiRes.Content ?? string.Empty, @"\{\s*""courseTitle"".*\}", RegexOptions.Singleline);
                if (match.Success)
                {
                    using var doc = JsonDocument.Parse(match.Value);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("courseTitle", out var ctProp)) courseTitle = ctProp.GetString() ?? courseTitle;
                    if (root.TryGetProperty("description", out var descProp)) courseDescription = descProp.GetString() ?? courseDescription;

                    if (root.TryGetProperty("chapters", out var chArray) && chArray.ValueKind == JsonValueKind.Array)
                    {
                        var aiChapters = new List<WizardChapterPreviewDto>();
                        int sOrder = 1;
                        foreach (var chElem in chArray.EnumerateArray())
                        {
                            var rawTitle = chElem.GetProperty("title").GetString() ?? $"Chương {sOrder}";
                            var chTitle = Regex.Replace(rawTitle, @"^(?:Chương|Chapter|Phần)\s*\d+\s*[:.\-–—]\s*", "", RegexOptions.IgnoreCase).Trim();
                            var chLessons = new List<WizardLessonPreviewDto>();
                            if (chElem.TryGetProperty("lessons", out var lesArray))
                            {
                                foreach (var lElem in lesArray.EnumerateArray())
                                {
                                    var lTitle = lElem.GetProperty("title").GetString() ?? "Bài học";
                                    var lSummary = lElem.TryGetProperty("summary", out var smProp) ? smProp.GetString() ?? "" : "";
                                    var estMin = lElem.TryGetProperty("estimatedMinutes", out var emProp) ? emProp.GetInt32() : 25;
                                    chLessons.Add(new WizardLessonPreviewDto(lTitle, lSummary, estMin));
                                }
                            }
                            aiChapters.Add(new WizardChapterPreviewDto(chTitle, sOrder++, chLessons));
                        }
                        if (aiChapters.Count > 0) chapters = aiChapters;
                    }
                }
            }
            catch
            {
                // Fallback gracefully to default robust syllabus
            }
        }

        return new WizardPreviewResponse(cleanTopic, courseTitle, courseDescription, chapters);
    }

    public static async Task<object> CommitWizardCourseAsync(
        SwmsDbContext db,
        GeminiProxyClient gemini,
        WizardCommitRequest req,
        Guid userId,
        AiCallOverrides? aiOverrides = null)
    {
        var now = DateTimeOffset.UtcNow;
        var courseTitle = !string.IsNullOrWhiteSpace(req.Title) ? req.Title.Trim() : $"Khóa học: {req.Topic}";
        var courseDesc = !string.IsNullOrWhiteSpace(req.Description) ? req.Description.Trim() : $"Khóa học {req.Topic} được tạo bằng Trợ lý AI.";
        var courseSlug = $"ai-wizard-{Guid.NewGuid():N}";

        // 1. Create Course
        var course = new Course
        {
            Id = Guid.NewGuid(),
            InstructorId = userId,
            Title = Truncate(courseTitle, 245),
            Slug = courseSlug,
            Description = Truncate(courseDesc, 500),
            Visibility = "personal",
            IsPublished = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Courses.Add(course);

        // Enrollment
        db.CourseEnrollments.Add(new CourseEnrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = course.Id,
            Role = "student",
            EnrolledAt = now
        });

        // 2. Chapters & Lessons
        var maxSortOrder = await db.Lessons.Select(l => (int?)l.SortOrder).MaxAsync() ?? 2;
        var baseLessonSort = Math.Max(10, maxSortOrder + 1);

        var createdLessons = new List<Lesson>();
        int lessonCounter = 0;

        for (int cIdx = 0; cIdx < req.Chapters.Count; cIdx++)
        {
            var chDto = req.Chapters[cIdx];
            var cleanTitle = Regex.Replace(chDto.Title, @"^(?:Chương|Chapter|Phần)\s*\d+\s*[:.\-–—]\s*", "", RegexOptions.IgnoreCase).Trim();
            var chapter = new Chapter
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                Title = Truncate($"Chương {cIdx + 1}: {cleanTitle}", 245),
                SortOrder = cIdx + 1,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Chapters.Add(chapter);

            foreach (var lDto in chDto.Lessons)
            {
                var lessonTitle = Truncate(lDto.Title, 245);
                var mdContent = $"""
                # 📚 Bài học: {lessonTitle}

                ## 🎯 Mục tiêu bài học
                - Hiểu rõ bản chất và phương pháp cốt lõi của **{lessonTitle}**.
                - Nắm vững quy trình vận dụng lý thuyết vào tình huống thực tế.
                - Rèn luyện phản xạ ôn tập chủ động với các câu hỏi kiểm tra FSRS v5.

                ## 📖 Khái niệm cốt lõi
                ### 1. Tổng quan vấn đề
                {lDto.Summary}

                ### 2. Nguyên lý hoạt động
                Các nguyên lý cơ bản chi phối chủ đề này cần được ghi nhớ và thực hành thường xuyên. Người học có thể sử dụng Sổ tay Split PKM bên cạnh để ghi chép các ý tưởng bổ sung và liên kết hai chiều [[Wikilink]].

                ## 💡 Ví dụ áp dụng thực tế
                > **Tình huống:** Vận dụng {lessonTitle} để giải quyết bài toán thực tiễn.  
                > **Giải pháp:** Phân rã bài toán thành các bước nhỏ, đo lường kết quả và liên tục cải tiến theo chu trình phản hồi.

                ## ⚠️ Những sai lầm phổ biến cần tránh
                - Học thuộc lòng máy móc mà không hiểu rõ nguyên lý nền tảng.
                - Bỏ qua các bước kiểm thử và xác minh tính đúng đắn.

                ## 🧠 Bài tập tự luyện
                1. Hãy tự tóm tắt lại nội dung trọng tâm của bài học trong 3 câu ngắn gọn.  
                2. Áp dụng kiến thức bài học vào một dự án cá nhân gần nhất của bạn.
                """;

                var (astJson, plainText) = BuildMarkdownAstAndPlainText(course.Id, lessonTitle, mdContent);

                var lesson = new Lesson
                {
                    Id = Guid.NewGuid(),
                    ChapterId = chapter.Id,
                    Title = lessonTitle,
                    Slug = $"ai-{course.Id.ToString("N")[..8]}-les-{lessonCounter + 1}",
                    SortOrder = baseLessonSort + lessonCounter,
                    CurrentVersion = 1,
                    ContentAstJson = astJson,
                    PlainText = plainText,
                    IsPublished = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.Lessons.Add(lesson);
                createdLessons.Add(lesson);

                db.LessonVersions.Add(new LessonVersion
                {
                    Id = Guid.NewGuid(),
                    LessonId = lesson.Id,
                    VersionNumber = 1,
                    ContentAstJson = astJson,
                    PlainText = plainText,
                    PublishedBy = userId,
                    PublishedAt = now,
                    ChangeSummary = $"Khởi tạo bài học bằng AI Wizard: \"{lessonTitle}\""
                });

                lessonCounter++;
            }
        }

        // 3. Flashcards (FSRS v5)
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = Truncate($"KH-AI-{course.Id.ToString("N")[..6]}", 45),
            ColorKey = "blue",
            CreatedAt = now
        };
        db.Tags.Add(tag);

        for (int i = 0; i < Math.Min(4, createdLessons.Count); i++)
        {
            var l = createdLessons[i];
            var card = new Flashcard
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = l.Id,
                CardType = "ai_generated",
                SourceTitleSnapshot = course.Title,
                SourceQuoteSnapshot = l.Title,
                FrontAstJson = JsonSerializer.Serialize(new { text = $"Khái niệm trọng tâm nào được trình bày trong \"{l.Title}\"?" }),
                BackAstJson = JsonSerializer.Serialize(new { text = $"Các nguyên lý nền tảng và phương pháp giải quyết vấn đề của \"{l.Title}\"." }),
                Stability = 0.0,
                Difficulty = 0.0,
                Reps = 0,
                Lapses = 0,
                State = 0,
                DueAt = now.AddMinutes(5),
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Flashcards.Add(card);
            db.FlashcardTags.Add(new FlashcardTag { FlashcardId = card.Id, TagId = tag.Id, CreatedAt = now });
        }

        // 4. Practice Quiz
        var firstLes = createdLessons.FirstOrDefault();
        if (firstLes != null)
        {
            var quiz = new PracticeQuiz
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = Truncate($"Đề kiểm tra: {course.Title}", 240),
                LessonId = firstLes.Id,
                TotalItems = 3,
                CorrectItems = 0,
                Status = "in_progress",
                StartedAt = now
            };
            db.PracticeQuizzes.Add(quiz);

            db.PracticeQuizItems.Add(new PracticeQuizItem
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                ItemType = "multiple_choice",
                PromptAstJson = JsonSerializer.Serialize(new { question = $"Mục tiêu hàng đầu của khóa học \"{course.Title}\" là gì?" }),
                ChoicesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "A", text = "Làm chủ kiến thức từ nền tảng đến ứng dụng thực tiễn" },
                    new { key = "B", text = "Chỉ học thuộc lý thuyết" },
                    new { key = "C", text = "Đọc tài liệu mà không cần ôn tập" },
                    new { key = "D", text = "Bỏ qua các bước làm bài tập" }
                }),
                ClozeSolution = "A",
                CreatedAt = now
            });

            db.PracticeQuizItems.Add(new PracticeQuizItem
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                ItemType = "multiple_choice",
                PromptAstJson = JsonSerializer.Serialize(new { question = "Để củng cố trí nhớ dài hạn hiệu quả nhất, người học nên làm gì?" }),
                ChoicesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "A", text = "Ôn tập ngắt quãng theo thuật toán FSRS v5" },
                    new { key = "B", text = "Chỉ đọc lại một lần trước ngày thi" },
                    new { key = "C", text = "Học nhồi nhét liên tục không nghỉ" },
                    new { key = "D", text = "Không ghi chép sổ tay" }
                }),
                ClozeSolution = "A",
                CreatedAt = now.AddSeconds(1)
            });

            db.PracticeQuizItems.Add(new PracticeQuizItem
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                ItemType = "essay",
                PromptAstJson = JsonSerializer.Serialize(new
                {
                    question = $"Hãy trình bày ngắn gọn phương án bạn sẽ áp dụng kiến thức của khóa học \"{course.Title}\" vào thực tế.",
                    rubricCriteria = new[]
                    {
                        "Xác định được mục tiêu cụ thể.",
                        "Lập kế hoạch hành động theo từng bước.",
                        "Có phương án đo lường và đánh giá kết quả."
                    }
                }),
                ClozeSolution = "Áp dụng có hệ thống từ phân tích, thiết kế, triển khai đến đo lường phản hồi.",
                CreatedAt = now.AddSeconds(2)
            });
        }

        // 5. Course Notebook
        var nbAst = new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 1, block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = $"Sổ tay Lộ trình: {course.Title}" } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = $"Danh sách bài giảng khóa học: {string.Join(" • ", createdLessons.Select(l => $"[[{l.Title}]]"))}"
                        }
                    }
                }
            }
        };

        var notebook = new Notebook
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = Truncate($"Sổ tay: {course.Title}", 240),
            PrimarySourceLessonId = firstLes?.Id,
            ContentAstJson = JsonSerializer.Serialize(nbAst),
            Version = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Notebooks.Add(notebook);

        await db.SaveChangesAsync();

        return await CourseGenerationService.BuildCourseSummaryAsync(db, course);
    }

    // ========================================================================
    // HELPER METHODS
    // ========================================================================
    public static (string AstJson, string PlainText) BuildMarkdownAstAndPlainText(
        Guid courseId,
        string lessonTitle,
        string markdown,
        string? sourceFolder = null,
        string? sourceRelativePath = null)
    {
        var cleanTitle = Truncate(lessonTitle, 200);
        var cleanMarkdown = Regex.Replace(markdown.Trim(), @"^---\r?\n[\s\S]*?\r?\n---\r?\n", string.Empty).Trim();
        var plainText = cleanMarkdown;

        var paragraphs = plainText
            .Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Take(300);

        var docContent = new List<object>
        {
            new
            {
                type = "heading",
                attrs = new { level = 1, block_id = Guid.NewGuid().ToString() },
                content = new[] { new { type = "text", text = cleanTitle } }
            }
        };

        foreach (var p in paragraphs)
        {
            var pTrim = p.Trim();
            if (pTrim.StartsWith("#"))
            {
                var hLevel = Math.Min(4, Math.Max(2, pTrim.TakeWhile(c => c == '#').Count()));
                var hText = pTrim.TrimStart('#').Trim();
                docContent.Add(new
                {
                    type = "heading",
                    attrs = new { level = hLevel, block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = hText } }
                });
            }
            else
            {
                docContent.Add(new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = pTrim } }
                });
            }
        }

        var attrs = new Dictionary<string, object?>
        {
            ["course_id"] = courseId,
            ["title"] = cleanTitle
        };
        if (!string.IsNullOrWhiteSpace(sourceFolder))
        {
            attrs["sourceFolder"] = sourceFolder;
        }
        if (!string.IsNullOrWhiteSpace(sourceRelativePath))
        {
            attrs["sourceRelativePath"] = sourceRelativePath.Replace('\\', '/');
        }

        var ast = new
        {
            type = "doc",
            attrs,
            content = docContent
        };

        return (JsonSerializer.Serialize(ast), plainText);
    }

    private static void ParseManifestYaml(
        string yaml,
        out string title,
        out string description,
        out string slug,
        List<WizardChapterPreviewDto> chapters)
    {
        title = "Khóa học SWM Đã Nhập";
        description = "Khóa học được nhập từ gói SWM chuẩn hóa.";
        slug = $"swm-{Guid.NewGuid():N}";

        var lines = yaml.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        WizardChapterPreviewDto? currentChapter = null;
        bool inChapters = false;
        bool inLessons = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
            var indent = rawLine.TakeWhile(char.IsWhiteSpace).Count();
            static string CleanYamlValue(string s) => s.Trim().Trim('"', '\'').Trim();

            if (indent == 0 && Regex.IsMatch(line, @"^display_title:\s*(.+)$", RegexOptions.IgnoreCase))
            {
                var val = CleanYamlValue(Regex.Match(line, @"^display_title:\s*(.+)$", RegexOptions.IgnoreCase).Groups[1].Value);
                if (!string.IsNullOrWhiteSpace(val)) title = val;
            }
            else if (indent == 0 && Regex.IsMatch(line, @"^(?:course|title):\s*(.+)$", RegexOptions.IgnoreCase) && title == "Khóa học SWM Đã Nhập")
            {
                var val = CleanYamlValue(Regex.Match(line, @"^(?:course|title):\s*(.+)$", RegexOptions.IgnoreCase).Groups[1].Value);
                if (!string.IsNullOrWhiteSpace(val)) title = val;
            }
            else if (indent == 0 && Regex.IsMatch(line, @"^(?:description|desc):\s*(.+)$", RegexOptions.IgnoreCase))
            {
                var val = CleanYamlValue(Regex.Match(line, @"^(?:description|desc):\s*(.+)$", RegexOptions.IgnoreCase).Groups[1].Value);
                if (!string.IsNullOrWhiteSpace(val)) description = val;
            }
            else if (indent == 0 && Regex.IsMatch(line, @"^slug:\s*(.+)$", RegexOptions.IgnoreCase))
            {
                var val = CleanYamlValue(Regex.Match(line, @"^slug:\s*(.+)$", RegexOptions.IgnoreCase).Groups[1].Value);
                if (!string.IsNullOrWhiteSpace(val)) slug = val;
            }
            else if (indent == 0 && line.StartsWith("chapters:", StringComparison.OrdinalIgnoreCase))
            {
                inChapters = true;
                inLessons = false;
            }
            else if (inChapters && inLessons && indent >= 4 && (line.StartsWith("- title:", StringComparison.OrdinalIgnoreCase) || line.StartsWith("- name:", StringComparison.OrdinalIgnoreCase)))
            {
                var lTitle = CleanYamlValue(line.Substring(line.IndexOf(':') + 1));
                currentChapter?.Lessons.Add(new WizardLessonPreviewDto(lTitle, "Bài học từ gói SWM", 25));
            }
            else if (inChapters && inLessons && indent >= 6)
            {
                if (currentChapter != null && currentChapter.Lessons.Count > 0)
                {
                    var lastIdx = currentChapter.Lessons.Count - 1;
                    var lastLes = currentChapter.Lessons[lastIdx];
                    if (line.StartsWith("video_url:", StringComparison.OrdinalIgnoreCase))
                    {
                        var vUrl = CleanYamlValue(line.Substring(line.IndexOf(':') + 1));
                        currentChapter.Lessons[lastIdx] = lastLes with { VideoUrl = vUrl };
                    }
                    else if (line.StartsWith("estimated_minutes:", StringComparison.OrdinalIgnoreCase))
                    {
                        var minStr = CleanYamlValue(line.Substring(line.IndexOf(':') + 1));
                        if (int.TryParse(minStr, out var mins))
                        {
                            currentChapter.Lessons[lastIdx] = lastLes with { EstimatedMinutes = mins };
                        }
                    }
                    else if (line.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                    {
                        var fPath = CleanYamlValue(line.Substring(line.IndexOf(':') + 1));
                        currentChapter.Lessons[lastIdx] = lastLes with { FilePath = fPath };
                    }
                }
            }
            else if (inChapters && (line.StartsWith("- title:", StringComparison.OrdinalIgnoreCase) || line.StartsWith("- name:", StringComparison.OrdinalIgnoreCase)))
            {
                var chTitle = CleanYamlValue(line.Substring(line.IndexOf(':') + 1));
                currentChapter = new WizardChapterPreviewDto(chTitle, chapters.Count + 1, new List<WizardLessonPreviewDto>());
                chapters.Add(currentChapter);
                inLessons = false;
            }
            else if (inChapters && line.StartsWith("lessons:", StringComparison.OrdinalIgnoreCase))
            {
                inLessons = true;
            }
        }
    }

    public record ParsedFlashcardYamlEntry(string Concept, string Front, string Back, string? SourceLesson, string? LessonId);

    public static List<(string concept, string front, string back)> ParseFlashcardsYaml(string yaml)
    {
        var (_, cards) = ParseFlashcardsYamlDeck(yaml);
        return cards.Select(c => (c.Concept, c.Front, c.Back)).ToList();
    }

    public static (string? DeckLessonId, List<ParsedFlashcardYamlEntry> Cards) ParseFlashcardsYamlDeck(string yaml)
    {
        var result = new List<ParsedFlashcardYamlEntry>();
        var lines = yaml.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        string? deckLessonId = null;
        string concept = string.Empty;
        string? cardSourceLesson = null;
        string? cardLessonId = null;
        var frontBuilder = new StringBuilder();
        var backBuilder = new StringBuilder();
        string currentField = string.Empty;

        static string CleanYamlScalar(string raw)
        {
            var trimmed = raw.Trim();
            if (trimmed.Length >= 2 &&
                ((trimmed.StartsWith('"') && trimmed.EndsWith('"')) ||
                 (trimmed.StartsWith('\'') && trimmed.EndsWith('\''))))
            {
                return trimmed[1..^1].Trim();
            }
            return trimmed.Trim('"', '\'', ' ');
        }

        void CommitCard()
        {
            var f = CleanYamlScalar(frontBuilder.ToString());
            var b = CleanYamlScalar(backBuilder.ToString());
            if (!string.IsNullOrWhiteSpace(f) && !string.IsNullOrWhiteSpace(b))
            {
                result.Add(new ParsedFlashcardYamlEntry(
                    CleanYamlScalar(concept),
                    f,
                    b,
                    string.IsNullOrWhiteSpace(cardSourceLesson) ? null : CleanYamlScalar(cardSourceLesson),
                    string.IsNullOrWhiteSpace(cardLessonId) ? deckLessonId : CleanYamlScalar(cardLessonId)));
            }
            concept = string.Empty;
            cardSourceLesson = null;
            cardLessonId = null;
            frontBuilder.Clear();
            backBuilder.Clear();
            currentField = string.Empty;
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;

            if (!rawLine.StartsWith(" ") && !rawLine.StartsWith("\t") && !line.StartsWith("-"))
            {
                var topMatch = Regex.Match(line, @"^(?:lesson_id|lesson|unit_id):\s*(.+)$", RegexOptions.IgnoreCase);
                if (topMatch.Success)
                {
                    deckLessonId = CleanYamlScalar(topMatch.Groups[1].Value);
                }
            }

            if (line.StartsWith("- id:") || (line.StartsWith("-") && line.Contains("concept:", StringComparison.OrdinalIgnoreCase)) ||
                (line.StartsWith("-") && line.Contains("front:", StringComparison.OrdinalIgnoreCase)))
            {
                CommitCard();
            }

            var conceptMatch = Regex.Match(line, @"^(?:-\s*)?concept:\s*(.*)$", RegexOptions.IgnoreCase);
            var frontMatch = Regex.Match(line, @"^(?:-\s*)?front:\s*(.*)$", RegexOptions.IgnoreCase);
            var backMatch = Regex.Match(line, @"^(?:-\s*)?back:\s*(.*)$", RegexOptions.IgnoreCase);
            var sourceLessonMatch = Regex.Match(line, @"^(?:-\s*)?source_lesson:\s*(.*)$", RegexOptions.IgnoreCase);
            var cardLessonMatch = Regex.Match(line, @"^(?:-\s*)?lesson_id:\s*(.*)$", RegexOptions.IgnoreCase);

            if (conceptMatch.Success)
            {
                concept = conceptMatch.Groups[1].Value.Trim();
                currentField = "concept";
            }
            else if (frontMatch.Success)
            {
                var val = frontMatch.Groups[1].Value.Trim();
                frontBuilder.Clear();
                if (!string.IsNullOrEmpty(val) && val != "|" && val != ">") frontBuilder.Append(val);
                currentField = "front";
            }
            else if (backMatch.Success)
            {
                var val = backMatch.Groups[1].Value.Trim();
                backBuilder.Clear();
                if (!string.IsNullOrEmpty(val) && val != "|" && val != ">") backBuilder.Append(val);
                currentField = "back";
            }
            else if (sourceLessonMatch.Success)
            {
                cardSourceLesson = sourceLessonMatch.Groups[1].Value.Trim();
                currentField = string.Empty;
            }
            else if (cardLessonMatch.Success && (rawLine.StartsWith(" ") || rawLine.StartsWith("\t") || line.StartsWith("-")))
            {
                cardLessonId = cardLessonMatch.Groups[1].Value.Trim();
                currentField = string.Empty;
            }
            else if (Regex.IsMatch(line, @"^(?:-\s*)?(?:version|claim_ids|tags|deck_id|subject|description|cards)\s*:"))
            {
                currentField = string.Empty;
            }
            else if (rawLine.StartsWith("    ") || rawLine.StartsWith("\t"))
            {
                if (currentField == "front")
                {
                    if (frontBuilder.Length > 0) frontBuilder.Append(' ');
                    frontBuilder.Append(line);
                }
                else if (currentField == "back")
                {
                    if (backBuilder.Length > 0) backBuilder.Append(' ');
                    backBuilder.Append(line);
                }
            }
            else
            {
                currentField = string.Empty;
            }
        }

        CommitCard();
        return (deckLessonId, result);
    }

    public static List<PracticeQuizItem> ParseQuizMarkdown(
        Guid quizId,
        string quizMarkdown,
        string? answerKeyMarkdown,
        DateTimeOffset now)
    {
        var items = new List<PracticeQuizItem>();
        var (answersByCode, answersByIndex) = ParseQuizAnswerKey(answerKeyMarkdown);

        var cleanedMarkdown = Regex.Replace(quizMarkdown.Trim(), @"^---\r?\n[\s\S]*?\r?\n---\r?\n", string.Empty);
        var lines = cleanedMarkdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        bool usesExplicitHeadingStyle = lines.Any(l =>
        {
            var t = l.Trim();
            return Regex.IsMatch(t, @"^(?:#{2,4}\s+(?:(?:Câu|Question)\s+`?(?:Q\d+(?:-\d+)?|\d+)|Q\d+(?:-\d+)?\b)|\*\*(?:Câu|Question)\s+\d+:?\*\*)", RegexOptions.IgnoreCase);
        });

        var headingPattern = new Regex(
            @"^(?:#{2,4}\s+(?:(?:Câu|Question)\s+`?(?<code1>Q\d+(?:-\d+)?|\d+)`?|(?<code2>Q\d+(?:-\d+)?))\b(?<rest>.*)|\*\*(?:Câu|Question)\s+(?<code3>\d+):?\*\*(?<rest>.*))$",
            RegexOptions.IgnoreCase);
        var numberedQuestionPattern = new Regex(
            @"^(?<num>\d+)\.\s+(?:\*\*\[?(?<code4>Q\d+(?:-\d+)?)\]?\*\*\s*)?(?<rest>.+)$",
            RegexOptions.IgnoreCase);
        var choicePattern = new Regex(
            @"^\s*(?:[-*]\s*)?(?:\*\*)?(?<key>[A-D])(?:\*\*)?[\.\)](?:\*\*)?\s+(?<text>.+)$");
        var inlineAnsPattern = new Regex(
            @"(?:\*\*Đáp án(?:\s+đúng)?:\*\*|\bĐáp án(?:\s+đúng)?:)\s*(?:\*\*)?(?<ans>[A-D])\b",
            RegexOptions.IgnoreCase);

        string? activeCode = null;
        int activeNum = 0;
        var promptLines = new List<string>();
        var choices = new List<(string Key, string Text)>();
        string? inlineAnswer = null;
        bool inCodeFence = false;
        int sequentialIdx = 0;

        static string CleanHeadingPromptRemainder(string rawRest)
        {
            var s = rawRest.Trim();
            // Strip leading punctuation, [LO-...] tags, backticks, (Analyze/Apply/...) markers
            s = Regex.Replace(s, @"^[`:\-—\s]+", string.Empty);
            s = Regex.Replace(s, @"^`?\[LO-[A-Z0-9-]+\]`?\s*", string.Empty, RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"^\*\*\[LO-[A-Z0-9-]+\]\*\*\s*", string.Empty, RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"^[\(\[（](?:Remember|Understand|Apply|Analyze|Evaluate|Create|Nhớ|Hiểu|Áp dụng|Vận dụng|Phân tích|Đánh giá|Apply/Analyze|Understand/Apply|Current[^)\]]*)[\)\]）]\s*", string.Empty, RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"^\*\*\([^)]+\)\*\*\s*", string.Empty);
            s = Regex.Replace(s, @"^[`:\-—\s]+", string.Empty);
            return s.Trim();
        }

        static int ExtractQuestionNumber(string code, int fallbackNum)
        {
            var m = Regex.Match(code, @"(\d+)$");
            return m.Success && int.TryParse(m.Groups[1].Value, out int parsed) ? parsed : fallbackNum;
        }

        void CommitQuestion()
        {
            if (activeCode == null && choices.Count == 0)
            {
                promptLines.Clear();
                return;
            }

            if (choices.Count == 0 && promptLines.Count == 0)
            {
                activeCode = null;
                activeNum = 0;
                inlineAnswer = null;
                return;
            }

            // Skip non-question trailing meta headings or intro numbered bullets in non-heading quizzes
            if ((choices.Count == 0 && activeCode == null) || (choices.Count == 0 && !usesExplicitHeadingStyle))
            {
                activeCode = null;
                activeNum = 0;
                promptLines.Clear();
                inlineAnswer = null;
                return;
            }

            sequentialIdx++;
            var qNumber = activeNum > 0 ? activeNum : sequentialIdx;
            var prompt = string.Join("\n", promptLines).Trim();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                prompt = $"Câu hỏi {qNumber}";
            }

            string correctKey = "A";
            if (!string.IsNullOrWhiteSpace(activeCode) && answersByCode.TryGetValue(activeCode, out var byCode))
            {
                correctKey = byCode;
            }
            else if (answersByCode.TryGetValue($"Q{qNumber:D2}", out var byPaddedQ))
            {
                correctKey = byPaddedQ;
            }
            else if (answersByIndex.TryGetValue(qNumber, out var byNumber))
            {
                correctKey = byNumber;
            }
            else if (answersByIndex.TryGetValue(sequentialIdx, out var bySeq))
            {
                correctKey = bySeq;
            }
            else if (!string.IsNullOrWhiteSpace(inlineAnswer))
            {
                correctKey = inlineAnswer;
            }

            var choiceObjects = choices
                .Select(c => (object)new { key = c.Key, text = c.Text })
                .ToList();

            items.Add(new PracticeQuizItem
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                ItemType = choiceObjects.Count > 0 ? "multiple_choice" : "essay",
                PromptAstJson = JsonSerializer.Serialize(new { question = prompt }),
                ChoicesJson = choiceObjects.Count > 0 ? JsonSerializer.Serialize(choiceObjects) : null,
                ClozeSolution = correctKey,
                CreatedAt = now.AddSeconds(items.Count)
            });

            activeCode = null;
            activeNum = 0;
            promptLines.Clear();
            choices.Clear();
            inlineAnswer = null;
        }

        foreach (var rawLine in lines)
        {
            var trimmed = rawLine.Trim();

            if (trimmed.StartsWith("```"))
            {
                inCodeFence = !inCodeFence;
                if (activeCode != null)
                {
                    promptLines.Add(rawLine.TrimEnd());
                }
                continue;
            }

            if (inCodeFence)
            {
                if (activeCode != null)
                {
                    promptLines.Add(rawLine.TrimEnd());
                }
                continue;
            }

            var hm = headingPattern.Match(trimmed);
            if (hm.Success)
            {
                CommitQuestion();
                activeCode = (hm.Groups["code1"].Success ? hm.Groups["code1"].Value :
                              hm.Groups["code2"].Success ? hm.Groups["code2"].Value :
                              hm.Groups["code3"].Value).Trim();
                activeNum = ExtractQuestionNumber(activeCode, sequentialIdx + 1);
                var rest = CleanHeadingPromptRemainder(hm.Groups["rest"].Value);
                if (!string.IsNullOrWhiteSpace(rest))
                {
                    promptLines.Add(rest);
                }
                continue;
            }

            if (!usesExplicitHeadingStyle)
            {
                var nm = numberedQuestionPattern.Match(trimmed);
                if (nm.Success && (!trimmed.StartsWith("A.") && !trimmed.StartsWith("B.")))
                {
                    CommitQuestion();
                    activeCode = nm.Groups["code4"].Success
                        ? nm.Groups["code4"].Value.Trim()
                        : nm.Groups["num"].Value.Trim();
                    int.TryParse(nm.Groups["num"].Value, out activeNum);
                    if (activeNum <= 0) activeNum = sequentialIdx + 1;
                    var rest = CleanHeadingPromptRemainder(nm.Groups["rest"].Value);
                    if (!string.IsNullOrWhiteSpace(rest))
                    {
                        promptLines.Add(rest);
                    }
                    continue;
                }
            }

            if (activeCode == null)
            {
                continue;
            }

            // Top-level non-question section headers end the previous question cleanly
            if (Regex.IsMatch(trimmed, @"^##\s+"))
            {
                CommitQuestion();
                continue;
            }

            if (trimmed == "---")
            {
                continue;
            }

            var ansMatch = inlineAnsPattern.Match(trimmed);
            if (ansMatch.Success)
            {
                inlineAnswer = ansMatch.Groups["ans"].Value.ToUpperInvariant();
                continue;
            }

            var cm = choicePattern.Match(rawLine);
            if (cm.Success)
            {
                choices.Add((cm.Groups["key"].Value.Trim().ToUpperInvariant(), cm.Groups["text"].Value.Trim()));
                continue;
            }

            if (choices.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(trimmed) || promptLines.Count > 0)
                {
                    promptLines.Add(trimmed);
                }
            }
        }

        CommitQuestion();
        return items;
    }

    private static (Dictionary<string, string> ByCode, Dictionary<int, string> ByIndex) ParseQuizAnswerKey(string? answerKeyMarkdown)
    {
        var byCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var byIndex = new Dictionary<int, string>();
        if (string.IsNullOrWhiteSpace(answerKeyMarkdown))
        {
            return (byCode, byIndex);
        }

        static string StripCellMarkdown(string raw) =>
            raw.Replace("**", string.Empty).Replace("`", string.Empty).Trim();

        static void RecordAnswer(
            Dictionary<string, string> codes,
            Dictionary<int, string> indices,
            string? code,
            int index,
            string letter)
        {
            var normLetter = letter.Trim().ToUpperInvariant();
            if (normLetter.Length != 1 || normLetter[0] < 'A' || normLetter[0] > 'D') return;
            if (!string.IsNullOrWhiteSpace(code))
            {
                codes.TryAdd(code.Trim(), normLetter);
            }
            if (index > 0)
            {
                indices.TryAdd(index, normLetter);
            }
        }

        var lines = answerKeyMarkdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        string? currentSectionCode = null;
        int currentSectionNum = 0;

        foreach (var rawLine in lines)
        {
            var trimmed = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            // 1. Markdown table row: | Q01-01 | ... | **B** | ...
            if (trimmed.StartsWith('|'))
            {
                if (trimmed.Contains("---")) continue;
                var cells = trimmed.Trim('|').Split('|').Select(StripCellMarkdown).ToArray();
                if (cells.Length >= 2)
                {
                    var codeMatch = Regex.Match(cells[0], @"^(?:Câu\s*)?(?<code>Q\d+(?:-\d+)?|\d+)$", RegexOptions.IgnoreCase);
                    if (codeMatch.Success)
                    {
                        var qCode = codeMatch.Groups["code"].Value;
                        var numMatch = Regex.Match(qCode, @"(\d+)$");
                        int qIdx = numMatch.Success && int.TryParse(numMatch.Groups[1].Value, out int parsed) ? parsed : 0;

                        for (int ci = 1; ci < cells.Length; ci++)
                        {
                            if (Regex.IsMatch(cells[ci], @"^[A-D]$", RegexOptions.IgnoreCase))
                            {
                                RecordAnswer(byCode, byIndex, qCode, qIdx, cells[ci]);
                                break;
                            }
                        }
                    }
                }
                continue;
            }

            // 2. Numbered answer list item: `1. **A** – ...` or `1. **B. Duy trì...**`
            var listAns = Regex.Match(trimmed, @"^(?<num>\d+)\.\s+\*\*(?<ans>[A-D])(?:\.|\*\*|\s|–|—|-)", RegexOptions.IgnoreCase);
            if (listAns.Success && int.TryParse(listAns.Groups["num"].Value, out int listNum))
            {
                RecordAnswer(byCode, byIndex, listNum.ToString(), listNum, listAns.Groups["ans"].Value);
                continue;
            }

            // 3. Section heading: `### Q01-01 — Đáp án **A**` or `### Câu \`Q01-01\`` or `### Câu 1:`
            var secHead = Regex.Match(trimmed, @"^#{1,4}\s*(?:Câu|Question)?\s*`?(?<code>Q\d+(?:-\d+)?|\d+)`?\b(.*)$", RegexOptions.IgnoreCase);
            if (secHead.Success)
            {
                currentSectionCode = secHead.Groups["code"].Value;
                var tailNum = Regex.Match(currentSectionCode, @"(\d+)$");
                currentSectionNum = tailNum.Success && int.TryParse(tailNum.Groups[1].Value, out int p) ? p : 0;

                var sameLineAns = Regex.Match(secHead.Groups[1].Value, @"Đáp án(?:\s+đúng)?\s*[:\-]?\s*\**(?<ans>[A-D])\**", RegexOptions.IgnoreCase);
                if (sameLineAns.Success)
                {
                    RecordAnswer(byCode, byIndex, currentSectionCode, currentSectionNum, sameLineAns.Groups["ans"].Value);
                }
                continue;
            }

            // 4. Answer bullet inside a question section: `- **Đáp án đúng:** **C...**`
            if (currentSectionCode != null)
            {
                var bodyAns = Regex.Match(trimmed, @"Đáp án(?:\s+đúng)?\s*[:\-]\s*(?:\*\*)?(?<ans>[A-D])\b", RegexOptions.IgnoreCase);
                if (bodyAns.Success)
                {
                    RecordAnswer(byCode, byIndex, currentSectionCode, currentSectionNum, bodyAns.Groups["ans"].Value);
                }
            }
        }

        return (byCode, byIndex);
    }

    private static string? ExtractFirstHeading(string markdown)
    {
        var match = Regex.Match(markdown, @"^#+\s+(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string Truncate(string value, int maxLen)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLen) return value;
        return value[..(maxLen - 1)].TrimEnd() + "…";
    }
}
