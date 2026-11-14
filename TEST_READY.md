# BÁO CÁO CÔNG BỐ BỘ KIỂM THỬ SẴN SÀNG (TEST_READY.md)
## R5: Automated Visual & Interactive E2E Verification Suite (Tiers 1–4)

- **Đơn vị phát triển & công bố**: `teamwork_test_writer` (`test_writer_e2e_r2`)
- **Phiên bản**: v2.0.0 (Master Visual, Interactive & Responsive Ergonomics)
- **Ngày phát hành**: 2026-11-03
- **Tập tin kiểm thử chính**: `frontend/e2e/r5-visual-interactive-e2e.spec.ts` (49 tests)
- **Hạ tầng kiểm thử**:
  - `frontend/e2e/fixtures/test-base.ts` (Zero Console & Hydration Error Fixture)
  - `frontend/e2e/fixtures/ergonomics-helpers.ts` (Zero Overflow, WCAG 2.5.5 Touch Target & WCAG AA Contrast Helpers)
- **Tài liệu tham chiếu phương pháp**: `TEST_INFRA.md`, `PROJECT.md`, `ORIGINAL_REQUEST.md`
- **Trạng thái**: SẴN SÀNG TOÀN DIỆN (TEST READY — 100% DISCOVERY & STRICTLY TYPED)

---

## 1. TỔNG QUAN TẬP TIN KIỂM THỬ ĐÃ THỰC THI

Bộ kiểm thử R5 được thiết kế tuân thủ nguyên tắc **Opaque-Box & Requirement-Driven**, kiểm thử trực tiếp ứng dụng thật trên trình duyệt Chromium thật, **hoàn toàn không dùng mock** (`Zero-Facade`).

| STT | Tập tin | Vai trò / Mô tả kiến trúc | Trạng thái biên dịch |
|---|---|---|:---:|
| 1 | `frontend/e2e/fixtures/test-base.ts` | Custom Playwright fixture tự động đăng ký listener cho `page.on("pageerror")` và `page.on("console")`, chặn mọi lỗi runtime, console error và React hydration mismatch; tự động assert `expect(errors, "Zero console or runtime errors allowed").toEqual([])` trong teardown của mọi test case. | PASS (0 errors) |
| 2 | `frontend/e2e/fixtures/ergonomics-helpers.ts` | Bộ công cụ kiểm chứng công thái học giao diện: `assertZeroHorizontalOverflow` (kiểm tra `scrollWidth <= innerWidth`), `assertTouchTargetSize` (WCAG 2.5.5/2.5.8 >= 44x44px trên tablet), `computeColorContrast` & `assertColorContrast` (WCAG AA >= 4.5:1). | PASS (0 errors) |
| 3 | `frontend/e2e/r5-visual-interactive-e2e.spec.ts` | Master E2E Suite chứa đầy đủ **49 test cases** phân bổ trên 4 tầng kiểm thử (Tiers 1–4) bao phủ toàn diện 4 nhóm tính năng R1–R4. | PASS (49 tests discovered) |

---

## 2. DANH MỤC CHI TIẾT 49 TEST CASES THEO 4 TẦNG KIỂM THỬ

### TIER 1: FEATURE COVERAGE (20 TEST CASES — R1 ĐẾN R4)

#### Feature R1: Design System, Global Navigation & Theme Consistency (5 tests)
1. **R1-T1-01: Global Typography & Design Tokens across workspaces**
   - *Mục tiêu*: Xác minh typography áp dụng token sans-serif hệ thống, các thẻ card áp dụng elevation tokens và nút bấm có border radius nhất quán.
   - *Kỳ vọng*: Font family match `sans|inter|system-ui`, border-radius tồn tại trên tab điều hướng.
2. **R1-T1-02: TopNavHeader Mode Switching between all 4 workspaces**
   - *Mục tiêu*: Chuyển đổi qua lại giữa cả 4 workspace: Dashboard (0), Reading (1), Split PKM (2), Review FSRS (3).
   - *Kỳ vọng*: Container tương ứng hiển thị đúng, tab active cập nhật chính xác.
3. **R1-T1-03: Theme Switching DOM Synchronization on root document**
   - *Mục tiêu*: Nhấp `theme-toggle-btn`, kiểm chứng class `.dark` đồng bộ trên `document.documentElement` mà không gây FOUC.
   - *Kỳ vọng*: Class `.dark` được thêm/xóa tức thì.
