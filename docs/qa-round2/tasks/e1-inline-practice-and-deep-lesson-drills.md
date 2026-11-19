Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ E1-INLINE-PRACTICE-AND-DEEP-LESSON-DRILLS: Đưa Phòng Thực Hành & Bài Tập Thật Vào Ngay Trong Bài Đang Học

## Bối cảnh vấn đề người dùng phản ánh
Người dùng kiểm tra trực tiếp trên web (`http://127.0.0.1:3000`) và nhận xét: *"thực hành có tý, muốn trong khóa học đang học là nó có luôn"*.
Nguyên nhân thực tế qua rà soát mã nguồn và database `swms_local_dev.db`:
1. Khi mở bài học (`/app/courses/[id]/learn` mặc định là tab `"Bài giảng"` hoặc `/workspace`), toàn bộ `<LessonPracticeLab />` bị ẩn sang tab `"Thực hành"` riêng; người dùng chỉ thấy một banner nhỏ 2 dòng ở cuối bài.
2. Các mục thực hành viết sẵn trong bài giảng (bảng từ vựng/IPA, *Minimal pairs*, *Bài tập trọng tâm đọc to*, thẻ `<details>` *Câu hỏi 1/2*, *Mini-review 1 & 2*, *Bài tập tư duy*, *Worked Example*) mới hiển thị như văn bản tĩnh.
3. Database đã có sẵn **1.586 câu Quiz thật** (FullStack 364, Đầu tư 234, Tài chính cá nhân 156, Kỹ năng sống 208, Tâm lý học 624) và **1.383 thẻ Flashcard thật** gắn theo từng khóa học/bài học, nhưng chưa được tải và hiển thị trực tiếp ngay trong trang bài đang học!
4. `EnglishPracticeLab` đang cắt tối đa 3 câu (`slice(0, 3)`) từ Markdown và chưa bóc tách bảng từ vựng IPA / Minimal pairs / 20 từ tự test trong bài.
5. Thanh sub-tab ở một số phòng thực hành bị hẹp ngang khi mở Sổ tay bên phải hoặc trên mobile.

## Ràng buộc an toàn (TUYỆT ĐỐI KHÔNG PHÁ E2E / KHÔNG XÓA DB)
- KHÔNG tự chạy `DELETE` / `TRUNCATE` xóa dữ liệu trong `swms_local_dev.db` và KHÔNG sửa `backend/`.
- Giữ nguyên mọi `data-testid` đang được 130 bài test Playwright (`scripts/verify.ps1`) kiểm tra:
  - Khi nhúng `<LessonPracticeLab />` inline bên trong tab `"Bài giảng"` / `"Song song"`, truyền prop `isEmbedded={true}` để container dùng `data-testid="lesson-practice-lab-inline"` (chỉ tab `"Thực hành"` chuyên biệt dùng `data-testid="lesson-practice-lab"`, tránh trùng strict locator của Playwright!).

## Các hạng mục bắt buộc thực hiện (trong `frontend/src/`)

