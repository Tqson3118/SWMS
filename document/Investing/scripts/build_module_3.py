"""
Module 3 Assessments Generator (W11 - W14)
Standardizes:
- Balanced distribution: 2A, 2B, 2C, 2D
- Clean markdown formatting: A., B., C., D. (no leading dash)
- Coherent, detailed explanations analyzing correct answers and distractors
- Full lesson links and unified titles (# Quiz WNN — ... and # Đáp án & giải thích WNN — ...)
"""

import os
import sys

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")

sys.path.insert(0, os.path.dirname(__file__))
from common_builder import build_week_assessment


def build_w11():
    title = "Tổng quan TTCK Việt Nam: HOSE, HNX, UPCoM và VN-Index"
    module_num = 3
    lesson_slug = "week-11-thi-truong-viet-nam"

    questions = [
        {
            "qid": "Q11-01",
            "lo": "LO-W11-01",
            "question": "Sàn giao dịch nào sau đây là nơi niêm yết tập trung của các doanh nghiệp có quy mô vốn hóa và thanh khoản lớn nhất thị trường chứng khoán Việt Nam?",
            "options": {
                "A": "Sàn HNX (Sở Giao dịch Chứng khoán Hà Nội)",
                "B": "Sàn UPCoM (Thị trường công ty đại chúng chưa niêm yết)",
                "C": "Thị trường phát hành nợ sơ cấp của các tổ chức tín dụng",
                "D": "Sàn HOSE (Sở Giao dịch Chứng khoán TP. Hồ Chí Minh)"
            },
            "ans": "D",
            "explanation": "Sàn HOSE là nơi niêm yết tập trung của các doanh nghiệp đầu ngành (Blue-chips), có điều kiện niêm yết khắt khe nhất về vốn điều lệ, thời gian hoạt động, lịch sử sinh lời và tiêu chuẩn quản trị công ty tại Việt Nam (D đúng). A là sàn HNX tập trung doanh nghiệp vốn hóa vừa và nhỏ; B là sàn UPCoM cho công ty đại chúng chưa niêm yết với biên độ dao động rộng hơn (±15%); C là thị trường phát hành nợ, không niêm yết cổ phiếu."
        },
        {
            "qid": "Q11-02",
            "lo": "LO-W11-01",
            "question": "Biên độ dao động giá trong ngày (Daily trading band) đối với cổ phiếu niêm yết trên sàn HOSE hiện hành là bao nhiêu so với giá tham chiếu?",
            "options": {
                "A": "±5,0% so với giá tham chiếu",
                "B": "±7,0% so với giá tham chiếu",
                "C": "±10,0% so với giá tham chiếu",
                "D": "±15,0% so với giá tham chiếu"
            },
            "ans": "B",
            "explanation": "Theo quy chế giao dịch hiện hành của Sở Giao dịch Chứng khoán Việt Nam, biên độ dao động giá hàng ngày trên sàn HOSE là ±7% so với giá tham chiếu (B đúng). A là biên độ áp dụng trong quá khứ cũ; C là biên độ áp dụng cho sàn HNX (±10%); D là biên độ áp dụng cho sàn UPCoM (±15%)."
        },
        {
            "qid": "Q11-03",
            "lo": "LO-W11-01",
            "question": "Chỉ số nào là thước đo tổng hợp tiêu biểu và lịch sử lâu đời nhất, phản ánh toàn bộ biến động vốn hóa của các cổ phiếu niêm yết trên sàn HOSE?",
            "options": {
                "A": "HNX-Index",
                "B": "UPCoM-Index",
                "C": "VN-Index",
                "D": "VN30-Index"
            },
            "ans": "C",
            "explanation": "VN-Index là chỉ số tổng hợp đầu tiên và đại diện bao quát nhất cho TTCK Việt Nam từ năm 2000, đo lường sự biến động tổng giá trị vốn hóa của tất cả các mã cổ phiếu niêm yết trên sàn HOSE (C đúng). A đại diện riêng cho sàn HNX; B đại diện cho sàn UPCoM; D là chỉ số chọn lọc riêng cho 30 cổ phiếu vốn hóa và thanh khoản hàng đầu trên HOSE."
        },
        {
            "qid": "Q11-04",
            "lo": "LO-W11-02",
            "question": "Phương pháp luận tính toán các chỉ số hiện đại như VN30 và rổ chỉ số của HOSE dựa trên nguyên tắc nào sau đây?",
            "options": {
                "A": "Tính theo giá trị vốn hóa thị trường có điều chỉnh tỷ lệ cổ phiếu tự do chuyển nhượng (Free-float) và áp trần tỷ trọng tối đa",
                "B": "Tính theo trung bình cộng giản đơn thị giá của tất cả các cổ phiếu mà không xét đến khối lượng niêm yết",
                "C": "Tính theo tổng doanh thu kế toán hàng năm của tất cả các doanh nghiệp niêm yết",
                "D": "Tính theo tổng số lượng tài khoản chứng khoán mở mới trong tháng của nhà đầu tư cá nhân"
            },
            "ans": "A",
            "explanation": "Rổ chỉ số chứng khoán hiện đại (như VN30) tính toán dựa trên vốn hóa thị trường điều chỉnh theo tỷ lệ cổ phiếu tự do chuyển nhượng (Free-float) và áp trần tỷ trọng (capping) để tránh việc một vài mã vốn hóa khổng lồ thao túng chỉ số (A đúng). B là cách tính thô sơ kiểu Dow Jones cũ; C và D không phải là phương pháp luận tính toán chỉ số giá chứng khoán."
        },
        {
            "qid": "Q11-05",
            "lo": "LO-W11-02",
            "question": "Khi nghiên cứu dữ liệu lịch sử của các chỉ số chứng khoán, nhà đầu tư cần áp dụng nguyên tắc thận trọng nào sau đây?",
            "options": {
                "A": "Ghi nhận chính xác mốc thời gian lấy dữ liệu, phương pháp luận tính toán chỉ số và các đợt cơ cấu thay đổi cổ phiếu thành phần trong rổ",
                "B": "Chỉ nhìn vào điểm số hiển thị trên mạng xã hội mà không cần đối chiếu ngày giờ giao dịch",
                "C": "Mặc định rằng danh sách các cổ phiếu thành phần trong rổ chỉ số là cố định vĩnh viễn không bao giờ thay đổi",
                "D": "Coi mức tăng trưởng của chỉ số chứng khoán là đại diện tuyệt đối cho tình trạng tài chính của toàn thể người dân"
            },
            "ans": "A",
            "explanation": "Rổ chỉ số định kỳ được cơ cấu lại (thêm bớt cổ phiếu, cập nhật tỷ lệ free-float); do đó dữ liệu lịch sử phải đi kèm mốc thời gian và phương pháp luận cụ thể để tránh so sánh sai lệch (A đúng). B là sự bất cẩn dữ liệu; C sai vì các chỉ số như VN30 định kỳ cơ cấu 6 tháng/lần; D đánh đồng chỉ số tài chính với phúc lợi toàn dân."
        },
        {
            "qid": "Q11-06",
            "lo": "LO-W11-03",
            "question": "Một nhà đầu tư nhận thấy chỉ số VN-Index tăng 15% trong quý, nhưng danh mục cổ phiếu cá nhân lại bị giảm 5%. Hiện tượng này được giải thích đúng nhất bởi lý do nào?",
            "options": {
                "A": "Sàn giao dịch chứng khoán đã tính toán sai hoàn toàn điểm số của chỉ số VN-Index",
                "B": "Tất cả các cổ phiếu trên thị trường bắt buộc phải tăng cùng một tỷ lệ phần trăm như VN-Index",
                "C": "Chỉ số VN-Index bị kéo tăng bởi một vài mã cổ phiếu vốn hóa khổng lồ (cổ phiếu trụ), trong khi các mã cổ phiếu trong danh mục cá nhân bị bán ra hoặc chịu chi phí giao dịch",
                "D": "Tài khoản cá nhân bị công ty chứng khoán tự ý can thiệp làm sụt giảm giá trị vốn"
            },
            "ans": "C",
            "explanation": "Hiện tượng 'Xanh vỏ đỏ lòng': Các mã vốn hóa khổng lồ (ngân hàng, bán lẻ lớn) tăng giá kéo chỉ số VN-Index tăng mạnh, nhưng phần lớn các cổ phiếu mid-cap và small-cap có thể đứng im hoặc giảm giá, dẫn đến danh mục cá nhân bị lệch pha so với chỉ số chung (C đúng). A, B, D là những suy đoán thiếu hiểu biết về cơ chế cấu thành chỉ số theo tỷ trọng vốn hóa."
        },
        {
            "qid": "Q11-07",
            "lo": "LO-W11-03",
            "question": "Sàn giao dịch UPCoM có đặc điểm nào sau đây mà nhà đầu tư cá nhân cần đặc biệt lưu ý về mặt rủi ro?",
            "options": {
                "A": "Luật pháp cấm hoàn toàn nhà đầu tư cá nhân trong nước tham gia mua bán trên UPCoM",
                "B": "Sàn UPCoM chỉ cho phép giao dịch trái phiếu chính phủ mà không có cổ phiếu doanh nghiệp",
                "C": "Giá cổ phiếu trên UPCoM được cơ quan quản lý bảo đảm không bao giờ giảm quá 1% mỗi phiên",
                "D": "Biên độ dao động giá rất rộng (±15% trong ngày) và yêu cầu chuẩn mực công bố thông tin, chất lượng kiểm toán ít khắt khe hơn sàn niêm yết HOSE/HNX"
            },
            "ans": "D",
            "explanation": "UPCoM có biên độ dao động ±15%/ngày (gấp đôi HOSE), khiến mức độ biến động giá rất dữ dội; đồng thời tiêu chuẩn công bố thông tin và kiểm toán không cao bằng sàn niêm yết chính thức, tiềm ẩn rủi ro thông tin bất cân xứng (D đúng). A sai vì nhà đầu tư cá nhân được giao dịch bình thường; B sai vì UPCoM là sàn cổ phiếu đại chúng; C hoàn toàn sai thực tế."
        },
        {
            "qid": "Q11-08",
            "lo": "LO-W11-04",
            "question": "Chu kỳ thanh toán bù trừ chuẩn hiện hành đối với giao dịch mua bán cổ phiếu trên thị trường chứng khoán Việt Nam là:",
            "options": {
                "A": "T+0 (khớp lệnh là tiền và cổ phiếu về tài khoản ngay lập tức để bán lại trong phiên)",
                "B": "T+2 (chiều ngày làm việc thứ hai sau ngày khớp lệnh thì tiền/cổ phiếu về tài khoản sẵn sàng giao dịch)",
                "C": "T+3 (chiều ngày làm việc thứ ba sau ngày giao dịch)",
                "D": "T+7 (sau đúng 1 tuần lễ tiền và cổ phiếu mới được chuyển giao thanh toán)"
            },
            "ans": "B",
            "explanation": "Theo quy chế của Tổng công ty Lưu ký và Bù trừ Chứng khoán Việt Nam (VSDC), chu kỳ thanh toán hiện hành là T+2: giao dịch mua/bán ngày T sẽ được thanh toán vào đầu giờ chiều ngày làm việc thứ 2 (T+2), cho phép nhà đầu tư giao dịch được ngay trong phiên chiều hôm đó (B đúng). A là chu kỳ trong ngày (T+0 hiện chưa áp dụng đại trà cho cổ phiếu cơ sở tại VN); C là chu kỳ cũ trước năm 2022; D không áp dụng trên thị trường chứng khoán niêm yết."
        }
    ]

    build_week_assessment(11, title, module_num, lesson_slug, questions)


