# Đáp án tuần 16 — API tests và atomic/idempotent import

[Quiz](../quizzes/week-16-quiz.md) · [Bài nguồn](../../notes/module-4/week-16.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q16-01 | LO-W16-01 | Analyze | Provider proof | A | W16-C01 |
| Q16-02 | LO-W16-01 | Analyze | DB isolation | B | W16-C01 |
| Q16-03 | LO-W16-02 | Analyze | Invalid import | C | W16-C02 |
| Q16-04 | LO-W16-02 | Analyze | Retry count | D | W16-C02 |
| Q16-05 | LO-W16-03 | Analyze | Fault boundary | A | W16-C03 |
| Q16-06 | LO-W16-03 | Analyze | Auth evidence | B | W16-C03 |



## Giải thích từng phương án

### Q16-01

Unitmock đủ chứng minh FKSQL chưa?

- **Đáp án đúng:** **A** — Chưa

- **Phương án A ĐÚNG:** Provider behavior cần actual relational test.

- **B SAI:** Coverage không DBsemantics.

- **C SAI:** Mock chỉ giả định của tác giảtest.

- **D SAI:** File chưa apply/test.

---

Vị trí bài học: [LO-W16-01](../../notes/module-4/week-16.md#lo-w16-01)

Ôn [bài 1](../../notes/module-4/week-16.md) · LO-W16-01 · W16-C01.

### Q16-02

Integration DB nên dùng gì?

- **Đáp án đúng:** **B** — Isolated lab DB và owned fixtures

- **Phương án B ĐÚNG:** Deterministic và tránh external writes.

- **A SAI:** Không record khó reproduce.

- **C SAI:** Testwrites không prod.

- **D SAI:** Sharedstate gây risk/flaky.

---

Vị trí bài học: [LO-W16-01](../../notes/module-4/week-16.md#lo-w16-01)

Ôn [bài 1](../../notes/module-4/week-16.md) · LO-W16-01 · W16-C01.

### Q16-03

Batch 3 rows, 1 invalid all-or-nothing làm gì?

- **Đáp án đúng:** **C** — Báo row error và0newrows

- **Phương án C ĐÚNG:** Validate whole batch trước write.

- **A SAI:** Sửa âm thầm sai ý nghĩa.

- **B SAI:** Owner critical không optional.

- **D SAI:** Partial trái contract.

---

Vị trí bài học: [LO-W16-02](../../notes/module-4/week-16.md#lo-w16-02)

Ôn [bài 2](../../notes/module-4/week-16.md) · LO-W16-02 · W16-C02.

### Q16-04

Samekey/hash 3 rows retry tạo tổng mấy expense?

- **Đáp án đúng:** **D** — 3

- **Phương án D ĐÚNG:** Mộtoperationeffect duy nhất.

- **A SAI:** Duplicate hai lần trái idempotency.

- **B SAI:** Triplicate cũng sai.

- **C SAI:** Firstsuccess phải lưu3.

---

Vị trí bài học: [LO-W16-02](../../notes/module-4/week-16.md#lo-w16-02)

Ôn [bài 2](../../notes/module-4/week-16.md) · LO-W16-02 · W16-C02.

### Q16-05

Fault debug endpoint deploy public không?

- **Đáp án đúng:** **A** — Không

- **Phương án A ĐÚNG:** Giữtest-only dependency/injection.

- **B SAI:** Demo không là lý do gây public crash.

- **C SAI:** Tên không permission.

- **D SAI:** Errorgeneric không làmroutecần thiết.

---

Vị trí bài học: [LO-W16-03](../../notes/module-4/week-16.md#lo-w16-03)

Ôn [bài 3](../../notes/module-4/week-16.md) · LO-W16-03 · W16-C03.

### Q16-06

ẢnhUI đủ chứng minh bad signature → 401 không?

- **Đáp án đúng:** **B** — Không,cần real auth test

- **Phương án B ĐÚNG:** UI không server crypto validation.

- **A SAI:** Compilefrontend khôngbearerbackend.

- **C SAI:** Loading không auth.

- **D SAI:** HidebypassHTTP.

---

Vị trí bài học: [LO-W16-03](../../notes/module-4/week-16.md#lo-w16-03)

Ôn [bài 3](../../notes/module-4/week-16.md) · LO-W16-03 · W16-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
