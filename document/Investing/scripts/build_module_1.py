"""
Module 1 Assessments Generator (W01 - W04)
Standardizes:
- Balanced distribution: 2A, 2B, 2C, 2D
- Clean markdown formatting: A., B., C., D.
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


def build_w01():
    title = "Tiền tệ, Lạm phát, Lãi suất và Giá trị thời gian của tiền (TVM)"
    module_num = 1
    lesson_slug = "week-01-tien-te-lam-phat-lai-suat-tvm"

    questions = [
        {
            "qid": "Q01-01",
            "lo": "LO-W01-01",
            "question": "Công thức nào sau đây đúng để tính giá trị tương lai (FV) của một khoản tiền hiện tại (PV) với lãi suất kép hàng năm r trong n năm?",
            "options": {
                "A": "FV = PV × (1 + r)^n",
                "B": "FV = PV × (1 + r × n)",
                "C": "FV = PV / (1 + r)^n",
                "D": "FV = PV × r × n"
            },
            "ans": "A",
            "explanation": "Công thức chuẩn xác tính giá trị tương lai theo cơ chế lãi kép là FV = PV × (1 + r)^n (A đúng). B nhầm sang công thức lãi đơn không tái đầu tư lãi; C là công thức chiết khấu tính giá trị hiện tại (PV); D là công thức tính số tiền lãi đơn của một kỳ."
        },
        {
            "qid": "Q01-02",
            "lo": "LO-W01-01",
            "question": "Một người gửi tiết kiệm 10 triệu đồng với lãi suất kép 6%/năm trong thời hạn 3 năm. Số tiền cả gốc lẫn lãi nhận được sau 3 năm xấp xỉ là:",
            "options": {
                "A": "10,60 triệu đồng",
                "B": "11,80 triệu đồng",
                "C": "11,91 triệu đồng",
                "D": "12,50 triệu đồng"
            },
            "ans": "C",
            "explanation": "Áp dụng công thức lãi kép: FV = 10 × (1 + 0,06)^3 = 10 × 1,191016 ≈ 11,91 triệu đồng (C đúng). A chỉ là số tiền sau 1 năm; B là kết quả tính theo lãi đơn 10 × (1 + 0,06 × 3) = 11,8 triệu đồng; D là con số ước tính khống không có căn cứ."
        },
        {
            "qid": "Q01-03",
            "lo": "LO-W01-02",
            "question": "Nếu lãi suất danh nghĩa gửi ngân hàng là 6%/năm và tỷ lệ lạm phát kỳ vọng là 3%/năm, thì lãi suất thực xấp xỉ theo phương trình Fisher là:",
            "options": {
                "A": "2,0% (lấy lãi suất danh nghĩa chia lạm phát)",
                "B": "9,0% (lấy lãi suất danh nghĩa cộng lạm phát)",
                "C": "18,0% (lấy lãi suất danh nghĩa nhân lạm phát)",
                "D": "3,0% (lấy lãi suất danh nghĩa trừ lạm phát)"
            },
            "ans": "D",
            "explanation": "Theo phương trình Fisher xấp xỉ: Lãi suất thực ≈ Lãi suất danh nghĩa - Lạm phát = 6% - 3% = 3,0% (D đúng). A nhầm sang phép chia đại lượng; B cộng lạm phát vào lãi suất danh nghĩa là hoàn toàn sai bản chất; C nhân hai đại lượng là phi lý."
        },
        {
            "qid": "Q01-04",
            "lo": "LO-W01-02",
            "question": "Vì sao lạm phát làm xói mòn giá trị thời gian của tiền tệ theo thời gian?",
            "options": {
                "A": "Vì lạm phát làm tăng sức mua thực tế của một đồng tiền trong tương lai",
                "B": "Vì cùng một lượng tiền danh nghĩa sẽ mua được ít hàng hóa và dịch vụ hơn trong tương lai khi mặt bằng giá cả leo thang",
                "C": "Vì lạm phát luôn ép lãi suất tiền gửi của tất cả các ngân hàng thương mại về mức 0%",
                "D": "Vì lạm phát chỉ tác động đến các tập đoàn đa quốc gia chứ không ảnh hưởng đến người tiêu dùng cá nhân"
            },
            "ans": "B",
            "explanation": "Lạm phát phản ánh sự gia tăng mức giá chung kéo dài, làm giảm sức mua: cùng một lượng tiền mặt danh nghĩa sẽ chỉ mua được lượng hàng hóa ít hơn trong tương lai (B đúng). A đảo ngược bản chất của lạm phát; C sai vì ngân hàng trung ương thường tăng lãi suất để đối phó lạm phát; D sai vì lạm phát bào mòn trực tiếp chi phí sinh hoạt của mọi cá nhân."
        },
        {
            "qid": "Q01-05",
            "lo": "LO-W01-03",
            "question": "Bạn cần một khoản tiền 12,1 triệu đồng sau 2 năm nữa để đóng học phí. Với lãi suất chiết khấu 10%/năm ghép lãi hàng năm, giá trị hiện tại (PV) cần chuẩn bị hôm nay là:",
            "options": {
                "A": "8,50 triệu đồng",
                "B": "10,00 triệu đồng",
                "C": "11,00 triệu đồng",
                "D": "14,64 triệu đồng"
            },
            "ans": "B",
            "explanation": "PV = FV / (1 + r)^n = 12,1 / (1 + 0,10)^2 = 12,1 / 1,21 = 10,00 triệu đồng (B đúng). A chiết khấu quá mức; C chỉ chiết khấu cho 1 kỳ (12,1 / 1,1); D là lấy 12,1 nhân tiếp với (1,1)^2 thành tính FV."
        },
        {
            "qid": "Q01-06",
            "lo": "LO-W01-03",
            "question": "Với cùng một mức lãi suất danh nghĩa hàng năm r, việc tăng tần suất ghép lãi (ví dụ ghép lãi hàng tháng hoặc hàng quý thay vì hàng năm) sẽ mang lại kết quả nào?",
            "options": {
                "A": "Làm giá trị tương lai nhỏ hơn do tiền lãi bị xé nhỏ thành nhiều phần",
                "B": "Giá trị tương lai hoàn toàn không thay đổi vì tổng lãi suất danh nghĩa của cả năm là cố định",
                "C": "Giá trị tương lai lớn hơn vì tiền lãi phát sinh sớm được nhập gốc để tiếp tục sinh lãi (lãi suất hiệu dụng EAR cao hơn)",
                "D": "Tiền vốn gốc ban đầu sẽ bị ngân hàng khấu trừ chi phí quản lý ghép lãi"
            },
            "ans": "C",
            "explanation": "Ghép lãi nội kỳ thường xuyên hơn giúp lãi phát sinh sớm trở thành vốn gốc để tiếp tục sinh lãi trong các kỳ tiếp theo, làm tăng lãi suất thực hưởng (EAR) và tạo ra FV lớn hơn (C đúng). A nhầm giữa việc chia nhỏ kỳ hạn với mất mát giá trị; B bỏ qua hiệu ứng lãi kép nội kỳ; D là giả định sai lệch về phí ngân hàng."
        },
        {
            "qid": "Q01-07",
            "lo": "LO-W01-03",
            "question": "Khoản đầu tư X có lãi suất danh nghĩa 7%/năm, lạm phát 5%/năm. Khoản đầu tư Y có lãi suất danh nghĩa 5%/năm, lạm phát 1%/năm. Nhận định nào sau đây đúng về sức mua thực tế?",
            "options": {
                "A": "Khoản X tốt hơn vì có lãi suất danh nghĩa 7% cao hơn mức 5% của khoản Y",
                "B": "Hai khoản có mức tăng sức mua thực tế bằng nhau vì cùng đem lại mức sinh lời danh nghĩa dương",
                "C": "Cả hai khoản đều làm suy giảm sức mua vì lạm phát luôn xóa sạch mọi khoản lãi suất ngân hàng",
                "D": "Khoản Y gia tăng sức mua thực tế tốt hơn khoản X (lãi thực xấp xỉ 4% so với 2%)"
            },
            "ans": "D",
            "explanation": "Lãi suất thực của X xấp xỉ 7% - 5% = 2%, trong khi của Y xấp xỉ 5% - 1% = 4%; do đó khoản Y gia tăng sức mua thực tế vượt trội (D đúng). A mắc ảo tưởng tiền tệ (Money Illusion) chỉ nhìn vào con số danh nghĩa; B sai vì 4% > 2%; C sai vì lãi suất danh nghĩa của cả hai đều cao hơn tỷ lệ lạm phát tương ứng."
        },
        {
            "qid": "Q01-08",
            "lo": "LO-W01-04",
            "question": "Sai lầm phương pháp luận phổ biến nhất khi người mới sử dụng bảng tính lãi kép để lập kế hoạch tài chính cá nhân dài hạn là gì?",
            "options": {
                "A": "Giả định mức lãi suất sinh lời cao cố định liên tục trong nhiều thập kỷ mà bỏ qua rủi ro chu kỳ, thuế, phí và lạm phát",
                "B": "Tính toán giá trị tương lai theo từng năm thay vì chỉ tính gộp một lần ở cuối kỳ",
                "C": "Tái đầu tư toàn bộ các khoản cổ tức nhận được vào danh mục tài sản",
                "D": "Chủ động tăng số tiền tiết kiệm định kỳ hàng năm theo mức tăng trưởng thu nhập thực tế"
            },
            "ans": "A",
            "explanation": "Sai lầm kinh điển là vẽ ra bảng tính Excel với lãi suất 15-20% đều đặn trong 30-40 năm, bỏ qua rủi ro thị trường lao dốc, chi phí giao dịch, thuế và lạm phát làm hao mòn sức mua thực tế (A đúng). B, C, D là các phương pháp luận và thói quen lập kế hoạch tài chính chuẩn mực, lành mạnh."
        }
    ]

    build_week_assessment(1, title, module_num, lesson_slug, questions)


def build_w02():
    title = "Thị trường tài chính: cổ phiếu, trái phiếu, hàng hóa, forex và crypto overview"
    module_num = 1
    lesson_slug = "week-02-thi-truong-tai-chinh"

    questions = [
        {
            "qid": "Q02-01",
            "lo": "LO-W02-01",
            "question": "Người nắm giữ cổ phiếu phổ thông của một doanh nghiệp niêm yết thường có quyền lợi kinh tế cốt lõi nào?",
            "options": {
                "A": "Quyền nhận lãi suất cố định định kỳ và hoàn trả gốc bắt buộc như một chủ nợ",
                "B": "Quyền sở hữu một phần vốn cổ phần và có thể nhận cổ tức tùy thuộc vào kết quả kinh doanh và chính sách công ty",
                "C": "Quyền yêu cầu ngân hàng nhà nước hoàn trả 100% tiền vốn bất kỳ khi nào muốn thoái vốn",
                "D": "Quyền bảo đảm rằng giá thị trường của cổ phiếu trên sàn sẽ không bao giờ bị giảm"
            },
            "ans": "B",
            "explanation": "Cổ phiếu phổ thông xác lập quyền sở hữu vốn chủ sở hữu: cổ đông được chia sẻ lợi nhuận qua cổ tức (nếu doanh nghiệp có lãi và ĐHĐCĐ thông qua) và hưởng chênh lệch giá vốn (B đúng). A là đặc điểm của trái phiếu/chủ nợ; C và D sai vì đầu tư cổ phiếu chịu rủi ro thị trường và không có cam kết bảo hiểm vốn."
        },
        {
            "qid": "Q02-02",
            "lo": "LO-W02-01",
            "question": "Đặc điểm pháp lý và tài chính cốt lõi của một trái phiếu doanh nghiệp tiêu chuẩn là gì?",
            "options": {
                "A": "Khoản nợ của tổ chức phát hành với cam kết pháp lý về việc trả lãi vay định kỳ và hoàn trả nợ gốc khi đáo hạn",
                "B": "Quyền sở hữu vô hạn đối với tài sản và quyền biểu quyết chi phối mọi hoạt động điều hành doanh nghiệp",
                "C": "Công cụ đầu tư hoàn toàn miễn nhiễm với rủi ro tín dụng hoặc nguy cơ phá sản của nhà phát hành",
                "D": "Luôn có tính thanh khoản và biên độ biến động giá thị trường cao hơn nhiều so với cổ phiếu phổ thông"
            },
            "ans": "A",
            "explanation": "Trái phiếu là chứng chỉ vay nợ: nhà phát hành cam kết thanh toán lãi (coupon) định kỳ và hoàn trả gốc tại ngày đáo hạn (A đúng). B là quyền của cổ đông nắm giữ cổ phiếu; C sai vì trái phiếu luôn tiềm ẩn rủi ro vỡ nợ (credit risk); D sai vì trái phiếu thường ít biến động hơn cổ phiếu cơ sở."
        },
        {
            "qid": "Q02-03",
            "lo": "LO-W02-02",
            "question": "Rủi ro tỷ giá hối đoái trong giao dịch thị trường ngoại hối (Forex) phát sinh chủ yếu từ nguyên nhân nào?",
            "options": {
                "A": "Mọi vị thế giao dịch tiền tệ đều được chia cổ tức doanh nghiệp cố định hàng tháng",
                "B": "Tỷ giá giữa các đồng tiền pháp định luôn được cố định tuyệt đối trong suốt ngày giao dịch",
                "C": "Giá trị vị thế tài sản biến động khi tỷ giá hối đoái giữa các cặp đồng tiền thay đổi do tác động vĩ mô và cung cầu",
                "D": "Thị trường Forex cấm hoàn toàn việc sử dụng tỷ lệ đòn bẩy tài chính của nhà đầu tư"
            },
            "ans": "C",
            "explanation": "Rủi ro tỷ giá (Exchange Rate Risk) phản ánh sự biến động giá trị vị thế khi tỷ giá giữa đồng tiền yết giá và định giá thay đổi bất lợi theo cung cầu vĩ mô (C đúng). A sai vì tiền tệ không tạo ra cổ tức doanh nghiệp; B sai vì tỷ giá thả nổi biến động liên tục; D sai vì Forex là thị trường sử dụng đòn bẩy tài chính rất cao."
        },
        {
            "qid": "Q02-04",
            "lo": "LO-W02-02",
            "question": "Đầu tư vào nhóm tài sản hàng hóa (commodities) tạo ra rủi ro đặc thù khác biệt so với cổ phiếu doanh nghiệp vì nhà đầu tư phải tính đến:",
            "options": {
                "A": "Quyền biểu quyết bầu hội đồng quản trị của mỏ dầu hoặc nông trường sản xuất",
                "B": "Tăng trưởng chỉ số thu nhập trên mỗi cổ phần (EPS) của các doanh nghiệp trong ngành",
                "C": "Chỉ số P/E trung bình của chính kim loại hoặc nông sản đang nắm giữ",
                "D": "Biến động giá hàng hóa giao ngay, chi phí lưu kho, bảo quản vật chất và cấu trúc kỳ hạn hợp đồng phái sinh (roll cost)"
            },
            "ans": "D",
            "explanation": "Đầu tư hàng hóa thực chịu chi phí lưu kho, bảo quản, hao hụt vật chất; khi đầu tư qua hợp đồng tương lai còn chịu chi phí đảo kỳ hạn (roll yield/contango/backwardation) (D đúng). A, B, C sai vì hàng hóa là tài sản vật chất không có quyền biểu quyết cổ đông, không có chỉ số EPS hay P/E."
        },
        {
            "qid": "Q02-05",
            "lo": "LO-W02-03",
            "question": "Phát biểu nào sau đây đúng nhất về bản chất pháp lý của token tiền mã hóa (crypto) so với cổ phiếu truyền thống?",
            "options": {
                "A": "Token luôn được pháp luật chứng khoán công nhận và bảo vệ đầy đủ quyền lợi cổ đông như cổ phiếu niêm yết",
                "B": "Token không mặc nhiên đại diện cho quyền sở hữu vốn chủ sở hữu hay quyền đòi nợ hợp pháp của doanh nghiệp như cổ phiếu",
                "C": "Mọi token đều tạo ra dòng tiền cổ tức tiền mặt định kỳ từ lợi nhuận kế toán đã được kiểm toán",
                "D": "Nắm giữ token hoàn toàn không chịu bất kỳ rủi ro nào về an ninh mạng hay bảo mật khóa riêng tư"
            },
            "ans": "B",
            "explanation": "Cổ phiếu xác lập quyền sở hữu pháp lý đối với tài sản và lợi nhuận công ty, trong khi phần lớn token crypto không đại diện cho vốn chủ sở hữu và không có quyền bảo hộ pháp lý tương đương (B đúng). A sai vì khung pháp lý crypto chưa đồng nhất với chứng khoán; C sai vì token không có cổ tức pháp lý chuẩn mực; D sai vì rủi ro mất khóa riêng tư (private key) và hack smart contract là rất cao."
        },
        {
            "qid": "Q02-06",
            "lo": "LO-W02-03",
            "question": "Khi đánh giá toàn diện một sản phẩm tài chính trước khi giải ngân, khung phân tích nào sau đây là đầy đủ và thận trọng nhất?",
            "options": {
                "A": "Chỉ nhìn vào mức lãi suất cam kết ghi trên áp phích quảng cáo tiếp thị",
                "B": "Chỉ cần xem biểu đồ giá đã tăng liên tục trong tháng gần nhất để ra quyết định",
                "C": "Chỉ dựa vào lời khuyên hoặc bài đăng tán dương của người nổi tiếng trên mạng xã hội",
                "D": "Thẩm định đồng thời lợi nhuận kỳ vọng, mức độ biến động, tính thanh khoản, rủi ro đối tác, khung pháp lý và khả năng mất vốn"
            },
            "ans": "D",
            "explanation": "Thẩm định sản phẩm tài chính chuyên nghiệp đòi hỏi đánh giá đa chiều: lợi nhuận kỳ vọng, rủi ro biến động, thanh khoản khi thoái vốn, uy tín đối tác và cơ chế bảo vệ pháp lý (D đúng). A, B, C là các hành vi cảm tính, thiển cận dẫn đến rủi ro sập bẫy lừa đảo tài chính."
        },
        {
            "qid": "Q02-07",
            "lo": "LO-W02-04",
            "question": "Một người cần số tiền này sau 2 tháng tới để thanh toán viện phí nhưng lại đem mua một tài sản biến động rất mạnh và thanh khoản thấp. Rủi ro lớn nhất người này đối mặt là:",
            "options": {
                "A": "Tài sản này được cơ quan quản lý cam kết mua lại với giá gấp đôi sau 2 tháng",
                "B": "Chắc chắn giá tài sản sẽ tăng vọt giúp người này có thêm khoản thặng dư lớn",
                "C": "Rủi ro thanh khoản và rủi ro buộc phải bán tháo ở mức giá cực kỳ bất lợi khi đến hạn cần tiền gấp",
                "D": "Chỉ chịu rủi ro mất giá tiền tệ do lạm phát thông thường mà không có rủi ro thị trường"
            },
            "ans": "C",
            "explanation": "Chân trời thời gian 2 tháng là quá ngắn; tài sản biến động mạnh và thanh khoản mỏng sẽ đẩy nhà đầu tư vào tình thế buộc phải bán tháo ở giá đáy hoặc không tìm được người mua khi cần tiền gấp (C đúng). A, B, D là những giả định sai lầm, ảo tưởng về sự an toàn và lợi nhuận ngắn hạn."
        },
        {
            "qid": "Q02-08",
            "lo": "LO-W02-04",
            "question": "Kết luận nào sau đây giúp nhà đầu tư tránh đánh đồng giữa 'lợi nhuận kỳ vọng' và 'lợi nhuận cam kết'?",
            "options": {
                "A": "Tài sản rủi ro cao có thể có mức lợi nhuận kỳ vọng cao hơn để bù đắp, nhưng kết quả thực tế luôn bất định và tiềm ẩn nguy cơ thua lỗ nặng",
                "B": "Chấp nhận rủi ro càng cao thì chắc chắn tài khoản sẽ đạt được tỷ suất sinh lời thực tế càng lớn",
                "C": "Hiệu suất đầu tư vượt trội trong quá khứ là bảo chứng chắc chắn cho lợi nhuận trong tương lai",
                "D": "Việc đa dạng hóa danh mục có thể loại bỏ hoàn toàn 100% mọi rủi ro của thị trường tài chính"
            },
            "ans": "A",
            "explanation": "Quy luật rủi ro - lợi nhuận chỉ phát biểu về 'lợi nhuận kỳ vọng' (expected return); rủi ro cao đồng nghĩa biên độ bất định lớn hơn, bao gồm cả khả năng mất trắng vốn, chứ không bảo đảm lợi nhuận (A đúng). B nhầm lẫn giữa kỳ vọng với kết quả thực tế; C vi phạm nguyên lý cơ bản của thị trường tài chính; D sai vì đa dạng hóa chỉ giảm rủi ro phi hệ thống, không xóa được rủi ro hệ thống."
        }
    ]

    build_week_assessment(2, title, module_num, lesson_slug, questions)


def build_w03():
    title = "Đọc Balance Sheet, Income Statement và Cash Flow"
    module_num = 1
    lesson_slug = "week-03-bao-cao-tai-chinh"

    questions = [
        {
            "qid": "Q03-01",
            "lo": "LO-W03-01",
            "question": "Phương trình kế toán nền tảng trên Bảng Cân đối Kế toán (Balance Sheet) được biểu diễn bằng công thức nào?",
            "options": {
                "A": "Tổng tài sản = Doanh thu thuần - Giá vốn hàng bán",
                "B": "Tổng tài sản = Nợ phải trả - Vốn chủ sở hữu",
                "C": "Tổng tài sản = Nợ phải trả + Vốn chủ sở hữu",
                "D": "Vốn chủ sở hữu = Tổng tài sản + Nợ phải trả"
            },
            "ans": "C",
            "explanation": "Phương trình kế toán cân bằng nền tảng: Toàn bộ nguồn lực tài sản của doanh nghiệp (Tổng tài sản) được tài trợ bằng vốn vay (Nợ phải trả) hoặc vốn của chủ sở hữu (Vốn chủ sở hữu) (C đúng). A nhầm sang cách tính lợi nhuận gộp trên báo cáo kết quả kinh doanh; B và D đảo lộn sai lệch các đại lượng kế toán."
        },
        {
            "qid": "Q03-02",
            "lo": "LO-W03-01",
            "question": "Bảng Cân đối Kế toán (Balance Sheet) phản ánh bức tranh tài chính của doanh nghiệp tại thời điểm nào?",
            "options": {
                "A": "Xuyên suốt trong một khoảng thời gian giữa hai kỳ báo cáo tài chính",
                "B": "Tại một thời điểm cụ thể (ảnh chụp nhanh tình trạng tài sản và nguồn vốn tại ngày khóa sổ kế toán)",
                "C": "Chỉ phản ánh các dòng tiền mặt vào và ra phát sinh trong tuần lễ gần nhất",
                "D": "Dự báo chính xác mức giá cổ phiếu của doanh nghiệp trong 12 tháng tới"
            },
            "ans": "B",
            "explanation": "Balance Sheet là một 'bức ảnh chụp nhanh' (snapshot) đo lường số dư tài sản, nợ và vốn chủ sở hữu tại một mốc thời gian cố định cuối kỳ (B đúng). A là đặc điểm của Income Statement và Cash Flow Statement (phản ánh dòng chảy thời kỳ); C là một phần nhỏ của dòng tiền; D hoàn toàn sai mục đích của BCTC."
        },
        {
            "qid": "Q03-03",
            "lo": "LO-W03-02",
            "question": "Báo cáo Kết quả Hoạt động Kinh doanh (Income Statement) phản ánh nội dung tài chính nào sau đây?",
            "options": {
                "A": "Toàn bộ số dư tiền mặt hiện có trong tài khoản ngân hàng của doanh nghiệp",
                "B": "Danh sách chi tiết tất cả các khoản nợ vay ngân hàng ngắn hạn và dài hạn",
                "C": "Giá trị thị trường ước tính của nhà xưởng, đất đai và dây chuyền sản xuất",
                "D": "Doanh thu, chi phí phát sinh và kết quả lợi nhuận ròng được tạo ra trong một kỳ kế toán cụ thể"
            },
            "ans": "D",
            "explanation": "Income Statement đo lường hiệu quả kinh doanh của doanh nghiệp qua một thời kỳ: Doanh thu - Chi phí = Lợi nhuận ròng (D đúng). A và B là các khoản mục số dư thuộc Bảng cân đối kế toán; C là giá trị định giá tài sản, không phải nội dung của Income Statement."
        },
        {
            "qid": "Q03-04",
            "lo": "LO-W03-02",
            "question": "Báo cáo Lưu chuyển Tiền tệ (Cash Flow Statement) phân loại toàn bộ dòng tiền của doanh nghiệp thành 3 hoạt động chuẩn mực nào?",
            "options": {
                "A": "Hoạt động kinh doanh (Operating), hoạt động đầu tư (Investing) và hoạt động tài chính (Financing)",
                "B": "Tài sản ngắn hạn, tài sản dài hạn và vốn chủ sở hữu",
                "C": "Doanh thu bán hàng, thu nhập tài chính và chi phí quản lý doanh nghiệp",
                "D": "Lãi suất tiền gửi, lạm phát kinh tế và biến động tỷ giá ngoại hối"
            },
            "ans": "A",
            "explanation": "Chuẩn mực kế toán quốc tế và Việt Nam phân loại dòng tiền thành 3 dòng chính: Dòng tiền hoạt động kinh doanh (CFO), dòng tiền hoạt động đầu tư (CFI), và dòng tiền hoạt động tài chính (CFF) (A đúng). B là phân loại tài sản trên Balance Sheet; C là các khoản mục trên Income Statement; D là các biến số vĩ mô bên ngoài."
        },
        {
            "qid": "Q03-05",
            "lo": "LO-W03-02",
            "question": "Doanh nghiệp chi tiền mặt để mua sắm máy móc thiết bị mới mở rộng nhà máy. Khoản chi này thuộc dòng tiền nào trên Báo cáo Lưu chuyển Tiền tệ?",
            "options": {
                "A": "Dòng tiền từ hoạt động kinh doanh (CFO)",
                "B": "Dòng tiền từ hoạt động đầu tư (CFI - Cash flow from Investing activities)",
                "C": "Dòng tiền từ hoạt động tài chính (CFF)",
                "D": "Dòng doanh thu tài chính chưa thực hiện"
            },
            "ans": "B",
            "explanation": "Chi tiền mua sắm tài sản cố định, máy móc nhà xưởng (CapEx) là dòng tiền ra thuộc hoạt động đầu tư (CFI) nhằm tạo năng lực sản xuất dài hạn (B đúng). A sai vì CFO phản ánh tiền từ hoạt động cốt lõi hàng ngày; C sai vì CFF phản ánh giao dịch vốn chủ và nợ vay; D là khoản mục kế toán dồn tích."
        },
        {
            "qid": "Q03-06",
            "lo": "LO-W03-03",
            "question": "Một công ty có lợi nhuận sau thuế tăng trưởng 40%, nhưng dòng tiền thuần từ hoạt động kinh doanh (CFO) lại âm nặng. Bước phân tích thận trọng đầu tiên là:",
            "options": {
                "A": "Kiểm tra kỹ các khoản phải thu, hàng tồn kho và chất lượng ghi nhận doanh thu dồn tích trên báo cáo",
                "B": "Kết luận ngay công ty sắp phá sản trong tuần tới và lập tức bán khống cổ phiếu",
                "C": "Bỏ qua vì chỉ cần có lợi nhuận kế toán là dòng tiền thực tế không còn quan trọng",
                "D": "Chỉ quan sát bảng điện tử xem giá cổ phiếu hôm nay có khớp lệnh nhiều hay không"
            },
            "ans": "A",
            "explanation": "Lợi nhuận kế toán tăng nhưng CFO âm nặng thường là dấu hiệu cảnh báo doanh thu chưa thu được tiền mặt (phải thu phình to) hoặc hàng tồn kho ứ đọng; cần kiểm tra kỹ tính lành mạnh của doanh thu (A đúng). B kết luận cực đoan vô căn cứ; C xem nhẹ dòng tiền mặt (yếu tố quyết định thanh khoản doanh nghiệp); D là giao dịch cảm tính mù quáng."
        },
        {
            "qid": "Q03-07",
            "lo": "LO-W03-03",
            "question": "Khoản mục 'Phải thu ngắn hạn khách hàng' tăng trưởng nhanh hơn nhiều so với tốc độ tăng doanh thu thuần có thể gợi ý rủi ro nào?",
            "options": {
                "A": "Tiền mặt trong két và tài khoản ngân hàng của công ty chắc chắn đang tăng rất mạnh",
                "B": "Toàn bộ các khoản nợ phải trả của doanh nghiệp đã được thanh toán hết",
                "C": "Doanh nghiệp nới lỏng chính sách bán chịu để tăng doanh số danh nghĩa, tiền chưa thực thu và tiềm ẩn rủi ro nợ xấu",
                "D": "Biên lợi nhuận ròng của doanh nghiệp chắc chắn sẽ tăng gấp đôi trong quý tới"
            },
            "ans": "C",
            "explanation": "Phải thu tăng vượt tốc độ tăng doanh thu cho thấy công ty bán hàng nhưng chưa thu được tiền mặt, vốn bị chiếm dụng và tiềm ẩn nguy cơ phải trích lập dự phòng nợ khó đòi (C đúng). A sai vì bán chịu không tạo tiền mặt ngay; B và D là những suy đoán vô căn cứ, trái ngược với bản chất kế toán."
        },
        {
            "qid": "Q03-08",
            "lo": "LO-W03-04",
            "question": "Tại sao nhà đầu tư chuyên nghiệp không bao giờ chỉ dùng một con số 'Lợi nhuận kế toán' để đánh giá sức khỏe doanh nghiệp?",
            "options": {
                "A": "Vì lợi nhuận kế toán hoàn toàn không có mối liên hệ nào với hoạt động của doanh nghiệp",
                "B": "Vì chỉ cần có kiểm toán độc lập là dòng tiền kinh doanh chắc chắn luôn luôn dương",
                "C": "Vì Bảng cân đối kế toán luôn là văn bản dự báo chính xác giá cổ phiếu tương lai",
                "D": "Vì lợi nhuận kế toán vận hành theo nguyên tắc dồn tích, chịu tác động của các ước tính kế toán và không bảo đảm có tiền mặt thực tế"
            },
            "ans": "D",
            "explanation": "Kế toán dồn tích ghi nhận doanh thu khi chuyển giao rủi ro/lợi ích và chi phí khi phát sinh nghĩa vụ, do đó lợi nhuận có thể bị thổi phồng hoặc bóp méo qua các thủ thuật kế toán trong khi tiền mặt thực tế lại thiếu hụt (D đúng). A cực đoan vì lợi nhuận vẫn quan trọng nếu đối chiếu dòng tiền; B sai vì kiểm toán không bảo đảm dòng tiền luôn dương; C sai vì BCTC không phải công cụ dự báo giá cổ phiếu."
        }
    ]

    build_week_assessment(3, title, module_num, lesson_slug, questions)


def build_w04():
    title = "Các chỉ số tài chính cơ bản: P/E, P/B, ROE, ROA, D/E, EPS"
    module_num = 1
    lesson_slug = "week-04-chi-so-tai-chinh-co-ban"

    questions = [
        {
            "qid": "Q04-01",
            "lo": "LO-W04-01",
            "question": "Chỉ số Thu nhập trên mỗi cổ phần cơ bản (Basic EPS) thường được tính toán theo công thức nào?",
            "options": {
                "A": "(Lợi nhuận sau thuế - Cổ tức ưu đãi) / Số lượng cổ phiếu phổ thông đang lưu hành bình quân trong kỳ",
                "B": "Doanh thu thuần / Tổng tài sản bình quân trong kỳ",
                "C": "Giá thị trường của cổ phiếu / Tổng vốn chủ sở hữu",
                "D": "Tổng số tiền cổ tức đã chi trả / Tổng nợ phải trả"
            },
            "ans": "A",
            "explanation": "EPS cơ bản đo lường phần lợi nhuận kế toán sau thuế dành cho mỗi cổ phần phổ thông đang lưu hành bình quân trong kỳ (A đúng). B là vòng quay tổng tài sản (Asset Turnover); C liên quan đến tỷ số vốn hóa; D không phải là công thức tài chính chuẩn tắc."
        },
        {
            "qid": "Q04-02",
            "lo": "LO-W04-02",
            "question": "Một cổ phiếu đang giao dịch ở thị giá 60.000 đồng/cp và có EPS trượt 4 quý gần nhất là 2.000 đồng/cp. Hệ số P/E của cổ phiếu này là:",
            "options": {
                "A": "3,0 lần",
                "B": "20,0 lần",
                "C": "30,0 lần",
                "D": "120,0 lần"
            },
            "ans": "C",
            "explanation": "P/E = Giá thị trường / EPS = 60.000 / 2.000 = 30,0 lần (C đúng). A nhầm dấu thập phân; B lấy 60.000 chia 3.000; D lấy 60.000 nhân với 2.000."
        },
        {
            "qid": "Q04-03",
            "lo": "LO-W04-01",
            "question": "Chỉ số P/B (Price-to-Book ratio) so sánh giá thị trường của một cổ phiếu với thước đo nào sau đây?",
            "options": {
                "A": "Doanh thu thuần tính trên mỗi nhân viên của doanh nghiệp",
                "B": "Lãi suất danh nghĩa của trái phiếu chính phủ kỳ hạn 10 năm",
                "C": "Dòng tiền thuần từ hoạt động kinh doanh trên mỗi cổ phần",
                "D": "Giá trị sổ sách của vốn chủ sở hữu trên mỗi cổ phần (BVPS - Book Value per Share)"
            },
            "ans": "D",
            "explanation": "P/B so sánh thị giá cổ phiếu với giá trị sổ sách của vốn chủ sở hữu trên mỗi cổ phần (BVPS = Vốn chủ sở hữu / Số cổ phiếu lưu hành) (D đúng). A là chỉ số năng suất lao động; B là lãi suất phi rủi ro; C là tỷ số P/CFO (Price to Cash Flow)."
        },
        {
            "qid": "Q04-04",
            "lo": "LO-W04-02",
            "question": "Chỉ số Lợi nhuận trên Vốn chủ sở hữu (ROE - Return on Equity) đo lường điều gì trong phân tích doanh nghiệp?",
            "options": {
                "A": "Tỷ lệ giá thị trường của cổ phiếu so với lợi nhuận EPS",
                "B": "Mức lợi nhuận ròng tạo ra trên mỗi đồng vốn chủ sở hữu bình quân của cổ đông",
                "C": "Tổng số nợ vay ngân hàng chia cho tổng vốn chủ sở hữu",
                "D": "Tỷ lệ tiền mặt và tiền gửi ngân hàng trên tổng doanh thu thuần"
            },
            "ans": "B",
            "explanation": "ROE = Lợi nhuận sau thuế / Vốn chủ sở hữu bình quân, phản ánh hiệu quả ban điều hành sinh lời trên đồng vốn mà cổ đông ủy thác (B đúng). A là định nghĩa của P/E; C là hệ số đòn bẩy D/E; D là tỷ số thanh khoản tiền mặt."
        },
        {
            "qid": "Q04-05",
            "lo": "LO-W04-02",
            "question": "Công ty ABC có lợi nhuận sau thuế trong năm là 12 tỷ đồng và tổng tài sản bình quân là 200 tỷ đồng. Tỷ số ROA xấp xỉ là:",
            "options": {
                "A": "0,6%",
                "B": "6,0%",
                "C": "16,7%",
                "D": "24,0%"
            },
            "ans": "B",
            "explanation": "ROA = Lợi nhuận sau thuế / Tổng tài sản bình quân = 12 / 200 = 0,06 = 6,0% (B đúng). A nhầm vị trí dấu thập phân; C tính ngược (200 / 12); D nhầm sang phép nhân đôi."
        },
        {
            "qid": "Q04-06",
            "lo": "LO-W04-03",
            "question": "Hệ số Nợ trên Vốn chủ sở hữu (D/E - Debt-to-Equity) của một doanh nghiệp bằng 2,0 lần. Điều này được hiểu là:",
            "options": {
                "A": "Vốn chủ sở hữu của doanh nghiệp lớn gấp 2 lần tổng nợ phải trả",
                "B": "Tổng nợ phải trả chỉ chiếm 2% trong cơ cấu nguồn vốn",
                "C": "Doanh nghiệp hoàn toàn không sử dụng bất kỳ khoản nợ vay nào",
                "D": "Tổng nợ phải trả lớn gấp 2 lần giá trị vốn chủ sở hữu của doanh nghiệp"
            },
            "ans": "D",
            "explanation": "D/E = Tổng nợ / Vốn chủ sở hữu = 2,0 nghĩa là với mỗi 1 đồng vốn chủ, doanh nghiệp đang gánh 2 đồng nợ (tổng tài sản gấp 3 lần vốn chủ) (D đúng). A đảo ngược tỷ số; B nhầm số lần thành tỷ lệ phần trăm; C phủ nhận sự tồn tại của đòn bẩy tài chính."
        },
        {
            "qid": "Q04-07",
            "lo": "LO-W04-03",
            "question": "Một cổ phiếu có P/E rất thấp (ví dụ P/E = 4) KHÔNG tự động đồng nghĩa là cơ hội đầu tư hời, vì:",
            "options": {
                "A": "P/E hoàn toàn không liên quan gì đến giá cổ phiếu hay lợi nhuận của doanh nghiệp",
                "B": "Mọi công ty có P/E thấp chắc chắn đều không sở hữu bất kỳ tài sản nào",
                "C": "Lợi nhuận EPS hiện tại có thể đang ở đỉnh chu kỳ kinh doanh hoặc chứa khoản thu đột biến, trong khi tương lai đối mặt suy thoái nặng (bẫy giá trị)",
                "D": "P/E thấp chỉ xuất hiện do lỗi kỹ thuật tính toán của sàn giao dịch chứng khoán"
            },
            "ans": "C",
            "explanation": "Hiện tượng 'Bẫy giá trị' (Value Trap): P/E thấp có thể do EPS quá khứ được thổi phồng bởi thu nhập một lần (bán tài sản, chuyển nhượng dự án) hoặc lợi nhuận ngành tạo đỉnh chu kỳ và sắp lao dốc (C đúng). A sai vì P/E trực tiếp kết hợp giá và EPS; B và D là nhận định vô căn cứ."
        },
        {
            "qid": "Q04-08",
            "lo": "LO-W04-04",
            "question": "Khi so sánh các chỉ số tài chính như P/E và ROE giữa các doanh nghiệp để lựa chọn đầu tư, nguyên tắc thận trọng nhất là:",
            "options": {
                "A": "So sánh giữa các doanh nghiệp trong cùng một ngành nghề, cùng chu kỳ và đọc kết hợp với dòng tiền, nợ vay và chất lượng tài sản",
                "B": "Luôn chọn cổ phiếu có P/E thấp nhất tuyệt đối bất kể doanh nghiệp hoạt động trong lĩnh vực nào",
                "C": "Chỉ cần chọn cổ phiếu có ROE cao nhất mà không cần quan tâm đến mức độ sử dụng đòn bẩy tài chính",
                "D": "Bỏ qua các khác biệt về chuẩn mực kế toán và đặc thù ngành vì tất cả các công ty niêm yết đều giống nhau"
            },
            "ans": "A",
            "explanation": "Mỗi ngành có cấu trúc vốn, đòn bẩy và chu kỳ khác nhau; các chỉ số định giá và sinh lời chỉ có ý nghĩa khi so sánh cùng ngành và đối chiếu với dòng tiền cùng cấu trúc nợ (A đúng). B dễ sập bẫy giá trị; C bỏ qua rủi ro nợ vay quá lớn làm phóng đại ROE (DuPont); D xem nhẹ đặc thù mô hình kinh doanh."
        }
    ]

    build_week_assessment(4, title, module_num, lesson_slug, questions)


if __name__ == "__main__":
    build_w01()
    build_w02()
    build_w03()
    build_w04()
    print("All Module 1 assessments built successfully!")
