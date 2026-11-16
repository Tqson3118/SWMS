# Tham chiếu xác thực và ownership — bản học tập đang kiểm tra

Đây là fixture học tập giới hạn cho .NET 10 / ASP.NET Core Identity. Tài khoản `learner-a@example.test` và `learner-b@example.test` là dữ liệu hư cấu; mật khẩu được sinh trong bộ nhớ. Không tự viết hash, cookie ticket, token hay mã hóa.

Cookie Identity có `HttpOnly`, `Secure`, `SameSite=Strict`; Data Protection dùng `UseEphemeralDataProtectionProvider()` để TestServer không phụ thuộc key profile. POST/PUT/DELETE dưới `/api` yêu cầu antiforgery token thật; thiếu hoặc sai token trả 400 cố định. Exception invariant/storage đi qua safe 500 middleware, không trả stack/path/credential.

Owner chỉ lấy từ `ClaimTypes.NameIdentifier`; body/path `OwnerUserId` bị bỏ qua. GET/PUT gắn owner ngay trong SQL query. Anonymous trả 401; foreign/unknown trả 404. Amount phải có tối đa hai chữ số thập phân và note tối đa 500 ký tự; JSON trả amount dạng chuỗi `0.00`.

Startup bắt buộc `ROADMAP_AUTH_DB=RoadmapAuth11_<32 lowercase hex>`, kiểm tra master trước rồi chạy migration bằng `MigrateAsync` (không `EnsureCreated`), và dùng Integrated Security. Mọi URL cấu hình được kiểm tra bằng cả tên biến môi trường double-underscore và configuration; mọi child dưới `Kestrel:Endpoints` đều bị từ chối. Fixture chỉ cho `http://127.0.0.1:0`; TestServer HTTPS URI không phải TLS/browser proof.

## Lệnh chạy

Từ `D:\03_Study\Roadmap`:

```powershell
dotnet restore .\DotNet-React-Roadmap\reference\stage-11-auth\Stage11Auth.csproj
$evidenceName = 'stage11-' + [Guid]::NewGuid().ToString('N')
$evidenceDir = Join-Path '.\DotNet-React-Roadmap\reference\stage-11-auth\.learning-evidence' $evidenceName
if (Test-Path -LiteralPath $evidenceDir) { throw 'Thư mục bằng chứng phải mới.' }
New-Item -ItemType Directory -Path $evidenceDir | Out-Null
dotnet test .\DotNet-React-Roadmap\reference\stage-11-auth\Stage11Auth.csproj --no-restore --logger "trx;LogFileName=stage11.trx" --results-directory $evidenceDir
```

Package tham chiếu là Identity/EF/Testing 10.0.1; SDK là 10.0.401, shared runtime là 10.0.12. Đây là fixture source, chưa phải production compatibility approval.

Bằng chứng guard đã chấp nhận kiểm tra cấu hình startup và biến môi trường độc lập, kể cả trường hợp giá trị môi trường hợp lệ che một cấu hình URL không hợp lệ. Tiến trình startup bị từ chối trước marker `AUTH_DB_BEFORE_CREATE`; các ca guard không tạo host hoặc database. Đây là bằng chứng guard riêng, không suy ra từ số test.

Bằng chứng cookie dùng `IOptionsMonitor<CookieAuthenticationOptions>` và `TicketDataFormat` thật của production để bảo vệ `AuthenticationTicket`; không cài auth handler giả, không inject principal, không tự viết mã hóa. Các ca được chấp nhận gồm owner hợp lệ tới 404 khi thiếu expense, `NameIdentifier` thiếu/rỗng/whitespace trả 401, owner hợp lệ hết hạn trả 401, và đăng nhập sai trả 401 không phát cookie production, không lộ password/email và không đổi snapshot tài khoản. Control đăng nhập hợp lệ trả 200 rồi endpoint protected trả 404. Đây là các run riêng, không phải một run gộp mới.

Bằng chứng storage/CSRF xác nhận trong SQL owner lấy từ identity, body owner giả bị bỏ qua, amount `12.34` và note được lưu, truy cập foreign trả 404 và PUT không đổi dòng; truy vấn SQL đối chiếu owner, amount, note và số dòng. Tám shape không hợp lệ trên POST/PUT (fractional cents, vượt trần, zero, âm, amount null, note null, note quá dài, body JSON null) đều trả 400 và không làm đổi rows. Cặp cookie/antiforgery thật của user khác bị từ chối cố định 400 ở register, login, logout, expense POST và PUT; missing PUT/logout cũng bị từ chối, còn logout hợp lệ trả 204.

