# W11 — Tổng quan TTCK Việt Nam: HOSE, HNX, UPCoM và VN-Index

> [!IMPORTANT]
> **Tuyên bố Phạm vi và Giới hạn Lĩnh vực (Scope và Safety Disclaimer):** Nội dung bài học chỉ phục vụ mục đích giáo dục, KHÔNG phải lời khuyên đầu tư hay khuyến nghị mua/bán bất kỳ tài sản, chứng khoán nào. Bài học KHÔNG đề cập mã cổ phiếu cụ thể, KHÔNG đưa tỷ trọng khuyến nghị; mọi ví dụ đều dùng rổ và con số giả định. Quy định, biên độ, chu kỳ thanh toán và số liệu thị trường có thể thay đổi theo văn bản mới của cơ quan quản lý và các sàn; bạn phải tự kiểm tra lại từ nguồn chính thức (UBCKNN, SGDCK Việt Nam, HOSE, HNX, VSDC) tại thời điểm sử dụng và tự chịu trách nhiệm trước khi hành động.

---

## 1. Ý Nghĩa Thực Tiễn (Why It Matters)

Tuần trước (W10) bạn đã học các công cụ đầu tư thụ động: quỹ chỉ số, ETF và DCA — nhưng tất cả đều "đậu" trên một hạ tầng cụ thể. Nếu bạn đầu tư tại Việt Nam, hạ tầng đó gồm ba sàn giao dịch (HOSE, HNX, UPCoM), một hệ thống chỉ số (VN-Index, VN30...), một cơ quan quản lý (UBCKNN thuộc Bộ Tài chính) và một tổ chức hậu cần (VSDC phụ trách lưu ký, bù trừ, thanh toán). Câu hỏi thực tế mà người mới bắt đầu thường nhầm là: "Sàn nào là 'chính thức'?" — "VN-Index tăng có nghĩa là danh mục của mình cũng tăng?" — "Con số ±7% mình hay nghe là gì, có bảo đảm gì cho mình không?". Bài này trả lời bằng cấu trúc đã kiểm chứng từ nguồn chính thức, không bằng tin đồn thị trường.

Bốn tầng sẽ được tách bạch rõ: (1) **cơ quan quản lý** — Bộ Tài chính và UBCKNN, người đặt luật chơi; (2) **sàn giao dịch** — SGDCK Việt Nam với hai công ty con HOSE và HNX, người tổ chức phiên và khớp lệnh; (3) **hạ tầng hậu cần** — VSDC, người ghi sổ sở hữu và thanh toán tiền; (4) **chỉ số** — VN-Index và bộ chỉ số HOSE-Index, thước đo theo phương pháp luận, KHÔNG phải danh mục của bạn. Nhầm lẫn giữa bốn tầng này là nguồn gốc của nhiều khuyến cáo sai dạng "chỉ số tăng thì mua gì cũng lời". Cuối bài, worked example sẽ tính giá trần/giá sàn bằng VND cho từng sàn và minh họa cơ chế chỉ số vốn hóa bằng rổ giả định hai cổ phiếu.

**Kiến thức tiên quyết:** W10 — [Đầu tư tăng trưởng, Index/ETF và DCA](../module-2/week-10-growth-index-etf-dca.md) (khái niệm quỹ chỉ số, ETF, NAV, tracking — bài này gắn chúng với hạ tầng giao dịch thật tại Việt Nam); hữu ích thêm W02 (cấu trúc thị trường tài chính) và W03 (công bố thông tin).

---

## 2. Mục Tiêu Học Tập Đo Lường Được (Measurable Learning Objectives)

Sau khi hoàn thành bài học này, người học có năng lực:

- **`[LO-W11-01]` (Understand):** Định nghĩa và phân biệt bốn tầng của hệ thống: cơ quan quản lý (Bộ Tài chính, UBCKNN), tổ chức thị trường (SGDCK Việt Nam, HOSE, HNX), hạ tầng lưu ký—bù trừ—thanh toán (VSDC) và chỉ số (VN-Index, VN30, HNX-Index, UPCoM-Index); giải thích vai trò của từng tổ chức với mốc thời gian và văn bản thành lập, và vì sao "sàn giao dịch" khác "cơ quan quản lý".
- **`[LO-W11-02]` (Apply):** Tính giá trần và giá sàn từ một giá tham chiếu cho trước bằng VND theo biên độ dao động giá của từng sàn, ghi đúng quy tắc làm tròn; đọc và tóm tắt được các quy tắc chính trong công bố phương pháp luận của bộ chỉ số HOSE-Index (sàng lọc, free-float, giới hạn tỷ trọng, kỳ xem xét).
- **`[LO-W11-03]` (Analyze):** Phân tích cơ chế chỉ số vốn hóa bằng rổ giả định để giải thích vì sao kết quả danh mục cá nhân có thể lệch xa chỉ số; phân tích độ nhạy vùng giá khi biên độ khác nhau giữa ba sàn; nhận diện rủi ro đặc thù của thị trường đăng ký giao dịch (UPCoM) so với thị trường niêm yết.
- **`[LO-W11-04]` (Evaluate):** Phản biện các nhận định phổ biến dạng "chỉ số tăng thì danh mục cũng tăng" và "biên độ ±7% là bảo đảm lỗ tối đa 7%"; phân định những gì nguồn chính thức đã xác nhận và những gì chưa thể kết luận; xây dựng quy trình kiểm chứng nguồn (ngày truy cập, văn bản gốc, phiên bản) trước khi dùng số liệu thị trường.

---

## 3. Tóm Lược Cốt Lõi và Fast-Track Core (30-45 Phút)

### 3.1. Bản Đồ Khái Niệm Nhanh (Core Summary)

Thị trường chứng khoán Việt Nam có một "sơ đồ tổ chức" cần thuộc trước khi đọc bất kỳ tin tức nào. Ở tầng quản lý, UBCKNN là đơn vị thuộc Bộ Tài chính, thực hiện chức năng tham mưu giúp Bộ trưởng quản lý nhà nước về chứng khoán và thị trường chứng khoán; UBCKNN được thành lập ngày 28/11/1996 theo Nghị định số 75/CP `[W11-C01]`. Ở tầng tổ chức thị trường, Quyết định số 37/2020/QĐ-TTg của Thủ tướng Chính phủ thành lập SGDCK Việt Nam (VNX) theo mô hình công ty mẹ — công ty con: VNX là công ty TNHH MTV do Nhà nước nắm giữ 100% vốn điều lệ, nắm giữ 100% vốn của SGDCK Hà Nội (HNX) và SGDCK TP.HCM (HOSE) `[W11-C02]`. Chính trang chủ của HNX nhắc lại nguyên tắc này: ngoài SGDCK Việt Nam và các công ty con, không tổ chức, cá nhân nào được phép tổ chức và vận hành thị trường giao dịch chứng khoán `[W11-C02]`.

HNX được thành lập theo Quyết định số 01/2009/QĐ-TTg trên cơ sở tổ chức lại Trung tâm Giao dịch Chứng khoán Hà Nội (thành lập theo Quyết định số 127/1998/QĐ-TTg, khai trương 08/03/2005); Sở GDCK Hà Nội chính thức ra mắt ngày 24/06/2009, với các chức năng: tổ chức đấu giá cổ phần, đấu thầu trái phiếu Chính phủ, và vận hành các thị trường giao dịch — thị trường cổ phiếu niêm yết, thị trường cổ phiếu công ty đại chúng chưa niêm yết (UPCoM), thị trường trái phiếu Chính phủ và thị trường chứng khoán phái sinh `[W11-C03]`. HOSE là nơi tập trung cổ phiếu niêm yết và là nơi công bố VN-Index — chỉ số tổng hợp gắn liền với phiên giao dịch đầu tiên của thị trường ngày 28/7/2000 với giá trị cơ sở 100 điểm `[W11-C07]`. Ở tầng hậu cần, VSDC thực hiện bù trừ và thanh toán: thanh toán theo kết quả bù trừ đa phương với thời gian T+2 đối với cổ phiếu, chứng chỉ quỹ, chứng quyền có bảo đảm (T+1 với trái phiếu doanh nghiệp niêm yết và công cụ nợ), theo nguyên tắc DVP — chuyển giao chứng khoán chỉ diễn ra khi thanh toán tiền hoàn tất `[W11-C04]`.

Về quy tắc giá: biên độ dao động giá là giới hạn dao động giá trong ngày tính theo phần trăm so với giá tham chiếu; theo Thông tư 120/2020/TT-BTC (đã hợp nhất), SGDCK Việt Nam quyết định biên độ sau khi được UBCKNN chấp thuận, và trong trường hợp cần thiết để ổn định thị trường, UBCKNN quyết định điều chỉnh biên độ `[W11-C05]`. Mức thông thường hiện hành được ghi nhận ở ±7% cho HOSE, ±10% cho HNX và ±15% cho UPCoM, với biên độ đặc biệt ±20%/±30%/±40% cho ngày giao dịch đầu tiên và các trường hợp đặc biệt `[W11-C06]`. Về chỉ số: bộ chỉ số HOSE-Index (VN30, VNMidcap, VNSmallcap, VN100, VNAllshare) được tính theo giá trị vốn hóa điều chỉnh free-float, áp giới hạn tỷ trọng 10%, và được xem xét cơ cấu rổ bán niên vào tháng 1 và tháng 7 hàng năm `[W11-C08]` — khác VN-Index vốn là chỉ số tổng hợp toàn sàn HOSE `[W11-C07]` `[W11-C08]`.

### 3.2. Bảng So Sánh Đối Kháng Nhanh (Quick Comparative Table)

