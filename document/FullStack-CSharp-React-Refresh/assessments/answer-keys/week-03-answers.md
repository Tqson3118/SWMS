# Đáp án tuần 03 — DOM, sự kiện, modules và persistence nhỏ

[Quiz](../quizzes/week-03-quiz.md) · [Bài nguồn](../../notes/module-1/week-03.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q03-01 | LO-W03-01 | Analyze | Default behavior | D | W03-C01 |
| Q03-02 | LO-W03-01 | Analyze | Safe text | A | W03-C01 |
| Q03-03 | LO-W03-02 | Analyze | Module import | B | W03-C02 |
| Q03-04 | LO-W03-02 | Analyze | Module hosting | C | W03-C02 |
| Q03-05 | LO-W03-03 | Analyze | JSON validation | D | W03-C03 |
| Q03-06 | LO-W03-03 | Analyze | Storage security | A | W03-C03 |



## Giải thích từng phương án

### Q03-01

preventDefault trên submit chặn gì?

- **Đáp án đúng:** **D** — Hành vi mặc định gửi/reload

- **Phương án D ĐÚNG:** Nó hủy default nếu event cancelable.

- **A SAI:** stopPropagation mới kiểm soát propagation.

- **B SAI:** Validation là luật riêng, không được bảo đảm bởi preventDefault.

- **C SAI:** Không phải firewall của ứng dụng.

---

Vị trí bài học: [LO-W03-01](../../notes/module-1/week-03.md#lo-w03-01)

Ôn [bài 1](../../notes/module-1/week-03.md) · LO-W03-01 · W03-C01.

### Q03-02

Render note chưa tin cậy nên dùng gì?

- **Đáp án đúng:** **A** — textContent

- **Phương án A ĐÚNG:** Text không diễn giải note thành HTML.

- **B SAI:** innerHTML có thể đưa markup/script nguy hiểm vào DOM.

- **C SAI:** eval thực thi input, nguy hiểm.

- **D SAI:** document.write diễn giải HTML và có side effects lớn.

---

Vị trí bài học: [LO-W03-01](../../notes/module-1/week-03.md#lo-w03-01)

Ôn [bài 1](../../notes/module-1/week-03.md) · LO-W03-01 · W03-C01.

### Q03-03

Named export total import đúng thế nào?

- **Đáp án đúng:** **B** — import {total} from './money.js'

- **Phương án B ĐÚNG:** Braces đọc named binding.

- **A SAI:** Module scope không gắn window.

- **C SAI:** Default import cần default export.

- **D SAI:** require không API ES module browser tự có.

---

Vị trí bài học: [LO-W03-02](../../notes/module-1/week-03.md#lo-w03-02)

Ôn [bài 2](../../notes/module-1/week-03.md) · LO-W03-02 · W03-C02.

### Q03-04

Tại sao dùng HTTP server local cho module?

- **Đáp án đúng:** **C** — Để origin/MIME/path hoạt động như web

- **Phương án C ĐÚNG:** Browser module tải theo cơ chế web/origin.

- **A SAI:** Import vẫn là boundary cần khai báo.

- **B SAI:** Module vẫn strict mode.

- **D SAI:** Server static không tự sinh API nghiệp vụ.

---

Vị trí bài học: [LO-W03-02](../../notes/module-1/week-03.md#lo-w03-02)

Ôn [bài 2](../../notes/module-1/week-03.md) · LO-W03-02 · W03-C02.

### Q03-05

JSON.parse('{}') thành công có đủ nhận expenses không?

- **Đáp án đúng:** **D** — Không, phải validate shape

- **Phương án D ĐÚNG:** Syntax JSON không đảm bảo array/schema.

- **A SAI:** Object không phải array.

- **B SAI:** Browser không biết domain expense.

- **C SAI:** Đổi key không sửa dữ liệu sai.

---

Vị trí bài học: [LO-W03-03](../../notes/module-1/week-03.md#lo-w03-03)

Ôn [bài 3](../../notes/module-1/week-03.md) · LO-W03-03 · W03-C03.

### Q03-06

localStorage phù hợp lab này cho gì?

- **Đáp án đúng:** **A** — Expense giả lập đã validate

- **Phương án A ĐÚNG:** Dữ liệu lab không nhạy cảm, có recovery.

- **B SAI:** Signing key không được đưa browser.

- **C SAI:** Storage đọc được bởi JS/XSS, không mặc định an toàn.

- **D SAI:** Password plaintext không được lưu.

---

Vị trí bài học: [LO-W03-03](../../notes/module-1/week-03.md#lo-w03-03)

Ôn [bài 3](../../notes/module-1/week-03.md) · LO-W03-03 · W03-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
