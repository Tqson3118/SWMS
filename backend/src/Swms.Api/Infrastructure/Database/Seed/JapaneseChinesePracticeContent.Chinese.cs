namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using Swms.Api.Domain.Entities;

public static partial class JapaneseChinesePracticeContent
{
    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG SƠ CẤP (HSK1) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG SƠ CẤP (HSK1) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG SƠ CẤP (HSK1) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG SƠ CẤP (HSK1) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG SƠ CẤP (HSK1) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG SƠ CẤP (HSK1) — 6 BÀI LÝ THUYẾT CHI TIẾT (~9.000-12.000 KÝ TỰ)
    // =========================================================================

    private static List<CjkLessonMeta> GetChineseLessons() => new()
    {
        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter1Id, SeedDataInitializer.ChineseLesson11Id,
            "Bài 1.1: Hệ thống Pinyin, 4 Thanh điệu & Chào hỏi",
            "zh-hsk1-pinyin-thanh-dieu-chao-hoi", 1, 35,
            @"# Bài 1.1: Hệ thống Pinyin, 4 Thanh điệu & Chào hỏi

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Chào mừng bạn bắt đầu cuộc hành trình khám phá ngôn ngữ của hơn 1,4 tỷ người trên thế giới! Bài học đầu tiên này là nền móng cốt tử:
- Hiểu rõ bản chất hệ thống phiên âm **Pinyin (Bính âm Hán ngữ)** và cách thức Pinyin dẫn đường giúp người học phương Tây và Việt Nam phát âm chuẩn Bắc Kinh.
- Làm chủ 21 Thanh mẫu (phụ âm đầu), phân biệt rạch ròi nhóm **âm bật hơi** (`p, t, k, q, c, ch`) với **âm không bật hơi** (`b, d, g, j, z, zh`), nhóm **âm đầu lưỡi** và nhóm **âm uốn lưỡi** (`zh, ch, sh, r`).
- Luyện phát âm tròn vành rõ chữ **4 Thanh điệu chính + Khinh thanh (thanh nhẹ)**, nắm bắt cao độ thanh 1 (55) và độ rơi dứt khoát của thanh 4 (51).
- Làm quen với **Mẹo chiết tự chữ Hán** thông qua các bộ thủ hình tượng vỡ lòng (`好`, `你`, `谢`, `再`, `见`).
- Giao tiếp tự tin các câu chào hỏi, cảm ơn, xin lỗi và tạm biệt chuẩn mực HSK1 trong đời sống thường nhật.
- Khắc phục các lỗi phát âm sai kinh điển mà người Việt Nam do ảnh hưởng của tiếng mẹ đẻ hay mắc phải.

---

## 2. 🌱 Nhập môn Pinyin & Chiết tự Chữ Hán dễ nhớ

### 2.1. Bản chất hệ thống Pinyin (拼音)
Chữ Hán là chữ viết tượng hình và biểu ý, **nhìn vào mặt chữ không thể biết chính xác cách phát âm** nếu chưa từng được học. Để giải quyết vấn đề này, hệ thống **Pinyin (Phiên âm Latinh Hán ngữ)** đã ra đời vào năm 1958.
Một âm tiết tiếng Trung tiêu chuẩn luôn được cấu tạo từ 3 thành phần:
$$\text{Âm tiết} = \text{Thanh mẫu (Phụ âm đầu)} + \text{Vận mẫu (Nguyên âm / Vần)} + \text{Thanh điệu (Dấu giọng)}$$
Ví dụ: Trong chữ `妈 (mā - mẹ)`, `m` là Thanh mẫu, `a` là Vận mẫu, dấu ngang `-` trên đầu chữ `a` là Thanh 1.

### 2.2. Phân loại 21 Thanh mẫu (Phụ âm đầu) theo cặp đối lập
Cách học Pinyin thông minh nhất là đặt các âm tương đồng cạnh nhau để so sánh độ bật hơi:
1. **Nhóm âm hai môi & răng môi (`b, p, m, f`)**:
   - `b`: Không bật hơi, ngậm môi rồi mở nhẹ, phát âm tương đương chữ ""p"" trong tiếng Việt (`bā` đọc như ""pa"").
   - `p`: **BẬT HƠI MẠNH**. Ngậm chặt hai môi, nén luồng hơi trong khoang miệng rồi bật bung ra dứt khoát làm rung tờ giấy đặt trước miệng (`pā`).
   - `m`: Đọc như ""m"" tiếng Việt (`mā`).
   - `f`: Răng trên chạm nhẹ môi dưới, đẩy hơi ra như ""ph"" tiếng Việt (`fā`).
2. **Nhóm âm đầu lưỡi (`d, t, n, l`)**:
   - `d`: Không bật hơi, đầu lưỡi chạm lợi trên, phát âm như chữ ""t"" tiếng Việt (`dà` đọc như ""tạ"").
   - `t`: **BẬT HƠI MẠNH**, phát âm như chữ ""th"" tiếng Việt kèm luồng gió mạnh (`tā` đọc như ""tha"").
   - `n, l`: Phát âm tương tự ""n, l"" tiếng Việt.
3. **Nhóm âm gốc lưỡi (`g, k, h`)**:
   - `g`: Không bật hơi, đọc như ""c/k"" tiếng Việt (`gēge` đọc như ""cơ-cơ"").
   - `k`: **BẬT HƠI TỪ CUỐNG HỌNG**, đẩy luồng hơi mạnh từ đáy họng (`kǒu` đọc như ""khẩu"").
   - `h`: Nằm giữa chữ ""h"" và ""kh"" nhẹ của tiếng Việt, luồng hơi xát nhẹ qua cuống họng (`hǎo`).
4. **Nhóm âm mặt lưỡi (`j, q, x`) — Đi với vận mẫu i, ü**:
   - `j`: Không bật hơi, mặt lưỡi áp sát ngạc cứng, phát âm như ""ch"" tiếng Việt (`jī`).
   - `q`: **BẬT HƠI XÌ MẠNH**, mặt lưỡi nén hơi rồi xì luồng gió sắc sảo qua kẽ răng (`qǐng`).
   - `x`: Mặt lưỡi nâng lên, đẩy hơi êm như chữ ""x"" tiếng Việt (`xièxie`).
5. **Nhóm âm đầu lưỡi trước (`z, c, s`) — Đầu lưỡi thẳng**:
   - `z`: Không bật hơi, đầu lưỡi chạm mặt sau răng trên rồi hé mở (`zài`).
   - `c`: **BẬT HƠI MẠNH**, đầu lưỡi nén hơi và bật mạnh qua kẽ răng (`cài`).
   - `s`: Đầu lưỡi thẳng, xát nhẹ ra âm ""s"" êm (`sān`).
6. **Nhóm âm uốn lưỡi (`zh, ch, sh, r`) — Đầu lưỡi cong lên vòm họng**:
   - `zh`: Cong đầu lưỡi chạm vòm miệng cứng, không bật hơi (`zhōng`).
   - `ch`: **UỐN LƯỠI + BẬT HƠI MẠNH CÙNG LÚC** (`chī`).
   - `sh`: Uốn lưỡi, đẩy luồng hơi dày qua vòm miệng (`shī`).
   - `r`: Uốn lưỡi nhẹ, dây thanh rung, không rung đầu lưỡi như r tiếng Việt (`rén`).

### 2.3. Bảng 4 Thanh điệu & Khinh thanh chuẩn Bắc Kinh
Tiếng Trung có 4 thanh điệu chính mô phỏng đường cao độ từ 1 (thấp nhất) đến 5 (cao nhất):
- **Thanh 1 (阴平 — 55, cao ngang)**: Ký hiệu `ā`. Cao độ giữ ở mức 5 thật cao, phẳng, ngân dài đều hơi (`mā - 妈`).
- **Thanh 2 (阳平 — 35, đi lên)**: Ký hiệu `á`. Cao độ vút từ trung bình lên đỉnh như hỏi ""Hả?"" (`má - 麻`).
- **Thanh 3 (上声 — 214, xuống rồi lên)**: Ký hiệu `ǎ`. Hạ sâu xuống đáy cổ họng rồi mới vồng lên (`mǎ - 马`). Khi đứng trước chữ khác chỉ cần đọc nửa đầu thanh 3 (hạ trầm 21).
- **Thanh 4 (去声 — 51, rơi dứt khoát)**: Ký hiệu `à`. Rơi cực nhanh từ đỉnh cao 5 dậm xuống đáy 1 (`mà - 骂`).
- **Khinh thanh (轻声 — thanh nhẹ)**: Không có dấu, đọc ngắn, nhẹ, thả lỏng cơ miệng (`ma - 吗`, `de - 的`).

### 2.4. Mẹo chiết tự chữ Hán vỡ lòng: Hiểu sâu để nhớ lâu
Thay vì chép phạt chữ Hán cơ học hàng trăm lần, chiết tự bộ thủ giúp bạn ghi nhớ bằng hình ảnh:
- **`好 (hǎo - tốt, đẹp)`**: Gồm bộ **Nữ (女 - người phụ nữ, người mẹ)** đứng bên trái và bộ **Tử (子 - đứa con nhỏ)** bên phải. Người mẹ ôm ấp đứa con thơ trong lòng chính là hình ảnh ấm áp, tốt đẹp, mỹ hảo nhất thế gian!
- **`你 (nǐ - bạn, anh, chị)`**: Gồm bộ **Nhân đứng (亻 - con người)** và chữ **Nhĩ (尔 - ngươi, đối phương)**. Người đứng đối diện ta trong cuộc đối thoại chính là ""bạn"".
- **`谢 (xiè - cảm ơn)`**: Gồm bộ **Ngôn (讠 - lời nói lễ độ)** + chữ **Thân (身 - thân thể)** + chữ **Thốn (寸 - tấc, khoảng cách nhỏ)**. Dùng lời nói chân thành, cúi mình từng tấc tấc để tỏ lòng biết ơn.
- **`再见 (zàijiàn - tạm biệt)`**: `再 (zài)` mang ý nghĩa ""lại một lần nữa"" + `见 (jiàn)` tượng hình đôi chân nâng đôi mắt mở to nhìn thấy. Ghép lại: ""Hẹn gặp lại bạn lần nữa!"".

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbǐao)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **你** | nǐ | Nhĩ | Bạn, anh, chị (ngôi thứ 2) | 你好！很高兴认识你。 *(Chào bạn! Rất vui được quen bạn.)* |
| **您** | nín | Nẫm | Ngài, ông, bà (kính trọng) | 老师，您好！ *(Em kính chào thầy/cô ạ!)* |
| **好** | hǎo | Hảo | Tốt, đẹp, khỏe, được | 今天天气很好。 *(Hôm nay thời tiết rất đẹp.)* |
| **我** | wǒ | Ngã | Tôi, bản thân tôi (ngôi thứ 1) | 我是越南人。 *(Tôi là người Việt Nam.)* |
| **很** | hěn | Hẩn | Rất, lắm (phó từ mức độ) | 我很好，谢谢你！ *(Tôi rất khỏe, cảm ơn bạn!)* |
| **吗** | ma | Ma | ... không? (trợ từ nghi vấn) | 你好吗？ *(Bạn có khỏe không?)* |
| **呢** | ne | Ni | ... thì sao? (trợ từ tỉnh lược) | 我很好，你呢？ *(Tôi rất khỏe, còn bạn thì sao?)* |
| **也** | yě | Dã | Cũng (phó từ liên kết) | 我也是学生。 *(Tôi cũng là sinh viên.)* |
| **谢谢** | xièxie | Tạ Tạ | Cảm ơn | 谢谢老师的帮助！ *(Cảm ơn sự giúp đỡ của thầy/cô!)* |
| **不** | bù | Bất | Không (phó từ phủ định) | 我不喝咖啡。 *(Tôi không uống cà phê.)* |
| **客气** | kèqi | Khách Khí | Khách khí, khách sáo | 不客气，这是我应该做的。 *(Đừng khách sáo, việc nên làm mà.)* |
| **对不起** | duìbuqǐ | Đối Bất Khởi | Xin lỗi | 对不起，我迟到了。 *(Xin lỗi, tôi đến muộn mất rồi.)* |
| **没关系** | méi guānxi | Một Quan Hệ | Không sao đâu, không hề gì | 没关系，请进！ *(Không sao đâu, mời vào!)* |
| **再见** | zàijiàn | Tái Kiến | Tạm biệt, hẹn gặp lại | 明天见，再见！ *(Ngày mai gặp nhé, tạm biệt!)* |
| **老师** | lǎoshī | Lão Sư | Thầy giáo, cô giáo | 王老师是中文老师。 *(Thầy Vương là giáo viên tiếng Trung.)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Trợ từ nghi vấn `吗 (ma)`: Công thức câu hỏi Có/Không
- **Công thức**: `[Câu khẳng định] + 吗？`
- **Ý nghĩa**: Chuyển một câu trần thuật thành câu hỏi nghi vấn Có/Không.
- Không cần thay đổi bất kỳ trật tự từ nào trong câu, không đảo ngữ như tiếng Anh.
- Ví dụ:
  - `你好。` *(Bạn khỏe.)* → `你好吗？` *(Bạn có khỏe không?)*
  - `他是老师。` *(Anh ấy là giáo viên.)* → `他是老师吗？` *(Anh ấy là giáo viên phải không?)*

### 4.2. Trợ từ tỉnh lược `呢 (ne)`: Hỏi ngược lại đối phương
- **Công thức**: `[Danh từ / Đại từ] + 呢？`
- **Ý nghĩa**: ""Còn... thì sao?"". Dùng để hỏi lại vấn đề vừa được đề cập mà không cần nhắc lại cả câu dài.
- Ví dụ:
  - `我很好，你呢？` *(Tôi rất khỏe, còn bạn thì sao?)*
  - `我是越南人，他呢？` *(Tôi là người Việt Nam, còn anh ấy thì sao?)*

### 4.3. Phó từ mức độ `很 (hěn)` & Nguyên tắc cân bằng câu
- Trong tiếng Trung, khi một **tính từ làm vị ngữ** trong câu khẳng định, người ta luôn đặt phó từ `很` đứng trước tính từ đó:
  - `我很好。` *(Tôi khỏe — ĐÚNG).*
  - `我是好。` *(SAI HOÀN TOÀN! Trong tiếng Trung không dùng động từ 是 trước tính từ).*
  - `我好。` *(Nghe cộc cằn, mang sắc thái so sánh ngầm: Tôi thì tốt còn người khác thì không).*
- Khi dịch sang tiếng Việt, trong nhiều trường hợp từ `很` chỉ đóng vai trò cân bằng âm tiết chứ không nhất thiết dịch là ""rất"".

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 我是很好 / 我是高兴 (Dùng động từ 是 trước vị ngữ tính từ)
   ⭕ ĐÚNG: 我很好。 / 我很高兴。
   💡 GIẢI THÍCH: Động từ 是 chỉ dùng để nối hai danh từ, TUYỆT ĐỐI không đứng trước tính từ!

2. ❌ SAI: 也我很好 (Đưa phó từ 也 ra đầu câu theo thói quen tiếng Việt ""Cũng tôi khỏe"")
   ⭕ ĐÚNG: 我也很 好。 (Chủ ngữ + 也 + 很 + Tính từ)
   💡 GIẢI THÍCH: Phó từ 也 trong tiếng Trung luôn luôn phải đứng SAU chủ ngữ và đứng TRƯỚC vị ngữ!

3. ❌ SAI: Đọc thanh 1 (mā, hē, chī) bị hạ thấp ngang như thanh ngang tiếng Việt
   ⭕ ĐÚNG: Giữ cao độ 55 thật cao, phẳng, ngân vang đều đặn.
   💡 GIẢI THÍCH: Thanh ngang tiếng Việt có cao độ trung bình (khoảng 33), trong khi thanh 1 tiếng Trung ở đỉnh cao (55).

4. ❌ SAI: Không bật hơi khi phát âm chữ 请 (qǐng) và 谢谢 (xièxie)
   ⭕ ĐÚNG: Bật hơi xì mạnh ở qǐng; đẩy hơi êm ở xièxie.
   💡 GIẢI THÍCH: Âm q là âm bật hơi mặt lưỡi cực mạnh, nếu không bật hơi sẽ bị nhầm sang âm j.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Buổi sáng ngày đầu tiên tại khuôn viên Đại học Ngôn ngữ Bắc Kinh. Bạn Linh (du học sinh Việt Nam) tình cờ gặp bạn Vương (sinh viên Trung Quốc) trước cửa giảng đường.

- **Linh**: 你好！请问，你好吗？
  - *Pinyin*: Nǐ hǎo! Qǐngwèn, nǐ hǎo ma?
  - *Phân tích*: `你好` (chào bạn — biến điệu ní hǎo) + `请问` (xin hỏi) + `你好吗` (bạn khỏe không?).
  - *Dịch*: Xin chào! Xin hỏi bạn có khỏe không?

- **Wang**: 我很好，谢谢！你呢？
  - *Pinyin*: Wǒ hěn hǎo, xièxie! Nǐ ne?
  - *Phân tích*: `我很好` (tôi rất khỏe) + `谢谢` (cảm ơn) + `你呢` (còn bạn thì sao?).
  - *Dịch*: Mình rất khỏe, cảm ơn bạn nhé! Còn bạn thì sao?

- **Linh**: 我也很好。对不起，请问王老师在吗？
  - *Pinyin*: Wǒ yě hěn hǎo. Duìbuqǐ, qǐngwèn Wáng lǎoshī zài ma?
  - *Phân tích*: `我也很好` (tôi cũng rất khỏe) + `对不起` (xin lỗi) + `王老师` (thầy Vương) + `在吗` (có ở đây không?).
  - *Dịch*: Mình cũng rất khỏe. Xin lỗi, xin hỏi thầy Vương có ở đây không bạn?

- **Wang**: 没关系！王老师在教室。我们一起去吧！
  - *Pinyin*: Méi guānxi! Wáng lǎoshī zài jiàoshì. Wǒmen yìqǐ qù ba!
  - *Phân tích*: `没关系` (không sao đâu) + `在教室` (ở phòng học) + `我们` (chúng ta) + `一起` (cùng nhau) + `去吧` (đi thôi nào).
  - *Dịch*: Không sao đâu! Thầy Vương đang ở trong phòng học đấy. Chúng mình cùng đi nhé!

- **Linh**: 太好了！谢谢你。明天见！
  - *Pinyin*: Tài hǎo le! Xièxie nǐ. Míngtiān jiàn!
  - *Phân tích*: `太好了` (tuyệt quá rồi) + `谢谢你` (cảm ơn bạn) + `明天见` (ngày mai gặp lại).
  - *Dịch*: Tuyệt quá! Cảm ơn bạn nhiều nhé. Hẹn mai gặp lại!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập phát âm & ngữ pháp
1. Hãy ghép Thanh mẫu và Vận mẫu sau thành chữ Pinyin hoàn chỉnh kèm thanh điệu:
   - `m + a` (thanh 1) = ?
   - `h + en` (thanh 3) = ?
   - `x + ie` (thanh 4) = ?
2. Điền trợ từ `吗` hoặc `呢` vào chỗ trống:
   - `你好___？`
   - `我很好，你___？`
3. Sửa lỗi sai trong câu sau: `我是很高兴。`

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - `m + a` (thanh 1) = **mā** (妈 - mẹ)
   - `h + en` (thanh 3) = **hěn** (很 - rất)
   - `x + ie` (thanh 4) = **xiè** (谢 - cảm ơn)
2. **Câu 2**:
   - Câu đầu điền **吗** (`你好吗？` — Bạn khỏe không?).
   - Câu sau điền **呢** (`你呢？` — Còn bạn thì sao?).
3. **Câu 3**: Bỏ động từ `是`, viết lại đúng là: `我很高兴。` (hoặc `我很开心。`).
4. **Câu 4 trắc nghiệm**: Khi ai đó nói ""对不起！"", câu trả lời chuẩn là gì?
   - *Đáp án*: `没关系！` (Méi guānxi - Không sao đâu).
5. **Câu 5 trắc nghiệm**: Chữ ""好"" được ghép từ hai bộ thủ nào?
   - *Đáp án*: Bộ Nữ (女) và bộ Tử (子).

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phát âm thanh điệu zh-CN chuẩn xác và lưu chữ Hán vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Làm chủ hệ thống Pinyin, 4 thanh điệu chuẩn, mẹo chiết tự bộ thủ và các mẫu câu chào hỏi HSK1.",
            "Phân biệt nhóm âm bật hơi p/t/k/q/c/ch và nhóm âm uốn lưỡi zh/ch/sh/r cho người Việt."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter1Id, SeedDataInitializer.ChineseLesson12Id,
            "Bài 1.2: Quy tắc biến điệu, Chữ Hán cơ bản & Giới thiệu tên, quốc tịch",
            "zh-hsk1-bien-dieu-chu-han-gioi-thieu", 2, 35,
            @"# Bài 1.2: Quy tắc biến điệu, Chữ Hán cơ bản & Giới thiệu tên, quốc tịch

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Bài học này giúp bạn tự tin mở lời làm quen, giới thiệu họ tên, quê hương xứ sở bằng tiếng Trung chuẩn mực:
- Nằm lòng **3 quy tắc biến điệu vàng** cốt tử của tiếng Trung: Biến điệu hai thanh 3 liền kề, biến điệu của chữ **不 (bù)** và biến điệu của chữ **一 (yī)**.
- Hiểu tường tận cấu tạo chữ Hán thông qua các bộ thủ thường dùng nhất: Bộ Nhân (`人 / 亻`), Bộ Khẩu (`口`), Bộ Nữ (`女`), Bộ Ngôn (`讠`), Bộ Vi (`囗`).
- Làm chủ câu khẳng định và phủ định chữ **是 (shì - là)**, phân biệt rõ các trường hợp bắt buộc dùng `是` và trường hợp cấm dùng `是`.
- Sử dụng thành thạo các đại từ nghi vấn: **什么 (shénme - cái gì)** và **哪 (nǎ - nào)** để hỏi tên (`你叫什么名字？`) và hỏi quốc tịch (`你是哪国人？`).
- Thực hành màn chào hỏi làm quen tự nhiên, lịch thiệp theo đúng văn hóa giao tiếp Trung Hoa.
- Sửa triệt để các lỗi sai kinh điển: quên biến điệu `bú shì`, `ní hǎo` và tật đảo ngữ theo kiểu tiếng Anh.

---

## 2. 🌱 Nhập môn Pinyin & Chiết tự Chữ Hán dễ nhớ

### 2.1. 3 Quy tắc biến điệu vàng trong tiếng Trung
Nếu không nắm vững quy tắc biến điệu, bạn sẽ nói tiếng Trung rất cứng nhắc và mỏi miệng. Hãy ghi nhớ 3 quy luật sau:

1. **Biến điệu của hai Thanh 3 đứng liền nhau (`3 + 3 → 2 + 3`)**:
   - Khi hai chữ mang thanh 3 đứng sát cạnh nhau, chữ thứ nhất **BẮT BUỘC phải chuyển sang đọc như thanh 2** (hướng giọng đi lên), chữ thứ hai giữ nguyên thanh 3.
   - Ví dụ:
     - `你好`: Viết là `nǐ hǎo`, nhưng phát âm thực tế là **ní hǎo**.
     - `很好`: Viết là `hěn hǎo`, phát âm thực tế là **hén hǎo**.
     - `手表`: Viết là `shǒubiǎo`, phát âm thực tế là **shóubiǎo** *(đồng hồ đeo tay)*.
   - **Chú ý**: Trên văn bản Pinyin chính thức, người ta vẫn giữ nguyên dấu thanh 3 gốc, người đọc phải tự động biến điệu khi phát âm.

2. **Biến điệu của chữ 不 (bù)**:
   - Bản thân chữ `不` có thanh điệu gốc là **Thanh 4 (bù)**.
   - **Khi đứng trước một chữ mang Thanh 4 khác**: Chữ `不` chuyển sang đọc thành **Thanh 2 (bú)**:
     - `不是` (bù + shì) → Đọc là **bú shì** *(không phải)*.
     - `不要` (bù + yào) → Đọc là **bú yào** *(không cần/không muốn)*.
     - `不客气` (bù + kèqi) → Đọc là **bú kèqi** *(đừng khách sáo)*.
   - **Khi đứng trước các chữ mang Thanh 1, Thanh 2, Thanh 3**: Chữ `不` giữ nguyên là **Thanh 4 (bù)**:
     - `不吃 (bù chī)`, `不好 (bù hǎo)`, `不来 (bù lái)`.

3. **Biến điệu của chữ 一 (yī)**:
   - Đứng một mình hoặc đọc số đếm, số thứ tự: giữ nguyên **Thanh 1 (yī)** (`一、二、三`, `第一`).
   - **Khi đứng trước chữ mang Thanh 4**: Đổi thành **Thanh 2 (yí)** (`一个` yí gè, `一共` yí gòng, `一块` yí kuài).
   - **Khi đứng trước chữ mang Thanh 1, Thanh 2, Thanh 3**: Đổi thành **Thanh 4 (yì)** (`一天` yì tiān, `一年` yì nián, `一起` yì qǐ).

### 2.2. Mẹo chiết tự các bộ thủ nền tảng
Hơn 80% chữ Hán là chữ hình thanh (vừa gợi nghĩa vừa gợi âm). Nắm bộ thủ là chìa khóa mở kho báu chữ Hán:
- **Bộ Nhân (人 / 亻 - con người)**:
  - `你 (nǐ - bạn)`: Có người đối diện ta.
  - `他 (tā - anh ấy)`: Người đàn ông khác.
  - `们 (men - số nhiều)`: `亻` người + `门` cửa → Nhiều người cùng tụ họp trước cổng nhà.
  - `休 (xiū - nghỉ ngơi)`: `亻` người tựa lưng vào `木` gốc cây xanh mát mẻ để dưỡng sức.
- **Bộ Khẩu (口 - cái miệng, lời nói, ăn uống)**:
  - `叫 (jiào - gọi tên)`: `口` cái miệng mở ra cất tiếng gọi.
  - `吃 (chī - ăn)` & `喝 (hē - uống)`: Đều cần dùng đến miệng.
  - `吗 (ma)` & `呢 (ne)`: Trợ từ nghi vấn cất tiếng hỏi cuối câu.
- **Bộ Vi (囗 - bờ cõi, bao bọc bốn phía)**:
  - `国 (guó - quốc gia, đất nước)`: Bờ cõi bao bọc bốn phía `囗`, bên trong bảo vệ viên ngọc bích `玉` quý giá của tổ quốc.
- **Chữ 明 (míng - sáng tỏ, thông minh)**: Gồm mặt trời `日` (Nhật) và mặt trăng `月` (Nguyệt) cùng tỏa sáng.

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbǐao)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **是** | shì | Thị | Là, phải, vâng | 我是大学生。 *(Tôi là sinh viên đại học.)* |
| **叫** | jiào | Khiếu | Tên là, gọi là | 我叫王明。 *(Tôi tên là Vương Minh.)* |
| **什么** | shénme | Thập Ma | Cái gì? (đại từ nghi vấn) | 你叫什么名字？ *(Bạn tên là gì?)* |
| **名字** | míngzi | Danh Tự | Tên gọi | 你的名字很好听。 *(Tên bạn nghe rất hay.)* |
| **哪** | nǎ | Na | Nào? (đại từ nghi vấn) | 你是哪国人？ *(Bạn là người nước nào?)* |
| **国** | guó | Quốc | Nước, quốc gia | 中国很大。 *(Trung Quốc rất lớn.)* |
| **人** | rén | Nhân | Người, con người | 他是美国人。 *(Anh ấy là người Mỹ.)* |
| **中国** | Zhōngguó | Trung Quốc | Nước Trung Quốc | 我想去中国旅游。 *(Tôi muốn đi du lịch TQ.)* |
| **越南** | Yuènán | Việt Nam | Nước Việt Nam | 越南菜很好吃。 *(Món ăn Việt Nam rất ngon.)* |
| **美国** | Měiguó | Mỹ Quốc | Nước Mỹ, Hoa Kỳ | 史密斯是美国人。 *(Smith là người Mỹ.)* |
| **认识** | rènshi | Nhận Thức | Quen biết, nhận biết | 认识你我很高兴！ *(Quen biết bạn tôi rất vui!)* |
| **高兴** | gāoxìng | Cao Hứng | Vui vẻ, mừng rỡ | 今天大家都很高兴。 *(Hôm nay mọi người đều vui.)* |
| **学生** | xuésheng | Học Sinh | Học sinh, sinh viên | 我们都是汉语学生。 *(Chúng tôi đều là học viên tiếng Trung.)* |
| **他 / 她** | tā | Tha | Anh ấy / Cô ấy | 他是我的同学。 *(Cậu ấy là bạn học của tôi.)* |
| **我们** | wǒmen | Ngã Môn | Chúng tôi, chúng ta | 我们一起学汉语。 *(Chúng ta cùng học tiếng Trung nhé.)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Câu chữ `是 (shì)`: Khẳng định & Phủ định
- **Khẳng định**: `[Chủ ngữ] + 是 + [Danh từ / Tân ngữ]`
  - Ý nghĩa: ""A là B"".
  - Ví dụ: `我是越南人。` *(Tôi là người Việt Nam).*
  - `她是我的老师。` *(Cô ấy là cô giáo của tôi).*
