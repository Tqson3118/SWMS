# Đáp án tuần 22 — Docker Compose và persistence

[Quiz](../quizzes/week-22-quiz.md) · [Bài nguồn](../../notes/module-6/week-22.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q22-01 | LO-W22-01 | Analyze | Stages | C | W22-C01 |
| Q22-02 | LO-W22-01 | Analyze | Image secret | D | W22-C01 |
| Q22-03 | LO-W22-02 | Analyze | Service address | A | W22-C02 |
| Q22-04 | LO-W22-02 | Analyze | Persistent data | B | W22-C02 |
| Q22-05 | LO-W22-03 | Analyze | Ready dependency | C | W22-C03 |
| Q22-06 | LO-W22-03 | Analyze | Migration rollout | D | W22-C03 |



## Giải thích từng phương án

### Q22-01

Multi-stage build giúp gì?

- **Đáp án đúng:** **C** — Chỉ copy runtime artifacts sang final image

- **Phương án C ĐÚNG:** Build tools không cần ở runtime layer.

- **A SAI:** Image không tạo identity protocol.

- **B SAI:** Build image không publish service.

- **D SAI:** Database backup là nhiệm vụ khác.

---

Vị trí bài học: [LO-W22-01](../../notes/module-6/week-22.md#lo-w22-01)

Ôn [bài 1](../../notes/module-6/week-22.md) · LO-W22-01 · W22-C01.

### Q22-02

COPY .env vào image có an toàn không?

- **Đáp án đúng:** **D** — Không, secret có thể tồn tại layers

- **Phương án D ĐÚNG:** Image/layers có thể inspect/redistribute.

- **A SAI:** Private deployment không xóa exposure.

- **B SAI:** Minify không encrypt credentials.

- **C SAI:** Xóa sau không xóa content layer trước.

---

Vị trí bài học: [LO-W22-01](../../notes/module-6/week-22.md#lo-w22-01)

Ôn [bài 1](../../notes/module-6/week-22.md) · LO-W22-01 · W22-C01.

### Q22-03

API container nối SQL Compose bằng gì?

- **Đáp án đúng:** **A** — db:1433

- **Phương án A ĐÚNG:** DNS service name resolve trong network.

- **B SAI:** Loopback trỏ chính API container.

- **C SAI:** 5173 là frontend devserver.

- **D SAI:** Laptop IP không service DNS ổn định.

---

Vị trí bài học: [LO-W22-02](../../notes/module-6/week-22.md#lo-w22-02)

Ôn [bài 2](../../notes/module-6/week-22.md) · LO-W22-02 · W22-C02.

### Q22-04

Named volume có phải backup đủ không?

- **Đáp án đúng:** **B** — Không, cần bản backup/restore độc lập

- **Phương án B ĐÚNG:** Volume vẫn mất khi host/storage hỏng.

- **A SAI:** Backup không thử restore chưa đáng tin.

- **C SAI:** Persistence không disaster recovery.

- **D SAI:** Cùng failure domain không độc lập.

---

Vị trí bài học: [LO-W22-02](../../notes/module-6/week-22.md#lo-w22-02)

Ôn [bài 2](../../notes/module-6/week-22.md) · LO-W22-02 · W22-C02.

### Q22-05

depends_on mặc định chứng minh DBready không?

- **Đáp án đúng:** **C** — Không, cần healthcheck/condition hoặc retry

- **Phương án C ĐÚNG:** Started khác accepting SQL queries.

- **A SAI:** Fixed delay phụ thuộc timing không chắc.

- **B SAI:** Healthcheck kiểm được readiness.

- **D SAI:** Service có thể đang initialize.

---

Vị trí bài học: [LO-W22-03](../../notes/module-6/week-22.md#lo-w22-03)

Ôn [bài 3](../../notes/module-6/week-22.md) · LO-W22-03 · W22-C03.

### Q22-06

Production nhiều replicas nên migrate thế nào?

- **Đáp án đúng:** **D** — Một job có review trước rollout

- **Phương án D ĐÚNG:** Điều phối schema change giảm race.

- **A SAI:** Concurrent migration có thể tranh chấp.

- **B SAI:** Xóa DB làm mất business data.

- **C SAI:** Schema history cần tracked/reviewed.

---

Vị trí bài học: [LO-W22-03](../../notes/module-6/week-22.md#lo-w22-03)

Ôn [bài 3](../../notes/module-6/week-22.md) · LO-W22-03 · W22-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
