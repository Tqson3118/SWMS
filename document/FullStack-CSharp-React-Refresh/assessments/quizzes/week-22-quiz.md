# Quiz tuần 22 — Docker Compose và persistence

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-6/week-22.md)

### Q22-01 [LO-W22-01]

Multi-stage build giúp gì?

A. Tự cấp JWT

B. Tự triển khai cloud

C. Chỉ copy runtime artifacts sang final image

D. Tự backup DB

### Q22-02 [LO-W22-01]

COPY .env vào image có an toàn không?

A. Có vì container private

B. Có vì minify

C. Có nếu xóa ở layer sau

D. Không, secret có thể tồn tại layers

### Q22-03 [LO-W22-02]

API container nối SQL Compose bằng gì?

A. db:1433

B. localhost:1433

C. Browser localhost5173

D. IP user laptop

### Q22-04 [LO-W22-02]

Named volume có phải backup đủ không?

A. Không cần restoretest

B. Không, cần bản backup/restore độc lập

C. Có bảo đảm mọi disaster

D. Có vì cùng host

### Q22-05 [LO-W22-03]

depends_on mặc định chứng minh DBready không?

A. Chỉ cần sleep1s

B. Không thể kiểm readiness

C. Không, cần healthcheck/condition hoặc retry

D. Có ngay lập tức

### Q22-06 [LO-W22-03]

Production nhiều replicas nên migrate thế nào?

A. Mọi replica tự migrate cùng lúc

B. Xóa DB trước mỗi start

C. Bỏ migrations khỏi repo

D. Một job có review trước rollout
