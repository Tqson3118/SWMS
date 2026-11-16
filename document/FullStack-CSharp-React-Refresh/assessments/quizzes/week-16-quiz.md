# Quiz tuần 16 — API tests và atomic/idempotent import

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-4/week-16.md)

### Q16-01 [LO-W16-01]

Unitmock đủ chứng minh FKSQL chưa?

A. Chưa

B. Đủ100%coverage

C. Đủmocksuccess

D. Đủmigrationfile

### Q16-02 [LO-W16-01]

Integration DB nên dùng gì?

A. Randomconnection không record

B. Isolated lab DB và owned fixtures

C. Productiondefault

D. SharedfriendDB

### Q16-03 [LO-W16-02]

Batch 3 rows, 1 invalid all-or-nothing làm gì?

A. Đổi0thành1

B. Bỏownership

C. Báo row error và0newrows

D. Lưu 2 rows silent

### Q16-04 [LO-W16-02]

Samekey/hash 3 rows retry tạo tổng mấy expense?

A. 6

B. 9

C. 0

D. 3

### Q16-05 [LO-W16-03]

Fault debug endpoint deploy public không?

A. Không

B. Có đểdemo

C. Có nameshort

D. Có 500 generic

### Q16-06 [LO-W16-03]

ẢnhUI đủ chứng minh bad signature → 401 không?

A. Đủ React compile

B. Không,cần real auth test

C. Đủspinner

D. Đủhidebutton
