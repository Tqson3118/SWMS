# Đáp án tuần 05 — C# value/reference, OOP và invariants

[Quiz](../quizzes/week-05-quiz.md) · [Bài nguồn](../../notes/module-2/week-05.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q05-01 | LO-W05-01 | Analyze | Reference copy | B | W05-C01 |
| Q05-02 | LO-W05-01 | Analyze | Value storage | C | W05-C01 |
| Q05-03 | LO-W05-02 | Analyze | Encapsulation | D | W05-C02 |
| Q05-04 | LO-W05-02 | Analyze | Composition | A | W05-C02 |
| Q05-05 | LO-W05-03 | Analyze | VND representation | B | W05-C03 |
| Q05-06 | LO-W05-03 | Analyze | Meaningful test | C | W05-C03 |



## Giải thích từng phương án

### Q05-01

b=a với a class rồi b.Amount=2 thường đổi a.Amount không?

- **Đáp án đúng:** **B** — Có, nếu cùng instance

- **Phương án B ĐÚNG:** Hai biến giữ reference tới cùng instance.

- **A SAI:** Field copy không phải default class assignment.

- **C SAI:** Assignment class không tự clone.

- **D SAI:** Alias không cần static.

---

Vị trí bài học: [LO-W05-01](../../notes/module-2/week-05.md#lo-w05-01)

Ôn [bài 1](../../notes/module-2/week-05.md) · LO-W05-01 · W05-C01.

### Q05-02

Có nên khẳng định value type luôn nằm stack không?

- **Đáp án đúng:** **C** — Không

- **Phương án C ĐÚNG:** Storage phụ thuộc context; boxing/fields có thể ở heap.

- **A SAI:** Enum cũng là value type, không quy định mọi storage.

- **B SAI:** Class có value fields hợp lệ.

- **D SAI:** Boxing là phản ví dụ cho khẳng định tuyệt đối.

---

Vị trí bài học: [LO-W05-01](../../notes/module-2/week-05.md#lo-w05-01)

Ôn [bài 1](../../notes/module-2/week-05.md) · LO-W05-01 · W05-C01.

### Q05-03

Setter amount public bỏ guard nguy cơ gì?

- **Đáp án đúng:** **D** — Caller tạo trạng thái invalid

- **Phương án D ĐÚNG:** Setter có thể bỏ quy tắc domain.

- **A SAI:** Performance không thay invariant.

- **B SAI:** OOP không chỉ có class/public property.

- **C SAI:** Setter không có atomic transaction mặc định.

---

Vị trí bài học: [LO-W05-02](../../notes/module-2/week-05.md#lo-w05-02)

Ôn [bài 2](../../notes/module-2/week-05.md) · LO-W05-02 · W05-C02.

### Q05-04

ReportService dùng reader hợp với quan hệ nào?

- **Đáp án đúng:** **A** — Has-a qua composition

- **Phương án A ĐÚNG:** Service cần collaborator reader, không phải reader subtype.

- **B SAI:** Report không substitutable DbContext.

- **C SAI:** Hai runtime không có inheritance như vậy.

- **D SAI:** Boundary có ích khi IO cần test/đổi.

---

Vị trí bài học: [LO-W05-02](../../notes/module-2/week-05.md#lo-w05-02)

Ôn [bài 2](../../notes/module-2/week-05.md) · LO-W05-02 · W05-C02.

### Q05-05

Core lưu tiền VND theo spec bằng gì?

- **Đáp án đúng:** **B** — long nguyên có guard

- **Phương án B ĐÚNG:** Long nguyên tránh rounding cho VND và guard miền.

- **A SAI:** Int dễ overflow tổng và vẫn cần guard.

- **C SAI:** Double có rounding, bỏ phần lẻ che input invalid.

- **D SAI:** String tùy ý không có numeric invariant.

---

Vị trí bài học: [LO-W05-03](../../notes/module-2/week-05.md#lo-w05-03)

Ôn [bài 3](../../notes/module-2/week-05.md) · LO-W05-03 · W05-C03.

### Q05-06

Test nào kiểm tra domain rule?

- **Đáp án đúng:** **C** — Amount0 bị chặn,50000 được nhận

- **Phương án C ĐÚNG:** Kiểm tra behavior và boundary thật.

- **A SAI:** File tồn tại không chứng minh behavior.

- **B SAI:** Naming không chứng minh domain đúng.

- **D SAI:** Getter thường không kiểm rule validation.

---

Vị trí bài học: [LO-W05-03](../../notes/module-2/week-05.md#lo-w05-03)

Ôn [bài 3](../../notes/module-2/week-05.md) · LO-W05-03 · W05-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
