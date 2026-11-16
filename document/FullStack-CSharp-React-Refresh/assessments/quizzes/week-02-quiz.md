# Quiz tuần 02 — JavaScript: scope, closure, array và tiền

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-1/week-02.md)

### Q02-01 [LO-W02-01]

Closure có giữ khả năng đọc biến lexical sau hàm ngoài return không?

A. Luôn copy deep mọi biến

B. Chỉ áp dụng cho var

C. Có, khi function còn reachable

D. Không, biến luôn bị hủy ngay

### Q02-02 [LO-W02-01]

const e={amount:1}; e.amount=2 có hợp lệ không?

A. Không vì const đóng băng object

B. Chỉ khi dùng var

C. Luôn tạo object mới

D. Có

### Q02-03 [LO-W02-02]

reduce cộng trên mảng rỗng với initial 0 trả gì?

A. 0

B. undefined

C. Luôn throw

D. 1

### Q02-04 [LO-W02-02]

[...xs] có clone sâu expense object không?

A. Nó trả string JSON

B. Không, object bên trong còn chung reference

C. Có, clone toàn bộ graph

D. Nó mutate xs ngay

### Q02-05 [LO-W02-03]

Vì sao check raw.trim() trước Number(raw)?

A. Number trả Promise

B. Rỗng tự thành 1

C. Chuỗi rỗng convert thành 0

D. Number luôn throw cho rỗng

### Q02-06 [LO-W02-03]

Hai object riêng {a:1} === {a:1} cho gì?

A. true vì nội dung giống

B. null

C. Throw vì object không so được

D. false
