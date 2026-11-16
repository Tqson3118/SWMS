"""
Module 2 Assessments Generator (W05 - W10)
Standardizes:
- Balanced distribution: 2A, 2B, 2C, 2D
- Clean markdown formatting: A., B., C., D. (no leading dash)
- Coherent, detailed explanations analyzing correct answers and distractors
"""

import os
import sys

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")

sys.path.insert(0, os.path.dirname(__file__))
from common_builder import build_week_assessment


def build_w05():
    title = "Phân tích cơ bản: top-down và bottom-up"
    module_num = 2
    lesson_slug = "week-05-phan-tich-co-ban-top-down-bottom-up"

    questions = [
        {
            "qid": "Q05-01",
            "lo": "LO-W05-01",
            "question": "Trình tự phân tích từ trên xuống (Top-down) trong phân tích cơ bản diễn ra theo các bước nào?",
            "options": {
                "A": "Doanh nghiệp cụ thể -> Ngành kinh tế -> Kinh tế vĩ mô",
                "B": "Kinh tế vĩ mô -> Ngành kinh tế -> Doanh nghiệp cụ thể",
                "C": "Phân tích chỉ báo kỹ thuật -> Đọc bảng giá -> Mua theo tin tức",
                "D": "Chọn ngẫu nhiên cổ phiếu -> Kiểm tra ngành -> Đánh giá vĩ mô"
            },
            "ans": "B",
            "explanation": "Tiếp cận Top-down bắt đầu từ bức tranh kinh tế vĩ mô (tăng trưởng GDP, lạm phát, lãi suất), lọc ra các ngành được hưởng lợi chu kỳ, sau đó mới chọn doanh nghiệp vượt trội trong ngành đó (B đúng). A là quy trình tiếp cận từ dưới lên (Bottom-up); C là kỹ thuật và tâm lý đám đông; D không phải là phương pháp luận khoa học."
        },
        {
            "qid": "Q05-02",
            "lo": "LO-W05-01",
            "question": "Tiếp cận phân tích từ dưới lên (Bottom-up) tập trung ưu tiên hàng đầu vào yếu tố nào?",
            "options": {
                "A": "Dự báo tốc độ tăng trưởng kinh tế toàn cầu trong 10 năm tới",
                "B": "Xu hướng biến động của chỉ số giá cả hàng hóa thế giới",
                "C": "Năng lực cạnh tranh, mô hình kinh doanh, ban lãnh đạo và sức khỏe tài chính của từng doanh nghiệp cụ thể",
                "D": "Tin đồn trên các diễn đàn mạng xã hội về chính sách tiền tệ"
            },
            "ans": "C",
            "explanation": "Tiếp cận Bottom-up tin rằng doanh nghiệp có lợi thế cạnh tranh bền vững, ban lãnh đạo xuất sắc và tài chính lành mạnh sẽ vượt qua chu kỳ kinh tế và đem lại lợi nhuận vượt trội bất chấp vĩ mô ngắn hạn (C đúng). A và B là trọng tâm của phương pháp Top-down; D là thông tin nhiễu không có cơ sở phân tích."
        },
        {
            "qid": "Q05-03",
            "lo": "LO-W05-02",
            "question": "Trong các chỉ số định giá cơ bản, chỉ số P/E (Price-to-Earnings) so sánh giá cổ phiếu với:",
            "options": {
                "A": "Lợi nhuận sau thuế trên mỗi cổ phần (EPS)",
                "B": "Giá trị sổ sách trên mỗi cổ phần (BVPS)",
                "C": "Lợi nhuận trên vốn chủ sở hữu (ROE)",
                "D": "Tổng nợ phải trả trên vốn chủ sở hữu (D/E)"
            },
            "ans": "A",
            "explanation": "P/E so sánh mức giá thị trường mà nhà đầu tư sẵn sàng trả cho 1 đồng thu nhập EPS của doanh nghiệp (A đúng). B là mẫu số của chỉ số P/B; C là chỉ số hiệu quả sử dụng vốn ROE; D là chỉ số đòn bẩy tài chính D/E."
        },
        {
            "qid": "Q05-04",
            "lo": "LO-W05-02",
            "question": "Mô hình '5 lực lượng cạnh tranh' của Michael Porter (Porter's Five Forces) thường được ứng dụng ở bước nào trong phân tích Top-down?",
            "options": {
                "A": "Phân tích chính sách tiền tệ của ngân hàng trung ương",
                "B": "Dự báo tỷ giá hối đoái của các đồng tiền dự trữ quốc tế",
                "C": "Tính toán dòng tiền chiết khấu của một dự án đơn lẻ",
                "D": "Đánh giá cấu trúc cạnh tranh, rào cản gia nhập và mức độ hấp dẫn của ngành kinh tế"
            },
            "ans": "D",
            "explanation": "Khung 5 áp lực cạnh tranh của Porter (áp lực từ nhà cung cấp, khách hàng, đối thủ tiềm tàng, sản phẩm thay thế, và cạnh tranh nội ngành) là công cụ kinh điển để phân tích cấu trúc và triển vọng lợi nhuận ngành (D đúng). A và B thuộc phân tích vĩ mô cấp độ cao; C là bước định giá doanh nghiệp hoặc dự án cụ thể."
        },
        {
            "qid": "Q05-05",
            "lo": "LO-W05-03",
            "question": "Khi phân tích kinh tế vĩ mô Việt Nam, chính sách nào sau đây tác động trực tiếp nhất đến thanh khoản hệ thống ngân hàng và chi phí vốn của doanh nghiệp?",
            "options": {
                "A": "Chính sách tiền tệ (lãi suất điều hành, hạn mức tín dụng và tỷ lệ dự trữ bắt buộc)",
                "B": "Quy định về trang phục công sở của cán bộ công chức",
                "C": "Kế hoạch tổ chức các sự kiện văn hóa thể thao quốc tế",
                "D": "Chỉ tiêu biên độ biến động giá vàng nhẫn trong tuần"
            },
            "ans": "A",
            "explanation": "Chính sách tiền tệ của Ngân hàng Nhà nước (qua lãi suất tái cấp vốn, tái chiết khấu, room tín dụng và dự trữ bắt buộc) điều tiết nguồn cung tiền, trực tiếp tác động lên mặt bằng lãi suất cho vay và chi phí vốn của toàn bộ nền kinh tế (A đúng). B, C, D không phải các công cụ chính sách kinh tế vĩ mô điều tiết vốn và thanh khoản."
        },
        {
            "qid": "Q05-06",
            "lo": "LO-W05-03",
            "question": "Một doanh nghiệp có biên lợi nhuận gộp (Gross Margin) cao và duy trì ổn định qua nhiều năm thường phản ánh:",
            "options": {
                "A": "Doanh nghiệp đang sử dụng tỷ lệ đòn bẩy tài chính quá mức rủi ro",
                "B": "Doanh nghiệp sở hữu lợi thế cạnh tranh bền vững (moat) và có quyền định giá với khách hàng",
                "C": "Doanh nghiệp không phải nộp bất kỳ khoản thuế nào cho nhà nước",
                "D": "Giá cổ phiếu trên thị trường chắc chắn sẽ tăng trần trong 5 phiên liên tiếp"
            },
            "ans": "B",
            "explanation": "Biên lợi nhuận gộp cao và ổn định chứng tỏ sản phẩm/dịch vụ của công ty có sự khác biệt hóa, khách hàng chấp nhận trả giá cao (pricing power) hoặc công ty có lợi thế quy mô kiểm soát giá vốn hàng bán (B đúng). A liên quan đến cấu trúc nợ chứ không phải biên lợi nhuận gộp; C sai thực tế kế toán và thuế; D là phỏng đoán ngắn hạn phi lý."
        },
        {
            "qid": "Q05-07",
            "lo": "LO-W05-04",
            "question": "Sai lầm phổ biến nhất khi áp dụng phương pháp định giá so sánh P/E là gì?",
            "options": {
                "A": "Luôn so sánh P/E của công ty với P/E trung bình quá khứ 5 năm của chính nó",
                "B": "Đọc kỹ báo cáo tài chính để loại trừ các khoản thu nhập bất thường trước khi tính EPS",
                "C": "Kiểm tra tốc độ tăng trưởng lợi nhuận kỳ vọng tương ứng với mức P/E",
                "D": "So sánh P/E giữa các doanh nghiệp thuộc các ngành nghề hoàn toàn khác nhau mà không xét đến tốc độ tăng trưởng và mức độ rủi ro"
            },
            "ans": "D",
            "explanation": "P/E giữa các ngành không thể so sánh ngang bằng (ví dụ công nghệ tăng trưởng cao thường có P/E cao hơn ngành tiện ích hay ngân hàng); so sánh chéo ngành mà không chuẩn hóa tốc độ tăng trưởng và cấu trúc vốn là sai lầm phương pháp luận nghiêm trọng (D đúng). A, B, C là các nguyên tắc thẩm định P/E đúng đắn và cần thiết."
        },
        {
            "qid": "Q05-08",
            "lo": "LO-W05-04",
            "question": "Để đánh giá chất lượng tăng trưởng doanh thu của một doanh nghiệp bán lẻ, chỉ số nào sau đây mang tính bản chất nhất?",
            "options": {
                "A": "Tăng trưởng số lượng nhân viên bảo vệ tại trụ sở chính",
                "B": "Tỷ lệ tăng giá cổ phiếu trong 3 phiên giao dịch gần nhất",
                "C": "Tăng trưởng doanh số trên cùng một cửa hàng hiện hữu (SSSG - Same-Store Sales Growth)",
                "D": "Tổng số lượng tài khoản theo dõi trang mạng xã hội của doanh nghiệp"
            },
            "ans": "C",
            "explanation": "SSSG đo lường mức tăng doanh thu từ các cửa hàng đã hoạt động trên 1 năm, giúp loại bỏ ảo tưởng tăng trưởng do mở ồ ạt cửa hàng mới kém hiệu quả, phản ánh sức hấp dẫn thực chất của thương hiệu và sức mua của khách hàng (C đúng). A, B, D là các chỉ số thứ yếu hoặc không phản ánh hiệu quả kinh doanh cốt lõi."
        }
    ]

    build_week_assessment(5, title, module_num, lesson_slug, questions)


