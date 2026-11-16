# Đáp án tuần 09 — SQL schema, constraints và normalization

[Quiz](../quizzes/week-09-quiz.md) · [Bài nguồn](../../notes/module-3/week-09.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q09-01 | LO-W09-01 | Analyze | Owner FK | B | W09-C01 |
| Q09-02 | LO-W09-01 | Analyze | Data type | C | W09-C01 |
| Q09-03 | LO-W09-02 | Analyze | Normalization | D | W09-C02 |
| Q09-04 | LO-W09-02 | Analyze | Business unique | A | W09-C02 |
| Q09-05 | LO-W09-03 | Analyze | Check null | B | W09-C03 |
| Q09-06 | LO-W09-03 | Analyze | Lab FK test | C | W09-C03 |



## Giải thích từng phương án

### Q09-01

CategoryId FK đơn lẻ bảo đảm cùng owner không?

- **Đáp án đúng:** **B** — Không

- **Phương án B ĐÚNG:** Nó chỉ bảo tồn tồn tại; cần owner constraint/query.

- **A SAI:** Tên không đủ định danh ownership.

- **C SAI:** PK không liên kết owner của expense.

- **D SAI:** Client filter không bảo vệ direct writes.

---

Vị trí bài học: [LO-W09-01](../../notes/module-3/week-09.md#lo-w09-01)

Ôn [bài 1](../../notes/module-3/week-09.md) · LO-W09-01 · W09-C01.

### Q09-02

Số tiền nguyên theo spec SQL nên lưu gì?

- **Đáp án đúng:** **C** — bigint NOT NULL có CHECK

- **Phương án C ĐÚNG:** Numeric nguyên hỗ trợ SUM và domain guard.

- **A SAI:** Float có representation không chính xác cho nhiều decimals.

- **B SAI:** Date biểu diễn ngày, không tiền.

- **D SAI:** Text formatted khó tính và validate.

---

Vị trí bài học: [LO-W09-01](../../notes/module-3/week-09.md#lo-w09-01)

Ôn [bài 1](../../notes/module-3/week-09.md) · LO-W09-01 · W09-C01.

### Q09-03

CategoryName current lặp trong expenses nguy cơ gì?

- **Đáp án đúng:** **D** — Update anomaly khi đổi tên không đồng bộ

- **Phương án D ĐÚNG:** Nhiều bản tên cần cập nhật gây inconsistency.

- **A SAI:** Performance phải đo workload.

- **B SAI:** Duplicate name không tạo relational constraint.

- **C SAI:** Vẫn insert được nhưng dễ sai semantics.

---

Vị trí bài học: [LO-W09-02](../../notes/module-3/week-09.md#lo-w09-02)

Ôn [bài 2](../../notes/module-3/week-09.md) · LO-W09-02 · W09-C02.

### Q09-04

UNIQUE(UserId,Name) cho categories nhằm gì?

- **Đáp án đúng:** **A** — Cùng user không trùng tên, khác user có thể trùng

- **Phương án A ĐÚNG:** Uniqueness xét tổ hợp columns.

- **B SAI:** Không cấm same Name khi UserId khác.

- **C SAI:** Surrogate PK vẫn định danh rows.

- **D SAI:** Category uniqueness không tiền expense.

---

Vị trí bài học: [LO-W09-02](../../notes/module-3/week-09.md#lo-w09-02)

Ôn [bài 2](../../notes/module-3/week-09.md) · LO-W09-02 · W09-C02.

### Q09-05

CHECK(amount>0) có đủ cấm NULL không?

- **Đáp án đúng:** **B** — Không, cần NOT NULL

- **Phương án B ĐÚNG:** SQL three-valued logic CHECK có thể chấp nhận UNKNOWN.

- **A SAI:** CSS/client không DB constraint.

- **C SAI:** NULL không tự chuyển0.

- **D SAI:** UNKNOWN không đồng nhất FALSE trong CHECK.

---

Vị trí bài học: [LO-W09-03](../../notes/module-3/week-09.md#lo-w09-03)

Ôn [bài 3](../../notes/module-3/week-09.md) · LO-W09-03 · W09-C03.

### Q09-06

Temp demo đã đủ chứng minh FK SQL Server chưa?

- **Đáp án đúng:** **C** — Chưa, phải kiểm persistent lab tables

- **Phương án C ĐÚNG:** SQL Server temp không enforce FK như bảng persistent.

- **A SAI:** Direct DB rejection là evidence cần.

- **B SAI:** Diagram thể hiện ý định, không behavior.

- **D SAI:** Đó là giả định sai cho SQL Server.

---

Vị trí bài học: [LO-W09-03](../../notes/module-3/week-09.md#lo-w09-03)

Ôn [bài 3](../../notes/module-3/week-09.md) · LO-W09-03 · W09-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
