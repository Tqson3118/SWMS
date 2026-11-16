> [!CAUTION]
> **Tuyên bố miễn trách:** Bài học chỉ nhằm mục đích giáo dục, không phải lời khuyên tài chính, pháp lý hay thuế. Không có dự báo giá, ticker, lệnh mua bán, đòn bẩy, tỷ trọng mặc định hoặc kết luận phù hợp cho một cá nhân. Crypto và digital assets có thể mất một phần hoặc toàn bộ giá trị; người học phải tự kiểm tra nguồn chính thức và quy định áp dụng trước khi hành động.

# W16 — Crypto và digital assets: công nghệ, sản phẩm, tài sản đầu cơ và tài chính phi tập trung (DeFi)

## 1. Prerequisite, phạm vi và cách đọc claim
**Prerequisite:** W15 — phân biệt observed fact, model, interpretation và uncertainty; không biến một tín hiệu thành kết luận nhân quả. W16 áp dụng cùng kỷ luật đó cho crypto và digital assets. Mục tiêu không phải học cách chọn token, dự đoán giá hay tối ưu lợi suất. Mục tiêu là nhận diện hệ thống, quyền lợi, điểm phụ thuộc và các cách một người có thể mất quyền kiểm soát hoặc mất vốn.

Trong bài này, mỗi câu nên được gắn một loại claim: **Technology** (cơ chế kỹ thuật), **Product/service** (sản phẩm hoặc dịch vụ có điều khoản riêng), **Speculative asset** (tài sản có giá thị trường biến động và có thể được mua vì kỳ vọng giá), **Risk mechanism** (cách rủi ro truyền qua hệ thống), hoặc **Hypothetical** (bài tập). Một blockchain có thể là công nghệ; một ví lưu ký là dịch vụ; một token có thể là tài sản đầu cơ; một stablecoin có thể vừa là token vừa là sản phẩm phát hành theo một cơ chế dự trữ. Không được suy ra rằng công nghệ tốt làm sản phẩm an toàn, hay nhãn “stable” làm tài sản không thể mất giá.

BIS mô tả **tài chính phi tập trung (Decentralized Finance, sau đây viết tắt là DeFi)** như các ứng dụng tài chính chạy bằng **hợp đồng thông minh (smart contract)** trên blockchain, thường dùng stablecoin để chuyển tiền; BIS cũng nhấn mạnh governance, leverage, liquidity mismatch và interconnectedness. FSB đưa ra khuyến nghị quốc tế cho global stablecoin arrangements, nhưng chính FSB nói việc áp dụng thuộc thẩm quyền và khuôn khổ của từng jurisdiction. Ethereum.org là nguồn kỹ thuật cho smart contract và oracle trong phạm vi Ethereum. Các nguồn không mở được không được dùng như bằng chứng đã xác minh.

## 2. Mục tiêu học tập đo được
Sau bài học, người học có thể:

- **[LO-W16-01]** Định nghĩa bằng ngôn ngữ của mình blockchain, token, stablecoin, smart contract, DeFi, oracle và custody trước khi dùng các chữ viết tắt; phân loại ít nhất 10 mô tả thành technology, product/service, speculative asset, risk mechanism hoặc hypothetical với ít nhất 8/10 đúng.
- **[LO-W16-02]** Vẽ hoặc viết một chuỗi phụ thuộc gồm asset, wallet/key, custodian, smart contract, oracle và governance; chỉ ra ít nhất bốn điểm có thể hỏng mà không gọi chúng là cùng một loại rủi ro.
- **[LO-W16-03]** Phân biệt volatility, liquidity, counterparty, custody/key loss, smart-contract/oracle/governance, legal/regulatory và scam/phishing risk trong hai hypothetical scenarios, nêu được ít nhất một fact, một unknown và một câu hỏi xác minh cho mỗi lớp.
- **[LO-W16-04]** Áp dụng risk-assessment checklist không dự báo giá, không hướng dẫn trading/leverage và không mặc định suitability; viết được kết luận “known/uncertain/pending” có provenance cho một digital-asset case.

