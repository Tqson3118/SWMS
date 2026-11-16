> [!CAUTION]
> Nội dung chỉ mang tính giáo dục, KHÔNG phải lời khuyên đầu tư.
> Mọi quyết định đầu tư là trách nhiệm cá nhân của bạn.
> Hãy tham khảo chuyên gia tài chính phù hợp trước khi đầu tư.

# W06 — Phân tích kỹ thuật cơ bản: trendline, support/resistance, moving average

## 1. Vì sao cần học
Tuần này nối W01 (tách dữ kiện — giả định — kết luận), W02 (rủi ro và các loại tài sản), W03–W04 (báo cáo tài chính và tỷ số) và W05 (phân tích cơ bản top-down/bottom-up) với một cách nhìn giá khác hẳn: **phân tích kỹ thuật (technical analysis)** đọc hành vi **giá và khối lượng đã quan sát** thay vì đọc nền tảng kinh tế — ngành — doanh nghiệp. Mục tiêu của bài không phải học "mẹo bắt đỉnh đáy", mà là hiểu các công cụ kỹ thuật cơ bản là gì, chúng **mô tả** quá khứ và **tạo giả thuyết** về xu hướng thế nào, vì sao chúng **không chứng minh** giá tương lai, và những lỗi nghiên cứu nào (look-ahead bias, data-snooping/cherry-pick, survivorship bias, bỏ qua phí và trượt giá) khiến một biểu đồ "đẹp" trở thành bằng chứng kém. Bài học chỉ dùng các nguồn giáo dục đã mở trực tiếp trong phiên này: hai bài đọc tham chiếu công khai của CFA Institute 2026 Curriculum (*Equity Valuation: Concepts and Basic Tools* và *Market Efficiency*), ba nghiên cứu học thuật mở trang tóm tắt trên IDEAS/RePEc (Sullivan–Timmermann–White 1999; Brown–Goetzmann–Ibbotson–Ross 1992; Blume–Easley–O'Hara 1994), bản làm việc của Sullivan–Timmermann–White trên kho LSE Research Online, tài liệu "Trading Basics" của U.S. SEC và từ điển thuật ngữ Investor.gov. Đây là nguồn Hoa Kỳ/quốc tế; quy tắc khớp lệnh, phí, biên độ dao động và dữ liệu tại Việt Nam chưa được xác minh trong phiên này. Không phần nào là khuyến nghị cá nhân, không tạo dự báo và không yêu cầu dùng tiền thật.

**Kiến thức tiên quyết:** hoàn thành W01, W02, W03, W04 và W05. Cần nhớ từ W04: mọi con số đều có đơn vị, kỳ tính và cửa sổ riêng; từ W05: tách dữ kiện/giả định/diễn giải, kịch bản khác dự báo, và cách ghi "chưa thể kết luận".

**Fast-Track Core (30–45 phút):** đọc mục 1–4 (20–25 phút), làm Mini-review 1–2 (5–10 phút), làm quiz 8 câu (8–10 phút), rồi đối chiếu answer key. Phần đào sâu gồm mục "Biết được gì / Chưa thể kết luận gì" và hoạt động tự nguyện.

## 2. Mục tiêu học tập đo được
- [LO-W06-01] Định nghĩa phân tích kỹ thuật (technical analysis) và các công cụ mô tả cơ bản — nến Nhật với bốn mức giá mở cửa/cao nhất/thấp nhất/đóng cửa (OHLC), đường xu hướng (trendline), vùng hỗ trợ (support), vùng kháng cự (resistance), đường trung bình động đơn giản (SMA), đường trung bình động hàm mũ (EMA), khối lượng (volume) — bằng tên Việt–Anh, nêu đúng cửa sổ N phiên và đơn vị (giá đo bằng VND/cổ phần, khối lượng đo bằng cổ phiếu), và giới hạn "công cụ mô tả/giả thuyết làm việc, không phải dự báo chắc chắn".
- [LO-W06-02] Tính SMA-N và EMA-N cho một chuỗi giá đóng cửa giả định với hệ số làm mượt α = 2/(N+1), ghi rõ đơn vị, cửa sổ, giả định khởi tạo (seeding), và đọc so sánh mức phản ứng của SMA với EMA; nhận diện vùng hỗ trợ/kháng cự trên dữ liệu giả định cùng quy ước "kháng cự cũ thành hỗ trợ mới" (role reversal).
- [LO-W06-03] Phân tích một kịch bản giả định (giá thủng trendline/hỗ trợ đi kèm khối lượng tăng vọt) bằng cách tách dữ kiện — giả định — diễn giải, đánh dấu tín hiệu là "cảnh báo/giả thuyết chờ kiểm chứng", và tính chênh lệch giữa "kết quả trên giấy" và "kết quả giả định sau phí giao dịch và trượt giá (slippage)".
- [LO-W06-04] Phản biện các khẳng định kiểu "chỉ báo này đúng 100%", "3/3 biểu đồ đều thắng" bằng cách chỉ ra look-ahead bias, data-snooping/cherry-pick, survivorship bias, chi phí giao dịch bị bỏ qua, và ghi đúng trạng thái "chưa thể kết luận".

## 3. Khái niệm cốt lõi
### 3.1. Phân tích kỹ thuật là gì: đầu vào là giá và khối lượng đã qua, mục tiêu là mô tả và đặt giả thuyết
CFA Institute (2026 Curriculum, bài đọc *Equity Valuation: Concepts and Basic Tools*) phân biệt hai hướng như sau: "Technical analysis uses such information as stock price and trading volume as the basis for investment decisions" — phân tích kỹ thuật dùng thông tin như **giá cổ phiếu và khối lượng giao dịch** làm cơ sở cho quyết định đầu tư; trong khi "fundamental analysis uses information about the economy, industry, and company as the basis for investment decisions" — phân tích cơ bản dùng thông tin về nền kinh tế, ngành và doanh nghiệp. Cùng bài đọc nêu khác biệt về mục đích: technical analysts dùng thông tin để **dự đoán chuyển động giá** và quyết định theo hướng dự đoán, còn fundamental analysts dùng thông tin để **ước lượng giá trị** của chứng khoán rồi so với giá thị trường. [W06-C01]

Bài học này dạy TA theo vai trò **mô tả — tạo giả thuyết**: đọc giá/khối lượng đã quan sát để mô tả cấu trúc xu hướng và tạo "giả thuyết làm việc" chờ kiểm chứng, không phải để khẳng định giá tương lai. Hai điểm cần đọc đúng:
- TA **không thay thế** phân tích cơ bản của W05: hai hướng dùng đầu vào thông tin khác nhau (giá/khối lượng đã qua so với nền tảng kinh tế — ngành — doanh nghiệp); trong thực hành nhiều người dùng cả hai, nhưng bài học này không khẳng định cách phối hợp nào "tốt hơn" vì không nguồn đã mở đánh giá điều đó.
- Cụm "dự đoán chuyển động giá" trong giáo trình CFA mô tả **ý định** của người dùng TA, không phải mức độ thành công: phần 3.6 và 3.7 cho thấy bằng chứng học thuật về khả năng thắng nhất quán là rất nhiều dấu hỏi.

Vai trò của khối lượng có một điểm tựa lý thuyết học thuật: Blume, Easley và O'Hara (1994, *Journal of Finance* 49(1), 153–181 — trang tóm tắt đã mở trên IDEAS/RePEc) viết trong mô hình lý thuyết của họ: "volume provides information on information quality that cannot be deduced from the price statistic" — khối lượng chứa thông tin về chất lượng thông tin mà **không suy được từ thống kê giá một mình**, và "Technical analysis, thus, arises as a natural component of the agents' learning process". Đây là mô hình lý thuyết với giả định riêng — không phải hằng số thị trường thực nghiệm. [W06-C02]

### 3.2. Dữ liệu nền của biểu đồ: mỗi phiên có O/H/L/C và khối lượng
Mỗi phiên giao dịch được tóm tắt bằng bốn mức giá: **giá mở cửa (Open)**, **giá cao nhất (High)**, **giá thấp nhất (Low)** và **giá đóng cửa (Close)** — viết tắt **OHLC**, đơn vị VND/cổ phần cho từng mức. Biểu đồ **nến Nhật (candlestick chart)** là quy ước vẽ phổ biến: thân nến thể hiện khoảng giữa Open và Close, bóng nến thể hiện High và Low; nến xanh/đỏ (hoặc trắng/đen) chỉ phiên tăng/giảm so với phiên trước — màu là quy ước hiển thị, không thêm thông tin. Đi kèm là **khối lượng (volume)**: số lượng chứng khoán được giao dịch trong kỳ, đơn vị cổ phiếu (thường ghi theo triệu cổ phiếu). Investor.gov (U.S. SEC) mô tả băng giá hợp nhất (consolidated tape) là hệ thống "reports the latest price and volume data on sales of exchange-listed stocks" — báo giá và khối lượng gần nhất của cổ phiếu niêm yết, tức giá và khối lượng là hai thống kê thị trường cơ bản được công bố song song. [W06-C02]

Đánh dấu bằng chứng: OHLC, nến, cách ghi volume là **quy ước trình bày dữ liệu** (convention), không phải khẳng định về xu hướng tương lai.

### 3.3. Đường trung bình động: SMA và EMA (công thức, đơn vị, cửa sổ)
Hai công cụ làm mượt giá phổ biến nhất, đều là phép tính toán học chuẩn (calculation only):

**Đường trung bình động đơn giản (Simple Moving Average — SMA):** với cửa sổ N phiên và giá đóng cửa P:

`SMA_N(t) = (P_t + P_(t−1) + ... + P_(t−N+1)) / N`

- Đơn vị: VND/cổ phần (cùng đơn vị với giá); cửa sổ: N phiên giao dịch; mỗi phiên có một giá trị SMA mới khi cửa sổ trượt.
- Ví dụ N = 20 hoặc 50 thường gặp; số N là **lựa chọn của người dùng**, không có giá N "đúng" nào được nguồn đã mở xác nhận.

**Đường trung bình động hàm mũ (Exponential Moving Average — EMA):** đặt trọng số lớn hơn cho phiên gần nhất:

`EMA_N(t) = α · P_t + (1 − α) · EMA_N(t−1)`, với hệ số làm mượt `α = 2/(N+1)` (quy ước phổ biến khi gọi tên "EMA-N")

- Vì EMA_N(t−1) phải biết trước, cần **giả định khởi tạo (seeding)** — bài học dùng quy ước: EMA của phiên đầu tiên lấy bằng SMA_N của N phiên đầu; quy ước khác chỉ ảnh hưởng vài phiên đầu, sau đó hai dãy hội tụ về nhau.
- Hệ quả định nghĩa: EMA phản ứng **nhanh hơn** SMA cùng cửa sổ với biến động giá mới vì phiên gần nhất được đặt trọng số α, các phiên cũ giảm dần theo cấp số nhân (1−α) — đây là thuộc tính của công thức, không phải "ưu điểm dự đoán".

Đánh dấu bằng chứng: toàn bộ mục này là **phép tính và quy ước**, không phải khẳng định rằng "giá có xu hướng quay về SMA" hay "giao cắt SMA báo hiệu đảo chiều". Các cách đọc kiểu đó là quy ước kỹ thuật phổ biến, nhưng không nguồn chính thống đã mở chứng minh độ tin cậy thống kê của chúng. [W06-C02]

### 3.4. Đường xu hướng, vùng hỗ trợ, vùng kháng cự và quy ước đảo vai
- **Xu hướng tăng (uptrend):** chuỗi các đỉnh và đáy **cao dần**; xu hướng giảm (downtrend): đỉnh và đáy **thấp dần**; **đường xu hướng (trendline)** tăng là đường nối các đáy của xu hướng tăng. Vẽ đường nào — nối điểm nào, khung thời gian nào — do người vẽ chọn, nên hai người có thể vẽ hai đường khác nhau trên cùng dữ liệu.
- **Vùng hỗ trợ (support level):** vùng giá mà tại đó áp lực mua kỳ vọng đủ mạnh để làm đà giảm chậm lại hoặc bật lên. **Vùng kháng cự (resistance level):** vùng giá mà tại đó áp lực bán kỳ vọng đủ mạnh để làm đà tăng chậm lại. Lưu ý định nghĩa dùng "kỳ vọng" — mức này là **diễn giải tâm lý cung–cầu quan sát được trên dữ liệu cũ**, không phải luật vật lý của giá.
- **Đảo vai (role reversal):** quy ước kỹ thuật phổ biến — khi giá phá lên trên kháng cự rồi quay lại kiểm định vùng đó và giữ vững, vùng kháng cự cũ được coi là **hỗ trợ mới** (và ngược lại với hỗ trợ cũ thành kháng cự mới). Đây là quy ước quan sát, độ "vững" của vùng phụ thuộc khung thời gian, số lần giá chạm, thanh khoản và cách xác định "phá" là bao nhiêu phần trăm.
- Điểm yếu phương pháp: "vùng" và "đường" là **kết quả của lựa chọn** (điểm neo, độ dày vùng, khung thời gian); cùng một biểu đồ có thể vẽ ra các mức hỗ trợ/kháng cự khác nhau, và sau sự việc người ta dễ chọn vẽ lại đường cho "khớp" — lỗi này được xử lý ở 3.7 và 4.5. [W06-C02]

### 3.5. Khối lượng đi cùng giá và chỉ báo động lượng RSI: tín hiệu cảnh báo, không phải kết luận
Quy ước kỹ thuật phổ biến: chuyển động giá đi kèm **khối lượng cao hơn bình quân** được coi là có sự tham gia rộng hơn; ngược lại, giá đi xa mà khối lượng thấp được coi là "thiếu xác nhận". Cú thủng trendline hoặc vùng hỗ trợ đi kèm khối lượng tăng vọt được coi là **cảnh báo** nghiêm trọng hơn cú thủng khối lượng thấp. Điểm tựa lý thuyết là mô hình của Blume–Easley–O'Hara (3.1): khối lượng có thể chứa thông tin mà giá một mình không chứa. Nhưng cần đọc đúng vai: **"cảnh báo" là từ khóa của bài học** — tín hiệu cảnh báo là giả thuyết chờ kiểm chứng, không phải dự báo "giá chắc chắn về X". [W06-C02][W06-C03]

**Chỉ số sức mạnh tương đối (Relative Strength Index — RSI)** là một chỉ báo động lượng: đo bằng thang 0–100, theo công thức chuẩn `RSI = 100 − 100/(1 + RS)`, với `RS = bình quân mức lãi N phiên / bình quân mức lỗ N phiên` (cửa sổ mặc định theo Wilder là N = 14 phiên). Quy ước phổ biến: RSI trên 70 gọi là "quá mua", dưới 30 gọi là "quá bán" — hai ngưỡng này là **quy ước**, không phải ngưỡng định giá (nhớ W04: không nhầm với P/E). **Phân kỳ âm (bearish divergence)**: giá lập đỉnh cao hơn nhưng RSI lập đỉnh thấp hơn — quy ước đọc này được dùng như **cảnh báo động lượng tăng suy yếu**; nó không khẳng định giá sẽ giảm, và lịch sử có nhiều lần giá tiếp tục tăng sau khi phân kỳ đã xuất hiện. [W06-C02][W06-C03]

### 3.6. Giới hạn dự báo: dạng yếu hiệu quả và "không thể thắng nhất quán"
CFA Institute (2026 Curriculum, bài đọc *Market Efficiency*) định nghĩa **dạng yếu (weak form)** của hiệu quả thị trường: "In the weak form, asset prices fully reflect all market data, which refers to all past price and trading volume information" — ở dạng yếu, giá tài sản phản ánh đầy đủ mọi dữ liệu thị trường, tức toàn bộ thông tin giá và khối lượng quá khứ. Đây chính là tập dữ liệu đầu vào của phân tích kỹ thuật. Tổng kết của cùng bài đọc: "The general conclusion drawn from the efficient market hypothesis is that it is not possible to beat the market on a consistent basis by generating returns in excess of those expected for the level of risk of the investment" — kết luận chung từ giả thuyết hiệu quả thị trường là **không thể đánh bại thị trường một cách nhất quán** bằng lợi nhuận vượt mức kỳ vọng cho mức rủi ro. Bài đọc cũng cảnh báo khi đọc kết quả ngược: người ta "consider the past history of active returns meaningless unless we understand why markets will allow those active returns to continue into the future" — coi lịch sử lợi nhuận vượt trội là vô nghĩa nếu không hiểu vì sao thị trường sẽ cho phép các lợi nhuận đó tiếp diễn. [W06-C03]

Đọc đúng hai chiều:
- EMH là **giả thuyết** có bằng chứng ủng hộ và phản biện: cùng bài đọc CFA liệt kê các **bất thường thị trường (market anomalies)** được ghi nhận, và nêu rõ "in most cases, however, contradictory evidence both supports and refutes the anomaly" — bằng chứng mâu thuẫn vừa ủng hộ vừa bác bỏ. Vì vậy không được suy "EMH đúng tuyệt đối nên TA vô dụng", cũng không được suy "có anomalies nên TA chắc chắn thắng".
- Ý nghĩa thực hành cho W06: tín hiệu kỹ thuật được dùng đúng vai là **cảnh báo/giả thuyết làm việc**; khi bằng chứng chỉ là biểu đồ đẹp hoặc giai đoạn được chọn lọc, kết luận đúng là **"chưa thể kết luận"**. [W06-C03]

### 3.7. Ba lỗi nghiên cứu làm "biểu đồ đẹp": look-ahead, data-snooping/cherry-pick, survivorship
- **Look-ahead bias (thiên kiến nhìn trước):** dùng thông tin **chưa có sẵn** tại thời điểm quyết định. Ví dụ điển hình trong backtest: mô phỏng "mua" bằng giá đóng cửa của chính phiên phát tín hiệu, trong khi thực tế lệnh chỉ có thể khớp ở phiên kế tiếp; hoặc chọn tham số (N, ngưỡng, khung thời gian) tối ưu sau khi đã biết kết quả — khi "đọc trước tương lai" bằng kết quả, hiệu năng mô phỏng sẽ đẹp giả tạo.
- **Data-snooping bias / cherry-pick (sàng dữ liệu / chọn khoảnh khắc):** chọn lọc quy tắc, khung thời gian hoặc đoạn lịch sử đẹp nhất sau khi đã nhìn thấy dữ liệu. Sullivan, Timmermann và White (1999, *Journal of Finance* 54(5), 1647–1691 — trang tóm tắt đã mở trên IDEAS/RePEc) dùng phương pháp bootstrap "White's Reality Check" để đánh giá các quy tắc giao dịch kỹ thuật đơn giản, "quantifying the data-snooping bias and fully adjusting for its effect in the context of the full universe from which the trading rules were drawn" — **định lượng thiên kiến sàng dữ liệu và hiệu chỉnh đầy đủ trên toàn bộ vũ khí quy tắc** mà các quy tắc được rút ra; họ mở rộng bộ 26 quy tắc của Brock–Lakonishok–LeBaron và áp lên 100 năm dữ liệu hằng ngày của Dow Jones Industrial Average. Bản làm việc của cùng nhóm (FMG Discussion Paper 303, mở trên LSE Research Online) trình bày thêm kiểm tra hiệu năng ở giai đoạn **sau mẫu (out-of-sample)**: quy tắc tốt nhất trong mẫu không còn thể hiện hiệu quả vượt trội khi giao dịch trong 10 năm tiếp theo sau mẫu. Ý nghĩa phương pháp: một quy tắc "đẹp" được chọn sau khi nhìn kết quả không phải bằng chứng độ tin cậy; kiểm định ngoài mẫu và trên toàn bộ vũ khí quy tắc là cách giảm lỗi chọn lọc. [W06-C04]
- **Survivorship bias (thiên kiến sống sót):** chỉ giữ lại các đối tượng **còn tồn tại** đến thời điểm nhìn lại, loại bỏ những đối tượng đã biến mất (phá sản, hủy niêm yết, giải thể quỹ). Brown, Goetzmann, Ibbotson và Ross (1992, *The Review of Financial Studies* 5(4), 553–580 — trang tóm tắt đã mở trên IDEAS/RePEc) phân tích mẫu bị cắt bởi sự sống sót và cho thấy "this relationship gives rise to the appearance of predictability" — việc cắt mẫu theo sống sót tạo ra **cảm giác giả về khả năng dự đoán**, đủ mạnh để giải thích phần lớn bằng chứng "người thắng lặp lại". Ví dụ W06: danh sách "cổ phiếu tăng 10 năm liền" lập vào hôm nay đương nhiên không chứa những mã đã bị hủy niêm yết trên đường đi. [W06-C04]

Ba lỗi này thường **cùng xuất hiện** trong một bài đăng "biểu đồ 3/3 đều thắng" — mục 4.5 dựng ví dụ giả định để tự phản biện.

### 3.8. Chi phí, khớp lệnh và trượt giá (slippage)
Tín hiệu kỹ thuật tồn tại dưới dạng **giá**, nên chi phí giao dịch trực tiếp làm xói kết quả:
- **Phí giao dịch (commission):** Investor.gov (U.S. SEC) viết: "You will likely pay a commission when you buy or sell stock or other securities" — bạn thường phải trả phí khi mua hoặc bán cổ phiếu/chứng khoán; phí bù đắp cho dịch vụ của người trung gian. Investor.gov cũng ghi chú về khớp lệnh: nơi và cách broker khớp lệnh "can impact the overall cost" của giao dịch.
- **Giá mua/giá bán hai phía (bid/ask):** theo Investor.gov, "bid" là giá cao nhất người mua sẵn sàng trả và "ask" là giá thấp nhất người bán sẵn sàng nhận… — chênh lệch ask−bid là chi phí ẩn khi quay lệnh nhanh; cổ phiếu kém thanh khoản thường có chênh lệch rộng hơn (xem mục Liquidity trên cùng trang).
- **Trượt giá (slippage) — định nghĩa của bài học:** chênh lệch giữa giá kỳ vọng (hoặc giá tín hiệu) và giá khớp thực tế. Tài liệu chính thức của U.S. SEC — Investor Bulletin "Trading Basics: Understanding the Different Ways to Buy and Sell Stock" (Pub. No. 141) — nêu rõ: với lệnh thị trường (market order), "the price at which a market order will be executed is not guaranteed" — giá khớp **không được bảo đảm**; "the last-traded price is not necessarily the price at which a market order will be executed. In fast-moving markets, the price at which a market order will execute often deviates from the last-traded price" — giá khớp gần nhất không nhất thiết là giá lệnh của bạn khớp, và trong thị trường biến động nhanh giá khớp thường lệch khỏi báo giá thời gian thực. Với lệnh dừng (stop order): "The stop price is not the guaranteed execution price for a stop order. The stop price is a trigger that causes the stop order to become a market order. The execution price... can deviate significantly from the stop price in a fast-moving market" — **giá dừng là điểm kích hoạt, không phải giá khớp bảo đảm**. Với lệnh giới hạn (limit order): "A limit order is not guaranteed to execute" — có thể không khớp. [W06-C05]
- Quy ước của bài học: **chi phí tròn chuyến (round-trip cost) = phí hai chiều + trượt giá mua + trượt giá bán.** Một tín hiệu có "lợi nhuận trên giấy" 2–3% có thể bị bào mòn đáng kể sau các khoản này — mục 4.4 tính con số giả định cụ thể.
- Giới hạn nguồn: các mô tả order type trên là **tài liệu giáo dục của SEC Hoa Kỳ**; quy tắc khớp lệnh, biên độ dao động, phí và thuế tại Việt Nam phải tra công bố chính thức trong nước — bài học không xác minh và không nêu chi tiết đó. [W06-C05]

## 4. Ví dụ và worked examples (giả định, đơn vị VND/cổ phần)
### 4.1. Dữ liệu nền — "Công ty V" (hư cấu hoàn toàn)
**Giả định của người viết:** một cổ phiếu hư cấu "Công ty V" với 10 phiên giả định (đánh số phiên 1–10, không gắn ngày lịch thật); giá đóng cửa đơn vị VND/cổ phần, khối lượng đơn vị triệu cổ phiếu:

| Phiên | Đóng cửa (VND/cp) | Khối lượng (triệu cp) |
|---|---:|---:|
| 1 | 10,00 | 0,9 |
| 2 | 10,20 | 1,0 |
| 3 | 10,10 | 0,8 |
| 4 | 10,40 | 1,1 |
| 5 | 10,60 | 1,2 |
| 6 | 10,30 | 0,9 |
| 7 | 10,50 | 1,0 |
| 8 | 10,90 | 1,4 |
| 9 | 11,20 | 1,6 |
| 10 | 11,00 | 1,1 |

Khối lượng bình quân 10 phiên = 11,0/10 = **1,10 triệu cp/phiên**. Không phiên nào trong bảng là dữ liệu thị trường thật. [W06-C06]

### 4.2. Tính SMA-5 và EMA-5 (calculation only)
**SMA-5** (cửa sổ 5 phiên, đơn vị VND/cp, bắt đầu có giá trị từ phiên 5):

`SMA_5(5) = (10,00 + 10,20 + 10,10 + 10,40 + 10,60)/5 = 51,30/5 = 10,26`
`SMA_5(6) = 51,60/5 = 10,32` ; `SMA_5(7) = 51,90/5 = 10,38` ; `SMA_5(8) = 52,70/5 = 10,54` ; `SMA_5(9) = 53,50/5 = 10,70` ; `SMA_5(10) = 53,90/5 = 10,78`

**EMA-5** với `α = 2/(5+1) = 2/6 ≈ 0,3333`. **Giả định khởi tạo khai báo:** EMA_5(5) = SMA_5(5) = 10,26.

`EMA_5(6) = 0,3333·10,30 + 0,6667·10,26 = 10,2733`
`EMA_5(7) = 0,3333·10,50 + 0,6667·10,2733 = 10,3489`
`EMA_5(8) = 0,3333·10,90 + 0,6667·10,3489 = 10,5326`
`EMA_5(9) = 0,3333·11,20 + 0,6667·10,5326 = 10,7551`
`EMA_5(10) = 0,3333·11,00 + 0,6667·10,7551 = 10,8367`

**Đọc theo định nghĩa (không đọc theo dự đoán):** tại phiên 10, EMA-5 = 10,84 gần giá hiện tại 11,00 hơn SMA-5 = 10,78 — đúng thuộc tính "trọng số thiên về phiên gần" của công thức. Từ đây **không suy ra** hướng giá phiên 11; hành vi "giá thường quay về đường trung bình" hoặc "giao cắt là tín hiệu mua/bán" là quy ước kỹ thuật chưa được nguồn đã mở xác nhận độ tin cậy. [W06-C02][W06-C06]

### 4.3. Vùng hỗ trợ/kháng cự, trendline và tín hiệu phiên 11 (giả định)
**Giả định mở rộng:** trong 20 phiên trước bảng 4.1, giá của "Công ty V" chạm đáy quanh 10,00–10,10 bốn lần và đỉnh quanh 10,90–11,10 hai lần. Theo quy ước 3.4, người vẽ có thể ghi: **vùng hỗ trợ giả định 9,90–10,10**; **vùng kháng cự giả định 10,90–11,10**; các đáy hồi 10,00 → 10,20 → 10,40 (giả định nội suy) tạo **trendline tăng**.

**Phiên 11 (giả định):** giá mở 10,85, thủng xuống dưới mức đường trendline tăng ước tính tại phiên đó (ước tính bằng mắt trên biểu đồ — chính sự "ước tính bằng mắt" này là một hạn chế: không có công thức chính xác cho vị trí đường), đóng cửa 10,40, khối lượng **3,0 triệu cp ≈ 2,7× bình quân 1,10**.

**Diễn giải (đánh dấu rõ):** theo quy ước 3.5, đây là **tín hiệu cảnh báo** "cấu trúc xu hướng tăng đang suy yếu" — giả thuyết làm việc chờ kiểm chứng bằng dữ kiện tiếp theo, **không** phải dự báo "giá chắc chắn về mức X". Cùng một phiên, người vẽ khác có thể chọn đường trendline khác và không thấy tín hiệu — tính chủ quan của việc vẽ là dữ kiện phương pháp, không phải khiếm khuyết tình cờ. [W06-C02][W06-C06]

### 4.4. "Kết quả trên giấy" so với "kết quả giả định sau phí và slippage" (calculation only)
**Giả định:** tín hiệu mua phát hiện khi đóng cửa phiên 10 ở **11,00 VND/cp**. Lệnh thị trường đặt phiên kế khớp **11,05 VND/cp** → trượt giá mua +0,05 VND/cp = +0,45% so với giá tín hiệu (0,05/11,00). Phí lệnh giả định **0,15%/chiều**, tính trên giá trị giao dịch, bỏ qua ghép lãi (khai báo giả định). Sau đó tín hiệu bán tại đóng cửa **11,30 VND/cp**; lệnh khớp **11,24 VND/cp** → trượt giá bán −0,06 VND/cp = −0,53% so với giá tín hiệu (0,06/11,30).

| Đại lượng | Công thức | Kết quả |
|---|---|---:|
| Lợi nhuận "trên giấy" | (11,30 − 11,00)/11,00 | **+2,73%** |
| Lợi nhuận khớp giả định, trước phí | (11,24 − 11,05)/11,05 | **+1,72%** |
| Phí tròn chuyến | 2 × 0,15% | −0,30% |
| Lợi nhuận giả định ròng | 1,72% − 0,30% | **≈ +1,42%** |

Chênh lệch giấy − ròng ≈ 2,73% − 1,42% = **1,31 điểm %**, phân rã gần đúng: 0,45% (trượt mua) + 0,53% (trượt bán) + 0,30% (phí) + ~0,03% phần dư do cộng số trên các cơ số nhân khác nhau. Bài học: khung "tín hiệu → lợi nhuận" luôn phải đi qua **khớp lệnh và chi phí**; con số trên giấy là giới hạn trên lý tưởng. Mọi tỷ lệ phí/slippage ở đây là giả định minh họa — tỷ lệ thực tế phụ thuộc thanh khoản, khung giờ, loại lệnh và biểu phí từng nơi, phải tra công bố chính thức. [W06-C05][W06-C06]

### 4.5. Bẫy "biểu đồ 3/3 đều thắng" — tự phản biện một mẫu cherry-pick (giả định)
**Bài đăng giả định:** "Trong 3 biểu đồ tôi đăng, mỗi lần SMA-20 cắt lên SMA-50 giá đều tăng tiếp — 3/3 — nên cắt lên là mua chắc thắng." Liệt kê lỗi theo 3.7:
1. **Cherry-pick/data-snooping:** 3 biểu đồ không cho biết tổng số tín hiệu đã xuất hiện trong mẫu; theo Sullivan–Timmermann–White, phải định lượng trên **toàn bộ vũ khí quy tắc** và hiệu chỉnh data-snooping thì mới nói được gì về độ tin cậy — bằng chứng "3/3 đẹp" là dạng cực đoan của chọn lọc.
2. **Look-ahead bias:** nếu mô phỏng "mua" đúng giá đóng cửa phiên phát tín hiệu, kết quả đã lợi hơn thực tế (lệnh phải khớp phiên kế, xem 4.4); nếu tham số 20/50 được chọn sau khi thấy biểu đồ thì đó là chọn tham số bằng kết quả.
3. **Survivorship bias:** 3 mã được chọn còn niêm yết đến hôm nay; các trường hợp "cắt lên rồi sụp, hủy niêm yết" đã biến mất khỏi khả năng được đăng — đúng cơ chế "appearance of predictability" của Brown–Goetzmann–Ibbotson–Ross.
4. **Bỏ qua chi phí:** chưa trừ phí và slippage (4.4).
5. **Biến cảnh báo thành dự báo:** "chắc thắng" là ngôn ngữ dự báo chắc chắn — trái với giới hạn 3.6.

Kết luận đúng: từ 3 biểu đồ chọn lọc **chưa thể kết luận** độ tin cậy của tín hiệu; cần mẫu đầy đủ, kiểm định ngoài mẫu, hiệu chỉnh chọn lọc và chi phí. [W06-C03][W06-C04][W06-C06]

### 4.6. Phân loại bằng chứng
- **Kết quả/nguồn (đã mở trực tiếp):** CFA *Equity Valuation: Concepts and Basic Tools* cho định nghĩa đầu vào TA (giá + khối lượng) và phân biệt với FA; CFA *Market Efficiency* cho dạng yếu, tổng kết "không thể thắng nhất quán", cảnh báo về lịch sử active returns và tình trạng bằng chứng hai chiều về anomalies; Blume–Easley–O'Hara (tóm tắt) cho vai trò thông tin của khối lượng trong mô hình lý thuyết; Sullivan–Timmermann–White 1999 (tóm tắt + bản làm việc LSE) cho data-snooping và kiểm định ngoài mẫu; Brown–Goetzmann–Ibbotson–Ross 1992 (tóm tắt) cho survivorship tạo "appearance of predictability"; SEC "Trading Basics" cho tính không bảo đảm của giá khớp lệnh; Investor.gov Glossary cho commission, bid/ask, khớp lệnh, băng giá hợp nhất.
- **Giả định minh họa:** "Công ty V", toàn bộ 10 phiên giá/khối lượng 4.1, 20 phiên mở rộng 4.3, phiên 11 (mở 10,85, đóng 10,40, khối lượng 3,0 triệu cp), vùng hỗ trợ/kháng cự giả định, seeding EMA, phí 0,15%/chiều, giá khớp 11,05/11,24, các tỷ lệ slippage 0,45%/0,53%; bài đăng giả định 4.5.
- **Phép tính/quy ước:** SMA/EMA (α = 2/(N+1)), RSI (thang 0–100, N = 14 theo Wilder), OHLC/nến, cách vẽ trendline và vùng hỗ trợ/kháng cự — calculation/convention only; các cách "đọc tín hiệu" là quy ước kỹ thuật chưa được chứng minh độ tin cậy bởi nguồn đã mở.

## 5. Mini-review và truy xuất kiến thức
### Mini-review 1: tính SMA-5 và EMA-5, đọc thuộc tính công thức
Không nhìn phần trên: dùng bảng 4.1, tính SMA-5 tại phiên 6 và EMA-5 tại phiên 6 (biết EMA-5 khởi tạo bằng SMA-5 tại phiên 5 = 10,26; α = 1/3). (a) Hai giá trị là bao nhiêu, đơn vị và cửa sổ là gì? (b) Giá đóng cửa phiên 6 là 10,30 — đường nào gần giá hiện tại hơn và vì sao theo định nghĩa? **Tự kiểm:** (a) SMA-5(6) = (10,20+10,10+10,40+10,60+10,30)/5 = 51,60/5 = **10,32 VND/cp** (cửa sổ 5 phiên); EMA-5(6) = (1/3)·10,30 + (2/3)·10,26 = 10,26 + 0,04/3 = **10,2733 ≈ 10,27 VND/cp**. (b) EMA (10,27) gần 10,30 hơn SMA (10,32) vì EMA đặt trọng số α cho phiên gần nhất — thuộc tính của công thức; không suy ra hướng giá phiên 7.

### Mini-review 2: đọc một bài đăng kỹ thuật có ba lỗi lồng nhau
Một bài đăng giả định viết: "Phiên 11 vừa thủng trendline kèm khối lượng gấp 3 bình quân, nên giá chắc chắn rơi về 9,00. Chứng minh: 3 biểu đồ cổ phiếu tôi chọn đều như vậy." Liệt kê ít nhất năm lỗi và trạng thái kết luận đúng. **Tự kiểm:** (1) "chắc chắn rơi về" biến **cảnh báo** thành **dự báo chắc chắn** — dữ kiện chỉ ủng hộ giả thuyết "cấu trúc tăng suy yếu"; (2) mức "9,00" không có dữ kiện hay mô hình nào kèm theo; (3) "3 biểu đồ" là cherry-pick — không nêu tổng số tín hiệu trong mẫu, đúng cấu trúc data-snooping mà Sullivan–Timmermann–White phải hiệu chỉnh bằng toàn bộ vũ khí quy tắc; (4) các mã được chọn còn tồn tại đến hôm nay — survivorship bias, các trường hợp thất bại đã biến mất (Brown và cộng sự: "appearance of predictability"); (5) chưa trừ phí và slippage (4.4); (6) nếu mô phỏng mua/bán tại giá đóng cửa phiên tín hiệu thì thêm look-ahead bias. Kết luận đúng: tín hiệu là **cảnh báo/giả thuyết chờ kiểm chứng**; về mục tiêu giá — **chưa thể kết luận**.

## 6. Cảnh báo & sai lầm thường gặp
- **Biến chỉ báo thành máy dự báo:** nói "cắt lên chắc tăng", "RSI quá mua chắc giảm". Tín hiệu kỹ thuật đúng vai là mô tả + giả thuyết cảnh báo; dạng yếu hiệu quả (CFA) nghĩa là dữ liệu giá/khối lượng quá khứ đã nằm trong giá.
- **Cherry-pick biểu đồ:** đăng 3/3 ví dụ thắng, không nêu tổng số tín hiệu. Sullivan–Timmermann–White: phải hiệu chỉnh data-snooping trên toàn bộ vũ khí quy tắc.
- **Survivorship:** lập danh sách "mã tăng 10 năm liền" hôm nay — tự động loại mã đã hủy niêm yết; Brown–Goetzmann–Ibbotson–Ross: cắt mẫu theo sống sót tạo "appearance of predictability".
- **Look-ahead bias:** mô phỏng mua/bán bằng giá đóng cửa phiên tín hiệu, hoặc chọn N/ngưỡng sau khi biết kết quả.
- **Bỏ qua phí và slippage:** tính "lợi nhuận trên giấy" từ giá tín hiệu; SEC: giá khớp lệnh thị trường không được bảo đảm, giá dừng chỉ là điểm kích hoạt — mục 4.4 cho thấy chênh ~1,31 điểm % trong ví dụ giả định.
- **Coi vùng hỗ trợ/kháng cự là bức tường cứng:** vùng do người vẽ chọn điểm neo, khung thời gian; hai người vẽ khác nhau ra hai vùng; "phá" là bao nhiêu phần trăm cũng phải định nghĩa trước.
- **Nhầm RSI "quá mua" với "đắt" (W04):** RSI là động lượng giá trên thang 0–100 với ngưỡng 70/30 là quy ước — không phải định giá, không liên hệ trực tiếp P/E.
- **Thiên kiến xác nhận — anchoring — overconfidence:** chỉ tìm biểu đồ ủng hộ luận điểm; bám vào một mức giá cũ; nhầm hiểu khái niệm với khả năng đoán đúng tương lai. Ghi thiếu nguồn, thời điểm, đơn vị, cửa sổ N của từng con số cũng là lỗi.

## 7. Biết được gì / Chưa thể kết luận gì
**Biết được:** định nghĩa TA theo CFA — đầu vào là giá và khối lượng quá khứ, khác đầu vào của phân tích cơ bản; bốn mức OHLC và quy ước nến; công thức SMA_N với đơn vị VND/cổ phần, cửa sổ N phiên; công thức EMA_N với α = 2/(N+1), cần giả định khởi tạo, phản ứng nhanh hơn SMA cùng cửa sổ do trọng số thiên về phiên gần; định nghĩa hỗ trợ/kháng cự/trendline và quy ước đảo vai, kèm tính chủ quan khi vẽ; vai trò khối lượng theo mô hình lý thuyết của Blume–Easley–O'Hara và quy ước đọc "giá đi kèm volume"; RSI (thang 0–100, N = 14, ngưỡng 70/30 là quy ước) và phân kỳ như cảnh báo động lượng; dạng yếu hiệu quả (giá phản ánh toàn bộ dữ liệu giá/khối lượng quá khứ) và tổng kết "không thể đánh bại thị trường nhất quán" của CFA; ba lỗi nghiên cứu — look-ahead, data-snooping/cherry-pick (Sullivan–Timmermann–White), survivorship (Brown và cộng sự); định nghĩa slippage và tính không bảo đảm của giá khớp lệnh theo SEC; cách tính chênh lệch "trên giấy" so với "sau phí và slippage".

**Chưa thể kết luận:** "chỉ báo X chắc chắn đúng" hoặc "cắt giao chắc thắng" — không nguồn nào đã mở chứng minh độ tin cậy thống kê của các quy ước tín hiệu; một mục tiêu giá cụ thể ("về 9,00") từ một tín hiệu cảnh báo; độ tin cậy của một quy tắc từ vài biểu đồ chọn lọc — cần mẫu đầy đủ, kiểm định ngoài mẫu, hiệu chỉnh data-snooping và chi phí; bất kỳ kết luận nào áp thẳng tài liệu Hoa Kỳ (CFA, SEC, dữ liệu DJIA) vào quy tắc khớp lệnh, biên độ, phí, thuế hoặc hành vi giá tại thị trường Việt Nam — bối cảnh Việt Nam chưa được xác minh trong phiên này; lợi nhuận hoặc hướng giá tương lai — bài học không tạo dự báo nào. Với bằng chứng chỉ là biểu đồ đã chọn lọc, kết luận đúng là "chưa thể kết luận".

## 8. Checklist thực hành
- [ ] Viết mục tiêu, thời hạn và giới hạn rủi ro.
- [ ] Ghi nguồn, thời điểm, đơn vị (VND/cp, triệu cp), cửa sổ N và trạng thái xác minh.
- [ ] Tách dữ kiện, giả định, phép tính và kết luận; đánh dấu tín hiệu là "cảnh báo/giả thuyết".
- [ ] Kiểm tra look-ahead, chọn lọc mẫu, sống sót, thanh khoản, phí và slippage.
- [ ] Không dùng tiền thật chỉ vì hoàn thành bài.

## 9. Hoạt động áp dụng tự nguyện (giả định, chỉ trên giấy)
Dùng **chỉ dữ liệu giả định tự tạo** (không tra cứu hay nhập dữ liệu thị trường thật): (1) tự viết chuỗi 15 giá đóng cửa giả định (VND/cp) và khối lượng giả định (triệu cp); (2) tính SMA-5 và EMA-5 (α = 2/6, khai báo seeding) trên giấy cho các phiên có đủ cửa sổ; (3) tự đặt một vùng hỗ trợ và một vùng kháng cự giả định, ghi rõ điểm neo đã chọn và vì sao; (4) mô phỏng một tín hiệu cảnh báo tự chọn và tính "trên giấy" so với "sau phí 0,15%/chiều + slippage giả định" như 4.4; (5) tự phản biện chuỗi của chính mình: có look-ahead không, có chọn số đẹp không, danh sách có "sống sót" không; (6) kết thúc bằng ba câu "chưa thể kết luận". Không dùng tiền thật, không đăng ký dịch vụ, không đặt lệnh nào.

## 10. Glossary
| Việt | English | Định nghĩa |
|---|---|---|
| Phân tích kỹ thuật | Technical Analysis | Phương pháp dùng dữ liệu giá và khối lượng quá khứ (thường trình bày bằng biểu đồ) làm đầu vào phân tích; trong bài học này: công cụ mô tả và tạo giả thuyết, không phải máy dự báo (CFA 2026, Equity Valuation: Concepts and Basic Tools). |
| Giá mở cửa/cao nhất/thấp nhất/đóng cửa | Open / High / Low / Close (OHLC) | Bốn mức giá tóm tắt một phiên giao dịch, đơn vị VND/cổ phần; nến Nhật biểu diễn thân = Open–Close, bóng = High/Low. |
| Biểu đồ nến Nhật | Candlestick Chart | Quy ước vẽ OHLC bằng thân và bóng nến; màu tăng/giảm là quy ước hiển thị, không thêm thông tin. |
| Đường trung bình động đơn giản | Simple Moving Average (SMA) | SMA_N(t) = (P_t + … + P_(t−N+1))/N — trung bình cộng giá đóng cửa N phiên gần nhất; đơn vị VND/cổ phần, cửa sổ N phiên. |
| Đường trung bình động hàm mũ | Exponential Moving Average (EMA) | EMA_N(t) = α·P_t + (1−α)·EMA_N(t−1) với α = 2/(N+1); đặt trọng số lớn hơn cho phiên gần nên phản ứng nhanh hơn SMA cùng cửa sổ; cần giả định khởi tạo (seeding). |
| Đường xu hướng | Trendline | Đường nối các đáy (xu hướng tăng) hoặc các đỉnh (xu hướng giảm); vị trí đường phụ thuộc điểm neo và khung thời gian do người vẽ chọn. |
| Vùng hỗ trợ | Support Level | Vùng giá nơi áp lực mua kỳ vọng đủ mạnh để làm đà giảm chậm lại; định nghĩa dùng "kỳ vọng" — là diễn giải trên dữ liệu cũ, không phải bức tường giá. |
| Vùng kháng cự | Resistance Level | Vùng giá nơi áp lực bán kỳ vọng đủ mạnh để làm đà tăng chậm lại; độ dày vùng và tiêu chí "phá" phải định nghĩa trước. |
| Đảo vai | Role Reversal | Quy ước kỹ thuật: sau khi giá phá kháng cự rồi quay lại kiểm định và giữ vững, vùng kháng cự cũ được coi là hỗ trợ mới (và ngược lại). |
| Khối lượng | Volume | Số lượng chứng khoán giao dịch trong kỳ, đơn vị cổ phiếu (thường ghi triệu cổ phiếu); theo mô hình Blume–Easley–O'Hara, có thể chứa thông tin mà giá một mình không chứa. |
| Chỉ số sức mạnh tương đối | Relative Strength Index (RSI) | Chỉ báo động lượng thang 0–100: RSI = 100 − 100/(1+RS), RS = bình quân lãi N phiên / bình quân lỗ N phiên (mặc định N = 14); ngưỡng 70/30 là quy ước "quá mua/quá bán", không phải định giá. |
| Phân kỳ âm | Bearish Divergence | Giá lập đỉnh cao hơn trong khi RSI lập đỉnh thấp hơn; quy ước đọc như cảnh báo động lượng tăng suy yếu — giả thuyết, không phải dự báo đảo chiều. |
| Thiên kiến nhìn trước | Look-Ahead Bias | Dùng thông tin chưa có sẵn tại thời điểm quyết định trong mô phỏng (ví dụ khớp lệnh bằng giá đóng cửa phiên tín hiệu, hoặc chọn tham số sau khi biết kết quả). |
| Sàng dữ liệu / chọn khoảnh khắc | Data-Snooping Bias / Cherry-Picking | Chọn quy tắc, khung thời gian hoặc đoạn lịch sử đẹp nhất sau khi đã nhìn dữ liệu; Sullivan–Timmermann–White (1999) định lượng và hiệu chỉnh bias này trên toàn bộ vũ khí quy tắc. |
| Thiên kiến sống sót | Survivorship Bias | Chỉ giữ lại đối tượng còn tồn tại đến thời điểm nhìn lại; Brown–Goetzmann–Ibbotson–Ross (1992): cắt mẫu theo sống sót tạo "appearance of predictability". |
| Hiệu quả thị trường dạng yếu | Weak-Form Market Efficiency | Trạng thái mà giá phản ánh đầy đủ mọi dữ liệu thị trường quá khứ (giá và khối lượng); theo CFA 2026, Market Efficiency — giả thuyết có bằng chứng hai chiều, không phải định luật. |
| Trượt giá | Slippage | Chênh lệch giữa giá kỳ vọng (giá tín hiệu) và giá khớp thực tế; SEC: giá khớp lệnh thị trường không được bảo đảm và có thể lệch đáng kể trong thị trường biến động nhanh. |
| Phí giao dịch | Commission | Phí trả khi mua/bán chứng khoán qua trung gian (Investor.gov); cấu phần chi phí tròn chuyến cùng slippage. |
| Chi phí tròn chuyến | Round-Trip Cost | Phí hai chiều + trượt giá mua + trượt giá bán; khung "tín hiệu → lợi nhuận" phải đi qua khung chi phí này. |
| Chưa thể kết luận | Not Yet Concluded | Trạng thái khi bằng chứng không đủ hoặc đã chọn lọc; cách ghi chuẩn thay cho mọi kết luận mua/bán/dự báo trong bài học này. |

## 11. Tài liệu tham khảo đã mở
- CFA Institute, "Equity Valuation: Concepts and Basic Tools" (2026 Curriculum, refresher reading, Level I Equity Investments): https://www.cfainstitute.org/insights/professional-learning/refresher-readings/2026/equity-valuation-concepts-basic-tools (truy cập 2026-09-15).
- CFA Institute, "Market Efficiency" (2026 Curriculum, refresher reading, Level I Equity Investments): https://www.cfainstitute.org/insights/professional-learning/refresher-readings/2026/market-efficiency (truy cập 2026-09-15).
- Sullivan, R., Timmermann, A., & White, H. (1999). "Data-Snooping, Technical Trading Rule Performance, and the Bootstrap", *Journal of Finance*, 54(5), 1647–1691 — trang tóm tắt trên IDEAS/RePEc: https://ideas.repec.org/a/bla/jfinan/v54y1999i5p1647-1691.html (truy cập 2026-09-15); bản làm việc FMG Discussion Paper 303 trên LSE Research Online: https://researchonline.lse.ac.uk/id/eprint/119144 (truy cập 2026-09-15).
- Brown, S. J., Goetzmann, W. N., Ibbotson, R. G., & Ross, S. A. (1992). "Survivorship Bias in Performance Studies", *The Review of Financial Studies*, 5(4), 553–580 — trang tóm tắt trên IDEAS/RePEc: https://ideas.repec.org/a/oup/rfinst/v5y1992i4p553-80.html (truy cập 2026-09-15).
- Blume, L., Easley, D., & O'Hara, M. (1994). "Market Statistics and Technical Analysis: The Role of Volume", *Journal of Finance*, 49(1), 153–181 — trang tóm tắt trên IDEAS/RePEc: https://ideas.repec.org/a/bla/jfinan/v49y1994i1p153-81.html (truy cập 2026-09-15).
- U.S. SEC, Investor Bulletin "Trading Basics: Understanding the Different Ways to Buy and Sell Stock" (Pub. No. 141): https://www.sec.gov/investor/alerts/trading101basics.pdf (truy cập 2026-09-15).
- U.S. SEC — Investor.gov, Glossary (mục Commissions; Executing an Order; Bid Price/Ask Price; Consolidated Tape; Liquidity): https://www.investor.gov/introduction-investing/investing-basics/glossary (truy cập 2026-09-15).
- Source ledger: [quality/module-2-source-ledger.md](../../quality/module-2-source-ledger.md).

## 12. Next steps
- [Quiz W06](../../assessments/quizzes/week-06-quiz.md)
- [Answer key W06](../../assessments/answer-keys/week-06-answers.md)
- Sau khi nắm W06, W07 (quản trị rủi ro, position sizing, stop loss) sẽ dùng đúng các khái niệm chi phí/khớp lệnh và "cảnh báo ≠ dự báo" của tuần này để học cách giới hạn tổn thất — nhớ rằng theo SEC, giá dừng là điểm kích hoạt chứ không phải giá khớp bảo đảm.

## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

**Bài:** Phân tích kỹ thuật. **Đầu ra:** Làm lại SMA/EMA và phản biện mẫu3/3 thắng ở mục4.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| Công thức | Ghi cửa sổN, seed EMA và dữ liệu OHLC  đúng thứ tự | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Phép tính | Đối chiếu từng bước với 4.2, không dùng phiên tương lai | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Chi phí | Tính kịch bản sau phí/slippage ở4.4 | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Sai lệch mẫu | Chỉ ra look-ahead, cherry-pick hoặc survivorship bằng ví dụ | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Kết luận | Tách mô tả/tín hiệu/giả thuyết; không coi RSI hoặc chart là bảo đảm | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
