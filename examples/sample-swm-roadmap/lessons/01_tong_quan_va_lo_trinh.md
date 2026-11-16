# Bài 1: Bản đồ Lộ trình & Nguyên tắc Học Chủ động

Tài liệu này là **bộ lộ trình mẫu được tối ưu riêng cho trình đọc Studxy**: câu văn gọn, bảng biểu tách bạch thay cho đoạn liệt kê dài, có hộp gợi ý tự kiểm tra thu gọn (`<details>`) và không lẫn ký hiệu kiểm định nội bộ khi học.

> [!IMPORTANT]
> **Nguyên tắc vàng khi tự học với lộ trình mẫu:** Đọc hiểu cơ chế trước, tự gõ lại ví dụ tối giản từ file trống, rồi mở phần gợi ý `<details>` để tự đối chiếu thay vì đọc lướt đáp án trước.

## 1. Bản đồ 4 chặng cốt lõi

| Chặng | Chủ đề trọng tâm | Đầu ra bắt buộc | Dấu hiệu đạt chuẩn |
| --- | --- | --- | --- |
| Chặng 1 | Bản đồ học tập & Chuẩn môi trường | Dự án chạy local, script kiểm tra sẵn sàng | Tự khởi chạy không lỗi cổng/biến môi trường |
| Chặng 2 | Domain Model & REST Contract | Endpoint `POST` / `GET` xử lý đủ 400, 404, 409 | Trả `ProblemDetails` nhất quán, không nuốt lỗi |
| Chặng 3 | React State & Hủy request cũ | Danh sách lọc không nhấp nháy stale data | Chuyển tab nhanh không hiển thị nhầm dữ liệu cũ |
| Chặng 4 | Responsive UI, Checklist & FSRS | Layout sticky chuẩn, thẻ ôn tập từ highlight | Mở trên 1280px và 390px không vỡ khung hoặc double scroll |

## 2. Quy trình biên soạn file Markdown chuẩn để upload lên Studxy

Để file `.md` khi tải lên hiển thị đẹp, mục lục bên phải tự động nhận diện đầy đủ và dễ tạo Flashcard:

- **Chỉ dùng một `# Heading 1`** ở dòng đầu tiên làm tiêu đề bài học.
- **Chia mục rõ bằng `## Heading 2` và `### Heading 3`** — thanh "Trong bài này" sẽ tự động trích xuất thành điều hướng nhanh.
- **Dùng bảng GFM (`| Cột 1 | Cột 2 |`)** cho các ma trận so sánh, lộ trình chặng hoặc thông số API thay vì xuống dòng rời rạc.
- **Dùng Callout GitHub (`> [!TIP]`, `> [!WARNING]`, `> [!IMPORTANT]`)** để làm nổi bật điểm bẫy hay gặp.
- **Dùng `<details><summary>...</summary>...</details>`** cho phần đáp án tự kiểm tra hoặc bài tập tình huống.

## 3. Checklist chuẩn bị trước khi sang Bài 2

- [x] Kiểm tra trình duyệt ở cả chế độ **Đọc tinh gọn** và **Chia đôi màn hình (Split)**.
- [ ] Bôi đen một định nghĩa quan trọng để tạo thử 1 thẻ nhớ **FSRS**.
- [ ] Mở một khối gợi ý trả lời ở phần Mini-Review bên dưới.

## 4. Mini-Review khởi động

1. **Câu hỏi 1:** Vì sao nên dùng bảng Markdown thay vì viết mỗi thuộc tính trên một dòng rời?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>

   **Đáp án gợi ý:** Bảng Markdown giữ tương quan trực diện giữa **Chặng — Trọng tâm — Đầu ra** trên cùng một hàng ngang, giúp mắt quét nhanh cấu trúc tổng thể mà không tạo trang đọc quá dài.
   </details>

2. **Câu hỏi 2:** Mức tiêu đề nào được thanh điều hướng "Trong bài này" tự động lập chỉ mục?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>

   **Đáp án gợi ý:** Các tiêu đề `#`, `##`, `###`, `####` trong nội dung bài sẽ được tự động trích xuất thành mục lục nhảy nhanh (`scroll-mt-20`).
   </details>
