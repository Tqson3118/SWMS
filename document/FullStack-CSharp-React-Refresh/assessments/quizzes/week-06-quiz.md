# Quiz tuần 06 — Collections, generics và delegates

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-2/week-06.md)

### Q06-01 [LO-W06-01]

Lookup Dictionary nên mô tả chi phí thế nào?

A. Luôn O(n²)

B. Không cần equality

C. Trung bình O(1), không tuyệt đối mọi case

D. Luôn O(1) bất kể hash

### Q06-02 [LO-W06-01]

Xóa List trong foreach nên đổi cách nào?

A. Bọc try/catch rồi bỏ lỗi

B. Đổi var thành int

C. Thêm Thread.Sleep

D. RemoveAll hoặc strategy không mutate enumerator đang chạy

### Q06-03 [LO-W06-02]

where T:new() cho phép điều gì?

A. new T() với constructor phù hợp

B. Gọi mọi member tùy ý

C. Bảo đảm deep immutable

D. Tự kết nối DB

### Q06-04 [LO-W06-02]

IReadOnlyList<T> bảo đảm source không đổi bởi bất kỳ ai không?

A. Nó cấm mọi reference type

B. Không

C. Có freeze deep

D. Có thread-safe luôn

### Q06-05 [LO-W06-03]

Func<long,bool> biểu diễn gì?

A. Không trả value

B. Luôn async

C. Nhận long và trả bool

D. Nhận bool trả long

### Q06-06 [LO-W06-03]

Lambda capture threshold rồi threshold đổi có thể thấy gì?

A. Luôn snapshot cũ

B. Luôn null

C. Compile lỗi vì capture cấm

D. Value mới của captured variable
