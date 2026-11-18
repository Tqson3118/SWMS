const { chromium } = require('../../../../frontend/node_modules/playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 390, height: 844 }, isMobile: true });
  await page.goto('http://127.0.0.1:3000/auth', { waitUntil: 'networkidle' });
  await page.click('[data-testid="demo-switcher-toggle"]');
  await page.click('[data-testid="switch-user-default"]');
  await page.waitForURL('**/app**');

  await page.goto('http://127.0.0.1:3000/app/courses/22222222-2222-4222-8222-222222222222/quiz', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  const text = await page.innerText('body');
  console.log('Page body text length:', text.length);
  console.log('Page body text sample:\n', text.slice(0, 600));

  const buttons = await page.$$eval('button', els => els.map(e => e.textContent.trim()));
  console.log('All buttons on quiz page:', buttons);

  await browser.close();
})();
