# Project: SWMS UI/UX, Micro-interactions, Responsive Ergonomics & Production E2E

## Architecture
- **Design System & Theme Engine**:
  - Centralized typography, spacing, border radiuses (`rounded-2xl`, `rounded-xl`), and micro-interaction timing tokens in `frontend/src/app/globals.css`.
  - 3-tier elevation surface tokens in Dark/Light modes: `--surface-canvas` (`#020617` / `#F8FAFC`), `--surface-panel` (`#0F172A` / `#FFFFFF`), `--surface-card` (`#1E293B` / `#F1F5F9`), `--surface-elevated` (`#334155` / `#E2E8F0`).
  - Unified theme synchronizer (`ThemeSynchronizer.tsx`) and TopNav header (`TopNavHeader.tsx`) eliminating duplicate DOM class toggling, preventing FOUC, and enforcing WCAG AA contrast ratio (>4.5:1).
- **Personal Workspace Dashboard (Modern Bento Grid Architecture)**:
  - 4 core Bento Grid blocks in `PersonalWorkspaceDashboard.tsx`:
    1. `hero-continue-learning-card`: Hero Continue Learning Card with active document progress, page indicator, and 1-click CTA into the reader.
    2. `review-station-card`: Review Station Bento Card with bold counter of cards due today, estimated review time, and 1-click "Bắt đầu ôn tập" CTA.
    3. `quick-stats-tile`: Quick Stats & study streak tile without distracting verbose text.
    4. `document-library-grid`: Flat, clean listing of uploaded PDFs and notes with status icons/tags, eliminating redundant onboarding banners and technical jargon.
  - Document Ingestion: Drag-and-drop state, visual multi-stage upload progress indicator, and friendly Vietnamese validation error modals.
- **Dual-Layer Reader & Split Workspace**:
  - `ReaderAndSplitViews.tsx`: Responsive PDF canvas scaling, smooth page navigation toolbar ("Trang X / Y", prev/next, direct page jump input with clamping), 5 semantic color annotation popover, and missing text layer badge ("Không có lớp văn bản — chưa OCR").
  - Responsive collapsible sidebar for viewports < 1536px (1440px desktop and 768px/1024px tablet).
  - Two-phase async Jump to Source: navigate to target page -> await DOM mount -> scroll and trigger GSAP pulse highlight.
  - Removal of internal debug buttons (`simulate-409-conflict-btn`).
- **Active Recall Review & Spaced Repetition (FSRS / Practice)**:
  - `ReviewAndPracticeView.tsx`: Distraction-free review canvas.
  - Smooth 3D card flip animation using clean CSS 3D transforms (`perspective: 1200px`, `transform-style: preserve-3d`, `rotateY(180deg)`) without `opacity` or `overflow-y` conflicts.
  - FSRS rating buttons (Again, Hard, Good, Easy) with explicit shortcut indicators (1-4, Space), hidden on front face and revealed only after card flip.
  - Real-time review queue progress counter and completion screen.
- **Automated Verification & E2E Testing**:
  - Master suite: `frontend/e2e/r5-visual-interactive-e2e.spec.ts` executed via `scripts/verify.ps1` against real application without mocks.
  - Viewports: Desktop (1440x900) and Tablet (768x1024 / 1024x768).
  - Global event listener capturing and asserting zero console errors, zero unhandled Promise rejections, and zero React hydration mismatches.
  - WCAG AA contrast (>4.5:1) and touch/click target size checks (>=44x44px).

## Code Layout
- `frontend/src/app/globals.css`: Design tokens, 3-tier elevation surfaces, 3D transform utilities, focus rings.
- `frontend/src/app/layout.tsx`: Root layout, theme script, fonts.
- `frontend/src/components/layout/ThemeSynchronizer.tsx`: Single source of truth for `.dark` class sync and storage persistence.
- `frontend/src/components/layout/TopNavHeader.tsx`: Navigation bar, mode tabs, theme toggle button, quota popover.
- `frontend/src/components/dashboard/PersonalWorkspaceDashboard.tsx`: Modern Bento Grid Dashboard, upload modal, Vietnamese validation messages.
- `frontend/src/components/reader/ReaderAndSplitViews.tsx`: Dual-layer reader, responsive split panes, page toolbar, 5-color palette, jump-to-source.
- `frontend/src/components/review/ReviewAndPracticeView.tsx`: FSRS 3D flip card, rating bar, review queue, formative quiz.
- `frontend/e2e/r5-visual-interactive-e2e.spec.ts`: Master R5 Playwright acceptance test suite (Tiers 1-4).
- `frontend/e2e/fixtures/test-base.ts`: Playwright zero-error listener fixture & viewport configurations.
- `scripts/verify.ps1`: Automated build, test, and E2E verification runner.