| Tầng / Tổ chức | Vai trò cốt lõi | Mốc / văn bản then chốt | Sai lầm phổ biến cần tránh | Nhãn bằng chứng |
| :--- | :--- | :--- | :--- | :---: |
| **Bộ Tài chính / UBCKNN** | Quản lý nhà nước về chứng khoán và TTCK; UBCKNN thuộc Bộ Tài chính | UBCKNN thành lập 28/11/1996 (Nghị định 75/CP) | Nhầm UBCKNN với "sàn giao dịch" hay công ty dịch vụ | `[W11-C01]` |
| **SGDCK Việt Nam (VNX)** | Công ty mẹ tổ chức thị trường; nắm 100% vốn HOSE và HNX | QĐ 37/2020/QĐ-TTg (hiệu lực 20/02/2021); vốn điều lệ 3.000 tỷ VND | Cho rằng HOSE và HNX là hai cơ quan quản lý độc lập | `[W11-C02]` |
| **HOSE (SGDCK TP.HCM)** | Tổ chức thị trường cổ phiếu niêm yết tập trung doanh nghiệp lớn; công bố VN-Index và bộ chỉ số HOSE-Index | Tiền thân TTGDCK TP.HCM khai trương phiên đầu 28/7/2000 | Coi HOSE là cơ quan quản lý hoặc là "người bảo đảm" giá | `[W11-C02]` `[W11-C07]` |
| **HNX (SGDCK Hà Nội)** | Tổ chức thị trường cổ phiếu niêm yết, UPCoM, trái phiếu Chính phủ, phái sinh; đấu thầu TPCP | QĐ 01/2009/QĐ-TTg; ra mắt 24/06/2009 từ TTGDCK HN (mở 08/03/2005) | Cho rằng UPCoM là thị trường "ngoài hệ thống" không có tổ chức quản lý | `[W11-C03]` |
| **UPCoM** | Thị trường giao dịch cổ phiếu công ty đại chúng chưa niêm yết, do HNX tổ chức | Vận hành trên hệ thống HNX từ 2009 | Coi UPCoM "không có luật"; bỏ qua biên độ rộng hơn và khác biệt chuẩn mực công bố | `[W11-C03]` `[W11-C06]` |
| **VSDC** | Đăng ký, lưu ký, bù trừ, thanh toán, thực hiện quyền | Quy chế bù trừ—thanh toán hiện hành QĐ 39/QĐ-HĐTV (29/4/2025) | Nhầm VSDC với sàn giao dịch hoặc công ty chứng khoán | `[W11-C04]` |
| **VN-Index** | Chỉ số tổng hợp cổ phiếu niêm yết HOSE; cơ sở 100 điểm ngày 28/7/2000 | Đo tăng trưởng giá trị vốn hóa so với thời điểm cơ sở | Cho rằng chỉ số = danh mục của bạn hoặc = "lợi nhuận bảo đảm" | `[W11-C07]` |
| **Bộ chỉ số HOSE-Index (VN30...)** | Chỉ số đầu tư được: sàng lọc + free-float + capping 10% + cơ cấu bán niên 1/7 | VN30: ngày cơ sở 02/01/2009, giá trị cơ sở 313,34 | Nhầm VN30 với "toàn bộ sàn" hoặc cho rằng rổ cố định vĩnh viễn | `[W11-C08]` |

### 3.3. Năm Điểm Rút Ra Then Chốt (5 Key Takeaways)

1. **Bốn tầng, bốn vai trò, không thể hoán đổi.** Quản lý (UBCKNN/Bộ Tài chính) ≠ tổ chức phiên (HOSE/HNX dưới SGDCK Việt Nam) ≠ ghi sổ và thanh toán (VSDC) ≠ thước đo (VN-Index, VN30) `[W11-C01]` `[W11-C02]` `[W11-C04]`.
2. **"Sàn" quyết định quy tắc giao dịch của bạn, không quyết định kết quả đầu tư.** Biên độ dao động, giờ giao dịch, loại lệnh khác nhau giữa HOSE (±7%), HNX (±10%), UPCoM (±15%) — đó là quy tắc chơi; kết quả vẫn do giá thị trường `[W11-C05]` `[W11-C06]`.
3. **Biên độ là giới hạn yết giá mỗi phiên, không phải bảo hiểm.** Nó tính so với giá tham chiếu của phiên đó; lỗ tích lũy nhiều phiên so với giá bạn đã mua có thể vượt xa con số biên độ, và cơ quan quản lý có thể điều chỉnh biên độ khi cần ổn định thị trường `[W11-C05]` `[W11-C06]`.
4. **Chỉ số là thước đo có phương pháp luận, không phải danh mục tự động của bạn.** VN-Index đo toàn sàn HOSE theo vốn hóa `[W11-C07]`; VN30 là rổ sàng lọc 30 mã, điều chỉnh free-float, áp trần 10%, cơ cấu bán niên `[W11-C08]` — rổ và tỷ trọng thay đổi theo quy tắc, và danh mục cá nhân của bạn có tỷ trọng riêng.
5. **Thanh toán cổ phiếu là T+2, theo nguyên tắc DVP.** Sau ngày khớp lệnh T, chứng khoán và tiền được chuyển giao tại VSDC vào ngày thanh toán T+2 (cổ phiếu, chứng chỉ quỹ, chứng quyền), với nguyên tắc "nhận chứng khoán khi và chỉ khi đã trả tiền" `[W11-C04]`.

---

## 4. Mổ Xẻ Chuyên Sâu (Deep Dive: 2-3 Giờ)

### 4.1. Bản Chất Cơ Chế và Cơ Sở Khoa Học

**Vì sao cần tách bạch "quản lý" và "tổ chức thị trường".** Luật chơi của thị trường (điều kiện niêm yết, công bố thông tin, xử lý vi phạm) do Bộ Tài chính ban hành văn bản quy phạm pháp luật và UBCKNN thực hiện quản lý nhà nước — UBCKNN "là đơn vị thuộc Bộ Tài chính, thực hiện chức năng tham mưu, giúp Bộ trưởng Bộ Tài chính quản lý nhà nước về chứng khoán và thị trường chứng khoán" `[W11-C01]`. Việc vận hành phiên (nhận lệnh, khớp giá, giám sát giao dịch) thuộc các sàn: từ 2021, mô hình công ty mẹ — công ty con được xác lập khi SGDCK Việt Nam được thành lập theo QĐ 37/2020/QĐ-TTg — "công ty TNHH một thành viên do Nhà nước nắm giữ 100% vốn điều lệ", nắm giữ 100% vốn điều lệ của HNX và HOSE `[W11-C02]`. Tách bạch này có hệ quả thực hành: khi quy định thay đổi (ví dụ biên độ, loại lệnh), nguồn xác minh là văn bản của Bộ Tài chính/UBCKNN và quy chế giao dịch của sàn — không phải bài đăng trên mạng xã hội; khi lỗi vận hành xảy ra, bên giải thích là sàn, trong khi thẩm quyền phê duyệt quy chế vẫn ở UBCKNN (ví dụ: biên độ dao động giá do SGDCK Việt Nam quyết định "sau khi được UBCKNN chấp thuận" `[W11-C05]`).

**HOSE, HNX và UPCoM: ba địa điểm, ba tính chất hàng hóa.** HOSE kế thừa Trung tâm Giao dịch Chứng khoán TP.HCM — nơi phiên giao dịch đầu tiên của thị trường Việt Nam diễn ra ngày 28/7/2000 `[W11-C07]` — và nay là thị trường tập trung các cổ phiếu niêm yết quy mô lớn; đồng thời là chủ thể công bố VN-Index và bộ chỉ số HOSE-Index `[W11-C07]` `[W11-C08]`. HNX ra mắt 24/06/2009 theo QĐ 01/2009/QĐ-TTg, vận hành bốn khu: cổ phiếu niêm yết, UPCoM, trái phiếu Chính phủ (kể cả đấu thầu) và chứng khoán phái sinh `[W11-C03]`. UPCoM — viết tắt của Unlisted Public Company Market — là thị trường giao dịch cổ phiếu của công ty đại chúng chưa niêm yết, được tổ chức trên hệ thống của HNX `[W11-C03]`. "Chưa niêm yết" không có nghĩa "ngoài pháp luật": đây là thị trường đăng ký giao dịch có quy chế riêng, nhưng chuẩn mực công bố thông tin và biên độ dao động khác thị trường niêm yết — biên độ rộng hơn (±15% so với ±7%/±10%) là đặc điểm dữ kiện, còn mức độ rủi ro thông tin cụ thể của từng cổ phiếu là điều phải tra cứu theo từng doanh nghiệp `[W11-C03]` `[W11-C06]`.

**Biên độ dao động giá: quy tắc chơi về "giá được phép yết".** Khung pháp lý hiện hành: "Biên độ dao động giá là giới hạn dao động giá chứng khoán quy định trong ngày giao dịch được tính theo tỷ lệ phần trăm (%) so với giá tham chiếu"; SGDCK Việt Nam quyết định biên độ sau khi được UBCKNN chấp thuận; "trong trường hợp cần thiết để ổn định thị trường, Ủy ban Chứng khoán Nhà nước quyết định điều chỉnh biên độ dao động giá và công bố thông tin" `[W11-C05]`. Giá trần = giá tham chiếu + (giá tham chiếu × biên độ); giá sàn = giá tham chiếu − (giá tham chiếu × biên độ). Mức thông thường hiện hành được ghi nhận: HOSE ±7%, HNX ±10%, UPCoM ±15%; các ngày đặc biệt (ngày giao dịch đầu tiên, ngày giao dịch trở lại sau khi tạm ngừng đủ 25 phiên liên tiếp, ngày không hưởng quyền trong một số trường hợp) áp biên độ ±20% (HOSE), ±30% (HNX), ±40% (UPCoM) `[W11-C06]`. Ba điểm đọc kỹ: (1) mốc so sánh là **giá tham chiếu của phiên đó** (thường là giá đóng cửa phiên trước), không phải giá bạn đã mua; (2) biên độ chỉ giới hạn **yết giá trong phiên**, không giới hạn tổng biến động nhiều phiên; (3) con số có thể được điều chỉnh theo thẩm quyền quy định `[W11-C05]` `[W11-C06]`.