## 3. Fast Track: bảy câu hỏi trước khi tin một claim
Nếu chỉ có 15 phút, không bắt đầu bằng biểu đồ giá. Hãy hỏi:

1. **Đây là gì?** Công nghệ, sản phẩm/dịch vụ hay tài sản đầu cơ? “Blockchain” không phải cùng một thứ với “token”; “ví” không phải cùng một thứ với “tài sản”.
2. **Ai có quyền gì?** Token có quyền sử dụng, quyền biểu quyết, quyền đòi hỏi tài sản hoặc không có quyền nào ngoài khả năng chuyển giao? Không suy ra quyền của cổ phiếu hay tiền gửi nếu tài liệu không nói vậy.
3. **Ai giữ hoặc điều khiển khóa?** Custody có thể nằm ở người dùng, bên cung cấp dịch vụ hoặc cấu trúc nhiều bên. Mất private key, sai địa chỉ hoặc bên lưu ký mất khả năng vận hành là các failure mode khác nhau.
4. **Code phụ thuộc dữ liệu nào?** Smart contract có thể cần oracle; oracle cần nguồn dữ liệu, node, quy tắc tổng hợp và thời gian cập nhật.
5. **Thanh khoản đến từ đâu?** “Có thể giao dịch” không đồng nghĩa luôn có người mua ở giá mong muốn. Hỏi thị trường nào, độ sâu nào, điều kiện rút nào và spread nào; không dùng số liệu chưa mở.
6. **Đối tác nào phải thực hiện lời hứa?** Issuer, exchange, custodian, validator, oracle operator, governance voter và nhà cung cấp giao diện có thể tạo counterparty hoặc operational dependency.
7. **Điều gì là known, uncertain hoặc pending?** Ghi nguồn, ngày truy cập, phạm vi đọc và giới hạn. Không thay uncertainty bằng một nhãn marketing.

## 4. Glossary song ngữ trước khi viết tắt
| Tiếng Việt | English | Ý nghĩa vận hành trong W16 |
|---|---|---|
| Chuỗi khối | Blockchain | Sổ cái dữ liệu được duy trì bằng cơ chế mạng lưới và quy tắc xác thực; thiết kế cụ thể quyết định ai có thể ghi/đọc và cách đạt đồng thuận. |
| Tài sản số | Digital asset | Tên nhóm rộng cho giá trị hoặc quyền được biểu diễn, lưu chuyển hay ghi nhận bằng công nghệ số; phân loại pháp lý phụ thuộc jurisdiction và đặc điểm cụ thể. |
| Token | Token | Đơn vị dữ liệu/quyền được phát hành hoặc ghi nhận trên một blockchain; quyền kinh tế, quyền sử dụng và khả năng chuyển nhượng phải đọc từ tài liệu cụ thể. |
| Stablecoin | Stablecoin | Cryptoasset cố gắng neo giá vào tiền pháp định hoặc tham chiếu khác bằng một cơ chế; “stable” là mục tiêu thiết kế, không phải bảo đảm giá trị hay thanh khoản. |
| Hợp đồng thông minh | Smart contract | Chương trình lưu trên blockchain thực thi logic theo code khi điều kiện và giao dịch phù hợp; không tự động đồng nghĩa với hợp đồng pháp lý. |
| Tài chính phi tập trung | Decentralized finance | Các ứng dụng tài chính chạy bằng smart contract trên blockchain; mức độ phi tập trung, governance và phụ thuộc tập trung cần kiểm tra riêng. |
| Oracle | Oracle | Thành phần đưa dữ liệu ngoài blockchain vào smart contract hoặc truyền kết quả ra ngoài; có rủi ro correctness, availability và incentive. |
| Lưu ký | Custody | Việc giữ, bảo vệ hoặc thực hiện quyền kiểm soát tài sản/khóa thay cho chính người sở hữu hoặc theo cơ chế thỏa thuận. |
| Khóa riêng | Private key | Dữ liệu mật dùng để ký quyền kiểm soát tài khoản hoặc giao dịch; lộ, mất hoặc ký nhầm có thể dẫn đến mất quyền kiểm soát. |
| Đối tác | Counterparty | Bên phải thực hiện một nghĩa vụ hoặc cung cấp một dịch vụ; rủi ro phát sinh khi bên đó không thực hiện, bị vỡ nợ, bị hạn chế hoặc bị tấn công. |
| Thanh khoản | Liquidity | Khả năng chuyển đổi hoặc giao dịch với mức trượt giá, thời gian và chi phí chấp nhận được; không chỉ là việc có một mức giá hiển thị. |
| Biến động | Volatility | Mức độ dao động của giá hoặc giá trị theo thời gian; không phải thước đo duy nhất của khả năng mất vốn hay khả năng bán. |
| Quản trị | Governance | Cấu trúc và quy trình quyết định thay đổi tham số, code, treasury, oracle hoặc quyền vận hành của hệ thống. |
| Phí mạng | Gas fee | Chi phí trả cho việc xử lý giao dịch trên một số blockchain; phụ thuộc chain, tình trạng mạng và giao dịch cụ thể. |

