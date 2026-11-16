# Stage 10 source ledger — draft, preliminary

This ledger maps the one manual activity to `10.md`, with Stage 09 as the
explicit prerequisite. It records source-scoped learner evidence only; no
build, runtime, SQL Server, browser, database, package or learner execution is
asserted.

| Claim ID | Source locator | Scoped claim / boundary | Observable evidence and positive rubric | Review state |
|---|---|---|---|---|
| W10-C01 | `10.md`, Tuần 1 Buổi 1–2; EF Core Efficient Querying; EF Core Pagination; EF Core Indexes | Query shape, projection, no-tracking, bounded page input, N+1 detection, plan/IO/time measurement and index trade-offs must be tied to an actual owner/date query. Page multiplication overflow needs an explicit bound or checked/long calculation. | Learner records SQL/plan or exact unavailable blocker, filter/order/page behavior, empty/oversized/overflow outcomes, N+1 evidence, and before/after logical reads/time with storage/write trade-off. | draft; preliminary; bounded mapping reviewed by root226; full approval open |
| W10-C02 | `10.md`, Tuần 1 Buổi 2; Tuần 2 Buổi 3; project; EF Core Transactions; EF Core Pagination | Projection/grouping and inclusive-start/exclusive-end period reporting are distinguished from entity loading; transaction atomicity and isolation are separate concerns. | Learner records monthly boundary/no-row/category cases, multi-step failure rollback/atomicity, stated isolation observation/limit and no implied universal isolation choice. | draft; preliminary; bounded mapping reviewed by root226; full approval open |
| W10-C03 | `10.md`, Tuần 2 Buổi 4; project; EF Core Migrations Overview; data-retention task | A migration on a populated learning database requires generated SQL, null/default/backfill/NOT NULL classification, count/schema verification and an optional authored retention/archival extension (not a stated 10.md task). | Learner preserves before count/copy, records migration application and after count/schema, separates empty/populated outcomes, and documents backup/rollback and non-destructive learning-data scope. | draft; preliminary; bounded mapping reviewed by root226; full approval open |
| W10-C04 | `10.md`, Tuần 3 Buổi 5; EF Core Handling Concurrency Conflicts; SQL Server rowversion boundary | Optimistic concurrency must preserve a winning update and expose a stable 409 contract; SQL Server rowversion mapping is provider/schema scoped and must not be generalized. | Learner records actual provider/schema model/migration/token, two stale updates, no lost update, API 409 and UI reload/compare behavior. Unavailable API/UI/browser evidence is incomplete and earns no point. | draft; preliminary; bounded mapping reviewed by root226; full approval open |
| W10-C05 | `10.md`, project/checkpoint/interview/debug | The one-million-row timed investigation requires hypothesis, query/plan/IO/time evidence, one controlled change, result and trade-off; interview answers must distinguish retry, statuses, transactions and concurrency. | Learner records actual commands/results and explicit blockers; snippets, assertions and unperformed tests do not count. | draft; preliminary; bounded mapping reviewed by root226; full approval open |

## Review state

- `source_status`: draft
- `evidence_status`: preliminary
- `mapping_reviewed`: true
- `safety_reviewed`: true
- `execution_status`: not run by author
- `reviewer`: root226 actual full lesson/activity/ledger mapping
- `approval_status`: bounded manual mapping only; full source/runtime/execution approval open

## Official primary-source access record

On 2026-10-04, the prerequisite review opened actual Microsoft Learn page
bodies (not search snippets) for:

- [EF Core Efficient Querying](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying): projection, limits, N+1, buffering/streaming and no-tracking sections.
- [EF Core Pagination](https://learn.microsoft.com/en-us/ef/core/querying/pagination): unique ordering and offset limitations.
- [EF Core Indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes): composite/index behavior and write/storage trade-off boundary.
- [EF Core Transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions): atomicity and SaveChanges transaction boundary.
- [EF Core Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/): model diff, generated migration/history and deployment considerations.
- [EF Core Handling Concurrency Conflicts](https://learn.microsoft.com/en-us/ef/core/saving/concurrency): optimistic token and conflict handling.

These bodies support the scoped teaching claims only. Access did not verify this
course project, provider runtime, SQL plan, database, browser/UI or learner
result. No source metadata or search snippet is treated as execution evidence.
