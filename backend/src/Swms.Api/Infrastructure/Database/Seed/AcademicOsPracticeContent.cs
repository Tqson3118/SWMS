namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;

/// <summary>
/// Expands AcademicCourseId (Hệ điều hành) from 1 chapter / 2 lessons into a full 3-chapter / 6-lesson
/// curriculum with PracticeActivities, Flashcards, Lesson Quizzes, Chapter Exams, and Final Exam,
/// while strictly preserving LessonPagingId's ContentAstJson, PlainText, and CurrentVersion (= 2)
/// so Annotation Self-Healing tests remain untouched.
/// Also seeds [Đề Chương 1..3] and [Đề Tổng Hợp Cuối Khóa] exams across all 6 interactive courses.
/// </summary>
public static class AcademicOsPracticeContent
{
    public const string AcademicCourseDescription = "Khóa học nền tảng về Hệ điều hành: Tiến trình và Luồng, Điều phối CPU, Đồng bộ hóa (Mutex, Semaphore, Deadlock), Quản lý bộ nhớ ảo, TLB và Thuật toán thay thế trang.";
    public const string UpgradeMarker = "<!-- studxy-os-v3 -->";
    public const string UpgradeMarkerV4 = "<!-- studxy-os-v4 -->";

    public static readonly Guid OsChapter1Id = Guid.Parse("33333333-3333-4333-8333-333333333361");
    public static readonly Guid OsChapter2Id = Guid.Parse("33333333-3333-4333-8333-333333333362");

    public static readonly Guid OsLesson11Id = Guid.Parse("44444444-4444-4444-8444-444444444491");
    public static readonly Guid OsLesson12Id = Guid.Parse("44444444-4444-4444-8444-444444444492");
    public static readonly Guid OsLesson21Id = Guid.Parse("44444444-4444-4444-8444-444444444493");
    public static readonly Guid OsLesson22Id = Guid.Parse("44444444-4444-4444-8444-444444444494");

    private static readonly JsonSerializerOptions SafeJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private sealed record OsLessonMeta(
        Guid ChapterId,
        Guid LessonId,
        string Title,
        string Slug,
        int SortOrder,
        int EstimatedMinutes,
        string Markdown,
        string Summary1,
        string Summary2
    );

    private sealed record ExamQuestionSpec(
        string ItemType, // multiple_choice | cloze | essay
        string Question,
        string? OptionA,
        string? OptionB,
        string? OptionC,
        string? OptionD,
        string? Solution,
        string Explanation,
        string[]? RubricCriteria = null
    );

