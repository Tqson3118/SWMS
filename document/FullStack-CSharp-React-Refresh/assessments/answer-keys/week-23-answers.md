# Đáp án tuần 23 — GitHub Actions và integration testing

[Quiz](../quizzes/week-23-quiz.md) · [Bài nguồn](../../notes/module-6/week-23.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q23-01 | LO-W23-01 | Analyze | Reproducibility | D | W23-C01 |
| Q23-02 | LO-W23-01 | Analyze | Fail gate | A | W23-C01 |
| Q23-03 | LO-W23-02 | Analyze | DB test double | B | W23-C02 |
| Q23-04 | LO-W23-02 | Analyze | Isolation | C | W23-C02 |
| Q23-05 | LO-W23-03 | Analyze | Fork security | D | W23-C03 |
| Q23-06 | LO-W23-03 | Analyze | Release trace | A | W23-C03 |



## Giải thích từng phương án

### Q23-01

Frontend CI nên cài dependencies bằng gì?

- **Đáp án đúng:** **D** — npm ci với committed lockfile

- **Phương án D ĐÚNG:** CI dùng graph locked và fail khi mismatch.

- **A SAI:** Local state không fresh reproducible.

- **B SAI:** Install có thể cập nhật lock graph.

- **C SAI:** Manifest khai báo scripts/dependencies.

---

Vị trí bài học: [LO-W23-01](../../notes/module-6/week-23.md#lo-w23-01)

Ôn [bài 1](../../notes/module-6/week-23.md) · LO-W23-01 · W23-C01.

### Q23-02

Build step exit nonzero nên làm gì?

- **Đáp án đúng:** **A** — Fail job và chặn rollout

- **Phương án A ĐÚNG:** Failure cần giữ evidence và sửa.

- **B SAI:** Ignore che regression ở pipeline.

- **C SAI:** Log giúp chẩn đoán failure.

- **D SAI:** Rollout failed artifact tăng rủi ro.

---

Vị trí bài học: [LO-W23-01](../../notes/module-6/week-23.md#lo-w23-01)

Ôn [bài 1](../../notes/module-6/week-23.md) · LO-W23-01 · W23-C01.

### Q23-03

EFInMemory chứng minh SQL constraints không?

- **Đáp án đúng:** **B** — Không, test relational provider thật

- **Phương án B ĐÚNG:** Provider fake khác relational semantics.

- **A SAI:** Fake không enforce mọi FK tương đương.

- **C SAI:** Rowversion do SQLServer thực thi.

- **D SAI:** InMemory không SQL queryplan.

---

Vị trí bài học: [LO-W23-02](../../notes/module-6/week-23.md#lo-w23-02)

Ôn [bài 2](../../notes/module-6/week-23.md) · LO-W23-02 · W23-C02.

### Q23-04

Integration test DB nên dùng gì?

- **Đáp án đúng:** **C** — Database test riêng có seed/reset

- **Phương án C ĐÚNG:** Isolation giữ tests deterministic/an toàn.

- **A SAI:** Local state khiến kết quả không tái hiện.

- **B SAI:** Order dependency tạo flaky behavior.

- **D SAI:** Tests không được sửa businessdata thật.

---

Vị trí bài học: [LO-W23-02](../../notes/module-6/week-23.md#lo-w23-02)

Ôn [bài 2](../../notes/module-6/week-23.md) · LO-W23-02 · W23-C02.

### Q23-05

ForkPR chưa tin cậy nên được gì?

- **Đáp án đúng:** **D** — Read-only verify, không deploy secrets

- **Phương án D ĐÚNG:** Untrusted code cần giới hạn credential.

- **A SAI:** Credentialadmin có thể bị exfiltrate.

- **B SAI:** PR không cần productiondata secret.

- **C SAI:** Least privilege giảm attack surface.

---

Vị trí bài học: [LO-W23-03](../../notes/module-6/week-23.md#lo-w23-03)

Ôn [bài 3](../../notes/module-6/week-23.md) · LO-W23-03 · W23-C03.

### Q23-06

Image nên nối release với gì?

- **Đáp án đúng:** **A** — CommitSHA và CIevidence

- **Phương án A ĐÚNG:** Có thể truy source/test của artifact.

- **B SAI:** Latest mutable khó biết code thật.

- **C SAI:** Screenshot không provenance artifact.

- **D SAI:** Ngày không xác định exact revision.

---

Vị trí bài học: [LO-W23-03](../../notes/module-6/week-23.md#lo-w23-03)

Ôn [bài 3](../../notes/module-6/week-23.md) · LO-W23-03 · W23-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
