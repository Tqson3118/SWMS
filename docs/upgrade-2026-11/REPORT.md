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

Tổng quy mô thay đổi (tính đến `fa9ce99`): `git diff --stat 3763b88..HEAD` ghi nhận **41 files changed, 41,077 insertions(+), 1,113 deletions(-)**, không có file nào bị xóa (`git diff --diff-filter=D --name-only` rỗng).

---

## 2. Cơ chế Xử lý Dữ liệu & DB (`swms_local_dev.db`)

- Tệp cơ sở dữ liệu hiện có của người dùng (`d:\03_Study\Studxy\swms_local_dev.db`, ~134 MB cùng `-shm`/`-wal`) **được giữ nguyên 100%**, không xóa, không ghi đè và không cần đổi tên sang bản backup mới.
- Lý do kỹ thuật: Toàn bộ bộ seed mới (`EnglishPracticeContent.cs`, `WriteInPracticeContent.cs`, `WriteInTheoryExpansion.cs`, `JapaneseChinesePracticeContent.cs`, `SeedDataInitializer.cs`) sử dụng ID xác định (`CreateDeterministicActivityId`), kiểm tra khóa `(CourseId, LessonId, Type, OrderIndex)` để bổ sung idempotent, và chỉ cập nhật `ContentMarkdown` khi `lesson.CurrentVersion == 1` và chưa có marker nâng cấp — không ghi đè bài học do người dùng tự chỉnh sửa (`CurrentVersion > 1`).
- Bài kiểm thử SQLite thực tế `Test17_LegacyDbAt3763b88_UpgradesCleanlyViaSeedAsync_WhenUsersAlreadyExist` đã mô phỏng đúng trạng thái DB ở `3763b88` (đã có `Users` và các khóa cũ) và xác nhận `SeedDataInitializer.SeedAsync(db)` nâng cấp sạch sẽ toàn bộ dữ liệu mới mà không mất/trùng dữ liệu cũ.

---

## 3. Bằng chứng Kiểm chứng Thực tế (Sau commit `fa9ce99`)

Tất cả 4 lệnh kiểm chứng bắt buộc đều đã được chạy độc lập sau lần chỉnh sửa cuối (log lưu tại thư mục artifact `scratch/logs/s4-verify-*.log`):

| # | Lệnh đã chạy | Exit code | Kết quả thực tế quan sát được |
|---|---|---|---|
| 1 | `dotnet build backend\Swms.Api.sln` | `0` | `Build succeeded.` — `0 Error(s)`, `4 Warning(s)` (cảnh báo NuGet `NU1903` có sẵn của `SQLitePCLRaw.lib.e_sqlite3` 2.1.11). Thời gian `1.62s`. |
| 2 | `dotnet test backend\tests\Swms.Tests --no-build` | `0` | `Passed! - Failed: 0, Passed: 98, Skipped: 0, Total: 98, Duration: 1 m 22 s` (tăng từ 88 lên 98 integration/unit tests xUnit). |
| 3 | `cd frontend && npm run test:unit` | `0` | Pass toàn bộ 5 suite unit test Node (`--experimental-strip-types`):<br>- `fsrs-engine.test.ts`: FSRS v5 Golden Vectors passed (`1,000 cards p99 = 0.0124ms < 5ms SLO`)<br>- `course-helpers.test.ts`: `18/18` passed (gồm Nhật N5 & Trung HSK1)<br>- `language-practice.test.ts`: `16/16` passed (EN + CJK `ja-JP`/`zh-CN`)<br>- `practice-helpers.test.ts`: `6/6` passed<br>- `writein-practice.test.ts`: `11/11` passed |
| 4 | `cd frontend && npm run build` | `0` | Next.js `15.5.28` build thành công, lint & typecheck pass, xuất đủ static/dynamic routes bao gồm `/app/practice` (`9.79 kB`) và `/app/practice/[courseId]/[lessonId]` (`8.05 kB`). |

*Lưu ý file làm việc ngoài phạm vi commit:* `docs/qa-round2/E1-inline-practice-summary.md` (đã có thay đổi sẵn từ trước khi bắt đầu phiên) và `frontend/tsconfig.tsbuildinfo` (file cache build) được loại trừ khỏi mọi commit.

---

## 4. Chưa kiểm chứng

1. **Playwright E2E trên server live & kiểm tra trực quan giao diện**: Các file E2E (`frontend/e2e/practice-expansion-live.spec.ts`, `frontend/e2e/practice-expansion.spec.ts`, `frontend/e2e/v2-topdown-sitemap.spec.ts`) đã được cập nhật selector theo luồng `/app/practice` mới và compile sạch, nhưng chưa khởi chạy đồng thời backend + frontend live để chạy trọn bộ Playwright trên trình duyệt thật (cũng như kiểm tra trực quan bằng mắt/screenshot tại các viewport `390x844`, `768x1024`, `1440x900` ở cả Dark/Light mode).
2. **Gemini Proxy live (`usedLiveEndpoint === true`)**: Nhánh fallback `"Chấm theo từ khóa (offline)"` và hợp đồng JSON đã được kiểm chứng đầy đủ qua backend HTTP test và frontend unit test; nhánh chấm AI trực tiếp qua proxy Gemini đang chạy tại `http://127.0.0.1:8045` chưa được kiểm chứng trực tiếp với khóa API live trong phiên này.
3. **Web Speech API phần cứng thực tế (`SpeechRecognition` & `speechSynthesis`)**: Các giọng đọc và nhận diện giọng nói `en-US` / `en-GB` / `ja-JP` / `zh-CN` trên trình duyệt Chrome/Edge thực tế phụ thuộc vào micro và gói ngôn ngữ OS chưa được kiểm thử tương tác thủ công trên trình duyệt.
