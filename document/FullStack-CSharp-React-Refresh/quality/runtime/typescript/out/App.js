"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = App;
const jsx_runtime_1 = require("react/jsx-runtime");
const react_1 = require("react");
const initial = [
    { id: 'e1', amountVnd: 50000 }, { id: 'e2', amountVnd: 20000 },
    { id: 'e3', amountVnd: 30000 }
];
function App() {
    const [rows, setRows] = (0, react_1.useState)(initial);
    const total = rows.reduce((sum, r) => sum + r.amountVnd, 0);
    return (0, jsx_runtime_1.jsxs)("main", { children: [(0, jsx_runtime_1.jsx)("h1", { children: "Expenses" }), (0, jsx_runtime_1.jsxs)("p", { children: ["Total: ", total, " VND"] }), (0, jsx_runtime_1.jsx)("ul", { children: rows.map(r => (0, jsx_runtime_1.jsxs)("li", { children: [r.amountVnd, (0, jsx_runtime_1.jsxs)("button", { onClick: () => setRows(old => old.filter(x => x.id !== r.id)), children: ["Delete ", r.id] })] }, r.id)) })] });
}
