# BÁO CÁO KIỂM THỬ A-ABUSER: ĐÓNG VAI NGƯỜI DÙNG CỐ TÌNH LẠM DỤNG LỖI ĐỂ TRỤC LỢI
**Dự án:** Studxy (SWMS - Study Workspace Management System)  
**Môi trường:** `Windows`, `PowerShell`, `Branch: qa/agy2-round2`  
**Backend:** .NET 10 LTS (`http://127.0.0.1:5080`)  
**Frontend:** Next.js 15 App Router (`http://127.0.0.1:3000`)  
**Vai trò:** Kẻ gian trục lợi, săn tìm kẽ hở nghiệp vụ (Quota AI, FSRS, Streak, Quiz, OCC, Input Injection)  
**Thời gian thực hiện:** 18/11/2026  

---

## 1. KẾT QUẢ CHÍNH (EXECUTIVE SUMMARY)

Đóng vai một người dùng tham lam muốn tối đa hóa lợi ích cá nhân, chiếm dụng tài nguyên AI miễn phí, làm đẹp hồ sơ học tập và gian lận điểm số, kiểm thử viên đã thực hiện rà soát chuyên sâu 6 phân hệ cốt lõi qua API backend và phát hiện **11 vấn đề bảo mật và logic nghiệp vụ nghiêm trọng**:

1. **Quota AI bị lách hoàn toàn (Bypass 100%):** Kẻ gian có thể gọi trực tiếp endpoint `POST /api/v1/documents/upload` và `POST /api/v1/documents/{id}/generate-course` để tạo hàng loạt khóa học đầy đủ chương mục, bài học và quiz từ tài liệu mà **không bị trừ bất kỳ AI credit nào** (`aiCreditsUsed` vẫn giữ nguyên 0/50).
2. **Lạm dụng đăng ký hàng loạt (Account Farming):** Hệ thống không có Rate Limiting theo IP và không có CAPTCHA, cho phép script tự động tạo 10 tài khoản chỉ trong **144ms** (tương đương ~70 tài khoản/giây). Mỗi tài khoản lập tức được cấp 50 AI credits và 50 MB lưu trữ, tạo ra nguồn tài nguyên AI vô tận cho kẻ gian.
3. **Bất công nghiêm trọng trong trừ credit AI:** Khi AI upstream proxy gặp sự cố, backend tự động chuyển sang câu trả lời mẫu (deterministic fallback), nhưng **vẫn trừ nguyên 1 AI credit và 240 tokens** của người dùng mà không hề hoàn tiền (no refund).
4. **Tấn công từ chối dịch vụ chéo (Cross-User DoS) qua ReviewEventId FSRS:** Logic kiểm tra Idempotency của Flashcard Review bị lỗi thiếu kiểm tra `UserId`. Kẻ gian có thể gửi trước một `reviewEventId` ngẫu nhiên; nếu người dùng khác gửi trùng ID này, lượt ôn tập của nạn nhân bị server **lẳng lặng nuốt chửng** (`idempotentDuplicate = true`), làm sai lệch lộ trình ôn tập của nạn nhân.
5. **Cày cấp FSRS siêu tốc (Fake 30-Day History in 1 Batch):** Gửi 25 lượt ôn tập lùi về quá khứ trong 1 batch duy nhất. Server chấp nhận toàn bộ và chạy thuật toán Replay, đẩy Stability từ 0 lên **234.4 ngày**, tăng 25 reps và kích hoạt tài khoản `user.IsActivated = true` chỉ trong 1 request.
6. **Xóa sạch lịch sử trượt thẻ (Infinite Undo):** Học viên có thể hoàn tác vô hạn số lần bằng `POST /flashcards/{id}/undo-last-review`. Khi trả lời sai (Again), học viên lập tức bấm Hoàn tác rồi chọn Easy, xóa sạch số lần quên (`lapses = 0`) để duy trì tỷ lệ nhớ 100% ảo.
7. **Hoàn thành bài học không cần đọc (Fast Lesson Farming):** Đánh dấu hoàn thành 5 bài học trong **18ms** (trung bình 3-4ms/bài) mà không cần thời gian đọc tối thiểu. Đồng thời có thể hoàn thành bài học của bất kỳ khóa học công khai nào mà không cần đăng ký (enroll).
8. **Gian lận Quiz trắc nghiệm 100% nhờ quy luật đáp án cố định:** Tất cả câu hỏi trắc nghiệm tự động sinh từ tài liệu đều tuân theo quy luật toán học cố định: **Câu 1 luôn là A, Câu 2 luôn là B, Câu 3 luôn là C**, kèm theo các đáp án nhiễu theo khuôn mẫu dễ dàng nhận biết bằng mắt thường.
9. **Nộp bài tự luận nhiều lần (Essay Re-submission):** Câu hỏi tự luận không bị chặn khi đã chấm xong, cho phép nộp lại nhiều lần để ghi đè điểm hoặc spam background worker làm cạn kiệt tài nguyên.
10. **Làm phình cơ sở dữ liệu vĩnh viễn (Permanent DB Bloat) qua Notebook OCC:** Bằng cách gửi `baseVersion` sai (-1, 0, 9999), kẻ gian ép server tạo hàng ngàn bản ghi `NotebookConflictDraft` và Notebook con riêng biệt. API hoàn toàn **không có endpoint xóa sổ tay** (DELETE trả về 405 Method Not Allowed), khiến rác dữ liệu tồn tại vĩnh viễn.
11. **Hệ thống Streak bị đóng băng (Frozen Streak):** Cột `StreakDays` trong database là tĩnh, không bao giờ tự động tăng khi học bài hay ôn tập thẻ, gây ức chế và thôi thúc người dùng tìm cách can thiệp dữ liệu.