**VN-Index và bộ chỉ số HOSE-Index: hai thước đo, hai phương pháp.** VN-Index là chỉ số đầu tiên của thị trường, khởi điểm 100 điểm tại phiên đầu ngày 28/7/2000, dùng để xác định sự tăng trưởng của giá trị vốn hóa hiện tại so với thời điểm cơ sở `[W11-C07]`. Bộ chỉ số HOSE-Index do HOSE xây dựng theo bộ quy tắc công bố (phiên bản hiện hành ban hành kèm QĐ 747/QĐ-SGDHCM ngày 30/12/2024) với đặc điểm đã đối chiếu từ công bố chính thức: sàng lọc tư cách (niêm yết tối thiểu 6 tháng, không thuộc diện cảnh báo/kiểm soát/tạm ngừng trong 3 tháng), free-float cao hơn 10%, thanh khoản (tỷ suất quay vòng tối thiểu 0,05%); tính theo giá trị vốn hóa điều chỉnh free-float; giới hạn tỷ trọng vốn hóa 10%; xem xét định kỳ bán niên vào tháng 1 và tháng 7 `[W11-C08]`. VN30 — rổ 30 công ty hàng đầu về vốn hóa và thanh khoản trong VNAllshare — có ngày cơ sở 02/01/2009, giá trị cơ sở 313,34; tại công bố ngày 30/06/2025, tỷ trọng vốn hóa của 10 công ty lớn nhất trong rổ VN30 chiếm 63,81% `[W11-C08]`. Con số tập trung 63,81% này (dữ kiện tại một thời điểm cụ thể, sẽ thay đổi theo từng công bố) giải thích phần lớn cơ chế "chỉ số bị kéo bởi nhóm vốn hóa lớn" được phân tích ở worked example 2.

**VSDC và chu kỳ T+2: hạ tầng "ghi sổ" quyết định khi nào bạn thật sự sở hữu.** Sau khi giao dịch được xác lập tại sàn, dữ liệu kết quả giao dịch chuyển sang VSDC để bù trừ; thanh toán thực hiện theo kết quả bù trừ đa phương với thời gian T+2 đối với cổ phiếu, chứng chỉ quỹ, chứng quyền có bảo đảm (T+1 với trái phiếu doanh nghiệp niêm yết và công cụ nợ); nguyên tắc DVP (Delivery Versus Payment) bảo đảm việc chuyển giao chứng khoán tại VSDC diễn ra khi và chỉ khi thanh toán tiền hoàn tất — loại bỏ rủi ro gốc của hai bên `[W11-C04]`. Hệ quả thực hành: tiền/cổ phiếu "về tài khoản sẵn sàng giao dịch" tuân theo lịch thanh toán và quy trình hạch toán của thành viên lưu ký; lập kế hoạch dòng tiền (ví dụ cần tiền vào một mốc cụ thể) phải tính chu kỳ này, không tính theo cảm giác "khớp lệnh là xong" `[W11-C04]`.

### 4.2. Mổ Xẻ Chi Tiết Từng Thành Phần Của Mô Hình Worked Example

**Thành phần 1 — Giá tham chiếu và đơn vị yết giá.** Mọi tính trần/sàn bắt đầu từ giá tham chiếu của phiên; ví dụ của bài chọn giá tham chiếu chia hết cho 100 VND để không phát sinh làm tròn theo đơn vị yết giá. Với giá tham chiếu bất kỳ, kết quả làm tròn theo đơn vị yết giá là quy tắc của quy chế giao dịch từng sàn — bạn phải tra quy chế hiện hành trước khi dùng kết quả cho mục đích thật `[W11-C05]` `[W11-C06]`.

**Thành phần 2 — Biên độ là tham số theo sàn và theo ngày.** Cùng một giá tham chiếu 30.000 VND cho ba vùng giá khác nhau trên ba sàn (xem Mục 6): chênh lệch biên độ 7% — 10% — 15% tạo dải giá trần/sàn rộng dần theo thứ tự HOSE → HNX → UPCoM; ngày đặc biệt mở rộng dải lên ±20/±30/±40%. Đây là dữ kiện về quy tắc, không phải dự báo hướng giá `[W11-C06]`.

**Thành phần 3 — Cơ chế chỉ số vốn hóa và hệ số chia.** Chỉ số vốn hóa đo tổng giá trị vốn hóa của rổ so với mốc cơ sở: Index = (CMV hiện tại / CMV cơ sở) × 100, với CMV = Σ (giá × số cổ phiếu đang lưu hành) của rổ. Khi rổ thay đổi hoặc có sự kiện doanh nghiệp (phát hành thêm, mua lại...), hệ số chia được điều chỉnh để chỉ số không "nhảy" ngoài biến động giá — nguyên tắc liên tục của các chỉ số. Ví dụ Mục 6 giả định hệ số chia cố định (không có sự kiện) để tập trung vào cơ chế tỷ trọng `[W11-C08]`.

**Thành phần 4 — Đại lượng so sánh khi đọc chỉ số.** Ba câu hỏi khi thấy chỉ số biến động: (1) rổ gồm gì (toàn sàn hay rổ sàng lọc)? (2) tỷ trọng của từng nhóm là bao nhiêu tại thời điểm đó (con số tập trung thay đổi theo công bố)? (3) danh mục của tôi có tỷ trọng giống rổ không? Ba câu hỏi này chuyển "chỉ số tăng/giảm" từ cảm xúc thành dữ kiện có thể kiểm `[W11-C07]` `[W11-C08]`.

### 4.3. Bối Cảnh Thực Tế và Ràng Buộc Đặc Thù

- **Bối cảnh Việt Nam:** mọi khung khái niệm trong bài gắn với hạ tầng hiện hành tại Việt Nam; số liệu thị trường (điểm chỉ số, giá trị giao dịch, số lượng cổ phiếu) thay đổi hằng ngày nên bài học cố tình không đưa số liệu thời điểm, trừ các con số có mốc công bố rõ trong nguồn (ví dụ tỷ trọng top-10 VN30 tại ngày 30/06/2025 `[W11-C08]`).
- **Tách bạch dữ kiện - giả định - phép tính - diễn giải** (ràng buộc của môn học):

| Loại nội dung | Ví dụ trong bài | Trạng thái |
| :--- | :--- | :--- |
| Dữ kiện đã nguồn hóa | Vai trò UBCKNN; cấu trúc SGDCK Việt Nam — HOSE — HNX; thành lập HNX và UPCoM; T+2 và DVP của VSDC; khung biên độ theo TT 120/2020/TT-BTC; phương pháp bộ chỉ số HOSE-Index; VN-Index cơ sở 100 điểm ngày 28/7/2000 | Đã mở và đọc trực tiếp nguồn chính thức hoặc bản sao văn bản (xem ledger `[W11-C01]`..`[W11-C08]`) |
| Giả định minh họa | Giá tham chiếu 10.000/30.000/50.000/100.000 VND; rổ chỉ số giả định 2 cổ phiếu X, Y với vốn hóa 50/20 tỷ VND; hệ số chia cố định | Giả định phục vụ học tập, không phải dữ liệu thị trường thật |
| Phép tính | Bảng giá trần/sàn 3 sàn; bảng kịch bản chỉ số và 2 danh mục giả định | Kiểm tra số học độc lập 2026-09-15 (`[W11-C09]`) |
| Diễn giải thực hành | Checklist kiểm chứng nguồn; quy tắc đọc chỉ số 3 câu hỏi | Đề xuất thực hành, không phải khuyến nghị đầu tư |

- **Ràng buộc độ bền bằng chứng:** các nguồn sơ cấp là trang chính thức của UBCKNN, Cổng TTĐT Chính phủ, HNX, VSDC và tên miền tệp chính thức của HOSE (staticfile.hsx.vn); một số văn bản chi tiết (quyết định của sàn về biên độ) chỉ mở được bản sao công khai, được ghi rõ giới hạn trong ledger. Số liệu điểm chỉ số, giá cả từng phiên KHÔNG được đưa vào bài vì tính thời điểm cao `[W11-C01]`..`[W11-C09]`.

---

## 5. Nghiên Cứu Gốc và Mô Hình Lý Thuyết (Key Studies and Theoretical Models)

### Nguồn sơ cấp 1: UBCKNN — vai trò cơ quan quản lý
- **Cơ quan công bố:** Ủy ban Chứng khoán Nhà nước, trang "Giới thiệu" trên Cổng thông tin điện tử, https://ssc.gov.vn/webcenter/portal/ubck/pages_r/m/giithiu `[W11-C01]`.
- **Nội dung đã đối chiếu:** UBCKNN được thành lập ngày 28/11/1996 theo Nghị định số 75/CP; "là đơn vị thuộc Bộ Tài chính, thực hiện chức năng tham mưu, giúp Bộ trưởng Bộ Tài chính quản lý nhà nước về chứng khoán và thị trường chứng khoán; tổ chức thực thi pháp luật về chứng khoán và thị trường chứng khoán".
- **Ý nghĩa:** xác định đúng tầng "quản lý" trong sơ đồ bốn tầng; nền tảng để phân biệt với sàn và công ty chứng khoán.
- **Giới hạn:** trang mô tả tổ chức, không phải hướng dẫn giao dịch; cấu trúc tổ chức có thể được điều chỉnh theo quyết định mới của Nhà nước.

### Nguồn sơ cấp 2: Cổng TTĐT Chính phủ — thành lập SGDCK Việt Nam
- **Nguồn:** Cổng Thông tin điện tử Chính phủ, tin "Thành lập Sở giao dịch Chứng khoán Việt Nam" (28/12/2020), https://vpcp.chinhphu.vn/thanh-lap-so-giao-dich-chung-khoan-viet-nam-11525304.htm `[W11-C02]`.
- **Nội dung đã đối chiếu:** Thủ tướng Chính phủ ký Quyết định số 37/2020/QĐ-TTg thành lập SGDCK Việt Nam (VNX) theo mô hình công ty mẹ — công ty con trên cơ sở sắp xếp lại HNX và HOSE; VNX là công ty TNHH MTV do Nhà nước nắm giữ 100% vốn điều lệ, vốn điều lệ 3.000 tỷ đồng, nắm giữ 100% vốn điều lệ của HNX và HOSE.
- **Ý nghĩa:** xác lập cấu trúc sở hữu — tổ chức hiện hành của hai sàn; giải thích vì sao quy chế giao dịch được ban hành thống nhất trong hệ thống.
- **Giới hạn:** bài tin của Cổng Chính phủ tóm tắt quyết định; nội dung chi tiết của QĐ 37/2020/QĐ-TTg cần tra nguyên văn khi dùng cho mục đích pháp lý.

