# Tuần 03 — DOM, sự kiện, modules và persistence nhỏ

Chặng 1; 3 bài L03-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W03-01: Xử lý submit, render an toàn và event delegation cho list.

- LO-W03-02: Tách domain/storage/view module và hiểu import/export qua HTTP.

- LO-W03-03: Lưu/đọc JSON có guard và recovery khi dữ liệu hỏng.

Tiên quyết: Đạt gate tuần 02; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-03-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w03-01"></a>

## Bài 1: DOM và event flow

LO-W03-01 · W03-C01

DOM là cây object browser biểu diễn document. querySelector có thể null; tải module sau markup hoặc dùng module deferred rồi kiểm tra type. Event đi capture -> target -> bubble; handler form submit nhận cả click nút và Enter. preventDefault chặn hành vi mặc định như reload, không chặn propagation. Delegation đặt một handler trên list, dùng event.target.closest('button[data-id]') và kiểm tra phần tử thuộc list; không gắn lặp handler sau mỗi render. textContent render text user, createElement tạo node; innerHTML với note chưa tin cậy có thể XSS. State là mảng expense; DOM là projection, không lấy text tiền đã format để tính tổng.

<a id="lo-w03-02"></a>

## Bài 2: ES modules và boundary

LO-W03-02 · W03-C02

File module export named functions, import đúng tên và đường dẫn có extension trong browser. script type=module có scope riêng, chạy deferred, strict mode; không ngầm đưa function vào window cho onclick inline. Modules phân boundary: money.js tính toán, storage.js persist, view.js render, app.js wiring. Không tạo circular dependency app<->domain; truyền callback hoặc dữ liệu qua function. Named export và default export khác cú pháp import, không rename nhầm. File:// có hạn chế origin và module nên dùng HTTP local server. MIME/path lỗi xuất hiện Network/Console; sửa URL thay vì mở browser bỏ bảo mật. Mỗi module chỉ giữ side effects cần thiết và test domain không cần document.

<a id="lo-w03-03"></a>

## Bài 3: localStorage và lỗi dữ liệu

LO-W03-03 · W03-C03

localStorage key/value đều string, tồn tại qua reload nhưng có thể bị xóa hoặc bị người dùng/XSS sửa. JSON.parse throw khi format sai, còn JSON hợp lệ chưa chắc đúng schema. Đọc key null thì default []; parse xong kiểm tra array và fields từng item. try/catch phải phân biệt storage unavailable/quota và dữ liệu hỏng; hiển thị lỗi, không giả vờ đã lưu. Chỉ ghi sau domain validate; dùng schemaVersion để migration sau này. Không lưu JWT/secret trong localStorage. UUID bằng crypto.randomUUID cho item tạm, id ổn định khi edit/delete. Khi reload, render từ state phục hồi, tổng tính từ data chứ không từ DOM. Persistence JS là lab thay thế bằng API từ tuần 13.

## Ví dụ giải từng bước

1. Bài toán: submit amount một lần, không reload, note '<b>x</b>' hiện nguyên văn.
2. Tách validate/state khỏi render; không dùng innerHTML.
3. app.js là script module, HTML tuần 1 cần input note name=note:
```javascript
const form=document.querySelector("#expense-form");
const message=document.querySelector("#message");
if(!(form instanceof HTMLFormElement)||!message) throw new Error("Thiếu markup");
let expenses=[];
form.addEventListener("submit",event=>{
 event.preventDefault();
 const data=new FormData(form);
 const raw=String(data.get("amountVnd")??"").trim();
 const value=Number(raw);
 if(!raw||!Number.isSafeInteger(value)||value<=0||value>1e9){
  message.textContent="Số tiền không hợp lệ"; return;
 }
 expenses=[...expenses,{id:crypto.randomUUID(),amountVnd:value}];
 message.textContent="Đã thêm "+expenses.length+" khoản";
});
```
4. Add 50000 hai lần -> count 2, page không reload. Thêm category/date/note theo project-spec trước gọi đây là feature đủ.

## Đào sâu và lỗi thường gặp

preventDefault không thay validation; event delegation cần guard target Element. JSON parse thành công không thay schema validation. Tránh lặp listeners gây double-submit: wiring một lần, render chỉ thay nodes. Storage version migration chỉ áp dữ liệu lab; backup trước biến đổi.

## Bài tập thực hành

### Bài tập 1

Tạo money.js export total; app.js import và tính fixture. Cố ý sai đường dẫn rồi debug.

### Bài tập 2

Đặt key expense-lab='bad json', reload. Thiết kế recover mà không crash.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

export function total(xs){return xs.reduce((s,x)=>s+x.amountVnd,0)}; import {total} from './money.js'. Expected100000; Network404 khi path sai; sửa path/case, không đổi sang global.

### Hướng giải 2

try JSON.parse; catch báo dữ liệu hỏng, chọn xóa key lab sau khi người dùng xác nhận hoặc dùng mảng rỗng trong memory. JSON {} phải bị schema guard chặn dù parse không throw.

## Project task

CRUD JS add/edit/delete bằng stable id, form/category/date/note đầy đủ; render text; persist array có schema guard. Có error/empty state, recovery và fixture.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w03.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Add/edit/delete fixture cập nhật tổng đúng và reload còn dữ liệu.

- [ ] Note '<img src=x onerror=alert(1)>' hiển thị text, không chạy script.

- [ ] Key thiếu/hỏng/sai shape không crash; submit không reload hoặc thêm hai lần.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'What is the difference between preventDefault and stopping propagation?' Viết 5 câu về module boundary và storage errors.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Event bubbling and delegation](https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Scripting/Event_bubbling) — W03-C01; primary. HTTP: 200; section xem ledger.

- [JavaScript modules](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules) — W03-C02; primary. HTTP: 200; section xem ledger.

- [localStorage description](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage) — W03-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-03-quiz.md) · [Đáp án](../../assessments/answer-keys/week-03-answers.md) · [Flashcards](../../flashcards/week-03.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-02.md)

[Tuần tiếp](week-04.md)