def build_w06():
    title = "Phân tích kỹ thuật cơ bản: trendline, support/resistance, moving average"
    module_num = 2
    lesson_slug = "week-06-phan-tich-ky-thuat-co-ban"

    questions = [
        {
            "qid": "Q06-01",
            "lo": "LO-W06-01",
            "question": "Trong biểu đồ nến Nhật Bản (Candlestick chart), mỗi cây nến tiêu chuẩn chứa 4 mức giá then chốt (OHLC) nào?",
            "options": {
                "A": "Giá trần, Giá sàn, Giá tham chiếu và Giá bình quân",
                "B": "Doanh thu, Chi phí, Lợi nhuận gộp và Lợi nhuận ròng",
                "C": "Giá mở cửa (Open), Giá cao nhất (High), Giá thấp nhất (Low) và Giá đóng cửa (Close)",
                "D": "Giá mua vào, Giá bán ra, Phí giao dịch và Thuế chuyển nhượng"
            },
            "ans": "C",
            "explanation": "Một cây nến chuẩn Nhật Bản cấu thành từ 4 mức giá trong phiên: Open (mở cửa), High (cao nhất), Low (thấp nhất), và Close (đóng cửa), trong đó thân nến biểu thị khoảng giữa Open và Close, bóng nến biểu thị High và Low (C đúng). A là các mức giá quy định của biên độ sàn giao dịch VN; B là các chỉ tiêu báo cáo tài chính; D là các tham số chi phí giao dịch."
        },
        {
            "qid": "Q06-02",
            "lo": "LO-W06-01",
            "question": "Đường trung bình động đơn giản (SMA - Simple Moving Average) chu kỳ 50 ngày được tính toán như thế nào?",
            "options": {
                "A": "Lấy giá cao nhất trừ giá thấp nhất của 50 phiên chia đôi",
                "B": "Nhân giá phiên hiện tại với 50 rồi chia cho khối lượng giao dịch",
                "C": "Trung bình cộng khối lượng giao dịch của 50 phiên nhân với chỉ số VN-Index",
                "D": "Trung bình cộng số học giá đóng cửa của 50 phiên giao dịch gần nhất"
            },
            "ans": "D",
            "explanation": "SMA 50 là trung bình cộng số học giản đơn của giá đóng cửa trong 50 phiên liên tiếp gần nhất, làm mượt biến động giá hàng ngày để nhận diện xu hướng trung hạn (D đúng). A là tính mức giá trung vị của biên độ; B và C là các phép tính tự chế không đúng công thức toán học của moving average."
        },
        {
            "qid": "Q06-03",
            "lo": "LO-W06-02",
            "question": "Đường trung bình động hàm mũ (EMA - Exponential Moving Average) có ưu điểm gì so với đường SMA cùng chu kỳ?",
            "options": {
                "A": "Hoàn toàn loại bỏ được 100% các tín hiệu nhiễu và tín hiệu giả",
                "B": "Đặt trọng số lớn hơn vào dữ liệu giá gần nhất, giúp phản ứng nhạy bén hơn với các biến động giá mới",
                "C": "Đảm bảo chắc chắn nhà đầu tư mua đúng đáy và bán đúng đỉnh tuyệt đối",
                "D": "Chỉ áp dụng được cho thị trường chứng khoán quốc tế, không dùng được tại Việt Nam"
            },
            "ans": "B",
            "explanation": "EMA áp dụng hệ số làm mượt theo cấp số nhân, gán trọng số cao hơn cho các phiên giao dịch gần nhất, giúp chỉ báo giảm độ trễ (lag) so với SMA và phản ánh sự đổi hướng của giá nhanh hơn (B đúng). A và C là những kỳ vọng viển vông vì mọi chỉ báo kỹ thuật đều có độ trễ và sinh ra tín hiệu giả; D sai vì EMA dùng được trên mọi thị trường có dữ liệu giá."
        },
        {
            "qid": "Q06-04",
            "lo": "LO-W06-02",
            "question": "Ngưỡng hỗ trợ (Support level) trên biểu đồ giá được định nghĩa là:",
            "options": {
                "A": "Vùng giá mà tại đó lực cầu mua vào kỳ vọng đủ mạnh để kìm hãm đà giảm và giúp giá phục hồi",
                "B": "Mức giá cao nhất mọi thời đại mà cổ phiếu đó từng đạt được",
                "C": "Vùng giá mà tại đó áp lực bán tháo của phe bán áp đảo hoàn toàn phe mua",
                "D": "Mức giá tương ứng với giá trị sổ sách BVPS của doanh nghiệp theo kế toán"
            },
            "ans": "A",
            "explanation": "Vùng hỗ trợ là nơi tập trung lực cầu mua vào của thị trường (do tâm lý thấy giá rẻ hoặc điểm cân bằng cung cầu), khiến đà giảm chậm lại hoặc đảo chiều tăng lên (A đúng). B là đỉnh lịch sử (All-Time High); C là mô tả của vùng kháng cự (Resistance); D là thước đo định giá cơ bản, không phải khái niệm phân tích kỹ thuật."
        },
        {
            "qid": "Q06-05",
            "lo": "LO-W06-03",
            "question": "Khi một đường xu hướng tăng (Uptrend line) bị giá xuyên thủng dứt khoát đi kèm khối lượng giao dịch (volume) tăng vọt, tín hiệu này cảnh báo điều gì?",
            "options": {
                "A": "Xu hướng tăng chắc chắn sẽ bùng nổ mạnh mẽ hơn gấp 3 lần",
                "B": "Doanh nghiệp vừa công bố báo cáo tài chính có lãi kỷ lục",
                "C": "Lập tức sử dụng tối đa đòn bẩy margin để mua thêm trung bình giá",
                "D": "Xu hướng tăng đã bị phá vỡ, cảnh báo nguy cơ đảo chiều sang xu hướng giảm hoặc chuyển sang pha tích lũy/đi ngang"
            },
            "ans": "D",
            "explanation": "Phá vỡ trendline tăng (Breakdown) với khối lượng lớn là tín hiệu kỹ thuật quan trọng cho thấy phe bán đã chiếm ưu thế, cấu trúc đỉnh cao hơn - đáy cao hơn bị phá vỡ, cảnh báo xu hướng tăng kết thúc (D đúng). A và C là hành vi giao dịch ngược xu hướng cực kỳ nguy hiểm; B là thông tin cơ bản không suy ra trực tiếp từ biến động kỹ thuật."
        },
        {
            "qid": "Q06-06",
            "lo": "LO-W06-03",
            "question": "Hiện tượng 'Kháng cự cũ trở thành hỗ trợ mới' (Role Reversal) xảy ra khi nào?",
            "options": {
                "A": "Khi giá liên tục giảm sâu và xuyên thủng qua đáy cũ lịch sử",
                "B": "Khi ban lãnh đạo doanh nghiệp mua vào cổ phiếu quỹ",
                "C": "Khi giá bứt phá vượt qua ngưỡng kháng cự mạnh, sau đó nhịp điều chỉnh quay lại kiểm định mức giá này và giữ vững được đà tăng",
                "D": "Khi sàn giao dịch áp dụng biên độ dao động giá mới từ 7% lên 10%"
            },
            "ans": "C",
            "explanation": "Khi ngưỡng kháng cự bị phá vỡ (Breakout thành công), tâm lý thị trường đảo chiều: những người đã bán non hoặc lỡ nhịp sẵn sàng mua vào khi giá quay lại mức này, biến vùng cản cũ thành bệ đỡ hỗ trợ mới (C đúng). A là hiện tượng tiếp diễn xu hướng giảm; B và D là các sự kiện doanh nghiệp và quy chế giao dịch."
        },
        {
            "qid": "Q06-07",
            "lo": "LO-W06-04",
            "question": "Chỉ báo RSI (Relative Strength Index) nằm trên mức 70 và xuất hiện hiện tượng 'Phân kỳ âm' (Bearish Divergence - giá lập đỉnh cao mới nhưng RSI tạo đỉnh thấp hơn) thường cảnh báo:",
            "options": {
                "A": "Động lượng tăng giá đang suy yếu dần và tiềm ẩn rủi ro đảo chiều điều chỉnh giảm",
                "B": "Cổ phiếu đang được định giá quá rẻ theo chỉ số P/E",
                "C": "Dòng tiền lớn của khối ngoại đang gom mua quyết liệt toàn bộ thị trường",
                "D": "Tín hiệu xác nhận chắc chắn giá cổ phiếu sẽ tăng thêm 50% trong tuần tới"
            },
            "ans": "A",
            "explanation": "RSI > 70 cho thấy thị trường đang ở vùng quá mua (Overbought); khi giá tạo đỉnh cao mới nhưng RSI suy yếu tạo đỉnh thấp hơn (phân kỳ âm), nó báo hiệu động lực mua yếu đi và nguy cơ điều chỉnh là rất cao (A đúng). B nhầm sang định giá P/E cơ bản; C và D hoàn toàn trái ngược với bản chất tín hiệu cảnh báo rủi ro của phân kỳ âm."
        },
        {
            "qid": "Q06-08",
            "lo": "LO-W06-04",
            "question": "Sai lầm phổ biến nhất của người mới học Phân tích Kỹ thuật (Technical Analysis) là gì?",
            "options": {
                "A": "Luôn xác định xu hướng chính trên khung thời gian lớn trước khi vào lệnh",
                "B": "Tin rằng chỉ báo kỹ thuật luôn dự báo chính xác 100% tương lai và từ chối tuân thủ kỷ luật dừng lỗ khi thị trường đi ngược kỳ vọng",
                "C": "Kết hợp quan sát khối lượng giao dịch (Volume) cùng với biến động giá",
                "D": "Ghi nhật ký giao dịch để đánh giá xác suất thắng/thua của từng chiến lược"
            },
            "ans": "B",
            "explanation": "Phân tích kỹ thuật là trò chơi của xác suất và quản trị rủi ro; không có mô hình nào đúng 100%. Sai lầm chết người là biến nhận định thành niềm tin mù quáng và không chấp nhận cắt lỗ khi mô hình kỹ thuật bị vi phạm (B đúng). A, C, D là các nguyên tắc kỷ luật chuẩn mực của nhà giao dịch chuyên nghiệp."
        }
    ]

    build_week_assessment(6, title, module_num, lesson_slug, questions)


