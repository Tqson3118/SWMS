> [!CAUTION]
> **Tuyên bố miễn trách:** Bài học chỉ nhằm giáo dục và tham khảo, không phải lời khuyên tài chính, pháp lý hay thuế. Không có ticker, sản phẩm, thời điểm, tỷ trọng hoặc phân bổ nào được khuyến nghị. Quy định, biểu phí, prospectus và tình trạng thanh khoản có thể thay đổi; luôn đọc tài liệu hiện hành trước khi hành động.

# W14 — Trái phiếu, quỹ mở và ETF tại Việt Nam

## 1. Vì sao chủ đề này khó
**Prerequisite:** W13 — đọc ngành bằng bằng chứng, provenance và giới hạn dữ liệu. W14 dùng lại thói quen W13: khóa kỳ/ngày/định nghĩa, tách fact khỏi assumption và không biến ví dụ thành khuyến nghị.
Trái phiếu, quỹ mở và quỹ hoán đổi danh mục (ETF) có thể được đặt cạnh nhau trong một bài tập so sánh, nhưng claim cụ thể về cơ chế pháp lý, định giá và đường thoát của từng sản phẩm vẫn đang pending. Bảng phân biệt trong W14 chỉ là **quy trình đọc tài liệu/hypothetical**, không phải fact đã xác minh cho mọi sản phẩm. `[W14-C08]` Vì vậy không gọi bất kỳ loại nào là “gửi tiền có lãi” khi chưa xác định issuer/manager, điều khoản và tài liệu hiện hành.

Bài này có ngày kiểm tra nguồn là **2026-09-16**. Nguồn chính của phần hạ tầng là trang dịch vụ Bù trừ và Thanh toán của Tổng công ty Lưu ký và Bù trừ Chứng khoán Việt Nam (VSDC), đọc trực tiếp ngày đó. Trang này nêu giao dịch trái phiếu doanh nghiệp niêm yết thanh toán T+1, cổ phiếu/chứng chỉ quỹ/chứng quyền T+2, công cụ nợ T+1, và trái phiếu riêng lẻ T+0; trang cũng nêu nguyên tắc DVP và các quyết định quy chế liên quan `[W14-C03]`. Đây là mô tả nghiệp vụ, không phải cam kết lợi nhuận hay bảo lãnh nghĩa vụ của tổ chức phát hành.

## 2. Mục tiêu học tập đo được
Sau bài học, người học có thể:

- **[LO-W14-01]** Phân biệt bằng văn bản một trái phiếu chính phủ, trái phiếu được chính phủ bảo lãnh, trái phiếu chính quyền địa phương, trái phiếu doanh nghiệp, quỹ mở và ETF theo chủ thể phát hành/quản lý, cơ chế giao dịch, định giá và rủi ro chính; đạt ít nhất 5/6 tiêu chí trong bảng so sánh.
- **[LO-W14-02]** Giải thích riêng rẽ rủi ro tín dụng/default, lãi suất, thanh khoản và tracking error, đồng thời chỉ ra ít nhất một chỉ báo cần kiểm tra cho mỗi rủi ro.
- **[LO-W14-03]** Tính NAV trên một đơn vị, tiền coupon và giá trị sau phí trong một ví dụ VND bốn giai đoạn; làm lại khi thay đổi ít nhất hai giả định và nêu tác động.
- **[LO-W14-04]** Áp dụng một quy trình đọc prospectus/factsheet gồm identity, ngày hiệu lực/phiên bản, locator, phạm vi đọc và giới hạn; không biến tài liệu chưa mở/đọc thành một fact.

## 3. Fast Track: bản đồ cơ chế
Nếu chỉ có 15 phút, giữ bốn câu hỏi:

