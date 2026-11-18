Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ A-FIX-FRONTEND: Sửa toàn bộ lỗi UX, CLS, Markdown, Mobile & Security Frontend (Chỉ sửa trong `frontend/`)

## Quy tắc chạy song song (BẮT BUỘC)
- **PHẠM VI SỬA CODE:** CHỈ được sửa file trong thư mục `frontend/`. **TUYỆT ĐỐI KHÔNG** sửa `backend/` hoặc `docs/qa-round2/scripts/`.
- **KHÔNG chạy `git commit`** và **KHÔNG chạy `restart-servers.ps1`** (có agent backend đang sửa song song; việc commit + rebuild + restart sẽ thực hiện tập trung sau khi cả 2 xong).
- Giữ nguyên MỌI `data-testid` hiện có.
- **KHÔNG để lại bất kỳ background task nào** trước khi kết thúc.
- Đầu vào tham khảo: `docs/qa-round2/A-hacker-report.md`, `docs/qa-round2/A-student-report.md`, `docs/qa-round2/A-ux-perf-report.md`, `docs/qa-round2/A-newbie-mobile-report.md`.

## Danh sách các lỗi phải sửa

### 1. `SEC-HIGH-01` (`INFO-04`): Gỡ mật khẩu cứng của tài khoản Demo khỏi bundle JS Client
- Trong `frontend/src/app/auth/page.tsx`: xóa hoàn toàn các chuỗi mật khẩu `"Studxy@2026"` và `"MinhAnhPassword123!"`.
- Trong `handleSwitchDefaultUser`: gọi `POST /api/v1/auth/login` với `{ email: "son.tran@studxy.vn", password: "__DEMO_SESSION__", demoProfile: "default" }`.
- Trong `handleSwitchSecondaryUser`: gọi thẳng `POST /api/v1/auth/login` với `{ email: "minh.anh@studxy.vn", password: "__DEMO_SESSION__", demoProfile: "secondary" }` (không gọi `/api/v1/auth/register` trước nữa để loại bỏ luôn lỗi 409 Conflict trên Console).

### 2. `SEC-HIGH-03` / `B3` (`HDR-04`..`HDR-07`): Frontend HTTP Security Headers
- Cập nhật cả `frontend/next.config.ts` và `frontend/next.config.js`:
  - Đặt `poweredByHeader: false`.
  - Thêm `async headers()` áp dụng cho `source: "/:path*"` với các header:
    - `X-Frame-Options: SAMEORIGIN`
    - `X-Content-Type-Options: nosniff`
    - `Referrer-Policy: strict-origin-when-cross-origin`
    - `Content-Security-Policy: default-src 'self' http://127.0.0.1:5080 http://localhost:5080 https://accounts.google.com; script-src 'self' 'unsafe-inline' 'unsafe-eval' blob: https://accounts.google.com; worker-src 'self' blob:; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://accounts.google.com; font-src 'self' data: https://fonts.gstatic.com; img-src 'self' data: blob: https:; connect-src 'self' http://127.0.0.1:5080 http://localhost:5080 https://accounts.google.com; frame-src 'self' https://accounts.google.com; frame-ancestors 'self';`

### 3. `UXP-01`: Sửa triệt để lỗi rò rỉ placeholder `\uE000CODE_0\uE000` (`CODE_0`) trong `LessonMarkdownView.tsx`
- Trong `frontend/src/components/reader/LessonMarkdownView.tsx` (hàm `tokenizeInlineMarkdown` và chỗ render `lesson-relative-md-link`):
  - Giải phóng toàn bộ placeholder `\uE000CODE_(\d+)\uE000` bên trong `linkItems` (thuộc tính `text`) về chuỗi code gốc từ `codeItems`.
  - Bổ sung bộ lọc an toàn loại bỏ mọi ký tự sót thuộc dải Private Use Area `[\uE000-\uF8FF]` và `\u001A` trong `cleanStudyInlineArtifacts()` / trước khi render text node.

