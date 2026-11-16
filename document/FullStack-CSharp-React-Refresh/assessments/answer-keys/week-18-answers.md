# Đáp án tuần 18 — Hooks, routing và API lifecycle

[Quiz](../quizzes/week-18-quiz.md) · [Bài nguồn](../../notes/module-5/week-18.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q18-01 | LO-W18-01 | Analyze | Effect boundary | C | W18-C01 |
| Q18-02 | LO-W18-01 | Analyze | Cleanup | D | W18-C01 |
| Q18-03 | LO-W18-02 | Analyze | Deep links | A | W18-C02 |
| Q18-04 | LO-W18-02 | Analyze | Route guard | B | W18-C02 |
| Q18-05 | LO-W18-03 | Analyze | Fetch status | C | W18-C03 |
| Q18-06 | LO-W18-03 | Analyze | 204 parsing | D | W18-C03 |



## Giải thích từng phương án

### Q18-01

POST do user bấm submit nên đặt đâu?

- **Đáp án đúng:** **C** — Trong event handler

- **Phương án C ĐÚNG:** Mutation gắn với intent submit.

- **A SAI:** Effect lặp có thể tạo expense trùng.

- **B SAI:** Startup không biết dữ liệu form/user.

- **D SAI:** Render phải không gây side effects.

---

Vị trí bài học: [LO-W18-01](../../notes/module-5/week-18.md#lo-w18-01)

Ôn [bài 1](../../notes/module-5/week-18.md) · LO-W18-01 · W18-C01.

### Q18-02

Effect fetch trả cleanup để làm gì?

- **Đáp án đúng:** **D** — Abort và ignore kết quả cũ

- **Phương án D ĐÚNG:** Cleanup bảo vệ UI khi đổi dependency.

- **A SAI:** Commit server có lifecycle riêng.

- **B SAI:** Type checking không liên quan cleanup.

- **C SAI:** Không hủy làm lãng phí và dễ race.

---

Vị trí bài học: [LO-W18-01](../../notes/module-5/week-18.md#lo-w18-01)

Ôn [bài 1](../../notes/module-5/week-18.md) · LO-W18-01 · W18-C01.

### Q18-03

SPA refresh route cần gì ở host?

- **Đáp án đúng:** **A** — Fallback index.html cho UI routes

- **Phương án A ĐÚNG:** Host phải phục vụ entry cho route UI.

- **B SAI:** API cần JSON/status đúng backend.

- **C SAI:** Deep links là yêu cầu navigation.

- **D SAI:** JWT không lưu browser route cần thiết.

---

Vị trí bài học: [LO-W18-02](../../notes/module-5/week-18.md#lo-w18-02)

Ôn [bài 2](../../notes/module-5/week-18.md) · LO-W18-02 · W18-C02.

### Q18-04

Guard client thay auth API không?

- **Đáp án đúng:** **B** — Không, API tự kiểm quyền

- **Phương án B ĐÚNG:** Client bị sửa và API gọi trực tiếp được.

- **A SAI:** Khó đoán không enforce authorization.

- **C SAI:** UI visibility không security boundary.

- **D SAI:** Router không validate bearer ở server.

---

Vị trí bài học: [LO-W18-02](../../notes/module-5/week-18.md#lo-w18-02)

Ôn [bài 2](../../notes/module-5/week-18.md) · LO-W18-02 · W18-C02.

### Q18-05

fetch gặp HTTP500 thường làm gì?

- **Đáp án đúng:** **C** — Resolve Response cần kiểm ok

- **Phương án C ĐÚNG:** HTTP lỗi vẫn là response network thành công.

- **A SAI:** Body parsing do ứng dụng xử lý.

- **B SAI:** Fetch không tự thực hiện retry policy.

- **D SAI:** Reject thường do network/abort.

---

Vị trí bài học: [LO-W18-03](../../notes/module-5/week-18.md#lo-w18-03)

Ôn [bài 3](../../notes/module-5/week-18.md) · LO-W18-03 · W18-C03.

### Q18-06

DELETE204 nên xử lý body thế nào?

- **Đáp án đúng:** **D** — Không gọi json, trả undefined

- **Phương án D ĐÚNG:** 204 biểu thị không có body.

- **A SAI:** JSON parser lỗi vì input rỗng.

- **B SAI:** Success204 không phải server failure.

- **C SAI:** Body {} vi phạm semantics204.

---

Vị trí bài học: [LO-W18-03](../../notes/module-5/week-18.md#lo-w18-03)

Ôn [bài 3](../../notes/module-5/week-18.md) · LO-W18-03 · W18-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
