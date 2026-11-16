# Tuần 13 — REST, middleware, DI và API errors

Chặng 4; 3 bài L13-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W13-01: Thiết kế routes/methods/status/DTO theo contract chi tiêu.

- LO-W13-02: Theo vết middleware và cấu hình DI/CORS đúng boundary.

- LO-W13-03: Trả errors nhất quán, truy trace và không lộ secrets.

Tiên quyết: Đạt gate tuần 12; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-13-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w13-01"></a>

## Bài 1: HTTP và REST contract

LO-W13-01 · W13-C01

HTTP request có method/URL/headers/body, response có status/headers/body. GET đọc và safe, POST tạo/operation, PUT thay trạng thái writable theo contract, DELETE xóa. POST create201+Location; GET missing404; DELETE204 không body; validation400, token không hợp lệ401, thiếu quyền403, conflict409. Idempotent là effect lặp không tăng thêm, không yêu cầu cùng status. DTO chỉ nhận fields cho phép; không nhận UserId để quyết scope hoặc bind entity có roles. GET /api/expenses trả {items,totalCount,page,pageSize}; ngày from inclusive/to exclusive. Viết OpenAPI/HTTP examples gồm failure cases, chưa cần versioning library khi một contract v1; thay contract public phải có migration.

<a id="lo-w13-02"></a>

## Bài 2: Pipeline và DI

LO-W13-02 · W13-C02

Middleware làm before/after next hoặc short-circuit. Exception handler sớm bao downstream, routing chọn endpoint, CORS allowlist origin, authentication trước authorization, rồi endpoint. AddControllers trước Build và MapControllers sau. Scoped DbContext/request không thread-safe; controller inject service, không tự new IO dependency. Origin khác theo scheme/host/port: localhost5500 khác5173. CORS là browser response policy, không thay auth; curl không bị CORS như browser. OPTIONS preflight cần policy đúng. Không AllowAnyOrigin cùng credentials. OpenAPI document trong template .NET10 không bảo đảm có Swagger UI; dùng HTTP file/curl để học request trước.

<a id="lo-w13-03"></a>

## Bài 3: ProblemDetails và logging

LO-W13-03 · W13-C03

ProblemDetails có type/title/status/detail/instance và extension traceId; validation có errors theo field. [ApiController] tự400 cho invalid ModelState; domain result cần map sang validation/conflict đúng. AddProblemDetails và UseExceptionHandler cho unexpected exception; production500 generic không stack. ILogger dùng structured template với operation/ExpenseId/traceId; không ghép text dài hay log Authorization/connection string/full note. Phân severity: routine info, unexpected failure error+exception. Client có traceId để liên kết server logs, không cần stack từ API. Catch không được trả200 error vì phá contract client; log không thay response status.

## Ví dụ giải từng bước

1. Bài toán: pipeline GETping và lỗi500 không cần DB.
2. Exception handler bọc middleware.
3. Program.cs API lab:
```csharp
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
var app=builder.Build();
app.UseExceptionHandler();
app.Use(async (ctx,next)=>{
 app.Logger.LogInformation("Before {Path}",ctx.Request.Path);
 await next(ctx);
 app.Logger.LogInformation("After {Status}",ctx.Response.StatusCode);
});
app.MapGet("/api/ping",()=>Results.Ok(new{message="pong"}));
app.MapGet("/api/fault", (Func<IResult>)(() =>
 throw new InvalidOperationException("Lab fault")));
app.Run();
```
4. Ping200/pong; fault production500generic. Fault route chỉ lab, không đưa production. Delegate Func<IResult> xác định kiểu trả về dù nhánh lab luôn throw.

## Đào sâu và lỗi thường gặp

Không catch từng controller rồi200{error}. After next có thể không chạy khi exception escape; nếu cần latency mọi request dùng finally/instrumentation. ModelState validation khác business ownership; CORS không chặn caller ngoài browser.

## Bài tập thực hành

### Bài tập 1

Lập status matrix create invalid/missingtoken/otherowner/stale/delete.

### Bài tập 2

Browser5173 lỗi CORS, curl được. Chẩn đoán và sửa.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

400/401/404 scoped/409/204. 403 cho principal valid thiếu policy. 204 không parse JSON ở client.

### Hướng giải 2

Xem Origin/preflight OPTIONS; allow đúng http://localhost:5173 Development và middleware order. Không tắt browser protection/no-cors; auth vẫn bắt buộc.

## Project task

Tạo API controller/service boundary, ProblemDetails và HTTP contract; JS UI gọi read endpoint fake persistence trước nối SQL.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w13.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Ping và fault thể hiện status/pipeline đúng, production không trả stack.

- [ ] DTO/status đúng contract, không lấy UserId từ body.

- [ ] Browser đúng origin đọc response; CORS và authorization kiểm độc lập.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'How does a request travel through this API?' Nói middleware/status/DTO/trace.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Web API responses](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0) — W13-C01; primary. HTTP: 200; section xem ledger.

- [Middleware order](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-10.0) — W13-C02; primary. HTTP: 200; section xem ledger.

- [API ProblemDetails](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0) — W13-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-13-quiz.md) · [Đáp án](../../assessments/answer-keys/week-13-answers.md) · [Flashcards](../../flashcards/week-13.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](../module-3/week-12.md)

[Tuần tiếp](week-14.md)
