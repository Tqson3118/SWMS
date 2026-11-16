# Quiz tuần 25 — HTTP, rate limiting và practical design

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-7/week-25.md)

### Q25-01 [LO-W25-01]

HTTP400invalidamount nên retry tự động không?

A. Có vì GET luôn

B. Không, sửa input

C. Có mãi đến200

D. Có bằng tokenmới

### Q25-02 [LO-W25-01]

Ownerfinance report có được sharedpubliccache không?

A. Có vì JWTdecode

B. Có nếu JSONnhỏ

C. Không, dùng private/no-store policy phù hợp

D. Có cùngURL mọi user

### Q25-03 [LO-W25-02]

Lab muốn rate reject429 cần gì?

A. Luôn framework429 mặc định

B. Trả 200 warning

C. Chỉ log quota

D. Đặt RejectionStatusCode=429

### Q25-04 [LO-W25-02]

Limiter memory trên 2 replicas có globalquota không?

A. Không, mỗi process giữ state riêng

B. Có tự syncRedis

C. Có loadbalancer cộng count

D. Có vì cùng code

### Q25-05 [LO-W25-03]

100active×2req/min tương đương RPS nào?

A. 0.033RPS

B. Khoảng3.33

C. 200RPS

D. 12000RPS

### Q25-06 [LO-W25-03]

Trước chọn microservices nên làm gì?

A. Thêm mọi queue

B. Bỏ ownerprivacy

C. Làm rõ requirements/constraints và đo bottleneck

D. Vẽ 20 services ngay
