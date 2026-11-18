Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ A-FIX-BACKEND: Sửa toàn bộ lỗi Bảo mật & Logic Nghiệp vụ Backend (Chỉ sửa trong `backend/`)

## Quy tắc chạy song song (BẮT BUỘC)
- **PHẠM VI SỬA CODE:** CHỈ được sửa file trong thư mục `backend/`. **TUYỆT ĐỐI KHÔNG** sửa `frontend/` hoặc `docs/qa-round2/scripts/`.
- **KHÔNG chạy `git commit`** và **KHÔNG chạy `restart-servers.ps1`** (có agent frontend đang sửa song song; việc commit + rebuild + restart sẽ làm tập trung sau khi cả 2 xong).
- **KHÔNG build/test với `-c Release`** (vì server Release đang khóa `bin/Release/net10.0/Swms.Api.dll` trên cổng `5080`). Dùng cấu hình **Debug** mặc định: `dotnet build backend/src/Swms.Api/Swms.Api.csproj` và `dotnet test backend/tests/Swms.Tests/Swms.Tests.csproj`.
- **KHÔNG để lại bất kỳ background task nào** trước khi kết thúc.
- Đầu vào tham khảo: `docs/qa-round2/A-hacker-report.md`, `docs/qa-round2/A-abuser-report.md`, `docs/qa-round2/A-student-report.md`.

## Danh sách các lỗi phải sửa (giữ tương thích 100% với 80 tests `Swms.Tests` & `scripts/benchmark-multiuser.mjs`)

### 1. `SEC-CRIT-01` & `SEC-LOW-02`: Xóa khóa ký JWT & API Key cứng khỏi `appsettings.json` và fallback
- Trong `backend/src/Swms.Api/appsettings.json`: đặt `"Jwt:SigningKey": ""` và `"AiProxy:ApiKey": ""`. (Nếu cần key AI local dùng biến môi trường hoặc để trống để service fallback an toàn).
- Trong `backend/src/Swms.Api/Infrastructure/Security/SecurityServices.cs` (`JwtTokenService`):
  - Khai báo khóa ngẫu nhiên an toàn cấp tiến trình (hoặc lưu/đọc file local `App_Data/.jwt_signing_key` không track trên git):
    `private static readonly string ProcessFallbackSigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));`
  - Trong `ResolveJwtSigningKey(IConfiguration config)`: từ chối chuỗi lộ `"SWMS-v1-Enterprise-Jwt-HmacSha256-Secret-Key-2026-Studxy-Workspace!"`. Ưu tiên biến môi trường `JWT_SIGNING_KEY` (>= 32 ký tự), tiếp theo là `config["Jwt:SigningKey"]` (>= 32 ký tự và khác chuỗi lộ), nếu trống thì dùng khóa ngẫu nhiên an toàn `ProcessFallbackSigningKey` (nhờ vậy `AUTH-05` trong `01-auth-tests.mjs` không thể giả mạo token bằng khóa cũ, đồng thời mọi unit/integration test `Swms.Tests` đều chạy bình thường).

### 2. `SEC-HIGH-01` (`INFO-04`): Hỗ trợ đăng nhập Demo phía server để không nhúng mật khẩu cứng ở Frontend
- Trong `backend/src/Swms.Api/Features/SwmsEndpoints.cs`, thêm trường tùy chọn `string? DemoProfile = null` vào record `LoginRequest`.
- Trong `POST /api/v1/auth/login`: nếu `req.Password == "__DEMO_SESSION__"` hoặc `!string.IsNullOrWhiteSpace(req.DemoProfile)`:
  - Chỉ chấp nhận cho 2 email demo `"son.tran@studxy.vn"` (`demoProfile == "default"`) và `"minh.anh@studxy.vn"` (`demoProfile == "secondary"`). Nếu email khác gửi `__DEMO_SESSION__` thì từ chối `401 Unauthorized`.
  - Với `"minh.anh@studxy.vn"`, nếu chưa tồn tại trong `db.Users` thì tự động khởi tạo tài khoản Minh Anh (giống logic register + tạo workspace course/quota ban đầu) ngay trong `POST /api/v1/auth/login` để frontend không cần gọi `/register` trước gây lỗi `409 Conflict`.
  - Vẫn giữ nguyên nhánh kiểm tra mật khẩu thông thường cho mọi trường hợp khác để tương thích với các test cũ gọi `/api/v1/auth/login` bằng mật khẩu.

