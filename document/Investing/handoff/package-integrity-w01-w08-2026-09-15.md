# Package Integrity Handoff — Investing W01–W08

**Ngày:** 2026-09-15  
**Phạm vi:** `D:\03_Study\Roadmap\Investing`  
**Không thực hiện:** sửa `D:\03_Study\Visualization`, import/apply DB, seed dữ liệu hoặc web smoke test.

## Kết luận

- Tất cả artifact được đối chiếu từ raw bytes hiện tại.
- Các artifact nội dung khác ngoài shared source ledger đều giữ nguyên hash/byte length đã có trong manifest.
- Chỉ cập nhật metadata checksum của shared `source-ledger.md` trong W01, W02 và W04.
- `--import-validate` offline PASS cho đủ W01–W08.
- Stable ID của lesson, quiz, answer key, deck, unit và concept không thay đổi.
- Không có bằng chứng package nào đã được ghi vào database hoặc học thử thành công trên web.

## Metadata sau đối chiếu

`Before` dưới đây là giá trị manifest trước khi sửa checksum; `After` là giá trị hiện tại sau khi sửa. Artifact không thay đổi nội dung được ghi `unchanged`.

### W01 — `package:investing:w01`

Manifest: `Investing/manifest.yaml`  
Manifest SHA-256 sau sửa: `04adbe9c2b370aaf2b04f53249e9f98343c6c4d56c6a54e34feff5f610c1b9fb`  
Composite package SHA-256: `9363ae32c8509dec6cf49fc09aec724f64bb1e7f4cb7bf0f00434581facd5f74`

| Role | Stable ID | Artifact | Before | After/raw bytes |
|---|---|---|---|---|
| lesson_theory | `roadmap:lesson:investing:w01` | `notes/module-1/week-01-tien-te-lam-phat-lai-suat-tvm.md` | `d97b989...878b78b`, 11865 | unchanged: `d97b989...878b78b`, 11865 |
| quiz_questions | `roadmap:quiz:investing:w01` | `assessments/quizzes/week-01-quiz.md` | `446f0ca...589b4ca`, 4677 | unchanged: `446f0ca...589b4ca`, 4677 |
| quiz_answers | `roadmap:answers:investing:w01` | `assessments/answer-keys/week-01-answers.md` | `561fea0...9addb4`, 3920 | unchanged: `561fea0...9addb4`, 3920 |
| flashcard_deck | `roadmap:deck:investing:w01` | `flashcards/week-01.yaml` | `50b0840...ff677e`, 3874 | unchanged: `50b0840...ff677e`, 3874 |
| source_ledger | none | `quality/module-1-source-ledger.md` | `8af137e...663d5d14`, 11551 | `0f6c5acd54546452f1976e2244b1ca7e39aabe7b24c7d0c0c9084049eed4ae2b`, 19537 |

### W02 — `package:investing:w02`

Manifest: `Investing/manifest-w02.yaml`  
Manifest SHA-256 after: `064dcbe34b39416914b3611e9d3512527e729c51fa049ebf802c8c66360c74b8`  
Composite package SHA-256: `7a72188f8ef69859f09ae2740f3cfc735fcacd67304887f291c646987c564306`

All four lesson/quiz/answer/deck artifacts: **unchanged**, with raw hashes and byte lengths matching their manifest values. Shared source ledger changed from `8af137e...663d5d14`, 11551 to `0f6c5acd...4ae2b`, 19537.

Stable IDs unchanged: `roadmap:unit:investing:w02`, `roadmap:lesson:investing:w02`, `roadmap:quiz:investing:w02`, `roadmap:answers:investing:w02`, `roadmap:deck:investing:w02`.

### W03 — `package:investing:w03`

Manifest: `Investing/manifest-w03.yaml`  
Manifest SHA-256 after: `97ae3c15e4fbabc9bb4fb2245f3f44f2d51529217a76224659d17d5f6aaa923e`  
Composite package SHA-256: `d5f9884cc472230191505a31b4f246ec8d28faa9457652ca72901387fb2a7bbf`

All five artifact hashes and byte lengths match raw bytes. No metadata change was needed in this integrity pass. Stable IDs unchanged.

### W04 — `package:investing:w04`

Manifest: `Investing/manifest-w04.yaml`  
Manifest SHA-256 after: `15e469cd366fbca8c9a4d95af21ca5d8d3b7631225b3454eff8aab7d1ba0eb6a`  
Composite package SHA-256: `ac2092965feda41f341e8dac95c38ec5c6182a16a44f7746524f654505df9323`

All four lesson/quiz/answer/deck artifacts: **unchanged**, with raw hashes and byte lengths matching their manifest values. Shared source ledger changed from `8af137e...663d5d14`, 11551 to `0f6c5acd...4ae2b`, 19537.

Stable IDs unchanged: `roadmap:unit:investing:w04`, `roadmap:lesson:investing:w04`, `roadmap:quiz:investing:w04`, `roadmap:answers:investing:w04`, `roadmap:deck:investing:w04`.

### W05 — `package:investing:w05`

Manifest: `Investing/manifest-w05.yaml`  
Manifest SHA-256 after: `6bdf3dd76e27deb6a99457fff37053af4913c6138b84b336c8205fb02a4c2322`  
Composite package SHA-256: `6ef4ac7889a2db62044676a1fe81ea181c3d66c7ee6121a7467dfb5352b3fd64`

