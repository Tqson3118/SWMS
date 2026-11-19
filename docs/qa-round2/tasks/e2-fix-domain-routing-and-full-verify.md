Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ E2-FIX-DOMAIN-ROUTING-AND-FULL-VERIFY: Sửa 2 Lỗi Nhận Diện Trên Khóa Tâm Lý Học & Chạy Đầy Đủ 11/11 Suites `verify.ps1`

## Bối cảnh
Điều phối viên đã tự mở xem ảnh chụp màn hình từ bước `E1` và `playwright-e2e.log` của lần chạy `20261119-134947`, phát hiện 2 lỗi logic thật và 1 điểm verification cần hoàn tất triệt để (KHÔNG sửa `backend/`, KHÔNG tự `git commit`):

### Lỗi 1: Khóa *"Tự Học Tâm Lý Học 48 Tuần"* bị nhận diện nhầm sang `FinancePracticeLab` (ảnh `09b-learn-psych-inline-desktop.png`)
- **Nguyên nhân:** Trong `frontend/src/components/learn/LessonPracticeLab.tsx` (`detectPreferredMode` dòng 159) và `frontend/src/components/learn/practice/FinancePracticeLab.tsx` (`detectDefaultTab` dòng 92), regex chứa từ đơn `\bnen\b` (vốn định bắt "nến Nhật Candlestick"), nên cụm tiếng Việt *"Nền tảng khoa học"* khi bỏ dấu thành `"nen tang khoa hoc"` đã bị khớp nhầm và đẩy cả khóa Tâm lý học sang phòng Tài chính!
- **Sửa bắt buộc:**
  1. Trong `LessonPracticeLab.tsx` (`detectPreferredMode`):
     - Kiểm tra `titleNorm = stripDiacritics(`${courseTitle} ${lessonTitle}`)` **TRƯỚC TIÊN**:
       - Nếu `titleNorm` khớp `/\b(tam ly|ky nang|giao tiep|cam xuc|cbt|life skills|psychology|eq|nvc|self-awareness|attachment)\b/` → trả về ngay `"recall"`.
       - Nếu `titleNorm` khớp `/\b(ielts|tieng anh|english|listening|speaking|vocabulary|grammar|phat am|shadowing|dictation|pronunciation|ipa)\b/` → trả về ngay `"english"`.
       - Nếu `titleNorm` khớp `/\b(tai chinh|dau tu|chung khoan|investing|finance|bieu do nen|candlestick|co phieu|ngan sach|lai kep|tra no|snowball|avalanche|personal finance|thue thu nhap)\b/` → trả về ngay `"finance"`.
       - Nếu `titleNorm` khớp `/\b(\.net|c#|csharp|react|typescript|javascript|sql|api|docker|ef core|backend|frontend|fullstack|git|code|lap trinh|he dieu hanh|bo nho ao|phan trang|hoc may|machine learning|kien truc may tinh|microservices|python|fastapi|nodejs|kubernetes|algorithm|thuat toan|cau truc du lieu)\b/` → trả về ngay `"code"`.
     - Chỉ khi `titleNorm` chưa khớp rõ ràng mới fallback sang xét `stripDiacritics(md.slice(0, 1500))`.
     - Xóa hoàn toàn `\bnen\b` đơn lẻ khỏi regex `finance` ở cả `LessonPracticeLab.tsx` và `FinancePracticeLab.tsx` (`detectDefaultTab`), thay bằng cụm rõ nghĩa `bieu do nen|nen nhat|candlestick`.

### Lỗi 2: Khóa *"Tự Học Tâm Lý Học 48 Tuần"* bị gắn nhầm `"Trạm Luyện Đọc To & Phát Âm Chuẩn IPA: docs vs superpowers"` (ảnh `08-learn-psych-desktop.png`)
- **Nguyên nhân:** Trong `frontend/src/components/reader/LessonMarkdownView.tsx`:
  - `extractSpeechItemsFromText` (dòng 2102) dùng `/\/[a-zəɪʊʌæɑɔɜːθðʃʒdʒtʃŋ]{1,20}\//i` nên khớp nhầm các đường dẫn `/docs/`, `/notes/`, `/assessments/` thành IPA!
  - Và `pairRegex` (`/\b([a-zA-Z]+)\/([a-zA-Z]+)\b/g`) bắt luôn chuỗi `docs/superpowers` thành `"docs vs superpowers"` trên khóa Tâm lý học!
