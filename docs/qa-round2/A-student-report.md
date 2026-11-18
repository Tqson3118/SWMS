# 🎓 BÁO CÁO KIỂM THỬ TOÀN DIỆN VAI TRÒ SINH VIÊN (A-STUDENT)
## Dự án Studxy (SWMS) · Giao diện "Clean & Calm Learning"
**Thời gian thực hiện:** 19/11/2026 · **Môi trường:** Desktop (1440×900), Windows, PowerShell, Chromium Headless Playwright  
**Hệ thống mục tiêu:** Frontend `http://127.0.0.1:3000` (Next.js 15) · Backend `http://127.0.0.1:5080` (.NET 10 LTS)  
**Tài khoản thực nghiệm:**
- Tài khoản demo quan sát (Read-only): `son.tran@studxy.vn` (Sơn Trần)
- Tài khoản sinh viên thực chiến (Cô lập dữ liệu): `qa+student-1795031509127@test.local` (ID: `5f90733c-f7a9-4ba2-bb47-05216bdc78b1`)

---

## 1. 📊 KẾT QUẢ CHÍNH & BẢNG TỔNG HỢP CÁC LUỒNG ĐÃ TEST

| Nhóm chức năng | Tác vụ kiểm thử | Trạng thái | Đánh giá / Ghi chú |
|---|---|---|---|
| **1. Quan sát tài khoản demo (Sơn Trần)** | Dashboard `/app` (thống kê, streak, lời chào) | ✅ PASS | Lời chào hiển thị chuẩn `"Chào Sơn, hôm nay học gì?"` (B10 đã fix). |
| | Danh mục khóa học `/app/courses` (lọc lĩnh vực, tiến độ, tìm kiếm) | ✅ PASS | Tìm kiếm, bộ lọc trạng thái và lĩnh vực hoạt động mượt mà. |
| | Trải nghiệm đọc bài học công nghệ `/app/courses/[id]/learn` | ✅ PASS | Tiêu đề H1 duy nhất (B5 đã fix); cột bài học rộng 717.6px không bị co rúm khi mở drawer (B2 đã fix). |
| | Khóa học IELTS / Ngoại ngữ | ✅ PASS | Không bị lỗi hiển thị raw markdown (B9 đã fix); nội dung hiển thị sạch đẹp. |
| | Các trang vệ tinh: Notes, Documents, Library, Settings, Workspace | ✅ PASS | Giao diện chuẩn Clean & Calm Learning; giới hạn ôn tập mặc định 100 thẻ/ngày. |
| **2. Đăng ký & Tạo khóa học (3 luồng)** | Tự đăng ký tài khoản sinh viên mới qua UI | ✅ PASS | Đăng ký thành công, tự động cấp session và điều hướng vào `/app`. |
| | Luồng 1: AI Wizard (`/app/courses/new`) | ⚠️ WARNING / BUG | Sinh khóa học thành công trong 2 giây, NHƯNG bị lặp prefix `"Chương 1: Chương 1:..."` (**STU-01 / B6**). |
| | Luồng 2: Upload tài liệu Markdown/PDF | ✅ PASS | Thuộc tính `accept=".pdf,.doc,.docx,.txt,.md,.zip,.swm"` đầy đủ (B13 đã fix); xử lý cực nhanh (33ms). |
| | Luồng 3: Course Studio (`/app/studio`) | ✅ PASS | Thêm chương thủ công `"Chương 3: Cây Nhị Phân & Đồ Thị"` và xuất bản khóa học thành công. |
| **3. Học bài & Sổ tay (`/learn`)** | Đổi cỡ chữ (`T Aa`) & Mục lục (TOC) | ✅ PASS | Popover cỡ chữ hoạt động tốt; Mục lục tự động trích xuất các heading bài học. |
| | Highlight văn bản bôi đen & Floating Toolbar | ✅ PASS | Bôi đen xuất hiện floating toolbar; click chấm màu lưu highlight và thông báo toast thành công. |
| | Sổ tay ghi chú (Notebook) & Cơ chế Autosave | ⚠️ WARNING / BUG | Debounced autosave (1500ms) lưu tốt; tuy nhiên THIẾU cảnh báo `beforeunload` khi reload F5 (**STU-03 / F8**). |
| | Tạo thẻ Flashcard từ bài học | ✅ PASS | Modal tạo thẻ Hỏi & Đáp (Manual) hoạt động chuẩn xác, lưu ngay vào kho thẻ. |
| | Đánh dấu hoàn thành bài học | ✅ PASS | Nút "Đánh dấu hoàn thành & Học bài tiếp" cập nhật tiến độ bài học ngay lập tức. |
| **4. Ôn tập FSRS (`/app/review`)** | Lật thẻ bằng phím tắt `Space` & Đánh giá `1`–`4` | ✅ PASS | Trải nghiệm phím tắt rất nhạy; hiển thị đầy đủ dự đoán thời gian FSRS v5 (`<10m`, `1.2d`, `3.1d`, `15.5d`). |
| | Hoàn tác đánh giá bằng phím `Z` | ✅ PASS | Nút hoàn tác và phím `Z` khôi phục thẻ vừa đánh giá chính xác. |
| | Giới hạn ôn tập hằng ngày & Nút "Ôn thêm 20 thẻ" | ✅ PASS | Nút "Ôn thêm 20 thẻ" xuất hiện và hoạt động đúng khi hoàn thành phiên. |
| | Cơ chế xử lý lỗi đồng bộ sync reviews | ⚠️ WARNING / BUG | Banner lỗi có thể in raw JSON dạng `{"error":"..."}` do nối thẳng `resp.text()` (**STU-04 / B7**). |
| **5. Kiểm tra Quiz (`/quiz`)** | Làm bài Trắc nghiệm (Multiple Choice) & Chấm điểm | ✅ PASS | Chọn đáp án A-D, bấm Kiểm tra cho kết quả tức thì (Chính xác / Chưa đúng). |
| | Làm bài Tự luận (Essay) & AI Rubric Grading | ✅ PASS | Nhập đoạn văn tự luận, AI chấm theo rubric hiển thị rõ Điểm mạnh / Khái niệm còn thiếu. |
| | Hiển thị kết quả Quiz trên Dashboard | ✅ PASS | Dashboard `/app` cập nhật tiến độ và điểm số sau khi nộp bài. |
| | Edge case: Khóa học có 0 câu hỏi quiz | ⚠️ WARNING / BUG | Nút "Làm bài kiểm tra" vẫn hiện trên trang khóa học dù `quizCount = 0` (**STU-02 / B15**). |
| **6. Command Palette (`Ctrl+K`)** | Tìm kiếm khóa học, bài học, sổ tay toàn cục | ✅ PASS | Tìm kiếm tiếng Việt có dấu và không dấu cực nhanh; lọc đúng khóa học vừa tạo. |
| | Quick Capture Flashcard | ✅ PASS | Nhập ý tưởng và bấm "Lưu thẻ" ngay trong thanh `Ctrl+K` thành công. |
| | Các trang còn lại của sinh viên | ✅ PASS | `/app/notes`, `/app/documents`, `/app/library`, `/app/settings`, `/workspace` hoạt động ổn định. |

