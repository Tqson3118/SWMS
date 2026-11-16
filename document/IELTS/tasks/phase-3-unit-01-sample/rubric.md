# 📏 RUBRIC — Chấm Writing & Speaking (đơn vị mẫu 3.1)

> Dùng cho mọi bài viết/ghi âm trong C1 (đoạn 120-200 từ, độc thoại 2') và cho Q7 của quiz. Thang 10đ (2.5đ/tiêu chí). Chấm thẳng thắn — rubric là gương, không là roi.

## A. WRITING (đoạn/essay C1)

| Tiêu chí | 2.5đ (tốt) | 1.5đ (đạt) | 0.5đ (yếu) |
|----------|------------|------------|-------------|
| **1. Content & Task** | Đúng đề, có ý chính rõ, ≥ số từ/từ vựng yêu cầu (VD ≥4 từ mới, ≥2 mệnh đề) | Đúng đề nhưng thiếu 1 yêu cầu | Lạc đề hoặc bỏ quá nửa yêu cầu |
| **2. Organization** | Có mở-thân-kết rõ; ý nối tự nhiên | Có đoạn nhưng ý nhảy | Mạch không rõ |
| **3. Language (grammar mục tiêu)** | Relative clauses đúng ≥ số yêu cầu; lỗi ngoài chủ điểm không cản hiểu | 1-2 lỗi relative clause nhưng câu vẫn hiểu | Lỗi relative clause khiến hiểu sai |
| **4. Vocabulary** | Dùng đúng ngữ cảnh + collocation ổn; lặp từ ít | Đúng nghĩa nhưng lặp/cơ bản | Sai nghĩa hoặc quá ít từ mới |

**Tổng /10.** Quy ước C1: ≥6/10 đạt; 4.5-5.5 viết lại 1 lần theo nhận xét; <4.5 xem lại đúng slot tương ứng.

### Checklist tự sửa trước khi chấm
- [ ] Mọi who/which/that đều đứng sau danh từ nó bổ nghĩa?
- [ ] Sau dấu phẩy có "that" không? (phải KHÔNG)
- [ ] whose + danh từ (không đứng một mình)?
- [ ] Đủ số từ/từ vựng đề yêu cầu? (đếm thật)
- [ ] Đọc to 1 lần — chỗ nào nghẹn là chỗ cần sửa.

## B. SPEAKING (độc thoại 2 phút)

| Tiêu chí | 2.5đ | 1.5đ | 0.5đ |
|----------|------|------|------|
| **1. Content & Task** | Trả lời đủ 3-4 gợi ý của cue card, có ví dụ cụ thể | Thiếu 1 gợi ý | Chỉ liệt kê, không ví dụ |
| **2. Organization** | Mở-triển-kết nghe ra; có từ nối (because, for example, so) | Bị ngắt quãng, thứ tự lộn | Không có cấu trúc |
| **3. Language** | ≥ số relative clauses yêu cầu, đúng thì | 1-2 lỗi nhưng không cản hiểu | Lỗi cản hiểu |
| **4. Delivery & Fluency** | Nói liên tục 2', dừng >5 giây ≤1 lần; phát âm đủ rõ để AI hiểu | Dừng 2-4 lần | Dừng thường xuyên, phải dừng lại thu |

**Tổng /10.** Q7 của quiz chấm bằng rubric này (×2.5 mỗi tiêu chí → quy về 10đ quiz).

## C. Prompt chấm bằng AI (copy-paste)

> Dùng đúng quy trình prompt 2 trong `Resources/ai-conversation-guide.md` (sửa lỗi). Với bài viết, dán prompt sau + bài của bạn:

```
You are an English teacher for a Vietnamese B1->B2 learner. The unit topic is
defining relative clauses (who/which/that/whose/where/when, defining vs
non-defining). Grade my text below.

1. Give a score out of 10 using this rubric: Content & Task (2.5),
   Organization (2.5), Language (2.5), Vocabulary (2.5). Show sub-scores.
2. List EVERY error in a table: my sentence -> corrected sentence -> error type
   (grammar/vocabulary/spelling/word choice).
3. Specifically check: relative clause placement, comma + that (must not exist),
   whose + noun, missing subjects.
4. Point out 3 habits I repeat the most.
5. Do NOT rewrite the whole text; only show corrections.
6. Do NOT give me any IELTS band score — I am not preparing for a test.

My text: "[dán bài vào đây]"
```

> Với speaking: dùng prompt 2 của `ai-conversation-guide.md` trên transcript (tính năng chuyển giọng nói thành văn bản) hoặc dán đoạn ghi âm; yêu cầu 3 lỗi theo 4 nhóm: grammar / vocabulary / pronunciation / fluency.

## D. Sau khi chấm — vòng lặp lỗi

1. Mỗi lỗi → 1 dòng vào `Phase-1/error-log.md` (nơi dùng chung theo HANDOFF; bạn đang dùng file phase riêng thì ghi vào đó): *lỗi → câu đúng → ngày*.
2. Chọn **top 3 lỗi lặp** cuối tuần → tự viết 3 câu đúng.
3. Bài viết dưới ngưỡng: sửa từng lỗi (không viết lại toàn bộ) → nộp lại AI chấm lần 2. Lần 2 tính điểm chính thức.
