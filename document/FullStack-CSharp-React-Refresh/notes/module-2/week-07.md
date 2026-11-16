# Tuần 07 — LINQ, nullable và exception boundaries

Chặng 2; 3 bài L07-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W07-01: Giải thích query deferred, chạy filter/projection/grouping và tránh enumerate thừa.

- LO-W07-02: Loại warnings null bằng guard đúng, không dùng null-forgiving để che lỗi.

- LO-W07-03: Đặt catch tại boundary, bảo toàn stack và phân biệt expected failures.

Tiên quyết: Đạt gate tuần 06; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-07-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w07-01"></a>

## Bài 1: LINQ deferred và materialization

LO-W07-01 · W07-C01

Where/Select thường deferred: tạo pipeline chưa enumerate. ToList/ToArray materialize snapshot; Sum/Count/First cũng thực thi terminal operation. IEnumerable LINQ chạy trên .NET objects; IQueryable expression có thể được EF translate SQL, không tùy tiện gọi method custom trong query DB. Source list đổi trước enumerate có thể đổi kết quả; mỗi enumerate có thể lặp công việc hoặc IO tùy provider. GroupBy category rồi Sum AmountVnd tạo report; OrderBy/ThenBy giữ thứ tự xác định. First throw nếu không có element, FirstOrDefault trả default có thể null/0 nên caller phải phân biệt absence.

<a id="lo-w07-02"></a>

## Bài 2: Nullable reference types

LO-W07-02 · W07-C02

NRT là compiler analysis cho annotation string/string?, không tự cài runtime validation. Enable Nullable trong csproj; string? cho data có thể thiếu. if(value is null) guard làm flow analysis biết branch còn lại non-null. ?. access và ?? fallback cần semantics rõ; note null có thể hợp lệ, category missing thì không. ! chỉ thông báo compiler tin tác giả, không biến null thành object; dùng chỉ khi invariant thật đã chứng minh. DTO từ deserialization vẫn có thể thiếu field; API/domain guard runtime. Try-pattern giúp parse without expected exception, ví dụ DateOnly.TryParseExact với invariant culture.

<a id="lo-w07-03"></a>

## Bài 3: Exceptions và error contract

LO-W07-03 · W07-C03

Exception biểu diễn failure phá normal flow; validation expected có thể return result thay throw liên tục. Catch loại có thể xử lý; catch(Exception) ở global boundary ghi log và response generic, không nuốt lỗi. throw; giữ stack; throw ex; reset điểm stack nên tránh. finally dọn trạng thái/resource, using đảm bảo Dispose sau scope; không đặt return trong finally. Cancellation OperationCanceledException gắn token được coi cancellation, không lỗi500 mặc định. Domain không trả HTTP status; API adapter map Argument/validation/conflict theo contract. Log structured dữ liệu không nhạy cảm và exception, không token/note đầy đủ.

## Ví dụ giải từng bước

1. Bài toán: deferred query trước/sau sửa source.
2. Query và snapshot có thời điểm khác nhau.
3. Console:
```csharp
var xs=new List<long>{50000,20000};
var q=xs.Where(x=>x>=30000);
var snapshot=q.ToList();
xs.Add(30000);
Console.WriteLine(q.Sum()); // 80000
Console.WriteLine(snapshot.Sum()); // 50000
string? note=null;
Console.WriteLine(note?.Trim()??"(không ghi chú)");
```
4. Nếu source bất biến kết quả bằng nhau; đừng materialize mọi query sớm vì mất SQL composition sau này. Null note hợp lệ nhưng categoryId null không được fallback thành category của user khác.

## Đào sâu và lỗi thường gặp

LINQ đọc gọn không bảo đảm query nhanh. Đếm Count rồi foreach có thể enumerate2 lần; materialize khi cần snapshot và hiểu memory. Async exception ở Task chỉ được quan sát khi await; fire-and-forget có thể mất error boundary.

## Bài tập thực hành

### Bài tập 1

Group fixture theo category, order total giảm dần và kiểm []

### Bài tập 2

Parse ngày yyyy-MM-dd và note optional; test ngày không tồn tại và null.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

GroupBy category; Select g=>new {Category=g.Key,Total=g.Sum(x=>x.AmountVnd)}; OrderByDescending Total rồi ThenBy Category. Food80000,Travel20000; [] no rows.

### Hướng giải 2

DateOnly.TryParseExact với CultureInfo.InvariantCulture/DateTimeStyles.None; '2026-02-30' invalid, '2026-10-01' valid; note?.Trim() cho null an toàn. Không dùng ! để lách guard.

## Project task

Report domain LINQ có projection và null-aware import validation. Tạo error result {row,field,message} cho lỗi input; không nuốt exception IO.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w07.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Deferred demo q80000/snapshot50000 và giải thích thời điểm enumerate.

- [ ] Report đúng fixture, empty và sort tie deterministic.

- [ ] Nullable warnings quan trọng được giải bằng guard; invalid date không crash hoặc lưu sai.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'When does this LINQ query execute?' Dùng deferred execution, materialize, nullable, boundary, stack trace.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Deferred execution and lazy evaluation](https://learn.microsoft.com/en-us/dotnet/standard/linq/deferred-execution-lazy-evaluation) — W07-C01; primary. HTTP: 200; section xem ledger.

- [Nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references) — W07-C02; primary. HTTP: 200; section xem ledger.

- [Best practices for exceptions](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions) — W07-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-07-quiz.md) · [Đáp án](../../assessments/answer-keys/week-07-answers.md) · [Flashcards](../../flashcards/week-07.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-06.md)

[Tuần tiếp](week-08.md)
