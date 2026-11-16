# Tuần 16 — API tests và atomic/idempotent import

Chặng 4; 3 bài L16-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W16-01: Test HTTP/EF/SQL riêng với WebApplicationFactory và DBisolated.

- LO-W16-02: Implement batch validation/key/hash/transaction và cancellation đúng.

- LO-W16-03: Tái hiện failure qua traceID và kiểm behavior quan trọng.

Tiên quyết: Đạt gate tuần 15; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-16-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w16-01"></a>

## Bài 1: Unit và integration

LO-W16-01 · W16-C01

UnitpureDomain nhanh, fakeIO; integration chạy middleware/controller/EF/provider. WebApplicationFactory<Program>testhost; Program publicpartial cho top-levelentrypoint. Mvc.Testing10 compatibleAPI. DBExpenseTestisolated, fixtureIDs cố định, không connectionshared/prod. SQLServerlab/container testrowversion/FK/transaction, EFInMemory không proof. Fakeauthenticationhandler chỉTest để testownership nhanh, JWTsignature suite riêng với realbearerlocaltestkey. Assert status/body/DBeffect, không mockeverycall để mirrorimplementation. Resetdata chỉ ownedtestfixture, không blanketDROPDB.

<a id="lo-w16-02"></a>

## Bài 2: Atomic import và retry

LO-W16-02 · W16-C02

ClientparseCSV quote-aware thành JSONrows, servervalidateagain max1000/allmoney/date/categoryownership; collect row errors trước write. Hash normalized payload canonical order không raw JSON property order. UNIQUE(UserId,key) và requesthash/result trong transaction cùng expenses+completedbatch. Samekey/hash return previous result;different hash → 409. Faultsauwrite1 rollbackwholeunit. Cancel before commit discard staging/rollback;sau commit querybatchstatus. Execution strategy retry wrap toàn transaction nếu enabled, không blind repeat writes. Contextfault/requestscope kết thúc đúng; không reuse invalid context vôhạn.

<a id="lo-w16-03"></a>

## Bài 3: Regression và trace diagnosis

LO-W16-03 · W16-C03

Ma trận happy/empty/invalid/wrongowner/stale/idempotent/fault/cancel; privacy checkAlice/Bob/report. Logoperation/batchID/trace, không token/fullnote. Faultinject qua testDI chỉTest, không debugendpointproduction. Clientstatus ->trace ->serverlog ->SQL/querycount để phân failureauth/domain/constraint/network. Fixeddata và isolatedDB tránh flaky; không randomsleep làm sync. Coverage cao không thay security/atomicity assertions. Pass chứng minh cases đã chạy, không khẳng định hếtbug. Report evidence ghi commit/environment/testcase/result.

## Ví dụ giải từng bước

1. Bài toán: invalid amount không ghi DB.
2. Unitguard và integration effect kiểm hai layer.
3. Xunitunit sau domainWeek5:
```csharp
[Fact]
public void ZeroAmountIsRejected(){
 Assert.Throws<ArgumentOutOfRangeException>(()=>new Expense(0));
}
```
Integrationpattern: factoryhost+Aliceprincipal+ownedcategory ->POSTamount0 ->assert400/errorfield và countbefore==after. Import2valid+1invalid ->400row3,0newrows. Fault giữa writes ->500generic,0partial. Retry3validsamekey ->1batch/3rows.
4. Tests dùng DBtestisolated và clearfixture mình sở hữu; không API thật/sharedDB.

## Đào sâu và lỗi thường gặp

ẢnhUI không chứng minh chữ kýJWT; unitmock không chứng minhFK. Securitybranches/faulttransactioncritical dù linecoveragecao. Đừng skiptestfail hoặc hạ assertion để gọi phase complete.

## Bài tập thực hành

### Bài tập 1

MatrixGETempty/invalidPOST/wrong-owner DELETE/stalePUT?

### Bài tập 2

2concurrentimportsamekey3rows expect?

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

200items[],400noeffect,404Alicedataunchanged,409firstwriterretained. Assert status+shape+DB.

### Hướng giải 2

Unique arbitration,onecompletedbatch/3rows. Documentin-progressreply/retrypath;different hash → 409,boundedretry. Không 6 rows.

## Project task

Hoàn thiệnintegration/atomicimport và HTTPexamples,querycounts/faultreport. Tagphase4-api sauchecks.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w16.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Regression dùng relationalisolatedDB cho constraints/rowversion/rollback.

- [ ] Retry3validsamekey3rows/1batch;invalid/faultbeforecommit0partial.

- [ ] Trace nối request/log,không secrets; mọi featuretrước vẫn pass.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'What can this integration test prove that a unit test cannot?'

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Integration test setup](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0) — W16-C01; primary. HTTP: 200; section xem ledger.

- [Atomic transactions, savepoints and rollback](https://learn.microsoft.com/en-us/ef/core/saving/transactions) — W16-C02; primary. HTTP: 200; section xem ledger.

- [Execution strategies and transactions](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency) — W16-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-16-quiz.md) · [Đáp án](../../assessments/answer-keys/week-16-answers.md) · [Flashcards](../../flashcards/week-16.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-15.md)

[Tuần tiếp](../module-5/week-17.md)
