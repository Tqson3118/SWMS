# Expense UI — Stage 04 frontend reference

Ứng dụng React + TypeScript dùng Vite kết nối với API `/api/v1`. Tên thư mục `reference/stage-04-frontend` là tên lịch sử của reference path; các bài học gốc `5.md`, `6.md`, `7.md` và DOCX tương ứng vẫn là nguồn giảng dạy riêng và không được sửa trong correction này.

## 1. Contract phía client

`src/api.ts` đọc danh sách dạng JSON array và lấy phân trang từ `X-Total-Count`, `X-Page`, `X-Page-Size`, `X-Total-Pages`. Các response phải được parse runtime; lỗi ProblemDetails/ValidationProblemDetails giữ lại thông báo nhập liệu khi POST/PUT thất bại.

`VITE_API_BASE_URL` là override rõ ràng theo origin, không chứa `/api/v1`; `VITE_API_URL` chỉ là alias cũ khi biến ưu tiên vắng mặt. Khi base rỗng, Vite proxy `/api` tới `VITE_API_TARGET`. Target phải là origin HTTP loopback có port cụ thể; không có default tự đoán. Không đặt secret trong `VITE_*`. Cross-origin API phải cho phép origin dev (`http://localhost:5173` hoặc `http://localhost:3000`) và expose các header phân trang cùng `Location`.

Response money fields `amount`, category `total` và `grandTotal` là decimal strings. Client giữ giá trị ở dạng lexical decimal string, thực hiện cộng/so sánh bằng BigInt cents, gửi request amount dạng string, và format đủ hai chữ số thập phân bằng dấu phân cách địa phương cùng ký hiệu tiền. Chart percentages chỉ là tỷ lệ gần đúng; label và tổng vẫn giữ exact value. `125.50` phải hiển thị `125,50 ₫`, không thành `126`.

Các thành phần chính:

- `src/api.ts`: fetch, DTO parsing, pagination headers và ProblemDetails.
- `src/components/ExpenseForm.tsx`: tạo/sửa khoản chi, kiểm tra client-side và giữ lỗi input.
- `src/components/ExpenseList.tsx`: list array, filter ngày, pagination, delete và các trạng thái loading/empty/error/success.
- `src/components/MonthlySummary.tsx`: totals/counts và chart tỷ lệ gần đúng.
- `src/App.tsx`: điều hướng và phối hợp các trạng thái dữ liệu.

## 2. Chạy đúng cổng thực tế bằng PowerShell

Khởi động API trước, đọc URL loopback mà API in ra, rồi chạy frontend với chính port đó:

```powershell
Set-Location 'D:\03_Study\Roadmap\DotNet-React-Roadmap\reference\stage-04-api'
dotnet --version                 # expected: 10.0.401
dotnet run --project '.\src\ExpenseApi\ExpenseApi.csproj'

# In a second PowerShell window, replace <printed-port> with the actual API port.
Set-Location 'D:\03_Study\Roadmap\DotNet-React-Roadmap\reference\stage-04-frontend'
$env:VITE_API_TARGET = 'http://localhost:<printed-port>'
npm.cmd install
npm.cmd run dev
```

Dev server mặc định là `http://localhost:5173`. Không dùng cổng API mặc định đoán hoặc cổng từ evidence cũ. Khi kết thúc một run, dừng cả hai process và giữ database/log evidence; không reset, drop hoặc reuse database cũ.

## 3. Kiểm tra nguồn hiện tại

```powershell
Set-Location 'D:\03_Study\Roadmap\DotNet-React-Roadmap\reference\stage-04-frontend'
npx.cmd tsc --noEmit
npm.cmd run build
npm.cmd test
```

Reference suite hiện tại có 6 test files và 21/21 tests pass. Focused evidence `validation/dotnet-wire-tests851` có thêm 4 wire tests cho POST/PUT exact money strings và 1 behavioral test cho date-filter event; đó là evidence riêng, không phải lần chạy lại full suite.

## 4. Runtime evidence đã lưu

`validation/dotnet-browser849/results.json` là run hiện tại bounded trên API thật và database LocalDB mới được giữ lại. Nó ghi nhận initial empty state, POST `125.50`, PUT `125.75`, SQL maximum `9999999999999999.99`, edit giữ exact digits, monthly summary vượt giới hạn một dòng SQL nhưng đúng bằng BigInt, date filter/reset và CORS header exposure. `validation/dotnet-money-runtime847` là evidence lịch sử trước đó, được giữ để đối chiếu chứ không mở rộng claim.

Build và typecheck không thay thế browser journey. Evidence hiện tại chưa bao gồm browser DELETE thực tế, production authentication, payment, cloud/container deployment, later-stage approval, learner completion hoặc whole-course approval. API/DB runtime là local teaching reference; không phải production payment precision.

