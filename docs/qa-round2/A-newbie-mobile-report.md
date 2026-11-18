# Báo Cáo Đánh Giá Trải Nghiệm Người Dùng Mới Trên Mobile & Tablet (A-NEWBIE-MOBILE)

- **Môi trường thử nghiệm:** Windows 10/11, Node.js v24.19.0, Playwright (Chromium Headless).
- **Thiết bị mô phỏng:** 
  - **Mobile:** `390×844` (iPhone 14/15/16 viewport chuẩn, `isMobile: true`, `hasTouch: true`).
  - **Tablet:** `768×1024` (iPad Mini / 9.7" Portrait viewport, `isMobile: true`, `hasTouch: true`).
- **Ứng dụng được kiểm thử:** `http://127.0.0.1:3000` (Next.js 15) và `http://127.0.0.1:5080` (.NET 10 API).
- **Tài khoản kiểm thử độc lập:** `qa+newbie-core-1795031987851@test.local`, `qa+newbie-mobile-1795031724924@test.local`, `qa+newbie-tablet-1795031741824@test.local` (tách biệt hoàn toàn, không đụng chạm dữ liệu của Sơn Trần và Minh Anh).

---

## 1. Kết Quả Chính & Mức Độ Thân Thiện Cho Người Mới Trên Mobile/Tablet

### Đánh giá tổng quan (Điểm trải nghiệm: 6.8 / 10)
Studxy đã xây dựng được một nền tảng Mobile Web tiến bộ với thanh điều hướng đáy `MobileNav` gắn cố định (`h-14` với 5 mục và nút Tìm kiếm toàn cục), hỗ trợ chuyển đổi giao diện mượt mà và chế độ đọc/ôn tập tập trung (`isImmersive` tự động ẩn thanh điều hướng để tối ưu không gian).

Tuy nhiên, đối với một **người dùng mới lần đầu tiếp cận bằng điện thoại**:
1. **Rào cản Onboarding & Điều hướng:** Nút CTA trên Landing Page dẫn sang `/workspace` (Workspace Classic) thay vì giao diện `/app` hiện đại. Sau khi đăng ký tài khoản thành công, người dùng bị chuyển hướng thẳng vào một không gian desktop dày đặc, gây sốc giao diện.
2. **Kích thước Tap Target (Nút bấm cảm ứng):** Hơn 50 nút bấm và chip lọc trên trang Khóa học (`/app/courses`), trang Ôn tập (`/app/review`), trang Đăng nhập (`/auth`) có kích thước dưới chuẩn công thái học di động (`< 44×44px`, nhiều nút chỉ `14-28px`), dễ dẫn tới chạm hụt hoặc bấm nhầm bằng ngón tay cái.
3. **Mục lục bài học (TOC) bị ẩn trên màn hình nhỏ:** Nút mở mục lục trên trang học (`/app/courses/[id]/learn`) bị ẩn hoàn toàn trên Mobile và Tablet (`hidden lg:inline-flex`), khiến người học không thể xem cấu trúc bài giảng để nhảy đến phần cần học.
4. **Thuật ngữ kỹ thuật gây bối rối:** Giao diện ngập tràn các khái niệm chuyên ngành như *"FSRS v5"*, *"Split PKM"*, *"Cloze"*, *"Workspace Classic"*, *"OCC"*, thiếu phần giải thích hoặc chú thích thân thiện cho người mới bắt đầu.
5. **Trải nghiệm một tay (One-Handed Ergonomics):** Chế độ ôn tập FSRS có 4 nút đánh giá to rõ (`64px` chiều cao), tuy nhiên nút *"Quên"* nằm ở tít mép trái ngoài cùng (tọa độ x: 16px), buộc người thuận tay phải phải rướn ngón tay cái tối đa để bấm.

---

## 2. Bằng Chứng Thực Thi

### 2.1. Lệnh thực thi & Mã thoát (Exit Codes)

| Script kiểm thử | Mục đích | Lệnh chạy | Exit Code | Nhật ký / Kết quả |
|---|---|---|---|---|
| `01-onboarding-journey.js` | Đo luồng đăng ký mới, landing page, khảo sát EmptyState trên Mobile | `node docs/qa-round2/scripts/a-newbie-mobile/01-onboarding-journey.js` | `0` | [onboarding-results.json](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-newbie-mobile/onboarding-results.json) |
| `02-tap-target-and-layout-audit.js` | Kiểm toán tự động toàn bộ Tap Targets, Spacing, Tràn ngang, Che khuất Nav trên Mobile & Tablet | `node docs/qa-round2/scripts/a-newbie-mobile/02-tap-target-and-layout-audit.js` | `0` | [tap-target-audit-results.json](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-newbie-mobile/tap-target-audit-results.json) |
| `03-core-learning-experience.js` | Tạo khóa học mẫu, học bài, mở Study Drawer ghi chú, chế độ Song song, lật thẻ FSRS, làm quiz | `node docs/qa-round2/scripts/a-newbie-mobile/03-core-learning-experience.js` | `0` | [core-learning-results.json](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-newbie-mobile/core-learning-results.json) |
| `find-pdf-course.js` | Kiểm tra chế độ `Song song` (Split) giữa Bài giảng và Tài liệu PDF trên Mobile & Tablet | `node docs/qa-round2/scripts/a-newbie-mobile/find-pdf-course.js` | `0` | Chụp `learn-390-tab-split.png`, `learn-768-tab-split.png` |
| `test-quiz-flow.js` | Thao tác chọn đáp án trắc nghiệm trên màn `390×844` và nộp bài kiểm tra | `node docs/qa-round2/scripts/a-newbie-mobile/test-quiz-flow.js` | `0` | Chụp `quiz-390-choice-selected.png`, `quiz-390-submitted-result.png` |

---

### 2.2. Bảng Thống Kê Tap Targets (< 44px) & Bố Cục (Layout Audit)

Kết quả đo bằng `getBoundingClientRect()` quét tự động trên toàn bộ các phần tử tương tác (`button`, `a[href]`, `input`, `select`, `[role="button"]`, `[role="tab"]`):

| Viewport | Tuyến đường (Route) | Tổng tương tác | Tap Target < 44×44px | < 36×36px (Rất nhỏ) | Tràn ngang màn hình? | Bị MobileNav che khuất? |
|---|---|:---:|:---:|:---:|:---:|:---:|
| **Mobile `390×844`** | `/` (Landing Page) | 10 | 5 | 5 | Không (`390px`) | Không |
| **Mobile `390×844`** | `/auth` (Đăng nhập/Đăng ký) | 10 | 4 | 1 | Không (`390px`) | Không |
| **Mobile `390×844`** | `/app` (Hôm nay / Dashboard) | 22 | 8 | 3 | Không (`390px`) | Không |
| **Mobile `390×844`** | `/app/courses` (Danh mục khóa học) | 65 | **59** | **55** | Không (`390px`) | Không |
| **Mobile `390×844`** | `/app/courses/new` (Tạo khóa học) | 13 | 7 | 2 | Không (`390px`) | Không |
| **Mobile `390×844`** | `/app/review` (Ôn tập FSRS) | 5 | **5** | **4** | Không (`390px`) | Không |
| **Mobile `390×844`** | `/app/notes` (Sổ tay ghi chép) | 9 | 3 | 0 | Không (`390px`) | Không |
| **Mobile `390×844`** | `/app/documents` (Thư viện tài liệu) | 9 | 3 | 0 | Không (`390px`) | Không |
| **Mobile `390×844`** | `/workspace` (Workspace Classic) | 124 | 37 | 0 | Không (`390px`) | Không |
| **Tablet `768×1024`** | `/` (Landing Page) | 10 | 7 | 7 | Không (`768px`) | Không |
| **Tablet `768×1024`** | `/auth` (Đăng nhập/Đăng ký) | 11 | 9 | 4 | Không (`768px`) | Không |
| **Tablet `768×1024`** | `/app` (Dashboard) | 34 | 26 | 19 | Không (`768px`) | Không |
| **Tablet `768×1024`** | `/app/courses` (Khóa học) | 76 | 76 | 54 | Không (`768px`) | Không |
| **Tablet `768×1024`** | `/app/courses/new` (Tạo khóa học) | 24 | 24 | 17 | Không (`768px`) | Không |
| **Tablet `768×1024`** | `/app/review` (Ôn tập FSRS) | 5 | 5 | 4 | Không (`768px`) | Không |
| **Tablet `768×1024`** | `/app/notes` (Sổ tay ghi chép) | 20 | 20 | 16 | Không (`768px`) | Không |
| **Tablet `768×1024`** | `/app/documents` (Tài liệu) | 20 | 20 | 16 | Không (`768px`) | Không |
| **Tablet `768×1024`** | `/workspace` (Workspace Classic) | 127 | 41 | 0 | Không (`768px`) | Không |

---

### 2.3. Danh Mục Ảnh Chụp Thực Tế (Screenshots)

Toàn bộ 49 ảnh chụp màn hình được lưu trữ tại `docs/qa-round2/screenshots/a-newbie-mobile/`:

- **Hành trình Người Mới (Onboarding):**
  - [01-landing-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/01-landing-390.png): Trang chủ `/` trên điện thoại 390px.
  - [02-auth-page-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/02-auth-page-390.png): Trang xác thực `/auth` hiển thị form đăng nhập và demo switcher.
  - [03-auth-register-form-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/03-auth-register-form-390.png): Form đăng ký tài khoản mới trên 390px.
  - [04-after-register-destination-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/04-after-register-destination-390.png): Giao diện đầu tiên người dùng nhìn thấy sau khi bấm đăng ký.
- **Trạng thái Trống (EmptyState) Tài khoản mới:**
  - [05-dashboard-empty-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/05-dashboard-empty-390.png): Dashboard `/app` khi chưa có khóa học/tiến độ.
  - [06-courses-empty-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/06-courses-empty-390.png): Danh mục `/app/courses` với các bộ lọc trạng thái và lĩnh vực.
  - [07-review-empty-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/07-review-empty-390.png): Màn hình `/app/review` khi không có thẻ đến hạn.
  - [08-notes-empty-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/08-notes-empty-390.png): Màn hình `/app/notes` khi chưa có sổ tay ghi chép.
  - [09-documents-empty-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/09-documents-empty-390.png): Màn hình `/app/documents` khi chưa tải lên tài liệu PDF nào.
- **So sánh Tài khoản Demo "Minh Anh" (Không gian mới):**
  - [10-minhanh-dashboard-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/10-minhanh-dashboard-390.png), [11-minhanh-courses-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/11-minhanh-courses-390.png), [12-minhanh-review-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/12-minhanh-review-390.png), [13-minhanh-notes-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/13-minhanh-notes-390.png), [14-minhanh-documents-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/14-minhanh-documents-390.png).
- **Học tập & Thao tác Một tay:**
  - [learn-390-initial.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/learn-390-initial.png): Không gian học bài giảng trên 390px (chú ý thiếu nút Mục lục).
  - [learn-390-drawer-opened.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/learn-390-drawer-opened.png): Bottom-sheet "Sổ tay & Thẻ" mở che toàn màn hình trên điện thoại.
  - [learn-390-drawer-note-typed.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/learn-390-drawer-note-typed.png): Gõ ghi chú vào Sổ tay trên điện thoại.
  - [learn-390-tab-practice.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/learn-390-tab-practice.png): Tab Phòng thực hành trên điện thoại.
  - [learn-390-tab-split.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/learn-390-tab-split.png): Chế độ `Song song` trên điện thoại (xếp chồng dọc Bài giảng + PDF).
  - [learn-768-tab-split.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/learn-768-tab-split.png): Chế độ `Song song` trên máy tính bảng 768px.
  - [review-390-card-front.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/review-390-card-front.png): Mặt trước thẻ FSRS trên điện thoại.
  - [review-390-card-flipped.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/review-390-card-flipped.png): Mặt sau thẻ FSRS với 4 nút đánh giá `Quên / Khó / Tốt / Dễ`.
  - [review-390-after-rate.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/review-390-after-rate.png): Kết quả sau khi bấm đánh giá "Tốt".
  - [quiz-390-overview.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/quiz-390-overview.png): Danh sách câu hỏi kiểm tra trên điện thoại.
  - [quiz-390-choice-selected.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/quiz-390-choice-selected.png): Chọn đáp án trắc nghiệm bằng radio button.
  - [quiz-390-submitted-result.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/quiz-390-submitted-result.png): Kết quả chấm điểm ngay sau khi nộp bài.
- **Tìm kiếm Toàn cục & Workspace Classic:**
  - [search-palette-open-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/search-palette-open-390.png): Command Palette mở từ nút Tìm kiếm trên `MobileNav`.
  - [search-palette-typed-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/search-palette-typed-390.png): Kết quả tìm kiếm từ khóa trên điện thoại.
  - [workspace-classic-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/workspace-classic-390.png): Giao diện `/workspace` trên điện thoại 390px (dày đặc, không có nút quay lại).
  - [workspace-classic-avatar-dropdown-390.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/workspace-classic-avatar-dropdown-390.png): Nút quay lại `/app` bị giấu sâu trong menu avatar.
  - [workspace-classic-768.png](file:///d:/03_Study/Studxy/docs/qa-round2/screenshots/a-newbie-mobile/workspace-classic-768.png): Giao diện `/workspace` trên máy tính bảng 768px.

---

## 3. Danh Sách Vấn Đề (Issues Catalog: `MOB-01` -> `MOB-08`)

### `MOB-01`: Nút CTA Trang Chủ Chuyển Hướng Người Dùng Mới Vào Workspace Classic Thay Vì `/app`
- **Mức độ:** **Critical**
- **Viewport xảy ra:** `390×844` & `768×1024`
- **Bước tái hiện:**
  1. Mở trình duyệt điện thoại truy cập `http://127.0.0.1:3000/`.
  2. Bấm nút CTA chính *"Trải nghiệm ngay"* (`data-testid="landing-hero-cta"`).
  3. URL dẫn sang `/workspace` -> do chưa đăng nhập, tự redirect sang `/auth?redirect=%2Fworkspace`.
  4. Đăng ký tài khoản mới thành công.
- **Mong đợi:** Người dùng di động mới phải được chuyển hướng vào Dashboard hiện đại `/app` với thanh điều hướng đáy di động.
- **Thực tế:** Sau khi đăng ký, Next.js chuyển hướng thẳng vào `/workspace` (`targetDestination = searchParams.get('redirect')`). Người dùng bị đưa vào giao diện desktop cổ điển phức tạp, khó thao tác bằng màn hình chạm.
- **Tập tin / Dòng nghi ngờ:**
  - [HeroSection.tsx#L100-L105](file:///d:/03_Study/Studxy/frontend/src/components/landing/HeroSection.tsx#L100-L105) (`href="/workspace"`)
  - [page.tsx (auth)#L62-L67](file:///d:/03_Study/Studxy/frontend/src/app/auth/page.tsx#L62-L67) (`resolveSafeRedirect(searchParams.get("redirect"))`)

---

### `MOB-02`: Nút Mở Mục Lục (TOC) Bài Học Bị Ẩn Hoàn Toàn Trên Mobile & Tablet
- **Mức độ:** **High**
- **Viewport xảy ra:** `390×844` (Mobile) và `768×1024` (Tablet)
- **Bước tái hiện:**
  1. Vào một khóa học bất kỳ, ví dụ `/app/courses/[id]/learn`.
  2. Quan sát thanh công cụ đầu trang (Header).
- **Mong đợi:** Có nút bấm mở danh sách các đề mục (TOC / Outline) của bài học để người dùng điện thoại cuộn nhanh đến các phần nội dung.
- **Thực tế:** Nút `Mục lục` bị gán class `hidden lg:inline-flex`. Trên màn hình nhỏ hơn 1024px (cả điện thoại 390px lẫn tablet 768px), nút biến mất hoàn toàn (`isTocVisible390 = false`, `isTocVisible768 = false`). Người học bài dài buộc phải vuốt tay liên tục qua hàng chục màn hình.
- **Tập tin / Dòng nghi ngờ:**
  - [CourseLearningWorkspace.tsx#L1663-L1676](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx#L1663-L1676)

---

### `MOB-03`: Hơn 50 Nút Bấm & Bộ Lọc Vi Phạm Kích Thước Tap Target Chuẩn (< 44×44px)
- **Mức độ:** **High**
- **Viewport xảy ra:** `390×844` và `768×1024`
- **Bước tái hiện & Số liệu đo đạc:**
  1. Trên `/app/courses`: Có **55 nút bấm nhỏ hơn 36×36px**! Các chip lọc trạng thái (`Tất cả trạng thái`, `Đang học`, `Chưa học`, `Hoàn thành`) chỉ cao **28px**; các chip lĩnh vực chỉ cao **32px** và đặt sát nhau cách nhau dưới 4px.
  2. Trên `/app/review`: Nút `Quay lại` (mũi tên góc trái) chỉ có kích thước **16×16px**; nút `Hoàn tác` (`review-undo-btn`) chỉ có kích thước **14×14px**! Dropdown chọn khóa học và nút lọc thẻ chỉ cao **28px**.
  3. Trên `/auth`: Nút con mắt bật/tắt hiển thị mật khẩu (`auth-toggle-password-btn`) chỉ có kích thước **24×24px**.
  4. Trên `/app/courses/[id]/learn`: Nút bật ngăn Study Drawer chỉ cao **32px** (`h-8`).
- **Mong đợi:** Mọi nút tương tác bằng ngón tay phải đạt tối thiểu `44×44px` (theo chuẩn Apple HIG và Google Material Design) hoặc ít nhất `36×36px` cho các thanh công cụ phụ kèm đệm padding.
- **Thực tế:** Các nút quá nhỏ gây hiện tượng chạm hụt (miss-tap) hoặc bấm nhầm sang phần tử kế bên.
- **Tập tin / Dòng nghi ngờ:**
  - [page.tsx (courses)#L250-L335](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx#L250-L335)
  - [page.tsx (review)#L620-L640](file:///d:/03_Study/Studxy/frontend/src/app/app/review/page.tsx#L620-L640)
  - [page.tsx (auth)#L560-L580](file:///d:/03_Study/Studxy/frontend/src/app/auth/page.tsx#L560-L580)
  - [CourseLearningWorkspace.tsx#L1682](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx#L1682)

---

### `MOB-04`: Thông Điệp Trạng Thái Trống (EmptyState) Đánh Lừa Người Dùng Mới
- **Mức độ:** **Medium**
- **Viewport xảy ra:** `390×844` & `768×1024`
- **Bước tái hiện:**
  1. Đăng ký tài khoản mới tinh.
  2. Truy cập `/app/courses`.
- **Mong đợi:** Hiển thị thông điệp chào đón và hướng dẫn bước đầu: *"Bạn chưa có khóa học nào. Hãy bắt đầu bằng cách tạo khóa học mới từ tài liệu hoặc khám phá các khóa học gợi ý"*.
- **Thực tế:** Giao diện hiển thị: *"Không tìm thấy khóa học phù hợp. Thử đổi từ khóa tìm kiếm, chọn lại lĩnh vực hoặc tạo khóa học mới"*. Người mới lần đầu đăng nhập tưởng rằng mình gõ sai từ khóa tìm kiếm hoặc ứng dụng bị lỗi dữ liệu.
- **Tập tin / Dòng nghi ngờ:**
  - [page.tsx (courses)#L401-L415](file:///d:/03_Study/Studxy/frontend/src/app/app/courses/page.tsx#L401-L415)

---

### `MOB-05`: Không Có Lối Thoát (Escape Hatch) Trực Quan Từ Workspace Classic Về `/app` Trên Điện Thoại
- **Mức độ:** **Medium**
- **Viewport xảy ra:** `390×844`
- **Bước tái hiện:**
  1. Đang dùng ứng dụng trên điện thoại, bấm vào link *"Workspace Classic"* ở chân trang hoặc trong menu.
  2. Màn hình chuyển vào `http://127.0.0.1:3000/workspace`.
  3. Tìm nút để quay trở lại giao diện chính `/app`.
- **Mong đợi:** Có banner hoặc nút bấm cố định nổi bật *"Về giao diện chính (/app)"*.
- **Thực tế:** Không có bất kỳ nút quay lại nào trên thanh tiêu đề `TopNavHeader`. Đường dẫn duy nhất `switch-to-app-v2-link` bị giấu sâu bên trong menu avatar cá nhân. Trên màn hình 390px, thanh tiêu đề bị dồn ép với hơn 10 nút nhỏ, khiến người dùng không biết làm sao để thoát ra ngoài nếu không ấn vào ảnh đại diện.
- **Tập tin / Dòng nghi ngờ:**
  - [TopNavHeader.tsx#L480-L488](file:///d:/03_Study/Studxy/frontend/src/components/layout/TopNavHeader.tsx#L480-L488)
  - [page.tsx (workspace)#L80-L100](file:///d:/03_Study/Studxy/frontend/src/app/workspace/page.tsx#L80-L100)

---

### `MOB-06`: Chế Độ "Song Song" (Split Mode) Gây Chật Chội & Xếp Chồng Khó Cuộn Trên Màn 390px
- **Mức độ:** **Medium**
- **Viewport xảy ra:** `390×844` (Mobile)
- **Bước tái hiện:**
  1. Mở bài học có tài liệu PDF đính kèm (ví dụ: khóa học OSTEP).
  2. Chuyển sang tab `Song song` (`data-testid="center-tab-split"`).
- **Mong đợi:** Trải nghiệm đọc song song trên điện thoại được điều chỉnh phù hợp (ví dụ: chế độ lật trang theo cử chỉ swipe hoặc chuyển đổi nhanh giữa 2 pane bằng tab nổi).
- **Thực tế:** Giao diện chia dọc (`flex-col divide-y`), mỗi phần chiếm chưa tới 320px chiều cao. Màn hình bị chia làm 2 thanh cuộn lồng nhau (cuộn bài giảng và cuộn PDF), các nút phóng to/thu nhỏ PDF chen chúc, gây ức chế thị giác nghiêm trọng trên màn hình điện thoại.
- **Tập tin / Dòng nghi ngờ:**
  - [CourseLearningWorkspace.tsx#L1980-L1984](file:///d:/03_Study/Studxy/frontend/src/components/learn/CourseLearningWorkspace.tsx#L1980-L1984) (`centerTab === "split" ? "flex-col lg:flex-row divide-y" : ""`)

---

### `MOB-07`: Bố Trí 4 Nút Đánh Giá Ôn Tập FSRS Chưa Tối Ưu Cho Thao Tác Ngón Cái Một Tay
- **Mức độ:** **Low**
- **Viewport xảy ra:** `390×844`
- **Bước tái hiện:**
  1. Vào `/app/review`, lật thẻ ôn tập.
  2. Quan sát hàng 4 nút: `Quên (1)`, `Khó (2)`, `Tốt (3)`, `Dễ (4)`.
- **Mong đợi:** Nút có tần suất sử dụng cao hoặc quan trọng (như `Quên` khi không nhớ bài) phải nằm trong vùng chạm tự nhiên của ngón cái (Thumb Zone).
- **Thực tế:** 4 nút dàn ngang thành một hàng dài ở đáy:
  - Nút `Quên`: nằm ở góc tột cùng bên trái (`x: 16px`). Khi cầm điện thoại bằng tay phải (90% người dùng), ngón cái phải vươn hết cỡ qua ngang màn hình 390px.
  - Vị trí cách đáy màn hình 161px, chưa áp sát cạnh đáy tự nhiên.
- **Tập tin / Dòng nghi ngờ:**
  - [page.tsx (review)#L880-L895](file:///d:/03_Study/Studxy/frontend/src/app/app/review/page.tsx#L880-L895)

---

### `MOB-08`: Các Thuật Ngữ Kỹ Thuật (Jargon) Không Được Chú Thích Cho Người Mới
- **Mức độ:** **Low**
- **Viewport xảy ra:** Tất cả các màn hình
- **Thực tế quan sát:**
  - `FSRS v5`: Xuất hiện trên trang chủ, màn hình đăng nhập và tiêu đề ôn tập mà không giải thích đây là thuật ngữ gì (Lặp lại ngắt quãng để chống quên).
  - `Split PKM`: Khái niệm "Quản trị tri thức cá nhân song song" gây khó hiểu cho học sinh, sinh viên không chuyên công nghệ.
  - `Workspace Classic`: Người dùng mới không hiểu "Classic" là gì so với giao diện thông thường.
  - `Cloze`: Thuật ngữ thẻ điền khuyết xuất hiện trong thông báo hạn mức.

---

## 4. Góp Ý Dưới Góc Nhìn Người Dùng Mới Dùng Điện Thoại

### 4.1. Những chỗ gây bối rối nhất khi mới tạo tài khoản
1. **Thiếu một "Welcome Tour" hoặc Checklist 3 bước rõ ràng:** Sau khi tạo tài khoản, Dashboard hiện lên với các khối trống rỗng. Người mới không biết nên: (1) Tạo khóa học trước, hay (2) Tải PDF lên trước, hay (3) Tạo thẻ ghi nhớ. Cần có một thẻ checklist nhỏ trên cùng:
   - `[ ] Bước 1: Tạo khóa học đầu tiên hoặc chọn từ thư viện`
   - `[ ] Bước 2: Đọc bài học đầu tiên & lưu 1 ghi chú`
   - `[ ] Bước 3: Hoàn thành 1 phiên ôn tập thẻ nhớ (FSRS)`
2. **Nút "Kiểm tra" bị ẩn trong bài học:** Trên điện thoại, nút dẫn sang bài Quiz kiểm tra kiến thức trong header bị ẩn (`hidden sm:inline-flex`), người học không biết làm sao để kiểm tra bài sau khi đọc xong nếu không quay ra ngoài danh sách khóa học.

---

### 4.2. Đề xuất luồng Onboarding & Chuyển đổi ngôn ngữ thân thiện
- Thay vì dùng từ ngữ học thuật:
  - *"Thuật toán FSRS v5"* -> đổi thành *"Khoa học ghi nhớ chống quên (Nhắc bài thông minh)"*.
  - *"Split PKM"* -> đổi thành *"Vừa đọc tài liệu, vừa ghi chép sổ tay"*.
  - *"Thẻ Cloze"* -> đổi thành *"Thẻ điền vào chỗ trống"*.
  - *"Workspace Classic"* -> đổi thành *"Giao diện Nâng cao cho Máy tính"*.
- Thêm tooltip giải thích khi chạm nhẹ (tap) vào các huy hiệu trên di động.

---

### 4.3. Góp ý thiết kế công thái học Một Tay (One-Handed Ergonomics) cho tính năng học Tiếng Anh sắp tới
Đối với khóa học Tiếng Anh trên điện thoại (nghe mẫu, thu âm phát âm, chép chính tả), người dùng thường cầm điện thoại bằng 1 tay khi đi xe bus, nằm nghỉ hoặc đi bộ:

1. **Vùng tác vụ ngón cái (Thumb-Zone Floating Action Bar):**
   - Đặt một thanh điều khiển nổi (Floating Bar) ở đáy màn hình, cách cạnh dưới `16px` (ngay trên viền an toàn safe-area).
2. **Nút Nghe & Giữ để Nói (Push-to-Talk Recording):**
   - Nút thu âm phát âm cần có đường kính tối thiểu **`64×64px`**, đặt lệch nhẹ sang góc phải (hoặc giữa màn hình) để ngón cái chạm tới tự nhiên không cần đổi thế cầm máy.
   - Thao tác: Chạm để nghe người bản xứ đọc mẫu; **Nhấn và giữ (Long-press & Hold)** để thu âm giọng đọc của học viên, thả tay ra là tự động gửi AI chấm điểm ngữ âm.
3. **Thao tác Chép chính tả (Dictation Mode) trên điện thoại:**
   - Không bắt người dùng gõ cả câu dài bằng bàn phím ảo (dễ gây nản và gõ sai chính tả trên phím nhỏ).
   - Thiết kế dạng **Word-bank Chips**: Các từ gợi ý được xếp thành các khối chip có chiều cao `40-44px` ở nửa dưới màn hình; người học chỉ cần chạm ngón cái vào từng từ theo thứ tự nghe được để ghép thành câu hoàn chỉnh.

---

## 5. Mục Chưa Kiểm Chứng (Unverified Scope)

1. **Cử chỉ cảm ứng thực tế (Touch Gestures) trên phần cứng iOS/Android thật:** Playwright mô phỏng `hasTouch: true` và kích thước viewport chính xác, nhưng chưa thể kiểm thử độ trễ phản hồi cảm ứng khi vuốt đa điểm (pinch-to-zoom) trên file PDF bản vẽ lớn thực tế trên phần cứng iPhone/iPad thật.
2. **Hành vi khi bàn phím ảo (Virtual Keyboard) bật lên trên iOS Safari:** Khi người dùng chạm vào ô nhập ghi chú hoặc ô tìm kiếm, bàn phím ảo bật lên làm giảm chiều cao viewport từ `844px` xuống còn khoảng `450-500px`. Cần kiểm chứng trực tiếp trên thiết bị thực tế xem bottom-sheet của Sổ tay có bị bàn phím che khuất nút lưu hay không.
3. **Hiệu năng cuộn file PDF dung lượng lớn (> 100 trang):** Khóa học thử nghiệm dùng tài liệu ngắn; việc hiển thị canvas PDF hơn 100 trang ở chế độ `Song song` trên thiết bị di động cấu hình thấp có thể gây giật lag bộ nhớ (Memory Pressure) chưa được đo lường bằng Profiler phần cứng di động.
