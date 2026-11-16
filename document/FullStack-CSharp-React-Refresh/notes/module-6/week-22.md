# Tuần 22 — Docker Compose và persistence

Chặng 6; 3 bài L22-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W22-01: Đóng gói API runtime tách khỏi SDK.

- LO-W22-02: Nối API–SQL–Redis bằng service DNS và giữ dữ liệu.

- LO-W22-03: Phân biệt startup order, retry và persistent state.

Tiên quyết: Đạt gate tuần 21; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-22-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w22-01"></a>

## Bài 1: Images và multi-stage build

LO-W22-01 · W22-C01

Image là template layers; container là process từ image có writable layer riêng. Dockerfile build stage dùng SDK để restore/publish, runtime stage chỉ aspnet runtime và published output. COPY csproj rồi restore trước code tận dụng cache; .dockerignore loại secrets/artifacts. .NET10 runtime container dùng8080 theo env ở lab; chạy non-root USER app khi image hỗ trợ. Tags10.0 nhận servicing updates nhưng không immutable; release dùng digest lưu trong inventory và rebuild có lịch. Build image không chứng minh startup/config/database. Không COPY .env vào image và không dùng build ARG để đưa secret vào layers.

<a id="lo-w22-02"></a>

## Bài 2: Compose networking và volumes

LO-W22-02 · W22-C02

Compose tạo network với DNS theo service name: API gọi db:1433/redis:6379, không localhost. Host browser dùng published API port8080. SQL volume /var/opt/mssql và Redis volume tùy durability design; container deletion không đồng nghĩa xóa named volume. depends_on chỉ service_started chưa bảo đảm DB sẵn; healthcheck/service_healthy hoặc retry bounded/app readiness cần thiết. Lab dùng SQL Server2022 Developer local, không coi license lab là production grant. Secrets được env/secret injection ngoài tracked files. Không publish DB/Redis ra internet; local SQL port nếu cần bind127.0.0.1. Reset test volumes chỉ khi xác nhận là dữ liệu test riêng.

<a id="lo-w22-03"></a>

## Bài 3: Startup và failure recovery

LO-W22-03 · W22-C03

Migrations là schema change có review, không để mọi replica tự Database.Migrate đồng thời ở production. Local lab chạy migrations từ host/tool một lần sau DBready; image app có schema expectation rõ. Readiness kiểm dependency cần cho request; liveness kiểm process còn hoạt động, không buộc DBhealthy khiến restartstorm. Redis cache optional: outage degrade về SQL và log/metric, không mất write. Graceful shutdown ngừng nhận mới, cancel work hợp lý; container restart không làm transaction đã commit mất. Volume cần backup; volume không là backup độc lập. Verify restart bằng fixture counts và ownership cases.

## Ví dụ giải từng bước

1. Repo layout src/Expense.Api/Expense.Api.csproj, assembly Expense.Api.dll. Dockerfile từ root:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Expense.Api/Expense.Api.csproj src/Expense.Api/
COPY src/Expense.Domain/Expense.Domain.csproj src/Expense.Domain/
RUN dotnet restore src/Expense.Api/Expense.Api.csproj
COPY src/Expense.Api/ src/Expense.Api/
COPY src/Expense.Domain/ src/Expense.Domain/
RUN dotnet publish src/Expense.Api/Expense.Api.csproj -c Release -o /out --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_HTTP_PORTS=8080
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet","Expense.Api.dll"]
```
2. Nếu project references thư viện domain, COPY csproj/code của các thư viện và restore toàn solution; không dùng Dockerfile này nguyên xi khi layout khác.
3. Compose db(SQL) volume, redis, api network; inject ConnectionStrings__Finance trỏ db:1433. Kiểm container logs và GETping.
4. Seed100000, restart api/db rồi report vẫn100000. Dừng Redis nếu chưa cache thì app vẫn đọc SQL; tuần26 bổ sung fallback.

## Đào sâu và lỗi thường gặp

SQL password với ; cần connection string escaping bằng builder, không nối string tùy ý. TrustServerCertificate chỉ lab; production verify certificate. Compose là lab/local, không tự cung cấp autoscaling hoặc managed backups.

## Bài tập thực hành

### Bài tập 1

API báo connection refused tới localhost1433 trong container.

### Bài tập 2

Restart container rồi mất dữ liệu; kiểm gì?

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Đổi server=db,1433 theo network; kiểm SQLready/log/credential. localhost là API container, không DBservice. Dùng bounded retry, không loop vô hạn.

### Hướng giải 2

Xem mount SQL data có namedvolume đúng /var/opt/mssql không; kiểm docker compose down -v đã xóa volume chưa. Khôi phục testfixture/backup, không xóa production.

## Project task

Đóng gói API+SQL+Redis local bằng Compose, thêm Dockerfile/.dockerignore/env instructions và runbook restart/migrate. UI có thể devserver ở host tuần này; cloud buildstatic tuần24.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w22.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Docker build và API container chạy non-root, GETping200, DBhostname db.

- [ ] Restart SQL giữ Alice100000/Bob0; volume path và backup procedure được ghi.

- [ ] Secrets không nằm image/commits; dependency-not-ready có lỗi rõ/retry bounded.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Giải thích image/container/volume/network trong English90 giây; đọc một Docker log không đọc secret.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Build and runtime stages](https://docs.docker.com/build/building/multi-stage/) — W22-C01; primary. HTTP: 200; section xem ledger.

- [Service DNS and host/container ports](https://docs.docker.com/compose/how-tos/networking/) — W22-C02; primary. HTTP: 200; section xem ledger.

- [Startup order versus readiness](https://docs.docker.com/compose/how-tos/startup-order/) — W22-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-22-quiz.md) · [Đáp án](../../assessments/answer-keys/week-22-answers.md) · [Flashcards](../../flashcards/week-22.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-21.md)

[Tuần tiếp](week-23.md)
