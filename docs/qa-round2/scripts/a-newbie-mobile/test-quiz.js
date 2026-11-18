const { chromium } = require('../../../../frontend/node_modules/playwright');
const path = require('path');
const fs = require('fs');

const SCREENSHOT_DIR = path.resolve(__dirname, '../../screenshots/a-newbie-mobile');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true });
  const page = await context.newPage();

  // Login as default user Son Tran
  await page.goto('http://127.0.0.1:3000/auth', { waitUntil: 'networkidle' });
  await page.click('[data-testid="demo-switcher-toggle"]');
  await page.click('[data-testid="switch-user-default"]');
  await page.waitForURL('**/app**');

  const courseId = '22222222-2222-4222-8222-222222222222';
  console.log(`Navigating to /app/courses/${courseId}/quiz on 390x844...`);
  await page.goto(`http://127.0.0.1:3000/app/courses/${courseId}/quiz`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);

  await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-overview.png'), fullPage: false });
  console.log('Captured: quiz-390-overview.png');

  // Look for quiz card with text "Bài luyện tập" or button
  const quizCard = page.locator('button, div[role="button"]').filter({ hasText: /Bài luyện tập/i }).first();
  if (await quizCard.isVisible().catch(() => false)) {
    console.log('Clicking quiz card...');
    await quizCard.click();
    await page.waitForTimeout(800);
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-active-question.png'), fullPage: false });
    console.log('Captured: quiz-390-active-question.png');

    // Click choice
    const choiceBtn = page.locator('button[data-testid^="quiz-choice-"], label:has(input[type="radio"]), button.text-left').first();
    if (await choiceBtn.isVisible().catch(() => false)) {
      console.log('Selecting answer choice on 390px...');
      await choiceBtn.click();
      await page.waitForTimeout(400);
      await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-choice-selected.png'), fullPage: false });
      console.log('Captured: quiz-390-choice-selected.png');

      const submitBtn = page.locator('button:has-text("Nộp bài"), button:has-text("Gửi câu trả lời"), button:has-text("Chấm điểm")').first();
      if (await submitBtn.isVisible().catch(() => false)) {
        console.log('Clicking submit quiz...');
        await submitBtn.click();
        await page.waitForTimeout(1500);
        await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'quiz-390-submitted-result.png'), fullPage: false });
        console.log('Captured: quiz-390-submitted-result.png');
      }
    }
  }

  await browser.close();
  console.log('Finished quiz test successfully!');
})();
