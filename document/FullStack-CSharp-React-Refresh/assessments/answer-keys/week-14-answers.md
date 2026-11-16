# Đáp án tuần 14 — EF Core mapping, migrations và validation

[Quiz](../quizzes/week-14-quiz.md) · [Bài nguồn](../../notes/module-4/week-14.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q14-01 | LO-W14-01 | Analyze | DbContext | C | W14-C01 |
| Q14-02 | LO-W14-01 | Analyze | Tenant filter | D | W14-C01 |
| Q14-03 | LO-W14-02 | Analyze | Rename migration | A | W14-C02 |
| Q14-04 | LO-W14-02 | Analyze | EF versions | B | W14-C02 |
| Q14-05 | LO-W14-03 | Analyze | Validation | C | W14-C03 |
| Q14-06 | LO-W14-03 | Analyze | Stale version | D | W14-C03 |



## Giải thích từng phương án

### Q14-01

2parallel queries dùng cùng DbContext có an toàn không?

- **Đáp án đúng:** **C** — Không

- **Phương án C ĐÚNG:** Context không thread-safe, cần await sequential/separate unit.

- **A SAI:** No tracking không đổi context concurrency rule.

- **B SAI:** Singleton context làm lifetime sai thêm.

- **D SAI:** Async không bảo vệ shared state.

---

Vị trí bài học: [LO-W14-01](../../notes/module-4/week-14.md#lo-w14-01)

Ôn [bài 1](../../notes/module-4/week-14.md) · LO-W14-01 · W14-C01.

### Q14-02

Owner filter ở đâu?

- **Đáp án đúng:** **D** — IQueryable trước ToList

- **Phương án D ĐÚNG:** DB chỉ trả rows scoped.

- **A SAI:** Materialize toànDB gây privacy/performance risk.

- **B SAI:** Frontend dễ bypass.

- **C SAI:** Field không filtering.

---

Vị trí bài học: [LO-W14-01](../../notes/module-4/week-14.md#lo-w14-01)

Ôn [bài 1](../../notes/module-4/week-14.md) · LO-W14-01 · W14-C01.

### Q14-03

Generated drop/add do đổi tên nên làm gì?

- **Đáp án đúng:** **A** — Review rename và test clone có data

- **Phương án A ĐÚNG:** Generator không hiểu mọi rename intent.

- **B SAI:** Generated không luôn bảo toàn data.

- **C SAI:** Xóa data trái migration mục tiêu.

- **D SAI:** History cần reproducible upgrades.

---

Vị trí bài học: [LO-W14-02](../../notes/module-4/week-14.md#lo-w14-02)

Ôn [bài 2](../../notes/module-4/week-14.md) · LO-W14-02 · W14-C02.

### Q14-04

Provider/Design cho EF10 nên major nào?

- **Đáp án đúng:** **B** — 10 cùng compatible set

- **Phương án B ĐÚNG:** API/tool major phải tương thích.

- **A SAI:** Restore/build xác nhận môi trường.

- **C SAI:** Oldprovider random gây mismatch.

- **D SAI:** Design1 không tool mọi EF.

---

Vị trí bài học: [LO-W14-02](../../notes/module-4/week-14.md#lo-w14-02)

Ôn [bài 2](../../notes/module-4/week-14.md) · LO-W14-02 · W14-C02.

### Q14-05

HTML required thay DB/domainguard không?

- **Đáp án đúng:** **C** — Không

- **Phương án C ĐÚNG:** Other caller/directwrite vẫn invalid.

- **A SAI:** HTML bypass được.

- **B SAI:** Token trust không business validation.

- **D SAI:** TS compile không externalinputvalidation.

---

Vị trí bài học: [LO-W14-03](../../notes/module-4/week-14.md#lo-w14-03)

Ôn [bài 3](../../notes/module-4/week-14.md) · LO-W14-03 · W14-C03.

### Q14-06

Loadlatest rồi bỏ version editor có detect stale intent không?

- **Đáp án đúng:** **D** — Không

- **Phương án D ĐÚNG:** Original editor version phải tham gia predicate.

- **A SAI:** EF không biết lịch sử editor tự động.

- **B SAI:** IDkhác concurrency token.

- **C SAI:** Clientclock không DBtoken.

---

Vị trí bài học: [LO-W14-03](../../notes/module-4/week-14.md#lo-w14-03)

Ôn [bài 3](../../notes/module-4/week-14.md) · LO-W14-03 · W14-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