**Viết tắt chỉ dùng sau khi giải nghĩa:** DeFi (Decentralized Finance), CEX (Centralized Exchange), DEX (Decentralized Exchange), API (Application Programming Interface), AML/CFT (Anti-Money Laundering/Countering the Financing of Terrorism), GSC (Global Stablecoin). Chữ viết tắt không thay thế định nghĩa.

## 5. Blockchain, token và ranh giới giữa công nghệ với tài sản
Blockchain là một cách tổ chức dữ liệu và xác thực trạng thái giữa nhiều máy tính theo một bộ quy tắc. Nó có thể ghi lại giao dịch hoặc trạng thái của token, nhưng blockchain không tự quyết định token đại diện cho quyền gì ngoài dữ liệu và quy tắc của hệ thống. Một token có thể được dùng để truy cập tính năng, biểu quyết, thanh toán trong một hệ thống, đại diện cho một quyền theo tài liệu phát hành, hoặc chỉ được thị trường mua bán vì kỳ vọng giá. Các khả năng đó không tương đương nhau.

Phân biệt ba câu hỏi:

- **Technology:** code, consensus, dữ liệu on-chain, khả năng nâng cấp, thông lượng và cách node xác thực.
- **Product/service:** giao diện, exchange, custodian, lending protocol, stablecoin issuer hoặc oracle service; các điều khoản vận hành và quyền truy cập là riêng.
- **Speculative asset:** token được định giá trên thị trường và có thể dao động mạnh; giá thị trường không chứng minh utility, doanh thu, reserve hoặc quyền pháp lý.

Ví dụ: “mã nguồn công khai” là một dữ kiện về khả năng kiểm tra code, không phải chứng cứ rằng code không lỗi. “Giao dịch được ghi trên chain” là một thuộc tính ghi nhận, không phải cam kết rằng bên nhận có thể đổi ra tiền pháp định. “Token có governance” là quyền tham gia một quy trình biểu quyết cụ thể, không tự động là quyền sở hữu công ty.

**Chi tiết retrieval 1:** Một token có thể nằm trên blockchain và được chuyển giao tự động, nhưng người học vẫn phải hỏi quyền mà issuer hoặc giao thức cam kết, ai có thể thay đổi code, và việc chuyển giao có được công nhận trong một jurisdiction cụ thể hay không. Công nghệ và quyền pháp lý không phải một claim.

