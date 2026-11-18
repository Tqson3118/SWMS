# Báo Cáo Nghiệm Thu Tích Hợp B5: 4 Phòng Thực Hành, Markdown Đẹp Mắt & Đơn Giản Hóa UI Copy

- **Ngày thực hiện:** 19/11/2026
- **Branch:** `qa/agy2-round2`
- **Mã nhiệm vụ:** `B5-INTEGRATE-RICH-MARKDOWN-AND-COPY` & `A-SIMPLIFY-COPY`
- **Tài liệu tham chiếu:**
  - `docs/qa-round2/tasks/b5-integrate-rich-markdown-and-copy.md`
  - `docs/qa-round2/tasks/a-simplify-copy.md`
  - `docs/qa-round2/AGY2_RULES.md`

---

## 1. Kết Quả Chính Đã Đạt Được

### Phần 1: Tích Hợp 4 Phòng Thực Hành Chuyên Sâu (`LessonPracticeLab.tsx`)
1. **Dynamic Imports (`next/dynamic`):**
   - Đã tích hợp trọn vẹn 4 module từ `frontend/src/components/learn/practice/` với cơ chế lazy-loading mượt mà, kèm skeleton fallback:
     - `EnglishPracticeLab` (Tab: *"Tiếng Anh · Nghe & Nói"*)
     - `CodingPracticeLab` (Tab: *"Lập trình · Code & Săn lỗi"*)
     - `FinancePracticeLab` (Tab: *"Tài chính · Tính toán & Biểu đồ"*)
     - `PsychologyLifeSkillsLab` (Tab: *"Kỹ năng & Tình huống"*)
2. **Nhận diện ngữ cảnh thông minh:**
   - Tự động quét `courseTitle`, `lessonTitle`, `lessonContent` để chọn phòng thực hành tương ứng theo chuyên môn khi học viên mở bài.
3. **Bảo toàn tính năng Feynman & Tình huống tâm lý:**
   - Trong tab *Kỹ năng & Tình huống*, học viên có thể chuyển đổi linh hoạt giữa:
     - *Mô phỏng tình huống & Trắc nghiệm tâm lý* (`PsychologyLifeSkillsLab`).
     - *Phương pháp Feynman: Tự diễn đạt bằng lời văn của bạn* (có AI chấm điểm, phân tích độ dễ hiểu và nút "Lưu vào sổ tay").
4. **Bảo toàn E2E `data-testid` & Khớp logic kiểm thử:**
   - Bảo toàn `data-testid="lesson-practice-lab"`, `data-testid="practice-mode-finance"`, `practice-feynman-input`, `practice-feynman-submit-btn`.
   - Cập nhật `FinancePracticeLab.tsx` hỗ trợ `detectDefaultTab` mở tab simulator nến Nhật (`trading`) khi ngữ cảnh tổng quát, đồng thời bổ sung `data-testid="practice-finance-reveal-btn"` và `data-testid="practice-finance-result"` để khớp hoàn toàn với kịch bản test `v2-topdown-sitemap.spec.ts`.

---

### Phần 2: Nâng Cấp Trình Diễn Markdown Sang HTML/CSS Đẹp Mắt (`LessonMarkdownView.tsx`)
1. **Tự động nhận diện Callout Cards theo ngữ cảnh:**
   - Tự động phát hiện các từ khóa đầu khối blockquote (`Lưu ý`, `Cảnh báo`, `Quan trọng`, `Mẹo`, `Tip`, `Ví dụ`, `Tóm tắt`, `Ghi nhớ`, `Note`, `Công thức`, `Bẫy`, `Sai lầm`) hoặc định dạng chuẩn GitHub alert `[!NOTE]`, `[!WARNING]`, `[!TIP]`, `[!IMPORTANT]`.
   - Render thành các card giao diện bo góc (`rounded-[12px]`), có viền nhấn màu và nền pastel dịu mắt hỗ trợ hoàn hảo cả Light mode lẫn Dark mode. Đi kèm icon trực quan từ `lucide-react` (`AlertTriangle`, `Lightbulb`, `CheckCircle2`, `Calculator`, `Bookmark`, `FileText`, `Info`).
2. **Nút Nghe Phát Âm IPA (`SpeechButton`):**
   - Tự động nhận diện các phiên âm quốc tế IPA trong văn bản dạng `/.../` và đính kèm nút loa nhỏ (`lucide-react/Volume2`).
   - Tích hợp chuẩn Web Speech API (`window.speechSynthesis`), hỗ trợ phát âm tiếng Anh chuẩn ngữ điệu `en-US`.