---

## 2. BẰNG CHỨNG THỰC NGHIỆM (REPRODUCIBLE EVIDENCE)

Toàn bộ các bài kiểm thử khai thác đã được viết thành các kịch bản PoC độc lập đặt tại [docs/qa-round2/scripts/a-abuser/](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/), có thể chạy lại bất kỳ lúc nào với trình điều phối trung tâm:

### Lệnh thực thi toàn bộ kịch bản PoC:
```powershell
node docs/qa-round2/scripts/a-abuser/run-all-pocs.js
```
- **Exit Code:** `0` (Tất cả 6 PoC chạy thành công và tạo bằng chứng đầy đủ)
- **Tổng thời gian thực thi:** ~35.2 giây (do PoC-04 thực hiện chấm điểm tự luận AI thực tế qua background job)

### Bảng kết quả chạy tổng thể:
| Index | Script PoC | Exit Code | Kết quả | Thời gian | Phân hệ kiểm thử |
| :---: | :--- | :---: | :---: | :---: | :--- |
| **0** | `poc-01-quota-abuse.js` | 0 | PASSED | 533ms | Quota AI, Mass Registration, Course Gen Bypass, Fallback Charging |
| **1** | `poc-02-fsrs-abuse.js` | 0 | PASSED | 465ms | FSRS Clock Skew, Fake History Batch, Cross-User Idempotency, Undo Exploit |
| **2** | `poc-03-streak-progress-abuse.js` | 0 | PASSED | 1027ms | Bulk Lesson Complete, Unenrolled Public Course, IDOR Check, Frozen Streak |
| **3** | `poc-04-quiz-cheat.js` | 0 | PASSED | 32527ms | Deterministic MCQ Answer Pattern (A/B/C), Essay Re-submission, IDOR |
| **4** | `poc-05-notebook-occ-abuse.js` | 0 | PASSED | 450ms | Wrong BaseVersion, 20 Conflict Drafts Limit, Irreversible DB Bloat |
| **5** | `poc-06-extreme-inputs.js` | 0 | PASSED | 185ms | Deeply Nested JSON, Null Bytes `\u0000`, Numeric Overflow, Empty/Bad Payloads |

---

## 3. DANH SÁCH CHI TIẾT CÁC LỖI & KẼ HỞ NGHIỆP VỤ

