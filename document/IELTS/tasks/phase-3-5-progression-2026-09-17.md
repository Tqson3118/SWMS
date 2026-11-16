# Progression Phase 3–5 (IELTS) — 2026-09-17

> **Bản thiết kế, không phải batch biên soạn.** Mục tiêu của file: mô tả tiến trình cả 3 phase còn lại ở mức **mục tiêu/prerequisites/kỹ năng/từ vựng-ngữ pháp/độ khó/tải học/ôn tích lũy/kiểm tra**. Đơn vị mẫu Phase 3 đầy đủ nằm ở `tasks/phase-3-unit-01-sample/` (7 file: README + 3 buổi + quiz + key + rubric). Thư mục `Phase-3/M1/C1/` hiện chỉ có 4 file nháp từ run trước — sẽ được đồng bộ khi batch M1 biên soạn, không sửa trong đợt này. Phase 4–5 **giữ ở mức thiết kế** trong đợt này — không tạo file bài học.
>
> **Nguồn sự thật:** `HANDOFF.md` §3-§4, `roadmap.md` §4d-h, `implementation_plan_new.md` §Phase 3-5, `tasks/phase-3-spec-2026-09-15.md`, `tasks/phase-3-file-inventory-2026-09-15.md`. Mọi mô tả dưới đây **kế thừa** mà không mâu thuẫn với các nguồn đó.
>
> **Lưu ý ownership:** file này thuộc T13 (thiết kế + đơn vị mẫu). Không cập nhật `HANDOFF.md`, `roadmap.md`, `README.md`, `implementation_plan_new.md` ở đợt này. Những tracker đó là P2 (sau khi review pass).

---

## 0. Bức tranh lớn (4 tháng × 3 phase = 12 tháng tiếp)

```
Tháng 9-12    ▓▓▓▓ Phase 3 — Intermediate (B1→B2)        [đơn vị mẫu: P3-M1-C1]
Tháng 13-16   ▓▓▓▓ Phase 4 — Upper-Intermediate (B2→C1)    [chỉ thiết kế]
Tháng 17-19+  ▓▓▓  Phase 5 — Mastery & Vận Dụng Thật       [chỉ thiết kế]
```

Mỗi tháng = 6 chu kỳ (C1–C6). Mỗi chu kỳ = 3 ngày học + 1 ngày test. C6 = chu kỳ review (không grammar mới). Ngày nghỉ nhẹ sau mỗi 6 ngày học (chỉ Anki + nghe thụ động).

**Số file quy ước mỗi tháng** (theo HANDOFF §3b): 1 daily-guide + 5 grammar + 8 vocabulary + 8 reading + 8 listening + 1 speaking + 2 writing + 7 tests = **40 file/tháng** × 11 tháng còn lại (P3-M1 → P5-M3) = ~440 file + Phase 3 finale (test-24) + Phase 4-5 finale = gần **450 file tiềm năng**. Con số này là **mục tiêu thiết kế**, không phải cam kết tạo hết trong một đợt.

---

## 1. Phase 3 — Intermediate (B1 → B2), 4 tháng (tháng 9-12 của lộ trình)

### 1.0 Mục tiêu cuối phase
- Đọc hiểu bài tâm lý 500-800 từ (tiếng Anh chuẩn, không simplify) ở tốc độ guide.
- Viết essay 200-250 từ có thesis, 2-3 đoạn phát triển, kết luận — dùng relative clauses, reported speech, conditionals, gerund/infinitive đúng chỗ.
- Nghe TED 10-15 phút, dictation ≥70-75% từ; tóm tắt 3-5 ý chính.
- Nói độc thoại 2 phút theo cue card/theme; mở ý → phát triển → ví dụ → kết; mỗi tuần 1 recording + 3 lỗi lặp vào error-log.
- Từ vựng tích lũy: ~140-160 core words/tháng + 1 nửa từ đến từ đọc-nghe. Tổng tích lũy cuối phase ~3.500 (theo `implementation_plan_new.md`).
- **Không cam kết band IELTS** (xem "Lựa chọn còn thiếu" ở cuối file).