## Feature Inventory
| # | Feature | Description | Milestone | Source |
|---|---------|-------------|-----------|--------|
| 1 | Global Typography & Design Tokens | Unify font family, scale, border-radius tokens (rounded-2xl) and micro-interactions | M1 | R1 |
| 2 | 3-Tier Elevation Surface Tokens | Define --surface-canvas, --surface-panel, --surface-card in Light and Dark modes | M1 | R1 / Survey |
| 3 | Seamless Theme Switching & FOUC Prevention | Deduplicate DOM class toggle in TopNavHeader vs ThemeSynchronizer; guarantee smooth toggle | M1 | R1 |
| 4 | WCAG AA Color Contrast Ratio | Guarantee contrast >4.5:1 for body text and >3:1 for large text/icons in Light & Dark modes | M1 | R1 |
| 5 | TopNavHeader & Mode Switcher Polish | Refine mode tabs (0-3), search trigger, quota indicator, shortcuts modal trigger (?) | M1 | R1 |
| 6 | Bento Grid Hero Continue Learning Card | Hero card displaying active material, progress indicator, and 1-click jump into Reader | M2 | R2 / User Decision |
| 7 | Bento Grid Review Station Card | Bento card with bold due cards count, estimated study time, and 1-click review CTA | M2 | R2 / User Decision |
| 8 | Bento Grid Quick Stats Tile | Compact streak counter and daily goal tile without verbose explanatory clutter | M2 | R2 / User Decision |
| 9 | Bento Grid Document & Resource Library | Flat, clean list/grid of materials with status tags; eliminate onboarding card clutter | M2 | R2 / User Decision |
| 10 | Document Ingestion UI/UX Polish | Smooth drag-and-drop, multi-step progress indicator, friendly Vietnamese error modals | M2 | R2 |
| 11 | Responsive Bento Grid Reflow | Graceful reflow across Desktop (1440x900) and Tablet (768x1024) viewports | M2 | R2 |
| 12 | Dual-Layer Reader Controls | Responsive canvas scaling, page toolbar ("Trang X / Y", prev/next, clamped jump input) | M3 | R3 |
| 13 | 5 Semantic Color Annotation Toolbar | Floating selection toolbar with 5 semantic colors, instant anchoring, and text layer badge | M3 | R3 |
| 14 | Responsive Split Workspace Ergonomics | Collapsible sidebar under 1536px, smooth splitter resizing with double-click reset to 15/45/40 | M3 | R3 |
| 15 | Two-Phase Async Jump-to-Source | Switch active document/page -> wait for DOM mount -> scroll and pulse highlight; hide debug button | M3 | R3 |
| 16 | Distraction-Free Review Canvas | Clean review layout focusing exclusively on active flashcard, queue counter, and progress | M4 | R4 |
| 17 | Glitch-Free 3D Card Flip Animation | Pure CSS 3D transform (perspective: 1200px, preserve-3d) eliminating opacity/overflow flattening | M4 | R4 |
| 18 | Gated FSRS Rating Bar & Hotkeys | Hide rating buttons on front face; reveal 4 buttons with interval badges on back face (1-4, Space) | M4 | R4 |
| 19 | Review Queue Progress & Quiz Rubric | Dynamic queue counter, completion celebration screen, formative quiz with AI rubric feedback | M4 | R4 |
| 20 | Master Playwright Acceptance Suite | Comprehensive unmocked E2E suite covering Tiers 1-4 across all features | M5 / Test Track | R5 |
| 21 | Zero Console & Hydration Error Listener | Global Playwright fixture failing on any console.error, unhandled rejection, or hydration error | M5 / Test Track | R5 |
| 22 | Multi-Viewport Responsive Verification | Automated verification at Desktop 1440x900 and Tablet 768x1024 / 1024x768 with zero overflow | M5 / Test Track | R5 |
| 23 | Touch Target Size Verification | Automated bounding box assertion (>=44x44px) for all interactive controls on tablet | M5 / Test Track | R5 |
| 24 | Theme Switching & Contrast E2E Check | Automated E2E verification of theme toggles, local persistence, and contrast ratios | M5 / Test Track | R5 |
| 25 | Verification Runner 2x Clean Pass | Integration into scripts/verify.ps1 with 2 consecutive exit code 0 runs | M5 / Test Track | R5 |

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | Design System, Typography & Theme Engine | globals.css tokens, 3-tier elevation surfaces, ThemeSynchronizer & TopNavHeader | none | DONE |
| M2 | Bento Grid Dashboard & Document Ingestion | Modern Bento Grid (Hero, Review Station, Stats, Library), upload progress, VN modals | M1 | DONE |
| M3 | Dual-Layer Reader & Split Workspace Ergonomics | Canvas scaling, page toolbar, 5-color annotation, sidebar collapse, jump-to-source fix | M1, M2 | IN_PROGRESS |
| M4 | Active Recall Review & Spaced Repetition (FSRS) | Distraction-free review, 3D flip card fix, gated rating buttons (1-4, Space), quiz | M1 | PLANNED |
| M5 | Final Integration, Regression Guard & Adversarial Verification | Pass 100% of all E2E tests, multi-viewport verification, zero console errors, auditor clean | M1, M2, M3, M4, Test Track | PLANNED |

## Interface Contracts
### Workspace Store & State Contracts
- `useWorkspaceStore`:
  - `activeMode`: `'dashboard' (0) | 'reading' (1) | 'split' (2) | 'review' (3)`
  - `darkMode`: `boolean` (persisted in `localStorage` under `swms_theme` as `'dark' | 'light'`)
  - `activeDocumentId`: `string | null`
  - `currentDocumentPage`: `number` (1-indexed)
  - `splitRatios`: `[number, number, number]` (default `[15, 45, 40]`)
  - `jumpToSource(documentId: string, pageNumber: number, annotationId: string)`: Asynchronous two-phase navigation.

### Bento Grid Test IDs Contract
- `hero-continue-learning-card`: Hero card containing active material title, progress bar, and reader CTA.
- `review-station-card`: FSRS review station containing due count badge and start review CTA.
- `quick-stats-tile`: Quick statistics tile containing streak and retention indicators.
- `document-library-grid`: Document listing container.
- `file-upload-input`: File input for binary upload.
