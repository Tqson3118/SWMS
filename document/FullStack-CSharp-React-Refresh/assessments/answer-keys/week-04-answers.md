# Đáp án tuần 04 — Promises, fetch và event loop

[Quiz](../quizzes/week-04-quiz.md) · [Bài nguồn](../../notes/module-1/week-04.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q04-01 | LO-W04-01 | Analyze | Async return | A | W04-C01 |
| Q04-02 | LO-W04-01 | Analyze | Promise all | B | W04-C01 |
| Q04-03 | LO-W04-02 | Analyze | HTTP failure | C | W04-C02 |
| Q04-04 | LO-W04-02 | Analyze | Abort semantics | D | W04-C02 |
| Q04-05 | LO-W04-03 | Analyze | Event order | A | W04-C03 |
| Q04-06 | LO-W04-03 | Analyze | CPU work | B | W04-C03 |



## Giải thích từng phương án

### Q04-01

async function return 3 cho caller nhận gì?

- **Đáp án đúng:** **A** — Promise fulfilled với 3

- **Phương án A ĐÚNG:** Async bao return value trong Promise.

- **B SAI:** Caller cần await/then để lấy value.

- **C SAI:** async không mặc định tạo thread.

- **D SAI:** Return3 không tự thành undefined.

---

Vị trí bài học: [LO-W04-01](../../notes/module-1/week-04.md#lo-w04-01)

Ôn [bài 1](../../notes/module-1/week-04.md) · LO-W04-01 · W04-C01.

### Q04-02

Một Promise reject trong Promise.all có tự hủy mọi request khác không?

- **Đáp án đúng:** **B** — Không

- **Phương án B ĐÚNG:** Fail-fast không điều khiển cancellation operations khác.

- **A SAI:** Rejection là lỗi, không mảng rỗng mặc định.

- **C SAI:** Abort phải nối explicit signal.

- **D SAI:** Promise không tự có transaction DB.

---

Vị trí bài học: [LO-W04-01](../../notes/module-1/week-04.md#lo-w04-01)

Ôn [bài 1](../../notes/module-1/week-04.md) · LO-W04-01 · W04-C01.

### Q04-03

fetch trả HTTP404 thì cần làm gì?

- **Đáp án đúng:** **C** — Check response.ok rồi throw/handle

- **Phương án C ĐÚNG:** HTTP response resolve nên check status.

- **A SAI:** Opaque response không mở quyền đọc.

- **B SAI:** Body stream chỉ đọc một lần theo cách này.

- **D SAI:** Fetch không reject chỉ vì404.

---

Vị trí bài học: [LO-W04-02](../../notes/module-1/week-04.md#lo-w04-02)

Ôn [bài 2](../../notes/module-1/week-04.md) · LO-W04-02 · W04-C02.

### Q04-04

Hủy POST phía browser chứng minh DB không ghi chưa?

- **Đáp án đúng:** **D** — Chưa, server có thể đã commit

- **Phương án D ĐÚNG:** Client cancellation không có rollback guarantee.

- **A SAI:** Network abort có thể đến sau write.

- **B SAI:** Commit không tự undo bởi fetch.

- **C SAI:** Abort không liên quan token revocation.

---

Vị trí bài học: [LO-W04-02](../../notes/module-1/week-04.md#lo-w04-02)

Ôn [bài 2](../../notes/module-1/week-04.md) · LO-W04-02 · W04-C02.

### Q04-05

A sync, timer T, Promise.then P, B sync thường log gì?

- **Đáp án đúng:** **A** — A B P T

- **Phương án A ĐÚNG:** Stack xong A/B rồi microtask P trước task T.

- **B SAI:** Timer không chen giữa sync cùng lượt.

- **C SAI:** Promise reaction không trước sync đang chạy.

- **D SAI:** Microtask drain trước timer task ở ví dụ.

---

Vị trí bài học: [LO-W04-03](../../notes/module-1/week-04.md#lo-w04-03)

Ôn [bài 3](../../notes/module-1/week-04.md) · LO-W04-03 · W04-C03.

### Q04-06

Loop nặng trước await có block UI không?

- **Đáp án đúng:** **B** — Có

- **Phương án B ĐÚNG:** JS CPU vẫn chạy main execution cho tới yield.

- **A SAI:** Promise wrapper không đổi nơi loop thực thi.

- **C SAI:** Async không tự offload CPU.

- **D SAI:** var/let không quyết định CPU scheduling.

---

Vị trí bài học: [LO-W04-03](../../notes/module-1/week-04.md#lo-w04-03)

Ôn [bài 3](../../notes/module-1/week-04.md) · LO-W04-03 · W04-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
