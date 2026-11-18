const path = require('path');
const fs = require('fs');
const {
  BASE_URL,
  DEFAULT_VIEWPORT,
  chromium,
  attachDiagnostics,
  safeScreenshot,
  loginViaUi
} = require('./common');

const SESSION_FILE = path.resolve(__dirname, '../../data/student-session.json');

async function runStudentQuiz() {
  console.log('--- STARTING TASK 5: STUDENT QUIZ & 0-QUESTION TEST ---');
  const diagnostics = { consoleErrors: [], pageErrors: [], findings: [] };

  if (!fs.existsSync(SESSION_FILE)) {
    throw new Error('Session file not found. Please run 02-student-course-creation.js first.');
  }
  const session = JSON.parse(fs.readFileSync(SESSION_FILE, 'utf-8'));
  const aiCourseId = session.courses?.aiCourse?.id;
  const zeroQuizCourseId = '52db4a51-8c0b-4b9f-a8aa-6fb77b3865fb'; // .NET & React Roadmap has 0 quizzes

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: DEFAULT_VIEWPORT });
  const page = await context.newPage();
  attachDiagnostics(page, 'student-quiz', diagnostics);

  try {
    // 1. Login as the student
    console.log(`[1/5] Logging in as student: ${session.email}...`);
    await loginViaUi(page, session.email, session.password);

    // ==========================================
    // PART A: Test Course WITH Quiz
    // ==========================================
    console.log(`[2/5] Navigating to Quiz for AI Course: ${aiCourseId}...`);
    await page.goto(`${BASE_URL}/app/courses/${aiCourseId}/quiz`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1500);

    // Verify Quiz Items Loaded
    const quizItems = page.locator('[data-testid^="quiz-item-"]');
    const itemCount = await quizItems.count();
    console.log(`Quiz items loaded on page: ${itemCount}`);

    if (itemCount > 0) {
      await safeScreenshot(page, '26-quiz-taking.png', 'Quiz Question Taking View');

      // Question 1: Multiple Choice
      console.log('Answering Question 1 (Multiple Choice)...');
      const firstChoiceRadio = page.locator('input[type="radio"]').first();
      if (await firstChoiceRadio.isVisible()) {
        await firstChoiceRadio.click();
        await page.waitForTimeout(300);

        const checkBtn1 = page.locator('button:has-text("Kiểm tra")').first();
        if (await checkBtn1.isVisible() && await checkBtn1.isEnabled()) {
          await checkBtn1.click();
          console.log('Submitted Answer 1, waiting for evaluation...');
          await page.waitForTimeout(2000);
        }
      }

      // Question 2: Multiple Choice
      console.log('Answering Question 2 (Multiple Choice)...');
      const secondQuestionRadios = page.locator('[data-testid="quiz-item-2"] input[type="radio"]').first();
      if (await secondQuestionRadios.isVisible()) {
        await secondQuestionRadios.click();
        await page.waitForTimeout(300);

        const checkBtn2 = page.locator('[data-testid="quiz-item-2"] button:has-text("Kiểm tra")').first();
        if (await checkBtn2.isVisible() && await checkBtn2.isEnabled()) {
          await checkBtn2.click();
          console.log('Submitted Answer 2...');
          await page.waitForTimeout(2000);
        }
      }

      // Question 3: Essay (Tự luận)
      console.log('Answering Question 3 (Essay / Tự luận)...');
      const essayTextarea = page.locator('textarea[placeholder*="Trình bày"]').first();
      if (await essayTextarea.isVisible()) {
        await essayTextarea.fill('Tôi sẽ áp dụng kiến thức của khóa học vào việc giao tiếp hằng ngày: luyện nói phản xạ 15 phút mỗi sáng, xem tin tức tiếng Anh và chủ động áp dụng từ vựng vào ngữ cảnh thực tế.');
        await page.waitForTimeout(400);

        const checkBtn3 = page.locator('button:has-text("Kiểm tra")').last();
        if (await checkBtn3.isVisible() && await checkBtn3.isEnabled()) {
          await checkBtn3.click();
          console.log('Submitted Essay, waiting for AI rubric evaluation...');
          // Essay grading may take 3-5 seconds
          await page.waitForTimeout(5000);
        }
      }

      await safeScreenshot(page, '27-quiz-result.png', 'Quiz Evaluation Results & Feedback');
    }

    // ==========================================
    // PART B: Check Dashboard Score Reflection
    // ==========================================
    console.log('[3/5] Checking Dashboard score reflection (/app)...');
    await page.goto(`${BASE_URL}/app`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1200);
    await safeScreenshot(page, '29-dashboard-quiz-progress.png', 'Dashboard with Quiz Progress');

    // ==========================================
    // PART C: Test Course with 0 Quiz Questions (B15 Check)
    // ==========================================
    console.log(`[4/5] Testing 0-question quiz course: ${zeroQuizCourseId}...`);
    // First visit course overview
    await page.goto(`${BASE_URL}/app/courses/${zeroQuizCourseId}`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);

    const quizBtn = page.locator('[data-testid="course-quiz-btn"]').first();
    const isQuizBtnVisible = await quizBtn.isVisible();
    const quizBtnText = await quizBtn.textContent().catch(() => '');
    console.log(`Course overview quiz button visible: ${isQuizBtnVisible}, text: "${quizBtnText.trim()}"`);

    if (isQuizBtnVisible) {
      diagnostics.findings.push({
        id: 'B15-QUIZ-EMPTY-BUTTON',
        title: 'Nút "Làm bài kiểm tra" vẫn hiển thị khi khóa học có 0 câu hỏi quiz',
        severity: 'Low',
        buttonText: quizBtnText.trim(),
        detail: 'Trên trang /app/courses/[id], nút Làm bài kiểm tra vẫn hiển thị và cho click dù course.quizCount = 0.'
      });

      // Click to view 0-question quiz page
      console.log('[5/5] Navigating to /quiz for 0-question course...');
      await quizBtn.click();
      await page.waitForURL('**/quiz**', { timeout: 10000 });
      await page.waitForTimeout(1000);

      const emptyHeading = await page.locator('text=Khóa học này chưa có bài kiểm tra nào').textContent().catch(() => null);
      console.log('0-quiz page heading detected:', emptyHeading);

      await safeScreenshot(page, '28-quiz-zero-questions.png', '0-Question Quiz Empty State');
    }

  } catch (err) {
    console.error('Error during quiz task:', err);
    diagnostics.findings.push({
      id: 'QUIZ-ERROR',
      title: 'Exception during Quiz Task',
      error: err.message
    });
  } finally {
    await browser.close();
  }

  console.log('--- TASK 5 COMPLETE ---');
  console.log('Console Errors:', diagnostics.consoleErrors.length);
  console.log('Page Errors:', diagnostics.pageErrors.length);
  console.log('Findings:', JSON.stringify(diagnostics.findings, null, 2));
  return diagnostics;
}

if (require.main === module) {
  runStudentQuiz().then(() => process.exit(0)).catch(err => {
    console.error(err);
    process.exit(1);
  });
}

module.exports = { runStudentQuiz };
