> [!CAUTION]
> Nội dung chỉ mang tính giáo dục, KHÔNG phải lời khuyên đầu tư.
> Mọi quyết định đầu tư là trách nhiệm cá nhân của bạn.
> Hãy tham khảo chuyên gia tài chính phù hợp trước khi đầu tư.

# W05 — Phân tích cơ bản: top-down và bottom-up

## 1. Vì sao cần học
Tuần này nối W01 (tách dữ kiện — giả định — kết luận), W02 (các loại tài sản và rủi ro), W03 (ba báo cáo tài chính) và W04 (các tỷ số tài chính) thành một **quy trình phân tích cơ bản (fundamental analysis) có tầng rõ ràng**: tầng kinh tế vĩ mô, tầng ngành, tầng doanh nghiệp. Có hai hướng tổ chức quy trình: **phân tích từ trên xuống (top-down)** đi từ vĩ mô xuống doanh nghiệp, và **phân tích từ dưới lên (bottom-up)** bắt đầu từ doanh nghiệp rồi tổng hợp ngược lên. Mục tiêu của bài không phải dự báo, mà là biết **mỗi con số thuộc tầng nào, dùng để trả lời câu hỏi nào, và dừng ở đâu khi bằng chứng không đủ**. Bài học chỉ dùng các nguồn giáo dục đã mở trực tiếp trong phiên này: ba bài đọc tham chiếu công khai của CFA Institute (*Equity Valuation: Applications and Processes*, *Industry and Competitive Analysis*, *Company Analysis: Forecasting* — 2026 Curriculum), trang "The Five Forces" của Viện Chiến lược và Cạnh tranh thuộc Harvard Business School (nơi Michael Porter công tác), trang tóm tắt bài Harvard Business Review tháng 01/2008 của chính Porter, giáo trình mở OpenStax *Principles of Macroeconomics 3e* (mục 6.1), bảng dữ liệu "PE Ratio by Sector (US)" tháng 01/2026 của giáo sư Aswath Damodaran (NYU Stern) và trang "What is Risk?" của Investor.gov (U.S. SEC). Đây là nguồn Hoa Kỳ/quốc tế; bối cảnh vĩ mô, ngành và quy định tại Việt Nam chưa được xác minh trong phiên này. Không phần nào là khuyến nghị cá nhân, không tạo dự báo và không yêu cầu dùng tiền thật.

**Kiến thức tiên quyết:** hoàn thành W01, W02, W03 và W04. Cần nhớ từ W04: mỗi tỷ số có tử số, mẫu số, đơn vị và kỳ tính riêng; EPS, P/E, P/B, ROE, ROA, D/E là phép chia từ báo cáo tài chính; mẫu số bằng 0/âm khiến tỷ số "không có ý nghĩa"; P/E thay đổi theo ngành và không có ngưỡng dùng chung.

**Fast-Track Core (30–45 phút):** đọc mục 1–4 (20–25 phút), làm Mini-review 1–2 (5–10 phút), làm quiz 8 câu (8–10 phút), rồi đối chiếu answer key. Phần đào sâu gồm mục "Biết được gì / Chưa thể kết luận gì" và hoạt động tự nguyện.

