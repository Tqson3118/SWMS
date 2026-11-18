Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ C1-REDESIGN: Làm lại TOÀN BỘ design system + mọi trang theo "Clean & Calm Learning" + sửa hiệu năng frontend

Đầu vào bắt buộc phải đọc trước:
- `docs/qa-round2/C0-ux-audit.md` (đặc biệt mục 3, 5, 6: token màu/typography/spacing/radius/shadow/motion, 10 component nền, thứ tự trang).
- `docs/qa-round2/C0-perf-baseline.md` (mục 5: top 10 nguyên nhân chậm).
- Ảnh "trước": `docs/qa-round2/screenshots/c0-before/`.

Bạn được phép SỬA CODE trong `frontend/` (KHÔNG sửa `backend/` ở nhiệm vụ này). Làm hết toàn bộ trang trong một phiên, theo đúng thứ tự dưới đây, commit sau mỗi giai đoạn.

## Quyết định đã chốt (không cần hỏi lại)
- Phong cách: "Clean & Calm Learning" — light-first, dark mode đầy đủ; dùng bảng màu ở mục 6.1 của C0-ux-audit (accent `#5E51E8` light / `#818CF8` dark, streak amber, success emerald). Mọi text đạt WCAG AA.
- Font: ưu tiên **Be Vietnam Pro** (thiết kế cho dấu tiếng Việt) qua `next/font/google` với subset `vietnamese`; nếu build không tải được font (offline) thì dùng font stack hệ thống, ghi rõ trong báo cáo.
- Thương hiệu: thống nhất duy nhất **"Studxy"** (title, header, metadata, favicon text, mọi chỗ "SWMS" hiển thị cho người dùng). Không đổi tên namespace/biến trong code backend.
- Một hệ token DUY NHẤT trong `globals.css` (CSS variables + map vào Tailwind). Gỡ dần hệ token V1/V2 song song: các class cũ phải được chuyển sang token mới; nếu còn chỗ chưa chuyển kịp thì để alias trỏ về token mới (không để 2 bảng màu lệch nhau).
- Xây 10 component nền trong `frontend/src/components/ui/` (Button, Card, Badge, Input, Dialog, Tabs, StatCard, ProgressBar, EmptyState, Tooltip) và DÙNG chúng ở các trang.
- Motion: duration 150/220/320ms, easing spring/smooth như mục 6.5; bắt buộc `prefers-reduced-motion`.
- Lời chào: không dùng `.pop()`. Quy tắc: nếu token CUỐI của họ tên là họ Việt phổ biến (Nguyễn, Trần, Lê, Phạm, Hoàng, Huỳnh, Phan, Vũ, Võ, Đặng, Bùi, Đỗ, Hồ, Ngô, Dương, Lý...) thì lấy token ĐẦU (vd "Sơn Trần" → "Sơn"); ngược lại lấy token cuối (vd "Nguyễn Minh Anh" → "Anh"). Không có tên → "bạn". Không có khoảng trắng thừa trước dấu phẩy.
- Quá tải ôn tập: KHÔNG đổi thuật toán FSRS/backend. Ở frontend thêm **giới hạn phiên mỗi ngày** (mặc định 100 thẻ ôn + 20 thẻ mới, chỉnh được trong `/app/settings`, lưu localStorage). Dashboard hiển thị "Hôm nay: X thẻ (~Y phút)" theo giới hạn, kèm dòng phụ "còn N thẻ tồn". Hết giới hạn thì màn hoàn thành có nút "Ôn thêm 20 thẻ".
- Lịch sử kiểm tra trên dashboard: chỉ hiển thị bài đã nộp; không có thì EmptyState có CTA.
- Phân loại khóa học (`frontend/src/lib/course-helpers.ts`): sửa regex có ranh giới từ (không khớp "ai" trong "giai", "bài"...), thêm nhóm "Ngoại ngữ" (english, ielts, toeic, tiếng anh, ngữ pháp, từ vựng, phát âm...). Viết unit test hoặc script kiểm chứng cho ít nhất 10 tên khóa thật trong DB demo.
- Markdown bài học (`LessonMarkdownView.tsx`): sửa lỗi bold bị vỡ khi có inline code (ISSUE-03), H1 trùng (B5), link tới file `.md` không tồn tại thì hiển thị dạng text thường (không phải link chết). Nếu viết lại parser, được phép dùng thư viện chuẩn (vd `react-markdown` + `remark-gfm` + sanitize) — ghi lý do; phải giữ an toàn XSS.
- Lesson player B2: dưới 1280px khi mở drawer "Sổ tay & Thẻ" thì tự thu sidebar trái; dưới 768px drawer thành bottom-sheet/overlay toàn màn. Cột đọc không bao giờ < 320px.
- Quiz: bật immersive mode như `/learn`.
- Mobile 390px: thêm nút tìm kiếm (mở Command Palette) vào header hoặc MobileNav. Nút "Tạo khóa học" trên tablet/mobile phải có nhãn hoặc tooltip + aria-label (B8). Separator metadata thống nhất một ký tự `·` (B11).
- Hiệu năng frontend (theo C0-perf): skeleton giữ kích thước layout thay spinner toàn màn ở auth gate `app/app/layout.tsx` (mục tiêu CLS < 0.1); `dynamic import` cho `pdfjs-dist`, `gsap` và các panel nặng của `/workspace` và `/learn`; khử request trùng `/api/v1/workspace/dashboard` (cache/dedupe dùng chung giữa Sidebar và page); giảm N+1 progress ở catalog nếu làm được thuần frontend (nếu cần endpoint mới thì ghi vào QUESTIONS, không sửa backend).