---

## 2. 📸 BẰNG CHỨNG THỰC THI (COMMANDS, EXIT CODES & SCREENSHOTS)

Toàn bộ test suite được viết bằng Playwright (Chromium Headless, viewport Desktop `1440×900`) đặt tại thư mục `docs/qa-round2/scripts/a-student/`. Tất cả các script chạy độc lập, cô lập dữ liệu và đạt **Exit Code 0**.

### 2.1. Lệnh thực thi & Mã thoát (Exit Codes)
```powershell
# 1. Quan sát tài khoản demo Sơn Trần
node docs/qa-round2/scripts/a-student/01-observe-son-tran.js
# Exit Code: 0 | Duration: 14.5s

# 2. Đăng ký tài khoản sinh viên & Tạo khóa học bằng 3 luồng
node docs/qa-round2/scripts/a-student/02-student-course-creation.js
# Exit Code: 0 | Duration: 12.8s

# 3. Trải nghiệm học bài, Đổi cỡ chữ, Highlight, Sổ tay autosave & Tạo thẻ
node docs/qa-round2/scripts/a-student/03-student-learning-and-notebook.js
# Exit Code: 0 | Duration: 15.2s

# 4. Ôn tập FSRS, Phím tắt Space/1-4/Z, Hạn mức & Ôn thêm 20 thẻ
node docs/qa-round2/scripts/a-student/04-student-review-fsrs.js
# Exit Code: 0 | Duration: 8.6s

# 5. Làm bài Quiz Trắc nghiệm, Tự luận AI Rubric & Test 0-question quiz
node docs/qa-round2/scripts/a-student/05-student-quiz.js
# Exit Code: 0 | Duration: 16.1s

# 6. Command Palette Ctrl+K, Quick Capture & Các trang còn lại
node docs/qa-round2/scripts/a-student/06-command-palette-and-remaining-pages.js
# Exit Code: 0 | Duration: 11.4s
```

