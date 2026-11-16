# Quiz tuần 07 — LINQ, nullable và exception boundaries

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-2/week-07.md)

### Q07-01 [LO-W07-01]

Tạo Where trên List có luôn chạy filter ngay không?

A. Luôn copy source ngay

B. Luôn gọi DB

C. Luôn tạo thread

D. Thường chưa, tới enumeration

### Q07-02 [LO-W07-01]

q deferred tạo từ50000/20000 rồi thêm30000; snapshot trước thêm tổng gì?

A. 50000

B. 80000

C. 20000

D. 0

### Q07-03 [LO-W07-02]

NRT thay validation runtime không?

A. Chỉ cần ! để an toàn

B. Không

C. Có cấm deserialization thiếu field

D. Có mọi null đều exception compile

### Q07-04 [LO-W07-02]

note?.Trim()??'' khi note=null trả gì?

A. null vì ?? không hoạt động

B. Tạo string 'null'

C. Chuỗi rỗng

D. Throw luôn

### Q07-05 [LO-W07-03]

Rethrow trong catch nên dùng gì để giữ stack?

A. throw ex;

B. return success;

C. throw new Exception() không inner

D. throw;

### Q07-06 [LO-W07-03]

Ngày invalid nên xử lý ra sao ở import?

A. TryParseExact và row/field error

B. Silently đổi thành hôm nay

C. ! để hết warning

D. Catch mọi lỗi rồi bỏ dòng không báo
