# Báo Cáo Bàn Giao Nâng Cấp Chương Trình Học 2026+, Retention Loop & Tối Ưu Tải Trang Studxy

> **Thời điểm hoàn tất:** 26/11/2026 01:12 (GMT+7)  
> **Thư mục dự án:** `d:\03_Study\Studxy`  
> **Trạng thái nghiệm thu:** Đã hoàn tất 100% nội dung 4 khóa học 2026+ mới, nâng cấp 10 khóa học chuẩn cũ, hệ thống `AppliedSystemsLab` + Retention Loop, sửa lỗi `403` trên quiz công khai, tinh chỉnh tương phản Light/Dark nút lưu FSRS, tối ưu tải trang 3 lớp (Backend `compact=true` + phân trang `limit/offset` + Progressive Reveal 4–8 mục kèm nút `Hiển thị thêm` + SWR `sessionStorage` + `content-visibility: auto`), kiểm chứng toàn diện qua `scripts/verify.ps1` (`130/130` Backend + `6/6` Benchmark + `145/145` E2E), `e2e/document-courses-e2e.spec.ts` (`5/5` E2E) và kiểm thử trình duyệt thực tế (`browser` subagent `10/10` luồng).  
> **Lưu ý tệp báo cáo cũ:** Các báo cáo trước đó tại [UPGRADE_HANDOFF_REPORT.md](file:///d:/03_Study/Studxy/docs/UPGRADE_HANDOFF_REPORT.md) và [UPGRADE_HANDOFF_REPORT_20261125.md](file:///d:/03_Study/Studxy/docs/UPGRADE_HANDOFF_REPORT_20261125.md) được giữ nguyên vẹn (không ghi đè).

---

## 1. Số Liệu Trước / Sau Của 14 Khóa Chuẩn Trong `swms_local_dev.db`

- **Bản sao lưu đã xác thực:** `d:\03_Study\Studxy\swms_local_dev.db.bak.20261125-234531` (**206.118.912 bytes**).
- **Trạng thái SQLite hiện tại (`swms_local_dev.db`):**
  - `PRAGMA quick_check = ok`
  - Tổng khóa học trước → sau: `39` (`10` khóa chuẩn cũ) → `44` (`14` khóa chuẩn mới + `1` personal workspace tự tạo khi test đăng ký user thật qua trình duyệt)
  - Tài khoản `son.tran@studxy.vn` (`11111111-1111-4111-8111-111111111111`) nguyên vẹn (`users = 668` → `669` do thêm 1 user test thật `browser.walkthrough.*@studxy.vn`)
  - Bất biến `LessonPagingId` (`44444444-4444-4444-8444-444444444444`, slug `bo-nho-ao-tlb-va-bang-trang-da-cap`): `current_version = 2` (giữ nguyên)
  - Khóa `document-course-personalfinance`: giữ nguyên bất biến **14 bài học**

| STT | Slug Khóa Chuẩn | Tên Khóa Học | Chương (Trước → Sau) | Bài học (Trước → Sau) | Thiếu Practice (`missing_pa`) | Practice Activities | Flashcards FSRS | Quiz Items |
|---|---|---|---|---|---|---|---|---|
| 1 | `doc-course-studxy-fullstack-bootcamp` | Studxy Full-Stack Developer Bootcamp (Zero to Production, AI & DevOps) | 12 → 12 | 150 → 150 | 0 → 0 | 509 → 509 | 380 → **996** | 1294 → 1294 |
| 2 | `document-course-ielts` | IELTS Mastery Roadmap (Phase 0 – Phase 3) | 9 → 9 | 494 → 494 | 99 → **0** | 1190 → **1487** | 1860 → 1860 | 6681 → **6969** |
| 3 | `document-course-investing` | Đầu Tư Tài Chính Nền Tảng (18 Tuần) | 5 → 5 | 21 → 21 | 2 → **0** | 128 → **134** | 2186 → 2186 | 2534 → 2534 |
| 4 | `document-course-lifeskills` | Kỹ Năng Sống & Giao Tiếp Hiệu Quả (16 Tuần) | 5 → 5 | 18 → 18 | 1 → **0** | 56 → **59** | 3150 → 3150 | 2456 → 2456 |
| 5 | `document-course-personalfinance` | Tài Chính Cá Nhân Thực Hành (12 Tuần) | 5 → 5 | **14 → 14** *(bất biến)* | 0 → 0 | 47 → 47 | 4680 → 4680 | 1936 → 1936 |
| 6 | `document-course-psychology` | Tự Học Tâm Lý Học 48 Tuần | 7 → 7 | 52 → 52 | 3 → **0** | 218 → **227** | 6800 → 6800 | 8646 → 8646 |
| 7 | `he-dieu-hanh-bo-nho-ao` | Hệ điều hành: Kiến trúc Bộ nhớ ảo & Phân trang | 3 → **5** | 6 → **12** | 0 → 0 | 24 → **48** | 17 → **92** | 119 → **169** |
| 8 | `tieng-anh-giao-tiep-thuc-chien-a2-b1` | Tiếng Anh Giao Tiếp Thực Chiến (A2 → B1) | 3 → **5** | 6 → **12** | 0 → 0 | 78 → **126** | 60 → **118** | 255 → **305** |
| 9 | `tieng-nhat-so-cap-n5` | Tiếng Nhật Sơ Cấp (N5) | 3 → **5** | 6 → **12** | 0 → 0 | 48 → **96** | 60 → **120** | 237 → **289** |
| 10 | `tieng-trung-so-cap-hsk1` | Tiếng Trung Sơ Cấp (HSK1) | 3 → **5** | 6 → **12** | 0 → 0 | 48 → **96** | 60 → **120** | 237 → **289** |
| 11 | `document-course-ai-workflows` *(Mới)* | AI Ứng Dụng & Tự Động Hóa Workflow Thực Chiến (2026) | **5** | **13** | **0** | **39** | **120** | **96** |
| 12 | `document-course-data-sql` *(Mới)* | Tư Duy Dữ Liệu, SQL & Ra Quyết Định Bằng Số Liệu | **5** | **13** | **0** | **39** | **120** | **120** |
| 13 | `document-course-cybersecurity` *(Mới)* | An Toàn Thông Tin, Quyền Riêng Tư & Phòng Vệ Lừa Đảo Số | **5** | **11** | **0** | **33** | **100** | **80** |
| 14 | `document-course-deep-learning-systems` *(Mới)* | Học Sâu, Tư Duy Hệ Thống & Quản Trị Hiệu Suất Thời AI | **5** | **11** | **0** | **33** | **100** | **100** |

- Bảo vệ danh sách 14 slug chuẩn trong [cleanup-local-dev-db.ps1](file:///d:/03_Study/Studxy/scripts/cleanup-local-dev-db.ps1) và [CuratedCourseMetadataCatalog.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/CuratedCourseMetadataCatalog.cs).

---

## 2. Kiến Trúc Đã Triển Khai Trong Đợt Nâng Cấp

### 2.1. `AppliedSystemsLab` + 4 Chế Độ Chấm Điểm Thực Hành
- **Backend ([SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs)):** Bổ sung 4 mode chấm bài trong endpoint đánh giá thực hành & gợi nhớ: `prompt_critique`, `sql_reasoning`, `threat_spotter`, và `active_recall`.
- **Frontend ([AppliedSystemsLab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/practice/AppliedSystemsLab.tsx), [LessonPracticeLab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/LessonPracticeLab.tsx)):** Tích hợp chế độ `"applied"` cho 4 khóa học 2026+ mới với luồng 2 bước (chẩn đoán lỗi/lỗ hổng → viết lại câu trả lời/truy vấn → chấm điểm thật hoặc offline fallback trung thực kèm nhãn rõ ràng → lưu vào FSRS v5).

### 2.2. Retention Loop (Ghi Nhớ Chủ Động & Ôn Tập Xen Kẽ FSRS)
- **Active Recall trong bài giảng ([LessonActiveRecallCard.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/workspace/LessonActiveRecallCard.tsx)):** Cho phép người học tự tóm tắt bài trước khi xem đáp án, chấm điểm theo từ khóa/AI, và lưu điểm yếu thành thẻ FSRS (`active-recall-save-fsrs-btn`). Đã tinh chỉnh tương phản WCAG Light/Dark cho trạng thái đã lưu: `bg-emerald-500/15 text-emerald-700 border-emerald-500/35 dark:text-emerald-300`.
- **Lưu câu sai 1 chạm từ Quick Check & Quiz:** `quick-check-save-wrong-fsrs-btn` trong [CourseLearningWorkspace.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx) và `quiz-save-wrong-fsrs-btn` trong [LessonPracticeLab.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/LessonPracticeLab.tsx).
- **Ôn tập xen kẽ đa chủ đề ([review/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/review/page.tsx)):** Tham số `interleave=true` trên Backend cùng nút `[data-testid="review-interleave-toggle"]` và thanh độ chắc nhớ phiên `[data-testid="review-session-mastery-bar"]`.
- **Khắc phục lỗi `403` trên Quiz Công Khai ([SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs)):** Bổ sung helper `IsPublicCurriculumQuizAsync` cho `submit`, `reset` và `submit-all`, giúp học viên mới làm bài & nộp điểm trên các quiz của khóa công khai trả `200 OK` mà không ghi đè trạng thái của học viên khác.

### 2.3. Tối Ưu Tải Trang 3 Lớp (Backend + Progressive Reveal + SWR Cache)
1. **Lớp 1 — Backend Payload Gọn & Phân Trang Tương Thích Ngược ([SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs)):**
   - `GET /api/v1/workspace/dashboard?compact=true`: Bỏ mảng ~800+ `lessons`, giới hạn 12 quiz gần nhất, giữ nguyên `documentsCount`, `notebooksCount`, `totalQuizzesCount` và giữ nguyên shape cũ khi không truyền `compact=true`.
   - `GET /api/v1/courses?limit=8&offset=0` & `GET /api/v1/documents?limit=8&offset=0`: Cắt phân trang trước khi build summary, trả `{ items, totalCount, offset, limit, hasMore }` và header `X-Total-Count` (giữ nguyên mảng đầy đủ khi không truyền `limit/offset`).
2. **Lớp 2 — Hiển Thị 4–8 Mục Đầu + Prefetch Ngầm ([ShowMoreFooter.tsx](file:///d:/03_Study/Studxy/frontend/src/components/ui/ShowMoreFooter.tsx)):**
   - `/app` ([app/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/page.tsx)): Hiển thị 4 khóa đầu + nút `dashboard-courses-show-more-btn` (`+4`).
   - `/app/courses` ([courses/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx)): Tải trang đầu 8 khóa (`6` với List view), prefetch ngầm phần còn lại khi trình duyệt rảnh (`requestIdleCallback`), hiện ngay tổng số khóa và nút `courses-show-more-btn` / `courses-show-all-btn`.
   - `/app/library` ([library/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/library/page.tsx)): Hiển thị 8 mục đầu + `library-show-more-btn`.
   - `/app/documents` ([documents/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/documents/page.tsx)) & `/app/notes` ([notes/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/notes/page.tsx)): Hiển thị 6 mục đầu + `documents-show-more-btn` / `notes-show-more-btn`.
3. **Lớp 3 — SWR `sessionStorage` Snapshot & CSS Paint ([api-client.ts](file:///d:/03_Study/Studxy/frontend/src/lib/api-client.ts), [globals.css](file:///d:/03_Study/Studxy/frontend/src/app/globals.css)):**
   - Snapshot ngắn hạn theo `userId` (TTL 5 phút, tự xóa khi `invalidateDashboardCache()` / `swms:data-mutated`) giúp chuyển qua lại giữa `/app`, `/app/courses`, `/app/library` hiển thị tức thì và revalidate ngầm.
   - Gắn `.swms-cv-card { content-visibility: auto; contain-intrinsic-size: 0 320px; }` cho card danh sách.

---

## 3. Bằng Chứng Kiểm Chứng Thực Tế

| Hạng mục kiểm chứng | Lệnh / Nguồn thực thi | Kết quả & Số liệu thực tế | Đường dẫn Log / Bằng chứng |
|---|---|---|---|
| **Frontend Unit Tests & Build** | `npm run test:unit` & `npm run build` trong `d:\03_Study\Studxy\frontend` *(env `127.0.0.1:5080`, `GOOGLE_CLIENT_ID=DISABLED`)* | **Exit `0`** — 5/5 bộ unit test pass, `next build` thành công **20/20 static pages** | [task-242.log](file:///C:/Users/Administrator/.gemini/antigravity/brain/4cd74d63-8220-435e-920a-b31315e5b01d/.system_generated/tasks/task-242.log), [task-304.log](file:///C:/Users/Administrator/.gemini/antigravity/brain/4cd74d63-8220-435e-920a-b31315e5b01d/.system_generated/tasks/task-304.log) |
| **`verify.ps1` Targeted (`document-courses-e2e`)** | `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1 -Suites @('e2e/document-courses-e2e.spec.ts')` | **Exit `0`** (`528.06s`) — **127/127** Backend, **6/6** Benchmark (`377 reqs`), **5/5** Playwright E2E | [verification-summary.log](file:///d:/03_Study/Studxy/test-results/verification/20261125-235301/verification-summary.log) |
| **`verify.ps1` Full Run (Sau Tối Ưu & Fix 403/Race)** | `powershell -ExecutionPolicy Bypass -File d:\03_Study\Studxy\scripts\verify.ps1` | **Exit `0`** (`926.97s`) — **130/130** Backend (`+3` test mới), **6/6** Benchmark (`377 reqs`), **145/145** Playwright E2E (14/14 suites) | [verification-summary.log](file:///d:/03_Study/Studxy/test-results/verification/20261126-004951/verification-summary.log) |
| **Live Browser Subagent (`browser`, 0 mock, 10 luồng)** | Khởi chạy Backend Release `:5080` (`swms_local_dev.db`) + Frontend `:3000`, `invoke_subagent` `7b8bc35c` | **10/10 luồng PASS** (Landing Light/Dark, Auth + F5, `/app` 4→8 khóa, `/app/library` 8→14 mục, `/app/courses` 8→14 khóa + Search + Quick Preview, Course Hub 4 tab, `/learn` Quick Check `0` lỗi 403 + Active Recall + nút lưu FSRS rõ cả Light/Dark, `AppliedSystemsLab`, Quiz 8/8 `0` lỗi 403 + `/app/review` Interleaved, `/workspace`/Docs/Notes/Exams/Settings), `0` console/page errors, `visited_origins=http://127.0.0.1:3000`, `blocked_origins=none` | [final_step_54.png](file:///C:/Users/Administrator/.gemini/antigravity/brain/4cd74d63-8220-435e-920a-b31315e5b01d/browser_evidence/7b8bc35c-33ff-4093-b868-98843617ec43/final_step_54.png) |
| **Hậu kiểm Tiến trình & SQLite** | `Get-NetTCPConnection -LocalPort 5080,3000`, `sqlite3` `PRAGMA quick_check`, kiểm tra backup & báo cáo cũ | `PORTS_5080_3000_CLEAN=True`, `quick_check=ok`, `son.tran@studxy.vn` & `LessonPagingId` `current_version=2` nguyên vẹn, backup `206.118.912` bytes & 2 báo cáo cũ nguyên vẹn | `d:\03_Study\Studxy\swms_local_dev.db` |

---

## 4. Tình Trạng PostgreSQL (Supabase)

- **Chưa đồng bộ dữ liệu lên Supabase PostgreSQL (`aws-1-ap-northeast-2.pooler.supabase.com`):** Toàn bộ nâng cấp dữ liệu mới chỉ thực hiện và kiểm chứng trên SQLite cục bộ (`swms_local_dev.db` và `swms_verify.db`). Không có thay đổi schema/DDL (`database/schema.sql` và `SwmsDbContext.cs` giữ nguyên).
- **Lưu ý quan trọng khi muốn đồng bộ lên PostgreSQL:** Trong [Program.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Program.cs#L304), điều kiện fast cold-start hiện là `if (forceStartupSeed || !hasAnyUser || existingCourseCount < 10)`. Khi triển khai lên Supabase đã có `>= 10` khóa, cần chạy một lần với biến môi trường `SWMS_FORCE_STARTUP_SEED=true` (và nên tạo snapshot trên Supabase trước khi chạy) hoặc nâng ngưỡng kiểm tra trong `Program.cs`.

---

## 5. Chưa Kiểm Chứng

- **PostgreSQL / Supabase Cloud:** Chưa chạy đồng bộ dữ liệu hay E2E trực tiếp trên Supabase PostgreSQL (chờ xác nhận Tier 3 của chủ dự án).
- **Google OAuth & Gemini AI Proxy Production:** Các lượt kiểm thử chạy với `GOOGLE_CLIENT_ID=DISABLED` và `--AiProxy:BaseUrl=http://127.0.0.1:18045` (xác minh nhánh fallback ngoại tuyến trung thực); chưa đo đạc phản hồi live từ Gemini API ngoài và popup OAuth Google thật.
- **Audio vật lý & Thiết bị cảm ứng thật:** Đã xác minh thẻ `<audio>` và endpoint `.wav` HTTP `200`/`206` cùng layout đa viewport, chưa kiểm thử loa/mic phần cứng hoặc thao tác cảm ứng trên điện thoại thật.
- **Git Commit / Push:** Chưa thực hiện `git commit` hay `git push` để bạn rà soát `git status` và `git diff` trước khi đẩy mã.

---

## 6. Hướng Dẫn Tự Nghiệm Thu Nhanh Trong 5 Phút

1. **Khởi chạy máy chủ local:**
   - **Terminal 1 (Backend `:5080`, SQLite cục bộ):**
     ```powershell
     Set-Location d:\03_Study\Studxy
     $env:SWMS_USE_POSTGRES = "false"
     dotnet run --project backend\src\Swms.Api\Swms.Api.csproj -c Release -- --urls http://127.0.0.1:5080
     ```
   - **Terminal 2 (Frontend `:3000`):**
     ```powershell
     Set-Location d:\03_Study\Studxy\frontend
     $env:BACKEND_INTERNAL_URL = "http://127.0.0.1:5080"
     $env:NEXT_PUBLIC_API_BASE_URL = "http://127.0.0.1:5080"
     npm run dev
     ```
2. **Kiểm tra nhanh (~3 phút):**
   - Mở `http://127.0.0.1:3000/app` → thấy 4 khóa đầu + nút **Hiển thị thêm (+4)**; chuyển sang `/app/courses` → thấy 8 thẻ đầu + badge **14 khóa**, bấm **Hiển thị thêm** để xem đủ 14 khóa gồm 4 khóa 2026+ mới.
   - Mở khóa **AI Ứng Dụng & Tự Động Hóa Workflow Thực Chiến (2026)** → `/learn`: kiểm tra Quick Check, thử **Gợi nhớ chủ động** + nút lưu FSRS ở cả Light Mode & Dark Mode, rồi sang **Thực hành (`AppliedSystemsLab`)** và **Ôn tập (`/app/review`)** bật chế độ **Đan xen chủ đề**.
3. **Kiểm tra DB & Git:**
   ```powershell
   Set-Location d:\03_Study\Studxy
   git status -s
   Get-ChildItem swms_local_dev.db* | Select-Object Name, Length, LastWriteTime
   ```

---

## 7. Kết Quả Đồng Bộ & Kiểm Chứng PostgreSQL (Supabase) — 26/11/2026

> **Mục tiêu kết nối:** `aws-1-ap-northeast-2.pooler.supabase.com:5432` (`db=postgres`, `SslMode=Require`; thông tin xác thực trong `backend/src/Swms.Api/appsettings.Development.local.json` được ẩn hoàn toàn).  
> **Trạng thái:** Đồng bộ idempotent hoàn tất **14/14 khóa chuẩn**, bảo toàn 100% dữ liệu người dùng và bất biến hệ thống, đã xác minh Fast Cold-Start, API thực tế và smoke test trình duyệt (0 mock).

### 7.1. Sửa Lỗi Gốc Rễ Phát Hiện Khi Đồng Bộ PostgreSQL
- **Chẩn đoán:** Trong lần chạy đồng bộ đầu tiên, hàm `EnsureSqliteSchemaUpdatedAsync` ở [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L269) gộp nhiều câu lệnh DDL Npgsql vào một batch `ExecuteSqlRawAsync` và nuốt ngoại lệ bằng `catch { }`, dẫn tới **11 cột additive** trên PostgreSQL chưa được thêm và `DocumentCourseSyncService` gặp lỗi `42703: column c.category does not exist`.
- **Khắc phục sạch (Zero-Facade):**
  1. Tách riêng từng câu lệnh DDL Npgsql trong [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L271) và thực thi các DDL cột/bảng bắt buộc qua ADO.NET `DbCommand` (không nuốt lỗi).
  2. Bổ sung `CanonicalCourseSlugs` (nguồn chuẩn 14 slug) trong [CuratedCourseMetadataCatalog.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/CuratedCourseMetadataCatalog.cs#L319) và cập nhật điều kiện fast cold-start trong [Program.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Program.cs#L296-L329) để kiểm tra đủ `14/14` slug chuẩn (thay cho ngưỡng `< 10` cũ), đồng thời đặt fallback `CommandTimeout = 180` trong `NormalizePostgresConnectionString`.

### 7.2. Đối Chiếu Trước / Sau Trên Supabase PostgreSQL (Read-Only Snapshots)
- **Schema additive (`information_schema.columns`):** Đủ **27/27 bảng** khớp [SwmsDbContext.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Infrastructure/Database/SwmsDbContext.cs) + **11/11 cột additive** đã có mặt (`courses`: `category, level, learning_outcomes, prerequisites, is_archived`; `course_enrollments`: `is_pinned, is_archived, daily_lesson_goal, target_completion_date`; `user_lesson_progress`: `is_bookmarked, needs_review`).
- **Bảo toàn dữ liệu & bất biến (Before → After ngay sau đồng bộ):**
  - `users`: `47 → 47` (nguyên vẹn; tài khoản `son.tran@studxy.vn` `11111111-1111-4111-8111-111111111111` nguyên vẹn)
  - `courses`: `62 → 66` (thêm đúng 4 khóa 2026+ mới, **`duplicate_course_slugs = 0`**)
  - `chapters`: `97 → 125` · `lessons`: `731 → 803` · `practice_activities`: `2.500 → 3.003` · `flashcards`: `2.891 → 4.443` · `practice_quizzes`: `1.127 → 1.296` · `practice_quiz_items`: `16.246 → 17.394`
  - Dữ liệu người dùng giữ nguyên vẹn: `user_lesson_progress = 21 → 21`, `flashcard_reviews = 2 → 2`, `practice_attempts = 5 → 5` *(trước smoke test)*, `annotations = 5 → 5`, `notebooks = 26 → 26`, `course_enrollments = 22 → 26`
  - Bất biến `LessonPagingId` (`44444444-4444-4444-8444-444444444444`, slug `bo-nho-ao-tlb-va-bang-trang-da-cap`): **`current_version = 2`** (giữ nguyên)
  - Khóa `document-course-personalfinance`: **`5 chương / 14 bài học`** (giữ nguyên bất biến)

| STT | Slug Khóa Chuẩn | Chương (Trước → Sau) | Bài học (Trước → Sau) | `missing_pa` (Trước → Sau) | Practice Activities | Flashcards FSRS | Quiz Items |
|---|---|---|---|---|---|---|---|
| 1 | `doc-course-studxy-fullstack-bootcamp` | 12 → 12 | 150 → 150 | 0 → **0** | 450 → **509** | 763 → **1380** | 4574 → **4694** |
| 2 | `document-course-ielts` | 9 → 9 | 395 → 395 | 0 → **0** | 1190 → 1190 | 240 → **300** | 6624 → **6912** |
| 3 | `document-course-investing` | 5 → 5 | 19 → 19 | 0 → **0** | 62 → **128** | 290 → 290 | 540 → **610** |
| 4 | `document-course-lifeskills` | 5 → 5 | 17 → 17 | 0 → **0** | 56 → 56 | 210 → 210 | 320 → 320 |
| 5 | `document-course-personalfinance` | 5 → 5 | **14 → 14** *(bất biến)* | 0 → **0** | 47 → 47 | 159 → 159 | 242 → 242 |
| 6 | `document-course-psychology` | 7 → 7 | 49 → 49 | 0 → **0** | 152 → **218** | 850 → 850 | 1608 → **1678** |
| 7 | `he-dieu-hanh-bo-nho-ao` | 3 → **5** | 6 → **12** | 0 → **0** | 24 → **48** | 14 → **89** | 193 → **243** |
| 8 | `tieng-anh-giao-tiep-thuc-chien-a2-b1` | 3 → **5** | 6 → **12** | 0 → **0** | 78 → **126** | 0 → **120** | 170 → **220** |
| 9 | `tieng-nhat-so-cap-n5` | 3 → **5** | 6 → **12** | 0 → **0** | 48 → **96** | 0 → **120** | 237 → **289** |
| 10 | `tieng-trung-so-cap-hsk1` | 3 → **5** | 6 → **12** | 0 → **0** | 48 → **96** | 0 → **120** | 158 → **210** |
| 11 | `document-course-ai-workflows` *(Mới)* | 0 → **5** | 0 → **13** | **0** | **39** | **120** | **96** |
| 12 | `document-course-data-sql` *(Mới)* | 0 → **5** | 0 → **13** | **0** | **39** | **120** | **120** |
| 13 | `document-course-cybersecurity` *(Mới)* | 0 → **5** | 0 → **11** | **0** | **33** | **100** | **80** |
| 14 | `document-course-deep-learning-systems` *(Mới)* | 0 → **5** | 0 → **11** | **0** | **33** | **100** | **100** |

### 7.3. Bằng Chứng Kiểm Chứng Thực Tế Trên PostgreSQL (Supabase)

| Hạng mục | Lệnh / Nguồn thực thi | Kết quả & Số liệu đo được | Exit code / Trạng thái |
|---|---|---|---|
| **Build & xUnit Backend (Release)** | `dotnet build backend\Swms.Api.sln -c Release` & `dotnet test backend\tests\Swms.Tests\Swms.Tests.csproj -c Release --no-build` | **130/130** tests pass (`0` failed, `0` skipped, `6m 55s`), `0 Error(s)`, không chạm cloud DB | Exit `0` |
| **Đồng bộ lần 1 lên Supabase** | `SWMS_USE_POSTGRES=true`, `SWMS_FORCE_STARTUP_SEED=true`, `dotnet backend\src\Swms.Api\bin\Release\net10.0\Swms.Api.dll --contentRoot d:\03_Study\Studxy\backend\src\Swms.Api --urls http://127.0.0.1:5080` | Log `PostgreSQL (Supabase)` → `Now listening on: http://127.0.0.1:5080` sạch **0 warning/exception**; `/health` HTTP `200` | Hoàn tất sạch |
| **Fast Cold-Start (không force seed)** | Khởi chạy lại Backend `:5080` chỉ với `SWMS_USE_POSTGRES=true` | Log `Fast cold-start: PostgreSQL already has all 14 canonical courses (66 total). Running lightweight schema check in background.` · `/health` HTTP `200` (`336 ms`) | Pass |
| **API Thực Tế (Supabase Live)** | `POST /api/v1/auth/login` (`497 ms`) → `GET /api/v1/workspace/dashboard?compact=true` · `GET /api/v1/courses?limit=8&offset=0` & `offset=8` | `dashboard?compact=true`: HTTP `200`, `10.064 ms`, `665.482 bytes`, `courses=14`, `lessonsCount=0`, `recentQuizzes=1` · `courses?limit=8&offset=0`: HTTP `200`, `2.821 ms`, `573.675 bytes`, `items=8`, `totalCount=14`, `hasMore=True`, `X-Total-Count=14` · `offset=8`: HTTP `200`, `1.805 ms`, `83.139 bytes`, `items=6`, `hasMore=False` | Exit `0` |
| **Smoke Test Trình Duyệt (0 Mock)** | Frontend `:3000` (`npm run dev`) + Backend `:5080` (Supabase) qua `browser` subagent `81ac6c0d` (`visited_origins=http://127.0.0.1:3000`, `blocked_origins=none`) | Đăng nhập `son.tran@studxy.vn` qua `/auth` → `/app` hiển thị `4 / 14` khóa + `Hiển thị thêm (+ 4 )` → `/app/courses` `8` thẻ đầu → bấm `Hiển thị thêm (+ 6 )` → đủ `14` thẻ → mở `document-course-ai-workflows` (`/learn` Quick Check đúng `2/2`; `AppliedSystemsLab` bước 1 chẩn đoán chính xác & bước 2 AI chấm `85/100` HTTP `200`). **`0` lỗi `403`, `0` lỗi `500`, `0` `pageerror`** (1 `console.error` từ Google Sign-In SDK trên `/auth` do `GOOGLE_CLIENT_ID=DISABLED`) | PASS ([final_step_31.png](file:///C:/Users/Administrator/.gemini/antigravity/brain/bf656ac0-07a6-41f4-9c4a-697e11ff4ce3/browser_evidence/81ac6c0d-8766-4bc0-9ee3-a690639619dc/final_step_31.png)) |
| **Dọn Tiến Trình Sau Kiểm Thử** | Tắt task `:5080`, `:3000` & kiểm tra `Get-NetTCPConnection -LocalPort 5080,3000` | Không còn tiến trình lắng nghe trên cổng `5080` và `3000` | Sạch |

### 7.4. Chưa Kiểm Chứng Trên PostgreSQL (Supabase)
- **Full Playwright E2E Suite (`145` test) trực tiếp trên Supabase:** Không chạy toàn bộ bộ E2E tạo/xóa dữ liệu hàng loạt lên DB Supabase thật để bảo toàn dữ liệu người dùng; mới chạy smoke test trình duyệt trực tiếp cho các luồng trọng yếu (`/login` → `/auth`, `/app`, `/app/courses` + Quick Preview, `/learn` Quick Check, `AppliedSystemsLab`) và đo API trực tiếp.
- **Google OAuth trên Supabase Live:** Trên `http://127.0.0.1:3000`, `appsettings.Development.local.json` cấu hình `Authentication:Google:ClientId` thật nhưng origin `127.0.0.1:3000` chưa nằm trong Authorized JavaScript origins của Google Cloud Console nên vẫn phát sinh 1 thông báo `[GSI_LOGGER]` trên `/auth`.
- **Git Commit / Push:** Chưa commit hay push để bạn rà soát `git status` và `git diff`.

### 7.5. Tối Ưu Hiệu Năng Cloud Supabase, Route `/login` & Dọn Cảnh Báo Build
- **Tối ưu Backend `compact` ([CourseGenerationService.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/CourseGenerationService.cs#L419), [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L1594), [api-client.ts](file:///d:/03_Study/Studxy/frontend/src/lib/api-client.ts#L354)):**
  - Bổ sung tham số `includeChapterLessons` (mặc định `true` giữ tương thích ngược 100%) cho `BuildCourseSummariesBatchAsync`: khi `compact=true` trên `/api/v1/workspace/dashboard?compact=true` và `/api/v1/courses?limit=...&offset=...&compact=true`, bỏ qua tải `ContentAstJson` của 700+ bài học qua WAN, bỏ query `FlashcardTags` lớn và bỏ dựng mảng `chapters[].lessons[]` / `lessonQuizMap` / `completedLessonIds` trong payload danh sách (giữ nguyên đầy đủ mọi số liệu tổng hợp và giữ nguyên chi tiết đầy đủ tại `GET /api/v1/courses/{id}`).
  - Rút gọn query `rawLessons` trên `dashboard?compact=true` để không kéo `Title`, `Slug`, `CurrentVersion`, `UpdatedAt` của ~800 bài học.
- **Route `/login` & `/auth` ([login/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/login/page.tsx), [auth/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/auth/page.tsx#L124)):** Thêm server redirect HTTP `307` từ `/login` → `/auth` (giữ nguyên query string) và trì hoãn nạp Google Sign-In SDK cho tới khi `/api/v1/auth/providers` xác nhận `googleEnabled`.
- **Dọn 10 cảnh báo nullability C# (`CS8601`/`CS8604`):** Xử lý tại [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L5821) và [WriteInPracticeContent.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Infrastructure/Database/Seed/WriteInPracticeContent.cs#L1639); `dotnet build -c Release` còn `0 Error(s)` và `0` cảnh báo C#.
- **Đo thực tế Trước → Sau trên Supabase PostgreSQL (0 mock):**

| Chỉ số đo trực tiếp (Supabase) | Trước tối ưu | Sau tối ưu | Mức cải thiện |
|---|---|---|---|
| `GET /api/v1/workspace/dashboard?compact=true` (Cold / Warm) | `10.064 ms` / — | **`5.109 ms` / `47 ms`** | Nhanh hơn **~2×** (cold), **`47 ms`** (warm) |
| Payload `dashboard?compact=true` | `665.482 bytes` | **`48.338 bytes`** | Giảm **92,7%** (**13,8×** nhỏ hơn) |
| `GET /api/v1/courses?limit=8&offset=0&compact=true` (Cold / Warm) | `2.821 ms` / — | **`1.473 ms` / `23 ms`** | Nhanh hơn **1,9×** (cold), **`23 ms`** (warm) |
| Payload `/courses?limit=8&offset=0&compact=true` | `573.675 bytes` | **`22.192 bytes`** | Giảm **96,1%** (**25,8×** nhỏ hơn) |
| `GET /api/v1/courses/{id}` (`document-course-ai-workflows`) | Đầy đủ | **`1.708 ms` · `5` chương · `13` bài** | Giữ nguyên chi tiết đầy đủ |
| Redirect `GET /login?next=%2Fapp%2Fcourses` | HTTP `404` | **HTTP `307` → `/auth?next=%2Fapp%2Fcourses`** | Khắc phục hoàn toàn `404` |
| Backend Build & xUnit (`dotnet build` + `dotnet test -c Release`) | `14` warnings, `130/130` | **`0` C# warnings (`0 Error(s)`), `130/130` pass (`7m 15s`)** | Exit `0` |
| Frontend Unit & Build (`npm run test:unit` & `npm run build`) | `5/5` suites, `20` pages | **`5/5` suites, `21/21` pages + `/login` (`135 B`)** | Exit `0` |
| Live Browser (`browser` subagent `0525b8af`, `visited_origins=http://127.0.0.1:3000`, `blocked_origins=none`) | — | `/login`→`/auth` OK, `/app` hiển thị `4/14` + bài tiếp theo, `/app/courses` `8→14` + Quick Preview `5` chương/`13` bài, **`0` `403`/`500`/`pageerror`** | PASS ([final_step_15.png](file:///C:/Users/Administrator/.gemini/antigravity/brain/bf656ac0-07a6-41f4-9c4a-697e11ff4ce3/browser_evidence/0525b8af-b64a-4ec1-ab62-945c0f9cbd5b/final_step_15.png)) |