    public static async Task EnsureSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == SeedDataInitializer.AcademicCourseId);
        if (course != null)
        {
            if (!await db.CourseEnrollments.AnyAsync(e => e.UserId == SeedDataInitializer.DefaultUserId && e.CourseId == course.Id))
            {
                db.CourseEnrollments.Add(new CourseEnrollment
                {
                    Id = Guid.NewGuid(),
                    UserId = SeedDataInitializer.DefaultUserId,
                    CourseId = course.Id,
                    Role = "student",
                    EnrolledAt = now
                });
            }

            if (!string.Equals(course.Visibility, "personal", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(course.Description, AcademicCourseDescription, StringComparison.Ordinal))
            {
                course.Description = AcademicCourseDescription;
                course.UpdatedAt = now;
            }

            await EnsureOsChaptersAndLessonsAsync(db, course.Id, now);
            await EnsureOsActivitiesAsync(db, course.Id, now);
            await EnsureOsFlashcardsAsync(db, now);
            await EnsureOsQuizzesAsync(db, now);
        }

        await EnsureComprehensiveCourseExamsSeededAsync(db, now);
    }

    private static async Task EnsureOsChaptersAndLessonsAsync(SwmsDbContext db, Guid courseId, DateTimeOffset now)
    {
        var chapterSpecs = new (Guid Id, string Title, int SortOrder)[]
        {
            (OsChapter1Id, "Chương 1: Tiến trình, Luồng & Điều phối CPU (Processes, Threads & Scheduling)", 1),
            (OsChapter2Id, "Chương 2: Đồng bộ hóa Đa luồng, Khóa & Phòng chống Deadlock", 2),
            (SeedDataInitializer.ChapterMemoryId, "Chương 3: Quản lý Bộ nhớ & Phân trang (Memory Management)", 3)
        };

        foreach (var (id, title, sortOrder) in chapterSpecs)
        {
            var ch = await db.Chapters.FirstOrDefaultAsync(c => c.Id == id);
            if (ch == null)
            {
                db.Chapters.Add(new Chapter
                {
                    Id = id,
                    CourseId = courseId,
                    Title = title,
                    SortOrder = sortOrder,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else if (ch.SortOrder != sortOrder)
            {
                ch.SortOrder = sortOrder;
                ch.UpdatedAt = now;
            }
        }
        await db.SaveChangesAsync();

        // 1.1, 1.2, 2.1, 2.2 new lessons
        foreach (var meta in GetNewOsLessons())
        {
            var existing = await db.Lessons.FirstOrDefaultAsync(l => l.Id == meta.LessonId);
            if (existing == null)
            {
                db.Lessons.Add(new Lesson
                {
                    Id = meta.LessonId,
                    ChapterId = meta.ChapterId,
                    Title = meta.Title,
                    Slug = meta.Slug,
                    SortOrder = meta.SortOrder,
                    CurrentVersion = 1,
                    EstimatedMinutes = meta.EstimatedMinutes,
                    ContentMarkdown = meta.Markdown,
                    PlainText = meta.Markdown,
                    ContentAstJson = BuildAst(meta.Title, meta.Summary1, meta.Summary2),
                    IsPublished = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else if (existing.CurrentVersion == 1 && !(existing.ContentMarkdown ?? string.Empty).Contains(UpgradeMarkerV4, StringComparison.Ordinal))
            {
                existing.ContentMarkdown = meta.Markdown;
                existing.PlainText = meta.Markdown;
                existing.EstimatedMinutes = meta.EstimatedMinutes;
                existing.UpdatedAt = now;
            }
        }

        // 3.2 (LessonPagingId): STRICT INVARIANT — NEVER modify ContentAstJson, PlainText, or CurrentVersion (= 2).
        // Only fill or upgrade ContentMarkdown if empty or missing UpgradeMarkerV4 so Markdown reader renders rich v4 theory alongside AST/Annotations.
        var lesson32 = await db.Lessons.FirstOrDefaultAsync(l => l.Id == SeedDataInitializer.LessonPagingId);
        if (lesson32 != null && (string.IsNullOrWhiteSpace(lesson32.ContentMarkdown) || !lesson32.ContentMarkdown.Contains(UpgradeMarkerV4, StringComparison.Ordinal)))
        {
            lesson32.ContentMarkdown = GetLesson32Markdown();
            lesson32.EstimatedMinutes = Math.Max(lesson32.EstimatedMinutes, 45);
            lesson32.UpdatedAt = now;
        }

        // 3.3 (LessonPageReplacementId): Safe upgrade ONLY if CurrentVersion == 1 and marker missing.
        var lesson33 = await db.Lessons.FirstOrDefaultAsync(l => l.Id == SeedDataInitializer.LessonPageReplacementId);
        if (lesson33 != null && lesson33.CurrentVersion == 1 && !(lesson33.ContentMarkdown ?? string.Empty).Contains(UpgradeMarkerV4, StringComparison.Ordinal))
        {
            lesson33.ContentMarkdown = GetLesson33Markdown();
            lesson33.PlainText = lesson33.ContentMarkdown;
            lesson33.EstimatedMinutes = 45;
            lesson33.UpdatedAt = now;
        }

        await db.SaveChangesAsync();
    }

    private static string BuildAst(string title, string p1, string p2)
    {
        return JsonSerializer.Serialize(new
        {
            type = "doc",
            content = new object[]
            {
                new { type = "heading", attrs = new { level = 2, block_id = Guid.NewGuid().ToString() }, content = new[] { new { type = "text", text = title } } },
                new { type = "paragraph", attrs = new { block_id = Guid.NewGuid().ToString() }, content = new[] { new { type = "text", text = p1 } } },
                new { type = "paragraph", attrs = new { block_id = Guid.NewGuid().ToString() }, content = new[] { new { type = "text", text = p2 } } }
            }
        }, SafeJsonOptions);
    }

    private static List<OsLessonMeta> GetNewOsLessons() => new()
    {
        new OsLessonMeta(
            OsChapter1Id,
            OsLesson11Id,
            "Bài 1.1: Tiến trình (Process), Luồng (Thread) & Chuyển ngữ cảnh (Context Switch)",
            "tien-trinh-luong-va-chuyen-ngu-canh",
            1,
            45,
            GetLesson11Markdown(),
            "Tiến trình sở hữu không gian địa chỉ cô lập và PCB; các luồng trong cùng tiến trình chia sẻ Heap nhưng giữ Stack và Registers riêng.",
            "Context Switch giữa các tiến trình có chi phí cao do phải chuyển thanh ghi bảng trang CR3 và làm mất nóng bộ đệm TLB."
        ),

        new OsLessonMeta(
            OsChapter1Id,
            OsLesson12Id,
            "Bài 1.2: Thuật toán Điều phối CPU (FCFS, SJF, Round Robin & MLFQ)",
            "thuat-toan-dieu-phoi-cpu-fcfs-sjf-rr-mlfq",
            2,
            45,
            GetLesson12Markdown(),
            "FCFS dễ gặp hiệu ứng đoàn tàu (Convoy Effect); SJF tối ưu thời gian hoàn thành nhưng có thể gây đói CPU cho tác vụ dài.",
            "Round Robin tối ưu thời gian phản hồi theo lát cắt quantum, còn MLFQ tự động học hành vi CPU-bound và I/O-bound."
        ),

        new OsLessonMeta(
            OsChapter2Id,
            OsLesson21Id,
            "Bài 2.1: Race Condition, Critical Section, Mutex & Semaphore",
            "race-condition-critical-section-mutex-semaphore",
            1,
            45,
            GetLesson21Markdown(),
            "Race Condition xảy ra khi nhiều luồng thực thi không nguyên tử trên cùng vùng nhớ chia sẻ; Mutex đảm bảo loại trừ lẫn nhau có sở hữu.",
            "Semaphore đếm cho phép tối đa K luồng truy cập tài nguyên đồng thời hoặc đồng bộ tín hiệu Producer-Consumer."
        ),

        new OsLessonMeta(
            OsChapter2Id,
            OsLesson22Id,
            "Bài 2.2: Deadlock — 4 Điều kiện Coffman, Phòng tránh & Phục hồi",
            "deadlock-4-dieu-kien-coffman-va-phong-tranh",
            2,
            45,
            GetLesson22Markdown(),
            "Deadlock chỉ xảy ra khi đồng thời thỏa mãn 4 điều kiện Coffman: Mutual Exclusion, Hold & Wait, No Preemption và Circular Wait.",
            "Sắp xếp thứ tự khóa toàn cục (Lock Ordering) là giải pháp thực chiến đơn giản và triệt để nhất để phá vỡ Circular Wait."
        )
    };

    private static string GetLesson11Markdown() => """"
# Bài 1.1: Tiến trình (Process), Luồng (Thread) & Chuyển ngữ cảnh (Context Switch)

> [!NOTE]
> **Khái niệm then chốt:** Một chương trình (Program) trên ổ đĩa chỉ là một tệp nhị phân tĩnh (Executable File). Khi được nạp vào bộ nhớ và cấp phát tài nguyên bởi hệ điều hành, nó trở thành một thực thể sống động mang tên **Tiến trình (Process)**. Bên trong mỗi tiến trình, một hoặc nhiều **Luồng (Threads)** cùng vận hành song song để thực thi các dòng lệnh.

> [!TIP]
> **Hình ảnh trực quan:** Tưởng tượng một nhà hàng tiêu chuẩn 5 sao:
> - **Chương trình (Program)**: Cuốn sách công thức nấu ăn nằm yên trên kệ sách.
> - **Tiến trình (Process)**: Căn bếp hoàn chỉnh đang hoạt động với đầy đủ mặt bằng, bếp ga, tủ lạnh, nguyên liệu và giấy phép kinh doanh riêng biệt.
> - **Luồng (Thread)**: Các đầu bếp làm việc đồng thời trong cùng căn bếp đó. Họ dùng chung bàn bếp, gia vị và tủ lạnh (Heap/Global Data), nhưng mỗi đầu bếp có đôi tay, mắt nhìn và sổ tay ghi nhớ bước làm riêng (Registers, Program Counter, Call Stack).

---

## 1. Không gian Địa chỉ Ảo của Tiến trình (Virtual Address Space)

Mỗi tiến trình trong hệ điều hành hiện đại (như Linux hoặc Windows 64-bit) đều được cung cấp một ảo giác rằng nó sở hữu toàn bộ không gian địa chỉ tuyến tính riêng biệt, cô lập hoàn toàn với các tiến trình khác.

```text
+------------------------------------+ 0xFFFFFFFFFFFFFFFF (Kernel Space Top)
| Linux Kernel Space (128 TB)        | Dành riêng cho nhân Hệ điều hành
+------------------------------------+ 0xFFFF800000000000 (Kernel Space Base)
| Không gian địa chỉ không hợp lệ     | Canonical Address Hole (Non-canonical gap)
+------------------------------------+ 0x00007FFFFFFFFFFF (User Space Top)
| User Stack (Ngăn xếp cục bộ)       | Phát triển từ địa chỉ cao xuống thấp (v)
|               |                    | Lưu trữ Stack Frame, biến cục bộ, địa chỉ trả về
|               v                    |
|                                    |
| Memory Mapping Segment (mmap)      | Nạp các Shared Libraries (.so / .dll), IPC
|               ^                    |
|               |                    |
| Run-time Heap (Bộ nhớ động)        | Phát triển từ địa chỉ thấp lên cao (^)
|                                    | Cấp phát qua malloc(), new, brk/sbrk
+------------------------------------+
| BSS Segment (Uninitialized Data)   | Biến toàn cục & static chưa khởi tạo (= 0)
+------------------------------------+
| Data Segment (Initialized Data)    | Biến toàn cục & static đã gán giá trị ban đầu
+------------------------------------+
| Text Segment (Code Segment)        | Mã máy nhị phân thực thi (Read-Only)
+------------------------------------+ 0x0000000000400000 (Base User Address)
```

### Các phân vùng bộ nhớ chính của Process:
1. **Text Segment (Phân vùng mã lệnh):** Chứa các chỉ thị mã máy nhị phân của chương trình. Vùng nhớ này được đánh dấu quyền **Chỉ đọc (Read-Only)** để ngăn chặn việc tự sửa mã lệnh và cho phép nhiều tiến trình chạy cùng một chương trình dùng chung một bản copy trong RAM vật lý.
2. **Data Segment (Dữ liệu đã khởi tạo):** Lưu trữ các biến toàn cục (global variables) và biến tĩnh (static variables) đã được gán giá trị rõ ràng khi biên dịch (ví dụ: `int max_users = 1000;`).
3. **BSS Segment (Block Started by Symbol):** Lưu trữ các biến toàn cục và static chưa được khởi tạo giá trị ban đầu. Kernel chỉ cần ghi nhớ kích thước của vùng này và tự động điền toàn bộ byte 0 khi nạp, giúp tiết kiệm dung lượng file thực thi trên ổ đĩa.
4. **Heap (Vùng nhớ động):** Dành cho dữ liệu được cấp phát linh hoạt trong thời gian chạy (runtime) thông qua các hàm như `malloc()`, `calloc()` trong C hoặc toán tử `new` trong C++/C#/Java. Vùng nhớ này phát triển từ địa chỉ thấp lên địa chỉ cao. Quản lý Heap đòi hỏi bộ thu gom rác (Garbage Collector) hoặc giải phóng thủ công (`free()`), nếu không sẽ dẫn tới hiện tượng rò rỉ bộ nhớ (Memory Leak).
5. **Memory Mapping Segment (mmap):** Vùng nạp các thư viện liên kết động (Dynamic Shared Libraries như `libc.so`, `kernel32.dll`), bộ nhớ chia sẻ IPC (POSIX Shared Memory), hoặc file mapping.
6. **Stack (Ngăn xếp lời gọi hàm):** Lưu trữ các khung ngăn xếp (Stack Frames), bao gồm tham số hàm truyền vào, biến cục bộ (local variables), và địa chỉ trả về của thanh ghi con trỏ lệnh (`RIP/PC`). Ngăn xếp phát triển từ địa chỉ cao xuống địa chỉ thấp theo cơ chế LIFO (Last-In, First-Out).

---

## 2. Kiến trúc Đa luồng (Multi-threading) & Phân tách Tài nguyên

Một tiến trình có thể chứa nhiều luồng thực thi (threads). Hãy phân tích sự khác biệt cốt lõi giữa Tiến trình và Luồng:

| Tiêu chí | Tiến trình (Process) | Luồng (Thread) |
|---|---|---|
| **Không gian nhớ** | Cô lập hoàn toàn; một tiến trình không thể đọc/ghi trực tiếp vào bộ nhớ của tiến trình khác | Dùng chung toàn bộ không gian địa chỉ ảo của tiến trình cha (Code, Data, BSS, Heap) |
| **Call Stack & Registers** | Mỗi tiến trình có ít nhất một Stack và tập thanh ghi của luồng chính | Mỗi luồng sở hữu một Stack riêng biệt, con trỏ Stack (`RSP`), và tập thanh ghi CPU riêng |
| **Thời gian khởi tạo** | Chậm (phải cấp phát VAS mới, khởi tạo bảng trang MMU, nạp mã từ đĩa) | Nhanh hơn từ 10 đến 100 lần (tận dụng lại không gian địa chỉ hiện có) |
| **Chi phí Chuyển ngữ cảnh** | Rất cao: phải lưu/tải thanh ghi, chuyển thanh ghi bảng trang `CR3`, làm mất hiệu lực bộ đệm TLB | Nhẹ hơn nhiều: giữ nguyên không gian nhớ và bảng trang, chỉ đổi thanh ghi CPU và Stack Pointer |
| **Giao tiếp liên lạc** | Phức tạp qua IPC: Unix Sockets, Pipes, Shared Memory, Message Queues | Rất nhanh qua biến toàn cục hoặc con trỏ Heap dùng chung (cần cơ chế đồng bộ) |
| **Khả năng chịu lỗi** | Cách ly lỗi cao: nếu 1 tiến trình gặp Segmentation Fault và sập, các tiến trình khác vẫn an toàn | Nguy cơ cao: 1 luồng gặp lỗi bộ nhớ (Segfault) sẽ kéo sập toàn bộ tiến trình |
| **Cấu trúc quản lý Kernel** | Quản lý bởi `struct task_struct` với `struct mm_struct` độc lập trong Linux | Quản lý bởi `struct task_struct` nhưng chia sẻ chung con trỏ `mm_struct` |

```text
TIẾN TRÌNH (PROCESS)
+------------------------------------------------------------------------+
| Shared Resources: Text Segment | Data | Heap | File Descriptors | PID   |
|                                                                        |
|  +--------------------+  +--------------------+  +--------------------+|
|  | Luồng 1 (Thread 1) |  | Luồng 2 (Thread 2) |  | Luồng 3 (Thread 3) ||
|  | - Registers / PC   |  | - Registers / PC   |  | - Registers / PC   ||
|  | - Private Stack    |  | - Private Stack    |  | - Private Stack    ||
|  | - Thread-Local (TLS)|  | - Thread-Local (TLS)|  | - Thread-Local (TLS)|
|  +--------------------+  +--------------------+  +--------------------+|
+------------------------------------------------------------------------+
```

### Các mô hình liên kết luồng (Threading Models):
- **Mô hình 1:1 (Kernel-Level Threads):** Mỗi luồng người dùng (user thread) tương ứng với đúng một luồng nhân (kernel thread). Đây là mô hình chuẩn mực của Linux (NPTL qua `clone()`) và Windows. Ưu điểm: tận dụng tối đa kiến trúc đa lõi CPU; nhược điểm: chi phí tạo luồng và chuyển ngữ cảnh vẫn phụ thuộc vào kernel.
- **Mô hình N:1 (User-Level Threads / Green Threads):** Toàn bộ các luồng ứng dụng được quản lý bởi một Runtime bên trong User Space, hệ điều hành chỉ nhìn thấy 1 tiến trình đơn luồng duy nhất. Ưu điểm: chuyển đổi cực nhanh; nhược điểm: khi 1 luồng gọi hàm I/O chặn (blocking system call), toàn bộ tiến trình bị đóng băng.
- **Mô hình M:N (Hybrid / Two-tier):** Ghép M luồng ứng dụng lên N luồng hệ điều hành ($M \ge N$). Tiêu biểu là Goroutines của Go hoặc Virtual Threads (Project Loom) của Java. Go Runtime tự lập lịch hàng triệu goroutines trên một nhóm nhỏ OS threads bằng kỹ thuật Work-Stealing Scheduler, mang lại hiệu năng đồng thời vượt trội.

---

## 3. Vòng đời Tiến trình & 5 Trạng thái Kinh điển

Hệ điều hành quản lý vòng đời của một tiến trình thông qua cấu trúc **PCB (Process Control Block)**, trong Linux là `struct task_struct`.

```text
                    +-------------------+
                    |     MỚI (NEW)     |
                    +---------+---------+
                              | Được chấp nhận (Admitted)
                              v
       Hết lát cắt (Preempt)  +-------------------+
        +---------------------| SẴN SÀNG (READY)  |<-------------------+
        |                     +---------+---------+                    |
        |                               |                              |
        |                               | Được cấp phát CPU (Dispatch) | Hoàn thành I/O
        |                               v                              | (Event occurs)
+-------+-----------+         +-------------------+                    |
| ĐANG CHẠY (RUNNING)|-------->| ĐANG CHỜ (BLOCKED)|--------------------+
+---------+---------+         +-------------------+
          | Yêu cầu I/O / Đợi tài nguyên (Wait for event)
          |
          | Hoàn tất câu lệnh cuối (Exit)
          v
+-------------------+
| KẾT THÚC (TERMINATED)
+-------------------+
```

### Phân tích trạng thái:
1. **New (Khởi tạo):** Tiến trình vừa được tạo ra bằng lời gọi hệ thống (như `fork()` trên POSIX hoặc `CreateProcess()` trên Windows) nhưng chưa được nạp đầy đủ vào hàng đợi sẵn sàng.
2. **Ready (Sẵn sàng):** Tiến trình đã có đầy đủ tài nguyên cần thiết trong bộ nhớ, chỉ đang chờ bộ điều phối (CPU Scheduler) cấp cho một lát cắt thời gian CPU để chạy.
3. **Running (Đang thực thi):** Các lệnh mã máy của tiến trình đang được CPU trực tiếp giải mã và tính toán trong các chu kỳ xung nhịp.
4. **Waiting / Blocked (Chờ / Nghẽn):** Tiến trình không thể chạy vì đang phải chờ một sự kiện ngoại vi hoàn tất: đọc/ghi đĩa SSD, nhận gói tin mạng qua Socket, chờ người dùng nhập bàn phím, hoặc chờ giải phóng khóa Mutex.
5. **Terminated (Kết thúc):** Tiến trình thực thi lệnh `exit()` hoặc bị hệ thống hủy. Tài nguyên bộ nhớ được giải phóng, nhưng mục nhập trong bảng tiến trình vẫn được lưu tạm để gửi mã thoát (exit code) cho tiến trình cha.

> [!WARNING]
> **Hiểm họa Tiến trình Zombie và Orphan:**
> - **Zombie Process (`<defunct>`):** Xảy ra khi tiến trình con kết thúc nhưng tiến trình cha chưa gọi hàm `wait()` hoặc `waitpid()` để đọc mã thoát. Lúc này, tiến trình con không còn tốn RAM hay CPU, nhưng nó chiếm giữ một **PID (Process ID)** trong bảng Process Table của kernel. Nếu hàng nghìn zombie tích tụ, hệ thống sẽ cạn kiệt PID (`PID exhaustion`) và không thể mở thêm bất kỳ phần mềm nào!
> - **Orphan Process (Tiến trình mồ côi):** Xảy ra khi tiến trình cha kết thúc trước tiến trình con. Khi đó, Kernel sẽ tự động chuyển quyền làm cha của tiến trình mồ côi này cho tiến trình gốc `init` hoặc `systemd` (PID 1). PID 1 có trách nhiệm định kỳ gọi `wait()` để dọn dẹp sạch sẽ các tiến trình con khi chúng kết thúc.

---

## 4. Cơ chế Chuyển ngữ cảnh (Context Switch) & Chi phí ẩn

**Chuyển ngữ cảnh (Context Switch)** là chuỗi thao tác của nhân hệ điều hành nhằm dừng việc thực thi một tiến trình/luồng hiện tại, lưu lại trạng thái tính toán của nó, và nạp trạng thái của một tiến trình/luồng khác lên CPU để tiếp tục chạy.

```text
TIẾN TRÌNH A                     KERNEL (HỆ ĐIỀU HÀNH)                  TIẾN TRÌNH B
    |                                     |                                    |
Đang chạy (User Mode)                     |                                    |
    |---- Ngắt Timer / System Call ------>|                                    |
    |                               Lưu trạng thái vào PCB_A                   |
    |                               (Registers, PC, SP)                        |
    |                                     |                                    |
    |                               Chọn Process B                             |
    |                               (CPU Scheduler)                            |
    |                                     |                                    |
    |                               Đổi thanh ghi CR3                          |
    |                               (Chuyển Page Table)                        |
    |                                     |                                    |
    |                               Nạp trạng thái từ PCB_B                    |
    |                                     |                                    |
    |                                     |---- Chuyển sang User Mode -------->|
    |                                     |                               Đang chạy (User Mode)
```

### Bảy bước thực thi vi mô của một Context Switch:
1. **Bẫy vào Kernel Mode (Trap):** Tín hiệu ngắt phần cứng (Timer Interrupt từ Local APIC) hoặc bẫy phần mềm (System Call) khiến CPU chuyển từ Ring 3 (User Space) sang Ring 0 (Kernel Space).
2. **Lưu thanh ghi phần cứng:** CPU tự động đẩy thanh ghi con trỏ ngăn xếp người dùng (`RSP`), con trỏ lệnh (`RIP`), và cờ trạng thái (`RFLAGS`) lên Kernel Stack của luồng hiện tại.
3. **Lưu thanh ghi đa dụng:** Mã hợp ngữ của Kernel lưu các thanh ghi đa năng (`RAX`, `RBX`, `RCX`, `RDX`, `RSI`, `RDI`, `RBP`, `R8`-`R15`) vào cấu trúc `struct pt_regs`.
4. **Bộ điều phối lựa chọn mục tiêu:** Hàm `schedule()` duyệt qua hàng đợi Runqueue và chọn luồng tiếp theo dựa trên chính sách lập lịch (ví dụ Linux CFS).
5. **Chuyển đổi Stack Pointer:** Hàm hợp ngữ `__switch_to` tráo đổi con trỏ ngăn xếp `RSP` từ Kernel Stack của luồng A sang Kernel Stack của luồng B.
6. **Thay đổi Bảng trang (Memory Context Swap):**
   - Nếu chuyển đổi giữa hai luồng trong **cùng một tiến trình**: Bước này được **bỏ qua** vì cả hai luồng dùng chung bảng trang!
   - Nếu chuyển đổi giữa hai **tiến trình khác nhau**: Kernel phải ghi địa chỉ vật lý của bảng trang mới vào thanh ghi điều khiển **`CR3`** (Control Register 3). Thao tác này làm vô hiệu hóa bộ nhớ đệm dịch địa chỉ **TLB (Translation Lookaside Buffer)** (gây hiện tượng *TLB Invalidation / TLB Flush*), trừ khi CPU hỗ trợ tính năng PCID (Process-Context Identifier).
7. **Khôi phục trạng thái & Tiếp tục:** Tải lại tập thanh ghi của tiến trình B và thực thi lệnh hợp ngữ `sysret` hoặc `iret` để hạ quyền về Ring 3, trao quyền điều khiển cho tiến trình B.

### Công thức tính toán Hiệu suất CPU và Lãng phí Chuyển ngữ cảnh:
Gọi $T_{\text{quantum}}$ là thời lượng một lát cắt thời gian CPU cấp cho tiến trình, và $T_{\text{overhead}}$ là tổng thời gian tiêu tốn cho một lần chuyển ngữ cảnh. Hiệu suất sử dụng CPU hữu ích $\eta$ được xác định theo công thức:

$$\eta = \frac{T_{\text{quantum}}}{T_{\text{quantum}} + T_{\text{overhead}}}$$

> [!IMPORTANT]
> **Hiện tượng Bão Chuyển Ngữ Cảnh (Context Switch Storm):**
> Trong điều kiện hoạt động bình thường, $T_{\text{quantum}} = 10\text{ ms} = 10,000\,\mu\text{s}$ và $T_{\text{overhead}} \approx 5\,\mu\text{s}$, hiệu suất đạt:
> $$\eta = \frac{10000}{10000 + 5} \approx 99.95\%$$
> Tuy nhiên, nếu một kỹ sư cấu hình ứng dụng backend tạo ra 10,000 OS threads cùng chạy tranh chấp trên một máy chủ 8 nhân CPU, các luồng liên tục bị ngắt nhả và tranh giành khóa. Khi đó $T_{\text{quantum}}$ thực tế giảm chỉ còn $50\,\mu\text{s}$, trong khi $T_{\text{overhead}}$ vọt lên $25\,\mu\text{s}$ do tranh chấp hàng đợi:
> $$\eta = \frac{50}{50 + 25} \approx 66.7\%$$
> Hơn **33% toàn bộ năng lực CPU máy chủ bị thiêu rụi** chỉ để lưu và nạp thanh ghi mà không xử lý được bất kỳ logic nghiệp vụ nào!
> Chưa kể chi phí lạnh bộ nhớ cache (**Cache Coldness Penalty**): Khi tiến trình mới vào chạy, toàn bộ bộ nhớ đệm CPU L1/L2/L3 đang chứa dữ liệu của tiến trình cũ. Tiến trình mới liên tục bị *Cache Miss*, khiến tốc độ xử lý lệnh ban đầu chậm đi hàng trăm chu kỳ xung nhịp.

---

## 5. Giám sát & Debug Thực tế trên Hệ điều hành Linux

### 1. Phân biệt Voluntary vs Involuntary Context Switch:
Truy cập thông tin chi tiết của một tiến trình bất kỳ qua tệp ảo `/proc/[pid]/status`:
```bash
$ cat /proc/12345/status | grep ctxt
voluntary_ctxt_switches:    45210
nonvoluntary_ctxt_switches: 320
```
- **`voluntary_ctxt_switches` (Tự nguyện chuyển ngữ cảnh):** Luồng chủ động nhường CPU khi cần chờ một tài nguyên chưa sẵn sàng: chờ đọc đĩa, gọi `sleep()`, chờ khóa Mutex, hoặc đợi dữ liệu từ mạng.
- **`nonvoluntary_ctxt_switches` (Cưỡng bức chuyển ngữ cảnh):** Luồng bị Kernel tước đoạt quyền CPU khi hết lát cắt thời gian (*quantum expired*) hoặc có một tiến trình ưu tiên cao hơn xuất hiện trong hàng đợi Ready. Nếu chỉ số này cao đột biến, hệ thống đang bị quá tải CPU (CPU Bottleneck).

### 2. Giám sát thời gian thực với `pidstat`:
```bash
# Giám sát tần suất chuyển ngữ cảnh mỗi 1 giây
$ pidstat -w 1

# Kết quả phân tích:
# UID       PID   cswch/s  nvcswch/s  Command
# 1000     4512    850.00      12.00  node
# 1000     4590     20.00    1450.00  ffmpeg
```
Ở ví dụ trên, tiến trình `ffmpeg` đang xử lý video nặng (CPU-bound) nên bị cưỡng bức chuyển ngữ cảnh tới 1,450 lần/giây (`nvcswch/s`), trong khi `node` xử lý web socket (I/O-bound) chủ yếu tự nguyện nhường CPU (`cswch/s`).

### 3. Kiểm tra không gian bộ nhớ qua `/proc/[pid]/maps`:
```bash
$ cat /proc/self/maps | head -n 5
00400000-0040c000 r-xp 00000000 08:01 131234  /bin/cat     # Text (r-x)
0060b000-0060c000 r--p 0000b000 08:01 131234  /bin/cat     # Data (r--)
0060c000-0060d000 rw-p 0000c000 08:01 131234  /bin/cat     # BSS (rw-)
019a2000-019c3000 rw-p 00000000 00:00 0         [heap]       # Heap (rw-)
7ffd8c21a000-7ffd8c23b000 rw-p 00000000 00:00 0 [stack]      # Stack (rw-)
```

---

## 6. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

### Câu 1: Tại sao chuyển ngữ cảnh giữa 2 luồng trong cùng tiến trình lại có chi phí rẻ hơn đáng kể so với chuyển ngữ cảnh giữa 2 tiến trình khác nhau?
> **Đáp án:** Khi chuyển đổi giữa 2 luồng cùng tiến trình, cả hai luồng dùng chung không gian địa chỉ ảo (VAS) và bảng trang. Do đó, Kernel không cần phải ghi đè thanh ghi bảng trang `CR3` và không làm mất hiệu lực bộ nhớ đệm dịch địa chỉ phần cứng (TLB). Ngược lại, chuyển giữa 2 tiến trình bắt buộc phải đổi `CR3`, dẫn tới xả sạch TLB (TLB Flush) và chịu tổn thất nặng nề từ hiện tượng Cache Coldness.

### Câu 2: Trong kiến trúc máy tính 64-bit, nếu một luồng trong tiến trình vô tình truy cập vào địa chỉ con trỏ `NULL` (0x00000000), điều gì sẽ xảy ra ở cấp độ phần cứng và Kernel?
> **Đáp án:** Địa chỉ `0x0` nằm trong vùng nhớ không được ánh xạ quyền truy cập trong bảng trang (PTE có bit `Present = 0`). Phần cứng MMU phát hiện lỗi và kích hoạt ngắt ngoại lệ **Page Fault Exception (Vector 14)**. Kernel nhận ngắt, tra cứu danh sách `vm_area_struct` và xác định đây là hành vi truy cập trái phép. Kernel lập tức gửi tín hiệu **`SIGSEGV` (Segmentation Fault)** đến tiến trình. Vì lỗi xảy ra ở cấp độ không gian địa chỉ của tiến trình cha, toàn bộ các luồng khác trong cùng tiến trình đó đều bị hệ điều hành tiêu diệt.

### Câu 3: Kích thước mặc định của Stack cho mỗi luồng là bao nhiêu và nguy cơ khi tạo quá nhiều luồng là gì?
> **Đáp án:** Trên Linux 64-bit, kích thước Stack mặc định của một luồng thường là 8 MB (có thể xem qua `ulimit -s`); trên Windows và .NET, mặc định là 1 MB. Nếu ứng dụng backend tạo 5,000 luồng theo mô hình "one-thread-per-request", riêng bộ nhớ ảo dành cho Stack đã ngốn $5,000 \times 8\text{ MB} = 40\text{ GB}$ không gian ảo! Điều này gây cạn kiệt bộ nhớ ảo hoặc RAM vật lý và gây nghẽn nghiêm trọng do bão chuyển ngữ cảnh.

### Câu 4: Một tiến trình chuyển từ trạng thái `Running` sang `Ready` trong trường hợp nào?
> **Đáp án:** Xảy ra khi tiến trình bị **chiếm quyền (Preempted)**: hoặc do bộ đếm thời gian (Hardware Timer) báo hiệu tiến trình đã sử dụng hết lát cắt thời gian (`Time Slice / Quantum`), hoặc do bộ điều phối phát hiện có một tiến trình khác có mức độ ưu tiên cao hơn (Higher Priority) vừa gia nhập hàng đợi Ready.

<!-- studxy-os-v4 -->
"""";

    private static string GetLesson12Markdown() => """"
# Bài 1.2: Thuật toán Điều phối CPU (FCFS, SJF, Round Robin & MLFQ)

> [!NOTE]
> **Khái niệm then chốt:** Một hệ thống máy tính hiện đại có thể chỉ có 8 nhân CPU vật lý nhưng lại chạy đồng thời hàng trăm tiến trình và hàng nghìn luồng. Trái tim của khả năng đa nhiệm này chính là **Bộ điều phối CPU (CPU Scheduler)** — thành phần nhân Kernel chịu trách nhiệm phân chia tài nguyên tính toán một cách công bằng, hiệu quả và tối ưu hóa độ trễ phản hồi.

> [!TIP]
> **Hình ảnh trực quan:** Hãy hình dung CPU Scheduler giống như nhân viên trực quầy vé máy bay tại sân bay quốc tế:
> - Nếu phục vụ theo kiểu ai đến trước mua trước (**FCFS**): Một vị khách mua vé cho cả đoàn 50 người sẽ khiến 20 vị khách đi một mình phía sau bị trễ chuyến bay (Hiệu ứng đoàn tàu - *Convoy Effect*).
> - Nếu ưu tiên khách làm thủ tục nhanh nhất (**SJF**): Khách có hành lý cồng kềnh có thể bị mắc kẹt cả ngày không bao giờ được phục vụ (*Starvation*).
> - Nếu xoay vòng mỗi khách 2 phút (**Round Robin**): Mọi người đều được hỏi han nhanh chóng, tạo cảm giác hệ thống luôn phản hồi mượt mà.

---

## 1. Các Đại lượng Đo lường Hiệu năng Bộ Điều phối

Để đánh giá một thuật toán điều phối là tốt hay xấu, các nhà khoa học máy tính sử dụng các đại lượng toán học chuẩn xác sau:

```text
Thời gian:   Arrival (T_arr)           Start (T_start)                 Completion (T_comp)
                  |                          |                                 |
                  v                          v                                 v
Tiến trình:       +--------------------------+=================================+
                  |   Chờ trong Ready Queue  |       Thực thi trên CPU         |
                  +--------------------------+=================================+
                  <------------------- Response Time ----------------->
                  <----------------------------- Turnaround Time ------------------------------>
```

### Các công thức cốt lõi:
1. **Turnaround Time (Thời gian hoàn thành toàn phần):** Tổng thời gian từ lúc tiến trình xuất hiện trong hệ thống cho đến khi hoàn thành xong câu lệnh cuối cùng:
   $$T_{\text{turnaround}} = T_{\text{completion}} - T_{\text{arrival}}$$
2. **Waiting Time (Thời gian chờ đợi):** Tổng thời gian tiến trình phải nằm yên trong hàng đợi Ready để chờ được cấp CPU:
   $$T_{\text{waiting}} = T_{\text{turnaround}} - T_{\text{burst}} = (T_{\text{completion}} - T_{\text{arrival}}) - T_{\text{burst}}$$
3. **Response Time (Thời gian phản hồi):** Khoảng thời gian từ lúc tiến trình xuất hiện đến lần đầu tiên CPU chạm vào thực thi nó:
   $$T_{\text{response}} = T_{\text{firstRun}} - T_{\text{arrival}}$$
4. **Throughput (Thông lượng):** Số lượng tiến trình hoàn thành trên một đơn vị thời gian (ví dụ: tiến trình/giây).
5. **CPU Utilization (Tỷ lệ sử dụng CPU):** Tỷ lệ phần trăm thời gian CPU bận rộn xử lý công việc hữu ích (thường hướng tới $40\% - 90\%$).

> [!IMPORTANT]
> **Định luật Little (Little's Law) trong Hệ thống Hàng đợi:**
> Trong điều kiện hệ thống ổn định, số lượng tiến trình trung bình đang nằm chờ trong hàng đợi Ready ($L$) bằng tích của tốc độ xuất hiện tiến trình mới ($\lambda$) và thời gian chờ trung bình ($W$):
> $$L = \lambda \times W$$
> Định luật này chứng minh rằng nếu muốn giảm độ dài hàng đợi chờ đợi $L$, ta bắt buộc phải tăng thông lượng xử lý hoặc giảm thời gian trễ trung bình $W$.

---

## 2. Bốn Thuật toán Điều phối CPU Kinh điển

| Thuật toán | Cơ chế Preemption | Ưu điểm cốt lõi | Nhược điểm lớn nhất |
|---|---|---|---|
| **FCFS (First-Come, First-Served)** | Non-preemptive | Đơn giản, không overhead, không bao giờ bị Starvation | Hiệu ứng đoàn tàu (**Convoy Effect**): tác vụ ngắn bị nghẽn sau tác vụ dài |
| **SJF / SRTF** | Non-preemptive (SJF) hoặc Preemptive (SRTF) | Tối ưu hóa tuyệt đối về thời gian hoàn thành trung bình (**Optimal Turnaround Time**) | Dễ gây đói tài nguyên (**Starvation**) cho tác vụ dài; không biết trước Burst Time |
| **Round Robin (RR)** | Preemptive (theo Quantum $q$) | Công bằng, tối ưu **Response Time** cho ứng dụng đồ họa/web tương tác | Nếu $q$ quá nhỏ $\to$ bão Context Switch; nếu $q$ quá lớn $\to$ thoái hóa thành FCFS |
| **MLFQ** | Preemptive đa tầng | Cân bằng tự động giữa tác vụ I/O tương tác và tác vụ CPU nặng | Phức tạp trong cấu hình số hàng đợi và cần cơ chế **Priority Boost** chống Starvation |

### Phân tích sâu từng thuật toán:

### 1. FCFS và Hiện tượng Convoy Effect:
Tiến trình nào đến trước trong Ready Queue thì được chiếm CPU chạy cho đến khi hoàn tất hoặc tự nhường.
Giả sử có 3 tiến trình: $P_1 (Burst = 24\text{ ms})$, $P_2 (Burst = 3\text{ ms})$, $P_3 (Burst = 3\text{ ms})$ cùng đến tại thời điểm $t = 0$.
- Nếu thứ tự chạy là $P_1 \to P_2 \to P_3$:
  - Waiting time: $P_1 = 0$, $P_2 = 24$, $P_3 = 27$.
  - Average Waiting Time: $\frac{0 + 24 + 27}{3} = 17\text{ ms}$.
- Nếu thứ tự chạy là $P_2 \to P_3 \to P_1$:
  - Waiting time: $P_2 = 0$, $P_3 = 3$, $P_1 = 6$.
  - Average Waiting Time: $\frac{0 + 3 + 6}{3} = 3\text{ ms}$ (Nhanh hơn gần 6 lần!).
Hiện tượng $P_1$ chạy dài khiến các tiến trình nhẹ phải lê bước sau lưng gọi là **Convoy Effect**.

### 2. SJF / SRTF và Dự đoán CPU Burst bằng Làm mịn Hàm mũ:
SJF chọn tiến trình có CPU burst ngắn nhất chạy trước. Phiên bản chiếm quyền của nó gọi là **SRTF (Shortest Remaining Time First)**: khi có tiến trình mới xuất hiện với thời gian còn lại ngắn hơn tiến trình đang chạy, CPU lập tức bị chiếm quyền.
Do hệ điều hành không thể biết trước chính xác thời gian một tiến trình sẽ chạy trong bao lâu, nó dùng kỹ thuật **Exponential Smoothing (Làm mịn hàm mũ)** để dự đoán burst tiếp theo:
$$\tau_{n+1} = \alpha t_n + (1 - \alpha) \tau_n$$
Trong đó:
- $t_n$: Thời gian burst thực tế của lần chạy thứ $n$.
- $\tau_n$: Thời gian burst được dự đoán ở lần thứ $n$.
- $\alpha \in [0, 1]$: Trọng số lịch sử gần (thường chọn $\alpha = 0.5$).

### 3. Round Robin (RR) và Nghệ thuật Chọn Lát cắt Thời gian ($q$):
RR gán cho mỗi tiến trình một khoảng thời gian tối đa gọi là **Time Slice / Quantum ($q$)**. Khi hết $q$, ngắt đồng hồ Timer Interrupt sẽ đưa tiến trình về cuối hàng đợi Ready.
- **Nếu $q \to \infty$:** Round Robin biến thành FCFS.
- **Nếu $q \to 0$:** Hệ thống tiếp cận mô hình lý tưởng Processor Sharing (mỗi tiến trình cảm giác như có riêng một CPU tốc độ $\frac{1}{N}$), nhưng chi phí chuyển ngữ cảnh $T_{\text{overhead}}$ sẽ nuốt chửng toàn bộ CPU.
- **Quy tắc thực nghiệm:** Khoảng $80\%$ CPU bursts của các tiến trình nên ngắn hơn độ dài của $q$. Giá trị chuẩn thường chọn là $10\text{ ms} - 50\text{ ms}$.

### 4. MLFQ (Multi-Level Feedback Queue) - Năm Quy tắc Vàng:
MLFQ là đỉnh cao của bộ điều phối tương tác, được xây dựng trên 5 quy tắc chuẩn của Remzi Arpaci-Dusseau:
1. **Quy tắc 1:** Nếu $\text{Priority}(A) > \text{Priority}(B)$, $A$ được chạy, $B$ không chạy.
2. **Quy tắc 2:** Nếu $\text{Priority}(A) == \text{Priority}(B)$, $A$ và $B$ luân phiên theo Round Robin với quantum của hàng đợi đó.
3. **Quy tắc 3:** Khi một tiến trình mới tham gia hệ thống, nó được xếp vào hàng đợi có mức ưu tiên cao nhất (**Top Priority Queue**).
4. **Quy tắc 4 (Chống gian lận - Anti-Gaming):** Khi tiến trình tích lũy đủ thời gian chạy ở một mức ưu tiên (dù nó có chủ động nhường CPU ngắt quãng để làm I/O), mức ưu tiên của nó sẽ bị hạ xuống một bậc.
5. **Quy tắc 5 (Chống đói CPU - Priority Boost):** Sau một chu kỳ thời gian $S$ cố định, toàn bộ tiến trình trong hệ thống sẽ được đưa trở lại hàng đợi cao nhất. Điều này giải quyết triệt để vấn đề Starvation và thích ứng khi một tiến trình chuyển từ tính toán nặng sang tương tác người dùng.

---

## 3. Linux Completely Fair Scheduler (CFS) - Chuẩn Công nghiệp

Kể từ nhân Linux 2.6.23, **CFS (Completely Fair Scheduler)** đã trở thành bộ điều phối mặc định cho các tiến trình thông thường (`SCHED_OTHER`).

```text
HÀNG ĐỢI CFS (RED-BLACK TREE)
                       [vruntime = 15ms]
                             /   \
                            /     \
             [vruntime = 10ms]    [vruntime = 25ms]
                   /
                  /
       (rb_leftmost) -> [vruntime = 4ms]  <--- Luồng tiếp theo được chọn chạy!
```

### Nguyên lý hoạt động của CFS:
1. **Virtual Runtime (`vruntime`):** Mỗi tiến trình được gán một bộ đếm `vruntime` đại diện cho lượng thời gian ảo nó đã chiếm dụng CPU.
   $$\Delta \text{vruntime} = \Delta \text{exec\_time} \times \frac{\text{NICE\_0\_LOAD}}{\text{weight}}$$
   - Tiến trình có mức ưu tiên cao (giá trị `nice` nhỏ, trọng số `weight` lớn) sẽ có `vruntime` tăng rất chậm.
   - Tiến trình có mức ưu tiên thấp (giá trị `nice` lớn) sẽ có `vruntime` tăng rất nhanh.
2. **Cây Đỏ-Đen (Red-Black Tree):** Thay vì dùng danh sách hàng đợi phẳng, CFS lưu các tiến trình có thể chạy trong một cây nhị phân cân bằng Red-Black Tree sắp xếp theo thứ tự `vruntime` tăng dần:
   - Node bên trái cùng (**`rb_leftmost`**) có `vruntime` bé nhất $\to$ luôn được chọn để chạy với chi phí tra cứu cực nhanh **$O(1)$**.
   - Sau khi chạy xong lát cắt, cập nhật lại `vruntime` và chèn lại vào cây với độ phức tạp **$O(\log N)$**.
3. **Mức ưu tiên `nice`:** Có dải giá trị từ $-20$ (ưu tiên cao nhất) đến $+19$ (ưu tiên thấp nhất, mức 0 là mặc định). Mỗi nấc chênh lệch tương ứng với việc thay đổi khoảng $10\%$ lượng thời gian CPU được phân bổ.

---

## 4. Lần vết Thực tế Từng bước (Step-by-Step Gantt Trace)

Hãy xem xét kịch bản kinh điển với 4 tiến trình sau:

| Tiến trình | Thời điểm đến ($T_{\text{arr}}$) | Thời gian chạy ($T_{\text{burst}}$) |
|---|---|---|
| **P1** | 0 ms | 8 ms |
| **P2** | 1 ms | 4 ms |
| **P3** | 2 ms | 9 ms |
| **P4** | 3 ms | 5 ms |

### 1. FCFS (First-Come, First-Served):
- Trình tự thực thi: `P1 (0..8) -> P2 (8..12) -> P3 (12..21) -> P4 (21..26)`
- $T_{\text{completion}}$: $P_1 = 8, P_2 = 12, P_3 = 21, P_4 = 26$.
- $T_{\text{turnaround}} = T_{\text{completion}} - T_{\text{arr}}$:
  - $P_1 = 8 - 0 = 8\text{ ms}$
  - $P_2 = 12 - 1 = 11\text{ ms}$
  - $P_3 = 21 - 2 = 19\text{ ms}$
  - $P_4 = 26 - 3 = 23\text{ ms}$
- **Average Turnaround Time:** $\frac{8 + 11 + 19 + 23}{4} = \frac{61}{4} = 15.25\text{ ms}$.
- $T_{\text{waiting}} = T_{\text{turnaround}} - T_{\text{burst}}$:
  - $P_1 = 8 - 8 = 0\text{ ms}$
  - $P_2 = 11 - 4 = 7\text{ ms}$
  - $P_3 = 19 - 9 = 10\text{ ms}$
  - $P_4 = 23 - 5 = 18\text{ ms}$
- **Average Waiting Time:** $\frac{0 + 7 + 10 + 18}{4} = \frac{35}{4} = 8.75\text{ ms}$.

### 2. SRTF (Shortest Remaining Time First - Preemptive):
- Tại $t = 0$: Chỉ có $P_1$ chạy.
- Tại $t = 1$: $P_2$ đến ($Burst = 4$). $P_1$ còn lại $7\text{ ms} > 4\text{ ms} \to P_2$ chiếm quyền!
- Tại $t = 2$: $P_3$ đến ($Burst = 9$). $P_2$ còn lại $3\text{ ms} < 9\text{ ms} \to P_2$ chạy tiếp.
- Tại $t = 3$: $P_4$ đến ($Burst = 5$). $P_2$ còn lại $2\text{ ms} < 5\text{ ms} \to P_2$ chạy tiếp đến $t = 5$ (xong $P_2$).
- Tại $t = 5$: Các tiến trình còn lại: $P_4 (5\text{ ms}), P_1 (7\text{ ms}), P_3 (9\text{ ms}) \to P_4$ chạy từ 5..10 (xong $P_4$).
- Tại $t = 10$: $P_1 (7\text{ ms})$ ngắn hơn $P_3 (9\text{ ms}) \to P_1$ chạy từ 10..17 (xong $P_1$).
- Tại $t = 17$: $P_3$ chạy từ 17..26 (xong $P_3$).
- Sơ đồ Gantt: `P1 (0..1) -> P2 (1..5) -> P4 (5..10) -> P1 (10..17) -> P3 (17..26)`
- $T_{\text{completion}}$: $P_2 = 5, P_4 = 10, P_1 = 17, P_3 = 26$.
- $T_{\text{turnaround}}$:
  - $P_1 = 17 - 0 = 17\text{ ms}$
  - $P_2 = 5 - 1 = 4\text{ ms}$
  - $P_3 = 26 - 2 = 24\text{ ms}$
  - $P_4 = 10 - 3 = 7\text{ ms}$
- **Average Turnaround Time:** $\frac{17 + 4 + 24 + 7}{4} = \frac{52}{4} = 13.0\text{ ms}$ (Tối ưu nhất!).
- **Average Waiting Time:** $\frac{(17-8) + (4-4) + (24-9) + (7-5)}{4} = \frac{9 + 0 + 15 + 2}{4} = 6.5\text{ ms}$.

### 3. Round Robin với Quantum $q = 3\text{ ms}$:
- Sơ đồ thực thi qua các lát cắt:
  - $t = 0..3$: $P_1$ chạy 3ms (còn 5ms). Hàng đợi: $P_2, P_3, P_4, P_1$.
  - $t = 3..6$: $P_2$ chạy 3ms (còn 1ms). Hàng đợi: $P_3, P_4, P_1, P_2$.
  - $t = 6..9$: $P_3$ chạy 3ms (còn 6ms). Hàng đợi: $P_4, P_1, P_2, P_3$.
  - $t = 9..12$: $P_4$ chạy 3ms (còn 2ms). Hàng đợi: $P_1, P_2, P_3, P_4$.
  - $t = 12..15$: $P_1$ chạy 3ms (còn 2ms). Hàng đợi: $P_2, P_3, P_4, P_1$.
  - $t = 15..16$: $P_2$ chạy 1ms (**Hoàn thành $P_2$ tại $t = 16$**).
  - $t = 16..19$: $P_3$ chạy 3ms (còn 3ms).
  - $t = 19..21$: $P_4$ chạy 2ms (**Hoàn thành $P_4$ tại $t = 21$**).
  - $t = 21..23$: $P_1$ chạy 2ms (**Hoàn thành $P_1$ tại $t = 23$**).
  - $t = 23..26$: $P_3$ chạy 3ms (**Hoàn thành $P_3$ tại $t = 26$**).
- $T_{\text{turnaround}}$:
  - $P_1 = 23 - 0 = 23\text{ ms}$
  - $P_2 = 16 - 1 = 15\text{ ms}$
  - $P_3 = 26 - 2 = 24\text{ ms}$
  - $P_4 = 21 - 3 = 18\text{ ms}$
- **Average Turnaround Time:** $\frac{23 + 15 + 24 + 18}{4} = \frac{80}{4} = 20.0\text{ ms}$.
- **Đánh đổi:** Round Robin có Turnaround Time trung bình kém hơn SRTF, nhưng đổi lại Response Time của các tiến trình tương tác ($P_2, P_3, P_4$) cực kỳ nhanh: $P_2$ chỉ đợi 2ms là được chạy lần đầu!

---

## 5. Giám sát & Tối ưu Thực chiến trên Hệ thống Linux

### 1. Kiểm tra chính sách lập lịch bằng `chrt`:
```bash
# Xem chính sách lập lịch của tiến trình
$ chrt -p 1234
pid 1234's current scheduling policy: SCHED_OTHER
pid 1234's current scheduling priority: 0

# Đổi sang chính sách Real-Time Round Robin (SCHED_RR) với ưu tiên 50 (cần quyền root)
$ sudo chrt -r -p 50 1234
```

### 2. Quản lý mức ưu tiên bằng `nice` và `renice`:
```bash
# Khởi chạy tác vụ backup nén dữ liệu với ưu tiên thấp nhất (+19)
$ nice -n 19 tar -czf backup.tar.gz /var/data

# Tăng ưu tiên cho database engine đang chạy (từ nice 0 thành -10)
$ sudo renice -n -10 -p $(pgrep postgres)
```

### 3. Sự cố CPU Throttling trong Kubernetes / Docker Container:
Trong Kubernetes, khi bạn cấu hình giới hạn CPU:
```yaml
resources:
  limits:
    cpu: "500m"  # Tương đương 0.5 core CPU
```
Linux Kernel áp dụng cơ chế cgroups CFS Bandwidth Control:
- `cpu.cfs_period_us = 100000` ($100\text{ ms}$)
- `cpu.cfs_quota_us = 50000` ($50\text{ ms}$)
Nếu ứng dụng backend đa luồng (như Java / Go / .NET) tạo ra 8 luồng cùng tính toán, nó sẽ đốt sạch $50\text{ ms}$ quota chỉ trong vòng $6.25\text{ ms}$ đầu tiên của chu kỳ! Suốt $93.75\text{ ms}$ còn lại, toàn bộ container bị **bóp nghẹt (CPU Throttled)**, dẫn tới hiện tượng *latency spikes* (độ trễ p99 vọt lên hàng trăm ms) dù mức sử dụng CPU trung bình hiển thị trên Grafana chỉ mới đạt 30%!

---

## 6. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

### Câu 1: Tại sao thuật toán Shortest Job First (SJF) được chứng minh toán học là tối ưu về Average Waiting Time nhưng không thể dùng làm bộ điều phối chính trong hệ điều hành đa năng?
> **Đáp án:** Vì hai lý do sống còn:
> 1. Hệ điều hành không thể biết trước chính xác độ dài CPU Burst của một tiến trình khi người dùng khởi chạy.
> 2. Gây ra hiện tượng **Starvation (Đói CPU vĩnh viễn)**: Nếu người dùng liên tục bấm phím hoặc các tiến trình nền ngắn liên tục xuất hiện, các tiến trình xử lý tính toán dài (như render video, nén file) sẽ không bao giờ được cấp CPU.

### Câu 2: Trong thuật toán Round Robin, điều gì sẽ xảy ra nếu ta chọn lát cắt thời gian Quantum $q$ bằng $0.001\text{ ms}$?
> **Đáp án:** Nếu $q = 0.001\text{ ms} = 1\,\mu\text{s}$, trong khi chi phí một lần chuyển ngữ cảnh $T_{\text{overhead}}$ mất từ $3\,\mu\text{s} - 5\,\mu\text{s}$, hệ thống sẽ dành hơn $80\%$ thời gian xung nhịp chỉ để lưu và nạp thanh ghi. CPU sẽ rơi vào tình trạng quá tải nghiêm trọng, thông lượng hữu ích sụt giảm gần như bằng 0.

### Câu 3: Thuật toán MLFQ giải quyết bài toán "Gian lận thời gian CPU" (Gaming the Scheduler) như thế nào?
> **Đáp án:** Một tiến trình gian xảo có thể chạy $99\%$ thời lượng của quantum rồi chủ động gọi một lệnh I/O giả để nhường CPU, nhằm giữ nguyên mức ưu tiên cao nhất ở hàng đợi đỉnh. Quy tắc 4 của MLFQ sửa đổi cơ chế này bằng cách: **Tính lũy kế tổng thời gian chạy** của tiến trình tại mức ưu tiên đó. Dù tiến trình có nhường CPU bao nhiêu lần, khi tổng thời gian CPU cộng dồn đạt đủ quota, nó vẫn bị giáng cấp xuống hàng đợi dưới.

### Câu 4: Trong hệ điều hành Linux CFS, nếu hai tiến trình $A$ có `nice = 0` và $B$ có `nice = 5` cùng chạy tính toán 100% CPU, tiến trình nào sẽ có `vruntime` tăng nhanh hơn?
> **Đáp án:** Tiến trình $B$ (nice = 5) có mức ưu tiên thấp hơn, do đó trọng số `weight` của nó nhỏ hơn. Theo công thức:
> $$\Delta \text{vruntime} = \Delta \text{exec\_time} \times \frac{\text{NICE\_0\_LOAD}}{\text{weight}}$$
> `vruntime` của tiến trình $B$ sẽ tăng nhanh hơn nhiều so với $A$. Vì CFS luôn ưu tiên chọn tiến trình có `vruntime` nhỏ nhất để cấp CPU, tiến trình $A$ sẽ nhận được nhiều thời gian thực thi thực tế trên CPU hơn $B$.

<!-- studxy-os-v4 -->
"""";

    private static string GetLesson21Markdown() => """"
# Bài 2.1: Race Condition, Critical Section, Mutex & Semaphore

> [!NOTE]
> **Khái niệm then chốt:** Trong môi trường đa luồng (Multi-threading) hoặc đa lõi (Multi-core), việc nhiều luồng cùng truy cập và chỉnh sửa một vùng nhớ dữ liệu chia sẻ mà không có cơ chế điều phối sẽ dẫn tới **Race Condition (Tranh chấp dữ liệu)** — một trong những loại lỗi phần mềm quái đản và khó tái hiện nhất trong lịch sử công nghệ thông tin.

> [!TIP]
> **Hình ảnh trực quan:** Hãy hình dung tài khoản ngân hàng của một cặp vợ chồng có số dư 10,000,000 VNĐ:
> - Người chồng đứng tại cây ATM ở Hà Nội rút 6,000,000 VNĐ.
> - Cùng một phần nghìn giây đó, người vợ quẹt thẻ POS tại TP.HCM mua túi xách trị giá 7,000,000 VNĐ.
> - Cả hai máy đều đọc số dư là 10 triệu (hợp lệ $\ge$ số tiền cần rút), trừ tiền và ghi đè số dư mới: một bên ghi 4 triệu, bên kia ghi 3 triệu! Ngân hàng bị thất thoát tiền vì hai thao tác không được thực hiện **Nguyên tử (Atomicity)**!

---

## 1. Bản chất Vi kiến trúc của Race Condition

Tại sao một dòng mã nguồn đơn giản như `counter++` trong ngôn ngữ C/C++/Java/C# lại không an toàn trong môi trường đa luồng?

```text
CẤP ĐỘ MÃ NGUỒN C#:
counter++;

CẤP ĐỘ HỢP NGỮ x86-64 (DISASSEMBLY):
1. mov eax, dword ptr [counter]    ; [READ]  Nạp giá trị từ RAM vào thanh ghi CPU EAX
2. add eax, 1                      ; [MODIFY] Cộng thêm 1 trong bộ số học logic ALU
3. mov dword ptr [counter], eax    ; [WRITE] Ghi giá trị mới từ EAX ngược lại RAM
```

### Kịch bản Xen kẽ (Interleaving) dẫn tới Lỗi Ghi đè (Lost Update):
Giả sử ban đầu `counter = 50`. Luồng A và Luồng B cùng thực thi `counter++`:

```text
Thời gian   Luồng A (Thread A)             Luồng B (Thread B)             RAM (counter)
   t1       mov eax, [counter] (eax_A=50)                                      50
   t2       add eax, 1         (eax_A=51)                                      50
   t3       ------ CONTEXT SWITCH TỪ LUỒNG A SANG LUỒNG B ------               50
   t4                                      mov eax, [counter] (eax_B=50)       50
   t5                                      add eax, 1         (eax_B=51)       50
   t6                                      mov [counter], eax (ghi 51)         51
   t7       ------ CONTEXT SWITCH TỪ LUỒNG B SANG LUỒNG A ------               51
   t8       mov [counter], eax (ghi 51)                                        51 !
```
Kết quả sau khi cả hai luồng tăng biến: `counter = 51` thay vì `52`! Lần tăng của Luồng B đã bị Luồng A ghi đè và biến mất hoàn toàn.

> [!IMPORTANT]
> **Hiện tượng Vi mô: Giao thức MESI & False Sharing trên CPU Cache:**
> Trên phần cứng đa nhân hiện đại, mỗi CPU Core có bộ nhớ đệm L1/L2 riêng. Để giữ cho dữ liệu nhất quán giữa các lõi, phần cứng sử dụng giao thức **MESI (Modified, Exclusive, Shared, Invalid)**:
> Khi Core 1 ghi vào biến, nó phát tín hiệu vô hiệu hóa (*Invalidate*) toàn bộ đường nhớ đệm (**Cache Line** 64 byte) chứa biến đó trên L1 Cache của Core 2. Nếu hai luồng nằm trên hai Core khác nhau liên tục ghi vào hai biến độc lập nhưng vô tình nằm chung một Cache Line 64 byte, hiện tượng **False Sharing** sẽ làm đường truyền bus vi xử lý bị nghẽn nghẹt, hiệu năng sụt giảm nghiêm trọng!

---

## 2. Vùng Tranh Chấp (Critical Section) & Ba Điều Kiện Tiên Quyết

**Vùng tranh chấp (Critical Section - CS)** là đoạn mã nguồn trong chương trình có hành vi truy cập và thao tác trên tài nguyên chia sẻ (biến bộ nhớ, bảng dữ liệu, tệp tin, socket).

```text
do {
    // 1. Entry Section: Xin quyền vào vùng tranh chấp (Lock)
    [ CRITICAL SECTION: Truy cập biến chia sẻ, ghi dữ liệu ]
    // 2. Exit Section: Giải phóng quyền cho luồng khác (Unlock)
    // 3. Remainder Section: Xử lý logic riêng không tranh chấp
} while (true);
```

### Ba điều kiện bắt buộc của một giải pháp Critical Section chuẩn mực:
1. **Mutual Exclusion (Loại trừ lẫn nhau):** Nếu luồng $T_i$ đang thực thi trong vùng Critical Section của nó, thì không có bất kỳ luồng $T_j$ nào khác được phép thực thi trong vùng Critical Section của chúng.
2. **Progress (Tính tiến triển):** Nếu không có luồng nào đang nằm trong Critical Section và có một số luồng đang muốn bước vào, thì chỉ những luồng không nằm trong Remainder Section mới được tham gia vào quyết định chọn ai sẽ vào tiếp theo, và quyết định này **không thể bị trì hoãn vô thời hạn**.
3. **Bounded Waiting (Chờ đợi có giới hạn):** Phải tồn tại một giới hạn xác định về số lần các luồng khác được phép vượt mặt đi vào Critical Section sau khi một luồng đã gửi yêu cầu xin vào và trước khi yêu cầu đó được chấp thuận. Điều kiện này đảm bảo **ngăn chặn triệt để tình trạng Starvation**.

---

## 3. Các Cơ Chế Hỗ Trợ Đồng Bộ từ Phần Cứng

Các thuật toán phần mềm thuần túy (như giải thuật Peterson) rất dễ bị vô hiệu hóa trên các CPU hiện đại do cơ chế đảo thứ tự lệnh (**Out-of-Order Execution**) của trình biên dịch và phần cứng. Do đó, hệ điều hành dựa vào các chỉ thị nguyên tử phần cứng:

### 1. Lệnh Nguyên tử Test-And-Set (TAS):
Lệnh phần cứng đọc một giá trị bộ nhớ và gán giá trị mới là `true` trong đúng một chu kỳ xung nhịp không thể bị ngắt:
```c
bool TestAndSet(bool *target) {
    bool rv = *target;
    *target = true;
    return rv; // Trả về giá trị cũ
}
```

### 2. Lệnh Nguyên tử Compare-And-Swap (CAS):
Cốt lõi của mọi cấu trúc dữ liệu không khóa (**Lock-Free Data Structures**):
```c
int CompareAndSwap(int *value, int expected, int new_value) {
    int temp = *value;
    if (temp == expected) {
        *value = new_value;
    }
    return temp; // Trên x86 là lệnh LOCK CMPXCHG
}
```
Nếu giá trị hiện tại tại ô nhớ khớp với kỳ vọng (`expected`), CPU ghi đè giá trị mới `new_value`. Nếu không khớp (nghĩa là có luồng khác đã can thiệp vào trước), thao tác thất bại và luồng có thể thử lại trong vòng lặp.

> [!WARNING]
> **Vấn đề ABA trong Lập trình Lock-Free:**
> Luồng 1 đọc giá trị ô nhớ là $A$. Luồng 2 chen ngang đổi $A \to B \to A$. Luồng 1 thức dậy thấy ô nhớ vẫn là $A$ nên nghĩ rằng không có gì thay đổi và thực thi lệnh CAS thành công! Tuy nhiên, con trỏ nội tại có thể đã trỏ tới một vùng nhớ đã bị giải phóng (*use-after-free*).
> **Giải pháp:** Sử dụng kỹ thuật **Tagged Pointer / Version Counter**: đi kèm với mỗi giá trị là một số nguyên phiên bản tăng dần ($A_1 \to B_2 \to A_3$). Khi so sánh, CAS kiểm tra cả giá trị lẫn số phiên bản!

### 3. Rào cản Bộ nhớ (Memory Barriers / Memory Fences):
Chỉ thị `MFENCE`, `LFENCE`, `SFENCE` trên kiến trúc x86 buộc CPU phải hoàn thành toàn bộ các lệnh đọc/ghi bộ nhớ phía trước trước khi thực thi bất kỳ lệnh nào phía sau rào cản. Trong C# và Java, từ khóa `volatile` áp dụng cơ chế Acquire/Release semantics để ngăn chặn compiler đảo thứ tự câu lệnh.

---

## 4. Các Công Cụ Đồng Bộ Hóa Phần Mềm (Software Synchronization Primitives)

| Công cụ | Cơ chế chờ đợi | Khái niệm Sở hữu (Ownership) | Trường hợp sử dụng chuẩn |
|---|---|---|---|
| **Spinlock** | Vòng lặp bận (**Busy-waiting**) ngốn 100% core CPU | Có | Vùng tranh chấp cực ngắn ($< 1\,\mu\text{s}$), trong nhân Kernel, trình xử lý ngắt ISR |
| **Mutex** | Ngủ trong hàng đợi Blocked Queue (hạ CPU về 0%) | **Bắt buộc**: Luồng nào Lock thì chỉ luồng đó được Unlock | Bảo vệ biến dùng chung, cấu trúc dữ liệu In-Memory, tài nguyên đơn độc |
| **Semaphore** | Bộ đếm nguyên $S \ge 0$, ngủ khi $S = 0$ | **Không có**: Luồng A có thể Wait, Luồng B có thể Signal | Giới hạn số kết nối đồng thời (Connection Pool), báo hiệu Producer-Consumer |
| **Reader-Writer Lock** | Cho phép nhiều Readers đọc đồng thời, chỉ 1 Writer được ghi | Có (đối với Writer) | Cấu trúc dữ liệu đọc nhiều ghi ít (Cache, bảng định tuyến Routing Table) |

### Chi tiết về Cơ chế Mutex và Linux Futex:
Trên hệ thống Linux hiện đại, Mutex được triển khai thông qua cơ chế **`futex` (Fast Userspace Mutex)**:
- Ở trường hợp không có tranh chấp (*Uncontended Case*): Luồng lấy được khóa hoàn toàn trong User Space bằng một lệnh nguyên tử CAS duy nhất, **không tốn chi phí gọi System Call** vào Kernel!
- Chỉ khi phát hiện có luồng khác đang giữ khóa (*Contended Case*): Luồng mới thực hiện system call `sys_futex()` để yêu cầu Kernel đưa mình vào danh sách ngủ (Wait Queue), nhường CPU cho tiến trình khác.

### Chi tiết về Semaphore (Dijkstra):
Semaphore là một biến đếm nguyên không âm $S$ đi kèm với 2 thao tác nguyên tử:
1. **`wait(S)` (hoặc $P(S)$):**
   ```c
   wait(S) {
       while (S <= 0) {
           // Đưa luồng vào hàng đợi ngủ Blocked Queue
       }
       S--;
   }
   ```
2. **`signal(S)` (hoặc $V(S)$):**
   ```c
   signal(S) {
       S++;
       // Đánh thức một luồng đang ngủ trong hàng đợi (nếu có)
   }
   ```

### Bài toán Bộ đệm Giới hạn (Bounded Buffer / Producer-Consumer):
Kịch bản kinh điển: Một luồng Sản xuất (Producer) tạo dữ liệu bỏ vào mảng cố định có kích thước $N$, một luồng Tiêu thụ (Consumer) lấy dữ liệu ra xử lý.
Cần sử dụng 3 primitive đồng bộ:
- `Semaphore empty = N;` // Đếm số vị trí còn trống trong bộ đệm
- `Semaphore full = 0;`  // Đếm số phần tử hiện có sẵn để tiêu thụ
- `Mutex mutex = 1;`     // Đảm bảo chỉ 1 người được cập nhật con trỏ mảng tại một thời điểm

```c
// TIẾN TRÌNH PRODUCER:
while (true) {
    item = produce_item();
    wait(empty);  // 1. Chờ cho đến khi có ít nhất 1 chỗ trống
    wait(mutex);  // 2. Khóa vùng đệm để ghi
    insert_item(item);
    signal(mutex);// 3. Mở khóa vùng đệm
    signal(full); // 4. Báo hiệu cho Consumer biết đã có thêm 1 phần tử
}

// TIẾN TRÌNH CONSUMER:
while (true) {
    wait(full);   // 1. Chờ cho đến khi có ít nhất 1 phần tử có sẵn
    wait(mutex);  // 2. Khóa vùng đệm để lấy
    item = remove_item();
    signal(mutex);// 3. Mở khóa vùng đệm
    signal(empty);// 4. Báo hiệu cho Producer biết đã giải phóng 1 chỗ trống
    consume_item(item);
}
```

> [!WARNING]
> **Thứ tự Lời gọi Khóa sống còn để tránh Deadlock:**
> Trong đoạn mã trên, nếu lập trình viên đảo thứ tự hai câu lệnh:
> ```c
> wait(mutex); // NGUY HIỂM CHẾT NGƯỜI!
> wait(empty);
> ```
> Khi bộ đệm bị đầy (`empty = 0`), Producer sẽ giữ chặt `mutex` rồi đi ngủ để chờ `empty`. Trong khi đó, Consumer cần `mutex` để lấy hàng ra giải phóng chỗ trống, nhưng `mutex` lại đang bị Producer nắm giữ! Kết quả: Cả Producer và Consumer nhìn nhau đứng im vĩnh viễn (**Deadlock**)!

---

## 5. Hiện tượng Đảo ngược Mức ưu tiên (Priority Inversion) & Bài học Mars Pathfinder

Một trong những sự cố đồng bộ hóa chấn động lịch sử diễn ra vào năm 1997 trên con tàu thám hiểm sao Hỏa **Mars Pathfinder** của NASA.

```text
Luồng H (High Priority)    --- Chờ Mutex M -------------------------> [ĐƯỢC CHẠY TIẾP]
                                  ^                                       ^
Luồng M (Medium Priority)         |   ====================== (Chạy) ===== |
                                  |   (Cướp CPU vì M > L!)                |
Luồng L (Low Priority)     -- Giữ M --                                    nhả M
```

### Kịch bản Thảm họa:
1. Luồng $L$ (Ưu tiên thấp: thu thập dữ liệu khí tượng) lấy khóa Mutex $M$ để ghi dữ liệu vào bus thông tin.
2. Luồng $H$ (Ưu tiên cao: quản lý điều hướng tàu vũ trụ) thức dậy, cần truy cập bus nên yêu cầu khóa Mutex $M$. Vì $L$ đang giữ $M$, luồng $H$ bị đưa vào trạng thái Blocked.
3. Luồng $M$ (Ưu tiên trung bình: truyền thông tin khoa học) thức dậy. Luồng $M$ không hề cần khóa $M$, nhưng vì mức ưu tiên của $M > L$, bộ điều phối CPU lập tức tước quyền CPU của $L$ để trao cho $M$ chạy!
4. **Hậu quả:** Luồng $L$ không được cấp CPU nên không thể chạy xong để nhả khóa $M$. Luồng $H$ (quan trọng nhất hệ thống) bị gián tiếp chặn đứng bởi luồng $M$! Sau vài phút, bộ đếm thời gian giám sát (**Watchdog Timer**) thấy luồng điều hướng $H$ không phản hồi nên kết luận hệ thống bị treo và tự động khởi động lại toàn bộ máy tính trên sao Hỏa nhiều lần mỗi ngày.

### Giải pháp Cứu rỗi: Giao thức Thừa kế Mức ưu tiên (Priority Inheritance Protocol)
Khi một luồng ưu tiên cao $H$ bị nghẽn vì chờ một khóa Mutex do luồng ưu tiên thấp $L$ nắm giữ, hệ điều hành **tạm thời nâng mức ưu tiên của luồng $L$ lên ngang bằng với mức ưu tiên của luồng $H$**. Nhờ đó, luồng $M$ không thể chen ngang cướp CPU của $L$. Ngay sau khi $L$ thực thi xong vùng tranh chấp và nhả khóa Mutex, mức ưu tiên của $L$ được trả về ban đầu, và luồng $H$ lập tức chiếm lấy khóa để chạy trơn tru.

---

## 6. Kỹ Năng Debug & Phát Hiện Race Condition trong Thực Tế

### 1. Sử dụng ThreadSanitizer (TSan):
Công cụ kiểm tra bộ nhớ động mạnh mẽ được tích hợp sẵn trong trình biên dịch GCC, Clang và Go runtime:
```bash
# Trong Golang:
$ go test -race ./...
$ go run -race server.go

# Trong C/C++ với GCC/Clang:
$ gcc -fsanitize=thread -g main.c -o app
$ ./app
# ==================
# WARNING: ThreadSanitizer: data race (pid=12345)
#   Write of size 4 at 0x7ffd8c21a000 by thread T2:
#     #0 thread_worker main.c:42 (app+0x1234)
#   Previous read of size 4 at 0x7ffd8c21a000 by thread T1:
#     #0 thread_reader main.c:28 (app+0x1120)
# ==================
```

### 2. Các nguyên tắc vàng để viết mã an toàn đa luồng:
- **Lock Striping:** Thay vì khóa toàn bộ bảng dữ liệu lớn bằng 1 Mutex duy nhất, hãy băm dữ liệu thành nhiều bucket nhỏ và mỗi bucket có một Mutex riêng (như kiến trúc `ConcurrentDictionary` trong .NET hoặc `ConcurrentHashMap` trong Java).
- **Hạn chế tối đa thời gian giữ khóa:** Đọc dữ liệu ra biến cục bộ, giải phóng Mutex ngay lập tức rồi mới xử lý các thuật toán nặng hoặc gửi mạng. **TUYỆT ĐỐI KHÔNG BAO GIỜ gọi I/O hoặc HTTP API khi đang nắm giữ Mutex!**
- **Ưu tiên cấu trúc dữ liệu bất biến (Immutability):** Dữ liệu chỉ đọc (Read-Only) có thể được hàng triệu luồng đọc song song an toàn mà không cần tốn bất kỳ khóa nào.

---

## 7. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

### Câu 1: Tại sao việc sử dụng biến cờ hiệu kiểm tra thông thường kiểu `while (flag == true)` để làm khóa lại không giải quyết được bài toán Critical Section?
> **Đáp án:** Vì thao tác kiểm tra điều kiện `flag == true` và thao tác gán `flag = true` là hai thao tác tách biệt không có tính nguyên tử. Hai luồng có thể cùng đọc thấy `flag == false` tại cùng một thời điểm, cùng vượt qua vòng lặp kiểm tra và cùng bước vào Critical Section, phá vỡ hoàn toàn tính chất Mutual Exclusion.

### Câu 2: Sự khác biệt bản chất giữa Binary Semaphore và Mutex là gì?
> **Đáp án:** Điểm khác biệt quan trọng nhất là **Khái niệm Sở hữu (Ownership)**. Với Mutex, luồng nào gọi lệnh khóa (`Lock()`) thì bắt buộc chỉ có chính luồng đó mới có quyền gọi lệnh mở khóa (`Unlock()`). Với Binary Semaphore, một luồng có thể gọi `wait()` để chờ tín hiệu, và một luồng hoàn toàn khác có thể gọi `signal()` để đánh thức nó (phục vụ cơ chế đồng bộ hóa luồng).

### Câu 3: Khi nào nên sử dụng Spinlock thay vì Mutex?
> **Đáp án:** Nên dùng Spinlock khi:
> 1. Vùng Critical Section cực kỳ ngắn (chỉ vài phép gán hoặc tính toán dưới $1\,\mu\text{s}$).
> 2. Chạy trên hệ thống máy tính có **nhiều nhân CPU thực sự** (multi-core).
> 3. Không thể đưa luồng vào giấc ngủ (ví dụ: bên trong trình xử lý ngắt Interrupt Service Routine của Kernel). Trong trường hợp này, thời gian bận chờ (busy-wait) của Spinlock còn ngắn hơn thời gian hệ điều hành tiêu tốn để thực hiện một lần chuyển ngữ cảnh context switch khi ngủ.

### Câu 4: Làm thế nào để ngăn chặn hiện tượng Priority Inversion trong các hệ điều hành thời gian thực (RTOS)?
> **Đáp án:** Sử dụng **Giao thức Thừa kế Mức ưu tiên (Priority Inheritance Protocol)** hoặc **Giao thức Trần Mức ưu tiên (Priority Ceiling Protocol)**. Khi luồng ưu tiên thấp đang giữ khóa cần thiết cho luồng ưu tiên cao, mức ưu tiên của luồng thấp sẽ được tự động nâng lên ngang mức ưu tiên cao nhất của các luồng đang chờ khóa, ngăn không cho các luồng trung gian cướp quyền CPU.

<!-- studxy-os-v4 -->
"""";

    private static string GetLesson22Markdown() => """"
# Bài 2.2: Deadlock — 4 Điều kiện Coffman, Phòng tránh & Phục hồi

> [!NOTE]
> **Khái niệm then chốt:** Trong khoa học máy tính, **Deadlock (Khóa chết)** là tình trạng đóng băng vĩnh viễn của một tập hợp các tiến trình hoặc luồng, trong đó mỗi tiến trình đang nắm giữ ít nhất một tài nguyên và vĩnh viễn chờ đợi một tài nguyên khác đang bị chiếm giữ bởi một tiến trình khác trong cùng tập hợp.

> [!TIP]
> **Hình ảnh trực quan:** Câu chuyện hai đoàn tàu đối đầu trên cây cầu đường ray đơn bắc qua hẻm núi sâu:
> - Đoàn tàu A đã đi vào giữa cầu từ phía bờ Tây và cần đi tiếp bờ Đông.
> - Đoàn tàu B đã đi vào giữa cầu từ phía bờ Đông và cần đi tiếp bờ Tây.
> - Cả hai tài xế đều kiên quyết không chịu lùi lại và không thể vượt qua nhau. Cả hai đoàn tàu đứng im bất động vĩnh viễn giữa cầu!

---

## 1. Phân biệt Deadlock, Livelock và Starvation

Rất nhiều kỹ sư nhầm lẫn giữa ba trạng thái bất thường của hệ thống đa nhiệm. Bảng phân tích chuẩn xác:

| Tiêu chí | Deadlock (Khóa chết) | Livelock (Khóa sống) | Starvation (Đói tài nguyên) |
|---|---|---|---|
| **Mức tiêu thụ CPU** | **0% CPU** (Tất cả các luồng rơi vào trạng thái ngủ Blocked Queue) | **100% CPU** (Các luồng liên tục chạy vòng lặp bận thay đổi trạng thái) | Tiến trình bị đói có thể tốn ít CPU, nhưng CPU hệ thống vẫn chạy các tiến trình khác |
| **Thay đổi trạng thái** | Đóng băng tĩnh hoàn toàn, không có bất kỳ lệnh nào được thực thi | Trạng thái luồng thay đổi liên tục nhưng không đạt được bước tiến triển nào | Tiến trình bị hoãn vô thời hạn vì luôn có tiến trình ưu tiên cao hơn vượt mặt |
| **Khả năng tự hồi phục** | Không thể tự thoát nếu không có sự can thiệp cưỡng bức từ bên ngoài | Rất khó thoát trừ khi có cơ chế giải phóng ngẫu nhiên (Random Backoff) | Tự thoát khi các tiến trình ưu tiên cao kết thúc hoặc nhờ cơ chế Aging |
| **Ví dụ đời thực** | Hai người đi qua cửa hẹp cùng lúc, người này nắm áo người kia và cả hai đứng im | Hai người đi đối diện trên vỉa hè: cùng né sang trái, cùng né sang phải hàng chục lần | Người nghèo xếp hàng mua vé luôn bị người có thẻ VIP chen ngang vào trước |

---

## 2. Bốn Điều Kiện Coffman (Coffman Conditions)

Vào năm 1971, nhà khoa học máy tính Edward G. Coffman Jr. đã chứng minh rằng một tình trạng Deadlock **chỉ có thể xảy ra khi và chỉ khi cả 4 điều kiện sau đồng thời thỏa mãn**:

```text
               +---------------------------------------+
               |      1. MUTUAL EXCLUSION              |
               | (Tài nguyên không thể chia sẻ)        |
               +-------------------+-------------------+
                                   |
                                   v
               +---------------------------------------+
               |      2. HOLD AND WAIT                 |
               | (Giữ tài nguyên và chờ xin thêm)      |
               +-------------------+-------------------+
                                   |
                                   v
               +---------------------------------------+
               |      3. NO PREEMPTION                 |
               | (Không thể tước đoạt cưỡng bức)       |
               +-------------------+-------------------+
                                   |
                                   v
               +---------------------------------------+
               |      4. CIRCULAR WAIT                 |
               | (Chờ đợi vòng tròn: P0->P1->...->P0)  |
               +-------------------+-------------------+
                                   |
                                   v
                        ====== DEADLOCK! ======
```

### Chi tiết 4 điều kiện:
1. **Mutual Exclusion (Loại trừ lẫn nhau):** Tồn tại ít nhất một tài nguyên được nắm giữ ở chế độ không thể chia sẻ (*Non-sharable*). Chỉ có đúng một tiến trình được phép sử dụng tài nguyên đó tại một thời điểm. Nếu tiến trình khác yêu cầu, nó bắt buộc phải chờ đợi.
2. **Hold and Wait (Nắm giữ và Chờ đợi):** Một tiến trình đang nắm giữ ít nhất một tài nguyên và đang tiếp tục yêu cầu thêm các tài nguyên mới mà các tài nguyên này hiện đang bị các tiến trình khác chiếm giữ.
3. **No Preemption (Không trưng dụng):** Tài nguyên không thể bị hệ điều hành tước đoạt cưỡng bức khỏi tay tiến trình. Tài nguyên chỉ có thể được giải phóng một cách tự nguyện bởi chính tiến trình đang sở hữu nó sau khi hoàn thành nhiệm vụ.
4. **Circular Wait (Chờ đợi vòng tròn):** Tồn tại một chuỗi các tiến trình $\{P_0, P_1, P_2, \dots, P_n\}$ sao cho:
   - $P_0$ đang chờ tài nguyên do $P_1$ nắm giữ.
   - $P_1$ đang chờ tài nguyên do $P_2$ nắm giữ.
   - $\dots$
   - $P_n$ đang chờ tài nguyên do $P_0$ nắm giữ.

> [!IMPORTANT]
> **Định lý Bất biến:** Bốn điều kiện Coffman vừa là **điều kiện cần**, vừa là **điều kiện đủ**. Nếu bạn phá vỡ được **bất kỳ một trong bốn điều kiện này**, Deadlock toán học chắc chắn không thể hình thành!

---

## 3. Đồ Thị Cấp Phát Tài Nguyên (Resource Allocation Graph - RAG)

Hệ điều hành mô hình hóa trạng thái phân bổ tài nguyên dưới dạng một đồ thị có hướng $G = (V, E)$:
- **Tập đỉnh $V$:** Chia làm 2 nhóm:
  - Tập các tiến trình $P = \{P_1, P_2, \dots, P_n\}$ (vẽ bằng hình tròn).
  - Tập các loại tài nguyên $R = \{R_1, R_2, \dots, R_m\}$ (vẽ bằng hình chữ nhật, bên trong chứa các dấu chấm tròn thể hiện số lượng cá thể - *instances*).
- **Tập cạnh $E$:**
  - **Cạnh yêu cầu (Request Edge):** $P_i \to R_j$ (tiến trình $P_i$ đang chờ xin cấp 1 cá thể của tài nguyên $R_j$).
  - **Cạnh cấp phát (Assignment Edge):** $R_j \to P_i$ (một cá thể của tài nguyên $R_j$ đã được cấp cho tiến trình $P_i$).

```text
KỊCH BẢN 1: CÓ DEADLOCK (Mỗi tài nguyên 1 instance, có chu trình)
      ( P1 ) ------------> [ R1 ]
        ^                    |
        |                    v
      [ R2 ] <------------ ( P2 )
  Chu trình: P1 -> R1 -> P2 -> R2 -> P1  ===> CHẮC CHẮN DEADLOCK!

KỊCH BẢN 2: CÓ CHU TRÌNH NHƯNG KHÔNG DEADLOCK (Tài nguyên có nhiều instances)
      ( P1 ) ------------> [ R1 (2 chấm) ] <---- ( P3 )
        ^                    |                      |
        |                    v                      v
      [ R2 (2 chấm) ] <--- ( P2 )                 [ R2 ]
  (Khi P3 chạy xong giải phóng R1 và R2, chu trình sẽ bị phá vỡ hoàn toàn!)
```

### Hai định lý vàng của đồ thị RAG:
1. **Định lý 1:** Nếu đồ thị RAG **không chứa bất kỳ chu trình nào** $\implies$ Hệ thống **chắc chắn không có Deadlock**.
2. **Định lý 2:** Nếu đồ thị RAG **có chứa chu trình**:
   - Nếu mỗi loại tài nguyên chỉ có **đúng 1 cá thể (Single-instance)** $\implies$ Chu trình là điều kiện cần và đủ, hệ thống **chắc chắn đang bị Deadlock**.
   - Nếu có tài nguyên có **nhiều cá thể (Multi-instance)** $\implies$ Chu trình chỉ là điều kiện cần nhưng chưa đủ, hệ thống **có thể** bị Deadlock hoặc không.

---

## 4. Bốn Chiến Lược Xử Lý Deadlock của Hệ Điều Hành

### 1. Bỏ qua (Ostrich Algorithm - Thuật toán Đà điểu):
- **Triết lý:** Vùi đầu vào cát và giả vờ như Deadlock không bao giờ xảy ra.
- **Thực tế:** Các hệ điều hành thông dụng (Linux, Windows, macOS) áp dụng chiến lược này cho không gian User Space! Lý do: Chi phí tính toán để phát hiện và phòng tránh Deadlock liên tục trong nhân Kernel đắt đỏ hơn rất nhiều so với tần suất xảy ra lỗi. Nếu ứng dụng người dùng bị Deadlock, Kernel phó mặc cho người dùng tắt tiến trình qua Task Manager hoặc `kill -9`.

### 2. Phòng ngừa Deadlock (Deadlock Prevention):
Mục tiêu là thiết kế hệ thống sao cho loại bỏ vĩnh viễn ít nhất 1 trong 4 điều kiện Coffman:
- **Phá vỡ Mutual Exclusion:** Chuyển đổi tài nguyên sang dạng có thể chia sẻ (ví dụ: cơ chế Spooling cho máy in). Tuy nhiên, các tài nguyên như Mutex, file ghi độc quyền không thể chia sẻ được.
- **Phá vỡ Hold and Wait:** Yêu cầu tiến trình phải xin cấp phát toàn bộ mọi tài nguyên cần thiết trong 1 lần duy nhất (*All-or-Nothing*). Nếu thiếu 1 tài nguyên, không được giữ bất kỳ cái nào. Nhược điểm: Lãng phí tài nguyên và dễ gây Starvation.
- **Phá vỡ No Preemption:** Nếu tiến trình đang giữ tài nguyên mà xin thêm tài nguyên khác không được cấp, nó buộc phải giải phóng toàn bộ các tài nguyên hiện có và thử lại từ đầu.
- **Phá vỡ Circular Wait (Chiến lược hiệu quả nhất trong thực tế):**
  Áp dụng nguyên tắc **Thứ tự Khóa Toàn cục (Lock Ordering)**. Định nghĩa một hàm số ánh xạ toàn ánh gán cho mỗi tài nguyên một số thứ tự nguyên duy nhất: $F: R \to \mathbb{N}$.
  **Quy tắc:** Mọi luồng trong hệ thống bắt buộc phải yêu cầu các khóa theo thứ tự tăng dần của chỉ số: $F(R_a) < F(R_b)$.
  *Chứng minh:* Giả sử có chu trình $P_0 \to P_1 \to \dots \to P_n \to P_0$, điều này dẫn tới chuỗi bất đẳng thức toán học mâu thuẫn: $F(R_0) < F(R_1) < \dots < F(R_n) < F(R_0)$ (vô lý vì một số không thể nhỏ hơn chính nó!). Chu trình Circular Wait bị tiêu diệt hoàn toàn.

---

## 5. Tránh Deadlock: Thuật toán Banker của Dijkstra (Banker's Algorithm)

Khác với Prevention (hạn chế cách viết mã), **Deadlock Avoidance** cho phép các luồng xin tài nguyên tùy ý, nhưng Kernel sẽ kiểm tra trạng thái an toàn trước khi trao tài nguyên.

### Khái niệm Trạng thái An toàn (Safe State):
Một trạng thái được gọi là **An toàn (Safe)** nếu tồn tại ít nhất một **Chuỗi An toàn (Safe Sequence)** $\langle P_0, P_1, \dots, P_n \rangle$ sao cho với mọi $P_i$, số tài nguyên mà $P_i$ còn cần tối đa có thể được đáp ứng bởi lượng tài nguyên khả dụng hiện tại cộng với lượng tài nguyên mà các tiến trình phía trước $P_j (j < i)$ sẽ giải phóng sau khi chạy xong.

```text
               +---------------------------------------+
               |        HỆ THỐNG CÁC TRẠNG THÁI        |
               |                                       |
               |   +-------------------------------+   |
               |   |      TRẠNG THÁI KHÔNG AN TOÀN |   |
               |   |      (UNSAFE STATE)           |   |
               |   |   +-----------------------+   |   |
               |   |   |  DEADLOCK             |   |   |
               |   |   +-----------------------+   |   |
               |   +-------------------------------+   |
               |                                       |
               |   +-------------------------------+   |
               |   |      TRẠNG THÁI AN TOÀN       |   |
               |   |      (SAFE STATE)             |   |
               |   +-------------------------------+   |
               +---------------------------------------+
```

### Cấu trúc dữ liệu của Thuật toán Banker:
Cho $n$ tiến trình và $m$ loại tài nguyên:
1. `Available[m]`: Vector kích thước $m$, `Available[j] = k` nghĩa là có $k$ cá thể của tài nguyên $R_j$ đang sẵn sàng cấp phát.
2. `Max[n][m]`: Ma trận nhu cầu tối đa của mỗi tiến trình.
3. `Allocation[n][m]`: Ma trận số tài nguyên hiện đang được giữ bởi mỗi tiến trình.
4. `Need[n][m]`: Ma trận số tài nguyên còn cần thêm để hoàn tất:
   $$\text{Need}[i][j] = \text{Max}[i][j] - \text{Allocation}[i][j]$$

---

### Bài toán Thực tế Lần vết Thuật toán Banker Từng bước:
Xét hệ thống gồm 5 tiến trình ($P_0, P_1, P_2, P_3, P_4$) và 3 loại tài nguyên $A$ (10 cá thể), $B$ (5 cá thể), $C$ (7 cá thể).

Tại thời điểm $T_0$, ảnh chụp dữ liệu hệ thống như sau:

| Tiến trình | Allocation (A B C) | Max (A B C) | Need = Max - Allocation (A B C) |
|---|---|---|---|
| **P0** | 0 1 0 | 7 5 3 | **7 4 3** |
| **P1** | 2 0 0 | 3 2 2 | **1 2 2** |
| **P2** | 3 0 2 | 9 0 2 | **6 0 0** |
| **P3** | 2 1 1 | 2 2 2 | **0 1 1** |
| **P4** | 0 0 2 | 4 3 3 | **4 3 1** |

Tổng tài nguyên đang được cấp phát:
$\sum \text{Allocation} = (0+2+3+2+0, 1+0+0+1+0, 0+0+2+1+2) = (7, 2, 5)$.
Vector tài nguyên khả dụng còn lại:
$$\text{Available} = (10, 5, 7) - (7, 2, 5) = \mathbf{(3, 3, 2)}$$

### Lần vết Tìm Chuỗi An Toàn (Safety Algorithm):
Khởi tạo: `Work = Available = (3, 3, 2)`, `Finish = [false, false, false, false, false]`.

1. **Xét P0:** $\text{Need}_0 = (7, 4, 3) \le \text{Work}(3, 3, 2)$ ? $\implies$ **Sai** ($7 > 3$). Bỏ qua $P_0$.
2. **Xét P1:** $\text{Need}_1 = (1, 2, 2) \le \text{Work}(3, 3, 2)$ ? $\implies$ **Đúng**!
   - Cấp phát giả định cho $P_1$ chạy xong, thu hồi lại tài nguyên:
     $$\text{Work} = \text{Work} + \text{Allocation}_1 = (3, 3, 2) + (2, 0, 0) = \mathbf{(5, 3, 2)}$$
   - Đánh dấu `Finish[1] = true`.
3. **Xét P2:** $\text{Need}_2 = (6, 0, 0) \le \text{Work}(5, 3, 2)$ ? $\implies$ **Sai** ($6 > 5$). Bỏ qua $P_2$.
4. **Xét P3:** $\text{Need}_3 = (0, 1, 1) \le \text{Work}(5, 3, 2)$ ? $\implies$ **Đúng**!
   - $P_3$ chạy xong, thu hồi tài nguyên:
     $$\text{Work} = \text{Work} + \text{Allocation}_3 = (5, 3, 2) + (2, 1, 1) = \mathbf{(7, 4, 3)}$$
   - Đánh dấu `Finish[3] = true`.
5. **Xét P4:** $\text{Need}_4 = (4, 3, 1) \le \text{Work}(7, 4, 3)$ ? $\implies$ **Đúng**!
   - $P_4$ chạy xong, thu hồi tài nguyên:
     $$\text{Work} = \text{Work} + \text{Allocation}_4 = (7, 4, 3) + (0, 0, 2) = \mathbf{(7, 4, 5)}$$
   - Đánh dấu `Finish[4] = true`.
6. **Quay lại xét P0:** $\text{Need}_0 = (7, 4, 3) \le \text{Work}(7, 4, 5)$ ? $\implies$ **Đúng**!
   - $P_0$ chạy xong, thu hồi tài nguyên:
     $$\text{Work} = \text{Work} + \text{Allocation}_0 = (7, 4, 5) + (0, 1, 0) = \mathbf{(7, 5, 5)}$$
   - Đánh dấu `Finish[0] = true`.
7. **Quay lại xét P2:** $\text{Need}_2 = (6, 0, 0) \le \text{Work}(7, 5, 5)$ ? $\implies$ **Đúng**!
   - $P_2$ chạy xong, thu hồi tài nguyên:
     $$\text{Work} = \text{Work} + \text{Allocation}_2 = (7, 5, 5) + (3, 0, 2) = \mathbf{(10, 5, 7)}$$
   - Đánh dấu `Finish[2] = true`.

**KẾT LUẬN:** Toàn bộ mảng `Finish` đều là `true`. Hệ thống đang ở **Trạng thái An toàn (Safe State)** với Chuỗi an toàn tìm được là:
$$\mathbf{\langle P_1, P_3, P_4, P_0, P_2 \rangle}$$

---

### Xử lý Yêu cầu Cấp phát Mới (Resource-Request Algorithm):
Bây giờ giả sử tiến trình $P_1$ gửi thêm một yêu cầu xin cấp tài nguyên:
$$\text{Request}_1 = (1, 0, 2)$$

- **Bước 1:** Kiểm tra $\text{Request}_1(1, 0, 2) \le \text{Need}_1(1, 2, 2)$ ? $\implies$ **Thỏa mãn** (Không vượt quá nhu cầu đã khai báo).
- **Bước 2:** Kiểm tra $\text{Request}_1(1, 0, 2) \le \text{Available}(3, 3, 2)$ ? $\implies$ **Thỏa mãn** (Kho tài nguyên đủ cấp).
- **Bước 3:** Hệ điều hành thực hiện cấp phát giả định:
  $$\text{Available} = (3, 3, 2) - (1, 0, 2) = \mathbf{(2, 3, 0)}$$
  $$\text{Allocation}_1 = (2, 0, 0) + (1, 0, 2) = \mathbf{(3, 0, 2)}$$
  $$\text{Need}_1 = (1, 2, 2) - (1, 0, 2) = \mathbf{(0, 2, 0)}$$
- **Bước 4:** Chạy lại thuật toán kiểm tra Safe State với dữ liệu giả định:
  - Với $\text{Available} = (2, 3, 0)$, tiến trình $P_1$ có $\text{Need}_1 = (0, 2, 0) \le (2, 3, 0) \to P_1$ chạy xong, trả về $\text{Work} = (2, 3, 0) + (3, 0, 2) = (5, 3, 2)$.
  - Tiếp theo $P_3$ có $\text{Need}_3 = (0, 1, 1) \le (5, 3, 2) \to$ chạy xong.
  - Tương tự, ta tiếp tục tìm được chuỗi an toàn $\langle P_1, P_3, P_4, P_0, P_2 \rangle$!
  $\implies$ Yêu cầu của $P_1$ được hệ thống **chấp thuận ngay lập tức**.

---

## 6. Phát Hiện & Phục Hồi Deadlock (Detection & Recovery)

Nếu hệ thống không dùng Prevention cũng không dùng Avoidance, nó cần cơ chế định kỳ chạy thuật toán dò tìm Deadlock:
- Với hệ thống đơn cá thể tài nguyên: Dựng đồ thị **Wait-For Graph (WFG)** bằng cách gộp các node tài nguyên lại, chỉ giữ các cạnh $P_i \to P_j$. Dùng thuật toán DFS / Tarjan với độ phức tạp $O(V^2)$ để tìm chu trình.
- Khi phát hiện Deadlock, Kernel tiến hành **Phục hồi (Recovery)** qua 2 hướng:
  1. **Hủy tiến trình (Process Termination):**
     - Hủy toàn bộ các tiến trình trong chu trình Deadlock (cách làm tốn kém nhất).
     - Hủy lần lượt từng tiến trình cho đến khi chu trình Deadlock bị phá vỡ. Tiêu chí chọn tiến trình "nạn nhân" (**Victim Selection**): ưu tiên hủy tiến trình có mức độ ưu tiên thấp nhất, chạy được ít thời gian nhất, tiêu tốn ít tài nguyên nhất.
  2. **Trưng dụng tài nguyên & Rollback (Resource Preemption):**
     - Buộc tiến trình giải phóng tài nguyên và quay lui (**Rollback**) về một điểm kiểm tra an toàn (**Checkpoint**) trước đó. Cần cơ chế chống Starvation: nếu một tiến trình liên tục bị chọn làm nạn nhân rollback, phải tăng mức ưu tiên của nó lên.

---

## 7. Deadlock trong Cơ Sở Dữ Liệu & Hệ Thống Backend Thực Tế

### 1. Phân tích Deadlock trong RDBMS (PostgreSQL / MySQL InnoDB):
```sql
-- Giao dịch 1 (Transaction 1):
BEGIN;
UPDATE accounts SET balance = balance - 100 WHERE id = 1; -- Giữ khóa Row 1
UPDATE accounts SET balance = balance + 100 WHERE id = 2; -- Chờ khóa Row 2!

-- Giao dịch 2 (Transaction 2):
BEGIN;
UPDATE accounts SET balance = balance - 50 WHERE id = 2;  -- Giữ khóa Row 2
UPDATE accounts SET balance = balance + 50 WHERE id = 1;  -- Chờ khóa Row 1!
```
- Khi xảy ra chu trình chờ đợi, engine MySQL InnoDB tự động phát hiện và ngắt một transaction với thông báo:
  `ERROR 1213 (40001): Deadlock found when trying to get lock; try restarting transaction`.
  InnoDB sẽ tự động **ROLLBACK transaction nào có khối lượng ghi ít hơn** để giải phóng khóa cho transaction còn lại.

### 2. Nguyên tắc phòng chống Deadlock cho Kỹ sư Backend:
1. **Truy cập dữ liệu theo cùng một thứ tự sắp xếp duy nhất:** Nếu cần sửa đổi nhiều dòng dữ liệu, luôn sắp xếp danh sách ID tăng dần trước khi thực hiện câu lệnh Update:
   ```csharp
   var accountIds = new[] { idA, idB }.OrderBy(x => x).ToList();
   foreach (var id in accountIds) {
       await db.UpdateAccountAsync(id);
   }
   ```
2. **Cấu hình Lock Timeout:** Không bao giờ chờ khóa vô hạn. Luôn đặt `CommandTimeout` hoặc `SET lock_timeout = '5s'`. Nếu quá 5 giây không lấy được khóa, chủ động hủy thao tác và thử lại với độ trễ ngẫu nhiên (Exponential Backoff with Jitter).

---

## 8. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

### Câu 1: Tại sao việc tồn tại một chu trình trong Đồ thị Cấp phát Tài nguyên (RAG) chưa chắc chắn dẫn tới Deadlock đối với tài nguyên có nhiều cá thể?
> **Đáp án:** Vì một tài nguyên trong chu trình có thể đang được chia sẻ cho một tiến trình thứ ba $P_k$ không nằm trong chu trình đó. Khi tiến trình $P_k$ hoàn thành công việc và giải phóng cá thể tài nguyên của nó, cá thể này có thể được cấp phát cho một tiến trình trong chu trình, từ đó phá vỡ thế bế tắc và chu trình tự tiêu biến.

### Câu 2: Trong thuật toán Banker, trạng thái "Không an toàn" (Unsafe State) có đồng nghĩa với trạng thái "Deadlock" hay không?
> **Đáp án:** **Không!** Unsafe State không phải là Deadlock. Unsafe State chỉ có nghĩa là hệ thống không thể đảm bảo chắc chắn rằng mọi tiến trình sẽ hoàn thành nếu tất cả chúng đột ngột yêu cầu tối đa số tài nguyên đã khai báo. Tuy nhiên, nếu trong thực tế các tiến trình nhả tài nguyên sớm trước khi xin mức tối đa, hệ thống vẫn có thể hoàn thành mà không hề xảy ra Deadlock. Deadlock chỉ là một tập con nằm bên trong Unsafe State.

### Câu 3: Kỹ thuật Lock Ordering phá vỡ điều kiện nào trong 4 điều kiện Coffman?
> **Đáp án:** Kỹ thuật Lock Ordering phá vỡ điều kiện **Circular Wait (Chờ đợi vòng tròn)**. Bằng cách bắt buộc mọi luồng phải lấy các khóa tài nguyên theo thứ tự chỉ số tăng dần ($F(R_a) < F(R_b)$), chu trình phụ thuộc có hướng không thể hình thành trên đồ thị tài nguyên.

### Câu 4: Khi hệ điều hành chọn giải pháp hủy tiến trình để phục hồi sau Deadlock, tiêu chuẩn nào thường được dùng để chọn tiến trình nạn nhân (Victim)?
> **Đáp án:** Các tiêu chí chọn nạn nhân nhằm giảm thiểu tổn thất công sức tính toán của hệ thống:
> 1. Mức độ ưu tiên của tiến trình (ưu tiên tiến trình thấp hơn).
> 2. Thời gian tiến trình đã chạy và thời gian còn lại để hoàn tất (chọn tiến trình mới bắt đầu chạy).
> 3. Số lượng tài nguyên mà tiến trình đang chiếm giữ (chọn tiến trình giữ nhiều tài nguyên bị tranh chấp).
> 4. Số lần tiến trình đã từng bị chọn làm nạn nhân trước đó (để tránh Starvation).

<!-- studxy-os-v4 -->
"""";

    private static string GetLesson32Markdown() => """"
# Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp

> [!NOTE]
> **Khái niệm then chốt:** **Bộ nhớ ảo (Virtual Memory)** là một trong những phát minh vĩ đại nhất của khoa học máy tính, cung cấp cho mỗi tiến trình một không gian địa chỉ tuyến tính liên tục, độc lập và an toàn, giải phóng lập trình viên khỏi gánh nặng của giới hạn phần cứng RAM vật lý thông qua sự phối hợp nhịp nhàng giữa Nhân hệ điều hành (Kernel) và Đơn vị Quản lý Bộ nhớ phần cứng (**MMU**).

> [!TIP]
> **Hình ảnh trực quan:** Hãy hình dung Bộ nhớ Ảo giống như dịch vụ cho thuê hộp thư bưu điện:
> - Mỗi công ty (Tiến trình) được cấp một mã số hòm thư ảo từ số `1` đến số `10,000`. Họ nghĩ rằng hòm thư của họ nằm liền nhau trên một dãy tủ riêng biệt.
> - Trên thực tế, nhân viên bưu điện (MMU + Bảng trang) gom các lá thư lại và cất rải rác vào bất kỳ ngăn kéo trống nào còn thừa trong kho lưu trữ lớn (RAM vật lý). Công ty không cần quan tâm lá thư thực sự nằm ở ngăn kéo nào, họ chỉ cần nhớ mã số hòm thư ảo của mình.

---

## 1. Động lực học & Bản chất của Bộ nhớ Ảo (Virtual Memory)

Trước khi cơ chế Phân trang (Paging) ra đời, các máy tính sử dụng kỹ thuật phân bổ bộ nhớ liên tục (Base and Bounds) hoặc Phân đoạn (Segmentation). Kỹ thuật này bộc lộ những điểm yếu chí tử:
- **Phân mảnh ngoại vi (External Fragmentation):** Bộ nhớ RAM bị chia cắt thành hàng trăm mẩu nhỏ không sử dụng được vì không có mẩu nào đủ lớn để nạp một tiến trình mới.
- **Giới hạn dung lượng vật lý:** Chương trình không thể có kích thước lớn hơn tổng dung lượng thanh RAM cắm trên bo mạch chủ.
- **Thiếu cơ chế bảo vệ:** Một con trỏ lỗi trong tiến trình A có thể vô tình ghi đè lên bộ nhớ của tiến trình B hoặc làm sập nhân Kernel.

```text
KHÔNG GIAN ĐỊA CHỈ ẢO (VIRTUAL ADDRESS SPACE - 48 bit)
0x0000000000000000 +------------------------------------+
                   | Trang Ảo 0 (VPN 0)                 | --------+
0x0000000000001000 +------------------------------------+         |
                   | Trang Ảo 1 (VPN 1)                 | ---+    |
0x0000000000002000 +------------------------------------+    |    |
                   | Trang Ảo 2 (VPN 2) [Chưa nạp RAM]  |    |    |
                   +------------------------------------+    |    |
                                                             |    | Tra cứu MMU
RAM VẬT LÝ (PHYSICAL FRAMES - 4 KB mỗi Frame)                |    | (Bảng trang)
Frame 0 [0x0000]   +------------------------------------+    |    |
                   | Frame trống                        |    |    |
Frame 1 [0x1000]   +------------------------------------+    |    |
                   | Dữ liệu của Trang Ảo 1             |<---+    |
Frame 2 [0x2000]   +------------------------------------+         |
                   | Dữ liệu của Trang Ảo 0             |<--------+
Frame 3 [0x3000]   +------------------------------------+
                   | Dữ liệu của Kernel / Process khác  |
```

### Ba mục tiêu tối thượng của Bộ nhớ Ảo:
1. **Phân lập & Bảo vệ (Protection & Isolation):** Mỗi tiến trình nằm trong một không gian địa chỉ riêng biệt. MMU kiểm tra quyền truy cập (*Read, Write, Execute*) trên từng trang; bất kỳ hành vi truy cập trái phép nào đều bị bẫy phần cứng chặn đứng ngay lập tức.
2. **Ảo giác Không gian Nhớ Khổng lồ (Contiguous Memory Illusion):** Trên kiến trúc x86-64, mỗi tiến trình nhìn thấy một không gian nhớ ảo người dùng lên tới **128 Terabytes** ($2^{47}$ bytes), dù máy tính vật lý có thể chỉ lắp thanh RAM 8 GB.
3. **Tối ưu hóa Tài nguyên Vật lý:**
   - **Nạp theo nhu cầu (Demand Paging):** Chỉ những trang dữ liệu thực sự được CPU chạm vào mới được nạp vào RAM.
   - **Sao chép khi ghi (Copy-On-Write - COW):** Khi tiến trình gọi lệnh `fork()`, hệ điều hành không sao chép toàn bộ bộ nhớ của tiến trình cha. Nó chỉ tạo bảng trang mới trỏ chung vào các Frame vật lý cũ và đánh dấu quyền **Chỉ đọc (Read-Only)**. Chỉ khi một trong hai tiến trình thực sự ghi dữ liệu vào một trang, MMU mới kích hoạt ngắt để Kernel nhân bản riêng trang đó trong $O(1)$.

---

## 2. Cơ Chế Phân Trang Cơ Bản (Basic Paging Mechanism)

- **Trang ảo (Page):** Không gian địa chỉ ảo được chia thành các khối cố định bằng nhau, kích thước phổ biến nhất là **4 KB** ($4096\text{ bytes} = 2^{12}\text{ bytes}$).
- **Khung trang vật lý (Frame):** Bộ nhớ RAM vật lý cũng được chia thành các khối có kích thước đúng bằng kích thước của trang ảo (4 KB).

```text
CẤU TRÚC ĐỊA CHỈ ẢO 32-BIT (VỚI TRANG 4 KB = 2^12 BYTES):
 31                                   12 11                     0
+---------------------------------------+------------------------+
|      Virtual Page Number (VPN)        |      Offset in Page    |
|               20 bits                 |         12 bits        |
+---------------------------------------+------------------------+
                   |                                   |
                   v                                   |
           [ BẢNG TRANG MMU ]                          |
            VPN -> Tra ra PFN                          |
                   |                                   |
                   v                                   v
 31                                   12 11                     0
+---------------------------------------+------------------------+
|      Physical Frame Number (PFN)      |      Offset in Frame   |
|               20 bits                 |         12 bits        |
+---------------------------------------+------------------------+
               ĐỊA CHỈ VẬT LÝ TRUY XUẤT TRÊN RAM
```

### Các cờ trạng thái quan trọng trong một Mục nhập Bảng Trang (Page Table Entry - PTE):
Mỗi mục nhập PTE (64-bit trên kiến trúc x86-64) chứa địa chỉ khung trang PFN cùng các bit điều khiển phần cứng:
- **Bit 0 - Present ($P$):** Bằng `1` nếu trang hiện đang nằm trong RAM vật lý; bằng `0` nếu trang chưa được nạp hoặc đang bị đẩy ra Swap Space (kích hoạt **Page Fault Exception**).
- **Bit 1 - Read/Write ($R/W$):** `0` = Chỉ cho phép đọc; `1` = Cho phép cả đọc và ghi.
- **Bit 2 - User/Supervisor ($U/S$):** `0` = Chỉ mã nhân Kernel (Ring 0) mới được truy cập; `1` = Mã người dùng (Ring 3) được phép truy cập.
- **Bit 5 - Accessed ($A$):** Phần cứng tự động bật lên `1` mỗi khi trang được đọc hoặc ghi (phục vụ thuật toán thay thế trang).
- **Bit 6 - Dirty ($D$):** Phần cứng tự động bật lên `1` khi có lệnh ghi làm thay đổi dữ liệu của trang (báo hiệu cần ghi ra đĩa khi tráo đổi).
- **Bit 63 - No-Execute ($NX$ bit):** Ngăn chặn việc nạp mã lệnh từ trang này vào CPU Pipeline để thực thi, vũ khí chống lại các cuộc tấn công tràn bộ đệm mã độc (Buffer Overflow Shellcode).

---

## 3. Tăng Tốc Phần Cứng với TLB & Công Thức EAT

Điểm nghẽn chí tử của cơ chế Phân trang: Mỗi khi chương trình đọc một biến trong bộ nhớ, CPU phải thực hiện **ít nhất 2 lần truy cập RAM**:
1. Lần 1: Đọc bảng trang trong RAM để lấy PFN.
2. Lần 2: Đọc dữ liệu thực tế tại ô nhớ vật lý $\text{PFN} + \text{Offset}$.
Tốc độ của toàn bộ máy tính sẽ bị giảm đi một nửa!

Để giải quyết thảm họa này, các nhà thiết kế vi xử lý đã tích hợp **TLB (Translation Lookaside Buffer)** ngay trên chip CPU.

```text
               +-----------------------------+
               |     Địa chỉ ảo từ CPU       |
               +--------------+--------------+
                              |
                     [ Tra cứu TLB Cache ]
                     /                   \
        (TLB Hit: 1 chu kỳ)          (TLB Miss: Chậm)
               /                           \
              v                             v
       Lấy ngay PFN                   Duyệt Bảng Trang trong RAM
              |                             |
              |                             +---> Nạp ánh xạ vào TLB
              +--------------+--------------+
                             |
                             v
               +-----------------------------+
               |      Truy cập RAM Vật lý    |
               +-----------------------------+
```

### Công thức tính Thời gian Truy cập Hiệu dụng (Effective Access Time - EAT):
Gọi $\alpha$ là tỷ lệ trúng bộ đệm TLB (**TLB Hit Ratio**), $t_{\text{TLB}}$ là thời gian tra cứu TLB, và $t_{\text{MEM}}$ là thời gian một lần truy xuất bộ nhớ RAM.

$$\text{EAT} = \alpha \times (t_{\text{TLB}} + t_{\text{MEM}}) + (1 - \alpha) \times (t_{\text{TLB}} + 2 \times t_{\text{MEM}})$$

Với hệ thống bảng trang đa cấp $n$ tầng, khi TLB Miss, CPU phải duyệt qua $n$ tầng bảng trang trong RAM:
$$\text{EAT} = \alpha \times (t_{\text{TLB}} + t_{\text{MEM}}) + (1 - \alpha) \times (t_{\text{TLB}} + (n + 1) \times t_{\text{MEM}})$$

### Ví dụ Tính toán Học thuật Chi tiết:
Giả sử hệ thống sử dụng bảng trang 4 cấp ($n = 4$), $t_{\text{TLB}} = 2\text{ ns}$, thời gian truy cập RAM $t_{\text{MEM}} = 80\text{ ns}$.

1. **Trường hợp TLB Hit Ratio đạt $\alpha = 98\%$ ($0.98$):**
   $$\text{EAT} = 0.98 \times (2 + 80) + 0.02 \times (2 + (4 + 1) \times 80)$$
   $$\text{EAT} = 0.98 \times 82 + 0.02 \times (2 + 400) = 80.36 + 0.02 \times 402 = 80.36 + 8.04 = \mathbf{88.4\text{ ns}}$$
   (Độ trễ chỉ tăng nhẹ từ $80\text{ ns}$ lên $88.4\text{ ns}$, hệ thống hoạt động với $91\%$ tốc độ RAM tối đa).

2. **Trường hợp TLB Hit Ratio tụt xuống $\alpha = 70\%$ ($0.70$):**
   $$\text{EAT} = 0.70 \times 82 + 0.30 \times 402 = 57.4 + 120.6 = \mathbf{178.0\text{ ns}}$$
   (Tốc độ máy tính bị chậm đi **hơn gấp đôi** chỉ vì tỷ lệ trượt TLB tăng lên!).

> [!IMPORTANT]
> **TLB Invalidation & Công nghệ PCID:**
> Khi chuyển ngữ cảnh giữa 2 tiến trình, địa chỉ ảo của tiến trình cũ không còn ý nghĩa. Trước đây, Kernel buộc phải xả sạch toàn bộ TLB (*TLB Flush*), khiến tiến trình mới vào chạy bị sụt giảm hiệu năng nghiêm trọng do liên tục gặp TLB Miss.
> Ngày nay, CPU x86 hỗ trợ tính năng **PCID (Process-Context Identifiers)**, gắn thẻ mã định danh tiến trình (12 bit) vào từng dòng cache TLB. Nhờ đó, các ánh xạ của nhiều tiến trình có thể cùng nằm chung trong TLB mà không bị lẫn lộn, triệt tiêu chi phí TLB Flush khi Context Switch!

---

## 4. Kiến Trúc Bảng Trang Đa Cấp (Multi-Level Paging trên Linux x86-64)

### Vì sao Bảng trang phẳng (Flat Page Table) hoàn toàn phá sản trên 64-bit?
Trên kiến trúc 64-bit, nếu dùng 1 bảng phẳng duy nhất cho không gian địa chỉ $2^{48}$ bytes với trang 4 KB, ta có $2^{36}$ trang ảo. Mỗi mục PTE tốn 8 bytes:
$$\text{Dung lượng bảng trang} = 2^{36} \times 8\text{ bytes} = 2^{39}\text{ bytes} = \mathbf{512\text{ Gigabytes RAM}}$$
Chỉ để lưu một bảng tra cứu cho một tiến trình đơn lẻ, ta đã cần tới 512 GB RAM!

### Giải pháp Bảng trang 4 cấp của Linux x86-64:
Không gian địa chỉ ảo 48-bit được chia thành 5 phần bằng nhau:

```text
 47        39 38        30 29        21 20        12 11                     0
+------------+------------+------------+------------+------------------------+
| PGD (9 bit)| PUD (9 bit)| PMD (9 bit)| PTE (9 bit)|     Offset (12 bit)    |
+------------+------------+------------+------------+------------------------+
      |            |            |            |                  |
      v            v            v            v                  v
    CR3 ---> [PGD] ---> [PUD] ---> [PMD] ---> [PTE] ---------> [RAM FRAME] + Offset
```
- **PGD (Page Global Directory):** 9 bits $\to 2^9 = 512$ mục nhập (kích thước đúng 4 KB).
- **PUD (Page Upper Directory):** 9 bits $\to 512$ mục nhập (kích thước 4 KB).
- **PMD (Page Middle Directory):** 9 bits $\to 512$ mục nhập (kích thước 4 KB).
- **PTE (Page Table Entry):** 9 bits $\to 512$ mục nhập (kích thước 4 KB).
- **Offset:** 12 bits $\to 4096$ bytes định vị byte chính xác trong trang vật lý.

### Sức mạnh của Cấp phát Thưa (Sparse Allocation):
Một tiến trình thông thường chỉ sử dụng một vài đoạn nhớ nhỏ (Code, Stack, Heap). Những dải địa chỉ ảo khổng lồ nằm giữa chưa được cấp phát sẽ có con trỏ ở cấp PGD/PUD mang giá trị `NULL`.
Nhờ đó, Kernel **không cần tạo ra bất kỳ bảng con nào** cho các vùng trống này! Một tiến trình nhỏ chỉ tiêu tốn vài chục Kilobytes bộ nhớ để lưu cấu trúc cây bảng trang, tiết kiệm tới **$99.999\%$ dung lượng RAM**.

### HugePages (Trang Khổng Lồ 2 MB và 1 GB):
Khi ứng dụng quản lý các tập dữ liệu khổng lồ trong RAM (như Database Buffer Pool của PostgreSQL, Redis, Oracle hoặc JVM Heap 64 GB):
- Nếu dùng trang 4 KB, CPU cần hàng triệu mục TLB $\to$ liên tục bị **TLB Thrashing**.
- Bằng cách bật bit $PS$ (*Page Size*) ở cấp PMD, địa chỉ ảo bỏ qua cấp PTE và trỏ thẳng vào một khung trang vật lý khổng lồ **2 MB** ($2^{21}\text{ bytes}$).
- Bật bit $PS$ ở cấp PUD tạo ra trang khổng lồ **1 GB** ($2^{30}\text{ bytes}$).
- **Lợi ích:** Một dòng TLB duy nhất có thể bao phủ dung lượng RAM lớn gấp 512 lần, triệt tiêu độ trễ TLB Miss và tăng tốc các hệ thống cơ sở dữ liệu lên tới $20\% - 40\%$.

---

## 5. Giám Sát & Phân Tích Bộ Nhớ Ảo trên Linux Thực Tế

### 1. Giải mã thông số bộ nhớ qua `/proc/[pid]/smaps`:
```bash
$ cat /proc/1234/smaps | head -n 20
00400000-00452000 r-xp 00000000 08:01 123456   /usr/bin/my_app
Size:                328 kB     # Tổng kích thước không gian ảo được cấp (VSZ)
Rss:                 216 kB     # Dung lượng thực tế đang nằm trên RAM vật lý
Pss:                 108 kB     # Proportional Set Size (tính cả tỷ lệ chia sẻ)
Shared_Clean:        216 kB     # Bộ nhớ thư viện dùng chung chưa bị sửa đổi
Private_Dirty:         0 kB     # Bộ nhớ riêng tư đã bị sửa đổi
Anonymous:             0 kB     # Bộ nhớ Heap/Stack không liên kết với file
```
- **VSZ (Virtual Memory Size):** Tổng dung lượng ảo mà tiến trình đã yêu cầu (kể cả chưa dùng).
- **RSS (Resident Set Size):** Lượng RAM vật lý thực tế mà tiến trình đang chiếm dụng.
- **PSS (Proportional Set Size):** Chỉ số chính xác nhất để tính mức RAM thực tế của tiến trình khi có nhiều tiến trình cùng dùng chung thư viện động.

### 2. Kỹ thuật Phân bổ Lười (Lazy Allocation / Overcommit):
Khi bạn gọi lệnh cấp phát bộ nhớ trong ngôn ngữ C:
```c
char *buffer = malloc(1024 * 1024 * 1024); // Xin cấp 1 GB RAM
```
Nhân Linux thực chất **chưa cấp bất kỳ một byte RAM vật lý nào**! Linux chỉ ghi nhận dải địa chỉ ảo 1 GB này vào cấu trúc `vm_area_struct` (làm chỉ số VSZ tăng 1 GB, trong khi RSS không đổi).
Chỉ khi chương trình chạy câu lệnh ghi đầu tiên:
```c
buffer[0] = 'A';
```
Phần cứng MMU phát hiện trang này chưa có trong bảng trang (PTE có $P = 0$), kích hoạt ngắt **Minor Page Fault**. Lúc này Kernel mới âm thầm tìm 1 Frame RAM vật lý trống, gán vào bảng trang và quay lại cho lệnh ghi hoàn thành. Đây là nghệ thuật tối ưu hóa tài nguyên đỉnh cao của Hệ điều hành.

---

## 6. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

### Câu 1: Tại sao địa chỉ ảo trên kiến trúc x86-64 hiện nay chỉ sử dụng 48 bits (hoặc 57 bits trong chuẩn mới) thay vì sử dụng toàn bộ 64 bits?
> **Đáp án:** Vì $2^{48}$ bytes tương đương với 256 Terabytes không gian địa chỉ, quá thừa thãi cho nhu cầu của hầu hết các siêu máy tính hiện nay. Việc giới hạn ở 48 bits giúp giảm số tầng của cây bảng trang xuống 4 cấp (thay vì 6 hoặc 7 cấp nếu dùng đủ 64 bits), giúp tiết kiệm dung lượng RAM quản lý và giảm đáng kể số lần truy xuất RAM khi xảy ra TLB Miss.

### Câu 2: Sự khác biệt cơ bản giữa Minor Page Fault và Major Page Fault là gì?
> **Đáp án:**
> - **Minor Page Fault (Lỗi thiếu trang nhẹ):** Xảy ra khi trang dữ liệu thực chất đã nằm sẵn trong RAM vật lý (ví dụ: trang của thư viện chia sẻ đã được nạp bởi tiến trình khác, hoặc trang vừa được cấp phát lười qua `malloc()`). Kernel chỉ cần cập nhật mục PTE mà không tốn bất kỳ chi phí đọc đĩa I/O nào.
> - **Major Page Fault (Lỗi thiếu trang nặng):** Xảy ra khi trang dữ liệu bắt buộc phải được đọc từ ổ đĩa SSD/HDD hoặc Swap Space vào RAM. Tiến trình bị đình chỉ để chờ đợi thao tác I/O đĩa chậm hơn hàng nghìn lần.

### Câu 3: Làm thế nào mà tính năng HugePages lại giúp tăng hiệu năng cho các hệ quản trị cơ sở dữ liệu như PostgreSQL hay Redis?
> **Đáp án:** Vì cơ sở dữ liệu thường duy trì vùng đệm dữ liệu (Shared Buffers) khổng lồ hàng chục GB trong RAM. Nếu dùng trang chuẩn 4 KB, số lượng trang quá lớn khiến bộ đệm TLB phần cứng bị tràn (TLB Miss liên tục). Khi chuyển sang HugePages (2 MB hoặc 1 GB), một mục TLB duy nhất quản lý được khối lượng bộ nhớ gấp 512 đến 262,144 lần, giúp tỷ lệ TLB Hit đạt xấp xỉ 100%, loại bỏ gần như hoàn toàn thời gian duyệt bảng trang trong RAM.

### Câu 4: Nếu một hệ thống có thời gian truy cập TLB là $1\text{ ns}$, truy cập RAM là $50\text{ ns}$, sử dụng bảng trang 3 cấp. Hỏi tỷ lệ TLB Hit Ratio $\alpha$ tối thiểu phải là bao nhiêu để thời gian truy cập hiệu dụng EAT không vượt quá $60\text{ ns}$?
> **Đáp án:**
> Ta có công thức EAT cho bảng trang 3 cấp ($n = 3$):
> $$\text{EAT} = \alpha \times (1 + 50) + (1 - \alpha) \times (1 + (3 + 1) \times 50) = 51\alpha + 201(1 - \alpha) = 201 - 150\alpha$$
> Để $\text{EAT} \le 60\text{ ns}$:
> $$201 - 150\alpha \le 60 \iff 150\alpha \ge 141 \iff \alpha \ge \frac{141}{150} = \mathbf{0.94} \text{ (tức } 94\%)$$

<!-- studxy-os-v4 -->
"""";

    private static string GetLesson33Markdown() => """"
# Bài 3.3: Thuật toán Thay thế Trang (FIFO, Optimal, LRU, Clock & Working Set)

> [!NOTE]
> **Khái niệm then chốt:** Trong cơ chế Phân trang theo Nhu cầu (**Demand Paging**), khi một tiến trình yêu cầu một trang bộ nhớ mà trang đó chưa nằm trong RAM, ngắt **Page Fault** sẽ xảy ra. Nếu toàn bộ các khung trang RAM vật lý đều đã bị lấp đầy, hệ điều hành buộc phải đưa ra quyết định sống còn: Chọn trang nào đang nằm trong RAM làm "nạn nhân" (**Victim Page**) để đẩy ra đĩa giải phóng chỗ trống? Quyết định này được chi phối bởi **Thuật toán Thay thế Trang (Page Replacement Algorithm)**.

> [!TIP]
> **Hình ảnh trực quan:** Bàn làm việc của một kỹ sư nghiên cứu:
> - Mặt bàn chỉ vừa đủ chỗ để mở 4 cuốn sách cùng lúc (Khung RAM vật lý).
> - Tủ sách lớn ở góc phòng chứa 10,000 cuốn sách (Ổ đĩa SSD / Swap Space).
> - Mỗi khi cần tra cứu một cuốn sách mới chưa có trên bàn, kỹ sư buộc phải cất bớt một cuốn sách trên bàn về lại tủ. Nếu cất nhầm cuốn sách mà 5 phút nữa lại cần dùng tiếp, kỹ sư sẽ phải đứng lên đi lại tủ sách liên tục, khiến toàn bộ công việc bị đình trệ hoàn toàn (**Thrashing**)!

---

## 1. Vòng Đời Xử Lý Ngắt Thiếu Trang (Page Fault Lifecycle) & Chi Phí

Một chu kỳ xử lý Page Fault của Kernel đòi hỏi sự phối hợp chặt chẽ giữa phần cứng CPU và nhân hệ điều hành qua 9 bước tuần tự:

```text
TIẾN TRÌNH                    PHẦN CỨNG MMU                  NHÂN KERNEL & Ổ ĐĨA
    |                               |                                |
1. Truy cập địa chỉ ảo              |                                |
    +------------------------------>|                                |
                                2. Tra cứu PTE                       |
                                (Present = 0)                        |
                                    |                                |
                                3. BẪY NGẮT (#PF Vector 14)          |
                                    +------------------------------->|
                                                             4. Kiểm tra tính hợp lệ
                                                                (Địa chỉ trong vm_area?)
                                                                     |
                                                             5. Tìm Frame RAM trống
                                                                (Nếu RAM đầy -> Thuật toán chọn Victim)
                                                                     |
                                                             6. Ghi Victim ra Swap (nếu Dirty=1)
                                                                     |
                                                             7. Đọc trang mới từ Đĩa vào Frame
                                                                     |
                                                             8. Cập nhật PTE (Present=1, PFN)
                                                                     |
                                9. Khôi phục ngữ cảnh CPU <----------+
                                   Chạy lại đúng câu lệnh cũ
```

### Chín bước thực thi vi mô:
1. **Truy cập bộ nhớ:** Lệnh CPU cố gắng tham chiếu tới một địa chỉ ảo.
2. **MMU kiểm tra:** Đơn vị MMU tra cứu mục PTE thấy bit **`Present = 0`**.
3. **Bẫy ngoại lệ phần cứng:** CPU kích hoạt ngắt **Page Fault Exception (Vector 14 / `#PF`)**, lưu trạng thái thanh ghi và chuyển quyền điều khiển cho Page Fault Handler của Kernel trong Ring 0.
4. **Kiểm tra tính hợp lệ:** Kernel kiểm tra xem địa chỉ ảo này có nằm trong không gian hợp lệ được cấp phép của tiến trình (`struct vm_area_struct`) hay không. Nếu địa chỉ không hợp lệ $\to$ Kernel gửi ngay tín hiệu **`SIGSEGV` (Segmentation Fault)** để tiêu diệt tiến trình.
5. **Tìm khung trang trống (Free Frame):** Kernel duyệt danh sách `free_list`. Nếu RAM đã hết chỗ trống, Kernel kích hoạt **Thuật toán Thay thế Trang** để chọn một trang làm nạn nhân (*Victim Frame*).
6. **Xử lý trang bẩn (Write-back nếu Dirty):** Nếu trang nạn nhân có bit **`Dirty = 1`** (dữ liệu đã bị ghi sửa đổi trong RAM), Kernel bắt buộc phải phát lệnh I/O ghi nội dung trang đó ra Swap Space trên ổ đĩa. Nếu `Dirty = 0`, trang có thể bị ghi đè trực tiếp mà không tốn công ghi đĩa.
7. **Đọc dữ liệu từ đĩa:** Kernel kích hoạt lệnh DMA đọc nội dung trang cần truy cập từ ổ đĩa vào khung trang vừa được giải phóng. Trong lúc chờ đọc đĩa, tiến trình bị chuyển sang trạng thái **Blocked/Waiting**, nhường CPU cho tiến trình khác.
8. **Cập nhật Bảng trang:** Khi I/O hoàn tất, Kernel cập nhật lại mục PTE: gán địa chỉ vật lý PFN mới, bật bit `Present = 1`, đặt `Dirty = 0` và `Accessed = 0`.
9. **Khởi động lại lệnh:** Kernel chuyển tiến trình về trạng thái Ready, khôi phục lại tập thanh ghi và cho CPU thực thi lại đúng câu lệnh vừa gây ra Page Fault.

### Công thức tính toán Hiệu năng Suy giảm do Page Fault:
Thời gian truy cập bộ nhớ hiệu dụng khi có phân trang theo nhu cầu được tính theo công thức:
$$\text{EAT} = (1 - p) \times t_{\text{MEM}} + p \times t_{\text{Fault}}$$
Trong đó:
- $t_{\text{MEM}}$: Thời gian truy xuất RAM thông thường ($\approx 100\text{ ns} = 10^{-7}\text{ s}$).
- $t_{\text{Fault}}$: Tổng thời gian xử lý một lần Page Fault nặng qua đĩa SSD/HDD ($\approx 8\text{ ms} = 8,000,000\text{ ns}$).
- $p$: Tỷ lệ xảy ra Page Fault ($0 \le p \le 1$).

> [!IMPORTANT]
> **Hệ quả Toán học Khủng khiếp:**
> Giả sử chỉ có **$0.1\%$ số lần truy cập bộ nhớ bị thiếu trang** ($p = 0.001$):
> $$\text{EAT} = (1 - 0.001) \times 100 + 0.001 \times 8,000,000 \approx 100 + 8,000 = \mathbf{8,100\text{ ns}} = 8.1\,\mu\text{s}$$
> Tốc độ máy tính bị **chậm đi tới 81 lần**!
> Để hiệu năng toàn hệ thống không bị suy giảm quá $10\%$ ($\text{EAT} \le 110\text{ ns}$):
> $$100 + p \times 8,000,000 \le 110 \iff p \le \frac{10}{8,000,000} = \mathbf{0.00000125} = 1.25 \times 10^{-6}$$
> Tỷ lệ Page Fault bắt buộc phải được giữ ở mức **ít hơn 1 lần lỗi trên mỗi 800,000 lần truy cập bộ nhớ**! Đó là lý do tại sao thuật toán thay thế trang là bài toán sống còn.

---

## 2. Bốn Thuật Toán Thay Thế Trang Kinh Điển

| Thuật toán | Nguyên lý chọn trang Nạn nhân | Miễn nhiễm Belady? | Độ phức tạp cài đặt |
|---|---|---|---|
| **Optimal (OPT / Belady MIN)** | Chọn trang mà trong tương lai **lâu nhất mới được dùng lại** | **Có** (Thuộc nhóm Stack Algorithm) | **Không khả thi** trong thực tế (không biết tương lai); dùng làm Benchmark chuẩn |
| **FIFO (First-In, First-Out)** | Chọn trang được nạp vào RAM **sớm nhất** theo hàng đợi Queue | **KHÔNG** (Dính Nghịch lý Belady) | Rất đơn giản ($O(1)$) |
| **LRU (Least Recently Used)** | Chọn trang đã **lâu nhất trong quá khứ chưa được truy cập** | **Có** (Thuộc nhóm Stack Algorithm) | Rất cao nếu làm chuẩn phần cứng (cần cập nhật con trỏ mỗi lệnh CPU) |
| **Clock (Second-Chance)** | Duyệt vòng tròn kim đồng hồ, kiểm tra **Reference Bit ($R$)** | Gần như miễn nhiễm | Cực kỳ nhẹ nhàng, chuẩn công nghiệp của Linux/Windows |

---

### 1. Thuật toán Tối ưu (Optimal Algorithm - OPT):
Thuật toán thay thế trang mà trong tương lai xa nhất mới được tham chiếu lại (hoặc không bao giờ được tham chiếu nữa).
Đã được chứng minh toán học là đạt **số lần Page Fault tối thiểu tuyệt đối** trên bất kỳ chuỗi tham chiếu nào. Vì Kernel không thể nhìn thấu tương lai của người dùng, OPT chỉ được dùng trong nghiên cứu lý thuyết để làm thang đo đánh giá xem các thuật toán thực tế đạt được bao nhiêu phần trăm hiệu năng tối ưu.

---

### 2. Thuật toán FIFO & Nghịch lý Belady (Belady's Anomaly):
Thông thường, trực giác cho ta thấy: Nếu cấp thêm bộ nhớ RAM cho một chương trình (tăng số khung trang vật lý), số lần bị lỗi thiếu trang phải giảm đi hoặc giữ nguyên.
Tuy nhiên, vào năm 1969, László Bélády đã phát hiện ra một hiện tượng kỳ dị: Với thuật toán FIFO, **tăng số khung trang RAM lại làm TĂNG số lần Page Fault**!

### Chứng minh Nghịch lý Belady bằng Số học:
Xét chuỗi tham chiếu trang kinh điển: **`1, 2, 3, 4, 1, 2, 5, 1, 2, 3, 4, 5`**

#### Kịch bản A: Cấp 3 khung trang RAM (3 Frames):
- Tham chiếu: `1, 2, 3` $\to 3$ Faults (Khung chứa: `[1, 2, 3]`)
- Tham chiếu `4` $\to$ Fault! FIFO đẩy `1` ra đĩa (Khung chứa: `[4, 2, 3]`)
- Tham chiếu `1` $\to$ Fault! FIFO đẩy `2` ra đĩa (Khung chứa: `[4, 1, 3]`)
- Tham chiếu `2` $\to$ Fault! FIFO đẩy `3` ra đĩa (Khung chứa: `[4, 1, 2]`)
- Tham chiếu `5` $\to$ Fault! FIFO đẩy `4` ra đĩa (Khung chứa: `[5, 1, 2]`)
- Tham chiếu `1` $\to$ Trúng (Hit)
- Tham chiếu `2` $\to$ Trúng (Hit)
- Tham chiếu `3` $\to$ Fault! FIFO đẩy `1` ra đĩa (Khung chứa: `[5, 3, 2]`)
- Tham chiếu `4` $\to$ Fault! FIFO đẩy `2` ra đĩa (Khung chứa: `[5, 3, 4]`)
- Tham chiếu `5` $\to$ Trúng (Hit)
$\implies$ **Tổng cộng với 3 Frames: 9 lần Page Fault.**

#### Kịch bản B: Tăng RAM lên 4 khung trang (4 Frames):
- Tham chiếu: `1, 2, 3, 4` $\to 4$ Faults (Khung chứa: `[1, 2, 3, 4]`)
- Tham chiếu `1` $\to$ Trúng (Hit)
- Tham chiếu `2` $\to$ Trúng (Hit)
- Tham chiếu `5` $\to$ Fault! FIFO đẩy `1` ra (Khung: `[5, 2, 3, 4]`)
- Tham chiếu `1` $\to$ Fault! FIFO đẩy `2` ra (Khung: `[5, 1, 3, 4]`)
- Tham chiếu `2` $\to$ Fault! FIFO đẩy `3` ra (Khung: `[5, 1, 2, 4]`)
- Tham chiếu `3` $\to$ Fault! FIFO đẩy `4` ra (Khung: `[5, 1, 2, 3]`)
- Tham chiếu `4` $\to$ Fault! FIFO đẩy `5` ra (Khung: `[4, 1, 2, 3]`)
- Tham chiếu `5` $\to$ Fault! FIFO đẩy `1` ra (Khung: `[4, 5, 2, 3]`)
$\implies$ **Tổng cộng với 4 Frames: 10 lần Page Fault!**
Cấp thêm RAM nhưng hiệu năng lại tệ hơn! Đó chính là Nghịch lý Belady.

---

### 3. Thuật toán LRU (Least Recently Used) & Tính chất Stack Algorithm:
LRU dựa trên nguyên lý Tính cục bộ thời gian (*Temporal Locality*): Trang nào lâu nhất chưa được đụng tới trong quá khứ thì khả năng cao sẽ không được dùng tới trong tương lai gần.
- **Tính chất Stack Algorithm:** Một thuật toán được gọi là Stack Algorithm nếu tập hợp các trang nằm trong bộ nhớ với $N$ khung luôn là một tập con của tập hợp các trang nằm trong bộ nhớ với $N+1$ khung:
  $$M(N, t) \subseteq M(N+1, t)$$
- **Định lý:** Mọi thuật toán thuộc nhóm Stack Algorithm (bao gồm cả OPT và LRU) **được chứng minh toán học là miễn nhiễm 100% với Nghịch lý Belady**.

---

### 4. Thuật toán Đồng Hồ (Clock / Second-Chance Algorithm):
Cài đặt LRU thuần túy bằng phần cứng đòi hỏi cập nhật danh sách liên kết đôi trên mỗi lệnh CPU đọc bộ nhớ, điều này làm chậm vi xử lý. Do đó, các hệ điều hành hiện đại dùng thuật toán **Clock** để xấp xỉ LRU với chi phí cực thấp nhờ cờ **Reference Bit ($R$)** sẵn có trên phần cứng PTE:

```text
CON TRỎ KIM ĐỒNG HỒ (CLOCK POINTER)
                      [ Trang 0 (R=1) ]
                        ^           \
                       /             v
          [ Trang 3 (R=0) ]        [ Trang 1 (R=1) ]
                 ^                   /
                  \                 v
                    [ Trang 2 (R=0) ]  <--- KIM ĐỒNG HỒ TRỎ VÀO ĐÂY!
```

### Cơ chế hoạt động:
- Khi một trang được CPU đọc hoặc ghi, phần cứng MMU tự động bật cờ **$R = 1$**.
- Khi cần tìm trang nạn nhân, kim đồng hồ duyệt vòng tròn qua từng khung trang:
  - Nếu thấy **$R == 1$**: Xóa cờ thành **$R = 0$** (cho trang một "cơ hội thứ hai") và xoay kim sang trang kế tiếp.
  - Nếu thấy **$R == 0$**: Chọn ngay trang này làm **Nạn nhân (Victim)** để thay thế, và dịch kim sang vị trí tiếp theo!

### Thuật toán Đồng hồ Cải tiến (Enhanced Second-Chance):
Kết hợp cặp 2 bit: $(R, M)$ với $R$ là Reference bit và $M$ là Modified (Dirty) bit:
1. **Lớp (0, 0):** Không truy cập gần đây, không bị sửa đổi $\implies$ **Nạn nhân hoàn hảo nhất** (không tốn chi phí I/O ghi ra đĩa).
2. **Lớp (0, 1):** Không truy cập gần đây, nhưng đã bị sửa đổi $\implies$ Cần ghi ra đĩa trước khi thay thế.
3. **Lớp (1, 0):** Vừa được truy cập, chưa bị sửa đổi $\implies$ Xóa $R=0$ và bỏ qua trong lượt quét đầu.
4. **Lớp (1, 1):** Vừa được truy cập và đã bị sửa đổi.

---

## 3. Lần Vết So Sánh Thực Tế Từng Bước (Comparative Trace)

Cho chuỗi tham chiếu gồm 13 truy cập: **`7, 0, 1, 2, 0, 3, 0, 4, 2, 3, 0, 3, 2`** với hệ thống có **3 khung trang RAM vật lý**.

| Tham chiếu | 7 | 0 | 1 | 2 | 0 | 3 | 0 | 4 | 2 | 3 | 0 | 3 | 2 | Tổng Page Faults |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **OPT** | **7** | **0** | **1** | **2** (thế 7) | 0 (Hit) | **3** (thế 1) | 0 (Hit) | **4** (thế 2) | **2** (thế 4) | 3 (Hit) | 0 (Hit) | 3 (Hit) | 2 (Hit) | **7 Faults** (Tối ưu) |
| **LRU** | **7** | **0** | **1** | **2** (thế 7) | 0 (Hit) | **3** (thế 1) | 0 (Hit) | **4** (thế 2) | **2** (thế 3) | **3** (thế 0) | **0** (thế 4) | 3 (Hit) | 2 (Hit) | **8 Faults** |
| **Clock** | **7** | **0** | **1** | **2** (thế 7) | 0 (Hit) | **3** (thế 1) | 0 (Hit) | **4** (thế 2) | **2** (thế 3) | **3** (thế 0) | **0** (thế 4) | 3 (Hit) | 2 (Hit) | **9 Faults** |
| **FIFO** | **7** | **0** | **1** | **2** (thế 7) | **0** (thế 0) | **3** (thế 1) | 0 (Hit) | **4** (thế 2) | **2** (thế 3) | **3** (thế 0) | **0** (thế 4) | 3 (Hit) | 2 (Hit) | **10 Faults** |

---

## 4. Hiện Tượng Nghẽn Hoán Đổi (Thrashing) & Mô Hình Working Set

### Hiểm họa Thrashing (Trì trệ hệ thống):
**Thrashing** là trạng thái bệnh lý nghiêm trọng mà hệ thống dành phần lớn thời gian để hoán đổi trang ra vào ổ đĩa (*Paging I/O*) thay vì thực thi các dòng lệnh hữu ích.

```text
PHẢN ỨNG DÂY CHUYỀN DẪN TỚI SỤP ĐỔ HỆ THỐNG:
1. Mức độ đa chương (Degree of Multiprogramming) tăng quá cao
   |
   v
2. Tổng nhu cầu bộ nhớ vượt quá dung lượng RAM thực tế
   |
   v
3. Các tiến trình liên tục bị Page Fault -> Phải ngủ chờ nạp đĩa
   |
   v
4. CPU Scheduler thấy CPU rảnh rỗi (CPU Utilization tụt dốc)
   |
   v
5. Scheduler lầm tưởng hệ thống còn rảnh rỗi -> Nạp THÊM tiến trình mới vào RAM!
   |
   v
6. RAM cạn kiệt hoàn toàn -> Toàn bộ tiến trình đứng im đợi đĩa -> SẬP ĐỘ TRỄ!
```

```text
ĐỒ THỊ HIỆU SUẤT CPU THEO MỨC ĐỘ ĐA CHƯƠNG:
CPU Utilization
   ^
100%|              /-------\
    |             /         \
    |            /           \
    |           /             \  <--- BẮT ĐẦU THRASHING!
    |          /               \
    |         /                 \
  0% +--------+------------------+---------> Degree of Multiprogramming
              Điểm tối ưu         Hệ thống sụp đổ
```

---

### Mô hình Tập làm việc (Working Set Model của Peter Denning):
Để ngăn chặn Thrashing, Peter Denning đề xuất mô hình **Working Set**:
- **Cửa sổ tham chiếu $\Delta$:** Một số lượng cố định các lệnh tham chiếu bộ nhớ gần nhất.
- **Tập làm việc $W(t, \Delta)$:** Tập hợp các trang duy nhất được tiến trình tham chiếu trong cửa sổ thời gian $[t - \Delta, t]$.
- **Kích thước tập làm việc $WSS_i = |W_i(t, \Delta)|$:** Lượng khung trang tối thiểu mà tiến trình $P_i$ cần để chạy mượt mà mà không bị Page Fault liên tục.
- **Tổng nhu cầu bộ nhớ toàn hệ thống:**
  $$D = \sum_{i=1}^n WSS_i$$
- **Chiến lược điều phối của Kernel:**
  - Nếu $D \le \text{Total RAM Frames}$: Hệ thống an toàn, có thể chấp nhận thêm tiến trình mới.
  - Nếu $D > \text{Total RAM Frames}$: **Nguy cơ Thrashing bùng nổ!** Kernel phải chủ động đình chỉ (**Suspend / Swap out**) toàn bộ một tiến trình ra đĩa để giải phóng đủ khung trang cho các tiến trình còn lại đạt được Working Set của chúng.

---

## 5. Giám Sát & Xử Lý Bộ Nhớ trên Linux Sản Xuất

### 1. Nhận diện Thrashing qua `vmstat 1`:
```bash
$ vmstat 1
procs -----------memory---------- ---swap-- -----io---- -system-- ------cpu-----
 r  b   swpd   free   buff  cache   si   so    bi    bo   in   cs us sy id wa st
 2  8 450210  12450   1200  45100 15200 18400 16000 19000 4500 8500  2  8  0 90  0
```
- **Cột `si` (Swap-In) và `so` (Swap-Out):** Đang nhảy vọt lên hàng chục Megabytes/giây liên tục!
- **Cột `wa` (I/O Wait):** Vọt lên **$90\%$**! CPU hầu như không xử lý lệnh của người dùng (`us = 2%`) mà đang bị "đóng băng" chờ đợi ổ đĩa quay. Hệ thống đang bị **Thrashing cấp độ báo động đỏ**.

### 2. Cơ chế OOM Killer (Out-Of-Memory Killer):
Khi RAM vật lý và Swap Space hoàn toàn cạn kiệt, Kernel Linux buộc phải kích hoạt cơ chế sinh tồn cuối cùng: **OOM Killer** để bắn hạ một tiến trình nhằm cứu sống máy chủ.
Kernel tính toán điểm số **`oom_score`** (từ 0 đến 1000) dựa trên tỷ lệ phần trăm RAM mà tiến trình đó đang nuốt trọn:
```bash
# Kiểm tra điểm nguy cơ bị giết của tiến trình
$ cat /proc/$(pgrep java)/oom_score
850

# Bảo vệ tiến trình trọng yếu (như Database, Nginx) không bao giờ bị OOM Killer bắn hạ:
$ sudo echo -1000 > /proc/$(pgrep postgres)/oom_score_adj
```

---

## 6. Câu hỏi Tự kiểm tra Học thuật (Self-Check Questions)

### Câu 1: Tại sao thuật toán thay thế trang LRU được chứng minh là không bao giờ gặp phải Nghịch lý Belady?
> **Đáp án:** Vì LRU thuộc nhóm **Thuật toán Ngăn xếp (Stack Algorithm)**. Trong thuật toán ngăn xếp, tập hợp $N$ trang được lưu trong bộ nhớ luôn luôn là tập con của tập hợp $N+1$ trang nếu ta cấp thêm một khung trang ($M(N) \subset M(N+1)$). Do đó, việc bổ sung thêm khung trang chỉ có thể làm tăng số lần tìm thấy trang hoặc giữ nguyên, chứ không bao giờ làm tăng số lần Page Fault.

### Câu 2: Trong thuật toán Đồng hồ (Clock), nếu tất cả các trang trong bộ nhớ đều có cờ $R = 1$ khi xảy ra Page Fault, thuật toán sẽ xử lý như thế nào?
> **Đáp án:** Kim đồng hồ sẽ duyệt qua toàn bộ các trang trong vòng tròn một lượt, xóa toàn bộ cờ $R = 1$ thành $R = 0$. Khi kim quay trở lại vị trí trang ban đầu, trang này lúc này đã có $R = 0$, do đó nó sẽ lập tức bị chọn làm nạn nhân (Victim). Trong trường hợp này, thuật toán Clock thoái hóa thành thuật toán FIFO thuần túy.

### Câu 3: Kích thước cửa sổ tham chiếu $\Delta$ trong mô hình Working Set ảnh hưởng như thế nào đến khả năng chống Thrashing của hệ thống?
> **Đáp án:**
> - Nếu chọn $\Delta$ quá nhỏ: Tập làm việc $W(t, \Delta)$ sẽ không bao phủ đủ toàn bộ các trang mà tiến trình thực sự cần trong vòng lặp hiện tại, dẫn tới hệ thống vẫn bị Page Fault và Thrashing.
> - Nếu chọn $\Delta$ quá lớn: Tập làm việc sẽ chứa cả những trang cũ không còn dùng nữa, làm lãng phí dung lượng RAM và hạn chế mức độ đa chương (Degree of Multiprogramming) của máy chủ.

### Câu 4: Một hệ thống có thời gian truy cập RAM là $100\text{ ns}$ và thời gian xử lý Page Fault qua đĩa là $10\text{ ms}$. Hỏi tỷ lệ lỗi trang $p$ tối đa là bao nhiêu để thời gian truy cập hiệu dụng không bị chậm đi quá 2 lần so với truy cập RAM thông thường?
> **Đáp án:**
> Chậm đi không quá 2 lần nghĩa là $\text{EAT} \le 2 \times 100\text{ ns} = 200\text{ ns}$.
> Ta có công thức:
> $$\text{EAT} = (1 - p) \times 100 + p \times 10,000,000 \approx 100 + 10,000,000p$$
> Đặt bất đẳng thức:
> $$100 + 10,000,000p \le 200 \iff 10,000,000p \le 100 \iff p \le \frac{100}{10,000,000} = \mathbf{0.00001} = 10^{-5}$$
> Tỷ lệ Page Fault phải nhỏ hơn **1 lần lỗi trên mỗi 100,000 lần truy cập bộ nhớ** ($p \le 0.001\%$).

<!-- studxy-os-v4 -->
"""";

    private static async Task EnsureOsActivitiesAsync(SwmsDbContext db, Guid courseId, DateTimeOffset now)
    {
        var desired = new List<PracticeActivity>
        {
            // Lesson 1.1: Process, Thread & Context Switch
            MakeActivity(courseId, OsLesson11Id, 1, "predict_output",
                "Đoán Output JS: Chia sẻ Heap vs Stack giữa 2 Thread mô phỏng",
                "Dự đoán giá trị biến chia sẻ và biến cục bộ khi 2 luồng chạy xen kẽ.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l11-pred-1",
                            difficulty = "Dễ",
                            title = "Heap dùng chung vs Stack riêng của từng Thread",
                            description = "Dự đoán 2 dòng console.log khi 2 luồng cùng cập nhật object Heap nhưng giữ biến cục bộ Stack.",
                            code = "const sharedHeap = { count: 0 };\nfunction threadRun(localVal) {\n  localVal += 10;\n  sharedHeap.count += 1;\n  return localVal;\n}\nconst s1 = threadRun(5);\nconst s2 = threadRun(5);\nconsole.log(sharedHeap.count);\nconsole.log(s1 + ':' + s2);",
                            expectedOutput = "2\n15:15",
                            hint = "sharedHeap dùng chung nên tăng 2 lần thành 2; biến localVal nằm trên Stack riêng mỗi lần gọi nên đều trả về 15.",
                            explanation = "Trong mô hình Multi-threading, Heap và Global dùng chung (count = 2), còn Stack riêng của mỗi luồng giữ bản sao localVal độc lập (15 và 15)."
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson11Id, 2, "fill_code",
                "Điền Code OS: Mô phỏng lưu & khôi phục PCB khi Context Switch",
                "Hoàn chỉnh hàm chuyển ngữ cảnh từ tiến trình đang chạy sang tiến trình tiếp theo.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l11-fill-1",
                            title = "Lưu trạng thái vào PCB khi Context Switch",
                            category = "os",
                            language = "javascript",
                            description = "Điền từ khóa cập nhật trạng thái PCB cũ và nạp thanh ghi cho PCB mới.",
                            practicalScenario = "Bộ lập lịch Kernel ngắt tiến trình A (hết quantum) để nhường CPU cho tiến trình B.",
                            codeTemplate = new[]
                            {
                                "function contextSwitch(currentPcb, nextPcb, cpuRegs) {",
                                "  currentPcb.savedRegs = { ...cpuRegs };",
                                "  currentPcb.state = '{{BLANK_0}}';",
                                "  nextPcb.state = '{{BLANK_1}}';",
                                "  return { ...nextPcb.{{BLANK_2}} };",
                                "}"
                            },
                            blanks = new object[]
                            {
                                new { id = "b0", blankIndex = 0, expected = "Ready", acceptableAnswers = new[] { "Ready", "READY" }, hint = "Tiến trình bị ngắt hết quantum quay về hàng đợi sẵn sàng.", chips = new[] { "Ready", "Terminated", "New" }, explanation = "Khi bị preempt (hết lát cắt thời gian), tiến trình về trạng thái Ready chờ lượt tiếp theo." },
                                new { id = "b1", blankIndex = 1, expected = "Running", acceptableAnswers = new[] { "Running", "RUNNING" }, hint = "Trạng thái của tiến trình được nhận CPU.", chips = new[] { "Running", "Blocked", "Waiting" }, explanation = "Tiến trình kế tiếp chuyển từ Ready sang Running trên CPU." },
                                new { id = "b2", blankIndex = 2, expected = "savedRegs", acceptableAnswers = new[] { "savedRegs" }, hint = "Trường lưu bộ thanh ghi đã cất của tiến trình mới.", chips = new[] { "savedRegs", "pid", "stackSize" }, explanation = "Nạp lại tập thanh ghi đã lưu từ PCB của tiến trình kế tiếp lên CPU." }
                            },
                            lineExplanations = Array.Empty<object>()
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson11Id, 3, "bug_fix",
                "Sửa Bug OS: Sai phạm vi chia sẻ bộ nhớ khi tạo Process mới",
                "Sửa lỗi bản sao địa chỉ dùng chung khiến Process con ghi đè trực tiếp bộ nhớ của Process cha.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l11-bug-1",
                            title = "Cô lập không gian địa chỉ khi fork() Process con",
                            language = "javascript",
                            categoryTag = "OS Process Isolation",
                            context = "Process con sau fork() phải có bản sao không gian nhớ riêng (Copy-on-Write) chứ không được chia sẻ cùng tham chiếu với Process cha.",
                            buggyCode = "function forkProcess(parentMem) {\n  const childMem = parentMem;\n  childMem.stackTop = 999;\n  return { parentTop: parentMem.stackTop, childTop: childMem.stackTop };\n}",
                            bugLineNumber = 2,
                            bugBriefDescription = "Gán trực tiếp parentMem làm parent và child trỏ cùng 1 object nhớ.",
                            rootCauseAnalysis = "Hai Process độc lập không dùng chung không gian địa chỉ ảo. Cần sao chép tách biệt ({ ...parentMem }) để bảo toàn cô lập.",
                            cleanFixedCode = "function forkProcess(parentMem) {\n  const childMem = { ...parentMem };\n  childMem.stackTop = 999;\n  return { parentTop: parentMem.stackTop, childTop: childMem.stackTop };\n}",
                            requiredTokens = new[] { "{ ...parentMem }|Object.assign" },
                            functionName = "forkProcess",
                            testCases = new object[]
                            {
                                new { name = "Parent giữ nguyên stackTop = 100", input = new object[] { new { stackTop = 100 } }, expected = new { parentTop = 100, childTop = 999 } }
                            },
                            explanation = "Sao chép không gian địa chỉ tách biệt giúp đảm bảo tính cô lập Process Isolation."
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson11Id, 4, "js_sandbox",
                "Code Lab OS: Tính tổng chi phí Context Switch trong 1 giây",
                "Viết hàm calcSwitchOverheadPct(quantumMs, switchMs) trả về phần trăm CPU bị hao phí cho chuyển ngữ cảnh.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l11-js-1",
                            difficulty = "Dễ",
                            title = "Tính % Overhead của Context Switch",
                            description = "Một chu kỳ gồm quantumMs chạy tác vụ cộng với switchMs chuyển ngữ cảnh. Trả về tỷ lệ % thời gian chuyển ngữ cảnh làm tròn 2 chữ số thập phân.",
                            functionName = "calcSwitchOverheadPct",
                            starterCode = "function calcSwitchOverheadPct(quantumMs, switchMs) {\n  // TODO: trả về Number( (switchMs / (quantumMs + switchMs) * 100).toFixed(2) )\n  return 0;\n}",
                            solutionCode = "function calcSwitchOverheadPct(quantumMs, switchMs) {\n  return Number(((switchMs / (quantumMs + switchMs)) * 100).toFixed(2));\n}",
                            hint = "Tổng thời gian một chu kỳ là quantumMs + switchMs.",
                            testCases = new object[]
                            {
                                new { name = "q=9ms, switch=1ms -> 10%", input = new object[] { 9, 1 }, expected = 10 },
                                new { name = "q=19ms, switch=1ms -> 5%", input = new object[] { 19, 1 }, expected = 5 }
                            }
                        }
                    }
                }, now),

            // Lesson 1.2: CPU Scheduling
            MakeActivity(courseId, OsLesson12Id, 1, "predict_output",
                "Đoán Output JS: Tính Turnaround & Waiting Time theo FCFS",
                "Theo dõi hiệu ứng Convoy Effect khi tiến trình dài chạy trước các tiến trình ngắn.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l12-pred-1",
                            difficulty = "Trung bình",
                            title = "Convoy Effect trong điều phối FCFS",
                            description = "3 tiến trình đến cùng lúc t=0 với burst [24, 3, 3]. Dự đoán Average Waiting Time.",
                            code = "const bursts = [24, 3, 3];\nlet time = 0;\nlet totalWait = 0;\nfor (const b of bursts) {\n  totalWait += time;\n  time += b;\n}\nconsole.log(totalWait / bursts.length);",
                            expectedOutput = "17",
                            hint = "Waiting times lần lượt là 0, 24 và 27. Trung bình = (0 + 24 + 27) / 3.",
                            explanation = "Tiến trình 24ms chạy trước khiến 2 tiến trình 3ms phải chờ 24ms và 27ms → Trung bình chờ 17ms!"
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson12Id, 2, "fill_code",
                "Điền Code OS: Bộ lọc xếp hàng SJF (Shortest Job First)",
                "Hoàn chỉnh logic sắp xếp tiến trình theo thời gian chạy ngắn nhất.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l12-fill-1",
                            title = "Sắp xếp hàng đợi theo Burst Time tăng dần (SJF)",
                            category = "os",
                            language = "javascript",
                            description = "Điền biểu thức so sánh và biến cộng dồn thời gian hoàn thành.",
                            practicalScenario = "Giảm thiểu Average Turnaround Time cho các batch jobs đến cùng thời điểm t=0.",
                            codeTemplate = new[]
                            {
                                "function sjfTurnaroundSum(bursts) {",
                                "  const sorted = [...bursts].sort((a, b) => {{BLANK_0}});",
                                "  let clock = 0, totalTurnaround = 0;",
                                "  for (const b of sorted) {",
                                "    clock += {{BLANK_1}};",
                                "    totalTurnaround += {{BLANK_2}};",
                                "  }",
                                "  return totalTurnaround;",
                                "}"
                            },
                            blanks = new object[]
                            {
                                new { id = "b0", blankIndex = 0, expected = "a - b", acceptableAnswers = new[] { "a - b", "a-b" }, hint = "Sắp xếp số tăng dần trong JS sort.", chips = new[] { "a - b", "b - a", "a + b" }, explanation = "SJF chọn job có burst time nhỏ nhất chạy trước." },
                                new { id = "b1", blankIndex = 1, expected = "b", acceptableAnswers = new[] { "b" }, hint = "Cộng thêm thời lượng của job hiện tại vào đồng hồ.", chips = new[] { "b", "1", "sorted.length" }, explanation = "Mỗi tiến trình chạy hết b đơn vị thời gian." },
                                new { id = "b2", blankIndex = 2, expected = "clock", acceptableAnswers = new[] { "clock" }, hint = "Turnaround time (arrival=0) chính là thời điểm hoàn thành clock.", chips = new[] { "clock", "b", "totalTurnaround" }, explanation = "Do arrival = 0 nên Turnaround = Completion Time = clock." }
                            },
                            lineExplanations = Array.Empty<object>()
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson12Id, 3, "bug_fix",
                "Sửa Bug OS: Round Robin không trừ đúng phần dư Quantum",
                "Sửa lỗi tính thời gian chạy còn lại khiến vòng lặp Round Robin chạy vô tận hoặc sai thời gian.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l12-bug-1",
                            title = "Sửa lỗi tính lát cắt Round Robin",
                            language = "javascript",
                            categoryTag = "OS CPU Scheduling",
                            context = "Hàm calcRrCompletion(bursts, quantum) phải trả về mảng thời điểm hoàn thành của các tiến trình đến tại t=0.",
                            buggyCode = "function calcRrTotalTime(bursts, quantum) {\n  const rem = [...bursts];\n  let t = 0;\n  while (rem.some(x => x > 0)) {\n    for (let i = 0; i < rem.length; i++) {\n      if (rem[i] <= 0) continue;\n      const slice = Math.max(rem[i], quantum);\n      rem[i] -= slice;\n      t += slice;\n    }\n  }\n  return t;\n}",
                            bugLineNumber = 7,
                            bugBriefDescription = "Dùng Math.max thay vì Math.min khiến tiến trình chạy quá burst thật!",
                            rootCauseAnalysis = "Lát cắt thực thi chỉ tối đa bằng min(remaining, quantum). Dùng Math.max làm t bị cộng dư thừa.",
                            cleanFixedCode = "function calcRrTotalTime(bursts, quantum) {\n  const rem = [...bursts];\n  let t = 0;\n  while (rem.some(x => x > 0)) {\n    for (let i = 0; i < rem.length; i++) {\n      if (rem[i] <= 0) continue;\n      const slice = Math.min(rem[i], quantum);\n      rem[i] -= slice;\n      t += slice;\n    }\n  }\n  return t;\n}",
                            requiredTokens = new[] { "Math.min" },
                            functionName = "calcRrTotalTime",
                            testCases = new object[]
                            {
                                new { name = "[5, 3] với q=2 -> tổng 8", input = new object[] { new[] { 5, 3 }, 2 }, expected = 8 }
                            },
                            explanation = "slice = Math.min(rem[i], quantum) đảm bảo không chạy vượt quá phần còn lại."
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson12Id, 4, "js_sandbox",
                "Code Lab OS: Mô phỏng Average Turnaround Time của Round Robin",
                "Viết hàm calcAvgTurnaroundRR(bursts, quantum) với các tiến trình đều đến tại t = 0.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l12-js-1",
                            difficulty = "Trung bình",
                            title = "Tính Average Turnaround Time Round Robin",
                            description = "Trả về thời gian hoàn thành trung bình (làm tròn 2 chữ số thập phân) khi các tiến trình chạy xoay vòng theo quantum.",
                            functionName = "calcAvgTurnaroundRR",
                            starterCode = "function calcAvgTurnaroundRR(bursts, quantum) {\n  const rem = [...bursts];\n  const doneAt = new Array(bursts.length).fill(0);\n  let t = 0;\n  // TODO: mô phỏng vòng lặp Round Robin\n  return 0;\n}",
                            solutionCode = "function calcAvgTurnaroundRR(bursts, quantum) {\n  const rem = [...bursts];\n  const doneAt = new Array(bursts.length).fill(0);\n  let t = 0;\n  while (rem.some(x => x > 0)) {\n    for (let i = 0; i < rem.length; i++) {\n      if (rem[i] <= 0) continue;\n      const slice = Math.min(rem[i], quantum);\n      rem[i] -= slice;\n      t += slice;\n      if (rem[i] === 0) doneAt[i] = t;\n    }\n  }\n  return Number((doneAt.reduce((a, b) => a + b, 0) / bursts.length).toFixed(2));\n}",
                            hint = "Khi rem[i] về 0 thì ghi nhận doneAt[i] = t.",
                            testCases = new object[]
                            {
                                new { name = "[4, 2] q=2 -> (6+4)/2 = 5", input = new object[] { new[] { 4, 2 }, 2 }, expected = 5 },
                                new { name = "[6, 3, 1] q=3 -> (10+6+7)/3 = 7.67", input = new object[] { new[] { 6, 3, 1 }, 3 }, expected = 7.67 }
                            }
                        }
                    }
                }, now),

            // Lesson 2.1: Synchronization, Mutex, Semaphore
            MakeActivity(courseId, OsLesson21Id, 1, "predict_output",
                "Đoán Output JS: Lost Update Race Condition vs Mutex",
                "Theo dõi hiện tượng mất dữ liệu khi 2 tác vụ đọc cùng phiên bản cũ và ghi đè.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l21-pred-1",
                            difficulty = "Dễ",
                            title = "Lost Update khi không có Mutex",
                            description = "Hai luồng cùng đọc balance=100 trước khi trừ 40 và trừ 30.",
                            code = "let balance = 100;\nconst t1Read = balance;\nconst t2Read = balance;\nbalance = t1Read - 40;\nbalance = t2Read - 30;\nconsole.log(balance);",
                            expectedOutput = "70",
                            hint = "t2Read vẫn giữ giá trị cũ 100, nên lần ghi sau ghi đè thành 100 - 30 = 70 (mất giao dịch trừ 40!).",
                            explanation = "Không có Critical Section, t2 ghi đè lên kết quả của t1 gây mất mát 40đ."
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson21Id, 2, "fill_code",
                "Điền Code OS: Bộ đếm Semaphore giới hạn K truy cập đồng thời",
                "Hoàn chỉnh lớp CountingSemaphore mô phỏng Acquire và Release.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l21-fill-1",
                            title = "Cài đặt Counting Semaphore Wait/Signal",
                            category = "os",
                            language = "javascript",
                            description = "Điền điều kiện kiểm tra permit và phép giải phóng slot.",
                            practicalScenario = "Giới hạn tối đa 5 worker AI chạy song song như SemaphoreSlim(5) trong Studxy Outbox.",
                            codeTemplate = new[]
                            {
                                "class SimpleSemaphore {",
                                "  constructor(maxCount) { this.permits = maxCount; }",
                                "  tryAcquire() {",
                                "    if (this.permits {{BLANK_0}} 0) { this.permits{{BLANK_1}}; return true; }",
                                "    return false;",
                                "  }",
                                "  release() { this.permits{{BLANK_2}}; }",
                                "}"
                            },
                            blanks = new object[]
                            {
                                new { id = "b0", blankIndex = 0, expected = ">", acceptableAnswers = new[] { ">" }, hint = "Chỉ cấp quyền khi còn ít nhất 1 slot.", chips = new[] { ">", "==", "<" }, explanation = "Khi permits > 0 nghĩa là còn chỗ trống để vào Critical Section." },
                                new { id = "b1", blankIndex = 1, expected = "--", acceptableAnswers = new[] { "--", "-= 1" }, hint = "Giảm số slot trống đi 1.", chips = new[] { "--", "++", "= 0" }, explanation = "Acquire làm giảm số permits hiện có đi 1." },
                                new { id = "b2", blankIndex = 2, expected = "++", acceptableAnswers = new[] { "++", "+= 1" }, hint = "Trả lại slot khi hoàn thành.", chips = new[] { "++", "--", "= 1" }, explanation = "Release tăng số permits lên 1 để báo hiệu slot trống." }
                            },
                            lineExplanations = Array.Empty<object>()
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson21Id, 3, "bug_fix",
                "Sửa Bug OS: Không mở khóa Mutex khi có ngoại lệ (Lock Leak)",
                "Sửa lỗi quên nhả khóa Mutex khi logic bên trong vùng tranh chấp ném lỗi.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l21-bug-1",
                            title = "Đảm bảo luôn Release Mutex trong khối finally",
                            language = "javascript",
                            categoryTag = "OS Synchronization",
                            context = "Nếu taskFn() ném lỗi mà không dùng try...finally, lock.locked sẽ kẹt true mãi mãi khiến mọi luồng sau bị treo!",
                            buggyCode = "function runWithLock(lock, taskFn) {\n  lock.locked = true;\n  const res = taskFn();\n  lock.locked = false;\n  return res;\n}",
                            bugLineNumber = 3,
                            bugBriefDescription = "Không bọc try...finally nên khi taskFn() lỗi, lock.locked không bao giờ về false.",
                            rootCauseAnalysis = "Luôn đặt thao tác nhả khóa (Release/Unlock) trong khối finally để tránh Lock Leak.",
                            cleanFixedCode = "function runWithLock(lock, taskFn) {\n  lock.locked = true;\n  try {\n    return taskFn();\n  } finally {\n    lock.locked = false;\n  }\n}",
                            requiredTokens = new[] { "finally" },
                            functionName = "runWithLock",
                            testCases = new object[]
                            {
                                new { name = "Thành công và nhả khóa", input = new object[] { }, expected = true }
                            },
                            explanation = "try { ... } finally { lock.locked = false; } bảo đảm an toàn kể cả khi ném Exception."
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson21Id, 4, "js_sandbox",
                "Code Lab OS: Mô phỏng bộ đệm Producer-Consumer có giới hạn",
                "Viết hàm simulateBufferOps(capacity, ops) trả về kích thước hàng đợi cuối cùng và số lần bị chặn (rejected).",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l21-js-1",
                            difficulty = "Trung bình",
                            title = "Bounded Buffer Producer-Consumer",
                            description = "ops là mảng các chuỗi 'P' (Produce) hoặc 'C' (Consume). Nếu P khi đầy hoặc C khi rỗng thì tăng rejected.",
                            functionName = "simulateBufferOps",
                            starterCode = "function simulateBufferOps(capacity, ops) {\n  let count = 0, rejected = 0;\n  // TODO: duyệt mảng ops\n  return { count, rejected };\n}",
                            solutionCode = "function simulateBufferOps(capacity, ops) {\n  let count = 0, rejected = 0;\n  for (const op of ops) {\n    if (op === 'P') {\n      if (count < capacity) count++; else rejected++;\n    } else if (op === 'C') {\n      if (count > 0) count--; else rejected++;\n    }\n  }\n  return { count, rejected };\n}",
                            hint = "Kiểm tra count < capacity khi 'P' và count > 0 khi 'C'.",
                            testCases = new object[]
                            {
                                new { name = "Cap=2, [P,P,P,C] -> count=1, rejected=1", input = new object[] { 2, new[] { "P", "P", "P", "C" } }, expected = new { count = 1, rejected = 1 } },
                                new { name = "Cap=1, [C,P] -> count=1, rejected=1", input = new object[] { 1, new[] { "C", "P" } }, expected = new { count = 1, rejected = 1 } }
                            }
                        }
                    }
                }, now),

            // Lesson 2.2: Deadlock
            MakeActivity(courseId, OsLesson22Id, 1, "predict_output",
                "Đoán Output JS: Phát hiện chu trình chờ đợi (Circular Wait)",
                "Kiểm tra đồ thị cấp phát tài nguyên có tồn tại Deadlock hay không.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l22-pred-1",
                            difficulty = "Trung bình",
                            title = "Phát hiện Circular Wait trên đồ thị khóa",
                            description = "T1 chờ T2, T2 chờ T3, T3 chờ T1. Dự đoán kết quả kiểm tra chu trình.",
                            code = "const waitsFor = { T1: 'T2', T2: 'T3', T3: 'T1' };\nlet cur = 'T1';\nconst visited = new Set();\nlet hasCycle = false;\nwhile (cur && waitsFor[cur]) {\n  if (visited.has(cur)) { hasCycle = true; break; }\n  visited.add(cur);\n  cur = waitsFor[cur];\n}\nconsole.log(hasCycle);",
                            expectedOutput = "true",
                            hint = "T1 -> T2 -> T3 -> T1 gặp lại T1 đã nằm trong visited.",
                            explanation = "Sự tồn tại chu trình khép kín trong Wait-For Graph là dấu hiệu chắc chắn của Deadlock."
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson22Id, 2, "fill_code",
                "Điền Code OS: Lock Ordering phá vỡ Circular Wait",
                "Hoàn chỉnh hàm chuyển tiền an toàn giữa 2 tài khoản bằng cách luôn khóa ID nhỏ trước.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l22-fill-1",
                            title = "Chuyển khoản an toàn bằng Lock Ordering",
                            category = "os",
                            language = "javascript",
                            description = "Điền biểu thức chọn tài khoản khóa trước và khóa sau để chống Deadlock.",
                            practicalScenario = "T1 chuyển A→B cùng lúc T2 chuyển B→A dễ gây Deadlock nếu không áp dụng thứ tự khóa toàn cục.",
                            codeTemplate = new[]
                            {
                                "function getLockOrder(accA, accB) {",
                                "  const first = accA.id {{BLANK_0}} accB.id ? accA : accB;",
                                "  const second = first === accA ? {{BLANK_1}} : {{BLANK_2}};",
                                "  return [first.id, second.id];",
                                "}"
                            },
                            blanks = new object[]
                            {
                                new { id = "b0", blankIndex = 0, expected = "<", acceptableAnswers = new[] { "<", "<=" }, hint = "Khóa tài khoản có ID nhỏ hơn trước.", chips = new[] { "<", ">", "===" }, explanation = "Mọi luồng đều khóa theo thứ tự ID tăng dần nên không thể tạo chu trình." },
                                new { id = "b1", blankIndex = 1, expected = "accB", acceptableAnswers = new[] { "accB" }, hint = "Nếu first là accA thì second là accB.", chips = new[] { "accB", "accA", "null" }, explanation = "Tài khoản còn lại được khóa thứ hai." },
                                new { id = "b2", blankIndex = 2, expected = "accA", acceptableAnswers = new[] { "accA" }, hint = "Nếu first là accB thì second là accA.", chips = new[] { "accA", "accB", "null" }, explanation = "Bảo đảm thứ tự khóa luôn cố định cho mọi cặp tài khoản." }
                            },
                            lineExplanations = Array.Empty<object>()
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson22Id, 3, "bug_fix",
                "Sửa Bug OS: Điều kiện kiểm tra Safe State của Thuật toán Banker",
                "Sửa lỗi kiểm tra tài nguyên cần thêm (Need <= Available) bị đảo dấu.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l22-bug-1",
                            title = "Sửa dấu điều kiện cấp phát Banker",
                            language = "javascript",
                            categoryTag = "OS Deadlock Avoidance",
                            context = "Một tiến trình chỉ có thể chạy hoàn tất an toàn khi toàn bộ tài nguyên nó cần thêm (need) nhỏ hơn hoặc bằng tài nguyên hệ thống đang rảnh (available).",
                            buggyCode = "function canRunSafely(need, available) {\n  return need.every((n, idx) => n > available[idx]);\n}",
                            bugLineNumber = 2,
                            bugBriefDescription = "So sánh n > available[idx] bị ngược hoàn toàn so với nguyên lý Banker.",
                            rootCauseAnalysis = "Phải kiểm tra n <= available[idx] cho mọi loại tài nguyên.",
                            cleanFixedCode = "function canRunSafely(need, available) {\n  return need.every((n, idx) => n <= available[idx]);\n}",
                            requiredTokens = new[] { "<=" },
                            functionName = "canRunSafely",
                            testCases = new object[]
                            {
                                new { name = "Need [2,1], Avail [3,2] -> true", input = new object[] { new[] { 2, 1 }, new[] { 3, 2 } }, expected = true },
                                new { name = "Need [4,1], Avail [3,2] -> false", input = new object[] { new[] { 4, 1 }, new[] { 3, 2 } }, expected = false }
                            },
                            explanation = "Tiến trình chỉ chạy tới đích được khi Need[i] <= Available[i] với mọi i."
                        }
                    }
                }, now),
            MakeActivity(courseId, OsLesson22Id, 4, "js_sandbox",
                "Code Lab OS: Kiểm tra Trạng thái An toàn (Banker 1 loại tài nguyên)",
                "Viết hàm isSafeSequenceExists(available, allocations, maxNeeds) trả về true/false.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l22-js-1",
                            difficulty = "Khó",
                            title = "Banker Safety Algorithm (1D)",
                            description = "Cho lượng tài nguyên rảnh available, mảng allocations và mảng maxNeeds. Trả về true nếu tồn tại chuỗi hoàn thành tất cả tiến trình mà không Deadlock.",
                            functionName = "isSafeSequenceExists",
                            starterCode = "function isSafeSequenceExists(available, allocations, maxNeeds) {\n  let work = available;\n  const n = allocations.length;\n  const finish = new Array(n).fill(false);\n  // TODO: tìm tiến trình chưa finish có need <= work để giải phóng allocation\n  return false;\n}",
                            solutionCode = "function isSafeSequenceExists(available, allocations, maxNeeds) {\n  let work = available;\n  const n = allocations.length;\n  const finish = new Array(n).fill(false);\n  let progressed = true;\n  while (progressed) {\n    progressed = false;\n    for (let i = 0; i < n; i++) {\n      if (!finish[i] && (maxNeeds[i] - allocations[i]) <= work) {\n        work += allocations[i];\n        finish[i] = true;\n        progressed = true;\n      }\n    }\n  }\n  return finish.every(Boolean);\n}",
                            hint = "need[i] = maxNeeds[i] - allocations[i]. Khi tiến trình chạy xong, work += allocations[i].",
                            testCases = new object[]
                            {
                                new { name = "Safe state: avail=3, alloc=[2,4,2], max=[5,6,9]", input = new object[] { 3, new[] { 2, 4, 2 }, new[] { 5, 6, 9 } }, expected = true },
                                new { name = "Unsafe state: avail=1, alloc=[2,2], max=[5,5]", input = new object[] { 1, new[] { 2, 2 }, new[] { 5, 5 } }, expected = false }
                            }
                        }
                    }
                }, now),

            // Lesson 3.2 (LessonPagingId): EAT, TLB & Multi-Level Page Table
            MakeActivity(courseId, SeedDataInitializer.LessonPagingId, 1, "predict_output",
                "Đoán Output JS: Tính Thời gian Truy cập Bộ nhớ Hiệu dụng (EAT)",
                "Tính toán EAT khi có bộ đệm TLB Hit Ratio = 98%.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l32-pred-1",
                            difficulty = "Dễ",
                            title = "Công thức EAT với TLB Hit 90%",
                            description = "Cho t_TLB = 1ns, t_MEM = 100ns, alpha = 0.9. Dự đoán EAT (ns).",
                            code = "const alpha = 0.9, tTlb = 1, tMem = 100;\nconst eat = alpha * (tTlb + tMem) + (1 - alpha) * (tTlb + 2 * tMem);\nconsole.log(Math.round(eat));",
                            expectedOutput = "111",
                            hint = "0.9 * 101 + 0.1 * 201 = 90.9 + 20.1 = 111.",
                            explanation = "Khi TLB Hit mất 1 + 100 = 101ns; khi Miss mất 1 + 200 = 201ns. Trung bình có trọng số là 111ns."
                        }
                    }
                }, now),
            MakeActivity(courseId, SeedDataInitializer.LessonPagingId, 2, "fill_code",
                "Điền Code OS: Dịch địa chỉ ảo sang khung vật lý từ Page Table",
                "Hoàn chỉnh hàm tách Virtual Page Number (VPN) và Offset với trang 4KB.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l32-fill-1",
                            title = "Ánh xạ Địa chỉ Ảo (VPN + Offset) qua Page Table",
                            category = "os",
                            language = "javascript",
                            description = "Điền phép chia lấy VPN, phép chia dư lấy Offset và ghép khung trang vật lý.",
                            practicalScenario = "MMU tách địa chỉ ảo 32-bit với kích thước trang 4096 bytes (4KB) để tìm địa chỉ RAM thật.",
                            codeTemplate = new[]
                            {
                                "function translateAddress(virtualAddr, pageSize, pageTable) {",
                                "  const vpn = Math.floor(virtualAddr {{BLANK_0}} pageSize);",
                                "  const offset = virtualAddr {{BLANK_1}} pageSize;",
                                "  const pfn = pageTable[vpn];",
                                "  return pfn * pageSize + {{BLANK_2}};",
                                "}"
                            },
                            blanks = new object[]
                            {
                                new { id = "b0", blankIndex = 0, expected = "/", acceptableAnswers = new[] { "/" }, hint = "Chia lấy nguyên cho kích thước trang.", chips = new[] { "/", "%", "*" }, explanation = "VPN = floor(virtualAddr / pageSize)." },
                                new { id = "b1", blankIndex = 1, expected = "%", acceptableAnswers = new[] { "%" }, hint = "Lấy phần dư làm độ lệch trong trang.", chips = new[] { "%", "/", "+" }, explanation = "Offset luôn giữ nguyên không đổi qua quá trình ánh xạ trang." },
                                new { id = "b2", blankIndex = 2, expected = "offset", acceptableAnswers = new[] { "offset" }, hint = "Ghép khung vật lý PFN với độ lệch offset.", chips = new[] { "offset", "vpn", "pageSize" }, explanation = "PhysicalAddress = PFN * PageSize + Offset." }
                            },
                            lineExplanations = Array.Empty<object>()
                        }
                    }
                }, now),
            MakeActivity(courseId, SeedDataInitializer.LessonPagingId, 3, "bug_fix",
                "Sửa Bug OS: Quên cộng chi phí TLB khi xảy ra TLB Miss",
                "Sửa lỗi công thức EAT bỏ quên tTlb ở nhánh Miss.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l32-bug-1",
                            title = "Sửa lỗi công thức Effective Access Time (EAT)",
                            language = "javascript",
                            categoryTag = "OS Memory & TLB",
                            context = "Kể cả khi TLB Miss, CPU vẫn phải mất thời gian tra cứu TLB trước khi biết là Miss, sau đó mới đọc bảng trang và đọc dữ liệu (2 lần RAM).",
                            buggyCode = "function calcEAT(hitRatio, tTlb, tMem) {\n  return hitRatio * (tTlb + tMem) + (1 - hitRatio) * (2 * tMem);\n}",
                            bugLineNumber = 2,
                            bugBriefDescription = "Nhánh (1 - hitRatio) thiếu tTlb.",
                            rootCauseAnalysis = "Tra TLB luôn tốn tTlb bất kể Hit hay Miss, nên nhánh Miss tốn (tTlb + 2 * tMem).",
                            cleanFixedCode = "function calcEAT(hitRatio, tTlb, tMem) {\n  return hitRatio * (tTlb + tMem) + (1 - hitRatio) * (tTlb + 2 * tMem);\n}",
                            requiredTokens = new[] { "tTlb + 2 * tMem|tTlb+2*tMem" },
                            functionName = "calcEAT",
                            testCases = new object[]
                            {
                                new { name = "alpha=0.9, tTlb=1, tMem=100 -> 111", input = new object[] { 0.9, 1, 100 }, expected = 111 }
                            },
                            explanation = "Nhánh TLB Miss phải cộng đủ tTlb + 2 * tMem."
                        }
                    }
                }, now),
            MakeActivity(courseId, SeedDataInitializer.LessonPagingId, 4, "js_sandbox",
                "Code Lab OS: Mô phỏng Tra cứu TLB + Page Table",
                "Viết hàm countTlbHits(pageRefs, tlbCapacity) dùng FIFO/LRU đơn giản để đếm số lần TLB Hit.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l32-js-1",
                            difficulty = "Trung bình",
                            title = "Mô phỏng TLB Cache Hit Counter (LRU)",
                            description = "Cho chuỗi trang tham chiếu pageRefs và kích thước tlbCapacity. Trả về số lần TLB Hit khi thay thế theo LRU.",
                            functionName = "countTlbHits",
                            starterCode = "function countTlbHits(pageRefs, tlbCapacity) {\n  const cache = [];\n  let hits = 0;\n  // TODO: cài đặt LRU cache trên mảng cache\n  return hits;\n}",
                            solutionCode = "function countTlbHits(pageRefs, tlbCapacity) {\n  const cache = [];\n  let hits = 0;\n  for (const p of pageRefs) {\n    const idx = cache.indexOf(p);\n    if (idx !== -1) {\n      hits++;\n      cache.splice(idx, 1);\n      cache.push(p);\n    } else {\n      if (cache.length >= tlbCapacity) cache.shift();\n      cache.push(p);\n    }\n  }\n  return hits;\n}",
                            hint = "Khi Hit: đưa trang lên cuối mảng cache. Khi Miss và đầy: shift() phần tử đầu mảng.",
                            testCases = new object[]
                            {
                                new { name = "[1,2,1,3,1,2] cap=2 -> 1 hit", input = new object[] { new[] { 1, 2, 1, 3, 1, 2 }, 2 }, expected = 1 },
                                new { name = "[1,1,1,2,2] cap=2 -> 3 hits", input = new object[] { new[] { 1, 1, 1, 2, 2 }, 2 }, expected = 3 }
                            }
                        }
                    }
                }, now),

            // Lesson 3.3 (LessonPageReplacementId): FIFO, Optimal, LRU, Clock
            MakeActivity(courseId, SeedDataInitializer.LessonPageReplacementId, 1, "predict_output",
                "Đoán Output JS: Số Page Fault theo thuật toán FIFO",
                "Theo dõi chuỗi trang [1, 2, 3, 1, 4, 5] với 3 khung RAM.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l33-pred-1",
                            difficulty = "Dễ",
                            title = "Đếm Page Fault của thuật toán FIFO",
                            description = "Dự đoán tổng số lần Page Fault khi chạy chuỗi truy cập trên 3 frames.",
                            code = "const refs = [1, 2, 3, 1, 4, 5];\nconst frames = [];\nlet faults = 0;\nfor (const p of refs) {\n  if (!frames.includes(p)) {\n    faults++;\n    if (frames.length === 3) frames.shift();\n    frames.push(p);\n  }\n}\nconsole.log(faults);",
                            expectedOutput = "5",
                            hint = "1(F), 2(F), 3(F), 1(Hit), 4(F đẩy 1), 5(F đẩy 2) -> Tổng cộng 5 lần Page Fault.",
                            explanation = "Chỉ lần truy cập số 1 thứ hai là Hit; cả 5 trang còn lại đều gây Page Fault."
                        }
                    }
                }, now),
            MakeActivity(courseId, SeedDataInitializer.LessonPageReplacementId, 2, "fill_code",
                "Điền Code OS: Thuật toán Clock (Second-Chance)",
                "Hoàn chỉnh vòng xoay kim đồng hồ kiểm tra bit tham chiếu refBit.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l33-fill-1",
                            title = "Kim đồng hồ Clock Page Replacement",
                            category = "os",
                            language = "javascript",
                            description = "Điền thao tác xóa Reference Bit về 0 khi cho cơ hội thứ 2 và bước xoay kim đồng hồ.",
                            practicalScenario = "Nhân Linux sử dụng biến thể thuật toán Clock để chọn trang nạn nhân với chi phí O(1) trung bình.",
                            codeTemplate = new[]
                            {
                                "function selectClockVictim(frames, hand) {",
                                "  while (true) {",
                                "    if (frames[hand].refBit === 0) return hand;",
                                "    frames[hand].refBit = {{BLANK_0}};",
                                "    hand = (hand + 1) {{BLANK_1}} frames.length;",
                                "  }",
                                "}"
                            },
                            blanks = new object[]
                            {
                                new { id = "b0", blankIndex = 0, expected = "0", acceptableAnswers = new[] { "0" }, hint = "Xóa bit tham chiếu để trang chỉ còn 1 cơ hội trước vòng sau.", chips = new[] { "0", "1", "-1" }, explanation = "Nếu refBit=1, thuật toán đặt về 0 (Second-Chance) rồi đi tiếp." },
                                new { id = "b1", blankIndex = 1, expected = "%", acceptableAnswers = new[] { "%" }, hint = "Phép chia lấy dư để quay vòng tròn kim đồng hồ.", chips = new[] { "%", "/", "*" }, explanation = "hand = (hand + 1) % frames.length giữ con trỏ quay tròn trong mảng." }
                            },
                            lineExplanations = Array.Empty<object>()
                        }
                    }
                }, now),
            MakeActivity(courseId, SeedDataInitializer.LessonPageReplacementId, 3, "bug_fix",
                "Sửa Bug OS: Thuật toán LRU không cập nhật vị trí trang khi Hit",
                "Sửa lỗi khiến LRU hoạt động sai thành FIFO.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l33-bug-1",
                            title = "Cập nhật thứ tự sử dụng gần nhất khi Hit trong LRU",
                            language = "javascript",
                            categoryTag = "OS Page Replacement",
                            context = "Khi trang đã có trong RAM (Hit), LRU bắt buộc phải dời trang đó về cuối danh sách (vừa mới dùng) để không bị đẩy nhầm.",
                            buggyCode = "function lruPageFaults(refs, capacity) {\n  const q = [];\n  let faults = 0;\n  for (const p of refs) {\n    if (q.includes(p)) {\n      continue;\n    }\n    faults++;\n    if (q.length === capacity) q.shift();\n    q.push(p);\n  }\n  return faults;\n}",
                            bugLineNumber = 5,
                            bugBriefDescription = "Khi q.includes(p) chỉ continue mà không dời p về cuối mảng q.",
                            rootCauseAnalysis = "Nếu không dời p về cuối mảng khi Hit, q[0] luôn là trang vào sớm nhất (FIFO) chứ không phải ít dùng nhất gần đây (LRU).",
                            cleanFixedCode = "function lruPageFaults(refs, capacity) {\n  const q = [];\n  let faults = 0;\n  for (const p of refs) {\n    const idx = q.indexOf(p);\n    if (idx !== -1) {\n      q.splice(idx, 1);\n      q.push(p);\n      continue;\n    }\n    faults++;\n    if (q.length === capacity) q.shift();\n    q.push(p);\n  }\n  return faults;\n}",
                            requiredTokens = new[] { "splice" },
                            functionName = "lruPageFaults",
                            testCases = new object[]
                            {
                                new { name = "[1,2,3,1,4,2] cap=3 -> 4 faults (LRU giữ 1)", input = new object[] { new[] { 1, 2, 3, 1, 4, 2 }, 3 }, expected = 4 },
                                new { name = "[1,2,1,3,2] cap=2 -> 3 faults (LRU giữ 1 khi Hit nên đẩy 2, rồi 2 fault)", input = new object[] { new[] { 1, 2, 1, 3, 2 }, 2 }, expected = 4 }
                            },
                            explanation = "Dùng q.splice(idx, 1); q.push(p); để đẩy trang vừa truy cập sang trạng thái mới dùng nhất."
                        }
                    }
                }, now),
            MakeActivity(courseId, SeedDataInitializer.LessonPageReplacementId, 4, "js_sandbox",
                "Code Lab OS: Mô phỏng Optimal Page Replacement",
                "Viết hàm optimalPageFaults(refs, capacity) chọn trang lâu nhất trong tương lai mới dùng lại để đẩy ra.",
                new
                {
                    items = new object[]
                    {
                        new
                        {
                            id = "os-l33-js-1",
                            difficulty = "Khó",
                            title = "Mô phỏng thuật toán Optimal (Belady)",
                            description = "Trả về tổng số Page Fault tối thiểu đạt được với thuật toán Optimal.",
                            functionName = "optimalPageFaults",
                            starterCode = "function optimalPageFaults(refs, capacity) {\n  const frames = [];\n  let faults = 0;\n  // TODO: khi đầy, tìm phần tử trong frames có lần xuất hiện tiếp theo xa nhất (hoặc không xuất hiện nữa)\n  return faults;\n}",
                            solutionCode = "function optimalPageFaults(refs, capacity) {\n  const frames = [];\n  let faults = 0;\n  for (let i = 0; i < refs.length; i++) {\n    const p = refs[i];\n    if (frames.includes(p)) continue;\n    faults++;\n    if (frames.length < capacity) {\n      frames.push(p);\n    } else {\n      let victimIdx = 0, farthest = -1;\n      for (let f = 0; f < frames.length; f++) {\n        const nextUse = refs.indexOf(frames[f], i + 1);\n        if (nextUse === -1) { victimIdx = f; break; }\n        if (nextUse > farthest) { farthest = nextUse; victimIdx = f; }\n      }\n      frames[victimIdx] = p;\n    }\n  }\n  return faults;\n}",
                            hint = "Dùng refs.indexOf(frames[f], i + 1). Nếu trả về -1 thì chọn ngay làm nạn nhân.",
                            testCases = new object[]
                            {
                                new { name = "[7,0,1,2,0,3,0,4,2,3,0,3,2] cap=3 -> 7 faults", input = new object[] { new[] { 7, 0, 1, 2, 0, 3, 0, 4, 2, 3, 0, 3, 2 }, 3 }, expected = 7 }
                            }
                        }
                    }
                }, now)
        };

        var existingIds = await db.PracticeActivities
            .Where(pa => pa.CourseId == courseId)
            .Select(pa => pa.Id)
            .ToHashSetAsync();

        var toAdd = desired.Where(a => !existingIds.Contains(a.Id)).ToList();
        if (toAdd.Count > 0)
        {
            db.PracticeActivities.AddRange(toAdd);
            await db.SaveChangesAsync();
        }
    }

    private static PracticeActivity MakeActivity(
        Guid courseId,
        Guid lessonId,
        int orderIndex,
        string type,
        string title,
        string description,
        object config,
        DateTimeOffset now)
    {
        return new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, lessonId, type, orderIndex),
            CourseId = courseId,
            LessonId = lessonId,
            OrderIndex = orderIndex,
            Type = type,
            Title = title,
            Description = description,
            ConfigJson = JsonSerializer.Serialize(config, SafeJsonOptions),
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static async Task EnsureOsFlashcardsAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var specs = new (Guid LessonId, string Front, string Back)[]
        {
            (OsLesson11Id, "Điểm khác biệt lớn nhất giữa Process và Thread về không gian bộ nhớ là gì?", "Process có không gian địa chỉ ảo cô lập hoàn toàn, còn các Thread trong cùng Process dùng chung Heap & Global nhưng giữ Stack và Registers riêng."),
            (OsLesson11Id, "Tại sao Context Switch giữa 2 Process tốn chi phí hơn chuyển giữa 2 Thread cùng Process?", "Chuyển Process phải thay đổi thanh ghi bảng trang (CR3) và thường làm mất hiệu lực TLB (TLB Flush), còn chuyển Thread giữ nguyên bảng trang."),
            (OsLesson12Id, "Hiệu ứng đoàn tàu (Convoy Effect) xảy ra ở thuật toán điều phối CPU nào?", "Thuật toán FCFS (First-Come, First-Served) khi một tiến trình CPU-bound dài chạy trước, làm hàng loạt tiến trình ngắn phải chờ đợi phía sau."),
            (OsLesson12Id, "MLFQ giải quyết bài toán cân bằng Response Time và Turnaround Time bằng cách nào?", "Dùng nhiều hàng đợi ưu tiên giảm dần cho tiến trình ngốn CPU, giữ ưu tiên cho tiến trình I/O-bound và Priority Boost định kỳ để tránh Starvation."),
            (OsLesson21Id, "Vì sao lệnh `counter++` không an toàn (non-atomic) trong môi trường đa luồng?", "Nó gồm 3 bước tách rời LOAD → ADD → STORE; nếu xảy ra Context Switch giữa chừng, hai luồng sẽ ghi đè kết quả của nhau (Race Condition)."),
            (OsLesson21Id, "Sự khác biệt cốt lõi giữa Mutex và Semaphore đếm là gì?", "Mutex là khóa nhị phân có quyền sở hữu (chỉ luồng khóa mới được mở), còn Semaphore đếm cho phép tối đa K luồng truy cập và bất kỳ luồng nào cũng có thể Release()."),
            (OsLesson22Id, "4 điều kiện Coffman dẫn đến Deadlock là gì?", "Mutual Exclusion (loại trừ lẫn nhau), Hold and Wait (giữ và chờ), No Preemption (không trưng dụng) và Circular Wait (chờ đợi vòng tròn)."),
            (OsLesson22Id, "Kỹ thuật Lock Ordering ngăn chặn Deadlock bằng cách phá vỡ điều kiện nào?", "Phá vỡ điều kiện Circular Wait bằng cách bắt buộc mọi luồng lấy khóa theo cùng một thứ tự toàn cục (ví dụ theo ID tăng dần)."),
            (SeedDataInitializer.LessonPageReplacementId, "Nghịch lý Belady (Belady's Anomaly) là gì và xảy ra ở thuật toán nào?", "Hiện tượng tăng số khung trang RAM nhưng số Page Fault lại tăng lên; xảy ra ở thuật toán FIFO (không xảy ra ở LRU hay Optimal)."),
            (SeedDataInitializer.LessonPageReplacementId, "Thuật toán Clock (Second-Chance) chọn trang nạn nhân bằng cách nào?", "Duyệt vòng tròn theo kim đồng hồ kiểm tra Reference Bit: nếu R=1 thì đặt R=0 rồi cho qua (cơ hội thứ 2), gặp R=0 thì thay thế.")
        };

        var osLessonIds = new[] { OsLesson11Id, OsLesson12Id, OsLesson21Id, OsLesson22Id, SeedDataInitializer.LessonPageReplacementId };
        var existingFronts = await db.Flashcards
            .AsNoTracking()
            .Where(f => f.UserId == SeedDataInitializer.DefaultUserId && f.LessonId.HasValue && osLessonIds.Contains(f.LessonId.Value))
            .Select(f => f.FrontAstJson)
            .ToHashSetAsync();

        var osTag = await db.Tags.FirstOrDefaultAsync(t => t.UserId == SeedDataInitializer.DefaultUserId && t.Name == "OS-Paging");
        var newCards = new List<Flashcard>();

        for (int i = 0; i < specs.Length; i++)
        {
            var (lesId, front, back) = specs[i];
            var frontJson = JsonSerializer.Serialize(new { text = front }, SafeJsonOptions);
            if (existingFronts.Contains(frontJson)) continue;

            var cardId = CreateDeterministicGuid($"swms:os-fc:{lesId:N}:{i}");
            if (await db.Flashcards.AnyAsync(f => f.Id == cardId)) continue;

            var card = new Flashcard
            {
                Id = cardId,
                UserId = SeedDataInitializer.DefaultUserId,
                LessonId = lesId,
                CardType = "standard",
                SourceTitleSnapshot = "Hệ điều hành: Kiến trúc Bộ nhớ ảo & Phân trang",
                FrontAstJson = frontJson,
                BackAstJson = JsonSerializer.Serialize(new { text = back }, SafeJsonOptions),
                Stability = 0,
                Difficulty = 0,
                Reps = 0,
                Lapses = 0,
                State = 0,
                DueAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };
            newCards.Add(card);
            existingFronts.Add(frontJson);
            if (osTag != null)
            {
                db.FlashcardTags.Add(new FlashcardTag { FlashcardId = cardId, TagId = osTag.Id, CreatedAt = now });
            }
        }

        if (newCards.Count > 0)
        {
            db.Flashcards.AddRange(newCards);
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureOsQuizzesAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var specs = new List<(string Key, Guid LessonId, string Title, ExamQuestionSpec[] Questions)>
        {
            ("os:l11", OsLesson11Id, "Bài 1.1: Kiểm tra Tiến trình, Luồng & Context Switch", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Thành phần nào được các Thread trong cùng một Process dùng chung với nhau?", "Call Stack riêng biệt", "Bộ thanh ghi CPU Registers", "Heap & Biến toàn cục (Global Data)", "Program Counter (PC)", "C", "Các Thread trong cùng Process chia sẻ Code, Data và Heap, nhưng giữ Stack và CPU Registers riêng."),
                new("multiple_choice", "Cấu trúc dữ liệu nào lưu trạng thái, PID, bộ đếm chương trình và danh sách file đang mở của một tiến trình?", "TLB Cache", "PCB (Process Control Block)", "Page Frame", "Inode Table", "B", "PCB là cấu trúc trung tâm của Kernel quản lý toàn bộ ngữ cảnh một tiến trình."),
                new("cloze", "Khi chuyển ngữ cảnh giữa 2 tiến trình khác nhau, CPU phải đổi thanh ghi {{c1::CR3}} trỏ tới bảng trang mới.", null, null, null, null, "CR3", "Trên x86-64, thanh ghi CR3 giữ địa chỉ gốc của bảng trang PGD của tiến trình hiện hành."),
                new("essay", "Phân tích lý do vì sao chi phí Context Switch giữa hai Process lớn hơn nhiều so với giữa hai Thread cùng Process.", null, null, null, null,
                    "Chuyển Process đòi hỏi đổi thanh ghi bảng trang CR3 và gây TLB Flush/Shootdown làm giảm tỷ lệ TLB Hit, trong khi chuyển Thread dùng chung không gian địa chỉ.",
                    "Đáp án chuẩn nhấn mạnh đổi không gian địa chỉ, CR3 và hiệu ứng TLB Flush.",
                    new[] { "Đề cập dùng chung vs cô lập không gian địa chỉ ảo", "Phân tích tác động lên TLB cache khi chuyển Process" })
            }),
            ("os:l12", OsLesson12Id, "Bài 1.2: Kiểm tra Thuật toán Điều phối CPU", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Hiệu ứng Convoy Effect (các tác vụ ngắn bị kẹt sau một tác vụ dài) là nhược điểm đặc trưng của thuật toán nào?", "Round Robin", "FCFS (First-Come, First-Served)", "MLFQ", "SJF", "B", "FCFS chạy tuần tự theo thứ tự đến không trưng dụng nên gặp hiệu ứng đoàn tàu."),
                new("multiple_choice", "Trong Round Robin, nếu chọn Time Quantum q quá nhỏ so với thời gian Context Switch thì hậu quả là gì?", "Thoái hóa thành FCFS", "Thời gian phản hồi chậm đi", "Chi phí Overhead chuyển ngữ cảnh tăng vọt làm giảm thông lượng CPU", "Gây Starvation ngay lập tức", "C", "Chuyển ngữ cảnh quá dày đặc khiến CPU tốn phần lớn thời gian vào việc lưu/nạp PCB thay vì chạy tác vụ."),
                new("cloze", "Trong MLFQ, cơ chế {{c1::Priority Boost}} được chạy định kỳ để đưa toàn bộ tiến trình lên hàng đợi ưu tiên cao nhất nhằm chống đói tài nguyên.", null, null, null, null, "Priority Boost", "Priority Boost ngăn các tiến trình CPU-bound bị kẹt vĩnh viễn ở hàng đợi thấp."),
                new("multiple_choice", "Chỉ số nào đo khoảng thời gian từ lúc tiến trình xuất hiện đến lúc chạy xong hoàn toàn?", "Response Time", "Turnaround Time", "Wait Ratio", "Throughput", "B", "Turnaround Time = Completion Time - Arrival Time.")
            }),
            ("os:l21", OsLesson21Id, "Bài 2.1: Kiểm tra Đồng bộ hóa, Mutex & Semaphore", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Vì sao câu lệnh `x++` dễ gây Race Condition khi chạy đồng thời trên 2 luồng?", "Vì trình biên dịch tự xóa biến x", "Vì nó gồm 3 thao tác tách rời Load-Add-Store không có tính nguyên tử", "Vì RAM không cho phép đọc đồng thời", "Vì CPU tự khóa biến", "B", "Nếu OS ngắt giữa bước Load và Store, luồng kia sẽ đọc giá trị cũ và ghi đè."),
                new("multiple_choice", "Công cụ nào phù hợp nhất để giới hạn tối đa 5 tác vụ Worker gọi LLM cùng một thời điểm?", "Spinlock", "Counting Semaphore (SemaphoreSlim(5))", "Page Table", "FIFO Queue", "B", "Counting Semaphore quản lý K slot truy cập song song."),
                new("cloze", "Đoạn mã truy cập vào tài nguyên chia sẻ cần được bảo vệ loại trừ lẫn nhau được gọi là {{c1::Critical Section}}.", null, null, null, null, "Critical Section", "Vùng tranh chấp (Critical Section) chỉ cho phép tối đa 1 luồng thực thi tại 1 thời điểm nếu dùng Mutex.")
            }),
            ("os:l22", OsLesson22Id, "Bài 2.2: Kiểm tra Deadlock & Thuật toán Banker", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Điều kiện nào KHÔNG nằm trong 4 điều kiện Coffman gây Deadlock?", "Mutual Exclusion", "Hold and Wait", "Circular Wait", "Temporal Locality", "D", "Temporal Locality thuộc về nguyên lý bộ nhớ đệm Cache/TLB, không phải điều kiện Coffman."),
                new("multiple_choice", "Chiến lược ép mọi luồng phải khóa tài nguyên theo thứ tự ID tăng dần phá vỡ điều kiện nào của Deadlock?", "Mutual Exclusion", "Hold and Wait", "No Preemption", "Circular Wait", "D", "Khóa theo thứ tự toàn cục triệt tiêu khả năng hình thành chu trình chờ đợi khép kín."),
                new("cloze", "Thuật toán tránh Deadlock kiểm tra trạng thái an toàn trước khi cấp phát tài nguyên có tên là {{c1::Banker}}.", null, null, null, null, "Banker", "Thuật toán Banker mô phỏng ngân hàng chỉ cho vay khi giữ được Safe Sequence.")
            }),
            ("os:l33", SeedDataInitializer.LessonPageReplacementId, "Bài 3.3: Kiểm tra Thuật toán Thay thế Trang", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Thuật toán nào có thể gặp Nghịch lý Belady (tăng khung trang RAM nhưng lại tăng số lỗi trang)?", "LRU", "Optimal", "FIFO", "Working Set", "C", "FIFO không thỏa tính chất stack property nên gặp nghịch lý Belady."),
                new("multiple_choice", "Trong thuật toán Clock (Second-Chance), nếu gặp trang có Reference Bit R = 1, hệ điều hành làm gì?", "Đẩy ngay trang đó ra đĩa", "Đặt R = 0 (cho cơ hội thứ 2) và tiến kim đồng hồ sang khung kế tiếp", "Dừng hệ điều hành", "Tăng kích thước trang lên gấp đôi", "B", "Bit R=1 biểu thị trang mới được dùng, nên được đặt về 0 và giữ lại thêm một vòng."),
                new("cloze", "Hiện tượng hệ điều hành dành phần lớn thời gian để hoán đổi trang qua lại với đĩa thay vì thực thi lệnh gọi là {{c1::Thrashing}}.", null, null, null, null, "Thrashing", "Thrashing xảy ra khi tổng Working Set vượt quá số khung RAM vật lý.")
            }),
            // Chapter Exams & Final Exam for OS
            ("os:ch1", OsLesson12Id, "[Đề Chương 1] Tổng hợp Tiến trình, Luồng & Điều phối CPU", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Dữ liệu nào sau đây là RIÊNG TƯ của từng Thread trong cùng một Process?", "Vùng nhớ Heap cấp phát động", "Biến toàn cục Static", "Call Stack và tập thanh ghi CPU", "Bảng mô tả file đang mở (File Descriptors)", "C", "Mỗi Thread sở hữu Call Stack và thanh ghi riêng để theo dõi hàm đang gọi."),
                new("multiple_choice", "Thuật toán điều phối nào đạt Average Turnaround Time tối ưu nhất về mặt lý thuyết (với giả định biết trước burst time)?", "FCFS", "SJF / STCF", "Round Robin", "Random", "B", "Chạy công việc ngắn nhất trước (SJF/STCF) giảm thiểu thời gian chờ tích lũy cho mọi job phía sau."),
                new("cloze", "Trong vòng đời tiến trình, khi tiến trình đợi dữ liệu I/O từ ổ đĩa, trạng thái của nó chuyển từ Running sang {{c1::Blocked}}.", null, null, null, null, "Blocked", "Tiến trình chờ I/O chuyển sang Blocked/Waiting để nhường CPU cho tiến trình Ready."),
                new("essay", "So sánh Round Robin và MLFQ khi phục vụ đồng thời các tác vụ tương tác (UI) và các tác vụ tính toán nặng (Batch CPU-bound).", null, null, null, null,
                    "Round Robin chia đều quantum nên các tác vụ tương tác vẫn phải đợi vòng dài; MLFQ ưu tiên các tác vụ I/O/tương tác ở hàng đợi cao và hạ cấp tác vụ CPU-bound xuống hàng đợi thấp kèm Priority Boost.",
                    "Cần nêu được sự ưu tiên tương tác của MLFQ và tránh Starvation qua Priority Boost.",
                    new[] { "Phân tích hạn chế của Round Robin khi có nhiều tác vụ nặng", "Giải thích cách MLFQ hạ cấp tác vụ CPU-bound và giữ độ mượt cho tác vụ ngắn" })
            }),
            ("os:ch2", OsLesson22Id, "[Đề Chương 2] Tổng hợp Đồng bộ hóa & Phòng chống Deadlock", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Điểm khác nhau quan trọng nhất về quyền mở khóa của Mutex so với Binary Semaphore là gì?", "Mutex chỉ dùng cho JavaScript", "Mutex có khái niệm Ownership: chỉ chính luồng đã Lock mới được phép Unlock", "Semaphore không thể dùng trong đa luồng", "Mutex luôn chạy busy-wait", "B", "Mutex gắn chủ sở hữu luồng (Ownership), còn Semaphore hoạt động như tín hiệu đếm không phân biệt luồng gọi Release()."),
                new("multiple_choice", "Nếu Thread 1 giữ Lock(A) xin Lock(B) và Thread 2 giữ Lock(B) xin Lock(A), điều kiện Coffman nào đang trực tiếp gây treo hệ thống?", "Circular Wait", "Paging", "Spooling", "Fragmentation", "A", "Chu trình chờ đợi T1 -> B -> T2 -> A -> T1 chính là Circular Wait."),
                new("cloze", "Trong khối lệnh truy cập Mutex, luôn đặt lệnh nhả khóa trong khối {{c1::finally}} để tránh rò rỉ khóa khi gặp ngoại lệ.", null, null, null, null, "finally", "Khối finally bảo đảm Mutex luôn được mở khóa dù có exception xảy ra.")
            }),
            ("os:ch3", SeedDataInitializer.LessonPageReplacementId, "[Đề Chương 3] Tổng hợp Bộ nhớ ảo, TLB & Thay thế Trang", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Vì sao Linux x86-64 dùng bảng trang đa cấp 4 tầng (PGD -> PUD -> PMD -> PTE) thay vì 1 bảng trang phẳng?", "Để tăng kích thước trang lên 4GB", "Để tiết kiệm RAM qua cấp phát thưa cho không gian địa chỉ 64-bit khổng lồ", "Để loại bỏ hoàn toàn TLB", "Để biến RAM thành ổ cứng", "B", "Bảng trang phẳng 64-bit cần hàng Terabyte RAM; bảng trang đa cấp chỉ cấp phát nhánh đang dùng."),
                new("multiple_choice", "Với 4 khung trang RAM và chuỗi truy cập [1, 2, 3, 4, 1, 2, 5, 1, 2, 3, 4, 5], thuật toán nào cho phép số Page Fault nhỏ nhất?", "Optimal", "FIFO", "Random", "LIFO", "A", "Thuật toán Optimal luôn đạt giới hạn dưới lý thuyết về số Page Fault."),
                new("cloze", "Hai nguyên lý cục bộ giúp TLB đạt Hit Ratio trên 98% là Temporal Locality và {{c1::Spatial}} Locality.", null, null, null, null, "Spatial", "Spatial Locality (cục bộ không gian) là xu hướng truy cập các ô nhớ nằm gần nhau trong cùng một trang.")
            }),
            ("os:final", SeedDataInitializer.LessonPageReplacementId, "[Đề Tổng Hợp Cuối Khóa] Hệ điều hành: Tiến trình, Đồng bộ hóa & Bộ nhớ ảo", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Một CPU chạy 2 tiến trình chuyển ngữ cảnh mỗi 10ms với chi phí Context Switch = 0.5ms. Tỷ lệ Overhead xấp xỉ bao nhiêu?", "~4.76%", "~20%", "~50%", "~0.5%", "A", "0.5 / (10 + 0.5) = 0.5 / 10.5 ≈ 4.76%."),
                new("multiple_choice", "Giải pháp nào triệt tiêu hoàn toàn Race Condition khi 20 người dùng cùng cập nhật một bản ghi trong Studxy?", "Bỏ kiểm tra phiên bản", "Kết hợp Lock tuần tự (SemaphoreSlim) và Optimistic Concurrency Control (baseVersion)", "Dùng biến toàn cục không khóa", "Tắt WAL của SQLite", "B", "Serialized locking + OCC bảo đảm toàn vẹn dữ liệu và phát hiện ghi đè xung đột."),
                new("multiple_choice", "Giả sử t_TLB = 2ns, t_RAM = 100ns, Hit Ratio = 95%, bảng trang 1 cấp trong RAM. Thời gian truy cập hiệu dụng (EAT) là bao nhiêu?", "107 ns", "102 ns", "112 ns", "202 ns", "A", "0.95 * (2 + 100) + 0.05 * (2 + 200) = 96.9 + 10.1 = 107 ns."),
                new("cloze", "Cấu trúc lưu trạng thái thanh ghi, PID và con trỏ lệnh của một tiến trình trong nhân OS viết tắt là {{c1::PCB}}.", null, null, null, null, "PCB", "PCB = Process Control Block."),
                new("essay", "Thiết kế kiến trúc tránh Deadlock và Thrashing cho một dịch vụ backend xử lý đồng thời nhiều tác vụ nặng trên RAM giới hạn.", null, null, null, null,
                    "Áp dụng Lock Ordering và TryLock Timeout để phá Circular Wait/Hold-and-Wait; giới hạn độ đồng thời bằng SemaphoreSlim(k) và kiểm soát Working Set/Page Cache để tránh quá tải RAM gây Thrashing.",
                    "Đáp án cần kết hợp được cả cơ chế đồng bộ chống Deadlock lẫn quản lý bộ nhớ/Working Set.",
                    new[] { "Đề xuất Lock Ordering hoặc Timeout chống Deadlock", "Giới hạn concurrency bằng Semaphore/Worker pool", "Kiểm soát Working Set và tránh cấp phát bộ nhớ vượt RAM vật lý" })
            })
        };

        await UpsertExamQuizzesAsync(db, specs, now);
    }

    /// <summary>
    /// Seeds comprehensive Chapter Exams ([Đề Chương 1..3]) and Final Exams ([Đề Tổng Hợp Cuối Khóa])
    /// across all 6 interactive courses so every course has rich multi-tier assessments.
    /// </summary>
    public static async Task EnsureComprehensiveCourseExamsSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var specs = new List<(string Key, Guid LessonId, string Title, ExamQuestionSpec[] Questions)>
        {
            // 1. ENGLISH COMMUNICATION (EnglishCourseId)
            ("exam:en:ch1", SeedDataInitializer.EnglishLesson12Id, "[Đề Chương 1] Tiếng Anh Giao Tiếp Sinh Hoạt & Phản Xạ Cơ Bản", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Chọn câu hỏi lịch sự và tự nhiên nhất khi hỏi xem ghế bên cạnh tại hội thảo có trống không:", "Sit here ok?", "Excuse me, is this seat taken?", "You sit here?", "This seat is empty?", "B", "\"Excuse me, is this seat taken?\" là cách mở lời lịch sự chuẩn bản xứ."),
                new("multiple_choice", "Sau cụm \"I look forward to...\" ta sử dụng dạng từ nào?", "Động từ nguyên mẫu có to", "Danh từ hoặc V-ing", "Động từ quá khứ V2", "Tính từ sở hữu", "B", "Trong \"look forward to\", 'to' là giới từ nên đi kèm V-ing hoặc Danh từ."),
                new("cloze", "Điền giới từ chỉ đường: \"Go straight ahead, then {{c1::turn}} left at the traffic lights.\"", null, null, null, null, "turn", "Cụm động từ chỉ hướng rẽ: turn left / turn right."),
                new("essay", "Viết một đoạn giới thiệu bản thân 3 bước (Hook - Background - Goal) bằng tiếng Anh (3-4 câu) trong bối cảnh gặp gỡ đối tác.", null, null, null, null,
                    "Hi there! Is this seat taken? I'm Minh, a software engineer based in Hanoi focusing on backend systems. I'm really looking forward to exchanging insights on scalable architectures with you today.",
                    "Cần có câu mở lời thân thiện (Hook), nghề nghiệp/nơi làm việc (Background) và mục tiêu kết nối (Goal).",
                    new[] { "Có Hook mở lời tự nhiên", "Nêu Background với mạo từ a/an đúng chuẩn", "Nêu Goal rõ ràng" })
            }),
            ("exam:en:ch2", SeedDataInitializer.EnglishLesson22Id, "[Đề Chương 2] Tiếng Anh Công Sở: Email, Đặt Lịch & Họp Trực Tuyến", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Cụm từ chuyên nghiệp nhất để bắt đầu khi đưa ra ý kiến trong cuộc họp trực tuyến là gì?", "Listen to me now!", "From my perspective, I believe we should...", "You are wrong!", "I don't care about that.", "B", "\"From my perspective...\" thể hiện sự tôn trọng và chuyên nghiệp."),
                new("multiple_choice", "Khi đính kèm tệp báo cáo trong email, câu viết chuẩn là gì?", "I send you file.", "Please find the quarterly report attached.", "Look at file below.", "File is here.", "B", "\"Please find ... attached\" là mẫu câu chuẩn mực trong email công việc."),
                new("cloze", "Điền từ nối xác nhận lịch họp: \"Does Tuesday at 2 PM {{c1::work}} for you?\"", null, null, null, null, "work", "\"Does [time] work for you?\" nghĩa là khung giờ đó có thuận tiện với bạn không.")
            }),
            ("exam:en:ch3", SeedDataInitializer.EnglishLesson32Id, "[Đề Chương 3] Thuyết Trình, Đàm Phán & Phản Biện Chuyên Nghiệp", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Cấu trúc bài thuyết trình tinh gọn 3 phần chuẩn quốc tế gồm những phần nào?", "Intro - Story - Joke", "Opening (Hook) - Body (3 Key Points) - Conclusion (Call to Action)", "Title - Table - End", "Greeting - Reading - Goodbye", "B", "Opening thu hút, Body mạch lạc tối đa 3 luận điểm và Conclusion kêu gọi hành động."),
                new("multiple_choice", "Cách phản biện khéo léo (Disagreeing Politely) khi không đồng tình với đề xuất giá của đối tác là gì?", "That is a terrible idea!", "I see your point, however, from a long-term budget perspective...", "No way!", "Forget it!", "B", "Thừa nhận góc nhìn đối phương trước (\"I see your point, however...\") giúp giảm sự phòng thủ."),
                new("cloze", "Khi thương lượng điều kiện đôi bên cùng có lợi, ta gọi đó là giải pháp {{c1::win-win}}.", null, null, null, null, "win-win", "Win-win solution = giải pháp đôi bên cùng thắng.")
            }),
            ("exam:en:final", SeedDataInitializer.EnglishLesson32Id, "[Đề Tổng Hợp Cuối Khóa] Giao Tiếp Tiếng Anh Thực Chiến A2 → B1", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Để phát âm chuẩn âm vô thanh /θ/ trong từ 'think', vị trí lưỡi đúng là gì?", "Ép lưỡi vào vòm họng trên", "Đặt nhẹ đầu lưỡi giữa hai hàm răng và thổi hơi nhẹ không rung cổ họng", "Uốn cong lưỡi vào trong", "Đọc giống chữ 't' tiếng Việt", "B", "Âm /θ/ là âm liên răng vô thanh, không rung dây thanh quản."),
                new("multiple_choice", "Câu nào dùng đúng mạo từ khi giới thiệu nghề nghiệp?", "I am software engineer.", "I am a software engineer based in Ho Chi Minh City.", "I am the software engineer.", "I work a software engineer.", "B", "Danh từ đếm được số ít chỉ nghề nghiệp luôn cần mạo từ a/an."),
                new("multiple_choice", "Trong cuộc họp online bị mất kết nối nhẹ, câu nói lịch sự để xin nhắc lại ý cuối là gì?", "Speak louder!", "Sorry, you broke up for a second. Could you please repeat your last point?", "I can't hear anything!", "Say again now!", "B", "\"You broke up for a second\" là cách diễn đạt tự nhiên khi mạng chập chờn."),
                new("cloze", "Hoàn chỉnh câu hẹn gặp lại: \"I look forward to {{c1::meeting}} you next week.\"", null, null, null, null, "meeting", "Sau look forward to dùng danh động từ V-ing (meeting)."),
                new("essay", "Viết email ngắn (4-5 câu) đề nghị dời lịch họp sang chiều thứ Năm vì có sự cố hệ thống khẩn cấp.", null, null, null, null,
                    "Dear Alex, I am writing to request a reschedule for our meeting tomorrow morning due to an unexpected production issue. Could we move our discussion to Thursday afternoon at 3 PM instead? Please let me know if that time works for you. Best regards, Minh.",
                    "Email cần đủ lời chào, lý do lịch sự, đề xuất thời gian mới rõ ràng và lời kết chuyên nghiệp.",
                    new[] { "Nêu rõ lý do dời lịch lịch sự", "Đưa ra khung giờ thay thế cụ thể", "Dùng mẫu câu xác nhận lịch chuyên nghiệp" })
            }),

            // 2. CODING JAVASCRIPT (CodingCourseId)
            ("exam:code:ch1", SeedDataInitializer.CodingLesson12Id, "[Đề Chương 1] JavaScript Mảng (Map/Filter/Reduce) & Two Pointers", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Kết quả của `[1, 2, 3, 4].filter(x => x % 2 === 0).map(x => x * 10)` là gì?", "[10, 30]", "[20, 40]", "[2, 4]", "[10, 20, 30, 40]", "B", "filter giữ số chẵn [2, 4], map nhân 10 thành [20, 40]."),
                new("multiple_choice", "Điều kiện tiên quyết để áp dụng kỹ thuật Two Pointers (Left/Right hội tụ) tìm cặp có tổng bằng Target trong $O(N)$ là gì?", "Mảng phải có độ dài chẵn", "Mảng phải được sắp xếp tăng dần (Sorted)", "Mảng chỉ chứa số dương", "Không có điều kiện nào", "B", "Nếu mảng chưa sắp xếp, việc tăng left hoặc giảm right không bảo đảm đơn điệu."),
                new("cloze", "Phương thức {{c1::reduce}} dùng để gộp toàn bộ phần tử của mảng thành một giá trị duy nhất (như tổng, object thống kê).", null, null, null, null, "reduce", "Array.prototype.reduce nhận callback (acc, item) và giá trị khởi tạo.")
            }),
            ("exam:code:ch2", SeedDataInitializer.CodingLesson22Id, "[Đề Chương 2] Tối ưu Hiệu năng JS: Debounce, Throttle & Memoization", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Kỹ thuật nào phù hợp nhất cho ô tìm kiếm (Search Input) để chỉ gọi API sau khi người dùng NGỪNG gõ 300ms?", "Throttle", "Debounce", "Infinite Loop", "Deep Freeze", "B", "Debounce hoãn thực thi đến khi chuỗi sự kiện dừng hẳn trong khoảng delay."),
                new("multiple_choice", "Khi nào hàm có thể áp dụng an toàn kỹ thuật Memoization?", "Khi hàm là Pure Function (cùng đầu vào luôn trả cùng đầu ra, không side effect)", "Khi hàm phụ thuộc thời gian thực Date.now()", "Khi hàm sinh số ngẫu nhiên Math.random()", "Khi hàm ghi trực tiếp vào DOM", "A", "Memoization chỉ đúng đắn với Pure Function."),
                new("cloze", "Trong Debounce, trước khi đặt bộ đếm thời gian mới bằng `setTimeout`, ta bắt buộc gọi {{c1::clearTimeout}} để hủy timer cũ.", null, null, null, null, "clearTimeout", "clearTimeout(timerId) đặt lại đồng hồ đếm ngược mỗi khi sự kiện mới phát sinh.")
            }),
            ("exam:code:ch3", SeedDataInitializer.CodingLesson32Id, "[Đề Chương 3] Bất đồng bộ Async/Await, Immutability & Error Handling", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Để chạy song song 3 API call độc lập và chờ tất cả hoàn tất nhanh nhất, ta dùng phương thức nào?", "Chờ tuần tự 3 lệnh await nối tiếp", "Promise.all([...])", "setTimeout lồng nhau", "try/catch trống", "B", "Promise.all kích hoạt đồng thời các Promise thay vì nối đuôi tuần tự."),
                new("multiple_choice", "Tại sao `{ ...user }` (Spread Operator) không sao chép sâu (Deep Clone) các object lồng nhau bên trong?", "Vì Spread Operator chỉ thực hiện Shallow Copy ở cấp đầu tiên", "Vì JS không hỗ trợ copy object", "Vì Spread Operator chỉ dùng cho mảng", "Vì nó xóa thuộc tính con", "A", "Shallow copy giữ nguyên tham chiếu tới các object lồng bên trong."),
                new("cloze", "Hàm chuẩn tích hợp sẵn trong trình duyệt hiện đại để sao chép sâu (Deep Clone) đối tượng JS có tên là {{c1::structuredClone}}.", null, null, null, null, "structuredClone", "structuredClone(obj) xử lý được cả cấu trúc lồng nhau, Date, Map, Set.")
            }),
            ("exam:code:final", SeedDataInitializer.CodingLesson32Id, "[Đề Tổng Hợp Cuối Khóa] JavaScript & Tư duy Giải quyết Vấn đề", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Hàm nào sau đây KHÔNG làm biến đổi (mutate) mảng gốc ban đầu?", "arr.push(5)", "arr.sort()", "arr.map(x => x + 1)", "arr.splice(0, 1)", "C", "map() luôn trả về một mảng mới; push, sort và splice sửa trực tiếp trên mảng gốc."),
                new("multiple_choice", "Sự khác biệt cốt lõi giữa Debounce và Throttle là gì?", "Debounce chỉ chạy 1 lần sau khi sự kiện dừng hẳn; Throttle bảo đảm chạy đều đặn tối đa 1 lần mỗi khoảng T ms trong khi sự kiện đang diễn ra", "Cả hai giống hệt nhau", "Throttle chỉ chạy 1 lần duy nhất suốt vòng đời app", "Debounce chạy trước khi người dùng gõ", "A", "Search input dùng Debounce; Scroll/Resize listener dùng Throttle."),
                new("multiple_choice", "Lỗi nghiêm trọng khi viết `catch (err) {}` rỗng (Swallowing Errors) là gì?", "Tăng tốc độ ứng dụng", "Che giấu sự cố khiến việc debug thất bại và làm dữ liệu sai lệch âm thầm", "Tiết kiệm RAM", "Tự động sửa lỗi dữ liệu", "B", "Nuốt lỗi phá vỡ khả năng quan sát hệ thống và tạo ra lỗi dây chuyền khó truy vết."),
                new("cloze", "Để dừng ngay vòng lặp Retry khi gặp lỗi 400 Bad Request (lỗi không thể tự phục hồi bằng cách thử lại), ta kiểm tra status và ném lỗi trước khi vào nhánh {{c1::retry}}.", null, null, null, null, "retry", "Chỉ retry với lỗi tạm thời (network, 429, 503), không retry lỗi client 4xx."),
                new("essay", "Giải thích cách kết hợp Promise.all với cơ chế Retry có Exponential Backoff khi gọi một danh sách API bên ngoài có tỷ lệ rớt mạng nhẹ.", null, null, null, null,
                    "Bọc từng API call trong một hàm fetchWithRetry tăng thời gian chờ theo cấp số nhân (100ms, 200ms, 400ms) và chỉ retry với lỗi 5xx/mạng, sau đó truyền toàn bộ danh sách vào Promise.all (hoặc Promise.allSettled để không hỏng cả lô khi 1 item lỗi).",
                    "Cần nêu được Exponential Backoff, phân loại lỗi nên retry và Promise.all/allSettled.",
                    new[] { "Nêu công thức tăng thời gian chờ Exponential Backoff", "Phân biệt lỗi tạm thời (5xx/mạng) vs lỗi cố định (4xx)", "Đề cập Promise.all hoặc Promise.allSettled" })
            }),

            // 3. INVESTING (InvestingCourseId)
            ("exam:inv:ch1", SeedDataInitializer.InvestingLesson12Id, "[Đề Chương 1] Nền tảng Lãi Kép, DCA & Kế Hoạch FIRE", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Theo quy tắc 4% của Nghiên cứu Trinity (FIRE), nếu chi tiêu mỗi năm là 240 triệu đồng thì Quỹ Tự do Tài chính mục tiêu xấp xỉ bao nhiêu?", "2.4 tỷ đồng", "6 tỷ đồng (25 × 240 triệu)", "10 tỷ đồng", "1.2 tỷ đồng", "B", "Số FIRE = 25 × Chi tiêu hàng năm = 25 × 240 triệu = 6 tỷ đồng."),
                new("multiple_choice", "Giữa phương pháp trả nợ Snowball (nợ nhỏ trước) và Avalanche (lãi cao trước), phương pháp nào tiết kiệm tổng tiền lãi nhiều nhất?", "Snowball", "Avalanche", "Trả tối thiểu đều cho tất cả", "Cả hai như nhau", "B", "Avalanche dồn lực xử lý khoản nợ có lãi suất cao nhất trước nên giảm tổng tiền lãi phải trả tối đa."),
                new("cloze", "Chiến lược giải ngân một số tiền cố định vào một ngày cố định hàng tháng bất kể thị trường tăng hay giảm gọi là {{c1::DCA}}.", null, null, null, null, "DCA", "DCA = Dollar-Cost Averaging (Trung bình giá).")
            }),
            ("exam:inv:ch2", SeedDataInitializer.InvestingLesson22Id, "[Đề Chương 2] Danh Mục Đa Tài Sản & Tái Cân Bằng (Rebalancing)", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Mục đích chính của việc Tái cân bằng danh mục (Rebalancing) định kỳ mỗi năm một lần là gì?", "Bán sạch toàn bộ tài sản đang tăng", "Đưa tỷ trọng các nhóm tài sản về lại mức mục tiêu ban đầu (tự động chốt lời phần tăng nóng và mua thêm phần đang rẻ)", "Chuyển toàn bộ sang tiền mặt", "Tăng số lượng giao dịch lướt sóng", "B", "Rebalancing biến kỷ luật bán cao mua thấp thành quy trình cơ học."),
                new("multiple_choice", "Công thức tính Max Drawdown của một danh mục giảm từ đỉnh 500 triệu xuống đáy 350 triệu là bao nhiêu?", "-15%", "-30%", "-35%", "-50%", "B", "(350 - 500) / 500 = -150 / 500 = -30%."),
                new("cloze", "Khi lựa chọn giữa hai tài sản biến động ngược chiều nhau (tương quan âm), việc kết hợp chúng giúp giảm độ biến động chung nhờ hiệu ứng {{c1::đa dạng hóa}}.", null, null, null, null, "đa dạng hóa", "Đa dạng hóa (Diversification) giúp giảm rủi ro phi hệ thống của danh mục.")
            }),
            ("exam:inv:ch3", SeedDataInitializer.InvestingLesson32Id, "[Đề Chương 3] Định giá Doanh nghiệp & Tài chính Hành vi", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Theo Benjamin Graham, nguyên lý 'Biên an toàn' (Margin of Safety) yêu cầu điều gì khi mua cổ phiếu?", "Mua bất chấp mọi mức giá nếu công ty nổi tiếng", "Chỉ mua khi thị giá thấp hơn đáng kể (ví dụ 25–30%) so với giá trị nội tại ước tính", "Mua bằng toàn bộ đòn bẩy Margin", "Chỉ mua khi chỉ số P/E trên 50", "B", "Biên an toàn giúp bảo vệ vốn khỏi sai số định giá và biến cố thị trường."),
                new("multiple_choice", "Hành vi 'gồng lỗ không dám cắt vì lỡ bỏ quá nhiều tiền và thời gian vào mã cổ phiếu yếu kém' minh họa cho thiên kiến nào?", "Sunk Cost Fallacy (Ảo tưởng chi phí chìm)", "Confirmation Bias", "Diversification", "Compound Interest", "A", "Chi phí chìm đã mất trong quá khứ không nên chi phối quyết định phân bổ vốn cho tương lai."),
                new("cloze", "Chỉ số đo hiệu quả sinh lời trên vốn chủ sở hữu của doanh nghiệp viết tắt là {{c1::ROE}}.", null, null, null, null, "ROE", "ROE = Return on Equity = Lợi nhuận sau thuế / Vốn chủ sở hữu bình quân.")
            }),
            ("exam:inv:final", SeedDataInitializer.InvestingLesson32Id, "[Đề Tổng Hợp Cuối Khóa] Đầu Tư & Quản Trị Danh Mục Bền Vững", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Nếu lạm phát là 4.5%/năm và lãi suất tiền gửi là 6%/năm, lợi suất thực tế (Real Return) xấp xỉ là bao nhiêu?", "10.5%/năm", "1.5%/năm", "6%/năm", "-1.5%/năm", "B", "Real Return ≈ Nominal Return - Inflation = 6% - 4.5% = 1.5%/năm."),
                new("multiple_choice", "Trước khi giải ngân đầu tư chứng khoán dài hạn, bước nền tảng BẮT BUỘC phải chuẩn bị trước là gì?", "Vay Margin tối đa", "Hoàn thiện Quỹ dự phòng khẩn cấp 3–6 tháng chi phí thiết yếu và xóa nợ lãi cao", "Theo dõi bảng điện tử từng giây", "Mua theo tin đồn trên mạng xã hội", "B", "Quỹ dự phòng ngăn việc phải bán tháo cổ phiếu đang lỗ khi gặp biến cố cuộc sống."),
                new("multiple_choice", "Tỷ lệ Risk : Reward (R:R) tối thiểu 1:2 trong quản trị lệnh có ý nghĩa gì?", "Mất 2 đồng để kỳ vọng lời 1 đồng", "Chấp nhận rủi ro tối đa 1 phần nếu sai (Stop-loss) để đổi lấy mục tiêu lợi nhuận tối thiểu 2 phần nếu đúng", "Thắng 100% các lệnh", "Không cần đặt điểm cắt lỗ", "B", "Với R:R = 1:2, chỉ cần tỷ lệ thắng trên 34% đã bắt đầu có kỳ vọng dương."),
                new("cloze", "Thiên kiến khiến nhà đầu tư bám chặt vào mức giá mua cũ và thề 'chỉ bán khi hòa vốn' là hiệu ứng {{c1::mỏ neo}}.", null, null, null, null, "mỏ neo", "Anchoring Bias (hiệu ứng mỏ neo) khiến người đầu tư bỏ qua thông tin cơ bản mới."),
                new("essay", "Trình bày 4 trụ cột trong Nhật ký Quyết định Đầu tư và giải thích vì sao viết nhật ký trước khi bấm lệnh giúp chống FOMO.", null, null, null, null,
                    "4 trụ cột gồm: (1) Luận điểm đầu tư & Con hào kinh tế, (2) Định giá & Biên an toàn, (3) Kịch bản sai & Ngưỡng cắt lỗ/hành động, (4) Trạng thái cảm xúc lúc ra quyết định. Việc buộc phải viết ra giấy kích hoạt tư duy chậm logic, làm nguội cảm xúc FOMO bốc đồng.",
                    "Cần nêu đủ 4 trụ cột và vai trò kiểm soát cảm xúc.",
                    new[] { "Liệt kê đủ luận điểm, định giá, kịch bản sai và kỷ luật cảm xúc", "Giải thích cơ chế chuyển từ cảm xúc sang phân tích khách quan" })
            }),

            // 4. PSYCHOLOGY (PsychologyCourseId)
            ("exam:psy:ch1", SeedDataInitializer.PsychologyLesson12Id, "[Đề Chương 1] Hệ thống 1 & 2, Hiệu ứng Stroop & Thiên kiến Nhận thức", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Theo Daniel Kahneman, đặc điểm của Hệ thống 1 (System 1) là gì?", "Chậm chạp, tốn nhiều năng lượng và phân tích toán học phức tạp", "Nhanh, tự động, trực giác và dễ bị đánh lừa bởi thiên kiến/bối cảnh", "Chỉ hoạt động khi ngủ", "Hoàn toàn miễn nhiễm với cảm xúc", "B", "Hệ thống 1 xử lý phản xạ nhanh nhưng dễ mắc bẫy trực giác và hiệu ứng Stroop."),
                new("multiple_choice", "Khi bạn chỉ đi tìm các bài báo ủng hộ quan điểm sẵn có của mình và phớt lờ bằng chứng trái chiều, đó là thiên kiến nào?", "Confirmation Bias (Thiên kiến xác nhận)", "Loss Aversion", "Sunk Cost", "Chunking", "A", "Confirmation Bias khiến chúng ta lọc thông tin theo niềm tin có sẵn."),
                new("cloze", "Thí nghiệm yêu cầu đọc MÀU MỰC của chữ 'ĐỎ' nhưng được tô bằng mực XANH dùng để đo sự xung đột nhận thức gọi là hiệu ứng {{c1::Stroop}}.", null, null, null, null, "Stroop", "Stroop Effect minh họa Hệ thống 1 (đọc chữ tự động) xung đột với Hệ thống 2 (nhận diện màu có chủ đích).")
            }),
            ("exam:psy:ch2", SeedDataInitializer.PsychologyLesson22Id, "[Đề Chương 2] Trí nhớ Ngắn hạn, Chunking & Đường cong Ebbinghaus", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Kỹ thuật Chunking (Đóng gói thông tin) giúp vượt qua giới hạn trí nhớ làm việc (7±2 hoặc 4±1 đơn vị) bằng cách nào?", "Gom các mẩu thông tin rời rạc thành các khối có ý nghĩa logic", "Đọc lặp lại liên tục không cần hiểu", "Học thức đêm trước ngày thi", "Bỏ qua toàn bộ chi tiết", "A", "Chunking biến nhiều ký tự lẻ thành 1 khối nghĩa duy nhất để não giữ được nhiều dữ liệu hơn."),
                new("multiple_choice", "Phương pháp nào chống lại Đường cong quên lãng Ebbinghaus hiệu quả gấp nhiều lần so với đọc lại thụ động (Re-reading)?", "Tô sáng toàn bộ trang sách", "Active Recall (Tái hiện chủ động) kết hợp Spaced Repetition (FSRS)", "Chép nguyên văn tài liệu 5 lần", "Đọc lướt thật nhanh", "B", "Việc tự truy xuất ký ức (Active Recall) vào đúng thời điểm sắp quên củng cố khớp thần kinh mạnh nhất."),
                new("cloze", "Thuật toán lặp lại ngắt quãng hiện đại đo 2 biến trạng thái chính của mỗi thẻ nhớ là Độ khó (Difficulty) và Độ ổn định ({{c1::Stability}}).", null, null, null, null, "Stability", "FSRS dùng Stability (S) và Difficulty (D) để dự đoán xác suất nhớ lại Retrievability.")
            }),
            ("exam:psy:ch3", SeedDataInitializer.PsychologyLesson32Id, "[Đề Chương 3] Mô hình CBT ABC, Giao tiếp NVC & Ranh giới Cá nhân", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Trong mô hình ABC của CBT (Liệu pháp Hành vi Nhận thức), chữ 'B' (Beliefs) đại diện cho điều gì?", "Sự kiện kích hoạt bên ngoài", "Niềm tin / Suy nghĩ diễn giải tự động của bản thân về sự kiện đó", "Hệ quả cảm xúc và hành vi", "Người đối diện", "B", "Không phải sự kiện A trực tiếp gây ra cảm xúc C, mà chính suy nghĩ diễn giải B mới quyết định C."),
                new("multiple_choice", "Thứ tự 4 bước chuẩn của Giao tiếp Phi Bạo Lực (NVC - OFNR) là gì?", "Phán xét - Trách móc - Im lặng - Bỏ đi", "Quan sát khách quan (Observation) - Cảm xúc (Feeling) - Nhu cầu (Need) - Đề nghị cụ thể (Request)", "Ra lệnh - Đe dọa - Thuyết phục - Hứa hẹn", "Hỏi - Trả lời - Ghi chép - Chấm điểm", "B", "Mô hình OFNR giúp giao tiếp chân thành mà không kích hoạt cơ chế tự vệ của đối phương."),
                new("cloze", "Theo Kristin Neff, Tự trắc ẩn (Self-Compassion) gồm 3 trụ cột: Tử tế với bản thân, Chánh niệm và Tính {{c1::nhân loại chung}} (Common Humanity).", null, null, null, null, "nhân loại chung", "Nhận ra rằng sai sót và khó khăn là trải nghiệm chung của con người chứ không phải riêng mình cô độc.")
            }),
            ("exam:psy:final", SeedDataInitializer.PsychologyLesson32Id, "[Đề Tổng Hợp Cuối Khóa] Tâm Lý Học Ứng Dụng & Khoa Học Trí Nhớ", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Cụm từ nào sau đây là 'Quan sát khách quan' (không pha lẫn phán xét) theo chuẩn NVC?", "Cậu lúc nào cũng vô trách nhiệm!", "Hôm nay đã 22h tối nhưng nhóm vẫn chưa nhận được bản slide phần của cậu.", "Cậu thật ích kỷ và lười biếng.", "Cậu cố tình phá nhóm đúng không?", "B", "Nêu thời gian và sự kiện đo đếm được giúp cuộc trò chuyện tập trung vào giải quyết vấn đề."),
                new("multiple_choice", "Bẫy nhận thức 'Nếu bài thuyết trình này có 1 lỗi nhỏ thì tôi là kẻ thất bại toàn tập' thuộc dạng bóp méo nhận thức nào trong CBT?", "Tư duy trắng - đen (All-or-Nothing Thinking)", "Tư duy tích cực", "Đa dạng hóa", "Active Recall", "A", "Tư duy nhị phân cực đoan không chấp nhận mức độ trung gian và tiến bộ từng bước."),
                new("multiple_choice", "Kỹ thuật Feynman yêu cầu người học làm gì để phát hiện ngay lỗ hổng kiến thức?", "Học thuộc lòng định nghĩa sách giáo khoa", "Tự giải thích lại khái niệm bằng ngôn từ đơn giản như đang giảng cho một học sinh mới", "Chỉ làm trắc nghiệm 4 lựa chọn", "Đọc thầm trong đầu không viết ra", "B", "Khi giải thích đơn giản mà bị khựng lại, đó chính là chỗ bạn chưa thực sự hiểu bản chất."),
                new("cloze", "Kỹ thuật '{{c1::Dừng 5 giây}}' giúp làm dịu phản ứng xung đột của Hệ thống 1 để kích hoạt vùng vỏ não phân tích của Hệ thống 2.", null, null, null, null, "Dừng 5 giây", "Khoảng dừng ngắn giúp tách rời kích thích bên ngoài khỏi phản ứng bốc đồng."),
                new("essay", "Áp dụng mô hình ABC (CBT) và Giao tiếp NVC (4 bước) để xử lý tình huống đồng đội liên tục giao bài sát giờ gây căng thẳng cho bạn.", null, null, null, null,
                    "Theo CBT: Nhận diện suy nghĩ tự động cực đoan ('mình luôn bị lợi dụng') và tái cấu trúc thành suy nghĩ cân bằng ('cần thiết lập lại quy ước giờ nộp rõ ràng'). Theo NVC: 'Khi slide gửi lúc 22h trước hạn sáng mai (Quan sát), mình cảm thấy rất lo lắng (Cảm xúc) vì mình cần thời gian kiểm tra đồng bộ bài (Nhu cầu). Lần tới cậu có thể gửi dàn ý trước 19h tối hôm trước được không? (Đề nghị)'.",
                    "Bài làm cần tách bạch bước tái cấu trúc CBT và đủ 4 bước OFNR trong lời thoại NVC.",
                    new[] { "Phân tích A-B-C và suy nghĩ cân bằng", "Viết đủ 4 thành phần Quan sát - Cảm xúc - Nhu cầu - Đề nghị" })
            }),

            // 5. JAPANESE N5 (JapaneseCourseId)
            ("exam:ja:ch1", SeedDataInitializer.JapaneseLesson12Id, "[Đề Chương 1] Tiếng Nhật N5: Hiragana, Katakana & Chào hỏi", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Trợ từ viết là \"は\" khi đánh dấu chủ đề câu như \"わたしは ミンです\" được phát âm là gì?", "ha", "wa", "ba", "pa", "B", "Chữ は khi đóng vai trò trợ từ đánh dấu chủ đề luôn đọc là 'wa'."),
                new("multiple_choice", "Phân biệt cặp Katakana dễ nhầm: Chữ nào đọc là \"ン\" (n) có nét hất từ dưới lên?", "ソ (so)", "ン (n)", "シ (shi)", "ツ (tsu)", "B", "ン (n) và シ (shi) có nét chấm ngang hơn và nét dài hất từ dưới lên."),
                new("cloze", "Hoàn chỉnh câu tự giới thiệu lần đầu gặp mặt: \"{{c1::はじめまして}}、ミンです。どうぞよろしく。\"", null, null, null, null, "はじめまして", "はじめまして (Hajimemashite) dùng khi lần đầu gặp gỡ ai đó.")
            }),
            ("exam:ja:ch2", SeedDataInitializer.JapaneseLesson22Id, "[Đề Chương 2] Tiếng Nhật N5: Giờ giấc, Số đếm & Mua sắm", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Số 4 giờ và 9 giờ trong tiếng Nhật có biến âm bất quy tắc đọc là gì?", "よじ và くじ", "しじ và きゅうじ", "よんじ và きゅうじ", "よじ và はちじ", "A", "4 giờ = よじ (yoji), 9 giờ = くじ (kuji)."),
                new("multiple_choice", "Khi muốn hỏi 'Cái này bao nhiêu tiền?', câu chuẩn là gì?", "これは なんじですか。", "これは いくらですか。", "これは どこですか。", "これは だれですか。", "B", "いくら (ikura) dùng để hỏi giá tiền."),
                new("cloze", "Điền trợ từ đánh dấu mốc thời gian cụ thể: \"まいあさ ろくじ{{c1::に}} おきます。\"", null, null, null, null, "に", "Trợ từ に đi sau mốc thời gian có con số cụ thể như 6 giờ.")
            }),
            ("exam:ja:ch3", SeedDataInitializer.JapaneseLesson32Id, "[Đề Chương 3] Tiếng Nhật N5: Phương hướng, Di chuyển & Đề nghị", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Để nói 'Tôi đi đến trường bằng tàu điện', chọn trợ từ đúng: \"わたしは でんしゃ___ がっこうへ いきます。\"", "に", "で", "を", "が", "B", "Trợ từ で chỉ phương tiện giao thông (ngoại trừ đi bộ dùng あるいて)."),
                new("multiple_choice", "Mẫu câu nào dùng để mời rủ lịch sự 'Cùng uống cà phê nhé?'", "コーヒーを のみませんか。", "コーヒーを のみました。", "コーヒーを のみません。", "コーヒーを ください。", "A", "Động từ thể 〜ませんか là lời mời lịch sự nhã nhặn."),
                new("cloze", "Với đồ vật vô tri và công trình địa điểm tĩnh (nhà ga, ngân hàng), ta dùng động từ tồn tại {{c1::あります}}.", null, null, null, null, "あります", "あります dùng cho vật vô tri; います dùng cho người và động vật.")
            }),

            // 6. CHINESE HSK1 (ChineseCourseId)
            ("exam:zh:ch1", SeedDataInitializer.ChineseLesson12Id, "[Đề Chương 1] Tiếng Trung HSK1: Pinyin, Thanh điệu & Chào hỏi", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Nhóm thanh mẫu nào sau đây đều là ÂM BẬT HƠI MẠNH trong tiếng Trung?", "b, d, g", "p, t, k", "m, n, l", "j, z, zh", "B", "Các cặp không bật hơi - bật hơi gồm: b/p, d/t, g/k, j/q, z/c, zh/ch."),
                new("multiple_choice", "Quy tắc biến điệu khi hai chữ mang thanh 3 đứng cạnh nhau (ví dụ: 你好 nǐ hǎo) đọc ra sao?", "Cả hai chữ đọc thành thanh 4", "Chữ thứ nhất biến thành thanh 2 (ní hǎo), chữ thứ hai giữ nguyên thanh 3", "Chữ thứ hai đọc thanh nhẹ", "Giữ nguyên cả hai thành thanh 1", "B", "Quy tắc biến điệu thanh 3: 3 + 3 → 2 + 3 (nǐ hǎo đọc thành ní hǎo)."),
                new("cloze", "Khi đối phương nói '谢谢 (Cảm ơn)', câu đáp lịch sự chuẩn HSK1 là '不{{c1::客气}}' (Đừng khách sáo).", null, null, null, null, "客气", "不客气 (bú kèqi) = Không có gì / Đừng khách sáo.")
            }),
            ("exam:zh:ch2", SeedDataInitializer.ChineseLesson22Id, "[Đề Chương 2] Tiếng Trung HSK1: Thời gian, Gia đình & Mua sắm", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Phủ định của động từ '有' (yǒu - có) BẮT BUỘC phải dùng từ nào?", "不有 (bùyǒu)", "没有 (méiyǒu)", "别有 (biéyǒu)", "非有 (fēiyǒu)", "B", "Động từ 有 chỉ có một dạng phủ định duy nhất là 没有."),
                new("multiple_choice", "Lượng từ chuyên dùng để đếm số lượng thành viên trong gia đình (ví dụ: Nhà tôi có 4 người) là gì?", "本 (běn)", "口 (kǒu)", "碗 (wǎn)", "张 (zhāng)", "B", "Nhà có 4 nhân khẩu: 我家有四口人."),
                new("cloze", "Khi nói '2 giờ' (2:00), bắt buộc phải dùng chữ số {{c1::两}} (liǎng) chứ không dùng 二.", null, null, null, null, "两", "两点 (liǎng diǎn) là cách nói 2 giờ chuẩn xác.")
            }),
            ("exam:zh:ch3", SeedDataInitializer.ChineseLesson32Id, "[Đề Chương 3] Tiếng Trung HSK1: Ăn uống, Phương hướng & Lịch trình", new ExamQuestionSpec[]
            {
                new("multiple_choice", "Câu hỏi lựa chọn 'Bạn muốn uống trà hay uống cà phê?' đúng ngữ pháp là gì?", "你想喝茶还是喝咖啡吗？", "你想喝茶还是喝咖啡？", "你想喝茶或者喝咖啡吗？", "你喝茶不喝咖啡吗？", "B", "Câu hỏi dùng 还是 (hay là) tuyệt đối KHÔNG thêm trợ từ 吗 ở cuối câu."),
                new("multiple_choice", "Trật tự câu liên động 'Tôi đi xe buýt đến trường học' đúng chuẩn là gì?", "我坐公共汽车去学校。", "我去学校坐公共汽车。", "公共汽车我坐去学校。", "学校我去坐公共汽车。", "A", "Chủ ngữ + 坐 + Phương tiện + 去 + Địa điểm."),
                new("cloze", "Mẫu câu hỏi đường 'Đi đến bệnh viện đi như thế nào?': 去医院{{c1::怎么}}走？", null, null, null, null, "怎么", "怎么走 (zěnme zǒu) = đi như thế nào.")
            })
        };

        await UpsertExamQuizzesAsync(db, specs, now);
    }

    private static async Task UpsertExamQuizzesAsync(
        SwmsDbContext db,
        List<(string Key, Guid LessonId, string Title, ExamQuestionSpec[] Questions)> specs,
        DateTimeOffset now)
    {
        var allTargetLessonIds = specs.Select(s => s.LessonId).Distinct().ToList();
        var existingLessonIds = await db.Lessons
            .AsNoTracking()
            .Where(l => allTargetLessonIds.Contains(l.Id))
            .Select(l => l.Id)
            .ToHashSetAsync();

        foreach (var (key, lessonId, title, questions) in specs)
        {
            if (!existingLessonIds.Contains(lessonId)) continue;

            var quizId = CreateDeterministicGuid($"swms:exam-quiz:{key}");
            var existing = await db.PracticeQuizzes.FirstOrDefaultAsync(q =>
                q.Id == quizId ||
                (q.UserId == SeedDataInitializer.DefaultUserId && q.LessonId == lessonId && q.Title == title));

            if (existing == null)
            {
                var quiz = new PracticeQuiz
                {
                    Id = quizId,
                    UserId = SeedDataInitializer.DefaultUserId,
                    Title = title,
                    LessonId = lessonId,
                    TotalItems = questions.Length,
                    CorrectItems = 0,
                    Status = "in_progress",
                    StartedAt = now
                };
                db.PracticeQuizzes.Add(quiz);

                for (int i = 0; i < questions.Length; i++)
                {
                    var q = questions[i];
                    db.PracticeQuizItems.Add(BuildExamItem(quizId, key, i, q, now));
                }
            }
            else
            {
                var existingCount = await db.PracticeQuizItems.CountAsync(it => it.QuizId == existing.Id);
                if (existingCount == 0)
                {
                    for (int i = 0; i < questions.Length; i++)
                    {
                        db.PracticeQuizItems.Add(BuildExamItem(existing.Id, key, i, questions[i], now));
                    }
                    existing.TotalItems = questions.Length;
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static PracticeQuizItem BuildExamItem(Guid quizId, string key, int idx, ExamQuestionSpec q, DateTimeOffset now)
    {
        string promptJson = q.RubricCriteria is { Length: > 0 }
            ? JsonSerializer.Serialize(new { question = q.Question, rubricCriteria = q.RubricCriteria }, SafeJsonOptions)
            : JsonSerializer.Serialize(new { question = q.Question }, SafeJsonOptions);

        string? choicesJson = null;
        if (q.ItemType == "multiple_choice")
        {
            choicesJson = JsonSerializer.Serialize(new[]
            {
                new { key = "A", text = q.OptionA ?? string.Empty },
                new { key = "B", text = q.OptionB ?? string.Empty },
                new { key = "C", text = q.OptionC ?? string.Empty },
                new { key = "D", text = q.OptionD ?? string.Empty }
            }, SafeJsonOptions);
        }

        return new PracticeQuizItem
        {
            Id = CreateDeterministicGuid($"swms:exam-item:{key}:{idx + 1}"),
            QuizId = quizId,
            ItemType = q.ItemType,
            PromptAstJson = promptJson,
            ChoicesJson = choicesJson,
            ClozeSolution = q.Solution,
            AiRubricFeedbackJson = JsonSerializer.Serialize(new { summary = q.Explanation }, SafeJsonOptions),
            CreatedAt = now.AddSeconds(idx)
        };
    }

    private static Guid CreateDeterministicGuid(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }
}
