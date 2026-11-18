import fs from 'node:fs';
import path from 'node:path';
import { execSync } from 'node:child_process';

const CHROME_PATH = 'C:\\Users\\Administrator\\AppData\\Local\\ms-playwright\\chromium-1248\\chrome-win64\\chrome.exe';
const LH_DIR = path.resolve('docs/qa-round2/data/c0-perf/lighthouse');
fs.mkdirSync(LH_DIR, { recursive: true });

const targets = [
  { name: 'landing', url: 'http://127.0.0.1:3000/' },
  { name: 'auth', url: 'http://127.0.0.1:3000/auth' },
];

const modes = [
  { name: 'desktop', flags: '--preset=desktop' },
  { name: 'mobile', flags: '--form-factor=mobile --throttling-method=simulate' },
];

function median(arr) {
  if (!arr.length) return 0;
  const s = [...arr].sort((a, b) => a - b);
  const mid = Math.floor(s.length / 2);
  return s.length % 2 !== 0 ? s[mid] : (s[mid - 1] + s[mid]) / 2;
}

const summary = [];

console.log('=== STARTING LIGHTHOUSE PUBLIC AUDIT ===');
for (const target of targets) {
  for (const mode of modes) {
    console.log(`\nAuditing [${target.name}] (${mode.name})...`);
    const runResults = [];

    for (let run = 1; run <= 3; run++) {
      const outPath = path.join(LH_DIR, `${target.name}-${mode.name}-run${run}.json`);
      const cmd = `npx --yes lighthouse "${target.url}" --output=json --output-path="${outPath}" --chrome-flags="--headless" ${mode.flags} --only-categories=performance --quiet`;
      console.log(`  Run ${run}/3: executing lighthouse...`);
      try {
        execSync(cmd, {
          env: { ...process.env, CHROME_PATH },
          stdio: ['ignore', 'pipe', 'pipe'],
          timeout: 60000,
        });

        const data = JSON.parse(fs.readFileSync(outPath, 'utf8'));
        const audits = data.audits || {};
        const score = (data.categories?.performance?.score || 0) * 100;
        const fcp = audits['first-contentful-paint']?.numericValue || 0;
        const lcp = audits['largest-contentful-paint']?.numericValue || 0;
        const tbt = audits['total-blocking-time']?.numericValue || 0;
        const cls = audits['cumulative-layout-shift']?.numericValue || 0;
        const si = audits['speed-index']?.numericValue || 0;

        console.log(`    Score: ${score.toFixed(0)} | FCP: ${fcp.toFixed(0)}ms | LCP: ${lcp.toFixed(0)}ms | TBT: ${tbt.toFixed(0)}ms | CLS: ${cls.toFixed(4)}`);
        runResults.push({ score, fcp, lcp, tbt, cls, si });
      } catch (e) {
        console.error(`    Lighthouse run ${run} failed:`, e.message);
      }
    }

    if (runResults.length > 0) {
      const med = {
        page: target.name,
        url: target.url,
        mode: mode.name,
        runs: runResults.length,
        score: +median(runResults.map(r => r.score)).toFixed(1),
        fcp: +median(runResults.map(r => r.fcp)).toFixed(1),
        lcp: +median(runResults.map(r => r.lcp)).toFixed(1),
        tbt: +median(runResults.map(r => r.tbt)).toFixed(1),
        cls: +median(runResults.map(r => r.cls)).toFixed(4),
        speedIndex: +median(runResults.map(r => r.si)).toFixed(1),
      };
      summary.push(med);
      console.log(`  -> MEDIAN [${target.name} ${mode.name}]: Score=${med.score} | LCP=${med.lcp}ms | TBT=${med.tbt}ms | CLS=${med.cls}`);
    }
  }
}

const summaryPath = path.join(LH_DIR, 'lighthouse-summary.json');
fs.writeFileSync(summaryPath, JSON.stringify(summary, null, 2), 'utf8');
console.log(`\nLighthouse audit finished. Saved summary to: ${summaryPath}`);