## 6. Stablecoin: mục tiêu ổn định không phải bảo hiểm
Stablecoin thường được thiết kế để cố gắng giữ giá tham chiếu, chẳng hạn một đơn vị tiền pháp định, bằng reserve, collateral, cơ chế phát hành/đổi lại hoặc thuật toán. BIS nhấn mạnh thiết kế khác nhau tạo profile rủi ro khác nhau: reserve có thể là tiền gửi, tài sản ngắn hạn, commercial paper hoặc cryptoasset; cơ chế on-chain có thể dựa vào overcollateralisation; cơ chế thuật toán điều chỉnh cung.

Checklist đọc stablecoin không hỏi “tên có stable không?” mà hỏi:

1. Ai phát hành, ai có thể mint/burn và ai có quyền đóng băng?
2. Reserve gồm gì, được lưu ở đâu, ai kiểm tra và có mismatch thanh khoản nào?
3. Người dùng có quyền redeem với issuer hay chỉ có thể bán trên thị trường thứ cấp?
4. Có cơ chế overcollateralisation, thuật toán, governance hay kết hợp? Điều gì xảy ra khi collateral giảm nhanh?
5. Có phụ thuộc ngân hàng, exchange, oracle, bridge hoặc chain khác không?
6. Quy định nào áp dụng cho người dùng và issuer trong jurisdiction liên quan? Câu hỏi này phải giữ pending nếu chưa mở nguồn chính thức hiện hành.

FSB khuyến nghị các nhà chức trách yêu cầu governance, risk management, cyber security, AML/CFT, disclosures, recovery/resolution và redemption/stabilisation cho global stablecoin arrangements. Đây là khung khuyến nghị cho authorities, không phải lời hứa rằng một stablecoin cụ thể đã đáp ứng hoặc người dùng ở mọi nơi có quyền redeem.

## 7. Smart contract, DeFi và governance: tự động không có nghĩa là an toàn
Smart contract là chương trình lưu trên blockchain, thực thi logic “nếu điều kiện này thì hành động kia” theo code. Ethereum.org giải thích ưu điểm của execution deterministic và public record, nhưng cũng cảnh báo người dùng có thể blind-sign dữ liệu giao dịch khó đọc. Một smart contract có thể tự động hóa; nó không tự đọc được ý định của người dùng, không tự sửa code lỗi và không tự biến lời hứa kinh tế thành quyền pháp lý.

DeFi là lớp ứng dụng tài chính dùng các contract và token. Một giao thức có thể cung cấp swap, lending, collateral hoặc portfolio automation, nhưng mỗi chức năng thêm dependency. BIS gọi attention đến “decentralisation illusion”: governance vẫn cần người/nhóm quyết định chiến lược và vận hành; quyền có thể tập trung ở developer, holder lớn, validator hoặc multisig. Vì vậy, risk assessment phải hỏi ai có thể nâng cấp contract, đổi oracle, thay fee, dừng thị trường, di chuyển treasury hoặc xử lý sự cố.

Không mô tả smart contract như “không cần trust” một cách tuyệt đối. Trust có thể chuyển từ nhân sự sang code, compiler, admin key, oracle, validator, interface, bridge, reserve manager và governance process. Mỗi thành phần có failure mode riêng.

## 8. Oracle và rủi ro dữ liệu
Blockchain chỉ xử lý dữ liệu mà mạng lưới có thể xác thực theo quy tắc của nó. Oracle đưa dữ liệu off-chain như giá, kết quả sự kiện hoặc dữ liệu thời tiết vào contract. Ethereum docs nêu ba tiêu chí chính: **correctness** (dữ liệu đúng và nguyên vẹn), **availability** (dữ liệu sẵn sàng đúng lúc) và **incentive compatibility** (động lực để cung cấp dữ liệu đúng và có trách nhiệm).

Một oracle bị sai không cần làm blockchain “bị hack” để gây hậu quả. Nếu contract tin một giá sai, nó có thể định giá collateral sai, mở/đóng vị thế sai, thanh toán sai hoặc ngăn người dùng rút. Nguồn đơn có thể có single point of failure; nhiều nguồn giảm một số rủi ro nhưng thêm vấn đề tổng hợp, độ trễ, outlier, governance và tương quan nguồn.

