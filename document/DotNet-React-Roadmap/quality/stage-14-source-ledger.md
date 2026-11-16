# Stage 14 bounded source and evidence ledger

Status: draft. Complete original lesson read; bounded operational mapping and selected primary-source sections reviewed on 2026-10-04. Full references, actual learner execution and course approval remain open.

| Claim ID | Scope | Locator / evidence basis | Boundary |
|---|---|---|---|
| W14-C01 | Build artifact and secret boundaries | 14.md Docker/configuration sections; authored evidence checklist | COPY context must be scoped; frontend values public; do not disclose secrets or publish production. |
| W14-C02 | Pipeline and compatible migrations | 14.md CI/CD/migration sections; authored checklist | Expand/contract and old/new compatibility require rehearsal; code rollback is distinct from schema rollback. |
| W14-C03 | Liveness and readiness | 14.md health sections; ASP.NET Core Health checks selected sections | Configure the intended checks separately and observe a dependency outage; no internal data in response. |
| W14-C04 | Structured telemetry | 14.md logs/metrics/tracing sections; authored checklist | Redact sensitive data and require observed correlated outcomes; diagrams alone are not runtime evidence. |
| W14-C05 | Backup restore and recovery | 14.md backup/rollback/incident/final sections; authored safety checklist | Restore a separate synthetic-data copy only; compare counts/report/migration state and state loss assumptions. |

Selected primary access (actual body read; limited to listed sections):

- [ASP.NET Core selected separate readiness/liveness sections](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0): Actual selected sections read on separate endpoints and readiness/liveness selection; no application runtime observation performed.

The original chapter remains unchanged. This ledger adds learner evidence requirements and makes assumptions explicit; it does not silently correct or certify all original code snippets, bibliography or runtime outcomes. The manual activity, not a fabricated quiz or answer key, provides four positive one-point criteria. Original DOCX artifacts are untouched.
