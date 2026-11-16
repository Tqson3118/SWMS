# Quiz tuần 11 — Index, execution plan, pagination và N+1

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-3/week-11.md)

### Q11-01 [LO-W11-01]

Thêm index có tradeoff gì?

A. Mọi query luôn nhanh hơn

B. Write không đổi bất kể index

C. Không cần stats

D. Read có thể nhanh hơn, write/storage tốn hơn

### Q11-02 [LO-W11-01]

Filter tháng nên thử form nào?

A. SpentOn>=from AND SpentOn<to

B. YEAR(SpentOn)=y luôn seek tối ưu

C. Format date thành text rồi compare

D. Bỏ UserId để nhanh

### Q11-03 [LO-W11-02]

Index Seek đủ chứng minh query nhanh chưa?

A. Chỉ screenshot không dataset

B. Chưa, xem reads/rows/lookup/timing

C. Đủ icon là guarantee

D. Chỉ cost% chắc chắn latency

### Q11-04 [LO-W11-02]

Scan small table luôn cần sửa không?

A. Có đổi thành NoSQL luôn

B. Có thêm100indexes

C. Không, có thể plan hợp lý

D. Có mọi scan là bug

### Q11-05 [LO-W11-03]

ORDER BY date có ties thì thêm gì?

A. Random() mỗi query

B. Không cần ORDER BY

C. Chỉ tăng pageSize

D. Unique Id tie-breaker

### Q11-06 [LO-W11-03]

List100rows rồi fetch category từngrow nên sửa gì?

A. Projection/JOIN theo dữ liệu cần, đo query count

B. Tăng timeout vô hạn

C. Load toàn DB vào memory

D. Thêm index frontend
