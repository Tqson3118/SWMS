// Master Test Runner for A-ABUSER QA Suite
const { spawnSync } = require('child_process');
const path = require('path');
const fs = require('fs');

const scripts = [
  'poc-01-quota-abuse.js',
  'poc-02-fsrs-abuse.js',
  'poc-03-streak-progress-abuse.js',
  'poc-04-quiz-cheat.js',
  'poc-05-notebook-occ-abuse.js',
  'poc-06-extreme-inputs.js'
];

console.log('================================================================');
console.log('🚀 STUDXY ROUND 2: A-ABUSER AUTOMATED TEST SUITE');
console.log(`Started at: ${new Date().toISOString()}`);
console.log('================================================================\n');

const summary = [];

for (const scriptName of scripts) {
  const scriptPath = path.join(__dirname, scriptName);
  console.log(`>>> RUNNING: ${scriptName} ...`);
  const start = Date.now();
  const proc = spawnSync('node', [scriptPath], {
    encoding: 'utf8',
    timeout: 60000
  });
  const duration = Date.now() - start;

  const passed = proc.status === 0;
  console.log(proc.stdout);
  if (proc.stderr) {
    console.error(`[STDERR ${scriptName}]:`, proc.stderr);
  }

  console.log(`>>> FINISHED: ${scriptName} | Exit Code: ${proc.status} | Duration: ${duration}ms\n`);
  summary.push({
    script: scriptName,
    exitCode: proc.status,
    passed,
    durationMs: duration
  });
}

console.log('================================================================');
console.log('📊 A-ABUSER FINAL TEST EXECUTION SUMMARY:');
console.table(summary);
console.log('================================================================');

const allPassed = summary.every(s => s.passed);
if (!allPassed) {
  console.error('❌ One or more PoC test scripts encountered unexpected failures!');
  process.exit(1);
} else {
  console.log('✅ All PoC scripts executed successfully and generated reproducible evidence.');
}
