/**
 * Master test runner for A-STUDENT QA Suite
 * Runs all individual student QA test suites in sequence and prints execution report.
 */
const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

const SCRIPTS_DIR = __dirname;
const SCREENSHOTS_DIR = path.resolve(__dirname, '../../screenshots/a-student');

const testSuites = [
  { file: '01-observe-son-tran.js', name: 'Task 1: Observe Demo User (Sơn Trần)' },
  { file: '02-student-course-creation.js', name: 'Task 2: Student Registration & Course Creation (3 Flows)' },
  { file: '03-student-learning-and-notebook.js', name: 'Task 3: Reading, Notebook Autosave & Flashcard' },
  { file: '04-student-review-fsrs.js', name: 'Task 4: FSRS Spaced Repetition Review' },
  { file: '05-student-quiz.js', name: 'Task 5: Quiz Evaluation & 0-Question Edge Case' },
  { file: '06-command-palette-and-remaining-pages.js', name: 'Task 6: Command Palette & Remaining Pages' }
];

console.log('================================================================');
console.log('       A-STUDENT COMPREHENSIVE QA TEST SUITE RUNNER            ');
console.log('================================================================');
console.log(`Time: ${new Date().toISOString()}`);
console.log(`Scripts Directory: ${SCRIPTS_DIR}`);
console.log(`Screenshots Directory: ${SCREENSHOTS_DIR}\n`);

const summary = [];

for (const suite of testSuites) {
  const scriptPath = path.join(SCRIPTS_DIR, suite.file);
  console.log(`>>> RUNNING: ${suite.name} (${suite.file})`);
  const start = Date.now();
  let exitCode = 0;
  let errorOutput = '';

  try {
    const stdout = execSync(`node "${scriptPath}"`, {
      encoding: 'utf-8',
      stdio: 'pipe',
      timeout: 120000
    });
    console.log(stdout.trim());
  } catch (err) {
    exitCode = err.status || 1;
    errorOutput = err.stderr || err.message;
    console.error(`FAILED with exit code ${exitCode}:\n`, errorOutput);
  }

  const duration = ((Date.now() - start) / 1000).toFixed(2);
  summary.push({
    name: suite.name,
    file: suite.file,
    exitCode,
    duration: `${duration}s`,
    status: exitCode === 0 ? 'PASS' : 'FAIL'
  });
  console.log(`<<< FINISHED ${suite.file} in ${duration}s (Exit code: ${exitCode})\n`);
}

console.log('================================================================');
console.log('                     EXECUTION SUMMARY                          ');
console.log('================================================================');
console.table(summary);

const screenshots = fs.existsSync(SCREENSHOTS_DIR) ? fs.readdirSync(SCREENSHOTS_DIR) : [];
console.log(`\nTotal Screenshots Captured: ${screenshots.length}`);
console.log('Screenshots:', screenshots);

const allPassed = summary.every(s => s.exitCode === 0);
process.exit(allPassed ? 0 : 1);
