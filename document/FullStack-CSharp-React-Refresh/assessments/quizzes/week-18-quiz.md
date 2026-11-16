# Quiz tuần 18 — Hooks, routing và API lifecycle

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-5/week-18.md)

### Q18-01 [LO-W18-01]

POST do user bấm submit nên đặt đâu?

A. Effect mỗi render

B. Module global startup

C. Trong event handler

D. Render body

### Q18-02 [LO-W18-01]

Effect fetch trả cleanup để làm gì?

A. Undo mọi server commit

B. Tắt TypeScript

C. Giữ request chạy mãi

D. Abort và ignore kết quả cũ

### Q18-03 [LO-W18-02]

SPA refresh route cần gì ở host?

A. Fallback index.html cho UI routes

B. Fallback mọi /api sang HTML

C. Chỉ mở root được

D. Lưu route trong JWT

### Q18-04 [LO-W18-02]

Guard client thay auth API không?

A. Có nếu URL khó đoán

B. Không, API tự kiểm quyền

C. Có nếu button ẩn

D. Có nếu router private

### Q18-05 [LO-W18-03]

fetch gặp HTTP500 thường làm gì?

A. Luôn parse ProblemDetails

B. Tự retry không giới hạn

C. Resolve Response cần kiểm ok

D. Luôn reject

### Q18-06 [LO-W18-03]

DELETE204 nên xử lý body thế nào?

A. Parse JSON rỗng

B. Gán error500

C. Đòi backend gửi {}

D. Không gọi json, trả undefined
