# Handoff G2A — Thiết kế importer và kế hoạch dry-run Psychology W01

Ngày: 10/09/2026. Người tạo: reviewer. Trạng thái nền: **G1 APPROVED trong phạm vi pilot W01 và đóng các điểm chặn round 3**. Đây là handoff của pipeline phần mềm, không phải yêu cầu bắt đầu bài Psychology W02.

## Vai trò và yêu cầu của người dùng

Bạn là agent khảo sát và thiết kế cho nền tảng học cá nhân/nhóm nhỏ. Nội dung do agent bên ngoài nghiên cứu, kiểm chứng và biên soạn; website phục vụ bài học đầy đủ, quiz có giải thích, flashcard và ôn tập.

Người dùng yêu cầu khảo sát, đưa bằng chứng và lập kế hoạch để duyệt trước khi sửa code hoặc reset dữ liệu. **Handoff này chỉ giao G2A thiết kế; không tự sửa code, chạy importer, migration, seed, apply, deploy hoặc reset.** Việc reviewer duyệt G1 không thay thế approval cho các thao tác G2.

## Đọc trước

1. `D:\03_Study\Roadmap\Psychology\AGENTS.md`.
2. `D:\03_Study\Roadmap\audit\g0-g1-review-2026-09-10\round-4\review.md` và `reviewed-files.json`.
3. `D:\03_Study\Roadmap\audit\production-assessment-2026-09-10.md`, đặc biệt F01–F08 và kế hoạch G2.
4. `D:\03_Study\quality\rfc-psychology-w01-content-contract-rev1-2026-09-10.md` bản hiện tại.
5. `D:\03_Study\Roadmap\Psychology\manifest.yaml` và các artifact/ledger/audit được tham chiếu.
6. `D:\03_Study\Visualization\backend\tests\DsaVisual.UnitTests\ContentContract\`.
7. Entities/configurations hiện có và các đường API học, quiz, Studio trong `D:\03_Study\Visualization\backend\src\`.

Các bảng tiến độ cũ và số gate trong gate-prompts chỉ là ngữ cảnh lịch sử. Handoff này không tự cấp quyền chạy các lệnh ghi dữ liệu trong tài liệu cũ. Không lấy nhận định cũ làm bằng chứng runtime hiện tại.

## Nhiệm vụ G2A

Chuẩn bị một thiết kế cụ thể cho W01 xuyên suốt validate → dry-run → apply local sau duyệt → xem thử trong app → kiểm lịch sử và phục hồi. Chỉ thiết kế trong lượt được giao này.

1. **Ranh giới module:** validator hiện nằm trong test project. Đề xuất nơi đặt thư viện logic dùng chung để importer không phụ thuộc xUnit; giữ regression tests và fixture tự chứa. Nêu file nào dự kiến thêm/sửa, không chuyển file ngay.
2. **Mapping W01:** subject/course/module/lesson/exercise/questions/concepts, external ID ổn định, 4 LO và 12 câu; bảo toàn rationale, claim, source, KeepOrder và version. Phân biệt metadata cho reviewer với payload người học. ID, quyền và trạng thái không được suy từ tiêu đề hoặc vị trí mảng một cách mơ hồ.
3. **Dry-run chỉ đọc:** xuất CREATE/UPDATE/UNCHANGED/CONFLICT/REJECTED, target environment, package hash, thay đổi dự kiến và ảnh hưởng lịch sử. Dùng quyền DB chỉ đọc trên local khi triển khai sau duyệt; không ghi rồi rollback để gọi là chỉ đọc. Validate tĩnh không cần DB; kiểm service account tồn tại thuộc preflight có DB.
4. **Apply local sau approval riêng:** transaction, idempotency, conflict abort mặc định, kiểm lại precondition/hash ngay trước ghi, service account, trạng thái nháp và phạm vi Private. Không tự publish hoặc retire. Mặc định dừng cả batch khi conflict/rejected.
5. **Snapshot và phục hồi:** Settings.Value chỉ 500 ký tự. Thiết kế snapshot artifact bền vững, kiểm checksum, tham chiếu ngắn hoặc đề xuất migration riêng nếu cần; không lưu cả bài vào Settings. Phân biệt rollback transaction thất bại với phục hồi batch đã commit; xử lý tiến trình dừng giữa việc tạo snapshot và ghi ledger. Giữ dữ liệu học phát sinh sau batch khi phục hồi hoặc báo conflict cần duyệt.
6. **Nội dung và runtime:** xác minh lại F01 quyền xem, F02 GET có thể ghi DB, F03 Studio làm mất metadata, F04–F07 giải thích/chấm bài/history/render. Đề xuất phạm vi sửa cần để xem thử W01 an toàn trên local. Giữ bảng, details, anchor và nguồn khi import rồi lưu qua Studio; không tự nới sanitizer toàn cục. GET phải chỉ đọc.
7. **Kế hoạch thử nghiệm:** manifest/fixture hợp lệ và không hợp lệ, hai lần import không trùng, sửa Studio gây conflict, thất bại giữa chừng và restore, hai người dùng/hai nhóm không đọc chéo, kết quả quiz cũ không đổi theo nội dung mới, link/render bảo toàn. Liệt kê baseline FE/BE/integration còn phải chạy; không coi 41 ContentContract tests là tất cả.

## Phân chia bước sau G2A

- **G2A — hiện tại:** thiết kế và kế hoạch có bằng chứng; trình người dùng duyệt.
- **G2B — sau duyệt code:** triển khai logic validate/dry-run, kiểm thử trên fixture và local tách biệt; trình diff và kết quả dry-run.
- **G2C — sau duyệt apply local:** thực hiện batch local và nghiệm thu vòng học/rollback theo phạm vi đã duyệt.
- Production/deploy/publish là quyết định riêng, không nằm trong approval cho local.

## Acceptance criteria cho bản thiết kế

- Có sơ đồ luồng, mapping, danh sách file dự kiến thay đổi và lý do.
- Phân biệt hash bytes nguồn với hash trạng thái DB để phát hiện sửa ngoài importer.
- Nêu rõ nếu cần migration; không hứa zero-migration cho toàn nền tảng khi chưa đủ dữ liệu.
- Có ma trận quyền và kế hoạch giải quyết các đường API có thể làm lộ bản nháp/Private/ClassOnly.
- Có mẫu báo cáo dry-run, tình huống conflict, snapshot và các bước phục hồi kiểm được.
- Có tiêu chí dừng khi môi trường không xác định, input khác hash đã duyệt hoặc thiếu cấu hình local. Không tìm credential trong Git/chat cũ, không ghi secret vào file hoặc báo cáo.
- Giữ nguyên W01 frozen, không bắt đầu W02, không chạy bất kỳ bước ghi DB nào.

## Bàn giao và điểm dừng

Gửi bản thiết kế, bằng chứng file/dòng, danh sách thay đổi đề xuất, test plan, rủi ro thực tế và những quyết định cần người dùng duyệt. Dừng sau khi bàn giao thiết kế; không coi hoàn thành thiết kế là quyền tự triển khai code.