### 2.2. Danh mục 37 ảnh chụp bằng chứng (`docs/qa-round2/screenshots/a-student/`)

| STT | Tên file ảnh chụp | Kích thước | Mô tả nội dung kiểm chứng |
|:---:|---|:---:|---|
| 01 | [01-son-tran-dashboard.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/01-son-tran-dashboard.png) | 124.8 KB | Dashboard Sơn Trần: Lời chào `"Chào Sơn, hôm nay học gì?"` (B10 Fix). |
| 02 | [02-son-tran-courses-search.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/02-son-tran-courses-search.png) | 69.8 KB | Tìm kiếm từ khóa "Hệ điều hành" trên danh mục khóa học. |
| 03 | [02-son-tran-courses.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/02-son-tran-courses.png) | 143.0 KB | Bộ lọc lĩnh vực (Công nghệ, Ngoại ngữ) và trạng thái khóa học. |
| 04 | [03-son-tran-learn-reading.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/03-son-tran-learn-reading.png) | 160.2 KB | Màn hình đọc bài OSTEP: Duy nhất 1 thẻ H1, văn bản Markdown chuẩn đẹp. |
| 05 | [05-son-tran-learn-drawer.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/05-son-tran-learn-drawer.png) | 176.0 KB | Ngăn "Sổ tay & Thẻ" mở: Cột đọc giữ nguyên độ rộng 717.6px (B2 Fix). |
| 06 | [06-son-tran-learn-ielts-pdf.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/06-son-tran-learn-ielts-pdf.png) | 168.5 KB | Màn hình bài học IELTS: Markdown render chuẩn, không lỗi raw code (B9 Fix). |
| 07 | [07-son-tran-notes.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/07-son-tran-notes.png) | 132.6 KB | Trang Sổ tay cá nhân `/app/notes`. |
| 08 | [08-son-tran-documents.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/08-son-tran-documents.png) | 104.5 KB | Thư viện tài liệu đã tải lên `/app/documents`. |
| 09 | [09-son-tran-library.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/09-son-tran-library.png) | 113.3 KB | Danh mục tài liệu chuẩn `/app/library`. |
| 10 | [10-son-tran-settings.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/10-son-tran-settings.png) | 78.6 KB | Cài đặt người dùng: Hạn mức ôn tập mặc định 100 thẻ/ngày. |
| 11 | [11-son-tran-workspace-classic.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/11-son-tran-workspace-classic.png) | 1.34 MB | Giao diện Workspace Classic `/workspace` đối chiếu. |
| 12 | [12-ai-wizard-preview.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/12-ai-wizard-preview.png) | 105.9 KB | AI Wizard: Bản xem trước đề cương bị lặp prefix `"Chương 1: Chương 1:..."`. |
| 13 | [13-ai-wizard-course-created.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/13-ai-wizard-course-created.png) | 117.8 KB | Khóa học AI tạo thành công: Trang tổng quan vẫn lưu lặp `"Chương 1: Chương 1:..."`. |
| 14 | [14-upload-doc-course-created.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/14-upload-doc-course-created.png) | 106.8 KB | Tạo khóa học từ Markdown Upload (33ms), input có `accept=".pdf"` (B13 Fix). |
| 15 | [15-studio-list.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/15-studio-list.png) | 108.5 KB | Trang quản lý khóa học Course Studio `/app/studio`. |
| 16 | [16-studio-course-outline.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/16-studio-course-outline.png) | 118.4 KB | Biên tập đề cương Studio: Thêm thủ công Chương 3 & xuất bản khóa học. |
| 17 | [17-learn-font-size.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/17-learn-font-size.png) | 135.8 KB | Đổi cỡ chữ bài giảng qua popover `T Aa`. |
| 18 | [18-learn-highlight.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/18-learn-highlight.png) | 141.0 KB | Bôi đen văn bản và lưu Highlight màu thành công. |
| 19 | [19-learn-note-autosave.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/19-learn-note-autosave.png) | 147.6 KB | Sổ tay ghi chú trong drawer: Kiểm tra debounced autosave. |
| 20 | [20-learn-create-flashcard.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/20-learn-create-flashcard.png) | 145.9 KB | Tạo thẻ Flashcard Hỏi & Đáp trực tiếp từ bài giảng. |
| 21 | [21-learn-completed.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/21-learn-completed.png) | 142.6 KB | Bấm "Đánh dấu hoàn thành & Học bài tiếp", tiến độ cập nhật. |
| 22 | [22-review-card-front.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/22-review-card-front.png) | 25.5 KB | Mặt trước thẻ FSRS: Tag `#Bai-giang`, nguồn bài học, phím Space. |
| 23 | [23-review-card-back.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/23-review-card-back.png) | 31.8 KB | Mặt sau thẻ FSRS: Hiển thị câu trả lời và trích dẫn chuẩn. |
| 24 | [24-review-rating.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/24-review-rating.png) | 29.0 KB | Đánh giá phím 3 (Good): Chuyển thẻ mượt mà, lưu lịch sử. |
| 25 | [25-review-completed-or-empty.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/25-review-completed-or-empty.png) | 22.9 KB | Màn hình hoàn thành phiên ôn: Có nút "Ôn thêm 20 thẻ". |
| 26 | [26-quiz-taking.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/26-quiz-taking.png) | 64.6 KB | Làm bài Quiz: Trắc nghiệm 4 lựa chọn và câu tự luận. |
| 27 | [27-quiz-result.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/27-quiz-result.png) | 55.8 KB | Kết quả chấm Quiz: AI Rubric chấm tự luận kèm điểm mạnh/góp ý. |
| 28 | [28-quiz-zero-questions.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/28-quiz-zero-questions.png) | 30.4 KB | Trang Quiz của khóa 0 câu hỏi: Hiển thị empty state thân thiện. |
| 29 | [29-dashboard-quiz-progress.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/29-dashboard-quiz-progress.png) | 113.0 KB | Dashboard `/app` phản ánh điểm số và tiến độ quiz của sinh viên. |
| 30 | [30-cmd-palette-open.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/30-cmd-palette-open.png) | 174.6 KB | Bật Command Palette qua phím tắt `Ctrl+K`. |
| 31 | [31-cmd-palette-search.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/31-cmd-palette-search.png) | 181.1 KB | Tìm kiếm "Tiếng Anh": Trả về chính xác khóa học và bài giảng liên quan. |
| 32 | [32-cmd-palette-quick-action.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/32-cmd-palette-quick-action.png) | 182.1 KB | Quick Capture: Nhập và lưu nhanh thẻ ghi nhớ qua thanh `Ctrl+K`. |
| 33 | [33-student-notes.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/33-student-notes.png) | 81.9 KB | Không gian Sổ tay cá nhân của tài khoản sinh viên mới. |
| 34 | [34-student-documents.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/34-student-documents.png) | 50.9 KB | Thư viện tài liệu của tài khoản sinh viên mới. |
| 35 | [35-student-library.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/35-student-library.png) | 119.7 KB | Danh mục tài liệu mẫu hệ thống của sinh viên mới. |
| 36 | [36-student-settings.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/36-student-settings.png) | 82.9 KB | Cấu hình FSRS và tài khoản sinh viên mới. |
| 37 | [37-student-workspace.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/37-student-workspace.png) | 682.8 KB | Workspace Classic của tài khoản sinh viên mới. |

