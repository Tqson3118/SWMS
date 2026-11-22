# Báo Cáo Hoàn Thành Nâng Cấp Hệ Thống Khóa Học & Thực Hành (Studxy — 22/11/2026)

## 1. Tổng quan & Chuỗi Commit

Đợt nâng cấp được thực hiện tuần tự từ commit nền `3763b88` qua các bước sau (chưa `git push`, toàn bộ commit nằm trên nhánh `main` cục bộ):

| Bước | Commit | Mô tả | Model CLI thực hiện |
|---|---|---|---|
| **Bước 0** | `3763b88` (base) | Kiểm tra baseline build & test trước nâng cấp (`dotnet test` 88/88, `npm run test:unit`, `npm run build`) | — |
| **Bước 1** | `d1579db` | Mở rộng engine & bộ đề tiếng Anh (`s1a` + `s1b`): 3 mức Dễ/Vừa/Khó, Cloze, Dịch Việt→Anh, Sửa ngữ pháp, Roleplay + chấm AI/từ khóa offline, tô màu phát âm từng từ + gợi ý lỗi người Việt, tự đưa từ/câu sai vào Flashcard FSRS, nâng cấp lý thuyết & 48 bộ `PracticeActivity` cho 6 bài tiếng Anh | `agy` (`gemini-4-neon-xhigh`) + `agy2` (`gemini-3.8-flash-high`) |
| **Bước 2** | `b041796` | Tách Thực hành thành tính năng độc lập (`s2`): trang Hub `/app/practice` (hàng ôn `needsReview`, `fsrsDueCount`, `streakDays`, thống kê 7 ngày, danh sách khóa/bài từ `GET /api/v1/practice/stats`), trang toàn màn hình `/app/practice/[courseId]/[lessonId]`, thêm mục `Luyện tập` trên `Sidebar` & `MobileNav`, bỏ tab giữa + lab nhúng trong trang học bài để thay bằng nút `Luyện bài này →` + 1–2 câu kiểm tra nhanh, cập nhật 3 file Playwright E2E liên quan | `agy` (`gemini-4-hydrogen-xhigh` sau khi `gemini-4-neon-xhigh` gặp 1 lần lỗi `failed to start conversation: backend returned an empty conversation id`) |
| **Bước 3a** | `9e07409` | Engine write-in (Active Recall) cho Code JS, Đầu tư & Tâm lý học (`s3a`): `writein-practice.ts`, Web Worker sandbox (Code JS: viết hàm chạy test thật, đoán output, sửa bug, điền code, Parsons), Đầu tư (tự tính tay số ngẫu nhiên 5 công thức có dung sai, tình huống + giải thích bằng lời, nhật ký quyết định), Tâm lý học (nhận diện thiên kiến tự gõ, Feynman, bài CBT có đáp án/phân tích, nhật ký áp dụng), nối mọi bài làm sai vào `needsReview` của `/app/practice` + FSRS | `agy` (`gemini-4-neon-xhigh`) |
| **Bước 3b** | `80b7faa` | Mở rộng kho đề write-in cho toàn bộ 18 bài (`Coding` 6 bài: 62 đề, `Investing` 6 bài: 48 đề, `Psychology` 6 bài: 48 đề — tổng 158 bài tập write-in, ≥ 8 bài/lesson, mỗi dạng ≥ 2) + nâng cấp lý thuyết an toàn (`WriteInTheoryExpansion.cs`) | `agy2` (`gemini-3.8-flash-high`) |
| **Bước 3c** | `ae7984f` | Thêm 2 khóa sơ cấp **Tiếng Nhật (N5)** (`22222222-2222-4222-8222-222222222227`, 3 chương × 2 bài, 48 gói / 228 đề) và **Tiếng Trung (HSK1)** (`22222222-2222-4222-8222-222222222228`, 3 chương × 2 bài, 48 gói / 228 đề) + engine chuẩn hóa CJK/Pinyin/Kana, Web Speech API `ja-JP`/`zh-CN`, định tuyến đa ngôn ngữ | `agy` (`gemini-4-neon-xhigh`) |
| **Bước 4** | `fa9ce99` | Kiểm toán toàn bộ diff `3763b88..ae7984f` (`s4_audit`): dọn dẹp Web Worker qua `AbortSignal` khi unmount, bổ sung nhãn chấm offline cho tab Feynman chung trong `LessonPracticeLab.tsx`, sửa seed enrollment & legacy activity keys theo từng `(LessonId, OrderIndex)`, Việt hóa chuỗi UI còn sót, bổ sung `Test17` mô phỏng nâng cấp DB cũ từ `3763b88` | `agy` (`gemini-4-argon-high`) |
| **Bước 5** | `8030d2a` | Vá 4 lỗi UI/UX phát hiện từ kiểm tra trực quan V1: hết tràn `390px` trên thanh công cụ `/learn` & chip `InlineSpeechPracticeBar`, bổ sung ô `speaking-manual-transcript` + fallback chấm phát âm từng từ & gợi ý lỗi người Việt ở Tab 3 `EnglishPracticeLab`, đưa `BugFixWriteInPanel` (JS Web Worker) lên đầu Tab 2 `Săn Lỗi Thực Chiến`, làm sạch `extractQuickCheckQuestions` & bổ sung fallback `/api/v1/practice/activities` cho `Quick-Check`, bỏ text nhắc "tab Thực hành" cũ, đồng bộ selector `FinancePracticeLab` & E2E | `agy2` (`gemini-3.8-flash-high`) |

