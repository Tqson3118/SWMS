# Tuần 21 — Git, Linux và environment configuration

Chặng 6; 3 bài L21-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W21-01: Tạo history có thể review và tái hiện.

- LO-W21-02: Chẩn đoán đường dẫn, quyền và network service.

- LO-W21-03: Phân biệt public frontend config với backend secret.

Tiên quyết: Đạt gate tuần 20; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-21-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w21-01"></a>

## Bài 1: Git và review workflow

LO-W21-01 · W21-C01

Git snapshot tracked files, index là staging chọn nội dung commit; git diff trước stage và git diff --cached trước commit. Branch cho feature, commit nhỏ có lý do và tests. git fetch tải refs; pull tích hợp nên hiểu merge/rebase trước dùng. Rebase đổi commit IDs, chỉ rewrite branch riêng chưa chia sẻ; không force-push main. Merge conflict đọc cả intent, sửa file rồi chạy kiểm hành vi; không chọn toàn ours/theirs mù. .gitignore loại bin/obj/node_modules/.env/secret/log chứa personal data. Commit lockfile và migrations, không commit credentials. Pull request mô tả behavior/why/evidence, không chỉ liệt kê tên file.

<a id="lo-w21-02"></a>

## Bài 2: Linux và process basics

LO-W21-02 · W21-C02

Linux paths case-sensitive; src/Api khác src/api, lỗi Windows có thể chỉ lộ trong CI. pwd/ls/cat/head/tail đọc context, rg tìm text, mkdir tạo thư mục, chmod thay quyền có chủ đích. Không dùng chmod777 để chữa mọi lỗi; service cần quyền đọc code và ghi đúng data/log path. ps xem process, ss -ltn xem listener, curl kiểm HTTP, exit code0 thường success. SIGTERM yêu cầu graceful shutdown; SIGKILL không cho cleanup. Port binding0.0.0.0 trong container khác localhost chỉ trong container. stdout/stderr phục vụ container logs; shell quoting cần tránh secrets lọt command history.

<a id="lo-w21-03"></a>

## Bài 3: Configuration và secrets

LO-W21-03 · W21-C03

ASP.NET Core đọc appsettings, environment-specific và env với key nested dùng __; ConnectionStrings__Finance overrides JSON. Development user-secrets là convenience ngoài repo, không là vault bảo mật production. Environment variables là cách truyền cấu hình, vẫn có thể lộ ở process/log nên production dùng secret manager/access control. Frontend VITE_API_BASE_URL đóng vào bundle lúc build, mọi VITE_ value public; đổi env container sau build không tự sửa bundle. Tách Development/Testing/Production, validate required configuration khi startup. Không bật detailed errors hoặc dev user-jwts trong production. .env.example có tên biến và giá trị lab công khai không có secrets thật.

## Ví dụ giải từng bước

1. Tạo branch feat/config-runbook; git status --short, git diff rồi stage đúng files.
2. Linux/WSL hoặc container shell: pwd; rg 'localhost' web/src; curl -i http://localhost:8080/api/ping. Các lệnh chạy riêng, không in env toàn bộ.
3. Trong API lab đọc builder.Configuration.GetConnectionString("Finance"); đặt ConnectionStrings__Finance ở process env, không ghi repo. Frontend đọc import.meta.env.VITE_API_BASE_URL.
4. Build client với API URL A, sau đó đổi env thànhB nhưng không rebuild: bundle vẫnA. Ghi kết quả và chọn rebuild hoặc runtime public config file có validation.

## Đào sâu và lỗi thường gặp

Env không tự là secret và minify không che giá trị. Secret đã commit: revoke/rotate trước, rồi xử lý history theo team workflow; chỉ xóa dòng không xóa lịch sử. Lab tránh cố tình commit secret thật.

## Bài tập thực hành

### Bài tập 1

Sửa conflict cùng category validator ở hai branch.

### Bài tập 2

CI Linux báo module không tìm được dù Windows chạy.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Đọc cả diff, thống nhất trim/range policy; sửa conflict markers, test empty/60/61 chars rồi commit resolution. Ghi lý do cho reviewer.

### Hướng giải 2

Kiểm casing filename/import, lockfile và cwd; sửa tên/import qua git mv nếu filesystem case-insensitive. Không thêm dependency ngẫu nhiên.

## Project task

Đưa finance repo lên GitHub riêng của học viên; tạo PR feature nhỏ, README runbook và configuration inventory không secrets. Git/English vốn chạy từtuần1, tuần này chuẩn hóa để người khác chạy.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w21.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Fresh clone chạy theo README với lockfile/migrations và không file ngầm cá nhân.

- [ ] PR có behavior/test evidence; resolve conflict không mất validator policy.

- [ ] Scan repo hiện tại không secret; client bundle chỉ chứa publicconfig, Linux import casing pass.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Viết PR description English120–180words: concrete change, validation, limitation; đọc thành tiếng.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Staging and reviewing commits](https://git-scm.com/book/en/v2/Git-Basics-Recording-Changes-to-the-Repository) — W21-C01; primary. HTTP: 200; section xem ledger.

- [Symbolic modes and permission bits](https://manpages.ubuntu.com/manpages/noble/en/man1/chmod.1.html) — W21-C02; primary. HTTP: 200; section xem ledger.

- [Configuration providers and environment overrides](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0) — W21-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-21-quiz.md) · [Đáp án](../../assessments/answer-keys/week-21-answers.md) · [Flashcards](../../flashcards/week-21.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](../module-5/week-20.md)

[Tuần tiếp](week-22.md)
