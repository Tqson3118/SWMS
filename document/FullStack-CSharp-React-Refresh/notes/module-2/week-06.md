# Tuần 06 — Collections, generics và delegates

Chặng 2; 3 bài L06-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W06-01: Chọn collection theo truy cập, duplicate và chi phí trung bình.

- LO-W06-02: Viết generic helper có constraint và phân biệt compile/runtime safety.

- LO-W06-03: Dùng Func/Action/Predicate để lọc và kiểm tra captured state.

Tiên quyết: Đạt gate tuần 05; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-06-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w06-01"></a>

## Bài 1: List, Dictionary, HashSet

LO-W06-01 · W06-C01

List<T> là dãy có thứ tự, truy cập index O(1), tìm theo điều kiện O(n). Dictionary<TKey,TValue> lookup key trung bình O(1), cần equality/hash đúng, không có guarantee mọi case O(1). HashSet<T> theo membership/unique, Add trả false nếu duplicate theo comparer. Dùng Dictionary categoryId->name khi render nhiều expense; không tìm List category lặp trong loop nếu dữ liệu lớn. Không sửa List trong foreach vì enumerator bị invalidated; dùng RemoveAll(predicate) hoặc duyệt snapshot khi phù hợp. Dictionary không thay index DB và HashSet memory không bảo đảm chống trùng giữa process. Chọn collection theo yêu cầu trước tối ưu.

<a id="lo-w06-02"></a>

## Bài 2: Generics và constraints

LO-W06-02 · W06-C02

Generic type/method nhận type parameter T, reuse thuật toán mà giữ compile-time type safety. Không cast object tùy tiện hoặc tạo List<object> khi domain homogeneous. where T:notnull hỗ trợ key không-null; where T:class cho reference; where T:new() cho tạo instance public parameterless; interface constraint cho hành vi. Constraints chỉ cho phép dùng members được contract xác định, không biến T thành dynamic. IEnumerable<T> mô tả khả năng enumerate, IReadOnlyList<T> thêm count/index nhưng không bảo đảm underlying object immutable. Generic wrapper không phải abstraction tốt nếu chỉ che code một lần và làm debug khó hơn.

<a id="lo-w06-03"></a>

## Bài 3: Delegate, lambda và callback

LO-W06-03 · W06-C03

Delegate là type-safe callable reference; Func<Expense,bool> trả bool, Action<Expense> không trả value, Predicate<T> là filter contract phổ biến. Lambda tạo function expression; e=>e.AmountVnd>threshold capture threshold lexical, có thể thấy threshold bị đổi. Tránh capture loop/shared mutable state khi dùng concurrent tasks. Method group có thể truyền thay lambda nếu signature khớp. Callback dùng cho rule/transform; không cần event bus khi một function param đủ. Event pattern cho thông báo nhiều subscribers là tùy chọn. Để tính báo cáo, truyền pure predicate, không sửa mảng trong callback.

## Ví dụ giải từng bước

1. Bài toán: lookup category và bỏ id nhập trùng trong batch.
2. Dictionary lookup, HashSet membership; không khẳng định chống trùng toàn DB.
3. Console:
```csharp
var categories=new Dictionary<string,string>{{"f1","Food"},{"t1","Travel"}};
var seen=new HashSet<string>();
foreach(var id in new[]{"f1","f1","t1"}) {
 if(seen.Add(id)&&categories.TryGetValue(id,out var name)) Console.WriteLine(name);
}
var xs=new List<long>{50000,20000,30000};
Func<long,bool> large=x=>x>=30000;
Console.WriteLine(string.Join(",",xs.Where(large))); // 50000,30000
xs.RemoveAll(x=>x<30000);
Console.WriteLine(xs.Count); // 2
```
4. Output Food/Travel một lần; missing key dùng TryGetValue không throw. Đổi comparer để thấy case sensitivity.

## Đào sâu và lỗi thường gặp

Hash/equality inconsistency làm lookup sai dù code compile. IReadOnly collection interface không freeze source; đừng expose mutable List domain trực tiếp. Delegate giúp Strategy đơn giản, chỉ tạo interface Strategy khi cần state/lifecycle nhiều method.

## Bài tập thực hành

### Bài tập 1

Group sums bằng Dictionary cho fixture; không dùng LINQ.

### Bài tập 2

Viết generic DistinctByKey<T,TKey>(IEnumerable<T>,Func<T,TKey>) where TKey:notnull.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Duyệt expense; TryGetValue category total cũ; checked cộng; assign. Food80000,Travel20000; [] dictionary rỗng. Complexity trung bình O(n).

### Hướng giải 2

HashSet<TKey>, foreach item nếu set.Add(key(item)) thì yield return item; giữ lần đầu. Test key a,a,b -> a,b; không biến loop thành mutation source.

## Project task

Console filter/report theo category và deduplicate import draft trong memory. Giữ giải thích đây chỉ là batch-local dedup; idempotency DB học W16.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w06.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Dictionary report Food80000/Travel20000, empty đúng; missing category báo lỗi rõ.

- [ ] Không Collection was modified khi xóa; Add HashSet nhận trùng đúng.

- [ ] Generic helper và Func filter chạy với2 kiểu dữ liệu, giải thích constraint.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'Why did you choose a Dictionary instead of a List?' Thêm average lookup, duplicate, constraint, delegate, captured variable.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Generic collections](https://learn.microsoft.com/en-us/dotnet/standard/collections/selecting-a-collection-class) — W06-C01; primary. HTTP: 200; section xem ledger.

- [Constraints on type parameters](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters) — W06-C02; primary. HTTP: 200; section xem ledger.

- [Delegates](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/) — W06-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-06-quiz.md) · [Đáp án](../../assessments/answer-keys/week-06-answers.md) · [Flashcards](../../flashcards/week-06.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-05.md)

[Tuần tiếp](week-07.md)
