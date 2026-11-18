import pkg from '../../../../frontend/node_modules/playwright/index.js';
const { chromium } = pkg;

async function testRoutes() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext();
  const page = await context.newPage();

  // 1. Login
  await page.goto('http://127.0.0.1:3000/auth');
  await page.click('[data-testid="demo-switcher-toggle"]');
  await Promise.all([
    page.waitForResponse(res => res.url().includes('/api/v1/auth/login') && res.status() === 200),
    page.click('[data-testid="switch-user-default"]')
  ]);
  await page.waitForURL('**/app**', { timeout: 15000 });
  await page.waitForLoadState('networkidle');

  const courseId = '25f79a81-8ac2-46ae-99e1-85975ff42a8b';
  const routes = [
    '/',
    '/auth',
    '/app',
    '/app/courses',
    `/app/courses/${courseId}`,
    `/app/courses/${courseId}/learn`,
    `/app/courses/${courseId}/quiz`,
    '/app/review',
    '/app/documents',
    '/app/notes',
    '/app/settings',
    '/workspace'
  ];

  console.log('Testing 12 routes...');
  for (const route of routes) {
    const url = `http://127.0.0.1:3000${route}`;
    const res = await page.goto(url, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(1000);
    console.log(`Route [${route}] -> Status: ${res.status()} | Final URL: ${page.url()}`);
  }

  await browser.close();
}

testRoutes().catch(console.error);