def build_w12():
    title = "Mở tài khoản, lệnh giao dịch, phí và thuế tại Việt Nam"
    module_num = 3
    lesson_slug = "week-12-tai-khoan-lenh-phi-thue-vn"

    questions = [
        {
            "qid": "Q12-01",
            "lo": "LO-W12-01",
            "question": "Lựa chọn nào mô tả đúng LO?",
            "options": {
                "A": "Lệnh mua/bán tại giá chỉ định hoặc tốt hơn; có thể không khớp nếu thị trường không đạt mức giá đó.",
                "B": "Lệnh mua luôn khớp ngay tại giá thấp nhất trong ngày.",
                "C": "Lệnh chỉ được nhập trong phiên đóng cửa.",
                "D": "Lệnh không chịu phí vì nhà đầu tư tự chỉ định giá."
            },
            "ans": "A",
            "bloom": "Understand", "cluster": "LO", "position": ["Mục 3.1", "Mục 4.2"], "claim": "(A đúng) W12-C01",
            "explanation": "LO là lệnh mua/bán tại giá chỉ định hoặc tốt hơn; giới hạn giá có thể khiến lệnh không khớp. **B sai** vì LO không bảo đảm giá thấp nhất cả ngày. **C sai** vì LO có thể dùng trong các phiên được quy định, không chỉ đóng cửa. **D sai** vì chỉ định giá không miễn phí dịch vụ hay thuế. Căn cứ: `W12-C01`, lesson Mục 4.2."
        },
        {
            "qid": "Q12-02",
            "lo": "LO-W12-01",
            "question": "Mô tả nào đúng về ATO và ATC theo tài liệu loại lệnh HOSE?",
            "options": {
                "A": "ATO dùng để xác định giá đóng cửa, ATC dùng để xác định giá mở cửa.",
                "B": "ATO dùng trong khớp định kỳ mở cửa, ATC dùng trong khớp định kỳ đóng cửa; phần chưa khớp được xử lý theo quy tắc phiên tương ứng.",
                "C": "Cả hai đều là LO có giá giới hạn do nhà đầu tư nhập.",
                "D": "Cả hai đều là lệnh thị trường có hiệu lực đến cuối ngày."
            },
            "ans": "B",
            "bloom": "Understand", "cluster": "ATO/ATC", "position": ["Mục 3.1", "Mục 4.2"], "claim": "(B đúng) W12-C01",
            "explanation": "ATO gắn với khớp định kỳ mở cửa, ATC gắn với khớp định kỳ đóng cửa; phần chưa khớp được hủy/hết hạn theo quy tắc. **A sai** do đảo ngược mở và đóng. **C sai** vì ATO/ATC không phải LO có mức giá cố định do nhà đầu tư nhập. **D sai** vì chúng không có hiệu lực suốt ngày như mô tả. Căn cứ: `W12-C01`."
        },
        {
            "qid": "Q12-03",
            "lo": "LO-W12-01",
            "question": "Rủi ro chính của lệnh thị trường/MTL khi sổ lệnh mỏng là gì?",
            "options": {
                "A": "Luôn bị khóa tài khoản 24 giờ.",
                "B": "Tự động được miễn thuế chuyển nhượng.",
                "C": "Có thể khớp qua nhiều mức giá, tạo slippage lớn hơn kỳ vọng.",
                "D": "Chắc chắn khớp tại giá tham chiếu."
            },
            "ans": "C",
            "bloom": "Analyze", "cluster": "Lệnh thị trường và slippage", "position": ["Mục 4.2"], "claim": "(C đúng) W12-C01",
            "explanation": "Lệnh thị trường ưu tiên khớp với giá đối ứng hiện có; sổ mỏng khiến lệnh ăn qua nhiều mức giá và slippage tăng. **A sai** vì không có quy tắc khóa tài khoản như vậy. **B sai** vì loại lệnh không miễn thuế. **D sai** vì giá tham chiếu không phải giá khớp bắt buộc. Căn cứ: `W12-C01`."
        },
        {
            "qid": "Q12-04",
            "lo": "LO-W12-02",
            "question": "Theo khung giá dịch vụ của Thông tư 128/2018/TT-BTC, phát biểu nào đúng?",
            "options": {
                "A": "Mọi công ty đều phải thu đúng một mức phí môi giới duy nhất.",
                "B": "Phí môi giới bị cấm hoàn toàn.",
                "C": "Spread và slippage là thuế do Bộ Tài chính thu.",
                "D": "Thông tư đặt mức tối đa/khung; tổ chức cung cấp dịch vụ quyết định mức cụ thể và phải kiểm tra biểu phí hiện hành."
            },
            "ans": "D",
            "bloom": "Understand", "cluster": "Khung phí dịch vụ", "position": ["Mục 4.3"], "claim": "(D đúng) W12-C02",
            "explanation": "Thông tư 128 đặt mức tối đa/khung và cho phép tổ chức dịch vụ quyết định giá cụ thể; người dùng phải xem biểu phí hiện hành. **A sai** vì mức cụ thể không đồng nhất giữa các nhà cung cấp. **B sai** vì phí dịch vụ được pháp luật thừa nhận trong khung. **C sai** vì spread/slippage là chi phí thị trường, không phải thuế nhà nước. Căn cứ: `W12-C02`."
        },
        {
            "qid": "Q12-05",
            "lo": "LO-W12-02",
            "question": "Trong ví dụ lịch sử/minh họa theo TT111/2013 và TT92/2015, bán 1.000 cổ phiếu giả định ở 20.000 VND/cổ phiếu; phí môi giới giả định 0,15%, khoản thuế minh họa 0,1%, không minimum/fixed fee hay khoản khác. Tiền ròng là bao nhiêu?",
            "options": {
                "A": "20.000.000 VND", "B": "19.950.000 VND", "C": "19.850.000 VND", "D": "20.050.000 VND"
            },
            "ans": "B",
            "bloom": "Apply", "cluster": "Tính phí và thuế", "position": ["Mục 5"], "claim": "(B đúng) 19.950.000 VND W12-C04",
            "explanation": "Giá trị bán = 1.000 × 20.000 = 20.000.000 VND. Phí = 30.000 VND; thuế minh họa = 20.000 VND. Ròng = 20.000.000 − 30.000 − 20.000 = **19.950.000 VND** (**B đúng**). **A sai** vì bỏ qua hai khoản; **C sai** vì trừ sai tổng; **D sai** vì cộng thay vì trừ. Đây không phải mức thuế hiện hành 2026. Căn cứ: `W12-C03`, `W12-C04`."
        },
        {
            "qid": "Q12-06",
            "lo": "LO-W12-03",
            "question": "**(Apply/Analyze)** Một ví dụ lịch sử/minh họa theo TT111/2013 và TT92/2015 dùng giao dịch bán 20.000.000 VND, phí 0,15%, không minimum/fixed fee và khoản thuế minh họa 0,1%. Nếu biểu phí thực tế có minimum fee 50.000 VND, kết luận nào đúng nhất?",
            "options": {
                "A": "Dùng phí 50.000 VND trong sensitivity check, tính tiền ròng minh họa 19.930.000 VND, rồi kiểm tra luật 2026 và điều khoản chuyển tiếp trước khi áp dụng.",
                "B": "Giữ nguyên phí 30.000 VND và coi 0,1% là mức thuế hiện hành năm 2026.",
                "C": "Bỏ toàn bộ thuế vì giao dịch đang lỗ theo giá vốn.",
                "D": "Dùng 5% trên giá trị bán vì minimum fee làm thay đổi loại thuế."
            },
            "ans": "A",
            "bloom": "Apply/Analyze", "cluster": "Khung lịch sử, minimum fee và sensitivity", "position": ["Mục 4.3", "Mục 5", "Mục 7"], "claim": "(A đúng) 19.930.000 VND; kiểm tra luật 2026",
            "explanation": "Phí gốc 30.000 VND; với minimum fee 50.000 VND, phí dùng trong ví dụ là 50.000 và tiền ròng minh họa là **19.930.000 VND**. **B sai** vì bỏ qua minimum fee và biến khung lịch sử thành hiện hành. **C sai** vì lỗ theo giá vốn không xóa giả định thuế. **D sai** vì minimum fee không đổi loại thuế. Khi dùng ngoài bài, phải đọc luật 2026 và điều khoản chuyển tiếp. Căn cứ: `W12-C03`, `W12-C04`, `W12-C05..C08`."
        },
        {
            "qid": "Q12-07",
            "lo": "LO-W12-03",
            "question": "**(Analyze)** Nhà đầu tư muốn dùng mức 5% trên cổ tức và 0,1% trên giá bán cho một giao dịch ngày 2026-08-10. Hành động nào phù hợp nhất với phạm vi W12?",
            "options": {
                "A": "Áp dụng ngay vì hai mức đã được dùng trong bài học.",
                "B": "Chỉ kiểm tra biểu phí công ty chứng khoán, không cần đọc văn bản thuế.",
                "C": "Ghi đây là khung lịch sử/minh họa, dừng kết luận và đọc các điều khoản thuế cùng chuyển tiếp hiện hành; metadata của văn bản mới chưa đủ để xác nhận mức.",
                "D": "Thay cả hai mức bằng 0% vì luật mới luôn miễn thuế cho nhà đầu tư cá nhân."
            },
            "ans": "C",
            "bloom": "Analyze", "cluster": "Kiểm tra luật hiện hành và chuyển tiếp", "position": ["Mục 4.3", "Mục 7"], "claim": "(C đúng) metadata chưa đủ xác nhận mức 2026",
            "explanation": "W12 chỉ dùng 5% và 0,1% trong **khung lịch sử/minh họa** dựa trên TT111/2013 và TT92/2015. Với ngày giao dịch 2026-08-10, phải đọc điều khoản thuế và chuyển tiếp đang có hiệu lực; metadata pending chưa đủ để kết luận mức. **A** và **D** suy đoán miễn/giảm thuế; **B** bỏ qua nguồn pháp lý. Căn cứ: `W12-C03`, `W12-C05..C08`."
        },
        {
            "qid": "Q12-08",
            "lo": "LO-W12-04",
            "question": "Checklist nào bảo vệ tốt nhất trước một lệnh trực tuyến?",
            "options": {
                "A": "Gửi mật khẩu và OTP cho môi giới để tránh thao tác nhầm.",
                "B": "Chỉ nhìn phí môi giới, bỏ qua spread, thanh khoản và thuế.",
                "C": "Dùng lệnh thị trường vì nó luôn có giá tốt nhất.",
                "D": "Xác nhận mục tiêu, mã/sàn, loại lệnh, chi phí, thuế, thanh khoản và bảo mật 2FA trước khi gửi lệnh."
            },
            "ans": "D",
            "bloom": "Evaluate", "cluster": "Checklist và bảo mật", "position": ["Mục 6", "Mục 8"], "claim": "(D đúng) W12-C01, W12-C02",
            "explanation": "Checklist đúng phải kết hợp mục tiêu, mã/sàn, loại lệnh, thanh khoản, phí, thuế và bảo mật 2FA. **A sai** vì OTP là thông tin bí mật. **B sai** vì bỏ qua chi phí ẩn và rủi ro. **C sai** vì lệnh thị trường không bảo đảm giá tốt nhất cả ngày. Căn cứ: `W12-C01`, `W12-C02`."
        }
    ]

    # W12 keeps the canonical matrix and lesson anchors used by its current artifacts.
    lesson_link = f"../../notes/module-{module_num}/{lesson_slug}.md"
    quiz_lines = [
        "> [!CAUTION]",
        "> Nội dung chỉ mang tính giáo dục, không phải lời khuyên đầu tư, thuế hay pháp lý.",
        "",
        f"# Quiz W12 — {title}",
        "",
        "**Hướng dẫn:** Chọn một đáp án đúng nhất. Mọi dữ kiện cần thiết nằm trong lesson W12; các số tiền là giả định trừ khi câu hỏi nêu rõ nguồn pháp lý.",
        ""
    ]
    for q in questions:
        quiz_lines.extend([
            f"### {q['qid']} — [{q['lo']}] {q['question']}",
            *(f"{letter}. {q['options'][letter]}" for letter in ("A", "B", "C", "D")),
            ""
        ])
    quiz_file = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "assessments", "quizzes", "week-12-quiz.md"))
    with open(quiz_file, "w", encoding="utf-8") as f:
        f.write("\n".join(quiz_lines).rstrip() + "\n")

    answer_lines = [
        "> [!CAUTION]",
        "> Nội dung chỉ mang tính giáo dục, không phải lời khuyên đầu tư, thuế hay pháp lý.",
        "",
        f"# Đáp án & giải thích W12 — {title}",
        "",
        f"**Bài học:** [Mở lesson tương ứng]({lesson_link})",
        "",
        "**Quy chuẩn:** Mỗi câu có LO, Bloom, vị trí ôn tập, claim và phân tích riêng từng phương án.",
        "",
        "## 1. Bảng Ma Trận Tra Cứu Nhanh",
        "| Câu | Đáp án | LO | Bloom | Cụm khái niệm | Vị trí lesson | Claim |",
        "|---|---|---|---|---|---|---|"
    ]
    lesson_anchors = {
        "Mục 3.1": "#31-bản-đồ-khái-niệm-nhanh",
        "Mục 4.2": "#42-lệnh-và-sổ-lệnh",
        "Mục 4.3": "#43-phí-thuế-và-chi-phí-ẩn",
        "Mục 5": "#5-ví-dụ-giải-từng-bước-worked-example",
        "Mục 6": "#6-quy-trình-thực-hành-trước-khi-đặt-lệnh",
        "Mục 7": "#7-điều-đã-sáng-tỏ-và-phần-còn-bất-định",
        "Mục 8": "#8-sai-lầm-phổ-biến-và-phản-biện",
    }
    for q in questions:
        positions = ", ".join(
            f"[{position}]({lesson_link}{lesson_anchors[position]})"
            for position in q["position"]
        )
        answer_lines.append(
            f"| {q['qid']} | **{q['ans']}** | [{q['lo']}] | {q['bloom']} | {q['cluster']} | {positions} | {q['claim']} |"
        )
    answer_lines.extend(["", "## 2. Hướng Dẫn Giải Chi Tiết Từng Câu"])
    for q in questions:
        answer_lines.extend([f"### {q['qid']} — Đáp án **{q['ans']}**", q["explanation"], ""])
    answer_lines.extend([
        "## 3. Tự Đánh Giá và Ôn Ngắt Quãng",
        "- **8/8:** phân biệt lệnh, tính đúng ví dụ và giải thích được cơ sở thuế/phí; chuyển sang bài ứng dụng Mục 10 lesson.",
        "- **6–7/8:** đọc lại Mục 4.2–4.3 và tự tính Q12-05 bằng số mới.",
        "- **Dưới 6/8:** học lại Fast Track, vẽ chuỗi tài khoản → lệnh → khớp → phí/thuế → tiền ròng trước khi làm lại.",
        "- **Ngày 1:** làm quiz không xem key.",
        "- **Ngày 3:** ôn flashcards W12.",
        "- **Ngày 7:** lập lại worked example với 50.000.000 VND.",
        "- **Ngày 14:** kiểm tra biểu phí và văn bản thuế hiện hành, không coi con số bài học là vĩnh viễn.",
        ""
    ])
    answer_file = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "assessments", "answer-keys", "week-12-answers.md"))
    with open(answer_file, "w", encoding="utf-8") as f:
        f.write("\n".join(answer_lines))
    print(f"✅ Generated W12 canonical artifacts: {[q['ans'] for q in questions]}")