### 1. Hiển thị Trực tiếp Khu Thực Hành Ngay Trong Bài Đang Học (`CourseLearningWorkspace.tsx` & `ReaderAndSplitViews.tsx`)
1. **`frontend/src/components/learn/CourseLearningWorkspace.tsx`:**
   - Khi đang xem bài giảng (`(centerTab === "lesson" || centerTab === "split") && hasLesson && lesson`), render trực tiếp `<LessonPracticeLab isEmbedded={true} ... />` **ngay bên dưới `<LessonMarkdownView />`** (nằm trước `<footer ...>` chứa nút Bài trước / Đánh dấu hoàn thành / Bài tiếp theo), với một đường ngăn cách rõ ràng có tiêu đề `"Thực hành ngay cho bài học này"`.
   - Thêm ref `inlinePracticeRef` để khi người học bấm nút `[data-testid="lesson-end-open-practice-btn"]` ở cuối `<LessonMarkdownView />` hoặc bấm nút trên thẻ ```` ```practice-* ````, trang sẽ cuộn mượt thẳng xuống `<LessonPracticeLab isEmbedded={true} />` ngay bên dưới (vẫn giữ tab `"Thực hành"` `data-testid="center-tab-practice"` trên thanh trên cùng cho ai muốn mở riêng toàn màn hình).
2. **`frontend/src/components/reader/ReaderAndSplitViews.tsx` (`/workspace`):**
   - Nhúng tương tự `<LessonPracticeLab isEmbedded={true} ... />` (dùng `next/dynamic`) ngay bên dưới `<LessonMarkdownView />` (khi `activeLessonId && !isDocMode && shouldRenderMarkdown`) để giao diện Classic cũng có thực hành ngay trong bài. Truyền `onSaveToNotebook` nối vào `saveNotebookWithSingleWriterLock` và `onCreateFlashcardQuick` nối vào `POST /api/v1/flashcards` hiện có trong file.

### 2. Biến Các Mục Bài Tập Sẵn Có Trong Nội Dung Bài Giảng Thành Widget Tương Tác (`LessonMarkdownView.tsx`)
1. **Biến thẻ `<details>` câu hỏi/bài tập (`renderDetailsAccordion`) thành Hộp Tự Trả Lời Tại Chỗ:**
   - Nếu `summary` khớp dạng câu hỏi/bài tập (`câu hỏi`, `mini-review`, `tình huống`, `case`, `bài tập`, `kiểm tra`, `đợt 1`, `đợt 2`, `self-check`, `reflection`):
     - Thêm một `<textarea rows={2}>` nhỏ gọn ngay trên phần đáp án với placeholder `"✍️ Thử gõ nhanh câu trả lời / suy nghĩ của bạn trước khi xem gợi ý..."` + nút **"Lưu câu trả lời vào Sổ tay"** (nếu truyền callback `onQuickSaveToNotebook`) bên cạnh phần nội dung gợi ý/đáp án.
     - Truyền optional prop `onQuickSaveToNotebook?: (text: string) => Promise<void>` từ `CourseLearningWorkspace.tsx`.
2. **Biến các đoạn Luyện Phát Âm / Đọc To / Minimal Pairs & Bảng Từ Vựng IPA trong bài Tiếng Anh thành Trạm Nghe + Thu Âm Mini ngay trong bài đọc:**
   - Tạo component nhỏ `InlineSpeechPracticeBar` ngay trong `LessonMarkdownView.tsx`:
     - Nhận danh sách câu/từ mẫu trích từ chính block bảng (`table`) hoặc `blockquote` / `paragraph` chứa cụm `Minimal pairs`, `Bài tập trọng tâm`, `đọc to`, `Tự test`, hoặc dòng có phiên âm IPA `/.../`.
     - Hiển thị ngay bên dưới block đó một thanh gọn đẹp: chọn từ/câu → **[🔊 Nghe đọc mẫu (US / UK · 1.0x / 0.75x)]** + **[🎙️ Thu âm đọc thử & Chấm từng từ]** (dùng `MediaRecorder` phát lại giọng của bạn + `SpeechRecognition` chấm xanh/vàng/đỏ, có fallback nhẹ nhàng nếu trình duyệt không hỗ trợ).
3. **Biến khối ```` ```practice-* ```` giữa bài thành thẻ hành động trực tiếp:**
   - Thay chữ tĩnh `"Chuyển sang tab Thực hành"` trên `CodeBlockCard` cho `practice-*` bằng nút bấm **"Bắt đầu bài tập ngay ↓"** gọi `onOpenPractice?.()`.

