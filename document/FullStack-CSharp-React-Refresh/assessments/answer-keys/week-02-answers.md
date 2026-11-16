# Đáp án tuần 02 — JavaScript: scope, closure, array và tiền

[Quiz](../quizzes/week-02-quiz.md) · [Bài nguồn](../../notes/module-1/week-02.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q02-01 | LO-W02-01 | Analyze | Closure state | C | W02-C01 |
| Q02-02 | LO-W02-01 | Analyze | Const binding | D | W02-C01 |
| Q02-03 | LO-W02-02 | Analyze | Reduce initial | A | W02-C02 |
| Q02-04 | LO-W02-02 | Analyze | Shallow copy | B | W02-C02 |
| Q02-05 | LO-W02-03 | Analyze | Empty number | C | W02-C03 |
| Q02-06 | LO-W02-03 | Analyze | Identity equality | D | W02-C03 |



## Giải thích từng phương án

### Q02-01

Closure có giữ khả năng đọc biến lexical sau hàm ngoài return không?

- **Đáp án đúng:** **C** — Có, khi function còn reachable

- **Phương án C ĐÚNG:** Lexical environment còn được closure tham chiếu.

- **A SAI:** Closure giữ binding/reference, không deep clone.

- **B SAI:** let/const cũng được capture.

- **D SAI:** Return không đồng nghĩa biến captured biến mất.

---

Vị trí bài học: [LO-W02-01](../../notes/module-1/week-02.md#lo-w02-01)

Ôn [bài 1](../../notes/module-1/week-02.md) · LO-W02-01 · W02-C01.

### Q02-02

const e={amount:1}; e.amount=2 có hợp lệ không?

- **Đáp án đúng:** **D** — Có

- **Phương án D ĐÚNG:** Binding không đổi, property vẫn mutable.

- **A SAI:** const không phải Object.freeze.

- **B SAI:** Không cần var để sửa property.

- **C SAI:** Assignment property không tạo object mới.

---

Vị trí bài học: [LO-W02-01](../../notes/module-1/week-02.md#lo-w02-01)

Ôn [bài 1](../../notes/module-1/week-02.md) · LO-W02-01 · W02-C01.

### Q02-03

reduce cộng trên mảng rỗng với initial 0 trả gì?

- **Đáp án đúng:** **A** — 0

- **Phương án A ĐÚNG:** Initial được trả khi không có phần tử.

- **B SAI:** undefined không phải initial đã truyền.

- **C SAI:** Throw xảy ra trường hợp không initial, không ở đây.

- **D SAI:** Không có phần tử để cộng 1.

---

Vị trí bài học: [LO-W02-02](../../notes/module-1/week-02.md#lo-w02-02)

Ôn [bài 2](../../notes/module-1/week-02.md) · LO-W02-02 · W02-C02.

### Q02-04

[...xs] có clone sâu expense object không?

- **Đáp án đúng:** **B** — Không, object bên trong còn chung reference

- **Phương án B ĐÚNG:** Spread tạo mảng mới nhưng không deep-copy element.

- **A SAI:** Spread array vẫn array.

- **C SAI:** Deep clone cần cơ chế khác và có giới hạn.

- **D SAI:** Tạo array không tự sửa input.

---

Vị trí bài học: [LO-W02-02](../../notes/module-1/week-02.md#lo-w02-02)

Ôn [bài 2](../../notes/module-1/week-02.md) · LO-W02-02 · W02-C02.

### Q02-05

Vì sao check raw.trim() trước Number(raw)?

- **Đáp án đúng:** **C** — Chuỗi rỗng convert thành 0

- **Phương án C ĐÚNG:** 0 có thể che missing input nên cần guard riêng.

- **A SAI:** Conversion đồng bộ, không Promise.

- **B SAI:** Không có conversion rỗng thành 1.

- **D SAI:** Number('') không throw.

---

Vị trí bài học: [LO-W02-03](../../notes/module-1/week-02.md#lo-w02-03)

Ôn [bài 3](../../notes/module-1/week-02.md) · LO-W02-03 · W02-C03.

### Q02-06

Hai object riêng {a:1} === {a:1} cho gì?

- **Đáp án đúng:** **D** — false

- **Phương án D ĐÚNG:** Identity riêng nên false.

- **A SAI:** === không deep compare properties.

- **B SAI:** Operator trả boolean.

- **C SAI:** Object identity comparison hợp lệ.

---

Vị trí bài học: [LO-W02-03](../../notes/module-1/week-02.md#lo-w02-03)

Ôn [bài 3](../../notes/module-1/week-02.md) · LO-W02-03 · W02-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
