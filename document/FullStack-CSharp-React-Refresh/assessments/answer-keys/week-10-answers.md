# Đáp án tuần 10 — JOIN, GROUP BY, subqueries và CTE

[Quiz](../quizzes/week-10-quiz.md) · [Bài nguồn](../../notes/module-3/week-10.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q10-01 | LO-W10-01 | Analyze | Left join filter | C | W10-C01 |
| Q10-02 | LO-W10-01 | Analyze | Join multiplicity | D | W10-C01 |
| Q10-03 | LO-W10-02 | Analyze | Count empty | A | W10-C02 |
| Q10-04 | LO-W10-02 | Analyze | Group filter | B | W10-C02 |
| Q10-05 | LO-W10-03 | Analyze | CTE semantics | C | W10-C03 |
| Q10-06 | LO-W10-03 | Analyze | Exists membership | D | W10-C03 |



## Giải thích từng phương án

### Q10-01

WHERE child.amount>0 sau LEFT JOIN làm gì với parent rỗng?

- **Đáp án đúng:** **C** — Loại parent rỗng vì predicate không true

- **Phương án C ĐÚNG:** Null comparison UNKNOWN bị WHERE lọc.

- **A SAI:** Không có automatic coalesce.

- **B SAI:** Query SELECT không insert rows.

- **D SAI:** LEFT JOIN preservation có thể bị WHERE loại.

---

Vị trí bài học: [LO-W10-01](../../notes/module-3/week-10.md#lo-w10-01)

Ôn [bài 1](../../notes/module-3/week-10.md) · LO-W10-01 · W10-C01.

### Q10-02

Category2 expenses JOIN trả2rows có phải SQL bug?

- **Đáp án đúng:** **D** — Không, đúng one-to-many cardinality

- **Phương án D ĐÚNG:** Mỗi matched child có row output.

- **A SAI:** Join không aggregate tự động.

- **B SAI:** FK không gây bug cardinality.

- **C SAI:** Distinct amount có thể bỏ hai expense cùng amount hợp lệ.

---

Vị trí bài học: [LO-W10-01](../../notes/module-3/week-10.md#lo-w10-01)

Ôn [bài 1](../../notes/module-3/week-10.md) · LO-W10-01 · W10-C01.

### Q10-03

LEFT JOIN category rỗng muốn expense count0 dùng gì?

- **Đáp án đúng:** **A** — COUNT(child.Id)

- **Phương án A ĐÚNG:** Null child PK không được COUNT(column) đếm.

- **B SAI:** COUNT* đếm preserved row.

- **C SAI:** COUNT1 cũng đếm synthetic row.

- **D SAI:** SUM1 cũng cho1 ở row preserved.

---

Vị trí bài học: [LO-W10-02](../../notes/module-3/week-10.md#lo-w10-02)

Ôn [bài 2](../../notes/module-3/week-10.md) · LO-W10-02 · W10-C02.

### Q10-04

Lọc category total>=50000 nên dùng gì?

- **Đáp án đúng:** **B** — HAVING SUM(amount)>=50000

- **Phương án B ĐÚNG:** HAVING lọc aggregate group.

- **A SAI:** SQL Server không dùng LIMIT và semantics sai.

- **C SAI:** WHERE thay tập items nên tổng có thể khác.

- **D SAI:** Ordering không filtering.

---

Vị trí bài học: [LO-W10-02](../../notes/module-3/week-10.md#lo-w10-02)

Ôn [bài 2](../../notes/module-3/week-10.md) · LO-W10-02 · W10-C02.

### Q10-05

CTE luôn materialize và cache query không?

- **Đáp án đúng:** **C** — Không

- **Phương án C ĐÚNG:** CTE tổ chức query, optimizer quyết strategy.

- **A SAI:** Performance phụ thuộc plan/workload.

- **B SAI:** WITH query không tự mở explicit transaction.

- **D SAI:** CTE không persistent table.

---

Vị trí bài học: [LO-W10-03](../../notes/module-3/week-10.md#lo-w10-03)

Ôn [bài 3](../../notes/module-3/week-10.md) · LO-W10-03 · W10-C03.

### Q10-06

Chỉ cần category có expense nên EXISTS ích gì?

- **Đáp án đúng:** **D** — Membership không nhân duplicate parent

- **Phương án D ĐÚNG:** EXISTS trả bool condition cho outer row.

- **A SAI:** Nó không projection child columns.

- **B SAI:** Query không create index.

- **C SAI:** Scope user vẫn cần explicit.

---

Vị trí bài học: [LO-W10-03](../../notes/module-3/week-10.md#lo-w10-03)

Ôn [bài 3](../../notes/module-3/week-10.md) · LO-W10-03 · W10-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