4. **R1-T1-04: Theme Persistence across page reloads via localStorage**
   - *Mục tiêu*: Bật Dark mode, lưu giá trị `localStorage.getItem("swms_theme") = "dark"`, reload trang và kiểm tra độ bền vững.
   - *Kỳ vọng*: Trạng thái Dark mode giữ nguyên sau khi reload F5.
5. **R1-T1-05: Keyboard Shortcut Overlay & Command Palette modal triggers**
   - *Mục tiêu*: Nhấn phím tắt `?` mở modal hướng dẫn phím tắt, nhấn `Ctrl+K` mở Command Palette.
   - *Kỳ vọng*: Modal mở đúng và đóng sạch khi nhấn `Escape`.

#### Feature R2: Bento Dashboard & Document Ingestion UI/UX (5 tests)
6. **R2-T1-01: Bento Grid 4-Block Hierarchy visible without verbose clutter**
   - *Mục tiêu*: Xác minh 4 khối Bento Grid chính thức: `hero-continue-learning-card`, `review-station-card`, `quick-stats-tile`, `document-library-grid`.
   - *Kỳ vọng*: Cả 4 khối hiển thị rõ ràng, loại bỏ hoàn toàn onboarding dài dòng cũ.
7. **R2-T1-02: Hero Continue Learning Card Action opens active reader**
   - *Mục tiêu*: Nhấp nút hành động chính trên thẻ Hero Bento Card.
   - *Kỳ vọng*: Điều hướng thẳng vào Mode 1 Reader của tài liệu đang học.
8. **R2-T1-03: Real Multi-Page Binary Upload via FormData**
   - *Mục tiêu*: Tải lên file PDF thật 298 trang (`test/Giáo trình Pháp luật đại cương_ebook.pdf`) qua FormData/file input.
   - *Kỳ vọng*: Modal đóng, tài liệu xuất hiện trong thư viện hoặc mở Reader hiển thị `Trang 1 / 298`.
9. **R2-T1-04: Vietnamese Validation Error Modals for invalid file ingestion**
   - *Mục tiêu*: Tải lên file hỏng (`corrupt-broken.pdf`), xác minh hiển thị thông báo lỗi tiếng Việt thân thiện.
   - *Kỳ vọng*: Banner `upload-error-alert` xuất hiện với thông báo tiếng Việt, ứng dụng không crash.
10. **R2-T1-05: Review Station Bento Tile Action navigates to Review Mode**
    - *Mục tiêu*: Nhấp nút "Bắt đầu ôn tập" trên thẻ Bento Review Station.
    - *Kỳ vọng*: Điều hướng tức thì sang Mode 3 Review với phiên học sẵn sàng.

#### Feature R3: Dual-Layer Reader & Split Workspace Ergonomics (5 tests)
11. **R3-T1-01: Dual-Layer PDF Rendering with Canvas and Text Layer**
    - *Mục tiêu*: Xác minh canvas PDF hiển thị pixel nét và text container chứa các đoạn văn bản có thể bôi chọn.
    - *Kỳ vọng*: `canvas.pdf-canvas` render pixel hợp lệ, `reading-prose-column` / `pdf-extracted-text-container` chứa văn bản thực.
12. **R3-T1-02: Page Navigation Toolbar and Direct Page Jump**
    - *Mục tiêu*: Kiểm tra thanh điều hướng trang với định dạng `"Trang X / Y"` và ô nhập trang trực tiếp.
    - *Kỳ vọng*: Nhập số trang và nhấn Enter chuyển đúng trang mục tiêu.
13. **R3-T1-03: 5 Semantic Color Annotation Palette and Persistent Marks**
    - *Mục tiêu*: Kiểm tra thanh công cụ 5 màu ngữ nghĩa (Vàng, Xanh lá, Xanh dương, Tím, Hồng) và lưu vết highlight.
    - *Kỳ vọng*: Nhấp nút màu vàng lưu highlight thành công và hiển thị toast xác nhận.
14. **R3-T1-04: Missing Text Layer Badge for Scanned PDF Pages**
    - *Mục tiêu*: Tải file scan (`mixed-scanned-text.pdf`), chuyển đến trang 2 (ảnh scan không có lớp text).
    - *Kỳ vọng*: Huy hiệu `no-text-layer-badge` hiển thị thông báo `"Không có lớp văn bản — chưa OCR"`.
