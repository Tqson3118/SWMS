import { useState } from 'react';
type Row = { id: string; amountVnd: number };
const initial: Row[] = [
 {id:'e1',amountVnd:50000}, {id:'e2',amountVnd:20000},
 {id:'e3',amountVnd:30000}
];
export default function App() {
 const [rows,setRows] = useState(initial);
 const total = rows.reduce((sum,r)=>sum+r.amountVnd,0);
 return <main><h1>Expenses</h1><p>Total: {total} VND</p>
 <ul>{rows.map(r=><li key={r.id}>{r.amountVnd}
 <button onClick={()=>setRows(old=>old.filter(x=>x.id!==r.id))}>
 Delete {r.id}</button></li>)}</ul></main>;
}