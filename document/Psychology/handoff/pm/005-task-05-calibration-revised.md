# Handoff 005 — Task 5: Hiệu chỉnh sau tuần mẫu (Bản sửa đổi theo phản hồi người học tại Checkpoint 3)

> **TÀI LIỆU QUẢN TRỊ BẮT BUỘC — THAY THẾ HANDOFF 004**  
> **Khóa học:** Tự học Tâm lý học 48 tuần (Psychology Curriculum)  
> **Chặng 1:** Nền tảng khoa học (Tuần 01 – Tuần 08)  
> **Giai đoạn:** Task 5 — Hiệu chỉnh sau tuần mẫu (Post-Sample Calibration)  
> **Ngày ban hành:** 2026-09-09  
> **Vai trò ban hành:** Reviewer / Product Manager (PM) khóa học  
> **Người thực thi tiếp nhận:** Agent Thực thi (Implementer)  
> **Tình trạng Handoff 004:** Được lưu trữ nguyên trạng làm hồ sơ lịch sử bất biến theo quy tắc **Strict Deletion Ban**; Handoff 005 này là bản đặc tả có hiệu lực thi hành duy nhất cho Task 5.

---

## 1. Vai trò và Tôn chỉ Thực hiện

Bạn là **agent thực thi** của khóa tự học Tâm lý học 48 tuần. Bạn tiếp nhận nhiệm vụ thực hiện **Task 5 — Hiệu chỉnh sau tuần mẫu** theo đặc tả sửa đổi toàn diện trong tài liệu này. 

Reviewer độc lập sẽ thẩm định và chấm điểm chất lượng sau khi bạn nộp gói bàn giao. Bạn **không tự tuyên bố APPROVED** và **tuyệt đối không viết Tuần 02 (W02)** trong bất kỳ trường hợp nào.

---

## 2. Bối cảnh & Lý do Thay thế Handoff 004

1. **Kết quả Vòng Review Task 4:**
   - Reviewer khoa học độc lập đã ra verdict **APPROVED** đối với bài đọc Tuần 01 (`notes/01-scientific-foundations/week-01-what-is-psychology.md`): toàn bộ 18 luận điểm khoa học có citation và ledger đối chiếu đầy đủ, không vi phạm an toàn, đáp ứng chuẩn mực sư phạm.
   - Nội dung bài học Tuần 01 đã đạt chuẩn và được người học duyệt về chất lượng kiến thức.

2. **Phản hồi Người học tại Checkpoint 3 (CHANGES REQUESTED đối với Quiz):**
   - Người học đã trực tiếp làm thử bài kiểm tra trắc nghiệm Tuần 01 (`assessments/quizzes/week-01-quiz.md`) và đưa ra kết luận **CHANGES REQUESTED** đối với bộ quiz và answer key.
   - Các điểm nghẽn nghiêm trọng được người học chỉ rõ:
     - **Answer-format cues (Manh mối hình thức):** Người học có thể chọn đúng đáp án mà không cần hiểu sâu bài học, chỉ nhờ nhận diện các manh mối bề mặt: đáp án đúng dài hơn hẳn các phương án khác (`Q01-02`, `Q01-06`, `Q01-09`, `Q01-10`); đáp án đúng là lựa chọn duy nhất có thuật ngữ tiếng Anh trong ngoặc đơn (`Q01-01`, `Q01-03`, `Q01-07`); đáp án đúng là lựa chọn duy nhất dùng ngôn ngữ thận trọng, xác suất trong khi các đáp án sai dùng từ ngữ tuyệt đối hóa, ngô nghê ("chắc chắn 100%", "luôn luôn", "mọi người"); hoặc đáp án đúng lặp lại nguyên từ khóa trong phần dẫn (stem-keyword echo ở `Q01-04`).
     - **Metadata leakage (Lộ siêu dữ liệu trên giao diện người học):** Quiz hiển thị công khai bảng ma trận nhận thức (Assessment Matrix), mã cấp độ Bloom (Nhớ/Hiểu, Áp dụng, Đánh giá), mã mục tiêu `[LO-W01-XX]`, tỷ lệ 40/40/20, và các tiêu đề phân nhóm câu hỏi (`## Phần 1...`, `## Phần 2...`, `## Phần 3...`), làm lộ chiến lược tư duy và gợi ý trực tiếp dạng câu hỏi cho người học.
     - **Distractor quá dễ loại (Weak distractors):** Nhiều phương án sai mang tính cực đoan, ngớ ngẩn hoặc phi thực tế, thiếu tính cạnh tranh nhận thức, không phản ánh các ngụy biện tư duy hoặc hiểu lầm pop psychology phổ biến trong đời sống thực.
     - **Độ phủ khái niệm chưa đủ:** Bài kiểm tra 10 câu cũ bỏ sót nhiều mảng kiến thức cốt lõi quan trọng của Tuần 01 (như bản chất đo lường gián tiếp, mô hình biopsychosocial, Gestalt, khủng hoảng tái lập, thiên lệch WEIRD).
     - **Sao chép cơ học định nghĩa:** Một số câu hỏi lấy nguyên văn câu chữ định nghĩa trong bài học làm phương án đúng thay vì diễn đạt lại (paraphrase) theo tình huống để kiểm tra mức độ thấu hiểu bản chất.
     - **Lỗi ánh xạ trong Báo cáo Audit cũ:** Báo cáo `quality/week-01-audit-report.md` (Mục 6, dòng 127) ghi nhận sai câu `Q01-08` là đo lường mô hình sinh–tâm–xã hội (`C01-12`), trong khi artifact thực tế của câu `Q01-08` trong cả quiz và answer key lại hỏi về Tâm lý học nhân văn (`C01-07`).

3. **Lý do Handoff 004 bị thay thế:**
   - `handoff/pm/004-task-05-calibration.md` được soạn thảo trước khi tiếp nhận phản hồi người học tại Checkpoint 3. Handoff 004 giả định Tuần 01 đã hoàn tất 100%, khóa phạm vi sửa đổi quiz/key, và yêu cầu đánh dấu hoàn thành Tuần 01 trên tracker.
   - Do đó, Handoff 004 không còn phù hợp với thực tế kiểm định chất lượng.
   - Handoff 005 này mở lại phạm vi sửa đổi đối với bộ đánh giá Tuần 01, đồng thời thiết lập các tiêu chuẩn sư phạm nghiêm ngặt để giải quyết triệt để lỗi answer-format cue cho Tuần 01 và toàn bộ 47 tuần tiếp theo.

4. **Nguyên tắc Quản trị Tiến độ:**
   - **Tuần 01 CHƯA ĐƯỢC ĐÁNH DẤU HOÀN THÀNH.** Không được đánh dấu `✓` trên `progress.md` và không gỡ nhãn chờ duyệt trên `learning-roadmap.md` cho đến khi người học trực tiếp duyệt bộ quiz 12 câu mới.
   - **Tuyệt đối không viết Tuần 02 (W02)** trong Task 5.