### 3. `SEC-HIGH-02` (`AUTH-01`): Chống Brute-Force trên `POST /api/v1/auth/login`
- Dùng `ConcurrentDictionary<string, (int FailedCount, DateTimeOffset WindowStart)>` theo email chuẩn hóa (`email.Trim().ToLowerInvariant()`):
  - Nếu cùng 1 email đăng nhập sai mật khẩu `>= 5` lần trong vòng 60 giây: trả ngay `Results.Json(new { error = "TOO_MANY_LOGIN_ATTEMPTS", message = "Tài khoản tạm khóa 1 phút do đăng nhập sai nhiều lần." }, statusCode: StatusCodes.Status429TooManyRequests)`.
  - Khi đăng nhập thành công thì xóa bộ đếm sai của email đó.
  - **Lưu ý:** Không khóa theo IP `127.0.0.1` cho các lượt đăng nhập đúng để không làm fail 130 test E2E và 30 virtual users trong `benchmark-multiuser.mjs`.

### 4. `SEC-HIGH-03` / `B3` (`HDR-01`, `HDR-02`, `HDR-03`): HTTP Security Headers & Tắt Server Banner
- Trong `backend/src/Swms.Api/Program.cs`:
  - Thêm `builder.WebHost.ConfigureKestrel(k => k.AddServerHeader = false);`.
  - Thêm middleware gắn các header cho mọi HTTP response:
    - `X-Content-Type-Options: nosniff`
    - `X-Frame-Options: SAMEORIGIN`
    - `Referrer-Policy: strict-origin-when-cross-origin`
    - `Content-Security-Policy: default-src 'self'; frame-ancestors 'self'`

### 5. `SEC-MED-03` (`AUTH-04`): Siết chính sách độ mạnh mật khẩu
- Trong `POST /api/v1/auth/register` và `POST /api/v1/auth/change-password`:
  - Yêu cầu mật khẩu `>= 6` ký tự, bắt buộc chứa ít nhất **1 chữ cái** (`char.IsLetter`) và **1 chữ số** (`char.IsDigit`).
  - Từ chối mật khẩu toàn số như `"123456"` với `400 BadRequest` (`{ error = "WEAK_PASSWORD", message = "Mật khẩu phải có ít nhất 6 ký tự, bao gồm cả chữ cái và chữ số." }`).

### 6. `SEC-MED-04` (`INJ-03`): Làm sạch thẻ HTML/XSS nguy hiểm khi lưu Flashcard & Notebook
- Viết helper `SanitizeDangerousHtml(string? input)` loại bỏ các thẻ `<script\b[^>]*>[\s\S]*?</script>`, `</?script\b[^>]*>`, thuộc tính sự kiện `on\w+\s*=\s*("[^"]*"|'[^']*'|[^\s>]+)`, và thẻ `<img\b[^>]*onerror[^>]*>` (kể cả khi xuất hiện trong chuỗi JSON/AST).
- Áp dụng cho `frontText`, `backText`, `frontAstJson`, `backAstJson` khi tạo/sửa Flashcard và cho `title`, `contentAstJson` khi tạo/sửa Notebook.

### 7. `ABUSE-01` & `ABUSE-02`: Trừ AI Quota khi `generate-course` & Hoàn credit khi Custom Upstream lỗi
- **`ABUSE-01`:** Trong `POST /api/v1/documents/{id:guid}/generate-course` (chỉ khi tạo khóa học mới, không áp dụng hoặc vẫn kiểm tra cẩn thận cho idempotent re-call để giữ pass `CourseGenerationTests.cs`): kiểm tra `CheckAiQuotaAvailableAsync(db, userId)` (nếu hết trả 429) và ghi 1 bản ghi `AiUsageLedger` (`Feature = "generate_course"`, `CreditsCharged = 1`, `PromptTokens = 120`, `CompletionTokens = 180`, `TotalTokens = 300`). Lưu ý kiểm tra `CourseGenerationTests.cs` xem test gọi `/generate-course` idempotent thế nào — khi khóa học đã tồn tại rồi (idempotent hit) thì trả ngay khóa học cũ không cần trừ lại credit!
- **`ABUSE-02`:** Trong `POST /api/v1/ai/explain` và `POST /api/v1/ai/generate-card`: nếu request có truyền custom upstream override (`aiOverrides != null && (!string.IsNullOrWhiteSpace(aiOverrides.BaseUrl) || !string.IsNullOrWhiteSpace(aiOverrides.ApiKey))`) mà gọi thất bại rơi về fallback (`!res.UsedLiveEndpoint`), hoàn lại credit: cập nhật `ledgerEntry.CreditsCharged = 0; ledgerEntry.TotalTokens = 0; ledgerEntry.CostUsd = 0;` và trả `creditsCharged = 0`. *(Khi `aiOverrides == null` vẫn giữ `CreditsCharged = 1` để Scenario S6 của `benchmark-multiuser.mjs` pass 50/50)*.

