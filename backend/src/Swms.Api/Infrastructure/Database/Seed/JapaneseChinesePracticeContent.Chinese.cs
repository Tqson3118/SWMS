namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using Swms.Api.Domain.Entities;

public static partial class JapaneseChinesePracticeContent
{
    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG SƠ CẤP (HSK1) — 6 BÀI LÝ THUYẾT CHI TIẾT
    // =========================================================================

    private static List<CjkLessonMeta> GetChineseLessons() => new()
    {
        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter1Id, SeedDataInitializer.ChineseLesson11Id,
            "Bài 1.1: Hệ thống Pinyin, 4 Thanh điệu & Chào hỏi",
            "zh-hsk1-pinyin-thanh-dieu-chao-hoi", 1, 25,
            @"# Bài 1.1: Hệ thống Pinyin, 4 Thanh điệu & Chào hỏi

## 1. Mục tiêu bài học
- Nắm vững hệ thống phiên âm **Pinyin** (Thanh mẫu — Vận mẫu — Thanh điệu) và đọc chuẩn **4 thanh điệu + thanh nhẹ**.
- Phát âm phân biệt nhóm **âm bật hơi** (`p, t, k, q, c, ch`) với **âm không bật hơi** (`b, d, g, j, z, zh`) và nhóm **âm uốn lưỡi** (`zh, ch, sh, r`).
- Giao tiếp tự nhiên các câu chào hỏi, cảm ơn, xin lỗi và tạm biệt cốt lõi của HSK1.

## 2. Bảng 4 Thanh điệu & Nhóm Thanh mẫu Pinyin trọng tâm

| Thanh điệu | Ký hiệu | Cao độ (1–5) | Ví dụ mẫu | Lưu ý phát âm cho người Việt |
|---|---|---|---|---|
| **Thanh 1** | `ā` (mā) | 55 (cao ngang) | 妈 mā (mẹ) | Cao hơn thanh ngang tiếng Việt và giữ phẳng, không tụt cuối |
| **Thanh 2** | `á` (má) | 35 (đi lên) | 麻 má (gai) | Lên dứt khoát từ trung bình lên cao như hỏi ""hả?"" |
| **Thanh 3** | `ǎ` (mǎ) | 214 (xuống-lên) | 马 mǎ (ngựa) | Hạ trầm sâu rồi mới vồng lên (trước âm khác đọc nửa thanh 3 trầm) |
| **Thanh 4** | `à` (mà) | 51 (rơi mạnh) | 骂 mà (mắng) | Rơi nhanh, gọn, dứt khoát từ cao xuống thấp |
| **Thanh nhẹ** | `a` (ma) | Nhẹ, ngắn | 吗 ma (không?) | Đọc ngắn và thả lỏng, không nhấn trọng âm |

> **Cặp thanh mẫu cần phân biệt rõ:** Không bật hơi (`b, d, g, j, z, zh`) ↔ Bật hơi mạnh (`p, t, k, q, c, ch`) | Đầu lưỡi thẳng (`z, c, s`) ↔ Uốn lưỡi (`zh, ch, sh, r`).

## 3. Hội thoại chào hỏi chuẩn HSK1
- **Linh:** 你好！请问，你好吗？ *(Nǐ hǎo! Qǐngwèn, nǐ hǎo ma?)* — Xin chào! Xin hỏi bạn có khỏe không?
- **Wang:** 我很好，谢谢！你呢？ *(Wǒ hěn hǎo, xièxie! Nǐ ne?)* — Tôi rất khỏe, cảm ơn! Còn bạn thì sao?
- **Linh:** 我也很好。对不起，请问老师在吗？ *(Wǒ yě hěn hǎo. Duìbuqǐ, qǐngwèn lǎoshī zài ma?)* — Tôi cũng rất khỏe. Xin lỗi, xin hỏi thầy/cô giáo có ở đây không?
- **Wang:** 没关系！老师在教室。再见！ *(Méi guānxi! Lǎoshī zài jiàoshì. Zàijiàn!)* — Không sao đâu! Thầy/cô ở phòng học. Tạm biệt!

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Pinyin (拼音)**: Hệ thống ký âm Latinh chuẩn của tiếng Trung phổ thông gồm Thanh mẫu (phụ âm đầu), Vận mẫu (vần) và 4 thanh điệu.
- **Trợ từ nghi vấn 吗 (ma)**: Đặt cuối câu trần thuật để chuyển ngay thành câu hỏi Có/Không (`你好吗？` = Bạn khỏe không?).
- **Trợ từ 呢 (ne)**: Đặt sau đại từ/danh từ để hỏi ngược lại ngắn gọn ""Còn... thì sao?"" (`你呢？` = Còn bạn thì sao?).
- **Phó từ mức độ 很 (hěn)**: Trong tiếng Trung, tính từ làm vị ngữ thường cần `很` đứng trước để cân bằng nhịp câu (`我很好`, không nói `我是好`).

## 5. Lỗi phát âm người Việt hay mắc
- **Đọc thanh 1 bị thấp thành thanh ngang tiếng Việt**: Thanh 1 phải giữ ở cao độ 55 thật cao và phẳng (`gāo`, `tā`, `zhōng`).
- **Không bật hơi ở `p, t, k, q, c, ch`**: Khi đọc `请 (qǐng)`, `茶 (chá)`, `吃 (chī)`, tờ giấy đặt trước miệng phải rung mạnh.
- **Đọc `zh/ch/sh` như `z/c/s` không uốn lưỡi**: Phải cong đầu lưỡi chạm vòm miệng trên cho `中 (zhōng)`, `老师 (lǎoshī)`.

<!-- studxy-cjk-v1 -->",
            "Làm chủ hệ thống Pinyin, 4 thanh điệu chuẩn và các mẫu câu chào hỏi HSK1.",
            "Phân biệt nhóm âm bật hơi p/t/k/q/c/ch và nhóm âm uốn lưỡi zh/ch/sh/r cho người Việt."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter1Id, SeedDataInitializer.ChineseLesson12Id,
            "Bài 1.2: Quy tắc biến điệu, Chữ Hán cơ bản & Giới thiệu tên, quốc tịch",
            "zh-hsk1-bien-dieu-chu-han-gioi-thieu", 2, 25,
            @"# Bài 1.2: Quy tắc biến điệu, Chữ Hán cơ bản & Giới thiệu tên, quốc tịch

## 1. Mục tiêu bài học
- Vận dụng chuẩn **3 quy tắc biến điệu vàng** của HSK1: biến điệu **hai thanh 3 liền nhau**, biến điệu của **不 (bù)** và **一 (yī)**.
- Nhận diện các nét cơ bản và bộ thủ chữ Hán quen thuộc (`人`, `口`, `女`, `言`, `国`).
- Tự tin hỏi đáp tên (`你叫什么名字？`), quốc tịch (`你是哪国人？`) và giới thiệu bản thân bằng câu chữ **是 (shì)**.

## 2. Bảng Quy tắc Biến điệu & Bộ thủ Chữ Hán nền tảng

| Quy tắc / Bộ thủ | Cách viết chuẩn | Cách đọc thực tế | Ví dụ HSK1 |
|---|---|---|---|
| **Thanh 3 + Thanh 3** | `nǐ hǎo` (3 + 3) | Đọc thành **2 + 3**: `ní hǎo` | 你好, 很好 (hén hǎo) |
| **Biến điệu 不 (bù)** | `bù` + thanh 4 | Đọc thành **bú**: `bú shì` | 不是 (bú shì), 不客气 (bú kèqi) |
| **Biến điệu 一 (yī)** | `yī` + thanh 4 / thanh 1-2-3 | `yí` (trước thanh 4) / `yì` (còn lại) | 一个 (yí gè), 一天 (yì tiān) |
| **Bộ Nhân (亻/人)** | Liên quan đến người | rén | 你 (bạn), 他 (anh ấy), 们 (các) |
| **Bộ Khẩu (口)** | Liên quan đến miệng/lời nói | kǒu | 叫 (gọi tên), 吗 (không?), 吃 (ăn) |
| **Bộ Vi (囗)** | Bao quanh / Lãnh thổ | wéi | 国 (nước), 园 (vườn) |

## 3. Hội thoại giới thiệu tên & quốc tịch
- **Minh:** 你好！我叫明，请问你叫什么名字？ *(Nǐ hǎo! Wǒ jiào Míng, qǐngwèn nǐ jiào shénme míngzi?)* — Xin chào! Tôi tên là Minh, xin hỏi bạn tên là gì?
- **Li:** 你好！我叫李娜。很高兴认识你！ *(Nǐ hǎo! Wǒ jiào Lǐ Nà. Hěn gāoxìng rènshi nǐ!)* — Xin chào! Tôi là Lý Na. Rất vui được quen bạn!
- **Minh:** 认识你我也很高兴。你是中国人吗？ *(Rènshi nǐ wǒ yě hěn gāoxìng. Nǐ shì Zhōngguó rén ma?)* — Quen bạn tôi cũng rất vui. Bạn là người Trung Quốc phải không?
- **Li:** 是的，我是中国人。你是哪国人？ *(Shì de, wǒ shì Zhōngguó rén. Nǐ shì nǎ guó rén?)* — Đúng vậy, tôi là người Trung Quốc. Bạn là người nước nào?
- **Minh:** 我是越南人，我是大学生。 *(Wǒ shì Yuènán rén, wǒ shì dàxuéshēng.)* — Tôi là người Việt Nam, tôi là sinh viên đại học.

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Câu chữ 是 (shì)**: Dùng nối hai danh từ (`我是越南人` = Tôi là người Việt Nam); phủ định là `不是` (`我不是老师`), TUYỆT ĐỐI không dùng `是` trước tính từ (`我很高兴`, không nói `我是高兴`).
- **Biến điệu thanh 3**: Khi hai chữ thanh 3 đứng cạnh nhau (`你好`, `很好`), chữ đầu chuyển sang đọc như thanh 2 (`ní hǎo`), nhưng Pinyin vẫn viết dấu gốc.
- **Đại từ nghi vấn 什么 và 哪**: `什么 (shénme)` = cái gì; `哪 (nǎ)` = nào (`哪国人` = người nước nào). Trật tự câu hỏi giữ nguyên như câu khẳng định, không đảo ngữ.
- **Phó từ 也 (yě — cũng)**: Luôn đứng TRƯỚC động từ hoặc phó từ `很` (`我也很好`, `我也是学生`).

## 5. Lỗi phát âm người Việt hay mắc
- **Quên biến điệu `不是 (bú shì)` và `你好 (ní hǎo)`**: Đọc nặng cả hai thanh 3 hoặc đọc `bù shì` đều nghe gượng gạo.
- **Đọc sai `认识 (rènshi)`**: `r` uốn lưỡi nhẹ (không rung kiểu `r` tiếng Việt), `sh` uốn lưỡi và đọc thanh nhẹ ở `shi`.

<!-- studxy-cjk-v1 -->",
            "Áp dụng quy tắc biến điệu thanh 3, 不, 一 và nhận diện các bộ thủ chữ Hán HSK1.",
            "Thành thạo mẫu câu hỏi tên, quốc tịch với 叫, 什么, 哪 và câu khẳng/phủ định chữ 是."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter2Id, SeedDataInitializer.ChineseLesson21Id,
            "Bài 2.1: Số đếm, Ngày tháng & Thời gian",
            "zh-hsk1-so-dem-ngay-thang-thoi-gian", 1, 25,
            @"# Bài 2.1: Số đếm, Ngày tháng & Thời gian

## 1. Mục tiêu bài học
- Đếm số từ 0 đến 100 và phân biệt chính xác **二 (èr)** với **两 (liǎng)** khi nói giờ và lượng từ.
- Nói ngày tháng năm, thứ trong tuần theo trật tự chuẩn tiếng Trung **Năm → Tháng → Ngày → Thứ**.
- Hỏi đáp giờ giấc (`现在几点？`), lịch hẹn bằng **点 (diǎn)**, **分 (fēn)**, **半 (bàn)**, **刻 (kè)**.

## 2. Bảng Số đếm, Thứ ngày & Thời gian HSK1

| Hán tự | Pinyin | Nghĩa | Mẫu ghép thực tế |
|---|---|---|---|
| **一 二 三 四 五** | yī, èr, sān, sì, wǔ | 1, 2, 3, 4, 5 | 一月 (tháng 1), 五点 (5 giờ) |
| **六 七 八 九 十** | liù, qī, bā, jiǔ, shí | 6, 7, 8, 9, 10 | 十月十日 (ngày 10 tháng 10) |
| **两** | liǎng | Hai (đi với giờ/lượng từ) | **两点** (2 giờ — không nói 二点!) |
| **年 / 月 / 号(日)** | nián / yuè / hào (rì) | Năm / Tháng / Ngày | 2026年5月8号 |
| **星期一 ~ 星期天** | xīngqī yī ~ xīngqī tiān | Thứ 2 ~ Chủ nhật | 星期六 (thứ Bảy), 星期天 (CN) |
| **点 / 分 / 半** | diǎn / fēn / bàn | Giờ / Phút / Rưỡi | 八点半 (8:30), 九点十五分 (9:15) |

## 3. Hội thoại hỏi ngày tháng & giờ giấc
- **An:** 请问，今天几月几号？星期几？ *(Qǐngwèn, jīntiān jǐ yuè jǐ hào? Xīngqī jǐ?)* — Xin hỏi hôm nay ngày mấy tháng mấy? Thứ mấy?
- **Ming:** 今天十月十二号，星期三。 *(Jīntiān shí yuè shí'èr hào, xīngqī sān.)* — Hôm nay là ngày 12 tháng 10, thứ Tư.
- **An:** 现在几点了？ *(Xiànzài jǐ diǎn le?)* — Bây giờ mấy giờ rồi?
- **Ming:** 现在上午九点半。我们下午两点见，好吗？ *(Xiànzài shàngwǔ jiǔ diǎn bàn. Wǒmen xiàwǔ liǎng diǎn jiàn, hǎo ma?)* — Bây giờ là 9 giờ rưỡi sáng. Chúng ta gặp nhau lúc 2 giờ chiều nhé?
- **An:** 好的，下午两点见！ *(Hǎo de, xiàwǔ liǎng diǎn jiàn!)* — Được thôi, 2 giờ chiều gặp nhé!

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Trật tự thời gian lớn trước – nhỏ sau**: Luôn sắp xếp theo thứ tự `Năm (年) → Tháng (月) → Ngày (号/日) → Buổi (上午/下午) → Giờ (点) → Phút (分)`.
- **Phân biệt 二 (èr) và 两 (liǎng)**: Đếm số, ngày tháng, thứ tự dùng `二` (`二月`, `十二号`, `星期二`); trước lượng từ và `点` (giờ) bắt buộc dùng `两` (`两点` = 2 giờ, `两个人` = 2 người).
- **Vị trí trạng ngữ thời gian**: Từ chỉ thời gian đặt trước hoặc ngay sau chủ ngữ, nhưng luôn đứng TRƯỚC động từ (`我明天上午九点去` = 9 giờ sáng mai tôi đi).
- **Cách nói thứ trong tuần**: `星期一` là thứ Hai (số 1), cộng thêm 1 so với tiếng Việt (`星期三` = thứ Tư); Chủ nhật là `星期天` hoặc `星期日`.

## 5. Lỗi phát âm người Việt hay mắc
- **Nhầm `四 (sì - 4)` và `十 (shí - 10)`**: `sì` đầu lưỡi thẳng + thanh 4 rơi mạnh; `shí` uốn lưỡi + thanh 2 đi lên.
- **Quên bật hơi `七 (qī)` và `千 (qiān)`**: Đẩy luồng hơi mạnh qua khe răng, giữ thanh 1 cao phẳng.

<!-- studxy-cjk-v1 -->",
            "Đếm số, nói ngày tháng năm, thứ trong tuần và giờ giấc theo chuẩn trật tự tiếng Trung.",
            "Phân biệt 二 vs 两, 四 (sì) vs 十 (shí) và đặt trạng ngữ thời gian đúng vị trí."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter2Id, SeedDataInitializer.ChineseLesson22Id,
            "Bài 2.2: Gia đình, Hỏi giá cả & Mua sắm đơn giản",
            "zh-hsk1-gia-dinh-hoi-gia-mua-sam", 2, 25,
            @"# Bài 2.2: Gia đình, Hỏi giá cả & Mua sắm đơn giản

## 1. Mục tiêu bài học
- Giới thiệu thành viên gia đình bằng động từ **有 (yǒu)**, lượng từ **口 (kǒu)**, **个 (gè)** và trợ từ sở hữu **的 (de)**.
- Tự tin đi chợ/cửa hàng hỏi giá tiền (**多少钱**), nói đơn vị tiền tệ (**块 / 元**) và mặc cả cơ bản (**太贵了，便宜一点儿**).
- Phân biệt chuẩn phó từ phủ định **不 (bù)** và **没 (méi)** (với `有` luôn dùng `没有`).

## 2. Từ vựng Gia đình, Lượng từ & Mua sắm HSK1

| Hán tự | Pinyin | Nghĩa tiếng Việt | Ví dụ mẫu |
|---|---|---|---|
| **家 / 口** | jiā / kǒu | Nhà, gia đình / lượng từ nhân khẩu | 我家有四口人。 |
| **爸爸 / 妈妈** | bàba / māma | Bố / Mẹ | 这是我爸爸和妈妈。 |
| **哥哥 / 妹妹** | gēge / mèimei | Anh trai / Em gái | 我有一个哥哥。 |
| **多少钱** | duōshao qián | Bao nhiêu tiền | 这个苹果多少钱？ |
| **块 / 元** | kuài / yuán | Tệ / Đồng (khẩu ngữ / viết) | 十五块钱。 |
| **太……了** | tài ... le | Quá ... rồi | 太贵了！(Đắt quá!) |
| **便宜** | piányi | Rẻ | 可以便宜一点儿吗？ |

## 3. Hội thoại giới thiệu gia đình & mua hoa quả
- **Lan:** 你家有几口人？ *(Nǐ jiā yǒu jǐ kǒu rén?)* — Nhà bạn có mấy người?
- **Minh:** 我家有四口人：爸爸、妈妈、姐姐和我。 *(Wǒ jiā yǒu sì kǒu rén: bàba, māma, jiějie hé wǒ.)* — Nhà tôi có 4 người: bố, mẹ, chị gái và tôi.
- **Người bán:** 你好！你要买什么？ *(Nǐ hǎo! Nǐ yào mǎi shénme?)* — Xin chào! Bạn muốn mua gì?
- **Minh:** 我要买苹果。这个苹果多少钱一斤？ *(Wǒ yào mǎi píngguǒ. Zhège píngguǒ duōshao qián yì jīn?)* — Tôi muốn mua táo. Táo này bao nhiêu tiền một cân (500g)?
- **Người bán:** 八块钱一斤。 *(Bā kuài qián yì jīn.)* — 8 tệ một cân.
- **Minh:** 太贵了！便宜一点儿吧，我买两斤。 *(Tài guì le! Piányi yìdiǎnr ba, wǒ mǎi liǎng jīn.)* — Đắt quá! Rẻ hơn chút đi, tôi mua 2 cân.

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Phủ định của 有 là 没有**: KHÔNG BAO GIỜ nói `不有`! Phải nói `我没有哥哥` (Tôi không có anh trai).
- **Lượng từ bắt buộc giữa Số từ và Danh từ**: Tiếng Trung luôn có cấu trúc `Số từ + Lượng từ + Danh từ` (`四口人`, `三个苹果`, `两本书`), không bỏ qua lượng từ.
- **Trợ từ kết cấu 的 (de)**: Biểu thị sở hữu (`我的书` = sách của tôi). Với người thân rất gần có thể lược bỏ (`我爸爸`, `我家`).
- **Cấu trúc cảm thán 太 + Tính từ + 了**: Diễn tả mức độ cao (`太贵了` = đắt quá, `太好了` = tuyệt quá!).

## 5. Lỗi phát âm người Việt hay mắc
- **Phân biệt `买 (mǎi - mua)` và `卖 (mài - bán)`**: Khác nhau DUY NHẤT ở thanh 3 (xuống-lên) và thanh 4 (rơi mạnh dứt khoát).
- **Âm bật hơi trong `钱 (qián)` và `苹果 (píngguǒ)`**: Cả `q` và `p` đều phải bật hơi rõ ràng.

<!-- studxy-cjk-v1 -->",
            "Giới thiệu gia đình bằng 有/口, dùng đúng lượng từ và mẫu câu hỏi giá 多少钱, mặc cả 太贵了.",
            "Phân biệt 买 (mǎi) vs 卖 (mài) và không bao giờ nhầm 没有 thành 不有."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter3Id, SeedDataInitializer.ChineseLesson31Id,
            "Bài 3.1: Ăn uống, Gọi món & Sở thích",
            "zh-hsk1-an-uong-goi-mon-so-thich", 1, 25,
            @"# Bài 3.1: Ăn uống, Gọi món & Sở thích

## 1. Mục tiêu bài học
- Sử dụng động từ năng nguyện **想 (xiǎng — muốn)**, **喜欢 (xǐhuan — thích)** và **会 (huì — biết làm kỹ năng)** để nói về đồ ăn và sở thích.
- Gọi món ăn, đồ uống quen thuộc tại nhà hàng Trung Quốc (`米饭`, `面条`, `茶`, `杯`, `碗`, `菜`).
- Luyện phát âm tròn vành nhóm âm uốn lưỡi `吃 (chī)`, `茶 (chá)`, `菜 (cài)`, `水 (shuǐ)` và thanh nhẹ trong `喜欢 (xǐhuan)`.

## 2. Từ vựng Ăn uống & Sở thích HSK1

| Hán tự | Pinyin | Nghĩa tiếng Việt | Cụm từ thường gặp |
|---|---|---|---|
| **吃 / 喝** | chī / hē | Ăn / Uống | 吃饭 (ăn cơm), 喝茶 (uống trà) |
| **米饭 / 面条** | mǐfàn / miàntiáo | Cơm trắng / Mì sợi | 一碗米饭 (1 bát cơm) |
| **茶 / 水 / 咖啡** | chá / shuǐ / kāfēi | Trà / Nước / Cà phê | 一杯热茶 (1 tách trà nóng) |
| **想 / 要** | xiǎng / yào | Muốn (nhẹ nhàng) / Cần, gọi món | 我想喝茶 / 我要一杯咖啡 |
| **喜欢** | xǐhuan | Thích | 我很喜欢吃中国菜。 |
| **好吃 / 好喝** | hǎochī / hǎohē | Ngon (đồ ăn) / Ngon (đồ uống) | 这个菜很好吃！ |

## 3. Hội thoại gọi món tại nhà hàng
- **Phục vụ:** 欢迎光临！请问您想吃什么？ *(Huānyíng guānglín! Qǐngwèn nín xiǎng chī shénme?)* — Chào mừng quý khách! Xin hỏi quý khách muốn ăn gì?
- **Minh:** 你好，我很喜欢吃中国菜。我要一碗米饭和一盘饺子。 *(Nǐ hǎo, wǒ hěn xǐhuan chī Zhōngguó cài. Wǒ yào yì wǎn mǐfàn hé yì pán jiǎozi.)* — Chào bạn, tôi rất thích ăn món Trung Quốc. Cho tôi 1 bát cơm và 1 đĩa sủi cảo.
- **Phục vụ:** 您想喝什么？茶还是咖啡？ *(Nín xiǎng hē shénme? Chá háishi kāfēi?)* — Quý khách muốn uống gì ạ? Trà hay cà phê?
- **Minh:** 请给我一杯热茶，谢谢！ *(Qǐng gěi wǒ yì bēi rè chá, xièxie!)* — Làm ơn cho tôi một tách trà nóng, cảm ơn!
- **Phục vụ:** 好的，请稍等！ *(Hǎo de, qǐng shāoděng!)* — Vâng ạ, xin vui lòng đợi một lát!

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Phân biệt 想 (xiǎng) và 要 (yào)**: `想` diễn tả mong muốn nhẹ nhàng (`我想去北京` = tôi muốn đi Bắc Kinh); `要` thể hiện ý định rõ hoặc gọi món trực tiếp (`我要一杯茶` = cho tôi 1 tách trà).
- **Câu hỏi lựa chọn 还是 (háishi)**: Dùng trong câu hỏi ""A hay là B?"" (`喝茶还是喝咖啡？`), KHÔNG dùng `吗` ở cuối câu đã có `还是`!
- **Cấu trúc 请给我... (Qǐng gěi wǒ...)**: Mẫu câu đề nghị lịch sự ""Làm ơn cho tôi..."" rất phổ biến khi gọi món hoặc mua đồ.
- **Động từ năng nguyện 会 (huì)**: Biểu thị kỹ năng học được (`我会说汉语` = tôi biết nói tiếng Trung, `我会做中国菜` = tôi biết nấu món Trung).

## 5. Lỗi phát âm người Việt hay mắc
- **Phân biệt `吃 (chī)` với `次 (cì)` và `茶 (chá)` với `菜 (cài)`**: `ch` phải uốn lưỡi + bật hơi mạnh; `c` đặt đầu lưỡi thẳng sau răng + bật hơi mạnh.
- **Đọc sai `水 (shuǐ)` thành `sủi`**: Phải uốn lưỡi `sh` và giữ thanh 3 trầm rõ.

<!-- studxy-cjk-v1 -->",
            "Gọi món ăn, đồ uống bằng 想/要/请给我 và dùng câu hỏi lựa chọn 还是 (hay là).",
            "Diễn đạt sở thích ăn uống, kỹ năng cá nhân với 喜欢/会 và luyện âm bật hơi uốn lưỡi 吃/茶/水."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter3Id, SeedDataInitializer.ChineseLesson32Id,
            "Bài 3.2: Hỏi đường, Di chuyển & Lịch trình hằng ngày",
            "zh-hsk1-hoi-duong-di-chuyen-lich-trinh", 2, 25,
            @"# Bài 3.2: Hỏi đường, Di chuyển & Lịch trình hằng ngày

## 1. Mục tiêu bài học
- Hỏi vị trí và đường đi bằng **……在哪儿？**, **去……怎么走？** và phương tiện giao thông với **坐 (zuò — ngồi/đi phương tiện)**.
- Diễn đạt lịch trình sinh hoạt hằng ngày theo cấu trúc câu liên động **Đi đâu làm gì** (`去 + Địa điểm + Động từ`).
- Sử dụng thành thạo phương vị từ (`前面`, `后面`, `左边`, `右边`, `里面`) và trạng từ chỉ hành động đang diễn ra **在 (zài)**.

## 2. Từ vựng Phương hướng, Phương tiện & Lịch trình HSK1

| Hán tự | Pinyin | Nghĩa tiếng Việt | Ví dụ mẫu |
|---|---|---|---|
| **在哪儿 / 哪里** | zài nǎr / nǎlǐ | Ở đâu | 火车站在哪儿？ |
| **怎么走** | zěnme zǒu | Đi như thế nào | 请问，去医院怎么走？ |
| **坐出租车 / 地铁** | zuò chūzūchē / dìtiě | Đi taxi / tàu điện ngầm | 我坐地铁去学校。 |
| **前面 / 后面** | qiánmian / hòumian | Phía trước / Phía sau | 学校在医院前面。 |
| **左边 / 右边** | zuǒbian / yòubian | Bên trái / Bên phải | 商店在银行右边。 |
| **学习 / 工作** | xuéxí / gōngzuò | Học tập / Làm việc | 我每天学习汉语。 |

## 3. Hội thoại hỏi đường ra ga tàu & lịch trình
- **Minh:** 请问，去火车站怎么走？ *(Qǐngwèn, qù huǒchēzhàn zěnme zǒu?)* — Xin hỏi, đi ra ga tàu hỏa đi thế nào ạ?
- **Người dân:** 从这里一直往前走，到路口往左拐就到了。 *(Cóng zhèlǐ yìzhí wǎng qián zǒu, dào lùkǒu wǎng zuǒ guǎi jiù dào le.)* — Từ đây đi thẳng về phía trước, đến ngã tư rẽ trái là tới.
- **Minh:** 走路远吗？我可以坐地铁去吗？ *(Zǒulù yuǎn ma? Wǒ kěyǐ zuò dìtiě qù ma?)* — Đi bộ có xa không? Tôi có thể đi tàu điện ngầm tới đó không?
- **Người dân:** 不远，走路十分钟。地铁站就在银行前面。 *(Bù yuǎn, zǒulù shí fēnzhōng. Dìtiězhàn jiù zài yínháng qiánmian.)* — Không xa đâu, đi bộ 10 phút. Ga tàu điện ngầm ở ngay trước ngân hàng.
- **Minh:** 太谢谢您了！ *(Tài xièxie nín le!)* — Cảm ơn bác rất nhiều ạ!

## 4. Điểm ngữ pháp & Khái niệm cốt lõi
- **Mẫu câu hỏi đường 去 + [Địa điểm] + 怎么走？**: Nghĩa là ""Đi đến [Địa điểm] đi như thế nào?"", cực kỳ tự nhiên và chuẩn HSK1.
- **Cấu trúc chỉ phương tiện 坐 + [Phương tiện] + 去 + [Địa điểm]**: Động từ `坐` đứng TRƯỚC `去` (`我坐公共汽车去公司` = Tôi đi xe buýt đến công ty; đi máy bay là `坐飞机`).
- **Câu liên động 去 + [Nơi chốn] + [Hành động]**: Không cần từ nối ""để"" (`我去图书馆看书` = Tôi đến thư viện đọc sách).
- **Trợ từ vị trí 在 (zài)**: Cấu trúc `A 在 B + Phương vị từ` (`商店在学校前面` = Cửa hàng ở phía trước trường học).

## 5. Lỗi phát âm người Việt hay mắc
- **Nhầm `坐 (zuò)` với `走 (zǒu)`**: `zuò` thanh 4 rơi gọn, vần `uo`; `zǒu` thanh 3 trầm, vần `ou`.
- **Âm uốn lưỡi & bật hơi trong `出租车 (chūzūchē)`**: Chú ý chuyển đổi linh hoạt giữa `ch` (uốn lưỡi bật hơi) và `z` (đầu lưỡi thẳng không bật hơi).

<!-- studxy-cjk-v1 -->",
            "Hỏi đường bằng 在哪儿 / 怎么走, diễn đạt phương tiện đi lại bằng 坐 và phương vị từ cơ bản.",
            "Thành thạo câu liên động 去 + Địa điểm + Hành động và lịch trình sinh hoạt hằng ngày HSK1.")
    };

    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG SƠ CẤP (HSK1) — KHO ĐỀ LUYỆN TẬP 6 BÀI (48 PACK / 228 CÂU)
    // =========================================================================