---

## 3. Nguyên tắc Thực thi Bắt buộc

1. **Strict Deletion Ban (Nghiêm cấm Xóa):**
   - Tuyệt đối không xóa bất kỳ file nào trong workspace. Không xóa hoặc ghi đè `handoff/pm/004-task-05-calibration.md`.
2. **Research-First Principles:**
   - Không bịa đặt nguồn, không trích dẫn tùy tiện. Mọi chỉnh sửa nội dung bài kiểm tra và đáp án phải dựa trên các luận điểm đã được xác minh trong bài đọc Tuần 01 và `quality/source-ledger.md`.
3. **Phục vụ Trải nghiệm Học Thực chất:**
   - Mục đích của trắc nghiệm là rèn luyện tư duy phản biện và khả năng truy hồi kiến thức (retrieval practice). Loại bỏ hoàn toàn các "mẹo làm bài thi" để buộc người học phải tư duy khoa học thực sự.
4. **Bảo toàn Cấu trúc 48 tuần:**
   - Không tự ý thay đổi tên tuần, mã chặng hay mục tiêu tổng thể đã được phê duyệt trong `docs/superpowers/specs/2026-08-28-psychology-curriculum-design.md`.

---

## 4. Thứ tự Đọc Tài liệu Bắt buộc

Trước khi bắt tay vào chỉnh sửa, agent thực thi phải đọc kỹ các tài liệu sau theo đúng thứ tự:

1. `Psychology/AGENTS.md` — Quy tắc thực thi, vai trò và Content Contract.
2. `Psychology/REVIEW_RULES.md` — Quy tắc review, các mức độ lỗi và điều kiện pass.
3. `Psychology/implementation_plan.md` — Trọng tâm phần Task 4, Checkpoint 3, Task 5 và Definition of Done.
4. `Psychology/handoff/pm/004-task-05-calibration.md` — Đọc để nắm ngữ cảnh lịch sử đã bàn giao.
5. `Psychology/notes/01-scientific-foundations/week-01-what-is-psychology.md` — Đọc kỹ toàn bộ 12 section để nắm vững mọi khái niệm khoa học cần đưa vào bài kiểm tra.
6. `Psychology/assessments/quizzes/week-01-quiz.md` và `assessments/answer-keys/week-01-answers.md` — Phân tích các lỗi format cues và metadata leakage hiện tại.
7. `Psychology/quality/week-01-audit-report.md` — Rà soát báo cáo audit cũ, đặc biệt là lỗi mapping câu `Q01-08`.
8. `Psychology/templates/quiz-template.md` và `templates/answer-key-template.md` — Nắm cấu trúc template cần hiệu chỉnh.
9. `Psychology/templates/lesson-template.md` — Đọc phần quy ước slug anchor.
10. `Psychology/quality/content-audit-checklist.md` — Nắm 43 tiêu chí kiểm toán hiện hành.
11. `Psychology/learning-roadmap.md` và `Psychology/progress.md` — Đối chiếu trạng thái hiển thị.

---

## 5. Phạm vi File Được phép Chỉnh sửa và Nghiêm cấm

### 5.1. Danh mục File Được phép Chỉnh sửa

| Đường dẫn File | Mục đích và Phạm vi Chỉnh sửa Cho phép |
| :--- | :--- |
| `assessments/quizzes/week-01-quiz.md` | Đại tu toàn diện: tăng lên 12 câu; xóa sạch metadata, Bloom level, LO tag, ma trận, section labels; interleave câu hỏi; cân bằng độ dài và format; nâng cấp distractor; triệt tiêu answer-format cues; phân bổ đáp án 3A, 3B, 3C, 3D. |
| `assessments/answer-keys/week-01-answers.md` | Đại tu toàn diện: mở rộng thành 12 câu; chuyển toàn bộ metadata vào đây; bổ sung cột Concept Cluster Tag; giải thích sâu sắc lý do đúng và lý do sai của từng phương án; chuẩn hóa heading link anchor Unicode; đối chiếu 100% Claim ID. |
| `templates/quiz-template.md` | Cập nhật chuẩn hóa: loại bỏ metadata leakage trên view người học; đưa hướng dẫn về interleave, chống cues, cân bằng độ dài và chất lượng distractor vào comment quản trị dành cho tác giả. |
| `templates/answer-key-template.md` | Cập nhật chuẩn hóa: bổ sung cột Concept Cluster Tag vào bảng tra nhanh; bổ sung chuẩn phân tích từng distractor sai; đưa quy ước tạo slug anchor Unicode tiếng Việt giữ dấu vào tài liệu. |
| `templates/lesson-template.md` | Chỉ chỉnh sửa duy nhất quy ước tạo heading anchor Unicode tiếng Việt giữ dấu để bảo đảm tính tương thích liên kết nội bộ cho toàn bộ 48 tuần; không sửa đổi cấu trúc 12 section H2. |
| `quality/content-audit-checklist.md` | Bổ sung 8 tiêu chí kiểm toán mới chống Answer-Format Cues (Nhóm CUE-01 đến CUE-08); bổ sung hướng dẫn kiểm toán mù (Blind Cue Audit) và ma trận phủ khái niệm (Concept Coverage Matrix); cập nhật Definition of Done v1.0 và ghi ngày hiệu chỉnh (2026-09-09); làm rõ cách tính từ tiếng Việt (SCC-03). |
| `quality/week-01-audit-report.md` | Cập nhật báo cáo audit cho 12 câu quiz mới; sửa lỗi mapping câu `Q01-08` cũ; bổ sung bảng kết quả kiểm toán mù (Cue Audit); bổ sung bảng ma trận phủ 12 cụm khái niệm; cập nhật bảng tổng kết 43+ tiêu chí. |
| `progress.md` | Cập nhật dòng trạng thái W01: ghi rõ đang trong quá trình hiệu chỉnh bài kiểm tra theo phản hồi người học tại Checkpoint 3; **TUYỆT ĐỐI KHÔNG ĐÁNH DẤU HOÀN THÀNH (`✓`)**, giữ ký hiệu `—` hoặc ghi chú tiến độ. |
| `learning-roadmap.md` | Giữ nguyên W01 ở trạng thái chờ người học duyệt (giữ nhãn `[Sắp biên soạn]` hoặc ghi rõ trạng thái đang hiệu chỉnh); không chuyển sang trạng thái bài học hoàn thành chính thức. |

### 5.2. Nghiêm cấm Tuyệt đối

