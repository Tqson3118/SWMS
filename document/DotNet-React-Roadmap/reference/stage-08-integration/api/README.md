# Stage08 — nhập CSV qua API và SQL cục bộ

Lát cắt này nối parser đã học với transaction SQL và HTTP. Mục tiêu là gửi một file, đọc lỗi theo dòng, kiểm tra các expense đã lưu và phân biệt hủy trước commit với kết quả chưa xác nhận. Dự án dùng SDK10.0.401/net10.0, SQL Server LocalDB, EF Core10.0.12; parser/persistence được dùng qua project reference, không tạo bản parser khác.

Đây là fixture giảng dạy một người học trên `http://127.0.0.1:0`; server tự chọn cổng và in URL. Category thuộc server:1,2,7. Tên database chỉ `RoadmapImport915_<32 chữ hex thường>`; LocalDB Integrated Security, tên mới, migration sở hữu. Không có authentication/ownership sản xuất ở Stage08; học phần đó ở Stage11. Request không chọn user, database hoặc fault hook. Không dùng database thật/Visualization/learner cũ.

## Chạy test — mỗi lần một thư mục evidence mới

Mở PowerShell tại thư mục này. LocalDB phải sẵn có. Dùng các lệnh dưới đây; không chạy lại vào folder/database cũ và không xóa fixture sau test.

```powershell
$apiStudyId = [guid]::NewGuid().ToString('N')
$apiStudyEvidence = Join-Path (Get-Location) ('learner-evidence-' + $apiStudyId)
New-Item -ItemType Directory -Path $apiStudyEvidence -ErrorAction Stop
$env:DN54_EVIDENCE_ROOT = $apiStudyEvidence
dotnet --version 2>&1 | Tee-Object (Join-Path $apiStudyEvidence 'sdk.log')
dotnet restore .\Stage8IntegrationApi.csproj 2>&1 | Tee-Object (Join-Path $apiStudyEvidence 'restore.log')
if ($LASTEXITCODE -ne 0) { throw 'Restore failed; stop and keep the log' }
dotnet build .\Stage8IntegrationApi.csproj --no-restore 2>&1 | Tee-Object (Join-Path $apiStudyEvidence 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'Build failed; stop and keep the log' }
dotnet test .\Stage8IntegrationApi.csproj --no-build --logger ('trx;LogFileName=' + $apiStudyId + '.trx') --results-directory $apiStudyEvidence --logger 'console;verbosity=normal' 2>&1 | Tee-Object (Join-Path $apiStudyEvidence 'test.log')
if ($LASTEXITCODE -ne 0) { throw 'Tests failed; keep stdout, stderr and TRX' }
```

Test tự tạo các tên database mới riêng, log tên trước tạo và giữ lại. Callback/EF interceptor trong tests là fault injection qua DI của instructor; client HTTP không bật được chúng. Guard unit tests không cần SQL. Kestrel test chạy một helper riêng và dừng đúng helper đó.

## Chạy server để thực hành thủ công

Tạo **một run mới khác test**:

```powershell
$apiDemoId = [guid]::NewGuid().ToString('N')
$apiDemoEvidence = Join-Path (Get-Location) ('demo-evidence-' + $apiDemoId)
New-Item -ItemType Directory -Path $apiDemoEvidence -ErrorAction Stop
$env:DN54_DB_NAME = 'RoadmapImport915_' + $apiDemoId
$env:DN54_FIXTURE_ROOT = Join-Path $apiDemoEvidence 'retained'
New-Item -ItemType Directory -Path $env:DN54_FIXTURE_ROOT -ErrorAction Stop
$env:DN54_LISTEN_URL = 'http://127.0.0.1:0'
dotnet run --no-build --project .\Stage8IntegrationApi.csproj 2>&1 | Tee-Object (Join-Path $apiDemoEvidence 'server.log')
```

Giữ terminal này mở; lấy URL `DN54_LISTENING=http://127.0.0.1:<port>` từ log. Không cấu hình `Kestrel:Endpoints` khác; guard từ chối endpoint override. Tên database đã tồn tại bị từ chối trước migration; không đổi tên để truy cập database cũ.

Ở terminal thứ hai, dùng file UTF8 mới có header chính xác, không BOM:

```csv
categoryId,amount,spentOn,note
1,12.50,2026-09-22,lunch
2,8.00,2026-09-23,bus
```

Gán `$apiBase` bằng URL thực tế và `$apiCsv` bằng đường dẫn file bạn vừa tạo:

```powershell
curl.exe -i -F ('file=@' + $apiCsv + ';type=text/csv') ($apiBase + '/api/v1/expenses/import')
curl.exe ($apiBase + '/api/v1/expenses')
```

