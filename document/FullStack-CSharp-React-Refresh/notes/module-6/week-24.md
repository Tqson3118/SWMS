# Tuần 24 — Cloud deployment, monitoring và recovery

Chặng 6; 3 bài L24-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W24-01: Triển khai container/SPA với cấu hình production.

- LO-W24-02: Đo latency/error và truy trace có bảo vệ dữ liệu.

- LO-W24-03: Diễn tập restore và rollback với schema compatibility.

Tiên quyết: Đạt gate tuần 23; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-24-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w24-01"></a>

## Bài 1: Deployment runbook

LO-W24-01 · W24-C01

Chọn cloud có managedcontainer và database, viết ADR chi phí/quota/region và cleanup trước tạo resource. Lab tham khảo Azure Container Apps cho APIimage .NET10, static hosting cho React build và managedSQL tương thích SQLServer; không yêu cầu mua cloud lớn. Dùng image digest từ CI, secretsmanager cho DB/auth, HTTPS và exact origins. SPAredirect/providerconfiguration phải khớp publicURL; không giữ localhost production. VITE_API_BASE_URL là build-time public; rebuild client đúng endpoint. Nếu AzureSQL khác localSQL tính năng cần test migration/provider. Deploy schema job một lần, health/readiness rồi smoke tests, chỉ expose API/UI. Không publicRedis/DBports.

<a id="lo-w24-02"></a>

## Bài 2: Logs, metrics và health

LO-W24-02 · W24-C02

Structured logs có traceId/requestID/operation/status/elapsed, không note/token/connectionstring. Metrics requestcount/errorrate/p95latency và dependencyfailures; define cửa sổ đo cùng traffic, không đo một request rồi gọip95. Liveness không DB-heavy; readiness kiểm essential DB, cacheRedis optional có degradedmetric. Monitoring alert có action/runbook, ví dụ5xx>2% trong5phút với mintraffic, đây là project mục tiêu không industryguarantee. Correlate frontendtraceId từ ProblemDetails với serverlog. Tracing useful cho API→SQL/Redis, sampling và redaction giảm overhead. Healthendpoint public trả trạng thái tối giản, không connectiondetails.

<a id="lo-w24-03"></a>

## Bài 3: Backup, rollback và operations

LO-W24-03 · W24-C03

Backup cần retention và restoretest trên DB riêng; RPO là data losswindow mục tiêu, RTO là thời gian khôi phục mục tiêu. Course đặt labRPO24h/RTO60phút để luyện, không bảo đảmcloud tự đạt. Release dùng additive schema trước để old/new app cùng chạy; deletecolumn cần contract/expand-contractphase sau. App rollback vềimage cũ không tự undo destructive migration. Giữ release manifestcommit/image/schema version và runbook thực thi. Smoke loginAlice/Bob,ownership,total,import,restart và tokenexpiry sau rollout. Demo public dùng dữ liệu giả, không finance thật; rotate credentials nếu logs lộ. Cleanup tài nguyên theo runbook khi không dùng.

## Ví dụ giải từng bước

1. Chọn provider và ghi inventory resource/APIURL/UIURL/image digest/schema; lấy hướng dẫn chính thức theo nguồn tuần.
2. Rollout staging với production auth; migrate reviewed SQL một lần, deploy API, kiểm health rồi build/deploy client trỏ URL thật.
3. Smoke Alice report100000, Bob0, BobGETAlice404; importkey lặp không duplicate;401expired,409stale.
4. Backup DB test và restore vào DBrecovery mới; counts/owner/constraints khớp. Deploy image previous lên staging với schema hiện tại để chứng minh rollback, ghi phút thực tế.

## Đào sâu và lỗi thường gặp

Cloud 'running' không chứng minh route/auth/DB. Đừng dùng Developerexceptionpage để debugpublic. Hosting có thể đổi support/price: đọc provider docs khi làm lab và ghi ngày/version; course không khóa giá. Không thực hiện cloud deploy thay user trong quá trình tạo course.

## Bài tập thực hành

### Bài tập 1

Sau deploy refresh /reports404 nhưng root200. Sửa gì?

### Bài tập 2

Oldimage rollback fail vì cột đãdrop. Viết prevention.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Static host cần SPAfallback UI routes; backend /api không fallback. Kiểm builtAPIURL và CORS, không sửa controller chỉ vì UIroute.

### Hướng giải 2

Expand-contract migrations và compatibility test; additive schema trước, old/new code cùng pass. Restore backup có dataloss và cần runbook, không rollback SQL blind.

## Project task

Triển khai bảndemo giả lập dữ liệu lên cloud học viên chọn; thêm monitoring/health/backup restore/rollback runbook và release evidence. Nếu cloud chưa làm thì không đánh dấugate nàyđạt.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w24.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] PublicHTTPSUI/API hoạt động login/provider và Alice/Bobownership; no devtoken/dev exception.

- [ ] Logs nối traceId, dashboard/erroralert có thử failure và không secrets.

- [ ] Restore vào DBriêng khớp fixture; rollback staging compatibility pass, ghi RPO/RTO thực tế và cleanup.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu demo deployment3 phút; giải thích 'What would you do if this release caused errors?' với rollback/schema/restore.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Container Apps deployment quickstart](https://learn.microsoft.com/en-us/azure/container-apps/get-started?tabs=azure-cli) — W24-C01; primary. HTTP: 200; section xem ledger.

- [Liveness and readiness checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0) — W24-C02; primary. HTTP: 200; section xem ledger.

- [Reviewing and applying production migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying) — W24-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-24-quiz.md) · [Đáp án](../../assessments/answer-keys/week-24-answers.md) · [Flashcards](../../flashcards/week-24.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-23.md)

[Tuần tiếp](../module-7/week-25.md)
