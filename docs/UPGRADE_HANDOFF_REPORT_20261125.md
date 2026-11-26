# Báo Cáo Nâng Cấp Toàn Diện, Biên Tập Văn Phong & Kiểm Chứng Studxy

> **Ngày hoàn tất:** 25/11/2026 (GMT+7)  
> **Thư mục làm việc:** `d:\03_Study\Studxy`  
> **Trạng thái nghiệm thu:** Đã hoàn tất 4 đợt nâng cấp UI/UX & biên tập văn phong tiếng Việt trên giao diện web và học liệu, khắc phục mọi lỗi phát hiện qua kiểm thử trình duyệt thật và E2E (Zero-Facade), dọn sạch `swms_local_dev.db` kèm bản sao lưu `.bak` đã xác thực, kiểm thử trực quan trình duyệt thực tế (`55/55` passed, `0` console/page/HTTP errors), chạy `e2e/document-courses-e2e.spec.ts` (`18/18` passed) và hoàn tất **2 lần liên tiếp** toàn bộ `scripts/verify.ps1` (`141/141` E2E + `124/124` Backend + `6/6` Benchmark) sau lần chỉnh sửa mã nguồn cuối cùng.  
> **Lưu ý tệp báo cáo cũ:** Tài liệu bàn giao trước đó tại [UPGRADE_HANDOFF_REPORT.md](file:///d:/03_Study/Studxy/docs/UPGRADE_HANDOFF_REPORT.md) được giữ nguyên vẹn (không ghi đè).

---

## 1. Tổng Quan 4 Đợt Nâng Cấp & Chỉnh Sửa Toàn Hệ Thống

Quy trình nâng cấp được điều phối qua 4 đợt song song bằng đội ngũ CLI (`agy`, `agy2`, `agy3`, `agy4`, `agy6` — 16/16 lượt thực thi kết thúc với `exit 0`), kết hợp rà soát trực tiếp, kiểm thử trực quan trình duyệt thật và sửa lỗi tận gốc từ Coordinator:

### Đợt 1 (`W1-A`..`W1-D`): Design System, Shell Điều Hướng & Các Trang Cốt Lõi
- **Shell & Hệ thống thiết kế:** Chuẩn hóa biến giao diện Light/Dark, Skeleton Loading đồng nhất, focus/keyboard accessibility tại [globals.css](file:///d:/03_Study/Studxy/frontend/src/app/globals.css), [layout.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/layout.tsx), [Sidebar.tsx](file:///d:/03_Study/Studxy/frontend/src/components/navigation/Sidebar.tsx), [MobileNav.tsx](file:///d:/03_Study/Studxy/frontend/src/components/navigation/MobileNav.tsx), [KeyboardShortcutsModal.tsx](file:///d:/03_Study/Studxy/frontend/src/components/auxiliary/KeyboardShortcutsModal.tsx) và [CommandPaletteModal.tsx](file:///d:/03_Study/Studxy/frontend/src/components/command/CommandPaletteModal.tsx).
- **Dashboard & Thư viện:** Nâng cấp màn hình Hôm nay (`/app`) và Thư viện (`/app/library`) với mục tiêu học hằng ngày, dự báo tiến độ và trạng thái trống giàu thông tin tại [app/app/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/page.tsx) và [library/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/library/page.tsx).
- **Catalog, Quick Preview, Course Hub 4 Tab & Chứng chỉ:** Tinh chỉnh Grid/List đồng đều, Quick Preview Drawer, Course Hub 4 tab (có preset `1 / 2 / 3 / 5 bài/ngày` và tự tính ngày đích hoàn thành) cùng trang chứng chỉ tại [courses/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx), [CourseQuickPreviewDrawer.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseQuickPreviewDrawer.tsx), [courses/[id]/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/page.tsx), [CourseSyllabusTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseSyllabusTab.tsx), [CoursePracticeTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CoursePracticeTab.tsx), [CourseAssetsTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseAssetsTab.tsx), [CourseSettingsTab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/courses/CourseSettingsTab.tsx) và [certificate/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/certificate/page.tsx).
- **Xác thực, Tài liệu, Sổ tay & Cài đặt:** Nâng cấp trải nghiệm đăng ký/đăng nhập, bộ lọc theo khóa học trên trang Tài liệu, Sổ tay và Cài đặt tại [auth/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/auth/page.tsx), [documents/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/documents/page.tsx), [notes/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/notes/page.tsx) và [settings/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/settings/page.tsx).

### Đợt 2 (`W2-A`..`W2-D`): Không Gian Học Tập, Markdown Reader, Quiz, Thực Hành & Ôn Tập FSRS
- **Không gian học tập `/learn`:** Cải thiện độ rộng cột đọc (`standard`/`wide`), cuộn mượt lên đầu trang khi chuyển bài, thanh đáy cố định gọn trên mobile và nút tạo thẻ FSRS tại [CourseLearningWorkspace.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx), [AutoGenerateFsrsCard.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/workspace/AutoGenerateFsrsCard.tsx) và [LearningStickyBottomBar.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/workspace/LearningStickyBottomBar.tsx).
- **Trình đọc Markdown & Code:** Nâng cấp khả năng đọc, copy mã nguồn và phóng to sơ đồ Mermaid tại [LessonMarkdownView.tsx](file:///d:/03_Study/Studxy/frontend/src/components/reader/LessonMarkdownView.tsx) và [CodeBlockCard.tsx](file:///d:/03_Study/Studxy/frontend/src/components/reader/markdown/CodeBlockCard.tsx).
- **Quiz Runner, Practice Labs, Ngân hàng Đề thi & Ôn tập FSRS:** Tinh chỉnh điều hướng câu hỏi, chấm điểm và bộ lọc/tiến độ trên [quiz/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/quiz/page.tsx), [practice/[courseId]/[lessonId]/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/practice/%5BcourseId%5D/%5BlessonId%5D/page.tsx), [practice/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/practice/page.tsx), [review/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/review/page.tsx) và [exams/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/exams/page.tsx).

### Đợt 3 (`W3-A`..`W3-D`): Studio Biên Tập, Landing V2, Script Dọn DB & Mở Rộng E2E
- **Studio `/edit` & Wizard `/courses/new`:** Nâng cấp công cụ soạn thảo Markdown, xem trước song song, cảnh báo trùng khóa và Trợ lý AI bài giảng tại [edit/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/edit/page.tsx) và [new/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/new/page.tsx).
- **Landing V2 & Card Khóa học:** Đồng bộ tương phản, banner thẻ và modal tạo khóa học tại [LandingPageV2.tsx](file:///d:/03_Study/Studxy/frontend/src/components/landing/LandingPageV2.tsx), [CourseCardGraphicBanner.tsx](file:///d:/03_Study/Studxy/frontend/src/components/dashboard/CourseCardGraphicBanner.tsx), [CourseCatalogGrid.tsx](file:///d:/03_Study/Studxy/frontend/src/components/dashboard/CourseCatalogGrid.tsx) và [CourseCreateModal.tsx](file:///d:/03_Study/Studxy/frontend/src/components/dashboard/CourseCreateModal.tsx).
- **Gia cố DB Cleanup & E2E:** Gia cố logic nhận diện và dọn khóa test/benchmark trong [cleanup-local-dev-db.ps1](file:///d:/03_Study/Studxy/scripts/cleanup-local-dev-db.ps1) và bổ sung bài kiểm thử E2E không mock cho nâng cấp mới tại [course-lifecycle-upgrade.spec.ts](file:///d:/03_Study/Studxy/frontend/e2e/course-lifecycle-upgrade.spec.ts).

### Đợt 4 (`W4-A`..`W4-D` + Tinh Chỉnh Coordinator): Châu Chuốt Văn Phong Tiếng Việt & Học Liệu
- **Giao diện Web (Frontend):** Chuyển toàn bộ nhãn nội bộ cũ `"Học viên SWMS"` sang `"Học viên Studxy"`, trau chuốt câu chữ tự nhiên, loại bỏ thuật ngữ kỹ thuật nội bộ (`Web Worker`, `1-click`, `tệp PDF`, biểu tượng thừa) trên toàn bộ trang, modal và phòng thực hành (`CodingPracticeLab`, `CodingWriteInViews`, `EnglishPracticeLab`, `FinancePracticeLab`, `PsychologyLifeSkillsLab`, `PsychologyWriteInViews`, `LessonPracticeLab`, `CourseRoadmapSidebar`, `learning-roadmaps.ts`, `CourseInSearchModal`).
- **Metadata & Dữ liệu Khởi tạo (Backend):** Biên tập lại mô tả 10 khóa học chuẩn, thông điệp lỗi thân thiện và sửa lỗi chính tả ngữ liệu tiếng Nhật/Trung (`"おととい"`, `"Shēngcíbiǎo"`) trong [CuratedCourseMetadataCatalog.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/CuratedCourseMetadataCatalog.cs), [DocumentCourseSyncService.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/DocumentCourseSyncService.cs), [SeedDataInitializer.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Infrastructure/Database/SeedDataInitializer.cs), [BootcampPracticeActivityCatalog.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/BootcampPracticeActivityCatalog.cs), [CourseGenerationService.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/CourseGenerationService.cs), [CourseWizardAndImportService.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/CourseWizardAndImportService.cs) cùng `Seed/*.cs`.
- **Học liệu Markdown (`course/` & `document/`):** Rà soát biên tập nội dung giảng dạy cho `IELTS`, `PersonalFinance`, `Investing`, `LifeSkills`, `Psychology`, loại bỏ ghi chú kiểm thử/nội bộ bị rò rỉ (`HANDOFF`, `AI executor`, `pf-release481`, `course-remediation`, đường dẫn máy cục bộ, `LEGACY UNVERIFIED`); giữ nguyên 100% file và liên kết tương đối (`0` liên kết gãy trên `1.474` liên kết đã kiểm tra).

---

## 2. Các Lỗi Thật Đã Phát Hiện & Khắc Phục Qua Kiểm Thử Live & E2E

1. **Phân loại lĩnh vực & nhãn trình độ khóa học:** Ưu tiên `course.category` chuẩn hóa và xử lý từ khóa nhiễu (`kỷ luật`, `hệ thống`) trong [course-helpers.ts](file:///d:/03_Study/Studxy/frontend/src/lib/course-helpers.ts) kèm 3 unit test hồi quy mới trong [course-helpers.test.ts](file:///d:/03_Study/Studxy/frontend/src/lib/course-helpers.test.ts); đồng thời bổ sung `formatLevelLabel` tại [courses/[id]/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/page.tsx) để hiển thị nhãn tiếng Việt (`Cơ bản`, `Trung cấp`, `Chuyên sâu`).
2. **Mất phiên khi `reload()` trình duyệt (`AbortError`):** Bổ sung cờ `isPageUnloading` và `bootstrapAuthSessionDetailed()` (`"authenticated" | "unauthenticated" | "aborted"`) tại [api-client.ts](file:///d:/03_Study/Studxy/frontend/src/lib/api-client.ts), [layout.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/layout.tsx) và [workspace/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/workspace/page.tsx) để request bị hủy khi tải lại trang không xóa nhầm `swms_active_user_id`.
3. **Nút bị cắt/tràn ở chiều rộng Tablet `768px`:** Cập nhật `min-h-*` linh hoạt trên [Button.tsx](file:///d:/03_Study/Studxy/frontend/src/components/ui/Button.tsx) và căn `shrink-0` / `truncate` tại [courses/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx) và [CourseCardGraphicBanner.tsx](file:///d:/03_Study/Studxy/frontend/src/components/dashboard/CourseCardGraphicBanner.tsx).
4. **Lỗi React DOM Nesting (`<pre>` lồng trong `<p>`/`<h3>`):** Đổi thẻ bao ngoài `<RichInlineMarkdown>` sang `<div>` trong phần QuickCheck tại [CourseLearningWorkspace.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx) và Quiz tại [quiz/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/quiz/page.tsx), loại bỏ hoàn toàn cảnh báo hydration/DOM trong Console.
5. **Trùng `data-testid="close-shortcuts-modal"`:** Tách nút footer `"Đã hiểu"` sang `data-testid="dismiss-shortcuts-modal"` tại [KeyboardShortcutsModal.tsx](file:///d:/03_Study/Studxy/frontend/src/components/auxiliary/KeyboardShortcutsModal.tsx), giữ `close-shortcuts-modal` duy nhất ở nút đóng đầu hộp thoại.
6. **Race hiển thị dữ liệu trang cũ khi nhảy trang PDF (`298 → 200`):** Bổ sung kiểm tra `documentId` ở cache và biến `activePageData` đối chiếu đúng `pageNumber === safeCurrentPage` tại [ReaderAndSplitViews.tsx](file:///d:/03_Study/Studxy/frontend/src/components/reader/ReaderAndSplitViews.tsx) để không giữ nội dung trang trước đó trong khung văn bản trích xuất khi đang tải trang mới.
7. **Đồng bộ bộ chọn E2E không mock với giao diện mới:** Bổ sung `data-testid="save-course-metadata-btn"` trên nút `"Lưu thông tin khóa học"` tại [edit/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/%5Bid%5D/edit/page.tsx) và cập nhật [course-lifecycle-upgrade.spec.ts](file:///d:/03_Study/Studxy/frontend/e2e/course-lifecycle-upgrade.spec.ts), [v2-topdown-sitemap.spec.ts](file:///d:/03_Study/Studxy/frontend/e2e/v2-topdown-sitemap.spec.ts) (`getByRole("heading", ...)` trên `/app/documents`).

---

## 3. Kết Quả Dọn Dẹp Dữ Liệu Local (`swms_local_dev.db`)

- **Dry-run (`cleanup-local-dev-db.ps1 -DryRun`):** Kiểm tra đọc ghi 85 khóa học, xác nhận 39 khóa được bảo vệ (đủ `10/10` khóa chuẩn `[OK]`), dự kiến dọn 46 khóa test/retired.
- **Thực thi ghi thật (`cleanup-local-dev-db.ps1`):**
  - Đã tạo và xác thực kích thước bản sao lưu trước khi xóa: `D:\03_Study\Studxy\swms_local_dev.db.bak.20261125-141431` (**206.118.912 bytes**). *(Các bản `.bak` trước đó từ ngày `20261125-035021` và `20261125-035102` cũng được giữ nguyên an toàn.)*
  - Đã dọn **46 khóa học** test/retired (**16** chapters, **44** lessons, **5** enrollments).
  - Sau dọn dẹp & kiểm tra lại cuối phiên bằng `sqlite3`: còn **39 khóa học** sạch, đủ **10/10 khóa chuẩn hệ thống** (`doc-course-studxy-fullstack-bootcamp`, `document-course-ielts`, `document-course-investing`, `document-course-lifeskills`, `document-course-personalfinance`, `document-course-psychology`, `he-dieu-hanh-bo-nho-ao`, `tieng-anh-giao-tiep-thuc-chien-a2-b1`, `tieng-nhat-so-cap-n5`, `tieng-trung-so-cap-hsk1`), tài khoản `'son.tran@studxy.vn'` (`Sơn Trần`) còn nguyên vẹn, `PRAGMA quick_check = ok`.

---

## 4. Bảng Tổng Hợp Bằng Chứng Kiểm Chứng

| Hạng mục kiểm chứng | Lệnh thực thi chính xác | Kết quả & Số liệu thực tế | Đường dẫn Log / Bằng chứng |
|---|---|---|---|
| **Backend Release Build & xUnit** | `dotnet build d:\03_Study\Studxy\backend\Swms.Api.sln -c Release` & `dotnet test ... --no-build` *(cùng bước G trong các lần `verify.ps1`)* | **Exit `0`** — `0 Error(s)`, **124 / 124 Passed** (`0 Failed`, `0 Skipped`) | `scratch\backend-build-test.log` |
| **Frontend Unit Tests & Production Build** | `npm --prefix d:\03_Study\Studxy\frontend run test:unit` & `npm --prefix d:\03_Study\Studxy\frontend run build` *(env local `127.0.0.1:5080`)* | **Exit `0`** — 5 bộ unit test pass 100% (`FSRS v5`, `21/21` phân loại, `16/16` language-practice, `8/8` practice-helpers, `14/14` writein-practice); `next build` pass **20/20 route** | `scratch\frontend-pre-audit-build.log` |
| **DB Cleanup Dry-run & Write** | `powershell -ExecutionPolicy Bypass -File d:\03_Study\Studxy\scripts\cleanup-local-dev-db.ps1 [-DryRun]` | **Exit `0`** — Backup `206.118.912` bytes, dọn `46` khóa rác, còn `39` khóa (`10/10` chuẩn + `son.tran@studxy.vn`), `PRAGMA quick_check = ok` | `scratch\db-cleanup-dryrun.log`, `scratch\db-cleanup-exec.log` |
| **Kiểm thử Trực quan Trình duyệt Thật (0 mock)** | `powershell -ExecutionPolicy Bypass -File ...\scratch\run-live-audit.ps1` *(Backend Release `:5080` + `next start :3000`, sau lần sửa cuối)* | **Exit `0`** (`LIVE_AUDIT_EXIT=0`) — **55/55 checks passed**, `PageErrors: 0`, `ConsoleErrors: 0`, `HttpFailures: 0`, lưu **23** ảnh chụp Desktop/Tablet/Mobile Light+Dark | `scratch\live-audit-post-final-edit.log`, `scratch\audit_shots\` |
| **5 Suite Trọng tâm + `document-courses-e2e`** | `powershell -ExecutionPolicy Bypass -Command "& 'd:\03_Study\Studxy\scripts\verify.ps1' -Suites @('e2e/bootcamp-exam-and-learn-ux.spec.ts','e2e/course-lifecycle-upgrade.spec.ts','e2e/pdf-multipage-e2e.spec.ts','e2e/v2-topdown-sitemap.spec.ts','e2e/document-courses-e2e.spec.ts')"` | **Exit `0`** (`452.54s`) — **124/124** Backend, **6/6** Benchmark, **18/18** Playwright E2E tests | [verification/20261125-150204](file:///d:/03_Study/Studxy/test-results/verification/20261125-150204), `scratch\verify-targeted-and-doc.log` |
| **`scripts/verify.ps1` Full Run #1** | `powershell -ExecutionPolicy Bypass -File d:\03_Study\Studxy\scripts\verify.ps1` *(sau lần sửa cuối)* | **Exit `0`** (`732.43s`) — **124/124** Backend, **6/6** Benchmark (`377 reqs`), **141/141** Playwright E2E (14/14 suite) | [verification/20261125-151220](file:///d:/03_Study/Studxy/test-results/verification/20261125-151220), `scratch\verify-full-run1.log` |
| **`scripts/verify.ps1` Full Run #2 (liên tiếp)** | `powershell -ExecutionPolicy Bypass -File d:\03_Study\Studxy\scripts\verify.ps1` *(lần 2 liên tiếp, 0 chỉnh sửa xen giữa)* | **Exit `0`** (`738.84s`) — **124/124** Backend, **6/6** Benchmark (`377 reqs`), **141/141** Playwright E2E (14/14 suite) | [verification/20261125-152444](file:///d:/03_Study/Studxy/test-results/verification/20261125-152444), `scratch\verify-full-run2.log` |
| **Kiểm tra Hậu kiểm Tiến trình & Repo** | `Get-NetTCPConnection -LocalPort 5080,3000`, `Get-Process`, `git status -s`, `git diff --stat`, `sqlite3` check | `PORTS_5080_3000_CLEAN=true`, không còn tiến trình `agy`/`Swms.Api`/`node` Studxy chạy nền; `67` file tracked thay đổi (`+5130 / -1699`); DB `ok` | `C:\Users\Administrator\.gemini\antigravity\brain\952de4f0-6307-4ddf-8f94-f72f9b8f7da4\.system_generated\steps\666\output.txt` |

*(Ghi chú: Vì `scripts/verify.ps1` khởi chạy `npx next dev`, thư mục tạm `.next/` sau lần kiểm chứng cuối đang ở trạng thái dev cache. Nếu muốn chạy `next start`, hãy chạy `npm --prefix frontend run build` trước.)*

---

## 5. Chưa Kiểm Chứng

- **Phát âm thanh thực tế qua phần cứng loa/tai nghe:** E2E và audit mới kiểm tra phần tử `<audio>` render đúng và endpoint tài nguyên `.wav` trả mã HTTP `200`/`206`; chưa nghe kiểm tra trực tiếp âm thanh vật lý hoặc Web Speech microphone thật.
- **Đăng nhập Google OAuth live & Gemini AI Proxy production:** Quá trình kiểm thử cục bộ cấu hình `NEXT_PUBLIC_GOOGLE_CLIENT_ID=DISABLED` và `--AiProxy:BaseUrl=http://127.0.0.1:18045` (fallback cục bộ); chưa xác minh popup Google OAuth thật và hạn mức/phản hồi từ Google Gemini Proxy bên ngoài.
- **Thao tác cảm ứng trên thiết bị thật & PostgreSQL/Supabase production:** Responsive đa màn hình được kiểm chứng qua Chromium trên các viewport từ `360x640` đến `2560x1440`, chưa test trực tiếp trên màn hình cảm ứng vật lý; toàn bộ DB test chạy trên SQLite cục bộ, chưa migrate/test trên PostgreSQL/Supabase cloud sau lần cập nhật này.
- **Git Commit / Push:** Chưa tạo commit hay push lên remote để bạn tự rà soát `git status` và nội dung thay đổi trước.

---

## 6. Hướng Dẫn Tự Nghiệm Thu Nhanh Trong 5 Phút (Sau Khi Bật Máy Lại)

1. **Khởi chạy máy chủ local:**
   - Terminal 1 (Backend `:5080`):
     ```powershell
     Set-Location d:\03_Study\Studxy
     dotnet run --project backend\src\Swms.Api\Swms.Api.csproj -c Release -- --urls http://127.0.0.1:5080
     ```
   - Terminal 2 (Frontend `:3000`, dev hoặc build + start):
     ```powershell
     Set-Location d:\03_Study\Studxy\frontend
     $env:BACKEND_INTERNAL_URL = "http://127.0.0.1:5080"
     $env:NEXT_PUBLIC_API_BASE_URL = "http://127.0.0.1:5080"
     npm run dev
     ```
2. **Kiểm tra nhanh giao diện & dữ liệu thật (~3 phút):**
   - Truy cập `http://127.0.0.1:3000`: thử chuyển Sáng/Tối trên Landing V2, vào `/auth` đăng nhập hoặc đăng ký tài khoản mới, kiểm tra tên người dùng trên Sidebar và nhấn `F5` để xác nhận giữ phiên.
   - Mở `/app/courses` (kiểm tra đủ `10` khóa chuẩn, lọc/tìm kiếm, Quick Preview Drawer), vào một khóa học để kiểm tra Course Hub 4 tab (lưu mục tiêu bài/ngày trong Cài đặt), sau đó vào `/learn` thử `?`, `Ctrl+K`, `Ctrl+Shift+F`, phòng Thực hành Lab, Quiz và Ôn tập FSRS.
3. **Kiểm tra Git & DB khi sẵn sàng commit:**
   ```powershell
   Set-Location d:\03_Study\Studxy
   git status -s
   Get-ChildItem swms_local_dev.db* | Select-Object Name, Length, LastWriteTime
   ```
