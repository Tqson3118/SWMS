# Đáp án tuần 19 — Forms, authentication và edit conflicts

[Quiz](../quizzes/week-19-quiz.md) · [Bài nguồn](../../notes/module-5/week-19.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q19-01 | LO-W19-01 | Analyze | Input parsing | D | W19-C01 |
| Q19-02 | LO-W19-01 | Analyze | 409 recovery | A | W19-C01 |
| Q19-03 | LO-W19-02 | Analyze | SPA credential | B | W19-C02 |
| Q19-04 | LO-W19-02 | Analyze | Token kind | C | W19-C02 |
| Q19-05 | LO-W19-03 | Analyze | Session cache | D | W19-C03 |
| Q19-06 | LO-W19-03 | Analyze | 401 versus403 | A | W19-C03 |



## Giải thích từng phương án

### Q19-01

Number('') có vấn đề gì cho amount?

- **Đáp án đúng:** **D** — Ra0 và mất phân biệt input trống

- **Phương án D ĐÚNG:** Cần kiểm chuỗi rỗng trước parse.

- **A SAI:** Number empty string không throw.

- **B SAI:** Conversion này trả số0.

- **C SAI:** Project yêu cầu integer1..1e9.

---

Vị trí bài học: [LO-W19-01](../../notes/module-5/week-19.md#lo-w19-01)

Ôn [bài 1](../../notes/module-5/week-19.md) · LO-W19-01 · W19-C01.

### Q19-02

PUT stale version nên làm gì ở UI?

- **Đáp án đúng:** **A** — Giữ input và cho reload/compare

- **Phương án A ĐÚNG:** User cần quyết khi có concurrent edit.

- **B SAI:** Overwrite che mất update mới.

- **C SAI:** Mất input khiến khó reconcile.

- **D SAI:** Create khác ý nghĩa edit resource.

---

Vị trí bài học: [LO-W19-01](../../notes/module-5/week-19.md#lo-w19-01)

Ôn [bài 1](../../notes/module-5/week-19.md) · LO-W19-01 · W19-C01.

### Q19-03

Public SPA nên chứa client secret không?

- **Đáp án đúng:** **B** — Không, dùng code flow PKCE

- **Phương án B ĐÚNG:** Browser code không giữ secret an toàn.

- **A SAI:** TLS không giấu bundle khỏi người dùng.

- **C SAI:** Minify không ngăn inspect bundle.

- **D SAI:** VITE public variables có trong client.

---

Vị trí bài học: [LO-W19-02](../../notes/module-5/week-19.md#lo-w19-02)

Ôn [bài 2](../../notes/module-5/week-19.md) · LO-W19-02 · W19-C02.

### Q19-04

API bearer nhận loại token nào?

- **Đáp án đúng:** **C** — Access token đúng API audience

- **Phương án C ĐÚNG:** Access token dùng để gọi protected API.

- **A SAI:** API phải validate issuer/signature.

- **B SAI:** Refresh token thuộc token endpoint flow.

- **D SAI:** ID token chứng minh login cho client.

---

Vị trí bài học: [LO-W19-02](../../notes/module-5/week-19.md#lo-w19-02)

Ôn [bài 2](../../notes/module-5/week-19.md) · LO-W19-02 · W19-C02.

### Q19-05

Đổi Alice sang Bob cần gì?

- **Đáp án đúng:** **D** — Clear/cancel data và key theo user

- **Phương án D ĐÚNG:** Ngăn dữ liệu session cũ hiển thị.

- **A SAI:** Global cache trộn chủ sở hữu.

- **B SAI:** Avatar không xóa finance state.

- **C SAI:** URL không reset user-scoped requests.

---

Vị trí bài học: [LO-W19-03](../../notes/module-5/week-19.md#lo-w19-03)

Ôn [bài 3](../../notes/module-5/week-19.md) · LO-W19-03 · W19-C03.

### Q19-06

Token hết hạn thường trả gì?

- **Đáp án đúng:** **A** — 401

- **Phương án A ĐÚNG:** Không còn authenticated hợp lệ.

- **B SAI:** 403 dành principal valid thiếu quyền.

- **C SAI:** Status200 không truyền failure đúng.

- **D SAI:** 404 scoped dùng resource visibility.

---

Vị trí bài học: [LO-W19-03](../../notes/module-5/week-19.md#lo-w19-03)

Ôn [bài 3](../../notes/module-5/week-19.md) · LO-W19-03 · W19-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
