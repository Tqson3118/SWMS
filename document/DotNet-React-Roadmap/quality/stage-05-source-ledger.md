# Stage 05 source ledger — draft, preliminary

This ledger maps the manual activity to the learner source in `5.md` and its
Stage 04 prerequisite. It is an authoring record only. No implementation,
execution, source approval, safety approval, or Stage 05 completion is asserted.
The positive evidence below means evidence a learner should produce from their
own scratch work; it is not a result supplied by this package.

| Claim ID | Source locator | Scoped claim / teaching boundary | Observable evidence and positive rubric | Status / review boundary |
|---|---|---|---|---|
| W05-C01 | `5.md:7-43`, “Hợp đồng dữ liệu dùng xuyên suốt Chặng 5–7”; `4.md` schema prerequisite | API returns DTOs rather than EF entities. The shared contract uses `categoryId`, `amount`, `spentOn`, optional `note`, and response `id`, `categoryId`, `categoryName`, `amount`, `spentOn`, optional `note`. | Learner records exact request/response JSON from a real scratch run and shows that the response shape matches the contract without a nested category or entity serialization. | draft; preliminary; safety_review false; root review required |
| W05-C02 | `5.md:72-101`, “Tuần 1: HTTP và endpoint đầu tiên”, Buổi 1–2 | HTTP methods/statuses and endpoint design are conventions whose exact semantics must be documented by the API. The source names GET/POST and common 200/201/400/401/403/404/409/500 cases. | Learner records method, route, input, observed status, and response for GET list, GET by id, POST, empty/not-found, and validation paths. PUT/DELETE semantics are recorded only if implemented. | draft; preliminary; do not invent run results |
| W05-C03 | `5.md:103-288`, Buổi 3; `4.md` schema, joins, parameter safety, transactions | EF Core request-to-database slice uses `ExpenseApiDbContext`, Category–Expense navigation, a separate `ExpenseApiDb`, migration, async calls, and DTO projection. | Learner identifies actual context/configuration, migration name, schema evidence, and projection; records observed SQL/log evidence when available. Scratch database only. | draft; preliminary; no production or destructive evidence |
| W05-C04 | `5.md:290-356`, Buổi 4–6 | DI lifetime and query behavior are conditional: AddDbContext’s scoped default can be changed; projection/AsNoTracking support read-only queries; N+1 must be observed or reasoned about under the actual query/provider. | Learner records registration/lifetime decision and a measured or inspected projection/query observation, without claiming Include is always one SQL query or DbContext is universally scoped. | draft; preliminary; root technical review required |
| W05-C05 | `5.md:358-364`, Buổi 7 | DTO validation should reject invalid category/amount/date and should not accept a database entity as the public contract. | Learner records exact invalid inputs and actual 400/validation response shape, including binding/date and missing-category cases where exercised. | draft; preliminary; no fabricated errors |
| W05-C06 | `5.md:366-378`, Buổi 8 | Async/cancellation and ProblemDetails/exception handling are boundaries. Production responses should omit stack traces/secrets while server logs may carry safe correlation context. | Learner identifies cancellation token and exception middleware boundaries, records actual failure response/log observations, and lists unavailable paths as incomplete. | draft; preliminary; safety false |
| W05-C07 | `5.md:380-382`, Buổi 9 | Logging and API checks must be searchable and must not expose passwords, tokens, connection strings, or sensitive bodies. | Learner attaches a safe log excerpt or structured-field observation and actual .http/curl/Postman request/status records, with secrets redacted and no invented output. | draft; preliminary; root review required |
| W05-C08 | `5.md:384-392`, Buổi 10 | Authentication identifies the caller and authorization controls permission. JWT signing supports integrity/source checks but base64url is not encryption; signing keys belong outside source. Cookie/Identity and bearer choices have tradeoffs. | Learner states the selected flow, records actual 401/403/authorized observations, and explains applicable HTTPS/storage/CSRF/XSS boundaries without logging credentials or claiming auth when blocked. | draft; preliminary; no auth approval |
| W05-C09 | `5.md:394-398`, Buổi 11 | The project slice broadens to date filtering/pagination, POST/PUT/DELETE, validation, ProblemDetails, logging, migration, tests, and auth after CRUD; checkpoint follows middleware → endpoint/service → EF → database → response. | Learner provides an end-to-end trace with actual artifacts and explicitly labels missing/deferred parts. | draft; preliminary; full-stage gate remains open |
| W05-C10 | `5.md:74-88`, common status list; `5.md:396`, later project boundary | 409 is a documented HTTP conflict status, not proof of optimistic concurrency. Stage05 may record a conflict only if the API actually implements and observes one; rowversion/EF concurrency proof is deferred to the later concurrency stage. | Learner records a real conflict response if present, otherwise marks it deferred; no invented 409, retry, rowversion, or “five success/five failure” outcome. | draft; preliminary; root mapping/safety review required |
| W05-C11 | `5.md:400-402`, “Dùng AI”; `README.md` safety | AI output must be checked against concrete conditions; no secrets, real credentials, or unsafe data in the learner project/evidence. | Learner records one checked assumption (for example lifetime, query count, JWT encryption, or Wait behavior) and the evidence/condition that bounds it, with no secret material. | draft; preliminary; safety false |

## Review state

- `source_status`: draft
- `evidence_status`: preliminary
- `mapping_reviewed`: false
- `safety_reviewed`: false
- `execution_status`: not run by author
- `approval_status`: pending root review

The source-ledger rows are intentionally scoped to observable learner evidence;
source verification, execution, integration, browser, and course-gate checks
remain open for the root owner.


## Root128 operational review — 2026-10-04

Root read the complete actual numbered lesson, all practice sessions and final checkpoint, plus README learner sequence; compared the three bounded source-to-task mappings and positive rubric. Stage04 explicit prerequisite and actual-commit completion criterion, Stage05 lifetime/query/AI assumption evidence clarified. Operational source-to-task mappings only accepted. source_status stays draft/evidence preliminary; technical source verification, independent project execution, full stage checks, DOCX parity and browser/course approval remain open. No actual learner result asserted.