**Chi tiết retrieval 2:** Khi thấy claim “oracle phi tập trung nên an toàn”, hãy tách ít nhất bốn câu: có bao nhiêu nguồn, ai chọn nguồn, quy tắc aggregate là gì, và contract xử lý stale/outlier data thế nào. “Nhiều node” không tự chứng minh dữ liệu gốc đúng.

## 9. Bảy lớp rủi ro cần đánh giá riêng
**Volatility risk:** Giá token hoặc collateral có thể dao động lớn; stablecoin có thể lệch giá mục tiêu. Không dùng biến động lịch sử để dự báo tương lai.

**Liquidity risk:** Thị trường có thể mỏng, spread rộng, pool mất cân bằng, withdrawal bị trì hoãn hoặc chain nghẽn. Giá hiển thị không bảo đảm một giao dịch lớn thực hiện được ở giá đó.

**Counterparty risk:** Issuer, exchange, custodian, reserve manager, oracle operator hoặc bridge có thể không thực hiện nghĩa vụ, hạn chế rút, mất khả năng thanh toán hoặc bị tấn công. “On-chain” không xóa bên phụ thuộc off-chain.

**Custody/key-loss risk:** Custodial service giữ khóa thay người dùng nhưng người dùng phụ thuộc vào chính sách và khả năng hoạt động của dịch vụ. Self-custody loại bỏ một số dependency nhưng chuyển trách nhiệm vào việc bảo vệ private key, seed phrase, thiết bị và quy trình ký. Mất khóa hoặc gửi nhầm địa chỉ có thể không thể đảo ngược; không cung cấp seed phrase là nguyên tắc an toàn cơ bản, không phải hướng dẫn đầu tư.

**Smart-contract/oracle/governance risk:** Bug, upgrade key, quyền admin, oracle sai, tham số xấu, vote tập trung hoặc composability có thể truyền lỗi từ một module sang module khác.

**Legal/regulatory risk:** Phân loại tài sản, cấp phép, quyền redeem, bảo vệ người tiêu dùng, thuế, AML/CFT và hiệu lực hợp đồng thay đổi theo jurisdiction và thời điểm. Nếu chưa mở văn bản chính thức hiện hành, trạng thái phải là pending; không dùng khẳng định “được bảo hiểm”, “được bảo lãnh” hoặc “được hợp pháp hóa” từ marketing.

**Scam/phishing risk:** Website giả, support giả, airdrop giả, endorsement giả, domain gần giống, yêu cầu ký transaction mù hoặc xin seed phrase nhắm vào người dùng. Một giao dịch hợp lệ trên chain vẫn có thể được người dùng ký nhầm cho kẻ gian. Xác minh domain, địa chỉ contract, nguồn thông báo và nội dung ký; không bấm link hoặc ký chỉ vì áp lực thời gian.

## 10. Hai hypothetical risk-assessment scenarios
### Scenario A — “Stable yield” và giao diện mới
Một trang web tuyên bố gửi stablecoin sẽ nhận lợi suất cố định rất cao. Trang có logo quen thuộc, đồng hồ đếm ngược và yêu cầu kết nối ví rồi ký một transaction. Người học không được giả định trang này là thật.

**Quan sát:** (1) có claim về lợi suất, (2) có giao diện yêu cầu kết nối và ký, (3) có áp lực thời gian, (4) chưa có tài liệu reserve/redemption/governance được mở. **Diễn giải cạnh tranh:** có thể là sản phẩm thật nhưng rủi ro cao; có thể là phishing/drainer; có thể là giao thức thật nhưng transaction cấp quyền rộng hơn người dùng hiểu. **Unknown:** contract address, calldata, quyền revoke, issuer, reserve, jurisdiction và điều kiện rút. **Risk classes:** scam/phishing, custody/key signing, smart contract, counterparty, liquidity, legal/regulatory. **Kết luận:** chưa đủ dữ liệu để gọi an toàn hoặc phù hợp; trạng thái là pending, không kết nối hoặc ký chỉ để “kiểm tra”.