def build_w07():
    title = "Quản trị rủi ro, Position Sizing và Stop Loss"
    module_num = 2
    lesson_slug = "week-07-quan-tri-rui-ro-position-sizing-stop-loss"

    questions = [
        {
            "qid": "Q07-01",
            "lo": "LO-W07-01",
            "question": "Trong quản trị danh mục, khái niệm 'Position Sizing' (Quy mô vị thế) nhằm mục đích chính là gì?",
            "options": {
                "A": "Mua tối đa số lượng cổ phiếu có thể bằng toàn bộ vốn và tiền vay margin",
                "B": "Đảm bảo chắc chắn mọi giao dịch đều mang lại lợi nhuận dương",
                "C": "Dự báo chính xác cổ phiếu nào sẽ tăng trần trong phiên giao dịch hôm nay",
                "D": "Xác định quy mô vốn và khối lượng cổ phiếu cho mỗi thương vụ sao cho mức lỗ tối đa không vượt quá giới hạn an toàn cho phép của tổng tài sản"
            },
            "ans": "D",
            "explanation": "Position Sizing là kỹ thuật định cỡ vị thế: dựa vào khoảng cách cắt lỗ và số tiền sẵn sàng mất trên một lệnh (ví dụ 1-2% tổng danh mục) để tính ra số cổ phiếu được phép mua, bảo vệ tài khoản khỏi thua lỗ lớn (D đúng). A là hành vi đánh bạc thiếu kỷ luật; B và C là những mục tiêu bất khả thi trong thị trường tài chính."
        },
        {
            "qid": "Q07-02",
            "lo": "LO-W07-01",
            "question": "Nhà đầu tư có tài khoản 100 triệu đồng, áp dụng quy tắc rủi ro tối đa 2% tổng tài sản cho một giao dịch. Số tiền rủi ro tối đa cho phép mất trên thương vụ này là:",
            "options": {
                "A": "2.000.000 đồng",
                "B": "500.000 đồng",
                "C": "10.000.000 đồng",
                "D": "20.000.000 đồng"
            },
            "ans": "A",
            "explanation": "Số tiền rủi ro tối đa = 100.000.000 × 2% = 2.000.000 đồng (A đúng). B là 0,5%; C là 10% (rủi ro quá cao phá vỡ nguyên tắc); D là 20% (nguy cơ sụt giảm vốn nghiêm trọng)."
        },
        {
            "qid": "Q07-03",
            "lo": "LO-W07-02",
            "question": "Một nhà đầu tư mua cổ phiếu tại giá 50.000 đồng/cp và đặt lệnh Stop Loss tại giá 46.000 đồng/cp. Nếu mức rủi ro tối đa cho phép chấp nhận mất là 2.000.000 đồng, số lượng cổ phiếu tối đa nhà đầu tư này được phép mua là bao nhiêu?",
            "options": {
                "A": "100 cổ phiếu",
                "B": "250 cổ phiếu",
                "C": "500 cổ phiếu",
                "D": "1.000 cổ phiếu"
            },
            "ans": "C",
            "explanation": "Khoảng cách cắt lỗ trên mỗi cổ phiếu = 50.000 - 46.000 = 4.000 đồng/cp. Số lượng cổ phiếu được phép mua = Rủi ro tối đa / Rủi ro mỗi cổ phiếu = 2.000.000 / 4.000 = 500 cổ phiếu (C đúng). A, B mua dưới quy mô cho phép; D mua 1.000 cp sẽ chịu rủi ro 4.000.000 đồng (gấp đôi hạn mức 2%)."
        },
        {
            "qid": "Q07-04",
            "lo": "LO-W07-02",
            "question": "Tỷ lệ Lợi nhuận / Rủi ro (Risk/Reward Ratio - R:R) đạt 2:1 có nghĩa là:",
            "options": {
                "A": "Chấp nhận mất 2 đồng vốn để hy vọng kiếm được 1 đồng lãi",
                "B": "Lợi nhuận kỳ vọng đạt được lớn gấp 2 lần mức rủi ro tiềm năng chấp nhận cắt lỗ",
                "C": "Tỷ lệ xác suất giao dịch thành công luôn đạt 200%",
                "D": "Danh mục được phân bổ 2 phần tiền mặt và 1 phần cổ phiếu"
            },
            "ans": "B",
            "explanation": "Tỷ lệ Risk/Reward = 2:1 (hoặc Reward/Risk = 2:1) nghĩa là nếu giao dịch đúng mục tiêu sẽ lãi 2 đơn vị, nếu sai chạm stop loss sẽ mất 1 đơn vị; tỷ lệ này giúp tài khoản tăng trưởng dù tỷ lệ lệnh thắng chỉ ở mức 40-50% (B đúng). A là tỷ lệ ngược bất lợi; C sai về mặt xác suất; D là tỷ lệ phân bổ tài sản."
        },
        {
            "qid": "Q07-05",
            "lo": "LO-W07-03",
            "question": "Khái niệm 'Maximum Drawdown' (Mức sụt giảm tài sản tối đa) đo lường điều gì trong đánh giá rủi ro danh mục?",
            "options": {
                "A": "Tỷ suất sinh lời trung bình hàng năm của danh mục đầu tư",
                "B": "Mức phí hoa hồng giao dịch tối đa phải trả cho công ty chứng khoán",
                "C": "Phần trăm sụt giảm lớn nhất của giá trị tài khoản từ mức đỉnh cao nhất xuống đáy sâu nhất trước khi phục hồi",
                "D": "Tỷ lệ nợ vay margin tối đa mà nhà đầu tư được phép sử dụng"
            },
            "ans": "C",
            "explanation": "Maximum Drawdown (MDD) đo lường mức giảm sâu nhất từ đỉnh (Peak) xuống đáy (Trough) của danh mục, phản ánh mức độ đau đớn tâm lý và rủi ro vốn tồi tệ nhất mà chiến lược đầu tư từng trải qua (C đúng). A là tỷ suất sinh lời; B là chi phí giao dịch; D là hạn mức đòn bẩy."
        },
        {
            "qid": "Q07-06",
            "lo": "LO-W07-03",
            "question": "Một tài khoản đầu tư bị thua lỗ 50% giá trị vốn ban đầu. Để phục hồi trở lại mức vốn ban đầu, tài khoản đó bắt buộc phải đạt tỷ suất sinh lời là bao nhiêu trên số vốn còn lại?",
            "options": {
                "A": "100%",
                "B": "50%",
                "C": "75%",
                "D": "200%"
            },
            "ans": "A",
            "explanation": "Khi mất 50% (từ 100 triệu còn 50 triệu), để quay lại 100 triệu cần kiếm được thêm 50 triệu trên số vốn mới 50 triệu, tương đương mức lãi 100% (A đúng). B là sai lầm phổ biến do ảo tưởng tỷ lệ đối xứng (mất 50% nhưng lãi 50% chỉ lên lại 75 triệu); C và D tính toán sai công thức phục hồi vốn."
        },
        {
            "qid": "Q07-07",
            "lo": "LO-W07-04",
            "question": "Hiện tượng 'Lệnh dừng lỗ bị trượt giá' (Slippage) thường xảy ra trong bối cảnh thị trường nào?",
            "options": {
                "A": "Khi thị trường giao dịch êm đềm, thanh khoản dồi dào và giá đi ngang",
                "B": "Khi thị trường giảm sàn hoảng loạn, xuất hiện khoảng trống giá giảm (Gap down) hoặc mất thanh khoản trắng bên mua",
                "C": "Khi công ty chứng khoán giảm phí giao dịch xuống 0%",
                "D": "Khi nhà đầu tư đặt lệnh giới hạn (Limit Order) với mức giá thấp hơn thị giá"
            },
            "ans": "B",
            "explanation": "Trượt giá (Slippage) là chênh lệch giữa giá đặt stop loss và giá khớp lệnh thực tế; điều này thường xảy ra khi thị trường mở cửa gap giảm mạnh hoặc rơi vào cảnh dư bán sàn hàng triệu đơn vị mà không có người mua (B đúng). A thanh khoản cao thì trượt giá rất nhỏ; C và D không phải nguyên nhân tạo ra slippage trong cắt lỗ."
        },
        {
            "qid": "Q07-08",
            "lo": "LO-W07-04",
            "question": "Sai lầm tâm lý nghiêm trọng nhất khi thực thi kỷ luật quản trị rủi ro Stop Loss là gì?",
            "options": {
                "A": "Tuân thủ nghiêm ngặt việc đặt lệnh dừng lỗ tự động ngay sau khi khớp lệnh mua",
                "B": "Luôn ghi chép lại lý do cắt lỗ vào nhật ký giao dịch để rút kinh nghiệm",
                "C": "Chấp nhận mức lỗ nhỏ theo kế hoạch đã vạch ra từ trước",
                "D": "Tự ý dời điểm dừng lỗ xuống thấp hơn hoặc hủy bỏ lệnh dừng lỗ khi thị trường giảm chạm ngưỡng vì tâm lý sợ mất tiền và ôm hy vọng hão huyền"
            },
            "ans": "D",
            "explanation": "'Nới stop loss' là sai lầm chết người của nhà đầu tư: khi giá chạm điểm vi phạm, thay vì chấp nhận khoản lỗ nhỏ theo kế hoạch, nhà đầu tư lại dời điểm dừng lỗ với hy vọng giá hồi, biến một khoản lỗ có kiểm soát 5-7% thành khoản lỗ thảm họa 30-50% (D đúng). A, B, C là các thói quen quản trị rủi ro chuyên nghiệp."
        }
    ]

    build_week_assessment(7, title, module_num, lesson_slug, questions)


