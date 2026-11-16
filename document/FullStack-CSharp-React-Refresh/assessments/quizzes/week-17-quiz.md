# Quiz tuần 17 — TypeScript, components và state

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-5/week-17.md)

### Q17-01 [LO-W17-01]

JSON cast bằng as ExpenseDto bảo đảm gì?

A. Tự parse ngày UTC

B. Chỉ thay cách compiler kiểm kiểu

C. Validate JSON thật

D. Tự gọi API

### Q17-02 [LO-W17-01]

Dữ liệu chưa tin cậy nên nhận kiểu gì?

A. never

B. ExpenseDto và tin tuyệt đối

C. unknown rồi narrow

D. any và bỏ checks

### Q17-03 [LO-W17-02]

Key nào giữ identity khi sort?

A. Index hiện tại

B. Math.random mỗi render

C. Amount có thể trùng

D. ID ổn định của expense

### Q17-04 [LO-W17-02]

Child muốn xóa row nên làm gì?

A. Gọi callback từ parent

B. Mutate props array

C. Query DOM xóa li

D. Gán lại props trực tiếp

### Q17-05 [LO-W17-03]

Total nên lấy từ đâu khi rows đủ dữ liệu?

A. Đọc text DOM

B. Derive từ rows bằng reduce

C. State total riêng không đồng bộ

D. Biến global mutable

### Q17-06 [LO-W17-03]

Xóa dựa trên rows trước dùng cách nào?

A. setRows(rows.push(x))

B. setState trong render vô điều kiện

C. setRows(old => old.filter(...))

D. rows.splice rồi không set
