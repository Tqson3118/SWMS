# Báo Cáo Nhiệm Vụ E2: Sửa 2 Lỗi Nhận Diện Khóa Tâm Lý Học & Kiểm Chứng Toàn Diện 11/11 Suites

**Thời gian hoàn thành:** 2026-11-19T14:36:00+07:00  
**Môi trường:** Windows 10, PowerShell, Next.js 15, .NET 10 LTS  
**Trạng thái nghiệm thu:** **HOÀN THÀNH TOÀN DIỆN (EXIT CODE 0)**

---

## 1. Kết Quả Chính & Các Điểm Đã Sửa

### 1.1. Sửa Lỗi 1: Khóa "Tự Học Tâm Lý Học 48 Tuần" bị nhận diện nhầm sang `FinancePracticeLab`
- **Tệp sửa đổi:**
  - [LessonPracticeLab.tsx](file:///D:/03_Study/Studxy/frontend/src/components/learn/LessonPracticeLab.tsx#L151-L215)
  - [FinancePracticeLab.tsx](file:///D:/03_Study/Studxy/frontend/src/components/learn/practice/FinancePracticeLab.tsx#L80-L97)
- **Giải pháp:**
  - Trong `detectPreferredMode`: Kiểm tra `titleNorm = stripDiacritics(`${courseTitle} ${lessonTitle}`)` trước tiên với độ ưu tiên hàng đầu cho Tâm lý học / Kỹ năng sống (`\b(tam ly|ky nang|giao tiep|cam xuc|cbt|life skills|psychology|eq|nvc|self-awareness|attachment)\b`), sau đó tới Tiếng Anh, Tài chính, và Lập trình.
  - Xóa bỏ hoàn toàn từ đơn `\bnen\b` khỏi regex tài chính ở cả `LessonPracticeLab.tsx` và `FinancePracticeLab.tsx` (`detectDefaultTab`), thay bằng cụm từ định danh rõ nghĩa `bieu do nen|nen nhat|candlestick`. Cụm tiếng Việt "Nền tảng khoa học" không còn bị khớp nhầm sang tài chính.

### 1.2. Sửa Lỗi 2: Khóa "Tự Học Tâm Lý Học 48 Tuần" bị gắn nhầm "Trạm Luyện Đọc To & Phát Âm Chuẩn IPA"
- **Tệp sửa đổi:**
  - [LessonMarkdownView.tsx](file:///D:/03_Study/Studxy/frontend/src/components/reader/LessonMarkdownView.tsx#L1719-L2350)
- **Giải pháp:**
  - Xác định `isEnglishContext` ngay đầu `LessonMarkdownView` dựa trên `sourceFolder === "IELTS"` hoặc từ khóa tiếng Anh trong tiêu đề / đường dẫn.
  - Định nghĩa tập từ khóa đường dẫn bị loại trừ `EXCLUDED_PATH_WORDS` (`docs`, `notes`, `assessments`, `quality`, `flashcards`, `module`, `phase`, `http`, `https`, `api`, `src`, `app`).
  - Trong `extractSpeechItemsFromText`: Chỉ nhận diện mẫu IPA khi có ít nhất 1 ký tự phiên âm IPA thực sự (`/[əɪʊʌæɑɔɜːθðʃʒŋˈˌ]/`), loại bỏ hoàn toàn các chuỗi Latin đường dẫn thuần túy như `/docs/` hay `/notes/`.
  - Chỉ chạy `pairRegex` (`word1/word2`) và danh sách từ khi `isEnglishContext === true` hoặc đoạn văn bản có từ khóa phát âm cụ thể, đồng thời loại trừ toàn bộ `EXCLUDED_PATH_WORDS`.
  - Giới hạn `quoteRegex` chỉ trích câu khi ở ngữ cảnh tiếng Anh hoặc có chỉ định đọc to câu.
  - Trong bảng (`block.type === "table"`): Chỉ kích hoạt trích xuất mẫu luyện phát âm khi ở ngữ cảnh tiếng Anh hoặc tiêu đề cột chỉ định rõ IPA / phát âm, đồng thời thẩm định ký tự IPA thực thụ.

### 1.3. Cố định Điểm 3: Đóng `KeyboardShortcutsModal` bằng phím `Escape` (`R1-T1-05`)
- **Tệp sửa đổi:**
  - [KeyboardShortcutsModal.tsx](file:///D:/03_Study/Studxy/frontend/src/components/auxiliary/KeyboardShortcutsModal.tsx#L235-L315)
  - [KeyboardShortcutsModal.jsx](file:///D:/03_Study/Studxy/frontend/src/components/auxiliary/KeyboardShortcutsModal.jsx#L192-L260)
- **Giải pháp:**
  - Đăng ký sự kiện `keydown` trên `window` với tùy chọn `{ capture: true }`, gọi `e.preventDefault()`, `e.stopPropagation()`, `onClose()`.
  - Thiết lập `tabIndex={-1}` trên container modal và tự động `modalRef.current.focus()` ngay khi mở, đảm bảo phím `Escape` luôn được bắt trọn vẹn và không bị nuốt bởi các listener khác.

---

## 2. Bằng Chứng & Kết Quả Kiểm Chứng Thực Tế

### 2.1. TypeScript Compilation & Unit Tests
| Bước kiểm tra | Lệnh thực thi | Exit Code | Kết quả |
| :--- | :--- | :---: | :--- |
| TypeScript check | `cd frontend; npx tsc --noEmit` | `0` | Không phát sinh bất kỳ lỗi kiểu dữ liệu nào |
| Unit tests | `cd frontend; npm run test:unit` | `0` | Passed 1,000 FSRS golden vectors + 16/16 course classification tests |

### 2.2. Kiểm chứng UI Upgrade & Miền Môn Học
- **Lệnh:** `node docs/qa-round2/scripts/verify-e1-ui-upgrade.js`
- **Exit Code:** `0`
- **Kết quả chẩn đoán:**
  - `[VERIFY-E2] Psych lesson speechBarCount: 0, hasIpaText: false` → **Đạt:** Khóa Tâm lý học hoàn toàn không còn xuất hiện trạm phát âm IPA nhầm.
  - `[VERIFY-E2] Psych lesson isPsychLabVisible: true, isFinanceLabVisible: false` → **Đạt:** Khối thực hành hiển thị đúng `PsychologyLifeSkillsLab` (Roleplay, Biases, CBT, Lesson Cases), không còn bị nhầm sang Tài chính.
  - Tổng số lỗi layout: `0`, lỗi console: `0`, tràn ngang (overflow): `0` trên cả desktop 1440px và mobile 390px.
- **Ảnh chụp màn hình cập nhật:**
  - [08-learn-psych-desktop.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/e1-inline-practice/08-learn-psych-desktop.png) (xác nhận sạch bóng IPA leak)
  - [09b-learn-psych-inline-desktop.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/e1-inline-practice/09b-learn-psych-inline-desktop.png) (phòng thực hành Tâm lý học & Kỹ năng sống)
  - [09b-learn-psych-tab4-desktop.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/e1-inline-practice/09b-learn-psych-tab4-desktop.png) (tab Checklist & Tình Huống Bài Học)

### 2.3. Kiểm chứng ĐẦY ĐỦ 11/11 Suites `scripts/verify.ps1`
- **Lệnh:** `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`
- **Thư mục log:** [test-results/verification/20261119-142757](file:///D:/03_Study/Studxy/test-results/verification/20261119-142757/)
- **Summary Log:** [verification-summary.log](file:///D:/03_Study\Studxy\test-results\verification\20261119-142757\verification-summary.log)
- **Playwright Log:** [playwright-e2e.log](file:///D:/03_Study\Studxy\test-results\verification\20261119-142757\playwright-e2e.log)
- **Exit Code:** `0`
- **Chi tiết các phần:**
  1. **Backend Tests:** `80 / 80 passed` (100%)
  2. **Multi-User Concurrency & Isolation Benchmark (30 VUs, 377 reqs):** `ALL 6 SCENARIOS PASSED`
     - S1: Concurrent Registration & Duplicate Guard (`PASS`)
     - S2: Multi-Tenant Data Isolation Audit (`PASS`)
     - S3: High-Concurrency Auto-Save & OCC 409 Forking (`PASS`)
     - S4: FSRS v5 Review Sync & Undo Guard (`PASS`)
     - S5: PDF Range Streaming (HTTP 206) & Vietnamese Hybrid Search (`PASS`)
     - S6: AI Daily Credit Quota Ceiling Burst Guard (`PASS`)
  3. **Playwright E2E Suites (11/11 Suites):** **130 / 130 passed** (100% xanh)
     - `e2e/pdf-multipage-e2e.spec.ts` (Flow A, Flow B)
     - `e2e/qa-e2e-suite.spec.ts` (Flow 1 – 6, Defect Hunt FUNC-01, FUNC-02)
     - `e2e/v2-landing-ergonomics.spec.ts` (13 viewports stress & zero overflow)
     - `e2e/swms-workspace.spec.ts`
     - `e2e/adversarial-viewport-stress.spec.ts`
     - `e2e/challenger-m2-stress.spec.ts`
     - `e2e/r5-visual-interactive-e2e.spec.ts` (Bao gồm R1-T1-05 Escape KeyboardShortcutsModal)
     - `e2e/course-creation-studio.spec.ts`
     - `e2e/v2-app-completeness.spec.ts`
     - `e2e/reader-polish-and-roadmap.spec.ts`
     - `e2e/v2-topdown-sitemap.spec.ts`
- **Dòng trạng thái cuối cùng từ log:**
  ```text
  [14:31:58] All Playwright E2E suites PASSED (exit code 0).
  [14:32:11] VERIFICATION RUN COMPLETE
  [14:32:11] Duration: 253.31s
  [14:32:11] Logs directory: D:\03_Study\Studxy\test-results\verification\20261119-142757
  [14:32:11] STATUS: ALL CHECKS PASSED (EXIT CODE 0)
  ```

### 2.4. Khởi động và Kiểm tra Sức khỏe Cặp Server Production
- **Lệnh:** `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 -Build`
- **Exit Code:** `0`
- **Kiểm tra HTTP Health:**
  - `http://127.0.0.1:3000/health` → `200 OK`
  - `http://127.0.0.1:5080/health` → `200 OK` (`{"status":"healthy","service":"SWMS Backend .NET 10 LTS"}`)
  - Cả 2 server đang chạy ngầm tách rời theo đúng quy định.

---

## 3. Tuân Thủ Quy Định Dự Án (AGY2_RULES.md)

1. **Không sửa thư mục backend:** Toàn bộ thư mục `backend/` được giữ nguyên vẹn 100%.
2. **Không commit git tự ý:** Không thực hiện bất kỳ lệnh `git commit` nào.
3. **Không để lại background task:** Kiểm tra `manage_task(Action: 'list')` cho kết quả rỗng (0 task tồn đọng).
4. **Không làm hỏng testid / dữ liệu DB:** Cơ sở dữ liệu và toàn bộ `data-testid` được bảo toàn nguyên vẹn.

---

## 4. Chưa Kiểm Chứng

- Chưa kiểm thử phản hồi giọng nói phát âm trực tiếp từ microphone phần cứng thực tế đối với trạm IPA (trong môi trường headless CI/agent, Web Speech API và MediaDevices được giả lập theo chuẩn trình duyệt Playwright).
- Chưa kiểm tra trên các trình duyệt di động gốc ngoại trừ Safari/WebKit và Chrome Mobile mô phỏng qua Playwright viewport.
