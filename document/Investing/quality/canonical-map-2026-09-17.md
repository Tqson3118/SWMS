# Canonical Map — 2026-09-17

**Cập nhật:** 2026-09-17 (phiên `learning-readiness-current`, owner T11 — Investing)
**Nguồn cũ:** `quality/canonical-map-2026-09-16.md` (giữ nguyên, không sửa)
**Phạm vi bản này:** cập nhật/bổ sung mục 2 (duplicate W15/W16), mục 3 (canonical W18) và mục 9 mới (22 thư mục `module-*` tên lỗi encoding trong `notes/`). Các mục khác giữ nguyên nội dung từ bản 2026-09-16.

> [!IMPORTANT]
> Tất cả thư mục/tệp legacy được giữ chủ ý — KHÔNG xóa. Bản này chỉ cập nhật canonical map; mọi hành động xóa/đổi tên cần quyết định riêng từ owner T11 và T07.

---

## 1. Cấu trúc thư mục notes/ (giữ nguyên từ 2026-09-16)

### Thư mục canonical (source of truth)
| Thư mục | Tuần | File chính |
|---|---|---|
| `notes/module-1/` | W01–W04 | week-01-*.md … week-04-*.md |
| `notes/module-2/` | W05–W10 | week-05-*.md … week-10-*.md |
| `notes/module-3/` | W11–W14 | week-11-*.md … week-14-*.md |
| `notes/module-4/` | W15–W18 | week-15-*.md … week-18-ips-ca-nhan.md |

### Thư mục legacy trong `notes/module-4/`
| File | Trạng thái | Bản canonical (điểm nhập) |
|---|---|---|
| `week-15-behavioral-finance.md` | Legacy/duplicate, giữ nguyên | `week-15-behavioral-finance-herd-fomo-loss-aversion-anchoring.md` |
| `week-16-crypto-digital-assets.md` | Legacy/duplicate, đã có marker `> [!NOTE] **Legacy duplicate**` ở dòng 1–3 trỏ về canonical | `week-16-crypto-digital-assets-blockchain-defi-risk-assessment.md` |

**Quy ước chống nhặt nhầm cho importer/app agent:**

- Bài W15 canonical có tên dài (chứa 4 cụm từ khóa `behavioral-finance-herd-fomo-loss-aversion-anchoring`) và là tệp ~184 dòng; bản legacy ngắn (~59 dòng) chỉ có placeholder khái niệm. **Dùng độ dài tệp/đếm mục H2 làm tín hiệu phụ trợ** nếu importer chỉ match theo `week-15-` thay vì canonical-id.
- Bài W16 canonical tên dài (`blockchain-defi-risk-assessment`); bản legacy đã có marker rõ ở đầu tệp — importer nên tôn trọng marker này. Hai tệp nằm cùng `notes/module-4/`.
- Khi gặp cả hai, **canonical = tên dài** cho cả W15 và W16. Ledger chỉ gắn claim ID cho canonical; ledger dòng W15 `[W15-C01/C02/C03]` cũ (legacy) và dòng mới `W15-C01..C08` (canonical) đều có trong `quality/module-3-source-ledger.md`; tuy nhiên bản mới thay thế dòng cũ, nên canonical claim map là `W15-C01..C08` (canonical) chứ không phải `[W15-C01/C02/C03]` (legacy).

### Thư mục module-[tên lỗi encoding] trong `notes/` (MỚI — chi tiết mục 9)

---

## 2. Bản canonical của W15 và W16 (cập nhật)