1. **Ai nợ ai?** Trong bài tập đọc hồ sơ, xác định issuer, nghĩa vụ coupon/gốc và điều khoản; không suy ra đặc điểm của issuer chưa đọc hoặc coi trái phiếu là tiền gửi.
2. **Giá được hình thành ở đâu?** Trong bài tập đọc tài liệu, đối chiếu cách hồ sơ mô tả định giá, NAV, nơi giao dịch và đường thoát của từng loại. W14 không xác nhận cơ chế quỹ mở/ETF cụ thể vì product mechanics vẫn pending.
3. **Rủi ro nào đang bị che?** Dùng checklist để tách credit/default, interest-rate, liquidity và tracking-error risk; các cơ chế hoặc mức đo cụ thể chỉ được ghi sau khi có tài liệu/locator đã mở.
4. **Nguồn nào đã thực sự được đọc?** VSDC đã được mở/đọc cho settlement; Investor.gov và FINRA đã được mở/đọc cho khái niệm NAV, ETF, phí, tracking error và duration. Prospectus/factsheet của một sản phẩm cụ thể, TT98 và quy tắc Việt Nam cụ thể vẫn phải đọc riêng; không dùng nguồn khái niệm chung để kết luận product fact.

## 4. Thuật ngữ song ngữ trước khi viết tắt
| Tiếng Việt | English | Ý nghĩa vận hành |
|---|---|---|
| Trái phiếu | Bond | **Trường cần đọc:** công cụ được hồ sơ mô tả là nghĩa vụ nợ và các điều khoản trả tiền; không phải xác nhận cho sản phẩm chưa mở. |
| Tổ chức phát hành | Issuer | **Trường cần đọc:** chủ thể huy động vốn và nghĩa vụ theo điều khoản phát hành trong tài liệu cụ thể. |
| Lãi coupon | Coupon interest | **Trường cần đọc:** khoản lãi và công thức theo điều khoản phát hành; W14 không xác nhận điều khoản của sản phẩm cụ thể. |
| Lợi suất đến đáo hạn | Yield to maturity | Mức lợi suất quy đổi theo giả định giữ đến đáo hạn và dòng tiền được tái đầu tư theo giả định; không phải bảo đảm. |
| Giá trị tài sản ròng | Net asset value | Tài sản quỹ trừ nợ phải trả. |
| Giá trị tài sản ròng trên một đơn vị | NAV per unit | **Công thức minh họa trong W14:** NAV tổng chia số đơn vị; cách định giá thực tế phải đọc trong tài liệu quỹ. |
| Quỹ tương hỗ mở | Open-end mutual fund | **Khái niệm chung đã đối chiếu:** Investor.gov mô tả nhà đầu tư mua/bán quỹ mở theo NAV mỗi đơn vị, thường tính cuối ngày; cơ chế, cut-off và phí của từng quỹ vẫn phải đọc prospectus. |
| Quỹ hoán đổi danh mục | Exchange-traded fund | **Khái niệm chung đã đối chiếu:** Investor.gov mô tả ETF giao dịch trên sàn trong ngày ở giá thị trường, có thể cao/thấp hơn NAV; chỉ ETF/sản phẩm cụ thể mới cần prospectus riêng. |
| Phí quản lý | Management fee | **Khái niệm chung đã đối chiếu:** SEC Investor Bulletin yêu cầu đọc prospectus fee table; expense ratio, shareholder fees và brokerage costs khác nhau, không có mức phí chung cho mọi quỹ. |
| Sai lệch bám chỉ số | Tracking error | **Khái niệm chung đã đối chiếu:** Investor.gov Index Funds mô tả tracking error là quỹ không bám hoàn hảo chỉ số; phí, chi phí giao dịch và sampling có thể góp phần vào sai lệch. Mức đo của từng quỹ vẫn cần tài liệu sản phẩm. |
| Chênh lệch giá mua-bán | Bid-ask spread | **Trường cần đọc:** cách dữ liệu/tài liệu xác định khoảng cách giá mua-bán và chi phí giao dịch; chưa phải số liệu sản phẩm. |
| Thời gian thanh toán | Settlement cycle | Khoảng thời gian từ khớp giao dịch đến hoàn tất chuyển tiền/chứng khoán. |

