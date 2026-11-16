import {createServer} from 'node:http';
import assert from 'node:assert/strict';
const server=createServer((req,res)=>{
 if(req.url==='/fixture.json'){res.setHeader('Content-Type','application/json');res.end('[1,2,3]');}
 else {res.statusCode=404;res.end('Missing');}
});
await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
const fixtureUrl='http://127.0.0.1:'+server.address().port+'/fixture.json';
const seen=[];const originalLog=console.log;
console.log=(...args)=>{seen.push(args.join(' '));originalLog(...args);};
async function loadExpenses(url,signal){
 const r=await fetch(url,{signal});
 if(!r.ok) throw new Error("HTTP "+r.status);
 const data=await r.json();
 if(!Array.isArray(data)) throw new Error("Sai shape");
 return data;
}
const controller=new AbortController();
loadExpenses(fixtureUrl,controller.signal)
 .then(xs=>console.log("count",xs.length))
 .catch(e=>console.log(e.name==="AbortError"?"Đã hủy":e.message));
console.log("A");
setTimeout(()=>console.log("T"),0);
Promise.resolve().then(()=>console.log("P"));
console.log("B");
await new Promise(resolve=>setTimeout(resolve,200));
assert.deepEqual(seen.filter(x=>['A','B','P','T'].includes(x)),['A','B','P','T']);
assert(seen.includes('count 3'));
await assert.rejects(loadExpenses(fixtureUrl+'-missing',controller.signal),/HTTP 404/);
await new Promise(resolve=>server.close(resolve));
console.log('HTTP404 rejected; microtask order passed');
