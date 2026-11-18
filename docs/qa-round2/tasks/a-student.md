Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ A-STUDENT: Đóng vai Sinh viên sử dụng Studxy hằng ngày trên giao diện mới (CHỈ TEST + BÁO CÁO, KHÔNG SỬA CODE)

## Quy tắc chạy song song (BẮT BUỘC)
- **KHÔNG sửa code** trong `frontend/` hoặc `backend/`.
- **KHÔNG restart hay tắt server** `127.0.0.1:3000` / `127.0.0.1:5080` (có 2 agent khác đang chạy test cùng lúc).
- **KHÔNG chạy `git commit`** (tránh xung đột lock git khi chạy song song).
- **Cô lập dữ liệu:** chỉ dùng tài khoản demo "Sơn Trần" và "Minh Anh" ở chế độ ĐỌC/QUAN SÁT giao diện; mọi thao tác tạo/sửa/ôn tập/nộp bài phải thực hiện trên tài khoản test DO BẠN TỰ ĐĂNG KÝ (`qa+student-<timestamp>@test.local`).
- **KHÔNG để lại bất kỳ background task nào** trước khi kết thúc.
- Mọi script Playwright đặt tại `docs/qa-round2/scripts/a-student/`, ảnh chụp bằng chứng lưu tại `docs/qa-round2/screenshots/a-student/`.

## Mục tiêu & Vai trò
Bạn là sinh viên đại học / người tự học nghiêm túc sử dụng Studxy hằng ngày trên Desktop (1440×900) sau đợt redesign "Clean & Calm Learning". Hãy viết và chạy script Playwright headless kiểm thử trực tiếp toàn bộ luồng học tập thực tế trên `http://127.0.0.1:3000`:

1. **Quan sát tổng thể trên tài khoản demo "Sơn Trần" (chỉ đọc):**
   - Kiểm tra `/app` (Dashboard), `/app/courses` (bộ lọc lĩnh vực, tiến độ, tìm kiếm), `/app/notes`, `/app/documents`, `/app/library`, `/app/settings`.
   - Mở một vài khóa học (trong đó có khóa IELTS / Công nghệ) vào `/app/courses/[id]/learn` để quan sát trải nghiệm đọc bài giảng, drawer "Sổ tay & Thẻ", đọc song song PDF.
2. **Tự đăng ký tài khoản sinh viên mới (`qa+student-<timestamp>@test.local`) và trải nghiệm trọn vòng đời học tập:**
   - **Tạo khóa học bằng 3 luồng:**
     1. AI Wizard (nhập chủ đề, ví dụ học thuật hoặc ngoại ngữ) — kiểm tra cấu trúc chương/bài học, xem có còn lỗi lặp prefix `"Chương 1 : Chương 1: ..."` (B6 cũ) không.
     2. Upload PDF hoặc Markdown — kiểm tra input upload có `accept=".pdf"` / định dạng hợp lệ không (B13 cũ), tốc độ xử lý, trạng thái hiển thị.
     3. Course Studio (`/app/studio` và `/app/studio/create`) — tạo/chỉnh sửa đề cương, thêm bài học thủ công.
   - **Học bài trên `/app/courses/[id]/learn`:**
     - Đọc bài giảng Markdown, đổi cỡ chữ (`T Aa`), mở/đóng Mục lục, mở drawer "Sổ tay & Thẻ".
     - Tạo highlight văn bản, ghi chú trong Sổ tay, kiểm tra cơ chế tự động lưu hoặc cảnh báo unsaved changes khi reload trang (F5).
     - Tạo thẻ Flashcard mới ngay từ bài học và đánh dấu hoàn thành bài học.
   - **Ôn tập FSRS trên `/app/review`:**
     - Ôn thẻ bằng chuột và phím tắt (`Space`, `1`–`4`, `Z` hoàn tác).
     - Kiểm tra giới hạn phiên ôn tập mỗi ngày (mặc định 100 thẻ trong `/app/settings`), nút "Ôn thêm 20 thẻ" nếu hết hạn mức, và kiểm tra lỗi đồng bộ (B7 cũ xem còn hiện raw JSON không).
   - **Kiểm tra Quiz trên `/app/courses/[id]/quiz`:**
     - Làm bài trắc nghiệm và tự luận, kiểm tra kết quả chấm và hiển thị trên Dashboard.
     - Kiểm tra trường hợp khóa học có **0 câu hỏi quiz** (B15 cũ): nút Kiểm tra hoặc màn hình hiển thị thế nào?
   - **Command Palette (`Ctrl+K`) & Các trang còn lại:**
     - Tìm kiếm khóa học/bài học/sổ tay và tạo nhanh ghi chú/thẻ qua `Ctrl+K`.
     - Kiểm tra `/app/notes`, `/app/documents`, `/app/library`, `/app/settings` và `/workspace` (Workspace Classic).

## Định dạng báo cáo bắt buộc (`docs/qa-round2/A-student-report.md`, tiếng Việt)
1. **Kết quả chính & Bảng tổng hợp các luồng đã test** (Pass / Warning / Bug).
2. **Bằng chứng thực thi:** lệnh chính xác + exit code + đường dẫn script & danh sách ảnh chụp trong `docs/qa-round2/screenshots/a-student/`.
3. **Danh sách vấn đề / bug phát hiện:** ID (`STU-01`...), mức độ (Critical / High / Medium / Low), bước tái hiện, mong đợi vs thực tế, file/dòng nghi ngờ trong `frontend/` hoặc `backend/`.
4. **Góp ý như người dùng thật (Sinh viên):**
   - Cảm nhận khi đọc/học lâu trên giao diện mới "Clean & Calm Learning".
   - Những điểm còn khựng/bất tiện trong luồng Đọc bài → Ghi chú → Thẻ FSRS → Quiz.
   - Đề xuất tính năng mới (đặc biệt cho nhu cầu học thực hành như luyện Tiếng Anh nghe/nói/chép chính tả, nhắc ôn tập, theo dõi tiến độ).
5. **Chưa kiểm chứng.**
