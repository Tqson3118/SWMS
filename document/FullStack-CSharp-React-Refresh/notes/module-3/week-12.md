# Tuần 12 — Transactions, concurrency và data correctness

Chặng 3; 3 bài L12-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W12-01: Ghi expense/import batch atomically và chứng minh rollback.

- LO-W12-02: Tái hiện lost update và dùng version token để phát hiện stale write.

- LO-W12-03: Thiết kế chống duplicate batch cùng owner/key và phân biệt retry/read.

Tiên quyết: Đạt gate tuần 11; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-12-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w12-01"></a>

## Bài 1: ACID và transaction boundary

LO-W12-01 · W12-C01

Atomicity all-or-nothing, consistency giữ constraints/invariants, isolation điều khiển tương tác concurrent, durability commit survive theo DB guarantees. Một statement thường atomic, nhiều statement nghiệp vụ cần transaction chung. Begin -> write -> commit; exception -> rollback; giữ transaction ngắn, không chờ HTTP user input trong lock. SET XACT_ABORT ON giúp transaction bị abort trên nhiều runtime errors; TRY/CATCH xử lý XACT_STATE rồi rollback và THROW. DB transaction không bao web requests/cache/broker bằng phép màu; external side effects cần outbox học cuối. Dùng data lab, test fault injection sau write đầu trước write2.

<a id="lo-w12-02"></a>

## Bài 2: Isolation, race và optimistic concurrency

LO-W12-02 · W12-C02

Read committed ngăn dirty reads nhưng không tự ngăn mọi lost update/read skew. SQL Server rowversion là binary version tăng khi update, không datetime. Update WHERE Id=@id AND UserId=@u AND Version=@old; affected0 là missing/stale cần scoped existence check cho404/409. EF concurrency token làm điều này và DbUpdateConcurrencyException khi conflict. Hai editor cùng version: đầu update thành công, thứ hai409; không last-write silently. Serializability tăng bảo vệ nhưng có locking/blocking và retry deadlock cần bounded; unique constraint vẫn cần chống insert duplicate race.

<a id="lo-w12-03"></a>

## Bài 3: Idempotency và retry an toàn

LO-W12-03 · W12-C03

Idempotency nghĩa lặp cùng operation key và payload không tạo extra effect; nó không yêu cầu HTTP response bytes luôn giống. UNIQUE(UserId,IdempotencyKey) + request hash + result trong transaction giữ retry nhất quán. Same key/same hash -> trả kết quả đã có; same key/different hash ->409. Check exists rồi insert không khóa/unique vẫn race; DB enforce uniqueness và xử conflict. Retry transient có backoff/giới hạn, không retry validation/auth failures; explicit transaction với EF execution strategy cần execute whole transaction unit. Hủy sau commit có thể khiến client chưa thấy success; tra batch status trước gửi lại key mới.

## Ví dụ giải từng bước

1. Bài toán:2editor cập nhật cùng amount.
2. Optimistic concurrency dùng old version trong WHERE.
3. Trên ExpenseLab: đọc Id/UserId/Version của expense Alice; giữ @OldVersion từ SELECT rồi update thử:
```sql
DECLARE @Id uniqueidentifier='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
DECLARE @UserId uniqueidentifier='11111111-1111-1111-1111-111111111111';
DECLARE @OldVersion binary(8);
SELECT @OldVersion=Version FROM dbo.Expenses WHERE Id=@Id AND UserId=@UserId;
UPDATE dbo.Expenses SET AmountVnd=60000
WHERE Id=@Id AND UserId=@UserId AND Version=@OldVersion;
SELECT @@ROWCOUNT FirstWriter;
UPDATE dbo.Expenses SET AmountVnd=70000
WHERE Id=@Id AND UserId=@UserId AND Version=@OldVersion;
SELECT @@ROWCOUNT StaleWriter;
```
4. Seed expense với Id trên trước test; expected1 rồi0, amount60000. Rollback test transaction nếu muốn giữ fixture nguyên; không chạy update trên DB thật.

## Đào sâu và lỗi thường gặp

Transaction không bảo đảm mọi nghiệp vụ đúng nếu code/invariant sai. rowversion SQL Server không tương đương PostgreSQL xmin một cách tùy tiện; PostgreSQL optional phải dùng token/version strategy riêng. Cache invalidation không atomic với DB; TTL giảm stale nhưng không strong consistency.

## Bài tập thực hành

### Bài tập 1

Fault sau insert expense trước insert batch status; DB cuối thế nào?

### Bài tập 2

Hai request check missing key rồi insert; chống double import bằng gì?

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Chung transaction, throw/fault rollback toàn unit ->0 newexpense/0batch. Nếu outside transaction thấy partial state, đó là bug. Test cùng key retry một lần ->1batch và sốrow đúng.

### Hướng giải 2

Unique composite key DB và transaction. Một insert thắng; loser đọc result hợp lệ sau xử uniqueness/retry theo boundary, không cả hai append expenses. Different hash same key ->409.

## Project task

Scripts transaction rollback và optimistic update; thiết kế ImportBatch unique/hash/status. Tag phase3-db và ghi evidence2sessions/fault injection.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w12.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Fault giữa multiwrites giữ0 partial records; success lưu toàn batch.

- [ ] Same old version2writes:1 success/1stale; không âm thầm overwrite.

- [ ] Same user/key/payload retry không nhân rows; different payload cùng key conflict.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'What does a transaction protect, and what does it not protect?' Giải thích lost update/idempotency/rollback.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Transactions and autocommit](https://learn.microsoft.com/en-us/sql/t-sql/language-elements/transactions-transact-sql) — W12-C01; primary. HTTP: 200; section xem ledger.

- [Optimistic concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency) — W12-C02; primary. HTTP: 200; section xem ledger.

- [Transaction control and execution strategy](https://learn.microsoft.com/en-us/ef/core/saving/transactions) — W12-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-12-quiz.md) · [Đáp án](../../assessments/answer-keys/week-12-answers.md) · [Flashcards](../../flashcards/week-12.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-11.md)

[Tuần tiếp](../module-4/week-13.md)