def build_w13():
    title = "Các ngành kinh tế Việt Nam: ngân hàng, BĐS, công nghệ, tiêu dùng"
    module_num = 3
    lesson_slug = "week-13-phan-tich-nganh-viet-nam"

    questions = [
        {
            "qid": "Q13-01",
            "lo": "LO-W13-01",
            "question": "Trong phân tích cổ phiếu ngành Ngân hàng tại Việt Nam, hai chỉ số đặc thù quan trọng nhất để đánh giá chất lượng tài sản và an toàn vốn là:",
            "options": {
                "A": "Tỷ lệ nợ xấu (NPL - Non-Performing Loans) và Hệ số an toàn vốn (CAR - Capital Adequacy Ratio)",
                "B": "Hệ số giá trên doanh thu (P/S) và Tỷ lệ hoàn vốn đầu tư (ROI)",
                "C": "Biên lợi nhuận gộp sản xuất và Vòng quay hàng tồn kho",
                "D": "Tỷ lệ nợ trên vốn chủ sở hữu D/E tính theo mô hình doanh nghiệp công nghiệp"
            },
            "ans": "A",
            "explanation": "Ngân hàng kinh doanh tiền tệ; do đó NPL (đo lường tỷ lệ nợ nhóm 3 đến nhóm 5 trên tổng dư nợ) và CAR (theo chuẩn Basel II/III) là thước đo sống còn về chất lượng nợ và tấm đệm vốn chống đỡ rủi ro tín dụng (A đúng). B, C, D là các chỉ số áp dụng cho doanh nghiệp sản xuất và thương mại thông thường, không phù hợp cho ngân hàng."
        },
        {
            "qid": "Q13-02",
            "lo": "LO-W13-01",
            "question": "Đối với phân tích doanh nghiệp ngành Bất động sản dân dụng tại Việt Nam, yếu tố rủi ro then chốt nào thường quyết định khả năng ghi nhận doanh thu và sinh tồn của doanh nghiệp?",
            "options": {
                "A": "Chỉ số P/E ngắn hạn tính theo 4 quý gần nhất của doanh nghiệp",
                "B": "Tốc độ tăng số lượng nhân viên văn phòng tại trụ sở công ty",
                "C": "Giá sắt thép và xi măng trên thị trường giao ngay quốc tế",
                "D": "Tình trạng pháp lý của quỹ đất/dự án, tiến độ triển khai bàn giao và áp lực dòng tiền thanh toán nợ vay/trái phiếu"
            },
            "ans": "D",
            "explanation": "Bất động sản VN chịu rủi ro pháp lý dự án rất lớn (phê duyệt quy hoạch 1/500, tiền sử dụng đất, giấy phép xây dựng); nếu dự án vướng pháp lý sẽ không thể mở bán hay bàn giao để ghi nhận doanh thu, dẫn đến nghẽn dòng tiền trả nợ (D đúng). A sai vì P/E của BĐS rất ảo do doanh thu phụ thuộc vào thời điểm bàn giao dự án; B và C không phải yếu tố then chốt quyết định sự sống còn."
        },
        {
            "qid": "Q13-03",
            "lo": "LO-W13-02",
            "question": "Chỉ số NIM (Net Interest Margin - Biên lãi thuần) của một ngân hàng thương mại phản ánh điều gì?",
            "options": {
                "A": "Tỷ lệ nợ xấu được trích lập dự phòng rủi ro tín dụng",
                "B": "Chênh lệch giữa thu nhập lãi thuần thu được từ tài sản sinh lời và chi phí vốn huy động đầu vào của ngân hàng",
                "C": "Tỷ lệ tăng trưởng số lượng thẻ ATM phát hành mới trong năm",
                "D": "Tỷ lệ sở hữu của cổ đông chiến lược nước ngoài tại ngân hàng"
            },
            "ans": "B",
            "explanation": "NIM = Thu nhập lãi thuần / Tài sản sinh lời bình quân. Đây là thước đo hiệu quả kinh doanh cốt lõi của ngân hàng, cho thấy khả năng duy trì biên chênh lệch giữa lãi suất cho vay ra và lãi suất huy động tiền gửi vào (B đúng). A là tỷ lệ bao phủ nợ xấu (LLR); C là số liệu bán lẻ thứ yếu; D là tỷ lệ sở hữu vốn (room ngoại)."
        },
        {
            "qid": "Q13-04",
            "lo": "LO-W13-02",
            "question": "Khi phân tích các doanh nghiệp thuộc ngành Công nghệ Thông tin tại Việt Nam (như FPT), chỉ tiêu nào sau đây phản ánh rõ nét nhất tiềm năng tăng trưởng doanh thu tương lai?",
            "options": {
                "A": "Tỷ lệ cổ tức tiền mặt chi trả hàng năm của công ty",
                "B": "Giá trị tài sản máy móc thiết bị cũ được thanh lý trong kỳ",
                "C": "Giá trị hợp đồng ký mới được cam kết (Signed Backlog) và doanh thu từ thị trường chuyển đổi số toàn cầu",
                "D": "Chỉ số giá trị sổ sách P/B của doanh nghiệp"
            },
            "ans": "C",
            "explanation": "Doanh nghiệp công nghệ dịch vụ phần mềm tăng trưởng dựa trên các hợp đồng ký kết chuyển đổi số và xuất khẩu phần mềm sang các thị trường lớn (Mỹ, Nhật, EU); lượng hợp đồng đã ký nhưng chưa thực hiện (Backlog) là chỉ báo sớm đáng tin cậy nhất về doanh thu (C đúng). A phản ánh chính sách phân phối lợi nhuận; B là thanh lý tài sản không đáng kể; D ít ý nghĩa với công ty tài sản vô hình."
        },
        {
            "qid": "Q13-05",
            "lo": "LO-W13-03",
            "question": "Ngành Bán lẻ và Tiêu dùng (Consumer Goods / Retail) chịu sự chi phối mạnh mẽ nhất bởi các chỉ số vĩ mô nào sau đây?",
            "options": {
                "A": "Lãi suất tái chiết khấu trái phiếu chính phủ kỳ hạn 30 năm",
                "B": "Sản lượng khai thác dầu thô tại các mỏ ngoài khơi",
                "C": "Biến động tỷ giá hối đoái của đồng Yên Nhật so với Franc Thụy Sĩ",
                "D": "Tổng mức bán lẻ hàng hóa tiêu dùng xã hội, mức tăng trưởng thu nhập khả dụng của hộ gia đình và chỉ số lạm phát CPI"
            },
            "ans": "D",
            "explanation": "Doanh thu của ngành tiêu dùng - bán lẻ phụ thuộc trực tiếp vào sức mua (Purchasing Power) của người tiêu dùng, được phản ánh qua tổng mức bán lẻ toàn xã hội, thu nhập khả dụng và áp lực lạm phát làm xói mòn chi tiêu (D đúng). A, B, C là các chỉ số vĩ mô và hàng hóa không tác động trực diện đến sức mua tiêu dùng nội địa."
        },
        {
            "qid": "Q13-06",
            "lo": "LO-W13-04",
            "question": "Một sai lầm kinh điển của nhà đầu tư mới khi phân tích cổ phiếu ngành mang tính chu kỳ cao (Cyclical Sectors như Thép, Vận tải biển, Bất động sản) là gì?",
            "options": {
                "A": "Đua mua khi P/E của cổ phiếu ở mức thấp kỷ lục do lợi nhuận vừa đạt đỉnh chu kỳ, và bán cắt lỗ khi P/E rất cao vì lợi nhuận chạm đáy chu kỳ",
                "B": "Đọc kỹ báo cáo chu kỳ giá hàng hóa thế giới trước khi ra quyết định đầu tư",
                "C": "Theo dõi sát sao tình hình cung cầu công suất ngành trên phạm vi toàn cầu",
                "D": "Phân bổ tỷ trọng nhỏ và thận trọng khi cổ phiếu đã trải qua chu kỳ tăng nóng nhiều năm"
            },
            "ans": "A",
            "explanation": "Nghịch lý cổ phiếu chu kỳ: Ở đỉnh chu kỳ, lợi nhuận bùng nổ khiến P/E quá khứ trông cực kỳ rẻ (ví dụ P/E = 3), nhà đầu tư lao vào mua đúng đỉnh; khi chu kỳ đảo chiều, lợi nhuận sụt giảm 90% khiến P/E vọt lên rất cao, nhà đầu tư lại sợ hãi bán tháo đúng đáy (A đúng). B, C, D là các phương pháp phân tích ngành chu kỳ chuyên nghiệp và tỉnh táo."
        },
        {
            "qid": "Q13-07",
            "lo": "LO-W13-04",
            "question": "Rủi ro 'Tập trung tín dụng' (Credit Concentration Risk) của một ngân hàng thương mại phát sinh khi:",
            "options": {
                "A": "Ngân hàng mở rộng cho vay bán lẻ phân tán cho hàng triệu người tiêu dùng nhỏ lẻ",
                "B": "Ngân hàng tăng cường đầu tư vào trái phiếu chính phủ có độ an toàn tuyệt đối",
                "C": "Ngân hàng dồn tỷ trọng vốn cho vay quá lớn vào một lĩnh vực rủi ro cao (như bất động sản) hoặc một nhóm hệ sinh thái khách hàng liên quan",
                "D": "Ngân hàng đa dạng hóa nguồn thu nhập từ dịch vụ phi tín dụng (bancassurance, phí thanh toán)"
            },
            "ans": "C",
            "explanation": "Tập trung tín dụng là rủi ro nguy hiểm hàng đầu: nếu một tập đoàn bất động sản lớn hoặc một hệ sinh thái sân sau gặp khó khăn về dòng tiền, các khoản nợ xấu khổng lồ sẽ trực tiếp đe dọa sự an toàn vốn của toàn bộ ngân hàng đó (C đúng). A là chiến lược phân tán rủi ro chuẩn mực; B và D là các biện pháp gia tăng an toàn và đa dạng hóa nguồn thu."
        },
        {
            "qid": "Q13-08",
            "lo": "LO-W13-04",
            "question": "Khi phân tích doanh nghiệp ngành Thép tại Việt Nam (như Hòa Phát), hai biến số vĩ mô và hàng hóa quốc tế nào có tác động lớn nhất đến biên lợi nhuận của doanh nghiệp?",
            "options": {
                "A": "Giá hợp đồng tương lai của hạt cà phê Robusta và giá lúa gạo thế giới",
                "B": "Biến động giá quặng sắt, than cốc luyện kim (chi phí đầu vào) và giá thép thanh, thép cuộn cán nóng HRC (đầu ra)",
                "C": "Tỷ lệ thất nghiệp của ngành công nghệ phần mềm tại Thung lũng Silicon",
                "D": "Giá vé máy bay khứ hồi giữa Hà Nội và TP. Hồ Chí Minh"
            },
            "ans": "B",
            "explanation": "Hòa Phát sản xuất thép theo chu trình lò cao khép kín (BF-BOF); biên lợi nhuận phụ thuộc mật thiết vào chênh lệch giữa giá bán sản phẩm thép (thép xây dựng, HRC) và chi phí nguyên liệu đầu vào chủ chốt (quặng sắt, than mỡ cốc nhập khẩu) (B đúng). A, C, D hoàn toàn không liên quan đến chuỗi giá trị và cấu trúc chi phí ngành luyện kim."
        }
    ]

    build_week_assessment(13, title, module_num, lesson_slug, questions)