### W15 canonical
| Artifact | Đường dẫn | Trạng thái | Ghi chú |
|---|---|---|---|
| Lesson (canonical) | `notes/module-4/week-15-behavioral-finance-herd-fomo-loss-aversion-anchoring.md` | Tồn tại (~184 dòng), 12 mục H2, glossary trước abbreviations | Bản dài; đã được review `APPROVED` (re-review 2026-09-16) |
| Lesson (legacy) | `notes/module-4/week-15-behavioral-finance.md` | Tồn tại (~59 dòng) | Legacy/duplicate, KHÔNG dùng để đóng gói, kiểm claim, import; giữ để không xóa nội dung ngoài phạm vi |
| Quiz | `assessments/quizzes/week-15-quiz.md` | Tồn tại (8 câu) | Canonical, gắn LO-W15-* |
| Answer Key | `assessments/answer-keys/week-15-answers.md` | Tồn tại (8 câu, 32 heading option) | Canonical |
| Flashcards | `flashcards/week-15.yaml` | Tồn tại (8 thẻ R5) | Canonical |
| Source Ledger | `quality/module-3-source-ledger.md` (block `W15-C01..C08`) | Tồn tại | Claim ID `W15-C01..C03` active verified; `W15-C04..C05` pending; `W15-C06..C08` illustrative/method |
| Handoff | `handoff/w15-handoff-2026-09-16.md` | Tồn tại | `AUTHOR-UPDATED — VALIDATED` |
| Review | `handoff/w15-review-2026-09-16.md` | REVIEWER-APPROVED (sau re-review) | |

### W16 canonical
| Artifact | Đường dẫn | Trạng thái | Ghi chú |
|---|---|---|---|
| Lesson (canonical) | `notes/module-4/week-16-crypto-digital-assets-blockchain-defi-risk-assessment.md` | Tồn tại (~189 dòng), 12 mục H2, glossary trước abbreviations | Bản dài; đã được review `APPROVED` (re-review 2026-09-16) |
| Lesson (legacy) | `notes/module-4/week-16-crypto-digital-assets.md` | Tồn tại (~63 dòng) với marker `> [!NOTE] **Legacy duplicate**` ở 3 dòng đầu trỏ về canonical | KHÔNG dùng để đóng gói, kiểm claim, import |
| Quiz | `assessments/quizzes/week-16-quiz.md` | Tồn tại (8 câu) | Canonical |
| Answer Key | `assessments/answer-keys/week-16-answers.md` | Tồn tại (8 câu, 32 heading option) | Canonical |
| Flashcards | `flashcards/week-16.yaml` | Tồn tại (8 thẻ R5) | Canonical |
| Source Ledger | `quality/module-4-source-ledger.md` (block `W16-C01..C06`) | Tồn tại | Tất cả active verified qua BIS 2021, FSB 2023, Ethereum.org (smart contract + oracles); IOSCO/NIST vẫn pending |
| Handoff | `handoff/w16-handoff-2026-09-16.md` | Tồn tại | `AUTHOR-UPDATED — VALIDATED` |
| Review | `handoff/w16-review-2026-09-16.md` | REVIEWER-APPROVED (sau re-review) | Canonical = tên dài |

---

## 3. Bản canonical của W17 và W18 (cập nhật W18)

### W17
| Artifact | Đường dẫn | Trạng thái |
|---|---|---|
| Lesson | `notes/module-4/week-17-bat-dong-san-leverage-rental-yield.md` | Tồn tại (26 236 byte) |
| Quiz | `assessments/quizzes/week-17-quiz.md` | Tồn tại (8 câu) |
| Answer Key | `assessments/answer-keys/week-17-answers.md` | Tồn tại (8 câu) |
| Flashcards | `flashcards/week-17.yaml` | Tồn tại (8 thẻ) |
| Source Ledger | `quality/module-4-source-ledger.md` (block `W17-C01..C04`) | Tồn tại |
| Handoff | `handoff/w17-handoff-2026-09-16.md` | Tồn tại |
| Review | `handoff/w17-review-2026-09-16.md` | REVIEWER-APPROVED |

