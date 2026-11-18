Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ A-RETEST: Kiểm chứng toàn diện sau đợt sửa Đợt A & Tổng hợp Báo cáo QA Đợt A

## Mục tiêu
Toàn bộ các bản sửa lỗi Backend (`a-fix-backend`), Frontend (`a-fix-frontend`), và đơn giản hóa chữ hiển thị (`a-simplify-copy`) đã được áp dụng trên nhánh `qa/agy2-round2`. Nhiệm vụ của bạn là build lại bản production mới nhất, chạy lại tất cả các bộ kiểm thử Đợt A và `scripts/verify.ps1`, sửa ngay nếu phát hiện hồi quy/sai lệch nhỏ, và viết báo cáo tổng kết Đợt A.

## Các bước thực hiện bắt buộc

### Bước 1: Rebuild & Khởi động lại server production
- Chạy `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 -Build` (bắt buộc exit code 0).
- Kiểm tra `http://127.0.0.1:3000` và `http://127.0.0.1:5080/health` đều trả HTTP `200`.

### Bước 2: Chạy lại các bộ kiểm thử Đợt A (tất cả kịch bản đã viết từ 5 vai)
1. **Bảo mật (`a-hacker`):**
   - Chạy `node docs/qa-round2/scripts/a-hacker/run-all-tests.mjs`
   - Xác minh kết quả mới cho các mục đã vá: `AUTH-01` (rate-limit 429), `AUTH-04` (mật khẩu yếu bị chặn), `AUTH-05` (từ chối JWT ký bằng key lộ cũ), `HDR-01`..`HDR-07` (security headers backend & frontend, không lộ server banner), `INJ-03` (HTML/XSS nguy hiểm bị làm sạch), `INFO-04` (không còn mật khẩu demo trong JS bundle client), `INFO-05` (không còn secret cứng trong `appsettings.json`).
2. **Chống lạm dụng nghiệp vụ (`a-abuser`):**
   - Chạy `node docs/qa-round2/scripts/a-abuser/run-all-pocs.js`
   - Xác minh từng PoC từ `poc-01` đến `poc-06`: quota AI được trừ khi `generate-course`, custom upstream lỗi được hoàn credit, idempotency `ReviewEventId` cô lập theo user, batch quá 5 lần/thẻ bị chặn, Undo không xóa `lapses` về 0, không thể hoàn thành bài khóa public khi chưa enroll & giới hạn tốc độ, MCQ không còn mẫu cố định A/B/C, tự luận đã chấm trả 409 nếu nộp lại, `baseVersion` sai trả 400 & có `DELETE /api/v1/notebooks/{id}`, `StreakDays` tăng khi học thật.
3. **UX, CLS, Markdown & Accessibility (`a-ux-perf`):**
   - Chạy lại các script kiểm toán:
     - `node docs/qa-round2/scripts/a-ux-perf/investigate-cls-sources.mjs`
     - `node docs/qa-round2/scripts/a-ux-perf/check-ielts-markdown.js`
     - `node docs/qa-round2/scripts/a-ux-perf/check-courses-filter.js`
     - `node docs/qa-round2/scripts/a-ux-perf/audit-accessibility.js`
     - `node docs/qa-round2/scripts/a-ux-perf/test-pdf-404.js`
   - Xác nhận CLS Desktop trên `/app/*` và CLS Mobile `/learn` đã giảm xuống `< 0.1` (mục tiêu `< 0.05`), không còn rò rỉ `CODE_0`, filter `/app/courses` 1440px không bị cắt, `/app/review` có `<h1>`, Dark Mode đạt tương phản WCAG AA, PDF 404 hiện lỗi thân thiện không quay spinner vô tận.
4. **Mobile & Tap Targets (`a-newbie-mobile`):**
   - Chạy lại `node docs/qa-round2/scripts/a-newbie-mobile/01-onboarding-journey.js` và `node docs/qa-round2/scripts/a-newbie-mobile/02-tap-target-and-layout-audit.js`.
   - Xác minh tap targets đã được cải thiện, nút Mục lục hiển thị trên mobile/tablet, EmptyState `/app/courses` thân thiện khi chưa có khóa học.
