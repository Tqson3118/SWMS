# Đáp án tuần 17 — TypeScript, components và state

[Quiz](../quizzes/week-17-quiz.md) · [Bài nguồn](../../notes/module-5/week-17.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q17-01 | LO-W17-01 | Analyze | Runtime types | B | W17-C01 |
| Q17-02 | LO-W17-01 | Analyze | Unknown | C | W17-C01 |
| Q17-03 | LO-W17-02 | Analyze | List identity | D | W17-C02 |
| Q17-04 | LO-W17-02 | Analyze | Props | A | W17-C02 |
| Q17-05 | LO-W17-03 | Analyze | Derived state | B | W17-C03 |
| Q17-06 | LO-W17-03 | Analyze | Functional updater | C | W17-C03 |



## Giải thích từng phương án

### Q17-01

JSON cast bằng as ExpenseDto bảo đảm gì?

- **Đáp án đúng:** **B** — Chỉ thay cách compiler kiểm kiểu

- **Phương án B ĐÚNG:** Assertion không tạo runtime checks.

- **A SAI:** String ngày không được tự chuyển timezone.

- **C SAI:** JSON shape có thể không đúng kiểu.

- **D SAI:** Cast không thực hiện network request.

---

Vị trí bài học: [LO-W17-01](../../notes/module-5/week-17.md#lo-w17-01)

Ôn [bài 1](../../notes/module-5/week-17.md) · LO-W17-01 · W17-C01.

### Q17-02

Dữ liệu chưa tin cậy nên nhận kiểu gì?

- **Đáp án đúng:** **C** — unknown rồi narrow

- **Phương án C ĐÚNG:** Unknown yêu cầu kiểm trước sử dụng.

- **A SAI:** Never mô tả giá trị không xảy ra.

- **B SAI:** Annotation không chứng minh external data.

- **D SAI:** Any tắt kiểm tra tại boundary.

---

Vị trí bài học: [LO-W17-01](../../notes/module-5/week-17.md#lo-w17-01)

Ôn [bài 1](../../notes/module-5/week-17.md) · LO-W17-01 · W17-C01.

### Q17-03

Key nào giữ identity khi sort?

- **Đáp án đúng:** **D** — ID ổn định của expense

- **Phương án D ĐÚNG:** ID theo resource không theo vị trí.

- **A SAI:** Index gắn state với vị trí sai.

- **B SAI:** Random làm remount toàn bộ rows.

- **C SAI:** Amount không duy nhất và có thể sửa.

---

Vị trí bài học: [LO-W17-02](../../notes/module-5/week-17.md#lo-w17-02)

Ôn [bài 2](../../notes/module-5/week-17.md) · LO-W17-02 · W17-C02.

### Q17-04

Child muốn xóa row nên làm gì?

- **Đáp án đúng:** **A** — Gọi callback từ parent

- **Phương án A ĐÚNG:** Owner state nhận intent rồi cập nhật.

- **B SAI:** Mutation phá luồng cập nhật React.

- **C SAI:** DOM riêng lệch với state và render.

- **D SAI:** Props do parent cấp, child không sở hữu.

---

Vị trí bài học: [LO-W17-02](../../notes/module-5/week-17.md#lo-w17-02)

Ôn [bài 2](../../notes/module-5/week-17.md) · LO-W17-02 · W17-C02.

### Q17-05

Total nên lấy từ đâu khi rows đủ dữ liệu?

- **Đáp án đúng:** **B** — Derive từ rows bằng reduce

- **Phương án B ĐÚNG:** Một nguồn dữ liệu giảm lỗi đồng bộ.

- **A SAI:** DOM là output, không nguồn business data.

- **C SAI:** State trùng dễ quên cập nhật.

- **D SAI:** Global không theo lifecycle component.

---

Vị trí bài học: [LO-W17-03](../../notes/module-5/week-17.md#lo-w17-03)

Ôn [bài 3](../../notes/module-5/week-17.md) · LO-W17-03 · W17-C03.

### Q17-06

Xóa dựa trên rows trước dùng cách nào?

- **Đáp án đúng:** **C** — setRows(old => old.filter(...))

- **Phương án C ĐÚNG:** Updater dùng previous state và array mới.

- **A SAI:** Push trả length, không trả array rows.

- **B SAI:** Set vô điều kiện trong render gây vòng lặp.

- **D SAI:** Mutation không yêu cầu render mới đúng cách.

---

Vị trí bài học: [LO-W17-03](../../notes/module-5/week-17.md#lo-w17-03)

Ôn [bài 3](../../notes/module-5/week-17.md) · LO-W17-03 · W17-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
