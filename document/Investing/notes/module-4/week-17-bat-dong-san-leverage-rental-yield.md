> [!CAUTION]
> **Tuyên bố miễn trách:** Bài học chỉ nhằm mục đích giáo dục, không phải lời khuyên tài chính, pháp lý, thuế hoặc tín dụng. Không có ticker, bất động sản, tỷ trọng, mức vay hay hồ sơ cá nhân nào được khuyến nghị. Không dùng ví dụ này để quyết định vay hoặc mua tài sản thật.

# W17 — Bất động sản: leverage, rental yield và dòng tiền

## 1. Why It Matters
Một bất động sản cho thuê có thể trông hấp dẫn vì giá tăng, tiền thuê đều hoặc ngân hàng chấp nhận cho vay. Ba quan sát đó chưa đủ để kết luận một thương vụ tốt. Người học cần tách tài sản khỏi khoản nợ, doanh thu danh nghĩa khỏi doanh thu thu được, dòng tiền vận hành khỏi dòng tiền sau nợ, và phép tính khỏi dự báo.

Leverage, hay đòn bẩy, là việc dùng vốn vay cùng vốn tự có để kiểm soát một tài sản lớn hơn số vốn bỏ ra. Đòn bẩy khuếch đại kết quả trên vốn tự có theo cả hai chiều: nếu tài sản tạo ra lợi nhuận cao hơn chi phí vốn, lợi nhuận trên vốn tự có có thể tăng; nếu thu nhập giảm, lãi suất tăng hoặc giá giảm, phần lỗ và áp lực trả nợ cũng có thể tăng. Vì vậy, “vay được bao nhiêu” không phải là câu hỏi thay thế cho “dòng tiền chịu được bao nhiêu”.

Bài này dùng một tài sản giả định bằng đồng Việt Nam (VND), không dùng dữ liệu thị trường hay mức thuế hiện hành. Mục tiêu là luyện quy trình: đọc giả định, tính nhiều lớp dòng tiền, stress test, chỉ ra số âm và ghi lại điều chưa biết.

## 2. Learning Objectives và W16 Prerequisite
Sau bài học, người học có thể:

- **[LO-W17-01]** Giải thích bằng lời của mình leverage, tỷ lệ khoản vay trên giá trị (Loan-to-Value, LTV), tỷ lệ bao phủ dịch vụ nợ (Debt Service Coverage Ratio, DSCR), gross yield, vacancy, cap rate, dòng tiền và cost of capital; đạt ít nhất 8/10 thuật ngữ trong một bài kiểm tra ngắn.
- **[LO-W17-02]** Tính đúng, có đơn vị và kỳ tính, cho một ví dụ: doanh thu thuê gộp, vacancy, chi phí vận hành, thu nhập vận hành ròng (Net Operating Income, NOI), gross yield, cap rate, debt service và dòng tiền trước/sau nợ; sai số làm tròn không quá 1 triệu VND ở ví dụ này.
- **[LO-W17-03]** Lập bảng độ nhạy ít nhất **năm chiều** gồm vacancy, lãi suất, tiền thuê, thời gian trống và sửa chữa; nêu được ít nhất một kết quả âm và một ngưỡng cần xác minh thay vì tự kết luận.
- **[LO-W17-04]** Phân loại mỗi câu trong một phân tích là fact, assumption, calculation, uncertainty hoặc pending; hoàn thành checklist không đưa ra khuyến nghị tài sản, ticker, tỷ trọng hay khoản vay cá nhân.

**W16 prerequisite:** W16 đã luyện tách technology, product/service, speculative asset và các lớp rủi ro; bài này áp dụng cùng nguyên tắc đó cho bất động sản: pháp lý, thanh khoản, vận hành, đối tác thuê, lãi suất và tài sản thế chấp không được gom thành một nhãn “rủi ro BĐS”. W15 cung cấp cách nhận diện anchoring và overconfidence khi người bán hoặc người mua chọn một con số thuận lợi. W12 nhắc rằng thuế/phí cần đúng thời điểm và văn bản; trong W17, các mức thuế/phí chưa được xác minh sẽ không được trình bày như luật hiện hành.