**Viết tắt dùng sau khi đã giải nghĩa:** NAV (Net Asset Value), ETF (Exchange-Traded Fund), YTM (Yield to Maturity), DVP (Delivery Versus Payment), SSC/UBCKNN (State Securities Commission/Ủy ban Chứng khoán Nhà nước), VSDC (Vietnam Securities Depository and Clearing Corporation), benchmark (chỉ số tham chiếu). Các định nghĩa cơ chế của NAV/ETF/YTM ngoài phần tính toán minh họa phải được kiểm tra trong tài liệu sản phẩm hoặc nguồn đã mở.

## 5. Trái phiếu: loại và thứ tự ưu tiên cần kiểm tra
W14 chưa mở/đọc một bộ hồ sơ phát hành cụ thể. Vì vậy các mục dưới đây là trường cần kiểm tra trong văn bản chính thức, không phải xác nhận pháp lý cho mọi trái phiếu:

**Checklist hồ sơ:** loại trái phiếu, issuer, nghĩa vụ trả tiền, bảo lãnh/bảo đảm, thứ tự ưu tiên, quyền mua lại/chuyển đổi, điều kiện phát hành, nguồn trả nợ và locator của từng điều khoản. Không gắn nhãn “an toàn” từ tên loại tài sản hoặc coupon.

VSDC hiện mô tả các luồng thanh toán khác nhau: trái phiếu doanh nghiệp niêm yết T+1; công cụ nợ T+1; trái phiếu riêng lẻ T+0 `[W14-C03]`. T+0/T+1 là chu kỳ hoàn tất giao dịch, không phải thời gian tổ chức phát hành trả coupon hoặc gốc. VSDC cũng nêu DVP: chuyển giao chứng khoán khi và chỉ khi thanh toán tiền hoàn tất `[W14-C03]`; đây là mô tả cơ chế thanh toán, không phải xác nhận khả năng trả nợ của issuer.

## 6. Bốn rủi ro phải tách riêng khi đọc nguồn đã mở
Các nhãn dưới đây là checklist phân tích; W14 không dùng chúng như claim đã xác minh từ CFA hoặc một prospectus cụ thể.
**Credit/default risk:** khi tài liệu có nêu, ghi nghĩa vụ coupon/gốc, lịch đáo hạn, covenant, thứ tự ưu tiên, tài sản bảo đảm và sự kiện vi phạm cùng locator. Không suy ra khả năng thu hồi từ nhãn bảo đảm.

**Interest-rate risk và duration:** FINRA mô tả duration (tính theo năm) là chỉ báo mức giá khoản đầu tư trái phiếu có thể biến động khi lãi suất thay đổi; Investor.gov giải thích lãi suất tăng thường làm giá trái phiếu giảm và kỳ hạn dài nhạy hơn. Đây là khái niệm chung, không phải dự báo giá Việt Nam.

**Liquidity risk:** ghi số ngày giao dịch, bid-ask, quy mô lô, hạn chế chuyển nhượng và cửa sổ mua lại nếu nguồn đã mở có nêu; nếu không có, đánh dấu **Uncertain**.

**Reinvestment và call risk:** kiểm tra tài liệu có nêu lịch dòng tiền, quyền mua lại và giả định YTM hay không; không dùng YTM như lời hứa.

Đối với quỹ/ETF, khái niệm NAV, premium/discount, phí và tracking error đã có nguồn giáo dục SEC/Investor.gov; timestamp, spread, premium/discount thực tế, phí và tracking error của từng sản phẩm vẫn là **Uncertain** nếu chưa có prospectus/factsheet. `[W14-C08]`

