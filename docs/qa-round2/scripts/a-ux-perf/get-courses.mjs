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
    page.click('[data-testid="switch-user-default"]'),
  ]);
  await page.waitForURL('**/app**');
  await page.goto('http://127.0.0.1:3000/app/courses');
  await page.waitForSelector('a[href^="/app/courses/"]', { timeout: 10000 });

  const courseLinks = await page.evaluate(() => {
    const links = Array.from(document.querySelectorAll('a[href^="/app/courses/"]'));
    return links.map(a => ({ href: a.getAttribute('href'), text: a.textContent?.trim() }));
  });

  console.log('Course links count:', courseLinks.length);
  console.log('Course links:', courseLinks.slice(0, 5));
  await browser.close();
})();
