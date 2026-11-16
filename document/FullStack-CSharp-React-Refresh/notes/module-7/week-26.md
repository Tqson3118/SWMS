# Tuần 26 — Redis, cache-aside và consistency

Chặng 7; 3 bài L26-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W26-01: Phân biệt cache với source of truth.

- LO-W26-02: Cập nhật SQL trước invalidation và hiểu race.

- LO-W26-03: Đo hit/miss và thiết kế cache outage fallback.

Tiên quyết: Đạt gate tuần 25; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-26-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w26-01"></a>

## Bài 1: Redis fundamentals

LO-W26-01 · W26-C01

Redis lưu key/value in-memory với data structures và TTL; cache có thể evict/expire/outage nên SQL là source of truth cho money. SET key value EX60 đặt TTL; GETmiss cần fetchdatabase; DEL invalidates. Production cần private networking/auth/TLS theo provider, không expose6379. App reuse Redisconnection client theo documentedlifetime, không newconnection mỗi request. IDistributedCache abstraction string/bytes với AddStackExchangeRedisCache có sliding/absoluteexpiration; cachekey chứa schema version/internalUserId/month/filter. Không đặt cachevalue token hoặc fullpersonalnote nếu không cần. Deserialize cachedDTO cũng validate schema; corruption xử lý miss+logmetric.

<a id="lo-w26-02"></a>

## Bài 2: Cache-aside và invalidation

LO-W26-02 · W26-C02

Readmiss→querySQL→cacheTTL→return; hit trả cachedreport. Write commitSQL trước DEL affectedkeys; invalidating trước commit có thể repopulateolddata. Deleteaftercommit cũng vẫn race: reader đã queryold trước write rồi SETold sauDEL. TTL bounds stale chứ không immediateconsistency. Mức cơbản project chấp nhận stale tối đa60seconds cho report, edit/list đọcSQLfresh; UX refetch và Redisdown bypass. Stronger: storedgenerationversion transaction cùng expense write, keybygeneration và readerreadgeneration fresh từ SQL; readerold chỉ populateoldkey. Cần include allaffectedmonths/categories, PUTđổingày phải invalidateold/newmonth. Ghi limitations rõ, không hứa zero stale từ DEL.

<a id="lo-w26-03"></a>

## Bài 3: Failure, stampede và evidence

LO-W26-03 · W26-C03

Manymisses cùngkey gây stampede; processsingle-flight giảm work một replica, distributedlock có lease/tokenrelease semantics và không thayDBinvariant. TTLjitter giảm expiry đồng loạt, shortnegativecache chỉ cho safeowner-specificemptyreport. Cachefailure logmetric và fallbackSQL bounded, không fail moneywrite vì optionalcache. TTLmemorysize/cost đo bằng workload; hitrate cao không có nghĩa reportcorrect. SQL querycount so before/after, p95 và datafreshness cần cùng test environment. RedispersistenceRDB/AOF không biến cachedreport thành authoritativeledger. Requiredlab cache/fallback/TTL; distributedlocks và generationkeyimplementation tùychọn nếu ADR stale60s đủ.

## Ví dụ giải từng bước

1. Trong Redislab shell:
```text
SET report:v1:alice:2026-10 "100000" EX 60
GET report:v1:alice:2026-10
TTL report:v1:alice:2026-10
DEL report:v1:alice:2026-10
```
2. Alice shorthand trong CLI chỉdemo; project key dùng internalGUID, reportJSON có schema version. Bobkeymiss không đọcAlicekey.
3. GETAlice miss→SQLtotal100000→SETTTL60, GETagainhit. CreateFood10000 commitSQL rồi invalidate; report kế tiếp 110000 khi không có race.
4. Diễn race readeroldSET sauDEL; assert possible100000untilTTL, ghi stalecontract. Dừng Redis: reportSQL110000 và write thành công, logcacheerror không secret.

## Đào sâu và lỗi thường gặp

Cache không sửa index/query lỗi; tối ưuSQL trước. Userqueryparam không được quyết cacheowner. Nếu yêu cầu immediate reportfreshness không chấp nhận TTL60s, chọn uncachedread hoặc generation/versionstrategy được test.

## Bài tập thực hành

### Bài tập 1

PUT expense đổiOct sangNov cần invalidate gì?

### Bài tập 2

Vì sao DELsaucommit vẫn có stale race?

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

CảOct vàNov keys của owner cùng budget/reportaffectedcategories. Queryoldvalues trước write, commit rồi invalidate; missedinvalidation có TTL bound nhưng cầntest.

### Hướng giải 2

Reader lấy oldSQL trước commit, writercommit+DEL, readerSETold sauDEL. Timeline này cầnTTL/versionkey; DELkhông atomic với in-flightreader.

## Project task

Thêm Redis monthlyreport cache owner-scoped TTL60s/fallback và cachemetrics. ADR nói rõ allowedstale; list/editmoney luônSQL.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w26.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Alice/Bobcachekeys riêng; hit/miss assertions total đúng, không globalfinancekey.

- [ ] Redisoutage reads/writes vẫn dùngSQL; no token/note trong cachelogs.

- [ ] Testold/newmonthinvalidation và lateSET race; TTL<=60s, limitations được ghi thay vì claimzero stale.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Giải thích 'Cache invalidation is difficult because...' qua timeline reader/writer trong2 phút.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [SET with expiration](https://redis.io/docs/latest/commands/set/) — W26-C01; primary. HTTP: 200; section xem ledger.

- [Cache-aside and consistency limitations](https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside) — W26-C02; primary. HTTP: 200; section xem ledger.

- [Distributed cache abstraction and Redis](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-10.0) — W26-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-26-quiz.md) · [Đáp án](../../assessments/answer-keys/week-26-answers.md) · [Flashcards](../../flashcards/week-26.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-25.md)

[Tuần tiếp](week-27.md)
