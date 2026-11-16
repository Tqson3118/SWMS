# Stage 8 — C# chuyên sâu và async (instructor reference)

This small expense-themed reference is a runnable teaching aid for `8.md`. It is not an import feature, API, database, authentication, production, or learner-attainment claim. The code uses SDK `10.0.401`, target `net10.0`, and has no `Task.Run`-as-async example, sleeps, external service, or shared database.

## Run from PowerShell

```powershell
Set-Location D:\03_Study\Roadmap\DotNet-React-Roadmap\reference\stage-08-async
dotnet --version                         # expected: 10.0.401
dotnet restore .\tests\AsyncReference.Tests\AsyncReference.Tests.csproj
$runId = [guid]::NewGuid().ToString('N')
$run = Join-Path (Get-Location) ("learner-evidence-" + $runId)
New-Item -ItemType Directory -Path $run -ErrorAction Stop
$env:STAGE8_FIXTURE_DIR = Join-Path $run 'fixtures'
New-Item -ItemType Directory -Path $env:STAGE8_FIXTURE_DIR -ErrorAction Stop
dotnet test .\tests\AsyncReference.Tests\AsyncReference.Tests.csproj --no-restore --filter FullyQualifiedName~CsvImportTests --logger ("trx;LogFileName=" + $runId + ".trx") --results-directory $run 2>&1 | Tee-Object (Join-Path $run 'test.log')
```

The saved historical 883 evidence uses separate `red4`, `green3`, and `green4` directories; its intentionally bad assertion is retained as historical metadata and is not a RED proof. The corrected CSV boundary evidence uses `validation/dotnet-async-correct899/red-behavior` and `green-final`; the earlier 892 fixtures and logs remain historical. Learner runs must generate a new GUID directory and must not overwrite retained TRX or stdout.

## What the reference demonstrates

`Models.cs` maps the lesson's first two weeks to expense data. `ExpenseKey` is a value-like record struct; `ExpenseEntity` remains an ordinary reference object, so two objects with equal fields are not automatically equal. `ExpenseQueries.PositiveAmounts` is deferred until enumeration; calling `ToList` creates a deliberate snapshot. `Map` accepts a generic source/result pair and a delegate. `InputParsing.TryParseDate` handles nullable user input at a runtime boundary and returns a boolean instead of using an exception for ordinary invalid input.

`AsyncImport.cs` demonstrates resource ownership and cooperative cancellation. `ImportAsync` owns an `IAsyncDisposable` with `await using`, checks the token before each row, and returns a report when cancellation occurs. A mid-operation cancellation keeps accepted rows and labels the result `partial-output-retained`; it does not pretend that a database transaction or external side effect was rolled back. Cancellation before reading retains zero rows. Cancellation after the final row is complete is successful because no further boundary is pending. The tests drive cancellation through a callback, so they do not depend on timing or sleeps.

`OwnedCounter` protects a small synchronous state transition with a private lock. No `await` occurs inside the lock and no `DbContext` or mutable collection is shared across operations. A real system may instead use ownership isolation or a channel; this example does not claim distributed synchronization.

## Actual async CSV stream boundary

`CsvImport.cs` adds the first real I/O prerequisite for the later import project. `CsvExpenseImporter` opens a `FileStream` with asynchronous/sequential options, wraps it in `StreamReader`, and exposes the physical rows through `IAsyncEnumerable<string>`. `await foreach` checks cancellation before each row, so a pre-cancelled file reads zero rows and a deterministic callback can stop after a known source row. `await using` closes the actual stream on success, cancellation, and the injected post-parse I/O exception; the tests prove closure by reopening the retained fixture with `FileShare.None`. No current fixture deletion is required. The fault is injected after a source row is parsed; this is not evidence of an operating-system read failure.