def build_w08():
    title = "Danh mục, Asset Allocation và Modern Portfolio Theory"
    module_num = 2
    lesson_slug = "week-08-danh-muc-phan-bo-tai-san-mpt"

    questions = [
        {
            "qid": "Q08-01",
            "lo": "LO-W08-01",
            "question": "Trong Lý thuyết Danh mục Hiện đại (MPT - Modern Portfolio Theory) của Harry Markowitz, 'Phân bổ tài sản' (Asset Allocation) được định nghĩa là:",
            "options": {
                "A": "Quyết định phân chia tỷ trọng vốn đầu tư vào các lớp tài sản khác nhau (như cổ phiếu, trái phiếu, tiền gửi, vàng)",
                "B": "Hoạt động lướt sóng mua bán liên tục từng mã cổ phiếu theo biểu đồ kỹ thuật từng phút",
                "C": "Việc vay nợ đòn bẩy tối đa để dồn 100% tiền vào duy nhất một mã cổ phiếu tiềm năng",
                "D": "Hành động rút toàn bộ tiền mặt cất giữ tại nhà không tham gia vào nền kinh tế"
            },
            "ans": "A",
            "explanation": "Asset Allocation là chiến lược cốt lõi quyết định phần lớn lợi nhuận và rủi ro dài hạn: phân chia tài sản thành các rổ có đặc tính sinh lời và rủi ro khác nhau (A đúng). B là giao dịch trong ngày (day trading); C là tập trung rủi ro cực đoan vi phạm nguyên lý đa dạng hóa; D là hành vi tích trữ tiền mặt phi hiệu quả."
        },
        {
            "qid": "Q08-02",
            "lo": "LO-W08-01",
            "question": "Lợi ích lớn nhất của việc kết hợp các tài sản có hệ số tương quan (Correlation) thấp hoặc âm vào cùng một danh mục đầu tư là:",
            "options": {
                "A": "Đảm bảo chắc chắn loại bỏ được 100% mọi rủi ro thị trường và rủi ro chính trị",
                "B": "Giảm phương sai và độ biến động chung của danh mục mà không làm suy giảm tương ứng lợi nhuận kỳ vọng",
                "C": "Đảm bảo tất cả các tài sản trong danh mục đều tăng giá mạnh cùng một lúc",
                "D": "Giúp nhà đầu tư không bao giờ phải chịu thuế thu nhập cá nhân"
            },
            "ans": "B",
            "explanation": "Khi các tài sản không biến động cùng chiều (tương quan thấp hoặc âm), mức giảm giá của tài sản này được bù đắp bởi sự ổn định của tài sản khác, giúp 'làm mượt' đường cong tài sản và hạ thấp rủi ro phi hệ thống (B đúng). A sai vì không loại bỏ được rủi ro hệ thống (thị trường sụp đổ chung); C sai vì nếu cùng tăng thì tương quan là dương tuyệt đối (+1); D không liên quan đến chính sách thuế."
        },
        {
            "qid": "Q08-03",
            "lo": "LO-W08-02",
            "question": "Trong quản trị danh mục, thước đo độ lệch chuẩn (Standard Deviation - σ) của tỷ suất sinh lời phản ánh điều gì?",
            "options": {
                "A": "Tốc độ tăng trưởng doanh thu hàng năm của doanh nghiệp lớn nhất trong danh mục",
                "B": "Tổng số lượng mã cổ phiếu đang được nắm giữ trong tài khoản",
                "C": "Mức cổ tức tiền mặt tối thiểu được cam kết chi trả trong năm",
                "D": "Độ biến động và mức độ phân tán của tỷ suất sinh lời xung quanh giá trị trung bình kỳ vọng"
            },
            "ans": "D",
            "explanation": "Độ lệch chuẩn (Standard Deviation) đo lường mức độ phân tán của lợi nhuận thực tế so với lợi nhuận trung bình kỳ vọng; độ lệch chuẩn càng lớn thì biên độ dao động giá càng cao, đồng nghĩa với rủi ro biến động càng lớn (D đúng). A, B, C không phải là ý nghĩa thống kê của độ lệch chuẩn."
        },
        {
            "qid": "Q08-04",
            "lo": "LO-W08-02",
            "question": "Khái niệm 'Đường biên hiệu quả' (Efficient Frontier) trong mô hình Markowitz mô tả tập hợp các danh mục đầu tư nào?",
            "options": {
                "A": "Các danh mục chỉ bao gồm duy nhất 1 loại tài sản không có bất kỳ rủi ro nào",
                "B": "Các danh mục luôn dự báo chính xác diễn biến thị trường trong chu kỳ 10 năm tới",
                "C": "Tập hợp các danh mục tối ưu mang lại lợi nhuận kỳ vọng cao nhất ứng với mỗi mức rủi ro xác định (hoặc rủi ro thấp nhất ứng với mức lợi nhuận mục tiêu)",
                "D": "Danh mục của các công ty có chỉ số P/E thấp nhất trên toàn thị trường chứng khoán"
            },
            "ans": "C",
            "explanation": "Efficient Frontier là đường biên tập hợp tất cả các danh mục tối ưu hóa MPT: không thể tìm thấy danh mục nào có lợi nhuận cao hơn ở cùng mức rủi ro, hoặc có rủi ro thấp hơn ở cùng mức lợi nhuận (C đúng). A, B, D là những mô tả sai lệch về lý thuyết danh mục."
        },
        {
            "qid": "Q08-05",
            "lo": "LO-W08-03",
            "question": "Hệ số Sharpe (Sharpe Ratio) được sử dụng để đánh giá hiệu quả đầu tư bằng cách:",
            "options": {
                "A": "So sánh trực tiếp giá trị vốn hóa của danh mục với tổng nợ vay",
                "B": "Chia tổng lợi nhuận kiếm được cho số lượng giao dịch đã thực hiện trong tháng",
                "C": "Đo lường phần lợi nhuận thặng dư vượt trên lãi suất phi rủi ro tạo ra trên mỗi đơn vị rủi ro tổng thể (độ lệch chuẩn)",
                "D": "Đo lường tỷ lệ cổ phiếu vốn hóa lớn so với cổ phiếu penny trong danh mục"
            },
            "ans": "C",
            "explanation": "Sharpe Ratio = (Lợi nhuận danh mục - Lãi suất phi rủi ro) / Độ lệch chuẩn. Chỉ số này cho biết nhà đầu tư nhận được bao nhiêu đơn vị lợi nhuận thặng dư cho mỗi đơn vị rủi ro biến động gánh chịu; Sharpe càng cao thì chất lượng danh mục càng tốt (C đúng). A, B, D không phải công thức hay định nghĩa của Sharpe Ratio."
        },
        {
            "qid": "Q08-06",
            "lo": "LO-W08-03",
            "question": "Trong thị trường tài chính, rủi ro phi hệ thống (Unsystematic Risk / Idiosyncratic Risk) là loại rủi ro:",
            "options": {
                "A": "Gắn liền với đặc thù của từng doanh nghiệp hoặc ngành riêng lẻ, có thể triệt tiêu phần lớn bằng đa dạng hóa",
                "B": "Ảnh hưởng đến toàn bộ nền kinh tế (như khủng hoảng tài chính, suy thoái toàn cầu) và không thể đa dạng hóa được",
                "C": "Bắt nguồn từ chính sách tiền tệ nâng lãi suất của Ngân hàng Trung ương",
                "D": "Hoàn toàn không bao giờ xuất hiện nếu doanh nghiệp có kiểm toán độc lập"
            },
            "ans": "A",
            "explanation": "Rủi ro phi hệ thống (như rủi ro ban lãnh đạo từ chức, cháy nhà máy, mất đơn hàng lớn) là rủi ro riêng lẻ của từng công ty; khi nắm giữ danh mục 20-30 cổ phiếu đa ngành, các cú sốc này sẽ triệt tiêu lẫn nhau (A đúng). B và C là rủi ro hệ thống (Systematic Risk); D sai vì kiểm toán không loại bỏ rủi ro kinh doanh."
        },
        {
            "qid": "Q08-07",
            "lo": "LO-W08-04",
            "question": "Tại sao việc nắm giữ 100 mã cổ phiếu thuộc cùng một ngành (ví dụ cùng ngành Bất động sản dân cư) KHÔNG mang lại hiệu quả đa dạng hóa danh mục thực chất?",
            "options": {
                "A": "Vì phí lưu ký chứng khoán sẽ tự động tăng gấp 100 lần",
                "B": "Vì các cổ phiếu trong cùng một ngành có mức độ tương quan rất cao, cùng suy giảm mạnh khi chu kỳ ngành hoặc chính sách pháp lý đảo chiều",
                "C": "Vì luật chứng khoán cấm một nhà đầu tư nắm giữ quá 10 mã cổ phiếu cùng ngành",
                "D": "Vì các công ty trong cùng ngành luôn sáp nhập lại thành một tổng công ty duy nhất"
            },
            "ans": "B",
            "explanation": "Đa dạng hóa thực chất đòi hỏi tương quan thấp giữa các tài sản. Nắm giữ nhiều mã trong cùng một ngành chỉ là 'đa dạng hóa giả tạo' (diworsification); khi ngành gặp khủng hoảng chính sách hay đóng băng thanh khoản, toàn bộ các mã này sẽ lao dốc cùng nhau (B đúng). A, C, D là các thông tin bịa đặt sai thực tế."
        },
        {
            "qid": "Q08-08",
            "lo": "LO-W08-04",
            "question": "Kỷ luật 'Tái cân bằng danh mục định kỳ' (Portfolio Rebalancing) giúp nhà đầu tư duy trì mục tiêu dài hạn bằng cách nào?",
            "options": {
                "A": "Rút toàn bộ tiền gửi ngân hàng để dồn hết vào tài sản vừa tăng giá mạnh nhất",
                "B": "Đóng băng tài khoản và không thực hiện bất kỳ giao dịch nào trong suốt 20 năm",
                "C": "Liên tục thay đổi tỷ trọng mục tiêu theo các bài viết khuyến nghị trên mạng xã hội hàng tuần",
                "D": "Bán bớt lớp tài sản tăng nóng vượt tỷ trọng quy định và mua thêm lớp tài sản sụt giảm dưới tỷ trọng, đưa danh mục về lại cơ cấu phân bổ ban đầu"
            },
            "ans": "D",
            "explanation": "Rebalancing là cơ chế chốt lời và gom hàng có kỷ luật: khi cổ phiếu tăng mạnh chiếm 80% danh mục (vượt mục tiêu 60%), việc bán 20% cổ phiếu để mua thêm trái phiếu giúp hiện thực hóa lợi nhuận và hạn chế rủi ro đu đỉnh (D đúng). A là hành vi FOMO đu đỉnh; B bỏ qua việc quản trị rủi ro; C là hành động thiếu kỷ luật chiến lược."
        }
    ]

    build_week_assessment(8, title, module_num, lesson_slug, questions)


