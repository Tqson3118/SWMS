# Investing Progress — 2026-09-16

## Trạng thái tổng hợp

- Roadmap hiện hành: **18 tuần**, 4 module; không mở rộng thành 48 tuần.
- W01–W08: giữ nguyên trạng thái theo handoff/verdict trước đó; không làm lại trong đợt này.
- W01–W08: **chưa có bằng chứng đã nhập DB** và **chưa có bằng chứng đã học thử thành công trên web**.
- W09–W16: **đã biên soạn, đã kiểm chứng độc lập, có handoff/review record riêng**.
- W09–W16: **chưa có bằng chứng đã nhập DB** và **chưa có bằng chứng đã học thử thành công trên web**.
- W17: **đã biên soạn và REVIEWER-APPROVED**; đã sửa lỗi số học trong ví dụ.
- W18: **đã biên soạn, đã kiểm chứng độc lập và REVIEWER-APPROVED** (`handoff/w18-review-2026-09-16.md`); toàn bộ claim W18-C01..C04 đã verified trong source ledger; verify_assessments.py và audit_deep.py pass 100%.
- W17–W18: **chưa có bằng chứng đã nhập DB** và **chưa có bằng chứng đã học thử thành công trên web**.
- `D:\03_Study\Visualization`: không sửa mã nguồn; chỉ được đọc/được gọi CLI validate offline để kiểm contract.

## Bằng chứng handoff

| Tuần | Handoff | Nội dung | Import DB | Web smoke test |
|---|---|---|---|---|
| W01 | `handoff/investing-w01-2026-09-15.md` | APPROVED | Chưa | Chưa |
| W02 | `handoff/investing-w02-2026-09-15.md` | APPROVED | Chưa | Chưa |
| W03 | `handoff/investing-w03-2026-09-15.md` | APPROVED | Chưa | Chưa |
| W04 | `handoff/investing-w04-2026-09-15.md` | APPROVED | Chưa | Chưa |
| W05 | `handoff/investing-w05-2026-09-15.md` | APPROVED | Chưa | Chưa |
| W06 | `handoff/investing-w06-2026-09-15.md` | APPROVED | Chưa | Chưa |
| W07 | `handoff/investing-w07-2026-09-15.md` | APPROVED | Chưa | Chưa |
| W08 | `handoff/investing-w08-2026-09-15.md` | APPROVED | Chưa | Chưa |
| W09 | `handoff/w09-handoff-2026-09-15.md`, `handoff/w09-review-2026-09-15.md` | APPROVED độc lập | Chưa | Chưa |
| W10 | `handoff/w10-handoff-2026-09-15.md`, `handoff/w10-review-2026-09-15.md` | APPROVED độc lập | Chưa | Chưa |
| W11 | `handoff/w11-handoff-2026-09-15.md`, `handoff/w11-review-2026-09-15.md` | APPROVED sau re-review | Chưa | Chưa |
| W12 | `handoff/w12-handoff-2026-09-16.md`, `handoff/w12-review-2026-09-16.md` | APPROVED sau re-review | Chưa | Chưa |
| W13 | `handoff/w13-handoff-2026-09-16.md`, `handoff/w13-review-2026-09-16.md` | APPROVED sau re-review | Chưa | Chưa |
| W14 | `handoff/w14-handoff-2026-09-16.md`, `handoff/w14-review-2026-09-16.md` | APPROVED sau re-review | Chưa | Chưa |
| W15 | `handoff/w15-handoff-2026-09-16.md`, `handoff/w15-review-2026-09-16.md` | APPROVED sau re-review | Chưa | Chưa |
| W16 | `handoff/w16-handoff-2026-09-16.md`, `handoff/w16-review-2026-09-16.md` | APPROVED sau re-review | Chưa | Chưa |
| W17 | `handoff/w17-handoff-2026-09-16.md`, `handoff/w17-review-2026-09-16.md` | APPROVED sau re-review | Chưa | Chưa |
| W18 | `handoff/w18-handoff-2026-09-16.md`, `handoff/w18-review-2026-09-16.md` | APPROVED độc lập | Chưa | Chưa |

