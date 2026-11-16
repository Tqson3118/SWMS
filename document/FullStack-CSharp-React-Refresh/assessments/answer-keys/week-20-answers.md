# Đáp án tuần 20 — Dashboard, accessibility và frontend tests

[Quiz](../quizzes/week-20-quiz.md) · [Bài nguồn](../../notes/module-5/week-20.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q20-01 | LO-W20-01 | Analyze | Report source | A | W20-C01 |
| Q20-02 | LO-W20-01 | Analyze | Zero budget | B | W20-C01 |
| Q20-03 | LO-W20-02 | Analyze | Reducer role | C | W20-C02 |
| Q20-04 | LO-W20-02 | Analyze | State choice | D | W20-C02 |
| Q20-05 | LO-W20-03 | Analyze | UI tests | A | W20-C03 |
| Q20-06 | LO-W20-03 | Analyze | Accessibility evidence | B | W20-C03 |



## Giải thích từng phương án

### Q20-01

Monthly total lấy từ đâu?

- **Đáp án đúng:** **A** — API report cho toàn tháng

- **Phương án A ĐÚNG:** Pagination không bao hết tháng.

- **B SAI:** Page subtotal thiếu rows khác.

- **C SAI:** DOM là output của dữ liệu.

- **D SAI:** Một expense không đại diện total.

---

Vị trí bài học: [LO-W20-01](../../notes/module-5/week-20.md#lo-w20-01)

Ôn [bài 1](../../notes/module-5/week-20.md) · LO-W20-01 · W20-C01.

### Q20-02

Budget0 xử lý percentage thế nào?

- **Đáp án đúng:** **B** — Không chia0; label và percent null

- **Phương án B ĐÚNG:** Giữ contract rõ và tránh giá trị invalid.

- **A SAI:** Chi tiêu vẫn cần hiển thị.

- **C SAI:** Infinity không meaningful cho người dùng.

- **D SAI:** Thay limit làm sai dữ liệu user.

---

Vị trí bài học: [LO-W20-01](../../notes/module-5/week-20.md#lo-w20-01)

Ôn [bài 1](../../notes/module-5/week-20.md) · LO-W20-01 · W20-C01.

### Q20-03

Reducer nên có đặc tính gì?

- **Đáp án đúng:** **C** — Pure state/action → next state

- **Phương án C ĐÚNG:** Pure transition dễ kiểm và tái hiện.

- **A SAI:** Global mutation phá predictability.

- **B SAI:** Frontend reducer không gọi DB trực tiếp.

- **D SAI:** IO đặt event/effect/service boundary.

---

Vị trí bài học: [LO-W20-02](../../notes/module-5/week-20.md#lo-w20-02)

Ôn [bài 2](../../notes/module-5/week-20.md) · LO-W20-02 · W20-C02.

### Q20-04

Khi nào thêm global state library?

- **Đáp án đúng:** **D** — Khi nhu cầu chia sẻ/transition có bằng chứng

- **Phương án D ĐÚNG:** ADR cân nhắc complexity so benefit.

- **A SAI:** React không yêu cầu library global.

- **B SAI:** UI state không bảo vệ server.

- **C SAI:** Props là nền tảng data flow.

---

Vị trí bài học: [LO-W20-02](../../notes/module-5/week-20.md#lo-w20-02)

Ôn [bài 2](../../notes/module-5/week-20.md) · LO-W20-02 · W20-C02.

### Q20-05

Query nào gần cách user tìm nút?

- **Đáp án đúng:** **A** — getByRole('button',{name:'Retry'})

- **Phương án A ĐÚNG:** Role/name kiểm UI có thể truy cập.

- **B SAI:** Class generated dễ thay không đổi behavior.

- **C SAI:** Implementation detail không user contract.

- **D SAI:** Snapshot rộng khó đọc regression có ý nghĩa.

---

Vị trí bài học: [LO-W20-03](../../notes/module-5/week-20.md#lo-w20-03)

Ôn [bài 3](../../notes/module-5/week-20.md) · LO-W20-03 · W20-C03.

### Q20-06

Test tự động pass có chứng minh mọi a11y?

- **Đáp án đúng:** **B** — Không, cần keyboard/focus/manual checks

- **Phương án B ĐÚNG:** Tool không phát hiện mọi usability issue.

- **A SAI:** Visible/accessible labels cần cho forms.

- **C SAI:** Coverage tool có giới hạn thực tế.

- **D SAI:** Màu đẹp không đủ contrast/semantics.

---

Vị trí bài học: [LO-W20-03](../../notes/module-5/week-20.md#lo-w20-03)

Ôn [bài 3](../../notes/module-5/week-20.md) · LO-W20-03 · W20-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