### Scenario B — Oracle, collateral và governance
Một protocol lending nói collateral được định giá bằng oracle; tài liệu marketing gọi protocol là decentralized. Trong một giờ hypothetical, nguồn giá phụ thuộc một thị trường mỏng, oracle cập nhật chậm, một nhóm governance có quyền thay tham số và chain đang nghẽn.

**Known trong bài tập:** contract phụ thuộc oracle; oracle có freshness/correctness concern; governance là một dependency; chain congestion có thể ảnh hưởng availability và thanh khoản. **Uncertain:** giá nào sẽ được sử dụng, ngưỡng liquidation, quyền nâng cấp, cách xử lý stale data và ai kiểm soát vote. **Câu hỏi xác minh:** đọc code/version, oracle methodology, update interval, emergency powers, timelock, audit scope và risk disclosures. **Không được suy ra:** protocol chắc chắn sẽ bị khai thác, người dùng chắc chắn mất tiền hoặc decentralization là một nhãn nhị phân. Kết luận phù hợp là “các failure modes chưa được loại trừ”, không phải dự báo sự cố.

## 11. DeepDive: ma trận provenance và kết luận known/uncertain
Một risk assessment tốt không chỉ liệt kê rủi ro; nó nối claim với nguồn và giới hạn:

| Claim cần kiểm tra | Evidence tối thiểu | Trạng thái khi chưa đủ evidence |
|---|---|---|
| Token có quyền gì | Whitepaper/terms/official technical docs, version và issuer | Uncertain; không gọi là ownership hoặc claim on reserve |
| Stablecoin redeem thế nào | Issuer terms, reserve disclosure, jurisdiction, current date | Pending; không gọi là cash equivalent hoặc insured |
| Contract làm gì | Verified source code, deployed address, upgrade/admin permissions | Uncertain; audit không phải bảo đảm không lỗi |
| Oracle lấy dữ liệu ra sao | Source set, aggregation, freshness, fallback, governance | Uncertain; không gọi giá oracle là sự thật tuyệt đối |
| Custody ai kiểm soát | Terms, withdrawal process, key model, incident procedure | Uncertain; không dùng slogan thay cho quyền pháp lý |
| Có được phép/được bảo vệ không | Văn bản cơ quan có thẩm quyền, jurisdiction và effective date | Pending nếu chưa mở; không khái quát xuyên biên giới |
| Đây là cơ hội hay scam | Independent verification, domain/contract provenance, no-pressure process | Uncertain; không ký hoặc chuyển tài sản để “thử” |

BIS và FSB là nguồn policy/financial-stability, không phải product due diligence. Ethereum.org là nguồn kỹ thuật của Ethereum, không phải chứng nhận an toàn của một protocol. IOSCO report được dự định làm nguồn policy bổ sung nhưng không mở được trong phiên này; NIST IR 8202 được trả dưới dạng PDF payload không đọc được. Hai nguồn này giữ **pending** và không được dùng để tăng độ chắc chắn của lesson.

## 12. Tóm tắt, tự luyện và tài liệu tham khảo có giới hạn
Tóm tắt: blockchain là công nghệ; token là đơn vị/quyền cần đọc theo tài liệu; stablecoin là cơ chế cố gắng neo giá chứ không phải bảo hiểm; smart contract là code tự thực thi theo điều kiện; DeFi là ứng dụng tài chính trên blockchain; oracle là cầu nối dữ liệu; custody là câu hỏi ai kiểm soát khóa và quyền truy cập. Công nghệ, sản phẩm và tài sản đầu cơ phải được phân biệt. Risk assessment phải tách volatility, liquidity, counterparty, custody/key loss, code/oracle/governance, legal/regulatory và scam/phishing.

**Tự luyện:**

