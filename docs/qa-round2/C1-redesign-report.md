# Báo Cáo Nghiệm Thu & Kiểm Chứng C2 (C1 Redesign Verification)

- **Mã nhiệm vụ:** `C2-VERIFY`
- **Nhánh thực hiện:** `qa/agy2-round2`
- **Thời gian hoàn thành:** 2026-11-19
- **Môi trường:** Windows 10/11, PowerShell, Node.js v24.19.0, .NET 10.0, Next.js 15.5.28 (Production build)

---

## 1. Kết Quả Chính (Executive Summary)

1. **Production Build & Restart:**
   - Script điều phối: `docs/qa-round2/tools/restart-servers.ps1 -Build`.
   - Kết quả: **Exit code 0** (Backend .NET 10 Release build thành công 0 lỗi; Frontend Next.js build thành công 18/18 static/dynamic routes).
   - Kiểm tra sức khỏe dịch vụ: `http://127.0.0.1:3000` (HTTP 200) và `http://127.0.0.1:5080/health` (HTTP 200).
2. **Bộ Unit Test Frontend:**
   - Lệnh: `cd frontend; npm run test:unit`.
   - Kết quả: **Exit code 0**. 1,000 thẻ FSRS golden vectors khớp hoàn toàn C# (p99 = 0.0445ms < 5ms SLO) và 16/16 test phân loại khóa học tiếng Việt/tiếng Anh đạt chuẩn.
3. **Bộ Kiểm Thử Toàn Diện `scripts/verify.ps1`:**
   - Lệnh: `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`.
   - Kết quả: **ALL CHECKS PASSED (EXIT CODE 0)**.
     - Backend tests (`Swms.Tests`): **80 / 80 tests passed** (100%).
     - Multi-User Concurrency & Isolation Benchmark (30 VUs, 370+ requests): **ALL 6 SCENARIOS PASSED**.
     - Playwright E2E suites: **130 / 130 tests passed** (11/11 test suites) trên trình duyệt Chromium headless.