### 1.1 Prerequisites để bắt đầu (P0 theo phase-3-spec)
| # | Prerequisite | Trạng thái | Ghi chú |
|---|---|---|---|
| P0-1 | Hoàn thành Phase 2 (test-24 ≥ ngưỡng đã chốt) | Theo HANDOFF: Phase 2 ✅ 158 file, test-24 = PHASE 2 FINALE | Người học cần xác nhận đã qua test-24 + điểm số + mẫu nói/viết gần nhất |
| P0-2 | Có baseline Phase 3 (1 reading 500-800 từ, 1 dictation 10-15', 1 speaking 2', 1 essay 200 từ, 1 grammar/vocab mini-test) | **Chưa có** trong workspace | Cần owner tạo trước khi viết đơn vị mẫu cho P3-M2 |
| P0-3 | Quyết định: học dùng thật (mặc định HANDOFF) hay luyện IELTS exam | **Chưa chốt** | Thiếu sẽ ảnh hưởng M2 trở đi (writing/speaking rubric) |
| P0-4 | Error-log canonical | HANDOFF ghi dùng `Phase-1/error-log.md`; Phase 2 có file riêng | Chốt 1 nơi; phase-3-spec đánh dấu cần xác nhận |
| P0-5 | `implementation_plan_new.md` phục hồi/xác nhận | Có file; phase-3-spec §1 ghi "thiếu" theo README cũ nhưng đợt này đã thấy tồn tại 552 dòng | Đã reconcile; dùng làm nguồn kế thừa |
| P0-6 | Ngưỡng test thống nhất (60% hay 70% cho cycle, 70% cho tổng?) | HANDOFF §3c ghi ≥60% cycle, ≥70% tổng; roadmap ghi ≥70% finale | Đề xuất giữ HANDOFF §3c: cycle ≥60%, tổng ≥70% |

### 1.2 Kỹ năng từng tháng

| Tháng | Theme (HANDOFF §4) | Grammar chính | Kỹ năng tăng trọng | Đầu ra quan sát được |
|---|---|---|---|---|
| **P3-M1** | Personality Deep Dive — Big Five, critique MBTI, traits vs states | Relative Clauses (defining/non-defining) | Đọc phân tích, mô tả/so sánh, câu phức | Reading 600-700 từ + essay 200-250 từ + độc thoại 2 phút "A person I understand" |
| **P3-M2** | Communication & Language — NVC, framing, lời nói đổi ý nghĩa | Reported Speech + reporting verbs | Paraphrase, attribution, nghe ý theo ngữ cảnh | Bài tường thuật lại 1 cuộc hội thoại 200 từ + speaking giải thích quan điểm |
| **P3-M3** | Regret & Counterfactual Thinking — những con đường không đi | Third + Mixed Conditionals + Wish/If only | Lập luận điều kiện, cause/effect, phản biện nhẹ | Essay counterfactual 200-250 từ + 2-min "A choice I would redo" (EF SET retest nếu đến lịch) |
| **P3-M4** | The Good Life — positive psychology, flow, meaning (Seligman, Csikszentmihalyi) | Gerunds & Infinitives + tổng ôn | Tích hợp 4 kỹ năng, tự sửa lỗi | Phase 3 finale: essay + speaking tổng hợp + test-24 (PHASE 3 FINALE) |

### 1.3 Từ vựng & ngữ pháp — quy ước phase 3