- **Phủ định**: `[Chủ ngữ] + 不是 (bú shì) + [Danh từ / Tân ngữ]`
  - Ý nghĩa: ""A không phải là B"". Nhớ biến điệu chữ `不` thành `bú` trước thanh 4 `shì`.
  - Ví dụ: `我不是美国人。` *(Tôi không phải là người Mỹ).*
- **LƯU Ý CỐT TỬ**: Động từ `是` KHÔNG BAO GIỜ đứng trước tính từ:
  - Nói `我是高兴` là SAI HOÀN TOÀN! Phải nói: `我很高兴` *(Tôi rất vui).*

### 4.2. Đại từ nghi vấn `什么 (shénme - cái gì)` & `哪 (nǎ - nào)`
Khác với tiếng Anh (phải đảo từ để hỏi lên đầu câu: *What is your name?*), tiếng Trung **giữ nguyên trật tự từ tự nhiên**:
- Chỗ nào cần hỏi thì đặt đại từ nghi vấn vào đúng vị trí của câu trả lời tương ứng:
  - Khẳng định: `我叫 [明]。` *(Tôi tên là Minh).*
  - Nghi vấn: `你叫 [什么] 名字？` *(Bạn tên là tên gì?)*.
- Mẫu câu hỏi quốc tịch: `你是 [哪] 国人？`
  - Trả lời: `我是 [越南] 人。`

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 发âm bù shì (Không biến điệu chữ 不 trước thanh 4 是)
   ⭕ ĐÚNG: bú shì (Biến điệu thanh 2 bú dứt khoát)
   💡 GIẢI THÍCH: Khi 不 đứng trước thanh 4, quy tắc bất biến là phải đọc thành bú.

2. ❌ SAI: 我是越南 (Tôi là Việt Nam — Thiếu chữ 人)
   ⭕ ĐÚNG: 我是越南人。 (Tôi là người Việt Nam)
   💡 GIẢI THÍCH: Tên quốc gia phải thêm chữ 人 (rén) ở cuối mới mang nghĩa người mang quốc tịch đó.

3. ❌ SAI: 什么你叫？ (Đảo từ nghi vấn 什么 lên đầu câu theo ngữ pháp tiếng Anh)
   ⭕ ĐÚNG: 你叫什么名字？ (Giữ nguyên trật tự SVO)
   💡 GIẢI THÍCH: Đại từ nghi vấn trong tiếng Trung nằm ở vị trí thông tin cần hỏi, không đảo lên đầu câu.

4. ❌ SAI: Nhầm lẫn giữa 哪 (nǎ - nào?) và 那 (nà - kia, đó)
   ⭕ ĐÚNG: 哪国人 (nǎ guó rén - người nước nào?) vs 那个 (nà ge - cái kia).
   💡 GIẢI THÍCH: Chữ 哪 có bộ Khẩu (口) ở trước dùng để hỏi (thanh 3), còn 那 là đại từ chỉ định (thanh 4).
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Buổi gặp gỡ đầu tiên tại Câu lạc bộ Giao lưu Văn hóa Quốc tế. Bạn Minh (Việt Nam) làm quen với bạn Lý Na (Trung Quốc).

- **Minh**: 你好！我叫明。请问你叫什么名字？
  - *Pinyin*: Nǐ hǎo! Wǒ jiào Míng. Qǐngwèn nǐ jiào shénme míngzi?
  - *Phân tích*: `我叫明` (tôi tên là Minh) + `请问` (xin hỏi) + `你叫什么名字` (bạn tên là gì?).
  - *Dịch*: Xin chào! Mình tên là Minh. Xin hỏi bạn tên là gì?

- **Li Na**: 你好，明！我叫李娜。很高兴认识你！
  - *Pinyin*: Nǐ hǎo, Míng! Wǒ jiào Lǐ Nà. Hěn gāoxìng rènshi nǐ!
  - *Phân tích*: `我叫李娜` (tôi là Lý Na) + `很高兴` (rất vui) + `认识你` (được quen bạn).
  - *Dịch*: Chào Minh! Mình tên là Lý Na. Rất vui được làm quen với bạn!

