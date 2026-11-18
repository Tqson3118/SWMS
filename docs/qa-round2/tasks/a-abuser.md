Đọc và tuân thủ tuyệt đối `docs/qa-round2/AGY2_RULES.md` trước khi làm bất cứ gì.

# Nhiệm vụ A-ABUSER: Đóng vai người cố tình lạm dụng lỗi để trục lợi (CHỈ TEST + BÁO CÁO, KHÔNG SỬA CODE ỨNG DỤNG)

Lưu ý môi trường: một agent khác đang redesign frontend song song và thỉnh thoảng restart server. Gặp connection refused thì đợi 30–60s rồi thử lại. Tập trung vào **logic nghiệp vụ qua API** (http://127.0.0.1:5080, hoặc proxy http://127.0.0.1:3000/api/v1/*).

Bạn là người dùng tham lam: muốn dùng AI miễn phí vô hạn, làm đẹp thống kê/streak, gian lận điểm quiz, phá công bằng. Chỉ dùng tài khoản test DO BẠN TỰ TẠO. Không làm sập server.

Đọc `backend/src/Swms.Api/Features/*`, `Infrastructure/*` (quota ledger, outbox, FSRS sync, idempotency, OCC) để tìm chỗ yếu, rồi thử thực tế:
1. Quota AI: vượt hạn mức ngày bằng request song song (race), tạo nhiều tài khoản (có giới hạn đăng ký không?), lỗi upstream có trả lại credit đúng không, đổi múi giờ/đồng hồ client để reset quota, gọi trực tiếp endpoint tạo khóa học/chấm essay bỏ qua UI.
2. FSRS/flashcard: gửi review với timestamp tương lai/quá khứ, replay cùng `reviewEventId`, offline replay giả lịch sử dài để farm streak/thống kê, rating bất thường, Undo nhiều lần, đồng bộ đồng thời từ 2 "thiết bị".
3. Streak/tiến độ: đánh dấu hoàn thành bài học hàng loạt không đọc, hoàn thành bài của khóa không sở hữu, progress > 100%, số âm.
4. Quiz: xem đáp án trước khi nộp (response có lộ `correctAnswer` không?), nộp nhiều lần lấy điểm cao nhất, sửa điểm trong body, nộp quiz của người khác.
5. Notebook OCC/409: ép tạo vô hạn conflict draft (phình DB), baseVersion sai.
6. Input cực đoan: chuỗi rất dài, unicode lạ, số khổng lồ, mảng rỗng, JSON lồng sâu — server có trả 4xx gọn gàng hay 500?

Mỗi lỗi: PoC script trong `docs/qa-round2/scripts/a-abuser/` (chạy lại được), mức độ, lợi ích kẻ gian thu được, file/dòng nghi ngờ, đề xuất fix.
Cuối báo cáo thêm mục **"Góp ý như người dùng"**: điều gì khiến bạn muốn gian lận (vd streak quá khắt khe), gợi ý thiết kế công bằng hơn, tính năng muốn có.

Báo cáo cuối: `docs/qa-round2/A-abuser-report.md`. Chỉ commit trong `docs/qa-round2/` (`chore(qa): A abuser report`).