## 3. Fast Track 30–45 phút
Lộ trình nhanh:

1. **5 phút:** đọc mục 1–2; viết lại bốn ranh giới: gross yield không phải cash flow, NOI chưa trừ nợ, giá chào không phải giá giao dịch, và tax/fees pending nếu chưa có nguồn mở.
2. **10 phút:** đọc mục 4–5; tự giải nghĩa các thuật ngữ trước khi nhìn bảng viết tắt.
3. **15 phút:** làm mục 6 theo bốn giai đoạn. Che kết quả, tự tính revenue, NOI, debt service và post-debt cash flow.
4. **5–10 phút:** đọc mục 7; khoanh các kịch bản có dòng tiền âm và ghi biến nào làm kết quả đổi dấu.
5. **5 phút:** làm hai retrieval block ở mục 9 và đối chiếu checklist mục 10.

Nếu có 45 phút, thêm Deep Dive ở mục 11: tính DSCR ở nhiều định nghĩa mẫu số, phân biệt cap rate với return on equity, và viết ba câu hỏi xác minh cho khoản thuế/phí hoặc điều khoản vay chưa có nguồn.

## 4. Deep Dive: bản đồ dòng tiền và thuật ngữ
**Gross rental yield** (lợi suất cho thuê gộp) là tiền thuê theo hợp đồng trong một năm chia cho giá mua hoặc giá trị tham chiếu được nêu rõ. Nó chưa trừ phòng trống, chi phí vận hành, lãi vay, thuế, chi phí giao dịch và vốn sửa chữa. Một căn có gross yield cao vẫn có thể có dòng tiền âm.

**Vacancy** (thời gian hoặc tỷ lệ trống) là phần thời gian tài sản không tạo doanh thu thuê theo giả định phân tích. Vacancy không chỉ là “mất một tháng tiền thuê”: nó có thể kéo theo chi phí tìm khách, dọn dẹp, môi giới, sửa chữa giữa hai hợp đồng và rủi ro tiền thuê mới thấp hơn. Trong ví dụ, vacancy được mô hình hóa bằng số tháng không thu được tiền, không tuyên bố đó là tỷ lệ thị trường.

**Operating costs** là các chi phí để duy trì việc cho thuê như bảo trì, phí quản lý, tiện ích do chủ nhà chịu, bảo hiểm hoặc dự phòng sửa chữa nếu được giả định. **NOI** là doanh thu thuê thực thu sau vacancy và chi phí vận hành, trước lãi vay, trả gốc và thuế thu nhập. NOI giúp nhìn tài sản như một hoạt động vận hành, chưa bị trộn với cấu trúc vốn.

**Cap rate** (tỷ suất vốn hóa) là NOI chia cho giá trị tài sản hoặc giá mua được chọn làm mẫu số. Cap rate là thước đo thu nhập vận hành trên giá trị tài sản, không phải lãi suất khoản vay và không phải lợi nhuận trên vốn tự có. So sánh cap rate chỉ có ý nghĩa khi kỳ tính, định nghĩa NOI, chất lượng tài sản, vacancy, giá trị và rủi ro tương đồng.

**Debt service** là tổng nghĩa vụ trả nợ trong kỳ theo hợp đồng, thường gồm gốc và lãi. **DSCR** trong bài là NOI chia cho debt service. DSCR lớn hơn 1 theo phép tính mẫu nghĩa là NOI đủ bao phủ debt service trước các khoản ngoài mô hình; nó không bảo đảm được ngân hàng chấp thuận, không thay thế DTI của hộ gia đình và không bao quát mọi chi phí. Khi một tổ chức dùng định nghĩa DSCR khác, phải ghi rõ tử số và mẫu số.