- **CẤM TẠO BẤT KỲ FILE NÀO THUỘC TUẦN 02 (W02)** (`notes/01-scientific-foundations/week-02-*.md`, `assessments/quizzes/week-02-*.md`, `assessments/answer-keys/week-02-*.md`). Việc bắt đầu Tuần 02 trước khi người học duyệt Tuần 01 là vi phạm nghiêm trọng quy trình Checkpoint.
- **CẤM XÓA HOẶC GHI ĐÈ BẤT KỲ FILE HANDOFF CŨ NÀO** trong thư mục `handoff/pm/`.
- **CẤM CHỈNH SỬA BÀI ĐỌC TUẦN 01** (`notes/01-scientific-foundations/week-01-what-is-psychology.md`). Bài học Tuần 01 đã được Reviewer khoa học APPROVED và người học duyệt tại Checkpoint 3; nội dung bài học là tài liệu bất biến (FROZEN). Mọi liên kết từ Answer Key phải trỏ chính xác đến các heading hiện có của bài học mà không được chỉnh sửa file bài đọc.
- **CẤM TỰ CÔNG BỐ HOÀN THÀNH W01:** Không tự ý đánh dấu `✓` trong `progress.md` hay gỡ nhãn chờ duyệt trên `learning-roadmap.md` trước khi người học chính thức nghiệm thu đợt 2.

---

## 6. Nhiệm vụ Cụ thể của Task 5 Hiệu chỉnh

### 6.1. Đại tu toàn diện Bài kiểm tra Trắc nghiệm Tuần 01 (`week-01-quiz.md`)

Bạn phải tái cấu trúc bài quiz theo đúng 13 yêu cầu sư phạm sau:

1. **Tăng số lượng câu hỏi:** Nâng quy mô bài kiểm tra từ 10 câu lên đúng **12 câu trắc nghiệm 4 lựa chọn (A, B, C, D)**, đánh mã từ `Q01-01` đến `Q01-12`.
2. **Loại bỏ triệt để Metadata Leakage trên Giao diện Người học:**
   - **Xóa bỏ hoàn toàn** bảng ma trận đề thi (Assessment Matrix) ở phần đầu file.
   - **Xóa bỏ hoàn toàn** các tiêu đề phân nhóm câu hỏi lộ chiến lược tư duy như `## Phần 1: Nhớ & Hiểu Kiến thức Cốt lõi (~40%)`, `## Phần 2: Áp dụng...`, `## Phần 3: Đánh giá...`. Thay vào đó, toàn bộ 12 câu hỏi được đặt liên tục dưới một tiêu đề duy nhất: `## Danh sách Câu hỏi Trắc nghiệm`.
   - **Xóa bỏ hoàn toàn** các nhãn Bloom level và mã LO tại tiêu đề từng câu hỏi. Không được ghi `### Câu Q01-01 ([LO-W01-01] — Nhớ/Hiểu)`. Tiêu đề câu hỏi chỉ được phép hiển thị duy nhất: `### Câu Q01-01`.
   - Toàn bộ thông tin Bloom, LO, ma trận được chuyển sang lưu trữ nội bộ tại file Answer Key và Báo cáo Audit.
3. **Đảo trộn Xen kẽ (Interleave) Thứ tự Câu hỏi:**
   - Không xếp câu hỏi theo từng cụm cấp độ nhận thức liên tiếp (không xếp 4 câu nhớ rồi đến 5 câu áp dụng rồi đến 3 câu đánh giá).
   - Đan xen các câu hỏi Nhớ/Hiểu, Áp dụng tình huống và Đánh giá bằng chứng một cách tự nhiên để phản ánh tình huống thực tế, chống thói quen đoán định dạng câu hỏi theo vị trí.
4. **Không Sao chép Cơ học Câu Định nghĩa:**
   - Tuyệt đối không lấy nguyên văn một câu văn định nghĩa trong bài đọc để làm phương án đúng.
   - Phải diễn đạt lại (paraphrase) bằng ngôn ngữ mới, đặt vào ngữ cảnh mới, hoặc mô tả cơ chế vận hành để kiểm tra việc hiểu bản chất khái niệm.
5. **Nâng cấp Chất lượng Distractors (Phương án Nhiễu):**
   - Mỗi phương án sai phải là một quan niệm sai lầm phổ biến (plausible misconception) trong thực tế đời sống, hoặc một lỗi ngụy biện tư duy logic có vẻ hợp lý bề mặt (face validity).
   - Tuyệt đối không dùng các phương án ngớ ngẩn, phi lý, vô nghĩa mang tính "làm nền" khiến người học loại trừ được ngay mà không cần suy nghĩ.
6. **Triệt tiêu Hoàn toàn Manh mối Độ dài (Length Cue):**
   - Không được để đáp án đúng dài vượt trội (hoặc ngắn vượt trội) so với các phương án sai.
   - Độ dài về số từ và số dòng của cả 4 lựa chọn (A, B, C, D) trong cùng một câu phải tương đương nhau (độ chênh lệch ký tự giữa phương án dài nhất và ngắn nhất không vượt quá 20–25%).
   - Nếu cần giải thích một luận điểm phức tạp, hãy viết các phương án sai với cấu trúc câu, mệnh đề phụ và độ dài tương đương.
7. **Cân bằng Thuật ngữ Song ngữ (Bilingual Terminology Cue):**
   - Không được để đáp án đúng là lựa chọn DUY NHẤT có thuật ngữ tiếng Anh trong ngoặc đơn (lỗi đã gặp ở `Q01-01`, `Q01-03`, `Q01-07` cũ).
   - Quy tắc xử lý: Hoặc là đưa thuật ngữ tiếng Anh lên phần dẫn của câu hỏi (stem), hoặc nếu đưa vào phương án thì **cả 4 lựa chọn A, B, C, D đều phải có thuật ngữ tiếng Anh tương ứng**.
8. **Cân bằng Ngôn ngữ Thận trọng (Cautious Language Cue):**
   - Không để xuất hiện mô hình: đáp án đúng dùng từ ngữ thận trọng, khoa học, xác suất ("có xu hướng", "có thể", "trong điều kiện nhất định") trong khi cả 3 đáp án sai đều dùng từ ngữ khẳng định tuyệt đối cực đoan ("luôn luôn", "chắc chắn 100%", "mọi người", "hoàn toàn không bao giờ").
   - Các phương án sai tinh vi cũng phải có vẻ thận trọng, hoặc đưa ra các lập luận sai dựa trên những điều kiện biên nghe có lý.
9. **Chống Lặp Từ khóa từ Đề bài (Stem-Keyword Echo):**
   - Không để từ khóa trọng tâm xuất hiện trong stem chỉ lặp lại duy nhất ở đáp án đúng (ví dụ: stem hỏi về "dự đoán" và chỉ có đáp án đúng lặp lại từ "dự đoán").
   - Các phương án sai cũng phải kết nối tự nhiên với ngữ cảnh của câu hỏi.
