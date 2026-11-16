# Bài kiểm tra tổng hợp: Kiến trúc API, React State & Trình đọc Studxy

## Câu hỏi trắc nghiệm
*Chọn đáp án đúng nhất cho từng tình huống kỹ thuật dưới đây.*

**Câu 1:** Mã trạng thái HTTP nào biểu thị dữ liệu gửi lên hợp lệ về cấu trúc JSON nhưng vi phạm quy tắc nghiệp vụ của hệ thống?
- A. 200 OK
- B. 404 Not Found
- C. 422 Unprocessable Entity
- D. 502 Bad Gateway

> [!TIP]
> **Đáp án:** C  
> **Giải thích:** 422 Unprocessable Entity cho thấy payload hợp lệ cú pháp nhưng không thỏa điều kiện logic nghiệp vụ.

**Câu 2:** Thuộc tính CSS nào cho phép chặn tràn ngang ở container cha mà vẫn giữ cho thanh bên `position: sticky` bám theo màn hình khi cuộn?
- A. `overflow-x: hidden`
- B. `overflow-x: clip`
- C. `overflow-y: scroll`
- D. `display: inline-block`

> [!TIP]
> **Đáp án:** B  
> **Giải thích:** `overflow-x: clip` không tạo scroll container mới nên `position: sticky` ở phần tử con tiếp tục hoạt động bình thường.

**Câu 3:** Trong `useEffect` tải dữ liệu theo `lessonId`, cặp công cụ nào giúp ngăn kết quả fetch chậm từ bài cũ ghi đè lên bài mới?
- A. `setTimeout` và `localStorage`
- B. `AbortController` kết hợp cờ `isCurrent` trong hàm cleanup
- C. `JSON.stringify` và `window.reload()`
- D. Chỉ dùng `try/catch` thông thường

> [!TIP]
> **Đáp án:** B  
> **Giải thích:** Hủy request qua `controller.abort()` và kiểm tra `isCurrent` đảm bảo dữ liệu lỗi thời bị loại bỏ hoàn toàn.

**Câu 4:** Để Studxy hiển thị câu hỏi tự kiểm tra có thể đóng/mở gọn gàng trong bài Markdown, nên dùng cặp thẻ nào?
- A. `<table>` và `<tr>`
- B. `<details>` và `<summary>`
- C. `<span>` và `<strong>`
- D. `<hr>` và `<br>`

> [!TIP]
> **Đáp án:** B  
> **Giải thích:** Studxy biên dịch `<details><summary>...</summary>...</details>` thành accordion tương tác, kể cả khi đặt lồng bên trong danh sách đánh số.
