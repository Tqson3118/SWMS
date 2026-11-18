# Luật bắt buộc cho mọi phiên agy2 (Studxy Round 2)

Bạn là agent thực thi. Người điều phối (Antigravity) sẽ nghiệm thu độc lập, nên **mọi tuyên bố phải có bằng chứng thật**.

## Môi trường
- Repo: `d:\03_Study\Studxy`, branch `qa/agy2-round2`. Windows, PowerShell.
- App production ĐANG CHẠY: frontend http://127.0.0.1:3000 (Next.js 15), backend http://127.0.0.1:5080 (.NET 10). Đừng tắt hai server này, trừ khi nhiệm vụ yêu cầu rebuild. Nếu cần rebuild: dùng `powershell -ExecutionPolicy Bypass -File scripts/start-prod.ps1` (script tự restart cả hai). Sau khi xong việc, app PHẢI đang chạy lại ở 3000/5080.
- Tài khoản demo: trang `/auth` → nút `demo-switcher-toggle` → "Sơn Trần" (đủ dữ liệu) hoặc "Minh Anh" (trống). Có thể tự đăng ký tài khoản test mới (email dạng `qa+<vai>-<số>@test.local`).
- Test trình duyệt: tự viết script Playwright (Node, `@playwright/test` hoặc `playwright` đã có trong `frontend/node_modules`) đặt trong `docs/qa-round2/scripts/`, chạy headless, chụp ảnh vào `docs/qa-round2/screenshots/<tác-vụ>/`.
- Báo cáo cũ để tham khảo: `docs/qa-round2/OLD_REPORT.md`.

## CẤM TUYỆT ĐỐI
- Xóa file/thư mục (`Remove-Item`, `del`, `rm`, `rmdir`), `git reset --hard`, `git clean`, `git branch -d/-D`, `git push`, `git checkout -- .`, `git stash drop`, đổi branch.
- Xóa / DROP / TRUNCATE dữ liệu trong `swms_local_dev.db`. Test phá hoại chỉ trên tài khoản test do bạn tự tạo.
- Đổi tên hoặc xóa bất kỳ `data-testid` nào hiện có.
- Cài package toàn hệ thống. Thêm dependency npm/nuget mới chỉ khi thật cần và phải ghi lý do trong báo cáo.
- Gắn cứng kết quả, nuốt lỗi, mock dữ liệu để test pass, sửa test cho "xanh" mà không sửa lỗi thật.
- Gửi dữ liệu ra ngoài internet (trừ tải tài liệu tra cứu).

## Commit
- Chỉ khi nhiệm vụ cho phép sửa code: commit nhỏ, mỗi fix 1 commit, message dạng `fix(scope): ...` / `feat(scope): ...`. Chỉ `git add` các file bạn sửa.

## Khi gặp quyết định quan trọng (kiến trúc, đổi luồng auth, xóa tính năng, đổi schema DB, thêm dependency lớn, mơ hồ về sản phẩm)
- Ghi câu hỏi vào `docs/qa-round2/QUESTIONS-<tên-tác-vụ>.md` (mỗi câu: bối cảnh, các phương án, đề xuất của bạn), làm tiếp phần không phụ thuộc, rồi kết thúc lượt. Người điều phối sẽ trả lời và cho bạn chạy tiếp.

## Báo cáo cuối (bắt buộc), tiếng Việt
Ghi vào file được chỉ định trong nhiệm vụ, gồm:
1. Kết quả chính.
2. Bằng chứng: lệnh chính xác + exit code + đường dẫn log/ảnh.
3. Danh sách vấn đề: ID, mức độ (Critical/High/Medium/Low), bước tái hiện, mong đợi vs thực tế, file/dòng nghi ngờ.
4. Mục **Chưa kiểm chứng**.