5. **Kiểm thử Mở rộng 4 Phòng Thực Hành Chuyên Sâu & Rich HTML/CSS Markdown Cards (`Phase B`):**
   - Viết và chạy script Playwright thật `docs/qa-round2/scripts/verify-b-practice-labs.js` kiểm tra trên trình duyệt thật (Desktop `1440×900` và Mobile `390×844`):
     - Mở `/app/courses/[id]/learn` cho cả 4 nhóm khóa học: Tiếng Anh/IELTS (`EnglishPracticeLab`), Lập trình .NET/Hệ điều hành (`CodingPracticeLab`), Tài chính cá nhân & Đầu tư (`FinancePracticeLab`), và Tâm lý học / Kỹ năng sống (`PsychologyLifeSkillsLab`).
     - Xác minh Rich HTML/CSS Markdown Cards trong tab Lý thuyết (Callout Card màu & icon, bảng bo góc, khối code có nhãn & nút sao chép, nút loa phát âm IPA/English nếu có, nút cuối bài `[data-testid="lesson-end-open-practice-btn"]` bấm là chuyển ngay sang tab Thực hành).
     - Chuyển sang tab `"Thực hành"` trên mỗi khóa học: xác minh từng phòng thực hành render đúng theo khóa (và chế độ Feynman + Thẻ khái niệm lật cho khóa lý thuyết), tương tác thử các nút/thanh trượt/tab con chính, bấm lưu Sổ tay / tạo Flashcard ngay từ Lab để xác nhận thông báo & lưu thật, kiểm tra chuyển bài trên mobile tự đóng drawer TOC và hoàn thành bài học đồng bộ Dashboard, **0 console errors / 0 pageerrors**, không tràn ngang trên Mobile `390×844`, và chụp ảnh màn hình (cả Light & Dark mode) lưu vào `docs/qa-round2/screenshots/b-practice-labs/` (kèm đưa vào báo cáo `A-master-qa-report.md`).

### Bước 3: Chạy Unit Test & Toàn bộ `scripts/verify.ps1`
1. Chạy `cd frontend; npm run test:unit` (exit code 0).
2. Chạy `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1` (80 backend tests + 6 multi-user concurrency scenarios + 130 Playwright E2E tests).
   - Nếu có bất kỳ test nào fail do các bản vá bảo mật/UX/đơn giản hóa chữ: kiểm tra nguyên nhân thật; sửa code nếu là hồi quy thật, hoặc cập nhật test hợp lệ (chỉ khi test assert text cũ đã đổi có chủ đích, liệt kê rõ từng test + lý do trong báo cáo; tuyệt đối không skip test hay làm giả kết quả).
   - Nhớ chạy lại `powershell -ExecutionPolicy Bypass -File docs/qa-round2/tools/restart-servers.ps1 -Build` (hoặc không `-Build` nếu không sửa code) sau khi `verify.ps1` kết thúc để cả 2 server sống ở `127.0.0.1:3000` và `127.0.0.1:5080`.

### Bước 4: Ghi báo cáo tổng kết Đợt A và commit
- Ghi báo cáo đầy đủ vào `docs/qa-round2/A-master-qa-report.md` gồm:
  1. Tổng quan trạng thái 5 vai Trước Fix vs Sau Fix (bảng tổng hợp + exit code từng bộ script).
  2. Chi tiết kết quả retest từng hạng mục Security (`SEC-*`), Anti-Abuse (`ABUSE-*`), Student (`STU-*`), UX/Perf/CLS (`UXP-*`), Mobile (`MOB-*`) và đơn giản hóa ngôn ngữ UI.
  3. Tổng hợp các góp ý tính năng từ cả 5 vai (để chuẩn bị làm đầu vào cho Đợt B — Khóa Tiếng Anh thực hành).
  4. Bằng chứng thực thi: lệnh chính xác + exit code + đường dẫn log/JSON (`test-results/verification/...`).
  5. Mục **Chưa kiểm chứng**.
- Commit mọi thay đổi của bước retest & báo cáo: `chore(qa): A round 2 master QA and retest report`.
- Đảm bảo **KHÔNG còn background task nào** và cả `3000` / `5080` đều trả HTTP `200`.
