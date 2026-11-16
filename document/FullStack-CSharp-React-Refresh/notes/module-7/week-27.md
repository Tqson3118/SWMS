# Tuần 27 — Queues, scaling và consistency trade-offs

Chặng 7; 3 bài L27-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W27-01: Thêm exportjob có202 và owner status endpoint.

- LO-W27-02: Tránh dual-write loss bằng durable SQL boundary.

- LO-W27-03: Giữ stateless API và xác định consistency boundary.

Tiên quyết: Đạt gate tuần 26; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-27-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w27-01"></a>

## Bài 1: Background job contract

LO-W27-01 · W27-C01

Giữ import<=1000rows synchronousatomic như contract; bổ sung export monthlyCSV là featurebackground riêng. POST /api/export-jobs nhậnmonth, trả202+Location statusendpoint, GETstatus chỉ owner, download chỉ owner. Job states pending/running/succeeded/failed và expiryretention rõ; downloadfile không publicstaticURL chứa dataowner. Request202 nghĩa accepted chưacompleted; UIpoll bounded/backoff vàcancelUI không tự cancel job server. Queue giúp absorbburst/slowwork, không làm SQLwriteatomic tự động. Requiredbaseline có DBjobs+BackgroundService trong modularmonolith; durablebrokerintegration optional khi đã hiểu redelivery/idempotency.

<a id="lo-w27-02"></a>

## Bài 2: Outbox và at-least-once processing

LO-W27-02 · W27-C02

DBcommit rồi publishmessage có gap: crashsaucommit trước publish làm job mất message; publish trước commit có thể worker đọcjobchưacó. Transactionaloutbox lưuJob+OutboxEvent cùng SQL transaction; dispatcher phát từ outbox, marksent sau publish. Crashsaupublish trước marksent có duplicate, nên consumeridempotent theoJobId với constraint/claimingtransaction. Ack sau effectdurable; retriesbounded+jitter và deadletter/manualreview khi poisonpayload. Không claim exactly-onceend-to-end chỉ vìqueue; duplicate vàout-of-order cần xử lý. SQLimplementation concept khác CosmosDBtransactionalbatch ở nguồn minh họa; project dùngSQLtransactions/uniqueconstraints.

<a id="lo-w27-03"></a>

## Bài 3: Load balancing và horizontal scaling

LO-W27-03 · W27-C03

Loadbalancer phân requests tới replicas; stateless API không lưu session/jobauthoritative trong process. JWTvalidated perrequest, sharedSQL vàRediscache, durablejobcoordination. Cookie/BFF deployment cần sharedDataProtectionkeys theo framework, không áp dụng nếu purebearerSPA. Forwardedheaders chỉknownproxy đểscheme/clientIP đúng; DBconnectionpool tổng replicas × pool size phải nằmDBcapacity. Readreplica/cache có thể stale; financialwrite vàversioncheck điprimarySQL. CAP là trade-off availability/consistency khi networkpartition với định nghĩa hẹp, không 'chọn2' vôđiềukiện cho mọi app. Modularmonolithscale trước service extraction; observebottleneck rồichọn.

## Ví dụ giải từng bước

1. Exportjob SQLrecord cùngOutbox trongtransaction; return202 với /api/export-jobs/{id}.
2. Workerclaims pendingjob bằng updatecondition/lease trongSQL; haiworkers không cùng claim. Generate deterministicCSV theo owner/month, lưu privateartifact.
3. Crashdispatcher sau publish, chạy lại: duplicateJobId không tạo hai logicalexports; storagepath theoJobId và state transition compare/constraint.
4. Run2APIreplicas qua proxy; alternatingrequests vẫn GETownerjob/status đúng, Alice/Bob404crossowner; killworker giữajob rồi leaseexpire/retrybounded.

## Đào sâu và lỗi thường gặp

Queue không thay transactionalimport và không bỏ ownership tạiworker. CSVexport note có thể bắt đầu =,+,-,@; cần spreadsheetformula-injectionpolicy khi targetExcel. QuotedCSV không tự chặn formula; labprefixtext hoặc exportJSON thay thế, ghi transformation cho người dùng.

## Bài tập thực hành

### Bài tập 1

Workerack trướcfiledurable rồicrash; hậuquả và sửa?

### Bài tập 2

2replicas mỗi MaxPoolSize 100 nhưng DB budget 120 connections, rủiro?

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Message đã remove nhưng artifactthiếu. Ackafterdurableeffect; idempotentJobId để retry không duplicate. Testcrashpoint before/afterack.

### Hướng giải 2

Có thể 200 connections potential vượt120; tính cả workers/admin, giảmpool/replicas hoặc tăngDBcapacity sauđo. Không autoscaleAPI vôhạn.

## Project task

Thêm owner-scoped asynchronousCSVexport/status bằng SQLdurablejobs và BackgroundService; ADRbroker/outbox/scaling. Required test crash/retry/two-workerclaim; brokerexternal optional.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w27.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] 202/status/downloadowner đúng; Bob không đọcAlicejob/file; UIpending/failure/success riêng.

- [ ] Crash/retry duplicateJobId không duplicateeffect;2workersclaim không tranh cùngjob.

- [ ] 2replicas statelesssmokepass; DBpoolbudget và stale-readtrade-offs được tính; noexactly-onceclaim.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Mockdesign: 'How would you scale this finance app?' trả lời5 phút với queuefailures, SQLboundary vàtrade-offs.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Queue buffering and worker load](https://learn.microsoft.com/en-us/azure/architecture/patterns/queue-based-load-leveling) — W27-C01; primary. HTTP: 200; section xem ledger.

- [Outbox concept; Cosmos implementation differs from SQL](https://learn.microsoft.com/en-us/azure/architecture/databases/guide/transactional-out-box-cosmos) — W27-C02; primary. HTTP: 200; section xem ledger.

- [Forwarded headers and trusted proxies](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0) — W27-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-27-quiz.md) · [Đáp án](../../assessments/answer-keys/week-27-answers.md) · [Flashcards](../../flashcards/week-27.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-26.md)

[Tuần tiếp](week-28.md)
