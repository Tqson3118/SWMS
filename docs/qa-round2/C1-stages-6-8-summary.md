# Báo Cáo Tổng Kết: Hoàn Tất Giai Đoạn 6, 7, 8 (Design System "Clean & Calm Learning" & Tối Ưu Hiệu Năng Frontend)

- **Mã nhiệm vụ:** `C1-REDESIGN-CONT`
- **Nhánh thực hiện:** `qa/agy2-round2`
- **Thời gian hoàn thành:** 2026-11-19
- **Phạm vi tác động:** Chỉ chỉnh sửa trong `frontend/` (TUYỆT ĐỐI KHÔNG sửa `backend/`)

---

## 1. Danh Sách Commit Đã Tạo Trong Đợt Này

1. **Giai đoạn 6:**
   - **Commit SHA:** `be8d111`
   - **Message:** `feat(pages): calm redesign for courses studio notes documents library`
   - **Nội dung:**
     - Tinh chỉnh regex và mở rộng phân loại khóa học tiếng Việt/tiếng Anh trong `course-helpers.ts` (bổ sung word boundaries `\b`, phân tách `ai`, ngoại ngữ, công nghệ, toán/khoa học).
     - Mở rộng bộ unit test lên 16 trường hợp thực tế trong `course-helpers.test.ts`.
     - Chuyển đổi toàn diện các trang sang design system "Clean & Calm Learning" (dùng các UI token CSS chuẩn, separator `·`, component nền `Button`, `Card`, `Badge`, `ProgressBar`, `EmptyState`):
       - `/app/courses` (danh sách catalog, phân loại domain, tìm kiếm, lọc tiến độ)
       - `/app/courses/[id]` (chi tiết khóa học, đề cương chapter-lesson)
       - `/app/courses/new` (tạo khóa học mới, upload tài liệu)
       - `/app/studio` & `/app/studio/create` (xưởng soạn thảo bài giảng)
       - `/app/notes` (quản lý sổ tay học tập)
       - `/app/documents` (quản lý tài liệu PDF)
       - `/app/library` (thư viện tổng hợp)
       - `/app/settings` (cài đặt tài khoản, AI model, và tích hợp giới hạn ôn tập FSRS hằng ngày)
     - Giữ nguyên toàn bộ `data-testid` phục vụ E2E regression.

2. **Giai đoạn 7:**
   - **Commit SHA:** `26cd94c`
   - **Message:** `feat(public-workspace): apply calm design tokens to landing auth and workspace`
   - **Nội dung:**
     - Áp dụng hệ token Clean & Calm Learning (light-first, hỗ trợ dark mode hoàn chỉnh, màu nhận diện `#5E51E8`, separator `·`, typography chuẩn, micro-animation tuân thủ `prefers-reduced-motion`) cho:
       - Landing Page `/` (`frontend/src/components/landing/LandingPageV2.tsx`)
       - Auth `/auth` (`frontend/src/app/auth/page.tsx`)
       - Personal Workspace `/workspace` (`frontend/src/app/workspace/page.tsx`)
     - Giữ nguyên toàn bộ `data-testid` (demo switcher, login tabs, action buttons, quick capture...).

3. **Giai đoạn 8:**
   - **Commit SHA:** `f1df9da`
   - **Message:** `perf(frontend): dynamic imports skeleton gate and dashboard fetch dedupe`
   - **Nội dung:**
     - **Dynamic Imports & Code Splitting:**
       - `frontend/src/components/reader/ReaderAndSplitViews.tsx`: Chuyển module-level static import của `pdfjs-dist` sang dynamic import bất đồng bộ bên trong `loadPdf()`, chỉ import type ở module level.
       - `frontend/src/components/learn/CourseLearningWorkspace.tsx`: Dynamic import component nặng `LessonPracticeLab` (~90 KB).
       - `frontend/src/lib/animations.ts` & `frontend/src/lib/animations.js`: Lazy-load `gsap` chỉ khi thực sự cần chạy animation; bỏ qua hoàn toàn nếu người dùng bật `prefers-reduced-motion`, giải phóng ~60 KB GSAP chunk khỏi bundle ban đầu.
       - `frontend/src/app/workspace/page.tsx`: Dynamic import với `next/dynamic` cho 5 panel và modal lớn (`ReaderAndSplitViews`, `ReviewAndPracticeView`, `CourseStudioView`, `CommandPaletteModal`, `KeyboardShortcutsModal`). First Load JS của `/workspace` giảm từ **1,154.4 kB xuống 145 kB**! First Load JS của `/app/courses/[id]/learn` giảm từ **867.1 kB xuống 143 kB**!
     - **Layout Shift Prevention (CLS < 0.1):**
       - Xác nhận và hoàn thiện skeleton gate tại `frontend/src/app/app/layout.tsx` và `frontend/src/app/workspace/page.tsx` giữ nguyên kích thước layout (sidebar `w-[240px]`, main grid skeleton), loại bỏ spinner toàn màn gây reflow đột ngột.
     - **Deduplication & Cache Gọi Kép `GET /api/v1/workspace/dashboard`:**
       - Bổ sung `fetchDashboardCached()` và `invalidateDashboardCache()` trong `frontend/src/lib/api-client.ts` với cơ chế in-flight promise deduplication và short-lived TTL cache (25s).
       - Tự động invalidate cache khi nhận event `swms:data-mutated` hoặc đổi tài khoản active user (`setActiveUserHeaderId`).
       - Thay thế các lệnh gọi trực tiếp đồng thời giữa `Sidebar` và các trang `/app/*` (`/app`, `/app/documents`, `/app/notes`, `/app/library`, `/app/settings`, `/app/review`, `/app/courses`, `/app/courses/[id]`, `/workspace`).
     - **Kiểm Soát N+1 Catalog Progress:**
       - Xác nhận trang `/app/courses` sử dụng hàm tính toán đồng bộ `getCourseProgress()` dựa trên danh sách khóa học có sẵn, không thực hiện N+1 HTTP request riêng rẽ `GET /courses/{id}/progress`.

