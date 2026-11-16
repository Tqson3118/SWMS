# Tuần 25 — HTTP, rate limiting và practical design

Chặng 7; 3 bài L25-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W25-01: Theo vết request và phân biệt retry-safe operations.

- LO-W25-02: Giới hạn theo validated identity và kiểm429.

- LO-W25-03: Định nghĩa nhu cầu trước vẽ kiến trúc.

Tiên quyết: Đạt gate tuần 24; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-25-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w25-01"></a>

## Bài 1: HTTP và latency budget

LO-W25-01 · W25-C01

Request browser quaDNS/TLS/proxy đếnAPI, rồi SQL/cache; latency tổng gồm network,queue,compute vàIO. HTTP keep-alive/reuse giảm kết nối, nhưng không chữa query chậm. GETsafe/idempotent, PUT/DELETE idempotent theo effect; POSTimport chỉ retry-safe theo idempotency contract. Status429/503 khác400/401; retry bounded với exponentialbackoff+jitter cho transient failure, không retryinvalidinput. Timeoutdeadline từ client đếnserver phải hợp lý; CancellationToken chuyển xuốngIO, không bảo đảm rollbackcommit. Authfinancialresponse private/no-store theo endpoint policy; publiccache/CDN không được dùng chung ownerdata. Đọc trace trước tối ưu, không đoán cache là câu trả lời mọi lỗi.

<a id="lo-w25-02"></a>

## Bài 2: Rate limiter

LO-W25-02 · W25-C02

Fixedwindow reset theo window, tokenbucket cho burst, concurrencylimiter giới hạn số in-flight chứ không requests/time. Chọn policy theo endpoint cost và loadtest. Partition theo internaluser được validated auth, anonymous theo remoteIP đángtin/controlledfallback; không lấy UserIdquerystring. UseRouting trước endpointlimiter và authentication trước identitypartition. QueueLimit0 để reject ngay tronglab; RejectionStatusCode429 mặc định framework có thể khác nên cấu hình rõ. Retry-After khi lease có metadata; client delay bounded. Limiter in-process không global acrossreplicas; distributedquota/gateway cần thiết nếu yêu cầu global. Không coi limiterapp là đủ DDoSprotection.

<a id="lo-w25-03"></a>

## Bài 3: Capacity và design communication

LO-W25-03 · W25-C03

Design bắt đầu functionalrequirements (expenses/budgets/import), nonfunctional(ownerprivacy,correctmoney,availability,latency) và assumptions. Ví dụ lab1000users,10%active,2req/minactive≈3.33RPSaverage; burst10x≈33.3RPS, đây là giả định để test, không data thực. Data estimate rows/user/day×users×days×avgbytes và indexes/backupsoverhead. Vẽ modularmonolithAPI+SQL+Redisoptional, không microserviceschỉ vì phỏng vấn. Chọn trade-off giữa stale cache và freshness, syncimport<=1000 và asyncjobs. Ghi decision,alternative,evidence,triggerđổi; explain bottleneckSQLCPU/IO/network dựa metrics.

## Ví dụ giải từng bước

1. Lab fixedwindow global chỉ minh họa, chưa identitypartition production:
```csharp
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
var b=WebApplication.CreateBuilder(args);
b.Services.AddRateLimiter(o=>{
 o.RejectionStatusCode=StatusCodes.Status429TooManyRequests;
 o.AddFixedWindowLimiter("lab",x=>{
  x.PermitLimit=3;x.Window=TimeSpan.FromMinutes(1);x.QueueLimit=0;
 });
});
var app=b.Build();app.UseRouting();app.UseRateLimiter();
app.MapGet("/api/ping",()=>Results.Ok()).RequireRateLimiting("lab");
app.Run();
```
2. Gọi4 lần nhanh trong cùng window:200/200/200/429; windowboundary có thể reset nên logtimestamp và repeatwithinwindow.
3. Project chuyển partition theo validatedinternaluser và assert Alicequota không tiêu Bobquota.
4. Chạy1replica rồi2replica: per-processpolicy không bảo đảm global3requests. Ghi trade-off thay vì claimdistributedlimit.

## Đào sâu và lỗi thường gặp

Không loadtest publicservice của người khác; lab chỉlocalhost/staging học viên. Header X-Forwarded-For chỉ tin knownproxy; blindly trust spoofedheader phá IPlimit. Numericquota3/min là lab, không productionrecommendation.

## Bài tập thực hành

### Bài tập 1

Server trả 400 invalid amount, client nên retry exponential không?

### Bài tập 2

Tính RPS cho1000users,100active,2requests/min và burst10x.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Không, input phải sửa. Retry transient429/503 theo policy và Retry-After; POST phải idempotent mới retry khi chưa rõ commit.

### Hướng giải 2

100×2/60=3.33average; burst≈33.3. Nêu giả định phân bố và đo p95 dưới load thay vì hứa capacity.

## Project task

Thêm rate limiting imports/report theo owner, consistent429 và ADR latency/capacity; UI message429/retry bounded.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w25.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Samewindow quota vượt trả429; Alice/Bobpartitions độc lập và không do clientUserId.

- [ ] Client không retry400/401 vô hạn; transientretry bounded có jitter/idempotency.

- [ ] ADR có assumptions,RPSestimate,trace/load evidence và ghi in-process limit scope.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Designinterview10 phút: yêu cầu, giả định, sơ đồ, bottleneck, failure, trade-off; thu lại 5 phút chính.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Safe and idempotent method semantics](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Methods) — W25-C01; primary. HTTP: 200; section xem ledger.

- [Algorithms, policies and middleware order](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0) — W25-C02; primary. HTTP: 200; section xem ledger.

- [Capacity planning](https://learn.microsoft.com/en-us/azure/architecture/framework/performance-efficiency/capacity-planning) — W25-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-25-quiz.md) · [Đáp án](../../assessments/answer-keys/week-25-answers.md) · [Flashcards](../../flashcards/week-25.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](../module-6/week-24.md)

[Tuần tiếp](week-26.md)
