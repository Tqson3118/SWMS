namespace Swms.Api.Infrastructure.Database.Seed;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Swms.Api.Domain.Entities;
using Swms.Api.Features;

/// <summary>
/// Seeds high-quality FSRS flashcards for Foreign Language courses:
/// 1. Japanese N5 (>= 60 cards across JapaneseLesson11Id..JapaneseLesson32Id) with Tag 'N5-Japanese'
/// 2. Chinese HSK1 (>= 60 cards across ChineseLesson11Id..ChineseLesson32Id) with Tag 'HSK1-Chinese'
/// 3. English A2-B1 (>= 60 cards across EnglishLesson11Id..EnglishLesson32Id) with Tag 'English-A2-B1'
/// 4. IELTS Academic (>= 60 supplementary academic cards) with Tag 'IELTS-Academic'
/// 100% idempotent via deterministic SHA256 GUIDs and existing database checks.
/// </summary>
public static class LanguageFlashcardsSeed
{
    private static readonly JsonSerializerOptions SafeJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static Guid CreateDeterministicGuid(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }

    public static async Task EnsureSeededAsync(SwmsDbContext db, DateTimeOffset now)
    {
        await SeedJapaneseN5CardsAsync(db, now);
        await SeedChineseHsk1CardsAsync(db, now);
        await SeedEnglishA2B1CardsAsync(db, now);
        await SeedIeltsAcademicCardsAsync(db, now);
    }