### W18 (cập nhật)
| Artifact | Đường dẫn | Trạng thái | Ghi chú mới |
|---|---|---|---|
| Lesson | `notes/module-4/week-18-ips-ca-nhan.md` | Tồn tại (~25 805 byte), 12 mục H2 + glossary + 2 retrieval details | OK |
| Quiz | `assessments/quizzes/week-18-quiz.md` | Tồn tại (8 câu, 2A/2B/2C/2D phân bố đều) | OK |
| Answer Key | `assessments/answer-keys/week-18-answers.md` | Tồn tại (8 câu, 32 heading option riêng, có Quick Matrix + Deep Distractor + SRS guide) | OK |
| Flashcards | `flashcards/week-18.yaml` | Tồn tại (8 thẻ R5, claim IDs `W18-C02/C03` chỉ trên 2 thẻ) | OK |
| Source Ledger | `quality/module-4-source-ledger.md` (block `W18-C01..C04`) | Tồn tại; **W18-C01 và W18-C04 vẫn pending** (chưa mở/đọc nguồn IPS chính thức/CFA + luật/thuế VN hiện hành) | Quan trọng cho T16 |
| Handoff W18 | `handoff/w18-handoff-2026-09-16.md` | **CHƯA CÓ FILE** — handoff W18 vẫn dẫn tới file không tồn tại trong lesson Mục 13.3 | Xem mục "Việc cần mở" bên dưới |
| Review W18 | `handoff/w18-review-2026-09-16.md` | **CHƯA CÓ FILE** | T16 sẽ là reviewer độc lập đầu tiên |

**Việc cần mở cho T16 (không nằm trong phạm vi T11, chỉ ghi nhận):**
- `quality/canonical-map-2026-09-16.md` đã ghi W18 review là `PENDING (chưa có reviewer độc lập)`. canonical-map 16-09 chưa ghi rõ handoff W18 cũng pending; bản này ghi rõ để khỏi nhầm khi điều phối đọc.
- Nếu T11 tạo handoff W18 thì đó là viết hộ — ngoài phạm vi; chỉ ghi nhận.

---

## 4. Thư mục module-3 — nội dung và liên quan (giữ nguyên)

`notes/module-3/` chứa W11–W14 (Module 3 — Thị trường Việt Nam):
- week-11-thi-truong-viet-nam.md — APPROVED sau re-review
- week-12-tai-khoan-lenh-phi-thue-vn.md — APPROVED sau re-review
- week-13-phan-tich-nganh-viet-nam.md — APPROVED sau re-review
- week-14-trai-phieu-quy-mo-etf-vn.md — APPROVED sau re-review

Không liên quan đến W17–W18 scope; không sửa trong phiên này.

---

## 5. Assessments và Flashcards — tổng quan (giữ nguyên)

- Quizzes: `week-01-quiz.md` … `week-18-quiz.md` (18 canonical) + `quiz-1..4-*.md` (4 legacy)
- Answer Keys: `week-01-answers.md` … `week-18-answers.md` (18 canonical) + `answer-key-1..4-*.md` (4 legacy)
- Flashcards: `week-01.yaml` … `week-18.yaml` — đủ 18 tuần, tất cả tồn tại.

---

## 6. Legacy files (không xóa) — bổ sung W15/W16

- `notes/module-4/week-15-behavioral-finance.md` — Legacy/duplicate W15
- `notes/module-4/week-16-crypto-digital-assets.md` — Legacy/duplicate W16 (đã có marker)
- 22 thư mục `notes/module-*.md` tên lỗi encoding — xem mục 9
- `assessments/quizzes/quiz-1..4-*.md`
- `assessments/answer-keys/answer-key-1..4-*.md`
- `subject/Investing/Module1/W01–W04` (theo progress.md)

---

## 7. Manifests (giữ nguyên, cập nhật đề xuất cho T07 xem t11-investing-report.md)

- `manifest.yaml` — W01
- `manifest-w02.yaml` … `manifest-w08.yaml` — W02–W08
- **W09–W18: chưa có manifest riêng** (không bắt buộc theo contract hiện tại). Bản t11-investing-report.md đề xuất nhóm gói T07 (3 ưu tiên cao + 4 ưu tiên thấp) nếu T07 muốn đẩy nhanh ingestion; nếu không, tiếp tục để W09–W18 chờ ingestion theo nhịp mặc định.

