# Stage 13 bounded source and evidence ledger

Status: draft. Complete original lesson read; bounded operational mapping and selected primary-source sections reviewed on 2026-10-04. Full references, actual learner execution and course approval remain open.

| Claim ID | Scope | Locator / evidence basis | Boundary |
|---|---|---|---|
| W13-C01 | Behavioral tests across boundaries | 13.md test pyramid/unit/report/CSV sections; authored checklist | Coverage and test names do not prove behavior; require explicit input and assertions. |
| W13-C02 | HTTP integration and isolated host | 13.md TestServer/HTTP sections; ASP.NET Core Integration tests | Host and database configuration must be recorded and isolation observed. |
| W13-C03 | Database strategy fidelity | 13.md database isolation sections; EF Core Choosing a testing strategy | Provider differences and fake/mocked query limits remain explicit; choose tests that prove the actual behavior. |
| W13-C04 | Component/E2E ownership and async behavior | 13.md component/E2E sections; authored checklist | Separate component assertions from browser observations; no live learner data. |
| W13-C05 | CI and red/green evidence | 13.md CI/debug/interview sections; authored checklist | Preserve genuine failed assertion and repaired execution; blanket retries do not establish correctness. |

Selected primary access (actual body read; limited to listed sections):

- [ASP.NET Core selected integration-test host/AAA sections](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0): Actual selected sections read on test host, environment and arrange/act/assert.
- [EF Core selected provider-strategy sections](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy): Actual selected sections read on SQLite/provider differences, InMemory transaction limits and mocked query behavior.

The original chapter remains unchanged. This ledger adds learner evidence requirements and makes assumptions explicit; it does not silently correct or certify all original code snippets, bibliography or runtime outcomes. The manual activity, not a fabricated quiz or answer key, provides four positive one-point criteria. Original DOCX artifacts are untouched.