- **Minh**: 认识你我也很高兴。你是中国人吗？
  - *Pinyin*: Rènshi nǐ wǒ yě hěn gāoxìng. Nǐ shì Zhōngguó rén ma?
  - *Phân tích*: `我也很高兴` (mình cũng rất vui) + `你是中国人吗` (bạn là người Trung Quốc phải không?).
  - *Dịch*: Quen bạn mình cũng rất vui. Bạn là người Trung Quốc phải không?

- **Li Na**: 是的，我是中国人。你是哪国人？
  - *Pinyin*: Shì de, wǒ shì Zhōngguó rén. Nǐ shì nǎ guó rén?
  - *Phân tích*: `是的` (đúng vậy) + `我是中国人` (tôi là người Trung Quốc) + `你是哪国人` (bạn là người nước nào?).
  - *Dịch*: Đúng thế, mình là người Trung Quốc. Còn bạn là người nước nào?

- **Minh**: 我是越南人。我是大学生，在学习汉语。
  - *Pinyin*: Wǒ shì Yuènán rén. Wǒ shì dàxuéshēng, zài xuéxí Hànyǔ.
  - *Phân tích*: `越南人` (người Việt Nam) + `大学生` (sinh viên đại học) + `在学习汉语` (đang học tiếng Trung).
  - *Dịch*: Mình là người Việt Nam. Mình là sinh viên đại học, đang học tiếng Trung.

- **Li Na**: 你的汉语很好！加油！
  - *Pinyin*: Nǐ de Hànyǔ hěn hǎo! Jiāyóu!
  - *Phân tích*: `你的汉语` (tiếng Trung của bạn) + `很好` (rất tốt) + `加油` (cố lên nhé!).
  - *Dịch*: Tiếng Trung của bạn tốt quá! Cố lên nhé!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập biến điệu & mẫu câu
1. Hãy ghi lại cách phát âm thực tế cho các cụm từ sau:
   - `你好 (nǐ hǎo)` → ___
   - `不是 (bù shì)` → ___
   - `一个人 (yī gè rén)` → ___
2. Dịch các câu sau sang tiếng Trung:
   - ""Tôi là người Việt Nam, tôi không phải là người Mỹ.""
   - ""Bạn tên là gì? — Tôi tên là Minh.""
3. Chữ `休` được ghép từ những bộ thủ nào và có ý nghĩa biểu ý là gì?

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - `你好` đọc thành: **ní hǎo** (2 + 3).
   - `不是` đọc thành: **bú shì** (2 + 4).
   - `一个人` đọc thành: **yí gè rén** (2 + 4 + 2).
2. **Câu 2**:
   - `我是越南人，我不是美国人。` *(Wǒ shì Yuènán rén, wǒ bú shì Měiguó rén).*
   - `你叫什么名字？—— 我叫明。` *(Nǐ jiào shénme míngzi? — Wǒ jiào Míng).*
3. **Câu 3**: Chữ `休` gồm bộ Nhân (`亻` - con người) tựa vào bộ Mộc (`木` - cái cây), mang ý nghĩa người lao động dừng chân tựa vào gốc cây nghỉ ngơi.
4. **Câu 4 trắc nghiệm**: Điền từ thích hợp: `你是___国人？`
   - *Đáp án*: Điền **哪 (nǎ)**.
5. **Câu 5 trắc nghiệm**: Tại sao nói `我是很高兴` lại sai ngữ pháp?
   - *Đáp án*: Vì động từ `是` chỉ dùng nối danh từ với danh từ, không đứng trước vị ngữ tính từ.

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phát âm thanh điệu zh-CN chuẩn xác và lưu chữ Hán vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Áp dụng quy tắc biến điệu thanh 3, 不, 一 và nhận diện các bộ thủ chữ Hán HSK1.",
            "Thành thạo mẫu câu hỏi tên, quốc tịch với 叫, 什么, 哪 và câu khẳng/phủ định chữ 是."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter2Id, SeedDataInitializer.ChineseLesson21Id,
            "Bài 2.1: Số đếm, Ngày tháng & Thời gian",
            "zh-hsk1-so-dem-ngay-thang-thoi-gian", 1, 35,
            @"# Bài 2.1: Số đếm, Ngày tháng & Thời gian

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Làm chủ thời gian là chìa khóa để quản lý cuộc sống, lên lịch hẹn và học tập hiệu quả trong môi trường Hoa ngữ:
- Đếm thành thạo dãy số từ 0 đến 100 bằng tiếng Trung, nắm vững cách đọc ghép số hàng chục và hàng lẻ.
- Phân biệt sâu sắc và triệt để sự khác nhau giữa **二 (èr)** và **两 (liǎng)** (lỗi sai kinh điển của 90% người mới học).
- Nắm bắt quy luật thời gian tư duy Trung Hoa: **TỪ LỚN ĐẾN NHỎ** (`Năm → Tháng → Ngày → Thứ → Buổi → Giờ → Phút`).
- Đọc chuẩn xác ngày tháng năm (`年, 月, 号/日`) và các thứ trong tuần (`星期一 ~ 星期天`).
- Hỏi đáp giờ giấc hiện tại bằng cấu trúc **现在几点了？**, diễn đạt giờ chẵn (**点**), phút (**分**), rưỡi (**半**), khắc 15 phút (**刻**).
- Đặt trạng ngữ chỉ thời gian đúng vị trí trong câu (trước động từ), khắc phục hoàn toàn thói quen đặt thời gian cuối câu của tiếng Việt.

---

## 2. 🌱 Nhập môn Pinyin & Chiết tự Chữ Hán dễ nhớ

### 2.1. Bản chất hệ số đếm 0 — 100 trong tiếng Trung
Số đếm tiếng Trung cực kỳ quy tắc và logic, tương đồng lớn với âm Hán Việt:
- `0: 零 (líng)` | `1: 一 (yī)` | `2: 二 (èr)` | `3: 三 (sān)` | `4: 四 (sì)` | `5: 五 (wǔ)`
- `6: 六 (liù)` | `7: 七 (qī)` | `8: 八 (bā)` | `9: 九 (jiǔ)` | `10: 十 (shí)`
- **Quy tắc ghép số hàng chục**:
  - Từ 11 đến 19: `十 + Số lẻ` (`十一` shíyī = 11, `十五` shíwǔ = 15).
  - Các số tròn chục: `Số hàng chục + 十` (`二十` èrshí = 20, `五十` wǔshí = 50, `九十` jiǔshí = 90).
  - Số ghép đầy đủ: `二十五` (25), `八十八` (88), `九十九` (99), `一百` (yìbǎi = 100).
- **Phân biệt phát âm nhạy cảm giữa 四 (sì - 4) và 十 (shí - 10)**:
  - `四 (sì)`: Âm đầu lưỡi thẳng, không uốn lưỡi, phát âm thanh 4 rơi mạnh dứt khoát.
  - `十 (shí)`: Âm **uốn lưỡi**, vòm lưỡi cong lên chạm ngạc cứng, phát âm thanh 2 đi lên.

### 2.2. Phân biệt sống còn giữa 二 (èr) và 两 (liǎng)
Cả hai từ đều mang nghĩa là số ""2"", nhưng phạm vi sử dụng tách bạch rõ ràng:
1. **Dùng 二 (èr)**:
   - Dùng khi **đếm số tự nhiên, đếm số thứ tự**: `一、二、三`, `第二 (thứ hai)`.
   - Dùng trong số phòng, số nhà, số điện thoại, biển số xe: `房间号 202` (èr líng èr).
   - Dùng trong ngày tháng, thứ trong tuần: `二月 (tháng 2)`, `十二号 (ngày 12)`, `星期二 (thứ Ba)`.
   - Dùng trong số thập phân, phân số: `0.2 (líng diǎn èr)`.
2. **BẮT BUỘC dùng 两 (liǎng)**:
   - Dùng khi **đứng trước LƯỢNG TỪ**: `两个人 (2 người)`, `两本书 (2 cuốn sách)`, `两块钱 (2 tệ)`.
   - Dùng khi **nói giờ giấc**: `两点 (2 giờ)`, `两点半 (2 giờ rưỡi)`. **TUYỆT ĐỐI KHÔNG NÓI 二点!**
   - Đứng trước đơn vị trăm, nghìn, vạn: `两百 (200)`, `两千 (2.000)`.

### 2.3. Tư duy thời gian Trung Hoa: Nguyên lý TỪ LỚN ĐẾN NHỎ
Văn hóa phương Đông coi trọng chỉnh thể vũ trụ bao quát trước rồi mới đi vào chi tiết nhỏ bên trong:
- **Tiếng Việt**: `8 giờ sáng, ngày 12, tháng 10, năm 2026` *(Nhỏ → Lớn)*.
- **Tiếng Trung**: `2026年 10月 12号 上午 8点` *(Lớn → Nhỏ: Năm → Tháng → Ngày → Buổi → Giờ)*.
- **Thứ trong tuần**: `星期 + Con số (1 đến 6)`:
  - `星期一 (xīngqī yī)`: Thứ Hai (ngày thứ 1 trong tuần làm việc).
  - `星期二 (xīngqī èr)`: Thứ Ba.
  - `星期三 (xīngqī sān)`: Thứ Tư.
  - `星期六 (xīngqī liù)`: Thứ Bảy.
  - `星期天 / 星期日 (xīngqī tiān / rì)`: Chủ Nhật (không nói 星期七!).

### 2.4. Mẹo chiết tự chữ Hán thời gian
- **`日 (rì - mặt trời, ngày)`**: Hình mặt trời tròn có tia sáng ở giữa. Trong khẩu ngữ thường dùng chữ **`号 (hào)`** để chỉ ngày.
- **`月 (yuè - mặt trăng, tháng)`**: Hình vầng trăng khuyết. Một vòng tuần hoàn mặt trăng là một tháng.
- **`年 (nián - năm)`**: Người xưa gánh bó lúa trĩu hạt sau mùa gặt. Một mùa gặt trôi qua là trọn vẹn một năm.
- **`分 (fēn - phút, chia ra)`**: Gồm bộ **Bát (八 - chia đôi)** ở trên và bộ **Đao (刀 - con dao)** ở dưới. Con dao bổ đôi, chia nhỏ thời gian ra từng phút.

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbǐao)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **现在** | xiànzài | Hiện Tại | Bây giờ, hiện nay | 现在几点了？ *(Bây giờ là mấy giờ rồi?)* |
| **点** | diǎn | Điểm | Giờ, tiếng đồng hồ | 现在上午十点。 *(Bây giờ là 10 giờ sáng.)* |
| **分** | fēn | Phân | Phút | 八点十五分。 *(8 giờ 15 phút.)* |
| **半** | bàn | Bán | Rưỡi, một nửa (30 phút) | 九点半见！ *(9 rưỡi gặp nhé!)* |
| **刻** | kè | Khắc | Khắc (15 phút) | 差一刻十点。 *(10 giờ kém 15 phút.)* |
| **年** | nián | Niên | Năm | 2026年。 *(Năm 2026.)* |
| **月** | yuè | Nguyệt | Tháng | 十月。 *(Tháng Mười.)* |
| **号 / 日** | hào / rì | Hào / Nhật | Ngày (khẩu ngữ / viết) | 今天是五号。 *(Hôm nay là ngày mùng 5.)* |
| **星期** | xīngqī | Tinh Kỳ | Thứ, tuần | 星期三见！ *(Thứ Tư gặp nhé!)* |
| **早上** | zǎoshang | Tảo Thượng | Buổi sáng sớm | 早上六点起床。 *(6 giờ sáng thức dậy.)* |
| **上午** | shàngwǔ | Thượng Ngọ | Buổi sáng (trước 12h) | 上午九点有中文课。 *(9h sáng có tiết tiếng Trung.)* |
| **中午** | zhōngwǔ | Trung Ngọ | Buổi trưa | 中午十二点吃午饭。 *(12h trưa ăn cơm trưa.)* |
| **下午** | xiàwǔ | Hạ Ngọ | Buổi chiều | 下午三点去图书馆。 *(3h chiều đi thư viện.)* |
| **晚上** | wǎnshang | Vãn Thượng | Buổi tối | 晚上看书。 *(Buổi tối đọc sách.)* |
| **几** | jǐ | Kỷ | Mấy? (dưới 10) | 几点？几月？ *(Mấy giờ? Tháng mấy?)* |
| **什么时候** | shénme shíhou | Thập Ma Thời Hậu | Khi nào, lúc nào? | 你什么时候来？ *(Khi nào bạn đến?)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Mẫu câu hỏi & trả lời giờ giấc
- **Câu hỏi**: `现在几点了？` *(Xiànzài jǐ diǎn le? — Bây giờ mấy giờ rồi?)*
  - `几 (jǐ)`: Đại từ nghi vấn dùng hỏi số lượng nhỏ, giờ giấc.
  - `了 (le)`: Trợ từ ngữ khí cuối câu biểu thị trạng thái thời gian đã chuyển biến sang mốc mới.
- **Công thức trả lời**: `现在 + [Buổi] + [Số đếm] 点 + [Số đếm] 分。`
  - Ví dụ: `现在上午八点三十分。` *(Bây giờ là 8 giờ 30 phút sáng).*
  - `现在下午两点半。` *(Bây giờ là 2 giờ rưỡi chiều — Nhớ dùng 两点, không dùng 二点!).*

### 4.2. Vị trí của Trạng ngữ chỉ thời gian trong câu
Đây là điểm người Việt nói tiếng Trung hay bị ""ngọng ngữ pháp"" nhất:
- Trong tiếng Việt: *""Tôi đi siêu thị [vào sáng mai]""* (Thời gian hay vứt ở cuối câu).
- **Trong tiếng Trung: Trạng ngữ thời gian TUYỆT ĐỐI KHÔNG ĐƯỢC ĐẶT Ở CUỐI CÂU!**
- Thời gian luôn đứng TRƯỚC động từ, có thể đứng sau hoặc đứng trước chủ ngữ:
  - `[Chủ ngữ] + [Thời gian] + [Động từ] + [Tân ngữ]`
  - `[Thời gian] + [Chủ ngữ] + [Động từ] + [Tân ngữ]`
  - Ví dụ:
    - `我明天上午去学校。` *(Sáng mai tôi đến trường — ĐÚNG).*
    - `明天上午我去学校。` *(Sáng mai tôi đến trường — ĐÚNG).*
    - `我去学校明天上午。` *(SAI HOÀN TOÀN!).*

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 我去图书馆明天下午 (Đặt thời gian ở cuối câu theo thói quen tiếng Việt)
   ⭕ ĐÚNG: 我明天下午去图书馆。 hoặc 明天下午我去图书馆。
   💡 GIẢI THÍCH: Trạng ngữ chỉ thời gian trong tiếng Trung luôn luôn đứng TRƯỚC động từ chính!

2. ❌ SAI: 现在二点了 (Dùng 二 để nói 2 giờ)
   ⭕ ĐÚNG: 现在两点了。 (Dùng 两 khi đi với đơn vị giờ 点)
   💡 GIẢI THÍCH: Đơn vị giờ 点 có tính chất như một lượng từ, bắt buộc dùng 两点, tuyệt đối không nói 二点.

3. ❌ SAI: 星期一 là thứ Nhất (nghĩ nhầm là thứ 1 kiểu tiếng Việt)
   ⭕ ĐÚNG: 星期一 là Thứ Hai; 星期二 là Thứ Ba.
   💡 GIẢI THÍCH: Thứ trong tuần tiếng Trung bắt đầu từ số 1 (thứ Hai), luôn lệch 1 số so với tiếng Việt.

4. ❌ SAI: 12号 10月 2026年 (Nói ngày tháng từ nhỏ đến lớn)
   ⭕ ĐÚNG: 2026年 10月 12号 (Trật tự lớn trước: Năm → Tháng → Ngày).
   💡 GIẢI THÍCH: Tư duy ngôn ngữ Trung Hoa luôn sắp xếp trật tự thời gian từ đơn vị lớn nhất đến nhỏ nhất.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Giờ giải lao tại sảnh ký túc xá. Bạn An hỏi bạn Minh về ngày thi tiếng Trung và hẹn giờ cùng lên thư viện ôn bài.

- **An**: 请问，今天几月几号？星期几？
  - *Pinyin*: Qǐngwèn, jīntiān jǐ yuè jǐ hào? Xīngqī jǐ?
  - *Phân tích*: `请问` (xin hỏi) + `今天` (hôm nay) + `几月几号` (tháng mấy ngày mấy) + `星期几` (thứ mấy?).
  - *Dịch*: Xin hỏi bạn, hôm nay là ngày mấy tháng mấy? Thứ mấy thế?

- **Minh**: 今天十月十二号，星期三。
  - *Pinyin*: Jīntiān shí yuè shí'èr hào, xīngqī sān.
  - *Phân tích*: `十月` (tháng 10) + `十二号` (ngày 12) + `星期三` (thứ Tư).
  - *Dịch*: Hôm nay là ngày 12 tháng 10, thứ Tư.

