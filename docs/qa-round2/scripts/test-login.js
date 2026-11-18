const { chromium } = require('D:/03_Study/Studxy/frontend/node_modules/@playwright/test');

async function testLogin() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const page = await context.newPage();

  console.log('Navigating to http://127.0.0.1:3000/auth...');
  await page.goto('http://127.0.0.1:3000/auth', { waitUntil: 'networkidle' });

  console.log('Clicking demo-switcher-toggle...');
  await page.click('[data-testid="demo-switcher-toggle"]');
  await page.waitForSelector('[data-testid="switch-user-default"]');

  console.log('Clicking switch-user-default (Son Tran)...');
  await page.click('[data-testid="switch-user-default"]');

  await page.waitForURL('**/app**', { timeout: 15000 });
  console.log('Logged in successfully! Current URL:', page.url());

  const title = await page.textContent('h1');
  console.log('Dashboard H1:', title);

  await browser.close();
}

testLogin().catch(err => {
  console.error('Error in testLogin:', err);
  process.exit(1);
});
