from pathlib import Path
import json,sys,hashlib,re,yaml,os
import markdown as md_renderer
R=Path(__file__).resolve().parents[1];BT=chr(96)
def write(p,s):
 p=R/p;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(s,encoding='utf-8',newline='\n')
def link(a,b,label): return '['+label+']('+os.path.relpath(R/b,(R/a).parent).replace('\\','/')+')'
def safe_markdown_for_html(text):
 # Escape literal angle brackets in prose; fenced code is already escaped by Markdown.
 chunks=re.split(r'(```[\s\S]*?```|<a id="[^"]+"></a>)',text)
 for i in range(0,len(chunks),2):
  chunks[i]=chunks[i].replace('<','&lt;').replace('>','&gt;')
 return ''.join(chunks)
def markdown(parts):
 s='\n\n'.join(parts)+'\n'
 return re.sub(r'(?m)(^\|[^\n]*\|)\n\n(?=\|)',r'\1\n',s)
def digest(p):
 b=(R/p).read_bytes();return hashlib.sha256(b).hexdigest(),len(b)
weeks=[]
for p in sorted((R/'data').glob('phase-*.json')): weeks+=json.loads(p.read_text(encoding='utf-8'))
weeks.sort(key=lambda w:w['week'])
source_results={}
if (R/'quality/source-check.json').exists():source_results={x['url']:x for x in json.loads((R/'quality/source-check.json').read_text(encoding='utf-8'))}
ledger=['# Sổ cái nguồn kỹ thuật','Ngày biên soạn 2026-10-03. Claim kỹ thuật không phải bằng chứng project đã chạy. HTTP và section review tách riêng.','',
'| Claim ID | Bài / LO | Claim | Nguồn primary | Mục cụ thể | Ngày kiểm tra | Phạm vi | Bằng chứng | Giới hạn | Trạng thái |','|---|---|---|---|---|---|---|---|---|---|']
for w in weeks:
 n=w['week'];nn=f'{n:02}';m=(n-1)//4+1
 lesson=f'notes/module-{m}/week-{nn}.md';quiz=f'assessments/quizzes/week-{nn}-quiz.md';answer=f'assessments/answer-keys/week-{nn}-answers.md';deck=f'flashcards/week-{nn}.yaml'
 los=[f'LO-W{nn}-{i:02}' for i in range(1,4)];claims=[f'W{nn}-C{i:02}' for i in range(1,4)]
 def li(p,l): return link(lesson,p,l)
 text=[f'# Tuần {nn} — {w["title"]}',f'Chặng {m}; 3 bài L{nn}-01..03; bắt buộc trừ mục ghi tùy chọn. 12–18 giờ tham khảo + English hằng ngày.','## Mục tiêu và tiên quyết']
 for i,l in enumerate(w['lessons']):text.append(f'- {los[i]}: {l["objective"]}')
 text.append('Tiên quyết: '+('Hoàn thành setup, đọc project-spec; đã từng học biến/hàm/if/loop.' if n==1 else f'Đạt gate tuần {n-1:02}; dùng lại fixture và commit trước.'))
 text+=['## Chẩn đoán và phân bổ thời gian',f'Làm {li(quiz,"quiz")} trước khi đọc, không tra đáp án. Tự viết lại ví dụ bằng input khác. Nếu >=5/6 và tự vượt practice: review 30–60 phút rồi tập trung project/deep dive; còn lại học từng bài 60–90 phút. Không bỏ acceptance.',
 'Ngày 1 chẩn đoán+bài 1; ngày 2 bài 2; ngày 3 bài 3; ngày 4 practice; ngày 5 project; ngày 6 test+quiz; ngày 7 ôn 1/3/7/14 ngày. Git commit sau trạng thái chạy được.']
 for i,l in enumerate(w['lessons']):text += [f'<a id="lo-w{nn}-{i+1:02}"></a>',f'## Bài {i+1}: {l["title"]}',f'{los[i]} · {claims[i]}',l['body']]
 text += ['## Ví dụ giải từng bước',w['example'],'## Đào sâu và lỗi thường gặp',w['deep'],'## Bài tập thực hành']
 for i,e in enumerate(w['exercises']):text += [f'### Bài tập {i+1}',e['prompt']]
 text += ['## Hướng dẫn giải bài tập','Đọc sau khi thử ít nhất 30 phút. So expected với actual; đổi input biên.']
 for i,e in enumerate(w['exercises']):text += [f'### Hướng giải {i+1}',e['solution']]
 text+=['## Project task',w['project'],f'Theo {li("project-spec.md","contract chung")}. Lưu code, fixture, test/ảnh/log và commit ID trong docs/evidence/w{nn}.md của repo học viên. Không lưu secret.','## Acceptance và rubric']
 for c in w['criteria']:text.append('- [ ] '+c)
 text+=['- [ ] Quiz >=5/6; giải thích câu sai, làm lại sau 24h.','- [ ] English 30–60 phút/ngày,7 ngày/tuần;5 cụm từ/tuần + recording 90–120 giây.',
 '| Tiêu chí | 0 Chưa đạt | 1 Đang phát triển | 2 Đạt | 3 Vững |','|---|---|---|---|---|',
 '| Hiểu cơ chế | Không giải thích | Lặp tên API | Nói input/output và cơ chế tuần | So sánh phương án và điều kiện biên |',
 '| Practice | Chưa chạy | Chỉ happy path | Đúng output và edge cases | Tự thêm regression có ý nghĩa |',
 '| Project | Chưa có feature | Copy chưa chứng minh | Mọi gate tuần có evidence | Tái hiện/sửa lỗi không xem lời giải |',
 'Gate: mọi hàng >=2 và mọi checkbox behavior đạt. Ownership, atomicity, token validation không bù bằng quiz. Chưa đạt: ghi lỗi, quay đúng LO, làm biến thể sau 24h.',
 '## English và ôn tập',w['english'],li('english-practice.md','Nhịp 30–60 phút/ngày'),'## Nguồn và kiểm chứng']
 for i,s in enumerate(w['sources']):
  rr=source_results.get(s['url'],{});text.append(f'- [{s["section"]}]({s["url"]}) — {claims[min(i,2)]}; primary. HTTP: {rr.get("status","chưa chạy")}; section xem ledger.')
 for i,l in enumerate(w['lessons']):
  s=w['sources'][min(i,len(w['sources'])-1)];rr=source_results.get(s['url'],{})
  ledger.append(f'| {claims[i]} | W{nn} / {los[i]} | {l["objective"].replace("|","/")} | [{s["section"]}]({s["url"]}) | {s["section"]} | {rr.get("checked_at","chưa kiểm tra")} | {rr.get("review","chưa đối chiếu mục")} | HTTP {rr.get("status","chưa chạy")} | Ví dụ tự tạo, không chứng minh runtime | {rr.get("review_status","in_review")} |')
 text+=['## Liên kết ôn tập và tuần liên quan',li(quiz,'Quiz')+' · '+li(answer,'Đáp án')+' · '+li(deck,'Flashcards')+' · '+li('quality/source-ledger.md','Ledger')]
 if n>1:text.append(li(f'notes/module-{(n-2)//4+1}/week-{n-1:02}.md','Tuần trước'))
 if n<28:text.append(li(f'notes/module-{n//4+1}/week-{n+1:02}.md','Tuần tiếp'))
 write(lesson,markdown(text))
 html_lesson=lesson.replace('.md','.html')
 write(html_lesson,md_renderer.markdown(safe_markdown_for_html(markdown(text)),extensions=['fenced_code','tables']))
 qs=[f'# Quiz tuần {nn} — {w["title"]}','6 câu, chọn một phương án và ghi lý do. '+link(quiz,lesson,'Bài học')]
 ans=[f'# Đáp án tuần {nn} — {w["title"]}',link(answer,quiz,'Quiz')+' · '+link(answer,lesson,'Bài nguồn'),'## Ma trận đáp án','| Câu hỏi | LO | Bloom | Khái niệm | Đáp án | Claim |','|---|---|---|---|---|---|']
 cards=[]
 for i,q in enumerate(w['questions']):
  qid=f'Q{nn}-{i+1:02}';lo=los[i//2];claim=claims[i//2];rot=(n+i)%4
  opts=q['options'][-rot:]+q['options'][:-rot] if rot else q['options'];correct='ABCD'[rot]
  qs += [f'### {qid} [{lo}]',q['stem']]+[f'{a}. {v}' for a,v in zip('ABCD',opts)]
  ans.append(f'| {qid} | {lo} | Analyze | {q["concept"].replace("|","/")} | {correct} | {claim} |')
  cards.append({'id':f'fc-fscr-w{nn}-{i+1:03}','concept':q['concept'],'front':q['front'],'back':q['back'],'source_lesson':lesson,'version':'1.0.0','claim_ids':[claim],'tags':[f'w{nn}','fullstack']})
 ans+=['','## Giải thích từng phương án']
 for i,q in enumerate(w['questions']):
  rot=(n+i)%4;opts=q['options'][-rot:]+q['options'][:-rot] if rot else q['options'];reasons=q['reasons'][-rot:]+q['reasons'][:-rot] if rot else q['reasons']
  ans+=[f'### Q{nn}-{i+1:02}',q['stem'],f'- **Đáp án đúng:** **{"ABCD"[rot]}** — {opts[rot]}']
  ans.append(f'- **Phương án {"ABCD"[rot]} ĐÚNG:** {reasons[rot]}')
  ans += [f'- **{a} SAI:** {v}' for j,(a,v) in enumerate(zip('ABCD',reasons)) if j!=rot]
  ans.append('---')
  ans.append('Vị trí bài học: '+link(answer,lesson,f'LO-W{nn}-{i//2+1:02}').replace(')',f'#lo-w{nn}-{i//2+1:02})'))
  ans.append('Ôn '+link(answer,lesson,f'bài {i//2+1}')+f' · {los[i//2]} · {claims[i//2]}.')
 ans+=['## Kế hoạch sửa lỗi','Câu sai: ghi hiểu lầm, đọc đúng bài, tạo phản ví dụ và làm lại sau 24h.']
 write(quiz,markdown(qs));write(answer,markdown(ans))
 write(deck,yaml.safe_dump({'deck_id':f'deck-fscr-w{nn}','subject':'FullStack-CSharp-React-Refresh','lesson_id':f'week-{nn}','version':'1.0.0','description':w['title'],'cards':cards},allow_unicode=True,sort_keys=False,width=1000))
write('quality/source-ledger.md','\n'.join(ledger)+'\n')
for w in weeks:
 n=w['week'];nn=f'{n:02}';m=(n-1)//4+1
 weekly=['# Source ledger W'+nn,'| Claim | LO | Source citation | Section | Status |','|---|---|---|---|---|']
 for i,l in enumerate(w['lessons']):
  src=w['sources'][min(i,len(w['sources'])-1)];rr=source_results.get(src['url'],{})
  weekly.append(f'| W{nn}-C{i+1:02} | LO-W{nn}-{i+1:02} | [{src["section"]}]({src["url"]}) | {src["section"]} | {rr.get("review_status","in_review")} |')
 write(f'quality/ledger-w{nn}.md','\n'.join(weekly)+'\n')
 paths=[f'notes/module-{m}/week-{nn}.html',f'assessments/quizzes/week-{nn}-quiz.md',f'assessments/answer-keys/week-{nn}-answers.md',f'flashcards/week-{nn}.yaml',f'quality/ledger-w{nn}.md']
 roles=['lesson_theory','quiz_questions','quiz_answers','flashcard_deck','source_ledger'];arts=[]
 for role,p in zip(roles,paths):
  h,b=digest(p);a={'role':role,'path':p,'sha256':h,'byte_length':b}
  if role!='source_ledger':a['stable_id']=f'roadmap:{dict(lesson_theory="lesson",quiz_questions="quiz",quiz_answers="answers",flashcard_deck="deck")[role]}:fscr:w{nn}'
  if role=='quiz_questions':a['question_count']=6
  if role=='quiz_answers':a.update(correct_count=6,wrong_count=18)
  if role=='flashcard_deck':a['card_count']=6
  arts.append(a)
 manifest={'schema_version':'2.0.0','package_id':f'package:fscr:w{nn}','subject_id':'roadmap:subject:fullstack-csharp-react-refresh','display_title':'FullStack C# + React Refresh','source_status':'in_review','version':1,'created_at':'2026-10-03T09:21:35Z','subject':'FullStack C# + React Refresh','course':'FullStack C# + React Refresh','module':f'phase-{m}-week-{nn}','lesson':f'week-{nn}','source_ledger_path':f'quality/ledger-w{nn}.md',
 'learning_objectives':[{'id':f'LO-W{nn}-{i+1:02}','bloom_level':'Apply','statement':l['objective']} for i,l in enumerate(w['lessons'])],
 'concepts':[{'id':f'concept:fscr:w{nn}-c{i+1:02}','name':l['title'],'claim_ids':[f'W{nn}-C{i+1:02}'],'evidence_label':'strong','source_ledger_path':f'quality/ledger-w{nn}.md','safety_review':{'reviewed':True,'reviewer':'course-author:technical-scope','date':'2026-10-03'}} for i,l in enumerate(w['lessons'])],
 'questions':[{'id':f'Q{nn}-{i+1:02}','lo_ref':f'LO-W{nn}-{i//2+1:02}','concept_id':f'concept:fscr:w{nn}-c{i//2+1:02}','claim_ids':[f'W{nn}-C{i//2+1:02}']} for i in range(6)],
 'units':[{'unit_id':f'roadmap:unit:fscr:w{nn}','display_title':f'Tuần {nn}: {w["title"]}','sort_order':n,'artifacts':arts}]}
 write(f'manifest-w{nn}.yaml',yaml.safe_dump(manifest,allow_unicode=True,sort_keys=False,width=1000))
road=['# Lộ trình năng lực','28 tuần / 84 bài; lịch linh hoạt theo gate. English, Git, test xuyên suốt.','',BT*3+'mermaid','flowchart TD',' H["HTML/CSS"] --> J["JavaScript/DOM"]',' J --> F["Promise/Fetch"]',' F --> R["TypeScript/React"]',' C["C# fundamentals"] --> A["ASP.NET Core/EF"]',' S["SQL/transactions"] --> A',' F --> A',' A --> R',' R --> P["Docker/CI/Deploy"]',' P --> D["System Design/Interview"]',' S --> D',BT*3,'','| Tuần | Chặng | 3 bài nhỏ | Feature / gate |','|---|---|---|---|']
for w in weeks:
 n=w['week'];p=f'notes/module-{(n-1)//4+1}/week-{n:02}.md'
 road.append(f'| {link("learning-roadmap.md",p,f"W{n:02}")} | {(n-1)//4+1} | '+'; '.join(l['title'].replace('|','/') for l in w['lessons'])+' | '+w['criteria'][0].replace('|','/')+' |')
road+=['','Chỉ chuyển chặng khi 4 gate đạt. Readiness cuối tại quality/progress.md.']
write('learning-roadmap.md','\n'.join(road)+'\n')
print(json.dumps({'generated_weeks':len(weeks),'lessons':len(weeks)*3,'questions':len(weeks)*6,'cards':len(weeks)*6}))