- **Sửa bắt buộc:**
  1. Xác định ngữ cảnh bài Tiếng Anh ngay đầu component `LessonMarkdownView` (`isEnglishContext`):
     `sourceFolder === "IELTS"` hoặc `lessonTitle`/`sourceRelativePath` có từ khóa tiếng Anh (`ielts`, `english`, `ipa`, `phát âm`, `từ vựng`, `listening`, `speaking`, `vocabulary`, `phase-0`, `phase-1`).
  2. Trong `extractSpeechItemsFromText` & `extractSpeechItemsFromTable`:
     - Chỉ bắt IPA khi có ít nhất 1 ký tự phiên âm IPA thực sự (`/[əɪʊʌæɑɔɜːθðʃʒŋˈˌ]/`) — không bao giờ bắt chuỗi Latin thuần kiểu `/docs/` hay `/notes/`!
     - Chỉ chạy `pairRegex` (`word1/word2`) và danh sách từ (`đọc ... từ`) khi `isEnglishContext === true` HOẶC đoạn đó có từ khóa `minimal pairs` / `phát âm`, VÀ loại trừ toàn bộ từ đường dẫn file phổ biến (`docs`, `notes`, `assessments`, `quality`, `flashcards`, `module`, `phase`, `http`, `https`, `api`, `src`, `app`).
     - Với `quoteRegex`, chỉ thêm câu tiếng Anh trong ngoặc kép nếu `isEnglishContext === true` (hoặc đoạn nói rõ `đọc to câu`), không trích nhầm thuật ngữ trích dẫn tiếng Anh trong bài Tâm lý / Lập trình / Tài chính!

### Điểm 3: Kiểm tra & cố định đóng `KeyboardShortcutsModal` bằng phím `Escape` (`R1-T1-05`)
- Kiểm tra `frontend/src/components/auxiliary/KeyboardShortcutsModal.tsx` (và `.jsx`), đảm bảo khi modal đang mở thì sự kiện `keydown` (`e.key === "Escape"`) trên `window` luôn gọi đóng modal reliably (`capture: true` và đặt focus hợp lý khi mở modal để `Escape` không bao giờ bị nuốt trong E2E test `r5-visual-interactive-e2e.spec.ts` `R1-T1-05`).

## Kiểm chứng bắt buộc
1. `cd frontend; npx tsc --noEmit` → exit code `0`.
2. `cd frontend; npm run test:unit` → exit code `0`.
3. `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 -Build` → exit code `0`.
4. Chạy lại `node docs/qa-round2/scripts/verify-e1-ui-upgrade.js`:
   - Kiểm tra riêng trong script (hoặc in log) xác nhận bài `"Lộ trình tự học Tâm lý học — 48 tuần"` KHÔNG còn hiện `"Trạm Luyện Đọc To & Phát Âm Chuẩn IPA"` và khối thực hành inline bên dưới đúng là `PsychologyLifeSkillsLab` (KHÔNG còn là Tài chính!).
   - Cập nhật lại ảnh trong `docs/qa-round2/screenshots/e1-inline-practice/` (đặc biệt `08-learn-psych-desktop.png` và `09b-learn-psych-inline-desktop.png`).
5. Chạy **ĐẦY ĐỦ 11/11 SUITES** `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1` (KHÔNG truyền `-Suites` rút gọn!):
   - Bắt buộc `STATUS: ALL CHECKS PASSED (EXIT CODE 0)` (80/80 backend + 6/6 multi-user scenarios + **130/130 Playwright E2E tests**).
6. Chạy lại `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1` để cả `:3000` và `:5080/health` đều trả HTTP `200`.
7. Ghi báo cáo ngắn vào `docs/qa-round2/E2-fix-and-verify-summary.md` kèm lệnh, exit code, đường dẫn log `test-results/verification/.../verification-summary.log` và mục **Chưa kiểm chứng**. KHÔNG commit git, KHÔNG để lại background task.