10. **Phân bổ Đáp án Đúng Cân bằng Tuyệt đối:**
    - Trong 12 câu hỏi, các chữ cái A, B, C, D phải là đáp án đúng với số lần hoàn toàn bằng nhau: **chính xác 3 câu A, 3 câu B, 3 câu C, và 3 câu D**.
    - Phân bổ rải đều ngẫu nhiên, không tạo chuỗi lặp (như AAA, BBB) và không tạo quy luật hình học dễ đoán.
11. **Tỷ lệ Phân bổ Cấp độ Nhận thức Nội bộ:**
    - Quản lý nội bộ theo tỷ lệ: **4 câu Nhớ/Hiểu (33.3%)**, **5 câu Áp dụng/Phân tích (41.7%)**, và **3 câu Đánh giá Giới hạn Bằng chứng (25.0%)**.
12. **Bảo đảm Độ phủ Toàn diện 12 Cụm Khái niệm Trọng yếu:**
    - 12 câu hỏi phải phân bổ phủ trọn vẹn 12 cụm kiến thức được định nghĩa chi tiết tại Mục 7 tài liệu này.
13. **Giữ nguyên Ghi chú về Tình huống Giả định:**
    - Giữ phần ghi chú đầu file giải thích rõ các kịch bản thực tế (bài báo, khảo sát mạng) là tình huống giả định phục vụ rèn luyện phương pháp luận, không phải dữ kiện nghiên cứu thật.

---

### 6.2. Đại tu toàn diện Đáp án & Hướng dẫn Học lại Tuần 01 (`week-01-answers.md`)

File Answer Key là tài liệu sư phạm trọng tâm dành cho người học sau khi tự làm bài. Cần đại tu toàn diện:

1. **Bảng Tra Cứu Nhanh Mở Rộng 12 Câu:**
   - Thiết kế bảng tra cứu nhanh với đầy đủ các cột:
     `| Câu hỏi | Mã mục tiêu (LO) | Cấp độ nhận thức | Cụm khái niệm (Concept Cluster) | Đáp án | Vị trí bài học cần ôn tập (Link Heading) | Claim ID & Nguồn |`
   - Đảm bảo hiển thị đầy đủ 12 dòng (`Q01-01` đến `Q01-12`).
2. **Cấu trúc Lời giải Chi tiết cho Từng Câu (Bắt buộc 6 thành phần):**
   - **Mã câu hỏi và Đáp án đúng:** Ghi rõ chữ cái (A, B, C, hoặc D) và nội dung tóm tắt.
   - **Vì sao đáp án này đúng:** Giải thích cặn kẽ cơ chế tâm lý, luận điểm khoa học hoặc nguyên tắc phương pháp luận; không giải thích qua loa kiểu "A đúng vì bài học nói thế".
   - **Phân tích vì sao TỪNG phương án khác SAI:** Bắt buộc phân tích chi tiết riêng cho từng lựa chọn sai (❌ Phương án A/B/C/D), chỉ rõ bẫy tư duy trực giác, ngụy biện logic hoặc hiểu lầm pop psychology mà người học hay mắc phải.
   - **Mục tiêu học tập đo lường & Cụm khái niệm:** Ghi rõ mã LO (`[LO-W01-XX]`) và tên cụm khái niệm trong 12 cụm.
   - **Vị trí bài học để ôn tập:** Chứa liên kết Markdown dạng tương đối trỏ chính xác đến heading của bài học Tuần 01, sử dụng định dạng anchor Unicode giữ dấu chuẩn xác (ví dụ: `[Mục 4.1. Tâm lý học là gì](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#41-tâm-lý-học-là-gì-và-không-phải-là-gì)`).
   - **Căn cứ khoa học:** Ghi rõ mã Claim ID (`[C00-XX]`, `[C01-XX]`) và trích dẫn chuẩn APA 7th khớp 100% với `quality/source-ledger.md`.
3. **Phần Hướng Dẫn Tự Đánh Giá & Spaced Review:**
   - Cập nhật thang điểm tương ứng 12 câu:
     - 11–12/12 câu: Nắm rất vững kiến thức và tư duy phương pháp luận; sẵn sàng cho Case Study thực hành.
     - 8–10/12 câu: Nắm tốt nhưng còn điểm mù ở một số trường phái hoặc bẫy tương quan; cần bấm link heading ôn lại các câu sai.
     - Dưới 8/12 câu: Cần dành thêm 30–45 phút đọc lại tầng Cốt lõi (Mục 3) và làm lại quiz sau 1–2 ngày.

---

### 6.3. Nâng cấp Quality Gate chống Answer-Format Cues (`quality/content-audit-checklist.md`)

Cập nhật file checklist chất lượng để bổ sung các quy tắc kiểm toán ngăn chặn triệt để hiện tượng lộ đáp án qua hình thức cho toàn khóa học:

#### 1. Bổ sung Nhóm Tiêu chí Kiểm toán Chống Manh mối Hình thức (Nhóm CUE — 8 tiêu chí chuẩn 6 cột):

Đưa bảng sau vào `quality/content-audit-checklist.md` dưới dạng **Nhóm 6: Chống Manh mối Hình thức & Chất lượng Trắc nghiệm (Anti-Cue & Quiz Rigor — 8 tiêu chí)**:

