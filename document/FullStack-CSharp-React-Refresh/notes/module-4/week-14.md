# Tuần 14 — EF Core mapping, migrations và validation

Chặng 4; 3 bài L14-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W14-01: Map keys/relationships/rowversion và owner-scoped projection.

- LO-W14-02: Review/apply migration trên DB lab có dữ liệu mà giữ fixture.

- LO-W14-03: Validate DTO/domain/DB và map stale update sang409.

Tiên quyết: Đạt gate tuần 13; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-14-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w14-01"></a>

## Bài 1: DbContext và relationships

LO-W14-01 · W14-C01

DbContext scoped là unit-of-work/change tracker, không thread-safe. DbSet entity không phải public DTO. Category.HasAlternateKey(c=>new{c.Id,c.UserId}); Expense foreign key2col tới alternate key giữ ownership DB. Version byte[] IsRowVersion. Configure money CHECK, length, unique business keys và OnDelete Restrict theo spec. Read query AsNoTracking, Where owner/date, OrderBy/ThenBy, Skip/Take, Select DTO, ToListAsync(token). Không ToList toànDB rồi filter user. Tracking update load owned entity và đổi writable fields; không attach body entity với UserId tùy ý. Projection categoryName bằng navigation để query JOIN thay N+1.

<a id="lo-w14-02"></a>

## Bài 2: Migration và upgrade

LO-W14-02 · W14-C02

dotnet ef migrations add Initial --project src/Expense.Api, database update vào lab. Migration có schema change và model snapshot, không sửa snapshot tùy tiện. Thêm index/length rồi review generated SQL. Rename bị detect drop/add có thể mất dữ liệu; chỉnh rename intent và thử trên clone populated fixture. Không chạy migrations đồng thời mọi replica startup production; release job có quyền migration, runtime ít quyền hơn. Provider/Design/tool EF10 tương thích cùng major. SQLServer migration không mang nguyên sang PostgreSQL. Seed fixed key idempotent cho lab; data user động không dùng HasData tùy tiện. Sau upgrade test query/output/constraints, file tồn tại chưa chứng minh safe.

<a id="lo-w14-03"></a>

## Bài 3: Validation và concurrency

LO-W14-03 · W14-C03

DTO guard shape/range cho feedback, domain invariant cho mọi caller, DB constraint cho race/direct writes. DataAnnotations Range long1..1e9,StringLength200; ApiController automatic400. Category owner check từ principal dù FK có layer DB. Version request Base64 làm OriginalValue concurrency token, không set Version như user writable data. EF stale save DbUpdateConcurrencyException; scoped missing404 hoặc existing stale409. Không retry silently overwrite latest vì mất user intent. Classify known constraint errors sang400/409; unexpectedDB500generic. EFInMemory không enforce relational FK/transaction/SQL translation nên integration gate dùng actualSQLServerlab.

## Ví dụ giải từng bước

1. Bài toán: listDTO, stable paging và noN+1.
2. Query composition trước materialize.
3. Đoạn trong service, db là ExpenseDb DI và fields theo project-spec:
```csharp
var items=await db.Expenses.AsNoTracking()
 .Where(e=>e.UserId==userId&&e.SpentOn>=from&&e.SpentOn<to)
 .OrderByDescending(e=>e.SpentOn).ThenByDescending(e=>e.Id)
 .Skip((page-1)*pageSize).Take(pageSize)
 .Select(e=>new{e.Id,e.CategoryId,e.AmountVnd,e.SpentOn,e.Note})
 .ToListAsync(cancellationToken);
```
4. Page1/20 Alice ->3items; SQL có owner filter. Generated SQL/query log chứng minh server translation. Context/entity tự tạo ở bài tập mapping, không snippet console độc lập.

## Đào sâu và lỗi thường gặp

Không AsNoTracking cho update rồi mong tracker tự biết mutation. Query transaction/unit-of-work không là DI lifetime tự động. Explicit concurrency token phải từ editor version, không latest chỉ vì vừa load DB.

## Bài tập thực hành

### Bài tập 1

Map compound Category FK và test cross-owner insert.

### Bài tập 2

StalePUT vẫn thắng; kiểm mapping gì?

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Category alternate key(Id,UserId); Expense HasForeignKey(CategoryId,UserId).HasPrincipalKey(Id,UserId).OnDelete(Restrict). ActualDB reject BobCategoryAlice, UI/API400/404 theo contract.

### Hướng giải 2

IsRowVersion và OriginalValue từ request Base64. Nếu bỏ requestVersion và loadlatest thì EF không biết stale editor. Test 2 clients,first200,second409.

## Project task

Nối EF/SQL,CRUD/categories/budgets/report,migrations và owner-scoped query. Giữ DTO/domain boundaries.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w14.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Migration trên lab giữ fixture và constraints; SQL translation đúng.

- [ ] ValidCRUD201/200/204,invalid400,otherowner404,stale409.

- [ ] List/count<=2queries,SQLscope+paging trước materialize.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'What does EF track, and why use AsNoTracking for reads?'

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Foreign and principal keys](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/foreign-and-principal-keys) — W14-C01; primary. HTTP: 200; section xem ledger.

- [Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/) — W14-C02; primary. HTTP: 200; section xem ledger.

- [Model validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0) — W14-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-14-quiz.md) · [Đáp án](../../assessments/answer-keys/week-14-answers.md) · [Flashcards](../../flashcards/week-14.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-13.md)

[Tuần tiếp](week-15.md)