1. Phân loại: “mã nguồn smart contract”, “stablecoin issuer”, “token governance”, “một mức giá trên sàn”, “câu chuyện giả định về phishing” vào năm nhóm claim ở mục 1.
2. Viết năm câu hỏi provenance cho một stablecoin mà không nêu tên sản phẩm và không đưa ra khuyến nghị.
3. Trong Scenario B, ghi hai facts, ba unknowns, một pending legal claim và một failure mode không phải volatility.
4. Giải thích vì sao “non-custodial” không có nghĩa là “không có rủi ro”.

**Tài liệu đã mở và đọc:**

- BIS, Aramonte, Huang & Schrimpf, “DeFi risks and the decentralisation illusion”, BIS Quarterly Review, 06 Dec 2021, https://www.bis.org/publ/qtrpdf/r_qt2112b.htm. Dùng trong phạm vi định nghĩa DeFi/stablecoin và cơ chế rủi ro; không dùng số liệu cũ làm hiện trạng.
- FSB, “High-level Recommendations for the Regulation, Supervision and Oversight of Global Stablecoin Arrangements: Final report”, 17 Jul 2023, https://www.fsb.org/2023/07/high-level-recommendations-for-the-regulation-supervision-and-oversight-of-global-stablecoin-arrangements-final-report/. Dùng cho recommendation framework; không thay thế luật địa phương.
- Ethereum.org, “Introduction to smart contracts”, updated 01 Jul 2026, https://ethereum.org/en/smart-contracts/. Dùng cho technical definition và blind-signing boundary trong hệ Ethereum.
- Ethereum.org, “Oracles”, updated 26 Jun 2026, https://ethereum.org/en/developers/docs/oracles/. Dùng cho oracle architecture, correctness, availability và incentive compatibility.
- IOSCO crypto/digital asset report: **pending**, endpoint trả 403 trong phiên 2026-09-16; không dùng làm verified source.
- NIST IR 8202: **pending**, endpoint trả PDF payload không đọc được trong phiên; không dùng làm verified source.

**Known / uncertain:** Known là nội dung locator được trong bốn nguồn đã mở và các hypothetical được gắn nhãn. Uncertain là quyền token, reserve, custody, contract permissions, oracle quality, liquidity và legal protection của bất kỳ sản phẩm cụ thể nào chưa được kiểm tra. Không có claim nào trong bài dự báo giá hoặc đảm bảo kết quả.

## Rubric tự chấm bài thực hành — bổ sung 2026-10-02

**Bài:** Rủi ro crypto. **Đầu ra:** Làm ma trận known/uncertain cho scenario A/B ở mục 10.

| Tiêu chí | 2 điểm: đủ bằng chứng trong bài làm | 1 điểm | 0 điểm |
|---|---|---|---|
| Quan sát | Tách claim marketing, yêu cầu ký và tài liệu chưa mở | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Rủi ro | Nêu ít nhất ba lớp khác nhau, tránh gộp tất cả thành volatility | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Provenance | Ghi contract/version/oracle/governance cần đọc; không dựa vào logo | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Kết luận | A là pending, không ký để test; B có failure modes chưa loại trừ, không dự báo sự cố | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |
| Giới hạn | Nêu thông tin thiếu/câu hỏi xác minh; không gọi audit là bảo đảm | Có ý chính nhưng thiếu bước/chi tiết kiểm tra | Bỏ trống hoặc sai trọng tâm |

Tổng 10 điểm. Đạt bài luyện khi ≥8/10 và không có kết luận chẩn đoán, bảo đảm kết quả hay áp dụng giả định như dữ kiện thật. Nếu 6–7: sửa tiêu chí mất điểm rồi chấm lại; ≤5: đọc lại phần case/khái niệm trước khi làm phiên bản mới. Rubric đánh giá cách lập luận; không thay kiểm tra nguồn hoặc reviewer. Bài phản tư luôn tự nguyện, được dùng nhân vật giả định.
