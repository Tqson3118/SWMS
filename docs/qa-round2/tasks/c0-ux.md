Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ C0-UX: Kiểm kê giao diện + audit UX trước redesign (CHỈ XEM/CHỤP, KHÔNG SỬA CODE ỨNG DỤNG)

Bối cảnh: sắp làm lại TOÀN BỘ design system theo phong cách **"Clean & Calm Learning"**: light-first (dark mode đầy đủ), nền thoáng, 1 màu nhấn tươi (giữ tím thương hiệu nhưng sáng/trong hơn), 1 màu phụ cho thành công/streak, bo góc lớn, typography dễ đọc lâu, micro-animation vui nhẹ (pha Duolingo + Notion). Bạn chuẩn bị đầu vào cho bước redesign.

1. Bằng Playwright, chụp ảnh full-page MỌI route (xem `frontend/src/app/**/page.tsx`) ở 3 viewport 1440x900, 768x1024, 390x844, cả light và dark mode. Đăng nhập demo "Sơn Trần" cho trang `/app/*` và `/workspace`; chụp thêm trạng thái trống bằng "Minh Anh". Chụp các trạng thái quan trọng: modal tạo khóa học, drawer "Sổ tay & Thẻ" trong trang learn, thẻ ôn tập lật mặt sau, quiz, command palette Ctrl+K.
   Ảnh: `docs/qa-round2/screenshots/c0-before/<route>-<viewport>-<theme>.png`.
2. Kiểm kê design system hiện tại: đọc `frontend/src/app/globals.css`, tailwind config, `frontend/src/components/**`. Liệt kê: token màu/spacing/radius/shadow/font đang dùng, các component lặp lại (Button, Card, Input, Badge, Modal, Tabs, Sidebar, ...), chỗ dùng màu/kích thước hard-code không theo token, chỗ không nhất quán.
3. Audit UX đóng vai người dùng thật, ghi lỗi + góp ý. Người điều phối đã thấy sẵn các vấn đề sau, hãy xác nhận và tìm thêm:
   - Khóa "IELTS Mastery Roadmap" bị gán danh mục "Công nghệ & Lập trình".
   - Bài học IELTS hiển thị markdown thô (`**Đọc`), link tới file `.md` nội bộ (implementation_plan_new.md, roadmap.md) không mở được, lộ nội dung nội bộ "(AI executor) ... HANDOFF.md".
   - Dashboard: 1508 thẻ đến hạn / ~754 phút trong 1 ngày → quá tải, thiếu giới hạn thẻ mới/ngày.
   - "Chào Trần," lấy họ thay tên. Các ô thống kê có khoảng trống lớn. Lịch sử kiểm tra toàn 0/3.
   - Các bug UI trong `docs/qa-round2/OLD_REPORT.md`: B2, B5, B8, B10, B11, B16 (kiểm tra còn hay đã hết).
4. Đề xuất cụ thể cho redesign: bảng màu (hex cho light & dark, kiểm contrast WCAG AA), thang chữ, spacing, radius, shadow, motion (duration/easing, tôn trọng `prefers-reduced-motion`), danh sách component nền cần xây, và thứ tự trang cần làm.

Báo cáo cuối: `docs/qa-round2/C0-ux-audit.md`. Chỉ commit trong `docs/qa-round2/` (`chore(qa): C0 UX audit`). Nếu ảnh quá nhiều (>40MB) thì KHÔNG commit thư mục ảnh, chỉ commit báo cáo.