### 3. Nối Bộ Câu Hỏi Quiz Thật (1.586 câu) & Flashcards Thật (1.383 thẻ) Vào Ngay Trong `LessonPracticeLab.tsx`
1. Trong `frontend/src/components/learn/LessonPracticeLab.tsx`:
   - Bổ sung prop `isEmbedded?: boolean`. Khi `isEmbedded === true`, bỏ `flex-1 overflow-y-auto` để cuộn liền mạch chung với bài học và dùng `data-testid="lesson-practice-lab-inline"`.
   - Khi `courseId` và `lessonId` thay đổi, tự động fetch song song từ Backend:
     - `GET /api/v1/practice-quizzes?courseId=${courseId}` → tự động dò bài quiz thuộc `lessonId` hiện tại, hoặc khớp theo số tuần/chặng/bài (`W01..W48`, `Tuần 01..28`, `Week 01..28`, `Chặng 1..14`, `Bài 1..`) giữa `quiz.title` và `lessonTitle`; nếu khớp thì gọi `GET /api/v1/practice-quizzes/${quiz.id}` để lấy danh sách câu hỏi thật (`items`).
     - `GET /api/v1/flashcards/due?courseId=${courseId}&lessonId=${lessonId}&includeAll=true` (nếu rỗng theo `lessonId`, fallback lấy tối đa 12 thẻ gần nhất của `courseId` hoặc khớp tên tuần trong `sourceTitleSnapshot`).
   - Hiển thị ngay một dải/khối **"Bài Kiểm Tra & Thẻ Ghi Nhớ Của Chính Bài Học Này"** bên trên hoặc tích hợp gọn thành tab nổi bật trong `LessonPracticeLab.tsx`:
     - **Làm Quiz Thật Tại Chỗ:** Hiển thị từng câu hỏi thật từ giáo trình (Trắc nghiệm / Điền từ / Tự luận), bấm chọn đáp án và bấm **"Kiểm tra câu này"** gọi `POST /api/v1/practice-quizzes/${quizId}/items/${itemId}/submit` (nếu đã nộp trước đó thì hiện kết quả + `expectedSolution` + giải thích từ server!).
     - **Thẻ Ghi Nhớ Của Bài (Flashcards):** Hiển thị lưới/carousel thẻ lật 1-chạm Mặt trước ↔ Mặt sau từ chính DB kèm nút **"Ôn chuyên sâu toàn khóa"** dẫn sang `/app/review?courseId=${courseId}`.

### 4. Mở Rộng Sâu Bộ Trích Xuất Nội Dung Bài Học Trong 4 Phòng Thực Hành (`frontend/src/components/learn/practice/*.tsx`)
1. **`EnglishPracticeLab.tsx`:**
   - Bỏ giới hạn `.slice(0, 3)`! Quét toàn bộ `lessonMarkdown` để trích xuất:
     - Tất cả hàng trong bảng từ vựng Markdown có chứa từ tiếng Anh / cột IPA / cột nghĩa (`| ... | ... |`) → chuyển thành drills có sẵn `sentence` (hoặc cụm từ), `ipa` và `meaningVi`.
     - Các dòng `Minimal pairs (đọc to): bad/bag · pat/pad...` và danh sách từ tự test (`Đọc 20 từ sau...`).
     - Các câu trong blockquote và bullet list chứa tiếng Anh dài $\ge 3$ từ.
   - Đảm bảo bài học nào cũng có danh sách đầy đủ câu/từ lấy từ **đúng bài đang mở** (lên tới 15–25+ mục thực hành + quiz tự động sinh từ chính các từ đó), chỉ fallback sang `DEFAULT_*_DRILLS` cho phần còn thiếu.
2. **`CodingPracticeLab.tsx`:**
   - Trích xuất tất cả các bài tập thực hành (`Bài tập:`, `Bài tập tư duy`, danh sách đánh số `1. Two Sum...`, `Bài kiểm tra chặng`, các lệnh CLI & code block) từ `lessonMarkdown` hiện tại để hiển thị danh sách **Nhiệm vụ Code Của Bài Học Này** (kèm checklist hoàn thành từng yêu cầu + chỗ dán/gõ code thử & AI góp ý qua `/api/v1/ai/explain` + nút lưu Sổ tay) bên cạnh 3 chế độ Điền code / Bug Hunter / Mô phỏng hệ thống.