## 2. Mục tiêu học tập đo được
- [LO-W05-01] Định nghĩa phân tích từ trên xuống (top-down), phân tích từ dưới lên (bottom-up), ba tầng phân tích vĩ mô — ngành — doanh nghiệp, và khung năm lực lượng cạnh tranh (Porter's Five Forces) bằng cách nêu đúng thứ tự các tầng, tên Việt–Anh của cả năm lực lượng, và giới hạn "khung tổ chức câu hỏi, không phải dự báo hay quy tắc sinh lời".
- [LO-W05-02] Áp dụng khung vào bộ dữ liệu giả định bằng cách: phân loại từng động lực doanh thu/biên lợi nhuận vào tầng vĩ mô, tầng ngành hoặc tầng doanh nghiệp; lập bảng chấm định tính năm lực lượng kèm lý do trích từ dữ kiện giả định; và tính lại các tỷ số tầng doanh nghiệp (EPS, P/E, ROE, D/E) của W04 với đơn vị và kỳ tính đầy đủ.
- [LO-W05-03] Phân tích một kịch bản giả định bằng cách tách dòng kết luận vĩ mô → ngành → doanh nghiệp thành các bước có dữ kiện và giả định riêng, đối chiếu kịch bản nghịch, và phân biệt kịch bản (scenario) với dự báo (forecast).
- [LO-W05-04] Phản biện các kết luận kiểu "GDP tăng nên thị trường chắc chắn tăng" hoặc "P/E thấp hơn mặt bằng chung là rẻ" bằng cách chỉ ra phép suy nhân quả thiếu bằng chứng, lỗi so sánh chéo ngành, sự tồn tại của ô NA, và ghi đúng trạng thái "chưa thể kết luận".

## 3. Khái niệm cốt lõi
### 3.1. Hai hướng tổ chức quy trình: từ trên xuống và từ dưới lên
CFA Institute (2026 Curriculum, bài đọc *Equity Valuation: Applications and Processes*) định nghĩa: hướng dự báo từ trên xuống "moves from macroeconomic forecasts to industry forecasts and then to individual company and asset forecasts" — đi từ dự báo kinh tế vĩ mô, đến dự báo ngành, rồi đến dự báo từng doanh nghiệp và tài sản cụ thể; hướng từ dưới lên "aggregates individual company forecasts to industry forecasts, which in turn may be aggregated to macroeconomic forecasts" — tổng hợp từ dự báo từng doanh nghiệp lên ngành, rồi có thể lên tiếp vĩ mô. [W05-C01]

Ba điểm cần đọc đúng:
- Đây là **hướng tổ chức dự báo/phân tích**, không phải quy tắc sinh lời. Trong quy trình định giá năm bước của cùng bài đọc CFA, bước đầu là "understanding the business" — hiểu triển vọng ngành, vị thế cạnh tranh và chiến lược doanh nghiệp; top-down/bottom-up chỉ là cách sắp thứ tự các bước dự báo phía sau.
- Hai hướng **không loại trừ nhau**. Bài đọc CFA *Company Analysis: Forecasting* nêu rõ: doanh thu có thể dự báo theo đối tượng từ trên xuống (tăng trưởng tương đối so với GDP, tăng trưởng thị trường và thị phần) hoặc từ dưới lên (sản lượng và giá bán bình quân, doanh thu theo dòng sản phẩm/khu vực/công đoạn), và việc dùng nhiều đối tượng dự báo giúp "uncovering implicit assumptions or errors in any single approach" — phát hiện giả định ngầm hoặc lỗi của một hướng đơn lẻ. [W05-C01]
- Giới hạn bằng chứng: định nghĩa trên là nội dung giáo dục của CFA Institute; nguồn không khẳng định hướng nào "thắng" hướng nào và không gắn với thị trường nào cụ thể.

### 3.2. Tầng vĩ mô: GDP đo sản lượng, không phải cái máy dự báo giá cổ phiếu
**Tổng sản phẩm quốc nội (Gross Domestic Product — GDP)** theo giáo trình mở OpenStax *Principles of Macroeconomics 3e* (mục 6.1): "the value of all final goods and services produced within a country in a given year" — giá trị của mọi hàng hóa và dịch vụ **cuối cùng** được sản xuất **trong phạm vi một quốc gia** trong **một năm**; đo theo bốn cấu phần cầu: tiêu dùng (consumption), đầu tư (investment), chi tiêu chính phủ (government) và xuất khẩu ròng (net exports). Ví dụ của OpenStax: GDP Hoa Kỳ năm 2020 là 20,9 nghìn tỷ USD (nguồn BEA). GDP là thước đo **sản lượng sản xuất**, đơn vị là giá trị tiền tệ cho cả một kỳ. [W05-C02]

Hai giới hạn khi dùng số vĩ mô:
- **GDP không phải công cụ dự báo giá cổ phiếu.** Bài học này **không** khẳng định và không nguồn nào đã mở **chứng minh** rằng tăng trưởng GDP "gây ra" tăng giá cổ phiếu. GDP trả lời câu hỏi "nền kinh tế sản xuất được bao nhiêu giá trị", không trả lời "chỉ số giá sẽ đi đâu". Khi một năm nào đó GDP cao và giá cổ phiếu cùng lên (hoặc cùng xuống), đó chỉ là tương quan quan sát được — tương quan không tự động là quan hệ nhân quả và không cho phép suy ra kỳ sau (ví dụ logic ở 4.4 minh họa vì sao phép suy "GDP tăng → thị trường chắc chắn tăng" không phải quy luật). [W05-C02]
- Số vĩ mô hữu ích ở bước **đặt câu hỏi theo độ nhạy của ngành**. Bài đọc CFA *Industry and Competitive Analysis* chỉ rõ cách dùng hợp lệ: so sánh metrics ngành với xu hướng kinh tế rộng trong giai đoạn suy thoái/phục hồi để đánh giá "the industry's level of maturity, sensitivity to business cycles, and competitive rivalry" — mức trưởng thành, **độ nhạy chu kỳ (sensitivity to business cycles)** và mức độ cạnh tranh của ngành. Nghĩa là số vĩ mô giúp phân loại "ngành này nhạy với chu kỳ, với lãi suất thế nào", không giúp dự đoán điểm giá.

Bối cảnh rủi ro theo SEC (Investor.gov, trang "What is Risk?"): "All investments involve some degree of risk"; giá cổ phiếu có thể chịu tác động của yếu tố **bên trong** doanh nghiệp (ví dụ sản phẩm lỗi) lẫn yếu tố doanh nghiệp **không kiểm soát** (sự kiện chính trị hoặc thị trường). SEC nêu điều này như một khẳng định về **bất định**, không phải mô hình định hướng giá.

### 3.3. Tầng ngành: định nghĩa ranh giới, khảo sát và đối chiếu chu kỳ
Bài đọc CFA *Industry and Competitive Analysis* mô tả ba việc nền của phân tích ngành: (1) **định nghĩa ranh giới ngành** — ngành thường được hiểu là các công ty bán sản phẩm/dịch vụ tương tự, nhưng ranh giới thực tế mơ hồ vì một công ty có thể bán nhiều dòng sản phẩm; (2) **khảo sát ngành** — ước tính tổng doanh thu năm, tốc độ tăng trưởng lịch sử, metrics sinh lời, rồi xác định thị phần và xu hướng của các người chơi chính; (3) **đối chiếu metrics ngành với xu hướng kinh tế rộng** trong suy thoái/phục hồi để đánh giá độ trưởng thành, độ nhạy chu kỳ và mức cạnh tranh (như 3.2). Cùng bài đọc này xác nhận nhà phân tích dùng khung năm lực lượng để hiểu cấu trúc ngành, và dùng **phân tích PESTLE** (chính trị — kinh tế — xã hội — công nghệ — pháp lý — môi trường) như công cụ bổ trợ đọc ảnh hưởng bên ngoài lên tăng trưởng và thị phần ngành. [W05-C02]

### 3.4. Năm lực lượng cạnh tranh (Porter's Five Forces)
Khung được **Michael Porter** trình bày lần đầu trong bài *Harvard Business Review (HBR)* năm 1979. Trang "The Five Forces" của **Viện Chiến lược và Cạnh tranh (Institute for Strategy and Competitiveness)** thuộc Harvard Business School mô tả: "The Five Forces is a framework for understanding the competitive forces at work in an industry, and which drive the way economic value is divided among industry actors" — khung đọc các lực lượng cạnh tranh trong ngành và cách giá trị kinh tế được chia giữa các bên trong ngành. Bài HBR tháng 01/2008 của chính Porter viết: cạnh tranh vì lợi nhuận không chỉ nằm ở các đối thủ hiện tại mà gồm cả bốn lực lượng khác — khách hàng, nhà cung cấp, đối thủ tiềm ẩn gia nhập và sản phẩm thay thế; "extended rivalry" (cạnh tranh mở rộng) từ cả năm lực lượng xác định **cấu trúc ngành**. [W05-C03]

Năm lực lượng (tên Việt–Anh và cơ chế theo trang HBS):
1. **Mối đe dọa từ đối thủ mới gia nhập (threat of new entrants):** việc gia nhập kéo thêm năng lực và sức ép giá — "puts a cap on the profit potential of an industry" (đặt trần cho tiềm năng lợi nhuận của ngành); mức đe dọa phụ thuộc rào cản: kinh tế theo quy mô, chi phí xây nhận diện thương hiệu, tiếp cận kênh phân phối, hạn chế từ chính phủ.
2. **Quyền thương lượng của nhà cung cấp (bargaining power of suppliers):** nhà cung cấp mạnh có thể ép giá đầu vào cao hơn hoặc điều kiện thuận hơn, hạ lợi nhuận ngành; mạnh khi chỉ có một hai nguồn cung thiết yếu hoặc chi phí đổi nhà cung cấp lớn.
3. **Quyền thương lượng của khách hàng (bargaining power of buyers):** khách hàng mạnh có thể ép giá xuống hoặc đòi thêm dịch vụ cùng mức giá; mạnh nhất khi khách lớn so với người bán, sản phẩm ít khác biệt hóa, chi phí chuyển đổi thấp.
4. **Mối đe dọa từ sản phẩm thay thế (threat of substitute products or services):** sản phẩm/dịch vụ đáp ứng cùng nhu cầu theo cách khác (ví dụ của HBS: họp trực tuyến thay cho đi lại; email thay cho chuyển phát nhanh); đe dọa cao khi đổi price–performance hấp dẫn và chi phí chuyển đổi thấp.
5. **Cạnh tranh nội ngành (rivalry among existing competitors):** rivalry gay gắt "drives down prices or dissipates profits by raising the cost of competing" (đè giá xuống hoặc xói mòn lợi nhuận bằng chi phí cạnh tranh); các dữ kiện khiến rivalry dữ dội gồm: đối thủ nhiều hoặc cỡ tương đương, tăng trưởng ngành chậm, chi phí cố định cao (kích thích giảm giá), rào cản thoát lui cao, cam kết với ngành mạnh, mục tiêu/cách cạnh tranh khác nhau. [W05-C03]

Giới hạn khi chấm khung: kết quả là **định tính phụ thuộc dữ kiện đưa vào** — cùng một ngành, hai người với bộ dữ kiện khác nhau có thể chấm khác nhau; kết quả là đầu vào để đặt câu hỏi, không phải "điểm hấp dẫn ngành" khách quan, và không phải dự báo giá. [W05-C03]

### 3.5. Tầng doanh nghiệp: dùng lại tỷ số của W04 như đầu vào, không phải kết luận
Tầng doanh nghiệp trong hướng từ dưới lên làm việc trực tiếp với nền tảng tài chính đã học ở W03–W04: thu nhập trên mỗi cổ phần (earnings per share — EPS) đo bằng VND/cổ phần cho một kỳ; hệ số giá–thu nhập (price-to-earnings — P/E) đo bằng "lần"; tỷ suất sinh lời trên vốn chủ sở hữu (return on equity — ROE) đo bằng %; tỷ lệ nợ trên vốn chủ sở hữu (debt-to-equity — D/E) đo bằng "lần". Cùng bài đọc CFA *Equity Valuation* định nghĩa giá trị nội tại (intrinsic value) là giá trị tài sản "given a hypothetically complete understanding of the asset's investment characteristics" — trong giả định hiểu đầy đủ hoàn hảo đặc tính đầu tư; vì không ai có sự hiểu đầy đủ đó, mọi con số định giá thực tế chỉ là **ước lượng theo giả định**, tiếp nối cảnh báo của W04: P/E thay đổi theo ngành, theo loại trailing/forward, và các ngành phần lớn công ty lỗ không tính được P/E. [W05-C01][W05-C04]

### 3.6. P/E không có ngưỡng "đắt/rẻ" phổ quát
Dữ liệu thực đã mở (Damodaran, "PE Ratio by Sector (US)", dữ liệu tháng 01/2026): hệ số Current P/E của nhóm Broadcasting (phát thanh – truyền hình) là **6,38 lần** (24 công ty; 70,83% đang lỗ), của nhóm Drugs (Pharmaceutical) (dược phẩm) là **335,27 lần** (228 công ty; 84,65% đang lỗ); nhóm Chemical (Diversified) (hóa chất đa ngành) có **100%** công ty lỗ nên trailing P/E ghi **NA**. Chênh lệch hàng chục lần giữa các ngành và sự tồn tại của NA cho thấy không thể áp một ngưỡng P/E chung cho mọi ngành; đồng thời đây là dữ liệu **thị trường Hoa Kỳ tại một thời điểm**, không phải chuẩn so sánh cho mọi thị trường. Kết nối W04: ngay trong cùng ngành cũng phải phân biệt trailing/forward và loại khoản bất thường. [W05-C04]

### 3.7. Tách dữ kiện — giả định — diễn giải; kịch bản khác dự báo
Mỗi câu trong phần ví dụ dưới đây được gắn nhãn: **dữ kiện** (có nguồn mở hoặc dữ kiện giả định được khai báo), **giả định** (con số tự đặt để minh họa), **diễn giải** (cách đọc mang tính phỏng đoán chờ kiểm chứng). Một **kịch bản (scenario)** là một bộ giả định có cấu trúc dùng để kiểm tra độ nhạy; một **dự báo (forecast)** là khẳng định về kết quả tương lai kèm mô hình và xác suất. Kịch bản không tự động thành dự báo, và cả hai đều khác dữ kiện.

## 4. Ví dụ và worked examples (giả định, đơn vị VND / %)
### 4.1. Dữ liệu nền — Quốc X và Ngành F (giả định hoàn toàn)
**Giả định của người viết:** một quốc gia hư cấu "Quốc X" và một ngành hư cấu "Ngành F — thực phẩm đóng gói" với các số vĩ mô giả định cho kỳ 12 tháng 2026:

| Chỉ số | Giá trị giả định | Đơn vị | Kỳ | Câu hỏi hệ quả (không kết luận) |
|---|---:|---|---|---|
| GDP | +6,5 | %/năm | 12 tháng 2026 | Ngành F bán hàng tiêu dùng thiết yếu: tăng trưởng này đến từ dân số, thu nhập bình quân hay giá bán? Độ nhạy chu kỳ của ngành ra sao? |
| CPI (chỉ số giá tiêu dùng) | +4,0 | %/năm | 12 tháng 2026 | Chi phí đầu vào và quyền chuyển giá (pricing power) của doanh nghiệp trong ngành thay đổi thế nào? |
| Lãi suất chính sách | 5,0 | %/năm | tại 31/12/2026 | Doanh nghiệp dùng nhiều nợ (D/E cao theo W04) chịu chi phí vốn bao nhiêu? |

Cả ba con số đều là giả định minh họa. GDP ở đây chỉ được dùng đúng định nghĩa của OpenStax: thước đo sản lượng năm — **không** suy ra hướng thị trường chứng khoán từ dòng nào trong bảng. [W05-C02][W05-C05]

### 4.2. Chấm định tính năm lực lượng cho Ngành F (giả định)
**Dữ kiện giả định của ngành F:** công thức sản phẩm đơn giản, kênh phân phối phổ thông; nguyên liệu chính từ 2 nhà cung cấp lớn, đổi nguồn mất 6 tháng; 60% doanh số đến từ 5 chuỗi bán lẻ lớn; phương án thay thế tại gia đình chỉ rẻ hơn 5% nhưng chất lượng kém rõ rệt; 12 công ty cỡ tương đương, tăng trưởng ngành 2%/năm, chi phí cố định cao.

| Lực lượng | Dữ kiện giả định dẫn chiếu | Đọc theo mô tả HBS | Mức áp lực (định tính) |
|---|---|---|---|
| Đối thủ mới gia nhập | Công thức đơn giản, kênh phân phối phổ thông | Rào cản quy mô/thương hiệu/kênh phân phối thấp | Cao |
| Nhà cung cấp | 2 nguồn cung thiết yếu; đổi nguồn 6 tháng | Nguồn cung ít, chi phí chuyển đổi cao → nhà cung cấp mạnh | Cao |
| Khách hàng | 60% doanh số từ 5 chuỗi lớn | Khách lớn so với người bán → quyền thương lượng mạnh | Cao |
| Sản phẩm thay thế | Thay thế rẻ hơn 5% nhưng kém chất lượng rõ | Price–performance chưa hấp dẫn, không khuyến khích chuyển đổi | Trung bình |
| Cạnh tranh nội ngành | 12 công ty tương đương; tăng trưởng 2%/năm; chi phí cố định cao | Đúng nhóm dữ kiện "rivalry gay gắt" của HBS | Cao |

**Diễn giải (đánh dấu rõ):** 4/5 lực lượng áp lực cao → **giả thuyết làm việc** là "cấu trúc cạnh tranh đè biên lợi nhuận ngành F". Đây là diễn giải chờ kiểm chứng bằng dữ liệu biên lợi nhuận ngành thật, không phải kết luận đã chứng minh, và không nói gì về giá cổ phiếu. [W05-C03][W05-C05]

### 4.3. Tầng doanh nghiệp: đọc lại "Công ty M" của W03–W04 (giả định)
**Giả định kế thừa từ W04** (số liệu hư cấu, đơn vị VND): lợi nhuận sau thuế 2.625.000.000 VND cho kỳ 12 tháng 2026; số cổ phiếu phổ thông bình quân gia quyền 10.000.000; giá tại 31/12/2026 là 5.250 VND/cổ phần; vốn chủ sở hữu bình quân 3.500.000.000 VND; tổng nợ phải trả 6.000.000.000 VND và vốn chủ 4.000.000.000 VND tại 31/12/2026.

`EPS = 2.625.000.000 / 10.000.000 = 262,5 VND/cổ phần (kỳ 12 tháng 2026)`

`P/E = 5.250 / 262,5 = 20,0 lần (giá tại một ngày chia EPS cả năm)`

`ROE = 2.625.000.000 / 3.500.000.000 = 75,0% (kỳ 12 tháng 2026; mẫu số bình quân)`

`D/E = 6.000.000.000 / 4.000.000.000 = 1,5 lần (tại 31/12/2026)`

Đọc theo tầng: bốn con số này thuộc **tầng doanh nghiệp** — đầu vào của hướng từ dưới lên; chúng không nói gì trực tiếp về vĩ mô hay cả ngành. Nhắc kết quả DuPont của W04: ROE 75,0% có phần lớn từ hệ số nhân vốn chủ ~2,86 lần (đòn bẩy) — câu hỏi bottom-up đúng là "chất lượng lợi nhuận thế nào?", không phải "ROE cao nên tốt". [W05-C01][W05-C05]

### 4.4. Minh họa logic: "GDP tăng nên thị trường chắc chắn tăng" sai ở đâu
**Dựng hai kịch bản giả định cho Quốc X, cùng GDP +6,5%/năm (12 tháng 2026):**

| Kịch bản | GDP (giả định) | Chỉ số giá thị trường Quốc X (giả định) | Mâu thuẫn với định nghĩa GDP? |
|---|---:|---:|---|
| A | +6,5%/năm | +10% trong cùng năm | Không |
| B | +6,5%/năm | −5% trong cùng năm | Không |

Cả hai kịch bản đều **không mâu thuẫn** với định nghĩa GDP của OpenStax, vì GDP đo giá trị sản lượng sản xuất trong năm, không đo giá thị trường chứng khoán. Mối quan hệ giữa tăng trưởng sản lượng và lợi nhuận nhà đầu tư đi qua nhiều kênh (lợi nhuận doanh nghiệp, lãi suất, mức định giá, cung–cầu cổ phiếu) và không đơn hướng; vì vậy từ "GDP tăng 6,5%/năm" kết luận đúng là **"chưa thể kết luận"** về hướng thị trường — cần dữ kiện riêng cho từng kênh. Đánh dấu: đây là minh họa logic trên số giả định, không phải mô phỏng thị trường của quốc gia nào. [W05-C02][W05-C05]

### 4.5. Dữ liệu thật đã mở: P/E theo ngành (Damodaran 01/2026, Hoa Kỳ)
**Dữ kiện từ nguồn đã mở** (dữ liệu as of January 2026; thị trường Hoa Kỳ):

| Nhóm ngành | Số công ty | % công ty lỗ | Current P/E | Trailing P/E |
|---|---:|---:|---:|---:|
| Broadcasting | 24 | 70,83% | 6,38 lần | 44,76 lần |
| Drugs (Pharmaceutical) | 228 | 84,65% | 335,27 lần | 55,67 lần |
| Chemical (Diversified) | 4 | 100,00% | 20,55 lần | NA |

**Diễn giải có kiểm soát:** trải rộng ~6,38 → 335,27 lần cho thấy mức P/E gắn chặt với ngành, loại P/E và cơ cấu công ty lỗ/lãi của ngành; ô NA xảy ra khi tử số lợi nhuận của ngành không có nghĩa so sánh (nhớ W04-C03). Những con số này **không** dùng làm ngưỡng mua/bán và **không** suy ra cho bất kỳ thị trường nào khác. [W05-C04]

### 4.6. Phân loại bằng chứng
- **Kết quả/nguồn (đã mở trực tiếp):** CFA *Equity Valuation: Applications and Processes* cho định nghĩa top-down/bottom-up, quy trình định giá năm bước, intrinsic value; CFA *Industry and Competitive Analysis* cho ba việc nền của phân tích ngành, cách dùng năm lực lượng và PESTLE, phân loại động lực vĩ mô/ngành/doanh nghiệp; CFA *Company Analysis: Forecasting* cho các động lực doanh thu từ trên xuống (so với GDP, thị trường, thị phần) và từ dưới lên (sản lượng, giá bán bình quân); HBS ISC và HBR 01/2008 cho cơ chế năm lực lượng; OpenStax Macro 3e 6.1 cho định nghĩa GDP và cấu phần C+I+G+NX; Damodaran 01/2026 cho P/E theo ngành và cách ghi NA; Investor.gov "What is Risk?" cho cảnh báo rủi ro chung.
- **Giả định minh họa:** Quốc X, Ngành F, toàn bộ số trong 4.1–4.4 (GDP +6,5%/năm, CPI +4,0%/năm, lãi suất 5,0%/năm, tăng trưởng ngành 2%/năm, 12 công ty, 60%/5 chuỗi, 2 nhà cung cấp, 6 tháng đổi nguồn, rẻ hơn 5%); bộ số Công ty M kế thừa W04; hai kịch bản chỉ số +10%/−5%; tỷ lệ "4/5 lực lượng cao".
- **Diễn giải thực hành:** các ví dụ dùng để luyện quy trình tầng — ngành — doanh nghiệp và cách ghi "chưa thể kết luận"; không phải phân tích ngành/doanh nghiệp thật, không phải khuyến nghị giao dịch.

## 5. Mini-review và truy xuất kiến thức
### Mini-review 1: phân loại lực lượng và tầng dữ liệu
Không nhìn phần trên: Dữ kiện giả định của "Ngành G": chỉ 3 nhà cung cấp chip cho toàn ngành; chi phí chip chiếm 45% giá vốn của một doanh nghiệp trong ngành; khách hàng phân tán nhỏ lẻ; sản phẩm khó thay thế. (a) Lực lượng nào đang gây áp lực cao nhất và vì sao theo mô tả HBS? (b) Dữ kiện "45% giá vốn" thuộc tầng nào? **Tự kiểm:** (a) quyền thương lượng của nhà cung cấp — nguồn cung thiết yếu ít và chi phí đổi nguồn cao (HBS: "If there are only one or two suppliers of an essential input product... a supplier group wields more power"); (b) tầng doanh nghiệp — cấu trúc giá vốn của một doanh nghiệp cụ thể; còn khung "quyền thương lượng nhà cung cấp" là công cụ ở tầng ngành.

### Mini-review 2: kịch bản, dự báo và "chưa thể kết luận"
Một tài liệu giả định viết: "GDP Quốc X năm tới sẽ tăng 8%/năm, nên thị trường cổ phiếu Quốc X chắc chắn tăng ít nhất 20%." Liệt kê ít nhất ba lỗi phân tích. **Tự kiểm:** (1) GDP là thước đo sản lượng (OpenStax 6.1) — nguồn đã mở không chứng minh quan hệ nhân quả GDP → giá thị trường, nên phép suy là nhảy cót kết luận; (2) "chắc chắn tăng" biến kịch bản thành dự báo chắc chắn mà không có mô hình, phân phối kịch bản hay xác suất; (3) "ít nhất 20%" là con số không có dữ kiện hay mô hình nào đi kèm; (4) nếu nói "nhìn quá khứ thấy cùng tăng" thì đó là tương quan — tương quan khác nhân quả và quá khứ không tự động lặp. Kết luận đúng: **chưa thể kết luận** từ dữ kiện đã đưa.

## 6. Cảnh báo & sai lầm thường gặp
- Dùng **GDP để suy ra giá cổ phiếu**: GDP đo sản lượng (OpenStax 6.1), không phải công cụ dự báo chỉ số; tương quan quá khứ không phải nhân quả và không tự động lặp.
- Coi **P/E thấp hơn mặt bằng là "rẻ"**: ngưỡng phổ quát không tồn tại; P/E trải rộng theo ngành (Damodaran 01/2026: 6,38 so với 335,27 lần — Hoa Kỳ, một thời điểm), theo loại trailing/forward, và có ngành không tính được (NA).
- **Nhầm chấm định tính thành đo đạc khách quan**: bảng năm lực lượng phụ thuộc dữ kiện đưa vào; cùng ngành có thể chấm khác nhau nếu dữ kiện khác.
- **Biến kịch bản thành dự báo chắc chắn**: kịch bản là bộ giả định để kiểm tra độ nhạy; dự báo cần mô hình và xác suất; cả hai đều khác dữ kiện.
- **Bottom-up "mù vĩ mô"**: theo CFA, hướng từ dưới lên vẫn **tổng hợp ngược** lên ngành/vĩ mô để đối chiếu — bỏ hoàn toàn bối cảnh là đọc sai quy trình, không phải "thuần bottom-up".
- **So doanh thu tăng của một công ty với "ngành phục hồi" khi chưa tách tầng**: tăng trưởng phải phân loại vĩ mô/ngành/doanh nghiệp (CFA) trước khi gán nguyên nhân.
- Chỉ đọc một nguồn rồi chốt "ngành hấp dẫn": thiếu đối chứng (thiên kiến xác nhận), anchoring vào một con số vĩ mô cũ, overconfidence (nhầm hiểu khung với khả năng dự đoán tương lai).
- Ghi thiếu **nguồn, thời điểm, đơn vị, kỳ** của từng dữ kiện vĩ mô hoặc doanh nghiệp.

## 7. Biết được gì / Chưa thể kết luận gì
**Biết được:** định nghĩa top-down và bottom-up theo CFA và thứ tự các tầng; định nghĩa GDP theo OpenStax (đơn vị tiền tệ cho một năm; cấu phần tiêu dùng, đầu tư, chi tiêu chính phủ, xuất khẩu ròng) và giới hạn "thước đo sản lượng"; cách CFA dùng số vĩ mô để phân loại độ trưởng thành, độ nhạy chu kỳ và mức cạnh tranh của ngành; ba việc nền của phân tích ngành (định nghĩa ranh giới, khảo sát doanh thu–tăng trưởng–sinh lời–thị phần, đối chiếu chu kỳ); năm lực lượng Porter với cơ chế từng lực lượng theo HBS/HBR và cách chấm định tính kèm lý do; các động lực doanh thu từ trên xuống và từ dưới lên; P/E trải rộng theo ngành và có NA; cách tách dữ kiện/giả định/diễn giải và viết "chưa thể kết luận".

**Chưa thể kết luận:** "GDP tăng (hoặc giảm) X% nên thị trường tăng/giảm Y%" — không nguồn nào đã mở chứng minh quan hệ nhân quả này; "ngành hấp dẫn/không hấp dẫn" tuyệt đối chỉ từ một bảng chấm năm lực lượng; mức P/E "hợp lý" dùng chung cho mọi ngành/mọi thị trường; bất kỳ kết luận nào áp thẳng dữ liệu Hoa Kỳ (Damodaran, CFA, OpenStax) vào thị trường hoặc ngành Việt Nam — bối cảnh vĩ mô, ngành và quy định Việt Nam chưa được xác minh trong phiên này; lợi nhuận hoặc giá tương lai — bài học không tạo dự báo nào. Với bằng chứng chỉ là tương quan, kết luận đúng là "chưa thể kết luận".

## 8. Checklist thực hành
- [ ] Viết mục tiêu, thời hạn và giới hạn rủi ro.
- [ ] Ghi nguồn, thời điểm, đơn vị và trạng thái xác minh.
- [ ] Tách dữ kiện, giả định, phép tính và kết luận.
- [ ] Kiểm tra kịch bản xấu, thanh khoản, phí và thuế.
- [ ] Không dùng tiền thật chỉ vì hoàn thành bài.

## 9. Hoạt động áp dụng tự nguyện (giả định)
Dùng **chỉ dữ liệu giả định**: tự chọn một "ngành tưởng tượng" (không dùng ngành thật). (1) Viết bộ số vĩ mô giả định ba dòng (GDP, CPI, lãi suất — đơn vị %/năm hoặc %/quý, kỳ rõ) và chuyển mỗi dòng thành một câu hỏi cho ngành theo cách 4.1. (2) Lập bảng chấm định tính năm lực lượng với ít nhất một dữ kiện giả định mỗi lực lượng và một dòng "Diễn giải" như 4.2. (3) Tạo bộ chỉ số doanh nghiệp giả định (EPS, P/E, ROE, D/E theo công thức W04) và ghi rõ mỗi con số thuộc tầng nào. (4) Viết một đoạn "GDP Quốc X tăng 7%/năm nên..." rồi tự phản biện bằng checklist 4.4. (5) Kết thúc bằng ba câu "chưa thể kết luận". Không tra cứu hay nhập dữ liệu thị trường thật, không dùng tiền thật.

## 10. Glossary
| Việt | English | Định nghĩa |
|---|---|---|
| Phân tích từ trên xuống | Top-down (forecasting approach) | Hướng dự báo/phân tích đi từ dự báo kinh tế vĩ mô → ngành → từng doanh nghiệp và tài sản (CFA 2026, Equity Valuation: Applications and Processes). |
| Phân tích từ dưới lên | Bottom-up (forecasting approach) | Hướng tổng hợp từ dự báo từng doanh nghiệp → ngành → vĩ mô (CFA, cùng nguồn); tập trung chất lượng doanh nghiệp nhưng vẫn đối chiếu bối cảnh khi tổng hợp. |
| Phân tích cơ bản | Fundamental Analysis | Nhóm phương pháp đọc giá trị/khả năng của tài sản từ nền tảng kinh tế — ngành — báo cáo tài chính, thay vì từ biến động giá đã qua. |
| Tổng sản phẩm quốc nội | Gross Domestic Product (GDP) | Giá trị mọi hàng hóa và dịch vụ cuối cùng được sản xuất trong phạm vi một quốc gia trong một năm (OpenStax Macro 3e, 6.1); gồm tiêu dùng, đầu tư, chi tiêu chính phủ và xuất khẩu ròng; đo sản lượng, không đo giá thị trường chứng khoán. |
| Chỉ số giá tiêu dùng | Consumer Price Index (CPI) | Chỉ số theo dõi mặt bằng giá giỏ hàng tiêu dùng theo thời gian; số vĩ mô đo theo kỳ, dùng để đặt câu hỏi chi phí–giá bán, không phải máy dự báo giá cổ phiếu. |
| Độ nhạy chu kỳ | Sensitivity to Business Cycles | Mức doanh thu/sinh lời của ngành thay đổi theo giai đoạn suy thoái/phục hồi kinh tế — thuộc bước đối chiếu metrics ngành với xu hướng kinh tế rộng (CFA Industry and Competitive Analysis). |
| Năm lực lượng cạnh tranh | Porter's Five Forces | Khung đọc cấu trúc cạnh tranh ngành gồm: đe dọa gia nhập, quyền thương lượng nhà cung cấp, quyền thương lượng khách hàng, sản phẩm thay thế, cạnh tranh nội ngành (Porter 1979; HBS ISC). |
| Mối đe dọa từ đối thủ mới gia nhập | Threat of New Entrants | Sức ép từ khả năng người mới vào ngành kéo thêm năng lực và sức ép giá; "đặt trần tiềm năng lợi nhuận ngành", phụ thuộc rào cản gia nhập (HBS ISC). |
| Quyền thương lượng của nhà cung cấp | Bargaining Power of Suppliers | Khả năng nhà cung cấp ép giá đầu vào/điều kiện thuận hơn, hạ lợi nhuận ngành; mạnh khi nguồn cung thiết yếu ít và chi phí đổi nguồn cao (HBS ISC). |
| Quyền thương lượng của khách hàng | Bargaining Power of Buyers | Khả năng khách hàng ép giá xuống hoặc đòi thêm dịch vụ; mạnh nhất khi khách lớn, sản phẩm ít khác biệt hóa, chi phí chuyển đổi thấp (HBS ISC). |
| Mối đe dọa từ sản phẩm thay thế | Threat of Substitute Products or Services | Sản phẩm/dịch vụ đáp ứng cùng nhu cầu theo cách khác; đe dọa cao khi đổi price–performance hấp dẫn và chi phí chuyển đổi thấp (HBS ISC). |
| Cạnh tranh nội ngành | Rivalry Among Existing Competitors | Mức độ đối đầu giữa các người chơi hiện tại; gay gắt khi đối thủ nhiều/tương đương, tăng trưởng ngành chậm, chi phí cố định cao, rào cản thoát lui cao (HBS ISC). |
| Phân tích PESTLE | PESTLE Analysis | Khung bổ trợ đọc ảnh hưởng bên ngoài (chính trị, kinh tế, xã hội, công nghệ, pháp lý, môi trường) lên tăng trưởng và thị phần ngành (CFA Industry and Competitive Analysis). |
| Giá trị nội tại | Intrinsic Value | Giá trị tài sản trong giả định "hiểu đầy đủ hoàn hảo" đặc tính đầu tư; trong thực tế chỉ ước lượng được theo giả định (CFA 2026). |
| Kịch bản | Scenario | Bộ giả định có cấu trúc dùng để kiểm tra độ nhạy của kết luận với thay đổi giả định; khác dữ kiện và khác dự báo. |
| Chưa thể kết luận | Not yet concluded | Trạng thái khi bằng chứng không đủ hoặc chỉ là tương quan; cách ghi chuẩn thay cho mọi kết luận mua/bán/dự báo trong bài học này. |

## 11. Tài liệu tham khảo đã mở
- CFA Institute, "Equity Valuation: Applications and Processes" (2026 Curriculum, refresher reading, Level II Equity Valuation): https://www.cfainstitute.org/insights/professional-learning/refresher-readings/2026/equity-valuation-applications-and-processes (truy cập 2026-09-15).
- CFA Institute, "Industry and Competitive Analysis" (2026 Curriculum, refresher reading, Level I): https://www.cfainstitute.org/insights/professional-learning/refresher-readings/2026/industry-and-competitive-analysis (truy cập 2026-09-15).
- CFA Institute, "Company Analysis: Forecasting" (2026 Curriculum, refresher reading, Level I): https://www.cfainstitute.org/insights/professional-learning/refresher-readings/2026/company-analysis-forecasting (truy cập 2026-09-15).
- Institute for Strategy and Competitiveness (Harvard Business School), "The Five Forces" — mô tả khung do Michael Porter khởi xướng: https://www.isc.hbs.edu/strategy/business-strategy/Pages/the-five-forces.aspx (truy cập 2026-09-15).
- Michael E. Porter, "The Five Competitive Forces That Shape Strategy", Harvard Business Review, tháng 01/2008 (trang tóm tắt): https://hbr.org/2008/01/the-five-competitive-forces-that-shape-strategy (truy cập 2026-09-15).
- OpenStax, *Principles of Macroeconomics 3e*, mục 6.1 "Measuring the Size of the Economy: Gross Domestic Product": https://openstax.org/books/principles-macroeconomics-3e/pages/6-1-measuring-the-size-of-the-economy-gross-domestic-product (truy cập 2026-09-15).
- Aswath Damodaran (NYU Stern), "PE Ratio by Sector (US)", dữ liệu tháng 01/2026: https://pages.stern.nyu.edu/~adamodar/New_Home_Page/datafile/pedata.html (truy cập 2026-09-15).
- U.S. SEC — Investor.gov, "What is Risk?": https://www.investor.gov/introduction-investing/investing-basics/what-risk (truy cập 2026-09-15).
- Source ledger: [quality/module-2-source-ledger.md](../../quality/module-2-source-ledger.md).

## 12. Next steps
- [Quiz W05](../../assessments/quizzes/week-05-quiz.md)
- [Answer key W05](../../assessments/answer-keys/week-05-answers.md)
- Sau khi nắm W05, W06 (phân tích kỹ thuật) sẽ tiếp cận giá theo hướng khác hẳn: đọc hành vi giá/khối lượng đã quan sát thay vì nền tảng kinh tế — hai bộ công cụ không thay thế nhau và đều không dự báo chắc chắn.

## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

**Bài:** Top-down và bottom-up. **Đầu ra:** Viết một trang phân tích Quốc X/Ngành F/Công ty M ở mục4.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| Vĩ mô | Ghi kỳ/đơn vị, không suy GDP tăng→giá chắc tăng | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Ngành | Áp dụng năm lực lượng bằng dữ kiện và giả định tách riêng | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Doanh nghiệp | Dùng tỷ số W04 làm đầu vào, không thành verdict | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Kịch bản | Viết hai kịch bản khác nhau và dữ kiện có thể phân biệt chúng | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Kết luận | Giữ trạng thái chưa thể kết luận khi thiếu dữ liệu; không dùng ngưỡng P/E phổ quát | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