## 7. Quỹ mở so với ETF: phạm vi được phép nói
W14 không có prospectus/factsheet đã mở và đọc của một sản phẩm cụ thể. Do đó bài không xác nhận cut-off, NAV, phí, creation/redemption, benchmark, tracking error hoặc cơ chế mua/bán lại của bất kỳ quỹ nào. Các mục này là checklist phải lấy trực tiếp từ tài liệu có tên sản phẩm, issuer/manager, phiên bản, ngày hiệu lực và locator `[W14-C08]`.

Các câu hỏi về ETF trong retrieval block chỉ là bài tập phương pháp/hypothetical, không phải mô tả đã xác minh về một ETF thật. Không dùng chúng để kết luận “mua rẻ”, chất lượng mô phỏng hay mức phí.

| Câu hỏi cần đối chiếu khi đọc tài liệu | Quỹ mở — hypothetical/document-reading procedure | ETF — hypothetical/document-reading procedure |
|---|---|---|
| Cách vào/ra | Kiểm tra tài liệu có mô tả mua lại với quỹ hay không | Kiểm tra tài liệu có mô tả đặt lệnh trên sàn, creation/redemption và thanh khoản hay không |
| Giá giao dịch | Kiểm tra cách tài liệu xác định giá/NAV và thời điểm định giá | Kiểm tra giá thị trường, NAV tham chiếu và cách đo premium/discount |
| Rủi ro nổi bật | Kiểm tra cut-off, phí, thanh khoản tài sản và định giá | Kiểm tra spread, premium/discount, tracking error, thanh khoản sàn và tài sản |
| Tài liệu cần đọc | Prospectus, điều lệ, phí, lịch giao dịch | Prospectus, benchmark methodology, phí, creation/redemption, factsheet |

## 8. NAV, phí, tracking error và thuế/quy định: chỉ dẫn kiểm tra
Phần này chỉ dạy cách ghi nhận tài liệu. Khi có một prospectus/factsheet đã mở, ghi ngày hiệu lực, phiên bản, phương pháp định giá, tài sản được phép nắm giữ, giới hạn tập trung, bên lưu ký, công ty quản lý quỹ, cơ chế công bố NAV và điều kiện tạm ngừng/hoãn giao dịch. Không có tài liệu đã mở thì các trường đó là **Uncertain**, không điền bằng suy đoán.

Phí, NAV và tracking error phải được chép từ đúng tài liệu, cùng ngày hiệu lực và locator. W14 không có claim verified về mức phí, thuế, benchmark hoặc tracking error của sản phẩm cụ thể; chỉ các khái niệm chung và yêu cầu đọc fee table là đã được source-bounded.

Về pháp lý, bài không khẳng định mức thuế cụ thể cho mọi nhà đầu tư. Cách xử lý có thể phụ thuộc tư cách cư trú, loại thu nhập, loại chứng khoán/quỹ, thời điểm và văn bản hiệu lực. Người học phải kiểm tra văn bản thuế hiện hành, quy định SSC, tài liệu sàn/VSDC và prospectus. Nguồn SSC trong phiên này không hiển thị toàn văn vì cổng yêu cầu JavaScript; ledger ghi rõ phạm vi này. Vì vậy, phần pháp lý chỉ dùng để dạy cách kiểm tra, không thay thế tư vấn.

## 9. Deep Dive: cách đọc prospectus và market document
Đọc theo thứ tự sau, đánh dấu **Fact**, **Hypothetical**, **Known**, **Uncertain**:

1. **Identity and mandate:** tên pháp lý, loại quỹ/trái phiếu, issuer/manager/custodian, mục tiêu và benchmark.
2. **Cash-flow mechanics:** coupon, ngày trả, đáo hạn, quyền mua lại, mua/bán lại, creation/redemption và cut-off.
3. **Valuation:** ngày NAV, tài sản nào dùng giá thị trường, tài sản nào dùng mô hình, cách xử lý ngày nghỉ và sai số.
4. **Cost:** phí hiện tại, phí tối đa, phí đã bao gồm trong NAV, chi phí giao dịch và thuế chưa biết.
5. **Risk and liquidity:** credit quality, duration, concentration, spread, hạn chế chuyển nhượng, tạm ngừng giao dịch, cơ chế xử lý stress.
6. **Disclosure date:** ngày ban hành, ngày hiệu lực, phiên bản prospectus, trang/điều/mục và URL. Không dùng một factsheet cũ để kết luận quy định hôm nay.

