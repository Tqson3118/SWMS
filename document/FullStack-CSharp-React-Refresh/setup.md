# Chuẩn bị và cách chạy ví dụ
## Công cụ
Cài .NET SDK 10 (TFM net10.0), Node LTS đáp ứng Vite hiện tại, Git, editor, SQL Server Developer/Express hoặc container SQL Server, Python 3 + PyYAML để kiểm tra học liệu. SQL Server LocalDB dùng Windows khi thiếu RAM/Docker; nếu container SQL không chạy trên kiến trúc máy, dùng SQL cài Windows. Redis/Docker chỉ cần từ W22/W26.
Kiểm tra dotnet --info, node --version, npm --version, git --version; ghi phiên bản vào docs/environment.md. .NET 10 là LTS theo policy Microsoft kiểm tra 2026-10-03; không dùng preview. Package EF/provider/Design cùng 10.x; pin exact patch đã restore thành công và commit lockfile.
Nguồn: https://dotnet.microsoft.com/en-us/platform/support/policy ; https://vite.dev/guide/ ; https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb .

## Repo học viên (tạo ngoài thư mục học liệu)
```powershell
mkdir expense-ledger
cd expense-ledger
git init
mkdir web
mkdir src
mkdir tests
dotnet new console -n Expense.Console -o src/Expense.Console -f net10.0
dotnet new classlib -n Expense.Domain -o src/Expense.Domain -f net10.0
dotnet add src/Expense.Console reference src/Expense.Domain
dotnet new xunit -n Expense.Tests -o tests/Expense.Tests -f net10.0
dotnet add tests/Expense.Tests reference src/Expense.Domain
dotnet run --project src/Expense.Console
dotnet test tests/Expense.Tests
```
.NET 10 có thể tạo solution .slnx; dùng dotnet new sln rồi dotnet sln add các project nếu muốn, không phụ thuộc extension .sln.
W01: viết index.html/style.css/app.js trong web. Mở server HTTP từ web bằng python -m http.server 5500; truy cập http://localhost:5500 . Module/fetch không mở bằng file://. Stop server bằng Ctrl+C.
JS console ví dụ chạy bằng browser DevTools. C# ví dụ top-level thay Program.cs console; types khai báo sau top-level. Snippet có db/app/builder là đoạn trong API, không phải console độc lập. SQL chạy trong DB ExpenseLab mới, chỉ dữ liệu fixture.
W13: dotnet new webapi --use-controllers -n Expense.Api -o src/Expense.Api -f net10.0 ; dotnet add src/Expense.Api reference src/Expense.Domain . .NET template OpenAPI không đảm bảo có Swagger UI; dùng curl/HTTP file trước.
W17: giữ bản JS trong commit/tag, tạo web-react bằng npm create vite@latest web-react -- --template react-ts ; cd web-react ; npm install ; npm run dev . Đọc requirement Node của Vite trước chạy, commit package-lock.json.
W14: cài Microsoft.EntityFrameworkCore.SqlServer và Microsoft.EntityFrameworkCore.Design major 10; tạo tool manifest local với dotnet new tool-manifest; dotnet tool install dotnet-ef --version 10.* rồi pin patch thực cài trong manifest. Commands migration cần --project src/Expense.Api ở repo root.
Đặt connection string Development bằng dotnet user-secrets; production dùng secret store/env ConnectionStrings__ExpenseDb. Không ghi secret vào VITE_ vì frontend public.
## Khi gặp lỗi
Không chạy được dotnet: kiểm tra SDK và target; restore mismatch: xem major; module JS: Network MIME/path; CORS: đúng origin và scheme/port; SQL login: DB/instance/connection string và TLS certificate dev; port in use: đổi port đồng bộ client/API. Ghi nguyên văn lỗi và bước tái hiện trước hỏi AI.