- **Core vocabulary/tháng: ~140-160 từ** (theo HANDOFF §4 "Con số chuẩn theo phase"). Cấu trúc: 3 file part1/2/3 (8 từ/file = 24 từ) + 4 file cycle (3 nhóm × 8 từ = 24 từ) + 1 file review (24 từ ôn từ cycle) = hạ tầng tối đa 96-120 core, phần còn lại tăng từ input.
- **Từ đến từ đọc-nghe (extensive):** Từ P3 trở đi, **một nửa** từ mới đến từ đọc + nghe (HANDOFF §4 footnote). Không nhồi Anki; chỉ thêm từ lặp lại ≥2 lần HOẶC từ chặn hiểu bài. Front Anki: nghĩa Việt; Back: từ + IPA + câu ví dụ + tag P3-Mx + theme.
- **Anki overload rule (giữ từ Phase 1):** nếu review >30 phút HOẶC >150 due → giảm new cards về 0-4/ngày, dọn review trước. **Không ép quota.**
- **Ngữ pháp:** 5 bài/tháng × 3 phần A/B/C (3 ngày học/cycle) + 1 bài workshop tích hợp (M1: phân tích câu phức, M2: tường thuật nâng cao, M3: workshop điều kiện, M4: workshop gerund/infinitive + tổng ôn).

### 1.4 Độ khó & tải học

| Chỉ số | P3-M1 | P3-M2 | P3-M3 | P3-M4 (finale) |
|---|---|---|---|---|
| Từ mới core | ~140-160 | ~140-160 | ~140-160 | ~120 (chỉ ôn) + collocations bổ sung |
| Độ dài bài đọc | 500-700 từ | 600-800 từ | 600-800 từ | 700-800 từ (kèm final review) |
| Nguồn nghe | TED-Ed, 6-Minute English (BBC) | TED 10-12', BBC 6min | TED 10-15', Psychology in 10 Min | TED 12-15', 1 podcast tâm lý |
| Viết | đoạn 150-200 từ | essay 200-250 từ | essay 200-250 từ | essay 250 từ (finale) + reflection 200 từ |
| Nói | độc thoại 1.5-2' + cue card thử | độc thoại 2' + tường thuật lại hội thoại | độc thoại 2' + điều kiện ngược | độc thoại 2-2.5' (finale) |
| Dictation mục tiêu | ≥70% | ≥72% | ≥73% | ≥75% (đỉnh phase) |
| Graded reader | L4 | L4-5 | L5 | L5 |

### 1.5 Ôn tích lũy
- **Mỗi cycle (3 ngày):** mở đầu Slot 1 dành 10 phút retrieval grammar cycle trước; cuối Slot 2 dành 5 phút ôn nhanh 8-10 từ cũ.
- **Mỗi tháng (cuối C6):** test-12 và test-24 (TỔNG) quét toàn bộ grammar + từ tháng.
- **Xuyên phase:** spaced repetition qua Anki + 1 graded reader/tuần + 1 bài viết cuối tuần submit AI.
- **Mục tiêu retention (theo dõi nhưng không cam kết):** recall nghĩa Việt → Anh ≥75% sau 30 ngày.

### 1.6 Kiểm tra đầu/cuối phase
- **Đầu phase (D1):** baseline mini-test 15-20 phút: 10 câu grammar B1 (bao quát), 20 từ Phase 2, 1 đoạn đọc 400 từ, 1 dictation 5 câu Phase 2. Ghi `roadmap.md` §1 và tracker riêng. **Baseline không tính điểm pass/fail** — chỉ để đo tiến bộ.
- **Giữa phase:** test-12 (D12) = TEST TỔNG #1 = 90-120 phút, ngưỡng ≥70% (HANDOFF §3c).
- **Cuối phase:** test-24 (D24) = TEST TỔNG #2 + PHASE 3 FINALE = 150 phút, ngưỡng ≥70% (HANDOFF §4-§3c). Test-24 **PHẢI** có: (a) phần tổng ôn 4 tháng, (b) nhắc retest EF SET, (c) nghe lại ghi âm Ngày 0 (mục 2 của `roadmap.md`).
- **Test phụ:** test-01-retry.md (chỉ test-01 có retry) + 1 retake nếu <60% thêm 1 lần cho cycle bất kỳ (ghi rõ trong daily-guide).
- **Cách chấm:** mọi test có đáp án đầy đủ ở cuối file. Điểm = tổng đúng / tổng câu. Không claim "tương đương IELTS X" trong test.

---

## 2. Phase 4 — Upper-Intermediate (B2 → C1), 4 tháng (tháng 13-16)