### VẤN ĐỀ 1: Bỏ qua Quota AI hoàn toàn khi tạo khóa học từ tài liệu
- **ID:** `ABUSE-01`
- **Mức độ:** `Critical`
- **PoC Script:** [poc-01-quota-abuse.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-01-quota-abuse.js) (Test 1.2)
- **Lợi ích kẻ gian thu được:** Dùng AI miễn phí không giới hạn. Tạo vô số khóa học hoàn chỉnh từ tài liệu mà không tốn bất kỳ credit nào trong hạn mức 50 credits/ngày của gói Free.
- **Bước tái hiện:**
  1. Đăng ký tài khoản mới và kiểm tra quota ban đầu: `aiCreditsUsed = 0/50`.
  2. Tải lên tệp tài liệu qua `POST /api/v1/documents/upload`.
  3. Gọi trực tiếp `POST /api/v1/documents/{docId}/generate-course`.
  4. Kiểm tra lại quota: `aiCreditsUsed` vẫn là `0/50`.
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Quá trình bóc tách bài học và tạo quiz bằng AI phải kiểm tra hạn mức và trừ tương ứng (ví dụ 5–10 credits).
  - *Thực tế:* Server tạo thành công khóa học, chương mục, bài học và quiz mà không hề ghi nhận bất kỳ dòng nào vào `AiUsageLedger`.