The bounded CSV grammar is `categoryId,amount,spentOn,note` with four columns. Fields may contain commas inside double quotes and escaped quotes written as `""`; junk after a closing quote and quotes in unquoted text are rejected. A quoted field may not span a physical line; an unclosed quote is reported as `unsupported-multiline`. The first physical line must be the exact header; a blank or wrong header stops before data rows. Amounts use invariant culture, non-negative values, scale 0–2, and the configured maximum. Dates are exact `yyyy-MM-dd`; categories must be in the configured set; an exact duplicate of the complete `(categoryId, amount, spentOn, note)` row is skipped. Different amounts or notes on the same category/date remain valid expenses. The report distinguishes all physical data rows (`SourceRows`) from accepted and skipped rows and carries line-numbered errors; rows with errors produce `complete-with-errors`. It covers empty and header-only files, missing columns, malformed dates, category/amount/note bounds, duplicates, pre-cancellation, mid-read cancellation, and deterministic post-parse fault injection. Caller options and the category set must be non-null; null-option behavior and CSV cancellation after the final source row have no dedicated retained tests. The simpler callback-only example has a separate final-row test and does not establish the CSV boundary.

This is still an in-memory report boundary. A subsequent slice must choose and test SQL batch/transaction semantics, then connect API cancellation and frontend cancelled/partial states. This reference does not claim persistence, API, UI, or learner completion.

## Mapping to `8.md`

| Lesson objective | Reference and check |
| --- | --- |
| value/reference/equality | `ExpenseKey_records_compare_by_value_but_expense_entities_use_identity` |
| generic/delegate/LINQ | `Map`, `PositiveAmounts`, and typed test |
| deferred execution and snapshot | `Deferred_query_reads_new_expense_until_snapshot_boundary` |
| nullable and validation boundary | `TryParseDate` test; malformed input returns `false` |
| `await using` and disposal | `Async_resource_is_disposed_after_successful_stream` |
| cancellation before and during work | deterministic cancellation tests with retained report |
| safe shared state | `OwnedCounter` test with concurrent increments and no sleeps |
| async does not mean a new thread | explanation and code use asynchronous control flow without `Task.Run` |
| actual async file/CSV boundary | focused `CsvImportTests` (11 tests): stream, header/grammar, counts, bounds, cancellation, fault, disposal |

The later project in `8.md` asks learners to design CSV validation and a database write strategy. This reference deliberately stops at an in-memory stream/report boundary. It does not prove learner import correctness, API wiring, persistence, or full Stage 8 completion.

## Self-check and fault boundaries

1. Explain why `query` sees an expense added before enumeration but `snapshot` does not see one added after `ToList`.
2. Explain why equal `ExpenseEntity` fields do not imply equal objects, and why an EF entity should not be changed to a record casually.
3. Change the input to `"1,25"` and state which culture assumption is missing; then add an explicit culture only as a learner exercise.
4. Cancel before the first row and after row two. Record the report status and accepted amounts. Treat partial output as a state requiring a caller decision, not as an unreported success.
5. Remove `await using` temporarily and predict the disposal assertion. Restore it before the next run.
6. Replace the counter's private lock with an unsynchronized field only as a local experiment; do not infer production race behavior from one lucky run.

Expected learner evidence is a unique TRX/stdout directory, a short explanation of each boundary, and a note separating in-memory partial output from any future batch/database policy. Run only the focused new boundary while iterating:

```powershell
$id = [guid]::NewGuid().ToString('N')
$evidence = Join-Path (Get-Location) ("..\..\..\tasks\course-remediation-2026-09-30\validation\dotnet-async-correct899\learner-" + $id)
New-Item -ItemType Directory -Path $evidence -ErrorAction Stop
$env:STAGE8_FIXTURE_DIR = Join-Path $evidence 'fixtures'
New-Item -ItemType Directory -Path $env:STAGE8_FIXTURE_DIR -ErrorAction Stop
dotnet test .\tests\AsyncReference.Tests\AsyncReference.Tests.csproj --no-restore --filter FullyQualifiedName~CsvImportTests --results-directory $evidence --logger ("trx;LogFileName=" + $id + ".trx")
```

A passing reference test count is evidence for this reference only. Do not reuse a retained output directory or a retained database name.

## Official sources captured for this reference

The evidence directory contains full response bodies, status, byte counts, and SHA-256 hashes for these Microsoft pages:

- https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
- https://learn.microsoft.com/en-us/dotnet/standard/linq/deferred-execution-lazy-evaluation
- https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
- https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads

