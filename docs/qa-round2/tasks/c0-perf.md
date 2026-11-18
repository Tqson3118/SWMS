Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ C0-PERF: Đo baseline hiệu năng (CHỈ ĐO, KHÔNG SỬA CODE ỨNG DỤNG)

Mục tiêu: có số liệu "trước" để so với sau đợt redesign.

1. Đo trên bản production đang chạy ở http://127.0.0.1:3000 các trang:
   `/`, `/auth`, `/app`, `/app/courses`, `/app/courses/<id>` (một khóa có dữ liệu), `/app/courses/<id>/learn`, `/app/courses/<id>/quiz`, `/app/review`, `/app/documents`, `/app/notes`, `/app/settings`, `/workspace`.
   Trang cần đăng nhập: đăng nhập demo "Sơn Trần" bằng Playwright trước, rồi đo trong cùng context.
2. Chỉ số cần có cho mỗi trang (desktop 1440x900, và mobile 390x844 với CPU throttle 4x):
   - LCP, CLS, INP/TBT (hoặc Long Tasks tổng), FCP, TTFB, tổng JS tải (KB), số request, thời gian tới khi nội dung chính hiển thị.
   - Cách đo gợi ý: Lighthouse qua `npx --yes lighthouse` (dùng Chromium của Playwright qua biến `CHROME_PATH`) cho trang public; trang cần đăng nhập dùng Playwright + CDP (`Performance.getMetrics`, `PerformanceObserver` cho LCP/CLS/longtask). Chạy mỗi trang 3 lần, lấy median.
3. Thêm: kích thước bundle từ `frontend/.next` (route nào nặng nhất, ví dụ `/workspace` 335 kB, `/learn` 258 kB first load), các thư viện nặng (pdf.js, gsap, ...), API chậm nhất (đo thời gian response các request `/api/v1/*` khi tải trang).
4. Liệt kê top 10 nguyên nhân gây chậm/giật kèm đề xuất cụ thể (file/component) cho đợt redesign.

Script đặt ở `docs/qa-round2/scripts/c0-perf/`, dữ liệu thô JSON ở `docs/qa-round2/data/c0-perf/`.
Báo cáo cuối: `docs/qa-round2/C0-perf-baseline.md` (có bảng số liệu median từng trang).
Không commit gì ngoài thư mục `docs/qa-round2/` (được phép commit: `chore(qa): C0 perf baseline`).
