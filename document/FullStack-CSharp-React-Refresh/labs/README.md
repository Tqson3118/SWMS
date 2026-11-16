# Labs cho chặng 4–7

Các đoạn code ngắn trong tuần minh họa một cơ chế; không được coi là server đầy đủ đã secure/deploy. Dùng [contract](../project-spec.md) và [setup](../setup.md), mở đúng tuần và ghi evidence trong repo học viên.

| Chặng | Thứ tự thực hiện | Bằng chứng tối thiểu |
|---|---|---|
| API W13–16 | ping/errors → EF mappings/migration → owner auth → CRUD/version → import/test | HTTP matrix, generated SQL, real SQL tests, auth failure cases |
| React W17–20 | typed fixture → routes/fetch race → forms/auth/conflict → dashboard/tests | strict build, reverse request order, field errors, two-user session |
| Production W21–24 | clone/config → Docker/network/volume → fresh CI/SQL tests → staging/restore | CI link, image digest, runbooks, smoke and restored fixtures |
| Design W25–28 | rate limit → cache/fallback → durable export/worker → demo/interviews | 429, cache race, crash/retry, owner job/file, portfolio evidence |

## EF relationship lab, W14

Entity IDs GUID, owner UserId GUID. Trong OnModelCreating:
```csharp
modelBuilder.Entity<Category>()
 .HasAlternateKey(c=>new {c.Id,c.UserId});
modelBuilder.Entity<Expense>()
 .HasOne(e=>e.Category).WithMany(c=>c.Expenses)
 .HasForeignKey(e=>new {e.CategoryId,e.UserId})
 .HasPrincipalKey(c=>new {c.Id,c.UserId})
 .OnDelete(DeleteBehavior.Restrict);
modelBuilder.Entity<Expense>()
 .Property(e=>e.Version).IsRowVersion();
```

Category và Expense là classes tự viết theo contract với navigation Category/Expenses và byte[]Version. Add DbContext scoped với UseSqlServer; gọi migration InitialFinance rồi đọc SQL: compoundFK, unique owner/categoryname và amount CHECK constraints phải có. Không chỉ dựa DataAnnotations vì DB invariants bảo vệ cả writes ngoài API.

Read query: AsNoTracking, Where owner/filter trước CountAsync/Select DTO, OrderByDescending SpentOn.ThenByDescending Id trước Skip/Take, truyền CancellationToken. Đo actual SQL query count và projection không lazy-loading category từng row.

PUT: load owner-scoped tracked entity, set OriginalValue của Version theo input Base64, update writable values, SaveChangesAsync; catch DbUpdateConcurrencyException →409. Invalid Base64→400. Wrongowner→404 trước update. Không attach entity từ request chứa UserId.

## HTTP regression matrix

| Input | Expected |
|---|---|
| valid create |201+Location |
| amount0/decimal/too large, invalid date |400 |
| missing/expired/wrong issuer/audience/signature token |401 |
| valid principal thiếu policy |403 |
| Bob GET/PUT/DELETE Alice expense |404, Alice unchanged |
| PUT stale version |409, first writer retained |
| DELETE own expense |204, no body |
| import3valid repeated/concurrent samekey/hash |3newrows once |
| import3rows oneinvalid |400,0newrows |
| import samekey differenthash |409 |
| fault before commit |500generic,0partialrows |

## Frontend and infrastructure integration

API URL được cấu hình public, CORS exactorigin. Test404/500 với response body khôngJSON: helper vẫn có genericmessage, không crash do parse. GETrace delayOct→switchNov phải giữNov. Logout phải cancel/ignore oldrequest và clear ownercache.

Dockerfile W22 là executable khi layout chỉ có Expense.Api; nếu nhiều csproj thì COPY/restore đủ dependencies. SQL readiness kiểm credentials/config sau startup; fixedsleep không gate. Fresh CI W23 cần SQL testfixture riêng; workflow baseline chưa provision SQL tự động.

W24 deployment, restore và rollback là thao tác học viên phải thực hiện và ghi results thật. W27 durable export cần claim condition/lease trong SQL, private artifact và idempotent JobId; queue delivery không cho phép bỏ owner validation ở worker.

