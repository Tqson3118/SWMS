# Quiz tuần 10 — JOIN, GROUP BY, subqueries và CTE

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-3/week-10.md)

### Q10-01 [LO-W10-01]

WHERE child.amount>0 sau LEFT JOIN làm gì với parent rỗng?

A. Tự chuyển amount null->0

B. Tạo child mới

C. Loại parent rỗng vì predicate không true

D. Giữ mọi parent bắt buộc

### Q10-02 [LO-W10-01]

Category2 expenses JOIN trả2rows có phải SQL bug?

A. Có, luôn1row/category

B. Có nên bỏ FK

C. Có nên SUM DISTINCT(amount) luôn

D. Không, đúng one-to-many cardinality

### Q10-03 [LO-W10-02]

LEFT JOIN category rỗng muốn expense count0 dùng gì?

A. COUNT(child.Id)

B. COUNT(*)

C. COUNT(1)

D. SUM(1)

### Q10-04 [LO-W10-02]

Lọc category total>=50000 nên dùng gì?

A. LIMIT total

B. HAVING SUM(amount)>=50000

C. WHERE amount>=50000 trước group như tương đương

D. ORDER BY50000

### Q10-05 [LO-W10-03]

CTE luôn materialize và cache query không?

A. Có mọi CTE chạy nhanh hơn

B. Có transaction tự mở

C. Không

D. Có persistent table tự tạo

### Q10-06 [LO-W10-03]

Chỉ cần category có expense nên EXISTS ích gì?

A. Tự trả mọi child fields

B. Tự index DB

C. Tự bỏ authorization

D. Membership không nhân duplicate parent