def build_w09():
    title = "Đầu tư giá trị: Graham và Buffett principles"
    module_num = 2
    lesson_slug = "week-09-dau-tu-gia-tri"

    questions = [
        {
            "qid": "Q09-01",
            "lo": "LO-W09-01",
            "question": "Nguyên lý 'Biên an toàn' (Margin of Safety) của Benjamin Graham được định nghĩa là:",
            "options": {
                "A": "Một cam kết pháp lý đảm bảo nhà đầu tư sẽ không bao giờ bị thua lỗ vốn",
                "B": "Khoảng cách chênh lệch chiết khấu an toàn giữa giá trị nội tại ước tính của doanh nghiệp và thị giá mua trên sàn",
                "C": "Việc sử dụng tỷ lệ đòn bẩy vay margin tối đa khi cổ phiếu giảm giá sàn",
                "D": "Mua tất cả các cổ phiếu có thị giá nhỏ hơn 10.000 đồng bất kể tình hình tài chính"
            },
            "ans": "B",
            "explanation": "Margin of Safety là hòn đá tảng của đầu tư giá trị: mua một tài sản với giá thấp hơn đáng kể so với giá trị nội tại ước tính (ví dụ mua tài sản đáng giá 100 đồng với giá 70 đồng) để phòng ngừa các sai sót trong định giá và biến cố khôn lường của thị trường (B đúng). A sai vì thị trường luôn có rủi ro, không có gì đảm bảo 100%; C và D là các hành vi đầu cơ liều lĩnh."
        },
        {
            "qid": "Q09-02",
            "lo": "LO-W09-01",
            "question": "Khái niệm 'Bẫy giá trị' (Value Trap) mô tả tình huống nào trong đầu tư thực tế?",
            "options": {
                "A": "Cổ phiếu của một công ty công nghệ tăng trưởng liên tục tạo đỉnh mới",
                "B": "Một doanh nghiệp có lợi thế con hào kinh tế bền vững và dòng tiền dồi dào",
                "C": "Quỹ ETF bám sát chỉ số VN30 đạt tỷ suất sinh lời vượt trội",
                "D": "Cổ phiếu có định giá bề ngoài rất rẻ (P/E, P/B thấp) nhưng năng lực kinh doanh suy thoái dài hạn khiến giá tiếp tục lao dốc"
            },
            "ans": "D",
            "explanation": "Value Trap là cái bẫy nhà đầu tư giá trị non trẻ thường mắc phải: nhìn vào P/E = 3 hay P/B = 0.5 tưởng là món hời, nhưng thực chất doanh nghiệp mất thị phần, công nghệ lỗi thời, ban lãnh đạo yếu kém, và giá trị nội tại liên tục xói mòn (D đúng). A, B, C là các trường hợp tích cực, không phải định nghĩa của bẫy giá trị."
        },
        {
            "qid": "Q09-03",
            "lo": "LO-W09-02",
            "question": "Khi phân tích một cổ phiếu có chỉ số P/B rất thấp (ví dụ P/B = 0,4), nhà đầu tư giá trị thận trọng bắt buộc phải kiểm tra yếu tố nào?",
            "options": {
                "A": "Chất lượng thực tế của tài sản, nguy cơ trích lập giảm giá hàng tồn kho, nợ xấu và chỉ số sinh lời ROE",
                "B": "Chỉ cần nhìn thị giá cổ phiếu hôm qua đã giảm bao nhiêu phần trăm",
                "C": "Số lượng người tham gia bàn luận về mã cổ phiếu đó trên các nhóm mạng xã hội",
                "D": "Tên gọi và logo của doanh nghiệp có bắt mắt và quen thuộc hay không"
            },
            "ans": "A",
            "explanation": "P/B thấp có thể là ảo tưởng nếu tài sản trên sổ sách là hàng tồn kho ứ đọng lỗi thời, nhà máy cũ kỹ không thanh lý được, hoặc các khoản phải thu khó đòi; cần kiểm tra chất lượng tài sản và mức sinh lời ROE để đánh giá tài sản có thực sự sinh ra tiền hay không (A đúng). B, C, D là các tín hiệu bề nổi, không có giá trị thẩm định tài sản."
        },
        {
            "qid": "Q09-04",
            "lo": "LO-W09-02",
            "question": "Theo Warren Buffett, một 'Con hào kinh tế' (Economic Moat) bền vững của doanh nghiệp có thể được hình thành từ:",
            "options": {
                "A": "Khoản lợi nhuận tài chính bất thường đột biến từ việc bán trụ sở công ty",
                "B": "Một bài viết quảng cáo khen ngợi trên báo chí trong kỳ nghỉ lễ",
                "C": "Lợi thế vô hình (thương hiệu, bằng sáng chế), chi phí chuyển đổi của khách hàng, hiệu ứng mạng lưới hoặc lợi thế chi phí thấp",
                "D": "Một khoản vay ưu đãi lãi suất thấp từ ngân hàng trong thời hạn 6 tháng"
            },
            "ans": "C",
            "explanation": "Moat là rào cản giúp doanh nghiệp bảo vệ thị phần và duy trì biên lợi nhuận cao trước sự tấn công của các đối thủ cạnh tranh: gồm Intangible Assets (thương hiệu, bản quyền), Switching Costs, Network Effects, và Cost Advantages (C đúng). A là thu nhập một lần không bền vững; B và D không tạo ra năng lực cạnh tranh cốt lõi dài hạn."
        },
        {
            "qid": "Q09-05",
            "lo": "LO-W09-03",
            "question": "Một doanh nghiệp có ROE tăng vọt lên mức 40%, nhưng khi soi xét kỹ bảng cân đối kế toán thì vốn chủ sở hữu rất mỏng trong khi tổng nợ vay lại phình to đột biến. Hiện tượng này cảnh báo điều gì?",
            "options": {
                "A": "Doanh nghiệp chắc chắn là công ty an toàn và bền vững nhất toàn thị trường",
                "B": "Bằng chứng rõ ràng cho thấy doanh nghiệp đã xóa sạch toàn bộ nợ vay",
                "C": "Hiệu quả kinh doanh từ hoạt động cốt lõi chắc chắn đã tăng trưởng gấp 10 lần",
                "D": "ROE cao do bị thổi phồng bởi tỷ lệ đòn bẩy tài chính quá lớn (High Leverage), tiềm ẩn nguy cơ mất khả năng thanh toán nghiêm trọng"
            },
            "ans": "D",
            "explanation": "Theo phân tích DuPont: ROE = Biên lợi nhuận ròng × Vòng quay tài sản × Đòn bẩy tài chính. Khi đòn bẩy quá cao, ROE có thể cao chót vót nhưng chỉ cần một cú sốc nhỏ về doanh thu hoặc lãi suất vay tăng là doanh nghiệp có thể rơi vào bờ vực phá sản (D đúng). A, B, C bỏ qua rủi ro cấu trúc vốn cực đoan."
        },
        {
            "qid": "Q09-06",
            "lo": "LO-W09-03",
            "question": "Khi xây dựng mô hình chiết khấu dòng tiền (DCF) để ước tính Giá trị nội tại (Intrinsic Value), yêu cầu phương pháp luận quan trọng nhất là gì?",
            "options": {
                "A": "Chọn một con số định giá kỳ vọng duy nhất và từ chối giải thích các tham số đầu vào",
                "B": "Minh bạch mọi giả định về tăng trưởng doanh thu, biên lợi nhuận, chiết khấu dòng tiền và kiểm tra độ nhạy đa kịch bản",
                "C": "Luôn lấy thị giá đóng cửa trên sàn làm giá trị nội tại chuẩn mực",
                "D": "Bỏ qua hoàn toàn dòng tiền tự do (FCFF) và chỉ dựa vào cảm tính cá nhân"
            },
            "ans": "B",
            "explanation": "Định giá DCF chỉ đáng tin cậy khi các giả định đầu vào (tốc độ tăng trưởng, tỷ suất chiết khấu WACC, dòng tiền tự do) được giải trình rõ ràng và được kiểm tra độ nhạy (Sensitivity Analysis) qua các kịch bản thận trọng, cơ sở và lạc quan (B đúng). A và D vi phạm nguyên tắc thẩm định; C nhầm lẫn giữa giá cả (Price) và giá trị (Value)."
        },
        {
            "qid": "Q09-07",
            "lo": "LO-W09-04",
            "question": "Một nhà đầu tư giá trị theo trường phái Graham & Buffett nên xem xét bán cổ phiếu khi nào?",
            "options": {
                "A": "Ngay khi thị trường chung xuất hiện một phiên giảm điểm 2%",
                "B": "Khi có một tài khoản ẩn danh trên diễn đàn chê bai mã cổ phiếu đó",
                "C": "Khi thị giá đã phản ánh vượt quá xa giá trị nội tại hợp lý, hoặc các yếu tố cơ bản và con hào kinh tế của doanh nghiệp bị suy thoái vĩnh viễn",
                "D": "Bán ra ngay sau đúng 30 ngày kể từ ngày mua bất kể giá cổ phiếu đang ở đâu"
            },
            "ans": "C",
            "explanation": "Nhà đầu tư giá trị chỉ bán khi: (1) Thị giá tăng nóng vượt xa giá trị nội tại khiến biên an toàn biến mất; (2) Luận điểm đầu tư ban đầu bị sai hoặc con hào kinh tế bị phá vỡ; hoặc (3) Tìm thấy một cơ hội khác có biên an toàn vượt trội hơn hẳn (C đúng). A, B, D là các phản ứng ngắn hạn cảm tính, đi ngược lại triết lý nắm giữ theo giá trị."
        },
        {
            "qid": "Q09-08",
            "lo": "LO-W09-04",
            "question": "Phát biểu nào sau đây thể hiện đúng nhất sự khác biệt giữa 'Giá cả' (Price) và 'Giá trị' (Value) theo châm ngôn nổi tiếng của Warren Buffett?",
            "options": {
                "A": "Giá cả là số tiền bạn phải bỏ ra, còn giá trị là những gì bạn thực sự nhận được về mặt kinh tế",
                "B": "Giá cả và giá trị luôn luôn bằng nhau tuyệt đối trong mọi thời điểm giao dịch trên sàn",
                "C": "Giá trị chỉ là con số do sàn chứng khoán tự động cập nhật theo từng giây",
                "D": "Giá cả phản ánh chính xác 100% dòng tiền của doanh nghiệp trong 50 năm tới"
            },
            "ans": "A",
            "explanation": "Buffett khẳng định: 'Price is what you pay; value is what you get'. Thị trường trong ngắn hạn là một cỗ máy bỏ phiếu (Voting Machine) bị chi phối bởi cảm xúc tham lam/sợ hãi khiến giá cả dao động quanh giá trị; nhưng trong dài hạn là cỗ máy bàn cân (Weighing Machine) phản ánh giá trị thực chất (A đúng). B, C, D là các hiểu lầm căn bản về mối quan hệ giữa thị giá và giá trị nội tại."
        }
    ]

    build_week_assessment(9, title, module_num, lesson_slug, questions)