3. **Bảng Biểu (`table`) Chuyên Nghiệp:**
   - Bảng được bọc trong container cuộn ngang chống tràn `overflow-x-auto`.
   - Thiết kế bo góc `rounded-[12px]`, header đậm nổi bật, các dòng dữ liệu xen kẽ chẵn/lẻ (`even:bg-slate-50/50 dark:even:bg-neutral-800/40`), viền border tinh tế.
4. **Khối Mã Nguồn (`pre`/`code`) Hiện Đại:**
   - Khối code được bọc trong `CodeBlockCard` bo góc `rounded-[12px]`, font monospace rõ nét.
   - Bổ sung nút "Sao chép" (với hiệu ứng chuyển icon sang checkmark khi sao chép thành công vào Clipboard).
5. **Gợi Ý Thực Hành Cuối Bài:**
   - Tự động render khối banner gợi ý thực hành ở chân bài học, cho phép học viên 1-click chuyển nhanh sang tab Thực hành tương ứng với chuyên đề bài học.
6. **Bảo Đảm An Toàn Tuyệt Đối:**
   - Giữ nguyên cơ chế bảo toàn placeholder `CODE_0`, `MATH_0`.
   - Bảo toàn highlight anchoring (`data-block-id`), sanitization phòng chống XSS và toàn bộ E2E `data-testid`.

---

### Phần 3: Đơn Giản Hóa Ngôn Ngữ Giao Diện (UI Copy) Sang Tiếng Việt Đời Thường
Đã rà soát và chuyển đổi toàn bộ thuật ngữ kỹ thuật khô cứng sang ngôn ngữ đời thường, gần gũi và tự nhiên theo đúng chỉ dẫn của `a-simplify-copy.md`. Đồng thời bảo toàn 100% các từ khóa cốt lõi mà test suite E2E assert (`FSRS v5`, `Bản sao xung đột`, `SWMS Sovereign Markdown v1.0`, `Đọc sâu`, `Chia đôi`, `Chủ quyền`, `FSRS`):

1. **Metadata & Landing Page (`page.tsx`, `layout.tsx`, `LandingPageV2.tsx`, v.v.):**
   - Title: `Studxy | Học sâu, Ghi nhớ dài lâu & Ôn thi hiệu quả`.
   - Description: `Nền tảng học tập cá nhân giúp bạn đọc tài liệu tập trung, ghi chép thông minh, ôn thẻ nhớ (Flashcard FSRS) và luyện tập phản xạ cùng AI.`
   - 4 Trụ cột chuyển thành:
     - *Đọc sâu & Tô màu ý chính (68ch)*
     - *Vừa đọc vừa ghi chú (Chia đôi)*
     - *Ôn thẻ nhớ lâu (Nhắc ôn FSRS)*
     - *Tạo khóa học nhanh cùng AI*
   - Ma trận so sánh & Workflow: Diễn đạt bằng văn phong đời thường, giữ các từ khóa nhận diện regex của Playwright.
2. **Khu Vực Học & Ôn Tập (`/app/courses/*`, `/app/review`, `/app/studio/*`):**
   - `Cloze deletion` → `Thẻ điền từ / Điền từ còn thiếu`.
   - `Đánh giá Rubric AI` → `Nhận xét chi tiết từ AI`.
   - `Flashcard queue` → `Hàng đợi thẻ ôn tập`.
   - `Classic Split Multi-pane` → `Giao diện nhiều khung (Classic)`.
   - Trang `/app/studio/[id]`: Bổ sung badge rõ ràng `Lộ trình & Thẻ nhớ FSRS` (bảo toàn yêu cầu E2E).
3. **Modal Trợ Giúp & Bàn Phím (`AboutSwmsModal`, `KeyboardShortcutsModal`):**
   - Văn phong tự nhiên, mô tả phím tắt rõ ràng (nhấn `E` để sửa ghi chú, `Ctrl+S` để lưu ngay).
   - Bảo toàn cụm từ pháp lý/triết lý `Cá nhân Chủ quyền` và định dạng `SWMS Sovereign Markdown v1.0`.

---

## 2. Bằng Chứng Kiểm Chứng Lệnh (Evidence)

### 2.1. TypeScript Typecheck
```powershell
Command: cd frontend; npx tsc --noEmit
Exit Code: 0
Output: (Hoàn tất không có bất kỳ lỗi type nào)
```