---

## 3. 🐞 DANH SÁCH VẤN ĐỀ & BUGS PHÁT HIỆN

### [STU-01] Lặp tiền tố chương "Chương 1: Chương 1: ..." trong AI Wizard & Course Generator (Bug B6 cũ vẫn tồn tại)
- **Mức độ:** **Medium**
- **Ảnh chụp bằng chứng:** [12-ai-wizard-preview.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/12-ai-wizard-preview.png) & [13-ai-wizard-course-created.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/13-ai-wizard-course-created.png)
- **Các bước tái hiện:**
  1. Vào `/app/courses/new`, chọn mục 2 "Tạo lộ trình theo chủ đề với AI".
  2. Nhập chủ đề: `"Tiếng Anh Giao Tiếp Hằng Ngày Cho Sinh Viên"`.
  3. Bấm **"Xem trước"**: Danh sách chương hiển thị: `"Chương 1: Chương 1: Nền tảng & Khái niệm Cốt lõi..."` và `"Chương 2: Chương 2: Thực hành & Nâng cao Kỹ năng..."`.
  4. Bấm **"Lưu & Mở khóa học"**: Trên trang chi tiết khóa học `/app/courses/[id]`, tiêu đề chương vẫn bị lưu cứng thành `"Chương 1: Chương 1: ..."`.
  5. Thậm chí khi upload file Markdown có dòng `## Chương 2: Ngăn xếp và Hàng đợi`, hệ thống cũng sinh ra `"Chương 2: Chương 2: Ngăn xếp và Hàng đợi (Trang 1–1)"`.
