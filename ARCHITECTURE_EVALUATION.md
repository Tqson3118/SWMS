# Studxy (SWMS) — Báo cáo Đánh giá Kiến trúc, Kiểm thử Chịu tải Đa người dùng & Nghiệm thu Sẵn sàng Triển khai (Architecture & Release Evaluation Report)

> **Hệ thống:** Studxy / SWMS (Sovereign Study Workspace Management System)  
> **Kiến trúc mục tiêu:** Modular Monolith (`.NET 10 Minimal API` + `EF Core` + `SQLite WAL / PostgreSQL 17` + `Next.js 15 React 19` + `Zustand` + `PDF.js Dual-Layer Reader` + `FSRS v5 Engine` + `AI Course Generation Engine` + `V2 Top-Down 4-Level Sitemap`)  
> **Trạng thái Nghiệm thu:** **PASSED — 2 lần chạy liên tiếp `scripts/verify.ps1` đạt `EXIT CODE 0` (Double Clean Pass):**
> - **Lần chạy sạch 1 (Pass 1):** `test-results/verification/20261116-155213` (`285.95s` — `68/68` Backend xUnit, `6/6` Multi-User Benchmark Scenarios / `377` requests, `129/129` Playwright E2E Tests trên 11 suites, Exit Code `0`)
> - **Lần chạy sạch 2 (Pass 2):** `test-results/verification/20261116-155713` (`301.18s` — `68/68` Backend xUnit, `6/6` Multi-User Benchmark Scenarios / `377` requests, `129/129` Playwright E2E Tests trên 11 suites, Exit Code `0`)

---

## 1. Tổng quan Kiến trúc & Đối chiếu Yêu cầu (Executive Architecture Summary)

