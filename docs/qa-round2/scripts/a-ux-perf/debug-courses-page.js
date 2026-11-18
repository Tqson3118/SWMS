const path = require('path');
const { chromium } = require(path.resolve(__dirname, '../../../../frontend/node_modules/@playwright/test'));

(async () => {
  const browser = await chromium.launch({ headless: true });
  const p = await browser.newPage({ viewport: { width: 1440, height: 900 } });

  await p.goto('http://127.0.0.1:3000/auth');
  await p.click('[data-testid="demo-switcher-toggle"]');
  await p.click('[data-testid="switch-user-default"]');
  await p.waitForURL('**/app**');

  await p.goto('http://127.0.0.1:3000/app/courses');
  await p.waitForTimeout(2000);

  const text = await p.evaluate(() => document.body.innerText);
  console.log('=== PAGE TEXT ON /app/courses ===');
  console.log(text.slice(0, 1000));

  await browser.close();
})();
