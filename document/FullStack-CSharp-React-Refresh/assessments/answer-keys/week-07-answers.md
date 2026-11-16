# Đáp án tuần 07 — LINQ, nullable và exception boundaries

[Quiz](../quizzes/week-07-quiz.md) · [Bài nguồn](../../notes/module-2/week-07.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q07-01 | LO-W07-01 | Analyze | Deferred query | D | W07-C01 |
| Q07-02 | LO-W07-01 | Analyze | Snapshot result | A | W07-C01 |
| Q07-03 | LO-W07-02 | Analyze | Nullable scope | B | W07-C02 |
| Q07-04 | LO-W07-02 | Analyze | Null guard | C | W07-C02 |
| Q07-05 | LO-W07-03 | Analyze | Stack preservation | D | W07-C03 |
| Q07-06 | LO-W07-03 | Analyze | Expected parse failure | A | W07-C03 |



## Giải thích từng phương án

### Q07-01

Tạo Where trên List có luôn chạy filter ngay không?

- **Đáp án đúng:** **D** — Thường chưa, tới enumeration

- **Phương án D ĐÚNG:** Where pipeline thường deferred.

- **A SAI:** ToList mới materialize snapshot ở ví dụ.

- **B SAI:** List LINQ không cần DB.

- **C SAI:** Enumeration không tự tạo thread.

---

Vị trí bài học: [LO-W07-01](../../notes/module-2/week-07.md#lo-w07-01)

Ôn [bài 1](../../notes/module-2/week-07.md) · LO-W07-01 · W07-C01.

### Q07-02

q deferred tạo từ50000/20000 rồi thêm30000; snapshot trước thêm tổng gì?

- **Đáp án đúng:** **A** — 50000

- **Phương án A ĐÚNG:** Snapshot đã materialize phần đạt filter trước thêm.

- **B SAI:** 80000 là q enumerate sau mutation.

- **C SAI:** 20000 không đạt filter>=30000.

- **D SAI:** Snapshot có50000 nên không0.

---

Vị trí bài học: [LO-W07-01](../../notes/module-2/week-07.md#lo-w07-01)

Ôn [bài 1](../../notes/module-2/week-07.md) · LO-W07-01 · W07-C01.

### Q07-03

NRT thay validation runtime không?

- **Đáp án đúng:** **B** — Không

- **Phương án B ĐÚNG:** NRT là annotation và compiler flow analysis.

- **A SAI:** ! chỉ suppress phân tích, không tạo object.

- **C SAI:** Input bên ngoài vẫn cần runtime guard.

- **D SAI:** Warnings không áp mọi runtime path.

---

Vị trí bài học: [LO-W07-02](../../notes/module-2/week-07.md#lo-w07-02)

Ôn [bài 2](../../notes/module-2/week-07.md) · LO-W07-02 · W07-C02.

### Q07-04

note?.Trim()??'' khi note=null trả gì?

- **Đáp án đúng:** **C** — Chuỗi rỗng

- **Phương án C ĐÚNG:** ?. cho null, ?? chọn fallback.

- **A SAI:** ?? dùng fallback khi left null.

- **B SAI:** Không tự convert thành chữ null.

- **D SAI:** Conditional access tránh gọi Trim trên null.

---

Vị trí bài học: [LO-W07-02](../../notes/module-2/week-07.md#lo-w07-02)

Ôn [bài 2](../../notes/module-2/week-07.md) · LO-W07-02 · W07-C02.

### Q07-05

Rethrow trong catch nên dùng gì để giữ stack?

- **Đáp án đúng:** **D** — throw;

- **Phương án D ĐÚNG:** Rethrow giữ original exception stack.

- **A SAI:** throw ex có thể reset trace location.

- **B SAI:** Return success che failure.

- **C SAI:** Exception mới không inner mất nguyên nhân.

---

Vị trí bài học: [LO-W07-03](../../notes/module-2/week-07.md#lo-w07-03)

Ôn [bài 3](../../notes/module-2/week-07.md) · LO-W07-03 · W07-C03.

### Q07-06

Ngày invalid nên xử lý ra sao ở import?

- **Đáp án đúng:** **A** — TryParseExact và row/field error

- **Phương án A ĐÚNG:** Input expected invalid cần error rõ và không partial commit.

- **B SAI:** Đổi ngày làm sai data nghiệp vụ.

- **C SAI:** ! không parse/validate.

- **D SAI:** Bỏ lỗi không báo gây mất dữ liệu.

---

Vị trí bài học: [LO-W07-03](../../notes/module-2/week-07.md#lo-w07-03)

Ôn [bài 3](../../notes/module-2/week-07.md) · LO-W07-03 · W07-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
