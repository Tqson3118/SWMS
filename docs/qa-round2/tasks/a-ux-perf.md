Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md`, `docs/qa-round2/C1-redesign-report.md`, và `docs/qa-round2/OLD_REPORT.md` trước khi làm bất cứ gì.

# Nhiệm vụ A-UX-PERF: Chuyên gia UX/UI, Accessibility & Performance Auditor (CHỈ TEST + BÁO CÁO, KHÔNG SỬA CODE)

## Quy tắc chạy song song (BẮT BUỘC)
- **KHÔNG sửa code** trong `frontend/` hoặc `backend/`.
- **KHÔNG restart hay tắt server** `127.0.0.1:3000` / `127.0.0.1:5080` (có 2 agent khác đang chạy test cùng lúc).
- **KHÔNG chạy `git commit`** (tránh xung đột lock git khi chạy song song).
- **Cô lập dữ liệu:** không sửa/xóa dữ liệu của tài khoản demo "Sơn Trần" và "Minh Anh". Khi cần tạo khóa học/tài liệu để test, tự đăng ký tài khoản `qa+uxperf-<timestamp>@test.local`.
- **KHÔNG để lại bất kỳ background task nào** trước khi kết thúc.
- Mọi script Playwright/CDP đặt tại `docs/qa-round2/scripts/a-ux-perf/`, ảnh chụp lưu tại `docs/qa-round2/screenshots/a-ux-perf/`.

## Mục tiêu & Phạm vi kiểm tra sâu
Bạn là chuyên gia UX/UI, Accessibility (WCAG AA) và Frontend Performance kiểm toán lại toàn bộ bản sau C1 Redesign trên `http://127.0.0.1:3000`:

1. **Điều tra gốc rễ CLS Desktop & LCP/CLS `/learn` Mobile (từ `C1-redesign-report.md`):**
   - Báo cáo C1 ghi nhận CLS Desktop trên `/app` (`0.2241`), `/app/courses` (`0.2241`), `/app/documents` (`0.2298`), `/app/notes` (`0.2299`), `/app/courses/[id]/quiz` (`0.2211`), `/app/settings` (`0.1416`) và `/app/courses/[id]/learn` Mobile (`CLS = 0.2299`, `LCP = 2,728ms`) vẫn vượt ngưỡng chuẩn (`CLS < 0.1`, `LCP < 2.5s`).
   - Viết script Playwright + `PerformanceObserver` (`layout-shift` với `buffered: true`) để trích xuất **`entry.sources`** (selector DOM node dịch chuyển, `previousRect`, `currentRect`, giá trị `value`, thời điểm `startTime`) trên Desktop & Mobile cho các route này.
   - Đọc mã nguồn liên quan (`frontend/src/app/app/layout.tsx`, các trang `/app/*`, `CourseLearningWorkspace.tsx`) để chỉ ra **chính xác nguyên nhân & dòng code** và đề xuất fix cụ thể đưa CLS về `< 0.05`.
2. **Kiểm tra lỗi Markdown, placeholder `CODE_0` & PDF 404 (B9, ISSUE-03):**
   - Mở bài học IELTS (`ielts-leak-readme` trong tài khoản Sơn Trần — xem `docs/qa-round2/scripts/capture-ielts-leak.js` để biết cách vào đúng bài) và các bài giảng có inline code, bảng, danh sách, link `.md`.
   - Kiểm tra lỗi rò rỉ chuỗi placeholder dạng `CODE_0` (hoặc `\u001aCODE_0\u001a`) trên giao diện sau C1; đọc `frontend/src/components/reader/LessonMarkdownView.tsx` để chỉ rõ chính xác hàm/dòng gây lỗi và cách fix triệt để.
   - Kiểm tra trường hợp PDF bị lỗi / 404 (B9 cũ) hiển thị thế nào.
3. **Kiểm tra danh sách bug UX cũ & lỗi nhỏ mới phát hiện:**
   - B6: Lặp prefix chương AI (`"Chương 1 : Chương 1: ..."`).
   - B7: Thông báo lỗi đồng bộ flashcard có hiện raw JSON không.
   - B12: Mật khẩu thiếu `minlength="6"` và dùng `alert()` thay inline error trên `/auth`.
   - B13: Input file upload thiếu `accept=".pdf"` (hoặc định dạng hỗ trợ).
   - B14: Nút đăng nhập Google trên `/auth` có hiện chữ kỹ thuật `"Chưa bật Client ID"` không.
   - B15: Khóa học có 0 câu hỏi quiz — trạng thái nút và trang `/quiz`.
   - Thanh filter lĩnh vực trên `/app/courses` ở 1440×900: nút cuối (`Chủ đề khác (...)`) có bị cắt/lẹm ở mép phải không.
4. **Kiểm toán Accessibility & Polish (Light + Dark mode ở 1440×900 & 390×844):**
   - Kiểm tra tương phản màu WCAG AA (text phụ/placeholder/badge trên nền light và dark).
   - Kiểm tra `Tab` focus ring, `aria-label` trên các nút chỉ có icon, semantic heading (`<h1>` duy nhất), `prefers-reduced-motion`.

## Định dạng báo cáo bắt buộc (`docs/qa-round2/A-ux-perf-report.md`, tiếng Việt)
1. **Kết quả chính & Điểm đánh giá UX/UI/Perf sau C1 Redesign.**
2. **Bằng chứng thực thi:** lệnh chính xác + exit code + bảng attribution `layout-shift` sources + đường dẫn script và ảnh chụp trong `docs/qa-round2/screenshots/a-ux-perf/`.
3. **Danh sách vấn đề (`UXP-01`...):** mức độ (Critical / High / Medium / Low), bước tái hiện, file/dòng chính xác và hướng dẫn code fix cụ thể (để team frontend sửa ngay ở bước tiếp theo).
4. **Góp ý như chuyên gia UX & người dùng:** đánh giá tổng quan "Clean & Calm Learning", điểm mạnh và gợi ý nâng cấp trải nghiệm / hiệu năng.
5. **Chưa kiểm chứng.**