<details>
<summary>Retrieval 1 — tự trả lời trước khi mở đáp án</summary>

Một trái phiếu doanh nghiệp có coupon 9% nhưng không có giao dịch thứ cấp trong 60 ngày. Hai câu hỏi nào phải hỏi trước khi gọi 9% là “lợi suất an toàn”?

**Đáp án ẩn:** Không kết luận “an toàn” từ 9%. Mở tài liệu phù hợp, ghi locator cho coupon, dòng tiền, phí, thanh khoản, bảo đảm và giới hạn; trường chưa đọc phải giữ **Uncertain**.
</details>

<details>
<summary>Retrieval 2 — tự trả lời trước khi mở đáp án</summary>

ETF có NAV 20.000 VND nhưng giao dịch 19.600 VND. Có thể kết luận “mua rẻ 2%” không?

**Đáp án ẩn:** Đây chỉ là phép tính hypothetical: 19.600/20.000 - 1 = -2%. Không kết luận discount là bargain; các trường NAV, spread, thanh khoản và tracking error phải được đọc từ tài liệu sản phẩm, nếu chưa thì giữ **Uncertain**.
</details>

## 10. Worked example VND bốn giai đoạn
Đây là **Hypothetical**, không dùng ticker, issuer, quỹ hay số liệu thị trường thật.

**Giai đoạn 1 — khóa giả định và NAV.** Một học viên giả định mua mệnh giá 100.000.000 VND trái phiếu doanh nghiệp, coupon 8%/năm, trả một lần cuối năm, kỳ hạn một năm, giá mua bằng mệnh giá. Giả định không phí, không thuế, không tái đầu tư, không default và giữ đến đáo hạn. Đây là bộ giả định thuận lợi, không phải fact.

Để luyện đúng **[LO-W14-03]**, coi khoản đầu tư là một tài sản giả định được chia thành **10.000 đơn vị**. NAV trên một đơn vị được tính từng bước: (1) tài sản giả định ban đầu = 100.000.000 VND; (2) nợ/phí chưa thanh toán = 0 VND; (3) NAV tổng = tài sản - nợ = 100.000.000 VND; (4) NAV/đơn vị = NAV tổng / số đơn vị = 100.000.000 / 10.000 = **10.000 VND/đơn vị**. Đây là công thức và số học minh họa `[W14-C05]`, không phải NAV của quỹ thật.

| Giai đoạn | Tài sản (VND) | Nợ/phí (VND) | NAV tổng (VND) | Số đơn vị | NAV/đơn vị (VND) |
|---|---:|---:|---:|---:|---:|
| Ban đầu | 100.000.000 | 0 | 100.000.000 | 10.000 | 10.000 |
| Sau coupon gộp | 108.000.000 | 0 | 108.000.000 | 10.000 | 10.800 |
| Sau phí và thuế minh họa | 107.450.000 | 0 | 107.450.000 | 10.000 | 10.745 |
| Nhận gốc và coupon ròng | 0 | 0 | 0 | 10.000 | 0 |

Trong bảng, “NAV” chỉ là cách trình bày giá trị giả định trên đơn vị; ở dòng cuối tài sản đã được thanh toán ra nên không còn NAV đang nắm giữ. Nếu thay số đơn vị thành 20.000, NAV/đơn vị ban đầu sẽ là 5.000 VND; tổng NAV không đổi. Nếu thêm nợ/phí chưa thanh toán 150.000 VND, NAV/đơn vị ban đầu sẽ là (100.000.000 - 150.000) / 10.000 = **9.985 VND**. Hai thay đổi này là sensitivity của công thức, không phải mechanic sản phẩm.

