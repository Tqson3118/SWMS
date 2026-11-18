Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ A-SIMPLIFY-COPY: Đơn giản hóa các đoạn text cứng học thuật/kỹ thuật trên UI sang tiếng Việt đời thường, dễ hiểu (CHỈ sửa `frontend/src/`)

## Mục tiêu
Người dùng phản hồi một số chữ cứng trên HTML/giao diện hiện tại mang nặng tính học thuật/kỹ thuật (như "Cá nhân Chủ quyền", "Deep Reader ergonomic 68ch", "Split PKM", "Spaced Repetition", "Formative Practice Quiz", "AI Semantic Rubric", "Cloze", "OCC Web Locks API"...). Hãy thay đổi toàn bộ các nhãn/mô tả hiển thị cho người dùng trong `frontend/src/` sang tiếng Việt tự nhiên, gần gũi, dễ hiểu ngay với học sinh/sinh viên mới dùng lần đầu.

## Ràng buộc an toàn (QUAN TRỌNG — tránh làm gãy 130 test Playwright E2E)
- **KHÔNG đổi** tên `data-testid`, route URL, hay tên trường API JSON.
- **BẮT BUỘC giữ các cụm từ khóa cụ thể đang được bộ test E2E `scripts/verify.ps1` assert:**
  1. Trong `frontend/src/components/reader/ReaderAndSplitViews.tsx` (và `.jsx` nếu có): thông báo toast khi lưu thẻ phải giữ cụm `"FSRS v5"` (ví dụ `"Đã lưu thẻ vào hàng đợi ôn tập (FSRS v5)!"`); toast xung đột ghi chú phải giữ `"Bản sao xung đột"`; hộp xuất markdown giữ `"SWMS Sovereign Markdown v1.0"`.
  2. Trên `/app/studio/[id]` (`frontend/src/app/app/studio/[id]/page.tsx`): giữ chữ `"FSRS"` trong ít nhất 1 nhãn hiển thị.
  3. Trên Landing V2 (`LandingPageV2.tsx`) & Landing V1 cũ (`AboutSwmsModal.*`, `CorePillarsSection.*`, `ComparisonMatrixSection.*`, `KeyboardShortcutsModal.*`): trong mỗi phần tương ứng phải giữ ít nhất một từ khóa khớp regex của `swms-landing.spec.ts` (`Đọc sâu`, `Chia đôi`, `FSRS`, và `Chủ quyền` trong `AboutSwmsModal`).
  4. Không xóa cụm `"Đã lưu thẻ Cloze nhanh vào Hàng đợi Ôn tập!"` nếu bị `challenger-empirical.spec.ts` assert; hoặc kiểm tra lại `frontend/e2e/challenger-empirical.spec.ts` (lưu ý `challenger-empirical.spec.ts` không nằm trong 11 suite của `verify.ps1`, nhưng vẫn giữ thông báo rõ ràng hoặc kiểm tra trước khi sửa).
  5. Không sửa nội dung các message đã được test assert nguyên văn như `"Tiếp tục lộ trình"`, `"Ôn tập ghi nhớ"`, `"Không gian học tập"`, `"Đề cương khóa học"`, `"Cấu trúc khóa học"`, `"Đã lưu bài học thành công."`, `"Đã bật/tắt..."`, `"1. Tạo từ tài liệu sẵn có"`, `"2. Tạo lộ trình theo chủ đề với AI"`, `"3. Nhập gói khóa học (.swm / .zip)"`.

## Danh sách rà soát & chuyển đổi cụ thể

1. **Metadata & Tiêu đề trang (`frontend/src/app/layout.tsx`, `layout.jsx`, `page.tsx`, `page.jsx`):**
   - `"Studxy — Không gian Học tập Thống nhất cho Cá nhân Chủ quyền"` → `"Studxy — Không gian Tự học & Ghi nhớ Thông minh"`
   - Mô tả meta chứa `"Deep Reader, PKM Notebook và thuật toán FSRS v5"` → `"Đọc tài liệu, ghi chú sổ tay và ôn thẻ nhớ lâu trong một nơi duy nhất. Biến mọi tài liệu thành kiến thức của riêng bạn."`