- **An**: 现在几点了？
  - *Pinyin*: Xiànzài jǐ diǎn le?
  - *Phân tích*: `现在` (bây giờ) + `几点了` (mấy giờ rồi?).
  - *Dịch*: Bây giờ mấy giờ rồi bạn nhỉ?

- **Minh**: 现在上午九点半。我们下午两点去图书馆，好吗？
  - *Pinyin*: Xiànzài shàngwǔ jiǔ diǎn bàn. Wǒmen xiàwǔ liǎng diǎn qù túshūguǎn, hǎo ma?
  - *Phân tích*: `上午九点半` (9h30 sáng) + `我们` (chúng ta) + `下午两点` (2h chiều) + `去图书馆` (đi thư viện) + `好吗` (được không?).
  - *Dịch*: Bây giờ là 9 giờ rưỡi sáng. Chiều nay 2 giờ chúng mình đến thư viện nhé, được không?

- **An**: 好的！太好了，下午两点见！
  - *Pinyin*: Hǎo de! Tài hǎo le, xiàwǔ liǎng diǎn jiàn!
  - *Phân tích*: `好的` (được thôi) + `太好了` (tuyệt quá) + `下午两点见` (2h chiều gặp nhau nhé).
  - *Dịch*: Được chứ! Tuyệt quá, hẹn bạn 2 giờ chiều gặp nhé!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập số đếm & thời gian
1. Chọn `二` hoặc `两` điền vào chỗ trống:
   - `现在___点半。`
   - `今天十___月___号。`
   - `我买___本书。`
2. Viết lại các cụm thời gian sau theo chuẩn trật tự tiếng Trung:
   - ""3 giờ chiều"" → ___
   - ""Ngày 20 tháng 11"" → ___
   - ""Thứ Bảy"" → ___
3. Dịch câu sau: *""Ngày mai 8 giờ sáng tôi đến trường.""*

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - Điền **两** (`两点半` — 2 giờ rưỡi).
   - Điền **二** (`十二月二号` — Ngày 2 tháng 12).
   - Điền **两** (`两本书` — 2 cuốn sách).
2. **Câu 2**:
   - 3 giờ chiều: `下午三点` (xiàwǔ sān diǎn).
   - Ngày 20 tháng 11: `十一月二十号` (shíyī yuè èrshí hào).
   - Thứ Bảy: `星期六` (xīngqī liù).
3. **Câu 3**: `我明天上午八点去学校。` (Thời gian phải đứng trước động từ `去`).
4. **Câu 4 trắc nghiệm**: Khác biệt giữa `四 (sì)` và `十 (shí)` trong phát âm là gì?
   - *Đáp án*: `sì` là âm đầu lưỡi thẳng mang thanh 4 rơi mạnh; `shí` là âm uốn lưỡi mang thanh 2 đi lên.
5. **Câu 5 trắc nghiệm**: Chủ nhật trong tiếng Trung nói là gì?
   - *Đáp án*: `星期天` hoặc `星期日` (không dùng 星期七).

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phát âm thanh điệu zh-CN chuẩn xác và lưu chữ Hán vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Đếm số, nói ngày tháng năm, thứ trong tuần và giờ giấc theo chuẩn trật tự tiếng Trung.",
            "Phân biệt 二 vs 两, 四 (sì) vs 十 (shí) và đặt trạng ngữ thời gian đúng vị trí trước động từ."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter2Id, SeedDataInitializer.ChineseLesson22Id,
            "Bài 2.2: Gia đình, Hỏi giá cả & Mua sắm đơn giản",
            "zh-hsk1-gia-dinh-hoi-gia-mua-sam", 2, 35,
            @"# Bài 2.2: Gia đình, Hỏi giá cả & Mua sắm đơn giản

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Bài học này trang bị cho bạn năng lực tự tin đi chợ, siêu thị mua sắm, trả giá hoa quả và giới thiệu người thân trong gia đình:
- Giới thiệu các thành viên gia đình bằng động từ sở hữu/tồn tại **有 (yǒu)**, phân biệt rõ lượng từ chuyên biệt **口 (kǒu)** với lượng từ phổ biến **个 (gè)**.
- Khắc ghi quy tắc bất di bất dịch: **Phủ định của 有 DUY NHẤT là 没有 (méiyǒu)**, tuyệt đối không bao giờ dùng `不有`.
- Hiểu và làm chủ cấu trúc bắt buộc: `Số từ + Lượng từ + Danh từ` (`三口人`, `两本书`, `五个苹果`).
- Tự tin đi mua sắm, hỏi giá tiền bằng cụm từ **多少钱 (duōshao qián)**, nắm vững đơn vị tiền tệ Trung Quốc (**块 / 元 / 角 / 毛**).
- Phân biệt cặp đại từ hỏi số lượng: **几 (jǐ - dưới 10, bắt buộc có lượng từ)** và **多少 (duōshao - không giới hạn số lượng)**.
- Sử dụng cấu trúc cảm thán **太……了 (tài ... le)** để biểu lộ cảm xúc và mặc cả giá (`太贵了！便宜一点儿吧`).

---

## 2. 🌱 Nhập môn Pinyin & Chiết tự Chữ Hán dễ nhớ

### 2.1. Bản chất động từ 有 (yǒu) & Quy tắc phủ định độc nhất 没有
Trong tiếng Trung, động từ `有` đảm nhận cả hai vai trò:
1. **Biểu thị sở hữu**: ""Có"" cái gì đó (`我有汉语书` - Tôi có sách tiếng Trung).
2. **Biểu thị sự tồn tại**: ""Ở đâu có cái gì"" (`学校里有很多人` - Trong trường có rất nhiều người).
- **QUY TẮC VÀNG VỀ PHỦ ĐỊNH**:
  - Đối với đại đa số động từ tiếng Trung, ta dùng phó từ phủ định `不 (bù)` (`不去`, `不吃`, `不是`).
  - **NHƯNG ĐỐI VỚI ĐỘNG TỪ 有: DUY NHẤT VÀ CHỈ CÓ THỂ DÙNG 没 (méi) ĐỂ PHỦ ĐỊNH!**
  - Phủ định là **没有 (méiyǒu)**.
  - **TUYỆT ĐỐI CẤM KỴ NÓI ""不有"" (bùyǒu)!** Đây là lỗi ngữ pháp tối kỵ khiến người bản xứ nhận ra ngay bạn học vẹt.

### 2.2. Lượng từ trong tiếng Trung: Cầu nối bắt buộc giữa Số từ và Danh từ
Khác với tiếng Việt có thể nói gộp (""ba người"", ""hai sách""), tiếng Trung có quy tắc ngữ pháp thép:
$$\text{Cấu trúc} = \text{Số từ} + \mathbf{\text{Lượng từ}} + \text{Danh từ}$$
- Không bao giờ được phép bỏ quên lượng từ ở giữa!
- **Các lượng từ cơ bản HSK1**:
  - **`个 (gè)`**: Lượng từ phổ biến nhất, dùng cho người, quả táo, đồ vật chung (`一个人`, `两个苹果`, `三个月`).
  - **`口 (kǒu)`**: Lượng từ chuyên dùng đếm nhân khẩu trong gia đình (`四口人` - 4 miệng ăn trong nhà).
  - **`本 (běn)`**: Lượng từ dành cho sách, vở, từ điển có gáy đóng (`两本书`).
  - **`块 (kuài)`**: Đơn vị đếm tiền tệ trong khẩu ngữ (`十块钱` - 10 tệ).

### 2.3. Đơn vị tiền tệ Trung Quốc & Cân nặng (市斤)
- **Đơn vị tiền**:
  - Khẩu ngữ: Dùng **块 (kuài - tệ)** và **毛 (máo - hào = 1/10 tệ)**.
  - Văn viết / Bảng giá siêu thị: Dùng **元 (yuán - tệ)** và **角 (jiǎo - hào)**.
  - Ví dụ: `5.50 tệ` đọc khẩu ngữ là `五块五 (wǔ kuài wǔ)`.
- **Đơn vị cân nặng CỰC KỲ QUAN TRỌNG KHI MUA HOA QUẢ**:
  - Ở Trung Quốc, đơn vị cân nặng thông dụng ngoài chợ là **斤 (jīn — Thị cân)**.
  - **1 斤 (cân Trung Quốc) = 500 gram (nửa ký)**!
  - 1 kg chuẩn quốc tế được gọi là **公斤 (gōngjīn)**.
  - Rất nhiều người Việt khi sang Trung Quốc mua hoa quả thấy ghi `8块一斤` tưởng là 8 tệ/kg, thực chất là 8 tệ/500g!

### 2.4. Mẹo chiết tự chữ Hán gia đình & mua sắm
- **`家 (jiā - gia đình, nhà)`**: Gồm bộ **Miên (宀 - mái nhà)** che chở ở trên và chữ **Thỉ (豕 - con lợn/heo)** ở dưới. Thời cổ đại, dưới mái nhà có nuôi gia súc heo gà chứng tỏ gia đình no ấm, sung túc.
- **`爸 (bà - cha, bố)`**: Gồm bộ **Phụ (父 - người cha cầm gậy làm việc)** ở trên và chữ **Ba (巴 - gợi âm)** ở dưới.
- **`妈 (mā - mẹ)`**: Gồm bộ **Nữ (女 - người phụ nữ)** gợi nghĩa và chữ **Mã (马 - con ngựa)** gợi âm `mā`.
- **`买 (mǎi - mua)` vs `卖 (mài - bán)`**:
  - `买 (mǎi)`: Thanh 3 xuống-lên, ít nét hơn (bỏ tiền ra tậu hàng về).
  - `卖 (mài)`: Thanh 4 rơi dứt khoát, có thêm chữ **Thập (十)** trên đầu (chất thêm hàng hóa lên xe mang đi bán).
- **`钱 (qián - tiền)`**: Gồm bộ **Kim (钅 - kim loại, tiền xu)** và hai mũi giáo **Tiễn (戋)** tượng trưng cho sự tranh giành của cải.

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbǐao)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **家** | jiā | Gia | Nhà, gia đình | 我爱我的家。 *(Tôi yêu gia đình tôi.)* |
| **有** | yǒu | Hữu | Có (sở hữu, tồn tại) | 我家有四口人。 *(Nhà tôi có 4 người.)* |
| **没有** | méiyǒu | Một Hữu | Không có (phủ định) | 我没有哥哥。 *(Tôi không có anh trai.)* |
| **口** | kǒu | Khẩu | Lượng từ nhân khẩu | 你家有几口人？ *(Nhà bạn có mấy người?)* |
| **爸爸** | bàba | Ba Ba | Bố, cha | 我爸爸是医生。 *(Bố tôi là bác sĩ.)* |
| **妈妈** | māma | Ma Ma | Mẹ | 我妈妈做饭很好吃。 *(Mẹ tôi nấu ăn rất ngon.)* |
| **哥哥** | gēge | Ca Ca | Anh trai | 我哥哥在北京工作。 *(Anh trai tôi làm việc ở BK.)* |
| **姐姐** | jiějie | Tỷ Tỷ | Chị gái | 这是我姐姐。 *(Đây là chị gái tôi.)* |
| **弟弟** | dìdi | Đệ Đệ | Em trai | 我弟弟是中学生。 *(Em trai tôi là học sinh cấp 2.)* |
| **妹妹** | mèimei | Muội Muội | Em gái | 我妹妹很可爱。 *(Em gái tôi rất đáng yêu.)* |
| **多少钱** | duōshao qián | Đa Thiểu Tiền | Bao nhiêu tiền? | 这个苹果多少钱一斤？ *(Táo này bao nhiêu tiền 1 cân?)* |
| **块 / 元** | kuài / yuán | Khối / Nguyên | Đồng tệ (khẩu ngữ / viết) | 十五块钱。 *(15 tệ.)* |
| **太……了** | tài ... le | Thái ... Liễu | Quá ... rồi! (cảm thán) | 太贵了！ *(Đắt quá rồi!)* |
| **贵** | guì | Quý | Đắt, đắt đỏ | 这件衣服太贵了。 *(Bộ quần áo này đắt quá.)* |
| **便宜** | piányi | Tiện Nghi | Rẻ | 可以便宜一点儿吗？ *(Có thể rẻ hơn một chút không?)* |
| **苹果** | píngguǒ | Bình Quả | Quả táo | 我买两斤苹果。 *(Tôi mua 2 cân táo - 1kg.)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Mẫu câu hỏi thành viên gia đình: `你家有几口人？`
- **Công thức**: `[Chủ ngữ] + 家 + 有 + 几 + 口 + 人？`
- **Trả lời**: `我家有 + [Số] + 口人：[Liệt kê thành viên] 和我。`
- `几 (jǐ)` dùng để hỏi số lượng nhỏ thường dưới 10.
- `和 (hé - và)`: Từ nối dùng để liệt kê người cuối cùng trong danh sách.
- Ví dụ: `我家有四口人：爸爸、妈妈、一个姐姐和我。`

### 4.2. Mẫu câu hỏi giá & Mặc cả trong mua sắm
- **Hỏi giá**: `这个 [Danh từ] 多少钱 (一斤 / 一个)？`
  - Ví dụ: `这个西瓜多少钱一斤？` *(Dưa hấu này bao nhiêu tiền một cân?)*
- **Cấu trúc cảm thán**: `太 + [Tính từ] + 了！`
  - `太贵了！` *(Tài guì le! — Đắt quá rồi!)*
  - `太好了！` *(Tài hǎo le! — Tuyệt vời quá rồi!)*
- **Mẫu câu mặc cả lịch sự**: `便宜一点儿吧！` *(Piányi yìdiǎnr ba! — Rẻ hơn một chút đi mà!).*
  - `一点儿 (yìdiǎnr)`: Một chút, một ít.
  - `吧 (ba)`: Trợ từ ngữ khí cuối câu biểu thị sự thương lượng, đề nghị nhẹ nhàng.

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 我不有弟弟 (Dùng 不 để phủ định động từ 有)
   ⭕ ĐÚNG: 我没有弟弟。
   💡 GIẢI THÍCH: Động từ 有 chỉ có dạng phủ định duy nhất là 没有, cấm tuyệt đối không dùng 不有.

2. ❌ SAI: 我买二斤苹果 (Bỏ lượng từ nói trực tiếp số từ và danh từ)
   ⭕ ĐÚNG: 我买两斤苹果。 (Lượng từ 斤 bắt buộc đi với 两)
   💡 GIẢI THÍCH: Khi đứng trước lượng từ 斤, số 2 bắt buộc phải đổi sang 两, không dùng 二.

3. ❌ SAI: 你家有多少口人？ (Dùng 多少 với lượng từ 口 khi hỏi số lượng gia đình nhỏ)
   ⭕ ĐÚNG: 你家有几口人？
   💡 GIẢI THÍCH: Số người trong gia đình thường dưới 10, người Trung Quốc luôn dùng đại từ 几.

4. ❌ SAI: Nhầm lẫn phát âm 买 (mǎi - thanh 3) và 卖 (mài - thanh 4)
   ⭕ ĐÚNG: 我要买 (mǎi) 苹果 / 商店在卖 (mài) 东西.
   💡 GIẢI THÍCH: Thanh 3 xuống-lên là ""mua"", thanh 4 rơi dứt khoát là ""bán"".
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Bạn Minh đi chợ hoa quả gần trường đại học để mua hoa quả về cho cả phòng ký túc xá.

- **Minh**: 老板，你好！请问这个苹果多少钱一斤？
  - *Pinyin*: Lǎobǎn, nǐ hǎo! Qǐngwèn zhège píngguǒ duōshao qián yì jīn?
  - *Phân tích*: `老板` (ông chủ) + `这个苹果` (quả táo này) + `多少钱` (bao nhiêu tiền) + `一斤` (1 cân 500g).
  - *Dịch*: Chào ông chủ! Xin hỏi táo này bao nhiêu tiền một cân (500g) thế ạ?

- **Người bán**: 你好！红苹果八块钱一斤，很甜很好吃！
  - *Pinyin*: Nǐ hǎo! Hóng píngguǒ bā kuài qián yì jīn, hěn tián hěn hǎochī!
  - *Phân tích*: `红苹果` (táo đỏ) + `八块钱一斤` (8 tệ một cân) + `很甜` (rất ngọt) + `很好吃` (rất ngon).
  - *Dịch*: Chào cậu! Táo đỏ 8 tệ một cân, ngọt và ngon lắm đó!

- **Minh**: 八块钱？太贵了！可以便宜一点儿吗？
  - *Pinyin*: Bā kuài qián? Tài guì le! Kěyǐ piányi yìdiǎnr ba?
  - *Phân tích*: `太贵了` (đắt quá rồi) + `可以` (có thể) + `便宜一点儿` (rẻ hơn chút) + `吧` (nhé/đi).
  - *Dịch*: 8 tệ ạ? Đắt quá! Bác bớt cho cháu một chút đi?