Tổng quy mô thay đổi (tính đến `8030d2a`): `git diff --stat 3763b88..8030d2a` ghi nhận đầy đủ các nâng cấp trên nhánh `main` cục bộ mà **không xóa file** nào (`git diff --diff-filter=D --name-only 3763b88..HEAD` rỗng).

---

## 2. Cơ chế Xử lý Dữ liệu & DB (`swms_local_dev.db`)

- Tệp cơ sở dữ liệu hiện có của người dùng (`d:\03_Study\Studxy\swms_local_dev.db`, ~134 MB cùng `-shm`/`-wal`) **được giữ nguyên 100%**, không xóa, không ghi đè và không làm mất dữ liệu học thật.
- Lý do kỹ thuật: Toàn bộ bộ seed mới (`EnglishPracticeContent.cs`, `WriteInPracticeContent.cs`, `WriteInTheoryExpansion.cs`, `JapaneseChinesePracticeContent.cs`, `SeedDataInitializer.cs`) sử dụng ID xác định (`CreateDeterministicActivityId`), kiểm tra khóa `(CourseId, LessonId, Type, OrderIndex)` để bổ sung idempotent, và chỉ cập nhật `ContentMarkdown` khi `lesson.CurrentVersion == 1` và chưa có marker nâng cấp — không ghi đè bài học do người dùng tự chỉnh sửa (`CurrentVersion > 1`).
- Bài kiểm thử SQLite thực tế `Test17_LegacyDbAt3763b88_UpgradesCleanlyViaSeedAsync_WhenUsersAlreadyExist` đã mô phỏng đúng trạng thái DB ở `3763b88` (đã có `Users` và các khóa cũ) và xác nhận `SeedDataInitializer.SeedAsync(db)` nâng cấp sạch sẽ toàn bộ dữ liệu mới mà không mất/trùng dữ liệu cũ.

---

## 3. Bằng chứng Kiểm chứng Thực tế (Sau bản vá UI/UX `8030d2a`)

| # | Hạng mục & Lệnh đã chạy | Exit code | Kết quả thực tế quan sát được |
|---|---|---|---|
| 1 | **Frontend Unit Tests:** `cd frontend && npm run test:unit` | `0` | Pass toàn bộ 5 suite (`FSRS v5`, `course-helpers` 18/18, `language-practice` 16/16, `practice-helpers` 7/7, `writein-practice` 11/11) |
| 2 | **Frontend Production Build:** `cd frontend && npm run build` | `0` | Next.js `15.5.28` build thành công, lint & typecheck pass, xuất đủ routes bao gồm `/app/practice` và `/app/practice/[courseId]/[lessonId]` |
| 3 | **Pipeline Kiểm chứng Toàn diện:** `powershell -NoProfile -ExecutionPolicy Bypass -File "d:\03_Study\Studxy\scripts\verify.ps1"` | `0` | Tổng thời gian `432.66s` (Log: `test-results/verification/20261122-154603`):<br>- **Backend Release Build** (`Swms.Api.sln`): succeeded<br>- **Backend xUnit** (`Swms.Tests`): **98 / 98 passed**, 0 failed, 0 skipped<br>- **Multi-User Benchmark** (`benchmark-multiuser.mjs`): **6 / 6 scenarios passed** (`377` requests, `30.91 req/s`, `p50: 73.59ms`, `p95: 2555.95ms`)<br>- **Playwright E2E (12 suites bao gồm `practice-expansion-live.spec.ts` không mock)**: **131 / 131 passed** (`4.8m`) |
| 4 | **Playwright Practice Suite (Mock + Live):** `npx.cmd playwright test e2e/practice-expansion.spec.ts e2e/practice-expansion-live.spec.ts --reporter=list` | `0` | **7 / 7 passed** (`15.7s`) trên Release backend (`5080`) + `next start` (`3000`) + DB audit riêng |
| 5 | **Kiểm tra Trực quan & Console Live (Visual Audit V2):** `node "...\scratch\audit_visual_live.mjs"` | `0` | **19 / 19 screenshots**, **0 ConsoleErrors**, **0 PageErrors**, **0 HttpErrors**, **0 Layout Overflows (>2px hoặc `overflowingSample`)** trên `1440×900` (Light/Dark) và `390×844` |

*Lưu ý file làm việc ngoài phạm vi commit:* `docs/qa-round2/E1-inline-practice-summary.md` (đã có thay đổi sẵn từ trước) và `frontend/tsconfig.tsbuildinfo` (file cache build) được loại trừ khỏi mọi commit.

---

## 4. Chưa kiểm chứng

1. **Gemini Proxy live (`usedLiveEndpoint === true`):** Nhánh fallback `"Chấm theo từ khóa (offline)"` và hợp đồng JSON đã được kiểm chứng đầy đủ qua backend HTTP test và frontend unit/E2E test; nhánh gọi AI trực tiếp qua proxy Gemini tại `http://127.0.0.1:8045` chưa chạy kiểm thử với API key live trong phiên này.
2. **Web Speech API phần cứng thực tế (`SpeechRecognition` & `speechSynthesis`) qua micro/loa vật lý:** Môi trường tự động headless không có micro vật lý để kiểm tra nhận diện giọng nói `en-US` / `en-GB` / `ja-JP` / `zh-CN` ngoài đời thật (hệ thống hiển thị thông báo rõ khi trình duyệt không hỗ trợ và cho phép nhập câu đọc thử để kiểm tra logic tô màu + gợi ý phát âm, không tạo điểm giả).
3. **Thiết bị di động vật lý & Production PostgreSQL:** Đã kiểm chứng bố cục `390×844` trên Playwright Chromium và toàn bộ logic SQL trên SQLite; chưa thử trực tiếp trên iOS Safari / Android Chrome vật lý và cụm PostgreSQL live.
