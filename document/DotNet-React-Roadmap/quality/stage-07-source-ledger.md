# Stage 07 source ledger — draft, preliminary

This ledger maps the manual activity to `7.md`, with Stage 05 API and Stage 06
browser/fetch/TypeScript work as explicit prerequisites. It records positive
learner evidence requirements only; no React run, browser test, build, or
source approval is asserted.

| Claim ID | Source locator | Scoped claim / boundary | Observable evidence and positive rubric | Review state |
|---|---|---|---|---|
| W07-C01 | `7.md:7-22`, Chuẩn bị | React project setup must identify actual package/API origins and keep VITE_ values free of secrets; CORS/proxy choice must be explicit. | Learner records versions, origins, local configuration boundary, and chosen CORS/proxy behavior without credentials. | draft; preliminary; safety false |
| W07-C02 | `7.md:24-77`, Tuần 1 Buổi 1–2 | Components receive props, stable keys render lists, controlled fields use state, and updates preserve immutability/functional-updater rules. | Learner identifies component responsibilities, props/callbacks, key choice, controlled fields, and observed valid/invalid/multiple-submit behavior. | draft; preliminary; safety false |
| W07-C03 | `7.md:79-163`, Tuần 2 Buổi 3 and POST slice | useEffect synchronizes external APIs; model idle/loading/ready/error, cleanup with AbortController, dependencies, AbortError handling, and runtime validation. | Learner records actual fetch states, cleanup/abort, dependencies, malformed-response handling, and the shared categoryId/categoryName contract. | draft; preliminary; safety false |
| W07-C04 | `7.md:165-172`, Buổi 4 | Derived values belong in render or a small state machine rather than unnecessary effects; state transitions should be explicit. | Learner shows one derived total/filter or state transition and explains its actual source/state boundary. | draft; preliminary; safety false |
| W07-C05 | `7.md:174-221`, Buổi 5 | Routes/layout and URL-not-found handling are client concerns; hosting must support index fallback on refresh. | Learner records actual expense/category routes, shared layout, unknown URL behavior, and hosting fallback assumption. | draft; preliminary; safety false |
| W07-C06 | `7.md:223-231`, Buổi 6–7 | Lift state only to the lowest common consumer; Context has rerender tradeoffs; error boundary handles render subtree errors, not async event/effect errors. | Learner records a justified lifting/Context choice, rerender boundary, and separate render/async error observations. | draft; preliminary; safety false |
| W07-C07 | `7.md:233-258`, completion and delivery | Accessible UX includes loading/empty/error/success, focus/labels/keyboard/status; API checks and build/config/deployment assumptions must be recorded. | Learner records actual API/browser/build evidence, one click-to-response trace, origins/HTTPS/refresh assumptions, and missing paths. | draft; preliminary; safety false |
| W07-C08 | `7.md:260-271`, AI and official sources | AI suggestions require acceptance tests and must exclude tokens, passwords, connection strings, and user data. | Learner records one edge-case check and redacted prompt/evidence boundary. | draft; preliminary; safety false |

## Review state

- `source_status`: draft
- `evidence_status`: preliminary
- `mapping_reviewed`: true
- `safety_reviewed`: true
- `execution_status`: not run by author
- `approval_status`: bounded manual task mapping accepted by root; full stage/source/execution approval open

## Root review — Phase33 parent143

Complete actual lesson and README task comparison accepted. Stage07 includes setup/origins, components/controlled/immutable state, effects/dependencies/abort and unknown JSON, derived state, routing/fallback, Context/render versus async errors, accessible delivery and click-to-response trace. Explicit prerequisites and scratch-only learning-data boundary retained. Root clarified required positive evidence: a documented blocker is incomplete and never earns an unperformed rubric point. This is an activity mapping review, not completed learner execution, browser/deployment proof or comprehensive external-source verification. Draft/preliminary labels retained.
