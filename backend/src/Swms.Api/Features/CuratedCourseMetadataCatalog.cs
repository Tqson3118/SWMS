using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;
using Swms.Api.Infrastructure.Database;

namespace Swms.Api.Features;

public sealed record CourseCuratedMetadata(
    string Category,
    string Level,
    string TargetAudience,
    string LabLabel,
    bool HasAudio,
    string[] LearningOutcomes,
    string[] Prerequisites);

public static class CuratedCourseMetadataCatalog
{
    private static readonly Dictionary<string, CourseCuratedMetadata> MetadataBySlug = new(StringComparer.OrdinalIgnoreCase)
    {
        ["doc-course-studxy-fullstack-bootcamp"] = new(
            Category: "Lập trình Full-Stack",
            Level: "intermediate",
            TargetAudience: "Lập trình viên và sinh viên CNTT muốn làm chủ trọn vẹn hệ sinh thái .NET 10 Web API & React 19 TypeScript, từ kiến trúc Clean/Vertical Slice đến quy trình CI/CD và vận hành thực tế.",
            LabLabel: "Lab Lập trình & Gỡ lỗi",
            HasAudio: false,
            LearningOutcomes:
            [
                "Thiết kế và xây dựng RESTful Web API chuẩn mực, dễ mở rộng với ASP.NET Core và C# 13.",
                "Xây dựng ứng dụng web đơn trang (SPA) trực quan, tương thích đa thiết bị với React 19, TypeScript và Tailwind CSS.",
                "Mô hình hóa dữ liệu quan hệ và tối ưu hiệu năng truy vấn cùng Entity Framework Core trên PostgreSQL và SQLite.",
                "Vận dụng Clean Architecture, CQRS, xác thực JWT, Google OAuth2 và phòng vệ bảo mật đa lớp.",
                "Xây dựng kiểm thử đơn vị, kiểm thử tích hợp với xUnit và đóng gói ứng dụng bằng Docker sẵn sàng triển khai thực tế."
            ],
            Prerequisites:
            [
                "Nền tảng lập trình hướng đối tượng (OOP) cơ bản và tư duy thuật toán.",
                "Hiểu biết cơ bản về HTML, CSS và cú pháp JavaScript ES6.",
                "Máy tính đã cài đặt .NET SDK, Node.js và trình soạn thảo mã nguồn (VS Code hoặc Visual Studio)."
            ]
        ),

        ["document-course-ielts"] = new(
            Category: "Ngoại ngữ IELTS",
            Level: "intermediate",
            TargetAudience: "Học viên luyện thi IELTS Academic hướng tới mục tiêu band 6.5–7.5+, cần củng cố phản xạ Nghe – Nói tự nhiên và tư duy Viết học thuật.",
            LabLabel: "Phòng Nghe – Nói IPA",
            HasAudio: true,
            LearningOutcomes:
            [
                "Làm chủ 4 kỹ năng Nghe, Đọc, Viết và Nói theo định dạng chuẩn của bài thi Cambridge IELTS.",
                "Rèn luyện kỹ thuật đọc lướt (Skimming), đọc tìm chi tiết (Scanning) và nhận diện diễn đạt tương đương (Paraphrasing) trong văn bản học thuật.",
                "Phát triển phát âm chuẩn IPA, ngữ điệu tự nhiên và bố cục trả lời mạch lạc cho IELTS Speaking Part 1–3.",
                "Xây dựng luận điểm chặt chẽ, liên kết ý mạch lạc và đa dạng hóa cấu trúc ngữ pháp trong IELTS Writing Task 1 & 2.",
                "Ghi nhớ bền vững và vận dụng linh hoạt vốn từ vựng học thuật theo chủ đề thông qua phương pháp lặp lại ngắt quãng FSRS."
            ],
            Prerequisites:
            [
                "Trình độ tiếng Anh nền tảng tương đương CEFR B1 hoặc IELTS 4.5–5.0.",
                "Nắm vững ngữ pháp cơ bản gồm các thì thông dụng, câu điều kiện và mệnh đề quan hệ.",
                "Có tai nghe hoặc thiết bị âm thanh để luyện tập các bài nghe tương tác."
            ]
        ),

        ["document-course-investing"] = new(
            Category: "Tài chính & Đầu tư",
            Level: "intermediate",
            TargetAudience: "Nhà đầu tư cá nhân muốn xây dựng hệ thống phân tích doanh nghiệp, định giá cổ phiếu và quản trị danh mục bài bản, giữ vững kỷ luật trước biến động thị trường.",
            LabLabel: "Lab Đầu tư & Định giá",
            HasAudio: false,
            LearningOutcomes:
            [
                "Đọc hiểu và phân tích chuyên sâu Báo cáo tài chính (Bảng cân đối kế toán, Kết quả kinh doanh, Lưu chuyển tiền tệ) của doanh nghiệp niêm yết.",
                "Vận dụng các phương pháp định giá phổ biến (P/E, P/B và chiết khấu dòng tiền DCF) để xác định biên an toàn khi đầu tư.",
                "Thiết kế danh mục đầu tư đa tài sản cân bằng giữa rủi ro và kỳ vọng sinh lời theo từng chu kỳ kinh tế vĩ mô.",
                "Thiết lập kỷ luật quản trị rủi ro, nguyên tắc chốt lời – cắt lỗ và chiến lược bình quân giá (DCA) nhằm kiểm soát mức sụt giảm tối đa (Max Drawdown).",
                "Nhận diện các thiên kiến hành vi phổ biến (FOMO, hiệu ứng mỏ neo, tâm lý đám đông) để giữ vững quyết định đầu tư lý tính."
            ],
            Prerequisites:
            [
                "Nắm được các khái niệm toán tài chính căn bản như lãi suất, tỷ suất sinh lời và tỷ lệ phần trăm.",
                "Có định hướng học tập nghiêm túc về đầu tư dài hạn dựa trên giá trị nội tại của doanh nghiệp."
            ]
        ),

        ["document-course-personalfinance"] = new(
            Category: "Tài chính & Đầu tư",
            Level: "foundation",
            TargetAudience: "Sinh viên, người mới đi làm và gia đình trẻ muốn làm chủ dòng tiền cá nhân, xử lý nợ lãi cao, xây dựng quỹ dự phòng và hoạch định lộ trình độc lập tài chính (FIRE).",
            LabLabel: "Lab Tài chính Cá nhân",
            HasAudio: false,
            LearningOutcomes:
            [
                "Kiểm soát toàn diện dòng tiền thu chi cá nhân bằng mô hình 6 chiếc lọ và nguyên tắc phân bổ ngân sách 50/30/20 linh hoạt.",
                "Xây dựng quỹ dự phòng khẩn cấp đủ 3–6 tháng sinh hoạt phí và lộ trình tất toán nợ lãi suất cao (Snowball và Avalanche).",
                "Hiểu đúng bản chất các sản phẩm bảo hiểm nhân thọ, phi nhân thọ để bảo vệ tài chính gia đình trước rủi ro bất ngờ.",
                "Tận dụng sức mạnh của lãi kép để bắt đầu tích lũy đầu tư bền vững từ quỹ chỉ số, vàng và tiền gửi tiết kiệm với số vốn nhỏ.",
                "Xác định con số mục tiêu độc lập tài chính (FIRE) và xây dựng kế hoạch tài chính cá nhân trong tầm nhìn 5–10 năm."
            ],
            Prerequisites:
            [
                "Không yêu cầu nền tảng kiến thức tài chính chuyên sâu.",
                "Sẵn sàng ghi chép và theo dõi thu chi thực tế xuyên suốt quá trình học."
            ]
        ),

        ["document-course-lifeskills"] = new(
            Category: "Tâm lý & Kỹ năng sống",
            Level: "foundation",
            TargetAudience: "Người học ở mọi lứa tuổi muốn phát triển trí tuệ cảm xúc (EQ), thực hành giao tiếp trắc ẩn, xử lý xung đột tinh tế, rèn tư duy phản biện và quản trị năng lượng bền vững.",
            LabLabel: "Phòng Tình huống NVC & CBT",
            HasAudio: false,
            LearningOutcomes:
            [
                "Thực hành Giao tiếp Phi Bạo Lực (NVC) để diễn đạt nhu cầu rõ ràng và lắng nghe thấu cảm không phán xét.",
                "Nhận diện và điều hòa cảm xúc cá nhân thông qua kỹ thuật tái cấu trúc nhận thức và hít thở chánh niệm.",
                "Rèn luyện tư duy phản biện để phân biệt dữ kiện khách quan với suy diễn chủ quan và nhận diện các ngụy biện logic.",
                "Thiết lập ranh giới lành mạnh trong công việc và các mối quan hệ mà vẫn giữ được sự tôn trọng lẫn nhau.",
                "Quản trị năng lượng sinh học và sắp xếp ưu tiên theo ma trận Eisenhower để làm việc hiệu quả và phòng tránh kiệt sức (burnout)."
            ],
            Prerequisites:
            [
                "Tâm thế cởi mở, sẵn sàng quan sát và soi chiếu thói quen giao tiếp của bản thân.",
                "Mong muốn nâng cao chất lượng các mối quan hệ và sự cân bằng trong nội tâm."
            ]
        ),

        ["document-course-psychology"] = new(
            Category: "Tâm lý & Kỹ năng sống",
            Level: "intermediate",
            TargetAudience: "Người yêu thích khám phá cơ chế vận hành của não bộ, khoa học hành vi, tâm lý học nhận thức, các kiểu gắn bó trong mối quan hệ và phương pháp chăm sóc sức khỏe tinh thần.",
            LabLabel: "Phòng Tình huống NVC & CBT",
            HasAudio: false,
            LearningOutcomes:
            [
                "Hiểu rõ cơ chế hoạt động của não bộ, hệ thần kinh và vai trò điều tiết của các chất dẫn truyền thần kinh (Dopamine, Serotonin, Cortisol).",
                "Phân tích hai cơ chế tư duy Hệ thống 1 (trực giác nhanh) và Hệ thống 2 (suy luận có chủ đích) theo nghiên cứu của Daniel Kahneman.",
                "Nhận diện 4 kiểu gắn bó trong tình cảm (an toàn, lo âu, né tránh, hỗn loạn) và hướng chữa lành tổn thương tâm lý.",
                "Ứng dụng mô hình ABC trong Liệu pháp Nhận thức Hành vi (CBT) để nhận biết và điều chỉnh các suy nghĩ tự động thiếu cân bằng.",
                "Vận dụng các quy luật ghi nhớ của não bộ (Đường cong quên Ebbinghaus, Chunking, Retrieval Practice) để học tập sâu và ghi nhớ bền vững."
            ],
            Prerequisites:
            [
                "Có niềm yêu thích tìm hiểu về tâm lý học đại cương và hành vi con người.",
                "Sẵn sàng đọc và suy ngẫm các nghiên cứu thực nghiệm cùng tình huống lâm sàng thực tế."
            ]
        ),

        ["he-dieu-hanh-bo-nho-ao"] = new(
            Category: "Khoa học máy tính",
            Level: "advanced",
            TargetAudience: "Sinh viên Khoa học Máy tính và kỹ sư phần mềm muốn nắm vững tầng hệ thống qua 12 bài học · 5 chương: quản lý bộ nhớ ảo, phân trang phần cứng, MMU, TLB, hệ thống tệp tin I/O, đồng bộ hóa nâng cao, Linux Kernel và ảo hóa Container.",
            LabLabel: "Lab Bộ nhớ & Điều phối OS",
            HasAudio: false,
            LearningOutcomes:
            [
                "Làm chủ lộ trình 12 bài học · 5 chương về Hệ điều hành: tiến trình, điều phối CPU, đồng bộ hóa và quản lý bộ nhớ ảo (Virtual Memory, Paging).",
                "Phân tích cơ chế phối hợp giữa khối quản lý bộ nhớ (MMU) và bộ đệm dịch địa chỉ (TLB) cùng thời gian truy cập bộ nhớ hiệu dụng (EAT).",
                "So sánh, mô phỏng và cài đặt các thuật toán thay thế trang kinh điển: FIFO, LRU, Clock (Second-Chance) và Optimal.",
                "Hiểu sâu cấu trúc hệ thống tệp tin (Inode, VFS, Page Cache), lập lịch I/O đĩa (NVMe/SSD) và cơ chế tối ưu bộ nhớ đệm Kernel.",
                "Vận dụng đồng bộ hóa nâng cao (Read-Write Lock, Lock-free, RCU), eBPF/System Calls và nguyên lý cô lập Namespace/Cgroups của Container."
            ],
            Prerequisites:
            [
                "Nắm vững ngôn ngữ C/C++ cơ bản và kiến trúc máy tính (thanh ghi, CPU, RAM).",
                "Hiểu biết về hệ nhị phân, thập lục phân và các phép toán thao tác bit (bitwise, bitmask)."
            ]
        ),

        ["tieng-anh-giao-tiep-thuc-chien-a2-b1"] = new(
            Category: "Ngoại ngữ",
            Level: "foundation",
            TargetAudience: "Người đi làm và sinh viên cần xây dựng nền tảng tiếng Anh giao tiếp A2–B1 qua 12 bài học · 5 chương: từ giao tiếp đời thường, email và họp trực tuyến đến du lịch quốc tế, phỏng vấn kỹ thuật STAR, storytelling số liệu và viết prompt AI tự nhiên.",
            LabLabel: "Phòng Nghe – Nói IPA",
            HasAudio: true,
            LearningOutcomes:
            [
                "Hoàn thành lộ trình 12 bài học · 5 chương nâng trình độ giao tiếp thực chiến từ A2 lên B1 tự tin trong đời sống và công sở quốc tế.",
                "Xử lý linh hoạt tình huống du lịch, xuất nhập cảnh, đặt phòng, soạn email công việc và điều phối họp trực tuyến chuyên nghiệp.",
                "Trả lời phỏng vấn kỹ thuật theo cấu trúc STAR (Situation–Task–Action–Result) và tham gia Code Review, báo cáo lỗi mạch lạc.",
                "Thuyết trình storytelling bằng số liệu kinh doanh (BLUF), xử lý khủng hoảng khách hàng (LAER) và từ chối khéo léo (Positive No).",
                "Viết email tự nhiên chuẩn bản xứ và thiết kế câu lệnh AI (Prompt Engineering) bằng tiếng Anh chính xác, súc tích."
            ],
            Prerequisites:
            [
                "Nhận biết bảng chữ cái tiếng Anh và các mẫu câu chào hỏi cơ bản.",
                "Duy trì thói quen luyện nói thành tiếng và ghi âm đối chiếu phát âm hằng ngày."
            ]
        ),

        ["tieng-nhat-so-cap-n5"] = new(
            Category: "Ngoại ngữ",
            Level: "foundation",
            TargetAudience: "Người mới bắt đầu học tiếng Nhật từ đầu với lộ trình 12 bài học · 5 chương toàn diện hướng tới chứng chỉ JLPT N5: Hiragana, Katakana, 100+ Kanji, tính từ, sở thích gia đình, động từ thể Te và diễn đạt kế hoạch tương lai.",
            LabLabel: "Lab Luyện Nhật N5",
            HasAudio: true,
            LearningOutcomes:
            [
                "Chinh phục trọn vẹn 12 bài học · 5 chương chuẩn JLPT N5 từ Hiragana, Katakana, trường âm, âm ngắt đến giao tiếp đời sống tự nhiên.",
                "Nhận diện và viết đúng quy tắc hơn 100 chữ Hán (Kanji) nền tảng trình độ N5 kèm âm On và âm Kun thông dụng.",
                "Vận dụng thành thạo tính từ đuôi い/な, mẫu câu miêu tả gia đình, sở thích, năng lực (〜ができます) và mức độ.",
                "Chia nhuần nhuyễn động từ thể Masu, thể Te (〜て形: xin phép 〜てもいいですか, cấm đoán 〜てはいけません, nối hành động) và thể Nai.",
                "Tự tin diễn đạt mong muốn (〜たいです, 〜が欲しいです), kế hoạch du lịch, đặt phòng và nghe đọc hiểu đề thi JLPT N5."
            ],
            Prerequisites:
            [
                "Không yêu cầu kiến thức tiếng Nhật trước đó.",
                "Chuẩn bị vở ô ly hoặc bảng luyện nét để thực hành viết Hiragana, Katakana và Kanji."
            ]
        ),

        ["tieng-trung-so-cap-hsk1"] = new(
            Category: "Ngoại ngữ",
            Level: "foundation",
            TargetAudience: "Người mới bắt đầu học tiếng Trung Quan Thoại với giáo trình 12 bài học · 5 chương chuẩn HSK 1+: Pinyin, 4 thanh điệu, gia đình, thời tiết, công việc, trợ động từ năng nguyện và cấu trúc nhấn mạnh quá khứ.",
            LabLabel: "Lab Luyện Trung HSK1",
            HasAudio: true,
            LearningOutcomes:
            [
                "Làm chủ lộ trình 12 bài học · 5 chương chuẩn HSK 1: hệ thống Pinyin (21 thanh mẫu, 36 vận mẫu), 4 thanh điệu và quy tắc biến điệu.",
                "Nhận diện và ghi nhớ cách viết hơn 150 chữ Hán (Hán tự) thông dụng nhất cấp độ HSK 1 theo đúng quy tắc thuận bút.",
                "Giao tiếp tự nhiên về chào hỏi, thời gian, gia đình, lượng từ, thời tiết 4 mùa, nghề nghiệp và trạng thái đang diễn ra (正在...呢).",
                "Sử dụng chính xác trợ động từ năng nguyện (会, 能, 可以, 想, 要), trợ từ hoàn thành 了 và khung nhấn mạnh quá khứ 是...的.",
                "Rèn luyện nghe hiểu, hỏi khoảng cách – thời lượng đi lại và sẵn sàng chinh phục kỳ thi HSK 1 với điểm số 180+/200."
            ],
            Prerequisites:
            [
                "Không yêu cầu kiến thức tiếng Trung trước đó.",
                "Có micro hoặc thiết bị ghi âm để luyện tập đối chiếu thanh điệu Pinyin."
            ]
        ),

        ["document-course-ai-workflows"] = new(
            Category: "AI & Tự động hóa",
            Level: "intermediate",
            TargetAudience: "Sinh viên, nhân sự vận hành, PM/BA và kỹ sư phần mềm muốn làm chủ Prompt Engineering chuẩn kỹ thuật, Structured Outputs, RAG, giao thức MCP và thiết kế Agent Workflow an toàn, đo lường được hiệu quả (Evals) năm 2026.",
            LabLabel: "Lab AI & Prompt Engineering",
            HasAudio: false,
            LearningOutcomes:
            [
                "Nắm vững cơ chế LLM, Tokenization tiếng Việt, giới hạn Context Window và dự toán chi phí/độ trễ mô hình thực tế.",
                "Thiết kế hoàn chỉnh kiến trúc Prompt 5 lớp, ép đầu ra JSON Schema (Structured Outputs) và xây dựng bộ kiểm thử tự động (Evals).",
                "Kiểm soát ảo giác AI (Hallucination) bằng quy trình kiểm chứng nguồn 3 bước và kiến trúc RAG (Chunking, Hybrid Search, trích dẫn bằng chứng).",
                "Xây dựng Tool Contract, tích hợp giao thức MCP, thiết lập Guardrails, Human-in-the-Loop (HITL) và tự động hóa quy trình email/báo cáo.",
                "Lập trình và review code an toàn với AI theo kỷ luật Test-First, che chắn dữ liệu PII (Redaction) và phòng vệ Prompt Injection."
            ],
            Prerequisites:
            [
                "Đã từng sử dụng công cụ AI tạo sinh cơ bản trong học tập hoặc công việc văn phòng.",
                "Có tư duy logic mạch lạc và sẵn sàng thực hành viết lại Prompt có cấu trúc JSON/Markdown."
            ]
        ),

        ["document-course-data-sql"] = new(
            Category: "Dữ liệu & Phân tích",
            Level: "intermediate",
            TargetAudience: "Business Analyst (BA), Product Manager (PM), Data Analyst mới vào nghề, nhân sự Marketing/Operations và sinh viên muốn viết SQL phân tích chuẩn xác, tránh bẫy thống kê và thiết kế Dashboard ra quyết định.",
            LabLabel: "Lab SQL & Phân tích Dữ liệu",
            HasAudio: false,
            LearningOutcomes:
            [
                "Phân biệt tương quan và nhân quả, nhận diện Nghịch lý Simpson, bẫy trung bình và xác định đúng Grain của bảng dữ liệu quan hệ.",
                "Viết thành thạo SQL theo đúng thứ tự thực thi logic (Logical Execution Order), xử lý NULL an toàn với GROUP BY, HAVING và CASE WHEN.",
                "Phòng tránh bẫy nhân bản dòng (Fan-out Join Trap), chuẩn hóa múi giờ Việt Nam (UTC+7) và đối soát doanh thu ròng sau hoàn/hủy.",
                "Làm chủ CTE (WITH), Window Functions (ROW_NUMBER, RANK, LAG/LEAD, Running Total), phân tích Cohort Retention và Conversion Funnel.",
                "Xây dựng cây chỉ số KPI Tree, thiết kế A/B Testing tránh dừng sớm/Goodhart's Law và kể chuyện ra quyết định qua Dashboard."
            ],
            Prerequisites:
            [
                "Sử dụng thành thạo bảng tính (Excel/Google Sheets) và các phép tính tỷ lệ phần trăm cơ bản.",
                "Không yêu cầu kinh nghiệm SQL chuyên sâu trước khi bắt đầu từ Module 1."
            ]
        ),

        ["document-course-cybersecurity"] = new(
            Category: "An toàn thông tin",
            Level: "foundation",
            TargetAudience: "Mọi công dân số tại Việt Nam sử dụng ngân hàng số, ví điện tử, VietQR, VNeID cũng như nhân viên văn phòng và lập trình viên muốn xây dựng phản xạ phòng vệ lừa đảo số, bảo mật tài khoản và ứng phó sự cố.",
            LabLabel: "Lab An toàn số & Phòng vệ",
            HasAudio: false,
            LearningOutcomes:
            [
                "Nhận diện và vô hiệu hóa kịch bản thao túng tâm lý khẩn cấp (Social Engineering), Phishing, Brand Spoofing và mã QR độc hại (Quishing).",
                "Phát hiện dấu hiệu Deepfake giọng nói/video giả mạo cơ quan chức năng, mã độc APK giả VNeID và tiện ích trình duyệt nguy hiểm.",
                "Triển khai bộ lớp phòng vệ xác thực hiện đại: Password Manager, Passkeys (FIDO2), Authenticator App/Hardware Key và khóa SIM/OTP.",
                "Bảo vệ tài khoản ngân hàng, thẻ tín dụng, ví điện tử; giảm thiểu dữ liệu cá nhân (PII), kiểm soát dấu chân số và dữ liệu đưa vào AI.",
                "Thực hành Zero Trust cá nhân, xác minh kênh độc lập (Out-of-Band), bảo mật secret/.env khi làm việc và quy trình sao lưu 3-2-1-1-0 cùng ứng cứu 15 phút."
            ],
            Prerequisites:
            [
                "Sử dụng điện thoại thông minh, máy tính cá nhân và các ứng dụng ngân hàng/mạng xã hội hằng ngày.",
                "Sẵn sàng kiểm tra và nâng cấp cài đặt bảo mật trên thiết bị cá nhân ngay trong quá trình học."
            ]
        ),

        ["document-course-deep-learning-systems"] = new(
            Category: "Phương pháp học & Hệ thống",
            Level: "foundation",
            TargetAudience: "Sinh viên, kỹ sư, nhà phân tích và người học suốt đời muốn thoát khỏi Ảo tưởng Năng lực thời AI (Fluency Illusion), làm chủ khoa học trí nhớ FSRS v5, tư duy hệ thống và quản trị sự chú ý sâu.",
            LabLabel: "Lab Học sâu & Hệ thống",
            HasAudio: false,
            LearningOutcomes:
            [
                "Nhận diện và loại bỏ Ảo tưởng Năng lực khi học cùng AI; chuyển AI từ công cụ làm hộ sang đối tác phản biện (Socratic Sparring Partner).",
                "Áp dụng hiệu quả Active Recall, Testing Effect, Desirable Difficulty, kỹ thuật Feynman và Ôn tập Xen kẽ (Interleaving).",
                "Thiết kế thẻ ghi nhớ nguyên tử chuẩn khoa học và vận hành lịch lặp lại ngắt quãng FSRS v5 kết hợp hệ thống Evergreen Notes (PKM).",
                "Phân tích vấn đề phức tạp bằng Tư duy Hệ thống: Stock & Flow, Feedback Loops, Điểm nghẽn (TOC), Tư duy Bậc hai, MECE, Issue Tree và 5 Whys.",
                "Bảo vệ sự tập trung sâu (Deep Work), giảm Dư lượng Chú ý (Attention Residue), thực hành Pre-mortem và vận hành Personal Learning OS trọn đời."
            ],
            Prerequisites:
            [
                "Không yêu cầu nền tảng học thuật chuyên ngành; phù hợp với mọi lĩnh vực học tập và làm việc tri thức.",
                "Sẵn sàng thực hành ghi chú chủ động, tự kiểm tra truy xuất ký ức và viết nhật ký phản tư mỗi tuần."
            ]
        )
    };

