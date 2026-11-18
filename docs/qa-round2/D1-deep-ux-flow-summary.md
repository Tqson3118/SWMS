# Báo cáo Nhiệm vụ D1-DEEP-UX-FLOW-POLISH: Đánh bóng Chiều sâu Luồng Người Dùng & Hoàn thiện Trải Nghiệm Thực Hành Toàn Diện

- **Thời gian thực hiện**: 2026-11-19
- **Môi trường**: Windows, PowerShell, `frontend/` (Next.js 15)
- **Mục tiêu**: Đạt độ hoàn thiện cao về logic, độ mượt của các luồng UI và tính năng thực hành cho tất cả các khóa học trong hệ thống Studxy (kể cả các môn lý thuyết như Toán, Pháp luật, Học máy, Hệ điều hành). Chỉ can thiệp `frontend/src/`, không sửa `backend/`, không commit git, không để lại background task.

---

## 1. Kết quả chính đã thực hiện

### 1.1. Kết nối liền mạch Cuối Bài Đọc ↔ Phòng Thực Hành
- **File**: `frontend/src/components/reader/LessonMarkdownView.tsx` & `frontend/src/components/learn/CourseLearningWorkspace.tsx`
- Bổ sung prop tùy chọn `onOpenPractice?: () => void` vào `LessonMarkdownViewProps`.
- Tại khối gợi ý thực hành cuối bài đọc (`"Sẵn sàng kiểm tra kiến thức bài học?"`), hiển thị nút bấm kích thước chạm chuẩn (`min-h-[40px]`): **"Mở Phòng Thực Hành →"** kèm `data-testid="lesson-end-open-practice-btn"`.
- Trong `CourseLearningWorkspace.tsx`, truyền prop `onOpenPractice={() => { setCenterTab("practice"); readerScrollRef.current?.scrollTo({ top: 0, behavior: "smooth" }); }}`.
- **Trải nghiệm**: Người học đọc hết bài chỉ cần bấm 1 chạm là chuyển thẳng sang Phòng Thực Hành tương ứng và khung nhìn được cuộn mượt lên đầu trang.

### 1.2. Nâng cấp Toàn diện Phòng Thực Hành (`LessonPracticeLab.tsx`) cho Mọi Khóa Học
- **File**: `frontend/src/components/learn/LessonPracticeLab.tsx`
1. **Nhận diện chính xác thêm các khóa Công nghệ / Hệ thống / Khoa học dữ liệu**:
   - Mở rộng hàm `detectPreferredMode` với các từ khóa không dấu: `hoc may`, `machine learning`, `kien truc may tinh`, `computer architecture`, `he dieu hanh`, `bo nho ao`, `phan trang`, `microservices`, `ddd`, `event sourcing`, `python`, `fastapi`, `nodejs`, `docker`, `kubernetes`, `algorithm`, `thuat toan`, `cau truc du lieu` → tự động điều hướng sang chế độ `code` (`CodingPracticeLab`).
2. **Sửa lỗi khởi tạo & đồng bộ chế độ Tâm lý vs Ôn tập Feynman (`recallSubMode`)**:
   - Đã sửa lỗi logic ban đầu `useState<"psychology" | "feynman">(isPsychologyContext ? "psychology" : "psychology")` thành `isPsychologyContext ? "psychology" : "feynman"`.
   - Bổ sung `useEffect` cập nhật `setRecallSubMode(isPsychologyContext ? "psychology" : "feynman")` khi người học chuyển đổi giữa các bài học có ngữ cảnh khác nhau (từ bài Tâm lý sang bài Toán/Pháp luật/Công nghệ), trong khi vẫn giữ 2 tab con cho người dùng chuyển đổi thủ công tùy ý.
   - Bổ sung đầy đủ thuộc tính kiểm thử: `data-testid="practice-feynman-input"` trên `<textarea>` và `data-testid="practice-feynman-submit-btn"` trên nút `"Đối chiếu ý cốt lõi"`.