---

## 2. Danh Sách Các File Đã Chỉnh Sửa Trong 3 Giai Đoạn

### Giai đoạn 6:
- `frontend/src/lib/course-helpers.ts`
- `frontend/src/lib/course-helpers.test.ts`
- `frontend/package.json`
- `frontend/src/app/app/courses/page.tsx`
- `frontend/src/app/app/courses/[id]/page.tsx`
- `frontend/src/app/app/courses/new/page.tsx`
- `frontend/src/app/app/studio/page.tsx`
- `frontend/src/app/app/studio/create/page.tsx`
- `frontend/src/app/app/notes/page.tsx`
- `frontend/src/app/app/documents/page.tsx`
- `frontend/src/app/app/library/page.tsx`
- `frontend/src/app/app/settings/page.tsx`

### Giai đoạn 7:
- `frontend/src/components/landing/LandingPageV2.tsx`
- `frontend/src/app/auth/page.tsx`
- `frontend/src/app/workspace/page.tsx`

### Giai đoạn 8:
- `frontend/src/lib/api-client.ts`
- `frontend/src/lib/animations.ts`
- `frontend/src/lib/animations.js`
- `frontend/src/stores/useWorkspaceStore.ts`
- `frontend/src/components/reader/ReaderAndSplitViews.tsx`
- `frontend/src/components/learn/CourseLearningWorkspace.tsx`
- `frontend/src/components/navigation/Sidebar.tsx`
- `frontend/src/app/workspace/page.tsx`
- `frontend/src/app/app/page.tsx`
- `frontend/src/app/app/courses/page.tsx`
- `frontend/src/app/app/courses/[id]/page.tsx`
- `frontend/src/app/app/documents/page.tsx`
- `frontend/src/app/app/notes/page.tsx`
- `frontend/src/app/app/library/page.tsx`
- `frontend/src/app/app/settings/page.tsx`
- `frontend/src/app/app/review/page.tsx`

---

## 3. Exit Code Các Lệnh Kiểm Tra

| Lệnh Kiểm Tra | Thư Mục / Cách Chạy | Exit Code | Kết Quả Chi Tiết |
| :--- | :--- | :---: | :--- |
| `npx tsc --noEmit` | `frontend/` | **0** | Thành công, 0 lỗi TypeScript trên toàn bộ codebase frontend |
| `npm run test:unit` | `frontend/` | **0** | 16/16 test phân loại khóa học pass + Golden vectors FSRS v5 pass |
| `restart-servers.ps1 -Build` | `docs/qa-round2/tools/` | **0** | Next.js production build 18/18 routes thành công; Backend build thành công; Cả 2 server sẵn sàng (`127.0.0.1:3000` & `127.0.0.1:5080`) |

---

## 4. Mục Chưa Kiểm Chứng (Unverified Scope)

Các hạng mục sau nằm ngoài phạm vi thực thi thuần frontend của nhiệm vụ này hoặc cần test chuyên sâu ở vòng tiếp theo:

1. **Hiệu năng thực tế Lighthouse sau khi triển khai trên mobile thiết bị yếu:**
   - Mặc dù bundle size của `/workspace` đã giảm từ ~1.15 MB xuống 145 kB và `/learn` từ 867 kB xuống 143 kB nhờ dynamic import, các chỉ số TBT/LCP cụ thể dưới điều kiện 4x CPU throttling cần được đo đạc lại bằng công cụ audit tự động ở task C2 (Verify).
2. **Batch Endpoint cho Course Progress ở Backend:**
   - Trang `/app/courses` hiện đang dùng dữ liệu đồng bộ có sẵn. Nếu trong tương lai catalog hiển thị các khóa học chưa đăng ký cần tiến độ chi tiết mà không có trong session dashboard, backend nên bổ sung endpoint `POST /api/v1/courses/progress-batch` thay vì gọi lẻ. Đã ghi nhận kiến nghị và không sửa mã backend theo đúng quy tắc.
3. **Multi-user Concurrent Load Test:**
   - Việc deduplicate request `GET /api/v1/workspace/dashboard` giúp giảm một nửa số lượng query nặng lên backend trên mỗi client session. Tuy nhiên, mức độ giảm tải tổng thể trên DB SQLite khi có nhiều người dùng đồng thời cần được kiểm chứng qua kịch bản benchmark tải ở giai đoạn nghiệm thu.
