Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ A-HACKER: Đóng vai hacker, kiểm thử bảo mật (CHỈ TEST + BÁO CÁO, KHÔNG SỬA CODE ỨNG DỤNG)

Lưu ý môi trường: một agent khác đang redesign frontend song song và thỉnh thoảng restart server. Nếu gặp connection refused, đợi 30–60s rồi thử lại (không tính là lỗi). Tập trung vào **backend API** (http://127.0.0.1:5080 và qua proxy http://127.0.0.1:3000/api/v1/*); phần UI chỉ dùng khi cần (XSS hiển thị).

Bạn là hacker mũ trắng muốn chiếm tài khoản, đọc dữ liệu người khác, leo quyền. Chỉ tấn công tài khoản test DO BẠN TỰ TẠO (ít nhất 2 tài khoản: victim và attacker). Không DoS làm sập server, không phá dữ liệu Sơn Trần/Minh Anh.

Đọc `backend/src/Swms.Api/Features/SwmsEndpoints.cs`, `Program.cs`, `Infrastructure/Security/*` để lập bản đồ endpoint, sau đó thử thực tế:
1. Auth: brute-force/rate-limit login, user enumeration (thông báo lỗi khác nhau), độ mạnh mật khẩu, JWT (alg none, đổi claim, token hết hạn, token sau logout, refresh token replay/RTR), lưu token trong localStorage (B1).
2. IDOR/BOLA: mọi endpoint có id (course, lesson, document, file stream range, notebook, flashcard, quiz, export) — attacker đọc/sửa/xóa tài nguyên của victim.
3. Injection: SQL (dù EF Core), path traversal ở upload/stream/export, XSS stored qua tên khóa học/ghi chú/markdown bài học/tên file PDF, SSRF nếu có import URL, ZIP slip ở import ZIP, file upload sai MIME/kích thước khổng lồ (giới hạn hợp lý, không làm đầy ổ).
4. Headers & CORS (B3, B4): kiểm tra thực tế response headers và CORS với Origin lạ.
5. Mass assignment: gửi thêm field (userId, role, quota, isAdmin) trong body.
6. Lộ thông tin: stack trace, endpoint debug/seed/demo dùng được trên prod, swagger, `.env`.

Mỗi lỗ hổng: PoC bằng script Node/PowerShell trong `docs/qa-round2/scripts/a-hacker/` (chạy được lại), mức độ theo CVSS sơ bộ, file/dòng nghi ngờ, đề xuất fix.
Cuối báo cáo thêm mục **"Góp ý như người dùng"**: cảm nhận về sự an toàn/riêng tư, tính năng bạn muốn có (vd xóa tài khoản, xuất dữ liệu, 2FA...).

Báo cáo cuối: `docs/qa-round2/A-hacker-report.md`. Chỉ commit trong `docs/qa-round2/` (`chore(qa): A hacker report`).
