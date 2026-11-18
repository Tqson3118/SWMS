Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ B2-CODING-PRACTICE: Xây dựng Module Thực Hành Lập Trình & Hệ Thống Chuyên Sâu

## Quy tắc phân vùng chạy song song (BẮT BUỘC)
- **PHẠM VI ĐƯỢC PHÉP TẠO / SỬA:**
  - CHỈ tạo/sửa file: `frontend/src/components/learn/practice/CodingPracticeLab.tsx`
  - Và ghi báo cáo ngắn vào: `docs/qa-round2/B2-coding-summary.md`
- **TUYỆT ĐỐI KHÔNG** sửa bất kỳ file nào khác trong `frontend/` hoặc `backend/` (đặc biệt KHÔNG chạm vào `LessonPracticeLab.tsx`, `LessonMarkdownView.tsx`, `tsconfig.tsbuildinfo`).
- **KHÔNG chạy `git commit`**, **KHÔNG restart server**, **KHÔNG để lại background task**.
- Kiểm tra TypeScript bằng: `cd frontend; npx tsc --noEmit --incremental false` (exit code `0`).

## Chuẩn Props & Giao diện
Export component `CodingPracticeLab` nhận props chuẩn khớp với `LessonPracticeLab.tsx`:
```ts
export interface PracticeModuleProps {
  courseId: string;
  courseTitle: string;
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  onSaveToNotebook: (textToAppend: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
}
```
- **Ngôn ngữ hiển thị trên UI:** Tiếng Việt dễ hiểu, rõ ràng, giải thích thực tế không hàn lâm khó hiểu.
- **Thiết kế "Clean & Calm Learning":** Tương thích Light Mode, Dark Mode, và Mobile `390×844` (code block cuộn ngang mượt `overflow-x-auto`, nút bấm `min-h-[44px]`).

## 3 Chế độ thực hành chuyên sâu cần xây dựng trong `CodingPracticeLab.tsx`

### Thích ứng theo bài học (`.NET / C#`, `React / TypeScript`, `SQL / EF Core`, `Hệ điều hành`)
- Tự động đọc `courseTitle`, `lessonTitle` và các block code ` ```...``` ` trong `lessonMarkdown` để chọn nhóm bài tập sát với bài đang học nhất, đồng thời cho phép người học chuyển qua các chủ đề khác.

### Chế độ 1: Điền Code Còn Thiếu & Giải Thích Từng Dòng
- Trích xuất đoạn code thực tế từ `lessonMarkdown` (hoặc dùng các bài tập chuẩn về `ASP.NET Core Web API`, `EF Core AsNoTracking / Include`, `React useState/useEffect`, `SQL JOIN/Index`, `Async/Await`) nếu bài học chưa có block code dài.
- Giao diện giống trình soạn code gọn gàng có đánh số dòng, highlight từ khóa cơ bản bằng CSS/JSX thuần (không cài thêm thư viện ngoài):
  - Có 2–3 chỗ trống quan trọng để người học **tự gõ** hoặc **chọn nhanh mảnh code gợi ý** (rất tiện trên điện thoại).
  - Bấm **"Kiểm tra kết quả"** → chấm từng chỗ trống, mở phần **Giải thích từng dòng** để người học hiểu tại sao phải viết như vậy.

### Chế độ 2: Săn Lỗi Thực Chiến (Bug Hunter — Chạm vào dòng gây lỗi)
- Kho 6+ tình huống lỗi kinh điển sát giáo trình Full-Stack & Hệ điều hành:
  1. C# / EF Core: Quên `AsNoTracking()` hoặc lỗi **N+1 Query** trong vòng lặp.
  2. ASP.NET Core / Concurrency: **Race condition** khi trừ số dư / kiểm tra quota không có lock hoặc transaction.
  3. React Hooks: **Stale closure / Infinite loop** trong `useEffect` thiếu dependency hoặc quên cleanup `clearInterval`.
  4. SQL Security: Cộng chuỗi SQL trực tiếp thay vì parameterized query.
  5. React Memory / Render: Gọi `fetch` liên tục hoặc không hủy `AbortController` khi unmount.
  6. C# Async: Dùng `.Result` / `.Wait()` gây **Deadlock**.
- Người học đọc đoạn code 8–14 dòng → **bấm trực tiếp vào dòng nghi ngờ có bug** → chọn phương án sửa đúng → xem phân tích hậu quả trên thực tế và đoạn code sau khi đã sửa sạch.

### Chế độ 3: Mô Phỏng Luồng Chạy Hệ Thống (Interactive Visualizer)
- Chọn tự động theo ngữ cảnh bài học:
  1. **Mô phỏng Đường đi Request Web Full-Stack:** Cho người học tự sắp xếp hoặc bấm **"Chạy từng bước"** qua các tầng (`Browser React` → `Rate Limit / Auth` → `Validation` → `Cache / Service` → `Database`) kèm mô tả dữ liệu thay đổi ở mỗi bước và xử lý lỗi khi 1 tầng thất bại.
  2. **Mô phỏng Bộ nhớ ảo & TLB (cho bài Hệ điều hành):** Thanh trượt điều chỉnh tỷ lệ **TLB Hit (50%–99%)**, thời gian đọc TLB & RAM → tính trực tiếp **Thời gian truy cập trung bình (EAT)** và mô phỏng trực quan luồng tìm trang (`CPU` → `TLB` → `Bảng trang` → `RAM`).
- Kèm nút **"Lưu ghi chú code vào Sổ tay"** (`onSaveToNotebook`) và **"Tạo thẻ ghi nhớ lỗi hay gặp"** (`onCreateFlashcardQuick`).

## Kiểm chứng
1. Chạy `cd frontend; npx tsc --noEmit --incremental false` → bắt buộc exit code `0`.
2. Ghi báo cáo ngắn vào `docs/qa-round2/B2-coding-summary.md` nêu rõ 3 chế độ, danh sách tình huống Bug Hunter, kết quả `tsc` và mục **Chưa kiểm chứng**.
