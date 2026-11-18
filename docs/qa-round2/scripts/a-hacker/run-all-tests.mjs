// docs/qa-round2/scripts/a-hacker/run-all-tests.mjs
// Master Penetration Test Suite Runner for Studxy (Round 2)
import { spawnSync } from 'node:child_process';
import path from 'node:path';

const suites = [
  '01-auth-tests.mjs',
  '02-idor-tests.mjs',
  '03-injection-tests.mjs',
  '04-headers-cors-tests.mjs',
  '05-mass-assignment-tests.mjs',
  '06-info-disclosure-tests.mjs'
];

console.log('================================================================');
console.log('     STUDXY PENETRATION TEST & SECURITY AUDIT RUNNER');
console.log('================================================================\n');

let totalSuites = 0;
let passedSuites = 0;

for (const suite of suites) {
  totalSuites++;
  const fullPath = path.resolve('docs/qa-round2/scripts/a-hacker', suite);
  console.log(`\n>>> RUNNING TEST SUITE [${totalSuites}/${suites.length}]: ${suite} ...`);
  const start = Date.now();
  const res = spawnSync(process.execPath, [fullPath], { stdio: 'inherit' });
  const duration = Date.now() - start;

  if (res.status === 0) {
    passedSuites++;
    console.log(`>>> [OK] ${suite} completed in ${duration}ms (exit code 0)`);
  } else {
    console.error(`>>> [FAIL] ${suite} exited with code ${res.status}`);
  }
}

console.log('\n================================================================');
console.log(`COMPLETED: ${passedSuites}/${totalSuites} test suites executed successfully.`);
console.log('================================================================\n');
