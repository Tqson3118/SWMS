from pathlib import Path
import json,re,subprocess,sys
R=Path(__file__).resolve().parents[1];P=R/'quality/runtime/typescript';P.mkdir(parents=True,exist_ok=True)
TSC=P/'node_modules/typescript/bin/tsc'
web=P/'node_modules'
ws=json.loads((R/'data/phase-5.json').read_text(encoding='utf-8'))
app=re.findall(r'```tsx\n(.*?)\n```',ws[0]['example'],re.S)[0]
(P/'App.tsx').write_text(app,encoding='utf-8')
parse=re.findall(r'```ts\n(.*?)\n```',ws[2]['example'],re.S)[0]
budget=re.findall(r'```ts\n(.*?)\n```',ws[3]['example'],re.S)[0]
tests="""
function expect(ok:boolean,label:string){if(!ok)throw new Error(label);}
expect(parseAmount('50000')===50000,'amount');
for(const x of ['', '1.5','-1','9007199254740993']){
 let rejected=false;try{parseAmount(x);}catch{rejected=true;}
 expect(rejected,'reject '+x);
}
expect(budgetStatus(80000,70000).excess===10000,'excess');
expect(Math.abs(budgetStatus(80000,70000).percent!-14.285714285714285)<0.0001,'percentage');
expect(budgetStatus(20000,0).percent===null,'zero budget');
expect(budgetStatus(20000,null).label==='No budget','missing budget');
console.log('Amount boundary and budget tests passed');
"""
(P/'domain.ts').write_text(parse+'\n'+budget+'\n'+tests,encoding='utf-8')
config={'compilerOptions':{'target':'ES2022','module':'NodeNext','moduleResolution':'NodeNext','strict':True,'jsx':'react-jsx','lib':['ES2022','DOM'],'skipLibCheck':True,'outDir':'out','paths':{'react':[str(web/'@types/react/index.d.ts')],'react/jsx-runtime':[str(web/'@types/react/jsx-runtime.d.ts')]}},'include':['App.tsx','domain.ts']}
(P/'tsconfig.json').write_text(json.dumps(config,indent=2),encoding='utf-8')
results=[];errors=[]
for cmd in [['node',str(TSC),'--project',str(P/'tsconfig.json')],['node',str(P/'out/domain.js')]]:
 run=subprocess.run(cmd,cwd=P,text=True,encoding='utf-8',errors='replace',capture_output=True,timeout=15)
 results.append({'command':cmd,'exit_code':run.returncode,'stdout':run.stdout,'stderr':run.stderr})
 if run.returncode:errors.append(run.stdout+run.stderr);break
report={'passed':not errors,'errors':errors,'results':results,'scope':'W17 React example strict compile, W19 amount parser boundary tests, W20 budget arithmetic/zero/missing tests; not full frontend integration/browser accessibility/auth tests'}
(R/'quality/example-check-phase-5.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=True));sys.exit(1 if errors else 0)
