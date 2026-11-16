# Đáp án tuần 11 — Index, execution plan, pagination và N+1

[Quiz](../quizzes/week-11-quiz.md) · [Bài nguồn](../../notes/module-3/week-11.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q11-01 | LO-W11-01 | Analyze | Index cost | D | W11-C01 |
| Q11-02 | LO-W11-01 | Analyze | SARGable date | A | W11-C01 |
| Q11-03 | LO-W11-02 | Analyze | Plan evidence | B | W11-C02 |
| Q11-04 | LO-W11-02 | Analyze | Small scan | C | W11-C02 |
| Q11-05 | LO-W11-03 | Analyze | Stable paging | D | W11-C03 |
| Q11-06 | LO-W11-03 | Analyze | N+1 fix | A | W11-C03 |



## Giải thích từng phương án

### Q11-01

Thêm index có tradeoff gì?

- **Đáp án đúng:** **D** — Read có thể nhanh hơn, write/storage tốn hơn

- **Phương án D ĐÚNG:** Index maintenance/storage đổi workload cost.

- **A SAI:** Không index nào tối ưu mọi query.

- **B SAI:** Every applicable write maintenance có chi phí.

- **C SAI:** Statistics ảnh hưởng optimizer.

---

Vị trí bài học: [LO-W11-01](../../notes/module-3/week-11.md#lo-w11-01)

Ôn [bài 1](../../notes/module-3/week-11.md) · LO-W11-01 · W11-C01.

### Q11-02

Filter tháng nên thử form nào?

- **Đáp án đúng:** **A** — SpentOn>=from AND SpentOn<to

- **Phương án A ĐÚNG:** Range trên column phù hợp index.

- **B SAI:** Function trên column có thể làm khó seek.

- **C SAI:** Formatting thêm computation/semantics sai.

- **D SAI:** Bỏ owner phá security.

---

Vị trí bài học: [LO-W11-01](../../notes/module-3/week-11.md#lo-w11-01)

Ôn [bài 1](../../notes/module-3/week-11.md) · LO-W11-01 · W11-C01.

### Q11-03

Index Seek đủ chứng minh query nhanh chưa?

- **Đáp án đúng:** **B** — Chưa, xem reads/rows/lookup/timing

- **Phương án B ĐÚNG:** Seek có thể lấy nhiều rows và costly lookups.

- **A SAI:** Dataset/conditions cần để reproduce.

- **C SAI:** Icon không total work.

- **D SAI:** Cost% estimate không thời gian thực.

---

Vị trí bài học: [LO-W11-02](../../notes/module-3/week-11.md#lo-w11-02)

Ôn [bài 2](../../notes/module-3/week-11.md) · LO-W11-02 · W11-C02.

### Q11-04

Scan small table luôn cần sửa không?

- **Đáp án đúng:** **C** — Không, có thể plan hợp lý

- **Phương án C ĐÚNG:** Optimizer có thể chọn scan rẻ cho small/manyrows.

- **A SAI:** Đổi DB không thay workload evidence.

- **B SAI:** Index bừa tăng write/storage.

- **D SAI:** Khẳng định tuyệt đối sai.

---

Vị trí bài học: [LO-W11-02](../../notes/module-3/week-11.md#lo-w11-02)

Ôn [bài 2](../../notes/module-3/week-11.md) · LO-W11-02 · W11-C02.

### Q11-05

ORDER BY date có ties thì thêm gì?

- **Đáp án đúng:** **D** — Unique Id tie-breaker

- **Phương án D ĐÚNG:** Unique secondary key ổn định fixed data.

- **A SAI:** Random phá thứ tự.

- **B SAI:** SQL output không bảo đảm order mặc định.

- **C SAI:** Page size không giải tie order.

---

Vị trí bài học: [LO-W11-03](../../notes/module-3/week-11.md#lo-w11-03)

Ôn [bài 3](../../notes/module-3/week-11.md) · LO-W11-03 · W11-C03.

### Q11-06

List100rows rồi fetch category từngrow nên sửa gì?

- **Đáp án đúng:** **A** — Projection/JOIN theo dữ liệu cần, đo query count

- **Phương án A ĐÚNG:** Giảm round trips và tải fields hợp lý.

- **B SAI:** Timeout không bỏ 101 calls.

- **C SAI:** Toàn DB không scale/security boundary.

- **D SAI:** Frontend index không giảm backend query count.

---

Vị trí bài học: [LO-W11-03](../../notes/module-3/week-11.md#lo-w11-03)

Ôn [bài 3](../../notes/module-3/week-11.md) · LO-W11-03 · W11-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