    private static async Task<Tag> EnsureTagAsync(SwmsDbContext db, string tagName, string colorKey, DateTimeOffset now)
    {
        var tagId = CreateDeterministicGuid($"swms:tag:{SeedDataInitializer.DefaultUserId:N}:{tagName.Trim().ToLowerInvariant()}");
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Id == tagId || (t.UserId == SeedDataInitializer.DefaultUserId && t.Name == tagName));
        if (tag == null)
        {
            tag = new Tag
            {
                Id = tagId,
                UserId = SeedDataInitializer.DefaultUserId,
                Name = tagName,
                ColorKey = colorKey,
                CreatedAt = now
            };
            db.Tags.Add(tag);
            await db.SaveChangesAsync();
        }
        return tag;
    }

    private static async Task SeedCardsForLessonListAsync(
        SwmsDbContext db,
        string domainKey,
        Tag tag,
        (Guid LessonId, string Front, string Back)[] specs,
        DateTimeOffset now)
    {
        var targetLessonIds = specs.Select(s => s.LessonId).Distinct().ToList();
        var existingLessonIds = (await db.Lessons
            .Where(l => targetLessonIds.Contains(l.Id))
            .Select(l => l.Id)
            .ToListAsync())
            .ToHashSet();

        if (existingLessonIds.Count == 0)
        {
            return;
        }

        var fallbackLessonId = existingLessonIds.First();

        var existingFronts = (await db.Flashcards
            .Where(f => f.UserId == SeedDataInitializer.DefaultUserId)
            .Select(f => f.FrontAstJson)
            .ToListAsync())
            .ToHashSet();

        var existingCardIds = (await db.Flashcards
            .Where(f => f.UserId == SeedDataInitializer.DefaultUserId)
            .Select(f => f.Id)
            .ToListAsync())
            .ToHashSet();

        var existingTagPairs = (await db.FlashcardTags
            .Where(ft => ft.TagId == tag.Id)
            .Select(ft => ft.FlashcardId)
            .ToListAsync())
            .ToHashSet();

        var cardsToAdd = new List<Flashcard>();
        var tagsToAdd = new List<FlashcardTag>();

        for (int i = 0; i < specs.Length; i++)
        {
            var (lessonId, front, back) = specs[i];
            var actualLessonId = existingLessonIds.Contains(lessonId) ? lessonId : fallbackLessonId;
            var cardId = CreateDeterministicGuid($"swms:lang-fc:{domainKey}:{actualLessonId:N}:{i + 1}");
            var frontJson = JsonSerializer.Serialize(new { text = front }, SafeJsonOptions);
            var backJson = JsonSerializer.Serialize(new { text = back }, SafeJsonOptions);

            if (!existingCardIds.Contains(cardId) && !existingFronts.Contains(frontJson))
            {
                var card = new Flashcard
                {
                    Id = cardId,
                    UserId = SeedDataInitializer.DefaultUserId,
                    LessonId = actualLessonId,
                    CardType = "basic",
                    SourceTitleSnapshot = $"Thẻ từ vựng {tag.Name}",
                    FrontAstJson = frontJson,
                    BackAstJson = backJson,
                    Stability = 0,
                    Difficulty = 0,
                    Reps = 0,
                    Lapses = 0,
                    State = 0,
                    IsArchived = false,
                    DueAt = now.AddMinutes(-i),
                    CreatedAt = now,
                    UpdatedAt = now
                };
                cardsToAdd.Add(card);
                existingCardIds.Add(cardId);
                existingFronts.Add(frontJson);
            }

            if (!existingTagPairs.Contains(cardId))
            {
                tagsToAdd.Add(new FlashcardTag
                {
                    FlashcardId = cardId,
                    TagId = tag.Id,
                    CreatedAt = now
                });
                existingTagPairs.Add(cardId);
            }
        }

        if (cardsToAdd.Count > 0)
        {
            db.Flashcards.AddRange(cardsToAdd);
        }
        if (tagsToAdd.Count > 0)
        {
            db.FlashcardTags.AddRange(tagsToAdd);
        }
        if (cardsToAdd.Count > 0 || tagsToAdd.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }

    #region 1. Japanese N5 Flashcards (>= 60 cards)
    private static async Task SeedJapaneseN5CardsAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var tag = await EnsureTagAsync(db, "N5-Japanese", "rose", now);
        var l11 = SeedDataInitializer.JapaneseLesson11Id;
        var l12 = SeedDataInitializer.JapaneseLesson12Id;
        var l21 = SeedDataInitializer.JapaneseLesson21Id;
        var l22 = SeedDataInitializer.JapaneseLesson22Id;
        var l31 = SeedDataInitializer.JapaneseLesson31Id;
        var l32 = SeedDataInitializer.JapaneseLesson32Id;

        var specs = new (Guid, string, string)[]
        {
            // Bài 1.1: Bảng chữ cái Hiragana/Katakana & Chào hỏi cơ bản (10 thẻ)
            (l11, "おはようございます (Ohayou gozaimasu)", "Chào buổi sáng (lịch sự, dùng với cấp trên, thầy cô hoặc người lớn tuổi).\nVí dụ: おはようございます、先生！(Chào buổi sáng thầy ạ!)"),
            (l11, "こんにちは (Konnichiwa)", "Xin chào (dùng vào ban ngày từ trưa đến chiều tối).\nVí dụ: 田中さん、こんにちは。(Chào anh Tanaka.)"),
            (l11, "こんばんは (Konbanwa)", "Chào buổi tối (dùng sau khi trời tối).\nVí dụ: みなさん、こんばんは。(Chào buổi tối mọi người.)"),
            (l11, "ありがとうございます (Arigatou gozaimasu)", "Xin cảm ơn rất nhiều (trang trọng).\nVí dụ: どうもありがとうございます。(Xin chân thành cảm ơn bạn.)"),
            (l11, "すみません (Sumimasen)", "Xin lỗi / Làm phiền một chút (dùng khi gây bất tiện hoặc gọi phục vụ).\nVí dụ: すみません、お水をください。(Xin lỗi, cho tôi xin nước lọc với.)"),
            (l11, "はじめまして (Hajimemashite)", "Rất hân hạnh được gặp bạn lần đầu (bắt đầu màn tự giới thiệu).\nVí dụ: はじめまして、ソンと申します。(Hân hạnh gặp bạn, tôi tên là Sơn.)"),
            (l11, "よろしくお願いします (Yoroshiku onegaishimasu)", "Rất mong nhận được sự giúp đỡ của bạn (kết thúc màn tự giới thiệu).\nVí dụ: これからよろしくお願いします。(Từ nay mong được bạn giúp đỡ nhiều.)"),
            (l11, "さようなら (Sayounara)", "Tạm biệt (khi lâu ngày mới gặp lại hoặc chia tay trang trọng).\nVí dụ: 先生、さようなら。(Em chào thầy em về ạ.)"),
            (l11, "じゃ、また (Ja, mata)", "Hẹn gặp lại nhé (thân mật giữa bạn bè, đồng nghiệp cùng trang lứa).\nVí dụ: じゃ、また明日！(Hẹn gặp lại ngày mai nhé!)"),
            (l11, "はい / いいえ (Hai / Iie)", "Vâng, đúng rồi / Không, không phải.\nVí dụ: はい、学生です。(Vâng, tôi là học sinh.) / いいえ、違います。(Không, không phải.)"),

            // Bài 1.2: Giới thiệu bản thân & Nghề nghiệp / Quốc tịch (10 thẻ)
            (l12, "私 (わたし - Watashi)", "Tôi (đại từ nhân xưng ngôi thứ nhất trung tính, lịch sự).\nVí dụ: 私はベトナム人です。(Tôi là người Việt Nam.)"),
            (l12, "学生 (がくせい - Gakusei)", "Học sinh, sinh viên.\nVí dụ: 兄は大学の学生です。(Anh trai tôi là sinh viên đại học.)"),
            (l12, "会社員 (かいしゃいん - Kaishain)", "Nhân viên công ty.\nVí dụ: 父はIT会社員です。(Bố tôi là nhân viên công ty công nghệ.)"),
            (l12, "エンジニア (Enjinia)", "Kỹ sư (Software Engineer, Hardware Engineer).\nVí dụ: 私はソフトウェアエンジニアです。(Tôi là kỹ sư phần mềm.)"),
            (l12, "ベトナム (Betonamu) & 日本 (にほん - Nihon)", "Việt Nam & Nhật Bản.\nVí dụ: ベトナムから来ました。(Tôi đến từ Việt Nam.)"),
            (l12, "先生 (せんせい - Sensei)", "Thầy cô giáo / Bác sĩ (dùng để xưng hô kính trọng với người dạy).\nVí dụ: 山田先生は親切です。(Thầy Yamada rất tốt bụng và chu đáo.)"),
            (l12, "何歳 (なんさい - Nansai) / おいくつ", "Bao nhiêu tuổi (おいくつ là cách hỏi trang trọng hơn).\nVí dụ: おいくつですか。(Bạn năm nay bao nhiêu tuổi rồi ạ?)"),
            (l12, "〜から来ました (~kara kimashita)", "Tôi đến từ ~ (chỉ xuất thân, quê quán).\nVí dụ: ハノイから来ました。(Tôi đến từ Hà Nội.)"),
            (l12, "友達 (ともだち - Tomodachi)", "Bạn bè.\nVí dụ: 彼は私の高校の友達です。(Anh ấy là bạn cấp ba của tôi.)"),
            (l12, "だれ (Dare) / どなた (Donata)", "Ai (どなた là kính ngữ trang trọng của だれ).\nVí dụ: あの方はどなたですか。(Vị kia là ai vậy ạ?)"),

            // Bài 2.1: Số đếm, Thời gian, Giờ giấc & Lịch hẹn (10 thẻ)
            (l21, "今何時 (いまなんじ - Ima nanji)", "Bây giờ là mấy giờ?\nVí dụ: すみません、今何時ですか。(Xin lỗi, bây giờ là mấy giờ rồi ạ?)"),
            (l21, "〜時〜分 (~ji ~fun)", "Cấu trúc nói giờ và phút.\nVí dụ: 今は午前9時30分です。(Bây giờ là 9 giờ 30 phút sáng.)"),
            (l21, "何曜日 (なんようび - Nan-youbi)", "Thứ mấy trong tuần?\nVí dụ: 今日は何曜日ですか。(Hôm nay là thứ mấy thế?)"),
            (l21, "月曜日〜日曜日 (Getsuyoubi ~ Nichiyoubi)", "Thứ Hai đến Chủ Nhật.\nVí dụ: 月曜日から金曜日まで働きます。(Tôi làm việc từ thứ Hai đến thứ Sáu.)"),
            (l21, "午前 (ごぜん - Gozen) & 午後 (ごご - Gogo)", "Buổi sáng (AM) & Buổi chiều (PM).\nVí dụ: 会議は午後2時から始まります。(Cuộc họp bắt đầu lúc 2 giờ chiều.)"),
            (l21, "半 (はん - Han)", "Rưỡi (30 phút).\nVí dụ: 7時半に朝ごはんを食べます。(Tôi ăn sáng lúc 7 giờ rưỡi.)"),
            (l21, "昨日・今日・明日 (Kinou / Kyou / Ashita)", "Hôm qua / Hôm nay / Ngày mai.\nVí dụ: 明日は休みです。(Ngày mai là ngày nghỉ.)"),
            (l21, "毎日 (まいにち - Mainichi)", "Mỗi ngày, hàng ngày.\nVí dụ: 毎日日本語を勉強します。(Hàng ngày tôi đều học tiếng Nhật.)"),
            (l21, "〜から〜まで (~kara ~made)", "Từ ~ đến ~ (thời gian hoặc địa điểm).\nVí dụ: 8時半から5時まで仕事をします。(Tôi làm việc từ 8 giờ rưỡi đến 5 giờ.)"),
            (l21, "休み (やすみ - Yasumi)", "Nghỉ ngơi, ngày nghỉ.\nVí dụ: 会社の休みは土曜日と日曜日です。(Ngày nghỉ của công ty là thứ Bảy và Chủ Nhật.)"),

            // Bài 2.2: Động từ sinh hoạt hàng ngày (Ikimasu, Tabemasu, Nomimasu...) (10 thẻ)
            (l22, "起きます (おきます - Okimasu)", "Thức dậy.\nVí dụ: 毎朝6時に起きます。(Mỗi sáng tôi thức dậy lúc 6 giờ.)"),
            (l22, "寝ます (ねます - Nemasu)", "Đi ngủ.\nVí dụ: 夜11時に寝ます。(Tôi đi ngủ lúc 11 giờ đêm.)"),
            (l22, "行きます (いきます - Ikimasu)", "Đi đến (dùng trợ từ へ hoặc に).\nVí dụ: 電車で会社へ行きます。(Tôi đi đến công ty bằng tàu điện.)"),
            (l22, "来ます (きます - Kimasu)", "Đến (hướng về phía người nói).\nVí dụ: 友達が家に来ました。(Bạn tôi đã đến nhà chơi.)"),
            (l22, "帰ります (かえります - Kaerimasu)", "Trở về (nhà, nước mẹ đẻ).\nVí dụ: 6時にうちへ帰ります。(Tôi về nhà lúc 6 giờ.)"),
            (l22, "食べます (たべます - Tabemasu)", "Ăn (dùng trợ từ を với đồ ăn).\nVí dụ: 朝ごはんを食べましたか。(Bạn đã ăn sáng chưa?)"),
            (l22, "飲みます (のみます - Nomimasu)", "Uống.\nVí dụ: カフェでコーヒーを飲みます。(Tôi uống cà phê ở quán cà phê.)"),
            (l22, "見ます (みます - Mimasu)", "Xem, nhìn, ngắm.\nVí dụ: 週末に映画を見ます。(Cuối tuần tôi xem phim.)"),
            (l22, "聞きます (ききます - Kikimasu)", "Nghe (nhạc, tin tức) / Hỏi.\nVí dụ: 音楽を聞きながら勉強します。(Vừa nghe nhạc vừa học bài.)"),
            (l22, "勉強します (べんきょうします - Benkyou shimasu)", "Học tập.\nVí dụ: 毎晩2時間日本語を勉強します。(Mỗi tối tôi học tiếng Nhật 2 tiếng.)"),

            // Bài 3.1: Mua sắm, Đồ vật, Giá cả & Chỉ vị trí (10 thẻ)
            (l31, "これ・それ・あれ (Kore / Sore / Are)", "Cái này (gần người nói) / Cái đó (gần người nghe) / Cái kia (xa cả hai).\nVí dụ: これはいくらですか。(Cái này giá bao nhiêu tiền vậy?)"),
            (l31, "いくら (Ikura)", "Bao nhiêu tiền?\nVí dụ: このシャツはいくらですか。(Chiếc áo sơ mi này giá bao nhiêu?)"),
            (l31, "〜円 (えん - En)", "Đồng Yên Nhật (JPY).\nVí dụ: 全部で3,000円です。(Tổng cộng là 3.000 Yên.)"),
            (l31, "ここ・そこ・あそこ (Koko / Soko / Asoko)", "Chỗ này / Chỗ đó / Chỗ đằng kia.\nVí dụ: トイレはあそこです。(Nhà vệ sinh ở đằng kia.)"),
            (l31, "本 (ほん - Hon) & 辞書 (じしょ - Jisho)", "Sách & Từ điển.\nVí dụ: 日本語の辞書を買いました。(Tôi đã mua từ điển tiếng Nhật.)"),
            (l31, "パソコン (Pasokon)", "Máy tính cá nhân (laptop/PC).\nVí dụ: 新しいパソコンが欲しいです。(Tôi muốn có một chiếc laptop mới.)"),
            (l31, "上・下・前・後ろ (Ue / Shita / Mae / Ushiro)", "Trên / Dưới / Trước / Sau.\nVí dụ: 机の上に本があります。(Trên bàn có cuốn sách.)"),
            (l31, "中・隣・近く (Naka / Tonari / Chikaku)", "Bên trong / Bên cạnh / Gần.\nVí dụ: 鞄の中に財布があります。(Trong cặp có chiếc ví tiền.)"),
            (l31, "ください (Kudasai)", "Xin hãy cho tôi ~ (khi mua hàng).\nVí dụ: これを2つください。(Cho tôi xin 2 cái này nhé.)"),
            (l31, "駅 (えき - Eki)", "Nhà ga xe điện.\nVí dụ: 新宿駅はどこですか。(Ga Shinjuku ở đâu vậy ạ?)"),

            // Bài 3.2: Rủ rê, Đề xuất (〜ませんか / 〜ましょう) & Thể quá khứ (10 thẻ)
            (l32, "〜ませんか (~masen ka)", "Bạn cùng ~ với tôi nhé? (câu rủ rê lịch sự).\nVí dụ: いっしょに昼ごはんを食べませんか。(Cùng đi ăn trưa với mình nhé?)"),
            (l32, "〜ましょう (~mashou)", "Cùng ~ nào! / Để tôi ~ cho (đồng ý hoặc khởi xướng nhiệt tình).\nVí dụ: ええ、行きましょう！(Ừ, cùng đi thôi nào!)"),
            (l32, "〜ました (~mashita)", "Đã ~ (đuôi chia động từ thì quá khứ lịch sự).\nVí dụ: 昨日図書館で勉強しました。(Hôm qua tôi đã học ở thư viện.)"),
            (l32, "〜ませんでした (~masendeshita)", "Đã không ~ (quá khứ phủ định lịch sự).\nVí dụ: 昨夜はあまり寝ませんでした。(Đêm qua tôi đã không ngủ được nhiều.)"),
            (l32, "どこか (Dokoka) & なにか (Nanika)", "Ở đâu đó / Cái gì đó.\nVí dụ: 週末どこかへ行きましたか。(Cuối tuần bạn có đi đâu chơi không?)"),
            (l32, "写真 (しゃしん - Shashin)", "Bức ảnh, chụp ảnh (写真を撮ります).\nVí dụ: いっしょに写真を撮りましょう。(Chúng ta cùng chụp một kiểu ảnh nhé!)"),
            (l32, "おいしい (Oishii)", "Ngon (món ăn, đồ uống).\nVí dụ: 日本のラーメンはとてもおいしいです。(Mì Ramen của Nhật rất là ngon.)"),
            (l32, "好き (すき - Suki)", "Thích (đi với trợ từ が).\nVí dụ: 私は日本のアニメが好きです。(Tôi rất thích phim hoạt hình Nhật Bản.)"),
            (l32, "少し (すこし - Sukoshi)", "Một chút, một ít.\nVí dụ: 日本語が少し分かります。(Tôi hiểu tiếng Nhật một chút.)"),
            (l32, "大丈夫 (だいじょうぶ - Daijoubu)", "Ổn, không sao, an toàn.\nVí dụ: 大丈夫ですか。手伝いましょうか。(Bạn có sao không? Để tôi giúp một tay nhé.)")
        };

        await SeedCardsForLessonListAsync(db, "jp-n5", tag, specs, now);
    }
    #endregion

    #region 2. Chinese HSK1 Flashcards (>= 60 cards)
    private static async Task SeedChineseHsk1CardsAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var tag = await EnsureTagAsync(db, "HSK1-Chinese", "amber", now);
        var l11 = SeedDataInitializer.ChineseLesson11Id;
        var l12 = SeedDataInitializer.ChineseLesson12Id;
        var l21 = SeedDataInitializer.ChineseLesson21Id;
        var l22 = SeedDataInitializer.ChineseLesson22Id;
        var l31 = SeedDataInitializer.ChineseLesson31Id;
        var l32 = SeedDataInitializer.ChineseLesson32Id;

        var specs = new (Guid, string, string)[]
        {
            // Bài 1.1: Thanh điệu Pinyin & Lời chào căn bản (10 thẻ)
            (l11, "你好 (nǐ hǎo)", "Xin chào (lời chào phổ biến nhất giữa bạn bè, người mới quen).\nVí dụ: 你好！很高兴认识你。(Xin chào! Rất vui được gặp bạn.)"),
            (l11, "您好 (nín hǎo)", "Kính chào ngài/thầy (cách xưng hô tôn kính với người lớn tuổi, khách hàng).\nVí dụ: 老师，您好！(Em kính chào thầy cô ạ!)"),
            (l11, "再见 (zàijiàn)", "Tạm biệt (hẹn gặp lại lần sau).\nVí dụ: 明天见，再见！(Mai gặp lại nhé, tạm biệt!)"),
            (l11, "谢谢 (xièxie)", "Cảm ơn.\nVí dụ: 太谢谢你了！(Cảm ơn bạn nhiều lắm!)"),
            (l11, "不客气 (bú kèqi)", "Không có chi, đừng khách sáo (đáp lại lời cảm ơn).\nVí dụ: 不客气，这是我应该做的。(Không có chi, đây là việc tôi nên làm mà.)"),
            (l11, "对不起 (duìbuqǐ)", "Xin lỗi (khi gây lỗi lầm hoặc phiền toái).\nVí dụ: 对不起，我迟到了。(Xin lỗi, tôi đến muộn mất rồi.)"),
            (l11, "没关系 (méi guānxi)", "Không sao đâu, không có hề gì (đáp lại lời xin lỗi).\nVí dụ: 没关系，请进！(Không sao đâu, xin mời vào!)"),
            (l11, "早 (zǎo) / 早上好 (zǎoshang hǎo)", "Chào buổi sáng!\nVí dụ: 大家早上好！(Chào buổi sáng cả nhà!)"),
            (l11, "明天见 (míngtiān jiàn)", "Hẹn gặp lại vào ngày mai.\nVí dụ: 今天辛苦了，明天见！(Hôm nay vất vả rồi, mai gặp lại nhé!)"),
            (l11, "好 (hǎo)", "Tốt, được, đồng ý.\nVí dụ: 好，没问题！(Được rồi, không thành vấn đề!)"),

            // Bài 1.2: Đại từ nhân xưng & Giới thiệu tên tuổi (10 thẻ)
            (l12, "我・你・他・她 (wǒ / nǐ / tā / tā)", "Tôi / Bạn / Anh ấy / Cô ấy.\nVí dụ: 我们都是学生。(Chúng tôi đều là học sinh.)"),
            (l12, "叫 (jiào)", "Tên là, gọi là (dùng nói họ tên).\nVí dụ: 我叫李明。(Tôi tên là Lý Minh.)"),
            (l12, "什么 (shénme)", "Cái gì? (đại từ nghi vấn).\nVí dụ: 你叫什么名字？(Bạn tên là gì thế?)"),
            (l12, "是 (shì)", "Là (động từ to-be trong tiếng Trung).\nVí dụ: 我是越南人。(Tôi là người Việt Nam.)"),
            (l12, "不 (bù)", "Không (phó từ phủ định hiện tại và tương lai).\nVí dụ: 我不是老师。(Tôi không phải là giáo viên.)"),
            (l12, "学生 (xuésheng)", "Học sinh, sinh viên.\nVí dụ: 他是大学生。(Anh ấy là sinh viên đại học.)"),
            (l12, "老师 (lǎoshī)", "Thầy cô giáo.\nVí dụ: 王老师教汉语。(Thầy Vương dạy tiếng Hán.)"),
            (l12, "中国人 / 越南人 (Zhōngguó rén / Yuènán rén)", "Người Trung Quốc / Người Việt Nam.\nVí dụ: 你是中国人吗？(Bạn có phải là người Trung Quốc không?)"),
            (l12, "朋友 (péngyou)", "Bạn bè.\nVí dụ: 他是我的好朋友。(Anh ấy là bạn thân của tôi.)"),
            (l12, "很高兴认识你 (hěn gāoxìng rènshi nǐ)", "Rất vui được làm quen với bạn!\nVí dụ: 认识你我也很高兴。(Gặp bạn tôi cũng rất vui.)"),

            // Bài 2.1: Số đếm, Ngày tháng & Thời gian (10 thẻ)
            (l21, "一二三四五六七八九十 (yī .. shí)", "Số đếm 1 đến 10 trong tiếng Trung.\nVí dụ: 我有三个汉语老师。(Tôi có ba người giáo viên tiếng Hán.)"),
            (l21, "几 (jǐ)", "Mấy? (hỏi số lượng dưới 10).\nVí dụ: 现在几点？(Bây giờ là mấy giờ rồi?)"),
            (l21, "点 (diǎn) & 分 (fēn)", "Giờ & Phút.\nVí dụ: 现在八点半。(Bây giờ là 8 giờ rưỡi.)"),
            (l21, "今天・明天・昨天 (jīntiān / míngtiān / zuótiān)", "Hôm nay / Ngày mai / Hôm qua.\nVí dụ: 今天星期几？(Hôm nay là thứ mấy?)"),
            (l21, "星期 (xīngqī)", "Tuần / Thứ (星期一 đến 星期天).\nVí dụ: 星期日我不去学校。(Chủ nhật tôi không đến trường.)"),
            (l21, "月 (yuè) & 号 (hào)", "Tháng & Ngày (mùng).\nVí dụ: 今天是十月一号。(Hôm nay là ngày 1 tháng 10.)"),
            (l21, "岁 (suì)", "Tuổi.\nVí dụ: 你今年多大？我二十岁。(Năm nay bạn bao nhiêu tuổi? Tôi 20 tuổi.)"),
            (l21, "年 (nián)", "Năm.\nVí dụ: 我在中国学了一年汉语。(Tôi đã học tiếng Hán 1 năm ở Trung Quốc.)"),
            (l21, "上午 / 下午 (shàngwǔ / xiàwǔ)", "Buổi sáng / Buổi chiều.\nVí dụ: 明天上午我有课。(Sáng mai tôi có tiết học.)"),
            (l21, "什么时候 (shénme shíhou)", "Khi nào, lúc nào?\nVí dụ: 你什么时候回家？(Khi nào thì bạn về nhà?)"),

            // Bài 2.2: Gia đình & Đồ vật xung quanh (10 thẻ)
            (l22, "家 (jiā)", "Nhà, gia đình.\nVí dụ: 我家有四口人。(Nhà tôi có 4 người.)"),
            (l22, "爸爸・妈妈 (bàba / māma)", "Bố & Mẹ.\nVí dụ: 爸爸妈妈身体很好。(Bố mẹ tôi sức khỏe đều rất tốt.)"),
            (l22, "哥哥・姐姐 (gēge / jiějie)", "Anh trai & Chị gái.\nVí dụ: 我有一个哥哥和一个姐姐。(Tôi có 1 anh trai và 1 chị gái.)"),
            (l22, "弟弟・妹妹 (dìdi / mèimei)", "Em trai & Em gái.\nVí dụ: 妹妹在读小学。(Em gái tôi đang học tiểu học.)"),
            (l22, "有 / 没有 (yǒu / méiyǒu)", "Có / Không có (phủ định của 有 bắt buộc dùng 没).\nVí dụ: 我没有电脑。(Tôi không có máy tính.)"),
            (l22, "书 (shū) & 本 (běn)", "Sách & Quyển (lượng từ của sách).\nVí dụ: 我买了两本书。(Tôi đã mua 2 quyển sách.)"),
            (l22, "这 / 那 (zhè / nà)", "Đây / Đó, kia.\nVí dụ: 这是谁的书？(Đây là cuốn sách của ai thế?)"),
            (l22, "谁 (shéi / shuí)", "Ai?\nVí dụ: 那个漂亮的女孩是谁？(Cô gái xinh đẹp đằng kia là ai thế?)"),
            (l22, "桌子・椅子 (zhuōzi / yǐzi)", "Bàn & Ghế.\nVí dụ: 椅子上有两只猫。(Trên ghế có hai chú mèo.)"),
            (l22, "猫・狗 (māo / gǒu)", "Mèo & Chó.\nVí dụ: 你喜欢猫还是狗？(Bạn thích mèo hay là chó?)"),

            // Bài 3.1: Ăn uống, Mua sắm & Giá tiền (10 thẻ)
            (l31, "吃 (chī) & 喝 (hē)", "Ăn & Uống.\nVí dụ: 你想吃什么？(Bạn muốn ăn món gì?)"),
            (l31, "米饭 (mǐfàn)", "Cơm trắng.\nVí dụ: 来一碗米饭！(Cho một bát cơm trắng với!)"),
            (l31, "茶 (chá) & 水 (shuǐ)", "Trà & Nước lọc.\nVí dụ: 请喝茶！(Xin mời bạn dùng trà!)"),
            (l31, "苹果 (píngguǒ)", "Quả táo.\nVí dụ: 苹果多少钱一斤？(Táo bao nhiêu tiền một cân thế?)"),
            (l31, "多少钱 (duōshao qián)", "Bao nhiêu tiền?\nVí dụ: 这个手机多少钱？(Chiếc điện thoại này giá bao nhiêu?)"),
            (l31, "块 (kuài) / 元 (yuán)", "Đồng tệ (đơn vị tiền tệ Trung Quốc).\nVí dụ: 一共二十五块钱。(Tổng cộng là 25 đồng tệ.)"),
            (l31, "买 (mǎi)", "Mua.\nVí dụ: 我想买一台新电脑。(Tôi muốn mua một chiếc máy tính mới.)"),
            (l31, "太...了 (tài... le)", "Quá... rồi! (cấu trúc biểu thị mức độ cao).\nVí dụ: 太贵了，便宜点儿吧！(Đắt quá rồi, bớt một chút đi!)"),
            (l31, "菜 (cài)", "Món ăn / Rau củ.\nVí dụ: 中国菜很好吃。(Món ăn Trung Quốc rất ngon.)"),
            (l31, "饭馆 (fànguǎn)", "Nhà hàng, quán ăn.\nVí dụ: 中午我们在饭馆吃饭。(Buổi trưa chúng tôi ăn ở nhà hàng.)"),

            // Bài 3.2: Phương hướng, Nơi chốn & Hoạt động sinh hoạt (10 thẻ)
            (l32, "在哪儿 (zài nǎr)", "Ở đâu?\nVí dụ: 请问，洗手间在哪儿？(Xin hỏi, nhà vệ sinh ở đâu ạ?)"),
            (l32, "学校 (xuéxiào) & 医院 (yīyuàn)", "Trường học & Bệnh viện.\nVí dụ: 他在医院工作。(Anh ấy làm việc ở bệnh viện.)"),
            (l32, "上 / 下 / 里 (shàng / xià / lǐ)", "Trên / Dưới / Trong.\nVí dụ: 书在桌子上。(Cuốn sách nằm ở trên bàn.)"),
            (l32, "前面 / 后面 (qiánmian / hòumian)", "Phía trước / Phía sau.\nVí dụ: 学校前面有一家商店。(Phía trước trường học có một cửa hàng.)"),
            (l32, "商店 (shāngdiàn)", "Cửa hàng, tiệm tạp hóa.\nVí dụ: 我去商店买水。(Tôi đi ra cửa hàng mua nước uống.)"),
            (l32, "出租车 (chūzūchē)", "Xe taxi.\nVí dụ: 我们坐出租车去机场。(Chúng tôi đi taxi ra sân bay.)"),
            (l32, "看见 (kànjiàn)", "Nhìn thấy, trông thấy.\nVí dụ: 你看见我的钥匙了吗？(Bạn có nhìn thấy chìa khóa của tôi đâu không?)"),
            (l32, "听 (tīng) & 说 (shuō)", "Nghe & Nói.\nVí dụ: 你会说汉语吗？(Bạn có biết nói tiếng Hán không?)"),
            (l32, "喜欢 (xǐhuan)", "Thích, yêu thích.\nVí dụ: 我很喜欢学中文。(Tôi rất thích học tiếng Trung.)"),
            (l32, "爱 (ài)", "Yêu, thương.\nVí dụ: 我爱我的爸爸妈妈。(Tôi yêu bố mẹ của tôi.)")
        };

        await SeedCardsForLessonListAsync(db, "zh-hsk1", tag, specs, now);
    }
    #endregion

    #region 3. English A2-B1 Flashcards (>= 60 cards)
    private static async Task SeedEnglishA2B1CardsAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var tag = await EnsureTagAsync(db, "English-A2-B1", "sky", now);
        var l11 = SeedDataInitializer.EnglishLesson11Id;
        var l12 = SeedDataInitializer.EnglishLesson12Id;
        var l21 = SeedDataInitializer.EnglishLesson21Id;
        var l22 = SeedDataInitializer.EnglishLesson22Id;
        var l31 = SeedDataInitializer.EnglishLesson31Id;
        var l32 = SeedDataInitializer.EnglishLesson32Id;

        var specs = new (Guid, string, string)[]
        {
            // Bài 1.1: Small Talk & Tự giới thiệu chuyên nghiệp (10 thẻ)
            (l11, "Break the ice", "Phrasal idiom: Bắt chuyện phá tan sự ngượng ngùng lúc ban đầu.\nVí dụ: He told a lighthearted joke to break the ice before the workshop started."),
            (l11, "Touch base with someone", "Cụm giao tiếp công sở: Liên lạc nhanh để nắm tình hình / cập nhật thông tin.\nVí dụ: Let's touch base next Monday to review the sprint deliverables."),
            (l11, "Catch up on something", "Bắt kịp tiến độ / tâm sự cập nhật chuyện đời sống sau một thời gian xa cách.\nVí dụ: We met for coffee to catch up on each other's work and life."),
            (l11, "What do you do for a living?", "Mẫu câu hỏi nghề nghiệp tự nhiên thay cho 'What is your job?'.\nVí dụ: I am currently working as a full-stack software engineer at a fintech startup."),
            (l11, "Keep in touch", "Giữ liên lạc với nhau nhé.\nVí dụ: It was wonderful meeting you today. Let's definitely keep in touch via LinkedIn!"),
            (l11, "Hit it off", "Hợp cạ / trở nên thân thiết ngay từ lần đầu gặp mặt.\nVí dụ: We hit it off immediately because we share the same passion for open-source AI."),
            (l11, "Out of curiosity", "Vì tò mò nên hỏi thăm (hỏi lịch sự không tọc mạch).\nVí dụ: Out of curiosity, what framework did you use to build this dashboard?"),
            (l11, "Make yourself at home", "Cứ tự nhiên như ở nhà nhé (hiếu khách).\nVí dụ: Please come in and make yourself at home while I brew some hot tea."),
            (l11, "Run into someone", "Tình cờ gặp gỡ ai đó ngoài đường mà không hẹn trước.\nVí dụ: I ran into my university classmate at the airport this morning."),
            (l11, "Give someone a hand", "Giúp ai đó một tay.\nVí dụ: Could you give me a hand with moving these conference tables?"),

            // Bài 1.2: Hỏi đường, Đặt phòng & Xử lý tình huống du lịch / công tác (10 thẻ)
            (l12, "Excuse me, could you point me in the right direction of...?", "Mẫu câu hỏi đường lịch sự chuẩn bản ngữ.\nVí dụ: Excuse me, could you point me in the right direction of the central metro station?"),
            (l12, "Book in advance", "Đặt trước (vé, phòng khách sạn, bàn tiệc).\nVí dụ: It is strongly recommended to book flight tickets at least two weeks in advance."),
            (l12, "Check in / Check out", "Làm thủ tục nhận phòng / trả phòng khách sạn hoặc sân bay.\nVí dụ: Standard hotel check-in time is 2:00 PM and check-out is 12:00 PM noon."),
            (l12, "Fully booked", "Đã kín chỗ / hết phòng hoàn toàn.\nVí dụ: I'm afraid all our ocean-view rooms are fully booked for the holiday weekend."),
            (l12, "Within walking distance", "Trong khoảng cách đi bộ được (rất gần).\nVí dụ: The conference hall is within five minutes' walking distance from our hotel."),
            (l12, "Take a detour", "Đi đường vòng (để tránh kẹt xe hoặc thi công).\nVí dụ: Because of road repairs, the taxi driver had to take a quick detour."),
            (l12, "Drop someone off", "Thả ai đó xuống xe tại điểm dừng.\nVí dụ: Can the shuttle bus drop us off right in front of Terminal 2?"),
            (l12, "Lost and found", "Quầy nhận đồ thất lạc.\nVí dụ: If you accidentally leave your umbrella on the train, visit the station's lost and found desk."),
            (l12, "Complimentary breakfast", "Bữa sáng miễn phí đi kèm dịch vụ lưu trú.\nVí dụ: All deluxe rooms include a complimentary buffet breakfast on the 3rd floor."),
            (l12, "Flight delay / cancellation", "Hoãn chuyến bay / Hủy chuyến bay.\nVí dụ: Due to adverse weather conditions, flight VN250 has been delayed by 45 minutes."),

            // Bài 2.1: Viết Email Công Việc & Báo Cáo Tiến Độ (10 thẻ)
            (l21, "Follow up on something", "Cụm từ email: Theo dõi / nhắc lại một vấn đề đã thảo luận trước đó.\nVí dụ: I am writing to follow up on the client proposal we submitted last Wednesday."),
            (l21, "Please find attached...", "Mẫu câu gửi tệp đính kèm trong thư điện tử.\nVí dụ: Please find attached the updated architectural design document for your review."),
            (l21, "Action item", "Đầu mục công việc cần hoàn thành với người phụ trách cụ thể.\nVí dụ: Here are the three key action items agreed upon during today's retrospective."),
            (l21, "Keep someone in the loop", "Giữ ai đó luôn nắm được thông tin tiến độ cập nhật.\nVí dụ: Please CC me on all client emails to keep our management team in the loop."),
            (l21, "Meet a tight deadline", "Kịp hạn chót rất gấp rút.\nVí dụ: The engineering team worked diligently to meet a tight deadline for the Q4 release."),
            (l21, "Fall behind schedule", "Bị chậm tiến độ so với kế hoạch ban đầu.\nVí dụ: We fell behind schedule due to unexpected third-party API downtime."),
            (l21, "On behalf of...", "Thay mặt cho, đại diện cho.\nVí dụ: On behalf of the engineering department, I would like to congratulate our new team leads."),
            (l21, "As per our conversation", "Như đã trao đổi qua cuộc trò chuyện của chúng ta.\nVí dụ: As per our morning conversation, I have adjusted the unit test coverage requirements to 85%."),
            (l21, "At your earliest convenience", "Vào lúc thuận tiện sớm nhất cho bạn (lịch sự thay cho 'as soon as possible').\nVí dụ: Please sign and return the NDA document at your earliest convenience."),
            (l21, "Do not hesitate to reach out", "Đừng ngần ngại liên hệ nhé (câu kết email chu đáo).\nVí dụ: Should you have any technical questions, please do not hesitate to reach out to me."),

            // Bài 2.2: Họp Trực Tuyến & Phản Biện Lịch Sự (10 thẻ)
            (l22, "You are on mute", "Bạn đang tắt mic kìa (câu kinh điển khi họp Google Meet / Zoom).\nVí dụ: John, you are on mute! We cannot hear your project overview."),
            (l22, "Share my screen", "Chia sẻ màn hình hiển thị nội dung slide/code.\nVí dụ: Allow me to share my screen so everyone can view the system workflow diagram."),
            (l22, "Cut in / Jump in", "Xin phép chen ngang để bổ sung ý kiến trong cuộc họp.\nVí dụ: May I jump in here for a moment to clarify the database migration constraint?"),
            (l22, "I see your point, but...", "Tôi hiểu quan điểm của bạn, tuy nhiên... (phản biện lịch sự).\nVí dụ: I see your point, but introducing microservices right now might add unnecessary operational overhead."),
            (l22, "On the same page", "Cùng chung cách hiểu / đồng thuận quan điểm.\nVí dụ: Before concluding the kickoff meeting, let's ensure all team members are on the same page."),
            (l22, "Take this offline", "Bàn chuyện này riêng sau buổi họp để tránh tốn thời gian chung.\nVí dụ: This bug seems complex; let's take this offline between senior engineers after the standup."),
            (l22, "Wrap up", "Tổng kết / kết thúc buổi họp.\nVí dụ: We have covered all agenda items, so let's wrap up today's session five minutes early."),
            (l22, "Audio glitch / Lag", "Lỗi âm thanh bị rè / đường truyền bị giật lag.\nVí dụ: Sorry, your voice cut out for a few seconds due to network lag. Could you repeat that?"),
            (l22, "Play devil's advocate", "Đóng vai người phản biện trái chiều để kiểm tra độ vững chắc của phương án.\nVí dụ: Just to play devil's advocate, what happens if our primary payment gateway goes down during Black Friday?"),
            (l22, "Reach a consensus", "Đạt được sự đồng thuận chung của cả tập thể.\nVí dụ: After intense deliberation, the stakeholders reached a unanimous consensus on the roadmap."),

            // Bài 3.1: Thuyết Trình Dự Án & Kể Chuyện Dữ Liệu (10 thẻ)
            (l31, "Walk you through...", "Dẫn dắt các bạn đi qua từng phần của bài thuyết trình.\nVí dụ: Today I would like to walk you through our vertical slice architecture refactoring."),
            (l31, "As you can see from this chart...", "Như các bạn có thể thấy từ biểu đồ này...\nVí dụ: As you can see from this line chart, page load latency dropped by 42% after enabling Redis caching."),
            (l31, "Bottom line", "Điểm cốt lõi nhất / kết luận quan trọng nhất.\nVí dụ: The bottom line is that investing in automated testing reduces production bugs by two-thirds."),
            (l31, "Drill down into details", "Đi sâu chi tiết vào một phần cụ thể.\nVí dụ: Now let us drill down into the database indexing strategy that resolved the query bottleneck."),
            (l31, "Key takeaway", "Bài học rút ra chính / điểm quan trọng cần ghi nhớ.\nVí dụ: The key takeaway from this post-mortem is to always implement circuit breakers for external integrations."),
            (l31, "Pique someone's interest", "Kích thích trí tò mò / thu hút sự chú ý của thính giả ngay từ mở bài.\nVí dụ: Start your pitch with an alarming statistic to pique the investors' interest."),
            (l31, "Draw your attention to...", "Thu hút sự chú ý của các bạn vào điểm này...\nVí dụ: I would like to draw your attention to the spike in mobile user signups in Q3."),
            (l31, "To summarize", "Tóm tắt lại các ý chính.\nVí dụ: To summarize, our proposal offers higher throughput, lower latency, and zero licensing fees."),
            (l31, "Open the floor for Q&A", "Mở phần hỏi đáp tự do cho khán giả đặt câu hỏi.\nVí dụ: That concludes my presentation; I would now like to open the floor for any questions."),
            (l31, "Compelling evidence", "Bằng chứng thuyết phục, rõ ràng.\nVí dụ: We presented compelling empirical evidence demonstrating that the new FSRS algorithm outperforms SM-2."),

            // Bài 3.2: Đàm Phán Hợp Đồng, Thương Lượng & Thuyết Phục (10 thẻ)
            (l32, "Win-win situation", "Đôi bên cùng có lợi (kết quả đàm phán lý tưởng).\nVí dụ: We aim for a partnership agreement that creates a genuine win-win situation for both parties."),
            (l32, "Reach a compromise", "Đạt được sự thỏa hiệp nhượng bộ lẫn nhau để đi đến hợp đồng.\nVí dụ: Both sides had to compromise on delivery milestones to finalize the commercial deal."),
            (l32, "Drive a hard bargain", "Đàm phán rất cứng rắn / đòi hỏi chặt chẽ để giành lợi thế tối đa.\nVí dụ: The procurement manager drove a hard bargain and secured an additional 15% discount."),
            (l32, "Ball is in your court", "Tới lượt bạn ra quyết định / hành động tiếp theo.\nVí dụ: We have sent our counter-offer with revised SLA terms; the ball is now in their court."),
            (l32, "Deal breaker", "Điều khoản bất khả nhượng bộ (nếu không đạt được thì hủy đàm phán).\nVí dụ: Complete ownership of intellectual property rights was a non-negotiable deal breaker for us."),
            (l32, "Sweeten the deal", "Tăng thêm quyền lợi / quà tặng để đối tác dễ dàng đồng ý chốt hợp đồng.\nVí dụ: They sweetened the deal by offering 24/7 dedicated enterprise support free of charge for the first year."),
            (l32, "Back to the drawing board", "Quay lại từ đầu để thiết kế lại phương án vì kế hoạch cũ không được chấp thuận.\nVí dụ: If the client rejects this architectural proposal, we will have to go back to the drawing board."),
            (l32, "Iron out the details", "Giải quyết dứt điểm các vướng mắc chi tiết nhỏ nhặt còn lại.\nVí dụ: The legal teams spent two days ironing out the liability clauses before signing."),
            (l32, "Cave in", "Chịu nhượng bộ trước áp lực của đối phương.\nVí dụ: Under intense client pushback, the agency finally caved in and agreed to shorten the rollout timeline."),
            (l32, "In good faith", "Thiện chí, chân thành, đáng tin cậy.\nVí dụ: Both companies entered the joint-venture negotiations in good faith.")
        };

        await SeedCardsForLessonListAsync(db, "en-a2-b1", tag, specs, now);
    }
    #endregion

    #region 4. IELTS Academic Flashcards (>= 60 supplementary academic cards)
    private static async Task SeedIeltsAcademicCardsAsync(SwmsDbContext db, DateTimeOffset now)
    {
        var tag = await EnsureTagAsync(db, "IELTS-Academic", "violet", now);

        // Find IELTS course lessons in DB
        var ieltsCourse = await db.Courses.FirstOrDefaultAsync(c => c.Slug == "document-course-ielts");
        if (ieltsCourse == null) return;

        var ieltsLessons = await db.Lessons
            .Where(l => db.Chapters.Any(ch => ch.Id == l.ChapterId && ch.CourseId == ieltsCourse.Id))
            .OrderBy(l => l.SortOrder)
            .ToListAsync();

        if (ieltsLessons.Count == 0) return;

        var defaultFallbackLesson = ieltsLessons.First();

        Lesson ResolveLesson(string slugFragment)
        {
            var matched = ieltsLessons.FirstOrDefault(l => (l.Slug ?? string.Empty).Contains(slugFragment, StringComparison.OrdinalIgnoreCase));
            return matched ?? defaultFallbackLesson;
        }

        var vocabLesson = ResolveLesson("vocabulary");
        var writingTask1Lesson = ResolveLesson("phase-1");
        var writingTask2Lesson = ResolveLesson("phase-3");
        var readingLesson = ResolveLesson("phase-2");

        var rawSpecs = new (Lesson targetLesson, string front, string back)[]
        {
            // Group 1: Academic Collocations for Writing & Speaking (15 thẻ)
            (vocabLesson, "Profound impact on...", "Collocation học thuật (Band 7.5+): Tác động sâu sắc / mạnh mẽ lên cái gì.\nParaphrase: Big effect / Huge influence.\nVí dụ: The widespread adoption of generative AI has had a profound impact on tertiary education pedagogy."),
            (vocabLesson, "Pave the way for...", "Collocation: Mở đường cho / Tạo tiền đề cho phát triển tương lai.\nParaphrase: Make it easier for / Facilitate.\nVí dụ: Breakthroughs in quantum computing are expected to pave the way for next-generation drug discovery."),
            (vocabLesson, "Play a pivotal role in...", "Collocation: Đóng vai trò then chốt / mang tính bước ngoặt.\nParaphrase: Very important role.\nVí dụ: Rigorous government regulations play a pivotal role in curbing carbon emissions from industrial zones."),
            (vocabLesson, "Exert negative pressure on...", "Collocation: Gây áp lực tiêu cực lên...\nParaphrase: Put bad stress on.\nVí dụ: Rapid unmanaged urbanization exerts severe pressure on municipal healthcare and public transport infrastructure."),
            (vocabLesson, "Stem from...", "Collocation: Bắt nguồn từ / Xuất phát từ nguyên nhân cốt lõi.\nParaphrase: Caused by / Arise from.\nVí dụ: Chronic lifestyle diseases often stem from a sedentary routine and diets high in processed sugars."),
            (vocabLesson, "Prevalent phenomenon", "Collocation: Hiện tượng phổ biến / thường gặp trong xã hội.\nParaphrase: Common thing.\nVí dụ: Remote working has evolved from a temporary contingency into a prevalent phenomenon in the tech sector."),
            (vocabLesson, "Strike a balance between...", "Collocation: Đạt được sự cân bằng hài hòa giữa hai mặt đối lập.\nParaphrase: Balance two things.\nVí dụ: City planners must strike a delicate balance between modern high-rise development and heritage conservation."),
            (vocabLesson, "Incur heavy costs / expenses", "Collocation: Gánh chịu chi phí tốn kém nặng nề.\nParaphrase: Pay high cost.\nVí dụ: Households that ignore energy efficiency tend to incur heavy electricity bills during peak summer months."),
            (vocabLesson, "Mitigate the adverse effects of...", "Collocation: Giảm nhẹ các tác động tiêu cực / bất lợi của...\nParaphrase: Reduce bad results.\nVí dụ: Afforestation campaigns are vital to mitigate the adverse effects of soil erosion and climate volatility."),
            (vocabLesson, "Pose a formidable challenge to...", "Collocation: Đặt ra thách thức cam go / ghê gớm đối với...\nParaphrase: Create big difficulty.\nVí dụ: The rapid proliferation of deepfake content poses a formidable challenge to online media authenticity."),
            (vocabLesson, "Foster critical thinking skills", "Collocation: Bồi dưỡng / nuôi dưỡng tư duy phản biện sắc bén.\nParaphrase: Help people think carefully.\nVí dụ: Project-based learning environments actively foster critical thinking and collaborative problem-solving."),
            (vocabLesson, "Mounting evidence suggests that...", "Collocation: Bằng chứng ngày càng nhiều chỉ ra rằng...\nParaphrase: More and more proof shows.\nVí dụ: Mounting clinical evidence suggests that prolonged screen exposure impairs adolescents' REM sleep cycles."),
            (vocabLesson, "Call for urgent action", "Collocation: Kêu gọi hành động khẩn cấp không chậm trễ.\nParaphrase: Need quick response.\nVí dụ: Environmental scientists call for urgent multilateral action to protect endangered marine ecosystems."),
            (vocabLesson, "Exacerbate existing inequalities", "Collocation: Làm trầm trọng thêm những bất bình đẳng sẵn có.\nParaphrase: Make unfairness much worse.\nVí dụ: Disparities in broadband access threaten to exacerbate existing educational inequalities in rural districts."),
            (vocabLesson, "A catalyst for economic growth", "Collocation: Chất xúc tác thúc đẩy tăng trưởng kinh tế nhanh chóng.\nParaphrase: Thing that pushes growth.\nVí dụ: High-speed rail networks have proven to be a catalyst for regional economic growth across Southeast Asia."),

            // Group 2: Paraphrasing Pairs for Reading & Writing Task 2 (15 thẻ)
            (writingTask2Lesson, "Many people think that... → A prevailing school of thought maintains that...", "Paraphrase mở bài Task 2 (Band 8.0):\nThay vì viết 'Many people believe that...', hãy dùng 'A prevailing school of thought maintains that...' hoặc 'It is widely contended that...' để tăng tính học thuật và khách quan."),
            (writingTask2Lesson, "Important → Paramount / Indispensable / Crucial", "Paraphrase từ vựng thông dụng:\n'Paramount' (tối quan trọng, có quyền ưu tiên cao nhất);\n'Indispensable' (không thể thiếu được);\n'Crucial' (mang tính quyết định sống còn)."),
            (writingTask2Lesson, "Dangerous / Harmful → Detrimental / Hazardous / Deleterious", "Paraphrase tính từ tiêu cực:\n'Detrimental' (gây tổn hại nghiêm trọng);\n'Hazardous' (nguy hại, độc hại, rủi ro cao);\n'Deleterious' (có hại dài hạn, hủy hoại dần)."),
            (writingTask2Lesson, "Solve a problem → Address / Tackle / Resolve / Ameliorate an issue", "Paraphrase cụm động từ giải quyết vấn đề:\n'Tackle an issue' (xắn tay trực diện giải quyết);\n'Address a problem' (giải quyết thỏa đáng);\n'Ameliorate a condition' (cải thiện tình trạng khó khăn)."),
            (writingTask2Lesson, "Rich people → Affluent individuals / High-net-worth individuals", "Paraphrase đối tượng xã hội:\nTránh lặp 'rich people', thay bằng 'affluent demographics', 'wealthier households', hoặc 'high-earning cohorts'."),
            (writingTask2Lesson, "Poor people → Underprivileged communities / Marginalized populations", "Paraphrase nhân khẩu học thiệt thòi:\n'Underprivileged households' (gia đình có hoàn cảnh khó khăn);\n'Marginalized socioeconomic groups' (nhóm dân cư bị yếu thế bên lề xã hội)."),
            (writingTask2Lesson, "Change → Undergo a profound transformation / Shift paradigm", "Paraphrase biến đổi:\n'Undergo a radical transformation' (trải qua sự chuyển mình toàn diện);\n'Shift the paradigm' (thay đổi mô thức căn bản)."),
            (writingTask2Lesson, "Lead to → Culminate in / Give rise to / Precipitate", "Paraphrase quan hệ nhân quả:\n'Give rise to' (làm nảy sinh);\n'Precipitate' (thúc đẩy nhanh xảy ra đột ngột);\n'Culminate in' (dẫn đến đỉnh điểm là)."),
            (writingTask2Lesson, "Stop / Prevent → Deter / Impede / Curb / Inhibit", "Paraphrase ngăn chặn, hạn chế:\n'Deter crime' (răn đe ngăn chặn tội phạm);\n'Curb inflation' (kìm chế lạm phát);\n'Impede progress' (cản trở bước tiến)."),
            (writingTask2Lesson, "Old people → The elderly / Senior citizens / The aging population", "Paraphrase đối tượng người cao tuổi:\n'Senior citizens' (người cao tuổi);\n'The aging demographic' (nhóm dân số già hóa);\n'Octogenarians' (người từ 80-89 tuổi)."),
            (writingTask2Lesson, "Children → Adolescents / Minors / Youngsters / Offspring", "Paraphrase trẻ em / thanh thiếu niên:\n'Adolescents' (vị thành niên 12-18 tuổi);\n'Minors' (người chưa đến tuổi trưởng thành);\n'Offspring' (con cái trong gia đình)."),
            (writingTask2Lesson, "Famous → Renowned / Acclaimed / Prestigious", "Paraphrase danh tiếng:\n'A renowned academic institution' (viện hàn lâm lừng danh);\n'An internationally acclaimed author' (tác giả được quốc tế ca ngợi)."),
            (writingTask2Lesson, "Obvious → Evident / Manifest / Self-evident", "Paraphrase hiển nhiên, rõ ràng:\n'It is readily evident that...' (Có thể thấy rõ ràng một điều là...);\n'Manifest symptoms' (triệu chứng bộc lộ rõ rệt)."),
            (writingTask2Lesson, "Get / Obtain → Attain / Acquire / Garner", "Paraphrase đạt được, nhận được:\n'Acquire knowledge' (tiếp thu tri thức);\n'Attain academic excellence' (đạt thành tích xuất sắc);\n'Garner public support' (thu hút sự ủng hộ của công chúng)."),
            (writingTask2Lesson, "Show → Illustrate / Exemplify / Demonstrate / Elucidate", "Paraphrase chỉ ra, minh họa:\n'Exemplify' (nêu gương làm mẫu chứng minh);\n'Elucidate a complex hypothesis' (làm sáng tỏ một giả thuyết phức tạp)."),

            // Group 3: Writing Task 1 Academic Trends & Data Descriptions (15 thẻ)
            (writingTask1Lesson, "Surge / Soar / Escalate exponentially", "Miêu tả xu hướng tăng đột biến (Writing Task 1):\nTăng vọt thẳng đứng trong thời gian ngắn.\nVí dụ: Electric vehicle sales surged exponentially from 50,000 units in 2020 to over 400,000 by 2024."),
            (writingTask1Lesson, "Plunge / Plummet / Tumble sharply", "Miêu tả xu hướng giảm mạnh / lao dốc (Writing Task 1):\nRơi thẳng đứng không phanh.\nVí dụ: International tourist arrivals plummeted dramatically by 78% during the global travel restrictions."),
            (writingTask1Lesson, "Fluctuate wildly within a narrow / broad band", "Miêu tả biến động bấp bênh (Writing Task 1):\nDao động lên xuống thất thường quanh một biên độ.\nVí dụ: Crude oil prices fluctuated wildly between $65 and $95 per barrel over the decade."),
            (writingTask1Lesson, "Plateau / Level off at...", "Miêu tả xu hướng đi ngang sau khi tăng/giảm:\nChạm ngưỡng bình ổn và không đổi.\nVí dụ: After five consecutive years of steady growth, mobile penetration leveled off at approximately 88%."),
            (writingTask1Lesson, "Peak at... before declining", "Chạm đỉnh cao nhất tại một con số cụ thể:\nVí dụ: Carbon emissions peaked at 36 gigatons in 2022 before initiating a gradual downward trajectory."),
            (writingTask1Lesson, "Hit a trough of... / Bottom out at...", "Chạm đáy thấp nhất trước khi hồi phục:\nVí dụ: Unemployment rates hit an all-time trough of 3.2% in the second quarter of 2023."),
            (writingTask1Lesson, "Stand at... in stark contrast to...", "So sánh tương phản rõ nét giữa 2 nhóm dữ liệu:\nVí dụ: Literacy rates in urban hubs stood at 96%, in stark contrast to just 58% recorded across remote rural provinces."),
            (writingTask1Lesson, "Account for / Constitute a lion's share of...", "Chiếm tỷ trọng áp đảo / lớn nhất trong biểu đồ tròn (Pie chart):\nVí dụ: Renewable hydropower accounts for a lion's share of the national electricity grid, registering at nearly 54%."),
            (writingTask1Lesson, "Double / Triple / Quadruple / Increase fourfold", "Tăng gấp 2 / gấp 3 / gấp 4 lần:\nVí dụ: The budgetary allocation for digital education tripled over the ten-year period, reaching 12 billion USD."),
            (writingTask1Lesson, "Marginal / Negligible change", "Biến động không đáng kể / rất nhỏ khó nhận thấy:\nVí dụ: There was only a negligible variation in wheat exports between 2018 and 2019, hovering at around 2.1%."),
            (writingTask1Lesson, "Outstrip / Overtake / Surpass", "Vượt mặt / Vượt qua một đại lượng khác trên biểu đồ đường:\nVí dụ: By mid-2021, online retail revenue had decisively outstripped traditional brick-and-mortar store sales."),
            (writingTask1Lesson, "A steady upward / downward trajectory", "Quỹ đạo tăng / giảm đều đặn, ổn định qua các mốc thời gian:\nVí dụ: Per capita GDP maintained a steady upward trajectory throughout the surveyed period."),
            (writingTask1Lesson, "In terms of... / Regarding... / As for...", "Cụm chuyển ý miêu tả tiêu chí mới trong Task 1:\nVí dụ: In terms of renewable energy adoption, Northern European nations comfortably outperformed their peers."),
            (writingTask1Lesson, "Witness / Experience a moderate contraction", "Chứng kiến sự co hẹp / sụt giảm vừa phải:\nVí dụ: The domestic automotive manufacturing sector witnessed a moderate contraction of 6% in Q2."),
            (writingTask1Lesson, "Remain relatively stable throughout the timeline", "Duy trì trạng thái tương đối ổn định trong suốt khoảng thời gian:\nVí dụ: Meat consumption per capita remained remarkably stable throughout the entire fifteen-year timeline."),

            // Group 4: Cohesive Devices & Academic Discourse Markers (15 thẻ)
            (readingLesson, "Notwithstanding the fact that...", "Linking phrase nhượng bộ Band 8.0+ (Mặc dù vậy...):\nĐồng nghĩa với 'Although' / 'Even though', mang giọng văn học thuật trang trọng.\nVí dụ: Notwithstanding the fact that renewable investments require high initial capital, their long-term environmental dividend is undeniable."),
            (readingLesson, "Conversely / In sharp contrast", "Linking phrase tương phản đối lập:\nĐồng nghĩa với 'On the other hand' nhưng học thuật hơn nhiều.\nVí dụ: While large corporations can absorb inflation shocks, small enterprises, conversely, face heightened insolvency risks."),
            (readingLesson, "By the same token", "Linking phrase tương đồng / Tương tự như vậy:\nDùng khi đưa ra một lập luận bổ trợ có cùng bản chất logic.\nVí dụ: Parents must monitor screen time; by the same token, educators need to impart media literacy skills."),
            (readingLesson, "To substantiate this assertion", "Linking phrase dẫn chứng: Để chứng minh cho nhận định này...\nThay thế cao cấp cho 'For example' / 'To illustrate'.\nVí dụ: To substantiate this assertion, historical data from Scandinavian welfare systems demonstrates higher happiness indices."),
            (readingLesson, "It is noteworthy that...", "Linking phrase nhấn mạnh: Một điểm rất đáng lưu ý là...\nDùng để dẫn dắt độc giả chú ý vào dữ liệu mấu chốt.\nVí dụ: It is noteworthy that over 70% of clinical trial respondents reported significant symptom alleviation."),
            (readingLesson, "Hence / Consequently / Thereby", "Linking phrase kết quả logic:\n'Thereby + V-ing' (bằng cách đó, nhờ thế mà).\nVí dụ: Solar micro-grids provide cheap electricity, thereby enabling rural families to study and work after sunset."),
            (readingLesson, "In light of the foregoing analysis", "Linking phrase kết bài Task 2 (Band 8.0+):\nNhìn nhận từ toàn bộ phân tích đã trình bày ở trên...\nThay thế xuất sắc cho cụm đã cũ 'In conclusion'.\nVí dụ: In light of the foregoing analysis, it is justifiable to conclude that state subsidies must prioritize green technology."),
            (readingLesson, "Be predicated on...", "Cụm diễn đạt học thuật: Được xây dựng dựa trên nền tảng / Tiền đề là...\nVí dụ: Modern democratic governance is fundamentally predicated on transparency, accountability, and the rule of law."),
            (readingLesson, "Give precedence to...", "Cụm diễn đạt học thuật: Dành sự ưu tiên hàng đầu cho cái gì.\nVí dụ: Fiscal policymakers ought to give precedence to public health expenditures over prestige infrastructure ventures."),
            (readingLesson, "Irrespective of...", "Bất kể / Không phân biệt điều kiện nào.\nVí dụ: High-quality primary education should be universally accessible to all children, irrespective of their socioeconomic background."),
            (readingLesson, "A double-edged sword", "Cụm thành ngữ học thuật: Con dao hai lưỡi (vừa có lợi vừa có hại).\nVí dụ: Artificial intelligence is undoubtedly a double-edged sword: while it supercharges productivity, it threatens white-collar employment."),
            (readingLesson, "Incurable complacency", "Sự tự mãn khinh suất không thể chữa lành (trong phân tích nguyên nhân thất bại).\nVí dụ: Technological incumbents that fall into complacency often succumb to nimble startup disruptors."),
            (readingLesson, "Corroborate findings", "Củng cố / Chứng thực các kết quả nghiên cứu khoa học.\nVí dụ: Peer-reviewed empirical data has further corroborated the findings published in the initial Lancet report."),
            (readingLesson, "A foregone conclusion", "Một kết cục đã được định trước / Điều hiển nhiên chắc chắn xảy ra.\nVí dụ: It is by no means a foregone conclusion that technological automation will inevitably lead to widespread net joblessness."),
            (readingLesson, "Cast doubt on...", "Dấy lên mối nghi ngờ / Đặt dấu hỏi lớn lên tính xác thực của...\nVí dụ: Recent auditing discoveries have cast serious doubt on the corporate governance claims of the insolvent conglomerate.")
        };

        var specs = rawSpecs.Select(x => (x.targetLesson.Id, x.front, x.back)).ToArray();
        await SeedCardsForLessonListAsync(db, "ielts-acad", tag, specs, now);
    }
    #endregion
}
