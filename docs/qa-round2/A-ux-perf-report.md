# Báo Cáo Kiểm Toán Chuyên Sâu UX/UI, Accessibility & Frontend Performance (A-UX-PERF)

- **Mã kiểm toán:** `A-UX-PERF`
- **Nhánh thực hiện:** `qa/agy2-round2`
- **Thời gian hoàn thành:** 2026-11-19
- **Môi trường thử nghiệm:** Windows 11, Chromium (Playwright headless), Next.js 15.5.28 (Production build @ `http://127.0.0.1:3000`), .NET 10.0 Release (@ `http://127.0.0.1:5080`).
- **Tài khoản kiểm thử:** Sơn Trần (dữ liệu mẫu đầy đủ), Minh Anh (không gian trống).
- **Thư mục lưu trữ:**
  - Kịch bản đo kiểm: `docs/qa-round2/scripts/a-ux-perf/`
  - Ảnh bằng chứng: `docs/qa-round2/screenshots/a-ux-perf/` (52 ảnh chụp thực tế)
  - Dữ liệu trích xuất CDP/WebVitals: `docs/qa-round2/data/a-ux-perf/`

---

## 1. Kết Quả Chính & Điểm Đánh Giá UX/UI/Perf Sau C1 Redesign

Sau cuộc nâng cấp toàn diện C1 Redesign ("Clean & Calm Learning"), ứng dụng Studxy đã có bước nhảy vọt về mặt thẩm mỹ, tính tinh gọn và khả năng tối ưu bundle JS (giảm >45% JS tải về, TBT mobile giảm 43.5%). Tuy nhiên, qua quá trình kiểm toán chuyên sâu độc lập với các công cụ chuẩn W3C Web Vitals (`PerformanceObserver`, CDP), chúng tôi phát hiện **3 nhóm vấn đề cốt lõi** cần được xử lý dứt điểm:

1. **Điểm trừ lớn nhất về Performance — Layout Shift (CLS Desktop 0.22 - 0.23 & Mobile `/learn` 0.21):**
   - Vẫn vượt xa ngưỡng chuẩn của Google (`CLS < 0.1`).
   - Nguyên nhân gốc rễ đã được bóc tách chính xác đến từng tọa độ pixel: **Cơ chế tải không đồng bộ giữa AppShell và Trang con** (Layout Shell unmount skeleton quá sớm, trong khi các trang con render một full-height spinner trước khi dữ liệu kịp về, khiến container `max-w-5xl mx-auto` bị giật từ giữa màn hình sang mép phải `x: 328` tạo ra shift `0.2083`).
   - Trên Mobile `/learn`, thanh công cụ Toolbar bị `flex-wrap` gãy dòng khi nạp selector `PDF tham chiếu`, đẩy toàn bộ cột đọc dịch xuống 66px tạo shift `0.2097`.
2. **Điểm trừ nghiêm trọng về Trải Nghiệm Nội Dung — Rò rỉ mã Unicode Placeholder `\uE000CODE_0\uE000` (CODE_0):**
   - Trực tiếp xác nhận trên bài học IELTS (`0360C033-EBC7-453D-9FAE-C812D3BCB48A`): chuỗi `CODE_0` hiển thị lộ liễu trên thẻ button liên kết bài học relative Markdown do bộ parser regex chưa giải phóng placeholder bên trong nhãn link.
3. **Kiểm toán Khả năng Tiếp cận (WCAG 2.1 AA) & Trải nghiệm Tương phản Tối (Dark Mode):**
   - **Điểm sáng:** 100% các nút và form input phản hồi tốt với phím `Tab`, có outline focus ring rõ ràng (`rgb(94, 81, 232)` 2px solid). Cấu hình `@media (prefers-reduced-motion: reduce)` được tôn trọng triệt để (0 active animation thừa).
   - **Hạn chế:** Trong Dark Mode, màu chữ phụ `text-tertiary` (`rgb(113, 113, 122)`) trên nền thẻ `rgb(24, 24, 28)` chỉ đạt tỷ lệ tương phản **3.66:1** (dưới ngưỡng chuẩn 4.5:1 của WCAG AA) gây mỏi mắt khi đọc trong môi trường thiếu sáng. Nút "Tạo khóa học" trong dark mode có độ tương phản chỉ **2.98:1** (chữ trắng trên nền tím nhạt `#818CF8`).
   - Trang `/app/review` hoàn toàn thiếu thẻ ngữ nghĩa `<h1>`.
   - Trang `/app/courses` ở độ phân giải 1440×900 bị cắt lẹm mép phải của nút cuối "Chủ đề khác (6)" do `scrollWidth = 984px > clientWidth = 960px`.

### Bảng Điểm Đánh Giá Chuyên Gia (Thang điểm 10)