2. **Landing Page (`frontend/src/components/landing/LandingPageV2.tsx`):**
   - Hero subtitle: `"Đọc tài liệu, viết sổ tay và ôn thẻ nhớ lâu trong một nơi duy nhất. Biến mọi tài liệu thành kiến thức của riêng bạn."`
   - 4 trụ cột (`landing-pillars-section`):
     - `"Deep Reader"` → `"Đọc sâu & Tô màu ý chính"`, mô tả: `"Đọc giáo trình và PDF dễ nhìn, tô màu 5 nhóm ý quan trọng và gắn ghi chú ngay tại đoạn đang đọc."`
     - `"Split PKM"` → `"Vừa đọc vừa ghi chú (Chia đôi)"`, mô tả: `"Chia đôi màn hình: đọc bài bên trái, viết sổ tay bên phải — không cần chuyển qua lại giữa nhiều ứng dụng."`
     - `"FSRS v5 Spaced Repetition"` → `"Ôn thẻ nhớ lâu (Nhắc ôn FSRS)"`, mô tả: `"Tự động nhắc ôn lại đúng lúc bạn sắp quên, giúp nhớ bài lâu dài mà không cần học vẹt."`
     - `"Course Studio + AI"` → `"Tạo khóa học nhanh cùng AI"`, mô tả: `"Tải file PDF lên hoặc nhập chủ đề để hệ thống tạo sẵn chương mục, bài học và thẻ ôn tập."`
   - 4 bước quy trình (`landing-workflow-section`):
     - `"Upload hoặc tạo khóa học"` → `"Tải tài liệu lên hoặc tạo khóa học"` / `"Kéo thả file PDF hoặc nhập chủ đề bạn muốn học để tạo sẵn lộ trình."`
     - `"Đọc & Highlight"` → `"Đọc bài & Tô màu ý chính"` / `"Tô màu đoạn quan trọng, viết ghi chú và tạo thẻ ôn tập ngay khi đang đọc."`
     - `"Ôn tập thông minh"` → `"Ôn thẻ đúng lúc sắp quên"` / `"Hệ thống tự chọn những thẻ cần ôn trong ngày. Lật thẻ và bấm mức độ nhớ của bạn."`
   - Phần Landing V1 cũ (`HeroSection.tsx`, `CorePillarsSection.tsx`, `ComparisonMatrixSection.tsx`, `WorkflowSection.tsx`, `PhilosophySection.tsx`, `LandingFooter.tsx`, `VisualWorkspacePreview.tsx`, và các file `.jsx` tương ứng nếu còn dùng): rút gọn thuật ngữ (`ergonomic`, `68ch`, `PKM`, `in-situ`, `Sovereign`, `định hình phi trừng phạt`) sang tiếng Việt đời thường, vẫn giữ đủ từ khóa `Đọc sâu`, `Chia đôi`, `FSRS`, `Chủ quyền` (trong modal giới thiệu) để không ảnh hưởng test.