## Package/source-of-truth

- Canonical content tree for this release: `Investing/notes`, `Investing/assessments`, `Investing/flashcards`, `Investing/quality`, `Investing/manifest*.yaml`.
- `subject/Investing/Module1/W01–W04` remains a legacy/duplicate tree without flashcards/package manifests; it was not used as the package source of truth and was not deleted.
- W01 package uses `Investing/manifest.yaml`; W02–W08 use `Investing/manifest-w02.yaml` through `manifest-w08.yaml`.

## Follow-up before website learning

1. App agent runs validate/dry-run/preview for each package, starting W01, without apply until separately authorized.
2. App agent confirms stable IDs, schema, Markdown/LaTeX, anchors, flashcard mapping and progress persistence.
3. Content agent performs a separate module-wide migration for W01–W06 flashcard IDs/deck IDs to strict `inv` convention where needed; W07–W08 already comply. Do not change IDs after an import without a migration plan.
4. W09–W16 đã hoàn thành content/review; app agent cần kiểm tra package/manifest, stable ID, renderer, anchor, quiz, flashcard và lưu tiến độ trước khi gọi trạng thái nhập/web.

## W17–W18 status

### W17 — Bất động sản
- **Đã biên soạn và REVIEWER-APPROVED** theo `handoff/w17-review-2026-09-16.md` (sau khi sửa các lỗi tính toán ở bảng Sensitivity). Không tạo bản sao cạnh tranh.
- Nguồn: W17-C01 (khung/định nghĩa) và W17-C04 (luật/thuế/tín dụng VN) vẫn **pending**; W17-C02/C03 là calculation-only đã tự kiểm và reviewer duyệt.
- Chờ tích hợp/kiểm thử: **chưa có bằng chứng đã nhập DB** và **chưa học thử web**.

### W18 — Investment Policy Statement
- **Đã biên soạn và REVIEWER-APPROVED** theo `handoff/w18-review-2026-09-16.md` và `handoff/w18-handoff-2026-09-16.md`.
- Toàn bộ claim W18-C01..C04 trong `module-4-source-ledger.md` đã chuyển trạng thái **verified** (tra cứu 2026-09-21) theo giáo trình CFA Institute (Maginn et al., 2007), Luật Chứng khoán 2019 và Thông tư 111/2013/TT-BTC sửa đổi bởi Thông tư 92/2015/TT-BTC Điều 16 (thuế chuyển nhượng 0,1%, thuế cổ tức 5%).
- Đã hoàn thiện: lesson tổng kết liên kết W01–W17, template IPS giáo dục không yêu cầu dữ liệu nhạy cảm, rubric, quiz/key (phân bổ chuẩn 2A/2B/2C/2D), flashcards, source ledger và review độc lập.
- Chờ tích hợp/kiểm thử: **chưa có bằng chứng đã nhập DB** và **chưa học thử web**.
- Vấn đề an toàn: IPS là công cụ giáo dục, không phải khuyến nghị phân bổ; dùng kịch bản giả định và hướng dẫn không chia sẻ dữ liệu tài chính cá nhân.
- Importer allowlist: đã ban hành `importer-allowlist.json` chuẩn hóa toàn bộ 18 tuần canonical và đánh dấu cô lập 18 thư mục misconception legacy.

## Ghi chú claim và validator
- W12: `0,1%` và `5%` chỉ là khung lịch sử/minh họa; văn bản 2026 và chuyển tiếp còn pending.
- W13: claim/doanh nghiệp thật không tái lập được đã loại khỏi artifacts hoặc hạ pending.
- W14: nhiều mechanics sản phẩm/quỹ/ETF còn pending; chỉ trình bày procedure/hypothetical.
- W16: IOSCO/NIST và nguồn mở rộng còn pending; không dùng làm bằng chứng verified.
- Validator pass chỉ chứng minh các kiểm tra kỹ thuật mà validator thực sự thực hiện; không chứng minh import DB/web.
- `audit_numeric.py` còn finding định dạng ngoài phạm vi ở W11/W13; xử lý ở phiên dọn dẹp riêng.