    private static List<PracticeActivity> GetChineseActivities(Guid cId, DateTimeOffset now)
    {
        var all = new List<PracticeActivity>();

        // ---------------- LESSON 1.1 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.ChineseLesson11Id, "zh-CN", "Trung HSK1 · Bài 1.1",
            shadowing: new[]
            {
                new ShItem("zh-l11-sh-1", "easy", "Chào hỏi cơ bản (Dễ)", "你好！", "nǐ hǎo", "Xin chào!", new[] { "你好" }),
                new ShItem("zh-l11-sh-2", "easy", "Cảm ơn & đáp lễ (Dễ)", "谢谢！不客气。", "xièxie! bú kèqi", "Cảm ơn! Không có gì.", new[] { "谢谢", "不客气" }),
                new ShItem("zh-l11-sh-3", "easy", "Chào thầy cô (Dễ)", "老师好！", "lǎoshī hǎo", "Em chào thầy/cô ạ!", new[] { "老师", "好" }),
                new ShItem("zh-l11-sh-4", "medium", "Hỏi sức khỏe (Vừa)", "你好吗？我很好。", "nǐ hǎo ma? wǒ hěn hǎo", "Bạn khỏe không? Tôi rất khỏe.", new[] { "你好吗", "很好" }),
                new ShItem("zh-l11-sh-5", "medium", "Xin lỗi & không sao (Vừa)", "对不起！没关系。", "duìbuqǐ! méi guānxi", "Xin lỗi! Không sao đâu.", new[] { "对不起", "没关系" }),
                new ShItem("zh-l11-sh-6", "medium", "Hỏi ngược lại (Vừa)", "我很好，谢谢！你呢？", "wǒ hěn hǎo, xièxie! nǐ ne", "Tôi rất khỏe, cảm ơn! Còn bạn thì sao?", new[] { "很好", "谢谢", "你呢" }),
                new ShItem("zh-l11-sh-7", "hard", "Phó từ 也 (Khó)", "我也很好，谢谢你！", "wǒ yě hěn hǎo, xièxie nǐ", "Tôi cũng rất khỏe, cảm ơn bạn!", new[] { "我也", "很好" }),
                new ShItem("zh-l11-sh-8", "hard", "Xin hỏi lịch sự (Khó)", "请问，老师今天在教室吗？", "qǐngwèn, lǎoshī jīntiān zài jiàoshì ma", "Xin hỏi hôm nay thầy/cô có ở phòng học không?", new[] { "请问", "老师", "教室" }),
                new ShItem("zh-l11-sh-9", "hard", "Chào buổi sáng & tạm biệt (Khó)", "早上好！明天见，再见！", "zǎoshang hǎo! míngtiān jiàn, zàijiàn", "Chào buổi sáng! Ngày mai gặp lại, tạm biệt!", new[] { "早上好", "明天见", "再见" })
            },
            dictation: new[]
            {
                new DiItem("zh-l11-di-1", "easy", "Hỏi thăm", "你好吗", "nǐ hǎo ma", "Bạn khỏe không?", new[] { "你", "好", "吗" }, new[] { "我", "呢", "不" }),
                new DiItem("zh-l11-di-2", "easy", "Cảm ơn", "谢谢你", "xièxie nǐ", "Cảm ơn bạn.", new[] { "谢谢", "你" }, new[] { "再见", "好", "吗" }),
                new DiItem("zh-l11-di-3", "medium", "Trả lời khỏe", "我很好你呢", "wǒ hěn hǎo nǐ ne", "Tôi rất khỏe, còn bạn?", new[] { "我", "很好", "你", "呢" }, new[] { "吗", "是", "不" }),
                new DiItem("zh-l11-di-4", "medium", "Xin lỗi & không sao", "对不起没关系", "duìbuqǐ méi guānxi", "Xin lỗi. Không sao đâu.", new[] { "对不起", "没关系" }, new[] { "不客气", "谢谢", "再见" }),
                new DiItem("zh-l11-di-5", "hard", "Cũng rất khỏe", "我也很好谢谢老师", "wǒ yě hěn hǎo xièxie lǎoshī", "Em cũng rất khỏe, cảm ơn thầy/cô.", new[] { "我", "也", "很好", "谢谢", "老师" }, new[] { "吗", "不", "再见" }),
                new DiItem("zh-l11-di-6", "hard", "Hẹn gặp ngày mai", "老师再见明天见", "lǎoshī zàijiàn míngtiān jiàn", "Em chào thầy/cô, hẹn gặp ngày mai!", new[] { "老师", "再见", "明天见" }, new[] { "你好", "今天", "请问" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l11-pr-1", "easy", "Biến điệu 2 thanh 3", "你好", "nǐ hǎo (đọc ní hǎo)", "Xin chào.", new[] { "你", "好" }, "Đọc 你 thành thanh 2 (ní) trước 好 thanh 3 (hǎo).", new[] { "Nâng giọng chữ 你 lên thanh 2", "Hạ sâu rồi vồng lên ở 好", "Giữ nhịp rõ tự nhiên" }, "你好！", new[] { "你好" }),
                new PrItem("zh-l11-pr-2", "easy", "Thanh 4 + Thanh nhẹ", "谢谢你", "xièxie nǐ", "Cảm ơn bạn.", new[] { "谢", "你" }, "Chữ 谢 đầu rơi dứt khoát thanh 4, chữ thứ 2 đọc thanh nhẹ.", new[] { "Thanh 4 rơi nhanh gọn ở xiè", "Chữ xie thứ 2 đọc ngắn nhẹ", "Đọc nǐ thanh 3 rõ" }, "谢谢你！", new[] { "谢谢" }),
                new PrItem("zh-l11-pr-3", "medium", "Âm uốn lưỡi 老师", "老师好", "lǎoshī hǎo", "Chào thầy/cô ạ.", new[] { "老", "师", "好" }, "Giữ âm uốn lưỡi sh thanh 1 cao phẳng trong 师.", new[] { "Đọc lǎo nửa thanh 3 trầm", "Uốn lưỡi shī cao ngang (55)", "Đọc hǎo thanh 3 rõ" }, "老师好！", new[] { "老师" }),
                new PrItem("zh-l11-pr-4", "medium", "Âm bật hơi 请问", "请问你好吗", "qǐngwèn nǐ hǎo ma", "Xin hỏi bạn có khỏe không?", new[] { "请", "问", "好" }, "Bật hơi mạnh âm q trong 请 (qǐng).", new[] { "Bật hơi mạnh ở qǐng", "Rơi dứt khoát thanh 4 wèn", "吗 đọc thanh nhẹ" }, "请问，你好吗？", new[] { "请问", "你好" }),
                new PrItem("zh-l11-pr-5", "hard", "Đáp lễ 不客气", "谢谢不客气", "xièxie bú kèqi", "Cảm ơn. Không có gì.", new[] { "谢", "不", "客", "气" }, "Biến điệu 不 thành bú + bật hơi mạnh k (kè) và q (qi).", new[] { "Đọc 不 thành bú (thanh 2)", "Bật hơi mạnh kè (thanh 4)", "气 đọc thanh nhẹ" }, "谢谢你！不客气。", new[] { "谢谢", "不客气" }),
                new PrItem("zh-l11-pr-6", "hard", "Xin lỗi & không sao", "对不起没关系", "duìbuqǐ méi guānxi", "Xin lỗi. Không sao đâu.", new[] { "对", "起", "关" }, "Bật hơi mạnh ở 起 (qǐ) và giữ thanh 1 cao phẳng ở 关 (guān).", new[] { "Đọc duì thanh 4 dứt khoát", "Bật hơi mạnh qǐ", "Giữ guān thanh 1 cao phẳng" }, "对不起！没关系。", new[] { "对不起", "没关系" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l11-wm-1", "easy", "Sơ đồ từ vựng: 好 (Tốt, khỏe)", "好", "hǎo", "Tốt, khỏe, được",
                    new[] { new WmNode("你好", "nǐ hǎo", "Xin chào", "你好，很高兴认识你！"), new WmNode("很好", "hěn hǎo", "Rất tốt / Rất khỏe", "我今天很好。") },
                    new[] { new WmNode("早上好", "zǎoshang hǎo", "Chào buổi sáng", "老师，早上好！"), new WmNode("好的", "hǎo de", "Vâng, được thôi", "好的，没问题！") },
                    new[] { new WmNode("不错", "búcuò", "Khá tốt, không tệ", "这个主意不错。") },
                    new[] { new WmEx("你今天好吗？我很好，谢谢！", "nǐ jīntiān hǎo ma? wǒ hěn hǎo, xièxie!", "Hôm nay bạn khỏe không? Tôi rất khỏe, cảm ơn!") }),
                new WmItem("zh-l11-wm-2", "medium", "Sơ đồ từ vựng: 谢谢 (Cảm ơn)", "谢谢", "xièxie", "Cảm ơn",
                    new[] { new WmNode("谢谢你", "xièxie nǐ", "Cảm ơn bạn", "谢谢你的帮助！"), new WmNode("非常感谢", "fēicháng gǎnxiè", "Vô cùng cảm ơn", "非常感谢您的支持。") },
                    new[] { new WmNode("不客气", "bú kèqi", "Không có gì (đáp lễ)", "不用谢，不客气！") },
                    new[] { new WmNode("多谢", "duōxiè", "Đa tạ, cảm ơn nhiều", "多谢提醒！") },
                    new[] { new WmEx("谢谢老师，明天见！", "xièxie lǎoshī, míngtiān jiàn!", "Cảm ơn thầy/cô, hẹn gặp ngày mai!") }),
                new WmItem("zh-l11-wm-3", "hard", "Sơ đồ từ vựng: 请 (Xin mời / Làm ơn)", "请", "qǐng", "Xin mời, vui lòng",
                    new[] { new WmNode("请问", "qǐngwèn", "Xin hỏi", "请问，洗手间在哪儿？"), new WmNode("请进 / 请坐", "qǐng jìn / qǐng zuò", "Mời vào / Mời ngồi", "请进，请坐！") },
                    new[] { new WmNode("请喝茶", "qǐng hē chá", "Mời uống trà", "欢迎光临，请喝茶。") },
                    new[] { new WmNode("麻烦你", "máfan nǐ", "Làm phiền bạn", "麻烦你帮我一下。") },
                    new[] { new WmEx("请问，您贵姓？", "qǐngwèn, nín guìxìng?", "Xin hỏi quý danh (họ) của ngài là gì?") })
            },
            cloze: new[]
            {
                new ClItem("zh-l11-cl-1", "easy", "Trợ từ hỏi 吗", "Điền trợ từ nghi vấn Có/Không vào cuối câu (Chữ Hán hoặc Pinyin):", "你好{{0}}？", new[] { new ClBlank(new[] { "吗", "ma" }, "không? (ma)", "Thêm 吗 cuối câu trần thuật để tạo câu hỏi Có/Không.") }, "你好吗？", "nǐ hǎo ma?", "Bạn có khỏe không?", "吗 luôn đọc thanh nhẹ."),
                new ClItem("zh-l11-cl-2", "easy", "Phó từ mức độ 很", "Điền phó từ đứng trước tính từ 好 (Chữ Hán hoặc Pinyin):", "我{{0}}好，谢谢！", new[] { new ClBlank(new[] { "很", "hěn", "hen3", "hen" }, "rất (hěn)", "Trước tính từ vị ngữ thường dùng 很 để câu tự nhiên.") }, "我很好，谢谢！", "wǒ hěn hǎo, xièxie!", "Tôi rất khỏe, cảm ơn!", "Không dùng 是 trước tính từ 好."),
                new ClItem("zh-l11-cl-3", "medium", "Hỏi ngược lại bằng 呢", "Điền trợ từ hỏi ngược lại 'Còn bạn thì sao?':", "我很好，你{{0}}？", new[] { new ClBlank(new[] { "呢", "ne" }, "thì sao? (ne)", "你呢？ = Còn bạn thì sao?") }, "我很好，你呢？", "wǒ hěn hǎo, nǐ ne?", "Tôi rất khỏe, còn bạn thì sao?", "Dùng 呢 để tránh lặp lại cả câu hỏi."),
                new ClItem("zh-l11-cl-4", "hard", "Đáp lời xin lỗi", "Điền cụm từ 'Không sao đâu' (Chữ Hán hoặc Pinyin có thanh điệu):", "对不起！—— {{0}}。", new[] { new ClBlank(new[] { "没关系", "méi guānxi", "mei2 guan1 xi" }, "không sao đâu (méi guānxi)", "Đáp lại 对不起 dùng 没关系 (khác với 不客气 dùng sau 谢谢).") }, "没关系。", "méi guānxi", "Không sao đâu.", "Câu này yêu cầu Hán tự hoặc Pinyin có thanh điệu chuẩn.")
            },
            translate: new[]
            {
                new TrItem("zh-l11-tr-1", "easy", "Xin chào! Bạn khỏe không?", "Xin chào! Bạn khỏe không?", new[] { "你好", "吗", "nǐ hǎo ma" }, new[] { "你好！你好吗？", "你好，你好吗？", "nǐ hǎo nǐ hǎo ma", "ni hao ni hao ma" }, "你好！你好吗？", "nǐ hǎo! nǐ hǎo ma?", "Bài dễ hỗ trợ Chữ Hán, Pinyin có dấu và Pinyin không dấu (kèm nhắc thanh điệu)."),
                new TrItem("zh-l11-tr-2", "easy", "Cảm ơn bạn", "Cảm ơn bạn! Không có gì.", new[] { "谢谢你", "不客气" }, new[] { "谢谢你！不客气。", "谢谢！不客气。", "xièxie nǐ bú kèqi", "xièxie nǐ bù kèqi", "xie xie ni bu ke qi" }, "谢谢你！不客气。", "xièxie nǐ! bú kèqi.", "不 trước thanh 4 đọc biến điệu thành bú."),
                new TrItem("zh-l11-tr-3", "medium", "Tôi cũng rất khỏe", "Tôi cũng rất khỏe, cảm ơn!", new[] { "我", "也", "很好", "谢谢" }, new[] { "我也很好，谢谢！", "我也很好，谢谢。", "wǒ yě hěn hǎo xièxie" }, "我也很好，谢谢！", "wǒ yě hěn hǎo, xièxie!", "Trật tự chuẩn: Chủ ngữ + 也 + 很 + Tính từ. (Yêu cầu Hán tự hoặc Pinyin có thanh điệu)."),
                new TrItem("zh-l11-tr-4", "hard", "Xin lỗi & không sao", "Xin lỗi! — Không sao đâu.", new[] { "对不起", "没关系" }, new[] { "对不起！没关系。", "对不起，没关系。", "duìbuqǐ méi guānxi" }, "对不起！没关系。", "duìbuqǐ! méi guānxi.", "Phân biệt 没关系 (đáp 对不起) với 不客气 (đáp 谢谢).")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l11-gf-1", "easy", "Dùng thừa 是 trước tính từ 好", "我是很好。", "Không dùng 是 trước tính từ vị ngữ 好.", "我很好。", new[] { "我很好。", "我很好" }, "wǒ hěn hǎo", "Tôi rất khỏe.", "Tiếng Trung dùng Chủ ngữ + 很 + Tính từ, không dùng 是."),
                new GfItem("zh-l11-gf-2", "medium", "Đặt nhầm vị trí 也", "也很我好。", "Phó từ 也 luôn đứng SAU chủ ngữ 我 và TRƯỚC 很.", "我也很好。", new[] { "我也很好。", "我也很好" }, "wǒ yě hěn hǎo", "Tôi cũng rất khỏe.", "Trật tự chuẩn: 我 + 也 + 很 + 好."),
                new GfItem("zh-l11-gf-3", "hard", "Dùng nhầm 不客气 sau 对不起", "对不起！—— 不客气。", "Đáp lại lời xin lỗi 对不起 phải dùng 没关系.", "对不起！—— 没关系。", new[] { "对不起！—— 没关系。", "对不起，没关系。", "没关系。" }, "duìbuqǐ! méi guānxi.", "Xin lỗi! — Không sao đâu.", "不客气 dùng để đáp lại 谢谢; 没关系 dùng để đáp lại 对不起.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l11-rp-1", "easy", "Chào hỏi buổi sáng", "Bạn gặp thầy giáo Vương ở cổng trường vào buổi sáng.", "Thầy Vương", "你好！你今天好吗？", "nǐ hǎo! nǐ jīntiān hǎo ma?", "Chào em! Hôm nay em có khỏe không?", "Chào thầy, nói bạn rất khỏe, cảm ơn thầy và hỏi ngược lại thầy (您呢 / 老师呢).", new[] { "老师好|你好|laoshi|ni hao", "很好|hen hao", "谢谢|xie xie", "呢|ne" }, new[] { "老师好！我很好，谢谢！您呢？" }, "lǎoshī hǎo! wǒ hěn hǎo, xièxie! nín ne?", 6),
                new RpItem("zh-l11-rp-2", "medium", "Lỡ va nhẹ vào bạn cùng lớp", "Bạn vô tình làm rơi quyển sách của bạn cùng bàn.", "Bạn Tiểu Minh", "哎呀，我的书掉了！", "āiyā, wǒ de shū diào le!", "Ôi, sách của mình rơi rồi!", "Xin lỗi bạn bằng 对不起 và xin phép hỏi thăm lịch sự.", new[] { "对不起|dui bu qi", "书|没关系|谢谢|不好意思" }, new[] { "对不起！我帮你拿书，真不好意思！" }, "duìbuqǐ! wǒ bāng nǐ ná shū, zhēn bù hǎoyìsi!", 6),
                new RpItem("zh-l11-rp-3", "hard", "Tạm biệt sau buổi học đầu", "Buổi học HSK1 đầu tiên kết thúc, bạn cùng lớp nói cảm ơn bạn đã giúp ghi chép.", "Bạn Lý Na", "谢谢你今天帮我！", "xièxie nǐ jīntiān bāng wǒ!", "Cảm ơn bạn hôm nay đã giúp mình nhé!", "Đáp lễ 'Không có gì' (不客气) và hẹn gặp lại ngày mai (明天见 / 再见).", new[] { "不客气|不用谢|bu ke qi", "明天见|再见|ming tian jian|zai jian" }, new[] { "不客气！明天见，再见！" }, "bú kèqi! míngtiān jiàn, zàijiàn!", 6)
            }, now));

        // ---------------- LESSON 1.2 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.ChineseLesson12Id, "zh-CN", "Trung HSK1 · Bài 1.2",
            shadowing: new[]
            {
                new ShItem("zh-l12-sh-1", "easy", "Hỏi tên (Dễ)", "你叫什么名字？", "nǐ jiào shénme míngzi?", "Bạn tên là gì?", new[] { "叫", "什么名字" }),
                new ShItem("zh-l12-sh-2", "easy", "Giới thiệu tên (Dễ)", "我叫明，很高兴认识你。", "wǒ jiào Míng, hěn gāoxìng rènshi nǐ", "Tôi tên là Minh, rất vui được quen bạn.", new[] { "我叫", "高兴", "认识" }),
                new ShItem("zh-l12-sh-3", "easy", "Quốc tịch Việt Nam (Dễ)", "我是越南人。", "wǒ shì Yuènán rén", "Tôi là người Việt Nam.", new[] { "我是", "越南人" }),
                new ShItem("zh-l12-sh-4", "medium", "Hỏi quốc tịch (Vừa)", "请问，你是哪国人？", "qǐngwèn, nǐ shì nǎ guó rén?", "Xin hỏi bạn là người nước nào?", new[] { "请问", "哪国人" }),
                new ShItem("zh-l12-sh-5", "medium", "Phủ định 不是 (Vừa)", "我不是老师，我是学生。", "wǒ bú shì lǎoshī, wǒ shì xuésheng", "Tôi không phải giáo viên, tôi là học sinh/sinh viên.", new[] { "不是", "学生" }),
                new ShItem("zh-l12-sh-6", "medium", "Đáp lễ làm quen (Vừa)", "认识你我也很高兴！", "rènshi nǐ wǒ yě hěn gāoxìng", "Quen bạn tôi cũng rất vui!", new[] { "认识你", "也很高兴" }),
                new ShItem("zh-l12-sh-7", "hard", "Biến điệu 一 (Khó)", "我有一个中国朋友。", "wǒ yǒu yí gè Zhōngguó péngyou", "Tôi có một người bạn Trung Quốc.", new[] { "一个", "中国朋友" }),
                new ShItem("zh-l12-sh-8", "hard", "Giới thiệu sinh viên (Khó)", "我是河内大学的大学生。", "wǒ shì Hénèi dàxué de dàxuéshēng", "Tôi là sinh viên Đại học Hà Nội.", new[] { "大学", "大学生" }),
                new ShItem("zh-l12-sh-9", "hard", "Hỏi người thứ ba (Khó)", "他是谁？他是我的汉语老师。", "tā shì shéi? tā shì wǒ de Hànyǔ lǎoshī", "Anh ấy là ai? Anh ấy là giáo viên tiếng Trung của tôi.", new[] { "是谁", "汉语老师" })
            },
            dictation: new[]
            {
                new DiItem("zh-l12-di-1", "easy", "Hỏi tên", "你叫什么名字", "nǐ jiào shénme míngzi", "Bạn tên là gì?", new[] { "你", "叫", "什么", "名字" }, new[] { "我", "是", "谁" }),
                new DiItem("zh-l12-di-2", "easy", "Quốc tịch", "我是越南人", "wǒ shì Yuènán rén", "Tôi là người Việt Nam.", new[] { "我", "是", "越南", "人" }, new[] { "中国", "不", "吗" }),
                new DiItem("zh-l12-di-3", "medium", "Rất vui quen bạn", "很高兴认识你", "hěn gāoxìng rènshi nǐ", "Rất vui được quen bạn.", new[] { "很", "高兴", "认识", "你" }, new[] { "我", "叫", "谢谢" }),
                new DiItem("zh-l12-di-4", "medium", "Phủ định", "我不是中国学生", "wǒ bú shì Zhōngguó xuésheng", "Tôi không phải sinh viên Trung Quốc.", new[] { "我", "不是", "中国", "学生" }, new[] { "老师", "越南", "也" }),
                new DiItem("zh-l12-di-5", "hard", "Hỏi quốc tịch", "请问你是哪国人", "qǐngwèn nǐ shì nǎ guó rén", "Xin hỏi bạn là người nước nào?", new[] { "请问", "你是", "哪国", "人" }, new[] { "什么", "吗", "我" }),
                new DiItem("zh-l12-di-6", "hard", "Thầy giáo tiếng Trung", "他是我的汉语老师", "tā shì wǒ de Hànyǔ lǎoshī", "Thầy ấy là giáo viên tiếng Trung của tôi.", new[] { "他是", "我的", "汉语", "老师" }, new[] { "学生", "朋友", "不是" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l12-pr-1", "easy", "Biến điệu 不是", "我不是老师", "wǒ bú shì lǎoshī", "Tôi không phải giáo viên.", new[] { "不", "是", "师" }, "Đọc 不 thành bú (thanh 2) trước 是 (thanh 4), uốn lưỡi shì & shī.", new[] { "Biến điệu bú thanh 2", "Uốn lưỡi shì thanh 4", "Giữ shī thanh 1 cao phẳng" }, "我不是老师，我是学生。", new[] { "不是", "老师" }),
                new PrItem("zh-l12-pr-2", "easy", "Uốn lưỡi 什么", "你叫什么名字", "nǐ jiào shénme míngzi", "Bạn tên là gì?", new[] { "叫", "什", "名" }, "Uốn lưỡi âm shén (thanh 2) và đọc nhẹ me, zi.", new[] { "Thanh 4 dứt khoát ở jiào", "Uốn lưỡi shén lên thanh 2", "Đọc nhẹ chữ 么 và 字" }, "请问，你叫什么名字？", new[] { "叫", "什么" }),
                new PrItem("zh-l12-pr-3", "medium", "Uốn lưỡi 认识", "很高兴认识你", "hěn gāoxìng rènshi nǐ", "Rất vui được quen bạn.", new[] { "高", "认", "识" }, "Giữ gāo thanh 1 cao ngang và uốn lưỡi nhẹ rènshi.", new[] { "gāo cao phẳng (55)", "rèn uốn lưỡi nhẹ, không rung mạnh", "识 đọc thanh nhẹ" }, "很高兴认识你！", new[] { "高兴", "认识" }),
                new PrItem("zh-l12-pr-4", "medium", "Uốn lưỡi 中国人", "我是中国人", "wǒ shì Zhōngguó rén", "Tôi là người Trung Quốc.", new[] { "是", "中", "人" }, "Cả 是, 中, 人 đều là nhóm âm uốn lưỡi zh/sh/r.", new[] { "zhōng uốn lưỡi + thanh 1 cao phẳng", "guó thanh 2 đi lên", "rén uốn lưỡi nhẹ" }, "我是中国人。", new[] { "中国", "人" }),
                new PrItem("zh-l12-pr-5", "hard", "Biến điệu 一个 & bật hơi 朋友", "我有一个朋友", "wǒ yǒu yí gè péngyou", "Tôi có một người bạn.", new[] { "一", "朋" }, "Đọc 一 thành yí (thanh 2) và bật hơi mạnh p trong 朋 (péng).", new[] { "Biến điệu yí thanh 2 trước gè", "Bật hơi mạnh péng", "友 đọc thanh nhẹ" }, "我有一个中国朋友。", new[] { "一个", "朋友" }),
                new PrItem("zh-l12-pr-6", "hard", "Hỏi quốc tịch", "请问你是哪国人", "qǐngwèn nǐ shì nǎ guó rén", "Xin hỏi bạn là người nước nào?", new[] { "请", "是", "哪", "人" }, "Kết hợp bật hơi q (qǐng) và uốn lưỡi sh/r.", new[] { "Bật hơi mạnh qǐng", "nǎ hạ trầm thanh 3", "rén uốn lưỡi lên thanh 2" }, "请问，你是哪国人？", new[] { "请问", "哪国人" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l12-wm-1", "easy", "Sơ đồ từ vựng: 叫 (Tên là / Gọi là)", "叫", "jiào", "Tên là, gọi",
                    new[] { new WmNode("你叫什么名字", "nǐ jiào shénme míngzi", "Bạn tên là gì?", "你好，你叫什么名字？"), new WmNode("我叫……", "wǒ jiào ...", "Tôi tên là...", "我叫阮明。") },
                    new[] { new WmNode("叫出租车", "jiào chūzūchē", "Gọi xe taxi", "我们叫一辆出租车吧。") },
                    new[] { new WmNode("姓", "xìng", "Mang họ", "我姓阮。") },
                    new[] { new WmEx("她叫李娜，是我的同学。", "tā jiào Lǐ Nà, shì wǒ de tóngxué", "Cô ấy tên là Lý Na, là bạn cùng lớp tôi.") }),
                new WmItem("zh-l12-wm-2", "medium", "Sơ đồ từ vựng: 认识 (Quen biết)", "认识", "rènshi", "Quen biết, nhận biết",
                    new[] { new WmNode("很高兴认识你", "hěn gāoxìng rènshi nǐ", "Rất vui được quen bạn", "你好，很高兴认识你！"), new WmNode("认识汉字", "rènshi Hànzì", "Biết mặt chữ Hán", "我认识两百个汉字。") },
                    new[] { new WmNode("认识很多朋友", "rènshi hěn duō péngyou", "Quen nhiều bạn bè", "我在大学认识了很多朋友。") },
                    new[] { new WmNode("知道", "zhīdao", "Biết (thông tin)", "我知道他的名字。") },
                    new[] { new WmEx("你认识王老师吗？", "nǐ rènshi Wáng lǎoshī ma?", "Bạn có quen thầy Vương không?") }),
                new WmItem("zh-l12-wm-3", "hard", "Sơ đồ từ vựng: 汉语 (Tiếng Trung)", "汉语", "Hànyǔ", "Tiếng Hán / Tiếng Trung",
                    new[] { new WmNode("学习汉语", "xuéxí Hànyǔ", "Học tiếng Trung", "我每天学习汉语。"), new WmNode("说汉语", "shuō Hànyǔ", "Nói tiếng Trung", "我会说一点儿汉语。") },
                    new[] { new WmNode("汉语老师", "Hànyǔ lǎoshī", "Giáo viên tiếng Trung", "李老师是我的汉语老师。") },
                    new[] { new WmNode("中文", "Zhōngwén", "Tiếng Hoa / Trung văn", "我喜欢看中文书。") },
                    new[] { new WmEx("她的汉语说得很好听。", "tā de Hànyǔ shuō de hěn hǎotīng", "Cô ấy nói tiếng Trung rất hay.") })
            },
            cloze: new[]
            {
                new ClItem("zh-l12-cl-1", "easy", "Hỏi tên", "Điền động từ 'tên là' và từ hỏi 'gì':", "你{{0}}{{1}}名字？", new[] { new ClBlank(new[] { "叫", "jiào", "jiao4", "jiao" }, "tên là (jiào)", "叫 = gọi tên là."), new ClBlank(new[] { "什么", "shénme", "shen2 me", "shenme" }, "cái gì (shénme)", "什么名字 = tên gì.") }, "你叫什么名字？", "nǐ jiào shénme míngzi?", "Bạn tên là gì?", "Không đảo ngữ trong câu hỏi tiếng Trung."),
                new ClItem("zh-l12-cl-2", "easy", "Câu chữ 是", "Điền động từ 'là' (Chữ Hán hoặc Pinyin):", "我{{0}}越南人。", new[] { new ClBlank(new[] { "是", "shì", "shi4", "shi" }, "là (shì)", "Chủ ngữ + 是 + Danh từ.") }, "我是越南人。", "wǒ shì Yuènán rén", "Tôi là người Việt Nam.", "是 nối hai danh từ với nhau."),
                new ClItem("zh-l12-cl-3", "medium", "Hỏi quốc tịch", "Điền từ hỏi 'nào' (nǎ):", "请问，你是{{0}}国人？", new[] { new ClBlank(new[] { "哪", "nǎ", "na3" }, "nào (nǎ - thanh 3)", "哪 (nǎ, có bộ khẩu) = nào; tránh nhầm 那 (nà = kia).") }, "请问，你是哪国人？", "qǐngwèn, nǐ shì nǎ guó rén?", "Xin hỏi bạn là người nước nào?", "Phân biệt 哪 (nǎ) và 那 (nà)."),
                new ClItem("zh-l12-cl-4", "hard", "Phủ định 不是", "Điền cụm 'không phải':", "他{{0}}学生，他是老师。", new[] { new ClBlank(new[] { "不是", "bú shì", "bù shì", "bu2 shi4", "bu4 shi4" }, "không phải (bú shì)", "Phủ định của 是 là 不是 (đọc biến điệu bú shì).") }, "他不是学生，他是老师。", "tā bú shì xuésheng, tā shì lǎoshī", "Anh ấy không phải học sinh, anh ấy là giáo viên.", "Nhớ quy tắc biến điệu 不 + thanh 4 → bú.")
            },
            translate: new[]
            {
                new TrItem("zh-l12-tr-1", "easy", "Giới thiệu quốc tịch", "Tôi là người Việt Nam.", new[] { "我", "是", "越南人" }, new[] { "我是越南人。", "我是越南人", "wǒ shì Yuènán rén", "wo shi yuenan ren" }, "我是越南人。", "wǒ shì Yuènán rén", "Mẫu câu cơ bản: 我是 + [Tên nước] + 人."),
                new TrItem("zh-l12-tr-2", "easy", "Hỏi tên", "Bạn tên là gì?", new[] { "你", "叫", "什么名字" }, new[] { "你叫什么名字？", "你叫什么名字", "nǐ jiào shénme míngzi", "ni jiao shenme mingzi" }, "你叫什么名字？", "nǐ jiào shénme míngzi?", "Chữ 什么 đặt ngay trước 名字."),
                new TrItem("zh-l12-tr-3", "medium", "Rất vui được quen bạn", "Rất vui được quen biết bạn!", new[] { "很", "高兴", "认识", "你" }, new[] { "很高兴认识你！", "很高兴认识你。", "hěn gāoxìng rènshi nǐ" }, "很高兴认识你！", "hěn gāoxìng rènshi nǐ!", "Câu giao tiếp mở đầu cực kỳ tự nhiên khi mới gặp."),
                new TrItem("zh-l12-tr-4", "hard", "Phủ định nghề nghiệp", "Tôi không phải giáo viên, tôi là sinh viên.", new[] { "不是", "老师", "学生" }, new[] { "我不是老师，我是学生。", "我不是老师，我是大学生。", "wǒ bú shì lǎoshī wǒ shì xuésheng" }, "我不是老师，我是学生。", "wǒ bú shì lǎoshī, wǒ shì xuésheng.", "Dùng 不是 để phủ định danh từ.")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l12-gf-1", "easy", "Nhầm 那 (kia) với 哪 (nào)", "你是那国人？", "Từ hỏi 'nước nào' là 哪 (có bộ khẩu 口).", "你是哪国人？", new[] { "你是哪国人？", "你是哪国人" }, "nǐ shì nǎ guó rén?", "Bạn là người nước nào?", "哪 (nǎ) dùng để hỏi; 那 (nà) là đại từ chỉ định 'kia/đó'."),
                new GfItem("zh-l12-gf-2", "medium", "Dùng thừa 吗 với đại từ nghi vấn", "你叫什么名字吗？", "Câu đã có từ hỏi 什么 thì KHÔNG thêm 吗 ở cuối.", "你叫什么名字？", new[] { "你叫什么名字？", "你叫什么名字" }, "nǐ jiào shénme míngzi?", "Bạn tên là gì?", "吗 chỉ dùng cho câu hỏi Có/Không, không đi với 什么/哪/谁."),
                new GfItem("zh-l12-gf-3", "hard", "Sai trật tự câu hỏi tên", "什么名字你叫？", "Tiếng Trung không đảo từ hỏi lên đầu câu.", "你叫什么名字？", new[] { "你叫什么名字？", "你叫什么名字" }, "nǐ jiào shénme míngzi?", "Bạn tên là gì?", "Giữ nguyên trật tự Chủ ngữ + Động từ + Tân ngữ.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l12-rp-1", "easy", "Giới thiệu tên khi gặp lần đầu", "Một bạn học người Trung Quốc hỏi tên bạn trong buổi khai giảng.", "Bạn Lý Na", "你好！我叫李娜。你叫什么名字？", "nǐ hǎo! wǒ jiào Lǐ Nà. nǐ jiào shénme míngzi?", "Chào bạn! Mình tên Lý Na. Bạn tên là gì?", "Chào lại, giới thiệu tên (我叫...) và nói rất vui được quen bạn (很高兴认识你).", new[] { "我叫|wo jiao", "高兴|认识|gao xing|ren shi" }, new[] { "你好！我叫明，很高兴认识你！" }, "nǐ hǎo! wǒ jiào Míng, hěn gāoxìng rènshi nǐ!", 6),
                new RpItem("zh-l12-rp-2", "medium", "Hỏi đáp quốc tịch", "Bạn mới quen hỏi bạn đến từ nước nào.", "Bạn Trương Vĩ", "请问，你是哪国人？你是大学生吗？", "qǐngwèn, nǐ shì nǎ guó rén? nǐ shì dàxuéshēng ma?", "Xin hỏi bạn là người nước nào? Bạn là sinh viên đại học à?", "Trả lời bạn là người Việt Nam (越南人) và xác nhận bạn là sinh viên/người đi làm.", new[] { "越南|yuenan", "是|shi", "学生|大学生|老师|工作" }, new[] { "我是越南人，我是大学生。你呢？" }, "wǒ shì Yuènán rén, wǒ shì dàxuéshēng. nǐ ne?", 8),
                new RpItem("zh-l12-rp-3", "hard", "Đính chính nhẹ nhàng", "Ai đó nhầm bạn là thầy giáo tiếng Trung.", "Khách tham quan", "请问，您是汉语老师吗？", "qǐngwèn, nín shì Hànyǔ lǎoshī ma?", "Xin hỏi ngài là giáo viên tiếng Trung phải không?", "Lịch sự phủ định bằng 不是 và nói bạn là học viên học tiếng Trung.", new[] { "不是|bu shi", "学生|学习|汉语|中文" }, new[] { "不好意思，我不是老师，我是学习汉语的学生。" }, "bù hǎoyìsi, wǒ bú shì lǎoshī, wǒ shì xuéxí Hànyǔ de xuésheng.", 10)
            }, now));

        // ---------------- LESSON 2.1 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.ChineseLesson21Id, "zh-CN", "Trung HSK1 · Bài 2.1",
            shadowing: new[]
            {
                new ShItem("zh-l21-sh-1", "easy", "Hỏi giờ (Dễ)", "现在几点？", "xiànzài jǐ diǎn?", "Bây giờ là mấy giờ?", new[] { "现在", "几点" }),
                new ShItem("zh-l21-sh-2", "easy", "Nói 2 giờ (Dễ)", "现在下午两点。", "xiànzài xiàwǔ liǎng diǎn", "Bây giờ là 2 giờ chiều.", new[] { "下午", "两点" }),
                new ShItem("zh-l21-sh-3", "easy", "Hỏi ngày tháng (Dễ)", "今天几月几号？", "jīntiān jǐ yuè jǐ hào?", "Hôm nay ngày mấy tháng mấy?", new[] { "今天", "几月几号" }),
                new ShItem("zh-l21-sh-4", "medium", "Nói thứ & ngày (Vừa)", "今天十月五号，星期三。", "jīntiān shí yuè wǔ hào, xīngqī sān", "Hôm nay mùng 5 tháng 10, thứ Tư.", new[] { "十月", "星期三" }),
                new ShItem("zh-l21-sh-5", "medium", "Giờ rưỡi sáng (Vừa)", "现在上午八点半。", "xiànzài shàngwǔ bā diǎn bàn", "Bây giờ là 8 giờ rưỡi sáng.", new[] { "上午", "八点半" }),
                new ShItem("zh-l21-sh-6", "medium", "Đặt lịch hẹn (Vừa)", "我们明天上午九点见。", "wǒmen míngtiān shàngwǔ jiǔ diǎn jiàn", "Chúng ta gặp nhau 9 giờ sáng mai nhé.", new[] { "明天", "九点见" }),
                new ShItem("zh-l21-sh-7", "hard", "Phân biệt 4 và 10 (Khó)", "现在十点四十分。", "xiànzài shí diǎn sìshí fēn", "Bây giờ là 10 giờ 40 phút.", new[] { "十点", "四十分" }),
                new ShItem("zh-l21-sh-8", "hard", "Hỏi sinh nhật (Khó)", "你的生日是几月几号？", "nǐ de shēngrì shì jǐ yuè jǐ hào?", "Sinh nhật của bạn là ngày mấy tháng mấy?", new[] { "生日", "几月几号" }),
                new ShItem("zh-l21-sh-9", "hard", "Lịch học cuối tuần (Khó)", "星期六下午两点我有汉语课。", "xīngqī liù xiàwǔ liǎng diǎn wǒ yǒu Hànyǔ kè", "2 giờ chiều thứ Bảy tôi có lớp tiếng Trung.", new[] { "星期六", "两点", "汉语课" })
            },
            dictation: new[]
            {
                new DiItem("zh-l21-di-1", "easy", "Hỏi giờ", "现在几点", "xiànzài jǐ diǎn", "Bây giờ mấy giờ?", new[] { "现在", "几", "点" }, new[] { "今天", "明天", "号" }),
                new DiItem("zh-l21-di-2", "easy", "2 giờ chiều", "下午两点", "xiàwǔ liǎng diǎn", "2 giờ chiều.", new[] { "下午", "两", "点" }, new[] { "上午", "二", "分" }),
                new DiItem("zh-l21-di-3", "medium", "Hỏi thứ mấy", "今天星期几", "jīntiān xīngqī jǐ", "Hôm nay thứ mấy?", new[] { "今天", "星期", "几" }, new[] { "明天", "点", "月" }),
                new DiItem("zh-l21-di-4", "medium", "8 giờ rưỡi sáng", "现在上午八点半", "xiànzài shàngwǔ bā diǎn bàn", "Bây giờ là 8 giờ rưỡi sáng.", new[] { "现在", "上午", "八点", "半" }, new[] { "下午", "分", "九点" }),
                new DiItem("zh-l21-di-5", "hard", "Hẹn ngày mai", "明天下午三点见", "míngtiān xiàwǔ sān diǎn jiàn", "3 giờ chiều mai gặp nhé.", new[] { "明天", "下午", "三点", "见" }, new[] { "今天", "上午", "二" }),
                new DiItem("zh-l21-di-6", "hard", "Ngày tháng & thứ", "今天五月十号星期五", "jīntiān wǔ yuè shí hào xīngqī wǔ", "Hôm nay mùng 10 tháng 5, thứ Sáu.", new[] { "今天", "五月", "十号", "星期五" }, new[] { "明天", "四号", "点" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l21-pr-1", "easy", "Phân biệt 四 (sì) vs 十 (shí)", "四点十分", "sì diǎn shí fēn", "4 giờ 10 phút.", new[] { "四", "十" }, "四 đầu lưỡi thẳng thanh 4; 十 uốn lưỡi thanh 2.", new[] { "sì không uốn lưỡi, rơi nhanh", "shí uốn lưỡi, lên thanh 2", "fēn thanh 1 cao phẳng" }, "现在四点十分。", new[] { "四", "十" }),
                new PrItem("zh-l21-pr-2", "easy", "Bật hơi 七 (qī)", "七月七号", "qī yuè qī hào", "Ngày 7 tháng 7.", new[] { "七", "月", "号" }, "Bật hơi mạnh q và giữ thanh 1 thật cao phẳng.", new[] { "qī bật hơi cao ngang", "yuè và hào rơi gọn thanh 4", "Đọc nhịp đều" }, "今天七月七号。", new[] { "七" }),
                new PrItem("zh-l21-pr-3", "medium", "Biến điệu 两点 (liǎng diǎn)", "下午两点半", "xiàwǔ liǎng diǎn bàn", "2 giờ rưỡi chiều.", new[] { "两", "点", "半" }, "两 và 点 đều thanh 3 nên 两 đọc biến điệu thành liáng.", new[] { "Biến điệu liáng (thanh 2)", "diǎn đọc nửa thanh 3 trầm", "bàn thanh 4 dứt khoát" }, "现在下午两点半。", new[] { "两点", "半" }),
                new PrItem("zh-l21-pr-4", "medium", "Thanh 1 + bật hơi 星期", "今天星期三", "jīntiān xīngqī sān", "Hôm nay thứ Tư.", new[] { "今", "天", "期", "三" }, "Giữ liền 4 âm thanh 1 cao phẳng và bật hơi q (qī).", new[] { "jīntiān cả hai thanh 1 cao đều", "Bật hơi qī cao ngang", "sān đầu lưỡi thẳng thanh 1" }, "今天星期三。", new[] { "星期" }),
                new PrItem("zh-l21-pr-5", "hard", "Uốn lưỡi 生日", "你的生日是十月", "nǐ de shēngrì shì shí yuè", "Sinh nhật bạn là tháng 10.", new[] { "生", "日", "是", "十" }, "Chú ý 4 âm uốn lưỡi liên tiếp shēng, rì, shì, shí.", new[] { "shēng uốn lưỡi thanh 1 cao", "rì uốn lưỡi thanh 4 gọn", "Phân biệt shì (4) và shí (2)" }, "我的生日是十月十号。", new[] { "生日", "十月" }),
                new PrItem("zh-l21-pr-6", "hard", "Hẹn giờ chi tiết", "我们明天上午九点十五分见", "wǒmen míngtiān shàngwǔ jiǔ diǎn shíwǔ fēn jiàn", "Sáng mai 9 giờ 15 phút chúng ta gặp nhau.", new[] { "天", "上", "十", "分" }, "Đọc trôi chảy cụm thời gian lớn → nhỏ.", new[] { "míngtiān rõ thanh 2-1", "jiǔ biến điệu thanh 2 trước diǎn", "shí uốn lưỡi thanh 2" }, "我们明天上午九点十五分见。", new[] { "明天", "九点" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l21-wm-1", "easy", "Sơ đồ từ vựng: 点 (Giờ)", "点", "diǎn", "Giờ đồng hồ / Gọi món",
                    new[] { new WmNode("几点", "jǐ diǎn", "Mấy giờ", "现在几点了？"), new WmNode("两点半", "liǎng diǎn bàn", "2 giờ rưỡi", "我们两点半出发。") },
                    new[] { new WmNode("一点儿", "yìdiǎnr", "Một chút", "我会说一点儿汉语。") },
                    new[] { new WmNode("小时", "xiǎoshí", "Tiếng đồng hồ (khoảng)", "我学习一个小时。") },
                    new[] { new WmEx("电影晚上七点开始。", "diànyǐng wǎnshang qī diǎn kāishǐ", "Bộ phim bắt đầu lúc 7 giờ tối.") }),
                new WmItem("zh-l21-wm-2", "medium", "Sơ đồ từ vựng: 星期 (Tuần / Thứ)", "星期", "xīngqī", "Tuần lễ, thứ",
                    new[] { new WmNode("星期几", "xīngqī jǐ", "Thứ mấy", "今天星期几？"), new WmNode("星期天", "xīngqī tiān", "Chủ nhật", "星期天我不工作。") },
                    new[] { new WmNode("下个星期", "xià ge xīngqī", "Tuần sau", "下个星期我去上海。") },
                    new[] { new WmNode("周末", "zhōumò", "Cuối tuần", "周末你做什么？") },
                    new[] { new WmEx("每个星期三我都有中文课。", "měi ge xīngqī sān wǒ dōu yǒu Zhōngwén kè", "Mỗi thứ Tư tôi đều có lớp tiếng Trung.") }),
                new WmItem("zh-l21-wm-3", "hard", "Sơ đồ từ vựng: 时候 (Khi, lúc)", "时候", "shíhou", "Thời điểm, lúc",
                    new[] { new WmNode("什么时候", "shénme shíhou", "Khi nào, bao giờ", "你什么时候去中国？"), new WmNode("小的时候", "xiǎo de shíhou", "Khi còn nhỏ", "我小的时候住在河内。") },
                    new[] { new WmNode("……的时候", "... de shíhou", "Vào lúc...", "吃饭的时候别看手机。") },
                    new[] { new WmNode("时间", "shíjiān", "Thời gian", "你明天有时间吗？") },
                    new[] { new WmEx("你什么时候有时间？", "nǐ shénme shíhou yǒu shíjiān?", "Khi nào bạn có thời gian?") })
            },
            cloze: new[]
            {
                new ClItem("zh-l21-cl-1", "easy", "Hỏi mấy giờ", "Điền từ hỏi 'mấy' và 'giờ':", "现在{{0}}{{1}}？", new[] { new ClBlank(new[] { "几", "jǐ", "ji3", "ji" }, "mấy (jǐ)", "几 dùng hỏi số nhỏ hơn 10 và hỏi giờ."), new ClBlank(new[] { "点", "diǎn", "dian3", "dian" }, "giờ (diǎn)", "几点 = mấy giờ.") }, "现在几点？", "xiànzài jǐ diǎn?", "Bây giờ là mấy giờ?", "Mẫu hỏi giờ chuẩn HSK1."),
                new ClItem("zh-l21-cl-2", "easy", "Dùng 两 cho 2 giờ", "Điền chữ số '2' đúng trước 点 (giờ):", "现在下午{{0}}点半。", new[] { new ClBlank(new[] { "两", "liǎng", "liang3" }, "2 giờ dùng 两 (liǎng)", "Trước 点 luôn dùng 两 (两点 = 2 giờ), KHÔNG dùng 二!") }, "现在下午两点半。", "xiànzài xiàwǔ liǎng diǎn bàn", "Bây giờ là 2 giờ rưỡi chiều.", "Quy tắc bắt buộc của tiếng Trung: 两点."),
                new ClItem("zh-l21-cl-3", "medium", "Ngày tháng năm", "Điền chữ 'tháng' và 'ngày':", "今天十{{0}}五{{1}}。", new[] { new ClBlank(new[] { "月", "yuè", "yue4" }, "tháng (yuè)", "Số + 月 = tháng."), new ClBlank(new[] { "号", "日", "hào", "rì", "hao4", "ri4" }, "ngày (hào / rì)", "Khẩu ngữ dùng 号, văn viết dùng 日.") }, "今天十月五号。", "jīntiān shí yuè wǔ hào", "Hôm nay ngày 5 tháng 10.", "Tháng luôn đứng trước ngày."),
                new ClItem("zh-l21-cl-4", "hard", "Thứ Bảy", "Điền số đúng cho 'Thứ Bảy' trong tiếng Trung:", "明天是星期{{0}}。", new[] { new ClBlank(new[] { "六", "liù", "liu4" }, "thứ Bảy là 星期六 (số 6)", "星期一 = Thứ 2 → 星期六 = Thứ 7.") }, "明天是星期六。", "míngtiān shì xīngqī liù", "Ngày mai là thứ Bảy.", "Cẩn thận lệch 1 đơn vị so với cách gọi thứ tiếng Việt!")
            },
            translate: new[]
            {
                new TrItem("zh-l21-tr-1", "easy", "Hỏi giờ", "Bây giờ là mấy giờ?", new[] { "现在", "几点" }, new[] { "现在几点？", "现在几点了？", "xiànzài jǐ diǎn", "xianzai ji dian" }, "现在几点？", "xiànzài jǐ diǎn?", "Thêm 了 (xiànzài jǐ diǎn le?) càng tự nhiên."),
                new TrItem("zh-l21-tr-2", "easy", "2 giờ chiều", "Bây giờ là 2 giờ chiều.", new[] { "现在", "下午", "两点" }, new[] { "现在下午两点。", "现在是下午两点。", "xiànzài xiàwǔ liǎng diǎn", "xianzai xiawu liang dian" }, "现在下午两点。", "xiànzài xiàwǔ liǎng diǎn.", "2 giờ bắt buộc dùng 两点 (không dùng 二点)."),
                new TrItem("zh-l21-tr-3", "medium", "Ngày 8 tháng 5", "Hôm nay là ngày 8 tháng 5.", new[] { "今天", "五月", "八号" }, new[] { "今天五月八号。", "今天是五月八号。", "今天五月八日。", "jīntiān wǔ yuè bā hào" }, "今天五月八号。", "jīntiān wǔ yuè bā hào.", "Trật tự chuẩn: Tháng trước, Ngày sau."),
                new TrItem("zh-l21-tr-4", "hard", "Hẹn 9 giờ sáng mai", "9 giờ sáng mai chúng ta gặp nhau nhé!", new[] { "明天", "上午", "九点", "见" }, new[] { "我们明天上午九点见！", "明天上午九点见！", "我们明天上午九点见。", "wǒmen míngtiān shàngwǔ jiǔ diǎn jiàn" }, "我们明天上午九点见！", "wǒmen míngtiān shàngwǔ jiǔ diǎn jiàn!", "Trạng ngữ thời gian đặt trước động từ 见.")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l21-gf-1", "easy", "Dùng nhầm 二点 thay vì 两点", "现在二点半。", "Nói 2 giờ phải dùng 两点 (không dùng 二点).", "现在两点半。", new[] { "现在两点半。", "现在两点半" }, "xiànzài liǎng diǎn bàn", "Bây giờ là 2 giờ rưỡi.", "Trước 点 luôn dùng 两."),
                new GfItem("zh-l21-gf-2", "medium", "Ngược trật tự Ngày → Tháng", "今天十号五月。", "Tiếng Trung nói Tháng trước, Ngày sau.", "今天五月十号。", new[] { "今天五月十号。", "今天是五月十号。", "今天五月十日。" }, "jīntiān wǔ yuè shí hào", "Hôm nay ngày 10 tháng 5.", "Trật tự thời gian luôn từ đơn vị lớn đến nhỏ."),
                new GfItem("zh-l21-gf-3", "hard", "Đặt thời gian sau động từ", "我们见明天下午三点。", "Thời gian đặt TRƯỚC động từ 见.", "我们明天下午三点见。", new[] { "我们明天下午三点见。", "明天下午三点我们见。" }, "wǒmen míngtiān xiàwǔ sān diǎn jiàn", "3 giờ chiều mai chúng ta gặp nhau.", "Chủ ngữ + Thời gian + Động từ.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l21-rp-1", "easy", "Hỏi giờ trên lớp", "Bạn cùng bàn quên đồng hồ và quay sang hỏi giờ bạn.", "Bạn Tiểu Hoa", "请问，现在几点了？", "qǐngwèn, xiànzài jǐ diǎn le?", "Xin hỏi bây giờ mấy giờ rồi?", "Trả lời bây giờ là 9 giờ rưỡi sáng (上午九点半) hoặc 2 giờ chiều (下午两点).", new[] { "现在|xian zai", "点|dian", "上午|下午|半|分" }, new[] { "现在上午九点半了。" }, "xiànzài shàngwǔ jiǔ diǎn bàn le.", 6),
                new RpItem("zh-l21-rp-2", "medium", "Xác nhận ngày & thứ", "Đồng nghiệp hỏi lịch hôm nay ngày mấy, thứ mấy.", "Anh Vương", "今天几月几号？星期几？", "jīntiān jǐ yuè jǐ hào? xīngqī jǐ?", "Hôm nay ngày mấy tháng mấy? Thứ mấy?", "Nói rõ tháng (月), ngày (号) và thứ (星期...) đúng trật tự lớn → nhỏ.", new[] { "月|yue", "号|日|hao", "星期|xing qi" }, new[] { "今天十月十二号，星期三。" }, "jīntiān shí yuè shí'èr hào, xīngqī sān.", 6),
                new RpItem("zh-l21-rp-3", "hard", "Chốt lịch hẹn chiều mai", "Bạn hẹn đồng nghiệp gặp nhau vào chiều mai.", "Chị Lý", "你明天有时间吗？我们几点见？", "nǐ míngtiān yǒu shíjiān ma? wǒmen jǐ diǎn jiàn?", "Ngày mai bạn có thời gian không? Mấy giờ ta gặp nhau?", "Nói bạn có thời gian (有时间) và hẹn gặp lúc 2 giờ chiều mai (明天下午两点见).", new[] { "有时间|有|you shi jian", "两点|三点|点|dian", "见|jian" }, new[] { "我有时间！我们明天下午两点见，好吗？" }, "wǒ yǒu shíjiān! wǒmen míngtiān xiàwǔ liǎng diǎn jiàn, hǎo ma?", 8)
            }, now));

        // ---------------- LESSON 2.2 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.ChineseLesson22Id, "zh-CN", "Trung HSK1 · Bài 2.2",
            shadowing: new[]
            {
                new ShItem("zh-l22-sh-1", "easy", "Nhân khẩu gia đình (Dễ)", "我家有四口人。", "wǒ jiā yǒu sì kǒu rén", "Nhà tôi có 4 người.", new[] { "我家", "四口人" }),
                new ShItem("zh-l22-sh-2", "easy", "Hỏi giá cơ bản (Dễ)", "这个多少钱？", "zhège duōshao qián?", "Cái này bao nhiêu tiền?", new[] { "这个", "多少钱" }),
                new ShItem("zh-l22-sh-3", "easy", "Phủ định 没有 (Dễ)", "我没有哥哥。", "wǒ méiyǒu gēge", "Tôi không có anh trai.", new[] { "没有", "哥哥" }),
                new ShItem("zh-l22-sh-4", "medium", "Cảm thán đắt quá (Vừa)", "这个苹果太贵了！", "zhège píngguǒ tài guì le", "Quả táo này đắt quá!", new[] { "苹果", "太贵了" }),
                new ShItem("zh-l22-sh-5", "medium", "Mặc cả nhẹ (Vừa)", "可以便宜一点儿吗？", "kěyǐ piányi yìdiǎnr ma?", "Có thể rẻ hơn một chút không?", new[] { "便宜", "一点儿" }),
                new ShItem("zh-l22-sh-6", "medium", "Thành viên gia đình (Vừa)", "这是我的爸爸和妈妈。", "zhè shì wǒ de bàba hé māma", "Đây là bố và mẹ của tôi.", new[] { "爸爸", "妈妈" }),
                new ShItem("zh-l22-sh-7", "hard", "Mua 2 cân táo (Khó)", "我要买两斤苹果，多少钱？", "wǒ yào mǎi liǎng jīn píngguǒ, duōshao qián?", "Tôi muốn mua 2 cân táo, bao nhiêu tiền?", new[] { "买", "两斤", "多少钱" }),
                new ShItem("zh-l22-sh-8", "hard", "Báo tổng tiền (Khó)", "一共二十五块钱。", "yígòng èrshíwǔ kuài qián", "Tổng cộng là 25 tệ.", new[] { "一共", "块钱" }),
                new ShItem("zh-l22-sh-9", "hard", "Hỏi gia đình đối phương (Khó)", "你家有几口人？你有姐姐或者妹妹吗？", "nǐ jiā yǒu jǐ kǒu rén? nǐ yǒu jiějie huòzhě mèimei ma?", "Nhà bạn có mấy người? Bạn có chị gái hay em gái không?", new[] { "几口人", "姐姐", "妹妹" })
            },
            dictation: new[]
            {
                new DiItem("zh-l22-di-1", "easy", "Gia đình 4 người", "我家有四口人", "wǒ jiā yǒu sì kǒu rén", "Nhà tôi có 4 người.", new[] { "我家", "有", "四口", "人" }, new[] { "个", "没有", "两" }),
                new DiItem("zh-l22-di-2", "easy", "Hỏi giá", "这个多少钱", "zhège duōshao qián", "Cái này bao nhiêu tiền?", new[] { "这个", "多少", "钱" }, new[] { "那个", "几", "块" }),
                new DiItem("zh-l22-di-3", "medium", "Không có em trai", "我没有弟弟", "wǒ méiyǒu dìdi", "Tôi không có em trai.", new[] { "我", "没有", "弟弟" }, new[] { "不", "哥哥", "有" }),
                new DiItem("zh-l22-di-4", "medium", "Đắt quá", "太贵了便宜一点儿", "tài guì le piányi yìdiǎnr", "Đắt quá, rẻ hơn một chút đi.", new[] { "太贵了", "便宜", "一点儿" }, new[] { "好", "多少", "块" }),
                new DiItem("zh-l22-di-5", "hard", "Mua 3 quả táo", "我要买三个苹果", "wǒ yào mǎi sān gè píngguǒ", "Tôi muốn mua 3 quả táo.", new[] { "我要", "买", "三个", "苹果" }, new[] { "卖", "口", "本" }),
                new DiItem("zh-l22-di-6", "hard", "Tổng 20 tệ", "一共二十块钱给你", "yígòng èrshí kuài qián gěi nǐ", "Tổng cộng 20 tệ, gửi bạn này.", new[] { "一共", "二十", "块钱", "给你" }, new[] { "两", "多少", "太" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l22-pr-1", "easy", "Phân biệt 买 (mǎi) vs 卖 (mài)", "我买苹果他卖苹果", "wǒ mǎi píngguǒ tā mài píngguǒ", "Tôi mua táo, anh ấy bán táo.", new[] { "买", "苹", "卖" }, "买 thanh 3 xuống-lên khác 卖 thanh 4 rơi mạnh; 苹 bật hơi mạnh p.", new[] { "mǎi hạ sâu thanh 3", "píng bật hơi mạnh", "mài thanh 4 rơi nhanh dứt khoát" }, "我要买苹果。", new[] { "买", "苹果" }),
                new PrItem("zh-l22-pr-2", "easy", "Bật hơi 钱 & uốn lưỡi 少", "这个多少钱", "zhège duōshao qián", "Cái này bao nhiêu tiền?", new[] { "这", "少", "钱" }, "Uốn lưỡi zhè & shao, bật hơi mạnh qián (thanh 2).", new[] { "zhè uốn lưỡi thanh 4", "duō thanh 1 cao phẳng", "qián bật hơi lên thanh 2" }, "请问，这个多少钱？", new[] { "多少", "钱" }),
                new PrItem("zh-l22-pr-3", "medium", "Bật hơi 口 (kǒu)", "我家有四口人", "wǒ jiā yǒu sì kǒu rén", "Nhà tôi có 4 người.", new[] { "口", "人" }, "Bật hơi mạnh k (kǒu) và uốn lưỡi nhẹ r (rén).", new[] { "jiā thanh 1 cao phẳng", "kǒu bật hơi rõ thanh 3", "rén uốn lưỡi lên thanh 2" }, "我家有四口人。", new[] { "口", "人" }),
                new PrItem("zh-l22-pr-4", "medium", "Bật hơi 太 & 便宜", "太贵了便宜一点儿", "tài guì le piányi yìdiǎnr", "Đắt quá, rẻ hơn chút đi.", new[] { "太", "便" }, "Bật hơi mạnh cả t (tài) và p (piányi).", new[] { "tài bật hơi thanh 4 gọn", "pián bật hơi thanh 2 đi lên", "Đọc cong lưỡi nhẹ ở 儿 (r)" }, "太贵了，便宜一点儿吧。", new[] { "太贵", "便宜" }),
                new PrItem("zh-l22-pr-5", "hard", "Bật hơi 块 (kuài)", "一共十五块钱", "yígòng shíwǔ kuài qián", "Tổng cộng 15 tệ.", new[] { "十", "块", "钱" }, "Kết hợp uốn lưỡi shí, bật hơi kuài và qián.", new[] { "Biến điệu yí thanh 2 trước gòng", "shí uốn lưỡi thanh 2", "Bật hơi mạnh cả kuài và qián" }, "一共十五块钱。", new[] { "十五", "块钱" }),
                new PrItem("zh-l22-pr-6", "hard", "Thanh 1 哥哥 & 妈妈", "我有妈妈和哥哥", "wǒ yǒu māma hé gēge", "Tôi có mẹ và anh trai.", new[] { "妈", "哥" }, "Giữ thanh 1 thật cao phẳng ở chữ đầu, chữ thứ hai thanh nhẹ.", new[] { "mā cao ngang + ma nhẹ", "gē cao ngang + ge nhẹ", "Không hạ giọng âm thanh 1" }, "我有爸爸、妈妈和一个哥哥。", new[] { "妈妈", "哥哥" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l22-wm-1", "easy", "Sơ đồ từ vựng: 家 (Nhà, gia đình)", "家", "jiā", "Nhà, gia đình",
                    new[] { new WmNode("我家有四口人", "wǒ jiā yǒu sì kǒu rén", "Nhà tôi có 4 người", "我家有四口人。"), new WmNode("回家", "huí jiā", "Về nhà", "我下午五点回家。") },
                    new[] { new WmNode("在家", "zài jiā", "Ở nhà", "周末我在家看书。") },
                    new WmNode[] { new WmNode("家人", "jiārén", "Người thân trong nhà", "我很爱我的家人。") },
                    new[] { new WmEx("你家在河内吗？", "nǐ jiā zài Hénèi ma?", "Nhà bạn có ở Hà Nội không?") }),
                new WmItem("zh-l22-wm-2", "medium", "Sơ đồ từ vựng: 买 (Mua)", "买", "mǎi", "Mua sắm",
                    new[] { new WmNode("买东西", "mǎi dōngxi", "Đi mua đồ", "我去商店买东西。"), new WmNode("买水果", "mǎi shuǐguǒ", "Mua hoa quả", "妈妈买了很多水果。") },
                    new[] { new WmNode("买衣服", "mǎi yīfu", "Mua quần áo", "我想买一件新衣服。") },
                    new[] { new WmNode("卖 (trái nghĩa)", "mài", "Bán hàng", "这个商店卖中文书。") },
                    new[] { new WmEx("我想买两斤苹果和一杯茶。", "wǒ xiǎng mǎi liǎng jīn píngguǒ hé yì bēi chá", "Tôi muốn mua 2 cân táo và 1 tách trà.") }),
                new WmItem("zh-l22-wm-3", "hard", "Sơ đồ từ vựng: 钱 (Tiền)", "钱", "qián", "Tiền bạc",
                    new[] { new WmNode("多少钱", "duōshao qián", "Bao nhiêu tiền", "这本书多少钱？"), new WmNode("十块钱", "shí kuài qián", "10 tệ", "一共十块钱。") },
                    new[] { new WmNode("付钱 / 找钱", "fù qián / zhǎo qián", "Trả tiền / Thối lại tiền", "请问在哪儿付钱？") },
                    new[] { new WmNode("价格", "jiàgé", "Giá cả", "价格很便宜。") },
                    new[] { new WmEx("这些水果一共三十块钱。", "zhèxiē shuǐguǒ yígòng sānshí kuài qián", "Chỗ hoa quả này tổng cộng 30 tệ.") })
            },
            cloze: new[]
            {
                new ClItem("zh-l22-cl-1", "easy", "Lượng từ nhân khẩu 口", "Điền lượng từ đúng cho thành viên gia đình:", "我家有四{{0}}人。", new[] { new ClBlank(new[] { "口", "kǒu", "kou3", "kou" }, "lượng từ cho nhân khẩu (kǒu)", "Hỏi/nói số người trong gia đình dùng lượng từ 口.") }, "我家有四口人。", "wǒ jiā yǒu sì kǒu rén", "Nhà tôi có 4 người.", "Số từ + Lượng từ + Danh từ."),
                new ClItem("zh-l22-cl-2", "easy", "Phủ định của 有 là 没有", "Điền phó từ phủ định đúng trước 有:", "我{{0}}有哥哥。", new[] { new ClBlank(new[] { "没", "méi", "mei2", "mei" }, "phủ định của 有 (méi)", "Phủ định của 有 luôn là 没有 (tuyệt đối không dùng 不有!).") }, "我没有哥哥。", "wǒ méiyǒu gēge", "Tôi không có anh trai.", "Nhớ quy tắc bất di bất dịch: 没有."),
                new ClItem("zh-l22-cl-3", "medium", "Hỏi giá tiền", "Điền cụm từ hỏi 'bao nhiêu tiền':", "这个苹果{{0}}？", new[] { new ClBlank(new[] { "多少钱", "duōshao qián", "duo1 shao qian2" }, "bao nhiêu tiền (duōshao qián)", "Mẫu hỏi giá chuẩn: [Đồ vật] + 多少钱？") }, "这个苹果多少钱？", "zhège píngguǒ duōshao qián?", "Quả táo này bao nhiêu tiền?", "Không cần thêm 吗 khi đã có 多少."),
                new ClItem("zh-l22-cl-4", "hard", "Cấu trúc cảm thán 太……了", "Hoàn thành câu 'Đắt quá! Rẻ hơn chút đi':", "{{0}}贵{{1}}！便宜一点儿吧。", new[] { new ClBlank(new[] { "太", "tài", "tai4" }, "quá (tài)", "太 + Tính từ + 了 = quá... rồi!"), new ClBlank(new[] { "了", "le" }, "rồi (le)", "Cặp cấu trúc cố định 太……了.") }, "太贵了！便宜一点儿吧。", "tài guì le! piányi yìdiǎnr ba.", "Đắt quá! Rẻ hơn chút đi.", "Cấu trúc cảm thán cực hay dùng khi đi mua sắm.")
            },
            translate: new[]
            {
                new TrItem("zh-l22-tr-1", "easy", "Nhà tôi có 4 người", "Nhà tôi có 4 người.", new[] { "我家", "有", "四口人" }, new[] { "我家有四口人。", "我家有四口人", "wǒ jiā yǒu sì kǒu rén", "wo jia you si kou ren" }, "我家有四口人。", "wǒ jiā yǒu sì kǒu rén.", "Dùng lượng từ 口 cho số thành viên trong gia đình."),
                new TrItem("zh-l22-tr-2", "easy", "Hỏi giá", "Cái này bao nhiêu tiền?", new[] { "这个", "多少钱" }, new[] { "这个多少钱？", "这个多少钱", "zhège duōshao qián", "zhege duoshao qian" }, "这个多少钱？", "zhège duōshao qián?", "Câu hỏi giá cơ bản nhất khi mua sắm."),
                new TrItem("zh-l22-tr-3", "medium", "Tôi không có chị gái", "Tôi không có chị gái.", new[] { "我", "没有", "姐姐" }, new[] { "我没有姐姐。", "我没有姐姐", "wǒ méiyǒu jiějie" }, "我没有姐姐。", "wǒ méiyǒu jiějie.", "Phủ định của 有 luôn là 没有."),
                new TrItem("zh-l22-tr-4", "hard", "Đắt quá, rẻ hơn chút đi", "Đắt quá! Có thể rẻ hơn một chút không?", new[] { "太贵了", "可以", "便宜一点儿", "吗" }, new[] { "太贵了！可以便宜一点儿吗？", "太贵了，可以便宜一点儿吗？", "tài guì le kěyǐ piányi yìdiǎnr ma" }, "太贵了！可以便宜一点儿吗？", "tài guì le! kěyǐ piányi yìdiǎnr ma?", "Cấu trúc mặc cả lịch sự bằng 可以……吗？.")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l22-gf-1", "easy", "Sai phủ định 不有", "我不有妹妹。", "Phủ định của động từ 有 bắt buộc dùng 没有.", "我没有妹妹。", new[] { "我没有妹妹。", "我没有妹妹" }, "wǒ méiyǒu mèimei", "Tôi không có em gái.", "Trong tiếng Trung KHÔNG TỒN TẠI cách nói 不有."),
                new GfItem("zh-l22-gf-2", "medium", "Thiếu lượng từ giữa số và danh từ", "我要买三苹果。", "Giữa số từ 三 và danh từ 苹果 bắt buộc có lượng từ 个.", "我要买三个苹果。", new[] { "我要买三个苹果。", "我要买三个苹果" }, "wǒ yào mǎi sān gè píngguǒ", "Tôi muốn mua 3 quả táo.", "Công thức bắt buộc: Số từ + Lượng từ + Danh từ."),
                new GfItem("zh-l22-gf-3", "hard", "Dùng nhầm 二 trước lượng từ 斤", "我买二斤苹果。", "Trước lượng từ 斤 phải dùng 两 (không dùng 二).", "我买两斤苹果。", new[] { "我买两斤苹果。", "我买两斤苹果" }, "wǒ mǎi liǎng jīn píngguǒ", "Tôi mua 2 cân táo.", "Trước mọi lượng từ (个, 斤, 本, 杯) luôn dùng 两.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l22-rp-1", "easy", "Giới thiệu gia đình", "Bạn học hỏi về gia đình bạn trong giờ nói HSK1.", "Bạn Tiểu Lan", "你家有几口人？都有谁？", "nǐ jiā yǒu jǐ kǒu rén? dōu yǒu shéi?", "Nhà bạn có mấy người? Gồm những ai?", "Trả lời nhà có mấy người (我家有...口人) và kể bố mẹ/anh chị em.", new[] { "我家|wo jia", "有|you", "口人|爸爸|妈妈|baba|mama" }, new[] { "我家有四口人：爸爸、妈妈、姐姐和我。" }, "wǒ jiā yǒu sì kǒu rén: bàba, māma, jiějie hé wǒ.", 6),
                new RpItem("zh-l22-rp-2", "medium", "Hỏi giá hoa quả ở chợ", "Chủ quầy hoa quả chào mời bạn chọn mua táo và chuối.", "Chủ quầy hàng", "你好！今天苹果很新鲜，你要买什么？", "nǐ hǎo! jīntiān píngguǒ hěn xīnxiān, nǐ yào mǎi shénme?", "Chào bạn! Hôm nay táo tươi lắm, bạn muốn mua gì?", "Nói muốn mua táo (苹果) và hỏi bao nhiêu tiền (多少钱).", new[] { "买|要|mai|yao", "苹果|ping guo", "多少钱|duo shao qian" }, new[] { "你好！我要买苹果。这个苹果多少钱一斤？" }, "nǐ hǎo! wǒ yào mǎi píngguǒ. zhège píngguǒ duōshao qián yì jīn?", 8),
                new RpItem("zh-l22-rp-3", "hard", "Mặc cả & chốt mua", "Người bán báo giá 12 tệ 1 cân táo, bạn thấy hơi đắt và muốn mua 2 cân.", "Chủ quầy hàng", "这个苹果十二块钱一斤。", "zhège píngguǒ shí'èr kuài qián yì jīn.", "Táo này 12 tệ một cân.", "Chê hơi đắt (太贵了), đề nghị giảm chút (便宜一点儿) và nói mua 2 cân (两斤).", new[] { "太贵|贵|tai gui", "便宜|pian yi", "两|买|liang|mai" }, new[] { "太贵了！便宜一点儿吧，我买两斤。" }, "tài guì le! piányi yìdiǎnr ba, wǒ mǎi liǎng jīn.", 8)
            }, now));

        // ---------------- LESSON 3.1 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.ChineseLesson31Id, "zh-CN", "Trung HSK1 · Bài 3.1",
            shadowing: new[]
            {
                new ShItem("zh-l31-sh-1", "easy", "Muốn ăn cơm (Dễ)", "我想吃米饭。", "wǒ xiǎng chī mǐfàn", "Tôi muốn ăn cơm.", new[] { "想吃", "米饭" }),
                new ShItem("zh-l31-sh-2", "easy", "Uống trà (Dễ)", "我喜欢喝茶。", "wǒ xǐhuan hē chá", "Tôi thích uống trà.", new[] { "喜欢", "喝茶" }),
                new ShItem("zh-l31-sh-3", "easy", "Khen đồ ăn ngon (Dễ)", "中国菜很好吃！", "Zhōngguó cài hěn hǎochī", "Món ăn Trung Quốc rất ngon!", new[] { "中国菜", "好吃" }),
                new ShItem("zh-l31-sh-4", "medium", "Gọi đồ uống (Vừa)", "请给我一杯热茶。", "qǐng gěi wǒ yì bēi rè chá", "Làm ơn cho tôi 1 tách trà nóng.", new[] { "请给我", "一杯", "热茶" }),
                new ShItem("zh-l31-sh-5", "medium", "Câu hỏi lựa chọn (Vừa)", "你想喝茶还是咖啡？", "nǐ xiǎng hē chá háishi kāfēi?", "Bạn muốn uống trà hay cà phê?", new[] { "还是", "茶", "咖啡" }),
                new ShItem("zh-l31-sh-6", "medium", "Kỹ năng 会 (Vừa)", "我会做中国菜。", "wǒ huì zuò Zhōngguó cài", "Tôi biết nấu món Trung Quốc.", new[] { "会", "中国菜" }),
                new ShItem("zh-l31-sh-7", "hard", "Gọi 2 món ăn (Khó)", "我要一碗面条和一杯水。", "wǒ yào yì wǎn miàntiáo hé yì bēi shuǐ", "Cho tôi 1 bát mì và 1 ly nước.", new[] { "一碗", "面条", "一杯水" }),
                new ShItem("zh-l31-sh-8", "hard", "Sở thích cuối tuần (Khó)", "我非常喜欢看电影和听音乐。", "wǒ fēicháng xǐhuan kàn diànyǐng hé tīng yīnyuè", "Tôi vô cùng thích xem phim và nghe nhạc.", new[] { "喜欢", "看电影", "听音乐" }),
                new ShItem("zh-l31-sh-9", "hard", "Nói tiếng Trung (Khó)", "我会说一点儿汉语，不会写很多汉字。", "wǒ huì shuō yìdiǎnr Hànyǔ, bú huì xiě hěn duō Hànzì", "Tôi biết nói một chút tiếng Trung, chưa biết viết nhiều chữ Hán.", new[] { "会说", "一点儿", "汉字" })
            },
            dictation: new[]
            {
                new DiItem("zh-l31-di-1", "easy", "Muốn uống trà", "我想喝茶", "wǒ xiǎng hē chá", "Tôi muốn uống trà.", new[] { "我", "想", "喝", "茶" }, new[] { "吃", "水", "不" }),
                new DiItem("zh-l31-di-2", "easy", "Món ăn ngon", "这个菜很好吃", "zhège cài hěn hǎochī", "Món này rất ngon.", new[] { "这个菜", "很", "好吃" }, new[] { "好喝", "贵", "是" }),
                new DiItem("zh-l31-di-3", "medium", "Cho tôi 1 tách trà", "请给我一杯茶", "qǐng gěi wǒ yì bēi chá", "Làm ơn cho tôi 1 tách trà.", new[] { "请", "给我", "一杯", "茶" }, new[] { "一碗", "米饭", "吗" }),
                new DiItem("zh-l31-di-4", "medium", "Lựa chọn đồ uống", "你喝茶还是咖啡", "nǐ hē chá háishi kāfēi", "Bạn uống trà hay cà phê?", new[] { "你", "喝茶", "还是", "咖啡" }, new[] { "吗", "和", "吃" }),
                new DiItem("zh-l31-di-5", "hard", "Thích ăn món Trung", "我很喜欢吃中国菜", "wǒ hěn xǐhuan chī Zhōngguó cài", "Tôi rất thích ăn món Trung Quốc.", new[] { "我很", "喜欢", "吃", "中国菜" }, new[] { "喝", "是", "吗" }),
                new DiItem("zh-l31-di-6", "hard", "Biết nói tiếng Trung", "我会说一点儿汉语", "wǒ huì shuō yìdiǎnr Hànyǔ", "Tôi biết nói một chút tiếng Trung.", new[] { "我会", "说", "一点儿", "汉语" }, new[] { "写", "看", "想" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l31-pr-1", "easy", "Uốn lưỡi + bật hơi 吃 & 茶", "吃饭喝茶", "chīfàn hēchá", "Ăn cơm uống trà.", new[] { "吃", "茶" }, "Cả 吃 (chī) và 茶 (chá) đều phải uốn lưỡi + bật hơi mạnh.", new[] { "chī uốn lưỡi bật hơi thanh 1 cao", "fàn thanh 4 rơi gọn", "chá uốn lưỡi bật hơi thanh 2" }, "我想吃饭和喝茶。", new[] { "吃", "茶" }),
                new PrItem("zh-l31-pr-2", "easy", "Uốn lưỡi 水 (shuǐ)", "请给我一杯水", "qǐng gěi wǒ yì bēi shuǐ", "Làm ơn cho tôi 1 ly nước.", new[] { "请", "水" }, "Bật hơi mạnh qǐng và uốn lưỡi rõ ở 水 (shuǐ).", new[] { "qǐng bật hơi", "bēi thanh 1 cao phẳng", "shuǐ uốn lưỡi thanh 3 trầm rõ" }, "请给我一杯水。", new[] { "请", "水" }),
                new PrItem("zh-l31-pr-3", "medium", "Phân biệt 茶 (chá) và 菜 (cài)", "中国菜和中国茶", "Zhōngguó cài hé Zhōngguó chá", "Món Trung Quốc và trà Trung Quốc.", new[] { "中", "菜", "茶" }, "菜 đầu lưỡi thẳng bật hơi thanh 4; 茶 uốn lưỡi bật hơi thanh 2.", new[] { "cài không uốn lưỡi, rơi mạnh thanh 4", "chá uốn lưỡi, lên thanh 2", "zhōng thanh 1 cao phẳng" }, "我喜欢中国菜和中国茶。", new[] { "菜", "茶" }),
                new PrItem("zh-l31-pr-4", "medium", "Uốn lưỡi 热 (rè) & 吃 (chī)", "这个热菜很好吃", "zhège rè cài hěn hǎochī", "Món nóng này rất ngon.", new[] { "这", "热", "菜", "吃" }, "Phối hợp trôi chảy âm uốn lưỡi zh/r/ch và âm đầu lưỡi c.", new[] { "rè uốn lưỡi thanh 4 gọn", "cài bật hơi đầu lưỡi thẳng", "chī uốn lưỡi thanh 1 cao" }, "这个菜很好吃！", new[] { "热", "好吃" }),
                new PrItem("zh-l31-pr-5", "hard", "Câu hỏi lựa chọn 还是", "你吃米饭还是面条", "nǐ chī mǐfàn háishi miàntiáo", "Bạn ăn cơm hay mì?", new[] { "吃", "是", "条" }, "Giữ rõ chī uốn lưỡi, hái thanh 2, tiáo bật hơi t.", new[] { "chī bật hơi uốn lưỡi", "háishi chữ shi đọc nhẹ", "tiáo bật hơi t lên thanh 2" }, "你想吃米饭还是面条？", new[] { "米饭", "还是", "面条" }),
                new PrItem("zh-l31-pr-6", "hard", "Kỹ năng 会说汉语", "我会说汉语也会做菜", "wǒ huì shuō Hànyǔ yě huì zuò cài", "Tôi biết nói tiếng Trung và cũng biết nấu ăn.", new[] { "说", "菜" }, "Uốn lưỡi thanh 1 cao ở 说 (shuō) và bật hơi ở 菜 (cài).", new[] { "shuō uốn lưỡi cao ngang", "zuò đầu lưỡi thẳng không bật hơi", "cài đầu lưỡi thẳng có bật hơi" }, "我会说汉语，也会做菜。", new[] { "说", "做菜" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l31-wm-1", "easy", "Sơ đồ từ vựng: 吃 (Ăn)", "吃", "chī", "Ăn",
                    new[] { new WmNode("吃饭", "chīfàn", "Ăn cơm", "我们十二点吃饭。"), new WmNode("吃苹果", "chī píngguǒ", "Ăn táo", "我每天吃一个苹果。") },
                    new[] { new WmNode("好吃", "hǎochī", "Ngon (đồ ăn)", "妈妈做的菜真好吃！") },
                    new[] { new WmNode("喝 (uống)", "hē", "Uống", "你想喝什么？") },
                    new[] { new WmEx("你中午想吃什么？", "nǐ zhōngwǔ xiǎng chī shénme?", "Trưa nay bạn muốn ăn gì?") }),
                new WmItem("zh-l31-wm-2", "medium", "Sơ đồ từ vựng: 喜欢 (Thích)", "喜欢", "xǐhuan", "Thích, yêu thích",
                    new[] { new WmNode("很喜欢", "hěn xǐhuan", "Rất thích", "我很喜欢喝绿茶。"), new WmNode("喜欢看书", "xǐhuan kànshū", "Thích đọc sách", "我喜欢在图书馆看书。") },
                    new[] { new WmNode("最喜欢", "zuì xǐhuan", "Thích nhất", "我最喜欢吃饺子。") },
                    new[] { new WmNode("爱", "ài", "Yêu thích (mức độ mạnh)", "我爱学习语言。") },
                    new[] { new WmEx("你喜欢喝茶还是喝咖啡？", "nǐ xǐhuan hē chá háishi hē kāfēi?", "Bạn thích uống trà hay cà phê?") }),
                new WmItem("zh-l31-wm-3", "hard", "Sơ đồ từ vựng: 会 (Biết làm / Sẽ)", "会", "huì", "Biết (kỹ năng học được)",
                    new[] { new WmNode("会说汉语", "huì shuō Hànyǔ", "Biết nói tiếng Trung", "我会说一点儿汉语。"), new WmNode("会写汉字", "huì xiě Hànzì", "Biết viết chữ Hán", "你会写这个字吗？") },
                    new[] { new WmNode("不会", "bú huì", "Không biết làm", "我不会开车。") },
                    new[] { new WmNode("能 / 可以", "néng / kěyǐ", "Có thể (khả năng / cho phép)", "我可以坐这里吗？") },
                    new[] { new WmEx("我姐姐会做很好吃的越南菜。", "wǒ jiějie huì zuò hěn hǎochī de Yuènán cài", "Chị gái tôi biết nấu món Việt rất ngon.") })
            },
            cloze: new[]
            {
                new ClItem("zh-l31-cl-1", "easy", "Lượng từ đồ uống 杯", "Điền lượng từ 'ly/tách' cho trà:", "请给我一{{0}}茶。", new[] { new ClBlank(new[] { "杯", "bēi", "bei1", "bei" }, "ly / tách (bēi)", "Đồ uống (trà, nước, cà phê) dùng lượng từ 杯.") }, "请给我一杯茶。", "qǐng gěi wǒ yì bēi chá", "Làm ơn cho tôi 1 tách trà.", "Một bát cơm là 一碗米饭, một tách trà là 一杯茶."),
                new ClItem("zh-l31-cl-2", "easy", "Động từ năng nguyện 喜欢", "Điền từ 'thích' vào câu:", "我很{{0}}吃中国菜。", new[] { new ClBlank(new[] { "喜欢", "xǐhuan", "xi3 huan", "xihuan" }, "thích (xǐhuan)", "喜欢 + Động từ / Danh từ = thích...") }, "我很喜欢吃中国菜。", "wǒ hěn xǐhuan chī Zhōngguó cài", "Tôi rất thích ăn món Trung Quốc.", "喜欢 đọc thanh 3 + thanh nhẹ."),
                new ClItem("zh-l31-cl-3", "medium", "Câu hỏi lựa chọn 还是", "Điền liên từ 'hay là' trong câu hỏi lựa chọn:", "你想喝茶{{0}}喝咖啡？", new[] { new ClBlank(new[] { "还是", "háishi", "hai2 shi" }, "hay là (háishi)", "Trong câu hỏi lựa chọn luôn dùng 还是 (không dùng 或者).") }, "你想喝茶还是喝咖啡？", "nǐ xiǎng hē chá háishi hē kāfēi?", "Bạn muốn uống trà hay cà phê?", "Câu có 还是 không thêm 吗 ở cuối."),
                new ClItem("zh-l31-cl-4", "hard", "Động từ chỉ kỹ năng 会", "Điền động từ 'biết (kỹ năng học được)':", "我{{0}}说汉语，但是不{{1}}做中国菜。", new[] { new ClBlank(new[] { "会", "huì", "hui4" }, "biết (huì)", "Kỹ năng học được dùng 会."), new ClBlank(new[] { "会", "huì", "hui4" }, "không biết (bú huì)", "Phủ định của 会 là 不会.") }, "我会说汉语，但是不会做中国菜。", "wǒ huì shuō Hànyǔ, dànshì bú huì zuò Zhōngguó cài", "Tôi biết nói tiếng Trung nhưng không biết nấu món Trung.", "Phân biệt 会 (kỹ năng) với 认识 (quen biết).")
            },
            translate: new[]
            {
                new TrItem("zh-l31-tr-1", "easy", "Muốn uống trà", "Tôi muốn uống trà.", new[] { "我", "想", "喝茶" }, new[] { "我想喝茶。", "我想喝茶", "wǒ xiǎng hē chá", "wo xiang he cha" }, "我想喝茶。", "wǒ xiǎng hē chá.", "Dùng 想 + Động từ để diễn đạt mong muốn."),
                new TrItem("zh-l31-tr-2", "easy", "Cho tôi 1 ly nước", "Làm ơn cho tôi một ly nước.", new[] { "请给我", "一杯水" }, new[] { "请给我一杯水。", "请给我一杯水", "给我一杯水。", "qǐng gěi wǒ yì bēi shuǐ", "qing gei wo yi bei shui" }, "请给我一杯水。", "qǐng gěi wǒ yì bēi shuǐ.", "Thêm 请 ở đầu câu giúp lời đề nghị lịch sự hơn."),
                new TrItem("zh-l31-tr-3", "medium", "Trà hay cà phê?", "Bạn muốn uống trà hay cà phê?", new[] { "你想", "喝茶", "还是", "咖啡" }, new[] { "你想喝茶还是咖啡？", "你想喝茶还是喝咖啡？", "nǐ xiǎng hē chá háishi kāfēi", "nǐ xiǎng hē chá háishi hē kāfēi" }, "你想喝茶还是咖啡？", "nǐ xiǎng hē chá háishi kāfēi?", "Dùng 还是 cho câu hỏi lựa chọn, không thêm 吗."),
                new TrItem("zh-l31-tr-4", "hard", "Thích món Trung Quốc", "Tôi rất thích ăn món Trung Quốc, rất ngon!", new[] { "很喜欢", "中国菜", "很好吃" }, new[] { "我很喜欢吃中国菜，很好吃！", "我很喜欢吃中国菜，很好吃。", "wǒ hěn xǐhuan chī Zhōngguó cài hěn hǎochī" }, "我很喜欢吃中国菜，很好吃！", "wǒ hěn xǐhuan chī Zhōngguó cài, hěn hǎochī!", "Đồ ăn ngon dùng 好吃, đồ uống ngon dùng 好喝.")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l31-gf-1", "easy", "Dùng thừa 吗 với câu hỏi 还是", "你喝茶还是喝咖啡吗？", "Câu hỏi lựa chọn có 还是 thì KHÔNG thêm 吗.", "你喝茶还是喝咖啡？", new[] { "你喝茶还是喝咖啡？", "你喝茶还是咖啡？" }, "nǐ hē chá háishi hē kāfēi?", "Bạn uống trà hay uống cà phê?", "还是 đã mang chức năng hỏi lựa chọn."),
                new GfItem("zh-l31-gf-2", "medium", "Nhầm 好吃 với 好喝 cho đồ uống", "这杯茶很好吃。", "Trà là đồ uống nên phải dùng 好喝 (không dùng 好吃).", "这杯茶很好喝。", new[] { "这杯茶很好喝。", "这杯茶很好喝" }, "zhè bēi chá hěn hǎohē", "Tách trà này rất ngon.", "Đồ ăn dùng 好吃, đồ uống dùng 好喝."),
                new GfItem("zh-l31-gf-3", "hard", "Nhầm 认识 với 会 cho kỹ năng nói", "我认识说汉语。", "Kỹ năng học được (nói tiếng Trung) phải dùng 会.", "我会说汉语。", new[] { "我会说汉语。", "我会说汉语" }, "wǒ huì shuō Hànyǔ", "Tôi biết nói tiếng Trung.", "认识 dùng cho người/mặt chữ; 会 dùng cho kỹ năng hành động.")
            },
            roleplay: new[]
            {
                new RpItem("zh-l31-rp-1", "easy", "Gọi đồ uống ở quán trà", "Nhân viên quán hỏi bạn muốn uống trà hay cà phê.", "Nhân viên phục vụ", "欢迎光临！您想喝茶还是喝咖啡？", "huānyíng guānglín! nín xiǎng hē chá háishi hē kāfēi?", "Chào mừng quý khách! Quý khách muốn uống trà hay cà phê?", "Gọi 1 tách trà hoặc cà phê bằng 请给我 / 我要 và 一杯.", new[] { "一杯|yi bei", "茶|咖啡|cha|ka fei", "谢谢|请|要|xie xie|qing|yao" }, new[] { "请给我一杯热茶，谢谢！" }, "qǐng gěi wǒ yì bēi rè chá, xièxie!", 6),
                new RpItem("zh-l31-rp-2", "medium", "Gọi bữa trưa tại nhà hàng", "Phục vụ nhà hàng mời bạn gọi món ăn trưa.", "Phục vụ nhà hàng", "请问您想吃什么？我们有米饭、面条和饺子。", "qǐngwèn nín xiǎng chī shénme? wǒmen yǒu mǐfàn, miàntiáo hé jiǎozi.", "Xin hỏi quý khách muốn ăn gì? Quán có cơm, mì và sủi cảo.", "Gọi món ăn (米饭/面条/饺子) kèm lượng từ (一碗/一盘) và đồ uống.", new[] { "要|想吃|yao|xiang chi", "米饭|面条|饺子|mi fan|mian tiao|jiao zi", "水|茶|碗|盘" }, new[] { "我要一碗面条和一杯水，谢谢！" }, "wǒ yào yì wǎn miàntiáo hé yì bēi shuǐ, xièxie!", 8),
                new RpItem("zh-l31-rp-3", "hard", "Trò chuyện về sở thích", "Bạn Trung Quốc hỏi bạn thích ăn món gì và có biết nấu ăn không.", "Bạn Trương Vĩ", "你喜欢吃中国菜吗？你会做菜吗？", "nǐ xǐhuan chī Zhōngguó cài ma? nǐ huì zuò cài ma?", "Bạn có thích ăn món Trung không? Bạn biết nấu ăn không?", "Nói thích ăn món Trung (很喜欢 / 很好吃) và trả lời bạn biết (会) hoặc không biết (不会) nấu ăn.", new[] { "喜欢|xi huan", "好吃|中国菜|hao chi", "会|不会|hui" }, new[] { "我很喜欢吃中国菜，很好吃！我会做一点儿越南菜。" }, "wǒ hěn xǐhuan chī Zhōngguó cài, hěn hǎochī! wǒ huì zuò yìdiǎnr Yuènán cài.", 10)
            }, now));

        // ---------------- LESSON 3.2 ----------------
        all.AddRange(BuildLessonPack(cId, SeedDataInitializer.ChineseLesson32Id, "zh-CN", "Trung HSK1 · Bài 3.2",
            shadowing: new[]
            {
                new ShItem("zh-l32-sh-1", "easy", "Hỏi địa điểm (Dễ)", "请问，火车站在哪儿？", "qǐngwèn, huǒchēzhàn zài nǎr?", "Xin hỏi ga tàu hỏa ở đâu?", new[] { "火车站", "在哪儿" }),
                new ShItem("zh-l32-sh-2", "easy", "Đi phương tiện (Dễ)", "我坐地铁去学校。", "wǒ zuò dìtiě qù xuéxiào", "Tôi đi tàu điện ngầm đến trường.", new[] { "坐地铁", "去学校" }),
                new ShItem("zh-l32-sh-3", "easy", "Vị trí phía trước (Dễ)", "商店在学校前面。", "shāngdiàn zài xuéxiào qiánmian", "Cửa hàng ở phía trước trường học.", new[] { "商店", "前面" }),
                new ShItem("zh-l32-sh-4", "medium", "Hỏi cách đi (Vừa)", "请问，去医院怎么走？", "qǐngwèn, qù yīyuàn zěnme zǒu?", "Xin hỏi đi bệnh viện đi như thế nào?", new[] { "去医院", "怎么走" }),
                new ShItem("zh-l32-sh-5", "medium", "Câu liên động (Vừa)", "我去图书馆看书。", "wǒ qù túshūguǎn kànshū", "Tôi đến thư viện đọc sách.", new[] { "去图书馆", "看书" }),
                new ShItem("zh-l32-sh-6", "medium", "Đi taxi (Vừa)", "我们坐出租车去火车站吧。", "wǒmen zuò chūzūchē qù huǒchēzhàn ba", "Chúng ta đi taxi ra ga tàu hỏa nhé.", new[] { "坐出租车", "火车站" }),
                new ShItem("zh-l32-sh-7", "hard", "Chỉ đường đi thẳng rẽ trái (Khó)", "一直往前走，到路口往左拐。", "yìzhí wǎng qián zǒu, dào lùkǒu wǎng zuǒ guǎi", "Đi thẳng về phía trước, đến ngã tư rẽ trái.", new[] { "一直", "往前走", "往左拐" }),
                new ShItem("zh-l32-sh-8", "hard", "Lịch trình hằng ngày (Khó)", "我每天早上七点起床，八点去公司工作。", "wǒ měitiān zǎoshang qī diǎn qǐchuáng, bā diǎn qù gōngsī gōngzuò", "Mỗi sáng 7 giờ tôi ngủ dậy, 8 giờ đến công ty làm việc.", new[] { "每天", "起床", "工作" }),
                new ShItem("zh-l32-sh-9", "hard", "Hành động đang diễn ra (Khó)", "我现在在家里学习汉语。", "wǒ xiànzài zài jiālǐ xuéxí Hànyǔ", "Bây giờ tôi đang học tiếng Trung ở nhà.", new[] { "现在", "在家里", "学习汉语" })
            },
            dictation: new[]
            {
                new DiItem("zh-l32-di-1", "easy", "Ở đâu", "学校在哪儿", "xuéxiào zài nǎr", "Trường học ở đâu?", new[] { "学校", "在", "哪儿" }, new[] { "医院", "去", "怎么" }),
                new DiItem("zh-l32-di-2", "easy", "Đi tàu điện ngầm", "我坐地铁去", "wǒ zuò dìtiě qù", "Tôi đi bằng tàu điện ngầm.", new[] { "我", "坐", "地铁", "去" }, new[] { "走", "飞机", "在" }),
                new DiItem("zh-l32-di-3", "medium", "Hỏi đường", "请问去火车站怎么走", "qǐngwèn qù huǒchēzhàn zěnme zǒu", "Xin hỏi đi ra ga tàu hỏa đi thế nào?", new[] { "请问", "去", "火车站", "怎么走" }, new[] { "在哪儿", "坐", "吗" }),
                new DiItem("zh-l32-di-4", "medium", "Đến thư viện đọc sách", "我去图书馆看书", "wǒ qù túshūguǎn kànshū", "Tôi đến thư viện đọc sách.", new[] { "我", "去", "图书馆", "看书" }, new[] { "在", "学校", "写" }),
                new DiItem("zh-l32-di-5", "hard", "Bên phải ngân hàng", "医院在银行右边", "yīyuàn zài yínháng yòubian", "Bệnh viện ở bên phải ngân hàng.", new[] { "医院", "在", "银行", "右边" }, new[] { "左边", "前面", "去" }),
                new DiItem("zh-l32-di-6", "hard", "Đi taxi đến sân bay", "我们坐出租车去机场", "wǒmen zuò chūzūchē qù jīchǎng", "Chúng tôi đi taxi ra sân bay.", new[] { "我们", "坐", "出租车", "去机场" }, new[] { "走", "火车站", "在" })
            },
            pronunciation: new[]
            {
                new PrItem("zh-l32-pr-1", "easy", "Uốn lưỡi 火车站", "火车站在哪儿", "huǒchēzhàn zài nǎr", "Ga tàu hỏa ở đâu?", new[] { "车", "站" }, "Bật hơi uốn lưỡi chē (thanh 1 cao) và uốn lưỡi zhàn (thanh 4).", new[] { "chē uốn lưỡi bật hơi cao ngang", "zhàn uốn lưỡi rơi dứt khoát", "zài đầu lưỡi thẳng thanh 4" }, "请问，火车站在哪儿？", new[] { "火车站", "在哪儿" }),
                new PrItem("zh-l32-pr-2", "easy", "Phân biệt 坐 (zuò) vs 走 (zǒu)", "坐车还是走路", "zuòchē háishi zǒulù", "Đi xe hay đi bộ?", new[] { "坐", "车", "走" }, "zuò thanh 4 gọn (vần uo); zǒu thanh 3 trầm (vần ou).", new[] { "zuò rơi nhanh thanh 4", "chē bật hơi uốn lưỡi thanh 1", "zǒu hạ sâu thanh 3" }, "你坐车去还是走路去？", new[] { "坐车", "走路" }),
                new PrItem("zh-l32-pr-3", "medium", "Tổ hợp 出租车 (chūzūchē)", "我坐出租车去", "wǒ zuò chūzūchē qù", "Tôi đi bằng taxi.", new[] { "出", "车", "去" }, "Chuyển linh hoạt giữa chū (uốn lưỡi bật hơi) → zū (thẳng không bật) → chē (uốn lưỡi bật hơi).", new[] { "Cả 3 âm chū-zū-chē đều thanh 1 cao phẳng", "Chỉ bật hơi ở chū và chē", "qù bật hơi q tròn môi ü" }, "我坐出租车去火车站。", new[] { "出租车", "去" }),
                new PrItem("zh-l32-pr-4", "medium", "Hỏi đường 怎么走", "去医院怎么走", "qù yīyuàn zěnme zǒu", "Đi bệnh viện đi thế nào?", new[] { "去", "走" }, "Bật hơi q (qù), giữ thanh 1 cao ở yī, đọc rõ zěn và zǒu.", new[] { "qù bật hơi gọn thanh 4", "yī thanh 1 cao phẳng", "zěnme thanh 3 + nhẹ" }, "请问，去医院怎么走？", new[] { "医院", "怎么走" }),
                new PrItem("zh-l32-pr-5", "hard", "Chỉ đường 一直往前走", "一直往前走往左拐", "yìzhí wǎng qián zǒu wǎng zuǒ guǎi", "Đi thẳng về trước rồi rẽ trái.", new[] { "直", "前", "左" }, "Uốn lưỡi zhí, bật hơi qián, phân biệt zuǒ (trái) và zǒu (đi).", new[] { "zhí uốn lưỡi thanh 2", "qián bật hơi lên thanh 2", "Phân biệt vần ou (zǒu) và uo (zuǒ)" }, "一直往前走，到路口往左拐。", new[] { "往前走", "往左拐" }),
                new PrItem("zh-l32-pr-6", "hard", "Thư viện 图书馆", "我去图书馆看书", "wǒ qù túshūguǎn kànshū", "Tôi đi thư viện đọc sách.", new[] { "去", "图", "书", "看" }, "Bật hơi t (tú) & k (kàn), uốn lưỡi shū (thanh 1 cao).", new[] { "tú bật hơi lên thanh 2", "shū uốn lưỡi cao phẳng", "kàn bật hơi rơi mạnh thanh 4" }, "我每天下午去图书馆看书。", new[] { "图书馆", "看书" })
            },
            wordMaps: new[]
            {
                new WmItem("zh-l32-wm-1", "easy", "Sơ đồ từ vựng: 去 (Đi)", "去", "qù", "Đi tới (nơi nào đó)",
                    new[] { new WmNode("去学校", "qù xuéxiào", "Đi đến trường", "我八点去学校。"), new WmNode("去北京", "qù Běijīng", "Đi Bắc Kinh", "我明年想去北京旅游。") },
                    new[] { new WmNode("去看电影", "qù kàn diànyǐng", "Đi xem phim", "周末我们去看电影吧。") },
                    new[] { new WmNode("来 / 回", "lái / huí", "Đến / Quay về", "你什么时候回家？") },
                    new[] { new WmEx("我坐地铁去公司上班。", "wǒ zuò dìtiě qù gōngsī shàngbān", "Tôi đi tàu điện ngầm tới công ty làm việc.") }),
                new WmItem("zh-l32-wm-2", "medium", "Sơ đồ từ vựng: 在 (Ở / Đang)", "在", "zài", "Ở tại (địa điểm) / Đang (hành động)",
                    new[] { new WmNode("在哪儿", "zài nǎr", "Ở đâu", "银行在哪儿？"), new WmNode("在前面", "zài qiánmian", "Ở phía trước", "地铁站在商店前面。") },
                    new[] { new WmNode("在学习", "zài xuéxí", "Đang học bài", "我在房间里学习汉语。") },
                    new[] { new WmNode("正在", "zhèngzài", "Đang (nhấn mạnh)", "他正在打电话。") },
                    new[] { new WmEx("我的朋友在中国工作。", "wǒ de péngyou zài Zhōngguó gōngzuò", "Bạn của tôi làm việc ở Trung Quốc.") }),
                new WmItem("zh-l32-wm-3", "hard", "Sơ đồ từ vựng: 坐 (Ngồi / Đi phương tiện)", "坐", "zuò", "Ngồi / Đi bằng phương tiện",
                    new[] { new WmNode("坐地铁", "zuò dìtiě", "Đi tàu điện ngầm", "坐地铁很快。"), new WmNode("坐飞机", "zuò fēijī", "Đi máy bay", "我们坐飞机去上海。") },
                    new[] { new WmNode("请坐", "qǐng zuò", "Xin mời ngồi", "欢迎，请坐！") },
                    new[] { new WmNode("走路 / 开车", "zǒulù / kāichē", "Đi bộ / Lái xe", "不远，我们走路去吧。") },
                    new[] { new WmEx("从学校坐公共汽车二十分钟就到。", "cóng xuéxiào zuò gōnggòng qìchē èrshí fēnzhōng jiù dào", "Từ trường đi xe buýt 20 phút là tới.") })
            },
            cloze: new[]
            {
                new ClItem("zh-l32-cl-1", "easy", "Hỏi ở đâu", "Điền động từ vị trí và từ hỏi 'ở đâu':", "请问，医院{{0}}{{1}}？", new[] { new ClBlank(new[] { "在", "zài", "zai4", "zai" }, "ở (zài)", "A 在 B = A ở B."), new ClBlank(new[] { "哪儿", "哪里", "nǎr", "nǎlǐ", "na3 r", "na3 li3", "nar", "nali" }, "đâu (nǎr / nǎlǐ)", "在哪儿 / 在哪里 = ở đâu.") }, "请问，医院在哪儿？", "qǐngwèn, yīyuàn zài nǎr?", "Xin hỏi bệnh viện ở đâu?", "Miền Bắc Trung Quốc hay dùng 哪儿, miền Nam hay dùng 哪里."),
                new ClItem("zh-l32-cl-2", "easy", "Đi phương tiện 坐", "Điền động từ dùng trước phương tiện giao thông 'tàu điện ngầm':", "我{{0}}地铁去公司。", new[] { new ClBlank(new[] { "坐", "zuò", "zuo4", "zuo" }, "đi phương tiện (zuò)", "坐 + Phương tiện + 去 + Địa điểm.") }, "我坐地铁去公司。", "wǒ zuò dìtiě qù gōngsī", "Tôi đi tàu điện ngầm đến công ty.", "Không dùng 走 với phương tiện như tàu điện hay xe buýt."),
                new ClItem("zh-l32-cl-3", "medium", "Mẫu hỏi đường 怎么走", "Điền cụm từ 'đi như thế nào':", "请问，去火车站{{0}}？", new[] { new ClBlank(new[] { "怎么走", "zěnme zǒu", "zen3 me zou3" }, "đi thế nào (zěnme zǒu)", "去 + [Địa điểm] + 怎么走？ = Đi... đi thế nào?") }, "请问，去火车站怎么走？", "qǐngwèn, qù huǒchēzhàn zěnme zǒu?", "Xin hỏi đi ra ga tàu hỏa đi thế nào?", "Mẫu câu hỏi đường thông dụng nhất khi đi du lịch Trung Quốc."),
                new ClItem("zh-l32-cl-4", "hard", "Vị trí + Hành động 在...学习", "Hoàn thành câu 'Tôi học tiếng Trung ở thư viện':", "我{{0}}图书馆{{1}}汉语。", new[] { new ClBlank(new[] { "在", "zài", "zai4" }, "ở tại (zài)", "Trạng ngữ địa điểm 在 + Nơi chốn đặt TRƯỚC động từ."), new ClBlank(new[] { "学习", "学", "xuéxí", "xué", "xue2 xi2" }, "học tập (xuéxí)", "学习汉语 = học tiếng Trung.") }, "我在图书馆学习汉语。", "wǒ zài túshūguǎn xuéxí Hànyǔ", "Tôi học tiếng Trung ở thư viện.", "Trật tự vàng tiếng Trung: Chủ ngữ + 在 + Địa điểm + Động từ!")
            },
            translate: new[]
            {
                new TrItem("zh-l32-tr-1", "easy", "Hỏi ga tàu hỏa ở đâu", "Xin hỏi ga tàu hỏa ở đâu?", new[] { "请问", "火车站", "在哪儿" }, new[] { "请问，火车站在哪儿？", "请问，火车站在哪里？", "火车站在哪儿？", "qǐngwèn huǒchēzhàn zài nǎr", "qingwen huochezhan zai nar" }, "请问，火车站在哪儿？", "qǐngwèn, huǒchēzhàn zài nǎr?", "Có thể dùng 哪儿 hoặc 哪里 đều chuẩn."),
                new TrItem("zh-l32-tr-2", "easy", "Đi tàu điện ngầm đến trường", "Tôi đi tàu điện ngầm đến trường học.", new[] { "我", "坐地铁", "去学校" }, new[] { "我坐地铁去学校。", "我坐地铁去学校", "wǒ zuò dìtiě qù xuéxiào", "wo zuo ditie qu xuexiao" }, "我坐地铁去学校。", "wǒ zuò dìtiě qù xuéxiào.", "Phương tiện (坐地铁) đặt TRƯỚC động từ đi (去学校)."),
                new TrItem("zh-l32-tr-3", "medium", "Hỏi đường tới bệnh viện", "Xin hỏi đi bệnh viện đi như thế nào?", new[] { "请问", "去医院", "怎么走" }, new[] { "请问，去医院怎么走？", "去医院怎么走？", "qǐngwèn qù yīyuàn zěnme zǒu" }, "请问，去医院怎么走？", "qǐngwèn, qù yīyuàn zěnme zǒu?", "Mẫu chuẩn: 请问，去 + [Địa điểm] + 怎么走？"),
                new TrItem("zh-l32-tr-4", "hard", "Học tiếng Trung ở thư viện", "Mỗi ngày tôi đều học tiếng Trung ở thư viện.", new[] { "每天", "在图书馆", "学习汉语" }, new[] { "我每天都在图书馆学习汉语。", "我每天在图书馆学习汉语。", "每天我在图书馆学习汉语。", "wǒ měitiān zài túshūguǎn xuéxí Hànyǔ" }, "我每天都在图书馆学习汉语。", "wǒ měitiān dōu zài túshūguǎn xuéxí Hànyǔ.", "Trật tự chuẩn: Chủ ngữ + Thời gian + 在 Địa điểm + Động từ.")
            },
            grammarFix: new[]
            {
                new GfItem("zh-l32-gf-1", "easy", "Đặt 在 Địa điểm sau động từ", "我学习汉语在图书馆。", "Trạng ngữ địa điểm 在图书馆 phải đặt TRƯỚC động từ 学习.", "我在图书馆学习汉语。", new[] { "我在图书馆学习汉语。", "我在图书馆学习汉语" }, "wǒ zài túshūguǎn xuéxí Hànyǔ", "Tôi học tiếng Trung ở thư viện.", "Khác tiếng Việt, tiếng Trung đặt 'ở đâu' TRƯỚC 'làm gì'."),
                new GfItem("zh-l32-gf-2", "medium", "Đặt phương tiện sau 去学校", "我去学校坐地铁。", "Phương tiện 坐地铁 phải đặt TRƯỚC 去学校.", "我坐地铁去学校。", new[] { "我坐地铁去学校。", "我坐地铁去学校" }, "wǒ zuò dìtiě qù xuéxiào", "Tôi đi tàu điện ngầm đến trường.", "Cấu trúc liên động: 坐 + Phương tiện + 去 + Địa điểm."),
                new GfItem("zh-l32-gf-3", "hard", "Nhầm 走 với 坐 cho taxi", "我走出租车去机场。", "Đi bằng taxi/xe/tàu phải dùng 坐 (走 chỉ dùng cho đi bộ 走路).", "我坐出租车去机场。", new[] { "我坐出租车去机场。", "我坐出租车去机场" }, "wǒ zuò chūzūchē qù jīchǎng", "Tôi đi taxi ra sân bay.", "Phân biệt 坐 (đi phương tiện) vs 走路 (đi bộ).")
            },
            roleplay: new[]
            {
                new RpItem("zh-l32-rp-1", "easy", "Hỏi đường ra ga tàu hỏa", "Bạn vừa ra khỏi khách sạn và muốn hỏi người dân đường ra ga tàu hỏa.", "Người dân địa phương", "你好！请问你要去哪儿？", "nǐ hǎo! qǐngwèn nǐ yào qù nǎr?", "Chào bạn! Xin hỏi bạn muốn đi đâu thế?", "Hỏi lịch sự ga tàu hỏa ở đâu hoặc đi như thế nào (请问, 火车站, 在哪儿 / 怎么走).", new[] { "请问|qing wen", "火车站|huo che zhan", "在哪儿|在哪里|怎么走|zai nar|zen me zou" }, new[] { "请问，去火车站怎么走？" }, "qǐngwèn, qù huǒchēzhàn zěnme zǒu?", 6),
                new RpItem("zh-l32-rp-2", "medium", "Hỏi phương tiện di chuyển", "Người dân bảo nhà ga hơi xa và hỏi bạn muốn đi tàu điện ngầm hay taxi.", "Người dân địa phương", "火车站有点儿远，你想坐地铁还是坐出租车？", "huǒchēzhàn yǒudiǎnr yuǎn, nǐ xiǎng zuò dìtiě háishi zuò chūzūchē?", "Ga tàu hỏa hơi xa, bạn muốn đi tàu điện ngầm hay đi taxi?", "Chọn đi tàu điện ngầm (坐地铁) hoặc taxi (坐出租车) và cảm ơn người chỉ đường.", new[] { "坐|zuo", "地铁|出租车|di tie|chu zu che", "谢谢|xie xie" }, new[] { "我想坐地铁去，请问地铁站在哪儿？谢谢您！" }, "wǒ xiǎng zuò dìtiě qù, qǐngwèn dìtiězhàn zài nǎr? xièxie nín!", 8),
                new RpItem("zh-l32-rp-3", "hard", "Kể lịch trình trong ngày", "Thầy giáo hỏi lịch sinh hoạt hằng ngày của bạn.", "Thầy Vương", "你每天几点去学校？下午做什么？", "nǐ měitiān jǐ diǎn qù xuéxiào? xiàwǔ zuò shénme?", "Mỗi ngày mấy giờ em đến trường? Buổi chiều làm gì?", "Kể giờ đến trường bằng phương tiện gì và buổi chiều làm gì ở đâu (đặt 在 Địa điểm trước Động từ).", new[] { "点|dian", "去|学校|公司|qu", "在|图书馆|家|学习|看书" }, new[] { "我每天八点坐地铁去学校，下午在图书馆学习汉语。" }, "wǒ měitiān bā diǎn zuò dìtiě qù xuéxiào, xiàwǔ zài túshūguǎn xuéxí Hànyǔ.", 10)
            }, now));

        return all;
    }
}
