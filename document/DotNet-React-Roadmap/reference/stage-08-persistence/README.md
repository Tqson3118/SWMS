# Stage 08 — Persistence slice (DN53-1)

Đây là reference dành cho giảng viên để học và kiểm tra ranh giới nhập CSV vào SQL Server. Phạm vi chỉ gồm parser đã được chấp nhận, EF Core migration, transaction và test LocalDB. File này không tuyên bố có API, UI, authentication, broker, payment hay production deployment.

## Mục tiêu học tập

Sau slice này, người học có thể giải thích vì sao phải kiểm tra toàn bộ input trước SQL, vì sao `decimal(18,2)` và `date` cần được giữ nguyên khi qua EF, và vì sao một transaction phải phân biệt lỗi trước commit, sau commit và kết quả commit không xác định. Người học cũng có thể đọc outcome code an toàn mà không làm lộ path, connection string hoặc exception detail.

Schema do `ImportDbContext` và migration sở hữu: `Categories` được seed với các category 1, 2, 7; `Expenses.Amount` là `decimal(18,2)`, giá trị tối đa `9999999999999999.99`; `SpentOn` là SQL `date`; `Note` bắt buộc, tối đa 500 ký tự; foreign key dùng `NoAction`. Caller được chọn giới hạn nhỏ hơn cho amount/note, nhưng không được mở rộng giới hạn SQL.

Input tối đa 1 MiB và 1000 physical data rows. Exact full-row duplicate có thể skip; mọi lỗi khác reject cả batch trước SQL. Service giữ read lease trên file thật khi parser chạy và so sánh độ dài trước/sau. Lỗi đọc, file bị thay đổi, cancellation khi còn partial output và validation lỗi đều không ghi row nào.

## Chạy một lần với bằng chứng riêng

Chạy PowerShell từ thư mục `reference/stage-08-persistence`. Mỗi lần dùng GUID mới; không dùng `-Force`, tên DB cố định, `DROP`, reset, reuse database hay xóa fixture. Các lệnh dừng ngay khi lỗi và ghi restore/build/test log cùng TRX vào thư mục GUID:

```powershell
$run = [guid]::NewGuid().ToString('N')
$repo = Resolve-Path '..\..\..'
$evidence = Join-Path $repo "tasks\course-remediation-2026-09-30\validation\dotnet-import-correct935\$run"
New-Item -ItemType Directory -Path $evidence -ErrorAction Stop | Out-Null
$env:DN53_FIXTURE_ROOT = Join-Path $evidence 'fixtures'
New-Item -ItemType Directory -Path $env:DN53_FIXTURE_ROOT -ErrorAction Stop | Out-Null
dotnet --version *>&1 | Tee-Object (Join-Path $evidence 'sdk.log')
if ($LASTEXITCODE -ne 0) { throw 'SDK check failed' }
dotnet restore .\ExpenseImportPersistence.csproj --verbosity minimal *>&1 | Tee-Object (Join-Path $evidence 'restore.log')
if ($LASTEXITCODE -ne 0) { throw 'restore failed' }
dotnet build .\ExpenseImportPersistence.csproj --no-restore --verbosity minimal *>&1 | Tee-Object (Join-Path $evidence 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'build failed' }
dotnet test .\ExpenseImportPersistence.csproj --no-restore --results-directory (Join-Path $evidence 'test-results') --logger 'trx' --verbosity minimal *>&1 | Tee-Object (Join-Path $evidence 'test.log')
if ($LASTEXITCODE -ne 0) { throw 'tests failed' }
```

`global.json` yêu cầu SDK `10.0.401` và project target `net10.0`. Mỗi fixture tạo tên `RoadmapImport915_` + 32 lowercase hex, in và ghi tên trước `CREATE DATABASE`, rồi giữ lại DB, CSV và log để reviewer đọc. Test guard tên sai không tạo DB; run 945 có 12 DB cho 13 test.

## Outcome và fault boundary

`committed` nghĩa transaction đã commit và không có lỗi hook sau đó. `committed-with-postcommit-error` nghĩa dữ liệu đã commit nhưng hook sau commit lỗi; caller không được coi đây là rollback. `Import commit outcome is unknown` nghĩa transport commit lỗi và service không đoán trạng thái server. Diagnostic sink chỉ nhận code cố định như `input-open-failed`, `persistence-failed`, `commit-outcome-unknown` và `postcommit-hook-failed`; public exception không chứa path, connection string hoặc exception detail. Diagnostic sink là secondary trace boundary: nếu sink tự lỗi, service nuốt lỗi đó và vẫn giữ outcome chính; vì vậy đây không phải cam kết mọi diagnostic đều được ghi. Các hook fault/cancellation là test-only injection để học boundary, không phải claim về broker hay payment.

## Map của 13 test thực tế (12 test tạo DB và 1 test guard)

| Test | Boundary |
|---|---|
| `Valid_import_applies_owned_schema_and_persists_exact_money_date_fk_and_note` | migration, decimal max, date, FK, note |
| `Exact_duplicates_are_the_only_errors_that_still_commit_unique_rows` | duplicate-only skip |
| `Validation_error_rejects_every_row_before_sql_effect` | unknown category, zero SQL rows |
| `Precommit_fault_rolls_back_rows_and_postcommit_cancellation_reports_committed` | precommit rollback, postcommit committed |
| `Cancellation_before_read_and_before_commit_leave_zero_rows` | cancellation before read/commit and partial parser output |
| `File_and_row_caps_are_rejected_without_sql_effect` | 1 MiB, 1000 rows, null options |
| `Caller_bounds_missing_file_and_null_categories_are_safe` | caller bounds, missing input, null categories |
| `Caller_stricter_amount_and_note_bounds_reject_without_sql_effect` | stricter amount/note and note 501 |
| `Read_fault_and_attempted_growth_reject_without_partial_sql` | injected read fault and file growth |
| `Postcommit_hook_failure_reports_committed_effects` | committed-with-postcommit-error |
| `Postcommit_diagnostic_failure_still_reports_committed_effects` | non-throwing diagnostic boundary after commit |
| `Database_guard_rejects_fixed_or_newline_names` | strict fresh-name guard |
| `Locked_input_returns_safe_error_and_private_diagnostic_code` | locked-file safe error and private diagnostic code |

Primary Microsoft documentation bodies, retrieval status and SHA-256 values are retained in the 915 evidence directory. The current source hashes and the complete pre-931 source snapshot are recorded in the 935 evidence directory. This slice remains a bounded teaching prerequisite, not course approval.
