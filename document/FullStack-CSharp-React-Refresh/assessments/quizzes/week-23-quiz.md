# Quiz tuần 23 — GitHub Actions và integration testing

6 câu, chọn một phương án và ghi lý do. [Bài học](../../notes/module-6/week-23.md)

### Q23-01 [LO-W23-01]

Frontend CI nên cài dependencies bằng gì?

A. Copy local node_modules

B. npm install rồi không review

C. Không cần package.json

D. npm ci với committed lockfile

### Q23-02 [LO-W23-01]

Build step exit nonzero nên làm gì?

A. Fail job và chặn rollout

B. Thêm ||true

C. Xóa failed log

D. Deploy vẫn tiếp

### Q23-03 [LO-W23-02]

EFInMemory chứng minh SQL constraints không?

A. Có FK như SQLServer

B. Không, test relational provider thật

C. Có đầy đủ rowversion

D. Có actualexecutionplan

### Q23-04 [LO-W23-02]

Integration test DB nên dùng gì?

A. DBcá nhân bất kỳ

B. Chạy phụ thuộc thứ tự

C. Database test riêng có seed/reset

D. Productionshared

### Q23-05 [LO-W23-03]

ForkPR chưa tin cậy nên được gì?

A. Cloudadminsecret

B. ProductionSQLpassword

C. Write-alltoken mặc định

D. Read-only verify, không deploy secrets

### Q23-06 [LO-W23-03]

Image nên nối release với gì?

A. CommitSHA và CIevidence

B. Tên latest duy nhất

C. Ảnh screenshot không version

D. Dateđơn lẻ