    public static IReadOnlyCollection<string> CanonicalCourseSlugs => MetadataBySlug.Keys;

    private static readonly Dictionary<Guid, CourseCuratedMetadata> MetadataByCourseId = new()
    {
        [SeedDataInitializer.AcademicCourseId] = MetadataBySlug["he-dieu-hanh-bo-nho-ao"],
        [SeedDataInitializer.EnglishCourseId] = MetadataBySlug["tieng-anh-giao-tiep-thuc-chien-a2-b1"],
        [SeedDataInitializer.JapaneseCourseId] = MetadataBySlug["tieng-nhat-so-cap-n5"],
        [SeedDataInitializer.ChineseCourseId] = MetadataBySlug["tieng-trung-so-cap-hsk1"]
    };

    private static CourseCuratedMetadata? FindCuratedMetadata(Course course)
    {
        if (!string.IsNullOrWhiteSpace(course.Slug) && MetadataBySlug.TryGetValue(course.Slug, out var bySlug))
        {
            return bySlug;
        }
        if (MetadataByCourseId.TryGetValue(course.Id, out var byId))
        {
            return byId;
        }
        return null;
    }

    public static CourseCuratedMetadata ResolveMetadata(Course course)
    {
        var defaultMeta = FindCuratedMetadata(course) ?? ResolveFallbackMetadata(course);

        var effectiveCategory = !string.IsNullOrWhiteSpace(course.Category)
            ? course.Category.Trim()
            : defaultMeta.Category;

        var effectiveLevel = !string.IsNullOrWhiteSpace(course.Level)
            ? course.Level.Trim().ToLowerInvariant()
            : defaultMeta.Level;

        var effectiveOutcomes = TryDeserializeStringList(course.LearningOutcomesJson);
        if (effectiveOutcomes == null || effectiveOutcomes.Length == 0)
        {
            effectiveOutcomes = defaultMeta.LearningOutcomes;
        }

        var effectivePrerequisites = TryDeserializeStringList(course.PrerequisitesJson);
        if (effectivePrerequisites == null || effectivePrerequisites.Length == 0)
        {
            effectivePrerequisites = defaultMeta.Prerequisites;
        }

        return new CourseCuratedMetadata(
            Category: effectiveCategory,
            Level: effectiveLevel,
            TargetAudience: defaultMeta.TargetAudience,
            LabLabel: defaultMeta.LabLabel,
            HasAudio: defaultMeta.HasAudio,
            LearningOutcomes: effectiveOutcomes,
            Prerequisites: effectivePrerequisites);
    }

