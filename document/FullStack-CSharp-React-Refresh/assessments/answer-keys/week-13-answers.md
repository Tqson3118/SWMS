# Đáp án tuần 13 — REST, middleware, DI và API errors

[Quiz](../quizzes/week-13-quiz.md) · [Bài nguồn](../../notes/module-4/week-13.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q13-01 | LO-W13-01 | Analyze | Create response | B | W13-C01 |
| Q13-02 | LO-W13-01 | Analyze | Owner DTO | C | W13-C01 |
| Q13-03 | LO-W13-02 | Analyze | Middleware order | D | W13-C02 |
| Q13-04 | LO-W13-02 | Analyze | CORS | A | W13-C02 |
| Q13-05 | LO-W13-03 | Analyze | Error contract | B | W13-C03 |
| Q13-06 | LO-W13-03 | Analyze | Logging | C | W13-C03 |



## Giải thích từng phương án

### Q13-01

POST tạo resource thành công trả gì?

- **Đáp án đúng:** **B** — 201 và Location

- **Phương án B ĐÚNG:** Created có URI rõ cho resource.

- **A SAI:** API bearer lỗi không redirect login.

- **C SAI:** 200 lỗi khiến client hiểu sai success.

- **D SAI:** 204 không có body response.

---

Vị trí bài học: [LO-W13-01](../../notes/module-4/week-13.md#lo-w13-01)

Ôn [bài 1](../../notes/module-4/week-13.md) · LO-W13-01 · W13-C01.

### Q13-02

Client UserId quyết owner có đúng không?

- **Đáp án đúng:** **C** — Không, owner từ validated principal

- **Phương án C ĐÚNG:** Type/binding không cấp quyền chọn owner.

- **A SAI:** Hidden input vẫn sửa được.

- **B SAI:** CORS không resource access control.

- **D SAI:** Guid đúng syntax không authorization.

---

Vị trí bài học: [LO-W13-01](../../notes/module-4/week-13.md#lo-w13-01)

Ôn [bài 1](../../notes/module-4/week-13.md) · LO-W13-01 · W13-C01.

### Q13-03

Authentication đứng đâu so authorization?

- **Đáp án đúng:** **D** — Trước authorization

- **Phương án D ĐÚNG:** Principal phải có trước kiểm quyền.

- **A SAI:** Sau response không bảo vệ request.

- **B SAI:** Order đảo khiến principal chưa có.

- **C SAI:** Decode JSON không token trust.

---

Vị trí bài học: [LO-W13-02](../../notes/module-4/week-13.md#lo-w13-02)

Ôn [bài 2](../../notes/module-4/week-13.md) · LO-W13-02 · W13-C02.

### Q13-04

CORS có thay ownership không?

- **Đáp án đúng:** **A** — Không

- **Phương án A ĐÚNG:** Browser policy không app business security.

- **B SAI:** Curl không bị browser CORS gate.

- **C SAI:** Bearer validator kiểm chữ ký riêng.

- **D SAI:** Queries phải scope theo user.

---

Vị trí bài học: [LO-W13-02](../../notes/module-4/week-13.md#lo-w13-02)

Ôn [bài 2](../../notes/module-4/week-13.md) · LO-W13-02 · W13-C02.

### Q13-05

Unexpected exception production nên trả gì?

- **Đáp án đúng:** **B** — 500 generic ProblemDetails

- **Phương án B ĐÚNG:** Status failure và trace đủ nối log.

- **A SAI:** 404 không đúng semantics unexpected error.

- **C SAI:** 200 misleading và stack lộ nội bộ.

- **D SAI:** Bearer là credential không response debug.

---

Vị trí bài học: [LO-W13-03](../../notes/module-4/week-13.md#lo-w13-03)

Ôn [bài 3](../../notes/module-4/week-13.md) · LO-W13-03 · W13-C03.

### Q13-06

Log nên giữ gì?

- **Đáp án đúng:** **C** — Operation/ID/trace, không secrets

- **Phương án C ĐÚNG:** Đủ diagnosis, giảm personal data.

- **A SAI:** Connection chứa credential.

- **B SAI:** Note có thể nhạy cảm, không cần log toàn bộ.

- **D SAI:** Header bearer là secret.

---

Vị trí bài học: [LO-W13-03](../../notes/module-4/week-13.md#lo-w13-03)

Ôn [bài 3](../../notes/module-4/week-13.md) · LO-W13-03 · W13-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