| Mã | Tên tiêu chí | Định nghĩa vi phạm | Điều kiện PASS (Chuẩn bắt buộc) | Phương pháp kiểm tra | Severity |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **CUE-01** | Cân bằng Độ dài & Định dạng | Đáp án đúng dài/ngắn vượt trội hoặc format khác biệt. | Cả 4 phương án A, B, C, D trong cùng câu có độ dài tương đương (độ lệch ký tự giữa dài nhất và ngắn nhất $\le 25\%$). Cấm đáp án đúng luôn dài nhất. | Đếm số ký tự/từ của từng phương án. | 🟡 Important |
| **CUE-02** | Cân xứng Thuật ngữ Song ngữ | Đáp án đúng là lựa chọn duy nhất có thuật ngữ tiếng Anh. | Thuật ngữ tiếng Anh gốc trong ngoặc đơn nếu có phải nằm trên dẫn đề (stem) hoặc xuất hiện đồng đều ở cả 4 lựa chọn A, B, C, D. | Quét ngoặc đơn tiếng Anh trong các phương án. | 🟡 Important |
| **CUE-03** | Phương án Nhiễu Hợp lý & Cùng Cấp độ | Distractor ngớ ngẩn, phi lý, lệch cấp độ khái niệm. | 100% phương án sai là hiểu lầm/ngụy biện thực tế có vẻ hợp lý và cùng cấp độ khái niệm với đáp án đúng (cùng trường phái, cùng chuyên ngành, cùng cấp bậc). | Thẩm định nội dung sư phạm từng distractor. | 🟡 Important |
| **CUE-04** | Chống Stem-Keyword Echo | Từ khóa đặc trưng của stem lặp lại đơn độc ở đáp án đúng. | Không có từ khóa hiếm/đặc trưng nào từ câu hỏi chỉ xuất hiện duy nhất ở đáp án đúng để người học bắt từ khóa đoán mò. | So khớp từ khóa giữa stem và 4 lựa chọn. | 🟡 Important |
| **CUE-05** | Chống Mẫu "Dài & Thận trọng Nhất" | Đáp án đúng vừa dài nhất vừa dùng ngôn ngữ thận trọng đối lập cực đoan. | Tuyệt đối không để xảy ra mô hình kép: đáp án đúng thận trọng ("có xu hướng", "có thể") trong khi cả 3 đáp án sai cực đoan ("luôn luôn", "chắc chắn 100%"). | Rà soát tương quan giữa độ dài và giọng văn. | 🔴 Critical |
| **CUE-06** | Đánh giá Độc lập Mù Manh mối (Blind Cue Review) | Người đọc chỉ nhìn 4 lựa chọn (không đọc stem) đoán được đáp án. | Reviewer độc lập đọc 4 lựa chọn mà không đọc câu hỏi; tỷ lệ nhận diện đáp án đúng không được vượt trội xác suất ngẫu nhiên (25%). | Thử nghiệm Blind Cue Review độc lập. | 🔴 Critical |
| **CUE-07** | Ma trận Phủ Khái niệm (Concept Coverage) | Chỉ đo LO chung chung, bỏ sót các cụm khái niệm cốt lõi. | Có ma trận đối chiếu câu hỏi kiểm tra phủ đủ 100% các cụm khái niệm thành phần chi tiết của tuần (Tuần 01: 12 cụm khái niệm). | Đối chiếu bảng Concept Coverage Matrix. | 🟡 Important |
| **CUE-08** | Cách ly Siêu dữ liệu Khỏi Người học | Lộ Bloom level, LO tag, ma trận, section labels trên view quiz. | File quiz của người học chỉ hiển thị mã câu `Q[NN]-XX`; toàn bộ Bloom, LO, ma trận, tỷ lệ phần trăm được chuyển sang Answer Key và Audit. | Soát file `assessments/quizzes/week-NN-quiz.md`. | 🔴 Critical |

#### 2. Cập nhật Definition of Done (DoD) Phiên bản 1 (v1.0):

Bổ sung phần sau vào cuối file `quality/content-audit-checklist.md`:

```markdown
---

## 4. Definition of Done Phiên bản 1 (DoD v1.0 — Hiệu chỉnh 2026-09-09)

Một tuần học chỉ được đóng (Closed / Complete) khi thỏa mãn trọn vẹn 8 điều kiện tiên quyết:
1. **Bài học hoàn tất:** Đủ 12 section H2, đạt chuẩn khối lượng từ hai tầng (Fast-Track 600–900 từ, Deep Dive 2.000–3.500 từ), không bịa đặt nguồn, không vi phạm an toàn.
2. **Quiz 12 câu sạch metadata:** Đủ 12 câu, interleave thứ tự, sạch hoàn toàn Bloom/LO/matrix trên giao diện người học, phân bổ đáp án cân bằng (3A, 3B, 3C, 3D).
3. **Pass 100% Cue Audit:** 12/12 câu vượt qua kiểm toán mù (Blind Cue Review), không có câu nào lộ đáp án qua độ dài, format, song ngữ hay ngôn ngữ thận trọng.
4. **Phủ trọn vẹn Concept Matrix:** 12/12 cụm khái niệm cốt lõi đều được đo lường trực tiếp.
5. **Answer Key hoàn chỉnh:** Bảng Quick Matrix 12 câu có cột Concept Tag; giải thích sâu sắc lý do đúng và lý do sai của từng distractor; link heading bài học Unicode chuẩn xác; 100% Claim ID truy xuất được trong ledger.
6. **Báo cáo Audit minh bạch:** Ghi nhận đủ 51 tiêu chí (43 cũ + 8 CUE); đính chính mọi sai lệch lịch sử.
7. **Template & Quy ước chuẩn hóa:** Áp dụng thống nhất slug anchor Unicode tiếng Việt giữ dấu và chuẩn cách ly metadata.
8. **Người học nghiệm thu:** Được Người học trực tiếp làm thử và xác nhận APPROVED tại Checkpoint tương ứng.
```

#### 3. Làm rõ Tiêu chí Đo lường Khối lượng Từ (SCC-03):

Bổ sung ghi chú kỹ thuật vào dòng mô tả của tiêu chí SCC-03:
- **Quy ước đếm từ tiếng Việt:** Lệnh PowerShell `(Get-Content -Raw $file | Measure-Object -Word).Words` đếm các âm tiết đơn lập cách nhau bởi khoảng trắng.
- **Phân định phạm vi:** Giới hạn 2.000–3.500 từ áp dụng cho khối lý thuyết Deep Dive (Mục 4); các phần bổ trợ thực hành, case study, glossary (Mục 5–10) được tính riêng để không làm loãng chiều sâu lý thuyết cốt lõi.

---

### 6.4. Chuẩn hóa các Template Hệ thống (`templates/`)

1. **`templates/quiz-template.md`:**
   - Loại bỏ hoàn toàn bảng ma trận đề thi (Assessment Matrix) và các tiêu đề `## Phần 1: Nhớ & Hiểu...`, `## Phần 2: Áp dụng...`, `## Phần 3: Đánh giá...` khỏi phần hiển thị người học. Toàn bộ câu hỏi nằm dưới một tiêu đề duy nhất: `## Danh sách Câu hỏi Trắc nghiệm`.
   - Đưa các quy tắc thiết kế sư phạm (interleave câu hỏi, cân bằng độ dài, chống cues song ngữ, tiêu chuẩn distractor cùng cấp độ khái niệm, không echo từ khóa) vào khối comment quản trị dành riêng cho tác giả biên soạn.
   - Tiêu đề câu hỏi chỉ giữ mã định danh sạch: `### Câu Q[NN]-XX` (không kèm Bloom level hay LO tag).
2. **`templates/answer-key-template.md`:**
   - Cập nhật bảng Quick Answer Matrix mẫu có thêm cột `Cụm khái niệm (Concept Cluster)`.
   - Hướng dẫn cấu trúc bắt buộc 6 thành phần: (1) Mã câu & Đáp án, (2) Vì sao đúng, (3) Phân tích chi tiết từng phương án sai (❌ A, B, C, D), (4) Mã LO & Cụm khái niệm, (5) Link tương đối đến heading bài học (anchor Unicode), (6) Claim ID & Nguồn.
   - Bổ sung quy ước chuẩn hóa slug anchor Unicode tiếng Việt giữ dấu nguyên bản.
