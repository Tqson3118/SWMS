# Đáp án tuần 12 — Transactions, concurrency và data correctness

[Quiz](../quizzes/week-12-quiz.md) · [Bài nguồn](../../notes/module-3/week-12.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q12-01 | LO-W12-01 | Analyze | Atomicity | A | W12-C01 |
| Q12-02 | LO-W12-01 | Analyze | External boundary | B | W12-C01 |
| Q12-03 | LO-W12-02 | Analyze | Rowversion semantics | C | W12-C02 |
| Q12-04 | LO-W12-02 | Analyze | Lost update guard | D | W12-C02 |
| Q12-05 | LO-W12-03 | Analyze | Idempotency hash | A | W12-C03 |
| Q12-06 | LO-W12-03 | Analyze | Exists race | B | W12-C03 |



## Giải thích từng phương án

### Q12-01

2writes nghiệp vụ cần all-or-nothing thì làm gì?

- **Đáp án đúng:** **A** — Cùng transaction boundary

- **Phương án A ĐÚNG:** Transaction chung cho phép rollback unit.

- **B SAI:** Writes độc lập có partial commit.

- **C SAI:** Client catch không undo DB writes.

- **D SAI:** Disable chỉ UX không atomicity.

---

Vị trí bài học: [LO-W12-01](../../notes/module-3/week-12.md#lo-w12-01)

Ôn [bài 1](../../notes/module-3/week-12.md) · LO-W12-01 · W12-C01.

### Q12-02

SQL transaction có tự rollback email đã gửi không?

- **Đáp án đúng:** **B** — Không

- **Phương án B ĐÚNG:** DB transaction chỉ bảo vệ resource tham gia.

- **A SAI:** Timestamp không tạo transaction.

- **C SAI:** External email ngoài DB không chung transaction tự động.

- **D SAI:** Async scheduling không distributed atomicity.

---

Vị trí bài học: [LO-W12-01](../../notes/module-3/week-12.md#lo-w12-01)

Ôn [bài 1](../../notes/module-3/week-12.md) · LO-W12-01 · W12-C01.

### Q12-03

SQL Server rowversion là gì?

- **Đáp án đúng:** **C** — Binary concurrency version, không thời gian

- **Phương án C ĐÚNG:** Token hỗ trợ detect stale write.

- **A SAI:** Auth token và row token khác mục đích.

- **B SAI:** Không numeric expense amount.

- **D SAI:** Tên cũ timestamp không phải time semantics.

---

Vị trí bài học: [LO-W12-02](../../notes/module-3/week-12.md#lo-w12-02)

Ôn [bài 2](../../notes/module-3/week-12.md) · LO-W12-02 · W12-C02.

### Q12-04

UPDATE với oldversion mismatch có affectedrows gì?

- **Đáp án đúng:** **D** — 0

- **Phương án D ĐÚNG:** Predicate version không match nên không write.

- **A SAI:** Có thể0 dù row tồn tại.

- **B SAI:** DB không suy ra merge semantics.

- **C SAI:** SQL rowcount không HTTP response.

---

Vị trí bài học: [LO-W12-02](../../notes/module-3/week-12.md#lo-w12-02)

Ôn [bài 2](../../notes/module-3/week-12.md) · LO-W12-02 · W12-C02.

### Q12-05

Same key/different payload nên xử lý gì?

- **Đáp án đúng:** **A** — 409 conflict

- **Phương án A ĐÚNG:** Key đã đại diện operation khác nên conflict.

- **B SAI:** Làm lại có duplicate side effect.

- **C SAI:** Silent overwrite mất contract key.

- **D SAI:** Owner phải từ principal, không body.

---

Vị trí bài học: [LO-W12-03](../../notes/module-3/week-12.md#lo-w12-03)

Ôn [bài 3](../../notes/module-3/week-12.md) · LO-W12-03 · W12-C03.

### Q12-06

Check exists trước insert đã đủ chống concurrent duplicate chưa?

- **Đáp án đúng:** **B** — Chưa, cần unique/atomicity

- **Phương án B ĐÚNG:** Hai transaction có thể cùng thấy missing.

- **A SAI:** Any check không khóa future insert.

- **C SAI:** Server xử concurrent requests.

- **D SAI:** Log không synchronization.

---

Vị trí bài học: [LO-W12-03](../../notes/module-3/week-12.md#lo-w12-03)

Ôn [bài 3](../../notes/module-3/week-12.md) · LO-W12-03 · W12-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
