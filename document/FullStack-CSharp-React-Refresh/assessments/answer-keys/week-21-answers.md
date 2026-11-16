# Đáp án tuần 21 — Git, Linux và environment configuration

[Quiz](../quizzes/week-21-quiz.md) · [Bài nguồn](../../notes/module-6/week-21.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q21-01 | LO-W21-01 | Analyze | Staging | B | W21-C01 |
| Q21-02 | LO-W21-01 | Analyze | Secret incident | C | W21-C01 |
| Q21-03 | LO-W21-02 | Analyze | Linux paths | D | W21-C02 |
| Q21-04 | LO-W21-02 | Analyze | Container listener | A | W21-C02 |
| Q21-05 | LO-W21-03 | Analyze | Public env | B | W21-C03 |
| Q21-06 | LO-W21-03 | Analyze | Nested config | C | W21-C03 |



## Giải thích từng phương án

### Q21-01

Lệnh nào xem nội dung sẽ commit?

- **Đáp án đúng:** **B** — git diff --cached

- **Phương án B ĐÚNG:** Cached diff là index so HEAD.

- **A SAI:** Fetch tải refs không xem staged change.

- **C SAI:** Diff mặc định xem worktree so index.

- **D SAI:** Log xem commits đã có.

---

Vị trí bài học: [LO-W21-01](../../notes/module-6/week-21.md#lo-w21-01)

Ôn [bài 1](../../notes/module-6/week-21.md) · LO-W21-01 · W21-C01.

### Q21-02

Secret đã commit cần bước ưu tiên gì?

- **Đáp án đúng:** **C** — Revoke/rotate credential

- **Phương án C ĐÚNG:** Credential có thể đã lộ từ history.

- **A SAI:** README không thu hồi quyền credential.

- **B SAI:** Obfuscation không bảo vệ secret.

- **D SAI:** History còn giữ secret cũ.

---

Vị trí bài học: [LO-W21-01](../../notes/module-6/week-21.md#lo-w21-01)

Ôn [bài 1](../../notes/module-6/week-21.md) · LO-W21-01 · W21-C01.

### Q21-03

Linux CI không tìm Expense.tsx thường kiểm gì?

- **Đáp án đúng:** **D** — Casing đường dẫn/import

- **Phương án D ĐÚNG:** Case-sensitive paths khác Windows.

- **A SAI:** Tắt checks không sửa đường dẫn.

- **B SAI:** Permissions khác lỗi tên file.

- **C SAI:** Xóa repo không chẩn đoán nguyên nhân.

---

Vị trí bài học: [LO-W21-02](../../notes/module-6/week-21.md#lo-w21-02)

Ôn [bài 2](../../notes/module-6/week-21.md) · LO-W21-02 · W21-C02.

### Q21-04

API trong container thường bind địa chỉ nào để nhận ngoài?

- **Đáp án đúng:** **A** — 0.0.0.0 trên port nội bộ

- **Phương án A ĐÚNG:** Bind mọi interfaces container cho mapped port.

- **B SAI:** Loopback không nhận qua container network.

- **C SAI:** Client IP không listener của server.

- **D SAI:** DB hostname thuộc dependency khác.

---

Vị trí bài học: [LO-W21-02](../../notes/module-6/week-21.md#lo-w21-02)

Ôn [bài 2](../../notes/module-6/week-21.md) · LO-W21-02 · W21-C02.

### Q21-05

VITE_ variable trong build được xem là gì?

- **Đáp án đúng:** **B** — Public client configuration

- **Phương án B ĐÚNG:** Bundle gửi browser có thể inspect.

- **A SAI:** Vite không tự mã hóa credential.

- **C SAI:** Frontend build không giữ secret vault.

- **D SAI:** Client code đọc giá trị được build.

---

Vị trí bài học: [LO-W21-03](../../notes/module-6/week-21.md#lo-w21-03)

Ôn [bài 3](../../notes/module-6/week-21.md) · LO-W21-03 · W21-C03.

### Q21-06

Env key cho ConnectionStrings:Finance là gì?

- **Đáp án đúng:** **C** — ConnectionStrings__Finance

- **Phương án C ĐÚNG:** Double underscore map nested config đa nền tảng.

- **A SAI:** VITE khiến giá trị public ở client.

- **B SAI:** Browser không nên chứa DB connection.

- **D SAI:** Dot không mặc định map hierarchical key.

---

Vị trí bài học: [LO-W21-03](../../notes/module-6/week-21.md#lo-w21-03)

Ôn [bài 3](../../notes/module-6/week-21.md) · LO-W21-03 · W21-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