3. **`templates/lesson-template.md`:**
   - **CHỈ CHỈNH SỬA QUY ƯỚC ANCHOR:** Cập nhật một ghi chú kỹ thuật ngắn gọn hướng dẫn quy ước tạo slug anchor Unicode tiếng Việt giữ dấu cho các heading (ví dụ: `### 4.1. Tâm lý học là gì...` $\rightarrow$ `#41-tâm-lý-học-là-gì-và-không-phải-là-gì`), bảo đảm tương thích 100% với GitHub Markdown và VS Code. Tuyệt đối không thay đổi cấu trúc 12 section H2 hay nội dung bài học.

---

### 6.5. Cập nhật Báo cáo Kiểm toán Tuần 01 & Sửa lỗi Mapping cũ (`quality/week-01-audit-report.md`)

1. **Đính chính Lỗi Ánh xạ Lịch sử Câu `Q01-08`:**
   - Bổ sung mục **Đính chính Lịch sử Kiểm toán (Audit Errata)**: Nêu rõ trong Báo cáo Audit vòng 1 (Task 4), bảng ma trận Mục 6 (dòng 127) đã ghi nhầm câu `Q01-08` là đo lường mô hình sinh–tâm–xã hội (`C01-12`), trong khi artifact thực tế của câu `Q01-08` trong cả quiz và answer key lại hỏi về Tâm lý học nhân văn (`C01-07`). Báo cáo mới đính chính và thay thế bằng ma trận 12 câu hoàn chỉnh.
2. **Cập nhật Toàn bộ Bảng Ma trận Đánh giá cho 12 Câu mới:**
   - Lập bảng ma trận 12 câu khớp 100% giữa: Mã câu hỏi (`Q01-01`..`Q01-12`), Cấp độ Bloom, Mã LO, 12 Cụm khái niệm, Đáp án đúng (3A, 3B, 3C, 3D), Heading bài học liên kết, và Claim ID.
3. **Bổ sung Bảng Đánh giá Kiểm toán Mù Manh mối (Blind Cue Audit Table — 8 cột):**
   - Đánh giá chi tiết từng câu trong 12 câu theo định dạng bảng chuẩn:
     `| Mã câu | CUE-01: Độ dài (Δ ≤ 25%) | CUE-02: Song ngữ cân xứng | CUE-03: Distractor cùng cấp độ | CUE-04: Không stem-echo | CUE-05: Không mẫu kép Dài+Thận trọng | CUE-06: Độc lập Blind Guess PASS | Kết luận |`
   - Xác nhận 12/12 câu đều đạt kết luận **PASS**.
4. **Bổ sung Bảng Ma trận Độ phủ Khái niệm (Concept Coverage Matrix):**
   - Lập bảng chứng minh 12 cụm khái niệm cốt lõi đều có ít nhất 1 câu hỏi đo lường trực tiếp, đối chiếu với Section trong bài học và Claim ID.
5. **Cập nhật Chấm điểm Toàn bộ 51 Tiêu chí Checklist:**
   - Chấm điểm đầy đủ 43 tiêu chí ban đầu (SCC, SRC, SAF, PED, TEC) cùng 8 tiêu chí CUE mới bổ sung (CUE-01 đến CUE-08).

---

### 6.6. Đồng bộ Trạng thái Lộ trình và Tracker (`learning-roadmap.md` & `progress.md`)

1. **`progress.md`:**
   - Cập nhật cột Ghi chú / Ngày của Tuần 01: Ghi rõ: *"Đã đại tu Quiz 12 câu & Answer Key theo phản hồi người học tại Checkpoint 3 (2026-09-09); đang chờ người học nghiệm thu đợt 2"*.
   - **GIỮ NGUYÊN KÝ HIỆU TRẠNG THÁI `—` (hoặc `▶`) TẠI CỘT QUIZ VÀ CORE; TUYỆT ĐỐI CHƯA ĐÁNH DẤU HOÀN THÀNH (`✓`)**.
2. **`learning-roadmap.md`:**
   - Giữ nguyên Tuần 01 ở trạng thái chờ duyệt (giữ nhãn `[Sắp biên soạn]` hoặc ghi rõ trạng thái đang hiệu chỉnh); không chuyển sang trạng thái bài học đã xuất bản hoàn thành.

---

## 7. Chi tiết Kỹ thuật về 12 Cụm Khái niệm & Ma trận Kiểm tra

Bài kiểm tra 12 câu của Tuần 01 bắt buộc phải bao phủ trọn vẹn 12 cụm kiến thức khoa học sau đây:

| Cụm | Tên Cụm Khái Niệm Trọng Yếu | Nội dung Bản chất Khoa học cần Kiểm tra | Căn cứ Khoa học & Claim ID | Mục tiêu LO liên quan |
| :---: | :--- | :--- | :--- | :---: |
| **01** | **Khoa học & Đo lường Gián tiếp** | Tâm trí không thể quan sát trực tiếp; tâm lý học đo gián tiếp qua hành vi, sinh học, self-report rồi suy luận kèm độ bất định; phân biệt với trực giác/đọc vị. | `[C00-01a]`, `[C01-01]` | `[LO-W01-01]` |
| **02** | **Bốn Mục tiêu Khoa học** | 4 mục tiêu (mô tả, giải thích, dự đoán, điều chỉnh); phân biệt mô tả khách quan với phán xét; bản chất xác suất của dự đoán (không đồng nghĩa chắc chắn). | `[C00-01b]`, `[C00-03]` | `[LO-W01-02]` |
| **03** | **Structuralism vs Functionalism** | Wundt/Titchener (cấu trúc ý thức, introspection và giới hạn chủ quan) vs William James (chức năng thích nghi theo Darwin, hoạt động toàn thể). | `[C01-02]`, `[C01-03]` | `[LO-W01-03]` |
| **04** | **Thuyết Phân tâm & Tiêu chuẩn Kiểm chứng** | Freud (vô thức, trải nghiệm thơ ấu); tính tranh cãi khoa học; tiêu chí falsifiability (khó kiểm chứng thực nghiệm bằng quan sát). | `[C01-04]` 🔴 | `[LO-W01-03]` |
| **05** | **Tâm lý học Gestalt** | Wertheimer, Koffka, Köhler; nguyên lý tổng thể khác tổng các phần rời rạc; đóng góp cho nghiên cứu cảm giác và tri giác. | `[C01-05]` | `[LO-W01-03]` |
| **06** | **Chủ nghĩa Hành vi (Behaviorism)** | Pavlov, Watson, Skinner; tập trung vào hành vi quan sát được và kích thích - phản xạ, củng cố/trừng phạt; giới hạn: bỏ qua tâm trí, quyết định luận. | `[C01-06]` | `[LO-W01-03]` |
| **07** | **Tâm lý học Nhân văn & Nhận thức** | Humanistic (Maslow, Rogers: tiềm năng tốt bẩm sinh, client-centered) vs Cognitive Revolution (Chomsky, Neisser: khôi phục nghiên cứu tâm trí bằng thực nghiệm). | `[C01-07]`, `[C01-08]` | `[LO-W01-03]` |
| **08** | **Chuyên ngành & Phân biệt Nghề nghiệp** | APA 54 phân hội; phân biệt rõ Clinical Psychology (rối loạn), Counseling Psychology (người khỏe mạnh), Psychiatrist (bác sĩ y khoa kê đơn thuốc). | `[C01-09]`, `[C01-10]`, `[C01-11]` | `[LO-W01-03]` |
| **09** | **Mô hình Sinh–Tâm–Xã hội** | Biopsychosocial model: sức khỏe và hành vi là sự tương tác đồng thời giữa 3 yếu tố sinh học, tâm lý và bối cảnh văn hóa - xã hội. | `[C01-12]` | `[LO-W01-03]` |
| **10** | **Thiên lệch Mẫu WEIRD & Khái quát hóa** | Henrich et al. (2010): cơ sở dữ liệu tâm lý học bị nghiên cứu quá mức trên mẫu phương Tây (WEIRD); thận trọng khi khái quát hóa sang văn hóa khác (kể cả Việt Nam). | `[C00-02]` 🟡 | `[LO-W01-04]` |
| **11** | **Khủng hoảng Tái lập & Tính Khiêm tốn** | Open Science Collaboration (2015): tỷ lệ tái lập thực tế thấp; phân biệt ý nghĩa thống kê ($p < .05$) với kích thước hiệu ứng (effect size); khoa học tự điều chỉnh. | `[C00-04]` | `[LO-W01-04]` |
| **12** | **Tương quan vs Nhân quả & Pop-Psych** | Hai biến đi kèm nhau không chứng minh quan hệ nhân quả (nguy cơ biến thứ ba, đảo chiều); tư duy đa giả thuyết khi đánh giá một claim trên truyền thông mạng. | `[C00-03]`, `[C01-13]` | `[LO-W01-04]` |

