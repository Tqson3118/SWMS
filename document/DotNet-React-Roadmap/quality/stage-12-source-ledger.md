# Stage 12 bounded source and evidence ledger

Status: draft. Complete original lesson read; bounded operational mapping and selected primary-source sections reviewed on 2026-10-04. Full references, actual learner execution and course approval remain open.

| Claim ID | Scope | Locator / evidence basis | Boundary |
|---|---|---|---|
| W12-C01 | UI/server state and reducer transitions | 12.md state/reducer sections; authored operational evidence | An Expense[] reducer alone cannot encode all loading/error states; library choice optional. |
| W12-C02 | URL filters, caching and invalidation | 12.md URL/server-state sections; authored operational evidence | Refresh/history/reset and mutation behavior must be observed; optimistic writes require rollback. |
| W12-C03 | Request race handling | 12.md fetch/race sections; React Synchronizing with Effects, Fetching data | Ignore stale results after cleanup; abort by itself does not demonstrate protection of both success and error paths. |
| W12-C04 | Accessible asynchronous forms | 12.md form/exception sections; authored learner checklist | Keyboard, zoom, focus and status observations required; not accessibility certification. |
| W12-C05 | Measured optimization and debugging | 12.md profiler/debug/interview sections; authored learner checklist | No automatic memoization benefit; use a repeatable workload and preserved behavior. |

Selected primary access (actual body read; limited to listed sections):

- [React selected Fetching data section](https://react.dev/learn/synchronizing-with-effects): Actual section read: cleanup ignore guard; race caused by responses arriving out of order. This is selected-section review only.

The original chapter remains unchanged. This ledger adds learner evidence requirements and makes assumptions explicit; it does not silently correct or certify all original code snippets, bibliography or runtime outcomes. The manual activity, not a fabricated quiz or answer key, provides four positive one-point criteria. Original DOCX artifacts are untouched.
