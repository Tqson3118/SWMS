# Đáp án tuần 06 — Collections, generics và delegates

[Quiz](../quizzes/week-06-quiz.md) · [Bài nguồn](../../notes/module-2/week-06.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q06-01 | LO-W06-01 | Analyze | Dictionary lookup | C | W06-C01 |
| Q06-02 | LO-W06-01 | Analyze | Foreach mutation | D | W06-C01 |
| Q06-03 | LO-W06-02 | Analyze | Generic constraint | A | W06-C02 |
| Q06-04 | LO-W06-02 | Analyze | Read-only interface | B | W06-C02 |
| Q06-05 | LO-W06-03 | Analyze | Func signature | C | W06-C03 |
| Q06-06 | LO-W06-03 | Analyze | Closure capture | D | W06-C03 |



## Giải thích từng phương án

### Q06-01

Lookup Dictionary nên mô tả chi phí thế nào?

- **Đáp án đúng:** **C** — Trung bình O(1), không tuyệt đối mọi case

- **Phương án C ĐÚNG:** Hash table average nhanh nhưng collision/pathology có thể khác.

- **A SAI:** Lookup không mặc định quadratic.

- **B SAI:** Hash/equality cần nhất quán.

- **D SAI:** Khẳng định tuyệt đối bỏ qua collisions.

---

Vị trí bài học: [LO-W06-01](../../notes/module-2/week-06.md#lo-w06-01)

Ôn [bài 1](../../notes/module-2/week-06.md) · LO-W06-01 · W06-C01.

### Q06-02

Xóa List trong foreach nên đổi cách nào?

- **Đáp án đúng:** **D** — RemoveAll hoặc strategy không mutate enumerator đang chạy

- **Phương án D ĐÚNG:** Tránh invalidating active enumerator.

- **A SAI:** Nuốt lỗi có thể chỉ xóa một phần.

- **B SAI:** Variable declaration không sửa enumeration semantics.

- **C SAI:** Sleep không giải quyết mutation.

---

Vị trí bài học: [LO-W06-01](../../notes/module-2/week-06.md#lo-w06-01)

Ôn [bài 1](../../notes/module-2/week-06.md) · LO-W06-01 · W06-C01.

### Q06-03

where T:new() cho phép điều gì?

- **Đáp án đúng:** **A** — new T() với constructor phù hợp

- **Phương án A ĐÚNG:** Constraint yêu cầu public parameterless construction.

- **B SAI:** Chỉ members có constraint mới dùng được.

- **C SAI:** new() không bảo đảm immutability.

- **D SAI:** Generic constraint không thực hiện IO.

---

Vị trí bài học: [LO-W06-02](../../notes/module-2/week-06.md#lo-w06-02)

Ôn [bài 2](../../notes/module-2/week-06.md) · LO-W06-02 · W06-C02.

### Q06-04

IReadOnlyList<T> bảo đảm source không đổi bởi bất kỳ ai không?

- **Đáp án đúng:** **B** — Không

- **Phương án B ĐÚNG:** Interface hạn chế API caller, underlying có thể mutable.

- **A SAI:** T có thể reference type.

- **C SAI:** Không thực hiện deep freeze.

- **D SAI:** Không bảo đảm concurrent safety.

---

Vị trí bài học: [LO-W06-02](../../notes/module-2/week-06.md#lo-w06-02)

Ôn [bài 2](../../notes/module-2/week-06.md) · LO-W06-02 · W06-C02.

### Q06-05

Func<long,bool> biểu diễn gì?

- **Đáp án đúng:** **C** — Nhận long và trả bool

- **Phương án C ĐÚNG:** Type cuối là return, trước là parameters.

- **A SAI:** Action mới không có return value.

- **B SAI:** Func có thể sync hoặc trả Task riêng.

- **D SAI:** Thứ tự parameter/return bị đảo.

---

Vị trí bài học: [LO-W06-03](../../notes/module-2/week-06.md#lo-w06-03)

Ôn [bài 3](../../notes/module-2/week-06.md) · LO-W06-03 · W06-C03.

### Q06-06

Lambda capture threshold rồi threshold đổi có thể thấy gì?

- **Đáp án đúng:** **D** — Value mới của captured variable

- **Phương án D ĐÚNG:** Capture binding có thể phản ánh mutation.

- **A SAI:** Không deep freeze value tự động.

- **B SAI:** Không tự biến số thành null.

- **C SAI:** C# cho phép capture locals phù hợp.

---

Vị trí bài học: [LO-W06-03](../../notes/module-2/week-06.md#lo-w06-03)

Ôn [bài 3](../../notes/module-2/week-06.md) · LO-W06-03 · W06-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
