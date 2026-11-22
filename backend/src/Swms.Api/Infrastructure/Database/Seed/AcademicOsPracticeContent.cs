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
    public const string UpgradeMarker = "<!-- studxy-os-v3 -->";

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

            if (course.Description != null && !course.Description.Contains("Tiến trình", StringComparison.Ordinal))
            {
                course.Description = "Khóa học cốt lõi về Hệ điều hành: Tiến trình & Luồng, Điều phối CPU, Đồng bộ hóa (Mutex/Semaphore/Deadlock), Quản lý bộ nhớ ảo, TLB và Thuật toán thay thế trang.";
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
            else if (existing.CurrentVersion == 1 && !(existing.ContentMarkdown ?? string.Empty).Contains(UpgradeMarker, StringComparison.Ordinal))
            {
                existing.ContentMarkdown = meta.Markdown;
                existing.PlainText = meta.Markdown;
                existing.EstimatedMinutes = meta.EstimatedMinutes;
                existing.UpdatedAt = now;
            }
        }

        // 3.2 (LessonPagingId): STRICT INVARIANT — NEVER modify ContentAstJson, PlainText, or CurrentVersion (= 2).
        // Only fill ContentMarkdown if empty so Markdown reader renders rich v3 theory alongside AST/Annotations.
        var lesson32 = await db.Lessons.FirstOrDefaultAsync(l => l.Id == SeedDataInitializer.LessonPagingId);
        if (lesson32 != null && string.IsNullOrWhiteSpace(lesson32.ContentMarkdown))
        {
            lesson32.ContentMarkdown = GetLesson32Markdown();
            lesson32.EstimatedMinutes = Math.Max(lesson32.EstimatedMinutes, 25);
        }

        // 3.3 (LessonPageReplacementId): Safe upgrade ONLY if CurrentVersion == 1 and marker missing.
        var lesson33 = await db.Lessons.FirstOrDefaultAsync(l => l.Id == SeedDataInitializer.LessonPageReplacementId);
        if (lesson33 != null && lesson33.CurrentVersion == 1 && !(lesson33.ContentMarkdown ?? string.Empty).Contains(UpgradeMarker, StringComparison.Ordinal))
        {
            lesson33.ContentMarkdown = GetLesson33Markdown();
            lesson33.EstimatedMinutes = 25;
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
            25,
            @"# Bài 1.1: Tiến trình (Process), Luồng (Thread) & Chuyển ngữ cảnh

> [!TIP]
> Nếu bạn mới làm quen với Hệ điều hành: hãy tưởng tượng **Program** là cuốn công thức nấu ăn nằm trên ổ đĩa, còn **Process** là một căn bếp đang thực sự mở lửa nấu theo công thức đó, và **Threads** là các đầu bếp làm việc cùng nhau trong cùng một căn bếp!

---

## 🌱 Phần 0 — Nhập môn từ trực giác
- **Chương trình (Program)** là tệp nhị phân tĩnh (`.exe`, ELF) nằm yên trên SSD.
- **Tiến trình (Process)** là một chương trình đang chạy, được hệ điều hành cấp cho **Không gian địa chỉ ảo riêng biệt** (Code, Data, Heap, Stack) và một cấu trúc quản lý gọi là **PCB (Process Control Block)**.
- **Luồng (Thread)** là đơn vị thực thi nhỏ nhất bên trong một tiến trình. Nhiều Thread trong cùng Process **dùng chung Heap & Biến toàn cục**, nhưng mỗi Thread sở hữu **Stack riêng và Bộ đếm lệnh (PC) + Thanh ghi (Registers) riêng**.

---

## 1. Kiến trúc Bộ nhớ của Process & Multi-Threading

| Thành phần | Process (Tiến trình) | Thread (Luồng) |
|---|---|---|
| **Không gian địa chỉ** | Cô lập hoàn toàn giữa các Process | Dùng chung không gian địa chỉ (Code, Data, Heap) của Process cha |
| **Stack & Registers** | Mỗi Process có ít nhất 1 Thread chính | Mỗi Thread có Call Stack và CPU Registers riêng |
| **Chi phí Context Switch** | Đắt (phải đổi bảng trang CR3, xả/nạp TLB) | Rẻ hơn nhiều (giữ nguyên bảng trang, chỉ đổi thanh ghi & Stack Pointer) |
| **Lỗi Crash** | 1 Process crash không làm sập Process khác | 1 Thread lỗi segmentation fault làm sập cả Process |

---

## 2. Vòng đời Tiến trình & Chuyển ngữ cảnh (Context Switch)
Một tiến trình luân chuyển qua 5 trạng thái kinh điển: `New → Ready → Running → Waiting/Blocked → Terminated`.
Khi hệ điều hành chuyển CPU từ Process A sang Process B:
1. Lưu toàn bộ trạng thái CPU hiện tại (Registers, Program Counter, Stack Pointer) vào `PCB_A`.
2. Cập nhật trạng thái `PCB_A` sang `Ready` hoặc `Blocked`.
3. Nạp trạng thái từ `PCB_B` lên thanh ghi CPU và chuyển thanh ghi bảng trang (`CR3` trên x86-64).
4. Tiếp tục thực thi Process B. Quá trình này tốn vài micro-giây và tạo ra **Overhead (chi phí ẩn)** nếu chuyển đổi quá dày đặc!

---

## 3. Checklist Ghi nhớ Thực chiến
- [ ] Phân biệt được dữ liệu **dùng chung (Heap/Global)** và **riêng tư (Stack/Registers)** giữa các Thread.
- [ ] Hiểu vì sao chuyển ngữ cảnh giữa 2 Process tốn kém hơn chuyển giữa 2 Thread cùng Process (liên quan tới TLB Shootdown/Flush).

<!-- studxy-os-v3 -->",
            "Tiến trình sở hữu không gian địa chỉ cô lập và PCB; các luồng trong cùng tiến trình chia sẻ Heap nhưng giữ Stack và Registers riêng.",
            "Context Switch giữa các tiến trình có chi phí cao do phải chuyển thanh ghi bảng trang CR3 và làm mất nóng bộ đệm TLB."
        ),

        new OsLessonMeta(
            OsChapter1Id,
            OsLesson12Id,
            "Bài 1.2: Thuật toán Điều phối CPU (FCFS, SJF, Round Robin & MLFQ)",
            "thuat-toan-dieu-phoi-cpu-fcfs-sjf-rr-mlfq",
            2,
            25,
            @"# Bài 1.2: Thuật toán Điều phối CPU (FCFS, SJF, Round Robin & MLFQ)

> [!TIP]
> Tại sao máy tính có 4 nhân CPU nhưng vẫn mở được 50 tab trình duyệt, nghe nhạc và chạy VS Code mượt mà cùng lúc? Bí quyết nằm ở bộ **CPU Scheduler** chia lát cắt thời gian (Time Slice) siêu nhỏ!

---

## 🌱 Phần 0 — Hai chỉ số đánh giá bộ điều phối
1. **Turnaround Time (Thời gian hoàn thành):** $T_{\text{turnaround}} = T_{\text{completion}} - T_{\text{arrival}}$. Đo lường tổng thời gian tiến trình chờ từ lúc xuất hiện đến lúc chạy xong.
2. **Response Time (Thời gian phản hồi):** $T_{\text{response}} = T_{\text{firstRun}} - T_{\text{arrival}}$. Đo lường độ trễ từ lúc người dùng bấm phím đến khi tiến trình được CPU chạm vào lần đầu.

---

## 1. Bốn thuật toán Điều phối CPU kinh điển

| Thuật toán | Cơ chế cốt lõi | Ưu điểm | Nhược điểm |
|---|---|---|---|
| **FCFS (First-Come, First-Served)** | Ai đến trước chạy trước (Non-preemptive) | Đơn giản, không gây đói tài nguyên (Starvation) | **Convoy Effect**: 1 tác vụ nặng chặn đứng hàng loạt tác vụ ngắn phía sau |
| **SJF / STCF (Shortest Job First)** | Ưu tiên tác vụ ngắn nhất chạy trước | Tối ưu nhất về Turnaround Time trung bình | Dễ gây **Starvation** cho tác vụ dài; không biết trước thời gian chạy |
| **Round Robin (RR)** | Xoay vòng mỗi tiến trình 1 lát cắt `quantum (q)` | Công bằng, tối ưu **Response Time** cho ứng dụng tương tác | `q` quá nhỏ → Overhead Context Switch cao; `q` quá lớn → thoái hóa thành FCFS |
| **MLFQ (Multi-Level Feedback Queue)** | Nhiều hàng đợi ưu tiên; tiến trình dùng hết quantum bị hạ cấp, tác vụ I/O được giữ ưu tiên | Cân bằng cả Response Time lẫn Turnaround Time; không cần biết trước burst time | Cần cơ chế **Priority Boost** định kỳ để tránh Starvation |

---

## 2. Công thức tính nhanh Round Robin
Với $N$ tiến trình trong hàng đợi Ready và độ dài lát cắt $q$, thời gian chờ tối đa cho lượt chạy đầu tiên xấp xỉ $(N - 1) \times q$. Nếu $q = 10\text{ ms}$ và Context Switch tốn $0.1\text{ ms}$, chi phí chuyển ngữ cảnh chỉ chiếm $\approx 1\%$.

<!-- studxy-os-v3 -->",
            "FCFS dễ gặp hiệu ứng đoàn tàu (Convoy Effect); SJF tối ưu thời gian hoàn thành nhưng có thể gây đói CPU cho tác vụ dài.",
            "Round Robin tối ưu thời gian phản hồi theo lát cắt quantum, còn MLFQ tự động học hành vi CPU-bound và I/O-bound."
        ),

        new OsLessonMeta(
            OsChapter2Id,
            OsLesson21Id,
            "Bài 2.1: Race Condition, Critical Section, Mutex & Semaphore",
            "race-condition-critical-section-mutex-semaphore",
            1,
            25,
            @"# Bài 2.1: Race Condition, Critical Section, Mutex & Semaphore

> [!TIP]
> Lỗi khó tìm nhất trong hệ thống backend chịu tải cao là **Race Condition**: hai luồng cùng đọc số dư 100k, cùng trừ 50k và ghi đè kết quả 50k thay vì 0đ!

---

## 🌱 Phần 0 — Vì sao `counter++` không an toàn trong đa luồng?
Một dòng code `counter++` thực chất biên dịch thành **3 lệnh máy riêng lẻ**:
1. `LOAD`: Đọc `counter` từ RAM vào thanh ghi CPU.
2. `ADD 1`: Cộng thêm 1 trong ALU.
3. `STORE`: Ghi giá trị mới từ thanh ghi ra RAM.

Nếu OS ngắt Context Switch đúng giữa bước 2 và bước 3, hai luồng sẽ ghi đè kết quả của nhau — gọi là **Race Condition (Tranh chấp dữ liệu)**. Đoạn mã truy cập biến dùng chung được gọi là **Critical Section (Vùng tranh chấp)**.

---

## 1. Các công cụ Đồng bộ hóa (Synchronization Primitives)

| Công cụ | Giá trị / Trạng thái | Ai được phép mở khóa (Release)? | Tình huống sử dụng chuẩn |
|---|---|---|---|
| **Spinlock** | Vòng lặp bận (Busy-waiting) với lệnh nguyên tử CAS | Luồng đang giữ khóa | Vùng tranh chấp cực ngắn (< vài micro-giây) trong Kernel |
| **Mutex (Mutual Exclusion)** | Nhị phân (`Locked / Unlocked`) có hàng đợi ngủ (Sleep) | **Chính luồng đã Lock** (Ownership) | Bảo vệ biến dùng chung, cấu trúc dữ liệu, OCC Locks |
| **Semaphore (`SemaphoreSlim(k)`)** | Bộ đếm nguyên $N \ge 0$ (`Wait()` giảm, `Release()` tăng) | Bất kỳ luồng nào gọi `Release()` | Giới hạn đồng thời $K$ kết nối (ví dụ `SemaphoreSlim(5)` trong Outbox Worker) & Producer-Consumer |

---

## 2. Ba điều kiện bắt buộc của Critical Section
1. **Mutual Exclusion (Loại trừ lẫn nhau):** Tại một thời điểm tối đa 1 luồng ở trong Critical Section.
2. **Progress:** Nếu không có luồng nào trong Critical Section, các luồng đang chờ phải chọn được người vào mà không trì hoãn vô hạn.
3. **Bounded Waiting:** Mỗi luồng chờ phải có giới hạn số lần bị luồng khác chen ngang (chống Starvation).

<!-- studxy-os-v3 -->",
            "Race Condition xảy ra khi nhiều luồng thực thi không nguyên tử trên cùng vùng nhớ chia sẻ; Mutex đảm bảo loại trừ lẫn nhau có sở hữu.",
            "Semaphore đếm cho phép tối đa K luồng truy cập tài nguyên đồng thời hoặc đồng bộ tín hiệu Producer-Consumer."
        ),

        new OsLessonMeta(
            OsChapter2Id,
            OsLesson22Id,
            "Bài 2.2: Deadlock — 4 Điều kiện Coffman, Phòng tránh & Phục hồi",
            "deadlock-4-dieu-kien-coffman-va-phong-tranh",
            2,
            25,
            @"# Bài 2.2: Deadlock — 4 Điều kiện Coffman, Phòng tránh & Phục hồi

> [!TIP]
> **Deadlock (Khóa chết)** giống như hai người đi qua cầu hẹp: người A giữ nửa cầu trái và chờ nửa phải; người B giữ nửa cầu phải và chờ nửa trái — cả hai đứng im mãi mãi!

---

## 🌱 Phần 0 — Kịch bản Deadlock kinh điển
```text
Thread 1: Lock(A) ---> chờ Lock(B)...
Thread 2: Lock(B) ---> chờ Lock(A)...
=> Cả hai luồng bị treo vĩnh viễn!
```

---

## 1. Bốn điều kiện Coffman (Cần VÀ Đủ để xảy ra Deadlock)
Deadlock **chỉ xảy ra** khi cả 4 điều kiện sau đồng thời thỏa mãn:
1. **Mutual Exclusion (Loại trừ lẫn nhau):** Tài nguyên chỉ phục vụ 1 luồng tại một thời điểm.
2. **Hold and Wait (Giữ và Chờ):** Luồng đang giữ ít nhất 1 tài nguyên và tiếp tục xin thêm tài nguyên khác đang bị giữ.
3. **No Preemption (Không trưng dụng):** Không thể tước đoạt cưỡng bức tài nguyên đang được luồng khác nắm giữ.
4. **Circular Wait (Chờ đợi vòng tròn):** Tồn tại chu trình $T_1 \rightarrow T_2 \rightarrow \dots \rightarrow T_n \rightarrow T_1$.

---

## 2. Chiến lược Phá vỡ Deadlock trong Thực tế

| Chiến lược | Phá vỡ điều kiện nào? | Cách triển khai thực tế trong hệ thống |
|---|---|---|
| **Lock Ordering (Sắp xếp thứ tự khóa)** | Phá vỡ **Circular Wait** | Mọi luồng luôn khóa tài nguyên theo thứ tự ID tăng dần (`Lock(minId)` trước `Lock(maxId)`) |
| **TryLock + Timeout / Backoff** | Phá vỡ **Hold & Wait / No Preemption** | Nếu không lấy được khóa B trong 50ms thì nhả khóa A ra và thử lại ngẫu nhiên |
| **Thuật toán Banker (Banker's Algorithm)** | Tránh trạng thái không an toàn (Deadlock Avoidance) | Kiểm tra trước khi cấp phát: chỉ cấp tài nguyên nếu hệ thống còn giữ được ít nhất 1 chuỗi hoàn thành an toàn (Safe Sequence) |

<!-- studxy-os-v3 -->",
            "Deadlock chỉ xảy ra khi đồng thời thỏa mãn 4 điều kiện Coffman: Mutual Exclusion, Hold & Wait, No Preemption và Circular Wait.",
            "Sắp xếp thứ tự khóa toàn cục (Lock Ordering) là giải pháp thực chiến đơn giản và triệt để nhất để phá vỡ Circular Wait."
        )
    };

    private static string GetLesson32Markdown() => @"# Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp

