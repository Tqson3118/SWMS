# 🔍 HANDOFF-REVIEW — Kiểm Tra Chất Lượng 1 Phase

> **File này dành cho AI executor ở session review.** Nhiệm vụ: AUDIT sâu 1 phase tài liệu, tìm và SỬA mọi lỗi, xác nhận phase đạt chuẩn trước khi người học sử dụng. Đọc `HANDOFF.md` (mục 1-3) trước để hiểu bối cảnh dự án — file này chỉ nói về việc KIỂM TRA.

---

## 0. CÁCH DÙNG

1. Xác định phase cần review (người học sẽ nói, VD: "review Phase 2").
2. Chạy lần lượt 7 phép kiểm tra dưới đây (mục 2 → mục 8) — mỗi phép có code chạy được ngay trong run_code.
3. Mọi lỗi phát hiện: SỬA NGAY bằng edit (không chỉ liệt kê), trừ lỗi cần quyết định người học (ghi lại câu hỏi).
4. Cập nhật **mục 9** (bảng trạng thái review) của file này sau khi xong.
5. Báo cáo cuối: tóm tắt lỗi tìm thấy + đã sửa + còn treo gì (nếu có).

**Nguyên tắc:** sửa tối thiểu, giữ nguyên giọng văn và cấu trúc gốc. KHÔNG viết lại nội dung nếu không có lỗi.

---

## 1. PHẠM VI & TIÊU CHUẨN PASS

**Review 1 phase = review từng tháng (batch) trong phase đó + giao diện liên phase.**

| # | Phép kiểm | Ngưỡng pass |
|---|-----------|-------------|
| 1 | Cấu trúc file đủ theo chuẩn ~40 file/tháng | Thiếu ≤ 0 file bắt buộc |
| 2 | Không ký tự CJK lọt vào tiếng Việt | 0 file lỗi |
| 3 | Số từ vựng thực tế khớp mọi con số claim trong guide/test/roadmap | 0 sai lệch |
| 4 | Tham chiếu file tồn tại thật | 0 tham chiếu gãy thật (trừ same-folder hợp lệ) |
| 5 | Mọi test có ĐÁP ÁN, mọi grammar có phần đáp án | 0 thiếu |
| 6 | Tracker nhất quán (roadmap có đủ bảng tháng, README/plan đúng trạng thái) | 0 lệch |
| 7 | Đọc thử 3 file ngẫu nhiên — chất lượng nội dung đúng cấp độ, tiếng Anh chuẩn | 0 lỗi nặng |

---

## 2. KIỂM TRA CẤU TRÚC FILE

Chạy cho từng tháng (thay monthN). Chuẩn mỗi tháng: 1 guide + 5 grammar + 8 vocabulary + 8 reading + 8 listening + 1 speaking + 2 writing + 7 tests (6 test + 1 retry) = **40 file**.

Đặt `PHASE = "Phase-2"` rồi đếm theo thư mục. Nếu thiếu file: xem daily-guide tháng đó tham chiếu gì → tạo file thiếu theo mẫu (mục 8 trong HANDOFF.md) hoặc sửa tham chiếu nếu file thừa tên khác.

**Lưu ý cấu trúc đặc biệt:** P5 các tháng Book Club/Application có cấu trúc khác (book-club/) — chấp nhận nếu daily-guide tự nhất quán.

---

## 3. KIỂM TRA CJK (ký tự Hán lọt vào tiếng Việt)

Lỗi phổ biến nhất của dự án — đã phải sửa 5+ lần. Chạy:

```javascript
const g = await tools.glob({ pattern: "Phase-N/**/*.md" });
const all = g.paths.map(p => p.replace(/\\/g, "/"));
for (const p of all) {
  const r = await tools.grep({ path: p, pattern: "[\\u4e00-\\u9fff]" });
  if (r.matches.length) {
    console.log("LỖI:", p);
    for (const m of r.matches) console.log("  L" + m.lineNumber + ": " + m.line.slice(0, 90));
  }
}
```

**Cách sửa:** đọc dòng lỗi → thay cụm CJK bằng tiếng Việt tương đương (thường là 1-3 từ bị gõ nhầm sang bộ gõ Trung). KHÔNG xóa cả dòng.

---

## 4. ĐẾM TỪ VỰNG vs CLAIM

Từ vựng nằm ở dòng bảng bắt đầu `| **`. Có 2 format:

```javascript
const R1 = /^\| \*\*(.+?)\*\* \/(.+?)\/ \| (.+?) \| (.+?) \| (.+?) \|$/;  // Phase-2+ chuẩn
const R2 = /^\| \*\*(.+?)\*\* \| \/(.+?)\/ \| (.+?) \| (.+?) \| (.+?) \|$/;  // Phase-0 cũ
```

Đếm từng file vocabulary → tổng tháng. Sau đó grep các chỗ claim số ("~150 từ", "120 từ", "ôn N từ", "≥N/120"...) trong daily-guide + tests + roadmap tháng đó → đối chiếu.

**Quy tắc sửa:** sửa CLAIM theo SỐ THẬT (đừng thêm từ cho khớp claim). Nếu chênh >20% thì cân nhắc: đó là lỗi thiết kế → báo người học.

---

## 5. KIỂM TRA THAM CHIẾU FILE

Mọi `ten-file.md` trong backtick phải tồn tại. Ba dạng hợp lệ:
1. Full path từ root: `Phase-2/grammar/01-....md`
2. Same-folder (file cùng thư mục): test nhắc `reading/cycle3-reading.md` — OK nếu tồn tại `Phase-2/reading/cycle3-reading.md`
3. Tên file trần trong cùng folder (hiếm)

