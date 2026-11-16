from pathlib import Path
import json,re,subprocess,sys,html
from html.parser import HTMLParser
R=Path(__file__).resolve().parents[1]
DLL=Path(r"D:\03_Study\Visualization\backend\src\DsaVisual.Api\bin\Debug\net10.0\DsaVisual.Api.dll")
phase=int(sys.argv[1]) if len(sys.argv)>1 else None
weeks=[]
for p in sorted((R/'data').glob('phase-*.json')):weeks+=json.loads(p.read_text(encoding='utf-8'))
weeks=[w for w in weeks if phase is None or (w['week']-1)//4+1==phase]
errors=[];rows=[]
class Text(HTMLParser):
 def __init__(self):super().__init__(convert_charrefs=True);self.parts=[]
 def handle_data(self,s):self.parts.append(s)
def visible(s):
 x=Text();x.feed(s);return re.sub(r'\s+',' ',''.join(x.parts)).strip()
for w in weeks:
 n=w['week'];nn=f'{n:02}';m=(n-1)//4+1
 mf=R/f'manifest-w{nn}.yaml';out=R/f'quality/preview-w{nn}.json'
 calls=[]
 for args in [
  ['--import-validate','--manifest',str(mf)],
  ['--import-dry-run','--manifest',str(mf)],
  ['--export-preview','--manifest',str(mf),'--output',str(out)]]:
  run=subprocess.run(['dotnet',str(DLL),*args],cwd=R,text=True,encoding='utf-8',errors='replace',capture_output=True,timeout=15)
  calls.append({'mode':args[0],'exit_code':run.returncode,'stdout':run.stdout,'stderr':run.stderr})
  if run.returncode:errors.append(f'W{nn}: {args[0]} failed')
 if not out.exists():continue
 p=json.loads(out.read_text(encoding='utf-8-sig'))
 def check(ok,label):
  if not ok:errors.append(f'W{nn}: '+label)
 check(p['unit_id']==f'roadmap:unit:fscr:w{nn}','unit ID')
 check(len(p['quiz']['questions'])==6,'question count')
 check(p['quiz']['pass_score']==5,'quiz pass threshold')
 check(len(p['flashcard_deck']['cards'])==6,'card count')
 original=(R/f'notes/module-{m}/week-{nn}.html').read_text(encoding='utf-8')
 check(visible(original)==visible(p['lesson']['content_markdown']),'lesson lost text in sanitizer')
 check(all(s['url'] in ';'.join(p['lesson']['source_citations']) for s in w['sources'][:3]),'source citations from wrong week')
 concepts={c['id'] for c in p['lesson']['concepts']}
 for i,(q,actual) in enumerate(zip(w['questions'],p['quiz']['questions'])):
  rot=(n+i)%4;options=q['options'][-rot:]+q['options'][:-rot] if rot else q['options'];reasons=q['reasons'][-rot:]+q['reasons'][:-rot] if rot else q['reasons']
  check(actual['prompt']==q['stem'],f'Q{i+1} prompt')
  check([o['text'] for o in actual['options']]==options,f'Q{i+1} choices')
  check(actual['correct_option_ids']==['ABCD'[rot]],f'Q{i+1} answer')
  check(actual['correct_explanation']==reasons[rot],f'Q{i+1} correct explanation')
  check(actual['wrong_explanations']=={a:reasons[j] for j,a in enumerate('ABCD') if j!=rot},f'Q{i+1} wrong explanations')
  check(actual['concept_id'] in concepts,f'Q{i+1} concept')
  check(actual.get('lesson_anchor')==f'#lo-w{nn}-{i//2+1:02}',f'Q{i+1} anchor')
  card=p['flashcard_deck']['cards'][i]
  check(card['front']==q['front'] and card['back']==q['back'],f'card{i+1} content')
  check(card['claim_ids']==[f'W{nn}-C{i//2+1:02}'],f'card{i+1} claim')
 rows.append({'week':n,'calls':calls,'preview_text_chars':len(p['lesson']['content_markdown'])})
report={'phase':phase,'weeks':len(weeks),'passed':not errors,'errors':errors,'results':rows,'scope':'existing importer DLL: static validate, offline dry-run, exported full lesson text, all question/options/answers/feedback/concepts/anchors/cards and weekly citations; no database import/upload'}
name=f'import-validation-phase-{phase}.json' if phase else 'import-validation-final.json'
(R/'quality'/name).write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'phase':phase,'weeks':len(weeks),'passed':not errors,'errors':errors},ensure_ascii=True))
sys.exit(1 if errors else 0)
