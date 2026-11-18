const { chromium } = require('../../../../frontend/node_modules/playwright');
const path = require('path');
const fs = require('fs');

const SCREENSHOT_DIR = path.resolve(__dirname, '../../screenshots/a-newbie-mobile');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true });
  await page.goto('http://127.0.0.1:3000', { waitUntil: 'networkidle' });

  const loginRes = await page.evaluate(async () => {
    const res = await fetch('/api/v1/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: 'son.tran@studxy.vn', password: 'Studxy@2026' })
    });
    return await res.json();
  });

  const token = loginRes.accessToken;
  const user = loginRes.user;
  console.log('Logged in as Son Tran. Token:', Boolean(token));

  // Fetch quizzes with token
  const quizzes = await page.evaluate(async (token) => {
    const res = await fetch('/api/v1/practice-quizzes', {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    return await res.json();
  }, token);

  console.log('Quizzes count:', quizzes.length);
  for (const q of quizzes) {
    console.log(`Quiz ID: ${q.id}, CourseId: ${q.courseId}, Title: ${q.title}`);
  }

  if (quizzes.length > 0) {
    const targetQuiz = quizzes[0];
    const targetCourseId = targetQuiz.courseId || '52db4a51-8c0b-4b9f-a8aa-6fb77b3865fb';
    
    // Set session in page
    await page.evaluate(({ user }) => {
      window.localStorage.setItem('swms_has_refresh_cookie', '1');
      window.localStorage.setItem('swms_active_user_id', user.id);
      window.localStorage.setItem('swms_active_user', JSON.stringify({
        id: user.id,
        fullName: user.fullName,
        email: user.email,
        authProvider: 'local',
        avatarUrl: null,
        hasLocalPassword: true,
      }));
    }, { user });

    console.log(`Navigating to /app/courses/${targetCourseId}/quiz on 390x844...`);
    await page.goto(`http://127.0.0.1:3000/app/courses/${targetCourseId}/quiz`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);

    await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-overview.png'), fullPage: false });
    console.log('Captured: quiz-390-overview.png');

    // Click on quiz card if present
    const quizCard = page.locator('button, div[role="button"], a').filter({ hasText: targetQuiz.title }).first();
    if (await quizCard.isVisible().catch(() => false)) {
      console.log('Clicking target quiz card...');
      await quizCard.click();
      await page.waitForTimeout(600);
      await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-active-question.png'), fullPage: false });
      console.log('Captured: quiz-390-active-question.png');

      // Click option
      const choiceBtn = page.locator('button[data-testid^="quiz-choice-"], label:has(input[type="radio"])').first();
      if (await choiceBtn.isVisible().catch(() => false)) {
        console.log('Selecting choice...');
        await choiceBtn.click();
        await page.waitForTimeout(400);
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-choice-selected.png'), fullPage: false });
        console.log('Captured: quiz-390-choice-selected.png');

        const submitBtn = page.locator('button:has-text("Nộp bài"), button:has-text("Gửi câu trả lời"), button[type="submit"]').first();
        if (await submitBtn.isVisible().catch(() => false)) {
          console.log('Submitting choice...');
          await submitBtn.click();
          await page.waitForTimeout(1500);
          await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-submitted-result.png'), fullPage: false });
          console.log('Captured: quiz-390-submitted-result.png');
        }
      }
    }
  }

  await browser.close();
})();