### 2.0 Mục tiêu cuối phase
- Đọc bài Academic-style 800-1.000 từ về tâm lý; trả lời ý chính + suy luận đạt ≥70% mà không tra từ liên tục.
- Viết essay 250-300 từ có thesis rõ, ví dụ cụ thể, cohesive devices, nominalization đúng chỗ; 1 bài/tuần chấm theo band descriptors (AI chấm + tự sửa theo rubric).
- Nghe TED 15-20' không sub, dictation ≥75-80%, paraphrase được 5-7 ý chính.
- Nói hội thoại 10-15' với AI + 1 buổi/tuần với người thật (Tandem/HelloTalk); ghi recording, 3 lỗi/tuần.
- Từ vựng: Academic Word List + collocations; tổng tích lũy cuối phase ~5.500-6.500 (receptive).
- **Không cam kết band IELTS.**

### 2.1 Prerequisites
- Phase 3 finale ≥70% (HANDOFF).
- Baseline Phase 4: 1 bài 800 từ, 1 dictation 15', 1 speaking 10', 1 essay 250 từ, mini grammar B2 (mixed conditionals, inversion, cleft).
- Đã có thói quen extensive reading ≥30'/ngày (Phase 3 xây).
- Anki: thẻ P3 ôn ổn định, review không vượt 30'/ngày.

### 2.2 Kỹ năng từng tháng

