# 📊 BÁO CÁO ĐO BASELINE HIỆU NĂNG STUDXY (C0-PERF)
**Mã tác vụ:** C0-PERF  
**Môi trường:** Production (Frontend http://127.0.0.1:3000 — Next.js 15, Backend http://127.0.0.1:5080 — .NET 10, SQLite `swms_local_dev.db`)  
**Tài khoản đo lường:** Demo Sơn Trần (`son.tran@studxy.vn`, ID: `11111111-1111-4111-8111-111111111111`)  
**Thời gian thực hiện:** 18/11/2026 20:45 – 21:00  
**Tình trạng:** Hoàn tất 100% (72 lượt đo Playwright CDP + 12 lượt đo Lighthouse + Bundle Analyzer) — **CHỈ ĐO, KHÔNG SỬA CODE ỨNG DỤNG**.

---

## 1. Kết Quả Chính (Executive Summary)

Đợt kiểm thử hiệu năng baseline C0-PERF đã đo đạc độc lập 12 route cốt lõi của Studxy trên cả hai cấu hình **Desktop (1440x900, CPU 1x)** và **Mobile (390x844, CPU throttle 4x)**. Mỗi trang được chạy 3 lần liên tiếp trong trạng thái xác thực phiên Sơn Trần để tính giá trị Trung vị (Median).

### 🎯 Nhận định tổng quan:
1. **Server & TTFB xuất sắc:** TTFB dao động cực thấp từ **1.3 ms – 4.6 ms** trên toàn bộ các route nhờ server Next.js 15 và .NET 10 chạy local. FCP đạt từ **24 ms – 120 ms**.
2. **Điểm nghẽn CLS (Layout Shift) trên Desktop:** Hầu hết các trang `/app/*` (`/app`, `/app/courses`, `/app/documents`, `/app/notes`, `/app/courses/.../quiz`) đều bị **CLS từ 0.2146 đến 0.2516** (vượt ngưỡng Google Core Web Vitals < 0.1). Nguyên nhân do layout auth gate (`app/layout.tsx`) hiển thị spinner toàn màn hình trước khi render đột ngột nội dung chính mà không dự lưu kích thước layout.
3. **Điểm nghẽn Bundle & TBT trên Mobile:**
   - Trang `/workspace` có bundle nặng **1,154.4 KB** JS (14 chunks), gây ra **TBT lên tới 1,219 ms** (Total Long Tasks 1,569 ms) trên mobile 4x CPU throttle.
   - Trang `/app/courses/<id>/learn` có bundle nặng **867.1 KB** JS (10 chunks), TBT mobile **184 ms** (Total Long Tasks 284 ms), do import tĩnh thư viện `pdfjs-dist` (323.5 KB) ngay ở module root.
4. **API chậm nhất:** `GET /api/v1/workspace/dashboard` bị gọi tới **84 lần** xuyên suốt phiên kiểm thử, thời gian phản hồi trung vị **213.45 ms** (tối đa **477.40 ms**), do backend thực hiện join nhiều bảng và parse JSON AST trong bộ nhớ trên mỗi request. Ngoài ra, giao diện còn gặp lỗi **N+1 API call** khi fetch progress từng khóa học riêng lẻ.

---

## 2. Bảng Số Liệu Median Từng Trang (Playwright + CDP)

*Phương pháp: Playwright Chromium CDP (`Performance.enable`, `PerformanceObserver` cho LCP/CLS/TBT/Longtask, `Performance.getMetrics`, `Network.loadingFinished`). Mỗi route chạy 3 lần, lấy giá trị trung vị.*

### 2.1. Cấu hình Desktop (1440 × 900, CPU throttle 1x)

| Route | Tên màn hình | LCP (ms) | CLS | TBT (ms) | Long Tasks (ms) | FCP (ms) | TTFB (ms) | Nội dung chính sẵn sàng (ms) | Tổng JS (KB) | Số Requests | JS Heap (MB) |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| `/` | Landing V2 | 100.0 | 0.0003 | 0.0 | 0.0 | 100.0 | 1.5 | 134.2 | 492.7 | 19 | 4.44 |
| `/auth` | Đăng nhập / Đăng ký | 108.0 | 0.0000 | 0.0 | 0.0 | 108.0 | 1.4 | 116.6 | 1273.6 | 25 | 5.64 |
| `/app` | Tổng quan Hôm nay | 672.0 | **0.2146** | 0.0 | 0.0 | 36.0 | 1.9 | 130.9 | 1319.3 | 47 | 8.57 |
| `/app/courses` | Danh mục khóa học | 300.0 | **0.2146** | 0.0 | 0.0 | 28.0 | 1.6 | 109.3 | 1319.3 | 51 | 7.88 |
| `/app/courses/25f79a81...` | Chi tiết khóa học | 492.0 | 0.0942 | 0.0 | 0.0 | 28.0 | 4.6 | 123.3 | 1338.0 | 46 | 7.03 |
| `/app/courses/25f79a81.../learn` | Phòng học bài giảng | 312.0 | 0.0013 | 0.0 | 0.0 | 28.0 | 4.0 | 141.4 | 916.1 | 25 | 5.94 |
| `/app/courses/25f79a81.../quiz` | Trắc nghiệm ôn luyện | 272.0 | **0.2516** | 0.0 | 0.0 | 28.0 | 3.7 | 119.2 | 1345.4 | 46 | 7.12 |
| `/app/review` | Ôn tập Flashcard FSRS | 384.0 | 0.0000 | 0.0 | 0.0 | 24.0 | 1.4 | 105.0 | 432.8 | 17 | 7.29 |
| `/app/documents` | Quản lý tài liệu PDF | 500.0 | **0.2146** | 0.0 | 0.0 | 24.0 | 1.4 | 117.7 | 1319.3 | 41 | 7.90 |
| `/app/notes` | Sổ tay ghi chép | 632.0 | **0.2146** | 8.0 | 68.0 | 28.0 | 1.6 | 125.0 | 1319.3 | 40 | 8.42 |
| `/app/settings` | Cài đặt tài khoản | 508.0 | **0.1312** | 0.0 | 0.0 | 28.0 | 1.4 | 117.6 | 1319.3 | 41 | 7.82 |
| `/workspace` | Workspace Classic | 212.0 | 0.0055 | 49.0 | 149.0 | 32.0 | 2.2 | 214.3 | **1196.1** | 65 | 10.73 |

---

### 2.2. Cấu hình Mobile (390 × 844, CPU throttle 4x)

| Route | Tên màn hình | LCP (ms) | CLS | TBT (ms) | Long Tasks (ms) | FCP (ms) | TTFB (ms) | Nội dung chính sẵn sàng (ms) | Tổng JS (KB) | Số Requests | JS Heap (MB) |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| `/` | Landing V2 | 340.0 | 0.0000 | 70.0 | 120.0 | 340.0 | 1.5 | 585.8 | 492.7 | 19 | 4.50 |
| `/auth` | Đăng nhập / Đăng ký | 568.0 | 0.0001 | 86.0 | 136.0 | 568.0 | 1.4 | 613.5 | 492.7 | 18 | 4.50 |
| `/app` | Tổng quan Hôm nay | **1316.0** | 0.0046 | 107.0 | 157.0 | 104.0 | 1.6 | 617.5 | 510.8 | 31 | 7.45 |
| `/app/courses` | Danh mục khóa học | 1084.0 | 0.0053 | 88.0 | 138.0 | 120.0 | 1.5 | 678.3 | 510.8 | 31 | 6.85 |
| `/app/courses/25f79a81...` | Chi tiết khóa học | 1140.0 | 0.0000 | 85.0 | 135.0 | 104.0 | 3.5 | 594.6 | 509.0 | 31 | 5.93 |
| `/app/courses/25f79a81.../learn` | Phòng học bài giảng | **1260.0** | **0.2073** | **184.0** | **284.0** | 108.0 | 3.9 | 675.8 | 916.1 | 25 | 6.42 |
| `/app/courses/25f79a81.../quiz` | Trắc nghiệm ôn luyện | 1176.0 | 0.0020 | 105.0 | 155.0 | 120.0 | 4.4 | 738.3 | 516.4 | 31 | 6.03 |
| `/app/review` | Ôn tập Flashcard FSRS | 1204.0 | 0.0000 | 71.0 | 121.0 | 120.0 | 1.8 | 541.8 | 432.8 | 17 | 7.36 |
| `/app/documents` | Quản lý tài liệu PDF | 1076.0 | 0.0045 | 111.0 | 211.0 | 112.0 | 1.3 | 542.6 | 490.3 | 26 | 6.78 |
| `/app/notes` | Sổ tay ghi chép | **1384.0** | 0.0024 | **145.0** | **245.0** | 112.0 | 1.5 | 609.1 | 490.3 | 25 | 7.22 |
| `/app/settings` | Cài đặt tài khoản | 1028.0 | 0.0000 | 71.0 | 121.0 | 92.0 | 1.4 | 514.2 | 517.8 | 28 | 6.72 |
| `/workspace` | Workspace Classic | 1072.0 | 0.0830 | **1219.0** | **1569.0** | 104.0 | 1.3 | **1145.7** | **1196.1** | 65 | **10.88** |

---

### 2.3. Đối chiếu kiểm thử độc lập qua Google Lighthouse (Trang Public)

*Chạy qua CLI: `npx --yes lighthouse` kết nối Chromium của Playwright, 3 lần chạy/chế độ.*

| Trang | Chế độ | Điểm Hiệu năng | FCP (ms) | LCP (ms) | TBT (ms) | CLS | Speed Index (ms) | File kết quả thô |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **Landing (`/`)** | Desktop | **100** | 244.7 | 244.7 | 0.0 | 0.0000 | 250.0 | [lh-landing-desktop.json](file:///d:/03_Study/Studxy/docs/qa-round2/data/c0-perf/lighthouse/landing-desktop-run1.json) |
| **Landing (`/`)** | Mobile (Simulated) | **99** | 904.7 | 904.7 | 136.5 | 0.0000 | 1100.0 | [lh-landing-mobile.json](file:///d:/03_Study/Studxy/docs/qa-round2/data/c0-perf/lighthouse/landing-mobile-run1.json) |
| **Auth (`/auth`)** | Desktop | **100** | 245.0 | 245.0 | 0.0 | 0.0000 | 260.0 | [lh-auth-desktop.json](file:///d:/03_Study/Studxy/docs/qa-round2/data/c0-perf/lighthouse/auth-desktop-run1.json) |
| **Auth (`/auth`)** | Mobile (Simulated) | **100** | 904.0 | 904.0 | 80.0 | 0.0144 | 1050.0 | [lh-auth-mobile.json](file:///d:/03_Study/Studxy/docs/qa-round2/data/c0-perf/lighthouse/auth-mobile-run1.json) |

---

## 3. Phân Tích Kích Thước Bundle & Thư Viện Nặng (`frontend/.next`)

Dữ liệu phân tích tĩnh được trích xuất trực tiếp từ [app-build-manifest.json](file:///d:/03_Study/Studxy/frontend/.next/app-build-manifest.json) và toàn bộ các chunk trong thư mục [static/chunks](file:///d:/03_Study/Studxy/frontend/.next/static/chunks).
- **Tổng số JS chunks của dự án:** 45 files (Tổng dung lượng nén/uncompressed trên đĩa: **2,141.97 KB**).

### 3.1. Top 10 Routes có Bundle nặng nhất

| Hạng | Route | Tổng dung lượng JS | Số Chunks | Nhận xét kiến trúc |
| :---: | :--- | :---: | :---: | :--- |
| 1 | `/workspace/page` | **1,154.4 KB** | 14 chunks | Nặng nhất hệ thống. Import tĩnh toàn bộ PDF Reader, Studio, Review và GSAP. |
| 2 | `/app/courses/[id]/learn/page` | **867.1 KB** | 10 chunks | Import tĩnh `pdfjs-dist` (323.5 KB) ở module root dù học bài text/markdown. |
| 3 | `/app/courses/[id]/edit/page` | **419.6 KB** | 8 chunks | Chứa Course Editor studio logic. |
| 4 | `/app/studio/[id]/page` | **419.0 KB** | 8 chunks | Course Creation Studio studio logic. |
| 5 | `/app/layout` | **392.9 KB** | 9 chunks | Root layout của portal: Sidebar, MobileNav, Auth session bootstrap. |
| 6 | `/app/page` | **389.8 KB** | 8 chunks | Trang Hôm nay (Dashboard). |
| 7 | `/auth/page` | **387.4 KB** | 7 chunks | Đăng nhập, Google OAuth, Demo Switcher. |
| 8 | `/page` (Landing) | **385.5 KB** | 7 chunks | Bento landing page, theme switcher. |
| 9 | `/app/review/page` | **383.8 KB** | 7 chunks | Review flashcards, FSRS engine. |
| 10 | `/app/courses/[id]/page` | **382.8 KB** | 8 chunks | Course detail & roadmap overview. |

### 3.2. Top Chunks nặng nhất & Thư viện bên thứ ba phát hiện

| File Chunk | Dung lượng | Thư viện xác định | Vị trí sử dụng & Ảnh hưởng |
| :--- | :---: | :--- | :--- |
| `static/chunks/9b0008ae-28de272f141264c7.js` | **323.5 KB** | `pdfjs-dist` | Trình render PDF worker & text layer. Đang bị import tĩnh vào `/learn` và `/workspace`. |
| `static/chunks/app/workspace/page-389a74fd05f9b628.js` | **319.6 KB** | `pdfjs-dist`, `gsap` | Chứa code trang `/workspace` cùng animation transition GSAP. |
| `static/chunks/framework-28b60818f5cffd83.js` | **213.9 KB** | `react`, `react-dom` 19 | Core framework React 19. |
| `static/chunks/493-07510acc0d2af281.js` | **170.1 KB** | React DOM client runtime | Shared runtime chunks. |
| `static/chunks/4bd1b696-c023c6e3521b1417.js` | **169.0 KB** | React DOM server/hydration | Shared runtime chunks. |
| `static/chunks/app/app/courses/.../learn/page-239adddcb354671a.js` | **128.6 KB** | Monolithic Lesson UI | File component đơn lẻ dài 2,842 dòng (`CourseLearningWorkspace.tsx`). |
| `static/chunks/main-a568ed019040df31.js` | **128.3 KB** | Next.js Router & Hydration | Core Next.js 15 client. |
| `static/chunks/polyfills-42372ed130431b0a.js` | **110.0 KB** | Standard Web Polyfills | Tương thích trình duyệt. |

---

## 4. Phân Tích API Chậm Nhất (`/api/v1/*`)

Dữ liệu thống kê từ 84 cuộc gọi API thực tế ghi nhận được từ quá trình đo đạc:

| Phương thức & Endpoint | Số lần gọi | Thời gian Trung vị (ms) | Nhanh nhất (ms) | Chậm nhất (ms) | Trung bình (ms) | Nguyên nhân backend / frontend |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| `GET /api/v1/workspace/dashboard` | **84** | **213.45** | 147.22 | **477.40** | **248.72** | **Chậm nhất & Bị gọi nhiều nhất.** Backend join 6 bảng + parse JSON AST; Frontend bị gọi kép (Sidebar + Page). |
| `GET /api/v1/courses` | 12 | **131.63** | 109.31 | 200.88 | 143.09 | Build danh sách khóa học kèm tóm tắt đề cương. |
| `GET /api/v1/courses/{id}/progress` | 42 | **54.55** | 24.40 | 112.61 | 59.20 | **N+1 API Problem:** Mỗi khóa học trên catalog kích hoạt 1 request riêng lẻ để lấy thanh tiến độ. |
| `GET /api/v1/flashcards/due` | 12 | **58.65** | 7.27 | 162.98 | 74.29 | Truy vấn FSRS tính toán thẻ đến hạn. |
| `GET /api/v1/courses/25f79a81...` | 24 | **51.80** | 10.26 | 82.19 | 44.98 | Lấy toàn bộ cây Chapter -> Lesson của khóa học. |
| `GET /api/v1/practice-quizzes/{id}` | 6 | **52.96** | 2.30 | 84.89 | 47.66 | Nạp đề thi quiz kèm phương án. |
| `GET /api/v1/auth/me` | 48 | **4.20** | 1.80 | 12.40 | 5.10 | Xác thực token và lấy profile người dùng. |

---

## 5. Top 10 Nguyên Nhân Gây Chậm / Giật & Đề Xuất Cụ Thể Cho Đợt Redesign

### 🔴 Top 1: Auth Gate Layout FOUC gây Layout Shift nghiêm trọng (CLS 0.21 – 0.25)
- **Mức độ:** **Critical**
- **File nghi ngờ:** [frontend/src/app/app/layout.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/layout.tsx#L83-L94)
- **Hiện tượng:** Biến trạng thái `authReady` ban đầu là `false`. Trang render ngay một spinner toàn màn hình ("Đang xác thực phiên học tập..."). Sau khi `bootstrapAuthSession()` hoàn tất (~100-200ms), DOM bị thay thế đột ngột bằng `<Sidebar>` và `<main>` có kích thước khác hoàn toàn.
- **Tác động:** CLS nhảy vọt lên **0.2146 – 0.2516** trên hầu hết các trang con desktop.
- **Đề xuất khắc phục:** 
  1. Loại bỏ spinner toàn màn hình chặn layout.
  2. Dùng Server Component hoặc CSS Skeleton giữ khung kích thước cố định (`w-64` cho sidebar, `flex-1` cho main) ngay từ lần render đầu tiên để tránh reflow.

---

### 🔴 Top 2: Import tĩnh Monolithic trên `/workspace` gây nghẽn Main-Thread Mobile (TBT 1,219 ms)
- **Mức độ:** **Critical**
- **File nghi ngờ:** [frontend/src/app/workspace/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/workspace/page.tsx#L7-L17)
- **Hiện tượng:** Trang import tĩnh cùng lúc: `PersonalWorkspaceDashboard`, `ReaderAndSplitViews`, `ReviewAndPracticeView`, `CourseStudioView`, `CommandPaletteModal`, và GSAP animations.
- **Tác động:** Bundle phình lên **1,154.4 KB** (14 chunks). Trên Mobile 4x CPU throttle, trình duyệt mất **1,219 ms** thời gian khóa luồng (TBT), tổng thời gian các tác vụ dài là **1,569 ms** (8 long tasks).
- **Đề xuất khắc phục:** Chuyển đổi toàn bộ các view phụ (Reader, Studio, Review) sang dynamic import với Next.js:
  ```tsx
  const ReaderAndSplitViews = dynamic(() => import('@/components/reader/ReaderAndSplitViews'), { ssr: false, loading: () => <ViewSkeleton /> });
  ```

---

### 🔴 Top 3: Import tĩnh thư viện `pdfjs-dist` (323.5 KB) ở Module-Level trong Lesson Player
- **Mức độ:** **High**
- **File nghi ngờ:** [frontend/src/components/learn/CourseLearningWorkspace.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx#L6)
- **Hiện tượng:** Dòng code `import * as pdfjsLib from "pdfjs-dist";` nằm trực tiếp ở đầu file component.
- **Tác động:** Mọi học viên khi vào học bài giảng bất kỳ (kể cả bài text/markdown thuần túy không có PDF) đều buộc phải tải và parse **323.5 KB** mã nguồn của PDF.js, khiến bundle trang `/learn` lên tới **867.1 KB** và TBT mobile lên **184 ms**.
- **Đề xuất khắc phục:** Lazy-load `pdfjs-dist` bất đồng bộ chỉ khi bài học có tài liệu nguồn PDF:
  ```tsx
  const pdfjsLib = await import("pdfjs-dist");
  ```

---

### 🔴 Top 4: Request kép đồng thời vào `/api/v1/workspace/dashboard` (Sidebar + Page)
- **Mức độ:** **High**
- **File nghi ngờ:**
  - [frontend/src/components/navigation/Sidebar.tsx](file:///d:/03_Study/Studxy/frontend/src/components/navigation/Sidebar.tsx#L107)
  - [frontend/src/app/app/courses/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx#L56)
  - [frontend/src/app/app/documents/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/documents/page.tsx#L80)
  - [frontend/src/app/app/notes/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/notes/page.tsx#L80)
- **Hiện tượng:** Khi người dùng vào bất kỳ trang nào trong `/app/*`, `Sidebar` tự động fetch `/api/v1/workspace/dashboard` để lấy số thẻ due và streak. Cùng thời điểm đó, trang chính (courses/docs/notes) cũng fetch lại đúng API đó để lấy dữ liệu.
- **Tác động:** Tăng gấp đôi tải lên backend (84 lượt gọi trong phiên test), gây lãng phí băng thông và CPU server.
- **Đề xuất khắc phục:** Sử dụng cache tầng client (React Query / SWR hoặc lưu trong Zustand `useWorkspaceStore`) với `staleTime: 60_000`, chia sẻ dữ liệu giữa Sidebar và các Page.

---

### 🟡 Top 5: Monolithic Component `CourseLearningWorkspace.tsx` (2,842 dòng)
- **Mức độ:** **High**
- **File nghi ngờ:** [frontend/src/components/learn/CourseLearningWorkspace.tsx](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx#L1-L2842)
- **Hiện tượng:** Một component duy nhất ôm trọn logic: PDF canvas rendering, Markdown rendering, Notes split drawer, Practice Lab, Audio Reader, Text Highlighter, Flashcard generator.
- **Tác động:** Bất kỳ thao tác nhỏ nào (như gõ một ký tự trong sổ tay hoặc đổi trang PDF) đều có nguy cơ kích hoạt re-render một cây component khổng lồ hàng nghìn node DOM.
- **Đề xuất khắc phục:** Tách thành các sub-components độc lập có `React.memo`: `LessonOutlineDrawer`, `LessonPdfViewer`, `LessonMarkdownViewer`, `LessonNotesEditor`, `FlashcardQuickModal`.

---

### 🟡 Top 6: N+1 API Calls cho Course Progress trên Catalog
- **Mức độ:** **Medium**
- **File nghi ngờ:** [frontend/src/app/app/courses/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx) & [backend/src/Swms.Api/Features/SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L818)
- **Hiện tượng:** Sau khi tải danh sách khóa học từ `/api/v1/courses`, frontend thực hiện lặp và bắn hàng loạt request riêng lẻ `GET /api/v1/courses/{id}/progress` cho từng card khóa học trên màn hình.
- **Tác động:** Phát sinh hàng chục request đồng thời (mỗi request mất ~50-60 ms), làm chậm tốc độ render hoàn chỉnh của trang catalog.
- **Đề xuất khắc phục:** Backend trả kèm luôn `progressPercentage` và `completedLessonCount` ngay trong payload của `GET /api/v1/courses` hoặc cung cấp endpoint batch: `POST /api/v1/courses/progress-batch`.

---

### 🟡 Top 7: Backend Join đa bảng & Parse JSON AST đồng bộ trong `/workspace/dashboard`
- **Mức độ:** **Medium**
- **File nghi ngờ:** [backend/src/Swms.Api/Features/SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L744-L822)
- **Hiện tượng:** Endpoint `/dashboard` query toàn bộ Flashcards, Enrollments, Courses, Chapters, và Lessons. Đối với từng Lesson, hàm `CourseGenerationService.ExtractLessonMetadata(l.ContentAstJson)` parse chuỗi JSON AST lớn trong bộ nhớ CPU C# trên mọi request.
- **Tác động:** Thời gian phản hồi API trung vị lên tới **213.45 ms** (tối đa **477.40 ms**).
- **Đề xuất khắc phục:**
  1. Thêm các cột thực thể riêng biệt `SourceDocumentId`, `StartPage`, `EndPage` vào bảng `Lessons` khi tạo/cập nhật, thay vì parse JSON AST mỗi lần đọc.
  2. Bổ sung `IMemoryCache` với TTL ngắn (15-30 giây) theo `userId`.

---

### 🟡 Top 8: Thư viện Animation `gsap` nằm trong Main Bundle của Workspace
- **Mức độ:** **Medium**
- **File nghi ngờ:** [frontend/src/lib/animations.ts](file:///d:/03_Study/Studxy/frontend/src/lib/animations.ts) & [frontend/src/app/workspace/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/workspace/page.tsx#L17)
- **Hiện tượng:** Thư viện `gsap` (~60 KB minified) được import để thực hiện hiệu ứng chuyển cảnh tab workspace.
- **Tác động:** Làm tăng thời gian parse/compile JS trên thiết bị di động cấu hình yếu.
- **Đề xuất khắc phục:** Thay thế GSAP bằng CSS transitions thuần (`transition-opacity transition-transform duration-200`) hoặc Web Animations API (`element.animate()`).

---

### 🟢 Top 9: Gọi lặp `/api/v1/workspace/dashboard` để lấy dữ liệu nhỏ lẻ trên trang con
- **Mức độ:** **Low**
- **File nghi ngờ:**
  - [frontend/src/app/app/documents/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/documents/page.tsx#L80) (chỉ để lấy map tên khóa học)
  - [frontend/src/app/app/notes/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/notes/page.tsx#L80) (để lấy danh sách notebooks)
  - [frontend/src/app/app/review/page.tsx](file:///d:/03_Study/Studxy/frontend/src/app/app/review/page.tsx#L145) (chỉ để lấy thông số reviewStation)
- **Hiện tượng:** Các trang chức năng riêng biệt lại đi fetch toàn bộ dữ liệu cồng kềnh của Dashboard tổng thể chỉ để trích xuất 1 trường dữ liệu phụ.
- **Tác động:** Payload dư thừa, lãng phí tài nguyên mạng và bộ nhớ.
- **Đề xuất khắc phục:** Tạo endpoint chuyên biệt nhẹ hơn (ví dụ: `GET /api/v1/notebooks`, `GET /api/v1/courses/lookup`) hoặc lấy từ store chung.

---

### 🟢 Top 10: Import toàn bộ Icons `lucide-react` mà không tối ưu Tree-Shaking
- **Mức độ:** **Low**
- **File nghi ngờ:** Toàn bộ các component trong `frontend/src/components/*`
- **Hiện tượng:** Nhiều component import hàng chục icon từ package `lucide-react`. Ví dụ: `CourseLearningWorkspace.tsx` import cùng lúc 32 icons.
- **Tác động:** Tăng kích thước AST phân tích và dung lượng chunk.
- **Đề xuất khắc phục:** Cấu hình Next.js `optimizePackageImports: ['lucide-react']` trong `next.config.ts` để Next.js tự động chuyển đổi sang direct import từng icon file.

---

## 6. Bằng Chứng Thực Thi & Đường Dẫn Tệp

### 6.1. Lệnh kiểm thử đã thực thi

| STT | Lệnh thực thi chính xác | Exit Code | Thời gian thực thi | Mô tả |
| :---: | :--- | :---: | :---: | :--- |
| 1 | `node docs/qa-round2/scripts/c0-perf/bundle-analyzer.mjs` | **0** | ~1s | Phân tích kích thước bundle từ `frontend/.next`, dò tìm chunk và thư viện nặng. |
| 2 | `node docs/qa-round2/scripts/c0-perf/measure-runner.mjs` | **0** | ~4m 20s | Đo đạc 72 lượt chạy Playwright CDP trên 12 routes (Desktop & Mobile 4x CPU throttle). |
| 3 | `node docs/qa-round2/scripts/c0-perf/lighthouse-public.mjs` | **0** | ~1m 30s | Chạy kiểm thử độc lập Lighthouse trên `/` và `/auth` (3 runs desktop, 3 runs mobile). |

### 6.2. Đường dẫn tệp dữ liệu thô (JSON)

- Bảng tổng hợp Medians: [summary-medians.json](file:///d:/03_Study/Studxy/docs/qa-round2/data/c0-perf/summary-medians.json)
- Toàn bộ 72 lượt chạy chi tiết: [raw-runs.json](file:///d:/03_Study/Studxy/docs/qa-round2/data/c0-perf/raw-runs.json)
- Thống kê chi tiết độ trễ từng API: [api-latencies.json](file:///d:/03_Study/Studxy/docs/qa-round2/data/c0-perf/api-latencies.json)
- Báo cáo phân tích Bundle Chunks: [bundle-analysis.json](file:///d:/03_Study/Studxy/docs/qa-round2/data/c0-perf/bundle-analysis.json)
- Tổng hợp điểm số Lighthouse: [lighthouse-summary.json](file:///d:/03_Study/Studxy/docs/qa-round2/data/c0-perf/lighthouse/lighthouse-summary.json)

### 6.3. Bằng chứng ảnh chụp màn hình (Screenshots)

Toàn bộ 24 ảnh chụp màn hình tương ứng với 12 route trên cả Desktop và Mobile đã được lưu trữ tại thư mục [screenshots/c0-perf](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf):

- **Landing:** [landing-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/landing-desktop.png) · [landing-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/landing-mobile.png)
- **Auth:** [auth-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/auth-desktop.png) · [auth-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/auth-mobile.png)
- **App Dashboard:** [app-dashboard-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/app-dashboard-desktop.png) · [app-dashboard-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/app-dashboard-mobile.png)
- **Courses Catalog:** [app-courses-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/app-courses-desktop.png) · [app-courses-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/app-courses-mobile.png)
- **Course Detail:** [course-detail-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/course-detail-desktop.png) · [course-detail-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/course-detail-mobile.png)
- **Learn:** [course-learn-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/course-learn-desktop.png) · [course-learn-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/course-learn-mobile.png)
- **Quiz:** [course-quiz-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/course-quiz-desktop.png) · [course-quiz-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/course-quiz-mobile.png)
- **Review:** [review-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/review-desktop.png) · [review-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/review-mobile.png)
- **Documents:** [documents-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/documents-desktop.png) · [documents-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/documents-mobile.png)
- **Notes:** [notes-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/notes-desktop.png) · [notes-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/notes-mobile.png)
- **Settings:** [settings-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/settings-desktop.png) · [settings-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/settings-mobile.png)
- **Workspace Classic:** [workspace-desktop.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/workspace-desktop.png) · [workspace-mobile.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/c0-perf/workspace-mobile.png)

---

## 7. Mục Chưa Kiểm Chứng (Unverified Scope)

Tuân thủ quy định tại [AGY2_RULES.md](file:///d:/03_Study/Studxy/docs/qa-round2/AGY2_RULES.md), các khía cạnh sau **chưa được đo lường** trong phiên C0-PERF và cần thực hiện bổ sung trong các phiên chuyên sâu tiếp theo:
1. **Network Throttling thực tế (Slow 3G / Fast 3G):** Phiên đo này thực hiện trên mạng localhost với độ trễ ~1-4ms. Khi triển khai trên internet công cộng với mạng 3G/4G, thời gian tải bundle 1.15 MB của `/workspace` có thể kéo dài thêm 3-5 giây.
2. **Hiệu năng render file PDF dung lượng lớn (> 50-100 MB):** Phiên đo này sử dụng khóa học lộ trình mẫu có bài giảng text/markdown. Chưa kiểm nghiệm mức tiêu thụ bộ nhớ RAM khi người dùng mở đồng thời tài liệu PDF kỹ thuật hàng trăm trang trên thiết bị di động 4GB RAM.
3. **Thao tác tương tác kéo dài (INP dưới tải nặng):** Chỉ số INP được ghi nhận qua Event Timing khi nạp trang. Chưa đo INP tương tác sâu khi người dùng thực hiện gõ liên tục 500 từ vào sổ tay hoặc lật 100 flashcards liên tục trong 1 phiên học.
4. **Hiệu năng tải đồng thời nhiều người dùng (Multi-user concurrency):** Bài test này tập trung đo lường phía Client-side UX baseline cho 1 người dùng; chưa kiểm thử benchmark tải áp lực đồng thời 100+ người dùng truy vấn API.