- **Người bán**: 你买几斤？买三斤的话，算你七块一斤。
  - *Pinyin*: Nǐ mǎi jǐ jīn? Mǎi sān jīn dehuà, suàn nǐ qī kuài yì jīn.
  - *Phân tích*: `你买几斤` (cậu mua mấy cân) + `买三斤的话` (nếu mua 3 cân) + `算你七块` (tính cho cậu 7 tệ).
  - *Dịch*: Cậu mua mấy cân? Nếu mua 3 cân thì tôi lấy 7 tệ một cân nhé.

- **Minh**: 好的！我要三斤，一共多少钱？
  - *Pinyin*: Hǎo de! Wǒ yào sān jīn, yígòng duōshao qián?
  - *Phân tích*: `我要三斤` (tôi lấy 3 cân = 1.5kg) + `一共` (tổng cộng) + `多少钱`.
  - *Dịch*: Dạ được ạ! Cháu lấy 3 cân, tổng cộng hết bao nhiêu tiền ạ?

- **Người bán**: 一共二十一块钱。
  - *Pinyin*: Yígòng èrshíyī kuài qián.
  - *Phân tích*: `二十一` (21) + `块钱` (đồng tệ).
  - *Dịch*: Tổng cộng hết 21 tệ cậu nhé.

- **Minh**: 给你三十块钱。
  - *Pinyin*: Gěi nǐ sānshí kuài qián.
  - *Phân tích*: `给你` (gửi bác) + `三十块钱` (30 tệ).
  - *Dịch*: Cháu gửi bác 30 tệ ạ.

- **Người bán**: 找你九块钱，谢谢！下次再来！
  - *Pinyin*: Zhǎo nǐ jiǔ kuài qián, xièxie! Xià cì zài lái!
  - *Phân tích*: `找你` (thối lại cho cậu) + `九块钱` (9 tệ) + `下次再来` (lần sau lại ghé nhé).
  - *Dịch*: Thối lại cậu 9 tệ, cảm ơn nhé! Lần sau lại tới mua nha!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập lượng từ & mẫu câu
1. Điền lượng từ thích hợp (`个`, `口`, `本`, `斤`) vào chỗ trống:
   - `我家有五___人。`
   - `我想买三___苹果。`
   - `桌子上有两___汉语书。`
2. Chọn `不` hoặc `没` điền vào câu:
   - `我___是老师，我___有哥哥。`
3. Dịch câu sau: *""Cái áo này đắt quá rồi! Rẻ hơn một chút được không?""*

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - `五口人` (lượng từ nhân khẩu dùng `口`).
   - `三斤苹果` (3 cân táo dùng `斤` hoặc `三个苹果` dùng `个`).
   - `两本书` (sách dùng lượng từ `本`).
2. **Câu 2**: Điền **不** trước `是` và điền **没** trước `有` (`我不是老师，我没有哥哥`).
3. **Câu 3**: `这件衣服太贵了！便宜一点儿好吗？` (hoặc `可以便宜一点儿吗？`).
4. **Câu 4 trắc nghiệm**: Một cân Trung Quốc (1 斤) bằng bao nhiêu gram?
   - *Đáp án*: 500 gram (nửa ký).
5. **Câu 5 trắc nghiệm**: Tại sao nói `我不有哥哥` lại sai ngữ pháp?
   - *Đáp án*: Vì động từ `有` bắt buộc phải phủ định bằng `没` (没有), cấm kỵ dùng `不有`.

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phát âm thanh điệu zh-CN chuẩn xác và lưu chữ Hán vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Giới thiệu gia đình bằng 有/口, dùng đúng lượng từ và mẫu câu hỏi giá 多少钱, mặc cả 太贵了.",
            "Phân biệt 买 (mǎi) vs 卖 (mài) và không bao giờ nhầm 没有 thành 不有."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter3Id, SeedDataInitializer.ChineseLesson31Id,
            "Bài 3.1: Ăn uống, Gọi món & Sở thích",
            "zh-hsk1-an-uong-goi-mon-so-thich", 1, 35,
            @"# Bài 3.1: Ăn uống, Gọi món & Sở thích

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Ẩm thực Trung Hoa nổi tiếng toàn cầu. Bài học này sẽ đưa bạn bước vào không gian ẩm thực sống động với đầy đủ khẩu ngữ gọi món:
- Phân biệt bản chất và sắc thái của 3 động từ năng nguyện cốt lõi: **想 (xiǎng — muốn, mong ước)**, **要 (yào — cần, muốn dứt khoát)** và **会 (huì — biết làm nhờ học tập)**.
- Bày tỏ sở thích cá nhân với cấu trúc **喜欢 (xǐhuan — thích)** và khen ngợi món ăn ngon (**好吃 / 好喝**).
- Sử dụng chuẩn xác câu hỏi lựa chọn **还是 (háishi — hay là)** và ghi nhớ quy tắc cấm dùng `吗` ở cuối câu có `还是`.
- Làm chủ mẫu câu gọi món lịch sự tại nhà hàng: **请给我…… (Qǐng gěi wǒ...)**.
- Nắm vững các lượng từ dành cho đồ ăn, thức uống: **碗 (wǎn - bát)**, **杯 (bēi - cốc/ly)**, **盘 (pán - đĩa)**.
- Luyện phát âm tròn vành rõ chữ nhóm âm uốn lưỡi và bật hơi khó: `吃 (chī)`, `茶 (chá)`, `菜 (cài)`, `水 (shuǐ)`.

---

## 2. 🌱 Nhập môn Pinyin & Chiết tự Chữ Hán dễ nhớ

### 2.1. Phân biệt bộ ba động từ năng nguyện: 想, 要, 会
Người mới học tiếng Trung thường nhầm lẫn giữa 3 từ này vì tiếng Việt đều có thể dịch nôm na là ""muốn/biết"":
1. **想 (xiǎng — Muốn, dự định, nhớ nhung)**:
   - Diễn tả **mong muốn chủ quan, nhẹ nhàng, lịch sự**, mang tính nguyện vọng trong suy nghĩ.
   - Ví dụ: `我想学汉语。` *(Tôi muốn học tiếng Trung).*
   - `我想喝茶。` *(Tôi muốn uống trà — cảm giác thèm uống nhẹ nhàng).*
2. **要 (yào — Cần, muốn dứt khoát, phải làm)**:
   - Diễn tả **ý định mạnh mẽ, dứt khoát**, hoặc dùng trực tiếp khi gọi món/mua hàng.
   - Khi gọi món trong quán ăn: `我要一碗牛肉面。` *(Cho tôi 1 bát mì bò — gọi món dứt khoát).*
3. **会 (huì — Biết)**:
   - Biểu thị **kỹ năng học được thông qua quá trình rèn luyện, học tập**:
   - `我会说汉语。` *(Tôi biết nói tiếng Trung — do đã đi học).*
   - `我会做中国菜。` *(Tôi biết nấu món ăn Trung Quốc).*
   - Khác với `知道 (zhīdào - biết thông tin)`: `我知道这件事` *(Tôi biết chuyện này)*.

### 2.2. Câu hỏi lựa chọn 还是 (háishi) vs 或者 (huòzhě)
Trong tiếng Trung có hai từ đều dịch là ""hoặc/hay là"", nhưng dùng trong hai ngữ cảnh trái ngược:
- **`还是 (háishi)`: BẮT BUỘC DÙNG TRONG CÂU HỎI LỰA CHỌN (""A hay là B?"")**:
  - `你想喝茶还是喝咖啡？` *(Bạn muốn uống trà hay là uống cà phê?)*
  - **LƯU Ý CỰC KỲ QUAN TRỌNG**: Khi câu đã dùng `还是` thì **TUYỆT ĐỐI KHÔNG THÊM `吗` Ở CUỐI CÂU NỮA!** Thêm `吗` là lỗi sai kinh điển!
- **`或者 (huòzhě)`: CHỈ DÙNG TRONG CÂU TRẦN THUẬT KHẲNG ĐỊNH (""A hoặc là B đều được"")**:
  - `我喝茶或者咖啡都可以。` *(Tôi uống trà hoặc cà phê đều được).*

### 2.3. Lượng từ dành cho đồ ăn, thức uống
Tiếng Trung luôn yêu cầu vật chứa đóng vai trò làm lượng từ:
- **`碗 (wǎn - cái bát)`**: `一碗米饭` *(1 bát cơm)*, `一碗面条` *(1 bát mì)*.
- **`杯 (bēi - cái ly, cốc, tách)`**: `一杯茶` *(1 tách trà)*, `一杯热咖啡` *(1 ly cà phê nóng)*, `一杯水` *(1 ly nước)*.
- **`盘 (pán - cái đĩa)`**: `一盘饺子` *(1 đĩa sủi cảo)*, `一盘菜` *(1 đĩa thức ăn)*.

### 2.4. Mẹo chiết tự chữ Hán ẩm thực
- **`吃 (chī - ăn)`**: Có bộ **Khẩu (口 - cái miệng)** bên trái gợi ý hành động ăn uống.
- **`喝 (hē - uống)`**: Cũng có bộ **Khẩu (口)** bên trái, bên phải là hình ảnh người đứng dưới ánh mặt trời khát nước.
- **`茶 (chá - trà)`**: Phía trên là bộ **Thảo (艹 - cỏ cây, búp chè)**, ở giữa là chữ **Nhân (人 - con người)**, ở dưới là bộ **Mộc (木 - thân cây)**. Hình ảnh tuyệt mỹ: Con người hòa quyện tĩnh lặng cùng cây cỏ thiên nhiên khi thưởng trà!
- **`想 (xiǎng - muốn, nhớ, nghĩ)`**: Phía trên là chữ **Tương (相 - hình mắt ngắm nhìn cây)**, phía dưới là bộ **Tâm (心 - trái tim)**. Khi trái tim hướng về, nghĩ về ai đó chính là nỗi nhớ nhung, mong ước.

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbǐao)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **吃** | chī | Cật | Ăn | 你想吃什么？ *(Bạn muốn ăn gì?)* |
| **喝** | hē | Hát | Uống | 请喝热茶。 *(Xin mời uống trà nóng.)* |
| **菜** | cài | Thái | Món ăn, rau | 中国菜很好吃。 *(Món Trung rất ngon.)* |
| **米饭** | mǐfàn | Mễ Phạn | Cơm trắng | 我要一碗米饭。 *(Cho tôi 1 bát cơm.)* |
| **面条** | miàntiáo | Diện Điều | Mì sợi | 他很喜欢吃面条。 *(Anh ấy rất thích ăn mì.)* |
| **饺子** | jiǎozi | Kiểu Tử | Bánh sủi cảo | 我们一起包饺子吧！ *(Chúng mình cùng gói sủi cảo đi!)* |
| **水** | shuǐ | Thủy | Nước lọc | 请给我一杯水。 *(Làm ơn cho tôi 1 ly nước.)* |
| **茶** | chá | Trà | Trà xanh | 中国人喜欢喝茶。 *(Người TQ thích uống trà.)* |
| **咖啡** | kāfēi | — | Cà phê | 我不喝咖啡。 *(Tôi không uống cà phê.)* |
| **想** | xiǎng | Tưởng | Muốn, nhớ, suy nghĩ | 我想去中国。 *(Tôi muốn đi Trung Quốc.)* |
| **要** | yào | Yếu | Cần, muốn, phải | 我要一杯冰水。 *(Cho tôi 1 ly nước đá.)* |
| **会** | huì | Hội | Biết (qua học tập) | 你会做饭吗？ *(Bạn biết nấu cơm không?)* |
| **喜欢** | xǐhuan | Hỉ Hoan | Thích, yêu thích | 我很喜欢学汉语。 *(Tôi rất thích học tiếng Trung.)* |
| **好吃** | hǎochī | Hảo Cật | Ngon (đồ ăn) | 这个菜真好吃！ *(Món này ngon thật đấy!)* |
| **好喝** | hǎohē | Hảo Hát | Ngon (đồ uống) | 这个茶很好喝。 *(Trà này rất thơm ngon.)* |
| **还是** | háishi | Hoàn Thị | Hay là (câu hỏi lựa chọn) | 喝茶还是喝咖啡？ *(Uống trà hay là cà phê?)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Mẫu câu gọi món lịch sự: `请给我 + [Lượng từ] + [Món ăn]`
- **Công thức**: `请给我 + [Số từ + Lượng từ] + [Tên món ăn / Đồ uống]`
- `请 (qǐng)`: Làm ơn, xin mời (thể hiện sự tôn trọng, văn minh).
- Ví dụ:
  - `请给我一碗米饭和一杯热茶。` *(Làm ơn cho tôi một bát cơm và một tách trà nóng).*
  - `服务员，请给我们两双筷子。` *(Phục vụ ơi, làm ơn cho chúng tôi 2 đôi đũa).*

### 4.2. Mẫu câu hỏi lựa chọn: `A 还是 B？`
- **Công thức**: `[Chủ ngữ] + [Động từ] + A + 还是 + B？`
- Trả lời bằng cách chọn một trong hai phương án A hoặc B.
- Ví dụ:
  - `你想吃米饭还是面条？` *(Bạn muốn ăn cơm hay ăn mì?)*
  - Trả lời: `我想吃面条。` *(Tôi muốn ăn mì).*

### 4.3. Cấu trúc sở thích: `喜欢 + [Danh từ / Động từ]`
- Có thể kết hợp trực tiếp với danh từ hoặc cụm hành động phía sau:
  - `我喜欢中国菜。` *(Tôi thích món ăn Trung Quốc).*
  - `我喜欢看书和听音乐。` *(Tôi thích đọc sách và nghe nhạc).*

### 4.4. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 你喝茶还是喝咖啡吗？ (Thêm 吗 vào câu hỏi lựa chọn có 还是)
   ⭕ ĐÚNG: 你喝茶还是喝咖啡？
   💡 GIẢI THÍCH: Bản thân 还是 đã mang chức năng nghi vấn lựa chọn, TUYỆT ĐỐI không thêm 吗 ở cuối.

2. ❌ SAI: 你想吃米饭或者面条？ (Dùng 或者 trong câu hỏi)
   ⭕ ĐÚNG: 你想吃米饭还是面条？
   💡 GIẢI THÍCH: 或者 chỉ dùng trong câu khẳng định, trong câu hỏi lựa chọn bắt buộc dùng 还是.

3. ❌ SAI: Đọc 吃 (chī) không uốn lưỡi bật hơi thành cī hoặc zī
   ⭕ ĐÚNG: chī (Uốn cong đầu lưỡi chạm ngạc trên và BẬT HƠI MẠNH).
   💡 GIẢI THÍCH: Âm ch là âm uốn lưỡi bật hơi mạnh nhất trong tiếng Trung.

4. ❌ SAI: Phân biệt nhầm 好吃 (ngon đồ ăn) và 好喝 (ngon đồ uống)
   ⭕ ĐÚNG: 这个菜很好吃 (Món ăn ngon) / 这个咖啡很好喝 (Cà phê ngon).
   💡 GIẢI THÍCH: Tiếng Trung tách bạch động từ ăn (吃) và uống (喝), tính từ khen ngon cũng phân chia tương ứng.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Buổi tối tại một nhà hàng ẩm thực Tứ Xuyên nổi tiếng tại khu phố Vương Phủ Tỉnh. Bạn Minh và bạn Lý Na đi ăn tối cùng nhau.

- **Phục vụ**: 晚上好！欢迎光临！两位想吃点儿什么？
  - *Pinyin*: Wǎnshang hǎo! Huānyíng guānglín! Liǎng wèi xiǎng chī diǎnr shénme?
  - *Phân tích*: `晚上好` (chào buổi tối) + `欢迎光临` (chào mừng quý khách) + `两位` (hai vị khách) + `想吃点儿什么` (muốn ăn chút gì?).
  - *Dịch*: Chào buổi tối quý khách! Hoan nghênh hai vị, hai vị muốn dùng món gì ạ?

- **Li Na**: 明，你会吃辣吗？你想吃米饭还是面条？
  - *Pinyin*: Míng, nǐ huì chī là ma? Nǐ xiǎng chī mǐfàn háishi miàntiáo?
  - *Phân tích*: `会吃辣吗` (biết ăn cay không) + `想吃米饭` (muốn ăn cơm) + `还是` (hay là) + `面条` (mì sợi?).
  - *Dịch*: Minh ơi, bạn có ăn được cay không? Bạn muốn ăn cơm hay ăn mì?

