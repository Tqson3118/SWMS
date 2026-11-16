# Handoff — Sửa logic G2B theo kế hoạch Rev4.2

Ngày: 10/09/2026. Vai trò: agent triển khai phần sửa importer validate/dry-run cho Psychology W01. Chỉ thực hiện sau khi người dùng đã Proceed/duyệt sửa code trong phiên thực thi. Reviewer đã duyệt **kế hoạch sửa**, chưa nghiệm thu code hay toàn bộ G2B. Không suy approval G2C từ handoff này.

## Mục tiêu và bối cảnh độc lập

Website hướng tới học cá nhân/nhóm nhỏ; nội dung nghiên cứu bởi agent ngoài hệ thống, nhập sau kiểm chứng, ưu tiên bài học đầy đủ và quiz có giải thích. G1 đã APPROVED cho W01. Code G2B hiện có offline preview và CLI chặn cờ ghi, nhưng review Rev4.1 tái hiện ba lỗi: baseline snapshot bị sửa vẫn được tin, quiz có bài nộp vẫn được UPDATE, guard và provider hiểu chuỗi kết nối khác nhau. Kiểm quyền còn bỏ sót grant qua custom role/group. Nhiệm vụ này chỉ đóng logic bốn findings đó và tạo bằng chứng đúng phạm vi.

## Đọc trước

1. `D:\03_Study\Roadmap\Psychology\AGENTS.md`.
2. [Kế hoạch Rev4.2](C:/Users/Administrator/.codex/attachments/f7aa08c7-7380-4b92-8339-b82e1db3d78a/pasted-text.txt).
3. [Review duyệt kế hoạch, gồm điều kiện nghiệm thu bắt buộc](D:/03_Study/Roadmap/audit/g2a-review-2026-09-10/rev4.2/review.md).
4. [Review Rev4.1](D:/03_Study/Roadmap/audit/g2a-review-2026-09-10/rev4.1/review.md), `probe-results.json`, `probe/Program.cs` cùng thư mục.
5. [Handoff G2A gốc](D:/03_Study/Roadmap/Psychology/handoff/pm/006-task-06-g2a-import-design.md).
6. Code `D:\03_Study\Visualization\backend\src\DsaVisual.Application\ContentContract\`, CLI importer, tests tương ứng; `D:\03_Study\Roadmap\Psychology\manifest.yaml` và các file được tham chiếu (chỉ đọc).

Không sửa những nguồn frozen hoặc audit cũ. Áp dụng acceptance của review Rev4.2 để sửa các chỗ chưa nhất quán trong ví dụ kiểm thử của kế hoạch; đó là phần đã được reviewer chốt.

## Phạm vi file dự kiến

Trong `D:\03_Study\Visualization\backend\`:

- `src/DsaVisual.Application/ContentContract/Services/ContentDryRunService.cs`: ledger/baseline integrity, phạm vi snapshot, source/current/baseline diff, submission guard, parser chung.
- `src/DsaVisual.Application/ContentContract/Services/PrincipalPermissionVerifier.cs`: kiểm quyền hiệu lực và xử lý chưa xác minh.
- `src/DsaVisual.Api/Cli/ContentImportCliDispatcher.cs`: dùng đúng cấu hình đã parse/kiểm, báo lỗi/exit đúng pha.
- `tests/DsaVisual.UnitTests/ContentContract/ContentDryRunServiceTests.cs`.
- `tests/DsaVisual.UnitTests/ContentContract/PrincipalPermissionVerifierTests.cs` (tạo nếu chưa có).
- `tests/DsaVisual.IntegrationTests/Cli/ImportCliEntryPointTests.cs`.

Fixture/harness mới phục vụ các test trên được đặt trong phạm vi tests hoặc thư mục audit kết quả mới, không ghi đè probe/log trước. Nếu cần mở rộng thay đổi sang schema, runtime học/Studio hoặc module ngoài phạm vi này, trình diff/phạm vi bổ sung trước khi làm.

## Acceptance và quality gate

1. Snapshot bytes phải khớp snap_sha; ledger committed/hash hợp lệ; schema/batch/package/entity baseline đúng. File JSON hợp lệ nhưng bị sửa vẫn phải REJECTED. Có control 31 UNCHANGED, không làm mọi input thành rejected để pass test. Giới hạn root và path; không fallback file khác sau lỗi integrity.
2. Update nội dung/tập câu hỏi quiz có submission → CONFLICT, CanApply=false; update chưa có submission → UPDATE; không đổi có submission → UNCHANGED. Kiểm qua đúng exercise và ghi impact trong báo cáo. G2C sẽ tái kiểm trước ghi, không triển khai apply ở đây.
3. Dùng SqlConnectionStringBuilder nhất quán; alias/quoted/duplicate key không làm guard kiểm host khác provider. Remote target hiệu lực phải bị chặn trước mở kết nối. Không log password/token.
4. Kiểm quyền qua kết quả truy vấn hiệu lực, không chỉ fixed role/catalog direct grants. Bất kỳ khả năng ghi trong phạm vi cần bảo vệ → WRITE_CAPABLE; NULL/error/chưa đủ chứng cứ → NOT_VERIFIED. Mock kiểm logic thật, không giả lập sẵn verdict rồi tuyên bố SQL đã xác minh.
5. Offline luôn DB_NOT_CHECKED, CanApply=false; offline invalid exit khác 0. Offline không đọc snapshot để xác minh ledger. Cờ ghi và cờ chưa hỗ trợ tiếp tục bị chặn trước startup; conflict không được báo nhầm lỗi validate tĩnh.
6. Build bản mới, chạy unit tests liên quan và CLI entry-point tests, sau đó bộ unit theo kế hoạch. Chạy lại phản ví dụ bằng harness tương thích baseline mới, có assert và dữ liệu đối chứng; lưu kết quả mới. Kiểm traceability DLL/source.

## SQL local: giữ trạng thái chưa nghiệm thu

Agent báo máy chưa có Docker/SQL local khả dụng; reviewer chưa xác minh lại hạ tầng. Không tự cài đặt, khởi tạo SQL, tạo login/user thật hoặc kết nối remote. Có thể hoàn thành logic và tests fixture mà chưa có SQL, nhưng báo riêng:

- Logic/tests: kết quả thực tế, pass/fail cụ thể.
- SQL local với principal đọc thuần/custom writer: **NOT_RUN / CHƯA NGHIỆM THU** nếu chưa được cấp phát và duyệt môi trường.
- Toàn G2B: chờ reviewer, không tự APPROVED/100% verified.

Tests mô phỏng không chứng minh permission SQL. Thử nghiệm tạo principal/grant/DML chỉ được chạy trên fixture SQL local sau approval riêng; không dùng DB đang khảo sát để thử quyền ghi.

## Điều kiện dừng và gói bàn giao

Dừng trước mọi apply/rollback/cleanup/migration/seed/reset/deploy, G2C/runtime hoặc thay W01 frozen. Không làm W02. Nếu chưa có user Proceed cho sửa code, trình kế hoạch và chờ; khi đã có thì không hỏi lại cho cùng phạm vi.

Bàn giao danh sách file/diff và lý do, log build/tests/CLI, test cases đóng từng finding, các phản ví dụ mới với control, nguồn DLL/source đã kiểm, hạn chế SQL còn lại. Dừng sau bàn giao để reviewer nghiệm thu. Không tự tạo handoff kế tiếp.
