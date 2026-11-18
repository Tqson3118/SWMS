const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../../screenshots/a-ux-perf');
const DATA_DIR = path.resolve(__dirname, '../../data/a-ux-perf');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const p = await context.newPage();

  // Login
  await p.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await p.click('[data-testid="demo-switcher-toggle"]');
  await Promise.all([
    p.waitForResponse(res => res.url().includes('/api/v1/auth/login') && res.status() === 200),
    p.click('[data-testid="switch-user-default"]')
  ]);
  await p.waitForURL('**/app**', { timeout: 15000 });
  await p.waitForTimeout(600);

  // Navigate to learn page with a non-existent PDF document ID
  const courseId = '22222222-2222-4222-8222-222222222222';
  const badDocId = '00000000-0000-0000-0000-000000000000';
  await p.goto(`${BASE_URL}/app/courses/${courseId}/learn?doc=${badDocId}`, { waitUntil: 'networkidle' });
  await p.waitForTimeout(1500);

  // Switch to PDF tab
  const pdfTabBtn = p.locator('[data-testid="center-tab-pdf"]');
  if (await pdfTabBtn.isVisible()) {
    await pdfTabBtn.click();
    await p.waitForTimeout(1000);
  }

  // Check what is rendered in the canvas/reading area
  const pdfAreaState = await p.evaluate(() => {
    const mainEl = document.querySelector('main');
    return {
      text: mainEl ? mainEl.innerText.slice(0, 500) : 'No main',
      html: mainEl ? mainEl.innerHTML.slice(0, 1500) : 'No main'
    };
  });

  console.log('=== PDF 404 AREA STATE ===');
  console.log(pdfAreaState.text);

  await p.screenshot({ path: path.join(OUT_DIR, 'pdf-404-state.png'), fullPage: false });

  // Now click "Bản gốc" to see if it spins forever
  const canvasModeBtn = p.locator('button:has-text("Bản gốc")');
  if (await canvasModeBtn.isVisible()) {
    await canvasModeBtn.click();
    await p.waitForTimeout(1000);
    const canvasSpinState = await p.evaluate(() => {
      const sp = document.querySelector('svg.animate-spin');
      return {
        hasInfiniteSpinner: !!sp,
        text: document.querySelector('main')?.innerText.slice(0, 300)
      };
    });
    console.log('=== CANVAS MODE ON 404 ===', canvasSpinState);
    await p.screenshot({ path: path.join(OUT_DIR, 'pdf-404-infinite-spin.png'), fullPage: false });
  }

  await browser.close();
})();
