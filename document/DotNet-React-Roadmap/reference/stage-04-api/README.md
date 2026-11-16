# Expense API — Stage 04 reference

Đây là reference API ASP.NET Core .NET 10 (`net10.0`) hiện tại cho hệ thống quản lý chi tiêu. Tên thư mục `reference/stage-04-api` là tên lịch sử của gói tham chiếu; mã chạy hiện tại dùng EF Core với `ExpenseDbContext` và `EfExpenseService`. Các file bài học gốc `5.md`, `6.md`, `7.md` và DOCX tương ứng là nguồn giảng dạy riêng, không bị thay đổi bởi reference này.

## 1. Runtime và database

- SDK và TFM: .NET SDK `10.0.401`, `net10.0`; kiểm tra bằng `dotnet --version` từ thư mục dự án.
- `src/ExpenseApi/Program.cs` đăng ký `ExpenseDbContext` với SQL Server và chọn `EfExpenseService` qua DI.
- `src/ExpenseApi/Data/ApiDatabase.cs` chỉ chấp nhận tên mới dạng `RoadmapApi779_<32 chữ hex thường>`, kiểm tra database trùng tên trong `master` trước khi tạo và ghi tên trước bước tạo. Database dùng `(localdb)\\MSSQLLocalDB`, integrated security và được giữ lại để kiểm tra.
- Schema được áp dụng bằng `Database.MigrateAsync()`, với hai migration hiện có: `20261007083824_InitialCreate` và `20261007083840_AddExpenseCreatedAtUtc`. `EnsureCreated` không phải bằng chứng migration.
Mỗi lần chạy phải dùng database mới và giữ lại database cùng log. Không dùng database cố định cũ, không `DROP`, reset hoặc xóa database bằng hướng dẫn này. Có thể ghi đường dẫn log tên database bằng biến PowerShell `DN45_API779_DB_LOG` trước khi chạy.

## 2. API contract (`/api/v1`)

| Method | Route | Kết quả chính |
|---|---|---|
| `GET` | `/api/v1/categories` | Danh sách danh mục |
| `POST` | `/api/v1/categories` | Tạo danh mục, `200` |
| `GET` | `/api/v1/expenses?page=1&pageSize=10&from=YYYY-MM-DD&to=YYYY-MM-DD` | JSON array `ExpenseResponse`; metadata ở `X-Total-Count`, `X-Page`, `X-Page-Size`, `X-Total-Pages`; thứ tự `SpentOn` giảm dần rồi `Id` giảm dần |
| `GET` | `/api/v1/expenses/{id}` | Chi tiết, `200` hoặc `404` |
| `GET` | `/api/v1/expenses/summary?year=2026&month=10` | Tổng tháng theo danh mục, `200` hoặc `400` |
| `POST` | `/api/v1/expenses` | Tạo khoản chi, `201` với `Location`, hoặc `400` |
| `PUT` | `/api/v1/expenses/{id}` | Cập nhật, `200`, `400` hoặc `404`; lỗi không làm đổi bản ghi cũ |
| `DELETE` | `/api/v1/expenses/{id}` | Xóa, `204` hoặc `404` |

Các response lỗi dùng ProblemDetails/ValidationProblemDetails an toàn, có `traceId` để đối chiếu log. Không đưa stack trace, secret, connection string hoặc request body nhạy cảm vào response/log.

### Money and date transport

Money response properties `amount`, category `total` và `grandTotal` là JSON decimal strings, ví dụ `"125.50"`. Request amount chấp nhận số JSON legacy và decimal string chính xác; giá trị phải không âm, tối đa `decimal(18,2)` và không quá hai chữ số thập phân. ID, count, year và month vẫn là JSON numbers. Đây là dữ liệu chi tiêu dùng cho bài học, không phải cam kết tiền tệ hoặc payment processing.

`spentOn` là ngày `DateOnly` hợp lệ; thiếu trường và ngày hợp lệ tối thiểu `0001-01-01` là hai trường hợp khác nhau. `categoryId` phải tồn tại và `note` có thể null nhưng tối đa 500 ký tự. Lọc ngày, phân trang và summary phải giữ đúng contract trên.

## 3. Chạy bằng PowerShell

```powershell
Set-Location 'D:\03_Study\Roadmap\DotNet-React-Roadmap\reference\stage-04-api'
dotnet --version                 # expected: 10.0.401
$env:DN45_API779_DB_LOG = (Join-Path (Get-Location) 'runtime-database.log')
dotnet run --project '.\src\ExpenseApi\ExpenseApi.csproj'
```

Đọc dòng startup để lấy database mới và URL loopback thực tế được in ra. API dùng cổng ephemeral; không đoán `5000`, `5080` hoặc cổng cũ. Khi gọi từ frontend khác origin, API cho phép `http://localhost:5173` và `http://localhost:3000`, đồng thời expose các header phân trang và `Location`.

Kiểm thử tham chiếu, khi cần chạy lại trong một evidence scope mới, dùng đường dẫn dự án hiện tại và tên TRX duy nhất:

```powershell
Set-Location 'D:\03_Study\Roadmap\DotNet-React-Roadmap\reference\stage-04-api'
dotnet test '.\tests\ExpenseApi.Tests\ExpenseApi.Tests.csproj' --no-restore --logger 'trx;LogFileName=runtime-<unique>.trx'
```

Không ghi đè raw stdout/stderr/TRX cũ. Bộ API hiện tại có 23/23 test pass trong evidence đã lưu; con số này mô tả reference runtime, không phải kết quả của người học.

## 4. Runtime evidence đã lưu

`validation/dotnet-browser849/results.json` là runtime hiện tại bounded: database mới được giữ lại, API `http://127.0.0.1:50699`, frontend `http://localhost:5173`, tạo/sửa khoản chi, giá trị phân số `125.50`, giá trị tối đa `9999999999999999.99`, lọc ngày, summary BigInt, CORS header exposure và type/build checks. Focused evidence `validation/dotnet-wire-tests851` bổ sung 4 wire tests cho POST/PUT money strings và 1 behavioral date-filter test; không chạy lại full suite.

`validation/dotnet-money-runtime847` là evidence lịch sử được giữ nguyên, với database và cổng khác. Nó hỗ trợ các bounded claims tương ứng, không phải bằng chứng không giới hạn cho toàn bộ course. Chưa có trong các evidence này: production authentication, external payment, cloud/container deployment, actual browser DELETE journey, later-stage acceptance, learner completion hoặc whole-course approval.

