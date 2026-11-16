# Guided SQL reference — Chặng 4

Đây là lát cắt instructor cho schema SQL Server của Sổ chi tiêu. Nó chạy provider thật trên LocalDB, không phải test giả lập. Mỗi lần chạy tạo database học mới có tên `RoadmapSql735_<32 hex>` và giữ lại database cùng dữ liệu để kiểm tra sau. Không dùng database cũ, không DROP/reset database, và không tự động chuyển dữ liệu JSON của Stage 3.

## Chạy

Từ thư mục này, với .NET SDK `10.0.401`:

```powershell
dotnet restore .\tests\ExpenseSqlGuided.Tests\ExpenseSqlGuided.Tests.csproj
dotnet test .\tests\ExpenseSqlGuided.Tests\ExpenseSqlGuided.Tests.csproj --logger "trx;LogFileName=DN45-2a.trx"
dotnet run --project .\src\ExpenseSqlGuided\ExpenseSqlGuided.csproj
```

Expected test result: `Passed! - Failed: 0, Passed: 7`. The console creates and prints a fresh database name before creation, then prints `Rows: 3` and `January total: 125.50`. Actual database names and full test output are retained in [validation/dotnet-sql752](../../../tasks/course-remediation-2026-09-30/validation/dotnet-sql752/results.json); the TRX is retained at `tests/ExpenseSqlGuided.Tests/TestResults/DN45-2a-final.trx`.

## Schema and handoff

`Categories` has identity `Id`, a non-null `Name`, explicit `Latin1_General_100_CI_AS` collation, and a unique constraint. `Expenses` has identity `Id`, a non-null FK `CategoryId`, `DECIMAL(18,2)` nonnegative `Amount`, SQL `DATE` `SpentOn`, and nullable `Note NVARCHAR(500)`. The database generates expense IDs, so a Stage 3 JSON identity is not silently migrated. The 727 domain used caller supplied positive IDs and unrestricted `decimal`; this SQL boundary uses database generated IDs and `decimal(18,2)`.

The store trims and bounds category names to 100 characters, bounds notes to 500 characters, and rejects negative, over-precision, or over-range amounts before opening a provider command. SQL `DECIMAL(18,2)` is exact storage; the guided domain rejects values beyond two fractional digits instead of silently allowing provider rounding. The store uses parameters for CRUD, joins category names, returns unused categories with a `LEFT JOIN`, groups monthly totals across years/months, and paginates with `ORDER BY e.Id OFFSET/FETCH`. Page and page-size overflow are rejected. Tests retain zero, NULL note, exact SQL error numbers for NOT NULL/CHECK/FK/UNIQUE, deterministic page1/page2 boundaries, quote-containing text, changed update/delete values, multiyear totals, and transaction visibility/rollback. All generated dates are learning data only.

Progressive hints: first inspect the two tables and their constraints; then identify which invalid write fails at the database boundary; then read the `JOIN`, `GROUP BY`, `ORDER BY`, and transaction code. Model reasoning: constraints protect invariants even when another caller bypasses the guided store; parameters keep values separate from SQL text; a unique order key makes page boundaries deterministic; rollback returns the owned learning database to its pre-transaction row count.

This is a guided provider slice. It does not implement an API, full learner gate, SQL migration, or course/stage approval.

## Primary sources

The evidence folder retains the actual retrieved HTML bodies, hashes, URLs, timestamps, and locators for Microsoft Learn sources covering `CREATE TABLE` constraints and precision/scale, parameterized `SqlParameter`, `ORDER BY`/`OFFSET FETCH`, and local transaction rollback.
