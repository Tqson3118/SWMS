Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ C1-REDESIGN-CONT: Hoàn tất Giai đoạn 6, 7, 8 của Design System "Clean & Calm Learning" + Hiệu năng Frontend

## Bối cảnh hiện tại
Các giai đoạn 1–5 của `docs/qa-round2/tasks/c1-redesign.md` đã hoàn thành và đã commit trên branch `qa/agy2-round2`:
- `fc76f44 feat(ui): design tokens and base components` (`frontend/src/components/ui/`: Badge, Button, Card, Dialog, EmptyState, Input, ProgressBar, StatCard, Tabs, Tooltip)
- `ab860ea feat(layout): shell navigation mobile search and studxy branding`
- `a12ba78 feat(learn): lesson player split layout and markdown fix`
- `88cfcde feat(dashboard): calm dashboard and daily review cap`
- `2110b93 feat(review): fsrs 3d card and immersive quiz`

Ngoài ra, đầu Giai đoạn 6 đã có sẵn thay đổi chưa commit trong working tree:
- `frontend/src/lib/course-helpers.ts` (đã sửa regex phân loại khóa học + thêm nhóm Ngoại ngữ)
- `frontend/src/lib/course-helpers.test.ts` (unit test)
- `frontend/package.json` (đã cập nhật `test:unit`)

Đầu vào bắt buộc phải đọc trước:
- `docs/qa-round2/tasks/c1-redesign.md` (giữ toàn bộ ràng buộc & quy chuẩn đã chốt)
- `docs/qa-round2/C0-ux-audit.md` (mục 3, 5, 6)
- `docs/qa-round2/C0-perf-baseline.md` (mục 5: top 10 nguyên nhân chậm)

Bạn được phép SỬA CODE trong `frontend/` (**KHÔNG sửa `backend/`** ở nhiệm vụ này). Làm tuần tự từ Giai đoạn 6 → 7 → 8, sau mỗi giai đoạn chạy `cd frontend; npx tsc --noEmit` (exit 0) rồi commit riêng theo quy định.

## Chi tiết công việc cần hoàn tất

### Giai đoạn 6: Courses, Studio, Notes, Documents, Library, Settings
1. Kiểm tra `frontend/src/lib/course-helpers.ts` và `frontend/src/lib/course-helpers.test.ts`, đảm bảo kiểm chứng ít nhất 10 tên khóa học thật trong DB demo (hoặc các case tiếng Việt thực tế) và chạy `cd frontend; npm run test:unit` pass (exit 0).
2. Áp dụng design system "Clean & Calm Learning" (token thống nhất từ `globals.css`, light-first + dark mode đầy đủ, font Be Vietnam Pro / stack chuẩn, micro-animation `prefers-reduced-motion`, separator `·`, nút Tạo khóa học responsive có label/tooltip + aria-label chuẩn B8) và tái sử dụng các component nền trong `frontend/src/components/ui/` cho toàn bộ các trang:
   - `/app/courses` (danh sách, chi tiết `[id]`, sửa, tạo mới)
   - `/app/studio`
   - `/app/notes`
   - `/app/documents`
   - `/app/library`
   - `/app/settings` (giữ và tích hợp phần cài đặt giới hạn phiên ôn tập mỗi ngày đã thêm ở commit `88cfcde`)
3. Giữ nguyên MỌI `data-testid`, route, và hành vi nghiệp vụ. Không xóa tính năng.
4. Kiểm tra `cd frontend; npx tsc --noEmit` và `npm run test:unit` (exit 0).
5. Commit các file thuộc Giai đoạn 6 (bao gồm `course-helpers.ts`, `course-helpers.test.ts`, `package.json`, và các trang đã sửa) với message:
   `feat(pages): calm redesign for courses studio notes documents library`

### Giai đoạn 7: Landing (`/`), `/auth`, và `/workspace`
1. Áp dụng hệ token mới ("Clean & Calm Learning" — light-first, dark mode đầy đủ, branding thống nhất "Studxy") cho:
   - Landing `/`
   - Auth `/auth`
   - Workspace `/workspace` (áp token mới, giữ nguyên logic nghiệp vụ)
2. Giữ nguyên MỌI `data-testid`, route, và hành vi nghiệp vụ. Không xóa tính năng.
3. Kiểm tra `cd frontend; npx tsc --noEmit` (exit 0).
4. Commit với message:
   `feat(public-workspace): apply calm design tokens to landing auth and workspace`

### Giai đoạn 8: Tối ưu hiệu năng Frontend (`perf(frontend): ...`)
1. Theo `docs/qa-round2/C0-perf-baseline.md` và `docs/qa-round2/tasks/c1-redesign.md`:
   - Kiểm tra và đảm bảo auth gate ở `frontend/src/app/app/layout.tsx` sử dụng skeleton giữ nguyên kích thước layout thay vì spinner toàn màn (mục tiêu CLS < 0.1).
   - Dùng `next/dynamic` (`dynamic import`) cho `pdfjs-dist`, `gsap` và các panel/component nặng của `/workspace` và `/learn` (khi không cần hiển thị ngay lúc initial load).
   - Khử/cache request trùng `GET /api/v1/workspace/dashboard` dùng chung giữa `Sidebar` và các trang `/app/*` (hoặc dedupe in-flight + short-lived cache đúng cách, vẫn giữ cơ chế invalidate/refresh khi mutate dữ liệu).
   - Giảm N+1 progress ở catalog nếu làm được thuần frontend (nếu cần endpoint mới ở backend thì ghi vào `docs/qa-round2/QUESTIONS-c1-redesign-cont.md`, không sửa backend).
2. Kiểm tra `cd frontend; npx tsc --noEmit` và `npm run test:unit` (exit 0).
3. Commit với message:
   `perf(frontend): dynamic imports skeleton gate and dashboard fetch dedupe`

## Kiểm tra build & chạy lại server
1. Chạy `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 -Build` (bắt buộc exit 0, đảm bảo Next.js production build thành công và cả 2 server sống ở `127.0.0.1:3000` & `127.0.0.1:5080`).
2. KHÔNG để lại bất kỳ background task nào.
3. Ghi tóm tắt ngắn gọn kết quả (các commit đã tạo, file đã sửa, exit code của `tsc`, `test:unit`, `restart-servers.ps1 -Build`, mục **Chưa kiểm chứng**) vào `docs/qa-round2/C1-stages-6-8-summary.md` (không cần commit file summary này, hoặc commit cùng docs nếu muốn, nhưng không động vào code backend).