---

## 8. Tình trạng import DB và web (giữ nguyên)

- W01–W18: Chưa có bằng chứng đã nhập DB và chưa học thử thành công trên web.
- Đây là nhiệm vụ của app agent theo progress.md Follow-up section.

---

## 9. 22 thư mục tên lỗi encoding trong `notes/` (MỚI)

Phát hiện trong phiên 2026-09-17: 22 thư mục con trong `notes/` có tên bắt đầu bằng `module-D` hoặc `module-Ch` và phần còn lại chứa ký tự tiếng Việt bị mã hóa sai (hiển thị thành `?` hoặc ký tự lạ trong PowerShell / Windows). 22 thư mục này nằm cạnh 4 thư mục canonical `module-1..4` và được xác nhận qua `Get-ChildItem` 2026-09-17.

**Đề xuất canonical (không xóa, chỉ ghi nhận):**
- Mỗi tên thư mục lỗi có dạng `module-D...` hoặc `module-Ch...` theo sau là câu tiếng Việt về một hiểu lầm phổ biến (ví dụ: "Dùng GDP để suy ra giá cổ phiếu", "Gọi mọi chỉ báo như chắc chắn", "Đa dạng hóa giả vờ…", "Gọi trái phiếu doanh nghiệp là tiền gửi…", v.v.). Đây **không phải lỗi đặt tên** — chúng là tên hiểu lầm phổ biến được lưu cố ý theo nguyên tắc giáo dục (mỗi thư mục đặt tên theo một sai lầm thường gặp).
- Tuy nhiên tên bị mã hóa sai (mojibake) làm PowerShell, Git và một số tool dòng lệnh không đọc được → dễ nhặt nhầm hoặc bỏ sót khi enumerate.
- Đề xuất không xóa nội dung. Khi có thời gian, owner T11/T16 có thể:
  1. Đổi tên thư mục sang dạng ASCII-safe (slug hóa) nhưng **chỉ khi** app agent / importer đã đối chiếu xong; hoặc
  2. Thêm file `__CANONICAL__.md` ở mỗi thư mục ghi rõ tên tiếng Việt gốc, claim ID liên quan, và hướng dẫn "không phải canonical" để chống nhầm.
- Trong phiên này **không thực hiện đổi tên/xóa** (ngoài phạm vi owner T11 và có thể vi phạm "không xóa nội dung").

**Số lượng xác nhận 2026-09-17:** 22 thư mục khớp với thống kê của canonical-map 2026-09-16. Tên đầy đủ bị console hiển thị mojibake; nếu cần thống kê chi tiết cho từng thư mục, dùng `dir /X` (short name 8.3) hoặc tool đọc raw bytes — không nằm trong phạm vi bản này.

**Quy ước chống nhặt nhầm cho importer/app agent:**
- Nếu importer enumerate `notes/` và thấy tên không phải `module-1`, `module-2`, `module-3`, `module-4` thì bỏ qua tệp lesson/quiz/key/deck bên trong — đây là tài liệu tham khảo, không phải lesson canonical.
- Manifest `package:investing:*` chỉ trỏ tới canonical `notes/module-{1,2,3,4}/`; không có manifest nào trỏ tới 22 thư mục tên lỗi.

---

## 10. Tóm tắt thay đổi so với bản 2026-09-16

| Mục | Thay đổi |
|---|---|
| 2 | Bổ sung bảng W15/W16 canonical so với legacy; quy ước "canonical = tên dài". |
| 3 | W18 ledger vẫn pending W18-C01/C04; ghi rõ `handoff/w18-handoff-2026-09-16.md` chưa tồn tại. |
| 9 | Mục mới: 22 thư mục tên lỗi encoding, đề xuất không xóa, ghi rõ hướng dẫn cho importer. |

Các mục còn lại (1, 4, 5, 6, 7, 8) giữ nguyên từ bản 2026-09-16.
