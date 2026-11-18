const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../screenshots/c0-before');

async function testCmd() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const page = await context.newPage();

  await page.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await page.click('[data-testid="demo-switcher-toggle"]');
  await page.click('[data-testid="switch-user-default"]');
  await page.waitForURL('**/app**');

  await page.evaluate(() => {
    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'k', ctrlKey: true, bubbles: true }));
  });

  await page.waitForSelector('[data-testid="command-palette-modal"]', { timeout: 3000 });
  console.log('Command palette modal is visible!');

  for (const theme of ['light', 'dark']) {
    for (const vp of [
      { name: '1440x900', width: 1440, height: 900 },
      { name: '768x1024', width: 768, height: 1024 },
      { name: '390x844', width: 390, height: 844 }
    ]) {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.evaluate((t) => {
        localStorage.setItem('swms_theme', t);
        if (t === 'dark') {
          document.documentElement.classList.add('dark');
          document.body?.classList.add('dark');
        } else {
          document.documentElement.classList.remove('dark');
          document.body?.classList.remove('dark');
        }
      }, theme);
      await page.waitForTimeout(300);
      const filename = `state-command-palette-${vp.name}-${theme}.png`;
      await page.screenshot({ path: path.join(OUT_DIR, filename), fullPage: true });
      console.log(`Saved: ${filename}`);
    }
  }

  await browser.close();
}

testCmd().catch(console.error);
