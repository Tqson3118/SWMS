# Quiz tuần 19 — Forms, authentication và edit conflicts

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-5/week-19.md)

### Q19-01 [LO-W19-01]

Number('') có vấn đề gì cho amount?

A. Ném exception luôn

B. Ra undefined

C. Bảo đảm valid amount

D. Ra0 và mất phân biệt input trống

### Q19-02 [LO-W19-01]

PUT stale version nên làm gì ở UI?

A. Giữ input và cho reload/compare

B. Tự overwrite không hỏi

C. Xóa form ngay

D. Đổi thành POST tạo trùng

### Q19-03 [LO-W19-02]

Public SPA nên chứa client secret không?

A. Có nếu HTTPS

B. Không, dùng code flow PKCE

C. Có nếu minify

D. Có nếu env VITE_

### Q19-04 [LO-W19-02]

API bearer nhận loại token nào?

A. JSON payload tự ký giả

B. Refresh token gửi mọi GET

C. Access token đúng API audience

D. ID token thay access token

### Q19-05 [LO-W19-03]

Đổi Alice sang Bob cần gì?

A. Giữ global total cache

B. Chỉ đổi avatar

C. Chỉ đổi URL

D. Clear/cancel data và key theo user

### Q19-06 [LO-W19-03]

Token hết hạn thường trả gì?

A. 401

B. 403 mọi lần

C. 200 error

D. 404 mọi endpoint