**Giai đoạn 2 — tính dòng tiền.** Coupon = 100.000.000 x 8% = **8.000.000 VND** `[W14-C05]`. Nếu hoàn trả đủ gốc, cuối kỳ nhận 108.000.000 VND; lợi suất danh nghĩa trên giá mua = 8%. Không dùng kết quả này để khẳng định YTM hay điều khoản của sản phẩm thật.

**Giai đoạn 3 — đưa phí/thuế giả định vào.** Giả định thêm phí mua 0,15% trên 100.000.000 = 150.000 VND và một khoản thuế minh họa 5% trên coupon = 400.000 VND. Tiền bỏ ra = 100.150.000; tiền nhận ròng cuối kỳ = 100.000.000 + 8.000.000 - 400.000 = **107.600.000 VND**. Lãi ròng so với tiền bỏ ra = 7.450.000 VND, tỷ lệ = 7.450.000 / 100.150.000 = **7,439%** `[W14-C06]`. Khoản thuế là giả định minh họa, không phải mức hiện hành áp cho mọi người.

**Giai đoạn 4 — sensitivity và giới hạn.** Nếu giá mua là 98.000.000 thay vì 100.000.000, giữ các giả định khác, phí = 147.000 và tiền bỏ ra = 98.147.000; tiền nhận ròng vẫn 107.600.000, lãi = 9.453.000, tỷ lệ = 9.453.000 / 98.147.000 = **9,631%**. Nếu coupon chỉ nhận 4.000.000 do một nửa kỳ coupon bị chậm trong kịch bản giả định, tiền ròng = 103.600.000; trên chi phí 100.150.000, lãi = 3.450.000 và tỷ lệ = **3,445%** `[W14-C06]`. Hai thay đổi cho thấy giá mua và dòng tiền thực nhận quan trọng hơn việc đọc riêng coupon; chúng không phải dự báo hoặc mechanic sản phẩm.

Known: công thức và số học của ví dụ. Uncertain: khả năng trả nợ, giá bán trước hạn, thuế thật, phí thật, lạm phát, tái đầu tư, định giá và tình trạng pháp lý của một sản phẩm cụ thể. Không chuyển kết quả thành forecast hoặc allocation.

## 11. Bài tập, lỗi thường gặp và checklist
Lỗi 1 là gọi trái phiếu doanh nghiệp là tiền gửi. Lỗi 2 là dùng coupon để thay cho YTM. Lỗi 3 là nghĩ DVP bảo vệ khỏi default. Lỗi 4 là so NAV quỹ mở với giá ETF mà bỏ qua thời điểm và cơ chế. Lỗi 5 là gọi tracking error thấp trong một kỳ là bằng chứng đủ. Lỗi 6 là dùng prospectus cũ hoặc mức thuế minh họa như quy định hiện hành.

Checklist trước khi ghi kết luận:

- [ ] Tôi đã ghi issuer/manager/custodian, loại tài sản và thứ tự ưu tiên chưa?
- [ ] Tôi đã tách credit, interest-rate, liquidity, reinvestment và tracking risk chưa?
- [ ] Tôi có ngày truy cập, ngày hiệu lực, phiên bản, điều/mục/trang và URL không?
- [ ] Tôi đã biết giá là coupon, YTM, NAV hay market price chưa?
- [ ] Tôi đã tính phí/thuế theo tài liệu hiện hành, chưa dùng số minh họa như fact chưa?
- [ ] Tôi đã chạy kịch bản xấu và ghi điều chưa biết chưa?
- [ ] Tôi có đang đưa ra ticker, sản phẩm, thời điểm hoặc tỷ trọng không? Nếu có, xóa khỏi bài học.