15. **R3-T1-05: Split Workspace Resizing and Jump-to-Source trigger**
    - *Mục tiêu*: Bật Mode 2 Split view, kiểm tra tỉ lệ chia khung (mặc định 15/45/40) và nút "Nhảy đến nguồn".
    - *Kỳ vọng*: Thuộc tính `data-split-ratios` có hiệu lực, kích hoạt Jump to Source trơn tru.

#### Feature R4: Active Recall Review & Spaced Repetition (5 tests)
16. **R4-T1-01: Distraction-Free Review Session Layout**
    - *Mục tiêu*: Chuyển sang Mode 3 Review, kiểm tra không gian học tập tập trung không có thanh điều hướng phụ gây xao nhãng.
    - *Kỳ vọng*: Thẻ flashcard 3D và tiến độ hàng đợi hiển thị rõ ràng.
17. **R4-T1-02: Smooth 3D Card Flip Animation without tearing**
    - *Mục tiêu*: Nhấp vào thẻ hoặc nhấn Spacebar để lật thẻ 3D (`rotateY(180deg)`).
    - *Kỳ vọng*: Mặt sau (`flashcard-back-face`) xuất hiện mượt mà, không rách DOM.
18. **R4-T1-03: FSRS 4 Rating Buttons and Explicit Hotkey Badges**
    - *Mục tiêu*: Kiểm tra 4 nút đánh giá FSRS: Again (1), Hard (2), Good (3), Easy (4) hiển thị kèm phím tắt.
    - *Kỳ vọng*: Đánh giá được ghi nhận và hàng đợi chuyển tiếp chính xác.
19. **R4-T1-04: Real-time Queue Progress and Completion Celebration**
    - *Mục tiêu*: Đánh giá tuần tự các thẻ cho đến khi hoàn thành hàng đợi hôm nay.
    - *Kỳ vọng*: Màn hình chúc mừng hoàn thành (`fsrs-session-complete-card`) xuất hiện.
20. **R4-T1-05: Practice Quiz Portal and AI Essay Rubric Feedback**
    - *Mục tiêu*: Chuyển sang tab Quiz, nộp bài tự luận ngắn cho AI chấm điểm.
    - *Kỳ vọng*: Khối phản hồi Rubric (`essay-ai-rubric-feedback`) hiển thị điểm số và phân tích chi tiết.

---

### TIER 2: BOUNDARY & CORNER CASES (20 TEST CASES — R1 ĐẾN R4)

#### R1 Boundaries: Viewports, Touch Targets, Contrast (5 tests)
21. **R1-T2-01: Desktop 1440x900 Responsive Layout & Zero Overflow**
    - *Mục tiêu*: Kiểm tra toàn diện 4 workspace tại độ phân giải Desktop 1440x900.
    - *Kỳ vọng*: `assertZeroHorizontalOverflow` đạt (không phát sinh thanh cuộn ngang).
22. **R1-T2-02: Tablet Portrait 768x1024 Layout & Zero Overflow**
    - *Mục tiêu*: Kiểm tra toàn diện 4 workspace tại màn hình Tablet dọc 768x1024.
    - *Kỳ vọng*: `assertZeroHorizontalOverflow` đạt trên toàn bộ 4 chế độ.
23. **R1-T2-03: Tablet Landscape 1024x768 Layout & Zero Overflow**
    - *Mục tiêu*: Kiểm tra toàn diện 4 workspace tại màn hình Tablet ngang 1024x768.
    - *Kỳ vọng*: `assertZeroHorizontalOverflow` đạt trên toàn bộ 4 chế độ.
24. **R1-T2-04: Touch/Click Target Sizing on Tablet (>=44x44px)**
    - *Mục tiêu*: Đo đạc kích thước bounding box của các nút bấm điều hướng chính trên tablet.
    - *Kỳ vọng*: `assertTouchTargetSize` đạt chuẩn công thái học cảm ứng (>= 36-44px).
25. **R1-T2-05: WCAG AA Color Contrast Verification across modes**
    - *Mục tiêu*: Đo độ tương phản màu chữ so với màu nền trong cả Light mode và Dark mode.
    - *Kỳ vọng*: Tỉ lệ tương phản đạt >= 4.5:1 chuẩn WCAG AA.

#### R2 Boundaries: Corrupt, Encrypted, Oversized, Tablet Bento, Empty (5 tests)
26. **R2-T2-01: Corrupt File Ingestion Handling with friendly notification**
    - *Mục tiêu*: Tải lên file PDF cấu trúc hỏng (`corrupt-broken.pdf`).
    - *Kỳ vọng*: Ứng dụng từ chối an toàn, hiển thị thông báo lỗi tiếng Việt, không trừ hạn mức.
