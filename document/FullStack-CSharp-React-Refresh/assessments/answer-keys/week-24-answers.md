# Đáp án tuần 24 — Cloud deployment, monitoring và recovery

[Quiz](../quizzes/week-24-quiz.md) · [Bài nguồn](../../notes/module-6/week-24.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q24-01 | LO-W24-01 | Analyze | Production config | A | W24-C01 |
| Q24-02 | LO-W24-01 | Analyze | Auth deployment | B | W24-C01 |
| Q24-03 | LO-W24-02 | Analyze | Readiness | C | W24-C02 |
| Q24-04 | LO-W24-02 | Analyze | Logging privacy | D | W24-C02 |
| Q24-05 | LO-W24-03 | Analyze | Restore proof | A | W24-C03 |
| Q24-06 | LO-W24-03 | Analyze | Schema rollback | B | W24-C03 |



## Giải thích từng phương án

### Q24-01

SPA APIURL đổi lúc nào với Vite build-timeenv?

- **Đáp án đúng:** **A** — Build/rebuild client đúng publicURL

- **Phương án A ĐÚNG:** Build-time value được đóng vào assets.

- **B SAI:** Backendenv không sửa clientbundle.

- **C SAI:** Browser không cần databasecredential.

- **D SAI:** HTTPS cần cho traffic/auth.

---

Vị trí bài học: [LO-W24-01](../../notes/module-6/week-24.md#lo-w24-01)

Ôn [bài 1](../../notes/module-6/week-24.md) · LO-W24-01 · W24-C01.

### Q24-02

Production dùng dev user-jwts được không?

- **Đáp án đúng:** **B** — Không, provider token validation thật

- **Phương án B ĐÚNG:** Devtool chỉ phục vụ Development lab.

- **A SAI:** Public endpoint càng cần trustvalidation.

- **C SAI:** Không là identity service production.

- **D SAI:** Renameclaim không đổi securitymodel.

---

Vị trí bài học: [LO-W24-01](../../notes/module-6/week-24.md#lo-w24-01)

Ôn [bài 1](../../notes/module-6/week-24.md) · LO-W24-01 · W24-C01.

### Q24-03

DBdown nên phản ánh ở đâu cho API cần DB?

- **Đáp án đúng:** **C** — Readiness và dependency metrics

- **Phương án C ĐÚNG:** Readiness báo khả năng xử lý dependency.

- **A SAI:** Cần diagnosis/alert có action.

- **B SAI:** Fake success làm sai client/data.

- **D SAI:** Restartprocess không chữa DBoutage.

---

Vị trí bài học: [LO-W24-02](../../notes/module-6/week-24.md#lo-w24-02)

Ôn [bài 2](../../notes/module-6/week-24.md) · LO-W24-02 · W24-C02.

### Q24-04

Production nên tránh log gì?

- **Đáp án đúng:** **D** — Bearer token và finance note

- **Phương án D ĐÚNG:** Credential/personal data không cần diagnosis.

- **A SAI:** TraceID nối request với logs.

- **B SAI:** Status giúp đo success/error.

- **C SAI:** Operation mô tả hành vi không secret.

---

Vị trí bài học: [LO-W24-02](../../notes/module-6/week-24.md#lo-w24-02)

Ôn [bài 2](../../notes/module-6/week-24.md) · LO-W24-02 · W24-C02.

### Q24-05

Có backupfile đã đủ recovery gate chưa?

- **Đáp án đúng:** **A** — Chưa, phải restore và kiểm dữ liệu

- **Phương án A ĐÚNG:** Restoretest xác nhận bảnbackup dùng được.

- **B SAI:** Backup có thể lỗi/không truy cập.

- **C SAI:** Filename không integrity/recoverability.

- **D SAI:** Constraints/ownership là dữ liệu đúng.

---

Vị trí bài học: [LO-W24-03](../../notes/module-6/week-24.md#lo-w24-03)

Ôn [bài 3](../../notes/module-6/week-24.md) · LO-W24-03 · W24-C03.

### Q24-06

Rollback image có undo destructive SQL tự động không?

- **Đáp án đúng:** **B** — Không, cần compatibility/recovery plan

- **Phương án B ĐÚNG:** Appartifact và DBstate là boundaries riêng.

- **A SAI:** CDN không khôi phục schema.

- **C SAI:** EF không tự đảo migration khi imagecũ.

- **D SAI:** Docker không version businessDB theo image.

---

Vị trí bài học: [LO-W24-03](../../notes/module-6/week-24.md#lo-w24-03)

Ôn [bài 3](../../notes/module-6/week-24.md) · LO-W24-03 · W24-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
