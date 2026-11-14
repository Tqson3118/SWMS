# E2E Test Infra: SWMS UI/UX & Production Ergonomics

## Test Philosophy
- **Opaque-box & Requirement-driven**: Derived strictly from user requirements (R1–R5) and official user design decisions (Bento Grid Architecture). No dependency on internal implementation mocks.
- **Strictly Unmocked**: Executes against real .NET 10 API backend (`http://127.0.0.1:5080`) and Next.js frontend (`http://127.0.0.1:3000`) on real Chromium without route mocking.
- **Methodology**: Systematic 4-tier coverage (Category-Partition, Boundary Value Analysis, Pairwise Combinations, Real-World Workloads).

## Feature Inventory & Test Mapping
| # | Feature | Source | Tier 1 (Coverage) | Tier 2 (Boundary) | Tier 3 (Cross-Feature) | Tier 4 (Workloads) |
|---|---------|--------|:-----------------:|:-----------------:|:----------------------:|:------------------:|
| R1 | Design System, Navigation & Theme | ORIGINAL_REQUEST §R1 | 5 tests | 5 tests | ✓ | ✓ |
| R2 | Bento Dashboard & Ingestion | ORIGINAL_REQUEST §R2 | 5 tests | 5 tests | ✓ | ✓ |
| R3 | Dual-Layer Reader & Split Workspace | ORIGINAL_REQUEST §R3 | 5 tests | 5 tests | ✓ | ✓ |
| R4 | Active Recall Review & Spaced Repetition | ORIGINAL_REQUEST §R4 | 5 tests | 5 tests | ✓ | ✓ |
| **Total** | | | **20 tests** | **20 tests** | **5 tests** | **4 tests** |

**Grand Total: 49 E2E Test Cases in Master Suite** (`frontend/e2e/r5-visual-interactive-e2e.spec.ts`).

## Test Architecture
- **Master Test Suite**: `frontend/e2e/r5-visual-interactive-e2e.spec.ts`
- **Zero-Error Listener Fixture**: `frontend/e2e/fixtures/test-base.ts`
  - Captures and asserts zero `page.on("pageerror")`, zero `page.on("console", msg.type() === "error")`, zero unhandled Promise rejections, and zero React hydration mismatches.
- **Ergonomics & WCAG Helpers**: `frontend/e2e/fixtures/ergonomics-helpers.ts`
  - `assertZeroHorizontalOverflow(page)`: confirms `scrollWidth <= innerWidth`.
  - `assertTouchTargetSize(locator, minWidth=44, minHeight=44)`: confirms touch targets meet WCAG 2.5.5 / 2.5.8.
  - `assertColorContrast(locator, minRatio=4.5)`: evaluates text contrast against computed background.
- **Viewports Tested**:
  - Desktop: `1440x900`
  - Tablet Portrait: `768x1024`
  - Tablet Landscape: `1024x768`
- **Runner Command**: `pwsh -ExecutionPolicy Bypass -File scripts/verify.ps1`

## Coverage Thresholds
- Tier 1: ≥5 per feature (Total 20)
- Tier 2: ≥5 per feature (Total 20)
- Tier 3: Pairwise cross-feature interaction scenarios (Total 5)
- Tier 4: Real-world application scenarios (Total 4)
- **Minimum required: 49 tests passing with 0 console errors on real Chromium**.
