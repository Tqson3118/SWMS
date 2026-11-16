from pathlib import Path
import subprocess,re,json,sys,socket,time,urllib.request,urllib.error,os
R=Path(__file__).resolve().parents[1]
phase=int(sys.argv[1])
weeks=[]
for p in sorted((R/'data').glob('phase-*.json')):weeks+=json.loads(p.read_text(encoding='utf-8'))
by={w['week']:w for w in weeks};results=[];errors=[]
def command(argv,cwd,timeout=25):
 r=subprocess.run(argv,cwd=cwd,capture_output=True,text=True,encoding='utf-8',errors='replace',timeout=timeout)
 if r.returncode:raise RuntimeError(r.stdout[-2500:]+r.stderr[-1500:])
 return r.stdout
def api_get(url):
 try:
  with urllib.request.urlopen(url,timeout=3) as r:return r.status,r.read().decode('utf-8')
 except urllib.error.HTTPError as e:return e.code,e.read().decode('utf-8')
if phase in [2,4,7]:
 ns=[5,6,7,8] if phase==2 else [13] if phase==4 else [25]
 for n in ns:
  folder=R/f'quality/runtime/w{n:02}';folder.mkdir(parents=True,exist_ok=True)
  code=re.findall(r'```csharp\n(.*?)\n```',by[n]['example'],re.S)[0]
  web=n in [13,25];sdk='Microsoft.NET.Sdk.Web' if web else 'Microsoft.NET.Sdk'
  (folder/'Lab.csproj').write_text(f'<Project Sdk="{sdk}"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>',encoding='utf-8')
  (folder/'Program.cs').write_text(code,encoding='utf-8')
  try:
   build=command(['dotnet','build','Lab.csproj','-c','Release','--nologo'],folder)
   if not web:
    out=command(['dotnet','run','--project','Lab.csproj','-c','Release','--no-build'],folder)
    wanted={5:['2 1 2'],6:['Food','Travel','50000,30000','2'],7:['80000','50000'],8:['Đã hủy']}[n]
    assert all(x in out for x in wanted),(wanted,out)
    results.append({'week':n,'build':'passed','output':out,'expected':wanted})
   else:
    with socket.socket() as s:s.bind(('127.0.0.1',0));port=s.getsockname()[1]
    env=os.environ.copy();env['ASPNETCORE_ENVIRONMENT']='Production'
    with (folder/'server.log').open('w',encoding='utf-8') as log:
     proc=subprocess.Popen(['dotnet','run','--project','Lab.csproj','-c','Release','--no-build','--','--urls',f'http://127.0.0.1:{port}'],cwd=folder,env=env,stdout=log,stderr=log)
     try:
      ready=False
      for _ in range(100):
       try:
        with socket.create_connection(('127.0.0.1',port),timeout=.1):pass
        ready=True;break
       except OSError:
        if proc.poll() is not None:break
        time.sleep(.05)
      assert ready,'API did not start'
      url=f'http://127.0.0.1:{port}/api/'
      if n==13:
       ping=api_get(url+'ping');fault=api_get(url+'fault')
       assert ping[0]==200 and json.loads(ping[1])['message']=='pong'
       assert fault[0]==500 and 'Lab fault' not in fault[1] and 'InvalidOperationException' not in fault[1]
       results.append({'week':n,'build':'passed','statuses':[ping[0],fault[0]],'production_stack_exposed':False})
      else:
       statuses=[api_get(url+'ping')[0] for _ in range(4)]
       assert statuses==[200,200,200,429],statuses
       results.append({'week':n,'build':'passed','statuses':statuses})
     finally:
      proc.terminate()
      try:proc.wait(timeout=5)
      except subprocess.TimeoutExpired:proc.kill();proc.wait()
  except Exception as e:errors.append(f'W{n:02}: {e}')
elif phase==1:
 folder=R/'quality/runtime/javascript';folder.mkdir(parents=True,exist_ok=True)
 code=re.findall(r'```javascript\n(.*?)\n```',by[2]['example'],re.S)[0]
 (folder/'week02.mjs').write_text(code,encoding='utf-8')
 try:
  out=command(['node','week02.mjs'],folder)
  assert '80000 10000' in out and '50000 60000' in out,out
  results.append({'week':2,'output':out,'expected':['80000 10000','50000 60000']})
 except Exception as e:errors.append(str(e))
 code=re.findall(r'```javascript\n(.*?)\n```',by[4]['example'],re.S)[0]
 prefix="""import {createServer} from 'node:http';
import assert from 'node:assert/strict';
const server=createServer((req,res)=>{
 if(req.url==='/fixture.json'){res.setHeader('Content-Type','application/json');res.end('[1,2,3]');}
 else {res.statusCode=404;res.end('Missing');}
});
await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
const fixtureUrl='http://127.0.0.1:'+server.address().port+'/fixture.json';
const seen=[];const originalLog=console.log;
console.log=(...args)=>{seen.push(args.join(' '));originalLog(...args);};
"""
 code=code.replace('loadExpenses("./fixture.json",controller.signal)','loadExpenses(fixtureUrl,controller.signal)')
 suffix="""
await new Promise(resolve=>setTimeout(resolve,200));
assert.deepEqual(seen.filter(x=>['A','B','P','T'].includes(x)),['A','B','P','T']);
assert(seen.includes('count 3'));
await assert.rejects(loadExpenses(fixtureUrl+'-missing',controller.signal),/HTTP 404/);
await new Promise(resolve=>server.close(resolve));
console.log('HTTP404 rejected; microtask order passed');
"""
 (folder/'week04.mjs').write_text(prefix+code+suffix,encoding='utf-8')
 try:
  out=command(['node','week04.mjs'],folder)
  results.append({'week':4,'output':out,'expected':'A,B,P,T; count3; HTTP404 reject'})
 except Exception as e:errors.append(str(e))
report={'phase':phase,'passed':not errors,'errors':errors,'results':results,'scope':'selected runnable worked examples; not complete learner project, SQL/provider/React/Docker/cloud tests'}
(R/f'quality/example-check-phase-{phase}.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=True))
sys.exit(1 if errors else 0)