### Ma trận Phân bổ Đáp án và Mức độ Nhận thức Nội bộ Gợi ý

Để đảm bảo đáp ứng đồng thời các tiêu chí: **Interleave thứ tự**, **Phân bổ 3A - 3B - 3C - 3D**, **Tỷ lệ 4 Nhớ/Hiểu - 5 Áp dụng - 3 Đánh giá**, và **Phủ 12 Cụm**, người thực thi tham chiếu ma trận cấu trúc chuẩn sau:

| Mã câu | Cụm Kiến Thức | Cấp độ Nhận thức | Đáp án Đúng | Dạng Thức Trình Bày & Tình Huống Sư Phạm | Vị trí Heading Ôn Tập (Anchor Unicode) | Claim ID |
| :---: | :--- | :---: | :---: | :--- | :--- | :---: |
| `Q01-01` | Cụm 01: Đo lường gián tiếp | Nhớ / Hiểu | **C** | Định nghĩa bản chất đo lường gián tiếp tâm trí qua 3 nguồn dữ liệu (hành vi, sinh học, self-report). | [Mục 4.1. Tâm lý học là gì](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#41-tâm-lý-học-là-gì-và-không-phải-là-gì) | `[C00-01a]`, `[C01-01]` |
| `Q01-02` | Cụm 03: Structuralism vs Functionalism | Áp dụng / Phân tích | **A** | So sánh tình huống nghiên cứu phản ứng tâm lý giữa Wundt (cấu trúc) và James (chức năng thích nghi). | [Mục 4.3. Bảy trường phái](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#43-bảy-trường-phái-và-tâm-lý-học-đương-đại) | `[C01-02]`, `[C01-03]` |
| `Q01-03` | Cụm 10: Thiên lệch WEIRD | Đánh giá Bằng chứng | **D** | Đánh giá tính khái quát của một kết luận nghiên cứu tâm lý thực hiện trên mẫu sinh viên phương Tây. | [Mục 6.2. Giới hạn](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#62-những-giới-hạn-và-điều-chưa-thể-kết-luận) | `[C00-02]` 🟡 |
| `Q01-04` | Cụm 06: Behaviorism | Nhớ / Hiểu | **B** | Nhận diện trọng tâm (hành vi quan sát được, củng cố/trừng phạt) và giới hạn quyết định luận của behaviorism. | [Mục 4.3. Bảy trường phái](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#43-bảy-trường-phái-và-tâm-lý-học-đương-đại) | `[C01-06]` |
| `Q01-05` | Cụm 02: Bốn mục tiêu khoa học | Áp dụng / Phân tích | **A** | Phân tích tình huống dự đoán hành vi mang tính xác suất của một nhà khoa học (phân biệt với phán xét). | [Mục 4.2. Bốn mục tiêu](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#42-bốn-mục-tiêu-của-khoa-học-tâm-lý-học) | `[C00-01b]` |
| `Q01-06` | Cụm 08: Phân biệt nghề nghiệp | Áp dụng / Phân tích | **D** | Tư vấn chọn dịch vụ hỗ trợ phù hợp cho một cá nhân khỏe mạnh đang stress (Counseling vs Clinical vs Psychiatrist). | [Mục 4.3. Bảy trường phái](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#43-bảy-trường-phái-và-tâm-lý-học-đương-đại) | `[C01-09]`, `[C01-10]`, `[C01-11]` |
| `Q01-07` | Cụm 05: Gestalt | Nhớ / Hiểu | **B** | Nhận diện định nghĩa và nguyên lý cốt lõi "tổng thể khác tổng các phần rời rạc" của Gestalt trong tri giác. | [Mục 4.3. Bảy trường phái](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#43-bảy-trường-phái-và-tâm-lý-học-đương-đại) | `[C01-05]` |
| `Q01-08` | Cụm 12: Tương quan vs Nhân quả | Đánh giá Bằng chứng | **C** | Phản biện một bài báo giật tít về mối liên hệ giữa thời gian dùng mạng và lo âu (chỉ ra bẫy biến thứ ba). | [Mục 4.2. Bốn mục tiêu](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#42-bốn-mục-tiêu-của-khoa-học-tâm-lý-học) | `[C00-03]`, `[C01-13]` |
| `Q01-09` | Cụm 04: Thuyết Phân tâm | Áp dụng / Phân tích | **A** | Phân tích tình huống trị liệu dựa trên giả thuyết vô thức và chỉ ra khó khăn kiểm chứng thực nghiệm (falsifiability). | [Mục 4.3. Bảy trường phái](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#43-bảy-trường-phái-và-tâm-lý-học-đương-đại) | `[C01-04]` 🔴 |
| `Q01-10` | Cụm 09: Biopsychosocial Model | Áp dụng / Phân tích | **C** | Phân tích đa tầng nguyên nhân (sinh học, tâm lý cá nhân, bối cảnh văn hóa - xã hội) trong một ca mệt mỏi/stress. | [Mục 4.1. Tâm lý học là gì](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#41-tâm-lý-học-là-gì-và-không-phải-là-gì) | `[C01-12]` |
| `Q01-11` | Cụm 07: Humanistic vs Cognitive | Nhớ / Hiểu | **B** | So sánh câu hỏi trung tâm và phương pháp giữa tâm lý học nhân văn (tiềm năng) và cách mạng nhận thức (thực nghiệm). | [Mục 4.3. Bảy trường phái](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#43-bảy-trường-phái-và-tâm-lý-học-đương-đại) | `[C01-07]`, `[C01-08]` |
| `Q01-12` | Cụm 11: Khủng hoảng Tái lập | Đánh giá Bằng chứng | **D** | Đánh giá thái độ khoa học chuẩn mực và ý nghĩa của Open Science khi một hiệu ứng kinh điển không tái lập được. | [Mục 6.2. Giới hạn](../../notes/01-scientific-foundations/week-01-what-is-psychology.md#62-những-giới-hạn-và-điều-chưa-thể-kết-luận) | `[C00-04]` |

*(Ghi chú: Ma trận trên đảm bảo chính xác 3 câu A, 3 câu B, 3 câu C, 3 câu D; đan xen xen kẽ hoàn toàn các mức độ nhận thức (4 Nhớ/Hiểu, 5 Áp dụng, 3 Đánh giá); phủ kín 12 cụm kiến thức; và trỏ chính xác đến các heading anchor Unicode hiện có trong bài học).*

---

## 8. Tiêu chí Nghiệm thu Chi tiết (Acceptance Criteria & Quality Gate)

Gói công việc Task 5 chỉ được Reviewer và Người học nghiệm thu khi thỏa mãn 100% các điều kiện sau:

1. **Pass Cue Audit 12/12 câu:** Toàn bộ 12 câu hỏi đều vượt qua bài kiểm tra mù (Blind Cue Audit). Không có câu nào lộ đáp án qua độ dài vượt trội, qua sự xuất hiện đơn độc của thuật ngữ tiếng Anh, qua ngôn ngữ thận trọng đối lập với từ ngữ cực đoan, hoặc qua stem-keyword echo.
2. **Độ phủ Toàn diện 12/12 Cụm:** 12 cụm khái niệm trọng yếu đều có ít nhất 1 câu hỏi đo lường trực tiếp, có liên kết tương ứng trong Answer Key và Báo cáo Audit.
3. **Learner-Facing Quiz Sạch Metadata:** Giao diện bài quiz của người học hoàn toàn không chứa Bloom level, mã LO, bảng ma trận nhận thức, tỷ lệ phần trăm hoặc tiêu đề phân nhóm câu hỏi.
4. **Phân bổ Đáp án Cân bằng:** Đúng chính xác 3A, 3B, 3C, 3D (mỗi chữ cái đúng 3 lần).
5. **Answer Key Sâu sắc & Chính xác:** Giải thích chi tiết bản chất đúng của đáp án; phân tích cặn kẽ vì sao từng phương án nhiễu sai; có đầy đủ LO, Concept Tag, link heading bài học, và Claim ID.
6. **Đính chính Báo cáo Audit cũ:** Báo cáo audit mới đính chính rõ ràng sai lệch của câu `Q01-08` cũ và cập nhật toàn diện bảng ma trận cho 12 câu mới.
7. **Cập nhật Quality Gate & Templates:** `content-audit-checklist.md` có đầy đủ 8 tiêu chí CUE, DoD v1.0, ngày hiệu chỉnh; 3 template (`quiz-template.md`, `answer-key-template.md`, `lesson-template.md`) được cập nhật các chuẩn hóa sư phạm và anchor Unicode.
8. **Liên kết Nội bộ Pass 100%:** Toàn bộ link tương đối và heading anchor giữa quiz, answer key, bài học, checklist và roadmap hoạt động hoàn hảo, không phát sinh broken link.
9. **Bảo toàn Tiến độ & Ràng buộc:** `progress.md` và `learning-roadmap.md` giữ nguyên trạng thái W01 chờ người học duyệt (không đánh dấu hoàn thành `✓`). Tuyệt đối không tạo bất kỳ file nào thuộc Tuần 02 (W02). Handoff 004 được giữ nguyên vẹn làm hồ sơ lịch sử.

---

## 9. Điều kiện Dừng & Xử lý Tình huống Blocker

Agent thực thi phải **dừng lại ngay lập tức và báo cáo Blocker** trong các tình huống sau:

1. **Phát hiện Mâu thuẫn Kiến thức:** Nếu phát hiện bất kỳ câu hỏi nào trong 12 cụm kiến thức không thể tìm thấy cơ sở lý thuyết tương ứng trong bài đọc Tuần 01 hiện tại.
2. **Yêu cầu Xóa hoặc Ghi đè Hồ sơ Lịch sử:** Nếu có bất kỳ chỉ thị hoặc xung đột nào dẫn đến nguy cơ xóa/ghi đè file `handoff/pm/004-task-05-calibration.md` hoặc các file handoff cũ.
3. **Hiểu nhầm Triển khai Tuần 02:** Nếu có bất kỳ bước xử lý nào chuẩn bị tạo file cho Tuần 02 (`week-02-*`). Phải dừng lại ngay và khẳng định Tuần 02 thuộc Task 6.

---

## 10. Gói Bàn giao Bắt buộc cho Vòng Review Kế tiếp

Sau khi hoàn thành công việc, agent thực thi nộp báo cáo hoàn thành kèm gói bàn giao gồm:

1. **Danh sách các file đã cập nhật** (kèm tóm tắt nội dung chỉnh sửa chính của từng file).
2. **Bảng Ma trận Tổng hợp 12 Câu hỏi Quiz W01:** Gồm Mã câu, Bloom level, Mã LO, Cụm khái niệm (1–12), Đáp án đúng, Vị trí heading ôn tập, và Claim ID liên kết.
3. **Bằng chứng Kiểm toán Mù Manh mối (Blind Cue Audit Table):** Bảng phân tích chứng minh 12/12 câu không bị lộ đáp án qua độ dài, song ngữ, từ khóa lặp hoặc ngôn ngữ thận trọng.
4. **Bằng chứng Đính chính Mapping `Q01-08`** trong Báo cáo Audit Tuần 01.
5. **Trích đoạn 8 Tiêu chí CUE và DoD v1.0** đã bổ sung vào `content-audit-checklist.md`.
6. **Bằng chứng Cập nhật 3 Template** (`quiz-template.md`, `answer-key-template.md`, `lesson-template.md`).
7. **Kết quả Kiểm tra Liên kết Nội bộ & Heading Anchor.**
8. **Xác nhận Tuân thủ Ràng buộc:** Xác nhận `progress.md` và `learning-roadmap.md` vẫn giữ W01 ở trạng thái chờ duyệt; xác nhận không tạo file W02; xác nhận không xóa Handoff 004.
9. **Tuyên bố Dừng:** Không tự tuyên bố APPROVED; dừng lại chờ Reviewer và Người học thẩm định.
