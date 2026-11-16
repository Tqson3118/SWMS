# Quiz tuần 05 — C# value/reference, OOP và invariants

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-2/week-05.md)

### Q05-01 [LO-W05-01]

b=a với a class rồi b.Amount=2 thường đổi a.Amount không?

A. Class luôn copy từng field

B. Có, nếu cùng instance

C. Không vì assignment deep clone

D. Chỉ khi dùng static

### Q05-02 [LO-W05-01]

Có nên khẳng định value type luôn nằm stack không?

A. Chỉ enum luôn vậy

B. C# cấm value field trong class

C. Không

D. Có bất kể boxing/field

### Q05-03 [LO-W05-02]

Setter amount public bỏ guard nguy cơ gì?

A. Tự tăng performance chắc chắn

B. Tự bảo đảm OOP

C. Tự tạo transaction

D. Caller tạo trạng thái invalid

### Q05-04 [LO-W05-02]

ReportService dùng reader hợp với quan hệ nào?

A. Has-a qua composition

B. Is-a DbContext

C. Kế thừa React component

D. Không cần boundary nào

### Q05-05 [LO-W05-03]

Core lưu tiền VND theo spec bằng gì?

A. int không cần giới hạn

B. long nguyên có guard

C. double rồi bỏ phần lẻ

D. string tùy ý

### Q05-06 [LO-W05-03]

Test nào kiểm tra domain rule?

A. Chỉ file tồn tại

B. Chỉ tên class đúng

C. Amount0 bị chặn,50000 được nhận

D. Chỉ getter trả value mặc định
