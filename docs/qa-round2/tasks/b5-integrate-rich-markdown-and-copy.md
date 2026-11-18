Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` và `docs/qa-round2/tasks/a-simplify-copy.md` trước khi làm bất cứ gì.

# Nhiệm vụ B5-INTEGRATE-RICH-MARKDOWN-AND-COPY: Tích hợp 4 Phòng Thực Hành, Nâng cấp Markdown → HTML/CSS Đẹp Mắt, và Đơn Giản Hóa Ngôn Ngữ UI

## Bối cảnh
4 module thực hành chuyên sâu cho 4 nhóm khóa học đã được xây dựng trong `frontend/src/components/learn/practice/`:
1. `EnglishPracticeLab.tsx` (Tiếng Anh & IELTS: Shadowing ghi âm + chấm từng từ, Dictation gõ/chạm từ, Speaking Cue Card, Mini-Quiz)
2. `CodingPracticeLab.tsx` (Lập trình & Hệ thống: Điền code + giải thích từng dòng, Bug Hunter, Mô phỏng luồng chạy Full-Stack & TLB/EAT)
3. `FinancePracticeLab.tsx` (Tài chính & Đầu tư: Ngân sách 50/30/20 + Quỹ dự phòng, Máy tính Lãi kép + Lạm phát SVG, So sánh trả nợ Snowball/Avalanche, Biểu đồ nến & Quản trị rủi ro)
4. `PsychologyLifeSkillsLab.tsx` (Tâm lý học & Kỹ năng sống: Mô phỏng hội thoại thực tế, Nhận diện bẫy tâm lý hằng ngày, Gỡ rối suy nghĩ 5 bước)

Đồng thời bạn sẽ gộp luôn toàn bộ yêu cầu từ `docs/qa-round2/tasks/a-simplify-copy.md` (đổi các đoạn chữ cứng học thuật/kỹ thuật sang tiếng Việt đời thường, dễ hiểu).

## Công việc cụ thể

### Phần 1: Tích hợp 4 module thực hành vào `frontend/src/components/learn/LessonPracticeLab.tsx`
1. Đọc kỹ `frontend/src/components/learn/LessonPracticeLab.tsx` và 4 file trong `frontend/src/components/learn/practice/` để nắm rõ tên export và props.
2. Dùng `next/dynamic` (hoặc import sạch gọn để giữ bundle nhẹ trên các môn khác) để render tương ứng theo 4 tab chế độ trong `LessonPracticeLab.tsx`:
   - `english` → `<EnglishPracticeLab {...props} />` (Nhãn tab dễ hiểu: `"Tiếng Anh · Nghe & Nói"`)
   - `code` → `<CodingPracticeLab {...props} />` (Nhãn tab: `"Lập trình · Code & Săn lỗi"`)
   - `finance` → `<FinancePracticeLab {...props} />` (Nhãn tab: `"Tài chính · Tính toán & Biểu đồ"`)
   - `recall` → `<PsychologyLifeSkillsLab {...props} />` (Nhãn tab: `"Kỹ năng & Tình huống"` hoặc `"Tâm lý & Thực hành"`) — đồng thời vẫn giữ nguyên hoặc tích hợp gọn phần ghi nhớ ý chính nếu bài học không thuộc tâm lý/kỹ năng sống!
3. Tự động nhận diện `courseTitle`, `lessonTitle`, `lessonMarkdown` để mở đúng phòng thực hành phù hợp (ví dụ khóa Tâm lý/Kỹ năng sống mở mặc định `PsychologyLifeSkillsLab`, khóa Tiếng Anh/IELTS mở `EnglishPracticeLab`, khóa Lập trình/.NET/OS mở `CodingPracticeLab`, khóa Tài chính/Đầu tư mở `FinancePracticeLab`).
4. Giữ nguyên mọi `data-testid` nếu có test E2E đang dùng.

### Phần 2: Nâng cấp `frontend/src/components/reader/LessonMarkdownView.tsx` — Render Markdown thành các thẻ HTML/CSS đẹp mắt, trực quan hơn
Mục tiêu là giữ nội dung gốc `.md` dễ bảo trì nhưng hiển thị thành giao diện HTML/CSS sinh động, dễ đọc lâu trên cả Light Mode, Dark Mode và Mobile `390×844`:
1. **Callout Cards có phân loại tự động theo ngữ cảnh đoạn blockquote / mục tiêu đề:**
   - Tự động nhận diện các blockquote hoặc hộp ghi chú bắt đầu bằng các từ khóa phổ biến trong giáo trình như `Lưu ý`, `Cảnh báo`, `Quan trọng`, `Mẹo`, `Tip`, `Ví dụ`, `Tóm tắt`, `Ghi nhớ`, `Note`, `Công thức`, `Bẫy`, `Sai lầm` → hiển thị thành **Thẻ Callout có viền trái nhấn màu + nền dịu + icon Lucide phù hợp** (vàng/amber cho Mẹo/Ghi nhớ, đỏ/rose cho Cảnh báo/Sai lầm, xanh ngọc/emerald cho Ví dụ/Thực hành, tím/indigo cho Công thức/Tóm tắt).
2. **Nút nghe phát âm ngay trên chip từ vựng / câu tiếng Anh mẫu (nếu dòng chứa phiên âm IPA `/.../` hoặc trong bài Tiếng Anh):**
   - Trong `LessonMarkdownView.tsx`, nếu phát hiện thẻ audio `.wav`/`.mp3` hoặc khối/mục từ vựng tiếng Anh có phiên âm IPA, hiển thị gọn nút loa nhỏ (`Volume2`) cho phép bấm nghe trực tiếp qua `window.speechSynthesis` (100% miễn phí) mà không cần rời đoạn đang đọc.
3. **Nâng cấp Khối Thuật Ngữ / Checklist / Bảng biểu & Chi tiết đóng mở (`<details>`):**
   - Làm đẹp bảng Markdown (`table`) với bo góc `rounded-[12px]`, đổ nền xen kẽ dịu mắt (`odd/even`), cuộn ngang mượt trên mobile `overflow-x-auto`.
   - Làm đẹp block code (`pre/code`) với nhãn ngôn ngữ ở góc trên, nút `"Sao chép"` rõ ràng và padding thoáng.
   - Đảm bảo phần **Khối Gợi ý Thực Hành cuối bài** (hoặc thẻ chuyển nhanh sang tab Thực hành) gọn đẹp, không chiếm chỗ.
4. **An toàn tuyệt đối:** giữ nguyên khả năng bôi đen đánh dấu (highlight anchoring, `data-block-id`, `anchoredAnnotations`), giữ nguyên xử lý sửa lỗi `CODE_0`, giữ nguyên XSS safety, không xóa bất kỳ `data-testid` nào.

### Phần 3: Đơn giản hóa các đoạn text cứng học thuật/kỹ thuật trên toàn UI (`a-simplify-copy`)
- Thực hiện ĐẦY ĐỦ danh sách chuyển đổi và các ràng buộc E2E trong `docs/qa-round2/tasks/a-simplify-copy.md` (trên `layout.tsx`/`jsx`, `LandingPageV2.tsx`, các component landing, `/auth`, `/app/review`, `/app/courses/[id]/quiz`, `CourseLearningWorkspace.tsx`, `Sidebar.tsx`, `TopNavHeader.tsx`, `ReaderAndSplitViews.tsx`, `ReviewAndPracticeView.tsx`, `PersonalWorkspaceDashboard.tsx`).
- Rà soát để **chắc chắn giữ lại các từ khóa E2E bắt buộc** đã liệt kê trong `docs/qa-round2/tasks/a-simplify-copy.md`.

## Kiểm chứng bắt buộc
1. Chạy `cd frontend; npx tsc --noEmit` → exit code `0`.
2. Chạy `cd frontend; npm run test:unit` → exit code `0`.
3. Chạy `cd frontend; npx next build --no-lint` → exit code `0` (xác nhận build production sạch 18/18 routes).
4. Ghi báo cáo chi tiết các thay đổi đã tích hợp vào `docs/qa-round2/B5-integration-summary.md` (gồm mục **Chưa kiểm chứng**).
5. KHÔNG commit git vội và KHÔNG restart server nếu không cần thiết (chờ bước `a-retest` chạy ngay sau đó để kiểm tra toàn bộ trên trình duyệt thật và `scripts/verify.ps1`).
6. KHÔNG để lại bất kỳ background task nào.
