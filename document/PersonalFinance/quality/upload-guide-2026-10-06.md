# Chưa sẵn sàng phát hành — đính chính 489

Bản chứng nhận và ZIP pf-release481 đã bị rút phê duyệt: còn lỗi nội dung W03 và kiểm tra giao diện479 chưa đạt. Không dùng ZIP đó để upload. Nội dung cũ dưới đây được giữ để truy vết, không phải phê duyệt hiện tại.

# PersonalFinance — Hướng dẫn import bản phát hành 2026-10-06

Bản phát hành bao gồm đầy đủ 12 tuần (W01–W12) của lộ trình Tài chính cá nhân. Mở [README](../README.md) để bắt đầu tự học với bài giảng lý thuyết, câu hỏi trắc nghiệm (quiz), giải thích đáp án chi tiết và bộ thẻ ghi nhớ (flashcard). Tệp ZIP `PersonalFinance-upload-ready-2026-10-06.zip` dùng để lưu trữ và phân phối; vui lòng giải nén nguyên vẹn thư mục `PersonalFinance` trước khi thực hiện thao tác import trên hệ thống Visualization.

## Thứ tự và quy trình import manifest
Thực hiện import từng manifest tuần tự theo thứ tự sau:
1. `manifest.yaml` (Tuần 1: Tư duy tiền bạc & Tâm lý học hành vi)
2. `manifest-w02.yaml` (Tuần 2: Quản lý thu nhập & Dòng tiền)
3. `manifest-w03.yaml` (Tuần 3: Nguyên tắc & Quy tắc lập ngân sách)
4. `manifest-w04.yaml` (Tuần 4: Quỹ khẩn cấp & Dự phòng rủi ro)
5. `manifest-w05.yaml` (Tuần 5: Bảo hiểm tại Việt Nam)
6. `manifest-w06.yaml` (Tuần 6: Quản lý nợ & Đòn bẩy tài chính)
7. `manifest-w07.yaml` (Tuần 7: Lãi kép & Lộ trình FIRE)
8. `manifest-w08.yaml` (Tuần 8: Các kênh đầu tư tại Việt Nam)
9. `manifest-w09.yaml` (Tuần 9: Thuế thu nhập cá nhân tại Việt Nam)
10. `manifest-w10.yaml` (Tuần 10: Lập kế hoạch theo giai đoạn cuộc đời)
11. `manifest-w11.yaml` (Tuần 11: Hưu trí & Bảo hiểm xã hội)
12. `manifest-w12.yaml` (Tuần 12: Bản kế hoạch tài chính cá nhân toàn diện)

Tất cả manifest đều có chung `subject_id: "roadmap:subject:personal-finance"` và được gán `sort_order` từ 1 đến 12. Mỗi manifest trỏ đến các tài nguyên tương đối (`notes/`, `assessments/`, `flashcards/`, `quality/`) bên trong thư mục `PersonalFinance`; không di chuyển tệp manifest ra ngoài thư mục gốc của môn học.

## Quy mô và kiểm thử chất lượng
- **12 tuần học hoàn chỉnh**: 12 bài giảng lý thuyết chuẩn sư phạm.
- **121 câu hỏi trắc nghiệm (quiz)**: Kèm 363 phần giải thích cụ thể cho từng phương án sai.
- **159 flashcard**: Được xác thực schema JSON Schema Draft 2020-12 (exit code 0).
- **Hợp đồng Importer**: Toàn bộ 12 manifest vượt qua `--import-validate` và `--import-preview` không có vi phạm hợp đồng (exit code 0).
- **Browser Journey E2E**: Kiểm thử hành trình giao diện học tập xuyên suốt 12 tuần bằng Playwright (`pf-navigation480`) đạt kết quả PASS với 0 lỗi console/render.

## Giới hạn và tuyên bố miễn trách nhiệm
Nội dung khóa học được biên soạn nhằm mục đích giáo dục và nâng cao nhận thức tài chính cá nhân, căn cứ trên các văn bản quy phạm pháp luật Việt Nam (Luật Kinh doanh bảo hiểm 2022, Luật Thuế TNCN, Thông tư 06/VBHN-NHNN, Luật BHXH 2024...) và các nghiên cứu khoa học hành vi (Kahneman & Tversky, Langer...). Nội dung không cấu thành lời khuyên đầu tư, khuyến nghị pháp lý hay tư vấn thuế chuyên nghiệp.
