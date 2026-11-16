# Quiz tuần 27 — Queues, scaling và consistency trade-offs

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-7/week-27.md)

### Q27-01 [LO-W27-01]

202exportjob có nghĩa gì?

A. Filechắc đãsẵn

B. Validationkhông cần

C. Authbịbỏqua

D. Accepted, chưa hoàn thành

### Q27-02 [LO-W27-01]

Downloadexportjob cần authorization không?

A. Có, scopeowner cả status/file

B. Không vìJobIdGUID

C. Không nếu202

D. Không nếuCSV

### Q27-03 [LO-W27-02]

Outbox giải gap nào?

A. Tự UIpagination

B. DBcommit và publishmessage riêng có thể lệch

C. Tự encryptiontoken

D. Tự indexmọibảng

### Q27-04 [LO-W27-02]

Crashafterpublish trước marksent tạo gì?

A. Rollback mọi SQL cũ

B. Xóa tất cảjobs

C. Có thể duplicatemessage; consumeridempotent

D. Exactlyonceguarantee

### Q27-05 [LO-W27-03]

Authoritativejobstate nên lưu đâu khi2replicas?

A. MemoryreplicaA

B. Browseronly

C. Localstaticmutablelist

D. Shared durable SQLstore

### Q27-06 [LO-W27-03]

2 replicas × 100 connections có thể vượtDB120 không?

A. Có, cần budget cả API/workers

B. Không vìasync

C. Không vìJWT

D. Không vìRedis