### Nguồn sơ cấp 3: HNX — lịch sử phát triển và nguyên tắc tổ chức thị trường
- **Nguồn:** Sở GDCK Hà Nội, trang "Lịch sử phát triển", https://www.hnx.vn/vi-vn/gioi-thieu-hnx-lspt.html; và thông báo đăng trang chủ www.hnx.vn về hình thức mạo danh (08/2022), đã đọc ngày 2026-09-15 `[W11-C03]`, `[W11-C02]`.
- **Nội dung đã đối chiếu:** HNX thành lập theo Quyết định số 01/2009/QĐ-TTg trên cơ sở tổ chức lại TTGDCK Hà Nội (QĐ 127/1998/QĐ-TTg, khai trương 08/03/2005); chính thức ra mắt 24/06/2009 với mô hình công ty TNHH MTV do Nhà nước (đại diện là Bộ Tài chính) làm chủ sở hữu; chức năng gồm đấu giá cổ phần, đấu thầu TPCP, vận hành thị trường cổ phiếu niêm yết, UPCoM, TPCP và CKPS. Trang chủ ghi: ngoài SGDCK Việt Nam và các công ty con, không tổ chức, cá nhân nào được phép tổ chức và vận hành thị trường giao dịch chứng khoán.
- **Ý nghĩa:** nguồn chính thức xác nhận UPCoM là thị trường do HNX tổ chức; xác nhận độc quyền tổ chức thị trường của hệ thống SGDCK Việt Nam.
- **Giới hạn:** trang giới thiệu không thay thế quy chế UPCoM hiện hành; chi tiết đối tượng đăng ký giao dịch và điều kiện tra theo văn bản của HNX tại thời điểm dùng.

### Nguồn sơ cấp 4: VSDC — bù trừ, thanh toán và nguyên tắc DVP
- **Nguồn:** Tổng công ty Lưu ký và Bù trừ chứng khoán Việt Nam, trang dịch vụ "Bù trừ và Thanh toán", https://www.vsdc.vn/sd/XAz40d2Q-9j569TvBgLQaQ `[W11-C04]`.
- **Nội dung đã đối chiếu:** "Việc thanh toán thực hiện theo kết quả bù trừ đa phương với thời gian thanh toán T+1 đối với giao dịch trái phiếu doanh nghiệp niêm yết, T+2 đối với giao dịch cổ phiếu, chứng chỉ quỹ, chứng quyền có bảo đảm"; nguyên tắc DVP: "việc chuyển giao chứng khoán tại VSDC diễn ra khi và chỉ khi việc thanh toán tiền được thực hiện tại NHTT"; quy chế hiện hành là QĐ 39/QĐ-HĐTV ngày 29/4/2025.
- **Ý nghĩa:** xác định chu kỳ thanh toán và cơ chế bảo vệ hai bên trong giao dịch; nền tảng lập kế hoạch dòng tiền.
- **Giới hạn:** trang mô tả nghiệp vụ tổng quát; chi tiết trình tự theo từng loại chứng khoán và lịch nghỉ thanh toán phải tra VSDC tại thời điểm giao dịch.

### Nguồn sơ cấp 5: Thông tư 120/2020/TT-BTC (văn bản hợp nhất) — khung biên độ dao động giá
- **Nguồn:** Bộ Tài chính, Thông tư số 120/2020/TT-BTC ngày 31/12/2020 "quy định giao dịch cổ phiếu niêm yết, đăng ký giao dịch và chứng chỉ quỹ, trái phiếu doanh nghiệp, chứng quyền có bảo đảm niêm yết trên hệ thống giao dịch chứng khoán" (hiệu lực 15/02/2021; được sửa đổi bởi Thông tư 68/2024/TT-BTC, hợp nhất trong văn bản hợp nhất 08/VBHN-BTC), đối chiếu qua bản sao công khai: https://luatvietnam.vn/doanh-nghiep/van-ban-hop-nhat-08-vbhn-btc-2026-quy-dinh-giao-dich-co-phieu-niem-yet-va-chung-chi-quy-435832-d5.html `[W11-C05]`.
- **Nội dung đã đối chiếu (Điều 2 khoản 1 và Điều 4):** định nghĩa "biên độ dao động giá" nằm tại Điều 2 khoản 1: "Biên độ dao động giá là giới hạn dao động giá chứng khoán quy định trong ngày giao dịch được tính theo tỷ lệ phần trăm (%) so với giá tham chiếu"; Điều 4 quy định "SGDCK Việt Nam quyết định biên độ dao động giá sau khi được UBCKNN chấp thuận" và "Trong trường hợp cần thiết để ổn định thị trường, UBCKNN quyết định điều chỉnh biên độ dao động giá và công bố thông tin trên Cổng thông tin điện tử của UBCKNN".
- **Ý nghĩa:** khung pháp lý hiện hành của biên độ; xác định thẩm quyền và khả năng điều chỉnh.
- **Giới hạn:** trong phiên kiểm chứng, toàn văn mở qua bản sao công khai của văn bản hợp nhất, bản gốc trên công báo không tải được; mức biên độ cụ thể theo quy chế giao dịch của sàn (nguồn `[W11-C06]`).

