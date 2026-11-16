# Stage 11 source ledger — draft, preliminary

This ledger maps the one manual activity to `11.md`, with Stage 10 as the
explicit prerequisite. It records source-scoped learner evidence only; no
identity, API, browser, database, package, security, build or learner execution
is asserted.

| Claim ID | Source locator | Scoped claim / boundary | Observable evidence and positive rubric | Review state |
|---|---|---|---|---|
| W11-C01 | `11.md`, Tuần 1–2; ASP.NET Core authentication overview; authorization introduction; resource-based authorization | Authentication establishes identity/claims; authorization combines a general policy with a resource/owner decision. Every Expense read/write must be owner-scoped, including category/resource checks. | Learner records chosen cookie/bearer flow, A/B/anonymous/missing-claim matrix, client OwnerUserId rejection, foreign/unknown behavior and consistent 401/403/404 results. | draft; preliminary; bounded mapping reviewed by root226; full approval open |
| W11-C02 | `11.md`, Tuần 2 Buổi 4; Tuần 3 Buổi 5; ASP.NET Core XSS guidance; authentication/authorization docs | DTO/input/resource/database validation, secret boundaries, CSRF treatment, exact CORS origins and safe output encoding have different threat/control responsibilities. CORS is not authorization. | Learner records invalid amount/date/category/foreign category/oversized/script outcomes, safe response/log contents, secret exclusion, valid/invalid origin and preflight, cookie anti-forgery or bearer threat control. Browser/API blockers remain incomplete. | draft; preliminary; bounded mapping reviewed by root226; full approval open |
| W11-C03 | `11.md`, Tuần 3 Buổi 6; ASP.NET Core rate limiting middleware | Rate limiting is endpoint/resource protection and does not replace authentication or authorization. | Learner records the selected endpoint policy, limit boundary and actual 429/retry response; unavailable execution is an explicit blocker. | draft; preliminary; bounded mapping reviewed by root226; full approval open |
| W11-C04 | `11.md`, Tuần 3 Buổi 6; project; ASP.NET Core authorization and rate-limit docs | Idempotency requires owner-scoped key, request fingerprint, result/status and expiry/cleanup; same payload replays the prior result, different payload conflicts, and concurrent first requests need unique/atomic persistence. | Learner records response-loss retry, same/different payload, cross-owner reuse, expiry, and a concurrent race with exactly one committed write/transaction. Check-then-insert prose is insufficient. | draft; preliminary; bounded mapping reviewed by root226; full approval open |
| W11-C05 | `11.md`, project/checkpoint/interview/debug; ASP.NET Core XSS/CORS/authentication docs | Threat-model and timed authorization regression evidence must distinguish cookie/bearer risks, CORS/direct clients, 401/403/404, OwnerUserId trust, DTOs, 429 and Idempotency-Key. | Learner records the 40-minute User B ID-change reproduction, minimal owner-filter repair and regression result with actual requests/tests and exact blockers. | draft; preliminary; bounded mapping reviewed by root226; full approval open |

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

- [ASP.NET Core authentication overview](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-10.0): scheme and claims boundary.
- [Introduction to authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-10.0): authentication/authorization distinction and policy boundary.
- [Resource-based authorization](https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/resource-based?view=aspnetcore-10.0): resource-specific checks after the resource is identified.
- [ASP.NET Core rate limiting middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0): endpoint policy and limit behavior boundary.
- [Prevent Cross-Site Scripting (XSS)](https://learn.microsoft.com/en-us/aspnet/core/security/cross-site-scripting?view=aspnetcore-10.0): untrusted output and encoding threat boundary.

These bodies support the scoped teaching claims only. Access did not verify this
course project, identity provider, API middleware ordering, browser behavior,
rate-limit runtime, database race or learner result. No source metadata or
search snippet is treated as execution evidence.
