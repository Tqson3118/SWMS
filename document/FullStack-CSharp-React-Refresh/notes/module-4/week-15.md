# Tuần 15 — JWT, authorization và ownership

Chặng 4; 3 bài L15-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W15-01: Validate token và tách devtoken khỏi productionOIDC.

- LO-W15-02: Bảo vệ endpoint/policy và mọi query theo owner.

- LO-W15-03: Thiết kế expiry/logout và bảo vệ token/config.

Tiên quyết: Đạt gate tuần 14; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-15-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w15-01"></a>

## Bài 1: JWT trust và môi trường

LO-W15-01 · W15-C01

JWT header.payload.signature, payload thường base64url không encryption; không đưa secrets vào claims. Validate trusted signature/key,issuer,audience,lifetime,algorithm policy bằng bearer library; decode không trust. Development dùng dotnet user-jwts/user-secrets cho localtoken. Production dùng standardsOIDC/OAuth, publicSPA authorizationcode+PKCE; không tự passwordlogin copy blog để phát token. Config Authority/Audience lấy actualregistration, thiếu phải failstartup. MapInboundClaims=false cho sub/iss rõ; production userId internal map Unique(Issuer,Subject), không email/displayname mutable. Dev app_user_id là claimlab có validGuid, chỉ đọc Development.

<a id="lo-w15-02"></a>

## Bài 2: Auth và resource quyền

LO-W15-02 · W15-C02

Authentication xác định ai; authorization xác định được làm gì. Authorize/RequireAuthorization đủ authenticated nhưng không ownership. WHERE Id+UserId cho GET/PUT/DELETE và owner filters ở list/report/import/category/budget. Policy scopes/roles là layer khác resource. Provider scp thường space-separated nên parse convention, RequireClaim exact không luôn phù hợp multi-scopes. 401tokeninvalid/missing;403 authenticated thiếu policy;404scopedabsent theo contract tránh lộ resource khácuser. Không tin role từ clientstate/unvalidatedtoken. Admin functionality optional phải có policy/audit, core chỉ personalownership.

<a id="lo-w15-03"></a>

## Bài 3: Session lifecycle và secrets

LO-W15-03 · W15-C03

LogoutUI xóa session không tự revoke mọi issuedJWT; bearer còn usable tới expiry hoặc policyrevocation server. Access shortlived và refresh/acquire do providerSDK/standards, không tự lưu refreshlongterm localStorage. CoreSPA provider memorycache, reload có thể reacquire qua providersession. Authority/clientId trong publicfrontend không secret; signingkey/clientsecret không VITE_. HTTPSproduction và HTTPS metadata, không chấp nhận cert/devissuer tùy tiện. Testexpired/wrongaudience/badsignature, không chỉmissing. Fakeprincipal integration onlyTestenvironment, tuyệt đối không headerbypass production.

## Ví dụ giải từng bước

1. Bài toán: Alice/BoblocalJWT và prodbranch standards.
2. Developer tool quản key qua user-secrets, không ghi token vào evidence.
3. Lab:
```powershell
dotnet add src/Expense.Api package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.*
dotnet user-jwts create --project src/Expense.Api --name Alice --claim app_user_id=11111111-1111-1111-1111-111111111111 --valid-for 15m
dotnet user-jwts create --project src/Expense.Api --name Bob --claim app_user_id=22222222-2222-2222-2222-222222222222 --valid-for 15m
```
```csharp
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer(o=>{
 o.MapInboundClaims=false;
 if(!builder.Environment.IsDevelopment()){
  o.Authority=builder.Configuration["Auth:Authority"]??throw new InvalidOperationException("Missing authority");
  o.Audience=builder.Configuration["Auth:Audience"]??throw new InvalidOperationException("Missing audience");
  o.RequireHttpsMetadata=true;
 }
});
```
4. UseAuthentication trước UseAuthorization; endpointRequireAuthorization. Missing401,Aliceown200,BobAlice404. Production ignoreapp_user_id và mapissuer/sub.

## Đào sâu và lỗi thường gặp

JWT không có automaticrefresh/revocation vì AddJwtBearer. Provider setup phải đăng ký SPAredirect/apiresource/scope và pintrustedauthority; scopeclaim convention đọc docs provider. Gateproduction không chấp nhận dev token.

## Bài tập thực hành

### Bài tập 1

Tokenexpired,signatuređổi1ký tự,audiencekhác: expected?

### Bài tập 2

BobDELETEAlice bị xóa dù Authorize; sửa.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

401 và challenge, không principal valid. Validtoken thiếu policy403. Tokenissuer phải trusted, không lấy Authority tùy JWTclient đưa.

### Hướng giải 2

Load Id+currentUserId rồi remove; absent404, DBunchanged. Regression với category/budget/report/import cùng scope, không chỉDELETE.

## Project task

JWTdevtests2users và ownerpolicy. Ghi authADR dev/prod, logoutlimit, secretsboundary. Chuẩn bị providerproduction cho frontendW19.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w15.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] 401missing/expired/wrongissuer/audience/signature,403policy,404crossowner.

- [ ] Bob không đọc/sửa/xóa/import/category/report Alice, DB giữ nguyên.

- [ ] Prodconfig thiếu failstartup,devtoken không valid production,HTTPS metadata.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'Authentication versus authorization: why is Authorize not enough?'

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [JWT validation and standards](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0) — W15-C01; primary. HTTP: 200; section xem ledger.

- [Resource authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-10.0) — W15-C02; primary. HTTP: 200; section xem ledger.

- [dotnet user-jwts](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn?view=aspnetcore-10.0) — W15-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-15-quiz.md) · [Đáp án](../../assessments/answer-keys/week-15-answers.md) · [Flashcards](../../flashcards/week-15.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-14.md)

[Tuần tiếp](week-16.md)