## Thứ tự
1. Tokens + font + 10 component nền → commit `feat(ui): design tokens and base components`.
2. Shell: Sidebar, TopNavHeader, MobileNav, Command Palette, branding → commit.
3. Lesson player `/learn` + markdown → commit.
4. Dashboard `/app` + review cap + settings → commit.
5. Review FSRS `/app/review` + Quiz → commit.
6. Courses, course detail/edit/new, Studio, Notes, Documents, Library, Settings → commit.
7. Landing `/`, `/auth`, và `/workspace` (áp token mới, không cần viết lại logic) → commit.
8. Perf fixes → commit `perf(frontend): ...`.

## Ràng buộc
- Giữ nguyên MỌI `data-testid`, route, và hành vi nghiệp vụ. Không xóa tính năng.
- Sau mỗi giai đoạn: `cd frontend; npx tsc --noEmit` phải exit 0.

## Kiểm chứng cuối (bắt buộc, ghi lệnh + exit code)
1. `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 -Build` → exit 0.
2. Chụp lại toàn bộ ảnh với đúng bộ tên như `c0-before` vào `docs/qa-round2/screenshots/c1-after/` (tái sử dụng script C0-UX trong `docs/qa-round2/scripts/`).
3. Chạy lại script đo của C0-PERF (`docs/qa-round2/scripts/c0-perf/`) → lưu `docs/qa-round2/data/c1-perf/`, lập bảng so sánh trước/sau. Mục tiêu: CLS < 0.1 mọi trang `/app/*`, LCP < 2.5s mobile, TBT `/workspace` mobile giảm ≥ 40%, JS `/learn` giảm.
4. Chạy `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1` (E2E 129 test + backend). Test nào fail: phân loại (a) lỗi thật do redesign → sửa code; (b) test assert text/visual cũ đã đổi có chủ đích → được sửa test, liệt kê TỪNG test + lý do trong báo cáo. Không được skip test.
5. Chạy lại `restart-servers.ps1` (không -Build) để app sống ở 3000/5080.

Báo cáo cuối: `docs/qa-round2/C1-redesign-report.md` — gồm bảng trước/sau hiệu năng, danh sách ảnh so sánh tiêu biểu (đường dẫn before/after), các issue C0 đã đóng/chưa đóng, test đã sửa và lý do, mục Chưa kiểm chứng.