- **File & dòng nghi ngờ:** [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L2770-L2797) và [CourseGenerationService.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/CourseGenerationService.cs#L37-L290).
- **Đề xuất fix:** Tích hợp `CheckAiQuotaAvailableAsync(db, userId, requiredCredits: X)` và thêm bản ghi `AiUsageLedger` trước khi xử lý tạo khóa học.

---

### VẤN ĐỀ 2: Thu tiền / Trừ credit khi AI Upstream lỗi hoặc trả kết quả Fallback
- **ID:** `ABUSE-02`
- **Mức độ:** `Critical`
- **PoC Script:** [poc-01-quota-abuse.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-01-quota-abuse.js) (Test 1.3)
- **Lợi ích kẻ gian / Tác hại:** Học viên bị trừ oan hạn mức AI cho câu trả lời định sẵn. Kẻ gian có thể cố tình ép gọi fallback để lấy dữ liệu mẫu mà không làm tốn chi phí token của server upstream.
- **Bước tái hiện:**
  1. Gọi `POST /api/v1/ai/explain` với header chỉ định upstream giả lập chết: `X-Ai-Base-Url: http://127.0.0.1:9999`, `X-Ai-Api-Key: dummy`.
  2. Quan sát server trả về 200 OK với `model: "gemini-3.8-flash-fallback"`, `usedLiveEndpoint: false`, `creditsCharged: 1`.
  3. Kiểm tra quota: `aiCreditsUsed` tăng thêm 1 credit và `aiTokensUsed` tăng 240 tokens.
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Khi không kết nối được AI live, hệ thống phải trả fallback miễn phí (0 credit) hoặc hoàn lại credit cho học viên.
  - *Thực tế:* Bản ghi `AiUsageLedger` bị lưu cứng vào DB trước khi gọi model; khi fallback xảy ra, credit vẫn bị trừ vĩnh viễn.
- **File & dòng nghi ngờ:** [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L4324-L4342) và [GeminiProxyClient.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Infrastructure/Ai/GeminiProxyClient.cs#L126-L141).
- **Đề xuất fix:** Chỉ trừ credit khi `res.UsedLiveEndpoint == true`. Nếu rơi vào fallback, cập nhật `CreditsCharged = 0`.

---

### VẤN ĐỀ 3: Đăng ký tài khoản ồ ạt không giới hạn (Account Farming)
- **ID:** `ABUSE-03`
- **Mức độ:** `High`
- **PoC Script:** [poc-01-quota-abuse.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-01-quota-abuse.js) (Test 1.1)
- **Lợi ích kẻ gian thu được:** Tạo hàng ngàn tài khoản tự động mà không gặp bất kỳ rào cản nào (không CAPTCHA, không giới hạn IP, không cần kích hoạt email). Tận dụng mỗi tài khoản 50 AI credits để gom tài nguyên tính toán AI.
- **Bước tái hiện:**
  1. Chạy vòng lặp gửi 10 request đồng thời tới `POST /api/v1/auth/register`.
  2. Cả 10 tài khoản đều tạo thành công trong 144ms, nhận đầy đủ Token và Workspace riêng.
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Phải có IP Rate Limiting (chặn sau 3-5 lần thử) và CAPTCHA bảo vệ.
  - *Thực tế:* Không có bất kỳ middleware hạn chế tần suất nào trên route `/auth/register`.
- **File & dòng nghi ngờ:** [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L373-L435).
- **Đề xuất fix:** Bật ASP.NET Core RateLimiter trên `/auth/register` (ví dụ `TokenBucket` 5 request/phút theo IP Remote Address).

---

### VẤN ĐỀ 4: Tấn công từ chối dịch vụ chéo người dùng qua ReviewEventId Idempotency
- **ID:** `ABUSE-04`
- **Mức độ:** `Critical`
- **PoC Script:** [poc-02-fsrs-abuse.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-02-fsrs-abuse.js) (Test 2.3)
- **Lợi ích kẻ gian thu được:** Phá hoại việc học tập của người khác. Kẻ gian dùng trước một `ReviewEventId` để khiến lượt ôn tập sau đó của nạn nhân bị server từ chối ghi nhận mà không hề báo lỗi.
- **Bước tái hiện:**
  1. User 1 gửi review với `reviewEventId = "UUID_X"`. Server lưu review vào DB.
  2. User 2 gửi review cho thẻ của mình, nhưng mang cùng `reviewEventId = "UUID_X"`.
  3. Server phản hồi: `status: 200`, `idempotentDuplicate: true` và hoàn toàn không lưu kết quả ôn tập của User 2!
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Idempotency phải được phân tách độc lập theo từng `UserId` (`ReviewEventId` + `UserId`).
  - *Thực tế:* Truy vấn `db.FlashcardReviews.FirstOrDefaultAsync(r => r.ReviewEventId == ev.ReviewEventId)` không kèm điều kiện `r.UserId == userId`.
- **File & dòng nghi ngờ:** [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L3788-L3790).
- **Đề xuất fix:** Đổi câu lệnh thành:
  ```csharp
  var existingReview = await db.FlashcardReviews
      .FirstOrDefaultAsync(r => r.ReviewEventId == ev.ReviewEventId && r.UserId == userId);
  ```

---

### VẤN ĐỀ 5: Offline Replay giả mạo lịch sử để cày chỉ số thẻ FSRS và kích hoạt tài khoản
- **ID:** `ABUSE-05`
- **Mức độ:** `High`
- **PoC Script:** [poc-02-fsrs-abuse.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-02-fsrs-abuse.js) (Test 2.2)
- **Lợi ích kẻ gian thu được:** Chỉ với 1 request gửi 25 lượt review lùi ngày trong quá khứ, thẻ mới tạo lập tức đạt Stability 234.4 ngày, reps = 25, ngày đến hạn dời sang năm sau, và tài khoản tự động được gắn cờ `isActivated = true` mà không phải học ngày nào.
- **Bước tái hiện:**
  1. Tạo một thẻ mới (reps = 0, stability = 0).
  2. Tạo mảng 25 review có `clientTimestamp` cách đều 1 ngày trong 25 ngày qua.
  3. Gửi `POST /api/v1/flashcards/sync-reviews`.
  4. Server chấp nhận toàn bộ, trả về `syncedCount: 25`, `userActivated: true`.
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Hệ thống phải có ngưỡng phát hiện bất thường khi một thẻ mới nhận hàng chục review ngoại tuyến cùng lúc trong quá khứ.
  - *Thực tế:* Thuật toán ReplayChronologically tính toán toàn bộ và ghi nhận lịch sử giả mạo.
- **File & dòng nghi ngờ:** [FsrsV5Engine.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Infrastructure/FSRS/FsrsV5Engine.cs#L58-L75) và [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L3813-L3852).
- **Đề xuất fix:** Giới hạn tối đa số lượt review ngoại tuyến trong 1 ngày cho mỗi thẻ, và yêu cầu khoảng thời gian tối thiểu hợp lý giữa các lần ôn tập.

---

### VẤN ĐỀ 6: Hoàn tác vô hạn (Infinite Undo) xóa sạch lỗi sai để giữ 100% Retention ảo
- **ID:** `ABUSE-06`
- **Mức độ:** `Medium`
- **PoC Script:** [poc-02-fsrs-abuse.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-02-fsrs-abuse.js) (Test 2.5)
- **Lợi ích kẻ gian thu được:** Làm giả hồ sơ học tập hoàn hảo. Mỗi khi bấm nhầm hoặc quên kiến thức (đánh giá 1 - Again), người dùng chỉ cần bấm Undo để xóa lượt đó khỏi database, rồi chọn 4 (Easy) để giữ Stability cao nhất và `lapses = 0`.
- **Bước tái hiện:**
  1. Review thẻ với Rating 1 (Again).
  2. Gọi `POST /api/v1/flashcards/{id}/undo-last-review`.
  3. Review lại thẻ với Rating 4 (Easy).
  4. Kiểm tra lại thẻ: `lapses = 0`, `stability = 15.47` (như chưa từng bị quên).
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Undo chỉ được phép thực hiện trong thời gian ngắn (ví dụ < 60 giây) và chỉ cho phép undo 1 lần liên tiếp.
  - *Thực tế:* Server cho phép gọi undo liên tục cho đến khi xóa hết toàn bộ lịch sử ôn tập của thẻ.
- **File & dòng nghi ngờ:** [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L3660-L3746).
- **Đề xuất fix:** Giới hạn thời gian hoàn tác trong 60 giây kể từ `ReviewedAt` của lượt gần nhất, và ghi nhận audit log thay vì xóa hẳn dòng review trong DB.

---

### VẤN ĐỀ 7: Hoàn thành bài học thần tốc không cần đọc và hoàn thành bài của khóa chưa đăng ký
- **ID:** `ABUSE-07`
- **Mức độ:** `High`
- **PoC Script:** [poc-03-streak-progress-abuse.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-03-streak-progress-abuse.js) (Test 3.1 & 3.2)
- **Lợi ích kẻ gian thu được:** Cày tiến độ 100% tất cả các khóa học trong vài giây để nhận chứng chỉ hoặc khoe thành tích. Có thể đánh dấu hoàn thành bài học của các khóa học công khai mà không cần đăng ký tham gia khóa học.
- **Bước tái hiện:**
  1. Gửi liên tiếp 5 request `POST /api/v1/lessons/{id}/complete`.
  2. Cả 5 bài học được đánh dấu hoàn thành chỉ trong **18ms** (không hề có độ trễ đọc bài).
  3. Lấy một khóa học công khai bất kỳ mà người dùng chưa hề ghi danh (enrolled: false), gửi `POST /lessons/{lessonId}/complete`.
  4. Server trả về 200 OK với thông điệp `"Đã hoàn thành bài học."`.
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Người dùng phải đăng ký khóa học trước khi lưu tiến độ bài học, và hệ thống nên yêu cầu thời gian đọc bài tối thiểu (heartbeat hoặc reading duration).
  - *Thực tế:* Hàm `CanReadCourseAsync` cho phép bất kỳ ai đọc được khóa học công khai đều có quyền ghi nhận `user_lesson_progress`.
- **File & dòng nghi ngờ:** [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L1830-L1899) và [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L315-L320).
- **Đề xuất fix:** Yêu cầu bản ghi `CourseEnrollments` hợp lệ trước khi cho phép complete bài học.

---

### VẤN ĐỀ 8: Quy luật đáp án trắc nghiệm cố định (Q1=A, Q2=B, Q3=C) trong khóa học sinh tự động
- **ID:** `ABUSE-08`
- **Mức độ:** `Critical`
- **PoC Script:** [poc-04-quiz-cheat.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-04-quiz-cheat.js) (Test 4.1)
- **Lợi ích kẻ gian thu được:** Đạt 100% điểm quiz trắc nghiệm trong mọi khóa học tạo từ tài liệu mà không cần đọc nội dung câu hỏi.
- **Bước tái hiện:**
  1. Tạo khóa học từ tài liệu.
  2. Lấy quiz thực hành tương ứng.
  3. Nộp đáp án Câu 1 là "A", Câu 2 là "B", Câu 3 là "C".
  4. Server trả về `isCorrect: true` cho cả 3 câu với tỷ lệ chính xác 100%.
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Vị trí đáp án đúng phải được xáo trộn ngẫu nhiên (random shuffle).
  - *Thực tế:* Vị trí đáp án đúng được gán cứng theo chỉ số vòng lặp:
    ```csharp
    var solutionKeys = new[] { "A", "B", "C" };
    for (int i = 0; i < 3; i++) {
        var correctKey = solutionKeys[i % solutionKeys.Length]; // i=0 -> A, i=1 -> B, i=2 -> C
    ```
- **File & dòng nghi ngờ:** [CourseGenerationService.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/CourseGenerationService.cs#L1159-L1181).
- **Đề xuất fix:** Sử dụng hàm xáo trộn danh sách đáp án ngẫu nhiên cho từng câu hỏi trước khi lưu vào DB.

---

### VẤN ĐỀ 9: Nộp bài tự luận nhiều lần để ghi đè điểm số và spam tài nguyên AI
- **ID:** `ABUSE-09`
- **Mức độ:** `High`
- **PoC Script:** [poc-04-quiz-cheat.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-04-quiz-cheat.js) (Test 4.2)
- **Lợi ích kẻ gian thu được:** Nộp lại nhiều lần để ép AI chấm lại cho đến khi nhận được nhận xét ưng ý, hoặc spam background worker làm cạn kiệt tài nguyên xử lý của hệ thống.
- **Bước tái hiện:**
  1. Nộp bài tự luận qua `POST /practice-quizzes/{quizId}/items/{itemId}/submit`. Chờ job hoàn thành (status 202).
  2. Tiếp tục gửi request nộp lần 2 trên cùng `itemId` đó.
  3. Server tiếp tục chấp nhận với status 202, tạo job chấm điểm mới và ghi đè câu trả lời cũ thay vì trả về 409 Conflict.
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Tương tự như câu hỏi trắc nghiệm, câu hỏi đã được nộp và chấm điểm (`item.EvaluatedAt.HasValue`) phải trả về lỗi `409 Conflict: "Câu hỏi này đã được nộp và chấm điểm trước đó."`.
  - *Thực tế:* Chỉ có `multiple_choice`, `cloze`, `short_answer` là có kiểm tra `EvaluatedAt`; nhánh `essay` bị bỏ sót hoàn toàn.
- **File & dòng nghi ngờ:** [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L4150-L4175).
- **Đề xuất fix:** Bổ sung điều kiện kiểm tra `if (item.EvaluatedAt.HasValue) return Results.Conflict(...)` ở đầu nhánh xử lý essay.

---

### VẤN ĐỀ 10: Gây phình cơ sở dữ liệu vĩnh viễn thông qua xung đột Notebook OCC
- **ID:** `ABUSE-10`
- **Mức độ:** `High`
- **PoC Script:** [poc-05-notebook-occ-abuse.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-05-notebook-occ-abuse.js) (Test 5.1, 5.2, 5.3)
- **Lợi ích kẻ gian thu được:** Gây tràn ổ cứng server và phình database SQLite/Postgres. Do không có endpoint xóa sổ tay, dữ liệu rác không thể bị dọn dẹp bởi người dùng.
- **Bước tái hiện:**
  1. Tạo 1 sổ tay ban đầu (`version = 1`).
  2. Gửi request cập nhật với `baseVersion = -1` hoặc `9999`.
  3. Server phản hồi 409 Conflict theo chiến lược "Keep Both", tự động sinh ra một Notebook mới trong bảng `Notebooks` và một bản ghi trong `NotebookConflictDrafts`.
  4. Lặp lại 20 lần: server tạo ra 20 sổ tay con. Đến lần 21 mới chặn với `CONFLICT_DRAFT_LIMIT_REACHED`.
  5. Thử gọi `DELETE /api/v1/notebooks/{id}` để xóa sổ tay rác: server trả về `405 Method Not Allowed` vì API không có route DELETE.
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Cần có cơ chế xác thực `baseVersion` hợp lệ, giới hạn tổng dung lượng sổ tay vào quota lưu trữ người dùng, và cung cấp API xóa sổ tay.
  - *Thực tế:* Mọi `baseVersion` bất thường đều sinh sổ tay con và không thể xóa bỏ.
- **File & dòng nghi ngờ:** [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L3166-L3235).
- **Đề xuất fix:** Cung cấp endpoint `DELETE /api/v1/notebooks/{id}`, tự động dọn dẹp các bản nháp xung đột cũ hơn 30 ngày, và tính dung lượng văn bản của sổ tay vào `UserQuota.CurrentStorageBytes`.

---

### VẤN ĐỀ 11: Cột StreakDays bị đông cứng hoàn toàn (Frozen Streak)
- **ID:** `ABUSE-11`
- **Mức độ:** `Medium`
- **PoC Script:** [poc-03-streak-progress-abuse.js](file:///d:/03_Study/Studxy/docs/qa-round2/scripts/a-abuser/poc-03-streak-progress-abuse.js) (Test 3.4)
- **Lợi ích kẻ gian / Tác hại:** Làm mất giá trị của tính năng gamification. Người dùng học thật hay học giả đều có chung 1 ngày streak (hoặc 7 ngày nếu dùng tài khoản demo).
- **Bước tái hiện:**
  1. Kiểm tra tài khoản mới: `streakDays = 1`.
  2. Hoàn thành 10 bài học, ôn tập 25 thẻ FSRS, hoàn thành onboarding.
  3. Kiểm tra lại `/api/v1/auth/me`: `streakDays` vẫn luôn là `1`.
- **Mong đợi vs Thực tế:**
  - *Mong đợi:* Mỗi ngày người dùng có hoạt động học tập (ôn thẻ hoặc học bài), `StreakDays` phải tăng thêm 1; nếu bỏ lỡ 1 ngày, streak phải reset về 1.
  - *Thực tế:* Giá trị `StreakDays` trong database là trường tĩnh, hoàn toàn không có bất kỳ logic nào trong backend cập nhật trường này.
- **File & dòng nghi ngờ:** [SwmsEndpoints.cs](file:///d:/03_Study/Studxy/backend/src/Swms.Api/Features/SwmsEndpoints.cs#L407, #L558, #L961, #L1011).
- **Đề xuất fix:** Tạo bảng `UserDailyActivities (UserId, ActivityDate)` và triển khai hàm tính streak thực tế dựa trên các ngày hoạt động liên tiếp theo múi giờ client.

---

## 4. MỤC CHƯA KIỂM CHỨNG (UNVERIFIED)

Mặc dù đã hoàn thành trọn vẹn các kịch bản kiểm thử lạm dụng theo yêu cầu, một số khía cạnh sau đây chưa thể kiểm chứng toàn diện do giới hạn môi trường:
1. **Race condition vượt quota khi có hàng trăm request đồng thời thực sự từ nhiều worker phân tán:** Hiện tại server chạy cục bộ trên 1 máy dev với `SemaphoreSlim` in-memory. Nếu backend được scale ra nhiều instance (multi-node / horizontal scaling) phía sau load balancer mà không dùng Distributed Lock (Redis Redlock), `AiQuotaLocks` và `FlashcardSyncLocks` dạng in-memory `ConcurrentDictionary` chắc chắn sẽ bị race condition xuyên server.
2. **Khai thác lỗ hổng OCR khi tải lên tệp PDF cực lớn hoặc chứa mã độc nhúng:** Do tuân thủ nguyên tắc không làm sập server local dev, kiểm thử viên chưa thử tải lên file PDF kích thước hàng trăm megabyte chứa hàng ngàn trang hoặc tệp nén zip bom (`.swm.zip` giải nén đệ quy).
3. **Thanh toán và nâng cấp gói cước (Billing / Stripe Integration):** Hiện tại hệ thống chưa tích hợp cổng thanh toán thực tế cho các gói Pro / Institution; mọi kiểm tra chỉ dừng lại ở gói Free mặc định.

---

## 5. GÓP Ý NHƯ NGƯỜI DÙNG (USER PERSPECTIVE FEEDBACK)

Đứng ở góc độ một người học thực thụ trên nền tảng Studxy, việc một học viên có xu hướng muốn "gian lận", "lách luật" hoặc "hack điểm" thường bắt nguồn từ những bất cập trong thiết kế trải nghiệm người dùng và cơ chế đánh giá:

### 1. Điều gì khiến người học muốn gian lận?
- **Sự khắt khe và bất công của thuật toán Spaced Repetition:** Khi một học viên ôn tập hàng chục thẻ, chỉ cần lỡ tay bấm nhầm nút "Again" (hoặc do giao diện phím tắt quá nhạy), toàn bộ chỉ số Stability tích lũy suốt nhiều tháng của thẻ bị tụt dốc thảm hại. Vì sợ mất thành quả, người học bắt buộc phải tìm cách Hoàn tác (Undo) hoặc dùng mẹo để cứu vãn.
- **Hạn mức AI 50 credits/ngày quá eo hẹp:** Trong một buổi học tập chuyên sâu kéo dài 2-3 tiếng, việc tra cứu giải thích khái niệm (1 credit) và chấm bài tự luận (2 credits) sẽ làm cạn sạch 50 credits chỉ sau vài chục phút. Khi bị chặn ngang mạch học với thông báo 429, tâm lý bức bối sẽ thôi thúc người học tìm cách tạo nhiều tài khoản hoặc gọi trực tiếp API để lách quota.
- **Streak bị đóng băng và vô nghĩa:** Khi người học nỗ lực học bài liên tục nhiều ngày mà con số Streak vẫn nằm bất động ở số 1, họ cảm thấy sự cố gắng của mình không được hệ thống tôn trọng. Điều này tạo tâm lý buông xuôi và biến các chỉ số thành trò cười.

### 2. Gợi ý thiết kế công bằng và nhân văn hơn:
- **Cơ chế "Bảo vệ Streak" (Streak Freeze) và "Học bù" (Grace Period):** Cuộc sống sinh viên có những ngày thi cử hoặc ốm đau không thể mở app. Cho phép tích lũy tối đa 2 "Băng bảo vệ streak" mỗi tháng để học viên không bị áp lực đứt chuỗi một cách oan uổng.
- **Nút "Chấm nhầm" (Fat-Finger Grace Window) hợp thức hóa:** Thay vì để tính năng Undo bị lạm dụng không giới hạn, hãy hiển thị nút "Tôi bấm nhầm" chỉ trong 15 giây đầu tiên sau khi đánh giá thẻ, có đếm ngược rõ ràng.
- **Hạn mức AI linh hoạt theo tiến độ (Earnable AI Credits):** Thay vì cố định cứng 50 credits, hãy cho phép học viên kiếm thêm AI credits bằng cách hoàn thành bài học, đạt chuỗi ôn tập thẻ đúng hạn, hoặc đóng góp ghi chú chất lượng. Điều này biến việc học chăm chỉ thành động lực thay vì đi tìm kẽ hở.
- **Xáo trộn trắc nghiệm thông minh và giải thích đáp án:** Các câu hỏi trắc nghiệm tự sinh cần được đảo thứ tự A/B/C/D ngẫu nhiên và giải thích rõ vì sao đáp án đó đúng, thay vì dùng các đáp án nhiễu khuôn mẫu dễ đoán.

### 3. Tính năng người học thực sự khao khát:
- **Chế độ học nhóm (Study Squads):** Cùng bạn bè thi đua streak và làm quiz nhóm với bảng xếp hạng công bằng, có cơ chế chống gian lận tập thể.
- **Chế độ thi thử có đồng hồ bấm giờ (Exam Simulation Mode):** Khóa chức năng xem lại đáp án và khóa hoàn tác trong suốt buổi làm bài thi để rèn luyện tâm lý phòng thi thật.
- **Tùy chỉnh độ sâu giải thích AI (Adaptive AI Depth):** Cho phép chọn giải thích nhanh 1 câu (tốn 0.2 credit) hoặc giải thích sâu đa chiều kèm ví dụ trực quan (tốn 1 credit) để tiết kiệm hạn mức tối đa cho học viên.
