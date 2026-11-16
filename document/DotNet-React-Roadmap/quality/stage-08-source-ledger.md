# Stage 08 source ledger — draft, preliminary

This ledger maps the one manual activity to `8.md`, with Stage 07 as the
explicit prerequisite. It records positive learner evidence only; no build,
runtime, database, browser, package or learner execution is asserted.

| Claim ID | Source locator | Scoped claim / boundary | Observable evidence and positive rubric | Review state |
|---|---|---|---|---|
| W08-C01 | `8.md`, objectives; Tuần 1 Buổi 1–2 | Value/reference behavior, record equality, entity identity, generic/delegate/lambda and Map are distinguished with code. | Learner records actual outputs and explains why entity identity differs from record value equality; generic and delegate results are shown. | draft; preliminary; safety false |
| W08-C02 | `8.md`, Tuần 1 Buổi 3–4 | LINQ pipeline construction is distinguished from execution/snapshot; IQueryable translation and runtime nullable/error validation have boundaries. | Learner records deferred versus ToList timing, actual SQL/log or unavailable translation evidence, TryParse/validation and exception responsibilities. | draft; preliminary; safety false |
| W08-C03 | `8.md`, Tuần 3 Buổi 5–6; project cancellation slice | Streams/resources are disposed; async does not imply a new thread; cancellation is cooperative and tokens propagate from a frontend cancel action through API/service/reader/database; shared DbContext/blocking waits are unsafe boundaries. | Learner records streaming/disposal, click/event-to-request cancellation, token path, cancelled UI state, no-hang and normal-error distinction. An unavailable browser/UI path is an explicit blocker and earns no point. | draft; preliminary; safety false |
| W08-C04 | `8.md`, project and edge data | Importer contract separates input rows from EF entities; CSV validates row fields and reports errors while handling file/cancellation edges. | Learner records valid/invalid counts, culture/category/date/duplicate handling, empty/header-only/missing-column/large-file cases and pre/mid/post-batch cancellation. | draft; preliminary; safety false |
| W08-C05 | `8.md`, project strategy/checkpoint | Partial database behavior is explicit rather than implied; focused parser/cancellation/report tests and timed debugging support claims. | Learner states read-then-confirm or batch strategy, batch result/report, actual tests and timed code-reading outcome. | draft; preliminary; safety false |

## Review state

- `source_status`: draft
- `evidence_status`: preliminary
- `mapping_reviewed`: true
- `safety_reviewed`: true
- `execution_status`: not run by author
- `approval_status`: bounded manual mapping only; full source/runtime/execution approval open

## Prerequisite handoff

The parent155 review is retained: Stage 08 requires all listed equality,
generic/delegate/map, deferred/EF, runtime-boundary, streaming/disposal,
cancellation, no-shared-context, CSV-edge, partial-commit and test evidence.
Stage 07 remains a sequence prerequisite. Blockers are incomplete and earn no
positive point.

## Root bounded review203 — 2026-10-04

Actual complete local lessons and operational activity mappings reviewed; independent193 identified Stage08 frontend cancellation omission, repaired196 and root actual diff accepted. Existing snippet154 results/run/source exist; wrong requested review filename corrected, no fabricated replacement. Scratch-only learning data, no arbitrary reflection, no shared database context, positive rubric and honest blocked/incomplete evidence boundaries reviewed. This is a bounded activity safety/mapping review, not full official-source verification, full stage runtime/learner/browser approval. Draft/preliminary remains.


## Current reference and teaching integration1050 — 2026-10-08

The historical rows above describe generic learner implementations and their review history. The current reference is an atomic whole-file transaction, not a partially committed batch example. Server-confirmed499/cancelled means precommit cancellation with zero accepted/read/duplicate counts in this fixture; browser abort or unreadable transport remains unconfirmed. GET cannot attribute rows to a particular request. Validation rejection has acceptedRows0 but readRows/duplicates can be nonzero. Postcommit diagnostics leave confirmed commit intact; simulated commit-unknown is not real lost-transport proof.

| Claim ID | Current learner source / scope | Observed evidence / boundary | Review state |
|---|---|---|---|
| W08-C03 | 8.md exercise/reference; activities/manual-stage-08.yaml; integration/frontend README/controller | Cooperative server token versus browserAbort/unconfirmed, no automatic POST retry, explicit manual reconciliation barrier; unavailable browser remains incomplete. | Teaching1039/1045 bounded accepted; real browser journey open |
| W08-C04 | reference/stage-08-async and stage-08-persistence; integration/api source/README | Actual CSV parser/persistence, exact decimal strings/calendar/category/caps and duplicate-only skipping; retained read lease. | Parser909/persistence947/API990 bounded acceptance; no multiuser/auth production approval |
| W08-C05 | integration/api and frontend tests/README | API26tests17newretainedfixtures, frontend19tests/build; root1031 actual same-origin Vite/API commit/reject/GET in one new retained fixture, both helpers stopped. | Retained evidence, not learner execution or full-stage/course approval |

Manifest declares current reference source and dependency metadata, excluding bin/obj/node_modules/dist/test logs/private evidence. Retained DOCX8_CSharp_Chuyen_Sau.docx is retained outside this manifest; it has not received current1041 teaching additions. Learners use current8.md and its four reference READMEs for this slice; DOCX parity remains an internal publication task. Stage07 prerequisite remains; Stage11 auth and later stages remain course gaps. All source_status/evidence_labels remain draft/preliminary; no new whole-course source approval or runtime approval is granted by hash refresh.
