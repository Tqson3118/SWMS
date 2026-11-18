import pkg from '../../../../frontend/node_modules/playwright/index.js';
const { chromium } = pkg;

async function checkPages() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext();
  const page = await context.newPage();

  // Login
  await page.goto('http://127.0.0.1:3000/auth');
  await page.click('[data-testid="demo-switcher-toggle"]');
  await Promise.all([
    page.waitForResponse(res => res.url().includes('/api/v1/auth/login') && res.status() === 200),
    page.click('[data-testid="switch-user-default"]')
  ]);
  await page.waitForURL('**/app**');
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(1000);

  const courseId = '25f79a81-8ac2-46ae-99e1-85975ff42a8b';
  const urls = [
    { name: 'courses', url: `http://127.0.0.1:3000/app/courses` },
    { name: 'course-detail', url: `http://127.0.0.1:3000/app/courses/${courseId}` },
    { name: 'learn', url: `http://127.0.0.1:3000/app/courses/${courseId}/learn` },
    { name: 'quiz', url: `http://127.0.0.1:3000/app/courses/${courseId}/quiz` },
    { name: 'review', url: `http://127.0.0.1:3000/app/review` },
  ];

  for (const item of urls) {
    await page.goto(item.url);
    await page.waitForLoadState('networkidle');
    const url = page.url();
    const textPreview = (await page.innerText('body')).replace(/\s+/g, ' ').slice(0, 100);
    console.log(`Page [${item.name}]: URL=${url} | Preview: ${textPreview}`);
  }

  await browser.close();
}

checkPages().catch(console.error);
