# Đáp án tuần 01 — HTML/CSS và giao diện có thể sử dụng

[Quiz](../quizzes/week-01-quiz.md) · [Bài nguồn](../../notes/module-1/week-01.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q01-01 | LO-W01-01 | Analyze | Label association | B | W01-C01 |
| Q01-02 | LO-W01-01 | Analyze | Button type | C | W01-C01 |
| Q01-03 | LO-W01-02 | Analyze | Border box | D | W01-C02 |
| Q01-04 | LO-W01-02 | Analyze | Layout axes | A | W01-C02 |
| Q01-05 | LO-W01-03 | Analyze | Responsive testing | B | W01-C03 |
| Q01-06 | LO-W01-03 | Analyze | Accessible error | C | W01-C03 |



## Giải thích từng phương án

### Q01-01

Label for='amount' liên kết với gì?

- **Đáp án đúng:** **B** — Input id='amount'

- **Phương án B ĐÚNG:** for khớp id tạo association/focus.

- **A SAI:** Placeholder biến mất và không liên kết focus.

- **C SAI:** name là key dữ liệu, không association label.

- **D SAI:** class chọn CSS, không định danh association.

---

Vị trí bài học: [LO-W01-01](../../notes/module-1/week-01.md#lo-w01-01)

Ôn [bài 1](../../notes/module-1/week-01.md) · LO-W01-01 · W01-C01.

### Q01-02

Nút xóa trong form nên dùng type gì?

- **Đáp án đúng:** **C** — button

- **Phương án C ĐÚNG:** button không submit mặc định.

- **A SAI:** reset reset mọi input thay vì xóa expense.

- **B SAI:** Không type có thể submit ngoài ý muốn.

- **D SAI:** submit gửi form, không chỉ xóa dòng.

---

Vị trí bài học: [LO-W01-01](../../notes/module-1/week-01.md#lo-w01-01)

Ôn [bài 1](../../notes/module-1/week-01.md) · LO-W01-01 · W01-C01.

### Q01-03

width=200px, padding 10px mỗi bên, border 1px mỗi bên; border-box cho outer box bao nhiêu?

- **Đáp án đúng:** **D** — 200px

- **Phương án D ĐÚNG:** width gồm content/padding/border, chưa tính margin.

- **A SAI:** 222 là content-box tổng kích thước.

- **B SAI:** 180 không tính border chính xác và nhầm outer với content.

- **C SAI:** 202 cộng border hai lần.

---

Vị trí bài học: [LO-W01-02](../../notes/module-1/week-01.md#lo-w01-02)

Ôn [bài 2](../../notes/module-1/week-01.md) · LO-W01-02 · W01-C02.

### Q01-04

Layout dashboard theo hàng và cột nên thử gì trước?

- **Đáp án đúng:** **A** — CSS Grid

- **Phương án A ĐÚNG:** Grid biểu diễn hai chiều trực tiếp.

- **B SAI:** Absolute thoát flow và khó responsive.

- **C SAI:** Float dành wrap content, không phải lựa chọn dashboard rõ nhất.

- **D SAI:** Margin âm sửa triệu chứng, dễ overlap.

---

Vị trí bài học: [LO-W01-02](../../notes/module-1/week-01.md#lo-w01-02)

Ôn [bài 2](../../notes/module-1/week-01.md) · LO-W01-02 · W01-C02.

### Q01-05

Bảng rộng ở 320px nên xử lý thế nào?

- **Đáp án đúng:** **B** — Cho bảng cuộn ngang trong vùng riêng

- **Phương án B ĐÚNG:** Giữ dữ liệu và giới hạn overflow trong thành phần.

- **A SAI:** Body cứng phá responsive trang.

- **C SAI:** Tắt zoom làm giảm khả năng đọc.

- **D SAI:** Ẩn dữ liệu quan trọng mất chức năng.

---

Vị trí bài học: [LO-W01-03](../../notes/module-1/week-01.md#lo-w01-03)

Ôn [bài 3](../../notes/module-1/week-01.md) · LO-W01-03 · W01-C03.

### Q01-06

Lỗi amount nên thông báo gì?

- **Đáp án đúng:** **C** — Text rõ, liên kết input và vùng thông báo

- **Phương án C ĐÚNG:** Text giúp người dùng hiểu/sửa, aria-live hỗ trợ thay đổi.

- **A SAI:** Console không phải feedback của form.

- **B SAI:** Server validation vẫn cần UI feedback.

- **D SAI:** Màu riêng không đủ cho nhiều người dùng.

---

Vị trí bài học: [LO-W01-03](../../notes/module-1/week-01.md#lo-w01-03)

Ôn [bài 3](../../notes/module-1/week-01.md) · LO-W01-03 · W01-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