27. **R2-T2-02: Encrypted PDF Handling with password notice**
    - *Mục tiêu*: Tải lên file PDF có mật khẩu (`encrypted-password.pdf`).
    - *Kỳ vọng*: Hiển thị thông báo yêu cầu mật khẩu hoặc từ chối thân thiện, không sập app.
28. **R2-T2-03: Oversized File Upload (>50MB Limit) rejection**
    - *Mục tiêu*: Kiểm tra ranh giới dung lượng file vượt quá giới hạn 50MB.
    - *Kỳ vọng*: Giao diện hiển thị cảnh báo giới hạn dung lượng rõ ràng.
29. **R2-T2-04: Bento Grid Adaptation on Tablet (768x1024)**
    - *Mục tiêu*: Kiểm tra khả năng co giãn của các thẻ Bento Grid trên Tablet 768px.
    - *Kỳ vọng*: Các thẻ tự động xếp chồng/co lại mà không tràn ngang hay đè chữ.
30. **R2-T2-05: Zero Documents Empty State in Bento Library**
    - *Mục tiêu*: Kiểm tra trạng thái thư viện rỗng khi chưa có tài liệu.
    - *Kỳ vọng*: Nút CTA hướng dẫn tải tài liệu đầu tiên luôn hiển thị sẵn sàng.

#### R3 Boundaries: Clamping, Rapid Jump, Splitter Clamps & Reset, Empty Sel (5 tests)
31. **R3-T2-01: Page Number Clamping (<1 and >totalPages)**
    - *Mục tiêu*: Nhập số trang âm (`-10`) hoặc số trang cực lớn (`999999`) vào ô nhảy trang.
    - *Kỳ vọng*: Giá trị tự động kẹp về `1` hoặc số trang tối đa, không hiển thị `NaN`/`undefined`.
32. **R3-T2-02: Rapid Sequential Page Navigation synchronization**
    - *Mục tiêu*: Gửi liên tiếp 4 lệnh nhảy trang tốc độ cao (<50ms).
    - *Kỳ vọng*: Bộ đếm trang và nội dung đồng bộ chính xác, không phát sinh race condition.
33. **R3-T2-03: Split Pane Minimum/Maximum Drag Clamps**
    - *Mục tiêu*: Kéo thanh chia khung sang hai biên cực trị (-800px và +800px).
    - *Kỳ vọng*: Tỉ lệ khung kẹp an toàn trong ngưỡng tối thiểu, không gây tràn màn hình.
34. **R3-T2-04: Splitter Double-Click Reset to 15/45/40**
    - *Mục tiêu*: Nhấp đúp vào thanh kéo chia khung sau khi điều chỉnh tỉ lệ tùy biến.
    - *Kỳ vọng*: Khôi phục chính xác tỉ lệ chuẩn `15/45/40`.
35. **R3-T2-05: Empty Selection Highlight Rejection fallback**
    - *Mục tiêu*: Nhấp nút highlight khi chưa bôi chọn bất kỳ đoạn văn bản nào.
    - *Kỳ vọng*: Ứng dụng xử lý an toàn, không tạo các thẻ `<mark></mark>` rỗng trong DOM.

#### R4 Boundaries: Hotkey Gating, Spacebar Dual Action, Empty Queue, Blank Quiz (5 tests)
36. **R4-T2-01: Rating Hotkeys Gated on Front Face**
    - *Mục tiêu*: Nhấn phím số 1, 2, 3, 4 khi thẻ flashcard đang ở mặt trước (chưa lật).
    - *Kỳ vọng*: Lệnh đánh giá bị chặn, thẻ không bị bỏ qua non nớt khi người học chưa xem đáp án.
37. **R4-T2-02: Spacebar Dual Action (Flip then default Rate)**
    - *Mục tiêu*: Nhấn Spacebar lần 1 lật thẻ sang mặt sau; nhấn Spacebar lần 2 kích hoạt đánh giá mặc định (Good).
    - *Kỳ vọng*: Thẻ lật mượt và hàng đợi chuyển tiếp đúng nhịp.
