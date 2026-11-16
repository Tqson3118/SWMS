# Giao diện nhập chi tiêu Stage 8

Chạy `npm install`, sau đó `npm test` và `npm run build`. Khi phát triển, khởi động API trên cổng loopback mà API đã chọn rồi đặt `VITE_API_ORIGIN=http://127.0.0.1:<cổng-đã-in-ra>` trước `npm run dev`; không có mặc định cứng tới cổng 5000. Không tự động thử lại POST.

Giao diện giữ tệp đã chọn bất biến trong một lần nhập, chặn POST chồng nhau, hủy callback khi unmount và bỏ qua phản hồi cũ. Tiền và ngày giữ dạng chuỗi từ DTO. `committed-with-postcommit-error` vẫn là đã commit; `cancelled` là trạng thái máy chủ xác nhận; hủy trình duyệt, lỗi mạng, phản hồi không nhất quán và `unknown` hiển thị chưa xác nhận. Làm mới danh sách chỉ cung cấp dữ liệu đối chiếu; một nút quyết định rõ ràng mới mở khóa lần POST tiếp theo. Không tự động suy ra kết quả hoặc thử lại POST.

Kiểm thử jsdom và build TypeScript đã chạy. Browser thật, SQL mới và transport uncertainty chưa được xác nhận trong môi trường này; không có claim về auth, idempotency hay retry.