def build_w10():
    title = "Đầu tư tăng trưởng, Index/ETF và DCA"
    module_num = 2
    lesson_slug = "week-10-growth-index-etf-dca"

    questions = [
        {
            "qid": "Q10-01",
            "lo": "LO-W10-01",
            "question": "Chiến lược 'Đầu tư tăng trưởng' (Growth Investing) tập trung ưu tiên hàng đầu vào tiêu chí nào?",
            "options": {
                "A": "Chỉ mua các cổ phiếu có chỉ số P/E cực thấp dưới 5 lần",
                "B": "Chỉ đầu tư vào các doanh nghiệp sở hữu nhiều tài sản cố định như đất đai, nhà xưởng",
                "C": "Tiềm năng mở rộng quy mô doanh thu, thị phần và tốc độ tăng trưởng lợi nhuận vượt trội trong tương lai",
                "D": "Lãi suất cố định hàng năm được bảo đảm bởi trái phiếu chính phủ"
            },
            "ans": "C",
            "explanation": "Growth Investing tìm kiếm các doanh nghiệp có tốc độ mở rộng thị trường, doanh thu và lợi nhuận nhanh hơn tốc độ trung bình của nền kinh tế, sẵn sàng chấp nhận định giá P/E cao để đổi lấy tiềm năng bùng nổ trong tương lai (C đúng). A là đặc trưng của Deep Value; B là trường phái tài sản ngầm; D là đầu tư công cụ nợ có thu nhập cố định."
        },
        {
            "qid": "Q10-02",
            "lo": "LO-W10-01",
            "question": "Quỹ hoán đổi danh mục (ETF - Exchange Traded Fund) có đặc điểm vận hành cốt lõi nào?",
            "options": {
                "A": "Là quỹ đầu tư mô phỏng chỉ số tham chiếu, được niêm yết và giao dịch trực tiếp trên sàn chứng khoán theo giá thị trường liên tục",
                "B": "Là một dạng tiền gửi tiết kiệm có kỳ hạn được bảo hiểm tuyệt đối 100%",
                "C": "Là một cổ phiếu đơn lẻ của một doanh nghiệp độc quyền nhà nước",
                "D": "Là hợp đồng vay nợ ký quỹ (margin) giữa nhà đầu tư và công ty chứng khoán"
            },
            "ans": "A",
            "explanation": "ETF kết hợp tính đa dạng hóa của quỹ đầu tư và tính thanh khoản linh hoạt của cổ phiếu: mô phỏng theo một rổ chỉ số (như VN30, VN Diamond, S&P 500) và giao dịch theo thời gian thực trên sàn (A đúng). B, C, D hoàn toàn sai bản chất pháp lý và cấu trúc của quỹ hoán đổi danh mục."
        },
        {
            "qid": "Q10-03",
            "lo": "LO-W10-02",
            "question": "Chiến lược 'Bình quân giá chi phí' (DCA - Dollar-Cost Averaging) vận hành theo nguyên tắc nào?",
            "options": {
                "A": "Dồn toàn bộ vốn gom mua duy nhất một lần tại đúng mức giá đáy tuyệt đối",
                "B": "Chỉ thực hiện lệnh mua thêm khi giá cổ phiếu liên tục tăng trần nhiều phiên",
                "C": "Sử dụng đòn bẩy vay ký quỹ tăng dần đều mỗi khi giá cổ phiếu lao dốc",
                "D": "Định kỳ giải ngân một khoản tiền cố định vào một tài sản theo lịch trình định trước, bất chấp thị trường đang lên hay xuống"
            },
            "ans": "D",
            "explanation": "Kỷ luật DCA giúp nhà đầu tư loại bỏ tâm lý đoán đỉnh bắt đáy: khi giá giảm, số tiền cố định sẽ mua được nhiều chứng chỉ quỹ/cổ phiếu hơn; khi giá tăng, sẽ mua ít hơn, giúp hạ giá vốn bình quân dài hạn một cách tự động (D đúng). A là bắt đáy bất khả thi; B là đu đỉnh theo cảm xúc; C là trung bình giá xuống bằng margin rất nguy hiểm."
        },
        {
            "qid": "Q10-04",
            "lo": "LO-W10-02",
            "question": "Thước đo 'Chênh lệch theo dõi' (Tracking Difference) của một quỹ ETF phản ánh điều gì?",
            "options": {
                "A": "Chỉ số P/E trung bình của từng cổ phiếu thành phần trong rổ chỉ số",
                "B": "Mức chênh lệch về hiệu suất sinh lời giữa quỹ ETF và chỉ số tham chiếu mà quỹ mô phỏng trong một chu kỳ xác định",
                "C": "Mặt bằng lãi suất tiền gửi không kỳ hạn của hệ thống ngân hàng thương mại",
                "D": "Tỷ lệ thất nghiệp và lạm phát của nền kinh tế trong tháng gần nhất"
            },
            "ans": "B",
            "explanation": "Tracking Difference = Hiệu suất ETF - Hiệu suất chỉ số tham chiếu. Chênh lệch này thường âm do quỹ phát sinh phí quản lý, chi phí tái cân bằng rổ cổ phiếu, và dòng tiền mặt dự trữ (B đúng). A, C, D là các chỉ số kinh tế và định giá không liên quan đến sai số mô phỏng của quỹ."
        },
        {
            "qid": "Q10-05",
            "lo": "LO-W10-03",
            "question": "Lợi thế cạnh tranh vượt trội của các quỹ đầu tư thụ động (Index Fund / ETF) so với phần lớn các quỹ đầu tư chủ động (Active Fund) trong dài hạn là:",
            "options": {
                "A": "Cam kết danh mục không bao giờ chịu bất kỳ biến động hay rủi ro giảm giá nào",
                "B": "Mức độ đa dạng hóa rộng khắp toàn thị trường kết hợp với chi phí quản lý vận hành thấp hơn đáng kể",
                "C": "Đảm bảo tỷ suất sinh lời chắc chắn luôn cao gấp 3 lần chỉ số thị trường chung",
                "D": "Nhà đầu tư không phải chịu bất kỳ loại phí giao dịch hay thuế nào"
            },
            "ans": "B",
            "explanation": "Nghiên cứu của SPIVA chỉ ra rằng trong dài hạn (>10 năm), hơn 80-90% các quỹ chủ động không thắng được chỉ số tham chiếu do chi phí quản lý cao (1,5-2%/năm) và sai lầm dự báo; ETF thụ động có phí rất thấp (0,2-0,8%) và phân bổ rộng rãi (B đúng). A và C là kỳ vọng sai lầm; D sai vì giao dịch ETF vẫn có phí môi giới và thuế chuẩn tắc."
        },
        {
            "qid": "Q10-06",
            "lo": "LO-W10-03",
            "question": "Trong quá trình thực hiện chiến lược DCA định kỳ với một quỹ ETF chỉ số, nếu thị trường trải qua giai đoạn giảm giá kéo dài, hành động đúng đắn theo nguyên tắc DCA là:",
            "options": {
                "A": "Bán tháo toàn bộ danh mục ngay lập tức để chuyển sang giữ 100% tiền mặt",
                "B": "Vay nóng bên ngoài để tăng gấp 10 lần số tiền mua ngay trong phiên tiếp theo",
                "C": "Hủy bỏ hoàn toàn kế hoạch đầu tư dài hạn vì cho rằng thị trường sẽ biến mất",
                "D": "Giữ vững kỷ luật giải ngân số tiền cố định theo đúng lịch trình, tận dụng vùng giá thấp để tích lũy được lượng chứng chỉ quỹ nhiều hơn"
            },
            "ans": "D",
            "explanation": "Bản chất cốt lõi của DCA phát huy tác dụng mạnh nhất trong thị trường con gấu (Bear market): kỷ luật mua đều đặn giúp nhà đầu tư gom được nhiều đơn vị tài sản giá rẻ mà không bị nỗi sợ hãi chi phối, tạo bàn đạp sinh lời bùng nổ khi chu kỳ kinh tế phục hồi (D đúng). A và C là phản ứng sợ hãi phá vỡ kế hoạch; B là hành vi liều lĩnh vi phạm quản trị vốn."
        },
        {
            "qid": "Q10-07",
            "lo": "LO-W10-04",
            "question": "Khi thẩm định và lựa chọn một quỹ ETF để đầu tư dài hạn, bộ tiêu chí nào sau đây là quan trọng nhất?",
            "options": {
                "A": "Chỉ số tham chiếu uy tín, tỷ lệ chi phí quản lý thấp, sai số mô phỏng nhỏ, quy mô tài sản lớn (AUM) và thanh khoản giao dịch dồi dào",
                "B": "Chỉ cần chọn quỹ có tên gọi nghe hấp dẫn và ấn tượng nhất",
                "C": "Chỉ nhìn vào lợi nhuận đột biến của quỹ trong duy nhất 1 tháng gần nhất",
                "D": "Mua theo lời kêu gọi của các hội nhóm quảng cáo trên mạng xã hội"
            },
            "ans": "A",
            "explanation": "Tiêu chí chọn ETF chuẩn mực quốc tế: (1) Chỉ số theo dõi minh bạch; (2) Phí quản lý (TER) càng thấp càng tốt; (3) Tracking Error nhỏ (bám sát chỉ số); (4) Quy mô tài sản (AUM) đủ lớn để tránh nguy cơ đóng quỹ; (5) Thanh khoản cao để dễ mua bán không bị trượt giá (A đúng). B, C, D là các phương pháp lựa chọn thiếu hiểu biết và rủi ro."
        },
        {
            "qid": "Q10-08",
            "lo": "LO-W10-04",
            "question": "Phát biểu nào sau đây thể hiện ĐÚNG NHẤT về giới hạn của chiến lược đầu tư định kỳ DCA?",
            "options": {
                "A": "DCA đảm bảo nhà đầu tư luôn mua được cổ phiếu ở đúng mức giá đáy thấp nhất của mọi chu kỳ",
                "B": "DCA có thể loại bỏ hoàn toàn 100% mọi rủi ro thị trường và nguy cơ thua lỗ vốn",
                "C": "DCA giúp quản trị tâm lý và phân tán rủi ro thời điểm vào lệnh, nhưng không bảo đảm luôn sinh lời cao hơn đầu tư một lần (Lump-sum) nếu thị trường tăng trưởng một mạch",
                "D": "DCA chỉ phù hợp cho những người có vốn cực lớn hàng triệu USD"
            },
            "ans": "C",
            "explanation": "Nghiên cứu tài chính chỉ ra trong thị trường tăng giá liên tục (Bull market dài hạn), đầu tư một lần toàn bộ tiền ngay từ đầu (Lump-sum) thường cho lợi nhuận cao hơn DCA vì tiền được sinh lời sớm hơn; DCA là công cụ xuất sắc về kỷ luật tâm lý và quản trị rủi ro thời điểm vào lệnh chứ không phải cỗ máy bảo đảm tỷ suất sinh lời cao nhất (C đúng). A, B tuyệt đối hóa sai lầm; D sai vì DCA đặc biệt phù hợp cho người có thu nhập tích lũy định kỳ hàng tháng."
        }
    ]

    build_week_assessment(10, title, module_num, lesson_slug, questions)


if __name__ == "__main__":
    build_w05()
    build_w06()
    build_w07()
    build_w08()
    build_w09()
    build_w10()
    print("All Module 2 assessments built successfully!")
