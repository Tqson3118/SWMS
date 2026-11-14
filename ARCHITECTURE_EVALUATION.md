# SWMS v1.0 — Báo cáo Đánh giá Kiến trúc, Kiểm thử Chịu tải Đa người dùng & Nghiệm thu Toàn diện (Architecture Evaluation Report)

> **Hệ thống:** SWMS (Sovereign Study Workspace Management System)  
> **Kiến trúc mục tiêu:** Modular Monolith (`.NET 10 Minimal API` + `EF Core` + `Next.js 15 React 19` + `Zustand` + `PDF.js Dual-Layer Reader` + `FSRS v5 Engine` + `AI Course Generation Engine`)  
> **Trạng thái Nghiệm thu:** **PASSED — 2 lần chạy liên tiếp `scripts/verify.ps1` đạt `EXIT CODE 0` (Double Clean Pass):**
> - **Lần chạy sạch 1 (Pass 1):** `test-results/verification/20261112-021528` (`312.61s` — `51/51` Backend xUnit, `6/6` Multi-User Benchmark Scenarios, `139/139` Playwright E2E Tests, Exit Code `0`)
> - **Lần chạy sạch 2 (Pass 2):** `test-results/verification/20261112-022102` (`304.87s` — `51/51` Backend xUnit, `6/6` Multi-User Benchmark Scenarios, `139/139` Playwright E2E Tests, Exit Code `0`)

---

## 1. Tổng quan Kiến trúc & Đối chiếu Yêu cầu (Executive Architecture Summary)

Hệ thống **SWMS v1.0** được thiết kế theo mô hình **Không gian Học tập Chủ quyền Cá nhân (Sovereign Personal Workspace)** kết hợp 4 chế độ học tập liền mạch trên cùng một trạng thái dữ liệu đồng nhất:
1. **Mode 0 — Bảng điều khiển Bento (Personal Workspace Dashboard)**: Cấu trúc Bento Grid 4 khối rõ ràng (`Hero Continue Learning`, `FSRS Review Station`, `Quick Stats & Quota`, `Document & Notebook Library` hỗ trợ lọc theo `Tất cả / Tài liệu PDF / Bài học / Sổ tay` và tạo sổ tay độc lập), thanh trạng thái Onboarding 3 bước tinh gọn.
2. **Mode 1 — Không gian Đọc sâu Hai lớp (Dual-Layer PDF & Prose Reader)**: Kết xuất đồng thời hoặc chuyển đổi linh hoạt 3 chế độ (`Song song` / `Bản in PDF` Canvas / `Văn bản` trích xuất 68ch), phát hiện trang scan không có lớp văn bản (`Không có lớp văn bản — chưa OCR`), bôi đậm ngữ nghĩa 5 màu (`Phím 1–5`), tạo thẻ Cloze tức thì (`Phím C`) và trợ lý AI giải thích tại chỗ (`Phím E`).
3. **Mode 2 — Không gian Tổng hợp PKM Chia đôi (Split Workspace 15/45/40)**: Đồng bộ cuộn (`Alt+S`), trích dẫn ngược hai chiều (`Jump to Source` kèm hiệu ứng GSAP Glow), chèn nhanh liên kết `[[Wikilink]]` và cú pháp `/flashcard`, cơ chế kiểm soát đồng thời lạc quan **OCC (Optimistic Concurrency Control)** bảo toàn 100% dữ liệu qua chiến lược **`409 CONFLICT_KEEP_BOTH`** (tự động tách nhánh bản sao xung đột `notebook_conflict_drafts`), xuất bản **Obsidian Sovereign Markdown v1.0**, và ngăn kéo **Hàn gắn Chú giải Mồ côi 3 tầng (3-Tier Self-Healing Annotation Reconciliation)**.
4. **Mode 3 — Trạm Ôn tập Chủ động FSRS v5 & Đánh giá Định tính AI (Active Recall & Formative Practice Quiz)**: Thuật toán lặp lại ngắt quãng **FSRS v5** với kiểm tra đồng hồ bất đối xứng phía máy chủ (Asymmetric Server-Authoritative Clock Validation), phát lại theo trình tự thời gian (Chronological Replay) khi đồng bộ ngoại tuyến, hoàn tác lượt ôn tập gần nhất (`Undo` / `Phím Z`), và chấm bài tự luận qua hàng đợi bền vững **Transactional Outbox Background Worker** (`SemaphoreSlim(5)`).
5. **Xác thực Đa người dùng & Cô lập Dữ liệu Multi-Tenant (`/auth`)**: Hỗ trợ đăng ký, đăng nhập, xoay vòng Refresh Token (RTR) kèm phát hiện xâm phạm trong cửa sổ ân hạn (Grace Window), và bộ chuyển đổi nhanh tài khoản Demo 1-click (`Sơn Trần` vs `Minh Anh`) phục vụ kiểm chứng cô lập dữ liệu trực tiếp trên giao diện.

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
| **THEME-01** | **Host OS Dark Theme Emulation Override** (`LAYOUT-03` & `R5-T3-02`) | Máy host Windows chạy Dark Mode (`AppsUseLightTheme: 0`) khiến Playwright nhận `prefers-color-scheme: dark`, inline script tự động thêm class `dark` làm đảo chiều các test kỳ vọng khởi đầu Light Mode. | Bổ sung cấu hình `colorScheme: 'light'` tất định trong `playwright.config.ts` (`.js`) và kiểm tra điều kiện ban đầu trong `R5-T3-02`. | **100% suite chạy trong môi trường ánh sáng chuẩn hóa, `139/139` bài test pass ổn định**. |
| **PERF-01** | **Backend Kestrel Direct Binary Execution & Dev Server Heap** (`scripts/verify.ps1`) | Khởi chạy qua `dotnet run` gián tiếp dễ bị ngắt tín hiệu khi có test concurrency, và Node.js dev server bị cạn heap khi biên dịch 139 bài test liên tục. | Khởi chạy trực tiếp compiled DLL `Swms.Api.dll` và thiết lập `NODE_OPTIONS="--max-old-space-size=4096"`. | **Hệ thống khởi chạy trong 0.2s, ổn định tuyệt đối qua toàn bộ phiên chạy 139 test E2E**. |

