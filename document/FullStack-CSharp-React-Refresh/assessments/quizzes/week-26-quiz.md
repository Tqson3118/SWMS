# Quiz tuần 26 — Redis, cache-aside và consistency

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-7/week-26.md)

### Q26-01 [LO-W26-01]

Authoritative expenseamount nằm đâu?

A. BrowserlocalStorage

B. Charttext

C. SQL database có constraints/transactions

D. RedisTTLcache

### Q26-02 [LO-W26-01]

Monthlycachekey cần phần nào?

A. Chỉ report

B. Chỉ month mọi user

C. UserIdquery chưa validate

D. Schema/user/month/filter

### Q26-03 [LO-W26-02]

Invalidatecache vào thời điểm nào?

A. Sau SQLcommit rồi bỏ affectedkeys

B. Trước SQL write là đủ

C. Không bao giờ

D. Trước authcheck

### Q26-04 [LO-W26-02]

DELsaucommit bảo đảm zerostale không?

A. Có vì asyncawait

B. Không, late reader có thể SETold

C. Có luôn

D. Có nếu TTL60

### Q26-05 [LO-W26-03]

Redisoptionaldown nên xử lý sao?

A. Trả fakezero

B. Lặp connectvôhạn

C. Fallback SQL và metric/log bounded

D. Mất moneywrite

### Q26-06 [LO-W26-03]

Nhiều miss cùngkey gây gì?

A. Tự khóa SQL toàn cục

B. Tự refreshJWT

C. Không bao giờ load

D. Nhiều SQLreads đồng thời