### Nguồn sơ cấp 6: Quyết định của sàn và tài liệu hướng dẫn thành viên — mức biên độ hiện hành
- **Nguồn:** (a) Quyết định số 635/QĐ-SGDHN ngày 24/9/2019 của SGDCK Hà Nội "biên độ dao động giá chứng khoán đăng ký giao dịch tại SGDCK Hà Nội là ±15%" (bản sao công khai: https://hethongphapluat.com/quyet-dinh-635-qd-sgdhn-nam-2019-ve-bien-do-dao-dong-gia-chung-khoan-dang-ky-giao-dich-do-so-giao-dich-chung-khoan-ha-noi-ban-hanh.html); (b) hướng dẫn giao dịch của các công ty chứng khoán thành viên (MBS 2024, VNCSSI, TCBS; VTGS "Tài liệu hướng dẫn Nhà đầu tư" 05/2025 kèm QĐ 31/2025/VTGS-QD ngày 05/05/2025): HOSE ±7%, HNX ±10%, UPCoM ±15%; biên độ đặc biệt ±20%/±30%/±40% `[W11-C06]`.
- **Nội dung đã đối chiếu:** các bảng biên độ nhất quán về mức thông thường và các trường hợp đặc biệt (ngày giao dịch đầu tiên; giao dịch trở lại sau tạm ngừng ≥25 phiên; ngày không hưởng quyền trong một số trường hợp).
- **Ý nghĩa:** cung cấp mức con số hiện hành để tính giá trần/sàn trong bài học.
- **Giới hạn:** nhóm nguồn này gồm bản sao quyết định của sàn và tài liệu của công ty chứng khoán (tổ chức được cấp phép nhưng không phải cơ quan ban hành quy chế); các trang chính thức hsx.vn/hnx.vn không tải được toàn văn trong phiên; mức biên độ có thể thay đổi theo quyết định của SGDCK Việt Nam/UBCKNN — tra lại quy chế giao dịch hiện hành trước khi dùng.

### Nguồn sơ cấp 7: HOSE — bộ chỉ số HOSE-Index và VN-Index
- **Nguồn:** HOSE, "Bộ chỉ số HOSE-Index" (factsheet ngày cập nhật 30/06/2025) trên tên miền tệp chính thức https://staticfile.hsx.vn/Uploads/UploadDocuments/2387607/Form_Factsheet_MCIndices_VN_T07.2025.pdf; quy tắc quản lý Bộ chỉ số HOSE-Index phiên bản hiện hành ban hành kèm QĐ 747/QĐ-SGDHCM ngày 30/12/2024 (bản dịch tiếng Anh đối chiếu qua bản sao công khai) `[W11-C08]`; về VN-Index: factsheet liệt kê VN-Index trong bảng so sánh của HOSE `[W11-C07]`; mốc cơ sở 100 điểm ngày 28/7/2000 đối chiếu thêm qua Báo Điện tử Đảng Cộng sản Việt Nam (Nhân Dân) bài "Hành trình HoSE 25 năm" (28/4/2025) https://nhandan.vn/ocop/hanh-trinh-hose-25-nam-nang-gia-tri-vung-niem-tin-phat-trien-va-hoan-thien-he-thong-chi-so-cho-thi-truong-chung-khoan-post875553.html và Đài Truyền hình Việt Nam (29/7/2026) https://vtv.vn/26-nam-thi-truong-chung-khoan-viet-nam-nang-tam-kenh-dan-von-cho-nen-kinh-te-100260728204034855.htm `[W11-C07]`.
- **Nội dung đã đối chiếu:** HOSE-Index là "bộ chỉ số vốn hóa có thể đầu tư... đáp ứng yêu cầu sàng lọc về tư cách, tỷ lệ tự do chuyển nhượng và thanh khoản"; phương pháp "Giá trị vốn hóa điều chỉnh free-float"; "Xem xét định kỳ: Bán niên vào tháng 1 và tháng 7 hàng năm"; "Giới hạn tỷ trọng vốn hóa: 10% (VNAllshare Sector không áp dụng)"; VN30 có ngày cơ sở 02/01/2009, giá trị cơ sở 313,34; tại ngày 30/06/2025, tỷ trọng vốn hóa của 10 công ty lớn nhất trong rổ VN30 là 63,81%. Về VN-Index: "khởi điểm ở mức 100 điểm" tại phiên đầu 28/7/2000, "được dùng để xác định sự tăng trưởng của giá trị vốn hóa ở thời điểm hiện tại so với giá trị vốn hóa tại thời điểm cơ sở".
- **Ý nghĩa:** phân biệt hai nhóm chỉ số của HOSE (tổng hợp và rổ đầu tư được) với phương pháp cụ thể; cung cấp dữ kiện tập trung tỷ trọng để phân tích cơ chế chỉ số.
- **Giới hạn:** đặc tính chi tiết của riêng VN-Index (có/không điều chỉnh free-float, có/không áp trần tỷ trọng) cần đối chiếu trực tiếp tài liệu phương pháp luận VN-Index trên hsx.vn — trang chính thức là ứng dụng JavaScript không tải được nội dung trong phiên kiểm chứng; số liệu tỷ trọng là dữ kiện tại một ngày công bố, không cố định.

### Ghi chú nguồn liên quan
Trang dulieu.nguoiquansat.vn mô tả VN-Index khớp với các nguồn trên nhưng là trang thuộc công ty tư nhân (không phải cơ quan quản lý) nên KHÔNG được dùng làm nguồn claim trong bài học này. Tương tự, các trang giáo dục của công ty chứng khoán chỉ dùng để đối chiếu thêm, không thay thế văn bản của cơ quan quản lý và sàn.

---

## 6. Ví Dụ Giải Từng Bước (Worked Examples)

### Ví dụ 1: Giá trần và giá sàn từ giá tham chiếu 30.000 VND trên ba sàn (hoàn toàn giả định)

- **Giai đoạn 1: Bối cảnh và Đề bài (Problem Formulation).**
  Một cổ phiếu giả định "CĐ-X" có giá tham chiếu 30.000 VND cho phiên hôm nay (giả định giá đóng cửa phiên trước là 30.000 VND và không có sự kiện điều chỉnh giá tham chiếu). Câu hỏi: giá trần và giá sàn của CĐ-X trong phiên này nếu cổ phiếu giao dịch trên (i) HOSE, (ii) HNX, (iii) UPCoM — và nếu đây là ngày giao dịch đầu tiên?

- **Bảng Giả Định (Assumptions Table) — giả định minh họa phục vụ học tập, không phải dữ liệu thị trường thực tế:**

  | Tham số | Ký hiệu | Giá trị giả định | Căn cứ thực tế và ghi chú |
  | :--- | :---: | :---: | :--- |
  | Giá tham chiếu | P | 30.000 VND | Đề bài giả định; giá tham chiếu thực tế do sàn công bố theo quy chế |
  | Biên độ thông thường HOSE | b_H | 7% | Mức hiện hành theo quy chế giao dịch, đối chiếu qua nguồn `[W11-C06]`; tra lại trước khi dùng |
  | Biên độ thông thường HNX | b_N | 10% | Như trên `[W11-C06]` |
  | Biên độ thông thường UPCoM | b_U | 15% | Như trên `[W11-C06]` |
  | Biên độ đặc biệt (ngày đầu) | b* | ±20% (HOSE), ±30% (HNX), ±40% (UPCoM) | Các ngày giao dịch đầu tiên/trở lại sau tạm ngừng ≥25 phiên `[W11-C06]` |
  | Quy tắc làm tròn | - | Giá tham chiếu chia hết cho 100 VND nên kết quả tròn số; ngoài ra làm tròn theo đơn vị yết giá của quy chế sàn | Quy ước của ví dụ để tránh phát sinh làm tròn |

- **Giai đoạn 2: Phân tích và Lựa chọn Mô hình (Conceptual Deconstruction).**
  Công thức từ khung quy định: giá trần = P × (1 + b); giá sàn = P × (1 − b) `[W11-C05]` `[W11-C06]`. Biên độ là tham số theo sàn và theo loại ngày; phép tính thuần túy nhân—trừ trên VND.

- **Giai đoạn 3: Thực hiện Từng bước (Step-by-Step Execution).**

  | Sàn (ngày bình thường) | Biên độ | Giá trần (VND) | Giá sàn (VND) |
  | :--- | :---: | :---: | :---: |
  | HOSE | ±7% | 30.000 × 1,07 = **32.100** | 30.000 × 0,93 = **27.900** |
  | HNX | ±10% | 30.000 × 1,10 = **33.000** | 30.000 × 0,90 = **27.000** |
  | UPCoM | ±15% | 30.000 × 1,15 = **34.500** | 30.000 × 0,85 = **25.500** |

  | Sàn (ngày giao dịch đầu tiên — biên độ đặc biệt) | Biên độ | Giá trần (VND) | Giá sàn (VND) |
  | :--- | :---: | :---: | :---: |
  | HOSE | ±20% | 30.000 × 1,20 = **36.000** | 30.000 × 0,80 = **24.000** |
  | HNX | ±30% | 30.000 × 1,30 = **39.000** | 30.000 × 0,70 = **21.000** |
  | UPCoM | ±40% | 30.000 × 1,40 = **42.000** | 30.000 × 0,60 = **18.000** |

- **Giai đoạn 4: Đúc kết Bài học và Phân tích Độ nhạy (Takeaway và Sensitivity Analysis).**
  (1) **Độ nhạy theo giá tham chiếu** (ngày bình thường, trần/sàn tính bằng VND):

  | Giá tham chiếu | HOSE trần/sàn (±7%) | HNX trần/sàn (±10%) | UPCoM trần/sàn (±15%) |
  | :---: | :---: | :---: | :---: |
  | 10.000 VND | 10.700 / 9.300 | 11.000 / 9.000 | 11.500 / 8.500 |
  | 30.000 VND | 32.100 / 27.900 | 33.000 / 27.000 | 34.500 / 25.500 |
  | 50.000 VND | 53.500 / 46.500 | 55.000 / 45.000 | 57.500 / 42.500 |
  | 100.000 VND | 107.000 / 93.000 | 110.000 / 90.000 | 115.000 / 85.000 |

  Chênh lệch vùng giá giữa HOSE và UPCoM tăng tuyến tính theo giá tham chiếu: chênh **8 điểm phần trăm** (15% − 7%) tương ứng **2.400 VND** ở mức tham chiếu 30.000 và **8.000 VND** ở mức 100.000 `[W11-C06]` `[W11-C09]`. (2) **Bài học cốt lõi:** biên độ là tham số của quy tắc yết giá, thay đổi theo sàn và theo loại ngày; kết quả không nói gì về hướng giá kỳ vọng. (3) **Giới hạn:** giá tham chiếu thực tế do sàn xác định (có thể là giá đóng cửa gia quyền trong một số trường hợp); đơn vị yết giá và cách làm tròn theo quy chế hiện hành; số liệu chỉ dùng để học cơ chế `[W11-C05]` `[W11-C09]`.

### Ví dụ 2: Cơ chế chỉ số vốn hóa — vì sao chỉ số tăng mà danh mục của bạn có thể đứng yên (rổ giả định)

- **Giai đoạn 1: Bối cảnh và Đề bài (Problem Formulation).**
  Một "chỉ số sàn giả định" gồm đúng 2 cổ phiếu: X (vốn hóa lớn) và Y (vốn hóa nhỏ). Bạn nắm giữ một danh mục chỉ có cổ phiếu nhỏ Y. Câu hỏi: chỉ số và danh mục của bạn biến động thế nào khi X tăng mạnh?

- **Bảng Giả Định (Assumptions Table) — giả định minh họa phục vụ học tập, không mô tả mã cổ phiếu nào:**

  | Tham số | Ký hiệu | Giá trị giả định | Ghi chú |
  | :--- | :---: | :---: | :--- |
  | Cổ phiếu X: giá / số cp đang lưu hành | P_X, N_X | 50.000 VND / 1.000.000 cp | Giả định minh họa |
  | Cổ phiếu Y: giá / số cp đang lưu hành | P_Y, N_Y | 20.000 VND / 1.000.000 cp | Giả định minh họa |
  | Vốn hóa rổ cơ sở | CMV₀ | 50 tỷ + 20 tỷ = 70 tỷ VND | Index₀ = 100 điểm |
  | Kịch bản kỳ sau | - | X tăng 60% lên 80.000 VND; Y đứng giá 20.000 VND | Giả định đường giá |
  | Hệ số chia | D | Cố định (không có sự kiện doanh nghiệp, rổ không đổi) | Giả định để tập trung cơ chế |
  | Danh mục giả định của bạn | - | 500 cp Y (10.000.000 VND ban đầu), không có X | Giả định minh họa |
  | Danh mục đối chứng "theo tỷ trọng rổ" | - | 71,43% giá trị vào X, 28,57% vào Y (50 triệu X = 1.000 cp; 20 triệu Y = 1.000 cp) | Tỷ trọng vốn hóa rổ: 50/70 và 20/70 |

- **Giai đoạn 2: Phân tích và Lựa chọn Mô hình (Conceptual Deconstruction).**
  Chỉ số vốn hóa: Index₁ = (CMV₁ / CMV₀) × 100 với CMV₁ = P_X′ × N_X + P_Y′ × N_Y. Danh mục: giá trị cuối = Σ (số cp nắm giữ × giá mới); P/L = giá trị cuối − vốn ban đầu.

- **Giai đoạn 3: Thực hiện Từng bước (Step-by-Step Execution).**
  - CMV₁ = 80.000 × 1.000.000 + 20.000 × 1.000.000 = 80 tỷ + 20 tỷ = **100 tỷ VND**.
  - Index₁ = 100/70 × 100 = **142,86 điểm**, tức **+42,86%** — tăng mạnh chỉ nhờ X (vốn hóa 80/100 = 80% trong rổ mới).
  - Danh mục của bạn (chỉ Y): đầu 500 × 20.000 = 10.000.000 VND; cuối 500 × 20.000 = 10.000.000 VND → **0%**.
  - Danh mục đối chứng theo tỷ trọng rổ: đầu 70.000.000 VND (1.000 cp X + 1.000 cp Y); cuối 1.000 × 80.000 + 1.000 × 20.000 = **100.000.000 VND** → **+42,86%**, khớp chỉ số vì tỷ trọng giữ đúng theo vốn hóa `[W11-C09]`.

- **Giai đoạn 4: Đúc kết Bài học và Phân tích Độ nhạy (Takeaway và Sensitivity Analysis).**
  (1) **Kết quả là hàm của tỷ trọng sở hữu:** cùng một thị trường, danh mục chỉ Y đứng nguyên 0% trong khi chỉ số +42,86% — "chỉ số tăng" không tự động là "danh mục tăng" `[W11-C09]`. (2) **Độ nhạy theo kịch bản ngược:** nếu X giảm 10% còn 45.000 VND và Y đứng giá, CMV = 45 + 20 = 65 tỷ → Index = 65/70 × 100 = **92,86 điểm (−7,14%)**, danh mục của bạn vẫn 0% — chỉ số giảm mà danh mục không đổi. (3) **Độ nhạy theo cổ phiếu nhỏ:** nếu Y tăng 5% (21.000 VND) và X đứng giá, CMV = 50 + 21 = 71 tỷ → Index = 101,43 (+1,43%); danh mục của bạn +5% — danh mục vượt chỉ số khi nhóm nhỏ tăng mạnh hơn nhóm lớn. (4) **Bài học thực hành:** khi đọc tin "chỉ số tăng X%", tự hỏi ba câu của Mục 4.2 (rổ gì, tỷ trọng bao nhiêu, danh mục mình ra sao); dữ kiện tập trung tỷ trọng ở rổ thật (ví dụ top-10 VN30 chiếm 63,81% tại công bố 30/06/2025 `[W11-C08]`) cho biết chỉ số vốn hóa bị chi phối mạnh bởi nhóm lớn. (5) **Giới hạn mô hình:** rổ 2 cổ phiếu là rút gọn; chỉ số thật có hệ số chia được điều chỉnh khi có sự kiện; bỏ qua phí, thuế, khớp lệnh một phần; kết quả chỉ dùng để học cơ chế `[W11-C09]`.

### Bài tập tự luyện ngắn (giải ở phần gợi ý cuối mục)
1. Cùng giá tham chiếu 30.000 VND, chênh lệch giữa giá trần HOSE và giá trần UPCoM là bao nhiêu VND? Nếu giá tham chiếu là 100.000 VND thì chênh lệch này là bao nhiêu?
2. Trong ví dụ 2, nếu X giảm 20% còn 40.000 VND và Y tăng 10% lên 22.000 VND, chỉ số và danh mục của bạn (chỉ Y) thay đổi thế nào?

**Gợi ý:** (1) trần HOSE 32.100; trần UPCoM 34.500 → chênh 2.400 VND (8% của 30.000); ở mức 100.000: 107.000 so với 115.000 → chênh 8.000 VND. (2) CMV = 40 + 22 = 62 tỷ → Index = 62/70 × 100 = 88,57 điểm (−11,43%); danh mục chỉ Y: 500 × 22.000 = 11.000.000 VND → **+10%** — minh chứng lần hai cho việc tách chỉ số khỏi danh mục.

---

## 7. Điều Đã Sáng Tỏ và Phần Còn Bất Định (What We Know vs What Remains Uncertain)

### 7.1. Những Điều Đã Được Xác Minh Vững Chắc 🟢
- UBCKNN là đơn vị thuộc Bộ Tài chính, thực hiện quản lý nhà nước về chứng khoán và TTCK; thành lập 28/11/1996 theo Nghị định 75/CP — nguồn chính thức UBCKNN `[W11-C01]`.
- SGDCK Việt Nam được thành lập theo QĐ 37/2020/QĐ-TTg theo mô hình công ty mẹ — công ty con, nắm giữ 100% vốn điều lệ HNX và HOSE; ngoài SGDCK Việt Nam và các công ty con, không tổ chức, cá nhân nào được tổ chức và vận hành thị trường giao dịch chứng khoán `[W11-C02]`.
- HNX thành lập theo QĐ 01/2009/QĐ-TTg từ TTGDCK Hà Nội (mở 08/03/2005), ra mắt 24/06/2009; tổ chức thị trường cổ phiếu niêm yết, UPCoM, TPCP (kể cả đấu thầu) và phái sinh; UPCoM là thị trường cổ phiếu công ty đại chúng chưa niêm yết do HNX tổ chức `[W11-C03]`.
- VSDC thực hiện bù trừ, thanh toán: T+2 cho cổ phiếu/chứng chỉ quỹ/chứng quyền có bảo đảm, T+1 cho trái phiếu doanh nghiệp niêm yết và công cụ nợ, theo nguyên tắc DVP `[W11-C04]`.
- Khung biên độ dao động giá: giới hạn phần trăm so với giá tham chiếu; SGDCK Việt Nam quyết định sau khi UBCKNN chấp thuận; UBCKNN có thể điều chỉnh khi cần ổn định thị trường (TT 120/2020/TT-BTC, Điều 4 — bản hợp nhất) `[W11-C05]`.
- Bộ chỉ số HOSE-Index: tính theo vốn hóa điều chỉnh free-float, giới hạn tỷ trọng 10%, xem xét bán niên tháng 1 và tháng 7; VN30 có ngày cơ sở 02/01/2009, giá trị cơ sở 313,34 — theo công bố chính thức của HOSE `[W11-C08]`.
- VN-Index khởi điểm 100 điểm tại phiên đầu ngày 28/7/2000, đo sự tăng trưởng giá trị vốn hóa so với thời điểm cơ sở `[W11-C07]`.

### 7.2. Những Vùng Xám Cần Cẩn Trọng 🔴 / 🟠
- **Đặc tính chi tiết của riêng VN-Index** 🟠: các mô tả phổ biến cho rằng VN-Index tính trên toàn bộ vốn hóa cổ phiếu niêm yết HOSE, chưa điều chỉnh free-float và không áp trần tỷ trọng như bộ chỉ số HOSE-Index; trong phiên kiểm chứng, tài liệu phương pháp luận VN-Index trên trang hsx.vn không tải được (ứng dụng JavaScript) nên đặc tính này ghi ở mức "phổ biến, cần đối chiếu trực tiếp tài liệu HOSE" — không đưa vào nhóm đã xác minh `[W11-C07]`.
- **Mức biên độ cụ thể ±7/±10/±15% (và ±20/±30/±40%)** 🟠: xác nhận qua bản sao quyết định của sàn và các tài liệu hướng dẫn của công ty chứng khoán thành viên (gồm bản sau khi hạ tầng giao dịch mới vận hành 2025) nhất quán với nhau; nhưng văn bản quy chế giao dịch hiện hành trên tên miền chính thức của các sàn không tải được toàn văn trong phiên. Mức này có thể được điều chỉnh theo thẩm quyền quy định `[W11-C05]` `[W11-C06]` — luôn tra lại trước khi dùng.
- **Số liệu thị trường thời điểm** 🔴: điểm chỉ số, giá trị vốn hóa, thanh khoản thay đổi hằng ngày; bài học không đưa số liệu thời điểm và mọi con số có mốc (như 63,81% top-10 VN30) phải kèm ngày công bố `[W11-C08]`.
- **Hạ tầng giao dịch mới (KRX) và lộ trình nâng hạng** 🔴: các thay đổi hạ tầng và phân loại thị trường quốc tế đang diễn tiến; bài này không dùng làm claim vì chưa mở được thông báo chính thức tổng hợp trong phiên — thuộc nội dung cần theo dõi riêng ở W12 `[W11-C05]`.
- **"Chỉ số là dự báo nền kinh tế"** 🔴: mối liên hệ chỉ số — nền kinh tế là chủ đề nghiên cứu phức tạp với nhiều giai đoạn lệch pha; bài học chỉ khẳng định chức năng thước đo của chỉ số, không khẳng định sức dự báo `[W11-C07]`.
- **Rủi ro từng cổ phiếu UPCoM cụ thể** 🟠: biên độ rộng là dữ kiện quy tắc; chất lượng công bố thông tin và thanh khoản của từng doanh nghiệp phải tự kiểm theo công bố của doanh nghiệp và HNX, không suy ra chung từ tên thị trường `[W11-C03]`.

---

## 8. Sai Lầm Phổ Biến và Phản Biện Pop Literature (Common Mistakes and Myths Debunked)

| Sai lầm phổ biến / Lời khuyên thường thức | Bản chất ngụy biện / Nguy cơ thực tế | Căn cứ khoa học phản biện | Hành vi đúng đắn thay thế |
| :--- | :--- | :--- | :--- |
| **Hiểu lầm 1:** "HOSE (hay sàn) là cơ quan quản lý chứng khoán, nên sàn bảo vệ mình khỏi rủi ro." | Gộp tầng tổ chức thị trường với tầng quản lý nhà nước; sàn tổ chức phiên và giám sát giao dịch, còn quản lý nhà nước thuộc Bộ Tài chính/UBCKNN | Vai trò UBCKNN `[W11-C01]`; mô hình SGDCK Việt Nam — HOSE — HNX `[W11-C02]` | Khi cần quy định pháp lý, tra văn bản Bộ Tài chính/UBCKNN; khi cần quy tắc phiên, tra quy chế giao dịch của sàn |
| **Hiểu lầm 2:** "VN-Index tăng 2% nghĩa là cổ phiếu của tôi tăng 2%." | Chỉ số vốn hóa phản ánh rổ theo tỷ trọng vốn hóa; nhóm vốn hóa lớn chi phối; danh mục cá nhân có tỷ trọng và cấu phần riêng | Cơ chế chỉ số vốn hóa và ví dụ 2 Mục 6 `[W11-C07]` `[W11-C08]` `[W11-C09]` | Đọc chỉ số như thước đo rổ; đối chiếu tỷ trọng danh mục với cấu trúc rổ trước khi kết luận |
| **Hiểu lầm 3:** "UPCoM là chợ đen, không có tổ chức quản lý." | UPCoM là thị trường đăng ký giao dịch của công ty đại chúng chưa niêm yết, do HNX tổ chức và vận hành trong hệ thống SGDCK Việt Nam | Chức năng HNX `[W11-C03]`; nguyên tắc độc quyền tổ chức thị trường `[W11-C02]` | Coi UPCoM là thị trường chính thức với biên độ rộng hơn và chuẩn mực công bố khác niêm yết; kiểm tra từng doanh nghiệp trước khi giao dịch |
| **Hiểu lầm 4:** "Biên độ ±7% nghĩa là tôi không bao giờ lỗ quá 7% so với giá đã mua." | Biên độ giới hạn yết giá so với giá tham chiếu của từng phiên; tích lũy nhiều phiên, mức giá mua cá nhân, và các phiên biên độ đặc biệt đều có thể khiến mức lỗ so với giá mua vượt xa con số đó; biên độ còn có thể bị điều chỉnh | Khung biên độ và thẩm quyền điều chỉnh `[W11-C05]`; mức biên độ theo sàn `[W11-C06]`; ví dụ 1 Mục 6 `[W11-C09]` | Quản lý rủi ro bằng kế hoạch vị thế và kỷ luật của W07–W08, không dựa vào "trần—sàn" như bảo hiểm |
| **Hiểu lầm 5:** "Mua 'theo chỉ số' là hết phải đọc quy tắc, vì rổ chỉ số cố định." | Rổ chỉ số có tiêu chí sàng lọc và kỳ cơ cấu định kỳ (VN30 cơ cấu bán niên tháng 1/7, capping 10%); thành phần và tỷ trọng thay đổi theo quy tắc | Phương pháp bộ chỉ số HOSE-Index `[W11-C08]` | Đọc công bố phương pháp luận và lịch cơ cấu; theo dõi thay đổi rổ trước khi đưa sản phẩm bám chỉ số vào kế hoạch (nối W10) |

---

## 9. Ôn Tập Giữa Tuần Tương Tác (Mid-Week Mini-Reviews)

### Đợt 1 (Ngày 3): Kiểm tra Nhận diện Khái niệm
<details>
<summary><b>Câu hỏi 1: Hãy xếp đúng vai trò cho bốn tổ chức sau — UBCKNN, SGDCK Việt Nam (HOSE, HNX), VSDC, VN-Index — và nêu mốc/văn bản then chốt của từng tổ chức.</b></summary>

> **Gợi ý tự kiểm tra:**
> - UBCKNN: cơ quan quản lý nhà nước về chứng khoán và TTCK, thuộc Bộ Tài chính; thành lập 28/11/1996 theo Nghị định 75/CP `[W11-C01]`.
> - SGDCK Việt Nam: công ty mẹ tổ chức thị trường, nắm 100% vốn HOSE và HNX (QĐ 37/2020/QĐ-TTg); HOSE tổ chức thị trường cổ phiếu niêm yết và công bố VN-Index/bộ chỉ số; HNX tổ chức cổ phiếu niêm yết, UPCoM, TPCP, phái sinh `[W11-C02]` `[W11-C03]`.
> - VSDC: đăng ký, lưu ký, bù trừ, thanh toán; thanh toán cổ phiếu/CCQ/chứng quyền T+2, TPDN niêm yết T+1, nguyên tắc DVP `[W11-C04]`.
> - VN-Index: thước đo (chỉ số tổng hợp cổ phiếu niêm yết HOSE), cơ sở 100 điểm ngày 28/7/2000 — là dữ liệu, không phải tổ chức `[W11-C07]`.
> - Mẹo ghi nhớ: "ai đặt luật — ai mở phiên — ai ghi sổ — cái gì đo".
> *Đọc lại mục:* [Mục 3.2](#32-bảng-so-sánh-đối-kháng-nhanh-quick-comparative-table) và [Mục 4.1](#41-bản-chất-cơ-chế-và-cơ-sở-khoa-học).
</details>

### Đợt 2 (Ngày 5): Kiểm tra Vận dụng Tình huống
<details>
<summary><b>Câu hỏi 2: Tuần qua chỉ số tăng mạnh nhưng danh mục giả định của bạn (tập trung nhóm vốn hóa nhỏ) đứng giá. Hãy dùng ví dụ Mục 6 để giải thích, và chỉ ra hai bước kiểm chứng nguồn trước khi tin một câu nói "chỉ số tăng là mua gì cũng lời".</b></summary>

> **Gợi ý tự kiểm tra:**
> - Cơ chế: chỉ số vốn hóa bị chi phối bởi nhóm vốn hóa lớn; trong ví dụ 2, X tăng 60% kéo chỉ số +42,86% trong khi danh mục chỉ Y đứng 0% — tương tự tình huống của bạn `[W11-C09]`.
> - Dữ kiện đối chiếu: tỷ trọng top-10 của rổ VN30 ở mức 63,81% tại công bố 30/06/2025 cho thấy mức tập trung có thể rất cao `[W11-C08]`.
> - Bước kiểm chứng nguồn 1: xác định rổ và phương pháp (chỉ số nào, gồm gì, tỷ trọng thế nào) từ công bố chính thức của HOSE `[W11-C08]`.
> - Bước kiểm chứng nguồn 2: xác nhận con số kèm ngày công bố và ghi lại URL/ngày truy cập; không dùng snippet mạng xã hội làm bằng chứng `[W11-C05]`.
> - Kết luận đúng đắn: "chỉ số tăng" là dữ kiện về rổ; kết quả danh mục phụ thuộc tỷ trọng sở hữu — không có quy luật tự động.
> *Đọc lại mục:* [Mục 6](#6-ví-dụ-giải-từng-bước-worked-examples) và [Mục 8](#8-sai-lầm-phổ-biến-và-phản-biện-pop-literature-common-mistakes-and-myths-debunked).
</details>

---

## 10. Bài Tập Thực Hành và Tiêu Chí Tự Đánh Giá (Practical Exercises and Rubric)

### Hướng Dẫn Lựa Chọn Bài Tập (An Toàn và Tự Nguyện)
- **Lựa chọn A (Tự quan sát cá nhân, không giao dịch):** trong 7 ngày tới, chọn MỘT cổ phiếu bất kỳ trên bảng giá chính thức của sàn (không mua/bán), ghi lại: giá tham chiếu, giá trần, giá sàn do sàn công bố; tự tính lại trần/sàn từ giá tham chiếu theo biên độ của sàn và so với bảng giá; ghi nguồn (URL chính thức), ngày xem và sàn. Mục tiêu là rèn thói quen kiểm chứng quy tắc, không phải dự đoán giá.
- **Lựa chọn B (Kịch bản giả định):** tự thiết kế rổ chỉ số giả định 3 cổ phiếu (vốn hóa 60/30/10 tỷ VND), đặt chuỗi giá kỳ sau cho từng cổ phiếu (tự chọn hướng), tính: CMV mới, điểm chỉ số, và kết quả của hai danh mục giả định (một danh mục theo tỷ trọng rổ, một danh mục chỉ nhóm nhỏ); viết 5 câu kết luận kèm bảng giả định, ghi rõ mọi con số là giả định minh họa.

### Tiêu Chí Tự Đánh Giá Năng Lực (Self-Assessment Rubric)

| Tiêu chí đánh giá | Chưa đạt (Emerging) | Đang phát triển (Developing) | Đạt chuẩn (Proficient) | Xuất sắc (Advanced) |
| :--- | :--- | :--- | :--- | :--- |
| **Phân biệt bốn tầng** | Gộp sàn với cơ quan quản lý. | Phân biệt được nhưng thiếu văn bản/mốc. | Nêu đúng vai trò UBCKNN, SGDCK Việt Nam—HOSE—HNX, VSDC kèm văn bản/mốc `[W11-C01]` `[W11-C02]` `[W11-C03]` `[W11-C04]`. | Giải thích được hệ quả thực hành của việc tách tầng khi tra cứu quy định. |
| **Tính trần/sàn và đọc quy tắc** | Nhớ con số mà không biết quy tắc. | Tính đúng một sàn nhưng nhầm ngày đặc biệt hoặc làm tròn. | Tính đúng trần/sàn cho ba sàn ở ngày thường và ngày đầu, ghi đúng đơn vị và giả định `[W11-C05]` `[W11-C06]` `[W11-C09]`. | Tự kiểm lại giá tham chiếu/đơn vị yết giá theo quy chế hiện hành và chỉ ra nguồn cần tra. |
| **Phân tích chỉ số vs danh mục** | Kết luận "chỉ số tăng = danh mục tăng". | Nêu được cơ chế nhưng không lượng hóa. | Dùng rổ giả định tính được độ lệch chỉ số — danh mục theo tỷ trọng sở hữu `[W11-C09]`. | Kết nối với dữ kiện tập trung tỷ trọng rổ thật và chỉ ra khi nào danh mục vượt/thua chỉ số. |
| **Kiểm chứng nguồn** | Dùng tin lan truyền không nguồn. | Có nguồn nhưng không ghi ngày/URL. | Ghi đầy đủ nguồn chính thức, ngày truy cập, phạm vi đọc và giới hạn `[W11-C05]` `[W11-C06]`. | Phân biệt nguồn sơ cấp (văn bản, công bố sàn) và bản sao; nêu rõ phần chưa thể kết luận. |

---

## 11. Bảng Thuật Ngữ Chuẩn Hóa (Weekly Glossary)

### 11.1. Thuật ngữ song ngữ

| Thuật ngữ Tiếng Việt | Thuật ngữ Tiếng Anh | Định nghĩa khoa học cô đọng |
| :--- | :--- | :--- |
| **Ủy ban Chứng khoán Nhà nước** | *State Securities Commission (SSC)* | Đơn vị thuộc Bộ Tài chính, thực hiện chức năng quản lý nhà nước về chứng khoán và thị trường chứng khoán; thành lập 28/11/1996. |
| **Sở Giao dịch Chứng khoán Việt Nam** | *Vietnam Exchange (VNX)* | Công ty TNHH MTV 100% vốn nhà nước, tổ chức thị trường giao dịch chứng khoán theo mô hình công ty mẹ — công ty con, nắm giữ 100% vốn của HNX và HOSE. |
| **Sở Giao dịch Chứng khoán TP.HCM** | *Ho Chi Minh Stock Exchange (HOSE)* | Công ty con của SGDCK Việt Nam, tổ chức thị trường cổ phiếu niêm yết; công bố VN-Index và bộ chỉ số HOSE-Index. |
| **Sở Giao dịch Chứng khoán Hà Nội** | *Hanoi Stock Exchange (HNX)* | Công ty con của SGDCK Việt Nam (ra mắt 24/06/2009), tổ chức thị trường cổ phiếu niêm yết, UPCoM, trái phiếu Chính phủ và chứng khoán phái sinh. |
| **Thị trường công ty đại chúng chưa niêm yết** | *UPCoM (Unlisted Public Company Market)* | Thị trường giao dịch cổ phiếu của công ty đại chúng chưa niêm yết, do HNX tổ chức; biên độ dao động giá rộng hơn thị trường niêm yết. |
| **Tổng công ty Lưu ký và Bù trừ chứng khoán Việt Nam** | *Vietnam Securities Depository and Clearing Corporation (VSDC)* | Tổ chức thực hiện đăng ký, lưu ký, bù trừ, thanh toán và thực hiện quyền chứng khoán; thanh toán cổ phiếu/chứng chỉ quỹ/chứng quyền theo T+2. |
| **Chỉ số VN-Index** | *VN-Index* | Chỉ số tổng hợp của HOSE, khởi điểm 100 điểm ngày 28/7/2000, đo sự tăng trưởng giá trị vốn hóa cổ phiếu niêm yết HOSE so với thời điểm cơ sở. |
| **Bộ chỉ số HOSE-Index** | *HOSE-Index Series* | Bộ chỉ số đầu tư được của HOSE (VNAllshare, VN30, VNMidcap, VNSmallcap, VN100, chỉ số ngành), tính theo vốn hóa điều chỉnh free-float, capping 10%, cơ cấu bán niên tháng 1 và tháng 7. |
| **Chỉ số VN30** | *VN30-Index* | Chỉ số gồm 30 công ty hàng đầu về vốn hóa và thanh khoản trong VNAllshare; ngày cơ sở 02/01/2009, giá trị cơ sở 313,34. |
| **Biên độ dao động giá** | *Daily Price Limit* | Giới hạn dao động giá chứng khoán trong ngày giao dịch, tính theo phần trăm so với giá tham chiếu; do SGDCK Việt Nam quyết định sau khi được UBCKNN chấp thuận. |
| **Giá tham chiếu** | *Reference Price* | Mức giá cơ sở xác định trần/sàn của phiên (thường là giá đóng cửa phiên giao dịch trước, có điều chỉnh theo trường hợp quy định). |
| **Giá trần / Giá sàn** | *Ceiling Price / Floor Price* | Mức giá yết cao nhất/thấp nhất trong phiên: giá trần = giá tham chiếu × (1 + biên độ); giá sàn = giá tham chiếu × (1 − biên độ). |
| **Tỷ lệ cổ phiếu tự do chuyển nhượng** | *Free Float* | Tỷ lệ cổ phiếu tự do chuyển nhượng trên tổng số cổ phiếu đang lưu hành; tham số sàng lọc và tính trọng số của bộ chỉ số HOSE-Index (tối thiểu 10%). |
| **Giới hạn tỷ trọng vốn hóa** | *Capping* | Cơ chế giới hạn tỷ trọng một cổ phiếu trong rổ chỉ số (HOSE-Index: 10%) nhằm hạn chế ảnh hưởng quá mức của một mã đơn lẻ. |
| **Nguyên tắc chuyển giao khi thanh toán** | *Delivery Versus Payment (DVP)* | Nguyên tắc: chứng khoán chỉ được chuyển giao khi và chỉ khi tiền được thanh toán — loại bỏ rủi ro gốc trong thanh toán giao dịch chứng khoán. |

### 11.2. Viết tắt sử dụng trong bài
| Viết tắt | Đầy đủ |
| :--- | :--- |
| UBCKNN | Ủy ban Chứng khoán Nhà nước — State Securities Commission (SSC) |
| SGDCK | Sở Giao dịch Chứng khoán — Stock Exchange |
| HOSE | Sở Giao dịch Chứng khoán TP.HCM — Ho Chi Minh Stock Exchange |
| HNX | Sở Giao dịch Chứng khoán Hà Nội — Hanoi Stock Exchange |
| UPCoM | Unlisted Public Company Market — thị trường công ty đại chúng chưa niêm yết |
| VSDC | Tổng công ty Lưu ký và Bù trừ chứng khoán Việt Nam — Vietnam Securities Depository and Clearing Corporation |
| VNX | Vietnam Exchange — Sở Giao dịch Chứng khoán Việt Nam |
| VN-Index | Vietnam Index — chỉ số tổng hợp cổ phiếu niêm yết HOSE |
| TPCP | Trái phiếu Chính phủ |
| CKPS | Chứng khoán phái sinh |
| DVP | Delivery Versus Payment — nguyên tắc chuyển giao chứng khoán khi thanh toán |
| CMV | Current Market Value — giá trị vốn hóa thị trường hiện tại của rổ |
| TT | Thông tư |
| QĐ | Quyết định |

---

## 12. Danh Mục Tài Liệu Tham Khảo và Bước Kế Tiếp (References and Next Steps)

### Tài liệu tham khảo (APA-ish, gắn mã Claim; toàn bộ URL đã mở và đọc ngày 2026-09-15)
- Bộ Tài chính. (2020). *Thông tư số 120/2020/TT-BTC ngày 31/12/2020 quy định giao dịch cổ phiếu niêm yết, đăng ký giao dịch và chứng chỉ quỹ, trái phiếu doanh nghiệp, chứng quyền có bảo đảm niêm yết trên hệ thống giao dịch chứng khoán* [Văn bản hợp nhất 08/VBHN-BTC]. https://luatvietnam.vn/doanh-nghiep/van-ban-hop-nhat-08-vbhn-btc-2026-quy-dinh-giao-dich-co-phieu-niem-yet-va-chung-chi-quy-435832-d5.html — `[W11-C05]` (Điều 4 đã đối chiếu qua bản sao công khai của văn bản hợp nhất)
- Hội đồng thành viên Sở GDCK Việt Nam. (2022). *Quy chế niêm yết và giao dịch chứng khoán niêm yết* (Quyết định 17/QĐ-HĐTV ngày 31/3/2022) — được dẫn trong nguồn `[W11-C02]`/`[W11-C05]`; nguyên văn cần tra khi dùng cho mục đích pháp lý.
- Sở GDCK Hà Nội. (n.d.). *Lịch sử phát triển*. https://www.hnx.vn/vi-vn/gioi-thieu-hnx-lspt.html — `[W11-C03]`
- Sở GDCK Hà Nội. (2019). *Quyết định số 635/QĐ-SGDHN ngày 24/9/2019 về biên độ dao động giá chứng khoán đăng ký giao dịch* [Bản sao công khai]. https://hethongphapluat.com/quyet-dinh-635-qd-sgdhn-nam-2019-ve-bien-do-dao-dong-gia-chung-khoan-dang-ky-giao-dich-do-so-giao-dich-chung-khoan-ha-noi-ban-hanh.html — `[W11-C06]`
- Sở Giao dịch Chứng khoán TP.HCM. (2025). *Bộ chỉ số HOSE-Index* [Factsheet, ngày cập nhật 30/06/2025]. https://staticfile.hsx.vn/Uploads/UploadDocuments/2387607/Form_Factsheet_MCIndices_VN_T07.2025.pdf — `[W11-C08]`, `[W11-C07]`
- Sở Giao dịch Chứng khoán TP.HCM. (2024). *Quy tắc xây dựng và quản lý Bộ chỉ số HOSE-Index* (ban hành kèm QĐ 747/QĐ-SGDHCM ngày 30/12/2024) [Bản dịch tiếng Anh, bản sao công khai]. https://static2.vietstock.vn/vietstock/2024/12/30/20241230_25_12_hose_index_4_0_translation.pdf — `[W11-C08]`
- Tổng công ty Lưu ký và Bù trừ chứng khoán Việt Nam. (n.d.). *Bù trừ và Thanh toán*. https://www.vsdc.vn/sd/XAz40d2Q-9j569TvBgLQaQ — `[W11-C04]`
- Ủy ban Chứng khoán Nhà nước. (n.d.). *Giới thiệu — Ủy ban Chứng khoán Nhà nước*. https://ssc.gov.vn/webcenter/portal/ubck/pages_r/m/giithiu — `[W11-C01]`
- Văn phòng Chính phủ. (2020, 28 tháng 12). *Thành lập Sở giao dịch Chứng khoán Việt Nam* [Tin về Quyết định 37/2020/QĐ-TTg]. Cổng Thông tin điện tử Chính phủ. https://vpcp.chinhphu.vn/thanh-lap-so-giao-dich-chung-khoan-viet-nam-11525304.htm — `[W11-C02]`
- Báo Điện tử Đảng Cộng sản Việt Nam (Nhân Dân). (2025, 28 tháng 4). *Hành trình HoSE 25 năm "Nâng giá trị - Vững niềm tin" phát triển và hoàn thiện hệ thống chỉ số cho thị trường chứng khoán*. https://nhandan.vn/ocop/hanh-trinh-hose-25-nam-nang-gia-tri-vung-niem-tin-phat-trien-va-hoan-thien-he-thong-chi-so-cho-thi-truong-chung-khoan-post875553.html — `[W11-C07]` (báo chí nhà nước, dùng để đối chiếu mốc lịch sử VN-Index)
- Đài Truyền hình Việt Nam. (2026, 29 tháng 7). *26 năm thị trường chứng khoán Việt Nam: Nâng tầm kênh dẫn vốn cho nền kinh tế*. https://vtv.vn/26-nam-thi-truong-chung-khoan-viet-nam-nang-tam-kenh-dan-von-cho-nen-kinh-te-100260728204034855.htm — `[W11-C07]` (báo chí nhà nước, đối chiếu mốc cơ sở 100 điểm ngày 28/7/2000)
- Ví dụ định lượng `[W11-C09]`: phép tính minh họa của bài học (giá trần/sàn và chỉ số rổ giả định), đã kiểm tra số học độc lập ngày 2026-09-15; không phải dữ liệu thị trường.
- Sổ cái nguồn: [Investing/quality/module-3-source-ledger.md](../../quality/module-3-source-ledger.md) — trạng thái từng claim, ngày kiểm tra và giới hạn.

### Bước Kế Tiếp
1. Ôn tập bộ Flashcards: `flashcards/week-11.yaml`.
2. Làm bài kiểm tra đánh giá: `assessments/quizzes/week-11-quiz.md`.
3. Đối chiếu đáp án phân tích sâu: `assessments/answer-keys/week-11-answers.md`.
4. Chuyển tiếp tuần sau: W12 — Mở tài khoản, lệnh giao dịch, phí và thuế tại Việt Nam (`notes/module-3/week-12-tai-khoan-lenh-phi-thue-vn.md`), nơi hạ tầng giao dịch đã nắm trong bài này sẽ gắn với quy trình thực tế của nhà đầu tư cá nhân.