Hệ thống **SWMS v1.0** được thiết kế theo mô hình **Không gian Học tập Chủ quyền Cá nhân (Sovereign Personal Workspace)** kết hợp 4 chế độ học tập liền mạch trên cùng một trạng thái dữ liệu đồng nhất:
1. **Mode 0 — Bảng điều khiển Bento (Personal Workspace Dashboard)**: Cấu trúc Bento Grid 4 khối rõ ràng (`Hero Continue Learning`, `FSRS Review Station`, `Quick Stats & Quota`, `Document & Notebook Library` hỗ trợ lọc theo `Tất cả / Tài liệu PDF / Bài học / Sổ tay` và tạo sổ tay độc lập), thanh trạng thái Onboarding 3 bước tinh gọn.
2. **Mode 1 — Không gian Đọc sâu Hai lớp (Dual-Layer PDF & Prose Reader)**: Kết xuất đồng thời hoặc chuyển đổi linh hoạt 3 chế độ (`Song song` / `Bản in PDF` Canvas / `Văn bản` trích xuất 68ch), phát hiện trang scan không có lớp văn bản (`Không có lớp văn bản — chưa OCR`), bôi đậm ngữ nghĩa 5 màu (`Phím 1–5`), tạo thẻ Cloze tức thì (`Phím C`) và trợ lý AI giải thích tại chỗ (`Phím E`).
3. **Mode 2 — Không gian Tổng hợp PKM Chia đôi (Split Workspace 15/45/40)**: Đồng bộ cuộn (`Alt+S`), trích dẫn ngược hai chiều (`Jump to Source` kèm hiệu ứng GSAP Glow), chèn nhanh liên kết `[[Wikilink]]` và cú pháp `/flashcard`, cơ chế kiểm soát đồng thời lạc quan **OCC (Optimistic Concurrency Control)** bảo toàn 100% dữ liệu qua chiến lược **`409 CONFLICT_KEEP_BOTH`** (tự động tách nhánh bản sao xung đột `notebook_conflict_drafts`), xuất bản **Obsidian Sovereign Markdown v1.0**, và ngăn kéo **Hàn gắn Chú giải Mồ côi 3 tầng (3-Tier Self-Healing Annotation Reconciliation)**.
4. **Mode 3 — Trạm Ôn tập Chủ động FSRS v5 & Đánh giá Định tính AI (Active Recall & Formative Practice Quiz)**: Thuật toán lặp lại ngắt quãng **FSRS v5** với kiểm tra đồng hồ bất đối xứng phía máy chủ (Asymmetric Server-Authoritative Clock Validation), phát lại theo trình tự thời gian (Chronological Replay) khi đồng bộ ngoại tuyến, hoàn tác lượt ôn tập gần nhất (`Undo` / `Phím Z`), và chấm bài tự luận qua hàng đợi bền vững **Transactional Outbox Background Worker** (`SemaphoreSlim(5)`).
5. **Xác thực Đa người dùng & Cô lập Dữ liệu Multi-Tenant (`/auth`)**: Hỗ trợ đăng ký, đăng nhập, xoay vòng Refresh Token (RTR) kèm phát hiện xâm phạm trong cửa sổ ân hạn (Grace Window), và bộ chuyển đổi nhanh tài khoản Demo 1-click (`Sơn Trần` vs `Minh Anh`) phục vụ kiểm chứng cô lập dữ liệu trực tiếp trên giao diện.
6. **Kiến trúc V2 Top-Down 4-Level Sitemap & Tương thích Kép Classic `/workspace`**: Trang chủ `LandingPageV2` (`/`) với Modal focus trap + body scroll lock, hệ điều hướng App V2 (`/app`, `/app/courses`, `/app/courses/new`, `/app/courses/[id]`, `/app/courses/[id]/learn`, `/app/courses/[id]/quiz`, `/app/documents`, `/app/notes`, `/app/library`, `/app/settings`) đồng bộ với không gian Classic `/workspace` qua Sidebar và `CommandPaletteModal`.
7. **Sẵn sàng Đóng gói & Triển khai Production**: Cấu hình `BACKEND_INTERNAL_URL` / `CORS_ALLOWED_ORIGINS` (`frontend/next.config.js`, `backend/src/Swms.Api/Program.cs`), bổ sung endpoint proxy `/health`, `backend/Dockerfile`, `frontend/Dockerfile`, `docker-compose.prod.yml`, `.env.example`, `scripts/start-prod.ps1`, và tài liệu [DEPLOY.md](file:///d:/03_Study/Studxy/DEPLOY.md).

---

## 2. Phát hiện & Khắc phục Lỗi Đồng thời / Cô lập Dữ liệu (Red-before-Green Audit)

Trước khi khóa kiến trúc, nhóm phát triển đã xây dựng bộ kiểm thử đối kháng đa luồng [MultiUserConcurrencyAndIsolationTests.cs](file:///d:/03_Study/Studxy/backend/tests/Swms.Tests/MultiUserConcurrencyAndIsolationTests.cs) và kịch bản tải [benchmark-multiuser.mjs](file:///d:/03_Study/Studxy/scripts/benchmark-multiuser.mjs) để tái hiện các lỗi điều kiện tranh chấp (Race Conditions) và rò rỉ dữ liệu chéo (Cross-Tenant Data Leaks) theo quy trình **Red-before-Green**:

| Mã lỗi | Hạng mục Kiểm thử | Hiện tượng ở pha RED (Trước khi sửa) | Giải pháp Kiến trúc trong `SwmsEndpoints.cs` | Kết quả ở pha GREEN (Sau khi sửa) |
| :--- | :--- | :--- | :--- | :--- |
| **CONC-01** | **Cross-Tenant IDOR / Data Leak** (`Test1_MultiTenantIsolation`) | Người dùng B có thể đọc/ghi đè sổ tay, xuất Markdown và tải tệp PDF riêng tư của Người dùng A qua UUID trực tiếp (`HTTP 200 OK`). | Áp dụng hàm kiểm tra quyền sở hữu `IsOwnerOrDefaultSeededAccess` trên toàn bộ các điểm cuối `/documents/{id}`, `/documents/{id}/file`, `/documents/{id}/pages`, `/notebooks/{id}`, `/notebooks/{id}/export-markdown`, `/practice-quizzes/{id}`, trả về `403 Forbidden` khi khác chủ sở hữu. | **100% Cô lập (`0` rò rỉ trên 60 mũi thăm dò chéo)**. |
| **CONC-02** | **Notebook OCC Lost Update Race** (`Test2_ConcurrentNotebookOcc`) | 15–20 luồng `PUT /api/v1/notebooks/{id}` gửi đồng thời cùng `BaseVersion = 1` đều đọc `nb.Version == 1` trước khi commit, dẫn đến cả 15–20 luồng đều trả `200 OK` và ghi đè mất dữ liệu của nhau. | Bổ sung khóa phân mảnh theo tài nguyên `NotebookOccLocks` (`ConcurrentDictionary<Guid, SemaphoreSlim>`), tuần tự hóa nguyên tử bước kiểm tra `BaseVersion == nb.Version` và lưu DB. | **Chính xác `1` luồng thắng (`200 OK`, lên `v2`) và `19` luồng tách nhánh an toàn (`409 CONFLICT_KEEP_BOTH`)**. |
| **CONC-03** | **Notebook Auto-Provision Duplicate Race** (`Test3_ConcurrentNotebookAutoProvisioning`) | 10–20 luồng gọi `POST /api/v1/notebooks/by-source` cùng lúc cho một `(userId, documentId)` tạo ra 10–20 sổ tay chính trùng lặp. | Bổ sung khóa `NotebookAutoProvisionLocks` theo tổ hợp khóa `(userId, lessonId, documentId)`. | **Hội tụ 100% về duy nhất `1` `notebook.Id` chuẩn**. |
| **CONC-04** | **FSRS Review Sync Duplicate Burst** (`Test4_ConcurrentFsrsReviewIdempotency`) | 20–30 luồng gửi trùng một `reviewEventId` song song vượt qua bước kiểm tra `AnyAsync` trước khi `SaveChangesAsync`, làm tăng `Reps` 15–30 lần cho cùng 1 sự kiện. | Bổ sung khóa `FlashcardSyncLocks` theo `userId` kết hợp kiểm tra `db.FlashcardReviews.Local` + DB và bổ sung điểm cuối hoàn tác `POST /api/v1/flashcards/{id}/undo-last-review`. | **30/30 request trả `200 OK`, chỉ tạo `1` bản ghi `FlashcardReview`, `Reps` tăng đúng `1` (hoàn tác về `0`)**. |
| **CONC-05** | **AI Daily Credit Quota Overspend** (`Test5_ConcurrentAiQuotaCeiling`) | 20 luồng gọi `/api/v1/ai/explain` đồng thời khi chỉ còn 5 credit đều đọc `usedCredits = 45 < 50` trước khi LLM trả kết quả, gây vượt trần lên `65/50` credits. | Bổ sung khóa `AiQuotaLocks` theo `userId`, kiểm tra và ghi nhận giữ chỗ `AiUsageLedger` nguyên tử trước khi nhả khóa gọi LLM; hoàn trả credit nếu có ngoại lệ. | **Chính xác `50/50` credits được cấp (`200 OK`), `10` request vượt trần bị chặn ngay lập tức (`429 Too Many Requests`, `0` overspend)**. |
| **CONC-06** | **Duplicate Email Registration Race** (`Test6_ConcurrentRegistrationDuplicateEmail`) | 30 luồng đăng ký cùng một địa chỉ email song song đều vượt qua `Users.AnyAsync`, tạo các tài khoản trùng email. | Bổ sung khóa `RegistrationEmailLocks` theo email đã chuẩn hóa (`normalizedEmail`). | **Chính xác `1` luồng đăng ký thành công (`200 OK`) và `29` luồng bị chặn (`409 Conflict`)**. |
| **FLASH-01** | **Course Auto-Gen Flashcard Priority vs User Highlight Anchor** (`pdf-multipage-e2e.spec.ts` & `R5-T3-01`) | 5 thẻ sinh tự động của khóa học được gán `DueAt = now`, đứng trước thẻ do người dùng vừa bôi đậm tạo thủ công từ highlight trang 200 (`DueAt = now + vài giây`), khiến nhảy nguồn sai vị trí. | `backend`: đặt `DueAt = now.AddMinutes(5)` cho thẻ sinh tự động khóa học; `frontend`: hàm `loadDueQueue` ưu tiên `activeDocumentId` và thẻ có `annotationId != null`. | **100% thẻ highlight trang 200 đứng đầu hàng đợi ôn tập, Jump-to-Source chính xác tuyệt đối**. |
| **THEME-01** | **Host OS Dark Theme Emulation Override** (`LAYOUT-03` & `R5-T3-02`) | Máy host Windows chạy Dark Mode (`AppsUseLightTheme: 0`) khiến Playwright nhận `prefers-color-scheme: dark`, inline script tự động thêm class `dark` làm đảo chiều các test kỳ vọng khởi đầu Light Mode. | Bổ sung cấu hình `colorScheme: 'light'` tất định trong `playwright.config.ts` (`.js`) và kiểm tra điều kiện ban đầu trong `R5-T3-02`. | **100% suite chạy trong môi trường ánh sáng chuẩn hóa, `129/129` bài test E2E pass ổn định**. |
| **PERF-01** | **Backend Kestrel Direct Binary Execution & Dev Server Heap** (`scripts/verify.ps1`) | Khởi chạy qua `dotnet run` gián tiếp dễ bị ngắt tín hiệu khi có test concurrency, và Node.js dev server bị cạn heap khi biên dịch hàng trăm lượt render liên tục. | Khởi chạy trực tiếp compiled DLL `Swms.Api.dll` và thiết lập `NODE_OPTIONS="--max-old-space-size=4096"`. | **Hệ thống khởi chạy nhanh và ổn định tuyệt đối qua toàn bộ phiên chạy 129 test E2E**. |
| **SQLITE-01** | **SQLite Write Tail Latency & Public Clone Amplification** (`S4` / `Program.cs` & `SwmsEndpoints.cs`) | Khi chạy đồng thời 30 VUs trên SQLite tệp cục bộ, PRAGMA không gắn theo từng kết nối và bước clone flashcard kích hoạt trùng khiến độ trễ `S4` tăng vọt (`~1887ms`). | Bổ sung `SqlitePragmaConnectionInterceptor` (`WAL`, `synchronous=NORMAL`, `temp_store=MEMORY`, `busy_timeout=5000`, `wal_autocheckpoint=20000`, `cache_size=-64000`), khóa `SqliteFlashcardWriteLock` trong `POST /api/v1/flashcards` & `sync-reviews`, rào `!hasAnyUserCard` ở `EnsureUserPublicDocumentFlashcardsAsync`, và cô lập DB kiểm thử theo `$runDir\swms_verify.db`. | **`S4` đạt `p95 = 68.71ms / p99 = 69.81ms` (Pass 1) và `p95 = 60.31ms / p99 = 62.30ms` (Pass 2) — ổn định dưới ngưỡng `200ms` SLO**. |

---

## 3. Kết quả Đo kiểm Hiệu năng & Chịu tải Đa người dùng (Multi-User Benchmark Metrics)

Kịch bản kiểm thử chịu tải [scripts/benchmark-multiuser.mjs](file:///d:/03_Study/Studxy/scripts/benchmark-multiuser.mjs) mô phỏng **30 người dùng ảo đồng thời (30 Virtual Users)** thực thi tổng cộng **377 HTTP requests** qua 6 kịch bản khắc nghiệt trực tiếp vào máy chủ Release (`http://127.0.0.1:5080`) trên CSDL SQLite WAL cô lập theo phiên kiểm thử.

### 3.1. Bảng Số liệu Thực nghiệm (Trích xuất từ Pass 2 — `20261116-155713/multiuser-benchmark.json`)

| Kịch bản (Scenario) | Số Request | Thông lượng (`req/s`) | `Min` (ms) | `p50` (ms) | `p95` (ms) | `p99` (ms) | `Max` (ms) | Chỉ tiêu SLO & Đối chiếu Kết quả | Trạng thái |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :--- | :---: |
| **S1: Concurrent Registration & Duplicate Email Guard** | 60 | **83.16** | 9.00 | **21.63** | **383.24** | **696.10** | 696.10 | `30/30` đăng ký mới thành công; Race trùng email: `1` thắng (`200`) + `29` chặn (`409`) | **PASS** |
| **S2: Multi-Tenant Data Isolation Audit** | 64 | **39.02** | 13.83 | **19.49** | **978.49** | **991.63** | 991.63 | `0` rò rỉ dữ liệu chéo qua thăm dò IDOR & liệt kê (`50` phản hồi `403 Forbidden`) | **PASS** |
| **S3: High-Concurrency Notebook Auto-Save & OCC 409 Forking** | 40 | **238.27** | 11.97 | **54.92** | **74.69** | **77.20** | 77.20 | **OCC `p95 = 61.05ms`, `p99 = 62.14ms`**; `1` thắng (`v2`) + `19` nhánh xung đột (`409`) | **PASS** |
| **S4: FSRS v5 Review Sync Burst, Idempotency & Undo Guard** | 62 | **233.53** | 27.34 | **51.03** | **60.31** | **62.30** | 62.30 | **FSRS `p95 = 60.31ms` (< 200ms SLO), `p99 = 62.30ms`** (Pass 1: `p95 = 68.71ms, p99 = 69.81ms`); bất biến idempotency & Undo | **PASS** |
| **S5: PDF Range Streaming (HTTP 206) & Vietnamese Hybrid Search** | 90 | **38.42** | 1319.09 | **1391.61** | **2303.03** | **2337.06** | 2337.06 | `30/30` luồng trả `HTTP 206 Partial Content`; `30/30` truy vấn không dấu `"bo nho ao"` khớp `"Bộ nhớ ảo"` | **PASS** |
| **S6: AI Daily Credit Quota Ceiling Burst Guard** | 61 | **6.59** | 15.37 | **3055.74** | **3134.22** | **3136.07** | 3136.07 | `50` yêu cầu hợp lệ (`200 OK`) + `10` yêu cầu vượt hạn mức (`429 Too Many Requests`); `0` overspend | **PASS** |
| **Tổng hợp Toàn cục (Overall Workload)** | **377** | **26.14** | **9.00** | **70.52** | **3103.39** | **3134.22** | **3136.07** | Phân phối mã HTTP: `200: 239`, `206: 30`, `403: 50`, `409: 48`, `429: 10` (`0` lỗi `5xx`) | **PASS** |

> [!NOTE]
> Ở **Pass 1** (`20261116-155213`), tổng thể `377` request đạt `25.17 req/s` (`14980.04ms`, `min = 16.53ms`, `p50 = 87.23ms`, `p95 = 3149.91ms`, `p99 = 3182.09ms`, `max = 3197.19ms`). Ở **Pass 2** (`20261116-155713`), trong kịch bản **S6**, độ trễ `p50 = 3055.74ms` phản ánh thời gian chờ kết nối TCP fallback tất định của `GeminiProxyClient` khi cổng proxy LLM cục bộ `127.0.0.1:8045` không bật, trong khi độ trễ nhỏ nhất của kịch bản đạt `min = 15.37ms`.

---

## 4. Kết quả Kiểm thử Backend (xUnit) & Giao diện Đầu-Cuối (Playwright E2E)

### 4.1. Kiểm thử Đơn vị & Tích hợp Backend (`Swms.Tests`)
- **Lệnh thực thi:** `dotnet test backend/tests/Swms.Tests/Swms.Tests.csproj -c Release --no-build`
- **Kết quả (Pass 1 & Pass 2):** **`Passed! - Failed: 0, Passed: 68, Skipped: 0, Total: 68`** (Exit code: `0`)
  - Bao phủ đầy đủ nghiệp vụ cốt lõi (PDF Extraction 298 trang, FSRS v5 Mathematical Engine, 3-Tier Annotation Self-Healing, Refresh Token Rotation & Grace Window Recovery, Hybrid Unaccent Search), kiểm thử đa luồng & cô lập dữ liệu [MultiUserConcurrencyAndIsolationTests.cs](file:///d:/03_Study/Studxy/backend/tests/Swms.Tests/MultiUserConcurrencyAndIsolationTests.cs), sinh Khóa học tự động [CourseGenerationTests.cs](file:///d:/03_Study/Studxy/backend/tests/Swms.Tests/CourseGenerationTests.cs), và phòng tạo khóa học [CourseCreatorStudioTests.cs](file:///d:/03_Study/Studxy/backend/tests/Swms.Tests/CourseCreatorStudioTests.cs).

### 4.2. Bộ Kiểm thử Giao diện Đầu-Cuối Playwright (11 Suites — 129 Tests)
- **Lệnh thực thi:** `npx.cmd playwright test e2e/pdf-multipage-e2e.spec.ts e2e/qa-e2e-suite.spec.ts e2e/v2-landing-ergonomics.spec.ts e2e/swms-workspace.spec.ts e2e/adversarial-viewport-stress.spec.ts e2e/challenger-m2-stress.spec.ts e2e/r5-visual-interactive-e2e.spec.ts e2e/course-creation-studio.spec.ts e2e/v2-app-completeness.spec.ts e2e/reader-polish-and-roadmap.spec.ts e2e/v2-topdown-sitemap.spec.ts --reporter=list`
- **Kết quả (Pass 1 & Pass 2):** **`129 passed, 0 failed`** (Exit code: `0`)
  - `e2e/v2-landing-ergonomics.spec.ts` (**13 tests**): Kiểm thử trang chủ `LandingPageV2` (`/`) với cấu trúc Section/CTA, Theme toggle, Privacy/Terms modal focus trap, khóa cuộn `body` và không tràn ngang trên đa độ phân giải.
  - `e2e/qa-e2e-suite.spec.ts` (**18 tests**): Kiểm thử luồng người dùng toàn diện trên `LandingPageV2`, `/app/*`, `/workspace` & chống hồi quy UI/UX.
  - `e2e/v2-app-completeness.spec.ts` & `e2e/v2-topdown-sitemap.spec.ts` (**2 tests** trong `v2-topdown-sitemap.spec.ts`): Kiểm thử 4 tầng điều hướng Top-Down Sitemap V2 (`/app`, `/app/courses`, `/app/courses/new`, `/app/courses/[id]`, `/app/courses/[id]/learn`, `/app/courses/[id]/quiz`, `/app/documents`, `/app/notes`, `/app/library`, `/app/settings`), nút tải gói mẫu `.zip`, và liên kết chuyển đổi `/workspace` từ Sidebar & `CommandPaletteModal`.
  - `e2e/reader-polish-and-roadmap.spec.ts` & `e2e/course-creation-studio.spec.ts`: Kiểm thử Dual-Layer PDF Reader, Course Roadmap Sidebar, màn hình hoàn thành bài học và AI Course Creation Studio.
  - `e2e/pdf-multipage-e2e.spec.ts`, `e2e/swms-workspace.spec.ts`, `e2e/adversarial-viewport-stress.spec.ts`, `e2e/challenger-m2-stress.spec.ts`, `e2e/r5-visual-interactive-e2e.spec.ts`: Kiểm thử PDF 298 trang, không gian học tập Classic `/workspace`, Stress đa độ phân giải (`Zero Horizontal Overflow`), Command Palette Cloze Quick Capture, và 50 kịch bản R5 Visual & Interactive.

---

## 5. Hướng dẫn Tự nghiệm thu 5 phút & Khởi chạy Production (5-Minute Self-Verification Guide)

### Cách 1: Chạy Lệnh Kiểm chứng Tự động Toàn diện (One-Command Runner)
Mở PowerShell tại thư mục gốc `d:\03_Study\Studxy` và chạy:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```
Script sẽ tự động:
1. Dọn dẹp các tiến trình còn sót trên cổng `5080` và `3000`.
2. Biên dịch và khởi chạy Backend API (`http://127.0.0.1:5080`) + Frontend Next.js (`http://127.0.0.1:3000`) với CSDL cô lập `$runDir\swms_verify.db`.
3. Thực thi **68 bài test xUnit Backend** (`Step G`).
4. Thực thi **Benchmark Đa người dùng 30 VUs / 377 requests** (`Step G2`).
5. Thực thi **129 bài test Playwright E2E trên 11 bộ test suites** (`Step H`).
6. Thu thập toàn bộ biên bản vào `test-results/verification/<timestamp>/` và giải phóng sạch cổng mạng.

### Cách 2: Khởi chạy Chế độ Production & Mở Trình duyệt (`scripts/start-prod.ps1`)
Để build bản phát hành (.NET Release + Next.js Production Build), khởi động dịch vụ, kiểm tra `/health` và mở thẳng ứng dụng trên trình duyệt:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/start-prod.ps1 -OpenBrowser
```
Xem thêm hướng dẫn đóng gói Docker (`docker-compose.prod.yml`, `backend/Dockerfile`, `frontend/Dockerfile`, `.env.example`) tại [DEPLOY.md](file:///d:/03_Study/Studxy/DEPLOY.md).

### Cách 3: Trải nghiệm & Nghiệm thu Trực tiếp trên Trình duyệt
1. Khởi chạy Backend và Frontend, mở `http://127.0.0.1:3000/` (`LandingPageV2`) hoặc `http://127.0.0.1:3000/app` (V2 Sitemap App Shell) và `/workspace` (Classic Workspace).
2. **Kiểm tra Điều hướng V2 & Tương thích `/workspace`**:
   - Trải nghiệm hệ 4 tầng `/app` -> `/app/courses` -> `/app/courses/[id]` -> `/app/courses/[id]/learn` & `/app/courses/[id]/quiz`, tải template mẫu tại `/app/courses/new`, hoặc dùng nút `switch-to-workspace-link` trên Sidebar / Command Palette (`Ctrl+K`) để chuyển qua lại với `/workspace`.
3. **Kiểm tra Bảng điều khiển Bento & Bộ lọc Thư viện (Mode 0)**:
   - Quan sát 4 khối Bento rõ ràng và thanh trạng thái Onboarding 3 bước nhỏ gọn.
   - Nhấn các tab lọc `Tất cả`, `Tài liệu PDF`, `Bài học`, `Sổ tay` trong khối Thư viện hoặc nhấn `+ Tạo ghi chú mới` để tạo sổ tay độc lập.
4. **Kiểm tra Chuyển đổi 3 Chế độ Hiển thị PDF & Phím tắt Đọc sâu (Mode 1)**:
   - Nhấn phím `1` (hoặc tab `Đọc sâu`) để vào trình đọc.
   - Tải lên một tệp PDF nhiều trang và thử chuyển đổi thanh chọn 3 chế độ trên thanh công cụ PDF: **`Song song`** / **`Bản in PDF`** / **`Văn bản`**.
   - Dùng phím `J` / `K` để cuộn trang, bôi đen một đoạn văn bản và nhấn phím `1`–`5` để tô sáng ngữ nghĩa 5 màu, nhấn `C` để mở hộp thoại tạo thẻ Cloze, hoặc nhấn `Z` để bật/tắt chế độ Đọc tập trung (Zen Mode).
5. **Kiểm tra Không gian Ghi chú Chia đôi & Chèn nhanh (Mode 2)**:
   - Nhấn `Alt+2` (hoặc tab `Ghi chú & Tổng hợp`).
   - Nhấn các nút gợi ý `+ [[Chèn liên kết Wikilink]]` hoặc `+ /flashcard` ngay phía trên khung soạn thảo và nhấn `Ctrl+S` để lưu.
   - Nhấn `Mô phỏng xung đột 409` để quan sát cơ chế tự động tách nhánh **Keep Both** bảo toàn dữ liệu.
6. **Kiểm tra Hoàn tác Ôn tập FSRS & Phím tắt Trắc nghiệm (Mode 3)**:
   - Nhấn `Alt+3` (hoặc tab `Ôn tập`). Nhấn `Space` để lật thẻ 3D, nhấn `3` (Tốt) để chấm thẻ, sau đó nhấn phím **`Z`** (hoặc nút **`Hoàn tác (Z)`**) để hoàn tác lượt ôn tập vừa thực hiện.
   - Chuyển sang tab `Đánh giá thực hành (Quiz)`, nhấn phím `A`/`B`/`C`/`D` để chọn nhanh đáp án trắc nghiệm và nhấn `Ctrl+Enter` trong ô tự luận để gửi chấm điểm AI theo Rubric.
7. **Kiểm tra Chuyển đổi Tài khoản Đa người dùng & Cô lập Dữ liệu (`/auth`)**:
   - Nhấp vào biểu tượng Hồ sơ góc phải trên cùng -> chọn **`Chuyển đổi tài khoản (/auth)`**.
   - Trong khung **Chuyển nhanh tài khoản Demo**, chọn **`Minh Anh (Tài khoản Độc lập - Kiểm thử Multi-Tenant)`**: hệ thống tự động đăng ký/đăng nhập và mở ra không gian làm việc riêng biệt hoàn toàn với tài khoản mặc định **`Sơn Trần`**.

---

## 6. Mục Chưa Kiểm Chứng & Giới hạn Hiện tại (Unverified Scope & Known Limitations)

- **Docker Compose & PostgreSQL 17 Thực tế:** Các tệp cấu hình đóng gói (`backend/Dockerfile`, `frontend/Dockerfile`, `docker-compose.prod.yml`, `.env.example`) đã được tạo và rà soát cú pháp, nhưng **chưa dựng container hay chạy trực tiếp trên Docker daemon với PostgreSQL 17 thật** trong môi trường Windows kiểm thử tự động (chỉ mới xác minh Backend trên `SQLite WAL` và cấu hình EF Core fallback). Khi triển khai cụm nhiều instance Backend (Multi-Node Horizontal Scaling) kết nối PostgreSQL vật lý, các khóa `SemaphoreSlim` cấp tiến trình cần được bổ trợ bởi ràng buộc `UNIQUE INDEX` và `SELECT ... FOR UPDATE` / `SERIALIZABLE` ở tầng PostgreSQL.
- **Suite Legacy `document-courses-e2e.spec.ts` trên `LandingPageV2`:** Bộ kiểm thử E2E kế thừa `document-courses-e2e.spec.ts` (vốn dựa trên luồng CTA của Landing v1 dẫn tới `/workspace`) **không nằm trong 11 suites chạy mặc định của `scripts/verify.ps1`** và chưa chạy lại trên `LandingPageV2` (3 bộ legacy v1-landing-only khác là `swms-landing.spec.ts`, `challenger-m1-stress.spec.ts`, `swms-challenger-m1-2.spec.ts` vẫn được giữ nguyên file, không xóa, và được thay thế trong runner bởi `v2-landing-ergonomics.spec.ts`).
- **Kết nối LLM Thượng nguồn (`http://127.0.0.1:8045`):** Khi proxy LLM cục bộ `http://127.0.0.1:8045` (`gemini-3.8-flash`) không bật trong môi trường kiểm thử, `GeminiProxyClient` tự động chuyển sang phản hồi học thuật tiếng Việt tất định sau thời gian chờ kết nối TCP; phản hồi từ LLM thượng nguồn thật chưa được kiểm chứng trực tiếp trong phiên nghiệm thu tự động.