> [!TIP]
> Mỗi chương trình chạy trên máy tính đều tin rằng nó sở hữu toàn bộ thanh RAM riêng tư, liên tục từ địa chỉ `0x0000...`. Ảo giác tuyệt đẹp này được tạo ra nhờ **Virtual Memory (Bộ nhớ ảo)** và phần cứng **MMU + TLB**!

---

## 1. Cơ sở lý thuyết Phân trang (Paging)
Phân trang cho phép không gian địa chỉ vật lý của tiến trình không cần liên tục:
- Không gian địa chỉ vật lý chia thành các **Khung trang (Frames)** kích thước cố định (thường 4 KB).
- Không gian địa chỉ logic (ảo) chia thành các **Trang (Pages)** cùng kích thước.
- Địa chỉ ảo gồm `VPN (Virtual Page Number) + Offset`; MMU tra bảng trang để đổi `VPN → PFN (Page Frame Number)` rồi ghép lại với `Offset` gốc.

---

## 2. Tăng tốc phần cứng với TLB & Công thức EAT
**TLB (Translation Lookaside Buffer)** là bộ nhớ cache liên kết siêu nhanh bên trong MMU, cho phép tra cứu ánh xạ `VPN → PFN` trong $O(1)$ chu kỳ xung nhịp nhờ **Tính cục bộ không gian & thời gian (Spatial & Temporal Locality)**.

