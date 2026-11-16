# 🤝 HANDOFF — Tiếp Tục Tạo Tài Liệu Đến Hết Phase 5

> **File này dành cho AI executor ở session mới.** Đọc toàn bộ file trước khi làm gì. Sau mỗi batch hoàn thành, CẬP NHẬT mục 2 (Trạng thái) của file này — nó là điểm neo để resume giữa nhiều session.

---

## 0. NHIỆM VỤ

Tạo tiếp tài liệu học cho dự án tự học tiếng Anh (chủ đề tâm lý học) tại `D:\03_Study\Roadmap\IELTS\`. Phase 3 Month 1 hiện có một guide điều phối và sáu cycle văn bản bounded; các batch còn lại vẫn là công việc tiếp theo. Làm theo đúng quy ước đã thiết lập (mục 3), quy trình từng batch (mục 5), và chạy quality gates (mục 6) sau mỗi batch.

Người học sẽ nhắn "tạo batch tiếp" sau mỗi tháng — nhưng nếu được yêu cầu "làm hết" thì cứ chạy tuần tự batch 1 → 14, không cần chờ.

---

## 1. BỐI CẢNH DỰ ÁN (2 phút)

- **Người học:** người Việt, xuất phát A1, mục tiêu trình độ tương đương IELTS 6.5-7.0 (C1) — **học để có kiến thức & dùng thật, KHÔNG thi** (luyện đề thi = tùy chọn).
- **Thời gian:** kế hoạch lịch sử khoảng 4 giờ/ngày, ~18-24 tháng, không deadline. Gói P3-M1 hiện hành ghi tải thực tế 310 phút cho C1–C5 và 280 phút cho C6; không dùng mốc 4 giờ như một yêu cầu đạt.
- **Phương pháp:** tự học 100% + AI (Anki flashcard, AI voice conversation hằng ngày, graded reader mỗi tối).
- **Chủ đề xuyên suốt:** tâm lý học — tình yêu, cảm xúc, tính cách, body language, ký ức, thiên kiến nhận thức... (mọi từ vựng, bài đọc, câu ví dụ đều xoay quanh tâm lý).
- **Ngôn ngữ tài liệu:** hướng dẫn bằng TIẾNG VIỆT, nội dung học bằng tiếng Anh (đúng trình độ mục tiêu của tháng đó).
- **File kế hoạch gốc:** `implementation_plan_new.md` (bản sửa 2 — nguồn sự thật cho lộ trình). `implementation_plan.md` là bản lỗi thời, KHÔNG dùng.

---

## 2. TRẠNG THÁI HIỆN TẠI (⬅️ CẬP NHẬT SAU MỖI BATCH)

| Khu vực | Số file | Trạng thái |
|---------|---------|------------|
| `Phase-0/` | 18 | ⚠️ Snapshot: nội dung đã tạo; technical/source closure 805 bounded; không phải whole-course approval |
| `Phase-1/` | 161 | ⚠️ Snapshot: nội dung đã tạo; technical/source closure 805 bounded; không phải whole-course approval |
| `Phase-2/` | 158 | ⚠️ Snapshot: nội dung đã tạo; technical/source closure 805 bounded; không phải whole-course approval |
| `Phase-3/` | 81 | ⚠️ Có guide Month 1, sáu cycle văn bản, assessment và C1 pilot; audio/browser/package/full-course gates còn mở |
| `Phase-4/` | 0 | ⏳ Chưa tạo |
| `Phase-5/` | 0 | ⏳ Chưa tạo |
| `Resources/` | 7 | ⚠️ Snapshot của resource inventory hiện tại; không phải whole-course approval |
| Root | 5 | Snapshot: README.md · roadmap.md · implementation_plan_new.md · implementation_plan.md (cũ) · HANDOFF.md (file này) |

**Gói hiện hành: `Phase 3 — Tháng 1 (P3-M1)`** (Relative Clauses + Personality Deep Dive — Big Five, critique MBTI). Mở [`Phase-3/month1-study-guide.md`](Phase-3/month1-study-guide.md) để dùng lịch và links hiện có. Đây là gói văn bản bounded; audio playback/alignment, browser/native package, các tháng sau và full-course release vẫn chưa đóng.

---

**Thứ tự ưu tiên cho quy ước:** các mục 3a–3e bên dưới giữ lại template lịch sử của Phase 1–2 để tra cứu, không tự động áp dụng cho P3-M1. P3-M1 hiện dùng [`Phase-3/month1-study-guide.md`](Phase-3/month1-study-guide.md): nguồn public hiện hành, đủ sáu retry, paper 100 điểm, pass 60 cho test thường và 70 cho D12/D24, cùng tải 310/280 phút. Các curriculum tương lai phải theo guide đã sửa; không khôi phục quy tắc chỉ có `test-01-retry` hoặc audio AI chưa được xác nhận.

## 3. QUY ƯỚC BẮT BUỘC (pattern đã thiết lập — PHẢI tuân theo)

### 3a. Cấu trúc 1 tháng học (mỗi batch)

- 6 chu kỳ (C1-C6). Mỗi chu kỳ = **3 ngày học + 1 ngày test** → 24 ngày học/tháng.
- C6 = chu kỳ REVIEW (không từ mới, không grammar mới).
- Ngày nghỉ nhẹ sau mỗi 6 ngày học (chỉ Anki + nghe thụ động).
- Khung ngày 4 slot: 🧠 Grammar 60' · 📝 Từ vựng 60' · 🎧 Nghe 55' · 📖 Đọc 50' · 🌙 tối: Anki + graded reader.

### 3b. Bộ file chuẩn 1 tháng (~40 file)

| Thư mục | File | Quy ước |
|---------|------|---------|
| root tháng | `daily-guide-monthN.md` | ⭐ file chính — bảng chu kỳ + bảng ngày + tên file chính xác từng slot |
| `grammar/` | 5 file `NN-topic.md` | Mỗi file 3 phần (A/B/C = 3 ngày), mỗi phần: bảng quy tắc + bài tập, cuối file ✅ ĐÁP ÁN đầy đủ |
| `vocabulary/` | 8 file | Tháng bắt đầu bằng 3 file `NN-theme-part1/2/3.md` (8 từ/file), sau đó 4 file `NN-theme-cycleN.md` (3 nhóm × 8 từ), cuối cùng 1 file review (không từ mới) |
| `reading/` | 8 file | 3 file đầu `NN-topic.md` + 5 file `cycleN-reading.md` (mỗi file 3 bài, đáp án trong thẻ details) |
| `listening/` | 8 file | 3 file `dayNN-topic.md` + 5 file `cycleN-listening.md` — mỗi file: nguồn YouTube thật + 5 câu shadowing + 3 câu dictation + mục tiêu % |
| `speaking/` | 1 file `speaking-prompts-monthN.md` | Bảng 24 ngày: chủ đề nói + gợi ý câu |
| `writing/` | 2 file | `monthN-dayNN-topic.md` (bài kickoff có mẫu câu) + `writing-prompts-monthN.md` (bảng prompt + checklist + prompt chấm bằng AI) |
| `tests/` | 7 file | `test-NN.md` × 6 (C1→C6) + `test-01-retry.md`. **Số test LIÊN TỤC trong phase** (P2: M1=01-06, M2=07-12, M3=13-18, M4=19-24) |

### 3c. Quy ước test

- Test chu kỳ (D4, D8, D16, D20): Grammar 50đ + Từ vựng 30đ + Nghe-Nói 20đ → thang /100. Ngưỡng ≥60%.
- **Test TỔNG #1** (D12) và **Test TỔNG #2** (D24): phủ toàn bộ grammar tháng + từ vựng, 90-120 phút. Ngưỡng ≥70%.
- Test cuối tháng cuối của phase = **PHASE FINALE** (thêm phần tổng ôn cả phase + nhắc retest EF SET + nghe lại ghi âm Ngày 0).
- Template lịch sử P1–P2: MỌI test phải có ĐÁP ÁN; phần nghe có thể dùng prompt AI voice. P3-M1 hiện hành dùng sáu public companion sources và transcript/key boundaries trong guide; không coi AI voice là đã xác nhận.

### 3d. Giọng văn & nội dung

- Hướng dẫn tiếng Việt, ấm nhưng gọn; câu ví dụ tiếng Anh LUÔN xoay quanh tâm lý/quan hệ/cảm xúc.
- Bài đọc = **tiểu luận tâm lý viết riêng** cho dự án này, nhắc khái niệm & nhà tâm lý có thật (Gottman, Dweck, Bowlby, Ainsworth, Freud, Kahneman, Asch, Milgram, Cialdini, Seligman...), kèm 3-4 câu hỏi hiểu + đáp án.
- Câu ví dụ từ vựng dùng đúng grammar đang học tuần đó (VD tháng Past Continuous → ví dụ dùng was remembering...).
- Mỗi daily-guide kết bằng 1 câu động lực gắn chủ đề tháng.

### 3e. Đặt tên & đánh số

- Số file **reset về 01 ở đầu mỗi phase** (Phase-2 bắt đầu grammar/01, test/01...), liên tục trong phase.
- Quy tắc lịch sử: chỉ test-01 của phase có file retry. P3-M1 hiện hành có retry riêng cho cả sáu papers theo guide.
- Error-log dùng chung: `Phase-1/error-log.md` (người học chép lỗi vào đây từ mọi phase).

---

## 4. LỘ TRÌNH CÒN LẠI — 14 BATCH

| # | Batch | Grammar | Chủ đề tâm lý | Ghi chú đặc biệt |
|---|-------|---------|----------------|-------------------|
| 1 | **P2-M2** | Present Perfect + Present Perfect Continuous (for/since, already/yet/just) | **The Mind & Decision-Making** — System 1/2, thiên kiến nhận thức (anchoring, loss aversion, confirmation bias) | Số test: 07-12 · ✅ ĐÃ TẠO (39 file, gates pass) |
| 2 | **P2-M3** | Past Perfect + Conditionals 0/1/2 | **Social Psychology** — Asch conformity, Milgram obedience, Cialdini persuasion, group dynamics | Test 13-18; cuối tháng nhắc retest EF SET (~tháng 7-8) · ✅ ĐÃ TẠO (39 file, gates pass) |
| 3 | **P2-M4** | Passive Voice (mọi thì) + tổng ôn | **Habits & Behavior Change** — vòng thói quen, dopamine, stress, self-control | Test 19-24; **test-24 = PHASE 2 FINALE** (tổng ôn 4 tháng + EF SET) · ✅ ĐÃ TẠO (39 file, gates pass) |
| 4 | **P3-M1** | Relative Clauses (defining/non-defining) | **Personality Deep Dive** — Big Five, critique MBTI, traits vs states | Gói hiện hành: C1 pilot reading 542/481 từ, essay 100-120 → 200-250; target chung 500-800 là kế hoạch, không phải claim của hai bài pilot |
| 5 | **P3-M2** | Reported Speech (+ reporting verbs) | **Communication & Language** — NVC, framing, lời nói đổi ý nghĩa | Test 07-12 |
| 6 | **P3-M3** | Third + Mixed Conditionals + Wish/If only | **Regret & Counterfactual Thinking** — những con đường không đi | Test 13-18; EF SET (~tháng 10-12) |
| 7 | **P3-M4** | Gerunds & Infinitives + tổng ôn | **The Good Life** — positive psychology, flow, meaning (Seligman, Csikszentmihalyi) | Test 19-24; **PHASE 3 FINALE** |
| 8 | **P4-M1** | Advanced Passive + Causative (have sth done) | **The Unconscious & Dreams** — Freud/Jung ở chiều sâu, biểu tượng | Bước sang B2→C1: reading 800-1000 từ, nghe TED 15-20' |
| 9 | **P4-M2** | Inversion + Cleft/Emphatic structures | **Persuasion & Rhetoric** — rhetoric, propaganda, manipulation vs influence | Test 07-12 |
| 10 | **P4-M3** | Mixed Conditionals + Subjunctive | **Moral Psychology** — trolley problem, moral intuitions, Kohlberg | Test 13-18; EF SET (~tháng 13-16) |
| 11 | **P4-M4** | Nominalization + Academic style + tổng ôn | **The Examined Mind** — metacognition, critical thinking | Test 19-24; **PHASE 4 FINALE** |
| 12 | **P5-M1** | (không grammar mới) | **Book Club: đọc sách thật** (VD *Attached* của Levine/Heller, hoặc sách The School of Life) | CẤU TRÚC KHÁC: `book-club/NN-chapters.md` + hướng dẫn đọc + từ vựng từ sách |
| 13 | **P5-M2** | (application) | **The Psychologist's Toolkit** — viết luận 400-500 từ, thảo luận dài, deep conversation | Viết kiểu blog; nói 15-20' với AI + người thật |
| 14 | **P5-M3** | (application) | **Final Project + tổng kết** | Test cuối = **PHASE 5 FINALE = TOÀN KHÓA HỌC**: EF SET cuối + nghe lại ghi âm Ngày 0 + reflection 300 từ + graduation |

### Con số chuẩn theo phase (giữ nhất quán)

| | P2 (còn) | P3 | P4 | P5 |
|-|---------|----|----|----|
| Từ mới/tháng | ~120-140 | ~140-160 | ~160-180 (AWL + collocations) | chỉ ôn (0 mới) |
| Độ dài bài đọc | 400-500 từ | 500-800 từ | 800-1000 từ | excerpt sách thật + bài báo |
| Nguồn nghe | TED-Ed, BBC 6min, The Moth | TED 10-15', Psychology in 10 Minutes | TED 15-20', Hidden Brain | Podcast 20-30' không sub |
| Viết | đoạn 100-150 từ | essay 200-250 từ | essay 250-300 + band descriptors | bài 400-500 từ |
| Nói | kể chuyện 2-3' | độc thoại 2' + kiểu cue card | hội thoại tự do 10-15' | 15-20' + AI hằng ngày |
| Dictation mục tiêu | ≥65-70% | ≥70-75% | ≥75-80% | ≥80-85% |
| Graded reader | Level 2-3 | Level 4-5 | nội dung thật (Psychology Today...) | sách nguyên bản |

> Ghi chú: tổng ~2.000 / 3.500 / 5.500 từ trong plan = flashcard + từ tích lũy từ đọc (plan ghi rõ từ P3: một nửa đến từ đọc-nghe). Flashcard core chỉ cần ~120-180 từ/tháng.

---

## 5. QUY TRÌNH 1 BATCH (checklist)

1. **todo_write** lên kế hoạch ~11 nhóm việc (theo cấu trúc các batch trước).
2. **Tạo thư mục** phase nếu chưa có (pwsh: New-Item -ItemType Directory -Force).
3. Viết `daily-guide-monthN.md` TRƯỚC (file điều phối — quyết định tên mọi file khác).
4. Viết 5 file grammar (mỗi file 3 phần A/B/C + đáp án).
5. Viết 8 file vocabulary (3 part + 4 cycle + 1 review; mỗi từ: IPA + loại + nghĩa + câu ví dụ dùng grammar tuần đó).
6. Viết 8 file reading (3 bài đầu + 5 file cycle × 3 bài — tiểu luận tâm lý CHẤT LƯỢNG, đây là linh hồn tài liệu).
7. Viết 8 file listening (nguồn YouTube thật + 5 câu shadowing + 3 câu dictation).
8. Viết speaking prompts (24 ngày) + 2 file writing.
9. Viết 7 file test (6 test + 1 retry; đủ đáp án).
10. **Cập nhật trackers:** `roadmap.md` (thêm bảng theo dõi tháng mới) · `implementation_plan_new.md` (block "Batch N ĐÃ TẠO" ở phần Bắt Đầu) · `README.md` (cột trạng thái phase).
11. **Chạy quality gates (mục 6)** → sửa hết lỗi → cập nhật **mục 2 của file này** (số file + batch kế tiếp).

---

## 6. QUALITY GATES (chạy sau MỖI batch — không bỏ qua)

Chạy trong run_code, mọi mục phải PASS:

1. **Đủ cấu trúc:** ~40 file (grammar 5, vocab 8, reading 8, listening 8, speaking 1, writing 2, tests 7, guide 1).
2. **CJK = 0:** grep từng file với pattern `[\u4e00-\u9fff]` — không được có ký tự Hán lọt vào tiếng Việt.
3. **Đếm từ vựng thực tế** (đếm dòng bắt đầu `| **`) → số này phải khớp mọi chỗ nhắc đến trong guide/test (không claim "~150" khi thực tế 112).
4. **Tham chiếu tồn tại:** mọi `ten-file.md` nhắc trong daily-guide phải có file thật (glob kiểm tra; chấp nhận tham chiếu same-folder không prefix).
5. **Mọi test có "ĐÁP ÁN"** và mọi grammar file có phần đáp án.

---

## 7. LỖI THƯỜNG GẶP & CÁCH TRÁNH (đã trả giá thật)

1. **Ký tự Hán (CJK) lọt vào tiếng Việt** khi gõ nhanh — đã gặp 4 lần trong dự án này → LUÔN grep pattern CJK trước khi kết thúc batch (xem mục 6).
2. **Backtick trong chuỗi code run_code** gây parse error → dùng mảng join hoặc nối chuỗi.
3. **pwsh stdout là OBJECT** `{text}` → gọi `r.stdout.text`.
4. **Glob Windows trả path với dấu gạch chéo ngược** → `.replace(/\\/g, "/")` trước khi regex.
5. **read có limit tối đa 2000 dòng** — file dài phải đọc nhiều lần.
6. **edit cần old_string CHÍNH XÁC** — đọc lại file khi không chắc (đặc biệt bảng có bold trong ô).
7. **Claim số mà không đếm** → đếm xong mới viết số.
8. **Test tham chiếu bài đọc thiếu prefix** (`cycleN-reading.md` → phải là `reading/cycleN-reading.md`).

---

## 8. MẪU NGẮN TỪNG LOẠI FILE

Chuẩn nhất: **mở file tương ứng của `Phase-2/` tháng 1** (mới nhất, đã qua audit) và bắt chước cấu trúc — `daily-guide-month1.md`, `grammar/01`, `vocabulary/01-memory-part1.md`, `reading/cycle2-reading.md`, `listening/day01-memory-stories.md`, `tests/test-03.md` (TỔNG), `tests/test-06.md` (TỔNG cuối tháng).

Phase-1 dùng format cũ hơn; Phase-0 vocabulary khác format bảng — **lấy Phase-2 làm chuẩn**.

---

## 9. SAU BATCH CUỐI CÙNG (P5-M3)

1. Chạy quality gates toàn workspace (tổng ~700 file).
2. Cập nhật README.md: mọi phase ✅ + dòng "Khóa học hoàn thiện".
3. Cập nhật roadmap.md: đủ bảng theo dõi mọi tháng.
4. Trong file finale P5-M3: kèm hướng dẫn "sau khi xong khóa học" (giữ thói quen Anki, đọc sách, AI voice; retest EF SET 6 tháng/lần).
5. Chỉ khi P5-M3 thực sự được nghiệm thu mới cập nhật mục 2 file này thành "🎉 TOÀN BỘ HOÀN TẤT — không còn batch nào"; đây là hướng dẫn lịch sử, không phải trạng thái hiện tại.

---

> *Nguyên tắc xuyên suốt của dự án: mỗi file phải tự chứa đủ để người học không bao giờ bị chặn giữa chừng chờ tài liệu. Batch nào xong là dùng được ngay.*
