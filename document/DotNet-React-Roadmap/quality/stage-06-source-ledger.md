# Stage 06 source ledger — draft, preliminary

This ledger maps the manual activity to `6.md`, with Stage 05 as the API
prerequisite. Rows describe learner evidence to collect, not completed runs or
source approval.

| Claim ID | Source locator | Scoped claim / boundary | Observable evidence and positive rubric | Review state |
|---|---|---|---|---|
| W06-C01 | `6.md:7-41`, Chuẩn bị and Tuần 1 Buổi 1–2 | Browser markup should use semantic elements, associated labels, keyboard operation, and responsive layout choices. Breakpoints are not universal rules. | Learner records semantic form/list structure, keyboard submit, invalid/empty behavior, and observed 320/768/wide layout changes. | draft; preliminary; safety false |
| W06-C02 | `6.md:43-113`, Tuần 2 Buổi 3–5 | JavaScript data transformations should preserve boundaries; DOM code checks nullable elements, renders untrusted text safely, separates modules, and treats localStorage strings/JSON errors explicitly. | Learner records a real DOM submit/render/delete path, null or invalid-input boundary, module files, and missing/malformed storage behavior; no sensitive storage. | draft; preliminary; safety false |
| W06-C03 | `6.md:115-135`, Buổi 6 | fetch returns a Promise and does not reject solely for HTTP error status; network/abort/CORS/URL failures are different paths and response bodies are read once. | Learner records actual loading/success/empty/error, 404, abort/network/CORS or blockers and distinguishes status responses from rejected requests. | draft; preliminary; safety false |
| W06-C04 | `6.md:137-204`, Buổi 7 | TypeScript types describe compile-time shape; runtime checks remain needed for unknown server JSON. API integration uses `categoryId` request and `categoryName` response. | Learner records type/build evidence and a runtime validator or observed rejection for malformed/unknown JSON, using the shared contract. | draft; preliminary; safety false |
| W06-C05 | `6.md:206-212`, Buổi 8 and checkpoint | A plain JS/TS page should connect to the Stage05 API and document outcomes across 2xx/400/401/404/500, network error, empty, and malformed JSON where tested. | Learner supplies actual test table, origin/CORS boundary, run instructions, and an end-to-end explanation; unavailable browser/API remains incomplete. | draft; preliminary; safety false |
| W06-C06 | `6.md:214-224`, AI and official sources | AI suggestions and frontend security decisions need condition-specific checking; do not send password/token/form secrets. | Learner records one checked edge case and shows redacted evidence with no secret material. | draft; preliminary; safety false |

## Review state

- `source_status`: draft
- `evidence_status`: preliminary
- `mapping_reviewed`: true
- `safety_reviewed`: true
- `execution_status`: not run by author
- `approval_status`: bounded manual task mapping accepted by root; full stage/source/execution approval open

## Root review — Phase33 parent143

Complete actual lesson and README task comparison accepted. Stage06 includes arrays/empty-zero boundaries, semantic keyboard/responsive layout, safe DOM/modules/storage, fetch status versus rejection/body handling, runtime JSON checks and shared API DTO contract. Explicit prerequisites and scratch-only learning-data boundary retained. Root clarified required positive evidence: a documented blocker is incomplete and never earns an unperformed rubric point. This is an activity mapping review, not completed learner execution, browser/deployment proof or comprehensive external-source verification. Draft/preliminary labels retained.
