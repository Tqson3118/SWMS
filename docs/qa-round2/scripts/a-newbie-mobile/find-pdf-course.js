const { chromium } = require('../../../../frontend/node_modules/playwright');
const path = require('path');
const fs = require('fs');

const SCREENSHOT_DIR = path.resolve(__dirname, '../../screenshots/a-newbie-mobile');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 768, height: 1024 }, isMobile: true, hasTouch: true });
  const page = await context.newPage();
  
  await page.goto('http://127.0.0.1:3000/auth', { waitUntil: 'networkidle' });
  await page.click('[data-testid="demo-switcher-toggle"]');
  await page.click('[data-testid="switch-user-default"]');
  await page.waitForURL('**/app**');

  const courseUrl = '/app/courses/52db4a51-8c0b-4b9f-a8aa-6fb77b3865fb/learn';
  console.log(`Navigating to ${courseUrl} on 768x1024...`);
  await page.goto(`http://127.0.0.1:3000${courseUrl}`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);

  const splitTab = page.locator('[data-testid="center-tab-split"]');
  const isSplitVisible = await splitTab.isVisible().catch(() => false);
  console.log('Split tab visible on 768px tablet:', isSplitVisible);

  if (isSplitVisible) {
    await splitTab.click();
    await page.waitForTimeout(800);
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'learn-768-tab-split.png'), fullPage: false });
    console.log('Captured: learn-768-tab-split.png');
  }

  await browser.close();
})();
