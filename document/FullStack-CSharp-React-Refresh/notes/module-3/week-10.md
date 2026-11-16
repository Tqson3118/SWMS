# Tuần 10 — JOIN, GROUP BY, subqueries và CTE

Chặng 3; 3 bài L10-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W10-01: Viết INNER/LEFT JOIN và dự đoán row count khi one-to-many.

- LO-W10-02: Tính category totals, empty groups và filter aggregate đúng.

- LO-W10-03: Dùng EXISTS cho membership và CTE tổ chức query, không coi CTE tự cache.

Tiên quyết: Đạt gate tuần 09; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-10-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w10-01"></a>

## Bài 1: JOIN và cardinality

LO-W10-01 · W10-C01

INNER JOIN giữ matches, LEFT JOIN giữ mọi row bên trái và null cho thiếu bên phải. Join predicate xác định quan hệ, không chỉ cùng tên column. Một category nhiều expenses tạo nhiều joined rows; SUM sau join thêm bảng khác1:N có thể nhân đôi expense. WHERE e.AmountVnd>0 sau LEFT JOIN loại null rows, biến hành vi gần inner; đặt filter child ở ON khi muốn giữ category rỗng. JOIN luôn kèm scope UserId và compound ownership khi cần. SELECT* gây cột thừa/name ambiguity; project rõ fields DTO. Kiểm row count trước aggregate để phát hiện duplicated sums.

<a id="lo-w10-02"></a>

## Bài 2: GROUP BY và HAVING

LO-W10-02 · W10-C02

GROUP BY chia rows theo group key; mọi selected non-aggregate columns phải có trong group theo SQL Server rules. SUM bỏ qua null; COALESCE(SUM(...),0) cho category rỗng khi LEFT JOIN. COUNT(*) đếm row join null synthetic, COUNT(e.Id) đếm expenses thật. WHERE lọc rows trước grouping; HAVING lọc group theo aggregate sau grouping. GROUP BY không tự sort, thêm ORDER BY. Tổng Alice100000, Food80000,Travel20000; category Entertainment không expense vẫn0 nếu LEFT JOIN. Không lấy average bằng integer division ngoài ý muốn, cast decimal khi cần.

<a id="lo-w10-03"></a>

## Bài 3: Subquery, EXISTS và CTE

LO-W10-03 · W10-C03

Subquery nằm trong SELECT/FROM/WHERE; correlated subquery tham chiếu outer row. EXISTS kiểm có row, phù hợp tìm category có expenses, tránh JOIN duplicate khi chỉ cần membership. NOT IN với NULL có thể làm predicate UNKNOWN; dùng NOT EXISTS khi semantics phù hợp. CTE WITH name AS (...) đặt tên query trong một statement, cải thiện readability nhưng không bảo đảm materialize/cache hoặc nhanh hơn. T-SQL dùng ;WITH nếu statement trước chưa kết thúc. Window SUM OVER cho totals bên cạnh row là tùy chọn; core focus readable report. Parameterize user/date, không nối input vào SQL string.

## Ví dụ giải từng bước

1. Bài toán: category Food/Travel/Empty, tổng kể cả rỗng.
2. LEFT JOIN + COALESCE, count child PK.
3. Query độc lập:
```sql
;WITH C AS(
 SELECT * FROM (VALUES(1,N'Food'),(2,N'Travel'),(3,N'Empty'))v(Id,Name)
), E AS(
 SELECT * FROM (VALUES(1,1,CAST(50000 AS bigint)),(2,2,20000),(3,1,30000))v(Id,CategoryId,AmountVnd)
)
SELECT C.Name,COALESCE(SUM(E.AmountVnd),0) Total,COUNT(E.Id) ExpenseCount
FROM C LEFT JOIN E ON E.CategoryId=C.Id
GROUP BY C.Name
ORDER BY Total DESC,C.Name;
```
4. Expected Food80000/2,Travel20000/1,Empty0/0. Đổi COUNT(E.Id)->COUNT(*) sẽ cho Empty1; giải thích synthetic joined row.

## Đào sâu và lỗi thường gặp

N+1 ở app không thể chữa bằng GROUP BY trong memory sau khi đã gọi nhiều query. Query aggregate SQL chỉ trả dữ liệu cần. JOIN multiplicity cần hiểu trước thêm DISTINCT; DISTINCT không sửa tổng đã cộng trùng.

## Bài tập thực hành

### Bài tập 1

Tìm categories có expenses>=30000 dùng EXISTS.

### Bài tập 2

Tính totals>=50000, giữ query ordering rõ.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

WHERE EXISTS(SELECT1 FROM Expenses e WHERE e.CategoryId=c.Id AND e.UserId=c.UserId AND e.AmountVnd>=30000); Food có,Travel không theo fixture. Không duplicate Food dù2 expenses.

### Hướng giải 2

GROUP BY category rồi HAVING SUM(amount)>=50000; fixture chỉ Food80000. WHERE amount>=50000 trước group cho Food50000, là bài toán khác.

## Project task

Lưu report monthly, category totals kể cả0 và membership queries. Báo cáo owner/date scope trước aggregate; read only scripts có parameters.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w10.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Report fixture Food80000/2,Travel20000/1,categoryempty0/0.

- [ ] HAVING total>=50000 trả Food80000; giải thích khác WHERE item>=50000.

- [ ] EXISTS không duplicate category; query Bob không chứa Alice.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'Why did this LEFT JOIN lose empty categories?' Trình bày WHERE vs ON/HAVING với fixture.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Logical and physical joins](https://learn.microsoft.com/en-us/sql/relational-databases/performance/joins) — W10-C01; primary. HTTP: 200; section xem ledger.

- [GROUP BY and HAVING](https://learn.microsoft.com/en-us/sql/t-sql/queries/select-group-by-transact-sql) — W10-C02; primary. HTTP: 200; section xem ledger.

- [Common table expressions](https://learn.microsoft.com/en-us/sql/t-sql/queries/with-common-table-expression-transact-sql) — W10-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-10-quiz.md) · [Đáp án](../../assessments/answer-keys/week-10-answers.md) · [Flashcards](../../flashcards/week-10.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-09.md)

[Tuần tiếp](week-11.md)