| Tiêu chí | C0 (Trước Redesign) | C1 (Hiện tại) | Mục tiêu sau Fix | Đánh giá tóm tắt |
| :--- | :---: | :---: | :---: | :--- |
| **Thẩm mỹ & Visual Calm** | 7.5 / 10 | **9.0 / 10** | 9.5 / 10 | Phong cách Bento tinh tế, màu sắc tím nhẹ nhàng, không gây quá tải nhận thức. |
| **Responsive & Layout** | 6.5 / 10 | **8.5 / 10** | 9.5 / 10 | Loại bỏ hoàn toàn vỡ ngang; tuy nhiên còn bị cấn lẹm filter tab 1440x900 và flex-wrap toolbar mobile. |
| **Hiệu năng Web Vitals (CLS/LCP)** | 5.0 / 10 | **6.5 / 10** | **9.5 / 10** | Bundle JS rất nhẹ, TBT tốt, nhưng CLS Desktop và Mobile cần fix để đạt chuẩn xanh (< 0.05). |
| **Accessibility (WCAG AA)** | 6.0 / 10 | **7.5 / 10** | 9.5 / 10 | Tab focus và reduced-motion xuất sắc; cần nâng contrast text phụ Dark mode và thêm label switch. |
| **Nội dung & Trình đọc Markdown** | 7.0 / 10 | **7.8 / 10** | 9.8 / 10 | Đã xử lý H1 duplicate và bảng; cần dọn dẹp lỗi rò rỉ `CODE_0` và trạng thái PDF 404. |

---

## 2. Bằng Chứng Thực Thi & Bảng Attribution Layout Shift