```javascript
const fileSet = new Set(allFiles);
const broken = [];
for (const f of allMdFiles) {
  const text = await readFull(f); // đọc theo chunk 2000 dòng
  const refRe = /`([A-Za-z0-9\-\/\.]+\.md)`/g;
  let m;
  while ((m = refRe.exec(text)) !== null) {
    const ref = m[1];
    // thử resolve từ root, từ thư mục cha của f, từ các subfolder phổ biến
    const cands = [ref,
      dir(f) + "/" + ref,
      dir(f) + "/" + ref.replace(/^(tests|reading|listening|vocabulary|grammar|writing|speaking)\//, x => x),
      root(f) + "/" + ref];
    if (!cands.some(c => fileSet.has(c))) broken.push(f + " → " + ref);
  }
}
```

**False positive đã biết (bỏ qua):** tham chiếu bare-name trong cùng thư mục (test nhắc `test-01-retry.md` nhưng resolver không thử cùng folder tests/). Chuẩn hóa luôn về dạng có prefix folder khi sửa.

---

## 6. KIỂM TRA TEST & GRAMMAR ĐỦ ĐÁP ÁN

```javascript
// Mọi file trong tests/ phải chứa "ĐÁP ÁN"
// Mọi file trong grammar/ phải chứa "ĐÁP ÁN" (hoặc phần đáp án cuối)
for (const p of allFiles.filter(x => /\/tests\//.test(x))) {
  const text = await readFull(p);
  if (!/ĐÁP ÁN|Đáp án/.test(text)) console.log("THIẾU ĐÁP ÁN:", p);
}
```

Kiểm tra thêm bằng mắt (đọc 1-2 test ngẫu nhiên): đáp án có ĐÚNG không (so từng câu), điểm có cộng đủ 100, số test có LIÊN TỤC trong phase.

---

## 7. KIỂM TRA TRACKER & GIAO DIỆN LIÊN PHASE

1. `roadmap.md`: mỗi tháng của phase có bảng theo dõi riêng (mục 4x...)?
2. `README.md`: cột trạng thái phase đúng thực tế?
3. `implementation_plan_new.md`: block "Batch N ĐÃ TẠO" cuối cùng khớp phase đang review?
4. `Resources/anki-master-vocabulary.csv`: có nên cập nhật với từ vựng phase mới không? (chỉ khi user yêu cầu — file master hiện dừng ở Phase 1; nếu cập nhật thì trích xuất lại theo format cũ Front/Back/Tags).
5. Daily-guide cuối tháng có nhắc retest EF SET đúng chu kỳ 3 tháng không?

---

## 8. ĐỌC THỬ CHẤT LƯỢNG (human-eye check)

Chọn ngẫu nhiên 3 file đọc kỹ:
- 1 file grammar: bài tập có giải được không? đáp án đúng không? (tự giải 2-3 câu để soi)
- 1 file reading: tiếng Anh có chuẩn không, độ dài đúng cấp độ phase không (đếm ~từ), khái niệm tâm lý có đúng không (Freud/Kahneman bị gán sai công lao là lỗi nặng).
- 1 file vocabulary: IPA đúng không (spot-check 3 từ trên Forvo), câu ví dụ có dùng đúng grammar tháng đó không.

Lỗi nặng (khái niệm tâm lý sai, tiếng Anh sai ngữ pháp trong bài học) → sửa trực tiếp. Lỗi nhẹ (giọng văn) → bỏ qua.

---

## 9. TRẠNG THÁI REVIEW (⬅️ CẬP NHẬT SAU MỖI LẦN)

| Phase | Ngày review | Người/khung review | Lỗi tìm thấy | Đã sửa | Ghi chú |
|-------|-------------|--------------------|--------------|--------|---------|
| Phase-0 | 2025-01 (audit tổng) | session chính | 4 nhóm lỗi (số từ, 44 âm, ref, plan cũ) | ✅ hết | đã fix trong audit trước |
| Phase-1 | 2025-01 (audit tổng) | session chính | ref gãy 12 chỗ + claim ~150 từ + cây plan | ✅ hết | đã fix trong audit trước |
| Phase-2 | 2026-08-27 | AI agent (auto-review) | 0 lỗi nặng, 1 vocab claim lệch | ✅ pass; ⚠️ vocab claim cần user quyết | 157 file, 4 tháng, roadmap đủ |
| Phase-3 | — | | | | chưa tạo |
| Phase-4 | — | | | | chưa tạo |
| Phase-5 | — | | | | chưa tạo |

> Phase 0-1 đã qua audit tổng tháng 1/2025 (cross-ref, CJK, claim số, tracker) — đã sửa sạch. Nếu review lại: tập trung mục 8 (chất lượng nội dung) vì phần máy đã pass.

---

## 10. SAU KHI REVIEW XONG

1. Cập nhật bảng mục 9.
2. Nếu phát hiện lỗi thuộc về PATTERN (VD: executor sau lặp lại lỗi claim số) → thêm 1 dòng vào mục 7 của `HANDOFF.md` để session tạo tài liệu sau tránh lặp.
3. Báo cáo người học: "Phase N đã review: X lỗi đã sửa, Y kiểm tra pass. [ghi chú]".

---

> *Review không phải để tìm việc — mà để người học mở file nào cũng tin được nội dung. Mỗi lỗi bắt được là một lần người học khỏi hoài nghi tài liệu.*