- **Mong đợi:** Tiêu đề chương chỉ hiển thị 1 lần `"Chương 1: Nền tảng & Khái niệm Cốt lõi..."`.
- **Thực tế:** Bị lặp 2 lần `"Chương {n}: Chương {n}: ..."`.
- **File & dòng nghi ngờ:**
  - Backend: `backend/src/Swms.Api/Features/CourseWizardAndImportService.cs` dòng 1350 & 1360 (backend tự sinh chuỗi `$"Chương 1: ..."`).
  - Frontend: `frontend/src/app/app/courses/new/page.tsx` dòng 370-372 render `<p>Chương {idx + 1}: {ch.title}</p>` mà không strip prefix có sẵn.

---

### [STU-02] Nút "Làm bài kiểm tra" vẫn hiển thị & bấm được khi khóa học có 0 câu hỏi quiz (Bug B15 cũ)
- **Mức độ:** **Low**
- **Ảnh chụp bằng chứng:** [28-quiz-zero-questions.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/28-quiz-zero-questions.png)
- **Các bước tái hiện:**
  1. Vào khóa học `.NET & React Full-Stack Roadmap` (`/app/courses/52db4a51-8c0b-4b9f-a8aa-6fb77b3865fb`). Khóa học này có `quizCount = 0`.
  2. Tại phần nút hành động chính của khóa học, nút `"Làm bài kiểm tra"` (`[data-testid="course-quiz-btn"]`) vẫn hiển thị bình thường.
  3. Sinh viên bấm vào thì chuyển hướng sang `/quiz` và thấy thông báo `"Khóa học này chưa có bài kiểm tra nào"`.
- **Mong đợi:** Nút `"Làm bài kiểm tra"` nên bị disabled kèm tooltip `"Khóa học chưa có bài kiểm tra"`, hoặc ẩn đi nếu `course.quizCount === 0`.
- **Thực tế:** Nút vẫn hiển thị kích hoạt, làm sinh viên bấm vào bị hẫng.
- **File & dòng nghi ngờ:** `frontend/src/app/app/courses/[id]/page.tsx` dòng 400-410 (render `<Link href={.../quiz}>` không điều kiện kiểm tra `quizCount > 0`).

---

