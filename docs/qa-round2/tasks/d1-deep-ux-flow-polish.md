Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ D1-DEEP-UX-FLOW-POLISH: Đánh bóng Chiều sâu Luồng Người Dùng & Hoàn thiện Trải Nghiệm Thực Hành Toàn Diện

## Mục tiêu
Người dùng yêu cầu ứng dụng đạt độ hoàn thiện cao về logic, độ mượt của các luồng UI và tính năng thực hành cho tất cả các khóa học (kể cả các môn lý thuyết như Toán, Pháp luật, Học máy, Hệ điều hành). Hãy xử lý dứt điểm các chi tiết tinh tế sau trong `frontend/src/` (KHÔNG sửa `backend/`).

## Các hạng mục bắt buộc thực hiện

### 1. Kết nối liền mạch từ Cuối Bài Đọc sang Phòng Thực Hành (`LessonMarkdownView.tsx` ↔ `CourseLearningWorkspace.tsx`)
- Trong `frontend/src/components/reader/LessonMarkdownView.tsx`:
  - Bổ sung prop tùy chọn `onOpenPractice?: () => void` vào `LessonMarkdownViewProps`.
  - Ở khối gợi ý thực hành cuối bài học (phần `"Sẵn sàng kiểm tra kiến thức bài học?"`), khi có `onOpenPractice`, hiển thị nút bấm rõ ràng kích thước chạm tốt (`min-h-[40px]`): **"Mở Phòng Thực Hành →"** (kèm `data-testid="lesson-end-open-practice-btn"`). Khi người học bấm nút này, chuyển thẳng sang tab `"Thực hành"` và cuộn lên đầu trang.
- Trong `frontend/src/components/learn/CourseLearningWorkspace.tsx`:
  - Truyền prop `onOpenPractice={() => { setCenterTab("practice"); readerScrollRef.current?.scrollTo({ top: 0, behavior: "smooth" }); }}` vào `<LessonMarkdownView />`.

### 2. Nâng cấp `frontend/src/components/learn/LessonPracticeLab.tsx` cho MỌI khóa học
1. **Nhận diện chính xác thêm các khóa Công nghệ / Hệ thống / Khoa học dữ liệu:**
   - Trong `detectPreferredMode`, bổ sung nhận diện các từ khóa không dấu: `hoc may`, `machine learning`, `kien truc may tinh`, `computer architecture`, `he dieu hanh`, `bo nho ao`, `phan trang`, `microservices`, `ddd`, `event sourcing`, `python`, `fastapi`, `nodejs`, `docker`, `kubernetes`, `algorithm`, `thuat toan`, `cau truc du lieu` → trả về `"code"` (`CodingPracticeLab`).
2. **Sửa lỗi khởi tạo chế độ Tâm lý vs Ôn tập Feynman (`recallSubMode`):**
   - Hiện ở `LessonPracticeLab.tsx` đang viết `useState<"psychology" | "feynman">(isPsychologyContext ? "psychology" : "psychology")` — sửa lại thành `isPsychologyContext ? "psychology" : "feynman"` và thêm `useEffect(() => { setRecallSubMode(isPsychologyContext ? "psychology" : "feynman"); }, [isPsychologyContext, lessonTitle]);` để khi đổi từ bài Tâm lý sang bài Toán/Pháp luật sẽ tự chuyển đúng sang tab Ôn tập & Tự kiểm tra, trong khi vẫn giữ 2 nút tab con để người học bấm chuyển tự do bất cứ lúc nào.
   - Bổ sung `data-testid="practice-feynman-input"` lên `<textarea>` tự diễn đạt Feynman và `data-testid="practice-feynman-submit-btn"` lên nút `"Đối chiếu ý cốt lõi"`.
3. **Làm sâu thêm chế độ Ôn tập cho các môn Lý thuyết / Toán / Pháp luật / Khoa học:**
   - Trong chế độ `feynman`, ngoài ô tự viết Feynman và AI góp ý, bổ sung thêm khối **"Thẻ Tự Kiểm Tra Khái Niệm (Lật để nhớ nhanh)"** tự động tạo ra từ `lessonMarkdown` (trích xuất từ mục định nghĩa `**Thuật ngữ**: giải thích` hoặc tiêu đề `##/###` kèm đoạn tóm tắt phía dưới, fallback sang `keyTerms` + tóm tắt bài).
   - Cho phép người học bấm lật xem giải thích từng thẻ và bấm nút **"Lưu thành thẻ Flashcard"** (`onCreateFlashcardQuick`) chỉ với 1 chạm (hiển thị trạng thái `✓ Đã lưu` khi đã bấm).