| Tháng | Theme (HANDOFF §4) | Grammar chính | Kỹ năng tăng trọng | Đầu ra |
|---|---|---|---|---|
| **P4-M1** | The Unconscious & Dreams — Freud/Jung ở chiều sâu, biểu tượng | Advanced Passive + Causative (have sth done) | Đọc 800-1.000 từ academic, paraphrase kết quả nghiên cứu | Essay 250 từ "What the unconscious does" + speaking giải thích 1 thuật ngữ |
| **P4-M2** | Persuasion & Rhetoric — rhetoric, propaganda, manipulation vs influence | Inversion + Cleft/Emphatic structures | Phân tích lập luận, nghe ý chính ngược | Essay 250-300 từ phân tích 1 quảng cáo/bài phát biểu + speaking phản biện 5' |
| **P4-M3** | Moral Psychology — trolley problem, moral intuitions, Kohlberg | Mixed Conditionals + Subjunctive | Lập luận đạo đức, counter-argument | Essay 300 tở "A moral dilemma I'd defend" + 1 mock speaking part 3 (5') (EF SET retest nếu đến lịch) |
| **P4-M4** | The Examined Mind — metacognition, critical thinking | Nominalization + Academic style + tổng ôn | Tích hợp 4 kỹ năng + tự sửa | Phase 4 finale + reflection 250 từ về 4 tháng |

### 2.3 Từ vựng & ngữ pháp
- Core: ~160-180 từ/tháng, **ưu tiên AWL + collocations** (theo HANDOFF §4 "Con số chuẩn theo phase"). File vocabulary tăng từ part đơn → part có collocations + register note (formal/neutral/informal).
- **Ngữ pháp M3 trùng P3-M3** (Mixed Conditionals) theo HANDOFF §4: M3 Phase 3 đã dạy, M3 Phase 4 **mở rộng** (subjunctive, formal wishes). Tránh lặp y nguyên — bài P4-M3 tập trung phân biệt **formal/academic style**, không dạy lại từ đầu.

### 2.4 Độ khó & tải học

| Chỉ số | P4-M1 | P4-M2 | P4-M3 | P4-M4 (finale) |
|---|---|---|---|---|
| Từ mới core | ~160-180 | ~160-180 | ~160-180 | ~120 (ôn + collocations) |
| Độ dài bài đọc | 800-900 từ | 800-1.000 từ | 800-1.000 từ | 900-1.000 từ |
| Nguồn nghe | TED 12-15' | TED 15-20' | TED 15-20', Hidden Brain | TED 15-20', 1 podcast dài |
| Viết | essay 250 từ | essay 250-300 từ | essay 300 từ | essay 300-350 từ (finale) + reflection |
| Nói | hội thoại 8-10' | hội thoại 10-12' | hội thoại 10-15' + mock part 3 | hội thoại 15' (finale) |
| Dictation mục tiêu | ≥75% | ≥77% | ≥78% | ≥80% |
| Graded reader | L5 + 1 bài báo Psychology Today | bài báo tâm lý thật | bài báo + excerpt sách | bài báo + sách tâm lý popular |

### 2.5 Ôn tích lũy
- 10 phút đầu Slot 1 mỗi cycle: retrieval 1 cấu trúc grammar cycle trước.
- Cuối tuần: 1 essay submit AI (chấm theo 4 tiêu chí: Task Response, Coherence, Lexical, Grammar) + tự sửa 1 bài cũ.
- Cuối tháng: 1 reading dài kèm summary 200 từ.
- Anki: thẻ mới tối đa 8/ngày; review cap 30'/ngày.

### 2.6 Kiểm tra đầu/cuối phase
- **Đầu phase:** baseline 25-30 phút: 15 câu grammar B2, 25 từ Phase 3, 1 đoạn đọc 700 từ, 1 dictation 10'. Ghi tracker.
- **Giữa phase:** test-12 = TEST TỔNG #1, 120 phút, ≥70% (giữ HANDOFF §3c; Phase 4 chưa nâng ngưỡng vì vẫn là tự học dùng thật).
- **Cuối phase:** test-24 = PHASE 4 FINALE, 150-180 phút, ≥70% + retest EF SET (theo HANDOFF §4 "Retest 5 (sau ~15 tháng)").
- **Mock test IELTS** (nếu người học chốt sẽ thi): **module tùy chọn**, không bắt buộc — Phase 4 chưa thêm mock để tránh trộn mục tiêu.

---

## 3. Phase 5 — Mastery & Vận Dụng Thật (C1), ~3 tháng (tháng 17-19+)

### 3.0 Mục tiêu cuối phase
- Đọc sách tâm lý nguyên bản (Levine/Heller *Attached*; The School of Life…) 1 cuốn trong 3-4 tuần; đọc article dài không cần từ điển thường xuyên.
- Viết bài blog/reflection 400-500 từ về tâm lý; 1 bài/tuần chấm theo band descriptors + tự sửa.
- Nghe podcast 20-30' không sub (Hidden Brain, TED long-form), hiểu ≥90% ý chính.
- Nói 15-20' với người thật 1 lần/tuần (Tandem/HelloTalk) + AI voice hàng ngày 5-10'.
- Tổng tích lũy: ~6.000-7.000 từ receptive (theo `implementation_plan_new.md`).
- **Không cam kết band IELTS.** Mục tiêu là dùng thật; nếu người học muốn đo band, dùng EF SET retest + IELTS Progress Check (British Council, có phí — tùy chọn theo `implementation_plan_new.md` §"App & Website").

### 3.1 Prerequisites
- Phase 4 finale ≥70% + baseline Phase 5.
- Có 1 cuốn sách tâm lý sẵn (mua/borrow/Kindle) hoặc chốt nguồn miễn phí (VD Open Library, Project Gutenberg cho sách public domain).
- Thói quen extensive reading 30-45'/ngày đã ổn định (P3+P4 xây).
- Anki: thẻ P1-P4 ôn tự động; new cards = 0 (chỉ thêm collocations hữu ích từ sách).

### 3.2 Kỹ năng từng tháng

| Tháng | Theme (HANDOFF §4) | Cấu trúc | Đầu ra |
|---|---|---|---|
| **P5-M1** | Book Club: đọc sách thật (Levine/Heller *Attached* hoặc sách The School of Life) | KHÁC cấu trúc: `book-club/NN-chapters.md` + hướng dẫn đọc + từ vựng từ sách; không grammar mới | Đọc 1 cuốn; 1 summary/chương (200-300 từ) + 1 thảo luận 15' |
| **P5-M2** | The Psychologist's Toolkit — viết luận 400-500 từ, thảo luận dài, deep conversation | Application (không grammar mới; tổng ôn qua dùng) | 1 essay 400-500 từ/tuần + 1 hội thoại 15-20' với AI + 1 buổi người thật |
| **P5-M3** | Final Project + tổng kết | Application | **PHASE 5 FINALE = TOÀN KHÓA HỌC:** EF SET cuối + nghe lại ghi âm Ngày 0 + reflection 300 từ + graduation checklist |

### 3.3 Từ vựng & ngữ pháp
- **Không grammar mới** (HANDOFF §4 #12-14). Mọi tháng P5 là application — người học đã đủ cấu trúc để viết/nói ở C1.
- Từ vựng: 0 mới core, thay bằng từ tích lũy từ sách + 5-10 collocations mới/tuần ghi vào Anki deck `P5-from-book`.
- Mỗi tháng: 1 bài viết submit AI + chấm theo band descriptors (nếu có chốt thi) hoặc theo 4 tiêu chí chung (Task Response, Coherence, Lexical, Grammar) nếu không thi.

### 3.4 Độ khó & tải học

| Chỉ số | P5-M1 (Book Club) | P5-M2 (Toolkit) | P5-M3 (Finale) |
|---|---|---|---|
| Từ mới core | 0 (chỉ collocations từ sách) | 0 (chỉ collocations từ output) | 0 (chỉ review) |
| Đọc | 1 cuốn sách tâm lý (300 trang/4 tuần) | 1 bài báo dài + 1 chapter sách/tuần | 1 bài tổng hợp + reflection |
| Nghe | podcast 20-30' không sub (2/tuần) | podcast 20-30' không sub (3/tuần) | 1 buổi nghe lại + 1 phân tích |
| Viết | summary 200-300 từ/chương | essay 400-500 từ + reflection | reflection 300 từ + 1 bài cuối |
| Nói | thảo luận 15' với AI/tandem | hội thoại 15-20' với AI + 1 buổi người thật | "graduation talk" 20' với AI |
| Dictation mục tiêu | ≥80-85% | ≥82% | ≥85% |

### 3.5 Ôn tích lũy
- Anki: tất cả thẻ P1-P4 ở trạng thái "mature"; 0 new; review 15-20'/ngày.
- Extensive reading 30-45'/ngày (sách + article).
- Cuối mỗi tuần: 1 bài viết dài submit AI.
- Cuối mỗi tháng: 1 cuộc hội thoại thật (Tandem/HelloTalk hoặc người quen).

### 3.6 Kiểm tra đầu/cuối phase
- **Đầu phase:** "Book readiness" mini-check: chọn 1 chương đầu sách, paraphrase 300 từ; chấm mức hiểu (không tính điểm).
- **Giữa phase:** test-12 = ứng dụng: đọc 1.000 từ academic + essay 400 tở + speaking 15'; ngưỡng ≥70% (giữ chuẩn HANDOFF).
- **Cuối phase:** test-24 = **PHASE 5 FINALE = TOÀN KHÓA HỌC** — gồm:
  1. EF SET retest cuối (ghi `roadmap.md` §1).
  2. Nghe lại ghi âm Ngày 0 (ghi `roadmap.md` §2).
  3. Reflection 300 tở: "I started A1; I'm here."
  4. 1 bài viết cuối 400-500 từ + 1 hội thoại cuối 20' với AI.
  5. Graduation checklist (giữ thói quen Anki, đọc sách, AI voice; retest EF SET 6 tháng/lần).
- **Tùy chọn thi:** nếu người học quyết định thi, **tách thành Phase 6 (luyện đề 2-3 tháng)**, không nhồi vào Phase 5. Phase 5 giữ trọng tâm "dùng thật".

---

## 4. Tiêu chí chuyển phase (gate)

| Gate | Tiêu chí | Đo bằng |
|---|---|---|
| P2 → P3 | test-24 (PHASE 2 FINALE) ≥70% + EF SET retest đã ghi | `roadmap.md` §1 + tracker test |
| P3 → P4 | test-24 (PHASE 3 FINALE) ≥70% + baseline P4 có + Anki ổn | tracker test + Anki stats |
| P4 → P5 | test-24 (PHASE 4 FINALE) ≥70% + 1 cuốn sách tâm lý đã đọc + EF SET retest | tracker + roadmap |
| P5 → "tốt nghiệp" | test-24 (PHASE 5 FINALE) đủ 5 phần + EF SET + nghe lại ghi âm | test-24 file + roadmap |

**Không tự động chuyển phase chỉ vì đủ ngày.** Phải đạt gate + người học tự xác nhận.

---

## 5. Lưu ý pedagogy xuyên 3 phase

1. **Ngữ pháp tăng dần theo trục complexity, không theo đề tài.** Mỗi phase thêm cấu trúc mới ở mức "1 bước khó hơn Phase trước". Ví dụ: P2 (Past Cont + Present Perfect + Past Perfect + Passive) → P3 (Relative + Reported + Third Cond + Gerund) → P4 (Advanced Passive + Inversion + Subjunctive + Nominalization).
2. **Chủ đề tâm lý xuyên suốt** (HANDOFF §3d): mỗi bài đọc, ví dụ từ vựng, bài nói/viết đều xoay quanh khái niệm tâm lý thật (Gottman, Dweck, Bowlby, Freud, Kahneman, Asch, Milgram, Cialdini, Seligman, Csikszentmihalyi, Levine...). P3-M1 thêm Big Five (Costa & McCrae) + critique MBTI (Pittenger).
3. **Tỉ lệ input/output:** 70% input (đọc + nghe) / 30% output (nói + viết) trong P3; 60/40 trong P4; 50/50 trong P5.
4. **Spaced repetition luôn chiếm buổi tối** (15-20' Anki + 15-30' graded reader/sách). Hai việc này quyết định 50% retention theo `implementation_plan_new.md` §Anki.
5. **Mọi file nội dung tiếng Việt** (hướng dẫn) + **tiếng Anh** (bài học, ví dụ, đáp án). Tuyệt đối không CJK (theo quality gate HANDOFF §6.2).
6. **Mọi test phải có đáp án/rubric đầy đủ** (HANDOFF §6.5). Mọi grammar file có phần ✅ ĐÁP ÁN.
7. **Mọi mục tiêu output phải đo được, không mơ hồ.** "Hiểu bài" ≠ "biết". Phase 3+ dùng: số từ đúng, % dictation, số câu đúng, tổng từ viết, số recording/tuần.
8. **Người học có quyền dừng/giảm tải** (theo `implementation_plan_new.md` §"Nhịp Tuần & Ngày Nghỉ"). Ngày tệ = maintenance, không tính đạt ngày; retry/review sau, không bỏ qua.

---

## 6. Lựa chọn còn thiếu (chưa chốt được trong đợt này)

| # | Lựa chọn | Tại sao cần | Thiếu gì | Đề xuất follow-up |
|---|---|---|---|---|
| 1 | **Academic hay General Training** (nếu luyện thi) | Ảnh hưởng rubric writing Task 1 (graph vs letter) và 1 phần vocabulary | Người học chưa xác nhận có thi hay không | Hỏi trước khi viết P3-M2 (M2 bắt đầu Reported Speech + paragraph/essay). Nếu vẫn "chưa thi" → giữ dùng thật, không thêm mock |
| 2 | **Band mục tiêu** (6.5 / 7.0 / khác) | Ảnh hưởng độ sâu từ vựng (Band 7+ cần collocations + paraphrase) | Theo HANDOFF §1 mục tiêu "6.5-7.0" nhưng ghi rõ "học để dùng, chưa thi" | KHÔNG claim band trong tài liệu Phase 3+. Chỉ ghi "tương đương C1 nếu so EF SET" |
| 3 | **Ngày thi dự kiến** | Có ngày thi → cần module luyện đề 2-3 tháng riêng | Người học chưa chốt | Nếu sau này chốt → tách Phase 6 |
| 4 | **Error-log canonical** | 1 nơi ghi lỗi duy nhất | HANDOFF ghi dùng `Phase-1/error-log.md`; Phase 2 có file riêng `Phase-2/error-log.md` | Đề xuất: giữ `Phase-1/error-log.md` (HANDOFF), Phase 2-5 đã có file riêng — chấp nhận 2 nơi, thêm note "xem Phase-1 trước" trong daily-guide P3 |
| 5 | **Ngưỡng test thống nhất** | HANDOFF §3c (60% cycle / 70% tổng) vs `roadmap.md` (70% finale) vs một số test Phase 2 dùng 60% cho cả finale | Số liệu Phase 2 chưa reconcile | Đề xuất: 60% cycle + 70% tổng (giữ HANDOFF). Tổng cuối phase ≥70% = đạt gate |
| 6 | **Tài liệu official IELTS có dùng không?** | Nếu có quyền dùng Cambridge IELTS Official → đa dạng nguồn | HANDOFF mặc định KHÔNG | Hỏi trước P3-M2; nếu có → mở rộng reading/listening pool |
| 7 | **Số file Phase 2 chính xác** | README ghi 157, HANDOFF ghi 158, inventory nói "không khớp" | Đếm tự động từ glob | Kế hoạch P2 của T12: chạy đếm trước khi đóng P0-2 |
| 8 | **Nguồn sách P5-M1** | Cần 1 cuốn tâm lý thật (Levine/Heller *Attached* hoặc sách The School of Life) | Chưa chốt cuốn nào | Đề xuất: *Attached* (Levine & Heller) — ngắn, thiết thực, đúng chủ đề. Xác nhận với người học trước P5-M1 |
| 9 | **Tandem/HelloTalk: có dùng?** | Phase 2+ thêm 1-2 buổi/tuần với người thật | Người học chưa xác nhận | Mặc định "có thể" — không bắt buộc, mỗi tuần đề xuất 1 buổi tùy chọn |
| 10 | **AI voice: ChatGPT hay Gemini?** | Hai voice mode có chất lượng khác nhau | Người học chưa chốt | Mặc định: prompt được viết trung tính, dùng được cả 2 |

---

## 7. Nguồn tham chiếu đã dùng (kế thừa, không mâu thuẫn)

| File | Mục dùng | Có mâu thuẫn? |
|---|---|---|
| `HANDOFF.md` §3-§4-§6 | Cấu trúc batch, quality gates, roadmap 14 batch còn lại | Không |
| `roadmap.md` §4d-h | Phase 1-2 themes, format tracking | Không |
| `implementation_plan_new.md` §Phase 3-5 | Mục tiêu phase, kỹ năng, từ vựng, ngữ pháp | Không (ghi rõ "chưa thi", "học để dùng") |
| `tasks/phase-3-spec-2026-09-15.md` | P0 blockers, mục tiêu đo được, rủi ro | Kế thừa; mục "không cam kết band" giữ vững |
| `tasks/phase-3-file-inventory-2026-09-15.md` | Manifest 40 file/tháng, ownership | Kế thừa; phần "không sửa Phase 4-5" tôn trọng — đợt này chỉ tạo P3-M1-C1 |
| `Resources/psychology-vocabulary.md` | Index concept Phase 0-1 (sẽ mở rộng ở P3+) | Tái sử dụng; chưa mở rộng đợt này |
| `Phase-2/daily-guide-month1.md` + các file mẫu | Format chuẩn (A/B/C, IPA, đáp án, rubric) | Bắt chước cấu trúc, không copy nội dung |

---

> *Phase 4-5 chỉ ở mức thiết kế trong file này. Đơn vị mẫu Phase 3 đầy đủ ở `tasks/phase-3-unit-01-sample/`. Bản đồ 24 cycle của Phase 3 ở `tasks/phase-3-map-2026-09-17.md` (bản chính thức; `tasks/phase-3-detailed-map-2026-09-17.md` là bản nháp trước đó, một số claim số file ở §9 chưa khớp thực tế — đã hiệu chỉnh trong bản map mới).*
