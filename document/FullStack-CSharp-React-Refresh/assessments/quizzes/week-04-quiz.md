# Quiz tuần 04 — Promises, fetch và event loop

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-1/week-04.md)

### Q04-01 [LO-W04-01]

async function return 3 cho caller nhận gì?

A. Promise fulfilled với 3

B. Số3 trực tiếp không Promise

C. Thread mới

D. undefined luôn

### Q04-02 [LO-W04-01]

Một Promise reject trong Promise.all có tự hủy mọi request khác không?

A. Có chuyển mọi result thành []

B. Không

C. Có tự AbortController

D. Có rollback database

### Q04-03 [LO-W04-02]

fetch trả HTTP404 thì cần làm gì?

A. Dùng no-cors để đọc mọi response

B. Parse JSON hai lần

C. Check response.ok rồi throw/handle

D. Chờ catch luôn tự bắt404

### Q04-04 [LO-W04-02]

Hủy POST phía browser chứng minh DB không ghi chưa?

A. Chắc chắn không ghi

B. Luôn undo commit

C. JWT tự bị thu hồi

D. Chưa, server có thể đã commit

### Q04-05 [LO-W04-03]

A sync, timer T, Promise.then P, B sync thường log gì?

A. A B P T

B. A T P B

C. P A B T

D. A B T P

### Q04-06 [LO-W04-03]

Loop nặng trước await có block UI không?

A. Không nếu return Promise

B. Có

C. Không vì async luôn background

D. Chỉ khi var