### 8. `ABUSE-04`, `ABUSE-05`, `ABUSE-06`: FSRS Idempotency theo User, Giới hạn Batch Replay, Chống xóa Lapses qua Undo
- **`ABUSE-04`:** Trong `POST /api/v1/flashcards/sync-reviews` (kiểm tra `existingReview`):
  - Nếu `existingReview != null && existingReview.UserId == userId`: trả `idempotentDuplicate = true`.
  - Nếu `existingReview != null && existingReview.UserId != userId`: sinh một `Guid` dẫn xuất tất định theo cặp `(userId, ev.ReviewEventId)` (dùng SHA256/MD5 của `$"{userId:N}:{ev.ReviewEventId:N}"`), kiểm tra xem `Guid` dẫn xuất này đã tồn tại trong DB/Local chưa; nếu chưa thì lưu bản ghi mới với `ReviewEventId = derivedEventId` và trả `reviewEventId = ev.ReviewEventId, idempotentDuplicate = false` (vừa cô lập giữa các user vừa không vi phạm unique index `review_event_id`).
- **`ABUSE-05`:** Trong `POST /api/v1/flashcards/sync-reviews`: nếu trong 1 batch có bất kỳ `flashcardId` nào xuất hiện **quá 5 lần** (`req.Reviews.GroupBy(r => r.FlashcardId).Any(g => g.Count() > 5)`), trả `400 BadRequest` (`{ error = "BATCH_PER_CARD_LIMIT_EXCEEDED", message = "Tối đa 5 lượt ôn tập cho cùng một thẻ trong một batch đồng bộ." }`).
- **`ABUSE-06`:** Khi gọi `POST /api/v1/flashcards/{id}/undo-last-review` để hoàn tác một lượt review có `lastReview.Rating == 1` (Again/Quên), lưu vết số lần quên đã hoàn tác theo từng thẻ (`ConcurrentDictionary<Guid, int> UndoneLapsesByCard`) để `card.Lapses = Math.Max(card.Lapses, undoneLapses)` cả sau khi undo lẫn trong lần review kế tiếp của thẻ đó (vẫn giữ nguyên `Stability` và `Reps` sau undo khớp với `SecurityAndAuditRegressionTests.cs`).

### 9. `ABUSE-07` & `ABUSE-11`: Chống cày bài học hàng loạt, Kiểm tra Enrollment khóa public, Cập nhật `StreakDays`
- **`ABUSE-07`:** Trong `POST /api/v1/lessons/{id:guid}/complete`:
  - Giới hạn tốc độ hoàn thành bài học theo từng user: tối đa **3 bài học khác nhau trong cửa sổ 2 giây** (nếu gọi 5 bài trong 18ms thì từ bài thứ 4 trả `429 TooManyRequests`).
  - Nếu `course != null && course.Visibility == "public" && course.InstructorId != userId && userId != SeedDataInitializer.DefaultUserId`: kiểm tra `await db.CourseEnrollments.AnyAsync(e => e.CourseId == course.Id && e.UserId == userId)`. Nếu chưa ghi danh thì trả `403 Forbidden` (`{ error = "COURSE_NOT_ENROLLED", message = "Vui lòng ghi danh khóa học trước khi đánh dấu hoàn thành bài học." }`).
  - Lưu ý kiểm tra các bài test backend trong `Swms.Tests` xem có test nào gọi `/lessons/{id}/complete` trên khóa public với user mới không — đọc trước khi sửa để không làm gãy test hợp lệ (nếu cần, đảm bảo endpoint enroll hoạt động mượt và đọc kỹ `DocumentCourseSyncTests.cs` / `CourseCreatorStudioTests.cs`).
