# Tuần 11 — Index, execution plan, pagination và N+1

Chặng 3; 3 bài L11-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W11-01: Thiết kế index cho owner/date và đo reads/write tradeoff.

- LO-W11-02: Đọc actual plan, row estimates, lookup/sort và logical reads.

- LO-W11-03: Viết stable paging và nhận diện N+1 ở SQL/EF contract.

Tiên quyết: Đạt gate tuần 10; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-11-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w11-01"></a>

## Bài 1: Index design

LO-W11-01 · W11-C01

B-tree index giúp tìm/range/order bằng key phù hợp; composite key order quan trọng. Query WHERE UserId=@u AND SpentOn>=@from AND SpentOn<@to ORDER BY SpentOn DESC,Id DESC có candidate(UserId,SpentOn DESC,Id DESC) INCLUDE(AmountVnd,CategoryId). Included columns giúp cover projection, không tham gia ordering key. Index tăng storage và write maintenance; không tạo mọi column index. SARGable date range so column trực tiếp tốt hơn YEAR(SpentOn)=... làm khó seek. Scan có thể đúng cho small table/manyrows, không gọi mọi scan là lỗi. Statistics/cardinality estimate ảnh hưởng lựa chọn plan.

<a id="lo-w11-02"></a>

## Bài 2: Basic execution plan và đo lường

LO-W11-02 · W11-C02

SQL Server SSMS bật Include Actual Execution Plan và SET STATISTICS IO,TIME ON. Actual rows so estimated rows để thấy estimation lệch; Index Seek vẫn có thể đọc nhiều rows/lookup, không chỉ nhìn icon. Key Lookup nhiều lần/sort spill có thể đáng chú ý; cost% là estimate relative, không latency thực. Dùng dataset100000 expense giả, query shape/parameters cố định, warm/cold state ghi rõ, chạy nhiều lượt báo median/p95 và logical reads. So before/after cùng data, không hứa50ms mọi máy. Kiểm functional output unchanged trước nhận optimization.

<a id="lo-w11-03"></a>

## Bài 3: Pagination và query round trips

LO-W11-03 · W11-C03

OFFSET/FETCH hỗ trợ page number nhưng page sâu skip nhiều rows; luôn ORDER BY ổn định có unique tie-breaker Id. Concurrent inserts có thể làm offset lặp/missing giữa các request; keyset cursor dùng (SpentOn,Id) after tuple theo sort để next page nhất quán hơn, nhưng không tự snapshot mọi data changes. N+1 là 1 query list rồi 1 query per row/category, tăng round trips; project DTO join/projection hoặc eager load có chủ đích, không Include mọi graph. Filter/paging trước materialize, giới hạn pageSize<=100. TotalCount thêm query là2 không phải N+1; cần đếm cùng filter.

## Ví dụ giải từng bước

1. Bài toán: xem trang đầu owner/date, không lấy toàn DB.
2. Parameter và stable ordering.
3. Trên schema project đã seed:
```sql
DECLARE @UserId uniqueidentifier='11111111-1111-1111-1111-111111111111';
DECLARE @From date='20261001',@To date='20261101';
SELECT Id,CategoryId,AmountVnd,SpentOn
FROM dbo.Expenses
WHERE UserId=@UserId AND SpentOn>=@From AND SpentOn<@To
ORDER BY SpentOn DESC,Id DESC
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY;
-- Tạo sau khi đã lưu plan/IO trước:
CREATE INDEX IX_Expenses_User_Date_Id
ON dbo.Expenses(UserId,SpentOn DESC,Id DESC) INCLUDE(AmountVnd,CategoryId);
```
4. Seed Alice với Guid này; expected3 rows descending dates. Index tạo1lần trong migration/script có guard về sau; don't blindly rerun CREATE INDEX. Data3rows chưa chứng minh hiệu năng, cần 100000 rows fixture.

## Đào sâu và lỗi thường gặp

NoSQL cũng cần indexes, không thoát query design. N+1 còn xảy ra trong API loop tự fetch child; đo count queries/requests, không chỉ SQL string. Tối ưu có bằng chứng không thay ownership/date filters.

## Bài tập thực hành

### Bài tập 1

3 expense cùng spentOn; phân trang1item/trang, test ties.

### Bài tập 2

100 expenses mỗi row gọi category query; sửa và đo.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

ORDER BY SpentOn DESC,Id DESC, unique Id phá ties. Ghi IDs mỗi trang trước mutation; không lặp/missing trên fixed dataset. Với concurrent inserts thử keyset và mô tả snapshot hạn chế.

### Hướng giải 2

JOIN/projection DTO lấy categoryName cùng expenses; query count1(list)+1(count) thay101. Nếu endpoint cần nhiều graph test split-query số roundtrips cố định, tránh Include bừa.

## Project task

Endpoint query contract và SQL scripts có owner/date filters, stable page và baseline plan100000rows. Chuẩn bị EF projection tuần14.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w11.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] Fixed dataset/tied dates phân trang không duplicate/missing; pageSize0/101 bị chặn.

- [ ] N+1 case100rows từ 101 queries xuống<=2 cho list+count; output đúng.

- [ ] Lưu before/after actual plan/logical reads/dataset/version; giải thích write cost index.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Thu: 'Can an index seek still be slow?' Nói workload, logical reads, round trip, keyset, tie-breaker.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Index design guide](https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-index-design-guide) — W11-C01; primary. HTTP: 200; section xem ledger.

- [Display an actual execution plan](https://learn.microsoft.com/en-us/sql/relational-databases/performance/display-an-actual-execution-plan) — W11-C02; primary. HTTP: 200; section xem ledger.

- [Pagination, projection and lazy loading N+1](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying) — W11-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-11-quiz.md) · [Đáp án](../../assessments/answer-keys/week-11-answers.md) · [Flashcards](../../flashcards/week-11.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-10.md)

[Tuần tiếp](week-12.md)
