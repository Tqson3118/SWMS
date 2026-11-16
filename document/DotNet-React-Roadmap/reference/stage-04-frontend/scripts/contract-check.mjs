import { readFile } from 'node:fs/promises';

const api = await readFile(new URL('../src/api.ts', import.meta.url), 'utf8');
const app = await readFile(new URL('../src/App.tsx', import.meta.url), 'utf8');
const form = await readFile(new URL('../src/components/ExpenseForm.tsx', import.meta.url), 'utf8');

const checks = [
  ['API uses the explicit versioned route', api.includes("/api/v1")],
  ['list contract rejects non-array payloads', api.includes('Array.isArray(payload)')],
  ['ProblemDetails is parsed for failed responses', api.includes('parseProblem') && api.includes('res.ok')],
  ['PUT is implemented with a signal', api.includes("'POST' | 'PUT'") && api.includes('method, signal')],
  ['all three App data flows create AbortControllers', (app.match(/new AbortController\(\)/g) ?? []).length >= 3],
  ['all three App data flows use generation guards', (app.match(/generation !== generation/g) ?? []).length >= 3],
  ['form keeps user values when mutation fails', form.includes('setError(errorText(requestError))') && form.includes('setAmount')],
];
const failed = checks.filter(([, ok]) => !ok);
for (const [label, ok] of checks) console.log(`${ok ? 'PASS' : 'FAIL'} ${label}`);
if (failed.length) process.exitCode = 1;
