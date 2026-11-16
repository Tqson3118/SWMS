# Tuần 09 — SQL schema, constraints và normalization

Chặng 3; 3 bài L09-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W09-01: Thiết kế Users/Categories/Expenses với PK/FK và data types đúng.

- LO-W09-02: Nhận diện update anomaly và tách bảng theo functional dependency.

- LO-W09-03: Tạo CHECK/UNIQUE/FK và seed dữ liệu hai user để kiểm ownership.

Tiên quyết: Đạt gate tuần 08; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-09-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w09-01"></a>

## Bài 1: Relational model và khóa

LO-W09-01 · W09-C01

Table là tập rows có schema, PK định danh duy nhất và not-null, FK bảo vệ quan hệ. SQL Server uniqueidentifier phù hợp Guid; bigint cho amount, date cho spentOn, nvarchar cho Unicode. Không lưu số tiền đã format '50.000₫' để SUM. Surrogate Id không thay business unique constraint: Category cần UNIQUE(UserId,Name). FK CategoryId đơn lẻ chỉ chứng minh category tồn tại, không chứng minh cùng owner, nên FK(CategoryId,UserId) tới unique Categories(Id,UserId). Constraint là lớp bảo vệ bất kể API/import nào ghi; validation UI hỗ trợ feedback nhưng không đủ. Nullability phải theo domain, note nullable còn amount/owner không-null.

<a id="lo-w09-02"></a>

## Bài 2: Normalization thực dụng

LO-W09-02 · W09-C02

1NF giữ giá trị atomic theo nhu cầu truy vấn, không chuỗi id '1,2,3'; 2NF loại dependency một phần với composite key; 3NF hạn chế non-key phụ thuộc qua non-key. Đừng lưu CategoryName lặp trên mọi Expense nếu muốn tên current: đổi1 category sẽ không cập nhật mọi row, gây update anomaly. Snapshot tên historical là chủ ý khác cần ADR, không tự gọi duplication sai. Budget phụ thuộc UserId/CategoryId/MonthStart nên unique bộ đó. Normalize trước đo query; denormalize cho read model có cơ chế đồng bộ khi bằng chứng cần. Không gộp tất cả JSON chỉ để tránh JOIN.

<a id="lo-w09-03"></a>

## Bài 3: Constraints và seed reproducible

LO-W09-03 · W09-C03

CHECK AmountVnd>0 và <=1e9 đi cùng NOT NULL vì CHECK có thể cho UNKNOWN do null. UNIQUE giữ không trùng business key; constraint fail trả error để API map400/409, không nuốt. Chọn NO ACTION khi xóa category còn expenses để tránh mất lịch sử. Seed dùng IDs cố định và transaction trong DB lab, không overwrite data thật. Đọc SELECT DB_NAME() trước script; tạo database lab thủ công và không DROP/TRUNCATE. Script repeatable dùng guard theo key, nhưng race chống duplicate vẫn do UNIQUE. Quan sát constraint reject trái user ngay cả insert trực tiếp ngoài API.

## Ví dụ giải từng bước

1. Bài toán: chứng minh category Alice không dùng cho expense Bob.
2. Dùng schema temp độc lập với int id để minh họa compound FK; production dùng Guid như spec.
3. Chạy cùng một session SQL:
```sql
CREATE TABLE #Users(Id int PRIMARY KEY);
CREATE TABLE #Categories(
 Id int PRIMARY KEY,UserId int NOT NULL,Name nvarchar(60) NOT NULL,
 UNIQUE(Id,UserId),UNIQUE(UserId,Name));
CREATE TABLE #Expenses(
 Id int PRIMARY KEY,UserId int NOT NULL,CategoryId int NOT NULL,
 AmountVnd bigint NOT NULL CHECK(AmountVnd>0 AND AmountVnd<=1000000000),
 SpentOn date NOT NULL);
INSERT #Users VALUES(1),(2);
INSERT #Categories VALUES(10,1,N'Food'),(20,2,N'Food');
-- SQL Server không enforce FK trên temp tables: dùng phép kiểm logic demo bên dưới.
SELECT c.Id FROM #Categories c WHERE c.Id=10 AND c.UserId=2; -- 0 rows
INSERT #Expenses VALUES(1,1,10,50000,'20261001');
SELECT SUM(AmountVnd) AS Total FROM #Expenses; -- 50000
```
4. Trong schema persistent dbo của project, tạo FK(UserId)->Users và compound FK(CategoryId,UserId)->Categories(Id,UserId); insert Bob/10 phải bị DB reject. Temp example không đủ để chứng minh FK; gate dùng bảng thật ExpenseLab.

## Đào sâu và lỗi thường gặp

SQL Server temp tables không enforce FK nên phải thử constraint trên bảng lab persistent. Constraint nhiều key có thể tạo index liên quan; FK không tự bảo đảm index tối ưu cho lookup child. UserId không được client tùy ý gửi để quyết định ownership.

## Bài tập thực hành

### Bài tập 1

Viết CREATE TABLE dbo Categories/Expenses theo spec và giải thích compound FK.

### Bài tập 2

Bảng Expense có CategoryName current lặp100 rows; đổi tên gây gì?

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Tạo Users trước, Categories có alternate UNIQUE(Id,UserId), Expenses FK2col. Test insert Alice/own thành công, Bob/categoryAlice fail, amount0 fail, duplicate categoryName/user fail. Null amount bị NOT NULL.

### Hướng giải 2

Nếu update một row có nhiều tên khác nhau cùng category -> anomaly. Tách Categories và JOIN lấy current name. Nếu muốn lịch sử, name snapshot được ghi rõ khi tạo và không đồng bộ rename; hai mô hình khác nhau.

## Project task

Tạo ExpenseLab và schema đầy đủ spec, seed Alice/Bob và3 expenses. Lưu ops/schema.sql + seed.sql trong repo học viên; không tạo/xóa bảng trên DB có dữ liệu cần giữ.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w09.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] SELECT fixture Alice3 rows,total100000,Bob0.

- [ ] Bảng thật reject amount0/null/overmax và cross-owner CategoryId.

- [ ] UNIQUE chặn category trùng cùng user, cho cùng tên khác user; delete category có rows bị chặn.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'Why is a foreign key on category ID alone insufficient for ownership?' Viết PK/FK/constraint/update anomaly.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Primary and foreign key constraints](https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints) — W09-C01; primary. HTTP: 200; section xem ledger.

- [Database normalization basics](https://learn.microsoft.com/en-us/office/troubleshoot/access/database-normalization-description) — W09-C02; primary. HTTP: 200; section xem ledger.

- [UNIQUE constraints and CHECK constraints](https://learn.microsoft.com/en-us/sql/relational-databases/tables/unique-constraints-and-check-constraints) — W09-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-09-quiz.md) · [Đáp án](../../assessments/answer-keys/week-09-answers.md) · [Flashcards](../../flashcards/week-09.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](../module-2/week-08.md)

[Tuần tiếp](week-10.md)