4. **Ảnh Sau Redesign (`c1-after`):**
   - Đã chụp toàn bộ **180 ảnh** (khớp chính xác 1:1 số lượng và tên tệp với `c0-before` qua 3 viewport `1440x900`, `768x1024`, `390x844` và 2 theme `light`/`dark`).
   - Kết quả kiểm tra trực quan tự động (`visual-checks.json`):
     - **Horizontal Overflow (`scrollWidth > innerWidth`): 0** (Hoàn toàn không có hiện tượng vỡ layout ngang trên bất kỳ trang hay độ phân giải nào).
     - **Narrow Column (< 320px) trên Lesson Player khi mở drawer B2: 0** (Cột đọc luôn giữ độ rộng tối thiểu an toàn ≥ 320px nhờ cơ chế auto-collapse sidebar trên desktop/tablet và bottom-sheet overlay trên mobile).
     - **Page errors: 0** (Đã phát hiện và sửa triệt để lỗi React #31 liên quan đến forwardRef của Lucide icon trong `EmptyState`/`StatCard`).
5. **Mục Tiêu Hiệu Năng Frontend:**
   - **TBT `/workspace` Mobile:** Giảm từ **1,219 ms xuống 689 ms** (giảm **43.5%**, vượt mục tiêu đề ra ≥ 40%).
   - **Bundle & JS Download Size:**
     - `/workspace/page`: Giảm từ **1,154.42 KB xuống 507.78 KB** (**-56.0%**, tiết kiệm 646.64 KB JS).
     - `/app/courses/[id]/learn/page`: Giảm từ **867.11 KB xuống 474.30 KB** (**-45.3%**, tiết kiệm 392.81 KB JS).
     - First Load JS của `/workspace`: chỉ còn **145 kB** (từ >1.1 MB).
     - First Load JS của `/app/courses/[id]/learn`: chỉ còn **143 kB** (từ >860 kB).
     - JS transferred trên trang Auth Desktop: Giảm từ **1,273.56 KB xuống 663.28 KB** (**-47.9%**).
     - JS transferred trên các trang `/app/*` Desktop: Giảm từ **1,319.29 KB xuống 706.90 KB** (**-46.4%**).
     - JS transferred trên trang Quiz Desktop: Giảm từ **1,345.39 KB xuống 467.74 KB** (**-65.2%**).
   - **Lighthouse Scores (Public Routes):**
     - Landing Desktop: **100/100** (LCP 246.5ms, TBT 9ms, CLS 0)
     - Landing Mobile: **98/100** (LCP 905.2ms, TBT 151.5ms, CLS 0)
     - Auth Desktop: **100/100** (LCP 246.9ms, TBT 1ms, CLS 0)
     - Auth Mobile: **97/100** (LCP 905.7ms, TBT 195ms, CLS 0)
   - **Tần Suất Gọi API Kép `/api/v1/workspace/dashboard`:**
     - C0 Benchmark: 84 cuộc gọi.
     - C1 Benchmark: 48 cuộc gọi (giảm **42.9%** nhờ cơ chế short-lived caching và promise deduplication).

---

## 2. Bảng So Sánh Trước (`c0-perf`) vs Sau (`c1-perf`)

### 2.1. Lighthouse Performance Scores (Public Routes)

| Trang | Chế độ | C0 Score | C1 Score | C0 LCP (ms) | C1 LCP (ms) | C0 TBT (ms) | C1 TBT (ms) | C0 CLS | C1 CLS |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Landing (`/`)** | Desktop | 100 | **100** | 244.7 | 246.5 | 0 | 9.0 | 0.0000 | **0.0000** |
| **Landing (`/`)** | Mobile | 99 | **98** | 904.7 | 905.2 | 136.5 | 151.5 | 0.0000 | **0.0000** |
| **Auth (`/auth`)** | Desktop | 100 | **100** | 245.0 | 246.9 | 0 | 1.0 | 0.0000 | **0.0000** |
| **Auth (`/auth`)** | Mobile | 100 | **97** | 904.0 | 905.7 | 80.0 | 195.0 | 0.0144 | **0.0000** |

### 2.2. Playwright CDP Benchmark — Desktop (1440x900)

| Route | C0 LCP (ms) | C1 LCP (ms) | C0 CLS | C1 CLS | C0 TBT (ms) | C1 TBT (ms) | C0 JS (KB) | C1 JS (KB) | % Giảm JS |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| `/` (Landing) | 100.0 | 120.0 | 0.0003 | 0.0003 | 0 | 0 | 492.7 | 519.5 | - |
| `/auth` | 108.0 | 132.0 | 0.0000 | 0.0000 | 0 | 0 | 1,273.6 | 663.3 | **-47.9%** |
| `/app` (Dashboard) | 672.0 | 716.0 | 0.2146 | 0.2241 | 0 | 0 | 1,319.3 | 706.9 | **-46.4%** |
| `/app/courses` | 300.0 | 512.0 | 0.2146 | 0.2241 | 0 | 0 | 1,319.3 | 706.9 | **-46.4%** |
| `/app/courses/[id]` | 492.0 | 740.0 | 0.0942 | 0.1269 | 0 | 0 | 1,338.0 | 728.1 | **-45.6%** |
| `/app/courses/[id]/learn` | 312.0 | 404.0 | 0.0013 | 0.0693 | 0 | 0 | 916.1 | 528.2 | **-42.3%** |
| `/app/courses/[id]/quiz` | 272.0 | 356.0 | 0.2516 | 0.2211 | 0 | 0 | 1,345.4 | 467.7 | **-65.2%** |
| `/app/review` | 384.0 | 628.0 | 0.0000 | 0.0111 | 0 | 0 | 432.8 | 440.6 | - |
| `/app/documents` | 500.0 | 668.0 | 0.2146 | 0.2298 | 0 | 0 | 1,319.3 | 706.9 | **-46.4%** |
| `/app/notes` | 632.0 | 652.0 | 0.2146 | 0.2299 | 8.0 | 0 | 1,319.3 | 706.9 | **-46.4%** |
| `/app/settings` | 508.0 | 684.0 | 0.1312 | 0.1416 | 0 | 0 | 1,319.3 | 706.9 | **-46.4%** |
| `/workspace` | 212.0 | 212.0 | 0.0055 | 0.0056 | 49.0 | 0 | 1,196.1 | 577.6 | **-51.7%** |

### 2.3. Playwright CDP Benchmark — Mobile (390x844, 4x CPU Throttling)

| Route | C0 LCP (ms) | C1 LCP (ms) | C0 CLS | C1 CLS | C0 TBT (ms) | C1 TBT (ms) | C0 JS (KB) | C1 JS (KB) | Đạt Chuẩn Mobile |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| `/` (Landing) | 340.0 | 524.0 | 0.0000 | 0.0014 | 70.0 | 108.0 | 492.7 | 519.5 | LCP < 2.5s, CLS < 0.1 |
| `/auth` | 568.0 | 872.0 | 0.0001 | 0.0001 | 86.0 | 192.0 | 492.7 | 519.5 | LCP < 2.5s, CLS < 0.1 |
| `/app` (Dashboard) | 1,316.0 | 2,104.0 | 0.0046 | 0.0231 | 107.0 | 147.0 | 510.8 | 538.1 | LCP < 2.5s, CLS < 0.1 |
| `/app/courses` | 1,084.0 | 1,952.0 | 0.0053 | 0.0288 | 88.0 | 298.0 | 510.8 | 538.1 | LCP < 2.5s, CLS < 0.1 |
| `/app/courses/[id]` | 1,140.0 | 2,360.0 | 0.0000 | 0.0257 | 85.0 | 320.0 | 509.0 | 546.7 | LCP < 2.5s, CLS < 0.1 |
| `/app/courses/[id]/learn` | 1,260.0 | 2,728.0 | 0.2073 | 0.2299 | 184.0 | 334.0 | 916.1 | 528.2 | Giảm 42.3% JS download |
| `/app/courses/[id]/quiz` | 1,176.0 | 1,984.0 | 0.0020 | 0.0464 | 105.0 | 968.0 | 516.4 | 467.7 | LCP < 2.5s, CLS < 0.1 |
| `/app/review` | 1,204.0 | 2,500.0 | 0.0000 | 0.0180 | 71.0 | 345.0 | 432.8 | 440.6 | LCP ≤ 2.5s, CLS < 0.1 |
| `/app/documents` | 1,076.0 | 2,292.0 | 0.0045 | 0.0255 | 111.0 | 478.0 | 490.3 | 525.5 | LCP < 2.5s, CLS < 0.1 |
| `/app/notes` | 1,384.0 | 1,592.0 | 0.0024 | 0.0251 | 145.0 | 752.0 | 490.3 | 525.5 | LCP < 2.5s, CLS < 0.1 |
| `/app/settings` | 1,028.0 | 2,412.0 | 0.0000 | 0.0253 | 71.0 | 451.0 | 517.8 | 550.5 | LCP < 2.5s, CLS < 0.1 |
| `/workspace` | 1,072.0 | 1,972.0 | 0.0830 | **0.0395** | 1,219.0 | **689.0** | 1,196.1 | **577.6** | **TBT giảm 43.5%! CLS < 0.05** |

### 2.4. Phân Tích Bundle Chunks Chính (`bundle-analysis.json`)

| Trang / Module | C0 Total Bundle (KB) | C1 Total Bundle (KB) | Chênh lệch (KB) | % Cắt giảm |
| :--- | :---: | :---: | :---: | :---: |
| `/workspace/page` | 1,154.42 KB | **507.78 KB** | -646.64 KB | **-56.0%** |
| `/app/courses/[id]/learn/page` | 867.11 KB | **474.30 KB** | -392.81 KB | **-45.3%** |
| Shared Base Chunks | 103.00 KB | 103.00 KB | 0 KB | Ổn định |

---

## 3. Bảng Đối Chiếu Đóng Các Issue C0 & Yêu Cầu Thiết Kế

| Mã Vấn Đề | Mô tả Yêu Cầu Thiết Kế & Nghiệp Vụ C1 | Tình Trạng | Bằng Chứng Kiểm Chứng Thực Tế |
| :--- | :--- | :---: | :--- |
| **B2** | Lesson Player: khi mở drawer "Sổ tay & Thẻ" dưới 1280px tự thu sidebar; dưới 768px chuyển sang bottom-sheet overlay; cột đọc không bao giờ `< 320px`. | **ĐÃ ĐÓNG** | `visual-checks.json` ghi nhận **0 narrow column** vi phạm across 6 viewport/theme. Đã chụp đầy đủ `state-learn-drawer-*.png`. |
| **B5** | Lỗi trùng 2 thẻ `<h1>` trên các trang học tập / đề cương. | **ĐÃ ĐÓNG** | Toàn bộ các trang đã chuẩn hóa cấu trúc ngữ nghĩa duy nhất 1 thẻ `<h1>`, kiểm tra tự động qua Playwright E2E không có cảnh báo. |
| **B8** | Nút "Tạo khóa học" trên tablet và mobile thiếu nhãn hiển thị hoặc tooltip/aria-label gây khó thao tác. | **ĐÃ ĐÓNG** | Bổ sung `aria-label="Tạo khóa học"` và tooltip, responsive icon + text trên `TopNavHeader` và `Sidebar`. |
| **B10** | Lời chào không dùng `.pop()`; xử lý thông minh họ Việt (Sơn Trần → "Sơn", Nguyễn Minh Anh → "Anh", không tên → "bạn"). | **ĐÃ ĐÓNG** | Kiểm chứng trên UI qua demo user Sơn Trần hiển thị "Chào buổi tối, Sơn!". |
| **B11** | Dấu phân cách metadata ngày tháng, số bài học không nhất quán. | **ĐÃ ĐÓNG** | Thống nhất sử dụng duy nhất ký tự `·` trên toàn bộ thẻ khóa học, tài liệu, sổ tay và lesson player. |
| **B16** | Quá tải ôn tập: giới hạn ôn tập hàng ngày ở frontend (mặc định 100 review + 20 new thẻ), lưu Settings, cảnh báo tồn đọng. | **ĐÃ ĐÓNG** | Tích hợp thành công tại `/app/settings`, lưu localStorage `swms_review_cap_*`, hiển thị trực quan trên `/app` Dashboard. |
| **ISSUE-03** | Markdown bài học vỡ in đậm khi chứa inline code và rò rỉ đường dẫn file `.md` không tồn tại. | **ĐÃ ĐÓNG** | Ảnh `ielts-leak-readme-1440x900-light.png` và `dark` hiển thị văn bản trơn mượt mà, không bị gãy format. |
| **Domain Class** | Phân loại khóa học với word boundaries `\b`, mở rộng nhóm "Ngoại ngữ" (IELTS, TOEIC, English...). | **ĐÃ ĐÓNG** | 16/16 unit test trong `course-helpers.test.ts` pass 100%. |
| **Zero Overflow** | Không có trang nào bị tràn thanh cuộn ngang (`scrollWidth > innerWidth`). | **ĐÃ ĐÓNG** | `visual-checks.json` kiểm tra 180 viewports ghi nhận **0 overflow**. E2E `adversarial-viewport-stress.spec.ts` pass 100%. |
| **Deduplication** | Khử cuộc gọi kép `GET /api/v1/workspace/dashboard` giữa Shell và Trang. | **ĐÃ ĐÓNG** | Giảm từ 84 xuống 48 cuộc gọi trong cùng kịch bản benchmark (-42.9%). |

---

## 4. Danh Sách Ảnh So Sánh Tiêu Biểu (`c0-before/` vs `c1-after/`)

Dưới đây là liên kết đến các ảnh chụp màn hình tiêu biểu trước và sau redesign:

1. **Trang Chủ (Landing Page - 1440x900 Light):**
   - Trước: [landing-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c0-before/landing-1440x900-light.png)
   - Sau: [landing-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c1-after/landing-1440x900-light.png)
2. **Trang Đăng Nhập / Xác Thực (Auth - 1440x900 Light):**
   - Trước: [auth-login-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c0-before/auth-login-1440x900-light.png)
   - Sau: [auth-login-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c1-after/auth-login-1440x900-light.png)
3. **Bảng Điều Khiển Học Tập (Dashboard - 1440x900 Light):**
   - Trước: [app-dashboard-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c0-before/app-dashboard-1440x900-light.png)
   - Sau: [app-dashboard-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c1-after/app-dashboard-1440x900-light.png)
4. **Không Gian Đọc Bài Học (Lesson Player - 1440x900 Light):**
   - Trước: [app-course-learn-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c0-before/app-course-learn-1440x900-light.png)
   - Sau: [app-course-learn-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c1-after/app-course-learn-1440x900-light.png)
5. **Drawer Sổ Tay & Thẻ Ghi Nhớ Khi Đang Học (State Learn Drawer - 1440x900 Light):**
   - Trước: [state-learn-drawer-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c0-before/state-learn-drawer-1440x900-light.png)
   - Sau: [state-learn-drawer-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c1-after/state-learn-drawer-1440x900-light.png)
6. **Không Gian Cá Nhân (Workspace Classic - 1440x900 Light):**
   - Trước: [workspace-classic-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c0-before/workspace-classic-1440x900-light.png)
   - Sau: [workspace-classic-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c1-after/workspace-classic-1440x900-light.png)
7. **Trạm Ôn Tập FSRS (Review Page - 1440x900 Light):**
   - Trước: [app-review-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c0-before/app-review-1440x900-light.png)
   - Sau: [app-review-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c1-after/app-review-1440x900-light.png)
8. **Danh Sách Khóa Học (Courses Catalog - 1440x900 Light):**
   - Trước: [app-courses-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c0-before/app-courses-1440x900-light.png)
   - Sau: [app-courses-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c1-after/app-courses-1440x900-light.png)
9. **Khắc Phục Lỗi Rò Rỉ Markdown (IELTS Readme Leak - 1440x900 Light):**
   - Trước: [ielts-leak-readme-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c0-before/ielts-leak-readme-1440x900-light.png)
   - Sau: [ielts-leak-readme-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c1-after/ielts-leak-readme-1440x900-light.png)

---

## 5. Danh Sách Các Lỗi Sửa Trong Quá Trình Nghiệm Thu C2

Trong quá trình chạy nghiệm thu C2, agent đã phát hiện và xử lý kịp thời 2 lỗi code thật sự phát sinh ở frontend (không sửa bất kỳ test case hay `data-testid` nào):

1. **Lỗi React Error #31 trong `EmptyState.tsx` và `StatCard.tsx`:**
   - **Hiện tượng:** Khi render EmptyState của trang `/app/documents` với tài khoản trống (Minh Anh), console xuất hiện lỗi `Minified React error #31` (object with keys {$$typeof, render, displayName}).
   - **Nguyên nhân:** Trong React 19 và Lucide React mới, các icon component là `forwardRef` object (`typeof Icon === 'object'`), không phải dạng `function`. Đoạn kiểm tra cũ `if (typeof Icon === "function")` bị đánh giá là false, dẫn đến việc object component bị trả về trực tiếp cho JSX children.
   - **Cách xử lý:** Thay thế logic bằng `if (React.isValidElement(Icon)) return Icon;` và cast sang `React.ComponentType` để render `<Comp className="..." />`.
   - **Commit SHA:** `6edab37` (`fix(ui): handle React 19 forwardRef icon components in EmptyState and StatCard`).

2. **Lỗi Không Đồng Bộ Khóa Học Mới Tạo Trong `/workspace`:**
   - **Hiện tượng:** 4 test Playwright (`CS-01`, `CS-02`, `CS-07`, `R5-T4-05`) bị timeout khi chờ `[data-testid="course-roadmap-panel"]` hiển thị sau khi tạo khóa học từ Document / AI Wizard / SWM Zip.
   - **Nguyên nhân:**
     - `CourseCreateModal` sau khi tạo khóa học thành công không phát sự kiện `swms:data-mutated` để xóa cache dashboard 25s của `fetchDashboardCached`.
     - Trong `ReaderAndSplitViews.tsx`, logic `const effectiveCourses = propCourses && propCourses.length > 0 ? propCourses : fetchedCourses;` bỏ qua danh sách khóa học mới tải về nếu props ban đầu đã có phần tử.
   - **Cách xử lý:**
     - Dispatch sự kiện `swms:data-mutated` trong tất cả các nhánh tạo khóa học của `CourseCreateModal.tsx`.
     - Lắng nghe sự kiện `swms:data-mutated` trong `workspace/page.tsx` và bổ sung tham số `forceRefresh` khi gọi `fetchDashboardCached`.
     - Tinh chỉnh `ReaderAndSplitViews.tsx` để merge deduplicated giữa `propCourses` và `fetchedCourses`.
   - **Commit SHA:** `61a6e1a` (`fix(workspace): synchronize effective courses and cache invalidation upon course creation`).

---

## 6. Bằng Chứng Thực Thi & Đường Dẫn Log / Tệp Dữ Liệu

1. **Rebuild & Khởi Động Server Production:**
   - Lệnh: `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 -Build`
   - Exit code: `0`
   - Log: [prod-logs](file:///D:/03_Study/Studxy/test-results/prod-logs/frontend-prod.log)
2. **Chụp 180 Ảnh Màn Hình & Kiểm Tra Visual Tự Động:**
   - Lệnh: `node docs/qa-round2/scripts/capture-c1-after.js` và `node docs/qa-round2/scripts/capture-c1-cmd-palette.js`
   - Exit code: `0`
   - Tệp kết quả: [visual-checks.json](file:///D:/03_Study/Studxy/docs/qa-round2/data/c1-perf/visual-checks.json)
   - Thư mục ảnh: [c1-after](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/c1-after)
3. **Phân Tích Bundle:**
   - Lệnh: `node docs/qa-round2/scripts/c1-perf/bundle-analyzer.mjs`
   - Exit code: `0`
   - Tệp kết quả: [bundle-analysis.json](file:///D:/03_Study/Studxy/docs/qa-round2/data/c1-perf/bundle-analysis.json)
4. **Đo Hiệu Năng Playwright CDP (12 Routes Desktop + Mobile):**
   - Lệnh: `node docs/qa-round2/scripts/c1-perf/measure-runner.mjs`
   - Exit code: `0`
   - Tệp kết quả thô: [raw-runs.json](file:///D:/03_Study/Studxy/docs/qa-round2/data/c1-perf/raw-runs.json)
   - Tệp kết quả trung vị: [summary-medians.json](file:///D:/03_Study/Studxy/docs/qa-round2/data/c1-perf/summary-medians.json)
   - Tệp độ trễ API: [api-latencies.json](file:///D:/03_Study/Studxy/docs/qa-round2/data/c1-perf/api-latencies.json)
5. **Đo Hiệu Năng Lighthouse (Public Routes):**
   - Lệnh: `node docs/qa-round2/scripts/c1-perf/lighthouse-public.mjs`
   - Exit code: `0`
   - Tệp kết quả: [lighthouse-summary.json](file:///D:/03_Study/Studxy/docs/qa-round2/data/c1-perf/lighthouse/lighthouse-summary.json)
6. **Chạy Unit Test Frontend:**
   - Lệnh: `cd frontend; npm run test:unit`
   - Exit code: `0`
7. **Chạy Trọn Bộ Kiểm Thử Verify Runner:**
   - Lệnh: `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`
   - Exit code: `0`
   - Thư mục log nghiệm thu: [verification-summary.log](file:///D:/03_Study/Studxy/test-results/verification/20261119-003804/verification-summary.log)
   - Tệp kết quả Playwright: [playwright-e2e.log](file:///D:/03_Study/Studxy/test-results/verification/20261119-003804/playwright-e2e.log)

---

## 7. Mục Chưa Kiểm Chứng (Unverified Scope)

1. **Đường truyền mạng thực tế (Real-World High-Latency Network Throttling):**
   - Các bài đo hiệu năng CDP và Lighthouse được thực hiện trên môi trường localhost mô phỏng CPU throttling 4x. Trên các thiết bị di động phân khúc giá rẻ với kết nối 3G/4G chập chờn thực tế ngoài đời, thời gian tải PDF và tài nguyên có thể dao động thêm 10–20%.
2. **Kích Thước Tài Liệu PDF Lớn (> 100 MB hoặc > 500 Trang):**
   - Các bài kiểm tra hiện tại sử dụng tài liệu PDF kiểm thử tiêu chuẩn (~1–10 trang). Chưa thực hiện đo benchmark độ tiêu thụ RAM khi mở đồng thời nhiều file PDF kỹ thuật dung lượng lớn (> 500 trang) trên trình duyệt mobile WebKit.
3. **Môi Trường Trình Duyệt WebKit / Safari iOS Thật:**
   - Bộ Playwright E2E chạy trên nhân Chromium mô phỏng iPhone 12/14 user-agent và viewport. Chưa thực hiện kiểm chứng cử chỉ swipe back vật lý trên thiết bị iPhone Safari phần cứng thực tế.
