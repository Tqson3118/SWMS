from pathlib import Path
import json,re,hashlib,urllib.request,concurrent.futures,datetime,sys
R=Path(__file__).resolve().parents[1]
weeks=[]
for p in (R/'data').glob('phase-*.json'):weeks+=json.loads(p.read_text(encoding='utf-8'))
urls=sorted({s['url'] for w in weeks for s in w['sources']})
for p in [R/'setup.md',R/'english-practice.md',R/'appendices/refresh-only.md']:
 urls+=re.findall(r'https://[^\s;)]+',p.read_text(encoding='utf-8'))
urls=sorted(set(urls));cache=R/'quality/source-cache';cache.mkdir(exist_ok=True)
old={}
if (R/'quality/source-check.json').exists():old={x['url']:x for x in json.loads((R/'quality/source-check.json').read_text(encoding='utf-8'))}
def fetch(url):
 if url in old and old[url].get('status')==200:return old[url]
 out={'url':url,'checked_at':datetime.datetime.now(datetime.timezone.utc).date().isoformat(),'review_status':'in_review','review':'HTTP/body fetched; semantic review not yet recorded'}
 try:
  req=urllib.request.Request(url,headers={'User-Agent':'CourseSourceVerifier/1.0'})
  with urllib.request.urlopen(req,timeout=12) as r: raw=r.read(3000000);out.update(status=r.status,final_url=r.url)
  from html.parser import HTMLParser
  class Extract(HTMLParser):
   def __init__(self):super().__init__();self.skip=0;self.parts=[]
   def handle_starttag(self,t,a):
    if t in ['script','style']:self.skip+=1
    if t in ['p','h1','h2','h3','li','pre']:self.parts.append('\n')
   def handle_endtag(self,t):
    if t in ['script','style']:self.skip=max(0,self.skip-1)
   def handle_data(self,s):
    if not self.skip:self.parts.append(s)
  x=Extract();x.feed(raw.decode('utf-8',errors='replace'));txt=re.sub(r'[ \t]+',' ',''.join(x.parts));txt=re.sub(r'\n\s*\n+','\n',txt)
  h=hashlib.sha256(url.encode()).hexdigest()[:16];(cache/(h+'.txt')).write_text(txt,encoding='utf-8')
  out.update(cache_path='quality/source-cache/'+h+'.txt',body_sha256=hashlib.sha256(raw).hexdigest(),body_bytes=len(raw),text_chars=len(txt))
 except Exception as e:out.update(status='error',error=str(e))
 return out
with concurrent.futures.ThreadPoolExecutor(max_workers=8) as ex:results=list(ex.map(fetch,urls))
(R/'quality/source-check.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'sources':len(results),'http_ok':sum(x.get('status')==200 for x in results),'failures':[x for x in results if x.get('status')!=200]}))
