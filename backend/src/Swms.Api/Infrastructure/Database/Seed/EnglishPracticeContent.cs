// Auto-generated extended content for English Communication Course (A2 -> B1)
// Contains rich Markdown theory, interactive quizzes, and expanded practice activities for all 6 lessons.
namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;

public static class EnglishPracticeContent
{
    public const string UpgradeMarker = "<!-- studxy-en-v3 -->";
    public const string LegacyUpgradeMarker = "<!-- studxy-en-v2 -->";

    public record LessonSeedMeta(
        Guid LessonId,
        Guid ChapterId,
        string Title,
        string Slug,
        int SortOrder,
        int EstimatedMinutes,
        string Markdown,
        string Summary1,
        string Summary2
    );

    public static List<LessonSeedMeta> GetLessonMetas()
    {
        return new List<LessonSeedMeta>
        {
            new LessonSeedMeta(
                Guid.Parse("44444444-4444-4444-8444-444444444446"),
                Guid.Parse("33333333-3333-4333-8333-333333333334"),
                @"Bài 1.1: Small Talk, Giới Thiệu Bản Thân & Phản Xạ Tự Nhiên",
                @"small-talk-gioi-thieu-ban-than-phan-xa",
                1,
                20,
                @"# Bài 1.1: Small Talk, Giới Thiệu Bản Thân & Phản Xạ Tự Nhiên

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững công thức giới thiệu bản thân 3 bước: **Hook** (Mở lời gây chú ý) - **Background** (Chuyên môn / Nơi làm việc) - **Passion & Goal** (Mục tiêu kết nối).
- Tự tin bắt chuyện (Small Talk) trong các sự kiện quốc tế, hội thảo khoa học và môi trường đa văn hóa.
- Làm chủ kỹ năng Shadowing ngữ điệu bản xứ và Dictation nghe - chép chính tả phản xạ.
- Khắc phục triệt để các lỗi phát âm người Việt hay gặp: âm vô thanh `/θ/`, hữu thanh `/ð/`, âm cuối `/s/, /t/, /d/` và hiện tượng nối âm.

---

## 2. Đoạn hội thoại mẫu thực tế (Model Dialogue)
**Bối cảnh:** Giờ giải lao tại Hội thảo Công nghệ Quốc tế Vietnam Tech Summit. Alex (kỹ sư giải pháp đến từ Melbourne, Úc) và Mai (kỹ sư phần mềm tại Hà Nội) tình cờ gặp nhau tại khu vực cà phê.

- **Alex:** Hi there! Excuse me, is this seat taken?
- **Mai:** No, not at all, please go ahead! Beautiful morning for a conference, isn't it?
- **Alex:** Absolutely! The keynote speaker was really inspiring. I'm Alex, by the way. I work on cloud solutions in Melbourne.
- **Mai:** Pleasure to meet you, Alex! I'm Mai. I am a software engineer based in Hanoi, focusing on distributed systems.
- **Alex:** Fantastic! Is this your first time attending the summit, or do you come here regularly?
- **Mai:** This is actually my second time. I'm hoping to connect with fellow engineers and exchange insights on system scalability.
- **Alex:** That aligns perfectly with our workshop this afternoon. Would you mind if we stay in touch on LinkedIn?
- **Mai:** That would be wonderful! Here is my QR code. I look forward to our fruitful discussions.

---

## 3. 🌱 Bước khởi động cho người mới bắt đầu (Beginner Warm-up)
Nếu bạn đã lâu không dùng tiếng Anh hoặc cảm thấy bối rối trước đoạn hội thoại trên, đừng lo lắng! Hãy quan sát các khối xây dựng cơ bản nhất:

### 3.1. Khung câu cơ bản S - V - O (Chủ ngữ + Động từ + Tân ngữ)
- Mọi câu tiếng Anh đều cần **Chủ ngữ (S)** và **Động từ chính (V)**:
  - *Ví dụ 1:* `I (S)` + `work (V)` + `on cloud solutions (Bổ ngữ)`.
  - *Ví dụ 2:* `The keynote speaker (S)` + `was (V To-Be)` + `really inspiring (Tính từ)`.
  - Người Việt hay nói tắt không chủ ngữ (như *""Trời đẹp nhỉ""*), nhưng tiếng Anh luôn cần chủ ngữ giả hoặc câu hỏi đuôi: *""Beautiful morning, isn't it?""*.

### 3.2. Khi nào dùng Is / Are (Động từ To Be)?
- Dùng **is** khi chủ ngữ là số ít hoặc danh từ không đếm được:
  - *""is this seat taken?""* (`this seat` là 1 chiếc ghế duy nhất).
- Dùng **are** khi chủ ngữ là số nhiều hoặc đại từ `you / we / they`:
  - *""You are a software engineer.""*, *""We are colleagues.""*.

### 3.3. Vì sao động từ có lúc thêm -s/-es, có lúc không?
- Khi kể sự việc hiển nhiên hoặc thói quen ở Hiện tại đơn:
  - Ngôi thứ 3 số ít (`He / She / It / Alex`): động từ thêm `-s`: *""That aligns perfectly""* (chủ ngữ `That` là số ít nên `align` thêm `s` thành `aligns`).
  - Ngôi `I / You / We / They`: động từ giữ nguyên mẫu: *""I work on cloud solutions.""*.

### 3.4. Hướng dẫn khẩu hình phát âm cho người Việt
- **Âm `/θ/` trong ""think, thank"":** Cắn nhẹ đầu lưỡi giữa hai hàm răng, thổi luồng gió nhẹ ra ngoài không rung thanh quản (đừng đọc thành ""th"" hay ""t"" tiếng Việt).
- **Âm `/ð/` trong ""this, that, there"":** Đặt lưỡi vị trí y hệt `/θ/` nhưng rung dây thanh quản trong cổ họng (không đọc thành ""đít"" hay ""dít"").

---

## 4. 🗣️ Phân tích hội thoại từng câu một (Line-by-line Breakdown)

| Người nói | Câu thoại tiếng Anh | Dịch nghĩa tự nhiên | Cụm từ hay dùng & Bẫy ngữ pháp cần tránh |
|---|---|---|---|
| **Alex** | *Hi there! Excuse me, is this seat taken?* | Xin chào bạn! Xin lỗi, chỗ này đã có ai ngồi chưa ạ? | • `Excuse me`: Mở lời lịch sự.<br>• `Is this seat taken?`: Thành ngữ tự nhiên nhất để hỏi ghế trống (tránh hỏi cộc lốc *""Sit here ok?""*). |
| **Mai** | *No, not at all, please go ahead! Beautiful morning for a conference, isn't it?* | Dạ chưa, bạn cứ tự nhiên ngồi nhé! Buổi sáng hôm nay thật đẹp cho một buổi hội thảo, bạn nhỉ? | • `Not at all`: Hoàn toàn không sao.<br>• `Please go ahead`: Cứ tự nhiên làm điều bạn muốn.<br>• `..., isn't it?`: Câu hỏi đuôi thân thiện phá tan sự e ngại (break the ice). |
| **Alex** | *Absolutely! The keynote speaker was really inspiring. I'm Alex, by the way. I work on cloud solutions in Melbourne.* | Chắc chắn rồi! Diễn giả mở màn thực sự rất truyền cảm hứng. Nhân tiện, tôi là Alex. Tôi làm về các giải pháp điện toán đám mây tại Melbourne. | • `keynote speaker`: Diễn giả chính mở màn sự kiện.<br>• `by the way`: Nhân tiện, tiện thể.<br>• `work on [field]`: Làm việc trong lĩnh vực gì. |
| **Mai** | *Pleasure to meet you, Alex! I'm Mai. I am a software engineer based in Hanoi, focusing on distributed systems.* | Rất hân hạnh được gặp bạn, Alex! Tôi là Mai. Tôi là kỹ sư phần mềm tại Hà Nội, chuyên sâu về các hệ thống phân tán. | • `Pleasure to meet you`: Lịch sự hơn *""Nice to meet you""* trong bối cảnh nghề nghiệp.<br>• `based in [City]`: Đang sinh sống và làm việc tại.<br>• ⚠️ **Bẫy:** Phải có mạo từ `a` trong *""a software engineer""*, không được nói *""I am software engineer""*. |
| **Alex** | *Fantastic! Is this your first time attending the summit, or do you come here regularly?* | Tuyệt quá! Đây là lần đầu tiên bạn tham dự hội nghị này, hay bạn đến đây thường xuyên? | • `first time attending`: Lần đầu tham dự.<br>• `regularly` /ˈreɡjələli/: Đều đặn, thường xuyên (trọng âm rơi vào âm 1). |
| **Mai** | *This is actually my second time. I'm hoping to connect with fellow engineers and exchange insights on system scalability.* | Thực ra đây là lần thứ hai của tôi. Tôi hy vọng sẽ được kết nối với các bạn kỹ sư đồng nghiệp và trao đổi kinh nghiệm về khả năng mở rộng hệ thống. | • `fellow engineers`: Các kỹ sư đồng nghiệp.<br>• `exchange insights`: Trao đổi góc nhìn, kinh nghiệm quý báu.<br>• `system scalability`: Khả năng mở rộng quy mô hệ thống. |
| **Alex** | *That aligns perfectly with our workshop this afternoon. Would you mind if we stay in touch on LinkedIn?* | Điều đó hoàn toàn trùng khớp với buổi hội thảo thực hành của chúng tôi chiều nay. Bạn có phiền nếu chúng ta giữ liên lạc qua LinkedIn không? | • `align with`: Hoàn toàn ăn khớp, phù hợp với.<br>• `Would you mind if + S + V`: Mẫu câu xin phép lịch thiệp tuyệt đối. |
| **Mai** | *That would be wonderful! Here is my QR code. I look forward to our fruitful discussions.* | Điều đó thật tuyệt vời! Đây là mã QR của tôi. Tôi rất mong đợi những buổi thảo luận hiệu quả của chúng ta. | • `That would be wonderful`: Đồng ý nhiệt tình và nhã nhặn.<br>• `look forward to + V-ing / Noun`: Cực kỳ trông đợi (⚠️ Không dùng động từ nguyên mẫu sau *to* ở cụm này). |

---

## 5. Ngữ pháp & Cấu trúc câu trọng tâm (Key Grammar & Sentence Structures)

### 5.1. Công thức giới thiệu 3 bước Hook - Background - Passion
1. **Bước 1 - Hook (Phá vỡ khoảng cách):**
   - *Công thức:* `Excuse me / Hi there + Friendly observation / Question?`
   - *Ví dụ:* ""Beautiful morning for a conference, isn't it?"" *(Hôm nay trời đẹp cho buổi hội thảo nhỉ?)*
2. **Bước 2 - Background (Chuyên môn & Vị trí):**
   - *Công thức:* `I work as a/an [Role] based in [City/Company], specializing in [Field].`
   - *Ví dụ:* ""I work as a software engineer based in Hanoi, specializing in backend architectures.""
3. **Bước 3 - Passion & Goal (Mục đích kết nối):**
   - *Công thức:* `I'm keen on / I'm looking forward to [V-ing / Noun].`
   - *Ví dụ:* ""I look forward to learning from experienced mentors and building long-term relationships.""

### 5.2. Cấu trúc câu hỏi gián tiếp & câu hỏi đuôi lịch thiệp (Tag Questions)
- Khi bắt chuyện, người bản xứ thường dùng câu hỏi đuôi hoặc câu hỏi gợi mở thay vì hỏi dồn dập:
  - *Positive statement + Negative tag:* ""Great turnout today, isn't it?""
  - *Indirect inquiry:* ""Do you happen to know when the next session starts?""

---

## 6. Từ vựng & Cụm từ then chốt (Key Vocabulary & Collocations)

| Từ vựng / Cụm từ | Phiên âm IPA | Nghĩa tiếng Việt | Ví dụ thực hành |
|---|---|---|---|
| **break the ice** | `/breɪk ðiː aɪs/` | Phá vỡ sự e ngại ban đầu | A warm smile helps break the ice immediately. |
| **make a lasting impression** | `/meɪk ə ˈlɑːstɪŋ ɪmˈpreʃn/` | Tạo ấn tượng sâu sắc lâu dài | Active listening helps make a lasting impression. |
| **fruitful collaboration** | `/ˈfruːtfl kəˌlæbəˈreɪʃn/` | Sự hợp tác hiệu quả, thành công | We look forward to a fruitful collaboration. |
| **look forward to** | `/lʊk ˈfɔːwəd tuː/` | Rất mong đợi (+ V-ing / Danh từ) | I look forward to connecting with your team. |
| **strike up a conversation** | `/straɪk ʌp ə ˌkɒnvəˈseɪʃn/` | Chủ động bắt chuyện | It is easy to strike up a conversation at lunch. |
| **common ground** | `/ˈkɒmən ɡraʊnd/` | Điểm tương đồng, tiếng nói chung | We quickly found common ground in tech innovation. |
| **rapport** | `/ræˈpɔːr/` | Mối quan hệ hòa hợp, thấu hiểu | Building rapport is crucial before discussing business. |

---

## 7. Lỗi phát âm & ngữ pháp người Việt hay mắc (Common Pitfalls & Fixes)

### 7.1. Cặp âm vô thanh `/θ/` và hữu thanh `/ð/`
- **Lỗi thường gặp:** Người Việt hay phát âm `/θ/` thành `/t/` hoặc `/s/` (*thank* đọc thành *tanh* hoặc *sanh*); phát âm `/ð/` thành `/d/` hoặc `/z/` (*this* đọc thành *đít*).
- **Cách khắc phục:** Đặt đầu lưỡi nhẹ nhàng giữa hai hàm răng, thổi luồng hơi ra:
  - Với `/θ/` (không rung dây thanh): *thank you* `/θæŋk juː/`, *think* `/θɪŋk/`, *three* `/θriː/`.
  - Với `/ð/` (có rung dây thanh): *this* `/ðɪs/`, *that* `/ðæt/`, *there* `/ðeə/`, *with* `/wɪð/`.

### 7.2. Quên âm cuối (Ending Sounds) & Đuôi `-s/-es`
- Tiếng Việt là ngôn ngữ đơn âm, không bật âm đuôi. Trong tiếng Anh, thiếu âm đuôi làm đổi nghĩa:
  - *seat* `/siːt/` khác *see* `/siː/`.
  - *projects* `/ˈprɒdʒekts/` bắt buộc bật cả cụm phụ âm `/kts/`.
  - *relationships* `/rɪˈleɪʃnʃɪps/` phải có âm xì `/ps/`.

### 7.3. Nối âm (Linking Consonant to Vowel)
- Trong cụm *is this seat taken*, nối âm: `seat_is` -> `/siːt ɪz/`.
- Cụm *look forward to*: không đọc ngắt quãng, nối âm nhẹ nhàng: `look_forward`.

### 7.4. Lỗi ngữ pháp: Thiếu mạo từ nghề nghiệp
- **Sai:** *""I am software engineer in Hanoi.""* (Người Việt hay bỏ quên mạo từ).
- **Đúng:** *""I am a software engineer based in Hanoi.""* (Nghề nghiệp đếm được số ít luôn cần *a/an*).
- **Sai:** *""I look forward to meet you.""*
- **Đúng:** *""I look forward to meeting you.""* (*look forward to* luôn đi với V-ing).

---

## 8. 📝 Bài tập tự luyện từng bước (Từ dễ đến khó) + Đề tự kiểm tra cuối bài

### Bài tập 1: Điền mạo từ hoặc dạng từ thích hợp (Cực dễ cho người mới)
1. I am (a / an / ø) ______ frontend developer based in Da Nang.
2. She looks forward to (meet / meeting) ______ the keynote speaker.
3. Is this seat (take / taken) ______?
4. Alex works (in / on) ______ cloud infrastructure solutions.

### Bài tập 2: Sắp xếp các cụm từ thành câu hoàn chỉnh
1. based / engineer / I / software / in / am / a / Hanoi / .
2. conference / Beautiful / isn't / a / morning / for / it / ?
3. would / wonderful / That / be / !
4. stay / in / on / Would / if / we / touch / you / mind / LinkedIn / ?

---

<details>
<summary>👉 Bấm vào đây để Xem đáp án & Giải thích từng câu</summary>

### Đáp án Bài tập 1:
1. **a** — ""frontend developer"" bắt đầu bằng phụ âm /f/, dùng mạo từ `a`.
2. **meeting** — Cấu trúc `look forward to + V-ing`.
3. **taken** — Thể bị động miêu tả trạng thái chiếc ghế: `is this seat taken?`.
4. **on** — Cụm collocations `work on something` (làm việc về mảng gì).

### Đáp án Bài tập 2:
1. **I am a software engineer based in Hanoi.** *(Tôi là một kỹ sư phần mềm tại Hà Nội).*
2. **Beautiful morning for a conference, isn't it?** *(Buổi sáng thật đẹp cho buổi hội thảo, phải không?)*
3. **That would be wonderful!** *(Điều đó thật tuyệt vời!)*
4. **Would you mind if we stay in touch on LinkedIn?** *(Bạn có phiền nếu chúng ta giữ liên lạc qua LinkedIn không?)*

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->",
                @"Nắm vững kỹ năng mở đầu cuộc trò chuyện tự nhiên và giới thiệu bản thân 3 bước.",
                @"Thực hành Shadowing và Dictation phản xạ với các âm hay sai."
            ),
            new LessonSeedMeta(
                Guid.Parse("44444444-4444-4444-8444-444444444447"),
                Guid.Parse("33333333-3333-4333-8333-333333333334"),
                @"Bài 1.2: Hỏi Đường, Mua Sắm & Xử Lý Tình Huống Thực Tế",
                @"hoi-duong-mua-sam-xu-ly-tinh-huong",
                2,
                20,
                @"# Bài 1.2: Hỏi Đường, Mua Sắm & Xử Lý Tình Huống Thực Tế

## 1. Mục tiêu bài học (Learning Objectives)
- Sử dụng thành thạo các cấu trúc hỏi đường và chỉ phương hướng (giving and asking for directions) trong đô thị quốc tế.
- Tự tin đi lại bằng phương tiện giao thông công cộng (tàu điện ngầm metro, xe bus sân bay, taxi công nghệ).
- Xử lý các tình huống mua sắm thực tế: hỏi size, hỏi chính sách đổi trả, mặc cả lịch sự và thanh toán không tiền mặt (contactless / credit card).
- Nắm vững cách phát âm các cụm phụ âm khó `/st/`, `/sk/` và các âm đuôi `/tʃ/`, `/dʒ/`.

---

## 2. Đoạn hội thoại mẫu thực tế (Model Dialogues)

### Tình huống 1: Hỏi đường tại ga tàu trung tâm (Transit & Navigation)
- **Du khách (Traveler):** Excuse me, could you tell me how to get to the international terminal from here?
- **Nhân viên ga (Station Attendant):** Certainly! Take the escalator down to platform two, board the blue line train, and ride it for three stops.
- **Du khách:** Thank you! Is it within walking distance if I prefer to walk with my luggage?
- **Nhân viên ga:** It takes about twenty-five minutes on foot, but with heavy bags, taking the automated shuttle is much more convenient.
- **Du khách:** That sounds great. Where can I tap my contactless card for the ticket?
- **Nhân viên ga:** Right over there at the smart gate. Just tap and go!

### Tình huống 2: Mua sắm và hoàn thuế (Shopping & Tax Refund)
- **Khách hàng (Shopper):** Hello, do you have this wool sweater in a medium size?
- **Thu ngân (Cashier):** Let me check our inventory... Yes, here is the last one in navy blue!
- **Khách hàng:** Wonderful! Does this store offer tax-free shopping for international tourists?
- **Thu ngân:** Yes, absolutely. If your total purchase exceeds fifty dollars, we can issue an official tax refund receipt.
- **Khách hàng:** Perfect. Can I pay with Apple Pay or credit card?
- **Thu ngân:** Both are accepted. Please hold your device near the reader.

---

## 3. 🌱 Bước khởi động cho người mới bắt đầu (Beginner Warm-up)

### 3.1. Khung câu mệnh lệnh thức chỉ đường (Imperative Sentences)
- Khi chỉ đường, người bản xứ dùng động từ nguyên mẫu đứng đầu câu (không cần chủ ngữ):
  - `Take the escalator down...` *(Hãy đi thang cuốn xuống...)*
  - `Board the blue line train...` *(Hãy bước lên chuyến tàu tuyến xanh...)*
  - `Turn right at the intersection.` *(Rẽ phải ở ngã tư).*

### 3.2. Cấu trúc câu hỏi gián tiếp: Tuyệt đối KHÔNG đảo ngữ
- Người mới bắt đầu hay hỏi trực diện theo kiểu dịch tiếng Việt: *""Where is the station?""* (nghe hơi cộc lốc).
- Người bản xứ chuộng câu hỏi gián tiếp: `Could you tell me where [Chủ ngữ] + [Động từ]?`.
  - ⚠️ **Quy tắc vàng:** Sau từ hỏi (`where/how`), giữ nguyên trật tự `S + V`, không đảo `is/can` lên trước:
  - *Đúng:* `Could you tell me where the pharmacy is?`
  - *Sai:* `Could you tell me where is the pharmacy?`

### 3.3. Vì sao dùng ""It takes + thời gian""?
- Cấu trúc diễn tả mất bao nhiêu thời gian để đi đâu: `It takes + [Thời gian] + on foot / by bus`.
  - *""It takes twenty minutes on foot.""* *(Mất 20 phút đi bộ).*

### 3.4. Hướng dẫn khẩu hình phát âm cho người Việt
- **Cụm phụ âm `/st/` (straight, station):** Khép nhẹ hai hàm răng, xì luồng hơi gió `/s/` trước, rồi lập tức bật âm `/t/`. Tuyệt đối không chèn âm ""tơ-rết"" hay ""xì-tây-sần"".
- **Âm cuối `/tʃ/` (touch, purchase):** Chu tròn môi, bật luồng hơi gió dứt khoát không rung cổ họng.
- **Âm cuối `/dʒ/` (bridge, change, luggage):** Chu tròn môi nhưng rung thanh quản mạnh ở cổ.

---

## 4. 🗣️ Phân tích hội thoại từng câu một (Line-by-line Breakdown)

| Tình huống & Người nói | Câu thoại tiếng Anh | Dịch nghĩa tự nhiên | Cụm từ hay dùng & Bẫy ngữ pháp cần tránh |
|---|---|---|---|
| **Transit — Khách** | *Excuse me, could you tell me how to get to the international terminal from here?* | Xin lỗi, bạn có thể chỉ cho tôi cách đến nhà ga quốc tế từ đây được không? | • `could you tell me how to get to...`: Mẫu câu hỏi đường lịch sự nhất.<br>• `international terminal`: Nhà ga sân bay quốc tế. |
| **Transit — Attendant** | *Certainly! Take the escalator down to platform two, board the blue line train, and ride it for three stops.* | Dĩ nhiên rồi! Bạn hãy đi thang cuốn xuống ke số hai, lên tàu tuyến xanh và đi qua ba ga dừng nhé. | • `escalator`: Thang cuốn tự động.<br>• `platform`: Ke ga, sân ga.<br>• `board`: Lên tàu / máy bay.<br>• `ride it for three stops`: Đi qua 3 trạm dừng. |
| **Transit — Khách** | *Thank you! Is it within walking distance if I prefer to walk with my luggage?* | Cảm ơn bạn! Liệu chỗ đó có trong khoảng cách đi bộ được không nếu tôi muốn đi bộ với hành lý? | • `within walking distance`: Khoảng cách gần có thể đi bộ.<br>• `luggage`: Hành lý (danh từ không đếm được, không có số nhiều *luggages*). |
| **Transit — Attendant** | *It takes about twenty-five minutes on foot, but with heavy bags, taking the automated shuttle is much more convenient.* | Mất khoảng hai mươi lăm phút đi bộ, nhưng với túi xách nặng thì đi xe đưa đón tự động sẽ tiện hơn nhiều. | • `on foot`: Đi bộ.<br>• `automated shuttle`: Xe buýt điện trung chuyển tự động. |
| **Transit — Khách** | *That sounds great. Where can I tap my contactless card for the ticket?* | Nghe hay quá. Tôi có thể chạm thẻ không tiếp xúc để mua vé ở đâu vậy? | • `That sounds great`: Lời đáp thể hiện sự ưng ý.<br>• `contactless card`: Thẻ thanh toán chạm. |
| **Transit — Attendant** | *Right over there at the smart gate. Just tap and go!* | Ngay đằng kia ở cổng thông minh. Chỉ cần chạm thẻ là đi qua thôi! | • `Right over there`: Ngay ở đằng kia.<br>• `tap and go`: Chạm và đi (khẩu hiệu thanh toán metro phổ biến). |
| **Shop — Khách** | *Hello, do you have this wool sweater in a medium size?* | Xin chào, cửa hàng có chiếc áo len lông cừu này ở cỡ vừa (size M) không ạ? | • `wool sweater`: Áo len dệt từ lông cừu.<br>• `in a medium size`: Ở cỡ size vừa. |
| **Shop — Thu ngân** | *Let me check our inventory... Yes, here is the last one in navy blue!* | Để tôi kiểm tra kho hàng... Vâng, đây là chiếc cuối cùng màu xanh navy! | • `inventory`: Kho hàng, danh mục tồn kho.<br>• `the last one`: Chiếc cuối cùng còn lại. |
| **Shop — Khách** | *Wonderful! Does this store offer tax-free shopping for international tourists?* | Tuyệt vời! Cửa hàng mình có chính sách mua sắm miễn thuế cho khách du lịch quốc tế không? | • `tax-free shopping`: Mua sắm hoàn thuế / miễn thuế.<br>• `international tourists`: Khách du lịch quốc tế. |
| **Shop — Thu ngân** | *Yes, absolutely. If your total purchase exceeds fifty dollars, we can issue an official tax refund receipt.* | Dạ có chứ. Nếu tổng hóa đơn mua sắm của bạn vượt quá năm mươi đô la, chúng tôi có thể xuất hóa đơn hoàn thuế chính thức. | • `total purchase exceeds`: Tổng giá trị mua sắm vượt qua mức.<br>• `tax refund receipt`: Hóa đơn hoàn thuế. |
| **Shop — Khách** | *Perfect. Can I pay with Apple Pay or credit card?* | Tuyệt. Tôi có thể thanh toán bằng Apple Pay hay thẻ tín dụng được không? | • Phương thức thanh toán phổ biến khi đi công tác nước ngoài. |
| **Shop — Thu ngân** | *Both are accepted. Please hold your device near the reader.* | Cả hai đều được chấp nhận ạ. Xin mời bạn đưa thiết bị lại gần máy đọc thẻ. | • `hold your device near the reader`: Giữ thiết bị gần đầu đọc NFC. |

---

## 5. Ngữ pháp & Cấu trúc câu trọng tâm (Key Grammar & Sentence Structures)

### 5.1. Câu hỏi gián tiếp lịch sự (Indirect Questions)
- Thay vì hỏi trực tiếp cộc lốc (*""Where is the train station?""*), người bản xứ luôn dùng câu hỏi gián tiếp:
  - *Công thức:* `Could you please tell me + where / how / whether + S + V?`
  - *Lưu ý vàng:* **Không đảo trợ động từ lên trước chủ ngữ!**
  - *Đúng:* ""Could you tell me where the nearest pharmacy **is**?"" (Không dùng *is the nearest pharmacy*).

### 5.2. Cụm từ chỉ phương hướng và vị trí không gian
- **straight ahead** `/streɪt əˈhed/`: Đi thẳng về phía trước.
- **turn left / right at the intersection**: Rẽ trái / phải ở ngã tư.
- **take the second exit at the roundabout**: Đi lối ra thứ hai ở bùng binh.
- **across from / opposite to**: Đối diện với.
- **within walking distance**: Trong khoảng cách có thể đi bộ được.

### 5.3. Mẫu câu mua sắm & thanh toán
- ""Do you have this item in stock?"" *(Món đồ này còn hàng không?)*
- ""Can I try this on in the fitting room?"" *(Tôi có thể thử món này trong phòng thay đồ không?)*
- ""Do you accept contactless payments or cards?"" *(Cửa hàng có nhận chạm thẻ hoặc thanh toán thẻ không?)*

---

## 6. Từ vựng & Cụm từ then chốt (Key Vocabulary & Collocations)

| Từ vựng / Cụm từ | Phiên âm IPA | Nghĩa tiếng Việt | Ví dụ thực hành |
|---|---|---|---|
| **intersection** | `/ˌɪntəˈsekʃn/` | Ngã tư, giao lộ | Turn right at the next busy intersection. |
| **roundabout** | `/ˈraʊndəbaʊt/` | Vòng xuyến, bùng binh | Take the second exit at the roundabout. |
| **pedestrian crossing** | `/pəˈdestriən ˈkrɒsɪŋ/` | Vạch kẻ cho người đi bộ | Use the pedestrian crossing for safety. |
| **contactless payment** | `/ˈkɒntæktləs ˈpeɪmənt/` | Thanh toán chạm thẻ | Contactless payment is fast and secure. |
| **tax refund** | `/tæks ˈriːfʌnd/` | Hoàn thuế | Keep your receipt for the airport tax refund. |
| **within walking distance** | `/wɪðˈɪn ˈwɔːkɪŋ ˈdɪstəns/` | Gần, đi bộ được | The museum is within walking distance. |
| **in stock** | `/ɪn stɒk/` | Còn hàng trong kho | We currently have all sizes in stock. |

---

## 7. Lỗi phát âm & ngữ pháp người Việt hay mắc (Common Pitfalls & Fixes)

### 7.1. Cụm phụ âm đầu `/st/` và `/sk/`
- **Lỗi thường gặp:** Người Việt hay bỏ âm `/s/` hoặc chèn nguyên âm đệm: đọc *straight* thành *tơ-rết* hoặc *rayt*; đọc *station* thành *tây-sần*.
- **Cách sửa:** Khép răng, thổi gió xì âm `/s/` trước rồi mới bật phụ âm `/t/`: `/streɪt/`, `/ˈsteɪʃn/`, `/skjuːz/` (*excuse*).

### 7.2. Âm cuối `/tʃ/` (ch) và `/dʒ/` (j)
- *touch* `/tʌtʃ/`, *purchase* `/ˈpɜːtʃəs/`: phải chu môi bật hơi dứt khoát.
- *bridge* `/brɪdʒ/`, *change* `/tʃeɪndʒ/`, *luggage* `/ˈlʌɡɪdʒ/`: rung cổ họng khi phát âm âm cuối `/dʒ/`.

### 7.3. Lỗi ngữ pháp: Đảo ngữ sai trong câu hỏi gián tiếp
- **Sai:** *""Could you tell me how can I get to the airport?""*
- **Đúng:** *""Could you tell me how I can get to the airport?""*
- **Sai:** *""Do you know what time does the shop close?""*
- **Đúng:** *""Do you know what time the shop closes?""*

---

## 8. 📝 Bài tập tự luyện từng bước (Từ dễ đến khó) + Đề tự kiểm tra cuối bài

### Bài tập 1: Chọn từ đúng điền vào câu hỏi gián tiếp
1. Could you tell me where (is the station / the station is)?
2. Do you know what time the store (opens / does open)?
3. Can I pay (by / with) contactless card?
4. Is the hotel within (walk / walking) distance?

### Bài tập 2: Sắp xếp các từ thành câu chỉ đường hoàn chỉnh
1. the / escalator / to / platform / Take / two / down / .
2. the / exit / at / second / Take / roundabout / the / .
3. device / hold / near / reader / Please / your / the / .

---

<details>
<summary>👉 Bấm vào đây để Xem đáp án & Giải thích từng câu</summary>

### Đáp án Bài tập 1:
1. **the station is** — Câu hỏi gián tiếp không đảo trợ động từ lên trước chủ ngữ.
2. **opens** — Sau từ hỏi *what time*, mệnh đề giữ trật tự S + V ở hiện tại đơn (chủ ngữ số ít *the store* đi với *opens*).
3. **with** (hoặc *by*) — *pay with a card / pay by card*.
4. **walking** — Cụm collocations cố định: *within walking distance*.

### Đáp án Bài tập 2:
1. **Take the escalator down to platform two.** *(Hãy đi thang cuốn xuống ke ga số hai).*
2. **Take the second exit at the roundabout.** *(Hãy đi theo lối ra thứ hai ở bùng binh).*
3. **Please hold your device near the reader.** *(Xin vui lòng giữ thiết bị của bạn gần đầu đọc thẻ).*

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->",
                @"Hỏi đường và xử lý các tình huống đời sống, mua sắm và thanh toán thực tế.",
                @"Mẫu câu thực tế và phản xạ nhanh khi du lịch hoặc công tác nước ngoài."
            ),
            new LessonSeedMeta(
                Guid.Parse("44444444-4444-4444-8444-444444444448"),
                Guid.Parse("33333333-3333-4333-8333-333333333335"),
                @"Bài 2.1: Viết Email Công Việc, Đặt Lịch Hẹn & Báo Cáo Tiến Độ",
                @"viet-email-cong-viec-dat-lich-hen-bao-cao",
                1,
                25,
                @"# Bài 2.1: Viết Email Công Việc, Đặt Lịch Hẹn & Báo Cáo Tiến Độ

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững cấu trúc email công sở quốc tế chuẩn mực (Subject line - Salutation - Purpose - Core Details - Call to Action - Sign-off).
- Sử dụng thành thạo văn phong trang trọng vừa phải (semi-formal & formal tone), tránh các từ ngữ quá suồng sã hoặc quá cứng nhắc.
- Đặt lịch hẹn, dời lịch họp (reschedule) khéo léo và báo cáo tiến độ tuần theo khung phương pháp SMART.
- Làm chủ cách phát âm các động từ quá khứ phân từ đuôi `-ed` (`/t/`, `/d/`, `/ɪd/`) và phụ âm cuối số nhiều `/s/`, `/z/`.

---

## 2. Mẫu Email công việc thực tế (Sample Professional Emails)

### Mẫu 1: Email báo cáo tiến độ dự án theo tuần (Status Update)
**Subject:** [Status Update] Sprint 14 Deliverables & Next Milestones - Team Atlas

Dear Mr. Harrison,

I am writing to provide a concise update regarding our progress on Sprint 14. 

Over the past week, our engineering team has successfully finalized the authentication microservice and conducted extensive integration testing. All critical deliverables remain on track for the staging deployment this Friday. 

Please find attached the detailed metric breakdown for your review. Would you be available for a brief fifteen-minute sync tomorrow at 3:00 PM to review the final release checklist?

Thank you for your continuous support.

Best regards,  
**Mai Tran**  
*Senior Software Engineer | Swms Ecosystem*

---

### Mẫu 2: Email xin dời lịch hẹn khéo léo (Polite Rescheduling Request)
**Subject:** Rescheduling Request: Q3 Marketing Alignment Sync

Dear Ms. Anderson,

I hope this email finds you well.

Due to an urgent production incident that requires my immediate presence, I regret to inform you that I will be unable to attend our scheduled alignment meeting this afternoon at 2:00 PM.

Please accept my sincere apologies for this short-notice inconvenience. Would any of the following alternative time slots work with your schedule?
- Tomorrow (Thursday) at 10:30 AM
- Friday at 2:00 PM

Please let me know which option suits you best, and I will gladly update the calendar invitation accordingly.

Thank you very much for your understanding.

Sincerely,  
**David Nguyen**  
*Technical Lead | Product Operations*

---

## 3. 🌱 Bước khởi động cho người mới bắt đầu (Beginner Warm-up)

### 3.1. Khung xương của một bức email công việc chuẩn quốc tế
Mọi email tiếng Anh chuyên nghiệp đều tuân theo 5 khối logic đơn giản:
1. **Tiêu đề (Subject Line):** Rõ ràng, có nhãn phân loại: `[Status Update] ...` hoặc `Rescheduling Request: ...`.
2. **Chào hỏi (Salutation):** `Dear Mr. Harrison,` hoặc `Dear Ms. Anderson,`.
3. **Mục đích viết thư (Purpose):** `I am writing to provide an update on...` hoặc `I am writing to request...`.
4. **Hành động tiếp theo (Call to Action):** `Would you be available for a brief sync tomorrow at 3:00 PM?`.
5. **Chào kết (Sign-off):** `Best regards,` hoặc `Sincerely,` kèm chữ ký chức danh.

### 3.2. Thì Hiện tại hoàn thành (Present Perfect) trong báo cáo tiến độ
- Khi báo cáo kết quả vừa hoàn thành và còn nóng hổi giá trị đến hiện tại, ta dùng:
  $$\text{have / has} + \text{V3 / V-ed}$$
  - *""Our team has finalized the authentication microservice.""* *(Nhóm chúng tôi đã hoàn tất vi dịch vụ xác thực).*
  - Dùng `has` vì chủ ngữ `our team` là một tập thể số ít.

### 3.3. Câu điều kiện lịch thiệp với ""Would""
- Để hỏi đối tác có rảnh không, không dùng *""Do you have time?""* (nghe hơi suồng sã). Hãy dùng:
  - `Would you be available for a brief sync on [Day] at [Time]?`
  - Chữ `Would` làm câu nói trở nên mềm mại và tôn trọng đối phương.

### 3.4. 3 Quy tắc vàng phát âm đuôi -ed không bao giờ sai
1. **Nhóm 1 - Đọc là `/ɪd/`:** Chỉ khi động từ tận cùng bằng âm `/t/` hoặc `/d/`:
   - *completed* `/kəmˈpliːtɪd/`, *decided* `/dɪˈsaɪdɪd/`.
2. **Nhóm 2 - Đọc là `/t/`:** Khi tận cùng là các âm vô thanh (`/k, p, s, ʃ, tʃ, f/`):
   - *launched* `/lɔːntʃt/`, *fixed* `/fɪkst/`, *developed* `/dɪˈveləpt/`.
3. **Nhóm 3 - Đọc là `/d/`:** Tất cả các âm còn lại (âm rung cổ họng):
   - *finalized* `/ˈfaɪnəlaɪzd/`, *streamlined* `/ˈstriːmlaɪnd/`, *deployed* `/dɪˈplɔɪd/`.

---

## 4. 🗣️ Phân tích hội thoại từng câu một (Line-by-line Breakdown)

| Đoạn trích email | Phân tích ngữ nghĩa tự nhiên | Điểm ngữ pháp then chốt & Từ vựng công sở |
|---|---|---|
| *I am writing to provide a concise update regarding our progress on Sprint 14.* | Tôi viết thư này nhằm cung cấp bản cập nhật ngắn gọn về tiến độ của nhóm trong Sprint 14. | • `concise`: Ngắn gọn, súc tích, đi thẳng vào trọng tâm.<br>• `regarding`: Về vấn đề gì (trang trọng hơn *about*). |
| *Over the past week, our engineering team has successfully finalized the authentication microservice.* | Trong tuần vừa qua, nhóm kỹ thuật của chúng tôi đã hoàn thiện thành công dịch vụ xác thực. | • `Over the past week`: Trong suốt tuần qua (dấu hiệu Hiện tại hoàn thành).<br>• `finalize`: Hoàn tất khâu cuối cùng. |
| *All critical deliverables remain on track for the staging deployment this Friday.* | Toàn bộ các sản phẩm bàn giao then chốt vẫn đang đúng tiến độ cho đợt triển khai môi trường staging thứ Sáu này. | • `critical deliverables`: Các hạng mục bàn giao trọng yếu.<br>• `on track`: Đúng lộ trình, đúng tiến độ. |
| *Please find attached the detailed metric breakdown for your review.* | Xin vui lòng xem tệp đính kèm bảng phân tích chỉ số chi tiết để anh xem xét. | • Mẫu câu đính kèm tệp chuẩn mực nhất trong thư giao dịch. |
| *Due to an urgent production incident that requires my immediate presence...* | Do có một sự cố nghiêm trọng trên hệ thống vận hành đòi hỏi sự có mặt trực tiếp ngay lập tức của tôi... | • `urgent production incident`: Sự cố kỹ thuật khẩn cấp trên hệ thống thực tế.<br>• `immediate presence`: Sự hiện diện ngay tức khắc. |
| *I regret to inform you that I will be unable to attend our scheduled alignment meeting...* | Tôi rất tiếc phải thông báo rằng tôi sẽ không thể tham dự cuộc họp định hướng đã lên lịch... | • `regret to inform you`: Rất tiếc phải báo tin (văn phong dời lịch trang trọng). |
| *Please accept my sincere apologies for this short-notice inconvenience.* | Xin vui lòng chấp nhận lời xin lỗi chân thành của tôi vì sự bất tiện gấp rút này. | • `sincere apologies`: Lời xin lỗi chân thành.<br>• `short-notice inconvenience`: Bất tiện do thông báo quá gấp. |

---

## 5. Ngữ pháp & Cấu trúc câu trọng tâm (Key Grammar & Sentence Structures)

### 5.1. Các mẫu câu mở đầu email chuyên nghiệp
- *Mở đầu trang trọng:* ""I hope this email finds you well."" *(Hy vọng email này tìm thấy bạn trong trạng thái tốt đẹp).*
- *Nêu mục đích trực tiếp:* ""I am writing to update you on..."", ""I am writing to request your approval regarding...""
- *Hạn chế:* Tránh viết cộc lốc *""I want to tell you about...""* trong thư giao dịch.

### 5.2. Cấu trúc câu hỏi điều kiện lịch thiệp (Courteous Requests)
- `Would you be available for a brief sync on [Day] at [Time]?`
- `Could you please confirm if this timeframe aligns with your availability?`
- `Should your schedule permit, we would greatly appreciate your input.`

### 5.3. Thì Hiện tại hoàn thành (Present Perfect) trong báo cáo tiến độ
- Dùng Present Perfect (`have/has + V3/ed`) để báo cáo các kết quả vừa hoàn thành có tác động đến hiện tại:
  - *""We have finalized the core modules.""* (Đã hoàn thiện các module cốt lõi).
  - *""Our team has resolved all blocking defects.""* (Nhóm đã khắc phục toàn bộ lỗi chặn tiến độ).

---

## 6. Từ vựng & Cụm từ then chốt (Key Vocabulary & Collocations)

| Từ vựng / Cụm từ | Phiên âm IPA | Nghĩa tiếng Việt | Ví dụ thực hành |
|---|---|---|---|
| **deliverable** | `/dɪˈlɪvərəbl/` | Sản phẩm bàn giao, đầu ra | All sprint deliverables are ready for review. |
| **milestone** | `/ˈmaɪlstəʊn/` | Mốc tiến độ quan trọng | We reached a major technical milestone today. |
| **streamline** | `/ˈstriːmlaɪn/` | Tinh gọn, tối ưu hóa | Automation streamlines our deployment workflow. |
| **schedule conflict** | `/ˈʃedjuːl ˈkɒnflɪkt/` | Trùng lịch, xung đột lịch | A sudden schedule conflict forced us to postpone. |
| **tentative agenda** | `/ˈtentətɪv əˈdʒendə/` | Chương trình làm việc dự kiến | I have attached the tentative agenda for tomorrow. |
| **action item** | `/ˈækʃn ˈaɪtəm/` | Đầu việc hành động cụ thể | Let's review the action items from last week. |
| **bottleneck** | `/ˈbɒtlnek/` | Điểm nghẽn tiến độ | Code reviews became our primary bottleneck. |

---

## 7. Lỗi phát âm & ngữ pháp người Việt hay mắc (Common Pitfalls & Fixes)

### 7.1. Quy tắc phát âm đuôi `-ed` trong động từ công sở
- **Nhóm 1 - Phát âm `/ɪd/`:** Khi động từ kết thúc bằng `/t/` hoặc `/d/`:
  - *completed* `/kəmˈpliːtɪd/`, *decided* `/dɪˈsaɪdɪd/`, *requested* `/rɪˈkwestɪd/`.
- **Nhóm 2 - Phát âm `/t/`:** Khi kết thúc bằng phụ âm vô thanh (`/k, p, s, ʃ, tʃ, f/`):
  - *launched* `/lɔːntʃt/`, *developed* `/dɪˈveləpt/`, *fixed* `/fɪkst/`.
- **Nhóm 3 - Phát âm `/d/`:** Các trường hợp còn lại (âm hữu thanh & nguyên âm):
  - *finalized* `/ˈfaɪnəlaɪzd/`, *streamlined* `/ˈstriːmlaɪnd/`, *deployed* `/dɪˈplɔɪd/`.

### 7.2. Âm cuối `/z/` trong danh từ số nhiều công sở
- *milestones* `/ˈmaɪlstəʊnz/`, *deliverables* `/dɪˈlɪvərəblz/`, *resources* `/rɪˈzɔːsɪz/`.
- Lỗi người Việt: Nuốt âm xì cuối làm câu nghe rời rạc và mất đi tính chính xác ngữ pháp.

### 7.3. Lỗi ngữ pháp: Trộn lẫn quá khứ đơn và hiện tại hoàn thành
- **Sai:** *""We finalized the task this morning and everything is ready now.""* (Nếu muốn nhấn mạnh kết quả còn nóng hổi).
- **Đúng:** *""We have finalized the task this morning and everything is ready now.""*
- **Sai:** *""Would you like to meet tomorrow at 2 PM? Wanna join?""* (Trộn lẫn trang trọng và tiếng lóng).
- **Đúng:** *""Would you be available to meet tomorrow at 2:00 PM?""*

---

## 8. 📝 Bài tập tự luyện từng bước (Từ dễ đến khó) + Đề tự kiểm tra cuối bài

### Bài tập 1: Phân loại cách đọc đuôi -ed (/t/, /d/, hay /ɪd/)
1. completed -> ______
2. launched -> ______
3. streamlined -> ______
4. requested -> ______

### Bài tập 2: Điền từ thích hợp vào chỗ trống trong email
1. I am writing to provide a ______ update on Sprint 14. (concise / conciseness)
2. All deliverables remain ______ track. (on / in)
3. Please find ______ the metric breakdown. (attach / attached)
4. Would you be available ______ a brief meeting? (for / with)

---

<details>
<summary>👉 Bấm vào đây để Xem đáp án & Giải thích từng câu</summary>

### Đáp án Bài tập 1:
1. **completed** -> `/ɪd/` (kết thúc bằng âm /t/).
2. **launched** -> `/t/` (kết thúc bằng âm vô thanh /tʃ/).
3. **streamlined** -> `/d/` (kết thúc bằng phụ âm hữu thanh /n/).
4. **requested** -> `/ɪd/` (kết thúc bằng âm /t/).

### Đáp án Bài tập 2:
1. **concise** — Cần tính từ đứng trước danh từ *update*.
2. **on** — Cụm thành ngữ *on track* (đúng tiến độ).
3. **attached** — Cụm từ *find attached* (tìm thấy tệp đính kèm).
4. **for** — Cấu trúc *available for something* (rảnh rỗi cho việc gì).

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->",
                @"Viết email chuyên nghiệp, đặt lịch hẹn và báo cáo tiến độ công việc theo chuẩn SMART.",
                @"Cấu trúc văn phong văn phòng chuẩn quốc tế và phản xạ giao tiếp công sở."
            ),
            new LessonSeedMeta(
                Guid.Parse("44444444-4444-4444-8444-444444444449"),
                Guid.Parse("33333333-3333-4333-8333-333333333335"),
                @"Bài 2.2: Hội Họp Trực Tuyến, Trình Bày Ý Kiến & Thống Nhất Phương Án",
                @"hoi-hop-truc-tuyen-trinh-bay-y-kien",
                2,
                25,
                @"# Bài 2.2: Hội Họp Trực Tuyến, Trình Bày Ý Kiến & Thống Nhất Phương Án

## 1. Mục tiêu bài học (Learning Objectives)
- Làm chủ bộ mẫu câu điều phối và tham gia cuộc họp trực tuyến chuyên nghiệp (Virtual Meeting Etiquette: kiểm tra âm thanh, chia sẻ màn hình, xử lý gián đoạn).
- Diễn đạt quan điểm chuyên môn tự tin, mạch lạc và súc tích trong môi trường đa quốc gia.
- Thực hành kỹ thuật phản biện ngoại giao (Diplomatic Disagreement) nhằm bảo vệ quan điểm mà vẫn giữ gìn hòa khí đồng nghiệp.
- Tổng kết biên bản cuộc họp, chốt các đầu việc hành động (Action Items) và gán quyền sở hữu công việc (Task Ownership) rõ ràng.

---

## 2. Đoạn hội thoại cuộc họp trực tuyến mẫu (Online Meeting Dialogue)
**Bối cảnh:** Buổi họp kỹ thuật Sprint Retrospective & Architecture Review qua Zoom giữa các kỹ sư tại Việt Nam, Úc và Singapore.

- **Chair (Robert):** Good morning, everyone! Can everyone hear me clearly and see my shared screen?
- **Mai:** Loud and clear, Robert! Your presentation slide is coming through perfectly.
- **Robert:** Excellent. Let's get down to business. Today our objective is to decide whether to migrate our monolith services to a microservices architecture. Mai, could you walk us through the proposed blueprint?
- **Mai:** Certainly. As you can see on slide four, decoupling our frontend from the backend services will significantly enhance horizontal scalability. This reduces database contention during peak traffic hours.
- **David (Singapore):** I see your point, Mai, and the scalability benefits are compelling. However, have we taken network latency and cross-service debugging overhead into account?
- **Mai:** That is a valid concern, David. To mitigate latency, we plan to implement lightweight gRPC communication and distributed tracing. Would that address your team's worries?
- **David:** Yes, that sounds like a viable compromise. I'm comfortable moving forward with a small prototype.
- **Robert:** Great consensus. Let's summarize our action items: Mai will lead the prototype development, and David will review the security benchmarks by next Wednesday.

---

## 3. 🌱 Bước khởi động cho người mới bắt đầu (Beginner Warm-up)

### 3.1. Khung câu bắt đầu cuộc họp trực tuyến
- Để bắt đầu cuộc họp chuyên nghiệp, người chủ trì (Chair) luôn kiểm tra 2 yếu tố: Âm thanh (Audio) và Màn hình (Video):
  - `Can everyone hear me clearly?` *(Mọi người có nghe rõ tiếng tôi không?)*
  - `Can everyone see my shared screen?` *(Mọi người có thấy màn hình tôi đang chia sẻ không?)*
- Phản xạ đáp lại nhanh chóng và tự nhiên:
  - `Loud and clear!` *(Nghe to và rõ ràng lắm ạ!)*
  - `Coming through perfectly!` *(Hình ảnh/âm thanh truyền tới rất mượt mà!)*

### 3.2. Công thức phản biện ngoại giao (Diplomatic Disagreement)
- Trong môi trường làm việc quốc tế, không bao giờ nói trực diện *""You are wrong""* hoặc *""I disagree""* vì dễ gây căng thẳng.
- Hãy dùng công thức **Đệm công nhận + Nêu góc nhìn khác**:
  $$\text{I see your point / That is a valid concern} + \text{however, have we considered [Y]?}$$
  - Điều này giúp đồng nghiệp thấy ý kiến của họ được trân trọng trước khi bàn về rủi ro.

### 3.3. Phân biệt cặp âm phụ âm `/v/` vs `/w/`
- **Âm `/v/` (video, valid, viable):** Đặt răng cửa trên chạm nhẹ vào lòng môi dưới, đẩy hơi và rung dây thanh quản (giống âm ""v"" tiếng Việt nhưng chặt hơn).
- **Âm `/w/` (window, workshop, walk):** Hai môi chúm tròn lại thành hình chữ ""O"" nhỏ rồi mở rộng ra (không để răng chạm vào môi!).

---

## 4. 🗣️ Phân tích hội thoại từng câu một (Line-by-line Breakdown)

| Người nói | Câu thoại tiếng Anh | Dịch nghĩa tự nhiên | Cụm từ hay dùng & Bẫy ngữ pháp cần tránh |
|---|---|---|---|
| **Robert (Chair)** | *Good morning, everyone! Can everyone hear me clearly and see my shared screen?* | Chào buổi sáng mọi người! Mọi người có nghe rõ tiếng tôi và thấy màn hình chia sẻ không ạ? | • Mẫu câu mở đầu cuộc họp trực tuyến chuẩn mực quốc tế. |
| **Mai** | *Loud and clear, Robert! Your presentation slide is coming through perfectly.* | Nghe rất to và rõ, Robert ơi! Slide thuyết trình của anh hiển thị rất mượt mà. | • `Loud and clear`: Thành ngữ phản xạ âm thanh hoàn hảo.<br>• `coming through`: Truyền qua mạng thông suốt. |
| **Robert** | *Excellent. Let's get down to business. Today our objective is to decide whether to migrate...* | Tuyệt vời. Chúng ta hãy bắt đầu vào việc chính thôi. Mục tiêu hôm nay là quyết định xem có nên chuyển đổi... | • `get down to business`: Đi thẳng vào công việc chính.<br>• `whether to [verb]`: Liệu có nên làm gì hay không. |
| **Mai** | *Certainly. As you can see on slide four, decoupling our frontend from the backend services will significantly enhance horizontal scalability.* | Chắc chắn rồi. Như mọi người thấy ở slide bốn, việc tách rời frontend khỏi backend sẽ gia tăng đáng kể khả năng mở rộng theo chiều ngang. | • `decoupling`: Tách rời kiến trúc độc lập.<br>• `horizontal scalability`: Khả năng mở rộng theo chiều ngang (thêm máy chủ). |
| **David** | *I see your point, Mai, and the scalability benefits are compelling. However, have we taken network latency into account?* | Tôi hiểu ý bạn rồi, Mai, và lợi ích về khả năng mở rộng rất thuyết phục. Tuy nhiên, chúng ta đã tính đến độ trễ mạng chưa? | • `I see your point`: Tôi hiểu góc nhìn của bạn.<br>• `compelling`: Đầy sức thuyết phục.<br>• `take into account`: Tính đến, cân nhắc đến yếu tố nào. |
| **Mai** | *That is a valid concern, David. To mitigate latency, we plan to implement lightweight gRPC communication. Would that address your team's worries?* | Đó là một mối bận tâm rất xác đáng, David. Để giảm thiểu độ trễ, chúng tôi dự định áp dụng giao tiếp gRPC tinh gọn. Liệu điều đó có giải tỏa được nỗi lo của đội bạn không? | • `valid concern`: Mối quan ngại hoàn toàn chính đáng.<br>• `mitigate`: Giảm nhẹ rủi ro / tổn thất.<br>• `address someone's worries`: Giải quyết nỗi lo lắng của ai. |
| **David** | *Yes, that sounds like a viable compromise. I'm comfortable moving forward with a small prototype.* | Được đó, nghe như một giải pháp thỏa hiệp khả thi. Tôi yên tâm để tiến hành với một bản thử nghiệm nhỏ. | • `viable compromise`: Thỏa hiệp khả thi.<br>• `comfortable moving forward`: Yên tâm tiến bước tiếp theo. |
| **Robert** | *Great consensus. Let's summarize our action items: Mai will lead the prototype development, and David will review...* | Sự đồng thuận tuyệt vời. Chúng ta hãy tóm tắt các đầu việc hành động: Mai sẽ dẫn dắt bản thử nghiệm, còn David sẽ rà soát... | • `consensus`: Sự nhất trí, đồng lòng.<br>• `action items`: Các đầu việc cần làm sau họp.<br>• `lead`: Giữ vai trò chủ trì. |

---

## 5. Ngữ pháp & Cấu trúc câu trọng tâm (Key Grammar & Sentence Structures)

### 5.1. Ngôn ngữ điều phối và kiểm tra kỹ thuật trực tuyến
- *Kiểm tra mic / kết nối:* ""Can you hear me alright?"", ""Am I coming through clearly?""
- *Chia sẻ màn hình:* ""Let me share my screen. Please let me know once you see the slides.""
- *Xử lý quên bật mic:* ""Alex, I believe you are on mute."" *(Alex ơi, hình như bạn đang tắt mic).*
- *Xử lý nói chen ngang lịch thiệp:* ""Sorry to cut you off, please go ahead."" *(Xin lỗi vì đã ngắt lời bạn, xin mời bạn nói tiếp).*

### 5.2. Cấu trúc phản biện ngoại giao (Diplomatic Disagreement)
- Thay vì nói cộc lốc *""You are wrong""* hoặc *""I disagree""*, người bản xứ luôn dùng các đệm xoa dịu (Softening Devices):
  - `I see your point, but we should also evaluate [Risk/Aspect].`
  - `That is a valid observation; however, have we considered [Alternative]?`
  - `While I agree in principle, wouldn't it be safer to start with a prototype?`
  - `From my perspective, allocating resources here might yield better long-term ROI.`

### 5.3. Cấu trúc chốt việc (Action Items & Task Ownership)
- `Let's wrap up by assigning clear ownership for each deliverable.`
- `[Person] will take the lead on [Task], with [Person] supporting on [Review].`
- `The hard deadline is agreed for [Day/Date].`

---

## 6. Từ vựng & Cụm từ then chốt (Key Vocabulary & Collocations)

| Từ vựng / Cụm từ | Phiên âm IPA | Nghĩa tiếng Việt | Ví dụ thực hành |
|---|---|---|---|
| **consensus** | `/kənˈsensəs/` | Sự đồng thuận, nhất trí | We reached a unanimous consensus on the roadmap. |
| **bandwidth** | `/ˈbændwɪdθ/` | Băng thông / Thời gian, năng lực | Does your team have the bandwidth for extra tasks? |
| **take offline** | `/teɪk ɒfˈlaɪn/` | Thảo luận riêng sau cuộc họp | Let's take this technical detail offline. |
| **table a discussion** | `/ˈteɪbl ə dɪˈskʌʃn/` | Tạm gác thảo luận lại lần sau | We will table this topic until more data arrives. |
| **walk through** | `/wɔːk θruː/` | Trình bày cặn kẽ từng bước | Please walk us through the system architecture. |
| **cross-functional** | `/krɒs ˈfʌŋkʃənl/` | Liên phòng ban, đa chức năng | Cross-functional alignment prevents project silos. |
| **viable compromise** | `/ˈvaɪəbl ˈkɒmprəmaɪz/` | Giải pháp thỏa hiệp khả thi | Both teams agreed on a viable compromise. |

---

## 7. Lỗi phát âm & ngữ pháp người Việt hay mắc (Common Pitfalls & Fixes)

### 7.1. Nối âm trong họp trực tuyến (Linking Sounds)
- *Can everyone* -> đọc nối âm: `/kæn ˈevriwʌn/` thành `Can_everyone`.
- *Walk us through* -> `/wɔːk ʌs θruː/` nối âm thành `walk_us through`.
- *Turn off* -> nối âm `turn_off`.

### 7.2. Cặp phụ âm `/v/` và `/w/`
- Người Việt hay đọc `/v/` thành `/w/` hoặc ngược lại:
  - *video* `/ˈvɪdiəʊ/` (răng trên chạm môi dưới, rung) khác *window* `/ˈwɪndəʊ/` (môi tròn chúm lại).
  - *valid* `/ˈvælɪd/`, *viable* `/ˈvaɪəbl/`, *evaluate* `/ɪˈvæljueɪt/`.

### 7.3. Lỗi ngữ pháp: Tranh luận trực diện thiếu tính ngoại giao
- **Kém chuyên nghiệp:** *""Your idea is impossible because our team doesn't have time.""*
- **Chuẩn mực ngoại giao:** *""That is an interesting proposition; however, given our current bandwidth constraints, could we explore Phase 2 implementation?""*

---

## 8. 📝 Bài tập tự luyện từng bước (Từ dễ đến khó) + Đề tự kiểm tra cuối bài

### Bài tập 1: Điền cụm từ phù hợp khi họp trực tuyến
1. When you want to discuss a detail after the meeting, you say: ""Let's take this ______."" (online / offline)
2. When someone speaks while muted, you say: ""You are on ______."" (mute / quiet)
3. When everyone agrees on a decision, they have reached a ______."" (conflict / consensus)
4. ""Loud and ______!"" (clear / clarity)

### Bài tập 2: Sắp xếp thành câu phản biện ngoại giao
1. concern / That / valid / a / David / is / .
2. this / walk / Could / us / proposed / you / blueprint / through / the / ?
3. action / summarize / Let's / items / our / .

---

<details>
<summary>👉 Bấm vào đây để Xem đáp án & Giải thích từng câu</summary>

### Đáp án Bài tập 1:
1. **offline** — *take something offline* (thảo luận riêng ngoài cuộc họp).
2. **mute** — *you are on mute* (bạn đang tắt mic).
3. **consensus** — *reach a consensus* (đạt được sự đồng thuận).
4. **clear** — *loud and clear* (nghe to và rõ ràng).

### Đáp án Bài tập 2:
1. **That is a valid concern, David.** *(Đó là một mối quan ngại hoàn toàn chính đáng, David).*
2. **Could you walk us through the proposed blueprint?** *(Bạn có thể trình bày cặn kẽ bản thiết kế đề xuất cho chúng tôi được không?)*
3. **Let's summarize our action items.** *(Chúng ta hãy cùng tóm tắt lại các đầu việc hành động).*

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->",
                @"Hội họp trực tuyến, chủ trì thảo luận và diễn đạt ý kiến chuyên môn rõ ràng.",
                @"Kỹ năng phản biện ngoại giao, tìm giải pháp đồng thuận và thuyết phục đội ngũ."
            ),
            new LessonSeedMeta(
                Guid.Parse("44444444-4444-4444-8444-444444444450"),
                Guid.Parse("33333333-3333-4333-8333-333333333336"),
                @"Bài 3.1: Cấu Trúc Bài Thuyết Trình Tinh Gọn (Opening - Body - Conclusion)",
                @"cau-truc-bai-thuyet-trinh-tinh-gon",
                1,
                25,
                @"# Bài 3.1: Cấu Trúc Bài Thuyết Trình Tinh Gọn (Opening - Body - Conclusion)

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững công thức xây dựng bài thuyết trình tiếng Anh 5 phút chuẩn cấu trúc 3 phần: **Opening** (Mở bài gây ấn tượng) - **Body** (Thân bài logic, có số liệu minh họa) - **Conclusion** (Kết bài đọng lại và kêu gọi hành động).
- Sử dụng thành thạo ngôn ngữ chỉ đường (Signposting Language) để dẫn dắt người nghe qua từng phần thuyết trình mượt mà.
- Trình bày biểu đồ, xu hướng số liệu (Trends & Data visualization) bằng từ vựng chuẩn xác và giàu sức gợi cảm.
- Khắc phục lỗi ngữ điệu đều đều (flat intonation), làm chủ trọng âm câu (sentence stress) và nhịp ngắt nghỉ (pausing techniques) để cuốn hút khán giả.

---

## 2. Kịch bản bài thuyết trình 5 phút mẫu (Sample 5-Minute Pitch)
**Chủ đề:** Tăng Tốc Năng Suất Kỹ Thuật Bằng Văn Hóa Học Tập Liên Tục (Continuous Learning).

### Phần 1: Opening (Mở đầu gây ấn tượng - 1 phút)
""Good morning, esteemed colleagues and distinguished guests. Did you know that over seventy percent of engineering productivity bottlenecks stem not from software bugs, but from knowledge silos? Today, I am excited to demonstrate how embedding continuous learning into our daily rituals accelerates both technical velocity and individual career growth.""

### Phần 2: Body (Thân bài mạch lạc có số liệu - 3 phút)
- **Luận điểm 1 - Hiện trạng:** ""To begin with, let us look at our baseline telemetry on slide three. Over the past two quarters, sprint velocity plateaued as team members spent significant time resolving duplicate architectural questions.""
- **Luận điểm 2 - Giải pháp & Dẫn chứng số liệu:** ""Moving on to our key initiative: by instituting weekly thirty-minute knowledge-sharing sessions, our code review turnaround time dropped by forty-two percent, while defect leakage plummeted to near zero.""
- **Luận điểm 3 - Tác động lâu dài:** ""Turning our attention to the economic dividends: investing in continuous learning yields compounding returns, dramatically lowering onboarding costs for new hires.""

### Phần 3: Conclusion (Kết luận & Kêu gọi hành động - 1 phút)
""In conclusion, disciplined daily practice and open knowledge exchange transform competent teams into industry-leading squads. As we look ahead to next quarter, I invite each of you to champion one peer-learning session within your respective domains. Thank you for your time, and I would now be delighted to take any questions.""

---

## 3. 🌱 Bước khởi động cho người mới bắt đầu (Beginner Warm-up)

### 3.1. Cấu trúc 3 phần bất biến của một bài nói 5 phút
Người mới thuyết trình hay sợ bị ""quên bài"" hoặc nói lan man. Bí quyết là bám sát khung 3 bước:
1. **Tell them what you are going to tell them (Opening):** Giới thiệu chủ đề và lý do vì sao khán giả nên quan tâm.
2. **Tell them (Body):** Trình bày tối đa 3 ý chính, mỗi ý kèm một con số thực tế.
3. **Tell them what you told them (Conclusion):** Tóm tắt 1 câu cốt lõi và kêu gọi hành động (Call to action).

### 3.2. Ngôn ngữ chỉ đường (Signposting Language) là gì?
- Giống như các biển chỉ dẫn trên đường cao tốc, biển chỉ đường bằng lời giúp khán giả biết bạn đang ở đâu:
  - *Bắt đầu:* `To begin with, let us look at...`
  - *Chuyển ý:* `Moving on to...`, `Turning our attention to...`
  - *Chốt bài:* `In conclusion...`, `To wrap up today's talk...`

### 3.3. Phân biệt động từ Rise vs Raise
- **rise** (nội động từ, tự tăng lên, KHÔNG có tân ngữ phía sau):
  - *""Our sprint velocity rose by twenty percent.""* (Vận tốc chạy sprint tăng 20%).
- **raise** (ngoại động từ, chủ động nâng cái gì lên, BẮT BUỘC có tân ngữ phía sau):
  - *""We raised our quality standards.""* (Chúng tôi đã nâng chuẩn chất lượng lên).

### 3.4. Kỹ thuật ngắt nghỉ kịch tính (Dramatic Pause)
- Người nói không tự tin thường có xu hướng đọc nhanh như đuổi theo kịch bản.
- Hãy hít sâu và dừng lại 1-2 giây sau khi đặt câu hỏi mở màn (*""Did you know that...?"" [Dừng 2 giây]*). Khoảng lặng này khiến cả hội trường tập trung chú ý vào bạn!

---

## 4. 🗣️ Phân tích hội thoại từng câu một (Line-by-line Breakdown)

| Đoạn thuyết trình | Ý nghĩa tự nhiên | Kỹ thuật thuyết trình & Lưu ý ngữ pháp |
|---|---|---|
| *Did you know that over seventy percent of bottlenecks stem not from software bugs, but from knowledge silos?* | Bạn có biết rằng hơn bảy mươi phần trăm các điểm nghẽn năng suất không phải xuất phát từ lỗi phần mềm, mà từ tình trạng ứ đọng kiến thức cục bộ? | • `Hook with Rhetorical Question`: Câu hỏi tu từ gây tò mò tức thì.<br>• `stem from`: Xuất phát từ nguyên nhân nào.<br>• `knowledge silos`: Tình trạng giấu nghề, cô lập kiến thức giữa các nhóm. |
| *Today, I am excited to demonstrate how embedding continuous learning accelerates technical velocity...* | Hôm nay, tôi rất hào hứng được chứng minh việc gắn việc học liên tục vào thói quen hàng ngày sẽ tăng tốc độ kỹ thuật như thế nào... | • `demonstrate`: Thị phạm, chứng minh.<br>• `embedding`: Nhúng, lồng ghép vào.<br>• `velocity`: Vận tốc, tốc độ xử lý. |
| *To begin with, let us look at our baseline telemetry on slide three.* | Đầu tiên, chúng ta hãy cùng nhìn vào các chỉ số đo lường gốc ở trang chiếu số ba. | • `To begin with`: Cụm chỉ dẫn bắt đầu luận điểm 1.<br>• `baseline telemetry`: Dữ liệu đo lường cơ sở ban đầu. |
| *...sprint velocity plateaued as team members spent significant time resolving duplicate questions.* | ...tốc độ sprint đã chững lại do các thành viên phải dành nhiều thời gian xử lý những thắc mắc trùng lặp. | • `plateaued`: Chững lại sau một chu kỳ tăng (tính từ/động từ miêu tả biểu đồ rất hay). |
| *Moving on to our key initiative: our turnaround time dropped by forty-two percent, while defect leakage plummeted...* | Chuyển sang sáng kiến then chốt: thời gian phản hồi duyệt code đã giảm bốn mươi hai phần trăm, trong khi lỗi lọt ra ngoài giảm chạm đáy... | • `turnaround time`: Thời gian hoàn thành một chu trình xử lý.<br>• `plummeted`: Rơi thẳng đứng, giảm cực mạnh. |
| *In conclusion, disciplined daily practice transforms competent teams into industry-leading squads.* | Tóm lại, việc rèn luyện kỷ luật mỗi ngày sẽ biến những đội ngũ có năng lực thành những biệt đội dẫn đầu ngành. | • `In conclusion`: Cụm từ chốt thông điệp.<br>• `disciplined daily practice`: Sự rèn luyện kỷ luật mỗi ngày. |
| *Thank you for your time, and I would now be delighted to take any questions.* | Xin cảm ơn thời gian quý báu của quý vị, và bây giờ tôi rất vui lòng được giải đáp mọi câu hỏi. | • Lời kết chuẩn mực và lịch sự mở màn phần hỏi đáp (Q&A). |

---

## 5. Ngữ pháp & Cấu trúc câu trọng tâm (Key Grammar & Sentence Structures)

### 5.1. Ngôn ngữ dẫn dắt chuyển ý (Signposting Language)
- *Mở đầu bài nói:* ""Today, I would like to demonstrate how [Subject] accomplishes [Goal].""
- *Chuyển sang luận điểm mới:* ""Moving on to our next key metric..."", ""Turning our attention to the customer retention charts...""
- *Trình bày biểu đồ:* ""As illustrated by this upward trend on slide five..."", ""If you direct your attention to the bar graph...""
- *Tóm tắt kết bài:* ""In conclusion..."", ""To wrap up today's presentation..."", ""The key takeaway here is that...""

### 5.2. Cấu trúc mô tả xu hướng số liệu (Trends & Movement)
- **Tăng vọt:** `surged by [X]%`, `experienced an exponential surge`, `peaked at [Number]`.
- **Giảm mạnh:** `plummeted to [Number]`, `dropped sharply by [X]%`.
- **Ổn định / Chững lại:** `remained relatively steady`, `plateaued at [Level]`.

### 5.3. Câu nhấn mạnh chốt thông điệp (Cleft Sentences & Emphatics)
- Dùng cấu trúc chẻ (Cleft sentences) để khắc sâu thông điệp trong tâm trí khán giả:
  - *""What truly sets this strategy apart is our uncompromising focus on user empathy.""*
  - *""It is disciplined daily practice that yields compound interest in communication mastery.""*

---

## 6. Từ vựng & Cụm từ then chốt (Key Vocabulary & Collocations)

| Từ vựng / Cụm từ | Phiên âm IPA | Nghĩa tiếng Việt | Ví dụ thực hành |
|---|---|---|---|
| **signposting** | `/ˈsaɪnpəʊstɪŋ/` | Ngôn ngữ dẫn dắt bài nói | Clear signposting keeps listeners focused. |
| **demonstrate** | `/ˈdemənstreɪt/` | Chứng minh, thị phạm | Today I will demonstrate our prototype. |
| **compelling evidence** | `/kəmˈpelɪŋ ˈevɪdəns/` | Bằng chứng thuyết phục | The data provides compelling evidence of ROI. |
| **exponential surge** | `/ˌekspəˈnenʃl sɜːdʒ/` | Sự tăng trưởng cấp số nhân | We recorded an exponential surge in adoption. |
| **plateau** | `/ˈplætəʊ/` | Chững lại ở mức ổn định | User acquisition plateaued after three months. |
| **key takeaway** | `/kiː ˈteɪkəweɪ/` | Thông điệp cốt lõi cần nhớ | The key takeaway is to invest in automation. |
| **call to action** | `/kɔːl tuː ˈækʃn/` | Lời kêu gọi hành động | Every effective pitch ends with a call to action. |

---

## 7. Lỗi phát âm & ngữ pháp người Việt hay mắc (Common Pitfalls & Fixes)

### 7.1. Trọng âm từ (Word Stress)
- **Lỗi phổ biến:** Đọc đều các âm tiết hoặc nhấn sai vị trí trọng âm:
  - *demonstrate* `/ˈdemənstreɪt/`: trọng âm rơi vào âm tiết đầu tiên (**de**-mơn-strết).
  - *present* (danh từ/tính từ) là `/ˈpreznt/`, nhưng động từ *present* là `/prɪˈzent/` (nhấn âm 2).
  - *conclusion* `/kənˈkluːʒn/`: nhấn âm tiết thứ hai.

### 7.2. Ngữ điệu (Intonation) & Kỹ thuật ngắt nghỉ (Pausing)
- Đọc lướt không ngừng nghỉ khiến khán giả choáng ngợp và không theo kịp.
- Hãy dùng **Dramatic Pause (khoảng dừng kịch tính)** 1-2 giây ngay trước hoặc sau thông điệp quan trọng nhất.
- Hạ giọng cuối câu khẳng định (Falling intonation) để tạo cảm giác tự tin, chắc chắn.

### 7.3. Lỗi ngữ pháp: Phân biệt ""Rise"" vs ""Raise""
- **rise** (nội động từ, tự tăng, không có tân ngữ): *""Our metrics rose significantly.""* (Không dùng *raised*).
- **raise** (ngoại động từ, chủ động nâng cái gì lên, cần tân ngữ): *""We raised our service standards.""*

---

## 8. 📝 Bài tập tự luyện từng bước (Từ dễ đến khó) + Đề tự kiểm tra cuối bài

### Bài tập 1: Điền động từ Rise hoặc Raise thích hợp
1. The company ______ its subscription prices last month. (rose / raised)
2. Our customer satisfaction score ______ to an all-time high. (rose / raised)
3. Defect leakage ______ to near zero. (dropped / plummeted)

### Bài tập 2: Ghép cụm từ chỉ đường (Signposting) đúng ngữ cảnh
1. Khi bắt đầu bài nói -> ______ (In conclusion / To begin with)
2. Khi chuyển sang luận điểm số 2 -> ______ (Moving on to slide four / In summary)
3. Khi mời khán giả đặt câu hỏi -> ______ (I am ready to take any questions / That is all)

---

<details>
<summary>👉 Bấm vào đây để Xem đáp án & Giải thích từng câu</summary>

### Đáp án Bài tập 1:
1. **raised** — Có tân ngữ *its subscription prices* phía sau nên dùng ngoại động từ *raise*.
2. **rose** — Không có tân ngữ trực tiếp (chỉ có giới từ *to*), dùng nội động từ *rise*.
3. **plummeted** — Giảm cực mạnh, rơi thẳng đứng.

### Đáp án Bài tập 2:
1. **To begin with** — Bắt đầu bài thuyết trình.
2. **Moving on to slide four** — Chuyển ý mượt mà sang slide tiếp theo.
3. **I am ready to take any questions** — Lời mời hỏi đáp chuyên nghiệp.

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->",
                @"Cấu trúc bài thuyết trình tiếng Anh tinh gọn 5 phút theo chuẩn quốc tế.",
                @"Kỹ thuật dẫn dắt chuyển ý (signposting) và giữ chân người nghe hiệu quả."
            ),
            new LessonSeedMeta(
                Guid.Parse("44444444-4444-4444-8444-444444444451"),
                Guid.Parse("33333333-3333-4333-8333-333333333336"),
                @"Bài 3.2: Đàm Phán Lợi Ích, Phản Biện Khéo Léo & Thuyết Phục Đối Tác",
                @"dam-phan-loi-ich-phan-bien-kheo-leo",
                2,
                25,
                @"# Bài 3.2: Đàm Phán Lợi Ích, Phản Biện Khéo Léo & Thuyết Phục Đối Tác

## 1. Mục tiêu bài học (Learning Objectives)
- Nắm vững triết lý Đàm phán theo nguyên tắc (Principled Negotiation: đôi bên cùng có lợi - Win-Win Solution) thay vì lối mặc cả vị thế đối đầu (Positional Bargaining).
- Phân biệt rạch ròi giữa **Lợi ích cốt lõi (Interests)** và **Vị thế cứng nhắc (Positions)** để mở rộng không gian thương lượng.
- Sử dụng kỹ thuật nhượng bộ chiến lược có điều kiện (Conditional Concessions: *""If you commit to X, we can adjust Y""*).
- Làm chủ kỹ thuật đặt câu hỏi làm rõ (Clarifying Questions) khi đối phương từ chối hoặc đưa ra rào cản giá cả, tránh rơi vào bế tắc (impasse).

---

## 2. Kịch bản đàm phán hợp đồng mẫu (Principled Negotiation Dialogue)
**Bối cảnh:** Buổi đàm phán hợp đồng triển khai phần mềm quản trị doanh nghiệp giữa Giám đốc Mua hàng (Client Procurement Director - Sarah) và Trưởng đoàn tư vấn giải pháp (Consultant Lead - Minh).

- **Sarah (Khách hàng):** Minh, we genuinely appreciate your technological capabilities, but your proposed delivery timeline of six weeks seems far too tight for our internal security auditing. Additionally, your fee is fifteen percent above our budgeted cap.
- **Minh (Tư vấn):** We completely understand your concerns, Sarah. Rigorous security auditing is absolutely essential, and we would never want you to compromise on compliance. Could you help me understand which specific modules require the most scrutiny?
- **Sarah:** Our chief compliance officer must thoroughly inspect the cryptographic storage and identity access management layers.
- **Minh:** Understood. If we deliver those two core modules within the first three weeks for your security team to review, would that alleviate your scheduling pressure?
- **Sarah:** Yes, that phased delivery would give our auditors ample time. But what about the budget gap?
- **Minh:** We could adjust our overall implementation fee by ten percent on condition that your team commits to a two-year enterprise support agreement. Does that sound like a fair compromise?
- **Sarah:** That strikes a very reasonable balance between our immediate capital constraints and long-term system stability. Let's draft the agreement terms accordingly!

---

## 3. 🌱 Bước khởi động cho người mới bắt đầu (Beginner Warm-up)

### 3.1. Sự khác nhau giữa Lập trường (Position) và Lợi ích (Interest)
- Trong đàm phán, người chưa có kinh nghiệm thường bám chặt vào con số bề nổi:
  - **Lập trường cứng nhắc (Position):** *""Tôi chỉ trả 100 triệu, bớt 1 đồng tôi cũng không mua!""*.
  - **Lợi ích ngầm (Interest):** Người mua lo sợ phần mềm chậm tiến độ hoặc không được hỗ trợ bảo hành tốt.
- Khi người tư vấn khéo léo bóc tách lợi ích thực sự (như Minh đề xuất chia kỳ bàn giao sớm để kiểm thử an ninh), hai bên sẽ tìm thấy giải pháp thỏa hiệp hoàn hảo (Win-Win).

### 3.2. Cấu trúc câu nhượng bộ có điều kiện (Conditional Concessions)
- **Quy tắc đắt giá:** Không bao giờ giảm giá hoặc nhượng bộ không công (*""Ok, I will lower the price""* -> đối tác sẽ tiếp tục ép giá).
- Hãy luôn gắn điều kiện ràng buộc:
  $$\text{If you commit to [Điều kiện của bạn]}, \text{we can adjust [Ưu đãi cho họ]}.$$
  - ⚠️ **Ngữ pháp:** Trong mệnh đề `If`, dùng thì **Hiện tại đơn** (không dùng *will*):
  - *Đúng:* `If you commit to a two-year agreement, we will adjust the fee.`
  - *Sai:* `If you will commit to a two-year agreement...`

### 3.3. Hướng dẫn phát âm các âm khó trong đàm phán
- **Âm `/ʃ/` (s nặng chu môi):** Xuất hiện liên tục trong từ vựng đàm phán: *negotiation* `/nɪˌɡəʊʃiˈeɪʃn/`, *concession* `/kənˈseʃn/`, *crucial* `/ˈkruːʃl/`, *beneficial* `/ˌbenɪˈfɪʃl/`. Hãy chu môi tròn và bật hơi gió ma sát rõ rệt!
- **Âm `/dʒ/` (j rung cổ):** *adjust* `/əˈdʒʌst/`, *objection* `/əbˈdʒekʃn/`.

---

## 4. 🗣️ Phân tích hội thoại từng câu một (Line-by-line Breakdown)

| Người nói | Câu thoại tiếng Anh | Dịch nghĩa tự nhiên | Phân tích chiến thuật & Lưu ý ngữ pháp |
|---|---|---|---|
| **Sarah (Client)** | *...your proposed timeline of six weeks seems far too tight for our internal security auditing. Additionally, your fee is fifteen percent above our budgeted cap.* | ...thời gian bàn giao sáu tuần bạn đề xuất có vẻ quá gấp rút cho đợt kiểm toán an ninh nội bộ của chúng tôi. Ngoài ra, mức phí của bạn cao hơn mười lăm phần trăm so với trần ngân sách. | • `proposed delivery timeline`: Lộ trình bàn giao đề xuất.<br>• `budgeted cap`: Mức trần ngân sách cho phép.<br>• Khách hàng đưa ra 2 rào cản: Thời gian & Giá cả. |
| **Minh (Consultant)** | *We completely understand your concerns, Sarah. Rigorous security auditing is absolutely essential... Could you help me understand which specific modules require the most scrutiny?* | Chúng tôi hoàn toàn thấu hiểu lo ngại của chị, Sarah. Việc kiểm toán an ninh nghiêm ngặt là tuyệt đối cần thiết... Chị có thể giúp tôi hiểu mô-đun cụ thể nào đòi hỏi sự xem xét kỹ lưỡng nhất không? | • `rigorous security auditing`: Kiểm toán an ninh nghiêm ngặt.<br>• `scrutiny`: Sự soi xét, kiểm tra chi tiết.<br>• `Clarifying Question`: Đặt câu hỏi làm rõ thay vì cãi lại giá cả. |
| **Sarah** | *Our chief compliance officer must thoroughly inspect the cryptographic storage and identity access management layers.* | Giám đốc phụ trách tuân thủ của chúng tôi phải kiểm tra kỹ lưỡng các lớp lưu trữ mã hóa và quản lý truy cập danh tính. | • `chief compliance officer`: Giám đốc tuân thủ pháp chế.<br>• `cryptographic storage`: Lưu trữ mã hóa bảo mật. |
| **Minh** | *If we deliver those two core modules within the first three weeks for your security team to review, would that alleviate your scheduling pressure?* | Nếu chúng tôi bàn giao trước hai mô-đun cốt lõi đó trong ba tuần đầu để đội an ninh rà soát, liệu điều đó có giải tỏa được áp lực tiến độ cho chị không? | • `alleviate pressure`: Làm giảm bớt áp lực.<br>• Đưa ra giải pháp phân kỳ bàn giao (phased delivery) giải quyết triệt để vấn đề thời gian. |
| **Sarah** | *Yes, that phased delivery would give our auditors ample time. But what about the budget gap?* | Được đó, bàn giao theo giai đoạn sẽ giúp các kiểm toán viên có nhiều thời gian. Nhưng còn khoảng cách ngân sách thì sao? | • `ample time`: Rất nhiều thời gian, dư dả thời gian.<br>• `budget gap`: Khoảng chênh lệch ngân sách. |
| **Minh** | *We could adjust our overall implementation fee by ten percent on condition that your team commits to a two-year enterprise support agreement.* | Chúng tôi có thể điều chỉnh giảm mười phần trăm tổng phí triển khai với điều kiện bên chị cam kết ký thỏa thuận hỗ trợ doanh nghiệp hai năm. | • `on condition that`: Với điều kiện là.<br>• Chiến thuật nhượng bộ có điều kiện chuẩn mực (Conditional Concession). |
| **Sarah** | *That strikes a very reasonable balance between our immediate capital constraints and long-term stability. Let's draft the agreement terms accordingly!* | Điều đó tạo nên một sự cân bằng rất hợp lý giữa giới hạn vốn trước mắt và tính ổn định dài hạn của hệ thống. Chúng ta hãy cùng soạn thảo các điều khoản hợp đồng thôi! | • `strikes a reasonable balance`: Đạt được sự cân bằng hợp lý.<br>• `capital constraints`: Các ràng buộc về vốn/ngân sách.<br>• Kết thúc đàm phán Win-Win. |

---

## 5. Ngữ pháp & Cấu trúc câu trọng tâm (Key Grammar & Sentence Structures)

### 5.1. Kỹ thuật nhượng bộ có điều kiện (Conditional Concessions)
- Tuyệt đối không nhượng bộ đơn phương (*""Okay, we will lower the price""*). Luôn đi kèm điều kiện ràng buộc:
  - `If you can commit to [X], we will gladly adjust [Y].`
  - `We can agree to [Concession] on condition that [Requirement].`
  - `Provided that [Term is accepted], we are prepared to accommodate [Request].`

### 5.2. Câu hỏi làm rõ sự từ chối thay vì tranh cãi (Clarifying Questions)
- Khi đối tác nói *""Your price is too high""* hoặc *""That timeline is impossible""*:
  - *Không nói:* ""No, our price is very cheap compared to competitors!""
  - *Nói chuẩn mực:*
    - ""Could you elaborate on the primary constraints preventing your team from adopting this proposal?""
    - ""What would it take for this package to be acceptable to your steering committee?""

### 5.3. Mẫu câu chốt thỏa thuận và ghi nhận tiếng nói chung
- `Subject to legal review, we are comfortable moving forward with these terms.`
- `This solution represents a mutually beneficial agreement that safeguards both parties' interests.`

---

## 6. Từ vựng & Cụm từ then chốt (Key Vocabulary & Collocations)

| Từ vựng / Cụm từ | Phiên âm IPA | Nghĩa tiếng Việt | Ví dụ thực hành |
|---|---|---|---|
| **principled negotiation** | `/ˈprɪnsəpld nɪˌɡəʊʃiˈeɪʃn/` | Đàm phán theo nguyên tắc | Principled negotiation builds lasting trust. |
| **mutually beneficial** | `/ˈmjuːtʃuəli ˌbenɪˈfɪʃl/` | Đôi bên cùng có lợi (win-win) | We reached a mutually beneficial agreement. |
| **conditional concession** | `/kənˈdɪʃənl kənˈseʃn/` | Nhượng bộ có điều kiện | Always offer a conditional concession. |
| **break the impasse** | `/breɪk ðiː æmˈpɑːs/` | Phá vỡ thế bế tắc | A creative compromise broke the impasse. |
| **non-negotiable clause** | `/nɒn nɪˈɡəʊʃiəbl klɔːz/` | Điều khoản bất khả thương lượng | Data privacy is our non-negotiable clause. |
| **safeguard interests** | `/ˈseɪfɡɑːd ˈɪntrəsts/` | Bảo vệ lợi ích các bên | The contract safeguards both parties' interests. |
| **bargaining power** | `/ˈbɑːɡənɪŋ ˈpaʊə/` | Vị thế, quyền lực thương lượng | Unique technology boosts bargaining power. |

---

## 7. Lỗi phát âm & ngữ pháp người Việt hay mắc (Common Pitfalls & Fixes)

### 7.1. Âm `/ʃ/` trong từ vựng đàm phán
- *negotiate* `/nɪˈɡəʊʃieɪt/` (âm giữa là /ʃ/, không đọc thành /s/ hay /z/).
- *concession* `/kənˈseʃn/` (âm /ʃ/ chu môi bật hơi).
- *crucial* `/ˈkruːʃl/`, *beneficial* `/ˌbenɪˈfɪʃl/`.

### 7.2. Âm `/dʒ/` trong adjust & objection
- *adjust* `/əˈdʒʌst/`: rung thanh quản rõ ràng.
- *objection* `/əbˈdʒekʃn/`: phát âm dứt khoát phụ âm `/dʒ/`.

### 7.3. Lỗi ngữ pháp: Dùng ""will"" trong mệnh đề điều kiện ""If""
- **Sai:** *""If you will agree to this timeline, we will adjust the price.""*
- **Đúng:** *""If you agree to this timeline, we will adjust the price.""* (Mệnh đề if ở hiện tại đơn).
- **Nhầm lẫn từ vựng:** Nhầm giữa *compromise* (sự thỏa hiệp nhượng bộ) và *promise* (lời hứa).

---

## 8. 📝 Bài tập tự luyện từng bước (Từ dễ đến khó) + Đề tự kiểm tra cuối bài

### Bài tập 1: Điền dạng đúng của từ trong câu điều kiện
1. If your team (commits / will commit) ______ to a two-year contract, we can discount the fee.
2. We can adjust the delivery schedule on condition that you (accept / accepted) ______ the phased plan.
3. Principled negotiation aims for a ______ outcome. (win-win / win-lose)

### Bài tập 2: Sắp xếp các cụm từ thành câu đàm phán hoàn chỉnh
1. both / safeguards / This / parties' / solution / interests / .
2. our / could / you / Would / that / alleviate / scheduling / pressure / ?
3. a / balance / strikes / very / reasonable / That / .

---

<details>
<summary>👉 Bấm vào đây để Xem đáp án & Giải thích từng câu</summary>

### Đáp án Bài tập 1:
1. **commits** — Trong mệnh đề *If*, động từ chia ở Hiện tại đơn khi nói về thỏa thuận tương lai.
2. **accept** — Sau *on condition that*, động từ chia ở Hiện tại đơn.
3. **win-win** — Đàm phán nguyên tắc hướng tới đôi bên cùng có lợi (*win-win*).

### Đáp án Bài tập 2:
1. **This solution safeguards both parties' interests.** *(Giải pháp này bảo vệ lợi ích của cả hai bên).*
2. **Would that alleviate your scheduling pressure?** *(Liệu điều đó có giải tỏa được áp lực tiến độ cho quý vị không?)*
3. **That strikes a very reasonable balance.** *(Điều đó tạo nên một sự cân bằng rất hợp lý).*

</details>

<!-- studxy-en-v2 -->
<!-- studxy-en-v3 -->",
                @"Đàm phán nguyên tắc (principled negotiation) và tìm giải pháp win-win cùng đối tác.",
                @"Phản biện khéo léo, xử lý phản hồi từ chối bằng câu hỏi làm rõ và thuyết phục đối tác."
            ),
        };
    }

    public static async Task EnsureLessonsUpgradedOrSeededAsync(SwmsDbContext db, Guid courseId, DateTimeOffset now)
    {
        var metas = GetLessonMetas();
        foreach (var meta in metas)
        {
            var existing = await db.Lessons.FirstOrDefaultAsync(l => l.Id == meta.LessonId);
            if (existing != null)
            {
                // Safe idempotency: ONLY upgrade if CurrentVersion == 1 and marker is not yet present.
                // If user modified lesson (CurrentVersion > 1) or it is already upgraded, DO NOT overwrite!
                if (existing.CurrentVersion == 1 && !(existing.ContentMarkdown ?? string.Empty).Contains(UpgradeMarker))
                {
                    existing.ContentMarkdown = meta.Markdown;
                    existing.PlainText = meta.Markdown;
                    existing.EstimatedMinutes = meta.EstimatedMinutes;
                    existing.ContentAstJson = BuildLessonAst(existing.Title, meta.Summary1, meta.Summary2);
                    existing.UpdatedAt = now;
                }
            }
            else
            {
                var newLesson = new Lesson
                {
                    Id = meta.LessonId,
                    ChapterId = meta.ChapterId,
                    Title = meta.Title,
                    Slug = meta.Slug,
                    SortOrder = meta.SortOrder,
                    CurrentVersion = 1,
                    EstimatedMinutes = meta.EstimatedMinutes,
                    ContentMarkdown = meta.Markdown,
                    PlainText = meta.Markdown,
                    ContentAstJson = BuildLessonAst(meta.Title, meta.Summary1, meta.Summary2),
                    IsPublished = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.Lessons.Add(newLesson);
            }
        }
        await db.SaveChangesAsync();

        // Ensure 6 lesson quizzes + 1 final comprehensive quiz are seeded idempotently
        await EnsureCourseQuizzesSeededAsync(db, courseId, now);
    }

    private static Guid CreateDeterministicGuid(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }

    private record QuizChoiceDef(string Key, string Text);
    private record QuizItemDef(string Question, QuizChoiceDef[] Choices, string CorrectKey, string Explanation);
    private record CourseQuizDef(Guid QuizId, Guid LessonId, string Title, List<QuizItemDef> Items);

    public static async Task EnsureCourseQuizzesSeededAsync(SwmsDbContext db, Guid courseId, DateTimeOffset now)
    {
        var quizDefs = GetCourseQuizDefinitions();
        var existingQuizzes = await db.PracticeQuizzes
            .Where(q => q.UserId == SeedDataInitializer.DefaultUserId && q.LessonId.HasValue)
            .ToListAsync();

        foreach (var def in quizDefs)
        {
            var existing = existingQuizzes.FirstOrDefault(q => q.LessonId == def.LessonId && q.Title == def.Title);
            if (existing == null)
            {
                var quiz = new PracticeQuiz
                {
                    Id = def.QuizId,
                    UserId = SeedDataInitializer.DefaultUserId,
                    Title = def.Title,
                    LessonId = def.LessonId,
                    TotalItems = def.Items.Count,
                    CorrectItems = 0,
                    Status = "in_progress",
                    StartedAt = now
                };
                db.PracticeQuizzes.Add(quiz);

                for (int i = 0; i < def.Items.Count; i++)
                {
                    var itemDef = def.Items[i];
                    var itemId = CreateDeterministicGuid($"quiz-item:en:{def.QuizId}:{i}");
                    db.PracticeQuizItems.Add(new PracticeQuizItem
                    {
                        Id = itemId,
                        QuizId = quiz.Id,
                        ItemType = "multiple_choice",
                        PromptAstJson = JsonSerializer.Serialize(new { question = itemDef.Question }),
                        ChoicesJson = JsonSerializer.Serialize(itemDef.Choices.Select(c => new { key = c.Key, text = c.Text })),
                        ClozeSolution = itemDef.CorrectKey,
                        AiRubricFeedbackJson = JsonSerializer.Serialize(new { summary = itemDef.Explanation }),
                        CreatedAt = now.AddSeconds(i)
                    });
                }
            }
            else
            {
                var existingItemsCount = await db.PracticeQuizItems.CountAsync(i => i.QuizId == existing.Id);
                if (existingItemsCount == 0)
                {
                    for (int i = 0; i < def.Items.Count; i++)
                    {
                        var itemDef = def.Items[i];
                        var itemId = CreateDeterministicGuid($"quiz-item:en:{existing.Id}:{i}");
                        db.PracticeQuizItems.Add(new PracticeQuizItem
                        {
                            Id = itemId,
                            QuizId = existing.Id,
                            ItemType = "multiple_choice",
                            PromptAstJson = JsonSerializer.Serialize(new { question = itemDef.Question }),
                            ChoicesJson = JsonSerializer.Serialize(itemDef.Choices.Select(c => new { key = c.Key, text = c.Text })),
                            ClozeSolution = itemDef.CorrectKey,
                            AiRubricFeedbackJson = JsonSerializer.Serialize(new { summary = itemDef.Explanation }),
                            CreatedAt = now.AddSeconds(i)
                        });
                    }
                    existing.TotalItems = def.Items.Count;
                }
            }
        }
        await db.SaveChangesAsync();
    }

    private static List<CourseQuizDef> GetCourseQuizDefinitions()
    {
        var list = new List<CourseQuizDef>();
        {
            var qId = CreateDeterministicGuid("quiz:en:44444444-4444-4444-8444-444444444446:1");
            var lId = Guid.Parse("44444444-4444-4444-8444-444444444446");
            var items = new List<QuizItemDef>();
            items.Add(new QuizItemDef(
                "Âm vô thanh /θ/ trong các từ 'think', 'thank you' được phát âm chuẩn như thế nào?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Khép chặt môi và bật hơi giống chữ 'th' tiếng Việt"),
                    new QuizChoiceDef("B", "Đặt đầu lưỡi giữa hai hàm răng, thổi luồng hơi nhẹ ra và không rung dây thanh quản"),
                    new QuizChoiceDef("C", "Đặt đầu lưỡi sau răng cửa trên và rung mạnh cổ họng"),
                    new QuizChoiceDef("D", "Phát âm giống hệt chữ 's' hoặc 't' trong tiếng Việt"),
                },
                "B",
                "Âm /θ/ là phụ âm xát vô thanh răng-lưỡi. Người học đặt đầu lưỡi nhẹ giữa hai hàm răng và đẩy luồng khí ra ngoài mà không rung dây thanh quản. Người Việt hay nhầm thành âm 't' hoặc 'th' tiếng Việt."
            ));
            items.Add(new QuizItemDef(
                "Cặp từ nào sau đây có sự khác biệt rõ rệt về âm kết thúc (ending sounds) mà người Việt hay bỏ quên?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "seat (/siːt/) và see (/siː/)"),
                    new QuizChoiceDef("B", "cat và hat"),
                    new QuizChoiceDef("C", "big và bag"),
                    new QuizChoiceDef("D", "pen và ten"),
                },
                "A",
                "Từ 'seat' kết thúc bằng âm /t/, trong khi 'see' kết thúc bằng nguyên âm dài /iː/. Nếu nuốt âm cuối /t/, người nghe bản xứ sẽ hiểu nhầm 'seat' thành 'see'."
            ));
            items.Add(new QuizItemDef(
                "Từ 'regularly' có trọng âm chính rơi vào âm tiết thứ mấy?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Âm tiết thứ nhất (/ˈreɡjələli/)"),
                    new QuizChoiceDef("B", "Âm tiết thứ hai (/reˈɡjələli/)"),
                    new QuizChoiceDef("C", "Âm tiết thứ ba (/reɡjəˈləli/)"),
                    new QuizChoiceDef("D", "Âm tiết thứ tư (/reɡjələˈli/)"),
                },
                "A",
                "Từ 'regularly' có trọng âm rơi vào âm tiết đầu tiên: /ˈreɡjələli/. Lỗi phổ biến của người học là nhấn vào âm thứ hai."
            ));
            items.Add(new QuizItemDef(
                "Thành ngữ 'break the ice' trong giao tiếp mở đầu có nghĩa là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Làm vỡ tảng băng để lấy nước uống"),
                    new QuizChoiceDef("B", "Phá vỡ sự e ngại, ngượng ngùng ban đầu để bắt đầu cuộc trò chuyện tự nhiên"),
                    new QuizChoiceDef("C", "Chấm dứt một cuộc tranh cãi căng thẳng"),
                    new QuizChoiceDef("D", "Từ chối lời mời của người khác một cách dứt khoát"),
                },
                "B",
                "'Break the ice' là thành ngữ rất phổ biến chỉ hành động làm quen, mở lời thân thiện để xóa tan bầu không khí xa lạ, e dè ban đầu."
            ));
            items.Add(new QuizItemDef(
                "Để diễn đạt sự hợp tác mang lại nhiều kết quả tốt đẹp và thành công, collocations nào chuẩn nhất?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "fruitful collaboration"),
                    new QuizChoiceDef("B", "delicious collaboration"),
                    new QuizChoiceDef("C", "sweet partnership"),
                    new QuizChoiceDef("D", "heavy cooperation"),
                },
                "A",
                "'Fruitful collaboration' (hoặc 'fruitful partnership') là sự kết hợp từ chuẩn mực trong tiếng Anh chuyên nghiệp mang nghĩa sự hợp tác gặt hái nhiều kết quả tốt đẹp."
            ));
            items.Add(new QuizItemDef(
                "Chọn từ thích hợp điền vào chỗ trống: 'We quickly found _______ ground because we both specialize in distributed systems.'",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "equal"),
                    new QuizChoiceDef("B", "common"),
                    new QuizChoiceDef("C", "public"),
                    new QuizChoiceDef("D", "familiar"),
                },
                "B",
                "Cụm thành ngữ cố định 'find common ground' mang nghĩa tìm thấy điểm tương đồng, mối quan tâm chung hoặc tiếng nói chung."
            ));
            items.Add(new QuizItemDef(
                "Câu nào sau đây đúng chuẩn ngữ pháp khi giới thiệu nghề nghiệp bản thân?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "I am software engineer based in Hanoi."),
                    new QuizChoiceDef("B", "I am a software engineer based in Hanoi."),
                    new QuizChoiceDef("C", "I work software engineer based in Hanoi."),
                    new QuizChoiceDef("D", "I am an software engineer based in Hanoi."),
                },
                "B",
                "Danh từ chỉ nghề nghiệp số ít đếm được bắt buộc phải có mạo từ 'a/an'. Vì 'software' bắt đầu bằng phụ âm /s/ nên dùng 'a software engineer'."
            ));
            items.Add(new QuizItemDef(
                "Sau cấu trúc 'look forward to', động từ theo sau ở dạng nào?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Động từ nguyên mẫu không 'to' (V-inf)"),
                    new QuizChoiceDef("B", "Động từ dạng V-ing (hoặc danh từ)"),
                    new QuizChoiceDef("C", "Động từ ở dạng quá khứ V2/ed"),
                    new QuizChoiceDef("D", "Động từ có 'to' (to V)"),
                },
                "B",
                "Trong cụm 'look forward to', từ 'to' là giới từ (preposition), vì vậy động từ theo sau bắt buộc phải ở dạng V-ing (ví dụ: 'look forward to meeting you')."
            ));
            items.Add(new QuizItemDef(
                "Trong đoạn hội thoại mẫu, Alex và Mai đã đề xuất kết nối với nhau qua kênh nào?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Facebook cá nhân"),
                    new QuizChoiceDef("B", "Mạng xã hội nghề nghiệp LinkedIn qua mã QR"),
                    new QuizChoiceDef("C", "Số điện thoại bàn văn phòng"),
                    new QuizChoiceDef("D", "Địa chỉ nhà riêng"),
                },
                "B",
                "Alex hỏi: 'Would you mind if we stay in touch on LinkedIn?' và Mai đưa mã QR: 'Here is my QR code.' Đây là chuẩn mực kết nối tại các hội thảo công nghệ quốc tế."
            ));
            items.Add(new QuizItemDef(
                "Khi một người lạ hỏi: 'Excuse me, is this seat taken?' và chiếc ghế còn trống, câu trả lời tự nhiên và thân thiện nhất là:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "No, not at all, please go ahead!"),
                    new QuizChoiceDef("B", "Yes, I am sitting here alone."),
                    new QuizChoiceDef("C", "You can take it away."),
                    new QuizChoiceDef("D", "I don't know who owns this chair."),
                },
                "A",
                "'No, not at all, please go ahead!' nghĩa là 'Dạ chưa có ai ngồi, bạn cứ tự nhiên ngồi nhé!'. Đây là phản xạ lịch thiệp chuẩn mực nhất."
            ));
            list.Add(new CourseQuizDef(qId, lId, "Đề kiểm tra đánh giá Bài 1.1: Small Talk & Giới Thiệu Bản Thân", items));
        }
        {
            var qId = CreateDeterministicGuid("quiz:en:44444444-4444-4444-8444-444444444447:1");
            var lId = Guid.Parse("44444444-4444-4444-8444-444444444447");
            var items = new List<QuizItemDef>();
            items.Add(new QuizItemDef(
                "Cụm phụ âm đầu trong các từ 'straight' và 'station' cần phát âm như thế nào để tránh lỗi của người Việt?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Xì âm /s/ nhẹ trước rồi bật âm /t/, không chèn nguyên âm đệm thành 'tơ-rết' hay 'xì-tây'"),
                    new QuizChoiceDef("B", "Bỏ qua âm /s/ và chỉ phát âm âm /t/"),
                    new QuizChoiceDef("C", "Đọc âm /s/ thật to và ngắt hẳn 1 giây trước khi đọc phần còn lại"),
                    new QuizChoiceDef("D", "Biến âm /st/ thành âm 'ch'"),
                },
                "A",
                "Cụm phụ âm /st/ đòi hỏi chuyển nhanh từ âm xát /s/ sang âm tắc /t/. Người Việt hay mắc lỗi nuốt âm /s/ hoặc chèn thêm nguyên âm thừa như 'tơ-rết'."
            ));
            items.Add(new QuizItemDef(
                "Từ 'purchase' (mua hàng) có âm kết thúc được phát âm chuẩn là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "/s/"),
                    new QuizChoiceDef("B", "/tʃ/ (chu môi bật hơi dứt khoát)"),
                    new QuizChoiceDef("C", "/z/"),
                    new QuizChoiceDef("D", "/k/"),
                },
                "B",
                "Từ 'purchase' có phiên âm /ˈpɜːtʃəs/ (danh từ/động từ), âm giữa là /tʃ/ và kết thúc bằng /s/, phát âm dứt khoát."
            ));
            items.Add(new QuizItemDef(
                "Từ 'pedestrian' (người đi bộ) có trọng âm chính rơi vào âm tiết thứ mấy?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Âm tiết thứ nhất (/ˈpedəstriən/)"),
                    new QuizChoiceDef("B", "Âm tiết thứ hai (/pəˈdestriən/)"),
                    new QuizChoiceDef("C", "Âm tiết thứ ba (/pədesˈtriən/)"),
                    new QuizChoiceDef("D", "Âm tiết thứ tư (/pədestriˈən/)"),
                },
                "B",
                "'Pedestrian' có trọng âm rơi vào âm tiết thứ hai: /pəˈdestriən/."
            ));
            items.Add(new QuizItemDef(
                "Cụm từ 'within walking distance' mang ý nghĩa gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Khoảng cách rất xa bắt buộc phải đi taxi"),
                    new QuizChoiceDef("B", "Trong khoảng cách gần, có thể đi bộ tới được"),
                    new QuizChoiceDef("C", "Đoạn đường cấm người đi bộ"),
                    new QuizChoiceDef("D", "Tuyến đường chỉ dành cho xe buýt"),
                },
                "B",
                "'Within walking distance' có nghĩa là đủ gần để có thể đi bộ mà không cần phương tiện cơ giới."
            ));
            items.Add(new QuizItemDef(
                "Khi muốn hỏi người bán hàng xem món đồ này có còn hàng trong kho không, mẫu câu nào tự nhiên nhất?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Do you have this item in stock?"),
                    new QuizChoiceDef("B", "Are you hiding this item?"),
                    new QuizChoiceDef("C", "Where is your store storage?"),
                    new QuizChoiceDef("D", "Is this item exist today?"),
                },
                "A",
                "'In stock' có nghĩa là còn hàng trong kho; ngược lại 'out of stock' là hết hàng."
            ));
            items.Add(new QuizItemDef(
                "Hình thức thanh toán bằng cách chạm điện thoại thông minh hoặc thẻ contactless vào máy đọc được gọi là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "contactless payment"),
                    new QuizChoiceDef("B", "cash-only transaction"),
                    new QuizChoiceDef("C", "paper check payment"),
                    new QuizChoiceDef("D", "manual wire transfer"),
                },
                "A",
                "'Contactless payment' là thanh toán không tiếp xúc (chạm thẻ hoặc chạm Apple Pay / Google Pay vào máy POS)."
            ));
            items.Add(new QuizItemDef(
                "Chọn câu hỏi gián tiếp đúng ngữ pháp:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Could you tell me where is the nearest pharmacy?"),
                    new QuizChoiceDef("B", "Could you tell me where the nearest pharmacy is?"),
                    new QuizChoiceDef("C", "Could you tell me where does the nearest pharmacy stay?"),
                    new QuizChoiceDef("D", "Could you tell me the nearest pharmacy where is?"),
                },
                "B",
                "Trong câu hỏi gián tiếp, sau từ để hỏi 'where', mệnh đề giữ nguyên trật tự S + V ('the nearest pharmacy is'), không đảo động từ to be lên trước chủ ngữ."
            ));
            items.Add(new QuizItemDef(
                "Câu nào sau đây mắc lỗi sai trật tự từ trong câu hỏi gián tiếp?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Do you know what time the museum closes?"),
                    new QuizChoiceDef("B", "Do you know what time does the museum close?"),
                    new QuizChoiceDef("C", "Could you explain how this ticket machine works?"),
                    new QuizChoiceDef("D", "Would you mind telling me how to get to platform two?"),
                },
                "B",
                "Câu B mắc lỗi đảo trợ động từ 'does' lên trước chủ ngữ 'the museum'. Câu đúng phải là: 'Do you know what time the museum closes?'"
            ));
            items.Add(new QuizItemDef(
                "Trong đoạn hội thoại tại ga tàu, nhân viên khuyên du khách đi phương tiện gì khi mang hành lý nặng?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Đi bộ 25 phút"),
                    new QuizChoiceDef("B", "Đi xe đưa đón tự động (automated shuttle)"),
                    new QuizChoiceDef("C", "Thuê xe đạp công cộng"),
                    new QuizChoiceDef("D", "Ở lại ga qua đêm"),
                },
                "B",
                "Nhân viên ga giải thích: 'It takes about twenty-five minutes on foot, but with heavy bags, taking the automated shuttle is much more convenient.'"
            ));
            items.Add(new QuizItemDef(
                "Khi nhân viên thu ngân thông báo số tiền và bạn muốn chạm thẻ Apple Pay thanh toán, câu nói nhanh gọn nhất là:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Can I tap with Apple Pay?"),
                    new QuizChoiceDef("B", "I will give you my mobile phone forever."),
                    new QuizChoiceDef("C", "Does your computer understand Apple?"),
                    new QuizChoiceDef("D", "You touch my screen please."),
                },
                "A",
                "'Can I tap with Apple Pay?' (hoặc 'Can I pay with Apple Pay?') là cách diễn đạt phổ biến và chuẩn xác nhất."
            ));
            list.Add(new CourseQuizDef(qId, lId, "Đề kiểm tra đánh giá Bài 1.2: Hỏi Đường & Xử Lý Tình Huống Thực Tế", items));
        }
        {
            var qId = CreateDeterministicGuid("quiz:en:44444444-4444-4444-8444-444444444448:1");
            var lId = Guid.Parse("44444444-4444-4444-8444-444444444448");
            var items = new List<QuizItemDef>();
            items.Add(new QuizItemDef(
                "Động từ 'completed' có đuôi '-ed' được phát âm là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "/t/"),
                    new QuizChoiceDef("B", "/d/"),
                    new QuizChoiceDef("C", "/ɪd/"),
                    new QuizChoiceDef("D", "/ed/"),
                },
                "C",
                "Các động từ có âm tận cùng là /t/ hoặc /d/ (như complete -> /t/) khi thêm '-ed' luôn được phát âm là /ɪd/: /kəmˈpliːtɪd/."
            ));
            items.Add(new QuizItemDef(
                "Động từ 'launched' có đuôi '-ed' được phát âm là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "/t/"),
                    new QuizChoiceDef("B", "/d/"),
                    new QuizChoiceDef("C", "/ɪd/"),
                    new QuizChoiceDef("D", "/əs/"),
                },
                "A",
                "Động từ 'launch' kết thúc bằng phụ âm vô thanh /tʃ/, vì vậy khi thêm '-ed' sẽ được phát âm là /t/: /lɔːntʃt/."
            ));
            items.Add(new QuizItemDef(
                "Từ 'deliverable' (sản phẩm bàn giao) có trọng âm chính rơi vào âm tiết thứ mấy?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Âm tiết thứ nhất (/ˈdelɪvərəbl/)"),
                    new QuizChoiceDef("B", "Âm tiết thứ hai (/dɪˈlɪvərəbl/)"),
                    new QuizChoiceDef("C", "Âm tiết thứ ba (/dɪlɪˈvərəbl/)"),
                    new QuizChoiceDef("D", "Âm tiết thứ tư (/dɪlɪvəˈrəbl/)"),
                },
                "B",
                "Từ 'deliverable' có trọng âm rơi vào âm tiết thứ hai: /dɪˈlɪvərəbl/."
            ));
            items.Add(new QuizItemDef(
                "Trong quản trị dự án công nghệ, 'action item' có nghĩa là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Một bộ phim hành động chiếu trong giờ giải lao"),
                    new QuizChoiceDef("B", "Một đầu việc hành động cụ thể cần được hoàn thành và gán người phụ trách"),
                    new QuizChoiceDef("C", "Danh sách các lỗi phần mềm nghiêm trọng"),
                    new QuizChoiceDef("D", "Hóa đơn thanh toán lương cho nhân viên"),
                },
                "B",
                "'Action item' là thuật ngữ quản trị chỉ một nhiệm vụ hay đầu việc cụ thể được phân công sau cuộc họp."
            ));
            items.Add(new QuizItemDef(
                "Thuật ngữ nào sau đây chỉ 'điểm nghẽn gây chậm trễ tiến độ toàn bộ dự án'?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "milestone"),
                    new QuizChoiceDef("B", "bottleneck"),
                    new QuizChoiceDef("C", "deliverable"),
                    new QuizChoiceDef("D", "streamline"),
                },
                "B",
                "'Bottleneck' (nghĩa đen: cổ chai) dùng để chỉ điểm nghẽn năng suất khiến cả quy trình bị ách tắc."
            ));
            items.Add(new QuizItemDef(
                "Từ nào sau đây mang ý nghĩa là 'tinh gọn, tối ưu hóa quy trình làm việc'?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "streamline"),
                    new QuizChoiceDef("B", "complicate"),
                    new QuizChoiceDef("C", "postpone"),
                    new QuizChoiceDef("D", "neglect"),
                },
                "A",
                "'Streamline' là động từ chuyên ngành chỉ việc tinh giản các bước rườm rà để quy trình chạy nhanh và mượt mà hơn."
            ));
            items.Add(new QuizItemDef(
                "Khi thông báo kết quả công việc vừa hoàn thành ngay trước thời điểm viết email báo cáo, thì nào là chuẩn xác nhất?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Thì Quá khứ hoàn thành (had finalized)"),
                    new QuizChoiceDef("B", "Thì Hiện tại hoàn thành (have/has finalized)"),
                    new QuizChoiceDef("C", "Thì Tương lai đơn (will finalize)"),
                    new QuizChoiceDef("D", "Thì Quá khứ tiếp diễn (was finalizing)"),
                },
                "B",
                "Thì Hiện tại hoàn thành (Present Perfect) dùng để báo cáo công việc đã hoàn thành nhưng kết quả còn liên hệ trực tiếp với thời điểm hiện tại: 'Our team has finalized all critical deliverables.'"
            ));
            items.Add(new QuizItemDef(
                "Câu nào thể hiện lời đề nghị dời lịch họp lịch sự và chuyên nghiệp nhất trong email?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "I am busy today, so no meeting."),
                    new QuizChoiceDef("B", "Would you be available for a brief sync tomorrow at 10:30 AM?"),
                    new QuizChoiceDef("C", "You must change the meeting time right now."),
                    new QuizChoiceDef("D", "I cancel the meeting because I have other things to do."),
                },
                "B",
                "'Would you be available for a brief sync tomorrow at 10:30 AM?' là cách hỏi lịch thiệp, tôn trọng lịch trình của đối tác."
            ));
            items.Add(new QuizItemDef(
                "Lý do David Nguyễn xin dời cuộc họp với Ms. Anderson trong email mẫu là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "David muốn đi nghỉ mát"),
                    new QuizChoiceDef("B", "Có sự cố kỹ thuật khẩn cấp trên hệ thống production đòi hỏi sự có mặt trực tiếp của anh"),
                    new QuizChoiceDef("C", "David bị mất mạng internet tại nhà"),
                    new QuizChoiceDef("D", "Ms. Anderson đã hủy dự án"),
                },
                "B",
                "David viết: 'Due to an urgent production incident that requires my immediate presence, I regret to inform you that I will be unable to attend...'"
            ));
            items.Add(new QuizItemDef(
                "Khi gửi tài liệu đính kèm trong email báo cáo, mẫu câu chuẩn mực quốc tế nhất là:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Look at my file now."),
                    new QuizChoiceDef("B", "Please find attached the detailed metric breakdown for your review."),
                    new QuizChoiceDef("C", "I throw a document into this mail."),
                    new QuizChoiceDef("D", "File is here, open it."),
                },
                "B",
                "'Please find attached [tên tài liệu] for your review' là mẫu câu kinh điển trong email công sở quốc tế."
            ));
            list.Add(new CourseQuizDef(qId, lId, "Đề kiểm tra đánh giá Bài 2.1: Viết Email Công Việc & Báo Cáo Tiến Độ", items));
        }
        {
            var qId = CreateDeterministicGuid("quiz:en:44444444-4444-4444-8444-444444444449:1");
            var lId = Guid.Parse("44444444-4444-4444-8444-444444444449");
            var items = new List<QuizItemDef>();
            items.Add(new QuizItemDef(
                "Cặp phụ âm /v/ và /w/ khác nhau cơ bản ở điểm nào trong khẩu hình miệng?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Âm /v/ răng trên chạm môi dưới có rung thanh quản; âm /w/ hai môi chúm tròn mở ra"),
                    new QuizChoiceDef("B", "Hai âm này hoàn toàn giống nhau, chỉ khác cách viết"),
                    new QuizChoiceDef("C", "Âm /v/ phát âm bằng mũi; âm /w/ phát âm bằng cổ họng"),
                    new QuizChoiceDef("D", "Âm /w/ răng cắn chặt môi dưới, âm /v/ mở to miệng"),
                },
                "A",
                "Âm /v/ là âm cọ xát răng-môi (labiodental). Âm /w/ là âm lướt môi-vòm (labio-velar approximation) đòi hỏi hai môi tròn lại rồi mở ra, không để răng chạm vào môi."
            ));
            items.Add(new QuizItemDef(
                "Hiện tượng nối âm (linking) chuẩn xác trong cụm 'walk us through' là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Đọc ngắt quãng từng từ: walk ... us ... through"),
                    new QuizChoiceDef("B", "Nối phụ âm /k/ sang nguyên âm /ʌ/ thành /wɔːk ʌs/ -> 'wal-kus through'"),
                    new QuizChoiceDef("C", "Bỏ âm /k/ đọc thành 'wal us'"),
                    new QuizChoiceDef("D", "Biến âm /k/ thành âm /s/"),
                },
                "B",
                "Khi từ trước kết thúc bằng phụ âm (/k/ trong walk) và từ sau bắt đầu bằng nguyên âm (/ʌ/ trong us), người bản xứ luôn nối âm mượt mà: /wɔːk_ʌs θruː/."
            ));
            items.Add(new QuizItemDef(
                "Từ 'consensus' (sự đồng thuận) có trọng âm chính rơi vào âm tiết thứ mấy?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Âm tiết thứ nhất (/ˈkɒnsənsəs/)"),
                    new QuizChoiceDef("B", "Âm tiết thứ hai (/kənˈsensəs/)"),
                    new QuizChoiceDef("C", "Âm tiết thứ ba (/kənsenˈsəs/)"),
                    new QuizChoiceDef("D", "Nhấn đều cả ba âm"),
                },
                "B",
                "'Consensus' có trọng âm rơi vào âm tiết thứ hai: /kənˈsensəs/."
            ));
            items.Add(new QuizItemDef(
                "Cụm từ 'take offline' trong một cuộc họp trực tuyến có ý nghĩa là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Tắt kết nối internet của phòng họp ngay lập tức"),
                    new QuizChoiceDef("B", "Tách riêng vấn đề kỹ thuật chuyên sâu để những người liên quan thảo luận sau cuộc họp, tránh làm mất thời gian chung"),
                    new QuizChoiceDef("C", "Xóa vĩnh viễn dự án khỏi hệ thống"),
                    new QuizChoiceDef("D", "Gặp mặt trực tiếp tại quán cà phê ngoài đời thực"),
                },
                "B",
                "'Take something offline' là thuật ngữ hội họp kinh điển chỉ việc tạm dừng tranh luận một chi tiết kỹ thuật quá sâu để bàn riêng sau."
            ));
            items.Add(new QuizItemDef(
                "Khi muốn diễn tả đội ngũ hiện tại không còn đủ thời gian hoặc năng lực để nhận thêm đầu việc mới, từ vựng công sở nào chuẩn nhất?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "We do not have the bandwidth."),
                    new QuizChoiceDef("B", "We do not have the internet cable."),
                    new QuizChoiceDef("C", "Our hard disk is full."),
                    new QuizChoiceDef("D", "We are lazy today."),
                },
                "A",
                "'Bandwidth' trong ngôn ngữ văn phòng nghĩa bóng là dung lượng thời gian, sức lực hoặc nhân sự để đảm nhận công việc."
            ));
            items.Add(new QuizItemDef(
                "Cụm từ 'table a discussion' trong hội họp quốc tế mang nghĩa là:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Đặt một chiếc bàn vào giữa phòng họp"),
                    new QuizChoiceDef("B", "Tạm gác lại một chủ đề thảo luận để dời sang buổi họp sau khi có thêm dữ liệu"),
                    new QuizChoiceDef("C", "Bắt buộc mọi người phải ký biên bản ngay lập tức"),
                    new QuizChoiceDef("D", "Đập bàn thể hiện sự tức giận"),
                },
                "B",
                "'Table a discussion/topic' nghĩa là tạm hoãn, gác chủ đề lại để xem xét vào thời điểm phù hợp hơn."
            ));
            items.Add(new QuizItemDef(
                "Mẫu câu nào sau đây thể hiện kỹ thuật Phản biện ngoại giao (Diplomatic Disagreement) khéo léo nhất?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Your idea is wrong and inefficient."),
                    new QuizChoiceDef("B", "That is a valid concern, but have we also evaluated the long-term maintenance costs?"),
                    new QuizChoiceDef("C", "I reject your proposal completely."),
                    new QuizChoiceDef("D", "Nobody in our team likes your suggestion."),
                },
                "B",
                "Câu B dùng kỹ thuật đệm công nhận ('That is a valid concern') trước khi mở rộng góc nhìn ('have we evaluated...')."
            ));
            items.Add(new QuizItemDef(
                "Khi đồng nghiệp đang nói hăng say nhưng quên mở micro trong Zoom, câu nhắc nhở lịch sự nhất là:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "You are stupid, turn on your mic!"),
                    new QuizChoiceDef("B", "Alex, I believe you are on mute."),
                    new QuizChoiceDef("C", "Why are you silent like that?"),
                    new QuizChoiceDef("D", "Your audio is dead."),
                },
                "B",
                "'I believe you are on mute' là câu nhắc nhở tế nhị và phổ biến nhất trên toàn thế giới trong các cuộc họp trực tuyến."
            ));
            items.Add(new QuizItemDef(
                "Trong đoạn hội thoại mẫu, Mai đã đưa ra giải pháp gì để hóa giải lo ngại về network latency của David?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Hủy bỏ toàn bộ kế hoạch chuyển đổi microservices"),
                    new QuizChoiceDef("B", "Áp dụng giao tiếp gRPC tinh gọn và giải pháp truy vết phân tán (distributed tracing)"),
                    new QuizChoiceDef("C", "Mua thêm máy chủ đắt tiền nhất"),
                    new QuizChoiceDef("D", "Chuyển toàn bộ database sang lưu file text"),
                },
                "B",
                "Mai giải thích: 'To mitigate latency, we plan to implement lightweight gRPC communication and distributed tracing.'"
            ));
            items.Add(new QuizItemDef(
                "Khi vừa bắt đầu cuộc họp và chuẩn bị chia sẻ màn hình, câu kiểm tra kỹ thuật chuẩn xác nhất là:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Can everyone hear me clearly and see my shared screen?"),
                    new QuizChoiceDef("B", "Look at my computer immediately!"),
                    new QuizChoiceDef("C", "Is my voice nice today?"),
                    new QuizChoiceDef("D", "Do you see something on your phone?"),
                },
                "A",
                "'Can everyone hear me clearly and see my shared screen?' là câu kiểm tra âm thanh và màn hình tiêu chuẩn khi bắt đầu họp."
            ));
            list.Add(new CourseQuizDef(qId, lId, "Đề kiểm tra đánh giá Bài 2.2: Hội Họp Trực Tuyến & Phản Biện Ngoại Giao", items));
        }
        {
            var qId = CreateDeterministicGuid("quiz:en:44444444-4444-4444-8444-444444444450:1");
            var lId = Guid.Parse("44444444-4444-4444-8444-444444444450");
            var items = new List<QuizItemDef>();
            items.Add(new QuizItemDef(
                "Động từ 'present' (thuyết trình) và danh từ 'present' (món quà/hiện tại) khác nhau thế nào về trọng âm?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Động từ nhấn âm 2 (/prɪˈzent/); danh từ nhấn âm 1 (/ˈpreznt/)"),
                    new QuizChoiceDef("B", "Động từ nhấn âm 1; danh từ nhấn âm 2"),
                    new QuizChoiceDef("C", "Cả hai đều nhấn âm tiết thứ hai"),
                    new QuizChoiceDef("D", "Cả hai đều nhấn âm tiết thứ nhất"),
                },
                "A",
                "Trong tiếng Anh, nhiều từ 2 âm tiết có dạng danh từ nhấn âm 1 ('PRE-sent' /ˈpreznt/) và dạng động từ nhấn âm 2 ('pre-SENT' /prɪˈzent/)."
            ));
            items.Add(new QuizItemDef(
                "Từ 'demonstrate' (chứng minh, thị phạm) có trọng âm chính rơi vào âm tiết thứ mấy?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Âm tiết thứ nhất (/ˈdemənstreɪt/)"),
                    new QuizChoiceDef("B", "Âm tiết thứ hai (/dɪˈmɒnstreɪt/)"),
                    new QuizChoiceDef("C", "Âm tiết thứ ba (/demənˈstreɪt/)"),
                    new QuizChoiceDef("D", "Nhấn đều cả 3 âm tiết"),
                },
                "A",
                "'Demonstrate' có trọng âm rơi vào âm tiết đầu tiên: /ˈdemənstreɪt/."
            ));
            items.Add(new QuizItemDef(
                "Kỹ thuật 'Dramatic Pause' trong thuyết trình mang lại tác dụng chính là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Để người nói tranh thủ uống nước hoặc kiểm tra điện thoại"),
                    new QuizChoiceDef("B", "Tạo khoảng ngắt nghỉ 1-2 giây giúp người nghe lắng đọng và tập trung vào thông điệp then chốt"),
                    new QuizChoiceDef("C", "Chờ ban giám khảo nhắc bài"),
                    new QuizChoiceDef("D", "Làm cho bài thuyết trình dài hơn để đủ giờ"),
                },
                "B",
                "Khoảng dừng kịch tính (Dramatic Pause) ngay trước hoặc sau một thông điệp quan trọng giúp thu hút toàn bộ sự chú ý của khán phòng."
            ));
            items.Add(new QuizItemDef(
                "Thuật ngữ 'Signposting language' trong thuyết trình tiếng Anh chỉ điều gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Các biển báo giao thông trên đường phố"),
                    new QuizChoiceDef("B", "Ngôn ngữ chỉ đường, dẫn dắt người nghe biết bài nói đang ở phần nào (Mở bài, Thân bài, Kết bài)"),
                    new QuizChoiceDef("C", "Chữ ký điện tử ở cuối bài trình chiếu"),
                    new QuizChoiceDef("D", "Font chữ in đậm trên slide"),
                },
                "B",
                "Signposting language là các cụm từ định hướng ('To begin with', 'Moving on to', 'In conclusion') giúp khán giả theo dõi mạch bài nói dễ dàng."
            ));
            items.Add(new QuizItemDef(
                "Để diễn tả một chỉ số tăng trưởng vọt đột biến theo cấp số nhân, từ vựng nào mang tính ấn tượng mạnh mẽ nhất?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "exponential surge"),
                    new QuizChoiceDef("B", "tiny increase"),
                    new QuizChoiceDef("C", "slow movement"),
                    new QuizChoiceDef("D", "small fluctuation"),
                },
                "A",
                "'Exponential surge' nghĩa là sự tăng vọt theo cấp số nhân, thể hiện bước nhảy vọt ấn tượng trong biểu đồ số liệu."
            ));
            items.Add(new QuizItemDef(
                "Từ vựng nào mô tả biểu đồ số liệu 'chững lại ở mức ổn định sau một chu kỳ tăng trưởng'?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "plummeted"),
                    new QuizChoiceDef("B", "plateaued"),
                    new QuizChoiceDef("C", "crashed"),
                    new QuizChoiceDef("D", "exploded"),
                },
                "B",
                "'Plateau' (danh từ: cao nguyên, động từ: chững lại ở đỉnh ổn định) là từ chuẩn để miêu tả biểu đồ đi ngang sau khi tăng."
            ));
            items.Add(new QuizItemDef(
                "Chọn câu đúng ngữ pháp khi mô tả chỉ số năng suất tăng lên:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Our engineering productivity rose significantly last quarter."),
                    new QuizChoiceDef("B", "Our engineering productivity raised significantly last quarter."),
                    new QuizChoiceDef("C", "Our engineering productivity is rise significantly."),
                    new QuizChoiceDef("D", "Our engineering productivity risen significantly."),
                },
                "A",
                "'Rise' là nội động từ (không cần tân ngữ): quá khứ là 'rose'. Còn 'raise' là ngoại động từ bắt buộc có tân ngữ phía sau ('We raised our fees')."
            ));
            items.Add(new QuizItemDef(
                "Câu chẻ (Cleft sentence) nào dưới đây dùng đúng ngữ pháp để tạo điểm nhấn chốt bài thuyết trình?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "What truly sets this strategy apart is our uncompromising focus on user empathy."),
                    new QuizChoiceDef("B", "That truly set this strategy apart are our focus."),
                    new QuizChoiceDef("C", "Which sets this strategy apart is focus."),
                    new QuizChoiceDef("D", "It truly setting this strategy apart is focus."),
                },
                "A",
                "Cấu trúc câu chẻ với 'What... is...' dùng để nhấn mạnh điều cốt lõi nhất: 'What truly sets this strategy apart is...'"
            ));
            items.Add(new QuizItemDef(
                "Trong bài thuyết trình 5 phút mẫu, hơn 70% điểm nghẽn năng suất kỹ thuật xuất phát từ đâu?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Từ lỗi phần mềm nghiêm trọng (software bugs)"),
                    new QuizChoiceDef("B", "Từ tình trạng cô lập, ứ đọng kiến thức cục bộ (knowledge silos)"),
                    new QuizChoiceDef("C", "Từ phần cứng máy tính quá cũ"),
                    new QuizChoiceDef("D", "Từ việc nhân viên đi làm muộn"),
                },
                "B",
                "Bài nói khẳng định: 'over seventy percent of engineering productivity bottlenecks stem not from software bugs, but from knowledge silos'."
            ));
            items.Add(new QuizItemDef(
                "Mẫu câu kết bài và mở lời cho phần hỏi đáp (Q&A) chuẩn mực và tự tin nhất là:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "I finish my speech, no more talking."),
                    new QuizChoiceDef("B", "Thank you for your time, and I would now be delighted to take any questions."),
                    new QuizChoiceDef("C", "Please don't ask any difficult questions."),
                    new QuizChoiceDef("D", "That is the end, goodbye everyone."),
                },
                "B",
                "'Thank you for your time, and I would now be delighted to take any questions' là lời chào kết thanh lịch và chuyên nghiệp."
            ));
            list.Add(new CourseQuizDef(qId, lId, "Đề kiểm tra đánh giá Bài 3.1: Cấu Trúc Thuyết Trình Tinh Gọn 5 Phút", items));
        }
        {
            var qId = CreateDeterministicGuid("quiz:en:44444444-4444-4444-8444-444444444451:1");
            var lId = Guid.Parse("44444444-4444-4444-8444-444444444451");
            var items = new List<QuizItemDef>();
            items.Add(new QuizItemDef(
                "Âm giữa trong từ 'negotiation' (/nɪˌɡəʊʃiˈeɪʃn/) được phát âm chuẩn là âm gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "/s/"),
                    new QuizChoiceDef("B", "/ʃ/ (chu tròn môi bật luồng hơi ma sát)"),
                    new QuizChoiceDef("C", "/z/"),
                    new QuizChoiceDef("D", "/tʃ/"),
                },
                "B",
                "Cả hai vị trí 'ti' trong 'negotiation' đều phát âm thành âm vô thanh /ʃ/: /nɪˌɡəʊʃiˈeɪʃn/."
            ));
            items.Add(new QuizItemDef(
                "Động từ 'adjust' (/əˈdʒʌst/) có phụ âm thứ hai là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "/dʒ/ (chu môi và rung mạnh dây thanh quản)"),
                    new QuizChoiceDef("B", "/j/"),
                    new QuizChoiceDef("C", "/z/"),
                    new QuizChoiceDef("D", "/t/"),
                },
                "A",
                "'Adjust' có âm /dʒ/: /əˈdʒʌst/. Người học cần chú ý phát âm rõ âm hữu thanh này."
            ));
            items.Add(new QuizItemDef(
                "Từ 'beneficial' (có lợi, sinh lợi) có trọng âm chính rơi vào âm tiết thứ mấy?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Âm tiết thứ nhất (/ˈbenɪfɪʃl/)"),
                    new QuizChoiceDef("B", "Âm tiết thứ hai (/bəˈnefɪʃl/)"),
                    new QuizChoiceDef("C", "Âm tiết thứ ba (/ˌbenɪˈfɪʃl/)"),
                    new QuizChoiceDef("D", "Âm tiết thứ tư (/benɪfɪˈʃl/)"),
                },
                "C",
                "'Beneficial' có trọng âm rơi vào âm tiết thứ ba: /ˌbenɪˈfɪʃl/."
            ));
            items.Add(new QuizItemDef(
                "Thế nào là 'Principled Negotiation' (Đàm phán theo nguyên tắc)?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Cố gắng ép giá đối phương xuống mức thấp nhất bất chấp hậu quả"),
                    new QuizChoiceDef("B", "Tìm kiếm giải pháp đôi bên cùng có lợi (Win-Win) dựa trên lợi ích cốt lõi thay vì tranh chấp vị thế cứng nhắc"),
                    new QuizChoiceDef("C", "Nhượng bộ vô điều kiện để giữ mối quan hệ"),
                    new QuizChoiceDef("D", "Kéo dài cuộc đàm phán nhiều tháng để đối phương mệt mỏi"),
                },
                "B",
                "Đàm phán theo nguyên tắc (trường phái Harvard) tập trung vào tách rời con người khỏi vấn đề, tập trung vào lợi ích (interests) thay vì lập trường bề mặt (positions)."
            ));
            items.Add(new QuizItemDef(
                "Thành ngữ 'break the impasse' trong đàm phán hợp đồng có nghĩa là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Làm hỏng tài liệu hợp đồng"),
                    new QuizChoiceDef("B", "Phá vỡ thế bế tắc để tìm ra hướng đi chung"),
                    new QuizChoiceDef("C", "Từ chối tiếp tục đàm phán"),
                    new QuizChoiceDef("D", "Khởi kiện đối tác ra tòa"),
                },
                "B",
                "'Impasse' nghĩa là ngõ cụt, thế bế tắc. 'Break the impasse' là tìm ra giải pháp tháo gỡ bế tắc."
            ));
            items.Add(new QuizItemDef(
                "Thuật ngữ 'non-negotiable clause' mang ý nghĩa là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Điều khoản có thể dễ dàng thay đổi bất cứ lúc nào"),
                    new QuizChoiceDef("B", "Điều khoản cốt lõi mang tính bất khả thương lượng vì lý do an ninh, pháp lý hoặc quy chuẩn bắt buộc"),
                    new QuizChoiceDef("C", "Điều khoản không cần ghi vào hợp đồng"),
                    new QuizChoiceDef("D", "Điều khoản dành cho khách hàng thân thiết"),
                },
                "B",
                "'Non-negotiable' là điều khoản sống còn, không thể thương lượng hay thỏa hiệp."
            ));
            items.Add(new QuizItemDef(
                "Chọn câu nhượng bộ có điều kiện (Conditional Concession) đúng chuẩn ngữ pháp:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "If you commit to a two-year enterprise agreement, we will adjust our implementation fee by ten percent."),
                    new QuizChoiceDef("B", "If you will commit to a two-year agreement, we will adjust the fee."),
                    new QuizChoiceDef("C", "If you commit to agreement, we adjust fee yesterday."),
                    new QuizChoiceDef("D", "We adjust the fee if you will agree."),
                },
                "A",
                "Trong câu điều kiện loại 1, mệnh đề 'If' dùng thì Hiện tại đơn ('If you commit...'), không dùng 'will commit'."
            ));
            items.Add(new QuizItemDef(
                "Khi đối tác nói 'Your price is too high', câu hỏi làm rõ (Clarifying Question) chuyên nghiệp nhất là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "You are wrong, our price is very cheap!"),
                    new QuizChoiceDef("B", "Could you elaborate on the primary constraints preventing your team from adopting this proposal?"),
                    new QuizChoiceDef("C", "Why do you have so little money?"),
                    new QuizChoiceDef("D", "If you don't buy, another client will."),
                },
                "B",
                "'Could you elaborate on the primary constraints...' giúp bóc tách rào cản thực tế của khách hàng một cách tôn trọng và chuyên nghiệp."
            ));
            items.Add(new QuizItemDef(
                "Trong đoạn hội thoại mẫu, Minh đã đưa ra giải pháp thỏa hiệp nào để giải quyết áp lực ngân sách của Sarah?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Làm việc miễn phí hoàn toàn trong năm đầu tiên"),
                    new QuizChoiceDef("B", "Giảm 10% phí triển khai với điều kiện phía khách hàng cam kết ký hợp đồng hỗ trợ doanh nghiệp 2 năm"),
                    new QuizChoiceDef("C", "Tăng giá thêm 20%"),
                    new QuizChoiceDef("D", "Bỏ bớt các tính năng bảo mật then chốt"),
                },
                "B",
                "Minh đề xuất nhượng bộ có điều kiện: 'adjust our overall implementation fee by ten percent on condition that your team commits to a two-year enterprise support agreement.'"
            ));
            items.Add(new QuizItemDef(
                "Mẫu câu chốt thỏa thuận tôn trọng quyền lợi của cả hai bên khi đàm phán thành công là:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "We won and you lost the negotiation."),
                    new QuizChoiceDef("B", "This solution represents a mutually beneficial agreement that safeguards both parties' interests."),
                    new QuizChoiceDef("C", "You must pay immediately before we change our mind."),
                    new QuizChoiceDef("D", "This deal is not very good for us."),
                },
                "B",
                "'This solution represents a mutually beneficial agreement that safeguards both parties' interests' thể hiện tinh thần Win-Win tôn trọng bền vững."
            ));
            list.Add(new CourseQuizDef(qId, lId, "Đề kiểm tra đánh giá Bài 3.2: Đàm Phán Nguyên Tắc & Thuyết Phục Đối Tác", items));
        }
        {
            var qId = CreateDeterministicGuid("quiz:en:44444444-4444-4444-8444-444444444451:final");
            var lId = Guid.Parse("44444444-4444-4444-8444-444444444451");
            var items = new List<QuizItemDef>();
            items.Add(new QuizItemDef(
                "Cặp âm nào sau đây phân biệt giữa âm răng-lưỡi vô thanh (unvoiced) và hữu thanh (voiced)?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "/θ/ trong 'think' (vô thanh) và /ð/ trong 'this' (hữu thanh)"),
                    new QuizChoiceDef("B", "/p/ trong 'pen' và /t/ trong 'ten'"),
                    new QuizChoiceDef("C", "/s/ trong 'see' và /k/ trong 'cat'"),
                    new QuizChoiceDef("D", "/m/ trong 'mother' và /n/ trong 'nice'"),
                },
                "A",
                "Cặp /θ/ và /ð/ có cùng vị trí đặt lưỡi giữa răng, nhưng /θ/ chỉ đẩy luồng khí (vô thanh) còn /ð/ có rung thanh quản (hữu thanh)."
            ));
            items.Add(new QuizItemDef(
                "Động từ nào sau đây có cách phát âm đuôi '-ed' khác với các động từ còn lại?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "decided (/dɪˈsaɪdɪd/ -> /ɪd/)"),
                    new QuizChoiceDef("B", "launched (/lɔːntʃt/ -> /t/)"),
                    new QuizChoiceDef("C", "fixed (/fɪkst/ -> /t/)"),
                    new QuizChoiceDef("D", "worked (/wɜːkt/ -> /t/)"),
                },
                "A",
                "'Decided' kết thúc bằng âm /d/ nên đuôi '-ed' đọc là /ɪd/. Ba từ còn lại kết thúc bằng phụ âm vô thanh (/tʃ, s, k/) nên đọc là /t/."
            ));
            items.Add(new QuizItemDef(
                "Từ nào sau đây có trọng âm chính rơi vào âm tiết thứ hai?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "consensus (/kənˈsensəs/)"),
                    new QuizChoiceDef("B", "demonstrate (/ˈdemənstreɪt/)"),
                    new QuizChoiceDef("C", "roundabout (/ˈraʊndəbaʊt/)"),
                    new QuizChoiceDef("D", "milestone (/ˈmaɪlstəʊn/)"),
                },
                "A",
                "'Consensus' nhấn âm tiết thứ hai: /kənˈsensəs/. Các từ còn lại đều có trọng âm rơi vào âm tiết thứ nhất."
            ));
            items.Add(new QuizItemDef(
                "Thành ngữ 'common ground' trong giao tiếp kinh doanh và ngoại giao quốc tế có nghĩa là gì?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Khu đất công cộng thuộc sở hữu nhà nước"),
                    new QuizChoiceDef("B", "Điểm tương đồng, nền tảng chung hoặc mối quan tâm chung giữa các bên"),
                    new QuizChoiceDef("C", "Mặt đất bằng phẳng để xây dựng tòa nhà"),
                    new QuizChoiceDef("D", "Ý kiến bất đồng không thể hòa giải"),
                },
                "B",
                "'Common ground' là tiếng nói chung, cơ sở đồng thuận để hai bên cùng hợp tác."
            ));
            items.Add(new QuizItemDef(
                "Khi muốn tổ chức một cuộc họp nội bộ nhanh chóng trong 15 phút để cập nhật thông tin, cụm từ công sở nào tự nhiên nhất?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "a brief fifteen-minute sync"),
                    new QuizChoiceDef("B", "a giant meeting conference"),
                    new QuizChoiceDef("C", "a long endless debate"),
                    new QuizChoiceDef("D", "a slow paper exchange"),
                },
                "A",
                "'A brief sync' (hoặc 'a quick check-in') là cách nói phổ biến nhất trong các công ty công nghệ quốc tế."
            ));
            items.Add(new QuizItemDef(
                "Từ vựng nào mô tả sự đồng thuận nhất trí 100% trong hội đồng quản trị hoặc ban lãnh đạo?",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "unanimous consensus"),
                    new QuizChoiceDef("B", "partial conflict"),
                    new QuizChoiceDef("C", "temporary division"),
                    new QuizChoiceDef("D", "hesitant vote"),
                },
                "A",
                "'Unanimous consensus' (hoặc 'unanimous agreement') nghĩa là sự nhất trí hoàn toàn, không có phiếu chống."
            ));
            items.Add(new QuizItemDef(
                "Phát hiện lỗi sai trong câu: 'I look forward to meet you at the tech summit tomorrow.'",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Sai ở 'look forward to meet' -> cần sửa thành 'look forward to meeting'"),
                    new QuizChoiceDef("B", "Sai ở 'at the' -> cần sửa thành 'on the'"),
                    new QuizChoiceDef("C", "Sai ở 'tomorrow' -> cần sửa thành 'yesterday'"),
                    new QuizChoiceDef("D", "Câu trên hoàn toàn không có lỗi"),
                },
                "A",
                "Sau cụm 'look forward to', động từ luôn ở dạng danh động từ V-ing: 'look forward to meeting you'."
            ));
            items.Add(new QuizItemDef(
                "Phát hiện lỗi sai trật tự từ trong câu hỏi gián tiếp: 'Could you tell me how can I get to platform two?'",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Đảo trợ động từ 'can' lên trước chủ ngữ 'I' -> cần sửa thành 'how I can get'"),
                    new QuizChoiceDef("B", "Dùng 'tell' sai -> cần đổi thành 'say'"),
                    new QuizChoiceDef("C", "Thiếu mạo từ trước 'platform two'"),
                    new QuizChoiceDef("D", "Dùng 'Could you' quá trang trọng không cần thiết"),
                },
                "A",
                "Trong câu hỏi gián tiếp, không đảo trợ động từ lên trước chủ ngữ. Mệnh đề sau 'how' phải là 'how I can get to platform two'."
            ));
            items.Add(new QuizItemDef(
                "Chọn câu nhượng bộ có điều kiện chuẩn xác trong đàm phán hợp đồng:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Provided that you accept the security schedule, we are prepared to accommodate your budget request."),
                    new QuizChoiceDef("B", "Provided that you will accept the schedule, we accommodated your request."),
                    new QuizChoiceDef("C", "If you accommodated, we accommodate."),
                    new QuizChoiceDef("D", "We accept your budget unconditionally without any terms."),
                },
                "A",
                "'Provided that [Hiện tại đơn], we are prepared to [V]' là cấu trúc nhượng bộ ngoại giao chuẩn mực cao cấp."
            ));
            items.Add(new QuizItemDef(
                "Khi chuyển sang trình bày trang chiếu số 4 trong buổi họp trực tuyến đa quốc gia, câu dẫn dắt chuyên nghiệp nhất là:",
                new QuizChoiceDef[]
                {
                    new QuizChoiceDef("A", "Moving on to slide four, let us examine our baseline performance metrics."),
                    new QuizChoiceDef("B", "I click next slide now, you must watch."),
                    new QuizChoiceDef("C", "Slide four is here, I stop talking."),
                    new QuizChoiceDef("D", "Do you see this picture or not?"),
                },
                "A",
                "'Moving on to slide four, let us examine our baseline performance metrics' là ngôn ngữ chuyển ý (signposting) mẫu mực và chuyên nghiệp."
            ));
            list.Add(new CourseQuizDef(qId, lId, "Đề Tổng kết Khóa học: Tiếng Anh Giao Tiếp Toàn Diện (A2 → B1)", items));
        }
        return list;
    }

    public static List<PracticeActivity> GetExtendedPracticeActivities(Guid courseId, DateTimeOffset now)
    {
        var activities = new List<PracticeActivity>();

        // Bài 1.1 Shadowing (OrderIndex 11)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444446"), "shadowing", 11),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444446"),
            OrderIndex = 11,
            Type = "shadowing",
            Title = "Shadowing mở rộng (Bài 1.1): 15 câu đa cấp độ Dễ → Vừa → Khó",
            Description = "Luyện nói đuổi theo ngữ điệu người bản xứ, tập trung vào nối âm, ngữ điệu và phát âm chuẩn các âm then chốt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l11-shad-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Chào hỏi làm quen (Dễ)"",
      ""sentence"": ""Hello, my name is Mai and I am glad to meet you."",
      ""ipa"": ""/həˈləʊ, maɪ neɪm ɪz maɪ ænd aɪ æm ɡlæd tuː miːt juː/"",
      ""meaningVi"": ""Xin chào, tôi tên là Mai và tôi rất vui được gặp bạn."",
      ""keyPhrases"": [
        ""my name is"",
        ""glad to meet you"",
        ""hello""
      ]
    },
    {
      ""id"": ""en-l11-shad-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi thăm chỗ ngồi (Dễ)"",
      ""sentence"": ""Excuse me, is anyone sitting here right now?"",
      ""ipa"": ""/ɪkˈskjuːs miː, ɪz ˈeniwʌn ˈsɪtɪŋ hɪə raɪt naʊ/"",
      ""meaningVi"": ""Xin lỗi, hiện tại chỗ này đã có ai ngồi chưa?"",
      ""keyPhrases"": [
        ""excuse me"",
        ""sitting here"",
        ""right now""
      ]
    },
    {
      ""id"": ""en-l11-shad-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Thời tiết bắt chuyện (Dễ)"",
      ""sentence"": ""It is such a wonderful day for an outdoor event."",
      ""ipa"": ""/ɪt ɪz sʌtʃ ə ˈwʌndəfl deɪ fɔːr ən ˌaʊtˈdɔːr ɪˈvent/"",
      ""meaningVi"": ""Hôm nay quả là một ngày tuyệt vời cho một sự kiện ngoài trời."",
      ""keyPhrases"": [
        ""wonderful day"",
        ""outdoor event"",
        ""such a""
      ]
    },
    {
      ""id"": ""en-l11-shad-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Tự giới thiệu nghề nghiệp (Dễ)"",
      ""sentence"": ""I work as a web developer at a local startup."",
      ""ipa"": ""/aɪ wɜːk æz ə web dɪˈveləpə æt ə ˈləʊkl ˈstɑːtʌp/"",
      ""meaningVi"": ""Tôi làm việc với vai trò lập trình viên web tại một công ty khởi nghiệp địa phương."",
      ""keyPhrases"": [
        ""work as a"",
        ""web developer"",
        ""local startup""
      ]
    },
    {
      ""id"": ""en-l11-shad-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Lời chúc tốt đẹp (Dễ)"",
      ""sentence"": ""I hope you enjoy the keynote presentation this morning."",
      ""ipa"": ""/aɪ həʊp juː ɪnˈdʒɔɪ ðə ˈkiːnəʊt ˌpreznˈteɪʃn ðɪs ˈmɔːnɪŋ/"",
      ""meaningVi"": ""Tôi chúc bạn thưởng thức trọn vẹn bài thuyết trình mở màn sáng nay."",
      ""keyPhrases"": [
        ""hope you enjoy"",
        ""keynote presentation"",
        ""this morning""
      ]
    },
    {
      ""id"": ""en-l11-shad-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Phá băng sự kiện (Vừa)"",
      ""sentence"": ""Striking up a conversation with international peers broadens our professional horizons."",
      ""ipa"": ""/ˈstraɪkɪŋ ʌp ə ˌkɒnvəˈseɪʃn wɪð ˌɪntəˈnæʃnəl pɪəz ˈbrɔːdnz ˈaʊə prəˈfeʃənl həˈraɪznz/"",
      ""meaningVi"": ""Chủ động bắt chuyện với đồng nghiệp quốc tế mở rộng tầm nhìn chuyên môn của chúng ta."",
      ""keyPhrases"": [
        ""striking up a conversation"",
        ""international peers"",
        ""professional horizons""
      ]
    },
    {
      ""id"": ""en-l11-shad-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Giới thiệu chuyên sâu (Vừa)"",
      ""sentence"": ""I specialize in backend architecture and cloud-native infrastructure solutions."",
      ""ipa"": ""/aɪ ˈspeʃəlaɪz ɪn ˈbækend ˈɑːkɪtektʃə ænd klaʊd ˈneɪtɪv ˌɪnfrəˈstrʌktʃə səˈluːʃnz/"",
      ""meaningVi"": ""Tôi chuyên sâu về kiến trúc máy chủ và các giải pháp hạ tầng điện toán đám mây."",
      ""keyPhrases"": [
        ""specialize in"",
        ""backend architecture"",
        ""cloud-native infrastructure""
      ]
    },
    {
      ""id"": ""en-l11-shad-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Tìm điểm chung (Vừa)"",
      ""sentence"": ""Finding common ground allows colleagues to establish genuine mutual trust quickly."",
      ""ipa"": ""/ˈfaɪndɪŋ ˈkɒmən ɡraʊnd əˈlaʊz ˈkɒliːɡz tuː ɪˈstæblɪʃ ˈdʒenjuɪn ˈmjuːtʃuəl trʌst ˈkwɪkli/"",
      ""meaningVi"": ""Tìm thấy tiếng nói chung giúp các đồng nghiệp nhanh chóng xây dựng niềm tin chân thành."",
      ""keyPhrases"": [
        ""finding common ground"",
        ""establish trust"",
        ""mutual trust""
      ]
    },
    {
      ""id"": ""en-l11-shad-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đề nghị giữ liên lạc (Vừa)"",
      ""sentence"": ""Would you mind if we connected on professional social networks to exchange ideas?"",
      ""ipa"": ""/wʊd juː maɪnd ɪf wiː kəˈnektɪd ɒn prəˈfeʃənl ˈsəʊʃl ˈnetwɜːks tuː ɪksˈtʃeɪndʒ aɪˈdɪəz/"",
      ""meaningVi"": ""Bạn có phiền nếu chúng ta kết nối trên mạng xã hội công việc để trao đổi ý tưởng không?"",
      ""keyPhrases"": [
        ""would you mind if"",
        ""professional networks"",
        ""exchange ideas""
      ]
    },
    {
      ""id"": ""en-l11-shad-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Lắng nghe tích cực (Vừa)"",
      ""sentence"": ""Active listening and authentic curiosity are the cornerstones of charismatic communication."",
      ""ipa"": ""/ˈæktɪv ˈlɪsnɪŋ ænd ɔːˈθentɪk ˌkjʊəriˈɒsəti ɑː ðə ˈkɔːnəstəʊnz əv ˌkærɪzˈmætɪk kəˌmjuːnɪˈkeɪʃn/"",
      ""meaningVi"": ""Lắng nghe chủ động và sự tò mò chân thành là nền tảng của phong thái giao tiếp cuốn hút."",
      ""keyPhrases"": [
        ""active listening"",
        ""authentic curiosity"",
        ""charismatic communication""
      ]
    },
    {
      ""id"": ""en-l11-shad-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Giao lưu đa văn hóa (Khó)"",
      ""sentence"": ""Navigating cross-cultural communication demands both linguistic versatility and profound empathy."",
      ""ipa"": ""/ˈnævɪɡeɪtɪŋ krɒs ˈkʌltʃərəl kəˌmjuːnɪˈkeɪʃn dɪˈmɑːndz bəʊθ lɪŋˈɡwɪstɪk ˌvɜːsəˈtɪləti ænd prəˈfaʊnd ˈempəθi/"",
      ""meaningVi"": ""Giao tiếp liên văn hóa đòi hỏi cả sự linh hoạt về ngôn ngữ lẫn sự thấu cảm sâu sắc."",
      ""keyPhrases"": [
        ""cross-cultural communication"",
        ""linguistic versatility"",
        ""profound empathy""
      ]
    },
    {
      ""id"": ""en-l11-shad-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Xây dựng mối quan hệ bền vững (Khó)"",
      ""sentence"": ""Establishing meaningful professional rapport often begins with thoughtful questions rather than self-promotion."",
      ""ipa"": ""/ɪˈstæblɪʃɪŋ ˈmiːnɪŋfl prəˈfeʃənl ræˈpɔːr ˈɒfn bɪˈɡɪnz wɪð ˈθɔːtfl ˈkwestʃənz ˈrɑːðə ðæn self prəˈməʊʃn/"",
      ""meaningVi"": ""Tạo dựng mối quan hệ nghề nghiệp ý nghĩa thường bắt đầu bằng những câu hỏi chu đáo thay vì tự quảng bá."",
      ""keyPhrases"": [
        ""professional rapport"",
        ""thoughtful questions"",
        ""self-promotion""
      ]
    },
    {
      ""id"": ""en-l11-shad-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Thích ứng môi trường mới (Khó)"",
      ""sentence"": ""Professionals who effortlessly initiate cordial interactions consistently unlock unprecedented strategic opportunities."",
      ""ipa"": ""/prəˈfeʃənlz huː ˈefətləsli ɪˈnɪʃieɪt ˈkɔːdiəl ˌɪntərˈækʃnz kənˈsɪstəntli ʌnˈlɒk ʌnˈpresɪdentɪd strəˈtiːdʒɪk ˌɒpəˈtjuːnətiz/"",
      ""meaningVi"": ""Những chuyên gia tự nhiên khởi xướng tương tác nồng hậu luôn mở ra các cơ hội chiến lược chưa từng có."",
      ""keyPhrases"": [
        ""cordial interactions"",
        ""unprecedented opportunities"",
        ""strategic opportunities""
      ]
    },
    {
      ""id"": ""en-l11-shad-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Hợp tác chiến lược (Khó)"",
      ""sentence"": ""We genuinely anticipate that this initial encounter will catalyze substantial multi-year partnerships."",
      ""ipa"": ""/wiː ˈdʒenjuɪnli ænˈtɪsɪpeɪt ðæt ðɪs ɪˈnɪʃl ɪnˈkaʊntə wɪl ˈkætəlaɪz səbˈstænʃl ˈmʌlti jɪə ˈpɑːtnəʃɪps/"",
      ""meaningVi"": ""Chúng tôi chân thành kỳ vọng rằng cuộc gặp gỡ ban đầu này sẽ thúc đẩy các quan hệ đối tác nhiều năm vững bền."",
      ""keyPhrases"": [
        ""genuinely anticipate"",
        ""catalyze partnerships"",
        ""initial encounter""
      ]
    },
    {
      ""id"": ""en-l11-shad-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Kể chuyện bản thân (Khó)"",
      ""sentence"": ""Articulating your career journey with genuine vulnerability and clarity instantly resonates with seasoned leaders."",
      ""ipa"": ""/ɑːˈtɪkjuleɪtɪŋ jɔː kəˈrɪə ˈdʒɜːni wɪð ˈdʒenjuɪn ˌvʌlnərəˈbɪləti ænd ˈklærəti ˈɪnstəntli ˈrezəneɪts wɪð ˈsiːznd ˈliːdəz/"",
      ""meaningVi"": ""Diễn đạt hành trình sự nghiệp bằng sự chân thật và rõ ràng tạo được sự đồng cảm tức thì với các nhà lãnh đạo dày dạn."",
      ""keyPhrases"": [
        ""articulating career journey"",
        ""genuine vulnerability"",
        ""seasoned leaders""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.1 Dictation (OrderIndex 12)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444446"), "dictation", 12),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444446"),
            OrderIndex = 12,
            Type = "dictation",
            Title = "Dictation mở rộng (Bài 1.1): 15 câu nghe chép chính tả phản xạ",
            Description = "Nghe câu hội thoại đa cấp độ và gõ lại chính xác từng từ hoặc chọn word chip để rèn luyện thính giác tiếng Anh.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l11-dict-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Làm quen đơn giản (Dễ)"",
      ""sentence"": ""Nice to meet you, and thank you for your warm welcome."",
      ""ipa"": ""/naɪs tuː miːt juː, ænd θæŋk juː fɔː jɔː wɔːm ˈwelkəm/"",
      ""meaningVi"": ""Rất vui được gặp bạn, và cảm ơn bạn vì sự chào đón ấm áp."",
      ""wordChips"": [
        ""Nice"",
        ""to"",
        ""meet"",
        ""you"",
        ""and"",
        ""thank"",
        ""you"",
        ""for"",
        ""your"",
        ""warm"",
        ""welcome""
      ],
      ""distractors"": [
        ""great"",
        ""their"",
        ""often"",
        ""good""
      ]
    },
    {
      ""id"": ""en-l11-dict-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi thăm công việc (Dễ)"",
      ""sentence"": ""What company do you work for at the moment?"",
      ""ipa"": ""/wɒt ˈkʌmpəni duː juː wɜːk fɔːr æt ðə ˈməʊmənt/"",
      ""meaningVi"": ""Hiện tại bạn đang làm việc cho công ty nào thế?"",
      ""wordChips"": [
        ""What"",
        ""company"",
        ""do"",
        ""you"",
        ""work"",
        ""for"",
        ""at"",
        ""the"",
        ""moment""
      ],
      ""distractors"": [
        ""where"",
        ""which"",
        ""working"",
        ""now""
      ]
    },
    {
      ""id"": ""en-l11-dict-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Giới thiệu thành phố (Dễ)"",
      ""sentence"": ""I live in Hanoi and I really love the city life."",
      ""ipa"": ""/aɪ lɪv ɪn hæˈnɔɪ ænd aɪ ˈrɪəli lʌv ðə ˈsɪti laɪf/"",
      ""meaningVi"": ""Tôi sống ở Hà Nội và tôi rất yêu cuộc sống nơi đô thị."",
      ""wordChips"": [
        ""I"",
        ""live"",
        ""in"",
        ""Hanoi"",
        ""and"",
        ""I"",
        ""really"",
        ""love"",
        ""the"",
        ""city"",
        ""life""
      ],
      ""distractors"": [
        ""stay"",
        ""living"",
        ""town"",
        ""urban""
      ]
    },
    {
      ""id"": ""en-l11-dict-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi về sự kiện (Dễ)"",
      ""sentence"": ""Are you enjoying the conference sessions today?"",
      ""ipa"": ""/ɑː juː ɪnˈdʒɔɪɪŋ ðə ˈkɒnfrəns ˈseʃnz təˈdeɪ/"",
      ""meaningVi"": ""Hôm nay bạn có thấy các phiên thảo luận của hội thảo thú vị không?"",
      ""wordChips"": [
        ""Are"",
        ""you"",
        ""enjoying"",
        ""the"",
        ""conference"",
        ""sessions"",
        ""today""
      ],
      ""distractors"": [
        ""do"",
        ""enjoy"",
        ""meeting"",
        ""workshops""
      ]
    },
    {
      ""id"": ""en-l11-dict-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Trao đổi danh thiếp (Dễ)"",
      ""sentence"": ""Here is my business card with my contact details."",
      ""ipa"": ""/hɪər ɪz maɪ ˈbɪznəs kɑːd wɪð maɪ ˈkɒntækt ˈdiːteɪlz/"",
      ""meaningVi"": ""Đây là danh thiếp của tôi kèm thông tin liên lạc."",
      ""wordChips"": [
        ""Here"",
        ""is"",
        ""my"",
        ""business"",
        ""card"",
        ""with"",
        ""my"",
        ""contact"",
        ""details""
      ],
      ""distractors"": [
        ""there"",
        ""give"",
        ""phone"",
        ""number""
      ]
    },
    {
      ""id"": ""en-l11-dict-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Tạo dựng niềm tin (Vừa)"",
      ""sentence"": ""Active listening allows team members to build strong interpersonal trust."",
      ""ipa"": ""/ˈæktɪv ˈlɪsnɪŋ əˈlaʊz tiːm ˈmembəz tuː bɪld strɒŋ ˌɪntəˈpɜːsənl trʌst/"",
      ""meaningVi"": ""Lắng nghe chủ động giúp các thành viên trong nhóm xây dựng niềm tin vững chắc."",
      ""wordChips"": [
        ""Active"",
        ""listening"",
        ""allows"",
        ""team"",
        ""members"",
        ""to"",
        ""build"",
        ""strong"",
        ""interpersonal"",
        ""trust""
      ],
      ""distractors"": [
        ""action"",
        ""creates"",
        ""faith"",
        ""relations""
      ]
    },
    {
      ""id"": ""en-l11-dict-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Mở rộng mạng lưới (Vừa)"",
      ""sentence"": ""Attending international workshops provides great opportunities to expand professional networks."",
      ""ipa"": ""/əˈtendɪŋ ˌɪntəˈnæʃnəl ˈwɜːkʃɒps prəˈvaɪdz ɡreɪt ˌɒpəˈtjuːnətiz tuː ɪkˈspænd prəˈfeʃənl ˈnetwɜːks/"",
      ""meaningVi"": ""Tham gia các buổi hội thảo quốc tế mang lại cơ hội tuyệt vời để mở rộng mạng lưới nghề nghiệp."",
      ""wordChips"": [
        ""Attending"",
        ""international"",
        ""workshops"",
        ""provides"",
        ""great"",
        ""opportunities"",
        ""to"",
        ""expand"",
        ""professional"",
        ""networks""
      ],
      ""distractors"": [
        ""join"",
        ""seminars"",
        ""gives"",
        ""contacts""
      ]
    },
    {
      ""id"": ""en-l11-dict-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Học hỏi kinh nghiệm (Vừa)"",
      ""sentence"": ""I am eager to learn from your hands-on experience in cloud computing."",
      ""ipa"": ""/aɪ æm ˈiːɡə tuː lɜːn frɒm jɔː hændz ɒn ɪkˈspɪəriəns ɪn klaʊd kəmˈpjuːtɪŋ/"",
      ""meaningVi"": ""Tôi rất hào hứng được học hỏi từ kinh nghiệm thực chiến của bạn trong lĩnh vực điện toán đám mây."",
      ""wordChips"": [
        ""I"",
        ""am"",
        ""eager"",
        ""to"",
        ""learn"",
        ""from"",
        ""your"",
        ""hands-on"",
        ""experience"",
        ""in"",
        ""cloud"",
        ""computing""
      ],
      ""distractors"": [
        ""keen"",
        ""gain"",
        ""practical"",
        ""systems""
      ]
    },
    {
      ""id"": ""en-l11-dict-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Hẹn gặp lại (Vừa)"",
      ""sentence"": ""Let us schedule a brief follow-up call sometime next week."",
      ""ipa"": ""/let ʌs ˈʃedjuːl ə briːf ˈfɒləʊ ʌp kɔːl ˈsʌmtaɪm nekst wiːk/"",
      ""meaningVi"": ""Chúng ta hãy sắp xếp một cuộc gọi trao đổi tiếp theo vào tuần tới nhé."",
      ""wordChips"": [
        ""Let"",
        ""us"",
        ""schedule"",
        ""a"",
        ""brief"",
        ""follow-up"",
        ""call"",
        ""sometime"",
        ""next"",
        ""week""
      ],
      ""distractors"": [
        ""make"",
        ""short"",
        ""meeting"",
        ""coming""
      ]
    },
    {
      ""id"": ""en-l11-dict-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Ấn tượng ban đầu (Vừa)"",
      ""sentence"": ""Her concise introduction left a very positive impression on the whole committee."",
      ""ipa"": ""/hɜː kənˈsaɪs ˌɪntrəˈdʌkʃn left ə ˈveri ˈpɒzətɪv ɪmˈpreʃn ɒn ðə həʊl kəˈmɪti/"",
      ""meaningVi"": ""Phần tự giới thiệu súc tích của cô ấy đã để lại ấn tượng rất tích cực cho toàn thể hội đồng."",
      ""wordChips"": [
        ""Her"",
        ""concise"",
        ""introduction"",
        ""left"",
        ""a"",
        ""very"",
        ""positive"",
        ""impression"",
        ""on"",
        ""the"",
        ""whole"",
        ""committee""
      ],
      ""distractors"": [
        ""short"",
        ""speech"",
        ""made"",
        ""board""
      ]
    },
    {
      ""id"": ""en-l11-dict-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Sự thấu cảm giao tiếp (Khó)"",
      ""sentence"": ""Demonstrating genuine professional curiosity fosters enduring collaborative partnerships across global industries."",
      ""ipa"": ""/ˈdemənstreɪtɪŋ ˈdʒenjuɪn prəˈfeʃənl ˌkjʊəriˈɒsəti ˈfɒstəz ɪnˈdjʊərɪŋ kəˈlæbərətɪv ˈpɑːtnəʃɪps əˈkrɒs ˈɡləʊbl ˈɪndəstriz/"",
      ""meaningVi"": ""Thể hiện sự hiếu tri nghề nghiệp chân thành nuôi dưỡng những quan hệ đối tác hợp tác bền bỉ qua các ngành công nghiệp toàn cầu."",
      ""wordChips"": [
        ""Demonstrating"",
        ""genuine"",
        ""professional"",
        ""curiosity"",
        ""fosters"",
        ""enduring"",
        ""collaborative"",
        ""partnerships"",
        ""across"",
        ""global"",
        ""industries""
      ],
      ""distractors"": [
        ""showing"",
        ""true"",
        ""creates"",
        ""long-term""
      ]
    },
    {
      ""id"": ""en-l11-dict-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Kết nối chiến lược (Khó)"",
      ""sentence"": ""Informal gatherings often lay the essential groundwork for high-stakes strategic negotiations."",
      ""ipa"": ""/ɪnˈfɔːml ˈɡæðərɪŋz ˈɒfn leɪ ði ɪˈsenʃl ˈɡraʊndwɜːk fɔː haɪ steɪks strəˈtiːdʒɪk nɪˌɡəʊʃiˈeɪʃnz/"",
      ""meaningVi"": ""Các buổi gặp gỡ thân mật thường đặt nền tảng trọng yếu cho những cuộc đàm phán chiến lược tầm cỡ."",
      ""wordChips"": [
        ""Informal"",
        ""gatherings"",
        ""often"",
        ""lay"",
        ""the"",
        ""essential"",
        ""groundwork"",
        ""for"",
        ""high-stakes"",
        ""strategic"",
        ""negotiations""
      ],
      ""distractors"": [
        ""casual"",
        ""meetings"",
        ""build"",
        ""foundation""
      ]
    },
    {
      ""id"": ""en-l11-dict-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Đối thoại mang tính xây dựng (Khó)"",
      ""sentence"": ""Effective conversationalists intuitively ask open-ended questions that encourage authentic personal disclosure."",
      ""ipa"": ""/ɪˈfektɪv ˌkɒnvəˈseɪʃnəlɪsts ɪnˈtjuːɪtɪvli ɑːsk ˌəʊpən ˈendɪd ˈkwestʃənz ðæt ɪnˈkʌrɪdʒ ɔːˈθentɪk ˈpɜːsənl dɪsˈkləʊʒə/"",
      ""meaningVi"": ""Những người giao tiếp hiệu quả luôn trực giác đặt câu hỏi mở khuyến khích sự chia sẻ cá nhân chân thực."",
      ""wordChips"": [
        ""Effective"",
        ""conversationalists"",
        ""intuitively"",
        ""ask"",
        ""open-ended"",
        ""questions"",
        ""that"",
        ""encourage"",
        ""authentic"",
        ""personal"",
        ""disclosure""
      ],
      ""distractors"": [
        ""skilled"",
        ""speakers"",
        ""prompt"",
        ""sharing""
      ]
    },
    {
      ""id"": ""en-l11-dict-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Hòa nhập cộng đồng công nghệ (Khó)"",
      ""sentence"": ""Immersing yourself in multi-disciplinary forums substantially accelerates your technical leadership growth."",
      ""ipa"": ""/ɪˈmɜːsɪŋ jɔːˈself ɪn ˈmʌlti ˈdɪsəplɪnəri ˈfɔːrəmz səbˈstænʃli əkˈseləreɪts jɔː ˈteknɪkl ˈliːdəʃɪp ɡrəʊθ/"",
      ""meaningVi"": ""Hòa mình vào các diễn đàn đa ngành tăng tốc đáng kể sự phát triển năng lực lãnh đạo kỹ thuật của bạn."",
      ""wordChips"": [
        ""Immersing"",
        ""yourself"",
        ""in"",
        ""multi-disciplinary"",
        ""forums"",
        ""substantially"",
        ""accelerates"",
        ""your"",
        ""technical"",
        ""leadership"",
        ""growth""
      ],
      ""distractors"": [
        ""joining"",
        ""groups"",
        ""greatly"",
        ""improves""
      ]
    },
    {
      ""id"": ""en-l11-dict-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Văn hóa ứng xử chuyên nghiệp (Khó)"",
      ""sentence"": ""Reciprocal respect and active presence transform casual pleasantries into enduring professional relationships."",
      ""ipa"": ""/rɪˈsɪprəkl rɪˈspekt ænd ˈæktɪv ˈprezns trænsˈfɔːm ˈkæʒuəl ˈplezntriz ˈɪntuː ɪnˈdjʊərɪŋ prəˈfeʃənl rɪˈleɪʃnʃɪps/"",
      ""meaningVi"": ""Sự tôn trọng hai chiều và sự hiện diện tích cực biến những lời xã giao thành các mối quan hệ nghề nghiệp bền vững."",
      ""wordChips"": [
        ""Reciprocal"",
        ""respect"",
        ""and"",
        ""active"",
        ""presence"",
        ""transform"",
        ""casual"",
        ""pleasantries"",
        ""into"",
        ""enduring"",
        ""professional"",
        ""relationships""
      ],
      ""distractors"": [
        ""mutual"",
        ""courtesy"",
        ""turn"",
        ""lasting""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.1 Pronunciation (OrderIndex 13)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444446"), "pronunciation", 13),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444446"),
            OrderIndex = 13,
            Type = "pronunciation",
            Title = "Chấm phát âm mở rộng (Bài 1.1): 15 câu luyện âm /θ/-/ð/, âm cuối và đuôi -s/-ed",
            Description = "Luyện phát âm với AI chấm điểm trực tiếp, phát hiện lỗi sai âm đuôi và các âm khó phổ biến của người Việt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l11-pron-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm th & âm cuối t (Dễ)"",
      ""sentence"": ""Thank you for this seat at the conference table."",
      ""ipa"": ""/θæŋk juː fɔː ðɪs siːt æt ðə ˈkɒnfrəns ˈteɪbl/"",
      ""meaningVi"": ""Cảm ơn bạn vì chỗ ngồi này tại bàn hội nghị."",
      ""targetWords"": [
        ""thank"",
        ""this"",
        ""seat"",
        ""table""
      ],
      ""keyPhrases"": [
        ""thank you"",
        ""this seat"",
        ""conference table""
      ]
    },
    {
      ""id"": ""en-l11-pron-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /ð/ và số ít -s (Dễ)"",
      ""sentence"": ""That engineer works on modern mobile apps."",
      ""ipa"": ""/ðæt ˌendʒɪˈnɪə wɜːks ɒn ˈmɒdn ˈməʊbaɪl æps/"",
      ""meaningVi"": ""Kỹ sư đó làm việc về các ứng dụng di động hiện đại."",
      ""targetWords"": [
        ""that"",
        ""works"",
        ""apps""
      ],
      ""keyPhrases"": [
        ""that engineer"",
        ""works on"",
        ""mobile apps""
      ]
    },
    {
      ""id"": ""en-l11-pron-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm cuối /θ/ & /t/ (Dễ)"",
      ""sentence"": ""Both teams hope to meet with our clients."",
      ""ipa"": ""/bəʊθ tiːmz həʊp tuː miːt wɪð ˈaʊə ˈklaɪənts/"",
      ""meaningVi"": ""Cả hai nhóm đều hy vọng gặp gỡ các khách hàng của chúng ta."",
      ""targetWords"": [
        ""both"",
        ""teams"",
        ""meet"",
        ""with"",
        ""clients""
      ],
      ""keyPhrases"": [
        ""both teams"",
        ""meet with"",
        ""our clients""
      ]
    },
    {
      ""id"": ""en-l11-pron-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm đuôi -ed dạng /t/ (Dễ)"",
      ""sentence"": ""She introduced herself and talked with everyone."",
      ""ipa"": ""/ʃiː ˌɪntrəˈdjuːst hɜːˈself ænd tɔːkt wɪð ˈevriwʌn/"",
      ""meaningVi"": ""Cô ấy đã tự giới thiệu và trò chuyện cùng tất cả mọi người."",
      ""targetWords"": [
        ""introduced"",
        ""talked"",
        ""with""
      ],
      ""keyPhrases"": [
        ""introduced herself"",
        ""talked with"",
        ""everyone""
      ]
    },
    {
      ""id"": ""en-l11-pron-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm đuôi -s/es dạng /z/ (Dễ)"",
      ""sentence"": ""He shares useful ideas and makes good friends."",
      ""ipa"": ""/hiː ʃeəz ˈjuːsfl aɪˈdɪəz ænd meɪks ɡʊd frendz/"",
      ""meaningVi"": ""Anh ấy chia sẻ các ý tưởng hữu ích và kết giao những người bạn tốt."",
      ""targetWords"": [
        ""shares"",
        ""ideas"",
        ""makes"",
        ""friends""
      ],
      ""keyPhrases"": [
        ""shares useful ideas"",
        ""makes good friends""
      ]
    },
    {
      ""id"": ""en-l11-pron-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Phụ âm ghép /kr/ & /θ/ (Vừa)"",
      ""sentence"": ""Thinking through complex technical challenges strengthens our engineering team."",
      ""ipa"": ""/ˈθɪŋkɪŋ θruː ˈkɒmpleks ˈteknɪkl ˈtʃælɪndʒɪz ˈstreŋθnz ˈaʊər ˌendʒɪˈnɪərɪŋ tiːm/"",
      ""meaningVi"": ""Tư duy kỹ càng qua các thách thức kỹ thuật phức tạp củng cố sức mạnh cho đội ngũ kỹ sư."",
      ""targetWords"": [
        ""thinking"",
        ""through"",
        ""complex"",
        ""strengthens""
      ],
      ""keyPhrases"": [
        ""thinking through"",
        ""technical challenges"",
        ""strengthens team""
      ]
    },
    {
      ""id"": ""en-l11-pron-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm cuối /ps/ & /kts/ (Vừa)"",
      ""sentence"": ""Our software architects launched three innovative projects this month."",
      ""ipa"": ""/ˈaʊə ˈsɒftweər ˈɑːkɪtekts lɔːntʃt θriː ɪnˈnɒvətɪv ˈprɒdʒekts ðɪs mʌnθ/"",
      ""meaningVi"": ""Các kiến trúc sư phần mềm của chúng tôi đã khởi động 3 dự án đột phá trong tháng này."",
      ""targetWords"": [
        ""architects"",
        ""launched"",
        ""three"",
        ""projects"",
        ""month""
      ],
      ""keyPhrases"": [
        ""software architects"",
        ""launched projects"",
        ""this month""
      ]
    },
    {
      ""id"": ""en-l11-pron-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Nối âm phụ âm sang nguyên âm (Vừa)"",
      ""sentence"": ""It is an honor to speak about artificial intelligence with you."",
      ""ipa"": ""/ɪt ɪz ən ˈɒnə tuː spiːk əˈbaʊt ˌɑːtɪˈfɪʃl ɪnˈtelɪdʒəns wɪð juː/"",
      ""meaningVi"": ""Thật là vinh hạnh khi được trò chuyện về trí tuệ nhân tạo cùng bạn."",
      ""targetWords"": [
        ""honor"",
        ""speak"",
        ""intelligence"",
        ""with""
      ],
      ""keyPhrases"": [
        ""it is an honor"",
        ""artificial intelligence"",
        ""with you""
      ]
    },
    {
      ""id"": ""en-l11-pron-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đuôi -ed dạng /ɪd/ & /d/ (Vừa)"",
      ""sentence"": ""The committee accepted the proposal and commended our collaborative efforts."",
      ""ipa"": ""/ðə kəˈmɪti əkˈseptɪd ðə prəˈpəʊzl ænd kəˈmendɪd ˈaʊə kəˈlæbərətɪv ˈefəts/"",
      ""meaningVi"": ""Hội đồng đã phê duyệt đề xuất và khen ngợi những nỗ lực hợp tác của chúng tôi."",
      ""targetWords"": [
        ""accepted"",
        ""commended"",
        ""collaborative"",
        ""efforts""
      ],
      ""keyPhrases"": [
        ""accepted proposal"",
        ""commended efforts"",
        ""collaborative efforts""
      ]
    },
    {
      ""id"": ""en-l11-pron-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Cụm phụ âm /str/ & /ndʒ/ (Vừa)"",
      ""sentence"": ""We streamlined our onboarding procedure to exchange knowledge more smoothly."",
      ""ipa"": ""/wiː ˈstriːmlaɪnd ˈaʊər ˈɒnbɔːdɪŋ prəˈsiːdʒə tuː ɪksˈtʃeɪndʒ ˈnɒlɪdʒ mɔː ˈsmuːðli/"",
      ""meaningVi"": ""Chúng tôi đã tối ưu hóa quy trình hòa nhập để trao đổi tri thức mượt mà hơn."",
      ""targetWords"": [
        ""streamlined"",
        ""procedure"",
        ""exchange"",
        ""knowledge"",
        ""smoothly""
      ],
      ""keyPhrases"": [
        ""streamlined procedure"",
        ""exchange knowledge"",
        ""more smoothly""
      ]
    },
    {
      ""id"": ""en-l11-pron-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Âm /θ/-/ð/ liên tục & trọng âm từ (Khó)"",
      ""sentence"": ""They thoroughly investigated whether these synthetic theories withstand rigorous verification."",
      ""ipa"": ""/ðeɪ ˈθʌrəli ɪnˈvestɪɡeɪtɪd ˈweðə ðiːz sɪnˈθetɪk ˈθɪəriz wɪðˈstænd ˈrɪɡərəs ˌverɪfɪˈkeɪʃn/"",
      ""meaningVi"": ""Họ đã điều tra kỹ lưỡng liệu những lý thuyết tổng hợp này có vượt qua được sự kiểm chứng nghiêm ngặt không."",
      ""targetWords"": [
        ""thoroughly"",
        ""whether"",
        ""these"",
        ""synthetic"",
        ""theories"",
        ""withstand""
      ],
      ""keyPhrases"": [
        ""thoroughly investigated"",
        ""synthetic theories"",
        ""rigorous verification""
      ]
    },
    {
      ""id"": ""en-l11-pron-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Đuôi phụ âm phức hợp /kts/ & /sts/ (Khó)"",
      ""sentence"": ""Distinguished panelists shared distinct insights on how trust influences organizational aspects."",
      ""ipa"": ""/dɪˈstɪŋɡwɪʃt ˈpænəlɪsts ʃeəd dɪˈstɪŋkt ˈɪnsaɪts ɒn haʊ trʌst ˈɪnfluənsɪz ˌɔːɡənaɪˈzeɪʃənl ˈæspekts/"",
      ""meaningVi"": ""Các chuyên gia danh tiếng đã chia sẻ góc nhìn khác biệt về cách niềm tin ảnh hưởng đến các khía cạnh tổ chức."",
      ""targetWords"": [
        ""distinguished"",
        ""panelists"",
        ""distinct"",
        ""insights"",
        ""aspects""
      ],
      ""keyPhrases"": [
        ""distinguished panelists"",
        ""distinct insights"",
        ""organizational aspects""
      ]
    },
    {
      ""id"": ""en-l11-pron-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Nhịp điệu và ngữ điệu câu phức (Khó)"",
      ""sentence"": ""Although enthusiasm initiates relationships, demonstrated competence consistently preserves them."",
      ""ipa"": ""/ɔːlˈðəʊ ɪnˈθjuːziæzəm ɪˈnɪʃieɪts rɪˈleɪʃnʃɪps, ˈdemənstreɪtɪd ˈkɒmpɪtəns kənˈsɪstəntli prɪˈzɜːvz ðem/"",
      ""meaningVi"": ""Dù sự nhiệt tình khởi đầu mối quan hệ, năng lực được chứng minh mới duy trì được nó lâu bền."",
      ""targetWords"": [
        ""enthusiasm"",
        ""initiates"",
        ""relationships"",
        ""competence"",
        ""preserves""
      ],
      ""keyPhrases"": [
        ""enthusiasm initiates"",
        ""demonstrated competence"",
        ""preserves them""
      ]
    },
    {
      ""id"": ""en-l11-pron-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Âm /v/ vs /w/ & /θ/ (Khó)"",
      ""sentence"": ""We value whether worthwhile viewpoints thrive within this collaborative workshop."",
      ""ipa"": ""/wiː ˈvæljuː ˈweðə ˈwɜːθwaɪl ˈvjuːpɔɪnts θraɪv wɪðˈɪn ðɪs kəˈlæbərətɪv ˈwɜːkʃɒp/"",
      ""meaningVi"": ""Chúng tôi trân trọng việc những quan điểm đáng giá phát triển mạnh mẽ trong buổi hội thảo hợp tác này."",
      ""targetWords"": [
        ""value"",
        ""whether"",
        ""worthwhile"",
        ""viewpoints"",
        ""thrive"",
        ""within""
      ],
      ""keyPhrases"": [
        ""worthwhile viewpoints"",
        ""thrive within"",
        ""collaborative workshop""
      ]
    },
    {
      ""id"": ""en-l11-pron-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Phụ âm cuối và giảm âm (Khó)"",
      ""sentence"": ""Authentic professionals effortlessly articulate their strengths without sounding boastful."",
      ""ipa"": ""/ɔːˈθentɪk prəˈfeʃənlz ˈefətləsli ɑːˈtɪkjuleɪt ðeə streŋθs wɪðˈaʊt ˈsaʊndɪŋ ˈbəʊstfl/"",
      ""meaningVi"": ""Những chuyên gia chân chính diễn đạt thế mạnh của mình một cách tự nhiên mà không tạo cảm giác tự cao."",
      ""targetWords"": [
        ""authentic"",
        ""effortlessly"",
        ""articulate"",
        ""strengths"",
        ""boastful""
      ],
      ""keyPhrases"": [
        ""authentic professionals"",
        ""articulate strengths"",
        ""without sounding boastful""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.1 Word Map (OrderIndex 14)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444446"), "word_map", 14),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444446"),
            OrderIndex = 14,
            Type = "word_map",
            Title = "Sơ đồ từ vựng Mind Map mở rộng (Bài 1.1): 12 sơ đồ Dễ → Vừa → Khó",
            Description = "Hệ thống sơ đồ tư duy từ vựng đa cấp độ kèm phiên âm IPA, collocations, phrasal verbs, synonyms và ví dụ ngữ cảnh thực chiến.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""wm-l11-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: connect"",
      ""centralWord"": ""connect"",
      ""ipa"": ""/kəˈnekt/"",
      ""meaningVi"": ""Kết nối, liên kết, làm quen"",
      ""collocations"": [
        {
          ""text"": ""connect with people"",
          ""ipa"": ""/kəˈnekt wɪð ˈpiːpl/"",
          ""meaningVi"": ""Kết nối với mọi người"",
          ""example"": ""Conferences are great places to connect with people.""
        },
        {
          ""text"": ""stay connected"",
          ""ipa"": ""/steɪ kəˈnektɪd/"",
          ""meaningVi"": ""Giữ liên lạc"",
          ""example"": ""Let's stay connected on LinkedIn.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""reach out to"",
          ""ipa"": ""/riːtʃ aʊt tuː/"",
          ""meaningVi"": ""Chủ động liên hệ"",
          ""example"": ""Feel free to reach out to me anytime.""
        },
        {
          ""text"": ""keep in touch"",
          ""ipa"": ""/kiːp ɪn tʌtʃ/"",
          ""meaningVi"": ""Giữ liên lạc"",
          ""example"": ""We promised to keep in touch after the summit.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""link"",
          ""ipa"": ""/lɪŋk/"",
          ""meaningVi"": ""Liên kết"",
          ""example"": ""Our mutual friends linked us together.""
        },
        {
          ""text"": ""network"",
          ""ipa"": ""/ˈnetwɜːk/"",
          ""meaningVi"": ""Xây dựng mối quan hệ"",
          ""example"": ""Young engineers should network actively.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Building meaningful connections accelerates career development."",
          ""meaningVi"": ""Xây dựng các mối quan hệ ý nghĩa thúc đẩy sự phát triển sự nghiệp.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: greet"",
      ""centralWord"": ""greet"",
      ""ipa"": ""/ɡriːt/"",
      ""meaningVi"": ""Chào hỏi, đón tiếp"",
      ""collocations"": [
        {
          ""text"": ""warmly greet"",
          ""ipa"": ""/ˈwɔːmli ɡriːt/"",
          ""meaningVi"": ""Nồng nhiệt chào đón"",
          ""example"": ""The host warmly greeted every attendee.""
        },
        {
          ""text"": ""greet with a smile"",
          ""ipa"": ""/ɡriːt wɪð ə smaɪl/"",
          ""meaningVi"": ""Chào với nụ cười"",
          ""example"": ""Always greet new colleagues with a smile.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""say hello to"",
          ""ipa"": ""/seɪ həˈləʊ tuː/"",
          ""meaningVi"": ""Chào hỏi ai đó"",
          ""example"": ""Take a moment to say hello to your neighbors.""
        },
        {
          ""text"": ""welcome in"",
          ""ipa"": ""/ˈwelkəm ɪn/"",
          ""meaningVi"": ""Mời vào, đón tiếp"",
          ""example"": ""They welcomed in the new participants.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""welcome"",
          ""ipa"": ""/ˈwelkəm/"",
          ""meaningVi"": ""Chào đón"",
          ""example"": ""We welcome you to our community.""
        },
        {
          ""text"": ""salute"",
          ""ipa"": ""/səˈluːt/"",
          ""meaningVi"": ""Chào kính cẩn"",
          ""example"": ""He saluted his mentor respectfully.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Greeting someone warmly sets a positive tone for conversation."",
          ""meaningVi"": ""Chào ai đó một cách ấm áp tạo ra không khí tích cực cho cuộc trò chuyện.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: hobby"",
      ""centralWord"": ""hobby"",
      ""ipa"": ""/ˈhɒbi/"",
      ""meaningVi"": ""Sở thích lúc rảnh rỗi"",
      ""collocations"": [
        {
          ""text"": ""pursue a hobby"",
          ""ipa"": ""/pəˈsjuː ə ˈhɒbi/"",
          ""meaningVi"": ""Theo đuổi sở thích"",
          ""example"": ""He pursues photography as a creative hobby.""
        },
        {
          ""text"": ""share common hobbies"",
          ""ipa"": ""/ʃeə ˈkɒmən ˈhɒbiz/"",
          ""meaningVi"": ""Có chung sở thích"",
          ""example"": ""We bonded because we share common hobbies.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""take up"",
          ""ipa"": ""/teɪk ʌp/"",
          ""meaningVi"": ""Bắt đầu một sở thích mới"",
          ""example"": ""I took up cycling during the summer.""
        },
        {
          ""text"": ""get into"",
          ""ipa"": ""/ɡet ˈɪntuː/"",
          ""meaningVi"": ""Hào hứng với"",
          ""example"": ""She recently got into marathon running.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""pastime"",
          ""ipa"": ""/ˈpɑːstaɪm/"",
          ""meaningVi"": ""Thú tiêu khiển"",
          ""example"": ""Reading is her favorite pastime.""
        },
        {
          ""text"": ""interest"",
          ""ipa"": ""/ˈɪntrəst/"",
          ""meaningVi"": ""Mối quan tâm"",
          ""example"": ""We discovered many shared personal interests.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Talking about personal hobbies is a gentle way to break the ice."",
          ""meaningVi"": ""Nói về sở thích cá nhân là một cách nhẹ nhàng để phá băng câu chuyện.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: chat"",
      ""centralWord"": ""chat"",
      ""ipa"": ""/tʃæt/"",
      ""meaningVi"": ""Nói chuyện phiếm, trò chuyện thân mật"",
      ""collocations"": [
        {
          ""text"": ""casual chat"",
          ""ipa"": ""/ˈkæʒuəl tʃæt/"",
          ""meaningVi"": ""Trò chuyện thân mật"",
          ""example"": ""A casual chat over coffee can lead to new ideas.""
        },
        {
          ""text"": ""have a quick chat"",
          ""ipa"": ""/hæv ə kwɪk tʃæt/"",
          ""meaningVi"": ""Nói chuyện nhanh"",
          ""example"": ""Can we have a quick chat before the session?""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""chat up"",
          ""ipa"": ""/tʃæt ʌp/"",
          ""meaningVi"": ""Bắt chuyện làm quen"",
          ""example"": ""He chatted up someone standing in line.""
        },
        {
          ""text"": ""catch up with"",
          ""ipa"": ""/kætʃ ʌp wɪð/"",
          ""meaningVi"": ""Hàn huyên cập nhật"",
          ""example"": ""I caught up with an old university classmate.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""converse"",
          ""ipa"": ""/kənˈvɜːs/"",
          ""meaningVi"": ""Trò chuyện"",
          ""example"": ""They conversed in fluent English.""
        },
        {
          ""text"": ""talk"",
          ""ipa"": ""/tɔːk/"",
          ""meaningVi"": ""Nói chuyện"",
          ""example"": ""We talked about our travel experiences.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""A friendly chat often paves the way for strong business partnerships."",
          ""meaningVi"": ""Một buổi nói chuyện thân thiện thường mở đường cho các quan hệ đối tác kinh doanh.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: background"",
      ""centralWord"": ""background"",
      ""ipa"": ""/ˈbækɡraʊnd/"",
      ""meaningVi"": ""Nền tảng học vấn, lý lịch chuyên môn"",
      ""collocations"": [
        {
          ""text"": ""educational background"",
          ""ipa"": ""/ˌedʒuˈkeɪʃənl ˈbækɡraʊnd/"",
          ""meaningVi"": ""Nền tảng học vấn"",
          ""example"": ""Her educational background is in computer science.""
        },
        {
          ""text"": ""professional background"",
          ""ipa"": ""/prəˈfeʃənl ˈbækɡraʊnd/"",
          ""meaningVi"": ""Kinh nghiệm chuyên môn"",
          ""example"": ""Tell us briefly about your professional background.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""come from"",
          ""ipa"": ""/kʌm frɒm/"",
          ""meaningVi"": ""Xuất thân từ"",
          ""example"": ""Many attendees come from diverse technical domains.""
        },
        {
          ""text"": ""draw upon"",
          ""ipa"": ""/drɔː əˈpɒn/"",
          ""meaningVi"": ""Tận dụng vốn kinh nghiệm"",
          ""example"": ""He draws upon his engineering background to solve problems.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""experience"",
          ""ipa"": ""/ɪkˈspɪəriəns/"",
          ""meaningVi"": ""Kinh nghiệm"",
          ""example"": ""She has extensive industry experience.""
        },
        {
          ""text"": ""credentials"",
          ""ipa"": ""/krəˈdenʃlz/"",
          ""meaningVi"": ""Năng lực, bằng cấp"",
          ""example"": ""His credentials speak for themselves.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Sharing your background clearly builds immediate credibility with new acquaintances."",
          ""meaningVi"": ""Chia sẻ nền tảng của bạn một cách rõ ràng tạo sự tin cậy tức thì với người mới quen.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: network"",
      ""centralWord"": ""network"",
      ""ipa"": ""/ˈnetwɜːk/"",
      ""meaningVi"": ""Mạng lưới quan hệ, mở rộng kết nối"",
      ""collocations"": [
        {
          ""text"": ""professional network"",
          ""ipa"": ""/prəˈfeʃənl ˈnetwɜːk/"",
          ""meaningVi"": ""Mạng lưới nghề nghiệp"",
          ""example"": ""A broad professional network offers unexpected opportunities.""
        },
        {
          ""text"": ""networking event"",
          ""ipa"": ""/ˈnetwɜːkɪŋ ɪˈvent/"",
          ""meaningVi"": ""Sự kiện giao lưu kết nối"",
          ""example"": ""This summit is the premier networking event of the year.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""branch out"",
          ""ipa"": ""/brɑːntʃ aʊt/"",
          ""meaningVi"": ""Mở rộng sang lĩnh vực mới"",
          ""example"": ""She decided to branch out into artificial intelligence.""
        },
        {
          ""text"": ""mingle with"",
          ""ipa"": ""/ˈmɪŋɡl wɪð/"",
          ""meaningVi"": ""Hòa đồng giao lưu"",
          ""example"": ""Take time to mingle with international delegates.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""connections"",
          ""ipa"": ""/kəˈnekʃnz/"",
          ""meaningVi"": ""Mối quan hệ"",
          ""example"": ""He used his industry connections to find funding.""
        },
        {
          ""text"": ""circle"",
          ""ipa"": ""/ˈsɜːkl/"",
          ""meaningVi"": ""Vòng quan hệ"",
          ""example"": ""She expanded her professional circle significantly.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Effective networking focuses on how you can add value to others."",
          ""meaningVi"": ""Giao lưu kết nối hiệu quả tập trung vào cách bạn có thể mang lại giá trị cho người khác.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: conversation"",
      ""centralWord"": ""conversation"",
      ""ipa"": ""/ˌkɒnvəˈseɪʃn/"",
      ""meaningVi"": ""Cuộc trò chuyện, đối thoại"",
      ""collocations"": [
        {
          ""text"": ""spark a conversation"",
          ""ipa"": ""/spɑːk ə ˌkɒnvəˈseɪʃn/"",
          ""meaningVi"": ""Khơi mào cuộc trò chuyện"",
          ""example"": ""An intriguing question can spark a conversation.""
        },
        {
          ""text"": ""meaningful conversation"",
          ""ipa"": ""/ˈmiːnɪŋfl ˌkɒnvəˈseɪʃn/"",
          ""meaningVi"": ""Cuộc trò chuyện sâu sắc"",
          ""example"": ""We had a truly meaningful conversation about career goals.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""keep up with"",
          ""ipa"": ""/kiːp ʌp wɪð/"",
          ""meaningVi"": ""Bắt kịp nhịp nói"",
          ""example"": ""It is easy to keep up with his clear speaking pace.""
        },
        {
          ""text"": ""wrap up"",
          ""ipa"": ""/ræp ʌp/"",
          ""meaningVi"": ""Khép lại cuộc nói chuyện"",
          ""example"": ""Let's wrap up our conversation before the speaker starts.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""dialogue"",
          ""ipa"": ""/ˈdaɪəlɒɡ/"",
          ""meaningVi"": ""Đối thoại"",
          ""example"": ""Constructive dialogue resolves misunderstandings.""
        },
        {
          ""text"": ""discussion"",
          ""ipa"": ""/dɪˈskʌʃn/"",
          ""meaningVi"": ""Thảo luận"",
          ""example"": ""The panel discussion provoked deep reflections.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Listening more than speaking turns a routine conversation into an authentic bond."",
          ""meaningVi"": ""Lắng nghe nhiều hơn nói biến một cuộc trò chuyện thông thường thành mối liên kết chân thật.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: collaborate"",
      ""centralWord"": ""collaborate"",
      ""ipa"": ""/kəˈlæbəreɪt/"",
      ""meaningVi"": ""Cộng tác, cùng làm việc"",
      ""collocations"": [
        {
          ""text"": ""collaborate on projects"",
          ""ipa"": ""/kəˈlæbəreɪt ɒn ˈprɒdʒekts/"",
          ""meaningVi"": ""Cộng tác trong dự án"",
          ""example"": ""Our research labs collaborate on biomedical projects.""
        },
        {
          ""text"": ""cross-border collaboration"",
          ""ipa"": ""/krɒs ˈbɔːdə kəˌlæbəˈreɪʃn/"",
          ""meaningVi"": ""Hợp tác xuyên quốc gia"",
          ""example"": ""Cross-border collaboration fuels global tech innovation.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""team up"",
          ""ipa"": ""/tiːm ʌp/"",
          ""meaningVi"": ""Phối hợp cùng"",
          ""example"": ""Engineers and designers team up during hackathons.""
        },
        {
          ""text"": ""partner with"",
          ""ipa"": ""/ˈpɑːtnə wɪð/"",
          ""meaningVi"": ""Hợp tác làm đối tác"",
          ""example"": ""Our company partnered with a leading university.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""cooperate"",
          ""ipa"": ""/kəʊˈɒpəreɪt/"",
          ""meaningVi"": ""Hợp tác"",
          ""example"": ""Both departments cooperated seamlessly.""
        },
        {
          ""text"": ""coordinate"",
          ""ipa"": ""/kəʊˈɔːdɪneɪt/"",
          ""meaningVi"": ""Phối hợp"",
          ""example"": ""We must coordinate our rollout schedules.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""When professionals collaborate with mutual respect, remarkable results follow."",
          ""meaningVi"": ""Khi các chuyên gia hợp tác với sự tôn trọng lẫn nhau, kết quả vượt trội sẽ xuất hiện.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: rapport"",
      ""centralWord"": ""rapport"",
      ""ipa"": ""/ræˈpɔːr/"",
      ""meaningVi"": ""Mối quan hệ hòa hợp, thấu hiểu lẫn nhau"",
      ""collocations"": [
        {
          ""text"": ""build instant rapport"",
          ""ipa"": ""/bɪld ˈɪnstənt ræˈpɔːr/"",
          ""meaningVi"": ""Tạo thiện cảm tức thì"",
          ""example"": ""Humor and empathy help build instant rapport.""
        },
        {
          ""text"": ""establish strong rapport"",
          ""ipa"": ""/ɪˈstæblɪʃ strɒŋ ræˈpɔːr/"",
          ""meaningVi"": ""Thiết lập sự thấu hiểu sâu"",
          ""example"": ""Leaders must establish strong rapport with their workforce.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""hit it off"",
          ""ipa"": ""/hɪt ɪt ɒf/"",
          ""meaningVi"": ""Hợp cạ ngay từ đầu"",
          ""example"": ""They hit it off immediately at the orientation.""
        },
        {
          ""text"": ""warm up to"",
          ""ipa"": ""/wɔːm ʌp tuː/"",
          ""meaningVi"": ""Dần trở nên cởi mở với"",
          ""example"": ""The client quickly warmed up to our design concept.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""affinity"",
          ""ipa"": ""/əˈfɪnəti/"",
          ""meaningVi"": ""Sự đồng điệu, cảm tình"",
          ""example"": ""There is a natural affinity between curious minds.""
        },
        {
          ""text"": ""harmony"",
          ""ipa"": ""/ˈhɑːməni/"",
          ""meaningVi"": ""Sự hòa hợp"",
          ""example"": ""Working in harmony improves team resilience.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Cultivating authentic rapport precedes any fruitful negotiation or leadership success."",
          ""meaningVi"": ""Xây dựng sự hòa hợp chân thành luôn đi trước mọi thành công đàm phán hay lãnh đạo.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: acquaintance"",
      ""centralWord"": ""acquaintance"",
      ""ipa"": ""/əˈkweɪntəns/"",
      ""meaningVi"": ""Người quen, sự quen biết xã giao"",
      ""collocations"": [
        {
          ""text"": ""casual acquaintance"",
          ""ipa"": ""/ˈkæʒuəl əˈkweɪntəns/"",
          ""meaningVi"": ""Người quen xã giao"",
          ""example"": ""He was just a casual acquaintance from college.""
        },
        {
          ""text"": ""make one's acquaintance"",
          ""ipa"": ""/meɪk wʌnz əˈkweɪntəns/"",
          ""meaningVi"": ""Làm quen với ai"",
          ""example"": ""I am delighted to make your acquaintance today.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""cross paths with"",
          ""ipa"": ""/krɒs pɑːðz wɪð/"",
          ""meaningVi"": ""Tình cờ gặp lại"",
          ""example"": ""I crossed paths with an old colleague at the expo.""
        },
        {
          ""text"": ""brush up on"",
          ""ipa"": ""/brʌʃ ʌp ɒn/"",
          ""meaningVi"": ""Ôn lại hiểu biết về"",
          ""example"": ""Brush up on cultural etiquette before meeting foreign partners.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""contact"",
          ""ipa"": ""/ˈkɒntækt/"",
          ""meaningVi"": ""Đầu mối liên hệ"",
          ""example"": ""Keep in touch with valuable industry contacts.""
        },
        {
          ""text"": ""associate"",
          ""ipa"": ""/əˈsəʊʃiət/"",
          ""meaningVi"": ""Cộng sự"",
          ""example"": ""She introduced me to her business associates.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Turning a casual acquaintance into a lifelong collaborator requires continuous care."",
          ""meaningVi"": ""Biến một người quen xã giao thành cộng sự lâu năm đòi hỏi sự chăm sóc kiên trì.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: versatility"",
      ""centralWord"": ""versatility"",
      ""ipa"": ""/ˌvɜːsəˈtɪləti/"",
      ""meaningVi"": ""Sự linh hoạt, đa năng"",
      ""collocations"": [
        {
          ""text"": ""linguistic versatility"",
          ""ipa"": ""/lɪŋˈɡwɪstɪk ˌvɜːsəˈtɪləti/"",
          ""meaningVi"": ""Sự linh hoạt ngôn ngữ"",
          ""example"": ""His linguistic versatility allowed him to moderate global forums.""
        },
        {
          ""text"": ""remarkable versatility"",
          ""ipa"": ""/rɪˈmɑːkəbl ˌvɜːsəˈtɪləti/"",
          ""meaningVi"": ""Độ đa năng vượt bậc"",
          ""example"": ""Modern engineers are valued for their remarkable versatility.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""adapt to"",
          ""ipa"": ""/əˈdæpt tuː/"",
          ""meaningVi"": ""Thích nghi với"",
          ""example"": ""Successful communicators quickly adapt to diverse audiences.""
        },
        {
          ""text"": ""fit in with"",
          ""ipa"": ""/fɪt ɪn wɪð/"",
          ""meaningVi"": ""Hòa nhập cùng"",
          ""example"": ""She easily fitted in with the international delegation.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""adaptability"",
          ""ipa"": ""/əˌdæptəˈbɪləti/"",
          ""meaningVi"": ""Khả năng thích ứng"",
          ""example"": ""Adaptability is critical in fast-changing tech markets.""
        },
        {
          ""text"": ""flexibility"",
          ""ipa"": ""/ˌfleksəˈbɪləti/"",
          ""meaningVi"": ""Sự linh hoạt"",
          ""example"": ""Schedule flexibility makes coordination much easier.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Linguistic versatility enables professionals to navigate nuanced social and technical discussions."",
          ""meaningVi"": ""Sự linh hoạt về ngôn từ giúp chuyên gia làm chủ cả những đối thoại xã giao lẫn kỹ thuật tinh tế.""
        }
      ]
    },
    {
      ""id"": ""wm-l11-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: resonance"",
      ""centralWord"": ""resonance"",
      ""ipa"": ""/ˈrezənəns/"",
      ""meaningVi"": ""Sự cộng hưởng, đồng điệu sâu sắc"",
      ""collocations"": [
        {
          ""text"": ""emotional resonance"",
          ""ipa"": ""/ɪˈməʊʃənl ˈrezənəns/"",
          ""meaningVi"": ""Sự cộng hưởng cảm xúc"",
          ""example"": ""Her speech achieved profound emotional resonance with the crowd.""
        },
        {
          ""text"": ""resonate with audiences"",
          ""ipa"": ""/ˈrezəneɪt wɪð ˈɔːdiənsɪz/"",
          ""meaningVi"": ""Gây ấn tượng sâu với người nghe"",
          ""example"": ""Authentic personal stories resonate with diverse audiences.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""strike a chord with"",
          ""ipa"": ""/straɪk ə kɔːd wɪð/"",
          ""meaningVi"": ""Chạm đúng tâm lý ai"",
          ""example"": ""His message struck a chord with aspiring entrepreneurs.""
        },
        {
          ""text"": ""sink in"",
          ""ipa"": ""/sɪŋk ɪn/"",
          ""meaningVi"": ""Thấm thía, ngấm sâu"",
          ""example"": ""It took a few moments for the significance of the advice to sink in.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""relevance"",
          ""ipa"": ""/ˈreləvəns/"",
          ""meaningVi"": ""Tính liên quan, thiết thực"",
          ""example"": ""The proposal has immediate relevance to our roadmap.""
        },
        {
          ""text"": ""alignment"",
          ""ipa"": ""/əˈlaɪnmənt/"",
          ""meaningVi"": ""Sự đồng thuận, ăn khớp"",
          ""example"": ""Strategic alignment ensures all teams move in unison.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Achieving genuine resonance requires understanding the unstated aspirations of your listeners."",
          ""meaningVi"": ""Đạt được sự cộng hưởng chân thực đòi hỏi thấu hiểu những nguyện vọng chưa nói của người nghe.""
        }
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.1 Cloze (OrderIndex 15)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444446"), "cloze", 15),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444446"),
            OrderIndex = 15,
            Type = "cloze",
            Title = "Điền từ vào chỗ trống (Bài 1.1): Bài tập ngữ cảnh thực tế",
            Description = "Đọc đoạn văn hoặc câu giao tiếp và điền từ thích hợp vào chỗ trống với gợi ý và giải thích chi tiết.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l11-cloze-ext1"",
      ""difficulty"": ""easy"",
      ""title"": ""Bắt chuyện tại sảnh hội nghị (Dễ)"",
      ""promptVi"": ""Điền từ thích hợp để hoàn thành câu hỏi xã giao lịch thiệp:"",
      ""template"": ""Excuse me, is this seat {{0}}? It is a great {{1}} to meet you in person."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""taken"",
            ""free"",
            ""occupied""
          ],
          ""hintVi"": ""đã có người ngồi chưa (bắt đầu bằng t)"",
          ""explanationVi"": ""\""Is this seat taken?\"" là câu hỏi lịch sự tiêu chuẩn khi tìm chỗ ngồi.""
        },
        {
          ""acceptedAnswers"": [
            ""pleasure"",
            ""honor""
          ],
          ""hintVi"": ""niềm vinh dự, hân hạnh (bắt đầu bằng p)"",
          ""explanationVi"": ""\""It is a great pleasure to meet you\"" thể hiện thái độ tôn trọng và ấm áp.""
        }
      ],
      ""fullSentence"": ""Excuse me, is this seat taken? It is a great pleasure to meet you in person."",
      ""meaningVi"": ""Xin lỗi, chỗ này đã có ai ngồi chưa? Rất hân hạnh được gặp mặt trực tiếp bạn."",
      ""explanationVi"": ""Hai mẫu câu bắt chuyện cơ bản và tự nhiên nhất trong mọi hội thảo quốc tế.""
    },
    {
      ""id"": ""en-l11-cloze-ext2"",
      ""difficulty"": ""medium"",
      ""title"": ""Giới thiệu chuyên môn và thành phố (Vừa)"",
      ""promptVi"": ""Điền các từ để hoàn thiện câu giới thiệu bản thân 3 bước:"",
      ""template"": ""I work as a software engineer {{0}} in Hanoi, and I look {{1}} to learning more about your projects."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""based"",
            ""working"",
            ""living""
          ],
          ""hintVi"": ""đặt văn phòng / sinh sống tại (bắt đầu bằng b)"",
          ""explanationVi"": ""\""based in + thành phố\"" là cách diễn đạt chuyên nghiệp cho nơi làm việc.""
        },
        {
          ""acceptedAnswers"": [
            ""forward""
          ],
          ""hintVi"": ""mong đợi (look ___ to)"",
          ""explanationVi"": ""\""look forward to + V-ing\"" diễn tả sự mong đợi chân thành.""
        }
      ],
      ""fullSentence"": ""I work as a software engineer based in Hanoi, and I look forward to learning more about your projects."",
      ""meaningVi"": ""Tôi làm kỹ sư phần mềm làm việc tại Hà Nội, và tôi rất mong được tìm hiểu thêm về các dự án của bạn."",
      ""explanationVi"": ""Kết hợp mượt mà giữa giới thiệu vị trí công tác và bày tỏ sự tò mò nghề nghiệp.""
    },
    {
      ""id"": ""en-l11-cloze-ext3"",
      ""difficulty"": ""hard"",
      ""title"": ""Xây dựng mối quan hệ chuyên sâu (Khó)"",
      ""promptVi"": ""Điền 2 từ học thuật/công sở để hoàn thiện câu kết nối chiến lược:"",
      ""template"": ""Striking up a conversation with international peers helps establish genuine {{0}} and opens doors to fruitful {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""rapport"",
            ""trust"",
            ""connections""
          ],
          ""hintVi"": ""sự hòa hợp, thấu hiểu (bắt đầu bằng r)"",
          ""explanationVi"": ""\""rapport\"" nghĩa là mối quan hệ hòa hợp, tin cậy lẫn nhau.""
        },
        {
          ""acceptedAnswers"": [
            ""collaboration"",
            ""partnerships"",
            ""cooperation""
          ],
          ""hintVi"": ""sự cộng tác, hợp tác (bắt đầu bằng c)"",
          ""explanationVi"": ""\""fruitful collaboration\"" là cụm từ nâng tầm văn phong B2-C1.""
        }
      ],
      ""fullSentence"": ""Striking up a conversation with international peers helps establish genuine rapport and opens doors to fruitful collaboration."",
      ""meaningVi"": ""Chủ động bắt chuyện với đồng nghiệp quốc tế giúp tạo dựng sự hòa hợp chân thành và mở ra cơ hội hợp tác hiệu quả."",
      ""explanationVi"": ""Mẫu câu thể hiện tư duy kết nối đối tác quốc tế tầm vóc.""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.1 Translate (OrderIndex 16)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444446"), "translate", 16),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444446"),
            OrderIndex = 16,
            Type = "translate",
            Title = "Dịch phản xạ Việt → Anh (Bài 1.1): Diễn đạt tự nhiên",
            Description = "Chuyển câu tiếng Việt sang tiếng Anh tự nhiên với nhiều đáp án được chấp nhận và lưu ý ngữ pháp hữu ích.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l11-trans-ext1"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Chào hỏi và tự giới thiệu (Dễ)"",
      ""sourceVi"": ""Xin chào, tôi là Mai và tôi rất vui được gặp bạn hôm nay."",
      ""hintKeywords"": [
        ""hello"",
        ""Mai"",
        ""glad to meet you"",
        ""today""
      ],
      ""acceptedAnswers"": [
        ""Hello, I am Mai and I am glad to meet you today."",
        ""Hi, I'm Mai and I'm very glad to meet you today."",
        ""Hello, my name is Mai and it is great to meet you today."",
        ""Hi, I am Mai, nice to meet you today.""
      ],
      ""sampleAnswer"": ""Hello, I am Mai and I am glad to meet you today."",
      ""grammarNoteVi"": ""Có thể dùng \""glad to meet you\"" hoặc \""nice to meet you\"".""
    },
    {
      ""id"": ""en-l11-trans-ext2"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Đề nghị kết nối LinkedIn (Vừa)"",
      ""sourceVi"": ""Bạn có phiền không nếu chúng ta kết nối trên LinkedIn để giữ liên lạc sau hội thảo?"",
      ""hintKeywords"": [
        ""would you mind"",
        ""connect on LinkedIn"",
        ""stay in touch"",
        ""conference""
      ],
      ""acceptedAnswers"": [
        ""Would you mind if we connected on LinkedIn to stay in touch after the conference?"",
        ""Would you mind if we connect on LinkedIn to keep in touch after the conference?"",
        ""Do you mind if we connect on LinkedIn to stay in touch after the conference?"",
        ""Would you mind connecting on LinkedIn to stay in touch after the summit?""
      ],
      ""sampleAnswer"": ""Would you mind if we connected on LinkedIn to stay in touch after the conference?"",
      ""grammarNoteVi"": ""Cấu trúc lịch sự: \""Would you mind if + S + V-ed/V...?\"".""
    },
    {
      ""id"": ""en-l11-trans-ext3"",
      ""difficulty"": ""hard"",
      ""title"": ""Dịch: Phá băng và tạo dựng niềm tin (Khó)"",
      ""sourceVi"": ""Việc chủ động tìm ra tiếng nói chung giúp các đồng nghiệp nhanh chóng thiết lập mối quan hệ hòa hợp và tin cậy lẫn nhau."",
      ""hintKeywords"": [
        ""finding common ground"",
        ""establish rapport"",
        ""mutual trust"",
        ""colleagues""
      ],
      ""acceptedAnswers"": [
        ""Finding common ground helps colleagues quickly establish rapport and mutual trust."",
        ""Proactively finding common ground allows colleagues to establish genuine rapport and mutual trust quickly."",
        ""Finding common ground helps colleagues establish genuine rapport and mutual trust rapidly.""
      ],
      ""sampleAnswer"": ""Finding common ground helps colleagues quickly establish rapport and mutual trust."",
      ""grammarNoteVi"": ""Chủ ngữ danh động từ \""Finding common ground\"" là số ít nên động từ chia \""helps\"".""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.1 Grammar Fix (OrderIndex 17)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444446"), "grammar_fix", 17),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444446"),
            OrderIndex = 17,
            Type = "grammar_fix",
            Title = "Sửa lỗi ngữ pháp (Bài 1.1): Khắc phục các lỗi người Việt hay mắc",
            Description = "Phát hiện và sửa các lỗi sai ngữ pháp, thì động từ, mạo từ và giới từ thường gặp trong giao tiếp thực tế.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l11-gf-ext1"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Thiếu mạo từ nghề nghiệp (Dễ)"",
      ""incorrectSentence"": ""She works as software engineer in international company."",
      ""errorTypeHintVi"": ""Thiếu mạo từ bất định (a/an) trước danh từ đếm được số ít."",
      ""correctedSentence"": ""She works as a software engineer in an international company."",
      ""acceptedAnswers"": [
        ""She works as a software engineer in an international company."",
        ""She works as a software engineer at an international company.""
      ],
      ""meaningVi"": ""Cô ấy làm kỹ sư phần mềm tại một công ty quốc tế."",
      ""explanationVi"": ""Phải có \""a\"" trước \""software engineer\"" và \""an\"" trước nguyên âm \""international\"".""
    },
    {
      ""id"": ""en-l11-gf-ext2"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Cấu trúc look forward to (Vừa)"",
      ""incorrectSentence"": ""We look forward to discuss potential collaboration opportunities with your team."",
      ""errorTypeHintVi"": ""Động từ sau cụm \""look forward to\"" phải ở dạng danh động từ (V-ing)."",
      ""correctedSentence"": ""We look forward to discussing potential collaboration opportunities with your team."",
      ""acceptedAnswers"": [
        ""We look forward to discussing potential collaboration opportunities with your team."",
        ""We are looking forward to discussing potential collaboration opportunities with your team.""
      ],
      ""meaningVi"": ""Chúng tôi rất mong được thảo luận về các cơ hội hợp tác tiềm năng với nhóm của bạn."",
      ""explanationVi"": ""Giới từ \""to\"" trong cụm \""look forward to\"" đòi hỏi theo sau là V-ing (\""discussing\"").""
    },
    {
      ""id"": ""en-l11-gf-ext3"",
      ""difficulty"": ""hard"",
      ""title"": ""Sửa lỗi: Đảo ngữ câu hỏi gián tiếp (Khó)"",
      ""incorrectSentence"": ""Could you tell me where is the keynote hall located?"",
      ""errorTypeHintVi"": ""Câu hỏi gián tiếp (Embedded question) không đảo trợ động từ/động từ to be lên trước chủ ngữ."",
      ""correctedSentence"": ""Could you tell me where the keynote hall is located?"",
      ""acceptedAnswers"": [
        ""Could you tell me where the keynote hall is located?"",
        ""Could you tell me where the keynote hall is?""
      ],
      ""meaningVi"": ""Bạn có thể cho tôi biết hội trường thuyết trình chính nằm ở đâu không?"",
      ""explanationVi"": ""Trong câu hỏi gián tiếp \""Could you tell me where...\"", trật tự từ là Subject + Verb (\""the keynote hall is located\"").""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.1 Roleplay (OrderIndex 18)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444446"), "roleplay", 18),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444446"),
            OrderIndex = 18,
            Type = "roleplay",
            Title = "Nhập vai hội thoại (Bài 1.1): Tình huống phản xạ thực chiến",
            Description = "Nhập vai giao tiếp với AI trong các tình huống thực tế, hỗ trợ chấm điểm tự động theo từ khóa và cấu trúc câu.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l11-rp-ext1"",
      ""difficulty"": ""easy"",
      ""title"": ""Bắt chuyện tại bàn ăn trưa (Dễ)"",
      ""scenarioVi"": ""Bạn đang ngồi một mình tại bàn ăn buffet của hội thảo, một kỹ sư người Singapore đến hỏi ngồi cùng."",
      ""aiRole"": ""David (Kỹ sư từ Singapore)"",
      ""aiPrompt"": ""Hi! Do you mind if I sit here? The dining hall is quite crowded today."",
      ""aiPromptVi"": ""Chào bạn! Bạn có phiền nếu tôi ngồi đây không? Phòng ăn hôm nay đông đúc quá."",
      ""userGoalVi"": ""Mời David ngồi nhiệt tình, đồng ý rằng phòng ăn rất đông và giới thiệu tên bạn."",
      ""expectedKeywords"": [
        ""please"",
        ""sit"",
        ""ahead"",
        ""crowded"",
        ""name""
      ],
      ""sampleResponses"": [
        ""Not at all, please go ahead and sit down! Yes, it really is crowded today. My name is Mai, nice to meet you.""
      ],
      ""minWords"": 7
    },
    {
      ""id"": ""en-l11-rp-ext2"",
      ""difficulty"": ""medium"",
      ""title"": ""Hỏi thăm về bài phát biểu (Vừa)"",
      ""scenarioVi"": ""David vừa ngồi xuống và hỏi cảm nhận của bạn về bài phát biểu khai mạc sáng nay."",
      ""aiRole"": ""David (Kỹ sư từ Singapore)"",
      ""aiPrompt"": ""Did you catch the keynote session on artificial intelligence this morning? What did you think of it?"",
      ""aiPromptVi"": ""Bạn có xem bài phát biểu khai mạc về trí tuệ nhân tạo sáng nay không? Bạn thấy nó thế nào?"",
      ""userGoalVi"": ""Xác nhận đã theo dõi, chia sẻ một điểm ấn tượng (inspiring / insightful) và hỏi ngược lại quan điểm của David."",
      ""expectedKeywords"": [
        ""keynote"",
        ""inspiring"",
        ""interesting"",
        ""think"",
        ""what about you""
      ],
      ""sampleResponses"": [
        ""Yes, I attended the keynote! I found the insights on scalability truly inspiring. What about you, did you enjoy the speaker?""
      ],
      ""minWords"": 10
    },
    {
      ""id"": ""en-l11-rp-ext3"",
      ""difficulty"": ""hard"",
      ""title"": ""Chủ động đề xuất kết nối hợp tác (Khó)"",
      ""scenarioVi"": ""Sau khi trao đổi về kỹ thuật, bạn thấy David có hướng nghiên cứu rất giống dự án của mình và muốn đề xuất hợp tác."",
      ""aiRole"": ""David (Kỹ sư từ Singapore)"",
      ""aiPrompt"": ""It sounds like our engineering challenges are remarkably aligned. We are also optimizing database latency."",
      ""aiPromptVi"": ""Có vẻ như các thách thức kỹ thuật của chúng ta tương đồng đáng kinh ngạc. Chúng tôi cũng đang tối ưu độ trễ cơ sở dữ liệu."",
      ""userGoalVi"": ""Đồng tình về sự tương đồng, đề xuất kết nối trên LinkedIn hoặc email để tiếp tục trao đổi sâu hơn về giải pháp."",
      ""expectedKeywords"": [
        ""aligned"",
        ""connect"",
        ""LinkedIn"",
        ""exchange"",
        ""collaborate"",
        ""stay in touch""
      ],
      ""sampleResponses"": [
        ""I completely agree! Our challenges align perfectly. Let's connect on LinkedIn and exchange emails so we can share technical solutions and explore future collaboration.""
      ],
      ""minWords"": 12
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.2 Shadowing (OrderIndex 11)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444447"), "shadowing", 11),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444447"),
            OrderIndex = 11,
            Type = "shadowing",
            Title = "Shadowing mở rộng (Bài 1.2): 15 câu đa cấp độ Dễ → Vừa → Khó",
            Description = "Luyện nói đuổi theo ngữ điệu người bản xứ, tập trung vào nối âm, ngữ điệu và phát âm chuẩn các âm then chốt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l12-shad-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi đường cơ bản (Dễ)"",
      ""sentence"": ""Excuse me, could you tell me where the subway station is?"",
      ""ipa"": ""/ɪkˈskjuːs miː, kəd juː tel miː weə ðə ˈsʌbweɪ ˈsteɪʃn ɪz/"",
      ""meaningVi"": ""Xin lỗi, bạn có thể chỉ giúp tôi ga tàu điện ngầm ở đâu không?"",
      ""keyPhrases"": [
        ""excuse me"",
        ""could you tell me"",
        ""subway station""
      ]
    },
    {
      ""id"": ""en-l12-shad-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Chỉ đường đi thẳng (Dễ)"",
      ""sentence"": ""Walk straight ahead for two blocks, then turn left."",
      ""ipa"": ""/wɔːk streɪt əˈhed fɔː tuː blɒks, ðen tɜːn left/"",
      ""meaningVi"": ""Đi thẳng hai dãy nhà nữa, sau đó rẽ trái."",
      ""keyPhrases"": [
        ""walk straight ahead"",
        ""two blocks"",
        ""turn left""
      ]
    },
    {
      ""id"": ""en-l12-shad-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi phương thức thanh toán (Dễ)"",
      ""sentence"": ""Do you accept credit cards or mobile payments here?"",
      ""ipa"": ""/duː juː əkˈsept ˈkredɪt kɑːdz ɔː ˈməʊbaɪl ˈpeɪmənts hɪə/"",
      ""meaningVi"": ""Ở đây có nhận thẻ tín dụng hoặc thanh toán di động không?"",
      ""keyPhrases"": [
        ""accept credit cards"",
        ""mobile payments"",
        ""here""
      ]
    },
    {
      ""id"": ""en-l12-shad-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Thử đồ mua sắm (Dễ)"",
      ""sentence"": ""Can I try this jacket on in the fitting room?"",
      ""ipa"": ""/kæn aɪ traɪ ðɪs ˈdʒækɪt ɒn ɪn ðə ˈfɪtɪŋ ruːm/"",
      ""meaningVi"": ""Tôi có thể thử chiếc áo khoác này trong phòng thay đồ được không?"",
      ""keyPhrases"": [
        ""try this jacket on"",
        ""fitting room""
      ]
    },
    {
      ""id"": ""en-l12-shad-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Khoảng cách đi bộ (Dễ)"",
      ""sentence"": ""The museum is only ten minutes away on foot."",
      ""ipa"": ""/ðə mjuːˈziːəm ɪz ˈəʊnli ten ˈmɪnɪts əˈweɪ ɒn fʊt/"",
      ""meaningVi"": ""Bảo tàng chỉ cách đây khoảng 10 phút đi bộ."",
      ""keyPhrases"": [
        ""ten minutes away"",
        ""on foot"",
        ""the museum""
      ]
    },
    {
      ""id"": ""en-l12-shad-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Chỉ dẫn bùng binh & giao lộ (Vừa)"",
      ""sentence"": ""Take the second exit at the roundabout, and you will see the hospital on your right."",
      ""ipa"": ""/teɪk ðə ˈsekənd ˈeɡzɪt æt ðə ˈraʊndəbaʊt, ænd juː wɪl siː ðə ˈhɒspɪtl ɒn jɔː raɪt/"",
      ""meaningVi"": ""Đi lối ra thứ hai ở bùng binh, và bạn sẽ thấy bệnh viện ở phía bên tay phải."",
      ""keyPhrases"": [
        ""second exit"",
        ""roundabout"",
        ""on your right""
      ]
    },
    {
      ""id"": ""en-l12-shad-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Hỏi chính sách đổi trả (Vừa)"",
      ""sentence"": ""Could you please explain your exchange policy if this item does not fit properly?"",
      ""ipa"": ""/kəd juː pliːz ɪkˈspleɪn jɔːr ɪksˈtʃeɪndʒ ˈpɒləsi ɪf ðɪs ˈaɪtəm dʌz nɒt fɪt ˈprɒpəli/"",
      ""meaningVi"": ""Bạn có thể giải thích chính sách đổi hàng nếu món đồ này mặc không vừa được không?"",
      ""keyPhrases"": [
        ""exchange policy"",
        ""does not fit properly"",
        ""could you please explain""
      ]
    },
    {
      ""id"": ""en-l12-shad-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Thủ tục hoàn thuế du lịch (Vừa)"",
      ""sentence"": ""Please provide the official tax invoice so that I can claim a refund at the airport."",
      ""ipa"": ""/pliːz prəˈvaɪd ði əˈfɪʃl tæks ˈɪnvɔɪs səʊ ðæt aɪ kæn kleɪm ə ˈriːfʌnd æt ði ˈeəpɔːt/"",
      ""meaningVi"": ""Làm ơn cung cấp hóa đơn thuế chính thức để tôi có thể làm thủ tục hoàn thuế tại sân bay."",
      ""keyPhrases"": [
        ""tax invoice"",
        ""claim a refund"",
        ""at the airport""
      ]
    },
    {
      ""id"": ""en-l12-shad-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Chuyển tuyến tàu điện (Vừa)"",
      ""sentence"": ""You need to transfer to the express line at the central interchange station."",
      ""ipa"": ""/juː niːd tuː trænsˈfɜː tuː ði ɪkˈspres laɪn æt ðə ˈsentrəl ˈɪntətʃeɪndʒ ˈsteɪʃn/"",
      ""meaningVi"": ""Bạn cần chuyển sang tuyến tàu nhanh tại nhà ga trung chuyển trung tâm."",
      ""keyPhrases"": [
        ""transfer to"",
        ""express line"",
        ""interchange station""
      ]
    },
    {
      ""id"": ""en-l12-shad-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Xác nhận giá ưu đãi (Vừa)"",
      ""sentence"": ""Is there any promotional discount available if we purchase multiple souvenirs together?"",
      ""ipa"": ""/ɪz ðeər ˈeni prəˈməʊʃənl ˈdɪskaʊnt əˈveɪləbl ɪf wiː ˈpɜːtʃəs ˈmʌltɪpl ˌsuːvəˈnɪəz təˈɡeðə/"",
      ""meaningVi"": ""Có chương trình chiết khấu khuyến mại nào nếu chúng tôi mua nhiều món quà lưu niệm cùng nhau không?"",
      ""keyPhrases"": [
        ""promotional discount"",
        ""purchase multiple souvenirs"",
        ""together""
      ]
    },
    {
      ""id"": ""en-l12-shad-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Xử lý sự cố hành lý thất lạc (Khó)"",
      ""sentence"": ""My checked luggage failed to arrive on the conveyor belt, so I must file an immediate irregularity report."",
      ""ipa"": ""/maɪ tʃekt ˈlʌɡɪdʒ feɪld tuː əˈraɪv ɒn ðə kənˈveɪə belt, səʊ aɪ mʌst faɪl ən ɪˈmiːdiət ɪˌreɡjʊˈlærəti rɪˈpɔːt/"",
      ""meaningVi"": ""Hành lý ký gửi của tôi không xuất hiện trên băng chuyền, nên tôi phải nộp ngay biên bản bất thường."",
      ""keyPhrases"": [
        ""checked luggage"",
        ""conveyor belt"",
        ""irregularity report""
      ]
    },
    {
      ""id"": ""en-l12-shad-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Hỏi thông tin kỹ thuật sản phẩm (Khó)"",
      ""sentence"": ""Could you clarify whether this electronic gadget operates across international voltage standards without an adapter?"",
      ""ipa"": ""/kəd juː ˈklærəfaɪ ˈweðə ðɪs ɪˌlekˈtrɒnɪk ˈɡædʒɪt ˈɒpəreɪts əˈkrɒs ˌɪntəˈnæʃnəl ˈvɒltɪdʒ ˈstændədz wɪðˈaʊt ən əˈdæptə/"",
      ""meaningVi"": ""Bạn có thể làm rõ liệu thiết bị điện tử này có hoạt động theo các tiêu chuẩn điện áp quốc tế mà không cần cục chuyển đổi không?"",
      ""keyPhrases"": [
        ""electronic gadget"",
        ""voltage standards"",
        ""without an adapter""
      ]
    },
    {
      ""id"": ""en-l12-shad-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Lộ trình đường vòng do thi công (Khó)"",
      ""sentence"": ""Due to ongoing downtown construction, pedestrians must follow the marked detour to access the subway entrance."",
      ""ipa"": ""/djuː tuː ˈɒnˌɡəʊɪŋ ˈdaʊntaʊn kənˈstrʌkʃn, pəˈdestriənz mʌst ˈfɒləʊ ðə mɑːkt ˈdiːtʊə tuː ˈækses ðə ˈsʌbweɪ ˈentrəns/"",
      ""meaningVi"": ""Do công trình đang thi công ở trung tâm, người đi bộ phải đi theo đường vòng có biển chỉ dẫn để vào lối ga tàu điện."",
      ""keyPhrases"": [
        ""ongoing construction"",
        ""marked detour"",
        ""subway entrance""
      ]
    },
    {
      ""id"": ""en-l12-shad-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Bảo hiểm và bồi hoàn chuyến đi (Khó)"",
      ""sentence"": ""If the transit cancellation disrupts our schedule, does the carrier provide complimentary lodging and compensation vouchers?"",
      ""ipa"": ""/ɪf ðə ˈtrænzɪt ˌkænsəˈleɪʃn dɪsˈrʌpts ˈaʊə ˈʃedjuːl, dʌz ðə ˈkæriə prəˈvaɪd ˌkɒmplɪˈmentri ˈlɒdʒɪŋ ænd ˌkɒmpenˈseɪʃn ˈvaʊtʃəz/"",
      ""meaningVi"": ""Nếu việc hủy chuyến làm gián đoạn lịch trình, hãng vận chuyển có cung cấp chỗ ở miễn phí và phiếu bồi thường không?"",
      ""keyPhrases"": [
        ""transit cancellation"",
        ""complimentary lodging"",
        ""compensation vouchers""
      ]
    },
    {
      ""id"": ""en-l12-shad-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Đàm phán dịch vụ thuê xe du lịch (Khó)"",
      ""sentence"": ""We would like to negotiate comprehensive insurance coverage that safeguards against accidental collision and theft liability."",
      ""ipa"": ""/wiː wʊd laɪk tuː nɪˈɡəʊʃieɪt ˌkɒmprɪˈhensɪv ɪnˈʃʊərəns ˈkʌvərɪdʒ ðæt ˈseɪfɡɑːdz əˈɡenst ˌæksɪˈdentl kəˈlɪʒn ænd θeft ˌlaɪəˈbɪləti/"",
      ""meaningVi"": ""Chúng tôi muốn thương lượng gói bảo hiểm toàn diện nhằm phòng ngừa rủi ro va chạm tai nạn và trách nhiệm trộm cắp."",
      ""keyPhrases"": [
        ""comprehensive insurance"",
        ""safeguards against"",
        ""collision and theft liability""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.2 Dictation (OrderIndex 12)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444447"), "dictation", 12),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444447"),
            OrderIndex = 12,
            Type = "dictation",
            Title = "Dictation mở rộng (Bài 1.2): 15 câu nghe chép chính tả phản xạ",
            Description = "Nghe câu hội thoại đa cấp độ và gõ lại chính xác từng từ hoặc chọn word chip để rèn luyện thính giác tiếng Anh.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l12-dict-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi trạm xe bus (Dễ)"",
      ""sentence"": ""Where is the nearest bus stop to the city center?"",
      ""ipa"": ""/weər ɪz ðə ˈnɪərɪst bʌs stɒp tuː ðə ˈsɪti ˈsentə/"",
      ""meaningVi"": ""Trạm xe bus gần nhất đi vào trung tâm thành phố ở đâu?"",
      ""wordChips"": [
        ""Where"",
        ""is"",
        ""the"",
        ""nearest"",
        ""bus"",
        ""stop"",
        ""to"",
        ""the"",
        ""city"",
        ""center""
      ],
      ""distractors"": [
        ""station"",
        ""far"",
        ""there"",
        ""which""
      ]
    },
    {
      ""id"": ""en-l12-dict-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi giá tiền (Dễ)"",
      ""sentence"": ""How much does this handmade souvenir cost?"",
      ""ipa"": ""/haʊ mʌtʃ dʌz ðɪs ˌhændˈmeɪd ˌsuːvəˈnɪə kɒst/"",
      ""meaningVi"": ""Món quà lưu niệm thủ công này có giá bao nhiêu?"",
      ""wordChips"": [
        ""How"",
        ""much"",
        ""does"",
        ""this"",
        ""handmade"",
        ""souvenir"",
        ""cost""
      ],
      ""distractors"": [
        ""many"",
        ""price"",
        ""is"",
        ""expensive""
      ]
    },
    {
      ""id"": ""en-l12-dict-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Chỉ đường rẽ phải (Dễ)"",
      ""sentence"": ""Turn right at the traffic lights and walk along the street."",
      ""ipa"": ""/tɜːn raɪt æt ðə ˈtræfɪk laɪts ænd wɔːk əˈlɒŋ ðə striːt/"",
      ""meaningVi"": ""Rẽ phải ở đèn giao thông và đi dọc theo con phố."",
      ""wordChips"": [
        ""Turn"",
        ""right"",
        ""at"",
        ""the"",
        ""traffic"",
        ""lights"",
        ""and"",
        ""walk"",
        ""along"",
        ""the"",
        ""street""
      ],
      ""distractors"": [
        ""left"",
        ""straight"",
        ""stop"",
        ""avenue""
      ]
    },
    {
      ""id"": ""en-l12-dict-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi lấy biên lai (Dễ)"",
      ""sentence"": ""Could I please have a printed receipt for my purchase?"",
      ""ipa"": ""/kəd aɪ pliːz hæv ə ˈprɪntɪd rɪˈsiːt fɔː maɪ ˈpɜːtʃəs/"",
      ""meaningVi"": ""Làm ơn cho tôi xin biên lai in cho đơn hàng này được không?"",
      ""wordChips"": [
        ""Could"",
        ""I"",
        ""please"",
        ""have"",
        ""a"",
        ""printed"",
        ""receipt"",
        ""for"",
        ""my"",
        ""purchase""
      ],
      ""distractors"": [
        ""can"",
        ""give"",
        ""paper"",
        ""bill""
      ]
    },
    {
      ""id"": ""en-l12-dict-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Phương tiện giao thông (Dễ)"",
      ""sentence"": ""The train leaves every fifteen minutes from track four."",
      ""ipa"": ""/ðə treɪn liːvz ˈevri ˌfɪfˈtiːn ˈmɪnɪts frɒm træk fɔː/"",
      ""meaningVi"": ""Tàu hỏa khởi hành cứ mỗi 15 phút một chuyến từ đường ray số 4."",
      ""wordChips"": [
        ""The"",
        ""train"",
        ""leaves"",
        ""every"",
        ""fifteen"",
        ""minutes"",
        ""from"",
        ""track"",
        ""four""
      ],
      ""distractors"": [
        ""goes"",
        ""depart"",
        ""platform"",
        ""each""
      ]
    },
    {
      ""id"": ""en-l12-dict-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Thanh toán không tiếp xúc (Vừa)"",
      ""sentence"": ""Tap your contactless payment card against the electronic reader to enter."",
      ""ipa"": ""/tæp jɔː ˈkɒntæktləs ˈpeɪmənt kɑːd əˈɡenst ði ˌɪlekˈtrɒnɪk ˈriːdə tuː ˈentə/"",
      ""meaningVi"": ""Chạm thẻ thanh toán không tiếp xúc của bạn vào đầu đọc điện tử để vào cửa."",
      ""wordChips"": [
        ""Tap"",
        ""your"",
        ""contactless"",
        ""payment"",
        ""card"",
        ""against"",
        ""the"",
        ""electronic"",
        ""reader"",
        ""to"",
        ""enter""
      ],
      ""distractors"": [
        ""touch"",
        ""device"",
        ""scanner"",
        ""pass""
      ]
    },
    {
      ""id"": ""en-l12-dict-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Chính sách hoàn tiền (Vừa)"",
      ""sentence"": ""Items purchased during clearance sales are not eligible for cash refunds."",
      ""ipa"": ""/ˈaɪtəmz ˈpɜːtʃəst ˈdjʊərɪŋ ˈklɪərəns seɪlz ɑː nɒt ˈelɪdʒəbl fɔː kæʃ ˈriːfʌndz/"",
      ""meaningVi"": ""Các sản phẩm mua trong đợt xả hàng không đủ điều kiện để hoàn lại tiền mặt."",
      ""wordChips"": [
        ""Items"",
        ""purchased"",
        ""during"",
        ""clearance"",
        ""sales"",
        ""are"",
        ""not"",
        ""eligible"",
        ""for"",
        ""cash"",
        ""refunds""
      ],
      ""distractors"": [
        ""goods"",
        ""bought"",
        ""allowed"",
        ""return""
      ]
    },
    {
      ""id"": ""en-l12-dict-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Chỉ đường bằng mốc địa danh (Vừa)"",
      ""sentence"": ""Continue straight past the cathedral until you reach the pedestrian shopping plaza."",
      ""ipa"": ""/kənˈtɪnjuː streɪt pɑːst ðə kəˈθiːdrəl ənˈtɪl juː riːtʃ ðə pəˈdestriən ˈʃɒpɪŋ ˈplɑːzə/"",
      ""meaningVi"": ""Tiếp tục đi thẳng qua nhà thờ lớn cho đến khi bạn tới quảng trường mua sắm đi bộ."",
      ""wordChips"": [
        ""Continue"",
        ""straight"",
        ""past"",
        ""the"",
        ""cathedral"",
        ""until"",
        ""you"",
        ""reach"",
        ""the"",
        ""pedestrian"",
        ""shopping"",
        ""plaza""
      ],
      ""distractors"": [
        ""go"",
        ""church"",
        ""mall"",
        ""market""
      ]
    },
    {
      ""id"": ""en-l12-dict-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Mua vé đi lại nhiều ngày (Vừa)"",
      ""sentence"": ""A three-day unlimited travel pass offers significant savings for international travelers."",
      ""ipa"": ""/ə θriː deɪ ʌnˈlɪmɪtɪd ˈtrævl pɑːs ˈɒfəz sɪɡˈnɪfɪkənt ˈseɪvɪŋz fɔːr ˌɪntəˈnæʃnəl ˈtrævələz/"",
      ""meaningVi"": ""Thẻ đi lại không giới hạn 3 ngày giúp tiết kiệm chi phí đáng kể cho du khách quốc tế."",
      ""wordChips"": [
        ""A"",
        ""three-day"",
        ""unlimited"",
        ""travel"",
        ""pass"",
        ""offers"",
        ""significant"",
        ""savings"",
        ""for"",
        ""international"",
        ""travelers""
      ],
      ""distractors"": [
        ""ticket"",
        ""provides"",
        ""huge"",
        ""tourists""
      ]
    },
    {
      ""id"": ""en-l12-dict-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Kiểm tra hàng tồn kho (Vừa)"",
      ""sentence"": ""The store assistant gladly checked the backroom storage for the requested shoe size."",
      ""ipa"": ""/ðə stɔːr əˈsɪstənt ˈɡlædli tʃekt ðə ˈbækruːm ˈstɔːrɪdʒ fɔː ðə rɪˈkwestɪd ʃuː saɪz/"",
      ""meaningVi"": ""Nhân viên bán hàng đã vui vẻ kiểm tra kho sau để tìm kích cỡ giày được yêu cầu."",
      ""wordChips"": [
        ""The"",
        ""store"",
        ""assistant"",
        ""gladly"",
        ""checked"",
        ""the"",
        ""backroom"",
        ""storage"",
        ""for"",
        ""the"",
        ""requested"",
        ""shoe"",
        ""size""
      ],
      ""distractors"": [
        ""clerk"",
        ""warehouse"",
        ""searched"",
        ""model""
      ]
    },
    {
      ""id"": ""en-l12-dict-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Thủ tục khiếu nại quá cảnh (Khó)"",
      ""sentence"": ""Unforeseen flight cancellations entitle passengers to verified accommodation vouchers and rerouted itineraries."",
      ""ipa"": ""/ˌʌnfɔːˈsiːn flaɪt ˌkænsəˈleɪʃnz ɪnˈtaɪtl ˈpæsɪndʒəz tuː ˈverɪfaɪd əˌkɒməˈdeɪʃn ˈvaʊtʃəz ænd ˌriːˈruːtɪd aɪˈtɪnərəriz/"",
      ""meaningVi"": ""Các sự cố hủy chuyến bay bất khả kháng trao quyền cho hành khách nhận phiếu khách sạn hợp lệ và hành trình thay thế."",
      ""wordChips"": [
        ""Unforeseen"",
        ""flight"",
        ""cancellations"",
        ""entitle"",
        ""passengers"",
        ""to"",
        ""verified"",
        ""accommodation"",
        ""vouchers"",
        ""and"",
        ""rerouted"",
        ""itineraries""
      ],
      ""distractors"": [
        ""unexpected"",
        ""travelers"",
        ""hotel"",
        ""tickets""
      ]
    },
    {
      ""id"": ""en-l12-dict-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Quy định hải quan và thuế quan (Khó)"",
      ""sentence"": ""Tourists must declare merchandise exceeding personal statutory allowances upon arrival at customs checkpoints."",
      ""ipa"": ""/ˈtʊərɪsts mʌst dɪˈkleə ˈmɜːtʃəndaɪs ɪkˈsiːdɪŋ ˈpɜːsənl ˈstætʃʊtri əˈlaʊənsɪz əˈpɒn əˈraɪvl æt ˈkʌstəmz ˈtʃekpɔɪnts/"",
      ""meaningVi"": ""Du khách phải khai báo hàng hóa vượt định mức miễn thuế cá nhân theo luật khi đến các trạm kiểm soát hải quan."",
      ""wordChips"": [
        ""Tourists"",
        ""must"",
        ""declare"",
        ""merchandise"",
        ""exceeding"",
        ""personal"",
        ""statutory"",
        ""allowances"",
        ""upon"",
        ""arrival"",
        ""at"",
        ""customs"",
        ""checkpoints""
      ],
      ""distractors"": [
        ""visitors"",
        ""report"",
        ""goods"",
        ""limits""
      ]
    },
    {
      ""id"": ""en-l12-dict-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Hợp đồng thuê xe quốc tế (Khó)"",
      ""sentence"": ""Reviewing rental agreements diligently ensures that deductible charges and mileage limitations are thoroughly understood."",
      ""ipa"": ""/rɪˈvjuːɪŋ ˈrentl əˈɡriːmənts ˈdɪlɪdʒəntli ɪnˈʃʊəz ðæt dɪˈdʌktəbl ˈtʃɑːdʒɪz ænd ˈmaɪlɪdʒ ˌlɪmɪˈteɪʃnz ɑː ˈθʌrəli ˌʌndəˈstʊd/"",
      ""meaningVi"": ""Kiểm tra kỹ hợp đồng thuê xe đảm bảo rằng các khoản phí khấu trừ và giới hạn quãng đường được thấu hiểu tường tận."",
      ""wordChips"": [
        ""Reviewing"",
        ""rental"",
        ""agreements"",
        ""diligently"",
        ""ensures"",
        ""that"",
        ""deductible"",
        ""charges"",
        ""and"",
        ""mileage"",
        ""limitations"",
        ""are"",
        ""thoroughly"",
        ""understood""
      ],
      ""distractors"": [
        ""checking"",
        ""contracts"",
        ""costs"",
        ""distance""
      ]
    },
    {
      ""id"": ""en-l12-dict-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Định vị trong nhà ga phức hợp (Khó)"",
      ""sentence"": ""Subterranean transit hubs utilize color-coded interactive wayfinding kiosks to guide disoriented commuters."",
      ""ipa"": ""/ˌsʌbtəˈreɪniən ˈtrænzɪt hʌbz ˈjuːtəlaɪz ˈkʌlə kəʊdɪd ˌɪntərˈæktɪv ˈweɪˌfaɪndɪŋ ˈkiːɒsks tuː ɡaɪd dɪsˈɔːrientɪd kəˈmjuːtəz/"",
      ""meaningVi"": ""Các trung tâm trung chuyển ngầm sử dụng các trạm chỉ đường tương tác mã hóa màu sắc để hướng dẫn hành khách bị lạc hướng."",
      ""wordChips"": [
        ""Subterranean"",
        ""transit"",
        ""hubs"",
        ""utilize"",
        ""color-coded"",
        ""interactive"",
        ""wayfinding"",
        ""kiosks"",
        ""to"",
        ""guide"",
        ""disoriented"",
        ""commuters""
      ],
      ""distractors"": [
        ""underground"",
        ""stations"",
        ""screens"",
        ""passengers""
      ]
    },
    {
      ""id"": ""en-l12-dict-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Xử lý phát sinh thanh toán quốc tế (Khó)"",
      ""sentence"": ""Foreign currency conversion surcharges can be minimized by selecting billing in local jurisdiction currency."",
      ""ipa"": ""/ˈfɒrən ˈkʌrənsi kənˈvɜːʃn ˈsɜːtʃɑːdʒɪz kæn biː ˈmɪnɪmaɪzd baɪ sɪˈlektɪŋ ˈbɪlɪŋ ɪn ˈləʊkl ˌdʒʊərɪsˈdɪkʃn ˈkʌrənsi/"",
      ""meaningVi"": ""Phụ phí chuyển đổi ngoại tệ có thể được giảm thiểu bằng cách chọn thanh toán theo đồng tiền của nước sở tại."",
      ""wordChips"": [
        ""Foreign"",
        ""currency"",
        ""conversion"",
        ""surcharges"",
        ""can"",
        ""be"",
        ""minimized"",
        ""by"",
        ""selecting"",
        ""billing"",
        ""in"",
        ""local"",
        ""jurisdiction"",
        ""currency""
      ],
      ""distractors"": [
        ""exchange"",
        ""fees"",
        ""reduced"",
        ""payment""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.2 Pronunciation (OrderIndex 13)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444447"), "pronunciation", 13),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444447"),
            OrderIndex = 13,
            Type = "pronunciation",
            Title = "Chấm phát âm mở rộng (Bài 1.2): 15 câu luyện âm /θ/-/ð/, âm cuối và đuôi -s/-ed",
            Description = "Luyện phát âm với AI chấm điểm trực tiếp, phát hiện lỗi sai âm đuôi và các âm khó phổ biến của người Việt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l12-pron-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm đầu /st/ trong station (Dễ)"",
      ""sentence"": ""Please stay near the central station bus stop."",
      ""ipa"": ""/pliːz steɪ nɪə ðə ˈsentrəl ˈsteɪʃn bʌs stɒp/"",
      ""meaningVi"": ""Làm ơn đứng gần trạm xe bus của nhà ga trung tâm."",
      ""targetWords"": [
        ""stay"",
        ""station"",
        ""stop""
      ],
      ""keyPhrases"": [
        ""near central station"",
        ""bus stop""
      ]
    },
    {
      ""id"": ""en-l12-pron-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /str/ trong straight & street (Dễ)"",
      ""sentence"": ""Walk straight along this wide commercial street."",
      ""ipa"": ""/wɔːk streɪt əˈlɒŋ ðɪs waɪd kəˈmɜːʃl striːt/"",
      ""meaningVi"": ""Đi thẳng dọc theo con phố thương mại rộng lớn này."",
      ""targetWords"": [
        ""straight"",
        ""street""
      ],
      ""keyPhrases"": [
        ""walk straight"",
        ""commercial street""
      ]
    },
    {
      ""id"": ""en-l12-pron-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm cuối /tʃ/ trong touch & purchase (Dễ)"",
      ""sentence"": ""You can touch the screen to confirm your purchase."",
      ""ipa"": ""/juː kæn tʌtʃ ðə skriːn tuː kənˈfɜːm jɔː ˈpɜːtʃəs/"",
      ""meaningVi"": ""Bạn có thể chạm vào màn hình để xác nhận giao dịch mua của mình."",
      ""targetWords"": [
        ""touch"",
        ""screen"",
        ""purchase""
      ],
      ""keyPhrases"": [
        ""touch the screen"",
        ""confirm purchase""
      ]
    },
    {
      ""id"": ""en-l12-pron-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm cuối /dʒ/ trong luggage & bridge (Dễ)"",
      ""sentence"": ""Carry your luggage across the pedestrian bridge."",
      ""ipa"": ""/ˈkæri jɔː ˈlʌɡɪdʒ əˈkrɒs ðə pəˈdestriən brɪdʒ/"",
      ""meaningVi"": ""Mang hành lý của bạn qua cây cầu dành cho người đi bộ."",
      ""targetWords"": [
        ""luggage"",
        ""bridge""
      ],
      ""keyPhrases"": [
        ""carry luggage"",
        ""pedestrian bridge""
      ]
    },
    {
      ""id"": ""en-l12-pron-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm đuôi -s/es trong blocks & prices (Dễ)"",
      ""sentence"": ""Compare the prices across these shopping blocks."",
      ""ipa"": ""/kəmˈpeə ðə ˈpraɪsɪz əˈkrɒs ðiːz ˈʃɒpɪŋ blɒks/"",
      ""meaningVi"": ""So sánh mức giá dọc theo các dãy nhà mua sắm này."",
      ""targetWords"": [
        ""prices"",
        ""these"",
        ""blocks""
      ],
      ""keyPhrases"": [
        ""compare prices"",
        ""shopping blocks""
      ]
    },
    {
      ""id"": ""en-l12-pron-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /sk/ trong excuse & schedule (Vừa)"",
      ""sentence"": ""Excuse me, could you check the train schedule on screen?"",
      ""ipa"": ""/ɪkˈskjuːs miː, kəd juː tʃek ðə treɪn ˈʃedjuːl ɒn skriːn/"",
      ""meaningVi"": ""Xin lỗi, bạn có thể kiểm tra lịch trình tàu trên màn hình giúp tôi không?"",
      ""targetWords"": [
        ""excuse"",
        ""schedule"",
        ""screen""
      ],
      ""keyPhrases"": [
        ""excuse me"",
        ""train schedule""
      ]
    },
    {
      ""id"": ""en-l12-pron-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /ð/ trong clothes & leather (Vừa)"",
      ""sentence"": ""These leather clothes withstand rainy weather beautifully."",
      ""ipa"": ""/ðiːz ˈleðə kləʊðz wɪðˈstænd ˈreɪni ˈweðə ˈbjuːtɪfli/"",
      ""meaningVi"": ""Những bộ quần áo da này chống chịu thời tiết mưa gió rất tốt."",
      ""targetWords"": [
        ""these"",
        ""leather"",
        ""clothes"",
        ""withstand"",
        ""weather""
      ],
      ""keyPhrases"": [
        ""leather clothes"",
        ""withstand weather""
      ]
    },
    {
      ""id"": ""en-l12-pron-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm cuối phức hợp /kts/ trong products (Vừa)"",
      ""sentence"": ""Our local craftsmen protect authentic handmade products."",
      ""ipa"": ""/ˈaʊə ˈləʊkl ˈkrɑːftsmən prəˈtekt ɔːˈθentɪk ˌhændˈmeɪd ˈprɒdʌkts/"",
      ""meaningVi"": ""Các nghệ nhân địa phương của chúng tôi bảo tồn những sản phẩm thủ công đích thực."",
      ""targetWords"": [
        ""craftsmen"",
        ""protect"",
        ""authentic"",
        ""products""
      ],
      ""keyPhrases"": [
        ""protect authentic"",
        ""handmade products""
      ]
    },
    {
      ""id"": ""en-l12-pron-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đuôi -ed dạng /t/ trong searched & checked (Vừa)"",
      ""sentence"": ""The clerk searched the inventory and checked every shelf."",
      ""ipa"": ""/ðə klɑːk sɜːtʃt ði ˈɪnvəntri ænd tʃekt ˈevri ʃelf/"",
      ""meaningVi"": ""Nhân viên đã tìm kiếm kho hàng và kiểm tra từng kệ đồ."",
      ""targetWords"": [
        ""searched"",
        ""checked"",
        ""shelf""
      ],
      ""keyPhrases"": [
        ""searched inventory"",
        ""checked shelf""
      ]
    },
    {
      ""id"": ""en-l12-pron-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Nối âm trong contactless card (Vừa)"",
      ""sentence"": ""Hold your contactless card flat against the reader."",
      ""ipa"": ""/həʊld jɔː ˈkɒntæktləs kɑːd flæt əˈɡenst ðə ˈriːdə/"",
      ""meaningVi"": ""Giữ thẻ thanh toán chạm của bạn nằm phẳng trước đầu đọc thẻ."",
      ""targetWords"": [
        ""contactless"",
        ""card"",
        ""against"",
        ""reader""
      ],
      ""keyPhrases"": [
        ""contactless card"",
        ""against reader""
      ]
    },
    {
      ""id"": ""en-l12-pron-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Trọng âm từ & âm /dʒ/ trong jurisdiction (Khó)"",
      ""sentence"": ""Jurisdictions rigorously legislate transit safety and congestion surcharges."",
      ""ipa"": ""/ˌdʒʊərɪsˈdɪkʃnz ˈrɪɡərəsli ˈledʒɪsleɪt ˈtrænzɪt ˈseɪfti ænd kənˈdʒestʃən ˈsɜːtʃɑːdʒɪz/"",
      ""meaningVi"": ""Các cơ quan quản lý lập pháp chặt chẽ về an toàn giao thông và các khoản phụ phí tắc nghẽn."",
      ""targetWords"": [
        ""jurisdictions"",
        ""legislate"",
        ""transit"",
        ""congestion"",
        ""surcharges""
      ],
      ""keyPhrases"": [
        ""rigorously legislate"",
        ""congestion surcharges""
      ]
    },
    {
      ""id"": ""en-l12-pron-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cụm phụ âm /ks/ & /θ/ trong sixth avenue (Khó)"",
      ""sentence"": ""Navigate past the sixth thoroughfare to find the authentic craft workshop."",
      ""ipa"": ""/ˈnævɪɡeɪt pɑːst ðə sɪksθ ˈθʌrəfeə tuː faɪnd ði ɔːˈθentɪk krɑːft ˈwɜːkʃɒp/"",
      ""meaningVi"": ""Hãy định vị vượt qua đại lộ thứ sáu để tìm xưởng thủ công mỹ nghệ chuẩn mực."",
      ""targetWords"": [
        ""navigate"",
        ""sixth"",
        ""thoroughfare"",
        ""authentic"",
        ""workshop""
      ],
      ""keyPhrases"": [
        ""sixth thoroughfare"",
        ""authentic craft workshop""
      ]
    },
    {
      ""id"": ""en-l12-pron-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Âm /v/ vs /w/ & /θ/ trong worthwhile vouchers (Khó)"",
      ""sentence"": ""Travelers with valid vouchers receive worthwhile transit accommodations."",
      ""ipa"": ""/ˈtrævələz wɪð ˈvælɪd ˈvaʊtʃəz rɪˈsiːv ˈwɜːθwaɪl ˈtrænzɪt əˌkɒməˈdeɪʃnz/"",
      ""meaningVi"": ""Những hành khách có phiếu hợp lệ sẽ nhận được chỗ ở quá cảnh rất xứng đáng."",
      ""targetWords"": [
        ""travelers"",
        ""valid"",
        ""vouchers"",
        ""worthwhile"",
        ""accommodations""
      ],
      ""keyPhrases"": [
        ""valid vouchers"",
        ""worthwhile accommodations""
      ]
    },
    {
      ""id"": ""en-l12-pron-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Âm đuôi /pt/ & /kt/ trong disrupted contactless (Khó)"",
      ""sentence"": ""Unexpected system disruptions affected contactless transactions throughout the terminal."",
      ""ipa"": ""/ˌʌnɪkˈspektɪd ˈsɪstəm dɪsˈrʌpʃnz əˈfektɪd ˈkɒntæktləs trænˈzækʃnz θruːˈaʊt ðə ˈtɜːmɪnl/"",
      ""meaningVi"": ""Sự cố gián đoạn hệ thống bất ngờ đã ảnh hưởng đến các giao dịch chạm thẻ trong toàn nhà ga."",
      ""targetWords"": [
        ""unexpected"",
        ""disruptions"",
        ""contactless"",
        ""transactions"",
        ""terminal""
      ],
      ""keyPhrases"": [
        ""system disruptions"",
        ""contactless transactions""
      ]
    },
    {
      ""id"": ""en-l12-pron-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Ngữ điệu câu điều kiện & âm /ʃ/ trong merchandise (Khó)"",
      ""sentence"": ""Purchasing perishable merchandise requires meticulous inspection of expiration credentials."",
      ""ipa"": ""/ˈpɜːtʃəsɪŋ ˈperɪʃəbl ˈmɜːtʃəndaɪs rɪˈkwaɪəz mɪˈtɪkjələs ɪnˈspekʃn əv ˌekspəˈreɪʃn krəˈdenʃlz/"",
      ""meaningVi"": ""Việc mua sắm hàng hóa mau hỏng đòi hỏi sự kiểm tra tỉ mỉ về hạn sử dụng."",
      ""targetWords"": [
        ""purchasing"",
        ""perishable"",
        ""merchandise"",
        ""meticulous"",
        ""inspection"",
        ""credentials""
      ],
      ""keyPhrases"": [
        ""perishable merchandise"",
        ""meticulous inspection""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.2 Word Map (OrderIndex 14)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444447"), "word_map", 14),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444447"),
            OrderIndex = 14,
            Type = "word_map",
            Title = "Sơ đồ từ vựng Mind Map mở rộng (Bài 1.2): 12 sơ đồ Dễ → Vừa → Khó",
            Description = "Hệ thống sơ đồ tư duy từ vựng đa cấp độ kèm phiên âm IPA, collocations, phrasal verbs, synonyms và ví dụ ngữ cảnh thực chiến.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""wm-l12-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: direction"",
      ""centralWord"": ""direction"",
      ""ipa"": ""/daɪˈrekʃn/"",
      ""meaningVi"": ""Phương hướng, sự chỉ đường"",
      ""collocations"": [
        {
          ""text"": ""ask for directions"",
          ""ipa"": ""/ɑːsk fɔː daɪˈrekʃnz/"",
          ""meaningVi"": ""Hỏi đường"",
          ""example"": ""Never hesitate to ask for directions when lost.""
        },
        {
          ""text"": ""follow directions"",
          ""ipa"": ""/ˈfɒləʊ daɪˈrekʃnz/"",
          ""meaningVi"": ""Đi theo chỉ dẫn"",
          ""example"": ""Follow the map directions carefully.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""head for"",
          ""ipa"": ""/hed fɔː/"",
          ""meaningVi"": ""Đi về hướng"",
          ""example"": ""Head for the tall tower across the river.""
        },
        {
          ""text"": ""turn into"",
          ""ipa"": ""/tɜːn ˈɪntuː/"",
          ""meaningVi"": ""Rẽ vào"",
          ""example"": ""Turn into the shaded pedestrian alley.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""guidance"",
          ""ipa"": ""/ˈɡaɪdns/"",
          ""meaningVi"": ""Sự hướng dẫn"",
          ""example"": ""The station attendant provided helpful guidance.""
        },
        {
          ""text"": ""orientation"",
          ""ipa"": ""/ˌɔːriənˈteɪʃn/"",
          ""meaningVi"": ""Định hướng"",
          ""example"": ""Check your compass for spatial orientation.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Clear directions save valuable travel time in unfamiliar cities."",
          ""meaningVi"": ""Chỉ dẫn đường rõ ràng giúp tiết kiệm thời gian quý giá ở các thành phố lạ.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: payment"",
      ""centralWord"": ""payment"",
      ""ipa"": ""/ˈpeɪmənt/"",
      ""meaningVi"": ""Sự thanh toán, tiền chi trả"",
      ""collocations"": [
        {
          ""text"": ""contactless payment"",
          ""ipa"": ""/ˈkɒntæktləs ˈpeɪmənt/"",
          ""meaningVi"": ""Thanh toán không tiếp xúc"",
          ""example"": ""Contactless payment is available at all checkout counters.""
        },
        {
          ""text"": ""make a payment"",
          ""ipa"": ""/meɪk ə ˈpeɪmənt/"",
          ""meaningVi"": ""Thực hiện thanh toán"",
          ""example"": ""You can make a payment via smart wallet.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""pay for"",
          ""ipa"": ""/peɪ fɔː/"",
          ""meaningVi"": ""Thanh toán cho"",
          ""example"": ""Who will pay for the public transit tickets?""
        },
        {
          ""text"": ""settle up"",
          ""ipa"": ""/ˈsetl ʌp/"",
          ""meaningVi"": ""Thanh toán sòng phẳng"",
          ""example"": ""Let's settle up before we leave the restaurant.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""settlement"",
          ""ipa"": ""/ˈsetlmənt/"",
          ""meaningVi"": ""Sự thanh quyết toán"",
          ""example"": ""Prompt settlement of bills avoids penalties.""
        },
        {
          ""text"": ""remittance"",
          ""ipa"": ""/rɪˈmɪtns/"",
          ""meaningVi"": ""Tiền gửi thanh toán"",
          ""example"": ""Foreign remittance arrives within hours.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Digital payment methods offer unparalleled convenience during foreign travel."",
          ""meaningVi"": ""Các phương thức thanh toán số mang lại sự tiện lợi vượt trội khi đi du lịch nước ngoài.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: station"",
      ""centralWord"": ""station"",
      ""ipa"": ""/ˈsteɪʃn/"",
      ""meaningVi"": ""Nhà ga, trạm trung chuyển"",
      ""collocations"": [
        {
          ""text"": ""subway station"",
          ""ipa"": ""/ˈsʌbweɪ ˈsteɪʃn/"",
          ""meaningVi"": ""Ga tàu điện ngầm"",
          ""example"": ""The subway station is located right beneath the plaza.""
        },
        {
          ""text"": ""central station"",
          ""ipa"": ""/ˈsentrəl ˈsteɪʃn/"",
          ""meaningVi"": ""Ga trung tâm"",
          ""example"": ""All intercity rail lines terminate at the central station.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""stop at"",
          ""ipa"": ""/stɒp æt/"",
          ""meaningVi"": ""Dừng lại ở"",
          ""example"": ""Does this express train stop at the terminal?""
        },
        {
          ""text"": ""pull into"",
          ""ipa"": ""/pʊl ˈɪntuː/"",
          ""meaningVi"": ""Tiến vào sân ga"",
          ""example"": ""The high-speed train pulled into platform three.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""terminal"",
          ""ipa"": ""/ˈtɜːmɪnl/"",
          ""meaningVi"": ""Nhà ga cuối"",
          ""example"": ""The airport terminal was gleaming with modern glass.""
        },
        {
          ""text"": ""depot"",
          ""ipa"": ""/ˈdepəʊ/"",
          ""meaningVi"": ""Trạm điều vận xe"",
          ""example"": ""Buses return to the depot after midnight.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Navigating the bustling central station requires following the illuminated overhead signs."",
          ""meaningVi"": ""Tìm đường trong ga trung tâm nhộn nhịp đòi hỏi phải nhìn theo các biển chỉ dẫn phát sáng phía trên.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: market"",
      ""centralWord"": ""market"",
      ""ipa"": ""/ˈmɑːkɪt/"",
      ""meaningVi"": ""Chợ, thị trường mua sắm"",
      ""collocations"": [
        {
          ""text"": ""night market"",
          ""ipa"": ""/naɪt ˈmɑːkɪt/"",
          ""meaningVi"": ""Chợ đêm"",
          ""example"": ""Exploring the street food night market is a must.""
        },
        {
          ""text"": ""flea market"",
          ""ipa"": ""/fliː ˈmɑːkɪt/"",
          ""meaningVi"": ""Chợ đồ cũ, chợ trời"",
          ""example"": ""You can discover vintage watches at the flea market.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""shop around"",
          ""ipa"": ""/ʃɒp əˈraʊnd/"",
          ""meaningVi"": ""Khảo giá các nơi"",
          ""example"": ""It pays to shop around before deciding to buy.""
        },
        {
          ""text"": ""pick out"",
          ""ipa"": ""/pɪk aʊt/"",
          ""meaningVi"": ""Lựa chọn ra"",
          ""example"": ""She picked out a beautiful ceramic vase.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""bazaar"",
          ""ipa"": ""/bəˈzɑː/"",
          ""meaningVi"": ""Khu chợ truyền thống"",
          ""example"": ""The grand bazaar was filled with colorful spices.""
        },
        {
          ""text"": ""marketplace"",
          ""ipa"": ""/ˈmɑːkɪtpleɪs/"",
          ""meaningVi"": ""Thương trường, sàn mua bán"",
          ""example"": ""Online marketplaces have transformed retail.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Traditional street markets offer intimate glimpses into local culture and cuisine."",
          ""meaningVi"": ""Các khu chợ truyền thống mang đến góc nhìn chân thực về văn hóa và ẩm thực bản địa.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: purchase"",
      ""centralWord"": ""purchase"",
      ""ipa"": ""/ˈpɜːtʃəs/"",
      ""meaningVi"": ""Mua, tậu, giao dịch mua hàng"",
      ""collocations"": [
        {
          ""text"": ""make a purchase"",
          ""ipa"": ""/meɪk ə ˈpɜːtʃəs/"",
          ""meaningVi"": ""Thực hiện mua hàng"",
          ""example"": ""Keep your sales slip whenever you make a purchase.""
        },
        {
          ""text"": ""proof of purchase"",
          ""ipa"": ""/pruːf əv ˈpɜːtʃəs/"",
          ""meaningVi"": ""Bằng chứng mua hàng"",
          ""example"": ""A receipt serves as valid proof of purchase.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""snap up"",
          ""ipa"": ""/snæp ʌp/"",
          ""meaningVi"": ""Nhanh tay mua ngay"",
          ""example"": ""Shoppers snapped up discounted luxury items.""
        },
        {
          ""text"": ""splash out on"",
          ""ipa"": ""/splæʃ aʊt ɒn/"",
          ""meaningVi"": ""Vung tiền mua món đồ đắt"",
          ""example"": ""They splashed out on an authentic bespoke suit.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""acquisition"",
          ""ipa"": ""/ˌækwɪˈzɪʃn/"",
          ""meaningVi"": ""Sự mua lại, thâu tóm"",
          ""example"": ""The painting was a prized acquisition for the collector.""
        },
        {
          ""text"": ""procurement"",
          ""ipa"": ""/prəˈkjʊəmənt/"",
          ""meaningVi"": ""Mua sắm trang thiết bị"",
          ""example"": ""Corporate procurement rules must be respected.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Thoughtful purchases of regional crafts directly support indigenous artisan families."",
          ""meaningVi"": ""Việc mua sắm các sản phẩm thủ công địa phương có ý thức sẽ hỗ trợ trực tiếp các gia đình nghệ nhân bản xứ.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: commute"",
      ""centralWord"": ""commute"",
      ""ipa"": ""/kəˈmjuːt/"",
      ""meaningVi"": ""Đi lại hằng ngày (từ nhà tới cơ quan)"",
      ""collocations"": [
        {
          ""text"": ""daily commute"",
          ""ipa"": ""/ˈdeɪli kəˈmjuːt/"",
          ""meaningVi"": ""Chuyến đi làm hằng ngày"",
          ""example"": ""His daily commute takes forty minutes on the express train.""
        },
        {
          ""text"": ""commuter rail"",
          ""ipa"": ""/kəˈmjuːtə reɪl/"",
          ""meaningVi"": ""Tuyến tàu ngoại ô đi làm"",
          ""example"": ""Commuter rail reduces highway carbon emissions.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""drop off"",
          ""ipa"": ""/drɒp ɒf/"",
          ""meaningVi"": ""Thả khách xuống"",
          ""example"": ""The taxi dropped us off right in front of the lobby.""
        },
        {
          ""text"": ""hop on"",
          ""ipa"": ""/hɒp ɒn/"",
          ""meaningVi"": ""Nhảy lên xe bus/tàu"",
          ""example"": ""Hop on the tram to reach the seaside promenade.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""transit"",
          ""ipa"": ""/ˈtrænzɪt/"",
          ""meaningVi"": ""Sự lưu thông, vận chuyển"",
          ""example"": ""Mass transit eases city congestion.""
        },
        {
          ""text"": ""travel"",
          ""ipa"": ""/ˈtrævl/"",
          ""meaningVi"": ""Đi lại"",
          ""example"": ""Urban travel is fast with underground metro lines.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Optimizing public transit significantly shortens average daily commute times."",
          ""meaningVi"": ""Tối ưu hóa giao thông công cộng rút ngắn đáng kể thời gian đi lại trung bình mỗi ngày.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: discount"",
      ""centralWord"": ""discount"",
      ""ipa"": ""/ˈdɪskaʊnt/"",
      ""meaningVi"": ""Sự chiết khấu, giảm giá"",
      ""collocations"": [
        {
          ""text"": ""promotional discount"",
          ""ipa"": ""/prəˈməʊʃənl ˈdɪskaʊnt/"",
          ""meaningVi"": ""Chiết khấu khuyến mại"",
          ""example"": ""Students are entitled to a twenty percent promotional discount.""
        },
        {
          ""text"": ""steep discount"",
          ""ipa"": ""/stiːp ˈdɪskaʊnt/"",
          ""meaningVi"": ""Giảm giá sâu"",
          ""example"": ""Clearance sales offer steep discounts on winter coats.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""mark down"",
          ""ipa"": ""/mɑːk daʊn/"",
          ""meaningVi"": ""Hạ giá bán"",
          ""example"": ""The department store marked down all summer stock.""
        },
        {
          ""text"": ""knock off"",
          ""ipa"": ""/nɒk ɒf/"",
          ""meaningVi"": ""Bớt cho một khoản tiền"",
          ""example"": ""The vendor knocked off five dollars because I bought two.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""markdown"",
          ""ipa"": ""/ˈmɑːkdaʊn/"",
          ""meaningVi"": ""Mức hạ giá"",
          ""example"": ""Price markdowns stimulated brisk seasonal sales.""
        },
        {
          ""text"": ""concession"",
          ""ipa"": ""/kənˈseʃn/"",
          ""meaningVi"": ""Sự nhượng bộ về giá"",
          ""example"": ""Senior citizens enjoy fare concessions on buses.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Inquiring politely about group discounts can yield substantial cost savings."",
          ""meaningVi"": ""Hỏi thăm lịch sự về chiết khấu cho nhóm có thể đem lại khoản tiết kiệm chi phí đáng kể.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: refund"",
      ""centralWord"": ""refund"",
      ""ipa"": ""/ˈriːfʌnd/"",
      ""meaningVi"": ""Khoản tiền hoàn lại, sự trả tiền"",
      ""collocations"": [
        {
          ""text"": ""full refund"",
          ""ipa"": ""/fʊl ˈriːfʌnd/"",
          ""meaningVi"": ""Hoàn tiền toàn bộ"",
          ""example"": ""You are entitled to a full refund if the flight is canceled.""
        },
        {
          ""text"": ""tax refund"",
          ""ipa"": ""/tæks ˈriːfʌnd/"",
          ""meaningVi"": ""Hoàn thuế du lịch"",
          ""example"": ""Submit your stamped receipts to claim your airport tax refund.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""pay back"",
          ""ipa"": ""/peɪ bæk/"",
          ""meaningVi"": ""Trả lại tiền"",
          ""example"": ""The vendor promised to pay back the difference.""
        },
        {
          ""text"": ""send back"",
          ""ipa"": ""/send bæk/"",
          ""meaningVi"": ""Gửi trả lại hàng"",
          ""example"": ""Customers can send back defective items free of charge.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""reimbursement"",
          ""ipa"": ""/ˌriːɪmˈbɜːsmənt/"",
          ""meaningVi"": ""Sự bồi hoàn chi phí"",
          ""example"": ""Submit receipts for lodging reimbursement.""
        },
        {
          ""text"": ""repayment"",
          ""ipa"": ""/rɪˈpeɪmənt/"",
          ""meaningVi"": ""Sự hoàn trả"",
          ""example"": ""The contract guarantees prompt repayment.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Understanding the retailer refund policy prevents frustrating disputes during international trips."",
          ""meaningVi"": ""Hiểu rõ chính sách hoàn tiền của đơn vị bán hàng giúp tránh các tranh cãi bực bội khi xuất ngoại.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: itinerary"",
      ""centralWord"": ""itinerary"",
      ""ipa"": ""/aɪˈtɪnərəri/"",
      ""meaningVi"": ""Lịch trình chuyến đi, lộ trình chi tiết"",
      ""collocations"": [
        {
          ""text"": ""detailed itinerary"",
          ""ipa"": ""/ˈdiːteɪld aɪˈtɪnərəri/"",
          ""meaningVi"": ""Lịch trình chi tiết"",
          ""example"": ""Our travel agency provided a meticulously detailed itinerary.""
        },
        {
          ""text"": ""customized itinerary"",
          ""ipa"": ""/ˈkʌstəmaɪzd aɪˈtɪnərəri/"",
          ""meaningVi"": ""Lịch trình thiết kế riêng"",
          ""example"": ""They enjoyed a customized itinerary tailored to historical landmarks.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""stick to"",
          ""ipa"": ""/stɪk tuː/"",
          ""meaningVi"": ""Bám sát theo"",
          ""example"": ""Try to stick to the planned itinerary to avoid delays.""
        },
        {
          ""text"": ""deviate from"",
          ""ipa"": ""/ˈdiːvieɪt frɒm/"",
          ""meaningVi"": ""Đi chệch khỏi lộ trình"",
          ""example"": ""We deviated from our itinerary to explore a remote hamlet.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""schedule"",
          ""ipa"": ""/ˈʃedjuːl/"",
          ""meaningVi"": ""Thời gian biểu"",
          ""example"": ""Our flight schedule allows ample layover time.""
        },
        {
          ""text"": ""route"",
          ""ipa"": ""/ruːt/"",
          ""meaningVi"": ""Lộ trình di chuyển"",
          ""example"": ""The coastal scenic route is breathtaking.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""A well-structured itinerary balances rigorous business engagements with restorative cultural leisure."",
          ""meaningVi"": ""Một lịch trình chu đáo cân bằng giữa các cam kết công việc nghiêm túc với những giờ thư giãn văn hóa phục hồi.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: transaction"",
      ""centralWord"": ""transaction"",
      ""ipa"": ""/trænˈzækʃn/"",
      ""meaningVi"": ""Giao dịch tài chính, thương vụ"",
      ""collocations"": [
        {
          ""text"": ""seamless transaction"",
          ""ipa"": ""/ˈsiːmləs trænˈzækʃn/"",
          ""meaningVi"": ""Giao dịch liền mạch"",
          ""example"": ""Modern POS terminals ensure a fast, seamless transaction.""
        },
        {
          ""text"": ""fraudulent transaction"",
          ""ipa"": ""/ˈfrɔːdʒələnt trænˈzækʃn/"",
          ""meaningVi"": ""Giao dịch gian lận"",
          ""example"": ""Banks immediately freeze suspicious or fraudulent transactions.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""carry through"",
          ""ipa"": ""/ˈkæri θruː/"",
          ""meaningVi"": ""Thực hiện trót lọt"",
          ""example"": ""The payment gateway carried through thousands of transactions.""
        },
        {
          ""text"": ""write off"",
          ""ipa"": ""/raɪt ɒf/"",
          ""meaningVi"": ""Xóa nợ, coi là chi phí mất đi"",
          ""example"": ""The merchant wrote off small currency conversion discrepancies.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""deal"",
          ""ipa"": ""/diːl/"",
          ""meaningVi"": ""Thỏa thuận, thương vụ"",
          ""example"": ""They closed a lucrative wholesale deal.""
        },
        {
          ""text"": ""exchange"",
          ""ipa"": ""/ɪksˈtʃeɪndʒ/"",
          ""meaningVi"": ""Sự trao đổi"",
          ""example"": ""Financial exchanges operate around the clock.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Encrypted digital protocols safeguard high-value transactions against unauthorized interception."",
          ""meaningVi"": ""Các giao thức số mã hóa bảo vệ các giao dịch giá trị cao khỏi sự can thiệp trái phép.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: congestion"",
      ""centralWord"": ""congestion"",
      ""ipa"": ""/kənˈdʒestʃən/"",
      ""meaningVi"": ""Sự ùn tắc giao thông, nghẽn mạng"",
      ""collocations"": [
        {
          ""text"": ""traffic congestion"",
          ""ipa"": ""/ˈtræfɪk kənˈdʒestʃən/"",
          ""meaningVi"": ""Ùn tắc giao thông"",
          ""example"": ""Congestion pricing curbs rush-hour traffic congestion.""
        },
        {
          ""text"": ""severe congestion"",
          ""ipa"": ""/sɪˈvɪə kənˈdʒestʃən/"",
          ""meaningVi"": ""Tắc nghẽn nghiêm trọng"",
          ""example"": ""Adverse weather caused severe congestion on the ring road.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""clog up"",
          ""ipa"": ""/klɒɡ ʌp/"",
          ""meaningVi"": ""Làm tắc nghẽn"",
          ""example"": ""Broken-down vehicles rapidly clog up narrow arterial roads.""
        },
        {
          ""text"": ""clear up"",
          ""ipa"": ""/klɪər ʌp/"",
          ""meaningVi"": ""Giải tỏa thông thoáng"",
          ""example"": ""Traffic officers worked tirelessly to clear up the blockage.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""gridlock"",
          ""ipa"": ""/ˈɡrɪdlɒk/"",
          ""meaningVi"": ""Tắc nghẽn hoàn toàn"",
          ""example"": ""Downtown streets reached absolute gridlock by 6 PM.""
        },
        {
          ""text"": ""bottleneck"",
          ""ipa"": ""/ˈbɒtlnek/"",
          ""meaningVi"": ""Điểm thắt cổ chai"",
          ""example"": ""The suspension bridge is a notorious traffic bottleneck.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Investing in high-capacity subterranean metros remains the definitive antidote to urban congestion."",
          ""meaningVi"": ""Đầu tư vào các tuyến metro ngầm công suất lớn vẫn là giải pháp tối thượng cho vấn nạn ùn tắc đô thị.""
        }
      ]
    },
    {
      ""id"": ""wm-l12-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: reimbursement"",
      ""centralWord"": ""reimbursement"",
      ""ipa"": ""/ˌriːɪmˈbɜːsmənt/"",
      ""meaningVi"": ""Khoản bồi hoàn, hoàn trả chi phí công tác"",
      ""collocations"": [
        {
          ""text"": ""expense reimbursement"",
          ""ipa"": ""/ɪkˈspens ˌriːɪmˈbɜːsmənt/"",
          ""meaningVi"": ""Bồi hoàn công tác phí"",
          ""example"": ""Submit your meal receipts to receive expense reimbursement.""
        },
        {
          ""text"": ""claim reimbursement"",
          ""ipa"": ""/kleɪm ˌriːɪmˈbɜːsmənt/"",
          ""meaningVi"": ""Yêu cầu bồi hoàn"",
          ""example"": ""Employees can claim reimbursement for foreign transit fares.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""sign off on"",
          ""ipa"": ""/saɪn ɒf ɒn/"",
          ""meaningVi"": ""Ký duyệt"",
          ""example"": ""The finance manager signed off on our travel reimbursement.""
        },
        {
          ""text"": ""pay out"",
          ""ipa"": ""/peɪ aʊt/"",
          ""meaningVi"": ""Chi trả tiền"",
          ""example"": ""Insurance claims are paid out within five business days.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""compensation"",
          ""ipa"": ""/ˌkɒmpenˈseɪʃn/"",
          ""meaningVi"": ""Khoản bồi thường"",
          ""example"": ""Airlines offer compensation for extensive baggage delays.""
        },
        {
          ""text"": ""indemnity"",
          ""ipa"": ""/ɪnˈdemnəti/"",
          ""meaningVi"": ""Sự bồi thường thiệt hại"",
          ""example"": ""The contract includes an indemnity clause against loss.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Meticulous preservation of itemized receipts expedites seamless corporate travel reimbursement."",
          ""meaningVi"": ""Việc giữ gìn cẩn thận các hóa đơn chi tiết giúp đẩy nhanh quá trình bồi hoàn chi phí công tác thuận lợi.""
        }
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.2 Cloze (OrderIndex 15)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444447"), "cloze", 15),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444447"),
            OrderIndex = 15,
            Type = "cloze",
            Title = "Điền từ vào chỗ trống (Bài 1.2): Bài tập ngữ cảnh thực tế",
            Description = "Đọc đoạn văn hoặc câu giao tiếp và điền từ thích hợp vào chỗ trống với gợi ý và giải thích chi tiết.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l12-cloze-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Hỏi đường đi bộ đến ga tàu (Dễ)"",
      ""promptVi"": ""Điền từ thích hợp để hoàn thiện câu hỏi đường lịch sự:"",
      ""template"": ""Excuse me, is the central train station within {{0}} distance from this {{1}}?"",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""walking""
          ],
          ""hintVi"": ""có thể đi bộ được (___ distance)"",
          ""explanationVi"": ""\""within walking distance\"" nghĩa là trong khoảng cách đi bộ được.""
        },
        {
          ""acceptedAnswers"": [
            ""hotel"",
            ""place"",
            ""spot"",
            ""location""
          ],
          ""hintVi"": ""khách sạn / địa điểm (bắt đầu bằng h hoặc p)"",
          ""explanationVi"": ""\""from this hotel / location\"" chỉ vị trí xuất phát.""
        }
      ],
      ""fullSentence"": ""Excuse me, is the central train station within walking distance from this hotel?"",
      ""meaningVi"": ""Xin lỗi, nhà ga trung tâm có nằm trong khoảng cách đi bộ được từ khách sạn này không?"",
      ""explanationVi"": ""Mẫu câu hỏi thông dụng bậc nhất khi ở khách sạn hỏi nhân viên lễ tân.""
    },
    {
      ""id"": ""en-l12-cloze-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Hỏi thanh toán quẹt thẻ (Dễ)"",
      ""promptVi"": ""Điền từ về thanh toán số:"",
      ""template"": ""Do you accept {{0}} cards or mobile {{1}} for small souvenir purchases?"",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""contactless"",
            ""credit""
          ],
          ""hintVi"": ""thanh toán chạm / tín dụng (bắt đầu bằng c)"",
          ""explanationVi"": ""\""contactless cards\"" là thẻ thanh toán chạm.""
        },
        {
          ""acceptedAnswers"": [
            ""payments"",
            ""wallets""
          ],
          ""hintVi"": ""thanh toán / ví di động (bắt đầu bằng p)"",
          ""explanationVi"": ""\""mobile payments\"" nghĩa là thanh toán qua điện thoại (Apple Pay, Google Pay).""
        }
      ],
      ""fullSentence"": ""Do you accept contactless cards or mobile payments for small souvenir purchases?"",
      ""meaningVi"": ""Cửa hàng có chấp nhận thẻ chạm hoặc thanh toán di động cho các món quà lưu niệm nhỏ không?"",
      ""explanationVi"": ""Mẫu câu chuẩn mực tại các cửa hàng bán lẻ và chợ đồ lưu niệm quốc tế.""
    },
    {
      ""id"": ""en-l12-cloze-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Chỉ đường qua bùng binh và ngã tư (Vừa)"",
      ""promptVi"": ""Điền từ chỉ phương hướng và mốc giao thông:"",
      ""template"": ""Take the second exit at the {{0}}, and continue straight until the next {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""roundabout""
          ],
          ""hintVi"": ""vòng xuyến / bùng binh (bắt đầu bằng r)"",
          ""explanationVi"": ""\""roundabout\"" là vòng xuyến giao thông.""
        },
        {
          ""acceptedAnswers"": [
            ""intersection"",
            ""crossroad"",
            ""traffic light"",
            ""junction""
          ],
          ""hintVi"": ""ngã tư / nút giao (bắt đầu bằng i)"",
          ""explanationVi"": ""\""intersection\"" là ngã tư đường phố.""
        }
      ],
      ""fullSentence"": ""Take the second exit at the roundabout, and continue straight until the next intersection."",
      ""meaningVi"": ""Đi theo lối ra thứ hai ở bùng binh, và tiếp tục đi thẳng cho đến ngã tư tiếp theo."",
      ""explanationVi"": ""Hai thuật ngữ giao thông đường bộ cốt lõi khi nghe người bản xứ chỉ đường.""
    },
    {
      ""id"": ""en-l12-cloze-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Thủ tục hoàn thuế mua sắm (Vừa)"",
      ""promptVi"": ""Điền các từ về hóa đơn và hoàn thuế:"",
      ""template"": ""Could you please stamp my tax refund {{0}} so that I can claim the money at the airport {{1}}?"",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""receipt"",
            ""form"",
            ""invoice""
          ],
          ""hintVi"": ""biên lai / mẫu đơn (bắt đầu bằng r hoặc f)"",
          ""explanationVi"": ""\""tax refund receipt / form\"" là hóa đơn hoàn thuế.""
        },
        {
          ""acceptedAnswers"": [
            ""customs"",
            ""counter"",
            ""checkpoint""
          ],
          ""hintVi"": ""quầy hải quan / quầy hoàn thuế (bắt đầu bằng c)"",
          ""explanationVi"": ""\""airport customs / counter\"" là quầy làm thủ tục tại sân bay.""
        }
      ],
      ""fullSentence"": ""Could you please stamp my tax refund receipt so that I can claim the money at the airport customs?"",
      ""meaningVi"": ""Bạn có thể đóng dấu biên lai hoàn thuế để tôi nhận tiền tại quầy hải quan sân bay được không?"",
      ""explanationVi"": ""Cụm từ then chốt cho khách du lịch và người đi công tác nước ngoài.""
    },
    {
      ""id"": ""en-l12-cloze-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Xử lý hành lý chậm trễ (Khó)"",
      ""promptVi"": ""Điền từ chuyên ngành hàng không về sự cố hành lý:"",
      ""template"": ""Since my checked {{0}} failed to appear, I need to submit a property {{1}} claim immediately."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""luggage"",
            ""baggage"",
            ""bags""
          ],
          ""hintVi"": ""hành lý ký gửi (bắt đầu bằng l hoặc b)"",
          ""explanationVi"": ""\""checked luggage / baggage\"" là hành lý gửi theo chuyến bay.""
        },
        {
          ""acceptedAnswers"": [
            ""irregularity"",
            ""loss"",
            ""damage""
          ],
          ""hintVi"": ""bất thường / thất lạc (bắt đầu bằng i hoặc l)"",
          ""explanationVi"": ""\""Property Irregularity Report (PIR)\"" là biên bản thất lạc hành lý quốc tế.""
        }
      ],
      ""fullSentence"": ""Since my checked luggage failed to appear, I need to submit a property irregularity claim immediately."",
      ""meaningVi"": ""Vì hành lý ký gửi của tôi không xuất hiện, tôi cần nộp biên bản khai báo bất thường về tài sản ngay lập tức."",
      ""explanationVi"": ""Thuật ngữ chuẩn xác giúp bạn giải quyết quyền lợi nhanh chóng tại sân bay quốc tế.""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.2 Translate (OrderIndex 16)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444447"), "translate", 16),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444447"),
            OrderIndex = 16,
            Type = "translate",
            Title = "Dịch phản xạ Việt → Anh (Bài 1.2): Diễn đạt tự nhiên",
            Description = "Chuyển câu tiếng Việt sang tiếng Anh tự nhiên với nhiều đáp án được chấp nhận và lưu ý ngữ pháp hữu ích.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l12-trans-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Hỏi đường đến bến tàu điện (Dễ)"",
      ""sourceVi"": ""Xin lỗi, bạn có thể chỉ cho tôi đường nhanh nhất để tới ga tàu điện ngầm không?"",
      ""hintKeywords"": [
        ""quickest way"",
        ""subway station"",
        ""could you tell me""
      ],
      ""acceptedAnswers"": [
        ""Excuse me, could you tell me the quickest way to get to the subway station?"",
        ""Excuse me, can you show me the quickest way to the subway station?"",
        ""Excuse me, could you tell me the fastest way to get to the metro station?""
      ],
      ""sampleAnswer"": ""Excuse me, could you tell me the quickest way to get to the subway station?"",
      ""grammarNoteVi"": ""Dùng câu hỏi gián tiếp \""Could you tell me...\"" và cụm so sánh hơn nhất \""the quickest way\"".""
    },
    {
      ""id"": ""en-l12-trans-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Hỏi thử quần áo (Dễ)"",
      ""sourceVi"": ""Tôi có thể thử chiếc áo sơ mi này trong phòng thay đồ được không?"",
      ""hintKeywords"": [
        ""try on"",
        ""fitting room"",
        ""shirt"",
        ""can I""
      ],
      ""acceptedAnswers"": [
        ""Can I try this shirt on in the fitting room?"",
        ""Could I try this shirt on in the fitting room?"",
        ""May I try this shirt on in the dressing room?""
      ],
      ""sampleAnswer"": ""Can I try this shirt on in the fitting room?"",
      ""grammarNoteVi"": ""Cụm động từ tách rời: \""try + tân ngữ + on\"".""
    },
    {
      ""id"": ""en-l12-trans-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Thanh toán chạm thẻ (Vừa)"",
      ""sourceVi"": ""Cửa hàng của bạn có chấp nhận thanh toán chạm bằng thẻ hoặc điện thoại thông minh không?"",
      ""hintKeywords"": [
        ""accept contactless payments"",
        ""cards"",
        ""smartphones""
      ],
      ""acceptedAnswers"": [
        ""Does your store accept contactless payments with cards or smartphones?"",
        ""Do you accept contactless payments with cards or smartphones here?"",
        ""Does this shop accept contactless payment via card or smartphone?""
      ],
      ""sampleAnswer"": ""Does your store accept contactless payments with cards or smartphones?"",
      ""grammarNoteVi"": ""Chủ ngữ \""your store\"" số ít dùng trợ động từ \""Does\"".""
    },
    {
      ""id"": ""en-l12-trans-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Đi đường vòng do sửa đường (Vừa)"",
      ""sourceVi"": ""Người đi bộ phải đi theo đường vòng có biển chỉ dẫn vì con đường chính đang được thi công."",
      ""hintKeywords"": [
        ""pedestrians"",
        ""marked detour"",
        ""under construction""
      ],
      ""acceptedAnswers"": [
        ""Pedestrians must follow the marked detour because the main street is under construction."",
        ""Pedestrians have to follow the detour as the main road is under construction."",
        ""Pedestrians must take the marked detour since the main street is undergoing repair.""
      ],
      ""sampleAnswer"": ""Pedestrians must follow the marked detour because the main street is under construction."",
      ""grammarNoteVi"": ""Cụm \""under construction\"" nghĩa là đang thi công xây dựng.""
    },
    {
      ""id"": ""en-l12-trans-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Dịch: Khiếu nại hành lý thất lạc (Khó)"",
      ""sourceVi"": ""Hành khách có quyền yêu cầu phiếu bồi thường và hỗ trợ khẩn cấp nếu hành lý ký gửi bị thất lạc quá 24 giờ."",
      ""hintKeywords"": [
        ""passengers are entitled to"",
        ""compensation vouchers"",
        ""checked luggage"",
        ""lost for over 24 hours""
      ],
      ""acceptedAnswers"": [
        ""Passengers are entitled to claim compensation vouchers and emergency assistance if checked luggage is delayed for over 24 hours."",
        ""Passengers have the right to request compensation vouchers and emergency support if their checked baggage is lost for more than 24 hours.""
      ],
      ""sampleAnswer"": ""Passengers are entitled to claim compensation vouchers and emergency assistance if checked luggage is delayed for over 24 hours."",
      ""grammarNoteVi"": ""Cấu trúc \""be entitled to + Noun/V-infinitive\"" biểu thị quyền lợi hợp pháp.""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.2 Grammar Fix (OrderIndex 17)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444447"), "grammar_fix", 17),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444447"),
            OrderIndex = 17,
            Type = "grammar_fix",
            Title = "Sửa lỗi ngữ pháp (Bài 1.2): Khắc phục các lỗi người Việt hay mắc",
            Description = "Phát hiện và sửa các lỗi sai ngữ pháp, thì động từ, mạo từ và giới từ thường gặp trong giao tiếp thực tế.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l12-gf-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Đảo ngữ trong câu hỏi gián tiếp (Dễ)"",
      ""incorrectSentence"": ""Could you tell me where is the tourist information center?"",
      ""errorTypeHintVi"": ""Trong câu hỏi gián tiếp, trật tự từ là Chủ ngữ + Động từ (không đảo to be lên trước)."",
      ""correctedSentence"": ""Could you tell me where the tourist information center is?"",
      ""acceptedAnswers"": [
        ""Could you tell me where the tourist information center is?"",
        ""Can you tell me where the tourist information center is?""
      ],
      ""meaningVi"": ""Bạn có thể cho tôi biết trung tâm thông tin du lịch ở đâu không?"",
      ""explanationVi"": ""Sau từ để hỏi trong câu gián tiếp (\""where\""), chủ ngữ (\""the tourist information center\"") đứng trước động từ to be (\""is\"").""
    },
    {
      ""id"": ""en-l12-gf-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Dùng sai giới từ khoảng cách (Dễ)"",
      ""incorrectSentence"": ""The subway entrance is located in walking distance from here."",
      ""errorTypeHintVi"": ""Cụm từ cố định chỉ cự ly đi bộ dùng giới từ \""within\"" chứ không dùng \""in\""."",
      ""correctedSentence"": ""The subway entrance is located within walking distance from here."",
      ""acceptedAnswers"": [
        ""The subway entrance is located within walking distance from here."",
        ""The subway entrance is within walking distance from here.""
      ],
      ""meaningVi"": ""Lối vào ga tàu điện ngầm nằm trong cự ly có thể đi bộ được từ đây."",
      ""explanationVi"": ""Thành ngữ chuẩn là \""within walking distance\"".""
    },
    {
      ""id"": ""en-l12-gf-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Trợ động từ thừa trong câu hỏi gián tiếp (Vừa)"",
      ""incorrectSentence"": ""Do you happen to know what time does the museum open?"",
      ""errorTypeHintVi"": ""Câu hỏi gián tiếp không dùng trợ động từ do/does mà chia trực tiếp vào động từ chính."",
      ""correctedSentence"": ""Do you happen to know what time the museum opens?"",
      ""acceptedAnswers"": [
        ""Do you happen to know what time the museum opens?"",
        ""Do you know what time the museum opens?""
      ],
      ""meaningVi"": ""Bạn có biết bảo tàng mở cửa lúc mấy giờ không?"",
      ""explanationVi"": ""Bỏ \""does\"" và chia động từ \""opens\"" theo chủ ngữ ngôi thứ 3 số ít \""the museum\"".""
    },
    {
      ""id"": ""en-l12-gf-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Giới từ phương tiện giao thông (Vừa)"",
      ""incorrectSentence"": ""We decided to reach the coastal market by foot instead of taxi."",
      ""errorTypeHintVi"": ""Đi bộ dùng cụm \""on foot\"", không dùng \""by foot\"". Đi taxi dùng \""by taxi\""."",
      ""correctedSentence"": ""We decided to reach the coastal market on foot instead of by taxi."",
      ""acceptedAnswers"": [
        ""We decided to reach the coastal market on foot instead of by taxi."",
        ""We decided to go to the coastal market on foot instead of by taxi.""
      ],
      ""meaningVi"": ""Chúng tôi quyết định đi bộ đến khu chợ ven biển thay vì đi bằng taxi."",
      ""explanationVi"": ""Quy tắc ngữ pháp cố định: \""on foot\"" (đi bộ) nhưng \""by bus / by taxi / by train\"".""
    },
    {
      ""id"": ""en-l12-gf-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Sửa lỗi: Thể bị động & danh từ không đếm được (Khó)"",
      ""incorrectSentence"": ""All luggages must inspected before boarding the express ferry."",
      ""errorTypeHintVi"": ""\""Luggage\"" là danh từ không đếm được (không có số nhiều) và cần dùng bị động đầy đủ \""must be inspected\""."",
      ""correctedSentence"": ""All luggage must be inspected before boarding the express ferry."",
      ""acceptedAnswers"": [
        ""All luggage must be inspected before boarding the express ferry."",
        ""All baggage must be inspected before boarding the express ferry.""
      ],
      ""meaningVi"": ""Tất cả hành lý phải được kiểm tra trước khi lên phà cao tốc."",
      ""explanationVi"": ""\""Luggage/baggage\"" không bao giờ thêm -s; cấu trúc bị động khuyết thiếu là \""must be + V3/ed\"".""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 1.2 Roleplay (OrderIndex 18)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444447"), "roleplay", 18),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444447"),
            OrderIndex = 18,
            Type = "roleplay",
            Title = "Nhập vai hội thoại (Bài 1.2): Tình huống phản xạ thực chiến",
            Description = "Nhập vai giao tiếp với AI trong các tình huống thực tế, hỗ trợ chấm điểm tự động theo từ khóa và cấu trúc câu.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l12-rp-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Hỏi đường đi tàu điện ngầm (Dễ)"",
      ""scenarioVi"": ""Bạn vừa ra khỏi khách sạn tại Tokyo và muốn hỏi một người đi đường cách đến ga tàu điện ngầm gần nhất."",
      ""aiRole"": ""Người dân địa phương (Local resident)"",
      ""aiPrompt"": ""Good morning! You look a bit lost. Can I help you with directions?"",
      ""aiPromptVi"": ""Chào buổi sáng! Trông bạn có vẻ hơi lạc đường. Tôi có thể chỉ đường giúp bạn không?"",
      ""userGoalVi"": ""Cảm ơn người dân, hỏi đường đến ga tàu điện gần nhất và hỏi xem có đi bộ được không."",
      ""expectedKeywords"": [
        ""thank you"",
        ""subway"",
        ""station"",
        ""walking distance"",
        ""how to get""
      ],
      ""sampleResponses"": [
        ""Good morning, thank you so much! Could you tell me how to get to the nearest subway station? Is it within walking distance?""
      ],
      ""minWords"": 8
    },
    {
      ""id"": ""en-l12-rp-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Mua sắm & hỏi size quần áo (Dễ)"",
      ""scenarioVi"": ""Bạn đang ở cửa hàng thời trang tại London và muốn tìm size vừa vặn cho chiếc áo khoác."",
      ""aiRole"": ""Nhân viên bán hàng (Shop assistant)"",
      ""aiPrompt"": ""Hello there! That trench coat looks great on you. Are you looking for a different size or color?"",
      ""aiPromptVi"": ""Xin chào! Chiếc áo măng tô đó trông rất hợp với bạn. Bạn có đang tìm kích cỡ hoặc màu sắc khác không?"",
      ""userGoalVi"": ""Khen chiếc áo đẹp, hỏi xem cửa hàng còn size M (medium size) màu xanh tím than (navy blue) không."",
      ""expectedKeywords"": [
        ""love"",
        ""medium size"",
        ""navy blue"",
        ""in stock"",
        ""have""
      ],
      ""sampleResponses"": [
        ""I really love this design! Do you have this jacket in medium size and navy blue in stock?""
      ],
      ""minWords"": 8
    },
    {
      ""id"": ""en-l12-rp-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Hỏi hoàn thuế du lịch (Vừa)"",
      ""scenarioVi"": ""Sau khi mua sắm hơn 150 USD đồ lưu niệm, bạn muốn nhân viên thu ngân hỗ trợ làm thủ tục hoàn thuế."",
      ""aiRole"": ""Thu ngân cửa hàng (Store cashier)"",
      ""aiPrompt"": ""Your total comes to one hundred and sixty dollars. Will you be paying with cash or card today?"",
      ""aiPromptVi"": ""Tổng hóa đơn của bạn là 160 đô la. Hôm nay bạn thanh toán bằng tiền mặt hay thẻ?"",
      ""userGoalVi"": ""Chọn thanh toán bằng thẻ chạm/thẻ tín dụng, và đề nghị thu ngân xuất hóa đơn hoàn thuế du lịch."",
      ""expectedKeywords"": [
        ""credit card"",
        ""contactless"",
        ""tax refund"",
        ""receipt"",
        ""tourist""
      ],
      ""sampleResponses"": [
        ""I will pay with my contactless credit card, please. Could you also provide a tax refund receipt for international tourists?""
      ],
      ""minWords"": 10
    },
    {
      ""id"": ""en-l12-rp-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Xử lý nhầm lẫn chuyến tàu (Vừa)"",
      ""scenarioVi"": ""Bạn đứng ở sân ga và không chắc đoàn tàu vừa tới có dừng ở sân bay quốc tế hay không."",
      ""aiRole"": ""Nhân viên trực sân ga (Platform supervisor)"",
      ""aiPrompt"": ""Attention passengers! The train on track two is boarding. Please check your tickets before stepping in."",
      ""aiPromptVi"": ""Hành khách chú ý! Chuyến tàu ở đường ray số 2 đang đón khách. Vui lòng kiểm tra vé trước khi bước lên."",
      ""userGoalVi"": ""Tiến lại hỏi nhân viên xem tàu này có phải là tàu nhanh đến sân bay không, hay bạn phải chờ tàu khác."",
      ""expectedKeywords"": [
        ""excuse me"",
        ""this train"",
        ""airport"",
        ""express"",
        ""track""
      ],
      ""sampleResponses"": [
        ""Excuse me! Does this train go directly to the international airport terminal, or should I wait for the express line on another track?""
      ],
      ""minWords"": 10
    },
    {
      ""id"": ""en-l12-rp-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Khiếu nại hành lý thất lạc tại sân bay (Khó)"",
      ""scenarioVi"": ""Băng chuyền hành lý đã dừng lại nhưng vali của bạn không thấy đâu. Bạn đến quầy Lost & Found của hãng bay."",
      ""aiRole"": ""Nhân viên quầy hành lý thất lạc (Baggage service agent)"",
      ""aiPrompt"": ""Welcome to baggage services. How can I assist you with your arrival today?"",
      ""aiPromptVi"": ""Chào mừng bạn đến bộ phận hỗ trợ hành lý. Tôi có thể hỗ trợ gì cho bạn trong chuyến đến hôm nay?"",
      ""userGoalVi"": ""Trình bày hành lý ký gửi không xuất hiện trên băng chuyền, xuất trình thẻ giữ hành lý (baggage tag) và yêu cầu lập biên bản PIR."",
      ""expectedKeywords"": [
        ""checked luggage"",
        ""conveyor belt"",
        ""baggage tag"",
        ""report"",
        ""irregularity"",
        ""claim""
      ],
      ""sampleResponses"": [
        ""My checked luggage failed to appear on the conveyor belt. Here are my boarding pass and baggage tag. I would like to file a formal Property Irregularity Report immediately.""
      ],
      ""minWords"": 14
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.1 Shadowing (OrderIndex 11)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444448"), "shadowing", 11),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444448"),
            OrderIndex = 11,
            Type = "shadowing",
            Title = "Shadowing mở rộng (Bài 2.1): 15 câu đa cấp độ Dễ → Vừa → Khó",
            Description = "Luyện nói đuổi theo ngữ điệu người bản xứ, tập trung vào nối âm, ngữ điệu và phát âm chuẩn các âm then chốt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l21-shad-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Mở đầu email thông báo (Dễ)"",
      ""sentence"": ""I am writing to confirm our meeting scheduled for tomorrow morning."",
      ""ipa"": ""/aɪ æm ˈraɪtɪŋ tuː kənˈfɜːm ˈaʊə ˈmiːtɪŋ ˈʃedjuːld fɔː təˈmɒrəʊ ˈmɔːnɪŋ/"",
      ""meaningVi"": ""Tôi viết thư này để xác nhận cuộc họp của chúng ta theo lịch vào sáng mai."",
      ""keyPhrases"": [
        ""writing to confirm"",
        ""scheduled for"",
        ""tomorrow morning""
      ]
    },
    {
      ""id"": ""en-l21-shad-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Đính kèm tài liệu (Dễ)"",
      ""sentence"": ""Please find attached the weekly progress report for your review."",
      ""ipa"": ""/pliːz faɪnd əˈtætʃt ðə ˈdeɪli ˈprəʊɡres rɪˈpɔːt fɔː jɔː rɪˈvjuː/"",
      ""meaningVi"": ""Xin vui lòng xem báo cáo tiến độ tuần được đính kèm để xem xét."",
      ""keyPhrases"": [
        ""please find attached"",
        ""progress report"",
        ""for your review""
      ]
    },
    {
      ""id"": ""en-l21-shad-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi thời gian rảnh (Dễ)"",
      ""sentence"": ""Are you free for a quick phone call this afternoon at three?"",
      ""ipa"": ""/ɑː juː friː fɔːr ə kwɪk fəʊn kɔːl ðɪs ˌɑːftəˈnuːn æt θriː/"",
      ""meaningVi"": ""Bạn có rảnh để trao đổi nhanh qua điện thoại vào 3 giờ chiều nay không?"",
      ""keyPhrases"": [
        ""free for a quick call"",
        ""this afternoon"",
        ""at three""
      ]
    },
    {
      ""id"": ""en-l21-shad-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Cảm ơn và hỗ trợ (Dễ)"",
      ""sentence"": ""Thank you for your prompt reply and valuable feedback."",
      ""ipa"": ""/θæŋk juː fɔː jɔː prɒmpt rɪˈplaɪ ænd ˈvæljuəbl ˈfiːdbæk/"",
      ""meaningVi"": ""Cảm ơn bạn đã phản hồi nhanh chóng và đóng góp ý kiến quý báu."",
      ""keyPhrases"": [
        ""prompt reply"",
        ""valuable feedback"",
        ""thank you for""
      ]
    },
    {
      ""id"": ""en-l21-shad-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Kết thúc thư lịch sự (Dễ)"",
      ""sentence"": ""Please let me know if you need any additional information."",
      ""ipa"": ""/pliːz let miː nəʊ ɪf juː niːd ˈeni əˈdɪʃənl ˌɪnfəˈmeɪʃn/"",
      ""meaningVi"": ""Xin vui lòng cho tôi biết nếu bạn cần thêm bất kỳ thông tin nào."",
      ""keyPhrases"": [
        ""let me know"",
        ""additional information"",
        ""need any""
      ]
    },
    {
      ""id"": ""en-l21-shad-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đề xuất lịch hẹn linh hoạt (Vừa)"",
      ""sentence"": ""Would you be available for a brief thirty-minute sync to discuss the revised project budget?"",
      ""ipa"": ""/wʊd juː biː əˈveɪləbl fɔːr ə briːf ˈθɜːti ˈmɪnɪt sɪŋk tuː dɪˈskʌs ðə rɪˈvaɪzd ˈprɒdʒekt ˈbʌdʒɪt/"",
      ""meaningVi"": ""Anh/chị có tiện dành 30 phút họp nhanh để thảo luận về ngân sách dự án đã sửa đổi không?"",
      ""keyPhrases"": [
        ""would you be available"",
        ""brief sync"",
        ""revised budget""
      ]
    },
    {
      ""id"": ""en-l21-shad-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Báo cáo mốc tiến độ (Vừa)"",
      ""sentence"": ""Our engineering department has accomplished all major milestones ahead of the quarterly deadline."",
      ""ipa"": ""/ˈaʊər ˌendʒɪˈnɪərɪŋ dɪˈpɑːtmənt hæz əˈkʌmplɪʃt ɔːl ˈmeɪdʒə ˈmaɪlstəʊnz əˈhed əv ðə ˈkwɔːtəli ˈdedlaɪn/"",
      ""meaningVi"": ""Bộ phận kỹ thuật của chúng tôi đã hoàn thành tất cả các mốc tiến độ lớn trước thời hạn quý."",
      ""keyPhrases"": [
        ""accomplished milestones"",
        ""ahead of deadline"",
        ""quarterly deadline""
      ]
    },
    {
      ""id"": ""en-l21-shad-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Hoãn lịch do sự cố khẩn (Vừa)"",
      ""sentence"": ""Due to an unexpected scheduling conflict, I must kindly request to reschedule our consultation."",
      ""ipa"": ""/djuː tuː ən ˌʌnɪkˈspektɪd ˈʃedjuːlɪŋ ˈkɒnflɪkt, aɪ mʌst ˈkaɪndli rɪˈkwest tuː ˌriːˈʃedjuːl ˈaʊə ˌkɒnslˈteɪʃn/"",
      ""meaningVi"": ""Do một sự cố trùng lịch bất ngờ, tôi xin phép được đề xuất dời lịch buổi tư vấn của chúng ta."",
      ""keyPhrases"": [
        ""scheduling conflict"",
        ""kindly request"",
        ""reschedule consultation""
      ]
    },
    {
      ""id"": ""en-l21-shad-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Khắc phục nút thắt cổ chai (Vừa)"",
      ""sentence"": ""We are currently streamlining our internal approvals to eliminate operational bottlenecks."",
      ""ipa"": ""/wiː ɑː ˈkʌrəntli ˈstriːmlaɪnɪŋ ˈaʊər ɪnˈtɜːnl əˈpruːvlz tuː ɪˈlɪmɪneɪt ˌɒpəˈreɪʃənl ˈbɒtlneks/"",
      ""meaningVi"": ""Chúng tôi hiện đang tinh gọn quy trình phê duyệt nội bộ nhằm xóa bỏ các điểm nghẽn vận hành."",
      ""keyPhrases"": [
        ""streamlining approvals"",
        ""eliminate bottlenecks"",
        ""operational bottlenecks""
      ]
    },
    {
      ""id"": ""en-l21-shad-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Theo dõi đầu việc hành động (Vừa)"",
      ""sentence"": ""I have documented the agreed action items and assigned clear ownership for each task."",
      ""ipa"": ""/aɪ hæv ˈdɒkjumentɪd ði əˈɡriːd ˈækʃn ˈaɪtəmz ænd əˈsaɪnd klɪər ˈəʊnəʃɪp fɔːr iːtʃ tɑːsk/"",
      ""meaningVi"": ""Tôi đã ghi chép lại các đầu việc đã thống nhất và phân công người chịu trách nhiệm rõ ràng cho từng nhiệm vụ."",
      ""keyPhrases"": [
        ""action items"",
        ""assigned ownership"",
        ""agreed tasks""
      ]
    },
    {
      ""id"": ""en-l21-shad-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Báo cáo quản trị rủi ro cấp cao (Khó)"",
      ""sentence"": ""This executive summary elucidates our strategic risk mitigation protocols and anticipated deliverable horizons."",
      ""ipa"": ""/ðɪs ɪɡˈzekjətɪv ˈsʌməri ɪˈluːsɪdeɪts ˈaʊə strəˈtiːdʒɪk rɪsk ˌmɪtɪˈɡeɪʃn ˈprəʊtəkɒlz ænd ænˈtɪsɪpeɪtɪd dɪˈlɪvərəbl həˈraɪznz/"",
      ""meaningVi"": ""Bản tóm tắt điều hành này làm sáng tỏ các quy trình giảm thiểu rủi ro chiến lược và các mốc sản phẩm kỳ vọng."",
      ""keyPhrases"": [
        ""executive summary"",
        ""risk mitigation protocols"",
        ""deliverable horizons""
      ]
    },
    {
      ""id"": ""en-l21-shad-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Phân bổ lại nguồn lực dự án (Khó)"",
      ""sentence"": ""Reallocating specialized engineering talent to mission-critical infrastructure prevented substantial deployment delays."",
      ""ipa"": ""/ˌriːˈæləkeɪtɪŋ ˈspeʃəlaɪzd ˌendʒɪˈnɪərɪŋ ˈtælənt tuː ˈmɪʃn ˈkrɪtɪkl ˌɪnfrəˈstrʌktʃə prɪˈventɪd səbˈstænʃl dɪˈplɔɪmənt dɪˈleɪz/"",
      ""meaningVi"": ""Việc tái phân bổ nhân tài kỹ thuật chuyên sâu cho hạ tầng trọng yếu đã ngăn chặn được sự chậm trễ bàn giao lớn."",
      ""keyPhrases"": [
        ""reallocating engineering talent"",
        ""mission-critical infrastructure"",
        ""deployment delays""
      ]
    },
    {
      ""id"": ""en-l21-shad-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Thương thảo điều chỉnh phạm vi (Khó)"",
      ""sentence"": ""Should unforeseen regulatory constraints jeopardize our roadmap, we shall promptly convene an extraordinary steering committee."",
      ""ipa"": ""/ʃʊd ˌʌnfɔːˈsiːn ˌreɡjʊˈleɪtəri kənˈstreɪnts ˈdʒepədaɪz ˈaʊə ˈrəʊdmæp, wiː ʃæl ˈprɒmptli kənˈviːn ən ɪkˈstrɔːdnri ˈstɪərɪŋ kəˈmɪti/"",
      ""meaningVi"": ""Nếu các rào cản pháp lý bất ngờ đe dọa lộ trình, chúng tôi sẽ lập tức triệu tập ban chỉ đạo bất thường."",
      ""keyPhrases"": [
        ""regulatory constraints"",
        ""jeopardize roadmap"",
        ""extraordinary steering committee""
      ]
    },
    {
      ""id"": ""en-l21-shad-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Báo cáo đối chiếu tiến độ tuần (Khó)"",
      ""sentence"": ""Our cross-functional velocity metrics substantiate that automation investments have yielded exponential efficiency dividends."",
      ""ipa"": ""/ˈaʊə krɒs ˈfʌŋkʃənl vəˈlɒsəti ˈmetrɪks səbˈstænʃieɪt ðæt ˌɔːtəˈmeɪʃn ɪnˈvestmənts hæv ˈjiːldɪd ˌekspəˈnenʃl ɪˈfɪʃnsi ˈdɪvɪdendz/"",
      ""meaningVi"": ""Các chỉ số vận tốc liên phòng ban chứng minh rằng đầu tư vào tự động hóa đã mang lại lợi suất hiệu quả cấp số nhân."",
      ""keyPhrases"": [
        ""velocity metrics"",
        ""automation investments"",
        ""exponential efficiency dividends""
      ]
    },
    {
      ""id"": ""en-l21-shad-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cam kết trách nhiệm giải trình (Khó)"",
      ""sentence"": ""Establishing uncompromising accountability frameworks guarantees that organizational deliverables withstand rigorous external audits."",
      ""ipa"": ""/ɪˈstæblɪʃɪŋ ʌnˈkɒmprəmaɪzɪŋ əˌkaʊntəˈbɪləti ˈfreɪmwɜːks ˌɡærənˈtiːz ðæt ˌɔːɡənaɪˈzeɪʃənl dɪˈlɪvərəblz wɪðˈstænd ˈrɪɡərəs ɪkˈstɜːnl ˈɔːdɪts/"",
      ""meaningVi"": ""Thiết lập các khung trách nhiệm giải trình nghiêm ngặt đảm bảo rằng các sản phẩm của tổ chức vượt qua các cuộc kiểm toán bên ngoài khắt khe."",
      ""keyPhrases"": [
        ""accountability frameworks"",
        ""organizational deliverables"",
        ""external audits""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.1 Dictation (OrderIndex 12)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444448"), "dictation", 12),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444448"),
            OrderIndex = 12,
            Type = "dictation",
            Title = "Dictation mở rộng (Bài 2.1): 15 câu nghe chép chính tả phản xạ",
            Description = "Nghe câu hội thoại đa cấp độ và gõ lại chính xác từng từ hoặc chọn word chip để rèn luyện thính giác tiếng Anh.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l21-dict-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Xác nhận lịch hẹn qua email (Dễ)"",
      ""sentence"": ""I am pleased to confirm our meeting for tomorrow at two o'clock."",
      ""ipa"": ""/aɪ æm pliːzd tuː kənˈfɜːm ˈaʊə ˈmiːtɪŋ fɔː təˈmɒrəʊ æt tuː əˈklɒk/"",
      ""meaningVi"": ""Tôi rất vui được xác nhận cuộc họp của chúng ta vào lúc 2 giờ ngày mai."",
      ""wordChips"": [
        ""I"",
        ""am"",
        ""pleased"",
        ""to"",
        ""confirm"",
        ""our"",
        ""meeting"",
        ""for"",
        ""tomorrow"",
        ""at"",
        ""two"",
        ""o'clock""
      ],
      ""distractors"": [
        ""glad"",
        ""verify"",
        ""time"",
        ""scheduled""
      ]
    },
    {
      ""id"": ""en-l21-dict-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Gửi tài liệu đính kèm (Dễ)"",
      ""sentence"": ""Attached is the latest spreadsheet detailing our project expenses."",
      ""ipa"": ""/əˈtætʃt ɪz ðə ˈleɪtɪst ˈspredʃiːt ˈdiːteɪlɪŋ ˈaʊə ˈprɒdʒekt ɪkˈspensɪz/"",
      ""meaningVi"": ""Đính kèm là bảng tính mới nhất nêu chi tiết các khoản chi của dự án."",
      ""wordChips"": [
        ""Attached"",
        ""is"",
        ""the"",
        ""latest"",
        ""spreadsheet"",
        ""detailing"",
        ""our"",
        ""project"",
        ""expenses""
      ],
      ""distractors"": [
        ""here"",
        ""file"",
        ""document"",
        ""costs""
      ]
    },
    {
      ""id"": ""en-l21-dict-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Cập nhật tiến độ hoàn thành (Dễ)"",
      ""sentence"": ""We have completed the first draft of the user documentation."",
      ""ipa"": ""/wiː hæv kəmˈpliːtɪd ðə fɜːst drɑːft əv ðə ˈjuːzə ˌdɒkjumenˈteɪʃn/"",
      ""meaningVi"": ""Chúng tôi đã hoàn thành bản thảo đầu tiên của tài liệu hướng dẫn người dùng."",
      ""wordChips"": [
        ""We"",
        ""have"",
        ""completed"",
        ""the"",
        ""first"",
        ""draft"",
        ""of"",
        ""the"",
        ""user"",
        ""documentation""
      ],
      ""distractors"": [
        ""did"",
        ""finished"",
        ""guide"",
        ""manual""
      ]
    },
    {
      ""id"": ""en-l21-dict-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi lại thời gian họp (Dễ)"",
      ""sentence"": ""Does Thursday morning work well for your team?"",
      ""ipa"": ""/dʌz ˈθɜːzdeɪ ˈmɔːnɪŋ wɜːk wel fɔː jɔː tiːm/"",
      ""meaningVi"": ""Sáng thứ Năm có thuận tiện cho nhóm của bạn không?"",
      ""wordChips"": [
        ""Does"",
        ""Thursday"",
        ""morning"",
        ""work"",
        ""well"",
        ""for"",
        ""your"",
        ""team""
      ],
      ""distractors"": [
        ""is"",
        ""convenient"",
        ""good"",
        ""fit""
      ]
    },
    {
      ""id"": ""en-l21-dict-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Lời chào kết thư trang trọng (Dễ)"",
      ""sentence"": ""I look forward to hearing your constructive thoughts soon."",
      ""ipa"": ""/aɪ lʊk ˈfɔːwəd tuː ˈhɪərɪŋ jɔː kənˈstrʌktɪv θɔːts suːn/"",
      ""meaningVi"": ""Tôi rất mong sớm nhận được những đóng góp mang tính xây dựng của bạn."",
      ""wordChips"": [
        ""I"",
        ""look"",
        ""forward"",
        ""to"",
        ""hearing"",
        ""your"",
        ""constructive"",
        ""thoughts"",
        ""soon""
      ],
      ""distractors"": [
        ""hear"",
        ""waiting"",
        ""ideas"",
        ""feedback""
      ]
    },
    {
      ""id"": ""en-l21-dict-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Thông báo dời lịch họp (Vừa)"",
      ""sentence"": ""Due to unforeseen circumstances, we must reschedule our strategy session to Friday."",
      ""ipa"": ""/djuː tuː ˌʌnfɔːˈsiːn ˈsɜːkəmstənsɪz, wiː mʌst ˌriːˈʃedjuːl ˈaʊə ˈstrætədʒi ˈseʃn tuː ˈfraɪdeɪ/"",
      ""meaningVi"": ""Do các tình huống bất khả kháng, chúng tôi phải dời buổi thảo luận chiến lược sang thứ Sáu."",
      ""wordChips"": [
        ""Due"",
        ""to"",
        ""unforeseen"",
        ""circumstances"",
        ""we"",
        ""must"",
        ""reschedule"",
        ""our"",
        ""strategy"",
        ""session"",
        ""to"",
        ""Friday""
      ],
      ""distractors"": [
        ""because"",
        ""reasons"",
        ""postpone"",
        ""meeting""
      ]
    },
    {
      ""id"": ""en-l21-dict-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đạt mốc dự án đúng hẹn (Vừa)"",
      ""sentence"": ""All key deliverables have been successfully deployed according to the agreed schedule."",
      ""ipa"": ""/ɔːl kiː dɪˈlɪvərəblz hæv biːn səkˈsesfəli dɪˈplɔɪd əˈkɔːdɪŋ tuː ði əˈɡriːd ˈʃedjuːl/"",
      ""meaningVi"": ""Tất cả các sản phẩm bàn giao then chốt đã được triển khai thành công theo đúng tiến độ đã thỏa thuận."",
      ""wordChips"": [
        ""All"",
        ""key"",
        ""deliverables"",
        ""have"",
        ""been"",
        ""successfully"",
        ""deployed"",
        ""according"",
        ""to"",
        ""the"",
        ""agreed"",
        ""schedule""
      ],
      ""distractors"": [
        ""were"",
        ""launched"",
        ""timeline"",
        ""milestones""
      ]
    },
    {
      ""id"": ""en-l21-dict-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đề xuất chương trình làm việc (Vừa)"",
      ""sentence"": ""Please review the attached tentative agenda and suggest any topics you wish to cover."",
      ""ipa"": ""/pliːz rɪˈvjuː ði əˈtætʃt ˈtentətɪv əˈdʒendə ænd səˈdʒest ˈeni ˈtɒpɪks juː wɪʃ tuː ˈkʌvə/"",
      ""meaningVi"": ""Xin vui lòng xem chương trình nghị sự dự kiến đính kèm và đề xuất các chủ đề bạn muốn bàn thảo."",
      ""wordChips"": [
        ""Please"",
        ""review"",
        ""the"",
        ""attached"",
        ""tentative"",
        ""agenda"",
        ""and"",
        ""suggest"",
        ""any"",
        ""topics"",
        ""you"",
        ""wish"",
        ""to"",
        ""cover""
      ],
      ""distractors"": [
        ""check"",
        ""draft"",
        ""points"",
        ""discuss""
      ]
    },
    {
      ""id"": ""en-l21-dict-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Báo cáo xử lý sự cố (Vừa)"",
      ""sentence"": ""Our site reliability engineers resolved the database latency issue within twenty minutes."",
      ""ipa"": ""/ˈaʊə saɪt rɪˌlaɪəˈbɪləti ˌendʒɪˈnɪəz rɪˈzɒlvd ðə ˈdeɪtəbeɪs ˈleɪtənsi ˈɪʃuː wɪðˈɪn ˈtwenti ˈmɪnɪts/"",
      ""meaningVi"": ""Các kỹ sư độ tin cậy hệ thống của chúng tôi đã xử lý sự cố trễ cơ sở dữ liệu trong vòng 20 phút."",
      ""wordChips"": [
        ""Our"",
        ""site"",
        ""reliability"",
        ""engineers"",
        ""resolved"",
        ""the"",
        ""database"",
        ""latency"",
        ""issue"",
        ""within"",
        ""twenty"",
        ""minutes""
      ],
      ""distractors"": [
        ""fixed"",
        ""problem"",
        ""speed"",
        ""under""
      ]
    },
    {
      ""id"": ""en-l21-dict-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Sắp xếp thứ tự ưu tiên (Vừa)"",
      ""sentence"": ""We recommend prioritizing automated regression tests before shipping the quarterly release."",
      ""ipa"": ""/wiː ˌrekəˈmend praɪˈɒrətaɪzɪŋ ˈɔːtəmeɪtɪd rɪˈɡreʃn tests bɪˈfɔː ˈʃɪpɪŋ ðə ˈkwɔːtəli rɪˈliːs/"",
      ""meaningVi"": ""Chúng tôi khuyến nghị ưu tiên kiểm thử hồi quy tự động trước khi xuất bản bản phát hành quý."",
      ""wordChips"": [
        ""We"",
        ""recommend"",
        ""prioritizing"",
        ""automated"",
        ""regression"",
        ""tests"",
        ""before"",
        ""shipping"",
        ""the"",
        ""quarterly"",
        ""release""
      ],
      ""distractors"": [
        ""suggest"",
        ""priority"",
        ""testing"",
        ""launching""
      ]
    },
    {
      ""id"": ""en-l21-dict-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Thủ tục phê duyệt ngân sách (Khó)"",
      ""sentence"": ""Securing cross-departmental budgetary approval requires comprehensive financial modeling and substantiated ROI projections."",
      ""ipa"": ""/sɪˈkjʊərɪŋ krɒs dɪˌpɑːtˈmentl ˈbʌdʒɪtri əˈpruːvl rɪˈkwaɪəz ˌkɒmprɪˈhensɪv faɪˈnænʃl ˈmɒdəlɪŋ ænd səbˈstænʃieɪtɪd ˌɑːr əʊ ˈaɪ prəˈdʒekʃnz/"",
      ""meaningVi"": ""Việc đạt được phê duyệt ngân sách liên phòng ban đòi hỏi mô hình tài chính toàn diện và dự báo hoàn vốn có căn cứ."",
      ""wordChips"": [
        ""Securing"",
        ""cross-departmental"",
        ""budgetary"",
        ""approval"",
        ""requires"",
        ""comprehensive"",
        ""financial"",
        ""modeling"",
        ""and"",
        ""substantiated"",
        ""ROI"",
        ""projections""
      ],
      ""distractors"": [
        ""getting"",
        ""budget"",
        ""clear"",
        ""estimates""
      ]
    },
    {
      ""id"": ""en-l21-dict-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Kế hoạch phòng ngừa rủi ro dự án (Khó)"",
      ""sentence"": ""Deploying robust contingency mechanisms effectively insulates critical infrastructure from unpredictable third-party outages."",
      ""ipa"": ""/dɪˈplɔɪɪŋ rəʊˈbʌst kənˈtɪndʒənsi ˈmekənɪzəmz ɪˈfektɪvli ˈɪnsjʊleɪts ˈkrɪtɪkl ˌɪnfrəˈstrʌktʃə frɒm ˌʌnprɪˈdɪktəbl ˈθɜːd ˈpɑːti ˈaʊtɪdʒɪz/"",
      ""meaningVi"": ""Triển khai các cơ chế dự phòng vững chắc cách ly hiệu quả hạ tầng trọng yếu khỏi các sự cố sập mạng bên thứ ba khó lường."",
      ""wordChips"": [
        ""Deploying"",
        ""robust"",
        ""contingency"",
        ""mechanisms"",
        ""effectively"",
        ""insulates"",
        ""critical"",
        ""infrastructure"",
        ""from"",
        ""unpredictable"",
        ""third-party"",
        ""outages""
      ],
      ""distractors"": [
        ""using"",
        ""strong"",
        ""protects"",
        ""downtime""
      ]
    },
    {
      ""id"": ""en-l21-dict-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Đánh giá tiến độ bằng chỉ số đo lường (Khó)"",
      ""sentence"": ""Quarterly milestone assessments demonstrate that agile engineering workflows consistently outpace legacy waterfall paradigms."",
      ""ipa"": ""/ˈkwɔːtəli ˈmaɪlstəʊn əˈsesmənts ˈdemənstreɪt ðæt ˈædʒaɪl ˌendʒɪˈnɪərɪŋ ˈwɜːkfləʊz kənˈsɪstəntli aʊtˈpeɪs ˈleɡəsi ˈwɔːtəfɔːl ˈpærədaɪmz/"",
      ""meaningVi"": ""Các đánh giá mốc tiến độ hàng quý chứng minh rằng quy trình kỹ thuật linh hoạt vượt trội bền bỉ so với mô hình thác nước truyền thống."",
      ""wordChips"": [
        ""Quarterly"",
        ""milestone"",
        ""assessments"",
        ""demonstrate"",
        ""that"",
        ""agile"",
        ""engineering"",
        ""workflows"",
        ""consistently"",
        ""outpace"",
        ""legacy"",
        ""waterfall"",
        ""paradigms""
      ],
      ""distractors"": [
        ""reviews"",
        ""show"",
        ""beat"",
        ""methods""
      ]
    },
    {
      ""id"": ""en-l21-dict-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Hội đồng chỉ đạo chiến lược (Khó)"",
      ""sentence"": ""The executive steering committee unanimously sanctioned the expedited deployment of decentralized security architecture."",
      ""ipa"": ""/ði ɪɡˈzekjətɪv ˈstɪərɪŋ kəˈmɪti juːˈnænɪməsli ˈsæŋkʃnd ði ˈekspədaɪtɪd dɪˈplɔɪmənt əv diːˈsentrəlaɪzd sɪˈkjʊərəti ˈɑːkɪtektʃə/"",
      ""meaningVi"": ""Ban chỉ đạo điều hành đã nhất trí thông qua việc triển khai khẩn cấp kiến trúc an ninh phi tập trung."",
      ""wordChips"": [
        ""The"",
        ""executive"",
        ""steering"",
        ""committee"",
        ""unanimously"",
        ""sanctioned"",
        ""the"",
        ""expedited"",
        ""deployment"",
        ""of"",
        ""decentralized"",
        ""security"",
        ""architecture""
      ],
      ""distractors"": [
        ""board"",
        ""approved"",
        ""fast"",
        ""framework""
      ]
    },
    {
      ""id"": ""en-l21-dict-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Minh bạch trong trách nhiệm giải trình (Khó)"",
      ""sentence"": ""Transparent progress documentation eliminates ambiguity and reinforces stakeholder alignment across global distributed teams."",
      ""ipa"": ""/trænsˈpærənt ˈprəʊɡres ˌdɒkjumenˈteɪʃn ɪˈlɪmɪneɪts ˌæmbɪˈɡjuːəti ænd ˌriːɪnˈfɔːsɪz ˈsteɪkhəʊldər əˈlaɪnmənt əˈkrɒs ˈɡləʊbl dɪˈstrɪbjuːtɪd tiːmz/"",
      ""meaningVi"": ""Việc ghi chép tiến độ minh bạch giúp xóa bỏ sự mơ hồ và củng cố sự đồng thuận của các bên liên quan trên các nhóm phân tán toàn cầu."",
      ""wordChips"": [
        ""Transparent"",
        ""progress"",
        ""documentation"",
        ""eliminates"",
        ""ambiguity"",
        ""and"",
        ""reinforces"",
        ""stakeholder"",
        ""alignment"",
        ""across"",
        ""global"",
        ""distributed"",
        ""teams""
      ],
      ""distractors"": [
        ""clear"",
        ""removes"",
        ""strengthens"",
        ""partners""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.1 Pronunciation (OrderIndex 13)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444448"), "pronunciation", 13),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444448"),
            OrderIndex = 13,
            Type = "pronunciation",
            Title = "Chấm phát âm mở rộng (Bài 2.1): 15 câu luyện âm /θ/-/ð/, âm cuối và đuôi -s/-ed",
            Description = "Luyện phát âm với AI chấm điểm trực tiếp, phát hiện lỗi sai âm đuôi và các âm khó phổ biến của người Việt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l21-pron-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm đuôi -ed dạng /d/ trong scheduled (Dễ)"",
      ""sentence"": ""Our meeting is scheduled for tomorrow at nine."",
      ""ipa"": ""/ˈaʊə ˈmiːtɪŋ ɪz ˈʃedjuːld fɔː təˈmɒrəʊ æt naɪn/"",
      ""meaningVi"": ""Cuộc họp của chúng ta được xếp lịch vào lúc 9 giờ sáng mai."",
      ""targetWords"": [
        ""scheduled"",
        ""tomorrow"",
        ""meeting""
      ],
      ""keyPhrases"": [
        ""meeting is scheduled"",
        ""tomorrow at nine""
      ]
    },
    {
      ""id"": ""en-l21-pron-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm đuôi -ed dạng /t/ trong attached (Dễ)"",
      ""sentence"": ""Please check the attached document before noon."",
      ""ipa"": ""/pliːz tʃek ði əˈtætʃt ˈdɒkjumənt bɪˈfɔː nuːn/"",
      ""meaningVi"": ""Làm ơn kiểm tra tài liệu đính kèm trước buổi trưa."",
      ""targetWords"": [
        ""attached"",
        ""document"",
        ""check""
      ],
      ""keyPhrases"": [
        ""attached document"",
        ""before noon""
      ]
    },
    {
      ""id"": ""en-l21-pron-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm đuôi -ed dạng /ɪd/ trong completed (Dễ)"",
      ""sentence"": ""The engineering team completed the initial phase."",
      ""ipa"": ""/ði ˌendʒɪˈnɪərɪŋ tiːm kəmˈpliːtɪd ði ɪˈnɪʃl feɪz/"",
      ""meaningVi"": ""Nhóm kỹ thuật đã hoàn thành giai đoạn ban đầu."",
      ""targetWords"": [
        ""completed"",
        ""initial"",
        ""phase""
      ],
      ""keyPhrases"": [
        ""completed initial phase""
      ]
    },
    {
      ""id"": ""en-l21-pron-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm đuôi -s/es trong deliverables (Dễ)"",
      ""sentence"": ""We delivered all project deliverables on time."",
      ""ipa"": ""/wiː dɪˈlɪvəd ɔːl ˈprɒdʒekt dɪˈlɪvərəblz ɒn taɪm/"",
      ""meaningVi"": ""Chúng tôi đã bàn giao mọi sản phẩm đầu ra đúng hạn."",
      ""targetWords"": [
        ""delivered"",
        ""deliverables"",
        ""time""
      ],
      ""keyPhrases"": [
        ""project deliverables"",
        ""on time""
      ]
    },
    {
      ""id"": ""en-l21-pron-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /v/ trong available & valuable (Dễ)"",
      ""sentence"": ""Are you available to provide valuable advice?"",
      ""ipa"": ""/ɑː juː əˈveɪləbl tuː prəˈvaɪd ˈvæljuəbl ədˈvaɪs/"",
      ""meaningVi"": ""Bạn có sẵn sàng cung cấp những lời khuyên quý báu không?"",
      ""targetWords"": [
        ""available"",
        ""provide"",
        ""valuable"",
        ""advice""
      ],
      ""keyPhrases"": [
        ""available to provide"",
        ""valuable advice""
      ]
    },
    {
      ""id"": ""en-l21-pron-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /str/ trong streamlined workflow (Vừa)"",
      ""sentence"": ""Automation streamlined our deployment cycles and boosted productivity."",
      ""ipa"": ""/ˌɔːtəˈmeɪʃn ˈstriːmlaɪnd ˈaʊə dɪˈplɔɪmənt ˈsaɪklz ænd ˈbuːstɪd ˌprɒdʌkˈtɪvəti/"",
      ""meaningVi"": ""Tự động hóa đã tinh gọn các chu kỳ triển khai và gia tăng năng suất."",
      ""targetWords"": [
        ""streamlined"",
        ""deployment"",
        ""cycles"",
        ""boosted""
      ],
      ""keyPhrases"": [
        ""streamlined deployment"",
        ""boosted productivity""
      ]
    },
    {
      ""id"": ""en-l21-pron-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /z/ trong prioritizes & resources (Vừa)"",
      ""sentence"": ""Management prioritizes resources for high-impact deliverables."",
      ""ipa"": ""/ˈmænɪdʒmənt praɪˈɒrətaɪzɪz rɪˈzɔːsɪz fɔː haɪ ˈɪmpækt dɪˈlɪvərəblz/"",
      ""meaningVi"": ""Ban quản lý ưu tiên nguồn lực cho các sản phẩm bàn giao có tầm ảnh hưởng lớn."",
      ""targetWords"": [
        ""prioritizes"",
        ""resources"",
        ""deliverables""
      ],
      ""keyPhrases"": [
        ""prioritizes resources"",
        ""high-impact deliverables""
      ]
    },
    {
      ""id"": ""en-l21-pron-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Cụm phụ âm /ks/ trong operational bottlenecks (Vừa)"",
      ""sentence"": ""Identifying operational bottlenecks protects the milestone schedule."",
      ""ipa"": ""/aɪˈdentɪfaɪɪŋ ˌɒpəˈreɪʃənl ˈbɒtlneks prəˈtekts ðə ˈmaɪlstəʊn ˈʃedjuːl/"",
      ""meaningVi"": ""Nhận diện các điểm nghẽn vận hành bảo vệ tiến độ của các mốc quan trọng."",
      ""targetWords"": [
        ""identifying"",
        ""bottlenecks"",
        ""protects"",
        ""milestone""
      ],
      ""keyPhrases"": [
        ""operational bottlenecks"",
        ""milestone schedule""
      ]
    },
    {
      ""id"": ""en-l21-pron-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /d/ vs /t/ trong resolved & launched (Vừa)"",
      ""sentence"": ""The engineers resolved the incident and launched the update."",
      ""ipa"": ""/ði ˌendʒɪˈnɪəz rɪˈzɒlvd ði ˈɪnsɪdənt ænd lɔːntʃt ði ˌʌpˈdeɪt/"",
      ""meaningVi"": ""Các kỹ sư đã xử lý xong sự cố và phát hành bản cập nhật."",
      ""targetWords"": [
        ""resolved"",
        ""incident"",
        ""launched"",
        ""update""
      ],
      ""keyPhrases"": [
        ""resolved incident"",
        ""launched update""
      ]
    },
    {
      ""id"": ""en-l21-pron-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Nối âm trong tentative agenda (Vừa)"",
      ""sentence"": ""Please review the tentative agenda attached to this email."",
      ""ipa"": ""/pliːz rɪˈvjuː ðə ˈtentətɪv əˈdʒendə əˈtætʃt tuː ðɪs ˈiːmeɪl/"",
      ""meaningVi"": ""Làm ơn xem chương trình làm việc dự kiến được đính kèm trong email này."",
      ""targetWords"": [
        ""tentative"",
        ""agenda"",
        ""attached"",
        ""email""
      ],
      ""keyPhrases"": [
        ""tentative agenda"",
        ""attached to this email""
      ]
    },
    {
      ""id"": ""en-l21-pron-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Trọng âm và âm /k/ trong accountability (Khó)"",
      ""sentence"": ""Strict accountability frameworks substantiate thorough regulatory compliance."",
      ""ipa"": ""/strɪkt əˌkaʊntəˈbɪləti ˈfreɪmwɜːks səbˈstænʃieɪt ˈθʌrə ˌreɡjʊˈleɪtəri kəmˈplaɪəns/"",
      ""meaningVi"": ""Các khung trách nhiệm giải trình nghiêm ngặt chứng minh sự tuân thủ quy định chặt chẽ."",
      ""targetWords"": [
        ""accountability"",
        ""frameworks"",
        ""substantiate"",
        ""regulatory"",
        ""compliance""
      ],
      ""keyPhrases"": [
        ""accountability frameworks"",
        ""regulatory compliance""
      ]
    },
    {
      ""id"": ""en-l21-pron-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cụm phụ âm /kst/ trong extraordinary context (Khó)"",
      ""sentence"": ""Extraordinary executive meetings address acute organizational bottlenecks."",
      ""ipa"": ""/ɪkˈstrɔːdnri ɪɡˈzekjətɪv ˈmiːtɪŋz əˈdres əˈkjuːt ˌɔːɡənaɪˈzeɪʃənl ˈbɒtlneks/"",
      ""meaningVi"": ""Các cuộc họp điều hành bất thường giải quyết những điểm nghẽn tổ chức cấp bách."",
      ""targetWords"": [
        ""extraordinary"",
        ""executive"",
        ""address"",
        ""acute"",
        ""bottlenecks""
      ],
      ""keyPhrases"": [
        ""extraordinary meetings"",
        ""organizational bottlenecks""
      ]
    },
    {
      ""id"": ""en-l21-pron-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Ngữ điệu câu phức và âm /ʃ/ trong substantiate (Khó)"",
      ""sentence"": ""Comprehensive metrics substantiate whether our agile velocity satisfies milestone requirements."",
      ""ipa"": ""/ˌkɒmprɪˈhensɪv ˈmetrɪks səbˈstænʃieɪt ˈweðər ˈaʊər ˈædʒaɪl vəˈlɒsəti ˈsætɪsfaɪz ˈmaɪlstəʊn rɪˈkwaɪəmənts/"",
      ""meaningVi"": ""Các chỉ số toàn diện chứng minh liệu tốc độ linh hoạt của chúng ta có đáp ứng các yêu cầu mốc tiến độ không."",
      ""targetWords"": [
        ""comprehensive"",
        ""substantiate"",
        ""velocity"",
        ""satisfies"",
        ""requirements""
      ],
      ""keyPhrases"": [
        ""comprehensive metrics"",
        ""substantiate whether"",
        ""milestone requirements""
      ]
    },
    {
      ""id"": ""en-l21-pron-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Âm /v/ vs /w/ & /ð/ trong worthwhile workflows (Khó)"",
      ""sentence"": ""Worthwhile automated workflows withstand unpredictable infrastructure disruptions."",
      ""ipa"": ""/ˈwɜːθwaɪl ˈɔːtəmeɪtɪd ˈwɜːkfləʊz wɪðˈstænd ˌʌnprɪˈdɪktəbl ˌɪnfrəˈstrʌktʃə dɪsˈrʌpʃnz/"",
      ""meaningVi"": ""Các quy trình tự động hóa giá trị có khả năng chống chọi trước những sự cố gián đoạn hạ tầng khó lường."",
      ""targetWords"": [
        ""worthwhile"",
        ""automated"",
        ""workflows"",
        ""withstand"",
        ""disruptions""
      ],
      ""keyPhrases"": [
        ""worthwhile workflows"",
        ""withstand disruptions""
      ]
    },
    {
      ""id"": ""en-l21-pron-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cụm phụ âm /ndʒ/ & /dʒ/ trong contingency budget (Khó)"",
      ""sentence"": ""Managing contingency budgets requires judicious allocation of specialized resources."",
      ""ipa"": ""/ˈmænɪdʒɪŋ kənˈtɪndʒənsi ˈbʌdʒɪts rɪˈkwaɪəz dʒuːˈdɪʃəs ˌæləˈkeɪʃn əv ˈspeʃəlaɪzd rɪˈzɔːsɪz/"",
      ""meaningVi"": ""Quản trị ngân sách dự phòng đòi hỏi sự phân bổ sáng suốt các nguồn lực chuyên môn."",
      ""targetWords"": [
        ""managing"",
        ""contingency"",
        ""budgets"",
        ""judicious"",
        ""allocation"",
        ""resources""
      ],
      ""keyPhrases"": [
        ""contingency budgets"",
        ""judicious allocation"",
        ""specialized resources""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.1 Word Map (OrderIndex 14)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444448"), "word_map", 14),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444448"),
            OrderIndex = 14,
            Type = "word_map",
            Title = "Sơ đồ từ vựng Mind Map mở rộng (Bài 2.1): 12 sơ đồ Dễ → Vừa → Khó",
            Description = "Hệ thống sơ đồ tư duy từ vựng đa cấp độ kèm phiên âm IPA, collocations, phrasal verbs, synonyms và ví dụ ngữ cảnh thực chiến.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""wm-l21-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: email"",
      ""centralWord"": ""email"",
      ""ipa"": ""/ˈiːmeɪl/"",
      ""meaningVi"": ""Thư điện tử, gửi thư công việc"",
      ""collocations"": [
        {
          ""text"": ""send an email"",
          ""ipa"": ""/send ən ˈiːmeɪl/"",
          ""meaningVi"": ""Gửi email"",
          ""example"": ""I will send an email confirming our schedule.""
        },
        {
          ""text"": ""reply to an email"",
          ""ipa"": ""/rɪˈplaɪ tuː ən ˈiːmeɪl/"",
          ""meaningVi"": ""Phản hồi email"",
          ""example"": ""Please reply to the client email promptly.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""follow up on"",
          ""ipa"": ""/ˈfɒləʊ ʌp ɒn/"",
          ""meaningVi"": ""Theo dõi tiến độ sau email"",
          ""example"": ""I am writing to follow up on our previous inquiry.""
        },
        {
          ""text"": ""cc in"",
          ""ipa"": ""/siː siː ɪn/"",
          ""meaningVi"": ""Đính kèm thêm người nhận"",
          ""example"": ""Please cc in the project manager on all replies.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""correspondence"",
          ""ipa"": ""/ˌkɒrəˈspɒndəns/"",
          ""meaningVi"": ""Thư từ giao dịch"",
          ""example"": ""Keep all formal correspondence archived.""
        },
        {
          ""text"": ""message"",
          ""ipa"": ""/ˈmesɪdʒ/"",
          ""meaningVi"": ""Tin nhắn, thông điệp"",
          ""example"": ""He sent an urgent message to the team.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Well-crafted emails maintain clarity and foster strong professional relationships."",
          ""meaningVi"": ""Các email được viết chỉn chu duy trì sự rõ ràng và nuôi dưỡng quan hệ nghề nghiệp vững chắc.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: schedule"",
      ""centralWord"": ""schedule"",
      ""ipa"": ""/ˈʃedjuːl/"",
      ""meaningVi"": ""Lịch trình, xếp lịch hẹn"",
      ""collocations"": [
        {
          ""text"": ""tight schedule"",
          ""ipa"": ""/taɪt ˈʃedjuːl/"",
          ""meaningVi"": ""Lịch trình dày đặc"",
          ""example"": ""The executive is working on an extremely tight schedule.""
        },
        {
          ""text"": ""ahead of schedule"",
          ""ipa"": ""/əˈhed əv ˈʃedjuːl/"",
          ""meaningVi"": ""Trước tiến độ dự kiến"",
          ""example"": ""We finished the sprint two days ahead of schedule.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""fit in"",
          ""ipa"": ""/fɪt ɪn/"",
          ""meaningVi"": ""Bố trí thời gian gặp"",
          ""example"": ""Can you fit in a short sync before lunch?""
        },
        {
          ""text"": ""push back"",
          ""ipa"": ""/pʊʃ bæk/"",
          ""meaningVi"": ""Lùi lịch lại"",
          ""example"": ""We had to push back the launch date by one week.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""timetable"",
          ""ipa"": ""/ˈtaɪmteɪbl/"",
          ""meaningVi"": ""Thời gian biểu"",
          ""example"": ""The conference timetable is available online.""
        },
        {
          ""text"": ""calendar"",
          ""ipa"": ""/ˈkælɪndə/"",
          ""meaningVi"": ""Lịch làm việc"",
          ""example"": ""Check my shared calendar for open slots.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Respecting colleagues' schedules is a fundamental mark of workplace professionalism."",
          ""meaningVi"": ""Tôn trọng lịch trình của đồng nghiệp là biểu hiện cơ bản của phong thái chuyên nghiệp nơi công sở.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: deadline"",
      ""centralWord"": ""deadline"",
      ""ipa"": ""/ˈdedlaɪn/"",
      ""meaningVi"": ""Hạn chót, thời hạn hoàn thành"",
      ""collocations"": [
        {
          ""text"": ""meet a deadline"",
          ""ipa"": ""/miːt ə ˈdedlaɪn/"",
          ""meaningVi"": ""Kịp thời hạn"",
          ""example"": ""Our entire team worked diligently to meet the deadline.""
        },
        {
          ""text"": ""miss a deadline"",
          ""ipa"": ""/mɪs ə ˈdedlaɪn/"",
          ""meaningVi"": ""Trễ hạn"",
          ""example"": ""Missing a client deadline damages organizational trust.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""push for"",
          ""ipa"": ""/pʊʃ fɔː/"",
          ""meaningVi"": ""Thúc đẩy tiến độ"",
          ""example"": ""Management is pushing for an early release date.""
        },
        {
          ""text"": ""fall behind"",
          ""ipa"": ""/fɔːl bɪˈhaɪnd/"",
          ""meaningVi"": ""Bị chậm tiến độ"",
          ""example"": ""Without automated testing, developers fall behind.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""target date"",
          ""ipa"": ""/ˈtɑːɡɪt deɪt/"",
          ""meaningVi"": ""Ngày mục tiêu"",
          ""example"": ""Our target date for deployment is next Friday.""
        },
        {
          ""text"": ""due date"",
          ""ipa"": ""/djuː deɪt/"",
          ""meaningVi"": ""Ngày đến hạn"",
          ""example"": ""The invoice due date is within thirty days.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Clear deadlines focus team efforts and prevent scope creep during sprints."",
          ""meaningVi"": ""Thời hạn rõ ràng giúp đội ngũ tập trung nỗ lực và ngăn ngừa việc mở rộng phạm vi tùy tiện.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: update"",
      ""centralWord"": ""update"",
      ""ipa"": ""/ˌʌpˈdeɪt/"",
      ""meaningVi"": ""Cập nhật, thông tin mới nhất"",
      ""collocations"": [
        {
          ""text"": ""status update"",
          ""ipa"": ""/ˈsteɪtəs ˌʌpˈdeɪt/"",
          ""meaningVi"": ""Cập nhật tình trạng"",
          ""example"": ""Please provide a concise status update in tomorrow's standup.""
        },
        {
          ""text"": ""keep updated"",
          ""ipa"": ""/kiːp ʌpˈdeɪtɪd/"",
          ""meaningVi"": ""Liên tục cập nhật cho ai"",
          ""example"": ""I will keep you updated on all testing results.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""bring up to speed"",
          ""ipa"": ""/brɪŋ ʌp tuː spiːd/"",
          ""meaningVi"": ""Cập nhật cho nắm kịp tiến độ"",
          ""example"": ""Let's bring new members up to speed on architecture.""
        },
        {
          ""text"": ""fill in on"",
          ""ipa"": ""/fɪl ɪn ɒn/"",
          ""meaningVi"": ""Cung cấp chi tiết cho ai"",
          ""example"": ""Can you fill me in on yesterday's client feedback?""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""briefing"",
          ""ipa"": ""/ˈbriːfɪŋ/"",
          ""meaningVi"": ""Bản báo cáo tóm tắt"",
          ""example"": ""The morning briefing covered key deliverables.""
        },
        {
          ""text"": ""report"",
          ""ipa"": ""/rɪˈpɔːt/"",
          ""meaningVi"": ""Báo cáo"",
          ""example"": ""Submit your weekly progress report on Friday.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Frequent updates eliminate misunderstandings and ensure organizational alignment."",
          ""meaningVi"": ""Cập nhật thông tin thường xuyên giúp xóa bỏ hiểu lầm và bảo đảm sự đồng lòng trong tổ chức.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: deliverable"",
      ""centralWord"": ""deliverable"",
      ""ipa"": ""/dɪˈlɪvərəbl/"",
      ""meaningVi"": ""Sản phẩm bàn giao, kết quả công việc"",
      ""collocations"": [
        {
          ""text"": ""key deliverable"",
          ""ipa"": ""/kiː dɪˈlɪvərəbl/"",
          ""meaningVi"": ""Sản phẩm bàn giao cốt lõi"",
          ""example"": ""The API specification is our key deliverable this sprint.""
        },
        {
          ""text"": ""final deliverable"",
          ""ipa"": ""/ˈfaɪnl dɪˈlɪvərəbl/"",
          ""meaningVi"": ""Sản phẩm hoàn thiện cuối cùng"",
          ""example"": ""Stakeholders signed off on the final deliverable.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""hand over"",
          ""ipa"": ""/hænd ˈəʊvə/"",
          ""meaningVi"": ""Bàn giao lại"",
          ""example"": ""We handed over the codebase to their internal operations team.""
        },
        {
          ""text"": ""sign off on"",
          ""ipa"": ""/saɪn ɒf ɒn/"",
          ""meaningVi"": ""Ký duyệt nghiệm thu"",
          ""example"": ""The product owner signed off on all sprint items.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""output"",
          ""ipa"": ""/ˈaʊtpʊt/"",
          ""meaningVi"": ""Đầu ra, sản lượng"",
          ""example"": ""High quality output requires continuous discipline.""
        },
        {
          ""text"": ""result"",
          ""ipa"": ""/rɪˈzʌlt/"",
          ""meaningVi"": ""Kết quả"",
          ""example"": ""The team delivered impressive tangible results.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Defining deliverables with rigorous acceptance criteria prevents costly rework."",
          ""meaningVi"": ""Xác định sản phẩm bàn giao với tiêu chuẩn nghiệm thu chặt chẽ giúp tránh làm lại tốn kém.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: milestone"",
      ""centralWord"": ""milestone"",
      ""ipa"": ""/ˈmaɪlstəʊn/"",
      ""meaningVi"": ""Cột mốc quan trọng, bước ngoặt"",
      ""collocations"": [
        {
          ""text"": ""reach a milestone"",
          ""ipa"": ""/riːtʃ ə ˈmaɪlstəʊn/"",
          ""meaningVi"": ""Chạm tới mốc quan trọng"",
          ""example"": ""We reached a major milestone by launching the beta release.""
        },
        {
          ""text"": ""project milestone"",
          ""ipa"": ""/ˈprɒdʒekt ˈmaɪlstəʊn/"",
          ""meaningVi"": ""Mốc tiến độ dự án"",
          ""example"": ""Reviewing project milestones weekly keeps everyone accountable.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""work towards"",
          ""ipa"": ""/wɜːk təˈwɔːdz/"",
          ""meaningVi"": ""Nỗ lực hướng tới"",
          ""example"": ""All developers work towards the next quarterly milestone.""
        },
        {
          ""text"": ""build up to"",
          ""ipa"": ""/bɪld ʌp tuː/"",
          ""meaningVi"": ""Từng bước chuẩn bị cho"",
          ""example"": ""Months of preparation built up to our successful launch.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""benchmark"",
          ""ipa"": ""/ˈbentʃmɑːk/"",
          ""meaningVi"": ""Tiêu chuẩn đánh giá"",
          ""example"": ""The product sets a new performance benchmark.""
        },
        {
          ""text"": ""turning point"",
          ""ipa"": ""/ˈtɜːnɪŋ pɔɪnt/"",
          ""meaningVi"": ""Bước ngoặt"",
          ""example"": ""The partnership marked a turning point in our expansion.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Celebrating reached milestones boosts team morale and sustains long-term momentum."",
          ""meaningVi"": ""Tôn vinh các mốc tiến độ đạt được giúp nâng cao sĩ khí đội ngũ và duy trì đà phát triển dài hạn.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: agenda"",
      ""centralWord"": ""agenda"",
      ""ipa"": ""/əˈdʒendə/"",
      ""meaningVi"": ""Chương trình nghị sự, nội dung cuộc họp"",
      ""collocations"": [
        {
          ""text"": ""set an agenda"",
          ""ipa"": ""/set ən əˈdʒendə/"",
          ""meaningVi"": ""Lập chương trình họp"",
          ""example"": ""Always set an agenda before scheduling any alignment call.""
        },
        {
          ""text"": ""on the agenda"",
          ""ipa"": ""/ɒn ði əˈdʒendə/"",
          ""meaningVi"": ""Nằm trong chương trình họp"",
          ""example"": ""Budget allocation is the first item on the agenda.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""stick to"",
          ""ipa"": ""/stɪk tuː/"",
          ""meaningVi"": ""Bám sát theo"",
          ""example"": ""Meeting chairs must stick to the agenda to respect time.""
        },
        {
          ""text"": ""go through"",
          ""ipa"": ""/ɡəʊ θruː/"",
          ""meaningVi"": ""Đi lần lượt qua"",
          ""example"": ""Let's go through the agenda point by point.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""schedule"",
          ""ipa"": ""/ˈʃedjuːl/"",
          ""meaningVi"": ""Lịch trình"",
          ""example"": ""Review the meeting schedule in advance.""
        },
        {
          ""text"": ""program"",
          ""ipa"": ""/ˈprəʊɡræm/"",
          ""meaningVi"": ""Chương trình"",
          ""example"": ""The summit program is packed with technical sessions.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Circulating an agenda in advance allows participants to prepare thoughtful insights."",
          ""meaningVi"": ""Gửi trước chương trình họp giúp người tham gia chuẩn bị những ý kiến đóng góp sâu sắc.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: conflict"",
      ""centralWord"": ""conflict"",
      ""ipa"": ""/ˈkɒnflɪkt/"",
      ""meaningVi"": ""Sự xung đột, trùng lịch, bất đồng"",
      ""collocations"": [
        {
          ""text"": ""scheduling conflict"",
          ""ipa"": ""/ˈʃedjuːlɪŋ ˈkɒnflɪkt/"",
          ""meaningVi"": ""Trùng lịch họp"",
          ""example"": ""A sudden scheduling conflict forced her to reschedule the demo.""
        },
        {
          ""text"": ""resolve a conflict"",
          ""ipa"": ""/rɪˈzɒlv ə ˈkɒnflɪkt/"",
          ""meaningVi"": ""Giải quyết xung đột"",
          ""example"": ""Transparent communication helps resolve workplace conflicts.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""iron out"",
          ""ipa"": ""/ˈaɪən aʊt/"",
          ""meaningVi"": ""Dàn xếp ổn thỏa"",
          ""example"": ""Let's meet to iron out our differences on architecture.""
        },
        {
          ""text"": ""step in"",
          ""ipa"": ""/step ɪn/"",
          ""meaningVi"": ""Can thiệp giải quyết"",
          ""example"": ""The manager stepped in to resolve the timeline dispute.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""dispute"",
          ""ipa"": ""/dɪˈspjuːt/"",
          ""meaningVi"": ""Bất đồng, tranh chấp"",
          ""example"": ""Both parties mediated the contractual dispute.""
        },
        {
          ""text"": ""clash"",
          ""ipa"": ""/klæʃ/"",
          ""meaningVi"": ""Sự đụng độ, trùng giờ"",
          ""example"": ""A calendar clash prevented him from attending.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Addressing scheduling conflicts politely and promptly shows respect for others' commitments."",
          ""meaningVi"": ""Xử lý xung đột lịch họp một cách lịch thiệp và nhanh chóng thể hiện sự tôn trọng các cam kết của đối tác.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: bottleneck"",
      ""centralWord"": ""bottleneck"",
      ""ipa"": ""/ˈbɒtlnek/"",
      ""meaningVi"": ""Điểm nghẽn tiến độ, nút thắt cổ chai"",
      ""collocations"": [
        {
          ""text"": ""operational bottleneck"",
          ""ipa"": ""/ˌɒpəˈreɪʃənl ˈbɒtlnek/"",
          ""meaningVi"": ""Điểm nghẽn vận hành"",
          ""example"": ""Manual database migration was our primary operational bottleneck.""
        },
        {
          ""text"": ""alleviate bottlenecks"",
          ""ipa"": ""/əˈliːvieɪt ˈbɒtlneks/"",
          ""meaningVi"": ""Giải tỏa các điểm thắt"",
          ""example"": ""Adopting CI/CD pipelines alleviates software release bottlenecks.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""slow down"",
          ""ipa"": ""/sləʊ daʊn/"",
          ""meaningVi"": ""Làm chậm lại"",
          ""example"": ""Unclear technical specifications slow down entire sprints.""
        },
        {
          ""text"": ""weed out"",
          ""ipa"": ""/wiːd aʊt/"",
          ""meaningVi"": ""Loại bỏ"",
          ""example"": ""We streamlined our review steps to weed out bureaucratic delays.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""impediment"",
          ""ipa"": ""/ɪmˈpedɪmənt/"",
          ""meaningVi"": ""Trở ngại, sự cản trở"",
          ""example"": ""Legacy software remains a major impediment to speed.""
        },
        {
          ""text"": ""obstruction"",
          ""ipa"": ""/əbˈstrʌkʃn/"",
          ""meaningVi"": ""Vật cản"",
          ""example"": ""Identify and remove process obstructions early.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Relentlessly diagnosing process bottlenecks is essential for maintaining sustained engineering velocity."",
          ""meaningVi"": ""Kiên trì chẩn đoán các nút thắt quy trình là điều cốt yếu để duy trì vận tốc kỹ thuật ổn định.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: accountability"",
      ""centralWord"": ""accountability"",
      ""ipa"": ""/əˌkaʊntəˈbɪləti/"",
      ""meaningVi"": ""Trách nhiệm giải trình, tính minh bạch"",
      ""collocations"": [
        {
          ""text"": ""mutual accountability"",
          ""ipa"": ""/ˈmjuːtʃuəl əˌkaʊntəˈbɪləti/"",
          ""meaningVi"": ""Trách nhiệm chung hai bên"",
          ""example"": ""High-performing squads thrive on mutual accountability.""
        },
        {
          ""text"": ""hold someone accountable"",
          ""ipa"": ""/həʊld ˈsʌmwʌn əˈkaʊntəbl/"",
          ""meaningVi"": ""Quy trách nhiệm cho ai"",
          ""example"": ""Leadership holds every team lead accountable for quality.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""answer for"",
          ""ipa"": ""/ˈɑːnsə fɔː/"",
          ""meaningVi"": ""Chịu trách nhiệm về"",
          ""example"": ""Each module owner must answer for test coverage metrics.""
        },
        {
          ""text"": ""stand by"",
          ""ipa"": ""/stænd baɪ/"",
          ""meaningVi"": ""Ủng hộ và bảo vệ"",
          ""example"": ""We stand by our deployment commitments to clients.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""responsibility"",
          ""ipa"": ""/rɪˌspɒnsəˈbɪləti/"",
          ""meaningVi"": ""Trách nhiệm"",
          ""example"": ""Shared responsibility builds team solidarity.""
        },
        {
          ""text"": ""answerability"",
          ""ipa"": ""/ˌɑːnsərəˈbɪləti/"",
          ""meaningVi"": ""Nghĩa vụ giải trình"",
          ""example"": ""Corporate governance requires strict answerability.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Institutional accountability bridges the gap between ambitious roadmaps and dependable execution."",
          ""meaningVi"": ""Trách nhiệm giải trình mang tính thể chế thu hẹp khoảng cách giữa lộ trình tham vọng và việc thực thi đáng tin cậy.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: contingency"",
      ""centralWord"": ""contingency"",
      ""ipa"": ""/kənˈtɪndʒənsi/"",
      ""meaningVi"": ""Phương án dự phòng, tình huống bất ngờ"",
      ""collocations"": [
        {
          ""text"": ""contingency plan"",
          ""ipa"": ""/kənˈtɪndʒənsi plæn/"",
          ""meaningVi"": ""Kế hoạch dự phòng rủi ro"",
          ""example"": ""We established a robust contingency plan in case of cloud outages.""
        },
        {
          ""text"": ""contingency fund"",
          ""ipa"": ""/kənˈtɪndʒənsi fʌnd/"",
          ""meaningVi"": ""Quỹ dự phòng khẩn cấp"",
          ""example"": ""Every large construction project allocates a contingency fund.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""fall back on"",
          ""ipa"": ""/fɔːl bæk ɒn/"",
          ""meaningVi"": ""Dựa vào phương án phụ"",
          ""example"": ""If the primary API fails, we fall back on cached local data.""
        },
        {
          ""text"": ""hedge against"",
          ""ipa"": ""/hedʒ əˈɡenst/"",
          ""meaningVi"": ""Phòng ngừa thiệt hại"",
          ""example"": ""Diversified investments hedge against sudden market volatility.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""safeguard"",
          ""ipa"": ""/ˈseɪfɡɑːd/"",
          ""meaningVi"": ""Biện pháp bảo vệ"",
          ""example"": ""Strict encryption is a vital cybersecurity safeguard.""
        },
        {
          ""text"": ""buffer"",
          ""ipa"": ""/ˈbʌfə/"",
          ""meaningVi"": ""Khoảng đệm an toàn"",
          ""example"": ""We added a two-day buffer before the product launch.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Proactive contingency design distinguishes resilient engineering systems from brittle architectures."",
          ""meaningVi"": ""Thiết kế dự phòng chủ động tạo nên sự khác biệt giữa hệ thống kỹ thuật kiên cường và những kiến trúc dễ tổn thương.""
        }
      ]
    },
    {
      ""id"": ""wm-l21-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: reallocation"",
      ""centralWord"": ""reallocation"",
      ""ipa"": ""/ˌriːæləˈkeɪʃn/"",
      ""meaningVi"": ""Sự tái phân bổ, điều chuyển nguồn lực"",
      ""collocations"": [
        {
          ""text"": ""resource reallocation"",
          ""ipa"": ""/rɪˈzɔːs ˌriːæləˈkeɪʃn/"",
          ""meaningVi"": ""Tái phân bổ nguồn lực"",
          ""example"": ""Dynamic resource reallocation resolved the infrastructure crunch.""
        },
        {
          ""text"": ""budgetary reallocation"",
          ""ipa"": ""/ˈbʌdʒɪtri ˌriːæləˈkeɪʃn/"",
          ""meaningVi"": ""Điều chuyển ngân sách"",
          ""example"": ""The executive committee approved a strategic budgetary reallocation.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""shift around"",
          ""ipa"": ""/ʃɪft əˈraʊnd/"",
          ""meaningVi"": ""Luân chuyển qua lại"",
          ""example"": ""We shifted developers around to meet the critical milestone.""
        },
        {
          ""text"": ""channel into"",
          ""ipa"": ""/ˈtʃænl ˈɪntuː/"",
          ""meaningVi"": ""Rót vốn / dồn sức vào"",
          ""example"": ""Surplus capital was channeled into machine learning research.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""redistribution"",
          ""ipa"": ""/ˌriːdɪstrɪˈbjuːʃn/"",
          ""meaningVi"": ""Sự phân phối lại"",
          ""example"": ""Redistribution of computing load improved uptime.""
        },
        {
          ""text"": ""realignment"",
          ""ipa"": ""/ˌriːəˈlaɪnmənt/"",
          ""meaningVi"": ""Sự định hướng lại"",
          ""example"": ""Organizational realignment enhanced cross-team collaboration.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Strategic talent reallocation prevents burn-out while accelerating delivery of critical milestones."",
          ""meaningVi"": ""Tái phân bổ nhân tài chiến lược giúp ngăn ngừa tình trạng kiệt sức đồng thời đẩy nhanh tiến độ bàn giao các mốc quan trọng.""
        }
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.1 Cloze (OrderIndex 15)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444448"), "cloze", 15),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444448"),
            OrderIndex = 15,
            Type = "cloze",
            Title = "Điền từ vào chỗ trống (Bài 2.1): Bài tập ngữ cảnh thực tế",
            Description = "Đọc đoạn văn hoặc câu giao tiếp và điền từ thích hợp vào chỗ trống với gợi ý và giải thích chi tiết.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l21-cloze-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Xác nhận lịch họp qua email (Dễ)"",
      ""promptVi"": ""Điền từ thích hợp để hoàn thành email xác nhận lịch hẹn:"",
      ""template"": ""I am writing to {{0}} our project sync scheduled for tomorrow morning at ten {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""confirm"",
            ""verify""
          ],
          ""hintVi"": ""xác nhận (bắt đầu bằng c)"",
          ""explanationVi"": ""\""writing to confirm\"" là mẫu câu mở đầu chuẩn mực.""
        },
        {
          ""acceptedAnswers"": [
            ""o'clock"",
            ""AM"",
            ""am""
          ],
          ""hintVi"": ""giờ sáng (o'clock hoặc AM)"",
          ""explanationVi"": ""\""at ten o'clock / at ten AM\"" chỉ thời gian cụ thể.""
        }
      ],
      ""fullSentence"": ""I am writing to confirm our project sync scheduled for tomorrow morning at ten o'clock."",
      ""meaningVi"": ""Tôi viết email này để xác nhận buổi họp dự án đã xếp lịch vào 10 giờ sáng mai."",
      ""explanationVi"": ""Mẫu câu mở đầu email xác nhận cuộc họp ngắn gọn và lịch sự.""
    },
    {
      ""id"": ""en-l21-cloze-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Gửi tài liệu đính kèm (Dễ)"",
      ""promptVi"": ""Điền cụm từ đính kèm và mời xem xét:"",
      ""template"": ""Please find {{0}} the quarterly budget report for your thoughtful {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""attached"",
            ""enclosed""
          ],
          ""hintVi"": ""được đính kèm (bắt đầu bằng a)"",
          ""explanationVi"": ""\""Please find attached\"" là mẫu câu đính kèm thông dụng nhất.""
        },
        {
          ""acceptedAnswers"": [
            ""review"",
            ""consideration"",
            ""inspection""
          ],
          ""hintVi"": ""sự xem xét, đánh giá (bắt đầu bằng r)"",
          ""explanationVi"": ""\""for your review\"" nghĩa là để anh/chị xem xét.""
        }
      ],
      ""fullSentence"": ""Please find attached the quarterly budget report for your thoughtful review."",
      ""meaningVi"": ""Xin vui lòng xem báo cáo ngân sách hàng quý đính kèm để xem xét kỹ lưỡng."",
      ""explanationVi"": ""Mẫu câu thông dụng khi gửi file bảng tính hoặc tài liệu cho cấp trên/đối tác.""
    },
    {
      ""id"": ""en-l21-cloze-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Xin dời lịch họp lịch thiệp (Vừa)"",
      ""promptVi"": ""Điền từ về sự cố trùng lịch và đề xuất thời gian:"",
      ""template"": ""Due to a scheduling {{0}}, I must kindly ask to reschedule our call to another available {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""conflict"",
            ""overlap"",
            ""issue""
          ],
          ""hintVi"": ""trùng lịch / xung đột (bắt đầu bằng c)"",
          ""explanationVi"": ""\""scheduling conflict\"" là lý do trang trọng khi dời lịch.""
        },
        {
          ""acceptedAnswers"": [
            ""slot"",
            ""time"",
            ""date"",
            ""timeframe""
          ],
          ""hintVi"": ""khung giờ / thời gian trống (bắt đầu bằng s)"",
          ""explanationVi"": ""\""available slot / time\"" là khung giờ còn trống.""
        }
      ],
      ""fullSentence"": ""Due to a scheduling conflict, I must kindly ask to reschedule our call to another available slot."",
      ""meaningVi"": ""Do bị trùng lịch, tôi xin phép được dời cuộc gọi của chúng ta sang một khung giờ trống khác."",
      ""explanationVi"": ""Văn phong ngoại giao chuẩn chỉnh khi không thể tham gia cuộc họp đã định.""
    },
    {
      ""id"": ""en-l21-cloze-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Báo cáo mốc tiến độ dự án (Vừa)"",
      ""promptVi"": ""Điền từ về mốc tiến độ và thời hạn:"",
      ""template"": ""Our team has completed all critical {{0}} ahead of the contractual {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""milestones"",
            ""deliverables"",
            ""tasks""
          ],
          ""hintVi"": ""các mốc tiến độ / sản phẩm (bắt đầu bằng m)"",
          ""explanationVi"": ""\""critical milestones\"" nghĩa là các mốc tiến độ quan trọng.""
        },
        {
          ""acceptedAnswers"": [
            ""deadline"",
            ""timeline"",
            ""date""
          ],
          ""hintVi"": ""hạn chót hợp đồng (bắt đầu bằng d)"",
          ""explanationVi"": ""\""contractual deadline\"" là thời hạn bàn giao theo hợp đồng.""
        }
      ],
      ""fullSentence"": ""Our team has completed all critical milestones ahead of the contractual deadline."",
      ""meaningVi"": ""Nhóm chúng tôi đã hoàn thành tất cả các mốc tiến độ quan trọng trước thời hạn hợp đồng."",
      ""explanationVi"": ""Mẫu câu báo cáo thành tích sprint xuất sắc tới ban giám đốc.""
    },
    {
      ""id"": ""en-l21-cloze-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Xóa bỏ nút thắt vận hành (Khó)"",
      ""promptVi"": ""Điền các thuật ngữ quản trị về điểm nghẽn và quy trình:"",
      ""template"": ""Automating manual deployments helped eliminate technical {{0}} and restored engineering {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""bottlenecks"",
            ""impediments"",
            ""hurdles""
          ],
          ""hintVi"": ""các điểm nghẽn / nút thắt (bắt đầu bằng b)"",
          ""explanationVi"": ""\""technical bottlenecks\"" là các điểm nghẽn kỹ thuật.""
        },
        {
          ""acceptedAnswers"": [
            ""velocity"",
            ""productivity"",
            ""efficiency""
          ],
          ""hintVi"": ""vận tốc làm việc / năng suất (bắt đầu bằng v)"",
          ""explanationVi"": ""\""engineering velocity\"" là chỉ số tốc độ hoàn thành công việc của đội ngũ kỹ sư.""
        }
      ],
      ""fullSentence"": ""Automating manual deployments helped eliminate technical bottlenecks and restored engineering velocity."",
      ""meaningVi"": ""Tự động hóa các đợt phát hành thủ công đã giúp xóa bỏ các điểm nghẽn kỹ thuật và khôi phục tốc độ hoàn thành công việc của kỹ sư."",
      ""explanationVi"": ""Văn phong cấp cao trong các báo cáo công nghệ và quản trị vận hành.""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.1 Translate (OrderIndex 16)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444448"), "translate", 16),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444448"),
            OrderIndex = 16,
            Type = "translate",
            Title = "Dịch phản xạ Việt → Anh (Bài 2.1): Diễn đạt tự nhiên",
            Description = "Chuyển câu tiếng Việt sang tiếng Anh tự nhiên với nhiều đáp án được chấp nhận và lưu ý ngữ pháp hữu ích.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l21-trans-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Hỏi thời gian rảnh họp nhanh (Dễ)"",
      ""sourceVi"": ""Bạn có tiện dành khoảng 15 phút vào chiều mai để họp nhanh không?"",
      ""hintKeywords"": [
        ""would you be available"",
        ""15 minutes"",
        ""tomorrow afternoon"",
        ""brief sync""
      ],
      ""acceptedAnswers"": [
        ""Would you be available for a brief 15-minute sync tomorrow afternoon?"",
        ""Would you be available for about 15 minutes tomorrow afternoon for a quick meeting?"",
        ""Are you free for a brief 15-minute sync tomorrow afternoon?"",
        ""Would you have 15 minutes tomorrow afternoon for a quick call?""
      ],
      ""sampleAnswer"": ""Would you be available for a brief 15-minute sync tomorrow afternoon?"",
      ""grammarNoteVi"": ""Mẫu câu lịch sự tiêu chuẩn trong môi trường công sở: \""Would you be available for...\"".""
    },
    {
      ""id"": ""en-l21-trans-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Báo cáo đã hoàn thành nhiệm vụ (Dễ)"",
      ""sourceVi"": ""Chúng tôi đã hoàn thành toàn bộ các bài kiểm thử tự động cho bản cập nhật mới."",
      ""hintKeywords"": [
        ""we have completed"",
        ""automated tests"",
        ""new update""
      ],
      ""acceptedAnswers"": [
        ""We have completed all automated tests for the new update."",
        ""We have finished all automated testing for the new release."",
        ""Our team has completed all automated tests for the new update.""
      ],
      ""sampleAnswer"": ""We have completed all automated tests for the new update."",
      ""grammarNoteVi"": ""Dùng thì Hiện tại hoàn thành (\""have completed\"") khi báo cáo kết quả công việc hoàn tất.""
    },
    {
      ""id"": ""en-l21-trans-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Xin lỗi vì sự bất tiện dời lịch (Vừa)"",
      ""sourceVi"": ""Tôi thành thật xin lỗi vì sự bất tiện do việc thông báo dời lịch gấp này gây ra."",
      ""hintKeywords"": [
        ""sincere apologies"",
        ""inconvenience"",
        ""short notice"",
        ""rescheduling""
      ],
      ""acceptedAnswers"": [
        ""I sincerely apologize for the inconvenience caused by this short-notice rescheduling."",
        ""Please accept my sincere apologies for any inconvenience caused by this short notice."",
        ""I am truly sorry for the inconvenience caused by rescheduling on such short notice.""
      ],
      ""sampleAnswer"": ""Please accept my sincere apologies for the inconvenience caused by this short-notice rescheduling."",
      ""grammarNoteVi"": ""Cụm danh từ \""short-notice rescheduling\"" diễn tả việc dời lịch đột xuất rất tinh tế.""
    },
    {
      ""id"": ""en-l21-trans-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Ưu tiên tinh gọn quy trình (Vừa)"",
      ""sourceVi"": ""Chúng tôi khuyến nghị tinh gọn quy trình phê duyệt tài chính để đẩy nhanh tiến độ dự án."",
      ""hintKeywords"": [
        ""recommend streamlining"",
        ""financial approval process"",
        ""accelerate project progress""
      ],
      ""acceptedAnswers"": [
        ""We recommend streamlining the financial approval process to accelerate project progress."",
        ""We suggest streamlining the financial approvals to speed up project delivery."",
        ""Our recommendation is to streamline financial approvals to accelerate project milestones.""
      ],
      ""sampleAnswer"": ""We recommend streamlining the financial approval process to accelerate project progress."",
      ""grammarNoteVi"": ""Sau động từ \""recommend\"", động từ theo sau chia dạng V-ing (\""recommend streamlining\"").""
    },
    {
      ""id"": ""en-l21-trans-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Dịch: Cam kết trách nhiệm giải trình (Khó)"",
      ""sourceVi"": ""Thiết lập cơ chế trách nhiệm giải trình rõ ràng đảm bảo rằng tất cả các bên liên quan đều bám sát theo các mốc tiến độ chiến lược."",
      ""hintKeywords"": [
        ""establishing accountability frameworks"",
        ""stakeholders remain aligned"",
        ""strategic milestones""
      ],
      ""acceptedAnswers"": [
        ""Establishing clear accountability mechanisms ensures that all stakeholders remain aligned with strategic milestones."",
        ""Establishing rigorous accountability frameworks guarantees that all stakeholders stay aligned with strategic milestones.""
      ],
      ""sampleAnswer"": ""Establishing clear accountability mechanisms ensures that all stakeholders remain aligned with strategic milestones."",
      ""grammarNoteVi"": ""Cụm danh động từ chủ ngữ (\""Establishing...\"") đi với động từ số ít (\""ensures\"").""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.1 Grammar Fix (OrderIndex 17)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444448"), "grammar_fix", 17),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444448"),
            OrderIndex = 17,
            Type = "grammar_fix",
            Title = "Sửa lỗi ngữ pháp (Bài 2.1): Khắc phục các lỗi người Việt hay mắc",
            Description = "Phát hiện và sửa các lỗi sai ngữ pháp, thì động từ, mạo từ và giới từ thường gặp trong giao tiếp thực tế.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l21-gf-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Giới từ chỉ ngày và giờ (Dễ)"",
      ""incorrectSentence"": ""Could we reschedule our sync at Friday on 3 PM?"",
      ""errorTypeHintVi"": ""Dùng giới từ \""on\"" cho ngày trong tuần và \""at\"" cho mốc thời gian cụ thể."",
      ""correctedSentence"": ""Could we reschedule our sync to Friday at 3 PM?"",
      ""acceptedAnswers"": [
        ""Could we reschedule our sync to Friday at 3 PM?"",
        ""Could we reschedule our meeting to Friday at 3 PM?""
      ],
      ""meaningVi"": ""Chúng ta có thể dời buổi họp sang thứ Sáu lúc 3 giờ chiều được không?"",
      ""explanationVi"": ""Dời lịch sang ngày nào dùng \""reschedule to + Ngày\"" và giờ dùng \""at + Giờ\"".""
    },
    {
      ""id"": ""en-l21-gf-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Dạng của động từ sau recommend (Dễ)"",
      ""incorrectSentence"": ""I recommend to review the attached spreadsheet before the call."",
      ""errorTypeHintVi"": ""Động từ theo sau \""recommend\"" trực tiếp phải ở dạng V-ing hoặc mệnh đề (that S + V-inf)."",
      ""correctedSentence"": ""I recommend reviewing the attached spreadsheet before the call."",
      ""acceptedAnswers"": [
        ""I recommend reviewing the attached spreadsheet before the call."",
        ""I recommend that you review the attached spreadsheet before the call.""
      ],
      ""meaningVi"": ""Tôi khuyến nghị nên xem lại bảng tính đính kèm trước cuộc gọi."",
      ""explanationVi"": ""Cấu trúc chuẩn: \""recommend doing something\"" hoặc \""recommend that someone do something\"".""
    },
    {
      ""id"": ""en-l21-gf-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Thiếu to be trong câu bị động (Vừa)"",
      ""incorrectSentence"": ""All weekly deliverables already finalized by the engineering department."",
      ""errorTypeHintVi"": ""Câu bị động ở thì hiện tại hoàn thành bắt buộc phải có trợ động từ \""have been finalized\""."",
      ""correctedSentence"": ""All weekly deliverables have already been finalized by the engineering department."",
      ""acceptedAnswers"": [
        ""All weekly deliverables have already been finalized by the engineering department."",
        ""All weekly deliverables were already finalized by the engineering department.""
      ],
      ""meaningVi"": ""Tất cả các sản phẩm bàn giao tuần đã được bộ phận kỹ thuật hoàn thiện."",
      ""explanationVi"": ""Cấu trúc bị động thì hiện tại hoàn thành: \""have/has + been + V3/ed\"".""
    },
    {
      ""id"": ""en-l21-gf-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Dùng sai mạo từ và tính từ sở hữu (Vừa)"",
      ""incorrectSentence"": ""Please find attached a document that summarizes our team progress."",
      ""errorTypeHintVi"": ""Cần thêm sở hữu cách hoặc tính từ: \""our team's progress\"" thay vì đặt danh từ trơ trọi."",
      ""correctedSentence"": ""Please find attached the document that summarizes our team's progress."",
      ""acceptedAnswers"": [
        ""Please find attached the document that summarizes our team's progress."",
        ""Please find attached a document that summarizes our team's progress.""
      ],
      ""meaningVi"": ""Xin vui lòng xem tài liệu đính kèm tóm tắt tiến độ của nhóm chúng tôi."",
      ""explanationVi"": ""Dùng sở hữu cách \""team's progress\"" để chỉ tiến độ của tập thể.""
    },
    {
      ""id"": ""en-l21-gf-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Sửa lỗi: Cấu trúc câu điều kiện đảo ngữ (Khó)"",
      ""incorrectSentence"": ""Should you will have any further inquiries, do not hesitate to contact our office."",
      ""errorTypeHintVi"": ""Trong mệnh đề đảo ngữ với \""Should\"" thay cho \""If\"", động từ theo sau luôn ở dạng nguyên mẫu không to (Should you have)."",
      ""correctedSentence"": ""Should you have any further inquiries, do not hesitate to contact our office."",
      ""acceptedAnswers"": [
        ""Should you have any further inquiries, do not hesitate to contact our office."",
        ""If you have any further inquiries, do not hesitate to contact our office.""
      ],
      ""meaningVi"": ""Nếu bạn có thêm bất kỳ thắc mắc nào, đừng ngần ngại liên hệ với văn phòng của chúng tôi."",
      ""explanationVi"": ""\""Should you have...\"" là dạng đảo ngữ trang trọng của \""If you have...\"", tuyệt đối không dùng \""will\"" sau \""Should\"".""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.1 Roleplay (OrderIndex 18)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444448"), "roleplay", 18),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444448"),
            OrderIndex = 18,
            Type = "roleplay",
            Title = "Nhập vai hội thoại (Bài 2.1): Tình huống phản xạ thực chiến",
            Description = "Nhập vai giao tiếp với AI trong các tình huống thực tế, hỗ trợ chấm điểm tự động theo từ khóa và cấu trúc câu.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l21-rp-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Xác nhận lịch họp dự án qua email/chat (Dễ)"",
      ""scenarioVi"": ""Quản lý dự án vừa gửi email đề xuất họp cập nhật tiến độ vào sáng mai lúc 10:00 AM."",
      ""aiRole"": ""Robert (Quản lý dự án)"",
      ""aiPrompt"": ""Good morning! Could we schedule our weekly progress check-in for tomorrow at 10:00 AM in meeting room B?"",
      ""aiPromptVi"": ""Chào buổi sáng! Chúng ta có thể xếp lịch họp tiến độ tuần vào sáng mai lúc 10:00 tại phòng họp B không?"",
      ""userGoalVi"": ""Đồng ý với khung giờ và địa điểm, xác nhận bạn sẽ chuẩn bị sẵn tài liệu báo cáo sprint."",
      ""expectedKeywords"": [
        ""confirm"",
        ""tomorrow"",
        ""10:00 AM"",
        ""meeting room"",
        ""report"",
        ""prepare""
      ],
      ""sampleResponses"": [
        ""Good morning, Robert! I am pleased to confirm our meeting tomorrow at 10:00 AM in room B. I will prepare our sprint progress report in advance.""
      ],
      ""minWords"": 10
    },
    {
      ""id"": ""en-l21-rp-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Báo cáo tiến độ hoàn thành tính năng (Dễ)"",
      ""scenarioVi"": ""Trưởng nhóm kỹ thuật hỏi về tình trạng module xác thực người dùng mà bạn phụ trách."",
      ""aiRole"": ""Robert (Quản lý dự án)"",
      ""aiPrompt"": ""Hi Mai, could you give me a quick status update on the user authentication module?"",
      ""aiPromptVi"": ""Chào Mai, bạn có thể cho tôi biết cập nhật nhanh về tình trạng module xác thực người dùng không?"",
      ""userGoalVi"": ""Báo cáo module đã hoàn thành kiểm thử, sẵn sàng triển khai đúng hạn vào thứ Sáu."",
      ""expectedKeywords"": [
        ""completed"",
        ""testing"",
        ""deployed"",
        ""on track"",
        ""Friday""
      ],
      ""sampleResponses"": [
        ""Hi Robert! We have successfully completed all automated tests for the authentication module. Everything is on track for deployment this Friday.""
      ],
      ""minWords"": 10
    },
    {
      ""id"": ""en-l21-rp-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Xin dời lịch họp vì sự cố kỹ thuật (Vừa)"",
      ""scenarioVi"": ""Hệ thống máy chủ gặp sự cố khẩn cấp, bạn cần liên hệ đối tác để dời cuộc họp chiều nay lúc 2:00 PM."",
      ""aiRole"": ""Ms. Anderson (Khách hàng đối tác)"",
      ""aiPrompt"": ""Hello! Looking forward to our scheduled demo at 2:00 PM today. Will your team be ready?"",
      ""aiPromptVi"": ""Xin chào! Rất mong đợi buổi thị phạm sản phẩm lúc 2:00 chiều nay. Nhóm bạn đã sẵn sàng chưa?"",
      ""userGoalVi"": ""Xin lỗi lịch thiệp, giải thích có sự cố máy chủ khẩn cấp và đề xuất 2 khung giờ thay thế vào ngày mai."",
      ""expectedKeywords"": [
        ""apologize"",
        ""urgent incident"",
        ""reschedule"",
        ""tomorrow"",
        ""alternative slot""
      ],
      ""sampleResponses"": [
        ""Dear Ms. Anderson, please accept my sincere apologies. Due to an urgent server incident, could we reschedule our demo to tomorrow morning at 10:00 AM or 2:00 PM?""
      ],
      ""minWords"": 14
    },
    {
      ""id"": ""en-l21-rp-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Thương lượng điều chỉnh phạm vi công việc (Vừa)"",
      ""scenarioVi"": ""Khách hàng yêu cầu bổ sung 3 tính năng mới ngay trước ngày bàn giao sản phẩm."",
      ""aiRole"": ""Robert (Quản lý dự án)"",
      ""aiPrompt"": ""The client just requested three additional payment integrations for this Friday. Can we squeeze them into this release?"",
      ""aiPromptVi"": ""Khách hàng vừa yêu cầu tích hợp thêm 3 cổng thanh toán mới cho thứ Sáu này. Chúng ta có thể nhồi thêm vào đợt phát hành này không?"",
      ""userGoalVi"": ""Phản hồi ngoại giao rằng làm gấp sẽ ảnh hưởng chất lượng, đề xuất đưa vào sprint tiếp theo để đảm bảo ổn định."",
      ""expectedKeywords"": [
        ""quality"",
        ""risk"",
        ""next sprint"",
        ""priority"",
        ""stabilize"",
        ""recommend""
      ],
      ""sampleResponses"": [
        ""I understand the client's enthusiasm, but adding three integrations now jeopardizes system stability. I strongly recommend delivering our core milestone this Friday and prioritizing those features in the next sprint.""
      ],
      ""minWords"": 14
    },
    {
      ""id"": ""en-l21-rp-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Báo cáo sự cố trễ hạn lên ban giám đốc (Khó)"",
      ""scenarioVi"": ""Do nhà cung cấp dịch vụ đám mây bên thứ ba bị sập, một mốc tiến độ quan trọng bị trễ 2 ngày. Bạn báo cáo trực tiếp với Giám đốc Công nghệ."",
      ""aiRole"": ""CTO (Giám đốc Công nghệ)"",
      ""aiPrompt"": ""I noticed the staging deployment pipeline is delayed. What caused this bottleneck, and what is our contingency plan?"",
      ""aiPromptVi"": ""Tôi nhận thấy quy trình triển khai môi trường staging đang bị chậm. Điều gì gây ra điểm nghẽn này, và phương án dự phòng của chúng ta là gì?"",
      ""userGoalVi"": ""Trình bày nguyên nhân khách quan từ nhà cung cấp, nêu rõ nhóm đã kích hoạt giải pháp dự phòng và cam kết hoàn thành trong 48 giờ."",
      ""expectedKeywords"": [
        ""bottleneck"",
        ""third-party"",
        ""contingency plan"",
        ""mitigate"",
        ""resolved"",
        ""accountability""
      ],
      ""sampleResponses"": [
        ""An unexpected outage from our third-party cloud provider caused the temporary bottleneck. We have immediately activated our local containerized contingency plan, and the team will have the deployment fully verified within forty-eight hours.""
      ],
      ""minWords"": 16
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.2 Shadowing (OrderIndex 11)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444449"), "shadowing", 11),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444449"),
            OrderIndex = 11,
            Type = "shadowing",
            Title = "Shadowing mở rộng (Bài 2.2): 15 câu đa cấp độ Dễ → Vừa → Khó",
            Description = "Luyện nói đuổi theo ngữ điệu người bản xứ, tập trung vào nối âm, ngữ điệu và phát âm chuẩn các âm then chốt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l22-shad-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Kiểm tra kỹ thuật họp trực tuyến (Dễ)"",
      ""sentence"": ""Can everyone hear me clearly and see my presentation slides?"",
      ""ipa"": ""/kæn ˈevriwʌn hɪə miː ˈklɪəli ænd siː maɪ ˌpreznˈteɪʃn slaɪdz/"",
      ""meaningVi"": ""Mọi người có nghe rõ tiếng và nhìn thấy slide thuyết trình của tôi không?"",
      ""keyPhrases"": [
        ""hear me clearly"",
        ""see presentation slides"",
        ""can everyone""
      ]
    },
    {
      ""id"": ""en-l22-shad-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Mời trình bày ý kiến (Dễ)"",
      ""sentence"": ""Thank you for joining today, please go ahead with your update."",
      ""ipa"": ""/θæŋk juː fɔː ˈdʒɔɪnɪŋ təˈdeɪ, pliːz ɡəʊ əˈhed wɪð jɔːr ˌʌpˈdeɪt/"",
      ""meaningVi"": ""Cảm ơn các bạn đã tham gia hôm nay, xin mời bạn trình bày phần cập nhật."",
      ""keyPhrases"": [
        ""thank you for joining"",
        ""go ahead"",
        ""with your update""
      ]
    },
    {
      ""id"": ""en-l22-shad-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Đồng thuận quan điểm (Dễ)"",
      ""sentence"": ""I completely agree with the proposed project timeline."",
      ""ipa"": ""/aɪ kəmˈpliːtli əˈɡriː wɪð ðə prəˈpəʊzd ˈprɒdʒekt ˈtaɪmlaɪn/"",
      ""meaningVi"": ""Tôi hoàn toàn đồng ý với mốc tiến độ dự án được đề xuất."",
      ""keyPhrases"": [
        ""completely agree"",
        ""proposed timeline"",
        ""project timeline""
      ]
    },
    {
      ""id"": ""en-l22-shad-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Nhắc nhở bật mic (Dễ)"",
      ""sentence"": ""Alex, you are on mute, could you unmute your microphone?"",
      ""ipa"": ""/ˈælɪks, juː ɑːr ɒn mjuːt, kəd juː ʌnˈmjuːt jɔː ˈmaɪkrəfəʊn/"",
      ""meaningVi"": ""Alex ơi bạn đang tắt mic, bạn có thể bật mic lên được không?"",
      ""keyPhrases"": [
        ""on mute"",
        ""unmute microphone"",
        ""could you""
      ]
    },
    {
      ""id"": ""en-l22-shad-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Chốt thời gian kết thúc (Dễ)"",
      ""sentence"": ""Let us wrap up today's meeting and review our action points."",
      ""ipa"": ""/let ʌs ræp ʌp təˈdeɪz ˈmiːtɪŋ ænd rɪˈvjuː ˈaʊər ˈækʃn pɔɪnts/"",
      ""meaningVi"": ""Chúng ta hãy khép lại cuộc họp hôm nay và xem lại các đầu việc cần làm."",
      ""keyPhrases"": [
        ""wrap up meeting"",
        ""review action points""
      ]
    },
    {
      ""id"": ""en-l22-shad-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Phản biện ngoại giao lịch thiệp (Vừa)"",
      ""sentence"": ""I see your point regarding speed, but we must also consider long-term software maintenance costs."",
      ""ipa"": ""/aɪ siː jɔː pɔɪnt rɪˈɡɑːdɪŋ spiːd, bʌt wiː mʌst ˈɔːlsəʊ kənˈsɪdə ˈlɒŋ tɜːm ˈsɒftweə ˈmeɪntənəns kɒsts/"",
      ""meaningVi"": ""Tôi hiểu ý kiến của bạn về tốc độ, nhưng chúng ta cũng phải cân nhắc chi phí bảo trì phần mềm lâu dài."",
      ""keyPhrases"": [
        ""I see your point"",
        ""long-term maintenance"",
        ""must consider""
      ]
    },
    {
      ""id"": ""en-l22-shad-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đề xuất thảo luận riêng (Vừa)"",
      ""sentence"": ""That is a very detailed technical question, so let us take it offline after this alignment call."",
      ""ipa"": ""/ðæt ɪz ə ˈveri ˈdiːteɪld ˈteknɪkl ˈkwestʃən, səʊ let ʌs teɪk ɪt ɒfˈlaɪn ˈɑːftə ðɪs əˈlaɪnmənt kɔːl/"",
      ""meaningVi"": ""Đó là một câu hỏi kỹ thuật rất chi tiết, vậy chúng ta hãy trao đổi riêng sau buổi họp này nhé."",
      ""keyPhrases"": [
        ""detailed technical question"",
        ""take it offline"",
        ""alignment call""
      ]
    },
    {
      ""id"": ""en-l22-shad-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Trình bày sơ đồ kiến trúc (Vừa)"",
      ""sentence"": ""Allow me to walk you through our proposed cloud migration strategy step by step."",
      ""ipa"": ""/əˈlaʊ miː tuː wɔːk juː θruː ˈaʊə prəˈpəʊzd klaʊd maɪˈɡreɪʃn ˈstrætədʒi step baɪ step/"",
      ""meaningVi"": ""Cho phép tôi hướng dẫn các bạn từng bước về chiến lược chuyển đổi lên đám mây được đề xuất."",
      ""keyPhrases"": [
        ""walk you through"",
        ""cloud migration"",
        ""step by step""
      ]
    },
    {
      ""id"": ""en-l22-shad-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đạt được sự đồng thuận chung (Vừa)"",
      ""sentence"": ""Do we have consensus on adopting this microservices pattern for our upcoming release?"",
      ""ipa"": ""/duː wiː hæv kənˈsensəs ɒn əˈdɒptɪŋ ðɪs ˈmaɪkrəʊˌsɜːvɪsɪz ˈpætn fɔːr ˈaʊər ˈʌpˌkʌmɪŋ rɪˈliːs/"",
      ""meaningVi"": ""Chúng ta đã có sự đồng thuận về việc áp dụng mô hình microservices này cho bản phát hành sắp tới chưa?"",
      ""keyPhrases"": [
        ""have consensus"",
        ""adopting pattern"",
        ""upcoming release""
      ]
    },
    {
      ""id"": ""en-l22-shad-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Phân công người phụ trách (Vừa)"",
      ""sentence"": ""Let us assign clear ownership for each deliverable to ensure complete team accountability."",
      ""ipa"": ""/let ʌs əˈsaɪn klɪər ˈəʊnəʃɪp fɔːr iːtʃ dɪˈlɪvərəbl tuː ɪnˈʃʊə kəmˈpliːt tiːm əˌkaʊntəˈbɪləti/"",
      ""meaningVi"": ""Chúng ta hãy phân công người chịu trách nhiệm rõ ràng cho từng sản phẩm để bảo đảm trách nhiệm giải trình tuyệt đối."",
      ""keyPhrases"": [
        ""assign clear ownership"",
        ""each deliverable"",
        ""team accountability""
      ]
    },
    {
      ""id"": ""en-l22-shad-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Tháo gỡ bất đồng chuyên môn sâu (Khó)"",
      ""sentence"": ""Constructive dissent during technical design deliberations fundamentally prevents catastrophic architectural regressions."",
      ""ipa"": ""/kənˈstrʌktɪv dɪˈsent ˈdjʊərɪŋ ˈteknɪkl dɪˈzaɪn dɪˌlɪbəˈreɪʃnz ˌfʌndəˈmentəli prɪˈvents ˌkætəˈstrɒfɪk ˌɑːkɪˈtektʃərəl rɪˈɡreʃnz/"",
      ""meaningVi"": ""Sự phản biện mang tính xây dựng trong các buổi thảo luận thiết kế kỹ thuật ngăn ngừa tận gốc những sự thụt lùi kiến trúc thảm khốc."",
      ""keyPhrases"": [
        ""constructive dissent"",
        ""design deliberations"",
        ""architectural regressions""
      ]
    },
    {
      ""id"": ""en-l22-shad-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Dàn xếp thỏa hiệp đa bên (Khó)"",
      ""sentence"": ""Balancing aggressive commercial deadlines with stringent compliance mandates requires nuanced cross-functional alignment."",
      ""ipa"": ""/ˈbælənsɪŋ əˈɡresɪv kəˈmɜːʃl ˈdedlaɪnz wɪð ˈstrɪndʒənt kəmˈplaɪəns ˈmændeɪts rɪˈkwaɪəz ˈnjuːɑːnst krɒs ˈfʌŋkʃənl əˈlaɪnmənt/"",
      ""meaningVi"": ""Cân bằng giữa các thời hạn thương mại gấp gáp với các quy chuẩn tuân thủ nghiêm ngặt đòi hỏi sự đồng thuận liên phòng ban tinh tế."",
      ""keyPhrases"": [
        ""commercial deadlines"",
        ""compliance mandates"",
        ""cross-functional alignment""
      ]
    },
    {
      ""id"": ""en-l22-shad-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Phân tích thỏa hiệp kỹ thuật (Khó)"",
      ""sentence"": ""While microservices provide organizational autonomy, they introduce substantial distributed tracing and latency trade-offs."",
      ""ipa"": ""/waɪl ˈmaɪkrəʊˌsɜːvɪsɪz prəˈvaɪd ˌɔːɡənaɪˈzeɪʃənl ɔːˈtɒnəmi, ðeɪ ˌɪntrəˈdjuːs səbˈstænʃl dɪˈstrɪbjuːtɪd ˈtreɪsɪŋ ænd ˈleɪtənsi ˈtreɪd ɒfs/"",
      ""meaningVi"": ""Dù microservices mang lại quyền tự chủ tổ chức, chúng lại tạo ra sự đánh đổi lớn về độ trễ và việc truy vết phân tán."",
      ""keyPhrases"": [
        ""organizational autonomy"",
        ""distributed tracing"",
        ""latency trade-offs""
      ]
    },
    {
      ""id"": ""en-l22-shad-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Điều phối cuộc họp đa quốc gia (Khó)"",
      ""sentence"": ""Seasoned meeting facilitators deftly synthesize disparate stakeholder opinions into cohesive operational roadmaps."",
      ""ipa"": ""/ˈsiːznd ˈmiːtɪŋ fəˈsɪlɪteɪtəz ˈdeftli ˈsɪnθəsaɪz ˈdɪspərət ˈsteɪkhəʊldər əˈpɪnjənz ˈɪntuː kəʊˈhiːsɪv ˌɒpəˈreɪʃənl ˈrəʊdmæps/"",
      ""meaningVi"": ""Các điều phối viên cuộc họp dày dạn kinh nghiệm tổng hợp khéo léo các ý kiến bất đồng thành lộ trình vận hành gắn kết."",
      ""keyPhrases"": [
        ""seasoned facilitators"",
        ""synthesize opinions"",
        ""cohesive operational roadmaps""
      ]
    },
    {
      ""id"": ""en-l22-shad-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Quyết định dựa trên dữ liệu thực chứng (Khó)"",
      ""sentence"": ""Empirical benchmark telemetry convincingly demonstrated that asynchronous messaging alleviated our peak throughput bottleneck."",
      ""ipa"": ""/ɪmˈpɪrɪkl ˈbentʃmɑːk təˈlemətri kənˈvɪnsɪŋli ˈdemənstreɪtɪd ðæt eɪˈsɪŋkrənəs ˈmesɪdʒɪŋ əˈliːvieɪtɪd ˈaʊə piːk ˈθruːpʊt ˈbɒtlnek/"",
      ""meaningVi"": ""Dữ liệu đo đạc thực nghiệm đã chứng minh một cách thuyết phục rằng cơ chế gửi tin nhắn bất đồng bộ đã giải tỏa nút thắt lưu lượng đỉnh của chúng ta."",
      ""keyPhrases"": [
        ""empirical telemetry"",
        ""asynchronous messaging"",
        ""peak throughput bottleneck""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.2 Dictation (OrderIndex 12)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444449"), "dictation", 12),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444449"),
            OrderIndex = 12,
            Type = "dictation",
            Title = "Dictation mở rộng (Bài 2.2): 15 câu nghe chép chính tả phản xạ",
            Description = "Nghe câu hội thoại đa cấp độ và gõ lại chính xác từng từ hoặc chọn word chip để rèn luyện thính giác tiếng Anh.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l22-dict-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Kiểm tra âm thanh cuộc họp (Dễ)"",
      ""sentence"": ""Can you hear my voice clearly over the conference audio?"",
      ""ipa"": ""/kæn juː hɪə maɪ vɔɪs ˈklɪəli ˈəʊvə ðə ˈkɒnfrəns ˈɔːdiəʊ/"",
      ""meaningVi"": ""Bạn có nghe rõ giọng của tôi qua đường truyền âm thanh hội nghị không?"",
      ""wordChips"": [
        ""Can"",
        ""you"",
        ""hear"",
        ""my"",
        ""voice"",
        ""clearly"",
        ""over"",
        ""the"",
        ""conference"",
        ""audio""
      ],
      ""distractors"": [
        ""sound"",
        ""listen"",
        ""call"",
        ""speaker""
      ]
    },
    {
      ""id"": ""en-l22-dict-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Chia sẻ màn hình làm việc (Dễ)"",
      ""sentence"": ""I am sharing my screen to show the updated project roadmap."",
      ""ipa"": ""/aɪ æm ˈʃeərɪŋ maɪ skriːn tuː ʃəʊ ði ˌʌpˈdeɪtɪd ˈprɒdʒekt ˈrəʊdmæp/"",
      ""meaningVi"": ""Tôi đang chia sẻ màn hình để hiển thị lộ trình dự án đã cập nhật."",
      ""wordChips"": [
        ""I"",
        ""am"",
        ""sharing"",
        ""my"",
        ""screen"",
        ""to"",
        ""show"",
        ""the"",
        ""updated"",
        ""project"",
        ""roadmap""
      ],
      ""distractors"": [
        ""showing"",
        ""display"",
        ""slides"",
        ""timeline""
      ]
    },
    {
      ""id"": ""en-l22-dict-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Đồng thuận phương án (Dễ)"",
      ""sentence"": ""Does everyone agree with the proposed release schedule?"",
      ""ipa"": ""/dʌz ˈevriwʌn əˈɡriː wɪð ðə prəˈpəʊzd rɪˈliːs ˈʃedjuːl/"",
      ""meaningVi"": ""Mọi người có đồng ý với lịch trình phát hành được đề xuất không?"",
      ""wordChips"": [
        ""Does"",
        ""everyone"",
        ""agree"",
        ""with"",
        ""the"",
        ""proposed"",
        ""release"",
        ""schedule""
      ],
      ""distractors"": [
        ""is"",
        ""accept"",
        ""plan"",
        ""date""
      ]
    },
    {
      ""id"": ""en-l22-dict-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Yêu cầu nhắc lại (Dễ)"",
      ""sentence"": ""Could you please repeat that last point once more?"",
      ""ipa"": ""/kəd juː pliːz rɪˈpiːt ðæt lɑːst pɔɪnt wʌns mɔː/"",
      ""meaningVi"": ""Bạn có thể nhắc lại ý cuối cùng đó thêm một lần nữa được không?"",
      ""wordChips"": [
        ""Could"",
        ""you"",
        ""please"",
        ""repeat"",
        ""that"",
        ""last"",
        ""point"",
        ""once"",
        ""more""
      ],
      ""distractors"": [
        ""can"",
        ""say"",
        ""again"",
        ""sentence""
      ]
    },
    {
      ""id"": ""en-l22-dict-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Ghi chép biên bản (Dễ)"",
      ""sentence"": ""I will take meeting notes and send them by email."",
      ""ipa"": ""/aɪ wɪl teɪk ˈmiːtɪŋ nəʊts ænd send ðem baɪ ˈiːmeɪl/"",
      ""meaningVi"": ""Tôi sẽ ghi chép biên bản cuộc họp và gửi qua email."",
      ""wordChips"": [
        ""I"",
        ""will"",
        ""take"",
        ""meeting"",
        ""notes"",
        ""and"",
        ""send"",
        ""them"",
        ""by"",
        ""email""
      ],
      ""distractors"": [
        ""make"",
        ""minutes"",
        ""share"",
        ""write""
      ]
    },
    {
      ""id"": ""en-l22-dict-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đề xuất giải pháp thay thế (Vừa)"",
      ""sentence"": ""While I appreciate your proposal, could we explore a less costly alternative?"",
      ""ipa"": ""/waɪl aɪ əˈpriːʃieɪt jɔː prəˈpəʊzl, kəd wiː ɪkˈsplɔːr ə les ˈkɒstli ɔːlˈtɜːnətɪv/"",
      ""meaningVi"": ""Dù tôi đánh giá cao đề xuất của bạn, liệu chúng ta có thể tìm hiểu một phương án ít tốn kém hơn không?"",
      ""wordChips"": [
        ""While"",
        ""I"",
        ""appreciate"",
        ""your"",
        ""proposal"",
        ""could"",
        ""we"",
        ""explore"",
        ""a"",
        ""less"",
        ""costly"",
        ""alternative""
      ],
      ""distractors"": [
        ""although"",
        ""idea"",
        ""cheaper"",
        ""solution""
      ]
    },
    {
      ""id"": ""en-l22-dict-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Tạm gác nội dung thảo luận (Vừa)"",
      ""sentence"": ""Let us table this detailed discussion until we receive official user feedback."",
      ""ipa"": ""/let ʌs ˈteɪbl ðɪs ˈdiːteɪld dɪˈskʌʃn ənˈtɪl wiː rɪˈsiːv əˈfɪʃl ˈjuːzə ˈfiːdbæk/"",
      ""meaningVi"": ""Chúng ta hãy tạm gác cuộc thảo luận chi tiết này cho đến khi nhận được phản hồi chính thức từ người dùng."",
      ""wordChips"": [
        ""Let"",
        ""us"",
        ""table"",
        ""this"",
        ""detailed"",
        ""discussion"",
        ""until"",
        ""we"",
        ""receive"",
        ""official"",
        ""user"",
        ""feedback""
      ],
      ""distractors"": [
        ""postpone"",
        ""meeting"",
        ""get"",
        ""results""
      ]
    },
    {
      ""id"": ""en-l22-dict-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Trình bày logic giải pháp (Vừa)"",
      ""sentence"": ""Mai will walk us through the architectural blueprint slide by slide."",
      ""ipa"": ""/maɪ wɪl wɔːk ʌs θruː ði ˌɑːkɪˈtektʃərəl ˈbluːprɪnt slaɪd baɪ slaɪd/"",
      ""meaningVi"": ""Mai sẽ hướng dẫn chúng ta qua bản thiết kế kiến trúc từng slide một."",
      ""wordChips"": [
        ""Mai"",
        ""will"",
        ""walk"",
        ""us"",
        ""through"",
        ""the"",
        ""architectural"",
        ""blueprint"",
        ""slide"",
        ""by"",
        ""slide""
      ],
      ""distractors"": [
        ""show"",
        ""guide"",
        ""diagram"",
        ""page""
      ]
    },
    {
      ""id"": ""en-l22-dict-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Gán trách nhiệm đầu việc (Vừa)"",
      ""sentence"": ""We need to assign clear ownership for each deliverable before adjourning."",
      ""ipa"": ""/wiː niːd tuː əˈsaɪn klɪər ˈəʊnəʃɪp fɔːr iːtʃ dɪˈlɪvərəbl bɪˈfɔːr əˈdʒɜːnɪŋ/"",
      ""meaningVi"": ""Chúng ta cần phân công người phụ trách rõ ràng cho từng sản phẩm trước khi kết thúc cuộc họp."",
      ""wordChips"": [
        ""We"",
        ""need"",
        ""to"",
        ""assign"",
        ""clear"",
        ""ownership"",
        ""for"",
        ""each"",
        ""deliverable"",
        ""before"",
        ""adjourning""
      ],
      ""distractors"": [
        ""give"",
        ""responsibility"",
        ""task"",
        ""ending""
      ]
    },
    {
      ""id"": ""en-l22-dict-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đánh giá năng lực xử lý (Vừa)"",
      ""sentence"": ""Does our engineering squad have sufficient bandwidth to take on extra tickets?"",
      ""ipa"": ""/dʌz ˈaʊər ˌendʒɪˈnɪərɪŋ skwɒd hæv səˈfɪʃnt ˈbændwɪdθ tuː teɪk ɒn ˈekstrə ˈtɪkɪts/"",
      ""meaningVi"": ""Đội ngũ kỹ sư của chúng ta có đủ thời gian và năng lực để nhận thêm các đầu việc không?"",
      ""wordChips"": [
        ""Does"",
        ""our"",
        ""engineering"",
        ""squad"",
        ""have"",
        ""sufficient"",
        ""bandwidth"",
        ""to"",
        ""take"",
        ""on"",
        ""extra"",
        ""tickets""
      ],
      ""distractors"": [
        ""team"",
        ""enough"",
        ""time"",
        ""tasks""
      ]
    },
    {
      ""id"": ""en-l22-dict-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Đạt đồng thuận sau tranh luận (Khó)"",
      ""sentence"": ""Vigorous technical debate ultimately crystallized into a solid, unanimous consensus regarding microservices decoupling."",
      ""ipa"": ""/ˈvɪɡərəs ˈteknɪkl dɪˈbeɪt ˈʌltɪmətli ˈkrɪstəlaɪzd ˈɪntuː ə ˈsɒlɪd, juːˈnænɪməs kənˈsensəs rɪˈɡɑːdɪŋ ˈmaɪkrəʊˌsɜːvɪsɪz dɪˈkʌplɪŋ/"",
      ""meaningVi"": ""Cuộc tranh luận kỹ thuật sôi nổi cuối cùng đã kết tinh thành sự đồng thuận vững chắc và nhất trí về việc phân tách microservices."",
      ""wordChips"": [
        ""Vigorous"",
        ""technical"",
        ""debate"",
        ""ultimately"",
        ""crystallized"",
        ""into"",
        ""a"",
        ""solid"",
        ""unanimous"",
        ""consensus"",
        ""regarding"",
        ""microservices"",
        ""decoupling""
      ],
      ""distractors"": [
        ""strong"",
        ""discussion"",
        ""agreement"",
        ""separation""
      ]
    },
    {
      ""id"": ""en-l22-dict-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Thấu hiểu sự đánh đổi kiến trúc (Khó)"",
      ""sentence"": ""Every architectural choice entails profound trade-offs between instantaneous scalability and operational complexity."",
      ""ipa"": ""/ˈevri ˌɑːkɪˈtektʃərəl tʃɔɪs ɪnˈteɪlz prəˈfaʊnd ˈtreɪd ɒfs bɪˈtwiːn ˌɪnstənˈteɪniəs ˌskeɪləˈbɪləti ænd ˌɒpəˈreɪʃənl kəmˈpleksəti/"",
      ""meaningVi"": ""Mỗi lựa chọn kiến trúc đều kéo theo sự đánh đổi sâu sắc giữa khả năng mở rộng tức thì và độ phức tạp vận hành."",
      ""wordChips"": [
        ""Every"",
        ""architectural"",
        ""choice"",
        ""entails"",
        ""profound"",
        ""trade-offs"",
        ""between"",
        ""instantaneous"",
        ""scalability"",
        ""and"",
        ""operational"",
        ""complexity""
      ],
      ""distractors"": [
        ""decision"",
        ""involves"",
        ""compromises"",
        ""system""
      ]
    },
    {
      ""id"": ""en-l22-dict-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Điều phối đa bên hài hòa (Khó)"",
      ""sentence"": ""Diplomatic facilitators harmonize conflicting departmental objectives to safeguard collective organizational alignment."",
      ""ipa"": ""/ˌdɪpləˈmætɪk fəˈsɪlɪteɪtəz ˈhɑːmənaɪz kənˈflɪktɪŋ ˌdiːpɑːtˈmentl əbˈdʒektɪvz tuː ˈseɪfɡɑːd kəˈlektɪv ˌɔːɡənaɪˈzeɪʃənl əˈlaɪnmənt/"",
      ""meaningVi"": ""Các điều phối viên ngoại giao điều hòa các mục tiêu phòng ban xung đột nhằm bảo vệ sự đồng lòng chung của tổ chức."",
      ""wordChips"": [
        ""Diplomatic"",
        ""facilitators"",
        ""harmonize"",
        ""conflicting"",
        ""departmental"",
        ""objectives"",
        ""to"",
        ""safeguard"",
        ""collective"",
        ""organizational"",
        ""alignment""
      ],
      ""distractors"": [
        ""mediators"",
        ""balance"",
        ""goals"",
        ""unity""
      ]
    },
    {
      ""id"": ""en-l22-dict-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Đo đạc kiểm thử độ trễ hệ thống (Khó)"",
      ""sentence"": ""Rigorous telemetry benchmark testing invalidated premature assertions that asynchronous queues introduce prohibitive overhead."",
      ""ipa"": ""/ˈrɪɡərəs təˈlemətri ˈbentʃmɑːk ˈtestɪŋ ɪnˈvælɪdeɪtɪd ˈpremətʃʊər əˈsɜːʃnz ðæt eɪˈsɪŋkrənəs kjuːz ˌɪntrəˈdjuːs prəˈhɪbɪtɪv ˌəʊvəˈhed/"",
      ""meaningVi"": ""Kiểm thử đo đạc nghiêm ngặt đã vô hiệu hóa những khẳng định vội vàng rằng hàng đợi bất đồng bộ gây ra chi phí phụ quá mức."",
      ""wordChips"": [
        ""Rigorous"",
        ""telemetry"",
        ""benchmark"",
        ""testing"",
        ""invalidated"",
        ""premature"",
        ""assertions"",
        ""that"",
        ""asynchronous"",
        ""queues"",
        ""introduce"",
        ""prohibitive"",
        ""overhead""
      ],
      ""distractors"": [
        ""strict"",
        ""data"",
        ""claims"",
        ""delays""
      ]
    },
    {
      ""id"": ""en-l22-dict-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Văn hóa tranh biện lành mạnh (Khó)"",
      ""sentence"": ""Fostering an atmosphere where dissenting perspectives are welcomed protects leadership from confirmation bias."",
      ""ipa"": ""/ˈfɒstərɪŋ ən ˈætməsfɪə weə dɪˈsentɪŋ pəˈspektɪvz ɑː ˈwelkəmd prəˈtekts ˈliːdəʃɪp frɒm ˌkɒnfəˈmeɪʃn ˈbaɪəs/"",
      ""meaningVi"": ""Nuôi dưỡng bầu không khí nơi các quan điểm phản biện được chào đón bảo vệ ban lãnh đạo khỏi thiên kiến xác nhận."",
      ""wordChips"": [
        ""Fostering"",
        ""an"",
        ""atmosphere"",
        ""where"",
        ""dissenting"",
        ""perspectives"",
        ""are"",
        ""welcomed"",
        ""protects"",
        ""leadership"",
        ""from"",
        ""confirmation"",
        ""bias""
      ],
      ""distractors"": [
        ""creating"",
        ""culture"",
        ""opposing"",
        ""views""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.2 Pronunciation (OrderIndex 13)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444449"), "pronunciation", 13),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444449"),
            OrderIndex = 13,
            Type = "pronunciation",
            Title = "Chấm phát âm mở rộng (Bài 2.2): 15 câu luyện âm /θ/-/ð/, âm cuối và đuôi -s/-ed",
            Description = "Luyện phát âm với AI chấm điểm trực tiếp, phát hiện lỗi sai âm đuôi và các âm khó phổ biến của người Việt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l22-pron-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /v/ trong video & voice (Dễ)"",
      ""sentence"": ""Please turn on your video and test your voice."",
      ""ipa"": ""/pliːz tɜːn ɒn jɔː ˈvɪdiəʊ ænd test jɔː vɔɪs/"",
      ""meaningVi"": ""Làm ơn bật camera và kiểm tra giọng nói của bạn."",
      ""targetWords"": [
        ""video"",
        ""voice""
      ],
      ""keyPhrases"": [
        ""turn on video"",
        ""test voice""
      ]
    },
    {
      ""id"": ""en-l22-pron-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /sk/ trong share screen (Dễ)"",
      ""sentence"": ""I will share my screen to show the new slide."",
      ""ipa"": ""/aɪ wɪl ʃeə maɪ skriːn tuː ʃəʊ ðə njuː slaɪd/"",
      ""meaningVi"": ""Tôi sẽ chia sẻ màn hình để hiển thị slide mới."",
      ""targetWords"": [
        ""screen"",
        ""slide""
      ],
      ""keyPhrases"": [
        ""share screen"",
        ""new slide""
      ]
    },
    {
      ""id"": ""en-l22-pron-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Nối âm trong loud and clear (Dễ)"",
      ""sentence"": ""We can hear you loud and clear today."",
      ""ipa"": ""/wiː kæn hɪə juː laʊd ænd klɪə təˈdeɪ/"",
      ""meaningVi"": ""Hôm nay chúng tôi nghe bạn rất rõ ràng."",
      ""targetWords"": [
        ""loud"",
        ""clear""
      ],
      ""keyPhrases"": [
        ""loud and clear""
      ]
    },
    {
      ""id"": ""en-l22-pron-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm đuôi -ed dạng /d/ trong agreed (Dễ)"",
      ""sentence"": ""Both teams agreed on the final action points."",
      ""ipa"": ""/bəʊθ tiːmz əˈɡriːd ɒn ðə ˈfaɪnl ˈækʃn pɔɪnts/"",
      ""meaningVi"": ""Cả hai nhóm đã đồng thuận về các đầu việc hành động cuối cùng."",
      ""targetWords"": [
        ""agreed"",
        ""action"",
        ""points""
      ],
      ""keyPhrases"": [
        ""agreed on"",
        ""action points""
      ]
    },
    {
      ""id"": ""en-l22-pron-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm đuôi /ks/ trong topics (Dễ)"",
      ""sentence"": ""Let us discuss these three important topics."",
      ""ipa"": ""/let ʌs dɪˈskʌs ðiːz θriː ɪmˈpɔːtnt ˈtɒpɪks/"",
      ""meaningVi"": ""Chúng ta hãy thảo luận ba chủ đề quan trọng này."",
      ""targetWords"": [
        ""topics"",
        ""three"",
        ""discuss""
      ],
      ""keyPhrases"": [
        ""discuss three topics""
      ]
    },
    {
      ""id"": ""en-l22-pron-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /v/ trong valid & viable (Vừa)"",
      ""sentence"": ""That is a valid concern, and we proposed a viable compromise."",
      ""ipa"": ""/ðæt ɪz ə ˈvælɪd kənˈsɜːn, ænd wiː prəˈpəʊzd ə ˈvaɪəbl ˈkɒmprəmaɪz/"",
      ""meaningVi"": ""Đó là một mối bận tâm xác đáng, và chúng tôi đã đề xuất một thỏa hiệp khả thi."",
      ""targetWords"": [
        ""valid"",
        ""viable"",
        ""compromise""
      ],
      ""keyPhrases"": [
        ""valid concern"",
        ""viable compromise""
      ]
    },
    {
      ""id"": ""en-l22-pron-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Nối âm trong walk us through (Vừa)"",
      ""sentence"": ""Could you walk us through the system architecture diagram?"",
      ""ipa"": ""/kəd juː wɔːk ʌs θruː ðə ˈsɪstəm ˈɑːkɪtektʃə ˈdaɪəɡræm/"",
      ""meaningVi"": ""Bạn có thể hướng dẫn chúng tôi qua sơ đồ kiến trúc hệ thống được không?"",
      ""targetWords"": [
        ""walk"",
        ""through"",
        ""architecture"",
        ""diagram""
      ],
      ""keyPhrases"": [
        ""walk us through"",
        ""system architecture""
      ]
    },
    {
      ""id"": ""en-l22-pron-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /θ/ trong bandwidth & throughput (Vừa)"",
      ""sentence"": ""Our engineering bandwidth limits maximum monthly throughput."",
      ""ipa"": ""/ˈaʊər ˌendʒɪˈnɪərɪŋ ˈbændwɪdθ ˈlɪmɪts ˈmæksɪməm ˈmʌnθli ˈθruːpʊt/"",
      ""meaningVi"": ""Năng lực kỹ thuật của chúng tôi giới hạn mức lưu lượng tối đa hàng tháng."",
      ""targetWords"": [
        ""bandwidth"",
        ""monthly"",
        ""throughput""
      ],
      ""keyPhrases"": [
        ""engineering bandwidth"",
        ""monthly throughput""
      ]
    },
    {
      ""id"": ""en-l22-pron-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Cụm phụ âm /ns/ trong consensus (Vừa)"",
      ""sentence"": ""Reaching unanimous consensus strengthens organizational alignment."",
      ""ipa"": ""/ˈriːtʃɪŋ juːˈnænɪməs kənˈsensəs ˈstreŋθnz ˌɔːɡənaɪˈzeɪʃənl əˈlaɪnmənt/"",
      ""meaningVi"": ""Đạt được sự đồng thuận nhất trí củng cố sự gắn kết tổ chức."",
      ""targetWords"": [
        ""unanimous"",
        ""consensus"",
        ""strengthens"",
        ""alignment""
      ],
      ""keyPhrases"": [
        ""unanimous consensus"",
        ""organizational alignment""
      ]
    },
    {
      ""id"": ""en-l22-pron-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đuôi -ed dạng /t/ trong decoupled (Vừa)"",
      ""sentence"": ""We decoupled the services and launched the prototype."",
      ""ipa"": ""/wiː diːˈkʌpld ðə ˈsɜːvɪsɪz ænd lɔːntʃt ðə ˈprəʊtətaɪp/"",
      ""meaningVi"": ""Chúng tôi đã tách rời các dịch vụ và phát hành bản thử nghiệm."",
      ""targetWords"": [
        ""decoupled"",
        ""services"",
        ""launched"",
        ""prototype""
      ],
      ""keyPhrases"": [
        ""decoupled services"",
        ""launched prototype""
      ]
    },
    {
      ""id"": ""en-l22-pron-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Trọng âm từ phức & âm /ʃ/ trong synchronization (Khó)"",
      ""sentence"": ""Distributed asynchronous synchronization entails stringent architectural scrutiny."",
      ""ipa"": ""/dɪˈstrɪbjuːtɪd eɪˈsɪŋkrənəs ˌsɪŋkrənaɪˈzeɪʃn ɪnˈteɪlz ˈstrɪndʒənt ˌɑːkɪˈtektʃərəl ˈskruːtəni/"",
      ""meaningVi"": ""Đồng bộ hóa bất đồng bộ phân tán đòi hỏi sự thẩm định kiến trúc nghiêm ngặt."",
      ""targetWords"": [
        ""distributed"",
        ""asynchronous"",
        ""synchronization"",
        ""stringent"",
        ""scrutiny""
      ],
      ""keyPhrases"": [
        ""asynchronous synchronization"",
        ""architectural scrutiny""
      ]
    },
    {
      ""id"": ""en-l22-pron-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cụm phụ âm /kr/ & /nt/ trong cross-functional dissent (Khó)"",
      ""sentence"": ""Constructive dissent among cross-functional stakeholders clarifies underlying trade-offs."",
      ""ipa"": ""/kənˈstrʌktɪv dɪˈsent əˈmʌŋ krɒs ˈfʌŋkʃənl ˈsteɪkhəʊldəz ˈklærɪfaɪz ˌʌndəˈlaɪɪŋ ˈtreɪd ɒfs/"",
      ""meaningVi"": ""Sự phản biện xây dựng giữa các bên liên quan đa ngành làm rõ những sự đánh đổi tiềm ẩn."",
      ""targetWords"": [
        ""constructive"",
        ""dissent"",
        ""cross-functional"",
        ""stakeholders"",
        ""trade-offs""
      ],
      ""keyPhrases"": [
        ""constructive dissent"",
        ""underlying trade-offs""
      ]
    },
    {
      ""id"": ""en-l22-pron-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Ngữ điệu điều phối phản biện (Khó)"",
      ""sentence"": ""Deftly synthesizing conflicting viewpoints requires emotional poise and articulate phrasing."",
      ""ipa"": ""/ˈdeftli ˈsɪnθəsaɪzɪŋ kənˈflɪktɪŋ ˈvjuːpɔɪnts rɪˈkwaɪəz ɪˈməʊʃənl pɔɪz ænd ɑːˈtɪkjələt ˈfreɪzɪŋ/"",
      ""meaningVi"": ""Tổng hợp khéo léo các quan điểm trái chiều đòi hỏi sự điềm tĩnh cảm xúc và cách diễn đạt gãy gọn."",
      ""targetWords"": [
        ""synthesizing"",
        ""conflicting"",
        ""viewpoints"",
        ""poise"",
        ""articulate""
      ],
      ""keyPhrases"": [
        ""synthesizing viewpoints"",
        ""emotional poise""
      ]
    },
    {
      ""id"": ""en-l22-pron-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Âm /θ/ trong telemetry & throughput (Khó)"",
      ""sentence"": ""Telemetry benchmarks substantiated throughput gains without compromising cryptographic integrity."",
      ""ipa"": ""/təˈlemətri ˈbentʃmɑːks səbˈstænʃieɪtɪd ˈθruːpʊt ɡeɪnz wɪðˈaʊt ˈkɒmprəmaɪzɪŋ ˌkrɪptəˈɡræfɪk ɪnˈteɡrəti/"",
      ""meaningVi"": ""Số liệu đo đạc thực tế đã chứng minh sự gia tăng lưu lượng mà không làm tổn hại tính toàn vẹn mật mã."",
      ""targetWords"": [
        ""telemetry"",
        ""benchmarks"",
        ""throughput"",
        ""cryptographic"",
        ""integrity""
      ],
      ""keyPhrases"": [
        ""telemetry benchmarks"",
        ""throughput gains"",
        ""cryptographic integrity""
      ]
    },
    {
      ""id"": ""en-l22-pron-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cụm phụ âm cuối /kts/ trong conflicts & respects (Khó)"",
      ""sentence"": ""Resolving interpersonal conflicts while maintaining mutual respect preserves team cohesion."",
      ""ipa"": ""/rɪˈzɒlvɪŋ ˌɪntəˈpɜːsənl ˈkɒnflɪkts waɪl meɪnˈteɪnɪŋ ˈmjuːtʃuəl rɪˈspekt prɪˈzɜːvz tiːm kəʊˈhiːʒn/"",
      ""meaningVi"": ""Giải quyết các mâu thuẫn cá nhân trong khi vẫn duy trì sự tôn trọng lẫn nhau sẽ bảo tồn sự gắn kết của nhóm."",
      ""targetWords"": [
        ""conflicts"",
        ""maintaining"",
        ""mutual"",
        ""respect"",
        ""cohesion""
      ],
      ""keyPhrases"": [
        ""interpersonal conflicts"",
        ""mutual respect"",
        ""team cohesion""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.2 Word Map (OrderIndex 14)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444449"), "word_map", 14),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444449"),
            OrderIndex = 14,
            Type = "word_map",
            Title = "Sơ đồ từ vựng Mind Map mở rộng (Bài 2.2): 12 sơ đồ Dễ → Vừa → Khó",
            Description = "Hệ thống sơ đồ tư duy từ vựng đa cấp độ kèm phiên âm IPA, collocations, phrasal verbs, synonyms và ví dụ ngữ cảnh thực chiến.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""wm-l22-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: meeting"",
      ""centralWord"": ""meeting"",
      ""ipa"": ""/ˈmiːtɪŋ/"",
      ""meaningVi"": ""Cuộc họp, buổi gặp mặt làm việc"",
      ""collocations"": [
        {
          ""text"": ""online meeting"",
          ""ipa"": ""/ˈɒnlaɪn ˈmiːtɪŋ/"",
          ""meaningVi"": ""Cuộc họp trực tuyến"",
          ""example"": ""We conduct our standup via an online meeting.""
        },
        {
          ""text"": ""chair a meeting"",
          ""ipa"": ""/tʃeər ə ˈmiːtɪŋ/"",
          ""meaningVi"": ""Chủ trì cuộc họp"",
          ""example"": ""Robert will chair the retrospective meeting.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""call off"",
          ""ipa"": ""/kɔːl ɒf/"",
          ""meaningVi"": ""Hủy bỏ cuộc họp"",
          ""example"": ""They called off the sync due to technical issues.""
        },
        {
          ""text"": ""kick off"",
          ""ipa"": ""/kɪk ɒf/"",
          ""meaningVi"": ""Bắt đầu cuộc họp"",
          ""example"": ""Let's kick off with quick round-robin updates.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""conference"",
          ""ipa"": ""/ˈkɒnfrəns/"",
          ""meaningVi"": ""Hội nghị"",
          ""example"": ""The annual tech conference gathers global innovators.""
        },
        {
          ""text"": ""gathering"",
          ""ipa"": ""/ˈɡæðərɪŋ/"",
          ""meaningVi"": ""Buổi tụ họp"",
          ""example"": ""A casual team gathering builds strong morale.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Productive meetings end with clear action items and accountable owners."",
          ""meaningVi"": ""Các cuộc họp hiệu quả luôn kết thúc bằng các đầu việc rõ ràng và người phụ trách chịu trách nhiệm.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: share"",
      ""centralWord"": ""share"",
      ""ipa"": ""/ʃeə/"",
      ""meaningVi"": ""Chia sẻ, trình chiếu màn hình"",
      ""collocations"": [
        {
          ""text"": ""share screen"",
          ""ipa"": ""/ʃeə skriːn/"",
          ""meaningVi"": ""Chia sẻ màn hình"",
          ""example"": ""May I share my screen to show the demo?""
        },
        {
          ""text"": ""share thoughts"",
          ""ipa"": ""/ʃeə θɔːts/"",
          ""meaningVi"": ""Chia sẻ suy nghĩ"",
          ""example"": ""Feel free to share your thoughts openly.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""pass on"",
          ""ipa"": ""/pɑːs ɒn/"",
          ""meaningVi"": ""Chuyển tiếp thông tin"",
          ""example"": ""Please pass on the slide deck to absent members.""
        },
        {
          ""text"": ""open up"",
          ""ipa"": ""/ˈəʊpən ʌp/"",
          ""meaningVi"": ""Cởi mở giãi bày"",
          ""example"": ""The team opened up about delivery pressures.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""present"",
          ""ipa"": ""/prɪˈzent/"",
          ""meaningVi"": ""Trình bày"",
          ""example"": ""She presented the architecture diagram with poise.""
        },
        {
          ""text"": ""display"",
          ""ipa"": ""/dɪˈspleɪ/"",
          ""meaningVi"": ""Hiển thị"",
          ""example"": ""The dashboard displays real-time system metrics.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Sharing your screen clearly guides remote participants through intricate workflows."",
          ""meaningVi"": ""Chia sẻ màn hình rõ ràng giúp dẫn dắt các thành viên làm việc từ xa qua các quy trình phức tạp.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: voice"",
      ""centralWord"": ""voice"",
      ""ipa"": ""/vɔɪs/"",
      ""meaningVi"": ""Giọng nói, bày tỏ ý kiến"",
      ""collocations"": [
        {
          ""text"": ""voice concerns"",
          ""ipa"": ""/vɔɪs kənˈsɜːnz/"",
          ""meaningVi"": ""Bày tỏ mối bận tâm"",
          ""example"": ""Engineers are encouraged to voice concerns early.""
        },
        {
          ""text"": ""tone of voice"",
          ""ipa"": ""/təʊn əv vɔɪs/"",
          ""meaningVi"": ""Giọng điệu"",
          ""example"": ""A calm tone of voice de-escalates meeting tension.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""speak up"",
          ""ipa"": ""/spiːk ʌp/"",
          ""meaningVi"": ""Nói to lên / Mạnh dạn phát biểu"",
          ""example"": ""Please speak up if your audio is cutting out.""
        },
        {
          ""text"": ""chime in"",
          ""ipa"": ""/tʃaɪm ɪn/"",
          ""meaningVi"": ""Góp lời vào"",
          ""example"": ""Feel free to chime in whenever you have feedback.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""express"",
          ""ipa"": ""/ɪkˈspres/"",
          ""meaningVi"": ""Diễn đạt"",
          ""example"": ""She expressed her reservations diplomatically.""
        },
        {
          ""text"": ""state"",
          ""ipa"": ""/steɪt/"",
          ""meaningVi"": ""Khẳng định, phát biểu"",
          ""example"": ""He clearly stated his technical preferences.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Encouraging junior engineers to voice their perspectives enriches architectural designs."",
          ""meaningVi"": ""Khuyến khích các kỹ sư trẻ cất lên tiếng nói quan điểm sẽ làm phong phú các bản thiết kế kiến trúc.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: screen"",
      ""centralWord"": ""screen"",
      ""ipa"": ""/skriːn/"",
      ""meaningVi"": ""Màn hình, rà soát kiểm tra"",
      ""collocations"": [
        {
          ""text"": ""full screen"",
          ""ipa"": ""/fʊl skriːn/"",
          ""meaningVi"": ""Toàn màn hình"",
          ""example"": ""Switch to full screen mode for clearer visibility.""
        },
        {
          ""text"": ""screen sharing"",
          ""ipa"": ""/skriːn ˈʃeərɪŋ/"",
          ""meaningVi"": ""Chia sẻ màn hình"",
          ""example"": ""Screen sharing simplifies collaborative pair programming.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""zoom in"",
          ""ipa"": ""/zuːm ɪn/"",
          ""meaningVi"": ""Phóng to"",
          ""example"": ""Could you zoom in on that database diagram?""
        },
        {
          ""text"": ""switch over"",
          ""ipa"": ""/swɪtʃ ˈəʊvə/"",
          ""meaningVi"": ""Chuyển sang cửa sổ khác"",
          ""example"": ""Let me switch over to our terminal window.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""monitor"",
          ""ipa"": ""/ˈmɒnɪtə/"",
          ""meaningVi"": ""Màn hình hiển thị"",
          ""example"": ""He checked the metrics on his secondary monitor.""
        },
        {
          ""text"": ""display"",
          ""ipa"": ""/dɪˈspleɪ/"",
          ""meaningVi"": ""Màn hình"",
          ""example"": ""The high-resolution display showed crisp charts.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Smooth screen sharing during virtual meetings prevents awkward technical stalls."",
          ""meaningVi"": ""Chia sẻ màn hình mượt mà trong các buổi họp ảo giúp tránh những lúc đình trệ kỹ thuật ngượng ngùng.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: consensus"",
      ""centralWord"": ""consensus"",
      ""ipa"": ""/kənˈsensəs/"",
      ""meaningVi"": ""Sự đồng thuận, nhất trí chung"",
      ""collocations"": [
        {
          ""text"": ""reach a consensus"",
          ""ipa"": ""/riːtʃ ə kənˈsensəs/"",
          ""meaningVi"": ""Đạt được đồng thuận"",
          ""example"": ""After long debate, we reached a consensus on framework choice.""
        },
        {
          ""text"": ""unanimous consensus"",
          ""ipa"": ""/juːˈnænɪməs kənˈsensəs/"",
          ""meaningVi"": ""Đồng thuận tuyệt đối"",
          ""example"": ""The committee reached unanimous consensus.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""agree on"",
          ""ipa"": ""/əˈɡriː ɒn/"",
          ""meaningVi"": ""Đồng ý về"",
          ""example"": ""All stakeholders agreed on the revised release checklist.""
        },
        {
          ""text"": ""come to"",
          ""ipa"": ""/kʌm tuː/"",
          ""meaningVi"": ""Đi đến kết luận"",
          ""example"": ""They came to a joint agreement on API contracts.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""agreement"",
          ""ipa"": ""/əˈɡriːmənt/"",
          ""meaningVi"": ""Thỏa thuận"",
          ""example"": ""Both parties signed a formal partnership agreement.""
        },
        {
          ""text"": ""concord"",
          ""ipa"": ""/ˈkɒŋkɔːd/"",
          ""meaningVi"": ""Sự hòa hợp"",
          ""example"": ""Teams worked in concord towards product delivery.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Building consensus early prevents friction during late-stage production releases."",
          ""meaningVi"": ""Xây dựng sự đồng thuận từ sớm giúp ngăn ngừa xích mích trong các giai đoạn phát hành muộn.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: bandwidth"",
      ""centralWord"": ""bandwidth"",
      ""ipa"": ""/ˈbændwɪdθ/"",
      ""meaningVi"": ""Băng thông mạng / Khả năng đảm đương công việc"",
      ""collocations"": [
        {
          ""text"": ""mental bandwidth"",
          ""ipa"": ""/ˈmentl ˈbændwɪdθ/"",
          ""meaningVi"": ""Tâm trí, năng lượng tinh thần"",
          ""example"": ""Context switching depletes engineers' mental bandwidth.""
        },
        {
          ""text"": ""limited bandwidth"",
          ""ipa"": ""/ˈlɪmɪtɪd ˈbændwɪdθ/"",
          ""meaningVi"": ""Quỹ thời gian/sức lực hạn chế"",
          ""example"": ""Our team has limited bandwidth for ad-hoc requests.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""take on"",
          ""ipa"": ""/teɪk ɒn/"",
          ""meaningVi"": ""Nhận thêm việc"",
          ""example"": ""Do not take on more tickets than your team can deliver.""
        },
        {
          ""text"": ""free up"",
          ""ipa"": ""/friː ʌp/"",
          ""meaningVi"": ""Giải phóng bớt thời gian"",
          ""example"": ""Automating tests frees up bandwidth for innovation.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""capacity"",
          ""ipa"": ""/kəˈpæsəti/"",
          ""meaningVi"": ""Năng lực, dung lượng"",
          ""example"": ""Server capacity must scale with user traffic.""
        },
        {
          ""text"": ""headroom"",
          ""ipa"": ""/ˈhedruːm/"",
          ""meaningVi"": ""Dung sai dự phòng"",
          ""example"": ""Leave enough headroom for unexpected production bugs.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Honest communication about team bandwidth prevents burnout and unfulfilled promises."",
          ""meaningVi"": ""Giao tiếp thành thật về năng lực đảm đương công việc giúp tránh kiệt sức và những lời hứa không thể thực hiện.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: feedback"",
      ""centralWord"": ""feedback"",
      ""ipa"": ""/ˈfiːdbæk/"",
      ""meaningVi"": ""Ý kiến phản hồi, nhận xét đóng góp"",
      ""collocations"": [
        {
          ""text"": ""constructive feedback"",
          ""ipa"": ""/kənˈstrʌktɪv ˈfiːdbæk/"",
          ""meaningVi"": ""Nhận xét mang tính xây dựng"",
          ""example"": ""Constructive feedback accelerates developer learning.""
        },
        {
          ""text"": ""solicit feedback"",
          ""ipa"": ""/səˈlɪsɪt ˈfiːdbæk/"",
          ""meaningVi"": ""Hỏi xin ý kiến đóng góp"",
          ""example"": ""Product owners regularly solicit feedback from active users.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""weigh in on"",
          ""ipa"": ""/weɪ ɪn ɒn/"",
          ""meaningVi"": ""Đưa ra bình luận về"",
          ""example"": ""The lead architect weighed in on the database schema.""
        },
        {
          ""text"": ""act on"",
          ""ipa"": ""/ækt ɒn/"",
          ""meaningVi"": ""Hành động theo phản hồi"",
          ""example"": ""We acted on user feedback to streamline our checkout flow.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""critique"",
          ""ipa"": ""/krɪˈtiːk/"",
          ""meaningVi"": ""Bài phê bình, phân tích"",
          ""example"": ""A rigorous code critique improves security.""
        },
        {
          ""text"": ""input"",
          ""ipa"": ""/ˈɪnpʊt/"",
          ""meaningVi"": ""Ý kiến đóng góp"",
          ""example"": ""Your strategic input during meetings is invaluable.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Creating a culture where constructive feedback is celebrated strengthens engineering excellence."",
          ""meaningVi"": ""Tạo dựng văn hóa nơi những phản hồi xây dựng được đón nhận sẽ củng cố sự xuất sắc trong kỹ thuật.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: proposal"",
      ""centralWord"": ""proposal"",
      ""ipa"": ""/prəˈpəʊzl/"",
      ""meaningVi"": ""Đề xuất, bản kế hoạch gợi ý"",
      ""collocations"": [
        {
          ""text"": ""counter-proposal"",
          ""ipa"": ""/ˈkaʊntə prəˈpəʊzl/"",
          ""meaningVi"": ""Đề xuất phản hồi đối ứng"",
          ""example"": ""They submitted a reasonable counter-proposal on pricing.""
        },
        {
          ""text"": ""approve a proposal"",
          ""ipa"": ""/əˈpruːv ə prəˈpəʊzl/"",
          ""meaningVi"": ""Thông qua đề xuất"",
          ""example"": ""The steering committee approved the architectural proposal.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""put forward"",
          ""ipa"": ""/pʊt ˈfɔːwəd/"",
          ""meaningVi"": ""Đưa ra đề xuất"",
          ""example"": ""The tech lead put forward an innovative refactoring plan.""
        },
        {
          ""text"": ""back up"",
          ""ipa"": ""/bæk ʌp/"",
          ""meaningVi"": ""Chứng minh ủng hộ"",
          ""example"": ""Always back up your technical proposals with benchmarks.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""proposition"",
          ""ipa"": ""/ˌprɒpəˈzɪʃn/"",
          ""meaningVi"": ""Đề xuất giá trị"",
          ""example"": ""The cloud migration is a sound business proposition.""
        },
        {
          ""text"": ""recommendation"",
          ""ipa"": ""/ˌrekəmenˈdeɪʃn/"",
          ""meaningVi"": ""Lời khuyến nghị"",
          ""example"": ""Follow the security team's recommendations.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""A persuasive technical proposal anticipates counterarguments and provides empirical proof."",
          ""meaningVi"": ""Một đề xuất kỹ thuật thuyết phục luôn lường trước các phản biện và cung cấp bằng chứng thực nghiệm.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: deliberation"",
      ""centralWord"": ""deliberation"",
      ""ipa"": ""/dɪˌlɪbəˈreɪʃn/"",
      ""meaningVi"": ""Sự cân nhắc kỹ lưỡng, bàn thảo sâu"",
      ""collocations"": [
        {
          ""text"": ""careful deliberation"",
          ""ipa"": ""/ˈkeəfl dɪˌlɪbəˈreɪʃn/"",
          ""meaningVi"": ""Cân nhắc cẩn trọng"",
          ""example"": ""After careful deliberation, we selected the PostgreSQL engine.""
        },
        {
          ""text"": ""board deliberations"",
          ""ipa"": ""/bɔːd dɪˌlɪbəˈreɪʃnz/"",
          ""meaningVi"": ""Các cuộc thảo luận của hội đồng"",
          ""example"": ""Board deliberations remain confidential.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""mull over"",
          ""ipa"": ""/mʌl ˈəʊvə/"",
          ""meaningVi"": ""Nghiền ngẫm kỹ"",
          ""example"": ""The architects mulled over the trade-offs for days.""
        },
        {
          ""text"": ""thrash out"",
          ""ipa"": ""/θræʃ aʊt/"",
          ""meaningVi"": ""Tranh luận tới cùng để giải quyết"",
          ""example"": ""Both engineering leads thrashed out the protocol differences.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""consultation"",
          ""ipa"": ""/ˌkɒnslˈteɪʃn/"",
          ""meaningVi"": ""Sự tham vấn"",
          ""example"": ""Extensive consultation with users guided our redesign.""
        },
        {
          ""text"": ""contemplation"",
          ""ipa"": ""/ˌkɒntəmˈpleɪʃn/"",
          ""meaningVi"": ""Sự suy ngẫm"",
          ""example"": ""Serious contemplation preceded the corporate restructuring.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Patient deliberation prevents impulsive technical compromises that burden teams with technical debt."",
          ""meaningVi"": ""Sự cân nhắc kiên nhẫn ngăn ngừa các thỏa hiệp kỹ thuật hấp tấp có thể đè nặng lên nhóm bằng nợ kỹ thuật.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: dissent"",
      ""centralWord"": ""dissent"",
      ""ipa"": ""/dɪˈsent/"",
      ""meaningVi"": ""Ý kiến bất đồng, sự phản biện"",
      ""collocations"": [
        {
          ""text"": ""constructive dissent"",
          ""ipa"": ""/kənˈstrʌktɪv dɪˈsent/"",
          ""meaningVi"": ""Phản biện mang tính xây dựng"",
          ""example"": ""Constructive dissent is the hallmark of resilient organizations.""
        },
        {
          ""text"": ""dissenting opinion"",
          ""ipa"": ""/dɪˈsentɪŋ əˈpɪnjən/"",
          ""meaningVi"": ""Ý kiến đối nghịch"",
          ""example"": ""He voiced a dissenting opinion regarding database sharding.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""push back against"",
          ""ipa"": ""/pʊʃ bæk əˈɡenst/"",
          ""meaningVi"": ""Phản bác lại"",
          ""example"": ""Engineers pushed back against unfeasible launch timelines.""
        },
        {
          ""text"": ""stand out from"",
          ""ipa"": ""/stænd aʊt frɒm/"",
          ""meaningVi"": ""Khác biệt với số đông"",
          ""example"": ""Her perspective stood out from the prevailing groupthink.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""disagreement"",
          ""ipa"": ""/ˌdɪsəˈɡriːmənt/"",
          ""meaningVi"": ""Sự bất đồng"",
          ""example"": ""Respectful disagreement sharpens strategic thinking.""
        },
        {
          ""text"": ""opposition"",
          ""ipa"": ""/ˌɒpəˈzɪʃn/"",
          ""meaningVi"": ""Sự phản đối"",
          ""example"": ""Initial opposition dissolved after rigorous demo testing.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Welcoming principled dissent inoculates engineering leadership against catastrophic groupthink."",
          ""meaningVi"": ""Đón nhận phản biện có nguyên tắc giúp bảo vệ ban lãnh đạo kỹ thuật khỏi tư duy bầy đàn thảm khốc.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: alignment"",
      ""centralWord"": ""alignment"",
      ""ipa"": ""/əˈlaɪnmənt/"",
      ""meaningVi"": ""Sự đồng thuận, thẳng hàng chiến lược"",
      ""collocations"": [
        {
          ""text"": ""strategic alignment"",
          ""ipa"": ""/strəˈtiːdʒɪk əˈlaɪnmənt/"",
          ""meaningVi"": ""Đồng thuận chiến lược"",
          ""example"": ""Strategic alignment ensures all squads pull in the same direction.""
        },
        {
          ""text"": ""maintain alignment"",
          ""ipa"": ""/meɪnˈteɪn əˈlaɪnmənt/"",
          ""meaningVi"": ""Duy trì sự đồng lòng"",
          ""example"": ""Weekly retrospectives maintain alignment across timezones.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""line up with"",
          ""ipa"": ""/laɪn ʌp wɪð/"",
          ""meaningVi"": ""Ăn khớp với"",
          ""example"": ""Our technical roadmap lines up with executive commercial goals.""
        },
        {
          ""text"": ""fall in line"",
          ""ipa"": ""/fɔːl ɪn laɪn/"",
          ""meaningVi"": ""Tuân theo định hướng chung"",
          ""example"": ""Subsystems fell in line with the unified design language.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""harmony"",
          ""ipa"": ""/ˈhɑːməni/"",
          ""meaningVi"": ""Sự hài hòa"",
          ""example"": ""Team harmony fosters psychological safety.""
        },
        {
          ""text"": ""cohesion"",
          ""ipa"": ""/kəʊˈhiːʒn/"",
          ""meaningVi"": ""Tính gắn kết"",
          ""example"": ""High squad cohesion elevates sprint velocity.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Sustained organizational alignment transforms fragmented efforts into synergistic market breakthroughs."",
          ""meaningVi"": ""Sự đồng thuận tổ chức bền vững biến những nỗ lực manh mún thành những đột phá thị trường mang tính cộng hưởng.""
        }
      ]
    },
    {
      ""id"": ""wm-l22-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: synchronization"",
      ""centralWord"": ""synchronization"",
      ""ipa"": ""/ˌsɪŋkrənaɪˈzeɪʃn/"",
      ""meaningVi"": ""Sự đồng bộ hóa, phối hợp nhịp nhàng"",
      ""collocations"": [
        {
          ""text"": ""data synchronization"",
          ""ipa"": ""/ˈdeɪtə ˌsɪŋkrənaɪˈzeɪʃn/"",
          ""meaningVi"": ""Đồng bộ hóa dữ liệu"",
          ""example"": ""Real-time data synchronization is crucial for financial ledgers.""
        },
        {
          ""text"": ""cross-team synchronization"",
          ""ipa"": ""/krɒs tiːm ˌsɪŋkrənaɪˈzeɪʃn/"",
          ""meaningVi"": ""Đồng bộ nhịp làm việc giữa các nhóm"",
          ""example"": ""Daily standups improve cross-team synchronization.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""sync up with"",
          ""ipa"": ""/sɪŋk ʌp wɪð/"",
          ""meaningVi"": ""Đồng bộ trao đổi cùng"",
          ""example"": ""Let's sync up with the frontend squad before deploying.""
        },
        {
          ""text"": ""mesh with"",
          ""ipa"": ""/meʃ wɪð/"",
          ""meaningVi"": ""Khớp nhịp với"",
          ""example"": ""Our release timetable meshes smoothly with client operations.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""coordination"",
          ""ipa"": ""/kəʊˌɔːdɪˈneɪʃn/"",
          ""meaningVi"": ""Sự điều phối"",
          ""example"": ""Flawless coordination made the migration unnoticeable.""
        },
        {
          ""text"": ""integration"",
          ""ipa"": ""/ˌɪntɪˈɡreɪʃn/"",
          ""meaningVi"": ""Sự tích hợp"",
          ""example"": ""Seamless API integration reduced onboarding friction.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Rigorous synchronization protocols guarantee data consistency across distributed database nodes."",
          ""meaningVi"": ""Các giao thức đồng bộ hóa chặt chẽ bảo đảm tính nhất quán của dữ liệu trên các nút cơ sở dữ liệu phân tán.""
        }
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.2 Cloze (OrderIndex 15)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444449"), "cloze", 15),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444449"),
            OrderIndex = 15,
            Type = "cloze",
            Title = "Điền từ vào chỗ trống (Bài 2.2): Bài tập ngữ cảnh thực tế",
            Description = "Đọc đoạn văn hoặc câu giao tiếp và điền từ thích hợp vào chỗ trống với gợi ý và giải thích chi tiết.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l22-cloze-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Kiểm tra kỹ thuật mở đầu cuộc họp (Dễ)"",
      ""promptVi"": ""Điền từ kiểm tra âm thanh và chia sẻ màn hình:"",
      ""template"": ""Good morning everyone, can you hear me {{0}} and see my presentation {{1}}?"",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""clearly"",
            ""well"",
            ""alright""
          ],
          ""hintVi"": ""rõ ràng (bắt đầu bằng c)"",
          ""explanationVi"": ""\""hear me clearly\"" là câu kiểm tra âm thanh tiêu chuẩn.""
        },
        {
          ""acceptedAnswers"": [
            ""slides"",
            ""screen""
          ],
          ""hintVi"": ""các slide / màn hình (bắt đầu bằng s)"",
          ""explanationVi"": ""\""presentation slides / screen\"" chỉ màn hình đang chiếu.""
        }
      ],
      ""fullSentence"": ""Good morning everyone, can you hear me clearly and see my presentation slides?"",
      ""meaningVi"": ""Chào buổi sáng mọi người, các bạn có nghe rõ tiếng tôi và nhìn thấy các slide thuyết trình không?"",
      ""explanationVi"": ""Câu mở đầu không thể thiếu của bất kỳ buổi họp Zoom/Teams nào.""
    },
    {
      ""id"": ""en-l22-cloze-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Nhắc nhở bật mic lịch sự (Dễ)"",
      ""promptVi"": ""Điền từ về trạng thái micro trong họp trực tuyến:"",
      ""template"": ""I believe you are on {{0}}, could you please {{1}} your microphone?"",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""mute""
          ],
          ""hintVi"": ""tắt tiếng (on ___)"",
          ""explanationVi"": ""\""you are on mute\"" nghĩa là bạn đang tắt mic.""
        },
        {
          ""acceptedAnswers"": [
            ""unmute"",
            ""turn on"",
            ""enable""
          ],
          ""hintVi"": ""bật mic lại (bắt đầu bằng u)"",
          ""explanationVi"": ""\""unmute your microphone\"" nghĩa là bật lại mic.""
        }
      ],
      ""fullSentence"": ""I believe you are on mute, could you please unmute your microphone?"",
      ""meaningVi"": ""Tôi nghĩ là bạn đang tắt mic, bạn có thể vui lòng bật lại mic được không?"",
      ""explanationVi"": ""Tình huống cực kỳ phổ biến trong môi trường làm việc từ xa.""
    },
    {
      ""id"": ""en-l22-cloze-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Phản biện ngoại giao (Vừa)"",
      ""promptVi"": ""Điền từ làm mềm lời phản biện:"",
      ""template"": ""I see your {{0}}, but have we taken the long-term maintenance {{1}} into account?"",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""point"",
            ""perspective"",
            ""view""
          ],
          ""hintVi"": ""quan điểm / ý (I see your ___)"",
          ""explanationVi"": ""\""I see your point\"" là cách nói ghi nhận ý kiến trước khi phản biện.""
        },
        {
          ""acceptedAnswers"": [
            ""costs"",
            ""overhead"",
            ""burdens""
          ],
          ""hintVi"": ""chi phí bảo trì (bắt đầu bằng c)"",
          ""explanationVi"": ""\""maintenance costs\"" nghĩa là chi phí bảo trì.""
        }
      ],
      ""fullSentence"": ""I see your point, but have we taken the long-term maintenance costs into account?"",
      ""meaningVi"": ""Tôi hiểu ý của bạn, nhưng chúng ta đã tính đến chi phí bảo trì lâu dài chưa?"",
      ""explanationVi"": ""Kỹ thuật phản biện ngoại giao kinh điển của người bản xứ.""
    },
    {
      ""id"": ""en-l22-cloze-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Chốt phân công trách nhiệm (Vừa)"",
      ""promptVi"": ""Điền từ về giao việc và kết thúc cuộc họp:"",
      ""template"": ""Let us assign clear {{0}} for each action item before we {{1}} today's sync."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""ownership"",
            ""responsibility""
          ],
          ""hintVi"": ""quyền sở hữu / trách nhiệm cá nhân (bắt đầu bằng o)"",
          ""explanationVi"": ""\""assign clear ownership\"" là gán người phụ trách chính.""
        },
        {
          ""acceptedAnswers"": [
            ""wrap up"",
            ""conclude"",
            ""adjourn"",
            ""end""
          ],
          ""hintVi"": ""khép lại / kết thúc (bắt đầu bằng w)"",
          ""explanationVi"": ""\""wrap up\"" nghĩa là kết thúc buổi họp.""
        }
      ],
      ""fullSentence"": ""Let us assign clear ownership for each action item before we wrap up today's sync."",
      ""meaningVi"": ""Chúng ta hãy phân công người phụ trách rõ ràng cho từng đầu việc trước khi khép lại buổi họp hôm nay."",
      ""explanationVi"": ""Quy tắc vàng để các cuộc họp trực tuyến thực sự tạo ra kết quả.""
    },
    {
      ""id"": ""en-l22-cloze-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Đạt đồng thuận kỹ thuật (Khó)"",
      ""promptVi"": ""Điền các thuật ngữ cấp cao về đồng thuận và kiến trúc:"",
      ""template"": ""After rigorous technical deliberations, the committee reached a unanimous {{0}} on microservices {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""consensus"",
            ""agreement""
          ],
          ""hintVi"": ""sự đồng thuận nhất trí (bắt đầu bằng c)"",
          ""explanationVi"": ""\""unanimous consensus\"" là sự đồng thuận tuyệt đối 100%.""
        },
        {
          ""acceptedAnswers"": [
            ""decoupling"",
            ""migration"",
            ""architecture""
          ],
          ""hintVi"": ""sự phân tách / chuyển đổi (bắt đầu bằng d)"",
          ""explanationVi"": ""\""microservices decoupling\"" là việc phân tách các dịch vụ microservices.""
        }
      ],
      ""fullSentence"": ""After rigorous technical deliberations, the committee reached a unanimous consensus on microservices decoupling."",
      ""meaningVi"": ""Sau các cuộc cân nhắc kỹ thuật nghiêm ngặt, hội đồng đã đạt được sự đồng thuận nhất trí về việc phân tách microservices."",
      ""explanationVi"": ""Văn phong học thuật & kỹ thuật chuyên nghiệp cho các cuộc họp hội đồng kiến trúc.""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.2 Translate (OrderIndex 16)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444449"), "translate", 16),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444449"),
            OrderIndex = 16,
            Type = "translate",
            Title = "Dịch phản xạ Việt → Anh (Bài 2.2): Diễn đạt tự nhiên",
            Description = "Chuyển câu tiếng Việt sang tiếng Anh tự nhiên với nhiều đáp án được chấp nhận và lưu ý ngữ pháp hữu ích.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l22-trans-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Kiểm tra slide thuyết trình (Dễ)"",
      ""sourceVi"": ""Mọi người có nhìn thấy slide mà tôi đang chia sẻ trên màn hình không?"",
      ""hintKeywords"": [
        ""see the slides"",
        ""sharing on screen"",
        ""can everyone""
      ],
      ""acceptedAnswers"": [
        ""Can everyone see the slides I am sharing on screen?"",
        ""Can you all see the slides that I am sharing on my screen?"",
        ""Can everyone see my shared slides on the screen?""
      ],
      ""sampleAnswer"": ""Can everyone see the slides I am sharing on screen?"",
      ""grammarNoteVi"": ""Mẫu câu hỏi tương tác mở đầu buổi thuyết trình: \""Can everyone see...?\"".""
    },
    {
      ""id"": ""en-l22-trans-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Xin lỗi vì ngắt lời (Dễ)"",
      ""sourceVi"": ""Xin lỗi vì đã ngắt lời bạn, xin mời bạn tiếp tục ý kiến của mình."",
      ""hintKeywords"": [
        ""sorry to cut you off"",
        ""please go ahead""
      ],
      ""acceptedAnswers"": [
        ""Sorry to cut you off, please go ahead with your point."",
        ""I apologize for cutting you off, please continue."",
        ""Sorry to interrupt, please go ahead.""
      ],
      ""sampleAnswer"": ""Sorry to cut you off, please go ahead with your point."",
      ""grammarNoteVi"": ""\""Sorry to cut you off\"" là thành ngữ tự nhiên để xin lỗi khi vô tình nói đè tiếng nhau trên mạng.""
    },
    {
      ""id"": ""en-l22-trans-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Thảo luận riêng sau cuộc họp (Vừa)"",
      ""sourceVi"": ""Đây là một câu hỏi kỹ thuật rất chi tiết, vì vậy chúng ta hãy trao đổi riêng sau cuộc họp này nhé."",
      ""hintKeywords"": [
        ""detailed technical question"",
        ""take it offline"",
        ""after this meeting""
      ],
      ""acceptedAnswers"": [
        ""This is a very detailed technical question, so let's take it offline after this meeting."",
        ""This is a detailed technical question, so let us discuss it offline after the call."",
        ""That's a very detailed question, let's take it offline after today's session.""
      ],
      ""sampleAnswer"": ""This is a very detailed technical question, so let's take it offline after this meeting."",
      ""grammarNoteVi"": ""Cụm \""take it offline\"" là thuật ngữ công sở phổ biến nhất để chỉ việc thảo luận riêng.""
    },
    {
      ""id"": ""en-l22-trans-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Phản biện ngoại giao về ngân sách (Vừa)"",
      ""sourceVi"": ""Tôi hiểu góc nhìn của bạn, nhưng chúng ta cũng nên đánh giá kỹ lưỡng các rủi ro về ngân sách."",
      ""hintKeywords"": [
        ""I see your perspective"",
        ""evaluate budgetary risks"",
        ""thoroughly""
      ],
      ""acceptedAnswers"": [
        ""I see your perspective, but we should also thoroughly evaluate budgetary risks."",
        ""I see your point, but we must also evaluate the budgetary risks carefully."",
        ""While I understand your perspective, we should thoroughly evaluate budgetary risks.""
      ],
      ""sampleAnswer"": ""I see your perspective, but we should also thoroughly evaluate budgetary risks."",
      ""grammarNoteVi"": ""Dùng \""I see your perspective/point\"" trước \""but\"" để tạo sự mềm mỏng, khách quan.""
    },
    {
      ""id"": ""en-l22-trans-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Dịch: Chốt trách nhiệm giải trình và thời hạn (Khó)"",
      ""sourceVi"": ""Việc phân công người phụ trách cụ thể cho từng sản phẩm bàn giao đảm bảo sự đồng thuận và trách nhiệm giải trình của toàn đội ngũ."",
      ""hintKeywords"": [
        ""assigning clear ownership"",
        ""each deliverable"",
        ""ensures team accountability and alignment""
      ],
      ""acceptedAnswers"": [
        ""Assigning clear ownership for each deliverable ensures complete team accountability and alignment."",
        ""Assigning explicit ownership for every deliverable guarantees team alignment and accountability.""
      ],
      ""sampleAnswer"": ""Assigning clear ownership for each deliverable ensures complete team accountability and alignment."",
      ""grammarNoteVi"": ""Chủ ngữ là danh động từ (\""Assigning...\"") chia động từ số ít (\""ensures\"").""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.2 Grammar Fix (OrderIndex 17)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444449"), "grammar_fix", 17),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444449"),
            OrderIndex = 17,
            Type = "grammar_fix",
            Title = "Sửa lỗi ngữ pháp (Bài 2.2): Khắc phục các lỗi người Việt hay mắc",
            Description = "Phát hiện và sửa các lỗi sai ngữ pháp, thì động từ, mạo từ và giới từ thường gặp trong giao tiếp thực tế.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l22-gf-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Dùng sai giới từ mute (Dễ)"",
      ""incorrectSentence"": ""Alex, you are in mute, please turn on your microphone."",
      ""errorTypeHintVi"": ""Tắt tiếng trên phần mềm gọi video dùng giới từ \""on mute\"", không dùng \""in mute\""."",
      ""correctedSentence"": ""Alex, you are on mute, please turn on your microphone."",
      ""acceptedAnswers"": [
        ""Alex, you are on mute, please turn on your microphone."",
        ""Alex, you're on mute, please unmute your microphone.""
      ],
      ""meaningVi"": ""Alex ơi bạn đang tắt mic, làm ơn bật mic của bạn lên."",
      ""explanationVi"": ""Cụm từ chuẩn là \""be on mute\"".""
    },
    {
      ""id"": ""en-l22-gf-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Ngắt lời người khác quá cộc lốc (Dễ)"",
      ""incorrectSentence"": ""Stop talking now, I want to say my opinion."",
      ""errorTypeHintVi"": ""Diễn đạt quá thô lỗ; cần dùng mẫu câu xin lỗi và xin phép nói lịch sự."",
      ""correctedSentence"": ""Sorry to cut you off, may I share my perspective on this?"",
      ""acceptedAnswers"": [
        ""Sorry to cut you off, may I share my perspective on this?"",
        ""Sorry to interrupt, may I share my perspective on this?"",
        ""Excuse me for interrupting, could I share my thoughts?""
      ],
      ""meaningVi"": ""Xin lỗi vì đã ngắt lời, tôi có thể chia sẻ góc nhìn của mình về điều này không?"",
      ""explanationVi"": ""Trong giao tiếp công sở quốc tế, luôn bắt đầu bằng \""Sorry to cut you off / interrupt\"" và xin phép bằng \""may I / could I\"".""
    },
    {
      ""id"": ""en-l22-gf-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Dùng sai liên từ nhượng bộ (Vừa)"",
      ""incorrectSentence"": ""Although I agree with your proposal, but we must evaluate the latency."",
      ""errorTypeHintVi"": ""Trong tiếng Anh không bao giờ đi cặp \""Although... but...\"" cùng lúc."",
      ""correctedSentence"": ""Although I agree with your proposal, we must evaluate the latency."",
      ""acceptedAnswers"": [
        ""Although I agree with your proposal, we must evaluate the latency."",
        ""I agree with your proposal, but we must evaluate the latency."",
        ""While I agree with your proposal, we must evaluate the latency.""
      ],
      ""meaningVi"": ""Dù tôi đồng ý với đề xuất của bạn, chúng ta vẫn phải đánh giá độ trễ."",
      ""explanationVi"": ""Chọn dùng \""Although/While\"" ở đầu câu HOẶC dùng \""but\"" ở giữa, tuyệt đối không dùng cả hai.""
    },
    {
      ""id"": ""en-l22-gf-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Giới từ sau discuss (Vừa)"",
      ""incorrectSentence"": ""Let us discuss about the cloud migration strategy in detail."",
      ""errorTypeHintVi"": ""Động từ \""discuss\"" là ngoại động từ trực tiếp, không đi kèm giới từ \""about\""."",
      ""correctedSentence"": ""Let us discuss the cloud migration strategy in detail."",
      ""acceptedAnswers"": [
        ""Let us discuss the cloud migration strategy in detail."",
        ""Let's discuss the cloud migration strategy in detail.""
      ],
      ""meaningVi"": ""Chúng ta hãy thảo luận chi tiết về chiến lược chuyển đổi đám mây."",
      ""explanationVi"": ""Nói \""discuss something\"", không nói \""discuss about something\"" (chỉ nói \""talk about / debate about\"").""
    },
    {
      ""id"": ""en-l22-gf-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Sửa lỗi: Thức giả định sau recommend (Khó)"",
      ""incorrectSentence"": ""The chair recommended that each module owner takes full accountability."",
      ""errorTypeHintVi"": ""Sau động từ \""recommend that...\"", mệnh đề danh từ dùng thức giả định với động từ nguyên mẫu không chia (take)."",
      ""correctedSentence"": ""The chair recommended that each module owner take full accountability."",
      ""acceptedAnswers"": [
        ""The chair recommended that each module owner take full accountability."",
        ""The chair recommended that each module owner should take full accountability.""
      ],
      ""meaningVi"": ""Chủ tọa đã khuyến nghị rằng mỗi người phụ trách module cần nhận hoàn toàn trách nhiệm giải trình."",
      ""explanationVi"": ""Cấu trúc Subjunctive: \""recommend that S + (should) V-infinitive\"" (\""take\"").""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 2.2 Roleplay (OrderIndex 18)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444449"), "roleplay", 18),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444449"),
            OrderIndex = 18,
            Type = "roleplay",
            Title = "Nhập vai hội thoại (Bài 2.2): Tình huống phản xạ thực chiến",
            Description = "Nhập vai giao tiếp với AI trong các tình huống thực tế, hỗ trợ chấm điểm tự động theo từ khóa và cấu trúc câu.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l22-rp-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Kiểm tra kỹ thuật đầu buổi họp Zoom (Dễ)"",
      ""scenarioVi"": ""Bạn vừa chủ trì một buổi họp trực tuyến và muốn kiểm tra đường truyền âm thanh cũng như việc chia sẻ màn hình."",
      ""aiRole"": ""Robert (Kỹ sư trưởng)"",
      ""aiPrompt"": ""Good morning! I see everyone is in the room. Are we ready to begin?"",
      ""aiPromptVi"": ""Chào buổi sáng! Tôi thấy mọi người đã vào phòng họp. Chúng ta đã sẵn sàng bắt đầu chưa?"",
      ""userGoalVi"": ""Chào buổi sáng, hỏi mọi người có nghe rõ tiếng bạn không và xin phép chia sẻ màn hình slide."",
      ""expectedKeywords"": [
        ""good morning"",
        ""hear me clearly"",
        ""share my screen"",
        ""slides""
      ],
      ""sampleResponses"": [
        ""Good morning, Robert! Can everyone hear me loud and clear? Please let me know once you can see my shared screen.""
      ],
      ""minWords"": 10
    },
    {
      ""id"": ""en-l22-rp-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Bày tỏ sự đồng thuận với đề xuất (Dễ)"",
      ""scenarioVi"": ""Đồng nghiệp vừa trình bày phương án dời ngày ra mắt bản beta sang thứ Sáu để kịp kiểm thử kỹ lưỡng."",
      ""aiRole"": ""Robert (Kỹ sư trưởng)"",
      ""aiPrompt"": ""We propose shifting our beta release to Friday so QA has ample time for regression testing. What are your thoughts?"",
      ""aiPromptVi"": ""Chúng tôi đề xuất dời ngày ra mắt bản beta sang thứ Sáu để bộ phận QA có đủ thời gian kiểm thử hồi quy. Ý kiến của bạn thế nào?"",
      ""userGoalVi"": ""Hoàn toàn đồng ý với đề xuất, nhấn mạnh chất lượng và sự ổn định của hệ thống là ưu tiên hàng đầu."",
      ""expectedKeywords"": [
        ""agree"",
        ""proposal"",
        ""quality"",
        ""stability"",
        ""priority""
      ],
      ""sampleResponses"": [
        ""I completely agree with this proposal. Giving QA extra time ensures system stability, which should always be our top priority.""
      ],
      ""minWords"": 10
    },
    {
      ""id"": ""en-l22-rp-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Phản biện ngoại giao về chi phí vận hành (Vừa)"",
      ""scenarioVi"": ""Trưởng nhóm đề xuất thuê thêm 10 máy chủ đám mây đắt tiền, bạn muốn đề xuất tối ưu hóa truy vấn trước."",
      ""aiRole"": ""David (Kiến trúc sư hệ thống)"",
      ""aiPrompt"": ""To handle next month's traffic spike, I suggest spinning up ten dedicated high-memory instances immediately."",
      ""aiPromptVi"": ""Để xử lý đợt tăng đột biến lưu lượng tháng tới, tôi đề xuất khởi chạy ngay 10 máy chủ chuyên dụng bộ nhớ cao."",
      ""userGoalVi"": ""Ghi nhận giải pháp mở rộng nhanh, nhưng lịch sự nêu quan ngại về chi phí và đề xuất tối ưu hóa bộ nhớ đệm trước."",
      ""expectedKeywords"": [
        ""see your point"",
        ""cost"",
        ""caching"",
        ""optimize"",
        ""alternative"",
        ""explore""
      ],
      ""sampleResponses"": [
        ""I see your point about handling traffic spikes quickly, David. However, that adds significant cloud costs. Could we explore optimizing our caching layer first to see if that handles the load?""
      ],
      ""minWords"": 14
    },
    {
      ""id"": ""en-l22-rp-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Đề xuất thảo luận chi tiết sau cuộc họp (Vừa)"",
      ""scenarioVi"": ""Một cuộc tranh luận kỹ thuật chi tiết về cú pháp ngôn ngữ lập trình đang làm kéo dài buổi họp toàn thể."",
      ""aiRole"": ""Robert (Kỹ sư trưởng)"",
      ""aiPrompt"": ""We have been debating database index syntax for twenty minutes and we are running out of scheduled meeting time."",
      ""aiPromptVi"": ""Chúng ta đã tranh luận về cú pháp chỉ mục cơ sở dữ liệu suốt 20 phút và sắp hết thời gian họp rồi."",
      ""userGoalVi"": ""Đề xuất tạm gác chủ đề này lại, hẹn thảo luận riêng với các kỹ sư cơ sở dữ liệu sau để tiếp tục chương trình họp."",
      ""expectedKeywords"": [
        ""take it offline"",
        ""agenda"",
        ""respect time"",
        ""follow up"",
        ""schedule""
      ],
      ""sampleResponses"": [
        ""I suggest we take this specific technical debate offline. Let's schedule a dedicated follow-up with the database engineers so we can stick to our main agenda today.""
      ],
      ""minWords"": 12
    },
    {
      ""id"": ""en-l22-rp-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Chốt biên bản và phân công người chịu trách nhiệm (Khó)"",
      ""scenarioVi"": ""Cuộc họp chuẩn bị kết thúc, bạn với vai trò chủ tọa cần tổng kết các quyết định và chốt quyền sở hữu nhiệm vụ."",
      ""aiRole"": ""David (Kiến trúc sư hệ thống)"",
      ""aiPrompt"": ""We have covered a lot of ground today on microservices migration. How should we formalize our next steps?"",
      ""aiPromptVi"": ""Hôm nay chúng ta đã đi qua rất nhiều nội dung về chuyển đổi microservices. Chúng ta nên chính thức hóa các bước tiếp theo như thế nào?"",
      ""userGoalVi"": ""Tổng kết 2 quyết định then chốt, phân công David phụ trách nguyên mẫu kỹ thuật và Mai lập tài liệu bảo mật vào thứ Tư tới."",
      ""expectedKeywords"": [
        ""summarize"",
        ""action items"",
        ""ownership"",
        ""prototype"",
        ""security"",
        ""deadline"",
        ""Wednesday""
      ],
      ""sampleResponses"": [
        ""To wrap up, we have consensus on building a pilot microservice. David will take ownership of the prototype implementation, and Mai will prepare the security benchmarks by next Wednesday. I will circulate the minutes shortly.""
      ],
      ""minWords"": 16
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.1 Shadowing (OrderIndex 11)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444450"), "shadowing", 11),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444450"),
            OrderIndex = 11,
            Type = "shadowing",
            Title = "Shadowing mở rộng (Bài 3.1): 15 câu đa cấp độ Dễ → Vừa → Khó",
            Description = "Luyện nói đuổi theo ngữ điệu người bản xứ, tập trung vào nối âm, ngữ điệu và phát âm chuẩn các âm then chốt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l31-shad-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Chào mừng mở đầu thuyết trình (Dễ)"",
      ""sentence"": ""Good morning everyone, and thank you for attending today's presentation."",
      ""ipa"": ""/ɡʊd ˈmɔːnɪŋ ˈevriwʌn, ænd θæŋk juː fɔːr əˈtendɪŋ təˈdeɪz ˌpreznˈteɪʃn/"",
      ""meaningVi"": ""Chào buổi sáng mọi người, và cảm ơn các bạn đã tham dự buổi thuyết trình ngày hôm nay."",
      ""keyPhrases"": [
        ""good morning everyone"",
        ""thank you for attending"",
        ""today's presentation""
      ]
    },
    {
      ""id"": ""en-l31-shad-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Giới thiệu chủ đề bài nói (Dễ)"",
      ""sentence"": ""Today I would like to talk about our new product strategy."",
      ""ipa"": ""/təˈdeɪ aɪ wʊd laɪk tuː tɔːk əˈbaʊt ˈaʊə njuː ˈprɒdʌkt ˈstrætədʒi/"",
      ""meaningVi"": ""Hôm nay tôi muốn chia sẻ về chiến lược sản phẩm mới của chúng tôi."",
      ""keyPhrases"": [
        ""would like to talk about"",
        ""new product strategy""
      ]
    },
    {
      ""id"": ""en-l31-shad-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hướng sự chú ý vào slide (Dễ)"",
      ""sentence"": ""As you can see on this slide, our sales increased this year."",
      ""ipa"": ""/æz juː kæn siː ɒn ðɪs slaɪd, ˈaʊə seɪlz ɪnˈkriːst ðɪs jɪə/"",
      ""meaningVi"": ""Như các bạn có thể thấy trên slide này, doanh số của chúng tôi đã tăng trong năm nay."",
      ""keyPhrases"": [
        ""as you can see"",
        ""on this slide"",
        ""sales increased""
      ]
    },
    {
      ""id"": ""en-l31-shad-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Chuyển ý sang phần tiếp theo (Dễ)"",
      ""sentence"": ""Now, let us move on to the second part of our presentation."",
      ""ipa"": ""/naʊ, let ʌs muːv ɒn tuː ðə ˈsekənd pɑːt əv ˈaʊə ˌpreznˈteɪʃn/"",
      ""meaningVi"": ""Bây giờ, chúng ta hãy cùng chuyển sang phần thứ hai của bài thuyết trình."",
      ""keyPhrases"": [
        ""move on to"",
        ""second part"",
        ""our presentation""
      ]
    },
    {
      ""id"": ""en-l31-shad-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Cảm ơn và mở phần hỏi đáp (Dễ)"",
      ""sentence"": ""Thank you for your attention, and I am happy to answer your questions."",
      ""ipa"": ""/θæŋk juː fɔː jɔːr əˈtenʃn, ænd aɪ æm ˈhæpi tuː ˈɑːnsə jɔː ˈkwestʃənz/"",
      ""meaningVi"": ""Cảm ơn sự chú ý của các bạn, và tôi rất vui được trả lời các câu hỏi của các bạn."",
      ""keyPhrases"": [
        ""thank you for your attention"",
        ""answer your questions""
      ]
    },
    {
      ""id"": ""en-l31-shad-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đặt câu hỏi mở bài gợi mở (Vừa)"",
      ""sentence"": ""Have you ever wondered why some development squads consistently outperform their ambitious quarterly targets?"",
      ""ipa"": ""/hæv juː ˈevə ˈwʌndəd waɪ sʌm dɪˈveləpmənt skwɒdz kənˈsɪstəntli aʊtpəˈfɔːm ðeər æmˈbɪʃəs ˈkwɔːtəli ˈtɑːɡɪts/"",
      ""meaningVi"": ""Bạn đã bao giờ tự hỏi tại sao một số đội ngũ phát triển luôn vượt trội bền bỉ so với các mục tiêu quý đầy tham vọng chưa?"",
      ""keyPhrases"": [
        ""have you ever wondered"",
        ""consistently outperform"",
        ""quarterly targets""
      ]
    },
    {
      ""id"": ""en-l31-shad-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Mô tả xu hướng biểu đồ (Vừa)"",
      ""sentence"": ""Direct your attention to the line graph, which illustrates an exponential surge in user adoption."",
      ""ipa"": ""/daɪˈrekt jɔːr əˈtenʃn tuː ðə laɪn ɡrɑːf, wɪtʃ ˈɪləstreɪts ən ˌekspəˈnenʃl sɜːdʒ ɪn ˈjuːzər əˈdɒpʃn/"",
      ""meaningVi"": ""Xin hãy hướng sự chú ý lên biểu đồ đường, nơi minh họa sự tăng trưởng cấp số nhân về số lượng người dùng."",
      ""keyPhrases"": [
        ""direct your attention"",
        ""line graph"",
        ""exponential surge"",
        ""user adoption""
      ]
    },
    {
      ""id"": ""en-l31-shad-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Từ nối chuyển ý chuyên nghiệp (Vừa)"",
      ""sentence"": ""Turning our attention to operational metrics, our customer satisfaction index reached an all-time high."",
      ""ipa"": ""/ˈtɜːnɪŋ ˈaʊər əˈtenʃn tuː ˌɒpəˈreɪʃənl ˈmetrɪks, ˈaʊə ˈkʌstəmə ˌsætɪsˈfækʃn ˈɪndeks riːtʃt ən ɔːl taɪm haɪ/"",
      ""meaningVi"": ""Chuyển sự chú ý sang các chỉ số vận hành, chỉ số hài lòng khách hàng của chúng ta đã đạt mức cao kỷ lục từ trước tới nay."",
      ""keyPhrases"": [
        ""turning our attention to"",
        ""customer satisfaction index"",
        ""all-time high""
      ]
    },
    {
      ""id"": ""en-l31-shad-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Nhấn mạnh thông điệp cốt lõi (Vừa)"",
      ""sentence"": ""The primary takeaway from today's discussion is that disciplined automation yields compounding returns."",
      ""ipa"": ""/ðə ˈpraɪməri ˈteɪkəweɪ frɒm təˈdeɪz dɪˈskʌʃn ɪz ðæt ˈdɪsəplɪnd ˌɔːtəˈmeɪʃn jiːldz kəmˈpaʊndɪŋ rɪˈtɜːnz/"",
      ""meaningVi"": ""Thông điệp cốt lõi từ cuộc thảo luận hôm nay là sự tự động hóa có kỷ luật mang lại lợi suất tích lũy kép."",
      ""keyPhrases"": [
        ""primary takeaway"",
        ""disciplined automation"",
        ""compounding returns""
      ]
    },
    {
      ""id"": ""en-l31-shad-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Kêu gọi hành động kết bài (Vừa)"",
      ""sentence"": ""I strongly invite each department to pilot this peer-learning model before the upcoming quarter."",
      ""ipa"": ""/aɪ ˈstrɒŋli ɪnˈvaɪt iːtʃ dɪˈpɑːtmənt tuː ˈpaɪlət ðɪs pɪə ˈlɜːnɪŋ ˈmɒdl bɪˈfɔː ði ˈʌpˌkʌmɪŋ ˈkwɔːtə/"",
      ""meaningVi"": ""Tôi nhiệt liệt mời từng phòng ban hãy thử nghiệm mô hình học tập đồng cấp này trước khi quý tới bắt đầu."",
      ""keyPhrases"": [
        ""strongly invite"",
        ""pilot this model"",
        ""peer-learning"",
        ""upcoming quarter""
      ]
    },
    {
      ""id"": ""en-l31-shad-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Nghệ thuật kể chuyện thuyết phục (Khó)"",
      ""sentence"": ""Articulating compelling data narratives with strategic visual storytelling captivates seasoned executive investors effortlessly."",
      ""ipa"": ""/ɑːˈtɪkjuleɪtɪŋ kəmˈpelɪŋ ˈdeɪtə ˈnærətɪvz wɪð strəˈtiːdʒɪk ˈvɪʒuəl ˈstɔːritelɪŋ ˈkæptɪveɪts ˈsiːznd ɪɡˈzekjətɪv ɪnˈvestəz ˈefətləsli/"",
      ""meaningVi"": ""Diễn đạt những câu chuyện dữ liệu giàu sức thuyết phục kết hợp kể chuyện bằng hình ảnh chiến lược lôi cuốn các nhà đầu tư điều hành lão luyện một cách tự nhiên."",
      ""keyPhrases"": [
        ""compelling data narratives"",
        ""visual storytelling"",
        ""seasoned executive investors""
      ]
    },
    {
      ""id"": ""en-l31-shad-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Chứng minh lợi suất đầu tư (Khó)"",
      ""sentence"": ""Empirical benchmark telemetry validates that investing in developer enablement generates unprecedented compound returns."",
      ""ipa"": ""/ɪmˈpɪrɪkl ˈbentʃmɑːk təˈlemətri ˈvælɪdeɪts ðæt ɪnˈvestɪŋ ɪn dɪˈveləpər ɪˈneɪblmənt ˈdʒenəreɪts ʌnˈpresɪdentɪd ˈkɒmpaʊnd rɪˈtɜːnz/"",
      ""meaningVi"": ""Số liệu đo đạc thực tế chứng minh rằng đầu tư vào việc trao quyền cho lập trình viên tạo ra lợi nhuận kép chưa từng có."",
      ""keyPhrases"": [
        ""empirical telemetry"",
        ""developer enablement"",
        ""unprecedented compound returns""
      ]
    },
    {
      ""id"": ""en-l31-shad-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Chuyển giao thông điệp cấp cao (Khó)"",
      ""sentence"": ""To synthesize our findings: organizational resilience is cultivated not by reactive firefighting, but by deliberate architectural discipline."",
      ""ipa"": ""/tuː ˈsɪnθəsaɪz ˈaʊə ˈfaɪndɪŋz: ˌɔːɡənaɪˈzeɪʃənl rɪˈzɪliəns ɪz ˈkʌltɪveɪtɪd nɒt baɪ riˈæktɪv ˈfaɪəfaɪtɪŋ, bʌt baɪ dɪˈlɪbərət ˌɑːkɪˈtektʃərəl ˈdɪsəplɪn/"",
      ""meaningVi"": ""Để tổng hợp các phát hiện của chúng tôi: sự kiên cường của tổ chức không được trui rèn bằng việc chữa cháy bị động, mà bằng kỷ luật kiến trúc có chủ đích."",
      ""keyPhrases"": [
        ""synthesize our findings"",
        ""organizational resilience"",
        ""deliberate architectural discipline""
      ]
    },
    {
      ""id"": ""en-l31-shad-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Dẫn dắt chuyển ý phức hợp (Khó)"",
      ""sentence"": ""Having substantiated our theoretical premise, let us now examine the quantitative implications across disparate market segments."",
      ""ipa"": ""/ˈhævɪŋ səbˈstænʃieɪtɪd ˈaʊə ˌθɪəˈretɪkl ˈpremɪs, let ʌs naʊ ɪɡˈzæmɪn ðə ˈkwɒntɪtətɪv ˌɪmplɪˈkeɪʃnz əˈkrɒs ˈdɪspərət ˈmɑːkɪt ˈseɡmənts/"",
      ""meaningVi"": ""Sau khi đã làm rõ tiền đề lý thuyết, giờ đây chúng ta hãy cùng phân tích các tác động định lượng trên các phân khúc thị trường khác nhau."",
      ""keyPhrases"": [
        ""substantiated theoretical premise"",
        ""quantitative implications"",
        ""disparate market segments""
      ]
    },
    {
      ""id"": ""en-l31-shad-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Lời kết bài hùng hồn và dứt khoát (Khó)"",
      ""sentence"": ""Mastering the synthesis of concise storytelling and rigorous analytical proof constitutes the pinnacle of executive communication."",
      ""ipa"": ""/ˈmɑːstərɪŋ ðə ˈsɪnθəsɪs əv kənˈsaɪs ˈstɔːritelɪŋ ænd ˈrɪɡərəs ˌænəˈlɪtɪkl pruːf ˈkɒnstɪtjuːts ðə ˈpɪnəkl əv ɪɡˈzekjətɪv kəˌmjuːnɪˈkeɪʃn/"",
      ""meaningVi"": ""Làm chủ sự kết hợp giữa lối kể chuyện cô đọng và bằng chứng phân tích chặt chẽ tạo nên đỉnh cao của nghệ thuật giao tiếp điều hành."",
      ""keyPhrases"": [
        ""synthesis of storytelling"",
        ""analytical proof"",
        ""pinnacle of executive communication""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.1 Dictation (OrderIndex 12)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444450"), "dictation", 12),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444450"),
            OrderIndex = 12,
            Type = "dictation",
            Title = "Dictation mở rộng (Bài 3.1): 15 câu nghe chép chính tả phản xạ",
            Description = "Nghe câu hội thoại đa cấp độ và gõ lại chính xác từng từ hoặc chọn word chip để rèn luyện thính giác tiếng Anh.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l31-dict-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Lời chào mở đầu bài nói (Dễ)"",
      ""sentence"": ""Thank you for joining today's presentation on cloud strategy."",
      ""ipa"": ""/θæŋk juː fɔː ˈdʒɔɪnɪŋ təˈdeɪz ˌpreznˈteɪʃn ɒn klaʊd ˈstrætədʒi/"",
      ""meaningVi"": ""Cảm ơn các bạn đã tham gia buổi thuyết trình hôm nay về chiến lược đám mây."",
      ""wordChips"": [
        ""Thank"",
        ""you"",
        ""for"",
        ""joining"",
        ""today's"",
        ""presentation"",
        ""on"",
        ""cloud"",
        ""strategy""
      ],
      ""distractors"": [
        ""attending"",
        ""talk"",
        ""system"",
        ""meeting""
      ]
    },
    {
      ""id"": ""en-l31-dict-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Chỉ vào slide hiển thị (Dễ)"",
      ""sentence"": ""Take a look at the bar chart on slide four."",
      ""ipa"": ""/teɪk ə lʊk æt ðə bɑː tʃɑːt ɒn slaɪd fɔː/"",
      ""meaningVi"": ""Xin hãy nhìn vào biểu đồ cột trên slide số bốn."",
      ""wordChips"": [
        ""Take"",
        ""a"",
        ""look"",
        ""at"",
        ""the"",
        ""bar"",
        ""chart"",
        ""on"",
        ""slide"",
        ""four""
      ],
      ""distractors"": [
        ""see"",
        ""view"",
        ""graph"",
        ""page""
      ]
    },
    {
      ""id"": ""en-l31-dict-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Báo cáo doanh số tăng (Dễ)"",
      ""sentence"": ""Our revenue grew steadily over the past twelve months."",
      ""ipa"": ""/ˈaʊə ˈrevənjuː ɡruː ˈstedəli ˈəʊvə ðə pɑːst twelv mʌnθs/"",
      ""meaningVi"": ""Doanh thu của chúng tôi đã tăng trưởng đều đặn trong 12 tháng qua."",
      ""wordChips"": [
        ""Our"",
        ""revenue"",
        ""grew"",
        ""steadily"",
        ""over"",
        ""the"",
        ""past"",
        ""twelve"",
        ""months""
      ],
      ""distractors"": [
        ""profit"",
        ""rose"",
        ""fast"",
        ""year""
      ]
    },
    {
      ""id"": ""en-l31-dict-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Chuyển tiếp sang luận điểm tiếp (Dễ)"",
      ""sentence"": ""Next, let us explore our key technological advantages."",
      ""ipa"": ""/nekst, let ʌs ɪkˈsplɔːr ˈaʊə kiː ˌteknəˈlɒdʒɪkl ədˈvɑːntɪdʒɪz/"",
      ""meaningVi"": ""Tiếp theo, chúng ta hãy cùng khám phá các lợi thế công nghệ cốt lõi của mình."",
      ""wordChips"": [
        ""Next"",
        ""let"",
        ""us"",
        ""explore"",
        ""our"",
        ""key"",
        ""technological"",
        ""advantages""
      ],
      ""distractors"": [
        ""then"",
        ""see"",
        ""main"",
        ""benefits""
      ]
    },
    {
      ""id"": ""en-l31-dict-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Tóm tắt bài nói (Dễ)"",
      ""sentence"": ""In conclusion, continuous practice brings great long-term success."",
      ""ipa"": ""/ɪn kənˈkluːʒn, kənˈtɪnjuəs ˈpræktɪs brɪŋz ɡreɪt ˈlɒŋ tɜːm səkˈses/"",
      ""meaningVi"": ""Tóm lại, sự luyện tập liên tục mang lại thành công lớn lâu dài."",
      ""wordChips"": [
        ""In"",
        ""conclusion"",
        ""continuous"",
        ""practice"",
        ""brings"",
        ""great"",
        ""long-term"",
        ""success""
      ],
      ""distractors"": [
        ""finally"",
        ""learning"",
        ""leads"",
        ""growth""
      ]
    },
    {
      ""id"": ""en-l31-dict-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Dẫn dắt người nghe bằng số liệu (Vừa)"",
      ""sentence"": ""Direct your attention to the line graph demonstrating our customer retention surge."",
      ""ipa"": ""/daɪˈrekt jɔːr əˈtenʃn tuː ðə laɪn ɡrɑːf ˈdemənstreɪtɪŋ ˈaʊə ˈkʌstəmə rɪˈtenʃn sɜːdʒ/"",
      ""meaningVi"": ""Hãy hướng sự chú ý lên biểu đồ đường minh họa sự tăng vọt về tỷ lệ giữ chân khách hàng."",
      ""wordChips"": [
        ""Direct"",
        ""your"",
        ""attention"",
        ""to"",
        ""the"",
        ""line"",
        ""graph"",
        ""demonstrating"",
        ""our"",
        ""customer"",
        ""retention"",
        ""surge""
      ],
      ""distractors"": [
        ""turn"",
        ""focus"",
        ""chart"",
        ""showing""
      ]
    },
    {
      ""id"": ""en-l31-dict-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Từ nối dẫn dắt chuyển ý (Vừa)"",
      ""sentence"": ""Moving on to our second main objective, let us examine system availability."",
      ""ipa"": ""/ˈmuːvɪŋ ɒn tuː ˈaʊə ˈsekənd meɪn əbˈdʒektɪv, let ʌs ɪɡˈzæmɪn ˈsɪstəm əˌveɪləˈbɪləti/"",
      ""meaningVi"": ""Chuyển sang mục tiêu chính thứ hai của chúng ta, hãy cùng xem xét mức độ sẵn sàng của hệ thống."",
      ""wordChips"": [
        ""Moving"",
        ""on"",
        ""to"",
        ""our"",
        ""second"",
        ""main"",
        ""objective"",
        ""let"",
        ""us"",
        ""examine"",
        ""system"",
        ""availability""
      ],
      ""distractors"": [
        ""turning"",
        ""next"",
        ""goal"",
        ""check""
      ]
    },
    {
      ""id"": ""en-l31-dict-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Minh họa xu hướng chững lại (Vừa)"",
      ""sentence"": ""While initial downloads surged, active engagement plateaued during the third quarter."",
      ""ipa"": ""/waɪl ɪˈnɪʃl ˌdaʊnˈləʊdz sɜːdʒd, ˈæktɪv ɪnˈɡeɪdʒmənt ˈplætəʊd ˈdjʊərɪŋ ðə θɜːd ˈkwɔːtə/"",
      ""meaningVi"": ""Dù lượng tải ban đầu tăng vọt, mức độ gắn kết chủ động đã chững lại trong suốt quý ba."",
      ""wordChips"": [
        ""While"",
        ""initial"",
        ""downloads"",
        ""surged"",
        ""active"",
        ""engagement"",
        ""plateaued"",
        ""during"",
        ""the"",
        ""third"",
        ""quarter""
      ],
      ""distractors"": [
        ""although"",
        ""rose"",
        ""users"",
        ""stopped""
      ]
    },
    {
      ""id"": ""en-l31-dict-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Nhấn mạnh bài học then chốt (Vừa)"",
      ""sentence"": ""The single most crucial takeaway is that automation preserves engineering bandwidth."",
      ""ipa"": ""/ðə ˈsɪŋɡl məʊst ˈkruːʃl ˈteɪkəweɪ ɪz ðæt ˌɔːtəˈmeɪʃn prɪˈzɜːvz ˌendʒɪˈnɪərɪŋ ˈbændwɪdθ/"",
      ""meaningVi"": ""Bài học quan trọng duy nhất cần nhớ là tự động hóa bảo toàn thời gian và năng lực của kỹ sư."",
      ""wordChips"": [
        ""The"",
        ""single"",
        ""most"",
        ""crucial"",
        ""takeaway"",
        ""is"",
        ""that"",
        ""automation"",
        ""preserves"",
        ""engineering"",
        ""bandwidth""
      ],
      ""distractors"": [
        ""one"",
        ""important"",
        ""lesson"",
        ""saves""
      ]
    },
    {
      ""id"": ""en-l31-dict-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Kêu gọi hành động dứt khoát (Vừa)"",
      ""sentence"": ""I encourage every team lead to implement these automated quality checks today."",
      ""ipa"": ""/aɪ ɪnˈkʌrɪdʒ ˈevri tiːm liːd tuː ˈɪmplɪment ðiːz ˈɔːtəmeɪtɪd ˈkwɒləti tʃeks təˈdeɪ/"",
      ""meaningVi"": ""Tôi khuyến khích từng trưởng nhóm hãy áp dụng các bước kiểm tra chất lượng tự động này ngay hôm nay."",
      ""wordChips"": [
        ""I"",
        ""encourage"",
        ""every"",
        ""team"",
        ""lead"",
        ""to"",
        ""implement"",
        ""these"",
        ""automated"",
        ""quality"",
        ""checks"",
        ""today""
      ],
      ""distractors"": [
        ""urge"",
        ""manager"",
        ""apply"",
        ""rules""
      ]
    },
    {
      ""id"": ""en-l31-dict-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Phân tích số liệu vi phân (Khó)"",
      ""sentence"": ""Statistical regression models convincingly demonstrate that latency optimizations directly correlate with customer retention."",
      ""ipa"": ""/stəˈtɪstɪkl rɪˈɡreʃn ˈmɒdlz kənˈvɪnsɪŋli ˈdemənstreɪt ðæt ˈleɪtənsi ˌɒptɪmaɪˈzeɪʃnz dəˈrektli ˈkɒrəleɪt wɪð ˈkʌstəmə rɪˈtenʃn/"",
      ""meaningVi"": ""Các mô hình hồi quy thống kê chứng minh đầy thuyết phục rằng việc tối ưu hóa độ trễ có tương quan trực tiếp với việc giữ chân khách hàng."",
      ""wordChips"": [
        ""Statistical"",
        ""regression"",
        ""models"",
        ""convincingly"",
        ""demonstrate"",
        ""that"",
        ""latency"",
        ""optimizations"",
        ""directly"",
        ""correlate"",
        ""with"",
        ""customer"",
        ""retention""
      ],
      ""distractors"": [
        ""analytical"",
        ""tests"",
        ""show"",
        ""linked""
      ]
    },
    {
      ""id"": ""en-l31-dict-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Tổng hợp kết quả nghiên cứu (Khó)"",
      ""sentence"": ""Synthesizing disparate qualitative feedback with telemetry metrics yielded actionable architectural recommendations."",
      ""ipa"": ""/ˈsɪnθəsaɪzɪŋ ˈdɪspərət ˈkwɒlɪtətɪv ˈfiːdbæk wɪð təˈlemətri ˈmetrɪks ˈjiːldɪd ˈækʃənəbl ˌɑːkɪˈtektʃərəl ˌrekəmenˈdeɪʃnz/"",
      ""meaningVi"": ""Tổng hợp các phản hồi định tính đa dạng với các số liệu đo lường hệ thống đã mang lại những khuyến nghị kiến trúc có tính khả thi cao."",
      ""wordChips"": [
        ""Synthesizing"",
        ""disparate"",
        ""qualitative"",
        ""feedback"",
        ""with"",
        ""telemetry"",
        ""metrics"",
        ""yielded"",
        ""actionable"",
        ""architectural"",
        ""recommendations""
      ],
      ""distractors"": [
        ""combining"",
        ""different"",
        ""data"",
        ""proposals""
      ]
    },
    {
      ""id"": ""en-l31-dict-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Dẫn dắt thuyết trình cuốn hút (Khó)"",
      ""sentence"": ""Eloquent speakers seamlessly integrate rhetorical pauses to underscore critical strategic paradigms."",
      ""ipa"": ""/ˈeləkwənt ˈspiːkəz ˈsiːmləsli ˈɪntɪɡreɪt rɪˈtɒrɪkl ˈpɔːzɪz tuː ˌʌndəˈskɔː ˈkrɪtɪkl strəˈtiːdʒɪk ˈpærədaɪmz/"",
      ""meaningVi"": ""Những diễn giả tài ba lồng ghép mượt mà các khoảng dừng tu từ để nêu bật các mô hình chiến lược trọng yếu."",
      ""wordChips"": [
        ""Eloquent"",
        ""speakers"",
        ""seamlessly"",
        ""integrate"",
        ""rhetorical"",
        ""pauses"",
        ""to"",
        ""underscore"",
        ""critical"",
        ""strategic"",
        ""paradigms""
      ],
      ""distractors"": [
        ""skilled"",
        ""orators"",
        ""use"",
        ""highlight""
      ]
    },
    {
      ""id"": ""en-l31-dict-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Khẳng định giá trị đòn bẩy cao (Khó)"",
      ""sentence"": ""Disciplined execution of high-leverage engineering initiatives consistently surpasses optimistic commercial forecasts."",
      ""ipa"": ""/ˈdɪsəplɪnd ˌeksɪˈkjuːʃn əv haɪ ˈliːvərɪdʒ ˌendʒɪˈnɪərɪŋ ɪˈnɪʃətɪvz kənˈsɪstəntli səˈpɑːsɪz ˌɒptɪˈmɪstɪk kəˈmɜːʃl ˈfɔːkɑːsts/"",
      ""meaningVi"": ""Việc thực thi có kỷ luật các sáng kiến kỹ thuật có đòn bẩy cao luôn vượt qua các dự báo thương mại lạc quan."",
      ""wordChips"": [
        ""Disciplined"",
        ""execution"",
        ""of"",
        ""high-leverage"",
        ""engineering"",
        ""initiatives"",
        ""consistently"",
        ""surpasses"",
        ""optimistic"",
        ""commercial"",
        ""forecasts""
      ],
      ""distractors"": [
        ""strict"",
        ""delivery"",
        ""projects"",
        ""beats""
      ]
    },
    {
      ""id"": ""en-l31-dict-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Đúc kết triết lý tổ chức (Khó)"",
      ""sentence"": ""Cultivating an organizational ethos centered on transparency empowers squads to navigate volatile market landscapes."",
      ""ipa"": ""/ˈkʌltɪveɪtɪŋ ən ˌɔːɡənaɪˈzeɪʃənl ˈiːθɒs ˈsentəd ɒn trænsˈpærənsi ɪmˈpaʊəz skwɒdz tuː ˈnævɪɡeɪt ˈvɒlətaɪl ˈmɑːkɪt ˈlændskeɪps/"",
      ""meaningVi"": ""Nuôi dưỡng một tinh thần tổ chức xoay quanh sự minh bạch trao quyền cho các đội nhóm vượt qua những bối cảnh thị trường biến động."",
      ""wordChips"": [
        ""Cultivating"",
        ""an"",
        ""organizational"",
        ""ethos"",
        ""centered"",
        ""on"",
        ""transparency"",
        ""empowers"",
        ""squads"",
        ""to"",
        ""navigate"",
        ""volatile"",
        ""market"",
        ""landscapes""
      ],
      ""distractors"": [
        ""building"",
        ""culture"",
        ""based"",
        ""teams""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.1 Pronunciation (OrderIndex 13)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444450"), "pronunciation", 13),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444450"),
            OrderIndex = 13,
            Type = "pronunciation",
            Title = "Chấm phát âm mở rộng (Bài 3.1): 15 câu luyện âm /θ/-/ð/, âm cuối và đuôi -s/-ed",
            Description = "Luyện phát âm với AI chấm điểm trực tiếp, phát hiện lỗi sai âm đuôi và các âm khó phổ biến của người Việt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l31-pron-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Trọng âm động từ present (Dễ)"",
      ""sentence"": ""Today I present our annual product performance."",
      ""ipa"": ""/təˈdeɪ aɪ prɪˈzent ˈaʊər ˈænjuəl ˈprɒdʌkt pəˈfɔːməns/"",
      ""meaningVi"": ""Hôm nay tôi trình bày hiệu quả sản phẩm hàng năm của chúng tôi."",
      ""targetWords"": [
        ""present"",
        ""product"",
        ""performance""
      ],
      ""keyPhrases"": [
        ""present performance"",
        ""annual product""
      ]
    },
    {
      ""id"": ""en-l31-pron-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /tʃ/ trong chart & slide (Dễ)"",
      ""sentence"": ""This line chart illustrates steady revenue growth."",
      ""ipa"": ""/ðɪs laɪn tʃɑːt ˈɪləstreɪts ˈstedi ˈrevənjuː ɡrəʊθ/"",
      ""meaningVi"": ""Biểu đồ đường này minh họa sự tăng trưởng doanh thu đều đặn."",
      ""targetWords"": [
        ""chart"",
        ""illustrates"",
        ""growth""
      ],
      ""keyPhrases"": [
        ""line chart"",
        ""steady growth""
      ]
    },
    {
      ""id"": ""en-l31-pron-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm cuối /z/ trong shows & figures (Dễ)"",
      ""sentence"": ""The slide shows three important financial figures."",
      ""ipa"": ""/ðə slaɪd ʃəʊz θriː ɪmˈpɔːtnt faɪˈnænʃl ˈfɪɡəz/"",
      ""meaningVi"": ""Slide này hiển thị ba số liệu tài chính quan trọng."",
      ""targetWords"": [
        ""shows"",
        ""three"",
        ""figures""
      ],
      ""keyPhrases"": [
        ""slide shows"",
        ""financial figures""
      ]
    },
    {
      ""id"": ""en-l31-pron-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /θ/ trong thank & fourth (Dễ)"",
      ""sentence"": ""Thank you as we enter the fourth quarter."",
      ""ipa"": ""/θæŋk juː æz wiː ˈentə ðə fɔːθ ˈkwɔːtə/"",
      ""meaningVi"": ""Cảm ơn bạn khi chúng ta bước vào quý thứ tư."",
      ""targetWords"": [
        ""thank"",
        ""fourth"",
        ""quarter""
      ],
      ""keyPhrases"": [
        ""thank you"",
        ""fourth quarter""
      ]
    },
    {
      ""id"": ""en-l31-pron-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /v/ trong move & conclusion (Dễ)"",
      ""sentence"": ""Let us move directly to our final conclusion."",
      ""ipa"": ""/let ʌs muːv dəˈrektli tuː ˈaʊə ˈfaɪnl kənˈkluːʒn/"",
      ""meaningVi"": ""Chúng ta hãy đi thẳng tới phần kết luận cuối cùng."",
      ""targetWords"": [
        ""move"",
        ""conclusion""
      ],
      ""keyPhrases"": [
        ""move directly"",
        ""final conclusion""
      ]
    },
    {
      ""id"": ""en-l31-pron-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Trọng âm âm 1 trong demonstrate (Vừa)"",
      ""sentence"": ""Today I demonstrate our revolutionary data caching architecture."",
      ""ipa"": ""/təˈdeɪ aɪ ˈdemənstreɪt ˈaʊə ˌrevəˈluːʃənri ˈdeɪtə ˈkæʃɪŋ ˈɑːkɪtektʃə/"",
      ""meaningVi"": ""Hôm nay tôi chứng minh kiến trúc lưu đệm dữ liệu mang tính cách mạng của chúng tôi."",
      ""targetWords"": [
        ""demonstrate"",
        ""revolutionary"",
        ""architecture""
      ],
      ""keyPhrases"": [
        ""demonstrate architecture"",
        ""data caching""
      ]
    },
    {
      ""id"": ""en-l31-pron-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /st/ trong signposting language (Vừa)"",
      ""sentence"": ""Mastering signposting language keeps listeners thoroughly engaged."",
      ""ipa"": ""/ˈmɑːstərɪŋ ˈsaɪnpəʊstɪŋ ˈlæŋɡwɪdʒ kiːps ˈlɪsnəz ˈθʌrəli ɪnˈɡeɪdʒd/"",
      ""meaningVi"": ""Làm chủ ngôn ngữ dẫn dắt giúp người nghe luôn chú ý cao độ."",
      ""targetWords"": [
        ""signposting"",
        ""listeners"",
        ""engaged""
      ],
      ""keyPhrases"": [
        ""signposting language"",
        ""thoroughly engaged""
      ]
    },
    {
      ""id"": ""en-l31-pron-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /s/ vs /ʃ/ trong exponential surge (Vừa)"",
      ""sentence"": ""The bar chart showcases an exponential surge in adoption."",
      ""ipa"": ""/ðə bɑː tʃɑːt ˈʃəʊkeɪsɪz ən ˌekspəˈnenʃl sɜːdʒ ɪn əˈdɒpʃn/"",
      ""meaningVi"": ""Biểu đồ cột nêu bật sự tăng trưởng cấp số nhân về số lượng người dùng."",
      ""targetWords"": [
        ""showcases"",
        ""exponential"",
        ""surge"",
        ""adoption""
      ],
      ""keyPhrases"": [
        ""bar chart showcases"",
        ""exponential surge""
      ]
    },
    {
      ""id"": ""en-l31-pron-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm đuôi -ed dạng /d/ trong plateaued (Vừa)"",
      ""sentence"": ""User growth plateaued until we launched automated updates."",
      ""ipa"": ""/ˈjuːzə ɡrəʊθ ˈplætəʊd ənˈtɪl wiː lɔːntʃt ˈɔːtəmeɪtɪd ˌʌpˈdeɪts/"",
      ""meaningVi"": ""Tăng trưởng người dùng đã chững lại cho đến khi chúng tôi phát hành các bản cập nhật tự động."",
      ""targetWords"": [
        ""plateaued"",
        ""launched"",
        ""automated"",
        ""updates""
      ],
      ""keyPhrases"": [
        ""growth plateaued"",
        ""automated updates""
      ]
    },
    {
      ""id"": ""en-l31-pron-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Nối âm trong call to action (Vừa)"",
      ""sentence"": ""We conclude our pitch with an inspiring call to action."",
      ""ipa"": ""/wiː kənˈkluːd ˈaʊə pɪtʃ wɪð ən ɪnˈspaɪərɪŋ kɔːl tuː ˈækʃn/"",
      ""meaningVi"": ""Chúng tôi khép lại bài thuyết trình bằng một lời kêu gọi hành động truyền cảm hứng."",
      ""targetWords"": [
        ""conclude"",
        ""pitch"",
        ""inspiring"",
        ""action""
      ],
      ""keyPhrases"": [
        ""call to action"",
        ""inspiring pitch""
      ]
    },
    {
      ""id"": ""en-l31-pron-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Trọng âm từ phức & âm /kw/ trong eloquence (Khó)"",
      ""sentence"": ""Articulate eloquence empowers leaders to communicate complex telemetry effortlessly."",
      ""ipa"": ""/ɑːˈtɪkjələt ˈeləkwəns ɪmˈpaʊəz ˈliːdəz tuː kəˈmjuːnɪkeɪt ˈkɒmpleks təˈlemətri ˈefətləsli/"",
      ""meaningVi"": ""Tài diễn đạt hùng hồn trao quyền cho các nhà lãnh đạo truyền đạt dữ liệu viễn thám phức tạp một cách dễ dàng."",
      ""targetWords"": [
        ""articulate"",
        ""eloquence"",
        ""telemetry"",
        ""effortlessly""
      ],
      ""keyPhrases"": [
        ""articulate eloquence"",
        ""complex telemetry""
      ]
    },
    {
      ""id"": ""en-l31-pron-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cụm phụ âm /nd/ & /st/ trong substantiated statistical (Khó)"",
      ""sentence"": ""Substantiated statistical regression models validated our strategic hypothesis."",
      ""ipa"": ""/səbˈstænʃieɪtɪd stəˈtɪstɪkl rɪˈɡreʃn ˈmɒdlz ˈvælɪdeɪtɪd ˈaʊə strəˈtiːdʒɪk haɪˈpɒθəsɪs/"",
      ""meaningVi"": ""Các mô hình hồi quy thống kê có căn cứ đã kiểm chứng giả thuyết chiến lược của chúng tôi."",
      ""targetWords"": [
        ""substantiated"",
        ""statistical"",
        ""regression"",
        ""hypothesis""
      ],
      ""keyPhrases"": [
        ""statistical regression"",
        ""strategic hypothesis""
      ]
    },
    {
      ""id"": ""en-l31-pron-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Âm /θ/ trong synthesizing theories (Khó)"",
      ""sentence"": ""Synthesizing qualitative theories with empirical benchmark testing convinces skeptical executives."",
      ""ipa"": ""/ˈsɪnθəsaɪzɪŋ ˈkwɒlɪtətɪv ˈθɪəriz wɪð ɪmˈpɪrɪkl ˈbentʃmɑːk ˈtestɪŋ kənˈvɪnsɪz ˈskeptɪkl ɪɡˈzekjətɪvz/"",
      ""meaningVi"": ""Tổng hợp các lý thuyết định tính với thử nghiệm chuẩn thực tế thuyết phục các nhà điều hành hoài nghi."",
      ""targetWords"": [
        ""synthesizing"",
        ""qualitative"",
        ""theories"",
        ""empirical"",
        ""skeptical""
      ],
      ""keyPhrases"": [
        ""synthesizing theories"",
        ""empirical benchmark""
      ]
    },
    {
      ""id"": ""en-l31-pron-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Âm /v/ vs /w/ & /ð/ trong worthwhile viewpoints (Khó)"",
      ""sentence"": ""We thoroughly examine whether worthwhile viewpoints resonate with seasoned board members."",
      ""ipa"": ""/wiː ˈθʌrəli ɪɡˈzæmɪn ˈweðə ˈwɜːθwaɪl ˈvjuːpɔɪnts ˈrezəneɪt wɪð ˈsiːznd bɔːd ˈmembəz/"",
      ""meaningVi"": ""Chúng tôi xem xét kỹ lưỡng liệu những góc nhìn đáng giá có tạo được sự đồng cảm với các thành viên hội đồng dày dạn kinh nghiệm không."",
      ""targetWords"": [
        ""thoroughly"",
        ""whether"",
        ""worthwhile"",
        ""viewpoints"",
        ""resonate""
      ],
      ""keyPhrases"": [
        ""worthwhile viewpoints"",
        ""resonate with members""
      ]
    },
    {
      ""id"": ""en-l31-pron-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cụm phụ âm cuối /kts/ & /sts/ trong distinct catalysts (Khó)"",
      ""sentence"": ""Presenting distinct catalysts reinforces confidence among seasoned venture capitalists."",
      ""ipa"": ""/prɪˈzentɪŋ dɪˈstɪŋkt ˈkætəlɪsts ˌriːɪnˈfɔːsɪz ˈkɒnfɪdəns əˈmʌŋ ˈsiːznd ˈventʃə ˈkæpɪtəlɪsts/"",
      ""meaningVi"": ""Trình bày những nhân tố xúc tác khác biệt củng cố niềm tin giữa các nhà đầu tư mạo hiểm dày dạn."",
      ""targetWords"": [
        ""presenting"",
        ""distinct"",
        ""catalysts"",
        ""reinforces"",
        ""capitalists""
      ],
      ""keyPhrases"": [
        ""distinct catalysts"",
        ""venture capitalists""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.1 Word Map (OrderIndex 14)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444450"), "word_map", 14),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444450"),
            OrderIndex = 14,
            Type = "word_map",
            Title = "Sơ đồ từ vựng Mind Map mở rộng (Bài 3.1): 12 sơ đồ Dễ → Vừa → Khó",
            Description = "Hệ thống sơ đồ tư duy từ vựng đa cấp độ kèm phiên âm IPA, collocations, phrasal verbs, synonyms và ví dụ ngữ cảnh thực chiến.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""wm-l31-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: present"",
      ""centralWord"": ""present"",
      ""ipa"": ""/prɪˈzent/"",
      ""meaningVi"": ""Thuyết trình, trình bày, giới thiệu"",
      ""collocations"": [
        {
          ""text"": ""present findings"",
          ""ipa"": ""/prɪˈzent ˈfaɪndɪŋz/"",
          ""meaningVi"": ""Trình bày kết quả nghiên cứu"",
          ""example"": ""She will present findings to the executive team.""
        },
        {
          ""text"": ""present ideas"",
          ""ipa"": ""/prɪˈzent aɪˈdɪəz/"",
          ""meaningVi"": ""Trình bày ý tưởng"",
          ""example"": ""Learn to present ideas concisely and clearly.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""lay out"",
          ""ipa"": ""/leɪ aʊt/"",
          ""meaningVi"": ""Bày ra, giải thích rõ"",
          ""example"": ""The slide lays out our three core strategic pillars.""
        },
        {
          ""text"": ""put across"",
          ""ipa"": ""/pʊt əˈkrɒs/"",
          ""meaningVi"": ""Truyền đạt hiệu quả"",
          ""example"": ""He put across his message with exceptional poise.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""showcase"",
          ""ipa"": ""/ˈʃəʊkeɪs/"",
          ""meaningVi"": ""Phô diễn, trưng bày"",
          ""example"": ""The demo showcases our prototype capability.""
        },
        {
          ""text"": ""introduce"",
          ""ipa"": ""/ˌɪntrəˈdjuːs/"",
          ""meaningVi"": ""Giới thiệu"",
          ""example"": ""Let me introduce our innovative product strategy.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Presenting complex ideas concisely is an indispensable leadership skill."",
          ""meaningVi"": ""Trình bày những ý tưởng phức tạp một cách súc tích là kỹ năng lãnh đạo không thể thiếu.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: chart"",
      ""centralWord"": ""chart"",
      ""ipa"": ""/tʃɑːt/"",
      ""meaningVi"": ""Biểu đồ, sơ đồ minh họa"",
      ""collocations"": [
        {
          ""text"": ""bar chart"",
          ""ipa"": ""/bɑː tʃɑːt/"",
          ""meaningVi"": ""Biểu đồ cột"",
          ""example"": ""This bar chart compares quarterly revenue across regions.""
        },
        {
          ""text"": ""pie chart"",
          ""ipa"": ""/paɪ tʃɑːt/"",
          ""meaningVi"": ""Biểu đồ tròn"",
          ""example"": ""The pie chart displays market share percentages.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""point to"",
          ""ipa"": ""/pɔɪnt tuː/"",
          ""meaningVi"": ""Chỉ vào"",
          ""example"": ""The speaker pointed to the sharp inflection on the graph.""
        },
        {
          ""text"": ""back up"",
          ""ipa"": ""/bæk ʌp/"",
          ""meaningVi"": ""Hậu thuẫn bằng số liệu"",
          ""example"": ""Charts back up your qualitative arguments with hard data.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""graph"",
          ""ipa"": ""/ɡrɑːf/"",
          ""meaningVi"": ""Đồ thị"",
          ""example"": ""The line graph demonstrates accelerating momentum.""
        },
        {
          ""text"": ""diagram"",
          ""ipa"": ""/ˈdaɪəɡræm/"",
          ""meaningVi"": ""Sơ đồ"",
          ""example"": ""Refer to the architectural diagram on slide four.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Visual charts distill voluminous raw data into immediate, actionable insights."",
          ""meaningVi"": ""Các biểu đồ trực quan chưng cất khối dữ liệu thô khổng lồ thành những hiểu biết hành động tức thì.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: summary"",
      ""centralWord"": ""summary"",
      ""ipa"": ""/ˈsʌməri/"",
      ""meaningVi"": ""Bản tóm tắt, phần tổng kết"",
      ""collocations"": [
        {
          ""text"": ""executive summary"",
          ""ipa"": ""/ɪɡˈzekjətɪv ˈsʌməri/"",
          ""meaningVi"": ""Bản tóm tắt điều hành"",
          ""example"": ""Start your pitch deck with a crisp executive summary.""
        },
        {
          ""text"": ""in summary"",
          ""ipa"": ""/ɪn ˈsʌməri/"",
          ""meaningVi"": ""Tóm lại"",
          ""example"": ""In summary, our investments have yielded substantial dividends.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""sum up"",
          ""ipa"": ""/sʌm ʌp/"",
          ""meaningVi"": ""Đúc kết, tóm lại"",
          ""example"": ""To sum up, continuous learning accelerates engineering speed.""
        },
        {
          ""text"": ""boil down to"",
          ""ipa"": ""/bɔɪl daʊn tuː/"",
          ""meaningVi"": ""Quy tụ về cốt lõi"",
          ""example"": ""Our entire strategy boils down to customer empathy.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""recap"",
          ""ipa"": ""/ˈriːkæp/"",
          ""meaningVi"": ""Điểm lại"",
          ""example"": ""Let's do a quick recap before taking questions.""
        },
        {
          ""text"": ""overview"",
          ""ipa"": ""/ˈəʊvəvjuː/"",
          ""meaningVi"": ""Tổng quan"",
          ""example"": ""This overview highlights our primary growth vectors.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""An effective summary crystallizes core concepts and reinforces audience memory retention."",
          ""meaningVi"": ""Một phần tóm tắt hiệu quả kết tinh các khái niệm cốt lõi và củng cố khả năng ghi nhớ của khán giả.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: audience"",
      ""centralWord"": ""audience"",
      ""ipa"": ""/ˈɔːdiəns/"",
      ""meaningVi"": ""Khán giả, thính giả, người nghe"",
      ""collocations"": [
        {
          ""text"": ""target audience"",
          ""ipa"": ""/ˈtɑːɡɪt ˈɔːdiəns/"",
          ""meaningVi"": ""Khán giả mục tiêu"",
          ""example"": ""Tailor your presentation vocabulary to your target audience.""
        },
        {
          ""text"": ""engage the audience"",
          ""ipa"": ""/ɪnˈɡeɪdʒ ði ˈɔːdiəns/"",
          ""meaningVi"": ""Thu hút sự tương tác của khán giả"",
          ""example"": ""Asking thought-provoking questions engages the audience.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""connect with"",
          ""ipa"": ""/kəˈnekt wɪð/"",
          ""meaningVi"": ""Tạo kết nối với"",
          ""example"": ""Great speakers connect with their audience through stories.""
        },
        {
          ""text"": ""hold onto"",
          ""ipa"": ""/həʊld ˈɒntuː/"",
          ""meaningVi"": ""Giữ chặt sự chú ý"",
          ""example"": ""Dynamic vocal variety holds onto audience attention.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""listeners"",
          ""ipa"": ""/ˈlɪsnəz/"",
          ""meaningVi"": ""Người nghe"",
          ""example"": ""Keep your listeners intrigued with signposting language.""
        },
        {
          ""text"": ""attendees"",
          ""ipa"": ""/əˌtenˈdiːz/"",
          ""meaningVi"": ""Người tham dự"",
          ""example"": ""Over five hundred attendees packed the keynote auditorium.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Audience empathy is the foundation upon which all captivating public speaking is built."",
          ""meaningVi"": ""Sự thấu cảm khán giả là nền móng để xây dựng nên mọi bài phát biểu công chúng lôi cuốn.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: demonstrate"",
      ""centralWord"": ""demonstrate"",
      ""ipa"": ""/ˈdemənstreɪt/"",
      ""meaningVi"": ""Chứng minh, thị phạm, làm sáng tỏ"",
      ""collocations"": [
        {
          ""text"": ""demonstrate capability"",
          ""ipa"": ""/ˈdemənstreɪt ˌkeɪpəˈbɪləti/"",
          ""meaningVi"": ""Chứng minh năng lực"",
          ""example"": ""This live demo clearly demonstrates our technical capability.""
        },
        {
          ""text"": ""convincingly demonstrate"",
          ""ipa"": ""/kənˈvɪnsɪŋli ˈdemənstreɪt/"",
          ""meaningVi"": ""Chứng minh đầy thuyết phục"",
          ""example"": ""Telemetry data convincingly demonstrates improved latency.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""point out"",
          ""ipa"": ""/pɔɪnt aʊt/"",
          ""meaningVi"": ""Chỉ ra cho thấy"",
          ""example"": ""The presenter pointed out the hidden architectural bottlenecks.""
        },
        {
          ""text"": ""bear out"",
          ""ipa"": ""/beər aʊt/"",
          ""meaningVi"": ""Xác thực, minh chứng"",
          ""example"": ""Quarterly earnings bore out our optimistic predictions.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""prove"",
          ""ipa"": ""/pruːv/"",
          ""meaningVi"": ""Chứng minh"",
          ""example"": ""Empirical tests prove the system's resilience.""
        },
        {
          ""text"": ""manifest"",
          ""ipa"": ""/ˈmænɪfest/"",
          ""meaningVi"": ""Biểu hiện rõ"",
          ""example"": ""Team alignment manifests in rapid sprint delivery.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""A well-structured pitch demonstrates analytical rigor alongside strategic vision."",
          ""meaningVi"": ""Một bài thuyết trình có cấu trúc tốt thể hiện sự chặt chẽ về phân tích song hành cùng tầm nhìn chiến lược.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: illustrate"",
      ""centralWord"": ""illustrate"",
      ""ipa"": ""/ˈɪləstreɪt/"",
      ""meaningVi"": ""Minh họa, làm rõ qua ví dụ"",
      ""collocations"": [
        {
          ""text"": ""vividly illustrate"",
          ""ipa"": ""/ˈvɪvɪdli ˈɪləstreɪt/"",
          ""meaningVi"": ""Minh họa sống động"",
          ""example"": ""Relatable analogies vividly illustrate intricate concepts.""
        },
        {
          ""text"": ""illustrate a point"",
          ""ipa"": ""/ˈɪləstreɪt ə pɔɪnt/"",
          ""meaningVi"": ""Minh họa cho một luận điểm"",
          ""example"": ""Let me share a quick case study to illustrate this point.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""spell out"",
          ""ipa"": ""/spel aʊt/"",
          ""meaningVi"": ""Giải thích cặn kẽ"",
          ""example"": ""Let me spell out the primary advantages for developers.""
        },
        {
          ""text"": ""draw on"",
          ""ipa"": ""/drɔː ɒn/"",
          ""meaningVi"": ""Vận dụng dẫn chứng"",
          ""example"": ""She drew on historical data to illustrate market cycles.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""exemplify"",
          ""ipa"": ""/ɪɡˈzemplɪfaɪ/"",
          ""meaningVi"": ""Là ví dụ điển hình cho"",
          ""example"": ""This prototype exemplifies concise software engineering.""
        },
        {
          ""text"": ""clarify"",
          ""ipa"": ""/ˈklærəfaɪ/"",
          ""meaningVi"": ""Làm sáng tỏ"",
          ""example"": ""Diagrams clarify complex data pipelines instantly.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Illustrating dry statistics with real human stories transforms presentations into unforgettable experiences."",
          ""meaningVi"": ""Minh họa những con số khô khan bằng câu chuyện thực tế biến bài thuyết trình thành trải nghiệm khó quên.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: signpost"",
      ""centralWord"": ""signpost"",
      ""ipa"": ""/ˈsaɪnpəʊst/"",
      ""meaningVi"": ""Biển chỉ đường, từ nối dẫn dắt bài nói"",
      ""collocations"": [
        {
          ""text"": ""signposting language"",
          ""ipa"": ""/ˈsaɪnpəʊstɪŋ ˈlæŋɡwɪdʒ/"",
          ""meaningVi"": ""Ngôn ngữ chỉ đường dẫn ý"",
          ""example"": ""Using signposting language ensures smooth transitions between slides.""
        },
        {
          ""text"": ""clear signposts"",
          ""ipa"": ""/klɪə ˈsaɪnpəʊsts/"",
          ""meaningVi"": ""Các mốc chỉ dẫn rõ ràng"",
          ""example"": ""The presentation had clear signposts from opening to close.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""guide through"",
          ""ipa"": ""/ɡaɪd θruː/"",
          ""meaningVi"": ""Dẫn dắt đi qua"",
          ""example"": ""Signposts guide listeners through complicated financial models.""
        },
        {
          ""text"": ""transition into"",
          ""ipa"": ""/trænˈzɪʃn ˈɪntuː/"",
          ""meaningVi"": ""Chuyển tiếp mượt mà sang"",
          ""example"": ""The speaker transitioned smoothly into the product demo.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""indicator"",
          ""ipa"": ""/ˈɪndɪkeɪtə/"",
          ""meaningVi"": ""Dấu hiệu chỉ báo"",
          ""example"": ""These metrics are reliable indicators of system health.""
        },
        {
          ""text"": ""marker"",
          ""ipa"": ""/ˈmɑːkə/"",
          ""meaningVi"": ""Cột mốc đánh dấu"",
          ""example"": ""Signpost markers prevent audience disorientation.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Verbal signposts act as mental anchors that keep listeners aligned with your central narrative."",
          ""meaningVi"": ""Các từ chỉ dẫn bằng lời đóng vai trò như những điểm neo tư duy giúp người nghe theo sát mạch chuyện cốt lõi.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: highlight"",
      ""centralWord"": ""highlight"",
      ""ipa"": ""/ˈhaɪlaɪt/"",
      ""meaningVi"": ""Nêu bật, điểm sáng, nhấn mạnh"",
      ""collocations"": [
        {
          ""text"": ""highlight key achievements"",
          ""ipa"": ""/ˈhaɪlaɪt kiː əˈtʃiːvmənts/"",
          ""meaningVi"": ""Nêu bật thành tựu then chốt"",
          ""example"": ""This slide highlights key achievements from our latest sprint.""
        },
        {
          ""text"": ""presentation highlight"",
          ""ipa"": ""/ˌpreznˈteɪʃn ˈhaɪlaɪt/"",
          ""meaningVi"": ""Điểm nhấn của bài thuyết trình"",
          ""example"": ""The live hardware demo was the highlight of the summit.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""stand out"",
          ""ipa"": ""/stænd aʊt/"",
          ""meaningVi"": ""Nổi bật lên"",
          ""example"": ""Our low latency benchmark stands out among competitors.""
        },
        {
          ""text"": ""shine light on"",
          ""ipa"": ""/ʃaɪn laɪt ɒn/"",
          ""meaningVi"": ""Làm sáng tỏ vấn đề"",
          ""example"": ""The investigative study shined light on workflow bottlenecks.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""emphasize"",
          ""ipa"": ""/ˈemfəsaɪz/"",
          ""meaningVi"": ""Nhấn mạnh"",
          ""example"": ""The CEO emphasized disciplined operational execution.""
        },
        {
          ""text"": ""accentuate"",
          ""ipa"": ""/əkˈsentʃueɪt/"",
          ""meaningVi"": ""Làm đậm nét thêm"",
          ""example"": ""Contrast colors accentuate important chart series.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Highlighting empirical breakthroughs anchors your executive pitch in indisputable reality."",
          ""meaningVi"": ""Nêu bật những đột phá thực nghiệm neo giữ bài thuyết trình điều hành của bạn vào thực tế vững chắc.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: compelling"",
      ""centralWord"": ""compelling"",
      ""ipa"": ""/kəmˈpelɪŋ/"",
      ""meaningVi"": ""Hấp dẫn, giàu sức thuyết phục"",
      ""collocations"": [
        {
          ""text"": ""compelling narrative"",
          ""ipa"": ""/kəmˈpelɪŋ ˈnærətɪv/"",
          ""meaningVi"": ""Câu chuyện giàu sức hút"",
          ""example"": ""A compelling narrative converts skeptical investors into enthusiastic advocates.""
        },
        {
          ""text"": ""compelling value proposition"",
          ""ipa"": ""/kəmˈpelɪŋ ˈvæljuː ˌprɒpəˈzɪʃn/"",
          ""meaningVi"": ""Tuyên ngôn giá trị thuyết phục"",
          ""example"": ""Our product delivers a compelling value proposition to enterprise clients.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""win over"",
          ""ipa"": ""/wɪn ˈəʊvə/"",
          ""meaningVi"": ""Chinh phục lòng tin"",
          ""example"": ""Rigorous data won over the most conservative board members.""
        },
        {
          ""text"": ""draw in"",
          ""ipa"": ""/drɔː ɪn/"",
          ""meaningVi"": ""Lôi cuốn người nghe"",
          ""example"": ""Her evocative opening hook drew in the whole auditorium.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""persuasive"",
          ""ipa"": ""/pəˈsweɪsɪv/"",
          ""meaningVi"": ""Có tính thuyết phục"",
          ""example"": ""He delivered a remarkably persuasive address.""
        },
        {
          ""text"": ""irresistible"",
          ""ipa"": ""/ˌɪrɪˈzɪstəbl/"",
          ""meaningVi"": ""Không thể cưỡng lại"",
          ""example"": ""The cost savings presented an irresistible business case.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Crafting a compelling narrative transforms routine technical updates into transformative strategic milestones."",
          ""meaningVi"": ""Xây dựng câu chuyện thuyết phục biến những cập nhật kỹ thuật thông thường thành các mốc chiến lược mang tính chuyển hóa.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: exponential"",
      ""centralWord"": ""exponential"",
      ""ipa"": ""/ˌekspəˈnenʃl/"",
      ""meaningVi"": ""Cấp số nhân, tăng trưởng phi mã"",
      ""collocations"": [
        {
          ""text"": ""exponential growth"",
          ""ipa"": ""/ˌekspəˈnenʃl ɡrəʊθ/"",
          ""meaningVi"": ""Tăng trưởng cấp số nhân"",
          ""example"": ""Network effects stimulated exponential growth across our platform.""
        },
        {
          ""text"": ""exponential surge"",
          ""ipa"": ""/ˌekspəˈnenʃl sɜːdʒ/"",
          ""meaningVi"": ""Sự tăng vọt phi mã"",
          ""example"": ""We witnessed an exponential surge in mobile transaction volumes.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""take off"",
          ""ipa"": ""/teɪk ɒf/"",
          ""meaningVi"": ""Cất cánh bứt phá"",
          ""example"": ""Adoption metrics took off after the zero-friction onboarding release.""
        },
        {
          ""text"": ""shoot up"",
          ""ipa"": ""/ʃuːt ʌp/"",
          ""meaningVi"": ""Bắn vọt lên cao"",
          ""example"": ""Quarterly subscription renewals shot up dramatically.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""meteoric"",
          ""ipa"": ""/ˌmiːtiˈɒrɪk/"",
          ""meaningVi"": ""Nhanh như sao băng"",
          ""example"": ""The startup experienced a meteoric rise to market prominence.""
        },
        {
          ""text"": ""compounding"",
          ""ipa"": ""/kəmˈpaʊndɪŋ/"",
          ""meaningVi"": ""Tích lũy dồn dập"",
          ""example"": ""Compounding efficiency gains lowered unit infrastructure costs.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Demonstrating exponential trajectory requires showing scalable unit economics rather than mere raw volume."",
          ""meaningVi"": ""Chứng minh quỹ đạo tăng trưởng cấp số nhân đòi hỏi việc phô diễn hiệu quả kinh tế trên từng đơn vị thay vì chỉ là khối lượng thô.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: articulate"",
      ""centralWord"": ""articulate"",
      ""ipa"": ""/ɑːˈtɪkjuleɪt/"",
      ""meaningVi"": ""Diễn đạt lưu loát, gãy gọn, khúc chiết"",
      ""collocations"": [
        {
          ""text"": ""clearly articulate"",
          ""ipa"": ""/ˈklɪəli ɑːˈtɪkjuleɪt/"",
          ""meaningVi"": ""Diễn đạt rõ ràng"",
          ""example"": ""Leaders must clearly articulate strategic intent to diverse squads.""
        },
        {
          ""text"": ""articulate a vision"",
          ""ipa"": ""/ɑːˈtɪkjuleɪt ə ˈvɪʒn/"",
          ""meaningVi"": ""Truyền tải tầm nhìn"",
          ""example"": ""The founder articulated a bold vision for decentralization.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""flesh out"",
          ""ipa"": ""/fleʃ aʊt/"",
          ""meaningVi"": ""Đắp thêm chi tiết cho rõ"",
          ""example"": ""The presentation fleshed out the bare architectural outline.""
        },
        {
          ""text"": ""voice out"",
          ""ipa"": ""/vɔɪs aʊt/"",
          ""meaningVi"": ""Nói to rõ quan điểm"",
          ""example"": ""She voiced out the technical challenges during the pitch.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""express"",
          ""ipa"": ""/ɪkˈspres/"",
          ""meaningVi"": ""Bày tỏ"",
          ""example"": ""He expressed sophisticated engineering ideas with ease.""
        },
        {
          ""text"": ""formulate"",
          ""ipa"": ""/ˈfɔːmjuleɪt/"",
          ""meaningVi"": ""Công thức hóa, trình bày"",
          ""example"": ""Formulate your core thesis into one memorable sentence.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""The ability to articulate complex abstractions with crisp clarity is the defining trait of influential technology leaders."",
          ""meaningVi"": ""Khả năng diễn đạt các khái niệm trừu tượng phức tạp một cách trong trẻo, sắc sảo là phẩm chất quyết định của những nhà lãnh đạo công nghệ có tầm ảnh hưởng.""
        }
      ]
    },
    {
      ""id"": ""wm-l31-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: resonate"",
      ""centralWord"": ""resonate"",
      ""ipa"": ""/ˈrezəneɪt/"",
      ""meaningVi"": ""Cộng hưởng, đọng lại sâu sắc trong tâm trí"",
      ""collocations"": [
        {
          ""text"": ""resonate deeply with"",
          ""ipa"": ""/ˈrezəneɪt ˈdiːpli wɪð/"",
          ""meaningVi"": ""Cộng hưởng sâu sắc với"",
          ""example"": ""The mission resonated deeply with mission-driven software engineers.""
        },
        {
          ""text"": ""resonate across markets"",
          ""ipa"": ""/ˈrezəneɪt əˈkrɒs ˈmɑːkɪts/"",
          ""meaningVi"": ""Tạo tiếng vang qua các thị trường"",
          ""example"": ""Our customer-centric approach resonates across international markets.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""sink into"",
          ""ipa"": ""/sɪŋk ˈɪntuː/"",
          ""meaningVi"": ""Thấm sâu vào"",
          ""example"": ""Allow the strategic implications to sink into the minds of the board.""
        },
        {
          ""text"": ""strike home"",
          ""ipa"": ""/straɪk həʊm/"",
          ""meaningVi"": ""Chạm đúng tim đen"",
          ""example"": ""His frank critique of our technical debt struck home.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""echo"",
          ""ipa"": ""/ˈekəʊ/"",
          ""meaningVi"": ""Vang vọng, lặp lại"",
          ""example"": ""Her words echoed throughout the subsequent discussions.""
        },
        {
          ""text"": ""connect"",
          ""ipa"": ""/kəˈnekt/"",
          ""meaningVi"": ""Kết nối cảm xúc"",
          ""example"": ""Stories connect with listeners far better than dry bullet points.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Keynote messages achieve lasting impact only when they resonate with the personal stakes of the audience."",
          ""meaningVi"": ""Các thông điệp mở màn chỉ đạt được tác động bền lâu khi chúng cộng hưởng được với những lợi ích thiết thân của người nghe.""
        }
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.1 Cloze (OrderIndex 15)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444450"), "cloze", 15),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444450"),
            OrderIndex = 15,
            Type = "cloze",
            Title = "Điền từ vào chỗ trống (Bài 3.1): Bài tập ngữ cảnh thực tế",
            Description = "Đọc đoạn văn hoặc câu giao tiếp và điền từ thích hợp vào chỗ trống với gợi ý và giải thích chi tiết.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l31-cloze-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Mở đầu bài thuyết trình (Dễ)"",
      ""promptVi"": ""Điền từ để hoàn thiện câu mở đầu bài thuyết trình lịch sự:"",
      ""template"": ""Good morning everyone, today I would like to {{0}} our product roadmap for the upcoming {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""present"",
            ""introduce"",
            ""share""
          ],
          ""hintVi"": ""trình bày / giới thiệu (bắt đầu bằng p hoặc i)"",
          ""explanationVi"": ""\""would like to present\"" là cách mở lời trang trọng và tự nhiên.""
        },
        {
          ""acceptedAnswers"": [
            ""quarter"",
            ""year"",
            ""sprint""
          ],
          ""hintVi"": ""quý / năm tới (bắt đầu bằng q hoặc y)"",
          ""explanationVi"": ""\""upcoming quarter\"" là mốc thời gian kế hoạch phổ biến.""
        }
      ],
      ""fullSentence"": ""Good morning everyone, today I would like to present our product roadmap for the upcoming quarter."",
      ""meaningVi"": ""Chào buổi sáng mọi người, hôm nay tôi muốn trình bày lộ trình sản phẩm của chúng tôi cho quý sắp tới."",
      ""explanationVi"": ""Câu mở đầu chuẩn mực cho mọi bài thuyết trình báo cáo tiến độ.""
    },
    {
      ""id"": ""en-l31-cloze-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Chỉ vào số liệu trên biểu đồ (Dễ)"",
      ""promptVi"": ""Điền từ hướng sự chú ý lên biểu đồ:"",
      ""template"": ""As you can see on this line {{0}}, our active user base experienced significant {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""chart"",
            ""graph""
          ],
          ""hintVi"": ""biểu đồ đường (line ___)"",
          ""explanationVi"": ""\""line chart / line graph\"" là biểu đồ đường.""
        },
        {
          ""acceptedAnswers"": [
            ""growth"",
            ""increase"",
            ""surge""
          ],
          ""hintVi"": ""sự tăng trưởng (bắt đầu bằng g)"",
          ""explanationVi"": ""\""experienced significant growth\"" nghĩa là trải qua sự tăng trưởng đáng kể.""
        }
      ],
      ""fullSentence"": ""As you can see on this line chart, our active user base experienced significant growth."",
      ""meaningVi"": ""Như các bạn có thể thấy trên biểu đồ đường này, lượng người dùng hoạt động của chúng tôi đã trải qua sự tăng trưởng đáng kể."",
      ""explanationVi"": ""Mẫu câu liên kết giữa lời nói và hình ảnh slide trực quan.""
    },
    {
      ""id"": ""en-l31-cloze-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Từ nối dẫn dắt chuyển ý (Vừa)"",
      ""promptVi"": ""Điền cụm từ nối chuyển tiếp giữa các phần thuyết trình:"",
      ""template"": ""Now that we have covered user retention, let us {{0}} on to our next key metric: system {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""move"",
            ""turn""
          ],
          ""hintVi"": ""chuyển sang (___ on to)"",
          ""explanationVi"": ""\""move on to\"" là cụm từ signposting kinh điển.""
        },
        {
          ""acceptedAnswers"": [
            ""availability"",
            ""performance"",
            ""reliability"",
            ""scalability""
          ],
          ""hintVi"": ""tính sẵn sàng / hiệu năng (bắt đầu bằng a hoặc p)"",
          ""explanationVi"": ""\""system availability\"" là chỉ số đo lường độ sẵn sàng hoạt động của hệ thống.""
        }
      ],
      ""fullSentence"": ""Now that we have covered user retention, let us move on to our next key metric: system availability."",
      ""meaningVi"": ""Sau khi đã đi qua phần giữ chân người dùng, chúng ta hãy chuyển sang chỉ số then chốt tiếp theo: tính sẵn sàng của hệ thống."",
      ""explanationVi"": ""Kỹ thuật signposting giúp khán giả nắm bắt bố cục bài nói không bị lạc hướng.""
    },
    {
      ""id"": ""en-l31-cloze-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Nhấn mạnh thông điệp cốt lõi (Vừa)"",
      ""promptVi"": ""Điền từ khóa thông điệp đọng lại:"",
      ""template"": ""The single most critical {{0}} from this study is that continuous learning yields compounding {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""takeaway"",
            ""lesson"",
            ""message""
          ],
          ""hintVi"": ""bài học cốt lõi cần nhớ (bắt đầu bằng t)"",
          ""explanationVi"": ""\""critical takeaway\"" là thông điệp trọng tâm đọng lại.""
        },
        {
          ""acceptedAnswers"": [
            ""returns"",
            ""dividends"",
            ""benefits""
          ],
          ""hintVi"": ""lợi suất / lợi ích tích lũy (bắt đầu bằng r)"",
          ""explanationVi"": ""\""compounding returns\"" nghĩa là lợi suất sinh lời tích lũy kép.""
        }
      ],
      ""fullSentence"": ""The single most critical takeaway from this study is that continuous learning yields compounding returns."",
      ""meaningVi"": ""Bài học quan trọng duy nhất rút ra từ nghiên cứu này là sự học tập liên tục mang lại lợi suất tích lũy kép."",
      ""explanationVi"": ""Câu chốt hạ thông điệp mang tính đòn bẩy trong các bài pitch.""
    },
    {
      ""id"": ""en-l31-cloze-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Lời kết bài và kêu gọi hành động (Khó)"",
      ""promptVi"": ""Điền từ về kết bài và lời hiệu triệu hành động:"",
      ""template"": ""To wrap up today's pitch, every successful transformation requires an uncompromising {{0}} to action and relentless {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""call""
          ],
          ""hintVi"": ""lời kêu gọi (___ to action)"",
          ""explanationVi"": ""\""call to action (CTA)\"" là lời kêu gọi hành động quyết định.""
        },
        {
          ""acceptedAnswers"": [
            ""discipline"",
            ""execution"",
            ""focus""
          ],
          ""hintVi"": ""kỷ luật / thực thi (bắt đầu bằng d hoặc e)"",
          ""explanationVi"": ""\""relentless discipline / execution\"" nghĩa là sự kỷ luật / thực thi bền bỉ.""
        }
      ],
      ""fullSentence"": ""To wrap up today's pitch, every successful transformation requires an uncompromising call to action and relentless discipline."",
      ""meaningVi"": ""Để khép lại bài thuyết trình hôm nay, mọi sự chuyển hóa thành công đều đòi hỏi một lời kêu gọi hành động dứt khoát và sự kỷ luật bền bỉ."",
      ""explanationVi"": ""Đoạn kết mạnh mẽ để lại dấu ấn sâu sắc cho các nhà lãnh đạo và nhà đầu tư.""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.1 Translate (OrderIndex 16)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444450"), "translate", 16),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444450"),
            OrderIndex = 16,
            Type = "translate",
            Title = "Dịch phản xạ Việt → Anh (Bài 3.1): Diễn đạt tự nhiên",
            Description = "Chuyển câu tiếng Việt sang tiếng Anh tự nhiên với nhiều đáp án được chấp nhận và lưu ý ngữ pháp hữu ích.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l31-trans-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Giới thiệu mục tiêu bài nói (Dễ)"",
      ""sourceVi"": ""Hôm nay tôi muốn chứng minh việc học tập liên tục giúp tăng tốc sự phát triển nghề nghiệp như thế nào."",
      ""hintKeywords"": [
        ""demonstrate"",
        ""continuous learning"",
        ""accelerates career growth""
      ],
      ""acceptedAnswers"": [
        ""Today I would like to demonstrate how continuous learning accelerates career growth."",
        ""Today I want to demonstrate how continuous learning speeds up professional development."",
        ""Today I would like to show how continuous learning accelerates our career growth.""
      ],
      ""sampleAnswer"": ""Today I would like to demonstrate how continuous learning accelerates career growth."",
      ""grammarNoteVi"": ""Dùng cấu trúc mệnh đề danh từ: \""demonstrate how + S + V\"".""
    },
    {
      ""id"": ""en-l31-trans-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Mời đặt câu hỏi (Dễ)"",
      ""sourceVi"": ""Cảm ơn sự chú ý của các bạn, và bây giờ tôi rất vui lòng được trả lời bất kỳ câu hỏi nào."",
      ""hintKeywords"": [
        ""thank you for your attention"",
        ""delighted to answer any questions""
      ],
      ""acceptedAnswers"": [
        ""Thank you for your attention, and I would now be delighted to answer any questions."",
        ""Thank you for your time, and I am happy to take any questions now."",
        ""Thank you for listening, and I would be pleased to answer your questions.""
      ],
      ""sampleAnswer"": ""Thank you for your attention, and I would now be delighted to answer any questions."",
      ""grammarNoteVi"": ""Mẫu câu kết bài thuyết trình chuẩn mực quốc tế: \""delighted to take/answer any questions\"".""
    },
    {
      ""id"": ""en-l31-trans-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Mô tả biểu đồ doanh thu tăng vọt (Vừa)"",
      ""sourceVi"": ""Như được minh họa trên biểu đồ cột, doanh thu của chúng tôi đã tăng vọt 45% trong quý vừa qua."",
      ""hintKeywords"": [
        ""as illustrated on the bar chart"",
        ""revenue surged by 45%"",
        ""past quarter""
      ],
      ""acceptedAnswers"": [
        ""As illustrated on the bar chart, our revenue surged by 45 percent over the past quarter."",
        ""As shown in the bar chart, our revenue increased by 45% during the last quarter."",
        ""As illustrated by this bar chart, our revenue rose by 45 percent in the past quarter.""
      ],
      ""sampleAnswer"": ""As illustrated on the bar chart, our revenue surged by 45 percent over the past quarter."",
      ""grammarNoteVi"": ""Động từ \""surged by + X%\"" biểu thị mức tăng mạnh mẽ và đột phá.""
    },
    {
      ""id"": ""en-l31-trans-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Chuyển ý bằng từ nối (Vừa)"",
      ""sourceVi"": ""Sau khi đã xem xét cơ sở lý thuyết, bây giờ chúng ta hãy cùng phân tích các ứng dụng thực tế."",
      ""hintKeywords"": [
        ""now that we have covered the theoretical foundation"",
        ""examine practical implications""
      ],
      ""acceptedAnswers"": [
        ""Now that we have covered the theoretical foundation, let us examine the practical implications."",
        ""Having reviewed the theoretical foundation, let's look at the practical applications."",
        ""Now that we covered the theoretical foundation, let us analyze practical implications.""
      ],
      ""sampleAnswer"": ""Now that we have covered the theoretical foundation, let us examine the practical implications."",
      ""grammarNoteVi"": ""\""Now that + S + V\"" là liên từ chỉ lý do / thời điểm rất học thuật.""
    },
    {
      ""id"": ""en-l31-trans-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Dịch: Nghệ thuật kết hợp dữ liệu và câu chuyện (Khó)"",
      ""sourceVi"": ""Việc kết hợp chặt chẽ giữa bằng chứng phân tích nghiêm ngặt và lối kể chuyện cô đọng tạo nên đỉnh cao của nghệ thuật thuyết trình điều hành."",
      ""hintKeywords"": [
        ""synthesis of rigorous analytical proof"",
        ""concise storytelling"",
        ""pinnacle of executive communication""
      ],
      ""acceptedAnswers"": [
        ""The synthesis of rigorous analytical proof and concise storytelling constitutes the pinnacle of executive communication."",
        ""Seamlessly combining rigorous analytical proof with concise storytelling represents the pinnacle of executive communication.""
      ],
      ""sampleAnswer"": ""The synthesis of rigorous analytical proof and concise storytelling constitutes the pinnacle of executive communication."",
      ""grammarNoteVi"": ""Động từ \""constitutes\"" (cấu thành, tạo nên) nâng tầm tính học thuật của câu.""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.1 Grammar Fix (OrderIndex 17)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444450"), "grammar_fix", 17),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444450"),
            OrderIndex = 17,
            Type = "grammar_fix",
            Title = "Sửa lỗi ngữ pháp (Bài 3.1): Khắc phục các lỗi người Việt hay mắc",
            Description = "Phát hiện và sửa các lỗi sai ngữ pháp, thì động từ, mạo từ và giới từ thường gặp trong giao tiếp thực tế.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l31-gf-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Phân biệt rise và raise (Dễ)"",
      ""incorrectSentence"": ""Our customer retention raised by twenty percent over the past two quarters."",
      ""errorTypeHintVi"": ""Tự tăng lên dùng nội động từ \""rose\"" (quá khứ của rise), không dùng ngoại động từ \""raised\""."",
      ""correctedSentence"": ""Our customer retention rose by twenty percent over the past two quarters."",
      ""acceptedAnswers"": [
        ""Our customer retention rose by twenty percent over the past two quarters."",
        ""Our customer retention increased by twenty percent over the past two quarters.""
      ],
      ""meaningVi"": ""Tỷ lệ giữ chân khách hàng của chúng tôi đã tăng 20% trong hai quý vừa qua."",
      ""explanationVi"": ""\""Rise (rose - risen)\"" là nội động từ (không cần tân ngữ); \""raise\"" là ngoại động từ bắt buộc có tân ngữ (ví dụ: *we raised our prices*).""
    },
    {
      ""id"": ""en-l31-gf-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Cụm từ kết bài in conclusion (Dễ)"",
      ""incorrectSentence"": ""In the conclusion, I would like to thank everyone for listening."",
      ""errorTypeHintVi"": ""Cụm từ liên kết chuyển ý kết bài là \""In conclusion\"", không có mạo từ \""the\""."",
      ""correctedSentence"": ""In conclusion, I would like to thank everyone for listening."",
      ""acceptedAnswers"": [
        ""In conclusion, I would like to thank everyone for listening."",
        ""To conclude, I would like to thank everyone for listening.""
      ],
      ""meaningVi"": ""Tóm lại, tôi xin cảm ơn mọi người đã chú ý lắng nghe."",
      ""explanationVi"": ""Thành ngữ chuẩn xác: \""In conclusion\"" (Tóm lại).""
    },
    {
      ""id"": ""en-l31-gf-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Dùng sai mạo từ trước số liệu phân trăm (Vừa)"",
      ""incorrectSentence"": ""Our quarterly revenue increased by the forty percent this sprint."",
      ""errorTypeHintVi"": ""Không dùng mạo từ xác định \""the\"" trước số phần trăm cụ thể (forty percent)."",
      ""correctedSentence"": ""Our quarterly revenue increased by forty percent this sprint."",
      ""acceptedAnswers"": [
        ""Our quarterly revenue increased by forty percent this sprint."",
        ""Our quarterly revenue increased by 40% this sprint.""
      ],
      ""meaningVi"": ""Doanh thu hàng quý của chúng tôi đã tăng 40% trong sprint này."",
      ""explanationVi"": ""Trước cụm phần trăm \""forty percent\"" không bao giờ đi kèm mạo từ \""the\"".""
    },
    {
      ""id"": ""en-l31-gf-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Giới từ chỉ đồ thị on vs in (Vừa)"",
      ""incorrectSentence"": ""As illustrated on the pie chart, sixty percent users prefer mobile."",
      ""errorTypeHintVi"": ""Thiếu giới từ \""of\"" sau \""sixty percent\"" và dùng \""in the pie chart\"" hoặc \""by the pie chart\""."",
      ""correctedSentence"": ""As illustrated in the pie chart, sixty percent of users prefer mobile."",
      ""acceptedAnswers"": [
        ""As illustrated in the pie chart, sixty percent of users prefer mobile."",
        ""As shown in the pie chart, sixty percent of users prefer mobile."",
        ""As illustrated by the pie chart, sixty percent of users prefer mobile.""
      ],
      ""meaningVi"": ""Như được minh họa trong biểu đồ tròn, 60% người dùng ưa chuộng thiết bị di động."",
      ""explanationVi"": ""Phải có \""of\"" sau phần trăm khi bổ nghĩa cho danh từ xác định: \""sixty percent of users\"".""
    },
    {
      ""id"": ""en-l31-gf-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Sửa lỗi: Cấu trúc câu chẻ nhấn mạnh (Khó)"",
      ""incorrectSentence"": ""What this chart demonstrates are the importance of continuous testing."",
      ""errorTypeHintVi"": ""Mệnh đề danh từ làm chủ ngữ (\""What this chart demonstrates\"") được xem là ngôi thứ 3 số ít, động từ to be phải chia là \""is\""."",
      ""correctedSentence"": ""What this chart demonstrates is the importance of continuous testing."",
      ""acceptedAnswers"": [
        ""What this chart demonstrates is the importance of continuous testing."",
        ""What this chart illustrates is the importance of continuous testing.""
      ],
      ""meaningVi"": ""Điều mà biểu đồ này chứng minh chính là tầm quan trọng của việc kiểm thử liên tục."",
      ""explanationVi"": ""Chủ ngữ là mệnh đề \""What this chart demonstrates\"" đi với động từ số ít \""is\"".""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.1 Roleplay (OrderIndex 18)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444450"), "roleplay", 18),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444450"),
            OrderIndex = 18,
            Type = "roleplay",
            Title = "Nhập vai hội thoại (Bài 3.1): Tình huống phản xạ thực chiến",
            Description = "Nhập vai giao tiếp với AI trong các tình huống thực tế, hỗ trợ chấm điểm tự động theo từ khóa và cấu trúc câu.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l31-rp-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Mở đầu bài thuyết trình trước hội đồng (Dễ)"",
      ""scenarioVi"": ""Bạn bước lên bục thuyết trình để trình bày kế hoạch sản phẩm quý mới trước ban lãnh đạo."",
      ""aiRole"": ""Chủ tịch hội đồng (Panel Chair)"",
      ""aiPrompt"": ""The floor is yours. Please introduce your topic and walk us through your core objectives."",
      ""aiPromptVi"": ""Xin mời bạn phát biểu. Hãy giới thiệu chủ đề và hướng dẫn chúng tôi qua các mục tiêu cốt lõi."",
      ""userGoalVi"": ""Chào hội đồng, giới thiệu tên và chủ đề bài thuyết trình về chiến lược tối ưu hóa sản phẩm 5 phút."",
      ""expectedKeywords"": [
        ""good morning"",
        ""pleasure"",
        ""present"",
        ""product strategy"",
        ""objectives""
      ],
      ""sampleResponses"": [
        ""Good morning, esteemed panel members! It is a true pleasure to present our quarterly product optimization roadmap today.""
      ],
      ""minWords"": 10
    },
    {
      ""id"": ""en-l31-rp-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Trình bày số liệu biểu đồ slide (Dễ)"",
      ""scenarioVi"": ""Bạn chuyển sang slide số 3 và muốn hướng sự chú ý của mọi người vào biểu đồ tăng trưởng doanh thu."",
      ""aiRole"": ""Chủ tịch hội đồng (Panel Chair)"",
      ""aiPrompt"": ""These revenue charts look interesting. What do the blue and green bars represent?"",
      ""aiPromptVi"": ""Các biểu đồ doanh thu này trông rất thú vị. Các cột xanh dương và xanh lá đại diện cho điều gì?"",
      ""userGoalVi"": ""Giải thích cột xanh dương là doanh thu quý trước, cột xanh lá là quý này với mức tăng trưởng 35%."",
      ""expectedKeywords"": [
        ""blue bar"",
        ""green bar"",
        ""represent"",
        ""growth"",
        ""revenue"",
        ""percent""
      ],
      ""sampleResponses"": [
        ""The blue bars represent our baseline revenue from last quarter, while the green bars illustrate this quarter's remarkable 35 percent growth.""
      ],
      ""minWords"": 12
    },
    {
      ""id"": ""en-l31-rp-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Chuyển ý dẫn dắt người nghe sang giải pháp (Vừa)"",
      ""scenarioVi"": ""Bạn vừa trình bày xong các thách thức về độ trễ hệ thống và muốn chuyển ý mượt mà sang giải pháp công nghệ mới."",
      ""aiRole"": ""Giám đốc Kỹ thuật (Technical Director)"",
      ""aiPrompt"": ""We clearly see the current bottlenecks. How does your proposed solution resolve this?"",
      ""aiPromptVi"": ""Chúng tôi thấy rõ các điểm nghẽn hiện tại. Giải pháp đề xuất của bạn giải quyết điều này như thế nào?"",
      ""userGoalVi"": ""Dùng từ nối dẫn dắt (moving on / turning attention), trình bày giải pháp kiến trúc phân tán giúp giảm độ trễ."",
      ""expectedKeywords"": [
        ""moving on"",
        ""proposed solution"",
        ""reduce latency"",
        ""architecture"",
        ""solve""
      ],
      ""sampleResponses"": [
        ""Moving on to our architectural solution: by adopting lightweight asynchronous queues, we directly reduce database latency by over fifty percent.""
      ],
      ""minWords"": 14
    },
    {
      ""id"": ""en-l31-rp-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Ứng phó với câu hỏi chất vấn về chi phí (Vừa)"",
      ""scenarioVi"": ""Một thành viên hội đồng chất vấn liệu chi phí tự động hóa có quá cao so với ngân sách hiện tại không."",
      ""aiRole"": ""Giám đốc Tài chính (CFO)"",
      ""aiPrompt"": ""Your proposal looks technologically sound, but the upfront tooling investment seems rather steep. How do you justify the ROI?"",
      ""aiPromptVi"": ""Đề xuất của bạn về mặt công nghệ thì ổn, nhưng khoản đầu tư công cụ ban đầu có vẻ khá đắt đỏ. Bạn giải thích thế nào về tỷ suất hoàn vốn?"",
      ""userGoalVi"": ""Ghi nhận lo ngại về chi phí, giải thích bằng số liệu rằng tự động hóa sẽ tiết kiệm hàng trăm giờ kỹ sư mỗi tháng."",
      ""expectedKeywords"": [
        ""valid concern"",
        ""investment"",
        ""ROI"",
        ""save hours"",
        ""compounding returns"",
        ""efficiency""
      ],
      ""sampleResponses"": [
        ""That is an entirely valid concern. While the upfront investment is notable, automating regression testing saves over two hundred engineering hours monthly, yielding a complete ROI within five months.""
      ],
      ""minWords"": 16
    },
    {
      ""id"": ""en-l31-rp-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Kết bài và kêu gọi phê duyệt ngân sách (Khó)"",
      ""scenarioVi"": ""Bạn còn 1 phút cuối để đúc kết thông điệp cốt lõi và kêu gọi ban giám đốc phê duyệt ngân sách triển khai dự án."",
      ""aiRole"": ""Chủ tịch hội đồng (Panel Chair)"",
      ""aiPrompt"": ""You have one minute left to wrap up. What is your final call to action for the board?"",
      ""aiPromptVi"": ""Bạn còn một phút để kết thúc. Lời kêu gọi hành động cuối cùng của bạn gửi tới hội đồng là gì?"",
      ""userGoalVi"": ""Đúc kết bài học cốt lõi, kêu gọi hội đồng phê duyệt ngân sách thí điểm giai đoạn 1 để dẫn đầu thị trường."",
      ""expectedKeywords"": [
        ""in conclusion"",
        ""key takeaway"",
        ""call to action"",
        ""approve budget"",
        ""pilot"",
        ""leadership""
      ],
      ""sampleResponses"": [
        ""In conclusion, our key takeaway is that disciplined automation transforms potential into market leadership. I strongly urge the board to approve the Phase 1 pilot budget today so our engineering teams can build the future of our platform.""
      ],
      ""minWords"": 18
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.2 Shadowing (OrderIndex 11)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444451"), "shadowing", 11),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444451"),
            OrderIndex = 11,
            Type = "shadowing",
            Title = "Shadowing mở rộng (Bài 3.2): 15 câu đa cấp độ Dễ → Vừa → Khó",
            Description = "Luyện nói đuổi theo ngữ điệu người bản xứ, tập trung vào nối âm, ngữ điệu và phát âm chuẩn các âm then chốt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l32-shad-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Bày tỏ thiện chí đàm phán (Dễ)"",
      ""sentence"": ""We hope to reach a fair agreement that benefits both sides."",
      ""ipa"": ""/wiː həʊp tuː riːtʃ ə feər əˈɡriːmənt ðæt ˈbenɪfɪts bəʊθ saɪdz/"",
      ""meaningVi"": ""Chúng tôi hy vọng đạt được một thỏa thuận công bằng mang lại lợi ích cho cả hai bên."",
      ""keyPhrases"": [
        ""reach a fair agreement"",
        ""benefits both sides""
      ]
    },
    {
      ""id"": ""en-l32-shad-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Hỏi thăm kỳ vọng đối tác (Dễ)"",
      ""sentence"": ""What are your main expectations for this contract?"",
      ""ipa"": ""/wɒt ɑː jɔː meɪn ˌekspekˈteɪʃnz fɔː ðɪs ˈkɒntrækt/"",
      ""meaningVi"": ""Những kỳ vọng chính của bạn đối với hợp đồng này là gì?"",
      ""keyPhrases"": [
        ""main expectations"",
        ""for this contract""
      ]
    },
    {
      ""id"": ""en-l32-shad-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Đề xuất mức giá linh hoạt (Dễ)"",
      ""sentence"": ""We are open to adjusting our price if you increase the order volume."",
      ""ipa"": ""/wiː ɑːr ˈəʊpən tuː əˈdʒʌstɪŋ ˈaʊə praɪs ɪf juː ɪnˈkriːs ði ˈɔːdə ˈvɒljuːm/"",
      ""meaningVi"": ""Chúng tôi sẵn sàng điều chỉnh giá nếu bạn tăng khối lượng đặt hàng."",
      ""keyPhrases"": [
        ""open to adjusting price"",
        ""increase order volume""
      ]
    },
    {
      ""id"": ""en-l32-shad-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Đồng thuận điều khoản (Dễ)"",
      ""sentence"": ""That sounds like a fair compromise for both of us."",
      ""ipa"": ""/ðæt saʊndz laɪk ə feə ˈkɒmprəmaɪz fɔː bəʊθ əv ʌs/"",
      ""meaningVi"": ""Điều đó nghe có vẻ là một giải pháp thỏa hiệp công bằng cho cả hai chúng ta."",
      ""keyPhrases"": [
        ""fair compromise"",
        ""for both of us""
      ]
    },
    {
      ""id"": ""en-l32-shad-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Chốt ký kết hợp đồng (Dễ)"",
      ""sentence"": ""Let us prepare the formal contract for signature next week."",
      ""ipa"": ""/let ʌs prɪˈpeə ðə ˈfɔːml ˈkɒntrækt fɔː ˈsɪɡnətʃə nekst wiːk/"",
      ""meaningVi"": ""Chúng ta hãy chuẩn bị hợp đồng chính thức để ký kết vào tuần tới."",
      ""keyPhrases"": [
        ""formal contract"",
        ""for signature"",
        ""next week""
      ]
    },
    {
      ""id"": ""en-l32-shad-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đàm phán nguyên tắc đôi bên cùng có lợi (Vừa)"",
      ""sentence"": ""Our primary goal is to forge a mutually beneficial partnership that safeguards long-term commercial interests."",
      ""ipa"": ""/ˈaʊə ˈpraɪməri ɡəʊl ɪz tuː fɔːdʒ ə ˈmjuːtʃuəli ˌbenɪˈfɪʃl ˈpɑːtnəʃɪps ðæt ˈseɪfɡɑːdz ˈlɒŋ tɜːm kəˈmɜːʃl ˈɪntrəsts/"",
      ""meaningVi"": ""Mục tiêu hàng đầu của chúng tôi là tạo dựng một quan hệ đối tác đôi bên cùng có lợi nhằm bảo vệ lợi ích thương mại lâu dài."",
      ""keyPhrases"": [
        ""mutually beneficial partnership"",
        ""safeguards interests"",
        ""commercial interests""
      ]
    },
    {
      ""id"": ""en-l32-shad-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Nhượng bộ chiến lược có điều kiện (Vừa)"",
      ""sentence"": ""We could consider expediting delivery on condition that the technical scope remains strictly defined."",
      ""ipa"": ""/wiː kʊd kənˈsɪdər ˈekspɪdaɪtɪŋ dɪˈlɪvəri ɒn kənˈdɪʃn ðæt ðə ˈteknɪkl skəʊp rɪˈmeɪnz ˈstrɪktli dɪˈfaɪnd/"",
      ""meaningVi"": ""Chúng tôi có thể cân nhắc đẩy nhanh tiến độ giao hàng với điều kiện là phạm vi kỹ thuật vẫn được xác định nghiêm ngặt."",
      ""keyPhrases"": [
        ""expediting delivery"",
        ""on condition that"",
        ""scope remains strictly defined""
      ]
    },
    {
      ""id"": ""en-l32-shad-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đặt câu hỏi làm rõ khúc mắc (Vừa)"",
      ""sentence"": ""Could you elaborate on the specific financial constraints preventing your team from adopting this licensing tier?"",
      ""ipa"": ""/kəd juː ɪˈlæbəreɪt ɒn ðə spəˈsɪfɪk faɪˈnænʃl kənˈstreɪnts prɪˈventɪŋ jɔː tiːm frəm əˈdɒptɪŋ ðɪs ˈlaɪsnsɪŋ tɪə/"",
      ""meaningVi"": ""Bạn có thể chia sẻ cụ thể hơn về những rào cản tài chính đang khiến nhóm của bạn chưa thể áp dụng gói bản quyền này không?"",
      ""keyPhrases"": [
        ""elaborate on constraints"",
        ""financial constraints"",
        ""licensing tier""
      ]
    },
    {
      ""id"": ""en-l32-shad-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Khẳng định điều khoản bất khả thương lượng (Vừa)"",
      ""sentence"": ""Data privacy and regulatory compliance represent strictly non-negotiable clauses in all our enterprise agreements."",
      ""ipa"": ""/ˈdeɪtə ˈprɪvəsi ænd ˌreɡjʊˈleɪtəri kəmˈplaɪəns ˌreprɪˈzent ˈstrɪktli nɒn nɪˈɡəʊʃiəbl ˈklɔːzɪz ɪn ɔːl ˈaʊər ˈentəpraɪz əˈɡriːmənts/"",
      ""meaningVi"": ""Quyền riêng tư dữ liệu và tuân thủ quy định pháp lý là các điều khoản tuyệt đối không thể nhân nhượng trong mọi thỏa thuận của chúng tôi."",
      ""keyPhrases"": [
        ""data privacy"",
        ""regulatory compliance"",
        ""non-negotiable clauses""
      ]
    },
    {
      ""id"": ""en-l32-shad-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Phá vỡ thế bế tắc trong đàm phán (Vừa)"",
      ""sentence"": ""Proposing a phased implementation roadmap successfully broke the negotiation impasse between our organizations."",
      ""ipa"": ""/prəˈpəʊzɪŋ ə feɪzd ˌɪmplɪmenˈteɪʃn ˈrəʊdmæp səkˈsesfəli brəʊk ðə nɪˌɡəʊʃiˈeɪʃn æmˈpɑːs bɪˈtwiːn ˈaʊə ˌɔːɡənaɪˈzeɪʃnz/"",
      ""meaningVi"": ""Đề xuất lộ trình triển khai theo giai đoạn đã phá vỡ thành công thế bế tắc đàm phán giữa hai tổ chức của chúng ta."",
      ""keyPhrases"": [
        ""phased implementation roadmap"",
        ""broke the impasse"",
        ""negotiation impasse""
      ]
    },
    {
      ""id"": ""en-l32-shad-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Đàm phán đa phương cấp cao (Khó)"",
      ""sentence"": ""Mastering principled negotiation enables commercial leaders to defuse adversarial tension and unlock synergistic enterprise value."",
      ""ipa"": ""/ˈmɑːstərɪŋ ˈprɪnsəpld nɪˌɡəʊʃiˈeɪʃn ɪnˈeɪblz kəˈmɜːʃl ˈliːdəz tuː diːˈfjuːz ˌædvəˈseəriəl ˈtenʃn ænd ʌnˈlɒk ˌsɪnəˈdʒɪstɪk ˈentəpraɪz ˈvæljuː/"",
      ""meaningVi"": ""Làm chủ đàm phán nguyên tắc cho phép các nhà lãnh đạo thương mại hóa giải căng thẳng đối đầu và giải phóng giá trị doanh nghiệp mang tính cộng hưởng."",
      ""keyPhrases"": [
        ""principled negotiation"",
        ""defuse adversarial tension"",
        ""synergistic enterprise value""
      ]
    },
    {
      ""id"": ""en-l32-shad-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cơ cấu điều khoản rủi ro hợp đồng (Khó)"",
      ""sentence"": ""Structuring bilateral indemnity clauses equitably distributes commercial liability while incentivizing exemplary performance."",
      ""ipa"": ""/ˈstrʌktʃərɪŋ baɪˈlætərəl ɪnˈdemnəti ˈklɔːzɪz ˈekwɪtəbli dɪˈstrɪbjuːts kəˈmɜːʃl ˌlaɪəˈbɪləti waɪl ɪnˈsentɪvaɪzɪŋ ɪɡˈzempləri pəˈfɔːməns/"",
      ""meaningVi"": ""Cơ cấu các điều khoản bồi thường song phương một cách công bằng giúp phân bổ trách nhiệm thương mại đồng thời khuyến khích hiệu quả thực thi mẫu mực."",
      ""keyPhrases"": [
        ""bilateral indemnity clauses"",
        ""commercial liability"",
        ""exemplary performance""
      ]
    },
    {
      ""id"": ""en-l32-shad-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Hóa giải phản đối của khách hàng (Khó)"",
      ""sentence"": ""Deftly navigating rigorous counter-proposals requires profound commercial acumen combined with unyielding emotional composure."",
      ""ipa"": ""/ˈdeftli ˈnævɪɡeɪtɪŋ ˈrɪɡərəs ˈkaʊntə prəˈpəʊzlz rɪˈkwaɪəz prəˈfaʊnd kəˈmɜːʃl ˈækjəmən kəmˈbaɪnd wɪð ʌnˈjiːldɪŋ ɪˈməʊʃənl kəmˈpəʊʒə/"",
      ""meaningVi"": ""Lèo lái khéo léo qua các đề xuất phản biện khắt khe đòi hỏi sự sắc bén thương mại sâu sắc kết hợp với sự điềm tĩnh cảm xúc kiên cường."",
      ""keyPhrases"": [
        ""navigating counter-proposals"",
        ""commercial acumen"",
        ""emotional composure""
      ]
    },
    {
      ""id"": ""en-l32-shad-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Thiết lập giải pháp thay thế tốt nhất (BATNA) (Khó)"",
      ""sentence"": ""Cultivating an unassailable best alternative to a negotiated agreement fortifies executive leverage during high-stakes deadlock."",
      ""ipa"": ""/ˈkʌltɪveɪtɪŋ ən ˌʌnəˈseɪləbl best ɔːlˈtɜːnətɪv tuː ə nɪˈɡəʊʃieɪtɪd əˈɡriːmənt ˈfɔːtɪfaɪz ɪɡˈzekjətɪv ˈliːvərɪdʒ ˈdjʊərɪŋ haɪ steɪks ˈdedlɒk/"",
      ""meaningVi"": ""Tạo dựng một phương án thay thế vững chắc nhất cho thỏa thuận đàm phán củng cố vị thế thương lượng điều hành trong những thời khắc bế tắc gay cấn."",
      ""keyPhrases"": [
        ""best alternative (BATNA)"",
        ""executive leverage"",
        ""high-stakes deadlock""
      ]
    },
    {
      ""id"": ""en-l32-shad-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Thỏa thuận hòa giải bền vững (Khó)"",
      ""sentence"": ""Durable contractual covenants transcend zero-sum bargaining by aligning institutional incentives with overarching strategic missions."",
      ""ipa"": ""/ˈdjʊərəbl kənˈtræktʃuəl ˈkʌvənənts trænˈsend ˌzɪərəʊ ˈsʌm ˈbɑːɡənɪŋ baɪ əˈlaɪnɪŋ ˌɪnstɪˈtjuːʃənl ɪnˈsentɪvz wɪð ˌəʊvərˈɑːtʃɪŋ strəˈtiːdʒɪk ˈmɪʃnz/"",
      ""meaningVi"": ""Các giao ước hợp đồng bền vững vượt lên trên lối mặc cả được-mất triệt tiêu bằng cách gắn kết các động lực tổ chức với những sứ mệnh chiến lược bao trùm."",
      ""keyPhrases"": [
        ""contractual covenants"",
        ""zero-sum bargaining"",
        ""overarching strategic missions""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.2 Dictation (OrderIndex 12)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444451"), "dictation", 12),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444451"),
            OrderIndex = 12,
            Type = "dictation",
            Title = "Dictation mở rộng (Bài 3.2): 15 câu nghe chép chính tả phản xạ",
            Description = "Nghe câu hội thoại đa cấp độ và gõ lại chính xác từng từ hoặc chọn word chip để rèn luyện thính giác tiếng Anh.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l32-dict-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Bàn về ngân sách đàm phán (Dễ)"",
      ""sentence"": ""Is this fee within your allocated project budget?"",
      ""ipa"": ""/ɪz ðɪs fiː wɪðˈɪn jɔːr ˈæləkeɪtɪd ˈprɒdʒekt ˈbʌdʒɪt/"",
      ""meaningVi"": ""Khoản chi phí này có nằm trong ngân sách dự án đã được cấp của bạn không?"",
      ""wordChips"": [
        ""Is"",
        ""this"",
        ""fee"",
        ""within"",
        ""your"",
        ""allocated"",
        ""project"",
        ""budget""
      ],
      ""distractors"": [
        ""cost"",
        ""inside"",
        ""planned"",
        ""total""
      ]
    },
    {
      ""id"": ""en-l32-dict-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Đề xuất hợp tác đôi bên cùng có lợi (Dễ)"",
      ""sentence"": ""We believe this proposal creates a win-win partnership."",
      ""ipa"": ""/wiː bɪˈliːv ðɪs prəˈpəʊzl kriˈeɪts ə wɪn wɪn ˈpɑːtnəʃɪps/"",
      ""meaningVi"": ""Chúng tôi tin rằng đề xuất này tạo ra một mối quan hệ hợp tác đôi bên cùng có lợi."",
      ""wordChips"": [
        ""We"",
        ""believe"",
        ""this"",
        ""proposal"",
        ""creates"",
        ""a"",
        ""win-win"",
        ""partnership""
      ],
      ""distractors"": [
        ""think"",
        ""offer"",
        ""makes"",
        ""deal""
      ]
    },
    {
      ""id"": ""en-l32-dict-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Đề nghị điều chỉnh tiến độ (Dễ)"",
      ""sentence"": ""Can we adjust the final delivery deadline by two weeks?"",
      ""ipa"": ""/kæn wiː əˈdʒʌst ðə ˈfaɪnl dɪˈlɪvəri ˈdedlaɪn baɪ tuː wiːks/"",
      ""meaningVi"": ""Chúng ta có thể điều chỉnh thời hạn bàn giao cuối cùng thêm 2 tuần không?"",
      ""wordChips"": [
        ""Can"",
        ""we"",
        ""adjust"",
        ""the"",
        ""final"",
        ""delivery"",
        ""deadline"",
        ""by"",
        ""two"",
        ""weeks""
      ],
      ""distractors"": [
        ""could"",
        ""change"",
        ""target"",
        ""months""
      ]
    },
    {
      ""id"": ""en-l32-dict-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Xác nhận điều khoản thỏa thuận (Dễ)"",
      ""sentence"": ""Both parties agreed to sign the memorandum of understanding."",
      ""ipa"": ""/bəʊθ ˈpɑːtiz əˈɡriːd tuː saɪn ðə ˌmeməˈrændəm əv ˌʌndəˈstændɪŋ/"",
      ""meaningVi"": ""Cả hai bên đã đồng ý ký bản ghi nhớ thỏa thuận."",
      ""wordChips"": [
        ""Both"",
        ""parties"",
        ""agreed"",
        ""to"",
        ""sign"",
        ""the"",
        ""memorandum"",
        ""of"",
        ""understanding""
      ],
      ""distractors"": [
        ""sides"",
        ""decided"",
        ""contract"",
        ""paper""
      ]
    },
    {
      ""id"": ""en-l32-dict-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Cam kết bảo mật thông tin (Dễ)"",
      ""sentence"": ""We respect your strict confidentiality and privacy requirements."",
      ""ipa"": ""/wiː rɪˈspekt jɔː strɪkt ˌkɒnfɪˌdenʃiˈæləti ænd ˈprɪvəsi rɪˈkwaɪəmənts/"",
      ""meaningVi"": ""Chúng tôi tôn trọng các yêu cầu bảo mật và quyền riêng tư nghiêm ngặt của bạn."",
      ""wordChips"": [
        ""We"",
        ""respect"",
        ""your"",
        ""strict"",
        ""confidentiality"",
        ""and"",
        ""privacy"",
        ""requirements""
      ],
      ""distractors"": [
        ""honor"",
        ""clear"",
        ""security"",
        ""rules""
      ]
    },
    {
      ""id"": ""en-l32-dict-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Nhượng bộ có điều kiện hợp đồng (Vừa)"",
      ""sentence"": ""We could offer a ten percent discount provided that payments are settled within thirty days."",
      ""ipa"": ""/wiː kʊd ˈɒfər ə ten pəˈsent ˈdɪskaʊnt prəˈvaɪdɪd ðæt ˈpeɪmənts ɑː ˈsetld wɪðˈɪn ˈθɜːti deɪz/"",
      ""meaningVi"": ""Chúng tôi có thể chiết khấu 10% với điều kiện là các khoản thanh toán được quyết toán trong vòng 30 ngày."",
      ""wordChips"": [
        ""We"",
        ""could"",
        ""offer"",
        ""a"",
        ""ten"",
        ""percent"",
        ""discount"",
        ""provided"",
        ""that"",
        ""payments"",
        ""are"",
        ""settled"",
        ""within"",
        ""thirty"",
        ""days""
      ],
      ""distractors"": [
        ""can"",
        ""give"",
        ""price"",
        ""made""
      ]
    },
    {
      ""id"": ""en-l32-dict-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Hỏi thăm lý do từ chối (Vừa)"",
      ""sentence"": ""Could you help us understand the main constraints preventing your approval?"",
      ""ipa"": ""/kəd juː help ʌs ˌʌndəˈstænd ðə meɪn kənˈstreɪnts prɪˈventɪŋ jɔːr əˈpruːvl/"",
      ""meaningVi"": ""Bạn có thể giúp chúng tôi hiểu những rào cản chính đang cản trở sự phê duyệt của bạn không?"",
      ""wordChips"": [
        ""Could"",
        ""you"",
        ""help"",
        ""us"",
        ""understand"",
        ""the"",
        ""main"",
        ""constraints"",
        ""preventing"",
        ""your"",
        ""approval""
      ],
      ""distractors"": [
        ""can"",
        ""explain"",
        ""reasons"",
        ""stopping""
      ]
    },
    {
      ""id"": ""en-l32-dict-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Điều khoản ràng buộc pháp lý (Vừa)"",
      ""sentence"": ""Subject to formal legal review, we are comfortable signing this agreement."",
      ""ipa"": ""/ˈsʌbdʒɪkt tuː ˈfɔːml ˈliːɡl rɪˈvjuː, wiː ɑː ˈkʌmftəbl ˈsaɪnɪŋ ðɪs əˈɡriːmənt/"",
      ""meaningVi"": ""Tùy thuộc vào sự rà soát pháp lý chính thức, chúng tôi sẵn sàng ký thỏa thuận này."",
      ""wordChips"": [
        ""Subject"",
        ""to"",
        ""formal"",
        ""legal"",
        ""review"",
        ""we"",
        ""are"",
        ""comfortable"",
        ""signing"",
        ""this"",
        ""agreement""
      ],
      ""distractors"": [
        ""depending"",
        ""on"",
        ""law"",
        ""contract""
      ]
    },
    {
      ""id"": ""en-l32-dict-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đàm phán phân kỳ giai đoạn (Vừa)"",
      ""sentence"": ""A phased rollout provides the ideal compromise between speed and security compliance."",
      ""ipa"": ""/ə feɪzd ˈrəʊlaʊt prəˈvaɪdz ði aɪˈdiːəl ˈkɒmprəmaɪz bɪˈtwiːn spiːd ænd sɪˈkjʊərəti kəmˈplaɪəns/"",
      ""meaningVi"": ""Việc triển khai theo giai đoạn mang lại giải pháp thỏa hiệp lý tưởng giữa tốc độ và sự tuân thủ an ninh."",
      ""wordChips"": [
        ""A"",
        ""phased"",
        ""rollout"",
        ""provides"",
        ""the"",
        ""ideal"",
        ""compromise"",
        ""between"",
        ""speed"",
        ""and"",
        ""security"",
        ""compliance""
      ],
      ""distractors"": [
        ""staged"",
        ""plan"",
        ""balance"",
        ""safety""
      ]
    },
    {
      ""id"": ""en-l32-dict-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Bảo vệ vị thế đàm phán (Vừa)"",
      ""sentence"": ""Our intellectual property ownership remains completely non-negotiable in this collaboration."",
      ""ipa"": ""/ˈaʊər ˌɪntəˈlektʃuəl ˈprɒpəti ˈəʊnəʃɪp rɪˈmeɪnz kəmˈpliːtli nɒn nɪˈɡəʊʃiəbl ɪn ðɪs kəˌlæbəˈreɪʃn/"",
      ""meaningVi"": ""Quyền sở hữu trí tuệ của chúng tôi hoàn toàn là điều khoản không thể thương lượng trong sự hợp tác này."",
      ""wordChips"": [
        ""Our"",
        ""intellectual"",
        ""property"",
        ""ownership"",
        ""remains"",
        ""completely"",
        ""non-negotiable"",
        ""in"",
        ""this"",
        ""collaboration""
      ],
      ""distractors"": [
        ""patent"",
        ""rights"",
        ""totally"",
        ""fixed""
      ]
    },
    {
      ""id"": ""en-l32-dict-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Tháo gỡ thế bế tắc pháp lý (Khó)"",
      ""sentence"": ""Deploying neutral third-party arbitration mechanisms effectively resolved the protracted contractual stalemate."",
      ""ipa"": ""/dɪˈplɔɪɪŋ ˈnjuːtrəl ˈθɜːd ˈpɑːti ˌɑːbɪˈtreɪʃn ˈmekənɪzəmz ɪˈfektɪvli rɪˈzɒlvd ðə prəˈtræktɪd kənˈtræktʃuəl ˈsteɪlmeɪt/"",
      ""meaningVi"": ""Việc áp dụng các cơ chế trọng tài bên thứ ba trung lập đã giải quyết hiệu quả tình trạng bế tắc hợp đồng kéo dài."",
      ""wordChips"": [
        ""Deploying"",
        ""neutral"",
        ""third-party"",
        ""arbitration"",
        ""mechanisms"",
        ""effectively"",
        ""resolved"",
        ""the"",
        ""protracted"",
        ""contractual"",
        ""stalemate""
      ],
      ""distractors"": [
        ""using"",
        ""independent"",
        ""settlement"",
        ""dispute""
      ]
    },
    {
      ""id"": ""en-l32-dict-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cơ cấu phân bổ trách nhiệm rủi ro (Khó)"",
      ""sentence"": ""Equitable indemnification stipulations safeguard enterprise participants from disproportionate commercial liabilities."",
      ""ipa"": ""/ˈekwɪtəbl ɪnˌdemnɪfɪˈkeɪʃn ˌstɪpjʊˈleɪʃnz ˈseɪfɡɑːd ˈentəpraɪz pɑːˈtɪsɪpənts frɒm ˌdɪsprəˈpɔːʃənət kəˈmɜːʃl ˌlaɪəˈbɪlətiz/"",
      ""meaningVi"": ""Các quy định bồi thường công bằng bảo vệ các bên tham gia doanh nghiệp khỏi các trách nhiệm thương mại không tương xứng."",
      ""wordChips"": [
        ""Equitable"",
        ""indemnification"",
        ""stipulations"",
        ""safeguard"",
        ""enterprise"",
        ""participants"",
        ""from"",
        ""disproportionate"",
        ""commercial"",
        ""liabilities""
      ],
      ""distractors"": [
        ""fair"",
        ""compensation"",
        ""clauses"",
        ""protect""
      ]
    },
    {
      ""id"": ""en-l32-dict-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Tận dụng đòn bẩy thương lượng (Khó)"",
      ""sentence"": ""Demonstrating differentiated technological superiority substantially enhances executive bargaining leverage."",
      ""ipa"": ""/ˈdemənstreɪtɪŋ ˌdɪfəˈrenʃieɪtɪd ˌteknəˈlɒdʒɪkl suːˌpɪəriˈɒrəti səbˈstænʃli ɪnˈhɑːnsɪz ɪɡˈzekjətɪv ˈbɑːɡənɪŋ ˈliːvərɪdʒ/"",
      ""meaningVi"": ""Chứng minh sự vượt trội về công nghệ khác biệt gia tăng đáng kể đòn bẩy thương lượng của ban điều hành."",
      ""wordChips"": [
        ""Demonstrating"",
        ""differentiated"",
        ""technological"",
        ""superiority"",
        ""substantially"",
        ""enhances"",
        ""executive"",
        ""bargaining"",
        ""leverage""
      ],
      ""distractors"": [
        ""showing"",
        ""unique"",
        ""advantage"",
        ""boosts""
      ]
    },
    {
      ""id"": ""en-l32-dict-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Hòa giải bất đồng lợi ích cốt lõi (Khó)"",
      ""sentence"": ""Uncoupling entrenched positions from underlying organizational interests unlocks unforeseen creative compromises."",
      ""ipa"": ""/ʌnˈkʌplɪŋ ɪnˈtrentʃt pəˈzɪʃnz frɒm ˌʌndəˈlaɪɪŋ ˌɔːɡənaɪˈzeɪʃənl ˈɪntrəsts ʌnˈlɒks ˌʌnfɔːˈsiːn kriˈeɪtɪv ˈkɒmprəmaɪzɪz/"",
      ""meaningVi"": ""Tách biệt các vị thế cố thủ khỏi các lợi ích tổ chức cơ bản mở ra những thỏa hiệp sáng tạo chưa từng thấy."",
      ""wordChips"": [
        ""Uncoupling"",
        ""entrenched"",
        ""positions"",
        ""from"",
        ""underlying"",
        ""organizational"",
        ""interests"",
        ""unlocks"",
        ""unforeseen"",
        ""creative"",
        ""compromises""
      ],
      ""distractors"": [
        ""separating"",
        ""fixed"",
        ""needs"",
        ""solutions""
      ]
    },
    {
      ""id"": ""en-l32-dict-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Xây dựng hiệp ước hợp tác bền vững (Khó)"",
      ""sentence"": ""Sustainable commercial alliances transcend transactional bargaining by establishing robust reciprocal value generation."",
      ""ipa"": ""/səˈsteɪnəbl kəˈmɜːʃl əˈlaɪənsɪz trænˈsend trænˈzækʃənl ˈbɑːɡənɪŋ baɪ ɪˈstæblɪʃɪŋ rəʊˈbʌst rɪˈsɪprəkl ˈvæljuː ˌdʒenəˈreɪʃn/"",
      ""meaningVi"": ""Các liên minh thương mại bền vững vượt lên trên việc mặc cả giao dịch bằng cách thiết lập sự tạo ra giá trị hai chiều vững chắc."",
      ""wordChips"": [
        ""Sustainable"",
        ""commercial"",
        ""alliances"",
        ""transcend"",
        ""transactional"",
        ""bargaining"",
        ""by"",
        ""establishing"",
        ""robust"",
        ""reciprocal"",
        ""value"",
        ""generation""
      ],
      ""distractors"": [
        ""enduring"",
        ""business"",
        ""deals"",
        ""creating""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.2 Pronunciation (OrderIndex 13)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444451"), "pronunciation", 13),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444451"),
            OrderIndex = 13,
            Type = "pronunciation",
            Title = "Chấm phát âm mở rộng (Bài 3.2): 15 câu luyện âm /θ/-/ð/, âm cuối và đuôi -s/-ed",
            Description = "Luyện phát âm với AI chấm điểm trực tiếp, phát hiện lỗi sai âm đuôi và các âm khó phổ biến của người Việt.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l32-pron-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /ʃ/ trong negotiate & share (Dễ)"",
      ""sentence"": ""We negotiate terms and share mutual benefits."",
      ""ipa"": ""/wiː nɪˈɡəʊʃieɪt tɜːmz ænd ʃeə ˈmjuːtʃuəl ˈbenɪfɪts/"",
      ""meaningVi"": ""Chúng tôi đàm phán điều khoản và chia sẻ lợi ích chung."",
      ""targetWords"": [
        ""negotiate"",
        ""share"",
        ""benefits""
      ],
      ""keyPhrases"": [
        ""negotiate terms"",
        ""mutual benefits""
      ]
    },
    {
      ""id"": ""en-l32-pron-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /dʒ/ trong adjust & project (Dễ)"",
      ""sentence"": ""Let us adjust our project budget schedule."",
      ""ipa"": ""/let ʌs əˈdʒʌst ˈaʊə ˈprɒdʒekt ˈbʌdʒɪt ˈʃedjuːl/"",
      ""meaningVi"": ""Chúng ta hãy điều chỉnh lịch trình ngân sách dự án."",
      ""targetWords"": [
        ""adjust"",
        ""project"",
        ""budget""
      ],
      ""keyPhrases"": [
        ""adjust schedule"",
        ""project budget""
      ]
    },
    {
      ""id"": ""en-l32-pron-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /z/ trong compromise & fees (Dễ)"",
      ""sentence"": ""This compromise balances the legal fees."",
      ""ipa"": ""/ðɪs ˈkɒmprəmaɪz ˈbælənsɪz ðə ˈliːɡl fiːz/"",
      ""meaningVi"": ""Sự thỏa hiệp này cân bằng các khoản phí pháp lý."",
      ""targetWords"": [
        ""compromise"",
        ""balances"",
        ""fees""
      ],
      ""keyPhrases"": [
        ""compromise balances"",
        ""legal fees""
      ]
    },
    {
      ""id"": ""en-l32-pron-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm /θ/ trong both & thirty (Dễ)"",
      ""sentence"": ""Both parties agreed within thirty days."",
      ""ipa"": ""/bəʊθ ˈpɑːtiz əˈɡriːd wɪðˈɪn ˈθɜːti deɪz/"",
      ""meaningVi"": ""Cả hai bên đã đồng ý trong vòng ba mươi ngày."",
      ""targetWords"": [
        ""both"",
        ""within"",
        ""thirty""
      ],
      ""keyPhrases"": [
        ""both parties"",
        ""within thirty days""
      ]
    },
    {
      ""id"": ""en-l32-pron-e5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""topic"": ""Âm cuối /kt/ trong contract (Dễ)"",
      ""sentence"": ""Please inspect the contract before we sign."",
      ""ipa"": ""/pliːz ɪnˈspekt ðə ˈkɒntrækt bɪˈfɔː wiː saɪn/"",
      ""meaningVi"": ""Làm ơn kiểm tra hợp đồng trước khi chúng ta ký kết."",
      ""targetWords"": [
        ""inspect"",
        ""contract"",
        ""sign""
      ],
      ""keyPhrases"": [
        ""inspect contract"",
        ""before we sign""
      ]
    },
    {
      ""id"": ""en-l32-pron-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /ʃ/ trong conditional concession (Vừa)"",
      ""sentence"": ""Offering a conditional concession helped resolve the negotiation impasse."",
      ""ipa"": ""/ˈɒfərɪŋ ə kənˈdɪʃənl kənˈseʃn helpt rɪˈzɒlv ðə nɪˌɡəʊʃiˈeɪʃn æmˈpɑːs/"",
      ""meaningVi"": ""Đưa ra nhượng bộ có điều kiện đã giúp giải quyết thế bế tắc đàm phán."",
      ""targetWords"": [
        ""conditional"",
        ""concession"",
        ""negotiation"",
        ""impasse""
      ],
      ""keyPhrases"": [
        ""conditional concession"",
        ""negotiation impasse""
      ]
    },
    {
      ""id"": ""en-l32-pron-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Âm /dʒ/ trong safeguards & legitimacy (Vừa)"",
      ""sentence"": ""This agreement safeguards our legitimacy and commercial interests."",
      ""ipa"": ""/ðɪs əˈɡriːmənt ˈseɪfɡɑːdz ˈaʊə lɪˈdʒɪtɪməsi ænd kəˈmɜːʃl ˈɪntrəsts/"",
      ""meaningVi"": ""Thỏa thuận này bảo vệ tính chính danh và các lợi ích thương mại của chúng ta."",
      ""targetWords"": [
        ""safeguards"",
        ""legitimacy"",
        ""interests""
      ],
      ""keyPhrases"": [
        ""safeguards legitimacy"",
        ""commercial interests""
      ]
    },
    {
      ""id"": ""en-l32-pron-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Cụm phụ âm /ndʒ/ trong mutually beneficial (Vừa)"",
      ""sentence"": ""We finalized a mutually beneficial agreement during bilateral talks."",
      ""ipa"": ""/wiː ˈfaɪnəlaɪzd ə ˈmjuːtʃuəli ˌbenɪˈfɪʃl əˈɡriːmənt ˈdjʊərɪŋ baɪˈlætərəl tɔːks/"",
      ""meaningVi"": ""Chúng tôi đã hoàn thiện thỏa thuận đôi bên cùng có lợi trong các cuộc đàm phán song phương."",
      ""targetWords"": [
        ""finalized"",
        ""mutually"",
        ""beneficial"",
        ""bilateral""
      ],
      ""keyPhrases"": [
        ""mutually beneficial"",
        ""bilateral talks""
      ]
    },
    {
      ""id"": ""en-l32-pron-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Đuôi -ed dạng /t/ trong reached & broke (Vừa)"",
      ""sentence"": ""Both companies reached consensus and broke the deadlock."",
      ""ipa"": ""/bəʊθ ˈkʌmpəniz riːtʃt kənˈsensəs ænd brəʊk ðə ˈdedlɒk/"",
      ""meaningVi"": ""Cả hai công ty đã đạt được sự đồng thuận và phá vỡ bế tắc."",
      ""targetWords"": [
        ""reached"",
        ""consensus"",
        ""deadlock""
      ],
      ""keyPhrases"": [
        ""reached consensus"",
        ""broke deadlock""
      ]
    },
    {
      ""id"": ""en-l32-pron-m5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""topic"": ""Cụm phụ âm /kl/ trong non-negotiable clause (Vừa)"",
      ""sentence"": ""Data privacy is a strictly non-negotiable clause for our leadership."",
      ""ipa"": ""/ˈdeɪtə ˈprɪvəsi ɪz ə ˈstrɪktli nɒn nɪˈɡəʊʃiəbl klɔːz fɔːr ˈaʊə ˈliːdəʃɪp/"",
      ""meaningVi"": ""Bảo mật dữ liệu là điều khoản tuyệt đối không thể đàm phán đối với ban lãnh đạo của chúng tôi."",
      ""targetWords"": [
        ""non-negotiable"",
        ""clause"",
        ""leadership""
      ],
      ""keyPhrases"": [
        ""non-negotiable clause"",
        ""data privacy""
      ]
    },
    {
      ""id"": ""en-l32-pron-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Trọng âm từ phức & âm /ʃ/ trong indemnification (Khó)"",
      ""sentence"": ""Equitable indemnification covenants insulate institutions from catastrophic litigation exposure."",
      ""ipa"": ""/ˈekwɪtəbl ɪnˌdemnɪfɪˈkeɪʃn ˈkʌvənənts ˈɪnsjʊleɪt ˌɪnstɪˈtjuːʃnz frɒm ˌkætəˈstrɒfɪk ˌlɪtɪˈɡeɪʃn ɪkˈspəʊʒə/"",
      ""meaningVi"": ""Các giao ước bồi thường công bằng cách ly các thể chế khỏi nguy cơ kiện tụng thảm khốc."",
      ""targetWords"": [
        ""indemnification"",
        ""covenants"",
        ""insulate"",
        ""litigation"",
        ""exposure""
      ],
      ""keyPhrases"": [
        ""indemnification covenants"",
        ""litigation exposure""
      ]
    },
    {
      ""id"": ""en-l32-pron-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Âm /v/ vs /w/ & /θ/ trong unassailable worth (Khó)"",
      ""sentence"": ""Cultivating an unassailable BATNA strengthens executive resolve during contentious deliberations."",
      ""ipa"": ""/ˈkʌltɪveɪtɪŋ ən ˌʌnəˈseɪləbl ˈbætnə ˈstreŋθnz ɪɡˈzekjətɪv rɪˈzɒlv ˈdjʊərɪŋ kənˈtenʃəs dɪˌlɪbəˈreɪʃnz/"",
      ""meaningVi"": ""Tạo lập phương án thay thế BATNA không thể công kích củng cố quyết tâm của ban điều hành trong các cuộc thảo luận tranh chấp."",
      ""targetWords"": [
        ""unassailable"",
        ""strengthens"",
        ""resolve"",
        ""contentious"",
        ""deliberations""
      ],
      ""keyPhrases"": [
        ""unassailable BATNA"",
        ""contentious deliberations""
      ]
    },
    {
      ""id"": ""en-l32-pron-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Cụm phụ âm /kr/ & /pl/ trong uncoupling entrenched (Khó)"",
      ""sentence"": ""Uncoupling entrenched positions from core interests unlocks unexpected reciprocal compromises."",
      ""ipa"": ""/ʌnˈkʌplɪŋ ɪnˈtrentʃt pəˈzɪʃnz frɒm kɔːr ˈɪntrəsts ʌnˈlɒks ˌʌnɪkˈspektɪd rɪˈsɪprəkl ˈkɒmprəmaɪzɪz/"",
      ""meaningVi"": ""Tách rời các vị thế cố thủ khỏi các lợi ích cốt lõi mở ra những thỏa hiệp hai chiều bất ngờ."",
      ""targetWords"": [
        ""uncoupling"",
        ""entrenched"",
        ""interests"",
        ""reciprocal"",
        ""compromises""
      ],
      ""keyPhrases"": [
        ""entrenched positions"",
        ""reciprocal compromises""
      ]
    },
    {
      ""id"": ""en-l32-pron-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Âm /dʒ/ trong judicious arbitration (Khó)"",
      ""sentence"": ""Judicious third-party arbitration defuses acute contractual antagonism without jeopardizing partnerships."",
      ""ipa"": ""/dʒuːˈdɪʃəs ˈθɜːd ˈpɑːti ˌɑːbɪˈtreɪʃn diːˈfjuːzɪz əˈkjuːt kənˈtræktʃuəl ænˈtæɡənɪzəm wɪðˈaʊt ˈdʒepədaɪzɪŋ ˈpɑːtnəʃɪps/"",
      ""meaningVi"": ""Trọng tài bên thứ ba sáng suốt hóa giải sự thù địch hợp đồng gay gắt mà không gây nguy hại cho mối quan hệ đối tác."",
      ""targetWords"": [
        ""judicious"",
        ""arbitration"",
        ""defuses"",
        ""antagonism"",
        ""jeopardizing""
      ],
      ""keyPhrases"": [
        ""judicious arbitration"",
        ""contractual antagonism""
      ]
    },
    {
      ""id"": ""en-l32-pron-h5"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""topic"": ""Nhịp điệu câu điều kiện & âm /tr/ trong intractable stalemate (Khó)"",
      ""sentence"": ""Principled commercial diplomacy transforms intractable stalemates into enduring institutional alliances."",
      ""ipa"": ""/ˈprɪnsəpld kəˈmɜːʃl dɪˈpləʊməsi trænsˈfɔːmz ɪnˈtræktəbl ˈsteɪlmeɪts ˈɪntuː ɪnˈdjʊərɪŋ ˌɪnstɪˈtjuːʃənl əˈlaɪənsɪz/"",
      ""meaningVi"": ""Ngoại giao thương mại có nguyên tắc biến những thế bế tắc nan giải thành các liên minh thể chế bền lâu."",
      ""targetWords"": [
        ""principled"",
        ""diplomacy"",
        ""intractable"",
        ""stalemates"",
        ""alliances""
      ],
      ""keyPhrases"": [
        ""commercial diplomacy"",
        ""intractable stalemates"",
        ""institutional alliances""
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.2 Word Map (OrderIndex 14)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444451"), "word_map", 14),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444451"),
            OrderIndex = 14,
            Type = "word_map",
            Title = "Sơ đồ từ vựng Mind Map mở rộng (Bài 3.2): 12 sơ đồ Dễ → Vừa → Khó",
            Description = "Hệ thống sơ đồ tư duy từ vựng đa cấp độ kèm phiên âm IPA, collocations, phrasal verbs, synonyms và ví dụ ngữ cảnh thực chiến.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""wm-l32-e1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: agree"",
      ""centralWord"": ""agree"",
      ""ipa"": ""/əˈɡriː/"",
      ""meaningVi"": ""Đồng ý, tán thành, nhất trí"",
      ""collocations"": [
        {
          ""text"": ""agree on terms"",
          ""ipa"": ""/əˈɡriː ɒn tɜːmz/"",
          ""meaningVi"": ""Đồng ý về điều khoản"",
          ""example"": ""Both sides agreed on terms within one hour.""
        },
        {
          ""text"": ""mutually agree"",
          ""ipa"": ""/ˈmjuːtʃuəli əˈɡriː/"",
          ""meaningVi"": ""Cùng nhau đồng thuận"",
          ""example"": ""We mutually agree to extend the project warranty.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""settle on"",
          ""ipa"": ""/ˈsetl ɒn/"",
          ""meaningVi"": ""Chốt lại ở"",
          ""example"": ""After debate, they settled on a fair price.""
        },
        {
          ""text"": ""go along with"",
          ""ipa"": ""/ɡəʊ əˈlɒŋ wɪð/"",
          ""meaningVi"": ""Tán thành theo"",
          ""example"": ""The client went along with our proposed milestone schedule.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""concur"",
          ""ipa"": ""/kənˈkɜː/"",
          ""meaningVi"": ""Đồng tình"",
          ""example"": ""Legal counsel concurred with the assessment.""
        },
        {
          ""text"": ""accept"",
          ""ipa"": ""/əkˈsept/"",
          ""meaningVi"": ""Chấp thuận"",
          ""example"": ""The vendor accepted our revised payment terms.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Agreeing on foundational principles early prevents disputes during final contract signing."",
          ""meaningVi"": ""Thống nhất các nguyên tắc nền tảng từ sớm giúp tránh tranh chấp trong lúc ký hợp đồng cuối cùng.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-e2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: deal"",
      ""centralWord"": ""deal"",
      ""ipa"": ""/diːl/"",
      ""meaningVi"": ""Thương vụ, giao kèo, thỏa thuận"",
      ""collocations"": [
        {
          ""text"": ""close a deal"",
          ""ipa"": ""/kləʊz ə diːl/"",
          ""meaningVi"": ""Chốt một thương vụ"",
          ""example"": ""The sales director closed a multi-million-dollar deal.""
        },
        {
          ""text"": ""fair deal"",
          ""ipa"": ""/feə diːl/"",
          ""meaningVi"": ""Thương vụ sòng phẳng"",
          ""example"": ""Both partners felt they secured a truly fair deal.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""deal with"",
          ""ipa"": ""/diːl wɪð/"",
          ""meaningVi"": ""Xử lý, đối phó với"",
          ""example"": ""Diplomacy helps you deal with tough counterarguments.""
        },
        {
          ""text"": ""strike a deal"",
          ""ipa"": ""/straɪk ə diːl/"",
          ""meaningVi"": ""Ký kết giao kèo"",
          ""example"": ""The founders struck a deal with strategic angel investors.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""bargain"",
          ""ipa"": ""/ˈbɑːɡən/"",
          ""meaningVi"": ""Món hời, sự mặc cả"",
          ""example"": ""Quality software tools are a great bargain.""
        },
        {
          ""text"": ""transaction"",
          ""ipa"": ""/trænˈzækʃn/"",
          ""meaningVi"": ""Giao dịch"",
          ""example"": ""The transaction was executed securely.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""A successful business deal leaves both parties satisfied and eager to collaborate again."",
          ""meaningVi"": ""Một thương vụ kinh doanh thành công để lại cho cả hai bên sự hài lòng và mong muốn tiếp tục hợp tác.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-e3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: price"",
      ""centralWord"": ""price"",
      ""ipa"": ""/praɪs/"",
      ""meaningVi"": ""Giá cả, mức giá"",
      ""collocations"": [
        {
          ""text"": ""competitive price"",
          ""ipa"": ""/kəmˈpetətɪv praɪs/"",
          ""meaningVi"": ""Giá cả cạnh tranh"",
          ""example"": ""Our cloud subscription offers a highly competitive price.""
        },
        {
          ""text"": ""fixed price"",
          ""ipa"": ""/fɪkst praɪs/"",
          ""meaningVi"": ""Giá cố định"",
          ""example"": ""The client prefers a fixed price contract for predictability.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""bring down"",
          ""ipa"": ""/brɪŋ daʊn/"",
          ""meaningVi"": ""Hạ giá xuống"",
          ""example"": ""Negotiating bulk volume helped bring down unit costs.""
        },
        {
          ""text"": ""quote for"",
          ""ipa"": ""/kwəʊt fɔː/"",
          ""meaningVi"": ""Báo giá cho"",
          ""example"": ""The vendor quoted for three years of maintenance.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""cost"",
          ""ipa"": ""/kɒst/"",
          ""meaningVi"": ""Chi phí"",
          ""example"": ""Lower infrastructure cost improves profit margins.""
        },
        {
          ""text"": ""fee"",
          ""ipa"": ""/fiː/"",
          ""meaningVi"": ""Khoản phí"",
          ""example"": ""Consulting fees are billed on milestone completion.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Defending your price requires anchoring the conversation on value rather than raw hours."",
          ""meaningVi"": ""Bảo vệ mức giá của bạn đòi hỏi phải neo cuộc trò chuyện vào giá trị thay vì chỉ là số giờ làm việc thô.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-e4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""easy"",
      ""title"": ""Sơ đồ từ vựng: term"",
      ""centralWord"": ""term"",
      ""ipa"": ""/tɜːm/"",
      ""meaningVi"": ""Điều khoản, thời hạn hợp đồng"",
      ""collocations"": [
        {
          ""text"": ""payment terms"",
          ""ipa"": ""/ˈpeɪmənt tɜːmz/"",
          ""meaningVi"": ""Điều khoản thanh toán"",
          ""example"": ""Standard payment terms are net thirty days.""
        },
        {
          ""text"": ""terms and conditions"",
          ""ipa"": ""/tɜːmz ænd kənˈdɪʃnz/"",
          ""meaningVi"": ""Các điều khoản và điều kiện"",
          ""example"": ""Read the enterprise terms and conditions carefully.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""agree on"",
          ""ipa"": ""/əˈɡriː ɒn/"",
          ""meaningVi"": ""Đồng thuận về"",
          ""example"": ""Let's agree on licensing terms before deployment.""
        },
        {
          ""text"": ""spell out"",
          ""ipa"": ""/spel aʊt/"",
          ""meaningVi"": ""Nêu rõ ràng"",
          ""example"": ""The contract spells out support SLA expectations.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""clause"",
          ""ipa"": ""/klɔːz/"",
          ""meaningVi"": ""Điều khoản pháp lý"",
          ""example"": ""The indemnity clause protects intellectual property.""
        },
        {
          ""text"": ""provision"",
          ""ipa"": ""/prəˈvɪʒn/"",
          ""meaningVi"": ""Quy định"",
          ""example"": ""Statutory provisions must be strictly observed.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Clarity in contractual terms prevents costly misunderstandings down the road."",
          ""meaningVi"": ""Sự rõ ràng trong các điều khoản hợp đồng giúp ngăn ngừa những hiểu lầm tốn kém trong tương lai.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-m1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: negotiate"",
      ""centralWord"": ""negotiate"",
      ""ipa"": ""/nɪˈɡəʊʃieɪt/"",
      ""meaningVi"": ""Đàm phán, thương lượng, dàn xếp"",
      ""collocations"": [
        {
          ""text"": ""negotiate in good faith"",
          ""ipa"": ""/nɪˈɡəʊʃieɪt ɪn ɡʊd feɪθ/"",
          ""meaningVi"": ""Đàm phán thiện chí"",
          ""example"": ""Both commercial parties committed to negotiate in good faith.""
        },
        {
          ""text"": ""successful negotiation"",
          ""ipa"": ""/səkˈsesfl ˌneɡəʊʃiˈeɪʃn/"",
          ""meaningVi"": ""Cuộc đàm phán thành công"",
          ""example"": ""A successful negotiation creates lasting enterprise value.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""hash out"",
          ""ipa"": ""/hæʃ aʊt/"",
          ""meaningVi"": ""Bàn thảo kỹ lưỡng"",
          ""example"": ""The lawyers hashed out the intellectual property clauses.""
        },
        {
          ""text"": ""iron out"",
          ""ipa"": ""/ˈaɪən aʊt/"",
          ""meaningVi"": ""Tháo gỡ khúc mắc"",
          ""example"": ""We ironed out remaining operational disagreements.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""bargain"",
          ""ipa"": ""/ˈbɑːɡən/"",
          ""meaningVi"": ""Thương thảo"",
          ""example"": ""They bargained hard for exclusive distribution rights.""
        },
        {
          ""text"": ""arbitrate"",
          ""ipa"": ""/ˈɑːbɪtreɪt/"",
          ""meaningVi"": ""Hòa giải"",
          ""example"": ""A neutral expert arbitrated the contract dispute.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Principled negotiation transforms adversarial tension into collaborative problem-solving."",
          ""meaningVi"": ""Đàm phán theo nguyên tắc biến căng thẳng đối đầu thành việc cùng nhau giải quyết vấn đề.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-m2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: compromise"",
      ""centralWord"": ""compromise"",
      ""ipa"": ""/ˈkɒmprəmaɪz/"",
      ""meaningVi"": ""Sự thỏa hiệp, giải pháp dung hòa"",
      ""collocations"": [
        {
          ""text"": ""reach a compromise"",
          ""ipa"": ""/riːtʃ ə ˈkɒmprəmaɪz/"",
          ""meaningVi"": ""Đạt được sự thỏa hiệp"",
          ""example"": ""After intense deliberations, both squads reached a compromise.""
        },
        {
          ""text"": ""fair compromise"",
          ""ipa"": ""/feə ˈkɒmprəmaɪz/"",
          ""meaningVi"": ""Sự thỏa hiệp công bằng"",
          ""example"": ""A fair compromise protects both parties' key interests.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""meet halfway"",
          ""ipa"": ""/miːt ˌhɑːfˈweɪ/"",
          ""meaningVi"": ""Nhượng bộ đôi bên gặp nhau ở giữa"",
          ""example"": ""We agreed to meet halfway on the maintenance fee.""
        },
        {
          ""text"": ""give and take"",
          ""ipa"": ""/ɡɪv ænd teɪk/"",
          ""meaningVi"": ""Có qua có lại"",
          ""example"": ""Effective diplomacy is built upon mutual give and take.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""concession"",
          ""ipa"": ""/kənˈseʃn/"",
          ""meaningVi"": ""Sự nhượng bộ"",
          ""example"": ""Each partner offered a meaningful concession.""
        },
        {
          ""text"": ""settlement"",
          ""ipa"": ""/ˈsetlmənt/"",
          ""meaningVi"": ""Sự dàn xếp"",
          ""example"": ""The financial settlement ended the commercial lawsuit.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""A viable compromise preserves the core objectives of both sides without leaving lingering resentment."",
          ""meaningVi"": ""Một sự thỏa hiệp khả thi bảo toàn được mục tiêu cốt lõi của hai bên mà không để lại sự ấm ức kéo dài.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-m3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: concession"",
      ""centralWord"": ""concession"",
      ""ipa"": ""/kənˈseʃn/"",
      ""meaningVi"": ""Sự nhượng bộ, điều khoản nhân nhượng"",
      ""collocations"": [
        {
          ""text"": ""conditional concession"",
          ""ipa"": ""/kənˈdɪʃənl kənˈseʃn/"",
          ""meaningVi"": ""Nhượng bộ có điều kiện"",
          ""example"": ""Always tie every price concession to a volume commitment.""
        },
        {
          ""text"": ""major concession"",
          ""ipa"": ""/ˈmeɪdʒə kənˈseʃn/"",
          ""meaningVi"": ""Sự nhượng bộ lớn"",
          ""example"": ""The vendor made a major concession on implementation fees.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""back down on"",
          ""ipa"": ""/bæk daʊn ɒn/"",
          ""meaningVi"": ""Lùi bước về"",
          ""example"": ""Neither side backed down on licensing exclusivity.""
        },
        {
          ""text"": ""yield to"",
          ""ipa"": ""/jiːld tuː/"",
          ""meaningVi"": ""Nhượng bộ trước"",
          ""example"": ""They yielded to our security compliance demands.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""allowance"",
          ""ipa"": ""/əˈlaʊəns/"",
          ""meaningVi"": ""Sự chiếu cố, giảm trừ"",
          ""example"": ""We factored in an allowance for currency fluctuation.""
        },
        {
          ""text"": ""adjustment"",
          ""ipa"": ""/əˈdʒʌstmənt/"",
          ""meaningVi"": ""Sự điều chỉnh"",
          ""example"": ""A modest fee adjustment satisfied both parties.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Never offer an unconditional concession; reciprocal trade-offs maintain your bargaining leverage."",
          ""meaningVi"": ""Đừng bao giờ nhượng bộ vô điều kiện; các trao đổi có đi có lại mới duy trì được vị thế thương lượng của bạn.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-m4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""medium"",
      ""title"": ""Sơ đồ từ vựng: safeguard"",
      ""centralWord"": ""safeguard"",
      ""ipa"": ""/ˈseɪfɡɑːd/"",
      ""meaningVi"": ""Bảo vệ, biện pháp phòng hộ an toàn"",
      ""collocations"": [
        {
          ""text"": ""safeguard interests"",
          ""ipa"": ""/ˈseɪfɡɑːd ˈɪntrəsts/"",
          ""meaningVi"": ""Bảo vệ lợi ích"",
          ""example"": ""The agreement safeguards both parties' confidential data.""
        },
        {
          ""text"": ""procedural safeguard"",
          ""ipa"": ""/prəˈsiːdʒərəl ˈseɪfɡɑːd/"",
          ""meaningVi"": ""Biện pháp phòng ngừa quy trình"",
          ""example"": ""Code reviews act as an essential procedural safeguard.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""guard against"",
          ""ipa"": ""/ɡɑːd əˈɡenst/"",
          ""meaningVi"": ""Phòng ngừa"",
          ""example"": ""Strict SLA penalties guard against service downtime.""
        },
        {
          ""text"": ""watch out for"",
          ""ipa"": ""/wɒtʃ aʊt fɔː/"",
          ""meaningVi"": ""Cảnh giác đề phòng"",
          ""example"": ""Watch out for ambiguous liability phrasing in contracts.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""protect"",
          ""ipa"": ""/prəˈtekt/"",
          ""meaningVi"": ""Bảo vệ"",
          ""example"": ""Encryption protects sensitive banking credentials.""
        },
        {
          ""text"": ""shield"",
          ""ipa"": ""/ʃiːld/"",
          ""meaningVi"": ""Che chắn, cách ly rủi ro"",
          ""example"": ""Limited liability clauses shield founders from debt.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""A well-negotiated contract safeguards long-term collaboration against volatile market disruptions."",
          ""meaningVi"": ""Một bản hợp đồng được đàm phán tốt sẽ bảo vệ sự hợp tác lâu dài trước những biến động thị trường khôn lường.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-h1"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: impasse"",
      ""centralWord"": ""impasse"",
      ""ipa"": ""/æmˈpɑːs/"",
      ""meaningVi"": ""Thế bế tắc, tình trạng không lối thoát"",
      ""collocations"": [
        {
          ""text"": ""reach an impasse"",
          ""ipa"": ""/riːtʃ ən æmˈpɑːs/"",
          ""meaningVi"": ""Rơi vào thế bế tắc"",
          ""example"": ""Negotiations reached an impasse over intellectual property ownership.""
        },
        {
          ""text"": ""break the impasse"",
          ""ipa"": ""/breɪk ðiː æmˈpɑːs/"",
          ""meaningVi"": ""Khai thông bế tắc"",
          ""example"": ""A creative compromise on licensing broke the negotiation impasse.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""bog down"",
          ""ipa"": ""/bɒɡ daʊn/"",
          ""meaningVi"": ""Bị sa lầy"",
          ""example"": ""Talks bogged down in endless legal technicalities.""
        },
        {
          ""text"": ""bridge the gap"",
          ""ipa"": ""/brɪdʒ ðə ɡæp/"",
          ""meaningVi"": ""Thu hẹp khoảng cách"",
          ""example"": ""The mediator bridged the gap between opposing claims.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""stalemate"",
          ""ipa"": ""/ˈsteɪlmeɪt/"",
          ""meaningVi"": ""Tình trạng giằng co bất phân"",
          ""example"": ""Neither side yielded, creating a diplomatic stalemate.""
        },
        {
          ""text"": ""deadlock"",
          ""ipa"": ""/ˈdedlɒk/"",
          ""meaningVi"": ""Điểm nghẽn không thể tiến"",
          ""example"": ""Executive intervention resolved the contract deadlock.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Reframing entrenched demands into shared interests is the definitive formula for breaking any negotiation impasse."",
          ""meaningVi"": ""Định hình lại các yêu sách cố thủ thành những lợi ích chung là công thức quyết định để phá vỡ mọi thế bế tắc đàm phán.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-h2"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: leverage"",
      ""centralWord"": ""leverage"",
      ""ipa"": ""/ˈliːvərɪdʒ/"",
      ""meaningVi"": ""Đòn bẩy thương lượng, ưu thế ép đàm phán"",
      ""collocations"": [
        {
          ""text"": ""bargaining leverage"",
          ""ipa"": ""/ˈbɑːɡənɪŋ ˈliːvərɪdʒ/"",
          ""meaningVi"": ""Đòn bẩy thương lượng"",
          ""example"": ""Proprietary technology gives our firm immense bargaining leverage.""
        },
        {
          ""text"": ""gain leverage"",
          ""ipa"": ""/ɡeɪn ˈliːvərɪdʒ/"",
          ""meaningVi"": ""Giành được lợi thế"",
          ""example"": ""Having multiple alternative vendors allowed us to gain leverage.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""capitalize on"",
          ""ipa"": ""/ˈkæpɪtəlaɪz ɒn/"",
          ""meaningVi"": ""Tận dụng triệt để"",
          ""example"": ""We capitalized on our strong market position during talks.""
        },
        {
          ""text"": ""play off against"",
          ""ipa"": ""/pleɪ ɒf əˈɡenst/"",
          ""meaningVi"": ""Tung hứng giữa các bên"",
          ""example"": ""The procurement lead played competing bids off against each other.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""advantage"",
          ""ipa"": ""/ədˈvɑːntɪdʒ/"",
          ""meaningVi"": ""Lợi thế"",
          ""example"": ""Our low latency gave us a distinct competitive advantage.""
        },
        {
          ""text"": ""influence"",
          ""ipa"": ""/ˈɪnfluəns/"",
          ""meaningVi"": ""Tầm ảnh hưởng"",
          ""example"": ""Strong brand reputation exerts decisive market influence.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Cultivating an unassailable BATNA creates the ultimate bargaining leverage during contentious negotiations."",
          ""meaningVi"": ""Tạo dựng một phương án BATNA không thể công kích mang lại đòn bẩy thương lượng tối thượng trong các cuộc đàm phán tranh chấp.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-h3"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: arbitration"",
      ""centralWord"": ""arbitration"",
      ""ipa"": ""/ˌɑːbɪˈtreɪʃn/"",
      ""meaningVi"": ""Sự phân xử của trọng tài, trọng tài thương mại"",
      ""collocations"": [
        {
          ""text"": ""binding arbitration"",
          ""ipa"": ""/ˈbaɪndɪŋ ˌɑːbɪˈtreɪʃn/"",
          ""meaningVi"": ""Trọng tài có tính ràng buộc"",
          ""example"": ""The contract mandates binding arbitration in Singapore.""
        },
        {
          ""text"": ""arbitration clause"",
          ""ipa"": ""/ˌɑːbɪˈtreɪʃn klɔːz/"",
          ""meaningVi"": ""Điều khoản trọng tài"",
          ""example"": ""An arbitration clause prevents costly public courtroom battles.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""submit to"",
          ""ipa"": ""/səbˈmɪt tuː/"",
          ""meaningVi"": ""Trình lên phân xử"",
          ""example"": ""Both corporations submitted the contractual dispute to arbitration.""
        },
        {
          ""text"": ""rule on"",
          ""ipa"": ""/ruːl ɒn/"",
          ""meaningVi"": ""Phán quyết về"",
          ""example"": ""The international tribunal ruled on the licensing validity.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""mediation"",
          ""ipa"": ""/ˌmiːdiˈeɪʃn/"",
          ""meaningVi"": ""Sự hòa giải"",
          ""example"": ""Mediation resolved the shareholder friction amicably.""
        },
        {
          ""text"": ""adjudication"",
          ""ipa"": ""/əˌdʒuːdɪˈkeɪʃn/"",
          ""meaningVi"": ""Sự xét xử phân định"",
          ""example"": ""Formal adjudication established legal ownership.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""International commercial arbitration clauses provide predictable legal certainty for cross-border investments."",
          ""meaningVi"": ""Các điều khoản trọng tài thương mại quốc tế mang lại sự chắc chắn về pháp lý có thể lường trước cho các khoản đầu tư xuyên biên giới.""
        }
      ]
    },
    {
      ""id"": ""wm-l32-h4"",
      ""lang"": ""en-US"",
      ""difficulty"": ""hard"",
      ""title"": ""Sơ đồ từ vựng: covenant"",
      ""centralWord"": ""covenant"",
      ""ipa"": ""/ˈkʌvənənt/"",
      ""meaningVi"": ""Giao ước, cam kết pháp lý ràng buộc"",
      ""collocations"": [
        {
          ""text"": ""restrictive covenant"",
          ""ipa"": ""/rɪˈstrɪktɪv ˈkʌvənənt/"",
          ""meaningVi"": ""Giao ước hạn chế quyền"",
          ""example"": ""Non-compete clauses serve as restrictive covenants in employment contracts.""
        },
        {
          ""text"": ""breach of covenant"",
          ""ipa"": ""/briːtʃ əv ˈkʌvənənt/"",
          ""meaningVi"": ""Vi phạm giao ước hợp đồng"",
          ""example"": ""Unapproved data sharing constitutes a severe breach of covenant.""
        }
      ],
      ""phrasalVerbs"": [
        {
          ""text"": ""abide by"",
          ""ipa"": ""/əˈbaɪd baɪ/"",
          ""meaningVi"": ""Tuân thủ nghiêm ngặt"",
          ""example"": ""Both signatories must abide by the agreed covenants.""
        },
        {
          ""text"": ""bind to"",
          ""ipa"": ""/baɪnd tuː/"",
          ""meaningVi"": ""Ràng buộc pháp lý với"",
          ""example"": ""The signed covenants bind future corporate successors.""
        }
      ],
      ""synonyms"": [
        {
          ""text"": ""stipulation"",
          ""ipa"": ""/ˌstɪpjʊˈleɪʃn/"",
          ""meaningVi"": ""Quy ước điều khoản"",
          ""example"": ""Payment stipulations were clearly documented.""
        },
        {
          ""text"": ""commitment"",
          ""ipa"": ""/kəˈmɪtmənt/"",
          ""meaningVi"": ""Sự cam kết"",
          ""example"": ""Our mutual commitment to quality remains unshakeable.""
        }
      ],
      ""contextExamples"": [
        {
          ""sentence"": ""Rigorous contractual covenants establish clear boundaries that protect trade secrets and intellectual capital."",
          ""meaningVi"": ""Các giao ước hợp đồng nghiêm ngặt thiết lập những ranh giới rõ ràng bảo vệ bí mật kinh doanh và vốn trí tuệ.""
        }
      ]
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.2 Cloze (OrderIndex 15)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444451"), "cloze", 15),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444451"),
            OrderIndex = 15,
            Type = "cloze",
            Title = "Điền từ vào chỗ trống (Bài 3.2): Bài tập ngữ cảnh thực tế",
            Description = "Đọc đoạn văn hoặc câu giao tiếp và điền từ thích hợp vào chỗ trống với gợi ý và giải thích chi tiết.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l32-cloze-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Mục tiêu đàm phán đôi bên cùng có lợi (Dễ)"",
      ""promptVi"": ""Điền từ về kết quả đàm phán win-win:"",
      ""template"": ""Our goal is to reach a mutually {{0}} agreement that creates a win-win {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""beneficial"",
            ""advantageous""
          ],
          ""hintVi"": ""có lợi cho cả hai (mutually ___)"",
          ""explanationVi"": ""\""mutually beneficial\"" là cụm từ đàm phán kinh điển.""
        },
        {
          ""acceptedAnswers"": [
            ""partnership"",
            ""outcome"",
            ""solution"",
            ""relationship""
          ],
          ""hintVi"": ""mối quan hệ đối tác / kết quả (bắt đầu bằng p hoặc o)"",
          ""explanationVi"": ""\""win-win partnership\"" nghĩa là sự hợp tác đôi bên cùng thắng.""
        }
      ],
      ""fullSentence"": ""Our goal is to reach a mutually beneficial agreement that creates a win-win partnership."",
      ""meaningVi"": ""Mục tiêu của chúng tôi là đạt được một thỏa thuận đôi bên cùng có lợi tạo nên mối quan hệ đối tác cùng thắng."",
      ""explanationVi"": ""Tuyên ngôn mở đầu đàm phán tạo thiện cảm ngay lập tức với đối tác.""
    },
    {
      ""id"": ""en-l32-cloze-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Đề xuất nhượng bộ có điều kiện (Dễ)"",
      ""promptVi"": ""Điền từ về điều kiện trong thương lượng giá:"",
      ""template"": ""We are open to {{0}} the unit price if your team commits to a larger {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""adjusting"",
            ""lowering"",
            ""reducing""
          ],
          ""hintVi"": ""điều chỉnh / giảm bớt (bắt đầu bằng a hoặc l)"",
          ""explanationVi"": ""\""open to adjusting\"" biểu thị sự linh hoạt nhưng có tính toán.""
        },
        {
          ""acceptedAnswers"": [
            ""volume"",
            ""order"",
            ""quantity""
          ],
          ""hintVi"": ""khối lượng / đơn hàng (bắt đầu bằng v hoặc o)"",
          ""explanationVi"": ""\""larger volume / order\"" nghĩa là số lượng đặt hàng lớn hơn.""
        }
      ],
      ""fullSentence"": ""We are open to adjusting the unit price if your team commits to a larger volume."",
      ""meaningVi"": ""Chúng tôi sẵn sàng điều chỉnh đơn giá nếu nhóm của bạn cam kết khối lượng đặt hàng lớn hơn."",
      ""explanationVi"": ""Nguyên tắc đàm phán: Không bao giờ giảm giá mà không yêu cầu tăng số lượng.""
    },
    {
      ""id"": ""en-l32-cloze-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Nhượng bộ có điều kiện ràng buộc (Vừa)"",
      ""promptVi"": ""Điền cụm từ điều kiện hợp đồng chuyên nghiệp:"",
      ""template"": ""We can deliver the core modules early on {{0}} that the project scope remains clearly {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""condition""
          ],
          ""hintVi"": ""điều kiện (on ___ that)"",
          ""explanationVi"": ""\""on condition that\"" là cụm từ pháp lý chỉ điều kiện tiên quyết.""
        },
        {
          ""acceptedAnswers"": [
            ""defined"",
            ""specified"",
            ""limited""
          ],
          ""hintVi"": ""được xác định rõ ràng (bắt đầu bằng d)"",
          ""explanationVi"": ""\""clearly defined\"" nghĩa là được định nghĩa rõ ràng, không mở rộng tùy tiện.""
        }
      ],
      ""fullSentence"": ""We can deliver the core modules early on condition that the project scope remains clearly defined."",
      ""meaningVi"": ""Chúng tôi có thể bàn giao các module cốt lõi sớm với điều kiện là phạm vi dự án vẫn được xác định rõ ràng."",
      ""explanationVi"": ""Kỹ thuật nhượng bộ có điều kiện giúp bảo vệ đội ngũ khỏi phát sinh việc ngoài phạm vi.""
    },
    {
      ""id"": ""en-l32-cloze-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Hỏi thăm rào cản từ chối (Vừa)"",
      ""promptVi"": ""Điền từ làm rõ nguyên nhân đối phương còn do dự:"",
      ""template"": ""Could you elaborate on the main {{0}} preventing your committee from approving this {{1}}?"",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""constraints"",
            ""concerns"",
            ""obstacles"",
            ""hurdles""
          ],
          ""hintVi"": ""các rào cản / lo ngại (bắt đầu bằng c)"",
          ""explanationVi"": ""\""main constraints / concerns\"" là những rào cản cốt lõi.""
        },
        {
          ""acceptedAnswers"": [
            ""proposal"",
            ""budget"",
            ""contract"",
            ""timeline""
          ],
          ""hintVi"": ""đề xuất / hợp đồng (bắt đầu bằng p)"",
          ""explanationVi"": ""\""approving this proposal\"" nghĩa là phê duyệt đề xuất này.""
        }
      ],
      ""fullSentence"": ""Could you elaborate on the main constraints preventing your committee from approving this proposal?"",
      ""meaningVi"": ""Bạn có thể chia sẻ cụ thể hơn về những rào cản chính đang khiến hội đồng của bạn chưa thể phê duyệt đề xuất này không?"",
      ""explanationVi"": ""Thay vì tranh cãi khi bị từ chối, đặt câu hỏi làm rõ sẽ mở ra không gian thương lượng mới.""
    },
    {
      ""id"": ""en-l32-cloze-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Phá vỡ thế bế tắc đàm phán (Khó)"",
      ""promptVi"": ""Điền thuật ngữ bế tắc và giải pháp phân kỳ:"",
      ""template"": ""Proposing a phased implementation framework successfully broke the negotiation {{0}} and fortified mutual {{1}}."",
      ""blanks"": [
        {
          ""acceptedAnswers"": [
            ""impasse"",
            ""deadlock"",
            ""stalemate""
          ],
          ""hintVi"": ""thế bế tắc (bắt đầu bằng i hoặc d)"",
          ""explanationVi"": ""\""broke the negotiation impasse\"" là phá vỡ bế tắc đàm phán.""
        },
        {
          ""acceptedAnswers"": [
            ""trust"",
            ""alignment"",
            ""respect""
          ],
          ""hintVi"": ""niềm tin / sự đồng thuận (bắt đầu bằng t hoặc a)"",
          ""explanationVi"": ""\""fortified mutual trust\"" củng cố niềm tin giữa hai bên.""
        }
      ],
      ""fullSentence"": ""Proposing a phased implementation framework successfully broke the negotiation impasse and fortified mutual trust."",
      ""meaningVi"": ""Đề xuất khung triển khai theo từng giai đoạn đã phá vỡ thành công thế bế tắc đàm phán và củng cố niềm tin lẫn nhau."",
      ""explanationVi"": ""Chiến thuật gỡ nút thắt kinh điển trong đàm phán hợp đồng công nghệ quy mô lớn.""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.2 Translate (OrderIndex 16)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444451"), "translate", 16),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444451"),
            OrderIndex = 16,
            Type = "translate",
            Title = "Dịch phản xạ Việt → Anh (Bài 3.2): Diễn đạt tự nhiên",
            Description = "Chuyển câu tiếng Việt sang tiếng Anh tự nhiên với nhiều đáp án được chấp nhận và lưu ý ngữ pháp hữu ích.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l32-trans-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Mục tiêu đạt thỏa thuận công bằng (Dễ)"",
      ""sourceVi"": ""Mục tiêu của chúng tôi là đạt được một thỏa thuận đôi bên cùng có lợi nhằm bảo vệ lợi ích lâu dài của cả hai phía."",
      ""hintKeywords"": [
        ""objective"",
        ""mutually beneficial agreement"",
        ""safeguards long-term interests""
      ],
      ""acceptedAnswers"": [
        ""Our objective is to reach a mutually beneficial agreement that safeguards both parties' long-term interests."",
        ""Our goal is to reach a mutually beneficial agreement that protects both sides' long-term interests."",
        ""Our primary objective is to achieve a mutually beneficial agreement safeguarding our long-term interests.""
      ],
      ""sampleAnswer"": ""Our objective is to reach a mutually beneficial agreement that safeguards both parties' long-term interests."",
      ""grammarNoteVi"": ""Cụm danh từ \""both parties' long-term interests\"" dùng sở hữu cách số nhiều ở đuôi dấu phẩy.""
    },
    {
      ""id"": ""en-l32-trans-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Dịch: Điều chỉnh giá theo số lượng (Dễ)"",
      ""sourceVi"": ""Chúng tôi có thể điều chỉnh đơn giá nếu bạn đồng ý ký hợp đồng hỗ trợ 2 năm."",
      ""hintKeywords"": [
        ""adjust unit price"",
        ""agree to sign"",
        ""two-year support contract""
      ],
      ""acceptedAnswers"": [
        ""We can adjust the unit price if you agree to sign a two-year support contract."",
        ""We could adjust our unit price if your team agrees to sign a two-year support agreement."",
        ""We are open to adjusting the unit price provided that you sign a two-year support agreement.""
      ],
      ""sampleAnswer"": ""We can adjust the unit price if you agree to sign a two-year support contract."",
      ""grammarNoteVi"": ""Trong mệnh đề điều kiện loại 1: \""if + S + V-hiện tại đơn\"", không dùng \""will agree\"".""
    },
    {
      ""id"": ""en-l32-trans-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Nhượng bộ tiến độ có điều kiện (Vừa)"",
      ""sourceVi"": ""Chúng tôi có thể đẩy nhanh tiến độ bàn giao với điều kiện là phạm vi công việc vẫn được xác định rõ ràng."",
      ""hintKeywords"": [
        ""expedite delivery"",
        ""on condition that"",
        ""scope remains clearly defined""
      ],
      ""acceptedAnswers"": [
        ""We could expedite delivery on condition that the scope of work remains clearly defined."",
        ""We can expedite the delivery schedule provided that the project scope remains clearly defined."",
        ""We could accelerate delivery on condition that the work scope remains strictly defined.""
      ],
      ""sampleAnswer"": ""We could expedite delivery on condition that the scope of work remains clearly defined."",
      ""grammarNoteVi"": ""\""on condition that\"" là liên từ chỉ điều kiện ràng buộc chặt chẽ trong hợp đồng kinh tế.""
    },
    {
      ""id"": ""en-l32-trans-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Dịch: Đặt câu hỏi làm rõ khúc mắc (Vừa)"",
      ""sourceVi"": ""Bạn có thể chia sẻ cụ thể hơn về những rào cản chính khiến đội ngũ của bạn chưa thể chấp nhận mức giá này không?"",
      ""hintKeywords"": [
        ""elaborate on main constraints"",
        ""preventing your team"",
        ""accepting this price""
      ],
      ""acceptedAnswers"": [
        ""Could you elaborate on the main constraints preventing your team from accepting this price?"",
        ""Can you elaborate on the primary constraints that prevent your team from adopting this pricing tier?"",
        ""Could you please explain the main constraints preventing your team from accepting this price?""
      ],
      ""sampleAnswer"": ""Could you elaborate on the main constraints preventing your team from accepting this price?"",
      ""grammarNoteVi"": ""Cấu trúc: \""prevent someone from doing something\"" (ngăn cản ai làm việc gì).""
    },
    {
      ""id"": ""en-l32-trans-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Dịch: Đàm phán nguyên tắc giải tỏa đối đầu (Khó)"",
      ""sourceVi"": ""Làm chủ nghệ thuật đàm phán nguyên tắc cho phép bạn giải quyết các xung đột lợi ích một cách khéo léo, tự tin và xây dựng quan hệ đối tác bền vững."",
      ""hintKeywords"": [
        ""mastering principled negotiation"",
        ""resolve conflicts of interest"",
        ""grace and confidence"",
        ""sustainable partnerships""
      ],
      ""acceptedAnswers"": [
        ""Mastering principled negotiation enables you to resolve conflicts with grace and confidence and build sustainable partnerships."",
        ""Mastering principled negotiation allows you to resolve conflicting interests with grace and confidence while creating sustainable partnerships.""
      ],
      ""sampleAnswer"": ""Mastering principled negotiation enables you to resolve conflicts with grace and confidence and build sustainable partnerships."",
      ""grammarNoteVi"": ""Cụm danh động từ chủ ngữ (\""Mastering...\"") đi với động từ số ít (\""enables\"").""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.2 Grammar Fix (OrderIndex 17)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444451"), "grammar_fix", 17),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444451"),
            OrderIndex = 17,
            Type = "grammar_fix",
            Title = "Sửa lỗi ngữ pháp (Bài 3.2): Khắc phục các lỗi người Việt hay mắc",
            Description = "Phát hiện và sửa các lỗi sai ngữ pháp, thì động từ, mạo từ và giới từ thường gặp trong giao tiếp thực tế.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l32-gf-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Dùng will trong mệnh đề if (Dễ)"",
      ""incorrectSentence"": ""If you will agree to our timeline, we will adjust the licensing fee."",
      ""errorTypeHintVi"": ""Trong mệnh đề chỉ điều kiện (mệnh đề If), không dùng trợ động từ \""will\""."",
      ""correctedSentence"": ""If you agree to our timeline, we will adjust the licensing fee."",
      ""acceptedAnswers"": [
        ""If you agree to our timeline, we will adjust the licensing fee."",
        ""Provided you agree to our timeline, we will adjust the licensing fee.""
      ],
      ""meaningVi"": ""Nếu bạn đồng ý với tiến độ của chúng tôi, chúng tôi sẽ điều chỉnh phí bản quyền."",
      ""explanationVi"": ""Quy tắc câu điều kiện loại 1: Mệnh đề if dùng thì Hiện tại đơn (\""agree\""), mệnh đề chính dùng \""will + V\"".""
    },
    {
      ""id"": ""en-l32-gf-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Sửa lỗi: Nhầm lẫn giữa compromise và promise (Dễ)"",
      ""incorrectSentence"": ""We reached a fair promise that satisfied both companies."",
      ""errorTypeHintVi"": ""Sự thỏa hiệp nhượng bộ hai bên dùng danh từ \""compromise\"", không dùng \""promise\"" (lời hứa)."",
      ""correctedSentence"": ""We reached a fair compromise that satisfied both companies."",
      ""acceptedAnswers"": [
        ""We reached a fair compromise that satisfied both companies."",
        ""We reached a fair settlement that satisfied both companies.""
      ],
      ""meaningVi"": ""Chúng tôi đã đạt được sự thỏa hiệp công bằng làm hài lòng cả hai công ty."",
      ""explanationVi"": ""\""Compromise\"" là giải pháp dung hòa đôi bên nhượng bộ nhau; \""promise\"" chỉ đơn thuần là một lời hứa.""
    },
    {
      ""id"": ""en-l32-gf-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Dùng sai giới từ sau condition (Vừa)"",
      ""incorrectSentence"": ""We can deliver early in condition that the scope is clearly defined."",
      ""errorTypeHintVi"": ""Cụm từ cố định chỉ điều kiện ràng buộc là \""on condition that\"", không dùng \""in condition that\""."",
      ""correctedSentence"": ""We can deliver early on condition that the scope is clearly defined."",
      ""acceptedAnswers"": [
        ""We can deliver early on condition that the scope is clearly defined."",
        ""We can deliver early provided that the scope is clearly defined.""
      ],
      ""meaningVi"": ""Chúng tôi có thể bàn giao sớm với điều kiện là phạm vi công việc được xác định rõ ràng."",
      ""explanationVi"": ""Thành ngữ pháp lý chuẩn xác là \""on condition that\"".""
    },
    {
      ""id"": ""en-l32-gf-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Sửa lỗi: Giới từ sau prevent (Vừa)"",
      ""incorrectSentence"": ""What are the main obstacles preventing you to sign the agreement?"",
      ""errorTypeHintVi"": ""Cấu trúc sau \""prevent someone\"" đi với giới từ \""from + V-ing\"", không đi với \""to + V-infinitive\""."",
      ""correctedSentence"": ""What are the main obstacles preventing you from signing the agreement?"",
      ""acceptedAnswers"": [
        ""What are the main obstacles preventing you from signing the agreement?"",
        ""What are the main obstacles that prevent you from signing the agreement?""
      ],
      ""meaningVi"": ""Những trở ngại chính ngăn cản bạn ký thỏa thuận này là gì?"",
      ""explanationVi"": ""Cấu trúc chuẩn: \""prevent someone from doing something\"".""
    },
    {
      ""id"": ""en-l32-gf-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Sửa lỗi: Dùng sai dạng đảo ngữ câu điều kiện loại 2 (Khó)"",
      ""incorrectSentence"": ""If we were split the difference, would your team commit to a two-year contract?"",
      ""errorTypeHintVi"": ""Trong cấu trúc giả định câu điều kiện loại 2 trang trọng, dạng đúng là \""If we were to split the difference\""."",
      ""correctedSentence"": ""If we were to split the difference, would your team commit to a two-year contract?"",
      ""acceptedAnswers"": [
        ""If we were to split the difference, would your team commit to a two-year contract?"",
        ""Were we to split the difference, would your team commit to a two-year contract?""
      ],
      ""meaningVi"": ""Nếu chúng ta chia đôi khoản chênh lệch, nhóm của bạn có cam kết hợp đồng hai năm không?"",
      ""explanationVi"": ""Cấu trúc đàm phán giả định lịch thiệp: \""If S + were to + V-inf\"" (hoặc đảo ngữ \""Were S to + V-inf\"").""
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Bài 3.2 Roleplay (OrderIndex 18)
        activities.Add(new PracticeActivity
        {
            Id = SeedDataInitializer.CreateDeterministicActivityId(courseId, Guid.Parse("44444444-4444-4444-8444-444444444451"), "roleplay", 18),
            CourseId = courseId,
            LessonId = Guid.Parse("44444444-4444-4444-8444-444444444451"),
            OrderIndex = 18,
            Type = "roleplay",
            Title = "Nhập vai hội thoại (Bài 3.2): Tình huống phản xạ thực chiến",
            Description = "Nhập vai giao tiếp với AI trong các tình huống thực tế, hỗ trợ chấm điểm tự động theo từ khóa và cấu trúc câu.",
            ConfigJson = @"{
  ""lang"": ""en-US"",
  ""difficulty"": ""medium"",
  ""items"": [
    {
      ""id"": ""en-l32-rp-1"",
      ""difficulty"": ""easy"",
      ""title"": ""Mở đầu đàm phán với tinh thần thiện chí (Dễ)"",
      ""scenarioVi"": ""Bạn bắt đầu buổi làm việc với giám đốc mua hàng của đối tác và muốn xác định tinh thần đôi bên cùng có lợi."",
      ""aiRole"": ""Sarah (Giám đốc Mua hàng)"",
      ""aiPrompt"": ""Good morning! We have reviewed your initial proposal. Our objective today is to see if we can align on terms."",
      ""aiPromptVi"": ""Chào buổi sáng! Chúng tôi đã xem qua đề xuất ban đầu. Mục tiêu hôm nay là xem liệu chúng ta có thể thống nhất các điều khoản không."",
      ""userGoalVi"": ""Bày tỏ sự trân trọng thời gian của đối tác, khẳng định mục tiêu đạt được thỏa thuận win-win công bằng."",
      ""expectedKeywords"": [
        ""good morning"",
        ""appreciate"",
        ""objective"",
        ""mutually beneficial"",
        ""win-win""
      ],
      ""sampleResponses"": [
        ""Good morning, Sarah! We truly appreciate your time today. Our primary objective is to forge a mutually beneficial agreement that supports both our strategic goals.""
      ],
      ""minWords"": 10
    },
    {
      ""id"": ""en-l32-rp-2"",
      ""difficulty"": ""easy"",
      ""title"": ""Phản hồi khi khách hàng chê giá đắt (Dễ)"",
      ""scenarioVi"": ""Khách hàng nhận xét mức phí đề xuất đang cao hơn ngân sách dự kiến 15%."",
      ""aiRole"": ""Sarah (Giám đốc Mua hàng)"",
      ""aiPrompt"": ""Your technological features are impressive, but your price is fifteen percent above our allocated budget cap."",
      ""aiPromptVi"": ""Các tính năng công nghệ của bạn rất ấn tượng, nhưng giá lại cao hơn 15% so với mức trần ngân sách của chúng tôi."",
      ""userGoalVi"": ""Ghi nhận lo ngại về ngân sách, giải thích giá trị hoàn vốn và đề xuất tìm hiểu các lựa chọn điều chỉnh linh hoạt."",
      ""expectedKeywords"": [
        ""understand"",
        ""budget"",
        ""value"",
        ""adjust"",
        ""explore options""
      ],
      ""sampleResponses"": [
        ""I completely understand your budget constraints, Sarah. Let's explore how we can adjust scope or payment terms to fit your financial parameters while delivering full value.""
      ],
      ""minWords"": 12
    },
    {
      ""id"": ""en-l32-rp-3"",
      ""difficulty"": ""medium"",
      ""title"": ""Đưa ra nhượng bộ chiến lược có điều kiện (Vừa)"",
      ""scenarioVi"": ""Khách hàng yêu cầu giảm giá 10%, bạn đồng ý nhưng kèm điều kiện họ phải ký hợp đồng cam kết 2 năm."",
      ""aiRole"": ""Sarah (Giám đốc Mua hàng)"",
      ""aiPrompt"": ""If you can reduce the overall implementation fee by ten percent, I can approve the contract today."",
      ""aiPromptVi"": ""Nếu bạn có thể giảm 10% phí triển khai tổng thể, tôi có thể phê duyệt hợp đồng ngay hôm nay."",
      ""userGoalVi"": ""Đồng ý giảm giá 10% nhưng gắn kèm điều kiện ràng buộc đối tác ký hợp đồng dịch vụ hỗ trợ 2 năm."",
      ""expectedKeywords"": [
        ""agree"",
        ""ten percent"",
        ""on condition that"",
        ""commit"",
        ""two-year agreement""
      ],
      ""sampleResponses"": [
        ""We can certainly accommodate that ten percent fee adjustment on condition that your enterprise commits to a two-year support agreement. Does that work for you?""
      ],
      ""minWords"": 14
    },
    {
      ""id"": ""en-l32-rp-4"",
      ""difficulty"": ""medium"",
      ""title"": ""Xử lý bế tắc về thời hạn bàn giao (Vừa)"",
      ""scenarioVi"": ""Khách hàng lo ngại thời hạn bàn giao 6 tuần là quá gấp cho bộ phận an ninh mạng của họ thẩm định."",
      ""aiRole"": ""Sarah (Giám đốc Mua hàng)"",
      ""aiPrompt"": ""Six weeks is too short for our security auditors. We might have to table this project until next quarter."",
      ""aiPromptVi"": ""Sáu tuần là quá ngắn để các chuyên gia kiểm toán an ninh của chúng tôi làm việc. Có lẽ chúng tôi phải gác dự án lại đến quý sau."",
      ""userGoalVi"": ""Khai thông bế tắc bằng cách đề xuất bàn giao phân kỳ: chuyển giao module bảo mật cốt lõi trong 3 tuần đầu tiên để kiểm toán trước."",
      ""expectedKeywords"": [
        ""understand concern"",
        ""phased delivery"",
        ""core module"",
        ""three weeks"",
        ""auditors"",
        ""compromise""
      ],
      ""sampleResponses"": [
        ""We completely understand. Instead of delaying the whole project, what if we adopt a phased delivery, handing over the core security modules within three weeks for your auditors to inspect early?""
      ],
      ""minWords"": 16
    },
    {
      ""id"": ""en-l32-rp-5"",
      ""difficulty"": ""hard"",
      ""title"": ""Chốt điều khoản thỏa hiệp và bước tiếp theo (Khó)"",
      ""scenarioVi"": ""Hai bên đã thống nhất các điều khoản then chốt về giá, tiến độ và bảo mật. Bạn cần chốt lại thỏa thuận trước khi ký chính thức."",
      ""aiRole"": ""Sarah (Giám đốc Mua hàng)"",
      ""aiPrompt"": ""The phased delivery and adjusted support terms sound like an exceptional compromise. How do we formalize this agreement?"",
      ""aiPromptVi"": ""Việc bàn giao theo giai đoạn và các điều khoản hỗ trợ được điều chỉnh nghe như một sự thỏa hiệp tuyệt vời. Chúng ta chính thức hóa thỏa thuận này thế nào?"",
      ""userGoalVi"": ""Tổng kết ngắn gọn 3 điểm cốt lõi đã thỏa hiệp, thông báo sẽ soạn thảo phụ lục hợp đồng chính thức và gửi cho bộ phận pháp lý rà soát."",
      ""expectedKeywords"": [
        ""summarize"",
        ""compromise"",
        ""phased delivery"",
        ""support agreement"",
        ""formal contract"",
        ""legal review"",
        ""partnership""
      ],
      ""sampleResponses"": [
        ""We are thrilled to reach this milestone, Sarah. To summarize: we will implement phased delivery with early security handover and a ten percent fee adjustment tied to a two-year agreement. I will prepare the formal contract draft for legal review by tomorrow morning.""
      ],
      ""minWords"": 18
    }
  ]
}",
            PassingScore = 70,
            IsSeeded = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        return activities;
    }

    private static string BuildLessonAst(string title, string p1, string p2)
    {
        var ast = new
        {
            type = "doc",
            content = new object[]
            {
                new
                {
                    type = "heading",
                    attrs = new { level = 1, block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = title } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = p1 } }
                },
                new
                {
                    type = "paragraph",
                    attrs = new { block_id = Guid.NewGuid().ToString() },
                    content = new[] { new { type = "text", text = p2 } }
                }
            }
        };
        return JsonSerializer.Serialize(ast);
    }
}