---

## 3. Kết quả Đo kiểm Hiệu năng & Chịu tải Đa người dùng (Multi-User Benchmark Metrics)

Kịch bản kiểm thử chịu tải [scripts/benchmark-multiuser.mjs](file:///d:/03_Study/Studxy/scripts/benchmark-multiuser.mjs) mô phỏng **30 người dùng ảo đồng thời (30 Virtual Users)** thực thi tổng cộng **377 HTTP requests** qua 6 kịch bản khắc nghiệt trực tiếp vào máy chủ Release (`http://127.0.0.1:5080`).

### 3.1. Bảng Số liệu Thực nghiệm (Trích xuất từ `multiuser-benchmark.json`)

| Kịch bản (Scenario) | Số Request | Thông lượng (`req/s`) | `Min` (ms) | `p50` (ms) | `p95` (ms) | `p99` (ms) | `Max` (ms) | Chỉ tiêu SLO & Đối chiếu Kết quả | Trạng thái |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :--- | :---: |
| **S1: Concurrent Registration & Duplicate Email Guard** | 60 | **318.69** | 11.48 | **18.82** | **161.94** | **163.01** | 163.01 | `30/30` đăng ký mới thành công; Race trùng email: `1` thắng (`200`) + `29` chặn (`409`) | **PASS** |
| **S2: Multi-Tenant Data Isolation Audit** | 64 | **107.59** | 14.51 | **23.59** | **92.11** | **407.07** | 407.07 | `0` rò rỉ dữ liệu chéo qua 60 mũi tấn công IDOR & liệt kê (`50` phản hồi `403 Forbidden`) | **PASS** |
| **S3: High-Concurrency Notebook Auto-Save & OCC 409 Forking** | 40 | **432.79** | 12.24 | **31.68** | **39.63** | **40.57** | 40.57 | **OCC `p95 = 40.31ms` (< 50ms SLO)**, **`p99 = 40.57ms` (< 150ms SLO)**; `1` thắng (`v2`) + `19` nhánh xung đột (`409`) | **PASS** |
| **S4: FSRS v5 Review Sync Burst, Idempotency & Undo Guard** | 62 | **312.25** | 12.38 | **34.46** | **69.66** | **72.23** | 72.23 | **FSRS `p95 = 69.66ms` (< 200ms SLO)**; 30/30 gói trùng `reviewEventId` bất biến (`reps=1`); Undo khôi phục `reps=0` | **PASS** |
| **S5: PDF Range Streaming (HTTP 206) & Vietnamese Hybrid Search** | 90 | **1203.47** | 61.90 | **66.78** | **71.57** | **72.33** | 72.33 | `30/30` luồng trả `HTTP 206 Partial Content`; `30/30` truy vấn không dấu `"bo nho ao"` khớp `"Bộ nhớ ảo"` | **PASS** |
| **S6: AI Daily Credit Quota Ceiling Burst Guard** | 61 | **9.75** | 6.20 | **2059.84** | **2133.31** | **2134.80** | 2134.80 | `50` yêu cầu hợp lệ (`200 OK`) + `10` yêu cầu bị chặn (`429 Too Many Requests`); sổ cái `50/50` credits (`0` vượt trần) | **PASS** |
| **Tổng hợp Toàn cục (Overall Workload)** | **377** | **50.66** | **6.20** | **63.16** | **2123.26** | **2133.31** | **2134.80** | Phân phối mã HTTP: `200: 239`, `206: 30`, `403: 50`, `409: 48`, `429: 10` (`0` lỗi `5xx`) | **PASS** |

> [!NOTE]
> Trong kịch bản **S6**, độ trễ `p50 ≈ 2059ms` phản ánh thời gian chờ kết nối TCP (fallback an toàn của `GeminiProxyClient` khi cổng proxy LLM cục bộ `127.0.0.1:8045` không bật), trong khi các request vượt hạn mức thứ 51–60 bị từ chối tức thì ở mức **`6.20ms`** với mã `429 Too Many Requests`.

---

## 4. Kết quả Kiểm thử Backend (xUnit) & Giao diện Đầu-Cuối (Playwright E2E)

### 4.1. Kiểm thử Đơn vị & Tích hợp Backend (`Swms.Tests`)
- **Lệnh thực thi:** `dotnet test backend/tests/Swms.Tests/Swms.Tests.csproj -c Release --no-build`
- **Kết quả:** **`Passed! - Failed: 0, Passed: 51, Skipped: 0, Total: 51`** (Exit code: `0`)
  - 37 bài kiểm thử cốt lõi (PDF Extraction 298 trang, FSRS v5 Mathematical Engine, 3-Tier Annotation Self-Healing, Refresh Token Rotation & Grace Window Recovery, Hybrid Unaccent Search).
  - 7 bài kiểm thử đa luồng & cô lập dữ liệu Phase 1 trong [MultiUserConcurrencyAndIsolationTests.cs](file:///d:/03_Study/Studxy/backend/tests/Swms.Tests/MultiUserConcurrencyAndIsolationTests.cs).
  - 2 bài kiểm thử sinh Khóa học tự động Phase 2 trong [CourseGenerationTests.cs](file:///d:/03_Study/Studxy/backend/tests/Swms.Tests/CourseGenerationTests.cs).
  - 5 bài kiểm thử phòng tạo khóa học chuyên sâu trong [CourseCreatorStudioTests.cs](file:///d:/03_Study/Studxy/backend/tests/Swms.Tests/CourseCreatorStudioTests.cs).

### 4.2. Bộ Kiểm thử Giao diện Đầu-Cuối Playwright (9 Suites — 139 Tests)
- **Lệnh thực thi:** `npx.cmd playwright test e2e/pdf-multipage-e2e.spec.ts e2e/qa-e2e-suite.spec.ts e2e/swms-landing.spec.ts e2e/swms-workspace.spec.ts e2e/adversarial-viewport-stress.spec.ts e2e/challenger-m1-stress.spec.ts e2e/challenger-m2-stress.spec.ts e2e/swms-challenger-m1-2.spec.ts e2e/r5-visual-interactive-e2e.spec.ts --reporter=list`
- **Kết quả:** **`139 passed, 0 failed`** (Exit code: `0`)
  - `e2e/pdf-multipage-e2e.spec.ts`: Kiểm thử chuyên sâu tải lên, nhảy trang, trích xuất văn bản và tạo thẻ trên giáo trình PDF 298 trang thực tế (`Giáo trình Pháp luật đại cương_ebook.pdf`).
  - `e2e/qa-e2e-suite.spec.ts`: 17 luồng người dùng toàn diện & kiểm chứng chống hồi quy lỗi UI/UX.
  - `e2e/swms-landing.spec.ts` & `e2e/swms-workspace.spec.ts`: Kiểm thử đa độ phân giải (`1920x1080`, `1536x864`, `1280x720`) và vòng lặp học tập cốt lõi.
  - `e2e/adversarial-viewport-stress.spec.ts`, `e2e/challenger-m1-stress.spec.ts`, `e2e/challenger-m2-stress.spec.ts`, `e2e/swms-challenger-m1-2.spec.ts`: Kiểm thử đối kháng trên 11 kích thước màn hình (`360x640` đến `2560x1440`), kiểm chứng không tràn ngang (`Zero Horizontal Overflow`), chống FOUC khi đổi giao diện Sáng/Tối liên tục 25 lần.
  - `e2e/r5-visual-interactive-e2e.spec.ts` (**50 bài kiểm thử R5 Visual & Interactive**):
    - **Tier 1 (20 tests)**: Bao phủ đầy đủ R1 (Design System & Navigation), R2 (Bento Grid 4 khối & Ingestion), R3 (Dual-Layer PDF Reader & Split Workspace), R4 (FSRS 3D Flip Card & AI Essay Rubric).
    - **Tier 2 (20 tests)**: Kiểm thử biên độ (Desktop/Tablet Portrait/Landscape, kích thước điểm chạm `>= 36–44px`, độ tương phản màu chuẩn WCAG AA `>= 4.5:1`, từ chối tệp PDF hỏng/mã hóa mật khẩu/vượt dung lượng 50MB, kẹp biên số trang âm/vượt tổng trang).
    - **Tier 3 (5 tests)**: Tương tác liên tính năng (Luồng đọc sách 298 trang -> trang 200 -> bôi đậm -> tạo thẻ -> ôn tập -> nhảy ngược về trang 200; đồng bộ giao diện tối qua 4 chế độ; xuất Markdown; thay đổi kích thước khung nhìn động; hàng đợi ôn tập ngoại tuyến).
    - **Tier 4 (5 tests)**: 5 hành trình học viên thực tế:
      - Sinh viên Luật chinh phục giáo trình 298 trang.
      - Phiên học tài liệu Scan đa phương thức.
      - Thao tác hoàn toàn bằng bàn phím cho Power User.
      - Phục hồi sau giới hạn tốc độ & gián đoạn mạng.
      - **Scenario 4.5 (R5-T4-05)**: Nạp tài liệu tự động sinh trọn bộ Khóa học AI (Lộ trình Chương mục, Bài giảng, Flashcard & Đề luyện tập Quiz).

---

## 5. Hướng dẫn Tự nghiệm thu 5 phút (5-Minute Self-Verification Guide)

### Cách 1: Chạy Lệnh Kiểm chứng Tự động Toàn diện (One-Command Runner)
Mở PowerShell tại thư mục gốc `d:\03_Study\Studxy` và chạy:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify.ps1
```
Script sẽ tự động:
1. Dọn dẹp các tiến trình còn sót trên cổng `5080` và `3000`.
2. Biên dịch và khởi chạy Backend API (`http://127.0.0.1:5080`) + Frontend Next.js (`http://127.0.0.1:3000`).
3. Thực thi **51 bài test xUnit Backend** (`Step G`).
4. Thực thi **Benchmark Đa người dùng 30 VUs / 377 requests** (`Step G2`).
5. Thực thi **139 bài test Playwright E2E trên 9 bộ test suites** (`Step H`).
6. Thu thập toàn bộ biên bản vào `test-results/verification/<timestamp>/` và giải phóng sạch cổng mạng.

### Cách 2: Trải nghiệm & Nghiệm thu Trực tiếp trên Trình duyệt
1. Khởi chạy Backend và Frontend, sau đó mở trình duyệt tại `http://127.0.0.1:3000/workspace`.
2. **Kiểm tra Bảng điều khiển Bento & Bộ lọc Thư viện (Mode 0)**:
   - Quan sát 4 khối Bento rõ ràng và thanh trạng thái Onboarding 3 bước nhỏ gọn.
   - Nhấn các tab lọc `Tất cả`, `Tài liệu PDF`, `Bài học`, `Sổ tay` trong khối Thư viện hoặc nhấn `+ Tạo ghi chú mới` để tạo sổ tay độc lập.
3. **Kiểm tra Chuyển đổi 3 Chế độ Hiển thị PDF & Phím tắt Đọc sâu (Mode 1)**:
   - Nhấn phím `1` (hoặc tab `Đọc sâu`) để vào trình đọc.
   - Tải lên một tệp PDF nhiều trang và thử chuyển đổi thanh chọn 3 chế độ trên thanh công cụ PDF: **`Song song`** / **`Bản in PDF`** / **`Văn bản`**.
   - Dùng phím `J` / `K` để cuộn trang, bôi đen một đoạn văn bản và nhấn phím `1`–`5` để tô sáng ngữ nghĩa 5 màu, nhấn `C` để mở hộp thoại tạo thẻ Cloze, hoặc nhấn `Z` để bật/tắt chế độ Đọc tập trung (Zen Mode).
4. **Kiểm tra Không gian Ghi chú Chia đôi & Chèn nhanh (Mode 2)**:
   - Nhấn `Alt+2` (hoặc tab `Ghi chú & Tổng hợp`).
   - Nhấn các nút gợi ý `+ [[Chèn liên kết Wikilink]]` hoặc `+ /flashcard` ngay phía trên khung soạn thảo và nhấn `Ctrl+S` để lưu.
   - Nhấn `Mô phỏng xung đột 409` để quan sát cơ chế tự động tách nhánh **Keep Both** bảo toàn dữ liệu.
5. **Kiểm tra Hoàn tác Ôn tập FSRS & Phím tắt Trắc nghiệm (Mode 3)**:
   - Nhấn `Alt+3` (hoặc tab `Ôn tập`). Nhấn `Space` để lật thẻ 3D, nhấn `3` (Tốt) để chấm thẻ, sau đó nhấn phím **`Z`** (hoặc nút **`Hoàn tác (Z)`**) để hoàn tác lượt ôn tập vừa thực hiện.
   - Chuyển sang tab `Đánh giá thực hành (Quiz)`, nhấn phím `A`/`B`/`C`/`D` để chọn nhanh đáp án trắc nghiệm và nhấn `Ctrl+Enter` trong ô tự luận để gửi chấm điểm AI theo Rubric.
6. **Kiểm tra Chuyển đổi Tài khoản Đa người dùng & Cô lập Dữ liệu (`/auth`)**:
   - Nhấp vào biểu tượng Hồ sơ góc phải trên cùng -> chọn **`Chuyển đổi tài khoản (/auth)`**.
   - Trong khung **Chuyển nhanh tài khoản Demo**, chọn **`Minh Anh (Tài khoản Độc lập - Kiểm thử Multi-Tenant)`**: hệ thống tự động đăng ký/đăng nhập và mở ra không gian làm việc riêng biệt hoàn toàn với tài khoản mặc định **`Sơn Trần`**.

---

## 6. Mục Chưa Kiểm Chứng & Giới hạn Hiện tại (Unverified Scope & Known Limitations)

- **Cơ sở dữ liệu vật lý trong kiểm thử cục bộ:** Trong môi trường chạy tự động không cần Docker (`UseInMemoryOrFallback`), EF Core sử dụng InMemory/Local provider kết hợp các khóa `SemaphoreSlim` phân mảnh trong tiến trình (`In-Process Fine-Grained Locks`). Khi triển khai cụm nhiều instance Backend (Multi-Node Horizontal Scaling) kết nối PostgreSQL vật lý, các khóa cấp ứng dụng cần được bổ trợ bởi ràng buộc `UNIQUE INDEX` và `SELECT ... FOR UPDATE` / `SERIALIZABLE` ở tầng PostgreSQL.
- **Kết nối LLM Thượng nguồn:** Khi proxy `http://127.0.0.1:8045` (`gemini-3.8-flash`) không hoạt động, `GeminiProxyClient` tự động chuyển sang phản hồi học thuật tiếng Việt tất định sau khi hết thời gian chờ kết nối ngắn để bảo đảm các bài kiểm thử ngoại tuyến luôn ổn định.