### 4. `UXP-02` & `UXP-03`: Triệt tiêu Layout Shift (đưa CLS Desktop `/app/*` và Mobile `/learn` về `< 0.05`)
- **`UXP-02` (Desktop `/app/*`):**
  - Trong các trang `/app/page.tsx`, `/app/courses/page.tsx`, `/app/courses/[id]/page.tsx`, `/app/documents/page.tsx`, `/app/notes/page.tsx`, `/app/library/page.tsx`, `/app/settings/page.tsx`, `/app/courses/[id]/quiz/page.tsx`:
    - Khi `isLoading === true`, **KHÔNG** render `<div className="flex items-center justify-center h-full">` (vì không có `max-w-* mx-auto` nên khi dữ liệu tải xong sẽ bị giật từ `x: 240` sang `x: 328` tạo CLS ~0.21).
    - Luôn bọc trạng thái loading trong **cùng container lớp ngoài** (`<div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-6 ... min-h-[80vh]">`, hoặc `max-w-4xl`/`max-w-3xl` khớp y hệt với trạng thái đã load của trang đó) với skeleton giữ chiều cao cố định bên trong.
  - Trong `frontend/src/components/navigation/Sidebar.tsx`: cố định chiều cao tối thiểu (`min-h-[156px]`) cho khối "GẦN ĐÂY" (render 4 dòng skeleton mượt khi đang tải) để không đẩy nút `+ Tạo khóa học` và phần chân sidebar tụt xuống 192px sau khi dữ liệu về.
- **`UXP-03` (Mobile `/learn`):**
  - Trong `frontend/src/components/learn/CourseLearningWorkspace.tsx` (thanh switcher toolbar chứa các tab `Bài giảng`, `Thực hành`, `Tài liệu PDF`, `Song song` và `<select>` PDF tham chiếu): không dùng `flex-wrap` gây nhảy dòng (+66px) khi nạp xong `courseDocuments` trên màn hình `390px`. Giữ thanh toolbar trên một dòng cố định chiều cao (`h-11` / `flex-nowrap overflow-x-auto swms-thin-scrollbar`) hoặc đặt `<select>` cố định chỗ để không đẩy cột bài học xuống dưới.

### 5. `UXP-04`: Thanh lọc lĩnh vực trên `/app/courses` ở `1440×900`
- Trong `frontend/src/app/app/courses/page.tsx`: tinh chỉnh padding/gap hoặc cho phép wrap hợp lý để nút cuối cùng `"Chủ đề khác (...)"` hiển thị trọn vẹn trong container `960px` ở màn hình `1440×900`, không bị cắt lẹm 24px bên phải.

### 6. `UXP-05` / `B9`: Xử lý lỗi PDF 404 trong `CourseLearningWorkspace.tsx`
- Khi tải tệp PDF thất bại (HTTP 404 hoặc lỗi `pdfjs.getDocument`), lưu state `pdfLoadError` và hiển thị thông báo lỗi rõ ràng + nút "Thử tải lại" / "Quay lại bài giảng" ở cả chế độ Văn bản và Bản gốc, tuyệt đối không để spinner quay vô tận.

### 7. `UXP-06` / `STU-04` / `B7`: Thông báo lỗi đồng bộ Flashcard thân thiện
- Trong `frontend/src/app/app/review/page.tsx` (chỗ xử lý `!resp.ok` khi sync review): parse JSON từ `errText` và ánh xạ sang thông báo tiếng Việt thân thiện (vd `TOKEN_VERSION_REVOKED` → `"Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."`), không nối chuỗi JSON kỹ thuật thô vào banner.

### 8. `UXP-07` / `STU-02` / `B15`, `STU-01` / `B6`, `STU-03` (`beforeunload`), và Auto-enroll `/learn`
- **`B15`:** Trong `frontend/src/app/app/courses/[id]/page.tsx` và `CourseLearningWorkspace.tsx`: kiểm tra số lượng quiz của khóa học (từ `course.quizCount` hoặc đối chiếu `dashData.quizzes`). Chỉ khi thực sự có `0` bài quiz thì render nút `[data-testid="course-quiz-btn"]` ở trạng thái `disabled` kèm `title="Khóa học chưa có bài kiểm tra"` và nhãn `"Chưa có bài kiểm tra"`; khi khóa học có quiz thì giữ nguyên link hoạt động bình thường.
- **`B6` (`STU-01`):** Trong `frontend/src/app/app/courses/new/page.tsx`, `frontend/src/app/app/courses/[id]/page.tsx`, và `CourseLearningWorkspace.tsx`, strip tiền tố `^(?:Chương|Chapter|Phần)\s*\d+\s*[:.\-–—]\s*` bị trùng trước khi render `Chương {idx + 1}: ...`.
- **`STU-03`:** Trong `CourseLearningWorkspace.tsx`, thêm `useEffect` đăng ký `window.addEventListener("beforeunload", ...)` khi `noteDirty === true` để cảnh báo người dùng nếu reload (F5) trong lúc autosave chưa kịp chạy.
- **Auto-enroll:** Trong `CourseLearningWorkspace.tsx`, khi người dùng mở bài học của một khóa học public (chưa enroll), tự động gọi ngầm `POST /api/v1/courses/${courseId}/enroll` (ignore nếu đã enroll) để người dùng bấm "Đánh dấu hoàn thành" không bị backend chặn 403.

