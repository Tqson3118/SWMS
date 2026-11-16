# Tuần 01 — HTML/CSS và giao diện có thể sử dụng

Chặng 1; 3 bài L01-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W01-01: Dựng form và danh sách khoản chi dùng semantic elements, label và bàn phím.

- LO-W01-02: Giải thích box model và chọn Flexbox/Grid cho layout chi tiêu.

- LO-W01-03: Kiểm tra UI ở 320/768/1280px, zoom và keyboard; sửa lỗi overflow.

Tiên quyết: Hoàn thành setup, đọc project-spec; đã từng học biến/hàm/if/loop.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-01-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w01-01"></a>

## Bài 1: Semantic HTML và form

LO-W01-01 · W01-C01

HTML biểu diễn ý nghĩa, CSS biểu diễn cách nhìn. Dùng header cho đầu trang, main cho nội dung chính, nav cho điều hướng; mỗi trang có heading mô tả nhiệm vụ. Form gom input cùng submit, label for phải khớp id. Input name quyết định key FormData; id chỉ định vị phần tử. button mặc định có thể submit trong form nên nút xóa đặt type="button". required/min/maxlength hỗ trợ người dùng nhưng backend vẫn phải validate. Table cần caption và th scope="col" khi dữ liệu có cột; danh sách ngắn có thể dùng ul/li. Giữ focus thấy rõ, thao tác Tab/Enter phải thực hiện cùng hành vi với chuột. aria-live="polite" thông báo kết quả; không đặt aria lên mọi div để thay semantic sẵn có.

<a id="lo-w01-02"></a>

## Bài 2: Box model, Flexbox, Grid

LO-W01-02 · W01-C02

Mặc định width chỉ content; padding và border tăng kích thước thực. box-sizing:border-box gộp padding/border trong width nên form dễ vừa container. Margin nằm ngoài box. Flexbox bố trí theo một trục chính; flex-wrap cho hàng tự xuống dòng. Grid bố trí hai chiều; repeat(auto-fit,minmax(240px,1fr)) tạo cột co giãn cho dashboard. Cột hẹp có thể minmax(0,1fr) để content dài không ép grid tràn. Không dùng absolute positioning cho toàn trang vì phần tử thoát normal flow. Cascade xét origin/importance/layer rồi specificity và thứ tự; tránh tăng !important khi chưa xem Computed styles. Dùng class cho thành phần và token CSS :root cho màu/spacing; dark/light sau này thay token.

<a id="lo-w01-03"></a>

## Bài 3: Responsive và accessibility

LO-W01-03 · W01-C03

Responsive bắt đầu từ nội dung: viewport meta đặt width=device-width,initial-scale=1. Dùng max-width cho container, gap linh hoạt, media query ở điểm nội dung khó đọc thay vì học thuộc một kích thước điện thoại. Bảng rộng có vùng overflow-x:auto riêng, không làm toàn body cuộn ngang. Không tắt zoom. Focus-visible có outline tương phản; lỗi form có text, không chỉ màu đỏ. Font relative và zoom 200% phải còn đọc/thao tác được. Ảnh dùng max-width:100%; không đặt width cứng cho input. Kiểm tra layout trên browser thực, thu screenshot từng width và ghi lỗi; công cụ tự động hỗ trợ nhưng không thay thao tác bàn phím.

## Ví dụ giải từng bước

1. Bài toán: form thêm khoản chi có số tiền, ngày và ghi chú, dashboard 2 card.
2. Chọn semantic form và grid; chưa xử lý lưu dữ liệu.
3. Tạo index.html và style.css theo setup:
```html
<meta name="viewport" content="width=device-width, initial-scale=1">
<main><h1>Sổ chi tiêu</h1>
<form id="expense-form">
<label for="amount">Số tiền VND</label>
<input id="amount" name="amountVnd" type="number" min="1" max="1000000000" step="1" required>
<label for="spent">Ngày chi</label>
<input id="spent" name="spentOn" type="date" required>
<button type="submit">Thêm khoản chi</button>
</form><p id="message" aria-live="polite"></p>
<section class="cards" aria-label="Tổng quan"><h2>Tổng tháng</h2><p>100.000 ₫</p></section>
</main>
```
```css
* { box-sizing: border-box; }
main { max-width: 64rem; margin: auto; padding: 1rem; }
form { display: flex; flex-wrap: wrap; gap: .75rem; }
.cards { display: grid; grid-template-columns: repeat(auto-fit,minmax(15rem,1fr)); gap: 1rem; }
:focus-visible { outline: 3px solid #2458a6; outline-offset: 3px; }
```
4. Expected: click label đưa focus vào input; Tab đến submit; browser chặn rỗng; ở 320px form xuống hàng. Thử width 100% + padding trước/sau border-box để thấy khác biệt.

## Đào sâu và lỗi thường gặp

Lỗi: placeholder không thay label; div onclick thiếu keyboard semantics; display:none trên error làm screen reader không nhận khi chưa hiện lại đúng lúc. Inspect accessibility tree để kiểm tra tên accessible. So sánh flex row và grid hai cột bằng cùng fixture; layout không được thay thứ tự đọc chỉ để trông đẹp.

## Bài tập thực hành

### Bài tập 1

Tạo bảng 3 khoản fixture. Thử tên danh mục dài 80 ký tự và zoom 200%.

### Bài tập 2

Form có nút Xóa và Thêm. Enter đang ở amount phải làm gì?

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Dùng table/caption/thead/tbody và th scope; bọc div overflow-x:auto. Container max-width, word wrapping cho note. Expected tổng body không tràn, bảng vẫn đọc và cuộn riêng.

### Hướng giải 2

Đặt Xóa type=button, Thêm type=submit. Enter kích hoạt submit; kiểm tra required/min. Dùng Tab để focus được cả hai, không xóa ngẫu nhiên.

## Project task

Tạo UI tĩnh với fixture Alice, form, table và tổng. Viết README chạy HTTP server và commit ui-static. Chưa dùng framework để che lỗi HTML/CSS.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w01.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] UI fixture hiện 3 dòng và tổng 100000 VND.

- [ ] Ở 320/768/1280px và zoom 200%, không mất input/nút; bảng cuộn riêng nếu cần.

- [ ] Label click focus đúng; Tab/Enter sử dụng được form; nút xóa không submit.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Đọc docs label/box model, ghi semantic element, focus, overflow, layout, validation. Thu: 'How does this form remain usable on a small screen?'

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Associating a label with a form control](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/label) — W01-C01; primary. HTTP: 200; section xem ledger.

- [Parts of the CSS box model](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_box_model/Introduction_to_the_CSS_box_model) — W01-C02; primary. HTTP: 200; section xem ledger.

- [Responsive design](https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/CSS_layout/Responsive_Design) — W01-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-01-quiz.md) · [Đáp án](../../assessments/answer-keys/week-01-answers.md) · [Flashcards](../../flashcards/week-01.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần tiếp](week-02.md)
