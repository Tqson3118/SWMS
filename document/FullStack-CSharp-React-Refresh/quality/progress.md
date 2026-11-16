# Nghiệm thu học liệu — 2026-10-03

**Đã hoàn tất biên soạn và kiểm tra gói học liệu 7 chặng /28 tuần /84 bài nhỏ.** Đây là khóa tự học và bài tập của một project; không đánh dấu learner project hoặc năng lực học viên đã hoàn thành.

## Kết quả

| Gate của học liệu | Kết quả | Evidence |
|---|---|---|
| Plan/scaffold/ref conventions |Đã đọc4 khóa tham chiếu,tạo plan/todo/scaffold trước full content |tasks/plan.md |
| Coverage/weekly structure |28 tuần,3 bài/tuần;core/deep/example/practice+solutions/project/quiz/answers/cards/English/gates |validation-final.json |
| Mapping/integrity |168 questions/168 cards,84LO;unique IDs,hash/byte counts,answer4options,concept/claim links đúng |validation-final.json |
| Internal links/placeholders/duplicates |714 links được kiểm;0 missing links,0 placeholders,0 byte-identical accidental artifacts |validation-final.json |
| Sources |89/89 URL HTTP200;primary/reliable;82 nguồn chủ đề tuần được đối chiếu selected sections |source-check.json,source-review-notes.md |
| Import format/parser |28/28 unit staticvalidate+offline dryrun+preview;full visiblelesson,prompts/options/keys/feedback/concepts/anchors/cards/citations đúng |import-validation-final.json |
| Runnable examples |JS W02/W04;C# W05–08;API W13;rate limit W25 pass |example-check-phase-1/2/4/7.json |
| TypeScript/React examples |W17 strict compile;W19 amount boundary;W20 budget arithmetic/zero/missing pass |example-check-phase-5.json |
| Packaging |ZIP giữ relative structure và28 manifest;cache/runtime/previews excluded |package-summary.json |

## Trạng thái từng chặng

1. W01–04: biên soạn và validation/import pass;JS outputs/fetch404/eventloop đã chạy.
2. W05–08: biên soạn và validation/import pass;bốn console examples đã compile/run .NET10.
3. W09–12: biên soạn và validation/import pass;SQL exercises có expected fixture vàsolutions.
4. W13–16: biên soạn và validation/import pass;ping200/error500generic đã chạy.
5. W17–20: biên soạn và validation/import pass;React strict compile,amount/budget tests đã chạy.
6. W21–24: biên soạn và validation/import pass;runbooks/gates cho Docker/CI/cloud/restore.
7. W25–28: biên soạn và validation/import pass;limiter200/200/200/429 đã chạy.

Final validation supersedes earlier phase snapshots after parser/editorial corrections. Source_status vẫn in_review và app Course Draft;không tự publish/approve một course trong ứng dụng đích.

## Giới hạn thực tế và việc học viên còn làm

- Đây là nghiệm thu học liệu và format import offline. Chưa upload/import vào database thật hoặc publish khóa trong app.
- Học viên còn thực hiện project,two-user SQL/auth integration,browser accessibility,Docker/CI/cloud,backuprestore vàmock interviews theo acceptance. Không claim những hành vi này đã pass chỉ từ file học liệu.
- Source review là selected technical sections/API behavior,không certification đọc toàn bộ mỗi trang. Project limits/fixture/TTL/RPO/RTO/DSA quota là quyết định author,không external guarantees.
- App importer hiện tạo28 weeklyfolders cho non-psychology;7 chặng vẫn là logical curriculum/notes folders. Markdownsource và renderedHTML đều được giữ.
- User yêu cầu gpt-5.6-luna subagents;LocalWorker policy enabled=false/maxConcurrent=0 nên biên soạn tuần tự. Không nhận đã dùng subagent model này.
- Chỉ khóa mới này được ghi. SubjectTemplate,DEV,DotNet-React-Roadmap,PersonalFinance và Visualization source chỉ đọc;importer DLL dùng offline.

## Bắt đầu

Đọc README.md,setup.md,project-spec.md vàstudy-method.md. Làm chẩn đoánW01;English30–60 phút/ngày. Gate học viên là mọibehavior criteria vàquiz>=5/6,có evidence thật;không phải lịch hayfile count.