3. **Các trang trong `/app/*` & Shell điều hướng:**
   - `frontend/src/app/auth/page.tsx`: `"Studxy · Nền tảng đọc sâu, ghi chú và ghi nhớ lâu dài với FSRS v5"` → `"Studxy · Không gian đọc bài, ghi chú và ôn thẻ nhớ lâu"`.
   - `frontend/src/app/app/review/page.tsx`:
     - `"Đã hoàn tác đánh giá thẻ vừa rồi theo FSRS server."` → `"Đã hoàn tác lượt đánh giá thẻ vừa rồi."`
     - `"Hãy bôi đen đoạn văn hoặc tạo thẻ Cloze từ bài học để bắt đầu ôn tập."` → `"Hãy bôi đen đoạn văn trong bài học để tạo thẻ điền từ hoặc thẻ hỏi đáp và bắt đầu ôn tập."`
   - `frontend/src/app/app/courses/[id]/quiz/page.tsx`:
     - `"Nhận xét AI Rubric"` → `"Nhận xét chi tiết từ AI"`
     - `cloze: "Điền khuyết"` → `cloze: "Điền từ còn thiếu"`
   - `frontend/src/components/learn/CourseLearningWorkspace.tsx`:
     - Chuyển các nhãn `"Thẻ Cloze"` / `"Cloze"` → `"Thẻ điền từ"`, `"AI Rubric"` → `"AI chấm & nhận xét"`, `"PKM"` → `"Sổ tay ghi chú"`.
   - `frontend/src/components/navigation/Sidebar.tsx` & `frontend/src/components/layout/TopNavHeader.tsx`:
     - `"Workspace Classic"` → `"Giao diện nhiều khung (Classic)"` kèm tooltip `"Chế độ bàn làm việc nhiều khung trên máy tính"`.

4. **Trang `/workspace` (`ReaderAndSplitViews.tsx`, `ReviewAndPracticeView.tsx`, `PersonalWorkspaceDashboard.tsx`):**
   - `"Trợ lý học thuật AI (gemini-3.8-flash In-Situ Ghostreader)"` → `"Trợ lý AI giải thích nhanh"`
   - `"Đã lưu tự động (OCC v...) qua Web Locks API!"` → `"Đã tự động lưu sổ tay (bản v...)!"`
   - `"(*) Cloze tức thì (Mặc định • 0 AI Credit)"` → `"(*) Thẻ điền từ nhanh (Miễn phí • 0 lượt AI)"`
   - `"Đánh giá Quá trình (Formative Practice Quiz • Không tính điểm phạt)"` → `"Bài luyện tập củng cố (Làm sai không bị trừ điểm)"`
   - `"Nhận xét định tính từ AI Rubric (gemini-3.8-flash)"` → `"Nhận xét chi tiết từ AI"`
   - `"Trạm ôn tập Spaced Repetition"` → `"Trạm ôn tập thẻ nhớ"`
   - `"Mở Ghi chú PKM"` → `"Mở Sổ tay ghi chú"`
   - `"2. Trích xuất Cloze"` → `"2. Tạo thẻ điền từ"`
   - Giữ nguyên `"FSRS v5"` trong toast tạo thẻ ở `ReaderAndSplitViews.tsx`, `"Bản sao xung đột"`, và `"SWMS Sovereign Markdown v1.0"` theo ràng buộc E2E trên.

5. **Rà soát thêm các file khác trong `frontend/src/`:**
   - Tìm mọi chỗ còn chứa `"ergonomic"`, `"68ch"`, `"PKM"`, `"Spaced Repetition"`, `"Formative"`, `"Sovereign"`, `"in-situ"`, `"Cá nhân Chủ quyền"` hiển thị trực tiếp lên màn hình và làm mượt lại sang tiếng Việt tự nhiên (không đụng tới comment code hay biến nội bộ).

## Kiểm chứng & Kết thúc
1. Chạy `cd frontend; npx tsc --noEmit` (bắt buộc exit code 0).
2. Chạy `cd frontend; npm run test:unit` (bắt buộc exit code 0).
3. KHÔNG tự restart server hoặc chạy `git commit` nếu chưa được yêu cầu? Chú ý: tác vụ này chạy tuần tự một mình sau khi `a-fix-backend` & `a-fix-frontend` đã commit, nên **ĐƯỢC PHÉP commit** một commit nhỏ: `feat(copy): simplify academic and technical UI text to plain Vietnamese`.
4. Ghi tóm tắt các file và chuỗi đã đổi vào `docs/qa-round2/A-simplify-copy-summary.md` (kèm mục **Chưa kiểm chứng**). Không để lại background task nào.
