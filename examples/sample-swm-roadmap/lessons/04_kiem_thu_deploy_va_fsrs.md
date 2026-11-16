# Bài 4: Checklist Hoàn thiện UI, Kiểm thử & Hệ thống FSRS

Kiến thức chỉ thực sự ở lại khi bạn kết hợp **trải nghiệm đọc không ma sát** với **chu kỳ ôn tập chủ động FSRS**.

## 1. Bảng chẩn đoán các lỗi CSS/UX kinh điển ở trang đọc dài

| Triệu chứng gặp phải | Nguyên nhân gốc | Cách khắc phục chuẩn |
| --- | --- | --- |
| Sidebar `sticky` trôi tuột theo trang khi cuộn | Phần tử cha phía trên dùng `overflow-x: hidden` | Đổi phần tử cha sang `overflow-x: clip` (giữ `sticky` hoạt động) |
| Xuất hiện 2 thanh cuộn dọc lồng nhau ở sidebar | Thẻ `<aside>` ngoài lẫn danh sách cây bên trong đều đặt `overflow-y-auto` | Chỉ giữ `overflow-hidden` ở khung ngoài, để cây nội dung `flex-1 min-h-0 overflow-y-auto` |
| Bấm chuyển bài xong vẫn đứng ở cuối trang | Không reset cuộn của `window` / `main` / container đọc | Gọi reset scroll về `0` trong `useEffect([activeLessonId])` |
| Trang học quá nhiều chữ ký hiệu kỹ thuật lúc đang đọc | Gắn lẫn metadata kiểm định ngay trong văn bản học | Tách sang nút bật/tắt **Đọc tinh gọn** để mặc định ẩn khi học và hiện khi kiểm tra nguồn |

## 2. Cách khai thác bộ thẻ FSRS đi kèm gói SWM này

Khi nhập gói ZIP này vào Studxy:

1. Toàn bộ bài học được dựng thành 2 chương gọn gàng kèm mục lục nhảy nhanh.
2. **6 thẻ Flashcard FSRS** trong `flashcards/deck-sample.yaml` được nạp sẵn vào **Trạm Ôn tập**.
3. **4 câu hỏi trắc nghiệm** trong `quizzes/quiz-sample.md` sẵn sàng kiểm tra nhanh sau khi học xong Chương 2.

> [!TIP]
> Nếu bạn muốn xuất lại lộ trình của riêng mình sang `.zip`, hãy tuân thủ đúng 3 thư mục: `lessons/*.md`, `flashcards/*.yaml`, `quizzes/*.md` cùng file `manifest.yaml` ở gốc.

## 3. Tự kiểm tra tổng kết khóa học

1. **Câu hỏi 1:** Thuộc tính CSS nào thay thế `overflow-x: hidden` để chặn tràn ngang mà không làm gãy `position: sticky`?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>

   **Đáp án gợi ý:** Dùng **`overflow-x: clip`** (trong Tailwind v4 là `overflow-x-clip`). Khác với `hidden`, `clip` không tạo ra một scroll container mới nên tổ tiên không vô hiệu hóa `position: sticky` của phần tử con.
   </details>

2. **Câu hỏi 2:** Làm sao đánh giá độ khó thẻ nhớ trong thuật toán FSRS để tối ưu lịch ôn tập?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>

   **Đáp án gợi ý:** Sau khi lật mặt sau, đánh giá trung thực theo 4 mức **Again / Hard / Good / Easy** — FSRS v5 sẽ tự điều chỉnh **Stability (S)** và **Difficulty (D)** thay cho lịch cố định truyền thống.
   </details>
