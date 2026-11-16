# Tuần 05 — C# value/reference, OOP và invariants

Chặng 2; 3 bài L05-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W05-01: Dự đoán copy/alias, phân biệt identity/value equality và sửa shared mutation.

- LO-W05-02: Tạo domain Expense bảo vệ amount/note và giải thích interface/composition.

- LO-W05-03: Viết domain có guard số tiền và unit tests cho biên.

Tiên quyết: Đạt gate tuần 04; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-05-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w05-01"></a>

## Bài 1: Value, reference và equality

LO-W05-01 · W05-C01

Value type assignment copy value; reference type assignment copy reference nên hai biến có thể trỏ cùng object. Không kết luận value luôn stack/reference luôn heap: storage phụ thuộc context, boxing/fields/closures. Struct chứa reference vẫn copy reference field shallow. string là reference type immutable nên thay chuỗi tạo giá trị mới, không sửa cùng chuỗi. Class mặc định Equals thường identity trừ override; record dùng value-based equality cho members, nhưng collection member không tự deep compare nội dung. == có thể overload nên phải đọc type. Dùng debugger xem hai biến trước/sau mutate; lựa chọn class/record/struct theo semantics và kích thước, không theo mẹo stack nhanh.

<a id="lo-w05-02"></a>

## Bài 2: OOP và encapsulation

LO-W05-02 · W05-C02

Encapsulation gom state với quy tắc thay đổi; private setter tránh caller bỏ qua validation. Constructor/factory kiểm tra invariants, method Rename/ChangeAmount biểu diễn hành vi. Inheritance dùng khi subtype thực sự substitutable; composition ghép collaborator thường dễ test hơn. Interface là contract hành vi, không phải nơi mặc định đặt mọi implementation. Polymorphism gọi đúng implementation qua interface mà caller không biết concrete type. Domain không biết HTTP/DbContext/React; lỗi domain được API map sang status sau. SOLID thực dụng: một lý do thay đổi, dependency hướng vào policy, thêm rule qua boundary có nhu cầu; không xây 10 layer cho CRUD nhỏ.

<a id="lo-w05-03"></a>

## Bài 3: Money/date và test đầu tiên

LO-W05-03 · W05-C03

Theo project-spec: long AmountVnd nguyên 1..1e9, DateOnly SpentOn, note tối đa200. checked bắt overflow khi cộng long; decimal phù hợp tính tỷ lệ hoặc đa tiền tệ về sau, không dùng double cho decimal currency. Truyền ngày vào nghiệp vụ thay gọi DateTime.Now rải rác để test deterministic. Unit test kiểm tra behavior: 1 hợp lệ,0/âm/trênmax invalid, note trim. Arrange input, Act gọi domain, Assert expected. Không test lại auto-property getter như tiêu chí hiểu OOP. Xunit Assert.Throws cho guard lỗi; fixture string/Guid cố định, không random làm expected khó tái hiện.

## Ví dụ giải từng bước

1. Bài toán: class alias so với struct copy.
2. Dự đoán output trước chạy; Type không quyết định đơn giản vị trí memory.
3. Program.cs console:
```csharp
var a=new Box { Amount=1 }; var b=a; b.Amount=2;
var x=new Money(1); var y=x; y=new Money(2);
Console.WriteLine($"{a.Amount} {x.Amount} {y.Amount}"); // 2 1 2
public sealed class Box { public long Amount {get;set;} }
public readonly record struct Money(long Amount);
```
Domain trong Expense.Domain:
```csharp
public sealed class Expense {
 public long AmountVnd {get;private set;}
 public Expense(long amount) => ChangeAmount(amount);
 public void ChangeAmount(long amount) {
  if(amount<=0||amount>1_000_000_000L) throw new ArgumentOutOfRangeException(nameof(amount));
  AmountVnd=amount;
 }
}
```
4. Expected new Expense(0) throw; new Expense(50000) hợp lệ. Mở rộng DateOnly/note theo spec, không xóa guard để test xanh.

## Đào sâu và lỗi thường gặp

Boxing value type vào object tạo boxed copy; sửa biến gốc không tự sửa boxed value. Record with copy cũng shallow đối với reference member. Invariant cần tồn tại cả domain và boundary, vì DB/import có thể gọi domain từ đường khác.

## Bài tập thực hành

### Bài tập 1

Thêm note trim<=200 và test whitespace/201 ký tự.

### Bài tập 2

Thay Report kế thừa DbContext bằng ReportService nhận IExpenseReader; giải thích.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Chuẩn hóa note?.Trim(); nếu Length>200 trả validation/throw theo domain contract. Whitespace thành chuỗi rỗng hoặc null quyết định nhất quán; test không vượt200 và không mất note hợp lệ.

### Hướng giải 2

ReportService sử dụng reader theo composition, không phải một DbContext. Interface chỉ có dữ liệu cần đọc; mock trả fixture, report total100000. Domain không phụ thuộc EF.

## Project task

Tạo Expense.Domain và console nhập fixture, guard money/date/note. Chạy unit tests biên; console report total100000. Giữ JS UI ở commit cũ.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w05.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Tự dự đoán và giải thích output2/1/2 cùng class alias/struct copy.

- [ ] Domain chặn0/âm/trên1e9/note201; nhận fixture; tests xanh.

- [ ] Console total100000, domain không reference ASP.NET/EF.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'What is copied when I assign a class variable?' Đọc value types và encapsulation, ghi invariant/reference/equality.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Value types and copying](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-types) — W05-C01; primary. HTTP: 200; section xem ledger.

- [Encapsulation, inheritance and polymorphism](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/) — W05-C02; primary. HTTP: 200; section xem ledger.

- [Unit testing C# with xUnit](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit) — W05-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-05-quiz.md) · [Đáp án](../../assessments/answer-keys/week-05-answers.md) · [Flashcards](../../flashcards/week-05.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](../module-1/week-04.md)

[Tuần tiếp](week-06.md)
