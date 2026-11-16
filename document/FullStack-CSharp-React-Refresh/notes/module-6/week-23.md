# Tuần 23 — GitHub Actions và integration testing

Chặng 6; 3 bài L23-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W23-01: Chạy restore/build/test trên fresh runner.

- LO-W23-02: Kiểm behavior trên SQL Server thật cùng provider.

- LO-W23-03: Phân biệt test pass, build artifact và released version.

Tiên quyết: Đạt gate tuần 22; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-23-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w23-01"></a>

## Bài 1: CI reproducible pipeline

LO-W23-01 · W23-C01

CI cần tái tạo từ repository: chọn .NET10 SDK/setup-dotnet và Node active LTS đã ghi inventory; npm ci đọc lockfile, không npm install để âm thầm đổi graph. Build Release rồi test; frontend tsc/build và behavior tests non-watch. Workflow pull_request và pushmain có permissions contents:read theo least privilege. Không chạy deploy secrets trên untrusted fork PR. Cache dependency theo lockfile/csproj để nhanh nhưng cold-cache vẫn pass. Pin actions commit SHA cho release supply chain sau khi kiểm revision; tagsmajor trong lab giúp đọc nhưng cần inventory thực tế. Exit code thất bại phải fail job, không ||true.

<a id="lo-w23-02"></a>

## Bài 2: Relational integration tests

LO-W23-02 · W23-C02

EF InMemory không enforce FK/transactions/SQL semantics giống production; SQLite khác SQLServer rowversion/collation/querytranslation. Unit test pure money/validation nhanh; relational integration dùng SQLServer container/testDB riêng. Mỗi test suite quản seed/reset riêng không đụng development/prod. WebApplicationFactory host test và mock principal deterministic chỉ Testing; test JWT invalid issuer/signature qua authintegration riêng chứ không mock luôn. Assert compound ownershipFK, duplicate importkey, rollbackinvalidrow, optimistic409, querycountN+1. Dùng unique DB hoặc serial fixture có reset rõ, tránh tests phụ thuộc order. Testcontainers là tùy chọn đáng dùng khi Docker runner có sẵn.

<a id="lo-w23-03"></a>

## Bài 3: Evidence và deployment boundary

LO-W23-03 · W23-C03

Publish TRX/frontend results logs không chứa secrets; artifact image tagged gitSHA nối commit/test/deployment. Integration tests gate trước push deploy; successful compile không chứng minh ownership/rollback. CI job ở PR chỉ kiểm; deploy jobmain/tag dùng protected environment/OIDC federation khi cloud hỗ trợ, tránh static long-lived credentials. Failure phải cho reproduction steps và expected/actual. Flaky test cần sửa isolation/timeouts, không retry mãi che bug. Code coverage là thông tin, không thay tests risk-based: ownership, concurrency, idempotency, failures quan trọng nhất. Security dependency check có thể thêm nhưng không substitute code review.

## Ví dụ giải từng bước

1. Repo cần tests Expense.Tests và client script test='vitest run'. Workflow build.yml:
```yaml
name: verify
on: [push, pull_request]
permissions:
  contents: read
jobs:
  verify:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v6
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore
      - run: dotnet build -c Release --no-restore
      - run: dotnet test -c Release --no-build --logger trx
      - uses: actions/setup-node@v4
        with:
          node-version: '24'
          cache: npm
          cache-dependency-path: web/package-lock.json
      - run: npm ci
        working-directory: web
      - run: npm run build
        working-directory: web
      - run: npm run test -- --run
        working-directory: web
```
2. Đây là baseline compile/unit/UI; chưa provision SQL container. Thêm separate integration job dùng testcontainer fixture để dotnet test chạy SQL behaviors trước markgate.
3. Cố ý thay ownership query test fail và PRred; sửa code/test rồi green.
4. Ghi action versions thực tế và SHA cho release; mẫu lab dùng major tags, không coi chúng immutable.

## Đào sâu và lỗi thường gặp

Do not share DB từ course folder hay importer. Test-only fake auth không được enabled Development/Production. YAML booleans parser có thể coi 'on' thành true với YAML1.1; GitHubworkflow dùng schema riêng, không validate bằng parsergeneric rồi kết luận lỗi.

## Bài tập thực hành

### Bài tập 1

Unit tests green nhưng duplicateimport thêm rows. Thêm test gì?

### Bài tập 2

Cold runner frontend thiếu dependency vì local node_modules có. Sửa thế nào?

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

SQLServerintegration gửi samekey/hash hai lần/concurrent, assert rowcount không tăng; different hash → 409. Use unique constraint thật, không dictionaryfake.

### Hướng giải 2

Chạy npm ci từ fresh checkout, commit manifest/lockfile đúng; build/test fail là evidence missing dependency. Không upload node_modules để che.

## Project task

Thêm CI gates .NET/frontend/SQL integration, publish test artifacts, Dockerbuild và imageSHA inventory; deployment job chỉ sau gates.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w23.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Fresh Linux runner cold-cache build/unit/UIpass với lockfile.

- [ ] SQLServer thật chứng minh ownershipFK,rollback,409,idempotency,N+1 guard; tests không chạm DB cá nhân.

- [ ] Một regression cố ý làmPRred; fixgreen và evidence lưu CI link/commit/image SHA.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Trình bày một CI failure bằng English: trigger, failing assertion, root cause, fix, validation.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Restore, build and test .NET workflow](https://docs.github.com/en/actions/tutorials/build-and-test-code/net) — W23-C01; primary. HTTP: 200; section xem ledger.

- [Real database versus test doubles](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy) — W23-C02; primary. HTTP: 200; section xem ledger.

- [Permissions, untrusted inputs and action pinning](https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions) — W23-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-23-quiz.md) · [Đáp án](../../assessments/answer-keys/week-23-answers.md) · [Flashcards](../../flashcards/week-23.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-22.md)

[Tuần tiếp](week-24.md)
