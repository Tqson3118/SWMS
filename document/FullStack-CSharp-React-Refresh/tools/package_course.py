from pathlib import Path
import zipfile,json,hashlib
R=Path(__file__).resolve().parents[1]
out=R/'quality/FullStack-CSharp-React-Refresh.zip'
excluded=['quality/source-cache/','quality/runtime/','quality/preview-w']
files=[]
with zipfile.ZipFile(out,'w',zipfile.ZIP_DEFLATED) as z:
 for p in sorted(R.rglob('*')):
  if not p.is_file():continue
  rel=p.relative_to(R).as_posix()
  if p==out or rel=='quality/package-summary.json' or rel.startswith(tuple(excluded)) or any(x in p.parts for x in ['__pycache__','bin','obj']):continue
  z.write(p,rel);files.append(rel)
report={'file_count':len(files),'sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'bytes':out.stat().st_size,'excludes':excluded,'files':files}
(R/'quality/package-summary.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='files'}))