$$\text{EAT} = \alpha \times (t_{\text{TLB}} + t_{\text{MEM}}) + (1 - \alpha) \times (t_{\text{TLB}} + 2 \times t_{\text{MEM}})$$

Trong đó $\alpha$ là tỷ lệ **TLB Hit Ratio**. Khi $\alpha = 98\%$, tốc độ truy cập bộ nhớ ảo gần như tương đương RAM trực tiếp!

---

## 3. Bảng trang đa cấp (Multi-level Page Tables)
Trên kiến trúc 64-bit, một bảng trang phẳng sẽ tiêu tốn hàng Terabyte RAM chỉ để lưu chỉ mục. Vì vậy Linux x86-64 dùng **bảng trang đa cấp 4 tầng** (`PGD → PUD → PMD → PTE`) kết hợp **cấp phát thưa (Sparse Allocation)**: chỉ tạo nhánh bảng trang cho những vùng nhớ thực sự được dùng.

<!-- studxy-os-v3 -->";

    private static string GetLesson33Markdown() => @"# Bài 3.3: Thuật toán Thay thế Trang (FIFO, Optimal, LRU, Clock & Working Set)

> [!TIP]
> Khi xảy ra **Page Fault** mà RAM đã đầy, hệ điều hành buộc phải chọn một trang đang nằm trong RAM làm ""nạn nhân"" (victim frame) để đẩy ra đĩa. Chọn đúng trang ít dùng nhất sẽ giảm Page Fault xuống hàng chục lần!

