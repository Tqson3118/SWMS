import pkg from '../../../../frontend/node_modules/playwright/index.js';
const { chromium } = pkg;

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext();
  const page = await context.newPage();
  await page.goto('http://127.0.0.1:3000/auth');
  await page.click('[data-testid="demo-switcher-toggle"]');
  await Promise.all([
    page.waitForResponse(res => res.url().includes('/api/v1/auth/login') && res.status() === 200),
    page.click('[data-testid="switch-user-default"]')
  ]);
  await page.waitForURL('**/app**');

  const courseId = '79B30ACD-0BD5-41C8-8744-271AD7B496CE';
  const lessonId = '0360C033-EBC7-453D-9FAE-C812D3BCB48A';

  // Navigate to the lesson page and let Next.js load it!
  await page.goto(`http://127.0.0.1:3000/app/courses/${courseId}/learn?lesson=${lessonId}`);
  await page.waitForSelector('[data-testid="lesson-markdown-view"]', { timeout: 15000 });

  const textContent = await page.evaluate(() => {
    const el = document.querySelector('[data-testid="lesson-markdown-view"]');
    return el ? el.innerHTML : 'NOT FOUND';
  });

  console.log('=== RENDERED HTML SAMPLE ===');
  console.log(textContent.slice(0, 2000));

  await browser.close();
})();
