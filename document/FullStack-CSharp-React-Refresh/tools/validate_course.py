from pathlib import Path
import json,re,sys,hashlib,yaml,collections
R=Path(__file__).resolve().parents[1];phase=int(sys.argv[1]) if len(sys.argv)>1 else None
data=[]
for p in sorted((R/'data').glob('phase-*.json')):data+=json.loads(p.read_text(encoding='utf-8'))
selected=[w for w in data if phase is None or (w['week']-1)//4+1==phase];errors=[];stats=collections.Counter()
def check(test,msg):
 if not test:errors.append(msg)
ids=set();hashes=collections.defaultdict(list)
for w in selected:
 n=w['week'];nn=f'{n:02}';m=(n-1)//4+1
 mf=yaml.safe_load((R/f'manifest-w{nn}.yaml').read_text(encoding='utf-8'))
 check(mf['schema_version']=='2.0.0',f'W{nn}: schema')
 check(len(w['lessons'])==3 and len(w['questions'])==6,f'W{nn}: lessons/questions count')
 check(len(w['criteria'])>=3 and len(w['exercises'])>=2,f'W{nn}: gates/exercises')
 for l in w['lessons']:check(len(l['body'])>=450,f'W{nn}: thin lesson {l["title"]}')
 for q in w['questions']:
  check(len(q['options'])==len(q['reasons'])==4,f'W{nn}: distractor count')
  check(len(set(q['options']))==4,f'W{nn}: duplicate choices')
  check(all(len(r)>=15 for r in q['reasons']),f'W{nn}: explanations')
 for kind,key in [('lo','learning_objectives'),('concept','concepts'),('question','questions')]:
  for v in mf[key]:check(v['id'] not in ids,'duplicate '+v['id']);ids.add(v['id'])
 lo_ids={v['id'] for v in mf['learning_objectives']};cons={v['id']:v for v in mf['concepts']}
 for q in mf['questions']:
  check(q['lo_ref'] in lo_ids and q['concept_id'] in cons,f'W{nn}: LO/concept mapping')
  check(q['claim_ids']==cons[q['concept_id']]['claim_ids'],f'W{nn}: claim mapping')
 for a in mf['units'][0]['artifacts']:
  p=R/a['path'];check(p.is_file(),f'missing {p}')
  if not p.is_file():continue
  b=p.read_bytes();h=hashlib.sha256(b).hexdigest()
  check(h==a['sha256'] and len(b)==a['byte_length'],f'W{nn}: integrity {a["path"]}')
  if a['role']!='source_ledger':hashes[h].append(a['path'])
  s=b.decode('utf-8')
  check(not re.search(r'\b(?:TODO|TBD|FIXME|lorem ipsum)\b|\[Tên bài\]|\[NN\]',s,re.I),f'W{nn}: placeholder {a["path"]}')
  if a['role']=='lesson_theory':
   for h2 in ['Mục tiêu và tiên quyết','Chẩn đoán','Ví dụ giải','Đào sâu','Bài tập thực hành','Hướng dẫn giải','Project task','Acceptance','English','Nguồn','Liên kết']:check(h2 in s,f'W{nn}: missing section {h2}')
   for q in mf['questions']:check(q['lo_ref'] in s and q['claim_ids'][0] in s,f'W{nn}: lesson mapping')
   stats['lessons']+=3
  if a['role']=='quiz_questions':
   qids=re.findall(r'^### (Q\d{2}-\d{2}) \[',s,re.M);check(qids==[q['id'] for q in mf['questions']],f'W{nn}: quiz IDs')
   for letter in 'ABCD':check(len(re.findall('^'+letter+r'\. ',s,re.M))==6,f'W{nn}: option count '+letter)
   stats['questions']+=len(qids)
  if a['role']=='quiz_answers':
   check(len(re.findall(r'^\| Q\d{2}-\d{2} \|',s,re.M))==6,f'W{nn}: answer matrix')
   check(len(re.findall(r'\*\*Phương án [A-D] ĐÚNG',s))==6 and len(re.findall(r'\*\*[A-D] SAI',s))==18,f'W{nn}: explanations')
  if a['role']=='flashcard_deck':
   d=yaml.safe_load(s);check(len(d['cards'])==6,f'W{nn}: cards count')
   for card in d['cards']:
    check(all(k in card for k in ['id','concept','front','back','source_lesson','version']),f'W{nn}: card schema')
    check(card['id'] not in ids,'duplicate card '+card['id']);ids.add(card['id'])
    check((R/card['source_lesson']).is_file(),f'W{nn}: card lesson missing')
    check(10<=len(card['front'])<=500 and 10<=len(card['back'])<=2000,f'W{nn}: card lengths')
   stats['cards']+=6
 for s in w['sources']:check(s['url'].startswith('https://') and len(s['section'])>5,f'W{nn}: source metadata')
 stats['weeks']+=1
for h,p in hashes.items():check(len(p)==1,'duplicate content '+str(p))
for p in list(R.rglob('*.md')):
 s=p.read_text(encoding='utf-8');s=re.sub(chr(96)*3+r'.*?'+chr(96)*3,'',s,flags=re.S)
 for raw in re.findall(r'\[[^\]]*\]\(([^)]+)\)',s):
  if raw.startswith(('http://','https://','#','mailto:')):continue
  target=raw.split('#')[0];fp=(p.parent/target).resolve()
  if not fp.is_file():
   if phase is not None and re.search(r'week-\d{2}\.md$',target):
    future=int(re.search(r'week-(\d{2})',target).group(1))
    if future>max(w['week'] for w in data):continue
   if phase is not None and target.endswith('import-guide.md'):continue
   errors.append('broken link '+str(p.relative_to(R))+' -> '+target)
  stats['links_checked']+=1
report={'phase':phase,'stats':dict(stats),'errors':errors,'passed':not errors,'scope':'structure, links, mapping, hashes/counts, placeholders, byte duplicates; not source semantic/project runtime/website import'}
out=f'quality/validation-phase-{phase}.json' if phase else 'quality/validation-final.json'
(R/out).write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=True));sys.exit(1 if errors else 0)

