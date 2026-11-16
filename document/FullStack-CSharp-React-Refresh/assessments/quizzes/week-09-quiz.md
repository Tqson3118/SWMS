# Quiz tuần 09 — SQL schema, constraints và normalization

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-3/week-09.md)

### Q09-01 [LO-W09-01]

CategoryId FK đơn lẻ bảo đảm cùng owner không?

A. Chỉ cần categoryName

B. Không

C. Có vì Id là PK

D. Có nếu frontend filter

### Q09-02 [LO-W09-01]

Số tiền nguyên theo spec SQL nên lưu gì?

A. float mặc định

B. date

C. bigint NOT NULL có CHECK

D. nvarchar đã format tiền

### Q09-03 [LO-W09-02]

CategoryName current lặp trong expenses nguy cơ gì?

A. Luôn query nhanh tuyệt đối

B. Tự có FK

C. Không thể insert dữ liệu

D. Update anomaly khi đổi tên không đồng bộ

### Q09-04 [LO-W09-02]

UNIQUE(UserId,Name) cho categories nhằm gì?

A. Cùng user không trùng tên, khác user có thể trùng

B. Cấm mọi Food trong hệ thống

C. Thay mọi PK

D. Cho null amount

### Q09-05 [LO-W09-03]

CHECK(amount>0) có đủ cấm NULL không?

A. Chỉ cần CSS required

B. Không, cần NOT NULL

C. Có mọi null tự thành0

D. Có CHECK luôn false cho NULL

### Q09-06 [LO-W09-03]

Temp demo đã đủ chứng minh FK SQL Server chưa?

A. Không cần kiểm DB

B. Chỉ nhìn diagram

C. Chưa, phải kiểm persistent lab tables

D. Đủ vì mọi temp FK enforced
