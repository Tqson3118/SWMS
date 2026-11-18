const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../frontend/node_modules/@playwright/test'));

async function testTheme() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const page = await context.newPage();

  await page.goto('http://127.0.0.1:3000/auth', { waitUntil: 'networkidle' });
  await page.click('[data-testid="demo-switcher-toggle"]');
  await page.click('[data-testid="switch-user-default"]');
  await page.waitForURL('**/app**');
  await page.waitForTimeout(1000);

  const outDir = path.resolve('docs/qa-round2/screenshots/test');
  if (!fs.existsSync(outDir)) fs.mkdirSync(outDir, { recursive: true });

  // Light
  await page.evaluate(() => {
    localStorage.setItem('swms_theme', 'light');
    document.documentElement.classList.remove('dark');
    document.body.classList.remove('dark');
    window.dispatchEvent(new StorageEvent('storage', { key: 'swms_theme', newValue: 'light' }));
  });
  await page.waitForTimeout(500);
  await page.screenshot({ path: path.join(outDir, 'dash-light.png'), fullPage: true });

  // Dark
  await page.evaluate(() => {
    localStorage.setItem('swms_theme', 'dark');
    document.documentElement.classList.add('dark');
    document.body.classList.add('dark');
    window.dispatchEvent(new StorageEvent('storage', { key: 'swms_theme', newValue: 'dark' }));
  });
  await page.waitForTimeout(500);
  await page.screenshot({ path: path.join(outDir, 'dash-dark.png'), fullPage: true });

  console.log('Screenshots saved successfully!');
  await browser.close();
}

testTheme().catch(console.error);
