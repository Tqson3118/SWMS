# Đáp án tuần 27 — Queues, scaling và consistency trade-offs

[Quiz](../quizzes/week-27-quiz.md) · [Bài nguồn](../../notes/module-7/week-27.md)

## Ma trận đáp án

| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |
|---|---|---|---|---|---|
| Q27-01 | LO-W27-01 | Analyze | Accepted status | D | W27-C01 |
| Q27-02 | LO-W27-01 | Analyze | Worker ownership | A | W27-C01 |
| Q27-03 | LO-W27-02 | Analyze | Dual write | B | W27-C02 |
| Q27-04 | LO-W27-02 | Analyze | Delivery semantics | C | W27-C02 |
| Q27-05 | LO-W27-03 | Analyze | Scaling state | D | W27-C03 |
| Q27-06 | LO-W27-03 | Analyze | Pool budget | A | W27-C03 |



## Giải thích từng phương án

### Q27-01

202exportjob có nghĩa gì?

- **Đáp án đúng:** **D** — Accepted, chưa hoàn thành

- **Phương án D ĐÚNG:** Asyncwork cần status endpoint để theo dõi.

- **A SAI:** Accepted không completed.

- **B SAI:** Requestvẫn phảivalidate month.

- **C SAI:** Ownership áp dụng mọi endpoint.

---

Vị trí bài học: [LO-W27-01](../../notes/module-7/week-27.md#lo-w27-01)

Ôn [bài 1](../../notes/module-7/week-27.md) · LO-W27-01 · W27-C01.

### Q27-02

Downloadexportjob cần authorization không?

- **Đáp án đúng:** **A** — Có, scopeowner cả status/file

- **Phương án A ĐÚNG:** Privatefinanceartifact vẫn resourceowner.

- **B SAI:** GUIDkhóđoán không cấp quyền.

- **C SAI:** Acceptedstatus không bỏ authorization.

- **D SAI:** Fileformat không securityboundary.

---

Vị trí bài học: [LO-W27-01](../../notes/module-7/week-27.md#lo-w27-01)

Ôn [bài 1](../../notes/module-7/week-27.md) · LO-W27-01 · W27-C01.

### Q27-03

Outbox giải gap nào?

- **Đáp án đúng:** **B** — DBcommit và publishmessage riêng có thể lệch

- **Phương án B ĐÚNG:** Eventdurable cùng DB transaction giảm lostpublish.

- **A SAI:** UIpagination là requestcontract khác.

- **C SAI:** Outbox không authprotocol.

- **D SAI:** Indexing vẫn cần query workload.

---

Vị trí bài học: [LO-W27-02](../../notes/module-7/week-27.md#lo-w27-02)

Ôn [bài 2](../../notes/module-7/week-27.md) · LO-W27-02 · W27-C02.

### Q27-04

Crashafterpublish trước marksent tạo gì?

- **Đáp án đúng:** **C** — Có thể duplicatemessage; consumeridempotent

- **Phương án C ĐÚNG:** Dispatcherretry có thể phát lạievent.

- **A SAI:** Crashkhông undo committedSQL tựđộng.

- **B SAI:** Deletejobs không recovery đúng.

- **D SAI:** Publish/marksent không atomic vớibroker.

---

Vị trí bài học: [LO-W27-02](../../notes/module-7/week-27.md#lo-w27-02)

Ôn [bài 2](../../notes/module-7/week-27.md) · LO-W27-02 · W27-C02.

### Q27-05

Authoritativejobstate nên lưu đâu khi2replicas?

- **Đáp án đúng:** **D** — Shared durable SQLstore

- **Phương án D ĐÚNG:** Replicas cùngđọc/coordinate durablestate.

- **A SAI:** Replicakhác không nhìn memory A.

- **B SAI:** Clientkhông điều phối serverjob.

- **C SAI:** Staticlist mất khi restart vàkhôngshared.

---

Vị trí bài học: [LO-W27-03](../../notes/module-7/week-27.md#lo-w27-03)

Ôn [bài 3](../../notes/module-7/week-27.md) · LO-W27-03 · W27-C03.

### Q27-06

2 replicas × 100 connections có thể vượtDB120 không?

- **Đáp án đúng:** **A** — Có, cần budget cả API/workers

- **Phương án A ĐÚNG:** Tổng pool potential 200 cộng workers/admin.

- **B SAI:** AsyncIO vẫn dùngDBconnections.

- **C SAI:** JWTkhông tăngDBcapacity.

- **D SAI:** Cachekhông xóa mọiDBrequests.

---

Vị trí bài học: [LO-W27-03](../../notes/module-7/week-27.md#lo-w27-03)

Ôn [bài 3](../../notes/module-7/week-27.md) · LO-W27-03 · W27-C03.

## Kế hoạch sửa lỗi

Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.