All five artifact hashes and byte lengths match raw bytes. Shared Module 2 ledger is current at `c5106df...b9ebe0`, 46279 bytes. Stable IDs unchanged.

### W06 — `package:investing:w06`

Manifest: `Investing/manifest-w06.yaml`  
Manifest SHA-256 after: `8007733edb181dd0b53fa2c609e9f176dd967fd83cb2902d5f4b5722916ef494`  
Composite package SHA-256: `5da140882c8d81bc0d97a316a94157e95d1b854db09e0f71a91b65723d6c40d8`

All five artifact hashes and byte lengths match raw bytes. Stable IDs unchanged.

### W07 — `package:investing:w07`

Manifest: `Investing/manifest-w07.yaml`  
Manifest SHA-256 after: `e8122cfc69144a0c9209ad05bd44cf8bb80dbad985bd5db2719b84158052deb7`  
Composite package SHA-256: `e8ccdf84cde25c2d171ef7cfa99356dc60f4d2db089e3a52a27b64e5927c67a9`

All five artifact hashes and byte lengths match raw bytes. Strict deck/card IDs remain `deck-inv-w07` and `fc-inv-w07-001..008`; no stable ID change was made.

### W08 — `package:investing:w08`

Manifest: `Investing/manifest-w08.yaml`  
Manifest SHA-256 after: `41eaaff38452a044a2384575f452de1b3adf168bac942327b7301b27992d78f4`  
Composite package SHA-256: `aa72339b1e88975f72dfd88d42f074e4a2da06cb91702cdb11f28897c428addd`

All five artifact hashes and byte lengths match raw bytes. Strict deck/card IDs use `deck-inv-w08` and `fc-inv-w08-001..008`; stable IDs unchanged.

## Validator results

### Import validation

Command pattern used, read-only/offline:

```powershell
dotnet run --project D:\03_Study\Visualization\backend\src\DsaVisual.Api\DsaVisual.Api.csproj -- --import-validate D:\03_Study\Roadmap\Investing\<manifest>.yaml
```

| Week | Manifest | Result | Parsed questions |
|---|---|---|---:|
| W01 | `manifest.yaml` | PASS, offline/zero DB | 8 |
| W02 | `manifest-w02.yaml` | PASS, offline/zero DB | 8 |
| W03 | `manifest-w03.yaml` | PASS, offline/zero DB | 8 |
| W04 | `manifest-w04.yaml` | PASS, offline/zero DB | 8 |
| W05 | `manifest-w05.yaml` | PASS, offline/zero DB | 8 |
| W06 | `manifest-w06.yaml` | PASS, offline/zero DB | 8 |
| W07 | `manifest-w07.yaml` | PASS, offline/zero DB | 8 |
| W08 | `manifest-w08.yaml` | PASS, offline/zero DB | 8 |

### Assessment and deep audits

- `python scripts/verify_assessments.py`: PASS, all W01–W18, 144 questions, balanced distributions.
- `python scripts/audit_deep.py`: PASS, 0 issues.
- `python scripts/audit_numeric.py`: PASS for the current run; no W01–W08 finding.
- `python -m unittest tests/tools/test_validate_flashcards.py`: PASS, 25 tests. The suite intentionally prints negative-fixture error messages while asserting those cases are rejected; final result is `OK`.
- Strict flashcard validation evidence: W07 and W08 passed during their independent review; W01–W06 retain the previously recorded legacy `fc-investing-*` naming issue and were not renamed in this package-integrity-only pass.

## Stable ID status

- No lesson, quiz, answer-key, deck, unit or concept stable ID was changed.
- No lesson, quiz, answer key or flashcard content was changed in this integrity pass.
- Only manifest checksum/byte-length metadata for W01, W02 and W04 was changed.

## Required status fields

| Week | Authored | Verified | Package-valid | Ingested | Web-smoke-tested |
|---|---|---|---|---|---|
| W01 | yes | yes, independent `APPROVED` | yes | no evidence | no evidence |
| W02 | yes | yes, independent `APPROVED` | yes | no evidence | no evidence |
| W03 | yes | yes, independent `APPROVED` | yes | no evidence | no evidence |
| W04 | yes | yes, independent `APPROVED` | yes | no evidence | no evidence |
| W05 | yes | yes, independent `APPROVED` | yes | no evidence | no evidence |
| W06 | yes | yes, independent `APPROVED` | yes | no evidence | no evidence |
| W07 | yes | yes, independent final `APPROVED` | yes | no evidence | no evidence |
| W08 | yes | yes, independent `APPROVED` | yes | no evidence | no evidence |

## Remaining issues

- W01–W06 flashcard decks retain the previously documented `fc-investing-*` strict-ID convention issue. This pass did not rename cards or deck IDs because the requested scope was package integrity and stable IDs must not change without a migration decision. W07–W08 use strict-compliant `inv` IDs.
- `--import-validate` is static/offline validation only. It does not prove database ingestion, renderer behavior, anchors, flashcard persistence or progress saving.
- No production or local database write was performed.
- No web smoke test was performed.
