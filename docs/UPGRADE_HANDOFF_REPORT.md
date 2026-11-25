# Báo Cáo Nâng Cấp & Bàn Giao Hệ Thống Studxy

> **Ngày hoàn tất:** 25/11/2026 (GMT+7)  
> **Thư mục làm việc:** `d:\03_Study\Studxy`  
> **Trạng thái nghiệm thu:** Đã hoàn tất nâng cấp UI/UX toàn app, dọn sạch CSDL local có backup xác thực, kiểm thử trực quan trình duyệt thật (18 ảnh Desktop/Tablet/Mobile + Light/Dark), sửa triệt để mọi lỗi phát sinh (Zero-Facade), và pass **2 lần liên tiếp** toàn bộ `scripts/verify.ps1` + `e2e/document-courses-e2e.spec.ts`.

---

## 1. Tổng Quan Hạng Mục Đã Nâng Cấp & Sửa Lỗi

### A. Hệ thống Thiết kế (Design System), Shell & Dashboard (Wave 1 + Wave 2)
- **Visual Design System & Micro-interactions:** Chuẩn hóa utility CSS (`.no-scrollbar`, `.text-accent-contrast`, `.swms-skeleton`, `.swms-animate-*`), nâng cấp tương phản Light/Dark trên các component lõi [Button.tsx](file:///d:/03_Study/Studxy/frontend/src/components/ui/Button.tsx), [Card.tsx](file:///d:/03_Study/Studxy/frontend/src/components/ui/Card.tsx), [Dialog.tsx](file:///d:/03_Study/Studxy/frontend/src/components/ui/Dialog.tsx) (kèm focus trap), [EmptyState.tsx](file:///d:/03_Study/Studxy/frontend/src/components/ui/EmptyState.tsx), [Badge.tsx](file:///d:/03_Study/Studxy/frontend/src/components/ui/Badge.tsx) và [globals.css](file:///d:/03_Study/Studxy/frontend/src/app/globals.css).
- **Điều hướng Desktop & Mobile:**
  - [Sidebar.tsx](file:///d:/03_Study/Studxy/frontend/src/components/navigation/Sidebar.tsx): Khắc phục popover hồ sơ khi thu gọn sidebar, bổ sung mini progress bar cho danh sách khóa học gần đây.
  - [MobileNav.tsx](file:///d:/03_Study/Studxy/frontend/src/components/navigation/MobileNav.tsx) & [layout.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/layout.tsx): Bổ sung `MobileTopBar` (`md:hidden`, `data-testid="mobile-top-bar"`) với nút tìm kiếm nhanh, chuyển sáng/tối và menu mở rộng, giữ nguyên thanh điều hướng đáy `mobile-bottom-nav`.
- **Today Dashboard ([page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/page.tsx)):** Hiển thị mục tiêu bài học/ngày (`dailyLessonGoal`), dự báo ngày hoàn thành, sửa tương phản nút, và nâng cấp Skeleton/Focus Cards.
- **Landing V2 & Command Palette:** Đồng bộ token màu, `overflow-x-hidden` trên [LandingPageV2.tsx](file:///d:/03_Study/Studxy/frontend/src/components/landing/LandingPageV2.tsx) và auto-scroll item đang chọn trên [CommandPaletteModal.tsx](file:///d:/03_Study/Studxy/frontend/src/components/command/CommandPaletteModal.tsx).

### B. Catalog, Course Hub 4 Tab & Studio Sáng Tạo
- **Catalog (`/app/courses` — [page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx)):** Chuẩn hóa chiều cao thẻ Grid, đồng bộ `data-course-slug` ở cả Grid và List, thêm menu 3 chấm WAI-ARIA (`role="menu"` / `role="menuitem"`) đóng khi click ngoài/Esc, toast `role="status"`, bộ đếm bộ lọc, cùng các component [CourseQuickPreviewDrawer.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseQuickPreviewDrawer.tsx) (mobile full-width, scroll lock, focus trap), [CourseDuplicateCleanupModal.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseDuplicateCleanupModal.tsx), và [CourseCardGraphicBanner.tsx](file:///d:/03_Study/Studxy/frontend/src/components/dashboard/CourseCardGraphicBanner.tsx).
- **Course Hub (`/app/courses/[id]` — [page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/page.tsx)):**
  - Khôi phục liên kết `?lesson=` cho `course-continue-learn-btn`, `data-testid="course-quiz-btn"` và nút `"Biên tập khóa học"` cho khóa cá nhân; đồng bộ 4 `role="tab"` hỗ trợ phím `ArrowLeft`/`ArrowRight`/`Home`/`End`, tích hợp `CourseInSearchModal` (`Ctrl+Shift+F`).
  - [CourseSettingsTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseSettingsTab.tsx): Bổ sung 4 preset chips (`1 / 2 / 3 / 5 bài/ngày`), thẻ dự báo hoàn thành động (`~N ngày`) và nút `"Áp dụng ngày dự kiến"`.
  - Kiểm tra khóa đã rút gọn (`/courses/{id}`) trước khi gọi `/resume-state` & `/reviews`, hiển thị mượt mà Retired Redirect Banner.
  - Tinh chỉnh [CoursePracticeTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CoursePracticeTab.tsx), [CourseAssetsTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseAssetsTab.tsx), [CourseSyllabusTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseSyllabusTab.tsx).
- **Creation Wizard & Studio Editor:**
  - [new/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/new/page.tsx): Stepper 3 bước trực quan, banner cảnh báo trùng khóa có link bấm thẳng, hỗ trợ Drag & Drop.
  - [edit/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/edit/page.tsx): Modal xóa khóa học 2 bước an toàn (`Tôi hiểu rủi ro, tiếp tục` → `confirm-delete-course-btn`), tab xem trước Markdown & nút sao chép trong Trợ lý AI bài giảng (`Mở rộng bài học`), phím tắt `Ctrl+S` và cảnh báo thay đổi chưa lưu.

### C. Learn Workspace, Practice/Exams/Review & Library
- **Learn Workspace:**
  - [CourseInSearchModal.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseInSearchModal.tsx): Điều hướng bàn phím `ArrowDown`/`ArrowUp`/`Home`/`End`/`Enter`/`Escape`, `role="listbox"`/`"option"`, `AbortController`, highlight từ khóa tiếng Việt không dấu chính xác (`buildNormalizedTextWithMap`), lưu lịch sử 5 tìm kiếm gần đây trong `sessionStorage`.
  - [CourseLearningWorkspace.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx), [CourseRoadmapSidebar.tsx](file:///d:/03_Study/Studxy/frontend/src/components/reader/CourseRoadmapSidebar.tsx), [LearningStickyBottomBar.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/workspace/LearningStickyBottomBar.tsx): Reading Progress Hairline `2px`, bộ đếm badge filter, auto-scroll bài học đang mở, phân tách `testId="course-roadmap-sidebar-mobile"` cho ngăn kéo mobile để tránh trùng DOM `data-testid="course-roadmap-sidebar"` trên desktop.
- **Practice, Exams, FSRS Review & Workspace phụ trợ:** Hoàn thiện skeleton, dialogue scroll lock, FSRS 3D card & bảng phím tắt, Drag & Drop PDF trên [practice/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/practice/page.tsx), [exams/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/exams/page.tsx), [review/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/review/page.tsx), [documents/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/documents/page.tsx), [notes/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/notes/page.tsx), [settings/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/settings/page.tsx).

### D. Bug Thật Phát Hiện & Sửa Triệt Để Trong Kiểm Chứng (Zero-Facade)
1. **[cleanup-local-dev-db.ps1](file:///d:/03_Study/Studxy/scripts/cleanup-local-dev-db.ps1):** Sửa câu lệnh `DELETE FROM practice_quizzes WHERE course_id IN ...` vốn lỗi `no such column: course_id` bằng kiểm tra schema `PRAGMA table_info(practice_quizzes)` và bổ sung dọn tham chiếu `document_id` (`practice_quizzes`, `annotations`, `flashcards`, `notebooks`, `notebook_links`) trước khi xóa `documents`.
2. **[math-markdown.tsx](file:///d:/03_Study/Studxy/frontend/src/lib/math-markdown.tsx#L38-L46):** Đổi thẻ bao `BlockMath` từ `<div>` sang `<span className="katex-display block ...">` để không vi phạm lồng `<div>` bên trong `<p>`, dứt điểm 7 lỗi React DOM Console Error trong suite `r5-visual-interactive-e2e.spec.ts`.
3. **[SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L4729-L4835) (`/api/v1/lessons/by-path`):** Sửa lỗi 404 khi bài học ở thư mục con (ví dụ `IELTS/Phase-1/listening/cycle11-listening.md`) tham chiếu tới tài liệu cùng phase dạng tương đối (`speaking/speaking-prompts-month2.md`). Endpoint hiện dò ngược theo các thư mục cha của `currentLessonRelPath` (`Phase-1/speaking/...`) sau khi giữ nguyên mọi kiểm tra chống path traversal.
4. **Cập nhật E2E Specs (Zero-Facade, 0 mock `page.route`):** Đồng bộ [course-lifecycle-upgrade.spec.ts](file:///d:/03_Study/Studxy/frontend/e2e/course-lifecycle-upgrade.spec.ts), [reader-polish-and-roadmap.spec.ts](file:///d:/03_Study/Studxy/frontend/e2e/reader-polish-and-roadmap.spec.ts), [v2-topdown-sitemap.spec.ts](file:///d:/03_Study/Studxy/frontend/e2e/v2-topdown-sitemap.spec.ts), và [document-courses-e2e.spec.ts](file:///d:/03_Study/Studxy/frontend/e2e/document-courses-e2e.spec.ts) khớp chính xác hành vi thực tế (`role="menuitem"`, theo dõi chặt chẽ chỉ cho phép 404 đúng ID khóa retired `22222222-2222-4222-8222-222222222225`, CTA Landing V2 → `/auth?tab=register`, bài `W01` Investing).

---

## 2. Bảng Phân Công Đội Ngũ 5 Nhân Viên CLI

| Đợt | Nhân viên & Model | Phạm vi file phụ trách | Trạng thái |
|---|---|---|---|
| **Wave 1 (W1-A)** | `agy` (`gemini-4-hydrogen-high`) | [globals.css](file:///d:/03_Study/Studxy/frontend/src/app/globals.css), UI primitives (`Button`, `Card`, `Dialog`, `EmptyState`, `Badge`), [Sidebar.tsx](file:///d:/03_Study/Studxy/frontend/src/components/navigation/Sidebar.tsx), [MobileNav.tsx](file:///d:/03_Study/Studxy/frontend/src/components/navigation/MobileNav.tsx), [app/app/layout.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/layout.tsx), [app/app/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/page.tsx) | Exit `0` |
| **Wave 1 (W1-B)** | `agy2` (`gemini-4-hydrogen-high`) | [app/app/courses/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx), [CourseQuickPreviewDrawer.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseQuickPreviewDrawer.tsx), [CourseDuplicateCleanupModal.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseDuplicateCleanupModal.tsx), [CourseCardGraphicBanner.tsx](file:///d:/03_Study/Studxy/frontend/src/components/dashboard/CourseCardGraphicBanner.tsx) | Exit `0` |
| **Wave 1 (W1-C)** | `agy3` (`gemini-4-hydrogen-high`) | [app/app/courses/[id]/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/page.tsx), [CourseSettingsTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseSettingsTab.tsx), [CoursePracticeTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CoursePracticeTab.tsx), [CourseAssetsTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseAssetsTab.tsx), [CourseSyllabusTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseSyllabusTab.tsx) | Exit `0` |
| **Wave 1 (W1-D)** | `agy4` (`gemini-4-hydrogen-medium`) | [app/app/courses/new/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/new/page.tsx), [app/app/courses/[id]/edit/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/edit/page.tsx) | Exit `0` |
| **Wave 2 (W2-A)** | `agy` (`gemini-4-hydrogen-high`) | [CourseInSearchModal.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseInSearchModal.tsx), [CourseLearningWorkspace.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx), [CourseRoadmapSidebar.tsx](file:///d:/03_Study/Studxy/frontend/src/components/reader/CourseRoadmapSidebar.tsx), [LearningStickyBottomBar.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/workspace/LearningStickyBottomBar.tsx) | Exit `0` |
| **Wave 2 (W2-B)** | `agy2` (`gemini-4-hydrogen-medium`) | [app/app/practice/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/practice/page.tsx), [app/app/exams/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/exams/page.tsx), [app/app/review/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/review/page.tsx) | Exit `0` |
| **Wave 2 (W2-C)** | `agy3` (`gemini-4-hydrogen-medium`) | [app/app/documents/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/documents/page.tsx), [app/app/notes/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/notes/page.tsx), [app/app/settings/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/settings/page.tsx), [CommandPaletteModal.tsx](file:///d:/03_Study/Studxy/frontend/src/components/command/CommandPaletteModal.tsx), [LandingPageV2.tsx](file:///d:/03_Study/Studxy/frontend/src/components/landing/LandingPageV2.tsx) | Exit `0` |
| **Wave 2 (W2-D)** | `agy6` (`gemini-4-hydrogen-medium`) | [course-lifecycle-upgrade.spec.ts](file:///d:/03_Study/Studxy/frontend/e2e/course-lifecycle-upgrade.spec.ts), [reader-polish-and-roadmap.spec.ts](file:///d:/03_Study/Studxy/frontend/e2e/reader-polish-and-roadmap.spec.ts), [v2-topdown-sitemap.spec.ts](file:///d:/03_Study/Studxy/frontend/e2e/v2-topdown-sitemap.spec.ts) | Exit `0` |

---

## 3. Bằng Chứng Kiểm Chứng Thực Tế (Sau Lần Chỉnh Sửa Cuối Cùng)

| Hạng mục | Lệnh thực thi chính xác | Kết quả / Exit Code | Log / Thư mục bằng chứng |
|---|---|---|---|
| **Backend Release Build & Unit/Integration Tests** | `dotnet build backend\Swms.Api.sln -c Release` & `dotnet test ... --no-build` (và chạy lại trong cả 3 lượt `verify.ps1`) | **Exit `0`** — `0 Error(s)`, **124 / 124 Passed** (`0 Failed`, `0 Skipped`) | `scratch\backend-build.log`, `scratch\backend-test.log`, `test-results\verification\20261125-051503\backend-tests.log` |
| **Frontend Production Build** | `npm --prefix d:\03_Study\Studxy\frontend run build` (sau sửa `math-markdown.tsx`, `CourseRoadmapSidebar.tsx`, `CourseLearningWorkspace.tsx`, `[id]/page.tsx`) | **Exit `0`** — 25/25 routes compiled & typechecked cleanly | `C:\Users\Administrator\.gemini\antigravity\brain\b445ade5-64b4-4f59-b865-c218ca5e2978\.system_generated\tasks\task-421.log` |
| **Dọn DB Local + Xác thực Backup** | `powershell -ExecutionPolicy Bypass -File d:\03_Study\Studxy\scripts\cleanup-local-dev-db.ps1 -DryRun` → thực thi `cleanup-local-dev-db.ps1` | **Exit `0`** — Backup `swms_local_dev.db.bak.20261125-035102` (`206,118,912` bytes) khớp kích thước (cùng bản `20261125-035021`); dọn `838` khóa test/trùng/retired (`398` chapters, `691` lessons, `215` enrollments); còn `85` khóa bảo vệ (`10/10` khóa chuẩn + `son.tran@studxy.vn`); `PRAGMA quick_check = ok` | `scratch\db-cleanup-dryrun.log`, `scratch\db-cleanup-exec.log` |
| **Live Visual Browser Audit (0 mock)** | `node ...\scratch\live-visual-audit.mjs` trên Backend Release `:5080` + `next start :3000` | **Exit `0`** (`LIVE_AUDIT_EXIT=0`) — `0` lỗi page, `0` HTTP 5xx, `0` HTTP 404 ngoài dự kiến (`expectedRetired404Count=3` chỉ của ID retired), `0` lỗi overflow ngang trên Desktop `1440x900`, Tablet `768x1024`, Mobile `375x812` & Light/Dark; đã kiểm tra trực quan toàn bộ 18 ảnh chụp | `scratch\live-visual-audit.log`, `scratch\audit-shots\01..18_*.png` |
| **`scripts/verify.ps1` Full Run #1** | `powershell -ExecutionPolicy Bypass -File d:\03_Study\Studxy\scripts\verify.ps1` | **Exit `0`** (`822.85s`) — **124/124** Backend tests, **6/6** Multi-User Benchmark (`377 reqs`), **140/140** Playwright E2E tests (14/14 suite) | `D:\03_Study\Studxy\test-results\verification\20261125-045232`, `scratch\verify-run1.log` |
| **Suite bổ sung `document-courses-e2e.spec.ts`** | `powershell -ExecutionPolicy Bypass -File d:\03_Study\Studxy\scripts\verify.ps1 -Suites "e2e/document-courses-e2e.spec.ts"` | **Exit `0`** (`473.37s`) — **124/124** Backend tests, **6/6** Benchmark, **4/4** `document-courses-e2e` tests (gồm `/api/v1/lessons/by-path` + audio `.wav` HTTP 206 + `W01` Quiz & FSRS) | `D:\03_Study\Studxy\test-results\verification\20261125-050644`, `scratch\verify-doc-courses.log` |
| **`scripts/verify.ps1` Full Run #2 (liên tiếp)** | `powershell -ExecutionPolicy Bypass -File d:\03_Study\Studxy\scripts\verify.ps1` | **Exit `0`** (`812.39s`) — **124/124** Backend tests, **6/6** Benchmark, **140/140** Playwright E2E tests (14/14 suite), giải phóng sạch PID & cổng `5080`/`3000` | `D:\03_Study\Studxy\test-results\verification\20261125-051503`, `scratch\verify-run2.log` |

---

## 4. Mục Chưa Kiểm Chứng

- **Git Commit / Push:** Chưa thực hiện `git add`, `git commit`, hay `git push` — toàn bộ thay đổi hiện đang nằm sạch trên working tree để bạn tự rà soát trước khi commit.
- **PostgreSQL Production thực tế:** Toàn bộ kiểm chứng DB và E2E chạy trên SQLite (`swms_local_dev.db`, `swms_verify.db` theo cơ chế của `verify.ps1`); chưa chạy đối chiếu trực tiếp trên PostgreSQL cloud/production.
- **Google OAuth & AI Proxy live từ nhà cung cấp thật:** Chưa thực hiện luồng đăng nhập Google OAuth live với client secret thật và chưa gọi API AI bên ngoài có tính phí (backend test sử dụng `--AiProxy:BaseUrl=http://127.0.0.1:18045` với fallback cục bộ).
- **Build `.next` local rewrite note:** Thư mục `.next/` hiện tại đã được build với `BACKEND_INTERNAL_URL=http://127.0.0.1:5080` để chạy `next start` test cục bộ (`.next/` là thư mục build tạm, không nằm trong git; khi deploy Vercel sẽ dùng `.env` / biến môi trường Vercel để build lại).

---

## 5. Hướng Dẫn Nghiệm Thu Nhanh (5 Phút Khi Mở Lại Máy)

1. **Rà soát thay đổi Git & Backup DB:**
   ```powershell
   Set-Location d:\03_Study\Studxy
   git status --short
   Get-ChildItem swms_local_dev.db* | Select-Object Name, Length, LastWriteTime
   ```
   *(Lưu ý: hai bản backup `swms_local_dev.db.bak.20261125-035021` và `swms_local_dev.db.bak.20261125-035102` được giữ nguyên an toàn; có thể tự xóa sau khi đã hoàn toàn yên tâm.)*

2. **Chạy app thật ở chế độ local dev/production:**
   - Terminal 1 (Backend):
     ```powershell
     $env:SWMS_ALLOW_TEST_USER_HEADER = "true"
     dotnet run --project d:\03_Study\Studxy\backend\src\Swms.Api\Swms.Api.csproj --urls http://127.0.0.1:5080
     ```
   - Terminal 2 (Frontend):
     ```powershell
     $env:NEXT_PUBLIC_API_BASE_URL = "http://127.0.0.1:5080"
     $env:BACKEND_INTERNAL_URL = "http://127.0.0.1:5080"
     npm --prefix d:\03_Study\Studxy\frontend run dev
     ```

3. **Kiểm tra nhanh 6 điểm vàng trên trình duyệt (`http://127.0.0.1:3000`):**
   - Landing `/` → chuyển Light/Dark, bấm `"Bắt đầu miễn phí"` sang `/auth?tab=register` và đăng ký/đăng nhập.
   - Today Dashboard `/app` → kiểm tra thẻ `"Tiếp tục lộ trình"` kèm mục tiêu bài/ngày & dự báo ngày hoàn thành.
   - Catalog `/app/courses` → kiểm tra đủ 10 khóa chuẩn, mở Quick Preview Drawer, thử menu 3 chấm Ghim/Bỏ ghim.
   - Course Hub `/app/courses/<id>` → thử 4 tab, tab Cài đặt với preset chips `1 / 2 / 3 / 5 bài/ngày`, và thử URL khóa retired `/app/courses/22222222-2222-4222-8222-222222222225`.
   - Learn Workspace `/app/courses/<id>/learn` → thử `Ctrl+Shift+F` điều hướng `↑ / ↓ / Enter`, nút Đánh dấu & Cần ôn lại, mở thử bài IELTS Listening (`Chu Kỳ 11`) click link `.md` tương đối.
   - Creation Wizard `/app/courses/new` & Studio `/app/courses/<id>/edit` → nhập `"Full-Stack"` xem cảnh báo trùng, thử Trợ lý AI bài giảng & modal xóa 2 bước trên bản clone cá nhân.
