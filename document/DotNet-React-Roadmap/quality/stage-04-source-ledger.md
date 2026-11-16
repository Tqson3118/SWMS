# DotNet-React-Roadmap Stage 04 manual source ledger

**Package:** `package:dotnet-react:stage-04`  
**Status:** `draft`  
**Scope:** A bounded guided activity derived from `DotNet-React-Roadmap/4.md`. It does not certify the complete Stage 04 gate, every SQL Server assertion, EF Core integration, or independent learner checks.

| Claim ID | Source location | Operational claim used by this activity | Evidence boundary |
|---|---|---|---|
| W04-C01 | `4.md` Chuẩn bị; Tuần 1 Buổi 1–2 | The learner creates Categories/Expenses in a scratch database with keys, foreign key, decimal precision/scale, NULL semantics, CHECK/UNIQUE behavior, sample data, and an explicit delete rule, then records actual constraint observations. | The evidence must identify the scratch database and actual rows/errors. No live database, successful command, or SQL Server compatibility result is claimed by the package. |
| W04-C02 | `4.md` Tuần 2 Buổi 3–4 | The learner records hand-checkable results for filtered/ordered and paginated SELECTs, INNER/LEFT JOINs, GROUP BY with SUM/COALESCE, and HAVING, including an unused category. | Results must come from the learner’s own sample and be recorded; no query output or totals are fabricated. The activity does not claim optimizer or production-performance behavior. |
| W04-C03 | `4.md` Tuần 3 Buổi 5–6; Bài kiểm tra chặng | The learner previews UPDATE/DELETE with the same WHERE, records row counts, explains parameterization, and demonstrates rollback plus an intentional commit or an environment blocker. | A stored procedure is not treated as proof of injection safety. No real data mutation, destructive operation, transaction success, ten-row gate, or EF Core integration is claimed without learner evidence. |

## Review gate

This ledger records three bounded source-to-task mappings. `source_status` remains `draft`, each manifest concept keeps `evidence_label: preliminary`, and `safety_review.reviewed: false` until root performs the actual lesson/source and package review. Static, native, preview, learner, browser, DOCX, and whole-course gates remain open. No learner result is included.


## Root128 operational review — 2026-10-04

Root read the complete actual numbered lesson, all practice sessions and final checkpoint, plus README learner sequence; compared the three bounded source-to-task mappings and positive rubric. Stage04 explicit prerequisite and actual-commit completion criterion, Stage05 lifetime/query/AI assumption evidence clarified. Operational source-to-task mappings only accepted. source_status stays draft/evidence preliminary; technical source verification, independent project execution, full stage checks, DOCX parity and browser/course approval remain open. No actual learner result asserted.
