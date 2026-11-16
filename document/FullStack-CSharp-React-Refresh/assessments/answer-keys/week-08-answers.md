# Đáp án tuần 08 — Task, cancellation, disposal và DI

[Quiz](../quizzes/week-08-quiz.md) · [Bài nguồn](../../notes/module-2/week-08.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q08-01 | LO-W08-01 | Analyze | Task meaning | A | W08-C01 |
| Q08-02 | LO-W08-01 | Analyze | Blocking task | B | W08-C01 |
| Q08-03 | LO-W08-02 | Analyze | Cooperative cancel | C | W08-C02 |
| Q08-04 | LO-W08-02 | Analyze | Using ownership | D | W08-C02 |
| Q08-05 | LO-W08-03 | Analyze | Scoped lifetime | A | W08-C03 |
| Q08-06 | LO-W08-03 | Analyze | Captive dependency | B | W08-C03 |



## Giải thích từng phương án

### Q08-01

Task luôn là thread riêng không?

- **Đáp án đúng:** **A** — Không

- **Phương án A ĐÚNG:** Task đại diện completion, có thể async IO không giữ thread chờ.

- **B SAI:** Mapping1:1 với OS thread sai.

- **C SAI:** Generic chỉ biểu diễn result type.

- **D SAI:** Await không tăng phần cứng.

---

Vị trí bài học: [LO-W08-01](../../notes/module-2/week-08.md#lo-w08-01)

Ôn [bài 1](../../notes/module-2/week-08.md) · LO-W08-01 · W08-C01.

### Q08-02

Tại sao tránh .Result trong web service?

- **Đáp án đúng:** **B** — Nó block thread và có rủi ro context/deadlock

- **Phương án B ĐÚNG:** Blocking giảm scalability và có context hazard.

- **A SAI:** Cancellation phải explicit.

- **C SAI:** Result có thể đúng value nhưng cách chờ có vấn đề.

- **D SAI:** Task.Result không xử auth.

---

Vị trí bài học: [LO-W08-01](../../notes/module-2/week-08.md#lo-w08-01)

Ôn [bài 1](../../notes/module-2/week-08.md) · LO-W08-01 · W08-C01.

### Q08-03

Token cancel có cưỡng chế mọi function dừng không?

- **Đáp án đúng:** **C** — Không, code/API phải quan sát token

- **Phương án C ĐÚNG:** Cancellation là cooperative contract.

- **A SAI:** Commit đã xong không tự undo.

- **B SAI:** Resource cleanup cần lifecycle/using.

- **D SAI:** Không thread abort toàn hệ thống.

---

Vị trí bài học: [LO-W08-02](../../notes/module-2/week-08.md#lo-w08-02)

Ôn [bài 2](../../notes/module-2/week-08.md) · LO-W08-02 · W08-C02.

### Q08-04

using cho stream bảo đảm gì khi exception trong scope?

- **Đáp án đúng:** **D** — Dispose stream khi thoát scope

- **Phương án D ĐÚNG:** Compiler cleanup gọi dispose cả exceptional exit.

- **A SAI:** Dispose không ép GC toàn heap.

- **B SAI:** Exception vẫn propagate nếu không catch.

- **C SAI:** Closing stream không transaction data rollback.

---

Vị trí bài học: [LO-W08-02](../../notes/module-2/week-08.md#lo-w08-02)

Ôn [bài 2](../../notes/module-2/week-08.md) · LO-W08-02 · W08-C02.

### Q08-05

Scoped ở ASP.NET thường dùng instance ra sao?

- **Đáp án đúng:** **A** — Một instance mỗi request scope

- **Phương án A ĐÚNG:** Request scope là lifetime phổ biến.

- **B SAI:** Singleton mới container-wide.

- **C SAI:** Transient per resolution, không per line.

- **D SAI:** Table không DI scope.

---

Vị trí bài học: [LO-W08-03](../../notes/module-2/week-08.md#lo-w08-03)

Ôn [bài 3](../../notes/module-2/week-08.md) · LO-W08-03 · W08-C03.

### Q08-06

Singleton giữ DbContext scoped nên xử lý thế nào?

- **Đáp án đúng:** **B** — Đổi lifetime hoặc tạo scope đúng boundary job

- **Phương án B ĐÚNG:** Tách lifetime tránh giữ resource ngoài scope.

- **A SAI:** Dispose constructor làm collaborator không dùng được.

- **C SAI:** Singleton không bảo đảm dependency an toàn.

- **D SAI:** Global lock che design sai và giảm throughput.

---

Vị trí bài học: [LO-W08-03](../../notes/module-2/week-08.md#lo-w08-03)

Ôn [bài 3](../../notes/module-2/week-08.md) · LO-W08-03 · W08-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
