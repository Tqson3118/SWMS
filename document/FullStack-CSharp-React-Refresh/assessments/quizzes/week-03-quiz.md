# Quiz tuần 03 — DOM, sự kiện, modules và persistence nhỏ

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-1/week-03.md)

### Q03-01 [LO-W03-01]

preventDefault trên submit chặn gì?

A. Mọi event bubbling

B. Mọi lỗi validation

C. Mọi request mạng trong app

D. Hành vi mặc định gửi/reload

### Q03-02 [LO-W03-01]

Render note chưa tin cậy nên dùng gì?

A. textContent

B. innerHTML trực tiếp

C. eval note

D. document.write note

### Q03-03 [LO-W03-02]

Named export total import đúng thế nào?

A. Đọc window.total luôn có

B. import {total} from './money.js'

C. import total from './money.js' nếu không default

D. require trong browser module mặc định

### Q03-04 [LO-W03-02]

Tại sao dùng HTTP server local cho module?

A. Để khỏi cần import

B. Để tắt strict mode

C. Để origin/MIME/path hoạt động như web

D. Để tự cung cấp mọi API

### Q03-05 [LO-W03-03]

JSON.parse('{}') thành công có đủ nhận expenses không?

A. Có vì JSON luôn array

B. Có vì browser kiểm tra domain

C. Chỉ cần rename key

D. Không, phải validate shape

### Q03-06 [LO-W03-03]

localStorage phù hợp lab này cho gì?

A. Expense giả lập đã validate

B. Secret signing key

C. JWT dài hạn như chuẩn an toàn

D. Password plaintext
