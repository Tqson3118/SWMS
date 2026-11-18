const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../screenshots/c0-before');

(async () => {
  const browser = await chromium.launch({ headless: true });

  const viewports = [
    { name: '1440x900', width: 1440, height: 900 },
    { name: '768x1024', width: 768, height: 1024 },
    { name: '390x844', width: 390, height: 844 }
  ];

  for (const theme of ['light', 'dark']) {
    for (const vp of viewports) {
      const context = await browser.newContext({ viewport: { width: vp.width, height: vp.height } });
      const p = await context.newPage();

      await p.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
      await p.click('[data-testid="demo-switcher-toggle"]');
      await p.click('[data-testid="switch-user-default"]');
      await p.waitForURL('**/app**');
      await p.waitForTimeout(600);

      await p.evaluate((t) => {
        localStorage.setItem('swms_theme', t);
        if (t === 'dark') {
          document.documentElement.classList.add('dark');
          document.body?.classList.add('dark');
        } else {
          document.documentElement.classList.remove('dark');
          document.body?.classList.remove('dark');
        }
      }, theme);

      // Open command palette
      const searchBtn = p.locator('button:has-text("Tìm nhanh"), button:has-text("Tìm kiếm")').first();
      if (await searchBtn.isVisible()) {
        await searchBtn.click();
      } else {
        await p.keyboard.press('Control+k');
      }

      const modal = p.locator('[data-testid="command-palette-modal"]');
      if (await modal.isVisible()) {
        await p.waitForTimeout(400);
        const filename = `state-command-palette-${vp.name}-${theme}.png`;
        await p.screenshot({ path: path.join(OUT_DIR, filename), fullPage: true });
        console.log(`Saved: ${filename}`);
      }

      await p.close();
      await context.close();
    }
  }

  await browser.close();
  console.log('Command palette screenshots captured successfully!');
})().catch(console.error);