- **Minh**: 我会吃一点儿辣。我想吃面条，中国面条很有名！
  - *Pinyin*: Wǒ huì chī yìdiǎnr là. Wǒ xiǎng chī miàntiáo, Zhōngguó miàntiáo hěn yǒumíng!
  - *Phân tích*: `会吃一点儿辣` (biết ăn cay một chút) + `想吃面条` (muốn ăn mì) + `很有名` (rất nổi tiếng).
  - *Dịch*: Mình ăn được cay một chút. Mình muốn ăn mì, mì Trung Quốc nổi tiếng lắm!

- **Li Na**: 好！服务员，请给我们两碗牛肉面和一盘饺子。
  - *Pinyin*: Hǎo! Fúwùyuán, qǐng gěi wǒmen liǎng wǎn niúròumiàn hé yì pán jiǎozi.
  - *Phân tích*: `服务员` (phục vụ ơi) + `请给我们` (làm ơn cho chúng tôi) + `两碗牛肉面` (2 bát mì bò) + `一盘饺子` (1 đĩa sủi cảo).
  - *Dịch*: Được! Phục vụ ơi, cho chúng tôi 2 bát mì thịt bò và 1 đĩa sủi cảo nhé.

- **Phục vụ**: 好的。两位想喝点儿什么？茶还是啤酒？
  - *Pinyin*: Hǎo de. Liǎng wèi xiǎng hē diǎnr shénme? Chá háishi píjiǔ?
  - *Phân tích*: `想喝点儿什么` (muốn uống gì) + `茶还是啤酒` (trà hay bia?).
  - *Dịch*: Dạ vâng. Hai vị muốn uống chút gì không ạ? Trà hay bia ạ?

- **Minh**: 请给我们一壶热茶，谢谢！
  - *Pinyin*: Qǐng gěi wǒmen yì hú rè chá, xièxie!
  - *Phân tích*: `一壶热茶` (1 ấm trà nóng) + `谢谢`.
  - *Dịch*: Làm ơn cho chúng tôi một ấm trà nóng, cảm ơn bạn!

- **Phục vụ**: 好的，请稍等！马上来！
  - *Pinyin*: Hǎo de, qǐng shāoděng! Mǎshàng lái!
  - *Phân tích*: `请稍等` (vui lòng đợi một lát) + `马上来` (sẽ mang lên ngay).
  - *Dịch*: Dạ vâng, xin quý khách đợi một lát, đồ ăn sẽ lên ngay ạ!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập động từ năng nguyện & gọi món
1. Chọn `想`, `要` hoặc `会` điền vào chỗ trống:
   - `我___说汉语，我是在大学学的。`
   - `服务员，我___一碗米饭。`
   - `明天周末，我___去图书馆看书。`
2. Điền `还是` hoặc `或者` vào câu:
   - `你想喝热茶___冰水？`
   - `星期六___星期天去都可以。`
3. Sửa lỗi sai trong câu: `你想吃米饭还是面条吗？`

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - Điền **会** (`我会说汉语` — kỹ năng học được).
   - Điền **要** (`我要一碗米饭` — gọi món dứt khoát).
   - Điền **想** (`我想去图书馆` — dự định mong muốn).
2. **Câu 2**:
   - Câu hỏi lựa chọn điền **还是** (`热茶还是冰水？`).
   - Câu trần thuật khẳng định điền **或者** (`星期六或者星期天`).
3. **Câu 3**: Bỏ trợ từ `吗` ở cuối câu: `你想吃米饭还是面条？`
4. **Câu 4 trắc nghiệm**: Lượng từ phù hợp cho sủi cảo (饺子) là gì?
   - *Đáp án*: `一盘饺子` (yì pán jiǎozi — một đĩa sủi cảo).
