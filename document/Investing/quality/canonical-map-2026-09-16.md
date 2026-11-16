# Canonical Map — 2026-09-16

**Cập nhật:** 2026-09-16 22:50 ICT (sau review độc lập W17 + hoàn thiện W18)

> [!IMPORTANT]
> Bản này là cập nhật dựa trên kiểm tra thực tế tất cả file và thư mục. Các thư mục tên lạ (hiểu lầm phổ biến) là legacy được giữ chủ ý — KHÔNG XÓA.

---

## 1. Cấu trúc thư mục notes/ (toàn bộ)

### Thư mục canonical (dùng làm source of truth)
| Thư mục | Tuần | File chính |
|---|---|---|
| 
otes/module-1/ | W01–W04 | week-01-*.md … week-04-*.md |
| 
otes/module-2/ | W05–W10 | week-05-*.md … week-10-*.md |
| 
otes/module-3/ | W11–W14 | week-11-*.md … week-14-*.md |
| 
otes/module-4/ | W15–W18 | week-15-*.md … week-18-ips-ca-nhan.md |

### Thư mục legacy trong notes/module-4/
| File | Trạng thái |
|---|---|
| week-15-behavioral-finance.md | Legacy/duplicate, giữ nguyên |
| week-16-crypto-digital-assets.md | Legacy/duplicate, giữ nguyên |

### Thư mục module-[hiểu lầm phổ biến] trong notes/
Các thư mục đặt tên theo sai lầm thường gặp. Đây là nội dung module được đặt tên theo hiểu lầm — KHÔNG phải lỗi đặt tên, KHÔNG được xóa. Danh sách đầy đủ: 22 thư mục tên lạ được xác nhận qua list_dir ngày 2026-09-16.

---

## 2. Bản canonical của W17

| Artifact | Đường dẫn | Trạng thái |
|---|---|---|
| Lesson | 
otes/module-4/week-17-bat-dong-san-leverage-rental-yield.md | Tồn tại (26 236 byte) |
| Quiz | ssessments/quizzes/week-17-quiz.md | Tồn tại (8 câu) |
| Answer Key | ssessments/answer-keys/week-17-answers.md | Tồn tại (8 câu) |
| Flashcards | lashcards/week-17.yaml | Tồn tại (8 thẻ) |
| Source Ledger | quality/module-4-source-ledger.md (block W17-C01..C04) | Tồn tại |
| Handoff | handoff/w17-handoff-2026-09-16.md | Tồn tại |
| Review | handoff/w17-review-2026-09-16.md | REVIEWER-APPROVED |

---

## 3. Bản canonical của W18

| Artifact | Đường dẫn | Trạng thái |
|---|---|---|
| Lesson | 
otes/module-4/week-18-ips-ca-nhan.md | Tồn tại (25 805 byte) |
| Quiz | ssessments/quizzes/week-18-quiz.md | Tồn tại (8 câu) |
| Answer Key | ssessments/answer-keys/week-18-answers.md | Tồn tại (8 câu) |
| Flashcards | lashcards/week-18.yaml | Tồn tại (8 thẻ) |
| Source Ledger | quality/module-4-source-ledger.md (block W18-C01..C04) | Tồn tại |
| Handoff | handoff/w18-handoff-2026-09-16.md | Tạo mới phiên 2026-09-16 |
| Review | handoff/w18-review-2026-09-16.md | PENDING (chưa có reviewer độc lập) |

---

## 4. Thư mục module-3 — nội dung và liên quan

notes/module-3/ chứa W11–W14 (Module 3 — Thị trường Việt Nam):
- week-11-thi-truong-viet-nam.md — APPROVED sau re-review
- week-12-tai-khoan-lenh-phi-thue-vn.md — APPROVED sau re-review
- week-13-phan-tich-nganh-viet-nam.md — APPROVED sau re-review
- week-14-trai-phieu-quy-mo-etf-vn.md — APPROVED sau re-review

Không liên quan đến W17–W18 scope; không sửa trong phiên này.

---

## 5. Assessments và Flashcards — tổng quan

Quizzes: week-01-quiz.md … week-18-quiz.md (18 canonical) + quiz-1..4-*.md (4 legacy)
Answer Keys: week-01-answers.md … week-18-answers.md (18 canonical) + answer-key-1..4-*.md (4 legacy)
Flashcards: week-01.yaml … week-18.yaml — đủ 18 tuần, tất cả tồn tại.

---

## 6. Legacy files (không xóa)

- notes/module-4/week-15-behavioral-finance.md — Legacy/duplicate W15
- notes/module-4/week-16-crypto-digital-assets.md — Legacy/duplicate W16
- notes/module-* (22 thư mục tên hiểu lầm) — xác nhận qua list_dir 2026-09-16
- assessments/quizzes/quiz-1..4-*.md
- assessments/answer-keys/answer-key-1..4-*.md
- subject/Investing/Module1/W01–W04 (theo progress.md)

---

## 7. Manifests

- manifest.yaml — W01
- manifest-w02.yaml … manifest-w08.yaml — W02–W08
- W09–W18: chưa có manifest riêng (không bắt buộc theo contract hiện tại)

---

## 8. Tình trạng import DB và web

- W01–W18: Chưa có bằng chứng đã nhập DB và chưa học thử thành công trên web.
- Đây là nhiệm vụ của app agent theo progress.md Follow-up section.
