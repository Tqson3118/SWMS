# Quiz tuần 12 — Transactions, concurrency và data correctness

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-3/week-12.md)

### Q12-01 [LO-W12-01]

2writes nghiệp vụ cần all-or-nothing thì làm gì?

A. Cùng transaction boundary

B. 2connections autocommit độc lập

C. Chỉ try/catch client

D. Chỉ disable nút

### Q12-02 [LO-W12-01]

SQL transaction có tự rollback email đã gửi không?

A. Có nếu timestamp giống

B. Không

C. Có mọi external effect chungACID

D. Có nếu dùng async

### Q12-03 [LO-W12-02]

SQL Server rowversion là gì?

A. User JWT

B. Số tiền

C. Binary concurrency version, không thời gian

D. Ngày giờ UTC

### Q12-04 [LO-W12-02]

UPDATE với oldversion mismatch có affectedrows gì?

A. Luôn1

B. Tự merge fields

C. Tự trả HTTP201

D. 0

### Q12-05 [LO-W12-03]

Same key/different payload nên xử lý gì?

A. 409 conflict

B. Import lần nữa

C. Ghi đè result cũ silent

D. Chỉ đổi userId từ body

### Q12-06 [LO-W12-03]

Check exists trước insert đã đủ chống concurrent duplicate chưa?

A. Đủ nếu LINQ Any trảfalse

B. Chưa, cần unique/atomicity

C. Đủ vì request chạy lần lượt luôn

D. Đủ nếu print log
