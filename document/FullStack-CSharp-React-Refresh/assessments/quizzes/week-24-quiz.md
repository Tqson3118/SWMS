# Quiz tuần 24 — Cloud deployment, monitoring và recovery

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-6/week-24.md)

### Q24-01 [LO-W24-01]

SPA APIURL đổi lúc nào với Vite build-timeenv?

A. Build/rebuild client đúng publicURL

B. Chỉ đổi API containerenv

C. Gửi DBpassword vào client

D. Tắt HTTPS

### Q24-02 [LO-W24-01]

Production dùng dev user-jwts được không?

A. Có nếu API public

B. Không, provider token validation thật

C. Có mãi mãi

D. Có nếu đổi tên claim

### Q24-03 [LO-W24-02]

DBdown nên phản ánh ở đâu cho API cần DB?

A. Không log bao giờ

B. Trả 200 fake expense

C. Readiness và dependency metrics

D. Livenessalwaysrestartstorm

### Q24-04 [LO-W24-02]

Production nên tránh log gì?

A. TraceID

B. Statuscode

C. Operationname

D. Bearer token và finance note

### Q24-05 [LO-W24-03]

Có backupfile đã đủ recovery gate chưa?

A. Chưa, phải restore và kiểm dữ liệu

B. Đủ tuyệt đối

C. Chỉ kiểm tênfile

D. Không cần constraints

### Q24-06 [LO-W24-03]

Rollback image có undo destructive SQL tự động không?

A. Có CDNcache

B. Không, cần compatibility/recovery plan

C. Có EF luônrollback

D. Có DockerundoDB