- **`ABUSE-11`:** Với tài khoản khác `SeedDataInitializer.DefaultUserId` (`11111111-1111-4111-8111-111111111111`), khi người dùng hoàn thành bài học (`POST /lessons/{id}/complete`) hoặc đồng bộ thẻ (`POST /flashcards/sync-reviews`) mà đã có cả ít nhất 1 bài học hoàn thành lẫn ít nhất 1 lượt ôn thẻ FSRS, tự động cập nhật `user.StreakDays = Math.Max(user.StreakDays, 2)` để ghi nhận chuỗi học tiến triển. (Với tài khoản mặc định Sơn Trần, giữ nguyên `streakDays = 7` để pass `qa-e2e-suite.spec.ts`).

### 10. `ABUSE-08` & `ABUSE-09`: Xáo trộn đáp án MCQ tự sinh & Chặn nộp lại bài tự luận đã chấm
- **`ABUSE-08`:** Trong `backend/src/Swms.Api/Features/CourseGenerationService.cs` (khoảng dòng 1159), xáo trộn thứ tự các lựa chọn A/B/C/D và `correctKey` (ví dụ đảo thứ tự bằng `Random.Shared` hoặc mẫu không trùng với `["A", "B", "C"]` liên tiếp như `["C", "A", "D", "B"]`, và cập nhật lại tiền tố `"A. "`, `"B. "`, `"C. "`, `"D. "` khớp chính xác với nội dung & `correctKey`). Kiểm tra `DocumentCourseSyncTests.cs` (test đọc `ClozeSolution` từ DB nên vẫn tương thích 100%!).
- **`ABUSE-09`:** Trong `backend/src/Swms.Api/Features/SwmsEndpoints.cs` (khoảng dòng 4150), ở đầu nhánh `essay` của `POST /api/v1/practice-quizzes/{quizId}/items/{itemId}/submit`: nếu `item.EvaluatedAt.HasValue && userId != SeedDataInitializer.DefaultUserId`, trả về `Results.Conflict(new { error = "Câu hỏi tự luận này đã được nộp và chấm điểm trước đó." })`.

### 11. `ABUSE-10`: Kiểm tra `baseVersion` hợp lệ trong Notebook OCC & Thêm `DELETE /api/v1/notebooks/{id}`
- Trong `PUT /api/v1/notebooks/{id:guid}`: nếu `req.BaseVersion.HasValue && (req.BaseVersion.Value < 1 || req.BaseVersion.Value > notebook.Version)`, trả ngay `Results.BadRequest(new { error = "INVALID_BASE_VERSION", message = "baseVersion không hợp lệ." })` (chỉ tạo bản sao xung đột 409 khi `1 <= req.BaseVersion.Value < notebook.Version`).
- Thêm endpoint `DELETE /api/v1/notebooks/{id:guid}`: kiểm tra xác thực & sở hữu `notebook.UserId == userId` (nếu thuộc user khác trả `403`, không tồn tại trả `404`), xóa các bản ghi `NotebookConflictDrafts` liên quan tới `notebook.Id` (kể cả `ConflictCopyNotebookId`), xóa `notebook`, `SaveChangesAsync()` và trả `200 OK` (`{ deleted = true, id }`).

### 12. `STU-01` / `B6` & Metadata `quizCount`
- Trong `CourseWizardAndImportService.cs` và `CourseGenerationService.cs`: trước khi ghép tiền tố `$"Chương {idx}: ..."`, dùng Regex xóa tiền tố `^(?:Chương|Chapter|Phần)\s*\d+\s*[:.\-–—]\s*` có sẵn trong tiêu đề để không bao giờ sinh ra `"Chương 1: Chương 1: ..."`.
- Trên `GET /api/v1/courses/{id:guid}`, bổ sung trường `quizCount` (số bài quiz/câu hỏi gắn với khóa học hoặc tài liệu của khóa học) vào payload trả về để frontend biết chính xác khi khóa học có `0` câu hỏi.

## Kiểm chứng bắt buộc trước khi kết thúc
1. Chạy `dotnet test backend/tests/Swms.Tests/Swms.Tests.csproj` (Debug, **KHÔNG** dùng `-c Release`) → bắt buộc **80/80 pass, exit code 0**.
2. Kiểm tra `git status -sb` xem chỉ sửa file trong `backend/` và `docs/qa-round2/A-fix-backend-summary.md`. KHÔNG commit git, KHÔNG restart server.
3. Ghi tóm tắt rõ ràng từng hạng mục đã sửa, file/dòng và kết quả chạy `dotnet test` (kèm mục **Chưa kiểm chứng**) vào `docs/qa-round2/A-fix-backend-summary.md`.
