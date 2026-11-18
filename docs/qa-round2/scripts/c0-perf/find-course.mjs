import pkg from '../../../../frontend/node_modules/playwright/index.js';
const { chromium } = pkg;

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext();
  const page = await context.newPage();

  page.on('response', async res => {
    if (res.url().includes('/api/v1/')) {
      console.log('API:', res.status(), res.request().method(), res.url());
      if (res.status() >= 400) {
        try {
          console.log('API ERROR BODY:', await res.text());
        } catch {}
      }
    }
  });

  console.log('1. Going to /auth');
  await page.goto('http://127.0.0.1:3000/auth');
  await page.click('[data-testid="demo-switcher-toggle"]');
  await page.click('[data-testid="switch-user-default"]');
  await page.waitForURL('**/app**', { timeout: 15000 });
  console.log('2. URL after login:', page.url());

  // Check cookies
  const cookies = await context.cookies();
  console.log('Cookies in context:', cookies);

  // Now reload page
  console.log('3. Reloading page...');
  await page.reload();
  await page.waitForTimeout(3000);
  console.log('URL after reload:', page.url());

  await browser.close();
}

main().catch(err => {
  console.error(err);
  process.exit(1);
});