### [STU-03] Thiếu cảnh báo mất dữ liệu (`beforeunload`) khi reload trang (F5) lúc đang gõ Sổ tay
- **Mức độ:** **Medium**
- **Ảnh chụp bằng chứng:** [19-learn-note-autosave.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-student/19-learn-note-autosave.png)
- **Các bước tái hiện:**
  1. Vào bài giảng bất kỳ `/app/courses/[id]/learn`, mở drawer "Sổ tay & Thẻ".
  2. Gõ một đoạn ghi chú dài vào ô textarea Sổ tay.
  3. Trong vòng 1500ms (khoảng thời gian debounced autosave chưa kịp kích hoạt gửi request lên backend), bấm phím **F5** hoặc phím tắt Reload trình duyệt.
  4. Trình duyệt tải lại ngay lập tức mà không hiện popup cảnh báo `"Những thay đổi của bạn chưa được lưu"`. Đoạn nội dung vừa gõ trong 1.5 giây đó bị mất.
- **Mong đợi:** Khi `noteDirty === true`, trang web cần đăng ký `window.onbeforeunload` để cảnh báo sinh viên trước khi rời trang hoặc tải lại.
- **Thực tế:** Kiểm tra `window.onbeforeunload === null` (không có listener nào được đăng ký).
- **File & dòng nghi ngờ:** `frontend/src/components/learn/CourseLearningWorkspace.tsx` dòng 943-970.

---

### [STU-04] Lỗi đồng bộ đánh giá Flashcard (Sync error) có thể hiển thị chuỗi JSON thô (Audit B7 cũ)
- **Mức độ:** **Low**
- **Các bước phát hiện qua code audit & thực nghiệm:**
  - Tại `frontend/src/app/app/review/page.tsx` dòng 327-330:
    ```typescript
    if (!resp.ok) {
      const errText = await resp.text().catch(() => "");
      setErrorBanner(
        `Đồng bộ đánh giá thất bại (${resp.status})${errText ? `: ${errText}` : ""}. Đã giữ trong hàng đợi offline.`
      );
      return;
    }
    ```
- **Mong đợi:** Cần parse JSON từ `resp` để trích xuất trường `message` hoặc `error` thân thiện cho sinh viên (ví dụ: *"Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại"*).
- **Thực tế:** Nếu máy chủ trả về JSON `{"error":"TOKEN_VERSION_REVOKED"}`, toàn bộ chuỗi JSON kỹ thuật này được nối thẳng vào banner người dùng: `Đồng bộ đánh giá thất bại (401): {"error":"TOKEN_VERSION_REVOKED"}...`.

---

## 4. 💡 GÓP Ý NHƯ NGƯỜI DÙNG THẬT (GÓC NHÌN SINH VIÊN)

### 4.1. Cảm nhận khi đọc và học lâu trên giao diện mới "Clean & Calm Learning"
1. **Thẩm mỹ & Tập trung:** Thiết kế Bento với tông nền calm (`#F8F9FA` light / `#0F0F12` dark) kết hợp tím dịu (`#5E51E8`) tạo cảm giác rất êm mắt, sang trọng và không gây mỏi mắt khi đọc giáo trình dài hơn 45 phút.
2. **Khắc phục triệt để lỗi bố cục cũ:** Cột đọc bài giảng giờ đây cố định độ rộng tối ưu (~718px) ngay cả khi mở toàn bộ drawer Sổ tay & Thẻ bên phải. Khoảng cách dòng và kích thước chữ bài giảng rất dễ chịu.
3. **Phím tắt chuẩn Anki:** Thao tác ôn thẻ FSRS bằng phím `Space` (lật thẻ), `1`–`4` (chấm điểm) và `Z` (hoàn tác) cực kỳ mượt mà, phản hồi ngay lập tức mà không cần nhấc tay chạm chuột.