**LTV** là khoản vay chia cho giá trị tài sản được dùng trong mô hình thẩm định. LTV cao nghĩa là ít vốn tự có hơn so với quy mô tài sản và biên an toàn giá trị thấp hơn nếu tài sản giảm giá hoặc chi phí bán phát sinh. LTV không tự nói lên khả năng trả nợ, vì khả năng trả nợ còn phụ thuộc lãi suất, tenor, lịch trả gốc, thu nhập và dòng tiền.

**Rental yield** thường chỉ một tỷ lệ thu nhập thuê; phải hỏi là gross hay net, mẫu số là giá mua hay giá trị thị trường, có trừ vacancy và chi phí nào, và có trừ nợ hay chưa. **Cash-on-cash return** là một tỷ lệ khác, thường lấy dòng tiền sau chi phí và sau nợ chia cho vốn tiền mặt đã bỏ vào; nếu dùng, phải công bố tử số và mẫu số thay vì gọi mọi tỷ lệ là yield.

### 4.1. Source-bounded underwriting terms

Các khái niệm tín dụng bất động sản dưới đây được đối chiếu trong phạm vi tài liệu underwriting chính thức, không phải ngưỡng áp dụng cho mọi khoản vay. Fannie Mae định nghĩa **underwritten DSCR** là underwritten net cash flow chia cho annual debt service trong hướng dẫn multifamily; tài liệu quản lý tài sản của Fannie Mae cũng nêu direct-cap method dùng cap rate và net cash flow để hỗ trợ định giá/LTV. Vì vậy W17 có thể dùng LTV và DSCR như thuật ngữ phân tích, nhưng phải ghi rõ định nghĩa tử số/mẫu số và sản phẩm underwriting nếu áp dụng thực tế.

