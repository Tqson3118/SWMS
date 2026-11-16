# Tuần 28 — DSA, interviews và job application readiness

Chặng 7; 3 bài L28-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.

## Mục tiêu và tiên quyết

- LO-W28-01: Giải thích Big-O và chọn structures cho bài thực tế.

- LO-W28-02: Kết nối câu hỏi với project evidence và trade-offs.

- LO-W28-03: Tạo evidence đủ để ứng tuyển và có kế hoạch sửa gaps.

Tiên quyết: Đạt gate tuần 27; dùng lại fixture và commit trước.

## Chẩn đoán và phân bổ thời gian

Làm [quiz](../../assessments/quizzes/week-28-quiz.md) trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.

Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.

<a id="lo-w28-01"></a>

## Bài 1: DSA hợp lý

LO-W28-01 · W28-C01

Required scope arrays/strings/hashmap/set,stack/queue,sorting,binarysearch,twopointers/slidingwindow vàBFS cơbản; không đưa advancedDP vào core. Big-O mô tả growthtime/space theo n, không đo seconds benchmark. DictionaryexpectedlookupO(1) phụthuộchash/collisions, worstcasecóthểO(n); sortingO(nlogn) tăngcost nhưng có thể cho binarysearchO(logn). Binarysearchcần sortedinput và loopinvariantlo/hi. Practice16problemschia2–3buổi/tuần từC#phase, không cramtuần28; mỗi problem explainconstraints,bruteforce,improvement,edgecases,complexity,test. C++refresh chỉ khi đọclegacy; interviewcode dùngC# hoặcTS là chính.

<a id="lo-w28-02"></a>

## Bài 2: Practical interview answers

LO-W28-02 · W28-C02

Chuẩn bị answers scope/closure/eventloop/await, C#value-reference/LINQdeferred/cancellation/DI, SQLJOIN/index/transactions, API401403404/ownership/N+1 vàReactstate/effectrace. Mỗianswer90–120seconds: definition, projectexample, failurecase, trade-off. BehavioralSTAR gồm Situation/Task/Action/Result thật; không bịa productionusers hayperformancegain. Systemdesign10–15minutes hỏi requirements,assumptions,API/data,scaling/failures trước code. Debugexercise đọcHTTPtrace/SQLplan/testfailure, nói giả thuyết và kiểm. MockinterviewEnglish2lần, tựnghe và sửa vocabulary/filler, không học thuộc script đếnmức không trả followup.

<a id="lo-w28-03"></a>

## Bài 3: Portfolio và application gate

LO-W28-03 · W28-C03

PortfolioREADME cho reviewer mới: problem,targetuser,stack/version,setup,migrations/seed,test/deployURLs,architecture,securityownership,limitations vàscreenshots. Resume mô tả feature/decisions/evidence thật, không liệt kê librarychưa dùng. Demo5minutes Alice/Bob,monthlybudget,conflict,importidempotency,cacheoutage/exportjob; explainonebugfix. Requiredgate tất cả week behavior/test, noexposedsecrets, ownerisolation/atomicity/version/auth, stagingdeploymentrestore vàmockfeedback. Không bảo đảm joboffer; jobready là có thể tựbuild/debug/explain vàprojectreviewable. Nếucloud/English/DSAgap còn thì ghiaction/date/target và chỉapply khi criticalprojectgatesđạt; electiveVue/NoSQL/C++ khôngchặncore.

## Ví dụ giải từng bước

1. Project-inspired frequency summary không thaySQLreport:
```csharp
static Dictionary<string,long> SumByCategory(
 IEnumerable<(string Category,long Amount)> rows) {
 var totals=new Dictionary<string,long>();
 foreach(var (category,amount) in rows) {
  totals.TryGetValue(category,out var old);
  totals[category]=checked(old+amount);
 }
 return totals;
}
```
2. Food50000/Travel20000/Food30000→Food80000/Travel20000; empty→emptydictionary. O(n)expectedtime/O(k)space, kcategories.
3. Compare nestedscanO(nk) và SQLGROUPBY cho serverlargefilter; giải thích không tải toàn DB vàoC# chỉđểDSA.
4. Demo jobready: showcommit/testCI/deploy/restoreevidence, trả2followups vềcacheconsistency và ownership. Chưa có evidence thì ghi unverified, khôngtick.

## Đào sâu và lỗi thường gặp

Đừng LeetCode đếnmức bỏ project hoặc chỉ copy known solution. DSA cần reasoning vàedgecases. Jobads thayđổi: learnerchọn5ads hiện hành tự map must-have/optional vớiproject; course không invent job market claim.

## Bài tập thực hành

### Bài tập 1

Giải TwoSum với dictionary và 3 edge cases; nóicomplexity.

### Bài tập 2

Trả lời 'Why not use microservices here?' với evidence.

## Hướng dẫn giải bài tập

Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.

### Hướng giải 1

Scan nums, tìm target-x trong map trước insert current để không dùng cùng index. Testduplicate[3,3]target6,negative,nomatch. ExpectedO(n),O(n)space; clarifyreturncontract.

### Hướng giải 2

Modular monolith đủ requirements/scale; ítdeployment/distributedtransactioncost; boundaries service/domain rõ. Nếu independentlyscaling/teamownership/bottleneck có evidence mới cân nhắc extraction.

## Project task

Hoàn tất repo/demo/resume evidence,2mockinterviews vàapplicationtracker. Featurecuối: publishprojectcase-study cóADR/cache/queuefailures vàbefore-afterbugfix thật.

Theo [contract chung](../../project-spec.md). Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w28.md của repo học viên. Không lưu secret.

## Acceptance và rubric

- [ ] 16DSAproblems có independentattempt/edgecases/complexity, làm2bài mới 45 phút không copy.

- [ ] 2mockEnglishinterviews có feedback vàrecording; giải thích project 5 phút và 3 followups.

- [ ] Finalprojectcriticalgates có evidence;READMEfreshclone/CI/deploy/restorepass;resume không claim bịa.

- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.

- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.

| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |
|---|---|---|---|---|
| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |
| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |
| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |

Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.

## English và ôn tập

Phỏng vấn tổng hợp 30 phút: introduction,project,debug,design,behavioral. Ghi 3 lỗi English và luyện lại sau 24h.

[Nhịp 30–60 phút/ngày](../../english-practice.md)

## Nguồn và kiểm chứng

- [Dictionary lookup complexity and comparer](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-10.0) — W28-C01; primary. HTTP: 200; section xem ledger.

- [Demonstrate reasoning and STAR(R) examples](https://careers.microsoft.com/v2/global/en/hiring-tips/interview-tips.html) — W28-C02; primary. HTTP: 200; section xem ledger.

- [Repository README purpose and links](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes) — W28-C03; primary. HTTP: 200; section xem ledger.

- [Sorted input and binary search](https://learn.microsoft.com/en-us/dotnet/api/system.array.binarysearch?view=net-10.0) — W28-C03; primary. HTTP: 200; section xem ledger.

## Liên kết ôn tập và tuần liên quan

[Quiz](../../assessments/quizzes/week-28-quiz.md) · [Đáp án](../../assessments/answer-keys/week-28-answers.md) · [Flashcards](../../flashcards/week-28.yaml) · [Ledger](../../quality/source-ledger.md)

[Tuần trước](week-27.md)
