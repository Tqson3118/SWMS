Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ A-NEWBIE-MOBILE: Người dùng mới lần đầu trên Mobile 390px & Tablet 768px (CHỈ TEST + BÁO CÁO, KHÔNG SỬA CODE)

## Quy tắc chạy song song (BẮT BUỘC)
- **KHÔNG sửa code** trong `frontend/` hoặc `backend/`.
- **KHÔNG restart hay tắt server** `127.0.0.1:3000` / `127.0.0.1:5080` (có 2 agent khác đang chạy test cùng lúc).
- **KHÔNG chạy `git commit`** (tránh xung đột lock git khi chạy song song).
- **Cô lập dữ liệu:** không sửa/xóa dữ liệu của tài khoản demo "Sơn Trần" và "Minh Anh". Mọi thao tác tạo khóa học, ghi chú, ôn tập thực hiện trên tài khoản test DO BẠN TỰ ĐĂNG KÝ (`qa+newbie-<timestamp>@test.local`).
- **KHÔNG để lại bất kỳ background task nào** trước khi kết thúc.
- Mọi script Playwright đặt tại `docs/qa-round2/scripts/a-newbie-mobile/`, ảnh chụp lưu tại `docs/qa-round2/screenshots/a-newbie-mobile/`.

## Mục tiêu & Vai trò
Bạn là người mới lần đầu sử dụng Studxy (chưa biết các thuật ngữ chuyên môn như FSRS, PKM, Spaced Repetition, OCC), truy cập chủ yếu bằng điện thoại **Mobile `390×844`** (`isMobile: true`, `hasTouch: true`) và **Tablet `768×1024`**. Hãy viết và chạy script Playwright mô phỏng thao tác chạm thật trên `http://127.0.0.1:3000`:

1. **Ấn tượng đầu & Hành trình người mới (Onboarding):**
   - Truy cập `/` trên `390×844` → bấm nút CTA chính → vào `/auth` → đăng ký tài khoản mới (`qa+newbie-<timestamp>@test.local`) và quan sát thêm trạng thái trống của tài khoản demo "Minh Anh".
   - Đánh giá EmptyState trên Dashboard `/app`, `/app/courses`, `/app/review`, `/app/notes`, `/app/documents`: người mới có hiểu ngay cần làm bước 1, 2, 3 gì không? Các nút bấm hoặc thuật ngữ ("FSRS", "PKM", "Workspace Classic"...) có gây khó hiểu cho người không rành kỹ thuật không?
2. **Kiểm toán Tap Target & Bố cục Mobile `390×844` + Tablet `768×1024`:**
   - Dùng script Playwright đo `getBoundingClientRect()` của toàn bộ nút, link, tab, input tương tác trên các trang chính để phát hiện **tap target nhỏ hơn `44×44px`** (hoặc `36×36px` trên thanh điều hướng phụ) hay các nút đặt quá sát nhau dễ bấm nhầm bằng ngón tay.
   - Kiểm tra tràn ngang (`scrollWidth > clientWidth`), chữ bị cắt cụt khó hiểu, hoặc thành phần bị che khuất bởi `MobileNav` ở đáy màn hình.
   - Kiểm tra nút **Tìm kiếm** trên `MobileNav`: mở Command Palette trên `390×844`, nhập từ khóa, xem popup có bị tràn hay khó đóng không.
3. **Trải nghiệm học tập cốt lõi bằng một tay trên Mobile & Tablet:**
   - Tạo một khóa học mẫu (trên tài khoản test của bạn) và vào `/app/courses/[id]/learn`:
     - Mở/đóng mục lục bài học, mở bottom-sheet "Sổ tay & Thẻ", gõ ghi chú, lưu lại.
     - Chuyển qua lại các tab (`Bài giảng`, `Thực hành`, `Tài liệu PDF`, `Song song`) trên `390×844` và `768×1024`: chế độ `Song song` trên điện thoại hiển thị thế nào?
   - Vào `/app/review`: lật thẻ FSRS và bấm 4 nút đánh giá (`Again/Hard/Good/Easy`) trên màn `390×844`, kiểm tra khả năng thao tác bằng một tay.
   - Vào `/app/courses/[id]/quiz`: chọn đáp án và nộp bài trên `390×844`.
   - Kiểm tra `/workspace` (Workspace Classic) trên `390×844` và `768×1024`: người dùng điện thoại lỡ bấm vào có dùng được hoặc quay lại `/app` dễ dàng không?

## Định dạng báo cáo bắt buộc (`docs/qa-round2/A-newbie-mobile-report.md`, tiếng Việt)
1. **Kết quả chính & Mức độ thân thiện cho người mới trên Mobile/Tablet.**
2. **Bằng chứng thực thi:** lệnh chính xác + exit code + bảng thống kê tap targets < 44px / kiểm tra tràn màn hình + đường dẫn script & ảnh chụp trong `docs/qa-round2/screenshots/a-newbie-mobile/`.
3. **Danh sách vấn đề (`MOB-01`...):** mức độ (Critical / High / Medium / Low), viewport xảy ra (`390×844` / `768×1024`), bước tái hiện, mong đợi vs thực tế, file/dòng nghi ngờ.
4. **Góp ý như người dùng mới dùng điện thoại:**
   - Những chỗ gây bối rối khi mới tạo tài khoản.
   - Đề xuất luồng onboarding / giải thích thuật ngữ dễ hiểu hơn.
   - Góp ý thiết kế thao tác một tay trên mobile cho tính năng sắp tới (ví dụ: khóa học Tiếng Anh có nút bấm nghe mẫu, giữ để ghi âm phát âm, chép chính tả trên điện thoại).
5. **Chưa kiểm chứng.**
