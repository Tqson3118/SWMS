const xs = [
 {id:"a",category:"Food",amountVnd:50000},
 {id:"b",category:"Travel",amountVnd:20000},
 {id:"c",category:"Food",amountVnd:30000}
];
const sum = xs.filter(x=>x.category==="Food").reduce((s,x)=>s+x.amountVnd,0);
function makeBudget(limit) { return total => Math.max(0,total-limit); }
const foodOver = makeBudget(70000);
console.log(sum,foodOver(sum)); // 80000 10000
const changed = xs.map(x=>x.id==="a"?{...x,amountVnd:60000}:x);
console.log(xs[0].amountVnd,changed[0].amountVnd); // 50000 60000