3. **`FinancePracticeLab.tsx` & `PsychologyLifeSkillsLab.tsx`:**
   - Tự động bóc tách các **Case Study / Ví dụ minh họa / Câu hỏi giữa tuần / Checklist hành động (Action Steps)** ngay từ `lessonMarkdown` đang mở để người học tương tác trả lời & lưu Sổ tay đúng nội dung bài đang đọc, bên cạnh các công cụ tính toán / mô phỏng tương tác có sẵn.
4. **Sửa triệt để lỗi sub-tab bị hẹp ngang:** Thêm `flex-wrap` cho thanh tab con của `CodingPracticeLab.tsx`, `FinancePracticeLab.tsx`, `EnglishPracticeLab.tsx`, `PsychologyLifeSkillsLab.tsx` để không bị cắt chữ khi mở Sổ tay bên phải ở `1440px` hay trên Mobile `390×844`.

### 5. Sắp Xếp Thông Minh Danh Sách Khóa Học Trên Dashboard `/app` & `/app/courses` (Không Xóa Dữ Liệu DB)
- Kiểm tra `frontend/src/app/app/page.tsx` và `frontend/src/app/app/courses/page.tsx`:
  - Sắp xếp danh sách khóa học để đưa **các khóa học chuẩn chất lượng cao (các khóa có `slug` bắt đầu bằng `document-course-` hoặc tiêu đề thuộc bộ giáo trình chính như IELTS, .NET, FullStack, Đầu Tư, Tài Chính, Kỹ Năng Sống, Tâm Lý Học, Hệ điều hành, Học máy, Pháp luật)** và các khóa có nhiều bài học lên **đầu danh sách**, đẩy các khóa rác/test ngắn (`small-3p`, `marker-12p`, `large-305p`, `ietf-rfc7995`, `mixed-scanned-text`, `Double Click Test`) xuống cuối (hoặc lọc vào nhóm phụ nhỏ cuối trang `"Khóa học kiểm thử / tài liệu lẻ"` có thể mở xem nếu muốn — kiểm tra kỹ không làm gãy bất kỳ E2E test nào trong `v2-app-completeness.spec.ts` hay `qa-e2e-suite.spec.ts`!).

## Kiểm chứng bắt buộc
1. `cd frontend; npx tsc --noEmit` → exit code `0`.
2. `cd frontend; npm run test:unit` → exit code `0`.
3. `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 -Build` → exit code `0`.
4. Chạy Playwright kiểm chứng toàn bộ:
   - Chạy `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1` → bắt buộc `STATUS: ALL CHECKS PASSED (EXIT CODE 0)`.
   - Chạy lại hoặc bổ sung trong `docs/qa-round2/scripts/verify-b-practice-labs.js` để chụp ảnh màn hình thực tế khi mở tab `"Bài giảng"` mặc định của 4 khóa học (IELTS, .NET/FullStack, Tài chính cá nhân, Kỹ năng sống/Tâm lý) chứng minh **phần thực hành xuất hiện ngay trong bài đang học**, quiz thật + flashcard thật tải thành công, **0 console errors / 0 pageerrors**; lưu ảnh vào `docs/qa-round2/screenshots/e1-inline-practice/`.
5. Khởi động lại sạch sẽ server cổng `3000` và `5080` qua `docs/qa-round2/tools/restart-servers.ps1` (cả hai trả HTTP `200`).
6. Ghi báo cáo vào `docs/qa-round2/E1-inline-practice-summary.md` (kèm mục **Chưa kiểm chứng**). Không tự `git commit` (để điều phối viên kiểm tra ảnh & log rồi commit). Không để lại background task nào.
