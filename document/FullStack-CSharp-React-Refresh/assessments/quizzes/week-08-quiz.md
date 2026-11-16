# Quiz tuần 08 — Task, cancellation, disposal và DI

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-2/week-08.md)

### Q08-01 [LO-W08-01]

Task luôn là thread riêng không?

A. Không

B. Có mỗi Task một OS thread

C. Chỉ Task<int> là thread

D. await tăng CPU core

### Q08-02 [LO-W08-01]

Tại sao tránh .Result trong web service?

A. Nó tự hủy token

B. Nó block thread và có rủi ro context/deadlock

C. Nó luôn trả null

D. Nó validate JWT

### Q08-03 [LO-W08-02]

Token cancel có cưỡng chế mọi function dừng không?

A. Có rollback commit đã xong

B. Có dispose mọi object trong app

C. Không, code/API phải quan sát token

D. Có kill mọi thread

### Q08-04 [LO-W08-02]

using cho stream bảo đảm gì khi exception trong scope?

A. GC chạy ngay toàn heap

B. Exception bị nuốt

C. File data luôn rollback

D. Dispose stream khi thoát scope

### Q08-05 [LO-W08-03]

Scoped ở ASP.NET thường dùng instance ra sao?

A. Một instance mỗi request scope

B. Một instance toàn process

C. Mới mỗi dòng code

D. Một instance mỗi table

### Q08-06 [LO-W08-03]

Singleton giữ DbContext scoped nên xử lý thế nào?

A. Dispose DbContext ngay constructor

B. Đổi lifetime hoặc tạo scope đúng boundary job

C. Để mặc định vì singleton thread-safe

D. Khóa mọi HTTP request toàn app