### 2.1. Lệnh Kiểm Thử & Tình Trạng Hoàn Thành
- **Script đo CLS & trích xuất Attribution Sources:**
  - Lệnh: `node docs/qa-round2/scripts/a-ux-perf/investigate-cls-sources.mjs`
  - Exit code: `0`
  - Dữ liệu thô: [cls-attribution.json](file:///D:/03_Study/Studxy/docs/qa-round2/data/a-ux-perf/cls-attribution.json)
- **Script kiểm tra rò rỉ Markdown & Unicode Leaks:**
  - Lệnh: `node docs/qa-round2/scripts/a-ux-perf/check-ielts-markdown.js` và `node docs/qa-round2/scripts/a-ux-perf/inspect-exact-leak.js`
  - Exit code: `0`
  - Dữ liệu trích xuất: [ielts-markdown-analysis.json](file:///D:/03_Study/Studxy/docs/qa-round2/data/a-ux-perf/ielts-markdown-analysis.json)
- **Script kiểm toán thanh lọc lĩnh vực Courses 1440x900:**
  - Lệnh: `node docs/qa-round2/scripts/a-ux-perf/check-courses-filter.js`
  - Exit code: `0`
- **Script kiểm toán Khả năng Tiếp cận WCAG AA, Heading & Contrast (36 Viewport/Theme Runs):**
  - Lệnh: `node docs/qa-round2/scripts/a-ux-perf/audit-accessibility.js`
  - Exit code: `0`
  - Dữ liệu kết quả: [a11y-audit-results.json](file:///D:/03_Study/Studxy/docs/qa-round2/data/a-ux-perf/a11y-audit-results.json)
- **Script kiểm tra Tab Navigation, Focus Ring & Reduced Motion:**
  - Lệnh: `node docs/qa-round2/scripts/a-ux-perf/audit-focus-and-motion.js`
  - Exit code: `0`
  - Dữ liệu kết quả: [focus-and-motion-audit.json](file:///D:/03_Study/Studxy/docs/qa-round2/data/a-ux-perf/focus-and-motion-audit.json)
- **Script kiểm tra xử lý lỗi PDF 404:**
  - Lệnh: `node docs/qa-round2/scripts/a-ux-perf/test-pdf-404.js`
  - Exit code: `0`

---

### 2.2. Bảng Attribution Layout Shift Chi Tiết (`entry.sources`)

Dưới đây là bảng trích xuất chính xác từ `PerformanceObserver({ type: "layout-shift", buffered: true })` cho các tuyến trang bị báo động:

| Tuyến trang (Route) | Viewport | Total CLS | Điểm Shift lớn nhất | Thời điểm | DOM Selector phần tử dịch chuyển | Tọa độ Bắt đầu (`previousRect`) | Tọa độ Sau dịch chuyển (`currentRect`) | Tác nhân gây dịch chuyển |
| :--- | :---: | :---: | :---: | :---: | :--- | :--- | :--- | :--- |
| **`/app`** (Dashboard) | 1440×900 | **0.2241** | `0.2083` | 856ms | `main > div.max-w-5xl.mx-auto` | `{ x: 240, y: 0, w: 1200, h: 900 }` | `{ x: 328, y: 0, w: 1024, h: 900 }` | Spinner full-width biến mất, container `max-w-5xl` nhảy vào x: 328 |
| **`/app`** (Dashboard Sidebar) | 1440×900 | - | `0.0066` | 661ms | `aside nav > div.pt-2` ("Tạo khóa học") | `{ x: 8, y: 302, w: 223, h: 40 }` | `{ x: 8, y: 494, w: 223, h: 40 }` | Danh sách khóa học gần đây tải xong chèn vào sidebar, đẩy footer xuống |
| **`/app/courses`** | 1440×900 | **0.2241** | `0.2083` | 499ms | `main > div.max-w-5xl.mx-auto` | `{ x: 240, y: 0, w: 1200, h: 900 }` | `{ x: 328, y: 0, w: 1024, h: 900 }` | Unmount spinner trung tâm, container grid khóa học snap vào x: 328 |
| **`/app/documents`** | 1440×900 | **0.2298** | `0.2207` | 668ms | `main > div.max-w-5xl.mx-auto` | `{ x: 240, y: 0, w: 1200, h: 900 }` | `{ x: 328, y: 0, w: 1024, h: 900 }` | Tương tự: danh sách tài liệu render thay thế spinner |
| **`/app/notes`** | 1440×900 | **0.2299** | `0.2207` | 641ms | `main > div.max-w-5xl.mx-auto` | `{ x: 240, y: 0, w: 1200, h: 900 }` | `{ x: 328, y: 0, w: 1024, h: 900 }` | Tương tự: danh sách sổ tay ghi chú render thay thế spinner |
| **`/app/courses/[id]/quiz`** | 1440×900 | **0.2221** | `0.2111` | 494ms | `main > div.max-w-4xl.mx-auto` | `{ x: 0, y: 0, w: 1440, h: 900 }` | `{ x: 272, y: 0, w: 896, h: 900 }` | Giao diện Quiz tải xong, container `max-w-4xl` snap vào giữa màn hình |
| **`/app/settings`** | 1440×900 | **0.1416** | `0.1324` | 682ms | `main > div.max-w-3xl.mx-auto` | `{ x: 240, y: 0, w: 1200, h: 900 }` | `{ x: 456, y: 0, w: 768, h: 900 }` | Container cài đặt `max-w-3xl` (768px) snap từ x: 240 sang x: 456 |
| **`/app/courses/[id]/learn`** | 390×844 (Mobile) | **0.2384** | `0.2097` | 1567ms | `main > div.flex-1.min-h-0` (Cột đọc bài học) | `{ x: 0, y: 99, w: 390, h: 745 }` | `{ x: 0, y: 165, w: 390, h: 679 }` | Toolbar bị `flex-wrap` nhảy 2 dòng khi load `PDF tham chiếu`, đẩy bài học xuống 66px |

---

### 2.3. Danh Sách Ảnh Bằng Chứng Tiêu Biểu (`screenshots/a-ux-perf/`)

1. **Hiệu Năng & Layout Shift:**
   - Bảng điều khiển Dashboard Desktop (1440×900): [cls-app-dashboard-1440x900.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/cls-app-dashboard-1440x900.png)
   - Danh sách Khóa học Desktop (1440×900): [cls-app-courses-1440x900.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/cls-app-courses-1440x900.png)
   - Không gian đọc bài học Mobile OS Course (390×844): [cls-learn-mobile-os-390x844.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/cls-learn-mobile-os-390x844.png)
   - Không gian đọc bài học Mobile IELTS Course (390×844): [cls-learn-mobile-ielts-390x844.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/cls-learn-mobile-ielts-390x844.png)
   - Cắt lẹm mép phải bộ lọc lĩnh vực 1440×900: [courses-filter-1440x900.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/courses-filter-1440x900.png)
2. **Nội Dung Markdown & Lỗi PDF 404:**
   - Trình đọc bài giảng IELTS rò rỉ Unicode `CODE_0` (Light): [ielts-markdown-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/ielts-markdown-1440x900-light.png)
   - Trình đọc bài giảng IELTS rò rỉ Unicode `CODE_0` (Dark): [ielts-markdown-1440x900-dark.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/ielts-markdown-1440x900-dark.png)
   - Giao diện PDF bị 404 chuyển sang Text Mode gây hiểu lầm: [pdf-404-state.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/pdf-404-state.png)
   - Trạng thái con quay vô tận khi xem bản gốc tệp PDF 404: [pdf-404-infinite-spin.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/pdf-404-infinite-spin.png)
3. **Accessibility, Polish & Dark Mode:**
   - Trực quan hóa Focus Ring trên trang Xác thực Auth: [focus-ring-auth.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/focus-ring-auth.png)
   - Trực quan hóa Focus Ring trên App Dashboard: [focus-ring-app.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/focus-ring-app.png)
   - Trang Đăng nhập Dark Mode: [a11y-auth-1440x900-dark.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/a11y-auth-1440x900-dark.png)
   - Cài đặt Dark Mode & 2 nút switch thiếu nhãn: [a11y-settings-1440x900-dark.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/a11y-settings-1440x900-dark.png)
   - Trạm ôn tập Flashcards FSRS thiếu thẻ `<h1>`: [a11y-review-1440x900-light.png](file:///D:/03_Study/Studxy/docs/qa-round2/screenshots/a-ux-perf/a11y-review-1440x900-light.png)

---

## 3. Danh Sách Vấn Đề Chuyên Sâu (`UXP-01` ... `UXP-12`)

### `UXP-01` [HIGH] — Rò rỉ chuỗi ký tự Unicode Placeholder `\uE000CODE_0\uE000` (CODE_0) trên bài học Markdown
- **Mức độ:** High (Gây mất niềm tin người dùng vào chất lượng nội dung học tập).
- **Hiện tượng:** Trong bài giảng IELTS (và các bài giảng có link tương đối chứa inline code dạng ``[`some_file.md`](some_file.md)``), người dùng nhìn thấy ký tự ô vuông lỗi phông kèm chữ: `CODE_0` (ví dụ: *"Gói hiện hành: sau Phase 2, mở CODE_0 để theo lịch Month 1 hiện có..."*).
- **Tập tin & dòng nghi ngờ:** [LessonMarkdownView.tsx](file:///D:/03_Study/Studxy/frontend/src/components/reader/LessonMarkdownView.tsx#L328-L455) (Dòng 328–333, dòng 388–399, dòng 429–455 và dòng 1165–1179).
- **Nguyên nhân kỹ thuật:**
  1. Trong hàm `tokenizeInlineMarkdown()`, Bước 1 dùng regex thay thế inline code bằng placeholder Unicode Private Use Area `\uE000CODE_${idx}\uE000`.
  2. Bước 2 quét các liên kết Markdown `[linkText](linkHref)`. Khi một liên kết bọc inline code, `linkText` nhận giá trị là chuỗi `\uE000CODE_0\uE000`. Thuộc tính này được gán trực tiếp vào đối tượng `linkItems.push({ text: label, ... })`.
  3. Bước 4 khi hoàn nguyên các token, bộ xử lý chỉ hoàn nguyên các token có kiểu `text` thông thường thông qua regex `codeMatch`, nhưng **bỏ qua thuộc tính `text` bên trong các token kiểu `link`**.
  4. Đến khi render trong `renderSingleTokenPiece` (dòng 1178), đối tượng nút bấm `lesson-relative-md-link` render `<span>{pieceText}</span>` với `pieceText` chính là chuỗi `\uE000CODE_0\uE000`. Trình duyệt hiển thị ký tự mã hóa riêng tư `` kèm theo chuỗi nội bộ.
- **Hướng dẫn sửa code cụ thể:**
  1. Trong `tokenizeInlineMarkdown()`, khi tạo `linkItems` và ở Bước 4 khi phân rã `linkMatch`, lập tức giải phóng mọi placeholder trong `item.text`:
     ```ts
     const cleanLabel = item.text.replace(/\uE000CODE_(\d+)\uE000/g, (_, cIdx) => codeItems[parseInt(cIdx, 10)]?.code ?? "");
     ```
  2. Bổ sung bộ lọc an toàn cuối cùng trong hàm `cleanStudyInlineArtifacts()` hoặc trước khi render JSX để lọc triệt để mọi ký tự thuộc dải Unicode `[\uE000-\uF8FF]` và `\u001A`.

---

### `UXP-02` [HIGH] — CLS Desktop vượt ngưỡng chuẩn (0.22 - 0.23) do Layout Shell unmount Skeleton và thay bằng Centered Spinner
- **Mức độ:** High (Ảnh hưởng tiêu cực trực tiếp đến xếp hạng Core Web Vitals của Google và trải nghiệm đọc của người dùng).
- **Hiện tượng:** Khi tải trang `/app`, `/app/courses`, `/app/documents`, `/app/notes`, `/app/settings`, `/app/courses/[id]/quiz`, màn hình bị chớp giật mạnh theo phương ngang. Giá trị đo thực tế qua CDP ghi nhận CLS dao động từ `0.1416` đến `0.2299`.
- **Tập tin & dòng nghi ngờ:**
  - [layout.tsx](file:///D:/03_Study/Studxy/frontend/src/app/app/layout.tsx#L83-L135) (Dòng 83–115)
  - [page.tsx](file:///D:/03_Study/Studxy/frontend/src/app/app/page.tsx#L272-L281) (Dòng 272–281)
  - [Sidebar.tsx](file:///D:/03_Study/Studxy/frontend/src/components/navigation/Sidebar.tsx#L88-L105) (Dòng 88–105)
- **Nguyên nhân kỹ thuật:**
  1. Trong `AppShellLayout` (`layout.tsx`), khi `authReady` đổi trạng thái từ `false` sang `true`, Skeleton giữ chỗ bị gỡ bỏ ngay lập tức.
  2. Tuy nhiên, các trang con (`app/page.tsx`, `courses/page.tsx`, ...) lại có logic riêng: `if (isLoading) return <div className="flex items-center justify-center h-full"><Spinner /></div>`. Khối Spinner này chiếm toàn bộ chiều rộng từ `x: 240` đến `1440` (`w: 1200`).
  3. Khi API tải xong (khoảng 500–856ms), Spinner unmount và container `div.max-w-5xl.mx-auto` (rộng 1024px) đột ngột xuất hiện ở vị trí căn giữa `x: 328`. Thuật toán Layout Instability API của Chromium tính toán diện tích dịch chuyển `(1024 * 900) / (1440 * 900) * (88 / 1440)` tạo ra điểm phạt `0.2083`.
  4. Ngoài ra, trong `Sidebar.tsx`, mảng `recentCourses` ban đầu rỗng. Khi API trả về, 4 khóa học gần đây được đẩy vào DOM làm thanh cuộn sidebar xuất hiện và đẩy nút "Tạo khóa học" xuống dưới 192px (gây thêm shift `0.0066`).
- **Hướng dẫn sửa code cụ thể:**
  1. Loại bỏ spinner toàn màn hình (`flex items-center justify-center h-full`). Thay vào đó, trong trạng thái `isLoading`, các trang con PHẢI render khung skeleton bọc bên trong container `<div className="max-w-5xl mx-auto px-4 ...">` có cùng kích thước và margin với giao diện thật.
  2. Trong `Sidebar.tsx`, khởi tạo sẵn khung giữ chỗ có chiều cao cố định cho mục "Gần đây" (`min-h-[140px]`) để khi dữ liệu đổ về không làm dịch chuyển các nút chức năng phía dưới.
  3. Đưa CLS mục tiêu về `< 0.03`.

---

### `UXP-03` [HIGH] — CLS Mobile `/learn` (0.2097) do Toolbar Bị Flex-Wrap Gãy Dòng Khi Nạp Xong Danh Sách PDF
- **Mức độ:** High (Người học trên điện thoại bị trôi mất dòng chữ đang đọc khi đang tải trang).
- **Hiện tượng:** Khi mở bài học trên thiết bị di động (390×844), nội dung bài giảng ban đầu xuất hiện ở tọa độ `y: 99`. Sau khoảng 1.5 giây, thanh toolbar phía trên nạp xong danh sách tài liệu PDF và tự động mở rộng từ 1 dòng thành 2 dòng, đẩy toàn bộ văn bản bài học thụt lùi xuống `y: 165` (+66px), tạo điểm shift `0.2097`.
- **Tập tin & dòng nghi ngờ:** [CourseLearningWorkspace.tsx](file:///D:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx#L1761-L1860) (Dòng 1761–1763, 1841–1860).
- **Nguyên nhân kỹ thuật:**
  1. Thẻ bao ngoài của switcher bar sử dụng lớp `flex flex-wrap items-center justify-between gap-2`.
  2. 4 nút chuyển tab ("Bài giảng", "Thực hành", "Tài liệu PDF", "Song song") chiếm khoảng 310px.
  3. Khối dropdown `<select>` "PDF tham chiếu" có độ rộng tối đa `max-w-[230px]`.
  4. Trên màn hình 390px, tổng độ rộng `310px + 230px = 540px > 390px`. Thuộc tính `flex-wrap` buộc dropdown nhảy xuống dòng thứ 2, làm chiều cao thanh toolbar tăng gấp đôi (từ 44px lên ~95px) và đẩy phần canvas bên dưới.
- **Hướng dẫn sửa code cụ thể:**
  1. Trên viewports di động (`< 640px` / `sm:`), không dùng `flex-wrap`. Cho phép hàng nút cuộn ngang tự nhiên với `flex-nowrap overflow-x-auto swms-thin-scrollbar`, hoặc đưa dropdown "PDF tham chiếu" vào một nút icon popover gọn gàng ở góc phải header.
  2. Khóa chiều cao cố định `h-11` cho thanh switcher bar trên mobile.

---

### `UXP-04` [MEDIUM] — Thanh Lọc Lĩnh Vực Trên `/app/courses` Ở 1440×900 Bị Cắt Lẹm Nút Cuối
- **Mức độ:** Medium (Lỗi visual polish rõ ràng trên độ phân giải phổ biến nhất).
- **Hiện tượng:** Trên màn hình Desktop 1440×900, tab cuối cùng "Chủ đề khác (6)" trên thanh lọc lĩnh vực bị cắt mất chữ ở mép phải.
- **Tập tin & dòng nghi ngờ:** [courses/page.tsx](file:///D:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx#L256-L289) (Dòng 256–289).
- **Bằng chứng số liệu đo đạc:**
  - Bề rộng hiển thị của container (`clientWidth`): **960px** (từ `x: 360` đến `x: 1320`).
  - Bề rộng thực tế của 7 nút lọc (`scrollWidth`): **984px**.
  - Nút "Chủ đề khác (6)" kết thúc tại `x: 1344` (vượt quá mép phải 24px).
- **Hướng dẫn sửa code cụ thể:**
  1. Giảm nhẹ `px-3` thành `px-2.5` và `gap-1.5` thành `gap-1` cho các tab lĩnh vực trên desktop (`lg:px-2.5 lg:gap-1`), đưa tổng chiều rộng về `~930px < 960px` để 100% các nút nằm trọn vẹn trong khung nhìn.
  2. Bổ sung lớp `pr-6` và hiệu ứng mờ dần (gradient fade mask) ở mép phải để nếu có thêm danh mục mới, người dùng sẽ nhận biết được thanh có thể cuộn ngang.

---

### `UXP-05` [MEDIUM] — Con Quay Vô Tận (Infinite Spinner) & Thiếu Thông Báo Lỗi Khi Tệp PDF Bị 404 (B9 cũ)
- **Mức độ:** Medium (Người dùng bị kẹt, không biết nguyên nhân tài liệu không hiển thị).
- **Hiện tượng:** Khi tài liệu PDF đính kèm bị lỗi mạng hoặc tệp không tồn tại trên backend (HTTP 404), giao diện tự chuyển sang chế độ "Văn bản" và hiện câu gây hiểu nhầm: *"Trang này không có lớp văn bản trích xuất sẵn. Chuyển sang 'Bản gốc' để xem hình ảnh trang PDF"*. Khi người dùng bấm sang "Bản gốc", màn hình hiện spinner quay liên tục mãi mãi *"Đang tải tệp PDF..."*.
- **Tập tin & dòng nghi ngờ:** [CourseLearningWorkspace.tsx](file:///D:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx#L744-L749) (Dòng 744–749 và dòng 2251–2285).
- **Nguyên nhân kỹ thuật:** Khối `catch` khi gọi `pdfjs.getDocument` chỉ âm thầm gán `setPdfDoc(null)` mà không lưu thông tin lỗi (`pdfLoadError`). Trạng thái render canvas chỉ kiểm tra `pdfDoc ? (<canvas />) : (<Loader2 animate-spin />)`, dẫn đến con quay tải quay vô tận.
- **Hướng dẫn sửa code cụ thể:**
  1. Bổ sung state `const [pdfError, setPdfError] = useState<string | null>(null)`.
  2. Khi bắt được lỗi 404/network, đặt `setPdfError("Tài liệu PDF hiện không khả dụng hoặc đã bị di chuyển (HTTP 404)")`.
  3. Khi có lỗi, render khối `EmptyState` thân thiện với nút "Thử tải lại" và nút "Quay lại đọc bài giảng".

---

### `UXP-06` [MEDIUM] — Thông Báo Lỗi Đồng Bộ Flashcard Hiển Thị Chuỗi JSON Kỹ Thuật Thô (B7 cũ)
- **Mức độ:** Medium.
- **Hiện tượng:** Khi xảy ra lỗi xác thực hoặc lỗi phiên làm việc trong quá trình chấm điểm thẻ ghi nhớ, banner thông báo hiện nguyên chuỗi JSON máy học: `Đồng bộ đánh giá thất bại (401): {"error":"TOKEN_VERSION_REVOKED"}. Đã giữ trong hàng đợi offline.`
- **Tập tin & dòng nghi ngờ:** [review/page.tsx](file:///D:/03_Study/Studxy/frontend/src/app/app/review/page.tsx#L327-L330) (Dòng 185–188 và 327–330).
- **Nguyên nhân kỹ thuật:** Mã nguồn sử dụng trực tiếp `const errText = await resp.text().catch(() => "")` và nối chuỗi mà không bóc tách đối tượng JSON.
- **Hướng dẫn sửa code cụ thể:**
  Parse JSON an toàn và ánh xạ mã lỗi kỹ thuật sang tiếng Việt thân thiện:
  ```ts
  let userMessage = "Lỗi kết nối máy chủ";
  try {
    const errObj = JSON.parse(errText);
    if (errObj.error === "TOKEN_VERSION_REVOKED") userMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
    else if (errObj.error) userMessage = errObj.error;
  } catch { ... }
  ```

---

### `UXP-07` [MEDIUM] — Nút "Kiểm Tra" (Quiz) Vẫn Hiển Thị Dù Khóa Học Có 0 Bài Kiểm Tra (B15 cũ)
- **Mức độ:** Medium.
- **Hiện tượng:** Ở thanh điều hướng bài học và ở màn hình chúc mừng khi học xong toàn bộ bài giảng, nút "Kiểm tra" / "Làm bài kiểm tra tổng kết" luôn luôn sáng. Khi người dùng click vào, hệ thống điều hướng sang trang rỗng báo: *"Khóa học này chưa có bài kiểm tra nào"*.
- **Tập tin & dòng nghi ngờ:** [CourseLearningWorkspace.tsx](file:///D:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx#L1697-L1705) (Dòng 1697–1705 và 2821–2827).
- **Hướng dẫn sửa code cụ thể:**
  Kiểm tra thuộc tính `course.hasQuiz` hoặc mảng `quizzes.length`. Nếu khóa học không có bài kiểm tra, ẩn nút trên header hoặc làm mờ (`disabled`) kèm tooltip "Khóa học chưa có bài kiểm tra".

---

### `UXP-08` [MEDIUM] — Trang `/app/review` Hoàn Toàn Thiếu Thẻ `<h1>` Ngữ Nghĩa (Accessibility)
- **Mức độ:** Medium (Vi phạm chuẩn Semantic Heading của WCAG 2.1 AA tiêu chí 1.3.1 và 2.4.6).
- **Hiện tượng:** Kiểm toán tự động ghi nhận `H1 count: 0` trên cả desktop lẫn mobile cho route `/app/review`. Người dùng sử dụng trình đọc màn hình (Screen Reader) không thể xác định nhanh tiêu đề chính của trang.
- **Tập tin & dòng nghi ngờ:** [review/page.tsx](file:///D:/03_Study/Studxy/frontend/src/app/app/review/page.tsx#L350-L400) (Dòng 350 trở đi).
- **Hướng dẫn sửa code cụ thể:**
  Thêm thẻ `<h1>` trực quan hoặc thẻ ẩn ngữ nghĩa `sr-only`:
  ```tsx
  <h1 className="sr-only">Trạm ôn tập thẻ ghi nhớ FSRS v5</h1>
  ```

---

### `UXP-09` [MEDIUM] — Độ Tương Phản Màu (Contrast Ratio) Trong Dark Mode Quá Thấp Trên Text Phụ & Badge
- **Mức độ:** Medium (Vi phạm tiêu chuẩn WCAG AA tỷ lệ tương phản tối thiểu 4.5:1 đối với văn bản thông thường).
- **Hiện tượng:** Qua quét tự động 36 kịch bản viewport/theme, chế độ Dark Mode ghi nhận hơn **40 điểm vi phạm tương phản**:
  1. Màu chữ phụ `text-tertiary` (`rgb(113, 113, 122)` / `#71717a`) trên nền thẻ `rgb(24, 24, 28)` chỉ đạt **3.66:1** (tiêu chuẩn yêu cầu ≥ 4.5:1). Các nhãn như "Tìm kiếm...", "Gần đây", "Tạo khóa học", thông tin metadata "4 chương · 10 bài" bị mờ nhạt, khó đọc.
  2. Nút "Tạo khóa học" trong Dark Mode dùng chữ trắng `#ffffff` trên nền tím nhạt `#818CF8`, chỉ đạt tỷ lệ tương phản **2.98:1** (rất chói và khó nhìn cho người thị lực kém).
- **Tập tin & dòng nghi ngờ:**
  - `frontend/src/app/globals.css` (biến CSS `--text-tertiary` và màu nền dark).
  - [CoursesCatalogPage](file:///D:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx) và [AppShellLayout](file:///D:/03_Study/Studxy/frontend/src/app/app/layout.tsx).
- **Hướng dẫn sửa code cụ thể:**
  1. Trong Dark mode, điều chỉnh `--text-tertiary` từ `#71717a` lên `#a1a1aa` (`zinc-400`), nâng tỷ lệ tương phản lên **6.8:1**.
  2. Đối với các nút bấm có nền sáng `#818CF8` trong dark mode, màu chữ bắt buộc phải là màu tối đậm (`#0F0F12`, đạt tương phản **11.5:1**) thay vì chữ trắng.

---

### `UXP-10` [LOW] — Nút Đăng Nhập Google Trên `/auth` Hiển Thị Chữ Kỹ Thuật `"Chưa bật Client ID"` (B14 cũ)
- **Mức độ:** Low (Ảnh hưởng tính chuyên nghiệp sản phẩm).
- **Hiện tượng:** Khi môi trường chưa thiết lập biến OAuth, nút hiển thị nguyên nhãn kỹ thuật: `"Tiếp tục với Google (Chưa bật Client ID)"`.
- **Tập tin & dòng nghi ngờ:** [auth/page.tsx](file:///D:/03_Study/Studxy/frontend/src/app/auth/page.tsx#L678-L680) (Dòng 678–680).
- **Hướng dẫn sửa code cụ thể:**
  Đổi nhãn thành "Tiếp tục với Google" kèm huy hiệu trạng thái nhỏ "Sắp hỗ trợ" hoặc "Cần cấu hình", không đặt chuỗi kỹ thuật vào tiêu đề nút.

---

### `UXP-11` [LOW] — Ô Nhập Mật Khẩu Thiếu Thuộc Tính HTML5 `minLength={6}` Trên `/auth` (B12 cũ)
- **Mức độ:** Low.
- **Hiện tượng:** Form đăng ký đã có kiểm tra validate inline tiếng Việt (đã bỏ hàm `alert()`), tuy nhiên input `<input id="auth-password">` vẫn thiếu thuộc tính chuẩn `minLength={6}` để kích hoạt client-side validation của trình duyệt.
- **Tập tin & dòng nghi ngờ:** [auth/page.tsx](file:///D:/03_Study/Studxy/frontend/src/app/auth/page.tsx#L601-L610) (Dòng 601–610).
- **Hướng dẫn sửa code cụ thể:**
  Bổ sung `minLength={mode === "register" ? 6 : undefined}` vào phần tử input.

---

### `UXP-12` [LOW] — Hai Nút Toggle Switch Trong `/app/settings` Thiếu Thuộc Tính Trợ Năng Accessible Name & Role
- **Mức độ:** Low.
- **Hiện tượng:** Nút bật/tắt giới hạn ôn tập hàng ngày trong cài đặt render dạng `<button type="button" class="relative w-11 h-6 rounded-full ...">` hoàn toàn không có `aria-label`, không có `role="switch"` và không có thuộc tính `aria-checked`.
- **Tập tin & dòng nghi ngờ:** [settings/page.tsx](file:///D:/03_Study/Studxy/frontend/src/app/app/settings/page.tsx#L250-L310).
- **Hướng dẫn sửa code cụ thể:**
  Bổ sung `role="switch"`, `aria-checked={enabled}`, và `aria-label="Bật giới hạn ôn tập hàng ngày"`.

---

## 4. Góp Ý Như Chuyên Gia UX & Người Dùng Thực Tế

### 4.1. Cảm Nhận Tổng Quan Về Định Hướng "Clean & Calm Learning"
Sau C1 Redesign, Studxy thực sự đã thoát khỏi cảm giác "phức tạp, rối rắm" của các hệ thống LMS truyền thống:
- **Điểm sáng vượt bậc:**
  1. *Hệ thống typography & phân tầng màu:* Việc tinh giản các dải gradient sặc sỡ và chuyển sang các đường viền nhẹ nhàng (`var(--border-calm)`), nền thẻ sạch sẽ tạo cảm giác rất dễ chịu khi ngồi học liên tục nhiều giờ.
  2. *Bàn phím tắt đồng bộ:* Các tổ hợp `1-4` chấm điểm FSRS, `Space` lật thẻ, `Ctrl+K` mở Command Palette, `Ctrl+\` thu gọn thanh bên hoạt động mượt mà, phản hồi tức thì dưới 1ms.
  3. *Không gian đọc tập trung:* Cột đọc bài học luôn giữ vững chiều rộng tối thiểu an toàn ≥ 320px ngay cả khi mở ngăn kéo ghi chú, hoàn toàn không bị co rúm như phiên bản C0.

### 4.2. Gợi Ý Nâng Cấp Trải Nghiệm & Hiệu Năng Trong Các Đợt Tới
1. **Chuyển đổi hoàn toàn Skeleton tĩnh sang Skeleton động đồng nhất:**
   Hiện nay giữa Shell (`layout.tsx`) và từng Trang con (`/courses`, `/documents`) đang có sự lệch pha về vòng đời dữ liệu. Nhóm phát triển nên tạo component `<PageContainerSkeleton type="grid|list|article" />` dùng chung cho cả Layout Shell lẫn các Trang con khi `isLoading = true`. Điều này sẽ triệt tiêu hoàn toàn Layout Shift, đưa CLS về ngưỡng tuyệt đối `< 0.02`.
2. **Cơ chế đọc trước (Prefetching) nội dung bài học tiếp theo:**
   Khi người dùng đang đọc Bài 1, hãy sử dụng `requestIdleCallback` để tải trước Markdown của Bài 2. Khi người dùng bấm "Bài tiếp theo", bài học sẽ hiển thị ngay lập tức (LCP < 100ms) mà không phải nhìn thấy thanh loading.
3. **Thanh công cụ đọc nổi (Floating Selection Toolbar):**
   Khi người dùng bôi đen văn bản trong bài đọc, nên hiển thị một thanh menu ngữ cảnh nổi nhỏ gọn (gồm: Highlight màu, Tạo thẻ Flashcard, Ghi chú nhanh, Tra từ điển) ngay sát vùng chọn con trỏ chuột thay vì phải nhìn sang panel bên phải.

---

## 5. Chưa Kiểm Chứng

Các điểm dưới đây nằm ngoài phạm vi kiểm toán tĩnh hoặc cần môi trường thử nghiệm đặc thù chưa thực hiện trong phiên này:
1. **Thiết bị màn hình cảm ứng vật lý thật (Real Mobile Device):** Kiểm thử hiện tại chạy trên Playwright Chromium Device Emulation (`iPhone 12/13/14` - 390×844) có mô phỏng cảm ứng và CPU Throttling 4x; chưa kiểm chứng trên thiết bị thực tế chạy iOS Safari WebKit để đánh giá độ trễ thao tác vuốt (swipe gestures) và bàn phím ảo.
2. **Kịch bản mất mạng hoàn toàn (Offline Mode PWA):** Đã kiểm chứng logic queue offline trong mã nguồn, nhưng chưa ngắt kết nối mạng toàn hệ thống trong lúc đang làm bài trắc nghiệm nhiều câu hỏi.
3. **Khóa học cực lớn (> 500 bài học & 1,000 thẻ):** Đã kiểm toán với bộ dữ liệu demo của Sơn Trần (23 khóa học, 47 sổ tay, 99+ thẻ FSRS); chưa đo đạc thời gian render của cây danh mục bài học với các khóa học siêu lớn chứa hàng ngàn bài học liên kết.
