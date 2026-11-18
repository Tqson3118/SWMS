# 📋 BÁO CÁO TỔNG HỢP KIỂM THỬ TOÀN DIỆN — STUDXY (SWMS)
## 4 Agent Browser Test · 18/11/2026

> [!NOTE]
> Báo cáo tổng hợp từ 4 agent browser test song song, mỗi agent đóng một vai trò khác nhau.
> Tất cả đều đã được host xác nhận truy cập thực tế `http://127.0.0.1:3000`.

---

## 📊 TỔNG QUAN

| Agent | Vai trò | Điểm/Đánh giá | ✅ Pass | ⚠️ Warning | ❌ Bug |
|-------|---------|---------------|---------|-----------|-------|
| 🧑‍💻 | Normal User (Sinh viên) | Rất hài lòng | 10 | 3 | 0 |
| 🔓 | Security QA Reviewer | Nền tảng vững | 10 | 2 | 1 |
| 🐛 | QA Bug Hunter | Ổn định tốt | 18 | 2 | 4 |
| 💡 | UX Designer (7.8/10) | Chuyên nghiệp | 3 | 5 | 4 |
| **Tổng** | | | **41** | **12** | **9** |

---

## 🔴 BUGS CẦN FIX (Xếp theo Severity)

### Priority 1 — Critical / High

| # | Bug | Severity | Nguồn | Hành động |
|---|-----|----------|-------|-----------|
| B1 | **JWT Token lưu `localStorage`** — rủi ro bị đánh cắp qua XSS, cần chuyển sang HttpOnly Cookie | Critical | Security QA | Cần thiết kế lại auth flow |
| B2 | **Layout Collapse Lesson Player** — khi bật drawer "Sổ tay & Thẻ", cột bài học co còn ~100-150px, không đọc được | High | UX + Normal User | Fix CSS grid/flex |
| B3 | **Thiếu HTTP Security Headers** — CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy | High | Security QA | Thêm middleware headers |
| B4 | **CORS wildcard `*`** — Backend API cho phép mọi origin | High | Security QA | Whitelist domain cụ thể |

### Priority 2 — Medium

| # | Bug | Severity | Nguồn | Hành động |
|---|-----|----------|-------|-----------|
| B5 | **Duplicate H1 Headings** — tiêu đề bài học render lặp 3 lần | Medium | UX | Fix render logic |
| B6 | **Lặp prefix chương AI** — "Chương 1 : Chương 1: ..." | Medium | QA Bug Hunter | Fix template/prompt |
| B7 | **Flashcard sync error hiện raw JSON** — `{"error":"TOKEN_VERSION_REVOKED"}` | Medium | Normal User | Thay bằng thông báo thân thiện |
| B8 | **Header buttons vỡ dòng trên Tablet 768px** — nút "+ Tạo khóa học" bẻ text 3 dòng | Medium | QA + UX | CSS whitespace-nowrap + responsive |
| B9 | **PDF 404 & Markdown thô** — hiển thị raw markdown thay vì rendered | Medium | UX | Fix markdown renderer |

### Priority 3 — Low

| # | Bug | Severity | Nguồn | Hành động |
|---|-----|----------|-------|-----------|
| B10 | **"Chào Trần ,"** — lấy họ thay tên, thừa space trước phẩy | Low | UX | Fix greeting logic |
| B11 | **Metadata separator lỗi** — `· • ·` | Low | UX | Fix template |
| B12 | **Thiếu `minlength="6"` trên password input** — dùng alert() thay inline error | Low | QA Bug Hunter | Thêm HTML5 attr + inline error |
| B13 | **File upload thiếu `accept=".pdf"`** — cho chọn mọi loại file | Low | QA Bug Hunter | Thêm accept attribute |
| B14 | **Google login hiện "Chưa bật Client ID"** — text kỹ thuật | Low | UX | Ẩn/disable khi chưa config |
| B15 | **Quiz rỗng vẫn hiện nút** — bấm vào báo 0 câu hỏi | Low | UX | Disable/ẩn nút khi empty |
| B16 | **Phân mảnh thương hiệu Studxy vs SWMS** — title, favicon inconsistent | Low | UX | Thống nhất branding |

---

## ⭐ ĐIỂM NỔI BẬT ĐÁNG KHEN

1. **FSRS v5 Spaced Repetition** — thiết kế ngang tầm Anki, phím tắt 1-4/Space chuyên nghiệp
2. **Command Palette (Ctrl+K)** — tìm kiếm toàn cục, tạo thẻ/ghi chú tức thì
3. **One-Click Demo Auth** — 2 profile demo cho trải nghiệm ngay
4. **Visual Aesthetics** — Bento dark/light mode rất đẹp, phối màu tím gradient cao cấp
5. **XSS Prevention** — React auto-escaping hoạt động tốt cả reflected + stored
6. **Route Guard** — 100% redirect về /auth khi chưa đăng nhập
7. **Double-click debounce** — nút disable ngay, chỉ 1 request
8. **AI Course Generation** — tạo khóa học hoàn chỉnh trong 2 giây
9. **Offline Queue** — flashcard review lưu offline khi mất kết nối

---

## 💡 ĐỀ XUẤT TÍNH NĂNG MỚI (Từ cả 4 agent)

| # | Đề xuất | Nguồn |
|---|---------|-------|
| F1 | Interactive Onboarding Checklist cho newbie (3 bước) | UX |
| F2 | AI Study Assistant trong Lesson Player (hỏi đáp AI) | UX |
| F3 | Pomodoro Timer tích hợp | UX |
| F4 | Audio Reader / Text-to-Speech tiếng Việt | UX |
| F5 | Thêm nút "Tạo bài học mới" thủ công trong đề cương | Normal User |
| F6 | Phím tắt Alt+Right Arrow chuyển bài tiếp | Normal User |
| F7 | Auto-collapse sidebar khi mở Split PKM trên < 1366px | Normal User |
| F8 | Autosave Draft / cảnh báo Unsaved Changes khi F5 | QA Bug Hunter |
| F9 | Drag & Drop Dropzone cho upload tài liệu | QA Bug Hunter |
| F10 | Skeleton Loading thay spinner | QA Bug Hunter |

---

## 🎯 KẾ HOẠCH FIX (AGY CLI)

### Đợt 1: Bugs Medium-High ảnh hưởng UX trực tiếp
- B2: Layout Collapse Lesson Player
- B5: Duplicate H1 Headings
- B6: Lặp prefix chương AI
- B7: Flashcard sync error raw JSON
- B8: Header buttons vỡ dòng tablet
- B10: Greeting "Chào Trần ,"
- B12: Password minlength + inline error
- B13: File upload accept
- B14: Google login text kỹ thuật
- B15: Quiz empty state

### Đợt 2: Security hardening (cần thiết kế cẩn thận)
- B1: JWT HttpOnly Cookie (cần refactor auth flow)
- B3: HTTP Security Headers
- B4: CORS whitelist

### Đợt 3: Content & Branding
- B9: Markdown renderer
- B11: Metadata separator
- B16: Branding unification