def build_w14():
    title = "Trái phiếu, quỹ mở và ETF tại Việt Nam"
    module_num = 3
    lesson_slug = "week-14-trai-phieu-quy-mo-etf-vn"

    questions = [
        {
            "qid": "Q14-01",
            "lo": "LO-W14-01",
            "question": "Về bản chất kinh tế và pháp lý, Trái phiếu doanh nghiệp (Corporate Bond) là loại công cụ tài chính nào?",
            "options": {
                "A": "Một chứng chỉ xác nhận quyền sở hữu một phần vốn cổ phần của công ty phát hành",
                "B": "Một hợp đồng quyền chọn mua cổ phiếu không bắt buộc hoàn trả vốn gốc",
                "C": "Một chứng chỉ nợ xác lập cam kết pháp lý của tổ chức phát hành về việc thanh toán lãi vay định kỳ và hoàn trả gốc khi đáo hạn",
                "D": "Một công cụ đầu tư hoàn toàn không có rủi ro được bảo lãnh thanh toán bởi ngân hàng nhà nước"
            },
            "ans": "C",
            "explanation": "Trái phiếu là công cụ vay nợ: nhà đầu tư đóng vai trò là chủ nợ của doanh nghiệp, có quyền ưu tiên thanh toán trước cổ đông khi doanh nghiệp giải thể nhưng không có quyền biểu quyết quản trị (C đúng). A mô tả cổ phiếu phổ thông; B là chứng quyền/quyền chọn; D là nhận thức sai lầm tai hại dẫn đến mất tiền trong các vụ khủng hoảng trái phiếu."
        },
        {
            "qid": "Q14-02",
            "lo": "LO-W14-01",
            "question": "Điểm khác biệt quan trọng nhất về cơ chế giao dịch giữa Quỹ mở truyền thống (Open-end Mutual Fund) và Quỹ hoán đổi danh mục (ETF) tại Việt Nam là gì?",
            "options": {
                "A": "Quỹ mở được niêm yết và giao dịch trực tiếp trên sàn chứng khoán từng giây, còn ETF chỉ được mua bán định kỳ hàng tháng",
                "B": "Quỹ mở mua bán trực tiếp với công ty quản lý quỹ theo giá trị tài sản ròng (NAV) cuối ngày, trong khi ETF được giao dịch trên sàn chứng khoán theo giá thị trường liên tục",
                "C": "Quỹ mở hoàn toàn không thu bất kỳ khoản phí quản lý nào, trong khi ETF thu phí rất cao",
                "D": "Không có bất kỳ sự khác biệt nào về mặt cơ chế giao dịch hay pháp lý"
            },
            "ans": "B",
            "explanation": "Quỹ mở được nhà đầu tư đặt lệnh mua/bán trực tiếp với công ty quản lý quỹ (qua đại lý phân phối) và khớp tại giá NAV/chứng chỉ quỹ xác định vào cuối ngày; ETF được niêm yết trên HOSE/HNX và nhà đầu tư có thể mua bán qua lại với nhau theo giá khớp lệnh từng giây (B đúng). A đảo ngược đặc điểm; C sai vì quỹ mở thường có phí quản lý cao hơn ETF; D phủ nhận cấu trúc thị trường."
        },
        {
            "qid": "Q14-03",
            "lo": "LO-W14-02",
            "question": "Mối quan hệ kinh điển giữa 'Giá thị trường của trái phiếu' và 'Lãi suất thị trường' diễn biến theo chiều hướng nào?",
            "options": {
                "A": "Biến động ngược chiều: khi mặt bằng lãi suất thị trường tăng lên, giá thị trường của các trái phiếu đang lưu hành sẽ giảm xuống và ngược lại",
                "B": "Biến động cùng chiều tuyệt đối: lãi suất tăng thì giá thị trường của trái phiếu luôn tăng theo cùng một tỷ lệ",
                "C": "Hoàn toàn độc lập và không có bất kỳ mối liên hệ nào với nhau",
                "D": "Giá trái phiếu luôn được cố định ở mức mệnh giá 100.000 đồng trong suốt vòng đời và không bao giờ biến động"
            },
            "ans": "A",
            "explanation": "Khi lãi suất thị trường tăng, các trái phiếu mới phát hành sẽ có mức coupon cao hơn, khiến các trái phiếu cũ có coupon thấp trở nên kém hấp dẫn và bị bán giảm giá trên thị trường thứ cấp để lợi suất đáo hạn (YTM) cân bằng lại (A đúng). B và C sai nguyên lý cơ bản của định giá công cụ nợ; D nhầm lẫn giữa mệnh giá kế toán (Face Value) với thị giá giao dịch (Market Price)."
        },
        {
            "qid": "Q14-04",
            "lo": "LO-W14-02",
            "question": "Giá trị tài sản ròng trên một đơn vị chứng chỉ quỹ (NAV per unit) được tính toán theo công thức chuẩn mực nào?",
            "options": {
                "A": "Tổng doanh thu của các công ty trong danh mục chia cho số lượng nhà đầu tư tham gia quỹ",
                "B": "Lấy giá cổ phiếu có thị giá cao nhất trong danh mục nhân với tổng số chứng chỉ quỹ",
                "C": "Tổng số tiền cổ tức nhận được trong năm chia cho mệnh giá ban đầu của chứng chỉ quỹ",
                "D": "(Tổng giá trị tài sản của quỹ theo thị giá - Tổng nợ phải trả của quỹ) chia cho tổng số lượng chứng chỉ quỹ đang lưu hành"
            },
            "ans": "D",
            "explanation": "NAV/CCQ = (Tổng tài sản - Nợ phải trả) / Số CCQ đang lưu hành. Đây là thước đo giá trị nội tại thực chất của mỗi đơn vị quỹ, được ngân hàng giám sát và công ty quản lý quỹ định giá hàng ngày dựa trên thị giá của tất cả các cổ phiếu/trái phiếu trong danh mục (D đúng). A, B, C là các phép tính tự chế không đúng chuẩn mực kế toán quỹ đầu tư."
        },
        {
            "qid": "Q14-05",
            "lo": "LO-W14-03",
            "question": "Rủi ro nào sau đây là rủi ro đặc thù và nghiêm trọng nhất khi đầu tư vào Trái phiếu Doanh nghiệp riêng lẻ tại Việt Nam?",
            "options": {
                "A": "Rủi ro được ngân hàng thương mại hoàn trả 100% vốn bất kể kết quả kinh doanh của doanh nghiệp",
                "B": "Rủi ro tín dụng (vỡ nợ không trả được gốc/lãi), rủi ro thanh khoản thị trường thứ cấp kém và rủi ro tài sản đảm bảo bị định giá khống",
                "C": "Trái phiếu doanh nghiệp hoàn toàn không có bất kỳ rủi ro nào vì luôn được kiểm toán bởi nhà nước",
                "D": "Chỉ duy nhất có rủi ro đồng tiền bị mất giá do lạm phát"
            },
            "ans": "B",
            "explanation": "Khủng hoảng trái phiếu tại VN giai đoạn 2022-2023 cho thấy rủi ro tín dụng (Default Risk) là vô cùng khốc liệt: doanh nghiệp mất khả năng thanh toán, tài sản đảm bảo là cổ phiếu không bán được hoặc đất dự án chưa đủ pháp lý, thanh khoản thứ cấp đóng băng (B đúng). A sai vì ngân hàng phân phối chỉ làm trung gian, không bảo lãnh thanh toán nợ thay; C và D xem nhẹ rủi ro vỡ nợ."
        },
        {
            "qid": "Q14-06",
            "lo": "LO-W14-04",
            "question": "Chỉ số 'Sai số theo dõi' (Tracking Error) của một quỹ ETF đo lường điều gì và nhà đầu tư mong muốn chỉ số này ở mức nào?",
            "options": {
                "A": "Đo lường mức lợi nhuận tối đa mà quỹ cam kết mang lại; mong muốn chỉ số này càng cao càng tốt",
                "B": "Đo lường tỷ lệ nợ vay margin của quỹ; mong muốn chỉ số này luôn bằng 100%",
                "C": "Đo lường mức độ biến động của chênh lệch hiệu suất giữa quỹ ETF và chỉ số tham chiếu; nhà đầu tư mong muốn chỉ số này càng nhỏ càng tốt",
                "D": "Đo lường số lượng nhân viên phân tích của công ty quản lý quỹ"
            },
            "ans": "C",
            "explanation": "Tracking Error (TE) đo độ lệch chuẩn của chênh lệch lợi nhuận giữa ETF và chỉ số tham chiếu. Một quỹ ETF mô phỏng chỉ số hoàn hảo phải có TE tiệm cận 0, chứng tỏ quỹ bám sát diễn biến của chỉ số một cách chuẩn xác (C đúng). A, B, D là những cách hiểu hoàn toàn sai lệch về mục tiêu mô phỏng thụ động của ETF."
        },
        {
            "qid": "Q14-07",
            "lo": "LO-W14-04",
            "question": "Sai lầm tai hại nhất của nhiều người dân khi mua Trái phiếu Doanh nghiệp qua các chi nhánh ngân hàng là gì?",
            "options": {
                "A": "Đọc kỹ bản cáo bạch và điều khoản hợp đồng trước khi ký kết mua trái phiếu",
                "B": "Thẩm định sức khỏe tài chính và năng lực trả nợ của tổ chức phát hành",
                "C": "Yêu cầu làm rõ sự khác biệt giữa chứng chỉ tiền gửi và trái phiếu doanh nghiệp",
                "D": "Nhầm tưởng trái phiếu doanh nghiệp an toàn như sổ tiết kiệm, ngộ nhận ngân hàng phân phối sẽ chịu trách nhiệm trả nợ thay khi công ty phát hành vỡ nợ"
            },
            "ans": "D",
            "explanation": "Rất nhiều nhà đầu tư cá nhân bị tư vấn mập mờ, cứ ngỡ mua trái phiếu tại quầy ngân hàng là 'tiết kiệm linh hoạt lãi suất cao', mà không hiểu rằng ngân hàng chỉ là đại lý phân phối hưởng hoa hồng; khi doanh nghiệp phát hành vỡ nợ, nhà đầu tư phải tự chịu toàn bộ rủi ro mất vốn (D đúng). A, B, C là các bước thẩm định khôn ngoan giúp tránh được bẫy lừa đảo."
        },
        {
            "qid": "Q14-08",
            "lo": "LO-W14-04",
            "question": "Trong Bản cáo bạch (Prospectus) của một quỹ mở hoặc quỹ ETF, thông tin nào sau đây là quan trọng nhất mà nhà đầu tư bắt buộc phải nghiên cứu kỹ trước khi rót vốn?",
            "options": {
                "A": "Mục tiêu, chiến lược đầu tư, cơ cấu biểu phí chi tiết (phí mua, bán, quản lý), rủi ro chính và phương pháp xác định NAV",
                "B": "Lời cam kết chắc chắn quỹ sẽ đạt lợi nhuận tối thiểu 20%/năm trong mọi hoàn cảnh",
                "C": "Danh sách họ tên và số điện thoại cá nhân của tất cả các nhà đầu tư khác tham gia quỹ",
                "D": "Biểu đồ dự báo chính xác giá của tất cả các cổ phiếu trên thị trường trong 5 năm tới"
            },
            "ans": "A",
            "explanation": "Bản cáo bạch là tài liệu pháp lý tối thượng của quỹ: nhà đầu tư phải nắm rõ quỹ đầu tư vào đâu (chiến lược), bị trừ những loại phí nào (phí mua/bán, TER quản lý hàng năm), rủi ro đối mặt là gì và cơ chế rút vốn thế nào (A đúng). B là cam kết bất hợp pháp bị pháp luật cấm; C vi phạm bảo mật thông tin cá nhân; D là điều viển vông."
        }
    ]

    build_week_assessment(14, title, module_num, lesson_slug, questions)


if __name__ == "__main__":
    build_w11()
    build_w12()
    build_w13()
    build_w14()
    print("All Module 3 assessments built successfully!")
