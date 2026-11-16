# Canonical Map W13–W16 — Module 4 (LifeSkills)

> Tạo ngày: 2026-09-17 — bởi T09 (LifeSkills) trong đợt `learning-readiness-current`.
> Mục đích: xác định file canonical khi có hai bài cùng tuần; chỉ ra sha256; đề xuất file giữ lại. KHÔNG xóa file legacy trong giai đoạn này.

## Tổng quan

| Tuần | File A (đề xuất canonical) | File B (legacy, ngắn hơn) | A unique hơn B? | Đề xuất |
|---|---|---|---|---|
| W13 | `notes/module-4/week-13-time-management.md` | — | — | Giữ nguyên (chỉ một file; đang ở trạng thái skeleton, cần viết lại) |
| W14 | `notes/module-4/week-14-decision-making.md` | — | — | Giữ nguyên (chỉ một file; đang ở trạng thái skeleton, cần viết lại) |
| W15 | `notes/module-4/week-15-networking-social-skills.md` | `notes/module-4/week-15-networking.md` | Có (5908 vs 3435 bytes; có LOs chi tiết, glossary 9 mục, case An cụ thể, 9 mục chuẩn) | Canonical = `week-15-networking-social-skills.md`; legacy giữ để tham chiếu |
| W16 | `notes/module-4/week-16-personal-development-plan.md` | `notes/module-4/week-16-personal-development.md` | Có (6278 vs 3513 bytes; có LOs chi tiết, glossary 9 mục, case An cụ thể, 9 mục chuẩn) | Canonical = `week-16-personal-development-plan.md`; legacy giữ để tham chiếu |

## Chi tiết sha256

| File | SHA-256 | Bytes | Nhận xét nội dung |
|---|---|---:|---|
| `notes/module-4/week-13-time-management.md` | `07E525D51E2811BC4F0D3627DF5CF37A4122C5EEAED0BC6A910319CD13094DE0` | 4216 | Skeleton; LOs chung chung (LO-W13-01..04 boilerplate); không có worked example cụ thể. Cần viết lại (xem objective-matrix: 🔴). |
| `notes/module-4/week-14-decision-making.md` | `8BFF282723D64FA6C8B390C361153A5FFEAFEE1DF3D6E123D1419CE2B1545254` | 4157 | Skeleton; LOs chung chung; không có worked example, bias checklist cụ thể. Cần viết lại (xem objective-matrix: 🔴). |
| `notes/module-4/week-15-networking-social-skills.md` (canonical) | `96A0A93B88BD60FF216517AEDDF9B4BB462B486A448985D6CF6188F2B78B0F1C` | 5908 | 9 mục chuẩn; LOs rõ (tương hỗ, consent, văn hóa); glossary 9 mục; case An cụ thể; myth rõ. |
| `notes/module-4/week-15-networking.md` (legacy) | `A8455AA82EA15FBE2E856F72A41A10EE8DEBB7AEA0FBCC38AA6013B122690370` | 3435 | 9 mục chuẩn nhưng rút gọn; LOs chung; glossary 4 mục; case An ngắn. **Không có nội dung unique độc lập** so với canonical sau khi rà. |
| `notes/module-4/week-16-personal-development-plan.md` (canonical) | `6A828EFEAA7CF767BE27250A56F2C9B09E8D2EEEF1A323988AFBCE3745C23CE2` | 6278 | 9 mục chuẩn; LOs rõ (mục tiêu kết quả vs hành vi, implementation intention, review); glossary 9 mục; case An cụ thể. |
| `notes/module-4/week-16-personal-development.md` (legacy) | `A368A23A488BD7BE1E691D2001803F82065773FB7AE7F52E03D5105FCA39D34E` | 3513 | 9 mục chuẩn rút gọn; LOs chung; glossary 4 mục. **Không có nội dung unique độc lập** so với canonical sau khi rà. |

## Phương pháp xác định canonical

1. So sánh theo cấu trúc 9 mục (lesson template).
2. So sánh độ rõ của Learning Objectives (Bloom cụ thể hay boilerplate).
3. So sánh Glossary (số mục, có tiếng Anh đi kèm không).
4. So sánh Case Study (có lời nói/quyết định cụ thể của nhân vật không, hay chỉ "An có phản ứng liên quan chủ đề").
5. So sánh Action Steps (có thể hành động được ngay hay chỉ generic).
6. Rà xem file ngắn có nội dung KHÔNG CÓ trong file canonical (để tránh mất dữ kiện nếu xóa).

## Kết quả rà nội dung legacy (W15, W16)

- **W15 legacy (`week-15-networking.md`)**: 
  - Có 1 thêm: nhắc "rủi ro networking giả tạo, hãy test qua hành vi và thời gian" — đã có trong canonical ở mục 5 (Common Myths "Networking là giả tạo").
  - Không có action step / script cụ thể nào vượt quá canonical.
  - **Kết luận: không có nội dung unique đáng giữ lại.**

- **W16 legacy (`week-16-personal-development.md`)**:
  - Có 1 thêm: nhắc "kế hoạch cần phương án thay thế, nghỉ ngơi" — đã có ở canonical mục 4 và 7.
  - Không có nội dung unique.
  - **Kết luận: không có nội dung unique đáng giữ lại.**

## Đề xuất hành động (cho T16 reviewer quyết)

1. **Duy trì trạng thái hiện tại** (canonical = file dài; legacy = file ngắn giữ để đối chiếu) cho đến khi có owner quyết định.
2. Sau khi canonical W15/W16 được bổ sung lần cuối (sau khi quiz/key/flashcard hoàn tất), có thể chuyển 2 file legacy vào thư mục `_archive/` hoặc xóa nếu user duyệt. **KHÔNG xóa trong giai đoạn này** (theo ràng buộc nhiệm vụ).
3. W13 và W14 không có duplicate file; cần viết lại nội dung (đã thực hiện trong đợt này).
4. Mọi tham chiếu trong báo cáo, manifest, ledger nên dùng tên file canonical.

## Liên kết nguồn

- Khảo sát trạng thái: `quality/objective-matrix-w02-w16.md` (cập nhật 2026-09-16).
- Ghi chép duplicate: `quality/module-4-duplicate-migration.md` (đã có từ 2026-09-16, xác nhận canonical W15/W16; báo cáo này cập nhật thêm sha256 + xác nhận không có nội dung unique trong legacy).
- Báo cáo tổng: `audit/learning-readiness-current/t09-lifeskills-report.md` (mục 2).