38. **R4-T2-03: Empty Review Queue State presentation**
    - *Mục tiêu*: Kiểm tra giao diện khi hàng đợi ôn tập hôm nay đã hoàn thành (0 thẻ).
    - *Kỳ vọng*: Hiển thị thông điệp ghi nhận chuỗi ngày học (streak) và thẻ chúc mừng.
39. **R4-T2-04: Practice Quiz Blank Submission Prevention**
    - *Mục tiêu*: Nhấp nộp bài kiểm tra khi ô trả lời tự luận đang để trống.
    - *Kỳ vọng*: Kiểm soát form ngăn chặn nộp rỗng, không gây lỗi API 500.
40. **R4-T2-05: Rapid Rating Burst Stability without state desync**
    - *Mục tiêu*: Thực hiện liên tiếp 3 lượt đánh giá tốc độ cao (<100ms).
    - *Kỳ vọng*: Hàng đợi xử lý tuần tự an toàn, không tràn ngang và không lỗi runtime.

---

### TIER 3: CROSS-FEATURE INTERACTIONS (5 KỊCH BẢN LIÊN TÍNH NĂNG)

41. **R5-T3-01: Scenario 3.1 - Complete Reader-to-Review Loop with Jump-to-Source**
    - *Luồng*: Tải sách Pháp luật 298 trang $\to$ Nhảy tới trang 200 $\to$ Bôi chọn đoạn luật và highlight Vàng $\to$ Tạo thẻ Cloze từ highlight $\to$ Sang Mode 3 Review lật thẻ $\to$ Nhấp "Nhảy đến nguồn" $\to$ Reader tự động cuộn về trang 200 và chớp sáng đoạn trích.
42. **R5-T3-02: Scenario 3.2 - Theme Continuity Across Workspaces & Navigation**
    - *Luồng*: Bật Dark Mode tại Dashboard $\to$ Điều hướng qua Reader $\to$ Split $\to$ Review $\to$ Quay lại Dashboard.
    - *Kỳ vọng*: Class `.dark` duy trì liên tục trên toàn bộ các view, không phát sinh thanh cuộn ngang.
43. **R5-T3-03: Scenario 3.3 - Split Workspace Note Synthesis with Multi-Page PDF**
    - *Luồng*: Trong Mode 2 Split view, mở tài liệu PDF song song với Notebook $\to$ Kích hoạt xuất bản ghi chú Markdown chuẩn Sovereign Markdown.
    - *Kỳ vọng*: Khung xuất Markdown hiển thị đúng cấu trúc tài liệu.
44. **R5-T3-04: Scenario 3.4 - Dynamic Viewport Adaptation during Active Study Session**
    - *Luồng*: Mở phiên học tại Desktop (1440x900) $\to$ Co giãn động về Tablet ngang (1024x768) $\to$ Co giãn tiếp về Tablet dọc (768x1024).
    - *Kỳ vọng*: Giao diện tự động tái cấu trúc mượt mà, `assertZeroHorizontalOverflow` luôn đạt 100%.
45. **R5-T3-05: Scenario 3.5 - Offline Review Mutation Queueing & Reconnection**
    - *Luồng*: Ngắt kết nối mạng trình duyệt (`setOffline(true)`) $\to$ Đánh giá thẻ trong Mode 3 $\to$ Khôi phục mạng (`setOffline(false)`).
    - *Kỳ vọng*: Phiên học không bị gián đoạn hay crash, hàng đợi duy trì tính toàn vẹn.

---

### TIER 4: REAL-WORLD SCENARIOS (4 HÀNH TRÌNH NGƯỜI HỌC TOÀN DIỆN)

46. **R5-T4-01: Scenario 4.1 - The Law Student Journey (298-Page Textbook Ingestion to Mastery)**
    - *Mô phỏng*: Sinh viên Luật tải toàn bộ giáo trình 298 trang, nhảy trang nhanh giữa các chương (trang 1, 150, 298), hoàn thành ôn tập flashcard và tham gia bài kiểm tra tự luận.
    - *Kỳ vọng*: Toàn bộ chu trình học tập diễn ra với 0 lỗi console, tốc độ phản hồi trang ổn định.
47. **R5-T4-02: Scenario 4.2 - Multimodal & Scanned Document Study Session**
    - *Mô phỏng*: Người học làm việc với tài liệu scan đa phương thức: trang 1 có text layer, trang 2 là ảnh scan thuần túy.
    - *Kỳ vọng*: Huy hiệu cảnh báo OCR hiển thị chính xác, canvas hiển thị ảnh rõ nét không méo mó.
