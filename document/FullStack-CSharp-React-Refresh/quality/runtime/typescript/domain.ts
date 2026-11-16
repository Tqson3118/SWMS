function parseAmount(text:string):number {
 const s=text.trim();
 if(!/^\d+$/.test(s)) throw new Error('Enter whole VND');
 const n=Number(s);
 if(!Number.isSafeInteger(n)||n<1||n>1_000_000_000)
  throw new Error('Amount out of range');
 return n;
}
export function budgetStatus(spent:number,limit:number|null) {
 if(limit===null) return {label:'No budget',excess:0,percent:null};
 const excess=Math.max(0,spent-limit);
 return {label:excess?'Over budget':'Within budget',excess,
 percent:limit===0?null:excess/limit*100};
}

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
