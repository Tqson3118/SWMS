# Bài 3: Quản trị State React & Chống Stale Request

Khi người dùng bấm chuyển nhanh từ Bài 1 sang Bài 2 rồi sang Bài 3, request mạng của Bài 1 có thể trả về chậm hơn Bài 3 — nếu không hủy request cũ, màn hình sẽ hiển thị nhầm nội dung của Bài 1 dưới tiêu đề Bài 3 (**Race Condition / Stale UI**).

## 1. Bảng phân loại State trước khi viết code

| Loại State | Nguồn sự thật | Công cụ phù hợp | Ví dụ cụ thể |
| --- | --- | --- | --- |
| Điều hướng, filter hiện tại | Thanh địa chỉ (URL) | Next.js Router / `useSearchParams` | `?courseId=...&mode=split` |
| Dữ liệu từ Database | Backend API | `fetch` + `AbortController` hoặc TanStack Query | Nội dung bài học, danh sách Flashcard |
| Tùy chọn cá nhân ở client | Browser Storage | Zustand `persist` | Cỡ chữ, Serif/Sans, chế độ Đọc tinh gọn |
| Trạng thái tạm của form | Component cục bộ | `useState` | Ô tìm kiếm chương, modal đang mở |

## 2. Mẫu `useEffect` chuẩn với `AbortController`

```tsx
useEffect(() => {
  const controller = new AbortController();
  let isCurrent = true;

  async function loadLessonDetail() {
    setLoading(true);
    try {
      const res = await fetch(`/api/v1/lessons/${lessonId}`, {
        signal: controller.signal,
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      if (isCurrent) setLesson(data);
    } catch (err) {
      if ((err as Error).name !== "AbortError" && isCurrent) {
        setError("Không thể tải nội dung bài học.");
      }
    } finally {
      if (isCurrent) setLoading(false);
    }
  }

  loadLessonDetail();
  return () => {
    isCurrent = false;
    controller.abort();
  };
}, [lessonId]);
```

> [!WARNING]
> Luôn reset vị trí cuộn (`window.scrollTo(0, 0)` và thanh cuộn container) mỗi khi `lessonId` thay đổi hoặc khi bấm "Đánh dấu hoàn thành & Tiếp tục", nếu không người học sẽ rơi vào phần cuối trống của bài tiếp theo.

## 3. Mini-Review phản xạ

1. **Câu hỏi 1:** Nếu đã có `AbortController`, vì sao vẫn nên giữ biến cờ `isCurrent` trong cleanup?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>

   **Đáp án gợi ý:** Trong trường hợp response đã hoàn tất `res.json()` đúng microtask trước khi cleanup kịp hủy fetch, cờ `isCurrent = false` chặn triệt để việc gọi `setLesson` cho ID đã lỗi thời.
   </details>

2. **Câu hỏi 2:** Vì sao URL thường tốt hơn `useState` cho lựa chọn khóa học/chương đang mở?
   <details>
   <summary>👉 Xem gợi ý trả lời</summary>

   **Đáp án gợi ý:** Giữ state trên URL cho phép bấm Refresh (`F5`), Back/Forward trình duyệt hoặc gửi link trực tiếp cho người khác mà vẫn mở đúng bài học đó.
   </details>
