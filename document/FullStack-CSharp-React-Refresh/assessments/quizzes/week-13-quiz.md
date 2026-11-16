# Quiz tuần 13 — REST, middleware, DI và API errors

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-4/week-13.md)

### Q13-01 [LO-W13-01]

POST tạo resource thành công trả gì?

A. 301 redirect login

B. 201 và Location

C. 200 cả khi lỗi

D. 204 bắt buộc JSON body

### Q13-02 [LO-W13-01]

Client UserId quyết owner có đúng không?

A. Có nếu hidden input

B. Có nếu CORS bật

C. Không, owner từ validated principal

D. Có nếu Guid

### Q13-03 [LO-W13-02]

Authentication đứng đâu so authorization?

A. Sau controller response

B. Sau UseAuthorization

C. Không cần vì JWT JSON

D. Trước authorization

### Q13-04 [LO-W13-02]

CORS có thay ownership không?

A. Không

B. Có chặn mọi curl

C. Có validate signature

D. Có tự filter SQL

### Q13-05 [LO-W13-03]

Unexpected exception production nên trả gì?

A. 404 mọi failure

B. 500 generic ProblemDetails

C. 200 có stack

D. Token để debug

### Q13-06 [LO-W13-03]

Log nên giữ gì?

A. Connection string

B. Mọi note plaintext

C. Operation/ID/trace, không secrets

D. Authorization header