File tối đa1MiB; toàn multipart request tối đa1MiB+64KiB (1,114,112bytes), gồm field/header/overhead. Server kiểm tra byte thực, lưu vào tên GUID mới và kiểm tra lại kích thước trước parse. Tối đa1000 dòng dữ liệu vật lý sau header, kể cả dòng trống/lặp. Note tối đa500 ký tự; amount không âm, tối đa9999999999999999.99 và hai số thập phân; date `yyyy-MM-dd`. Note có dấu phẩy phải quote theo CSV. File rỗng/header-only là committed no-op0 rows. Duplicate có category/amount/date/note giống nhau trong **cùng file** được skip; không có dedup giữa hai lần POST.

Thử một category9 trong batch có dòng đúng: response400 cho dòng lỗi và SQL không thêm expense nào. Thử hai dòng giống nhau: một accepted, một skipped. Mỗi thử tạo file/run evidence mới, giữ log và đối chiếu GET. Không gửi POST lặp để đoán lần trước thành công hay chưa.

## Response contract

`POST /api/v1/expenses/import`, multipart field`file`. Tất cả response do endpoint xử lý có sáu field:

```json
{"status":"committed","readRows":2,"acceptedRows":2,"skippedDuplicates":0,"errors":[],"traceId":"..."}
```

Error: `{"lineNumber":3,"code":"category","message":"..."}`. Request/file errors có lineNumbernull. Dòng header là1; data bắt đầu2. Số accepted=0 ở rejection nghĩa là0 expense được ghi, không phải parser không nhìn thấy dòng hợp lệ. Status/counts:

| HTTP | status | Ý nghĩa/counts |
|---|---|---|
|200|committed|Commit được xác nhận; counts là số; no-op0 là hợp lệ.|
|200|committed-with-postcommit-error|Rows đã commit; follow-up hook lỗi; có safe error code. Không gửi lại POST.|
|400/413|rejected|Không ghi expense; safe errors;413 cho toàn request vượt cap.|
|499|cancelled|Server quan sát token hủy trước commit;0 accepted; JSON có traceId nếu kết nối còn đọc được.|
|500|unknown|Không xác nhận được outcome; cả ba countsnull. Refresh để đối chiếu; không suy ra rollback.|

```json
{"status":"unknown","readRows":null,"acceptedRows":null,"skippedDuplicates":null,"errors":[{"lineNumber":null,"code":"outcome_unknown","message":"The import outcome could not be confirmed. Refresh before deciding what to do next."}],"traceId":"..."}
```

`GET /api/v1/expenses` trả array `{id,categoryId,amount,spentOn,note}`. Amount luôn string như`"12.50"`/`"9999999999999999.99"`; giữ string trong browser, không `Number`/`parseFloat`. SpentOn là date-only string. GET không phân trang trong fixture nhỏ này; đây không là bằng chứng tải lớn.

## Hủy, unknown và đối chiếu

Browser abort chỉ cho biết browser ngừng chờ. Server có thể đã commit; UI phải giữ trạng thái chưa xác nhận cho đến refresh. Server test có connected token hủy trước upload, trong upload/parser và trước commit; mỗi case quan sát0 rows. Sau commit xác nhận, token hủy/follow-up lỗi không biến rows thành rollback.

Test EF postcommit interceptor mô phỏng mất xác nhận sau SQL commit: caller nhậnunknown, observer GET thấy row. SaveChanges fault mô phỏng khác nhậnunknown và observer thấy0; sự khác nhau chứng minh rằng unknown không là một kết quả database cụ thể. Không có bằng chứng live transport failure, receipt/idempotency/exactly-once hay payment.

Trong fixture một người học này, đối chiếu GET trước/sau bằng note/date/amount giúp quyết định thủ công. GET không chứng minh một request cụ thể đã commit khi nhiều dữ liệu giống nhau; giữ `unconfirmed` nếu không xác định được. Không tự động retry POST. Nếu muốn thực hành lần mới, dùng một fixture mới, giữ toàn bộ evidence trước đó.

## Evidence và giới hạn

Current runtime suite sau source guard992 có build/TRX mới; đọc số thực trong TRX thay vì cộng các focused run vào full suite. Historical98624tests và root987six startup guards là các evidence riêng. Runtime RED973 tái hiện empty499body trước sửa; guard992 missing-helper RED là compile-time feature gap, không là proof đã bind an unsafe listener. Source tests gồm CSV limits/errors/exact money/no-op/duplicates, connected cancellation, fault simulations, actual SQL observations và real Kestrel body-cap. Same-length file replacement và live commit transport failure chưa được quan sát. File retain/private fixture không là distributed import service. Browser/frontend và toàn Stage08/course cần gate riêng; reference này không tự phê duyệt course.
