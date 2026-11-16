# Tuần 08 — Task, cancellation, disposal và DI

Chặng 2; 3 bài L08-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W08-01: Viết async import, truyền cancellation và không block Task bằng Result.

- LO-W08-02: Hủy import trước commit, dispose stream và phân biệt timeout/cancellation.

- LO-W08-03: Đăng ký DI đúng lifetime và tránh singleton giữ scoped dependency.

Tiên quyết: Đạt gate tuần 07; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-08-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w08-01"></a>

## Bài 1: Task và async I/O

LO-W08-01 · W08-C01

Task đại diện completion, không đồng nghĩa thread riêng. I/O-bound dùng API async và await; CPU-bound chỉ cân nhắc Task.Run ở client/console, ASP.NET không bọc IO Task.Run để tăng throughput giả. Không .Result/.Wait trong request vì block thread và có thể deadlock trong synchronization context khác. Return Task<T> thay async void trừ event handler; caller await và xử lý failure. Task.WhenAll không làm DbContext thread-safe; không chạy parallel operations chung instance. Async method chạy synchronous đến await chưa completed đầu tiên; giữ cancellation token truyền xuyên boundary. Đo throughput/latency trước đổi concurrency.

<a id="lo-w08-02"></a>

## Bài 2: CancellationToken và resource

LO-W08-02 · W08-C02

Cancellation hợp tác: token.ThrowIfCancellationRequested tại điểm hợp lý, truyền token vào ReadAsync/DB calls; caller request cancel không cưỡng chế mọi code dừng. catch OperationCanceledException khi token cancellation để báo 'đã hủy'; không nuốt rồi return success. using/await using gọi Dispose/DisposeAsync deterministic cho IDisposable/IAsyncDisposable dù exception; GC không thay đóng file/socket đúng lúc. Resource ownership: ai tạo có trách nhiệm dispose trừ contract khác; injected DbContext/HttpClientFactory thường container/factory quản lifecycle, không dispose tùy tiện. Hủy sau commit không rollback, trả operation status theo batchId. Timeout có giới hạn duration, khác caller cancel dù cùng cơ chế token có thể dùng linked source.

<a id="lo-w08-03"></a>

## Bài 3: Dependency Injection lifetimes

LO-W08-03 · W08-C03

DI đưa collaborator qua constructor; container xây graph và quản lifecycle. Transient mới mỗi resolution, scoped một instance mỗi scope (web request phổ biến), singleton một instance cho container. Scoped DbContext không thread-safe, một unit of work; singleton report cache không giữ trực tiếp scoped DbContext (captive dependency). Background job tạo scope cho từng job bằng IServiceScopeFactory; chưa cần implement ở console lab. Domain chỉ biết interface reader/importer cần thiết; concrete registered ở composition root. DI không thay encapsulation hoặc automatically thread safety. Kiểm tra scope validation ở Development; service singleton có mutable state cần synchronization hoặc tránh state.

## Ví dụ giải từng bước

1. Bài toán: đọc file fixture có thể hủy và đóng handle.
2. Await IO, truyền token, using stream do hàm sở hữu.
3. Console:
```csharp
using var cts=new CancellationTokenSource();
await File.WriteAllTextAsync("fixture.txt","50000\n20000\n30000");
cts.Cancel();
try { Console.WriteLine(await ReadAsync("fixture.txt",cts.Token)); }
catch(OperationCanceledException) { Console.WriteLine("Đã hủy"); }
static async Task<string> ReadAsync(string path,CancellationToken token) {
 using var reader=new StreamReader(path);
 token.ThrowIfCancellationRequested();
 return await reader.ReadToEndAsync(token);
}
```
4. Expected Đã hủy, file handle đóng. Bỏ cts.Cancel để đọc3 dòng. Import production validate tất cả rows rồi commit1 transaction W12/W16.

## Đào sâu và lỗi thường gặp

DI lifetime không tương đương lifetime DB transaction. Một request có thể nhiều queries; transaction explicit cho multi-write. Cancellation token source phải dispose khi hàm tạo nó. Hủy read partial phải discard staging rows; import batch lớn giới hạn1000 nên chưa cần streaming DB partial commit.

## Bài tập thực hành

### Bài tập 1

Viết SumAsync đọc dòng VND, hủy sau10 dòng trên file1000 dòng; không ghi DB.

### Bài tập 2

Reader dùng scoped DbContext, Report singleton. Sửa DI và giải thích.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

await read line token, TryParse long guard; token check mỗi lượt. Return tổng chỉ khi complete; catch cancellation bên caller, không expose partial như success. Tạo fixture có dòng invalid để test error row.

### Hướng giải 2

Đổi Report scoped hoặc tạo scope per job nếu background. Không inject scoped reader vào singleton. Test2 scopes reader khác, cùng scope dùng chung; container dispose scoped resources.

## Project task

Console import staging rows có cancellation, validation và tests; dependency reader qua constructor. Ghi ADR ngắn why scoped/why no async void. Tag phase2-domain.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w08.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Cancel trước khi đọc/giữa staging báo canceled, không trả success partial.

- [ ] Stream đóng cả success/error/cancel; run lại không file locked.

- [ ] Task được await; không .Result/async void cho service; DI không captive scoped.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'Does async await create a new thread?' Trả lời rồi mô tả cancellation hợp tác và disposal.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Task asynchronous programming model](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/) — W08-C01; primary. HTTP: 200; section xem ledger.

- [Cooperative cancellation](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads) — W08-C02; primary. HTTP: 200; section xem ledger.

- [Service lifetimes and scopes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) — W08-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-08-quiz.md) · [Đáp án](../../assessments/answer-keys/week-08-answers.md) · [Flashcards](../../flashcards/week-08.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-07.md)

[Tuần tiếp](../module-3/week-09.md)