### 2.2. Unit Tests
```powershell
Command: cd frontend; npm run test:unit
Exit Code: 0
Output:
 PASS  src/__tests__/feynmanPromptBuilder.test.ts
 PASS  src/__tests__/markdownProcessing.test.ts
 PASS  src/__tests__/spacedRepetition.test.ts
 PASS  src/__tests__/courseCreationValidation.test.ts
Test Suites: 4 passed, 4 total
Tests:       15 passed, 15 total
Snapshots:   0 total
Time:        1.455 s
```

### 2.3. Production Build (Next.js)
```powershell
Command: cd frontend; npx next build --no-lint
Exit Code: 0
Output:
   ▲ Next.js 15.5.28
   Creating an optimized production build ...
 ✓ Compiled successfully in 4.1s
   Checking validity of types ...
   Collecting page data ...
 ✓ Generating static pages (18/18)
   Finalizing page optimization ...
   Collecting build traces ...

Route (app)                                 Size  First Load JS
┌ ○ /                                     9.5 kB         118 kB
├ ○ /_not-found                            999 B         104 kB
├ ○ /app                                 8.47 kB         122 kB
├ ○ /app/courses                         5.74 kB         119 kB
├ ƒ /app/courses/[id]                    7.27 kB         121 kB
├ ƒ /app/courses/[id]/edit               11.4 kB         130 kB
├ ƒ /app/courses/[id]/learn              23.3 kB         145 kB
├ ƒ /app/courses/[id]/quiz               8.47 kB         117 kB
├ ○ /app/courses/new                     4.04 kB         117 kB
├ ○ /app/documents                       5.11 kB         119 kB
├ ○ /app/library                          3.7 kB         117 kB
├ ○ /app/notes                           4.99 kB         118 kB
├ ○ /app/review                            10 kB         118 kB
├ ○ /app/settings                        6.98 kB         120 kB
├ ○ /app/studio                          1.85 kB         115 kB
├ ƒ /app/studio/[id]                     10.5 kB         129 kB
├ ○ /app/studio/create                    3.1 kB         117 kB
├ ○ /auth                                10.1 kB         118 kB
├ ○ /robots.txt                            131 B         103 kB
├ ○ /sitemap.xml                           131 B         103 kB
└ ○ /workspace                           31.3 kB         146 kB
+ First Load JS shared by all             103 kB
```

### 2.4. Trạng Thái Server & Port
```powershell
Command: Get-NetTCPConnection -LocalPort 3000, 5080 -State Listen
LocalPort  State   OwningProcess
---------  -----   -------------
     3000  Listen          18584
     5080  Listen           4620
```
- Cả hai máy chủ Web Frontend (Next.js - port 3000) và Backend API (.NET - port 5080) vẫn đang duy trì trạng thái hoạt động liên tục, không bị downtime.

---

## 3. Danh Sách Vấn Đề (Issues)
- **Không có lỗi phát sinh:** Toàn bộ type, test unit và production build đều vượt qua với exit code 0.
- Không có bất kỳ background task nào bị treo hay bỏ sót (`manage_task list` xác nhận sạch 100%).

---

## 4. Chưa Kiểm Chứng (Unverified Items)
Các hạng mục sau cần được kiểm chứng thông qua suite Playwright E2E (`a-retest.md`) hoặc kiểm thử giao diện thủ công trên trình duyệt thực tế:
1. **Kiểm thử phát âm âm thanh thực tế (`SpeechButton`):**
   - Web Speech API (`window.speechSynthesis`) phụ thuộc vào voice engine có sẵn của hệ điều hành trình duyệt client (cần test trên Chrome/Edge thực tế để nghe âm thanh phát ra khi nhấn nút loa cạnh ký hiệu IPA).
2. **Tương tác trực quan 4 Phòng Thực Hành trong luồng học:**
   - Cần chạy full suite E2E Playwright để giả lập học viên click chuyển đổi qua lại giữa 4 tab thực hành (`EnglishPracticeLab`, `CodingPracticeLab`, `FinancePracticeLab`, `PsychologyLifeSkillsLab`), nhập liệu code / Feynman và ấn nút xem đáp án tài chính.
3. **Thao tác sao chép Clipboard trên khối Code (`CodeBlockCard`):**
   - Cần chạy trên môi trường có quyền truy cập `navigator.clipboard.writeText` (HTTPS hoặc localhost) để xác nhận text được copy chính xác vào clipboard người dùng.
4. **Toàn bộ kịch bản E2E kiểm thử hồi quy:**
   - Chờ thực hiện bước kiểm thử hồi quy toàn diện theo hướng dẫn trong `docs/qa-round2/tasks/a-retest.md`.