4. **Phản hồi tức thì trong mọi Phòng Thực Hành:**
   - Trong `LessonPracticeLab.tsx`, bọc `onSaveToNotebook` và `onCreateFlashcardQuick` qua hàm wrapper gọi `notify("Đã lưu vào Sổ tay bài học!")` / `notify("Đã tạo thẻ Flashcard ôn tập!")` (hiển thị banner xác nhận nổi bật ngay phía trên phòng thực hành, kể cả khi người dùng bấm từ `EnglishPracticeLab`, `CodingPracticeLab`, `FinancePracticeLab` hay `PsychologyLifeSkillsLab`).

### 3. Đánh bóng Luồng Học Tập & Đồng bộ Trạng thái (`CourseLearningWorkspace.tsx`)
1. **Chuyển bài mượt mà (`handleSelectLesson`):**
   - Khi chuyển bài học (qua Mục lục TOC, nút Bài trước / Bài tiếp theo, hoặc tự động chuyển sau khi Hoàn thành bài):
     - Tự động cuộn `readerScrollRef.current` về đầu (`scrollTo({ top: 0 })`) để không bị giữ vị trí cuộn giữa chừng của bài cũ.
     - Tự động đóng drawer Mục lục trên mobile/tablet (`setMobileTocOpen(false)`) để người dùng thấy ngay nội dung bài mới chọn.
     - Xóa vùng chọn văn bản cũ (`setSelectedText("")`, `setSelectionCoords(null)`).
2. **Đồng bộ Cache Dashboard & Xử lý thông báo rõ ràng khi hoàn thành bài (`handleMarkComplete`, `handleSaveToNotebook`, `handleCreateFlashcard`, `handleCreateFlashcardQuick`):**
   - Import `invalidateDashboardCache` từ `@/lib/api-client` (kiểm tra tên hàm chính xác trước khi import).
   - Gọi `invalidateDashboardCache()` mỗi khi hoàn thành bài học, lưu sổ tay hoặc tạo flashcard thành công (cùng với `window.dispatchEvent(new Event("swms:data-mutated"))`), đảm bảo quay về `/app` hoặc `/app/courses` thấy ngay phần trăm tiến độ mới, thẻ mới và Streak mới.
   - Khi `POST /api/v1/lessons/{id}/complete` trả `429` hoặc `4xx`, đọc message JSON (`detail` / `message` / `error`) từ backend nếu có để hiển thị thông báo tiếng Việt thân thiện thay vì chỉ hiện mã HTTP khô khan.
3. **Kiểm tra nhanh nút Quay lại & Khung điều hướng các trang chính:**
   - Rà soát nhanh luồng đi từ `/app` → `/app/courses` → `/app/courses/[id]` → `/app/courses/[id]/learn` → `/app/courses/[id]/quiz` → `/app/review` xem có nút bị lệch, dead button hay chữ tiếng Anh sót / học thuật khó hiểu nào không; nếu thấy nhỏ gọn dễ sửa an toàn (không làm gãy E2E `data-testid` hay từ khóa trong `a-simplify-copy.md`) thì làm mượt luôn.

## Kiểm chứng bắt buộc
1. Chạy `cd frontend; npx tsc --noEmit` → exit code `0`.
2. Chạy `cd frontend; npm run test:unit` → exit code `0`.
3. Chạy `cd frontend; npx next build --no-lint` → exit code `0` (sạch 18/18 routes).
4. Ghi báo cáo chi tiết vào `docs/qa-round2/D1-deep-ux-flow-summary.md` (gồm cả mục **Chưa kiểm chứng**).
5. KHÔNG commit git, KHÔNG tự restart server (để điều phối viên commit sạch rồi chạy `a-retest` rebuild + verify toàn diện ngay sau đó), và KHÔNG để lại bất kỳ background task nào.