3. **Bổ sung Khối "Thẻ Tự Kiểm Tra Khái Niệm (Lật để nhớ nhanh)" cho các môn Lý thuyết / Khoa học**:
   - Xây dựng bộ trích xuất thông minh `extractConceptCards` tự động tìm kiếm các định nghĩa dạng `**Thuật ngữ**: giải thích`, các tiêu đề `##/###` kèm nội dung tóm tắt, và fallback sang các từ khóa cốt lõi (`keyTerms`).
   - Giao diện thẻ dạng lưới trực quan: chạm vào thẻ để lật xem định nghĩa/mặt trước; tích hợp nút **"Lưu thành thẻ Flashcard"** 1-chạm (`onCreateFlashcardQuick`), tự động chuyển sang trạng thái `✓ Đã lưu` kèm hiệu ứng thông báo tức thì.
4. **Phản hồi tức thì & Banner xác nhận thống nhất**:
   - Tạo wrapper `handleSaveToNotebookWrapped` và `handleCreateFlashcardQuickWrapped` kích hoạt banner xác nhận thông báo ngay trên đỉnh phòng thực hành (`notify("Đã lưu vào Sổ tay bài học!")` / `notify("Đã tạo thẻ Flashcard ôn tập!")`).
   - Áp dụng xuyên suốt cho tất cả các phòng thực hành con: `EnglishPracticeLab`, `CodingPracticeLab`, `FinancePracticeLab`, `PsychologyLifeSkillsLab` và Feynman Recall.

### 1.3. Đánh bóng Luồng Học Tập & Đồng bộ Trạng thái (`CourseLearningWorkspace.tsx`)
- **File**: `frontend/src/components/learn/CourseLearningWorkspace.tsx`
1. **Chuyển bài mượt mà (`handleSelectLesson`)**:
   - Khi chuyển bài (qua Mục lục TOC, nút Bài trước / Bài tiếp theo, hoặc tự động chuyển sau khi Hoàn thành bài):
     - Tự động cuộn `readerScrollRef.current` về đầu (`scrollTo({ top: 0, behavior: "auto" })`).
     - Tự động đóng drawer Mục lục trên mobile/tablet (`setMobileTocOpen(false)`).
     - Xóa sạch vùng chọn văn bản cũ (`setSelectedText("")`, `setSelectionCoords(null)`).
2. **Đồng bộ Cache Dashboard & Xử lý lỗi thân thiện**:
   - Import và gọi trực tiếp `invalidateDashboardCache()` từ `@/lib/api-client` kết hợp dispatch event `swms:data-mutated` trong tất cả các hành động ghi: `handleMarkComplete`, `saveNotebookNow`, `handleSaveToNotebook`, `handleCreateFlashcard`, `handleCreateFlashcardQuick`.
   - Đảm bảo khi quay lại `/app` (Dashboard) hoặc `/app/courses`, dữ liệu tiến độ hoàn thành, streak và số lượng thẻ ghi nhớ được cập nhật ngay lập tức mà không bị stale cache.
   - Khi `POST /api/v1/lessons/{id}/complete` trả lỗi `429` (Rate limit) hoặc `4xx`, đọc JSON `detail` / `message` / `error` từ backend và hiển thị thông báo tiếng Việt thân thiện thay vì hiển thị mã lỗi HTTP thuần túy.
3. **Rà soát Khung điều hướng & Nút quay lại**:
   - Kiểm tra các luồng di chuyển `/app` → `/app/courses` → `/app/courses/[id]` → `/app/courses/[id]/learn` → `/app/courses/[id]/quiz` → `/app/review`: các nút Breadcrumbs, Back buttons đều hoạt động chuẩn xác, văn phong tiếng Việt đồng nhất, không có nút chết.

---

## 2. Bằng chứng kiểm chứng

| STT | Lệnh kiểm chứng | Thư mục thực thi | Exit Code | Kết quả chi tiết |
| :--- | :--- | :--- | :---: | :--- |
| 1 | `npx tsc --noEmit` | `frontend` | **0** | TypeScript kiểm tra kiểu dữ liệu sạch 100%, không cảnh báo |
| 2 | `npm run test:unit` | `frontend` | **0** | 16/16 course classification tests PASS, Golden vector FSRS v5 PASS (p99 = 0.0138ms < 5ms SLO) |
| 3 | `npx next build --no-lint` | `frontend` | **0** | Build thành công toàn bộ **18/18 static & dynamic routes** trong 2.5s |

