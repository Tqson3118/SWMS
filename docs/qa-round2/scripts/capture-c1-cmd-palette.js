const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../screenshots/c1-after');

const VIEWPORTS = [
  { name: '1440x900', width: 1440, height: 900 },
  { name: '768x1024', width: 768, height: 1024 },
  { name: '390x844', width: 390, height: 844, isMobile: true }
];

const THEMES = ['light', 'dark'];

async function captureCmdPalette() {
  const browser = await chromium.launch({ headless: true });

  for (const vp of VIEWPORTS) {
    for (const theme of THEMES) {
      const context = await browser.newContext({
        viewport: { width: vp.width, height: vp.height },
        isMobile: vp.isMobile || false
      });
      const page = await context.newPage();

      await page.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
      await page.click('[data-testid="demo-switcher-toggle"]');
      await page.click('[data-testid="switch-user-default"]');
      await page.waitForURL('**/app**');

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

      await page.waitForTimeout(600);

      // Trigger search button or keyboard
      const searchBtn = page.locator('[data-testid="cmd-k-trigger"], [data-testid="mobile-search-btn"], button:has-text("Tìm kiếm..."), button:has-text("Tìm nhanh")').first();
      if (await searchBtn.isVisible()) {
        await searchBtn.click();
      } else {
        await page.keyboard.press('Control+KeyK');
      }

      await page.waitForSelector('[data-testid="command-palette-modal"]', { timeout: 5000 });
      await page.waitForTimeout(400);

      const filename = `state-command-palette-${vp.name}-${theme}.png`;
      await page.screenshot({ path: path.join(OUT_DIR, filename), fullPage: true });
      console.log(`Saved: ${filename}`);

      await page.close();
      await context.close();
    }
  }

  await browser.close();
  console.log('Command palette captures finished!');
}

captureCmdPalette().catch(console.error);