### 9. `UXP-08`..`UXP-12`: Accessibility (WCAG AA), Dark Mode Contrast, B12, B14
- **`UXP-08`:** Thêm `<h1 className="sr-only">Trạm ôn tập thẻ ghi nhớ FSRS</h1>` trong `frontend/src/app/app/review/page.tsx`.
- **`UXP-09`:** Trong `frontend/src/app/globals.css`, nâng `--text-tertiary` trong `.dark` từ `#71717a` lên `#a1a1aa` (tỷ lệ tương phản >= 6.5:1), và kiểm tra đảm bảo mọi nút có nền sáng `#818CF8` ở dark mode đều dùng chữ tối `#0F0F12` thay vì chữ trắng.
- **`UXP-10` (B14):** Trong `frontend/src/app/auth/page.tsx`, đổi nhãn `"Tiếp tục với Google (Chưa bật Client ID)"` thành `"Tiếp tục với Google"` kèm badge nhỏ `"Chưa cấu hình"`.
- **`UXP-11` (B12):** Trong `frontend/src/app/auth/page.tsx`, thêm `minLength={6}` cho `#auth-password`, đồng thời validate rõ ràng khi đăng ký yêu cầu mật khẩu có ít nhất 6 ký tự gồm cả chữ cái và chữ số.
- **`UXP-12`:** Trong `frontend/src/app/app/settings/page.tsx`, bổ sung `role="switch"`, `aria-checked`, và `aria-label` cho các nút toggle switch.

### 10. `MOB-01`..`MOB-05`: Tối ưu cho Người mới & Mobile `390×844` / Tablet `768×1024`
- **`MOB-01`:** Kiểm tra và cập nhật `frontend/src/components/landing/HeroSection.tsx` (và mọi CTA landing liên quan) dẫn về `/app` thay vì `/workspace` (lưu ý kiểm tra `LandingPageV2.tsx` xem đã là `/auth?tab=register` hoặc `/app` chưa).
- **`MOB-02`:** Trong `CourseLearningWorkspace.tsx`, cho nút `Mục lục` hiển thị trên cả Mobile và Tablet (`inline-flex` thay vì `hidden lg:inline-flex`).
- **`MOB-03`:** Nâng kích thước tap target tối thiểu lên `min-h-[36px] min-w-[36px]` (khuyến nghị `h-9`–`h-10`) cho các chip lọc ở `/app/courses`, nút Quay lại & Hoàn tác ở `/app/review`, và nút bật/tắt mật khẩu ở `/auth`.
- **`MOB-04`:** Trong `frontend/src/app/app/courses/page.tsx`, phân biệt khi `courses.length === 0` (người mới chưa có khóa học nào) thì hiển thị EmptyState chào mừng và CTA tạo khóa học đầu tiên, không báo *"Không tìm thấy khóa học phù hợp"*.
- **`MOB-05`:** Trong `frontend/src/components/layout/TopNavHeader.tsx`, hiển thị nút/liên kết quay về `/app` rõ ràng trên header mobile khi đang ở `/workspace`.

## Kiểm chứng bắt buộc trước khi kết thúc
1. Chạy `cd frontend; npx tsc --noEmit` → exit code `0`.
2. Chạy `cd frontend; npm run test:unit` → exit code `0`.
3. Kiểm tra `git status -sb` xem chỉ sửa file trong `frontend/` và `docs/qa-round2/A-fix-frontend-summary.md`. KHÔNG commit git, KHÔNG restart server.
4. Ghi tóm tắt rõ ràng từng hạng mục đã sửa, file/dòng và kết quả kiểm chứng (kèm mục **Chưa kiểm chứng**) vào `docs/qa-round2/A-fix-frontend-summary.md`.