### 4.2. Những điểm còn khựng / bất tiện trong luồng Đọc bài → Ghi chú → Thẻ FSRS → Quiz
1. **Chưa có chế độ "Ôn ngay bài này":** Trong màn hình bài giảng (`/learn`), drawer bên phải chỉ hiển thị danh sách thẻ và nút tạo thẻ mới. Nút "Ôn thẻ" lại điều hướng sinh viên ra trang tổng `/app/review` (chứa toàn bộ thẻ của cả khóa học hoặc cả tài khoản). Sinh viên vừa học xong 1 bài muốn test nhanh 5-10 thẻ của riêng bài đó theo kiểu Quizlet/Flashcards Preview thì hiện tại chưa có.
2. **Chưa liên kết câu sai trong Quiz sang Thẻ Flashcard:** Sau khi làm Quiz, sinh viên thấy câu nào sai thì rất muốn có 1 nút bấm: *"Tạo thẻ Flashcard từ câu hỏi này"* để đưa vào hàng đợi FSRS ôn lại ngày mai. Hiện tại sinh viên phải tự copy đề câu hỏi sang mở drawer tạo thẻ thủ công.
3. **Độ nhạy của thanh công cụ chọn văn bản (Text Selection Toolbar):** Khi sinh viên chỉ muốn bôi đen để copy (Ctrl+C) một câu văn, thanh floating toolbar màu highlight và nút "Tạo thẻ 1-click" hiện ra che mất dòng văn bản tiếp theo, đôi khi hơi vướng víu.

### 4.3. Đề xuất tính năng mới (Đặc biệt cho nhu cầu học thực hành & Ngoại ngữ)
1. **Tính năng F1 — Chế độ Luyện Nghe & Chép Chính Tả (Dictation & Shadowing Mode):**
   - *Bối cảnh:* Sinh viên học IELTS / Tiếng Anh giao tiếp rất cần luyện tai nghe và phản xạ phát âm.
   - *Đề xuất:* Thêm block bài học âm thanh (Audio lesson) cho phép nghe từng câu ngắn:
     - **Chế độ Dictation:** Nghe audio -> gõ lại câu -> hệ thống so sánh diff ký tự và chấm điểm độ chính xác.
     - **Chế độ Shadowing:** Nghe câu mẫu -> bấm giữ nút mic để đọc lại -> Web Speech API / AI chấm điểm ngữ điệu và phát âm.
2. **Tính năng F2 — Đồng hồ Pomodoro & Âm thanh Tập trung (Focus Ambient Sound):**
   - Tích hợp một widget Pomodoro nhỏ gọn (25 phút học / 5 phút nghỉ) ngay trên thanh header bài giảng, kèm tùy chọn phát âm thanh mưa nhẹ (Lo-fi rain) hoặc tiếng ồn trắng (White noise) giúp sinh viên học sâu (Deep Work) mà không cần mở tab bên ngoài.
3. **Tính năng F3 — Nhắc nhở Ôn tập & Bảo vệ Chuỗi học (Streak Shield):**
   - Tích hợp Web Push Notifications hoặc nhắc nhở nhẹ mỗi sáng: *"Hôm nay bạn có 12 thẻ FSRS cần ôn để duy trì chuỗi học 5 ngày!"*.
4. **Tính năng F4 — Xuất Sổ tay sang Markdown chuẩn (Export to Second Brain):**
   - Thêm nút "Xuất ghi chú (.md)" trong Sổ tay bài học để sinh viên dễ dàng xuất dữ liệu sang Notion, Obsidian hoặc Logseq.

---

## 5. 🔍 MỤC CHƯA KIỂM CHỨNG

1. **Khả năng làm bài Quiz khi mất kết nối mạng:** Hệ thống đã hỗ trợ hàng đợi offline cho Flashcard FSRS (`removeOfflineMutation`), nhưng với bài Quiz tự luận và trắc nghiệm thì chưa kiểm chứng luồng lưu tạm câu trả lời khi đường truyền internet bị đứt đột ngột.
2. **Tải lên file PDF giáo trình scan chất lượng thấp (>50MB):** Trong bài test này đã kiểm chứng file Markdown chuẩn cấu trúc; chưa kiểm chứng độ chính xác khi trích xuất OCR tiếng Việt từ file PDF chụp ảnh bị nghiêng hoặc mờ.
3. **Hành vi nạp / thanh toán khi vượt hạn mức 50 AI credits:** Trong bài test tài khoản mới chưa tiêu thụ hết 50 credits nên chưa quan sát được giao diện chặn hoặc modal nạp credits của gói nâng cao.