5. **Câu 5 trắc nghiệm**: Tại sao khen cà phê ngon không dùng `好吃`?
   - *Đáp án*: Vì cà phê là đồ uống dạng lỏng, bắt buộc dùng `好喝` (hǎohē).

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phát âm thanh điệu zh-CN chuẩn xác và lưu chữ Hán vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Gọi món ăn, đồ uống bằng 想/要/请给我 và dùng câu hỏi lựa chọn 还是 (hay là).",
            "Diễn đạt sở thích ăn uống, kỹ năng cá nhân với 喜欢/会 và luyện âm bật hơi uốn lưỡi 吃/茶/水."),

        new CjkLessonMeta(
            SeedDataInitializer.ChineseCourseId, SeedDataInitializer.ChineseChapter3Id, SeedDataInitializer.ChineseLesson32Id,
            "Bài 3.2: Hỏi đường, Di chuyển & Lịch trình hằng ngày",
            "zh-hsk1-hoi-duong-di-chuyen-lich-trinh", 2, 35,
            @"# Bài 3.2: Hỏi đường, Di chuyển & Lịch trình hằng ngày

## 1. Mục tiêu bài học (学习目标 — Xuéxí Mùbiāo)
Bài học đỉnh cao này hoàn thiện toàn bộ hệ thống ngữ pháp và giao tiếp nền tảng của cấp độ HSK1:
- Hỏi vị trí và phương hướng chuẩn xác bằng hai mẫu câu kinh điển: **……在哪儿？ (zài nǎr)** và **去……怎么走？ (qù ... zěnme zǒu)**.
- Làm chủ cấu trúc câu chỉ phương tiện di chuyển: **坐 + [Phương tiện] + 去 + [Địa điểm]** (`坐地铁去学校`, `坐出租车去机场`).
- Nắm vững cấu trúc câu liên động đặc thù của tiếng Trung: **Đi đâu làm gì** (`去 + Địa điểm + Hành động`).
- Sử dụng thành thạo toàn bộ các phương vị từ: **前面 (đằng trước)**, **后面 (đằng sau)**, **左边 (bên trái)**, **右边 (bên phải)**, **旁边 (bên cạnh)**.
- Phân biệt rõ hai chức năng của chữ **在 (zài)**: Động từ chỉ vị trí nơi chốn (""Ở tại..."") và Phó từ chỉ hành động đang tiếp diễn (""Đang..."").
- Lên lịch trình hoạt động sinh hoạt một ngày chuẩn HSK1 từ sáng đến tối.

---

## 2. 🌱 Nhập môn Pinyin & Chiết tự Chữ Hán dễ nhớ

### 2.1. Bản chất phương vị từ & Cấu trúc định vị không gian
Trong tiếng Trung, từ chỉ phương hướng và vị trí được gọi là Phương vị từ:
- `前 (qián - trước)` → `前面 (qiánmian - phía trước)`
- `后 (hòu - sau)` → `后面 (hòumian - phía sau)`
- `左 (zuǒ - trái)` → `左边 (zuǒbian - bên trái)`
- `右 (yòu - phải)` → `右边 (yòubian - bên phải)`
- `上 (shàng - trên)` → `上面 (shàngmian - phía trên)`
- `下 (xià - dưới)` → `下面 (xiàmian - phía dưới)`
- `旁 (páng - bên)` → `旁边 (pángbiān - bên cạnh)`

**QUY TẮC ĐỊNH VỊ KHÔNG GIAN CỦA TIẾNG TRUNG**:
- Tiếng Việt nói: *""Trước trường học""* (Từ chỉ vị trí đứng trước danh từ).
- **Tiếng Trung nói ngược lại**: `[Địa điểm danh từ] + [Phương vị từ]`!
  - `学校前面` *(Phía trước trường học — ĐÚNG).*
  - `前面学校` *(SAI HOÀN TOÀN!).*
  - `银行旁边` *(Bên cạnh ngân hàng).*
  - `医院后面` *(Phía sau bệnh viện).*

### 2.2. Động từ 坐 (zuò) & Phương tiện giao thông
Khi di chuyển bằng các phương tiện công cộng có ghế ngồi (xe buýt, tàu điện ngầm, taxi, máy bay, tàu hỏa), tiếng Trung luôn dùng động từ **坐 (zuò — ngồi)** đặt TRƯỚC phương tiện:
$$\text{Công thức} = \text{Chủ ngữ} + \mathbf{\text{坐}} + \text{[Phương tiện]} + \mathbf{\text{去}} + \text{[Địa điểm]}$$
- `我坐地铁去学校。` *(Tôi đi tàu điện ngầm đến trường).*
- `他坐飞机去北京。` *(Anh ấy đi máy bay đến Bắc Kinh).*
- Đi taxi: `坐出租车 (zuò chūzūchē)`.
- Đi xe buýt: `坐公共汽车 (zuò gōnggòng qìchē)`.
- Riêng đối với **đi bộ**, ta dùng cụm từ: **走路 (zǒulù)** hoặc **步行 (bùxíng)**, không dùng chữ `坐`.

### 2.3. Cấu trúc câu liên động (Đi đâu làm gì)
Câu liên động là câu có hai hay nhiều hành động xảy ra nối tiếp nhau do cùng một chủ ngữ thực hiện:
$$\text{Công thức} = \text{Chủ ngữ} + \mathbf{\text{去}} + \text{[Địa điểm]} + \text{[Hành động làm gì]}$$
- Khác với tiếng Anh phải dùng giới từ nối (*I go to the library to read books*), tiếng Trung xếp hai động từ liền kề theo thứ tự thời gian xảy ra:
  - Hành động đi đến địa điểm xảy ra trước (`去图书馆`).
  - Hành động đọc sách xảy ra sau (`看书`).
  - Ghép lại: `我去图书馆看书。` *(Tôi đến thư viện đọc sách).*
  - `我们去饭店吃饭。` *(Chúng tôi đến nhà hàng ăn cơm).*

### 2.4. Mẹo chiết tự chữ Hán giao thông & phương hướng
- **`坐 (zuò - ngồi)`**: Trông giống như hai chữ **Nhân (人 - hai người)** ngồi đối diện nhau trên mảnh **Thổ (土 - mặt đất)** trò chuyện.
- **`车 (chē - xe cộ)`**: Hình ảnh cỗ xe ngựa thời xưa nhìn từ trên xuống: có hai bánh xe song song và trục càng xe ở giữa.
- **`站 (zhàn - trạm, ga, đứng)`**: Gồm chữ **Lập (立 - đứng sững sững)** và chữ **Chiếm (占 - chiếm giữ một vị trí)**. Nơi xe cộ dừng bánh đứng chờ đón khách chính là trạm, ga.
- **`路 (lù - con đường)`**: Gồm bộ **Túc (足 - bàn chân)** và chữ **Các (各 - muôn nẻo)**. Nơi muôn bàn chân người qua lại in dấu tạo thành con đường.

---

## 3. Bảng Từ vựng đầy đủ (生词表 — Shēngcíbǐao)

| Chữ Hán | Pinyin | Âm Hán Việt | Nghĩa tiếng Việt | Câu ví dụ hoàn chỉnh |
|---|---|---|---|---|
| **在哪儿** | zài nǎr | Tại Na Nhi | Ở đâu? | 请问，洗手间在哪儿？ *(Xin hỏi nhà vệ sinh ở đâu?)* |
| **怎么走** | zěnme zǒu | Chẩm Ma Tẩu | Đi như thế nào? | 去火车站怎么走？ *(Đi ra ga tàu hỏa đi thế nào?)* |
| **火车站** | huǒchēzhàn | Hỏa Xa Trạm | Ga tàu hỏa | 火车站在市中心。 *(Ga tàu hỏa ở trung tâm TP.)* |
| **地铁 / 地铁站** | dìtiě / dìtiězhàn | Địa Thiết Trạm | Tàu điện ngầm / Ga tàu điện | 我每天坐地铁。 *(Mỗi ngày tôi đi tàu điện ngầm.)* |
| **出租车** | chūzūchē | Xuất Tô Xa | Xe taxi | 坐出租车很方便。 *(Đi taxi rất thuận tiện.)* |
| **公共汽车** | gōnggòng qìchē | Công Cộng Khí Xa | Xe buýt | 学校门前有公共汽车。 *(Trước cổng trường có xe buýt.)* |
| **飞机 / 机场** | fēijī / jīchǎng | Phi Cơ / Cơ Trường | Máy bay / Sân bay | 坐飞机去上海。 *(Đi máy bay đến Thượng Hải.)* |
| **前面 / 后面** | qiánmian / hòumian | Tiền / Hậu | Phía trước / Phía sau | 医院在学校前面。 *(Bệnh viện ở trước trường học.)* |
| **左边 / 右边** | zuǒbian / yòubian | Tả / Hữu | Bên trái / Bên phải | 往左拐就到了。 *(Rẽ trái là tới nơi.)* |
| **旁边** | pángbiān | Bàng Biên | Bên cạnh | 银行就在超市旁边。 *(Ngân hàng ở ngay cạnh siêu thị.)* |
| **一直** | yìzhí | Nhất Trực | Cứ thẳng, liên tục | 一直往前走。 *(Cứ đi thẳng về phía trước.)* |
| **往 / 拐** | wǎng / guǎi | Vãng / Quải | Hướng về... / Rẽ, quẹo | 往右拐。 *(Rẽ về bên phải.)* |
| **远 / 近** | yuǎn / jìn | Viễn / Cận | Xa / Gần | 这里离车站很近。 *(Ở đây cách bến xe rất gần.)* |
| **走路** | zǒulù | Tẩu Lộ | Đi bộ | 走路十分钟。 *(Đi bộ mất 10 phút.)* |
| **工作** | gōngzuò | Công Tác | Làm việc / Công việc | 我爸爸在医院工作。 *(Bố tôi làm việc ở bệnh viện.)* |
| **学习** | xuéxí | Học Tập | Học tập, rèn luyện | 我在学习汉语。 *(Tôi đang học tiếng Trung.)* |

---

## 4. Ngữ pháp mổ xẻ từng trợ từ & Cấu trúc câu

### 4.1. Mẫu câu hỏi vị trí & Chỉ đường
- **Hỏi vị trí**: `[Địa điểm] + 在哪儿 (zài nǎr) / 在哪里 (zài nǎlǐ)？`
  - Ví dụ: `请问，地铁站在哪儿？` *(Xin hỏi ga tàu điện ngầm ở đâu?)*
- **Hỏi đường đi**: `去 + [Địa điểm] + 怎么走 (zěnme zǒu)？`
  - Ví dụ: `去火车站怎么走？` *(Đi ra ga tàu hỏa đi như thế nào?)*
- **Các mẫu câu chỉ đường cơ bản**:
  - `一直往前走。` *(Yìzhí wǎng qián zǒu — Cứ đi thẳng về phía trước).*
  - `到路口往左/右拐。` *(Dào lùkǒu wǎng zuǒ/yòu guǎi — Đến ngã tư rẽ trái/phải).*
  - `就在 [Địa điểm] + 旁边/后面。` *(Ở ngay bên cạnh/phía sau...).*

### 4.2. Hai chức năng của chữ 在 (zài)
1. **Động từ chỉ vị trí (Ở, tại)**:
   - `我在家。` *(Tôi ở nhà).*
   - `学校在银行前面。` *(Trường học ở phía trước ngân hàng).*
2. **Phó từ biểu thị hành động đang diễn ra (Đang...)**:
   - `我在看书呢。` *(Tôi đang đọc sách đấy).*
   - `妈妈在做饭。` *(Mẹ đang nấu cơm).*

### 4.3. 4 ví dụ đối chiếu Đúng / Sai kinh điển người Việt hay mắc

```markdown
1. ❌ SAI: 我去学校坐地铁 (Đặt phương tiện ở cuối câu theo trật tự tiếng Việt)
   ⭕ ĐÚNG: 我坐地铁去学校。
   💡 GIẢI THÍCH: Trật tự chuẩn tiếng Trung luôn là: 坐 + Phương tiện + 去 + Địa điểm.

2. ❌ SAI: 前面学校 có bệnh viện (Đặt phương vị từ trước danh từ)
   ⭕ ĐÚNG: 学校前面有医院。
   💡 GIẢI THÍCH: Danh từ địa điểm phải đứng TRƯỚC phương vị từ: 学校 (trường) + 前面 (phía trước).

3. ❌ SAI: Nhầm lẫn giữa 坐 (zuò - ngồi/đi xe) và 走 (zǒu - đi bộ)
   ⭕ ĐÚNG: 坐车 (đi xe) vs 走路 (đi bộ).
   💡 GIẢI THÍCH: zuò thanh 4 rơi dứt khoát; zǒu thanh 3 xuống-lên.

4. ❌ SAI: 我看书去图书馆 (Đảo lộn trật tự câu liên động)
   ⭕ ĐÚNG: 我去图书馆看书。 (Đi đến thư viện rồi mới đọc sách)
   💡 GIẢI THÍCH: Hành động nào xảy ra trước về mặt thời gian thì đứng trước trong câu liên động.
```

---

## 5. Hội thoại thực tế bóc tách từng từ (对话 — Duìhuà)

### Bối cảnh:
Bạn Minh vừa ra khỏi khách sạn ở Bắc Kinh và muốn tìm đường đến ga tàu điện ngầm để đi tham quan Cố Cung. Minh gặp một bác bảo vệ trên phố.

- **Minh**: 您好！请问，去地铁站怎么走？
  - *Pinyin*: Nín hǎo! Qǐngwèn, qù dìtiězhàn zěnme zǒu?
  - *Phân tích*: `您好` (chào bác - kính ngữ) + `请问` (xin hỏi) + `去地铁站` (đến ga tàu điện) + `怎么走` (đi như thế nào?).
  - *Dịch*: Cháu chào bác ạ! Xin hỏi đi ra ga tàu điện ngầm đi thế nào ạ?

- **Bác bảo vệ**: 你好！你从这里一直往前走，大概两百米。
  - *Pinyin*: Nǐ hǎo! Nǐ cóng zhèlǐ yìzhí wǎng qián zǒu, dàgài liǎng bǎi mǐ.
  - *Phân tích*: `从这里` (từ đây) + `一直往前走` (cứ đi thẳng về phía trước) + `大概` (khoảng chừng) + `两百米` (200 mét).
  - *Dịch*: Chào cháu! Cháu cứ từ đây đi thẳng về phía trước khoảng 200 mét nhé.

- **Minh**: 到了十字路口呢？
  - *Pinyin*: Dào le shízì lùkǒu ne?
  - *Phân tích*: `到了` (đến nơi) + `十字路口` (ngã tư đường) + `呢` (thì sao ạ?).
  - *Dịch*: Đến ngã tư đường thì đi tiếp sao ạ?

- **Bác bảo vệ**: 到路口往右拐，地铁站就在中国银行旁边。
  - *Pinyin*: Dào lùkǒu wǎng yòu guǎi, dìtiězhàn jiù zài Zhōngguó Yínháng pángbiān.
  - *Phân tích*: `到路口` (đến ngã rẽ) + `往右拐` (rẽ về bên phải) + `就在` (ở ngay tại) + `中国银行旁边` (bên cạnh Ngân hàng Trung Quốc).
  - *Dịch*: Đến ngã rẽ cháu quẹo phải nhé, ga tàu điện ngầm nằm ngay bên cạnh Ngân hàng Trung Quốc ấy.

- **Minh**: 走路远吗？要走多长时间？
  - *Pinyin*: Zǒulù yuǎn ma? Yào zǒu duō cháng shíjiān?
  - *Phân tích*: `走路远吗` (đi bộ xa không) + `要走` (cần đi) + `多长时间` (bao lâu?).
  - *Dịch*: Đi bộ có xa không bác? Đi mất khoảng bao lâu ạ?

- **Bác bảo vệ**: 不远，很近！走路五分钟就到了。
  - *Pinyin*: Bù yuǎn, hěn jìn! Zǒulù wǔ fēnzhōng jiù dào le.
  - *Phân tích*: `不远` (không xa) + `很近` (rất gần) + `走路五分钟` (đi bộ 5 phút) + `就到了` (là tới ngay).
  - *Dịch*: Không xa đâu, gần lắm! Đi bộ 5 phút là tới rồi.

- **Minh**: 太感谢您了！
  - *Pinyin*: Tài gǎnxiè nín le!
  - *Phân tích*: `太感谢您了` (vô cùng cảm ơn bác ạ).
  - *Dịch*: Cháu cảm ơn bác rất nhiều ạ!

---

## 6. Bài tập tự luyện & Tự kiểm tra cuối bài

### Phần A: Luyện tập phương vị từ & phương tiện
1. Điền phương vị từ thích hợp vào chỗ trống:
   - `学校___ (phía trước) 有一家商店。`
   - `地铁站在银行___ (bên cạnh)。`
2. Sắp xếp các từ sau thành câu hoàn chỉnh:
   - `去 / 我 / 地铁 / 坐 / 公司` → ___
   - `看书 / 去 / 图书馆 / 他` → ___
3. Dịch câu sau: *""Xin hỏi, đi ra sân bay đi như thế nào?""*

### Phần B: Đề tự kiểm tra năng lực cuối bài (5 câu mô phỏng HSK1)

<details>
<summary>👉 Xem đáp án & Giải thích chi tiết từng câu</summary>

1. **Câu 1**:
   - `学校前面` (Phía trước trường học).
   - `银行旁边` (Bên cạnh ngân hàng).
2. **Câu 2**:
   - `我坐地铁去公司。` *(Tôi đi tàu điện ngầm đến công ty).*
   - `他去图书馆看书。` *(Cậu ấy đến thư viện đọc sách).*
3. **Câu 3**: `请问，去飞机场怎么走？` *(Qǐngwèn, qù fēijīchǎng zěnme zǒu?).*
4. **Câu 4 trắc nghiệm**: Cụm từ `一直往前走` có nghĩa là gì?
   - *Đáp án*: Cứ đi thẳng về phía trước.
5. **Câu 5 trắc nghiệm**: Chữ `在` trong câu `我在看书呢` đóng vai trò gì?
   - *Đáp án*: Phó từ biểu thị hành động đang diễn ra (Đang đọc sách).

</details>

> [!TIP]
> **Mẹo ôn tập nhanh:** Sau khi đọc xong lý thuyết, hãy mở **Phòng Thực Hành (Shadowing / Dictation / Word Map)** ngay bên dưới để rèn phát âm thanh điệu zh-CN chuẩn xác và lưu chữ Hán vào FSRS.

<!-- studxy-cjk-v1 -->
<!-- studxy-cjk-v2 -->
<!-- studxy-cjk-v3 -->",
            "Hỏi đường bằng 在哪儿 / 怎么走, diễn đạt phương tiện đi lại bằng 坐 và phương vị từ cơ bản.",
            "Thành thạo câu liên động 去 + Địa điểm + Hành động và lịch trình sinh hoạt hằng ngày HSK1.")
    };

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

    // =========================================================================
    // KHÓA 2: TIẾNG TRUNG SƠ CẤP (HSK1) — 7 ĐỀ THI THỬ (70 CÂU HỎI MULTIPLE CHOICE)
    // =========================================================================

    private static List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)> GetChineseQuizzes(DateTimeOffset now)
    {
        return new List<(PracticeQuiz Quiz, List<PracticeQuizItem> Items)>
        {
            BuildCjkQuiz("cjk-quiz:zh:l11", SeedDataInitializer.ChineseLesson11Id,
                "Bài 1.1: Trắc nghiệm Pinyin, Thanh điệu & Chào hỏi HSK1", now,
                new CjkQuizQuestion[]
                {
                    new("Phụ âm nào sau đây thuộc nhóm \"âm bật hơi mạnh\" làm rung tờ giấy đặt trước miệng?", "b", "p", "m", "d", "B", "Âm p là âm hai môi bật hơi mạnh, nén hơi rồi bật tung ra dứt khoát."),
                    new("Thanh 1 trong tiếng Trung có cao độ chuẩn là bao nhiêu trên thang âm độ 1-5?", "55 (cao và phẳng)", "35 (đi lên)", "214 (xuống sâu rồi lên)", "51 (rơi dứt khoát)", "A", "Thanh 1 giữ ở cao độ 55 thật cao và phẳng, ngân dài đều hơi."),
                    new("Hai chữ \"你好\" (nǐ hǎo) đều mang thanh 3, khi phát âm thực tế áp dụng quy tắc biến điệu như thế nào?", "Giữ nguyên 3 + 3", "Đổi thành 2 + 3 (ní hǎo)", "Đổi thành 3 + 2", "Đổi thành 1 + 3", "B", "Khi hai thanh 3 đứng liền kề, chữ đầu biến điệu thành thanh 2: nǐ hǎo → ní hǎo."),
                    new("Chữ Hán \"好\" (hảo - tốt) được ghép từ hai bộ thủ mang hình tượng người mẹ ôm con là gì?", "女 (nữ) và 子 (tử)", "亻 (nhân) và 木 (mộc)", "口 (khẩu) và 乞 (khất)", "日 (nhật) và 月 (nguyệt)", "A", "Chữ 好 gồm bộ Nữ (女) và bộ Tử (子), tượng trưng cho tình mẫu tử tốt đẹp."),
                    new("Khi ai đó nói \"谢谢！\", câu đáp lễ lịch sự chuẩn mực HSK1 là gì?", "没关系", "不客气", "对不起", "再见", "B", "Đáp lại 谢谢 (cảm ơn) là 不客气 (bú kèqi - đừng khách sáo)."),
                    new("Khi ai đó nói \"对不起！\", câu an ủi xua tan áy náy đáp lại là gì?", "不客气", "没关系", "谢谢", "你好", "B", "Đáp lại 对不起 (xin lỗi) là 没关系 (méi guānxi - không sao đâu)."),
                    new("Trợ từ nghi vấn nào đặt ở cuối câu trần thuật để chuyển thành câu hỏi Có/Không?", "吗 (ma)", "呢 (ne)", "的 (de)", "了 (le)", "A", "Trợ từ 吗 đặt cuối câu trần thuật để tạo câu hỏi Có/Không (ví dụ: 你好吗？)."),
                    new("Trợ từ nào dùng để hỏi ngược lại ngắn gọn \"Còn bạn thì sao?\"", "你吗？", "你呢？", "你的？", "你了？", "B", "Trợ từ tỉnh lược 呢 dùng để hỏi lửng ngược lại đối phương: 你呢？ (Còn bạn thì sao?)."),
                    new("A: \"你好！你好吗？\" B: \"我很好，谢谢！你呢？\" A: \"我也很好。\" Hỏi: Tình trạng sức khỏe của A và B như thế nào?", "Cả hai người đều rất khỏe", "A khỏe nhưng B bị mệt", "B khỏe nhưng A bị ốm", "Cả hai đều mệt", "A", "Cả B (我很好) và A (我也很好) đều khẳng định bản thân rất khỏe."),
                    new("Học sinh chào thầy giáo trước khi ra về: \"老师再见！明天见。\" Thầy giáo đáp lại: \"再见！\" Họ sẽ gặp lại nhau khi nào?", "Hôm nay", "Ngày mai", "Tuần sau", "Không gặp nữa", "B", "明天见 (míngtiān jiàn) có nghĩa là hẹn ngày mai gặp lại.")
                }),

            BuildCjkQuiz("cjk-quiz:zh:l12", SeedDataInitializer.ChineseLesson12Id,
                "Bài 1.2: Trắc nghiệm Biến điệu, Chữ Hán & Giới thiệu bản thân HSK1", now,
                new CjkQuizQuestion[]
                {
                    new("Phó từ phủ định \"不 (bù)\" khi đứng trước một chữ mang thanh 4 (như 是 shì) sẽ biến điệu thành gì?", "bù (thanh 4)", "bú (thanh 2)", "bǔ (thanh 3)", "bu (thanh nhẹ)", "B", "不 khi đứng trước thanh 4 bắt buộc phải biến điệu thành thanh 2: bú shì."),
                    new("Số từ \"一 (yī)\" khi đứng trước chữ mang thanh 4 (như 个 gè) sẽ biến điệu thành gì?", "yī (thanh 1)", "yí (thanh 2)", "yì (thanh 4)", "yi (thanh nhẹ)", "B", "一 khi đứng trước thanh 4 đổi thành thanh 2: yí gè."),
                    new("Phiên âm Pinyin chuẩn của từ \"名字\" (tên) là gì?", "míngzi", "mǐngzi", "míngzhì", "mīngzi", "A", "名字 có Pinyin là míngzi (chữ zi đọc thanh nhẹ)."),
                    new("Chữ Hán \"休\" (nghỉ ngơi) gồm bộ Nhân (亻) và bộ Mộc (木) mang ý nghĩa hình tượng gì?", "Người tựa lưng vào gốc cây nghỉ ngơi", "Người chặt cây lấy gỗ", "Người trồng cây gây rừng", "Cây che bóng mát", "A", "Chữ 休 tượng hình người lao động dừng chân tựa lưng vào bóng cây nghỉ ngơi."),
                    new("Cụm từ nào sau đây có nghĩa là \"sinh viên đại học\"?", "老师", "大学生", "中学生", "朋友", "B", "大学生 (dàxuéshēng) là sinh viên đại học."),
                    new("Để hỏi đối phương \"Bạn tên là gì?\", câu tiếng Trung chuẩn xác là gì?", "你叫什么名字？", "你叫哪国人？", "你是什么名字？", "你的名字叫什么？", "A", "你叫什么名字？ (Nǐ jiào shénme míngzi?) là mẫu câu hỏi họ tên chuẩn HSK1."),
                    new("Trong câu chữ \"是\", câu nào sau đây có ngữ pháp HOÀN TOÀN ĐÚNG?", "我是越南人。", "我是很高兴。", "我是好。", "我是也学生。", "A", "我是越南人 đúng ngữ pháp. 是 không bao giờ đứng trước tính từ (không nói 我是很高兴)."),
                    new("Điền đại từ nghi vấn thích hợp vào chỗ trống: \"你是___国人？\"", "哪", "那", "什么", "几", "A", "哪 (nǎ - nào) dùng trong câu hỏi quốc tịch: 你是哪国人？ (Bạn là người nước nào?)."),
                    new("Minh: \"我是越南人。你呢？\" Li Na: \"我是中国人。\" Hỏi: Lý Na là người nước nào?", "Người Việt Nam", "Người Trung Quốc", "Người Mỹ", "Người Nhật Bản", "B", "Lý Na nói rõ: 我是中国人 (Tôi là người Trung Quốc)."),
                    new("Hai người bạn mới quen nói với nhau: \"认识你很高兴！\" Câu này có nghĩa là gì?", "Rất vui được quen biết bạn!", "Bạn có khỏe không?", "Hẹn ngày mai gặp lại!", "Cảm ơn bạn rất nhiều!", "A", "认识你很高兴 (Rènshi nǐ hěn gāoxìng) có nghĩa là Rất vui được quen biết bạn!")
                }),

            BuildCjkQuiz("cjk-quiz:zh:l21", SeedDataInitializer.ChineseLesson21Id,
                "Bài 2.1: Trắc nghiệm Số đếm, Ngày tháng & Thời gian HSK1", now,
                new CjkQuizQuestion[]
                {
                    new("Cặp chữ số \"四 (4)\" và \"十 (10)\" có Pinyin tương ứng là gì?", "sì (thanh 4) và shí (thanh 2)", "shí (thanh 2) và sì (thanh 4)", "shì và sí", "sǐ và shí", "A", "四 đọc là sì (đầu lưỡi thẳng, thanh 4), 十 đọc là shí (uốn lưỡi, thanh 2)."),
                    new("Khi nói \"2 giờ\" (2:00), chữ số 2 BẮT BUỘC phải dùng là gì?", "二 (èr)", "两 (liǎng)", "双 (shuāng)", "对 (duì)", "B", "Trước đơn vị giờ 点 bắt buộc dùng 两: 两点 (liǎng diǎn), cấm kỵ nói 二点."),
                    new("Trong cách đọc thời gian tiếng Trung, từ \"半 (bàn)\" chỉ bao nhiêu phút?", "15 phút", "30 phút (nửa giờ)", "45 phút", "60 phút", "B", "半 (bàn) nghĩa là một nửa, nửa giờ tương đương 30 phút (ví dụ: 九点半 = 9 rưỡi)."),
                    new("Trật tự nói ngày tháng năm chuẩn theo tư duy Trung Quốc là gì?", "Ngày → Tháng → Năm", "Năm → Tháng → Ngày", "Tháng → Ngày → Năm", "Thứ → Ngày → Năm", "B", "Tư duy thời gian tiếng Trung đi từ lớn đến nhỏ: Năm (年) → Tháng (月) → Ngày (号/日)."),
                    new("\"Thứ Tư\" trong tiếng Trung được gọi là gì?", "星期三 (xīngqīsān)", "星期四 (xīngqīsì)", "星期五 (xīngqīwǔ)", "星期二 (xīngqī'èr)", "A", "Thứ trong tuần tiếng Trung bắt đầu từ số 1 (thứ Hai), nên thứ Tư là 星期三."),
                    new("Từ nào có nghĩa là \"Bây giờ / Hiện tại\" trong tiếng Trung?", "今天", "明天", "现在", "昨天", "C", "现在 (xiànzài) có nghĩa là bây giờ, hiện nay."),
                    new("Câu nào có trật tự trạng ngữ thời gian ĐÚNG chuẩn ngữ pháp tiếng Trung?", "我明天上午九点去学校。", "我去学校明天上午九点。", "我去明天上午九点学校。", "明天去学校我上午九点。", "A", "Trạng ngữ thời gian luôn đứng TRƯỚC động từ chính: 我 + 明天上午九点 + 去学校."),
                    new("Để hỏi \"Bây giờ là mấy giờ rồi?\", câu hỏi chuẩn xác là gì?", "现在几点了？", "现在多少点了？", "现在什么点了？", "现在哪点了？", "A", "现在几点了？ (Xiànzài jǐ diǎn le?) là câu hỏi giờ chuẩn HSK1."),
                    new("Hôm nay là: \"十月十二号，星期三。\" Ngày mai là thứ mấy, ngày bao nhiêu?", "Thứ Năm, ngày 13 tháng 10", "Thứ Năm, ngày 11 tháng 10", "Thứ Ba, ngày 13 tháng 10", "Thứ Sáu, ngày 14 tháng 10", "A", "Hôm nay 12/10 thứ Tư thì ngày mai là 13/10 thứ Năm (星期四)."),
                    new("Hẹn: \"我们下午两点半在图书馆见。\" Cuộc hẹn diễn ra khi nào và ở đâu?", "2:30 chiều ở thư viện", "2:30 sáng ở trường học", "2:00 chiều ở thư viện", "3:30 chiều ở quán ăn", "A", "下午两点半 (2h30 chiều) + 在图书馆 (ở thư viện) + 见 (gặp nhau).")
                }),

            BuildCjkQuiz("cjk-quiz:zh:l22", SeedDataInitializer.ChineseLesson22Id,
                "Bài 2.2: Trắc nghiệm Gia đình, Mua sắm & Hỏi giá HSK1", now,
                new CjkQuizQuestion[]
                {
                    new("Hai chữ \"买 (mua)\" và \"卖 (bán)\" phân biệt nhau bởi thanh điệu nào?", "买 mang thanh 3 (mǎi), 卖 mang thanh 4 (mài)", "买 mang thanh 4, 卖 mang thanh 3", "Cả hai đều mang thanh 1", "买 mang thanh 2, 卖 mang thanh 4", "A", "买 đọc thanh 3 (mǎi - mua), 卖 đọc thanh 4 rơi dứt khoát (mài - bán)."),
                    new("Từ \"多少钱\" (bao nhiêu tiền) có phiên âm Pinyin chuẩn là gì?", "duōshao qián", "duōshǎo qiān", "duóshao qǐan", "dūoshǎo qián", "A", "多少钱 có Pinyin là duōshao qián (chữ shao đọc thanh nhẹ)."),
                    new("Chữ Hán \"家\" (nhà, gia đình) gồm bộ Miên (宀 - mái nhà) và chữ Thỉ (豕 - con lợn) ngụ ý điều gì?", "Dưới mái nhà nuôi heo biểu thị cuộc sống no ấm sung túc", "Nuôi heo ngoài đồng cỏ", "Ngôi nhà làm bằng gạch", "Gia đình đông con cháu", "A", "Thời cổ, dưới mái nhà có gia súc là dấu hiệu của gia đình ấm no, sung túc."),
                    new("Lượng từ chuyên biệt dùng để đếm nhân khẩu trong gia đình là gì?", "个 (gè)", "口 (kǒu)", "本 (běn)", "块 (kuài)", "B", "Lượng từ 口 (khẩu) chuyên dùng đếm nhân khẩu trong gia đình (ví dụ: 四口人)."),
                    new("Dạng phủ định DUY NHẤT và bất di bất dịch của động từ \"有\" (yǒu - có) là từ nào?", "没有 (méiyǒu)", "不有 (bùyǒu)", "别有 (biéyǒu)", "无有 (wúyǒu)", "A", "Phủ định của 有 bắt buộc phải dùng 没有, cấm tuyệt đối không nói 不有."),
                    new("Một cân Trung Quốc (一斤 - yì jīn) tương đương bao nhiêu gram?", "1.000g (1kg)", "500g (nửa ký)", "250g", "100g", "B", "1 斤 (Thị cân Trung Quốc) = 500 gram = nửa ký."),
                    new("Điền lượng từ thích hợp cho sách: \"我买了两___汉语书。\"", "个", "本", "口", "块", "B", "Lượng từ dành cho sách, vở, tài liệu đóng gáy là 本 (běn: 两本书)."),
                    new("Cấu trúc cảm thán diễn đạt \"Đắt quá rồi!\" trong tiếng Trung là gì?", "太贵了！", "很贵了！", "真贵了！", "极贵了！", "A", "Cấu trúc cảm thán 太 + Tính từ + 了: 太贵了！ (Tài guì le!)."),
                    new("Khách: \"老板，这个西瓜多少钱一斤？\" Chủ quán: \"两块五一斤。\" Mua 2 cân dưa hấu hết bao nhiêu tiền?", "2.5 tệ", "5 tệ", "10 tệ", "4 tệ", "B", "1 cân giá 2.5 tệ (两块五), mua 2 cân (两斤) = 2.5 x 2 = 5 tệ (五块钱)."),
                    new("\"我家有五口人：爸爸、妈妈、一个哥哥、一个妹妹和我。\" Gia đình người nói có mấy người con?", "2 người con", "3 người con", "4 người con", "5 người con", "B", "Gồm anh trai, em gái và tôi = 3 người con (bố mẹ là 2 người lớn).")
                }),

            BuildCjkQuiz("cjk-quiz:zh:l31", SeedDataInitializer.ChineseLesson31Id,
                "Bài 3.1: Trắc nghiệm Ăn uống, Gọi món & Sở thích HSK1", now,
                new CjkQuizQuestion[]
                {
                    new("Động từ \"吃\" (ăn) thuộc nhóm âm nào và phát âm chuẩn ra sao?", "Âm uốn lưỡi bật hơi mạnh (chī)", "Âm đầu lưỡi không bật hơi (cī)", "Âm mặt lưỡi (qī)", "Âm răng môi (fī)", "A", "ch là âm uốn lưỡi bật hơi cực mạnh, phát âm chuẩn là chī."),
                    new("Chữ thứ hai trong từ \"喜欢\" (thích) phát âm thanh gì?", "Thanh 1", "Thanh 3", "Khinh thanh (thanh nhẹ - xǐhuan)", "Thanh 4", "C", "Trong từ 喜欢, chữ huan đọc thanh nhẹ (xǐhuan)."),
                    new("Chữ Hán nào sau đây có nghĩa là \"trà / nước chè\"?", "茶 (chá)", "菜 (cài)", "水 (shuǐ)", "米 (mǐ)", "A", "茶 đọc là chá có nghĩa là trà."),
                    new("Cụm từ nào có nghĩa là \"một bát cơm\"?", "一杯米饭", "一碗米饭", "一盘米饭", "一个米饭", "B", "Bát cơm dùng lượng từ 碗 (wǎn): 一碗米饭 (yì wǎn mǐfàn)."),
                    new("Động từ năng nguyện \"会\" (huì) trong câu \"我会说汉语\" biểu thị điều gì?", "Biết một kỹ năng thông qua quá trình học tập rèn luyện", "Mong muốn chủ quan nhẹ nhàng", "Bắt buộc phải làm", "Hành động đang tiếp diễn", "A", "会 biểu thị kỹ năng đạt được qua học tập, rèn luyện."),
                    new("Khi khen món ăn ngon miệng, người Trung Quốc dùng từ nào?", "好看", "好吃", "好听", "好喝", "B", "Đồ ăn ngon dùng 好吃 (hǎochī). Đồ uống ngon dùng 好喝 (hǎohē)."),
                    new("Câu nào sau đây sử dụng câu hỏi lựa chọn \"还是\" (hay là) ĐÚNG ngữ pháp?", "你想喝茶还是喝咖啡？", "你想喝茶还是喝咖啡吗？", "你想喝茶或者喝咖啡吗？", "你或者喝茶还是咖啡？", "A", "Câu hỏi lựa chọn dùng 还是 và TUYỆT ĐỐI không có trợ từ 吗 ở cuối."),
                    new("Khi gọi món lịch sự trong quán ăn, ta dùng mẫu câu đề nghị nào?", "请给我...", "你给我...", "我买你...", "请在...", "A", "请给我... (Qǐng gěi wǒ... - Làm ơn cho tôi...) là mẫu câu gọi món lịch sự."),
                    new("Khách gọi: \"我要一盘饺子和两碗牛肉面。\" Khách đã gọi những món gì?", "1 đĩa sủi cảo và 2 bát mì bò", "2 đĩa sủi cảo và 1 bát mì bò", "1 bát cơm và 2 đĩa rau", "1 đĩa sủi cảo và 1 bát canh", "A", "一盘饺子 (1 đĩa sủi cảo) + 两碗牛肉面 (2 bát mì bò)."),
                    new("A: \"你会做中国菜吗？\" B: \"我不会做，但是我非常喜欢吃！\" Hỏi: B có biết nấu món Trung không và B có thích ăn không?", "Không biết nấu nhưng rất thích ăn", "Biết nấu và rất thích ăn", "Biết nấu nhưng không thích ăn", "Không biết nấu và không thích ăn", "A", "B nói: 我不会做 (Tôi không biết nấu) nhưng 非常喜欢吃 (Vô cùng thích ăn).")
                }),

            BuildCjkQuiz("cjk-quiz:zh:l32", SeedDataInitializer.ChineseLesson32Id,
                "Bài 3.2: Trắc nghiệm Phương hướng, Di chuyển & Lịch trình HSK1", now,
                new CjkQuizQuestion[]
                {
                    new("Hai động từ \"坐\" (ngồi/đi xe) và \"走\" (đi bộ) có Pinyin tương ứng là gì?", "zuò (thanh 4) và zǒu (thanh 3)", "zǒu và zuò", "cuò và cǒu", "zhuò và zhǒu", "A", "坐 đọc là zuò (thanh 4 rơi dứt khoát), 走 đọc là zǒu (thanh 3 xuống-lên)."),
                    new("Từ \"火车站\" (ga tàu hỏa) có Pinyin chuẩn là gì?", "huǒchēzhàn", "huǒchèzhàn", "huōchēzhàn", "huǒchēzàn", "A", "火车站 có Pinyin là huǒchēzhàn."),
                    new("Phương vị từ nào sau đây có nghĩa là \"phía trước\"?", "前面 (qiánmian)", "后面 (hòumian)", "左边 (zuǒbian)", "右边 (yòubian)", "A", "前面 (qiánmian) là phía trước. 后面 là phía sau."),
                    new("Từ nào có nghĩa là \"tàu điện ngầm\" trong tiếng Trung?", "出租车", "地铁", "公共汽车", "飞机", "B", "地铁 (dìtiě) là tàu điện ngầm. 出租车 là taxi, 公共汽车 là xe buýt."),
                    new("Câu liên động nào đúng ngữ pháp khi nói \"Tôi đến thư viện đọc sách\"?", "我去图书馆看书。", "我看书去图书馆。", "我去图书馆为了看书。", "我在图书馆去看书。", "A", "Câu liên động xếp theo trình tự thời gian xảy ra: Đi đến thư viện trước, rồi đọc sách sau (我去图书馆看书)."),
                    new("Mẫu câu nào dùng để hỏi cách đi: \"Đi đến bệnh viện đi như thế nào?\"", "去医院怎么走？", "医院去怎么走？", "去医院在哪儿？", "去医院什么走？", "A", "去 + Địa điểm + 怎么走？ là mẫu câu hỏi đường đi chuẩn HSK1."),
                    new("Câu nào diễn đạt \"Tôi đi xe buýt đến trường\" ĐÚNG chuẩn trật tự tiếng Trung?", "我坐公共汽车去学校。", "我去学校坐公共汽车。", "我坐公共汽车学校去。", "学校去我坐公共汽车。", "A", "Trật tự chuẩn: 坐 + Phương tiện + 去 + Địa điểm (我坐公共汽车去学校)."),
                    new("Vị trí của phương vị từ đối với danh từ: \"Phía trước trường học\" nói là gì?", "学校前面", "前面学校", "在前学校", "学校的前", "A", "Danh từ đứng TRƯỚC phương vị từ: 学校 (trường học) + 前面 (phía trước)."),
                    new("Chỉ đường: \"一直往前走，就在银行后面。\" Địa điểm cần tìm nằm ở đâu?", "Đi thẳng về phía trước, ở phía sau ngân hàng", "Rẽ trái, ở phía trước ngân hàng", "Đi lùi lại, ở trong ngân hàng", "Rẽ phải, đối diện ngân hàng", "A", "一直往前走 (Đi thẳng về phía trước) + 银行后面 (Phía sau ngân hàng)."),
                    new("\"今天下午我坐出租车去火车站买票。\" Người nói làm việc gì vào chiều nay?", "Đi taxi ra ga tàu hỏa mua vé", "Đi tàu điện ra bến xe mua vé", "Đi bộ ra sân bay đón bạn", "Đi xe buýt về nhà", "A", "坐出租车 (đi taxi) + 去火车站 (đến ga tàu hỏa) + 买票 (mua vé).")
                }),

            BuildCjkQuiz("cjk-quiz:zh:final-hsk1", SeedDataInitializer.ChineseLesson32Id,
                "Đề thi thử Tổng kết Chuẩn hóa Tiếng Trung HSK1", now,
                new CjkQuizQuestion[]
                {
                    new("[HSK1 Ngữ âm] Từ nào sau đây có chứa âm uốn lưỡi và thanh 1 cao phẳng (55)?", "老师 (lǎoshī)", "谢谢 (xièxie)", "学生 (xuésheng)", "朋友 (péngyou)", "A", "Chữ 师 trong 老师 có âm uốn lưỡi sh và thanh 1 cao phẳng shī."),
                    new("[HSK1 Biến điệu] Cụm từ \"一个人\" phát âm thanh điệu của chữ \"一\" như thế nào?", "yí (thanh 2)", "yī (thanh 1)", "yì (thanh 4)", "yi (thanh nhẹ)", "A", "一 đứng trước thanh 4 (gè) biến điệu thành thanh 2: yí gè rén."),
                    new("[HSK1 Chữ Hán] Chữ Hán nào mang nghĩa là \"uống\"?", "吃", "喝", "看", "听", "B", "喝 (hē) có nghĩa là uống. 吃 là ăn, 看 là nhìn/xem, 听 là nghe."),
                    new("[HSK1 Lượng từ] Điền cặp lượng từ thích hợp: \"桌子上有三___苹果和两___书。\"", "个 / 本", "本 / 个", "口 / 张", "块 / 支", "A", "Táo dùng lượng từ 个 (三个苹果); sách dùng lượng từ 本 (两本书)."),
                    new("[HSK1 Từ vựng] Đơn vị tiền tệ thường dùng trong khẩu ngữ tiếng Trung hàng ngày là gì?", "块 (kuài)", "元 (yuán)", "分 (fēn)", "角 (jiǎo)", "A", "Khẩu ngữ hàng ngày luôn dùng 块 (kuài). 元 thường dùng trong văn bản hoặc in trên bao bì."),
                    new("[HSK1 Phủ định] Câu phủ định nào sau đây là HOÀN TOÀN ĐÚNG ngữ pháp?", "我没有中国朋友。", "我不有中国朋友。", "我不中国朋友。", "我没是不朋友。", "A", "Phủ định của 有 bắt buộc phải dùng 没有: 我没有中国朋友 (Tôi không có bạn Trung Quốc)."),
                    new("[HSK1 Trật tự câu] Sắp xếp các từ sau thành câu đúng: (1) 去 (2) 坐飞机 (3) 他 (4) 北京", "3 - 2 - 1 - 4 (他坐飞机去北京。)", "3 - 1 - 4 - 2 (他去北京坐飞机。)", "2 - 3 - 1 - 4 (坐飞机他去北京。)", "1 - 4 - 3 - 2 (去北京他坐飞机。)", "A", "Trật tự chuẩn: Chủ ngữ + 坐 + Phương tiện + 去 + Địa điểm (他坐飞机去北京)."),
                    new("[HSK1 Tiếp diễn] Để diễn đạt hành động đang tiếp diễn \"Tôi đang học tiếng Trung\", câu đúng là gì?", "我在学习汉语。", "我是学习汉语。", "我看学习汉语。", "我有学习汉语。", "A", "Chữ 在 đứng trước động từ đóng vai trò phó từ chỉ hành động đang diễn ra (我在学习汉语)."),
                    new("[HSK1 Đọc hiểu] \"我叫王明，我现在在河内工作。我每天早上七点半起床，八点坐出租车去公司。\" Vương Minh đi làm bằng phương tiện gì và lúc mấy giờ?", "Đi taxi lúc 8:00", "Đi xe buýt lúc 7:30", "Đi tàu điện lúc 8:00", "Đi bộ lúc 7:00", "A", "Đoạn văn viết: 八点坐出租车去公司 (8 giờ đi taxi đến công ty)."),
                    new("[HSK1 Đọc hiểu] Khách hỏi tính tiền: \"一盘牛肉三十五块，两碗米饭四块，一共多少钱？\" Phục vụ tính tổng cộng bao nhiêu?", "39 tệ", "35 tệ", "40 tệ", "37 tệ", "A", "Thịt bò 35 tệ + 2 bát cơm 4 tệ = Tổng cộng 39 tệ (三十九块钱).")
                })

        };
    }
}