- [Fannie Mae Multifamily Guide — Underwritten DSCR](https://mfguide-acpt.fanniemae.com/node/3781), mục 905.02, đọc 2026-10-06: underwritten DSCR = underwritten NCF / annual debt service; tài liệu thuộc phạm vi underwriting multifamily của Fannie Mae.
- [Fannie Mae Multifamily Asset Management form](https://multifamily.fanniemae.com/media/37281/download), đọc 2026-10-06: direct-cap method dùng cap rate và NCF trong bối cảnh định giá/LTV; không phải chuẩn thị trường Việt Nam.
- [FINRA Bonds](https://www.finra.org/investors/investing/investment-products/bonds), đọc 2026-10-06: nguồn định nghĩa duration/rủi ro lãi suất được dùng ở W14, không phải nguồn pháp lý BĐS.

Gross yield, vacancy, NOI, cap rate, cash flow và cost of capital trong W17 tiếp tục là các định nghĩa sư phạm và phép tính tự chứa: gross yield/cap rate chỉ có ý nghĩa theo tử số, mẫu số, kỳ và assumptions đã công bố; không biến thành chuẩn thẩm định, giá thuê, tỷ lệ trống, ngưỡng DSCR/LTV hay khuyến nghị tín dụng tại Việt Nam.

**Cost of capital** là chi phí kinh tế của các nguồn vốn. Với nợ, đó có thể là lãi suất và các khoản phí liên quan; với vốn tự có, đó là mức sinh lời thay thế mà người sở hữu yêu cầu hoặc bỏ qua khi dùng tiền cho tài sản này. Trong bài, cost of capital chỉ là khung phân tích, không phải một “lãi suất chuẩn” hay ngưỡng khuyến nghị. So sánh NOI với lãi vay chỉ là một lát cắt; phải xem kỳ cố định/thả nổi, phí, gốc, thuế và rủi ro.

**Pre-tax cash flow** là dòng tiền trước thuế theo mô hình: NOI trừ debt service và các khoản vốn được đưa vào mô hình. **Post-tax cash flow** là dòng tiền đó sau khoản thuế được xác minh hoặc giả định. Không được gọi một con số là post-tax nếu chưa nêu căn cứ, kỳ tính, chủ thể chịu thuế và khoản nào được khấu trừ. Trong W17, thuế và phí pháp lý Việt Nam là pending vì chưa mở nguồn authoritative; các số 0 VND trong ví dụ chỉ là giả định loại trừ, không phải mức pháp luật.

## 5. Fact, Assumption, Calculation, Uncertainty
| Loại | Cách dùng trong bài | Ví dụ W17 |
|---|---|---|
| Fact đã có bằng chứng | Có nguồn đã mở/đọc hoặc quan sát được trong phạm vi xác định | Không có fact pháp lý, thuế, tín dụng Việt Nam active trong bài này |
| Assumption | Con số đặt ra để luyện tính, phải ghi rõ | Giá 3.000.000.000 VND; thuê 18.000.000 VND/tháng |
| Calculation | Kết quả suy ra từ assumption bằng công thức | NOI 164.100.000 VND/năm trong base case |
| Uncertainty | Điều có thể đúng nhưng dữ liệu chưa đủ | Số tháng trống, sửa chữa, giá thuê thực thu |
| Pending | Claim cần nguồn hoặc kiểm tra chuyên gia trước khi dùng | Thuế/phí hiện hành, điều kiện pháp lý dự án, điều khoản tín dụng cụ thể |

Giá chào, cam kết lợi nhuận, quảng cáo “cho thuê chắc chắn” và một bảng tính do người bán cung cấp không tự biến thành fact. Một bảng tính có thể là calculation đúng trên assumptions sai. Ghi ngày, địa bàn, đơn vị, kỳ tính, nguồn và người chịu chi phí. Nếu chưa có nguồn, hạ claim xuống pending và không dùng nó để khẳng định “an toàn”, “được phép”, “được khấu trừ” hoặc “chắc chắn có người thuê”.

## 6. Worked Example VND: bốn giai đoạn
Đây là **hypothetical activity tự nguyện**, không phải lời mời mua, vay hoặc cho thuê. Đơn vị là VND, làm tròn đến 1.000 VND ở bảng trình bày. Giá mua giả định **3.000.000.000**; vốn tự có **1.200.000.000**; khoản vay **1.800.000.000**; lãi suất giả định cố định **9%/năm**; tenor **20 năm**; phương pháp khấu hao **dư nợ giảm dần với gốc đều hàng tháng**. Tiền thuê niêm yết **18.000.000/tháng**; vacancy base **1 tháng/năm**; bảo trì **24.000.000/năm**; phí quản lý **5% tiền thuê thực thu**. Thuế/phí giao dịch được đặt **0 VND trong phép tính base để loại trừ**, không phải mức thuế/phí hiện hành. Tăng giá giả định **0%**, tức chưa tính lãi vốn. Các số là illustrative, không phải dữ liệu thị trường.

### Giai đoạn 1 — Đầu vào, leverage và tăng trưởng
LTV = khoản vay / giá trị tài sản = 1.800.000.000 / 3.000.000.000 = **60%**. Equity ban đầu = **1.200.000.000**, chưa cộng các khoản mua bán ngoài mô hình. Tỷ lệ tiền thuê năm theo hợp đồng = 18.000.000 × 12 = **216.000.000**. Vì giả định giá tăng **0%**, giá cuối kỳ dùng trong bài vẫn là 3.000.000.000; điều này không dự báo giá và cũng không định giá thanh khoản.

LTV chỉ mô tả cấu trúc vốn tại thời điểm đầu. Nếu giá trị giả định giảm 10% còn 2.700.000.000 trong khi dư nợ chưa trả vẫn cao, LTV theo giá trị mới sẽ tăng lên 1.800.000.000 / 2.700.000.000 = **66,7%**. Đây là phép minh họa rủi ro mẫu số, không phải dự báo thị trường.

### Giai đoạn 2 — Doanh thu, vacancy, operating costs, gross yield và NOI
Doanh thu thực thu base = 18.000.000 × 11 tháng = **198.000.000/năm**. Gross yield theo 12 tháng hợp đồng = 216.000.000 / 3.000.000.000 = **7,20%**. Nếu muốn một tỷ lệ sau vacancy nhưng trước chi phí, effective gross income yield = 198.000.000 / 3.000.000.000 = **6,60%**.

Phí quản lý = 5% × 198.000.000 = **9.900.000**. Tổng operating costs = bảo trì 24.000.000 + quản lý 9.900.000 = **33.900.000**. NOI = 198.000.000 − 33.900.000 = **164.100.000/năm**. Cap rate theo giá mua = 164.100.000 / 3.000.000.000 = **5,47%**, làm tròn hai chữ số. Đây không phải post-debt return; chưa trừ gốc và lãi.

Nếu ai dùng 216.000.000 làm NOI, người đó đã bỏ qua vacancy và operating costs. Nếu dùng NOI chia equity, người đó đang tính một tỷ lệ trên vốn tự có, không phải cap rate. Nếu mẫu số là giá trị thị trường thay vì giá mua, phải ghi rõ giá trị nào và ngày nào.

### Giai đoạn 3 — Debt service, DSCR và dòng tiền trước/sau nợ
Gốc đều tháng = 1.800.000.000 / (20 × 12) = **7.500.000/tháng**. Lãi tháng đầu = 1.800.000.000 × 9% / 12 = **13.500.000**. Debt service tháng đầu = 7.500.000 + 13.500.000 = **21.000.000**, tương đương **252.000.000/năm đầu** nếu giữ nguyên lãi suất và dùng lãi tháng đầu cho phép tính đơn giản. Vì dư nợ giảm dần, lãi các tháng sau giảm; bài không giả vờ rằng 252.000.000 là lịch khấu hao đầy đủ của ngân hàng.

DSCR base giản lược = NOI / debt service năm đầu = 164.100.000 / 252.000.000 = **0,65x**. Pre-debt cash flow theo mô hình = NOI = **164.100.000/năm**. Pre-tax cash flow after debt service = 164.100.000 − 252.000.000 = **−87.900.000/năm**, tức **−7.325.000/tháng bình quân**. Kết quả âm là chủ ý: gross yield 7,20% không có nghĩa tài sản tự trả được khoản nợ này.

Post-tax cash flow **chưa được xác định**. Trong base case, tax/fees = 0 chỉ là dòng loại trừ để không đưa claim chưa xác minh vào số học. Không được biến −87.900.000 thành “sau thuế” hoặc khẳng định được khấu trừ khoản nào. Một bài phân tích thật cần nguồn hiện hành, chủ thể nộp, kỳ kê khai và cách xử lý chi phí.

### Giai đoạn 4 — Kết luận có điều kiện và sensitivity
Base case cho thấy tài sản có NOI dương nhưng dòng tiền sau debt service âm. Kết luận hợp lệ chỉ là: **với các assumption đã nêu, trong năm đầu và trước thuế, tiền thuê không đủ debt service**. Không kết luận tài sản “xấu”, vì chưa biết giá trị, thanh khoản, mục tiêu, chi phí cơ hội hoặc lịch nợ đầy đủ; cũng không kết luận “sẽ tăng giá”.

| Kịch bản | Thay đổi so base | NOI/năm | Debt service/năm | Cash flow trước thuế sau nợ |
|---|---:|---:|---:|---:|
| Base | 1 tháng trống; thuê 18 triệu; bảo trì 24 triệu | 164.100.000 | 252.000.000 | **−87.900.000** |
| Vacancy 3 tháng | Thu 9 tháng | 129.900.000 | 252.000.000 | **−122.100.000** |
| Rate 12% | Gốc đều; lãi tháng đầu 18 triệu | 164.100.000 | 306.000.000 | **−141.900.000** |
| Rent 22 triệu | 1 tháng trống; phí quản lý 5% | 205.900.000 | 252.000.000 | **−46.100.000** |
| Vacancy 6 tháng | Thu 6 tháng | 78.600.000 | 252.000.000 | **−173.400.000** |
| Repairs 120 triệu | Base cộng sửa chữa một lần | 44.100.000 | 252.000.000 | **−207.900.000** |

Kịch bản “rent 22 triệu” vẫn âm; đó là negative result quan trọng chống lại việc chỉ nhìn doanh thu. Kịch bản lãi 12% là minh họa lãi suất tăng, không phải dự báo hay mức trần. Sửa chữa 120 triệu được đặt ra để kiểm tra tail risk; nó không đại diện chi phí thực tế. Người học có thể tự thay một biến mỗi lần, giữ các biến khác cố định, rồi ghi kết quả và giới hạn.

## 7. Sensitivity, liquidity và rủi ro leverage
Sensitivity không trả lời “kịch bản nào sẽ xảy ra”; nó trả lời “nếu assumption đổi, phép tính đổi thế nào”. Ít nhất cần kiểm tra:

- **Vacancy:** 0, 1, 3 và 6 tháng. Ghi rõ mất doanh thu, phí tìm khách và chi phí tái ký có chưa đưa vào.
- **Lãi suất:** fixed, reset hoặc floating chỉ là các nhãn cần đọc trong hợp đồng; không được suy ra điều khoản cụ thể. Trong bài, 9% và 12% đều là giả định.
- **Rent:** tăng, giảm hoặc không thu đủ; phân biệt giá chào với số tiền thực thu.
- **Vacancy duration:** một cú trống ngắn và sáu tháng trống tạo tác động khác nhau dù cùng gọi là vacancy.
- **Repairs:** sửa chữa định kỳ và một lần lớn nên tách; không chia đều một khoản bất thường rồi gọi là chi phí ổn định.

Thanh khoản là khả năng chuyển tài sản thành tiền với thời gian và chi phí chấp nhận được. Bất động sản thường cần quy trình tìm người mua, kiểm tra hồ sơ, thương lượng và hoàn tất thủ tục; thời gian thực tế thay đổi theo tài sản và thị trường. Vì chưa mở nguồn địa phương, bài không gắn số tháng, thuế chuyển nhượng, phí trước bạ, môi giới hay chiết khấu bán gấp nào cho Việt Nam. Chỉ cần ghi “chi phí và thời gian thanh khoản chưa biết” đã tốt hơn bịa một con số có vẻ chính xác.

Leverage tạo mismatch giữa thu nhập biến động và nghĩa vụ nợ theo lịch. Nếu vacancy kéo dài, rent giảm, repairs phát sinh hoặc lãi reset tăng, chủ tài sản vẫn có thể phải trả nợ. LTV nhìn vào giá trị tài sản; DSCR nhìn vào NOI và debt service; cả hai không đo toàn bộ sức chịu đựng của một hộ gia đình. Không dùng các ngưỡng DTI/DSCR phổ biến trên mạng như luật hoặc lời khuyên cá nhân trong bài này.

## 8. Known, Uncertain và Pending
**Known trong phạm vi bài:** các công thức được nêu; các kết quả VND suy ra từ assumption; base case có NOI dương và post-debt pre-tax cash flow âm; 0% growth là assumption không tăng giá. **Uncertain:** giá trị thị trường, mức thuê có thể thu, số tháng trống, chi phí sửa chữa, thanh khoản, lịch trả nợ thực tế và cost of capital của người sở hữu. **Pending:** luật đất đai, điều kiện giao dịch dự án, thuế/phí hiện hành, xử lý khấu trừ, bảo lãnh, điều kiện tín dụng, lãi suất sản phẩm và yêu cầu hồ sơ cụ thể.

Pending không có nghĩa claim chắc chắn sai; nó có nghĩa bài chưa có evidence đủ để nói như fact. Nếu mở được văn bản authoritative sau này, phải ghi URL, ngày truy cập, điều/khoản hoặc trang đã đọc, phạm vi và giới hạn vào ledger trước khi nâng trạng thái. Không dùng tiêu đề báo chí, quảng cáo, snippet tìm kiếm hoặc lời truyền miệng làm bằng chứng thay thế.

## 9. Retrieval tự kiểm tra
<details>
<summary>Retrieval 1 — mở để xem đáp án</summary>

Trong base case, gross yield theo tiền thuê 12 tháng là **7,20%**; doanh thu sau một tháng vacancy là **198.000.000 VND**; operating costs là **33.900.000 VND**; NOI là **164.100.000 VND**; cap rate theo giá mua là **5,47%**. Gross yield cao hơn cap rate vì cap rate đã trừ vacancy và chi phí vận hành.
</details>

<details>
<summary>Retrieval 2 — mở để xem đáp án</summary>

Base debt service giản lược là **252.000.000 VND/năm**, DSCR là **0,65x**, và cash flow trước thuế sau nợ là **−87.900.000 VND/năm**. Kết quả âm không bị che bởi giả định tăng giá 0%; nó chỉ cho biết tiền thuê theo mô hình không đủ trả nợ trong kỳ đã tính. Tax/fees không được gọi là post-tax vì chưa có nguồn authoritative mở.
</details>

## 10. Checklist thực hành an toàn
- [ ] Ghi giá mua, vốn tự có, khoản vay, LTV, lãi suất, tenor và phương pháp amortization.
- [ ] Ghi tiền thuê thực thu, vacancy theo tháng, maintenance, management fee và khoản chủ nhà chịu.
- [ ] Tách gross yield, effective gross income, NOI, cap rate, debt service và cash flow sau nợ.
- [ ] Ghi rõ giá mua hay giá trị thị trường làm mẫu số.
- [ ] Chạy ít nhất một kịch bản dòng tiền âm và một kịch bản sửa chữa lớn.
- [ ] Không gọi tax/fees là hiện hành nếu chưa mở và đọc văn bản authoritative.
- [ ] Kiểm tra pháp lý, quy hoạch, quyền sở hữu, điều kiện giao dịch và hợp đồng bằng nguồn/chuyên gia phù hợp trước mọi giao dịch thật.
- [ ] Không nhập dữ liệu tài chính cá nhân, không vay tiền và không giao dịch chỉ để hoàn thành bài.

## 11. Deep Dive: bài tập tự nguyện và câu hỏi phản biện
Người học có thể tự thay vacancy từ 1 lên 2 tháng, giữ các biến khác, rồi tính lại NOI và cash flow. Tiếp đó hãy viết hai câu: “Số nào là calculation?” và “Số nào cần nguồn hoặc báo giá thực tế?”. Một cách làm tốt sẽ không chỉ ghi kết quả mà còn ghi rounding, kỳ tính và biến bị giữ cố định.

Các câu hỏi phản biện:

1. Nếu người bán chỉ đưa gross yield, khoản nào chưa xuất hiện trong mẫu số hoặc tử số?
2. Nếu NOI dương nhưng DSCR dưới 1, rủi ro nào đang bị che bởi cách nói “có tiền thuê trả nợ”?
3. Nếu lãi suất được quảng cáo là ưu đãi, cần đọc thời gian ưu đãi, công thức reset, biên độ, phí và lịch trả gốc nào?
4. Nếu một mức thuế/phí được gửi qua tin nhắn, locator authoritative nào sẽ nâng nó từ pending thành usable claim?
5. Nếu giá tăng 10%, điều đó có tự làm cash flow cho thuê dương không? Không, vì tăng giá vốn và dòng tiền vận hành là hai trục khác nhau.

Không có câu trả lời nào ở trên là khuyến nghị. Một quyết định thực tế còn cần thẩm định hồ sơ, hợp đồng, chi phí cơ hội, dự phòng thanh khoản và tư vấn độc lập phù hợp.

## 12. Glossary và References
### 12.1 Glossary song ngữ
| Tiếng Việt | English | Định nghĩa ngắn |
|---|---|---|
| Đòn bẩy | Leverage | Dùng nợ cùng vốn tự có để kiểm soát tài sản lớn hơn vốn tiền mặt ban đầu; khuếch đại cả lời và lỗ. |
| Tỷ lệ khoản vay trên giá trị | Loan-to-Value (LTV) | Khoản vay chia cho giá trị tài sản được chọn trong mô hình. |
| Tỷ lệ bao phủ dịch vụ nợ | Debt Service Coverage Ratio (DSCR) | NOI chia cho debt service theo định nghĩa và kỳ tính đã công bố. |
| Lợi suất cho thuê gộp | Gross Rental Yield | Tiền thuê 12 tháng theo giả định chia cho giá mua hoặc giá trị tham chiếu, trước vacancy và chi phí. |
| Tỷ lệ/thời gian trống | Vacancy | Phần thời gian hoặc công suất không tạo doanh thu thuê theo mô hình. |
| Tỷ suất vốn hóa | Capitalization Rate (Cap Rate) | NOI chia cho giá mua hoặc giá trị tài sản; không phải lợi nhuận trên vốn tự có. |
| Thu nhập vận hành ròng | Net Operating Income (NOI) | Doanh thu thuê sau vacancy và operating costs, trước nợ và thuế thu nhập. |
| Dòng tiền | Cash Flow | Tiền vào trừ tiền ra trong một kỳ; phải ghi rõ trước/sau nợ và trước/sau thuế. |
| Chi phí vốn | Cost of Capital | Chi phí kinh tế của nợ và vốn tự có, gồm chi phí cơ hội theo khung phân tích. |
| Khấu hao khoản vay | Amortization | Lịch phân bổ các khoản trả nợ theo thời gian; bài dùng gốc đều để minh họa. |
| Vốn tự có | Equity | Phần vốn của chủ sở hữu trong giá trị tài sản sau khi trừ khoản nợ theo mô hình. |
| Dòng tiền trước thuế | Pre-tax Cash Flow | Dòng tiền sau các khoản đã mô hình hóa nhưng trước khoản thuế được xác minh/giả định. |
| Dòng tiền sau thuế | Post-tax Cash Flow | Dòng tiền sau thuế với căn cứ, chủ thể và kỳ tính được nêu rõ; W17 chưa tính được hợp lệ cho Việt Nam. |

### 12.2 Abbreviations
| Viết tắt | Mở rộng |
|---|---|
| LTV | Loan-to-Value |
| DSCR | Debt Service Coverage Ratio |
| NOI | Net Operating Income |
| Cap Rate | Capitalization Rate |

### 12.3 References và source boundary
- Các công thức và số VND trong mục 6 là **phép tính minh họa của bài học**, được kiểm tra lại độc lập; không phải số liệu thị trường.
- W17 không sử dụng claim luật, thuế, phí hoặc điều kiện tín dụng Việt Nam như evidence đã xác minh. Các nhóm này vẫn `pending`; các công thức LTV/DSCR và khái niệm underwriting nêu ở mục 4.1 chỉ được dùng trong phạm vi nguồn Fannie Mae đã ghi.
- Xem `quality/module-4-source-ledger.md` và `handoff/w17-handoff-2026-09-16.md` để biết claim inventory, trạng thái nguồn và giới hạn.

**Next steps:** [Quiz W17](../../assessments/quizzes/week-17-quiz.md) · [Answer key W17](../../assessments/answer-keys/week-17-answers.md)

## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

**Bài:** Bất động sản và nợ. **Đầu ra:** Thay vacancy base từ 1 thành 2 tháng, giữ các biến khác và mô hình nợ giản lược.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| Đầu vào | Thuê 18 triệu/tháng, bảo trì 24 triệu/năm, phí quản lý 5% tiền thực thu | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| NOI | Thực thu 180 triệu, phí quản lý 9 triệu, NOI 147 triệu/năm | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Dòng tiền | 147−252=−105 triệu/năm trước thuế theo mô hình giản lược | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| So sánh | NOI giảm 17,1 triệu so với base; debt service 252 triệu chưa là lịch amortization đầy đủ | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Kết luận | Tax/fees bị loại trừ, không gọi kết quả sau thuế hoặc suy giá tăng khiến cash flow dương | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
