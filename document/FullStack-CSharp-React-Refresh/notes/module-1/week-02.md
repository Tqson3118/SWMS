# Tuần 02 — JavaScript: scope, closure, array và tiền

Chặng 1; 3 bài L02-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W02-01: Dự đoán scope/closure và viết factory quản lý ngân sách.

- LO-W02-02: Dùng map/filter/reduce/find/some với mảng rỗng và không mutate input.

- LO-W02-03: Validate số tiền nguyên và viết hàm thuần cho fixture báo cáo.

Tiên quyết: Đạt gate tuần 01; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-02-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w02-01"></a>

## Bài 1: Scope và closure

LO-W02-01 · W02-C01

let/const có block scope; var có function scope và dễ gây callback dùng chung biến vòng lặp. const cấm gán lại binding, không làm object immutable. Closure là function kèm lexical environment nơi được tạo; environment còn được dùng sau function ngoài return. Factory tạo hai counter có state riêng. Closure giữ reference tới biến, không luôn đóng băng một value tại thời điểm tạo; callback chạy sau có thể thấy value mới. Trước let/const initialization có temporal dead zone; không dựa vào hoisting để gọi biến chưa khởi tạo. Parameter/local scope che biến ngoài cùng tên; rename rõ nghĩa trước debug. Dùng debugger Scope để quan sát, không đoán bằng log duy nhất.

<a id="lo-w02-02"></a>

## Bài 2: Array methods và immutability

LO-W02-02 · W02-C02

map biến đổi mỗi phần tử và trả mảng cùng số phần tử; filter giữ phần tử thỏa predicate; reduce gom với initial value; find trả phần tử đầu hoặc undefined; some trả boolean tồn tại. forEach dùng side effect và trả undefined, không thay map. sort mặc định so string và mutate; tạo bản sao [...items].sort((a,b)=>a.amountVnd-b.amountVnd). Spread mảng chỉ copy shallow: object bên trong vẫn chung reference, nên update expense bằng map và object spread. Tổng dùng reduce((s,e)=>s+e.amountVnd,0), mảng rỗng ->0. Optional result từ find phải kiểm tra trước property access. Viết loop trước rồi so array method để hiểu callback, không ép mọi bài vào reduce khó đọc.

<a id="lo-w02-03"></a>

## Bài 3: Validation, equality và dữ liệu

LO-W02-03 · W02-C03

Input form luôn có thể là string. Number('') là 0 nên check trim rỗng trước convert. Number.isSafeInteger(amount) && amount>0 && amount<=1000000000 là guard tiền VND. == có coercion, === so kiểu và value nên mặc định chọn ===; object === so identity, không deep value. Hàm thuần không sửa input và output phụ thuộc input: filter/report dễ test. Giữ spentOn string yyyy-MM-dd cho ngày chi, không parse UTC date rồi hiển thị theo timezone khi chỉ cần ngày. Boundary validation phải phân biệt null/missing/empty. Tổng vượt safe integer cần reject hoặc hợp đồng representation khác; core giới hạn dữ liệu và backend checked long, không âm thầm mất chính xác.

## Ví dụ giải từng bước

1. Bài toán: Alice Food 50000 và 30000, Travel 20000. Báo cáo Food.
2. Dùng filter cho membership, reduce cộng tiền; closure giữ limit.
3. Chạy trong DevTools:
```javascript
const xs = [
 {id:"a",category:"Food",amountVnd:50000},
 {id:"b",category:"Travel",amountVnd:20000},
 {id:"c",category:"Food",amountVnd:30000}
];
const sum = xs.filter(x=>x.category==="Food").reduce((s,x)=>s+x.amountVnd,0);
function makeBudget(limit) { return total => Math.max(0,total-limit); }
const foodOver = makeBudget(70000);
console.log(sum,foodOver(sum)); // 80000 10000
const changed = xs.map(x=>x.id==="a"?{...x,amountVnd:60000}:x);
console.log(xs[0].amountVnd,changed[0].amountVnd); // 50000 60000
```
4. Đổi filter thành Unknown -> total 0; check original không đổi. Hai makeBudget(70000)/(90000) cho kết quả 10000/0 trên 80000.

## Đào sâu và lỗi thường gặp

Giữ callback chỉ một trách nhiệm. Mutate object trong map vẫn mutate input dù mảng mới. Closure dùng lâu có thể giữ dữ liệu lớn còn reachable; xóa event listener/cache khi không cần. Tổng 0 không đồng nghĩa có dữ liệu: report UI cần empty state riêng.

## Bài tập thực hành

### Bài tập 1

Viết totalByCategory(xs,name) và kiểm tra []/unknown/fixture.

### Bài tập 2

Viết parseAmount cho '', '  ', '1.5', '-1', '50000', 'Infinity'.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

filter theo category === name rồi reduce với 0. [] và unknown ->0, Food ->80000. Thử input trước/sau bằng JSON.stringify để phát hiện mutation.

### Hướng giải 2

trim rỗng reject; Number; safe integer/positive/max. Chỉ 50000 được nhận; báo lỗi field. Không parseInt('1.5') vì biến dữ liệu sai thành 1.

## Project task

Thêm module hàm tính total/category/budget và unit-like assertions console cho fixture. UI chưa gọi network. Hàm domain JS độc lập DOM để tuần 3 tái dùng.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w02.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Fixture total 100000; Food 80000; excess Food 10000; [] total 0.

- [ ] Update một dòng tạo mảng/object mới, input gốc 50000 không đổi.

- [ ] Guard nhận 50000 và chặn rỗng, phần thập phân, âm, NaN/Infinity và trên max.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Đọc closure và indexed collections. Thu: 'Why does const not make this expense immutable?' Giải thích map, filter, reduce bằng một câu mỗi hàm.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Lexical scoping and closure](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Closures) — W02-C01; primary. HTTP: 200; section xem ledger.

- [Array methods and copying](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array) — W02-C02; primary. HTTP: 200; section xem ledger.

- [Number.isSafeInteger](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/isSafeInteger) — W02-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-02-quiz.md) · [Đáp án](../../assessments/answer-keys/week-02-answers.md) · [Flashcards](../../flashcards/week-02.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-01.md)

[Tuần tiếp](week-03.md)
