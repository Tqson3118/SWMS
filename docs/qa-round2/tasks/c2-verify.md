Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ C2-VERIFY: Kiểm chứng sau Redesign (Ảnh After, Hiệu năng C1 vs C0, `scripts/verify.ps1`, Báo cáo C1)

## Bối cảnh
Toàn bộ Giai đoạn 1–8 của C1 Redesign đã được implement. Nhiệm vụ của bạn là thực hiện bước nghiệm thu C2 toàn diện trên bản production build mới nhất.

**QUAN TRỌNG — KHÔNG XÓA/GHI ĐÈ DỮ LIỆU C0:**
- Giữ nguyên toàn bộ `docs/qa-round2/screenshots/c0-before/` và `docs/qa-round2/data/c0-perf/` và `docs/qa-round2/scripts/c0-perf/`.
- Tạo script mới (copy/sửa đường dẫn đầu ra) để lưu kết quả sau redesign vào `docs/qa-round2/screenshots/c1-after/` và `docs/qa-round2/data/c1-perf/`.

## Các bước thực hiện bắt buộc

### Bước 1: Đảm bảo bản production build mới nhất đang chạy
- Chạy `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 -Build` (bắt buộc exit 0).
- Kiểm tra `http://127.0.0.1:3000` và `http://127.0.0.1:5080/health` đều trả HTTP 200.

### Bước 2: Chụp toàn bộ ảnh sau redesign (`c1-after`)
- Đọc `docs/qa-round2/scripts/capture-all.js` và danh sách file trong `docs/qa-round2/screenshots/c0-before/`.
- Tạo script mới `docs/qa-round2/scripts/capture-c1-after.js` sao chép logic chụp của `capture-all.js` (cùng routes, viewports 1440x900 / 768x1024 / 390x844, light/dark, cùng tên file ảnh) nhưng xuất ra `docs/qa-round2/screenshots/c1-after/`.
- Đồng thời kiểm tra tự động trong script: phát hiện overflow ngang (`scrollWidth > innerWidth`) hoặc cột nội dung `< 320px` trên Lesson Player khi mở drawer (B2), kiểm tra Console/Page errors. Ghi log kết quả vào `docs/qa-round2/data/c1-perf/visual-checks.json`.

### Bước 3: Đo hiệu năng & Bundle & Lighthouse sau redesign (`c1-perf`)
- Sao chép bộ script trong `docs/qa-round2/scripts/c0-perf/` sang `docs/qa-round2/scripts/c1-perf/` và sửa các đường dẫn output sang `docs/qa-round2/data/c1-perf/` và `docs/qa-round2/screenshots/c1-perf/`.
- Chạy đầy đủ cả 3 phần:
  1. `measure-runner.mjs` (Playwright + CDP trên 12 routes, Desktop 1440x900 và Mobile 390x844, 3 lần lấy median).
  2. `bundle-analyzer.mjs` (phân tích kích thước chunk từ `frontend/.next`).
  3. `lighthouse-public.mjs` (Lighthouse cho `/` và `/auth` desktop + mobile).
- Nếu phát hiện trang `/app/*` vẫn có CLS >= 0.1 hoặc regression rõ rệt so với mục tiêu trong `docs/qa-round2/tasks/c1-redesign.md`: ghi rõ nguyên nhân; nếu là lỗi nhỏ dễ sửa thuần frontend thì sửa và commit `fix(perf): ...` rồi rebuild & đo lại.

### Bước 4: Chạy toàn bộ bộ kiểm thử `scripts/verify.ps1` + unit test
- Chạy `cd frontend; npm run test:unit` (exit 0).
- Chạy `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`.
- Lưu ý quan trọng: `scripts/verify.ps1` sẽ tự dừng/chạy server theo cách riêng của nó; sau khi xong BẮT BUỘC chạy lại `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 -Build` (hoặc không `-Build` nếu không sửa code) để đảm bảo app sống lại ở `127.0.0.1:3000` và `127.0.0.1:5080`.
- Xử lý test fail (nếu có):
  - (a) Lỗi thật do redesign gây ra → sửa code trong `frontend/` rồi commit `fix(ui): ...`, rebuild và chạy lại.
  - (b) Test assert text/visual cũ đã đổi có chủ đích theo yêu cầu C1 (ví dụ `"SWMS"` → `"Studxy"`, định dạng lời chào mới) → được sửa test hợp lệ, liệt kê TỪNG test + file + lý do trong báo cáo.
  - **TUYỆT ĐỐI KHÔNG** skip test, làm giả pass, hay sửa `data-testid` hiện có.

### Bước 5: Ghi báo cáo cuối và commit
- Viết báo cáo đầy đủ vào `docs/qa-round2/C1-redesign-report.md` gồm:
  1. Kết quả chính (Build, `test:unit`, `verify.ps1`, số lượng ảnh `c1-after`, đạt/chưa đạt mục tiêu CLS < 0.1, LCP < 2.5s, TBT `/workspace` mobile, JS `/learn`, zero horizontal overflow).
  2. Bảng **Trước (`c0-perf`) vs Sau (`c1-perf`)** cho: Lighthouse, Playwright CDP (Desktop & Mobile), Bundle size các route chính, và tần suất gọi `/api/v1/workspace/dashboard`.
  3. Bảng đối chiếu đóng các issue từ `C0-ux-audit.md` và danh sách cũ (`B2`, `B5`, `B8`, `B10`, `B11`, `B16`, `ISSUE-03`, v.v.).
  4. Danh sách ảnh so sánh tiêu biểu (`c0-before/` vs `c1-after/`).
  5. Danh sách test đã sửa (nếu có) + lý do cụ thể.
  6. Bằng chứng thực thi: lệnh chính xác + exit code + đường dẫn log/json.
  7. Mục **Chưa kiểm chứng**.
- KHÔNG để lại background task nào.
- Đảm bảo server đang chạy ở `http://127.0.0.1:3000` và `http://127.0.0.1:5080/health`.
- Commit script kiểm chứng mới và báo cáo (`docs/qa-round2/`) với message `chore(qa): C1 redesign and C2 verification report`.