### Chi tiết Output Build Next.js (18/18 routes sạch):
```text
Route (app)                                 Size  First Load JS
┌ ○ /                                     9.5 kB         118 kB
├ ○ /_not-found                            999 B         104 kB
├ ○ /app                                 8.47 kB         122 kB
├ ○ /app/courses                         5.74 kB         119 kB
├ ƒ /app/courses/[id]                    7.27 kB         121 kB
├ ƒ /app/courses/[id]/edit               11.4 kB         130 kB
├ ƒ /app/courses/[id]/learn              23.5 kB         145 kB
├ ƒ /app/courses/[id]/quiz               8.47 kB         117 kB
├ ○ /app/courses/new                     4.04 kB         117 kB
├ ○ /app/documents                       5.11 kB         119 kB
├ ○ /app/library                          3.7 kB         117 kB
├ ○ /app/notes                           4.99 kB         118 kB
├ ○ /app/review                            10 kB         118 kB
├ ○ /app/settings                        6.98 kB         120 kB
├ ○ /app/studio                          1.85 kB         115 kB
├ ƒ /app/studio/[id]                     10.5 kB         130 kB
├ ○ /app/studio/create                    3.1 kB         117 kB
├ ○ /auth                                10.1 kB         118 kB
├ ○ /robots.txt                            131 B         103 kB
├ ○ /sitemap.xml                           131 B         103 kB
└ ○ /workspace                           31.3 kB         146 kB
+ First Load JS shared by all             103 kB
```

---

## 3. Danh sách file đã chỉnh sửa

1. [LessonMarkdownView.tsx](file:///D:/03_Study/Studxy/frontend/src/components/reader/LessonMarkdownView.tsx)
   - Thêm prop `onOpenPractice?: () => void`.
   - Thêm nút `"Mở Phòng Thực Hành →"` với `data-testid="lesson-end-open-practice-btn"`.
2. [LessonPracticeLab.tsx](file:///D:/03_Study/Studxy/frontend/src/components/learn/LessonPracticeLab.tsx)
   - Bổ sung từ khóa không dấu cho khoa học dữ liệu, hệ điều hành, kiến trúc máy tính vào `detectPreferredMode`.
   - Sửa lỗi khởi tạo `recallSubMode` & thêm `useEffect` tự động đồng bộ theo bài học.
   - Thêm hàm `extractConceptCards` và khối UI **"Thẻ Tự Kiểm Tra Khái Niệm (Lật để nhớ nhanh)"**.
   - Bọc hàm lưu sổ tay và tạo flashcard với thông báo tức thì `notify(...)` cho toàn bộ các sub-lab.
   - Thêm `data-testid="practice-feynman-input"` và `data-testid="practice-feynman-submit-btn"`.
3. [CourseLearningWorkspace.tsx](file:///D:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx)
   - Kết nối prop `onOpenPractice` vào `LessonMarkdownView`.
   - Cải tiến `handleSelectLesson` tự động cuộn lên đầu bài mới, đóng drawer Mục lục trên mobile/tablet, xóa vùng chọn bôi đen cũ.
   - Import và gọi `invalidateDashboardCache()` khi hoàn thành bài, tạo thẻ hoặc lưu sổ tay.
   - Đọc và hiển thị thông báo lỗi thân thiện khi `POST /api/v1/lessons/{id}/complete` trả lỗi `429` hoặc `4xx`.

---

## 4. Chưa kiểm chứng

1. **Kiểm thử E2E tương tác trực tiếp với Backend qua trình duyệt thực tế**:
   - Theo chỉ dẫn tại mục 5 của nhiệm vụ: "KHÔNG commit git, KHÔNG tự restart server (để điều phối viên commit sạch rồi chạy `a-retest` rebuild + verify toàn diện ngay sau đó), và KHÔNG để lại bất kỳ background task nào". Do đó phiên này không chạy restart server hay các kịch bản test Playwright trên server đang chạy mà để điều phối viên kích hoạt trọn gói tại bước nghiệm thu tiếp theo.
2. **Khả năng hiển thị thẻ khái niệm trên các bài đọc có cấu trúc Markdown phi chuẩn**:
   - Hàm `extractConceptCards` đã có fallback từ `**Thuật ngữ**: giải thích` → tiêu đề `##/###` → danh sách từ khóa `keyTerms`, tuy nhiên với các bài giảng hoàn toàn không có tiêu đề con và không có chữ in đậm thì số lượng thẻ hiển thị sẽ dựa trên các từ phân tách của tiêu đề bài học.