## 12. Tài liệu, ngày hiệu lực và giới hạn nguồn
1. `[W14-C03]` VSDC, **Bù trừ và Thanh toán**, đọc 2026-09-16, https://www.vsdc.vn/sd/XAz40d2Q-9j569TvBgLQaQ. Phần “Bù trừ, thanh toán giao dịch chứng khoán cơ sở”: T+1 trái phiếu doanh nghiệp niêm yết; T+2 cổ phiếu/chứng chỉ quỹ/chứng quyền; T+1 công cụ nợ; T+0 trái phiếu riêng lẻ; nguyên tắc DVP; dẫn Quyết định 39/QĐ-HĐTV ngày 29/04/2025, Quyết định 27/QĐ-HĐTV ngày 10/08/2023 và Quyết định 25/QĐ-HĐTV ngày 16/04/2025. Trang nghiệp vụ không thay thế toàn văn quy chế và không mô tả mọi trường hợp ngoại lệ.
2. `[W14-C08]` Bộ Tài chính, Thông tư 98/2020/TT-BTC, **quy định hoạt động và quản lý quỹ đầu tư chứng khoán**, Công báo, URL tra cứu đã thử ngày 2026-09-16 nhưng endpoint trả nhầm văn bản 23/VBHN-BGTVT; vì vậy không dùng số điều/trang chưa đọc được để kết luận. Cần mở bản chính thức trước khi dùng cho tư vấn pháp lý.
3. `[W14-C08]` SSC/UBCKNN, cổng văn bản pháp luật, https://ssc.gov.vn/webcenter/portal/ubck/pages_r/m/vanbanphapluat, thử đọc 2026-09-16; cổng yêu cầu JavaScript trong kênh truy xuất này. Các claim về luật/quy chế trong bài bị giới hạn ở phạm vi ledger, không khẳng định toàn bộ văn bản hiện hành.
4. FINRA, “Bonds”, https://www.finra.org/investors/investing/investment-products/bonds, đọc 2026-10-06: duration là chỉ báo năm về độ nhạy giá với lãi suất. Investor.gov, “Fixed Income Investments — When Interest Rates Go Up, Prices of Fixed-Rate Bonds Fall”, https://www.investor.gov/introduction-investing/general-resources/news-alerts/alerts-bulletins/investor-bulletins-86, đọc 2026-10-06: quan hệ lãi suất–giá và kỳ hạn. Các nguồn này hỗ trợ khái niệm chung, không phải sản phẩm Việt Nam hay dự báo.
5. Tài liệu prospectus/factsheet của quỹ phải được đọc theo từng sản phẩm và phiên bản. W14 không chọn hoặc giới thiệu sản phẩm/ticker và không có ví dụ official product document đã mở/đọc; vì vậy LO-W14-04 chỉ dạy quy trình đọc và ghi provenance.

**Tóm tắt trạng thái:** Fact đã kích hoạt trong W14 gồm settlement VSDC `[W14-C03]`, khái niệm chung NAV/quỹ/ETF/phí/tracking error/duration từ SEC/Investor.gov/FINRA `[W14-C01][W14-C02][W14-C04][W14-C07]`, và phép tính minh họa `[W14-C05][W14-C06]`; uncertain/pending là claim về sản phẩm cụ thể, giá tương lai, thuế áp dụng, phí thực tế, thanh khoản, tracking error thực tế và tính phù hợp cá nhân.

**Bài kiểm tra:** [Quiz W14](../../assessments/quizzes/week-14-quiz.md) · [Answer key W14](../../assessments/answer-keys/week-14-answers.md)

## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

**Bài:** Trái phiếu và quỹ. **Đầu ra:** So sánh trái phiếu, quỹ mở và ETF giả định bằng checklist ở mục 11.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| Sản phẩm | Tách issuer, manager, custodian và quyền lợi | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Rủi ro | Tách credit, interest-rate, liquidity, reinvestment và tracking | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Giá/thu nhập | Phân biệt coupon, YTM, NAV và market price | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Nguồn | Ghi phiên bản, ngày hiệu lực, locator; chưa mở thì pending | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Kết luận | Có stress case và dữ kiện thiếu; không coi DVP hoặc tracking error thấp là bảo đảm | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