Bằng chứng safe-500 dùng fault seam chỉ dành cho test: `SaveChangesInterceptor` được inject qua DI và ném lỗi trước khi save. Body 500 có shape cố định `server error` và trace không rỗng; marker/path hư cấu không xuất hiện trong body hoặc log; log dùng mã `auth_server_error` và tương quan với trace; không có expense sau lỗi. Khi tắt seam, amount 10.25 và note được lưu/đọc lại. Kết quả full local integration được lưu là 38/38 GREEN qua 16 database mới có tên strict `RoadmapAuth11_<32 lowercase hex>`; đây không phải bằng chứng cho mọi provider log hoặc lỗi transport SQL thật.

Các bằng chứng trên là kiểm thử nội bộ bằng TestServer và SQL fixture cục bộ. URI HTTPS của TestServer chỉ mô phỏng request handling; chưa có browser/TLS thật, deployment secrets, hoặc production compatibility. Nguồn ngoài về exception handling: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-10.0>.

Các gate vẫn mở: migration/backfill, durable idempotency race, lỗi SQL transport thật và bất định sau commit, log mặc định của EF/provider ngoài handler cấu hình, browser/TLS thật, deployment, teaching/source/native learner gates và toàn bộ course evidence. Chưa được coi khóa học, Stage11 hoặc upload là sẵn sàng.

## Rate limiting fixture đã được kiểm tra

Fixture local có giới hạn mặc định **64 permits trong 60 giây**, `QueueLimit=0`; bộ test thay thế duy nhất record DI này bằng **2 permits** để kiểm tra nhanh. Register/login anonymous phân vùng theo `RemoteIpAddress` trực tiếp của server với bucket `unknown` giới hạn khi thiếu địa chỉ; logout, POST expense và PUT expense phân vùng theo `ClaimTypes.NameIdentifier` không rỗng. Không dùng X-Forwarded-For, body, email, password hoặc owner do client gửi làm khóa.

Từ chối trả **429** với JSON an toàn `{ "error": "rate limit exceeded" }` và `Retry-After: 60`; giá trị này là gợi ý bảo thủ, không phải cam kết thời điểm reset. Queue bằng 0 nên request bị từ chối ngay. Bằng chứng cuối gồm **8/8** test focused và **46/46** test full; 24 tên database mới được ghi nhận từ marker `AUTH_DB_BEFORE_CREATE` của run full cuối, còn 8 tên thuộc run focused riêng. Con số 25 cũ thuộc inventory của một run trước và gồm 24 tên runtime cộng một tham số tĩnh của guard test, không phải 25 fixture của run 46.

Đây là limiter process-local cho fixture học tập. Chưa kết luận về proxy tin cậy, limiter phân tán, tuning production, logging của provider, transport SQL, trạng thái sau commit, migration, idempotency, browser/TLS thật, deployment hoặc toàn bộ course evidence.

Test tích hợp tự sinh database mới theo mẫu nghiêm ngặt. Database và bằng chứng được giữ lại; không dùng lại tên, không DROP/reset. Các file `.before*.cs`, thư mục `.learning-evidence`, `bin` và `obj` là lịch sử hoặc kết quả cục bộ, không đưa vào source package. Chạy app trực tiếp yêu cầu tự đặt một tên database GUID mới; cùng tên sẽ bị từ chối trước tạo schema. Cookie `Secure` chỉ được mô phỏng với TestServer HTTPS trong các test này; app loopback HTTP không đủ để dùng luồng đăng nhập bằng trình duyệt thật.

## Bằng chứng sửa caller C2

Nguồn caller C2 hiện tại giữ nguyên identity và assertion của focused46. Baseline current chưa sửa là 46/46 tại `tasks/course-remediation-2026-09-30/validation/dotnet-stage11-c2-correction1411/baseline/baseline-current.trx`; focused sau sửa là một run riêng 46/46 tại `tasks/course-remediation-2026-09-30/validation/dotnet-stage11-c2-correction1411/focused/focused-final.trx`. Bộ kiểm thử full-current đạt 94/94 pass tại `tasks/course-remediation-2026-09-30/validation/dotnet-stage11-c2-1389/full-current.trx` (và run xác minh lại 94/94 tại `tasks/course-remediation-2026-09-30/validation/dotnet-stage11-c2-correction1411/full-current/full-current.trx`), bao gồm các caller class cùng với các định nghĩa kiểm thử C1b model và migration dưới dạng một đợt xác minh độc lập (không cộng gộp hoặc thay thế 46 ca kiểm thử focused); các số này không cộng thành một TRX.

Ở source hiện tại, startup từ chối database tên mới đã tồn tại trong master rồi chạy migration theo thứ tự bằng `MigrateAsync`; không nhận `EnsureCreated` hoặc database cũ. Fixture auth và rate khôi phục giá trị `ROADMAP_AUTH_DB` trước đó sau khi dispose, kể cả khi setup dở dang. Request fault và recovery không đổi payload dùng cùng một idempotency key; các expense độc lập tiếp tục dùng key mới.