    private static string[]? TryDeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(json);
            return list != null && list.Count > 0 ? list.ToArray() : null;
        }
        catch
        {
            return null;
        }
    }

    private static CourseCuratedMetadata ResolveFallbackMetadata(Course course)
    {
        var title = course.Title ?? string.Empty;
        var desc = course.Description ?? string.Empty;

        string category = "Chung";
        string level = "foundation";
        string targetAudience = "Người học tự định hướng trên nền tảng Studxy.";
        string labLabel = "Thực hành & Ôn tập";
        bool hasAudio = false;

        if (title.Contains("Lập trình", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("Code", StringComparison.OrdinalIgnoreCase) ||
            title.Contains(".NET", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("React", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("Web", StringComparison.OrdinalIgnoreCase))
        {
            category = "Lập trình Full-Stack";
            level = "intermediate";
            labLabel = "Lab Lập trình & Gỡ lỗi";
        }
        else if (title.Contains("Tài chính", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Đầu tư", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Tiền tệ", StringComparison.OrdinalIgnoreCase))
        {
            category = "Tài chính & Đầu tư";
            level = "intermediate";
            labLabel = "Lab Tài chính & Đầu tư";
        }
        else if (title.Contains("Tâm lý", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Kỹ năng", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Giao tiếp", StringComparison.OrdinalIgnoreCase))
        {
            category = "Tâm lý & Kỹ năng sống";
            level = "foundation";
            labLabel = "Phòng Tình huống & EQ";
        }
        else if (title.Contains("Tiếng", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("IELTS", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Ngoại ngữ", StringComparison.OrdinalIgnoreCase))
        {
            category = "Ngoại ngữ";
            level = "foundation";
            labLabel = "Phòng Nghe – Nói";
            hasAudio = true;
        }

        return new CourseCuratedMetadata(
            Category: category,
            Level: level,
            TargetAudience: targetAudience,
            LabLabel: labLabel,
            HasAudio: hasAudio,
            LearningOutcomes: new[] { "Nắm vững kiến thức nền tảng và phương pháp tư duy", "Áp dụng vào thực hành và giải quyết vấn đề thực tế" },
            Prerequisites: new[] { "Sẵn sàng học tập và kiên trì rèn luyện mỗi ngày" });
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task ApplyDefaultsToCoursesAsync(SwmsDbContext db, CancellationToken ct = default)
    {
        var courses = await db.Courses.ToListAsync(ct);
        bool modified = false;
        var now = DateTimeOffset.UtcNow;

        foreach (var course in courses)
        {
            if (DocumentCourseSyncService.IsRetiredCourseSlug(course.Slug))
            {
                continue;
            }

            bool courseChanged = false;
            var curated = FindCuratedMetadata(course);
            var isProtectedCurated = curated != null
                && DocumentCourseSyncService.IsProtectedSystemCourse(course)
                && !string.Equals(course.Visibility, "personal", StringComparison.OrdinalIgnoreCase);
            var meta = ResolveMetadata(course);

            if (string.IsNullOrWhiteSpace(course.Category) && !string.IsNullOrWhiteSpace(meta.Category))
            {
                course.Category = meta.Category;
                courseChanged = true;
            }

            if (string.IsNullOrWhiteSpace(course.Level) && !string.IsNullOrWhiteSpace(meta.Level))
            {
                course.Level = meta.Level;
                courseChanged = true;
            }

            var targetOutcomes = isProtectedCurated ? curated!.LearningOutcomes : meta.LearningOutcomes;
            if (targetOutcomes is { Length: > 0 })
            {
                var serializedOutcomes = JsonSerializer.Serialize(targetOutcomes, JsonOptions);
                if (string.IsNullOrWhiteSpace(course.LearningOutcomesJson) ||
                    (isProtectedCurated && !string.Equals(course.LearningOutcomesJson, serializedOutcomes, StringComparison.Ordinal)))
                {
                    course.LearningOutcomesJson = serializedOutcomes;
                    courseChanged = true;
                }
            }

            var targetPrereqs = isProtectedCurated ? curated!.Prerequisites : meta.Prerequisites;
            if (targetPrereqs is { Length: > 0 })
            {
                var serializedPrereqs = JsonSerializer.Serialize(targetPrereqs, JsonOptions);
                if (string.IsNullOrWhiteSpace(course.PrerequisitesJson) ||
                    (isProtectedCurated && !string.Equals(course.PrerequisitesJson, serializedPrereqs, StringComparison.Ordinal)))
                {
                    course.PrerequisitesJson = serializedPrereqs;
                    courseChanged = true;
                }
            }

            if (courseChanged)
            {
                course.UpdatedAt = now;
                modified = true;
            }
        }

        if (modified)
        {
            await db.SaveChangesAsync(ct);
        }
    }
}
