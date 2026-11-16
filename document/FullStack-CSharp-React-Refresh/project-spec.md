# Dự án xuyên suốt: Sổ chi tiêu cá nhân
## Phạm vi bắt buộc
Một người có nhiều danh mục và khoản chi; hai user không được đọc/sửa dữ liệu nhau. Tiền nguyên VND, ngày chi DateOnly/ISO yyyy-MM-dd; createdAt UTC. Không kết nối ngân hàng, không xử lý thanh toán thật.

## Quy tắc và dữ liệu mẫu
amountVnd: số nguyên 1..1.000.000.000; note trim, tối đa 200 ký tự; category/name 1..60. Danh mục phải thuộc user đang đăng nhập. Tổng báo cáo backend dùng long + checked, frontend phải kiểm tra safe integer. Không cộng tiền bằng float rồi làm tròn để che lỗi.
Alice có Food (f1), Travel (t1); Bob có Food (f2). Alice: 2026-10-01 Food 50000, 2026-10-02 Travel 20000, 2026-10-03 Food 30000. Tổng Alice 100000, Food 80000, Travel 20000; Bob rỗng. Đây là fixture chuẩn cho các gate.
Budget Food tháng 2026-10 = 70000 -> vượt 10000 (14,29% so budget). Budget zero hiển thị số tuyệt đối, không chia zero.
Giao dịch: expenses + import batch/idempotency record phải commit/rollback cùng nhau. Một batch có tối đa 1000 dòng; amount/date/category sai -> báo số dòng, không import nửa chừng. Hủy trước commit -> không lưu; hủy sau commit -> tra batchId để biết trạng thái.

## Data model
Users(Id uniqueidentifier PK, Issuer nvarchar(200), Subject nvarchar(200), UNIQUE(Issuer,Subject)).
Categories(Id uniqueidentifier PK, UserId FK, Name nvarchar(60), UNIQUE(UserId,Name), UNIQUE(Id,UserId)).
Expenses(Id uniqueidentifier PK, UserId FK, CategoryId, AmountVnd bigint CHECK >0 AND <=1000000000, SpentOn date, Note nvarchar(200), CreatedAt datetime2, Version rowversion).
FK(CategoryId,UserId) -> Categories(Id,UserId) bảo vệ ownership ở DB. Không cascade xóa category có expense; API trả conflict, cho archive tùy chọn.
Budgets(UserId,CategoryId,MonthStart date,LimitVnd bigint CHECK >=0, UNIQUE(UserId,CategoryId,MonthStart)).
ImportBatches(UserId,IdempotencyKey unique,RequestHash,Status,CreatedAt). Bắt UNIQUE(UserId,IdempotencyKey), không dựa chỉ vào if-exists trước insert.
Production userId là internal Guid map bằng (issuer,subject) từ JWT hợp lệ; không lấy userId từ body. Dev local token dùng claim mapped Guid riêng chỉ trong environment Development.

## API contract thống nhất
- GET /api/expenses?from=2026-10-01&to=2026-11-01&page=1&pageSize=20: from inclusive, to exclusive. Response {items,totalCount,page,pageSize}; stable ORDER BY SpentOn DESC,Id DESC; pageSize 1..100.
- POST /api/expenses: {categoryId,amountVnd,spentOn,note}; 201 và Location.
- GET /api/expenses/{id}: 200 hoặc 404 theo scope user.
- PUT /api/expenses/{id}: update đầy đủ writable fields + version Base64; 409 nếu stale.
- DELETE /api/expenses/{id}: 204 nếu user sở hữu; 404 nếu không thấy theo scope.
- GET/POST /api/categories; GET/PUT /api/budgets/{month}/{categoryId}.
- GET /api/reports/monthly?month=2026-10: {totalVnd,byCategory,budgets}.
- POST /api/imports: JSON {rows:[...]} và Idempotency-Key header. Client đọc CSV có header categoryId,amountVnd,spentOn,note; dùng thư viện CSV chuẩn khi note chứa dấu phẩy/quote. Server nhận JSON đã parse và validate lại.
Error: ProblemDetails (type,title,status,detail,instance,traceId); 400 validation, 401 token không hợp lệ, 403 policy, 404 scoped absence, 409 conflict, 429 limit, 500 generic. Không trả stack/secret. DELETE 204 không parse JSON.
Note hiển thị text, không HTML. Thống kê cache key bao gồm userId,month và report version; không cache dữ liệu user dưới key global.

## Kiến trúc và lịch tăng trưởng
W01–04 JS mock UI; W05–08 domain console; W09–12 DB/scripts; W13–16 API/EF/auth/test; W17–20 thay UI bằng React; W21–24 vận hành; W25–28 thiết kế/cache/portfolio. Domain chứa luật, API chứa transport, Infrastructure chứa SQL/Redis; không interface hóa mọi class.
Repo học viên riêng expense-ledger/ với src/Expense.Api, src/Expense.Domain, tests/Expense.Tests, web, ops, docs. Khóa học chỉ cung cấp hướng dẫn và ví dụ, học viên tự hiện thực dự án.

## Definition of Done feature
Có input/output cụ thể, happy + empty + invalid + forbidden + failure case; code giải thích được; test behavior; diff nhỏ; README chạy từ máy mới; không secret; lưu evidence screenshot/log/test gắn commit. Feature mới không làm mất fixture tuần trước. Quy tắc ownership, atomicity và token validation là gate critical, điểm quiz không bù được.


## Bổ sung W26–W27
Report cache TTL<=60 giây theo ADR, Redis lỗi thì fallback SQL. DEL sau commit vẫn có late-reader race; cần test và ghi allowed staleness, hoặc dùng uncached/version generation khi freshness bắt buộc.
POST /api/export-jobs nhận tháng,202+Location; GET status/download đều scope owner, artifact private có retention. Baseline SQL durable jobs+BackgroundService; broker/outbox external là tùy chọn.202 không nghĩa đã hoàn tất; consumer/worker idempotent theo JobId, không claim exactly-once end-to-end.