---

## 🌱 Phần 0 — Chi phí khổng lồ của Page Fault
Truy cập RAM mất khoảng `100 ns`, trong khi đọc trang từ SSD/HDD mất từ `100,000 ns` đến `10,000,000 ns` (chậm hơn từ **1,000 đến 100,000 lần**). Chỉ cần tỷ lệ Page Fault vượt quá 1% là máy tính sẽ rơi vào trạng thái **Thrashing** (đơ toàn tập vì CPU chỉ bận hoán đổi trang).

---

## 1. So sánh các Thuật toán Thay thế Trang

| Thuật toán | Nguyên lý chọn trang nạn nhân | Đánh đổi (Trade-off) |
|---|---|---|
| **Optimal (OPT / Belady)** | Đẩy trang lâu nhất trong tương lai mới được dùng lại | Lý thuyết tối ưu nhất (ít Page Fault nhất), không cài được thật vì không biết tương lai |
| **FIFO** | Đẩy trang vào RAM sớm nhất (Queue) | Dễ cài đặt nhưng dính **Nghịch lý Belady** (tăng số khung RAM lại tăng Page Fault!) |
| **LRU (Least Recently Used)** | Đẩy trang lâu nhất chưa được truy cập trong quá khứ | Rất sát Optimal, không dính nghịch lý Belady; chi phí theo dõi dấu thời gian cao |
| **Clock (Second-Chance)** | Duyệt vòng tròn kiểm tra **Reference Bit (R)**: `R=1` thì xóa thành `0` cho cơ hội thứ 2, `R=0` thì thay | Xấp xỉ LRU siêu nhẹ, được dùng thực tế trong nhân Linux & Windows |

---

## 2. Mô hình Working Set & Chống Thrashing
**Working Set $W(t, \Delta)$** là tập các trang mà tiến trình thực sự truy cập trong cửa sổ $\Delta$ tham chiếu gần nhất. Nếu tổng kích thước Working Set của các tiến trình đang chạy lớn hơn tổng RAM vật lý ($\sum |W_i| > \text{TotalFrames}$), hệ điều hành phải tạm đình chỉ (Swap out) một số tiến trình để cứu hiệu năng toàn hệ thống.

<!-- studxy-os-v3 -->";

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
