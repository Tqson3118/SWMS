# W01 — Bảng Trước/Sau

## 1. Luồng học và giao diện

| Trước | Sau | Lý do |
| --- | --- | --- |
| Người học chọn Cốt lõi, Đầy đủ hoặc Đào sâu. | Một luồng học đầy đủ, chia phiên; vị trí được lưu theo người dùng, khóa học và bài học. | Kiến thức bắt buộc không bị ẩn sau chế độ; có thể học nhiều phiên mà không đổi điều kiện hoàn thành. |
| Đọc phần Core có thể gọi hoàn thành riêng, cấp 50% XP và mở bài sau. | Hoàn thành bài chỉ xảy ra qua các hoạt động bắt buộc của luồng; XP không còn phụ thuộc lựa chọn mode mới. | Tránh trạng thái “đã hoàn thành” nhưng còn thiếu quiz/thực hành. Dữ liệu Core cũ vẫn được đọc trong record legacy, không tự nâng cấp thành full. |
| Resume lưu `mode` và khôi phục mode. | Resume tiếp tục `step` và vị trí đọc; trường `mode` cũ được bỏ qua nhưng không làm mất record. | Mode không còn là khái niệm runtime. |

## 2. Nội dung W01

| Trước | Sau | Lý do |
| --- | --- | --- |
| “Học nhanh khi mệt” và “Bài đọc sâu” tạo cảm giác hai tầng lựa chọn. | Giữ toàn bộ kiến thức trong cùng bài, mô tả thời lượng theo 4 phiên học. | Chia nhỏ theo thời gian thực tế mà không cắt nội dung thiết yếu. |
| Counseling psychology được mô tả như phục vụ người “tâm lý khỏe mạnh”. | Nêu vùng giao nhau giữa clinical/counseling; phạm vi phụ thuộc đào tạo, giấy phép và hệ thống hành nghề. | Tránh nhị nguyên chuyên môn sai lệch. |
| Chi tiết quyền kê đơn theo APA 2014 và hai bang Hoa Kỳ xuất hiện như thông tin dễ khái quát. | Chỉ giữ nguyên tắc: psychiatrist là bác sĩ y khoa; quyền kê đơn phụ thuộc luật, giấy phép và nơi hành nghề. | Thông tin pháp lý thay đổi theo nơi và thời gian. |
| Bốn mục tiêu được trình bày như thang khó tăng dần. | Bốn loại câu hỏi bổ trợ, không phải thang khó phổ quát. | Phù hợp hơn với phương pháp luận và tránh kết luận vượt nguồn. |
| Ví dụ “chim hót vì vui” dễ đọc như trạng thái tâm trí đo trực tiếp được. | Giải thích đây là vấn đề thao tác hóa; chỉ báo là phép đo thay thế và có giới hạn suy luận. | Không tuyệt đối hóa suy luận về tâm trí động vật/con người. |
| Cuối bài nói W02 “sắp biên soạn”. | Link trực tiếp tới bài đọc, quiz và answer key W02 hiện có. | Lấy trạng thái từ tài nguyên hiện hành. |

## 3. Hồ sơ kiểm chứng

- Claim `C01-10` và `C01-11` đã được thu hẹp trong source ledger để không công bố phân đôi counseling/clinical hoặc mốc pháp lý cũ như quy định toàn cầu.
- `manifest.yaml` đã cập nhật SHA-256/byte length của lesson, quiz, answer key và source ledger sau khi sửa; `RealManifestValidationTest.Validate_RealManifest_ReturnsPass` pass.
- Claim mới về tách khoa học và đạo đức được ghi rõ là nguyên tắc sư phạm, chưa gắn nhãn bằng chứng độc lập.

## 4. Giới hạn còn lại

- Frontend test/build đã chạy trong worktree tích hợp; chưa chạy browser E2E đầy đủ vì môi trường chưa khởi động backend/frontend live.
- Bộ test ContentContract đầy đủ còn có các test validator độc lập đang fail do expected behavior không khớp implementation hiện tại; không sửa ngoài phạm vi W01.
- Các bài khác chưa được đổi giọng văn hoặc trạng thái; chỉ inventory và pipeline đã được đối chiếu để tránh nhân rộng trước khi người dùng duyệt W01.