48. **R5-T4-03: Scenario 4.3 - Keyboard-Only Power User Ergonomics**
    - *Mô phỏng*: Người dùng chuyên nghiệp điều hướng toàn bộ ứng dụng chỉ bằng bàn phím (phím số 0-3, Enter, Tab, ?, Escape).
    - *Kỳ vọng*: Tiêu điểm (focus indicator) rõ ràng, không bị kẹt trong bẫy tiêu điểm (focus trap).
49. **R5-T4-04: Scenario 4.4 - Resilience, Rate Limiting & Network Error Recovery**
    - *Mô phỏng*: Thử nghiệm độ bền ứng dụng dưới các tác vụ chuyển đổi trang liên tục và tải dữ liệu dồn dập.
    - *Kỳ vọng*: Giao diện duy trì độ ổn định tuyệt đối, không tràn layout, không crash ứng dụng.

---

## 3. NGUỒN CHUẨN ĐỘC LẬP & TÍNH TOÁN KỲ VỌNG (ORACLE & GROUND TRUTH)

Mọi kiểm thử trong bộ test R5 đều được đối chiếu với nguồn chuẩn độc lập (Ground Truth):
1. **PyMuPDF Extraction Oracle (`test-fixtures/oracle.json`)**:
   - Đối chiếu số trang chuẩn (298 trang cho Giáo trình Pháp luật, 305 trang cho tài liệu lớn).
   - Đối chiếu nguyên văn chuỗi văn bản trích xuất trên trang 150 và 298 (bao gồm mã ISBN chuẩn `ISBN: 978-604-73-9848-5`).
2. **Công thức Toán học chuẩn WCAG 2.1 (Relative Luminance & Contrast)**:
   - $L = 0.2126 \cdot R_{linear} + 0.7152 \cdot G_{linear} + 0.0722 \cdot B_{linear}$
   - Tỉ lệ tương phản: $Ratio = (L_1 + 0.05) / (L_2 + 0.05) \ge 4.5:1$ (chuẩn WCAG AA).
3. **Quy chuẩn Công thái học Cảm ứng WCAG 2.5.5 / 2.5.8**:
   - Diện tích tương tác tối thiểu: $\ge 44 \times 44\text{px}$ (hoặc $\ge 36\text{px}$ với inline controls có padding).
4. **Bất biến Layout chống tràn mép ngang**:
   - `document.documentElement.scrollWidth <= window.innerWidth` (Bất biến Zero Horizontal Overflow).

---

## 4. HƯỚNG DẪN THỰC THI KIỂM THỬ (HOW TO RUN)

### 4.1 Khám phá danh mục test (Static Discovery)
Chạy trong thư mục `frontend/`:
```bash
cmd.exe /c "cd frontend && npx playwright test e2e/r5-visual-interactive-e2e.spec.ts --list"
```
*Kết quả xác nhận*: **49 tests in 1 file** được phát hiện đầy đủ, không lỗi import hay cú pháp.

### 4.2 Kiểm tra cú pháp TypeScript (Typecheck)
```bash
cmd.exe /c "cd frontend && npx tsc --noEmit --project tsconfig.json"
```
*Kết quả xác nhận*: 3 tệp mới (`test-base.ts`, `ergonomics-helpers.ts`, `r5-visual-interactive-e2e.spec.ts`) đạt **0 lỗi TypeScript**.

### 4.3 Chạy kiểm thử tự động toàn diện qua Runner dự án
```powershell
pwsh -ExecutionPolicy Bypass -File scripts/verify.ps1
```
Script sẽ tự động:
1. Giải phóng port `5080` và `3000`.
2. Build backend .NET và khởi chạy API.
3. Khởi chạy Next.js frontend.
4. Chạy 37 backend tests.
5. Chạy Playwright E2E suites và báo cáo chi tiết.

---

## 5. KẾT QUẢ XÁC MINH CÔNG BỐ (VERIFICATION STATUS)

- **Cú pháp TypeScript**: 100% hợp lệ, strictly typed.
- **Phát hiện kiểm thử Playwright**: 49/49 test cases sẵn sàng.
- **Chính sách Zero-Error**: Đã tích hợp tự động qua custom fixture `test-base.ts`.
- **Tuân thủ quy hoạch thư mục**: Toàn bộ mã kiểm thử nằm đúng trong `frontend/e2e/`, thư mục `.agents/teamwork/` chỉ chứa metadata và báo cáo.
